namespace Game.Core;

internal static class CombatRanking
{
    public static T? Select<T>(IEnumerable<T> candidates, Func<T, int> distance, Func<T, int> initiative, Func<T, int> identity, CombatDecisionKey key, WorkCounters? work = null) where T : class
    {
        var best = new List<T>();
        int bestDistance = int.MaxValue, bestInitiative = int.MaxValue;
        foreach (T value in candidates)
        {
            int d = distance(value), i = initiative(value);
            if (best.Count == 0 || d < bestDistance || d == bestDistance && i < bestInitiative)
            { best.Clear(); bestDistance = d; bestInitiative = i; }
            if (d == bestDistance && i == bestInitiative) best.Add(value);
        }
        if (best.Count == 0) return null;
        T[] ties = WorkOrdering.Input(best, work).OrderBy(identity).ToArray();
        return ties[SeededDecision.Choose(key, ties.Length)];
    }
}

internal static class CombatDecisions
{
    public static CombatUnit? Select(IEnumerable<CombatUnit> candidates, Func<CombatUnit, int> distance, CombatDecisionKey key, WorkCounters? work = null)
        => CombatRanking.Select(candidates, distance, u => u.Profile.Initiative, u => u.Id, key, work);
    public static bool InRange(HexBoard board, CombatUnit actor, CombatUnit target, WorkCounters? work = null)
    {
        work?.Add(WorkMetric.RangeCandidates);
        int distance = board.Distance(actor.Location.Position.Cell, target.Location.Position.Cell);
        return distance >= 1 && distance <= actor.Profile.HexRange;
    }
    public static int[] Victims(HexBoard board, CombatUnit primary, CombatUnit[] all, int cap, int radius, CombatDecisionKey key, WorkCounters? work = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cap); ArgumentOutOfRangeException.ThrowIfNegative(radius);
        if (cap == 1) return [primary.Id];
        var result = new List<int> { primary.Id };
        work?.Add(WorkMetric.SplashCandidates, all.Length);
        var remaining = all.Where(u => u.IsTargetable && u.Id != primary.Id && u.Destination == primary.Destination && u.Faction == primary.Faction
            && board.Distance(primary.Location.Position.Cell, u.Location.Position.Cell) <= radius).ToList();
        while (result.Count < cap && remaining.Count > 0)
        {
            CombatUnit victim = Select(remaining, u => board.Distance(primary.Location.Position.Cell, u.Location.Position.Cell), key with { Generation = result.Count }, work)!;
            result.Add(victim.Id); remaining.Remove(victim);
        }
        return result.ToArray();
    }
}

// Exact local inputs, not a hash or a global revision. Changes in another city,
// health without a casualty, and mere observation cannot invalidate this view.
internal sealed record BattlefieldObservation(long DecisionSequence, PositionReservation[] Positions, ObservedActor[] Actors)
{
    public static BattlefieldObservation Capture(int city, long sequence, CombatReservations reservations, CombatUnit[] units, WorkCounters? work = null)
    {
        work?.Add(WorkMetric.ObservationBuilds); work?.Add(WorkMetric.ObservationActorVisits, units.Length); work?.Add(WorkMetric.ObservationReservationVisits, reservations.Positions.Length);
        return new(sequence, reservations.Positions.Where(p => p.City == city).Select(p => p with { ActionSequence = 0 }).ToArray(),
            units.Where(u => u.Destination == city).Select(u => new ObservedActor(u.Id, u.Faction, u.Class, u.Location.Position.Cell, u.Profile.Initiative)).ToArray());
    }
    public bool Matches(BattlefieldObservation other) => DecisionSequence == other.DecisionSequence
        && (ReferenceEquals(Positions, other.Positions) || Positions.SequenceEqual(other.Positions))
        && (ReferenceEquals(Actors, other.Actors) || Actors.SequenceEqual(other.Actors));
}
internal readonly record struct ObservedActor(int Id, Faction Faction, UnitClass Class, int Cell, int Initiative);
