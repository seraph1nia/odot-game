using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

// Tier A: real Match/authority/ECS/codec, deterministic seeds, no processes.
// Catalog-only checks cannot catch city-local integration, duplicate settlement,
// reconnect loss or combat profile drift. Expected cost: <2s excluding build.
public sealed class RogueliteTests
{
    private static CommandResult Act(Match m, string action, int city = 1) => m.Apply(city, Command.FromSnapshot(m.Snapshot(), 1, action, city));
    private static RunState WithCards(string doctrine, params string[] ids) => new() { Doctrine = doctrine, Started = true, Cards = ids.GroupBy(i => i).Select(g => new AcquiredCard(g.Key, g.Count())).ToArray() };
    [Fact]
    public void CatalogHasSixDoctrinesAndThirtySixStableCardsWithExactCaps()
    {
        Assert.Equal(6, RunCatalog.Doctrines.Count); Assert.Equal(36, RunCatalog.Cards.Count);
        foreach (string prefix in new[] { "E", "A", "T", "M", "F", "R" })
            Assert.Equal(Enumerable.Range(1, 6).Select(i => prefix + i.ToString("00", System.Globalization.CultureInfo.InvariantCulture)), RunCatalog.Cards.Where(c => c.Id.StartsWith(prefix, StringComparison.Ordinal)).Select(c => c.Id));
        Assert.Equal(36, RunCatalog.Cards.Select(c => c.Id).Distinct().Count());
        Assert.All(RunCatalog.Cards.Where(c => c.Rarity == CardRarity.Relic), c => Assert.Equal(1, c.MaxStacks));
        Assert.Equal(new ResourceCost(Metal: 3, Cloth: 2), RunCatalog.Card("F03").Resources);
        Assert.All(RunCatalog.Cards, c => Assert.False(string.IsNullOrWhiteSpace(c.Effect)));
    }
    [Theory]
    [InlineData("warden", 10, 6, 2, 0, 0, 0, 0)]
    [InlineData("merchant", 16, 4, 0, 0, 0, 0, 0)]
    [InlineData("warmaster", 12, 6, 0, 4, 0, 6, 0)]
    [InlineData("arcanist", 9, 6, 3, 0, 3, 0, 1)]
    [InlineData("grovekeeper", 12, 8, 0, 0, 0, 0, 0)]
    [InlineData("huntsman", 12, 6, 0, 2, 0, 0, 0)]
    public void StartIsPersonalOnceAndDoctrineLocks(string doctrine, int gold, int wood, int stone, int metal, int cloth, int food, int research)
    {
        using var m = new Match(combatSeed: 1); m.Join(); m.Join();
        Assert.Equal("warden", m.Players[1].Run.Doctrine);
        Assert.True(Act(m, "doctrine:" + doctrine).Accepted); Assert.True(Act(m, "doctrine:" + doctrine, 2).Accepted);
        Assert.True(Act(m, "start").Accepted);
        foreach (City city in m.Players.Values)
        {
            Assert.Equal(new ResourceCost(gold, wood, food, stone, metal, cloth), city.Resources); Assert.Equal(research, city.Research.Points);
        }
        string before = JsonSerializer.Serialize(m.Snapshot(), WireJson.Options);
        Assert.False(Act(m, "start").Accepted); Assert.False(Act(m, "doctrine:merchant").Accepted);
        Assert.Equal(before, JsonSerializer.Serialize(m.Snapshot(), WireJson.Options));
        SnapshotCompatibility.RequireSharedArena(SessionFlow.RoundTrip(m.Snapshot()));
    }
    [Fact]
    public void ProductionPerBuildingAndGrovePerCityStackWithResearch()
    {
        using var m = new Match(); City city = m.Join()!; city.Run = WithCards("grovekeeper", "E01", "E02", "E03", "E04", "E05", "E06");
        SlotState[] slots = [new(Building.Lumbermill, 1), new(Building.Lumbermill, 2), new(Building.Farm, 1), new(Building.Mine, 1), new(Building.Stonecutter, 1), new(Building.MetalMine, 1), new(Building.Weaver, 1)];
        ResourceCost output = m.Economy.Production(slots, run: city.Run);
        Assert.Equal(new ResourceCost(Gold: 4, Wood: 6, Food: 7, Stone: 2, Metal: 5, Cloth: 5), output);
        Assert.Equal(new ResourceCost(Gold: 2), m.Economy.Production([], run: city.Run));
        ResearchState researched = new(100);
        foreach (TechnologyId id in new[] { TechnologyId.MeleeFoundation, TechnologyId.Guardian, TechnologyId.Stonecraft,
            TechnologyId.RangedFoundation, TechnologyId.Precision, TechnologyId.ManagedForestry, TechnologyId.MagicFoundation, TechnologyId.Frost, TechnologyId.Irrigation })
            researched = researched.Purchase(id, m.Economy.Research);
        Assert.Equal(new ResourceCost(Gold: 4, Wood: 8, Food: 9, Stone: 3, Metal: 5, Cloth: 5), m.Economy.Production(slots, researched, city.Run));
    }
    [Fact]
    public void ProfilePercentsAreAdditiveBoundedAndResearchAware()
    {
        using var m = new Match();
        foreach (UnitType type in Enum.GetValues<UnitType>())
        {
            RunState run = WithCards("arcanist", "A01", "A01", "A02", "A02", "A03", "A03", "A04", "A04", "A05", "M01", "M01", "M02", "M02", "M03", "M04", "M05", "R04");
            UnitCapabilities caps = RunEffects.Capabilities(new(FoundationPercent: 5, DirectDamagePercent: 20, BurnPercent: type == UnitType.Mage ? 30 : 0, PoisonPercent: type == UnitType.Crossbowman ? 15 : 0, ChillPercent: type == UnitType.Mage ? 40 : 0), run, type);
            WeaponProfile original = m.Combat.Profile(type);
            WeaponProfile actual = m.Configuration.Unit(type, capabilities: caps).Runtime();
            Assert.Equal(RunEffects.Scale(RunEffects.Scale(original.Health, 5), type == UnitType.Swordsman ? 25 : type == UnitType.Mage ? 29 : 5), actual.Health);
            Assert.Equal(RunEffects.Scale(RunEffects.Scale(original.Damage, 5), 20 + (type == UnitType.Berserker ? 24 : type == UnitType.Crossbowman || type == UnitType.Mage ? 20 : 0)), actual.Damage);
            if (type == UnitType.Mage) { Assert.Equal(40, caps.BurnPercent); Assert.Equal(50, caps.ChillPercent); Assert.Equal(RunEffects.Scale(RunEffects.Scale(original.SecondaryDamage, 5), 74), actual.SecondaryDamage); }
            if (type == UnitType.Crossbowman) Assert.Equal(20, caps.PoisonPercent);
        }
        UnitCapabilities huntsman = RunEffects.Capabilities(default, WithCards("huntsman"), UnitType.Crossbowman);
        Assert.Equal(3600, m.Configuration.Unit(UnitType.Crossbowman, capabilities: huntsman).Health);
        Assert.Equal(4, m.Configuration.Unit(UnitType.Crossbowman, capabilities: huntsman).Runtime().HexRange);
        Assert.Equal(1, RunEffects.Scale(1, -15)); Assert.Equal(0, RunEffects.Scale(0, 20)); Assert.Equal(2, RunEffects.Portion(6, 20));
        Assert.Throws<OverflowException>(() => RunEffects.Scale(int.MaxValue, 100));
    }
    [Fact]
    public void UpkeepAndEquipmentArePersonalAndProjected()
    {
        using var m = new Match();
        RunState run = WithCards("grovekeeper", "A06");
        RecruitmentQuote[] quotes = m.Economy.RecruitmentQuotes(m.Configuration, run: run);
        Assert.Equal(3, quotes.Single(q => q.Type == UnitType.Swordsman && q.Level == 1).Cost.Metal);
        Assert.Equal(4, quotes.Single(q => q.Type == UnitType.Mage && q.Level == 1).Cost.Cloth);
        Assert.All(quotes, q => Assert.Equal(1, q.Upkeep));
        Assert.Equal(2, m.Economy.RecruitmentQuotes(m.Configuration).Single(q => q.Type == UnitType.Mage && q.Level == 1).Upkeep);
    }
    [Fact]
    public void DoctrineTuningDrivesEquipmentProductionMarketAndDescriptions()
    {
        DoctrineDefinition tuned = RunCatalog.Doctrine("grovekeeper") with
        {
            MeleeMetalSurcharge = 2,
            CrossbowMetalSurcharge = 3,
            MageClothSurcharge = 4,
            LumbermillWood = 5,
            FarmFood = 6,
            MerchantSaleGold = 7
        };
        using var m = new Match();
        foreach (UnitType type in Enum.GetValues<UnitType>())
            for (int level = 1; level <= 5; level++)
            {
                ResourceCost original = m.Economy.Recruitment(type, level);
                ResourceCost actual = tuned.Recruitment(original, type);
                Assert.Equal(original.Metal + (type is UnitType.Swordsman or UnitType.Berserker ? 2 : type == UnitType.Crossbowman ? 3 : 0), actual.Metal);
                Assert.Equal(original.Cloth + (type == UnitType.Mage ? 4 : 0), actual.Cloth);
                Assert.Equal(original.Gold, actual.Gold); Assert.Equal(original.Wood, actual.Wood);
            }
        Assert.Equal(default, tuned.ProductionBonus([]));
        Assert.Equal(new ResourceCost(Wood: 5), tuned.ProductionBonus([new(Building.Lumbermill, 2), new(Building.Lumbermill, 1)]));
        Assert.Equal(new ResourceCost(Food: 6), tuned.ProductionBonus([new(Building.Farm, 1), new(Building.Farm, 2)]));
        Assert.Equal(new ResourceCost(Wood: 5, Food: 6), tuned.ProductionBonus([new(Building.Lumbermill, 1), new(Building.Farm, 1)]));
        Assert.Equal(7, tuned.MarketBonus(0, 1)); Assert.Equal(0, tuned.MarketBonus(1, 1)); Assert.Equal(7, tuned.MarketBonus(1, 2));
        Assert.Contains("+5 Wood", tuned.Benefits, StringComparison.Ordinal);
        Assert.Contains("+6 Food", tuned.Benefits, StringComparison.Ordinal);
        Assert.Contains("+7 Gold", tuned.Benefits, StringComparison.Ordinal);
        Assert.Contains("+2 Metal per Swordsman/Berserker", tuned.Drawback, StringComparison.Ordinal);
        Assert.Contains("+3 Metal per Crossbowman", tuned.Drawback, StringComparison.Ordinal);
        Assert.Contains("+4 Cloth per Mage", tuned.Drawback, StringComparison.Ordinal);
        Assert.Throws<OverflowException>(() => tuned.Recruitment(new(Metal: int.MaxValue), UnitType.Swordsman));
    }
    [Theory]
    [InlineData(1, 75, 25, 0, 1)]
    [InlineData(4, 75, 25, 0, 1)]
    [InlineData(5, 60, 40, 0, 1)]
    [InlineData(6, 60, 40, 0, 1)]
    [InlineData(7, 60, 35, 5, 2)]
    [InlineData(9, 60, 35, 5, 2)]
    [InlineData(10, 45, 40, 15, 2)]
    [InlineData(13, 45, 40, 15, 2)]
    [InlineData(14, 45, 40, 15, 3)]
    [InlineData(19, 45, 40, 15, 3)]
    public void BandBoundariesAreExact(int wave, int common, int uncommon, int relic, int multiplier)
    { Assert.Equal((common, uncommon, relic), RunEffects.Weights(wave)); Assert.Equal(multiplier, RunEffects.Multiplier(wave)); }
    [Fact]
    public void NineteenDraftsHaveThreeDistinctEligibleOffersEvenWithAdversarialCaps()
    {
        for (ulong seed = 0; seed < 8; seed++)
        {
            using var m = new Match(combatSeed: seed); City c = m.Join()!; c.Run = WithCards("warden");
            for (int wave = 1; wave <= 19; wave++)
            {
                string[] offers = RunEffects.Offers(c, wave, RunEffects.Seed(seed, RunEffects.RewardStream, wave, c.Id), 10000, m.Campaign, m.Players.Count);
                Assert.Equal(3, offers.Distinct().Count());
                Assert.All(offers, id => Assert.True(RunEffects.Eligible(c, RunCatalog.Card(id), wave, 10000, m.Campaign, m.Players.Count)));
                if (wave < 7) Assert.DoesNotContain(offers, id => RunCatalog.Card(id).Rarity == CardRarity.Relic);
                string selected = offers.OrderByDescending(id => RunCatalog.Card(id).Rarity).ThenBy(id => id, StringComparer.Ordinal).First();
                c.Run = c.Run with { Cards = c.Run.Cards.Where(s => s.Id != selected).Append(new AcquiredCard(selected, c.Run.Stacks(selected) + 1)).ToArray() };
            }
        }
    }
    [Fact]
    public void EveryCardAtCapIsExcludedAndRelicsAreUniqueLimitedToTwo()
    {
        using var m = new Match(); City c = m.Join()!;
        foreach (RewardCard card in RunCatalog.Cards)
        {
            c.Run = new() { Started = true, Cards = [new(card.Id, card.MaxStacks)] };
            Assert.False(RunEffects.Eligible(c, card, 10, 10000, m.Campaign, m.Players.Count));
        }
        c.Run = WithCards("warden", "R01", "R02");
        Assert.All(RunCatalog.Cards.Where(c => c.Rarity == CardRarity.Relic), card => Assert.False(RunEffects.Eligible(c, card, 10, 10000, m.Campaign, m.Players.Count)));
        Assert.False(RunEffects.Eligible(c, RunCatalog.Card("F04"), 10, 10000, m.Campaign, m.Players.Count)); c.Health--;
        Assert.True(RunEffects.Eligible(c, RunCatalog.Card("F04"), 10, 10000, m.Campaign, m.Players.Count));
    }
    [Fact]
    public void AllNineteenAuthoritativeDraftsPreserveSixtySettlementsAndNoFinalDraft()
    {
        using var m = new Match(combatSeed: 123); m.Join(); Assert.True(Act(m, "start").Accepted);
        for (int wave = 1; wave <= 20; wave++)
        {
            for (int preparation = 0; preparation < 3; preparation++) Assert.True(Act(m, "ready").Accepted);
            Assert.Equal(Phase.Combat, m.Phase); Assert.Equal(wave * 3, m.ProductionCount);
            UnitState[] forecast = m.Enemies.ToArray(); Assert.NotEmpty(forecast);
            // Controlled result boundary, not earned combat/balance evidence.
            foreach (UnitState enemy in forecast) m.Combat.Remove(enemy.Id);
            m.Step(); Assert.Equal(wave, m.LastRewardedWave);
            if (wave == 20) { Assert.Equal(Phase.Victory, m.Phase); break; }
            Assert.Equal(Phase.RewardDraft, m.Phase); RunState run = m.Players[1].Run;
            Assert.Equal(wave, run.DraftWave); Assert.Equal(3, run.Offers.Length);
            MatchSnapshot draft = SessionFlow.RoundTrip(m.Snapshot()); string expected = JsonSerializer.Serialize(draft.NextWave, WireJson.Options);
            Assert.False(Act(m, "ready").Accepted); Assert.False(Act(m, "reward:invalid").Accepted);
            Assert.True(Act(m, "reward:" + run.Offers[0]).Accepted); Assert.Equal(Phase.Building, m.Phase);
            Assert.False(Act(m, "reward:" + run.Offers[0]).Accepted);
            Assert.Equal(expected, JsonSerializer.Serialize(m.Snapshot().NextWave, WireJson.Options));
            Assert.Equal(wave * 3, m.ProductionCount);
        }
        Assert.Equal(19, m.Players[1].Run.Cards.Sum(c => c.Count)); Assert.Equal(60, m.ProductionCount);
        string terminal = JsonSerializer.Serialize(m.Snapshot(), WireJson.Options); m.Step(); m.Step();
        Assert.Equal(terminal, JsonSerializer.Serialize(m.Snapshot(), WireJson.Options));
    }
    [Fact]
    public void DisconnectFallbackIsAuthorityOwnedAndReconnectRetainsOffersAndChoice()
    {
        using var m = new Match(combatSeed: 8); m.Join(); m.Join(); Assert.True(Act(m, "start").Accepted);
        for (int stage = 0; stage < 3; stage++) { Assert.True(Act(m, "ready").Accepted); Assert.True(Act(m, "ready", 2).Accepted); }
        foreach (UnitState enemy in m.Enemies.ToArray()) m.Combat.Remove(enemy.Id);
        m.Step(); Assert.Equal(Phase.RewardDraft, m.Phase);
        string[] offers = m.Players[2].Run.Offers.ToArray();
        Assert.True(Act(m, "reward:" + m.Players[1].Run.Offers[0]).Accepted); Assert.Equal(Phase.RewardDraft, m.Phase);
        m.SetConnected(2, false); Assert.Equal(Phase.Building, m.Phase); Assert.Equal(offers[0], m.Players[2].Run.Choice);
        m.SetConnected(2, true); Assert.Equal(offers, m.Players[2].Run.Offers);
        SnapshotCompatibility.RequireSharedArena(SessionFlow.RoundTrip(m.Snapshot()));
    }
    [Fact]
    public void LethalTickAttributionUsesActualAppliedSharesAndLowestMemberTie()
    {
        Assert.Equal(2, KillAttribution.Winner(100, [(3, 50), (2, 50)]));
        Assert.Equal(3, KillAttribution.Winner(100, [(2, 20), (3, 80)]));
        Assert.Equal(2, KillAttribution.Winner(1, [(3, 50), (2, 50)]));
        Assert.Equal(4, KillAttribution.Winner(100, [(4, 30), (4, 30), (2, 50)]));
    }
}
