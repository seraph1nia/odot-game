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
        foreach (string method in new[] { "loggedOn", "getPersonaName", "isSteamRunning", "getFriendCount", "getFriendByIndex", "getFriendPersonaName", "getFriendPersonaState", "getFriendRelationship", "inviteUserToLobby" })
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
        if (!online) VerifyUiPayload(owner);
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
            Game.Core.SteamFriend[] friends = platform.Friends();
            if (friends.Any(friend => friend.Id == 0 || !platform.IsFriend(friend.Id)))
                throw new InvalidOperationException("Native friend enumeration/relationship lookup failed.");
            GD.Print(WireJson.EventPrefix + System.Text.Json.JsonSerializer.Serialize(new GameEvent("steam-friends-probe",
                Message: "Native friend count, IDs, names, presence and relationships queried; identities omitted. No invitation sent."), WireJson.Options));
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

    private static void VerifyUiPayload(Node owner)
    {
        // The protocol smoke never creates UI. Consume the real source/PCK resources
        // here using the existing bounded offline probe. The separately selected
        // Worldtree package probe retains its minimal guarded EngineTests scene.
        if (DisplayServer.GetName() != "headless") throw new InvalidOperationException("Offline payload verification requires headless Godot.");
        using Theme theme = ApplicationTheme.Create();
        if (theme.DefaultFont.ResourcePath != "res://UI/fonts/DejaVuSerif.ttf"
            || theme.DefaultFont.GetStringSize("The Common Watch").X <= 0
            || UiAssets.Texture("art/menu_seal.png").GetSize() != new Vector2(192, 192)
            || ((StyleBoxTexture)theme.GetStylebox("panel", "PanelContainer")).Texture.ResourcePath != "res://UI/art/woodland/branch_frame.svg")
            throw new InvalidOperationException("Woodland theme/font/art did not load from the active resource pack.");
        foreach (string component in new[] { "army_roster", "city_navigation", "match_controls", "modal", "quoted_action", "resource_ledger", "session_feedback", "skill_tree", "unit_inspection", "upkeep_summary" })
        {
            Control loaded = UiAssets.Component(component, owner);
            try
            {
                loaded.Hide();
                if (!loaded.IsNodeReady()) throw new InvalidOperationException("UI payload scene did not initialize: " + component);
            }
            finally { loaded.Free(); }
        }
        foreach (string path in new[] { "res://UI/manifest.json", "res://UI/woodland-art.json", "res://UI/seal-provenance.json" })
        {
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read)
                ?? throw new InvalidOperationException("Bundled UI provenance is missing: " + path);
            using var json = System.Text.Json.JsonDocument.Parse(file.GetAsText());
            if (json.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
                throw new InvalidOperationException("Bundled UI provenance is invalid: " + path);
        }
        foreach (string path in new[] { "res://UI/README.md", "res://UI/fonts/LICENSE-DejaVu.txt" })
        {
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            if (file is null || file.GetLength() == 0) throw new InvalidOperationException("Bundled UI notice is missing: " + path);
        }
        if (ResourceLoader.Exists("res://Verification/ui_resources.gd"))
            throw new InvalidOperationException("Client contains the retired competing UI verification launcher.");
        Main.Emit(new GameEvent("ui-payload", Message: "Woodland theme, licensed serif font, original seal, branch art, all ten production scenes and bundled UI provenance/notices loaded; retired UI launcher absent. Headless resources only, not rendered pixels or gameplay binding."));
    }
}
