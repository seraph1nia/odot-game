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
    public float Top { get; init; }
    public float[] Contact { get; init; } = [];
}

internal sealed record TerrainBoundsObservation(string Asset, float[] Min, float[] Max);
internal sealed record RiverObservation(string Asset, float[] Start, float[] End);
internal sealed record LandscapeObservation
{
    public int Tiles { get; init; }
    public int TerrainBatches { get; init; }
    public int TerrainSurfaces { get; init; }
    public TerrainBoundsObservation[] TerrainBounds { get; init; } = [];
    public RiverObservation[] River { get; init; } = [];
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
        return row == 5 ? -.03f : row >= 4 ? .18f : row >= 3 ? .09f : 0;
    }

    internal static bool JoinedRiver(LandscapeObservation landscape)
    {
        RiverObservation[] cells = landscape.River.OrderBy(r => r.Start.ElementAtOrDefault(0)).ToArray();
        return cells.Length >= 3 && cells.Count(c => c.Asset == Game.AssetCatalog.Bridge) == 1
            && cells.All(c => c.Start.Length == 3 && c.End.Length == 3 && c.Start.Concat(c.End).All(float.IsFinite)
                && Math.Abs(c.End[0] - c.Start[0] - 3) < .003f && Math.Abs(c.Start[1] - c.End[1]) < .003f && Math.Abs(c.Start[2] - c.End[2]) < .003f
                && Math.Abs(c.Start[2] - 5 * 2.598076f) < .003f && Math.Abs(c.Start[1] - (-.03f - .1675f * (1.7320508f / 2.55f))) < .003f)
            && cells.Zip(cells.Skip(1)).All(p => p.First.End.Zip(p.Second.Start).All(q => Math.Abs(q.First - q.Second) < .003f));
    }
    internal static bool AuthoredTerrain(LandscapeObservation landscape) => landscape.TerrainBounds.Length == 2
        && landscape.TerrainBounds.All(b => b.Asset is Game.AssetCatalog.Meadow or Game.AssetCatalog.Stream
            && b.Min.Length == 3 && b.Max.Length == 3
            && Math.Abs(b.Min[0] + 2.55f) < .01 && Math.Abs(b.Max[0] - 2.55f) < .01
            && Math.Abs(b.Min[2] + 2.2083647f) < .01 && Math.Abs(b.Max[2] - 2.2083647f) < .01
            && Math.Abs(b.Max[1]) < .002 && Math.Abs(b.Min[1] + .36f) < .002);

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
        Require(frame.AuthoredProvenanceBundled && frame.InstalledModels.SequenceEqual(Game.AssetCatalog.RequiredPaths.Select(p => Game.AssetCatalog.Root + p)), "actual source/package 3D inventory is exactly the required authored set with owner-permission provenance");
        Require(LandscapeChecks.Covered(frame.Landscape), "observed countryside covers final view with exterior edges offscreen");
        Require(frame.Landscape.Plots == 9 && frame.Landscape.CoreTerrain.Length >= 77 && frame.Landscape.Bridge.Length > 0, "shared plots, central terrain and bridge present");
        Require(frame.Landscape.Static.Length >= 16, "shared starting structures and decorations instantiated");
        Require(LandscapeChecks.AuthoredTerrain(frame.Landscape), "actual imported terrain bounds match the common hex footprint and ground surface");
        Require(LandscapeChecks.JoinedRiver(frame.Landscape), "actual authored stream and single bridge water edges join without gaps or double cells");
        Require(frame.Landscape.Static.All(asset => LandscapeChecks.Contact(asset, LandscapeChecks.TerrainHeight(asset))), "starting assets meet supporting hex surfaces");
        AssetPlacementObservation home = frame.Landscape.Static.Single(p => p.Asset == Game.AssetCatalog.Home);
        AssetPlacementObservation defender = frame.Landscape.Static.Single(p => p.Asset == Game.AssetCatalog.Defender);
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
