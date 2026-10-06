using Game.Core;

namespace DevRunner;

// One ordered driver supplies current state, a revision wait and ordinary commands.
// Retained witnesses wake the driver; they never authorize a pause or a capture.
internal sealed record CasualtyIdentityState(string MatchId, CombatFingerprint ConfigurationFingerprint,
    Phase Phase, int TurnSerial, int Wave, long Tick, long Revision, bool Paused)
{
    internal static CasualtyIdentityState From(MatchSnapshot state) => new(state.MatchId, state.ConfigurationFingerprint,
        state.Phase, state.TurnSerial, state.Wave, state.Tick, state.Revision, state.Paused);
}
internal sealed record CasualtyIdentityDisjuncts(bool MatchId, bool ConfigurationFingerprint, bool CombatPhase, bool TurnSerial, bool Wave);
internal sealed record CasualtyIdentityRejection(string Callsite, CasualtyIdentityDisjuncts Failed,
    CasualtyIdentityState Origin, CasualtyIdentityState Current, string? ReceiptType, int? ReceiptPeerId,
    int? ReceiptPlayerId, long? ReceiptSequence, bool? ReceiptAccepted);

internal static class CasualtyAdmission
{
    internal const int MaximumAttempts = 3;
    internal const string IdentityEvidenceKey = "CasualtyIdentityRejection", IdentityStateKey = "CasualtyIdentityCurrentState";

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
            Identity(origin, current, "loop-entry");
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
            Frozen(frozen, actual, pause);
            if (Runner.CasualtyInspectionReady(frozen, city, after) && Runner.CasualtyInspectionReady(actual, city, after))
                return frozen;

            // Only this driver's newly accepted, still-current pause may be undone.
            // Never remain frozen waiting for damage or reuse the earlier quote.
            token.ThrowIfCancellationRequested();
            GameEvent resume = await action("resume", token);
            MatchSnapshot running = Receipt(origin, actual, resume, sequenceFloor, false);
            sequenceFloor = resume.Result!.Sequence;
            Identity(origin, latest(), "after-owned-resume", resume);
            floor = Math.Max(running.Revision, latest().Revision);
            if (attempts >= MaximumAttempts)
                throw new InvalidOperationException("Casualty admission exhausted three ordinary pause attempts after resuming its own ineligible pause.");
        }
    }

    private static void Identity(MatchSnapshot origin, MatchSnapshot state, string callsite, GameEvent? receipt = null)
    {
        if (state.MatchId != origin.MatchId || state.ConfigurationFingerprint != origin.ConfigurationFingerprint
            || state.Phase != Phase.Combat || state.TurnSerial != origin.TurnSerial || state.Wave != origin.Wave)
        {
            var error = new InvalidOperationException("Casualty admission lost its current combat/session identity.");
            // One rejection only, serialized by the terminal failure boundary, never in live callbacks.
            error.Data[IdentityEvidenceKey] = new CasualtyIdentityRejection(callsite,
                new(state.MatchId != origin.MatchId, state.ConfigurationFingerprint != origin.ConfigurationFingerprint,
                    state.Phase != Phase.Combat, state.TurnSerial != origin.TurnSerial, state.Wave != origin.Wave),
                CasualtyIdentityState.From(origin), CasualtyIdentityState.From(state), receipt?.Type, receipt?.PeerId,
                receipt?.PlayerId, receipt?.Result?.Sequence, receipt?.Result?.Accepted);
            error.Data[IdentityStateKey] = state;
            throw error;
        }
    }

    private static MatchSnapshot Receipt(MatchSnapshot origin, MatchSnapshot quote, GameEvent receipt, long sequence, bool paused)
    {
        if (receipt.Type != "ack" || receipt.Result is not { Accepted: true } result || result.Sequence <= sequence || receipt.State is not { } state)
            throw new InvalidOperationException("Casualty admission requires a readable newly accepted ordinary command receipt.");
        Identity(origin, state, "accepted-receipt", receipt);
        if (state.Revision < quote.Revision || state.Tick < quote.Tick || state.Paused != paused)
            throw new InvalidOperationException("Casualty admission received a stale or inconsistent ordinary command receipt.");
        return state;
    }

    internal static void Frozen(MatchSnapshot receipt, MatchSnapshot current, GameEvent? acceptedReceipt = null)
    {
        Identity(receipt, current, "frozen-current", acceptedReceipt);
        if (!receipt.Paused || !current.Paused || current.Tick != receipt.Tick || current.Revision < receipt.Revision)
            throw new InvalidOperationException("Casualty pause no longer belongs to the accepted frozen receipt.");
    }

    internal static void Frame(MatchSnapshot receipt, MatchSnapshot current, UiObservation frame, int city, long after, int dead)
    {
        Frozen(receipt, current);
        if (!Runner.CasualtyInspectionReady(receipt, city, after) || !Runner.CasualtyInspectionReady(current, city, after)
            || frame.ObservedCity != city || frame.Revision != current.Revision || frame.CombatTick != receipt.Tick
            || !frame.PhaseText.Contains("PAUSED", StringComparison.Ordinal)
            || !Runner.EligibleCasualties(receipt, city, after).Any(u => u.Id == dead)
            || !Runner.EligibleCasualties(current, city, after).Any(u => u.Id == dead)
            || !frame.Units.Any(u => u.Id == dead && u.Dead && u.Visible && u.Destination == city && Runner.FreshDeathPose(u.PoseSeconds))
            || !frame.Units.Any(u => u.Visible && !u.Dead && u.Faction == Faction.Skeletons && u.Destination == city && u.Health > 0 && u.Health < u.MaximumHealth
                && receipt.Enemies.Any(e => e.Id == u.Id && e.Destination == city && e.Health == u.Health && e.Profile.Health == u.MaximumHealth)))
            throw new InvalidOperationException("Casualty frame does not bind the current pause, fresh body and focused living damaged opponent.");
    }
}
