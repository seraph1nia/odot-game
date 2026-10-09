using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class VillageWalkTests
{
    private static WalkObservation Walk(string name, float x1, float z1, float x2, float z2)
    {
        float length = MathF.Sqrt((x2 - x1) * (x2 - x1) + (z2 - z1) * (z2 - z1));
        int count = (int)Math.Ceiling(length / .17f) + 1;
        float[][] basis = Enumerable.Range(0, count).Select(i => new[] { x1 + (x2 - x1) * i / (count - 1), -.03f, z1 + (z2 - z1) * i / (count - 1) }).ToArray();
        float[][] centers = [.. basis, .. basis.Select(p => new[] { p[0], p[1] + .125f, p[2] })];
        float halfX = x1 == x2 ? .55f : .0891f, halfZ = x1 == x2 ? .0891f : .55f;
        float[][] axes = [[(x2 - x1) / length, 0, (z2 - z1) / length], [0, 1, 0], [-(z2 - z1) / length, 0, (x2 - x1) / length]];
        return new(name, centers.Length, count, .125f, basis[0], basis[^1], centers,
            centers.Select(p => new[] { p[0] - halfX, p[2] - halfZ, p[0] + halfX, p[2] + halfZ }).ToArray(), centers.Select(_ => -.03f).ToArray(),
            centers.Select(_ => axes).ToArray(), centers.Select(p => new[] { p[1], p[1] }).ToArray());
    }
    private static LandscapeObservation Scene() => new()
    {
        BridgeLandings = [[4.5f, .22f, 12.15f], [4.5f, .22f, 13.83f]],
        Paths = [Walk("west1", -6, 5.196f, -6, 7.794f), Walk("west2", -6, 7.794f, -6, 10.392f),
            Walk("west3", -6, 10.392f, -6, 11.95f), Walk("bank", -6, 11.95f, 4.5f, 11.95f),
            Walk("landing", 4.5f, 11.95f, 4.5f, 12.15f), Walk("south", 4.5f, 13.83f, 4.5f, 15.588f)]
    };
    [Fact]
    public void ObservedWalkRejectsGapsBadContactsWrongDeckAndPlotIntrusion()
    {
        LandscapeObservation scene = Scene(); Assert.True(LandscapeChecks.WalksJoined(scene));
        Assert.False(LandscapeChecks.WalksJoined(scene with { Paths = [] }));
        Assert.False(LandscapeChecks.WalksJoined(scene with { BridgeLandings = [[4.5f, .22f, 11], scene.BridgeLandings[1]] }));
        WalkObservation first = scene.Paths[0];
        foreach (WalkObservation invalid in new[]
        {
            first with { End = [-6, -.03f, 7] },
            first with { Centers = [[-6, 1, 5.196f], .. first.Centers.Skip(1)] },
            first with { Footprints = [[-3.9f, 4.3f, -2.1f, 6.1f], .. first.Footprints.Skip(1)] },
            first with { Supports = [] }
        }) Assert.False(LandscapeChecks.WalksJoined(scene with { Paths = [invalid, .. scene.Paths.Skip(1)] }));
    }
    [Fact]
    public void ProjectedPocketsCannotHideActualPlotPointsOrEnterBattle()
    {
        var prop = new AssetPlacementObservation { Pocket = "grove", Z = 10, ProjectedBounds = [-1, -1, 1, 1] };
        var scene = new LandscapeObservation { Static = [prop] };
        Assert.True(LandscapeChecks.SceneryClear(scene, [new(2, 2, true, true)]));
        Assert.False(LandscapeChecks.SceneryClear(scene, [new(0, 0, true, true)]));
        Assert.False(LandscapeChecks.SceneryClear(scene with { Static = [prop with { Z = 0 }] }, []));
        Assert.False(LandscapeChecks.SceneryClear(scene with { Static = [prop with { ProjectedBounds = [] }] }, []));
    }
}
