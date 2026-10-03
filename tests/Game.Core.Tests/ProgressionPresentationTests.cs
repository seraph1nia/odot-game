using Game;
using Xunit;

namespace Game.Core.Tests;

public sealed class ProgressionPresentationTests
{
    [Theory]
    [InlineData(1, "I")]
    [InlineData(4, "IV")]
    [InlineData(9, "IX")]
    [InlineData(18, "XVIII")]
    [InlineData(20, "XX")]
    public void LevelsUseConventionalRomanNumerals(int level, string expected)
        => Assert.Equal(expected, ProgressionPresentation.RomanLevel(level));

    [Theory]
    [InlineData(10000, 10000, 100)]
    [InlineData(525, 1050, 50)]
    [InlineData(0, 10000, 0)]
    [InlineData(9950, 10000, 100)]
    public void CityPercentUsesMatchingScales(int current, int maximum, int expected)
        => Assert.Equal(expected, ProgressionPresentation.HealthPercent(current, maximum));

    [Theory]
    [InlineData(Phase.Building, 1, 0)]
    [InlineData(Phase.Building, 2, 1)]
    [InlineData(Phase.Building, 3, 2)]
    [InlineData(Phase.Preparation, 3, 3)]
    [InlineData(Phase.Combat, 3, 4)]
    [InlineData(Phase.Lobby, 0, -1)]
    [InlineData(Phase.Victory, 3, -1)]
    public void PhaseListHighlightsOnlyTheAuthoritativeStage(Phase phase, int turn, int active)
    {
        using var match = new Match(combatSeed: 1);
        string[] rows = ProgressionPresentation.PhaseRows(match.Snapshot() with { Phase = phase, Turn = turn });
        Assert.Equal(5, rows.Length);
        Assert.Equal(active < 0 ? 0 : 1, rows.Count(row => row.StartsWith('>')));
        if (active >= 0) Assert.StartsWith(">", rows[active]);
    }

    [Fact]
    public void InspectionUsesTheVeteransResolvedRankedBossProfile()
    {
        var unit = new UnitState(42, 525) { Type = UnitType.Mage, Faction = Faction.Skeletons, Level = 4, Rank = 2, IsBoss = true, Profile = new(1050, 1975, 3, 20, 18, 24) { Size = 6 } };
        string text = ProgressionPresentation.UnitStats(unit);
        Assert.Contains("Health 5.25/10.5", text);
        Assert.Contains("Level 4", text);
        Assert.Contains("Damage per attack 19.75", text);
        Assert.Contains("Size 6", text);
        Assert.Equal("Boss · Skeleton Mage", ProgressionPresentation.UnitName(unit));
        Assert.NotEmpty(ProgressionPresentation.UnitDescription(unit));
    }

