namespace Game.Core;

internal static class CombatRanking
{
    public static T? Select<T>(IEnumerable<T> candidates, Func<T, int> distance, Func<T, int> initiative, Func<T, int> identity, CombatDecisionKey key) where T : class
    {
        T[] values = candidates.OrderBy(distance).ThenBy(initiative).ThenBy(identity).ToArray();
        if (values.Length == 0) return null;
        int bestDistance = distance(values[0]), bestInitiative = initiative(values[0]);
        int ties = values.TakeWhile(value => distance(value) == bestDistance && initiative(value) == bestInitiative).Count();
        return values[SeededDecision.Choose(key, ties)];
    }
}

internal static class CombatDecisions
{
    public static CombatUnit? Select(IEnumerable<CombatUnit> candidates, Func<CombatUnit, int> distance, CombatDecisionKey key)
        => CombatRanking.Select(candidates, distance, u => u.Profile.Initiative, u => u.Id, key);
    public static bool InRange(HexBoard board, CombatUnit actor, CombatUnit target)
    {
        int distance = board.Distance(actor.Location.Position.Cell, target.Location.Position.Cell);
        return distance >= 1 && distance <= actor.Profile.HexRange;
    }
    public static int[] Victims(HexBoard board, CombatUnit primary, CombatUnit[] all, int cap, int radius, CombatDecisionKey key)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cap); ArgumentOutOfRangeException.ThrowIfNegative(radius);
        var result = new List<int> { primary.Id };
        var remaining = all.Where(u => u.IsTargetable && u.Id != primary.Id && u.Destination == primary.Destination && u.Faction == primary.Faction
            && board.Distance(primary.Location.Position.Cell, u.Location.Position.Cell) <= radius).ToList();
        while (result.Count < cap && remaining.Count > 0)
        {
            CombatUnit victim = Select(remaining, u => board.Distance(primary.Location.Position.Cell, u.Location.Position.Cell), key with { Generation = result.Count })!;
            result.Add(victim.Id); remaining.Remove(victim);
        }
        return result.ToArray();
    }
}

// Exact local inputs, not a hash or a global revision. Changes in another city,
// health without a casualty, and mere observation cannot invalidate this view.
internal sealed record BattlefieldObservation(long DecisionSequence, PositionReservation[] Positions, ObservedActor[] Actors)
{
    public static BattlefieldObservation Capture(int city, long sequence, CombatReservations reservations, CombatUnit[] units)
        => new(sequence, reservations.Positions.Where(p => p.City == city).Select(p => p with { ActionSequence = 0 }).ToArray(),
            units.Where(u => u.Destination == city).Select(u => new ObservedActor(u.Id, u.Faction, u.Class, u.Location.Position.Cell, u.Profile.Initiative)).ToArray());
    public bool Matches(BattlefieldObservation other) => DecisionSequence == other.DecisionSequence
        && Positions.SequenceEqual(other.Positions) && Actors.SequenceEqual(other.Actors);
}
internal readonly record struct ObservedActor(int Id, Faction Faction, UnitClass Class, int Cell, int Initiative);
