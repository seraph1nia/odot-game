using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class HexOccupancyTests
{
    private readonly HexBoard _board = new(HexBoardDefinition.Default());
    private static ReservationOwner Unit(int id, int cell, int anchor, Faction faction = Faction.Adventurers, int size = 1)
        => new(id, 1, faction, UnitLifecycle.Alive, new(cell, anchor), 0, true, Size: size);
    private ReservationOwner Move(ReservationOwner unit, int cell, int anchor, long sequence = 1)
    {
        HexPosition to = new(cell, anchor);
        return unit with { Move = new(sequence, to, _board.Transition(unit.Position, to).Id, 100, 130), ActionSequence = sequence, Ready = false };
    }
    [Fact]
    public void SharedSizesAndFactionProtectionApplyEvenWhenCapacityIsFree()
    {
        var occupancy = new HexOccupancy(_board);
        Assert.True(occupancy.TryPlace(Unit(1, 11, 1, size: 2)));
        Assert.True(occupancy.TryPlace(Unit(2, 11, 3, size: 2)));
        Assert.True(occupancy.TryPlace(Unit(3, 11, 5)));
        Assert.Equal(5, occupancy.UsedCapacity(1, 11));
        Assert.False(occupancy.TryPlace(Unit(4, 11, 6, size: 2)));
        Assert.False(occupancy.TryPlace(Unit(5, 11, 6, Faction.Skeletons)));
        Assert.True(occupancy.TryPlace(Unit(6, 11, 6))); Assert.Equal(6, occupancy.UsedCapacity(1, 11));
        int enemyRear = _board.Rear(Faction.Skeletons)[0];
        Assert.False(occupancy.TryPlace(Unit(7, enemyRear, 1)));
        Assert.True(occupancy.TryPlace(Unit(8, enemyRear, 1, Faction.Skeletons)));
        occupancy.Release(1, 8); Assert.False(occupancy.TryPlace(Unit(7, enemyRear, 1)));
    }
    [Fact]
    public void SourceDestinationAndTransitReserveAtomicallyAndIndependentMovesCanStartTogether()
    {
        var occupancy = new HexOccupancy(_board);
        ReservationOwner a = Unit(1, 11, 1), b = Unit(2, 11, 2), c = Unit(3, 7, 3), independent = Unit(4, 16, 4);
        foreach (ReservationOwner unit in new[] { a, b, c, independent }) Assert.True(occupancy.TryPlace(unit));
        ReservationOwner move = Move(a, 10, 1); Assert.True(occupancy.TryMove(a, move.Destination, 1));
        string before = JsonSerializer.Serialize(occupancy.Snapshot()); long revision = occupancy.Revision;
        Assert.False(occupancy.TryMove(b, new(10, b.Position.Anchor), 1));
        Assert.False(occupancy.TryMove(c, new(10, c.Position.Anchor), 1));
        Assert.Equal(before, JsonSerializer.Serialize(occupancy.Snapshot())); Assert.Equal(revision, occupancy.Revision);
        Assert.True(occupancy.TryMove(independent, new(17, independent.Position.Anchor), 1));
        Assert.Equal(2, occupancy.Snapshot().Transit.Length);
        Assert.Equal(2, occupancy.UsedCapacity(1, 11)); Assert.Equal(1, occupancy.UsedCapacity(1, 10));
    }
    [Fact]
    public void ArrivalChangesOneAttackableCellOnlyAtTheDeclaredTick()
    {
        var occupancy = new HexOccupancy(_board); ReservationOwner unit = Unit(1, 11, 1), move = Move(unit, 10, 1);
        Assert.True(occupancy.TryPlace(unit)); Assert.True(occupancy.TryMove(unit, move.Destination, 1));
        Assert.False(occupancy.Arrive(move, 129)); Assert.Equal(11, move.Position.Cell);
        Assert.Equal(2, occupancy.Snapshot().Positions.Length);
        Assert.True(occupancy.Arrive(move, 130));
        ReservationOwner arrived = move with { Position = move.Destination, Move = null, Ready = true };
        Assert.Equal(10, arrived.Position.Cell); Assert.Equal(0, occupancy.UsedCapacity(1, 11));
        Assert.Single(occupancy.Snapshot().Positions); Assert.Empty(occupancy.Snapshot().Transit);
    }
    [Fact]
    public void ATransitDeathKeepsBothEndpointsAndCanBeReconstructedWithoutEvents()
    {
        ReservationOwner dead = Move(Unit(1, 11, 1), 10, 1) with
        { Lifecycle = UnitLifecycle.Dying, DeathStartTick = 115, DeathEndTick = 163, FrozenMoveTicks = 15 };
        var occupancy = new HexOccupancy(_board); occupancy.Rebuild([dead]);
        Assert.Equal(UnitLifecycle.Dying, dead.Lifecycle); Assert.False(occupancy.Arrive(dead, 130)); Assert.False(occupancy.ExpireDeath(dead, 162));
        Assert.Equal(2, occupancy.Snapshot().Positions.Length); Assert.Single(occupancy.Snapshot().Transit);
        var restored = new HexOccupancy(_board); restored.Rebuild([dead]);
        Assert.Equal(JsonSerializer.Serialize(occupancy.Snapshot()), JsonSerializer.Serialize(restored.Snapshot()));
        Assert.True(occupancy.ExpireDeath(dead, 163)); Assert.Empty(occupancy.Snapshot().Positions); Assert.Empty(occupancy.Snapshot().Transit);
        Assert.True(occupancy.TryPlace(Unit(2, 11, 1)));
    }
    [Fact]
    public void ReconstructionRejectsConflictsWithoutMutatingTheCurrentIndexAndTransferReleasesAllLocks()
    {
        var occupancy = new HexOccupancy(_board); ReservationOwner a = Unit(1, 11, 1), b = Unit(2, 7, 2);
        ReservationOwner move = Move(a, 10, 1); occupancy.Rebuild([move, b]);
        string before = JsonSerializer.Serialize(occupancy.Snapshot());
        Assert.Throws<ArgumentException>(() => occupancy.Rebuild([move, move]));
        Assert.Throws<ArgumentException>(() => occupancy.Rebuild([move, Move(b, 10, 2)]));
        Assert.Equal(before, JsonSerializer.Serialize(occupancy.Snapshot()));
        occupancy.Release(1, a.Id); Assert.Single(occupancy.Snapshot().Positions); Assert.Empty(occupancy.Snapshot().Transit);
        var queued = a with { City = 2, Lifecycle = UnitLifecycle.Queued, Position = default }; occupancy.Rebuild([queued, b]);
        Assert.Single(occupancy.Snapshot().Positions); Assert.Equal(UnitLifecycle.Queued, queued.Lifecycle);
    }
}
