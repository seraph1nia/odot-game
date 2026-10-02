using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class SplashTowerTests
{
    private static CombatSimulation Cluster(bool invalid = false)
    {
        var combat = new CombatSimulation(new Rules { SoldierHealth = 10, RangedHealth = 8, MageDamage = 4, DefenderDamage = 0, Combat = new() { MageVictimCap = 3, Crossbowman = new(1, 30, 30, 48) } });
        int mage = combat.Create(UnitType.Mage, 1, 1, 1, rank: 1);
        for (int n = 0; n < 5; n++)
        {
            int id = combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
            combat.Seed(combat.Read(id) with
            {
                Hex = CombatFixture.At(combat.Read(id), 8, n + 1),
                Deployed = true,
                Profile = combat.Read(id).Profile with { Damage = 0 }
            });
        }
        int friendly = combat.Create(UnitType.Swordsman, 1, 1, 1);
        combat.Seed(combat.Read(friendly) with { Hex = CombatFixture.At(combat.Read(friendly), 10, 1), Deployed = true, Profile = combat.Read(friendly).Profile with { Damage = 0 } });
        UnitState actor = combat.Read(mage);
        combat.Seed(actor with
        {
            Hex = CombatFixture.At(actor, 11, 1) with { Action = UnitActionKind.Windup, StartTick = 1, EndTick = 91, ActionSequence = 1 },
            TargetId = 2,
            PendingImpact = true,
            AttackSequence = 1,
            ActionStartTick = 1,
            ImpactTick = 25,
            ReadyTick = 91
        });
        combat.Step(1, []);
        if (invalid) combat.Transfer(2, 2);
        return combat;
    }
    [Fact]
    public void MageSplashCapsThreePrimaryThenDistanceIdAndExactFractionalDamage()
    {
        using var combat = Cluster();
        for (int tick = 2; tick <= 25; tick++) combat.Step(tick, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact && e.Unit?.Id == 1);
        Assert.Equal(3, impact.Victims.Length); Assert.Equal(2, impact.Victims[0]); Assert.Equal(3, impact.Victims.Distinct().Count()); Assert.Equal(420, impact.Damage);
        Assert.All(impact.Victims, id => Assert.Equal(380, combat.Read(id).Health));
        Assert.All(Enumerable.Range(2, 5).Except(impact.Victims), id => Assert.Equal(800, combat.Read(id).Health));
        Assert.Equal(1000, combat.Read(7).Health);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TransferredOrDeadPrimaryMissesWithoutRetargetingOrSplash(bool dead)
    {
        using var combat = Cluster(!dead); if (dead) combat.Remove(2); for (int tick = 2; tick <= 25; tick++) combat.Step(tick, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact && e.Unit?.Id == 1);
        Assert.False(impact.Landed); Assert.Empty(impact.Victims); Assert.Equal(800, combat.Read(3).Health);
    }
    [Theory]
    [InlineData(129, false)]
    [InlineData(130, true)]
    public void SplashUsesMovingPrimarySourceUntilArrivalAndExcludesQueuedAndDying(int impactTick, bool arrived)
    {
        using var combat = new CombatSimulation(new Rules { RangedHealth = 8, MageDamage = 4, Combat = new() { MageSplashHexRadius = 0 } });
        int actor = combat.Create(UnitType.Mage, 1, 1, 1);
        int primary = combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
        int source = combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
        int destination = combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
        int dying = combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
        int queued = combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
        UnitState mage = combat.Read(actor), moving = combat.Read(primary); HexPosition to = new(14, 1);
        combat.Seed(mage with
        {
            Hex = CombatFixture.At(mage, 11, 1) with { Action = UnitActionKind.Windup, ActionSequence = 1, StartTick = impactTick - 24, EndTick = impactTick + 66 },
            TargetId = primary,
            PendingImpact = true,
            AttackSequence = 1,
            ActionStartTick = impactTick - 24,
            ImpactTick = impactTick,
            ReadyTick = impactTick + 66
        });
        combat.Seed(moving with
        {
            Hex = CombatFixture.At(moving, 17, 1) with
            { Action = UnitActionKind.Moving, Destination = to, Transition = combat.Board.Transition(new(17, 1), to).Id, ActionSequence = 1, StartTick = 100, EndTick = 130 }
        });
        combat.Seed(combat.Read(source) with { Hex = CombatFixture.At(combat.Read(source), 17, 2) });
        combat.Seed(combat.Read(destination) with { Hex = CombatFixture.At(combat.Read(destination), 14, 2) });
        combat.Seed(combat.Read(dying) with
        {
            Health = 0,
            Hex = CombatFixture.At(combat.Read(dying), 17, 3) with
            { Lifecycle = UnitLifecycle.Dying, DeathStartTick = 100, DeathEndTick = 148 }
        });
        combat.Advance(impactTick, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact);
        Assert.True(impact.Landed); Assert.Equal(primary, impact.Victims[0]); Assert.Equal(2, impact.Victims.Length);
        Assert.Contains(arrived ? destination : source, impact.Victims); Assert.DoesNotContain(arrived ? source : destination, impact.Victims);
        Assert.DoesNotContain(dying, impact.Victims); Assert.DoesNotContain(queued, impact.Victims);
        Assert.Equal(800, combat.Read(queued).Health); Assert.Equal(0, combat.Read(dying).Health);
    }
    [Fact]
    public void TowerPlotsAndBuiltInDefenderUseTheSameDistanceAnchor()
    {
        using var match = new Match(combatSeed: 123); match.Join(); VillageStrategyTests.Act(match, 1, "start");
        Assert.True(VillageStrategyTests.Act(match, 1, "build", 0, Building.ArrowTower).Accepted);
        Assert.True(VillageStrategyTests.Act(match, 1, "ready").Accepted);
        Assert.True(match.Apply(1, new(1, match.Id, match.Phase, match.TurnSerial, "buy-plot", 1, 8, ExpectedExpansionCount: 0)).Accepted);
        Assert.True(VillageStrategyTests.Act(match, 1, "build", 8, Building.ArrowTower).Accepted);
        for (int n = 0; n < 3; n++) VillageStrategyTests.Act(match, 1, "ready");
        foreach (UnitState enemy in match.Enemies) match.Combat.Remove(enemy.Id);
        int near = match.Combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
        int far = match.Combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        match.Combat.Seed(match.Combat.Read(near) with { Hex = CombatFixture.At(match.Combat.Read(near), 8, 1) });
        match.Combat.Seed(match.Combat.Read(far) with { Hex = CombatFixture.At(match.Combat.Read(far), 2, 1) });
        match.Step();
        Assert.Equal(near, match.Players[1].Towers[0].TargetId); Assert.Equal(near, match.Players[1].Towers[8].TargetId);
        Assert.Equal(near, match.Players[1].Defender.TargetId);
        Assert.Equal(match.Players[1].Towers[0].ImpactTick, match.Players[1].Towers[8].ImpactTick);
    }
    [Theory]
    [InlineData(Building.ArrowTower, 12, 60, 1)]
    [InlineData(Building.CatapultTower, 30, 120, 3)]
    public void OrdinaryTowerOpeningHasStableTimedSourceAndNoUnitBody(Building building, int windup, int cadence, int cap)
    {
        using var match = new Match(); match.Join(); Assert.True(VillageStrategyTests.Act(match, 1, "start").Accepted);
        if (building == Building.CatapultTower)
        {
            Assert.True(VillageStrategyTests.Act(match, 1, "build", 0, Building.Stonecutter).Accepted);
            Assert.True(VillageStrategyTests.Act(match, 1, "build", 1, Building.MetalMine).Accepted);
            Assert.True(VillageStrategyTests.Act(match, 1, "build", 2, Building.Lumbermill).Accepted);
            for (int n = 0; n < 3; n++) Assert.True(VillageStrategyTests.Act(match, 1, "ready").Accepted);
        }
        Assert.True(VillageStrategyTests.Act(match, 1, "build", 4, building).Accepted);
        for (int n = 0; n < (building == Building.CatapultTower ? 1 : 4); n++) Assert.True(VillageStrategyTests.Act(match, 1, "ready").Accepted);
        int bodies = match.Combat.Snapshot().Length; match.Step();
        TowerState tower = match.Players[1].Towers[4]; Assert.Equal(1, tower.AttackSequence); Assert.Equal(1 + windup, tower.ImpactTick); Assert.Equal(1 + cadence, tower.ReadyTick);
        Assert.True(VillageStrategyTests.Act(match, 1, "pause").Accepted); CombatFixture.Steps(match, 100); Assert.Equal(tower, match.Players[1].Towers[4]);
        Assert.True(VillageStrategyTests.Act(match, 1, "resume").Accepted); CombatFixture.Steps(match, windup);
        CombatEvent impact = Assert.Single(match.Snapshot().CombatEvents, e => e.Type == CombatEventType.Impact && e.Tower?.Slot == 4);
        Assert.Null(impact.Unit); Assert.True(impact.Landed); Assert.InRange(impact.Victims.Length, 1, cap); Assert.Equal(4, impact.Tower!.Slot);
        Assert.Equal(bodies, match.Combat.Snapshot().Length); Assert.Empty(match.Players[1].Soldiers);
    }
}
