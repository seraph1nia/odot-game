using System.Text.Json;
using DevRunner;
using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Tier A replacement of recovered individual-combat transcripts: earned command
// inputs and explicit physical/individual outcomes, not refreshed tick/HP goldens.
// Price, retry, authority and veteran negatives survive independently of victory.
public sealed class EconomyArmyFixtureTests(ITestOutputHelper output)
{
    [Fact]
    public void PaidHomogeneousHomesRefuseSupportUntilARealPlotIsBoughtAndKeepLedgerRetries()
    {
        using var flow = new SessionFlow(seed: 1);
        void Act(int player, EconomyAction action) => flow.Act(player, action);
        void Ready() { Act(1, new("ready")); Act(2, new("ready")); }
        Act(1, new("start"));
        foreach (int player in new[] { 1, 2 })
        {
            Act(player, new("build", 0, Building.Farm)); Act(player, new("build", 1, Building.Barracks));
            Act(player, new("build", 2, Building.MetalMine)); Act(player, new("build", 3, Building.Lumbermill, Payment: ConstructionPayment.GoldRecovery));
        }
        for (int turn = 0; turn < 4; turn++)
        {
            foreach (int player in new[] { 1, 2 })
            {
                if (turn > 0) { Act(player, new("recruit", 1)); Act(player, new("recruit", 1)); }
                if (turn == 2) Act(player, new("build", 4, Building.Stonecutter));
            }
            Ready();
        }
        flow.StepUntil(s => s.Phase != Phase.Combat && s.DyingBodies.Length == 0, 4000);
        Assert.Equal(Phase.Building, flow.State.Phase); Assert.Equal(2, flow.State.Wave);
        for (int turn = 0; turn < 3; turn++)
        {
            foreach (int player in new[] { 1, 2 })
                while (flow.State.Players[player - 1].Soldiers.Length < 6 && flow.State.Players[player - 1].Metal > 0) Act(player, new("recruit", 1));
            Ready();
        }
        foreach (int player in new[] { 1, 2 })
            while (flow.State.Players[player - 1].Soldiers.Length < 6) Act(player, new("recruit", 1));
        Act(2, new("buy-plot", 5)); Act(2, new("build", 5, Building.ArcheryRange));
        CityState before = flow.State.Players[1];
        Assert.Equal([3, 3], before.Army!.Homes.Where(h => h.Purchased).Select(h => h.Used));
        var recruit = new EconomyAction("recruit", 5, Unit: UnitType.Crossbowman);
        Command blocked = flow.Command(2, recruit); string frozen = JsonSerializer.Serialize(flow.State, WireJson.Options);
        CommandResult refusal = flow.Send(2, blocked); Assert.False(refusal.Accepted);
        Assert.Equal(frozen, JsonSerializer.Serialize(flow.State, WireJson.Options));
        Assert.Equal(refusal, flow.Send(2, blocked));
        int price = before.Army.HomePrices[before.Army.PurchasedHomes - 2];
        Command purchase = flow.Act(2, new("buy-home")); CityState room = flow.State.Players[1];
        Assert.True(before.Resources.TryPay(new(price), out ResourceCost bought)); Assert.Equal(bought, room.Resources);
        Assert.Equal(before.Soldiers.Select(Member), room.Soldiers.Select(Member));
        frozen = JsonSerializer.Serialize(flow.State, WireJson.Options);
        Assert.True(flow.Send(2, purchase).Accepted); Assert.Equal(refusal, flow.Send(2, blocked));
        Assert.Equal(frozen, JsonSerializer.Serialize(flow.State, WireJson.Options));
        Command paid = flow.Command(2, recruit); CommandResult result = flow.Send(2, paid); Assert.True(result.Accepted);
        CityState after = flow.State.Players[1];
        RecruitmentQuote quote = room.RecruitmentQuotes.Single(q => q.Type == UnitType.Crossbowman && q.Level == 1);
        Assert.True(room.Resources.TryPay(quote.Cost, out ResourceCost equipped)); Assert.Equal(equipped, after.Resources);
        Assert.Equal(before.Soldiers.Select(Member), after.Soldiers.Take(6).Select(Member));
        UnitState member = after.Soldiers[^1]; Assert.Equal(UnitType.Crossbowman, member.Type);
        Assert.Equal(0, member.Experience); Assert.Equal(member.Profile.Health, member.Health); Assert.Equal(2, member.Assignment!.Tile);
        PaidCampaignFixture.Legal(flow.State);
        frozen = SessionFlow.Gameplay(flow.State); flow.Disconnect(2); flow.Rebind(2, 9);
        Assert.Equal(result, flow.Send(2, paid)); Assert.Equal(frozen, SessionFlow.Gameplay(flow.State));
        output.WriteLine($"PROVISIONAL current-phase paid room={price}, recruitment={quote.Cost}, veteran count={before.Soldiers.Length}");
    }
    [Fact]
    public void EarnedHallIndividualSlotsBeyondSixNeverBecomeFieldDeploymentHomes()
    {
        // Small paid regression of the reserve baseline's genuine W5 store
        // crash: Hall tile6 is legal, but not a field home. No seeded injury/stock.
        PaidCampaignFixture.Result result = PaidCampaignFixture.Run(output, "reserve", 1, 123, stopAfterWave: 5);
        Assert.Equal(Phase.Building, result.Outcome); Assert.Equal(6, result.ReachedWave);
        Assert.Contains(result.Final.Players[0].Soldiers, u => u.Assignment is { Stored: true, Tile: >= 6 });
        Assert.True(result.HealedHealth > 0); Assert.True(result.RecoveryInvestment.Gold > 0);
    }
    private static object Member(UnitState u) => (u.Id, u.Health, u.Profile, u.Level, u.Experience, u.SquadId, u.FormationSlot, u.Assignment);

