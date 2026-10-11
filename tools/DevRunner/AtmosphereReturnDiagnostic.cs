using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed partial class Runner
{
    // One focused owned reproduction, NOT scenic/native acceptance or a benchmark.
    // Reproduce the same preset/camera inputs and paid paused scene without eight
    // PNG readbacks, then record every modal/lifecycle boundary and one effect control.
    private async Task AtmosphereReturnDiagnostic(Child client, CancellationToken token)
    {
        Require(options.Renderer == "compatibility" && !options.Exported, "focused diagnostic owns source Compatibility only");
        async Task Preset(int weather, string view, int? glow = null)
        {
            string command = $"ui-atmosphere {weather} {view} 12" + (glow is null ? "" : $" {glow}");
            await client.Send(command);
            await client.WaitFor(e => e.Type == "atmosphere-proof" && e.Message == command, "diagnostic fixed scene/effect", options.StartupTimeout, token);
        }
        async Task<UiObservation> Boundary(string name)
        {
            Console.WriteLine($"RETURN DIAGNOSTIC awaiting {name} at {DateTimeOffset.UtcNow:O}");
            string path = Path.Combine(_scope!.EvidenceDirectory, "return-" + name + ".png");
            UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token, path);
            UiProtocol.Frame(frame, path);
            await File.WriteAllTextAsync(Path.ChangeExtension(path, ".json"), JsonSerializer.Serialize(frame, Evidence.JsonOptions), token);
            Console.WriteLine($"RETURN DIAGNOSTIC {name}: screen={frame.Screen}; connected={frame.Connected}; settings={frame.SettingsOpen}; leave={frame.ReturnConfirmationOpen}; modal={frame.ModalOpen}; dropdown={frame.GraphicsDropdownOpen}; focused={frame.FocusedControl}/{frame.GraphicsFocus}/{frame.ConfirmationFocus}; windowFocused={frame.WindowFocused}; glow={frame.Graphics.GlowEnabled}; processMs={frame.RenderCosts?.ProcessMilliseconds}; frameWaitMs={frame.ProbeCosts?.FrameWaitMilliseconds}; {path}");
            return frame;
        }
        await Preset(0, "menu");
        await Click(client, "Singleplayer", token);
        await Observe(client, s => s.Phase == Phase.Building && s.Players.Length == 1, "ordinary solo diagnostic setup", token);
        foreach ((int weather, string view) in new[] { (0, "overview"), (0, "forest"), (0, "river"), (2, "overview"), (3, "overview"), (4, "overview") }) await Preset(weather, view);
        await Action(client, "build 0 farm", token); await Action(client, "build 1 barracks", token); await Action(client, "build 2 metalmine", token);
        MatchSnapshot state = State(await Action(client, "ready", token)); await Action(client, "recruit 1", token);
        for (int i = 0; i < 4 && state.Phase is Phase.Building or Phase.Preparation; i++) state = State(await Action(client, "ready", token));
        Require(state.Phase == Phase.Combat, "diagnostic retains ordinary paid first-wave combat");
        state = State(await Action(client, "pause", token)); Require(state.Paused, "diagnostic uses the same paused scene");
        await Preset(0, "combat");
        UiObservation baseline = await Boundary("baseline");
        await Preset(0, "combat", 0);
        UiObservation withoutGlow = await Boundary("glow-off-control");
        Require(baseline.Graphics.GlowEnabled && !withoutGlow.Graphics.GlowEnabled && baseline.Graphics.Settings.SequenceEqual(withoutGlow.Graphics.Settings), "counterfactual changes only actual glow, not renderer, scene quality, desired settings or clock");
        await Preset(0, "combat");
        UiObservation restored = await Boundary("restored"); Require(restored.Graphics.GlowEnabled, "restore intended glow before lifecycle witnesses");
        string unchanged = Gameplay(Latest(client));
        await Click(client, "Settings", token);
        await GraphicsOptionInput(client, "renderer", 0, token); await Click(client, "ApplyGraphics", token);
        UiObservation applied = await Boundary("applied");
        Require(applied.Graphics.DesiredRenderer == "forward_plus" && applied.Graphics.Method == "gl_compatibility" && applied.Connected && Gameplay(Latest(client)) == unchanged, "real Apply retains desired/actual separation and live play");
        await Click(client, "ReturnToMenu", token);
        UiObservation confirmation = await Boundary("confirmation");
        Require(confirmation.ReturnConfirmationOpen && confirmation.ConfirmationFocus == "CancelReturn", "actual native leave modal owns Cancel focus");
        await Click(client, "ConfirmReturn", token);
        UiObservation menu = await Boundary("menu");
        Require(menu.Screen == "menu" && !menu.Connected && !menu.SettingsOpen && !menu.ReturnConfirmationOpen && !menu.ModalOpen && !menu.GraphicsDropdownOpen, "confirmed return disposes session and every modal");
        Console.WriteLine("RETURN DIAGNOSTIC awaiting actual Exit click");
        await Click(client, "Exit", token);
        Console.WriteLine("RETURN DIAGNOSTIC awaiting owned process exit");
        Require(await client.WaitExit(token) == 0, "actual Exit drains owners/audio and terminates cleanly");
        Require(!client.HasEngineErrors, "focused diagnostic records no engine errors");
    }
}
