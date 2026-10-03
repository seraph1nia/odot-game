using Xunit;

namespace Game.Core.Tests;

public sealed class SnapshotEfficiencyTests
{
    [Fact]
    public void CompleteSnapshotProjectsEachStoredUnitOnceAndDetachesNestedValues()
    {
        using var match = new Match(combatSeed: 1);
        City city = match.Join()!; match.Join();
        int first = match.Combat.Create(UnitType.Swordsman, 1, 1, 1);
        match.Combat.Create(UnitType.Mage, 2, 2, 2);
        match.Combat.Create(UnitType.Crossbowman, 0, 1, 1, Faction.Skeletons);
        UnitState unit = match.Combat.Read(first);
        match.Combat.Seed(unit with { Decision = unit.Decision! with { Route = [new(11, 1)], Visited = [17] } });
        city.LastUpkeep = new(1, 2, [first], [999]);
        var work = new WorkCounters(); match.SetWorkCounters(work); work.Reset();
        MatchSnapshot a = match.Snapshot();
        Assert.Equal(3, work.Snapshot()[WorkMetric.UnitProjections]); Assert.Equal(1, work.Snapshot()[WorkMetric.WorldViews]);
        MatchSnapshot b = match.Snapshot();
        a.Rules.Combat.Board.Cells[0].Neighbors[0] = 999;
        a.Rules.Campaign.Waves[0].Entries[0] = new(UnitType.Mage, 6, 99);
        a.WaveCatalog[0].Entries[0] = new(UnitType.Mage, 6, 99);
        a.BuildingCatalog.Single(x => x.Type == Building.Barracks).Recruits![0] = UnitType.Mage;
        a.Players[0].Soldiers[0].Decision!.Route[0] = new(999, 999);
        a.Players[0].Soldiers[0].Decision!.Visited[0] = 999;
        a.Players[0].LastUpkeep!.Participating[0] = 999;
        a.Players[0].LastUpkeep!.Unfed[0] = first;
        a.Players[0].RecruitmentQuotes[0] = a.Players[0].RecruitmentQuotes[0] with { Upkeep = 999 };
        MatchSnapshot fresh = match.Snapshot();
        Assert.Equal(Profiling.LargeBattle.Digest(b), Profiling.LargeBattle.Digest(fresh));
        Assert.Equal(first, city.LastUpkeep.Participating[0]); Assert.Equal(999, city.LastUpkeep.Unfed[0]);
        Assert.Equal(Profiling.LargeBattle.Digest(b.Players[0]), Profiling.LargeBattle.Digest(city.Snapshot()));
    }

    [Fact]
    public void ProfileAndQuoteCachesAreValidatedOwnedAndMutationSafe()
    {
        using var first = new Match(combatSeed: 1); using var second = new Match(combatSeed: 1);
        var work = new WorkCounters(); first.SetWorkCounters(work);
        HexCombatProfile original = first.Configuration.Unit(UnitType.Mage, 1, level: 4);
        work.Reset(); Assert.Same(original, first.Configuration.Unit(UnitType.Mage, 1, level: 4));
        Assert.Equal(1, work.Snapshot()[WorkMetric.ProfileCacheHits]); Assert.Equal(0, work.Snapshot()[WorkMetric.ProfileCacheMisses]);
        Assert.NotSame(original, second.Configuration.Unit(UnitType.Mage, 1, level: 4));
        int size = first.Configuration.CachedProfiles;
        Assert.ThrowsAny<Exception>(() => first.Configuration.Unit(UnitType.Mage, level: 256));
        Assert.Throws<ArgumentOutOfRangeException>(() => first.Configuration.Unit(UnitType.Mage, rank: 3));
        Assert.Equal(size, first.Configuration.CachedProfiles);
        RecruitmentQuote[] quotes = first.Economy.RecruitmentQuotes(first.Configuration);
        RecruitmentQuote before = quotes[0]; quotes[0] = before with { Upkeep = 999 };
        Assert.Equal(before, first.Economy.RecruitmentQuotes(first.Configuration)[0]);
        RecruitmentQuote ranked = first.Economy.RecruitmentQuotes(first.Configuration, new(Melee: 1))[0];
        Assert.True(ranked.Profile.Health > before.Profile.Health);
        first.Dispose(); Assert.Equal(0, first.Configuration.CachedProfiles);
    }

    [Fact]
    public void AdmissionRefreshesAfterPlacementAndFailedBandsReuseExactInputs()
    {
        using Match match = Profiling.LargeBattle.Create(512, 1);
        var work = new WorkCounters(); match.SetWorkCounters(work);
        int queued = match.Combat.Units().Count(u => u.Location.Lifecycle == UnitLifecycle.Queued);
        match.Combat.AdmitEntries();
        Assert.Equal(queued, work.Snapshot()[WorkMetric.QueuedCandidates]);
        Assert.True(work.Snapshot()[WorkMetric.OccupancyChecks] < queued * 32 * 6);
        CombatUnit removed = match.Combat.Units().First(u => u.IsTargetable && u.Faction == Faction.Adventurers && match.Configuration.Board.Rear(u.Faction).Contains(u.Location.Position.Cell));
        match.Combat.Remove(removed.Id);
        match.Combat.AdmitEntries();
        Assert.Equal(queued - 1, match.Combat.Units().Count(u => u.Location.Lifecycle == UnitLifecycle.Queued));
        Assert.Contains(match.Combat.Units(), u => u.Id != removed.Id && u.Location.Position == removed.Location.Position && u.IsTargetable);
    }

    [Fact]
    public void ReturnedEventDecisionAndVictimArraysCannotChangeHistory()
    {
        using var combat = new CombatSimulation(new(), seed: 1);
        int id = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 11);
        CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 8);
        UnitState actor = combat.Read(id);
        combat.Seed(actor with { Decision = actor.Decision! with { Route = [new(11, 1)], Visited = [17] } });
        for (int tick = 1; tick < 20; tick++) combat.Step(tick, []);
        CombatEvent[] before = combat.Events(); string expected = Profiling.LargeBattle.Digest(before);
        Assert.Contains(before, e => e.Victims.Length > 0);
        Assert.Contains(before, e => e.Unit?.Decision?.Route.Length > 0);
        foreach (CombatEvent entry in combat.Events())
        {
            if (entry.Victims.Length > 0) entry.Victims[0] = 999;
            if (entry.Unit?.Decision?.Route.Length > 0) entry.Unit.Decision.Route[0] = new(999, 999);
        }
        Assert.Equal(expected, Profiling.LargeBattle.Digest(combat.Events()));
    }
}
