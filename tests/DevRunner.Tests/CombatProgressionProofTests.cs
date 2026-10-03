using DevRunner;
using Game;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class CombatProgressionProofTests
{
    private static readonly HexBoard Board = new(HexBoardDefinition.Default());
    private static UiObservation Frame(double tick)
    {
        HexUnitState hex = new(1, 1, Faction.Adventurers, UnitLifecycle.Alive, new(14, 1), UnitActionKind.Moving,
            new(11, 3), StartTick: 100, EndTick: 130);
        System.Numerics.Vector3 Anchor(HexPosition position)
        {
            HexCoordinate cell = Board.Cell(position.Cell).Coordinate; HexAnchor anchor = Board.Anchor(position.Anchor);
            return new(cell.Column * 3 + (Math.Abs(cell.R) % 2) * 1.5f + anchor.AnchorX / 1000f, 0,
                cell.R * (1.1547005f * 1.5f * 1.5f) + anchor.AnchorForward / 1000f);
        }
        System.Numerics.Vector3 position = CombatVisualTiming.Position(Anchor(hex.Position), Anchor(hex.Destination), hex, tick);
        return new()
        {
            CombatTick = tick,
            Units = [new() { Id = 1, Destination = 1, Visible = true, Deployed = true, Hex = hex,
                X = position.X, Z = position.Z, Clip = "Walking_A", WalkingBlend = .8 }]
        };
    }
    [Fact]
    public void LiveMotionWitnessRejectsDetoursSprintsAndStaticSamples()
    {
        UiObservation first = Frame(105), next = Frame(110);
        CombatProgressionProof.Motion(first, Board); CombatProgressionProof.Motion(next, Board);
        Assert.True(CombatProgressionProof.MovementAdvanced([first, next]));
        Assert.False(CombatProgressionProof.MovementAdvanced([first, first]));
        Assert.Throws<InvalidOperationException>(() => CombatProgressionProof.Motion(first with { Units = [first.Units[0] with { X = first.Units[0].X + .1f }] }, Board));
        Assert.Throws<InvalidOperationException>(() => CombatProgressionProof.Motion(first with { Units = [first.Units[0] with { Clip = "Running_A" }] }, Board));
        Assert.Throws<InvalidOperationException>(() => CombatProgressionProof.Motion(first with { Units = [first.Units[0] with { WalkingBlend = 1 }] }, Board));
    }
    [Fact]
    public void AttackWitnessRequiresOneIdentityAdvancingClockAndImportedBones()
    {
        UnitObservation actor = new()
        {
            Id = 1,
            Visible = true,
            AttackActive = true,
            AttackBlend = 1,
            AttackSequence = 1,
            ImpactTick = 112,
            PoseSeconds = .2,
            BoneRotation = "first"
        };
        UiObservation first = new() { CombatTick = 106, Units = [actor] };
        UiObservation next = new() { CombatTick = 110, Units = [actor with { PoseSeconds = .3, BoneRotation = "second" }] };
        Assert.True(CombatProgressionProof.AttackAdvanced([first, next]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, first]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, next with { CombatTick = first.CombatTick }]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, next with { Units = [next.Units[0] with { AttackSequence = 2 }] }]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, next with { Units = [next.Units[0] with { BoneRotation = "first" }] }]));
    }
}
