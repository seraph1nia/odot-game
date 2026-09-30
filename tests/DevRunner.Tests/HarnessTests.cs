using System.Diagnostics;
using System.Globalization;
using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class HarnessTests
{
    [Fact]
    public void NumericRunnerArgumentsUseInvariantCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        var culture = (CultureInfo)original.Clone();
        culture.NumberFormat.PositiveSign = "plus";
        try
        {
            CultureInfo.CurrentCulture = culture;
            Options options = Options.Parse(["test-network", "--jobs", "+2", "--startup-timeout-ms", "+15000"]);
            Assert.Equal(2, options.Jobs);
            Assert.Equal(15000, options.StartupTimeout);
        }
        finally { CultureInfo.CurrentCulture = original; }
    }
    [Theory]
    [InlineData("rules")]
    [InlineData("network")]
    [InlineData("source-ui")]
    public async Task FailedSourceGatePreventsBothExports(string condition)
    {
        bool exported = false, packaged = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() => VerificationGate.Run(() => Task.FromException(new InvalidOperationException(condition)),
            () => { exported = true; return Task.CompletedTask; }, () => { packaged = true; return Task.CompletedTask; }));
        Assert.False(exported); Assert.False(packaged);
    }
    [Fact]
    public async Task ExportedGraphicalFailurePreventsOverallSuccess()
    {
        bool exported = false;
        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => VerificationGate.Run(() => Task.CompletedTask,
            () => { exported = true; return Task.CompletedTask; }, () => Task.FromException(new InvalidOperationException("packed UI failure"))));
        Assert.True(exported); Assert.Equal("packed UI failure", failure.Message);
    }
    [Fact]
    public async Task OwnedGroupCleanupStopsDescendantsWhenWrapperAlreadyExited()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-group-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var child = new Child("wrapper", "setsid", ["/bin/sh", "-c", "printf 'ODOT_EVENT {\"Type\":\"ready\"}\\n'; sleep 30 &"], root, ownsGroup: true);
            await child.WaitFor(e => e.Type == "ready", "fixture readiness", 2000, CancellationToken.None);
            await child.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(child.HasExited);
        }
        finally { Directory.Delete(root, true); }
    }
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task SchedulerBoundsWorkersAndRunsEveryCase(int jobs)
    {
        int active = 0, max = 0, completed = 0;
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var scenarios = Enumerable.Range(0, 4).Select(n => new Scenario(n.ToString(CultureInfo.InvariantCulture), "fixture", async token =>
        {
            int count = Interlocked.Increment(ref active);
            Interlocked.Exchange(ref max, Math.Max(max, count));
            if (count == jobs) started.TrySetResult();
            await release.Task.WaitAsync(token);
            Interlocked.Decrement(ref active); Interlocked.Increment(ref completed);
        })).ToArray();
        Task run = ScenarioScheduler.Run(scenarios, jobs, CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(jobs, active);
        release.SetResult(); await run.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal(jobs, max); Assert.Equal(4, completed);
    }
    [Fact]
    public async Task FailureCancelsSiblingAwaitsCleanupAndStopsQueuedWork()
    {
        bool cleaned = false, queued = false;
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Scenario[] scenarios = [
            new("active", "fixture", async token => { started.SetResult(); try { await Task.Delay(Timeout.Infinite, token); } finally { cleaned = true; } }),
            new("origin", "fixture", async _ => { await started.Task; throw new InvalidOperationException("original assertion"); }),
            new("queued", "fixture", _ => { queued = true; return Task.CompletedTask; })];
        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => ScenarioScheduler.Run(scenarios, 2, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(3)));
        Assert.Contains("origin: original assertion", failure.Message);
        Assert.True(cleaned); Assert.False(queued);
    }
    [Theory]
    [InlineData("test-network", "--jobs", "0")]
    [InlineData("test-network", "--jobs", "-1")]
    [InlineData("test-network", "--scenario", "unknown")]
    [InlineData("test-ui", "--scenario", "unknown")]
    [InlineData("ci", "--scenario", "economy")]
    [InlineData("test-ui", "--engine-arg", "--headless")]
    public void InvalidSelectionFailsBeforeAnyProcess(string command, string flag, string value)
        => Assert.Throws<ArgumentException>(() => Options.Parse([command, flag, value]));
    [Fact]
    public void SelectionAndExplicitEndpointContract()
    {
        Options selected = Options.Parse(["test-network", "--scenario", "defeat", "--jobs", "1"]);
        Assert.Equal("defeat", selected.Scenario); Assert.Equal(1, selected.Jobs);
        Assert.Equal(2, Options.Parse(["test-network"]).Jobs);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-network", "--scenario", "defeat", "--port", "7001"]));
        Assert.Equal(7001, Options.Parse(["test-network", "--scenario", "authority-resume-victory", "--port", "7001"]).Port);
    }
    [Fact]
    public void MissingDisplayPrerequisitesFailWithoutFallback()
    {
        var error = Assert.Throws<InvalidOperationException>(() => PrivateDisplay.CheckPrerequisites(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
        Assert.Contains("Xvfb", error.Message); Assert.Contains("No desktop fallback", error.Message);
        Assert.Throws<InvalidOperationException>(() => PrivateDisplay.ValidateWorker(Options.Parse(["_ui-worker"])));
        Assert.Throws<InvalidOperationException>(() => PrivateDisplay.RequireSoftwareGraphics("OpenGL version string: 4.6\nOpenGL renderer string: hardware GPU"));
    }
    [Fact]
    public async Task OwnedStateIsIsolatedAndCleanupPreservesUnrelatedProcess()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        using var unrelated = Process.Start("sleep", "30")!;
        try
        {
            var evidence = new Evidence(root);
            var scope = new ScenarioScope("fixture", evidence);
            string runtime = scope.Directory;
            Assert.Equal(scope.EnvironmentFor("a")["XDG_DATA_HOME"], scope.EnvironmentFor("a")["XDG_DATA_HOME"]);
            Assert.NotEqual(scope.EnvironmentFor("a")["XDG_DATA_HOME"], scope.EnvironmentFor("b")["XDG_DATA_HOME"]);
            Child owned = scope.Own(new Child("owned", "/bin/sh", ["-c", "printf 'ODOT_EVENT {\"Type\":\"ready\"}\\n'; sleep 30"], root, environment: scope.EnvironmentFor("a"), evidenceDirectory: scope.EvidenceDirectory));
            await owned.WaitFor(e => e.Type == "ready", "fixture readiness", 2000, CancellationToken.None);
            File.WriteAllText(Path.Combine(scope.EnvironmentFor("a")["XDG_DATA_HOME"]!, "settings.cfg"), "owned setting");
            await scope.DisposeAsync(); await scope.DisposeAsync();
            Assert.True(owned.HasExited); Assert.False(Directory.Exists(runtime)); Assert.False(unrelated.HasExited);
            Assert.True(File.Exists(owned.LogPath));
        }
        finally { if (!unrelated.HasExited) unrelated.Kill(); await unrelated.WaitForExitAsync(); Directory.Delete(root, true); }
    }
    [Fact]
    public async Task UiRequestIdsRejectStaleResponsesAndPropagateCaptureFailure()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-ui-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var child = new Child("probe", "/bin/sh", ["-c", "printf 'ODOT_UI {\"Id\":\"stale\"}\\n'; read -r command id; printf 'ODOT_UI {\"Id\":\"%s\",\"Error\":\"capture failed\"}\\n' \"$id\""], root);
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => UiProtocol.Probe(child, 2000, CancellationToken.None));
            Assert.Contains("capture failed", error.Message);
        }
        finally { Directory.Delete(root, true); }
    }
    [Fact]
    public async Task UnexpectedEngineErrorsAndMissingFramesFail()
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-error-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var scope = new ScenarioScope("fixture", new Evidence(root));
            var child = scope.Own(new Child("error", "/bin/sh", ["-c", "echo 'ERROR: fixture engine error' >&2"], root));
            await child.WaitExit(CancellationToken.None); await scope.DisposeAsync();
            Assert.Throws<InvalidOperationException>(scope.CheckErrors);
            Assert.Throws<InvalidOperationException>(() => UiProtocol.Target(new UiObservation(), "Start"));
            Assert.Throws<InvalidOperationException>(() => UiProtocol.Frame(new UiObservation(), Path.Combine(root, "missing.png")));
        }
        finally { Directory.Delete(root, true); }
    }
}
