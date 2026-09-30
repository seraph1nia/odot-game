using Game.Core;
using Godot;

namespace Game;

// Explicit diagnostic launch; no match, lobby, invite, player or presentation is created.
internal static class SteamProbe
{
    internal static void Run(Node owner, bool online)
    {
        if (!Engine.HasSingleton("Steam") || !ClassDB.ClassExists("SteamMultiplayerPeer"))
            throw new InvalidOperationException("The pinned native Steam singleton/peer did not load.");
        using var platform = new SteamPlatform();
        // Verify the extension's typed overlay callback even in offline CI.
        // Local emission opens no overlay and sends no invitation.
        GodotObject steam = Engine.GetSingleton("Steam");
        foreach (string method in new[] { "loggedOn", "getPersonaName", "isSteamRunning" })
            if (!steam.HasMethod(method)) throw new InvalidOperationException("The native Steam identity API is missing: " + method);
        bool overlaySignalObserved = false;
        var overlayCallback = Callable.From<bool, bool, long>((active, initiated, appId) =>
            overlaySignalObserved = active && !initiated && appId == 42);
        if (steam.Connect("overlay_toggled", overlayCallback) != Error.Ok)
            throw new InvalidOperationException("C# overlay signal subscription failed.");
        try
        {
            steam.EmitSignal("overlay_toggled", true, false, 42);
            if (!overlaySignalObserved) throw new InvalidOperationException("C# overlay signal arguments were not delivered.");
        }
        finally { steam.Disconnect("overlay_toggled", overlayCallback); }
        if (online)
        {
            if (!platform.TryInitialize(out string feedback))
                throw new InvalidOperationException("Steam probe prerequisite: " + feedback);
            if (platform.Api.Call("getAppID").AsInt64() != SteamPlatform.AppId())
                throw new InvalidOperationException("Steam initialized a different AppID.");
            if (!platform.LoggedIn || string.IsNullOrWhiteSpace(platform.PersonaName))
                throw new InvalidOperationException("Steam login/persona lookup failed.");
            GD.Print(WireJson.EventPrefix + System.Text.Json.JsonSerializer.Serialize(new GameEvent("steam-identity-probe",
                Message: "Native login and persona lookup passed; account name omitted from diagnostics."), WireJson.Options));
            GD.Print(WireJson.EventPrefix + System.Text.Json.JsonSerializer.Serialize(new GameEvent("steam-overlay-probe",
                Message: $"Overlay availability API returned {platform.OverlayEnabled}. No overlay was requested."), WireJson.Options));
            bool observed = false;
            var callback = Callable.From<long, ulong>((result, lobby) => observed = result == 1 && lobby == 0);
            if (platform.Api.Connect("lobby_created", callback) != Error.Ok)
                throw new InvalidOperationException("C# native signal subscription failed.");
            try
            {
                // Exercise local C# signal marshaling without creating an external lobby.
                platform.Api.EmitSignal("lobby_created", 1, 0);
                if (!observed) throw new InvalidOperationException("C# native signal arguments were not delivered.");
                platform.Poll();
                using var peer = platform.CreateHost(0);
                try
                {
                    if (peer is not MultiplayerPeerExtension || peer.GetUniqueId() != 1)
                        throw new InvalidOperationException("Native peer casting/host identity failed.");
                    peer.TransferChannel = 1;
                    if (peer.TransferChannel != 1) throw new InvalidOperationException("Native peer channel configuration failed.");
                    owner.Multiplayer.MultiplayerPeer = peer;
                    if (owner.Multiplayer.MultiplayerPeer != peer)
                        throw new InvalidOperationException("Native peer assignment failed.");
                }
                finally { owner.Multiplayer.MultiplayerPeer = null; peer.Close(); }
            }
            finally { platform.Api.Disconnect("lobby_created", callback); }
            platform.Dispose();
        }
        GD.Print(WireJson.EventPrefix + System.Text.Json.JsonSerializer.Serialize(new GameEvent("steam-probe", Message: online
            ? "Native load, AppID initialization, C# signal, peer host/channel/assignment, close/disposal and SDK shutdown passed."
            : "Native classes and typed overlay callback loaded; Steam initialization skipped; no match, socket or presentation created."), WireJson.Options));
        owner.GetTree().Quit();
    }
}
