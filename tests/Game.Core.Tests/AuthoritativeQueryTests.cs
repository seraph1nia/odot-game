using Xunit;

namespace Game.Core.Tests;

public sealed class AuthoritativeQueryTests
{
    [Fact]
    public void LevelFiveHarnessObservationMatchesProjectedParticipationAtEveryLifecycle()
    {
        using var match = new Match(combatSeed: 1);
        match.Join();
        int id = match.Combat.Create(UnitType.Swordsman, 1, 1, 1, level: 5);
        match.Combat.AdmitEntries();
        foreach (UnitLifecycle life in new[] { UnitLifecycle.Queued, UnitLifecycle.Reserve, UnitLifecycle.Alive, UnitLifecycle.Dying })
        {
            UnitState value = match.Combat.Read(id);
            match.Combat.Seed(value with
            {
                Health = life == UnitLifecycle.Dying ? 0 : value.Health,
                Hex = value.Hex! with { Lifecycle = life, DeathStartTick = 0, DeathEndTick = 48 }
            });
            bool projected = match.Players.Values.SelectMany(city => city.Soldiers).Any(u => u.Level == 5 && u.Participating && u.Deployed);
            var work = new WorkCounters(); match.SetWorkCounters(work); work.Reset();
            Assert.Equal(projected, match.Combat.HasParticipatingSoldierLevel(5));
            Assert.Equal(0, work.Snapshot()[WorkMetric.UnitProjections]);
        }
    }
    [Fact]
    public void MembershipIncludesQueuedAndReserveExcludesDyingAndDoesNotProject()
    {
        using var match = new Match(combatSeed: 1);
        var work = new WorkCounters(); match.SetWorkCounters(work);
        int queued = match.Combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        int reserve = match.Combat.Create(UnitType.Mage, 0, 1, 1, Faction.Skeletons);
        UnitState old = match.Combat.Read(reserve);
        match.Combat.Seed(old with { Hex = old.Hex! with { Lifecycle = UnitLifecycle.Reserve } });
        int dying = CombatDecisionRegressionTests.At(match.Combat, Faction.Skeletons, 8);
        UnitState body = match.Combat.Read(dying);
        match.Combat.Seed(body with { Health = 0, Hex = body.Hex! with { Lifecycle = UnitLifecycle.Dying, DeathStartTick = 0, DeathEndTick = 48 } });
        work.Reset();
        Assert.Equal(new[] { queued, reserve }, match.Combat.EnemyMembership().Select(u => u.Id));
        Assert.Equal(0, work.Snapshot()[WorkMetric.UnitProjections]);
        match.Combat.Transfer(queued, 2);
        Assert.Equal(2, match.Combat.EnemyMembership()[0].Destination);
        Assert.Equal(UnitLifecycle.Reserve, match.Combat.EnemyMembership()[1].Location.Lifecycle);
        Assert.True(match.Combat.HasDeaths);
        match.Combat.Cleanup(48);
        Assert.False(match.Combat.HasDeaths);
    }

    [Fact]
    public void SoldierProjectionFiltersBeforeConstructingDtosAndRefreshesAtTheSameTick()
    {
        using var match = new Match(combatSeed: 1);
        var work = new WorkCounters(); match.SetWorkCounters(work);
        int friendly = match.Combat.Create(UnitType.Swordsman, 1, 1, 1);
        match.Combat.Create(UnitType.Mage, 2, 2, 2);
        match.Combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        work.Reset(); Assert.Equal(friendly, Assert.Single(match.Combat.Soldiers(1)).Id);
        Assert.Equal(1, work.Snapshot()[WorkMetric.UnitProjections]);
        int next = match.Combat.Create(UnitType.Mage, 1, 1, 1);
        Assert.Equal(new[] { friendly, next }, match.Combat.SoldierMembership(1).Select(u => u.Id));
        match.Combat.Remove(friendly);
        Assert.Equal(next, Assert.Single(match.Combat.SoldierMembership(1)).Id);
    }
}
