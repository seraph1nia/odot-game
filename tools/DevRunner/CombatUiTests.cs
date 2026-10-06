using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    internal static bool AuthoredAttack(UnitObservation unit, UnitType type)
        => unit.Visible && !unit.Dead && unit.Type == type && unit.Clip == "attack" && unit.AttackActive;

    private static readonly Lazy<IReadOnlyDictionary<string, GlbSummary>> CasualtyClips = new(() =>
        AuthoredAssets.Validate(Path.Combine(FindRoot(), "src", "Game", "Assets", "Authored")));

    internal static bool FreshDeathPose(double poseSeconds) => poseSeconds < .35;

    internal static IEnumerable<UnitState> EligibleCasualties(MatchSnapshot state, int city, long after)
        => state.DyingBodies.Where(u => PresentationLimits.SamplesPose(u, city)
            && u.Hex is { Lifecycle: UnitLifecycle.Dying } hex && hex.DeathStartTick > after && hex.DeathEndTick > state.Tick + 12
            && FreshDeathPose(CombatPlayback.DeathPose(u, hex.FrozenTick ?? state.Tick,
                CasualtyClips.Value[Game.AssetCatalog.Unit(u.Type, u.Faction).Path].ClipLengths["death"])));

    internal static bool CasualtyInspectionReady(MatchSnapshot state, int city, long after)
        => EligibleCasualties(state, city, after).Any()
            && state.Enemies.Any(u => PresentationLimits.SamplesPose(u, city) && u.Health > 0 && u.Health < u.Profile.Health);

    internal static void RenderedContact(UiObservation observation)
    {
        // Numerical transit claims still hold. Only committed visual crossings
        // are exempt; settled living/death anchors must never collapse.
        UnitObservation[] units = observation.Units.Where(u => u.Visible && u.Deployed && u.Hex?.HoldsTransit != true).ToArray();
        foreach (UnitObservation unit in units)
            foreach (UnitObservation other in units.Where(u => u.Id > unit.Id && u.Destination == unit.Destination))
                if (Math.Sqrt(Math.Pow(unit.X - other.X, 2) + Math.Pow(unit.Z - other.Z, 2)) < 0.4 - 1e-4)
                    throw new InvalidOperationException("Settled rendered anchors collapsed: " + unit.Id + "/" + other.Id);
        foreach (StrikeObservation strike in observation.Strikes.Where(s => s.Visible))
        {
            UnitObservation actor = observation.Units.Single(u => u.Id == strike.Id);
            UnitObservation? target = observation.Units.SingleOrDefault(u => u.Id == strike.TargetId);
            Require(actor.Class == UnitClass.Melee && !actor.Dead && actor.Hex?.Action is UnitActionKind.Windup or UnitActionKind.Recovery,
                "melee connection belongs to a current stationary attack");
            Require(actor.AttackSequence == strike.AttackSequence && actor.ImpactTick == strike.ImpactTick,
                "melee connection uses the observed authoritative action");
            double tick = actor.Hex?.FrozenTick ?? observation.CombatTick;
            Require(tick >= actor.ActionStartTick && tick < strike.ImpactTick + 12,
                "melee connection stays inside its shared action window");
            if (target is not null && target.Destination == actor.Destination)
                Require(Math.Abs(strike.TargetX - target.X) < .02 && Math.Abs(strike.TargetZ - target.Z) < .02,
                    "melee connection follows the actual sampled target position");
            Require(strike.CueStyle == "dashed-intent-local-strike" && strike.IntentDashes == 12 && strike.StrikeRadius <= .35f,
                "melee uses bounded dashed intent and a local strike rather than a spanning beam");
            Require(tick < strike.ImpactTick ? strike.Phase == "intent" && strike.IntentVisible && !strike.StrikeVisible
                : strike.Phase == (strike.AttackLanded == true ? "landed" : "miss") && !strike.IntentVisible && strike.StrikeVisible,
                "melee intention and local strike are separate current-action phases");
            Require(!strike.ImpactVisible || strike.AttackLanded == true && tick >= strike.ImpactTick && target is not null,
                "target-side impact is shown only for an authoritative landed strike");
        }
    }
    private async Task MixedArmy(Child client, Child observer, CancellationToken token, bool farmExists = false, bool towers = false)
    {
        if (!farmExists) await Action(client, "build 0 farm", token);
        await Action(client, "build 1 metalmine", token);
        await Action(client, "build 2 barracks", token);
        if (towers) await TowerOpening(observer, token);
        else { await Action(observer, "build 0 farm", token); await Action(observer, "build 1 metalmine", token); await Action(observer, "build 2 barracks", token); }
        await UiReadyPair(client, observer, token); await UiReadyPair(client, observer, token);
        await Action(client, "build-recovery 3 lumbermill", token);
        await UiReadyPair(client, observer, token);
        await Pick(client, 2, token);
        UiObservation equipment = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(equipment.Targets["Recruit"].Text.Contains("L1", StringComparison.Ordinal)
            && equipment.Targets["Recruit"].CostText.Contains("2 metal", StringComparison.Ordinal)
            && equipment.UpkeepText.Contains("Next:", StringComparison.Ordinal), "source and packed recruitment exposes level, material quote and separate next-battle upkeep");
        await UiSwords(client, 2, 6, token);
        if (towers) await TowerInvestment(observer, token);
        else
        {
            while (Latest(observer).Players.Single(p => p.Id == observer.PlayerId).Resources.TryPay(Latest(observer).Players.Single(p => p.Id == observer.PlayerId).RecruitmentQuotes.Single(q => q.Type == UnitType.Swordsman && q.Level == 1).Cost, out _)) await Action(observer, "recruit 2", token);
        }
        await UiReadyPair(client, observer, token); await UiClear(client, observer, 1, token);
        await Pick(client, 4, token); await ClickAck(client, "ArcheryRange", token);
        await UiReadyPair(client, observer, token); if (towers) await TowerInvestment(observer, token);
        await UiReadyPair(client, observer, token); if (towers) await TowerInvestment(observer, token);
        await Pick(client, 4, token); int food = Latest(client).Players.Single(p => p.Id == client.PlayerId).Food;
        await EnsureFieldRoom(client, token);
        GameEvent ranged = await ClickAck(client, "RecruitRanged", token); CityState army = State(ranged).Players.Single(p => p.Id == client.PlayerId);
        Require(army.Soldiers.Any(u => u.Type == UnitType.Swordsman) && army.Soldiers.Any(u => u.Type == UnitType.Crossbowman) && army.Food == food,
            "ordinary building-specific UI recruitment pays equipment and retains food");
        UiObservation controls = await UiProtocol.Probe(client, options.StartupTimeout, token); UiTarget control = controls.Targets["RecruitRanged"];
        Require(control.Visible && !control.Enabled && control.X > 0 && control.X < controls.Width && control.Y > 0 && control.Y < controls.Height,
            "ranged recruitment remains visible, disabled without equipment and inside the window");
        await UiReadyPair(client, observer, token); if (towers) await TowerInvestment(observer, token);
        if (towers) await Click(client, "City" + observer.PlayerId, token);
        await SimulationSpeed(_scope!.Children.First(c => c.Name.StartsWith("ui-server", StringComparison.Ordinal)), 1, token);
        await UiReadyPair(client, observer, token);
        await Observe(client, state => state.Phase == Phase.Combat && state.Wave == 2, "funded mixed second wave", token);
    }

    private async Task SpecialistArmy(Child client, Child observer, CancellationToken token)
    {
        await Action(client, "build 0 farm", token);
        await Action(client, "build 1 metalmine", token);
        await Action(client, "build 2 barracks", token); await TowerOpening(observer, token);
        await UiReadyPair(client, observer, token, actualInput: false); await UiReadyPair(client, observer, token, actualInput: false);
        await Action(client, "build-recovery 3 lumbermill", token);
        await UiReadyPair(client, observer, token, actualInput: false); await UiSwords(client, 2, 6, token, actualInput: false); await TowerInvestment(observer, token);
        await UiReadyPair(client, observer, token, actualInput: false); await UiClear(client, observer, 1, token);
        await Action(client, "build 4 stonecutter", token);
        for (int production = 0; production < 3; production++) { await UiReadyPair(client, observer, token, actualInput: false); await TowerInvestment(observer, token); }
        await Action(client, "sell 1", token); await Action(client, "build 1 weaver", token);
        await Action(client, "sell 4", token); await Action(client, "build 4 arcanum", token);
        await Pick(client, 2, token); await EnsureFieldRoom(client, token); await ClickAck(client, "RecruitBerserker", token);
        await UiSwords(client, 2, 6, token, actualInput: false); await UiReadyPair(client, observer, token, actualInput: false); await UiClear(client, observer, 2, token);
        await UiReadyPair(client, observer, token, actualInput: false);
        await Pick(client, 4, token); await EnsureFieldRoom(client, token); await ClickAck(client, "RecruitMage", token);
        await Pick(client, 2, token);
        if (!Latest(client).Players.Single(p => p.Id == client.PlayerId).Soldiers.Any(u => u.Type == UnitType.Berserker))
        { await EnsureFieldRoom(client, token); await ClickAck(client, "RecruitBerserker", token); }
        await ClickAck(client, "Sell", token); await ClickAck(client, "ArcheryRange", token); await EnsureFieldRoom(client, token); await ClickAck(client, "RecruitRanged", token);
        await UiReadyPair(client, observer, token, actualInput: false); await UiReadyPair(client, observer, token, actualInput: false);
        UiObservation piles = await UiProtocol.Probe(client, options.StartupTimeout, token); CityState city = Latest(client).Players.Single(c => c.Id == client.PlayerId);
        Require(piles.Stockpiles == new StockpileObservation(PresentationLimits.StockpileCount(city.Gold), PresentationLimits.StockpileCount(city.Food), PresentationLimits.StockpileCount(city.Wood)), "specialist equipment updates exact current stockpile tiers");
        Require(city.Soldiers.Any(u => u.Type == UnitType.Mage) && city.Soldiers.Any(u => u.Type == UnitType.Berserker) && city.Soldiers.Any(u => u.Type == UnitType.Crossbowman), "ordinary three-wave specialist supply chain fields all required friendly roles");
        // The showcase previously ended during this battle. Retain its roles
        // and Catapult while restoring ordinary frontline supply for research's
        // later opening; replacement and recruitment pay the existing quotes.
        await Action(client, "sell 1", token); await Action(client, "build 1 barracks", token);
        await EnsureFieldRoom(client, token, actualInput: false); await Action(client, "recruit 1 berserker", token);
        await EnsureFieldRoom(client, token, actualInput: false); await Action(client, "recruit 1 berserker", token);
        await Action(observer, "sell 4", token); await Action(observer, "build 4 barracks", token);
        await UiSwords(observer, 4, 12, token, actualInput: false);
        await Click(client, "City" + observer.PlayerId, token);
        await SimulationSpeed(_scope!.Children.First(c => c.Name.StartsWith("ui-server", StringComparison.Ordinal)), 1, token);
        await UiReadyPair(client, observer, token, actualInput: false); await Observe(client, state => state.Phase == Phase.Combat && state.Wave == 3, "funded specialist third wave", token);
        long towerAfter = Latest(observer).Tick;
        await Observe(observer, s => s.CombatEvents.Any(e => e.Tick > towerAfter && e.Tower?.Type == Building.CatapultTower && e.Type == CombatEventType.Impact && e.Landed), "real catapult impact", token);
        MatchSnapshot towerPause = State(await Action(observer, "pause", token));
        await Observe(client, s => s.Paused && s.Tick == towerPause.Tick, "tower capture pause barrier", token);
        await WaitUi(client, p => p.Effects.Active > 0 && p.Effects.Bus == "Master", "tower projectiles in observed city", token);
        await Checkpoint(client, "combat-catapult", token);
        await Click(client, "City" + client.PlayerId, token);
    }

    private async Task CombatCheckpoint(Child client, Child observer, CancellationToken token, bool shortCheck = false)
    {
        await SimulationSpeed(_scope!.Children.First(c => c.Name.StartsWith("ui-server", StringComparison.Ordinal)), 1, token);
        // The source tower capture leaves combat paused. Sample its retained
        // route before resuming; pacing/probe round trips must not consume it.
        UiObservation moving = await WaitUi(client, p => p.Units.Any(u => u.Visible && u.Type == UnitType.Swordsman && u.Clip == "walk" && u.WalkingBlend > 0), "actual walking pose", token);
        HealthBars(moving);
        Require(moving.HealthBars.Any(b => b.Visible && b.Fraction == 1) && moving.HealthBars.Any(b => b.Visible && moving.Units.Single(u => u.Id == b.Id).Faction == Faction.Adventurers) && moving.HealthBars.Any(b => b.Visible && moving.Units.Single(u => u.Id == b.Id).Faction == Faction.Skeletons), "full overhead bars on both friendly and enemy models");
        UnitObservation first = moving.Units.First(u => u.Visible && u.Type == UnitType.Swordsman && u.Clip == "walk" && u.WalkingBlend > 0);
        Require(first.Hex is { Action: UnitActionKind.Moving, HoldsTransit: true } && first.Hex.EndTick > first.Hex.StartTick, "walking model follows a declared direct timed step with endpoint reservations");
        if (!shortCheck)
        {
            Require(Latest(client).Paused && moving.PhaseText.Contains("PAUSED", StringComparison.Ordinal),
                "first locomotion witness retains the tower capture pause");
            MatchSnapshot towerResume = State(await Action(observer, "resume", token));
            await Observe(client, s => !s.Paused && s.Revision >= towerResume.Revision, "combat resumes after tower capture", token);
        }
        UiObservation moved = await WaitUi(client, p => p.Units.Any(u => u.Id == first.Id && (u.X != first.X || u.Z != first.Z) && u.BoneRotation != first.BoneRotation), "moving skeleton changes position and pose", token);
        Require(moved.Units.All(u => u.WeaponAttached && !u.InteractionEnabled), "units bind real skeleton weapons without gameplay interaction");
        UiObservation swordPose = shortCheck ? moved : await WaitUi(client, p => p.Units.Any(u => u.Visible && u.Type == UnitType.Swordsman && u.Clip == "attack" && u.AttackActive), "rendered sword attack before inspection", token);
        if (shortCheck) await Action(observer, "pause", token);
        else await ClickAck(client, "Pause", token, swordPose);
        await Checkpoint(client, shortCheck ? "packed-locomotion" : "combat-locomotion", token);
        await UnitInspection(client, shortCheck ? "packed-unit-inspection" : "combat-unit-inspection", token);
        if (shortCheck)
        {
            UiObservation packedWindow = await UiProtocol.Probe(client, options.StartupTimeout, token);
            NativeWindowClose.Resize(client, options, packedWindow.NativeWindow, 1280, 720);
            await WaitUi(client, p => p.Width == 1280 && p.Height == 720, "owned packed window resized to supported 1280x720", token);
            await UnitInspection(client, "packed-unit-inspection-resized", token);
            async Task<MatchSnapshot> PausePackedEvent(Func<MatchSnapshot, bool> predicate, string expectation)
            {
                await Observe(observer, predicate, expectation, token);
                return State(await Action(observer, "pause", token));
            }
            long shootingAfter = Latest(observer).Tick;
            Task<MatchSnapshot> packedShotPause = PausePackedEvent(s => s.Tick > shootingAfter && s.CombatEvents.Any(e => e.Tick > shootingAfter && e.Type == CombatEventType.Impact && e.Unit?.Type == UnitType.Crossbowman), "packed authoritative Crossbowman shot");
            await ClickAck(client, "Pause", token);
            MatchSnapshot packedShot = await packedShotPause;
            await Observe(client, s => s.Paused && s.Tick == packedShot.Tick, "packed shot pause barrier", token);
            await WaitUi(client, p => p.Units.Any(u => u.Type == UnitType.Crossbowman && u.ShotVisible), "packed shooting effect", token, 60000);
            await Checkpoint(client, "packed-shooting", token);
            Task<MatchSnapshot> packedDeathPause = PausePackedEvent(s => s.Tick > packedShot.Tick && s.DyingBodies.Any(u => u.Hex!.DeathStartTick > packedShot.Tick), "packed authoritative casualty");
            await ClickAck(client, "Pause", token);
            MatchSnapshot packedCasualty = await packedDeathPause;
            await Observe(client, s => s.Paused && s.Tick == packedCasualty.Tick, "packed death pause barrier", token);
            UiObservation packedDeath = await WaitUi(client, p => p.Units.Any(u => u.Dead), "packed current death mapping", token);
            RenderedContact(packedDeath);
            Require(packedDeath.Units.All(u => u.Hex is not null) && packedDeath.Units.Where(u => u.Dead).All(u => u.Hex!.Lifecycle == UnitLifecycle.Dying && u.Hex.DeathEndTick > u.Hex.DeathStartTick), "exported models preserve typed shared-hex actions and death intervals");
            await Checkpoint(client, "packed-death", token); return;
        }
        UiObservation frozen = await WaitUi(client, p => p.PhaseText.Contains("PAUSED", StringComparison.Ordinal), "posed pause", token);
        // Two fresh probes separated by an authority barrier; no arbitrary sleep.
        await Action(observer, "unknown", token, false);
        UiObservation still = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(frozen.VisualSeconds == still.VisualSeconds && frozen.CombatTick == still.CombatTick &&
            JsonSerializer.Serialize(frozen.Units) == JsonSerializer.Serialize(still.Units) && JsonSerializer.Serialize(frozen.Strikes) == JsonSerializer.Serialize(still.Strikes) && JsonSerializer.Serialize(frozen.HealthBars) == JsonSerializer.Serialize(still.HealthBars) && frozen.Effects.Voices == 0 && still.Effects.Voices == 0 && frozen.Effects.Positions.SequenceEqual(still.Effects.Positions) && frozen.AmbientAngles.SequenceEqual(still.AmbientAngles), "pause freezes positions, skeleton poses, shots, strike connections and death clocks");
        await Checkpoint(client, "combat-paused", token);
        // Two input actions in the existing paused battle catch stale hidden rig
        // poses and historical cue replay without another expensive battle.
        await Click(client, "NextCity", token);
        await WaitUi(client, p => p.ObservedCity != frozen.ObservedCity, "hide paused combat city", token);
        await Click(client, "PreviousCity", token);
        UiObservation returned = await WaitUi(client, p => p.ObservedCity == frozen.ObservedCity, "return to paused combat city", token);
        Require(returned.VisualSeconds == frozen.VisualSeconds && returned.CombatTick == frozen.CombatTick
            && returned.Effects.CueCount == frozen.Effects.CueCount && returned.Effects.Voices == 0
            && returned.Units.Where(u => u.Visible).Select(u => u.Id).Order().SequenceEqual(frozen.Units.Where(u => u.Visible).Select(u => u.Id).Order())
            && returned.Units.Where(u => u.Visible).All(u => frozen.Units.Any(old => old.Id == u.Id && old.Clip == u.Clip
                && old.PoseSeconds == u.PoseSeconds && old.X == u.X && old.Z == u.Z && old.BoneRotation == u.BoneRotation)),
            "focus return seeks current paused visible pose without historical audio or attacks");
        // The briefly visible other city's rigs have now been sought too. Use
        // this settled observation for the later all-rig resize comparison.
        frozen = returned;
        await CameraFrozenBars(client, token);
        await Click(client, "Settings", token);
        await WaitUi(client, p => p.SettingsOpen, "settings open over paused battle", token);
        UiObservation resized = await ResizeTo1280(client, token);
        HealthBars(resized);
        Require(resized.VisualSeconds == frozen.VisualSeconds && JsonSerializer.Serialize(resized.Units) == JsonSerializer.Serialize(frozen.Units)
            && resized.HealthBars.Any(b => b.Visible && b.X != frozen.HealthBars.Single(old => old.Id == b.Id).X), "resizing reprojects health bars while authoritative health and paused unit poses stay frozen");
        await Click(client, "CloseSettings", token);
        await WaitUi(client, p => !p.SettingsOpen, "return to paused battle after resizing", token);
        await UnitInspection(client, "combat-unit-inspection-resized", token);
        await Checkpoint(client, "combat-resized", token);
        await OpenUnitInspector(client, token);
        // Validate clip provenance while the authority is still frozen. Cold asset
        // I/O must not consume the fresh-body window after the resume below.
        _ = CasualtyClips.Value;
        long priorPause = AckSequence(client);
        long casualtyAfter = Latest(observer).Tick;
        using var casualtyDeadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        casualtyDeadline.CancelAfter(options.Timeout);
        var casualtyTrace = new List<object>();
        async Task<GameEvent> CasualtyAction(string command, CancellationToken cancellation)
        {
            MatchSnapshot quote = Latest(observer);
            casualtyTrace.Add(new { Stage = "request", Command = command, State = quote });
            Require(command != "pause" || !quote.Paused, "casualty driver never initiates an already-owned pause");
            GameEvent receipt = await Action(observer, command, cancellation);
            casualtyTrace.Add(new { Stage = "receipt", Command = command, Receipt = receipt });
            return receipt;
        }
        Task<MatchSnapshot> casualtyPause = CasualtyAdmission.Pause(Latest(observer), client.PlayerId, casualtyAfter, AckSequence(observer),
            () => Latest(observer),
            async (floor, cancellation) =>
            {
                await observer.WaitFor(e => e.State is { } state && state.Revision > floor,
                    "new current casualty admission revision", options.Timeout, cancellation);
                casualtyTrace.Add(new { Stage = "current", Floor = floor, State = Latest(observer) });
            }, CasualtyAction, casualtyDeadline.Token);
        MatchSnapshot retainedDeath;
        try
        {
            await ClickAck(client, "Pause", token);
            UiObservation outsidePause = await UiProtocol.Probe(client, options.StartupTimeout, token);
            Require(outsidePause.InspectedUnit is null && AckSequence(client) == priorPause + 1, "outside Pause dismisses inspection and executes once");
            retainedDeath = await casualtyPause;
        }
        finally
        {
            await casualtyDeadline.CancelAsync();
            try { await casualtyPause; } catch when (casualtyDeadline.IsCancellationRequested) { }
            await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "combat-casualty-admission.json"),
                JsonSerializer.Serialize(new { City = client.PlayerId, AfterTick = casualtyAfter, Attempts = CasualtyAdmission.MaximumAttempts, Events = casualtyTrace }, Evidence.JsonOptions));
        }
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "combat-casualty-pause.json"),
            JsonSerializer.Serialize(retainedDeath, Evidence.JsonOptions), token);
        int dead = EligibleCasualties(retainedDeath, client.PlayerId, casualtyAfter)
            .MaxBy(u => u.Hex!.DeathStartTick)!.Id;
        await Action(client, "unknown", token, false);
        UiObservation casualty = await WaitUi(client, p => p.CombatTick == retainedDeath.Tick && p.PhaseText.Contains("PAUSED", StringComparison.Ordinal)
            && p.Units.Any(u => u.Id == dead && u.Dead && u.Visible && FreshDeathPose(u.PoseSeconds)), "same paused authoritative casualty rendered freshly", token);
        CasualtyAdmission.Frame(retainedDeath, Latest(client), casualty, client.PlayerId, casualtyAfter, dead);
        Require(!casualty.HealthBars.Any(b => b.Id == dead), "death immediately removes overhead bar");
        Require(!CombatPlayback.All(Latest(client)).Any(u => u.Id == dead), "death visual is absent from living combat state");
        Require(retainedDeath.DyingBodies.Any(u => u.Id == dead), "authoritative casualty pause retains the sampled death");
        UiObservation deathPaused = await WaitUi(client, p => p.PhaseText.Contains("PAUSED", StringComparison.Ordinal) && p.Units.Any(u => u.Id == dead && u.Dead), "paused death remains", token);
        await Action(observer, "unknown", token, false);
        UiObservation deathStill = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(deathStill.VisualSeconds == deathPaused.VisualSeconds && JsonSerializer.Serialize(deathPaused.Units) == JsonSerializer.Serialize(deathStill.Units) && JsonSerializer.Serialize(deathPaused.HealthBars) == JsonSerializer.Serialize(deathStill.HealthBars), "death pose and cleanup freeze on shared pause");
        await Checkpoint(client, "combat-casualty", token, validate: frame =>
            CasualtyAdmission.Frame(retainedDeath, Latest(client), frame, client.PlayerId, casualtyAfter, dead));
        // Select from the frozen current battle, not a low-health target that can
        // die between the live probe and native click. Resume through the ordinary
        // observer command so the already-tested outside Pause click does not dismiss it.
        UiObservation liveTarget = await WaitUi(client, p => p.Units.Any(u => u.Visible && !u.Dead && u.Faction == Faction.Skeletons && u.Health < u.MaximumHealth), "currently damaged opponent for live inspection", token);
        UnitObservation damagedUnit = liveTarget.Units.Where(u => u.Visible && !u.Dead && u.Faction == Faction.Skeletons && u.Health < u.MaximumHealth).OrderBy(u => u.Health).ThenBy(u => u.Id).First();
        await ClickPoint(client, liveTarget.Targets["Unit" + damagedUnit.Id]);
        UiObservation liveInspection = await WaitUi(client, p => p.InspectedUnit?.Id == damagedUnit.Id, "live damaged-unit inspector opens", token);
        InspectorObservation liveUnit = liveInspection.InspectedUnit!;
        await Action(observer, "resume", token);
        await Action(client, "unknown", token, false);
        Require(!Latest(client).Paused, "live inspection resumes without dismissal");
        long deathEnd = deathPaused.Units.Single(u => u.Id == dead).Hex!.DeathEndTick;
        UiObservation cleaned = await WaitUi(client, p =>
        {
            bool present = p.Units.Any(u => u.Id == dead);
            Require(present == (p.CombatTick < deathEnd), "death model lifetime agrees with the declared tick boundary");
            return !present;
        }, "declared death cleanup", token);
        MatchSnapshot deathReleased = await Observe(observer, s => s.Tick >= deathEnd && s.DyingBodies.All(u => u.Id != dead), "authoritative death reservation release", token);
        CombatContact(deathReleased);
        DeathCleanupObservation cleanup = UiProtocol.DeathCleanup(cleaned, dead, deathEnd, casualty.VisualSeconds);
        Require(cleanup.VisualSeconds - casualty.VisualSeconds <= 2, "death view frees within two unpaused seconds");
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "combat-death-cleanup.json"),
            JsonSerializer.Serialize(new
            {
                Unit = dead,
                PausedVisualSeconds = casualty.VisualSeconds,
                Removal = cleanup,
                ProbeTick = cleaned.CombatTick,
                ProbeVisualSeconds = cleaned.VisualSeconds
            }, Evidence.JsonOptions), token);
        bool inspectionChanged = false;
        // Retain actual frames already witnessed during movement, pause and
        // casualty capture while observing live inspection and recovery.
        UiObservation[] retainedFrames = [moving, moved, swordPose, frozen, casualty, deathPaused, deathStill, cleaned];
        UnitObservation[] witnessed = retainedFrames.SelectMany(p => p.Units).Where(u => u.Visible).ToArray();
        bool sword = witnessed.Any(u => AuthoredAttack(u, UnitType.Swordsman));
        bool shot = witnessed.Any(u => AuthoredAttack(u, UnitType.Mage));
        bool hit = witnessed.Any(u => u.Clip == "hit" && u.HitActive);
        bool axe = witnessed.Any(u => AuthoredAttack(u, UnitType.Berserker));
        bool damagedBar = frozen.HealthBars.Any(b => b.Visible && b.Fraction > 0 && b.Fraction < 1), recovery = false;
        var priorRecovery = new Dictionary<int, (UnitObservation Unit, CombatPoseWitness Witness)>();
        var poseDiagnostic = new CombatPoseDiagnostic();
        CombatPoseFlags Flags() => new(sword, shot, axe, hit, damagedBar, recovery, inspectionChanged);
        void AttackWitness(string flag, UiObservation frame, UnitType type, string source, MatchSnapshot? received = null)
            => poseDiagnostic.First(flag, frame.Units.FirstOrDefault(u => AuthoredAttack(u, type)) is { } unit
                ? CombatPoseDiagnostic.Witness(frame, unit, source, received) : null);
        foreach (UiObservation frame in retainedFrames)
        {
            AttackWitness("sword", frame, UnitType.Swordsman, "retained-entry-frame");
            AttackWitness("Mage", frame, UnitType.Mage, "retained-entry-frame");
            AttackWitness("axe", frame, UnitType.Berserker, "retained-entry-frame");
            if (frame.Units.FirstOrDefault(u => u.Visible && u.Clip == "hit" && u.HitActive) is { } unit)
                poseDiagnostic.First("hit", CombatPoseDiagnostic.Witness(frame, unit, "retained-entry-frame"));
        }
        if (frozen.HealthBars.FirstOrDefault(b => b.Visible && b.Fraction > 0 && b.Fraction < 1) is { } entryBar)
            poseDiagnostic.First("damagedBar", CombatPoseDiagnostic.Witness(frozen,
                frozen.Units.FirstOrDefault(u => u.Id == entryBar.Id), "retained-entry-frame"));
        poseDiagnostic.Begin(Flags());
        bool posePassed = false;
        try
        {
            await WaitUi(client, p =>
            {
                MatchSnapshot received = Latest(client);
                poseDiagnostic.Poll(p, received);
                RenderedContact(p); HealthBars(p);
                UnitState? observed = CombatPlayback.All(Latest(client)).FirstOrDefault(unit => unit.Id == liveUnit.Id);
                if (p.InspectedUnit is { } inspected && inspected.Id == liveUnit.Id)
                {
                    inspectionChanged |= inspected.Health != liveUnit.Health;
                    if (p.Revision == Latest(client).Revision && observed is not null)
                        Require(inspected.Health == observed.Health && inspected.MaximumHealth == observed.Profile.Health && inspected.Damage == observed.Profile.Damage, "live inspector follows authoritative damage and resolved stats");
                }
                else if (observed is null || observed.Health <= 0) { Require(p.InspectedUnit is null, "selected casualty removes its inspection panel"); inspectionChanged = true; }
                if (inspectionChanged)
                    poseDiagnostic.First("inspectionChanged", CombatPoseDiagnostic.Witness(p,
                        p.Units.FirstOrDefault(u => u.Id == liveUnit.Id), "current-poll:inspection-target-" + liveUnit.Id, received));
                damagedBar |= p.HealthBars.Any(b => b.Visible && b.Fraction > 0 && b.Fraction < 1);
                if (p.HealthBars.FirstOrDefault(b => b.Visible && b.Fraction > 0 && b.Fraction < 1) is { } bar)
                    poseDiagnostic.First("damagedBar", CombatPoseDiagnostic.Witness(p,
                        p.Units.FirstOrDefault(u => u.Id == bar.Id), "current-poll", received));
                foreach (UnitObservation unit in p.Units.Where(u => !u.Dead && u.Hex?.Action == UnitActionKind.Recovery))
                {
                    if (priorRecovery.TryGetValue(unit.Id, out var previous) && previous.Unit.ReadyTick == unit.ReadyTick)
                    {
                        if (unit.X != previous.Unit.X || unit.Z != previous.Unit.Z) throw new InvalidOperationException("Recovery moved away from its declared hex anchor.");
                        recovery = true;
                        poseDiagnostic.Recovery(previous.Witness, CombatPoseDiagnostic.Witness(p, unit, "current-poll", received));
                    }
                    priorRecovery[unit.Id] = (unit, CombatPoseDiagnostic.Witness(p, unit, "current-poll", received));
                }
                foreach (UnitObservation ranged in p.Units.Where(u => u.Type == UnitType.Crossbowman))
                    if (ranged.ShotCount > ranged.AttackSequence) throw new InvalidOperationException("Repeated snapshots duplicated a shot.");
                foreach (UnitObservation unit in p.Units.Where(u => u.Visible))
                {
                    if (unit.Clip == "hit" && !unit.HitActive) throw new InvalidOperationException("Declared hit has no active animation layer.");
                    if (unit.Clip == "attack" && !unit.AttackActive)
                        throw new InvalidOperationException("Declared attack has no active animation one-shot.");
                }
                sword |= p.Units.Any(u => AuthoredAttack(u, UnitType.Swordsman));
                shot |= p.Units.Any(u => AuthoredAttack(u, UnitType.Mage));
                axe |= p.Units.Any(u => AuthoredAttack(u, UnitType.Berserker));
                hit |= p.Units.Any(u => u.Visible && u.Clip == "hit");
                AttackWitness("sword", p, UnitType.Swordsman, "current-poll", received);
                AttackWitness("Mage", p, UnitType.Mage, "current-poll", received);
                AttackWitness("axe", p, UnitType.Berserker, "current-poll", received);
                if (p.Units.FirstOrDefault(u => u.Visible && u.Clip == "hit") is { } hitUnit)
                    poseDiagnostic.First("hit", CombatPoseDiagnostic.Witness(p, hitUnit, "current-poll", received));
                return sword && shot && hit && axe && damagedBar && recovery && inspectionChanged;
            }, "sword/axe, Mage cast, hit and recovery poses", token, 60000);
            posePassed = true;
        }
        finally
        {
            File.WriteAllText(Path.Combine(_scope!.EvidenceDirectory, "combat-pose-proof.json"),
                poseDiagnostic.Finish(Flags(), posePassed ? "passed" : "failed-or-cancelled"));
            if (!posePassed)
            {
                client.DumpEvidence("Default combat pose proof failed");
                observer.DumpEvidence("Default combat pose proof failed");
                Child authority = _scope.Children.Last(c => c.Name.StartsWith("ui-server", StringComparison.Ordinal) && !c.ExpectedFailure);
                authority.DumpEvidence("Default combat pose proof failed");
                CombatFailureEvidence.Collect(_scope.EvidenceDirectory, client.LogPath + ".states.json",
                    observer.LogPath + ".states.json", authority.LogPath + ".states.json");
            }
        }
        Require(inspectionChanged, "live inspection updates damage or closes on its casualty");
        Require(sword && shot && hit && axe && casualty.Effects.Active <= 64 && casualty.Effects.Voices <= 8, "rendered sword/axe/cast/hit states and bounded effects sampled from live nodes");
        Require(damagedBar, "authoritative damage visibly reduces a living health bar");
        await ResearchCheckpoint(client, observer, token);
        await CameraZoom(client, token);
        await Click(client, "ReturnToMenu", token);
        await Click(client, "ConfirmReturn", token);
        await WaitUi(client, p => p.Screen == "menu", "combat return cleanup", token);
        await Click(client, "Singleplayer", token);
        UiObservation fresh = await WaitUi(client, p => p.Screen == "session" && p.PhaseText.Contains("Building", StringComparison.Ordinal)
            && p.HudHeight >= 179 && p.HudHeight <= 190 && LandscapeChecks.Covered(p.Landscape), "fresh solo layout after combat", token);
        RequireOverview(fresh);
        Require(fresh.Units.Length == 0 && fresh.DeathCleanups.Length == 0 && fresh.HealthBars.Length == 0 && fresh.EventCursor == 0 && Latest(client).MatchId != Latest(observer).MatchId,
            "fresh match clears living/dead views, cleanup witnesses, events and old authority identity");
        await Checkpoint(client, "combat-fresh-session", token);
    }
}
