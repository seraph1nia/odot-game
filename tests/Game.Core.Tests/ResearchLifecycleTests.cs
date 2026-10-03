using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class ResearchLifecycleTests
{
    [Fact]
    public void ForeignStaleReadyAndDisconnectedTechnologyRequestsRejectWithoutMutation()
    {
        using var m = new Match(); m.Join(); m.Join(); Assert.True(VillageStrategyTests.Act(m, 1, "start").Accepted); m.Players[1].Research = new(3);
        Command command = new(1, m.Id, m.Phase, m.TurnSerial, "research-tech", 1, Technology: TechnologyId.MeleeFoundation);
        void Reject(Command request)
        { string before = JsonSerializer.Serialize(m.Snapshot(), WireJson.Options); Assert.False(m.Apply(1, request).Accepted); Assert.Equal(before, JsonSerializer.Serialize(m.Snapshot(), WireJson.Options)); }
        Reject(command with { City = 2 }); Reject(command with { MatchId = "stale" }); Reject(command with { TurnSerial = -1 }); Reject(command with { Technology = TechnologyId.Guardian });
        Assert.True(VillageStrategyTests.Act(m, 1, "ready").Accepted); Reject(command); Assert.True(VillageStrategyTests.Act(m, 1, "unready").Accepted);
        m.SetConnected(1, false); Reject(command);
    }
    [Fact]
    public void RewardResearchOverflowRejectsBattleBeforeUpkeepAndPauseFreezesActiveStatuses()
    {
        using var m = new Match(); m.Join(); Assert.True(VillageStrategyTests.Act(m, 1, "start").Accepted);
        for (int n = 0; n < 3; n++) Assert.True(VillageStrategyTests.Act(m, 1, "ready").Accepted);
        m.Players[1].Research = new(int.MaxValue); string before = JsonSerializer.Serialize(m.Snapshot(), WireJson.Options);
        Assert.False(VillageStrategyTests.Act(m, 1, "ready").Accepted); Assert.Equal(before, JsonSerializer.Serialize(m.Snapshot(), WireJson.Options));
        m.Players[1].Research = default; Assert.True(VillageStrategyTests.Act(m, 1, "ready").Accepted);
        UnitState enemy = m.Enemies[0]; m.Combat.Seed(enemy with { Statuses = StatusPolicy.Apply(new(), new(enemy.Id, StatusKind.Poison, 100, 1, m.Tick, 100), new()) });
        Assert.True(VillageStrategyTests.Act(m, 1, "pause").Accepted); before = JsonSerializer.Serialize(m.Snapshot(), WireJson.Options);
        for (int n = 0; n < 100; n++) m.Step(); Assert.Equal(before, JsonSerializer.Serialize(m.Snapshot(), WireJson.Options));
    }
    [Fact]
    public void AfflictedQueuedEnemyCanDieWithoutAdmissionAndWaveStopCleansSurvivorsWithoutHealing()
    {
        using var combat = new CombatSimulation(new()); int id = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        CombatUnit enemy = combat.Inspect(id); combat.Seed(enemy with { Health = 100, Statuses = StatusPolicy.Apply(new(), new(id, StatusKind.Poison, 99, 1, 0, 100), new()) });
        combat.Advance(60, []); Assert.Empty(combat.Enemies()); Assert.Empty(combat.Reservations.Positions); Assert.Contains(combat.Events(), e => e.Type == CombatEventType.Death && e.Periodic.Length == 1);
        int ally = combat.Create(UnitType.Swordsman, 1, 1, 1); CombatUnit soldier = combat.Inspect(ally);
        combat.Seed(soldier with { Health = 1000, Statuses = StatusPolicy.Apply(new(), new(ally, StatusKind.Burn, 99, 1, 60, 100), new()) });
        combat.StopActions([]); Assert.Equal(1000, combat.Read(ally).Health); Assert.False(combat.Read(ally).Statuses.Active);
        combat.Cleanup(240); Assert.Equal(1000, combat.Read(ally).Health);
    }
    [Fact]
    public void NewAuthoritativeActionsSampleChillAndLaterRefreshCannotChangeTheirCommitments()
    {
        using var combat = new CombatSimulation(new()); int a = combat.Create(UnitType.Swordsman, 1, 1, 1), b = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        foreach (var (id, cell) in new[] { (a, 11), (b, 8) }) { UnitState unit = combat.Read(id); combat.Seed(unit with { Hex = CombatFixture.At(unit, cell, 1) }); }
        CombatUnit actor = combat.Inspect(a); combat.Seed(actor with { Statuses = StatusPolicy.Apply(new(), new(a, StatusKind.Chill, b, 1, 0, 40), new()) });
        combat.StartActions([]); UnitState slowed = combat.Read(a); Assert.Equal(17, slowed.ImpactTick); Assert.Equal(85, slowed.ReadyTick);
        CombatUnit current = combat.Inspect(a); combat.Seed(current with { Statuses = StatusPolicy.Apply(current.Statuses, new(a, StatusKind.Chill, b, 2, 5, 20), new()) });
        Assert.Equal(slowed.ImpactTick, combat.Read(a).ImpactTick); Assert.Equal(slowed.ReadyTick, combat.Read(a).ReadyTick);
    }
}
