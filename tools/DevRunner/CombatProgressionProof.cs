using Game.Core;

namespace DevRunner;

// Live-node assertions complement pure timing tests; no competing child reader.
internal static class CombatProgressionProof
{
    public static void Motion(UiObservation frame, HexBoard board)
    {
        foreach (UnitObservation unit in frame.Units.Where(u => u.Visible && u.Deployed && u.Hex is { HoldsTransit: true }))
        {
            HexUnitState hex = unit.Hex!;
            (double X, double Z) Anchor(HexPosition position)
            {
                HexCoordinate cell = board.Cell(position.Cell).Coordinate; HexAnchor anchor = board.Anchor(position.Anchor);
                return ((unit.Destination - 1) * 40 + cell.Column * 3 + (Math.Abs(cell.R) % 2) * 1.5 + anchor.AnchorX / 1000d,
                    cell.R * (1.1547005 * 1.5 * 1.5) + anchor.AnchorForward / 1000d);
            }
            var from = Anchor(hex.Position); var to = Anchor(hex.Destination);
            double elapsed = unit.Dead ? hex.FrozenMoveTicks : (hex.FrozenTick ?? frame.CombatTick) - hex.StartTick;
            double fraction = Math.Clamp(elapsed / Math.Max(1, hex.EndTick - hex.StartTick), 0, 1);
            if (Math.Abs(unit.X - (from.X + (to.X - from.X) * fraction)) > .002
                || Math.Abs(unit.Z - (from.Z + (to.Z - from.Z) * fraction)) > .002)
                throw new InvalidOperationException("Rendered committed step detoured or used the wrong clock: " + unit.Id);
            if (!unit.Dead && unit.Clip == "Walking_A" && (unit.WalkingBlend < 0 || unit.WalkingBlend > .8))
                throw new InvalidOperationException("Committed walking selected a sprint blend: " + unit.Id);
            if (unit.Clip == "Running_A") throw new InvalidOperationException("Committed movement is still sprinting: " + unit.Id);
        }
    }
    public static bool MovementAdvanced(IEnumerable<UiObservation> frames)
        => frames.SelectMany(f => f.Units.Where(u => u.Visible && !u.Dead && u.Clip == "Walking_A" && u.WalkingBlend > 0 && u.Hex is { HoldsTransit: true })
            .Select(u => (Frame: f, Unit: u))).GroupBy(p => (p.Unit.Id, p.Unit.Hex!.StartTick, p.Unit.Hex.EndTick))
            .Any(g => g.Select(p => p.Frame.CombatTick).Distinct().Count() >= 2
                && g.Any(p => g.Any(q => Math.Abs(p.Unit.X - q.Unit.X) + Math.Abs(p.Unit.Z - q.Unit.Z) > .03)));
    public static bool AttackAdvanced(IEnumerable<UiObservation> frames)
        => frames.SelectMany(f => f.Units.Where(u => u.Visible && !u.Dead && u.WeaponAttached && u.AttackActive && u.AttackBlend > 0
            && u.Clip is "1H_Melee_Attack_Slice_Horizontal" or "2H_Melee_Attack_Chop" or "2H_Ranged_Shoot" or "Spellcast_Shoot")
            .Select(u => (Frame: f, Unit: u))).GroupBy(p => (p.Unit.Id, p.Unit.AttackSequence, p.Unit.ImpactTick))
            .Any(g => g.Select(p => p.Frame.CombatTick).Distinct().Count() >= 2
                && g.Select(p => p.Unit.PoseSeconds).Max() - g.Select(p => p.Unit.PoseSeconds).Min() > .02
                && g.Select(p => p.Unit.BoneRotation).Distinct().Count() >= 2);
}
