using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    internal static void RenderedContact(UiObservation observation)
    {
        UnitObservation[] units = observation.Units.Where(u => u.Visible && u.Deployed).ToArray();
        foreach (UnitObservation unit in units)
            foreach (UnitObservation other in units.Where(u => u.Id > unit.Id && u.Destination == unit.Destination))
                if (Math.Sqrt(Math.Pow(unit.X - other.X, 2) + Math.Pow(unit.Z - other.Z, 2)) < 0.4 - 1e-4)
                    throw new InvalidOperationException("Rendered bodies crossed contact: " + unit.Id + "/" + other.Id);
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
            Require(!strike.ImpactVisible || strike.AttackLanded == true && tick >= strike.ImpactTick && target is not null,
                "target-side impact is shown only for an authoritative landed strike");
        }
    }
    private async Task MixedArmy(Child client, Child observer, CancellationToken token, bool farmExists = false, bool towers = false)
    {
        if (!farmExists) { await Pick(client, 0, token); await ClickAck(client, "Farm", token); }
        await Pick(client, 1, token); await ClickAck(client, "Barracks", token);
        if (towers) { await Action(observer, "build 0 catapulttower", token); await Action(observer, "build 1 lumbermill", token); }
        await Action(client, "ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Turn == 2, "first production", token);
        if (towers) await Action(observer, "build 2 arrowtower", token);
        await Pick(client, 2, token); await ClickAck(client, "ArcheryRange", token);
        await Pick(client, 1, token);
        GameEvent sword = await ClickAck(client, "Recruit", token);
        Require(State(sword).Players.Single(p => p.Id == client.PlayerId).Food == 0, "Swordsman cost comes from authoritative catalog");
        await Action(client, "ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Turn == 3, "second production", token);
        await Pick(client, 2, token);
        GameEvent ranged = await ClickAck(client, "RecruitRanged", token);
        CityState army = State(ranged).Players.Single(p => p.Id == client.PlayerId);
        Require(army.Soldiers.Select(u => u.Type).SequenceEqual([UnitType.Swordsman, UnitType.Crossbowman]) && army.Food == 0,
            "ordinary building-specific UI recruits melee/ranged for food exactly once");
        UiObservation controls = await UiProtocol.Probe(client, options.StartupTimeout, token);
        UiTarget control = controls.Targets["RecruitRanged"];
        Require(control.Visible && !control.Enabled && control.X > 0 && control.X < controls.Width && control.Y > 0 && control.Y < controls.Height,
            "Ranged recruitment remains visible, disabled without food and inside the window");
        for (int stage = 0; stage < 2; stage++)
        {
            await Action(client, "ready", token);
            MatchSnapshot resolved = State(await Action(observer, "ready", token));
            await Observe(client, s => s.Revision >= resolved.Revision && s.TurnSerial == resolved.TurnSerial && s.Phase == resolved.Phase, "mixed stage synchronization", token);
        }
        await Observe(client, s => s.Phase == Phase.Combat, "mixed first wave", token);
    }

    private async Task SpecialistArmy(Child client, Child observer, CancellationToken token)
    {
        await Pick(client, 0, token); await ClickAck(client, "Farm", token);
        await Pick(client, 1, token); await ClickAck(client, "Arcanum", token);
        await Action(observer, "build 0 catapulttower", token); await Action(observer, "build 1 lumbermill", token);
        await Action(client, "ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Turn == 2, "specialist first production", token);
        await Pick(client, 2, token); await ClickAck(client, "Lumbermill", token);
        await Action(observer, "build 2 arrowtower", token);
        await Action(client, "ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Turn == 3, "specialist second production", token);
        await Pick(client, 1, token); await ClickAck(client, "RecruitMage", token);
        await Action(client, "ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Phase == Phase.Preparation, "specialist preparation", token);
        await Pick(client, 3, token); await ClickAck(client, "Barracks", token);
        await Pick(client, 3, token); await ClickAck(client, "RecruitBerserker", token);
        UiObservation piles = await UiProtocol.Probe(client, options.StartupTimeout, token);
        CityState city = Latest(client).Players.Single(c => c.Id == client.PlayerId);
        Require(piles.Stockpiles == new StockpileObservation(PresentationLimits.StockpileCount(city.Gold), PresentationLimits.StockpileCount(city.Food), PresentationLimits.StockpileCount(city.Wood)), "specialist spending updates exact current stockpile tiers");
        await Action(client, "ready", token); await Action(observer, "ready", token);
        await Observe(client, s => s.Phase == Phase.Combat, "specialist first wave", token);
        async Task<MatchSnapshot> PauseTowerImpact()
        {
            await Observe(observer, s => s.CombatEvents.Any(e => e.Tower?.Type == Building.CatapultTower && e.Type == CombatEventType.Impact && e.Landed), "real catapult impact", token);
            return State(await Action(observer, "pause", token));
        }
        Task<MatchSnapshot> pauseTower = PauseTowerImpact();
        await Click(client, "City" + observer.PlayerId, token);
        MatchSnapshot towerPause = await pauseTower;
        await Observe(client, s => s.Paused && s.Tick == towerPause.Tick, "tower capture pause barrier", token);
        await WaitUi(client, p => p.Effects.Active > 0 && p.Effects.Bus == "Master", "tower projectiles in observed city", token);
        await Checkpoint(client, "combat-catapult", token);
        await Click(client, "City" + client.PlayerId, token);
        MatchSnapshot towerResume = State(await Action(observer, "resume", token));
        await Observe(client, s => !s.Paused && s.Revision >= towerResume.Revision, "combat resumes after tower capture", token);
    }

    private async Task CombatCheckpoint(Child client, Child observer, CancellationToken token, bool shortCheck = false)
    {
        UiObservation moving = await WaitUi(client, p => p.Units.Any(u => u.Type == UnitType.Swordsman && u.Clip == "Running_A"), "actual locomotion pose", token);
        HealthBars(moving);
        Require(moving.HealthBars.Any(b => b.Visible && b.Fraction == 1) && moving.HealthBars.Any(b => b.Visible && moving.Units.Single(u => u.Id == b.Id).Faction == Faction.Adventurers) && moving.HealthBars.Any(b => b.Visible && moving.Units.Single(u => u.Id == b.Id).Faction == Faction.Skeletons), "full overhead bars on both friendly and enemy models");
        UnitObservation first = moving.Units.First(u => u.Type == UnitType.Swordsman && u.Clip == "Running_A");
        Require(first.Hex is { Action: UnitActionKind.Moving, HoldsTransit: true } && first.Hex.EndTick > first.Hex.StartTick, "running model follows a declared timed route with endpoint reservations");
        UiObservation moved = await WaitUi(client, p => p.Units.Any(u => u.Id == first.Id && (u.X != first.X || u.Z != first.Z) && u.BoneRotation != first.BoneRotation), "moving skeleton changes position and pose", token);
        Require(moved.Units.All(u => u.WeaponAttached && !u.InteractionEnabled), "units bind real skeleton weapons without gameplay interaction");
        if (shortCheck) await Action(observer, "pause", token);
        else await ClickAck(client, "Pause", token, moved);
        await Checkpoint(client, shortCheck ? "packed-locomotion" : "combat-locomotion", token);
        if (shortCheck)
        {
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
        await CameraFrozenBars(client, token);
        await Click(client, "Settings", token);
        await WaitUi(client, p => p.SettingsOpen, "settings open over paused battle", token);
        UiObservation resized = await ResizeTo1280(client, token);
        HealthBars(resized);
        Require(resized.VisualSeconds == frozen.VisualSeconds && JsonSerializer.Serialize(resized.Units) == JsonSerializer.Serialize(frozen.Units)
            && resized.HealthBars.Any(b => b.Visible && b.X != frozen.HealthBars.Single(old => old.Id == b.Id).X), "resizing reprojects health bars while authoritative health and paused unit poses stay frozen");
        await Click(client, "CloseSettings", token);
        await WaitUi(client, p => !p.SettingsOpen, "return to paused battle after resizing", token);
        await Checkpoint(client, "combat-resized", token);
        await ClickAck(client, "Pause", token);
        async Task<MatchSnapshot> PauseFreshDeath()
        {
            await Observe(observer, s => s.DyingBodies.Any(u => u.Hex!.DeathStartTick >= frozen.CombatTick + 30 && u.Hex.DeathEndTick > s.Tick + 12), "new authoritative casualty after attack sampling", token);
            return State(await Action(observer, "pause", token));
        }
        Task<MatchSnapshot> pauseDeath = PauseFreshDeath();
        bool sword = false, shot = false, hit = false, axe = false, damagedBar = false, recovery = false;
        var priorRecovery = new Dictionary<int, UnitObservation>();
        UiObservation casualty = await WaitUi(client, p =>
        {
            RenderedContact(p); HealthBars(p);
            damagedBar |= p.HealthBars.Any(b => b.Visible && b.Fraction > 0 && b.Fraction < 1);
            foreach (UnitObservation unit in p.Units.Where(u => !u.Dead && u.Hex?.Action == UnitActionKind.Recovery))
            {
                if (priorRecovery.TryGetValue(unit.Id, out UnitObservation? previous) && previous.ReadyTick == unit.ReadyTick)
                {
                    Require(unit.X == previous.X && unit.Z == previous.Z, "recovery remains stationary at the declared footprint");
                    recovery = true;
                }
                priorRecovery[unit.Id] = unit;
            }
            foreach (UnitObservation ranged in p.Units.Where(u => u.Type == UnitType.Crossbowman))
                if (ranged.ShotCount > ranged.AttackSequence) throw new InvalidOperationException("Repeated snapshots duplicated a shot.");
            foreach (UnitObservation unit in p.Units)
            {
                if (unit.Clip == "Hit_A" && !unit.HitActive) throw new InvalidOperationException("Declared hit has no active animation layer.");
                if (unit.Clip is "1H_Melee_Attack_Slice_Horizontal" or "2H_Ranged_Shoot" && !unit.AttackActive)
                    throw new InvalidOperationException("Declared attack has no active animation one-shot.");
            }
            sword |= p.Units.Any(u => u.Type == UnitType.Swordsman && u.Clip == "1H_Melee_Attack_Slice_Horizontal");
            shot |= p.Units.Any(u => u.Type == UnitType.Mage && u.Clip == "Spellcast_Shoot" && u.AttackActive);
            axe |= p.Units.Any(u => u.Type == UnitType.Berserker && u.Clip == "2H_Melee_Attack_Chop" && u.AttackActive);
            hit |= p.Units.Any(u => u.Clip == "Hit_A");
            return sword && shot && hit && axe && damagedBar && recovery && p.Units.Any(u => u.Dead && u.PoseSeconds < .35);
        }, "skeleton sword/axe, Mage cast, hit and first death poses", token, 60000);
        Require(sword && shot && hit && axe && casualty.Effects.Active <= 64 && casualty.Effects.Voices <= 8, "rendered sword/axe/cast/hit states and bounded effects sampled from live nodes");
        Require(damagedBar, "authoritative damage visibly reduces a living health bar");
        // Choose the fresh casualty, rather than an older corpse approaching cleanup.
        int dead = casualty.Units.Where(u => u.Dead).MinBy(u => u.PoseSeconds)!.Id;
        Require(!casualty.HealthBars.Any(b => b.Id == dead), "death immediately removes overhead bar");
        Require(!CombatPlayback.All(Latest(client)).Any(u => u.Id == dead), "death visual is absent from living combat state");
        MatchSnapshot retainedDeath = await pauseDeath;
        Require(retainedDeath.DyingBodies.Any(u => u.Id == dead), "authoritative casualty pause retains the sampled death");
        UiObservation deathPaused = await WaitUi(client, p => p.PhaseText.Contains("PAUSED", StringComparison.Ordinal) && p.Units.Any(u => u.Id == dead && u.Dead), "paused death remains", token);
        await Action(observer, "unknown", token, false);
        UiObservation deathStill = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(deathStill.VisualSeconds == deathPaused.VisualSeconds && JsonSerializer.Serialize(deathPaused.Units) == JsonSerializer.Serialize(deathStill.Units) && JsonSerializer.Serialize(deathPaused.HealthBars) == JsonSerializer.Serialize(deathStill.HealthBars), "death pose and cleanup freeze on shared pause");
        await Checkpoint(client, "combat-casualty", token);
        await ClickAck(client, "Pause", token);
        long deathEnd = deathPaused.Units.Single(u => u.Id == dead).Hex!.DeathEndTick;
        UiObservation cleaned = await WaitUi(client, p =>
        {
            bool present = p.Units.Any(u => u.Id == dead);
            Require(present == (p.CombatTick < deathEnd), "death model lifetime agrees with the declared tick boundary");
            return !present;
        }, "declared death cleanup", token);
        MatchSnapshot deathReleased = await Observe(observer, s => s.Tick >= deathEnd && s.DyingBodies.All(u => u.Id != dead), "authoritative death reservation release", token);
        CombatContact(deathReleased);
        Require(cleaned.VisualSeconds - casualty.VisualSeconds <= 2, "death view frees within two unpaused seconds");
        await CameraZoom(client, token);
        await Click(client, "ReturnToMenu", token);
        await WaitUi(client, p => p.Screen == "menu", "combat return cleanup", token);
        await Click(client, "Singleplayer", token);
        UiObservation fresh = await WaitUi(client, p => p.Screen == "session" && p.PhaseText.Contains("Building", StringComparison.Ordinal), "fresh solo after combat", token);
        RequireOverview(fresh);
        Require(fresh.Units.Length == 0 && fresh.HealthBars.Length == 0 && fresh.EventCursor == 0 && Latest(client).MatchId != Latest(observer).MatchId,
            "fresh match clears living/dead views, events and old authority identity");
        await Checkpoint(client, "combat-fresh-session", token);
    }
}
