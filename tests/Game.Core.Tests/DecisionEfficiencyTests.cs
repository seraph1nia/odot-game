using Xunit;

namespace Game.Core.Tests;

public sealed class DecisionEfficiencyTests
{
    // Retain the immutable graph contract. Unused member-ranking/capped splash
    // and city-local observation tests are retired; CombatPolicyTests asserts
    // actual shared actor decisions and their measured one-expansion work bound.
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
