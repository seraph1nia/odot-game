using Xunit;

namespace Game.Core.Tests;

// Tier A reconstruction seam: old member/size leases reject coordinated squad
// exchanges and retain corpses. Typed current actor state must reproduce source
// ownership/transit and reject conflicting malformed baselines without history.
// Deterministic minimal fixtures, no engine/process, estimated <1s.
public sealed class SquadReservationTests
{
    private static SquadState Actor(SharedArena arena, int id, int cell, int destination = 0, Faction faction = Faction.Adventurers)
    {
        HexTransition? move = destination == 0 ? null : arena.Board.Transition(new(cell, 1), new(destination, 1));
        var hex = new HexUnitState(id, 1, faction, UnitLifecycle.Alive, new(cell, 1),
            move is null ? UnitActionKind.Waiting : UnitActionKind.Moving, new(destination, 1), move?.Id ?? 0,
            ActionSequence: 1, StartTick: 10, EndTick: 30, Size: 1);
        return new(id, 1, 1, faction, UnitType.Swordsman, false, Enumerable.Range(id * 10, 1).ToArray(), Enumerable.Range(id * 10, 1).ToArray(), hex, new(1, 0, 0));
    }

    [Theory]
    [InlineData("size-zero")]
    [InlineData("size-negative")]
    [InlineData("size-stacking")]
    [InlineData("size-overflow")]
    [InlineData("identity")]
    [InlineData("namespace")]
    [InlineData("anchor")]
    [InlineData("static")]
    [InlineData("sequence")]
    [InlineData("interval")]
    [InlineData("transition")]
    [InlineData("lifecycle")]
    [InlineData("reserve-transit")]
    public void MalformedClaimsAreRejectedWithoutSizeOrCosmeticLoopholes(string defect)
    {
        var arena = new SharedArena([1]); int source = arena.Board.Cells.First(c => arena.CanOccupy(c.Id)).Id;
        int target = arena.Board.Cell(source).Neighbors.First(arena.CanOccupy);
        SquadState valid = Actor(arena, 1, source, target);
        HexUnitState broken = defect switch
        {
            "size-zero" => valid.Hex with { Size = 0 },
            "size-negative" => valid.Hex with { Size = -1 },
            "size-stacking" => valid.Hex with { Size = 2 },
            "size-overflow" => valid.Hex with { Size = int.MaxValue },
            "identity" => valid.Hex with { Id = 2 },
            "namespace" => valid.Hex with { City = 2 },
            "anchor" => valid.Hex with { Position = new(source, 2) },
            "static" => valid.Hex with { Destination = new(arena.Keep(1), 1) },
            "sequence" => valid.Hex with { ActionSequence = -1 },
            "interval" => valid.Hex with { EndTick = valid.Hex.StartTick },
            "transition" => valid.Hex with { Transition = 0 },
            "lifecycle" => valid.Hex with { Lifecycle = (UnitLifecycle)99 },
            _ => valid.Hex with { Lifecycle = UnitLifecycle.Reserve }
        };
        Assert.Throws<ArgumentException>(() => CombatReservations.Reconstruct(arena, new[] { valid with { Hex = broken } }));
        Assert.Single(CombatReservations.Reconstruct(arena, new[] { valid }).Transit);
    }

    [Fact]
    public void FriendlyExchangeRetainsExclusiveSourcesAndBothTransitsWithoutCorpseLocks()
    {
        var arena = new SharedArena([1]); int source = arena.Board.Cells.First(c => arena.CanOccupy(c.Id)).Id;
        int target = arena.Board.Cell(source).Neighbors.First(arena.CanOccupy);
        SquadState first = Actor(arena, 1, source, target), second = Actor(arena, 2, target, source);
        CombatReservations state = CombatReservations.Reconstruct(arena.Snapshot(), new[] { first, second });
        Assert.Equal(2, state.Positions.Length); Assert.Equal(2, state.Transit.Length);
        Assert.Equal(2, state.Positions.Select(p => p.Cell).Distinct().Count());
        Assert.All(state.Positions, p => Assert.Equal(1, p.Size));
        Assert.Equal(state.Transit[0].EdgeToken, state.Transit[1].EdgeToken);
        SquadState fallen = first with { Hex = first.Hex with { Lifecycle = UnitLifecycle.Dying } };
        CombatReservations afterDeath = CombatReservations.Reconstruct(arena.Snapshot(), new[] { fallen, second });
        Assert.Single(afterDeath.Positions); Assert.Single(afterDeath.Transit);
        Assert.Equal(second.Id, afterDeath.Positions[0].UnitId);
    }

