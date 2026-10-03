using System.Text.Json;
using Game.Core;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

internal static class CampaignAcceptance
{
    public static object? Run(ITestOutputHelper output, string strategy, int players, ulong seed, Profiling.Measurements? measurement = null, WorkCounters? work = null, Profiling.CampaignTrace? trace = null)
    {
        var diagnostics = new CampaignDiagnostics(output, Environment.GetEnvironmentVariable("ODOT_CAMPAIGN_TRACE") == "1");
        try { return RunCore(diagnostics, strategy, players, seed, measurement, work, trace); }
        catch { diagnostics.Failure(); throw; }
    }
    private static object? RunCore(CampaignDiagnostics diagnostics, string strategy, int players, ulong seed,
        Profiling.Measurements? measurement, WorkCounters? work, Profiling.CampaignTrace? trace)
    {
        measurement?.Enter("setup");
        using Match match = VillageStrategyTests.Start(players, seed);
        match.SetWorkCounters(work);
        int ticks = 0, recruits = 0, preparations = 0, trades = 0, expansions = 0, sales = 0;
        ResourceCost commandSpending = default, commandIncome = default;
        var recruitedRoles = new HashSet<UnitType>(); var waveTicks = new Dictionary<int, int>();
        int firstLevelFiveBattle = 0, laterMetalRecruits = 0, laterClothRecruits = 0; bool upgradedProducer = false, expandedFullLand = false;
        Assert.All(match.Players.Values, c => { Assert.Equal(0, c.Metal); Assert.Equal(0, c.Cloth); Assert.Equal(0, c.Stone); });
        while (match.Phase is Phase.Building or Phase.Preparation or Phase.Combat)
        {
            measurement?.Enter(match.Phase == Phase.Combat ? "assertion" : "preparation");
            if (match.Phase == Phase.Combat)
            {
                if (trace is not null) { measurement?.Enter("evidence"); trace.Wave(match); measurement?.Enter("assertion"); }
                Assert.True(ticks++ < 100000, "Strategy exceeded its complete campaign bound.");
                waveTicks[match.Wave] = waveTicks.GetValueOrDefault(match.Wave) + 1;
                if (firstLevelFiveBattle == 0 && match.Combat.HasParticipatingSoldierLevel(5)) firstLevelFiveBattle = match.Wave;
                measurement?.Enter("stepping"); match.Step(); continue;
            }
            foreach (City city in match.Players.Values.Where(c => !c.Eliminated && !c.Ready))
            {
                int actions = 0; CityState state;
                while (true)
                {
                    MatchSnapshot before = match.Snapshot();
                    if (CampaignStrategy.Next(before, city.Id, strategy) is not EconomyAction decision) { state = before.Players.Single(p => p.Id == city.Id); break; }
                    Assert.True(actions++ < 100, "Economy policy did not reach a finite ready state.");
                    Command command = decision.Command(before, city.Id);
                    if (trace is not null) { measurement?.Enter("evidence"); trace.Command(command); measurement?.Enter("preparation"); }
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
                    measurement?.Enter("assertion");
                    ResourceCost investment = CampaignDiagnostics.Investment(city.Slots.Where(s => s.Type != Building.Empty).Select(s => s.Investment));
                    ResourceCost oldResources = before.Players.Single(p => p.Id == city.Id).Resources, resources = city.Resources;
                    ResourceCost spent = default, income = default;
                    foreach (Resource resource in Enum.GetValues<Resource>())
                    {
                        int change = resources.Amount(resource) - oldResources.Amount(resource);
                        spent = spent.With(resource, Math.Max(0, -change)); income = income.With(resource, Math.Max(0, change));
                    }
                    Assert.True(commandSpending.TryAdd(spent, out commandSpending));
                    Assert.True(commandIncome.TryAdd(income, out commandIncome));
                    int wave = match.Wave, turn = match.Turn, plots = city.Slots.Count(s => s.Purchased), player = city.Id;
                    measurement?.Enter("diagnostic-assertion");
                    diagnostics.Record(() => $"{strategy} P{player} W{wave} T{turn}: {decision}; before={oldResources}; after={resources}; investment={investment}; plots={plots}");
                    measurement?.Enter("preparation");
                }
                measurement?.Enter("diagnostic-assertion");
                diagnostics.Summary($"{strategy} P{city.Id} W{match.Wave} T{match.Turn} {match.Phase}: stocks={state.Resources}; demand={state.FoodForecast!.Demand}; forecastPaid={state.FoodForecast.Paid}; army={string.Join(',', state.Soldiers.Select(u => $"{u.Id}:{u.Type}L{u.Level}:{u.Health}/{u.Profile.Health}:fed={u.Participating}:field={u.Deployed}"))}; cityHP={city.Health}; reward={JsonSerializer.Serialize(city.LastReward, WireJson.Options)}; upkeep={JsonSerializer.Serialize(city.LastUpkeep, WireJson.Options)}");
                measurement?.Enter("preparation");
            }
            if (match.Phase == Phase.Preparation) preparations++;
            foreach (City city in match.Players.Values.Where(c => c.Connected && !c.Eliminated && !c.Ready))
            {
                if (trace is not null) { measurement?.Enter("evidence"); trace.Command(new(1, match.Id, match.Phase, match.TurnSerial, "ready", city.Id)); measurement?.Enter("preparation"); }
                Assert.True(VillageStrategyTests.Act(match, city.Id, "ready").Accepted);
            }
            if (match.Phase == Phase.Preparation && match.Players.Values.Where(c => c.Connected && !c.Eliminated).All(c => c.Ready)) match.Step();
        }
        measurement?.Enter("diagnostic-assertion");
        diagnostics.Summary($"{strategy}/{players}: production={match.ProductionCount}; economicCommandSpending={commandSpending}; economicCommandIncome={commandIncome}");
        foreach (City city in match.Players.Values)
            diagnostics.Summary($"final P{city.Id}: stocks={city.Resources}; cityHP={city.Health}; army={city.Soldiers.Count}; reward={JsonSerializer.Serialize(city.LastReward, WireJson.Options)}; upkeep={JsonSerializer.Serialize(city.LastUpkeep, WireJson.Options)}");
        diagnostics.Summary($"{strategy}/{players}, seed={seed}, config={match.ConfigurationFingerprint}: waveTicks={string.Join(',', waveTicks.OrderBy(p => p.Key).Select(p => $"{p.Key}:{p.Value}"))}; casualties={recruits - match.Players.Values.Sum(c => c.Soldiers.Count)}; recruits={recruits}; expansions={expansions}; trades={trades}; sales={sales}; firstLevelFiveBattle={firstLevelFiveBattle}; cityHP={string.Join(',', match.Players.Values.Select(c => c.Health))}; final={match.Phase}/{match.DefeatReason}.");
        measurement?.Enter("assertion");
        Assert.Equal(Phase.Victory, match.Phase); Assert.Equal(60, match.ProductionCount); Assert.True(preparations >= 20);
        Assert.True(expansions > 0); Assert.True(trades > 0); Assert.True(sales > 0); Assert.True(upgradedProducer); Assert.True(expandedFullLand); Assert.True(laterMetalRecruits > 0);
        Assert.InRange(firstLevelFiveBattle, 1, 19); Assert.All(match.Players.Values, c => Assert.False(c.Eliminated));
        if (strategy == "mixed") { Assert.Equal(4, recruitedRoles.Count); Assert.True(laterClothRecruits > 0); }
        if (strategy == "research") Assert.True(match.Players[1].Research.Has(TechnologyId.GuardianMastery));
        if (strategy == "towers") Assert.Contains(match.Players[1].Towers.Values, t => t.AttackSequence > 0);
        measurement?.Enter("evidence");
        return trace?.Finish(match, ticks, preparations);
    }
}
