using System.Globalization;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Game.Core;
using Godot;

namespace Game;

// Owns one session and adapts ordinary Godot RPC delivery to the shared authority.
public partial class Main : Node, IGameSession
{
    private enum SessionRole { None, Solo, PlayingHost, Guest, Dedicated }
    private readonly ConcurrentQueue<(long Generation, string Text)> _commands = new();
    private readonly List<(StringName Signal, Callable Handler)> _signals = [];
    private readonly Dictionary<long, Command> _sent = [];
    private MultiplayerPeer? _peer;
    private MultiplayerPeer? _drainingPeer;
    private Action? _afterDrain;
    private AuthoritySession? _authority;
    private SessionRole _role;
    private SessionFile? _session;
    private Func<MultiplayerPeer>? _guestPeerFactory;
    private Func<int, string?>? _authenticatedIdentity;
    private string _originalHostIdentity = "";
    private string? _expectedNativeMatch;
    private string _attempt = "";
    private string? _sessionPath;
    private bool _automated;
    private bool _supervised;
    private bool _finished;
    private bool _resettingRpcNode;
    private bool _exiting;
    private bool _guestCanStart;
    private SteamSessions? _steam;
    private int _version = WireJson.ProtocolVersion;
    private ulong _started;
    private int _connectionTimeout = 10000;
    private int _port = 7000;
    private string _host = "127.0.0.1";
    private string _bind = "127.0.0.1";
    private long _localSequence = 1;
    private int _broadcastTick;
    private long _broadcastRevision = -1;

    public MatchSnapshot? State { get; private set; }
    public int PlayerId { get; private set; }
    public int HostPlayerId { get; private set; }
    public bool Connected { get; private set; }
    public string Status { get; private set; } = "Menu";
    public string Feedback { get; private set; } = "";
    public bool CanStart => Connected && State?.Phase == Phase.Lobby
        && (_authority?.CanStart(PlayerId) == true || _role == SessionRole.Guest && _guestCanStart);
    public bool CanInvite { get; set; }
    public bool HasSession => _role != SessionRole.None || _drainingPeer is not null;
    public long SessionGeneration { get; private set; }
    internal GameApplication? Application { get; private set; }
    public event Action? SessionLeaving;
    public event Action<MatchSnapshot>? StateChanged;

    public override void _Ready()
    {
        try
        {
            string[] args = OS.GetCmdlineUserArgs();
            if (args.Length == 1 && args[0] is "--steam-probe" or "--steam-probe-offline")
                SteamProbe.Run(this, args[0] == "--steam-probe");
            else if (DisplayServer.GetName() == "headless") Setup(args);
            else Callable.From(() => SetupGraphical(args)).CallDeferred();
        }
        catch (Exception error) { Emit(new("error", Message: error.Message)); GD.PrintErr(error.Message); GetTree().Quit(1); }
    }

    private void SetupGraphical(string[] args)
    {
        try { Setup(args); }
        catch (Exception error) { Emit(new("error", Message: error.Message)); GD.PrintErr(error.Message); GetTree().Quit(1); }
    }

