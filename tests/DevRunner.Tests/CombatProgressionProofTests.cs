using DevRunner;
using Game;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class CombatProgressionProofTests
{
    private static readonly HexBoard Board = new SharedArena([1, 2]).Board;
    private static UiObservation Frame(double tick, int owner = 1, int slot = 0, int capacity = 3, bool boss = false, bool dead = false)
    {
        HexUnitState hex = new(1, owner, Faction.Adventurers, dead ? UnitLifecycle.Dying : UnitLifecycle.Alive, new(14, 1), UnitActionKind.Moving,
            new(Board.Cell(14).Neighbors.First(), 1), StartTick: 100, EndTick: 130, FrozenMoveTicks: dead ? 10 : 0);
        var position = SharedCombatProjection.Position(Board, hex, slot, capacity, boss, tick);
        return new()
        {
            CombatTick = tick,
            Units = [new() { Id = 1, Destination = owner, FormationSlot = slot, IsBoss = boss, Dead = dead, Visible = true, Deployed = true, Hex = hex,
                X = position.X, Z = position.Z, Clip = "walk", WalkingBlend = .8 }]
        };
    }
    [Fact]
    public void SharedProjectionHasIndependentCenteredHexAndCosmeticSlotGeometry()
    {
        // Analytical two-sector geometry, not another call to the projection:
        // 14 columns x9 rows, 3-world-unit column spacing, 2.598076 row
        // spacing. The connected midpoint is (19.5,0,10.392304), NOT +40
        // per owning city. Slots are cosmetic; a boss has no slot offset.
        var source = Game.SharedCombatProjection.CellPosition(Board, 14);
        Assert.Equal(19.5f, source.X, .0001f); Assert.Equal(-10.392304f, source.Z, .0001f);
        var boss = Game.SharedCombatProjection.Formation(2, 3, true); Assert.Equal(System.Numerics.Vector3.Zero, boss);
        var slot = Game.SharedCombatProjection.Formation(0, 3, false);
        Assert.Equal(0, slot.X, .0001f); Assert.Equal(-.65f, slot.Z, .0001f);
        var body = new HexUnitState(1, 2, Faction.Adventurers, UnitLifecycle.Alive, new(14, 1));
        var position = Game.SharedCombatProjection.Position(Board, body, 0, 3, false, 100);
        Assert.Equal(19.5f, position.X, .0001f); Assert.Equal(88.957696f, position.Z, .0001f);
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
    [InlineData(1, 0, 3, false, false)]
    [InlineData(2, 2, 3, false, false)]
    [InlineData(2, 4, 6, false, false)]
    [InlineData(2, 2, 3, true, false)]
    [InlineData(2, 2, 3, false, true)]
    public void SharedMotionRetainsOwnerIndependenceFormationBossAndFrozenCorpseClock(int owner, int slot, int capacity, bool boss, bool dead)
    {
        UiObservation first = Frame(110, owner, slot, capacity, boss, dead), next = Frame(120, owner, slot, capacity, boss, dead);
        CombatProgressionProof.Motion(first, Board, capacity); CombatProgressionProof.Motion(next, Board, capacity);
        Assert.Equal(!dead, CombatProgressionProof.MovementAdvanced([first, next]));
        Assert.False(CombatProgressionProof.MovementAdvanced([first, first]));
        Assert.Throws<InvalidOperationException>(() => CombatProgressionProof.Motion(first with { Units = [first.Units[0] with { X = first.Units[0].X + 40 }] }, Board, capacity));
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
