using Xunit;

namespace Game.Core.Tests;

// Tier A preflight for the exact fixed source-defined input used over ENet.
// Existing paid stored-spectator proof cannot catch disappearance/noncontribution
// of active field squads. Two threshold controls, <=1000 steps, production
// session/codec/claims, no transport or earned/balance claim (estimated <2s).
public sealed class CombatReceivingEncounterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FixedEncounterRetainsIdentifiedFieldMembersAndActuallyReceivesTheirLaterImpact(bool protectedKeep)
    {
        using var flow = new SessionFlow();
        flow.Act(1, new("start"));
        foreach (int owner in new[] { 1, 2 })
        {
            flow.Act(owner, new("build", 0, Building.Farm));
            flow.Act(owner, new("build", 1, Building.Barracks));
            flow.Act(owner, new("build", 2, Building.MetalMine));
            flow.Act(owner, new("build", 3, Building.Lumbermill, Payment: ConstructionPayment.GoldRecovery));
        }
        for (int gate = 0; gate < 4; gate++)
            foreach (int owner in new[] { 1, 2 })
            {
                if (gate > 0) for (int member = 0; member < 2; member++) flow.Act(owner, new("recruit", 1));
                if (gate == 2) flow.Act(owner, new("build", 4, Building.Stonecutter));
                flow.Act(owner, new("ready"));
                flow.Session.ConfigureReceivingEncounter(protectedKeep);
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
