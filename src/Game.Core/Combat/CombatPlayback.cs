namespace Game.Core;

// Presentation policy, shared by every transport; never advances game rules.
public sealed class CombatPlayback
{
    private WorkCounters? _work;
    public WorkCounters? Work
    {
        get => _work;
        set { _work = value; value?.Support(WorkMetric.PlaybackIndexBuilds, WorkMetric.PlaybackEventVisits, WorkMetric.Sorts, WorkMetric.SortElements); }
    }
    private sealed record AcceptedState(string MatchId, long Revision, long Tick, bool Paused, UnitState[] Units, Dictionary<int, UnitState> ById);
    private AcceptedState? _previous, _current;
    private int[] _ids = [];
    private readonly Dictionary<int, int> _pendingDeaths = [];
    private readonly Queue<CombatEvent> _pending = new();
    private long _acceptedSequence;
    public long EventCursor { get; private set; }
    public double Tick { get; private set; }
    public double VisualSeconds { get; private set; }
    public int Generation { get; private set; }
    public bool Accept(MatchSnapshot state, bool baseline = false)
    {
        bool fresh = _current?.MatchId != state.MatchId;
        if (!fresh && !baseline && state.Revision <= _current!.Revision) return false;
        bool gap = !fresh && state.OldestEventSequence > _acceptedSequence + 1;
        if (fresh || baseline || gap)
        {
            _pending.Clear(); _pendingDeaths.Clear(); _previous = null; _current = Index(state);
            _ids = _current.Units.Select(u => u.Id).ToArray();
            EventCursor = _acceptedSequence = state.EventSequence; Tick = state.Tick;
            if (fresh) VisualSeconds = 0;
            Generation++;
            return true;
        }
        _previous = _current; _current = Index(state);
        _ids = WorkOrdering.Input(_previous!.ById.Keys.Concat(_current.ById.Keys).Distinct(), _work).Order().ToArray();
        _work?.Add(WorkMetric.PlaybackEventVisits, state.CombatEvents.Length);
        foreach (CombatEvent entry in WorkOrdering.Input(state.CombatEvents.Where(e => e.Sequence > _acceptedSequence), _work).OrderBy(e => e.Sequence))
        {
            var owned = entry with { Unit = entry.Unit is null ? null : CombatProjection.Detach(entry.Unit), Victims = entry.Victims.ToArray() };
            _pending.Enqueue(owned);
            if (owned.Type == CombatEventType.Death && owned.Unit is { } dead) _pendingDeaths[dead.Id] = _pendingDeaths.GetValueOrDefault(dead.Id) + 1;
        }
        _acceptedSequence = state.EventSequence;
        // Corrections have one common fraction for all bodies. No extrapolation.
        Tick = state.Paused ? state.Tick : Math.Max(Tick, state.Tick - 3);
        return true;
    }
    public void Advance(double seconds, bool connected)
    {
        if (!connected || _current is null || _current.Paused) return;
        VisualSeconds += Math.Clamp(seconds, 0, 0.1);
        Tick = Math.Min(_current.Tick, Tick + Math.Clamp(seconds, 0, 0.1) * Match.StepsPerSecond);
    }
    public CombatEvent[] Drain()
    {
        var events = new List<CombatEvent>();
        while (_pending.TryPeek(out CombatEvent? entry) && entry.Tick <= Tick)
        {
            _work?.Add(WorkMetric.PlaybackEventVisits); events.Add(_pending.Dequeue()); EventCursor = entry.Sequence;
            if (entry.Type == CombatEventType.Death && entry.Unit is { } dead)
            {
                if (_pendingDeaths[dead.Id] == 1) _pendingDeaths.Remove(dead.Id);
                else _pendingDeaths[dead.Id]--;
            }
        }
        return events.ToArray();
    }
    public UnitState[] Units()
    {
        if (_current is null) return [];
        return HexUnits();
    }
    private UnitState[] HexUnits()
    {
        var sampled = new List<UnitState>(_ids.Length);
        foreach (int id in _ids)
        {
            UnitState? old = _previous?.ById.GetValueOrDefault(id);
            if (!_current!.ById.TryGetValue(id, out UnitState? unit))
            {
                if (old?.Hex is { Lifecycle: UnitLifecycle.Dying } priorDeath && Tick < priorDeath.DeathEndTick) sampled.Add(CombatProjection.Detach(old));
                continue;
            }
            UnitState selected = unit; HexUnitState? hex = unit.Hex;
            if (old?.Hex is { } prior && hex is not null)
            {
                if (hex.Lifecycle == UnitLifecycle.Dying && Tick < hex.DeathStartTick
                    || hex.Lifecycle == UnitLifecycle.Alive && prior.Lifecycle == UnitLifecycle.Queued && Tick < hex.AdmittedTick
                    || prior.HoldsTransit && Tick < prior.EndTick && (hex.Action != UnitActionKind.Moving || hex.StartTick > Tick)
                    || hex.StartTick > Tick && hex.Lifecycle != UnitLifecycle.Dying) selected = old;
            }
            if (selected.Hex is not { Lifecycle: UnitLifecycle.Dying } death || Tick < death.DeathEndTick) sampled.Add(CombatProjection.Detach(selected));
        }
        // A snapshot can omit a death that expires up to three buffered ticks
        // ahead of the presentation clock. Keep its previous current state only
        // through the declared deadline, never recreate it from historical events.
        return sampled.ToArray();
    }
    private AcceptedState Index(MatchSnapshot state)
    {
        _work?.Add(WorkMetric.PlaybackIndexBuilds);
        UnitState[] ordered = WorkOrdering.Input(state.Players.SelectMany(p => p.Soldiers).Concat(state.Enemies).Concat(state.DyingBodies), _work)
            .OrderBy(u => u.Id).Select(u => CombatProjection.Detach(u)).ToArray();
        return new(state.MatchId, state.Revision, state.Tick, state.Paused, ordered, ordered.ToDictionary(u => u.Id));
    }
    public bool AwaitingDeath(int id) => _pendingDeaths.ContainsKey(id);
    public static double DeathPose(UnitState unit, double tick, double clipLength)
    {
        HexUnitState hex = unit.Hex ?? throw new ArgumentException("Death sampling requires authoritative hex state.", nameof(unit));
        return Math.Clamp((tick - hex.DeathStartTick) / Math.Max(1, hex.DeathEndTick - hex.DeathStartTick), 0, 1) * clipLength;
    }
    public static UnitState[] All(MatchSnapshot state, WorkCounters? work = null)
    {
        work?.Add(WorkMetric.Sorts); work?.Add(WorkMetric.SortElements, state.Players.Sum(p => p.Soldiers.Length) + state.Enemies.Length);
        return state.Players.SelectMany(p => p.Soldiers).Concat(state.Enemies).OrderBy(u => u.Id).ToArray();
    }
    // Imported clip sampling: axe hand descends at frame 23, cast hand reaches
    // maximum forward extension at frame 8 (30 fps); sword/shot retain their markers.
    public static double ImpactMarker(UnitType type) => type switch
    { UnitType.Crossbowman => .43, UnitType.Berserker => 23 / 30.0, UnitType.Mage => 8 / 30.0, _ => .40 };
    public static double AttackPose(UnitState unit, double tick, double clipLength)
    {
        double marker = ImpactMarker(unit.Type);
        double pose = tick <= unit.ImpactTick ? marker * (tick - unit.ActionStartTick) / Math.Max(1, unit.ImpactTick - unit.ActionStartTick)
            : marker + (clipLength - marker) * (tick - unit.ImpactTick) / Math.Max(1, unit.ReadyTick - unit.ImpactTick);
        return Math.Clamp(pose, 0, clipLength);
    }
}
