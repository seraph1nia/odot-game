using System.Text.Json;
using DevRunner;
using Xunit;

namespace Game.Core.Tests;

public sealed class CommandConstructionTests
{
    [Theory]
    [InlineData("upgrade-capacity", 2)]
    [InlineData("upgrade-healing", 3)]
    [InlineData("upgrade", -1)]
    [InlineData("store", -1)]
    public void ContextAndExplicitArgumentsMatchTheUnchangedWireConstructor(string action, int track)
    {
        using var match = new Match(matchId: "quoted-match", combatSeed: 1);
        match.Join(); match.Join();
        MatchSnapshot state = match.Snapshot() with { Phase = Phase.Preparation, TurnSerial = 17 };
        CityState target = state.Players[1];
        SlotState[] slots = target.Slots.ToArray();
        slots[4] = new(Building.TownHall, 1) { Generation = 19, CapacityLevel = 2, HealingLevel = 3 };
        slots[8] = slots[8] with { Purchased = true };
        state = state with
        {
            Players = [state.Players[0], target with { Slots = slots, Army = target.Army! with { PurchasedHomes = 4 } }]
        };
        Command actual = Command.FromSnapshot(state, 23, action, 2, 4, Building.ArrowTower,
            UnitType.Crossbowman, TechnologyId.RangedFoundation, Resource.Cloth, 7, ConstructionPayment.GoldRecovery, 31);
        var expected = new Command(23, "quoted-match", Phase.Preparation, 17, action, 2, 4, Building.ArrowTower,
            UnitType.Crossbowman, 19, 1, Resource.Cloth, 7, TechnologyId.RangedFoundation,
            ConstructionPayment.GoldRecovery, 31, 4, track);
        Assert.Equal(expected, actual);
        string json = JsonSerializer.Serialize(actual, WireJson.Options);
        Assert.Equal(JsonSerializer.Serialize(expected, WireJson.Options), json);
        Assert.Equal(expected, JsonSerializer.Deserialize<Command>(json, WireJson.Options));
        var plan = new EconomyAction(action, 4, Building.ArrowTower, UnitType.Crossbowman,
            TechnologyId.RangedFoundation, Resource.Cloth, 7, ConstructionPayment.GoldRecovery, 31);
        Assert.Equal(actual, plan.Command(state, 2, 23));
    }

    [Fact]
    public void DefaultsAndInvalidTargetsRemainRequestsForAuthorityValidation()
    {
        using var match = new Match(matchId: "defaults", combatSeed: 1); match.Join();
        MatchSnapshot state = match.Snapshot();
        Assert.Equal(new Command(1, "defaults", Phase.Lobby, 1, "recruit", 1, ExpectedExpansionCount: 0, ExpectedHomeCount: 2),
            Command.FromSnapshot(state, 1, "recruit", 1));
        foreach (int slot in new[] { -1, 9, int.MaxValue })
        {
            Command invalid = Command.FromSnapshot(state, 2, "upgrade-capacity", 1, slot);
            Assert.Equal(slot, invalid.Slot); Assert.Equal(0, invalid.ExpectedGeneration); Assert.Equal(-1, invalid.ExpectedTrackLevel);
        }
        Command foreign = Command.FromSnapshot(state, 3, "upgrade-healing", 99, 0);
        Assert.Equal(99, foreign.City); Assert.Equal(0, foreign.ExpectedGeneration);
        Assert.Equal(-1, foreign.ExpectedExpansionCount); Assert.Equal(-1, foreign.ExpectedHomeCount); Assert.Equal(-1, foreign.ExpectedTrackLevel);
        MatchSnapshot noArmy = state with { Players = [state.Players[0] with { Army = null }] };
        Assert.Equal(-1, Command.FromSnapshot(noArmy, 4, "buy-home", 1).ExpectedHomeCount);
        Assert.Throws<InvalidOperationException>(() => new EconomyAction("ready").Command(state, 99));
    }

    [Theory]
    [InlineData(AuthorityPolicy.Solo)]
    [InlineData(AuthorityPolicy.Dedicated)]
    public void CapturedInstanceAndPriceStayStaleWhileAcceptedRetryKeepsItsIdentity(AuthorityPolicy policy)
    {
        using var flow = new SessionFlow(policy, players: 1);
        int player = flow.State.Players.Single().Id;
        Command Request(string action, int slot = -1, Building building = Building.Empty)
        {
            // Reserve through the ordinary driver's ledger, but construct from the captured snapshot.
            Command reserved = flow.Command(player, new(action, slot, building));
            return Command.FromSnapshot(flow.State, reserved.Sequence, action, player, slot, building);
        }
        void Act(string action, int slot = -1, Building building = Building.Empty)
            => Assert.True(flow.Send(player, Request(action, slot, building)).Accepted);
        string State() => JsonSerializer.Serialize(flow.State, WireJson.Options);
        Act("start"); Act("build", 0, Building.Farm);
        Command oldSale = Request("sell", 0);
        Act("sell", 0); Act("build", 0, Building.Farm);
        string before = State();
        Assert.Equal("Already processed; use current state.", flow.Send(player, oldSale).Message); Assert.Equal(before, State());
        // A fresh identity carrying the old captured context reaches the stale-instance guard.
        Command delayedSale = oldSale with { Sequence = Request("sell", 0).Sequence };
        Assert.Equal("Stale building instance.", flow.Send(player, delayedSale).Message); Assert.Equal(before, State());
        Command stalePrice = Request("buy-home"), accepted = Request("buy-home");
        CommandResult result = flow.Send(player, accepted); Assert.True(result.Accepted);
        before = State();
        Assert.Equal("Already processed; use current state.", flow.Send(player, stalePrice).Message); Assert.Equal(before, State());
        Command delayedPrice = stalePrice with { Sequence = Request("buy-home").Sequence };
        Assert.Equal("Stale home purchase price.", flow.Send(player, delayedPrice).Message); Assert.Equal(before, State());
        Assert.Equal(result, flow.Send(player, accepted)); Assert.Equal(before, State());
        Assert.Equal(2, accepted.ExpectedHomeCount);
        Assert.Equal(3, Request("buy-home").ExpectedHomeCount);
    }
}
