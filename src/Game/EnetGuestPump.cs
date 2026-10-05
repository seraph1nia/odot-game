using Godot;

namespace Game;

// Godot's documented API-extension passthrough, limited to one ENet guest.
// Only poll admission changes. The SAME SceneMultiplayer handles every RPC,
// replication config, peer, channel, signal, security check and sender identity.
internal sealed partial class EnetGuestPump : MultiplayerApiExtension
{
    private static readonly HashSet<ulong> Owners = [];
    internal SceneMultiplayer Inner { get; private set; } = null!;
    private SceneTree _tree = null!;
    private NodePath _root = null!;
    private GuestPollOwnership _lease = null!;
    private EnetGuestPumpDriver? _driver;
    private readonly List<(StringName Signal, Callable Callback)> _signals = [];
    private ulong _lastFrame = ulong.MaxValue;
    internal ulong LastServicedFrame => _lastFrame;
    private bool _released;
    internal long Services { get; private set; }
    internal long AutomaticAdmissionsSuppressed { get; private set; }
    internal OwnedTimingTrace? Trace { get; set; }

    internal static EnetGuestPump Attach(Node owner, SceneMultiplayer inner, NodePath root, Func<long> generation, Action<Exception> failure, OwnedTimingTrace? trace = null)
    {
        if (inner.MultiplayerPeer is not ENetMultiplayerPeer || inner.IsServer() || !Owners.Add(inner.GetInstanceId()))
            throw new InvalidOperationException("Only an exclusively owned ENet guest can acquire fixed-physics polling.");
        var pump = new EnetGuestPump
        {
            Inner = inner,
            _tree = owner.GetTree(),
            _root = root,
            _lease = new GuestPollOwnership(generation(), System.Environment.CurrentManagedThreadId),
            Trace = trace
        };
        try
        {
            if (pump._tree.GetMultiplayer(root) != inner) throw new InvalidOperationException("Guest API changed before polling acquisition.");
            foreach (StringName signal in new[] { SignalName.ConnectedToServer, SignalName.ConnectionFailed, SignalName.ServerDisconnected })
            {
                StringName captured = signal; Callable callback = Callable.From(() => pump.EmitSignal(captured));
                inner.Connect(signal, callback); pump._signals.Add((signal, callback));
            }
            foreach (StringName signal in new[] { SignalName.PeerConnected, SignalName.PeerDisconnected })
            {
                StringName captured = signal; Callable callback = Callable.From<long>(id => pump.EmitSignal(captured, id));
                inner.Connect(signal, callback); pump._signals.Add((signal, callback));
            }
            pump._tree.SetMultiplayer(pump, root); // Does NOT change global auto polling or other APIs.
            pump._driver = new EnetGuestPumpDriver
            {
                Pump = pump,
                Generation = generation,
                Failure = failure,
                ProcessMode = Node.ProcessModeEnum.Always,
                ProcessPhysicsPriority = -1000
            };
            owner.AddChild(pump._driver);
            trace?.Record("guest-poll-owner-acquired", info: new { Generation = generation(), Thread = System.Environment.CurrentManagedThreadId });
            return pump;
        }
        catch { pump.Release(); throw; }
    }
    internal bool Service(long generation, ulong frame)
    {
        _lease.Enter(generation, System.Environment.CurrentManagedThreadId);
        try
        {
            if (_tree.GetMultiplayer(_root) != this) throw new InvalidOperationException("Guest polling API ownership was replaced.");
            if (_lastFrame == frame) return false;
            _lastFrame = frame; Services++;
            Trace?.Record("guest-fixed-poll-enter", info: new { Generation = generation, PhysicsFrame = frame, Thread = System.Environment.CurrentManagedThreadId });
            Error result = Inner.Poll();
            Trace?.Record("guest-fixed-poll-exit", info: new { Generation = generation, PhysicsFrame = frame, Result = result });
            if (result != Error.Ok) throw new InvalidOperationException("Guest native polling failed: " + result);
            return true;
        }
        finally { _lease.Exit(); }
    }
    internal void Release()
    {
        if (_released) return;
        _released = true; _lease.Release();
        if (GodotObject.IsInstanceValid(_tree) && _tree.GetMultiplayer(_root) == this) _tree.SetMultiplayer(Inner, _root);
        foreach (var (signal, callback) in _signals) if (Inner.IsConnected(signal, callback)) Inner.Disconnect(signal, callback);
        _signals.Clear(); Owners.Remove(Inner.GetInstanceId());
        if (_driver is { } driver && GodotObject.IsInstanceValid(driver)) { driver.SetPhysicsProcess(false); driver.QueueFree(); }
        _driver = null;
        Trace?.Record("guest-poll-owner-released", info: new { Services, AutomaticAdmissionsSuppressed });
        // Release can occur inside Inner.Poll's disconnect/RPC callback. Native
        // object destruction waits until that owner-thread stack has unwound.
        Callable.From(Dispose).CallDeferred();
    }
    public override Error _Poll() { AutomaticAdmissionsSuppressed++; return Error.Ok; }
    public override MultiplayerPeer _GetMultiplayerPeer() => Inner.MultiplayerPeer;
    public override void _SetMultiplayerPeer(MultiplayerPeer peer) => Inner.MultiplayerPeer = peer;
    public override int _GetUniqueId() => Inner.GetUniqueId();
    public override int _GetRemoteSenderId() => Inner.GetRemoteSenderId();
    public override int[] _GetPeerIds() => Inner.GetPeers();
    public override Error _Rpc(int peer, GodotObject obj, StringName method, Godot.Collections.Array args) => Inner.Rpc(peer, obj, method, args);
    public override Error _ObjectConfigurationAdd(GodotObject obj, Variant config) => Inner.ObjectConfigurationAdd(obj, config);
    public override Error _ObjectConfigurationRemove(GodotObject obj, Variant config) => Inner.ObjectConfigurationRemove(obj, config);
}
