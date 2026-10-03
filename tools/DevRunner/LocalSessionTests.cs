using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // A process may enter a fresh match and restart its local sequence numbering.
    private async Task<GameEvent> SessionAction(Child child, string command, CancellationToken token, bool accepted = true)
    {
        string match = Latest(child).MatchId;
        long previous = child.History().Where(e => e.Type == "ack" && e.State?.MatchId == match)
            .Select(e => e.Result!.Sequence).DefaultIfEmpty().Max();
        await child.Send(command);
        GameEvent result = await child.WaitFor(e => e.Type == "ack" && e.State?.MatchId == match && e.Result!.Sequence > previous,
            command, options.StartupTimeout, token);
        Require(result.Result!.Accepted == accepted, $"{child.Name}: {command}: {result.Message}");
        return result;
    }

    private async Task SoloSessionTest(CancellationToken token)
    {
        int port = _scope!.Port();
        using var occupied = new UdpClient(new IPEndPoint(IPAddress.Loopback, port));
        await using var solo = StartGameRole("solo", "solo", true, port, null, "--port", port.ToString(CultureInfo.InvariantCulture));
        GameEvent connected = await solo.WaitFor(e => e.Type == "connected", "bound solo player", options.StartupTimeout, token);
        MatchSnapshot started = State(await solo.WaitFor(e => e.Type == "ack" && e.Result!.Accepted && e.State?.Phase == Phase.Building,
            "ordinary solo start", options.StartupTimeout, token));
        CityState city = started.Players.Single();
        Require(connected.PeerId == 1 && city.Id == connected.PlayerId && city.Slots.Length == 9
            && city.Slots.All(s => s.Type == Building.Empty) && occupied.Client.IsBound, "socketless solo binds one fresh nine-slot city and preserves endpoint owner");
        await SessionAction(solo, "build 0 farm", token);
        await SessionAction(solo, "upgrade 0", token, false);
        await SessionAction(solo, "build 1 barracks", token);
        await SessionAction(solo, "build 2 metalmine", token);
        await SessionAction(solo, "build 9 mine", token, false);
        await SessionAction(solo, "build 2 mine", token, false);
        await SessionAction(solo, "ready", token);
        MatchSnapshot before = Latest(solo);
        GameEvent recruited = await SessionAction(solo, "recruit 1", token);
        var request = new Command(recruited.Result!.Sequence, before.MatchId, before.Phase, before.TurnSerial, "recruit", solo.PlayerId, 1, ExpectedGeneration: before.Players.Single(p => p.Id == solo.PlayerId).Slots[1].Generation);
        string replay = "raw " + JsonSerializer.Serialize(request, WireJson.Options);
        int retryCount = solo.History().Count(e => e.Type == "ack" && e.Result?.Sequence == request.Sequence);
        await solo.Send(replay);
        GameEvent repeated = await solo.WaitFor(e => e.Type == "ack" && e.Result!.Sequence == request.Sequence && e.Message == recruited.Message
            && solo.History().Count(ack => ack.Type == "ack" && ack.Result?.Sequence == request.Sequence) > retryCount,
            "solo accepted-command retry", options.StartupTimeout, token);
        Require(Gameplay(State(repeated)) == Gameplay(State(recruited)), "solo retry does not spend equipment or add another soldier");
        await SessionAction(solo, "pause", token);
        await SessionAction(solo, "ready", token, false);
        await SessionAction(solo, "resume", token);
        await solo.Send("menu");
        await solo.WaitFor(e => e.Type == "menu", "solo leave", options.StartupTimeout, token);
        await solo.Send("solo");
        MatchSnapshot fresh = State(await solo.WaitFor(e => e.Type == "ack" && e.State?.MatchId != started.MatchId
            && e.Result!.Accepted && e.State?.Phase == Phase.Building, "fresh solo start", options.StartupTimeout, token));
        Require(fresh.Players.Length == 1 && fresh.Players[0].Slots.All(s => s.Type == Building.Empty)
            && fresh.Players[0].Gold == city.Gold && fresh.Players[0].Food == 0 && fresh.Players[0].Soldiers.Length == 0
            && fresh.Players[0].Stone == 0 && fresh.Players[0].Metal == 0 && fresh.Players[0].Cloth == 0
            && fresh.Players[0].Slots.Count(s => s.Purchased) == 5 && fresh.Players[0].LastUpkeep is null && fresh.Players[0].LastReward is null,
            "second solo session discards unsaved gameplay and starts a new match");
        await solo.Send(replay);
        GameEvent stale = await solo.WaitFor(e => e.Type == "ack" && e.State?.MatchId == fresh.MatchId
            && e.Result!.Sequence == request.Sequence && !e.Result.Accepted, "old solo request refusal", options.StartupTimeout, token);
        Require(Gameplay(State(stale)) == Gameplay(fresh), "old-session command cannot change the fresh solo city");
        await solo.Send("build 0 farm");
        GameEvent next = await solo.WaitFor(e => e.Type == "ack" && e.State?.MatchId == fresh.MatchId
            && e.Result!.Sequence == 2 && e.Result.Accepted, "new solo sequence remains usable after stale packet", options.StartupTimeout, token);
        Require(State(next).Players[0].Gold == city.Gold - fresh.BuildingCatalog.Single(b => b.Type == Building.Farm).Construction.Gold, "stale-session rejection does not poison the new request ledger");
        await solo.Send("quit");
        Require(await solo.WaitExit(token) == 0 && occupied.Client.IsBound, "solo exit releases its session without touching endpoint owner");
    }

    private async Task PlayingHostLifecycleTest(int port, CancellationToken token)
    {
        await using var host = StartGameRole("playing-host", "playing-host", true, port);
        GameEvent hostJoined = await host.WaitFor(e => e.Type == "connected", "bound playing host", options.StartupTimeout, token);
        await host.WaitFor(e => e.Type == "ready", "playing host transport ready", options.StartupTimeout, token);
        string path = Path.Combine(_scope!.Directory, "hosted-guest.json");
        await using var guest = StartGameRole("hosted-guest", "client", true, port, null, "--session-file", path);
        GameEvent joined = await guest.WaitFor(e => e.Type == "connected", "hosted guest admitted", options.StartupTimeout, token);
        MatchSnapshot roster = await Observe(host, s => s.Players.Length == 2, "shared hosted roster", token);
        Require(hostJoined.PlayerId == 1 && joined.PlayerId != hostJoined.PlayerId && State(joined).MatchId == roster.MatchId,
            "playing host is a directly bound player in the same authority as its guest");
        await Action(guest, "start", token, false);
        await SessionAction(host, "start", token);
        await Observe(guest, s => s.Phase == Phase.Building, "guest sees host start", token);
        await SessionAction(host, $"build 0 farm {guest.PlayerId}", token, false);
        await Action(guest, $"build 0 farm {host.PlayerId}", token, false);
        await SessionAction(host, "build 9 farm", token, false);
        await Economy(host, token); await Economy(guest, token);
        await SessionAction(host, "build 3 mine", token, false);
        await Advance([host, guest], token);
        MatchSnapshot before = Latest(guest);
        GameEvent spent = await Action(guest, "recruit 1", token);
        var request = new Command(spent.Result!.Sequence, before.MatchId, before.Phase, before.TurnSerial, "recruit", guest.PlayerId, 1, ExpectedGeneration: before.Players.Single(p => p.Id == guest.PlayerId).Slots[1].Generation);
        string replay = "raw " + JsonSerializer.Serialize(request, WireJson.Options);
        MatchSnapshot frozen = State(await SessionAction(host, "pause", token));
        await Observe(guest, s => s.Paused && s.Revision >= frozen.Revision, "guest sees paused authority", token);
        await guest.Send("quit"); Require(await guest.WaitExit(token) == 0, "hosted guest leaves normally");
        await Observe(host, s => s.Paused && !s.Players.Single(p => p.Id == joined.PlayerId).Connected, "host retains disconnected paused city", token);
        await using var resumed = StartGameRole("hosted-resumed", "client", true, port, null, "--session-file", path);
        GameEvent restored = await resumed.WaitFor(e => e.Type == "connected", "hosted guest process resume", options.StartupTimeout, token);
        Require(restored.PlayerId == joined.PlayerId && restored.PeerId != joined.PeerId && Gameplay(State(restored)) == Gameplay(frozen),
            "hosted resume restores the same frozen city without a resource grant");
        await resumed.Send(replay);
        GameEvent retried = await resumed.WaitFor(e => e.Type == "ack" && e.Result!.Sequence == request.Sequence,
            "hosted request retry after restart", options.StartupTimeout, token);
        Require(retried.Result!.Accepted && Gameplay(State(retried)) == Gameplay(frozen), "hosted retry ledger survives guest process restart");
        await SessionAction(host, "resume", token);
        await Advance([host, resumed], token); await Advance([host, resumed], token);
        MatchSnapshot combat = await Observe(host, s => s.Phase == Phase.Combat && s.Tick > 3, "one playing-host authority advances combat", token);
        int malformed = resumed.History().Count(e => e.Type == "ack" && e.Result?.Sequence == 0);
        for (int n = 0; n < 8; n++) await resumed.Send("raw null");
        await resumed.WaitFor(e => e.Type == "ack" && e.Result?.Sequence == 0 && !e.Result.Accepted
            && resumed.History().Count(ack => ack.Type == "ack" && ack.Result?.Sequence == 0) >= malformed + 8,
            "channel zero acknowledgments progress during combat snapshot traffic", options.StartupTimeout, token);
        MatchSnapshot paused = State(await SessionAction(host, "pause", token));
        await Observe(resumed, s => s.Paused && s.Tick == paused.Tick && s.Revision >= paused.Revision, "shared paused combat revision", token);
        Require(combat.Tick <= paused.Tick && Gameplay(Latest(resumed)) == Gameplay(paused), "guest consumes identical paused authority state without numerical stepping");
        await Action(resumed, "ready", token, false);
        await host.Send("menu");
        await host.WaitFor(e => e.Type == "menu", "host leaves to menu", options.StartupTimeout, token);
        await resumed.WaitFor(e => e.Type == "session-ended", "orderly original host end reaches guest", options.StartupTimeout, token);
        await host.Send("host");
        MatchSnapshot fresh = State(await host.WaitFor(e => e.Type == "connected" && e.State is { } state && state.MatchId != roster.MatchId,
            "same-process fresh host", options.StartupTimeout, token));
        Require(fresh.Phase == Phase.Lobby && fresh.Players.Length == 1 && fresh.Players[0].Slots.All(s => s.Type == Building.Empty),
            "fresh hosted authority has a new match and only its local host");
        await using (var stale = ExpectedFailure(StartGameRole("hosted-stale", "client", true, port, null, "--automated", "--session-file", path)))
        {
            await stale.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("expired", StringComparison.Ordinal),
                "old hosted credentials refused", options.StartupTimeout, token);
            Require(await stale.WaitExit(token) != 0, "expired hosted credentials fail clearly");
        }
        await using var freshGuest = StartGameRole("hosted-fresh", "client", true, port, null, "--connect-timeout-ms", "1000");
        await freshGuest.WaitFor(e => e.Type == "connected", "fresh guest admitted after stale refusal", options.StartupTimeout, token);
        Require(Latest(freshGuest).Players.Length == 2, "stale claim consumes no roster slot");
        host.TerminateUnexpectedly();
        Require(await host.WaitExit(token) != 0, "only the owned playing host is terminated abruptly");
        await freshGuest.WaitFor(e => e.Type == "server-disconnected", "native liveness bounds unexpected host loss", options.StartupTimeout, token);
        int acknowledgedBeforeLoss = freshGuest.History().Count(e => e.Type == "ack");
        await freshGuest.Send("build 0 farm");
        await freshGuest.Send("menu");
        await freshGuest.WaitFor(e => e.Type == "menu", "host loss leaves an actionable return path", options.StartupTimeout, token);
        Require(freshGuest.History().Count(e => e.Type == "ack") == acknowledgedBeforeLoss, "guest gameplay remains disabled after unexpected authority loss");
    }
}
