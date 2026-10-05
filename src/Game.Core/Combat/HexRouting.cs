using System.Collections.ObjectModel;

namespace Game.Core;

internal sealed record ApproachScore(CombatTarget Target, int Cell, int Initiative, int Steps, int StaticSteps)
{
    public bool Reachable => Steps != int.MaxValue;
}

// One immutable reachability view is shared by every opponent of an actor.
// Temporary transit conflicts are checked by occupancy only when committing.
internal sealed class HexReachability
{
    public IReadOnlyDictionary<int, int> Distances { get; }
    public IReadOnlyDictionary<int, ReadOnlyCollection<int>> Parents { get; }
    internal Dictionary<(Faction Faction, int Source, int Range, CombatTargetKind Kind, int Cell), (int Steps, int StaticSteps)> Scores { get; } = [];
    public HexReachability(HexBoard board, int source, Func<int, bool> allows, WorkCounters? work = null)
    {
        work?.Add(WorkMetric.BfsSearches);
        var distances = new Dictionary<int, int> { [source] = 0 };
        var parents = new Dictionary<int, List<int>>(); var queue = new Queue<int>(); queue.Enqueue(source);
        while (queue.TryDequeue(out int cell))
        {
            work?.Add(WorkMetric.BfsDequeues);
            int steps = distances[cell] + 1;
            foreach (int next in board.Cell(cell).Neighbors)
            {
                work?.Add(WorkMetric.BfsEdges);
                if (!allows(next)) continue;
                if (!distances.TryGetValue(next, out int existing))
                { distances.Add(next, steps); parents.Add(next, [cell]); queue.Enqueue(next); }
                else if (existing == steps) parents[next].Add(cell);
            }
        }
        Distances = new ReadOnlyDictionary<int, int>(distances);
        Parents = new ReadOnlyDictionary<int, ReadOnlyCollection<int>>(parents.ToDictionary(p => p.Key, p => Array.AsReadOnly(p.Value.ToArray())));
    }
}

