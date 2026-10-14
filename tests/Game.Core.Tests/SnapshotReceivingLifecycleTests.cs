using System.Text.Json;
using Game;
using Xunit;

namespace Game.Core.Tests;

public sealed class SnapshotReceivingLifecycleTests
{
    private static void Act(Match match, string action, int slot = -1, Building building = Building.Empty, UnitType type = UnitType.Swordsman, int city = 1)
    {
        CommandResult result = match.Apply(city, Command.FromSnapshot(match.Snapshot(), 1, action, city, slot, building, type));
        Assert.True(result.Accepted, result.Message);
    }
    private static void Ready(Match match)
    {
        foreach (int city in match.Players.Keys) Act(match, "ready", city: city);
    }
    private static MatchSnapshot Receive(Match match)
    {
        MatchSnapshot sent = match.Snapshot();
        MatchSnapshot received = SnapshotPayload.DecodeForReceiving(SnapshotPayload.Encode(sent));
        Assert.Equal(JsonSerializer.Serialize(sent, WireJson.Options), JsonSerializer.Serialize(received, WireJson.Options));
        return received;
    }

    [Fact]
    public void PaidPartialSquadRetainsReservesWhenItsLastParticipantDiesAndBodyExpires()
    {
        using var match = new Match(new Rules
        {
            StartingGold = 100,
            StartingWood = 100,
            SoldierHealth = 1,
            FarmOutput = 1,
            DefenderDamage = 0,
            Campaign = new([new(1, false, [new(UnitType.Crossbowman, 1)], default)])
        }, combatSeed: 1);
        match.Join(); Act(match, "start"); Act(match, "build", 0, Building.Barracks); Act(match, "build", 2, Building.MetalMine);
        Act(match, "ready"); Act(match, "ready"); Act(match, "build", 4, Building.Farm);
        for (int i = 0; i < 3; i++) Act(match, "recruit", 0);
        UnitState[] purchased = Receive(match).Players[0].Soldiers;
        Act(match, "ready");
        MatchSnapshot entered = Receive(match);
        int participant = Assert.Single(entered.Players[0].LastUpkeep!.Participating);
        int[] reserves = entered.Players[0].LastUpkeep!.Unfed;
        Assert.Equal(2, reserves.Length);
        SquadState original = entered.Squads.Single(s => s.Owner == 1);
        MatchSnapshot frame = entered;
        for (int i = 0; i < 1000 && !frame.DyingBodies.Any(b => b.Id == participant); i++)
        {
            Assert.Equal(Phase.Combat, match.Phase); match.Step(); frame = Receive(match);
        }
        UnitState body = Assert.Single(frame.DyingBodies, b => b.Id == participant);
        SquadState fallen = frame.Squads.Single(s => s.Id == original.Id);
        Assert.Equal(UnitLifecycle.Dying, fallen.Hex.Lifecycle); Assert.Empty(fallen.ParticipatingMembers);
        Assert.Equal(reserves, fallen.Members);
        Assert.DoesNotContain(frame.Reservations.Positions, p => p.UnitId == fallen.Id);
        Assert.DoesNotContain(frame.Reservations.Transit, p => p.UnitId == fallen.Id);
        foreach (int id in reserves)
        {
            UnitState reserve = frame.Players[0].Soldiers.Single(u => u.Id == id);
            UnitState bought = purchased.Single(u => u.Id == id);
            Assert.Equal(UnitLifecycle.Reserve, reserve.Hex!.Lifecycle); Assert.False(reserve.PendingImpact);
            Assert.Equal((bought.Health, bought.Level, bought.Experience, bought.Assignment, bought.SquadId, bought.FormationSlot),
                (reserve.Health, reserve.Level, reserve.Experience, reserve.Assignment, reserve.SquadId, reserve.FormationSlot));
        }
        while (match.Tick < body.Hex!.DeathEndTick)
        {
            Assert.Equal(body.Hex, frame.DyingBodies.Single(b => b.Id == body.Id).Hex);
            match.Step(); frame = Receive(match);
        }
        Assert.DoesNotContain(frame.DyingBodies, b => b.Id == participant);
        Assert.Equal(reserves, frame.Players[0].Soldiers.Select(u => u.Id));
        Assert.DoesNotContain(frame.Reservations.Positions, p => p.UnitId == original.Id);
    }

