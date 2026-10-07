using System.Text.Json.Nodes;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class LandscapeBoundaryProofTests
{
    private static JsonObject Observation()
    {
        var fields = new JsonObject();
        foreach (string key in new[] { "Units", "Strikes", "Placements", "PlotHeights", "BuildingVariants", "HomeHealth", "CombatTick", "VisualSeconds", "MatchId", "MatchPhase", "Wave", "TurnSerial" }) fields[key] = new JsonArray();
        fields["Camera"] = JsonNode.Parse("""{"Size":10,"Rotation":[-0.5934119,0.48869216,0],"ReferenceX":8,"ReferenceY":8}""");
        fields["Landscape"] = JsonNode.Parse("""{"CoreTerrain":["installed"],"River":[],"Bridge":"bridge","Plots":9,"Static":[{"Asset":"home","X":1.5,"Y":0,"Z":2.598}]}""");
        return new JsonObject { ["InputDigest"] = "same-input", ["Frame"] = 99, ["Fields"] = fields };
    }
    [Fact]
    public void ChangedVillagePixelsDoNotWaiveAnyBattlePixel()
    {
        JsonNode before = Observation(), after = before.DeepClone();
        byte[] pixels = new byte[16 * 16 * 4], changed = (byte[])pixels.Clone();
        Assert.True(LandscapeBoundaryProof.ProtectedPixel(after["Fields"]!["Camera"]!, 16, 8, 8));
        Assert.False(LandscapeBoundaryProof.ProtectedPixel(after["Fields"]!["Camera"]!, 16, 0, 15));
        changed[(15 * 16) * 4] = 255;
        Assert.True(LandscapeBoundaryProof.Verify(before, after, 16, 16, pixels, changed) > 0);
        changed[(8 * 16 + 8) * 4] = 255;
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(before, after, 16, 16, pixels, changed));
    }
    [Theory]
    [InlineData("Camera")]
    [InlineData("Units")]
    [InlineData("Placements")]
    [InlineData("CombatTick")]
    public void MatchingPixelsCannotHideChangedControlState(string field)
    {
        JsonNode before = Observation(), after = before.DeepClone(); byte[] pixels = new byte[16 * 16 * 4];
        after["Fields"]![field] = 999;
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(before, after, 16, 16, pixels, pixels));
    }
    [Fact]
    public void OnlyEphemeralNativeCountersAreIgnoredNotPhysicalOrSlotIdentity()
    {
        JsonNode before = Observation(), after = before.DeepClone(); byte[] pixels = new byte[16 * 16 * 4];
        before["Fields"]!["Placements"] = JsonNode.Parse("""[{"Name":"@Node3D@12","Asset":"crate","X":7,"Y":0,"Z":2}]""");
        after["Fields"]!["Placements"] = JsonNode.Parse("""[{"Name":"@Node3D@99","Asset":"crate","X":7,"Y":0,"Z":2}]""");
        Assert.True(LandscapeBoundaryProof.Verify(before, after, 16, 16, pixels, pixels) > 0);
        after["Fields"]!["Placements"]![0]!["X"] = 8;
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(before, after, 16, 16, pixels, pixels));
        before["Fields"]!["Placements"]![0]!["Name"] = "Slot0";
        after["Fields"]!["Placements"] = before["Fields"]!["Placements"]!.DeepClone();
        after["Fields"]!["Placements"]![0]!["Name"] = "Slot1";
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(before, after, 16, 16, pixels, pixels));
    }
    [Fact]
    public void MissingFramesNewBattlePropsAndChangedInputFailClosed()
    {
        JsonNode before = Observation(), after = before.DeepClone(); byte[] pixels = new byte[16 * 16 * 4];
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(before, after, 16, 16, [], pixels));
        after["InputDigest"] = "different";
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(before, after, 16, 16, pixels, pixels));
        after = before.DeepClone(); after["Fields"]!["Landscape"]!["Static"]![0]!["Pocket"] = "new canopy";
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(before, after, 16, 16, pixels, pixels));
        after = before.DeepClone(); after["Fields"]!.AsObject().Remove("Units");
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(before, after, 16, 16, pixels, pixels));
    }
}