    private void Setup(string[] args)
    {
        SessionRole role = OS.HasFeature("dedicated_server") ? SessionRole.Dedicated : SessionRole.None;
        bool roleSet = false;
        for (int i = 0; i < args.Length; i++)
        {
            string Value() => ++i < args.Length ? args[i] : throw new ArgumentException("Missing argument value.");
            switch (args[i])
            {
                case "--server":
                case "--client":
                case "--playing-host":
                case "--solo":
                    if (roleSet) throw new ArgumentException("Specify exactly one session role.");
                    role = args[i] switch { "--server" => SessionRole.Dedicated, "--client" => SessionRole.Guest, "--playing-host" => SessionRole.PlayingHost, _ => SessionRole.Solo };
                    roleSet = true; break;
                case "--host": _host = Value(); break;
                case "--bind": _bind = Value(); break;
                case "--port": _port = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--session-file": _sessionPath = Value(); break;
                case "--protocol-version": _version = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--connect-timeout-ms": _connectionTimeout = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--automated": _automated = true; break;
                case "--supervised": _supervised = true; break;
                // Steam's own launch arguments are consumed by the application platform owner.
                case "+connect_lobby": _ = Value(); break;
                default: throw new ArgumentException($"Unknown game argument: {args[i]}");
            }
        }
        if (_port is < 1 or > 65535 || _connectionTimeout <= 0) throw new ArgumentException("Invalid port or timeout.");
        if (System.Environment.GetEnvironmentVariable("ODOT_OWNED_DATA") is { } owned)
        {
            string data = Path.GetFullPath(ProjectSettings.GlobalizePath("user://"));
            if (!data.StartsWith(Path.GetFullPath(owned).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new InvalidOperationException("Verification user:// is outside its owned data directory.");
        }
        if (DisplayServer.GetName() != "headless" && role != SessionRole.Dedicated)
        {
            Application = new GameApplication(this) { Name = "Application" };
            GetTree().Root.AddChild(Application);
            _steam = new SteamSessions(this, Application);
        }
        switch (role)
        {
            case SessionRole.Solo: StartSolo(); break;
            case SessionRole.PlayingHost: StartPlayingHost(_bind, _port); break;
            case SessionRole.Dedicated: StartEnetAuthority(AuthorityPolicy.Dedicated, _bind, _port); break;
            case SessionRole.Guest: StartEnetGuest(); break;
            default: Application?.ShowMenu(); Emit(new("menu", Message: "Start screen")); break;
        }
        if (_automated || _supervised)
            _ = Task.Run(() =>
            {
                string? line;
                while ((line = Console.ReadLine()) is not null) _commands.Enqueue((SessionGeneration, line));
            });
    }

    public void StartSolo()
    {
        if (DeferSessionStart(StartSolo)) return;
        BeginSession(SessionRole.Solo);
        _authority = new(AuthorityPolicy.Solo);
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
        BindLocalPlayer();
        SendAction("start");
    }

    public void StartPlayingHost(string bind = "127.0.0.1", int port = 7000) => StartEnetAuthority(AuthorityPolicy.PlayingHost, bind, port);

    private void StartEnetAuthority(AuthorityPolicy policy, string bind, int port)
    {
        if (port is < 1 or > 65535) throw new ArgumentException("Invalid port.");
        if (DeferSessionStart(() => StartEnetAuthority(policy, bind, port))) return;
        BeginSession(policy == AuthorityPolicy.Dedicated ? SessionRole.Dedicated : SessionRole.PlayingHost);
        _bind = bind; _port = port;
        var peer = new ENetMultiplayerPeer(); _peer = peer; peer.SetBindIP(bind);
        Error result = peer.CreateServer(port, 16, 2);
        if (result != Error.Ok) throw new InvalidOperationException($"Cannot bind server to {bind}:{port}: {result}");
        _authority = new(policy);
        Multiplayer.MultiplayerPeer = peer;
        BindAuthoritySignals();
        SetState(_authority.Snapshot());
        Emit(new("ready", State: State, Message: $"{policy} on {bind}:{port}"));
        if (policy == AuthorityPolicy.PlayingHost) BindLocalPlayer();
    }

    // The native peer owns reliability and routing; the same RPC adapters admit guests.
    public void StartNativeHost(MultiplayerPeer peer, string originalHostIdentity, Func<int, string?> authenticatedIdentity)
    {
        ArgumentNullException.ThrowIfNull(peer);
        if (DeferSessionStart(() => StartNativeHost(peer, originalHostIdentity, authenticatedIdentity))) return;
        BeginSession(SessionRole.PlayingHost);
        _peer = peer; _authenticatedIdentity = authenticatedIdentity; _originalHostIdentity = originalHostIdentity;
        _authority = new(AuthorityPolicy.PlayingHost, originalHostIdentity: originalHostIdentity, requireTrustedIdentity: true);
        Multiplayer.MultiplayerPeer = peer;
        BindAuthoritySignals();
        SetState(_authority.Snapshot());
        Emit(new("ready", State: State, Message: "Hosted session ready."));
        BindLocalPlayer();
    }

    public void ConnectNativePeer(Func<MultiplayerPeer> peerFactory, string endpoint, string application, string originalHostIdentity,
        Func<int, string?>? authenticatedIdentity = null, string? expectedMatchId = null)
    {
        ArgumentNullException.ThrowIfNull(peerFactory);
        ArgumentException.ThrowIfNullOrEmpty(originalHostIdentity);
        if (DeferSessionStart(() => ConnectNativePeer(peerFactory, endpoint, application, originalHostIdentity, authenticatedIdentity, expectedMatchId))) return;
        BeginSession(SessionRole.Guest);
        _originalHostIdentity = originalHostIdentity; _expectedNativeMatch = expectedMatchId;
        _authenticatedIdentity = authenticatedIdentity ?? (id => _peer?.HasMethod("get_steam_id_for_peer_id") == true
            ? _peer.Call("get_steam_id_for_peer_id", id).AsUInt64().ToString(CultureInfo.InvariantCulture) : null);
        ConfigureGuestStorage(endpoint, "steam", application, originalHostIdentity);
        _guestPeerFactory = peerFactory;
        Application?.ShowSession();
        Connect();
    }

    private void StartEnetGuest()
    {
        if (DeferSessionStart(StartEnetGuest)) return;
        BeginSession(SessionRole.Guest);
        ConfigureGuestStorage($"{_host}:{_port}", "enet", "odot", "");
        _guestPeerFactory = () =>
        {
            var peer = new ENetMultiplayerPeer();
            Error result = peer.CreateClient(_host, _port, 2);
            if (result == Error.Ok) return peer;
            peer.Close(); peer.Dispose(); throw new InvalidOperationException($"Cannot connect: {result}");
        };
        Application?.ShowSession();
        Connect();
    }

    private void ConfigureGuestStorage(string endpoint, string transport, string application, string originalHostIdentity)
    {
        // Keep the existing ENet filename and accept its legacy file schema.
        string storage = transport == "enet" && application == "odot" ? endpoint : $"{transport}\n{application}\n{originalHostIdentity}\n{endpoint}";
        string filename = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(storage)))[..16];
        _session = new(_sessionPath ?? ProjectSettings.GlobalizePath($"user://sessions/{filename}.json"), endpoint, transport, application, originalHostIdentity);
        _session.Load();
    }

