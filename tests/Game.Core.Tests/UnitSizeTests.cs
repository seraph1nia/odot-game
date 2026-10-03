using System.Text.Json;
using Xunit;

namespace Game.Core.Tests;

public sealed class UnitSizeTests
{
    private static ReservationOwner Actor(int id, int size, int anchor, int cell = 11, Faction faction = Faction.Adventurers)
        => new(id, 1, faction, UnitLifecycle.Alive, new(cell, anchor), 0, true, Size: size);

    [Fact]
    public void EverySizeAndMixedTotalFitsRegardlessOfAnchorArrangement()
    {
        var board = new HexBoard(HexBoardDefinition.Default());
        foreach (Faction faction in Enum.GetValues<Faction>())
            for (int first = 1; first <= 6; first++)
                for (int second = 1; second <= 6; second++)
                    foreach (int anchor in Enumerable.Range(2, 5))
                    {
                        var index = new HexOccupancy(board);
                        Assert.True(index.TryPlace(Actor(1, first, 1, faction: faction)));
                        Assert.Equal(first + second <= 6, index.TryPlace(Actor(2, second, anchor, faction: faction)));
                        Assert.Equal(first + (first + second <= 6 ? second : 0), index.UsedCapacity(1, 11));
                    }
        foreach (int[] sizes in new[] { new[] { 2, 2, 2 }, new[] { 1, 2, 3 } })
        {
            var index = new HexOccupancy(board);
            for (int n = 0; n < sizes.Length; n++) Assert.True(index.TryPlace(Actor(n + 1, sizes[n], n * 2 + 1)));
            Assert.Equal(6, index.UsedCapacity(1, 11));
            Assert.Empty(index.Free(1, Faction.Adventurers, 11, 1));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(int.MaxValue)]
    public void InvalidSizeCannotReplaceValidReservations(int size)
    {
        var index = new HexOccupancy(new(HexBoardDefinition.Default()));
        Assert.True(index.TryPlace(Actor(1, 2, 1)));
        string before = JsonSerializer.Serialize(index.Snapshot());
        Assert.False(index.TryPlace(Actor(2, size, 2)));
        Assert.Throws<ArgumentException>(() => index.Rebuild([Actor(2, size, 2)]));
        Assert.Equal(before, JsonSerializer.Serialize(index.Snapshot()));
        Assert.Throws<ArgumentException>(() => new CombatConfiguration(new() { Combat = new() { Swordsman = new(size, 10, 30, 48) } }));
    }

    [Fact]
    public void OrdinaryAndBossIdentityPreserveSizeAcrossResearchTransferAndWireState()
    {
        using var combat = new CombatSimulation(new(), seed: 1);
        foreach (UnitType type in Enum.GetValues<UnitType>())
            foreach (Faction faction in Enum.GetValues<Faction>())
                Assert.Equal(2, combat.Read(combat.Create(type, faction == Faction.Adventurers ? 1 : 0, 1, 1, faction)).Size);
        int boss = combat.Create(UnitType.Swordsman, 1, 1, 1, isBoss: true);
        combat.Research(1, new(Owned: 1UL << (int)TechnologyId.MeleeFoundation), new());
        combat.BeginWave();
        UnitState before = combat.Read(boss);
        Assert.True(before.IsBoss); Assert.Equal(6, before.Size);
        combat.Transfer(boss, 2); combat.AdmitEntries();
        UnitState after = combat.Read(boss);
        Assert.Equal((before.Id, before.Health, before.Profile, before.IsBoss), (after.Id, after.Health, after.Profile, after.IsBoss));
        Assert.Equal(6, after.Hex!.Size);
        Assert.Equal(after, JsonSerializer.Deserialize<UnitState>(JsonSerializer.Serialize(after, WireJson.Options), WireJson.Options)! with { Decision = after.Decision });
        Assert.Equal(6, Assert.Single(combat.Reservations.Positions, r => r.UnitId == boss).Size);
    }

    [Fact]
    public void FullSizeMoveAndMovingDeathHoldBothEndpointsWithoutPartialClaims()
    {
        var board = new HexBoard(HexBoardDefinition.Default());
        var index = new HexOccupancy(board);
        ReservationOwner mover = Actor(1, 4, 1), blocker = Actor(2, 3, 3, cell: 10);
        Assert.True(index.TryPlace(mover)); Assert.True(index.TryPlace(blocker));
        HexPosition destination = new(10, 2);
        string before = JsonSerializer.Serialize(index.Snapshot());
        Assert.False(index.TryMove(mover, destination, 1));
        Assert.Equal(before, JsonSerializer.Serialize(index.Snapshot()));
        index.Release(1, 2);
        Assert.True(index.TryMove(mover, destination, 1));
        Assert.Equal(4, index.UsedCapacity(1, 11)); Assert.Equal(4, index.UsedCapacity(1, 10));
        ReservationOwner dead = mover with
        {
            Move = new(1, destination, board.Transition(mover.Position, destination).Id, 10, 40),
            ActionSequence = 1,
            Ready = false,
            Lifecycle = UnitLifecycle.Dying,
            DeathStartTick = 20,
            DeathEndTick = 68,
            FrozenMoveTicks = 10
        };
        index.Rebuild([dead]);
        Assert.False(index.ExpireDeath(dead, 67)); Assert.False(index.Arrive(dead, 40));
        Assert.All(index.Snapshot().Positions, r => Assert.Equal(4, r.Size));
        Assert.True(index.ExpireDeath(dead, 68)); Assert.Empty(index.Snapshot().Positions); Assert.Empty(index.Snapshot().Transit);
    }

    [Fact]
    public void BlockedBossCannotPoolCapacityOrPreventAFittingArrival()
    {
        using var combat = new CombatSimulation(new(), seed: 1);
        int Stand(int cell, Faction faction, bool boss)
        {
            int id = combat.Create(UnitType.Swordsman, faction == Faction.Adventurers ? 1 : 0, 1, 1, faction, isBoss: boss);
            int anchor = combat.Reservations.Positions.Count(r => r.Cell == cell) + 1;
            combat.Seed(combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, anchor)) });
            return id;
        }
        foreach (int cell in combat.Board.Front(Faction.Skeletons)) Stand(cell, Faction.Adventurers, true);
        int[] rear = combat.Board.Rear(Faction.Skeletons).ToArray();
        var blockers = new Dictionary<int, int[]>();
        foreach (int cell in rear) blockers[cell] = [Stand(cell, Faction.Skeletons, false), Stand(cell, Faction.Skeletons, false)];
        int boss = combat.Create(UnitType.Swordsman, 0, 2, 1, Faction.Skeletons, isBoss: true);
        int smaller = combat.Create(UnitType.Swordsman, 0, 2, 1, Faction.Skeletons);
        combat.Seed(combat.Inspect(boss) with { Decision = new(0, 0, 0) });
        combat.Seed(combat.Inspect(smaller) with { Decision = new(0, 0, ulong.MaxValue) });
        UnitState waiting = combat.Read(boss);
        combat.AdmitEntries();
        Assert.False(combat.Read(boss).Deployed); Assert.True(combat.Read(smaller).Deployed);
        Assert.Equal(waiting, combat.Read(boss));
        Assert.DoesNotContain(combat.Reservations.Positions, r => r.UnitId == boss);
        int freeCell = rear.First(cell => cell != combat.Read(smaller).Hex!.Position.Cell);
        combat.Remove(blockers[freeCell][0]); combat.AdmitEntries();
        Assert.False(combat.Read(boss).Deployed); // Four free in this cell, two in another cannot be pooled.
        combat.Remove(blockers[freeCell][1]); combat.AdmitEntries();
        Assert.True(combat.Read(boss).Deployed); Assert.Equal(freeCell, combat.Read(boss).Hex!.Position.Cell);
        Assert.Equal(6, Assert.Single(combat.Reservations.Positions, r => r.UnitId == boss).Size);
    }
}
