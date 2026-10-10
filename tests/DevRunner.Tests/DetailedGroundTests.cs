using System.Buffers.Binary;
using System.Text.Json;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class DetailedGroundTests
{
    [Theory]
    [InlineData(GroundOverlay.Path)]
    [InlineData(GroundOverlay.River)]
    public void EveryUnorderedPairAndEndSelectsCanonicalTypedExport(GroundOverlay family)
    {
        var selected = new HashSet<(string, int)>();
        for (int first = 0; first < 6; first++)
            for (int second = first; second < 6; second++)
            {
                GroundConnector piece = DetailedGround.Connector(family, first, first == second ? null : second);
                Assert.True(selected.Add((piece.Asset, piece.Turns)));
                Assert.Equal(piece.Edges, DetailedGround.Connector(family, first, first == second ? null : second).Edges);
                if (first != second)
                {
                    GroundConnector reverse = DetailedGround.Connector(family, second, first);
                    Assert.Equal(piece.Asset, reverse.Asset); Assert.Equal(piece.Turns, reverse.Turns);
                }
                using JsonDocument glb = Read(piece.Asset);
                JsonElement metadata = glb.RootElement.GetProperty("nodes").EnumerateArray().Select(n => n.TryGetProperty("extras", out JsonElement e) ? e : default)
                    .Single(e => e.ValueKind == JsonValueKind.Object && e.TryGetProperty("ground_schema", out _));
                int[] actual = metadata.GetProperty("connector_edges").EnumerateArray().Select(e => (e.GetInt32() + piece.Turns) % 6).Order().ToArray();
                Assert.Equal(piece.Edges, actual);
                Assert.Equal(first == second, metadata.GetProperty("connector_center_end").GetBoolean());
                Assert.Equal(family == GroundOverlay.River ? "surface_stream_v1" : "surface_path_v1", metadata.GetProperty("connector_interface").GetString());
                Assert.Equal(1, metadata.GetProperty("ground_schema").GetInt32());
            }
        Assert.Equal(21, selected.Count);
    }
    [Fact]
    public void InvalidEdgesAndFamiliesAreNotCoerced()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DetailedGround.Connector(GroundOverlay.Path, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetailedGround.Connector(GroundOverlay.Path, 6));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetailedGround.Connector(GroundOverlay.Path, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => DetailedGround.Connector((GroundOverlay)9, 0));
    }
    [Fact]
    public void NeighborOppositesWorkAcrossPositiveAndNegativeOddRows()
    {
        for (int row = -7; row <= 7; row++)
            for (int column = -3; column <= 3; column++)
                for (int edge = 0; edge < 6; edge++)
                {
                    var next = DetailedGround.Neighbor(column, row, edge);
                    Assert.Equal((column, row), DetailedGround.Neighbor(next.Column, next.Row, (edge + 3) % 6));
                }
    }
    [Fact]
    public void FloorProjectionIsBoundedStableAndKeepsTheApproachOpen()
    {
        var installed = new HashSet<string>();
        for (int row = -8; row <= 8; row++)
            for (int column = -5; column <= 5; column++)
            {
                string floor = DetailedGround.Base(column, row); installed.Add(floor);
                Assert.Contains(floor, DetailedGround.Bases);
                Assert.Equal(floor, DetailedGround.Base(column, row));
                if (row < 2 && column is >= -2 and <= 1) Assert.Equal(DetailedGround.Grass, floor);
                if (DetailedGround.Watercourse().Any(p => p.Column == column && p.Row == row)) Assert.Equal(DetailedGround.Moss, floor);
            }
        Assert.Equal(5, installed.Count);
        Assert.Equal(13, DetailedGround.Paths.Distinct().Count());
    }
    [Fact]
    public void AuthoredRunsAreAdjacentFlatNonIntersectingAndHaveRealCenterEnds()
    {
        var all = new HashSet<(int, int)>();
        foreach (var trail in new[] { DetailedGround.BankTrail().ToArray(), DetailedGround.WoodlandTrail().ToArray(), DetailedGround.Watercourse().ToArray() })
        {
            Assert.True(trail.Length >= 5);
            Assert.Single(trail[0].Connector.Edges); Assert.Single(trail[^1].Connector.Edges);
            Assert.All(trail, p =>
            {
                Assert.True(p.Row >= 5);
                Assert.True(all.Add((p.Column, p.Row))); // no fake junction/crossing
                Assert.Equal(DetailedGround.Elevation(trail[0].Column, trail[0].Row), DetailedGround.Elevation(p.Column, p.Row));
                Assert.InRange(p.Connector.Edges.Length, 1, 2);
            });
            for (int i = 1; i < trail.Length; i++)
            {
                var a = trail[i - 1]; var b = trail[i];
                int edge = a.Connector.Edges.Single(e => DetailedGround.Neighbor(a.Column, a.Row, e) == (b.Column, b.Row));
                Assert.Contains((edge + 3) % 6, b.Connector.Edges);
            }
        }
        Assert.Equal(34, DetailedGround.Watercourse().Count());
        Assert.Equal(.18f, DetailedGround.Elevation(5, 5)); // no ghost straight trench after the bend
        Assert.Equal(-.03f, DetailedGround.Elevation(4, 6));
        Assert.Contains(DetailedGround.Watercourse(), p => p.Column == 1 && p.Row == 5 && p.Connector.Edges.SequenceEqual(new[] { 0, 3 }));
        Assert.Equal(new[] { (1, 6), (1, 7), (2, 7), (3, 6) }, DetailedGround.BankTrail().Take(4).Select(p => (p.Column, p.Row)));
        Assert.Contains(DetailedGround.Watercourse(), p => p.Connector.Asset.EndsWith("turn_120.glb", StringComparison.Ordinal));
        Assert.Contains(DetailedGround.Watercourse(), p => p.Connector.Asset.EndsWith("turn_60.glb", StringComparison.Ordinal));
    }
    [Fact]
    public void ForestPatchesHaveOpeningsAndManySmallUnevenSectionsNotStripesOrCheckerboards()
    {
        var counts = new Dictionary<string, int>();
        int sameNeighbors = 0, differentNeighbors = 0;
        for (int row = 3; row <= 14; row++)
            for (int column = 3; column <= 12; column++)
            {
                string floor = DetailedGround.Base(column, row);
                counts[floor] = counts.GetValueOrDefault(floor) + 1;
                if (floor == DetailedGround.Base(column + 1, row)) sameNeighbors++; else differentNeighbors++;
            }
        Assert.True(counts.Count >= 4);
        Assert.True(counts[DetailedGround.Grass] >= 10);
        Assert.InRange(sameNeighbors, 35, 100);
        Assert.InRange(differentNeighbors, 20, 85);
        Assert.All(counts.Values, n => Assert.True(n < 70)); // rejects the previous all-duff east
        for (int row = -8; row <= 4; row++)
            for (int column = -2; column <= 2; column++)
                Assert.Equal(row >= 4 ? .18f : row >= 3 ? .09f : 0, DetailedGround.Elevation(column, row));
    }
    [Fact]
    public void FullCombatBoundaryRetainsOriginalFloorHeightsFamiliesAndSixWayOrientations()
    {
        for (int row = -10; row <= 2; row++)
            for (int column = -5; column <= 5; column++)
            {
                string expected = row == 2 && column is >= -1 and <= 1 ? DetailedGround.Dirt
                    : column >= 2 ? DetailedGround.Duff : column <= -3 ? ((column / 3 + row / 3) & 1) == 0 ? DetailedGround.Litter : DetailedGround.Moss : DetailedGround.Grass;
                Assert.Equal(expected, DetailedGround.Base(column, row));
                Assert.Equal(((column * 17 + row * 31) % 6 + 6) % 6, DetailedGround.FloorTurns(column, row));
                Assert.Equal(0, DetailedGround.Elevation(column, row));
                Assert.Empty(DetailedGround.Scatter(column, row));
            }
    }
    [Fact]
    public void ScatterIsStableClusteredVariedAndKeepsRoutesPlotsAndBattleBandClear()
    {
        var placed = new List<GroundDecoration>();
        int empty = 0, full = 0;
        for (int row = -8; row <= 14; row++)
            for (int column = -12; column <= 12; column++)
            {
                GroundDecoration[] cell = DetailedGround.Scatter(column, row).ToArray();
                Assert.Equal(cell, DetailedGround.Scatter(column, row).ToArray());
                if (row < 3 || row < 6 && column is >= -2 and <= 2) Assert.Empty(cell);
                if (row >= 6) { if (cell.Length == 0) empty++; else full++; }
                Assert.All(cell, p =>
                {
                    Assert.Contains(p.Asset, AssetCatalog.RequiredPaths);
                    Assert.True(DetailedGround.ClearOfRoutes(p.X, p.Z, p.Asset.EndsWith("tree.glb", StringComparison.Ordinal) || p.Asset.EndsWith("pine.glb", StringComparison.Ordinal) ? p.Size * .35f : p.Size / 2));
                    Assert.True(p.Size <= 2.8f);
                    Assert.InRange(p.Yaw, 0, 359);
                });
                placed.AddRange(cell);
            }
        Assert.True(empty > 20 && full > 50);
        Assert.InRange(placed.Count, 400, 1400);
        Assert.Equal(7, placed.Select(p => p.Asset).Distinct().Count());
        Assert.True(placed.Select(p => p.Yaw).Distinct().Count() > 100);
        Assert.True(placed.Select(p => p.Size).Distinct().Count() > 15);
        Assert.False(DetailedGround.ClearOfRoutes(4.5f, 5 * 2.598076f, .1f));
        Assert.False(DetailedGround.ClearOfRoutes(3.75f, 6.5f * 2.598076f, .1f));
    }
    [Fact]
    public void CachedCorridorBroadPhaseMatchesTheFullSegmentTestIncludingNearPorts()
    {
        var segments = DetailedGround.Watercourse().Concat(DetailedGround.BankTrail()).Concat(DetailedGround.WoodlandTrail())
            .SelectMany(p => p.Connector.Edges.Select(e =>
            {
                double angle = e * Math.PI / 3;
                return (X: p.Column * 3 + Math.Abs(p.Row % 2) * 1.5f, Z: p.Row * 2.598076f,
                    Dx: (float)(-1.5 * Math.Cos(angle)), Dz: (float)(1.5 * Math.Sin(angle)), Width: p.Connector.Asset.Contains("river", StringComparison.Ordinal) ? .65f : .45f);
            })).ToArray();
        bool Full(float x, float z, float radius) => segments.All(s =>
        {
            float t = Math.Clamp(((x - s.X) * s.Dx + (z - s.Z) * s.Dz) / 2.25f, 0, 1);
            return MathF.Sqrt(MathF.Pow(x - s.X - t * s.Dx, 2) + MathF.Pow(z - s.Z - t * s.Dz, 2)) >= radius + s.Width;
        });
        foreach (float radius in new[] { .1f, .35f, .98f })
        {
            for (float z = 7; z <= 38; z += .75f)
                for (float x = -40; x <= 42; x += .75f)
                    Assert.Equal(Full(x, z, radius), DetailedGround.ClearOfRoutes(x, z, radius));
            foreach (var s in segments)
                foreach (float offset in new[] { -.001f, .001f })
                {
                    float x = s.X + s.Dx + s.Dz / 1.5f * (radius + s.Width + offset);
                    float z = s.Z + s.Dz - s.Dx / 1.5f * (radius + s.Width + offset);
                    Assert.Equal(Full(x, z, radius), DetailedGround.ClearOfRoutes(x, z, radius));
                }
        }
    }
    private static JsonDocument Read(string asset)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Odot.slnx"))) directory = directory.Parent;
        byte[] bytes = File.ReadAllBytes(Path.Combine(directory!.FullName, "src/Game/Assets/Authored", asset));
        return JsonDocument.Parse(bytes.AsMemory(20, checked((int)BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(12)))));
    }
}
