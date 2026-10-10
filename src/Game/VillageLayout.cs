using Godot;

namespace Game;

// Decorative geometry only; these IDs remain the nine authoritative array indices.
internal static class VillageLayout
{
    public const float HalfWidth = 1.5f;
    public const float Radius = 1.1547005f * HalfWidth;
    public const float RowStep = Radius * 1.5f;
    public const float TerrainScale = Radius / 2.55f;
    public const int RiverRow = 5;
    public const int BridgeColumn = 1;
    public static float Height(int column, int row) => row == RiverRow ? -.03f : row >= 4 ? .18f : row >= 3 ? .09f : 0;
    public static Vector3 Hex(int column, int row) => new(column * HalfWidth * 2 + (Math.Abs(row) % 2) * HalfWidth, Height(column, row), row * RowStep);
    public static float Surface(Vector3 point)
    {
        int row = (int)Math.Round(point.Z / RowStep);
        int column = (int)Math.Round((point.X - (Math.Abs(row) % 2) * HalfWidth) / (HalfWidth * 2));
        return Height(column, row);
    }
    public static Transform3D GroundTransform(int column, int row, int turns = 0) => new(
        new Basis(Vector3.Up, Mathf.Pi / 2 + turns * Mathf.Pi / 3).Scaled(Vector3.One * TerrainScale),
        new Vector3(Hex(column, row).X, DetailedGround.Elevation(column, row), Hex(column, row).Z));
    public static Vector3 Slot(int slot) => Hex(slot % 3 - 1, slot / 3 + 2);
    public static bool Contains(int slot, Vector3 point)
    {
        Vector3 local = point - Slot(slot);
        float x = Math.Abs(local.X), z = Math.Abs(local.Z);
        return x <= HalfWidth && z <= Radius - x / Mathf.Sqrt(3);
    }
    // Slab intersection returns distance along a normalized camera ray, including roofs.
    public static float? RayBounds(Vector3 origin, Vector3 direction, Aabb bounds)
    {
        float near = 0, far = float.PositiveInfinity;
        for (int axis = 0; axis < 3; axis++)
        {
            if (Math.Abs(direction[axis]) < 0.00001f)
            {
                if (origin[axis] < bounds.Position[axis] || origin[axis] > bounds.End[axis]) return null;
                continue;
            }
            float a = (bounds.Position[axis] - origin[axis]) / direction[axis];
            float b = (bounds.End[axis] - origin[axis]) / direction[axis];
            near = Math.Max(near, Math.Min(a, b)); far = Math.Min(far, Math.Max(a, b));
            if (near > far) return null;
        }
        return near;
    }
}
