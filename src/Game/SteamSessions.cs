using System.Globalization;
using System.Text.Json;
using Game.Core;
using Godot;

namespace Game;

// One application-thread owner of native discovery callbacks and lobby membership.
// Native peers deliver gameplay through Main's ordinary Godot RPC adapters.
internal sealed class SteamSessions : IDisposable
{
    private const int VirtualPort = 0;
    private const ulong OperationTimeoutMsec = 15000;
    private readonly Main _session;
    private readonly GameApplication _application;
    private readonly SteamPlatform _platform = new();
    private readonly List<(StringName Signal, Callable Handler)> _subscriptions = [];
    private readonly SteamOperationState _operations = new();
    private ulong _lobby, _originalHost, _nextOwnerCheck;
    private string _advertisedMatch = "", _publishedState = "";
    private bool _hosting, _transitioning, _disposed, _launchArgumentsRead;
    private MultiplayerPeer? _nativePeer;
    private readonly Dictionary<int, string> _reportedConnections = [];
    private ulong _nextConnectionCheck;
    private readonly SteamFriendInvitations _invitations;
    private ulong _nextIdentityCheck;
    private readonly bool _automaticInitialization;
    private bool _lastSteamRunning;
    private ulong _nextInitializationCheck;

    internal SteamSessions(Main session, GameApplication application)
    {
        _session = session; _application = application;
        _invitations = new(application, _platform, () => new(_lobby, session.SessionGeneration,
            !_disposed && _hosting && session.CanInvite, _platform.LoggedIn));
        application.HostRequested = Host;
        application.InviteRequested = Invite;
        session.SessionLeaving += Leave;
        session.StateChanged += OnStateChanged;
        string[] arguments = OS.GetCmdlineUserArgs();
        bool localRole = arguments.Any(value => value is "--solo" or "--playing-host" or "--server" or "--client");
        _automaticInitialization = !localRole && System.Environment.GetEnvironmentVariable("ODOT_STEAM_DISABLED") != "1";
        if (!localRole) EnsureInitialized();
        _lastSteamRunning = _automaticInitialization && SteamPlatform.ClientRunning;
    }

    private bool EnsureInitialized()
    {
        try
        {
            bool available = _platform.TryInitialize(out string feedback);
            // Initialization can succeed before the client reconnects. Subscribe now
            // so later warm invitations do not depend on a Host button retry.
            if (_platform.Initialized && _subscriptions.Count == 0)
            {
                Subscribe("join_requested", Callable.From<ulong, ulong>((lobby, _) =>
                {
                    Emit("steam-invitation", "Accepted lobby invitation received from Steam.");
                    OfferJoin(lobby);
                }));
                Subscribe("lobby_created", Callable.From<long, ulong>(OnCreated));
                Subscribe("lobby_joined", Callable.From<ulong, long, bool, long>(OnJoined));
                Subscribe("lobby_chat_update", Callable.From<ulong, ulong, ulong, long>(OnMemberChanged));
            }
            if (!available)
            { _application.SetSteamIdentity(null); Feedback(feedback); Emit("steam-availability", feedback); Emit("steam-unavailable", feedback); return false; }
            RefreshIdentity();
            Emit("steam-availability", "available");
            return true;
        }
        catch (Exception)
        {
            DisconnectSubscriptions(); Cleanup(_platform.Dispose);
            _application.SetSteamIdentity(null);
            const string message = "Open Steam and log in first. If it is already open, check application access and try again.";
            Feedback(message); Emit("steam-availability", message); Emit("steam-unavailable", message); return false;
        }
    }

    private void Subscribe(StringName signal, Callable callback)
    {
        if (_platform.Api.Connect(signal, callback) != Error.Ok)
            throw new InvalidOperationException("Steam callback subscription failed.");
        _subscriptions.Add((signal, callback));
    }

