namespace Game;

public enum GroundOverlay { Path, River }
public sealed record GroundConnector(string Asset, int Turns, int[] Edges);

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
    public static string Base(int column, int row)
    {
        if (row == 5) return Moss; // same decorative river row, now a surface stream
        if (row is >= 2 and <= 4 && column is >= -1 and <= 1) return Dirt;
        if (column >= 2) return Duff;
        if (column <= -3) return ((column / 3 + row / 3) & 1) == 0 ? Litter : Moss;
        return Grass; // open battle approach and home/defender receivers
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
    // Four bank-clearing cells extend the existing south bridge walk towards
    // the east grove, on its already-flat .18 terrace. No random road generator,
    // crossings, changes to plot centers, or connectors over terrace steps.
    public static IEnumerable<(int Column, int Row, GroundConnector Connector)> BankTrail()
    {
        (int Column, int Row)[] cells = [(1, 6), (1, 7), (2, 7), (3, 6)];
        for (int i = 0; i < cells.Length; i++)
        {
            var cell = cells[i];
            int EdgeTo((int Column, int Row) next) => Enumerable.Range(0, 6)
                .Single(e => Neighbor(cell.Column, cell.Row, e) == next);
            int first = EdgeTo(cells[i == 0 ? 1 : i - 1]);
            int? second = i > 0 && i < cells.Length - 1 ? EdgeTo(cells[i + 1]) : null;
            yield return (cell.Column, cell.Row, Connector(GroundOverlay.Path, first, second));
        }
    }
}
