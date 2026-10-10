using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class LandscapeChecksTests
{
    [Fact]
    public void CoverageRejectsMissingTilesAndExposedPerimeter()
    {
        var wide = new LandscapeObservation
        {
            Tiles = 400,
            MinX = -30,
            MaxX = 30,
            MinZ = -30,
            MaxZ = 30,
            View = [[-20, -10], [20, -10], [20, 10], [-20, 10]]
        };
        Assert.True(LandscapeChecks.Covered(wide));
        Assert.False(LandscapeChecks.Covered(wide with { MaxX = 21 }));
        Assert.False(LandscapeChecks.Covered(wide with { Tiles = 0 }));
        Assert.False(LandscapeChecks.Covered(wide with { View = [[float.NaN, 0], [0, 0], [0, 0], [0, 0]] }));
    }

    [Fact]
    public void ContactRejectsSunkenFloatingAndOffCenterGeometry()
    {
        var asset = new AssetPlacementObservation { X = 3, Y = 1, Z = 5, Contact = [3, 1, 5] };
        Assert.True(LandscapeChecks.Contact(asset, 1));
        Assert.False(LandscapeChecks.Contact(asset, 0));
        Assert.False(LandscapeChecks.Contact(asset with { Contact = [3, .8f, 5] }, 1));
        Assert.False(LandscapeChecks.Contact(asset with { Contact = [3.1f, 1, 5] }, 1));
        Assert.False(LandscapeChecks.Contact(asset, LandscapeChecks.TerrainHeight(asset)));
    }

    [Fact]
    public void MenuParityRequiresActualMatchingStartingPlacements()
    {
        var scene = new LandscapeObservation { Plots = 9, Bridge = "bridge-transform", CoreTerrain = ["grass-transform"], Static = [new() { Asset = "home", X = 3, Y = 1, Z = 5 }] };
        Assert.True(LandscapeChecks.SameStartingArea(scene, scene));
        Assert.False(LandscapeChecks.SameStartingArea(scene, new LandscapeObservation()));
        Assert.False(LandscapeChecks.SameStartingArea(scene, scene with { CoreTerrain = ["river-transform"] }));
        Assert.False(LandscapeChecks.SameStartingArea(scene, scene with { Static = [scene.Static[0] with { Scale = 2 }] }));
        Assert.False(LandscapeChecks.SameStartingArea(scene, scene with { Static = [scene.Static[0] with { X = 0 }] }));
    }
    [Fact]
    public void SurfaceStreamRequiresContinuousPortsAndRemovedBridgeSubstrate()
    {
        var scene = new LandscapeObservation
        {
            BridgeSubstratesRemoved = 5,
            River = RouteObservation(Game.DetailedGround.Watercourse(), Game.GroundOverlay.River)
        };
        Assert.True(LandscapeChecks.JoinedRiver(scene));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { River = [.. scene.River, scene.River[1]] }));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { River = scene.River.Select((p, i) => i == 2 ? p with { Ports = [[1.6f, -.144f, 12.99f]] } : p).ToArray() }));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { River = scene.River.Select((p, i) => i == 1 ? p with { Ports = p.Ports.Select(q => new[] { q[0], q[1] + .01f, q[2] }).ToArray() } : p).ToArray() }));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { River = scene.River.Select((p, i) => i == 1 ? p with { Profiles = p.Profiles.Select(q => q.Select(v => new[] { v[0], v[1] + .01f, v[2] }).ToArray()).ToArray() } : p).ToArray() }));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { BridgeRecessedMeshes = 1 }));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { BridgeSubstratesRemoved = 0 }));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { River = scene.River.Select((p, i) => i == 0 ? p with { Asset = Game.AssetCatalog.Stream } : p).ToArray() }));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { River = scene.River[..^1] }));
    }
    [Fact]
    public void LiveDirtTrailRejectsWrongOrientationHeightMissingEndsAndBuriedOverlay()
    {
        float y = .18f + .012f * (1.7320508f / 2.55f);
        var scene = new LandscapeObservation
        {
            DirtTrail = RouteObservation(Game.DetailedGround.BankTrail(), Game.GroundOverlay.Path),
            WoodlandTrail = RouteObservation(Game.DetailedGround.WoodlandTrail(), Game.GroundOverlay.Path)
        };
        Assert.True(LandscapeChecks.JoinedTrail(scene));
        Assert.False(LandscapeChecks.JoinedTrail(scene with { DirtTrail = scene.DirtTrail[..^1] }));
        Assert.False(LandscapeChecks.JoinedTrail(scene with { DirtTrail = scene.DirtTrail.Select((p, i) => i == 0 ? p with { Ports = [[4, y, 16.8875f]] } : p).ToArray() }));
        Assert.False(LandscapeChecks.JoinedTrail(scene with { DirtTrail = scene.DirtTrail.Select((p, i) => i == 0 ? p with { Ports = [[3.75f, y + .01f, 16.8875f]] } : p).ToArray() }));
        Assert.False(LandscapeChecks.JoinedTrail(scene with { DirtTrail = scene.DirtTrail.Select((p, i) => i == 0 ? p with { Clearance = 0 } : p).ToArray() }));
        Assert.False(LandscapeChecks.JoinedTrail(scene with { WoodlandTrail = [] }));
        Assert.False(LandscapeChecks.JoinedTrail(scene with { WoodlandTrail = scene.WoodlandTrail.Select((p, i) => i == 1 ? p with { Profiles = [] } : p).ToArray() }));
    }
    private static GroundTrailObservation[] RouteObservation(IEnumerable<(int Column, int Row, Game.GroundConnector Connector)> route, Game.GroundOverlay family) => route.Select(p =>
    {
        float cx = p.Column * 3 + Math.Abs(p.Row % 2) * 1.5f, cz = p.Row * 2.598076f;
        float y = Game.DetailedGround.Elevation(p.Column, p.Row) + (family == Game.GroundOverlay.River ? .008f : .012f) * (1.7320508f / 2.55f);
        float[][] ports = p.Connector.Edges.Select(e => new[] { cx - 1.5f * MathF.Cos(e * MathF.PI / 3), y, cz + 1.5f * MathF.Sin(e * MathF.PI / 3) }).ToArray();
        float[][][] profiles = ports.Select((v, i) => Enumerable.Range(-2, 5).Select(j => new[]
        {
            v[0] + j * .1f * MathF.Sin(p.Connector.Edges[i] * MathF.PI / 3), v[1],
            v[2] + j * .1f * MathF.Cos(p.Connector.Edges[i] * MathF.PI / 3)
        }).ToArray()).ToArray();
        return new GroundTrailObservation(p.Connector.Asset, p.Column, p.Row, Game.DetailedGround.Base(p.Column, p.Row), .0040754f, ports) { Profiles = profiles };
    }).ToArray();
    [Fact]
    public void SharedMeshScatterRejectsMissingPopulationWrongContactsPortsPlotObstructionAndPartTransforms()
    {
        AssetPlacementObservation[] props = Enumerable.Range(3, 12).SelectMany(row => Enumerable.Range(-12, 25)
            .SelectMany(column => Game.DetailedGround.Scatter(column, row).Select(p => new AssetPlacementObservation
            {
                Asset = p.Asset,
                X = p.X,
                Z = p.Z,
                Y = Game.DetailedGround.Elevation(column, row),
                Scale = p.Size,
                Support = Game.DetailedGround.Elevation(column, row),
                Contact = [p.X, Game.DetailedGround.Elevation(column, row), p.Z],
                ProjectedBounds = [-10, -10, -5, -5]
            }))).ToArray();
        var scene = new LandscapeObservation { Scenery = props, SceneryBatches = 330, SceneryMeshes = 165, SceneryInstances = props.Length * 2 };
        UiTarget[] plots = [new(100, 100, true, true)];
        Assert.True(LandscapeChecks.ScatterClear(scene, plots));
        Assert.False(LandscapeChecks.ScatterClear(scene with { Scenery = [] }, plots));
        Assert.False(LandscapeChecks.ScatterClear(scene with { SceneryTransformError = .01f }, plots));
        Assert.False(LandscapeChecks.ScatterClear(scene with { SceneryTransformError = float.NaN }, plots));
        Assert.False(LandscapeChecks.ScatterClear(scene with { Scenery = props.Select((p, i) => i == 0 ? p with { Support = p.Support + .01f } : p).ToArray() }, plots));
        Assert.False(LandscapeChecks.ScatterClear(scene with { Scenery = props.Select((p, i) => i == 0 ? p with { ProjectedBounds = [99, 99, 101, 101] } : p).ToArray() }, plots));
        Assert.False(LandscapeChecks.ScatterClear(scene with { Scenery = props.Select((p, i) => i == 0 ? p with { X = 4.5f, Z = 5 * 2.598076f, Y = -.03f, Support = -.03f, Contact = [4.5f, -.03f, 5 * 2.598076f] } : p).ToArray() }, plots));
    }
    [Fact]
    public void EmittedWalkGeometryRejectsShearAndSlopedContactDespiteSupportedCenters()
    {
        float[][] basis = [[.6f, 0, .8f], [0, 1, 0], [-.8f, 0, .6f]];
        var path = new WalkObservation("South bank walk", 2, 1, .12f, [4.5f, 0, 13.83f], [3, .18f, 15.588456f],
            [[4, .18f, 14.5f], [4, .30f, 14.5f]], [[3.9f, 14, 4.1f, 15], [3.9f, 14, 4.1f, 15]], [.18f, .18f],
            [basis, basis], [[.18f, .18f], [.30f, .30f]]);
        Assert.True(LandscapeChecks.WalkGeometryGrounded(path));
        var tangent = System.Numerics.Vector3.Normalize(new(-1.5f, .18f, 6 * 2.598076f - 13.83f));
        var across = System.Numerics.Vector3.Cross(tangent, System.Numerics.Vector3.UnitY);
        float[][] sheared = [[tangent.X, tangent.Y, tangent.Z], [0, 1, 0], [across.X, across.Y, across.Z]];
        for (int instance = 0; instance < path.Planks; instance++)
        {
            Assert.False(LandscapeChecks.WalkGeometryGrounded(path with
            {
                Bases = path.Bases.Select((b, i) => i == instance ? sheared : b).ToArray()
            }));
            foreach (float offset in new[] { -.007f, .007f })
                Assert.False(LandscapeChecks.WalkGeometryGrounded(path with
                {
                    BottomHeights = path.BottomHeights.Select((h, i) => i == instance ? new[] { h[0] + Math.Min(offset, 0), h[1] + Math.Max(offset, 0) } : h).ToArray()
                }));
        }
        for (int axis = 0; axis < 3; axis++)
        {
            float[][] scaled = basis.Select((a, i) => i == axis ? a.Select(v => v * .99f).ToArray() : a).ToArray();
            Assert.False(LandscapeChecks.WalkGeometryGrounded(path with { Bases = [scaled, basis] }));
        }
        Assert.False(LandscapeChecks.WalkGeometryGrounded(path with { Bases = [] }));
        Assert.False(LandscapeChecks.WalkGeometryGrounded(path with { BottomHeights = [] }));
        Assert.False(LandscapeChecks.WalkGeometryGrounded(path with { BottomHeights = [[float.NaN, .18f], [.30f, .30f]] }));
    }
    [Fact]
    public void ImportedTerrainFootprintRejectsWrongScaleHeightAndLegacySubstitution()
    {
        var scene = new LandscapeObservation
        {
            Tiles = 100,
            FloorCells = 100,
            TerrainBounds = Game.DetailedGround.Bases.Select(b => new TerrainBoundsObservation(b,
                [-2.55f, -.36f, -2.20836f], [2.55f, 0, 2.20836f])).ToArray()
        };
        Assert.True(LandscapeChecks.AuthoredTerrain(scene));
        Assert.False(LandscapeChecks.AuthoredTerrain(scene with { TerrainBounds = [.. scene.TerrainBounds, new(Game.AssetCatalog.Meadow, [0, 0, 0], [1, 1, 1])] }));
        Assert.False(LandscapeChecks.AuthoredTerrain(scene with { FloorCells = 101 }));
        Assert.False(LandscapeChecks.AuthoredTerrain(scene with { TerrainBounds = scene.TerrainBounds.Select((b, i) => i == 0 ? b with { Max = [3, 0, 2.20836f] } : b).ToArray() }));
        Assert.False(LandscapeChecks.AuthoredTerrain(scene with { TerrainBounds = scene.TerrainBounds.Select((b, i) => i == 0 ? b with { Max = [2.55f, .2f, 2.20836f] } : b).ToArray() }));
    }
}
