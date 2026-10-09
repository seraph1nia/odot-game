namespace DevRunner;

internal sealed record AssetPlacementObservation
{
    public string Asset { get; init; } = "";
    public string Name { get; init; } = "";
    public string Pocket { get; init; } = "";
    public float[] ProjectedBounds { get; init; } = [];
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }
    public float Scale { get; init; }
    public float Support { get; init; }
    public float Top { get; init; }
    public float[] Contact { get; init; } = [];
}

internal sealed record TerrainBoundsObservation(string Asset, float[] Min, float[] Max);
internal sealed record WalkObservation(string Name, int Planks, int Foundations, float Thickness, float[] Start, float[] End, float[][] Centers, float[][] Footprints, float[] Supports);
internal sealed record RiverObservation(string Asset, float[] Start, float[] End);
internal sealed record GroundTrailObservation(string Asset, int Column, int Row, string Floor, float Clearance, float[][] Ports);
internal sealed record LandscapeObservation
{
    public int Tiles { get; init; }
    public int TerrainBatches { get; init; }
    public int TerrainSurfaces { get; init; }
    public int FloorCells { get; init; }
    public int BridgeSubstratesRemoved { get; init; }
    public int BridgeRecessedMeshes { get; init; }
    public GroundTrailObservation[] DirtTrail { get; init; } = [];
    public float[] TrailView { get; init; } = [];
    public float[] ForestView { get; init; } = [];
    public TerrainBoundsObservation[] TerrainBounds { get; init; } = [];
    public RiverObservation[] River { get; init; } = [];
    public WalkObservation[] Paths { get; init; } = [];
    public float[][] BridgeLandings { get; init; } = [];
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
        return cells.Length >= 3 && landscape.BridgeSubstratesRemoved == 5 && landscape.BridgeRecessedMeshes == 0
            && cells.All(c => c.Asset == Game.DetailedGround.Connector(Game.GroundOverlay.River, 0, 3).Asset
                && c.Start.Length == 3 && c.End.Length == 3 && c.Start.Concat(c.End).All(float.IsFinite)
                && Math.Abs(c.End[0] - c.Start[0] - 3) < .003f && Math.Abs(c.Start[1] - c.End[1]) < .003f && Math.Abs(c.Start[2] - c.End[2]) < .003f
                && Math.Abs(c.Start[2] - 5 * 2.598076f) < .003f && Math.Abs(c.Start[1] - (-.03f + .008f * (1.7320508f / 2.55f))) < .003f)
            && cells.Zip(cells.Skip(1)).All(p => p.First.End.Zip(p.Second.Start).All(q => Math.Abs(q.First - q.Second) < .003f));
    }
    internal static bool AuthoredTerrain(LandscapeObservation landscape) => landscape.FloorCells == landscape.Tiles && landscape.Tiles > 0
        && !landscape.TerrainBounds.Any(b => b.Asset is Game.AssetCatalog.Meadow or Game.AssetCatalog.Stream)
        && landscape.TerrainBounds.Count(b => Game.DetailedGround.Bases.Contains(b.Asset)) == 5
        && landscape.TerrainBounds.Where(b => Game.DetailedGround.Bases.Contains(b.Asset)).All(b => b.Min.Length == 3 && b.Max.Length == 3
            && Math.Abs(b.Min[0] + 2.55f) < .01 && Math.Abs(b.Max[0] - 2.55f) < .01
            && Math.Abs(b.Min[2] + 2.2083647f) < .01 && Math.Abs(b.Max[2] - 2.2083647f) < .01
            && Math.Abs(b.Max[1]) < .002 && Math.Abs(b.Min[1] + .36f) < .002);

    internal static bool JoinedTrail(LandscapeObservation scene)
    {
        GroundTrailObservation[] trail = scene.DirtTrail;
        var expected = Game.DetailedGround.BankTrail().ToArray();
        if (trail.Length != expected.Length) return false;
        for (int i = 0; i < trail.Length; i++)
        {
            var cell = trail[i]; var planned = expected[i];
            if (cell.Column != planned.Column || cell.Row != planned.Row || cell.Asset != planned.Connector.Asset
                || cell.Floor != Game.DetailedGround.Base(cell.Column, cell.Row) || cell.Clearance is < .004f or > .005f
                || cell.Ports.Length != planned.Connector.Edges.Length || cell.Ports.Any(p => p.Length != 3 || !p.All(float.IsFinite)
                    || Math.Abs(p[1] - (.18f + .012f * (1.7320508f / 2.55f))) > .0003f)) return false;
            if (i > 0 && !cell.Ports.Any(p => trail[i - 1].Ports.Any(q => p.Zip(q).All(v => Math.Abs(v.First - v.Second) < .0003f)))) return false;
        }
        return true;
    }
    internal static bool WalksJoined(LandscapeObservation scene)
    {
        static bool Point(float[] p) => p.Length == 3 && p.All(float.IsFinite);
        static float Distance(float[] a, float[] b) => MathF.Sqrt(MathF.Pow(a[0] - b[0], 2) + MathF.Pow(a[2] - b[2], 2));
        if (scene.Paths.Length != 6 || scene.BridgeLandings.Length != 2 || !scene.BridgeLandings.All(Point)) return false;
        foreach (WalkObservation path in scene.Paths)
        {
            if (!Point(path.Start) || !Point(path.End) || path.Thickness is < .1f or > .15f
                || path.Foundations < 1 || path.Planks < path.Foundations || path.Centers.Length != path.Planks || !path.Centers.All(Point)
                || path.Footprints.Length != path.Planks || path.Footprints.Any(p => p.Length != 4 || !p.All(float.IsFinite) || p[0] >= p[2] || p[1] >= p[3])) return false;
            if (path.Supports.Length != path.Planks || path.Supports.Any(s => !float.IsFinite(s))
                || path.Centers.Where((c, i) => Math.Abs(c[1] - path.Supports[i] - (i < path.Foundations ? 0 : path.Thickness)) > .002f).Any()) return false;
            if (path.Centers.Take(path.Foundations).Zip(path.Centers.Take(path.Foundations).Skip(1)).Any(p => Distance(p.First, p.Second) > .18f || Math.Abs(p.First[1] - p.Second[1]) > .22f)) return false;
            // The largest physical building/raised upgrade is still 1.9 wide.
            // Check installed plank bounds against every fixed plot's structure
            // envelope, rather than assuming a center-only cosmetic line is safe.
            for (int slot = 0; slot < 9; slot++)
            {
                int row = slot / 3 + 2; float x = (slot % 3 - 1) * 3 + row % 2 * 1.5f, z = row * 2.598076f;
                if (path.Footprints.Any(p => p[0] < x + .95f && p[2] > x - .95f && p[1] < z + .95f && p[3] > z - .95f)) return false;
            }
        }
        if (scene.Paths.Take(5).Zip(scene.Paths.Take(5).Skip(1)).Any(p => Distance(p.First.End, p.Second.Start) > .06f)) return false;
        foreach (var (path, landing, endpoint) in new[] { (scene.Paths[4], scene.BridgeLandings[0], scene.Paths[4].End), (scene.Paths[5], scene.BridgeLandings[1], scene.Paths[5].Start) })
        {
            if (Distance(endpoint, landing) > .06f) return false;
            float top = path.Centers.Where(c => Distance(c, endpoint) < .15f).Select(c => c[1] + path.Thickness).DefaultIfEmpty(float.NaN).Max();
            if (!float.IsFinite(top) || Math.Abs(top - landing[1]) > .06f) return false;
        }
        return true;
    }
    internal static bool SceneryClear(LandscapeObservation scene, IEnumerable<UiTarget> plots) => scene.Static.Any(p => p.Pocket.Length > 0)
        && scene.Static.Where(p => p.Pocket.Length > 0).All(p => p.Z >= 7 && p.ProjectedBounds.Length == 4 && p.ProjectedBounds.All(float.IsFinite)
            && plots.All(t => t.X < p.ProjectedBounds[0] || t.X > p.ProjectedBounds[2] || t.Y < p.ProjectedBounds[1] || t.Y > p.ProjectedBounds[3]));

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
        Require(LandscapeChecks.JoinedRiver(frame.Landscape), "actual surface stream ports join through the timber-only bridge without recessed water or competing floors");
        Require(LandscapeChecks.JoinedTrail(frame.Landscape), "actual same-origin dirt turns/ends join on the bank terrace with positive floor clearance");
        Require(LandscapeChecks.WalksJoined(frame.Landscape), "installed village-edge walk connects to actual bridge deck ends without occupying any fixed building envelope");
        Require(LandscapeChecks.SceneryClear(frame.Landscape, frame.Targets.Where(p => p.Key.StartsWith("Plot", StringComparison.Ordinal) && p.Value.Visible).Select(p => p.Value)), "new clustered scenery leaves actual projected plot selection points unobscured");
        Require(frame.Landscape.Static.All(asset => LandscapeChecks.Contact(asset, asset.Support)), "starting assets meet supporting hex/overlay surfaces");
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
