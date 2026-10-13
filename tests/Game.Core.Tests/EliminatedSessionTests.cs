using System.Text.Json;
using DevRunner;
using Xunit;

namespace Game.Core.Tests;

// Tier A authority/codec/retry negative; real transport belongs to the existing
// eliminated-resume ENet scenario. Default paid seed1, at most two early waves,
// 4000 combat steps/wave and 100 decisions/gate. No new simulator or health stock
// grant. The fallen owner's stored survivor is NOT an active-fighter witness.
public sealed class EliminatedSessionTests
{
    [Fact]
    public void PaidFallenOwnerRetainsStoredMemberAndLedgerWithoutFutureProductionOrParticipation()
    {
        using var flow = new SessionFlow(players: 3, seed: 1);
        flow.Act(1, new("start"));
        Command? purchase = null;
        for (int wave = 1; wave <= 2; wave++)
        {
            flow.StepUntil(s => s.DyingBodies.Length == 0);
            for (int gate = 0; gate < 4; gate++)
                foreach (CityState city in flow.State.Players.Where(c => !c.Eliminated))
                {
                    if (city.Id != 3) flow.Invest(city.Id);
                    else for (int action = 0; CampaignStrategy.SpectatorReserveOpening(flow.State, 3) is EconomyAction plan; action++)
                    {
                        Assert.True(action < 100);
                        Command command = flow.Act(3, plan);
                        if (plan.Action == "recruit") purchase = command;
                    }
                    flow.Act(city.Id, new("ready"));
                }
            if (wave == 1) flow.StepUntil(s => s.Phase != Phase.Combat, 4000);
            else flow.StepUntil(s => s.Players[2].Eliminated, 4000);
        }
        Assert.Equal(Phase.Combat, flow.State.Phase);
        Assert.NotNull(purchase);
        flow.Act(1, new("pause"));
        MatchSnapshot frozen = Receive(flow.State);
        CityState fallen = frozen.Players[2]; UnitState retained = Assert.Single(fallen.Soldiers);
        Assert.True(fallen.Eliminated); Assert.False(retained.Participating); Assert.True(retained.Assignment!.Stored);
        Assert.Equal(default, retained.Hex!.Position); Assert.DoesNotContain(frozen.Reservations.Positions, p => p.UnitId == retained.SquadId);
        Assert.Equal(default, fallen.ProductionIncome!.Value);
        foreach (string action in new[] { "ready", "recruit", "build", "retire" })
        {
            Command command = flow.Command(3, new(action, 2, Building.Farm, UnitId: retained.Id));
            CommandResult denied = flow.Send(3, command); Assert.False(denied.Accepted);
            Assert.Equal(Json(frozen), Json(Receive(flow.State)));
            Assert.Equal(denied, flow.Send(3, command));
        }
        flow.Disconnect(3); flow.Rebind(3, 9);
        Assert.Equal(SessionFlow.Gameplay(frozen), SessionFlow.Gameplay(Receive(flow.State)));
        Assert.True(flow.Send(3, purchase).Accepted); // Original paid receipt, not a new recruit.
        Assert.Equal(SessionFlow.Gameplay(frozen), SessionFlow.Gameplay(Receive(flow.State)));
        flow.Act(1, new("resume"));
        flow.StepUntil(s => s.Phase != Phase.Combat && s.DyingBodies.Length == 0, 4000);
        Assert.Equal(Phase.Building, flow.State.Phase); Assert.Equal(3, flow.State.Wave);
        CityState beforeIncome = flow.State.Players[2];
        foreach (int player in new[] { 1, 2 }) { flow.Invest(player); flow.Act(player, new("ready")); }
        CityState afterIncome = Receive(flow.State).Players[2];
        Assert.Equal(beforeIncome.Resources, afterIncome.Resources); Assert.Equal(beforeIncome.Research, afterIncome.Research);
        Assert.Equal(Json(beforeIncome.Soldiers), Json(afterIncome.Soldiers)); Assert.False(afterIncome.Ready);
        for (int gate = 0; gate < 3; gate++)
            foreach (int player in new[] { 1, 2 }) { flow.Invest(player); flow.Act(player, new("ready")); }
        MatchSnapshot next = Receive(flow.State);
        Assert.Equal(Phase.Combat, next.Phase); Assert.Equal(3, next.Wave);
        Assert.Equal(15, next.Enemies.Length); Assert.DoesNotContain(next.Enemies, u => u.Destination == 3);
        Assert.Contains(next.Enemies, u => u.Origin == 3);
        Assert.Equal(afterIncome.Soldiers.Select(Member), next.Players[2].Soldiers.Select(Member));
        Assert.All(next.Players[2].Soldiers, u => { Assert.False(u.Participating); Assert.Equal(UnitLifecycle.Stored, u.Hex!.Lifecycle); Assert.Equal(default, u.Hex.Position); });
        Assert.DoesNotContain(next.Reservations.Positions, p => p.UnitId == retained.SquadId);
        Assert.Equal(Json(beforeIncome.LastUpkeep), Json(next.Players[2].LastUpkeep));
    }
    private static object Member(UnitState member) => (member.Id, member.Health, member.Level, member.Profile, member.Experience, member.SquadId, member.FormationSlot, member.Assignment, member.Statuses);
    private static string Json<T>(T state) => JsonSerializer.Serialize(state, WireJson.Options);
    private static MatchSnapshot Receive(MatchSnapshot state)
    {
        MatchSnapshot received = Game.SnapshotPayload.DecodeForReceiving(Game.SnapshotPayload.Encode(state));
        Assert.Equal(Json(state), Json(received));
        CombatReservations claims = CombatReservations.Reconstruct(received.Arena!, received.Squads);
        Assert.Equal(received.Reservations.Positions, claims.Positions); Assert.Equal(received.Reservations.Transit, claims.Transit);
        return received;
    }
}