    private bool DeferSessionStart(Action start)
    {
        if (_exiting) return true;
        if (_drainingPeer is null && _role == SessionRole.PlayingHost && _peer is not null && Multiplayer.GetPeers().Length != 0) EndSession();
        if (_drainingPeer is null) return false;
        if (!_exiting) _afterDrain = start;
        return true;
    }

    private void BeginSession(SessionRole role)
    {
        EndSession();
        _role = role; _finished = false; _localSequence = 1;
        Emit(new("role", Message: role.ToString()));
    }

    private void BindLocalPlayer()
    {
        PlayerId = _authority!.LocalPlayerId; HostPlayerId = _role == SessionRole.PlayingHost ? PlayerId : 0; Connected = true; Status = "Connected";
        SetState(_authority.Snapshot());
        Application?.ShowSession();
        Emit(new("connected", 1, State, PlayerId: PlayerId));
    }

    private void BindSignal(StringName signal, Callable handler)
    {
        Multiplayer.Connect(signal, handler); _signals.Add((signal, handler));
    }
    private void UnbindSignals()
    {
        foreach (var (signal, handler) in _signals)
            if (Multiplayer.IsConnected(signal, handler)) Multiplayer.Disconnect(signal, handler);
        _signals.Clear();
    }
    private void BindAuthoritySignals()
    {
        long generation = SessionGeneration;
        BindSignal(MultiplayerApi.SignalName.PeerConnected, Callable.From<long>(id =>
        {
            if (generation == SessionGeneration) ConfigureEnetTimeout((int)id);
        }));
        BindSignal(MultiplayerApi.SignalName.PeerDisconnected, Callable.From<long>(id =>
        {
            if (generation != SessionGeneration || _authority is null) return;
            if (_authority.Disconnect((int)id) is { } player)
            {
                SetState(_authority.Snapshot());
                Emit(new("left", (int)id, State, PlayerId: player));
            }
        }));
    }
    private void BindClientSignals()
    {
        long generation = SessionGeneration;
        BindSignal(MultiplayerApi.SignalName.ConnectedToServer, Callable.From(() =>
        {
            if (generation != SessionGeneration || _role != SessionRole.Guest || _finished) return;
            if (_originalHostIdentity.Length != 0 && _authenticatedIdentity?.Invoke(1) != _originalHostIdentity)
            { FailConnection("connection-failed", "The original host identity could not be authenticated."); return; }
            ConfigureEnetTimeout(1);
            RpcId(1, MethodName.Hello, _version, _session?.Value?.Token ?? "", _session?.Value?.MatchId ?? _expectedNativeMatch ?? "", _attempt);
        }));
        BindSignal(MultiplayerApi.SignalName.ConnectionFailed, Callable.From(() =>
        {
            if (generation == SessionGeneration) FailConnection("connection-failed", "Unable to connect to server.");
        }));
        BindSignal(MultiplayerApi.SignalName.ServerDisconnected, Callable.From(() =>
        {
            if (generation == SessionGeneration) FailConnection("server-disconnected", "The host disconnected. Reconnect to the same running host to recover your city.");
        }));
    }
    private void ConfigureEnetTimeout(int peer)
    {
        // Use native reliable-packet liveness and the same ordinary connection bound.
        if (_peer is not ENetMultiplayerPeer enet) return;
        using ENetPacketPeer connection = enet.GetPeer(peer);
        connection.SetTimeout(32, Math.Max(1, _connectionTimeout / 2), _connectionTimeout);
    }

