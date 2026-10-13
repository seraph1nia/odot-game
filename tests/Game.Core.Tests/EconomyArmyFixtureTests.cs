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
        for (int turn = 0; turn < 3; turn++)
        {
            foreach (var (player, action) in CampaignStrategy.RedistributionPreparation(flow.State, 1, 2)) Act(player, action);
            Ready();
        }
        flow.StepUntil(s => s.Phase != Phase.Combat && s.DyingBodies.Length == 0, 4000);
        Assert.Equal(Phase.Building, flow.State.Phase); Assert.Equal(2, flow.State.Wave);
        foreach (int player in new[] { 1, 2 })
        {
            Act(player, new("sell", 4)); Act(player, new("build", 4, Building.Stonecutter));
        }
        // Edit at preparation 3 before its END, never after final production.
        for (int turn = 0; turn < 2; turn++)
        {
            foreach (int player in new[] { 1, 2 })
                while (flow.State.Players[player - 1].Soldiers.Length < 6 && flow.State.Players[player - 1].Metal > 0) Act(player, new("recruit", 1));
            Ready();
        }
        foreach (int player in new[] { 1, 2 })
            while (flow.State.Players[player - 1].Soldiers.Length < 6) Act(player, new("recruit", 1));
        // Final END's timber is not spendable yet. Earn the missing equipment
        // wood through the actual paid mine refund, never a fourth edit/grant.
        Act(2, new("sell", 2));
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
        Command paid = flow.Command(2, recruit); CommandResult result = flow.Send(2, paid); Assert.True(result.Accepted, result.Message);
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
    // Hall slot6, real healing/returned identity, overflow and independent paid
    // upgrades are consolidated into PaidPreparationScheduleTests' earned Hall
    // witness. An arbitrary reserve-W5 policy deadline was not that invariant.
    private static object Member(UnitState u) => (u.Id, u.Health, u.Profile, u.Level, u.Experience, u.SquadId, u.FormationSlot, u.Assignment);

    [Theory]
    [InlineData(1UL)]
    [InlineData(123UL)]
    public void ExtraEmptyLandIsNotAnArmyOrAHealAndCannotBeOccupiedByTwoActors(ulong seed)
    {
        using Match match = PaidCampaignFixture.Opening(seed: seed); PaidCampaignFixture.Clear(match);
        CityState before = match.Snapshot().Players[0];
        PaidCampaignFixture.Apply(match, 1, new("buy-home")); CityState empty = match.Snapshot().Players[0];
        Assert.Equal(before.Soldiers.Select(Member), empty.Soldiers.Select(Member)); Assert.Equal(0, empty.Army!.Homes[2].Used);
        // A real clear may leave only one squad. Land itself created/healed
        // nobody above; explicitly pay for a second actor before testing its
        // rejection. Capacity3 guarantees two groups by four living members.
        for (int recruited = 0; match.Snapshot().Squads.Count(s => s.Owner == 1 && s.Members.Length > 0) < 2; recruited++)
        {
            Assert.True(recruited < 4, "Paid two-squad occupancy source did not converge.");
            PaidCampaignFixture.Apply(match, 1, new("recruit", 1));
        }
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
