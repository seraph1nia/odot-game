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
    [Theory]
    [InlineData("--frames", "601")]
    [InlineData("--iterations", "0")]
    [InlineData("--iterations", "11")]
    [InlineData("--configuration", "Release")]
    public void PresentationRejectsUnmatchedFrameOrBuildInputs(string name, string value)
        => Assert.Throws<ArgumentException>(() => Options.Parse(["profile-presentation", "--scenario", "combat-playback", name, value]));
}
