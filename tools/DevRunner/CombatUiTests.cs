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
    private async Task MixedArmy(Child client, Child observer, CancellationToken token, bool farmExists = false)
    {
        if (!farmExists) { await Pick(client, 0, token); await ClickAck(client, "Farm", token); }
        await Pick(client, 0, token); await ClickAck(client, "Upgrade", token);
        await Pick(client, 1, token); await ClickAck(client, "Barracks", token);
        await Advance([client, observer], token);
        await Pick(client, 1, token);
        GameEvent sword = await ClickAck(client, "Recruit", token);
        GameEvent ranged = await ClickAck(client, "RecruitRanged", token);
        CityState army = State(ranged).Players.Single(p => p.Id == client.PlayerId);
        Require(army.Soldiers.Select(u => u.Type).SequenceEqual([UnitType.Swordsman, UnitType.Crossbowman]) && army.Food == 0,
            "ordinary melee/ranged UI input recruits both profiles for food exactly once");
        Require(State(sword).Players.Single(p => p.Id == client.PlayerId).Food == 5, "Swordsman cost comes from authoritative rules");
        UiObservation controls = await UiProtocol.Probe(client, options.StartupTimeout, token);
        foreach (string name in new[] { "Recruit", "RecruitRanged" })
        {
            UiTarget control = controls.Targets[name];
            Require(control.Visible && !control.Enabled && control.X > 0 && control.X < controls.Width && control.Y > 0 && control.Y < controls.Height,
                name + " remains visible, disabled without food and inside the window");
        }
        // Do not auto-recruit more soldiers: this is one small mixed army, not a battle matrix.
        for (int turn = 0; turn < 2; turn++)
        {
            await Action(client, "ready", token);
            MatchSnapshot resolved = State(await Action(observer, "ready", token));
            await Observe(client, s => s.Revision >= resolved.Revision && s.TurnSerial == resolved.TurnSerial && s.Phase == resolved.Phase, "mixed turn synchronization", token);
        }
        await Observe(client, s => s.Phase == Phase.Combat, "mixed first wave", token);
    }

    private async Task CombatCheckpoint(Child client, Child observer, CancellationToken token, bool shortCheck = false)
    {
        UiObservation moving = await WaitUi(client, p => p.Units.Any(u => u.Type == UnitType.Swordsman && u.Clip == "Running_A"), "actual locomotion pose", token);
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
            JsonSerializer.Serialize(frozen.Units) == JsonSerializer.Serialize(still.Units), "pause freezes positions, skeleton poses, shots and death clocks");
        await Checkpoint(client, "combat-paused", token);
        await ClickAck(client, "Pause", token);
        bool sword = false, shot = false, hit = false;
        UiObservation casualty = await WaitUi(client, p =>
        {
            RenderedContact(p);
            foreach (UnitObservation ranged in p.Units.Where(u => u.Type == UnitType.Crossbowman))
                if (ranged.ShotCount > ranged.AttackSequence) throw new InvalidOperationException("Repeated snapshots duplicated a shot.");
            foreach (UnitObservation unit in p.Units)
            {
                if (unit.Clip == "Hit_A" && !unit.HitActive) throw new InvalidOperationException("Declared hit has no active animation layer.");
                if (unit.Clip is "1H_Melee_Attack_Slice_Horizontal" or "2H_Ranged_Shoot" && !unit.AttackActive)
                    throw new InvalidOperationException("Declared attack has no active animation one-shot.");
            }
            sword |= p.Units.Any(u => u.Type == UnitType.Swordsman && u.Clip == "1H_Melee_Attack_Slice_Horizontal");
            shot |= p.Units.Any(u => u.Type == UnitType.Crossbowman && u.ShotVisible);
            hit |= p.Units.Any(u => u.Clip == "Hit_A");
            return sword && shot && hit && p.Units.Any(u => u.Dead);
        }, "melee, shot, hit and first death poses", token, 60000);
        Require(sword && shot && hit, "rendered melee/shooting/hit states sampled from live nodes");
        int dead = casualty.Units.First(u => u.Dead).Id;
        Require(!CombatPlayback.All(Latest(client)).Any(u => u.Id == dead), "death visual is absent from living combat state");
        await ClickAck(client, "Pause", token);
        UiObservation deathPaused = await WaitUi(client, p => p.PhaseText.Contains("PAUSED", StringComparison.Ordinal) && p.Units.Any(u => u.Id == dead && u.Dead), "paused death remains", token);
        await Action(observer, "unknown", token, false);
        UiObservation deathStill = await UiProtocol.Probe(client, options.StartupTimeout, token);
        Require(deathStill.VisualSeconds == deathPaused.VisualSeconds && JsonSerializer.Serialize(deathPaused.Units) == JsonSerializer.Serialize(deathStill.Units), "death pose and cleanup freeze on shared pause");
        await Checkpoint(client, "combat-casualty", token);
        await ClickAck(client, "Pause", token);
        UiObservation cleaned = await WaitUi(client, p => p.Units.All(u => u.Id != dead), "bounded death cleanup", token);
        Require(cleaned.VisualSeconds - casualty.VisualSeconds <= 2, "death view frees within two unpaused seconds");
        await Click(client, "ReturnToMenu", token);
        await WaitUi(client, p => p.Screen == "menu", "combat return cleanup", token);
        await Click(client, "Singleplayer", token);
        UiObservation fresh = await WaitUi(client, p => p.Screen == "session" && p.PhaseText.Contains("Building", StringComparison.Ordinal), "fresh solo after combat", token);
        Require(fresh.Units.Length == 0 && fresh.EventCursor == 0 && Latest(client).MatchId != Latest(observer).MatchId,
            "fresh match clears living/dead views, events and old authority identity");
        await Checkpoint(client, "combat-fresh-session", token);
    }
}
