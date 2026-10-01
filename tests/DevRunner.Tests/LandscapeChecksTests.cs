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
}
