using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed record GraphicsObservation
{
    public string DesiredRenderer { get; init; } = "";
    public string Method { get; init; } = "";
    public string Driver { get; init; } = "";
    public string Gpu { get; init; } = "";
    public string Vendor { get; init; } = "";
    public string DeviceType { get; init; } = "";
    public string Api { get; init; } = "";
    public bool RenderingDevice { get; init; }
    public string Display { get; init; } = "";
    public Dictionary<string, int> Settings { get; init; } = [];
    public Dictionary<string, int> Effective { get; init; } = [];
    public float RenderScale { get; init; }
    public string Msaa { get; init; } = "";
    public bool Taa { get; init; }
    public string Fxaa { get; init; } = "";
    public int FpsLimit { get; init; }
    public string Vsync { get; init; } = "";
    public bool Shadows { get; init; }
    public bool SsaoEnabled { get; init; }
    public bool SsilEnabled { get; init; }
    public bool FogEnabled { get; init; }
    public float FogDensity { get; init; }
    public bool VolumetricFogEnabled { get; init; }
    public float VolumetricFogDensity { get; init; }
    public bool GlowEnabled { get; init; }
    public float TonemapExposure { get; init; }
    public float SunEnergy { get; init; }
    public float Wind { get; init; }
    public bool Rain { get; init; }
    public float RainRatio { get; init; }
    public bool Leaves { get; init; }
    public bool LocalFog { get; init; }
    public float CosmeticTime { get; init; }
    public long SkyUploads { get; init; }
}

internal sealed partial class Runner
{
    internal static bool HardwareVulkan(GraphicsObservation graphics) => graphics.Method == "forward_plus" && graphics.Driver == "vulkan" && graphics.RenderingDevice &&
        graphics.DeviceType is "DiscreteGpu" or "IntegratedGpu" && graphics.Display == "X11";

