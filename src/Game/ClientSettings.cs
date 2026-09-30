using Godot;

// Instantiated only by the graphical presentation, independently of match/session state.
public partial class ClientSettings : Node
{
    private const string SettingsPath = "user://settings.cfg";
    private static readonly Vector2I DefaultSize = new(1100, 820);
    private static readonly Vector2I[] Presets = [DefaultSize, new(1280, 720), new(1600, 900), new(1920, 1080)];
    private Window _window = null!;
    private AcceptDialog _dialog = null!;
    private Button _button = null!;
    private OptionButton _mode = null!, _resolution = null!;
    private HSlider _volume = null!;
    private Label _value = null!, _saveFeedback = null!;
    private readonly List<Vector2I> _sizes = [];
    private int _masterVolume = 50;
    private Window.ModeEnum _savedMode = Window.ModeEnum.Windowed;
    private Vector2I _savedSize = DefaultSize, _windowedSize = DefaultSize;
    private bool _launchDisplayOverride, _displayEdited, _changingDisplay, _initialized, _dirty;

    public bool IsOpen => _dialog.Visible;
    public bool BlocksWorldHover(Vector2 mouse) => IsOpen || _button.GetGlobalRect().HasPoint(mouse);

    public void Initialize(Control ui, Theme theme)
    {
        _window = GetWindow();
        Load();
        ApplyMasterVolume();
        string[] args = EngineArguments();
        bool sizeOverride = args.Any(a => a is "--resolution" or "-r");
        bool modeOverride = args.Any(a => a is "--fullscreen" or "-f" or "--windowed" or "-w" or "--maximized" or "-m");
        _launchDisplayOverride = sizeOverride || modeOverride;
        _windowedSize = sizeOverride ? _window.Size : Fit(_savedSize);
        if (!modeOverride) _window.Mode = _savedMode;
        if (!sizeOverride && _window.Mode == Window.ModeEnum.Windowed) _window.Size = _windowedSize;

        _button = new Button { Name = "SettingsButton", Text = "Settings", Position = new(12, 12), TooltipText = "Settings (Esc)" };
        ui.AddChild(_button);
        _button.Pressed += Open;
        _dialog = new AcceptDialog { Name = "SettingsDialog", Title = "Settings", Theme = theme, Transient = true, Exclusive = true, OkButtonText = "Close", DialogCloseOnEscape = true };
        // Let Godot route modal input and dropdown focus within the parent viewport.
        _window.GuiEmbedSubwindows = true;
        AddChild(_dialog);
        var content = new VBoxContainer(); content.AddThemeConstantOverride("separation", 12); _dialog.AddChild(content);
        var tabs = new TabContainer { Name = "Categories", SizeFlagsVertical = Control.SizeFlags.ExpandFill, CustomMinimumSize = new(0, 180) }; content.AddChild(tabs);
        var graphics = Page(tabs, "Graphics");
        Label(graphics, "Display mode");
        _mode = new OptionButton { Name = "DisplayMode", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; graphics.AddChild(_mode);
        _mode.AddItem("Windowed"); _mode.AddItem("Fullscreen");
        _mode.ItemSelected += SelectMode;
        Label(graphics, "Windowed resolution");
        _resolution = new OptionButton { Name = "Resolution", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; graphics.AddChild(_resolution);
        _resolution.ItemSelected += SelectResolution;
        Label(graphics, "Fullscreen uses your monitor's native size.");
        var audio = Page(tabs, "Audio");
        _value = Label(audio, $"Master volume: {_masterVolume}");
        _volume = new HSlider { Name = "MasterVolume", MinValue = 0, MaxValue = 100, Step = 1, Value = _masterVolume, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, CustomMinimumSize = new(0, 32) }; audio.AddChild(_volume);
        _volume.ValueChanged += value =>
        {
            _masterVolume = (int)value; _dirty = true;
            _value.Text = $"Master volume: {_masterVolume}";
            ApplyMasterVolume();
        };
        _volume.DragEnded += changed => { if (changed) Save(); };
        _volume.GuiInput += input =>
        {
            if (input is InputEventKey { Pressed: false } || input is InputEventMouseButton { Pressed: false }) Save();
        };
        Label(audio, "0 mutes all audio. Music keeps playing while muted.");
        _saveFeedback = Label(content, "");
        _dialog.VisibilityChanged += () =>
        {
            if (!_dialog.Visible) { Save(); _button.GrabFocus(); }
        };
        _window.SizeChanged += WindowResized;
        RefreshDisplay();
        // Initial native window notifications must not turn launch overrides into preferences.
        Callable.From(() => _initialized = true).CallDeferred();
    }

    private static string[] EngineArguments()
    {
        // Godot consumes display flags, and its embedded .NET host has no managed argv.
        // The supported Linux desktop export can read its original process arguments.
        string[] args = OS.GetCmdlineArgs();
        if (OS.GetName() == "Linux")
        {
            try { args = System.IO.File.ReadAllText("/proc/self/cmdline").Split('\0'); }
            catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException) { }
        }
        return args.TakeWhile(a => a != "--").ToArray();
    }

    private static VBoxContainer Page(TabContainer tabs, string name)
    {
        var margin = new MarginContainer { Name = name }; tabs.AddChild(margin);
        foreach (string side in new[] { "left", "top", "right", "bottom" }) margin.AddThemeConstantOverride("margin_" + side, 12);
        var box = new VBoxContainer(); box.AddThemeConstantOverride("separation", 8); margin.AddChild(box); return box;
    }
    private static Label Label(Node parent, string text)
    {
        var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(280, 0), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; parent.AddChild(label); return label;
    }
    private void Load()
    {
        using var file = new ConfigFile();
        if (file.Load(SettingsPath) != Error.Ok) return;
        _masterVolume = ReadInteger(file, "audio", "master_volume", 50, 0, 100);
        Variant mode = file.GetValue("graphics", "mode", "windowed");
        if (mode.VariantType == Variant.Type.String && mode.AsString() == "fullscreen") _savedMode = Window.ModeEnum.Fullscreen;
        _savedSize = new(ReadInteger(file, "graphics", "window_width", DefaultSize.X, 1, int.MaxValue), ReadInteger(file, "graphics", "window_height", DefaultSize.Y, 1, int.MaxValue));
    }
    private static int ReadInteger(ConfigFile file, string section, string key, int fallback, int min, int max)
    {
        Variant value = file.GetValue(section, key, fallback);
        if (value.VariantType != Variant.Type.Int) return fallback;
        long number = value.AsInt64();
        return number >= min && number <= max ? (int)number : fallback;
    }
    private void ApplyMasterVolume()
    {
        int master = AudioServer.GetBusIndex("Master");
        AudioServer.SetBusVolumeLinear(master, _masterVolume / 100f);
        AudioServer.SetBusMute(master, _masterVolume == 0);
    }
    private Vector2I AvailableSize()
    {
        Vector2I usable = DisplayServer.ScreenGetUsableRect(_window.CurrentScreen).Size;
        // Leave space for ordinary desktop window decorations.
        return new(Math.Max(1, usable.X - 32), Math.Max(1, usable.Y - 64));
    }
    private Vector2I Fit(Vector2I size)
    {
        Vector2I available = AvailableSize();
        if (size.X <= available.X && size.Y <= available.Y) return size;
        Vector2I fallback = Presets.FirstOrDefault(p => p.X <= available.X && p.Y <= available.Y);
        return fallback != Vector2I.Zero ? fallback : new(Math.Min(DefaultSize.X, available.X), Math.Min(DefaultSize.Y, available.Y));
    }
    private void RefreshDisplay()
    {
        bool fullscreen = _window.Mode is Window.ModeEnum.Fullscreen or Window.ModeEnum.ExclusiveFullscreen;
        _mode.Select(fullscreen ? 1 : 0);
        _resolution.Disabled = fullscreen;
        _resolution.Clear(); _sizes.Clear();
        Vector2I available = AvailableSize();
        foreach (Vector2I preset in Presets)
            if (preset.X <= available.X && preset.Y <= available.Y) _sizes.Add(preset);
        if (!_sizes.Contains(_windowedSize)) _sizes.Add(_windowedSize);
        foreach (Vector2I size in _sizes) _resolution.AddItem($"{size.X} × {size.Y}{(Presets.Contains(size) ? "" : " (custom)")}");
        _resolution.Select(_sizes.IndexOf(_windowedSize));
    }
    public void Open()
    {
        if (IsOpen) return;
        RefreshDisplay();
        _dialog.PopupCenteredClamped(new(500, 330), 0.9f);
    }
    private void SelectMode(long index)
    {
        _displayEdited = true; _dirty = true; _changingDisplay = true;
        if (index == 1)
        {
            if (_window.Mode == Window.ModeEnum.Windowed) _windowedSize = _window.Size;
            _window.Mode = Window.ModeEnum.Fullscreen;
        }
        else
        {
            _windowedSize = Fit(_windowedSize);
            _window.Mode = Window.ModeEnum.Windowed;
            _window.Size = _windowedSize;
        }
        _changingDisplay = false;
        RefreshDisplay(); Save();
    }
    private void SelectResolution(long index)
    {
        if (_window.Mode != Window.ModeEnum.Windowed) return;
        _displayEdited = true; _dirty = true; _changingDisplay = true;
        _windowedSize = _sizes[(int)index];
        _window.Size = _windowedSize;
        _changingDisplay = false;
        RefreshDisplay(); Save();
    }
    private void WindowResized()
    {
        if (_initialized && !_changingDisplay && _window.Mode == Window.ModeEnum.Windowed && _window.Size != _windowedSize)
        {
            _windowedSize = _window.Size; _displayEdited = true; _dirty = true;
        }
        RefreshDisplay();
        if (IsOpen) _dialog.PopupCenteredClamped(new(500, 330), 0.9f);
    }
    private void Save()
    {
        if (!_dirty) return;
        bool saveDisplay = _displayEdited || !_launchDisplayOverride;
        using var file = new ConfigFile();
        file.SetValue("audio", "master_volume", _masterVolume);
        file.SetValue("graphics", "mode", (saveDisplay ? _window.Mode : _savedMode) is Window.ModeEnum.Fullscreen or Window.ModeEnum.ExclusiveFullscreen ? "fullscreen" : "windowed");
        Vector2I size = saveDisplay ? _windowedSize : _savedSize;
        file.SetValue("graphics", "window_width", size.X); file.SetValue("graphics", "window_height", size.Y);
        Error result = file.Save(SettingsPath);
        _saveFeedback.Text = result == Error.Ok ? "" : "Couldn't save settings. Changes work for this session.";
        if (result == Error.Ok) _dirty = false;
    }
    public override void _ExitTree()
    {
        _window.SizeChanged -= WindowResized;
        Save();
    }
}
