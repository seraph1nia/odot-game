using System.Collections.Frozen;
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
    public HexReachability(HexBoard board, int source, Func<int, bool> allows)
    {
        var distances = new Dictionary<int, int> { [source] = 0 };
        var parents = new Dictionary<int, List<int>>(); var queue = new Queue<int>(); queue.Enqueue(source);
        while (queue.TryDequeue(out int cell))
        {
            int steps = distances[cell] + 1;
            foreach (int next in board.Cell(cell).Neighbors)
            {
                if (!allows(next)) continue;
                if (!distances.TryGetValue(next, out int existing))
                { distances.Add(next, steps); parents.Add(next, [cell]); queue.Enqueue(next); }
                else if (existing == steps) parents[next].Add(cell);
            }
        }
        Distances = distances.ToFrozenDictionary();
        Parents = parents.ToFrozenDictionary(p => p.Key, p => Array.AsReadOnly(p.Value.ToArray()));
    }
}

internal sealed class HexRouting(HexBoard board, HexOccupancy occupancy)
{
    private long _revision = -1;
    private readonly Dictionary<(int City, Faction Faction, int Capacity), Dictionary<int, HexFootprint[]>> _available = [];
    internal int Searches { get; private set; }
    private Dictionary<int, HexFootprint[]> Available(CombatUnit actor)
    {
        if (_revision != occupancy.Revision) { _available.Clear(); _revision = occupancy.Revision; }
        var key = (actor.Destination, actor.Faction, actor.Profile.CapacityCost);
        if (!_available.TryGetValue(key, out Dictionary<int, HexFootprint[]>? cells))
        {
            cells = board.Cells.ToDictionary(c => c.Id,
                c => occupancy.Free(actor.Destination, actor.Faction, c.Id, actor.Profile.CapacityCost).OrderBy(f => f.Id).ToArray());
            _available.Add(key, cells);
        }
        return cells;
    }
    public void Clear() { _available.Clear(); _revision = -1; Searches = 0; }
    public HexReachability Search(CombatUnit actor, int[] excluded)
    {
        Dictionary<int, HexFootprint[]> available = Available(actor); Searches++;
        return new(board, actor.Location.Position.Cell, cell => available[cell].Length > 0 && !excluded.Contains(cell));
    }
    private bool Goal(CombatUnit actor, CombatTarget target, int targetCell, int cell)
    {
        int distance = target.Kind == CombatTargetKind.City ? board.CityDistance(cell) : board.Distance(cell, targetCell);
        return distance >= 1 && distance <= actor.Profile.HexRange;
    }
    public ApproachScore Score(CombatUnit actor, CombatTarget target, int targetCell, int initiative, HexReachability search)
    {
        int steps = search.Distances.Where(p => p.Value > 0 && Goal(actor, target, targetCell, p.Key))
            .Select(p => p.Value).DefaultIfEmpty(int.MaxValue).Min();
        int staticSteps = board.Cells.Where(c => board.Allows(actor.Faction, c.Id) && Goal(actor, target, targetCell, c.Id))
            .Select(c => board.MovementDistance(actor.Faction, actor.Location.Position.Cell, c.Id)).DefaultIfEmpty(int.MaxValue).Min();
        return new(target, targetCell, initiative, steps, staticSteps);
    }
    public static ApproachScore Select(ApproachScore[] scores, CombatDecisionKey key)
    {
        ApproachScore[] candidates = scores.Where(s => s.Reachable).ToArray();
        bool feasible = candidates.Length > 0;
        if (!feasible) candidates = scores;
        return CombatRanking.Select(candidates, s => feasible ? s.Steps : s.StaticSteps, s => s.Initiative, s => s.Target.Id, key)
            ?? throw new ArgumentException("At least one objective is required.", nameof(scores));
    }
    public HexPosition[] Route(CombatUnit actor, ApproachScore objective, CombatUnit[] all, CombatDecisionKey key, HexReachability search)
    {
        Dictionary<int, HexFootprint[]> available = Available(actor);
        int shortest = search.Distances.Where(p => p.Value > 0 && Goal(actor, objective.Target, objective.Cell, p.Key))
            .Select(p => p.Value).DefaultIfEmpty(int.MaxValue).Min();
        if (shortest == int.MaxValue) return [];
        HexPosition[] goals = search.Distances.Where(p => p.Value == shortest && Goal(actor, objective.Target, objective.Cell, p.Key))
            .SelectMany(p => available[p.Key].Select(f => new HexPosition(p.Key, f.Id))).ToArray();
        bool Screened(HexPosition goal) => actor.Class != UnitClass.Melee && objective.Target.Kind == CombatTargetKind.Unit && all.Any(u => u.Id != actor.Id
            && u.IsTargetable && u.Destination == actor.Destination && u.Faction == actor.Faction && u.Class == UnitClass.Melee
            && board.Distance(goal.Cell, u.Location.Position.Cell) + board.Distance(u.Location.Position.Cell, objective.Cell) == board.Distance(goal.Cell, objective.Cell));
        bool screened = goals.Any(Screened); var candidates = goals.Where(g => Screened(g) == screened).ToArray();
        int capacity = candidates.Min(g => occupancy.UsedCapacity(actor.Destination, g.Cell));
        candidates = candidates.Where(g => occupancy.UsedCapacity(actor.Destination, g.Cell) == capacity).OrderBy(g => g.Cell).ThenBy(g => g.Footprint).ToArray();
        HexPosition chosen = candidates[SeededDecision.Choose(key with { Purpose = CombatPurpose.GoalFootprint }, candidates.Length)];
        var reversed = new List<HexPosition> { chosen }; int current = chosen.Cell;
        while (search.Distances[current] > 1)
        {
            int[] predecessors = search.Parents[current].Order().ToArray();
            current = predecessors[SeededDecision.Choose(key with { Purpose = CombatPurpose.Route, Generation = checked(key.Generation + search.Distances[current]) }, predecessors.Length)];
            HexFootprint[] footprints = available[current];
            HexFootprint footprint = footprints[SeededDecision.Choose(key with { Purpose = CombatPurpose.Route, Generation = checked(key.Generation + current) }, footprints.Length)];
            reversed.Add(new(current, footprint.Id));
        }
        reversed.Reverse();
        return reversed.ToArray();
    }
}
