using Game.Core;
using Godot;

namespace Game.Verification;

// Minimal SceneTree, not Main/GameApplication, a match, or a multiplayer peer.
// Exercises production components which pure .NET cannot instantiate.
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
            string scenario = args is ["--scenario", var selected] ? selected : throw new ArgumentException("Select one engine scenario.");
            Main.Emit(new GameEvent("engine-ready", Message: scenario));
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            switch (scenario)
            {
                case "settings": await Settings(); await UiComponents(); break;
                case "animation": Animation(); break;
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
        RemoveChild(ui); ui.Free();
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
