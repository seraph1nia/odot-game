using Xunit;

namespace Game.Core.Tests;

public sealed class HexRoutingTests
{
    private static readonly CombatFingerprint Fingerprint = new CombatConfiguration(new()).Fingerprint;
    private static int At(CombatSimulation combat, UnitType type, Faction faction, int cell, int footprint)
    {
        int id = combat.Create(type, faction == Faction.Adventurers ? 1 : 0, 1, 1, faction);
        UnitState unit = combat.Read(id);
        combat.Seed(unit with { Hex = CombatFixture.At(unit, cell, footprint) });
        return id;
    }
    private static Approach Query(CombatSimulation combat, int actor, int target, HexOccupancy occupancy, int[]? visited = null, ulong seed = 123, HexRouting? routing = null)
        => (routing ?? new HexRouting(combat.Board, occupancy)).Find(combat.Read(actor), combat.Read(target), combat.Snapshot(),
            new(seed, Fingerprint, 1, 1, CombatActorKind.Unit, actor, 0, 0, CombatPurpose.Target), visited ?? []);
    [Fact]
    public void OccupiedDirectApproachUsesAFeasibleDetourWithoutEnteringOpposingCells()
    {
        using var combat = new CombatSimulation(new());
        int actor = At(combat, UnitType.Swordsman, Faction.Adventurers, 17, 7);
        int target = At(combat, UnitType.Swordsman, Faction.Skeletons, 8, 7);
        At(combat, UnitType.Crossbowman, Faction.Skeletons, 13, 1);
        At(combat, UnitType.Crossbowman, Faction.Skeletons, 14, 1);
        var occupancy = new HexOccupancy(combat.Board); occupancy.Rebuild(combat.Snapshot().Select(u => u.Hex!));
        Approach detour = Query(combat, actor, target, occupancy);
        Assert.Equal(2, detour.StaticSteps); Assert.Equal(3, detour.Route.Length); Assert.Equal(18, detour.Route[0].Cell);
        Assert.DoesNotContain(detour.Route, p => p.Cell is 13 or 14);
        Assert.Equal(1, combat.Board.Distance(detour.Route[^1].Cell, 8));
        Assert.Equal(detour.Route.Length, detour.Route.Select(p => p.Cell).Distinct().Count());
        int previous = 17;
        foreach (HexPosition step in detour.Route)
        { Assert.Contains(step.Cell, combat.Board.Cell(previous).Neighbors); previous = step.Cell; }
    }
    [Fact]
    public void EqualShortestSupportGoalsPreferScreenThenUnusedCapacity()
    {
        using var combat = new CombatSimulation(new());
        int actor = At(combat, UnitType.Crossbowman, Faction.Adventurers, 17, 1);
        int target = At(combat, UnitType.Swordsman, Faction.Skeletons, 2, 7);
        int screen = At(combat, UnitType.Swordsman, Faction.Adventurers, 8, 7);
        At(combat, UnitType.Crossbowman, Faction.Adventurers, 11, 1);
        At(combat, UnitType.Crossbowman, Faction.Skeletons, 12, 1);
        var occupancy = new HexOccupancy(combat.Board); occupancy.Rebuild(combat.Snapshot().Select(u => u.Hex!));
        var routing = new HexRouting(combat.Board, occupancy);
        foreach (ulong seed in new ulong[] { 0, 1, 123 })
        {
            Approach approach = Query(combat, actor, target, occupancy, seed: seed, routing: routing);
            Assert.Equal(2, approach.Route.Length); Assert.Equal(11, approach.Route[^1].Cell);
        }
        combat.Remove(screen); occupancy.Rebuild(combat.Snapshot().Select(u => u.Hex!));
        foreach (ulong seed in new ulong[] { 0, 1, 123 })
        {
            Approach approach = Query(combat, actor, target, occupancy, seed: seed, routing: routing);
            Assert.Equal(2, approach.Route.Length); Assert.Equal(10, approach.Route[^1].Cell);
        }
        var blockers = new List<int>();
        for (int footprint = 1; footprint <= 6; footprint++) blockers.Add(At(combat, UnitType.Crossbowman, Faction.Adventurers, 10, footprint));
        occupancy.Rebuild(combat.Snapshot().Select(u => u.Hex!));
        Assert.Equal(11, Query(combat, actor, target, occupancy, routing: routing).Route[^1].Cell);
        foreach (int id in blockers) combat.Remove(id);
        occupancy.Rebuild(combat.Snapshot().Select(u => u.Hex!));
        Assert.Equal(10, Query(combat, actor, target, occupancy, routing: routing).Route[^1].Cell);
    }
    [Fact]
    public void TemporaryTransitContentionDoesNotEraseTheApproachAndReleaseAllowsCommit()
    {
        using var combat = new CombatSimulation(new());
        int actor = At(combat, UnitType.Crossbowman, Faction.Adventurers, 17, 1);
        int target = At(combat, UnitType.Swordsman, Faction.Skeletons, 2, 7);
        int moving = At(combat, UnitType.Crossbowman, Faction.Adventurers, 17, 2);
        UnitState mover = combat.Read(moving); HexPosition to = new(14, 1);
        combat.Seed(mover with
        {
            Hex = mover.Hex! with
            { Action = UnitActionKind.Moving, Destination = to, Transition = combat.Board.Transition(mover.Hex.Position, to).Id, ActionSequence = 1, StartTick = 1, EndTick = 31 }
        });
        var occupancy = new HexOccupancy(combat.Board); occupancy.Rebuild(combat.Snapshot().Select(u => u.Hex!));
        Approach blocked = Query(combat, actor, target, occupancy);
        Assert.Equal(2, blocked.Route.Length); Assert.False(occupancy.TryMove(combat.Read(actor).Hex!, blocked.Route[0], 1));
        combat.Seed(combat.Read(moving) with { Hex = mover.Hex! with { Position = to, ActionSequence = 1 } });
        occupancy.Rebuild(combat.Snapshot().Select(u => u.Hex!));
        Approach released = Query(combat, actor, target, occupancy);
        Assert.Equal(2, released.Route.Length); Assert.True(occupancy.TryMove(combat.Read(actor).Hex!, released.Route[0], 1));
    }
    [Fact]
    public void UnchangedEpisodeExcludesPreviouslyVisitedCellsRatherThanWalkingBack()
    {
        using var combat = new CombatSimulation(new());
        int actor = At(combat, UnitType.Crossbowman, Faction.Adventurers, 17, 1);
        int target = At(combat, UnitType.Swordsman, Faction.Skeletons, 2, 7);
        var occupancy = new HexOccupancy(combat.Board); occupancy.Rebuild(combat.Snapshot().Select(u => u.Hex!));
        Approach alternative = Query(combat, actor, target, occupancy, [13]);
        Assert.Equal(2, alternative.Route.Length); Assert.DoesNotContain(alternative.Route, p => p.Cell == 13);
        Assert.Empty(Query(combat, actor, target, occupancy, [13, 14, 16, 18]).Route);
    }
}
