using System.Text.Json;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class CombatRecoveryObservationTests
{
    // Realistic chronology and actor/action geometry from the hosted handoff.
    // These are executable controls, not a claim that paused artifact frames
    // themselves provide the missing live pair.
    private static UnitObservation Actor => new()
    {
        Id = 7,
        Type = UnitType.Swordsman,
        Visible = true,
        Health = 2000,
        MaximumHealth = 2000,
        Clip = "attack",
        AttackActive = true,
        AttackSequence = 4,
        ImpactTick = 822,
        ReadyTick = 870,
        ActionStartTick = 792,
        X = 1.5f,
        Z = -3.148076f,
        Hex = new(7, 1, Faction.Adventurers, UnitLifecycle.Alive, new(1, 1),
            UnitActionKind.Recovery, ActionSequence: 10)
    };
    private static UiObservation Frame(string id, double tick = 824, long revision = 917) => new()
    {
        Id = id,
        MatchId = "mixed-army-match",
        MatchPhase = Phase.Combat,
        Wave = 3,
        Connected = true,
        ObservedCity = 1,
        CombatTick = tick,
        Revision = revision,
        Units = [Actor],
        Width = 1280,
        Height = 720,
        Camera = new() { Zoom = 1 }
    };
    private static MatchSnapshot Received
    {
        get
        {
            using var authority = new AuthoritySession(AuthorityPolicy.Solo);
            return authority.Snapshot() with { MatchId = "mixed-army-match", Tick = 829, Revision = 922 };
        }
    }
    private static UiObservation WithActor(UiObservation frame, UnitObservation actor) => frame with { Units = [actor] };

    [Fact]
    public void EarlyOrderedPlacementRetainsPairThroughInspectionWhileLateOnlyMissesIt()
    {
        UiObservation[] arrivalOrder =
        [
            Frame("live-first"), Frame("live-second", 825, 918),
            Frame("paused-focus", 844, 937) with { Paused = true },
            Frame("paused-casualty", 861, 956) with { Paused = true },
            Frame("death-cleaned", 911, 1007) with { Units = [] },
            Frame("late-inspection", 995, 1091) with { Units = [] },
            Frame("late-final", 1110, 1206) with { MatchPhase = Phase.Building, Wave = 4, Units = [] }
        ];
        var earlyDiagnostic = new CombatPoseDiagnostic();
        var early = new CombatRecoveryObservation(earlyDiagnostic);
        var late = new CombatRecoveryObservation(new());
        for (int index = 0; index < arrivalOrder.Length; index++)
        {
            early.Observe(arrivalOrder[index], Received, index < 2 ? "early-live:attack" : "current-poll");
            if (index >= 5) late.Observe(arrivalOrder[index], Received, "current-poll");
        }
        Assert.True(early.Proven);
        Assert.False(late.Proven);
        using JsonDocument proof = JsonDocument.Parse(earlyDiagnostic.Finish(new(false, false, false, false, false, early.Proven, false), "control"));
        JsonElement previous = proof.RootElement.GetProperty("RecoveryPrevious"), current = proof.RootElement.GetProperty("RecoveryCurrent");
        Assert.Equal("live-first", previous.GetProperty("ResponseId").GetString());
        Assert.Equal("live-second", current.GetProperty("ResponseId").GetString());
        Assert.Equal(824, previous.GetProperty("Tick").GetDouble());
        Assert.Equal(825, current.GetProperty("Tick").GetDouble());
        Assert.False(current.GetProperty("Paused").GetBoolean());
        Assert.Equal(870, current.GetProperty(nameof(CombatPoseWitness.Actor)).GetProperty("ReadyTick").GetInt64());
        Assert.Equal(1.5, current.GetProperty(nameof(CombatPoseWitness.Actor)).GetProperty("X").GetDouble());
        // Latest arrival is deliberately newer than the presented frame.
        Assert.Equal(829, current.GetProperty("ReceivedTick").GetInt64());
    }

    public static IEnumerable<object[]> IneligibleSecond() => IneligibleFrames().Select((_, index) => new object[] { index });

    private static IEnumerable<UiObservation> IneligibleFrames()
    {
        UiObservation next = Frame("second", 825, 918);
        yield return next with { Paused = true };
        yield return next with { Connected = false };
        yield return next with { Id = "first" };
        yield return next with { Id = "" };
        yield return next with { CombatTick = 824 };
        yield return next with { CombatTick = 823 };
        yield return next with { Revision = 916 };
        yield return next with { MatchId = "another-match" };
        yield return next with { ObservedCity = 2 };
        yield return next with { Wave = 4 };
        yield return next with { PlaybackGeneration = 1 };
        yield return next with { MatchPhase = Phase.Building };
        yield return next with { Units = [] };
        yield return WithActor(next, Actor with { Id = 8, Hex = Actor.Hex! with { Id = 8 } });
        yield return WithActor(next, Actor with { ReadyTick = 871 });
        yield return WithActor(next, Actor with { AttackSequence = 5 });
        yield return WithActor(next, Actor with { ImpactTick = 823 });
        yield return WithActor(next, Actor with { Hex = Actor.Hex! with { ActionSequence = 11 } });
        yield return WithActor(next, Actor with { Dead = true });
        yield return WithActor(next, Actor with { Health = 0 });
        yield return WithActor(next, Actor with { Visible = false });
        yield return WithActor(next, Actor with { Hex = Actor.Hex! with { Lifecycle = UnitLifecycle.Dying } });
        yield return WithActor(next, Actor with { Hex = Actor.Hex! with { FrozenTick = 824 } });
        yield return WithActor(next, Actor with { Hex = Actor.Hex! with { City = 2 } });
        yield return WithActor(next, Actor with { Hex = Actor.Hex! with { Action = UnitActionKind.Windup } });
        yield return WithActor(next, Actor with { Hex = null });
        yield return next with { CombatTick = 821 };
        yield return next with { CombatTick = 870 };
        yield return next with { CombatTick = 995 };
    }

    [Theory]
    [MemberData(nameof(IneligibleSecond))]
    public void InvalidPairCannotProveRecovery(int index)
    {
        UiObservation second = IneligibleFrames().ElementAt(index);
        var collector = new CombatRecoveryObservation(new());
        Assert.False(collector.Observe(Frame("first"), Received, "early-live"));
        Assert.False(collector.Observe(second, Received, "early-live"));
        Assert.False(collector.Proven);
    }

    [Fact]
    public void PausedMilestonesCannotSeedLivePairAndEndedActionCannotBeBridged()
    {
        var collector = new CombatRecoveryObservation(new());
        Assert.False(collector.Observe(Frame("paused", 844, 937) with { Paused = true }, Received, "early-live"));
        Assert.False(collector.Observe(Frame("resumed", 861, 956), Received, "early-live"));
        Assert.False(collector.Observe(Frame("missing", 862, 957) with { Units = [] }, Received, "early-live"));
        Assert.False(collector.Observe(Frame("replayed-action", 863, 958), Received, "early-live"));
    }

    [Fact]
    public void ReplayedResponseCannotBridgeOrRewindCurrentArrivalOrder()
    {
        var collector = new CombatRecoveryObservation(new());
        Assert.False(collector.Observe(Frame("first"), Received, "early-live"));
        Assert.False(collector.Observe(Frame("current", 850, 943) with { Units = [] }, Received, "early-live"));
        Assert.False(collector.Observe(Frame("first"), Received, "early-live"));
        Assert.False(collector.Observe(Frame("new-id-old-tick", 825, 918), Received, "early-live"));
        Assert.False(collector.Observe(Frame("new-current", 851, 944), Received, "early-live"));
        Assert.True(collector.Observe(Frame("next-current", 852, 945), Received, "early-live"));
    }

    [Theory]
    [InlineData(-.1f, 0)]
    [InlineData(.1f, 0)]
    [InlineData(0, -.1f)]
    [InlineData(0, .1f)]
    public void BothPositionAxesAndDirectionsMustRemainAnchored(float dx, float dz)
    {
        var collector = new CombatRecoveryObservation(new());
        collector.Observe(Frame("first"), Received, "early-live");
        var second = WithActor(Frame("second", 825, 918), Actor with { X = Actor.X + dx, Z = Actor.Z + dz });
        Assert.Throws<InvalidOperationException>(() => collector.Observe(second, Received, "early-live"));
        Assert.False(collector.Proven);
    }

    [Theory]
    [InlineData("attack")]
    [InlineData("hit")]
    public void AnimationLayerGuardsStillFail(string clip)
    {
        var collector = new CombatRecoveryObservation(new());
        var frame = WithActor(Frame("bad-layer"), Actor with { Clip = clip, AttackActive = false, HitActive = false });
        Assert.Throws<InvalidOperationException>(() => collector.Observe(frame, Received, "early-live"));
    }

    [Fact]
    public void CurrentHitLayerMayOverlayAnchoredRecovery()
    {
        var collector = new CombatRecoveryObservation(new());
        collector.Observe(Frame("first"), Received, "early-live");
        Assert.True(collector.Observe(WithActor(Frame("second", 825, 918), Actor with { Clip = "hit", HitActive = true }), Received, "early-live"));
    }
}
