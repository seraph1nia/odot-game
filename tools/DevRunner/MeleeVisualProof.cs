using Game.Core;

namespace DevRunner;

[Flags]
internal enum MeleeCoverage { None = 0, Shared = 1, Near = 2, Far = 4, Simultaneous = 8, Windup = 16, Impact = 32 }
internal sealed record MeleeWitness(int Actor, int Target, long Sequence, long ImpactTick, int ActorCell, int TargetCell,
    int ActorFootprint, int TargetFootprint, double Distance, MeleeCoverage Coverage);

// Measures live rendered nodes. Distances classify visual near/far positions;
// authoritative graph adjacency alone establishes whether the attack is legal.
internal static class MeleeVisualProof
{
    // Authority milestones let the ordinary pause request reach a short windup
    // before graphical probing finishes. Only live-node Inspect proves rendering.
    public static bool HasMilestone(MatchSnapshot state, MeleeCoverage phase, HexBoard board)
    {
        UnitState[] standing = CombatPlayback.All(state).Concat(state.DyingBodies)
            .Where(u => u.Deployed && u.Hex is { HoldsTransit: false }).ToArray();
        foreach (UnitState actor in standing.Where(u => u.Class == UnitClass.Melee && u.Hex is { Lifecycle: UnitLifecycle.Alive }
            && u.AttackSequence > 0 && !u.TargetCity))
        {
            UnitState? target = standing.SingleOrDefault(u => u.Id == actor.TargetId);
            if (target is null || target.Faction == actor.Faction || target.Destination != actor.Destination
                || board.Distance(actor.Hex!.Position.Cell, target.Hex!.Position.Cell) != 1) continue;
            if (standing.Count(u => u.Destination == actor.Destination && u.Hex!.Position.Cell == actor.Hex.Position.Cell) < 2
                || standing.Count(u => u.Destination == actor.Destination && u.Hex!.Position.Cell == target.Hex.Position.Cell) < 2) continue;
            if (phase == MeleeCoverage.Impact && actor.AttackLanded == true && state.Tick >= actor.ImpactTick && state.Tick < actor.ImpactTick + 12) return true;
            if (phase == MeleeCoverage.Windup && actor.PendingImpact && actor.ImpactTick - state.Tick >= 6
                && standing.Any(u => u.Hex is { Lifecycle: UnitLifecycle.Alive } && u.Faction == target.Faction && u.Destination == actor.Destination
                    && u.PendingImpact && u.ImpactTick == actor.ImpactTick && u.Hex.Position.Cell == target.Hex.Position.Cell
                    && standing.Any(t => t.Id == u.TargetId && t.Hex!.Position.Cell == actor.Hex.Position.Cell))) return true;
        }
        return false;
    }
    public static MeleeWitness[] Inspect(UiObservation frame, HexBoard board)
    {
        UnitObservation[] standing = frame.Units.Where(u => u.Visible && u.Deployed && u.Hex is { Lifecycle: not UnitLifecycle.Queued, HoldsTransit: false }).ToArray();
        var result = new List<MeleeWitness>();
        foreach (StrikeObservation strike in frame.Strikes.Where(s => s.Visible))
        {
            UnitObservation? actor = standing.SingleOrDefault(u => u.Id == strike.Id), target = standing.SingleOrDefault(u => u.Id == strike.TargetId);
            if (actor is null || target is null || actor.Dead || actor.Class != UnitClass.Melee || actor.Faction == target.Faction
                || actor.Destination != target.Destination || board.Distance(actor.Hex!.Position.Cell, target.Hex!.Position.Cell) != 1
                || actor.AttackSequence != strike.AttackSequence || actor.ImpactTick != strike.ImpactTick) continue;
            MeleeCoverage coverage = MeleeCoverage.None;
            UnitObservation[] allies = standing.Where(u => u.Destination == actor.Destination && u.Hex!.Position.Cell == actor.Hex.Position.Cell).ToArray();
            UnitObservation[] opponents = standing.Where(u => u.Destination == actor.Destination && u.Hex!.Position.Cell == target.Hex.Position.Cell).ToArray();
            if (allies.Length >= 2 && opponents.Length >= 2) coverage |= MeleeCoverage.Shared;
            double Distance(UnitObservation u) => Math.Sqrt(Math.Pow(u.X - actor.X, 2) + Math.Pow(u.Z - actor.Z, 2));
            double distance = Distance(target);
            double near = opponents.Min(Distance), far = opponents.Max(Distance);
            if (far - near > .15)
            {
                if (distance <= near + .03) coverage |= MeleeCoverage.Near;
                if (distance >= far - .03) coverage |= MeleeCoverage.Far;
            }
            double tick = actor.Hex.FrozenTick ?? frame.CombatTick;
            bool linked = Math.Abs(strike.TargetX - target.X) < .02 && Math.Abs(strike.TargetZ - target.Z) < .02;
            if (linked && actor.WeaponAttached && actor.AttackActive && actor.BoneRotation.Length > 0)
            {
                if (tick >= actor.ActionStartTick && tick < strike.ImpactTick) coverage |= MeleeCoverage.Windup;
                if (tick >= strike.ImpactTick && strike.AttackLanded == true && strike.ImpactVisible) coverage |= MeleeCoverage.Impact;
            }
            result.Add(new(actor.Id, target.Id, strike.AttackSequence, strike.ImpactTick, actor.Hex.Position.Cell, target.Hex.Position.Cell,
                actor.Hex.Position.Footprint, target.Hex.Position.Footprint, distance, coverage));
        }
        return result.Select(w => (w.Coverage & (MeleeCoverage.Windup | MeleeCoverage.Impact)) != 0 && result.Any(other => other.ActorCell == w.TargetCell && other.TargetCell == w.ActorCell
            && other.ImpactTick == w.ImpactTick && other.Actor != w.Actor && (other.Coverage & (MeleeCoverage.Windup | MeleeCoverage.Impact)) != 0)
            ? w with { Coverage = w.Coverage | MeleeCoverage.Simultaneous } : w).ToArray();
    }
}
