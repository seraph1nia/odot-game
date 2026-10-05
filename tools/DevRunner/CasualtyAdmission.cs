using Game.Core;

namespace DevRunner;

// One ordered driver supplies current state, a revision wait and ordinary commands.
// Retained witnesses wake the driver; they never authorize a pause or a capture.
internal static class CasualtyAdmission
{
    internal const int MaximumAttempts = 3;

    internal static async Task<MatchSnapshot> Pause(MatchSnapshot origin, int city, long after, long sequenceFloor,
        Func<MatchSnapshot> latest, Func<long, CancellationToken, Task> wait,
        Func<string, CancellationToken, Task<GameEvent>> action, CancellationToken token)
    {
        long floor = origin.Revision;
        int attempts = 0;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            MatchSnapshot current = latest();
            Identity(origin, current);
            if (current.Revision <= floor || current.Paused || !Runner.CasualtyInspectionReady(current, city, after))
            {
                floor = Math.Max(floor, current.Revision);
                await wait(floor, token);
                continue;
            }
            attempts++;
            GameEvent pause = await action("pause", token);
            token.ThrowIfCancellationRequested();
            MatchSnapshot frozen = Receipt(origin, current, pause, sequenceFloor, true);
            sequenceFloor = pause.Result!.Sequence;
            MatchSnapshot actual = latest();
            Frozen(frozen, actual);
            if (Runner.CasualtyInspectionReady(frozen, city, after) && Runner.CasualtyInspectionReady(actual, city, after))
                return frozen;

            // Only this driver's newly accepted, still-current pause may be undone.
            // Never remain frozen waiting for damage or reuse the earlier quote.
            token.ThrowIfCancellationRequested();
            GameEvent resume = await action("resume", token);
            MatchSnapshot running = Receipt(origin, actual, resume, sequenceFloor, false);
            sequenceFloor = resume.Result!.Sequence;
            Identity(origin, latest());
            floor = Math.Max(running.Revision, latest().Revision);
            if (attempts >= MaximumAttempts)
                throw new InvalidOperationException("Casualty admission exhausted three ordinary pause attempts after resuming its own ineligible pause.");
        }
    }

    private static void Identity(MatchSnapshot origin, MatchSnapshot state)
    {
        if (state.MatchId != origin.MatchId || state.ConfigurationFingerprint != origin.ConfigurationFingerprint
            || state.Phase != Phase.Combat || state.TurnSerial != origin.TurnSerial || state.Wave != origin.Wave)
            throw new InvalidOperationException("Casualty admission lost its current combat/session identity.");
    }

    private static MatchSnapshot Receipt(MatchSnapshot origin, MatchSnapshot quote, GameEvent receipt, long sequence, bool paused)
    {
        if (receipt.Type != "ack" || receipt.Result is not { Accepted: true } result || result.Sequence <= sequence || receipt.State is not { } state)
            throw new InvalidOperationException("Casualty admission requires a readable newly accepted ordinary command receipt.");
        Identity(origin, state);
        if (state.Revision < quote.Revision || state.Tick < quote.Tick || state.Paused != paused)
            throw new InvalidOperationException("Casualty admission received a stale or inconsistent ordinary command receipt.");
        return state;
    }

    internal static void Frozen(MatchSnapshot receipt, MatchSnapshot current)
    {
        Identity(receipt, current);
        if (!receipt.Paused || !current.Paused || current.Tick != receipt.Tick || current.Revision < receipt.Revision)
            throw new InvalidOperationException("Casualty pause no longer belongs to the accepted frozen receipt.");
    }

    internal static void Frame(MatchSnapshot receipt, MatchSnapshot current, UiObservation frame, int city, long after, int dead)
    {
        Frozen(receipt, current);
        if (!Runner.CasualtyInspectionReady(receipt, city, after) || !Runner.CasualtyInspectionReady(current, city, after)
            || frame.ObservedCity != city || frame.Revision != current.Revision || frame.CombatTick != receipt.Tick
            || !frame.PhaseText.Contains("PAUSED", StringComparison.Ordinal)
            || !receipt.DyingBodies.Any(u => u.Id == dead && u.Destination == city && u.Hex!.DeathStartTick > after && u.Hex.DeathEndTick > receipt.Tick + 12)
            || !frame.Units.Any(u => u.Id == dead && u.Dead && u.Visible && u.Destination == city && u.PoseSeconds < .35)
            || !frame.Units.Any(u => u.Visible && !u.Dead && u.Faction == Faction.Skeletons && u.Destination == city && u.Health > 0 && u.Health < u.MaximumHealth
                && receipt.Enemies.Any(e => e.Id == u.Id && e.Destination == city && e.Health == u.Health && e.Profile.Health == u.MaximumHealth)))
            throw new InvalidOperationException("Casualty frame does not bind the current pause, fresh body and focused living damaged opponent.");
    }
}
