using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace DevRunner.Tests;

public sealed class PresentationProfileOptionsTests
{
    [Fact]
    public void PresentationProfileIsExplicitAndDoesNotExpandDefaultUiCoverage()
    {
        Options options = Options.Parse(["profile-presentation", "--scenario", "combat-playback", "--frames", "600", "--iterations", "1", "--configuration", "Debug", "--work-counters"]);
        Assert.Equal(1, options.ProfileIterations); Assert.Equal(600, options.ProfileFrames); Assert.True(options.ProfileWorkCounters);
        Assert.DoesNotContain("combat-playback", ScenarioNames.Ui);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-ui", "--scenario", "combat-playback"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["profile-presentation"]));
    }
    [Fact]
    public void AuthoredScalesRemainSelectedAndDeterministic()
    {
        Options options = Options.Parse(["profile-presentation", "--scenario", "authored-scale", "--iterations", "1"]);
        Assert.Equal("authored-scale", options.Scenario);
        Assert.DoesNotContain("authored-scale", ScenarioNames.Ui);
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-ui", "--scenario", "authored-scale"]));
        CombatReplayInput first = CombatReplayFixture.GenerateAuthored(), second = CombatReplayFixture.GenerateAuthored();
        Assert.Equal(first.Digest, second.Digest);
        Assert.Equal(600, first.Frames.Length);
        Assert.Equal(6, first.Frames.Select(f => f.View).Distinct().Count());
        Assert.All(first.Frames.GroupBy(f => (f.View, f.Zoom)), frames => Assert.Equal(50, frames.Count()));
        Assert.All(first.Frames.Where(f => f.View!.StartsWith("combat", StringComparison.Ordinal) && f.Snapshot is not null),
            frame => Assert.Contains(frame.Snapshot!.Players.SelectMany(c => c.Soldiers).Concat(frame.Snapshot.Enemies), u => u.Deployed));
    }
    [Fact]
    public void FamilyEvidenceRequiresOriginalBaselineAndNeverOwnershipReconstruction()
    {
        string directory = Path.Combine(Path.GetTempPath(), "odot-profile-options-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string evidence = Path.Combine(directory, "evidence.json"); File.WriteAllText(evidence, "{}");
        try
        {
            string[] selected = ["profile-presentation", "--scenario", "authored-scale", "--iterations", "1"];
            Assert.Throws<ArgumentException>(() => Options.Parse([.. selected, "--boundary-evidence", evidence]));
            Assert.Throws<ArgumentException>(() => Options.Parse([.. selected, "--baseline", directory, "--boundary-evidence", evidence, "--pixel-ownership-request", evidence]));
            Options parsed = Options.Parse([.. selected, "--baseline", directory, "--boundary-evidence", evidence]);
            Assert.Equal(evidence, parsed.BoundaryEvidence); Assert.Equal(600, parsed.ProfileFrames); Assert.Equal(600000, parsed.Timeout); Assert.Equal(15000, parsed.StartupTimeout);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
    [Theory]
    [InlineData(null, false, 300)]
    [InlineData(299, false, 300)]
    [InlineData(399, false, 400)]
    [InlineData(499, false, 500)]
    [InlineData(499, true, 600)]
    [InlineData(599, true, 600)]
    public async Task EmittedInvestigationIdentityBindsRequestedAndConfirmedCoverage(int? targetFrame, bool remainingViews, int expectedFrames)
    {
        string root = IdentityFixtureRoot();
        try
        {
            string requestPath = Path.Combine(root, "request.json");
            var request = new JsonObject { ["RemainingViews"] = remainingViews };
            if (targetFrame is not null) request["TargetFrame"] = targetFrame.Value;
            await File.WriteAllTextAsync(requestPath, request.ToJsonString());
            Options options = Options.Parse(["profile-presentation", "--scenario", "authored-scale", "--iterations", "1", "--pixel-ownership-request", requestPath]);
            var evidence = new Evidence(root);
            var runner = new Runner(options, CancellationToken.None, evidence, root: root);
            string identityPath = Path.Combine(evidence.Directory, "profile-identity.json");
            await runner.WritePresentationIdentity(CancellationToken.None);
            using (JsonDocument pending = JsonDocument.Parse(await File.ReadAllTextAsync(identityPath)))
            {
                JsonElement identity = pending.RootElement;
                Assert.Equal(targetFrame ?? 299, identity.GetProperty("TargetFrame").GetInt32());
                Assert.Equal(remainingViews, identity.GetProperty("RemainingViews").GetBoolean());
                Assert.Equal(expectedFrames, identity.GetProperty("RequestedScriptedFrames").GetInt32());
                Assert.False(identity.TryGetProperty("ExecutedScriptedFrames", out _));
                Assert.Equal(0, identity.GetProperty("WarmupExecutions").GetInt32());
                Assert.Equal(1, identity.GetProperty("Concurrency").GetInt32());
                Assert.Equal(600, identity.GetProperty("PerExecutionBoundSeconds").GetInt32());
                Assert.Equal(1200, identity.GetProperty("OverallBoundSeconds").GetInt32());
                Assert.Contains("execution unconfirmed", identity.GetProperty("Coverage").GetString());
            }
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => runner.WritePresentationIdentity(CancellationToken.None, completed: true));
            string workerDirectory = Path.Combine(evidence.Directory, "six-pixel-ownership-worker", "authored-scale");
            Directory.CreateDirectory(workerDirectory);
            string receiptPath = Path.Combine(workerDirectory, "profile.json" + (remainingViews ? "" : ".prefix.json"));
            await File.WriteAllTextAsync(receiptPath, JsonSerializer.Serialize(new { Frames = expectedFrames - 1 }));
            await Assert.ThrowsAsync<InvalidDataException>(() => runner.WritePresentationIdentity(CancellationToken.None, completed: true));
            using (JsonDocument incomplete = JsonDocument.Parse(await File.ReadAllTextAsync(identityPath)))
                Assert.False(incomplete.RootElement.TryGetProperty("ExecutedScriptedFrames", out _));
            await File.WriteAllTextAsync(receiptPath, JsonSerializer.Serialize(new { Frames = expectedFrames }));
            await runner.WritePresentationIdentity(CancellationToken.None, completed: true);
            using JsonDocument completed = JsonDocument.Parse(await File.ReadAllTextAsync(identityPath));
            Assert.Equal(targetFrame ?? 299, completed.RootElement.GetProperty("TargetFrame").GetInt32());
            Assert.Equal(expectedFrames, completed.RootElement.GetProperty("ExecutedScriptedFrames").GetInt32());
            Assert.Equal($"executed ordinary rendered frames0..{expectedFrames - 1} ({expectedFrames} scripted frames); investigation only; not complete replay acceptance",
                completed.RootElement.GetProperty("Coverage").GetString());
            Assert.Contains("not acceptance", completed.RootElement.GetProperty("Mode").GetString());
            Assert.Equal(600000, options.Timeout); Assert.Equal(15000, options.StartupTimeout);
        }
        finally { Directory.Delete(root, recursive: true); }
    }
    [Fact]
    public async Task EmittedMeasurementIdentityDoesNotClaimInvestigationOrCompletedExecution()
    {
        string root = IdentityFixtureRoot();
        try
        {
            var runner = new Runner(Options.Parse(["profile-presentation", "--scenario", "authored-scale", "--iterations", "1"]), CancellationToken.None, new Evidence(root), root: root);
            await runner.WritePresentationIdentity(CancellationToken.None);
            string identityPath = Directory.GetFiles(Path.Combine(root, "logs"), "profile-identity.json", SearchOption.AllDirectories).Single();
            using JsonDocument document = JsonDocument.Parse(await File.ReadAllTextAsync(identityPath));
            JsonElement identity = document.RootElement;
            Assert.False(identity.TryGetProperty("TargetFrame", out _));
            Assert.False(identity.TryGetProperty("ExecutedScriptedFrames", out _));
            Assert.Equal(600, identity.GetProperty("RequestedScriptedFrames").GetInt32());
            Assert.Equal(1, identity.GetProperty("WarmupExecutions").GetInt32());
            Assert.Equal("600-frame measurement", identity.GetProperty("Mode").GetString());
            Assert.Equal("selected diagnostic replay; not default UI or native GPU performance", identity.GetProperty("Coverage").GetString());
        }
        finally { Directory.Delete(root, recursive: true); }
    }
    private static string IdentityFixtureRoot()
    {
        string root = Path.Combine(Environment.CurrentDirectory, "logs", "profile-identity-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        foreach (string area in new[] { "src", "tests", "tools", ".github", ".mise" }) Directory.CreateDirectory(Path.Combine(root, area));
        foreach (string file in new[] { "mise.toml", "mise.lock", "global.json", "Directory.Build.props", ".editorconfig", "Odot.slnx" })
            File.WriteAllText(Path.Combine(root, file), "{}");
        return root;
    }
    [Theory]
    [InlineData("--frames", "601")]
    [InlineData("--iterations", "0")]
    [InlineData("--iterations", "11")]
    [InlineData("--configuration", "Release")]
    public void PresentationRejectsUnmatchedFrameOrBuildInputs(string name, string value)
        => Assert.Throws<ArgumentException>(() => Options.Parse(["profile-presentation", "--scenario", "combat-playback", name, value]));
}
