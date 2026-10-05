using System.Text.Json;
using Game;
using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class AssetPresentationTests
{
    [Theory]
    [InlineData(UnitType.Swordsman, Faction.Adventurers, "Knight")]
    [InlineData(UnitType.Crossbowman, Faction.Adventurers, "Archer")]
    [InlineData(UnitType.Swordsman, Faction.Skeletons, "Boneguard")]
    [InlineData(UnitType.Mage, Faction.Skeletons, "Horned mage")]
    public void NameAndDescriptionProjectionDoesNotRenameOrMutateSerializedUnits(UnitType type, Faction faction, string name)
    {
        using var match = new Match(combatSeed: 1);
        match.Join();
        int id = match.Combat.Create(type, 1, 1, 1);
        UnitState unit = match.Combat.Read(id) with { Faction = faction, IsBoss = true };
        string before = JsonSerializer.Serialize(unit, WireJson.Options);
        Assert.Equal("Boss · " + name, ProgressionPresentation.UnitName(unit));
        Assert.NotEmpty(ProgressionPresentation.UnitDescription(unit));
        Assert.Contains(HealthPoints.Format(unit.Profile.Damage), ProgressionPresentation.UnitStats(unit));
        Assert.Equal(before, JsonSerializer.Serialize(unit, WireJson.Options));
        UnitState restored = JsonSerializer.Deserialize<UnitState>(before, WireJson.Options)!;
        Assert.Equal(type, restored.Type); Assert.Equal(faction, restored.Faction);
        Assert.Equal(unit.Profile, restored.Profile); Assert.Equal(unit.Assignment, restored.Assignment);
    }
    [Fact]
    public void AuthoredAttackPeakMapsToCommittedImpactWithoutChangingItsTicks()
    {
        var unit = new UnitState(1, 100) { ActionStartTick = 10, ImpactTick = 30, ReadyTick = 50 };
        Assert.Equal(0, AssetCatalog.AttackPose(unit, 0, AssetCatalog.AttackKeyEnd));
        Assert.Equal(0, AssetCatalog.AttackPose(unit, 10, AssetCatalog.AttackKeyEnd));
        Assert.Equal(AssetCatalog.AttackMarker, AssetCatalog.AttackPose(unit, 30, AssetCatalog.AttackKeyEnd));
        Assert.Equal(AssetCatalog.AttackKeyEnd, AssetCatalog.AttackPose(unit, 50, AssetCatalog.AttackKeyEnd));
        Assert.Equal(AssetCatalog.AttackKeyEnd, AssetCatalog.AttackPose(unit, 60, AssetCatalog.AttackKeyEnd));
        Assert.Equal(10, unit.ActionStartTick); Assert.Equal(30, unit.ImpactTick); Assert.Equal(50, unit.ReadyTick);
    }
    [Fact]
    public void ProducerHelpUsesAvailablePresentationNamesAndExactUnchangedAmounts()
    {
        Assert.Equal("Need 2 more food · Bakery", ProgressionPresentation.CostExplanation(new(Food: 2), default));
        Assert.Equal("Need 3 more wood · Woodcutter hut", ProgressionPresentation.CostExplanation(new(Wood: 3), default));
        using var match = new Match(combatSeed: 1);
        match.Join();
        BuildingDefinition farm = match.Economy.Building(Building.Farm);
        Assert.Equal("Bakery", AssetCatalog.BuildingName(farm.Type));
        Assert.Equal($"+{farm.Output(1)} food/turn", ProgressionPresentation.ProducerBenefit(farm));
        Assert.Equal(Building.Farm, farm.Type); Assert.Equal(Resource.Food, farm.Produces);
        CityState city = match.Snapshot().Players[0] with { Food = 0 };
        MarketRate rate = match.Economy.MarketRates().Single(r => r.Resource == Resource.Food);
        Assert.Equal($"Need {rate.Units} more food · Bakery", ProgressionPresentation.FoodSalePreview(city, rate));
        Assert.Equal(0, city.Food);
    }
}
