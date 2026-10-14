using Xunit;

namespace Game.Core.Tests;

// Historical name retained for discovery; the active contract is one selected
// squad with independent X/Y channels, not cross-hex explosions or victim caps.
// Tower payment/upgrades/retry/codec/resume live in ArmyPlotTowerTests; physical
// defense timing/range/pre-impact aggregation live in DefensePolicyTests.
public sealed class SplashTowerTests
{
    [Theory]
    [InlineData(4, 1)]
    [InlineData(0, 1)]
    [InlineData(4, 0)]
    [InlineData(0, 0)]
    public void RankedXYStrikesAffectOnlyTheSelectedFiveMemberGroup(int x, int y)
    {
        using var combat = new CombatSimulation(new()
        { RangedHealth = 8, MageDamage = x, Arena = new() { SquadCapacity = 5, MageSecondaryDamage = y } }, seed: 123);
        int mage = CombatFixture.Group(combat, UnitType.Mage, 1, 1, Faction.Adventurers, 17, rank: 1)[0];
        int[] selected = CombatFixture.Group(combat, UnitType.Crossbowman, 5, 0, Faction.Skeletons, 18);
        int neighbor = CombatFixture.Group(combat, UnitType.Crossbowman, 1, 0, Faction.Skeletons, 19)[0];
        int friendly = CombatFixture.Group(combat, UnitType.Swordsman, 1, 1, Faction.Adventurers, 24)[0];
        CombatUnit actor = combat.Inspect(mage);
        combat.Seed(actor with { Action = CombatActions.Attack(actor.Action, new(CombatTargetKind.Unit, combat.Read(selected[0]).SquadId), 0, 24, 66) });
        combat.Advance(24, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.True(impact.Landed);
        Assert.Contains(impact.TargetId, selected);
        int primary = x * 105, secondary = y * 105; // exact rank-one fractional health-point contract
        Assert.Equal(primary + 4 * secondary, impact.Damage);
        foreach (int id in selected)
        {
            int damage = id == impact.TargetId ? primary : secondary;
            Assert.Equal(800 - damage, combat.Read(id).Health);
            Assert.Equal(damage > 0, impact.Victims.Contains(id));
        }
        Assert.Equal(800, combat.Read(neighbor).Health);
        Assert.Equal(combat.Read(friendly).Profile.Health, combat.Read(friendly).Health);
        Assert.Equal(impact.Victims.Length, impact.Victims.Distinct().Count());
        Assert.DoesNotContain(neighbor, impact.Victims); Assert.DoesNotContain(friendly, impact.Victims);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MemberLossOrSectorRedirectionDoesNotCancelTheCommittedGroup(bool removeMember)
    {
        using var combat = new CombatSimulation(new() { Arena = new() { SquadCapacity = 5 } });
        int mage = CombatFixture.Group(combat, UnitType.Mage, 1, 1, Faction.Adventurers, 17)[0];
        int[] group = CombatFixture.Group(combat, UnitType.Crossbowman, 5, 0, Faction.Skeletons, 18);
        int target = combat.Read(group[0]).SquadId;
        CombatUnit actor = combat.Inspect(mage);
        combat.Seed(actor with { Action = CombatActions.Attack(actor.Action, new(CombatTargetKind.Unit, target), 0, 24, 66) });
        if (removeMember) combat.Remove(group[0]); else combat.Transfer(group[0], 2);
        combat.Advance(24, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.True(impact.Landed); Assert.Equal(removeMember ? 4 : 5, impact.Victims.Length);
        Assert.All(group.Skip(1), id => Assert.True(combat.Read(id).Health < combat.Read(id).Profile.Health));
        Assert.All(group.Skip(removeMember ? 1 : 0), id => Assert.Equal(new HexPosition(18, 1), combat.Read(id).Hex!.Position));
        Assert.Equal(target, combat.SquadSnapshots().Single(s => s.Owner == 1).TargetId);
    }

    [Fact]
    public void FinalGroupLossMissesInsteadOfRetargetingItsAdjacentReplacement()
    {
        using var combat = new CombatSimulation(new());
        int mage = CombatFixture.Group(combat, UnitType.Mage, 1, 1, Faction.Adventurers, 17)[0];
        int[] selected = CombatFixture.Group(combat, UnitType.Crossbowman, 3, 0, Faction.Skeletons, 18);
        int neighbor = CombatFixture.Group(combat, UnitType.Crossbowman, 1, 0, Faction.Skeletons, 19)[0];
        CombatUnit actor = combat.Inspect(mage);
        combat.Seed(actor with { Action = CombatActions.Attack(actor.Action, new(CombatTargetKind.Unit, combat.Read(selected[0]).SquadId), 0, 24, 66) });
        foreach (int id in selected) combat.Remove(id);
        combat.Advance(24, []);
        CombatEvent miss = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.False(miss.Landed); Assert.Empty(miss.Victims); Assert.Equal(0, miss.Damage);
        Assert.Equal(combat.Read(neighbor).Profile.Health, combat.Read(neighbor).Health);
        Assert.Equal(UnitActionKind.Recovery, combat.Read(mage).Hex!.Action);
    }

    [Theory]
    [InlineData(129, true)]
    [InlineData(130, false)]
    public void MovingTargetGroupUsesOneSourceAndNeverSplashesIntoOtherSquads(int impactTick, bool landed)
    {
        using var combat = new CombatSimulation(new()
        { RangedHealth = 8, MageDamage = 4, Combat = new() { MageHexRange = 1 }, Arena = new() { MageSecondaryDamage = 1 } });
        int mage = CombatFixture.Group(combat, UnitType.Mage, 1, 1, Faction.Adventurers, 18)[0];
        int[] selected = CombatFixture.Group(combat, UnitType.Crossbowman, 3, 0, Faction.Skeletons, 17);
        int neighbor = CombatFixture.Group(combat, UnitType.Crossbowman, 1, 0, Faction.Skeletons, 19)[0];
        CombatUnit mover = combat.Inspect(selected[0]), actor = combat.Inspect(mage);
        combat.Seed(mover with { Action = CombatActions.Move(mover.Action, combat.Board.Transition(new(17, 1), new(16, 1)), 100, 30) });
        combat.Seed(actor with { Action = CombatActions.Attack(actor.Action, new(CombatTargetKind.Unit, mover.SquadId), impactTick - 24, 24, 66) });
        combat.Advance(impactTick, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.Equal(landed, impact.Landed); Assert.Equal(landed ? 3 : 0, impact.Victims.Length);
        Assert.Equal(landed ? 1800 : 2400, selected.Sum(id => combat.Read(id).Health));
        Assert.All(selected, id => Assert.Equal(landed ? 17 : 16, combat.Read(id).Hex!.Position.Cell));
        Assert.Equal(800, combat.Read(neighbor).Health);
    }

    [Fact]
    public void NonparticipatingAndDyingMembersCannotReceiveGroupDirectDamage()
    {
        using var combat = new CombatSimulation(new() { Arena = new() { SquadCapacity = 5 } });
        int mage = CombatFixture.Group(combat, UnitType.Mage, 1, 1, Faction.Adventurers, 17)[0];
        int[] group = CombatFixture.Group(combat, UnitType.Crossbowman, 5, 0, Faction.Skeletons, 18);
        CombatUnit corpse = combat.Inspect(group[4]);
        combat.Seed(corpse with { Health = 0, Location = corpse.Location with { Lifecycle = UnitLifecycle.Dying, DeathStartTick = 0, DeathEndTick = 48 } });
        CombatUnit reserve = combat.Inspect(group[3]);
        combat.Seed(reserve with { Location = reserve.Location with { Lifecycle = UnitLifecycle.Reserve } });
        // Restore the actor pose without changing either inactive member's life.
        CombatUnit live = combat.Inspect(group[0]);
        combat.Seed(live with { Location = new(UnitLifecycle.Alive, new(18, 1)) });
        CombatUnit actor = combat.Inspect(mage);
        combat.Seed(actor with { Action = CombatActions.Attack(actor.Action, new(CombatTargetKind.Unit, live.SquadId), 0, 24, 66) });
        combat.Advance(24, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.Equal(group.Take(3).Order(), impact.Victims.Order());
        Assert.Equal(reserve.Health, combat.Read(reserve.Id).Health); Assert.Equal(0, combat.Read(corpse.Id).Health);
        Assert.False(combat.Read(reserve.Id).Participating); Assert.Empty(combat.Read(corpse.Id).Statuses.Poison);
    }
}
