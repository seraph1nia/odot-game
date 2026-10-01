namespace Game.Core;

internal sealed record Approach(int TargetId, bool City, int TargetCell, HexPosition[] Route, int StaticSteps);

// Integer graph/footprint queries only. A route has a finite simple sequence;
// temporary transit tokens are arbitrated at commit rather than making an
// opponent disappear from the list of feasible long-term objectives.
internal sealed class HexRouting(HexBoard board, HexOccupancy occupancy)
{
    public Approach Find(UnitState actor, UnitState? target, UnitState[] all, CombatDecisionKey key, int[] excluded)
    {
        HexUnitState state = actor.Hex!;
        int targetCell = target?.Hex!.Position.Cell ?? 0;
        int Distance(int cell) => target is null ? board.CityDistance(cell) : board.Distance(cell, targetCell);
        bool Goal(int cell) => Distance(cell) >= 1 && Distance(cell) <= actor.Profile.HexRange;
        HexCell[] legal = board.Cells.Where(c => board.Allows(actor.Faction, c.Id)).ToArray();
        int staticSteps = legal.Where(c => Goal(c.Id)).Select(c => board.Distance(state.Position.Cell, c.Id)).DefaultIfEmpty(int.MaxValue).Min();
        var lengths = new Dictionary<int, int> { [state.Position.Cell] = 0 };
        var parents = new Dictionary<int, List<int>>(); var queue = new Queue<int>(); queue.Enqueue(state.Position.Cell);
        var goals = new List<HexPosition>(); int shortest = int.MaxValue;
        while (queue.TryDequeue(out int cell))
        {
            int steps = lengths[cell];
            if (steps > shortest) break;
            if (steps > 0 && Goal(cell))
            {
                shortest = steps;
                goals.AddRange(occupancy.Free(state.City, state.Faction, cell, actor.Profile.CapacityCost).Select(f => new HexPosition(cell, f.Id)));
                continue;
            }
            foreach (int next in board.Cell(cell).Neighbors)
            {
                if (excluded.Contains(next) || !occupancy.Free(state.City, state.Faction, next, actor.Profile.CapacityCost).Any()) continue;
                if (!lengths.TryGetValue(next, out int existing))
                { lengths.Add(next, steps + 1); parents.Add(next, [cell]); queue.Enqueue(next); }
                else if (existing == steps + 1) parents[next].Add(cell);
            }
        }
        if (goals.Count == 0) return new(target?.Id ?? state.City, target is null, targetCell, [], staticSteps);
        bool Screened(HexPosition goal) => actor.Class != UnitClass.Melee && target is not null && all.Any(u => u.Id != actor.Id
            && u.Hex?.IsTargetable == true && u.Destination == actor.Destination && u.Faction == actor.Faction && u.Class == UnitClass.Melee
            && board.Distance(goal.Cell, u.Hex.Position.Cell) + board.Distance(u.Hex.Position.Cell, targetCell) == board.Distance(goal.Cell, targetCell));
        bool screened = goals.Any(Screened); var candidates = goals.Where(g => Screened(g) == screened).ToArray();
        int capacity = candidates.Min(g => occupancy.UsedCapacity(state.City, g.Cell));
        candidates = candidates.Where(g => occupancy.UsedCapacity(state.City, g.Cell) == capacity).OrderBy(g => g.Cell).ThenBy(g => g.Footprint).ToArray();
        HexPosition chosen = candidates[SeededDecision.Choose(key with { Purpose = CombatPurpose.GoalFootprint }, candidates.Length)];
        var reversed = new List<HexPosition> { chosen }; int current = chosen.Cell;
        while (lengths[current] > 1)
        {
            int[] predecessors = parents[current].Order().ToArray();
            current = predecessors[SeededDecision.Choose(key with { Purpose = CombatPurpose.Route, Generation = checked(key.Generation + lengths[current]) }, predecessors.Length)];
            HexFootprint[] footprints = occupancy.Free(state.City, state.Faction, current, actor.Profile.CapacityCost).OrderBy(f => f.Id).ToArray();
            HexFootprint footprint = footprints[SeededDecision.Choose(key with { Purpose = CombatPurpose.Route, Generation = checked(key.Generation + current) }, footprints.Length)];
            reversed.Add(new(current, footprint.Id));
        }
        reversed.Reverse();
        return new(target?.Id ?? state.City, target is null, targetCell, reversed.ToArray(), staticSteps);
    }
}
