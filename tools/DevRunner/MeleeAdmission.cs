using Game.Core;

namespace DevRunner;

// One fixture handoff, no processes/timers/authority mutations. The driver
// supplies latest current state; historical witnesses never authorize pause.
internal sealed class MeleeAdmission
{
    private readonly string _match;
    private readonly long _floor;
    private bool _armed;
    private volatile bool _handoff;
    internal int LiveWitnesses { get; private set; }
    internal MeleeAdmission(MatchSnapshot preparation)
    {
        if (preparation.Phase != Phase.Preparation || preparation.Tick != 0) throw new InvalidOperationException("Melee registration requires pre-Ready tick zero.");
        _match = preparation.MatchId; _floor = preparation.Revision;
    }
    internal void Arm(MatchSnapshot readyAck)
    {
        if (_armed || readyAck.MatchId != _match || readyAck.Phase != Phase.Combat || readyAck.Revision <= _floor)
            throw new InvalidOperationException("Melee observer requires its own current combat Ready acknowledgement.");
        _armed = true;
    }
    internal void Witness(MatchSnapshot latest, HexBoard board)
    {
        if (_armed && latest.MatchId == _match && latest.Phase == Phase.Combat && !latest.Paused && latest.Wave <= 2
            && MeleeVisualProof.HasMilestone(latest, MeleeCoverage.Windup, board)) LiveWitnesses++;
    }
    internal void Handoff(TimeSpan elapsed, int observations, int acquisitions)
    {
        Game.OwnedFrameCapture.RequireLiveCompletion(elapsed, observations, acquisitions);
        if (!_armed || _handoff) throw new InvalidOperationException("Missing/duplicate melee progression handoff.");
        _handoff = true;
    }
    internal bool MayPause(MatchSnapshot candidate, MatchSnapshot current, MeleeCoverage phase, HexBoard board)
        => _armed && _handoff && current.MatchId == _match && current.Revision > _floor && current.Phase == Phase.Combat
            && !current.Paused && current.Wave <= 2 && candidate.MatchId == current.MatchId && candidate.Revision == current.Revision
            && candidate.Tick == current.Tick && candidate.Phase == current.Phase && candidate.TurnSerial == current.TurnSerial
            && MeleeVisualProof.HasMilestone(current, phase, board);
    internal static void FrozenFrame(UiObservation frame, MatchSnapshot paused)
    {
        if (!paused.Paused || paused.Phase != Phase.Combat || paused.Wave > 2 || frame.Revision < paused.Revision
            || !frame.PhaseText.Contains("PAUSED", StringComparison.Ordinal) || frame.CombatTick != paused.Tick)
            throw new InvalidOperationException("Melee frame must match current nonterminal authority pause receipt.");
    }
}
