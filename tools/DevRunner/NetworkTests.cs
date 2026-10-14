using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private readonly HashSet<string> _retriedEconomicActions = [];
    private static MatchSnapshot Latest(Child child) => child.LatestState ?? throw new InvalidOperationException($"{child.Name}: no delivered state.");
    private async Task<GameEvent> Action(Child child, string command, CancellationToken token, bool accepted = true)
    {
        long previous = child.History().Where(e => e.Type == "ack").Select(e => e.Result!.Sequence).DefaultIfEmpty().Max();
        if (command is "ready" or "pause" or "resume") _timing?.Record("driver-request", Latest(child), info: new { Child = child.Name, Command = command, AfterSequence = previous });
        await child.Send(command);
        GameEvent result = await child.WaitFor(e => e.Type == "ack" && e.Result!.Sequence > previous, command, options.StartupTimeout, token);
        if (command is "ready" or "pause" or "resume") _timing?.Record("driver-ack", result.State, info: new { Child = child.Name, Command = command, result.Result!.Sequence, result.Result.Accepted });
        Require(result.Result!.Accepted == accepted, $"{child.Name}: {command}: {result.Message}");
        return result;
    }
    private async Task<MatchSnapshot> Observe(Child child, Func<MatchSnapshot, bool> predicate, string expectation, CancellationToken token)
        => State(await child.WaitFor(e => e.State is not null && predicate(e.State), expectation, options.Timeout, token));
    // Transient witnesses must still exist in the newest delivered state;
    // ordinary Observe intentionally permits retained historical barriers.
    internal Task<MatchSnapshot> ObserveCurrent(Child child, Func<MatchSnapshot, bool> predicate, string expectation, CancellationToken token)
        => Observe(child, s => ReferenceEquals(s, Latest(child)) && predicate(s), expectation, token);
    private Task<(GameEvent Receipt, int TargetId)> PauseBurn(Child child, int? city, CancellationToken token)
        => BurnAdmission.Pause(Latest(child), city, AckSequence(child), () => Latest(child),
            async (floor, cancellation) => { await ObserveCurrent(child, s => s.Revision > floor, "fresh current burn admission revision", cancellation); },
            (command, cancellation) => Action(child, command, cancellation), token);
    private async Task RecruitAll(Child child, CancellationToken token)
    {
        // Cosmetic bodies can outlive the clear, but hold no reservations.
        // Let ordinary cleanup restore persistent homes before exact retry measurements.
        MatchSnapshot entry = Latest(child);
        await Observe(child, s => s.MatchId == entry.MatchId && s.Revision >= entry.Revision && s.DyingBodies.Length == 0,
            "post-clear home restoration before investment", token);
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
                "build" => $"{(decision.Payment == ConstructionPayment.GoldRecovery ? "build-recovery" : "build")} {slot} {decision.Building}",
                "recruit" => $"recruit {slot} {decision.Unit}",
                "retire" => $"retire {decision.UnitId}",
                "research-tech" => $"research-tech {TechnologyIds.Name(decision.Technology)}",
                "trade" => $"trade {slot} {decision.Resource} {decision.Bundles}",
                _ => $"{decision.Action} {slot}"
            };
            GameEvent accepted = await Action(child, command, token);
            if (decision.Action is "buy-plot" or "buy-home" or "retire" or "sell" or "trade" && _retriedEconomicActions.Add(decision.Action))
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
        // One Advance means one visible numbered preparation, never a hidden
        // extra income-free ready check. Final editing happens in preparation 3.
        MatchSnapshot target = resolved!;
        // Ready can acknowledge Preparation while the next authority tick
        // starts combat, before peers receive that intermediate revision.
        foreach (Child child in clients) await Observe(child, s => s.Revision >= target.Revision && s.TurnSerial >= target.TurnSerial
            && (s.Phase == target.Phase || target.Phase == Phase.Preparation && s.TurnSerial > target.TurnSerial && s.Wave >= target.Wave), "resolved ready check", token);
    }
    // Relative cooldown displays can age before pause. Absolute recovery
    // identity, placement and every economic field remain.
    private static string EconomicCity(CityState city) => JsonSerializer.Serialize(city with
    { DefenderCooldown = 0, Soldiers = city.Soldiers.Select(unit => unit with { Cooldown = 0 }).ToArray() }, WireJson.Options);
    private static string Gameplay(MatchSnapshot s) => JsonSerializer.Serialize(s with { Revision = 0, Players = s.Players.Select(p => p with { Connected = false, Ready = false }).ToArray() }, WireJson.Options);
    private async Task NetworkTests(bool deep = false)
    {
        using var suite = CancellationTokenSource.CreateLinkedTokenSource(cancellation); suite.CancelAfter(options.Timeout);
        string coverage = options.Scenario is not null ? "selected" : deep ? "deep" : "bounded";
        string[] defaults = deep ? ScenarioNames.Network : ["failure-cases", "solo-session", "playing-host-lifecycle"];
        var scenarios = (options.Scenario is null ? defaults : [options.Scenario]).Select(name =>
            new Scenario(name, name switch
            {
                "solo-session" => "Socketless local authority and fresh application session state; pure core tests miss delivery/lifetime.",
                "playing-host-lifecycle" => "Bound host, real guest delivery, reconnect and host termination; dedicated tests lack local authority presentation.",
                "active-fallen" => "Fixed numerical active-field Keep-health controls: real host/ENet/codec, exact member/action/claim receiving, same-host peer/process resume and actual later impacts. Synthetic inputs, NOT balance/full-game acceptance.",
                "eliminated-resume" => "Earned two-wave paid veteran/storage prefix; controlled W3 fallen/protected-stored encounter; real ENet spectator receiving/reconnect/restart/ledger and command nonmutation; next living-owner allocation W4. Not earned eliminated-stored balance; active fallen fighters are a separate deterministic authority witness.",
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
                    case "eliminated-resume": await worker.EliminatedResume(owned.Port(), token); break;
                    case "active-fallen": await worker.ActiveFallen(owned.Port(), token); break;
                    case "defeat": await worker.Defeat(owned.Port(), token); break;
                    case "failure-cases": await worker.FailureCases(token); break;
                    case "solo-session": await worker.SoloSessionTest(token); break;
                    case "playing-host-lifecycle": await worker.PlayingHostLifecycleTest(owned.Port(), token); break;
                }
                await owned.DisposeAsync(); owned.CheckErrors();
            }))).ToArray();
        try
        {
            Console.WriteLine($"Network coverage: {coverage}; jobs={options.Jobs}; {string.Join(", ", scenarios.Select(s => s.Name))}");
            await _evidence.Measure("network", "suite", () => ScenarioScheduler.Run(scenarios, options.Jobs, suite.Token, _admission));
            Console.WriteLine(options.Scenario is null ? deep ? "Deep headless network journeys passed." : "Bounded headless transport verification passed; deep journeys not executed." : $"Selected network scenario passed: {options.Scenario}.");
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested) { throw new TimeoutException($"Network suite exceeded {options.Timeout} ms; children were cleaned up."); }
    }
    // Retained complete paid/recovery/research pass: logs/20261005-171424-d97ea622.
    // Random combat can clear the team wave while eliminating the city needed below.
    internal const ulong AuthorityResumeVictorySeed = 16366921918512773030UL;
    private async Task AuthorityResumeVictory(int port, CancellationToken token)
    {
        var started = await StartServer("test-server", port, token, options.Port is null,
            extra: ["--combat-seed", AuthorityResumeVictorySeed.ToString(System.Globalization.CultureInfo.InvariantCulture)]);
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
        Require(firstClear.Phase == Phase.Building && firstClear.Players.All(p => !p.Eliminated), "ordinary equipment and upkeep clear wave one with both cities living");
        await Observe(b, s => s.Phase == Phase.Building && s.Wave == 2, "B first clear", token);
        await Action(b, "buy-plot 7", token);
        await Action(b, "build 7 archeryrange", token);
        await Advance([a, b], token); await Advance([a, b], token);
        // Policy refill can occupy both homes after ordinary first-wave casualties.
        // Buy quoted field room before the explicit retry/reconnect witness.
        await EnsureFieldRoom(b, token, actualInput: false);
        MatchSnapshot before = Latest(b);
        CommandResult spent = (await Action(b, "recruit 7 crossbowman", token)).Result!;
        var original = new Command(spent.Sequence, before.MatchId, before.Phase, before.TurnSerial, "recruit", cb.PlayerId, 7, SoldierType: UnitType.Crossbowman, ExpectedGeneration: before.Players.Single(p => p.Id == cb.PlayerId).Slots[7].Generation);
        string replay = "raw " + JsonSerializer.Serialize(original, WireJson.Options);
        int army = Latest(b).Players.Single(p => p.Id == cb.PlayerId).Soldiers.Length;
        GameEvent[] beforeDuplicate = b.History();
        await b.Send(replay); GameEvent dup = await b.WaitFor(e => !beforeDuplicate.Any(old => ReferenceEquals(old, e)) && e.Type == "ack" && e.Result!.Sequence == spent.Sequence && e.State!.Players.Single(p => p.Id == cb.PlayerId).Soldiers.Length == army, "duplicate recruitment", options.StartupTimeout, token);
        CityState beforeRecruit = before.Players.Single(p => p.Id == cb.PlayerId), afterRecruit = State(dup).Players.Single(p => p.Id == cb.PlayerId);
        ResourceCost equipment = beforeRecruit.RecruitmentQuotes.Single(q => q.Type == UnitType.Crossbowman && q.Level == beforeRecruit.Slots[7].Level).Cost;
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
        Require(secondClear.Phase == Phase.Building && secondClear.Players.All(p => !p.Eliminated), "ordinary cooperative equipment survives recovery and wave two with both cities living");
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
    private async Task Redistribution(int port, CancellationToken token)
    {
        // Paid shared-arena support + exact wire agreement/resume. The third
        // sector funds an ordinary tower, with no field army or seeded strength.
        // Uninvested-sector rescue is qualified separately by the four-owner
        // paid witness, rather than imposed on every transport setup. Old teleport
        // admission and duplicated second-fall choreography are consolidated in
        // deterministic core redirection/spectator tests, not predicted here.
        await using var a = StartGameRole("transfer-a", "playing-host", true, port, null, "--combat-seed", "8");
        await a.WaitFor(e => e.Type == "ready", "playing host transfer readiness", options.StartupTimeout, token);
        await using var b = StartGame("transfer-b", false, true, port, null, "--automated");
        await b.WaitFor(e => e.Type == "connected", "ordered second city admission", options.StartupTimeout, token);
        string cPath = Path.Combine(_scope!.Directory, "observer.json");
        await using var c = StartGame("transfer-c", false, true, port, null, "--automated", "--session-file", cPath);
        foreach (Child p in new[] { a, b, c }) await p.WaitFor(e => e.Type == "connected", "three-player lobby", options.StartupTimeout, token);
        int thirdId = c.PlayerId;
        await Action(a, "start", token); foreach (Child p in new[] { b, c }) await Observe(p, s => s.Phase == Phase.Building, "transfer start", token);
        await SimulationSpeed(a, 1, token);
        var born = new Dictionary<int, (int Owner, UnitType Type, int Level)>();
        async Task Paid(Child child, EconomyAction action)
        {
            MatchSnapshot before = Latest(child);
            CityState city = before.Players.Single(p => p.Id == child.PlayerId);
            ResourceCost quote = action.Action switch
            {
                "build" => action.Payment == ConstructionPayment.GoldRecovery
                    ? before.BuildingCatalog.Single(b => b.Type == action.Building).RecoveryConstruction!.Value
                    : before.BuildingCatalog.Single(b => b.Type == action.Building).Construction,
                "upgrade" => city.Slots[action.Slot].UpgradeQuote!.Value,
                "recruit" => city.RecruitmentQuotes.Single(q => q.Type == action.Unit && q.Level == city.Slots[action.Slot].Level).Cost,
                "buy-home" => new(city.Army!.HomePrices[city.Army.PurchasedHomes - 2]),
                "sell" => default,
                _ => throw new InvalidOperationException("Unexpected redistribution action: " + action.Action)
            };
            string request = action.Action switch
            {
                "build" => $"{(action.Payment == ConstructionPayment.GoldRecovery ? "build-recovery" : "build")} {action.Slot} {action.Building}",
                "recruit" => $"recruit {action.Slot} {action.Unit}",
                _ => $"{action.Action} {action.Slot}"
            };
            CityState after = State(await Action(child, request, token)).Players.Single(p => p.Id == child.PlayerId);
            Require(city.Resources.TryPay(quote, out ResourceCost balance), "redistribution action is actually affordable");
            if (action.Action == "sell")
            {
                Require(balance.TryAdd(city.Slots[action.Slot].Refund, out ResourceCost refunded), "redistribution sale refund fits");
                balance = refunded;
            }
            Require(after.Resources == balance, "redistribution pays the published quote or receives the actual investment refund");
            UnitState[] recruited = after.Soldiers.Where(u => !city.Soldiers.Any(old => old.Id == u.Id)).ToArray();
            Require(recruited.Length == (action.Action == "recruit" ? 1 : 0), "only recruitment creates a paid unit identity");
            foreach (UnitState unit in recruited)
            {
                Require(unit.Owner == child.PlayerId && unit.Type == action.Unit && unit.Level == city.Slots[action.Slot].Level, "paid births retain owned type and trainer level");
                born.Add(unit.Id, (unit.Owner, unit.Type, unit.Level));
            }
        }
        async Task Prepare(Child[] clients, bool firstWave)
        {
            MatchSnapshot before = Latest(a);
            if (firstWave && before.Turn == 1)
                await Paid(c, new("build", ArmyConfiguration.PlotOffset, Building.ArrowTower));
            foreach ((int player, EconomyAction action) in CampaignStrategy.RedistributionPreparation(before, a.PlayerId, b.PlayerId))
                await Paid(player == a.PlayerId ? a : b, action);
            if (!firstWave)
            {
                for (int actions = 0; CampaignStrategy.ReinforcementInvestment(Latest(a), a.PlayerId, true) is EconomyAction investment; actions++)
                {
                    Require(actions < 100, "bounded receiving-city investment");
                    await Paid(a, investment);
                }
                if (before.Turn > 1) await Paid(b, new("recruit", 1));
            }
            foreach (Child child in clients) await Action(child, "ready", token);
            foreach (Child child in clients)
                await Observe(child, s => s.TurnSerial > before.TurnSerial && s.ProductionCount == before.ProductionCount + 1,
                    "redistribution preparation settlement synchronized", token);
        }
        for (int preparation = 0; preparation < 3; preparation++) await Prepare([a, b, c], true);
        MatchSnapshot initial = await Observe(a, s => s.Phase == Phase.Combat && s.Wave == 1, "paid redistribution opening", token);
        Require(initial.ProductionCount == 3 && initial.Players.Where(p => p.Id != thirdId).All(p => p.Soldiers.Length == 6
            && p.Metal == 12 && p.Food == 9 && p.LastUpkeep!.Paid == 6 && p.LastUpkeep.Unfed.Length == 0
            && p.LastUpkeep.Funded.Order().SequenceEqual(p.Soldiers.Select(u => u.Id).Order())), "six actually funded opening identities per equipped city");
        CombatContact(initial);
        Require(initial.Players.Single(p => p.Id == thirdId).Soldiers.Length == 0, "third sector has no privately seeded army");
        Require(initial.Structures.Any(s => s.Owner == thirdId && s.Kind == ArenaStructureKind.ArrowTower),
            "third sector retains its ordinarily paid defense");
        // Combat publication can skip transient revisions. An ordinary accepted
        // pause gives every peer the same stable receipt before exact comparison.
        MatchSnapshot paused = State(await Action(a, "pause", token));
        foreach (Child child in new[] { b, c })
        {
            MatchSnapshot seen = await Observe(child, s => s.Revision == paused.Revision, "matching paused shared arena", token);
            Require(JsonSerializer.Serialize(paused, WireJson.Options) == JsonSerializer.Serialize(seen, WireJson.Options), "all three peers agree on squads, members and claims");
            CombatContact(seen);
        }
        await Action(a, "resume", token);
        MatchSnapshot clear = await Observe(a, s => s.Wave == 2 && s.Phase == Phase.Building, "paid cooperative rescue clear", token);
        Require(clear.Players.All(p => !p.Eliminated), "paid team protects all three Keeps including the tower-only sector");
        Require(clear.Players.All(city => JsonSerializer.Serialize(city.LastUpkeep, WireJson.Options)
            == JsonSerializer.Serialize(initial.Players.Single(old => old.Id == city.Id).LastUpkeep, WireJson.Options)), "clear retains actual paid battle receipts");
        foreach (Child child in new[] { b, c }) await Observe(child, s => s.Wave == 2 && s.Phase == Phase.Building, "all peers observe rescue clear", token);
        await c.Send("quit"); await c.WaitExit(token);
        await using var observer = StartGame("observer-resumed", false, true, port, null, "--automated", "--session-file", cPath);
        GameEvent restored = await observer.WaitFor(e => e.Type == "connected", "third player resume", options.StartupTimeout, token);
        Require(restored.PlayerId == thirdId && !State(restored).Players.Single(p => p.Id == thirdId).Eliminated, "resume retains rescued owner identity");
        CombatContact(State(restored));
        await Observe(a, s => s.DyingBodies.Length == 0, "cosmetic cleanup before investment", token);
        MatchSnapshot? next = null;
        for (int preparation = 0; preparation < 3; preparation++)
        {
            foreach (var (player, action) in CampaignStrategy.RedistributionPreparation(Latest(a), a.PlayerId, b.PlayerId))
                await Paid(player == a.PlayerId ? a : b, action);
            foreach (Child client in new[] { a, b })
                for (int actions = 0; CampaignStrategy.ReinforcementInvestment(Latest(client), client.PlayerId, client == a) is EconomyAction investment; actions++)
                {
                    Require(actions < 100, "bounded receiving-city investment");
                    await Paid(client, investment);
                }
            GameEvent? ready = null;
            foreach (Child child in new[] { a, b, observer }) ready = await Action(child, "ready", token);
            MatchSnapshot advanced = State(ready!);
            if (preparation == 2) next = advanced;
            foreach (Child child in new[] { a, b, observer })
                await Observe(child, s => s.Revision >= advanced.Revision && s.Phase == advanced.Phase
                    && s.TurnSerial >= advanced.TurnSerial, "shared preparation synchronized", token);
        }
        // Initial allocation belongs to the accepted final Ready receipt. A
        // later bounded-ring snapshot can legitimately contain fewer members.
        Require(next is { Phase: Phase.Combat, Wave: 2 } && next.Enemies.Length == 12
            && next.Players.All(p => next.Enemies.Count(e => e.Destination == p.Id) == 4), "next roster counted once and divided 4/4/4");
        CombatContact(next!);

    }
    private static void CombatContact(MatchSnapshot state)
    {
        SnapshotCompatibility.RequireSharedArena(state);
        UnitState[] units = CombatPlayback.All(state).Concat(state.DyingBodies).ToArray();
        Require(units.All(u => u.Hex is not null && u.Hex.Id == u.Id && u.Hex.City == u.Destination && u.Hex.Faction == u.Faction), "complete hex identity for living and dying units");
        CombatReservations reconstructed = CombatReservations.Reconstruct(state.Arena!, state.Squads);
        Require(JsonSerializer.Serialize(reconstructed, WireJson.Options) == JsonSerializer.Serialize(state.Reservations, WireJson.Options),
            "exclusive squad sources and committed transits reconstruct independently of dying member cosmetics");
        Require(RulesIdentity.Resolve(state.Rules) == state.ConfigurationFingerprint, "published combat configuration matches board and profiles");
    }
    private async Task Defeat(int port, CancellationToken token)
    {
        var started = await StartServer("defeat-server", port, token);
        await using var server = started.Server; port = started.Port;
        await using var client = StartGame("defeat-client", false, true, port, null, "--automated"); await client.WaitFor(e => e.Type == "connected", "defeat client", options.StartupTimeout, token);
        await Action(client, "start", token);
        for (int readiness = 0; readiness < 3; readiness++) await Action(client, "ready", token);
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
