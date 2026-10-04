using System.Text.Json;
using DevRunner;
using Xunit;

namespace Game.Core.Tests;

// Characterization at the existing authority interface, before dispatch extraction.
// All stock and research come from default paid construction/production/battles.
public sealed class SettlementCommandTests
{
    [Theory]
    [InlineData(AuthorityPolicy.Solo)]
    [InlineData(AuthorityPolicy.Dedicated)]
    public void PaidEditsKeepFeedbackOneRevisionAndOriginalRetryResult(AuthorityPolicy policy)
    {
        using var flow = new SessionFlow(policy, players: 1);
        int player = flow.State.Players.Single().Id;
        flow.Act(player, new("start"));
        for (int wave = 1; wave <= 3; wave++)
        {
            flow.Prepare();
            flow.StepUntil(s => s.Phase != Phase.Combat && s.DyingBodies.Length == 0);
            Assert.Equal(Phase.Building, flow.State.Phase);
            Assert.Equal(wave + 1, flow.State.Wave);
        }
        // Earn one full production cycle without further policy spending.
        for (int turn = 0; turn < 3; turn++) flow.Act(player, new("ready"));
        CityState City() => flow.State.Players.Single();
        string State() => JsonSerializer.Serialize(flow.State, WireJson.Options);
        void Earn(ResourceCost cost)
        {
            for (int attempts = 0; !City().Resources.TryPay(cost, out _) && attempts < 100; attempts++)
            {
                if (City().Gold < cost.Gold && City().Metal >= 15 && City().Slots[5].Type == Building.Market)
                    flow.Act(player, new("trade", 5, Resource: Resource.Metal, Bundles: 1));
                else if (flow.State.Phase == Phase.Building) flow.Act(player, new("ready"));
                else
                {
                    Assert.Equal(Phase.Preparation, flow.State.Phase);
                    Assert.InRange(flow.State.Wave, 1, 8);
                    flow.Invest(player); flow.Act(player, new("ready"));
                    flow.StepUntil(s => s.Phase != Phase.Combat && s.DyingBodies.Length == 0);
                    Assert.Equal(Phase.Building, flow.State.Phase);
                }
            }
            Assert.True(City().Resources.TryPay(cost, out _), $"Paid setup cannot afford {cost}: {City().Resources}");
        }
        void Accepted(EconomyAction action)
        {
            ResourceCost cost = action.Action switch
            {
                "build" => flow.State.BuildingCatalog.Single(b => b.Type == action.Building).Construction,
                "buy-plot" => new(flow.State.PlotPrices[City().Slots.Count(s => s.Purchased) - 5]),
                "buy-home" => new(City().Army!.HomePrices[City().Army!.PurchasedHomes - 2]),
                "upgrade-capacity" => new(8, 2, Stone: 2),
                "upgrade-healing" => new(6, 2, Stone: 2),
                "upgrade" => City().Slots[action.Slot].UpgradeQuote!.Value,
                "recruit" => City().RecruitmentQuotes.Single(q => q.Type == action.Unit && q.Level == City().Slots[action.Slot].Level).Cost,
                _ => default
            };
            Earn(cost);
            long revision = flow.State.Revision;
            Command request = flow.Command(player, action);
            CommandResult result = flow.Send(player, request);
            Assert.True(result.Accepted, $"{action}: {result.Message}; stock={City().Resources}");
            Assert.Equal(action.Action + " accepted.", result.Message);
            Assert.Equal(revision + 1, flow.State.Revision);
            string after = State();
            Assert.Equal(result, flow.Send(player, request));
            Assert.Equal(after, State());
        }
        void Rejected(Command request, string message)
        {
            string before = State();
            CommandResult result = flow.Send(player, request);
            Assert.False(result.Accepted); Assert.Equal(message, result.Message);
            Assert.Equal(before, State());
        }
        Accepted(new("research-tech", Technology: TechnologyId.MeleeFoundation));
        Accepted(new("buy-home"));
        // Frontline setup owns plots 0..6; reserve this independent edit plot.
        int plot = Enumerable.Range(0, 9).Last(i => !City().Slots[i].Purchased);
        Accepted(new("buy-plot", plot));
        Accepted(new("build", plot, Building.TownHall));
        Accepted(new("upgrade-capacity", plot));
        Accepted(new("upgrade-healing", plot));
        int unit = City().Soldiers.First().Id;
        Accepted(new("store", plot, UnitId: unit));
        Rejected(flow.Command(player, new("sell", plot)), "Send or retire stored units before selling this Town hall.");
        Accepted(new("send", plot, UnitId: unit));
        Accepted(new("retire", UnitId: unit));
        Accepted(new("recruit", 2));
        Accepted(new("sell", plot));
        Accepted(new("build", plot, Building.ArrowTower));
        Accepted(new("upgrade", plot));
        Assert.Equal(2, City().Towers.Single(t => t.Slot == plot).Level);
        Accepted(new("sell", plot));
        Accepted(new("trade", 5, Resource: Resource.Metal, Bundles: 1));
        Rejected(flow.Command(player, new("unknown", plot)), "Unknown action.");
        Rejected(flow.Command(player, new("research", plot)), "Unknown action.");
        Rejected(flow.Command(player, new("store", -1, UnitId: City().Soldiers.First().Id)), "Choose a Town hall.");
        Rejected(flow.Command(player, new("recruit", -1)), "Choose a slot from 0 to 8.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedGuardsRetainPrecedenceForMultiplyInvalidEdits(bool remote)
    {
        using var flow = new SessionFlow(players: 2);
        int player = remote ? 2 : 1;
        flow.Act(1, new("start"));
        Command Invalid() => flow.Command(player, new("store", 99, UnitId: 999));
        void Reject(Command command, string expected)
        {
            string before = JsonSerializer.Serialize(flow.State, WireJson.Options);
            CommandResult result = flow.Send(player, command);
            Assert.False(result.Accepted); Assert.Equal(expected, result.Message);
            Assert.Equal(before, JsonSerializer.Serialize(flow.State, WireJson.Options));
        }
        Reject(Invalid() with { ExpectedPhase = Phase.Combat, City = 999, Payment = (ConstructionPayment)99 }, "Stale match, phase or turn.");
        Reject(Invalid() with { City = 999, Payment = (ConstructionPayment)99 }, "You do not own this city.");
        Reject(Invalid() with { Payment = (ConstructionPayment)99 }, "Invalid construction payment choice.");
        flow.Act(1, new("pause"));
        Reject(Invalid(), "Match paused.");
        Reject(Invalid() with { Payment = ConstructionPayment.GoldRecovery }, "Invalid construction payment choice.");
        flow.Act(1, new("resume"));
        flow.Act(player, new("ready"));
        Reject(Invalid(), "Unready before editing.");
        flow.Act(player, new("unready"));
        Reject(Invalid(), "Choose a living owned roster unit.");
        if (remote)
        {
            Command saved = Invalid(); flow.Disconnect(player);
            string before = JsonSerializer.Serialize(flow.State, WireJson.Options);
            Assert.Null(flow.Send(player, saved));
            Assert.Equal(before, JsonSerializer.Serialize(flow.State, WireJson.Options));
        }
    }

    [Fact]
    public void PlacementScopesFieldAndExactHallGenerationBeforeSizeAndAnchors()
    {
        var board = new HexBoard(HexBoardDefinition.Default());
        var army = new ArmyConfiguration(new(), board);
        int cell = army.HomeCells[0];
        int[] anchors = board.Anchors.OrderBy(a => a.AnchorForward).ThenBy(a => a.Id).Select(a => a.Id).ToArray();
        UnitState Soldier(int id, int size, ArmyAssignment? assignment, int health = 100) => new(id, health)
        { Profile = new() { Size = size }, Assignment = assignment };
        UnitState[] roster =
        [
            Soldier(1, 4, new(-1, 0, cell, anchors[0])),
            Soldier(2, 6, new(3, 10, cell, anchors[0])),
            Soldier(3, 6, new(3, 11, cell, anchors[1])),
            Soldier(4, 6, new(-1, 0, cell, anchors[1]), health: 0),
            Soldier(5, 6, null),
            Soldier(6, 4, new(3, 10, 0, anchors[0])),
            Soldier(7, 6, new(4, 10, 0, anchors[1])),
            Soldier(8, 6, new(3, 11, 0, anchors[1]))
        ];
        Assert.Equal(new ArmyAssignment(-1, 0, cell, anchors[1]), army.Find(roster, 2, 2));
        Assert.Equal(new ArmyAssignment(3, 10, 0, anchors[1]), army.Find(roster, 2, 2, 3, 10, 6));
        Assert.Null(army.Find(roster, 2, 3, 3, 10, 6));
        Assert.Equal(new ArmyAssignment(3, 12, 0, anchors[0]), army.Find(roster, 2, 6, 3, 12, 6));
        Assert.Equal(new ArmyAssignment(-1, 0, cell, anchors[1]), army.Find(roster.Reverse(), 2, 2));
    }
}