    private async Task AtmosphereScenario(CancellationToken token)
    {
        bool vulkan = options.Renderer == "forward-plus";
        string? executable = options.Exported ? AtmosphereClientPackage() : null;
        Child client = StartGameRole("atmosphere-client", "menu", false, 0, executable, "--combat-seed", "1");
        await client.WaitFor(e => e.Type == "menu", "owned graphical atmosphere start", options.StartupTimeout, token);
        if (options.UiCheckpoint is "vanilla-popup" or "vanilla-native")
        {
            await VanillaPopupDiagnostic(client, token);
            return;
        }
        if (options.UiCheckpoint == "startup-diagnostic")
        {
            await StartupObservationDiagnostic(client, token);
            return;
        }
        UiObservation initial = await WaitUi(client, p => p.Models > 0 && p.Screen == "menu", "authentic rendered menu", token);
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "actual-renderer.json"), JsonSerializer.Serialize(initial.Graphics, Evidence.JsonOptions), token);
        if (vulkan && (!HardwareVulkan(initial.Graphics) || initial.NativeWindow == 0))
            throw new VerificationPrerequisiteException($"Native Vulkan hardware/surface qualification absent: actual={initial.Graphics.Method}/{initial.Graphics.Driver}, device={initial.Graphics.DeviceType}, GPU={initial.Graphics.Gpu}, display={initial.Graphics.Display}. Software Vulkan and backend fallback are not a hardware pass.");
        Require(vulkan ? HardwareVulkan(initial.Graphics) : initial.Graphics.Method == "gl_compatibility" && initial.Graphics.Driver == "opengl3", "actual source/export method and driver match the explicitly selected renderer");
        Require(!client.HasEngineErrors, "renderer and atmosphere shaders initialize without engine errors");
        Require(initial.ProcessId == client.ProcessId && initial.WindowMode == "Windowed" && initial.NativeWindow != 0, "actual rendered native window belongs to this dedicated windowed child");
        if (options.UiCheckpoint == "popup-diagnostic")
        {
            await PopupLifecycleDiagnostic(client, token);
            return;
        }
        if (options.UiCheckpoint == "return-diagnostic")
        {
            await AtmosphereReturnDiagnostic(client, token);
            return;
        }
        UiObservation menu = await AtmosphereFrame(client, "menu-landscape", 0, "menu", token);
        await Click(client, "Singleplayer", token);
        MatchSnapshot state = await Observe(client, s => s.Phase == Phase.Building && s.Players.Length == 1, "ordinary deterministic solo setup", token);
        await WaitUi(client, p => p.Screen == "session" && p.Models > 0, "same atmosphere in solo village", token);
        await AtmosphereFrame(client, "village-overview", 0, "overview", token);
        await AtmosphereFrame(client, "zoomed-forest", 0, "forest", token);
        UiObservation river = await AtmosphereFrame(client, "river-bridge", 0, "river", token);
        Require(river.Graphics.SkyUploads == menu.Graphics.SkyUploads, "exact stable sky color uploads are suppressed across real rendered frames");
        UiObservation mist = await AtmosphereFrame(client, "misty-morning", 2, "overview", token);
        Require(mist.Graphics.SkyUploads > river.Graphics.SkyUploads, "changed weather still uploads its actual sky palette");
        await AtmosphereFrame(client, "gentle-rain", 3, "overview", token);
        await AtmosphereFrame(client, "evening", 4, "overview", token);
        await Action(client, "build 0 farm", token);
        await Action(client, "build 1 barracks", token);
        await Action(client, "build 2 metalmine", token);
        state = State(await Action(client, "ready", token));
        await Action(client, "recruit 1", token);
        for (int i = 0; i < 4 && state.Phase is Phase.Building or Phase.Preparation; i++) state = State(await Action(client, "ready", token));
        Require(state.Phase == Phase.Combat, "ordinary paid first wave provides actual combat evidence, not a presentation-only fake");
        state = State(await Action(client, "pause", token));
        Require(state.Paused && state.Phase == Phase.Combat, "combat capture uses a deterministic paused authority state");
        await WaitUi(client, p => p.MatchPhase == Phase.Combat && p.Units.Length > 0, "actual combat models and health bars", token);
        await AtmosphereFrame(client, "active-combat", 0, "combat", token);
        Require(!client.HasEngineErrors, "all native weather/material variants render without shader/engine errors");
        await RendererPreferenceProof(client, vulkan, executable, token);
    }
    private async Task RendererPreferenceProof(Child client, bool vulkan, string? executable, CancellationToken token)
    {
        string unchanged = Gameplay(Latest(client));
        await Click(client, "Settings", token);
        await GraphicsOptionInput(client, "renderer", vulkan ? 1 : 0, token);
        await Click(client, "ApplyGraphics", token);
        UiObservation applied = await UiProtocol.Probe(client, options.StartupTimeout, token);
        string desired = vulkan ? "gl_compatibility" : "forward_plus";
        Require(applied.Graphics.DesiredRenderer == desired && applied.Graphics.Method == (vulkan ? "forward_plus" : "gl_compatibility") && applied.Connected && Gameplay(Latest(client)) == unchanged,
            "desired renderer is separate from actual rendering and Apply never restarts or changes live play");
        // Deliberate native controls end the session before this owned restart.
        await Click(client, "ReturnToMenu", token); await Click(client, "ConfirmReturn", token);
        await WaitUi(client, p => p.Screen == "menu" && !p.Connected, "explicit safe return before renderer restart", token);
        await Click(client, "Exit", token); Require(await client.WaitExit(token) == 0, "owned atmosphere client exits cleanly");
        if (!vulkan) return; // Compatibility lane must not opportunistically demand Vulkan hardware.
        var saved = new Runner(options with { UseSavedRenderer = true }, token, _evidence, _scope);
        Child restarted = saved.StartGameRole("atmosphere-client", "menu", false, 0, executable);
        await restarted.WaitFor(e => e.Type == "menu", "saved native renderer bootstrap", options.StartupTimeout, token);
        UiObservation fallback = await UiProtocol.Probe(restarted, options.StartupTimeout, token);
        Require(fallback.Graphics.Method == "gl_compatibility" && fallback.Graphics.Driver == "opengl3" && fallback.Graphics.DesiredRenderer == "gl_compatibility", "native user renderer override is honored before source/export engine initialization");
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "saved-renderer-bootstrap.json"), JsonSerializer.Serialize(fallback.Graphics, Evidence.JsonOptions), token);
        await restarted.Send("quit"); Require(await restarted.WaitExit(token) == 0, "saved-renderer witness closes before explicit override witness");
        Child overridden = StartGameRole("atmosphere-client", "menu", false, 0, executable);
        await overridden.WaitFor(e => e.Type == "menu", "explicit Vulkan override of saved Compatibility", options.StartupTimeout, token);
        UiObservation overrideFrame = await UiProtocol.Probe(overridden, options.StartupTimeout, token);
        Require(HardwareVulkan(overrideFrame.Graphics) && overrideFrame.Graphics.DesiredRenderer == "gl_compatibility", "explicit source/export Vulkan override is transient, authentic and does not overwrite saved desired Compatibility");
        await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, "transient-renderer-override.json"), JsonSerializer.Serialize(overrideFrame.Graphics, Evidence.JsonOptions), token);
        await overridden.Send("quit"); Require(await overridden.WaitExit(token) == 0, "explicit Vulkan witness exits cleanly");
    }
    private async Task<UiObservation> AtmosphereFrame(Child client, string name, int weather, string view, CancellationToken token)
    {
        string command = $"ui-atmosphere {weather} {view} 12";
        await client.Send(command);
        await client.WaitFor(e => e.Type == "atmosphere-proof" && e.Message == command, "owned fixed-clock atmospheric preset", options.StartupTimeout, token);
        string path = Path.Combine(_scope!.EvidenceDirectory, name + ".png");
        UiObservation frame = await UiProtocol.Probe(client, options.StartupTimeout, token, path);
        UiProtocol.Frame(frame, path);
        Require(frame.Graphics.CosmeticTime == 12 && frame.Graphics.Settings["weather"] == weather, "fixed cosmetic clock and manually selected preset reach real presentation");
        Require(frame.Graphics.FogDensity <= .4f && frame.Graphics.VolumetricFogDensity < .006f && frame.Graphics.TonemapExposure is >= .8f and <= 1.2f, "fog and exposure stay within conservative readable bounds");
        if (weather == 3) Require(frame.Graphics.Rain && frame.Graphics.RainRatio > 0, "real native bounded rain emitter is active");
        if (frame.Graphics.Method == "gl_compatibility") Require(!frame.Graphics.VolumetricFogEnabled && !frame.Graphics.SsilEnabled && !frame.Graphics.Taa, "Compatibility uses actual supported fog/AA/lighting fallback");
        if (frame.Screen == "session") HealthBars(frame);
        await File.WriteAllTextAsync(Path.Combine(_scope.EvidenceDirectory, name + ".json"), JsonSerializer.Serialize(frame, Evidence.JsonOptions), token);
        Console.WriteLine($"ATMOSPHERE FRAME {name}: {frame.Graphics.Method}/{frame.Graphics.Driver}; quality={frame.Graphics.Settings["quality"]}; {frame.Graphics.DeviceType}; {frame.Graphics.Gpu}; {path}");
        return frame;
    }
}
