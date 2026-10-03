using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class CityResearchTests
{
    [Fact]
    public void CatalogHasThreeIndependentExclusivePathsWithReplacementMastery()
    {
        var catalog = new ResearchCatalog(); Assert.Equal(15, catalog.Definitions().Length);
        foreach (TechnologyDefinition node in catalog.Definitions())
        {
            Assert.Equal(node.Tier * 3, node.Cost);
            Assert.True(TechnologyIds.TryParse(node.Name, out TechnologyId id)); Assert.Equal(node.Id, id);
        }
        ResearchState state = new(Points: 30);
        foreach (TechnologyId id in new[] { TechnologyId.MagicFoundation, TechnologyId.Fire, TechnologyId.FireMastery, TechnologyId.RangedFoundation, TechnologyId.Venom }) state = state.Purchase(id, catalog);
        Assert.Equal(3, state.Points); Assert.Contains("Permanently", catalog.Eligibility(state, TechnologyId.FrostMastery).Reason);
        Assert.False(catalog.Eligibility(state, TechnologyId.Frost).Available); Assert.False(catalog.Eligibility(state, TechnologyId.Fire).Available);
        Assert.Equal(new UnitCapabilities(FoundationPercent: 5, BurnPercent: 30), catalog.Capabilities(state, UnitType.Mage));
        Assert.Equal(10, catalog.Capabilities(state, UnitType.Crossbowman).PoisonPercent); Assert.Equal(default, catalog.Capabilities(state, UnitType.Swordsman));
        TechnologyDefinition[] nodes = catalog.Definitions(); nodes[0] = nodes[0] with { Cost = 999 }; Assert.Equal(3, catalog.Get(TechnologyId.MeleeFoundation).Cost);
    }
    [Fact]
    public void InvalidCatalogsAndStateFailBeforeUse()
    {
        var catalog = new ResearchCatalog(); TechnologyDefinition[] nodes = catalog.Definitions();
        Assert.Throws<ArgumentException>(() => new ResearchCatalog(nodes.Append(nodes[0])));
        foreach (TechnologyDefinition changed in new[] { nodes[0] with { Id = (TechnologyId)999 }, nodes[0] with { Prerequisite = TechnologyId.Guardian }, nodes[2] with { Branch = TechnologyId.Assault }, nodes[0] with { Benefit = new(ChillPercent: 51) } })
            Assert.Throws<ArgumentException>(() => new ResearchCatalog(nodes.Select(n => n.Id == nodes[0].Id || changed.Id == nodes[2].Id && n.Id == nodes[2].Id ? changed : n)));
        Assert.Throws<ArgumentException>(() => new ResearchState(-1).Validate()); Assert.Throws<ArgumentException>(() => new ResearchState(Progress: 3).Validate());
        Assert.Throws<OverflowException>(() => new ResearchState(int.MaxValue, 2).Income(progress: 1));
        Assert.Throws<OverflowException>(() => new ResearchState(int.MaxValue).Income(points: 1));
        Assert.False(catalog.Eligibility(default, (TechnologyId)999).Available);
        Assert.Throws<ArgumentException>(() => new ResearchCatalog(nodes.Select(n => n.Id == TechnologyId.AssaultMastery ? n with { Prerequisite = TechnologyId.Guardian, Branch = TechnologyId.Guardian } : n)));
    }
    [Fact]
    public void ProductionUsesActualTowersAndRetainsCarryAcrossSalesWithAtomicOverflow()
    {
        using var match = new Match(new Rules { StartingGold = 500, StartingWood = 100 }); match.Join(); match.Join(); Act(match, 1, "start");
        foreach (City city in match.Players.Values) city.Stone = 100;
        Act(match, 1, "build", 0, Building.ResearchTower); Act(match, 1, "build", 1, Building.ResearchTower); Act(match, 1, "upgrade", 1);
        match.SetConnected(2, false); Act(match, 1, "ready"); Assert.Equal(new ResearchState(1), match.Players[1].Research); Assert.Equal(default, match.Players[2].Research);
        Act(match, 1, "sell", 1); Act(match, 1, "ready"); Assert.Equal(new ResearchState(1, 1), match.Players[1].Research);
        Act(match, 1, "upgrade", 0); Act(match, 1, "ready"); Assert.Equal(new ResearchState(2), match.Players[1].Research); Assert.Empty(match.Players[1].Towers);
        using var overflow = new Match(new Rules { StartingGold = 500 }); overflow.Join(); overflow.Join(); Act(overflow, 1, "start");
        overflow.Players[2].Stone = 100; Act(overflow, 2, "build", 0, Building.ResearchTower); overflow.Players[2].Research = new(int.MaxValue, 2);
        Act(overflow, 1, "ready"); string before = JsonSerializer.Serialize(overflow.Snapshot(), WireJson.Options);
        Assert.False(overflow.Apply(2, Request(overflow, 2, "ready")).Accepted); Assert.Equal(before, JsonSerializer.Serialize(overflow.Snapshot(), WireJson.Options));
    }
    [Theory]
    [InlineData(AuthorityPolicy.Solo)]
    [InlineData(AuthorityPolicy.PlayingHost)]
    [InlineData(AuthorityPolicy.Dedicated)]
    public void PurchaseGuardsApplyToEveryAuthorityPolicy(AuthorityPolicy mode)
    {
        AuthorityPolicy policy = mode;
        using var session = new AuthoritySession(policy);
        int player = mode == AuthorityPolicy.Dedicated ? session.Admit(2, WireJson.ProtocolVersion, "", 0).PlayerId : session.LocalPlayerId;
        long seq = 0;
        CommandResult Send(string action, TechnologyId id = TechnologyId.None)
        {
            MatchSnapshot s = session.Snapshot(); var c = new Command(++seq, s.MatchId, s.Phase, s.TurnSerial, action, player, Technology: id);
            return mode == AuthorityPolicy.Dedicated ? session.Request(2, JsonSerializer.Serialize(c, WireJson.Options), 0)! : session.ExecuteLocal(c);
        }
        Assert.True(Send("start").Accepted); Assert.False(Send("research-tech", TechnologyId.MeleeFoundation).Accepted);
        // Ledger transport and ordinary campaign income are separately verified; this fixture isolates ownership/profile atomicity.
        Assert.False(Send("research", TechnologyId.MeleeFoundation).Accepted);
        Assert.True(Send("pause").Accepted); Assert.False(Send("research-tech", TechnologyId.MeleeFoundation).Accepted);
    }
    [Fact]
    public void TowerlessPurchaseRefreshesQuotesWithoutHealingOrChangingLevelAndDetachesSnapshots()
    {
        using var match = new Match(); match.Join(); Act(match, 1, "start"); City city = match.Players[1]; city.Research = new(18);
        int id = match.Combat.Create(UnitType.Mage, 1, 1, 1, level: 3); UnitState original = match.Combat.Read(id); match.Combat.Seed(original with { Health = original.Health - 100 });
        MatchSnapshot before = match.Snapshot();
        Assert.True(match.Apply(1, Request(match, 1, "research-tech") with { Technology = TechnologyId.MagicFoundation }).Accepted);
        UnitState after = Assert.Single(city.Soldiers); Assert.Equal(original.Health - 100, after.Health); Assert.Equal(3, after.Level); Assert.True(after.Profile.Health > original.Profile.Health);
        Assert.True(city.Snapshot().RecruitmentQuotes.Single(q => q.Type == UnitType.Mage && q.Level == 3).Profile.Health > before.Players[0].RecruitmentQuotes.Single(q => q.Type == UnitType.Mage && q.Level == 3).Profile.Health);
        foreach (TechnologyId tech in new[] { TechnologyId.Fire, TechnologyId.FireMastery }) Assert.True(match.Apply(1, Request(match, 1, "research-tech") with { Technology = tech }).Accepted);
        Assert.Equal(30, Assert.Single(city.Soldiers).Capabilities.BurnPercent); Assert.Equal(0, city.Research.Points);
        MatchSnapshot snapshot = match.Snapshot(); snapshot.TechnologyCatalog[0] = snapshot.TechnologyCatalog[0] with { Cost = 99 }; snapshot.Players[0].Technologies[0] = new(TechnologyId.None, true, "fake");
        Assert.Equal(3, match.Snapshot().TechnologyCatalog[0].Cost); Assert.NotEqual(TechnologyId.None, match.Snapshot().Players[0].Technologies[0].Id);
        Assert.Equal(JsonSerializer.Serialize(match.Snapshot(), WireJson.Options), JsonSerializer.Serialize(JsonSerializer.Deserialize<MatchSnapshot>(JsonSerializer.Serialize(match.Snapshot(), WireJson.Options), WireJson.Options), WireJson.Options));
        using var fresh = new Match(); fresh.Join(); Assert.Equal(default, fresh.Players[1].Research);
    }
    [Fact]
    public void EconomicResearchChangesIdentityWithoutRerollingCombat()
    {
        Rules rules = new(); Rules changed = rules with { Research = rules.Research with { FoundationCost = 4 } };
        Assert.Equal(new CombatConfiguration(rules).Fingerprint, new CombatConfiguration(changed).Fingerprint);
        Assert.NotEqual(new EconomyConfiguration(rules).Fingerprint, new EconomyConfiguration(changed).Fingerprint);
        Assert.NotEqual(new CombatConfiguration(rules).Fingerprint, new CombatConfiguration(rules with { Combat = rules.Combat with { Statuses = new StatusRules { BurnDuration = 240 } } }).Fingerprint);
        Assert.Equal(1, new UnitCapabilities(ReductionPercent: 20).Reduce(1));
    }
    private static Command Request(Match m, int city, string action, int slot = -1, Building building = Building.Empty)
        => new(1, m.Id, m.Phase, m.TurnSerial, action, city, slot, building, ExpectedGeneration: slot < 0 ? 0 : m.Players[city].Slots[slot].Generation);
    private static void Act(Match m, int city, string action, int slot = -1, Building building = Building.Empty)
        => Assert.True(m.Apply(city, Request(m, city, action, slot, building)).Accepted);
}
