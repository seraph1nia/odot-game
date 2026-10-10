using System.Numerics;

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
internal sealed record WalkObservation(string Name, int Planks, int Foundations, float Thickness, float[] Start, float[] End, float[][] Centers, float[][] Footprints, float[] Supports, float[][][] Bases, float[][] BottomHeights);
internal sealed record GroundTrailObservation(string Asset, int Column, int Row, string Floor, float Clearance, float[][] Ports)
{
    public float[][][] Profiles { get; init; } = [];
}
internal sealed record LandscapeObservation
{
    public int Tiles { get; init; }
    public int TerrainBatches { get; init; }
    public int TerrainSurfaces { get; init; }
    public int FloorCells { get; init; }
    public int BridgeSubstratesRemoved { get; init; }
    public int BridgeRecessedMeshes { get; init; }
    public GroundTrailObservation[] DirtTrail { get; init; } = [];
    public GroundTrailObservation[] WoodlandTrail { get; init; } = [];
    public AssetPlacementObservation[] Scenery { get; init; } = [];
    public int SceneryBatches { get; init; }
    public int SceneryInstances { get; init; }
    public int SceneryMeshes { get; init; }
    public float SceneryTransformError { get; init; }
    public float[] TrailView { get; init; } = [];
    public float[] ForestView { get; init; } = [];
    public TerrainBoundsObservation[] TerrainBounds { get; init; } = [];
    public GroundTrailObservation[] River { get; init; } = [];
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

    internal static bool JoinedRiver(LandscapeObservation landscape) => landscape.BridgeSubstratesRemoved == 5 && landscape.BridgeRecessedMeshes == 0
        && JoinedRoute(landscape.River, Game.DetailedGround.Watercourse(), Game.GroundOverlay.River);
    internal static bool AuthoredTerrain(LandscapeObservation landscape) => landscape.FloorCells == landscape.Tiles && landscape.Tiles > 0
        && !landscape.TerrainBounds.Any(b => b.Asset is Game.AssetCatalog.Meadow or Game.AssetCatalog.Stream)
        && landscape.TerrainBounds.Count(b => Game.DetailedGround.Bases.Contains(b.Asset)) == 5
        && landscape.TerrainBounds.Where(b => Game.DetailedGround.Bases.Contains(b.Asset)).All(b => b.Min.Length == 3 && b.Max.Length == 3
            && Math.Abs(b.Min[0] + 2.55f) < .01 && Math.Abs(b.Max[0] - 2.55f) < .01
            && Math.Abs(b.Min[2] + 2.2083647f) < .01 && Math.Abs(b.Max[2] - 2.2083647f) < .01
            && Math.Abs(b.Max[1]) < .002 && Math.Abs(b.Min[1] + .36f) < .002);

    internal static bool JoinedTrail(LandscapeObservation scene) => JoinedRoute(scene.DirtTrail, Game.DetailedGround.BankTrail(), Game.GroundOverlay.Path)
        && JoinedRoute(scene.WoodlandTrail, Game.DetailedGround.WoodlandTrail(), Game.GroundOverlay.Path);

