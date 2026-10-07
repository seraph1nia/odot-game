using System.Text.Json.Nodes;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class CanonicalBoundaryCaptureTests
{
    private static JsonObject Request(int frame = 399) => new()
    {
        ["TargetFrame"] = frame,
        ["GlyphCompletion"] = true,
        ["BoundaryControls"] = true
    };
    [Fact]
    public async Task ComparedCapturePrecedesGlyphAndControlsAndReplayContinues()
    {
        var events = new List<string>();
        for (int frame = 49; frame <= 599; frame += 50)
        {
            await CanonicalBoundaryCapture.Run(frame, frame == 399 ? Request() : null,
                () => events.Add("compare" + frame),
                () => { events.Add("glyph" + frame); return Task.CompletedTask; },
                () => { events.Add("controls" + frame); return Task.CompletedTask; });
            events.Add("resume" + frame);
        }
        Assert.Equal(new[] { "compare399", "glyph399", "controls399", "resume399", "compare449", "resume449" }, events.Skip(14).Take(6));
        Assert.Equal("resume599", events[^1]);
        Assert.Equal(1, events.Count(e => e.StartsWith("glyph", StringComparison.Ordinal)));
        Assert.Equal(1, events.Count(e => e.StartsWith("controls", StringComparison.Ordinal)));
    }
    [Theory]
    [InlineData(299, 299)]
    [InlineData(399, 299)]
    [InlineData(499, 399)]
    [InlineData(599, 599)]
    public async Task OtherOrStaleBindingsCannotInvokeProducers(int frame, int target)
    {
        bool compared = false, produced = false;
        await Assert.ThrowsAsync<InvalidDataException>(() => CanonicalBoundaryCapture.Run(frame, Request(target),
            () => compared = true, () => { produced = true; return Task.CompletedTask; }, () => { produced = true; return Task.CompletedTask; }));
        Assert.True(compared);
        Assert.False(produced);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task FirstFailureStopsSubsequentProducersAndContinuation(int stage)
    {
        var first = new InvalidDataException("first failure");
        var events = new List<int>();
        bool restored = false;
        void Execute(int current)
        {
            events.Add(current);
            if (stage == current)
            {
                try { throw first; }
                finally { if (current > 0) restored = true; }
            }
        }
        async Task Replay()
        {
            await CanonicalBoundaryCapture.Run(399, Request(), () => Execute(0),
                () => { Execute(1); return Task.CompletedTask; }, () => { Execute(2); return Task.CompletedTask; });
            events.Add(3);
        }
        Assert.Same(first, await Record.ExceptionAsync(Replay));
        Assert.Equal(Enumerable.Range(0, stage + 1), events);
        Assert.Equal(stage > 0, restored);
    }
    [Fact]
    public async Task DefaultBoundRequestDoesNotProduceDiagnostics()
    {
        bool compared = false, produced = false;
        await CanonicalBoundaryCapture.Run(399, new JsonObject { ["TargetFrame"] = 399 }, () => compared = true,
            () => { produced = true; return Task.CompletedTask; }, () => { produced = true; return Task.CompletedTask; });
        Assert.True(compared);
        Assert.False(produced);
    }
    [Fact]
    public async Task WarmupUsesSameProofInputsWithoutProducerFlagsAndMeasuredStillProduces()
    {
        string directory = Path.Combine(Path.GetTempPath(), "canonical-binding-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            JsonObject selected = Request();
            selected["EvidenceHashes"] = new JsonObject { ["immutable-original"] = "exact-original-hash" };
            selected["BeforePng"] = "original-before.png";
            string selectedPath = Path.Combine(directory, "399.json");
            string otherPath = Path.Combine(directory, "299.json");
            File.WriteAllText(selectedPath, selected.ToJsonString());
            File.WriteAllText(otherPath, new JsonObject { ["TargetFrame"] = 299 }.ToJsonString());
            var manifest = new JsonObject
            {
                ["Schema"] = "bounded-native-roof-bindings-v1",
                ["Bindings"] = new JsonArray(
                    new JsonObject { ["Frame"] = 299, ["View"] = "settlement-64", ["Zoom"] = 3, ["Request"] = otherPath },
                    new JsonObject { ["Frame"] = 399, ["View"] = "combat-64", ["Zoom"] = 3, ["Request"] = selectedPath })
            };
            string manifestPath = Path.Combine(directory, "manifest.json");
            File.WriteAllText(manifestPath, manifest.ToJsonString());
            byte[] originalManifest = File.ReadAllBytes(manifestPath), originalRequest = File.ReadAllBytes(selectedPath);
            string warmup = Runner.WarmupBoundaryEvidence(manifestPath, directory)!;
            JsonNode warmupManifest = JsonNode.Parse(File.ReadAllText(warmup))!;
            string warmupRequest = NonDefenseRoofEvidence.BoundRequest(warmupManifest, 399, "combat-64", 3)!;
            JsonNode warmupNode = JsonNode.Parse(File.ReadAllText(warmupRequest))!;
            Assert.False(warmupNode.AsObject().ContainsKey("GlyphCompletion"));
            Assert.False(warmupNode.AsObject().ContainsKey("BoundaryControls"));
            JsonNode expected = selected.DeepClone();
            expected.AsObject().Remove("GlyphCompletion"); expected.AsObject().Remove("BoundaryControls");
            Assert.True(JsonNode.DeepEquals(expected, warmupNode));
            Assert.Equal(otherPath, NonDefenseRoofEvidence.BoundRequest(warmupManifest, 299, "settlement-64", 3));
            int glyphs = 0, controls = 0, comparisons = 0;
            await CanonicalBoundaryCapture.Run(399, warmupNode, () => comparisons++,
                () => { glyphs++; return Task.CompletedTask; }, () => { controls++; return Task.CompletedTask; });
            Assert.Equal(0, glyphs); Assert.Equal(0, controls);
            await CanonicalBoundaryCapture.Run(399, selected, () => comparisons++,
                () => { glyphs++; return Task.CompletedTask; }, () => { controls++; return Task.CompletedTask; });
            Assert.Equal(2, comparisons); Assert.Equal(1, glyphs); Assert.Equal(1, controls);
            Assert.Equal(originalManifest, File.ReadAllBytes(manifestPath));
            Assert.Equal(originalRequest, File.ReadAllBytes(selectedPath));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
