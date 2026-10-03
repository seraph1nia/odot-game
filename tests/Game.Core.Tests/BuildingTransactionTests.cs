using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class BuildingTransactionTests
{
    private static Match Start(int players = 1, Rules? rules = null)
    {
        var match = new Match(rules, combatSeed: 1);
        for (int i = 0; i < players; i++) match.Join();
        Assert.True(match.Apply(1, Request(match, "start")).Accepted); return match;
    }
    private static Command Request(Match match, string action, int slot = -1, Building building = Building.Empty, int city = 1,
        Resource resource = Resource.Wood, int bundles = 0)
        => new(1, match.Id, match.Phase, match.TurnSerial, action, city, slot, building,
            ExpectedGeneration: slot is >= 0 and < 9 ? match.Players[city].Slots[slot].Generation : 0,
            ExpectedExpansionCount: match.Players[city].ExpansionCount, Resource: resource, Bundles: bundles);
    private static void Act(Match match, string action, int slot = -1, Building building = Building.Empty)
        => Assert.True(match.Apply(1, Request(match, action, slot, building)).Accepted);
    private static void RejectUnchanged(Match match, Command command, int sender = 1)
    {
        string before = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(match.Apply(sender, command).Accepted);
        Assert.Equal(before, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
    }
    private static void Stock(City city) { city.Gold = 1000; city.Wood = 1000; city.Food = 100; city.Stone = 1000; city.Metal = 1000; city.Cloth = 1000; }

    [Fact]
    public void ExpansionIsSelectedPermanentCountPricedAndOwned()
    {
        using Match match = Start(2); Stock(match.Players[1]);
        Assert.Equal(Enumerable.Range(0, 5), match.Players[1].Slots.Select((s, i) => (s, i)).Where(p => p.s.Purchased).Select(p => p.i));
        for (int slot = 0; slot < 5; slot++) Act(match, "build", slot, Building.Farm);
        RejectUnchanged(match, Request(match, "build", 5, Building.Farm));
        Command stale = Request(match, "buy-plot", 5);
        RejectUnchanged(match, Request(match, "buy-plot", 8, city: 2));
        int[] order = [8, 6, 5, 7], prices = [25, 40, 60, 90];
        for (int count = 0; count < 4; count++)
        {
            int gold = match.Players[1].Gold; Act(match, "buy-plot", order[count]);
            Assert.Equal(gold - prices[count], match.Players[1].Gold);
            Assert.Equal(Building.Empty, match.Players[1].Slots[order[count]].Type);
            if (count == 0) RejectUnchanged(match, stale);
            RejectUnchanged(match, Request(match, "buy-plot", order[count]));
        }
        Act(match, "build", 8, Building.Farm); Act(match, "sell", 8);
        Assert.True(match.Players[1].Slots[8].Purchased); Assert.Equal(4, match.Players[1].ExpansionCount);
        match.Players[1].Gold = 0; RejectUnchanged(match, Request(match, "build", 8, Building.Farm));
    }

    [Fact]
    public void EconomicTransactionsRespectReadyPauseConnectionAndElimination()
    {
        using Match match = Start(2); City city = match.Players[1]; Stock(city);
        Act(match, "build", 0, Building.Market);
        Command[] transactions = [Request(match, "buy-plot", 8), Request(match, "sell", 0), Request(match, "trade", 0, bundles: 1)];
        Act(match, "ready"); foreach (Command command in transactions) RejectUnchanged(match, command);
        Act(match, "unready"); Act(match, "pause"); foreach (Command command in transactions) RejectUnchanged(match, command);
        Act(match, "resume"); match.SetConnected(1, false); foreach (Command command in transactions) RejectUnchanged(match, command);
        match.SetConnected(1, true); city.Gold = 0; RejectUnchanged(match, Request(match, "buy-plot", 8));
        city.Health = 0; foreach (Command command in transactions) RejectUnchanged(match, command);
    }

    [Fact]
    public void SaleRefundsActualSummedInvestmentAndRejectsReplacementTargets()
    {
        using Match match = Start(rules: new Rules { BuildCost = 21, UpgradeCost = 23 }); City city = match.Players[1]; Stock(city);
        Act(match, "build", 0, Building.Farm); Act(match, "upgrade", 0);
        Assert.Equal(new ResourceCost(44, 20, Stone: 10), city.Slots[0].Investment);
        Assert.Equal(new ResourceCost(22, 10, Stone: 5), city.Slots[0].Refund);
        Command[] stale = [Request(match, "sell", 0), Request(match, "upgrade", 0), Request(match, "recruit", 0), Request(match, "trade", 0), Request(match, "research", 0)];
        ResourceCost before = city.Resources; long oldGeneration = city.Slots[0].Generation;
        Act(match, "sell", 0); Assert.True(before.TryAdd(new(22, 10, Stone: 5), out ResourceCost refunded));
        Assert.Equal(refunded, city.Resources); Assert.Equal(default, city.Slots[0].Investment); Assert.True(city.Slots[0].Purchased);
        Act(match, "build", 0, Building.Farm); Assert.NotEqual(oldGeneration, city.Slots[0].Generation);
        Assert.Equal(1, city.Slots[0].Level); Assert.Equal(new ResourceCost(21, 10), city.Slots[0].Investment);
        Assert.Equal(new ResourceCost(10, 5), city.Slots[0].Refund);
        foreach (Command command in stale) RejectUnchanged(match, command);
        Act(match, "sell", 0); ResourceCost noProducer = city.Resources; Act(match, "ready");
        Assert.Equal(noProducer.Food, city.Food); Assert.Equal(noProducer.Gold + 10, city.Gold);
    }

    [Fact]
    public void RefundAndTradeOverflowRejectWithoutRemovingInstanceOrStock()
    {
        using Match match = Start(); City city = match.Players[1]; Stock(city);
        Act(match, "build", 0, Building.Market); city.Gold = int.MaxValue;
        RejectUnchanged(match, Request(match, "sell", 0));
        RejectUnchanged(match, Request(match, "trade", 0, resource: Resource.Food, bundles: 1));
        Assert.Equal(Building.Market, city.Slots[0].Type);
    }

    [Fact]
    public void MarketUsesOwnedInstancesExactBundlesAndLedgerDeduplication()
    {
        using Match match = Start(2); City city = match.Players[1]; Stock(city);
        Act(match, "build", 0, Building.Market); Act(match, "build", 1, Building.Market);
        var ledger = new CommandLedger(); Command sell = Request(match, "trade", 0, resource: Resource.Cloth, bundles: 3);
        ResourceCost before = city.Resources;
        Assert.True(ledger.Execute(sell, () => match.Apply(1, sell)).Accepted);
        Assert.Equal(before.Cloth - 15, city.Cloth); Assert.Equal(before.Gold + 6, city.Gold);
        string after = JsonSerializer.Serialize(match.Snapshot());
        Assert.True(ledger.Execute(sell, () => match.Apply(1, sell)).Accepted); Assert.Equal(after, JsonSerializer.Serialize(match.Snapshot()));
        RejectUnchanged(match, Request(match, "trade", 0, bundles: 0));
        RejectUnchanged(match, Request(match, "trade", 0, bundles: -1));
        RejectUnchanged(match, Request(match, "trade", 0, resource: Resource.Gold, bundles: 1));
        RejectUnchanged(match, Request(match, "trade", 0, resource: (Resource)99, bundles: 1));
        RejectUnchanged(match, Request(match, "trade", 0, resource: Resource.Cloth, bundles: int.MaxValue));
        RejectUnchanged(match, Request(match, "trade", 0, city: 2, bundles: 1));
        city.Wood = 4; RejectUnchanged(match, Request(match, "trade", 0, bundles: 1));
        Act(match, "sell", 0);
        Assert.True(match.Apply(1, Request(match, "trade", 1, resource: Resource.Cloth, bundles: 1)).Accepted);
        Act(match, "sell", 1); RejectUnchanged(match, Request(match, "trade", 1, bundles: 1));
        Act(match, "buy-plot", 8);
    }

    [Theory]
    [InlineData(AuthorityPolicy.Solo, false)]
    [InlineData(AuthorityPolicy.PlayingHost, false)]
    [InlineData(AuthorityPolicy.PlayingHost, true)]
    [InlineData(AuthorityPolicy.Dedicated, true)]
    public void OrdinaryAuthorityRequestsDeduplicateNewTransactions(AuthorityPolicy policy, bool remote)
    {
        using var session = new AuthoritySession(policy, new Rules { StartingGold = 300, StartingWood = 100 });
        int player = remote ? session.Admit(2, WireJson.ProtocolVersion, "", 1000).PlayerId : session.LocalPlayerId;
        long sequence = 0;
        Command RequestAt(string action, int slot = -1, Building building = Building.Empty, Resource resource = Resource.Stone, int bundles = 0)
        {
            MatchSnapshot state = session.Snapshot(); CityState city = state.Players.Single(p => p.Id == player);
            return new(++sequence, state.MatchId, state.Phase, state.TurnSerial, action, player, slot, building,
                ExpectedGeneration: slot is >= 0 and < 9 ? city.Slots[slot].Generation : 0,
                ExpectedExpansionCount: city.Slots.Count(s => s.Purchased) - 5, Resource: resource, Bundles: bundles);
        }
        CommandResult Send(Command request) => remote ? session.Request(2, JsonSerializer.Serialize(request, WireJson.Options), 1000)! : session.ExecuteLocal(request);
        if (remote && policy == AuthorityPolicy.PlayingHost)
        {
            MatchSnapshot lobby = session.Snapshot(); Assert.True(session.ExecuteLocal(new(1, lobby.MatchId, lobby.Phase, lobby.TurnSerial, "start", session.LocalPlayerId)).Accepted);
        }
        else Assert.True(Send(RequestAt("start")).Accepted);
        Assert.True(Send(RequestAt("build", 0, Building.Stonecutter)).Accepted);
        for (int production = 0; production < 3; production++)
        {
            Assert.True(Send(RequestAt("ready")).Accepted);
            if (remote && policy == AuthorityPolicy.PlayingHost)
            {
                MatchSnapshot state = session.Snapshot(); Assert.True(session.ExecuteLocal(new(production + 2, state.MatchId, state.Phase, state.TurnSerial, "ready", session.LocalPlayerId)).Accepted);
            }
        }
        Assert.True(Send(RequestAt("build", 1, Building.Market)).Accepted);
        foreach (Command command in new[] { RequestAt("buy-plot", 8), RequestAt("trade", 1, bundles: 1), RequestAt("sell", 1) })
        {
            Assert.True(Send(command).Accepted);
            string after = JsonSerializer.Serialize(session.Snapshot(), WireJson.Options);
            Assert.True(Send(command).Accepted); Assert.Equal(after, JsonSerializer.Serialize(session.Snapshot(), WireJson.Options));
        }
        CityState result = session.Snapshot().Players.Single(p => p.Id == player);
        Assert.True(result.Slots[8].Purchased); Assert.Equal(Building.Empty, result.Slots[1].Type); Assert.Equal(5, result.Stone);
    }

    [Fact]
    public void SalesKeepVeteransAndResearchButRemoveTowerAndResearchAccess()
    {
        using Match match = Start(); City city = match.Players[1]; Stock(city);
        Act(match, "build", 0, Building.Barracks); Act(match, "recruit", 0);
        UnitState old = Assert.Single(city.Soldiers); match.Combat.Seed(old with { Health = old.Health - 100 });
        Act(match, "build", 1, Building.ResearchTower); city.Research = city.Research.Income(points: 3); Assert.True(match.Apply(1, Request(match, "research-tech") with { Technology = TechnologyId.MeleeFoundation }).Accepted);
        UnitState ranked = Assert.Single(city.Soldiers); Act(match, "sell", 0); Act(match, "sell", 1);
        Assert.Equal(ranked, Assert.Single(city.Soldiers)); Assert.True(city.Research.Has(TechnologyId.MeleeFoundation));
        RejectUnchanged(match, Request(match, "research", 1));
        Act(match, "build", 0, Building.Barracks); Assert.Single(city.Soldiers); Assert.Equal(1, city.Slots[0].Level);
        Act(match, "build", 2, Building.CatapultTower); Act(match, "sell", 2); Assert.Empty(city.Towers);
        for (int turn = 0; turn < 4; turn++) Act(match, "ready");
        for (int step = 0; step < 200; step++) match.Step();
        Assert.DoesNotContain(match.Snapshot().CombatEvents, e => e.Tower?.Slot == 2);
    }
}
