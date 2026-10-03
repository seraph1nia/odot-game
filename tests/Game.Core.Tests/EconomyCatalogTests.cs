using System.Text.Json;
using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class EconomyCatalogTests
{
    private static CommandResult Act(Match match, int city, string action, int slot = -1, Building building = Building.Empty)
        => match.Apply(city, new(1, match.Id, match.Phase, match.TurnSerial, action, city, slot, building, ExpectedGeneration: slot is >= 0 and < 9 ? match.Players[city].Slots[slot].Generation : 0));

    [Fact]
    public void CatalogGraphContainsDirectProducersAndPerTypeMaximums()
    {
        var economy = new EconomyConfiguration(new());
        Assert.Equal(13, economy.Buildings().Length);
        foreach (BuildingDefinition building in economy.Buildings())
        {
            Assert.Equal(building.Type is Building.Arcanum or Building.ArrowTower or Building.CatapultTower, building.Construction.Gold > 0);
            Assert.Equal(building.Type == Building.Market ? 1 : building.Recruits is not null ? 5 : 2, building.MaximumLevel);
            Assert.False(economy.TryUpgrade(building.Type, building.MaximumLevel, out _));
            if (building.Produces is Resource resource)
            {
                Assert.Equal(resource == Resource.Food ? 5 : resource is Resource.Metal or Resource.Cloth ? 4 : 1, economy.Production([new(building.Type, 1)]).Amount(resource) - economy.BaseProduction.Amount(resource));
                Assert.Equal(resource == Resource.Food ? 8 : resource is Resource.Metal or Resource.Cloth ? 8 : 2, economy.Production([new(building.Type, 2)]).Amount(resource) - economy.BaseProduction.Amount(resource));
                Assert.True(economy.TryUpgrade(building.Type, 1, out ResourceCost upgrade));
                Assert.Equal(2, upgrade.Stone);
            }
        }
        Assert.Equal(new ResourceCost(Wood: 1), economy.Building(Building.Lumbermill).Construction);
        Assert.Equal(new ResourceCost(Wood: 2), economy.Building(Building.Mine).Construction);
        Assert.Equal(new ResourceCost(5, 2, Stone: 3), economy.Building(Building.Arcanum).Construction);
        Assert.Equal(0, economy.Building(Building.Arcanum).Construction.Cloth);
        string json = JsonSerializer.Serialize(economy.Buildings(), WireJson.Options);
        Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<BuildingDefinition[]>(json, WireJson.Options), WireJson.Options));
        Assert.Throws<ArgumentException>(() => new EconomyConfiguration(new Rules { StoneOutput = -1 }));
        Assert.NotEqual(economy.Fingerprint, new EconomyConfiguration(new Rules { ClothOutput = 6 }).Fingerprint);
    }

    [Fact]
    public void SnapshotQuotesUseTheSameFrozenUpgradeRatesAndExpansionPrices()
    {
        using var match = new Match(combatSeed: 1); match.Join();
        ResourceCost[] upgrades = [new(Wood: 2), new(6, 2, Stone: 2), new(9, 2, Stone: 3), new(14, 2, Stone: 4)];
        foreach (Building type in new[] { Building.Barracks, Building.ArcheryRange, Building.Arcanum })
            for (int level = 1; level <= 4; level++)
            {
                Assert.True(match.Economy.TryUpgrade(type, level, out ResourceCost quote));
                Assert.Equal(type == Building.Arcanum && level == 1 ? new(4, 2) : upgrades[level - 1], quote);
            }
        Assert.False(match.Economy.TryUpgrade(Building.Market, 1, out _));
        Assert.False(match.Economy.TryUpgrade(Building.Empty, 1, out _));
        Assert.False(match.Economy.TryUpgrade(Building.Barracks, 0, out _));
        int[] prices = [5, 8, 12, 18];
        Assert.Equal(prices, match.Snapshot().PlotPrices);
        for (int count = 0; count < 4; count++)
        {
            Assert.True(match.Economy.TryPlotPrice(count, out ResourceCost quote)); Assert.Equal(new ResourceCost(prices[count]), quote);
        }
        Assert.False(match.Economy.TryPlotPrice(-1, out _)); Assert.False(match.Economy.TryPlotPrice(4, out _));
        MatchSnapshot projection = match.Snapshot(); projection.PlotPrices[0] = 0;
        Assert.Equal(5, match.Snapshot().PlotPrices[0]);
        string json = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options), WireJson.Options));
    }

    [Fact]
    public void StockAllowsAdvancedConstructionWithoutAStandingProducer()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        Assert.Equal(new ResourceCost(12, 6), city.Resources);
        Assert.True(Act(match, 1, "start").Accepted);
        Assert.False(Act(match, 1, "build", 0, Building.Arcanum).Accepted);
        city.Stone = 3;
        Assert.True(Act(match, 1, "build", 0, Building.Arcanum).Accepted);
        Assert.DoesNotContain(city.Slots, s => s.Type == Building.Stonecutter);
        Assert.Equal(0, city.Stone); Assert.Equal(0, city.Cloth);
    }

    [Fact]
    public void DisconnectedLivingCityProducesExactlyOnceWithAllSixResources()
    {
        using var match = new Match(combatSeed: 1); match.Join(); City city = match.Join()!;
        Assert.True(Act(match, 1, "start").Accepted);
        city.Gold = 200; city.Wood = 100;
        Building[] producers = [Building.Mine, Building.Farm, Building.Lumbermill, Building.Stonecutter, Building.MetalMine, Building.Weaver];
        Command expansion = new(1, match.Id, match.Phase, match.TurnSerial, "buy-plot", 2, 5, ExpectedExpansionCount: 0);
        Assert.True(match.Apply(2, expansion).Accepted);
        for (int slot = 0; slot < producers.Length; slot++) Assert.True(Act(match, 2, "build", slot, producers[slot]).Accepted);
        ResourceCost before = city.Resources;
        match.SetConnected(2, false);
        Assert.True(Act(match, 1, "ready").Accepted);
        Assert.True(before.TryAdd(new(3, 1, 5, 1, 4, 4), out ResourceCost expected));
        Assert.Equal(expected, city.Resources); Assert.Equal(1, match.ProductionCount);
        match.Snapshot(); match.Step(); match.SetConnected(2, true);
        Assert.Equal(expected, city.Resources); Assert.Equal(1, match.ProductionCount);
    }
    [Fact]
    public void ProducerUpgradesSaveLandWhileMarketsExchangeRecurringEquipmentSurplus()
    {
        var economy = new EconomyConfiguration(new());
        Assert.True(economy.TryPlotPrice(0, out ResourceCost plot));
        foreach (Building type in new[] { Building.Mine, Building.Lumbermill, Building.Stonecutter, Building.MetalMine, Building.Weaver })
        {
            BuildingDefinition producer = economy.Building(type);
            Assert.True(economy.TryUpgrade(type, 1, out ResourceCost upgrade));
            Assert.Equal(producer.LevelOneOutput, producer.LevelTwoOutput - producer.LevelOneOutput);
            Assert.True(producer.Construction.TryAdd(plot, out ResourceCost extraPlot));
            Assert.True(upgrade.Gold < extraPlot.Gold);
            Assert.True(upgrade.Stone > extraPlot.Stone); // Upgrading trades stone for saved land and gold.
        }
        BuildingDefinition metal = economy.Building(Building.MetalMine), gold = economy.Building(Building.Mine);
        MarketRate rate = economy.MarketRates().Single(r => r.Resource == Resource.Metal);
        Assert.True(economy.TryMarketQuote(Resource.Metal, metal.LevelTwoOutput / rate.Units, out ResourceCost stock, out ResourceCost proceeds));
        Assert.Equal(5, stock.Metal);
        Assert.Equal(3, metal.LevelTwoOutput - stock.Metal);
        Assert.True((long)metal.LevelOneOutput * rate.Gold > (long)gold.LevelOneOutput * rate.Units);
        Assert.True(economy.Building(Building.Market).Construction.Stone > 0); // Proceeds need a paid additional building and consume recruitment stock.
    }

    [Fact]
    public void IncomeProjectionUsesFrozenCapacityAndNeverPaysDuringInspection()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!; match.Join();
        Assert.True(Act(match, 1, "start").Accepted);
        city.Wood = 20; city.Stone = 10;
        Assert.True(Act(match, 1, "build", 0, Building.Lumbermill).Accepted);
        Assert.True(Act(match, 1, "build", 1, Building.MetalMine).Accepted);
        Assert.True(Act(match, 1, "upgrade", 1).Accepted);
        ResourceCost stocks = city.Resources; long revision = match.Revision;
        Assert.Equal(new ResourceCost(2, 1, Metal: 8), city.Snapshot().ProductionIncome);
        match.SetConnected(1, false);
        Assert.Equal(new ResourceCost(2, 1, Metal: 8), match.Snapshot().Players[0].ProductionIncome);
        match.SetConnected(1, true);
        Assert.Equal(stocks, city.Resources); Assert.Equal(revision + 2, match.Revision);
        Assert.True(Act(match, 1, "sell", 1).Accepted);
        Assert.Equal(new ResourceCost(2, 1), city.Snapshot().ProductionIncome);
        city.Health = 0;
        Assert.Equal(default(ResourceCost), city.Snapshot().ProductionIncome);
    }

    [Fact]
    public void UnrepresentableCustomIncomeIsUnavailableAndProductionRejectsAtomically()
    {
        using var match = new Match(new Rules { StartingGold = 0, BaseGold = int.MaxValue, MineOutput = 1 }, combatSeed: 1);
        City city = match.Join()!;
        Assert.True(Act(match, 1, "start").Accepted);
        Assert.True(Act(match, 1, "build", 0, Building.Mine).Accepted);
        Assert.Null(city.Snapshot().ProductionIncome);
        string before = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(Act(match, 1, "ready").Accepted);
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        Assert.False(city.Ready); Assert.Equal(0, match.ProductionCount);
        Assert.Equal(0, city.Gold);
    }

    [Fact]
    public void DefaultOpeningFundsSixSwordsmenWithoutSpendingGoldOnBasicBuildings()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        Assert.True(Act(match, 1, "start").Accepted);
        Assert.True(Act(match, 1, "build", 0, Building.Farm).Accepted);
        Assert.True(Act(match, 1, "build", 1, Building.MetalMine).Accepted);
        Assert.True(Act(match, 1, "build", 2, Building.Barracks).Accepted);
        Assert.Equal(12, city.Gold); Assert.Equal(0, city.Wood);
        for (int turn = 0; turn < 3; turn++) Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(Phase.Preparation, match.Phase);
        Assert.Equal(new ResourceCost(18, Food: 15, Metal: 12), city.Resources);
        for (int recruit = 0; recruit < 6; recruit++) Assert.True(Act(match, 1, "recruit", 2).Accepted);
        Assert.Equal(6, city.Soldiers.Count); Assert.Equal(0, city.Metal);
        Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(18, city.Gold); Assert.Equal(9, city.Food); Assert.Equal(6, city.LastUpkeep!.Paid);
    }

}
