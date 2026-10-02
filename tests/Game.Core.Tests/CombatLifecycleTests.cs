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
        combat.Seed(unit with { Health = 100, Hex = CombatFixture.At(unit, 8, 7) });
        combat.Seed(enemy with { Hex = CombatFixture.At(enemy, 14, 1) });
        combat.Step(1, []);
        Assert.Equal(UnitActionKind.Moving, combat.Read(mover).Hex!.Action);
        for (int tick = 2; tick <= 19; tick++) combat.Step(tick, []);
        Assert.Equal(mover, Assert.Single(combat.Dying()).Id);
        return (combat, mover);
    }
    [Fact]
    public void MovingDeathKeepsBothEndpointsAndTransitUntilExactExpiry()
    {
        var (combat, mover) = MovingCasualty(); using (combat)
        {
            UnitState corpse = Assert.Single(combat.Dying()); HexUnitState frozen = corpse.Hex!;
            Assert.False(corpse.PendingImpact); Assert.DoesNotContain(combat.Snapshot(), u => u.Id == mover);
            Assert.Equal(19, frozen.DeathStartTick); Assert.Equal(67, frozen.DeathEndTick);
            Assert.Equal(18, frozen.FrozenMoveTicks); Assert.True(frozen.HoldsTransit);
            PositionReservation[] positions = combat.Reservations.Positions.Where(r => r.UnitId == mover).ToArray();
            Assert.Equal(2, positions.Length);
            Assert.Contains(positions, r => r.Cell == frozen.Position.Cell && r.Footprint == frozen.Position.Footprint);
            Assert.Contains(positions, r => r.Cell == frozen.Destination.Cell && r.Footprint == frozen.Destination.Footprint);
            Assert.Equal(frozen.ActionSequence, Assert.Single(combat.Reservations.Transit, r => r.UnitId == mover).ActionSequence);
            for (long tick = 20; tick < frozen.DeathEndTick; tick++)
            {
                combat.Step(tick, []);
                Assert.Equal(frozen, Assert.Single(combat.Dying()).Hex);
                Assert.Equal(JsonSerializer.Serialize(positions), JsonSerializer.Serialize(combat.Reservations.Positions.Where(r => r.UnitId == mover)));
                Assert.Single(combat.Reservations.Transit, r => r.UnitId == mover);
            }
            Assert.DoesNotContain(combat.Events(), e => e.Unit?.Id == mover && e.Tick >= frozen.DeathStartTick && e.Type == CombatEventType.AttackStarted);
            combat.Step(frozen.DeathEndTick, []);
            Assert.Empty(combat.Dying());
            Assert.DoesNotContain(combat.Reservations.Positions, r => r.UnitId == mover);
            Assert.DoesNotContain(combat.Reservations.Transit, r => r.UnitId == mover);
        }
    }
    [Fact]
    public void DyingTransferIsRejectedWithoutChangingItsOriginReservations()
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
    public void DisposalReleasesRetainedMoveReservationsAndDoesNotAffectAFreshCombat()
    {
        var (combat, _) = MovingCasualty();
        Assert.NotEmpty(combat.Reservations.Transit); Assert.NotEmpty(combat.Events());
        combat.Dispose(); combat.Dispose();
        Assert.True(combat.RegistryReleased); Assert.Empty(combat.Reservations.Positions); Assert.Empty(combat.Reservations.Transit);
        Assert.Empty(combat.Events()); Assert.Empty(combat.Admissions); Assert.Empty(combat.HealthProgressCities);
        Assert.Throws<ObjectDisposedException>(() => combat.Dying());
        using var fresh = new CombatSimulation(new(), seed: 123);
        Assert.Empty(fresh.Snapshot()); Assert.Empty(fresh.Dying()); Assert.Empty(fresh.Reservations.Positions); Assert.Empty(fresh.Reservations.Transit);
        Assert.Equal(0, fresh.EventSequence);
        Assert.Equal(1, fresh.Create(UnitType.Swordsman, 1, 1, 1));
    }
}
