using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // This slice catches application-lifetime regressions that direct clients bypass:
    // stale screen input, duplicated/restarted music, menu focus and a nonterminating Exit.
    // Two small solo routes reuse existing picking/protocol helpers without replaying battles.
    private async Task MenuUiScenario(string? executable, CancellationToken token)
    {
        foreach (string size in new[] { "1100x820", "1280x720" })
        {
            var worker = new Runner(options with { EngineArgs = ["--resolution", size] }, token, _evidence, _scope);
            await worker.MenuRoute(size, executable, token);
        }
        var hosted = new Runner(options with { EngineArgs = ["--resolution", "1280x720"] }, token, _evidence, _scope);
        await hosted.HostedMenuRoute(executable, token);
    }

    private async Task HostedMenuRoute(string? executable, CancellationToken token)
    {
        int port = _scope!.Port();
        Child host = StartGameRole("ui-local-host", "playing-host", false, port, executable);
        await host.WaitFor(e => e.Type == "ready", "graphical playing-host readiness", options.StartupTimeout, token);
        Child guest = StartGameRole("ui-local-guest", "client", false, port, executable);
        await guest.WaitFor(e => e.Type == "connected", "local guest admitted", options.StartupTimeout, token);
        UiObservation guestLobby = await WaitUi(guest, p => p.Screen == "session" && p.Connected && p.Models > 0,
            "graphical hosted guest ready", token);
        Require(!guestLobby.Targets["Start"].Visible && !guestLobby.Targets["Start"].Enabled, "guest UI cannot start hosted game");
        await Observe(host, s => s.Players.Length == 2, "hosted roster", token);
        UiObservation lobby = await WaitUi(host, p => p.Screen == "session" && p.RosterText.Contains("Host", StringComparison.Ordinal)
            && p.RosterText.Contains("P" + guest.PlayerId, StringComparison.Ordinal) && p.Targets["Start"].Visible && p.Targets["Start"].Enabled,
            "hosted roster and host Start available", token);
        Require(lobby.Targets["Invite"].Visible && !lobby.Targets["Invite"].Enabled, "local ENet host displays unavailable Steam Invite");
        foreach (string name in new[] { "Start", "Invite", "ReturnToMenu" })
        {
            UiTarget target = lobby.Targets[name];
            Require(target.X > 0 && target.X < lobby.Width && target.Y > 0 && target.Y < lobby.Height, "hosted " + name + " stays inside bottom panel at 1280x720");
        }
        GameEvent refused = await Action(guest, "start", token, false);
        Require(State(refused).Phase == Phase.Lobby, "guest cannot start hosted game");
        await ClickAck(host, "Start", token);
        await Observe(guest, s => s.Phase == Phase.Building, "host start reaches guest", token);
        await Pick(host, 0, token);
        await ClickAck(host, "Farm", token);
        await Action(guest, "build 0 farm", token);
        await Observe(host, s => s.Players.Length == 2 && s.Players.All(p => p.Slots[0].Type == Building.Farm), "host and guest purchases share authority", token);
        UiObservation purchases = await WaitUi(host, p => !p.Targets["Start"].Visible && p.SelectedSlot == 0 && p.FeedbackText.Length != 0,
            "hosted purchase feedback and direct selection", token);
        Require(purchases.MusicPlaying, "hosted application retains music");
        await MenuCheckpoint(host, "hosted-1280x720", token);
        await Click(guest, "Settings", token);
        UiObservation guestModal = await WaitUi(guest, p => p.SettingsOpen, "guest settings open before original host ends", token);
        await Click(host, "Settings", token);
        UiObservation modal = await WaitUi(host, p => p.SettingsOpen, "native close with settings modal open", token);
        NativeWindowClose.Request(host, options, modal.NativeWindow);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(options.StartupTimeout);
        try { Require(await host.WaitExit(deadline.Token) == 0, "native window close cleans up and actually terminates host"); }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        { throw new TimeoutException($"{host.Name}: native main-window close did not terminate within {options.StartupTimeout} ms. Evidence: {host.LogPath}"); }
        await guest.WaitFor(e => e.Type is "session-ended" or "server-disconnected", "native host close ends guest session", options.StartupTimeout, token);
        UiObservation ended = await WaitUi(guest, p => p.Screen == "menu" && !p.Connected && !p.SettingsOpen && p.FocusedControl == "Singleplayer",
            "original host close disposes guest session and modal into usable menu", token);
        Require(ended.MusicInstance == guestModal.MusicInstance && ended.MusicPlaying && ended.MasterVolume == guestModal.MasterVolume
            && ended.FeedbackText.Contains("host", StringComparison.OrdinalIgnoreCase), "guest keeps music/preferences and sees explanatory original-host feedback");
        await MenuCheckpoint(guest, "guest-ended-1280x720", token);
        using var released = new UdpClient(new IPEndPoint(IPAddress.Loopback, port));
        Require(released.Client.IsBound, "native close releases hosted gameplay endpoint");
        await Click(guest, "Exit", token);
        Require(await guest.WaitExit(deadline.Token) == 0, "guest cleans up after original host native close");
    }

    private async Task MenuRoute(string size, string? executable, CancellationToken token)
    {
        Child client = StartGameRole("ui-menu-" + size, "menu", false, 0, executable);
        await client.WaitFor(e => e.Type == "menu", "normal graphical start screen", options.StartupTimeout, token);
        UiObservation menu = await WaitUi(client, p => p.Screen == "menu" && p.MusicLoaded && p.MusicPlaying && p.FocusedControl == "Singleplayer", "menu/music/focus ready", token);
        foreach (string name in new[] { "Singleplayer", "Multiplayer", "Settings", "Exit" })
        {
            UiTarget target = UiProtocol.Target(menu, name);
            Require(target.X > 0 && target.X < menu.Width && target.Y > 0 && target.Y < menu.Height, name + " is accessible at " + size);
        }
        Require(!menu.Connected && menu.Revision == -1 && !client.History().Any(HasState), "start screen creates no match or connection");
        Require(menu.SteamStatus == "Open Steam to log in", "offline start screen explains Steam login");
        UiTarget identity = UiProtocol.Target(menu, "SteamStatus");
        Require(identity.Y > menu.Height * 0.8f && identity.Y < menu.Height, "Steam login status stays at bottom of start screen at " + size);
        Require(menu.MasterVolume == 50 && Math.Abs(menu.MasterGain - 0.5f) < 0.001f, "initial preferences apply before menu music");
        await MenuCheckpoint(client, "menu-" + size, token);
        await client.Send("key Down");
        await WaitUi(client, p => p.FocusedControl == "Multiplayer", "native menu keyboard navigation", token);
        await client.Send("key Enter");
        await WaitUi(client, p => p.Screen == "multiplayer", "keyboard activates Multiplayer", token);
        await Click(client, "Host", token);
        UiObservation unavailable = await WaitUi(client, p => p.Screen == "multiplayer" && p.FeedbackText.Length != 0 && p.Targets["Host"].Enabled, "recoverable platform feedback", token);
        Require(!unavailable.Connected && !client.History().Any(HasState), "unavailable platform creates no substitute lobby");
        Require(unavailable.SteamStatus == "Open Steam to log in" && unavailable.FeedbackText.Contains("Open Steam", StringComparison.Ordinal), "offline hosting asks player to open Steam while login footer remains available");
        await Click(client, "Back", token);
        await WaitUi(client, p => p.Screen == "menu", "Back restores start screen", token);

        await Click(client, "Settings", token);
        await WaitUi(client, p => p.SettingsOpen, "shared menu settings open", token);
        await Click(client, "AudioTab", token);
        await Click(client, "Volume", token);
        await UiProtocol.Probe(client, options.StartupTimeout, token);
        await client.Send("key Right");
        UiObservation changed = await WaitUi(client, p => p.MasterVolume != menu.MasterVolume, "actual volume input", token);
        Require(Math.Abs(changed.MasterGain - changed.MasterVolume / 100f) < 0.001f, "native audio slider changes Master gain");
        await Click(client, "CloseSettings", token);
        await WaitUi(client, p => !p.SettingsOpen && p.FocusedControl == "MenuSettings", "settings restores valid menu focus", token);
        await Click(client, "Singleplayer", token);
        GameEvent started = await client.WaitFor(e => e.Type == "ack" && e.Result?.Accepted == true && e.State?.Phase == Phase.Building, "solo validated start", options.StartupTimeout, token);
        MatchSnapshot solo = State(started);
        Require(solo.Players.Length == 1 && solo.Players[0].Slots.Length == 9 && solo.Players[0].Slots.All(s => s.Type == Building.Empty), "solo starts one fresh city with nine empty plots");
        UiObservation game = await WaitUi(client, p => p.Screen == "session" && p.Connected && p.Models > 0, "solo tabletop ready", token);
        Require(game.MasterVolume == changed.MasterVolume && game.MusicInstance == menu.MusicInstance && game.MusicPlaying && game.MusicPosition >= menu.MusicPosition, "settings and same music continue into solo");
        await Pick(client, 0, token);
        UiTarget behind = UiProtocol.Target(await UiProtocol.Probe(client, options.StartupTimeout, token), "Farm");
        await Click(client, "Settings", token);
        await WaitUi(client, p => p.SettingsOpen, "in-game shared modal", token);
        await ClickPoint(client, behind);
        UiObservation modal = await UiProtocol.Probe(client, options.StartupTimeout, token);
        MatchSnapshot afterModal = Latest(client);
        Require(modal.SettingsOpen && afterModal.Players[0].Slots[0].Type == Building.Empty && !afterModal.Paused && !afterModal.Players[0].Ready, "settings blocks plot actions without changing readiness or pause");
        await client.Send("key Tab");
        UiObservation tabbed = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(tabbed.SettingsOpen && Latest(client).Players[0].Slots[0].Type == Building.Empty, "outside click then native Tab stays inside settings");
        await client.Send("key Escape");
        await WaitUi(client, p => !p.SettingsOpen && p.FocusedControl.Length != 0, "in-game Escape closes settings and restores valid focus", token);
        await Pick(client, 0, token);
        GameEvent farm = await ClickAck(client, "Farm", token);
        Require(State(farm).Players[0].Gold == solo.Rules.StartingGold - solo.Rules.BuildCost, "solo pointer purchase uses authoritative costs");
        await ClickAck(client, "Upgrade", token);
        await Pick(client, 1, token);
        await ClickAck(client, "Barracks", token);
        int turn = Latest(client).TurnSerial;
        await ClickAck(client, "Ready", token);
        await Observe(client, s => s.TurnSerial > turn && s.Phase == Phase.Building, "solo ordinary turn progression", token);
        await Pick(client, 1, token);
        GameEvent recruit = await ClickAck(client, "Recruit", token);
        Require(State(recruit).Players[0].Soldiers.Length == 1, "solo explicit recruitment follows ordinary production");
        UiObservation recruitment = await UiProtocol.Probe(client, options.StartupTimeout, token);
        foreach (string name in new[] { "Recruit", "RecruitRanged" })
        {
            UiTarget control = UiProtocol.Target(recruitment, name);
            Require(control.X > 0 && control.X < recruitment.Width && control.Y > 0 && control.Y < recruitment.Height, name + " fits at " + size);
        }
        GameEvent ranged = await ClickAck(client, "RecruitRanged", token);
        Require(State(ranged).Players[0].Soldiers.Length == 2 && State(ranged).Players[0].Soldiers[1].Type == UnitType.Crossbowman, "ranged recruitment through actual solo controls at " + size);
        await ClickAck(client, "Pause", token);
        Require(Latest(client).Paused, "solo Pause freezes match");
        await MenuCheckpoint(client, "solo-" + size, token);
        await ClickAck(client, "Pause", token);
        Require(!Latest(client).Paused, "solo Resume continues match");
        if (size == "1100x820") await DeclineConsent(client, token);
        await ReturnViaControl(client, token);
        UiObservation returned = await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "Return to menu disposes solo", token);
        Require(returned.MasterVolume == changed.MasterVolume && returned.MusicInstance == menu.MusicInstance && returned.MusicPlaying && returned.MusicPosition >= game.MusicPosition, "preferences and uninterrupted music survive return");
        await Click(client, "Singleplayer", token);
        GameEvent second = await client.WaitFor(e => e.Type == "ack" && e.Result?.Accepted == true && e.State?.Phase == Phase.Building && e.State.MatchId != solo.MatchId, "second fresh solo session", options.StartupTimeout, token);
        MatchSnapshot fresh = State(second);
        Require(fresh.Players.Length == 1 && fresh.Players[0].Slots.All(s => s.Type == Building.Empty) && fresh.Players[0].Gold == fresh.Rules.StartingGold && fresh.Players[0].Soldiers.Length == 0, "second session contains no old state or purchases");
        await WaitUi(client, p => p.Screen == "session" && p.SelectedSlot == -1 && p.Connected, "fresh session selection cleared", token);
        if (size == "1100x820") await AcceptConsent(client, token); else await ReturnViaControl(client, token);
        await WaitUi(client, p => p.Screen == "menu", "fresh solo returns", token);
        await Click(client, "Exit", token);
        Require(await client.WaitExit(token) == 0, "displayed Exit actually terminates process at " + size);
    }

    // Controlled incoming application consent, with Steam disabled. Decisions use
    // real native controls; these assertions do not establish Steam invitation delivery.
    private async Task DeclineConsent(Child client, CancellationToken token)
    {
        MatchSnapshot before = Latest(client);
        UiObservation ui = await UiProtocol.Probe(client, options.StartupTimeout, token);
        UiTarget behind = UiProtocol.Target(ui, "Pause");
        await Click(client, "Settings", token);
        await WaitUi(client, p => p.SettingsOpen, "settings open before incoming consent", token);
        string id = Guid.NewGuid().ToString("N"), duplicate = Guid.NewGuid().ToString("N");
        await client.Send("ui-confirm-join " + id);
        await WaitUi(client, p => p.JoinConfirmationOpen && !p.SettingsOpen && p.Connected, "native active-session consent replaces settings", token);
        await client.Send("ui-confirm-join " + duplicate);
        await ConsentDecision(client, duplicate, "declined", token);
        await WaitUi(client, p => p.JoinConfirmationOpen, "duplicate consent preserves original dialog", token);
        await ClickPoint(client, behind);
        await Click(client, "DeclineJoin", token);
        await ConsentDecision(client, id, "declined", token);
        UiObservation after = await WaitUi(client, p => !p.JoinConfirmationOpen && p.Screen == "session" && p.Connected && p.FocusedControl.Length != 0,
            "decline keeps current session with valid focus", token);
        MatchSnapshot retained = Latest(client);
        Require(retained.MatchId == before.MatchId && Gameplay(retained) == Gameplay(before)
            && retained.Players[0].Ready == before.Players[0].Ready && after.SelectedSlot == ui.SelectedSlot,
            "declining consent and blocked outside input preserve current match and selection");
        Require(after.MusicInstance == ui.MusicInstance && after.MusicPlaying && after.MasterVolume == ui.MasterVolume,
            "declining consent preserves application music/preferences");
    }

    private async Task AcceptConsent(Child client, CancellationToken token)
    {
        UiObservation before = await UiProtocol.Probe(client, options.StartupTimeout, token);
        string id = Guid.NewGuid().ToString("N");
        await client.Send("ui-confirm-join " + id);
        await WaitUi(client, p => p.JoinConfirmationOpen && p.Screen == "session" && p.Connected, "fresh-session native consent ready", token);
        await MenuCheckpoint(client, "local-consent-1100x820", token);
        await Click(client, "AcceptJoin", token);
        await ConsentDecision(client, id, "accepted", token);
        UiObservation after = await WaitUi(client, p => p.Screen == "menu" && !p.Connected && !p.JoinConfirmationOpen && !p.SettingsOpen && p.FocusedControl == "Singleplayer",
            "accepted local consent disposes current match into usable menu", token);
        Require(after.MusicInstance == before.MusicInstance && after.MusicPlaying && after.MasterVolume == before.MasterVolume,
            "accepted consent retains application music/preferences");
        Require(!client.History().Any(e => e.Type is "steam-lobby" or "steam-connection"), "offline consent boundary creates no Steam lobby or transport");
    }

    private Task<GameEvent> ConsentDecision(Child client, string id, string decision, CancellationToken token)
        => client.WaitFor(e => e.Type == "ui-join-decision" && e.Message == id + ":" + decision,
            "local native consent " + decision, options.StartupTimeout, token);

    private async Task ReturnViaControl(Child client, CancellationToken token)
    {
        int previous = client.History().Count(e => e.Type == "menu");
        await Click(client, "ReturnToMenu", token);
        await client.WaitFor(e => e.Type == "menu" && client.History().Count(value => value.Type == "menu") > previous,
            "fresh session disposal/menu event", options.StartupTimeout, token);
    }

    private async Task MenuCheckpoint(Child client, string name, CancellationToken token)
    {
        string path = Path.Combine(_scope!.EvidenceDirectory, name + ".png");
        UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token, path);
        UiProtocol.Frame(frame, path);
        Require(frame.Display == "X11" && frame.Models > 0 && frame.MusicLoaded && frame.MusicPlaying, "menu/solo frame uses bundled scenery and persistent music");
        Require(frame.AudioDriver == "Dummy" && (frame.Renderer.Contains("llvmpipe", StringComparison.OrdinalIgnoreCase) || frame.Renderer.Contains("softpipe", StringComparison.OrdinalIgnoreCase)), "menu route uses owned software graphics and silent audio");
        Require(Path.GetFullPath(frame.UserDataPath).StartsWith(Path.Combine(_scope.Directory, client.Name, "data") + Path.DirectorySeparatorChar, StringComparison.Ordinal), "menu preferences belong to owned client storage");
        await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, name + "-observation.json"), JsonSerializer.Serialize(frame, Evidence.JsonOptions), token);
        Console.WriteLine($"FRAME {name}: {frame.Width}x{frame.Height}; {path}");
    }
}