    [Fact]
    public void PaidPartialSquadKeepsCommittedWindupAndLatchedAimAfterAnotherActorKillsItsTarget()
    {
        using var match = new Match(new Rules
        {
            StartingGold = 100,
            StartingWood = 100,
            SoldierHealth = 1,
            SoldierDamage = 0,
            BerserkerHealth = 400,
            BerserkerDamage = 0,
            RangedDamage = 50,
            FarmOutput = 1,
            StoneOutput = 2,
            ClothOutput = 5,
            MageUpkeep = 1,
            DefenderDamage = 0,
            Combat = new() { MageHexRange = 4, MageWindupTicks = 120, MageRecoveryTicks = 30 },
            Campaign = new([new(1, false, [new(UnitType.Swordsman, 1), new(UnitType.Berserker, 1)], default)])
        }, combatSeed: 1);
        match.Join(); match.Join(); Act(match, "start"); Act(match, "build", 0, Building.ArcheryRange);
        Act(match, "build", 2, Building.MetalMine); Act(match, "build", 3, Building.Stonecutter); Act(match, "build", 4, Building.Weaver);
        Ready(match); Act(match, "buy-plot", 5); Act(match, "build", 5, Building.Farm); Ready(match);
        Act(match, "build", 1, Building.Arcanum); Act(match, "recruit", 0, type: UnitType.Crossbowman);
        for (int i = 0; i < 3; i++) Act(match, "recruit", 1, type: UnitType.Mage);
        Ready(match);
        MatchSnapshot frame = Receive(match);
        SquadState mages = frame.Squads.Single(s => s.Owner == 1 && s.Type == UnitType.Mage);
        Assert.Single(mages.ParticipatingMembers); Assert.Equal(3, mages.Members.Length);
        UnitState? latched = null;
        for (int i = 0; i < 1200 && latched is null; i++)
        {
            Assert.Equal(Phase.Combat, match.Phase); match.Step(); frame = Receive(match);
            SquadState actor = frame.Squads.Single(s => s.Id == mages.Id);
            if (actor.Hex.Action != UnitActionKind.Windup || actor.TargetKind != SquadTargetKind.Squad
                || frame.Squads.Any(s => s.Id == actor.TargetId && s.Hex.Lifecycle == UnitLifecycle.Alive)) continue;
            latched = frame.Players[0].Soldiers.Single(u => actor.ParticipatingMembers.Contains(u.Id));
        }
        Assert.NotNull(latched); Assert.True(latched.PendingImpact); Assert.NotNull(latched.Hex!.FrozenAim);
        SquadState committed = frame.Squads.Single(s => s.Id == mages.Id);
        Assert.All(frame.Players[0].Soldiers.Where(u => u.SquadId == mages.Id && u.Hex!.Lifecycle == UnitLifecycle.Reserve), reserve =>
        {
            Assert.Equal(UnitActionKind.Windup, reserve.Hex!.Action); Assert.Equal(0, reserve.TargetId);
            Assert.Equal(committed.AttackSequence, reserve.AttackSequence); Assert.False(reserve.PendingImpact);
        });
        Assert.Contains(frame.CombatEvents, e => e.Type == CombatEventType.Death && e.Unit?.Id == latched.TargetId);
        while (match.Tick < committed.ImpactTick)
        {
            UnitState pending = frame.Players[0].Soldiers.Single(u => u.Id == latched.Id);
            Assert.Equal(latched.TargetId, pending.TargetId); Assert.Equal(latched.Hex.FrozenAim, pending.Hex!.FrozenAim);
            Assert.Equal(committed.AttackSequence, pending.AttackSequence);
            match.Step(); frame = Receive(match);
        }
        UnitState recovered = frame.Players[0].Soldiers.Single(u => u.Id == latched.Id);
        Assert.Equal(UnitActionKind.Recovery, recovered.Hex!.Action); Assert.False(recovered.PendingImpact);
        Assert.Equal(committed.ReadyTick, recovered.ReadyTick); Assert.Equal(latched.TargetId, recovered.TargetId);
        Assert.Equal(latched.Hex.FrozenAim, recovered.Hex.FrozenAim);
        Assert.Contains(frame.CombatEvents, e => e.Unit?.Id == latched.Id && e.Type == CombatEventType.Impact
            && e.Unit.AttackSequence == committed.AttackSequence && !e.Landed);
    }
}
