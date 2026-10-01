using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private static void RenderedContact(UiObservation observation)
    {
        UnitObservation[] units = observation.Units.Where(u => u.Visible && !u.Dead).ToArray();
        foreach (UnitObservation unit in units)
            foreach (UnitObservation other in units.Where(u => u.Id > unit.Id && u.Destination == unit.Destination))
                if (Math.Sqrt(Math.Pow(unit.X - other.X, 2) + Math.Pow(unit.Z - other.Z, 2)) < 0.4 - 1e-4)
                    throw new InvalidOperationException("Rendered bodies crossed contact: " + unit.Id + "/" + other.Id);
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
        await Click(client, "City" + observer.PlayerId, token);
        await WaitUi(client, p => p.Effects.Active > 0 && p.Effects.Bus == "Master", "tower projectiles in observed city", token);
        await Observe(client, s => s.CombatEvents.Any(e => e.Tower?.Type == Building.CatapultTower && e.Type == CombatEventType.Impact && e.Landed), "real catapult impact", token);
        await Checkpoint(client, "combat-catapult", token);
        await Click(client, "City" + client.PlayerId, token);
    }

    private async Task CombatCheckpoint(Child client, Child observer, CancellationToken token, bool shortCheck = false)
    {
        UiObservation moving = await WaitUi(client, p => p.Units.Any(u => u.Type == UnitType.Swordsman && u.Clip == "Running_A"), "actual locomotion pose", token);
        HealthBars(moving);
        Require(moving.HealthBars.Any(b => b.Visible && b.Fraction == 1) && moving.HealthBars.Any(b => b.Visible && moving.Units.Single(u => u.Id == b.Id).Faction == Faction.Adventurers) && moving.HealthBars.Any(b => b.Visible && moving.Units.Single(u => u.Id == b.Id).Faction == Faction.Skeletons), "full overhead bars on both friendly and enemy models");
        UnitObservation first = moving.Units.First(u => u.Type == UnitType.Swordsman);
        UiObservation moved = await WaitUi(client, p => p.Units.Any(u => u.Id == first.Id && u.Z != first.Z && u.BoneRotation != first.BoneRotation), "moving skeleton changes position and pose", token);
        Require(moved.Units.All(u => u.WeaponAttached && !u.InteractionEnabled), "units bind real skeleton weapons without gameplay interaction");
        await Checkpoint(client, shortCheck ? "packed-locomotion" : "combat-locomotion", token);
        if (shortCheck)
        {
            await WaitUi(client, p => p.Units.Any(u => u.Type == UnitType.Crossbowman && u.ShotVisible), "packed shooting effect", token, 60000);
            await Checkpoint(client, "packed-shooting", token); return;
        }
        await Observe(client, s => s.Players.Single(p => p.Id == client.PlayerId).Soldiers.Any(u => u.PendingImpact), "pending first attack", token);
        await ClickAck(client, "Pause", token);
        UiObservation frozen = await WaitUi(client, p => p.PhaseText.Contains("PAUSED", StringComparison.Ordinal), "posed pause", token);
        // Two fresh probes separated by an authority barrier; no arbitrary sleep.
        await Action(observer, "unknown", token, false);
        UiObservation still = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(frozen.VisualSeconds == still.VisualSeconds && frozen.CombatTick == still.CombatTick &&
            JsonSerializer.Serialize(frozen.Units) == JsonSerializer.Serialize(still.Units) && JsonSerializer.Serialize(frozen.HealthBars) == JsonSerializer.Serialize(still.HealthBars) && frozen.Effects.Voices == 0 && still.Effects.Voices == 0 && frozen.Effects.Positions.SequenceEqual(still.Effects.Positions) && frozen.AmbientAngles.SequenceEqual(still.AmbientAngles), "pause freezes positions, skeleton poses, shots and death clocks");
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
        bool sword = false, shot = false, hit = false, axe = false, damagedBar = false;
        UiObservation casualty = await WaitUi(client, p =>
        {
            RenderedContact(p); HealthBars(p);
            damagedBar |= p.HealthBars.Any(b => b.Visible && b.Fraction > 0 && b.Fraction < 1);
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
            return sword && shot && hit && axe && damagedBar && p.Units.Any(u => u.Dead && u.PoseSeconds < .35);
        }, "skeleton sword/axe, Mage cast, hit and first death poses", token, 60000);
        Require(sword && shot && hit && axe && casualty.Effects.Active <= 64 && casualty.Effects.Voices <= 8, "rendered sword/axe/cast/hit states and bounded effects sampled from live nodes");
        Require(damagedBar, "authoritative damage visibly reduces a living health bar");
        // Choose the fresh casualty, rather than an older corpse approaching cleanup.
        int dead = casualty.Units.Where(u => u.Dead).MinBy(u => u.PoseSeconds)!.Id;
        Require(!casualty.HealthBars.Any(b => b.Id == dead), "death immediately removes overhead bar");
        Require(!CombatPlayback.All(Latest(client)).Any(u => u.Id == dead), "death visual is absent from living combat state");
        await ClickAck(client, "Pause", token, casualty);
        UiObservation deathPaused = await WaitUi(client, p => p.PhaseText.Contains("PAUSED", StringComparison.Ordinal) && p.Units.Any(u => u.Id == dead && u.Dead), "paused death remains", token);
        await Action(observer, "unknown", token, false);
        UiObservation deathStill = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(deathStill.VisualSeconds == deathPaused.VisualSeconds && JsonSerializer.Serialize(deathPaused.Units) == JsonSerializer.Serialize(deathStill.Units) && JsonSerializer.Serialize(deathPaused.HealthBars) == JsonSerializer.Serialize(deathStill.HealthBars), "death pose and cleanup freeze on shared pause");
        await Checkpoint(client, "combat-casualty", token);
        await ClickAck(client, "Pause", token);
        UiObservation cleaned = await WaitUi(client, p => p.Units.All(u => u.Id != dead), "bounded death cleanup", token);
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
