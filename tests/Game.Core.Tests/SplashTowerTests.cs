using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class SplashTowerTests
{
    private static readonly int[] CappedVictims = [2, 3, 4];
    private static CombatSimulation Cluster(bool invalid = false)
    {
        var combat = new CombatSimulation(new Rules { DefenderDamage = 0 });
        int mage = combat.Create(UnitType.Mage, 1, 1, 1, rank: 1);
        combat.Seed(combat.Read(mage) with { Position = 2, Deployed = true, Profile = combat.Read(mage).Profile with { Speed = 0 } });
        for (int n = 0; n < 5; n++)
        {
            int id = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
            combat.Seed(combat.Read(id) with { Position = 4.5 + (n > 2 ? 0.5 : 0), Lateral = n switch { 1 => -.48, 2 => .48, 3 => -.48, 4 => .48, _ => 0 }, Deployed = true, Profile = combat.Read(id).Profile with { Speed = 0, Damage = 0 } });
        }
        int friendly = combat.Create(UnitType.Swordsman, 1, 1, 1);
        combat.Seed(combat.Read(friendly) with { Position = 4.5, Lateral = .96, Deployed = true, Profile = combat.Read(friendly).Profile with { Speed = 0, Damage = 0 } });
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
        Assert.Equal(CappedVictims, impact.Victims); Assert.Equal(210, impact.Damage);
        Assert.All(impact.Victims, id => Assert.Equal(790, combat.Read(id).Health));
        Assert.Equal(1000, combat.Read(5).Health); Assert.Equal(1000, combat.Read(7).Health);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TransferredOrDeadPrimaryMissesWithoutRetargetingOrSplash(bool dead)
    {
        using var combat = Cluster(!dead); if (dead) combat.Remove(2); for (int tick = 2; tick <= 25; tick++) combat.Step(tick, []);
        CombatEvent impact = Assert.Single(combat.Events(), e => e.Type == CombatEventType.Impact && e.Unit?.Id == 1);
        Assert.False(impact.Landed); Assert.Empty(impact.Victims); Assert.Equal(1000, combat.Read(3).Health);
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
        CombatEvent impact = Assert.Single(match.Snapshot().CombatEvents, e => e.Type == CombatEventType.Impact && e.Tower is not null);
        Assert.Null(impact.Unit); Assert.True(impact.Landed); Assert.InRange(impact.Victims.Length, 1, cap); Assert.Equal(8, impact.Tower!.Slot);
        Assert.Equal(bodies, match.Combat.Snapshot().Length); Assert.Empty(match.Players[1].Soldiers);
    }
}
