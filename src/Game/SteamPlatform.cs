using System.Globalization;
using Godot;

namespace Game;

// Application-thread helper over the official extension, not a transport implementation.
internal sealed class SteamPlatform : IDisposable
{
    private GodotObject? _steam;
    internal bool Initialized { get; private set; }
    internal GodotObject Api => Initialized ? _steam! : throw new InvalidOperationException("Steam is not initialized.");

    internal static uint AppId()
    {
        using var config = new ConfigFile();
        bool configured = config.Load(System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(), "steam-app.cfg")) == Error.Ok;
        if (OS.HasFeature("production"))
        {
            if (!configured)
                throw new InvalidOperationException("Production Steam configuration is missing.");
            uint appId = ConfiguredAppId(config);
            if (appId == 480)
                throw new InvalidOperationException("Production requires this game's own Steam AppID, not 480.");
            return appId;
        }
        string? value = System.Environment.GetEnvironmentVariable("ODOT_STEAM_APP_ID");
        if (value is null) return configured ? ConfiguredAppId(config) : 480;
        if (!uint.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out uint developmentAppId) || developmentAppId == 0)
            throw new InvalidOperationException("ODOT_STEAM_APP_ID must be a positive Steam AppID.");
        return developmentAppId;
    }

    private static uint ConfiguredAppId(ConfigFile config)
    {
        Variant value = config.GetValue("steam", "app_id", 0);
        long appId = value.VariantType == Variant.Type.Int ? value.AsInt64() : 0;
        if (appId is <= 0 or > uint.MaxValue) throw new InvalidOperationException("Steam configuration requires a positive AppID.");
        return (uint)appId;
    }

    internal bool TryInitialize(out string feedback)
    {
        if (System.Environment.GetEnvironmentVariable("ODOT_STEAM_DISABLED") == "1")
        { feedback = "Open Steam and log in before hosting or joining a game. Steam is disabled for this launch; Single player and local games remain available."; return false; }
        if (Initialized) return RequireLogin(out feedback);
        if (!Engine.HasSingleton("Steam") || !ClassDB.ClassExists("SteamMultiplayerPeer"))
        { feedback = "Steam support is unavailable in this build."; return false; }
        _steam = Engine.GetSingleton("Steam");
        if (!_steam.Call("isSteamRunning").AsBool())
        { feedback = "Open Steam and log in before hosting or joining a game."; return false; }
        var result = _steam.Call("steamInitEx", AppId(), false).AsGodotDictionary();
        if (result["status"].AsInt64() != 0)
        { feedback = "Steam initialization failed. Check your login and application access."; return false; }
        Initialized = true;
        Api.Call("initRelayNetworkAccess");
        return RequireLogin(out feedback);
    }

    private bool RequireLogin(out string feedback)
    {
        feedback = LoggedIn ? "" : "Open Steam and log in before hosting or joining a game.";
        return feedback.Length == 0;
    }

    internal bool LoggedIn => Initialized && Api.Call("isSteamRunning").AsBool() && Api.Call("loggedOn").AsBool();
    internal string PersonaName => Api.Call("getPersonaName").AsString();
    internal static bool ClientRunning => System.Environment.GetEnvironmentVariable("ODOT_STEAM_DISABLED") != "1"
        && Engine.HasSingleton("Steam") && ClassDB.ClassExists("SteamMultiplayerPeer")
        && Engine.GetSingleton("Steam").Call("isSteamRunning").AsBool();

    internal void Poll() { if (Initialized) Api.Call("run_callbacks"); }

    internal MultiplayerPeer CreateHost(int virtualPort)
    {
        MultiplayerPeer peer = CreatePeer();
        return FinishPeer(peer, () => (Error)peer.Call("create_host", virtualPort).AsInt64());
    }

    internal MultiplayerPeer CreateClient(ulong originalHost, int virtualPort)
    {
        if (originalHost == 0) throw new ArgumentException("The original Steam host is missing.", nameof(originalHost));
        MultiplayerPeer peer = CreatePeer();
        return FinishPeer(peer, () => (Error)peer.Call("create_client", originalHost, virtualPort).AsInt64());
    }

    private MultiplayerPeer CreatePeer()
    {
        _ = Api;
        var peer = ClassDB.Instantiate("SteamMultiplayerPeer").AsGodotObject() as MultiplayerPeer
            ?? throw new InvalidOperationException("The native Steam peer cannot be used by Godot C#.");
        peer.TransferMode = MultiplayerPeer.TransferModeEnum.Reliable;
        return peer;
    }

    private static MultiplayerPeer FinishPeer(MultiplayerPeer peer, Func<Error> initialize)
    {
        try
        {
            Error result = initialize();
            if (result != Error.Ok) throw new InvalidOperationException($"Steam connection creation failed: {result}.");
            return peer;
        }
        catch
        {
            try { peer.Close(); }
            finally { peer.Dispose(); }
            throw;
        }
    }

    internal static string? IdentityForPeer(MultiplayerPeer? peer, int id)
    {
        if (peer is null || !GodotObject.IsInstanceValid(peer)) return null;
        ulong identity = peer.Call("get_steam_id_for_peer_id", id).AsUInt64();
        return identity == 0 ? null : identity.ToString(CultureInfo.InvariantCulture);
    }

    internal ulong SteamId => Api.Call("getSteamID").AsUInt64();
    internal void CreatePrivateLobby() => Api.Call("createLobby", 0, 4);
    internal void JoinLobby(ulong lobby) => Api.Call("joinLobby", lobby);
    internal void LeaveLobby(ulong lobby) { if (Initialized && lobby != 0) Api.Call("leaveLobby", lobby); }
    internal ulong LobbyOwner(ulong lobby) => Api.Call("getLobbyOwner", lobby).AsUInt64();
    internal string LobbyData(ulong lobby, string key) => Api.Call("getLobbyData", lobby, key).AsString();
    internal bool SetLobbyData(ulong lobby, string key, string value) => Api.Call("setLobbyData", lobby, key, value).AsBool();
    internal void AllowRosterReentry(ulong lobby) => Api.Call("setLobbyJoinable", lobby, true);
    internal bool OverlayEnabled => Api.Call("isOverlayEnabled").AsBool();
    internal void InviteFriends(ulong lobby) => Api.Call("activateGameOverlayInviteDialog", lobby);
    internal string LaunchCommandLine() => Api.Call("getLaunchCommandLine").AsString();

    internal object? ConnectionEvidence(MultiplayerPeer peer, int peerId, string role, string match)
    {
        using GodotObject? packet = peer.Call("get_peer", peerId).AsGodotObject();
        if (packet is null) return null;
        long handle = packet.Call("get_connection_handle").AsInt64();
        if (handle == 0) return null;
        var info = Api.Call("getConnectionInfo", handle).AsGodotDictionary();
        if (!info.ContainsKey("connection_state")) return null;
        // Valve's public SteamNetConnectionInfo_t: Relayed flag = 16; POP is 0 when N/A.
        long flags = info["info_flags"].AsInt64();
        return new
        {
            Transport = "native-steam",
            Role = role,
            MatchId = match,
            ConnectionState = info["connection_state"].AsInt64(),
            Authenticated = (flags & 1) == 0 && IdentityForPeer(peer, peerId) is not null,
            Relay = (flags & 16) != 0,
            RelayPop = info["pop_relay"].AsInt64()
        };
    }

    public void Dispose()
    {
        try { if (Initialized) _steam!.Call("steamShutdown"); }
        finally { Initialized = false; _steam = null; }
    }
}
