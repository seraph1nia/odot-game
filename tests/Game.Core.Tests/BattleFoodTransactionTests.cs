using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

// Tier A: full quoted-sale parity and atomic overflow; no child processes.
// Reconstructed from retained round3 recipes, not recovered dirty source.
public sealed class BattleFoodTransactionTests
{
    private static CommandResult Act(Match match, string action, int slot = -1, Building building = Building.Empty)
        => match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1, action, 1, slot, building,
            resource: Resource.Food, bundles: action == "trade" ? 1 : 0));
    private static void Accept(Match match, string action, int slot = -1, Building building = Building.Empty)
    {
        CommandResult result = Act(match, action, slot, building);
        Assert.True(result.Accepted, result.Message);
    }
    private static string State(Match match) => JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompleteSaleMatchesAuthorityWithoutMutatingProjection(bool final)
    {
        using var match = new Match(new Rules { StartingWood = 10, FarmOutput = 28, StoneOutput = 2 }, combatSeed: 1);
        City city = match.Join()!;
        Accept(match, "start");
        Accept(match, "build", 0, Building.Farm);
        Accept(match, "build", 1, Building.Stonecutter);
        Accept(match, "build", 2, Building.MetalMine);
        Accept(match, "build", 3, Building.Barracks);
        Accept(match, "ready");
        Accept(match, "build", 4, Building.Market);
        Accept(match, "recruit", 3); Accept(match, "recruit", 3);
        if (final) Accept(match, "ready");
        MatchSnapshot before = match.Snapshot();
        int[] ids = city.Soldiers.Select(u => u.Id).ToArray();
        Assert.Equal(final ? Phase.Preparation : Phase.Building, before.Phase);
        Assert.Equal(new ResourceCost(final ? 16 : 14, 0, Food: final ? 56 : 28, Stone: final ? 2 : 0, Metal: final ? 4 : 0), city.Resources);
        MarketRate rate = before.MarketRates.Single(r => r.Resource == Resource.Food);
        Assert.Equal(new MarketRate(Resource.Food, 25, 1), rate);
        string unchanged = State(match);
        BattleFoodForecast projection = BattleFood.Project(before.Players[0], final, rate)!;
        Assert.Equal(final ? 59 : 3, projection.Available);
        Assert.Equal(2, projection.Paid); Assert.Equal(2, projection.Demand);
        Assert.Equal(ids, projection.Participating); Assert.Equal(ids, projection.Funded); Assert.Empty(projection.Unfed);
        Assert.Contains(final ? "food after payment 57" : "food after payment 1", ProgressionPresentation.FoodSalePreview(before.Players[0], rate, before.Phase));
        Assert.Equal(unchanged, State(match));
        Accept(match, "trade", 4);
        CityState after = match.Snapshot().Players[0];
        Assert.Equal(final ? 17 : 15, city.Gold); Assert.Equal(final ? 31 : 3, city.Food);
        Assert.Equal(projection.Available, after.FoodForecast!.Available);
        Assert.Equal(projection.Paid, after.FoodForecast.Paid);
        Assert.Equal(projection.Funded, after.FoodForecast.Funded);
        Assert.Equal(projection.Participating, after.FoodForecast.Participating);
        Assert.Equal(projection.Unfed, after.FoodForecast.Unfed);
        if (!final) Accept(match, "ready");
        BattleFoodForecast entry = match.Snapshot().Players[0].FoodForecast!;
        Assert.Equal(59, entry.Available); Assert.Equal(2, entry.Paid);
        Accept(match, "ready");
        Assert.Equal(Phase.Combat, match.Phase); Assert.Equal(3, match.ProductionCount);
        Assert.Equal(new ResourceCost(19, 0, Food: 57, Stone: 4, Metal: 8), city.Resources);
        Assert.Equal(entry.Available - entry.Paid, city.Food);
        Assert.Equal(entry.Funded, city.LastUpkeep!.Funded);
        Assert.Equal(entry.Participating, city.LastUpkeep.Participating);
        Assert.Equal(entry.Unfed, city.LastUpkeep.Unfed);
    }

    [Theory]
    [InlineData(int.MaxValue - 6, true)]
    [InlineData(int.MaxValue - 4, false)]
    public void GoldOverflowDistinguishesSaleRejectionFromLaterProductionRejection(int startingGold, bool saleSucceeds)
    {
        WaveDefinition wave = CampaignDefinition.Default().Waves[0] with { Reward = new ResourceCost() };
        using var match = new Match(new Rules { StartingGold = startingGold, FarmOutput = 14, StoneOutput = 2, Campaign = new([wave]) }, combatSeed: 1);
        City city = match.Join()!;
        Accept(match, "start"); Accept(match, "build", 0, Building.Farm); Accept(match, "build", 1, Building.Stonecutter);
        Accept(match, "ready"); Accept(match, "build", 2, Building.Market); Accept(match, "ready");
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(2, match.ProductionCount);
        Assert.Equal(new ResourceCost(saleSucceeds ? int.MaxValue - 2 : int.MaxValue, 0, Food: 28, Stone: 2), city.Resources);
        MarketRate rate = match.Snapshot().MarketRates.Single(r => r.Resource == Resource.Food);
        string before = State(match);
        Assert.Null(BattleFood.Project(match.Snapshot().Players[0], true, rate));
        Assert.Contains("unavailable", ProgressionPresentation.FoodSalePreview(match.Snapshot().Players[0], rate, Phase.Preparation));
        Assert.Equal(before, State(match));
        CommandResult sale = Act(match, "trade", 2);
        Assert.Equal(saleSucceeds, sale.Accepted);
        if (saleSucceeds)
        {
            Assert.Equal(new ResourceCost(int.MaxValue - 1, 0, Food: 3, Stone: 2), city.Resources);
            Assert.Null(match.Snapshot().Players[0].FoodForecast);
            before = State(match);
            Assert.False(Act(match, "ready").Accepted);
            Assert.Equal(before, State(match));
            Assert.False(city.Ready); Assert.Equal(2, match.ProductionCount);
        }
        else
        {
            Assert.Contains("overflow", sale.Message);
            Assert.Equal(before, State(match));
        }
    }
}
