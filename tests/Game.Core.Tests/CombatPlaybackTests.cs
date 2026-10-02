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
        UnitState unit = new(1, 8, 0, 1, 1) { Type = type, ActionStartTick = 10, ImpactTick = 10 + windup, ReadyTick = 70 };
        Assert.Equal(0, CombatPlayback.AttackPose(unit, 10, 1.0667));
        Assert.Equal(marker, CombatPlayback.AttackPose(unit, 10 + windup, 1.0667), 12);
        Assert.Equal(marker / 2, CombatPlayback.AttackPose(unit, 10 + windup / 2.0, 1.0667), 12);
        Assert.Equal(1.0667, CombatPlayback.AttackPose(unit, 70, 1.0667));
    }
    private static MatchSnapshot State(long revision, long tick, long sequence, CombatEvent[]? events = null, string match = "a", bool paused = false)
        => new(match, revision, tick, Phase.Combat, paused, 1, 3, 3, new Rules(), [], [])
        { EventSequence = sequence, OldestEventSequence = events is { Length: > 0 } ? events[0].Sequence : sequence + 1, CombatEvents = events ?? [] };
    private static CombatEvent Event(long seq, long tick, CombatEventType type = CombatEventType.Death)
        => new(seq, tick, type, new(1, 0, 0, 1, 1));
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
        UnitState unit = new(1, 8, 40, 1, 1)
        {
            Type = UnitType.Crossbowman,
            AttackSequence = 2,
            ActionStartTick = 30,
            ImpactTick = 48,
            ReadyTick = 90,
            Hex = new(1, 1, Faction.Skeletons, UnitLifecycle.Alive, new(8, 1), UnitActionKind.Windup, StartTick: 30, EndTick: 90)
        };
        var playback = new CombatPlayback();
        playback.Accept(State(1, 40, 5) with { Enemies = [unit] });
        Assert.Equal(40, playback.Tick); Assert.Equal(unit, Assert.Single(playback.Units()));
        playback.Accept(State(2, 43, 5) with
        {
            Enemies = [unit with { Destination = 2, Deployed = false,
            Hex = new(1, 2, Faction.Skeletons, UnitLifecycle.Queued, default) }]
        });
        UnitState transferred = Assert.Single(playback.Units()); Assert.Equal(2, transferred.Destination);
        Assert.Equal(UnitLifecycle.Queued, transferred.Hex!.Lifecycle); Assert.False(transferred.Deployed); Assert.Equal(90, transferred.ReadyTick);
    }
    [Fact]
    public void BufferedAttackKeepsRecoveryAndSharedAnchorsUntilTheDeclaredStart()
    {
        UnitState recovering = new(1, 800, 0, 1, 1)
        {
            Type = UnitType.Crossbowman,
            AttackSequence = 1,
            ActionStartTick = 70,
            ImpactTick = 88,
            ReadyTick = 130,
            Hex = new(1, 1, Faction.Skeletons, UnitLifecycle.Alive, new(11, 1), UnitActionKind.Recovery, StartTick: 70, EndTick: 130)
        };
        UnitState neighbor = new(2, 800, 0, 1, 1)
        {
            Type = UnitType.Crossbowman,
            Hex = new(2, 1, Faction.Skeletons, UnitLifecycle.Alive, new(11, 4))
        };
        UnitState attack = recovering with
        {
            AttackSequence = 2,
            ActionStartTick = 130,
            ImpactTick = 148,
            ReadyTick = 190,
            PendingImpact = true,
            Hex = recovering.Hex! with { Action = UnitActionKind.Windup, StartTick = 130, EndTick = 190 }
        };
        var playback = new CombatPlayback(); playback.Accept(State(1, 127, 0) with { Enemies = [recovering, neighbor] });
        playback.Accept(State(2, 131, 0) with { Enemies = [attack, neighbor] });
        Assert.Equal(recovering, playback.Units()[0]); Assert.Equal(neighbor, playback.Units()[1]);
        playback.Advance(2 / 60.0, true);
        Assert.Equal(130, playback.Tick); Assert.Equal(attack, playback.Units()[0]); Assert.Equal(neighbor, playback.Units()[1]);
        Assert.Equal(recovering.Hex.Position, attack.Hex.Position);
    }
    [Fact]
    public void EntryWaitsForItsSnapshotClockAndTerminalResultRetainsTheFrozenPose()
    {
        UnitState old = new(1, 800, 0, 1, 1)
        {
            Type = UnitType.Crossbowman,
            ReadyTick = 30,
            Hex = new(1, 1, Faction.Skeletons, UnitLifecycle.Alive, new(11, 1), UnitActionKind.Moving, new(8, 1), StartTick: 0, EndTick: 30)
        };
        UnitState arrival = new(2, 800, 0, 1, 1)
        {
            Type = UnitType.Crossbowman,
            Deployed = false,
            Hex = new(2, 1, Faction.Skeletons, UnitLifecycle.Queued, default)
        };
        var playback = new CombatPlayback();
        playback.Accept(State(1, 0, 0) with { Enemies = [old, arrival] });
        playback.Accept(State(2, 3, 0) with
        {
            Enemies = [old, arrival with { Deployed = true,
            Hex = arrival.Hex! with { Lifecycle = UnitLifecycle.Alive, Position = new(2, 1), AdmittedTick = 3 } }]
        });
        Assert.False(playback.Units().Single(u => u.Id == 2).Deployed);
        playback.Advance(0.1, true); Assert.True(playback.Units().Single(u => u.Id == 2).Deployed);
        UnitState frozen = old with { Hex = old.Hex! with { FrozenTick = 3 } };
        playback.Accept(State(3, 3, 0) with { Phase = Phase.Defeat, Enemies = [frozen] });
        Assert.Equal(frozen, Assert.Single(playback.Units()));
        playback.Accept(State(4, 10, 0) with { Phase = Phase.Defeat, Enemies = [frozen] }); playback.Advance(.1, true);
        Assert.Equal(3, Assert.Single(playback.Units()).Hex!.FrozenTick);
    }
    private static UnitState Corpse() => new(1, 0, 0, 1, 1)
    {
        Type = UnitType.Crossbowman,
        Faction = Faction.Skeletons,
        Hex = new(1, 1, Faction.Skeletons, UnitLifecycle.Dying, new(11, 1),
            UnitActionKind.Moving, new(8, 1), ActionSequence: 1, StartTick: 190, EndTick: 220, DeathStartTick: 200, DeathEndTick: 248, FrozenMoveTicks: 10)
    };
    [Fact]
    public void CurrentDeathRestoresWithoutHistoryAndExpiresOnTheSharedTick()
    {
        var playback = new CombatPlayback(); UnitState corpse = Corpse();
        playback.Accept(State(1, 224, 90) with { DyingBodies = [corpse] }, baseline: true);
        Assert.Empty(playback.Drain()); Assert.Equal(corpse, Assert.Single(playback.Units()));
        Assert.Equal(.4, CombatPlayback.DeathPose(corpse, playback.Tick, .8), 12);
        playback.Accept(State(2, 245, 90, paused: true) with { DyingBodies = [corpse] });
        double frozen = playback.Tick; playback.Advance(1, true); Assert.Equal(frozen, playback.Tick); Assert.Single(playback.Units());
        playback.Accept(State(3, 245, 90) with { DyingBodies = [corpse] }); playback.Advance(.1, true);
        playback.Accept(State(4, 248, 90)); Assert.Single(playback.Units());
        playback.Advance(.05, true); Assert.Equal(248, playback.Tick); Assert.Empty(playback.Units()); Assert.Empty(playback.Drain());
    }
    [Fact]
    public void BufferedArrivalKeepsTheCommittedRouteUntilTheDeclaredBoundary()
    {
        UnitState moving = new(1, 800, 0, 1, 1)
        {
            Type = UnitType.Crossbowman,
            Hex = new(1, 1, Faction.Adventurers, UnitLifecycle.Alive, new(11, 1), UnitActionKind.Moving, new(8, 1), StartTick: 100, EndTick: 130)
        };
        UnitState arrived = moving with { Hex = moving.Hex! with { Position = new(8, 1), Action = UnitActionKind.Waiting, Destination = default } };
        var playback = new CombatPlayback(); playback.Accept(State(1, 127, 0) with { Enemies = [moving] });
        playback.Accept(State(2, 131, 0) with { Enemies = [arrived] });
        Assert.Equal(128, playback.Tick); Assert.Equal(UnitActionKind.Moving, Assert.Single(playback.Units()).Hex!.Action);
        playback.Advance(1 / 60.0, true); Assert.Equal(129, playback.Tick); Assert.Equal(11, Assert.Single(playback.Units()).Hex!.Position.Cell);
        playback.Advance(1 / 60.0, true); Assert.Equal(130, playback.Tick); Assert.Equal(8, Assert.Single(playback.Units()).Hex!.Position.Cell);
    }
    [Fact]
    public void TypedAdmissionDoesNotAppearBeforeItsAuthoritativeTick()
    {
        UnitState queued = new(1, 800, 0, 1, 1)
        {
            Deployed = false,
            Type = UnitType.Crossbowman,
            Hex = new(1, 1, Faction.Adventurers, UnitLifecycle.Queued, default)
        };
        UnitState admitted = queued with { Deployed = true, Hex = queued.Hex! with { Lifecycle = UnitLifecycle.Alive, Position = new(20, 1), AdmittedTick = 10 } };
        var playback = new CombatPlayback(); playback.Accept(State(1, 8, 0) with { Enemies = [queued] });
        playback.Accept(State(2, 11, 0) with { Enemies = [admitted] }); Assert.False(Assert.Single(playback.Units()).Deployed);
        playback.Advance(2 / 60.0, true); Assert.Equal(10, playback.Tick); Assert.True(Assert.Single(playback.Units()).Deployed);
    }
}
