using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task PresentationProfiles()
    {
        using var total = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        total.CancelAfter(TimeSpan.FromSeconds(600 * (options.ProfileIterations + 1)));
        await WritePresentationIdentity(total.Token);
        var serial = new Runner(options with { SimulationSpeed = 1 }, total.Token, _evidence, root: _root);
        if (options.PixelOwnershipRequest is not null)
        {
            await serial.UiDisplay("authored-scale", total.Token, "six-pixel-ownership");
            await WritePresentationIdentity(total.Token, completed: true);
            return;
        }
        string? warmupEvidence = WarmupBoundaryEvidence(options.BoundaryEvidence, _evidence.Directory);
        for (int iteration = 0; iteration <= options.ProfileIterations; iteration++)
        {
            string name = options.Scenario + "-" + (iteration == 0 ? "warmup" : "iteration-" + iteration);
            var execution = iteration == 0 ? new Runner(options with { SimulationSpeed = 1, BoundaryEvidence = warmupEvidence }, total.Token, _evidence, root: _root) : serial;
            await execution.UiDisplay(options.Scenario!, total.Token, name);
        }
    }
    internal static string? WarmupBoundaryEvidence(string? manifestPath, string directory)
    {
        if (manifestPath is null) return null;
        JsonNode manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
        if (manifest["Bindings"] is null) return manifestPath;
        string? selected = Game.NonDefenseRoofEvidence.BoundRequest(manifest, 399, "combat-64", 3);
        if (selected is null) return manifestPath;
        JsonNode selectedRequest = JsonNode.Parse(File.ReadAllText(selected))!;
        if (selectedRequest["GlyphCompletion"]?.GetValue<bool>() != true && selectedRequest["BoundaryControls"]?.GetValue<bool>() != true) return manifestPath;
        if (selectedRequest["TargetFrame"]?.GetValue<int>() != 399)
            throw new InvalidDataException("Canonical producers require the independently bound frame399 request.");
        JsonNode selectedBinding = manifest["Bindings"]!.AsArray().Single(binding => binding!["Frame"]!.GetValue<int>() == 399)!;
        selectedRequest.AsObject().Remove("GlyphCompletion");
        selectedRequest.AsObject().Remove("BoundaryControls");
        string warmupRequest = Path.Combine(directory, "warmup-frame399-request.json");
        File.WriteAllText(warmupRequest, selectedRequest.ToJsonString(WireJson.Options));
        selectedBinding["Request"] = warmupRequest;
        string warmupManifest = Path.Combine(directory, "warmup-boundary-evidence.json");
        File.WriteAllText(warmupManifest, manifest.ToJsonString(WireJson.Options));
        return warmupManifest;
    }
    internal async Task WritePresentationIdentity(CancellationToken token, bool completed = false)
    {
        JsonNode? request = options.PixelOwnershipRequest is null ? null : JsonNode.Parse(await File.ReadAllTextAsync(options.PixelOwnershipRequest, token));
        int? targetFrame = options.PixelOwnershipRequest is null ? null : request?["TargetFrame"]?.GetValue<int>() ?? 299;
        bool remainingViews = request?["RemainingViews"]?.GetValue<bool>() == true;
        int requestedFrames = targetFrame is null || remainingViews ? options.ProfileFrames : targetFrame.Value + 1;
        int? executedFrames = null;
        if (completed && targetFrame is not null)
        {
            string receiptPath = Path.Combine(_evidence.Directory, "six-pixel-ownership-worker", "authored-scale", "profile.json" + (remainingViews ? "" : ".prefix.json"));
            using JsonDocument receipt = JsonDocument.Parse(await File.ReadAllTextAsync(receiptPath, token));
            executedFrames = receipt.RootElement.GetProperty("Frames").GetInt32();
            if (executedFrames != requestedFrames) throw new InvalidDataException("Investigation receipt does not cover the requested scripted frames.");
        }
        await File.WriteAllTextAsync(Path.Combine(_evidence.Directory, "profile-identity.json"), JsonSerializer.Serialize(new
        {
            Schema = "odot-presentation-run-v1",
            Configuration = options.ProfileConfiguration,
            options.ProfileFrames,
            options.ProfileIterations,
            options.ProfileWorkCounters,
            options.ProfileBaseline,
            options.PixelOwnershipRequest,
            options.BoundaryEvidence,
            TargetFrame = targetFrame,
            RemainingViews = remainingViews,
            RequestedScriptedFrames = requestedFrames,
            ExecutedScriptedFrames = executedFrames,
            Mode = options.PixelOwnershipRequest is null ? "600-frame measurement" : "bounded native investigation; request defines authorized samples; not acceptance",
            WarmupExecutions = options.PixelOwnershipRequest is null ? 1 : 0,
            Concurrency = 1,
            PerExecutionBoundSeconds = 600,
            OverallBoundSeconds = 600 * (options.ProfileIterations + 1),
            Sdk = File.ReadAllText(Path.Combine(_root, "global.json")),
            Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            Machine = Environment.MachineName,
            Os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            LogicalProcessors = Environment.ProcessorCount,
            Inputs = PresentationSourceInputs(),
            Coverage = targetFrame is null ? "selected diagnostic replay; not default UI or native GPU performance"
                : $"{(executedFrames is null ? "requested" : "executed")} ordinary rendered frames0..{requestedFrames - 1} ({requestedFrames} scripted frames); {(executedFrames is null ? "execution unconfirmed; " : "")}investigation only; not complete replay acceptance"
        }, WireJson.Options), token);
    }
    private SortedDictionary<string, string> PresentationSourceInputs()
    {
        var inputs = new SortedDictionary<string, string>(StringComparer.Ordinal);
        void Visit(string directory)
        {
            foreach (string path in Directory.EnumerateFiles(directory)) inputs[Path.GetRelativePath(_root, path)] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
            foreach (string child in Directory.EnumerateDirectories(directory))
                if (Path.GetFileName(child) is not ("bin" or "obj" or ".godot")) Visit(child);
        }
        foreach (string area in new[] { "src", "tests", "tools", ".github", ".mise" }) Visit(Path.Combine(_root, area));
        foreach (string file in new[] { "mise.toml", "mise.lock", "global.json", "Directory.Build.props", ".editorconfig", "Odot.slnx" })
            inputs[file] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(_root, file))));
        return inputs;
    }
    internal static string PresentationOwnershipWitness(string outputPath, JsonNode request, CombatReplayInput input)
    {
        if (File.Exists(outputPath + ".ownership.json")) return outputPath + ".ownership.json";
        const string missing = "Missing or inconsistent six-pixel ownership witness.";
        if (request["TargetFrame"]?.GetValue<int>() != 599 || request["RemainingViews"]?.GetValue<bool>() != true
            || request["RestorationUnion"]?.GetValue<bool>() == true || request["BoundaryControls"]?.GetValue<bool>() == true)
            throw new InvalidOperationException(missing);
        foreach (string suffix in new[] { "", ".png", ".observation.json", ".remaining-proof.json", ".remaining-differences.json" })
            if (!File.Exists(outputPath + suffix)) throw new InvalidOperationException(missing);
        JsonNode result = JsonNode.Parse(File.ReadAllText(outputPath))!;
        JsonNode observation = JsonNode.Parse(File.ReadAllText(outputPath + ".observation.json"))!;
        JsonNode proof = JsonNode.Parse(File.ReadAllText(outputPath + ".remaining-proof.json"))!;
        JsonNode differences = JsonNode.Parse(File.ReadAllText(outputPath + ".remaining-differences.json"))!;
        if (result["Schema"]?.GetValue<string>() != "odot-presentation-profile-v1" || result["Frames"]?.GetValue<int>() != 600
            || result["InputDigest"]?.GetValue<string>() != input.Digest || observation["InputDigest"]?.GetValue<string>() != input.Digest
            || observation["Frame"]?.GetValue<int>() != 599 || proof["Frame"]?.GetValue<int>() != 599 || differences["Frame"]?.GetValue<int>() != 599
            || proof["View"]?.GetValue<string>() != input.Frames[599].View || differences["View"]?.GetValue<string>() != input.Frames[599].View
            || proof["ChangedProtectedPixels"]?.GetValue<int>() != 0 || proof["ProtectedPixels"]?.GetValue<int>() is not > 0
            || proof["Result"]?.GetValue<string>() != "exact unchanged; no exclusion"
            || differences["Pixels"] is not JsonArray { Count: 0 } || differences["Differences"] is not JsonArray { Count: 0 })
            throw new InvalidOperationException(missing);
        return outputPath + ".remaining-proof.json";
    }
    internal static void RequireFreshOwnershipOutput(string outputPath)
    {
        if (Directory.EnumerateFiles(Path.GetDirectoryName(outputPath)!, Path.GetFileName(outputPath) + "*").Any())
            throw new InvalidOperationException("Ownership investigation output is not fresh.");
    }
    internal void RequireUnchangedOwnershipInputs(string requestPath, string requestJson, IReadOnlyDictionary<string, string> sourceInputs)
    {
        if (File.ReadAllText(requestPath) != requestJson || !JsonNode.DeepEquals(JsonSerializer.SerializeToNode(sourceInputs), JsonSerializer.SerializeToNode(PresentationSourceInputs())))
            throw new InvalidOperationException("Ownership investigation request/source inputs changed during execution.");
    }
    private async Task PresentationProfileScenario(CancellationToken token)
    {
        if (_scope is null || !_scope.Graphical) throw new InvalidOperationException("Presentation profiling requires an owned display and storage.");
        CombatReplayInput input = options.Scenario == "authored-scale" ? CombatReplayFixture.GenerateAuthored() : CombatReplayFixture.Generate();
        string inputPath = Path.Combine(_scope.EvidenceDirectory, "replay-input.json");
        string outputPath = Path.Combine(_scope.EvidenceDirectory, "profile.json");
        await File.WriteAllTextAsync(inputPath, JsonSerializer.Serialize(input, WireJson.Options), token);
        var extra = new List<string> { "--profile-replay-input", inputPath, "--profile-replay-output", outputPath };
        if (options.ProfileWorkCounters) extra.Add("--profile-work-counters");
        if (options.ProfileBaseline is not null) extra.AddRange(["--profile-replay-baseline", options.ProfileBaseline]);
        if (options.BoundaryEvidence is not null) extra.AddRange(["--profile-boundary-evidence", options.BoundaryEvidence]);
        if (options.PixelOwnershipRequest is not null) extra.AddRange(["--pixel-ownership-request", options.PixelOwnershipRequest]);
        string? ownershipRequest = null;
        IReadOnlyDictionary<string, string>? ownershipSources = null;
        if (options.PixelOwnershipRequest is not null)
        {
            RequireFreshOwnershipOutput(outputPath);
            ownershipRequest = await File.ReadAllTextAsync(options.PixelOwnershipRequest, token);
            ownershipSources = PresentationSourceInputs();
        }
        Child game = StartGameRole("presentation-replay", "solo", false, 0, extra: extra.ToArray());
        int code = await game.WaitExit(token);
        if (code != 0 || game.HasEngineErrors) throw new InvalidOperationException($"Presentation replay failed ({code}).\n{game.Tail()}");
        if (options.PixelOwnershipRequest is not null)
        {
            RequireUnchangedOwnershipInputs(options.PixelOwnershipRequest, ownershipRequest!, ownershipSources!);
            string witness = PresentationOwnershipWitness(outputPath, JsonNode.Parse(ownershipRequest!)!, input);
            Console.WriteLine("Native ownership inspection only; no replay measurement or acceptance: " + witness);
            return;
        }
        using JsonDocument result = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath, token));
        if (result.RootElement.GetProperty("Frames").GetInt32() != 600 || result.RootElement.GetProperty("InputDigest").GetString() != input.Digest
            || !File.Exists(outputPath + ".png")) throw new InvalidOperationException("Presentation profile has incomplete frame/input/capture evidence.");
        Console.WriteLine($"Presentation profile: {input.Digest}, 600 frames; {outputPath}");
    }
}