    [Fact]
    public void CombatShowsItsPaidReceiptEvenWhenTheCurrentForecastIsUnfunded()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        CityState snapshot = city.Snapshot() with
        {
            FoodForecast = new(6, 0, 0, [], [1, 2, 3]),
            LastUpkeep = new(3, 6, [1, 2, 3], [])
        };
        ProgressionView view = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Combat, Wave = 3 }, snapshot);
        Assert.Contains("paid 6 food", view.Food); Assert.Contains("Wave 3", view.Food); Assert.DoesNotContain("Next", view.Food);
        ProgressionView preview = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Preparation }, snapshot);
        Assert.Contains("pay 0", preview.Food); Assert.Contains("3 reserve", preview.Food);
    }
    [Fact]
    public void VictoryKeepsTheActualFinalRewardAndUpkeepWithoutInventingAnotherBattle()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        CityState snapshot = city.Snapshot() with
        {
            FoodForecast = new(12, 30, 12, [], []),
            LastUpkeep = new(20, 9, [1], []),
            LastReward = new(20, true, new(20, 10, 10))
        };
        ProgressionView view = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Victory, Wave = 20 }, snapshot);
        Assert.Contains("20 waves held", view.Phase); Assert.Contains("Wave 20: paid 9", view.Food);
        Assert.DoesNotContain("Next", view.Food); Assert.Contains("W20: +20 gold, +10 wood, +10 food", view.Reward);
    }
    [Fact]
    public void CitySwitchesRepeatedSnapshotsAndFreshSessionsDoNotReplayPreviousReceipts()
    {
        using var match = new Match(combatSeed: 1); City first = match.Join()!, second = match.Join()!;
        MatchSnapshot state = match.Snapshot() with { Phase = Phase.Building };
        CityState rewarded = first.Snapshot() with { LastReward = new(2, false, new(10, 5, 5)) };
        ProgressionView a = ProgressionPresentation.Describe(state, rewarded);
        Assert.Equal(a, ProgressionPresentation.Describe(state, rewarded));
        Assert.Equal("No clear reward yet", ProgressionPresentation.Describe(state, second.Snapshot()).Reward);
        using var fresh = new Match(combatSeed: 1); City newCity = fresh.Join()!;
        ProgressionView reset = ProgressionPresentation.Describe(fresh.Snapshot(), newCity.Snapshot());
        Assert.Equal("No clear reward yet", reset.Reward); Assert.Equal("No battle upkeep yet", reset.Food);
        Assert.Contains("Land 5/9", reset.Land); Assert.Contains("5 gold", reset.Land);
    }
    [Fact]
    public void PausedBossAndStalledDefeatKeepTheirAuthoritativeMeaning()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        MatchSnapshot state = match.Snapshot() with { Phase = Phase.Combat, Wave = 10, Paused = true };
        string boss = ProgressionPresentation.Describe(state, city.Snapshot()).Phase;
        Assert.Contains("PAUSED", boss); Assert.Contains("Wave 10/20 · BOSS", boss);
        string stalled = ProgressionPresentation.Describe(state with { Phase = Phase.Defeat, DefeatReason = DefeatReason.BattleStalled }, city.Snapshot()).Phase;
        Assert.Contains("battle stalled", stalled); Assert.DoesNotContain("all cities fell", stalled);
    }
    [Fact]
    public void ArmyDetailsDistinguishPreviewReservesActualReservesAndFedCapacityQueues()
    {
        using var match = new Match(combatSeed: 1); City city = match.Join()!;
        int high = match.Combat.Create(UnitType.Swordsman, 1, 1, 1, level: 5), low = match.Combat.Create(UnitType.Swordsman, 1, 1, 1);
        UnitState veteran = match.Combat.Read(high);
        Assert.Equal("V", ProgressionPresentation.UnitLabel(veteran));
        Assert.Equal("V", ProgressionPresentation.UnitLabel(veteran with { IsBoss = true, Faction = Faction.Skeletons })); match.Combat.Seed(veteran with { Health = 2100 });
        CityState snapshot = city.Snapshot() with { FoodForecast = new(2, 1, 1, [high], [low]) };
        ProgressionView preview = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Preparation }, snapshot);
        Assert.Contains($"#{high} Swordsman L5 · HP 21/133 · damage 33 · size 2 · next: fed", preview.Army);
        Assert.Contains($"#{low} Swordsman L1", preview.Army); Assert.Contains("next: reserve", preview.Army);
        UnitState[] units = snapshot.Soldiers.Select(u => u with { Deployed = false, Hex = u.Hex! with { Lifecycle = u.Id == high ? UnitLifecycle.Queued : UnitLifecycle.Reserve } }).ToArray();
        ProgressionView battle = ProgressionPresentation.Describe(match.Snapshot() with { Phase = Phase.Combat }, snapshot with { Soldiers = units });
        Assert.Contains("fed · capacity queue", battle.Army); Assert.Contains("· reserve", battle.Army); Assert.DoesNotContain("next:", battle.Army);
    }
    [Fact]
    public void CompactEconomyKeepsFutureIncomeForecastAndPaidReceiptsSeparate()
    {
        using var match = new Match(combatSeed: 1); match.Join();
        MatchSnapshot state = match.Snapshot() with { Phase = Phase.Building };
        CityState city = state.Players[0] with { Food = 15, FoodForecast = new(6, 15, 6, [1, 2, 3, 4, 5, 6], []) };
        EconomyView view = ProgressionPresentation.Economy(state, city, true);
        Assert.Equal("Income/turn", view.IncomeLabel); Assert.Equal("Next battle", view.UpkeepLabel);
        Assert.Equal("6 food", view.UpkeepValue); Assert.Equal("Food after payment", view.BalanceLabel); Assert.Equal("9", view.BalanceValue);
        Assert.Equal("+2", ProgressionPresentation.Income(city.ProductionIncome, Resource.Gold));
        Assert.Equal("0", ProgressionPresentation.Income(city.ProductionIncome, Resource.Metal));
        Assert.Equal("Unavailable", ProgressionPresentation.Income(null, Resource.Gold));
        EconomyView shortage = ProgressionPresentation.Economy(state with { Phase = Phase.Preparation }, city with { FoodForecast = new(6, 4, 4, [1, 2, 3, 4], [5, 6]) }, true);
        Assert.Equal("Next building turn", shortage.IncomeLabel); Assert.Contains("no income", shortage.Context); Assert.Equal("2 soldiers will sit out", shortage.BalanceLabel);
        CityState paid = city with { LastUpkeep = new(3, 4, [1, 2, 3, 4], [5, 6]) };
        EconomyView combat = ProgressionPresentation.Economy(state with { Phase = Phase.Combat }, paid, true);
        Assert.Equal("Paid this battle · W3", combat.UpkeepLabel); Assert.Equal("4 food", combat.UpkeepValue); Assert.Equal("2", combat.BalanceValue);
        Assert.Equal("Last battle · W3", ProgressionPresentation.Economy(state with { Phase = Phase.Victory }, paid, true).UpkeepLabel);
        Assert.Contains("Inactive", ProgressionPresentation.Economy(state with { Phase = Phase.Victory }, paid, true).Context);
        Assert.Contains("Paused", ProgressionPresentation.Economy(state with { Paused = true }, city, true).Context);
        Assert.Contains("Stale", ProgressionPresentation.Economy(state, city, false).Context);
        Assert.Equal("0 food", ProgressionPresentation.Economy(state, state.Players[0], true).UpkeepValue);
    }

    [Fact]
    public void ProducerCostsAndFoodSaleConsequencesUseCompletePublishedQuotes()
    {
        using var match = new Match(combatSeed: 1); match.Join();
        BuildingDefinition lumbermill = match.Economy.Building(Building.Lumbermill);
        Assert.Equal("+1 wood/turn", ProgressionPresentation.ProducerBenefit(lumbermill));
        Assert.Equal("1 → 2 wood/turn", ProgressionPresentation.ProducerBenefit(lumbermill, upgrade: true));
        Assert.Contains("Need 2 more stone · Stonecutter", ProgressionPresentation.CostExplanation(lumbermill.Upgrade, new(Wood: 2)));
        CityState city = match.Snapshot().Players[0] with { Food = 28, Soldiers = Enumerable.Range(1, 6).Select(id => new UnitState(id, 4000) { Type = UnitType.Swordsman }).ToArray() };
        MarketRate rate = match.Economy.MarketRates().Single(r => r.Resource == Resource.Food);
        Assert.Contains("pay 3 food · 3 soldiers will sit out", ProgressionPresentation.FoodSalePreview(city, rate));
        Assert.Contains("Need 1 more food", ProgressionPresentation.FoodSalePreview(city with { Food = 24 }, rate));
        Assert.Equal(28, city.Food);
    }

}
