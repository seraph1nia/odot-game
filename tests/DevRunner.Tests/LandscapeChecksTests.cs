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
    public void StreamAndBridgeRequireActualContinuousWaterEdgesAndExactlyOneBridge()
    {
        var scene = new LandscapeObservation
        {
            River = [
            new(Game.AssetCatalog.Stream, [-4.5f, -.144f, 12.99f], [-1.5f, -.144f, 12.99f]),
            new(Game.AssetCatalog.Bridge, [-1.5f, -.144f, 12.99f], [1.5f, -.144f, 12.99f]),
            new(Game.AssetCatalog.Stream, [1.5f, -.144f, 12.99f], [4.5f, -.144f, 12.99f])]
        };
        Assert.True(LandscapeChecks.JoinedRiver(scene));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { River = [.. scene.River, scene.River[1]] }));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { River = [scene.River[0], scene.River[1], scene.River[2] with { Start = [1.6f, -.144f, 12.99f] }] }));
        Assert.False(LandscapeChecks.JoinedRiver(scene with { River = [scene.River[0], scene.River[1] with { Start = [-1.5f, .2f, 12.99f] }, scene.River[2]] }));
    }
    [Fact]
    public void ImportedTerrainFootprintRejectsWrongScaleHeightAndLegacySubstitution()
    {
        var scene = new LandscapeObservation
        {
            TerrainBounds = [
            new(Game.AssetCatalog.Meadow, [-2.55f, -.36f, -2.20836f], [2.55f, 0, 2.20836f]),
            new(Game.AssetCatalog.Stream, [-2.55f, -.36f, -2.20836f], [2.55f, 0, 2.20836f])]
        };
        Assert.True(LandscapeChecks.AuthoredTerrain(scene));
        Assert.False(LandscapeChecks.AuthoredTerrain(scene with { TerrainBounds = [scene.TerrainBounds[0], scene.TerrainBounds[1] with { Asset = "legacy" }] }));
        Assert.False(LandscapeChecks.AuthoredTerrain(scene with { TerrainBounds = [scene.TerrainBounds[0] with { Max = [3, 0, 2.20836f] }, scene.TerrainBounds[1]] }));
        Assert.False(LandscapeChecks.AuthoredTerrain(scene with { TerrainBounds = [scene.TerrainBounds[0] with { Max = [2.55f, .2f, 2.20836f] }, scene.TerrainBounds[1]] }));
    }
}
