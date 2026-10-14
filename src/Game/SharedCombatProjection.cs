using System.Numerics;
using Game.Core;

namespace Game;

// Pure shared-world cosmetic projection. Native views and numerical observers
// share this seam; no rules, claims, targets, simulation or engine nodes here.
internal static class SharedCombatProjection
{
    internal const float HalfWidth = 1.5f;
    internal const float RowStep = 1.1547005f * HalfWidth * 1.5f;
    internal static Vector3 Origin => new(0, 0, 100);
    private static Vector3 Center(HexBoard board, int id)
    {
        HexCoordinate cell = board.Cell(id).Coordinate;
        return new(cell.Column * HalfWidth * 2 + (Math.Abs(cell.R) % 2) * HalfWidth, 0, cell.R * RowStep);
    }
    internal static Vector3 CellPosition(HexBoard board, int id)
        => Center(board, id) - Vector3.Lerp(Center(board, board.Cells[0].Id), Center(board, board.Cells[^1].Id), .5f);
    internal static Vector3 Formation(int slot, int capacity, bool boss)
    {
        if (boss) return Vector3.Zero;
        float angle = -MathF.PI / 2 + slot * MathF.Tau / capacity;
        float radius = capacity <= 3 ? .65f : 1.05f;
        return new(MathF.Cos(angle) * radius, 0, MathF.Sin(angle) * radius);
    }
    internal static Vector3 Pose(HexBoard board, HexUnitState hex, double tick)
    {
        if (hex.Lifecycle is UnitLifecycle.Queued or UnitLifecycle.Reserve or UnitLifecycle.Stored) return Origin;
        Vector3 point = CellPosition(board, hex.Position.Cell);
        if (hex.HoldsTransit) point = CombatVisualTiming.Position(point, CellPosition(board, hex.Destination.Cell), hex, tick);
        return Origin + point;
    }
    internal static Vector3 Position(HexBoard board, HexUnitState hex, int slot, int capacity, bool boss, double tick)
        => Pose(board, hex, tick) + Formation(slot, capacity, boss);
}
