using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class RunRewardTests
{
    private static string Json(Match m) => JsonSerializer.Serialize(m.Snapshot(), WireJson.Options);
    private static Command Cmd(Match m, int player, string action) => Command.FromSnapshot(m.Snapshot(), 1, action, player);
    private static void Act(Match m, int player, string action) => Assert.True(m.Apply(player, Cmd(m, player, action)).Accepted);
    private static Match Start(ulong seed = 1, int players = 1)
    {
        var m = new Match(combatSeed: seed); for (int i = 0; i < players; i++) m.Join(); Act(m, 1, "start"); return m;
    }
    private static void Clear(Match m)
    {
        for (int preparation = 0; preparation < 3; preparation++)
            foreach (City city in m.Players.Values.Where(c => c.Connected && !c.Eliminated)) Act(m, city.Id, "ready");
        Assert.Equal(Phase.Combat, m.Phase); CombatFixture.ClearEnemies(m); m.Step();
        // Controlled-result flow boundary, not earned combat/balance evidence.
        Assert.Equal(Phase.RewardDraft, m.Phase);
    }
    private static void OfferedEffect(Match m, string id)
    {
        // An explicit effect fixture authors an offer AFTER a real shared clear.
        // The normal authenticated phase/offer/eligibility/selection command applies it.
        City city = m.Players[1]; city.Run = city.Run with { Offers = [id, "E01", "E02"], Choice = null };
        Act(m, 1, "reward:" + id);
    }
    [Theory]
    [InlineData("F01", 1, 0, 4, 0, 0, 0)]
    [InlineData("F01", 7, 0, 8, 0, 0, 0)]
    [InlineData("F01", 14, 0, 12, 0, 0, 0)]
    [InlineData("F02", 1, 0, 0, 8, 0, 0)]
    [InlineData("F02", 7, 0, 0, 16, 0, 0)]
    [InlineData("F02", 14, 0, 0, 24, 0, 0)]
    [InlineData("F03", 1, 0, 0, 0, 3, 2)]
    [InlineData("F03", 7, 0, 0, 0, 6, 4)]
    [InlineData("F03", 14, 0, 0, 0, 9, 6)]
    [InlineData("F06", 1, 5, 0, 0, 0, 0)]
    [InlineData("F06", 7, 10, 0, 0, 0, 0)]
    [InlineData("F06", 14, 15, 0, 0, 0, 0)]
    public void ImmediateShipmentsUseClearedWaveNotNextWaveAndNeverPayOnRetry(string id, int cleared, int gold, int wood, int food, int metal, int cloth)
    {
        using Match m = Start();
        for (int wave = 1; wave <= cleared; wave++) { Clear(m); if (wave < cleared) RewardFixture.Resolve(m); }
        City city = m.Players[1]; city.Run = city.Run with { Cards = [] };
        ResourceCost before = city.Resources; int production = m.ProductionCount;
        OfferedEffect(m, id); Assert.True(before.TryAdd(new(gold, wood, food, Metal: metal, Cloth: cloth), out ResourceCost expected));
        Assert.Equal(expected, city.Resources); Assert.Equal(production, m.ProductionCount); Assert.Equal(1, city.Run.Stacks(id));
        string chosen = Json(m); Assert.False(m.Apply(1, Cmd(m, 1, "reward:" + id)).Accepted); Assert.Equal(chosen, Json(m));
    }
    [Fact]
    public void CodexExcludesAcquisitionAndPaysEverySecondSubsequentClearWhileHarvestCapsAndIsReceipted()
    {
        using Match m = Start();
        for (int wave = 1; wave <= 7; wave++) { Clear(m); if (wave < 7) RewardFixture.Resolve(m); }
        int acquiredPoints = m.Players[1].Research.Points;
        OfferedEffect(m, "R05"); Assert.Equal(acquiredPoints, m.Players[1].Research.Points);
        for (int subsequent = 1; subsequent <= 4; subsequent++)
        {
            City city = m.Players[1]; int before = city.Research.Points;
            Clear(m); Assert.Equal(before + (subsequent % 2 == 0 ? 2 : 1), city.Research.Points);
            Assert.Equal(subsequent, city.Run.CodexClears); RewardFixture.Resolve(m);
        }
        City harvester = m.Players[1]; harvester.Run = harvester.Run with { Cards = [new("R03", 1)] };
        for (int preparation = 0; preparation < 3; preparation++) Act(m, 1, "ready");
        int metal = harvester.Metal;
        harvester.Run = harvester.Run with { EnemyKills = 18 }; CombatFixture.ClearEnemies(m); m.Step();
        Assert.Equal(metal + 5, harvester.Metal); Assert.Equal(5, harvester.LastReward!.Amount.Metal);
        int points = harvester.Research.Points; m.Step(); Assert.Equal(metal + 5, harvester.Metal); Assert.Equal(points, harvester.Research.Points);
    }
    [Fact]
    public void InvalidSpoofedStalePausedAndSecondSelectionsAreAtomicAndZeroConnectedFallbackConverges()
    {
        using Match m = Start(players: 2); Clear(m);
        Command command = Cmd(m, 1, "reward:" + m.Players[1].Run.Offers[0]);
        foreach (Command bad in new[] { command with { City = 2 }, command with { MatchId = "wrong" }, command with { ExpectedPhase = Phase.Building },
            command with { TurnSerial = command.TurnSerial - 1 }, command with { Action = "reward:not-offered" }, command with { Action = "doctrine:merchant" }, command with { Action = null! } })
        { string before = Json(m); Assert.False(m.Apply(1, bad).Accepted); Assert.Equal(before, Json(m)); }
        Act(m, 1, "pause"); string paused = Json(m); Assert.False(m.Apply(1, command).Accepted); Assert.Equal(paused, Json(m)); Act(m, 1, "resume");
        Assert.True(m.Apply(1, command).Accepted); string chosen = Json(m); Assert.False(m.Apply(1, command).Accepted); Assert.Equal(chosen, Json(m));
        m.SetConnected(1, false); m.SetConnected(2, false); Assert.Equal(Phase.Building, m.Phase);
        Assert.All(m.Players.Values, c => { Assert.NotNull(c.Run.Choice); Assert.Equal(1, c.Run.Stacks(c.Run.Choice!)); });
        string detached = Json(m); m.Step(); Assert.Equal(detached, Json(m)); Assert.Equal(3, m.ProductionCount);
    }
    [Fact]
    public void SeededEncountersAreIndependentOfChoiceArrivalOrderCardEffectsAndRewardStream()
    {
        using Match a = Start(123, 2), b = Start(123, 2); Clear(a); Clear(b);
        foreach (int player in new[] { 1, 2 })
        { Assert.Equal(a.Players[player].Run.Offers, b.Players[player].Run.Offers); Assert.Equal(a.Players[player].Run.OfferSeed, b.Players[player].Run.OfferSeed); }
        Act(a, 1, "reward:" + a.Players[1].Run.Offers[0]); Act(a, 2, "reward:" + a.Players[2].Run.Offers[0]);
        Act(b, 2, "reward:" + b.Players[2].Run.Offers[1]); Act(b, 1, "reward:" + b.Players[1].Run.Offers[1]);
        UnitState[] planned = a.Snapshot().NextWave;
        string forecast = JsonSerializer.Serialize(planned, WireJson.Options);
        Assert.Equal(forecast, JsonSerializer.Serialize(b.Snapshot().NextWave, WireJson.Options));
        Assert.NotEqual(RunEffects.Seed(123, RunEffects.RewardStream, 2, 1), RunEffects.Seed(123, RunEffects.EncounterStream, 2, 1));
        for (int preparation = 0; preparation < 3; preparation++) foreach (Match m in new[] { a, b }) foreach (int player in new[] { 1, 2 }) Act(m, player, "ready");
        Assert.Equal(a.Enemies.Select(u => (u.Type, u.Level, u.Rank, u.IsBoss, u.Origin, u.Destination, u.Profile)), b.Enemies.Select(u => (u.Type, u.Level, u.Rank, u.IsBoss, u.Origin, u.Destination, u.Profile)));
        Assert.Equal(planned.Length, a.Enemies.Count);
        Assert.Equal(planned.Select(u => (u.Type, u.Level, u.Rank, u.IsBoss, u.Origin, u.Destination, u.Profile)), a.Enemies.Select(u => (u.Type, u.Level, u.Rank, u.IsBoss, u.Origin, u.Destination, u.Profile)));
    }
    [Fact]
    public void MerchantBonusBelongsToFirstSuccessfulTransactionNotEachBundleOrMarketAndResetsPerWave()
    {
        using var m = new Match(combatSeed: 1); City city = m.Join()!;
        Act(m, 1, "doctrine:merchant"); Act(m, 1, "start");
        city.Wood = 10; city.Stone = 4; city.Metal = 15; // isolated Market effect, not earned-balance evidence
        foreach (int slot in new[] { 0, 1 }) Assert.True(m.Apply(1, Cmd(m, 1, "build") with { Slot = slot, Building = Building.Market }).Accepted);
        Command Trade(int slot, Resource resource, int bundles) => Cmd(m, 1, "trade") with
        { Slot = slot, ExpectedGeneration = city.Slots[slot].Generation, Resource = resource, Bundles = bundles };
        Assert.False(m.Apply(1, Trade(0, Resource.Metal, 0)).Accepted); Assert.Equal(0, city.Run.MerchantWave);
        Assert.Equal(1, m.Snapshot().Players[0].MarketSaleBonus);
        int gold = city.Gold; Assert.True(m.Apply(1, Trade(0, Resource.Metal, 2)).Accepted);
        Assert.Equal(0, m.Snapshot().Players[0].MarketSaleBonus);
        Assert.Equal(gold + 5, city.Gold); Assert.Equal(1, city.Run.MerchantWave);
        Assert.True(m.Apply(1, Trade(1, Resource.Metal, 1)).Accepted); Assert.Equal(gold + 7, city.Gold);
        Clear(m); RewardFixture.Resolve(m); gold = city.Gold;
        Assert.Equal(1, m.Snapshot().Players[0].MarketSaleBonus);
        Assert.True(m.Apply(1, Trade(1, Resource.Wood, 1)).Accepted); Assert.Equal(gold + 2, city.Gold); Assert.Equal(2, city.Run.MerchantWave);
    }
    [Fact]
    public void ForgottenKnowledgeIsUnmultipliedPersonalAndSurgeonsCapKeepHealing()
    {
        using Match m = Start(); Clear(m); int points = m.Players[1].Research.Points;
        OfferedEffect(m, "F05"); Assert.Equal(points + 2, m.Players[1].Research.Points);
        Clear(m); City city = m.Players[1]; city.Health = 9501; int food = city.Food;
        OfferedEffect(m, "F04"); Assert.Equal(10000, city.Health); Assert.Equal(food, city.Food);
        Assert.False(RunEffects.Eligible(city, RunCatalog.Card("F04"), 2, 10000, m.Campaign, m.Players.Count));
    }
    [Fact]
    public void EncounterOrderVariesAcrossSeedsAtExactlyTheAuthoredBudget()
    {
        var orders = new HashSet<string>();
        for (ulong seed = 0; seed < 8; seed++)
        {
            using Match varied = Start(seed);
            for (int wave = 1; wave <= 20; wave++)
            {
                UnitState[] forecast = varied.Snapshot().NextWave;
                if (wave == 2) orders.Add(string.Join(',', forecast.Select(u => u.Type)));
                Assert.All(forecast, u => Assert.Equal(varied.Configuration.Unit(u.Type, u.Rank, u.IsBoss, u.Level, faction: Faction.Skeletons).Runtime(), u.Profile));
                var authored = varied.Campaign.Wave(wave).Entries.SelectMany(e => Enumerable.Repeat((e.Type, e.Level, e.Rank, e.IsBoss), e.Count));
                Assert.Equal(authored.OrderBy(e => e.Type), forecast.Select(u => (u.Type, u.Level, u.Rank, u.IsBoss)).OrderBy(e => e.Type));
                if (wave < 20) { Clear(varied); RewardFixture.Resolve(varied); }
            }
        }
        Assert.Equal(2, orders.Count);
    }
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void LateRelicEligibilityIsSharedByOffersSelectionAndDisconnectedFallback(int players)
    {
        using Match m = Start(players: players);
        for (int wave = 1; wave <= 18; wave++) { Clear(m); if (wave < 18) RewardFixture.Resolve(m); }
        City city = m.Players[1]; city.Run = city.Run with { Cards = [] };
        Assert.True(RunEffects.Eligible(city, RunCatalog.Card("R05"), 18, 10000, m.Campaign, players));
        Assert.True(RunEffects.Eligible(city, RunCatalog.Card("R03"), 18, 10000, m.Campaign, players));
        RewardFixture.Resolve(m); Clear(m);
        foreach (City survivor in m.Players.Values) survivor.Run = survivor.Run with { Cards = [] };
        Assert.False(RunEffects.Eligible(city, RunCatalog.Card("R05"), 19, 10000, m.Campaign, players));
        Assert.Equal(players >= 3, RunEffects.Eligible(city, RunCatalog.Card("R03"), 19, 10000, m.Campaign, players));
        for (ulong seed = 0; seed < 64; seed++)
        {
            string[] offers = RunEffects.Offers(city, 19, seed, 10000, m.Campaign, players);
            Assert.DoesNotContain("R05", offers);
            if (players < 3) Assert.DoesNotContain("R03", offers);
        }
        foreach (City survivor in m.Players.Values) survivor.Run = survivor.Run with { Offers = ["R05", "R03", "F05"], Choice = null };
        string before = Json(m); Assert.False(m.Apply(1, Cmd(m, 1, "reward:R05")).Accepted); Assert.Equal(before, Json(m));
        if (players < 3)
        { Assert.False(m.Apply(1, Cmd(m, 1, "reward:R03")).Accepted); Assert.Equal(before, Json(m)); }
        else
        {
            foreach (City fallen in m.Players.Values.Skip(1)) fallen.Health = 0;
            Act(m, 1, "reward:R03"); Assert.Equal("R03", city.Run.Choice);
        }
        if (players < 3) foreach (City survivor in m.Players.Values) m.SetConnected(survivor.Id, false);
        Assert.Equal(Phase.Building, m.Phase);
        Assert.Equal(players < 3 ? "F05" : "R03", city.Run.Choice);
        Assert.Equal(19 * 3, m.ProductionCount);
    }
    [Fact]
    public void HarvestNeedsOneSufficientWaveAndCodexUsesConfiguredRemainingClears()
    {
        WaveDefinition[] waves = CampaignDefinition.Default().Waves.Take(9).Select(w => w.Number >= 8
            ? w with { Entries = [new(UnitType.Swordsman, 1)], IsBoss = false } : w).ToArray();
        using var m = new Match(new() { Campaign = new(waves) }); City city = m.Join()!; m.Join();
        Assert.True(RunEffects.Eligible(city, RunCatalog.Card("R05"), 7, 10000, m.Campaign, 2));
        Assert.False(RunEffects.Eligible(city, RunCatalog.Card("R05"), 8, 10000, m.Campaign, 2));
        Assert.False(RunEffects.Eligible(city, RunCatalog.Card("R03"), 7, 10000, m.Campaign, 2));
        Assert.True(RunEffects.Eligible(city, RunCatalog.Card("R03"), 7, 10000, m.Campaign, 3));
        Assert.False(RunEffects.Eligible(city, RunCatalog.Card("R03"), 9, 10000, m.Campaign, 3));
    }
    [Fact]
    public void CodecRefusesMalformedRunStateInsteadOfInventingSelectionsOrPassives()
    {
        using Match m = Start(); MatchSnapshot state = m.Snapshot(); RunState run = state.Players[0].Run;
        foreach (RunState invalid in new[] { run with { Started = false }, run with { Doctrine = "unknown" }, run with { EnemyKills = -1 },
            run with { Cards = [new("A01", 3)] }, run with { Cards = [new("A01", 1), new("A01", 1)] },
            run with { Cards = [new("R01", 1), new("R02", 1), new("R03", 1)] }, run with { Cards = [new("unknown", 1)] },
            run with { FirstAttackSquads = [1, 1] }, run with { WarhornSquads = [1] }, run with { DraftWave = 20 },
            run with { DraftWave = 1, Offers = ["R01", "E01", "E02"] }, run with { DraftWave = 1, Offers = ["F01", "E01", "E02"], Choice = "F01" } })
        {
            MatchSnapshot bad = state with { Players = [state.Players[0] with { Run = invalid }] };
            Assert.Throws<ArgumentException>(() => SnapshotCompatibility.RequireSharedArena(bad));
        }
        Clear(m); MatchSnapshot detached = m.Snapshot(); detached.Players[0].Run.Offers[0] = "unknown";
        Assert.NotEqual("unknown", m.Players[1].Run.Offers[0]); SnapshotCompatibility.RequireSharedArena(m.Snapshot());
    }
    [Fact]
    public void DoctrinePreviewHandlesConfiguredStartingBoundsAndRejectedStartAllocatesNobody()
    {
        using var m = new Match(new() { StartingGold = 1 }, combatSeed: 1); m.Join(); m.Join();
        MatchSnapshot lobby = m.Snapshot(); RunView view = RunPresentation.Project(lobby, lobby.Players[0], 1, true, true);
        Assert.Equal(6, view.Cards.Length); Assert.False(view.Cards.Single(c => c.Id == "warden").Enabled);
        Assert.Contains("Unavailable", view.Cards.Single(c => c.Id == "arcanist").Footer, StringComparison.Ordinal);
        Assert.True(view.Cards.Single(c => c.Id == "merchant").Enabled);
        Act(m, 1, "doctrine:merchant"); string before = Json(m);
        Assert.False(m.Apply(1, Cmd(m, 1, "start")).Accepted); Assert.Equal(before, Json(m));
        Act(m, 2, "doctrine:huntsman"); Act(m, 1, "start");
        Assert.Equal(5, m.Players[1].Gold); Assert.Equal(2, m.Players[2].Metal); Assert.All(m.Players.Values, c => Assert.True(c.Run.Started));
    }
    [Fact]
    public void BoundedClearPreflightIncludesFutureCodexAndMaximumPossibleHarvest()
    {
        using Match m = Start(); City city = m.Players[1];
        city.Run = city.Run with { Cards = [new("R03", 1), new("R05", 1)], CodexClears = 1 };
        Act(m, 1, "ready"); Act(m, 1, "ready");
        city.Research = new(int.MaxValue - 1); string before = Json(m);
        Assert.False(m.Apply(1, Cmd(m, 1, "ready")).Accepted); Assert.Equal(before, Json(m));
        city.Research = default; city.Metal = int.MaxValue; before = Json(m);
        Assert.False(m.Apply(1, Cmd(m, 1, "ready")).Accepted); Assert.Equal(before, Json(m));
        city.Metal = int.MaxValue - 1; Act(m, 1, "ready"); Assert.Equal(Phase.Combat, m.Phase);
    }
}
