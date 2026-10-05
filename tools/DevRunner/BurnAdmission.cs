using Game.Core;

namespace DevRunner;

// Caller-side research witness admission; one ordered driver, ordinary commands only.
internal static class BurnAdmission
{
    internal const int MaximumAttempts = 3;

    internal static async Task<(GameEvent Receipt, int TargetId)> Pause(MatchSnapshot origin, int? city, long sequenceFloor,
        Func<MatchSnapshot> latest, Func<long, CancellationToken, Task> wait,
        Func<string, CancellationToken, Task<GameEvent>> action, CancellationToken token)
    {
        long floor = origin.Revision - 1;
        int attempts = 0;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            MatchSnapshot quote = latest();
            Identity(origin, quote);
            UnitState? target = quote.Enemies.FirstOrDefault(u => (city is null || u.Destination == city) && Active(quote, u));
            if (quote.Revision <= floor || quote.Paused || target is null)
            {
                floor = Math.Max(floor, quote.Revision);
                await wait(floor, token);
                continue;
            }
            attempts++;
            GameEvent pause = await action("pause", token);
            token.ThrowIfCancellationRequested();
            MatchSnapshot frozen = Receipt(origin, quote, pause, sequenceFloor, true);
            sequenceFloor = pause.Result!.Sequence;
            MatchSnapshot current = latest();
            Identity(frozen, current);
            if (!current.Paused || current.Tick != frozen.Tick || current.Revision < frozen.Revision)
                throw new InvalidOperationException("Research burn pause no longer belongs to its accepted frozen receipt.");
            UnitState? captured = frozen.Enemies.SingleOrDefault(u => u.Id == target.Id);
            UnitState? actual = current.Enemies.SingleOrDefault(u => u.Id == target.Id);
            if (captured != actual)
                throw new InvalidOperationException("Research burn target changed after the accepted frozen receipt.");
            // Bind the observed target/effect, never another burn in the receipt.
            if (captured is not null && Active(frozen, captured) && captured.Destination == target.Destination
                && captured.Statuses.Burn!.Identity == target.Statuses.Burn!.Identity)
                return (pause, target.Id);

            // Undo only this driver's newly accepted, still-current ineligible pause.
            token.ThrowIfCancellationRequested();
            GameEvent resume = await action("resume", token);
            MatchSnapshot running = Receipt(origin, current, resume, sequenceFloor, false);
            sequenceFloor = resume.Result!.Sequence;
            token.ThrowIfCancellationRequested();
            MatchSnapshot fresh = latest();
            Identity(origin, fresh);
            floor = Math.Max(running.Revision, fresh.Revision);
            if (attempts >= MaximumAttempts)
                throw new InvalidOperationException("Research burn admission exhausted three ordinary pause attempts after resuming its own ineligible pause.");
        }
    }

    internal static bool Active(MatchSnapshot state, UnitState enemy)
        => enemy.Health > 0 && enemy.Statuses.Burn is { } burn && burn.ExpiresTick > state.Tick;

    private static void Identity(MatchSnapshot origin, MatchSnapshot state)
    {
        if (state.MatchId != origin.MatchId || state.ConfigurationFingerprint != origin.ConfigurationFingerprint
            || state.Phase != Phase.Combat || state.TurnSerial != origin.TurnSerial || state.Wave != origin.Wave)
            throw new InvalidOperationException("Research burn admission lost its current combat/session identity.");
    }

    private static MatchSnapshot Receipt(MatchSnapshot origin, MatchSnapshot quote, GameEvent receipt, long sequence, bool paused)
    {
        if (receipt.Type != "ack" || receipt.Result is not { Accepted: true } result || result.Sequence <= sequence || receipt.State is not { } state)
            throw new InvalidOperationException("Research burn admission requires a readable newly accepted ordinary command receipt.");
        Identity(origin, state);
        if (state.Revision < quote.Revision || state.Tick < quote.Tick || state.Paused != paused)
            throw new InvalidOperationException("Research burn admission received a stale or inconsistent ordinary command receipt.");
        return state;
    }
}
