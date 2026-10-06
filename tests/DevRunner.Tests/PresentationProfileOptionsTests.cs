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
    [Theory]
    [InlineData("--frames", "601")]
    [InlineData("--iterations", "0")]
    [InlineData("--iterations", "11")]
    [InlineData("--configuration", "Release")]
    public void PresentationRejectsUnmatchedFrameOrBuildInputs(string name, string value)
        => Assert.Throws<ArgumentException>(() => Options.Parse(["profile-presentation", "--scenario", "combat-playback", name, value]));
}
