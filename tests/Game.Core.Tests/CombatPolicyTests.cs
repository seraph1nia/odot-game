using Xunit;

namespace Game.Core.Tests;

public sealed class CombatPolicyTests
{
    [Fact]
    public void ReleasedFootprintsMakeTheCloserOpponentEligibleAtArrival()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 16);
        int nearer = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 11);
        int reachable = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 15, UnitType.Mage);
        foreach (int id in new[] { nearer, reachable }) combat.Seed(combat.Inspect(id) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        var blockers = new List<int>();
        foreach (int cell in new[] { 7, 8, 10, 12, 13, 14 })
            for (int footprint = 1; footprint <= 6; footprint++)
            {
                int id = combat.Create(UnitType.Crossbowman, 1, 1, 1);
                combat.Seed(combat.Inspect(id) with { Location = new(UnitLifecycle.Alive, new(cell, footprint)), Action = new CombatAction.Recovery(0, 0, 1000) });
                if (cell == 13) blockers.Add(id);
            }
        combat.StartActions([]);
        Assert.Equal(reachable, combat.Read(actor).TargetId);
        CombatAction.Moving move = Assert.IsType<CombatAction.Moving>(combat.Inspect(actor).Action);
        foreach (int id in blockers) combat.Remove(id);
        combat.StartActions([]);
        Assert.Same(move, combat.Inspect(actor).Action);
        combat.Advance(move.EndTick, []);
        combat.StartActions([]);
        Assert.Equal(nearer, combat.Read(actor).TargetId);
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
    public void OneExpansionScoresEveryOpponentAndOnlyWinnerNeedsRouteConstruction()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 17);
        foreach (int cell in new[] { 1, 2, 3, 4, 5, 6, 8 })
        {
            int id = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, cell);
            combat.Seed(combat.Inspect(id) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        }
        combat.StartActions([]);
        Assert.Equal(1, combat.RouteSearches);
        Assert.Equal(8, combat.Read(actor).Decision!.ObjectiveCell);
        Assert.IsType<CombatAction.Moving>(combat.Inspect(actor).Action);
        combat.StartActions([]);
        Assert.Equal(1, combat.RouteSearches);
    }

    [Fact]
    public void RetainedRepairUsesAtMostOneAdditionalExpansion()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 17);
        int target = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 2);
        combat.Seed(combat.Inspect(target) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        CombatUnit unit = combat.Inspect(actor);
        combat.Seed(unit with { Decision = unit.Decision with { ObjectiveId = target, ObjectiveCell = 2, Visited = [13] } });
        combat.StartActions([]);
        Assert.Equal(2, combat.RouteSearches);
        Assert.DoesNotContain(combat.Read(actor).Decision!.Route, p => p.Cell == 13);
    }

    [Fact]
    public void NewObjectiveResetsVisitedCellsAndCommittedMoveKeepsItsDestination()
    {
        using var combat = new CombatSimulation(new(), seed: 123);
        int actor = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 17);
        int old = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 2);
        int nearer = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 8);
        foreach (int id in new[] { old, nearer }) combat.Seed(combat.Inspect(id) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        CombatUnit unit = combat.Inspect(actor);
        combat.Seed(unit with { Decision = unit.Decision with { ObjectiveId = old, ObjectiveCell = 2, Visited = [13, 14, 16, 18] } });
        combat.StartActions([]);
        CombatAction.Moving move = Assert.IsType<CombatAction.Moving>(combat.Inspect(actor).Action);
        Assert.Empty(combat.Inspect(actor).Decision.Visited);
        Assert.Equal(nearer, combat.Inspect(actor).Decision.ObjectiveId);
        int incoming = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 11);
        combat.Seed(combat.Inspect(incoming) with { Action = new CombatAction.Recovery(0, 0, 1000) });
        combat.StartActions([]);
        Assert.Same(move, combat.Inspect(actor).Action);
        combat.Advance(move.EndTick, []);
        combat.StartActions([]);
        Assert.Equal(incoming, combat.Read(actor).TargetId);
    }

    [Fact]
    public void ObservationIsLocalAndIgnoresHealthAndActionOrdinals()
    {
        using var combat = new CombatSimulation(new());
        int actor = CombatDecisionRegressionTests.At(combat, Faction.Adventurers, 17);
        BattlefieldObservation Capture() => BattlefieldObservation.Capture(1, 4, combat.Reservations, combat.Units());
        BattlefieldObservation first = Capture();
        int other = combat.Create(UnitType.Swordsman, 2, 2, 2);
        combat.Seed(combat.Inspect(other) with { Location = new(UnitLifecycle.Alive, new(17, 7)) });
        Assert.True(first.Matches(Capture()));
        CombatUnit unit = combat.Inspect(actor);
        combat.Seed(unit with { Health = unit.Health - 100, Action = new CombatAction.Waiting(3) });
        Assert.True(first.Matches(Capture()));
        combat.Seed(combat.Inspect(actor) with { Location = new(UnitLifecycle.Alive, new(14, 7)) });
        Assert.False(first.Matches(Capture()));
    }
}
