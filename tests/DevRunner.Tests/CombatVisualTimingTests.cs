using System.Numerics;
using Game;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class CombatVisualTimingTests
{
    private static HexUnitState Move => new(1, 1, Faction.Adventurers, UnitLifecycle.Alive, new(14, 1), UnitActionKind.Moving,
        new(11, 3), StartTick: 100, EndTick: 130);
    private static UnitState Actor => new(1, 1000, 0, 1, 1)
    {
        Type = UnitType.Swordsman,
        TargetId = 2,
        AttackSequence = 1,
        ActionStartTick = 100,
        ImpactTick = 112,
        ReadyTick = 160,
        Hex = Move with { Action = UnitActionKind.Windup }
    };
    [Theory]
    [InlineData(99, 0)]
    [InlineData(100, 0)]
    [InlineData(115, .5)]
    [InlineData(130, 1)]
    [InlineData(200, 1)]
    public void StraightStepsHaveExactClampedEndpointsAndNoBackwardProgress(double tick, double fraction)
    {
        Vector3 source = new(-1, 0, -10), destination = new(2, 0, -7);
        Assert.Equal(fraction, CombatVisualTiming.MoveFraction(Move, tick));
        Assert.Equal(Vector3.Lerp(source, destination, (float)fraction), CombatVisualTiming.Position(source, destination, Move, tick));
        float previous = 0;
        for (double sample = 100; sample <= 130; sample += .25)
        {
            Vector3 offset = CombatVisualTiming.Position(source, destination, Move, sample) - source;
            float progress = Vector3.Dot(offset, destination - source);
            Assert.True(progress >= previous); previous = progress;
            Assert.InRange(Vector3.Cross(offset, destination - source).Length(), 0, .00001f);
        }
    }
    [Fact]
    public void FrozenAndTerminalStepsDoNotUseASecondClock()
    {
        HexUnitState frozen = Move with { FrozenTick = 115 };
        HexUnitState dead = Move with { Lifecycle = UnitLifecycle.Dying, FrozenMoveTicks = 15 };
        foreach (double tick in new[] { 115d, 130, 200, 1000 })
        {
            Assert.Equal(.5, CombatVisualTiming.MoveFraction(frozen, tick));
            Assert.Equal(.5, CombatVisualTiming.MoveFraction(dead, tick));
            Assert.Equal(.8, CombatVisualTiming.Walking(frozen, tick));
            Assert.Equal(0, CombatVisualTiming.Walking(dead, tick));
        }
        Assert.Equal(0, CombatVisualTiming.Walking(Move with { Action = UnitActionKind.Waiting }, 115));
        Assert.Equal(0, CombatVisualTiming.Walking(Move with { Lifecycle = UnitLifecycle.Queued }, 115));
    }
    [Fact]
    public void WalkingFadesAreBoundedAndNeverChooseTheRunBlend()
    {
        Assert.Equal(0, CombatVisualTiming.Walking(Move, 100));
        Assert.Equal(.4, CombatVisualTiming.Walking(Move, 102));
        Assert.Equal(.8, CombatVisualTiming.Walking(Move, 115));
        Assert.Equal(.4, CombatVisualTiming.Walking(Move, 128));
        Assert.Equal(0, CombatVisualTiming.Walking(Move, 130));
        for (double tick = 99; tick <= 131; tick += .1) Assert.InRange(CombatVisualTiming.Walking(Move, tick), 0, .8);
    }
    [Theory]
    [InlineData(UnitType.Swordsman)]
    [InlineData(UnitType.Berserker)]
    [InlineData(UnitType.Crossbowman)]
    [InlineData(UnitType.Mage)]
    public void SampledAttackBlendsKeepAuthoredImpactAndDeathPrecedence(UnitType type)
    {
        UnitState actor = Actor with { Type = type };
        Assert.Equal(0, CombatVisualTiming.Attack(actor, 100));
        Assert.Equal(1, CombatVisualTiming.Attack(actor, 112));
        Assert.Equal(CombatPlayback.ImpactMarker(type), CombatPlayback.AttackPose(actor, 112, 1.5), 12);
        Assert.Equal(.5, CombatVisualTiming.Attack(actor, 158));
        Assert.Equal(0, CombatVisualTiming.Attack(actor, 160));
        Assert.Equal(0, CombatVisualTiming.Attack(actor with { Hex = actor.Hex! with { Lifecycle = UnitLifecycle.Dying } }, 112));
        Assert.Equal(1, CombatVisualTiming.Attack(actor with { Hex = actor.Hex! with { FrozenTick = 112 } }, 500));
        Assert.Equal(0, CombatVisualTiming.Hit(1.05, 1, true, false));
        Assert.InRange(CombatVisualTiming.Hit(1.05, 1, false, true), .299, .3);
        Assert.Equal(0, CombatVisualTiming.Hit(1.15, 1, false, false));
    }
    [Fact]
    public void FacingUsesTheShortArcAndFreezesWithoutCombatAdvancement()
    {
        float current = 3.1f, desired = -3.1f;
        Assert.Equal(current, CombatVisualTiming.Facing(current, desired, 0));
        Assert.Equal(current, CombatVisualTiming.Facing(current, desired, -1));
        Assert.InRange(CombatVisualTiming.Facing(current, desired, 3), 3.1f, 3.1832f);
        float whole = CombatVisualTiming.Facing(0, 1, 6);
        float halves = CombatVisualTiming.Facing(CombatVisualTiming.Facing(0, 1, 3), 1, 3);
        Assert.InRange(Math.Abs(whole - halves), 0, .000001f);
    }
    [Fact]
    public void MeleePhasesAreDistinctBoundedFrozenAndCurrentActionOnly()
    {
        Assert.Equal("none", CombatVisualTiming.MeleePhase(Actor, 99));
        Assert.Equal("intent", CombatVisualTiming.MeleePhase(Actor, 106));
        Assert.Equal("miss", CombatVisualTiming.MeleePhase(Actor with { AttackLanded = false }, 112));
        Assert.Equal("landed", CombatVisualTiming.MeleePhase(Actor with { AttackLanded = true }, 112));
        Assert.Equal("none", CombatVisualTiming.MeleePhase(Actor, 124));
        Assert.Equal("intent", CombatVisualTiming.MeleePhase(Actor with { Hex = Actor.Hex! with { FrozenTick = 106 } }, 500));
        Assert.Equal("none", CombatVisualTiming.MeleePhase(Actor with { Hex = Actor.Hex! with { Lifecycle = UnitLifecycle.Dying } }, 112));
        Assert.Equal("none", CombatVisualTiming.MeleePhase(Actor with { AttackSequence = 0 }, 106));
        Assert.Equal("none", CombatVisualTiming.MeleePhase(Actor with { TargetCity = true }, 106));
    }
}
