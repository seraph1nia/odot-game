using System.Text.Json.Nodes;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class RoofFamilyEvidenceTests
{
    private static (JsonNode Observation, JsonNode Native, JsonNode Glyph, JsonNode Labels) Fixture(int frame = 299)
    {
        var fields = new JsonObject();
        foreach (string key in new[] { "Units", "Strikes", "Placements", "PlotHeights", "BuildingVariants", "HomeHealth", "CombatTick", "VisualSeconds", "MatchId", "MatchPhase", "Wave", "TurnSerial" }) fields[key] = new JsonArray();
        fields["Camera"] = JsonNode.Parse("""{"Size":10,"Rotation":[-0.5934119,0.48869216,0],"ReferenceX":8,"ReferenceY":8}""");
        fields["Landscape"] = JsonNode.Parse("""{"CoreTerrain":["installed"],"River":[],"Bridge":"bridge","Plots":9,"Static":[]}""");
        JsonNode observation = new JsonObject { ["Frame"] = frame, ["InputDigest"] = "fixed", ["Fields"] = fields };
        JsonNode hit = JsonNode.Parse("""{"Node":"City1/Buildings/Slot2/metal_mine/Mine office/roof_tile_instance_028/tile","Owner":"Slot2","Asset":"buildings/metal_mine.glb","NewDecoration":false,"OpaqueUnmodified":true,"Priority":0,"Material":"roof_blue","PixelEdgeMargin":0.1,"Depth":35.97,"BufferHash":"native-buffer","TriangleWorld":[1,2,3],"TriangleScreen":[4,5,6]}""")!;
        var inputs = new JsonObject(); for (int i = 0; i < 641; i++) inputs["input" + i] = "immutable";
        JsonNode native = new JsonObject
        {
            ["Schema"] = "six-pixel-native-ownership-v1",
            ["Frame"] = frame,
            ["InputDigest"] = "fixed",
            ["ImmutableInputs"] = inputs,
            ["NativeCamera"] = new JsonArray(1, 2, 3),
            ["Near"] = .05,
            ["Far"] = 180,
            ["Pixels"] = new JsonArray(new JsonObject
            {
                ["Pixel"] = new JsonArray(8, 8),
                ["RayOrigin"] = new JsonArray(1, 2, 3),
                ["RayDirection"] = new JsonArray(4, 5, 6),
                ["BeforeCandidates"] = new JsonArray(hit.DeepClone(), hit.DeepClone(), hit.DeepClone()),
                ["AfterCandidates"] = new JsonArray(hit.DeepClone(), hit.DeepClone(), hit.DeepClone()),
                ["Unknown"] = new JsonArray(new JsonObject { ["Node"] = "City1/Buildings/@Label3D@123", ["Reason"] = "glyph" })
            })
        };
        JsonNode labels = JsonNode.Parse("""[{"Node":"City1/Buildings/@Label3D@123","Text":"Metal mine L1","Font":"native-fallback","NoDepthTest":false}]""")!;
        var captures = new JsonArray(); foreach (string name in new[] { "empty", "label0", "label1", "both" })
            captures.Add(new JsonObject
            {
                ["Name"] = name,
                ["AlphaPixels"] = name == "empty" ? 0 : 100,
                ["Width"] = 16,
                ["Height"] = 16,
                ["Samples"] = new JsonArray(new JsonObject { ["Pixel"] = new JsonArray(8, 8), ["Rgba"] = new JsonArray(0, 0, 0, 0), ["NeighborhoodMaximumAlpha"] = 0, ["ChangedFromEmpty"] = false })
            });
        JsonNode glyph = new JsonObject { ["Schema"] = "six-pixel-glyph-coverage-v1", ["Frame"] = frame, ["InputDigest"] = "fixed", ["Restored"] = true, ["CameraExact"] = true, ["ControlsRestored"] = true, ["Labels"] = labels.DeepClone(), ["Captures"] = captures };
        if (frame >= 399)
        {
            glyph["MineTitleIdentity"] = "Slot2/mine-title";
            captures.RemoveAt(3); captures.RemoveAt(2);
        }
        if (frame >= 499)
        {
            labels[0]!["Identity"] = JsonNode.Parse("""{"Matched":true,"Key":"city:1/slot:2/title","City":1,"Slot":2,"Building":"MetalMine"}""");
            glyph["Labels"] = labels.DeepClone();
            glyph["RoleIdentities"] = new JsonArray("city:1/slot:2/title");
            native["GlyphInventory"] = new JsonArray(new JsonObject { ["Node"] = "City1/Buildings/@Label3D@123", ["PossiblePixels"] = new JsonArray(0) });
        }
        return (observation, native, glyph, labels);
    }
    [Theory]
    [InlineData(299)]
    [InlineData(399)]
    [InlineData(499)]
    [InlineData(599)]
    public void SameFamilyEvidenceIsBoundToBothObservationsAndOnlyProvedCoverage(int frame)
    {
        var (o, n, g, l) = Fixture(frame); byte[] before = new byte[16 * 16 * 4], after = (byte[])before.Clone(); after[(8 * 16 + 8) * 4] = 255;
        var evidence = NonDefenseRoofEvidence.Create(o, o, n, n, g, l, 16, 16);
        Assert.True(LandscapeBoundaryProof.Verify(o, o, 16, 16, before, after, evidence) > 0);
        JsonNode stale = o.DeepClone(); stale["Frame"] = frame + 1;
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(stale, stale, 16, 16, before, after, evidence));
        after[(7 * 16 + 8) * 4] = 255;
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(o, o, 16, 16, before, after, evidence));
    }
    [Fact]
    public void DistinctBindingsRefuseStaleViewsDuplicatesAndFutureInheritance()
    {
        JsonNode manifest = JsonNode.Parse("""{"Schema":"bounded-native-roof-bindings-v1","Bindings":[{"Frame":299,"View":"settlement-64","Zoom":3,"Request":"settlement-proof"},{"Frame":399,"View":"combat-64","Zoom":3,"Request":"combat-proof"}]}""")!;
        Assert.Equal("settlement-proof", NonDefenseRoofEvidence.BoundRequest(manifest, 299, "settlement-64", 3));
        Assert.Equal("combat-proof", NonDefenseRoofEvidence.BoundRequest(manifest, 399, "combat-64", 3));
        Assert.Null(NonDefenseRoofEvidence.BoundRequest(manifest, 499, "settlement-128", 3));
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.BoundRequest(manifest, 399, "settlement-64", 3));
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.BoundRequest(manifest, 399, "combat-64", 1));
        manifest["Bindings"]![1]!["Frame"] = 299;
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.BoundRequest(manifest, 299, "settlement-64", 3));
        manifest["Bindings"]![1]!["Frame"] = 499;
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.BoundRequest(manifest, 499, "settlement-128", 3));
    }
    [Fact]
    public void CompletedRemainingBindingsAreFiniteDistinctAndCannotBorrowZoomOrView()
    {
        JsonNode manifest = JsonNode.Parse("""{"Schema":"bounded-native-roof-bindings-v2","Bindings":[{"Frame":299,"View":"settlement-64","Zoom":3,"Request":"proof299"},{"Frame":399,"View":"combat-64","Zoom":3,"Request":"proof399"},{"Frame":499,"View":"settlement-256","Zoom":3,"Request":"proof499"},{"Frame":599,"View":"combat-256","Zoom":3,"Request":"proof599"}]}""")!;
        foreach (var (frame, view) in new[] { (299, "settlement-64"), (399, "combat-64"), (499, "settlement-256"), (599, "combat-256") })
        {
            Assert.Equal("proof" + frame, NonDefenseRoofEvidence.BoundRequest(manifest, frame, view, 3));
            Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.BoundRequest(manifest, frame, view, 1));
            Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.BoundRequest(manifest, frame, "another-view", 3));
        }
        Assert.Null(NonDefenseRoofEvidence.BoundRequest(manifest, 549, "combat-256", 1));
        JsonNode missing = manifest.DeepClone(); missing["Bindings"]!.AsArray().RemoveAt(3);
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.BoundRequest(missing, 499, "settlement-256", 3));
        manifest["Bindings"]![3]!["Frame"] = 499;
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.BoundRequest(manifest, 499, "settlement-256", 3));
        manifest["Bindings"]![3]!["Frame"] = 699;
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.BoundRequest(manifest, 599, "combat-256", 3));
    }
    [Fact]
    public void MineOnlyCoverageCannotBorrowOtherFrameOrUnaccountedLabel()
    {
        var (o, n, g, l) = Fixture(399);
        g["Frame"] = 299;
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.Create(o, o, n, n, g, l, 16, 16));
        g["Frame"] = 399; g["MineTitleIdentity"] = "another-title";
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.Create(o, o, n, n, g, l, 16, 16));
        g["MineTitleIdentity"] = "Slot2/mine-title"; g["Labels"]!.AsArray().Add(l[0]!.DeepClone());
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.Create(o, o, n, n, g, l, 16, 16));
    }
    [Fact]
    public void CompleteThreeLabelSetIncludingProtectedTitleRequiresActualCoverageAndIdentity()
    {
        var (o, n, g, labels) = Fixture(499);
        foreach (int slot in new[] { 3, 5 })
        {
            JsonNode label = labels[0]!.DeepClone(); label["Node"] = "City1/Buildings/@Label3D@" + slot;
            label["Identity"]!["Key"] = $"city:1/slot:{slot}/title"; label["Identity"]!["Slot"] = slot; label["Identity"]!["Protected"] = true;
            labels.AsArray().Add(label); g["Labels"]!.AsArray().Add(label.DeepClone()); g["RoleIdentities"]!.AsArray().Add($"city:1/slot:{slot}/title");
            JsonNode capture = g["Captures"]![1]!.DeepClone(); capture["Name"] = "label" + (labels.AsArray().Count - 1); g["Captures"]!.AsArray().Add(capture);
            n["Pixels"]![0]!["Unknown"]!.AsArray().Add(new JsonObject { ["Node"] = label["Node"]!.GetValue<string>(), ["Reason"] = "glyph" });
        }
        _ = NonDefenseRoofEvidence.Create(o, o, n, n, g, labels, 16, 16);
        g["Captures"]![3]!["Samples"]![0]!["Rgba"]![3] = 255;
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.Create(o, o, n, n, g, labels, 16, 16));
        g["Captures"]![3]!["Samples"]![0]!["Rgba"]![3] = 0; g["Labels"]![2]!["Identity"]!["Matched"] = false; labels[2]!["Identity"]!["Matched"] = false;
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.Create(o, o, n, n, g, labels, 16, 16));
    }
    [Fact]
    public void DarkTileMemberRequiresOwnRemainingFrameBufferAndBeforeIdentity()
    {
        var (o, n, g, labels) = Fixture(499);
        foreach (string set in new[] { "BeforeCandidates", "AfterCandidates" }) foreach (JsonNode? hit in n["Pixels"]![0]![set]!.AsArray()) hit!["Material"] = "roof_blue_dark";
        _ = NonDefenseRoofEvidence.Create(o, o, n, n, g, labels, 16, 16);
        JsonNode stale = n.DeepClone(); stale["Pixels"]![0]!["BeforeCandidates"]![0]!["BufferHash"] = "different";
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.Create(o, o, stale, n, g, labels, 16, 16));
        (o, n, g, labels) = Fixture(299);
        foreach (string set in new[] { "BeforeCandidates", "AfterCandidates" }) foreach (JsonNode? hit in n["Pixels"]![0]![set]!.AsArray()) hit!["Material"] = "roof_blue_dark";
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.Create(o, o, n, n, g, labels, 16, 16));
    }
    [Theory]
    [InlineData(499)]
    [InlineData(599)]
    public void ZeroPossibleGlyphsRequireEmptyCompleteNativeCandidateInventory(int frame)
    {
        var (o, n, g, _) = Fixture(frame);
        n["Pixels"]![0]!["Unknown"] = new JsonArray(); n["GlyphInventory"]![0]!["PossiblePixels"] = new JsonArray();
        g["Labels"] = new JsonArray(); g["RoleIdentities"] = new JsonArray(); g["Captures"] = new JsonArray(); g["NoPossibleGlyphs"] = true;
        var proof = NonDefenseRoofEvidence.Create(o, o, n, n, g, new JsonArray(), 16, 16);
        byte[] before = new byte[16 * 16 * 4], after = (byte[])before.Clone(); after[(8 * 16 + 8) * 4] = 1;
        Assert.True(LandscapeBoundaryProof.Verify(o, o, 16, 16, before, after, proof) > 0);
        n["Pixels"]![0]!["Unknown"]!.AsArray().Add(new JsonObject { ["Node"] = "City1/UnknownSprite" });
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.Create(o, o, n, n, g, new JsonArray(), 16, 16));
    }
    [Fact]
    public void ChangedPointDiscoveryUsesRealProtectedBytesAndRefusesStateDrift()
    {
        var (o, _, _, _) = Fixture(); byte[] before = new byte[16 * 16 * 4], after = (byte[])before.Clone(); after[(8 * 16 + 8) * 4] = 2;
        Assert.Collection(Assert.Single(LandscapeBoundaryProof.ChangedProtectedPixels(o, o, 16, 16, before, after)), x => Assert.Equal(8, x), y => Assert.Equal(8, y));
        Assert.Empty(LandscapeBoundaryProof.ChangedProtectedPixels(o, o, 16, 16, before, before));
        JsonNode drift = o.DeepClone(); drift["Fields"]!["Units"]!.AsArray().Add(new JsonObject { ["Id"] = 99 });
        Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.ChangedProtectedPixels(o, drift, 16, 16, before, after));
    }
    [Theory]
    [InlineData("new", 299)]
    [InlineData("new", 399)]
    [InlineData("new", 499)]
    [InlineData("new", 599)]
    [InlineData("tower", 299)]
    [InlineData("tower", 399)]
    [InlineData("tower", 499)]
    [InlineData("tower", 599)]
    [InlineData("unit", 299)]
    [InlineData("unit", 399)]
    [InlineData("depth", 299)]
    [InlineData("depth", 399)]
    [InlineData("buffer", 299)]
    [InlineData("buffer", 399)]
    [InlineData("buffer", 499)]
    [InlineData("buffer", 599)]
    [InlineData("projection", 299)]
    [InlineData("projection", 399)]
    [InlineData("source", 299)]
    [InlineData("source", 399)]
    [InlineData("source", 499)]
    [InlineData("source", 599)]
    [InlineData("glyph", 299)]
    [InlineData("glyph", 399)]
    [InlineData("glyph", 499)]
    [InlineData("glyph", 599)]
    [InlineData("glyph-state", 299)]
    [InlineData("glyph-state", 399)]
    [InlineData("unknown", 299)]
    [InlineData("unknown", 399)]
    [InlineData("before-protected", 299)]
    [InlineData("before-protected", 399)]
    [InlineData("before-protected", 499)]
    [InlineData("before-protected", 599)]
    public void ChangedOrAmbiguousEvidenceRefuses(string mutation, int frame)
    {
        var (o, n, g, l) = Fixture(frame); JsonNode fresh = n.DeepClone(); JsonNode hit = fresh["Pixels"]![0]!["AfterCandidates"]![0]!;
        switch (mutation)
        {
            case "new": hit["NewDecoration"] = true; break;
            case "tower": hit["Asset"] = "buildings/tower.glb"; break;
            case "unit": hit["Owner"] = "unit:1"; break;
            case "depth": hit["Depth"] = 35; break;
            case "buffer": hit["BufferHash"] = "different"; break;
            case "projection": fresh["NativeCamera"]![0] = 42; break;
            case "source": fresh["ImmutableInputs"]!["input0"] = "different"; break;
            case "glyph": g["Captures"]![1]!["Samples"]![0]!["Rgba"]![3] = 1; break;
            case "glyph-state": l[0]!["NoDepthTest"] = true; break;
            case "unknown": fresh["Pixels"]![0]!["Unknown"]!.AsArray().Add(new JsonObject { ["Node"] = "City1/ProtectedEffect" }); break;
            case "before-protected": n["Pixels"]![0]!["BeforeCandidates"]![0]!["Asset"] = "terrain"; break;
        }
        Assert.Throws<InvalidDataException>(() => NonDefenseRoofEvidence.Create(o, o, n, fresh, g, l, 16, 16));
    }
}
