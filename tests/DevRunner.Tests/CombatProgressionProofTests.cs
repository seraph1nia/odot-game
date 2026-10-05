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
                X = position.X, Z = position.Z, Clip = "walk", WalkingBlend = .8 }]
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
        Assert.Throws<InvalidOperationException>(() => CombatProgressionProof.Motion(first with { Units = [first.Units[0] with { Clip = "run" }] }, Board));
        Assert.Throws<InvalidOperationException>(() => CombatProgressionProof.Motion(first with { Units = [first.Units[0] with { WalkingBlend = 1 }] }, Board));
    }
    [Theory]
    [InlineData(UnitType.Swordsman)]
    [InlineData(UnitType.Berserker)]
    [InlineData(UnitType.Crossbowman)]
    [InlineData(UnitType.Mage)]
    public void EveryAuthoredRoleWitnessUsesItsActualClipAndLiveLayer(UnitType type)
    {
        UnitObservation unit = new() { Type = type, Visible = true, Clip = "attack", AttackActive = true };
        Assert.True(Runner.AuthoredAttack(unit, type));
        foreach (string oldOrInactive in new[] { "2H_Melee_Attack_Chop", "1H_Melee_Attack_Slice", "Spellcast_Shoot", "hit", "idle", "walk" })
            Assert.False(Runner.AuthoredAttack(unit with { Clip = oldOrInactive }, type));
        Assert.False(Runner.AuthoredAttack(unit with { AttackActive = false }, type));
        Assert.False(Runner.AuthoredAttack(unit with { Visible = false }, type));
        Assert.False(Runner.AuthoredAttack(unit with { Dead = true }, type));
        Assert.False(Runner.AuthoredAttack(unit with { Type = type == UnitType.Mage ? UnitType.Berserker : UnitType.Mage }, type));
    }

    [Fact]
    public void AttackWitnessRequiresOneIdentityAdvancingClockAndImportedBones()
    {
        UnitObservation actor = new()
        {
            Id = 1,
            Visible = true,
            WeaponAttached = true,
            EquipmentAligned = true,
            EquipmentRotation = "first",
            Clip = "attack",
            AttackActive = true,
            AttackBlend = 1,
            AttackSequence = 1,
            ImpactTick = 112,
            PoseSeconds = .2,
            BoneRotation = "first"
        };
        UiObservation first = new() { CombatTick = 106, Units = [actor] };
        UiObservation next = new() { CombatTick = 110, Units = [actor with { PoseSeconds = .3, BoneRotation = "second", EquipmentRotation = "second" }] };
        Assert.True(CombatProgressionProof.AttackAdvanced([first, next]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, first]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first with { Units = [actor with { Clip = "hit" }] }, next with { Units = [next.Units[0] with { Clip = "hit" }] }]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, next with { Units = [next.Units[0] with { WeaponAttached = false }] }]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, next with { CombatTick = first.CombatTick }]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, next with { Units = [next.Units[0] with { AttackSequence = 2 }] }]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, next with { Units = [next.Units[0] with { BoneRotation = "first" }] }]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, next with { Units = [next.Units[0] with { EquipmentAligned = false }] }]));
        Assert.False(CombatProgressionProof.AttackAdvanced([first, next with { Units = [next.Units[0] with { EquipmentRotation = "first" }] }]));
    }
}