    private void ResetRpcNode()
    {
        // A real tree exit resets the broken exported Godot 4.7.2 RPC path cache.
        // The persistent application is a sibling and retains its settings and music.
        UnbindSignals();
        Node parent = GetParent();
        _resettingRpcNode = true;
        parent.RemoveChild(this); parent.AddChild(this);
        GetTree().CurrentScene = this;
        _resettingRpcNode = false;
    }

    public void Connect(bool fresh = false)
    {
        if (_role != SessionRole.Guest || Connected || _guestPeerFactory is null) return;
        if (fresh)
        {
            _session?.Clear(); State = null; PlayerId = 0; _sent.Clear(); Application?.ShowSession();
        }
        SessionGeneration++;
        if (_peer is not null) ResetRpcNode(); else UnbindSignals();
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer(); _peer?.Close(); _peer?.Dispose(); _peer = null;
        _attempt = Guid.NewGuid().ToString("N");
        BindClientSignals();
        _peer = _guestPeerFactory();
        Multiplayer.MultiplayerPeer = _peer; _finished = false; Status = "Connecting"; Feedback = ""; _started = Time.GetTicksMsec();
        Emit(new("connecting", Message: _session?.Transport == "steam" ? "Connecting to invited host." : $"{_host}:{_port}"));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Hello(int version, string credential, string expectedMatchId, string attempt)
    {
        if (_authority is null || _role == SessionRole.Solo) return;
        int peer = Multiplayer.GetRemoteSenderId();
        if (attempt.Length > 64 || expectedMatchId.Length > 64) return;
        AdmissionResult admission = _authority.Admit(peer, version, credential, Time.GetTicksMsec(), _authenticatedIdentity?.Invoke(peer), expectedMatchId.Length == 0 ? null : expectedMatchId);
        if (admission.Ignored) return;
        if (!admission.Accepted) { RpcId(peer, MethodName.Rejected, admission.Message, attempt); return; }
        RpcId(peer, MethodName.Welcome, admission.PlayerId, admission.Credential, JsonSerializer.Serialize(admission.State, WireJson.Options), _authority.CanStart(admission.PlayerId), _authority.Policy == AuthorityPolicy.PlayingHost ? _authority.LocalPlayerId : 0, attempt);
        SetState(admission.State);
        Emit(new("joined", peer, State, PlayerId: admission.PlayerId));
    }
    private bool FromAuthority() => Multiplayer.GetRemoteSenderId() == 1
        && (_originalHostIdentity.Length == 0 || _authenticatedIdentity?.Invoke(1) == _originalHostIdentity);

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Rejected(string reason, string attempt)
    {
        if (_role == SessionRole.Guest && !_finished && attempt == _attempt && FromAuthority())
            FailConnection("connection-failed", reason);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Welcome(int player, string token, string json, bool mayStart, int hostPlayerId, string attempt)
    {
        if (_role != SessionRole.Guest || _finished || attempt != _attempt || !FromAuthority()) return;
        MatchSnapshot state = JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options)!;
        if (_expectedNativeMatch is not null && state.MatchId != _expectedNativeMatch)
        { FailConnection("connection-failed", "The invited match has ended or changed. Request a new invitation."); return; }
        if (_session?.Value is { } previous && previous.MatchId != state.MatchId)
        { FailConnection("connection-failed", "The previous match has ended. Join a fresh lobby explicitly."); return; }
        _session!.Welcome(state.MatchId, token); PlayerId = player; HostPlayerId = hostPlayerId; Connected = true; _guestCanStart = mayStart; Status = "Connected";
        SetState(state);
        Emit(new("connected", Multiplayer.GetUniqueId(), state, PlayerId: player));
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Request(string json)
    {
        if (_authority is null || _role == SessionRole.Solo) return;
        int peer = Multiplayer.GetRemoteSenderId();
        CommandResult? result = _authority.Request(peer, json, Time.GetTicksMsec());
        if (result is null) return;
        MatchSnapshot state = _authority.Snapshot(); SetState(state);
        RpcId(peer, MethodName.Acknowledged, JsonSerializer.Serialize(result, WireJson.Options), JsonSerializer.Serialize(state, WireJson.Options));
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Acknowledged(string json, string state)
    {
        if (_role != SessionRole.Guest || !Connected || !FromAuthority()) return;
        MatchSnapshot? received = JsonSerializer.Deserialize<MatchSnapshot>(state, WireJson.Options);
        if (received is null || State is null || received.MatchId != State.MatchId) return;
        // Snapshots on channel 1 may overtake a channel 0 acknowledgment. Keep the
        // latest state while still delivering the result for this running match.
        if (received.Revision >= State.Revision) SetState(received);
        CommandResult result = JsonSerializer.Deserialize<CommandResult>(json, WireJson.Options)!;
        Feedback = result.Message;
        Emit(new("ack", Multiplayer.GetUniqueId(), State, result.Message, PlayerId, result));
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 1)]
    private void Snapshot(string json)
    {
        if (_role != SessionRole.Guest || !Connected || !FromAuthority()) return;
        if (AcceptState(json) && (_automated || _supervised)) Emit(new("snapshot", Multiplayer.GetUniqueId(), State, PlayerId: PlayerId));
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SessionEnded(string matchId, string message)
    {
        if (_role != SessionRole.Guest || !FromAuthority() || State?.MatchId != matchId) return;
        FailConnection("session-ended", message);
        if (Application is not null) ReturnToMenu(message);
    }
    private bool AcceptState(string json)
    {
        MatchSnapshot? state = JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options);
        if (state is null || (State is not null && (state.MatchId != State.MatchId || state.Revision < State.Revision))) return false;
        SetState(state); return true;
    }
    private void SetState(MatchSnapshot? state)
    {
        State = state;
        if (state is not null) StateChanged?.Invoke(state);
    }
    private long ReserveSequence() => _authority is not null ? _localSequence++ : _session!.Reserve();
    public long SendAction(string action, int slot = -1, Building building = Building.Empty, int city = 0)
    {
        if (!Connected || State is null) return 0;
        long sequence = ReserveSequence();
        var request = new Command(sequence, State.MatchId, State.Phase, State.TurnSerial, action, city == 0 ? PlayerId : city, slot, building);
        _sent[sequence] = request;
        SendRequest(request); return sequence;
    }
    private void SendRequest(Command request)
    {
        if (_authority is not null) AcknowledgeLocal(_authority.ExecuteLocal(request));
        else if (_role == SessionRole.Guest && Connected) RpcId(1, MethodName.Request, JsonSerializer.Serialize(request, WireJson.Options));
    }
    private void AcknowledgeLocal(CommandResult result)
    {
        Feedback = result.Message; SetState(_authority!.Snapshot());
        Emit(new("ack", 1, State, result.Message, PlayerId, result));
    }

    public override void _Process(double delta) => _steam?.Process();

    public override void _PhysicsProcess(double delta)
    {
        for (int n = 0; n < 32 && _commands.TryDequeue(out var command); n++)
            if (command.Generation == SessionGeneration || IsApplicationCommand(command.Text)) HandleCommand(command.Text);
        if (_authority is not null)
        {
            _authority.Step();
            if (++_broadcastTick >= 3 || _broadcastRevision != _authority.Revision && _authority.Phase != Phase.Combat)
            {
                MatchSnapshot state = _authority.Snapshot();
                _broadcastTick = 0; _broadcastRevision = state.Revision; SetState(state);
                if (_peer is not null && Multiplayer.GetPeers().Length != 0) Rpc(MethodName.Snapshot, JsonSerializer.Serialize(state, WireJson.Options));
                if (Connected && (_automated || _supervised)) Emit(new("snapshot", 1, State, PlayerId: PlayerId));
            }
        }
        else if (_role == SessionRole.Guest && !Connected && Status == "Connecting" && Time.GetTicksMsec() - _started > (ulong)_connectionTimeout)
            FailConnection("connection-failed", "Connection deadline expired.");
    }

    private static bool IsApplicationCommand(string text) => text is "quit" or "exit" or "ui" or "ui-probe"
        || text.StartsWith("ui ", StringComparison.Ordinal) || text.StartsWith("ui-probe ", StringComparison.Ordinal);

    private void HandleCommand(string text)
    {
        try
        {
            string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            switch (parts[0])
            {
                case "ui" or "ui-probe" when _supervised && Application is not null:
                    _ = ProbeUi(parts.Length > 1 ? parts[1] : Guid.NewGuid().ToString("N"), parts.Length > 2 ? System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parts[2])) : null); break;
                case "ui-confirm-join": ProbeJoinConfirmation(parts[1]); break;
                case "key" when _supervised && Application is not null:
                    Key key = Enum.Parse<Key>(parts[1], true);
                    Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = true });
                    Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = false }); break;
                case "click" when Application is not null:
                    Vector2 position = new(float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture));
                    Input.ParseInputEvent(new InputEventMouseMotion { Position = position, GlobalPosition = position });
                    Input.ParseInputEvent(new InputEventMouseButton { Position = position, GlobalPosition = position, ButtonIndex = MouseButton.Left, Pressed = true });
                    Input.ParseInputEvent(new InputEventMouseButton { Position = position, GlobalPosition = position, ButtonIndex = MouseButton.Left, Pressed = false }); break;
                case "steam-host" when _supervised && Application is not null: _steam?.Host(); break;
                case "steam-join" when _supervised && Application is not null: _steam?.Join(ulong.Parse(parts[1], CultureInfo.InvariantCulture)); break;
                case "solo": StartSolo(); break;
                case "host": StartPlayingHost(_bind, _port); break;
                case "menu": ReturnToMenu(); break;
                case "disconnect" when _role == SessionRole.Guest: FailConnection("server-disconnected", "Connection closed locally. Reconnect to resume your city."); break;
                case "quit": case "exit": RequestExit(); break;
                case "reconnect": Connect(); break;
                case "fresh": Connect(true); break;
                case "stale-ready" when Connected:
                    SendRequest(new(ReserveSequence(), State!.MatchId, State.Phase, 1, "ready", PlayerId)); break;
                case "raw" when Connected:
                    if (_authority is not null) AcknowledgeLocal(_authority.RequestLocal(text[4..])); else RpcId(1, MethodName.Request, text[4..]); break;
                case "retry" when Connected:
                    Command? saved = _sent.GetValueOrDefault(long.Parse(parts[1], CultureInfo.InvariantCulture));
                    if (saved is not null) SendRequest(saved); else Feedback = "Original request unavailable in this process; use raw to resend its identity.";
                    break;
                case "build": SendAction("build", int.Parse(parts[1], CultureInfo.InvariantCulture), Enum.Parse<Building>(parts[2], true), parts.Length > 3 ? int.Parse(parts[3], CultureInfo.InvariantCulture) : 0); break;
                case "upgrade": case "recruit": SendAction(parts[0], int.Parse(parts[1], CultureInfo.InvariantCulture)); break;
                default: SendAction(parts[0]); break;
            }
        }
        catch (Exception e) when (e is ArgumentException or FormatException or IndexOutOfRangeException) { Emit(new("error", Message: "Invalid automation command.")); }
    }

    // Exercise native consent controls offline on the runner's owned display.
    // Decisions still arrive through actual UI input; this creates no Steam operation.
    private void ProbeJoinConfirmation(string id)
    {
        string? runtime = System.Environment.GetEnvironmentVariable("ODOT_UI_RUNTIME");
        string? worker = System.Environment.GetEnvironmentVariable("ODOT_UI_WORKER");
        string? authority = System.Environment.GetEnvironmentVariable("XAUTHORITY");
        string? display = System.Environment.GetEnvironmentVariable("DISPLAY");
        if (!_supervised || Application is null || _role is not (SessionRole.Solo or SessionRole.PlayingHost)
            || System.Environment.GetEnvironmentVariable("ODOT_STEAM_DISABLED") != "1" || DisplayServer.GetName() != "X11"
            || string.IsNullOrEmpty(worker) || runtime is null || !System.IO.File.Exists(System.IO.Path.Combine(runtime, "worker-token"))
            || System.IO.File.ReadAllText(System.IO.Path.Combine(runtime, "worker-token")) != worker
            || authority is null || System.IO.Path.GetFullPath(authority) != System.IO.Path.Combine(System.IO.Path.GetFullPath(runtime), "xauthority")
            || !System.IO.File.Exists(authority) || display is null || !System.Text.RegularExpressions.Regex.IsMatch(display, "^:[0-9]+$")
            || System.Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null)
            throw new ArgumentException("Join confirmation probe requires an owned offline UI worker.");
        long generation = SessionGeneration;
        Application.OpenJoinConfirmation(() =>
        {
            if (generation != SessionGeneration) { Emit(new("ui-join-decision", Message: id + ":stale")); return; }
            ReturnToMenu();
            Emit(new("ui-join-decision", Message: id + ":accepted"));
        }, () => Emit(new("ui-join-decision", Message: id + ":declined")));
    }

    private void FailConnection(string type, string message)
    {
        if (_finished || _role != SessionRole.Guest) return;
        _finished = true; Connected = false; Status = type == "connection-failed" ? "Connection failed" : "Disconnected"; Feedback = message;
        Emit(new(type, Message: message));
        SessionGeneration++;
        ResetRpcNode(); Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer(); _peer?.Close();
        if (_automated) GetTree().Quit(type == "connection-failed" ? 1 : 0);
    }

    private void EndSession()
    {
        SessionGeneration++;
        _afterDrain = null;
        MultiplayerPeer? peer = _peer;
        bool drain = _role == SessionRole.PlayingHost && _authority is not null && peer is not null && Multiplayer.GetPeers().Length != 0;
        if (drain)
        {
            peer!.RefuseNewConnections = true;
            Rpc(MethodName.SessionEnded, _authority!.MatchId, "The host ended this session. Return to the menu to join another game.");
            if (peer is ENetMultiplayerPeer enet) enet.Host.Flush();
        }
        if (_role != SessionRole.None) SessionLeaving?.Invoke();
        _authority?.End(); _authority = null;
        UnbindSignals();
        _peer = null;
        if (drain)
        {
            // Close() discards queued reliable packets. Keep the old transport alive
            // until guests process SessionEnded and close, bounded by the same deadline.
            _drainingPeer = peer;
            _ = DrainHostedPeer(peer!);
        }
        else if (_drainingPeer is null)
        {
            if (peer is not null) ResetRpcNode();
            Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer(); peer?.Close(); peer?.Dispose();
        }
        _role = SessionRole.None; _guestPeerFactory = null; _authenticatedIdentity = null; _originalHostIdentity = ""; _expectedNativeMatch = null; _session = null;
        _sent.Clear(); _attempt = ""; _broadcastTick = 0; _broadcastRevision = -1;
        Connected = false; PlayerId = 0; HostPlayerId = 0; CanInvite = false; _guestCanStart = false; State = null; Status = "Menu"; Feedback = "";
    }

    private async Task DrainHostedPeer(MultiplayerPeer peer)
    {
        ulong deadline = Time.GetTicksMsec() + (ulong)_connectionTimeout;
        do
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        while (peer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Disconnected
            && Multiplayer.GetPeers().Length != 0 && Time.GetTicksMsec() < deadline);
        ResetRpcNode();
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer(); peer.Close(); peer.Dispose();
        _drainingPeer = null;
        Action? next = _afterDrain; _afterDrain = null;
        if (_exiting) FinishExit(); else next?.Invoke();
    }

    public void ReturnToMenu(string feedback = "")
    {
        EndSession(); Feedback = feedback; Application?.ShowMenu(feedback);
        Emit(new("menu", Message: feedback.Length == 0 ? "Start screen" : feedback));
    }
    public void SetFeedback(string feedback) => Feedback = feedback;
    public bool SetFeedback(long generation, string feedback)
    {
        if (generation != SessionGeneration) return false;
        Feedback = feedback; return true;
    }
    private static void Emit(GameEvent value) => GD.Print(WireJson.EventPrefix + JsonSerializer.Serialize(value, WireJson.Options));

    public void RequestExit()
    {
        if (_exiting) return;
        _exiting = true; EndSession();
        if (_drainingPeer is null) FinishExit();
    }
    private void FinishExit()
    {
        _steam?.Dispose(); _steam = null;
        if (Application is not null) _ = QuitGraphical(); else GetTree().Quit();
    }
    private async Task QuitGraphical()
    {
        Application!.StopAudio();
        // Observe actual mixer cycles before destroying its server, retaining the bound.
        ulong deadline = Time.GetTicksMsec() + 1000;
        double previous = AudioServer.GetTimeSinceLastMix();
        int cycles = 0;
        while (cycles < 2 && Time.GetTicksMsec() < deadline)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            double current = AudioServer.GetTimeSinceLastMix();
            if (current < previous) cycles++;
            previous = current;
        }
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (cycles < 2) Emit(new("error", Message: "Audio mixer did not drain within the shutdown deadline."));
        GetTree().Quit(cycles >= 2 ? 0 : 1);
    }
    private async Task ProbeUi(string id, string? screenshot)
    {
        try
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            if (Application is null || _exiting)
                throw new InvalidOperationException("The application is closing.");
            int colors = 0;
            if (screenshot is not null)
            {
                using ViewportTexture texture = GetViewport().GetTexture();
                using Image image = texture.GetImage();
                var samples = new HashSet<string>();
                for (int y = 0; y < image.GetHeight(); y += Math.Max(1, image.GetHeight() / 20))
                    for (int x = 0; x < image.GetWidth(); x += Math.Max(1, image.GetWidth() / 20)) samples.Add(image.GetPixel(x, y).ToHtml());
                colors = samples.Count;
                Error result = image.SavePng(screenshot);
                if (result != Error.Ok) throw new InvalidOperationException($"Frame capture failed: {result}");
            }
            GD.Print("ODOT_UI " + JsonSerializer.Serialize(Application.ObserveUi(id, screenshot, colors), WireJson.Options));
        }
        catch (Exception error) { GD.Print("ODOT_UI " + JsonSerializer.Serialize(new { Id = id, Error = error.Message }, WireJson.Options)); }
    }
    public override void _ExitTree()
    {
        if (!_resettingRpcNode)
        {
            UnbindSignals(); _authority?.End(); _peer?.Close(); _peer?.Dispose(); _drainingPeer?.Close(); _drainingPeer?.Dispose(); _steam?.Dispose(); _steam = null;
        }
    }
}
