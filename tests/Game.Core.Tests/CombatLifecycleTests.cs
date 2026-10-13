using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class CombatLifecycleTests
{
    private static (CombatSimulation Combat, int Mover) MovingCasualty()
    {
        var combat = new CombatSimulation(new Rules { DefenderDamage = 0 }, seed: 123);
        int mover = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        int attacker = combat.Create(UnitType.Crossbowman, 1, 1, 1);
        UnitState unit = combat.Read(mover), enemy = combat.Read(attacker);
        combat.Seed(unit with { Health = 100, Hex = CombatFixture.At(unit, 17, 1) });
        combat.Seed(enemy with { Hex = CombatFixture.At(enemy, 20, 1) });
        combat.Step(1, []);
        Assert.Equal(UnitActionKind.Moving, combat.Read(mover).Hex!.Action);
        for (int tick = 2; tick <= 19; tick++) combat.Step(tick, []);
        Assert.Equal(mover, Assert.Single(combat.Dying()).Id);
        return (combat, mover);
    }
    [Fact]
    public void MovingDeathReleasesLogicalClaimsImmediatelyAndRetainsFrozenCosmeticsUntilExpiry()
    {
        var (combat, mover) = MovingCasualty(); using (combat)
        {
            UnitState corpse = Assert.Single(combat.Dying()); HexUnitState frozen = corpse.Hex!;
            Assert.False(corpse.PendingImpact); Assert.DoesNotContain(combat.Snapshot(), u => u.Id == mover);
            Assert.Equal(19, frozen.DeathStartTick); Assert.Equal(67, frozen.DeathEndTick);
            Assert.Equal(18, frozen.FrozenMoveTicks); Assert.True(frozen.HoldsTransit);
            Assert.DoesNotContain(combat.Reservations.Positions, r => r.UnitId == corpse.SquadId);
            Assert.DoesNotContain(combat.Reservations.Transit, r => r.UnitId == corpse.SquadId);
            CombatReservations rebuilt = CombatReservations.Reconstruct(combat.Arena, combat.SquadSnapshots());
            Assert.Equal(combat.Reservations.Positions, rebuilt.Positions);
            Assert.Equal(combat.Reservations.Transit, rebuilt.Transit);
            for (long tick = 20; tick < frozen.DeathEndTick; tick++)
            {
                combat.Step(tick, []);
                Assert.Equal(frozen, Assert.Single(combat.Dying()).Hex);
                Assert.DoesNotContain(combat.Reservations.Positions, r => r.UnitId == corpse.SquadId);
                Assert.DoesNotContain(combat.Reservations.Transit, r => r.UnitId == corpse.SquadId);
            }
            Assert.DoesNotContain(combat.Events(), e => e.Unit?.Id == mover && e.Tick >= frozen.DeathStartTick && e.Type == CombatEventType.AttackStarted);
            combat.Step(frozen.DeathEndTick, []);
            Assert.Empty(combat.Dying());
            Assert.DoesNotContain(combat.Reservations.Positions, r => r.UnitId == mover);
            Assert.DoesNotContain(combat.Reservations.Transit, r => r.UnitId == mover);
        }
    }
    [Theory]
    [InlineData("retire")]
    [InlineData("regroup")]
    [InlineData("store")]
    public void LastLivingRosterDepartureReleasesActorBeforeRetainedCasualtyExpires(string action)
    {
        using var combat = new CombatSimulation(new Rules { DefenderDamage = 0 }, seed: 123);
        var arena = new SharedArena([1]);
        int casualty = combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
        int survivor = combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1));
        int oldSquad = combat.Read(survivor).SquadId;
        HexTransition transition = arena.Board.Transition(new(17, 1), new(18, 1));
        var move = new CombatAction.Moving(1, transition.Destination, transition.Id, 0, 100);
        combat.Seed(combat.Inspect(casualty) with
        {
            Health = 1,
            Location = new(UnitLifecycle.Alive, transition.Source),
            Action = move,
            Statuses = StatusPolicy.Apply(StatusState.Empty, new(casualty, StatusKind.Burn, survivor, 1, 0, 100), new())
        });
        combat.Seed(combat.Inspect(survivor) with { Location = new(UnitLifecycle.Alive, transition.Source), Action = move });
        combat.Advance(60, []); combat.StopActions([]);
        UnitState body = Assert.Single(combat.Dying());
        Assert.Equal(oldSquad, Assert.Single(combat.Reservations.Transit).UnitId);
        if (action == "retire") combat.Remove(survivor);
        else combat.Assign(survivor, action == "store" ? new(1, 1, 0, 1) : new(-1, 0, 1, 1));
        Assert.DoesNotContain(combat.Reservations.Positions, p => p.UnitId == oldSquad);
        Assert.DoesNotContain(combat.Reservations.Transit, p => p.UnitId == oldSquad);
        Assert.DoesNotContain(combat.SquadSnapshots(), s => s.Id == oldSquad);
        Assert.Equal(body, Assert.Single(combat.Dying()));
        combat.RestoreHomes();
        combat.Create(UnitType.Swordsman, 1, 1, 1, assignment: new(-1, 0, 0, 1)); combat.RestoreHomes();
        CombatReservations rebuilt = CombatReservations.Reconstruct(arena, combat.SquadSnapshots());
        Assert.Equal(combat.Reservations.Positions, rebuilt.Positions); Assert.Empty(rebuilt.Transit);
        Assert.Equal(body, Assert.Single(combat.Dying()));
        combat.Cleanup(body.Hex!.DeathEndTick);
        Assert.Empty(combat.Dying());
        Assert.NotEmpty(combat.Snapshot());
    }

    [Fact]
    public void DyingTransferIsRejectedWithoutChangingFrozenIdentityOrOtherSquadClaims()
    {
        var (combat, mover) = MovingCasualty(); using (combat)
        {
            UnitState corpse = Assert.Single(combat.Dying());
            string reservations = JsonSerializer.Serialize(combat.Reservations);
            Assert.Throws<InvalidOperationException>(() => combat.Transfer(mover, 2));
            Assert.Equal(JsonSerializer.Serialize(corpse), JsonSerializer.Serialize(Assert.Single(combat.Dying())));
            Assert.Equal(1, corpse.Destination);
            Assert.Equal(reservations, JsonSerializer.Serialize(combat.Reservations));
        }
    }
    [Fact]
    public void DisposalReleasesOtherSquadClaimsAndCosmeticsWithoutAffectingFreshCombat()
    {
        var (combat, _) = MovingCasualty();
        Assert.NotEmpty(combat.Reservations.Positions); Assert.NotEmpty(combat.Events());
        combat.Dispose(); combat.Dispose();
        Assert.True(combat.RegistryReleased); Assert.Empty(combat.Reservations.Positions); Assert.Empty(combat.Reservations.Transit);
        Assert.Empty(combat.Events()); Assert.Empty(combat.HealthProgressCities);
        Assert.Throws<ObjectDisposedException>(() => combat.Dying());
        using var fresh = new CombatSimulation(new(), seed: 123);
        Assert.Empty(fresh.Snapshot()); Assert.Empty(fresh.Dying()); Assert.Empty(fresh.Reservations.Positions); Assert.Empty(fresh.Reservations.Transit);
        Assert.Equal(0, fresh.EventSequence);
        Assert.Equal(1, fresh.Create(UnitType.Swordsman, 1, 1, 1));
    }
}
