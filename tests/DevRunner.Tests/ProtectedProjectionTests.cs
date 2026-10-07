using System.Text.Json.Nodes;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class ProtectedProjectionTests
{
    // Actual camera contracts from all twelve retained 1100x820 baseline views:
    // each camera is shared by settlement/combat 16/64/256 at its own zoom.
    [Theory]
    [InlineData(35.20043f, 488.51974f, 328.86765f)]
    [InlineData(11.733478f, 365.5592f, 346.60294f)]
    public void CachedMaskIsBitExactForEveryRetainedViewportPixelAndBorder(float size, float referenceX, float referenceY)
    {
        JsonNode camera = JsonNode.Parse("""{"Size":1,"Rotation":[-0.5934119,0.48869216,0],"ReferenceX":0,"ReferenceY":0}""")!;
        camera["Size"] = size; camera["ReferenceX"] = referenceX; camera["ReferenceY"] = referenceY;
        var cached = new LandscapeBoundaryProof.ProtectedProjection(camera, 820);
        byte[] expected = new byte[1102 * 822], actual = new byte[expected.Length];
        int count = 0;
        for (int y = -1; y <= 820; y++) for (int x = -1; x <= 1100; x++)
        {
            expected[count] = LegacyPixel(camera, 820, x, y) ? (byte)1 : (byte)0;
            actual[count++] = cached.Contains(x, y) ? (byte)1 : (byte)0;
        }
        Assert.Equal(expected, actual);
        Assert.Contains((byte)0, actual); Assert.Contains((byte)1, actual);
    }
    [Theory]
    [InlineData("{\"Size\":0,\"Rotation\":[-0.6,0.5]}")]
    [InlineData("{\"Size\":10,\"Rotation\":[0,0.5]}")]
    [InlineData("{\"Size\":10,\"Rotation\":[-0.6,0.5],\"ReferenceY\":8}")]
    [InlineData("{\"Size\":10,\"Rotation\":[-0.6,0.5],\"ReferenceX\":8}")]
    [InlineData("{\"Size\":10,\"Rotation\":[-0.6,0.5],\"ReferenceX\":\"invalid\",\"ReferenceY\":8}")]
    public void MalformedCameraKeepsValidationAndExceptionContract(string json)
    {
        JsonNode camera = JsonNode.Parse(json)!;
        Exception? before = Record.Exception(() => LegacyPixel(camera, 820, 0, 0));
        Exception? after = Record.Exception(() => LandscapeBoundaryProof.ProtectedPixel(camera, 820, 0, 0));
        Assert.NotNull(before); Assert.NotNull(after);
        Assert.Equal(before.GetType(), after.GetType()); Assert.Equal(before.Message, after.Message);
    }
    [Fact]
    public void ProjectionEdgesKeepTheOriginalMultiplyDivideAndNonfiniteSemantics()
    {
        foreach (float size in new[] { 10f, float.BitIncrement(10f), float.BitDecrement(10f) })
            foreach (int height in new[] { 0, 1, 820 })
            {
                JsonNode camera = JsonNode.Parse("""{"Size":10,"Rotation":[-0.5934119,0.48869216,0],"ReferenceX":488.51974,"ReferenceY":328.86765}""")!;
                camera["Size"] = size;
                var cached = new LandscapeBoundaryProof.ProtectedProjection(camera, height);
                foreach (int x in new[] { int.MinValue, -1, 0, 138, 139, 365, 488, 1099, 1100, int.MaxValue })
                    foreach (int y in new[] { int.MinValue, -1, 0, 299, 328, 573, 819, 820, int.MaxValue })
                        Assert.Equal(LegacyPixel(camera, height, x, y), cached.Contains(x, y));
            }
    }
    // Independent executable reference for the original numeric contract, not
    // a source-text snapshot, coordinate whitelist or native-raster exemption.
    private static bool LegacyPixel(JsonNode camera, int height, int x, int y)
    {
        float size = camera["Size"]!.GetValue<float>();
        float pitch = -camera["Rotation"]![0]!.GetValue<float>(), yaw = camera["Rotation"]![1]!.GetValue<float>();
        if (!float.IsFinite(size) || size <= 0 || pitch is <= 0 or >= 1.5f) throw new InvalidDataException("Invalid controlled camera.");
        double right = (x + .5 - camera["ReferenceX"]!.GetValue<float>()) * size / height;
        double up = -(y + .5 - camera["ReferenceY"]!.GetValue<float>()) * size / height;
        double groundX = 2 + Math.Cos(yaw) * right - Math.Sin(yaw) * up / Math.Sin(pitch);
        double groundZ = -3 - Math.Sin(yaw) * right - Math.Cos(yaw) * up / Math.Sin(pitch);
        return groundX is >= -14 and <= 14 && groundZ is >= -22 and <= 4.34;
    }
}
