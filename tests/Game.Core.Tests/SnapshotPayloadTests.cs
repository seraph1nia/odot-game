using System.IO.Compression;
using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

public sealed class SnapshotPayloadTests
{
    [Fact]
    public void FullFourCityCatalogAndPausedStateRoundTripWithoutChangingAnyFields()
    {
        using var match = new Match(combatSeed: 1);
        for (int city = 0; city < 4; city++) match.Join();
        MatchSnapshot snapshot = match.Snapshot() with { Paused = true, Wave = 20 };
        string json = JsonSerializer.Serialize(snapshot, WireJson.Options);
        string payload = SnapshotPayload.Encode(snapshot);
        Assert.True(payload.Length < json.Length / 4, "The complete repeated catalogs must fit well below their uncompressed reliable message size.");
        Assert.Equal(json, JsonSerializer.Serialize(SnapshotPayload.Decode(payload), WireJson.Options));
    }
    [Fact]
    public void CompressedExpansionCannotExceedTheTransportBound()
    {
        using var output = new MemoryStream();
        using (var compressor = new BrotliStream(output, CompressionLevel.Fastest, leaveOpen: true)) compressor.Write(new byte[16 * 1024 * 1024 + 1]);
        var error = Assert.Throws<InvalidDataException>(() => SnapshotPayload.Decode(Convert.ToBase64String(output.ToArray())));
        Assert.Contains("transport bound", error.Message);
        Assert.Throws<FormatException>(() => SnapshotPayload.Decode("invalid base64"));
    }
}
