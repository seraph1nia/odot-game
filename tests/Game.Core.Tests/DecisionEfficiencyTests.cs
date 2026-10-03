using Xunit;

namespace Game.Core.Tests;

public sealed class DecisionEfficiencyTests
{
    private sealed record Candidate(int Id, int Distance, int Initiative);

    [Fact]
    public void BestGroupMatchesStableFullOrderingForEverySeedAndInputOrder()
    {
        Candidate[] source = [new(8, 9, 1), new(3, 2, 4), new(9, 2, 4), new(7, 2, 5), new(1, 3, 0), new(3, 2, 4)];
        foreach (Candidate[] input in new[] { source, source.Reverse().ToArray(), source[..1], Array.Empty<Candidate>() })
            foreach (ulong seed in new ulong[] { 0, 1, 8, 123 })
            {
                var key = new CombatDecisionKey(seed, default, 1, 1, CombatActorKind.Unit, 1, 2, 3, CombatPurpose.Target);
                Candidate[] ordered = input.OrderBy(x => x.Distance).ThenBy(x => x.Initiative).ThenBy(x => x.Id).ToArray();
                Candidate? expected = ordered.Length == 0 ? null : ordered[SeededDecision.Choose(key,
                    ordered.TakeWhile(x => x.Distance == ordered[0].Distance && x.Initiative == ordered[0].Initiative).Count())];
                var work = new WorkCounters(); work.Support(WorkMetric.Sorts, WorkMetric.SortElements);
                Assert.Same(expected, CombatRanking.Select(input, x => x.Distance, x => x.Initiative, x => x.Id, key, work));
                Assert.True(work.Snapshot()[WorkMetric.SortElements] <= input.Length);
            }
    }

    [Fact]
    public void CapOneChecksArgumentsWithoutExaminingSplashCandidates()
    {
        using var combat = new CombatSimulation(new());
        int id = CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 8);
        CombatDecisionRegressionTests.At(combat, Faction.Skeletons, 9);
        CombatUnit primary = combat.Inspect(id);
        var work = new WorkCounters(); work.Support(WorkMetric.SplashCandidates);
        Assert.Equal(new[] { id }, CombatDecisions.Victims(combat.Board, primary, combat.Units(), 1, 100, default, work));
        Assert.Equal(0, work.Snapshot()[WorkMetric.SplashCandidates]);
        Assert.Throws<ArgumentOutOfRangeException>(() => CombatDecisions.Victims(combat.Board, primary, [], 1, -1, default, work));
        Assert.Throws<ArgumentOutOfRangeException>(() => CombatDecisions.Victims(combat.Board, primary, [], 0, 0, default, work));
    }

    [Fact]
    public void ActionStageSharesOneExactPayloadPerCity()
    {
        using var combat = new CombatSimulation(new(), seed: 1);
        for (int city = 1; city <= 2; city++)
        {
            foreach (var setup in new[] { (Faction.Adventurers, 17, 1), (Faction.Adventurers, 17, 2), (Faction.Skeletons, 2, 1) })
            {
                int id = combat.Create(UnitType.Swordsman, setup.Item1 == Faction.Adventurers ? city : 0, city, city, setup.Item1);
                UnitState unit = combat.Read(id);
                combat.Seed(unit with { Hex = CombatFixture.At(unit, setup.Item2, setup.Item3) });
            }
        }
        var work = new WorkCounters(); combat.Work = work;
        combat.StartActions([]);
        Assert.Equal(2, work.Snapshot()[WorkMetric.ObservationBuilds]);
        Assert.Equal(6, work.Snapshot()[WorkMetric.ObservationActorVisits]);
        CombatUnit[] units = combat.Units(); CombatReservations reservations = combat.Reservations;
        BattlefieldObservation initial = BattlefieldObservation.Capture(1, 7, reservations, units);
        Assert.True(initial.Matches(BattlefieldObservation.Capture(1, 7, reservations,
            units.Select(u => u with { Health = u.Health - 1 }).ToArray())));
        Assert.False(initial.Matches(initial with { DecisionSequence = 8 }));
    }

    [Fact]
    public void ReadOnlySearchRetainsAllIndependentShortestPredecessors()
    {
        var board = new HexBoard(HexBoardDefinition.Default());
        bool Allows(int cell) => cell is not (8 or 9);
        var search = new HexReachability(board, 17, Allows);
        var expected = new Dictionary<int, int> { [17] = 0 }; var queue = new Queue<int>(); queue.Enqueue(17);
        while (queue.TryDequeue(out int cell))
            foreach (HexCell neighbor in board.Cells.Where(n => n.Neighbors.Contains(cell)))
                if (Allows(neighbor.Id) && expected.TryAdd(neighbor.Id, expected[cell] + 1)) queue.Enqueue(neighbor.Id);
        Assert.Equal(expected.OrderBy(p => p.Key), search.Distances.OrderBy(p => p.Key));
        foreach ((int cell, int distance) in expected.Where(p => p.Value > 0))
            Assert.Equal(board.Cell(cell).Neighbors.Where(n => expected.TryGetValue(n, out int d) && d == distance - 1).Order(), search.Parents[cell].Order());
        Assert.Throws<NotSupportedException>(() => ((IDictionary<int, int>)search.Distances).Add(999, 1));
        Assert.Throws<NotSupportedException>(() => ((IList<int>)search.Parents.First().Value).Add(999));
    }
}
