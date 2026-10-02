using System.Text.Json;
using Game.Core;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

internal static class CampaignAcceptance
{
    public static void Run(ITestOutputHelper output, string strategy, int players, ulong seed)
    {
        using Match match = VillageStrategyTests.Start(players, seed);
        int ticks = 0, recruits = 0, preparations = 0, trades = 0, expansions = 0, sales = 0;
        var recruitedRoles = new HashSet<UnitType>(); var waveTicks = new Dictionary<int, int>();
        int firstLevelFiveBattle = 0, laterMetalRecruits = 0, laterClothRecruits = 0; bool upgradedProducer = false, expandedFullLand = false;
        Assert.All(match.Players.Values, c => { Assert.Equal(0, c.Metal); Assert.Equal(0, c.Cloth); Assert.Equal(0, c.Stone); });
        while (match.Phase is Phase.Building or Phase.Preparation or Phase.Combat)
        {
            if (match.Phase == Phase.Combat)
            {
                Assert.True(ticks++ < 100000, "Strategy exceeded its complete campaign bound.");
                waveTicks[match.Wave] = waveTicks.GetValueOrDefault(match.Wave) + 1;
                if (firstLevelFiveBattle == 0 && match.Players.Values.SelectMany(c => c.Soldiers).Any(u => u.Level == 5 && u.Participating && u.Deployed)) firstLevelFiveBattle = match.Wave;
                match.Step(); continue;
            }
            foreach (City city in match.Players.Values.Where(c => !c.Eliminated && !c.Ready))
            {
                int actions = 0;
                while (CampaignStrategy.Next(match.Snapshot(), city.Id, strategy) is EconomyAction decision)
                {
                    Assert.True(actions++ < 100, "Economy policy did not reach a finite ready state.");
                    MatchSnapshot before = match.Snapshot(); Command command = decision.Command(before, city.Id);
                    CommandResult result = match.Apply(city.Id, command); Assert.True(result.Accepted, result.Message);
                    if (decision.Action == "recruit")
                    {
                        recruits++; recruitedRoles.Add(decision.Unit);
                        RecruitmentQuote quote = before.Players.Single(p => p.Id == city.Id).RecruitmentQuotes.Single(q => q.Type == decision.Unit && q.Level == before.Players.Single(p => p.Id == city.Id).Slots[decision.Slot].Level);
                        Assert.Equal(0, quote.Cost.Food); Assert.Equal(before.Players.Single(p => p.Id == city.Id).Food, city.Food);
                        if (match.Wave > 1 && quote.Cost.Metal > 0) laterMetalRecruits++;
                        if (match.Wave > 1 && quote.Cost.Cloth > 0) laterClothRecruits++;
                    }
                    if (decision.Action == "upgrade" && before.BuildingCatalog.Single(b => b.Type == before.Players.Single(p => p.Id == city.Id).Slots[decision.Slot].Type).Produces is not null) upgradedProducer = true;
                    if (decision.Action == "buy-plot" && before.Players.Single(p => p.Id == city.Id).Slots.Where(s => s.Purchased).All(s => s.Type != Building.Empty)) expandedFullLand = true;
                    if (decision.Action == "trade") trades++;
                    if (decision.Action == "sell") sales++;
                    if (decision.Action == "buy-plot") expansions++;
                    output.WriteLine($"{strategy} P{city.Id} W{match.Wave} T{match.Turn}: {decision}; before={before.Players.Single(p => p.Id == city.Id).Resources}; after={city.Resources}; investment={city.Slots.Where(s => s.Type != Building.Empty).Aggregate(default(ResourceCost), (sum, slot) => { Assert.True(sum.TryAdd(slot.Investment, out ResourceCost next)); return next; })}; plots={city.Slots.Count(s => s.Purchased)}");
                }
                CityState state = city.Snapshot();
                output.WriteLine($"{strategy} P{city.Id} W{match.Wave} T{match.Turn} {match.Phase}: stocks={state.Resources}; demand={state.FoodForecast!.Demand}; forecastPaid={state.FoodForecast.Paid}; army={string.Join(',', state.Soldiers.Select(u => $"{u.Id}:{u.Type}L{u.Level}:{u.Health}/{u.Profile.Health}:fed={u.Participating}:field={u.Deployed}"))}; cityHP={city.Health}; reward={JsonSerializer.Serialize(city.LastReward, WireJson.Options)}; upkeep={JsonSerializer.Serialize(city.LastUpkeep, WireJson.Options)}");
            }
            if (match.Phase == Phase.Preparation) preparations++;
            foreach (City city in match.Players.Values.Where(c => c.Connected && !c.Eliminated && !c.Ready)) Assert.True(VillageStrategyTests.Act(match, city.Id, "ready").Accepted);
            if (match.Phase == Phase.Preparation && match.Players.Values.Where(c => c.Connected && !c.Eliminated).All(c => c.Ready)) match.Step();
        }
        output.WriteLine($"{strategy}/{players}, seed={seed}, config={match.ConfigurationFingerprint}: waveTicks={string.Join(',', waveTicks.OrderBy(p => p.Key).Select(p => $"{p.Key}:{p.Value}"))}; casualties={recruits - match.Players.Values.Sum(c => c.Soldiers.Count)}; recruits={recruits}; expansions={expansions}; trades={trades}; sales={sales}; firstLevelFiveBattle={firstLevelFiveBattle}; cityHP={string.Join(',', match.Players.Values.Select(c => c.Health))}; final={match.Phase}/{match.DefeatReason}.");
        Assert.Equal(Phase.Victory, match.Phase); Assert.Equal(60, match.ProductionCount); Assert.True(preparations >= 20);
        Assert.True(expansions > 0); Assert.True(trades > 0); Assert.True(sales > 0); Assert.True(upgradedProducer); Assert.True(expandedFullLand); Assert.True(laterMetalRecruits > 0);
        Assert.InRange(firstLevelFiveBattle, 1, 19); Assert.All(match.Players.Values, c => Assert.False(c.Eliminated));
        if (strategy == "mixed") { Assert.Equal(4, recruitedRoles.Count); Assert.True(laterClothRecruits > 0); }
        if (strategy == "research") Assert.Equal(2, match.Players[1].Research.Melee);
        if (strategy == "towers") Assert.Contains(match.Players[1].Towers.Values, t => t.AttackSequence > 0);

    }
}
