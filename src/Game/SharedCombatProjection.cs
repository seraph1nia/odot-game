using System.Numerics;
using Game.Core;

namespace Game;

// Pure shared-world cosmetic projection. Native views and numerical observers
// share this seam; no rules, claims, targets, simulation or engine nodes here.
internal static class SharedCombatProjection
{
    internal const float HalfWidth = 1.5f;
    internal const float Radius = 1.1547005f * HalfWidth;
    internal const float RowStep = Radius * 1.5f;
    internal static Vector3 Origin => new(0, 0, 100);
    // Pointy-top odd-row offset projection: shared by meshes, rays, terrain,
    // formations and the camera. Axial Q/R and authority topology never change.
    internal static Vector3 Lattice(int column, int row)
        => new(column * HalfWidth * 2 + (Math.Abs(row) % 2) * HalfWidth, 0, row * RowStep);
    internal static Vector3 Corner(int corner, float scale = 1)
    {
        float angle = MathF.PI / 6 + corner * MathF.PI / 3;
        return new(MathF.Cos(angle) * Radius * scale, 0, MathF.Sin(angle) * Radius * scale);
    }
    internal static bool Contains(Vector3 local)
        => Math.Abs(local.X) <= HalfWidth && Math.Abs(local.Z) <= Radius - Math.Abs(local.X) / MathF.Sqrt(3);
    private static Vector3 Center(HexBoard board, int id)
    {
        HexCoordinate cell = board.Cell(id).Coordinate;
        return Lattice(cell.Column, cell.R);
    }
    internal static Vector3 CellPosition(HexBoardDefinition board, int id) => CellPosition(board, board.Cells.First(c => c.Id == id));
    private static Vector3 CellPosition(HexBoardDefinition board, HexCellDefinition cell)
    {
        Vector3 CenterOf(HexCellDefinition c) => Lattice(c.Coordinate.Column, c.Coordinate.R);
        return CenterOf(cell) - Vector3.Lerp(CenterOf(board.Cells[0]), CenterOf(board.Cells[^1]), .5f);
    }
    internal static Vector3 DistrictOrigin(HexBoardDefinition board)
        => Origin + new Vector3(board.Cells.Min(c => CellPosition(board, c).X) - 9, 0,
            board.Cells.Min(c => CellPosition(board, c).Z));
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
