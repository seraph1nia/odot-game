using Godot;

namespace Game;

public partial class ClientSettings
{
    internal GraphicsPreferences Graphics { get; private set; } = new();
    internal Action<GraphicsPreferences>? GraphicsApplied { get; set; }
    internal IReadOnlyDictionary<string, int> GraphicsDraft => _draftGraphics.Values;
    internal IReadOnlyDictionary<string, int> GraphicsPopupFocus => _graphicsControls.ToDictionary(pair => pair.Key, pair => pair.Value.GetPopup().GetFocusedItem());
    internal bool GraphicsDropdownOpen => _graphicsControls.Values.Any(control => control.GetPopup().Visible);
    internal string GraphicsFocus => _dialog.GuiGetFocusOwner()?.Name.ToString() ?? "";
    private GraphicsPreferences _draftGraphics = new();
    private readonly Dictionary<string, OptionButton> _graphicsControls = [];
    private HSlider _exposure = null!;
    private Label _rendererStatus = null!;
    private Window.ModeEnum _previewMode;
    private Vector2I _previewSize;
    private bool _previewEdited, _rendererEdited, _commitDisplay;
    private Button _applyGraphics = null!, _cancelGraphics = null!, _resetGraphics = null!;
    private ScrollContainer _graphicsScroll = null!;

