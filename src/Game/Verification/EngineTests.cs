using Game.Core;
using Godot;

namespace Game.Verification;

// Minimal SceneTree, not Main/GameApplication, a world, or a multiplayer peer.
// Receiving uses numerical Match authority, not a launched application/session.
public partial class EngineTests : Node
{
    public override async void _Ready()
    {
        try
        {
            var ownership = new VerificationPacing(System.Environment.GetEnvironmentVariable("ODOT_OWNED_DATA"),
                System.Environment.GetEnvironmentVariable("ODOT_VERIFICATION_MARKER"), System.Environment.GetEnvironmentVariable("ODOT_VERIFICATION_TOKEN"));
            if (DisplayServer.GetName() != "headless" || !ownership.Owned)
                throw new InvalidOperationException("Engine tests require owned headless runtime state.");
            string[] args = OS.GetCmdlineUserArgs();
            string scenario = args switch
            {
                ["--scenario", var selected] => selected,
                ["--supervised", "--engine-component-probe", "worldtree"] => "worldtree",
                _ => throw new ArgumentException("Select one engine scenario.")
            };
            Console.WriteLine($"MEASURE component invocation: executable={OS.GetExecutablePath()}; runtime={System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}; root={System.Environment.GetEnvironmentVariable("DOTNET_ROOT")}; rootX64={System.Environment.GetEnvironmentVariable("DOTNET_ROOT_X64")}; rollForward={System.Environment.GetEnvironmentVariable("DOTNET_ROLL_FORWARD")}; args={string.Join(' ', ClientSettings.EngineArguments())}");
            Main.Emit(new GameEvent("engine-ready", Message: scenario));
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            switch (scenario)
            {
                case "settings": await Settings(); await CameraBoundaries(); await UiComponents(); PrepareRendererRestart(0); break;
                case "renderer-forward": RendererStartup("forward_plus"); PrepareRendererRestart(1); break;
                case "renderer-compatibility": RendererStartup("gl_compatibility"); break;
                case "animation": Animation(); await CombatPresentation(); break;
                case "worldtree": await Worldtree(); break;
                case "receiving": await Receiving(); break;
                case "arena": await Arena(); break;
                case "assets": await AssetFidelityProbe.Run(this); return;
                default: throw new ArgumentException("Unknown engine scenario: " + scenario);
            }
            Main.Emit(new GameEvent("engine-test", Message: scenario));
            GetTree().Quit();
        }
        catch (Exception error)
        {
            Main.Emit(new GameEvent("error", Message: error.ToString()));
            GetTree().Quit(1);
        }
    }

