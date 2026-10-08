using System.Text.Json.Nodes;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class NativeBoundaryControlReceiptTests
{
    [Theory]
    [InlineData(299)]
    [InlineData(399)]
    [InlineData(499)]
    [InlineData(599)]
    public void ExactRestorationBindsActualCapturedFrame(int frame)
    {
        string directory = Path.Combine(Path.GetTempPath(), "native-control-receipt-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            JsonNode current = new JsonObject
            {
                ["InputDigest"] = "bound-input",
                ["Frame"] = frame,
                ["Fields"] = JsonNode.Parse("""{"Camera":{"Size":10},"Units":[]}""")
            };
            string output = Path.Combine(directory, "control");
            bool exact = NativeBoundaryControls.WriteRestorationReceipt(current, current["Fields"]!, Enumerable.Range(0, 5).Select(i => (object)new { Name = i }).ToArray(), output, null);
            Assert.True(exact);
            NativeBoundaryControls.RequireComplete(5, exact);
            JsonNode receipt = JsonNode.Parse(File.ReadAllText(output + ".controls.json"))!;
            Assert.True(receipt["Complete"]!.GetValue<bool>());
            Assert.True(receipt["ExactObservedRestoration"]!.GetValue<bool>());
            Assert.Contains($"frame{frame}/camera/light", receipt["Method"]!.GetValue<string>());
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
    [Theory]
    [InlineData(5, false)]
    [InlineData(0, true)]
    [InlineData(4, true)]
    [InlineData(6, true)]
    public void SuccessfulTerminalResultRequiresAllControlsAndExactRestoration(int count, bool exact)
        => Assert.Throws<InvalidDataException>(() => NativeBoundaryControls.RequireComplete(count, exact));
    [Fact]
    public void AFailedFinalObservationCannotReplaceTheFirstControlFailure()
    {
        var first = new InvalidDataException("original control failure");
        var final = new IOException("final observation failure");
        Exception? reported = null;
        Func<object> observer = () => throw final;
        Action execute = () =>
        {
            try { throw first; }
            finally { NativeBoundaryControls.FinishRestoration(new JsonObject(), observer, [], "unused", first, e => reported = e); }
        };
        Exception? actual = Record.Exception(execute);
        Assert.Same(first, actual);
        Assert.Same(final, reported);
        Assert.Throws<IOException>(() => NativeBoundaryControls.FinishRestoration(new JsonObject(), observer, [], "unused", null, _ => { }));
    }
    private static JsonObject RasterObservation()
    {
        var fields = new JsonObject();
        foreach (string key in new[] { "Units", "Strikes", "Placements", "PlotHeights", "BuildingVariants", "HomeHealth", "CombatTick", "VisualSeconds", "MatchId", "MatchPhase", "Wave", "TurnSerial" }) fields[key] = new JsonArray();
        fields["Camera"] = JsonNode.Parse("""{"Size":10,"Rotation":[-0.5934119,0.48869216,0],"ReferenceX":8,"ReferenceY":8}""");
        fields["Landscape"] = JsonNode.Parse("""{"CoreTerrain":[],"River":[],"Bridge":"bridge","Plots":9,"Static":[]}""");
        return new JsonObject { ["InputDigest"] = "bound-input", ["Frame"] = 499, ["Fields"] = fields };
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShadowControlIsCompletedOnlyAfterRestoredRasterVerification(bool rasterMatches)
    {
        JsonNode current = RasterObservation();
        byte[] before = new byte[16 * 16 * 4], restored = (byte[])before.Clone();
        if (!rasterMatches) restored[(8 * 16 + 8) * 4] = 255;
        var results = Enumerable.Range(0, 4).Select(i => (object)new { Name = i }).ToList();
        var shadow = new { Name = "real-new-shadow-on-protected-receiver", Result = "rejected" };
        Action verifyRestoration = () => LandscapeBoundaryProof.Verify(current, current, 16, 16, before, restored);
        Exception? failure = Record.Exception(() => NativeBoundaryControls.CompleteControl(results, shadow, verifyRestoration));
        Assert.Equal(rasterMatches ? 5 : 4, results.Count);
        if (rasterMatches)
        {
            Assert.Null(failure);
            Assert.Same(shadow, results[^1]);
        }
        else
        {
            var rasterFailure = Assert.IsType<InvalidDataException>(failure);
            Assert.Equal("Battle pixel changed at (8,8); non-battle differences are not a global waiver.", rasterFailure.Message);
            Assert.DoesNotContain(shadow, results);
        }
    }
    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public void PendingRasterFailureKeepsExactRestorationReceiptIncompleteAndPreservesException(int count)
    {
        string directory = Path.Combine(Path.GetTempPath(), "native-control-failure-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            JsonNode current = RasterObservation();
            byte[] before = new byte[16 * 16 * 4], restored = (byte[])before.Clone();
            restored[(8 * 16 + 8) * 4] = 255;
            var first = Assert.Throws<InvalidDataException>(() => LandscapeBoundaryProof.Verify(current, current, 16, 16, before, restored));
            object[] results = Enumerable.Range(0, count).Select(i => (object)new { Name = i, Result = "rejected" }).ToArray();
            string output = Path.Combine(directory, "control");
            bool exact = false;
            Exception? reported = null;
            Action execute = () =>
            {
                try { throw first; }
                finally { exact = NativeBoundaryControls.FinishRestoration(current, () => current["Fields"]!, results, output, first, e => reported = e); }
            };
            Assert.Same(first, Record.Exception(execute));
            Assert.Null(reported);
            Assert.True(exact);
            JsonNode receipt = JsonNode.Parse(File.ReadAllText(output + ".controls.json"))!;
            Assert.True(receipt["ExactObservedRestoration"]!.GetValue<bool>());
            Assert.Equal(count, receipt["Results"]!.AsArray().Count);
            Assert.False(receipt["Complete"]!.GetValue<bool>());
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
    [Fact]
    public void FailedRestorationKeepsNegativeReceiptAndRefusesSuccess()
    {
        string directory = Path.Combine(Path.GetTempPath(), "native-control-negative-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            JsonNode current = JsonNode.Parse("""{"InputDigest":"bound-input","Frame":299,"Fields":{"UnitMaterial":"original"}}""")!;
            bool exact = NativeBoundaryControls.WriteRestorationReceipt(current, JsonNode.Parse("""{"UnitMaterial":"changed"}""")!, new object[5], Path.Combine(directory, "control"), null);
            Assert.False(exact);
            Assert.Throws<InvalidDataException>(() => NativeBoundaryControls.RequireComplete(5, exact));
            Assert.False(JsonNode.Parse(File.ReadAllText(Path.Combine(directory, "control.controls.json")))!["Complete"]!.GetValue<bool>());
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
