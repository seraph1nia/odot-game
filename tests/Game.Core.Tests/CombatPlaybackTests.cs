using Xunit;

namespace Game.Core.Tests;

public sealed class CombatPlaybackTests
{
    [Theory]
    [InlineData(UnitType.Swordsman, 12, 0.40)]
    [InlineData(UnitType.Crossbowman, 18, 0.43)]
    [InlineData(UnitType.Berserker, 18, 0.7666666666666667)]
    [InlineData(UnitType.Mage, 24, 0.26666666666666666)]
    public void StrikeMarkersSeekToAuthorityImpactAndRecovery(UnitType type, int windup, double marker)
    {
        UnitState unit = new(1, 8, 2, 0, 1, 1) { Type = type, ActionStartTick = 10, ImpactTick = 10 + windup, ReadyTick = 70 };
        Assert.Equal(0, CombatPlayback.AttackPose(unit, 10, 1.0667));
        Assert.Equal(marker, CombatPlayback.AttackPose(unit, 10 + windup, 1.0667), 12);
        Assert.Equal(marker / 2, CombatPlayback.AttackPose(unit, 10 + windup / 2.0, 1.0667), 12);
        Assert.Equal(1.0667, CombatPlayback.AttackPose(unit, 70, 1.0667));
    }
    private static MatchSnapshot State(long revision, long tick, long sequence, CombatEvent[]? events = null, string match = "a", bool paused = false)
        => new(match, revision, tick, Phase.Combat, paused, 1, 3, 3, new Rules(), [], [])
        { EventSequence = sequence, OldestEventSequence = events is { Length: > 0 } ? events[0].Sequence : sequence + 1, CombatEvents = events ?? [] };
    private static CombatEvent Event(long seq, long tick, CombatEventType type = CombatEventType.Death)
        => new(seq, tick, type, new(1, 0, 3, 0, 1, 1));
    [Fact]
    public void InitialResumeAndNewMatchBaselineDoNotReplayHistory()
    {
        var playback = new CombatPlayback();
        playback.Accept(State(1, 10, 1, [Event(1, 9)])); Assert.Empty(playback.Drain()); Assert.Equal(1, playback.EventCursor);
        playback.Accept(State(2, 13, 2, [Event(1, 9), Event(2, 12)])); playback.Advance(0.1, true);
        Assert.Single(playback.Drain()); Assert.Empty(playback.Drain());
        playback.Accept(State(3, 30, 3, [Event(3, 29)]), baseline: true); Assert.Empty(playback.Drain());
        playback.Accept(State(1, 0, 0, match: "b")); Assert.Equal(0, playback.EventCursor); Assert.Equal(0, playback.VisualSeconds);
    }
    [Fact]
    public void OverlappingAndOutOfOrderSnapshotsDeduplicateEventsAndGapResets()
    {
        var playback = new CombatPlayback(); playback.Accept(State(1, 0, 0));
        MatchSnapshot next = State(2, 3, 2, [Event(1, 1), Event(2, 2)]);
        Assert.True(playback.Accept(next)); Assert.False(playback.Accept(next)); Assert.False(playback.Accept(State(1, 0, 0)));
        playback.Advance(0.1, true); Assert.Equal(new long[] { 1, 2 }, playback.Drain().Select(e => e.Sequence));
        playback.Accept(State(3, 6, 3, [Event(1, 1), Event(2, 2), Event(3, 5)])); playback.Advance(0.1, true);
        Assert.Equal(3, Assert.Single(playback.Drain()).Sequence);
        int generation = playback.Generation;
        playback.Accept(State(4, 300, 100, [Event(90, 290), Event(100, 300)]));
        Assert.Empty(playback.Drain()); Assert.Equal(100, playback.EventCursor); Assert.True(playback.Generation > generation);
    }
    [Fact]
    public void ClockIsBoundedAndFreezesOnPauseOrTransportLoss()
    {
        var playback = new CombatPlayback(); playback.Accept(State(1, 0, 0)); playback.Accept(State(2, 3, 0));
        playback.Advance(10, true); Assert.Equal(3, playback.Tick);
        double seconds = playback.VisualSeconds;
        playback.Advance(10, false); Assert.Equal(seconds, playback.VisualSeconds);
        playback.Accept(State(3, 3, 0, paused: true)); playback.Advance(10, true); Assert.Equal(seconds, playback.VisualSeconds);
        playback.Accept(State(4, 6, 0)); playback.Advance(0.02, true); Assert.InRange(playback.Tick, 3, 6);
    }
    [Fact]
    public void LateActionSeeksCurrentTimingAndTransfersDoNotInterpolateBetweenCities()
    {
        UnitState unit = new(1, 8, 2, 40, 1, 1) { Type = UnitType.Crossbowman, AttackSequence = 2, ActionStartTick = 30, ImpactTick = 48, ReadyTick = 90 };
        var playback = new CombatPlayback();
        playback.Accept(State(1, 40, 5) with { Enemies = [unit] });
        Assert.Equal(40, playback.Tick); Assert.Equal(unit, Assert.Single(playback.Units()));
        playback.Accept(State(2, 43, 5) with { Enemies = [unit with { Destination = 2, Position = 12, Deployed = false }] });
        UnitState transferred = Assert.Single(playback.Units()); Assert.Equal(2, transferred.Destination); Assert.Equal(12, transferred.Position);
    }
    [Fact]
    public void ContactArcUsesOneSafeFrameInsteadOfAnOverlappingChord()
    {
        UnitState a = new(1, 1000, 2, 0, 1, 1);
        UnitState b = new(2, 1000, 2.4, 0, 1, 1);
        var playback = new CombatPlayback(); playback.Accept(State(1, 0, 0) with { Enemies = [a, b] });
        playback.Accept(State(2, 6, 0) with { Enemies = [a, b with { Position = 2, Lateral = .4 }] });
        Assert.Equal(2.4, playback.Units()[1].Position); Assert.Equal(0, playback.Units()[1].Lateral);
        playback.Advance(.1, true); Assert.Equal(2, playback.Units()[1].Position); Assert.Equal(.4, playback.Units()[1].Lateral);
    }
    [Fact]
    public void EntryWaitsForItsSnapshotClockAndSurvivorsBecomeIdleAfterWaveEnd()
    {
        UnitState old = new(1, 8, 11.4, 0, 1, 1) { MoveForward = -1, TargetId = 2, ReadyTick = 100 };
        UnitState arrival = new(2, 8, 11.8, 0, 1, 1) { Deployed = false };
        var playback = new CombatPlayback();
        playback.Accept(State(1, 0, 0) with { Enemies = [old, arrival] });
        playback.Accept(State(2, 3, 0) with { Enemies = [old with { Position = 11.35 }, arrival with { Deployed = true }] });
        Assert.False(playback.Units().Single(u => u.Id == 2).Deployed);
        playback.Advance(0.1, true); Assert.True(playback.Units().Single(u => u.Id == 2).Deployed);
        playback.Accept(State(3, 3, 0) with { Phase = Phase.Building, Enemies = [old] });
        UnitState survivor = Assert.Single(playback.Units()); Assert.Equal(0, survivor.MoveForward); Assert.Equal(0, survivor.ReadyTick); Assert.Equal(0, survivor.TargetId);
    }
}
