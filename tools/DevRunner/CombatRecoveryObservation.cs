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
        bool fresh = !string.IsNullOrEmpty(frame.Id) && _responses.Add(frame.Id);
        bool sameWindow = _last is not null && frame.MatchId == _last.MatchId
            && frame.ObservedCity == _last.ObservedCity && frame.Wave == _last.Wave
            && frame.PlaybackGeneration == _last.PlaybackGeneration;
        if (!fresh || sameWindow && (frame.CombatTick <= _last!.CombatTick || frame.Revision < _last.Revision))
        {
            _previous.Clear();
            return Proven;
        }
        if (!sameWindow) _previous.Clear();
        _last = frame;
        if (!frame.Connected || frame.MatchPhase != Phase.Combat || frame.Paused || string.IsNullOrEmpty(frame.MatchId))
        {
            _previous.Clear();
            return Proven;
        }
        var current = new Dictionary<int, (UnitObservation Unit, CombatPoseWitness Witness)>();
        foreach (UnitObservation unit in frame.Units.Where(u => u.Visible))
        {
            if (unit.Clip == "hit" && !unit.HitActive)
                throw new InvalidOperationException("Declared hit has no active animation layer.");
            if (unit.Clip == "attack" && !unit.AttackActive)
                throw new InvalidOperationException("Declared attack has no active animation one-shot.");
            if (unit.Dead || unit.Health <= 0 || unit.Hex is not { Lifecycle: UnitLifecycle.Alive, Action: UnitActionKind.Recovery, FrozenTick: null } hex
                || hex.Id != unit.Id || hex.City != frame.ObservedCity
                || unit.AttackSequence <= 0 || frame.CombatTick < unit.ImpactTick || frame.CombatTick >= unit.ReadyTick
                || !unit.AttackActive || unit.Clip is not ("attack" or "hit")) continue;
            CombatPoseWitness witness = CombatPoseDiagnostic.Witness(frame, unit, source, received);
            diagnostic.RecoveryCandidate(witness);
            if (_previous.TryGetValue(unit.Id, out var previous) && previous.Unit.ReadyTick == unit.ReadyTick
                && previous.Unit.AttackSequence == unit.AttackSequence && previous.Unit.Hex!.ActionSequence == hex.ActionSequence
                && previous.Unit.ImpactTick == unit.ImpactTick)
            {
                if (unit.X != previous.Unit.X || unit.Z != previous.Unit.Z)
                    throw new InvalidOperationException("Recovery moved away from its declared hex anchor.");
                diagnostic.Recovery(previous.Witness, witness);
                Proven = true;
            }
            current.Add(unit.Id, (unit, witness));
        }
        _previous = current;
        return Proven;
    }
}
