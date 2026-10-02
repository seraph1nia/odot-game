using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // Only fixture data on the owned offline display; no SDK, lobby or invite.
    private async Task FriendsUiScenario(string size, string? executable, CancellationToken token)
    {
        Child host = StartGameRole("ui-friends-" + size, "playing-host", false, _scope!.Port(), executable);
        await host.WaitFor(e => e.Type == "ready", "friends fixture host", options.StartupTimeout, token);
        UiObservation lobby = await WaitUi(host, ui => ui.Screen == "session" && ui.Connected && ui.Targets.ContainsKey("Start"), "fixture lobby controls", token);
        Require(!lobby.FriendsOpen && !lobby.Targets["Invite"].Enabled, "ordinary offline host cannot open Steam friends");
        UiTarget behind = UiProtocol.Target(lobby, "Start");
        await FriendFixture(host, "list", token);
        await Click(host, "Invite", token);
        UiObservation picker = await WaitUi(host, ui => ui.FriendsOpen && ui.FriendCount == 32 && ui.FocusedControl == "RefreshFriends", "friends picker list and focus", token);
        foreach (string name in new[] { "RefreshFriends", "CloseFriends", "InviteFriend101", "InviteFriend202" })
        {
            UiTarget target = UiProtocol.Target(picker, name);
            Require(target.X > 0 && target.X < picker.Width && target.Y > 0 && target.Y < picker.Height, name + " fits at " + size);
        }
        await MenuCheckpoint(host, "friends-list-" + size, token);
        await GeometryAtBothSizes(host, "friends-list", token);
        for (int step = 0; step < 32; step++) await host.Send("key Tab");
        UiObservation scrolled = await WaitUi(host, ui => ui.FocusedControl == "InviteFriend309", "keyboard scrolls to final friends row", token);
        UiTarget last = UiProtocol.Target(scrolled, "InviteFriend309");
        Require(last.Y > 0 && last.Y < scrolled.Height, "keyboard-focused last row stays inside viewport");
        await Click(host, "RefreshFriends", token);
        await WaitUi(host, ui => ui.FocusedControl == "RefreshFriends", "refresh resets scroll and focus", token);
        await ClickPoint(host, behind);
        await host.Send("key Tab");
        UiObservation blocked = await UiProtocol.Probe(host, options.StartupTimeout, token);
        Require(blocked.FriendsOpen && blocked.FocusedControl.Length != 0 && Latest(host).Phase == Phase.Lobby,
            "outside click and Tab cannot start gameplay through friends picker");
        await Click(host, "InviteFriend202", token);
        await host.WaitFor(e => e.Type == "ui-friend-invite" && e.Message == "42:202", "selected duplicate-name account only", options.StartupTimeout, token);
        await WaitUi(host, ui => ui.InviteStatus.StartsWith("Invitation sent", StringComparison.Ordinal), "sent feedback", token);
        Require(Latest(host).Players.Length == 1, "send request does not admit a player");
        await FriendFixture(host, "failure", token);
        await Click(host, "InviteFriend101", token);
        await WaitUi(host, ui => ui.InviteStatus.Contains("could not send", StringComparison.Ordinal), "retryable rejected invitation", token);
        await FriendFixture(host, "empty", token);
        await Click(host, "RefreshFriends", token);
        await WaitUi(host, ui => ui.FriendCount == 0 && ui.InviteStatus.Contains("No Steam friends", StringComparison.Ordinal), "empty friends explanation", token);
        await FriendFixture(host, "list", token);
        await Click(host, "RefreshFriends", token);
        await WaitUi(host, ui => ui.FriendCount == 32, "refresh recovers friends list", token);
        await FriendFixture(host, "unavailable", token);
        await WaitUi(host, ui => ui.InviteStatus.Contains("Open Steam", StringComparison.Ordinal) && !ui.Targets["InviteFriend101"].Enabled,
            "login loss disables stale sends", token);
        await Click(host, "RefreshFriends", token);
        await WaitUi(host, ui => ui.InviteStatus.Contains("Open Steam", StringComparison.Ordinal), "unavailable refresh is usable", token);
        await FriendFixture(host, "list", token);
        await Click(host, "RefreshFriends", token);
        await WaitUi(host, ui => ui.FriendCount == 32 && ui.Targets["InviteFriend101"].Enabled, "login recovery refresh", token);
        await host.Send("key Escape");
        await WaitUi(host, ui => !ui.FriendsOpen && ui.FocusedControl == "Invite", "Escape restores invitation button focus", token);
        await Click(host, "Invite", token);
        await WaitUi(host, ui => ui.FriendsOpen, "picker reopens", token);
        await Click(host, "CloseFriends", token);
        await WaitUi(host, ui => !ui.FriendsOpen && ui.FocusedControl == "Invite", "Close restores invitation focus", token);
        await Click(host, "Invite", token);
        await WaitUi(host, ui => ui.FriendsOpen, "picker before consent", token);
        string consent = Guid.NewGuid().ToString("N");
        await host.Send("ui-confirm-join " + consent);
        await WaitUi(host, ui => ui.JoinConfirmationOpen && !ui.FriendsOpen, "join consent replaces picker", token);
        await Click(host, "DeclineJoin", token);
        await ConsentDecision(host, consent, "declined", token);
        await Click(host, "Invite", token);
        await WaitUi(host, ui => ui.FriendsOpen, "picker before session change", token);
        int sent = host.History().Count(e => e.Type == "ui-friend-invite");
        await host.Send("menu");
        await WaitUi(host, ui => ui.Screen == "menu" && !ui.FriendsOpen, "session teardown closes picker", token);
        Require(host.History().Count(e => e.Type == "ui-friend-invite") == sent
            && !host.History().Any(e => e.Type is "steam-lobby" or "steam-connection"), "cleanup sends no invitation or native connection");
        await host.Send("host");
        await WaitUi(host, ui => ui.Screen == "session" && !ui.FriendsOpen && !ui.Targets["Invite"].Enabled, "new host has no stale fixture", token);
        await ReturnViaControl(host, token);
        await host.Send("host");
        await WaitUi(host, ui => ui.Screen == "session" && ui.Connected && !ui.FriendsOpen, "fresh host after confirmed menu return", token);
        await FriendFixture(host, "list", token);
        await Click(host, "Invite", token);
        UiObservation final = await WaitUi(host, ui => ui.FriendsOpen, "picker open for native close", token);
        NativeWindowClose.Request(host, options, final.NativeWindow);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(options.StartupTimeout);
        Require(await host.WaitExit(deadline.Token) == 0, "native window exit cleans up friends modal and host");
    }

    private async Task FriendFixture(Child host, string mode, CancellationToken token)
    {
        var prior = new HashSet<GameEvent>(host.History(), ReferenceEqualityComparer.Instance);
        await host.Send("ui-steam-friends " + mode);
        await host.WaitFor(e => !prior.Contains(e) && e.Type == "ui-friends-fixture" && e.Message == mode,
            "owned friends fixture " + mode, options.StartupTimeout, token);
    }
}
