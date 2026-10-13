using Xunit;

namespace Game.Core.Tests;

// Tier A replacement for per-member, city-isolated/size-budget routing. Real
// SharedArena routes must respect footprints and temporary blockers yet cross
// sector boundaries. Fixed topology, no child resources; estimated <1s.
public sealed class HexRoutingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public void ReachableRoutesAreShortestAdjacentAndNeverUseStaticOrOpposingCells(int players)
    {
        var arena = new SharedArena(Enumerable.Range(1, players));
        int source = arena.Deployment(1, 0), target = arena.Keep(players);
        var work = new WorkCounters(); work.Support(WorkMetric.BfsSearches, WorkMetric.BfsDequeues, WorkMetric.BfsEdges);
        IReadOnlyDictionary<int, int[]> routes = arena.Routes(source, [target, arena.Watchpost(1)], 1, _ => true, work);
        int[] direct = routes[target];
        Assert.Equal(2, routes.Count); Assert.Equal(1, work.Snapshot()[WorkMetric.BfsSearches]);
        Assert.NotEmpty(direct);
        var blockers = new HashSet<int> { direct[0] };
        int[] detour = arena.Routes(source, [target], 1, cell => !blockers.Contains(cell))[target];
        Assert.NotEmpty(detour); Assert.DoesNotContain(detour, blockers.Contains);
        Assert.Equal(detour, arena.Routes(source, [target], 1, cell => !blockers.Contains(cell))[target]);
        int previous = source;
        foreach (int cell in detour)
        {
            Assert.Contains(cell, arena.Board.Cell(previous).Neighbors);
            Assert.True(arena.CanOccupy(cell)); previous = cell;
        }
        Assert.Equal(detour.Length, detour.Distinct().Count());
        Assert.Equal(1, arena.Board.Distance(detour[^1], target));
        // Geometry oracle preserves independent shortest-distance evidence from
        // retired RoutingScoreEfficiency without obsolete goal/size cache policy.
        var distances = new Dictionary<int, int> { [source] = 0 };
        var pending = new Queue<int>(); pending.Enqueue(source);
        while (pending.TryDequeue(out int current))
            foreach (int neighbor in arena.Board.Cell(current).Neighbors)
                if (arena.CanOccupy(neighbor) && !blockers.Contains(neighbor) && distances.TryAdd(neighbor, distances[current] + 1))
                    pending.Enqueue(neighbor);
        int shortest = distances.Where(p => arena.Board.Distance(p.Key, target) == 1).Min(p => p.Value);
        Assert.Equal(shortest, detour.Length);
        if (players == 4) Assert.True(arena.Board.Cell(detour[^1]).Coordinate.Column >= 21);
    }

    [Fact]
    public void BlockedRouteCannotEnterOccupiedGoalAndBecomesAvailableWhenBlockersLeave()
    {
        var arena = new SharedArena([1]); int source = arena.Deployment(1, 3), target = arena.Watchpost(1);
        Assert.NotEmpty(arena.Routes(source, [target], 1, _ => true)[target]);
        Assert.Empty(arena.Routes(source, [target], 1, _ => false)[target]);
        Assert.NotEmpty(arena.Routes(source, [target], 1, _ => true)[target]);
        Assert.Empty(arena.Routes(source, [target], arena.Board.Diameter, _ => false)[target]);
        Assert.Throws<ArgumentException>(() => arena.Routes(arena.Keep(1), [target], 1, _ => true));
        Assert.Throws<ArgumentException>(() => arena.Routes(source, [target], 0, _ => true));
    }
}