    private void BuildGraphicsControls(VBoxContainer page, VBoxContainer content)
    {
        _rendererStatus = Label(page, ""); _rendererStatus.Name = "ActiveRenderer";
        Label(page, "Display changes preview immediately. Apply keeps them; Cancel restores them. Renderer changes take effect next launch, never during a live game.");
        string group = "Display and performance";
        foreach (GraphicsOption option in GraphicsPreferences.Options)
        {
            if (option.Group != group) { group = option.Group; Label(page, group); }
            Label(page, option.Label);
            var control = new OptionButton { Name = "Graphics_" + option.Key, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, CustomMinimumSize = new(0, 32) };
            control.GetPopup().ForceNative = true;
            foreach (string choice in option.Choices) control.AddItem(choice);
            page.AddChild(control); _graphicsControls.Add(option.Key, control);
            control.ItemSelected += index => { _draftGraphics.Edit(option.Key, (int)index); RefreshGraphics(); };
            AttachPopupTrace(control);
        }
        Label(page, "Brightness / exposure (80–120%)");
        _exposure = new HSlider { Name = "GraphicsExposure", MinValue = 80, MaxValue = 120, Step = 1, Value = 100, CustomMinimumSize = new(0, 32), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        page.AddChild(_exposure);
        _exposure.ValueChanged += value => { _draftGraphics.SetExposure((int)value); RefreshGraphics(); };
        _resetGraphics = new Button { Name = "ResetGraphics", Text = "Reset graphics to defaults", CustomMinimumSize = new(0, 38) }; page.AddChild(_resetGraphics);
        _resetGraphics.Pressed += () => { _draftGraphics = new(); RefreshGraphics(); };
        var actions = new HBoxContainer(); content.AddChild(actions);
        _applyGraphics = new Button { Name = "ApplyGraphics", Text = "Apply graphics", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        _cancelGraphics = new Button { Name = "CancelGraphics", Text = "Cancel graphics changes", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        actions.AddChild(_applyGraphics); actions.AddChild(_cancelGraphics);
        _applyGraphics.Pressed += ApplyGraphics; _cancelGraphics.Pressed += CancelGraphics;
        _dialog.Confirmed += ApplyGraphics;
        RefreshGraphics();
    }
    private void BeginGraphicsEdit()
    {
        _draftGraphics = Graphics.Clone();
        _previewMode = _window.Mode; _previewSize = _windowedSize; _previewEdited = _displayEdited;
        RefreshGraphics();
    }
    private void RefreshGraphics()
    {
        bool forward = AtmosphereController.ForwardPlus;
        foreach (GraphicsOption option in GraphicsPreferences.Options)
        {
            OptionButton control = _graphicsControls[option.Key];
            for (int i = 0; i < option.Choices.Length; i++) control.SetItemDisabled(i, !GraphicsPreferences.Supported(option.Key, i, forward));
            control.Disabled = !forward && option.Key is "volumetric" or "indirect";
            int desired = _draftGraphics[option.Key];
            control.Select(GraphicsPreferences.Supported(option.Key, desired, forward) ? desired : _draftGraphics.Effective(forward)[option.Key]);
            bool compatibilityOff = !forward && option.Key == "aa" && desired is 1 or 3;
            control.TooltipText = compatibilityOff ? "Compatibility uses Off for desired FXAA (unsupported) or MSAA 2× (default cost policy). Your desired value is retained; High still uses supported MSAA 4×."
                : control.Disabled || !GraphicsPreferences.Supported(option.Key, desired, forward)
                ? "Requires Forward+. Your saved preference is retained; this renderer uses the supported fallback." : "Apply to keep this change.";
        }
        _exposure.SetValueNoSignal(_draftGraphics.Exposure);
        string method = RenderingServer.GetCurrentRenderingMethod(), driver = RenderingServer.GetCurrentRenderingDriverName();
        bool restart = method != _draftGraphics.DesiredMethod;
        _rendererStatus.Text = $"Running: {method} / {driver}\n{RenderingServer.GetVideoAdapterName()}\nNext launch: {_draftGraphics.DesiredMethod}{(restart ? " — restart required" : "")}";
        string[] aa = GraphicsPreferences.Options.Single(option => option.Key == "aa").Choices;
        _rendererStatus.Text += $"\nAA — applied: {aa[Graphics.Effective(forward)["aa"]]}; desired draft: {aa[_draftGraphics["aa"]]}; draft effective: {aa[_draftGraphics.Effective(forward)["aa"]]}.";
        if (!forward) _rendererStatus.Text += "\nCompatibility: FXAA/TAA unsupported; desired MSAA 2× uses Off by default cost policy. High MSAA 4× remains supported.";
    }
    private void ApplyGraphics()
    {
        _rendererEdited |= Graphics["renderer"] != _draftGraphics["renderer"];
        Graphics = _draftGraphics.Clone(); GraphicsApplied?.Invoke(Graphics);
        _commitDisplay = _displayEdited; _dirty = true; Save();
        // Apply sets the new cancellation baseline, allowing several deliberate edits.
        _previewMode = _window.Mode; _previewSize = _windowedSize; _previewEdited = _displayEdited;
        RefreshGraphics();
    }
    private void CancelGraphics()
    {
        _draftGraphics = Graphics.Clone();
        _changingDisplay = true; _window.Mode = _previewMode; _windowedSize = Fit(_previewSize);
        if (_previewMode == Window.ModeEnum.Windowed) _window.Size = _windowedSize;
        _changingDisplay = false; _displayEdited = _previewEdited;
        RefreshDisplay(); RefreshGraphics();
    }
    private void LoadGraphics(ConfigFile file)
    {
        Graphics = GraphicsPreferences.Load(key =>
        {
            Variant value = file.GetValue("visuals", key, new Variant());
            return value.VariantType == Variant.Type.Int ? (object)value.AsInt64() : null;
        });
        _draftGraphics = Graphics.Clone();
    }
    private Error SaveGraphics(ConfigFile file)
    {
        foreach (var option in Graphics.Values) file.SetValue("visuals", option.Key, option.Value);
        file.SetValue("visuals", "exposure", Graphics.Exposure);
        if (!_rendererEdited) return Error.Ok;
        // The engine reads this native project override BEFORE initializing its renderer.
        // CLI method/driver flags win. Merely launching with a CI override never writes it.
        using var renderer = new ConfigFile();
        renderer.SetValue("rendering", "renderer/rendering_method", Graphics.DesiredMethod);
        renderer.SetValue("rendering", "rendering_device/driver", Graphics["renderer"] == 0 ? "vulkan" : "opengl3");
        renderer.SetValue("rendering", "rendering_device/driver.windows", Graphics["renderer"] == 0 ? "vulkan" : "opengl3");
        Error error = renderer.Save("user://renderer.cfg");
        if (error == Error.Ok) _rendererEdited = false;
        return error;
    }
    internal void ObserveGraphics(GameApplication application, Dictionary<string, object> targets)
    {
        foreach (Control control in _graphicsControls.Values.Cast<Control>().Concat(new Control[] { _exposure, _resetGraphics }))
        {
            Rect2 rect = control.GetGlobalRect();
            bool visible = control.IsVisibleInTree() && _graphicsScroll.GetGlobalRect().HasPoint(rect.GetCenter());
            application.ObserveControl(targets, control.Name, control, visible: visible);
        }
        application.ObserveControl(targets, "ApplyGraphics", _applyGraphics);
        application.ObserveControl(targets, "CancelGraphics", _cancelGraphics);
        application.ObserveControl(targets, "GraphicsScroll", _graphicsScroll);
    }
}
