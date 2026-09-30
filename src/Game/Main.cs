using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Game.Core;
using Godot;

public partial class Main : Node
{
    private readonly ConcurrentQueue<string> _commands = new();
    private readonly Dictionary<int, int> _bindings = [];
    private readonly Dictionary<int, string> _credentials = [];
    private readonly Dictionary<int, CommandLedger> _ledgers = [];
    private readonly Dictionary<int, (ulong Second, int Count)> _rates = [];
    private ENetMultiplayerPeer? _peer;
    private Match? _match;
    public MatchSnapshot? State { get; private set; }
    public int PlayerId { get; private set; }
    public bool Connected { get; private set; }
    public string Status { get; private set; } = "Connecting";
    public string Feedback { get; private set; } = "";
    private bool _server;
    private bool _automated;
    private bool _supervised;
    private bool _finished;
    private bool _resettingRpcNode;
    private bool _clientSignals;
    private int _version = WireJson.ProtocolVersion;
    private ulong _started;
    private int _connectionTimeout = 10000;
    private int _port = 7000;
    private string _host = "127.0.0.1";
    private SessionFile? _session;
    private readonly Dictionary<long, Command> _sent = [];
    private int _broadcastTick;
    private long _broadcastRevision = -1;

    public override void _Ready()
    {
        try { Setup(); }
        catch (Exception error) { Emit(new("error", Message: error.Message)); GD.PrintErr(error.Message); GetTree().Quit(1); }
    }
    private void Setup()
    {
        string[] args = OS.GetCmdlineUserArgs();
        _server = OS.HasFeature("dedicated_server");
        string bind = "127.0.0.1";
        string? sessionPath = null;
        bool roleSet = false;
        for (int i = 0; i < args.Length; i++)
        {
            string Value() => ++i < args.Length ? args[i] : throw new ArgumentException("Missing argument value.");
            switch (args[i])
            {
                case "--server":
                case "--client":
                    if (roleSet) throw new ArgumentException("Specify exactly one role: --server or --client.");
                    _server = args[i] == "--server"; roleSet = true; break;
                case "--host": _host = Value(); break;
                case "--bind": bind = Value(); break;
                case "--port": _port = int.Parse(Value()); break;
                case "--session-file": sessionPath = Value(); break;
                case "--protocol-version": _version = int.Parse(Value()); break;
                case "--connect-timeout-ms": _connectionTimeout = int.Parse(Value()); break;
                case "--automated": _automated = true; break;
                case "--supervised": _supervised = true; break;
                default: throw new ArgumentException($"Unknown game argument: {args[i]}");
            }
        }
        if (_port is < 1 or > 65535 || _connectionTimeout <= 0) throw new ArgumentException("Invalid port or timeout.");
        if (_server)
        {
            _match = new(); _peer = new(); _peer.SetBindIP(bind);
            Error result = _peer.CreateServer(_port, 16, 2);
            if (result != Error.Ok) throw new InvalidOperationException($"Cannot bind server to {bind}:{_port}: {result}");
            Multiplayer.MultiplayerPeer = _peer;
            Multiplayer.PeerDisconnected += id =>
            {
                int peer = (int)id;
                if (_bindings.Remove(peer, out int player)) { _match.SetConnected(player, false); Emit(new("left", peer, _match.Snapshot(), PlayerId: player)); }
                _rates.Remove(peer);
            };
            Emit(new("ready", State: _match.Snapshot(), Message: $"Server on {bind}:{_port}"));
        }
        else
        {
            string endpoint = $"{_host}:{_port}";
            string filename = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(endpoint)))[..16];
            _session = new(sessionPath ?? ProjectSettings.GlobalizePath($"user://sessions/{filename}.json"), endpoint);
            _session.Load();
            BindClientSignals();
            Connect();
            if (DisplayServer.GetName() != "headless") GetTree().Root.CallDeferred(Node.MethodName.AddChild, new Tabletop(this));
        }
        if (_automated || _supervised)
            _ = Task.Run(() => { string? line; while ((line = Console.ReadLine()) is not null) _commands.Enqueue(line); });
    }
    private void OnConnected() => RpcId(1, MethodName.Hello, _version, _session?.Value?.Token ?? "");
    private void OnConnectionFailed() => FailConnection("connection-failed", "Unable to connect to server.");
    private void OnServerDisconnected() => FailConnection("server-disconnected", "The server disconnected. Reconnect to the same running server to recover your city.");
    private void BindClientSignals()
    {
        Multiplayer.ConnectedToServer += OnConnected;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
        _clientSignals = true;
    }
    private void ResetRpcNode()
    {
        // Godot 4.7.2 export templates clear RPC path-cache signals without their bound node ID.
        // A real tree exit untracks this RPC node correctly before replacing the transport.
        // The client view is a sibling, so its controls and frozen scene remain intact.
        if (_clientSignals)
        {
            Multiplayer.ConnectedToServer -= OnConnected;
            Multiplayer.ConnectionFailed -= OnConnectionFailed;
            Multiplayer.ServerDisconnected -= OnServerDisconnected;
            _clientSignals = false;
        }
        Node parent = GetParent();
        _resettingRpcNode = true;
        parent.RemoveChild(this); parent.AddChild(this);
        GetTree().CurrentScene = this;
        _resettingRpcNode = false;
        BindClientSignals();
    }
    public void Connect(bool fresh = false)
    {
        if (_server || Connected) return;
        if (fresh) _session?.Clear();
        if (_peer is not null) ResetRpcNode();
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer(); _peer?.Close(); _peer?.Dispose();
        _peer = new();
        Error result = _peer.CreateClient(_host, _port, 2);
        if (result != Error.Ok) throw new InvalidOperationException($"Cannot connect: {result}");
        Multiplayer.MultiplayerPeer = _peer; _finished = false; Status = "Connecting"; _started = Time.GetTicksMsec();
        Emit(new("connecting", Message: $"{_host}:{_port}"));
    }
    private bool Allowed(int peer)
    {
        ulong second = Time.GetTicksMsec() / 1000;
        var rate = _rates.GetValueOrDefault(peer);
        rate = rate.Second == second ? (second, rate.Count + 1) : (second, 1);
        _rates[peer] = rate; return rate.Count <= 64;
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Hello(int version, string credential)
    {
        if (!_server || _match is null) return;
        int peer = Multiplayer.GetRemoteSenderId();
        if (peer <= 1 || !Allowed(peer) || _bindings.ContainsKey(peer)) return;
        string? refusal = null;
        int player = 0;
        if (version != WireJson.ProtocolVersion) refusal = "Protocol version mismatch.";
        else if (credential.Length > 128) refusal = "Invalid or expired session. Fresh join is available only in a lobby.";
        else if (credential.Length != 0)
        {
            player = _credentials.FirstOrDefault(kv => kv.Value == credential).Key;
            if (player == 0 || !_match.Players.ContainsKey(player)) refusal = "Invalid or expired session. Fresh join is available only in a lobby.";
            else if (_bindings.ContainsValue(player)) refusal = "Session already connected.";
        }
        else
        {
            City? city = _match.Join();
            if (city is null) refusal = "Roster locked or lobby full. Resume an existing session.";
            else { player = city.Id; _credentials[player] = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)); _ledgers[player] = new(); }
        }
        if (refusal is not null) { RpcId(peer, MethodName.Rejected, refusal); return; }
        _bindings.Add(peer, player); _match.SetConnected(player, true);
        RpcId(peer, MethodName.Welcome, player, _credentials[player], JsonSerializer.Serialize(_match.Snapshot(), WireJson.Options));
        Emit(new("joined", peer, _match.Snapshot(), PlayerId: player));
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Rejected(string reason) { if (!_server && Multiplayer.GetRemoteSenderId() == 1) FailConnection("connection-failed", reason); }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Welcome(int player, string token, string json)
    {
        if (_server || Multiplayer.GetRemoteSenderId() != 1) return;
        MatchSnapshot state = JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options)!;
        _session!.Welcome(state.MatchId, token); State = state; PlayerId = player; Connected = true; Status = "Connected";
        Emit(new("connected", Multiplayer.GetUniqueId(), state, PlayerId: player));
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Request(string json)
    {
        if (!_server || _match is null) return;
        int peer = Multiplayer.GetRemoteSenderId();
        if (!_bindings.TryGetValue(peer, out int player)) return;
        CommandResult result;
        if (!Allowed(peer)) result = new(0, false, "Command rate exceeded.");
        else if (json.Length > 2048) result = new(0, false, "Command too large.");
        else
        {
            try
            {
                Command? command = JsonSerializer.Deserialize<Command>(json, WireJson.Options);
                result = command is null ? new(0, false, "Malformed command.") : _ledgers[player].Execute(command, () => _match.Apply(player, command));
            }
            catch (JsonException) { result = new(0, false, "Malformed command."); }
        }
        RpcId(peer, MethodName.Acknowledged, JsonSerializer.Serialize(result, WireJson.Options), JsonSerializer.Serialize(_match.Snapshot(), WireJson.Options));
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Acknowledged(string json, string state)
    {
        if (_server || !Connected || Multiplayer.GetRemoteSenderId() != 1) return;
        CommandResult result = JsonSerializer.Deserialize<CommandResult>(json, WireJson.Options)!;
        Feedback = result.Message; AcceptState(state);
        Emit(new("ack", Multiplayer.GetUniqueId(), State, result.Message, PlayerId, result));
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 1)]
    private void Snapshot(string json)
    {
        if (_server || !Connected || Multiplayer.GetRemoteSenderId() != 1) return;
        if (AcceptState(json) && (_automated || _supervised)) Emit(new("snapshot", Multiplayer.GetUniqueId(), State, PlayerId: PlayerId));
    }
    private bool AcceptState(string json)
    {
        MatchSnapshot? state = JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options);
        if (state is null || (State is not null && (state.MatchId != State.MatchId || state.Revision < State.Revision))) return false;
        State = state; return true;
    }
    public long SendAction(string action, int slot = -1, Building building = Building.Empty, int city = 0)
    {
        if (!Connected || State is null) return 0;
        long sequence = _session!.Reserve();
        var request = new Command(sequence, State.MatchId, State.Phase, State.TurnSerial, action, city == 0 ? PlayerId : city, slot, building);
        _sent[sequence] = request;
        SendRequest(request); return sequence;
    }
    private void SendRequest(Command request) => RpcId(1, MethodName.Request, JsonSerializer.Serialize(request, WireJson.Options));
    public override void _PhysicsProcess(double delta)
    {
        for (int n = 0; n < 32 && _commands.TryDequeue(out string? command); n++) HandleCommand(command);
        if (_server && _match is not null)
        {
            _match.Step();
            if (++_broadcastTick >= 3 || _broadcastRevision != _match.Revision && _match.Phase != Phase.Combat)
            {
                _broadcastTick = 0; _broadcastRevision = _match.Revision;
                State = _match.Snapshot(); Rpc(MethodName.Snapshot, JsonSerializer.Serialize(State, WireJson.Options));
            }
        }
        else if (!Connected && Status == "Connecting" && Time.GetTicksMsec() - _started > (ulong)_connectionTimeout)
            FailConnection("connection-failed", "Connection deadline expired.");
    }
    private void HandleCommand(string text)
    {
        try
        {
            string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            switch (parts[0])
            {
                case "click" when !_server && DisplayServer.GetName() != "headless":
                    Vector2 position = new(float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                    Input.ParseInputEvent(new InputEventMouseMotion { Position = position, GlobalPosition = position });
                    Input.ParseInputEvent(new InputEventMouseButton { Position = position, GlobalPosition = position, ButtonIndex = MouseButton.Left, Pressed = true });
                    Input.ParseInputEvent(new InputEventMouseButton { Position = position, GlobalPosition = position, ButtonIndex = MouseButton.Left, Pressed = false });
                    break;
                case "disconnect" when !_server: FailConnection("server-disconnected", "Connection closed locally. Reconnect to resume your city."); break;
                case "quit": GetTree().Quit(); break;
                case "reconnect": Connect(); break;
                case "fresh": Connect(true); break;
                case "stale-ready" when Connected:
                    SendRequest(new(_session!.Reserve(), State!.MatchId, State.Phase, 1, "ready", PlayerId)); break;
                case "raw" when Connected: RpcId(1, MethodName.Request, text[4..]); break;
                case "retry" when Connected:
                    Command? saved = _sent.GetValueOrDefault(long.Parse(parts[1]));
                    if (saved is not null) SendRequest(saved); else Feedback = "Original request unavailable in this process; use raw to resend its identity.";
                    break;
                case "build": SendAction("build", int.Parse(parts[1]), Enum.Parse<Building>(parts[2], true), parts.Length > 3 ? int.Parse(parts[3]) : 0); break;
                case "upgrade": case "recruit": SendAction(parts[0], int.Parse(parts[1])); break;
                default: SendAction(parts[0]); break;
            }
        }
        catch (Exception e) when (e is ArgumentException or FormatException or IndexOutOfRangeException) { Emit(new("error", Message: "Invalid automation command.")); }
    }
    private void FailConnection(string type, string message)
    {
        if (_finished) return;
        _finished = true; Connected = false; Status = type == "connection-failed" ? "Connection failed" : "Disconnected"; Feedback = message;
        Emit(new(type, Message: message));
        ResetRpcNode(); Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer(); _peer?.Close();
        if (_automated) GetTree().Quit(type == "connection-failed" ? 1 : 0);
    }
    private static void Emit(GameEvent value) => GD.Print(WireJson.EventPrefix + JsonSerializer.Serialize(value, WireJson.Options));
    public override void _ExitTree() { if (!_resettingRpcNode) { _peer?.Close(); _peer?.Dispose(); } }
}
