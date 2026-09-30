namespace Game.Core;

// Presentation policy, shared by every transport; never advances game rules.
public sealed class CombatPlayback
{
    private MatchSnapshot? _previous, _current;
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
            _pending.Clear(); _previous = null; _current = state;
            EventCursor = _acceptedSequence = state.EventSequence; Tick = state.Tick;
            if (fresh) VisualSeconds = 0;
            Generation++;
            return true;
        }
        _previous = _current; _current = state;
        foreach (CombatEvent entry in state.CombatEvents.Where(e => e.Sequence > _acceptedSequence).OrderBy(e => e.Sequence))
            _pending.Enqueue(entry);
        _acceptedSequence = state.EventSequence;
        // Corrections have one common fraction for all bodies. No extrapolation.
        Tick = Math.Max(Tick, state.Tick - 3);
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
        { events.Add(_pending.Dequeue()); EventCursor = entry.Sequence; }
        return events.ToArray();
    }
    public UnitState[] Units()
    {
        if (_current is null) return [];
        UnitState[] current = All(_current);
        if (_current.Phase != Phase.Combat)
            return current.Select(u => u with { MoveForward = 0, MoveLateral = 0, TargetId = 0, TargetCity = false, PendingImpact = false, ReadyTick = 0 }).ToArray();
        if (_previous is null || _current.Paused || _current.Tick <= _previous.Tick) return current;
        var previous = All(_previous).ToDictionary(u => u.Id);
        double fraction = Math.Clamp((Tick - _previous.Tick) / (_current.Tick - _previous.Tick), 0, 1);
        return current.Select(u =>
        {
            previous.TryGetValue(u.Id, out UnitState? old);
            if (u.Deployed && (old is null || !old.Deployed || old.Destination != u.Destination) && fraction < 1)
                return u with { Deployed = false };
            return old is not null && old.Destination == u.Destination && old.Deployed && u.Deployed
                ? u with { Position = old.Position + (u.Position - old.Position) * fraction, Lateral = old.Lateral + (u.Lateral - old.Lateral) * fraction } : u;
        }).ToArray();
    }
    public static UnitState[] All(MatchSnapshot state) => state.Players.SelectMany(p => p.Soldiers).Concat(state.Enemies).OrderBy(u => u.Id).ToArray();
    public static double AttackPose(UnitState unit, double tick, double clipLength)
    {
        double marker = unit.Type == UnitType.Crossbowman ? 0.43 : 0.40;
        double pose = tick <= unit.ImpactTick ? marker * (tick - unit.ActionStartTick) / Math.Max(1, unit.ImpactTick - unit.ActionStartTick)
            : marker + (clipLength - marker) * (tick - unit.ImpactTick) / Math.Max(1, unit.ReadyTick - unit.ImpactTick);
        return Math.Clamp(pose, 0, clipLength);
    }
}
