using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class CampaignTests
{
    private static Match Start(int players = 1, Rules? rules = null)
    {
        var match = new Match(rules, combatSeed: 1); for (int i = 0; i < players; i++) match.Join();
        Assert.True(VillageStrategyTests.Act(match, 1, "start").Accepted); return match;
    }
    private static void Begin(Match match)
    {
        while (match.Phase is Phase.Building or Phase.Preparation)
        {
            foreach (City city in match.Players.Values.Where(c => c.Connected && !c.Eliminated && !c.Ready))
                Assert.True(VillageStrategyTests.Act(match, city.Id, "ready").Accepted);
            if (match.Phase == Phase.Preparation && match.Players.Values.Where(c => c.Connected && !c.Eliminated).All(c => c.Ready)) match.Step();
        }
        Assert.Equal(Phase.Combat, match.Phase);
    }
    private static void Clear(Match match)
    {
        foreach (UnitState enemy in match.Enemies.ToArray()) match.Combat.Remove(enemy.Id);
        match.Step();
    }
    private static string Composition(WaveDefinition wave)
        => string.Join(',', wave.Entries.Select(e => $"{e.Count}{e.Type.ToString()[0]}{e.Level}{(e.IsBoss ? "!" : "")}"));

    [Fact]
    public void DefaultAuthoredCompositionAndRewardsMatchAllTwentyWaves()
    {
        using var match = new Match(combatSeed: 1);
        string[] compositions = ["4S1", "3S1,1B1", "3S1,2C1", "3S1,1B1,1C1,1M1", "3S2,1C1", "3S2,1B2,1C1",
            "3S2,2C2,1M1", "3S2,2B2,2C2,1M2", "3S3,1B2,1C2,1M2", "1S3!", "3S3,2C2", "3S3,1B3,2C3",
            "3S3,2C3,1M3", "3S3,2B3,2C3,1M3", "3S4,1C3,1M3", "3S4,1B4,2C4", "3S4,2C4,1M4",
            "3S4,2B4,2C4,1M4", "3S5,1B4,2C4,1M4", "1S5!"];
        Assert.Equal(20, match.Campaign.TotalWaves);
        for (int number = 1; number <= 20; number++)
        {
            WaveDefinition wave = match.Campaign.Wave(number); Assert.Equal(number, wave.Number);
            Assert.Equal(compositions[number - 1], Composition(wave));
            Assert.Equal(number is 10 or 20, wave.IsBoss);
            Assert.Equal(wave.IsBoss ? new ResourceCost(20, 10, 10) : new(10, 5, 5), wave.Reward);
        }
        Assert.Equal(new ResourceCost(220, 110, 110), match.Campaign.Definition().Waves.Aggregate(default(ResourceCost), (sum, w) => { Assert.True(sum.TryAdd(w.Reward, out ResourceCost next)); return next; }));
    }

    [Fact]
    public void FrozenWaveCatalogAndIdentityRejectMutableProjectionChanges()
    {
        var authored = new Rules(); using var match = new Match(authored, combatSeed: 1);
        CombatFingerprint identity = match.ConfigurationFingerprint;
        authored.Campaign.Waves[0].Entries[0] = new(UnitType.Mage, 5, 99);
        MatchSnapshot projection = match.Snapshot(); projection.WaveCatalog[0].Entries[0] = new(UnitType.Mage, 5, 99);
        projection.Rules.Campaign.Waves[0].Entries[0] = new(UnitType.Mage, 5, 99);
        Assert.Equal("4S1", Composition(match.Campaign.Wave(1))); Assert.Equal(identity, RulesIdentity.Resolve(match.Snapshot().Rules));
        Assert.NotEqual(identity, RulesIdentity.Resolve(authored));
        string json = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.Equal(json, JsonSerializer.Serialize(JsonSerializer.Deserialize<MatchSnapshot>(json, WireJson.Options), WireJson.Options));
    }

    [Fact]
    public void InvalidAuthoredDefinitionsFailBeforeMatchStarts()
    {
        CampaignDefinition[] invalid = [new([]), new([new(2, false, [new(UnitType.Swordsman, 1)], default)]),
            new([new(1, false, [new(UnitType.Swordsman, 0)], default)]), new([new(1, false, [new((UnitType)99, 1)], default)]),
            new([new(1, false, [new(UnitType.Swordsman, 1, 0)], default)]), new([new(1, false, [new(UnitType.Swordsman, 1, Rank: 3)], default)]),
            new([new(1, true, [new(UnitType.Swordsman, 3, IsBoss: true), new(UnitType.Swordsman, 1)], default)]),
            new([new(1, false, [new(UnitType.Swordsman, 3, IsBoss: true)], default)]),
            new([new(1, false, [new(UnitType.Swordsman, 1)], new(Metal: 1))]), new([new(1, false, [new(UnitType.Swordsman, 1)], new(-1))])];
        foreach (CampaignDefinition campaign in invalid) Assert.ThrowsAny<ArgumentException>(() => new Match(new Rules { Campaign = campaign }));
        Assert.Throws<OverflowException>(() => new Match(new Rules { Campaign = new([new(1, false, [new(UnitType.Swordsman, 100)], default)]) }));
        using Match supported = Start(rules: new Rules { Campaign = new([new(1, false, [new(UnitType.Swordsman, 6)], default)]) });
        Begin(supported); Assert.Equal(6, Assert.Single(supported.Enemies).Level); Assert.Equal(17900, supported.Enemies[0].Profile.Health);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void DefaultAllocationIsPerOriginalRosterWithStableAuthoredOrder(int players)
    {
        using Match match = Start(players); Begin(match);
        Assert.Equal(players * 4, match.Enemies.Count);
        foreach (City city in match.Players.Values)
        {
            UnitState[] allocation = match.Enemies.Where(e => e.Origin == city.Id).ToArray();
            Assert.Equal(4, allocation.Length); Assert.All(allocation, e => { Assert.Equal(UnitType.Swordsman, e.Type); Assert.Equal(1, e.Level); Assert.Equal(2, e.Size); Assert.False(e.IsBoss); Assert.Equal(city.Id, e.Destination); });
        }
        Clear(match); Begin(match);
        foreach (City city in match.Players.Values)
            Assert.Collection(match.Enemies.Where(e => e.Origin == city.Id),
                e => Assert.Equal(UnitType.Swordsman, e.Type), e => Assert.Equal(UnitType.Swordsman, e.Type),
                e => Assert.Equal(UnitType.Swordsman, e.Type), e => Assert.Equal(UnitType.Berserker, e.Type));
    }

    [Fact]
    public void FallenRosterStillCreatesBossesAndWoundedTransferRetainsUpkeepAndProfile()
    {
        using Match match = Start(3);
        for (int wave = 1; wave < 10; wave++) { Begin(match); Clear(match); }
        match.Players[3].Health = 0; match.SetConnected(2, false); Begin(match);
        Assert.Equal(3, match.Enemies.Count); Assert.Equal(2, match.Enemies.Count(e => e.Destination == 1));
        Assert.All(match.Enemies, e => { Assert.True(e.IsBoss); Assert.Equal(3, e.Level); Assert.Equal(6, e.Size); Assert.Equal(58400, e.Profile.Health); });
        UnitState inherited = Assert.Single(match.Enemies, e => e.Origin == 3); Assert.Equal(1, inherited.Destination);
        match.Combat.Seed(inherited with { Health = inherited.Health - 1000 }); inherited = match.Combat.Read(inherited.Id);
        string foodBefore = JsonSerializer.Serialize(match.Players[2].LastUpkeep); int food = match.Players[2].Food;
        match.Players[1].Health = 0; match.Step();
        UnitState transferred = Assert.Single(match.Enemies, e => e.Id == inherited.Id);
        Assert.Equal(2, transferred.Destination); Assert.Equal(inherited.Health, transferred.Health); Assert.Equal(inherited.Profile, transferred.Profile);
        Assert.Equal(inherited.Level, transferred.Level); Assert.Equal(inherited.IsBoss, transferred.IsBoss); Assert.True(transferred.Deployed);
        PositionReservation[] claims = match.Snapshot().Reservations.Positions.Where(p => p.UnitId == transferred.Id).ToArray();
        Assert.Equal(transferred.Hex!.HoldsTransit ? 2 : 1, claims.Length);
        Assert.All(claims, p => { Assert.Equal(6, p.Size); Assert.Equal(2, p.City); });
        Assert.Equal(food, match.Players[2].Food); Assert.Equal(foodBefore, JsonSerializer.Serialize(match.Players[2].LastUpkeep));
        Clear(match); Assert.Equal(11, match.Wave); Assert.Equal(10, match.LastRewardedWave);
        Assert.Equal(10, match.Players[2].LastReward!.Wave); Assert.Equal(new ResourceCost(20, 10, 10), match.Players[2].LastReward!.Amount);
        Assert.Equal(9, match.Players[1].LastReward!.Wave); Assert.Equal(9, match.Players[3].LastReward!.Wave);
        match.SetConnected(2, true); Begin(match); Assert.Equal(15, match.Enemies.Count);
        Assert.All(match.Enemies, e => Assert.Equal(2, e.Destination));
        foreach (int origin in Enumerable.Range(1, 3)) Assert.Equal(5, match.Enemies.Count(e => e.Origin == origin));
    }

    [Fact]
    public void LocalClearDoesNotPayAndSharedClearPaysAbsentSurvivorsOnce()
    {
        using Match match = Start(2); Begin(match); ResourceCost before = match.Players[1].Resources;
        foreach (UnitState enemy in match.Enemies.Where(e => e.Destination == 1).ToArray()) match.Combat.Remove(enemy.Id);
        match.Step(); Assert.Equal(0, match.LastRewardedWave); Assert.Null(match.Players[1].LastReward); Assert.Equal(before, match.Players[1].Resources);
        match.SetConnected(2, false); Clear(match);
        Assert.True(before.TryAdd(new(10, 5, 5), out ResourceCost expected)); Assert.Equal(expected, match.Players[1].Resources);
        Assert.Equal(1, match.Players[2].LastReward!.Wave); Assert.Equal(1, match.LastRewardedWave);
        string after = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options); match.Step(); match.Step();
        Assert.Equal(after, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
    }

    [Fact]
    public void TwentySharedTransitionsPayGrossRewardsBeforeVictoryWithoutWaveTwentyOne()
    {
        // Deliberate enemy removal isolates transitions; it is not campaign balance evidence.
        using Match match = Start();
        for (int wave = 1; wave <= 20; wave++)
        {
            Begin(match); Assert.Equal(wave, match.Wave); Assert.Equal(wave, match.Players[1].LastUpkeep!.Wave); Assert.Equal(0, match.Players[1].LastUpkeep!.Paid);
            if (wave is 10 or 20) Assert.True(Assert.Single(match.Enemies).IsBoss);
            Clear(match); Assert.Equal(wave, match.LastRewardedWave); Assert.Equal(wave, match.Players[1].LastReward!.Wave);
            Assert.Equal(wave == 20 ? Phase.Victory : Phase.Building, match.Phase);
        }
        Assert.Equal(20, match.Wave); Assert.Equal(60, match.ProductionCount);
        Assert.Equal(new ResourceCost(880, 140, 110), match.Players[1].Resources);
        Assert.Null(match.Snapshot().Players[0].FoodForecast); Assert.Empty(match.Enemies);
        string final = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options); for (int step = 0; step < 10; step++) match.Step();
        Assert.Equal(final, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
    }

    [Fact]
    public void DefeatAndBattleStallNeverPayRewards()
    {
        using Match fallen = Start(); Begin(fallen); fallen.Players[1].Health = 0; Clear(fallen);
        Assert.Equal(Phase.Defeat, fallen.Phase); Assert.Equal(DefeatReason.AllCitiesFallen, fallen.DefeatReason);
        Assert.Equal(0, fallen.LastRewardedWave); Assert.Null(fallen.Players[1].LastReward);
        using Match stalled = Start(rules: new Rules { SoldierDamage = 0, DefenderDamage = 0, Combat = new() { NoHealthProgressTicks = 400, MaximumWaveTicks = 500 } });
        Begin(stalled); ResourceCost before = stalled.Players[1].Resources;
        while (stalled.Phase == Phase.Combat) stalled.Step();
        Assert.Equal(DefeatReason.BattleStalled, stalled.DefeatReason); Assert.Equal(before, stalled.Players[1].Resources);
        Assert.Equal(0, stalled.LastRewardedWave); Assert.Null(stalled.Players[1].LastReward);
    }

    [Fact]
    public void BattleRewardBoundsRejectReadinessBeforeFoodIsSpent()
    {
        using Match match = Start(); BeginPreparation(); City city = match.Players[1]; city.Gold = int.MaxValue;
        string before = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(VillageStrategyTests.Act(match, 1, "ready").Accepted);
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options)); Assert.Null(city.LastUpkeep);
        city.Gold = 0; Assert.True(VillageStrategyTests.Act(match, 1, "ready").Accepted); Assert.Equal(Phase.Combat, match.Phase);
        void BeginPreparation() { for (int production = 0; production < 3; production++) Assert.True(VillageStrategyTests.Act(match, 1, "ready").Accepted); }
    }
}
