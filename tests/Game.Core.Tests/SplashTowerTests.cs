using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class SplashTowerTests
{
    private static CombatSimulation Cluster(bool invalid = false)
    {
        var combat = new CombatSimulation(new Rules { DefenderDamage = 0 });
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
        combat.Seed(combat.Read(friendly) with { Hex = CombatFixture.At(combat.Read(friendly), 10, 7), Deployed = true, Profile = combat.Read(friendly).Profile with { Damage = 0 } });
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
        Assert.Equal(3, impact.Victims.Length); Assert.Equal(2, impact.Victims[0]); Assert.Equal(3, impact.Victims.Distinct().Count()); Assert.Equal(315, impact.Damage);
        Assert.All(impact.Victims, id => Assert.Equal(485, combat.Read(id).Health));
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
    [InlineData(Building.ArrowTower, 12, 60, 1)]
    [InlineData(Building.CatapultTower, 30, 120, 3)]
    public void OrdinaryTowerOpeningHasStableTimedSourceAndNoUnitBody(Building building, int windup, int cadence, int cap)
    {
        using var match = new Match(); match.Join(); Assert.True(VillageStrategyTests.Act(match, 1, "start").Accepted);
        Assert.True(VillageStrategyTests.Act(match, 1, "build", 8, building).Accepted);
        for (int n = 0; n < 4; n++) Assert.True(VillageStrategyTests.Act(match, 1, "ready").Accepted);
        int bodies = match.Combat.Snapshot().Length; match.Step();
        TowerState tower = match.Players[1].Towers[8]; Assert.Equal(1, tower.AttackSequence); Assert.Equal(1 + windup, tower.ImpactTick); Assert.Equal(1 + cadence, tower.ReadyTick);
        Assert.True(VillageStrategyTests.Act(match, 1, "pause").Accepted); CombatFixture.Steps(match, 100); Assert.Equal(tower, match.Players[1].Towers[8]);
        Assert.True(VillageStrategyTests.Act(match, 1, "resume").Accepted); CombatFixture.Steps(match, windup);
        CombatEvent impact = Assert.Single(match.Snapshot().CombatEvents, e => e.Type == CombatEventType.Impact && e.Tower?.Slot == 8);
        Assert.Null(impact.Unit); Assert.True(impact.Landed); Assert.InRange(impact.Victims.Length, 1, cap); Assert.Equal(8, impact.Tower!.Slot);
        Assert.Equal(bodies, match.Combat.Snapshot().Length); Assert.Empty(match.Players[1].Soldiers);
    }
}
