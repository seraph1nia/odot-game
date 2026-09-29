using System.Collections.Concurrent;
using System.Text.Json;
using Game.Core;
using Godot;
using Point = Game.Core.Point;

public partial class Main : Node2D
{
    private const int ProtocolVersion = 1;
    private readonly ConcurrentQueue<string> _commands = new();
    private readonly Dictionary<int, ulong> _lastInput = new();
    private ENetMultiplayerPeer? _peer;
    private World? _world;
    private WorldSnapshot? _state;
    private bool _server;
    private bool _automated;
    private bool _supervised;
    private bool _connected;
    private bool _finished;
    private int _localId;
    private string _status = "Connecting";
    private ulong _started;
    private int _connectionTimeout = 10000;
    private int _botCoinTarget;
    private Point _botDirection;
    private readonly Font _font = ThemeDB.FallbackFont;
    private static readonly Vector2 RoomOffset = new(20, 150);

    public override void _Ready()
    {
        try
        {
            Setup();
        }
        catch (Exception error)
        {
            Emit(new("error", Message: error.Message));
            GD.PrintErr(error.Message);
            GetTree().Quit(1);
        }
    }

    private void Setup()
    {
        AddMovementAction("move_left", Key.A, Key.Left);
        AddMovementAction("move_right", Key.D, Key.Right);
        AddMovementAction("move_up", Key.W, Key.Up);
        AddMovementAction("move_down", Key.S, Key.Down);
        string[] args = OS.GetCmdlineUserArgs();
        _server = OS.HasFeature("dedicated_server");
        string host = "127.0.0.1";
        string bind = "127.0.0.1";
        int port = 7000;
        int seed = 42;
        bool roleSet = false;
        for (int i = 0; i < args.Length; i++)
        {
            string Value() => ++i < args.Length ? args[i] : throw new ArgumentException($"Missing value for {args[i - 1]}.");
            switch (args[i])
            {
                case "--server":
                case "--client":
                    if (roleSet) throw new ArgumentException("Specify exactly one role: --server or --client.");
                    _server = args[i] == "--server";
                    roleSet = true;
                    break;
                case "--host": host = Value(); break;
                case "--bind": bind = Value(); break;
                case "--port": port = int.Parse(Value()); break;
                case "--seed": seed = int.Parse(Value()); break;
                case "--connect-timeout-ms": _connectionTimeout = int.Parse(Value()); break;
                case "--automated": _automated = true; break;
                case "--supervised": _supervised = true; break;
                default: throw new ArgumentException($"Unknown game argument: {args[i]}");
            }
        }
        if (port is < 1 or > 65535 || _connectionTimeout <= 0) throw new ArgumentException("Port must be 1..65535 and timeout must be positive.");
        _peer = new ENetMultiplayerPeer();
        if (_server)
        {
            _world = new World(seed);
            _peer.SetBindIP(bind);
            Error result = _peer.CreateServer(port, 16, 2);
            if (result != Error.Ok) throw new InvalidOperationException($"Cannot bind server to {bind}:{port}: {result}");
            Multiplayer.MultiplayerPeer = _peer;
            Multiplayer.PeerDisconnected += id =>
            {
                _world.RemovePlayer((int)id);
                _lastInput.Remove((int)id);
                Emit(new("left", (int)id));
            };
            _status = $"Server on {bind}:{port}";
            Emit(new("ready", State: _world.Snapshot(), Message: _status));
        }
        else
        {
            Multiplayer.ConnectedToServer += () => RpcId(1, MethodName.Hello, ProtocolVersion);
            Multiplayer.ConnectionFailed += () => FailConnection("connection-failed", "Unable to connect to server.");
            Multiplayer.ServerDisconnected += () => FailConnection("server-disconnected", "The server disconnected.");
            Error result = _peer.CreateClient(host, port, 2);
            if (result != Error.Ok) throw new InvalidOperationException($"Cannot connect to {host}:{port}: {result}");
            Multiplayer.MultiplayerPeer = _peer;
            _started = Time.GetTicksMsec();
            Emit(new("connecting", Message: $"{host}:{port}"));
        }
        if (_automated || _supervised)
        {
            _ = Task.Run(() =>
            {
                string? line;
                while ((line = Console.ReadLine()) is not null) _commands.Enqueue(line);
            });
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Hello(int version)
    {
        if (!_server || _world is null) return;
        int id = Multiplayer.GetRemoteSenderId();
        if (id <= 1 || _world.Players.ContainsKey(id)) return;
        if (version != ProtocolVersion)
        {
            RpcId(id, MethodName.Rejected, "Protocol version mismatch.");
            return;
        }
        _world.AddPlayer(id);
        _lastInput[id] = Time.GetTicksMsec();
        RpcId(id, MethodName.InitialState, JsonSerializer.Serialize(_world.Snapshot(), WireJson.Options));
        Emit(new("joined", id, _world.Snapshot()));
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Rejected(string reason) => FailConnection("connection-failed", reason);

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void InitialState(string json)
    {
        if (_server || Multiplayer.GetRemoteSenderId() != 1) return;
        _state = JsonSerializer.Deserialize<WorldSnapshot>(json, WireJson.Options);
        _localId = Multiplayer.GetUniqueId();
        _connected = true;
        _status = "Connected";
        Emit(new("connected", Multiplayer.GetUniqueId(), _state));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void Move(float x, float y)
    {
        if (!_server || _world is null) return;
        int id = Multiplayer.GetRemoteSenderId();
        if (_world.SetInput(id, new(x, y))) _lastInput[id] = Time.GetTicksMsec();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered, TransferChannel = 1)]
    private void Snapshot(string json)
    {
        if (_server || !_connected || Multiplayer.GetRemoteSenderId() != 1) return;
        WorldSnapshot? state = JsonSerializer.Deserialize<WorldSnapshot>(json, WireJson.Options);
        if (state is null || state.Tick <= (_state?.Tick ?? -1)) return;
        _state = state;
        if (_automated) Emit(new("snapshot", Multiplayer.GetUniqueId(), state));
    }

    public override void _PhysicsProcess(double delta)
    {
        while (_commands.TryDequeue(out string? command)) HandleCommand(command);
        if (_server && _world is not null)
        {
            ulong now = Time.GetTicksMsec();
            foreach ((int id, ulong timestamp) in _lastInput)
                if (now - timestamp > 500) _world.SetInput(id, Point.Zero);
            _world.Step();
            if (_world.Tick % 3 == 0)
            {
                _state = _world.Snapshot();
                Rpc(MethodName.Snapshot, JsonSerializer.Serialize(_state, WireJson.Options));
            }
        }
        else if (_connected)
        {
            Point input = _automated ? BotInput() : KeyboardInput();
            RpcId(1, MethodName.Move, input.X, input.Y);
        }
        else if (_status == "Connecting" && Time.GetTicksMsec() - _started > (ulong)_connectionTimeout)
        {
            FailConnection("connection-failed", "Connection deadline expired.");
        }
        QueueRedraw();
    }

    private Point KeyboardInput()
    {
        if (!DisplayServer.WindowIsFocused()) return Point.Zero;
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        return new(direction.X, direction.Y);
    }

    private static void AddMovementAction(string name, params Key[] keys)
    {
        if (InputMap.HasAction(name)) return;
        InputMap.AddAction(name);
        foreach (Key key in keys) InputMap.ActionAddEvent(name, new InputEventKey { PhysicalKeycode = key });
    }

    private Point BotInput()
    {
        if (_state is null) return Point.Zero;
        PlayerState? me = _state.Players.FirstOrDefault(p => p.Id == _localId);
        if (me is null) return Point.Zero;
        if (_botCoinTarget > 0)
        {
            if (_state.CoinGeneration >= _botCoinTarget)
            {
                _botCoinTarget = 0;
                _botDirection = Point.Zero;
                Emit(new("bot-stopped", Multiplayer.GetUniqueId(), _state));
                return Point.Zero;
            }
            return (_state.Coin - me.Position).Limited();
        }
        return _botDirection;
    }

    private void HandleCommand(string command)
    {
        string[] parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;
        switch (parts[0])
        {
            case "collect": _botCoinTarget = (_state?.CoinGeneration ?? 0) + 1; break;
            case "stop": _botCoinTarget = 0; _botDirection = Point.Zero; break;
            case "move" when parts.Length == 3:
                _botDirection = new(float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                break;
            case "invalid": RpcId(1, MethodName.Move, float.NaN, float.PositiveInfinity); break;
            case "quit": GetTree().Quit(); break;
            default: Emit(new("error", Message: $"Invalid bot command: {command}")); break;
        }
    }

    private void FailConnection(string type, string message)
    {
        if (_finished) return;
        _finished = true;
        _connected = false;
        _status = type == "connection-failed" ? "Connection failed" : "Server disconnected";
        Emit(new(type, Message: message));
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
        if (_peer is not null) _peer.Close();
        if (_automated) GetTree().Quit(type == "connection-failed" ? 1 : 0);
    }

    private static void Emit(GameEvent value) => GD.Print(WireJson.EventPrefix + JsonSerializer.Serialize(value, WireJson.Options));

    public override void _Draw()
    {
        if (DisplayServer.GetName() == "headless") return;
        Color ink = new("e9efff");
        Color muted = new("7e91b5");
        Color accent = new("a9ef74");
        DrawString(_font, new(20, 36), "ODOT  /  COIN ROOM", fontSize: 24, modulate: ink);
        DrawString(_font, new(20, 64), "A small room. A shared world. One more coin.", fontSize: 15, modulate: muted);
        DrawString(_font, new(20, 99), _status.ToUpperInvariant(), fontSize: 13, modulate: _connected ? accent : muted);
        DrawString(_font, new(20, 128), "WASD / ARROWS     Move     |     Focus a window to control its player", fontSize: 14, modulate: ink);
        DrawRect(new(RoomOffset, new(World.Width, World.Height)), new("101b2e"));
        for (int x = 40; x < World.Width; x += 40)
            DrawLine(RoomOffset + new Vector2(x, 0), RoomOffset + new Vector2(x, World.Height), new("19263c"));
        for (int y = 40; y < World.Height; y += 40)
            DrawLine(RoomOffset + new Vector2(0, y), RoomOffset + new Vector2(World.Width, y), new("19263c"));
        DrawRect(new(RoomOffset, new(World.Width, World.Height)), new("304566"), false, 2);
        if (_state is not null)
        {
            DrawCircle(RoomOffset + new Vector2(_state.Coin.X, _state.Coin.Y), 17, new Color(0.66f, 0.94f, 0.45f, 0.14f));
            DrawCircle(RoomOffset + new Vector2(_state.Coin.X, _state.Coin.Y), World.CoinRadius, accent);
            int row = 0;
            foreach (PlayerState player in _state.Players)
            {
                bool local = player.Id == _localId;
                Color color = PlayerColor(row);
                Vector2 pos = RoomOffset + new Vector2(player.Position.X, player.Position.Y);
                if (local) DrawCircle(pos, 18, new Color(color.R, color.G, color.B, 0.22f));
                DrawCircle(pos, World.PlayerRadius, color);
                DrawString(_font, pos + new Vector2(-12, -22), local ? "YOU" : "P" + (row + 1), fontSize: 12, modulate: ink);
                DrawString(_font, new(500 + row * 155, 98), $"{(local ? "YOU" : "P" + (row + 1))}   {player.Score:00}", fontSize: 20, modulate: color);
                row++;
            }
        }
        DrawString(_font, new(20, 651), "FIND THE GREEN COIN     /     EVERY PICKUP COUNTS", fontSize: 12, modulate: muted);
    }

    private static Color PlayerColor(int id) => (id % 3) switch
    {
        0 => new("79baff"),
        1 => new("ffae8a"),
        _ => new("bb9dff")
    };

    public override void _ExitTree()
    {
        _peer?.Close();
        _peer?.Dispose();
    }
}
