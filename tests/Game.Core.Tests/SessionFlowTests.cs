using System.Text.Json;
using DevRunner;
using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class SessionFlowTests
{
    [Theory]
    [InlineData(AuthorityPolicy.PlayingHost)]
    [InlineData(AuthorityPolicy.Dedicated)]
    public void DisconnectAcrossUpkeepRebindAndRecruitmentRetryRetainOrdinaryState(AuthorityPolicy policy)
    {
        using var flow = new SessionFlow(policy);
        flow.Act(1, new("start"));
        Command? recruitment = null;
        for (int turn = 0; turn < 2; turn++)
            foreach (int player in new[] { 1, 2 })
            {
                Command? recruited = flow.Invest(player);
                if (player == 2 && recruited is not null) recruitment = recruited;
                flow.Act(player, new("ready"));
            }
        foreach (int player in new[] { 1, 2 })
        {
            Command? recruited = flow.Invest(player);
            if (player == 2 && recruited is not null) recruitment = recruited;
        }
        Assert.NotNull(recruitment);
        CityState before = flow.State.Players.Single(c => c.Id == 2);
        flow.Disconnect(2);
        flow.Act(1, new("ready"));
        Assert.Equal(Phase.Combat, flow.Session.Phase);
        CityState paid = flow.State.Players.Single(c => c.Id == 2);
        Assert.NotNull(paid.LastUpkeep);
        Assert.Equal(before.Food + before.ProductionIncome!.Value.Food - paid.LastUpkeep.Paid, paid.Food);
        Assert.Equal(before.FoodForecast!.Funded, paid.LastUpkeep.Funded);
        Assert.Equal(before.FoodForecast.Paid, paid.LastUpkeep.Paid);
        Assert.Equal(before.Soldiers.Select(u => u.Id), paid.Soldiers.Select(u => u.Id));
        flow.Act(1, new("pause"));
        string frozen = SessionFlow.Gameplay(flow.State);
        for (int step = 0; step < 100; step++) flow.Session.Step();
        Assert.Equal(frozen, SessionFlow.Gameplay(flow.State));
        flow.Rebind(2, 99);
        Assert.Equal(frozen, SessionFlow.Gameplay(SessionFlow.RoundTrip(flow.State)));
        Assert.True(flow.Send(2, recruitment).Accepted);
        Assert.Equal(frozen, SessionFlow.Gameplay(flow.State));
        Assert.False(flow.Send(2, flow.Command(2, new("build", 0, Building.Mine))).Accepted);
        Assert.Equal(frozen, SessionFlow.Gameplay(flow.State));
        flow.Act(2, new("resume"));
        flow.StepUntil(s => s.Phase != Phase.Combat);
        Assert.Equal(Phase.Building, flow.Session.Phase);
        Assert.Equal(2, flow.State.Wave);
        SessionFlow.RoundTrip(flow.State);
    }

    [Fact]
    public void SoldBuildingRetryCannotPayTwiceOrMutateReplacement()
    {
        using var flow = new SessionFlow();
        flow.Act(1, new("start"));
        flow.Act(2, new("build", 0, Building.Farm));
        long generation = flow.State.Players[1].Slots[0].Generation;
        Command sale = flow.Act(2, new("sell", 0));
        flow.Act(2, new("build", 0, Building.Lumbermill));
        flow.Disconnect(2); flow.Rebind(2, 50);
        string before = SessionFlow.Gameplay(flow.State);
        Assert.True(flow.Send(2, sale).Accepted);
        Assert.Equal(before, SessionFlow.Gameplay(flow.State));
        Command stale = flow.Command(2, new("sell", 0)) with { ExpectedGeneration = generation };
        Assert.False(flow.Send(2, stale).Accepted);
        Assert.Equal(before, SessionFlow.Gameplay(SessionFlow.RoundTrip(flow.State)));
        Assert.True(flow.State.Players[1].Slots[0].Generation > generation);
    }

    [Fact]
    public void SerializedPaidThreeCityFallAndTeamClearPreserveVeteransReceiptsAndFutureAllocation()
    {
        using var flow = new SessionFlow(players: 3, seed: 8);
        flow.Act(1, new("start"));
        foreach (int player in new[] { 1, 2 })
        {
            flow.Act(player, new("build", 0, Building.Farm));
            flow.Act(player, new("build", 2, Building.MetalMine));
            flow.Act(player, new("build", 4, Building.MetalMine));
            flow.Act(player, new("build", 3, Building.Lumbermill, Payment: ConstructionPayment.GoldRecovery));
        }
        // This isolated transfer/redistribution witness requires six paid units
        // against three cities' pressure. It is not the ordinary opening or the
        // single-mine economic control: all extra material and timber are paid.
        for (int turn = 0; turn < 3; turn++)
        {
            foreach (int player in new[] { 1, 2 })
            {
                if (turn == 2)
                {
                    flow.Act(player, new("build", 1, Building.Barracks));
                    for (int recruit = 0; recruit < 6; recruit++) flow.Act(player, new("recruit", 1));
                }
            }
            foreach (int player in new[] { 1, 2, 3 }) flow.Act(player, new("ready"));
        }
        MatchSnapshot initial = flow.State;
        Assert.All(initial.Players.Where(p => p.Id != 3), p =>
        {
            Assert.Equal(6, p.Soldiers.Length); Assert.Equal(6, p.LastUpkeep!.Paid);
            Assert.Equal(p.Soldiers.Select(u => u.Id), p.LastUpkeep.Funded);
            Assert.Equal(12, p.Metal); Assert.Equal(9, p.Food);
        });
        // Tier A: actual new paid timing changes recruitment/actor identities;
        // this fixed six-member opening now sacrifices the uninvested Keep.
        // Preserve the real loss, received redirection/identity and team clear,
        // not an all-Keeps rescue claim or another chosen seed/balance change.
        Assert.Empty(initial.Players[2].Soldiers);
        MatchSnapshot fallen = flow.StepUntil(s => s.Players[2].Eliminated, 3000);
        UnitState[] incoming = fallen.Enemies.Where(u => u.Origin == 3).ToArray();
        Assert.NotEmpty(incoming);
        Assert.All(incoming, u =>
        {
            UnitState original = initial.Enemies.Single(e => e.Id == u.Id);
            Assert.NotEqual(3, u.Destination); Assert.Equal(original.Profile, u.Profile);
            Assert.Equal(original.SquadId, u.SquadId); Assert.Equal(original.FormationSlot, u.FormationSlot);
            Assert.InRange(u.Health, 1, original.Health);
        });
        SessionFlow.RoundTrip(fallen);
        MatchSnapshot clear = flow.StepUntil(s => s.Phase != Phase.Combat, 3000);
        Assert.Equal(Phase.Building, clear.Phase); Assert.Equal(2, clear.Wave);
        Assert.All(clear.Players.Take(2), city => Assert.False(city.Eliminated));
        Assert.True(clear.Players[2].Eliminated);
        Assert.NotEmpty(clear.Players.Take(2).SelectMany(c => c.Soldiers));
        foreach (CityState city in clear.Players)
        {
            Assert.Equal(JsonSerializer.Serialize(initial.Players.Single(c => c.Id == city.Id).LastUpkeep, WireJson.Options), JsonSerializer.Serialize(city.LastUpkeep, WireJson.Options));
            foreach (UnitState member in city.Soldiers)
            {
                UnitState old = initial.Players.Single(c => c.Id == city.Id).Soldiers.Single(u => u.Id == member.Id);
                Assert.Equal(old.Profile, member.Profile); Assert.Equal(old.SquadId, member.SquadId);
                Assert.Equal(old.FormationSlot, member.FormationSlot); Assert.Equal(old.Assignment, member.Assignment);
                Assert.True(member.Health <= old.Health); Assert.Equal(old.Experience + 1, member.Experience);
            }
        }
        MatchSnapshot received = SessionFlow.RoundTrip(clear);
        SnapshotCompatibility.RequireSharedArena(received);
        CombatReservations claims = CombatReservations.Reconstruct(received.Arena!, received.Squads);
        Assert.Equal(received.Reservations.Positions, claims.Positions); Assert.Equal(received.Reservations.Transit, claims.Transit);
        flow.Disconnect(3); flow.Rebind(3, 77);
        string before = SessionFlow.Gameplay(flow.State);
        Assert.False(flow.Send(3, flow.Command(3, new("build", 0, Building.Farm)) with { City = 1 }).Accepted);
        Assert.False(flow.Send(3, flow.Command(3, new("build", 0, Building.Farm))).Accepted);
        Assert.Equal(before, SessionFlow.Gameplay(flow.State));
        flow.StepUntil(s => s.DyingBodies.Length == 0, 120);
        for (int turn = 0; turn < 3; turn++)
        {
            foreach (int player in new[] { 1, 2 })
                while (CampaignStrategy.ReinforcementInvestment(flow.State, player, player == 1) is EconomyAction decision) flow.Act(player, decision);
            foreach (int player in new[] { 1, 2 }) flow.Act(player, new("ready"));
        }
        flow.StepUntil(s => s.Phase == Phase.Combat, 120);
        Assert.Equal(12, flow.State.Enemies.Length);
        Assert.Equal(4, flow.State.Enemies.Count(u => u.Origin == 3));
        Assert.All(flow.State.Players.Take(2), c => Assert.Equal(6, flow.State.Enemies.Count(u => u.Destination == c.Id)));
        Assert.Equal(clear.Players[2].Resources, flow.State.Players[2].Resources);
        Assert.Equal(clear.Players[2].Research, flow.State.Players[2].Research);
        SessionFlow.RoundTrip(flow.State);
    }

    [Fact]
    public void StepBoundAndSeedReproductionAreExplicit()
    {
        using var first = new SessionFlow(); using var second = new SessionFlow();
        foreach (SessionFlow flow in new[] { first, second })
        {
            flow.Act(1, new("start")); flow.Prepare();
            Assert.Throws<InvalidOperationException>(() => flow.StepUntil(_ => false, 2));
        }
        Assert.Equal(SessionFlow.Gameplay(first.State with { MatchId = "same" }), SessionFlow.Gameplay(second.State with { MatchId = "same" }));
    }
}
