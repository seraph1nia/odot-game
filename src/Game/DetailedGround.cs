namespace Game;

public enum GroundOverlay { Path, River }
public sealed record GroundConnector(string Asset, int Turns, int[] Edges);
public sealed record GroundDecoration(string Asset, float X, float Z, float Size, float Yaw);

// Cosmetic projection of the existing village lattice, never an authority map.
// Edge 0 becomes west after the existing +90 degree flat-to-pointy rotation.
public static class DetailedGround
{
    public const string Moss = "environment/hex_ground_moss.glb";
    public const string Grass = "environment/hex_ground_grass.glb";
    public const string Dirt = "environment/hex_ground_dirt.glb";
    public const string Litter = "environment/hex_ground_leaf_litter.glb";
    public const string Duff = "environment/hex_ground_pine_duff.glb";
    public static IEnumerable<string> Bases => new[] { Moss, Grass, Dirt, Litter, Duff };
    private static readonly (string Piece, int[] Edges)[] Pieces =
        [("straight", [0, 3]), ("turn_120", [0, 2]), ("turn_60", [0, 1]), ("end", [0])];
    public static IEnumerable<string> Paths => Bases.Concat(Enum.GetValues<GroundOverlay>()
        .SelectMany(f => Pieces.Select(p => Asset(f, p.Piece))));
    private static string Asset(GroundOverlay family, string piece) => family switch
    {
        GroundOverlay.Path => "environment/hex_path_" + piece + ".glb",
        GroundOverlay.River => "environment/hex_river_overlay_" + piece + ".glb",
        _ => throw new ArgumentOutOfRangeException(nameof(family))
    };
    public static GroundConnector Connector(GroundOverlay family, int first, int? second = null)
    {
        if (first is < 0 or > 5 || second is < 0 or > 5 || first == second)
            throw new ArgumentOutOfRangeException(nameof(first), "Use one end or two distinct hex edges.");
        int[] wanted = second is int other ? new[] { first, other }.Order().ToArray() : [first];
        foreach (var piece in Pieces)
            for (int turns = 0; turns < (piece.Piece == "straight" ? 3 : 6); turns++)
            {
                int[] edges = piece.Edges.Select(e => (e + turns) % 6).Order().ToArray();
                if (edges.SequenceEqual(wanted)) return new(Asset(family, piece.Piece), turns, edges);
            }
        throw new InvalidOperationException("Missing canonical ground connector.");
    }
    // Complete authored runs, not intersecting through-pieces or gameplay roads.
    private static readonly (int Column, int Row)[] WaterCells =
        [(-12, 8), (-11, 8), (-11, 7), (-10, 6), (-9, 6), (-9, 7), (-8, 8), (-7, 8),
        (-7, 7), (-6, 6), (-5, 6), (-4, 6), (-4, 5), (-3, 5), (-2, 5), (-1, 5), (0, 5), (1, 5),
        (2, 5), (3, 5), (4, 5), (4, 6), (4, 7), (5, 7), (6, 7), (7, 6), (7, 5), (8, 5),
        (9, 6), (9, 7), (10, 7), (11, 7), (12, 6), (13, 6)];
    private static readonly (int Column, int Row)[] BankCells =
        [(1, 6), (1, 7), (2, 7), (3, 6), (3, 7), (3, 8), (2, 9), (1, 9), (1, 10)];
    private static readonly (int Column, int Row)[] WoodlandCells =
        [(-2, 6), (-2, 7), (-2, 8), (-3, 8), (-4, 8), (-5, 7)];
    private static readonly HashSet<(int, int)> Water = [.. WaterCells];
    private static readonly HashSet<(int, int)> Trails = [.. BankCells, .. WoodlandCells];
    private static readonly (int Column, int Row, GroundConnector Connector)[] WaterRoute = Route(GroundOverlay.River, WaterCells).ToArray();
    private static readonly (int Column, int Row, GroundConnector Connector)[] BankRoute = Route(GroundOverlay.Path, BankCells).ToArray();
    private static readonly (int Column, int Row, GroundConnector Connector)[] WoodlandRoute = Route(GroundOverlay.Path, WoodlandCells).ToArray();
    private static readonly (float X, float Z, float Dx, float Dz, float Width)[] Corridors = WaterRoute.Concat(BankRoute).Concat(WoodlandRoute)
        .SelectMany(p => p.Connector.Edges.Select(e =>
        {
            double angle = e * Math.PI / 3;
            return (p.Column * 3 + Math.Abs(p.Row % 2) * 1.5f, p.Row * 2.598076f,
                (float)(-1.5 * Math.Cos(angle)), (float)(1.5 * Math.Sin(angle)), Water.Contains((p.Column, p.Row)) ? .65f : .45f);
        })).ToArray();

