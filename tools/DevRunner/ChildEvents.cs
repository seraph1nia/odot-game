using Game.Core;

namespace DevRunner;

// Caller holds the child's gate. Arrival order survives match revision resets.
internal sealed class ChildEvents
{
    public const int MaximumStates = 16, MaximumStateBytes = 16 * 1024 * 1024, MaximumEvents = 512;
    private readonly List<(GameEvent Event, int Bytes, long Order)> _states = [];
    private readonly List<(GameEvent Event, bool Observed, long Order)> _events = [];
    private int _stateBytes;
    private long _order;
    public int DroppedStates { get; private set; }
    public long LastTick { get; private set; }
    public MatchSnapshot? LatestState => _states.Count == 0 ? null : _states[^1].Event.State;
    public GameEvent? RecentUi { get; private set; }
    public string? Failure { get; private set; }

    public void Add(GameEvent value, int bytes)
    {
        long order = ++_order;
        if (value.Type == "ui" && bytes <= 1024 * 1024) RecentUi = value;
        if (value.State is not null)
        {
            LastTick = value.State.Tick;
            _states.Add((value, bytes, order)); _stateBytes += bytes;
            while (_states.Count > MaximumStates || _stateBytes > MaximumStateBytes)
            {
                _stateBytes -= _states[0].Bytes; _states.RemoveAt(0); DroppedStates++;
            }
        }
        if (value.Type == "snapshot") return;
        if (_events.Count == MaximumEvents)
        {
            if (!_events[0].Observed) { Failure = "Reliable child event retention overflow; the ordered driver did not observe pending events."; return; }
            _events.RemoveAt(0);
        }
        _events.Add((value with { State = null }, false, order));
    }

    public GameEvent? Find(Func<GameEvent, bool> predicate)
    {
        if (Failure is not null) throw new InvalidOperationException(Failure);
        GameEvent? found = History().LastOrDefault(predicate);
        GameEvent? error = _events.LastOrDefault(e => !e.Observed && e.Event.Type == "error").Event;
        for (int i = 0; i < _events.Count; i++)
        {
            var retained = _events[i];
            if (retained.Event.Type == "ui") retained.Event = retained.Event with { Message = "{}" };
            _events[i] = (retained.Event, true, retained.Order);
        }
        if (found is not null) return found;
        if (error is not null) throw new InvalidOperationException(error.Message);
        return null;
    }
    public GameEvent[] History() => _events.Select(e => (e.Event, e.Order)).Concat(_states.Select(s => (s.Event, s.Order)))
        .GroupBy(e => e.Order).OrderBy(g => g.Key).Select(g => g.Last().Event).ToArray();
    public GameEvent[] States() => _states.Select(s => s.Event).ToArray();
}
