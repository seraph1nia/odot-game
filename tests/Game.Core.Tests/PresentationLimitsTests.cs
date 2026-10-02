using Xunit;
using Game.Core;
using Game;

namespace Game.Core.Tests;

public sealed class PresentationLimitsTests
{
    [Theory]
    [InlineData(DefeatReason.BattleStalled, "DEFEAT • battle stalled")]
    [InlineData(DefeatReason.AllCitiesFallen, "DEFEAT • all cities fell")]
    public void DefeatLabelPreservesTheAuthoritativeReason(DefeatReason reason, string expected)
        => Assert.Equal(expected, PresentationLimits.DefeatText(reason));

    [Theory]
    [InlineData(525, 1050, 0.5)]
    [InlineData(1050, 1050, 1)]
    [InlineData(0, 1050, 0)]
    [InlineData(-10, 1050, 0)]
    [InlineData(2000, 1050, 1)]
    [InlineData(3, 6, 0.5)]
    public void HealthBarsUseEffectiveMaximumInsteadOfFirstObservedHealth(int current, int maximum, double expected)
        => Assert.Equal(expected, PresentationLimits.HealthFraction(current, maximum));
    [Fact]
    public void MissingMaximumHealthIsReported()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PresentationLimits.HealthFraction(5, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => PresentationLimits.HealthFraction(5, -1));
    }
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(19, 1)]
    [InlineData(20, 3)]
    [InlineData(49, 3)]
    [InlineData(50, 6)]
    [InlineData(int.MaxValue, 6)]
    public void StockpilesAreBoundedAtResourceThresholds(int amount, int expected)
        => Assert.Equal(expected, PresentationLimits.StockpileCount(amount));
    [Fact]
    public void OnlyCurrentAcceptedCommandsCueOnceWithoutReplayingOldSessions()
    {
        var cues = new ActionCues(); cues.Clear("one");
        var command = new Command(1, "one", Phase.Building, 1, "build", 1, 0, Building.Farm);
        cues.Observe(command, new(1, true, "ok")); cues.Observe(command, new(1, true, "retry"));
        Assert.Single(cues.Drain()); Assert.Empty(cues.Drain());
        cues.Observe(command with { Sequence = 2 }, new(2, false, "no wood")); Assert.Empty(cues.Drain());
        cues.Clear("two"); cues.Observe(command with { Sequence = 3 }, new(3, true, "old")); Assert.Empty(cues.Drain());
        for (int n = 1; n <= 100; n++) cues.Observe(command with { MatchId = "two", Sequence = n }, new(n, true, "ok"));
        Assert.Equal(64, cues.Drain().Length);
        cues.Baseline("two", 100); cues.Observe(command with { MatchId = "two", Sequence = 100 }, new(100, true, "late retry")); Assert.Empty(cues.Drain());
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void OriginalSynthesizedPcmHasBoundedDurationAndAmplitude(int cue)
    {
        byte[] pcm = CueSynthesis.Generate(cue); Assert.Equal(5292, pcm.Length); Assert.Equal(pcm, CueSynthesis.Generate(cue));
        for (int n = 0; n < pcm.Length; n += 2) Assert.InRange((short)(pcm[n] | pcm[n + 1] << 8), -6000, 6000);
    }
}