    [Fact]
    public void AuthorityRetainsSourceUntilExactArrivalThenReleasesTransit()
    {
        var arena = new SharedArena([1]);
        using var combat = new CombatSimulation(new() { SoldierDamage = 0 }, seed: 5);
        int ally = combat.Create(UnitType.Swordsman, 1, 1, 1);
        int foe = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        combat.Seed(combat.Inspect(ally) with { Location = new(UnitLifecycle.Alive, new(17, 1)) });
        combat.Seed(combat.Inspect(foe) with { Location = new(UnitLifecycle.Alive, new(20, 1)) });
        combat.StartActions([]);
        SquadState[] committed = combat.SquadSnapshots();
        Assert.All(committed, s => Assert.Equal(UnitActionKind.Moving, s.Hex.Action));
        Assert.Equal(2, combat.Reservations.Positions.Length); Assert.Equal(2, combat.Reservations.Transit.Length);
        combat.Advance(29, []);
        Assert.Equal(committed.Select(s => s.Hex.Position), combat.SquadSnapshots().Select(s => s.Hex.Position));
        combat.Advance(30, []);
        Assert.Equal(committed.Select(s => s.Hex.Destination), combat.SquadSnapshots().Select(s => s.Hex.Position));
        Assert.All(combat.SquadSnapshots(), s => Assert.Equal(UnitActionKind.Waiting, s.Hex.Action));
        Assert.Empty(combat.Reservations.Transit);
        Assert.Equal(2, combat.Reservations.Positions.Select(p => p.Cell).Distinct().Count());
        CombatReservations rebuilt = CombatReservations.Reconstruct(arena, combat.SquadSnapshots());
        Assert.Equal(combat.Reservations.Positions, rebuilt.Positions); Assert.Empty(rebuilt.Transit);
    }

    [Fact]
    public void ConflictingOrUnsynchronizedBaselinesAreRefusedWithoutChangingPriorProjection()
    {
        var arena = new SharedArena([1]); int source = arena.Board.Cells.First(c => arena.CanOccupy(c.Id)).Id;
        int target = arena.Board.Cell(source).Neighbors.First(arena.CanOccupy);
        SquadState first = Actor(arena, 1, source, target), second = Actor(arena, 2, target, source);
        CombatReservations before = CombatReservations.Reconstruct(arena.Snapshot(), new[] { first, second });
        void Reject(SquadState other) => Assert.Throws<ArgumentException>(() => CombatReservations.Reconstruct(arena.Snapshot(), new[] { first, other }));
        Reject(second with { Hex = second.Hex with { Position = first.Hex.Position } });
        Reject(second with { Hex = second.Hex with { EndTick = 31 } });
        Reject(second with { Faction = Faction.Skeletons, Hex = second.Hex with { Faction = Faction.Skeletons } });
        Reject(second with { Hex = second.Hex with { Destination = first.Hex.Destination } });
        Assert.Throws<ArgumentException>(() => CombatReservations.Reconstruct(arena.Snapshot(), new[] { first with { Hex = first.Hex with { Transition = 0 } } }));
        CombatReservations after = CombatReservations.Reconstruct(arena.Snapshot(), new[] { first, second });
        Assert.Equal(before.Positions, after.Positions); Assert.Equal(before.Transit, after.Transit);
    }
}
