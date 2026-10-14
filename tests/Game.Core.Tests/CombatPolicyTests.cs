using Xunit;

namespace Game.Core.Tests;

// Tier A against actual squad action/routing owners. Old anchor release,
// city-local observations and member-policy caches are intentionally absent.
// Fixed graph/cells/seeds; no processes. One expansion per decision is measured
// at the genuine BFS site, rather than inferred from a copied scoring policy.
public sealed class CombatPolicyTests
{
    [Fact]
    public void ReleasedSquadCellsMakeTheCloserOpponentReachableAtTheNextBoundary()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 38);
        int nearer = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 18);
        int reachable = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 1);
        foreach (int id in new[] { nearer, reachable }) combat.Seed(combat.Inspect(id) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        int[] blockers = combat.Board.Cell(18).Neighbors.Select(cell =>
        {
            int id = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, cell, UnitType.Crossbowman);
            combat.Seed(combat.Inspect(id) with { Action = new CombatAction.Recovery(0, 0, 1000) });
            return id;
        }).ToArray();
        combat.StartActions([]);
        Assert.Equal(combat.Read(reachable).SquadId, combat.Read(actor).Decision!.ObjectiveId);
        CombatAction.Moving move = Assert.IsType<CombatAction.Moving>(combat.Inspect(actor).Action);
        foreach (int id in blockers) combat.Remove(id);
        combat.StartActions([]);
        Assert.Same(move, combat.Inspect(actor).Action);
        combat.Advance(move.EndTick, []); combat.StartActions([]);
        Assert.Equal(combat.Read(nearer).SquadId, combat.Read(actor).Decision!.ObjectiveId);
    }

    [Fact]
    public void ReachabilityMatchesIndependentBoundedRelaxationForEverySource()
    {
        var board = new HexBoard(HexBoardDefinition.Default());
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (int source in board.Cells.Where(c => board.Allows(faction, c.Id)).Select(c => c.Id))
            {
                bool Allows(int cell) => board.Allows(faction, cell) && cell is not (10 or 13);
                var reference = new Dictionary<int, int> { [source] = 0 };
                for (int length = 1; length < board.Cells.Count; length++)
                {
                    int[] next = board.Cells.Where(c => Allows(c.Id) && !reference.ContainsKey(c.Id)
                        && c.Neighbors.Any(n => reference.TryGetValue(n, out int distance) && distance == length - 1)).Select(c => c.Id).ToArray();
                    foreach (int cell in next) reference.Add(cell, length);
                }
                var actual = new HexReachability(board, source, Allows);
                Assert.Equal(reference.OrderBy(p => p.Key), actual.Distances.OrderBy(p => p.Key));
                foreach (var (cell, parents) in actual.Parents)
                {
                    Assert.Equal(parents.Count, parents.Distinct().Count());
                    Assert.All(parents, parent =>
                    {
                        Assert.Contains(cell, board.Cell(parent).Neighbors);
                        Assert.Equal(actual.Distances[cell] - 1, actual.Distances[parent]);
                    });
                }
            }
    }

    [Fact]
    public void OneExpansionScoresEveryOpponentAndCommittedMovesDoNotSearchAgain()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 38);
        foreach (int cell in new[] { 1, 2, 3, 4, 5, 6, 18 })
        {
            int id = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, cell);
            combat.Seed(combat.Inspect(id) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        }
        var work = new WorkCounters(); combat.Work = work;
        combat.StartActions([]);
        Assert.Equal(1, combat.RouteSearches); Assert.Equal(1, work.Snapshot()[WorkMetric.BfsSearches]);
        Assert.Equal(18, combat.Read(actor).Decision!.ObjectiveCell);
        Assert.IsType<CombatAction.Moving>(combat.Inspect(actor).Action);
        combat.StartActions([]);
        Assert.Equal(1, combat.RouteSearches); Assert.Equal(1, work.Snapshot()[WorkMetric.BfsSearches]);
    }

    [Fact]
    public void StaleMemberVisitedHintsCannotExcludeTheSharedShortestRoute()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 38);
        int target = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 2);
        combat.Seed(combat.Inspect(target) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        CombatUnit unit = combat.Inspect(actor);
        combat.Seed(unit with { Decision = unit.Decision with { ObjectiveId = combat.Read(target).SquadId, ObjectiveCell = 2, Visited = [32] } });
        combat.StartActions([]);
        Assert.Equal(1, combat.RouteSearches);
        Assert.Empty(combat.Read(actor).Decision!.Visited);
        Assert.DoesNotContain(combat.Read(actor).Decision!.Route, p => p.Cell == 32); // physical Watchpost
    }

    [Fact]
    public void NewObjectiveClearsOldHintsButNeverRedirectsACommittedMove()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 38);
        int old = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 1);
        int nearer = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 18);
        foreach (int id in new[] { old, nearer }) combat.Seed(combat.Inspect(id) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        CombatUnit unit = combat.Inspect(actor);
        combat.Seed(unit with { Decision = unit.Decision with { ObjectiveId = combat.Read(old).SquadId, ObjectiveCell = 1, Visited = [13, 14, 16, 18] } });
        combat.StartActions([]);
        CombatAction.Moving move = Assert.IsType<CombatAction.Moving>(combat.Inspect(actor).Action);
        Assert.Empty(combat.Inspect(actor).Decision.Visited);
        Assert.Equal(combat.Read(nearer).SquadId, combat.Inspect(actor).Decision.ObjectiveId);
        int cell = combat.Board.Cell(move.Destination.Cell).Neighbors.First(c => c != unit.Location.Position.Cell && c != 18 && !combat.Arena.Footprints.Any(f => f.Cell == c));
        int incoming = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, cell);
        combat.Seed(combat.Inspect(incoming) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        combat.StartActions([]); Assert.Same(move, combat.Inspect(actor).Action);
        combat.Advance(move.EndTick, []); combat.StartActions([]);
        Assert.Equal(incoming, combat.Read(actor).TargetId);
    }

    [Fact]
    public void NearbyEnemyInAnotherAssignedSectorIsAnImmediateSharedEngagement()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        combat.ConfigureArena([1, 2]);
        var arena = new SharedArena([1, 2]);
        int cell = arena.Deployment(2, 2);
        int actor = combat.Create(UnitType.Swordsman, 2, 2, 2);
        combat.Seed(combat.Inspect(actor) with { Location = new(UnitLifecycle.Alive, new(cell, 1)) });
        int adjacent = arena.Board.Cell(cell).Neighbors.First(arena.CanOccupy);
        int enemy = combat.Create(UnitType.Swordsman, 0, 1, 1, Faction.Skeletons);
        combat.Seed(combat.Inspect(enemy) with { Location = new(UnitLifecycle.Alive, new(adjacent, 1)) });
        combat.StartActions([]);
        Assert.Equal(UnitActionKind.Windup, combat.Read(actor).Hex!.Action);
        Assert.Equal(enemy, combat.Read(actor).TargetId);
        Assert.Equal(0, combat.RouteSearches);
    }
}