    [Fact]
    public void EarnedWoundsStoredAtAPaidHallHealOnlyOnRealProductionWithoutChangingEquipment()
    {
        using Match match = PaidCampaignFixture.Opening(seed: 1); PaidCampaignFixture.Clear(match);
        CityState city = match.Snapshot().Players[0];
        UnitState veteran = city.Soldiers.Where(u => u.Health < u.Profile.Health && u.RecoveryEligible).OrderBy(u => u.Health).First();
        PaidCampaignFixture.Apply(match, 1, new("buy-plot", 5));
        // The clear has only one earned stone: a Hall is still unaffordable.
        // Preserve the negative, then earn its missing material through END.
        string denied = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1000, "build", 1, 5, Building.TownHall)).Accepted);
        Assert.Equal(denied, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
        PaidCampaignFixture.Apply(match, 1, new("ready")); PaidCampaignFixture.Apply(match, 2, new("ready"));
        PaidCampaignFixture.Apply(match, 1, new("build", 5, Building.TownHall));
        ResourceCost beforeStore = match.Players[1].Resources;
        PaidCampaignFixture.Apply(match, 1, new("store", 5, UnitId: veteran.Id));
        Assert.Equal(beforeStore, match.Players[1].Resources);
        UnitState stored = match.Players[1].Soldiers.Single(u => u.Id == veteran.Id);
        Assert.Equal(veteran.Health, stored.Health); Assert.Equal(veteran.Level, stored.Level); Assert.Equal(veteran.Profile, stored.Profile);
        int production = match.ProductionCount;
        PaidCampaignFixture.Apply(match, 1, new("ready")); Assert.Equal(stored.Health, match.Players[1].Soldiers.Single(u => u.Id == veteran.Id).Health);
        PaidCampaignFixture.Apply(match, 2, new("ready"));
        UnitState healed = match.Players[1].Soldiers.Single(u => u.Id == veteran.Id);
        Assert.Equal(production + 1, match.ProductionCount);
        int percent = match.Snapshot().Players[0].Army!.Halls.Single(h => h.Slot == 5).HealingPercent;
        Assert.Equal(ArmyConfiguration.HealedHealth(stored.Health, stored.Profile.Health, percent), healed.Health);
        Assert.Equal(stored.Level, healed.Level); Assert.Equal(stored.Experience, healed.Experience);
        PaidCampaignFixture.Apply(match, 1, new("send", 5, UnitId: veteran.Id));
        UnitState returned = match.Players[1].Soldiers.Single(u => u.Id == veteran.Id);
        Assert.Equal(healed.Health, returned.Health); Assert.Equal(healed.Level, returned.Level); Assert.Equal(healed.Profile, returned.Profile);
        output.WriteLine($"PROVISIONAL earned injury {stored.Health}/{stored.Profile.Health}, actual Hall healing={healed.Health - stored.Health}, purchased Hall={match.Players[1].Slots[5].Investment}");
    }

    [Theory]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void ExtraEmptyLandIsNotAnArmyOrAHealAndCannotBeOccupiedByTwoActors(ulong seed)
    {
        using Match match = PaidCampaignFixture.Opening(seed: seed); PaidCampaignFixture.Clear(match);
        CityState before = match.Snapshot().Players[0];
        PaidCampaignFixture.Apply(match, 1, new("buy-home")); CityState empty = match.Snapshot().Players[0];
        Assert.Equal(before.Soldiers.Select(Member), empty.Soldiers.Select(Member)); Assert.Equal(0, empty.Army!.Homes[2].Used);
        var arena = new SharedArena(match.Players.Keys);
        SquadState group = match.Snapshot().Squads.First(s => s.Owner == 1 && s.Members.Length > 0);
        Command deploy = Command.FromSnapshot(match.Snapshot(), 1000, "deploy-squad", 1, squadId: group.Id, cell: arena.Deployment(1, 2));
        Assert.True(match.Apply(1, deploy).Accepted); PaidCampaignFixture.Legal(match.Snapshot());
        SquadState other = match.Snapshot().Squads.First(s => s.Owner == 1 && s.Id != group.Id && s.Members.Length > 0);
        string frozen = JsonSerializer.Serialize(match.Snapshot(), WireJson.Options);
        Assert.False(match.Apply(1, Command.FromSnapshot(match.Snapshot(), 1001, "deploy-squad", 1, squadId: other.Id, cell: arena.Deployment(1, 2))).Accepted);
        Assert.Equal(frozen, JsonSerializer.Serialize(match.Snapshot(), WireJson.Options));
    }
}
