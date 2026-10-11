using System.Xml.Linq;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task CheapTests()
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(options.Timeout);
        bool campaign = options.Command == "test-campaign";
        string tier = campaign ? "Tier=Deep" : "Tier!=Deep";
        string filter = options.Filter is null ? tier : $"({tier})&({options.Filter})";
        var checks = new List<(string Name, string Project)>();
        if (options.Command != "test-runner") checks.Add(("core", "tests/Game.Core.Tests/Game.Core.Tests.csproj"));
        if (options.Command is not ("test-core" or "test-campaign")) checks.Add(("runner", "tests/DevRunner.Tests/DevRunner.Tests.csproj"));
        int executed = 0;
        var scenarios = checks.Select(check => new Scenario(check.Name, "Isolated completed C# binary; no build/import mutation", async token =>
        {
            var worker = new Runner(options, token, _evidence, root: _root);
            string results = Path.Combine(_evidence.Directory, check.Name + "-results");
            await worker.Execute(check.Name, "dotnet", "test", check.Project, "--no-restore", "--no-build", "--nologo",
                "--filter", filter, "--logger", "trx;LogFileName=tests.trx", "--results-directory", results);
            var document = XDocument.Load(Path.Combine(results, "tests.trx"));
            var counters = document.Descendants().Single(e => e.Name.LocalName == "Counters");
            int count = (int?)counters.Attribute("executed") ?? 0;
            Interlocked.Add(ref executed, count);
            Console.WriteLine($"C# {check.Name}: executed={count}; filter={filter}");
        })).ToArray();
        Console.WriteLine($"C# coverage: {(campaign ? "deep campaigns" : "fast (excludes Tier=Deep)")}; filter={filter}; jobs={options.Jobs}; {string.Join(", ", checks.Select(c => c.Name))}.");
        await _evidence.Measure("rules", "suite", async () =>
        {
            try { await ScenarioScheduler.Run(scenarios, options.Jobs, deadline.Token); }
            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
            { throw new TimeoutException($"C# checks exceeded {options.Timeout} ms; owned test hosts were cleaned up."); }
            if (executed == 0) throw new InvalidOperationException($"Filter '{filter}' executed zero tests across the selected projects.");
        });
    }
}
