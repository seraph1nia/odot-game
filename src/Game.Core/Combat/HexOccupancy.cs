namespace Game.Core;

public enum UnitLifecycle { Queued, Alive, Dying, Reserve, Stored }
public enum UnitActionKind { Waiting, Moving, Windup, Recovery }
public readonly record struct HexPosePoint(HexPosition Position, HexPosition Destination = default, int Transition = 0, int ElapsedTicks = 0, int DurationTicks = 0);
public sealed record HexUnitState(int Id, int City, Faction Faction, UnitLifecycle Lifecycle, HexPosition Position,
    UnitActionKind Action = UnitActionKind.Waiting, HexPosition Destination = default, int Transition = 0,
    long ActionSequence = 0, long StartTick = 0, long EndTick = 0, long DeathStartTick = 0, long DeathEndTick = 0, int FrozenMoveTicks = 0,
    long AdmittedTick = 0, long? FrozenTick = null, HexPosePoint? FrozenAim = null, int Size = 2)
{
    public bool HoldsTransit => Action == UnitActionKind.Moving;
    public bool IsTargetable => Lifecycle == UnitLifecycle.Alive;
}
public readonly record struct PositionReservation(int City, int Cell, int Anchor, int UnitId, long ActionSequence, Faction Faction, int Size);
public readonly record struct TransitReservation(int City, int SourceCell, int DestinationCell, int EdgeToken, int UnitId, long ActionSequence);
public sealed record CombatReservations(PositionReservation[] Positions, TransitReservation[] Transit)
{
    public static CombatReservations Reconstruct(HexBoard board, IEnumerable<HexUnitState> units)
    {
        var occupancy = new HexOccupancy(board); occupancy.Rebuild(units.Select(ReservationOwner.FromSnapshot));
        return occupancy.Snapshot();
    }
}

// An auxiliary index, not a second unit store. ECS owns the states supplied to
// rebuild/commit; the index records only per-actor size claims and action ownership.
// Its complete projection is independently derivable from those current states.
internal sealed class HexOccupancy(HexBoard board)
{
    private WorkCounters? _work;
    internal WorkCounters? Work
    {
        get => _work;
        set { _work = value; value?.Support(WorkMetric.OccupancyChecks, WorkMetric.Sorts, WorkMetric.SortElements); }
    }
    private readonly Dictionary<(int City, int Cell), Dictionary<int, PositionReservation>> _positions = [];
    private readonly Dictionary<(int City, int Cell), TransitReservation> _endpointTransit = [];
    private readonly Dictionary<(int City, int Edge), TransitReservation> _edgeTransit = [];
    public long Revision { get; private set; }

