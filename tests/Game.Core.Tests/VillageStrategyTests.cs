using System.Text.Json;
using Game.Core;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

public sealed class VillageStrategyTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(8UL)]
    // A reproducible live-frontage witness, not a balance sample. The new wave-two
    // all-melee composition and funded L2 opening produce this contact at seed 8.
    public void OrdinaryThreeCityProgressionReinforcesAClearedOccupiedForwardBand(ulong seed)
    {
        using Match match = Start(3, seed);
        foreach (int id in new[] { 1, 2 })
        {
            Assert.True(Act(match, id, "build", 0, Building.Farm).Accepted);
            Assert.True(Act(match, id, "build", 1, Building.Barracks).Accepted);
            Assert.True(Act(match, id, "build", 2, Building.MetalMine).Accepted);
        }
        void Invest(int id)
        {
            City city = match.Players[id];
            while (CampaignStrategy.ReinforcementInvestment(match.Snapshot(), id, id == 1) is EconomyAction decision)
                Assert.True(match.Apply(id, decision.Command(match.Snapshot(), id)).Accepted);
            output.WriteLine($"P{id} W{match.Wave} T{match.Turn} army={city.Soldiers.Count} gold={city.Gold} wood={city.Wood}");
        }
        void Prepare(bool firstWave)
        {
            while (match.Phase is Phase.Building or Phase.Preparation)
            {
                foreach (City city in match.Players.Values.Where(c => !c.Eliminated && !c.Ready))
                {
                    if (city.Id == 1 || firstWave && city.Id == 2) Invest(city.Id);
                    else if (city.Id == 2 && match.Phase == Phase.Preparation) Assert.True(Act(match, 2, "recruit", 1).Accepted);

                    Assert.True(Act(match, city.Id, "ready").Accepted);
                }
                if (match.Players.Values.Where(c => !c.Eliminated).All(c => c.Ready)) match.Step();
            }
        }
        Prepare(true);
        while (match.Phase == Phase.Combat) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(2, match.Wave); Assert.True(match.Players[3].Eliminated);
        Prepare(false); Assert.Equal(12, match.Enemies.Count);
        MatchSnapshot? before = null, transfer = null;
        for (int step = 0; step < 3000 && match.Phase == Phase.Combat && !match.Players[2].Eliminated; step++)
        {
            before = match.Snapshot(); match.Step();
            if (match.Players[2].Eliminated) transfer = match.Snapshot();
        }
        Assert.NotNull(before); Assert.NotNull(transfer);
        output.WriteLine($"transfer {before.Tick}: A soldiers={before.Players[0].Soldiers.Length}, enemies={before.Enemies.Count(u => u.Destination == 1)}");
        Assert.DoesNotContain(before.Enemies, u => u.Destination == 1);
        int[] forward = match.Configuration.Board.Front(Faction.Skeletons).ToArray();
        int[] held = before.Players[0].Soldiers.Where(u => u.Deployed && forward.Contains(u.Hex!.Position.Cell)).Select(u => u.Hex!.Position.Cell).Distinct().Order().ToArray();
        output.WriteLine($"cleared-forward transfer seed={seed} tick={transfer.Tick}; held={string.Join(',', held)}; required={string.Join(',', forward)}");
        Assert.NotEmpty(held);
        AdmissionBound bound = Assert.Single(transfer.Admissions, a => a.City == 1);
        Assert.NotNull(bound.AdmissionTick); Assert.True(bound.AdmissionTick <= bound.FirstAdmissionBound);
        Assert.Contains(transfer.Enemies, u => u.Id == bound.FirstUnitId && u.Deployed && u.Origin != 1);
        while (match.Phase == Phase.Combat) match.Step();
        Assert.Equal(Phase.Building, match.Phase); Assert.Equal(3, match.Wave); Assert.Equal(DefeatReason.None, match.DefeatReason);
    }

    internal static CommandResult Act(Match match, int city, string action, int slot = -1, Building building = Building.Empty, UnitType unit = UnitType.Swordsman, UnitClass @class = UnitClass.Melee)
        => match.Apply(city, new(1, match.Id, match.Phase, match.TurnSerial, action, city, slot, building, unit, @class, ExpectedGeneration: slot is >= 0 and < 9 ? match.Players[city].Slots[slot].Generation : 0));
    private static Match Start(int count = 1, ulong seed = 123)
    {
        var match = new Match(combatSeed: seed); for (int n = 0; n < count; n++) match.Join();
        Assert.True(Act(match, 1, "start").Accepted); return match;
    }
    [Fact]
    public void FixedPointRanksFormattingAndOverflowAreExplicit()
    {
        Assert.Equal(105, HealthPoints.Ranked(100, 1)); Assert.Equal(110, HealthPoints.Ranked(100, 2));
        Assert.Equal("1.05", HealthPoints.Format(105)); Assert.Equal("10", HealthPoints.Format(1000));
        Assert.Throws<OverflowException>(() => HealthPoints.FromWhole(int.MaxValue));
        Assert.Throws<OverflowException>(() => HealthPoints.Ranked(int.MaxValue, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => HealthPoints.Ranked(100, 3));
        Assert.Throws<ArgumentException>(() => new Match(new Rules { StartingWood = -1 }));
    }
    [Fact]
    public void CatalogConstructionIsAtomicAndLumbermillNeedsNoWood()
    {
        using Match match = Start(); City city = match.Players[1]; city.Wood = 0;
        string before = JsonSerializer.Serialize(match.Snapshot());
        Assert.False(Act(match, 1, "build", 0, Building.Farm).Accepted); Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot()));
        Assert.True(Act(match, 1, "build", 0, Building.Lumbermill).Accepted); Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(5, city.Wood); Assert.Equal(50, city.Gold);
        Assert.Equal(13, match.Snapshot().BuildingCatalog.Length); Assert.Equal(4, match.Snapshot().UnitCatalog.Length);
    }
    [Fact]
    public void ThirdProductionCanBeSpentBeforeBattleAndPreparationHasNoIncome()
    {
        using Match match = Start(); Assert.True(Act(match, 1, "build", 0, Building.Farm).Accepted);
        Assert.True(Act(match, 1, "build", 1, Building.Barracks).Accepted);
        Assert.True(Act(match, 1, "build", 2, Building.MetalMine).Accepted);
        for (int n = 0; n < 3; n++) Assert.True(Act(match, 1, "ready").Accepted);
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(3, match.ProductionCount); Assert.Equal(3, match.Turn);
        Command stale = new(1, match.Id, Phase.Building, match.TurnSerial - 1, "ready", 1);
        Assert.False(match.Apply(1, stale).Accepted);
        Assert.True(Act(match, 1, "recruit", 1).Accepted); CityState before = match.Players[1].Snapshot();
        Assert.True(Act(match, 1, "ready").Accepted); Assert.Equal(Phase.Combat, match.Phase);
        CityState after = match.Players[1].Snapshot(); Assert.Equal(before.Gold, after.Gold); Assert.Equal(before.Food - match.Economy.Upkeep(UnitType.Swordsman), after.Food); Assert.Equal(before.Wood, after.Wood);
    }
    [Theory]
    [InlineData(UnitType.Swordsman, Building.Barracks)]
    [InlineData(UnitType.Berserker, Building.Barracks)]
    [InlineData(UnitType.Crossbowman, Building.ArcheryRange)]
    [InlineData(UnitType.Mage, Building.Arcanum)]
    public void RecruitmentUsesTheCorrectBuildingAndRanksWithoutHealing(UnitType type, Building building)
    {
        using Match match = Start(); City city = match.Players[1]; city.Gold = 200; city.Food = 100; city.Wood = 100; city.Stone = 100; city.Metal = 100; city.Cloth = 100;
        Assert.True(Act(match, 1, "build", 0, building).Accepted); Assert.True(Act(match, 1, "build", 1, Building.Blacksmith).Accepted);
        Assert.True(Act(match, 1, "build", 2, Building.Farm).Accepted);
        Assert.False(Act(match, 1, "recruit", 2, unit: type).Accepted);
        Assert.True(Act(match, 1, "recruit", 0, unit: type).Accepted); UnitState old = city.Soldiers.Single();
        match.Combat.Seed(old with { Health = old.Health - 100 });
        UnitClass @class = Catalogs.Class(type);
        Assert.True(Act(match, 1, "research", 1, @class: @class).Accepted);
        UnitState ranked = city.Soldiers.Single(); Assert.Equal(old.Health - 100, ranked.Health); Assert.Equal(old.Id, ranked.Id);
        Assert.Equal(old.Hex, ranked.Hex); Assert.Equal(HealthPoints.Ranked(old.Profile.Damage, 1), ranked.Profile.Damage);
        Assert.False(Act(match, 1, "research", 1, @class: @class).Accepted);
        Assert.True(Act(match, 1, "upgrade", 1).Accepted); Assert.True(Act(match, 1, "research", 1, @class: @class).Accepted);
        Assert.False(Act(match, 1, "research", 1, @class: @class).Accepted);
        Assert.True(Act(match, 1, "recruit", 0, unit: type).Accepted);
        Assert.Equal(HealthPoints.Ranked(old.Profile.Health, 2), city.Soldiers[^1].Health);
        using var combat = new CombatSimulation(new()); int skeleton = combat.Create(type, 0, 1, 1, Faction.Skeletons, 2);
        Assert.Equal(city.Soldiers[^1].Profile, combat.Read(skeleton).Profile);
    }
    public static IEnumerable<object[]> StrategySeeds()
    {
        foreach (ulong seed in new ulong[] { 0, 1, 123 })
        {
            foreach (string strategy in new[] { "frontline", "mixed", "towers", "research" }) yield return [strategy, 1, seed];
            foreach (int players in new[] { 2, 3, 4 }) yield return ["frontline", players, seed];
        }
    }
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void NoInvestmentLosesThroughCityDamageInsteadOfTheStallFallback(ulong seed)
    {
        using Match match = Start(seed: seed);
        while (match.Phase is not Phase.Victory and not Phase.Defeat && match.Tick < match.Configuration.MaximumWaveTicks)
        {
            if (match.Phase is Phase.Building or Phase.Preparation) Assert.True(Act(match, 1, "ready").Accepted);
            else match.Step();
        }
        Assert.Equal(Phase.Defeat, match.Phase); Assert.Equal(DefeatReason.AllCitiesFallen, match.DefeatReason);
        Assert.Null(match.Stall); Assert.Equal(0, match.Players[1].Health); Assert.True(match.Players[1].Eliminated);
        output.WriteLine($"No investment: seed={seed}, config={match.Configuration.Fingerprint}, wave={match.Wave}, tick={match.Tick}, cityHP={match.Players[1].Health}.");
    }
    [Theory]
    [MemberData(nameof(StrategySeeds))]
    public void OrdinaryStrategiesWinWithinBoundedSteps(string strategy, int players, ulong seed)
    {
        using Match match = Start(players, seed);
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
            foreach (City city in match.Players.Values.Where(c => c.Connected && !c.Eliminated && !c.Ready)) Assert.True(Act(match, city.Id, "ready").Accepted);
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
