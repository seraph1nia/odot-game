using Xunit;
using Xunit.Abstractions;

namespace Game.Core.Tests;

// Tier A preflight for the exact fixed source-defined input used over ENet.
// Existing paid stored-spectator proof cannot catch disappearance/noncontribution
// of active field squads. Two threshold controls, <=1000 steps, production
// session/codec/claims, no transport or earned/balance claim (estimated <2s).
public sealed class CombatReceivingEncounterTests(ITestOutputHelper output)
{
    // Tier A mapped020: authentic paid service/storage prefix, then declared
    // numerical enemy-roster/poses/Keep-health control through existing owner.
    // FULL fallen+servedstored+anotherKeep+stillCombat state, not separate passes.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FixedStoredVeteransReceiveActualKeepDestructionAndRetainOwnerDenialsAndNextWave(bool protectedKeep)
    {
        using var flow = new SessionFlow(players: 3, seed: 1);
        flow.Act(1, new("start"));
        Command Paid(int player, DevRunner.EconomyAction action)
        {
            MatchSnapshot before = flow.State;
            Command command = flow.Act(player, action);
            PaidCampaignFixture.AssertPaidTransition(before, flow.State, player, action);
            return command;
        }
        Command purchase = PaidStoredVeteranHistoryTests.PreparePaidPrefix(flow, Paid, observe: s =>
        {
            // Cheapest discriminant for real ENet ally's last delivered991:
            // validate exact production payload/config around that boundary,
            // not a larger transport deadline or assumed native failure.
            if (!protectedKeep && s.Wave == 2 && s.Tick is >= 991 and <= 994)
            {
                SnapshotCompatibility.RequireSharedArena(s);
                Assert.Equal(Json(s), Json(SessionFlow.RoundTrip(s)));
                output.WriteLine($"Mid-prefix codec tick{s.Tick}: owners={string.Join(',', s.Players.Select(c => c.Soldiers.Length))} accepted.");
            }
        });
        flow.StepUntil(s => s.DyingBodies.Length == 0);
        for (int gate = 0; gate < 3; gate++)
            foreach (int player in new[] { 1, 2, 3 }) Paid(player, new("ready"));
        MatchSnapshot authentic = SessionFlow.RoundTrip(flow.State);
        string name = protectedKeep ? "protected-stored" : "fallen-stored";
        Assert.True(flow.Session.ConfigureReceivingEncounter(name));
        MatchSnapshot input = SessionFlow.RoundTrip(flow.State);
        Assert.Equal(Phase.Combat, input.Phase); Assert.Equal(3, input.Wave); Assert.Single(input.Enemies);
        Assert.Equal(Members(authentic.Players[2]), Members(input.Players[2]));
        Assert.Equal(authentic.Players[2].Resources, input.Players[2].Resources);
        Assert.All(input.Players[2].Soldiers, u => Assert.True(u.Experience >= 1 && u.Assignment is { Stored: true } && !u.Participating));
        if (!protectedKeep)
        {
            MatchSnapshot lost = flow.StepUntil(s => s.Players[2].Eliminated || s.Tick >= input.Tick + 100, 100);
            output.WriteLine($"Controlled stored INPUT={Json(input)} OBSERVED={Json(lost)}");
            Assert.True(lost.Players[2].Eliminated, "Actual controlled enemy did not destroy intended Keep in fixed observation.");
            Assert.Equal(Phase.Combat, lost.Phase); Assert.True(lost.Players.Take(2).All(c => !c.Eliminated));
            Assert.NotEmpty(lost.Enemies);
            Assert.Contains(lost.Squads, s => s.Owner != 3 && s.Faction == Faction.Adventurers && s.ParticipatingMembers.Length > 0);
            Assert.Equal(Members(input.Players[2]), Members(lost.Players[2]));
            Assert.Contains(lost.CombatEvents, e => e.Type == CombatEventType.StructureDestroyed && e.Structure is { Owner: 3, Kind: ArenaStructureKind.Keep });
            Assert.All(lost.Structures.Where(s => s.Owner == 3), s => Assert.False(s.Active));
            flow.Act(3, new("pause"));
            MatchSnapshot frozen = SessionFlow.RoundTrip(flow.State);
            Assert.Equal(default, frozen.Players[2].ProductionIncome!.Value);
            foreach (var (player, action) in new[] { ("ready", new DevRunner.EconomyAction("ready")),
                ("build", new DevRunner.EconomyAction("build", 2, Building.Farm)),
                ("recruit", new DevRunner.EconomyAction("recruit", 2)),
                ("retire", new DevRunner.EconomyAction("retire", UnitId: input.Players[2].Soldiers[0].Id)),
                ("research", new DevRunner.EconomyAction("research-tech", Technology: TechnologyId.MeleeFoundation)) })
            {
                Command command = flow.Command(3, action);
                CommandResult refused = flow.Send(3, command);
                Assert.False(refused.Accepted, player); Assert.Equal(Json(frozen), Json(SessionFlow.RoundTrip(flow.State)));
                Assert.Equal(refused, flow.Send(3, command));
            }
            Command foreign = flow.Command(3, new("ready")) with { City = 1 };
            Assert.False(flow.Send(3, foreign).Accepted); Assert.Equal(Json(frozen), Json(flow.State));
            flow.Disconnect(3); flow.Rebind(3, 9);
            Assert.Equal(SessionFlow.Gameplay(frozen), SessionFlow.Gameplay(SessionFlow.RoundTrip(flow.State)));
            Assert.True(flow.Send(3, purchase).Accepted);
            Assert.Equal(SessionFlow.Gameplay(frozen), SessionFlow.Gameplay(flow.State));
            flow.Act(3, new("resume"));
        }
        MatchSnapshot clear = flow.StepUntil(s => s.Phase != Phase.Combat && s.DyingBodies.Length == 0, 1000);
        clear = SessionFlow.RoundTrip(clear);
        Assert.Equal(Phase.Building, clear.Phase); Assert.Equal(4, clear.Wave);
        Assert.Equal(!protectedKeep, clear.Players[2].Eliminated);
        Assert.Equal(Members(input.Players[2]), Members(clear.Players[2]));
        if (protectedKeep) return; // Disconfirming threshold control, not fallen acceptance.
        CityState fallen = clear.Players[2]; UnitState[] manifest = clear.NextWave;
        for (int gate = 0; gate < 3; gate++)
        {
            foreach (int player in new[] { 1, 2 }) Paid(player, new("ready"));
            CityState current = SessionFlow.RoundTrip(flow.State).Players[2];
            Assert.Equal(fallen.Resources, current.Resources); Assert.Equal(fallen.Research, current.Research);
            Assert.Equal(Members(fallen), Members(current)); Assert.Equal(Json(fallen.LastUpkeep), Json(current.LastUpkeep));
            Assert.False(current.Ready);
        }
        MatchSnapshot next = SessionFlow.RoundTrip(flow.State);
        Assert.Equal(Phase.Combat, next.Phase); Assert.Equal(4, next.Wave); Assert.Equal(manifest.Length, next.Enemies.Length);
        Assert.DoesNotContain(next.Enemies, u => u.Destination == 3); Assert.Contains(next.Enemies, u => u.Origin == 3);
        Assert.Equal(manifest.Select(u => (u.Type, u.Level, u.Rank, u.Origin, u.Destination, u.Profile)),
            next.Enemies.Select(u => (u.Type, u.Level, u.Rank, u.Origin, u.Destination, u.Profile)));
        Assert.All(next.Players[2].Soldiers, u =>
        {
            Assert.False(u.Participating); Assert.Equal(UnitLifecycle.Stored, u.Hex!.Lifecycle); Assert.Equal(default, u.Hex.Position);
            Assert.DoesNotContain(next.Reservations.Positions, p => p.UnitId == u.SquadId);
        });
        CombatReservations claims = CombatReservations.Reconstruct(next.Arena!, next.Squads);
        Assert.Equal(next.Reservations.Positions, claims.Positions); Assert.Equal(next.Reservations.Transit, claims.Transit);
    }
    private static string Json<T>(T state) => System.Text.Json.JsonSerializer.Serialize(state, WireJson.Options);
    private static string Members(CityState city) => Json(city.Soldiers.Select(u => new
    { u.Id, u.Health, u.Profile, u.Level, u.Experience, u.SquadId, u.FormationSlot, u.Assignment, u.Statuses }));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FixedEncounterRetainsIdentifiedFieldMembersAndActuallyReceivesTheirLaterImpact(bool protectedKeep)
    {
        using var flow = new SessionFlow();
        flow.Act(1, new("start"));
        for (int gate = 0; gate < 3; gate++)
        {
            foreach (var (owner, action) in DevRunner.CampaignStrategy.RedistributionPreparation(flow.State, 1, 2)) flow.Act(owner, action);
            foreach (int owner in new[] { 1, 2 })
            {
                flow.Act(owner, new("ready"));
                flow.Session.ConfigureReceivingEncounter(protectedKeep);
            }
        }
        MatchSnapshot initial = SessionFlow.RoundTrip(flow.State);
        Assert.Equal(Phase.Combat, initial.Phase); Assert.Equal(0, initial.Tick);
        Assert.Single(initial.Enemies);
        int injured = Assert.Single(initial.Players[1].Soldiers, u => u.Health == HealthPoints.FromWhole(33)).Id;
        int[] fields = initial.Players[1].Soldiers.Select(u => u.Id).ToArray();
        Assert.Equal(6, fields.Length);
        long loss = 0;
        if (!protectedKeep)
        {
            MatchSnapshot fallen = flow.StepUntil(s => s.Players[1].Eliminated, 30);
            fallen = SessionFlow.RoundTrip(fallen);
            Assert.Equal(Phase.Combat, fallen.Phase);
            loss = Assert.Single(fallen.CombatEvents, e => e.Type == CombatEventType.StructureDestroyed && e.Structure is { Owner: 2, Kind: ArenaStructureKind.Keep }).Tick;
            Assert.All(fallen.Players[1].Soldiers, u => { Assert.True(u.Participating); Assert.True(u.Health > 0); });
            Assert.Equal(HealthPoints.FromWhole(33), fallen.Players[1].Soldiers.Single(u => u.Id == injured).Health);
            Assert.All(fallen.Structures.Where(s => s.Owner == 2), s => Assert.False(s.Active));
            Assert.All(fallen.Players[1].Towers, t => Assert.False(t.PendingImpact));
            flow.Act(2, new("pause")); MatchSnapshot frozen = flow.State;
            flow.Disconnect(2); flow.Rebind(2, 9);
            Assert.Equal(SessionFlow.Gameplay(frozen), SessionFlow.Gameplay(SessionFlow.RoundTrip(flow.State)));
            flow.Act(2, new("resume"));
        }
        MatchSnapshot receipt = flow.StepUntil(s => s.CombatEvents.Any(e => e.Type == CombatEventType.Impact && e.Landed
            && e.Unit?.Owner == 2 && fields.Contains(e.Unit.Id) && e.Damage > 0 && e.Victims.Length > 0), 1000);
        receipt = SessionFlow.RoundTrip(receipt);
        Assert.Equal(!protectedKeep, receipt.Players[1].Eliminated);
        Assert.Contains(receipt.CombatEvents, e => e.Type == CombatEventType.Impact && e.Landed && e.Unit?.Owner == 2 && e.Victims.Length > 0);
        if (!protectedKeep)
        {
            Assert.Contains(receipt.CombatEvents, e => e.Tick > loss && e.Type == CombatEventType.Impact && e.Landed && e.Unit?.Owner == 2);
            Assert.DoesNotContain(receipt.CombatEvents, e => e.Tick > loss && e.Type == CombatEventType.Impact && e.Tower?.City == 2);
        }
        CombatReservations claims = CombatReservations.Reconstruct(receipt.Arena!, receipt.Squads);
        Assert.Equal(receipt.Reservations.Positions, claims.Positions); Assert.Equal(receipt.Reservations.Transit, claims.Transit);
    }
}
