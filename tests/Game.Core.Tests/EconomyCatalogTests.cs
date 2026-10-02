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
            Assert.True(building.Construction.Gold > 0);
            Assert.Equal(building.Type == Building.Market ? 1 : building.Recruits is not null ? 5 : 2, building.MaximumLevel);
            Assert.False(economy.TryUpgrade(building.Type, building.MaximumLevel, out _));
            if (building.Produces is Resource resource)
            {
                Assert.Equal(resource is Resource.Metal or Resource.Cloth ? 20 : 5, economy.Production([new(building.Type, 1)]).Amount(resource) - economy.BaseProduction.Amount(resource));
                Assert.Equal(resource == Resource.Food ? 8 : resource is Resource.Metal or Resource.Cloth ? 40 : 10, economy.Production([new(building.Type, 2)]).Amount(resource) - economy.BaseProduction.Amount(resource));
                Assert.True(economy.TryUpgrade(building.Type, 1, out ResourceCost upgrade));
                Assert.Equal(10, upgrade.Stone);
            }
        }
        Assert.Equal(new ResourceCost(20), economy.Building(Building.Lumbermill).Construction);
        Assert.Equal(new ResourceCost(20, 10), economy.Building(Building.Mine).Construction);
        Assert.Equal(new ResourceCost(25, 10, Stone: 15), economy.Building(Building.Arcanum).Construction);
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
        ResourceCost[] upgrades = [new(20, 10), new(30, 10, Stone: 10), new(45, 10, Stone: 15), new(70, 10, Stone: 20)];
        foreach (Building type in new[] { Building.Barracks, Building.ArcheryRange, Building.Arcanum })
            for (int level = 1; level <= 4; level++)
            {
                Assert.True(match.Economy.TryUpgrade(type, level, out ResourceCost quote));
                Assert.Equal(upgrades[level - 1], quote);
            }
        Assert.False(match.Economy.TryUpgrade(Building.Market, 1, out _));
        Assert.False(match.Economy.TryUpgrade(Building.Empty, 1, out _));
        Assert.False(match.Economy.TryUpgrade(Building.Barracks, 0, out _));
        int[] prices = [25, 40, 60, 90];
        Assert.Equal(prices, match.Snapshot().PlotPrices);
        for (int count = 0; count < 4; count++)
        {
            Assert.True(match.Economy.TryPlotPrice(count, out ResourceCost quote)); Assert.Equal(new ResourceCost(prices[count]), quote);
        }
        Assert.False(match.Economy.TryPlotPrice(-1, out _)); Assert.False(match.Economy.TryPlotPrice(4, out _));
        MatchSnapshot projection = match.Snapshot(); projection.PlotPrices[0] = 0;
        Assert.Equal(25, match.Snapshot().PlotPrices[0]);
        string json = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options), WireJson.Options));
    }

    [Fact]
    public void StockAllowsAdvancedConstructionWithoutAStandingProducer()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        Assert.Equal(new ResourceCost(60, 30), city.Resources);
        Assert.True(Act(match, 1, "start").Accepted);
        Assert.False(Act(match, 1, "build", 0, Building.Arcanum).Accepted);
        city.Stone = 15;
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
        Assert.True(before.TryAdd(new(15, 5, 5, 5, 20, 20), out ResourceCost expected));
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
        Assert.True(economy.TryMarketQuote(Resource.Metal, metal.LevelOneOutput / rate.Units, out ResourceCost stock, out ResourceCost proceeds));
        Assert.Equal(metal.LevelOneOutput, stock.Metal);
        Assert.True(proceeds.Gold > gold.LevelOneOutput);
        Assert.True(economy.Building(Building.Market).Construction.Stone > 0); // Proceeds need a paid additional building and consume recruitment stock.
    }

}
