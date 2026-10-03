using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private readonly HashSet<string> _retriedEconomicActions = [];
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
        for (int actions = 0; actions < 100; actions++)
        {
            MatchSnapshot state = Latest(child);
            // Existing process fixtures put their Barracks at one and Metal Mine at two.
            CityState city = state.Players.Single(p => p.Id == child.PlayerId);
            SlotState[] slots = city.Slots.ToArray(); (slots[1], slots[2]) = (slots[2], slots[1]);
            MatchSnapshot canonical = state with { Players = state.Players.Select(p => p.Id == city.Id ? p with { Slots = slots } : p).ToArray() };
            EconomyAction? decision = CampaignStrategy.Next(canonical, child.PlayerId);
            if (decision is null) return;
            int slot = decision.Slot switch { 1 => 2, 2 => 1, _ => decision.Slot };
            string command = decision.Action switch
            {
                "build" => $"build {slot} {decision.Building}",
                "recruit" => $"recruit {slot} {decision.Unit}",
                "research-tech" => $"research-tech {TechnologyIds.Name(decision.Technology)}",
                "trade" => $"trade {slot} {decision.Resource} {decision.Bundles}",
                _ => $"{decision.Action} {slot}"
            };
            GameEvent accepted = await Action(child, command, token);
            if (decision.Action is "buy-plot" or "sell" or "trade" && _retriedEconomicActions.Add(decision.Action))
            {
                MatchSnapshot frozenRetry = State(await Action(child, "pause", token));
                Require(EconomicCity(frozenRetry.Players.Single(p => p.Id == child.PlayerId))
                    == EconomicCity(State(accepted).Players.Single(p => p.Id == child.PlayerId)), "pause preserves the accepted economic transaction");
                GameEvent[] prior = child.History(); string beforeRetry = Gameplay(frozenRetry);
                await child.Send($"retry {accepted.Result!.Sequence}");
                GameEvent retried = await child.WaitFor(e => e.Type == "ack" && e.Result == accepted.Result && !prior.Any(old => ReferenceEquals(old, e)), "fresh economic retry delivery", options.StartupTimeout, token);
                Require(Gameplay(State(retried)) == beforeRetry, "economic retry preserves exact stocks, land, investment and army: " + decision.Action);
                await Action(child, "resume", token);
            }
        }
        throw new InvalidOperationException("Campaign policy exceeded its bounded action count.");
    }
    private async Task Economy(Child child, CancellationToken token)
    {
        await Action(child, "build 0 farm", token); await Action(child, "build 1 barracks", token); await Action(child, "build 2 metalmine", token);
    }
    private async Task Advance(Child[] clients, CancellationToken token)
    {
        MatchSnapshot? resolved = null;
        foreach (Child child in clients) { await RecruitAll(child, token); resolved = State(await Action(child, "ready", token)); }
        if (resolved!.Phase == Phase.Preparation)
        {
            MatchSnapshot preparation = resolved;
            foreach (Child child in clients) await Observe(child, s => s.Phase == Phase.Preparation && s.TurnSerial == preparation.TurnSerial, "preparation synchronization", token);
            foreach (Child child in clients) { await RecruitAll(child, token); resolved = State(await Action(child, "ready", token)); }
        }
        MatchSnapshot target = resolved!;
        // Ready can acknowledge Preparation while the next authority tick
        // starts combat, before peers receive that intermediate revision.
        foreach (Child child in clients) await Observe(child, s => s.Revision >= target.Revision && s.TurnSerial >= target.TurnSerial
            && (s.Phase == target.Phase || target.Phase == Phase.Preparation && s.TurnSerial > target.TurnSerial && s.Wave >= target.Wave), "resolved ready check", token);
    }
    // Relative cooldown displays can age during legitimate post-clear cleanup
    // before pause. Absolute recovery identity and every economic field remain.
    private static string EconomicCity(CityState city) => JsonSerializer.Serialize(city with
    { DefenderCooldown = 0, Soldiers = city.Soldiers.Select(unit => unit with { Cooldown = 0 }).ToArray() }, WireJson.Options);
    private static string Gameplay(MatchSnapshot s) => JsonSerializer.Serialize(s with { Revision = 0, Players = s.Players.Select(p => p with { Connected = false, Ready = false }).ToArray() }, WireJson.Options);
    private async Task NetworkTests()
    {
        using var suite = CancellationTokenSource.CreateLinkedTokenSource(cancellation); suite.CancelAfter(options.Timeout);
        var scenarios = ScenarioNames.Network.Where(n => options.Scenario is null || options.Scenario == n).Select(name =>
            new Scenario(name, name switch
            {
                "solo-session" => "Socketless local authority and fresh application session state; pure core tests miss delivery/lifetime.",
                "playing-host-lifecycle" => "Bound host, real guest delivery, reconnect and host termination; dedicated tests lack local authority presentation.",
                "authority-resume-victory" => "Dedicated ENet authority, ownership/refusals, retry and real peer recovery; ordinary investment through wave eight adds earned personal research, active captured burn deadlines and a paused owned process restart. Research setup bounded at wave ten/120s, reuses peers and cleanup; terminal/boss campaign owned by SessionCampaignTests.",
                _ => "Preserved real ENet gameplay/lifecycle integration"
            }, token => _evidence.Measure(name, "network", async () =>
            {
                await using var owned = new ScenarioScope(name, _evidence);
                var worker = new Runner(options, token, _evidence, owned);
                switch (name)
                {
                    case "authority-resume-victory": await worker.AuthorityResumeVictory(options.Port ?? owned.Port(), token); break;
                    case "redistribution": await worker.Redistribution(owned.Port(), token); break;
                    case "defeat": await worker.Defeat(owned.Port(), token); break;
                    case "failure-cases": await worker.FailureCases(token); break;
                    case "solo-session": await worker.SoloSessionTest(token); break;
                    case "playing-host-lifecycle": await worker.PlayingHostLifecycleTest(owned.Port(), token); break;
                }
                await owned.DisposeAsync(); owned.CheckErrors();
            }))).ToArray();
        try
        {
            Console.WriteLine($"Network coverage: {(options.Scenario is null ? "full" : "selected")}; jobs={options.Jobs}; {string.Join(", ", scenarios.Select(s => s.Name))}");
            await _evidence.Measure("network", "suite", () => ScenarioScheduler.Run(scenarios, options.Jobs, suite.Token, _admission));
            Console.WriteLine(options.Scenario is null ? "Network verification passed (all required groups)." : $"Selected network scenario passed: {options.Scenario}.");
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested) { throw new TimeoutException($"Network suite exceeded {options.Timeout} ms; children were cleaned up."); }
    }
    private async Task AuthorityResumeVictory(int port, CancellationToken token)
    {
        var started = await StartServer("test-server", port, token, options.Port is null);
        await using var server = started.Server; port = started.Port;
        await using var a = StartGame("test-a", false, true, port, null, "--automated");
        GameEvent ca = await a.WaitFor(e => e.Type == "connected", "A connected", options.StartupTimeout, token);
        string bPath = Path.Combine(_scope!.Directory, "resume-b.json");
        await using var b = StartGame("test-b", false, true, port, null, "--automated", "--session-file", bPath);
        GameEvent cb = await b.WaitFor(e => e.Type == "connected", "B connected", options.StartupTimeout, token);
        Require(ca.PlayerId != cb.PlayerId && ca.PeerId != cb.PeerId, "stable IDs and distinct ENet peers");
        await using (var concurrent = ExpectedFailure(StartGame("concurrent", false, true, port, null, "--automated", "--session-file", bPath)))
            await concurrent.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("already connected"), "concurrent claim refusal", options.StartupTimeout, token);
        await using (var protocol = ExpectedFailure(StartGame("protocol", false, true, port, null, "--automated", "--protocol-version", "1")))
            await protocol.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("Protocol"), "old protocol refusal", options.StartupTimeout, token);
        await Action(a, "start", token); await Observe(b, s => s.Phase == Phase.Building, "B sees start", token);
        await Action(a, "ready", token);
        await Action(a, "build 0 farm", token, false);
        await Action(a, "unready", token);
        await Action(a, $"build 0 farm {cb.PlayerId}", token, false); await Action(a, "build 9 farm", token, false);
        await a.Send("raw {bad"); await a.WaitFor(e => e.Type == "ack" && e.Result!.Sequence == 0 && !e.Result.Accepted, "malformed rejection", options.StartupTimeout, token);
        string invalidPath = Path.Combine(_scope!.Directory, "invalid.json");
        File.WriteAllText(invalidPath, JsonSerializer.Serialize(new { Endpoint = $"{options.Host}:{port}", MatchId = State(ca).MatchId, Token = "INVALID", NextSequence = 1 }));
        await using (var invalid = ExpectedFailure(StartGame("invalid-session", false, true, port, null, "--automated", "--session-file", invalidPath)))
            await invalid.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("expired"), "invalid credential refusal", options.StartupTimeout, token);
        await Action(a, "unknown", token, false);
        await Economy(a, token); await Economy(b, token);
        await Action(a, "build 3 mine", token, false); await Action(a, "recruit 1", token, false);
        await using (var late = ExpectedFailure(StartGame("late", false, true, port, null, "--automated")))
            await late.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("locked"), "late join refusal", options.StartupTimeout, token);
        await Advance([a, b], token); await Advance([a, b], token); await Advance([a, b], token);
        MatchSnapshot firstClear = await Observe(a, s => s.Phase == Phase.Defeat || s.Phase == Phase.Building && s.Wave == 2, "paid first-wave defense", token);
        Require(firstClear.Phase == Phase.Building, "ordinary equipment and upkeep clear wave one");
        await Observe(b, s => s.Phase == Phase.Building && s.Wave == 2, "B first clear", token);
        await Action(b, "build 4 archeryrange", token);
        await Advance([a, b], token); await Advance([a, b], token);
        MatchSnapshot before = Latest(b);
        CommandResult spent = (await Action(b, "recruit 4 crossbowman", token)).Result!;
        var original = new Command(spent.Sequence, before.MatchId, before.Phase, before.TurnSerial, "recruit", cb.PlayerId, 4, SoldierType: UnitType.Crossbowman, ExpectedGeneration: before.Players.Single(p => p.Id == cb.PlayerId).Slots[4].Generation);
        string replay = "raw " + JsonSerializer.Serialize(original, WireJson.Options);
        int army = Latest(b).Players.Single(p => p.Id == cb.PlayerId).Soldiers.Length;
        GameEvent[] beforeDuplicate = b.History();
        await b.Send(replay); GameEvent dup = await b.WaitFor(e => !beforeDuplicate.Any(old => ReferenceEquals(old, e)) && e.Type == "ack" && e.Result!.Sequence == spent.Sequence && e.State!.Players.Single(p => p.Id == cb.PlayerId).Soldiers.Length == army, "duplicate recruitment", options.StartupTimeout, token);
        CityState beforeRecruit = before.Players.Single(p => p.Id == cb.PlayerId), afterRecruit = State(dup).Players.Single(p => p.Id == cb.PlayerId);
        ResourceCost equipment = beforeRecruit.RecruitmentQuotes.Single(q => q.Type == UnitType.Crossbowman && q.Level == beforeRecruit.Slots[4].Level).Cost;
        Require(beforeRecruit.Resources.TryPay(equipment, out ResourceCost paidEquipment) && afterRecruit.Resources == paidEquipment && afterRecruit.Food == beforeRecruit.Food, "equipment spends exactly once without recruitment food");
        await Advance([a, b], token);
        await Action(a, "build 4 mine", token, false);
        await b.Send("quit"); Require(await b.WaitExit(token) == 0, "departing client exits");
        MatchSnapshot absent = await Observe(a, s => s.Phase == Phase.Combat && !s.Players.Single(p => p.Id == cb.PlayerId).Connected, "retained disconnected city", token);
        // A pending attack is a transport timing witness. Keep ordinary speed
        // until the pause acknowledgement, then restore the setup budget while
        // frozen; faster simulation must not consume the observed windup in flight.
        await SimulationSpeed(server, 1, token);
        await Observe(a, s => s.Tick > absent.Tick + 9 && s.DyingBodies.Length > 0 && CombatPlayback.All(s).Any(u => u.PendingImpact && u.ImpactTick >= s.Tick + 6), "current casualty and pending combat continue while absent", token);
        MatchSnapshot frozen = State(await Action(a, "pause", token));
        await Observe(a, s => s.Paused && s.Tick == frozen.Tick, "paused snapshot", token);
        Require(CombatPlayback.All(frozen).Any(u => u.PendingImpact) && frozen.Players.Single(p => p.Id == cb.PlayerId).Soldiers.Any(u => u.Type == UnitType.Crossbowman),
            "paused authority retains pending action and typed soldier before resume");
        CombatContact(frozen);
        Require(frozen.DyingBodies.Length > 0, "paused authority retains current deaths");
        await SimulationSpeed(server, options.SimulationSpeed, token);
        await using var resumed = StartGame("test-resumed", false, true, port, null, "--automated", "--session-file", bPath);
        GameEvent cr = await resumed.WaitFor(e => e.Type == "connected", "restart resume", options.StartupTimeout, token);
        Require(cr.PlayerId == cb.PlayerId && cr.PeerId != cb.PeerId && Gameplay(State(cr)) == Gameplay(frozen), "new peer restores original city and frozen gameplay without regrant");
        CombatContact(State(cr));
        await resumed.Send(replay); GameEvent retried = await resumed.WaitFor(e => e.Type == "ack" && e.Result!.Sequence == spent.Sequence, "retry after process restart", options.StartupTimeout, token);
        Require(Gameplay(State(retried)) == Gameplay(frozen), "old accepted spending remains exactly once after resume");
        await Action(resumed, "ready", token, false); MatchSnapshot paused = Latest(resumed);
        await Observe(a, s => s.Paused && s.Revision >= paused.Revision && s.Tick == frozen.Tick, "paused revisions remain visible", token);
        await Action(a, "resume", token);
        MatchSnapshot secondClear = await Observe(a, s => s.Phase == Phase.Defeat || s.Phase == Phase.Building && s.Wave == 3, "ordinary early clear after peer recovery", token);
        Require(secondClear.Phase == Phase.Building, "ordinary cooperative equipment survives recovery and wave two");
        await Observe(resumed, s => s.Revision >= secondClear.Revision && s.Phase == Phase.Building, "shared early result", token);
        await using var researched = await TransportedResearch(a, resumed, server, port, bPath, token);
        await Action(a, "stale-ready", token, false);
        // Drain one bounded burst in one client frame. Separate stdin lines are
        // throttled by the client's command pump and can straddle rate windows.
        await a.Send("raw-burst 192 null");
        await a.WaitFor(e => e.Type == "ack" && e.Message == "Command rate exceeded.", "excessive request rejection", options.StartupTimeout, token);
        await Action(researched, "unknown", token, false);
        await server.Send("quit"); await a.WaitFor(e => e.Type == "server-disconnected", "server stopped feedback", options.StartupTimeout, token);
        // A credential from a stopped match must not silently create a new city.
        await using var replacement = StartGame("replacement-server", true, true, port);
        await replacement.WaitFor(e => e.Type == "ready", "replacement server", options.StartupTimeout, token);
        await using var staleSession = ExpectedFailure(StartGame("stale-session", false, true, port, null, "--automated", "--session-file", bPath));
        await staleSession.WaitFor(e => e.Type == "connection-failed" && e.Message!.Contains("expired"), "expired session refusal", options.StartupTimeout, token);
    }
    private async Task ReinforcementInvestment(Child child, bool receivingCity, CancellationToken token)
    {
        while (CampaignStrategy.ReinforcementInvestment(Latest(child), child.PlayerId, receivingCity) is EconomyAction action)
        {
            string request = action.Action == "build" ? $"build {action.Slot} {action.Building}" : $"{action.Action} {action.Slot}";
            await Action(child, request, token);
        }
    }
    private async Task Redistribution(int port, CancellationToken token)
    {
        await using var a = StartGameRole("transfer-a", "playing-host", true, port, null, "--combat-seed", "90");
        await a.WaitFor(e => e.Type == "ready", "playing host transfer readiness", options.StartupTimeout, token);
        await using var b = StartGame("transfer-b", false, true, port, null, "--automated");
        await b.WaitFor(e => e.Type == "connected", "ordered second city admission", options.StartupTimeout, token);
        string cPath = Path.Combine(_scope!.Directory, "observer.json");
        await using var c = StartGame("transfer-c", false, true, port, null, "--automated", "--session-file", cPath);
        foreach (Child p in new[] { a, b, c }) await p.WaitFor(e => e.Type == "connected", "three-player lobby", options.StartupTimeout, token);
        int deadId = c.PlayerId;
        await Action(a, "start", token); foreach (Child p in new[] { b, c }) await Observe(p, s => s.Phase == Phase.Building, "transfer start", token);
        await Economy(a, token); await Economy(b, token);
        for (int preparation = 0; preparation < 4; preparation++)
        {
            await ReinforcementInvestment(a, true, token); await ReinforcementInvestment(b, false, token);
            foreach (Child child in new[] { a, b, c }) await Action(child, "ready", token);
            MatchSnapshot advanced = Latest(c);
            foreach (Child child in new[] { a, b }) await Observe(child, state => state.Revision >= advanced.Revision && state.Phase == advanced.Phase, "reinforcement opening synchronized", token);
        }
        MatchSnapshot initial = Latest(a);
        await SimulationSpeed(a, 1, token); // Transfer/admission transients are ordinary-speed transport witnesses.
        UnitState[] attackers = initial.Enemies.Where(e => e.Origin == deadId).ToArray();
        MatchSnapshot fallen = await Observe(a, s => s.Phase == Phase.Combat && s.Players.Single(p => p.Id == deadId).Eliminated, "ordinary under-defended city falls", token);
        UnitState[] transferred = fallen.Enemies.Where(e => e.Origin == deadId).ToArray();
        Require(transferred.Length > 0 && transferred.All(e => e.Destination != deadId) && transferred.Select(e => e.Id).Distinct().Count() == transferred.Length, "live enemies transferred immediately without duplication");
        Require(transferred.All(e => attackers.Any(old => old.Id == e.Id && old.Health >= e.Health)), "transfers preserve identity and damage");
        Require(transferred.All(e => e.Faction == Faction.Skeletons && attackers.Any(old => old.Id == e.Id && old.Type == e.Type && old.Profile == e.Profile && old.Rank == e.Rank && old.Level == e.Level && old.Size == e.Size && old.IsBoss == e.IsBoss) && !e.PendingImpact && e.Cooldown == Math.Max(0, e.ReadyTick - fallen.Tick))
            && transferred.Any(e => e.Cooldown > 0), "transfers retain profiles/recovery and cancel former windups");
        Require(fallen.Players.All(city => city.Food == initial.Players.Single(old => old.Id == city.Id).Food
            && JsonSerializer.Serialize(city.LastUpkeep, WireJson.Options) == JsonSerializer.Serialize(initial.Players.Single(old => old.Id == city.Id).LastUpkeep, WireJson.Options)), "transfers retain already-resolved food balances and battle receipts");
        CombatContact(fallen);
        MatchSnapshot seen = await Observe(b, s => s.Revision == fallen.Revision, "matching transfer revision", token);
        Require(JsonSerializer.Serialize(fallen, WireJson.Options) == JsonSerializer.Serialize(seen, WireJson.Options), "surviving clients agree on transfers");
        await Observe(a, s => s.Wave == 2 && s.Phase == Phase.Building, "redistributed wave clear", token); await Observe(b, s => s.Wave == 2 && s.Phase == Phase.Building, "B next wave", token);
        await c.Send("quit"); await c.WaitExit(token);
        await using var observer = StartGame("observer-resumed", false, true, port, null, "--automated", "--session-file", cPath);
        GameEvent restored = await observer.WaitFor(e => e.Type == "connected", "eliminated observer resume", options.StartupTimeout, token);
        Require(restored.PlayerId == deadId && State(restored).Players.Single(p => p.Id == deadId).Eliminated, "resume cannot revive eliminated city");
        await Action(observer, "build 0 mine", token, false); await Action(observer, "pause", token); await Action(observer, "resume", token);
        for (int preparation = 0; preparation < 4; preparation++)
        {
            await ReinforcementInvestment(a, true, token);
            if (preparation == 3) await Action(b, "recruit 1", token);
            await Action(a, "ready", token);
            MatchSnapshot advanced = State(await Action(b, "ready", token));
            await Observe(a, s => s.Revision >= advanced.Revision && s.Phase == advanced.Phase, "transfer preparation synchronized", token);
        }
        MatchSnapshot next = await Observe(a, s => s.Phase == Phase.Combat && s.Wave == 2, "future original roster allocation", token);
        Require(next.Enemies.Length == 12 && next.Players.Where(p => !p.Eliminated).All(p => next.Enemies.Count(e => e.Destination == p.Id) == 6), "fallen future allocation counted once and divided 6/6");
        var board = new HexBoard(next.Rules.Combat.Board);
        int[] forward = board.Front(Faction.Skeletons).ToArray();
        MatchSnapshot cleared = await Observe(a, s => s.Phase is Phase.Defeat or Phase.Victory || s.Phase == Phase.Combat && !s.Players.Single(p => p.Id == b.PlayerId).Eliminated
            && !s.Enemies.Any(u => u.Destination == a.PlayerId)
            && s.Players.Single(p => p.Id == a.PlayerId).Soldiers.Any(u => u.Deployed && forward.Contains(u.Hex!.Position.Cell)), "receiving city cleared with surviving forward defenders", token);
        Require(cleared.Phase == Phase.Combat, "cleared-frontage witness remains a live battle");
        CombatContact(cleared);
        MatchSnapshot reinforced = await Observe(a, s => s.Phase == Phase.Combat && s.Wave == 2 && s.Players.Any(p => p.Id == b.PlayerId && p.Eliminated) && s.Admissions.Any(d => d.City == a.PlayerId), "reinforcement into cleared forward band", token);
        AdmissionBound admission = reinforced.Admissions.Single(d => d.City == a.PlayerId);
        Require(admission.AdmissionTick is not null && admission.AdmissionTick <= admission.FirstAdmissionBound, "reinforcement meets original protected-entry admission bound");
        CombatContact(reinforced);
        MatchSnapshot engaged = await Observe(a, s => s.Tick > reinforced.Tick && s.CombatEvents.Any(e => e.Tick >= admission.TransferTick && e.Type == CombatEventType.Hit), "transferred army engages after admission", token);
        CombatContact(engaged);
        MatchSnapshot completed = await Observe(a, s => s.Tick >= reinforced.Tick && (s.Phase == Phase.Defeat || s.Phase == Phase.Building && s.Wave == 3), "reinforced battlefield completes", token);
        Require(completed.Phase == Phase.Building && completed.Wave == 3 && completed.DefeatReason == DefeatReason.None, "reinforced battlefield completes ordinarily without stall fallback");
    }
    private static void CombatContact(MatchSnapshot state)
    {
        var board = new HexBoard(state.Rules.Combat.Board);
        UnitState[] units = CombatPlayback.All(state).Concat(state.DyingBodies).ToArray();
        Require(units.All(u => u.Hex is not null && u.Hex.Id == u.Id && u.Hex.City == u.Destination && u.Hex.Faction == u.Faction), "complete hex identity for living and dying units");
        CombatReservations reconstructed = CombatReservations.Reconstruct(board, units.Select(u => u.Hex!));
        Require(JsonSerializer.Serialize(reconstructed, WireJson.Options) == JsonSerializer.Serialize(state.Reservations, WireJson.Options),
            "complete occupied and moving/death reservations reconstruct from current units");
        Require(RulesIdentity.Resolve(state.Rules) == state.ConfigurationFingerprint, "published combat configuration matches board and profiles");
    }
    private async Task Defeat(int port, CancellationToken token)
    {
        var started = await StartServer("defeat-server", port, token);
        await using var server = started.Server; port = started.Port;
        await using var client = StartGame("defeat-client", false, true, port, null, "--automated"); await client.WaitFor(e => e.Type == "connected", "defeat client", options.StartupTimeout, token);
        await Action(client, "start", token);
        for (int readiness = 0; readiness < 4; readiness++) await Action(client, "ready", token);
        MatchSnapshot lost = await Observe(client, s => s.Phase == Phase.Defeat, "ordinary losing strategy", token);
        Require(lost.Players.All(p => p.Eliminated && p.Health == 0) && lost.DefeatReason == DefeatReason.AllCitiesFallen && lost.Stall is null, "zero city health ends match with ordinary defeat, without stall fallback");
    }
    private async Task FailureCases(CancellationToken token)
    {
        using var occupied = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); occupied.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        int port = ((IPEndPoint)occupied.LocalEndPoint!).Port;
        await using (var client = ExpectedFailure(StartGame("unavailable-client", false, true, port, null, "--automated", "--connect-timeout-ms", "1000")))
        { await client.WaitFor(e => e.Type == "connection-failed", "unavailable server", 5000, token); Require(await client.WaitExit(token) != 0, "bounded unavailable server failure"); }
        await using (var server = ExpectedFailure(StartGame("occupied-server", true, true, port), bindFailure: true))
        { await server.WaitFor(e => e.Type == "error", "occupied port", 5000, token); Require(await server.WaitExit(token) != 0 && occupied.IsBound, "collision leaves port owner intact"); }
        var retried = await StartServer("readiness-server", port, token);
        await using var readiness = retried.Server;
        Require(retried.Port != port && occupied.IsBound, "automatic bind collision retries without touching endpoint owner");
        try { await readiness.WaitFor(e => e.Type == "missing", "missing readiness", 200, token); throw new InvalidOperationException("Missing readiness passed."); } catch (TimeoutException) { Require(true, "missing readiness deadline"); }
        await readiness.Send("quit"); await readiness.WaitExit(token);
        try { await readiness.WaitFor(e => e.Type == "missing", "exited child", 1000, token); throw new InvalidOperationException("Exited child passed."); } catch (InvalidOperationException e) when (e.Message.Contains("exited before", StringComparison.Ordinal)) { Require(true, "exited child fails pending expectation"); }
    }
}
