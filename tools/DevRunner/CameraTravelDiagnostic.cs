using System.Diagnostics;
using System.Text.Json;

namespace DevRunner;

internal sealed partial class Runner
{
    // Bounded diagnosis of the unchanged economy travel predicate, not acceptance.
    // Same peers/seed/size/overview/input/15s bound. ONE new MSAA effect control;
    // preferences, world, input clocks, shadows, glow and motion remain unchanged.
    private async Task CameraTravelDiagnostic(Child client, CancellationToken token)
    {
        async Task<UiObservation> Record(string name)
        {
            string path = Path.Combine(_scope!.EvidenceDirectory, "travel-" + name + ".png");
            UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token, path);
            UiProtocol.Frame(frame, path);
            await File.WriteAllTextAsync(Path.ChangeExtension(path, ".json"), JsonSerializer.Serialize(frame, Evidence.JsonOptions), token);
            Console.WriteLine($"TRAVEL DIAGNOSTIC {name}: viewport={frame.Width}x{frame.Height}; windowFocus={frame.WindowFocused}; focus={frame.FocusedControl}; modal={frame.ModalOpen}; camera={JsonSerializer.Serialize(frame.Camera)}; graphics={JsonSerializer.Serialize(frame.Graphics)}; processMs={frame.RenderCosts?.ProcessMilliseconds}; frameWaitMs={frame.ProbeCosts?.FrameWaitMilliseconds}");
            return frame;
        }
        async Task<bool> Travel(string name)
        {
            await Click(client, "ResetView", token);
            UiObservation before = await Record(name + "-start");
            RequireOverview(before);
            Require(before.WindowFocused && !before.ModalOpen && before.FocusedControl.Length == 0, "same focused non-modal world owns travel input");
            var timer = Stopwatch.StartNew();
            bool passed = false;
            try
            {
                await client.Send("key-down D");
                await WaitUi(client, p => Math.Abs(p.Camera.PanX - 6) < 0.001 && Math.Abs(p.Camera.PanZ + 8) < 0.001, "bounded city travel", token);
                passed = true;
            }
            catch (TimeoutException error) { Console.WriteLine($"TRAVEL DIAGNOSTIC ORIGINAL BOUND FAILED {name}: {error.Message}"); }
            finally { await client.Send("key-up D"); }
            double wall = timer.Elapsed.TotalSeconds;
            UiObservation after = await Record(name + "-end");
            double processed = after.Camera.MovementSeconds - before.Camera.MovementSeconds;
            var expected = CameraPanExpectation.Calculate(before.Camera, processed, before.HudTop / before.Height, ["D"]);
            Require(Math.Abs(after.Camera.PanX - expected.X) < .025 && Math.Abs(after.Camera.PanZ - expected.Z) < .025, "actual movement follows original processed delta/yaw/HUD fraction/clamp, not an alternate target");
            Console.WriteLine(FormattableString.Invariant($"TRAVEL DIAGNOSTIC {name}: reachedOriginalTarget={passed}; wallSeconds={wall:R}; processedMovementSeconds={processed:R}; expected=({expected.X:R},{expected.Z:R}); actual=({after.Camera.PanX:R},{after.Camera.PanZ:R})"));
            return passed;
        }
        UiObservation initial = await Record("intended-initial");
        Require(initial.Width == 1100 && initial.Height == 820 && initial.Graphics.Method == "gl_compatibility" && initial.Graphics.Settings["aa"] == 3,
            "same original Compatibility 1100x820 desired Medium AA inputs; actual sampling is recorded separately");
        bool intended = await Travel("intended-msaa2x");
        if (intended)
        {
            Console.WriteLine("TRAVEL DIAGNOSTIC did not reproduce original failure; no counterfactual or causal fix justified.");
            return;
        }
        Require(initial.Graphics.Msaa == "Msaa2X", "only a reproduced original MSAA2x failure admits this one-effect counterfactual");
        await client.Send("ui-travel-msaa off");
        await client.WaitFor(e => e.Type == "travel-msaa" && e.Message == "off", "one owned transient MSAA control", options.StartupTimeout, token);
        try
        {
            UiObservation control = await Record("msaa-off-control");
            Require(control.Graphics.Msaa == "Disabled" && initial.Graphics.Settings.SequenceEqual(control.Graphics.Settings)
                && control.Graphics.RenderScale == initial.Graphics.RenderScale && control.Graphics.Shadows == initial.Graphics.Shadows
                && control.Graphics.GlowEnabled == initial.Graphics.GlowEnabled && control.Graphics.Method == initial.Graphics.Method,
                "only actual MSAA changes; desired settings/renderer/scale/shadows/glow stay intended");
            await Travel("msaa-off");
        }
        finally
        {
            await client.Send("ui-travel-msaa on");
            await client.WaitFor(e => e.Type == "travel-msaa" && e.Message == "on", "restore original MSAA after owned counterfactual", options.StartupTimeout, token);
            await Record("restored-msaa2x");
        }
        throw new InvalidOperationException("Original bounded city travel failed; diagnostic control does not convert this into acceptance.");
    }
}
