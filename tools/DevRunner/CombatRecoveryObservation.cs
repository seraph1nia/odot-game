using Game.Core;

namespace DevRunner;

// Owned by the existing ordered UI driver, never fed retained milestone frames.
internal sealed class CombatRecoveryObservation(CombatPoseDiagnostic diagnostic)
{
    private readonly HashSet<string> _responses = [];
    private Dictionary<int, (UnitObservation Unit, CombatPoseWitness Witness)> _previous = [];
    private UiObservation? _last;
    public bool Proven { get; private set; }

    public bool Observe(UiObservation frame, MatchSnapshot received, string source)
    {
        CombatRecoveryTrace.Callback? trace = diagnostic.EarlyRecoveryTrace.Begin(frame, received, source, _last, _previous);
        bool fresh = !string.IsNullOrEmpty(frame.Id) && _responses.Add(frame.Id);
        bool sameWindow = _last is not null && frame.MatchId == _last.MatchId
            && frame.ObservedCity == _last.ObservedCity && frame.Wave == _last.Wave
            && frame.PlaybackGeneration == _last.PlaybackGeneration;
        if (trace is not null)
        {
            trace.Fresh = fresh;
            trace.SameWindow = sameWindow;
            trace.WindowChanges = _last is null ? ["first-callback"] : new[]
            {
                frame.MatchId != _last.MatchId ? "match" : null,
                frame.ObservedCity != _last.ObservedCity ? "city" : null,
                frame.Wave != _last.Wave ? "wave" : null,
                frame.PlaybackGeneration != _last.PlaybackGeneration ? "playback-generation" : null
            }.OfType<string>().ToArray();
        }
        if (!fresh || sameWindow && (frame.CombatTick <= _last!.CombatTick || frame.Revision < _last.Revision))
        {
            if (trace is not null)
                foreach (var prior in trace.PreviousCandidates) prior.Decision = "response-rejected:prior-cleared";
            trace?.End(!fresh ? (string.IsNullOrEmpty(frame.Id) ? "missing-response" : "duplicate-response")
                : "response-rejected:" + string.Join(",", new[]
                {
                    frame.CombatTick <= _last!.CombatTick ? "nonadvancing-tick" : null,
                    frame.Revision < _last.Revision ? "regressing-revision" : null
                }.Where(reason => reason is not null)), Proven);
            _previous.Clear();
            return Proven;
        }
        if (!sameWindow)
        {
            if (trace is not null)
                foreach (var prior in trace.PreviousCandidates) prior.Decision = "window-changed:prior-cleared";
            _previous.Clear();
        }
        _last = frame;
        if (!frame.Connected || frame.MatchPhase != Phase.Combat || frame.Paused || string.IsNullOrEmpty(frame.MatchId))
        {
            if (trace is not null)
                foreach (var prior in trace.PreviousCandidates) prior.Decision = "frame-ineligible:prior-cleared";
            trace?.End("frame-ineligible:" + string.Join(",", new[]
            {
                !frame.Connected ? "disconnected" : null,
                frame.MatchPhase != Phase.Combat ? "non-combat" : null,
                frame.Paused ? "paused" : null,
                string.IsNullOrEmpty(frame.MatchId) ? "missing-match" : null
            }.Where(reason => reason is not null)), Proven);
            _previous.Clear();
            return Proven;
        }
        var current = new Dictionary<int, (UnitObservation Unit, CombatPoseWitness Witness)>();
        foreach (UnitObservation unit in frame.Units.Where(u => u.Visible))
        {
            if (unit.Clip == "hit" && !unit.HitActive)
            {
                trace?.Actor(unit, "invalid-hit-layer");
                trace?.End("throw:invalid-hit-layer", Proven);
                throw new InvalidOperationException("Declared hit has no active animation layer.");
            }
            if (unit.Clip == "attack" && !unit.AttackActive)
            {
                trace?.Actor(unit, "invalid-attack-layer");
                trace?.End("throw:invalid-attack-layer", Proven);
                throw new InvalidOperationException("Declared attack has no active animation one-shot.");
            }
            if (unit.Dead || unit.Health <= 0 || unit.Hex is not { Lifecycle: UnitLifecycle.Alive, Action: UnitActionKind.Recovery, FrozenTick: null } hex
                || hex.Id != unit.Id || hex.City != frame.ObservedCity
                || unit.AttackSequence <= 0 || frame.CombatTick < unit.ImpactTick || frame.CombatTick >= unit.ReadyTick
                || !unit.AttackActive || unit.Clip is not ("attack" or "hit"))
            {
                trace?.Actor(unit, CombatRecoveryTrace.Ineligible(frame, unit));
                continue;
            }
            CombatPoseWitness witness = CombatPoseDiagnostic.Witness(frame, unit, source, received);
            diagnostic.RecoveryCandidate(witness);
            if (_previous.TryGetValue(unit.Id, out var previous) && previous.Unit.ReadyTick == unit.ReadyTick
                && previous.Unit.AttackSequence == unit.AttackSequence && previous.Unit.Hex!.ActionSequence == hex.ActionSequence
                && previous.Unit.ImpactTick == unit.ImpactTick)
            {
                if (unit.X != previous.Unit.X || unit.Z != previous.Unit.Z)
                {
                    trace?.Actor(unit, "position-drift:" + (unit.X != previous.Unit.X ? "X" : "") + (unit.Z != previous.Unit.Z ? "Z" : ""));
                    trace?.End("throw:position-drift", Proven);
                    throw new InvalidOperationException("Recovery moved away from its declared hex anchor.");
                }
                diagnostic.Recovery(previous.Witness, witness);
                Proven = true;
                trace?.Actor(unit, "accepted-pair");
            }
            else trace?.Actor(unit, _previous.TryGetValue(unit.Id, out var prior)
                ? CombatRecoveryTrace.Mismatch(prior.Unit, unit) : "eligible:no-prior-candidate");
            current.Add(unit.Id, (unit, witness));
        }
        _previous = current;
        trace?.End("completed", Proven);
        return Proven;
    }
}
