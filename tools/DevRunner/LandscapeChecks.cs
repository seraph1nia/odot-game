namespace DevRunner;

internal sealed record AssetPlacementObservation
{
    public string Asset { get; init; } = "";
    public string Name { get; init; } = "";
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
    public float Scale { get; init; }
    public float Support { get; init; }
    public float[] Contact { get; init; } = [];
}

internal sealed record LandscapeObservation
{
    public int Tiles { get; init; }
    public int TerrainBatches { get; init; }
    public int Plots { get; init; }
    public string Bridge { get; init; } = "";
    public string[] CoreTerrain { get; init; } = [];
    public float MinX { get; init; }
    public float MaxX { get; init; }
    public float MinZ { get; init; }
    public float MaxZ { get; init; }
    public float[][] View { get; init; } = [];
    public AssetPlacementObservation[] Static { get; init; } = [];
}

// Check observed installed geometry, not the producer's calculated coverage range.
internal static class LandscapeChecks
{
    internal static bool Covered(LandscapeObservation landscape) => landscape.Tiles > 0 && landscape.View.Length >= 4
        && landscape.View.All(p => p.Length == 2 && float.IsFinite(p[0]) && float.IsFinite(p[1])
            && p[0] > landscape.MinX + 3 && p[0] < landscape.MaxX - 3
            && p[1] > landscape.MinZ + 2.6f && p[1] < landscape.MaxZ - 2.6f);

    internal static bool Contact(AssetPlacementObservation asset, float support) => asset.Contact.Length == 3
        && Math.Abs(asset.Contact[0] - asset.X) < .002f && Math.Abs(asset.Contact[1] - support) < .002f
        && Math.Abs(asset.Contact[2] - asset.Z) < .002f;

    internal static float TerrainHeight(AssetPlacementObservation asset)
    {
        int row = (int)Math.Round(asset.Z / 2.598076f);
        int column = (int)Math.Round((asset.X - Math.Abs(row) % 2 * 1.5f) / 3);
        return column == 3 ? -.4f : row >= 4 ? 1 : row >= 3 ? .5f : 0;
    }

    internal static bool SameStartingArea(LandscapeObservation a, LandscapeObservation b) => a.Static.Length > 0
        && a.Plots == 9 && b.Plots == 9 && a.Bridge.Length > 0 && a.Bridge == b.Bridge
        && a.CoreTerrain.Length > 0 && a.CoreTerrain.SequenceEqual(b.CoreTerrain)
        && a.Static.Select(Signature).SequenceEqual(b.Static.Select(Signature));
    private static string Signature(AssetPlacementObservation asset) => FormattableString.Invariant($"{asset.Asset}:{asset.X:F3}:{asset.Y:F3}:{asset.Z:F3}:{asset.Scale:F3}");
}

internal sealed partial class Runner
{
    private static void Countryside(UiObservation frame)
    {
        Require(LandscapeChecks.Covered(frame.Landscape), "observed countryside covers final view with exterior edges offscreen");
        Require(frame.Landscape.Plots == 9 && frame.Landscape.CoreTerrain.Length >= 77 && frame.Landscape.Bridge.Length > 0, "shared plots, central terrain and bridge present");
        Require(frame.Landscape.Static.Length >= 16, "shared starting structures and decorations instantiated");
        Require(frame.Landscape.Static.All(asset => LandscapeChecks.Contact(asset, LandscapeChecks.TerrainHeight(asset))), "starting assets meet supporting hex surfaces");
        AssetPlacementObservation home = frame.Landscape.Static.Single(p => p.Asset.EndsWith("building_home_A_blue.gltf", StringComparison.Ordinal));
        AssetPlacementObservation defender = frame.Landscape.Static.Single(p => p.Asset.EndsWith("building_tower_A_blue.gltf", StringComparison.Ordinal));
        Require(home.X == 1.5f && Math.Abs(home.Z - 2.598076f) < .002 && defender.X == -3 && defender.Z == 0, "home and defender centered on reserved hexes");
        Require(frame.Placements.All(asset => LandscapeChecks.Contact(asset, asset.Support)), "dynamic assets meet supporting terrain/decks");
        foreach (AssetPlacementObservation asset in frame.Placements)
        {
            if (asset.Name.StartsWith("Slot", StringComparison.Ordinal) && int.TryParse(asset.Name.AsSpan(4), out int slot))
            {
                int row = slot / 3 + 2;
                Require(Math.Abs(asset.X - ((slot % 3 - 1) * 3 + row % 2 * 1.5f)) < .002
                    && Math.Abs(asset.Z - row * 2.598076f) < .002, "building footprint centered on stable slot " + slot);
            }
        }
    }
}
