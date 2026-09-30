using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class AudioMixProgressTests
{
    [Fact]
    public void CountsRealMixesWhenElapsedPhaseIncreasesBetweenSlowFrames()
    {
        var progress = new AudioMixProgress();
        progress.Observe(100_000, .001, 100_010);
        progress.Observe(133_000, .002, 133_010);
        progress.Observe(166_000, .003, 166_010);
        Assert.Equal(2, progress.Cycles);
    }

    [Fact]
    public void StalledMixerDoesNotBecomeProgressAsTimePasses()
    {
        var progress = new AudioMixProgress();
        progress.Observe(100_000, .001, 100_010);
        progress.Observe(133_000, .034, 133_010);
        progress.Observe(166_000, .067, 166_010);
        Assert.Equal(0, progress.Cycles);
    }

    [Fact]
    public void SchedulingDelayAndRoundingDoNotInventMixes()
    {
        var progress = new AudioMixProgress();
        progress.Observe(100_000, .011, 110_000);
        progress.Observe(133_000, .034, 133_010);
        progress.Observe(166_000, .067, 166_010);
        Assert.Equal(0, progress.Cycles);
        progress.Observe(200_000, .001, 200_010);
        Assert.Equal(1, progress.Cycles);
    }
}