    internal static bool JoinedRoute(GroundTrailObservation[] actual, IEnumerable<(int Column, int Row, Game.GroundConnector Connector)> route, Game.GroundOverlay family)
    {
        var expected = route.ToArray();
        if (actual.Length != expected.Length) return false;
        static bool Point(float[] p) => p.Length == 3 && p.All(float.IsFinite);
        static bool Same(float[] a, float[] b) => Point(a) && Point(b) && a.Zip(b).All(v => Math.Abs(v.First - v.Second) < .0003f);
        for (int i = 0; i < actual.Length; i++)
        {
            var cell = actual[i]; var planned = expected[i];
            float height = Game.DetailedGround.Elevation(cell.Column, cell.Row);
            if (cell.Column != planned.Column || cell.Row != planned.Row || cell.Asset != planned.Connector.Asset
                || cell.Floor != Game.DetailedGround.Base(cell.Column, cell.Row) || cell.Clearance is < .004f or > .005f
                || cell.Ports.Length != planned.Connector.Edges.Length || cell.Profiles.Length != cell.Ports.Length) return false;
            for (int j = 0; j < cell.Ports.Length; j++)
            {
                double angle = planned.Connector.Edges[j] * Math.PI / 3;
                float x = cell.Column * 3 + Math.Abs(cell.Row % 2) * 1.5f, z = cell.Row * 2.598076f;
                float dx = (float)(-1.5 * Math.Cos(angle)), dz = (float)(1.5 * Math.Sin(angle));
                float y = height + (family == Game.GroundOverlay.River ? .008f : .012f) * (1.7320508f / 2.55f);
                float[][] profile = cell.Profiles[j];
                if (!Same(cell.Ports[j], [x + dx, y, z + dz]) || profile.Length < 5
                    || profile.Any(p => !Point(p) || Math.Abs((p[0] - x) * dx + (p[2] - z) * dz - 2.25f) > .001f)) return false;
            }
            if (i == 0) continue;
            int edge = Array.FindIndex(planned.Connector.Edges, e => Game.DetailedGround.Neighbor(cell.Column, cell.Row, e) == (actual[i - 1].Column, actual[i - 1].Row));
            if (edge < 0) return false;
            int previous = Array.FindIndex(expected[i - 1].Connector.Edges, e => Game.DetailedGround.Neighbor(actual[i - 1].Column, actual[i - 1].Row, e) == (cell.Column, cell.Row));
            if (previous < 0 || !Same(cell.Ports[edge], actual[i - 1].Ports[previous])) return false;
            // Inspect full emitted bank/water or path edge profiles, not only
            // ideal centers. A raised lip, buried bank or rotated corner fails.
            float[][] a = cell.Profiles[edge], b = actual[i - 1].Profiles[previous];
            if (a.Length != b.Length || a.Any(p => !b.Any(q => Same(p, q)))) return false;
        }
        return true;
    }
    internal static bool ScatterClear(LandscapeObservation scene, IEnumerable<UiTarget> plots) => scene.Scenery.Length >= 200
        && scene.SceneryMeshes > 0 && scene.SceneryBatches > 0 && scene.SceneryInstances >= scene.Scenery.Length
        && float.IsFinite(scene.SceneryTransformError) && scene.SceneryTransformError < .0001f
        && scene.Scenery.All(p => p.Z >= 7 && Contact(p, p.Support) && p.ProjectedBounds.Length == 4 && p.ProjectedBounds.All(float.IsFinite)
            && plots.All(t => t.X < p.ProjectedBounds[0] || t.X > p.ProjectedBounds[2] || t.Y < p.ProjectedBounds[1] || t.Y > p.ProjectedBounds[3])
            && Game.DetailedGround.ClearOfRoutes(p.X, p.Z, p.Asset.EndsWith("tree.glb", StringComparison.Ordinal) || p.Asset.EndsWith("pine.glb", StringComparison.Ordinal) ? p.Scale * .35f : p.Scale / 2));
    internal static bool WalkGeometryGrounded(WalkObservation path)
    {
        if (path.Planks < 1 || path.Foundations < 1 || path.Foundations > path.Planks || !float.IsFinite(path.Thickness)
            || path.Supports.Length != path.Planks || path.Supports.Any(s => !float.IsFinite(s))
            || path.Bases.Length != path.Planks || path.BottomHeights.Length != path.Planks) return false;
        for (int i = 0; i < path.Planks; i++)
        {
            float[][] axes = path.Bases[i]; float[] heights = path.BottomHeights[i];
            if (axes.Length != 3 || axes.Any(a => a.Length != 3 || !a.All(float.IsFinite))
                || heights.Length != 2 || !heights.All(float.IsFinite) || heights[0] > heights[1]) return false;
            var x = new Vector3(axes[0][0], axes[0][1], axes[0][2]);
            var y = new Vector3(axes[1][0], axes[1][1], axes[1][2]);
            var z = new Vector3(axes[2][0], axes[2][1], axes[2][2]);
            if (Math.Abs(x.Length() - 1) > .0001f || Math.Abs(y.Length() - 1) > .0001f || Math.Abs(z.Length() - 1) > .0001f
                || Math.Abs(Vector3.Dot(x, y)) > .0001f || Math.Abs(Vector3.Dot(x, z)) > .0001f
                || Math.Abs(Vector3.Dot(y, z)) > .0001f || Vector3.Distance(Vector3.Cross(x, y), z) > .0001f
                || Math.Abs(x.Y) > .0001f || Math.Abs(z.Y) > .0001f || Vector3.Distance(y, Vector3.UnitY) > .0001f) return false;
            float support = path.Supports[i] + (i < path.Foundations ? 0 : path.Thickness);
            if (heights.Any(h => Math.Abs(h - support) > .0002f)) return false;
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
            if (!WalkGeometryGrounded(path) || !Point(path.Start) || !Point(path.End) || path.Thickness is < .1f or > .15f
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
        Require(LandscapeChecks.JoinedTrail(frame.Landscape), "actual same-origin bank and woodland trails join full edge profiles on flat terraces with positive floor clearance");
        Require(LandscapeChecks.ScatterClear(frame.Landscape, frame.Targets.Where(p => p.Key.StartsWith("Plot", StringComparison.Ordinal) && p.Value.Visible).Select(p => p.Value)), "actual shared-mesh woodland scatter is grounded, outside route corridors and clear of projected plot selection points");
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
