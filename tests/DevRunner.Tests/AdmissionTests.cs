using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class AdmissionTests
{
    [Fact]
    public async Task NetworkAndGraphicalSchedulersShareBudgetAndAwaitCleanup()
    {
        using var budget = new ScenarioAdmission(2, 1);
        var admitted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int active = 0, maximum = 0, completed = 0;
        async Task Run(CancellationToken token)
        {
            int count = Interlocked.Increment(ref active);
            Interlocked.Exchange(ref maximum, Math.Max(maximum, count));
            if (count == 2) admitted.TrySetResult();
            try { await release.Task.WaitAsync(token); }
            finally { Interlocked.Decrement(ref active); Interlocked.Increment(ref completed); }
        }
        Scenario[] scenarios = Enumerable.Range(0, 3).Select(i => new Scenario(i.ToString(System.Globalization.CultureInfo.InvariantCulture), "fixture", Run)).ToArray();
        Task network = ScenarioScheduler.Run(scenarios, 2, CancellationToken.None, budget);
        Task graphics = ScenarioScheduler.Run(scenarios, 2, CancellationToken.None, budget, graphical: true);
        await admitted.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(2, active);
        release.SetResult();
        await Task.WhenAll(network, graphics);
        Assert.Equal(2, maximum); Assert.Equal(2, budget.MaximumActive); Assert.Equal(1, budget.MaximumGraphical); Assert.Equal(6, completed); Assert.Equal(0, active);
    }
    [Fact]
    public async Task ParentOwnsGraphicalRuntimeAfterAbruptWorkerExit()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-nested-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var evidence = new Evidence(root);
            await using var parent = new ScenarioScope("display", evidence);
            await using var nested = new ScenarioScope("graphics", evidence, graphical: true, runtimeRoot: parent.Directory);
            Assert.StartsWith(parent.Directory + Path.DirectorySeparatorChar, nested.Directory, StringComparison.Ordinal);
            string data = nested.EnvironmentFor("client")["XDG_DATA_HOME"]!;
            await File.WriteAllTextAsync(Path.Combine(data, "fixture.cfg"), "owned fixture");
            Child worker = parent.Own(new Child("worker", "setsid", ["/bin/sh", "-c", "printf 'ODOT_EVENT {\"Type\":\"ready\"}\\n'; sleep 30 &"], root, ownsGroup: true));
            await worker.WaitFor(e => e.Type == "ready", "nested fixture", 2000, CancellationToken.None);
            await parent.DisposeAsync();
            Assert.True(worker.HasExited);
            Assert.False(Directory.Exists(nested.Directory));
            // The worker need not reach its own finally for the parent to reclaim it.
            await nested.DisposeAsync();
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData("test-ui", "--jobs", "3")]
    [InlineData("ci", "--ui-jobs", "0")]
    [InlineData("ci", "--ui-jobs", "3")]
    [InlineData("play", "--simulation-speed", "4")]
    public void InvalidAdmissionAndPacingOptionsFail(string command, string option, string value)
        => Assert.Throws<ArgumentException>(() => Options.Parse([command, option, value]));
    [Fact]
    public void SerialOverridesAndPackageTimeoutRemainAvailable()
    {
        Assert.Equal(2, Options.Parse(["test-ui"]).Jobs);
        Assert.Equal(1, Options.Parse(["test-ui", "--jobs", "1"]).Jobs);
        Assert.Equal(1, Options.Parse(["ci", "--jobs", "1", "--ui-jobs", "1"]).UiJobs);
        Assert.Equal(4, Options.Parse(["ci"]).SimulationSpeed);
        Assert.Equal(300000, Options.Parse(["ci-linux-package"]).Timeout);
        Assert.Equal(1, Options.Parse(["test-network", "--simulation-speed", "1"]).SimulationSpeed);
    }
}
