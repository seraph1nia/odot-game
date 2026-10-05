using System.Text.Json;
using Game;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task CoordinatedMelee(Child client, Child observer, CancellationToken token)
    {
        using var ownedWait = CancellationTokenSource.CreateLinkedTokenSource(token);
        using var trace = new OwnedTimingTrace(Path.Combine(_scope!.EvidenceDirectory, "melee-handoff-timing.jsonl"), true);
        _timing = trace; MeleeAdmission? admission = null; Task<MatchSnapshot>? pause = null;
        try
        {
            await MeleeArmy(client, observer, token,
                beforeReady: state => { admission = new(state); trace.Record("milestone-register-before-Ready", state); },
                readyAcknowledged: state =>
                {
                    admission!.Arm(state); trace.Record("milestone-arm-after-own-Ready-ack", state);
                    pause = Watch(state);
                });
            await MeleeCheckpoint(client, observer, token, admission!, pause!);
        }
        finally
        {
            ownedWait.Cancel();
            if (pause is not null) { try { await pause; } catch (OperationCanceledException) { } }
            _timing = null;
        }
        async Task<MatchSnapshot> Watch(MatchSnapshot battle)
        {
            var board = new HexBoard(battle.Rules.Combat.Board);
            while (true)
            {
                ownedWait.Token.ThrowIfCancellationRequested();
                MatchSnapshot latest = Latest(observer);
                if (latest.Phase != Phase.Combat || latest.Wave > 2) { trace.Record("current-authority-stage-guard", latest); return latest; }
                admission!.Witness(latest, board);
                if (admission.MayPause(latest, Latest(observer), MeleeCoverage.Windup, board))
                {
                    trace.Record("current-live-pause-request-after-handoff", latest);
                    MatchSnapshot paused = State(await Action(observer, "pause", ownedWait.Token));
                    trace.Record("current-live-pause-ack", paused, info: new { admission.LiveWitnesses });
                    // This is the actual receipt, never the stored earlier witness.
                    Require(paused.Phase == Phase.Combat && paused.Paused && paused.Wave <= 2,
                        "progression handoff pauses current ordinary combat before terminal/wave three");
                    return paused;
                }
                long revision = latest.Revision;
                await Observe(observer, s => s.Revision > revision, "ordered live melee observer/progression handoff", ownedWait.Token);
            }
        }
    }
    private async Task MeleeCapture(Child client, Child observer, string name, MatchSnapshot paused,
        MeleeWitness action, MeleeCoverage phase, HexBoard board, CancellationToken token)
    {
        MatchSnapshot current = Latest(observer);
        Require(current.MatchId == paused.MatchId && current.Phase == Phase.Combat && current.Paused && current.Tick == paused.Tick && current.Wave <= 2,
            "milestone capture retains current nonterminal authority pause");
        string path = Path.Combine(_scope!.EvidenceDirectory, name + ".png");
        UiObservation raw = await UiProtocol.Probe(client, options.StartupTimeout, token, path, deferred: true);
        MeleeAdmission.FrozenFrame(raw, current);
        Require(raw.Screenshot is null && raw.RawCapture is { } receipt && receipt.Id == raw.Id && receipt.Path == path,
            "milestone image has actual raw acquisition identity before persistence");
        Require(MeleeVisualProof.Inspect(raw, board).Any(w => w.Actor == action.Actor && w.Target == action.Target && w.Sequence == action.Sequence
            && w.ImpactTick == action.ImpactTick && (w.Coverage & phase) != 0), "milestone pixels/metadata belong to the actual paused action");
        RenderedContact(raw); HealthBars(raw);
        var persisted = await UiProtocol.Persist(client, raw, options.StartupTimeout, token);
        MeleeAdmission.FrozenFrame(persisted.Frame, current);
        await File.WriteAllTextAsync(Path.ChangeExtension(path, ".json"), JsonSerializer.Serialize(persisted.Frame, Evidence.JsonOptions), token);
    }
}