    public int UsedCapacity(int city, int cell) => _positions.TryGetValue((city, cell), out var occupants)
        ? occupants.Values.Aggregate(0, (total, r) => checked(total + r.Size)) : 0;
    public bool CanPlace(int city, Faction faction, HexPosition position, int size)
    {
        Work?.Add(WorkMetric.OccupancyChecks);
        if (size is < 1 or > 6 || city <= 0 || !Enum.IsDefined(faction) || !board.Allows(faction, position.Cell)) return false;
        _ = board.Anchor(position.Anchor);
        return checked(UsedCapacity(city, position.Cell) + size) <= board.Capacity
            && (!_positions.TryGetValue((city, position.Cell), out var occupants)
                || occupants.Values.All(r => r.Faction == faction && r.Anchor != position.Anchor));
    }
    public IEnumerable<HexAnchor> Free(int city, Faction faction, int cell, int size)
        => board.Anchors.Where(f => CanPlace(city, faction, new(cell, f.Id), size));
    public bool TryPlace(ReservationOwner state)
    {
        if (state.Id <= 0 || state.Lifecycle != UnitLifecycle.Alive || state.HoldsTransit || Contains(state.Id)
            || !CanPlace(state.City, state.Faction, state.Position, state.Size)) return false;
        AddPosition(new(state.City, state.Position.Cell, state.Position.Anchor, state.Id, state.ActionSequence, state.Faction, state.Size));
        Revision = checked(Revision + 1); return true;
    }
    public bool TryMove(ReservationOwner state, HexPosition destination, long sequence)
    {
        // All validation precedes all mutations. The caller commits the matching
        // ECS action only after this transaction succeeds.
        if (state.Lifecycle != UnitLifecycle.Alive || !state.Ready || sequence <= state.ActionSequence
            || !_positions.TryGetValue((state.City, state.Position.Cell), out var source)
            || !source.TryGetValue(state.Id, out PositionReservation reservation) || reservation.Anchor != state.Position.Anchor || reservation.ActionSequence != state.ActionSequence
            || reservation.Faction != state.Faction || !board.Cell(state.Position.Cell).Neighbors.Contains(destination.Cell)
            || reservation.Size != state.Size
            || !CanPlace(state.City, state.Faction, destination, state.Size)) return false;
        HexTransition transition = board.Transition(state.Position, destination);
        if (_endpointTransit.ContainsKey((state.City, state.Position.Cell)) || _endpointTransit.ContainsKey((state.City, destination.Cell))
            || _edgeTransit.ContainsKey((state.City, transition.EdgeToken))) return false;
        var transit = new TransitReservation(state.City, state.Position.Cell, destination.Cell, transition.EdgeToken, state.Id, sequence);
        source[state.Id] = reservation with { ActionSequence = sequence };
        AddPosition(new(state.City, destination.Cell, destination.Anchor, state.Id, sequence, state.Faction, state.Size));
        _endpointTransit.Add((state.City, state.Position.Cell), transit); _endpointTransit.Add((state.City, destination.Cell), transit);
        _edgeTransit.Add((state.City, transition.EdgeToken), transit);
        Revision = checked(Revision + 1); return true;
    }
    public bool TryAction(ReservationOwner before, CombatAction action)
    {
        if (action is CombatAction.Moving move)
        {
            if (board.Transition(before.Position, move.Destination).Id != move.Transition) return false;
            return TryMove(before, move.Destination, action.Sequence);
        }
        if (action is not CombatAction.Windup || before.Lifecycle != UnitLifecycle.Alive || !before.Ready || before.HoldsTransit || action.Sequence <= before.ActionSequence
            || !_positions.TryGetValue((before.City, before.Position.Cell), out var positions)
            || !positions.TryGetValue(before.Id, out PositionReservation reservation)
            || reservation.ActionSequence != before.ActionSequence || reservation.Anchor != before.Position.Anchor || reservation.Size != before.Size || reservation.Faction != before.Faction)
            return false;
        positions[before.Id] = reservation with { ActionSequence = action.Sequence };
        return true;
    }
    public bool Arrive(ReservationOwner state, long tick)
    {
        if (state.Lifecycle != UnitLifecycle.Alive || !state.HoldsTransit || tick < state.EndTick) return false;
        if (!_positions.TryGetValue((state.City, state.Destination.Cell), out var destination)
            || !destination.TryGetValue(state.Id, out PositionReservation reservation) || reservation.ActionSequence != state.ActionSequence)
            throw new InvalidOperationException("Arrival lacks its committed destination reservation.");
        RemovePosition(state.City, state.Position.Cell, state.Id); ReleaseTransit(state.City, state.Id, state.ActionSequence);
        Revision = checked(Revision + 1); return true;
    }
    public void Release(int city, int id)
    {
        bool changed = false;
        foreach (var key in _positions.Keys.Where(k => k.City == city).ToArray())
            changed |= RemovePosition(key.City, key.Cell, id);
        foreach (TransitReservation transit in _edgeTransit.Values.Where(t => t.City == city && t.UnitId == id).ToArray())
        { ReleaseTransit(city, id, transit.ActionSequence); changed = true; }
        if (changed) Revision = checked(Revision + 1);
    }
    public bool ExpireDeath(ReservationOwner state, long tick)
    {
        if (state.Lifecycle != UnitLifecycle.Dying || tick < state.DeathEndTick) return false;
        Release(state.City, state.Id); return true;
    }
    public CombatReservations Snapshot()
    {
        Work?.Add(WorkMetric.Sorts, 2); Work?.Add(WorkMetric.SortElements, _positions.Values.Sum(p => p.Count) + _edgeTransit.Count);
        return new(_positions.Values.SelectMany(p => p.Values).OrderBy(p => p.City).ThenBy(p => p.Cell).ThenBy(p => p.UnitId).ToArray(),
        _edgeTransit.Values.OrderBy(t => t.City).ThenBy(t => t.EdgeToken).ThenBy(t => t.UnitId).ToArray());
    }
    public void Rebuild(IEnumerable<ReservationOwner> units)
    {
        // Build separately so a malformed restoration cannot partially replace
        // the current valid index. Queued/dying data needs no event history.
        var rebuilt = new HexOccupancy(board); var identities = new HashSet<int>();
        foreach (ReservationOwner state in WorkOrdering.Input(units, Work).OrderBy(s => s.Id))
        {
            if (state.Id <= 0 || state.City <= 0 || !Enum.IsDefined(state.Faction) || !Enum.IsDefined(state.Lifecycle)
                || state.Size is < 1 or > 6 || state.ActionSequence < 0 || !identities.Add(state.Id))
                throw new ArgumentException("Invalid or duplicate unit identities.", nameof(units));
            if (state.Lifecycle is UnitLifecycle.Queued or UnitLifecycle.Reserve or UnitLifecycle.Stored)
            {
                if (state.HoldsTransit) throw new ArgumentException("Inactive/queued actors cannot retain transit.", nameof(units));
                continue;
            }
            if (state.Lifecycle == UnitLifecycle.Dying && (state.DeathStartTick < 0 || state.DeathEndTick <= state.DeathStartTick))
                throw new ArgumentException("Invalid retained death interval.", nameof(units));
            if (state.HoldsTransit && (state.StartTick < 0 || state.EndTick <= state.StartTick || state.ActionSequence <= 0
                || state.Lifecycle == UnitLifecycle.Dying && (state.FrozenMoveTicks < 0 || state.FrozenMoveTicks >= state.EndTick - state.StartTick)))
                throw new ArgumentException("Invalid committed move interval/progress.", nameof(units));
            if (!rebuilt.CanPlace(state.City, state.Faction, state.Position, state.Size)) throw new ArgumentException("Overlapping or forbidden source reservations.", nameof(units));
            rebuilt.AddPosition(new(state.City, state.Position.Cell, state.Position.Anchor, state.Id, state.ActionSequence, state.Faction, state.Size));
            if (!state.HoldsTransit) continue;
            if (!board.Cell(state.Position.Cell).Neighbors.Contains(state.Destination.Cell)
                || !rebuilt.CanPlace(state.City, state.Faction, state.Destination, state.Size)) throw new ArgumentException("Invalid reserved destination.", nameof(units));
            HexTransition transition = board.Transition(state.Position, state.Destination);
            if (transition.Id != state.Transition || rebuilt._endpointTransit.ContainsKey((state.City, state.Position.Cell))
                || rebuilt._endpointTransit.ContainsKey((state.City, state.Destination.Cell))) throw new ArgumentException("Invalid/conflicting declared transit.", nameof(units));
            var transit = new TransitReservation(state.City, state.Position.Cell, state.Destination.Cell, transition.EdgeToken, state.Id, state.ActionSequence);
            rebuilt.AddPosition(new(state.City, state.Destination.Cell, state.Destination.Anchor, state.Id, state.ActionSequence, state.Faction, state.Size));
            rebuilt._endpointTransit.Add((state.City, state.Position.Cell), transit); rebuilt._endpointTransit.Add((state.City, state.Destination.Cell), transit);
            rebuilt._edgeTransit.Add((state.City, transition.EdgeToken), transit);
        }
        _positions.Clear(); foreach (var item in rebuilt._positions) _positions.Add(item.Key, item.Value);
        _endpointTransit.Clear(); foreach (var item in rebuilt._endpointTransit) _endpointTransit.Add(item.Key, item.Value);
        _edgeTransit.Clear(); foreach (var item in rebuilt._edgeTransit) _edgeTransit.Add(item.Key, item.Value);
        Revision = checked(Revision + 1);
    }
    public void Clear()
    {
        _positions.Clear(); _endpointTransit.Clear(); _edgeTransit.Clear(); Revision = checked(Revision + 1);
    }
    private bool Contains(int id) => _positions.Any(p => p.Value.ContainsKey(id));
    private void AddPosition(PositionReservation reservation)
    {
        var key = (reservation.City, reservation.Cell);
        if (!_positions.TryGetValue(key, out var occupants)) _positions.Add(key, occupants = []);
        occupants.Add(reservation.UnitId, reservation);
    }
    private bool RemovePosition(int city, int cell, int id)
    {
        var key = (city, cell);
        if (!_positions.TryGetValue(key, out var occupants) || !occupants.Remove(id)) return false;
        if (occupants.Count == 0) _positions.Remove(key); return true;
    }
    private void ReleaseTransit(int city, int id, long sequence)
    {
        foreach (var key in _endpointTransit.Where(p => p.Key.City == city && p.Value.UnitId == id && p.Value.ActionSequence == sequence).Select(p => p.Key).ToArray())
            _endpointTransit.Remove(key);
        foreach (var key in _edgeTransit.Where(p => p.Key.City == city && p.Value.UnitId == id && p.Value.ActionSequence == sequence).Select(p => p.Key).ToArray())
            _edgeTransit.Remove(key);
    }
}
