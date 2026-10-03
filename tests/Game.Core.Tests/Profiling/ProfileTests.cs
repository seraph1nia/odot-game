using Xunit;

namespace Game.Core.Tests.Profiling;

public sealed class ProfileTests
{
    [Fact]
    public async Task OwnedProfileWorkerHasAHardBoundAndAwaitedExit()
    {
        var start = new System.Diagnostics.ProcessStartInfo("dotnet") { UseShellExecute = false };
        start.ArgumentList.Add(typeof(ProfileProgram).Assembly.Location); start.ArgumentList.Add("_profile-lifecycle-probe");
        await Assert.ThrowsAsync<TimeoutException>(() => OwnedProfileProcess.Run(start, TimeSpan.FromMilliseconds(200), CancellationToken.None));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => OwnedProfileProcess.Run(start, TimeSpan.FromSeconds(5), cancellation.Token));
    }
    [Fact]
    public void SelectionDefaultsAndExplicitSoloAreDistinctFromTestDiscovery()
    {
        ProfileOptions defaults = ProfileOptions.Parse(["profile-campaign"]);
        Assert.Equal(("frontline", 4, 1UL, 3, "Release"), (defaults.Strategy, defaults.Players, defaults.Seed, defaults.Iterations, defaults.Configuration));
        ProfileOptions solo = ProfileOptions.Parse(["profile-campaign", "--strategy", "research", "--players", "1", "--seed", "8", "--iterations", "1", "--configuration", "Debug"]);
        Assert.Equal(("research", 1, 8UL, 1, "Debug"), (solo.Strategy, solo.Players, solo.Seed, solo.Iterations, solo.Configuration));
    }
    [Theory]
    [InlineData("--iterations", "0")]
    [InlineData("--iterations", "11")]
    [InlineData("--players", "5")]
    [InlineData("--players", "0")]
    [InlineData("--seed", "-1")]
    [InlineData("--strategy", "unknown")]
    [InlineData("--configuration", "ExportRelease")]
    [InlineData("--unknown", "1")]
    public void InvalidArgumentsNeverSelectAWorkload(string option, string value)
        => Assert.ThrowsAny<Exception>(() => ProfileOptions.Parse(["profile-campaign", option, value]));
    [Fact]
    public void MissingRepeatedAndUnsupportedCooperativeSelectionsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => ProfileOptions.Parse(["profile-campaign", "--iterations"]));
        Assert.Throws<ArgumentException>(() => ProfileOptions.Parse(["profile-campaign", "--seed", "1", "--seed", "1"]));
        Assert.Throws<ArgumentException>(() => ProfileOptions.Parse(["profile-campaign", "--strategy", "mixed"]));
    }
    [Fact]
    public void MetricsHaveExplicitUnitsAndCollectionDeltas()
    {
        var result = MetricSample.Difference(new(10, 4, 100, 1, 2, 3), new(30, 12, 600, 4, 3, 3));
        Assert.Equal(new MetricSample(20, 8, 500, 3, 1, 0), result);
        using var metrics = new Measurements(TimeSpan.FromSeconds(1), CancellationToken.None);
        metrics.Enter("setup"); metrics.Enter("setup"); metrics.Enter("stepping");
        Assert.Collection(metrics.Finish().Keys, p => Assert.Equal("setup", p), p => Assert.Equal("stepping", p));
    }
    [Fact]
    public void SummaryUsesMeasuredSamplesAndReportsMediansRangesWithoutTailClaims()
    {
        ProfileSample Sample(double value) => new(1, false, new(value, value * 2, 100, 0, 0, 0), new Dictionary<string, double>(StringComparer.Ordinal) { ["stepping"] = value }, 0, 0, null);
        using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(ProfileProgram.Summarize([Sample(30), Sample(10), Sample(20)])));
        System.Text.Json.JsonElement elapsed = document.RootElement.GetProperty("ElapsedMilliseconds");
        Assert.Equal(3, elapsed.GetProperty("Count").GetInt32()); Assert.Equal(20, elapsed.GetProperty("Median").GetDouble());
        Assert.Equal(10, elapsed.GetProperty("Minimum").GetDouble()); Assert.Equal(30, elapsed.GetProperty("Maximum").GetDouble());
        Assert.False(elapsed.TryGetProperty("P99", out _));
    }
    [Fact]
    public void CancellationIsObservedAtPhaseBoundariesAndRecentDiagnosticsAreBounded()
    {
        using var cancellation = new CancellationTokenSource();
        using var metrics = new Measurements(TimeSpan.FromSeconds(1), cancellation.Token);
        cancellation.Cancel(); Assert.Throws<OperationCanceledException>(() => metrics.Enter("setup"));
        var output = new ProfileOutput();
        for (int i = 0; i < 100; i++) output.WriteLine(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(100, output.Lines); Assert.Equal(64, output.Recent.Count); Assert.Equal("36", output.Recent.Peek());
    }
}
