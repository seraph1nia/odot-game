using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private string? _testSessions;
    private static MatchSnapshot Latest(Child child) => State(child.History().Last(e => e.State is not null));
    private async Task<GameEvent> Action(Child child, string command, CancellationToken token, bool accepted = true)
    {
        long previous = child.History().Where(e => e.Type == "ack").Select(e => e.Result!.Sequence).DefaultIfEmpty().Max();
        await child.Send(command);
        GameEvent result = await child.WaitFor(e => e.Type == "ack" && e.Result!.Sequence > previous, command, options.StartupTimeout, token);
        Require(result.Result!.Accepted == accepted, $"{child.Name}: {command}: {result.Message}");
        return result;
    }
    private async Task<MatchSnapshot> Observe(Child child, Func<MatchSnapshot, bool> predicate, string expectation, CancellationToken token)
        => State(await child.WaitFor(e => e.State is not null && predicate(e.State), expectation, options.Timeout, token));
    private async Task RecruitAll(Child child, CancellationToken token)
    {
        MatchSnapshot state = Latest(child);
        int id = child.PlayerId;
        CityState city = state.Players.Single(p => p.Id == id);
        int slot = Array.FindIndex(city.Slots, s => s.Type == Building.Barracks);
        if (slot < 0) return;
        int cost = state.Rules.RecruitCost - (city.Slots[slot].Level - 1);
        while (city.Food >= cost)
        {
            GameEvent ack = await Action(child, $"recruit {slot}", token);
            city = State(ack).Players.Single(p => p.Id == id);
        }
    }
    private async Task Economy(Child child, CancellationToken token)
    {
        await Action(child, "build 0 farm", token); await Action(child, "upgrade 0", token); await Action(child, "build 1 barracks", token);
    }
    private async Task Advance(Child[] clients, CancellationToken token)
    {
        MatchSnapshot? resolved = null;
        foreach (Child child in clients) { await RecruitAll(child, token); resolved = State(await Action(child, "ready", token)); }
        MatchSnapshot target = resolved!;
        foreach (Child child in clients) await Observe(child, s => s.Revision >= target.Revision && s.TurnSerial >= target.TurnSerial && s.Phase == target.Phase, "resolved ready check", token);
    }
    private static string Gameplay(MatchSnapshot s) => JsonSerializer.Serialize(s with { Revision = 0, Players = s.Players.Select(p => p with { Connected = false, Ready = false }).ToArray() }, WireJson.Options);
    private async Task NetworkTests()
    {
        using var suite = CancellationTokenSource.CreateLinkedTokenSource(cancellation); suite.CancelAfter(options.Timeout);
        CancellationToken token = suite.Token;
        _testSessions = Path.Combine(Path.GetTempPath(), "odot-network-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(_testSessions);
        try
        {
            await AuthorityResumeVictory(options.Port ?? FreePort(), token);
            await Redistribution(FreePort(), token);
            await Defeat(FreePort(), token);
            await FailureCases(token);
            Console.WriteLine("Network verification passed.");
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested) { throw new TimeoutException($"Network suite exceeded {options.Timeout} ms; children were cleaned up."); }
        finally { Directory.Delete(_testSessions, true); _testSessions = null; }
    }
    private async Task AuthorityResumeVictory(int port, CancellationToken token)
    {
        await using var server = StartGame("test-server", true, true, port);
        await server.WaitFor(e => e.Type == "ready", "server readiness", options.StartupTimeout, token);
        await using var a = StartGame("test-a", false, true, port, null, "--automated");
        GameEvent ca = await a.WaitFor(e => e.Type == "connected", "A connected", options.StartupTimeout, token);
        string bPath = Path.Combine(_testSessions!, "resume-b.json");
        await using var b = StartGame("test-b", false, true, port, null, "--automated", "--session-file", bPath);
        GameEvent cb = await b.WaitFor(e => e.Type == "connected", "B connected", options.StartupTimeout, token);
        Require(ca.PlayerId != cb.PlayerId && ca.PeerId != cb.PeerId, "stable IDs and distinct ENet peers");
        await using (var concurrent = StartGame("concurrent", false, true, port, null, "--automated", "--session-file", bPath))
            await concurrent.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("already connected"), "concurrent claim refusal", options.StartupTimeout, token);
        await using (var protocol = StartGame("protocol", false, true, port, null, "--automated", "--protocol-version", "1"))
            await protocol.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("Protocol"), "old protocol refusal", options.StartupTimeout, token);
        await Action(a, "start", token); await Observe(b, s => s.Phase == Phase.Building, "B sees start", token);
        await Action(a, "ready", token);
        await Action(a, "build 0 farm", token, false);
        await Action(a, "unready", token);
        await Action(a, $"build 0 farm {cb.PlayerId}", token, false); await Action(a, "build 9 farm", token, false);
        await a.Send("raw {bad"); await a.WaitFor(e => e.Type == "ack" && e.Result!.Sequence == 0 && !e.Result.Accepted, "malformed rejection", options.StartupTimeout, token);
        string invalidPath = Path.Combine(_testSessions!, "invalid.json");
        File.WriteAllText(invalidPath, JsonSerializer.Serialize(new { Endpoint = $"{options.Host}:{port}", MatchId = State(ca).MatchId, Token = "INVALID", NextSequence = 1 }));
        await using (var invalid = StartGame("invalid-session", false, true, port, null, "--automated", "--session-file", invalidPath))
            await invalid.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("expired"), "invalid credential refusal", options.StartupTimeout, token);
        await Action(a, "unknown", token, false);
        await Economy(a, token); await Economy(b, token);
        await Action(a, "build 3 mine", token, false); await Action(a, "recruit 1", token, false);
        await using (var late = StartGame("late", false, true, port, null, "--automated"))
            await late.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("locked"), "late join refusal", options.StartupTimeout, token);
        await Advance([a, b], token);
        MatchSnapshot before = Latest(b);
        CommandResult spent = (await Action(b, "recruit 1", token)).Result!;
        var original = new Command(spent.Sequence, before.MatchId, before.Phase, before.TurnSerial, "recruit", cb.PlayerId, 1);
        string replay = "raw " + JsonSerializer.Serialize(original, WireJson.Options);
        int army = Latest(b).Players.Single(p => p.Id == cb.PlayerId).Soldiers.Length;
        await b.Send(replay); GameEvent dup = await b.WaitFor(e => e.Type == "ack" && e.Result!.Sequence == spent.Sequence && e.State!.Players.Single(p => p.Id == cb.PlayerId).Soldiers.Length == army, "duplicate recruitment", options.StartupTimeout, token);
        Require(State(dup).Players.Single(p => p.Id == cb.PlayerId).Food == before.Players.Single(p => p.Id == cb.PlayerId).Food - 5, "recruitment spends exactly once");
        await Advance([a, b], token); await Advance([a, b], token);
        await Action(a, "build 4 mine", token, false);
        await b.Send("quit"); Require(await b.WaitExit(token) == 0, "departing client exits");
        MatchSnapshot absent = await Observe(a, s => s.Phase == Phase.Combat && !s.Players.Single(p => p.Id == cb.PlayerId).Connected, "retained disconnected city", token);
        await Observe(a, s => s.Tick > absent.Tick + 9, "combat continues while absent", token);
        MatchSnapshot frozen = State(await Action(a, "pause", token));
        await Observe(a, s => s.Paused && s.Tick == frozen.Tick, "paused snapshot", token);
        await using var resumed = StartGame("test-resumed", false, true, port, null, "--automated", "--session-file", bPath);
        GameEvent cr = await resumed.WaitFor(e => e.Type == "connected", "restart resume", options.StartupTimeout, token);
        Require(cr.PlayerId == cb.PlayerId && cr.PeerId != cb.PeerId && Gameplay(State(cr)) == Gameplay(frozen), "new peer restores original city and frozen gameplay without regrant");
        await resumed.Send(replay); GameEvent retried = await resumed.WaitFor(e => e.Type == "ack" && e.Result!.Sequence == spent.Sequence, "retry after process restart", options.StartupTimeout, token);
        Require(Gameplay(State(retried)) == Gameplay(frozen), "old accepted spending remains exactly once after resume");
        await Action(resumed, "ready", token, false); MatchSnapshot paused = Latest(resumed);
        await Observe(a, s => s.Paused && s.Revision >= paused.Revision && s.Tick == frozen.Tick, "paused revisions remain visible", token);
        await Action(a, "resume", token);
        for (int wave = 1; wave <= 3; wave++)
        {
            MatchSnapshot cleared = await Observe(a, s => s.Phase is Phase.Building or Phase.Victory && (s.Wave > wave || s.Phase == Phase.Victory), "wave clear", token);
            await Observe(resumed, s => s.Revision >= cleared.Revision && s.Phase == cleared.Phase, "shared wave result", token);
            if (wave < 3)
            {
                // A stale ready command from wave one cannot match the repeated turn display.
                await Action(a, "stale-ready", token, false);
                await Advance([a, resumed], token); await Advance([a, resumed], token); await Advance([a, resumed], token);
            }
        }
        Require(Latest(a).Phase == Phase.Victory && Latest(resumed).Phase == Phase.Victory, "standard strategy wins exactly three waves");
        await Action(a, "ready", token, false);
        for (int n = 0; n < 80; n++) await a.Send("raw null");
        await a.WaitFor(e => e.Type == "ack" && e.Message == "Command rate exceeded.", "excessive request rejection", options.StartupTimeout, token);
        await Action(resumed, "ready", token, false);
        await server.Send("quit"); await a.WaitFor(e => e.Type == "server-disconnected", "server stopped feedback", options.StartupTimeout, token);
        // A credential from a stopped match must not silently create a new city.
        await using var replacement = StartGame("replacement-server", true, true, port);
        await replacement.WaitFor(e => e.Type == "ready", "replacement server", options.StartupTimeout, token);
        await using var staleSession = StartGame("stale-session", false, true, port, null, "--automated", "--session-file", bPath);
        await staleSession.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("expired"), "expired session refusal", options.StartupTimeout, token);
    }
    private async Task Redistribution(int port, CancellationToken token)
    {
        await using var server = StartGame("transfer-server", true, true, port); await server.WaitFor(e => e.Type == "ready", "transfer server", options.StartupTimeout, token);
        await using var a = StartGame("transfer-a", false, true, port, null, "--automated");
        await using var b = StartGame("transfer-b", false, true, port, null, "--automated");
        string cPath = Path.Combine(_testSessions!, "observer.json");
        await using var c = StartGame("transfer-c", false, true, port, null, "--automated", "--session-file", cPath);
        foreach (Child p in new[] { a, b, c }) await p.WaitFor(e => e.Type == "connected", "three-player lobby", options.StartupTimeout, token);
        int deadId = c.PlayerId;
        await Action(a, "start", token); foreach (Child p in new[] { b, c }) await Observe(p, s => s.Phase == Phase.Building, "transfer start", token);
        await Economy(a, token); await Economy(b, token); await Advance([a, b, c], token); await Advance([a, b, c], token); await Advance([a, b, c], token);
        MatchSnapshot initial = Latest(a); UnitState[] attackers = initial.Enemies.Where(e => e.Origin == deadId).ToArray();
        MatchSnapshot fallen = await Observe(a, s => s.Phase == Phase.Combat && s.Players.Single(p => p.Id == deadId).Eliminated, "ordinary under-defended city falls", token);
        UnitState[] transferred = fallen.Enemies.Where(e => e.Origin == deadId).ToArray();
        Require(transferred.Length > 0 && transferred.All(e => e.Destination != deadId) && transferred.Select(e => e.Id).Distinct().Count() == transferred.Length, "live enemies transferred immediately without duplication");
        Require(transferred.All(e => attackers.Any(old => old.Id == e.Id && old.Health >= e.Health)), "transfers preserve identity and damage");
        MatchSnapshot seen = await Observe(b, s => s.Revision == fallen.Revision, "matching transfer revision", token);
        Require(JsonSerializer.Serialize(fallen, WireJson.Options) == JsonSerializer.Serialize(seen, WireJson.Options), "surviving clients agree on transfers");
        await Observe(a, s => s.Wave == 2 && s.Phase == Phase.Building, "redistributed wave clear", token); await Observe(b, s => s.Wave == 2 && s.Phase == Phase.Building, "B next wave", token);
        await c.Send("quit"); await c.WaitExit(token);
        await using var observer = StartGame("observer-resumed", false, true, port, null, "--automated", "--session-file", cPath);
        GameEvent restored = await observer.WaitFor(e => e.Type == "connected", "eliminated observer resume", options.StartupTimeout, token);
        Require(restored.PlayerId == deadId && State(restored).Players.Single(p => p.Id == deadId).Eliminated, "resume cannot revive eliminated city");
        await Action(observer, "build 0 mine", token, false); await Action(observer, "pause", token); await Action(observer, "resume", token);
        await Advance([a, b], token); await Advance([a, b], token); await Advance([a, b], token);
        MatchSnapshot next = await Observe(a, s => s.Phase == Phase.Combat && s.Wave == 2, "future original roster allocation", token);
        Require(next.Enemies.Length == 18 && next.Players.Where(p => !p.Eliminated).All(p => next.Enemies.Count(e => e.Destination == p.Id) == 9), "fallen future allocation counted once and divided 9/9");
    }
    private async Task Defeat(int port, CancellationToken token)
    {
        await using var server = StartGame("defeat-server", true, true, port); await server.WaitFor(e => e.Type == "ready", "defeat server", options.StartupTimeout, token);
        await using var client = StartGame("defeat-client", false, true, port, null, "--automated"); await client.WaitFor(e => e.Type == "connected", "defeat client", options.StartupTimeout, token);
        await Action(client, "start", token); await Advance([client], token); await Advance([client], token); await Advance([client], token);
        MatchSnapshot lost = await Observe(client, s => s.Phase == Phase.Defeat, "ordinary losing strategy", token);
        Require(lost.Players.All(p => p.Eliminated), "zero city health ends match with defeat");
    }
    private async Task FailureCases(CancellationToken token)
    {
        using var occupied = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); occupied.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)occupied.LocalEndPoint!).Port;
        await using (var client = StartGame("unavailable-client", false, true, port, null, "--automated", "--connect-timeout-ms", "1000"))
        { await client.WaitFor(e => e.Type == "connection-failed", "unavailable server", 5000, token); Require(await client.WaitExit(token) != 0, "bounded unavailable server failure"); }
        await using (var server = StartGame("occupied-server", true, true, port))
        { await server.WaitFor(e => e.Type == "error", "occupied port", 5000, token); Require(await server.WaitExit(token) != 0 && occupied.IsBound, "collision leaves port owner intact"); }
        await using var readiness = StartGame("readiness-server", true, true, FreePort()); await readiness.WaitFor(e => e.Type == "ready", "readiness test", options.StartupTimeout, token);
        try { await readiness.WaitFor(e => e.Type == "missing", "missing readiness", 200, token); throw new Exception("Missing readiness passed."); } catch (TimeoutException) { Require(true, "missing readiness deadline"); }
        await readiness.Send("quit"); await readiness.WaitExit(token);
        try { await readiness.WaitFor(e => e.Type == "missing", "exited child", 1000, token); throw new Exception("Exited child passed."); } catch (InvalidOperationException e) when (e.Message.Contains("exited before", StringComparison.Ordinal)) { Require(true, "exited child fails pending expectation"); }
    }
}
