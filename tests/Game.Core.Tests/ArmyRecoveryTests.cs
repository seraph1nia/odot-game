using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class ArmyRecoveryTests
{
    private static Command Request(Match match, string action, int slot = -1, int unit = 0, Building building = Building.Empty)
        => new(1, match.Id, match.Phase, match.TurnSerial, action, 1, slot, building,
            ExpectedGeneration: slot >= 0 ? match.Players[1].Slots[slot].Generation : 0, UnitId: unit,
            ExpectedTrackLevel: slot >= 0 ? action == "upgrade-healing" ? match.Players[1].Slots[slot].HealingLevel : action == "upgrade-capacity" ? match.Players[1].Slots[slot].CapacityLevel : -1 : -1);
    private static void Act(Match match, string action, int slot = -1, int unit = 0, Building building = Building.Empty)
        => Assert.True(match.Apply(1, Request(match, action, slot, unit, building)).Accepted);
    private static Match Start(int food = 100)
    {
        var match = new Match(new Rules { Campaign = CampaignFixture.Three(1, 1, 1) }, combatSeed: 1);
        City city = match.Join()!; Act(match, "start");
        city.Gold = 100; city.Wood = 100; city.Stone = 100; city.Metal = 100; city.Food = food;
        Act(match, "build", 0, building: Building.Barracks); Act(match, "build", 1, building: Building.TownHall);
        return match;
    }
    private static UnitState Read(Match match, int id) => match.Players[1].Soldiers.Single(u => u.Id == id);
    private static string State(Match match) => JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
    private static void Complete(Match match)
    {
        Act(match, "ready"); Assert.Equal(Phase.Combat, match.Phase);
        for (int ticks = 0; ticks < 2000 && (match.Phase == Phase.Combat || match.Combat.HasDeaths); ticks++) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.False(match.Combat.HasDeaths);
    }

    [Fact]
    public void FieldFirstFoodIncludesStoredButNeverMakesStoredCombatants()
    {
        UnitState Unit(int id, int level, bool stored) => new(id, 100)
        { Level = level, Assignment = new(stored ? 1 : -1, stored ? 1 : 0, stored ? 0 : 17, id) };
        UnitState[] soldiers = [Unit(1, 1, false), Unit(2, 5, true), Unit(3, 2, false)];
        var economy = new EconomyConfiguration(new());
        BattleFoodForecast low = BattleFood.Forecast(soldiers, 2, economy);
        Assert.Equal(3, low.Demand); Assert.Equal(2, low.FieldDemand); Assert.Equal(1, low.StoredDemand);
        Assert.Equal([3, 1], low.Funded); Assert.Equal([3, 1], low.Participating); Assert.Equal([2], low.Unfed);
        BattleFoodForecast enough = BattleFood.Forecast(soldiers, 3, economy);
        Assert.Equal([3, 1, 2], enough.Funded); Assert.Equal([3, 1], enough.Participating); Assert.Empty(enough.Unfed);
    }

    [Fact]
    public void ActualProductionHealsOnlyCompletedPaidStoredIdsAndUsesCeiling()
    {
        using Match match = Start();
        for (int i = 0; i < 6; i++) Act(match, "recruit", 0);
        int reserve = match.Players[1].Soldiers[0].Id;
        match.Combat.Seed(Read(match, reserve) with { Health = 2901 }); Act(match, "store", 1, reserve);
        for (int i = 0; i < 3; i++) { Act(match, "ready"); Assert.Equal(2901, Read(match, reserve).Health); }
        Assert.False(Read(match, reserve).RecoveryEligible); Assert.Equal(Phase.Preparation, match.Phase);
        Complete(match);
        Assert.True(Read(match, reserve).RecoveryEligible); Assert.Equal(2901, Read(match, reserve).Health);
        Assert.False(Read(match, reserve).Deployed); Assert.False(Read(match, reserve).Participating);
        Assert.Contains(reserve, match.Players[1].LastUpkeep!.Funded); Assert.DoesNotContain(reserve, match.Players[1].LastUpkeep!.Participating);
        Act(match, "recruit", 0);
        int fresh = match.Players[1].Soldiers.Max(u => u.Id);
        match.Combat.Seed(Read(match, fresh) with { Health = 2901 }); Act(match, "store", 1, fresh);
        Assert.False(Read(match, fresh).RecoveryEligible); Assert.DoesNotContain(fresh, match.Players[1].LastUpkeep!.Funded);
        int survivor = match.Players[1].Soldiers.First(u => !u.Assignment!.Stored).Id;
        match.Combat.Seed(Read(match, survivor) with { Health = 3101 }); Act(match, "store", 1, survivor);
        Assert.True(Read(match, survivor).RecoveryEligible); Assert.Equal(3101, Read(match, survivor).Health);
        Act(match, "upgrade-healing", 1); Assert.Equal(2901, Read(match, reserve).Health);
        Act(match, "pause"); for (int i = 0; i < 5; i++) match.Step(); Assert.Equal(2901, Read(match, reserve).Health); Act(match, "resume");
        for (int i = 1; i <= 3; i++)
        {
            Act(match, "ready"); Assert.Equal(Math.Min(4000, 2901 + 400 * i), Read(match, reserve).Health);
            Assert.Equal(Math.Min(4000, 3101 + 400 * i), Read(match, survivor).Health);
            Assert.Equal(2901, Read(match, fresh).Health);
        }
        string preparation = State(match); match.Step(); Assert.Equal(preparation, State(match));
        Assert.Equal(101, ArmyConfiguration.HealedHealth(100, 101, 5));
        Assert.Equal(111, ArmyConfiguration.HealedHealth(100, 1010, 1)); // ceiling(10.1) is 11.
    }

    [Fact]
    public void UnderfedStoredIdsDoNotHealAndProductionPreflightCannotPartiallyHeal()
    {
        using Match match = Start(food: 2);
        for (int i = 0; i < 3; i++) Act(match, "recruit", 0);
        int reserve = match.Players[1].Soldiers[0].Id; match.Combat.Seed(Read(match, reserve) with { Health = 3000 }); Act(match, "store", 1, reserve);
        for (int i = 0; i < 3; i++) Act(match, "ready");
        Complete(match); Assert.False(Read(match, reserve).RecoveryEligible);
        Assert.Contains(reserve, match.Players[1].LastUpkeep!.Unfed);
        int survivor = match.Players[1].Soldiers.First(u => !u.Assignment!.Stored).Id;
        match.Combat.Seed(Read(match, survivor) with { Health = 3000 }); Act(match, "store", 1, survivor);
        City city = match.Players[1]; city.Gold = int.MaxValue;
        string before = State(match); Assert.False(match.Apply(1, Request(match, "ready")).Accepted); Assert.Equal(before, State(match));
        city.Gold = 0; Act(match, "ready"); Assert.Equal(3000, Read(match, reserve).Health); Assert.Equal(3200, Read(match, survivor).Health);
        Act(match, "send", 1, survivor); int health = Read(match, survivor).Health;
        Act(match, "ready"); Assert.Equal(health, Read(match, survivor).Health); Assert.Equal(3000, Read(match, reserve).Health);
    }

    [Fact]
    public void FinalVictoryRestoresHomesAndEliminationRemovesStoredIdentity()
    {
        using (Match match = Start())
        {
            for (int i = 0; i < 6; i++) Act(match, "recruit", 0);
            for (int wave = 1; wave <= 3; wave++)
            {
                for (int i = 0; i < 4; i++) Act(match, "ready");
                for (int ticks = 0; ticks < 2000 && (match.Phase == Phase.Combat || match.Combat.HasDeaths); ticks++) match.Step();
                Assert.False(match.Combat.HasDeaths);
                Assert.All(match.Players[1].Soldiers, u => Assert.Equal(u.Assignment!.Position, u.Hex!.Position));
            }
            Assert.Equal(Phase.Victory, match.Phase); Assert.Equal(3, match.LastRewardedWave);
        }
        using (Match match = Start())
        {
            Act(match, "recruit", 0); int id = match.Players[1].Soldiers.Single().Id; Act(match, "store", 1, id);
            match.Players[1].Health = 1; // Isolated elimination boundary, not paid balance evidence.
            for (int i = 0; i < 4; i++) Act(match, "ready");
            for (int ticks = 0; ticks < 2000 && match.Phase == Phase.Combat; ticks++) match.Step();
            Assert.Equal(Phase.Defeat, match.Phase); Assert.True(match.Players[1].Eliminated);
            Assert.Empty(match.Players[1].Soldiers); Assert.DoesNotContain(match.Snapshot().DyingBodies, u => u.Id == id);
            Assert.Equal(6, match.Players[1].Snapshot().Army!.Halls.Single().Capacity);
            Assert.DoesNotContain(match.Snapshot().Players.SelectMany(p => p.Soldiers), u => u.Id == id);
        }
    }

    [Fact]
    public void SurvivorsReturnToExactHomesOnlyAfterDeathReservationsExpire()
    {
        using Match match = Start();
        for (int i = 0; i < 6; i++) Act(match, "recruit", 0);
        Dictionary<int, ArmyAssignment> homes = match.Players[1].Soldiers.ToDictionary(u => u.Id, u => u.Assignment!);
        for (int i = 0; i < 4; i++) Act(match, "ready");
        bool cleanupObserved = false;
        for (int i = 0; i < 2000 && (match.Phase == Phase.Combat || match.Combat.HasDeaths); i++)
        {
            if (match.Phase != Phase.Combat && match.Combat.HasDeaths)
            {
                cleanupObserved = true;
                Assert.NotEmpty(match.Combat.Dying());
                Assert.All(match.Players[1].Soldiers, u => Assert.Equal(homes[u.Id], u.Assignment));
            }
            match.Step();
        }
        Assert.True(cleanupObserved); Assert.Equal(Phase.Building, match.Phase);
        Assert.All(match.Players[1].Soldiers, u => { Assert.Equal(homes[u.Id], u.Assignment); Assert.Equal(homes[u.Id].Position, u.Hex!.Position); Assert.Null(u.Hex.FrozenTick); });
        Assert.All(match.Combat.Events().Where(e => e.Type == CombatEventType.Death), e => Assert.DoesNotContain(match.Players[1].Soldiers, u => u.Id == e.Unit!.Id));
    }
}
