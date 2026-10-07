using System.Security.Cryptography;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task PresentationProfiles()
    {
        using var total = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        total.CancelAfter(TimeSpan.FromSeconds(600 * (options.ProfileIterations + 1)));
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
            Coverage = options.PixelOwnershipRequest is null ? "selected diagnostic replay; not default UI or native GPU performance" : "fixed frame299 investigation only; no full600-frame coverage"
        }, WireJson.Options), total.Token);
        var serial = new Runner(options with { SimulationSpeed = 1 }, total.Token, _evidence, root: _root);
        if (options.PixelOwnershipRequest is not null)
        {
            await serial.UiDisplay("authored-scale", total.Token, "six-pixel-ownership");
            return;
        }
        for (int iteration = 0; iteration <= options.ProfileIterations; iteration++)
        {
            string name = options.Scenario + "-" + (iteration == 0 ? "warmup" : "iteration-" + iteration);
            await serial.UiDisplay(options.Scenario!, total.Token, name);
        }
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
        Child game = StartGameRole("presentation-replay", "solo", false, 0, extra: extra.ToArray());
        int code = await game.WaitExit(token);
        if (code != 0 || game.HasEngineErrors) throw new InvalidOperationException($"Presentation replay failed ({code}).\n{game.Tail()}");
        if (options.PixelOwnershipRequest is not null)
        {
            if (!File.Exists(outputPath + ".ownership.json")) throw new InvalidOperationException("Missing six-pixel ownership witness.");
            Console.WriteLine("Native ownership inspection only; no replay measurement or acceptance: " + outputPath + ".ownership.json");
            return;
        }
        using JsonDocument result = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath, token));
        if (result.RootElement.GetProperty("Frames").GetInt32() != 600 || result.RootElement.GetProperty("InputDigest").GetString() != input.Digest
            || !File.Exists(outputPath + ".png")) throw new InvalidOperationException("Presentation profile has incomplete frame/input/capture evidence.");
        Console.WriteLine($"Presentation profile: {input.Digest}, 600 frames; {outputPath}");
    }
}
