using System.Text.Json;
using Game;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    private OwnedTimingTrace? _timing;
    private async Task NativePump()
    {
        await _evidence.Measure("native-pump", "diagnostic", async () =>
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation); deadline.CancelAfter(60000);
            await using var owned = new ScenarioScope("native-pump", _evidence);
            var worker = new Runner(options, deadline.Token, _evidence, owned, _root);
            Child child = worker.StartGameRole("native-pump", "menu", true, 0, extra: [options.Scenario == "peer-liveness" ? "--native-liveness-control" : "--native-pump-control"]);
            GameEvent result = await child.WaitFor(e => e.Type == "native-pump", "native record/receipt conservation", 60000, deadline.Token);
            await File.WriteAllTextAsync(Path.Combine(owned.EvidenceDirectory, "native-pump.json"), result.Message, deadline.Token);
            Require(await child.WaitExit(deadline.Token) == 0 && !child.HasEngineErrors, "native pump process and teardown succeed");
            await owned.DisposeAsync(); owned.CheckErrors();
        });
    }
    private async Task MeleeAdmissionDiagnostic(Child client, Child observer, CancellationToken token)
    {
        using var trace = new OwnedTimingTrace(Path.Combine(_scope!.EvidenceDirectory, "driver-timing.jsonl"), true);
        _timing = trace;
        var frames = new List<UiObservation>(); var witnesses = new List<MeleeWitness>();
        Task<MatchSnapshot>? early = null; long floor = 0;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(45000);
        try
        {
            await MeleeArmy(client, observer, deadline.Token, s =>
            {
                floor = s.Revision;
                trace.Record("early-observer-register-before-Ready", s, info: new { Floor = floor });
            });
            var board = new HexBoard(Latest(client).Rules.Combat.Board);
            // One observer driver: Ready ack has completed. This starts its
            // sole wait immediately, using the pre-Ready floor. No competing
            // reader, no pause and no blocking of the graphical proof.
            async Task<MatchSnapshot> Early()
            {
                trace.Record("early-observer-wait-start", Latest(observer));
                MatchSnapshot state = await Observe(observer, s => s.Revision > floor
                    && (s.Phase != Phase.Combat || MeleeVisualProof.HasMilestone(s, MeleeCoverage.Windup, board)), "early live milestone without pause", deadline.Token);
                trace.Record("early-observer-found", state);
                return state;
            }
            early = Early(); long started = OwnedTimingTrace.Now(); bool modeledLate = false;
            // A diagnostic read-only sample series, NOT MeleeProgression, a
            // renewed 12s budget, near/far screenshots or game acceptance.
            for (int sample = 0; sample < 40 && OwnedTimingTrace.Now() - started < 18_000_000_000; sample++)
            {
                UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, deadline.Token, live: true);
                frames.Add(frame);
                trace.Record("driver-render-response", Latest(client), info: new { frame.Id, frame.CombatTick, frame.Revision, Sample = sample });
                foreach (MeleeWitness witness in MeleeVisualProof.Inspect(frame, board)) witnesses.Add(witness);
                if (!modeledLate && OwnedTimingTrace.Now() - started >= 10_633_581_500)
                {
                    modeledLate = true; trace.Record("prior-serialization-duration-registration-control", Latest(observer));
                }
                if (Latest(observer).Phase is Phase.Defeat or Phase.Victory) { trace.Record("current-authority-terminal", Latest(observer)); break; }
            }
            MatchSnapshot found = await early;
            Require(found.Phase == Phase.Combat && !found.Paused, "early observer finds current live milestone without pausing movement");
            Require(frames.All(f => f.Screenshot is null && f.RawCapture is null), "diagnostic frames never claim PNG acquisition");
            trace.Record("diagnostic-complete", Latest(observer), info: new { GraphicalTick = Latest(client).Tick });
            await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, "admission-diagnostic.json"), JsonSerializer.Serialize(new
            {
                Acceptance = false,
                EarlyMilestoneTick = found.Tick,
                EarlyMilestoneRevision = found.Revision,
                MovementAdvanced = CombatProgressionProof.MovementAdvanced(frames),
                AttackAdvanced = CombatProgressionProof.AttackAdvanced(frames),
                Paused = frames.Any(f => f.PhaseText.Contains("PAUSED", StringComparison.Ordinal)),
                LatestAuthority = new { Latest(observer).Revision, Latest(observer).Tick, Latest(observer).Phase },
                LatestGraphical = new { Latest(client).Revision, Latest(client).Tick, Latest(client).Phase },
                Frames = frames,
                Witnesses = witnesses
            }, Evidence.JsonOptions), deadline.Token);
        }
        finally
        {
            deadline.Cancel();
            if (early is not null) { try { await early; } catch (OperationCanceledException) { } }
            _timing = null;
        }
    }
}