    // Cosmetic channel floors alone follow the water. Supporting heights of all
    // buildings, plots, the timber walks and the authority approach stay fixed.
    public static float Elevation(int column, int row) => Water.Contains((column, row)) ? -.03f
        : row >= 4 ? .18f : row >= 3 ? .09f : 0;

    public static uint Variation(int column, int row, int salt = 0)
    {
        uint value = unchecked((uint)column * 0x9e3779b9u ^ (uint)row * 0x85ebca6bu ^ (uint)salt * 0xc2b2ae35u);
        value ^= value >> 16; value *= 0x7feb352du; value ^= value >> 15; value *= 0x846ca68bu;
        return value ^ (value >> 16);
    }
    // Include the boundary cells whose footprints overlap the established
    // projected combat band. Preserve their original floor AND rotation.
    public static bool BattleReceiver(int column, int row) => column is >= -5 and <= 5 && row is >= -10 and <= 2;
    public static int FloorTurns(int column, int row) => BattleReceiver(column, row)
        ? ((column * 17 + row * 31) % 6 + 6) % 6 : (int)(Variation(column, row) % 6);
    public static string Base(int column, int row)
    {
        if (BattleReceiver(column, row))
        {
            if (row == 2 && column is >= -1 and <= 1) return Dirt;
            if (column >= 2) return Duff;
            if (column <= -3) return ((column / 3 + row / 3) & 1) == 0 ? Litter : Moss;
            return Grass;
        }
        if (Water.Contains((column, row))) return Moss;
        if (row is >= 2 and <= 4 && column is >= -1 and <= 1) return Dirt;
        if (row < 2 && column is >= -2 and <= 1) return Grass; // unchanged combat/home receivers
        if (Trails.Contains((column, row))) return Grass;
        // Irregular, weighted little groves with interleaved openings. One family
        // per coherent patch, not per-cell random noise or five giant stripes.
        int blockX = (int)Math.Floor(column / 3.0), blockZ = (int)Math.Floor(row / 3.0);
        double best = double.PositiveInfinity;
        string floor = Grass;
        for (int z = blockZ - 1; z <= blockZ + 1; z++)
            for (int x = blockX - 1; x <= blockX + 1; x++)
            {
                uint seed = Variation(x, z, 7);
                double cx = x * 3 + .4 + (seed % 17) / 10.0, rz = z * 3 + .3 + ((seed >> 8) % 19) / 10.0;
                double dx = column + Math.Abs(row % 2) * .5 - cx, dz = (row - rz) * .8660254;
                double distance = dx * dx + dz * dz - ((seed >> 16) % 13) / 10.0;
                if (distance >= best) continue;
                best = distance;
                string[] families = x < 0 ? [Moss, Litter, Grass, Litter, Duff] : [Duff, Grass, Moss, Litter, Duff];
                floor = families[(seed >> 24) % 5];
            }
        return floor;
    }
    public static (int Column, int Row) Neighbor(int column, int row, int edge)
    {
        int odd = Math.Abs(row % 2);
        return edge switch
        {
            0 => (column - 1, row),
            1 => (column + odd - 1, row + 1),
            2 => (column + odd, row + 1),
            3 => (column + 1, row),
            4 => (column + odd, row - 1),
            5 => (column + odd - 1, row - 1),
            _ => throw new ArgumentOutOfRangeException(nameof(edge))
        };
    }
    public static IEnumerable<(int Column, int Row, GroundConnector Connector)> Watercourse() => WaterRoute;
    public static IEnumerable<(int Column, int Row, GroundConnector Connector)> BankTrail() => BankRoute;
    public static IEnumerable<(int Column, int Row, GroundConnector Connector)> WoodlandTrail() => WoodlandRoute;
    private static IEnumerable<(int Column, int Row, GroundConnector Connector)> Route(GroundOverlay family, (int Column, int Row)[] cells)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            var cell = cells[i];
            int EdgeTo((int Column, int Row) next) => Enumerable.Range(0, 6)
                .Single(e => Neighbor(cell.Column, cell.Row, e) == next);
            int first = EdgeTo(cells[i == 0 ? 1 : i - 1]);
            int? second = i > 0 && i < cells.Length - 1 ? EdgeTo(cells[i + 1]) : null;
            yield return (cell.Column, cell.Row, Connector(family, first, second));
        }
    }

    // Distance to every actual connector's center/edge corridor, conservatively
    // widened around corners. Only cosmetics consume this; no gameplay RNG.
    public static bool ClearOfRoutes(float x, float z, float radius)
    {
        foreach (var (cx, cz, dx, dz, width) in Corridors)
        {
            // Conservative broad phase; still apply the identical segment/width
            // test near a route. Avoid rebuilding/sorting thousands of connector
            // arrays and recomputing trigonometry for every observed prop.
            float limit = radius + width + 1.5f;
            if (Math.Abs(x - cx) > limit || Math.Abs(z - cz) > limit) continue;
            float t = Math.Clamp(((x - cx) * dx + (z - cz) * dz) / 2.25f, 0, 1);
            float distance = MathF.Sqrt(MathF.Pow(x - cx - t * dx, 2) + MathF.Pow(z - cz - t * dz, 2));
            if (distance < radius + width) return false;
        }
        return true;
    }
    public static IEnumerable<GroundDecoration> Scatter(int column, int row)
    {
        // Preserve the battle band and the village's sightlines. The finite
        // dressed envelope covers ordinary overview/pan; exterior coverage keeps
        // its previous passive scatter. No tall canopy in front of the plots.
        if (column is < -12 or > 12 || row is < 3 or > 14 || row < 6 && column is >= -2 and <= 2
            || Water.Contains((column, row)) || Trails.Contains((column, row))) yield break;
        uint seed = Variation(column, row, 11);
        if (seed % 5 == 0) yield break; // genuine unfilled pockets, not even spacing
        string family = Base(column, row);
        float cx = column * 3 + Math.Abs(row % 2) * 1.5f, cz = row * 2.598076f;
        float ax = ((seed >> 4) % 11 - 5f) * .09f, az = ((seed >> 12) % 11 - 5f) * .08f;
        bool canopy = (column <= -3 || column >= 4) && seed % 3 != 0 && family != Grass;
        if (canopy)
        {
            float size = 1.65f + (seed >> 20) % 13 * .095f;
            float x = cx + ax * .4f, z = cz + az * .4f;
            if (z - size * .35f >= 7 && ClearOfRoutes(x, z, size * .35f))
                yield return new(family == Duff || seed % 4 == 0 ? "props/kit_pine.glb" : "environment/components/forest_spiral_tree.glb", x, z, size, seed % 360);
        }
        int count = family == Grass ? 2 : 3 + (int)(seed % 3);
        for (int i = 0; i < count; i++)
        {
            uint detail = Variation(column, row, 21 + i);
            float angle = detail % 360 * MathF.PI / 180, reach = .3f + (detail >> 12) % 7 * .07f;
            float x = cx + ax + MathF.Cos(angle) * reach, z = cz + az + MathF.Sin(angle) * reach;
            float size = .3f + (detail >> 20) % 9 * .055f;
            // Keep full footprint on one terrace, not merely its center.
            float lx = Math.Abs(x - cx) + size / 2, lz = Math.Abs(z - cz) + size / 2;
            if (z - size / 2 < 7 || lx > 1.4f || lz > 1.7320508f - lx / MathF.Sqrt(3) - .1f || !ClearOfRoutes(x, z, size / 2)) continue;
            string[] props = family == Duff ? ["forest_fern", "forest_leaf_clump", "forest_boulder", "forest_moss"]
                : family == Moss ? ["forest_fern", "forest_moss", "forest_mushroom_small", "forest_leaf_clump"]
                : ["forest_leaf_clump", "forest_fern", "forest_mushroom_small", "forest_boulder", "forest_moss"];
            yield return new("environment/components/" + props[detail % (uint)props.Length] + ".glb", x, z, size, detail % 360);
        }
    }
}