    private async Task Settings()
    {
        Check(!Godot.FileAccess.FileExists("user://renderer.cfg") && ProjectSettings.GetSetting("rendering/renderer/rendering_method").AsString() == "forward_plus"
            && RenderingServer.GetCurrentRenderingMethod() == "forward_plus" && RenderingServer.GetCurrentRenderingDriverName() == "vulkan",
            "fresh native headless configuration requests and reports Forward+ / Vulkan before settings, not just a menu label");
        var ui = new Control(); AddChild(ui);
        using Theme theme = ApplicationTheme.Create();
        ClientSettings Create()
        {
            var settings = new ClientSettings(); AddChild(settings); settings.Initialize(ui, theme); return settings;
        }
        ClientSettings first = Create();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Check(first.MasterVolume == 50, "fresh preferences use default gain");
        first.Open();
        Check(first.IsOpen, "settings button component opens its actual dialog");
        first.VolumeSlider.Value = 0;
        int bus = AudioServer.GetBusIndex("Master");
        Check(first.MasterVolume == 0 && AudioServer.IsBusMute(bus), "volume signal mutes the actual audio bus");
        first.VolumeSlider.Value = 37;
        Check(!AudioServer.IsBusMute(bus) && Math.Abs(AudioServer.GetBusVolumeLinear(bus) - .37f) < .001,
            "volume signal updates actual bus gain");
        first.VolumeSlider.EmitSignal(Slider.SignalName.DragEnded, true);
        // The production control's signal path, not a duplicate graphics implementation.
        GraphicsPreferences? preview = null;
        first.GraphicsApplied = preferences => preview = preferences.Clone();
        var forest = first.Dialog.Content.FindChildren("Graphics_forest_theme", "OptionButton", true, false).OfType<OptionButton>().Single();
        var magic = first.Dialog.Content.FindChildren("Graphics_magic", "OptionButton", true, false).OfType<OptionButton>().Single();
        forest.EmitSignal(OptionButton.SignalName.ItemSelected, 0L);
        magic.EmitSignal(OptionButton.SignalName.ItemSelected, 2L);
        Check(first.Graphics["forest_theme"] == 2 && first.Graphics["magic"] == 1 && preview is not null && preview["forest_theme"] == 0 && preview["magic"] == 2,
            "art signals preview a local draft without committing default Dreaming/Subtle preferences");
        first.Dialog.FooterContent.FindChildren("CancelGraphics", "Button", true, false).OfType<Button>().Single().EmitSignal(BaseButton.SignalName.Pressed);
        Check(preview is not null && preview["forest_theme"] == 2 && preview["magic"] == 1, "Cancel restores applied artistic preview");
        forest.EmitSignal(OptionButton.SignalName.ItemSelected, 1L);
        magic.EmitSignal(OptionButton.SignalName.ItemSelected, 0L);
        var weather = first.Dialog.Content.FindChildren("Graphics_weather", "OptionButton", true, false).OfType<OptionButton>().Single();
        weather.EmitSignal(OptionButton.SignalName.ItemSelected, 2L);
        Check(first.Graphics["weather"] != 2 && first.GraphicsDraft["weather"] == 2, "graphics edits remain draft before Apply");
        var apply = first.Dialog.FooterContent.FindChildren("ApplyGraphics", "Button", true, false).OfType<Button>().Single();
        apply.EmitSignal(BaseButton.SignalName.Pressed);
        Check(first.Graphics["weather"] == 2 && first.Graphics["forest_theme"] == 1 && first.Graphics["magic"] == 0, "Apply signal commits independent weather/theme/magic preferences");
        // Reset belongs to the scrollable Graphics page; only Apply/Cancel live in the footer.
        first.Dialog.Content.FindChildren("ResetGraphics", "Button", true, false).OfType<Button>().Single().EmitSignal(BaseButton.SignalName.Pressed);
        Check(first.GraphicsDraft["forest_theme"] == 2 && first.GraphicsDraft["magic"] == 1 && first.Graphics["forest_theme"] == 1,
            "Reset previews cancellable art defaults");
        first.Close(false);
        Check(!first.IsOpen, "close dismisses dialog");
        DestroySettings(first);
        Check(ui.GetChildCount() == 0, "settings reload releases externally parented controls");
        ClientSettings restored = Create();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Check(restored.MasterVolume == 37 && restored.Graphics["weather"] == 2 && restored.Graphics["forest_theme"] == 1 && restored.Graphics["magic"] == 0, "new component reloads persisted volume and graphics");
        Check(!AudioServer.IsBusMute(bus) && Math.Abs(AudioServer.GetBusVolumeLinear(bus) - .37f) < .001,
            "reload applies persisted audio gain");
        DestroySettings(restored);
        using (var invalid = new ConfigFile())
        {
            Check(invalid.Load("user://settings.cfg") == Error.Ok, "isolated saved preferences available");
            invalid.SetValue("visuals", "forest_theme", "dreaming"); invalid.SetValue("visuals", "magic", 99);
            Check(invalid.Save("user://settings.cfg") == Error.Ok, "invalid art fixture stored in owned preferences only");
        }
        ClientSettings fallback = Create();
        Check(fallback.Graphics["forest_theme"] == 2 && fallback.Graphics["magic"] == 1 && fallback.Graphics["weather"] == 2 && fallback.MasterVolume == 37,
            "invalid saved art fields fallback independently, leaving valid weather/audio intact");
        DestroySettings(fallback);
        // Headless proves the production signal -> ConfigFile startup request,
        // not GPU initialization. Active method stays authoritative for resources.
        ClientSettings rendererSettings = Create(); rendererSettings.Open();
        var renderer = rendererSettings.Dialog.Content.FindChildren("Graphics_renderer", "OptionButton", true, false).OfType<OptionButton>().Single();
        var volumetric = rendererSettings.Dialog.Content.FindChildren("Graphics_volumetric", "OptionButton", true, false).OfType<OptionButton>().Single();
        var rendererApply = rendererSettings.Dialog.FooterContent.FindChildren("ApplyGraphics", "Button", true, false).OfType<Button>().Single();
        Check(rendererSettings.Graphics.DesiredMethod == "forward_plus", "fresh requested renderer is real Forward+ default, not Compatibility");
        using (var stale = new ConfigFile())
        {
            stale.SetValue("rendering", "renderer/rendering_method", "gl_compatibility");
            Check(stale.Save("user://renderer.cfg") == Error.Ok, "owned stale startup override fixture");
        }
        Check(renderer.AllowReselect && renderer.Selected == 0, "real renderer OptionButton permits explicit same-value re-selection");
        PopupMenu rendererPopup = renderer.GetPopup();
        Console.WriteLine($"MEASURE renderer popup wiring: indexPressed={rendererPopup.GetSignalConnectionList(PopupMenu.SignalName.IndexPressed).Count}; idPressed={rendererPopup.GetSignalConnectionList(PopupMenu.SignalName.IdPressed).Count}");
        Check(rendererPopup.GetSignalConnectionList(PopupMenu.SignalName.IndexPressed).Count > 0, "actual OptionButton owns the popup index signal path");
        renderer.AllowReselect = false;
        rendererPopup.EmitSignal(PopupMenu.SignalName.IndexPressed, 0L); rendererApply.EmitSignal(BaseButton.SignalName.Pressed);
        using (var oldControl = new ConfigFile())
        {
            Check(oldControl.Load("user://renderer.cfg") == Error.Ok && oldControl.GetValue("rendering", "renderer/rendering_method").AsString() == "gl_compatibility",
                "same-value native popup selection without AllowReselect reproduces retained stale startup override");
        }
        renderer.AllowReselect = true;
        rendererPopup.EmitSignal(PopupMenu.SignalName.IndexPressed, 0L);
        rendererSettings.Dialog.FooterContent.FindChildren("CancelGraphics", "Button", true, false).OfType<Button>().Single().EmitSignal(BaseButton.SignalName.Pressed);
        rendererApply.EmitSignal(BaseButton.SignalName.Pressed);
        using (var canceled = new ConfigFile())
        {
            Check(canceled.Load("user://renderer.cfg") == Error.Ok && canceled.GetValue("rendering", "renderer/rendering_method").AsString() == "gl_compatibility", "Cancel preserves existing startup choice");
        }
        // Native popup index signal exercises OptionButton's actual built-in
        // handler and re-selection policy. ItemSelected would bypass both;
        // IdPressed is not connected by this native OptionButton.
        rendererPopup.EmitSignal(PopupMenu.SignalName.IndexPressed, 0L);
        Check(!volumetric.Disabled && !volumetric.IsItemDisabled(1), "requested Forward+ permits next-launch volumetric configuration");
        volumetric.EmitSignal(OptionButton.SignalName.ItemSelected, 1L); rendererApply.EmitSignal(BaseButton.SignalName.Pressed);
        using (var repaired = new ConfigFile())
        {
            Check(repaired.Load("user://renderer.cfg") == Error.Ok && repaired.GetValue("rendering", "renderer/rendering_method").AsString() == "forward_plus"
                && repaired.GetValue("rendering", "rendering_device/driver").AsString() == "vulkan", "explicit same-default selection repairs stale startup override before next engine initialization");
        }
        Check(rendererSettings.Graphics.Effective(false)["volumetric"] == 0 && rendererSettings.Graphics.Effective(true)["volumetric"] == 1,
            "unsupported active method masks volumetric without destroying the supported requested preference");
        renderer.EmitSignal(OptionButton.SignalName.ItemSelected, 1L); rendererApply.EmitSignal(BaseButton.SignalName.Pressed);
        Check(volumetric.Disabled == !AtmosphereController.ForwardPlus, "Compatibility request cannot pretend unsupported fog is active");
        DestroySettings(rendererSettings);
        using (var cleanup = new ConfigFile())
        {
            cleanup.Load("user://settings.cfg"); cleanup.SetValue("visuals", "renderer", 0); cleanup.Save("user://settings.cfg");
        }
        DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath("user://renderer.cfg"));
        RemoveChild(ui); ui.Free();
    }

    private void RendererStartup(string expected)
    {
        string requested = ProjectSettings.GetSetting("rendering/renderer/rendering_method").AsString();
        string active = RenderingServer.GetCurrentRenderingMethod(), driver = RenderingServer.GetCurrentRenderingDriverName();
        Check(requested == expected, "native next-process project override applies saved request: " + expected);
        Console.WriteLine($"MEASURE renderer startup: requested={requested}; active={active}; driver={driver}; forwardPlus={AtmosphereController.ForwardPlus}");
        // The headless driver is not proof that a Vulkan GPU can start. Its
        // actual capability, not the persisted request, owns effective options.
        var preferences = new GraphicsPreferences();
        Check(preferences.Effective(AtmosphereController.ForwardPlus)["volumetric"] == (AtmosphereController.ForwardPlus ? 1 : 0),
            "headless startup keeps actual-method volumetric fallback separate from request");
        var atmosphere = new AtmosphereController(); AddChild(atmosphere);
        try
        {
            atmosphere.Apply(preferences);
            Check(atmosphere.Environment.VolumetricFogEnabled == AtmosphereController.ForwardPlus,
                "production environment application enables fog only for supported active method");
            preferences.Edit("atmosphere", 0); atmosphere.Apply(preferences);
            Check(!atmosphere.Environment.VolumetricFogEnabled, "atmosphere Off still disables the actual fog resource");
        }
        finally { RemoveChild(atmosphere); atmosphere.Free(); }
    }
    private void PrepareRendererRestart(int selected)
    {
        var ui = new Control(); AddChild(ui);
        using Theme theme = ApplicationTheme.Create();
        var settings = new ClientSettings(); AddChild(settings); settings.Initialize(ui, theme); settings.Open();
        var renderer = settings.Dialog.Content.FindChildren("Graphics_renderer", "OptionButton", true, false).OfType<OptionButton>().Single();
        renderer.EmitSignal(OptionButton.SignalName.ItemSelected, (long)selected);
        settings.Dialog.FooterContent.FindChildren("ApplyGraphics", "Button", true, false).OfType<Button>().Single().EmitSignal(BaseButton.SignalName.Pressed);
        DestroySettings(settings); RemoveChild(ui); ui.Free();
    }
    private static void DestroySettings(ClientSettings settings)
    {
        UiDialog dialog = settings.Dialog;
        Button button = settings.SettingsButton;
        settings.GetParent().RemoveChild(settings);
        settings.Free();
        dialog.Free();
        button.Free();
    }

    private void Animation()
    {
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (UnitType type in Enum.GetValues<UnitType>())
            {
                Node3D model = UnitAssets.Instantiate(UnitAssets.Character(type, faction)); AddChild(model);
                try
                {
                    var rig = UnitAssets.Bind(model, type);
                    UnitAssets.Equip(model, rig.Skeleton, type, faction);
                    rig.Player.Play("attack"); rig.Player.Advance(0);
                    Quaternion[] before = Enumerable.Range(0, rig.Skeleton.GetBoneCount()).Select(rig.Skeleton.GetBonePoseRotation).ToArray();
                    rig.Player.Advance(AssetCatalog.AttackMarker);
                    Check(Enumerable.Range(0, before.Length).Any(i => !before[i].IsEqualApprox(rig.Skeleton.GetBonePoseRotation(i))),
                        $"{faction}/{type}: imported attack actually changes skeleton pose");
                    Check(rig.Player.GetAnimation("attack").LoopMode == Godot.Animation.LoopModeEnum.None
                        && rig.Player.GetAnimation("walk").LoopMode == Godot.Animation.LoopModeEnum.Linear,
                        $"{faction}/{type}: component sets combat/locomotion loop policy");
                    rig.Player.Stop();
                }
                finally { RemoveChild(model); model.Free(); }
            }
    }

    private static void Check(bool passed, string behavior)
    {
        if (!passed) throw new InvalidOperationException(behavior);
        Console.WriteLine("PASS engine: " + behavior);
    }
}
