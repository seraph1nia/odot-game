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
    [InlineData(399, 299)]
    [InlineData(399, 499)]
    [InlineData(399, 599)]
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
    [Theory]
    [InlineData(299)]
    [InlineData(499)]
    [InlineData(599)]
    public async Task Non399BoundFlagsRemainInertAfterOrdinaryComparison(int frame)
    {
        var events = new List<string>();
        await CanonicalBoundaryCapture.Run(frame, Request(frame), () => events.Add("compare"),
            () => { events.Add("glyph"); return Task.CompletedTask; }, () => { events.Add("controls"); return Task.CompletedTask; });
        events.Add("resume");
        Assert.Equal(new[] { "compare", "resume" }, events);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FourRetainedBindingsKeepLaterFlagsInertAndDisableOnlySelected399ForWarmup(bool select399)
    {
        using Stream fixture = typeof(CanonicalBoundaryCaptureTests).Assembly.GetManifestResourceStream("DevRunner.Tests.Fixtures.canonical-four-view-selection.json")!;
        JsonNode model = JsonNode.Parse(fixture)!;
        string directory = Path.Combine(Path.GetTempPath(), "canonical-four-bindings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var manifest = new JsonObject { ["Schema"] = model["Schema"]!.DeepClone(), ["Bindings"] = new JsonArray() };
            var originalRequests = new Dictionary<int, byte[]>();
            foreach (JsonNode? row in model["Bindings"]!.AsArray())
            {
                int frame = row!["Frame"]!.GetValue<int>();
                JsonNode request = row["Request"]!.DeepClone();
                if (frame == 399 && select399)
                {
                    request["GlyphCompletion"] = true;
                    request["BoundaryControls"] = true;
                }
                string requestPath = Path.Combine(directory, frame + ".json");
                File.WriteAllText(requestPath, request.ToJsonString());
                originalRequests[frame] = File.ReadAllBytes(requestPath);
                manifest["Bindings"]!.AsArray().Add(new JsonObject
                {
                    ["Frame"] = frame, ["View"] = row["View"]!.DeepClone(), ["Zoom"] = row["Zoom"]!.DeepClone(), ["Request"] = requestPath
                });
            }
            string manifestPath = Path.Combine(directory, "manifest.json");
            File.WriteAllText(manifestPath, manifest.ToJsonString());
            byte[] originalManifest = File.ReadAllBytes(manifestPath);
            string warmupPath = Runner.WarmupBoundaryEvidence(manifestPath, directory)!;
            Assert.Equal(select399, warmupPath != manifestPath);
            JsonNode warmup = JsonNode.Parse(File.ReadAllText(warmupPath))!;
            var events = new List<string>();
            foreach ((string phase, JsonNode bindings) in new[] { ("warmup", warmup), ("measured", (JsonNode)manifest) })
                foreach (JsonNode? row in bindings["Bindings"]!.AsArray())
                {
                    int frame = row!["Frame"]!.GetValue<int>();
                    string path = NonDefenseRoofEvidence.BoundRequest(bindings, frame, row["View"]!.GetValue<string>(), 3)!;
                    JsonNode request = JsonNode.Parse(File.ReadAllText(path))!;
                    JsonNode expected = JsonNode.Parse(originalRequests[frame])!;
                    if (phase == "warmup" && frame == 399 && select399)
                    {
                        expected.AsObject().Remove("GlyphCompletion");
                        expected.AsObject().Remove("BoundaryControls");
                    }
                    else Assert.Equal(Path.Combine(directory, frame + ".json"), path);
                    Assert.True(JsonNode.DeepEquals(expected, request));
                    await CanonicalBoundaryCapture.Run(frame, request, () => events.Add(phase + "compare" + frame),
                        () => { events.Add(phase + "glyph" + frame); return Task.CompletedTask; },
                        () => { events.Add(phase + "controls" + frame); return Task.CompletedTask; });
                    events.Add(phase + "resume" + frame);
                }
            var expectedEvents = new List<string>();
            foreach (string phase in new[] { "warmup", "measured" })
                foreach (int frame in new[] { 299, 399, 499, 599 })
                {
                    expectedEvents.Add(phase + "compare" + frame);
                    if (select399 && phase == "measured" && frame == 399)
                    {
                        expectedEvents.Add(phase + "glyph399");
                        expectedEvents.Add(phase + "controls399");
                    }
                    expectedEvents.Add(phase + "resume" + frame);
                }
            Assert.Equal(expectedEvents, events);
            Assert.Equal(originalManifest, File.ReadAllBytes(manifestPath));
            foreach ((int frame, byte[] bytes) in originalRequests) Assert.Equal(bytes, File.ReadAllBytes(Path.Combine(directory, frame + ".json")));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
    [Theory]
    [InlineData("{\"GlyphCompletion\":true}")]
    [InlineData("{\"TargetFrame\":299,\"GlyphCompletion\":true}")]
    [InlineData("{\"TargetFrame\":499,\"GlyphCompletion\":true}")]
    [InlineData("{\"TargetFrame\":599,\"BoundaryControls\":true}")]
    [InlineData("{\"TargetFrame\":\"399\",\"GlyphCompletion\":true}")]
    [InlineData("{\"TargetFrame\":399,\"GlyphCompletion\":\"true\"}")]
    public async Task Invalid399SelectionRefusesWarmupOutputAndMeasuredProducers(string requestJson)
    {
        string directory = Path.Combine(Path.GetTempPath(), "canonical-invalid-binding-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using Stream fixture = typeof(CanonicalBoundaryCaptureTests).Assembly.GetManifestResourceStream("DevRunner.Tests.Fixtures.canonical-four-view-selection.json")!;
            JsonNode manifest = JsonNode.Parse(fixture)!;
            string selectedPath = Path.Combine(directory, "399.json");
            File.WriteAllText(selectedPath, requestJson);
            foreach (JsonNode? binding in manifest["Bindings"]!.AsArray())
                binding!["Request"] = binding["Frame"]!.GetValue<int>() == 399 ? selectedPath : Path.Combine(directory, binding["Frame"] + ".json");
            string manifestPath = Path.Combine(directory, "manifest.json");
            File.WriteAllText(manifestPath, manifest.ToJsonString());
            Assert.NotNull(Record.Exception(() => Runner.WarmupBoundaryEvidence(manifestPath, directory)));
            Assert.False(File.Exists(Path.Combine(directory, "warmup-frame399-request.json")));
            Assert.False(File.Exists(Path.Combine(directory, "warmup-boundary-evidence.json")));
            bool compared = false, produced = false;
            Assert.NotNull(await Record.ExceptionAsync(() => CanonicalBoundaryCapture.Run(399, JsonNode.Parse(requestJson),
                () => compared = true, () => { produced = true; return Task.CompletedTask; }, () => { produced = true; return Task.CompletedTask; })));
            Assert.True(compared);
            Assert.False(produced);
            Assert.Equal(requestJson, File.ReadAllText(selectedPath));
        }
        finally { Directory.Delete(directory, recursive: true); }
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
