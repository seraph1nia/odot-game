using Game.Core;

namespace DevRunner;

// Live-node assertions complement pure timing tests; no competing child reader.
internal static class CombatProgressionProof
{
    public static void Motion(UiObservation frame, HexBoard board, int squadCapacity = 3)
    {
        foreach (UnitObservation unit in frame.Units.Where(u => u.Visible && u.Deployed && u.Hex is { HoldsTransit: true }))
        {
            // Observe the shared production projection, not a second city-local
            // anchor/interpolation model. Imported animation still needs tier B.
            var expected = Game.SharedCombatProjection.Position(board, unit.Hex!, unit.FormationSlot, squadCapacity, unit.IsBoss, frame.CombatTick);
            if (Math.Abs(unit.X - expected.X) > .002 || Math.Abs(unit.Z - expected.Z) > .002)
                throw new InvalidOperationException("Rendered committed step detoured or used the wrong clock: " + unit.Id);
            if (!unit.Dead && unit.Clip == "walk" && (unit.WalkingBlend < 0 || unit.WalkingBlend > .8))
                throw new InvalidOperationException("Committed walking selected a sprint blend: " + unit.Id);
            if (unit.Clip == "run") throw new InvalidOperationException("Committed movement is still sprinting: " + unit.Id);
        }
    }
    public static bool MovementAdvanced(IEnumerable<UiObservation> frames)
        => frames.SelectMany(f => f.Units.Where(u => u.Visible && !u.Dead && u.Clip == "walk" && u.WalkingBlend > 0 && u.Hex is { HoldsTransit: true })
            .Select(u => (Frame: f, Unit: u))).GroupBy(p => (p.Unit.Id, p.Unit.Hex!.StartTick, p.Unit.Hex.EndTick))
            .Any(g => g.Select(p => p.Frame.CombatTick).Distinct().Count() >= 2
                && g.Any(p => g.Any(q => Math.Abs(p.Unit.X - q.Unit.X) + Math.Abs(p.Unit.Z - q.Unit.Z) > .03)));
    public static bool AttackAdvanced(IEnumerable<UiObservation> frames)
        => frames.SelectMany(f => f.Units.Where(u => u.Visible && !u.Dead && u.WeaponAttached && u.EquipmentAligned && u.AttackActive && u.AttackBlend > 0
            && u.Clip == "attack")
            .Select(u => (Frame: f, Unit: u))).GroupBy(p => (p.Unit.Id, p.Unit.AttackSequence, p.Unit.ImpactTick))
            .Any(g => g.Select(p => p.Frame.CombatTick).Distinct().Count() >= 2
                && g.Select(p => p.Unit.PoseSeconds).Max() - g.Select(p => p.Unit.PoseSeconds).Min() > .02
                && g.Select(p => p.Unit.BoneRotation).Distinct().Count() >= 2
                && g.Select(p => p.Unit.EquipmentRotation).Distinct().Count() >= 2);
}