    internal void Process()
    {
        if (_disposed) return;
        try
        {
            if (_operations.ExpireConfirmation(_session.SessionGeneration)) _application.CancelJoinConfirmation();
            if (!_platform.Initialized && _automaticInitialization && Time.GetTicksMsec() >= _nextInitializationCheck)
            {
                _nextInitializationCheck = Time.GetTicksMsec() + 1000;
                bool running = SteamPlatform.ClientRunning;
                if (running && !_lastSteamRunning) EnsureInitialized();
                _lastSteamRunning = running;
            }
            if (!_platform.Initialized) return;
            _platform.Poll();
            if (Time.GetTicksMsec() >= _nextIdentityCheck)
            {
                _nextIdentityCheck = Time.GetTicksMsec() + 1000;
                RefreshIdentity();
            }
            _invitations.Process();
            if (!_launchArgumentsRead && _platform.LoggedIn)
            {
                _launchArgumentsRead = true;
                string[] arguments = OS.GetCmdlineUserArgs().Concat(_platform.LaunchCommandLine().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToArray();
                ulong[] targets = SteamLobbyRules.InvitationTargets(arguments);
                if (targets.Length != 0) OfferJoin(targets[0]);
            }
            if (_operations.CancelIfInvalid(_session.SessionGeneration, _application.Screen == "multiplayer",
                Time.GetTicksMsec(), OperationTimeoutMsec, out ulong abandonedLobby))
            {
                _platform.LeaveLobby(abandonedLobby);
                Feedback("Steam did not finish joining or hosting in time. Play Single player, or restart the game if the previous request stays pending.");
            }
            if (_nativePeer is not null && GodotObject.IsInstanceValid(_nativePeer) && _session.Connected
                && _session.State is { Players.Length: >= 2 } state && Time.GetTicksMsec() >= _nextConnectionCheck)
            {
                _nextConnectionCheck = Time.GetTicksMsec() + 1000;
                foreach (int peerId in _session.Multiplayer.GetPeers())
                {
                    object? evidence = _platform.ConnectionEvidence(_nativePeer, peerId, _hosting ? "host" : "guest", state.MatchId);
                    if (evidence is null) continue;
                    string message = JsonSerializer.Serialize(evidence, WireJson.Options);
                    if (_reportedConnections.GetValueOrDefault(peerId) == message) continue;
                    _reportedConnections[peerId] = message;
                    Emit("steam-connection", message);
                }
            }
            if (_lobby != 0 && !_hosting && Time.GetTicksMsec() >= _nextOwnerCheck)
            {
                _nextOwnerCheck = Time.GetTicksMsec() + 1000;
                if (_platform.LobbyOwner(_lobby) != _originalHost)
                    _session.ReturnToMenu("The original host ended this game. No host migration is supported.");
                else if (_session.Status == "Connection failed") Leave();
            }
        }
        catch (Exception)
        {
            Leave();
            _application.SetSteamIdentity(null);
            const string message = "Steam is unavailable. Try again from the menu, or play Single player.";
            Feedback(message);
            ReturnWhenCurrent(message);
        }
    }

    private void RefreshIdentity() => _application.SetSteamIdentity(_platform.LoggedIn ? _platform.PersonaName : null);

    internal void Host()
    {
        if (_disposed) return;
        if (_session.HasSession)
        { Feedback("The previous game is still closing. Try hosting again shortly."); return; }
        if (_operations.Kind != SteamOperationKind.None || !EnsureInitialized()) return;
        if (!_operations.BeginCreate(_session.SessionGeneration, Time.GetTicksMsec(), _platform.Initialized))
        { Feedback("Steam is still finishing the previous request. Try again shortly, or restart the game if it stays pending."); return; }
        _application.ShowMultiplayer();
        Feedback("Creating your private game…", true);
        try { _platform.CreatePrivateLobby(); }
        catch (Exception) { _operations.CreateFailed(); Feedback("Steam could not create a private game. Try again."); }
    }

    private void OnCreated(long result, ulong lobby)
    {
        bool accepted = _operations.CompleteCreate(_session.SessionGeneration, _application.Screen == "multiplayer");
        if (_disposed || lobby == _lobby) return;
        if (!accepted) { _platform.LeaveLobby(lobby); return; }
        if (result != 1 || lobby == 0) { Feedback("Steam could not create a private game. Check your connection and try again."); return; }
        if (!_platform.LoggedIn)
        {
            _platform.LeaveLobby(lobby);
            _application.SetSteamIdentity(null);
            Feedback("Open Steam and log in before hosting or joining a game.");
            return;
        }
        try
        {
            ulong host = _platform.SteamId;
            if (host == 0) throw new InvalidOperationException("Authenticated host unavailable.");
            MultiplayerPeer peer = _platform.CreateHost(VirtualPort);
            _nativePeer = peer;
            _transitioning = true;
            try { _session.StartNativeHost(peer, host.ToString(CultureInfo.InvariantCulture), id => SteamPlatform.IdentityForPeer(peer, id)); }
            finally { _transitioning = false; }
            _lobby = lobby; _originalHost = host; _hosting = true; _advertisedMatch = _session.State!.MatchId;
            _platform.AllowRosterReentry(lobby);
            Publish(_session.State);
            _session.CanInvite = true;
            Emit("steam-lobby", lobby.ToString(CultureInfo.InvariantCulture));
        }
        catch (Exception)
        {
            _platform.LeaveLobby(lobby);
            _session.ReturnToMenu("Steam could not prepare the hosted game. You can try again or play Single player.");
        }
    }

    internal void Join(ulong lobby) => OfferJoin(lobby);

    private void OfferJoin(ulong lobby)
    {
        if (_disposed || lobby == 0 || lobby == _lobby) return;
        if (!_operations.CanOffer(lobby, _lobby))
        {
            if (_operations.Kind == SteamOperationKind.None && _operations.Confirmation == 0
                && (_operations.CreateOutstanding || _operations.JoinOutstanding(lobby)))
                Feedback("Steam is still finishing the previous request. Try again shortly, or restart the game if it stays pending.");
            return;
        }
        if (!EnsureInitialized()) return;
        long generation = _session.SessionGeneration;
        if (!_operations.Offer(lobby, _lobby, generation, _platform.Initialized)) return;
        void Accept()
        {
            if (_disposed || !_operations.AcceptConfirmation(lobby, _session.SessionGeneration, generation)) return;
            if (!EnsureInitialized()) return;
            _session.ReturnToMenu();
            _application.ShowMultiplayer("Joining your friend's game…");
            if (!_operations.BeginJoin(lobby, _session.SessionGeneration, Time.GetTicksMsec(), _platform.Initialized)) return;
            Feedback("Joining your friend's game…", true);
            try { _platform.JoinLobby(lobby); }
            catch (Exception) { _operations.JoinFailed(lobby); Feedback("Steam could not join this invitation. Try again."); }
        }
        if (_session.HasSession) _application.OpenJoinConfirmation(Accept, () => _operations.DeclineConfirmation(lobby, generation));
        else Accept();
    }

    private void OnJoined(ulong lobby, long permissions, bool locked, long response)
    {
        SteamJoinCompletion completion = _operations.CompleteJoin(lobby, _lobby, _session.SessionGeneration, _application.Screen == "multiplayer");
        if (_disposed || completion == SteamJoinCompletion.Ignore) return;
        if (completion == SteamJoinCompletion.Leave) { _platform.LeaveLobby(lobby); return; }
        if (response != 1) { _platform.LeaveLobby(lobby); Feedback("This invitation could not be joined. Ask your friend for a new invitation."); return; }
        if (!_platform.LoggedIn)
        {
            _platform.LeaveLobby(lobby);
            _application.SetSteamIdentity(null);
            Feedback("Open Steam and log in before hosting or joining a game.");
            return;
        }
        try
        {
            var metadata = SteamLobbyRules.MetadataKeys.ToDictionary(key => key, key => _platform.LobbyData(lobby, key));
            if (!SteamLobbyRules.TryReadHost(metadata, SteamPlatform.AppId(), _platform.LobbyOwner(lobby), out ulong host, out string match, out string feedback))
            { _platform.LeaveLobby(lobby); Feedback(feedback); return; }
            if (host == _platform.SteamId) { _platform.LeaveLobby(lobby); Feedback("You cannot join your own game as another player."); return; }
            _lobby = lobby; _originalHost = host; _advertisedMatch = match; _hosting = false;
            _transitioning = true;
            try
            {
                _session.ConnectNativePeer(() => _nativePeer = _platform.CreateClient(host, VirtualPort),
                    $"steam:{host}:{lobby}", "odot:" + SteamPlatform.AppId().ToString(CultureInfo.InvariantCulture),
                    host.ToString(CultureInfo.InvariantCulture), expectedMatchId: match);
            }
            finally { _transitioning = false; }
        }
        catch (Exception) { _platform.LeaveLobby(lobby); _session.ReturnToMenu("Steam could not connect to the original host. Try another invitation."); }
    }

    private void OnStateChanged(MatchSnapshot state)
    {
        if (_disposed || _lobby == 0 || _transitioning) return;
        if (!_hosting)
        {
            if (state.MatchId != _advertisedMatch)
                ReturnWhenCurrent("The invitation belongs to a previous game. Ask your friend for a new invitation.");
            return;
        }
        try { Publish(state); }
        catch (Exception) { ReturnWhenCurrent("Steam could not maintain this private game. Please host again."); }
    }

    private void Publish(MatchSnapshot state)
    {
        string phase = state.Phase == Phase.Lobby ? "lobby" : "in-progress";
        if (_publishedState == phase) return;
        foreach (var (key, value) in SteamLobbyRules.Advertisement(SteamPlatform.AppId(), _originalHost, state.MatchId, state.Phase != Phase.Lobby))
            if (!_platform.SetLobbyData(_lobby, key, value)) throw new InvalidOperationException("Lobby metadata update failed.");
        _publishedState = phase;
    }

    private void OnMemberChanged(ulong lobby, ulong changed, ulong makingChange, long state)
    {
        if (_disposed || _hosting || lobby != _lobby || changed != _originalHost || (state & ~1L) == 0) return;
        _session.ReturnToMenu("The original host left this game. Ask your friend for a new invitation.");
    }

    private void Invite()
    {
        if (!_disposed && _hosting && _lobby != 0 && _session.CanInvite) _invitations.Open();
    }

    private void Feedback(string message, bool busy = false)
    {
        _session.SetFeedback(message);
        _application.SetPlatformFeedback(message, busy);
    }

    private void ReturnWhenCurrent(string message)
    {
        long generation = _session.SessionGeneration;
        Callable.From(() =>
        {
            if (!_disposed && generation == _session.SessionGeneration) _session.ReturnToMenu(message);
        }).CallDeferred();
    }

    private static void Emit(string type, string message) => GD.Print(WireJson.EventPrefix
        + JsonSerializer.Serialize(new GameEvent(type, Message: message), WireJson.Options));

    private void Leave()
    {
        if (_transitioning || _disposed) return;
        _invitations.Close();
        _session.CanInvite = false;
        ulong target = _operations.Cancel();
        if (_platform.Initialized)
        {
            if (_hosting && _lobby != 0) Cleanup(() => _platform.SetLobbyData(_lobby, "state", "ended"));
            Cleanup(() => _platform.LeaveLobby(target));
            Cleanup(() => _platform.LeaveLobby(_lobby));
        }
        _lobby = _originalHost = 0; _hosting = false; _nativePeer = null;
        _advertisedMatch = _publishedState = ""; _reportedConnections.Clear();
    }

    private static void Cleanup(Action action)
    {
        try { action(); }
        catch (Exception)
        { Emit("steam-unavailable", "Steam could not finish native cleanup. Local session cleanup continues."); }
    }

    private void DisconnectSubscriptions()
    {
        if (_platform.Initialized)
            foreach (var (signal, callback) in _subscriptions)
                Cleanup(() => { if (_platform.Api.IsConnected(signal, callback)) _platform.Api.Disconnect(signal, callback); });
        _subscriptions.Clear();
    }

    public void Dispose()
    {
        if (_disposed) return;
        try { Leave(); }
        finally
        {
            _disposed = true;
            _application.HostRequested = _application.InviteRequested = null;
            _session.SessionLeaving -= Leave; _session.StateChanged -= OnStateChanged;
            try { DisconnectSubscriptions(); }
            finally { Cleanup(_platform.Dispose); }
        }
    }
}
