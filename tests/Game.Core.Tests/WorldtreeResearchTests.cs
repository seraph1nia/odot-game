using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

// Tier A: production/authority/projection regressions need no engine. Small fixed
// cities and ordinary commands; cheaper existing combat tests miss new output and
// the canonical visual mapping. No campaign, sockets or shared mutable fixtures.
public sealed class WorldtreeResearchTests
{
    [Theory]
    [InlineData(TechnologyId.Stonecraft, TechnologyId.MeleeFoundation, TechnologyId.Guardian, Building.Stonecutter, Resource.Stone, 1, 2, 3)]
    [InlineData(TechnologyId.ManagedForestry, TechnologyId.RangedFoundation, TechnologyId.Precision, Building.Lumbermill, Resource.Wood, 1, 2, 3)]
    [InlineData(TechnologyId.Irrigation, TechnologyId.MagicFoundation, TechnologyId.Frost, Building.Farm, Resource.Food, 2, 7, 10)]
    public void PurchasesAffectActualProductionAndForecastOnceAndSurviveWireRestoration(TechnologyId id, TechnologyId foundation, TechnologyId prerequisite, Building producer, Resource resource, int bonus, int levelOne, int levelTwo)
    {
        using var match = new Match(); match.Join(); Apply(match, "start"); City city = match.Players[1]; city.Research = new(30);
        Assert.False(match.Apply(1, Request(match, "research-tech", id)).Accepted);
        Apply(match, "research-tech", foundation); Apply(match, "research-tech", prerequisite);
        city.Slots[0] = new(producer, 1); city.Slots[1] = new(producer, 2);
        var before = city.Snapshot(); var stocks = city.Resources; var combatIdentity = match.Configuration.Fingerprint;
        Apply(match, "research-tech", id);
        Assert.Equal(before.ProductionIncome!.Value.Amount(resource) + 2 * bonus, city.Snapshot().ProductionIncome!.Value.Amount(resource));
        Assert.Equal(stocks, city.Resources); Assert.Equal(12, city.Research.Points);
        Assert.Equal(before.RecruitmentQuotes, city.Snapshot().RecruitmentQuotes);
        var purchased = match.Snapshot();
        BuildingDefinition definition = purchased.BuildingCatalog.Single(b => b.Type == producer);
        string unit = resource.ToString().ToLowerInvariant();
        Assert.Equal($"+{levelOne} {unit}/turn", ProgressionPresentation.ProducerBenefit(definition, research: city.Research, technologies: purchased.TechnologyCatalog));
        Assert.Equal($"+{levelTwo} {unit}/turn", ProgressionPresentation.ProducerBenefit(definition, 2, research: city.Research, technologies: purchased.TechnologyCatalog));
        Assert.Equal($"{levelOne} → {levelTwo} {unit}/turn", ProgressionPresentation.ProducerBenefit(definition, upgrade: true, research: city.Research, technologies: purchased.TechnologyCatalog));
        Assert.Equal(levelOne + levelTwo, purchased.Players[0].ProductionIncome!.Value.Amount(resource) - match.Economy.BaseProduction.Amount(resource));
        Assert.False(match.Apply(1, Request(match, "research-tech", id)).Accepted);
        Assert.False(match.Apply(1, Request(match, "research-tech", (TechnologyId)63)).Accepted);
        Assert.Equal(JsonSerializer.Serialize(purchased, WireJson.Options), JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        ResourceCost income = city.Snapshot().ProductionIncome!.Value; Apply(match, "ready");
        Assert.True(stocks.TryAdd(income, out ResourceCost expected)); Assert.Equal(expected, city.Resources);
        MatchSnapshot restored = SnapshotPayload.Decode(SnapshotPayload.Encode(match.Snapshot()))!;
        Assert.Equal(city.Research, restored.Players[0].Research);
        Assert.Equal(city.Snapshot().ProductionIncome, new EconomyConfiguration(restored.Rules).ProjectedProduction(restored.Players[0].Slots, restored.Players[0].Research));
        Assert.Equal(combatIdentity, match.Configuration.Fingerprint);
        city.Slots[0] = new(Building.Empty, 0); city.Slots[1] = new(Building.Empty, 0);
        Assert.Equal(match.Economy.BaseProduction, city.Snapshot().ProductionIncome);
    }
    [Theory]
    [InlineData(AuthorityPolicy.Solo)]
    [InlineData(AuthorityPolicy.PlayingHost)]
    [InlineData(AuthorityPolicy.Dedicated)]
    public void OrdinaryAuthorityIncomePurchaseDuplicateStaleAndResumeKeepNewResearch(AuthorityPolicy policy)
    {
        var rules = new Rules { StartingGold = 500, StartingWood = 100, StoneOutput = 100, Research = new() { FoundationCost = 1, SpecializationCost = 1, MasteryCost = 1 } };
        using var session = new AuthoritySession(policy, rules);
        AdmissionResult? admission = policy == AuthorityPolicy.Dedicated ? session.Admit(2, WireJson.ProtocolVersion, "", 0) : null;
        int player = admission?.PlayerId ?? session.LocalPlayerId; int peer = 2; long sequence = 0;
        Command RequestFor(string action, int slot = -1, Building building = Building.Empty, TechnologyId technology = TechnologyId.None)
        {
            var state = session.Snapshot(); return new(++sequence, state.MatchId, state.Phase, state.TurnSerial, action, player, slot, building, Technology: technology,
                ExpectedGeneration: slot < 0 ? 0 : state.Players[0].Slots[slot].Generation);
        }
        CommandResult Send(Command command) => policy == AuthorityPolicy.Dedicated ? session.Request(peer, JsonSerializer.Serialize(command, WireJson.Options), (ulong)sequence * 1000)! : session.ExecuteLocal(command);
        void Act(string action, int slot = -1, Building building = Building.Empty, TechnologyId technology = TechnologyId.None) => Assert.True(Send(RequestFor(action, slot, building, technology)).Accepted);
        Act("start");
        Assert.False(Send(RequestFor("research-tech", technology: TechnologyId.Stonecraft)).Accepted);
        Act("build", 0, Building.Stonecutter);
        Command stale = RequestFor("research-tech", technology: TechnologyId.MeleeFoundation); Act("ready"); Assert.Contains("Stale match, phase or turn", Send(stale with { Sequence = ++sequence }).Message);
        for (int slot = 1; slot <= 4; slot++) Act("build", slot, Building.ResearchTower);
        Act("upgrade", 1); Act("ready");
        Act("research-tech", technology: TechnologyId.MeleeFoundation); Act("research-tech", technology: TechnologyId.Guardian);
        Command purchase = RequestFor("research-tech", technology: TechnologyId.Stonecraft);
        Assert.True(Send(purchase).Accepted); Assert.Equal(Send(purchase), Send(purchase));
        Assert.False(Send(RequestFor("research-tech", technology: TechnologyId.Stonecraft)).Accepted);
        Assert.True(session.Snapshot().Players[0].Research.Has(TechnologyId.Stonecraft));
        Assert.Equal(0, session.Snapshot().Players[0].Research.Points);
        if (policy == AuthorityPolicy.Dedicated)
        {
            session.Disconnect(peer); peer = 3;
            var resumed = session.Admit(peer, WireJson.ProtocolVersion, admission!.Credential, 100000);
            Assert.True(resumed.Accepted); Assert.True(resumed.State!.Players[0].Research.Has(TechnologyId.Stonecraft));
            Assert.Equal(101, resumed.State.Players[0].ProductionIncome!.Value.Stone);
            Assert.False(session.Request(peer, "{\"technology\":\"bogus\"}", 101000)!.Accepted);
        }
    }
    [Fact]
    public void NewBitsAreAdditiveAndUnsafeStateOrOverflowNeverAwardsProduction()
    {
        var catalog = new ResearchCatalog(); ResearchState legacy = new(Points: 5, Progress: 2, Owned: 1UL << (int)TechnologyId.MeleeFoundation);
        legacy.Validate(); Assert.True(legacy.Has(TechnologyId.MeleeFoundation)); Assert.False(legacy.Has(TechnologyId.Stonecraft));
        Assert.Throws<ArgumentException>(() => new ResearchState(Owned: 1UL << 19).Validate());
        Assert.Throws<ArgumentException>(() => new ResearchState(Owned: 1).Validate());
        using var malformed = new Match(); malformed.Join(); Apply(malformed, "start");
        malformed.Players[1].Research = new(Owned: 1UL << (int)TechnologyId.Stonecraft);
        long revision = malformed.Revision;
        CommandResult rejected = malformed.Apply(1, Request(malformed, "research-tech", TechnologyId.MeleeFoundation));
        Assert.False(rejected.Accepted); Assert.Equal("Invalid research state.", rejected.Message); Assert.Equal(revision, malformed.Revision);
        var economy = new EconomyConfiguration(new Rules { WoodOutput = int.MaxValue });
        ResearchState owned = new(Owned: (1UL << (int)TechnologyId.RangedFoundation) | (1UL << (int)TechnologyId.Precision) | (1UL << (int)TechnologyId.ManagedForestry));
        Assert.Null(economy.ProjectedProduction([new(Building.Lumbermill, 1)], owned));
        Assert.Equal("Unavailable", ProgressionPresentation.ProducerBenefit(economy.Building(Building.Lumbermill), research: owned, technologies: catalog.Definitions()));
        Assert.Equal("Unavailable", ProgressionPresentation.ProducerBenefit(economy.Building(Building.Lumbermill), upgrade: true, research: owned, technologies: catalog.Definitions()));
        Assert.Throws<ArgumentException>(() => catalog.ValidateState(new(Owned: 1UL << (int)TechnologyId.ManagedForestry)));
        Assert.Throws<ArgumentException>(() => catalog.ValidateState(new(Owned: (1UL << (int)TechnologyId.MeleeFoundation) | (1UL << (int)TechnologyId.Guardian) | (1UL << (int)TechnologyId.Assault))));
        Assert.False(catalog.Eligibility(default, TechnologyId.ManagedForestry).Available);
        Assert.False(catalog.Eligibility(new ResearchState(100).Purchase(TechnologyId.RangedFoundation, catalog).Purchase(TechnologyId.Venom, catalog), TechnologyId.ManagedForestry).Available);
    }
    [Fact]
    public void CompleteMappingDistinguishesBaselineInactiveOwnedAndAuthorityGates()
    {
        using var match = new Match(); match.Join(); Apply(match, "start"); City city = match.Players[1]; city.Research = new(30);
        var slots = WorldtreeProjection.Slots; Assert.Equal(24, slots.Count); Assert.Equal(24, slots.Select(s => s.VisualId).Distinct().Count());
        Assert.Equal(Enum.GetValues<TechnologyId>().Where(id => id != TechnologyId.None), slots.Where(s => s.Technology != TechnologyId.None).Select(s => s.Technology).Order());
        var before = WorldtreeProjection.Project(match.Snapshot(), city.Snapshot(), 1, true, "primary_left"); Assert.Equal(3, before.Acquired.Length); Assert.Equal(3, before.Available.Length);
        // Tier A: exact authoritative costs/denials survive concise hover copy;
        // native focus/layout belongs to the existing Tier B component checks.
        string description = WorldtreeProjection.Description("primary_left", match.Snapshot(), city.Snapshot(), 1, true);
        Assert.Contains("• 3 points", description);
        Assert.DoesNotContain("Available", description); Assert.DoesNotContain("No prerequisite", description); Assert.DoesNotContain("no healing", description);
        Assert.Contains("• Requires Melee Foundation", WorldtreeProjection.Description("secondary_left_a", match.Snapshot(), city.Snapshot(), 1, true));
        Assert.Contains("• Building/preparation only", WorldtreeProjection.Description("primary_left", match.Snapshot() with { Phase = Phase.Combat }, city.Snapshot(), 1, true));
        Apply(match, "research-tech", TechnologyId.MeleeFoundation); Apply(match, "research-tech", TechnologyId.Guardian); Apply(match, "research-tech", TechnologyId.Stonecraft);
        var projected = WorldtreeProjection.Project(match.Snapshot(), city.Snapshot(), 1, true, "terminal_left_a_2"); Assert.Contains("terminal_left_a_2", projected.Acquired);
        Assert.DoesNotContain("terminal_left_a_2", projected.Available); Assert.DoesNotContain("terminal_left_b_2", projected.Available);
        Assert.Contains("Inactive", WorldtreeProjection.Description("terminal_left_b_2", match.Snapshot(), city.Snapshot(), 1, true));
        Assert.Empty(WorldtreeProjection.Project(match.Snapshot(), city.Snapshot(), 2, true, "root").Available);
        Assert.Empty(WorldtreeProjection.Project(match.Snapshot(), city.Snapshot(), 1, false, "root").Available);
        Apply(match, "pause"); Assert.Empty(WorldtreeProjection.Project(match.Snapshot(), city.Snapshot(), 1, true, "root").Available);
    }
    private static Command Request(Match match, string action, TechnologyId technology = TechnologyId.None)
        => new(1, match.Id, match.Phase, match.TurnSerial, action, 1, Technology: technology);
    private static void Apply(Match match, string action, TechnologyId technology = TechnologyId.None) => Assert.True(match.Apply(1, Request(match, action, technology)).Accepted);
}
