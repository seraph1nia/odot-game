using Godot;

namespace Game.Verification;

public partial class EngineTests
{
    private Control _ui = null!;
    private async Task UiComponents()
    {
        GetWindow().Size = new(1100, 820);
        _ui = new Control { Theme = ApplicationTheme.Create(), MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(_ui); _ui.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        foreach (string component in new[] { "army_roster", "city_navigation", "match_controls", "modal", "quoted_action", "resource_ledger", "session_feedback", "skill_tree", "unit_inspection", "upkeep_summary" })
        {
            Control loaded = UiAssets.Component(component, _ui);
            Check(loaded.IsNodeReady(), "production scene loads: " + component);
            loaded.Hide(); loaded.QueueFree();
        }
        Theme theme = _ui.Theme;
        Check(theme.DefaultFont.ResourcePath == "res://UI/fonts/DejaVuSerif.ttf", "licensed font loads");
        Check(UiAssets.Texture("art/menu_seal.png").GetSize() == new Vector2(192, 192), "original seal loads at authored resolution");
        Check(((StyleBoxTexture)theme.GetStylebox("panel", "PanelContainer")).Texture.ResourcePath == "res://UI/art/woodland/branch_frame.svg", "woodland branch resource loads");
        Check(((StyleBoxFlat)theme.GetStylebox("focus", "Button")).BorderWidthTop == 2, "independent native focus border");
        Check(ApplicationTheme.ButtonStates.Select(s => theme.GetStylebox(s, "Button").ResourcePath).Distinct().Count() == 4, "four distinct native states");
        Control quote = UiAssets.Component("quoted_action", _ui);
        string requested = "";
        quote.Connect("requested", Callable.From<string>(id => requested = id));
        quote.Call("set_data", new Godot.Collections.Dictionary { ["id"] = "build-farm", ["title"] = "Bakery", ["quote"] = "4 wood", ["enabled"] = true, ["reason"] = "Authoritative fixture quote", ["icon"] = "wood" });
        quote.GetNode<Button>("Body/Action").EmitSignal(BaseButton.SignalName.Pressed);
        Check(requested == "build-farm" && quote.GetNode<Label>("Body/Quote").Text == "4 wood", "opaque quote intent and supplied cost projection");
        quote.Call("set_data", new Godot.Collections.Dictionary { ["title"] = "Bakery", ["enabled"] = false, ["reason"] = "Insufficient wood" });
        Check(quote.GetNode<Button>("Body/Action").Disabled && quote.GetNode<Label>("Body/Reason").Text == "Insufficient wood", "disabled quote retains exact supplied reason");
        quote.Hide();
        var definitions = new Game.Core.ResearchCatalog().Definitions();
        var centers = Tabletop.ResearchCenters(definitions);
        foreach (Game.Core.UnitClass role in Enum.GetValues<Game.Core.UnitClass>())
        {
            var nodes = definitions.Where(node => node.Class == role).ToArray();
            Check(nodes.All(node => new Rect2(0, 0, 900, 310).Encloses(new Rect2(centers[node.Id] - new Vector2(88, 44), new Vector2(176, 88)))) && nodes.Select(node => centers[node.Id]).Distinct().Count() == 5, "compact real-catalog " + role + " nodes have distinct bounded native footprints");
            Check(nodes.Where(node => node.Prerequisite != Game.Core.TechnologyId.None).All(node => centers[node.Id].Y < centers[node.Prerequisite].Y), "compact real-catalog " + role + " links follow actual prerequisites outward");
        }
        await ModalInput();
        await SettingsServices();
        Theme themeResource = _ui.Theme;
        _ui.Free();
        themeResource.Dispose();
    }
    private async Task Frames()
    {
        for (int i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
    private async Task ModalInput()
    {
        var dialog = new UiDialog { Title = "Owned native modal fixture", Theme = _ui.Theme };
        _ui.AddChild(dialog);
        // Regression seam: a menu created after Settings must not win GUI picking.
        var behind = new Button { Text = "Underlying menu", Size = new(1100, 820) };
        int behindClicks = 0, confirmations = 0;
        behind.Pressed += () => behindClicks++;
        _ui.AddChild(behind);
        behind.GrabFocus();
        dialog.Confirmed += () => confirmations++;
        var bodyAction = new Button { Text = "Body action" };
        dialog.Content.AddChild(bodyAction);
        var choice = new OptionButton();
        choice.AddItem("First choice"); choice.AddItem("Second choice");
        dialog.Content.AddChild(choice);
        dialog.PopupCenteredClamped(new(560, 420));
        await Frames();
        Check(_ui.GetChild(_ui.GetChildCount() - 1) == dialog, "active modal is front sibling for native GUI hit testing");
        Click(new(10, 10)); await Frames();
        Check(dialog.Visible && behindClicks == 0, "outside click neither dismisses modal nor reaches underlying menu");
        PressKey(Key.Tab); await Frames();
        Check(dialog.IsAncestorOf(GetViewport().GuiGetFocusOwner()), "native Tab stays within visible modal scope");
        bodyAction.FocusNeighborRight = bodyAction.GetPathTo(choice);
        bodyAction.GrabFocus(); PressKey(Key.Right); await Frames();
        Check(GetViewport().GuiGetFocusOwner() == choice, "native directional focus still traverses within modal content");
        Click(choice.GetGlobalRect().GetCenter()); await Frames();
        Check(choice.GetPopup().Visible && dialog.Visible, "native dropdown opens within active modal");
        choice.GetPopup().SetFocusedItem(0);
        PressKey(Key.Down); PressKey(Key.Enter); await Frames();
        Check(choice.Selected == 1 && !choice.GetPopup().Visible && dialog.Visible, "native dropdown retains directional selection and acceptance");
        Click(dialog.GetOkButton().GetGlobalRect().GetCenter()); await Frames();
        Check(confirmations == 1 && !dialog.Visible && behindClicks == 0, "actual headless native pointer routes footer intent before close");
        dialog.PopupCenteredClamped(new(560, 420)); await Frames();
        PressKey(Key.Escape); await Frames();
        Check(!dialog.Visible && confirmations == 1, "visible modal Esc cancels without confirming");
        Check(dialog.GetOkButton().Name == "Close" && dialog.GetCancelButton().Name == "Stay", "adapter retains kit-owned node identities");
        behind.Free();
        var currentInvoker = new Button { Text = "New session invoker" };
        _ui.AddChild(currentInvoker); currentInvoker.GrabFocus();
        dialog.PopupCenteredClamped(new(560, 420)); await Frames();
        Check(dialog.Visible && dialog.GetChild(0).Get("invoker").As<Control>() == currentInvoker,
            "reused modal captures current implicit invoker after previous session control is freed");
        dialog.Hide();
        dialog.Invoker = currentInvoker;
        currentInvoker.Free();
        dialog.PopupCenteredClamped(new(560, 420)); await Frames();
        Check(dialog.Visible && dialog.GetChild(0).Get("invoker").VariantType == Variant.Type.Nil,
            "reused modal refuses a disposed explicit invoker");
        dialog.Hide();
        var queuedInvoker = new Button(); _ui.AddChild(queuedInvoker);
        dialog.Invoker = queuedInvoker; queuedInvoker.QueueFree();
        dialog.PopupCenteredClamped(new(560, 420));
        Check(dialog.GetChild(0).Get("invoker").VariantType == Variant.Type.Nil, "modal refuses an explicit invoker queued for deletion");
        dialog.Hide();
        var detachedInvoker = new Button();
        dialog.Invoker = detachedInvoker;
        dialog.PopupCenteredClamped(new(560, 420));
        Check(dialog.GetChild(0).Get("invoker").VariantType == Variant.Type.Nil, "modal refuses a detached explicit invoker");
        dialog.Hide(); detachedInvoker.Free();
        dialog.QueueFree();
        await Frames();
    }
    private async Task SettingsServices()
    {
        GetWindow().Size = new(1100, 820);
        var settings = new ClientSettings(); AddChild(settings); settings.Initialize(_ui, _ui.Theme);
        var menu = new Button { Text = "Owned menu ordering fixture", Size = new(1100, 820) };
        _ui.AddChild(menu);
        settings.Open(); await Frames();
        Check(settings.IsOpen && settings.Categories.GetTabCount() == 3, "actual settings composition opens with graphics/audio/about");
        TabBar tabs = settings.Categories.GetTabBar();
        Click(tabs.GetGlobalTransformWithCanvas() * tabs.GetTabRect(1).GetCenter()); await Frames();
        Check(settings.Categories.CurrentTab == 1, "actual Settings native Audio tab receives pointer ahead of later menu sibling");
        tabs.GrabFocus(); PressKey(Key.Right); await Frames();
        Check(settings.Categories.CurrentTab == 2, "native Settings tab consumes Right without escaping modal");
        PressKey(Key.Left); await Frames();
        Check(settings.Categories.CurrentTab == 1, "native Settings tab consumes Left without escaping modal");
        settings.VolumeSlider.Value = 37;
        settings.VolumeSlider.GrabFocus(); PressKey(Key.Right); await Frames();
        Check(settings.MasterVolume == 38 && Math.Abs(AudioServer.GetBusVolumeLinear(AudioServer.GetBusIndex("Master")) - .38f) < .001f, "actual volume service applies native slider keyboard input");
        foreach (Vector2I size in new[] { new Vector2I(1100, 820), new Vector2I(1280, 720) })
        {
            GetWindow().Size = size; await Frames();
            Rect2 viewport = GetViewport().GetVisibleRect();
            Check(viewport.Encloses(settings.Dialog.GetOkButton().GetGlobalRect()) && viewport.Encloses(settings.Dialog.HeaderClose.GetGlobalRect()), "actual settings header/footer bounds remain reachable at " + size);
        }
        await NestedSettingsResize(settings);
        settings.Categories.CurrentTab = 0;
        int original = settings.Graphics["quality"];
        OptionButton quality = settings.Dialog.FindChildren("Graphics_quality", "OptionButton", true, false).OfType<OptionButton>().Single();
        quality.Select(2); quality.EmitSignal(OptionButton.SignalName.ItemSelected, 2L);
        Check(settings.GraphicsDraft["quality"] == 2 && settings.Graphics["quality"] == original, "native graphics intent changes draft, not applied preferences");
        Button apply = settings.Dialog.FooterContent.FindChildren("ApplyGraphics", "Button", true, false).OfType<Button>().Single();
        apply.EmitSignal(BaseButton.SignalName.Pressed);
        Check(settings.Graphics["quality"] == 2, "actual Apply commits graphics draft");
        settings.Close(false);
        using var saved = new ConfigFile();
        Check(saved.Load("user://settings.cfg") == Error.Ok && saved.GetValue("audio", "master_volume").AsInt32() == 38, "actual settings persist only in owned user data");
        Check(!Godot.FileAccess.FileExists("user://renderer.cfg"), "transient renderer context never creates a renderer preference override");
        menu.QueueFree(); DestroySettings(settings);
        await Frames();
    }
    private async Task NestedSettingsResize(ClientSettings settings)
    {
        var confirmation = new UiDialog { Title = "Return to menu?", Confirmation = true, Theme = _ui.Theme };
        _ui.AddChild(confirmation);
        int confirmations = 0, cancellations = 0;
        confirmation.Confirmed += () => confirmations++;
        confirmation.Canceled += () => cancellations++;
        confirmation.VisibilityChanged += () => settings.Dialog.SetInputScope(!confirmation.Visible);
        settings.InSession = true;
        settings.ReturnRequested = () =>
        {
            confirmation.PopupCenteredClamped(new(500, 190));
            confirmation.GetCancelButton().GrabFocus();
        };
        settings.ReturnButton.EmitSignal(BaseButton.SignalName.Pressed);
        await Frames();
        Control focus = GetViewport().GuiGetFocusOwner();
        foreach (Vector2I size in new[] { new Vector2I(1100, 820), new Vector2I(1280, 720) })
        {
            GetWindow().Size = size; await Frames();
            Check(settings.IsOpen && confirmation.Visible && _ui.GetChild(_ui.GetChildCount() - 1) == confirmation
                && GetViewport().GuiGetFocusOwner() == focus && !settings.Dialog.GetChild(0).IsProcessingInput(),
                "Settings resize preserves nested confirmation order, visibility, focus and suspended input at " + size);
        }
        PressKey(Key.Tab); await Frames();
        Check(confirmation.IsAncestorOf(GetViewport().GuiGetFocusOwner()), "nested confirmation retains Tab scope after resize");
        PressKey(Key.Escape); await Frames();
        Check(cancellations == 1 && confirmations == 0 && !confirmation.Visible && settings.IsOpen
            && settings.Dialog.GetChild(0).IsProcessingInput(), "Escape cancels only nested confirmation and resumes Settings scope");
        settings.Categories.CurrentTab = 0;
        int appliedQuality = settings.Graphics["quality"];
        OptionButton quality = settings.Dialog.Content.FindChildren("Graphics_quality", "OptionButton", true, false).OfType<OptionButton>().Single();
        quality.EmitSignal(OptionButton.SignalName.ItemSelected, 2L);
        Button apply = settings.Dialog.FooterContent.FindChildren("ApplyGraphics", "Button", true, false).OfType<Button>().Single();
        Button cancel = settings.Dialog.FooterContent.FindChildren("CancelGraphics", "Button", true, false).OfType<Button>().Single();
        int applyIntents = 0, cancelIntents = 0;
        apply.Pressed += () => applyIntents++;
        cancel.Pressed += () => cancelIntents++;
        settings.ReturnButton.EmitSignal(BaseButton.SignalName.Pressed); await Frames();
        confirmation.GetOkButton().GrabFocus(); PressKey(Key.Down); await Frames();
        Check(confirmation.IsAncestorOf(GetViewport().GuiGetFocusOwner()), "native Down from confirmation footer cannot escape into Settings");
        PressKey(Key.Enter); await Frames();
        Check(confirmations == 1 && !confirmation.Visible && settings.IsOpen && applyIntents == 0 && cancelIntents == 0
            && settings.Graphics["quality"] == appliedQuality && settings.GraphicsDraft["quality"] == 2,
            "Down and Enter confirm only the foreground modal, leaving the Settings draft untouched");
        foreach (var (key, side, target) in new[] { (Key.Left, Side.Left, apply), (Key.Right, Side.Right, cancel), (Key.Up, Side.Top, apply), (Key.Down, Side.Bottom, cancel) })
        {
            settings.ReturnButton.EmitSignal(BaseButton.SignalName.Pressed); await Frames();
            Button source = confirmation.GetOkButton();
            source.SetFocusNeighbor(side, source.GetPathTo(target));
            source.GrabFocus();
            Check(source.FindValidFocusNeighbor(side) == target, "fixture exposes an outgoing native " + key + " focus path to suspended Settings");
            PressKey(key); await Frames();
            Check(confirmation.IsAncestorOf(GetViewport().GuiGetFocusOwner()), "modal contains native " + key + " focus traversal");
            int before = confirmations;
            PressKey(Key.Enter); await Frames();
            Check(confirmations == before + 1 && !confirmation.Visible && settings.IsOpen && applyIntents == 0 && cancelIntents == 0
                && settings.Graphics["quality"] == appliedQuality && settings.GraphicsDraft["quality"] == 2,
                key + " and Enter cannot activate suspended Settings Apply or Cancel");
        }
        settings.ReturnRequested = null;
        settings.InSession = false;
        confirmation.Free();
    }
    private static void Click(Vector2 point)
    {
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = point, GlobalPosition = point, Pressed = true });
        Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = point, GlobalPosition = point, Pressed = false });
    }
    private static void PressKey(Key key)
    {
        Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = true });
        Input.ParseInputEvent(new InputEventKey { Keycode = key, PhysicalKeycode = key, Pressed = false });
    }
}
