using Xunit;

namespace Game.Core.Tests;

// Tier A paid schedule coverage: synthetic ample-balance recovery tests cannot
// catch missing material windows. Real default rules, seed 1, two early clears,
// no grants/children. Newly reconstructed from historical mutation recipes.
public sealed class PaidPreparationScheduleTests
{
    private static void Act(Match match, string action, int slot = -1, Building building = Building.Empty,
        ConstructionPayment payment = ConstructionPayment.Standard, int unit = 0, UnitType type = UnitType.Swordsman)
    {
        CommandResult result = match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, action, 1, slot, building,
            soldierType: type, payment: payment, unitId: unit));
        Assert.True(result.Accepted, $"W{match.Wave} T{match.Turn} {action} {slot}: {result.Message}");
    }
    private static void Clear(Match match, int wave)
    {
        for (int i = 0; i < 10000 && (match.Phase == Phase.Combat || match.Combat.HasDeaths); i++) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(wave + 1, match.Wave); Assert.Equal(wave, match.LastRewardedWave);
        Assert.False(match.Combat.HasDeaths);
    }
    private static void Fill(Match match)
    {
        while (match.Players[1].Soldiers.Count(u => !u.Assignment!.Stored) < 6)
        {
            if (!match.Snapshot().Players[0].Army!.Homes.Any(h => h.Purchased && h.Used + 2 <= 6)) Act(match, "buy-home");
            Act(match, "recruit", 2);
        }
    }

    [Fact]
    public void EarlyPaidLumberAndFinalStonecutterFundTheSpecialistSupplyChain()
    {
        using var match = new Match(combatSeed: 1); match.Join(); Act(match, "start");
        Act(match, "build", 0, Building.Farm); Act(match, "build", 1, Building.MetalMine); Act(match, "build", 2, Building.Barracks);
        Act(match, "build", 3, Building.Lumbermill, ConstructionPayment.GoldRecovery);
        Act(match, "ready"); Act(match, "ready"); Act(match, "build", 4, Building.Stonecutter);
        for (int i = 0; i < 4; i++) Act(match, "recruit", 2);
        Assert.Equal(new ResourceCost(12, Food: 10), match.Players[1].Resources);
        Act(match, "ready");
        Assert.Equal(new ResourceCost(14, 1, Food: 11, Stone: 1, Metal: 4), match.Players[1].Resources);
        Clear(match, 1); Act(match, "ready"); Act(match, "ready");
        Assert.Equal(new ResourceCost(20, 4, Food: 26, Stone: 3, Metal: 12), match.Players[1].Resources);
        Act(match, "sell", 1); Act(match, "build", 1, Building.Weaver);
        Act(match, "sell", 4); Act(match, "build", 4, Building.Arcanum);
        Assert.Equal(new ResourceCost(15, 2, Food: 26, Metal: 12), match.Players[1].Resources);
        Act(match, "recruit", 2, type: UnitType.Berserker); Fill(match);
        Assert.Contains(match.Players[1].Soldiers, u => u.Type == UnitType.Berserker);
        Act(match, "ready"); Assert.Equal(4, match.Players[1].Cloth);
        Clear(match, 2); Act(match, "ready");
        if (!match.Snapshot().Players[0].Army!.Homes.Any(h => h.Purchased && h.Used + 2 <= 6)) Act(match, "buy-home");
        Act(match, "recruit", 4, type: UnitType.Mage);
        Assert.Contains(match.Players[1].Soldiers, u => u.Type == UnitType.Mage);
    }

    // Paid preparation counterpart of the retained native economy consumer.
    // Independent synthetic research/trade negatives do not prove this legal
    // joint material schedule. No engine journey, grants or balance changes.
    [Fact]
    public void PaidResearchLandMarketAndReceivingTrainerFundTheFinalMixedVeteranArmy()
    {
        using var match = new Match(combatSeed: EconomyArmyFixtureTests.Seed); match.Join(); match.Join();
        void Send(int p, string action, int slot = -1, Building building = Building.Empty,
            ConstructionPayment payment = ConstructionPayment.Standard, Resource resource = Resource.Metal)
        {
            CommandResult result = match.Apply(p, Command.FromSnapshot(match.Snapshot(), 1, action, p, slot, building,
                payment: payment, technology: action == "research-tech" ? TechnologyId.MeleeFoundation : TechnologyId.None,
                resource: resource, bundles: action == "trade" ? 1 : 0));
            Assert.True(result.Accepted, $"P{p} W{match.Wave} T{match.Turn} {action} {slot}: {result.Message}");
        }
        void Ready() { Send(1, "ready"); Send(2, "ready"); }
        void FillCity(int p, int slot, int target)
        {
            while (match.Players[p].Soldiers.Count < target)
            {
                if (!match.Snapshot().Players.Single(c => c.Id == p).Army!.Homes.Any(h => h.Purchased && h.Used + 2 <= 6)) Send(p, "buy-home");
                Send(p, "recruit", slot);
            }
        }
        Send(1, "start");
        foreach (int p in new[] { 1, 2 })
        {
            Send(p, "build", 0, Building.Farm); Send(p, "build", 1, Building.MetalMine); Send(p, "build", 2, Building.Barracks);
            Send(p, "build", 3, Building.Lumbermill, ConstructionPayment.GoldRecovery);
        }
        Ready(); Send(2, "recruit", 2); Send(2, "recruit", 2); Ready();
        foreach (int p in new[] { 1, 2 }) Send(p, "build", 4, Building.Stonecutter);
        FillCity(2, 2, 4); FillCity(1, 2, 4); Ready(); Clear(match, 1);
        Ready(); Ready(); FillCity(2, 2, 6); Send(2, "sell", 2); Send(2, "build", 2, Building.CatapultTower);
        Send(1, "buy-plot", 5); Send(1, "build", 5, Building.ResearchTower); FillCity(1, 2, 6);
        Ready(); Clear(match, 2);
        Send(2, "buy-plot", 5); Send(2, "build", 5, Building.Barracks);
        Send(1, "upgrade", 1); Ready(); Send(2, "upgrade", 5); Ready(); Send(1, "upgrade", 5);
        City owner = match.Players[1]; UnitState veteran = owner.Soldiers.First(u => u.Health < u.Profile.Health);
        Send(1, "research-tech"); Assert.True(owner.Research.Has(TechnologyId.MeleeFoundation));
        Assert.Equal(veteran.Health, owner.Soldiers.Single(u => u.Id == veteran.Id).Health);
        Send(1, "sell", 5); Send(1, "sell", 4); Send(1, "build", 5, Building.Market);
        Assert.True(owner.Research.Has(TechnologyId.MeleeFoundation));
        Assert.Equal(veteran.Health, owner.Soldiers.Single(u => u.Id == veteran.Id).Health);
        Send(1, "trade", 5); Send(1, "upgrade", 2); Send(1, "recruit", 2);
        CityState before = match.Snapshot().Players[0]; MarketRate rate = match.Snapshot().MarketRates.Single(r => r.Resource == Resource.Food);
        BattleFoodForecast projection = BattleFood.Project(before, true, rate)!;
        Send(1, "trade", 5, resource: Resource.Food);
        Assert.Equal(before.Gold + rate.Gold, owner.Gold); Assert.Equal(before.Food - rate.Units, owner.Food);
        Assert.Equal(projection.Available, match.Snapshot().Players[0].FoodForecast!.Available);
        Send(1, "sell", 3); Send(1, "build", 3, Building.Lumbermill);
        Send(2, "sell", 4); Send(2, "upgrade", 2);
        FillCity(1, 2, 9); FillCity(2, 5, 9);
        Assert.All(match.Players.Values, c => Assert.Single(c.Slots, s => s.Type == Building.MetalMine));
        Ready();
        Assert.Equal(Phase.Combat, match.Phase); Assert.Equal(9, match.ProductionCount);
        foreach (CityState city in match.Snapshot().Players)
        {
            Assert.Equal(9, city.Soldiers.Length); Assert.Equal(3, city.Army!.PurchasedHomes);
            Assert.Equal(9, city.LastUpkeep!.Paid); Assert.Empty(city.LastUpkeep.Unfed);
            Assert.Equal(city.Soldiers.Select(u => u.Id).Order(), city.LastUpkeep.Funded.Order());
            Assert.Contains(city.Soldiers, u => u.Level == 1); Assert.Contains(city.Soldiers, u => u.Level == 2);
        }
        Clear(match, 3); Assert.All(match.Players.Values, c => Assert.False(c.Eliminated));
    }

    [Fact]
    public void PaidHallAndIndependentUpgradeTracksRetainThreeLaterRecoverySettlements()
    {
        using var match = new Match(combatSeed: 1); match.Join(); Act(match, "start");
        Act(match, "build", 0, Building.Farm); Act(match, "build", 1, Building.MetalMine); Act(match, "build", 3, Building.Lumbermill);
        Act(match, "ready"); Act(match, "build", 4, Building.Stonecutter); Act(match, "ready");
        Act(match, "sell", 0); Act(match, "build", 2, Building.Barracks);
        for (int i = 0; i < 4; i++) Act(match, "recruit", 2);
        City city = match.Players[1]; Assert.Equal(new ResourceCost(16, Food: 10, Stone: 1), city.Resources);
        Act(match, "ready"); Assert.Equal(new ResourceCost(18, 1, Food: 6, Stone: 2, Metal: 4), city.Resources);
        Clear(match, 1); Assert.Equal(new ResourceCost(20, 2, Food: 11, Stone: 2, Metal: 4), city.Resources);
        Act(match, "build", 0, Building.Farm); Act(match, "ready"); Act(match, "ready");
        Assert.Equal(new ResourceCost(24, 2, Food: 21, Stone: 4, Metal: 12), city.Resources);
        Act(match, "buy-plot", 5); Act(match, "build", 5, Building.TownHall);
        while (city.Soldiers.Count < 4) Act(match, "recruit", 2);
        int[] veterans = city.Soldiers.Where(u => u.RecoveryEligible).OrderBy(u => u.Health).ThenBy(u => u.Id).Take(3).Select(u => u.Id).ToArray();
        Assert.Equal(3, veterans.Length);
        foreach (int id in veterans) Act(match, "store", 5, unit: id);
        Assert.Contains(city.Soldiers, u => !u.Assignment!.Stored);
        int reserve = veterans[0], wounded = city.Soldiers.Single(u => u.Id == reserve).Health;
        Fill(match); Assert.Equal(2, city.Stone);
        Assert.Equal(wounded, city.Soldiers.Single(u => u.Id == reserve).Health);
        Act(match, "buy-home"); Act(match, "send", 5, unit: reserve);
        int retired = city.Soldiers.Where(u => !u.Assignment!.Stored && u.Id != reserve).OrderBy(u => u.Health).ThenBy(u => u.Id).First().Id;
        Act(match, "retire", unit: retired);
        Assert.Equal(6, city.Soldiers.Count(u => !u.Assignment!.Stored));
        int[] stored = city.Soldiers.Where(u => u.Assignment!.Stored).Select(u => u.Id).ToArray();
        Assert.Equal(2, stored.Length);
        var beforeSettlement = stored.ToDictionary(id => id, id => city.Soldiers.Single(u => u.Id == id).Health);
        Act(match, "ready"); Assert.Equal(8, city.LastUpkeep!.Paid);
        Assert.Equal(stored.Order(), city.LastUpkeep.Funded.Except(city.LastUpkeep.Participating).Order());
        Assert.All(stored, id => Assert.Equal(Math.Min(city.Soldiers.Single(u => u.Id == id).Profile.Health, beforeSettlement[id] + 200), city.Soldiers.Single(u => u.Id == id).Health));
        Clear(match, 2); Assert.Equal(3, city.Stone);
        var health = stored.ToDictionary(id => id, id => city.Soldiers.Single(u => u.Id == id).Health);
        ResourceCost before = city.Resources;
        Act(match, "upgrade-healing", 5);
        Assert.Equal(new ResourceCost(before.Gold - 6, before.Wood - 2, before.Food, before.Stone - 2, before.Metal, before.Cloth), city.Resources);
        Assert.Equal(2, city.Slots[5].HealingLevel); Assert.Equal(1, city.Slots[5].CapacityLevel); Assert.Equal(3, city.PurchasedHomes);
        Assert.All(stored, id => Assert.Equal(health[id], city.Soldiers.Single(u => u.Id == id).Health));
        for (int production = 1; production <= 2; production++)
        {
            Act(match, "ready");
            Assert.All(stored, id => Assert.Equal(Math.Min(city.Soldiers.Single(u => u.Id == id).Profile.Health, health[id] + 400 * production), city.Soldiers.Single(u => u.Id == id).Health));
        }
        Act(match, "upgrade-capacity", 5); Assert.Equal(2, city.Slots[5].CapacityLevel); Assert.Equal(2, city.Slots[5].HealingLevel);
        Act(match, "ready");
        Assert.All(stored, id => Assert.Equal(Math.Min(city.Soldiers.Single(u => u.Id == id).Profile.Health, health[id] + 1200), city.Soldiers.Single(u => u.Id == id).Health));
    }
}