internal sealed class HexRouting(HexBoard board, HexOccupancy occupancy)
{
    private WorkCounters? _work;
    internal WorkCounters? Work
    {
        get => _work;
        set { _work = value; value?.Support(WorkMetric.BfsSearches, WorkMetric.BfsDequeues, WorkMetric.BfsEdges, WorkMetric.Sorts, WorkMetric.SortElements); }
    }
    private long _revision = -1;
    private readonly Dictionary<(int City, Faction Faction, int Capacity), Dictionary<int, HexAnchor[]>> _available = [];
    private readonly Dictionary<(Faction Faction, int Range, CombatTargetKind Kind, int Cell), (int[] All, int[] Legal)> _goalCells = [];
    internal int Searches { get; private set; }
    private Dictionary<int, HexAnchor[]> Available(CombatUnit actor)
    {
        if (_revision != occupancy.Revision) { _available.Clear(); _revision = occupancy.Revision; }
        var key = (actor.Destination, actor.Faction, actor.Profile.Size);
        if (!_available.TryGetValue(key, out Dictionary<int, HexAnchor[]>? cells))
        {
            cells = board.Cells.ToDictionary(c => c.Id,
                c => WorkOrdering.Input(occupancy.Free(actor.Destination, actor.Faction, c.Id, actor.Profile.Size), Work).OrderBy(f => f.Id).ToArray());
            _available.Add(key, cells);
        }
        return cells;
    }
    public void Clear() { _available.Clear(); _goalCells.Clear(); _revision = -1; Searches = 0; }
    public HexReachability Search(CombatUnit actor, int[] excluded)
    {
        Dictionary<int, HexAnchor[]> available = Available(actor); Searches++;
        return new(board, actor.Location.Position.Cell, cell => available[cell].Length > 0 && !excluded.Contains(cell), Work);
    }
    private bool Goal(CombatUnit actor, CombatTarget target, int targetCell, int cell)
    {
        int distance = target.Kind == CombatTargetKind.City ? board.CityDistance(cell) : board.Distance(cell, targetCell);
        return distance >= 1 && distance <= actor.Profile.HexRange;
    }
    private (int[] All, int[] Legal) GoalCells(CombatUnit actor, CombatTarget target, int targetCell)
    {
        var key = (actor.Faction, actor.Profile.HexRange, target.Kind, targetCell);
        if (_goalCells.TryGetValue(key, out var cells)) return cells;
        int[] all = board.Cells.Where(c => Goal(actor, target, targetCell, c.Id)).Select(c => c.Id).ToArray();
        cells = (all, all.Where(cell => board.Allows(actor.Faction, cell)).ToArray());
        // Normal configuration has four archetypes. Arbitrary direct fixtures
        // can fall back to exact construction without unbounded cache growth.
        if (_goalCells.Count < 8 * (board.Cells.Count + 1)) _goalCells.Add(key, cells);
        return cells;
    }
    public ApproachScore Score(CombatUnit actor, CombatTarget target, int targetCell, int initiative, HexReachability search)
    {
        var key = (actor.Faction, actor.Location.Position.Cell, actor.Profile.HexRange, target.Kind, targetCell);
        if (search.Scores.TryGetValue(key, out var cached)) return new(target, targetCell, initiative, cached.Steps, cached.StaticSteps);
        var goals = GoalCells(actor, target, targetCell);
        int steps = int.MaxValue;
        foreach (int cell in goals.All)
        {
            int distance = search.Distances.GetValueOrDefault(cell, int.MaxValue);
            if (distance > 0 && distance < steps) steps = distance;
            // Zero is deliberately excluded: one is the exact lower bound.
            if (steps == 1) break;
        }
        int staticSteps = int.MaxValue;
        foreach (int cell in goals.Legal)
        {
            staticSteps = Math.Min(staticSteps, board.MovementDistance(actor.Faction, actor.Location.Position.Cell, cell));
            if (staticSteps == 0) break;
        }
        search.Scores.Add(key, (steps, staticSteps));
        return new(target, targetCell, initiative, steps, staticSteps);
    }
    public static ApproachScore Select(ApproachScore[] scores, CombatDecisionKey key, WorkCounters? work = null)
    {
        bool feasible = scores.Any(s => s.Reachable);
        IEnumerable<ApproachScore> candidates = feasible ? scores.Where(s => s.Reachable) : scores;
        return CombatRanking.Select(candidates, s => feasible ? s.Steps : s.StaticSteps, s => s.Initiative, s => s.Target.Id, key, work)
            ?? throw new ArgumentException("At least one objective is required.", nameof(scores));
    }
    public HexPosition[] Route(CombatUnit actor, ApproachScore objective, CombatUnit[] all, CombatDecisionKey key, HexReachability search)
    {
        Dictionary<int, HexAnchor[]> available = Available(actor);
        int shortest = search.Distances.Where(p => p.Value > 0 && Goal(actor, objective.Target, objective.Cell, p.Key))
            .Select(p => p.Value).DefaultIfEmpty(int.MaxValue).Min();
        if (shortest == int.MaxValue) return [];
        HexPosition[] goals = search.Distances.Where(p => p.Value == shortest && Goal(actor, objective.Target, objective.Cell, p.Key))
            .SelectMany(p => available[p.Key].Select(f => new HexPosition(p.Key, f.Id))).ToArray();
        CombatUnit[] screens = actor.Class != UnitClass.Melee && objective.Target.Kind == CombatTargetKind.Unit
            ? all.Where(u => u.Id != actor.Id && u.IsTargetable && u.Destination == actor.Destination && u.Faction == actor.Faction && u.Class == UnitClass.Melee).ToArray() : [];
        var screenedCells = new Dictionary<int, bool>();
        bool Screened(HexPosition goal)
        {
            if (screenedCells.TryGetValue(goal.Cell, out bool cached)) return cached;
            bool result = screens.Any(u => board.Distance(goal.Cell, u.Location.Position.Cell) + board.Distance(u.Location.Position.Cell, objective.Cell) == board.Distance(goal.Cell, objective.Cell));
            screenedCells.Add(goal.Cell, result); return result;
        }
        bool screened = goals.Any(Screened); var candidates = goals.Where(g => Screened(g) == screened).ToArray();
        int capacity = candidates.Min(g => occupancy.UsedCapacity(actor.Destination, g.Cell));
        candidates = WorkOrdering.Input(candidates.Where(g => occupancy.UsedCapacity(actor.Destination, g.Cell) == capacity), Work).OrderBy(g => g.Cell).ThenBy(g => g.Anchor).ToArray();
        HexPosition chosen = candidates[SeededDecision.Choose(key with { Purpose = CombatPurpose.GoalAnchor }, candidates.Length)];
        var reversed = new List<HexPosition> { chosen }; int current = chosen.Cell;
        while (search.Distances[current] > 1)
        {
            int[] predecessors = WorkOrdering.Input(search.Parents[current], Work).Order().ToArray();
            current = predecessors[SeededDecision.Choose(key with { Purpose = CombatPurpose.Route, Generation = checked(key.Generation + search.Distances[current]) }, predecessors.Length)];
            HexAnchor[] anchors = available[current];
            HexAnchor anchor = anchors[SeededDecision.Choose(key with { Purpose = CombatPurpose.Route, Generation = checked(key.Generation + current) }, anchors.Length)];
            reversed.Add(new(current, anchor.Id));
        }
        reversed.Reverse();
        return reversed.ToArray();
    }
}
