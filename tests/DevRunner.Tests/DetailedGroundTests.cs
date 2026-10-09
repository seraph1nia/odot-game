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
                if (row == 5) Assert.Equal(DetailedGround.Moss, floor);
            }
        Assert.Equal(5, installed.Count);
        Assert.Equal(13, DetailedGround.Paths.Distinct().Count());
    }
    [Fact]
    public void BankTrailUsesFourAdjacentFlatNonPlotCellsAndBothCenterEnds()
    {
        var trail = DetailedGround.BankTrail().ToArray();
        Assert.Equal(4, trail.Length);
        Assert.Single(trail[0].Connector.Edges); Assert.Single(trail[^1].Connector.Edges);
        Assert.All(trail, p => Assert.True(p.Row >= 6));
        for (int i = 1; i < trail.Length; i++)
        {
            var a = trail[i - 1]; var b = trail[i];
            int edge = a.Connector.Edges.Single(e => DetailedGround.Neighbor(a.Column, a.Row, e) == (b.Column, b.Row));
            Assert.Contains((edge + 3) % 6, b.Connector.Edges);
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
