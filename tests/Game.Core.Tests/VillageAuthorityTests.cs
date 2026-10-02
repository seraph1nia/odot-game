using System.Text.Json;
using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class VillageAuthorityTests
{
    [Theory]
    [InlineData(false, UnitType.Swordsman, Building.Barracks)]
    [InlineData(true, UnitType.Swordsman, Building.Barracks)]
    [InlineData(false, UnitType.Berserker, Building.Barracks)]
    [InlineData(true, UnitType.Berserker, Building.Barracks)]
    [InlineData(false, UnitType.Crossbowman, Building.ArcheryRange)]
    [InlineData(true, UnitType.Crossbowman, Building.ArcheryRange)]
    [InlineData(false, UnitType.Mage, Building.Arcanum)]
    [InlineData(true, UnitType.Mage, Building.Arcanum)]
    public void CatalogRecruitmentAndResearchAreIdenticalThroughLocalAndGuestLedgers(bool remote, UnitType role, Building building)
    {
        using var session = new AuthoritySession(remote ? AuthorityPolicy.Dedicated : AuthorityPolicy.Solo, new Rules { StartingGold = 500, StartingWood = 150 });
        int player = remote ? session.Admit(2, WireJson.ProtocolVersion, "", 1000).PlayerId : session.LocalPlayerId;
        long sequence = 0;
        Command Request(string action, int slot = -1, Building type = Building.Empty)
        {
            MatchSnapshot state = session.Snapshot();
            return new(++sequence, state.MatchId, state.Phase, state.TurnSerial, action, player, slot, type, role, Catalogs.Class(role), ExpectedGeneration: slot is >= 0 and < 9 ? state.Players.Single(p => p.Id == player).Slots[slot].Generation : 0, ExpectedExpansionCount: state.Players.Single(p => p.Id == player).Slots.Count(s => s.Purchased) - 5);
        }
        CommandResult Send(Command request) => remote ? session.Request(2, JsonSerializer.Serialize(request, WireJson.Options), 1000)! : session.ExecuteLocal(request);
        foreach (Command request in new[] { Request("start"), Request("build", 0, Building.Farm) })
        {
            // Capture each request at its actual phase (start changes the next command's phase).
            Command current = request with { ExpectedPhase = session.Phase, TurnSerial = session.Snapshot().TurnSerial };
            Assert.True(Send(current).Accepted);
        }
        Assert.True(Send(Request("buy-plot", 5)).Accepted); Assert.True(Send(Request("buy-plot", 6)).Accepted); Assert.True(Send(Request("buy-plot", 7)).Accepted);
        for (int slot = 4; slot <= 6; slot++) Assert.True(Send(Request("build", slot, Building.Stonecutter)).Accepted);
        Assert.True(Send(Request("build", 7, role == UnitType.Mage ? Building.Weaver : Building.MetalMine)).Accepted);
        for (int production = 0; production < 3; production++)
        {
            Assert.True(Send(Request("ready")).Accepted);
            if (production == 1) Assert.True(Send(Request("upgrade", 7)).Accepted);
        }
        Assert.True(Send(Request("build", 1, building)).Accepted); Assert.True(Send(Request("upgrade", 1)).Accepted);
        Assert.True(Send(Request("build", 2, Building.Blacksmith)).Accepted);
        Assert.True(Send(Request("build", 3, Building.Blacksmith)).Accepted);
        Command rank = Request("research", 2); Assert.True(Send(rank).Accepted);
        string beforeRetry = JsonSerializer.Serialize(session.Snapshot()); Assert.True(Send(rank).Accepted); Assert.Equal(beforeRetry, JsonSerializer.Serialize(session.Snapshot()));
        Assert.False(Send(Request("research", 3)).Accepted);
        CityState before = session.Snapshot().Players.Single(); Command recruit = Request("recruit", 1);
        Assert.True(Send(recruit).Accepted); CityState after = session.Snapshot().Players.Single();
        ResourceCost price = before.RecruitmentQuotes.Single(q => q.Type == role && q.Level == before.Slots[1].Level).Cost;
        Assert.True(before.Resources.TryPay(price, out ResourceCost balance)); Assert.Equal(balance, after.Resources); Assert.Equal(before.Food, after.Food);
        Assert.Equal(1, after.Soldiers.Single().Rank); Assert.Equal(before.Wood - price.Wood, after.Wood);
        beforeRetry = JsonSerializer.Serialize(session.Snapshot()); Assert.True(Send(recruit).Accepted); Assert.Equal(beforeRetry, JsonSerializer.Serialize(session.Snapshot()));
        Command bad = Request("recruit", 0); Assert.False(Send(bad).Accepted);
    }
    [Fact]
    public void PreparationDisconnectAndPauseResolveExactlyOneIncomeFreeCheck()
    {
        using var match = new Match(); match.Join(); match.Join(); VillageStrategyTests.Act(match, 1, "start");
        for (int production = 0; production < 3; production++)
        { Assert.True(VillageStrategyTests.Act(match, 1, "ready").Accepted); Assert.True(VillageStrategyTests.Act(match, 2, "ready").Accepted); }
        Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(3, match.ProductionCount);
        Assert.True(VillageStrategyTests.Act(match, 1, "ready").Accepted); Assert.True(VillageStrategyTests.Act(match, 2, "pause").Accepted);
        match.SetConnected(2, false); Assert.Equal(Phase.Preparation, match.Phase); Assert.Equal(90, match.Players[1].Gold);
        Assert.True(VillageStrategyTests.Act(match, 1, "resume").Accepted); Assert.Equal(Phase.Combat, match.Phase); Assert.Equal(90, match.Players[1].Gold);
        Assert.Equal(3, match.ProductionCount); Assert.All(match.Players.Values, c => Assert.False(c.Ready));
    }
}
