using System.Diagnostics;
using System.Text.Json;

namespace DevRunner;

internal sealed partial class Runner
{
    // Selected diagnostic only: one ordinary static solo, no battle/campaign.
    // Catches capture/probe latency attribution errors hidden by numerical tests.
    private async Task AssetCostScenario(CancellationToken token)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(TimeSpan.FromSeconds(120));
        token = deadline.Token;
        Child client = StartGameRole("ui-cost", "solo", false, 0);
        await client.WaitFor(e => e.Type == "ack" && e.State?.Phase == Game.Core.Phase.Building, "ordinary static solo", options.StartupTimeout, token);
        UiObservation ready = await WaitUi(client, p => p.Connected && p.Screen == "session" && p.UnitBindings.Length > 0, "actual static authored scene ready", token);
        NativeWindowClose.Resize(client, options, ready.NativeWindow, 1280, 720);
        await WaitUi(client, p => p.Width == 1280 && p.Height == 720, "supported static cost resolution", token);
        await Pick(client, 0, token); await ClickAck(client, "Farm", token);
        await Pick(client, 2, token); await ClickAck(client, "MetalMine", token);
        await Pick(client, 1, token); await ClickAck(client, "Barracks", token);
        var samples = new List<object>();
        for (int index = 0; index < 16; index++)
        {
            string? png = index % 8 >= 4 ? Path.Combine(_scope!.EvidenceDirectory, $"static-cost-{index}.png") : null;
            long stamp = Stopwatch.GetTimestamp();
            UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token, png, live: index >= 8);
            double response = Stopwatch.GetElapsedTime(stamp).TotalMilliseconds;
            if (png is not null) UiProtocol.Frame(frame, png);
            stamp = Stopwatch.GetTimestamp();
            await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, $"static-cost-{index}-observation.json"), JsonSerializer.Serialize(frame, Evidence.JsonOptions), token);
            double evidenceWrite = Stopwatch.GetElapsedTime(stamp).TotalMilliseconds;
            Require(frame.Width == 1280 && frame.Height == 720 && frame.Revision == Latest(client).Revision && frame.CombatTick == 0,
                "static cost sample has unchanged camera/resolution/authority stage; no battle pacing");
            samples.Add(new
            {
                Index = index,
                LiveAdmission = index >= 8,
                Screenshot = png is not null,
                ResponseMilliseconds = response,
                EvidenceWriteMilliseconds = evidenceWrite,
                frame.ProbeCosts,
                frame.RenderCosts,
                frame.RenderGroups,
                frame.Camera,
                AuthorityRevision = Latest(client).Revision,
                frame.Revision,
                frame.CombatTick
            });
        }
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "static-cost-attribution.json"), JsonSerializer.Serialize(samples, Evidence.JsonOptions), token);
        await client.Send("quit"); Require(await client.WaitExit(token) == 0, "owned static cost window exits cleanly");
    }
}
