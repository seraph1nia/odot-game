using Godot;

namespace Game;

// One graphical lifetime: screens change, preferences and the authored music do not.
public partial class GameApplication(Main session) : Node
{
    private Node3D _scenery = null!;
    private Camera3D _menuCamera = null!;
    private WorldEnvironment _environment = null!;
    private Godot.Environment _menuEnvironment = null!;
    private Control _menuRoot = null!;
    private VBoxContainer _startMenu = null!, _multiplayerMenu = null!;
    private Button _solo = null!, _multiplayer = null!, _menuSettings = null!, _exit = null!, _host = null!, _back = null!;
    private Label _feedback = null!;
    private Label _steamStatus = null!;
    private AudioStreamPlayer _music = null!;
    private Tabletop? _tabletop;
    private ConfirmationDialog _join = null!;
    private Action? _joinAccepted, _joinDeclined;
    private bool _exiting;
    private int _sceneryModels;

    public Action? HostRequested { get; set; }
    public Action? InviteRequested { get; set; }
    public Theme Theme { get; private set; } = null!;
    public ClientSettings Settings { get; private set; } = null!;
    internal SteamFriendsDialog Friends { get; private set; } = null!;
    public string Screen { get; private set; } = "menu";
    public bool IsModalOpen => Settings.IsOpen || _join.Visible || Friends.IsOpen;

    public override void _Ready()
    {
        Theme = ApplicationTheme.Create();
        var canvas = new CanvasLayer { Name = "ApplicationUi", Layer = 10 }; AddChild(canvas);
        var root = new Control { Name = "ApplicationControls", Theme = Theme, MouseFilter = Control.MouseFilterEnum.Ignore };
        canvas.AddChild(root); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        Settings = new ClientSettings { Name = "ClientSettings", FocusFallback = ActiveFocus }; AddChild(Settings);
        Settings.Initialize(root, Theme);
        using var music = GD.Load<AudioStreamWav>("res://Assets/Music/LVS04_11_Echoes_of_Valhalla_bpm82_loop.wav");
        _music = new AudioStreamPlayer { Name = "BackgroundMusic", Stream = music, Bus = "Master", VolumeDb = -12, Autoplay = true };
        _music.TreeExiting += _music.Stop;
        AddChild(_music);
        CreateBackground();
        _menuRoot = new CenterContainer { Name = "Menu", MouseFilter = Control.MouseFilterEnum.Ignore };
        root.AddChild(_menuRoot); _menuRoot.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var panel = new PanelContainer { Name = "MenuPanel", CustomMinimumSize = new(370, 0) }; _menuRoot.AddChild(panel);
        var content = new VBoxContainer(); content.AddThemeConstantOverride("separation", 14); panel.AddChild(content);
        var title = new Label { Text = "ODOT", HorizontalAlignment = HorizontalAlignment.Center }; title.AddThemeFontSizeOverride("font_size", 42); content.AddChild(title);
        var subtitle = new Label { Text = "Nine plots. One countryside.", HorizontalAlignment = HorizontalAlignment.Center }; content.AddChild(subtitle);
        _startMenu = new VBoxContainer { Name = "StartMenu" }; _startMenu.AddThemeConstantOverride("separation", 12); content.AddChild(_startMenu);
        _solo = Button(_startMenu, "Singleplayer", "Single player", session.StartSolo);
        _multiplayer = Button(_startMenu, "Multiplayer", "Multiplayer", () => ShowMultiplayer());
        _menuSettings = Button(_startMenu, "MenuSettings", "Settings", () => Settings.Open(_menuSettings));
        _exit = Button(_startMenu, "Exit", "Exit Game", RequestExit);
        SetFocusOrder(_solo, _multiplayer, _menuSettings, _exit);
        _multiplayerMenu = new VBoxContainer { Name = "MultiplayerMenu", Visible = false }; _multiplayerMenu.AddThemeConstantOverride("separation", 12); content.AddChild(_multiplayerMenu);
        var explanation = new Label { Text = "Host a private game, then invite friends through Steam.\nJoin friends through their Steam invitations.", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(320, 0) }; _multiplayerMenu.AddChild(explanation);
        _host = Button(_multiplayerMenu, "Host", "Host game", RequestHost);
        _back = Button(_multiplayerMenu, "Back", "Back", () => ShowMenu());
        SetFocusOrder(_host, _back);
        _feedback = new Label { Name = "PlatformFeedback", Text = "", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(320, 0) }; content.AddChild(_feedback);
        _steamStatus = new Label
        {
            Name = "SteamStatus",
            Text = "Open Steam to log in",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ClipText = true,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
        };
        _steamStatus.AddThemeColorOverride("font_color", new("233d39"));
        root.AddChild(_steamStatus);
        _steamStatus.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        _steamStatus.OffsetLeft = 14; _steamStatus.OffsetRight = -14; _steamStatus.OffsetTop = -46; _steamStatus.OffsetBottom = -14;
        _join = new ConfirmationDialog { Name = "JoinConfirmation", Title = "Join your friend?", DialogText = "Leave the current game and join your friend's invitation? Your current solo or hosted game will end.", OkButtonText = "Join game", CancelButtonText = "Stay here", Transient = true, Exclusive = false, Theme = Theme, DialogCloseOnEscape = true };
        AddChild(_join);
        _join.Confirmed += AcceptJoin;
        _join.Canceled += DeclineJoin;
        Friends = new SteamFriendsDialog { Name = "SteamFriends", Theme = Theme, FocusFallback = FocusCurrent };
        AddChild(Friends);
        GetTree().AutoAcceptQuit = false;
        GetWindow().CloseRequested += RequestExit;
        ShowMenu();
    }

    private Button Button(Node parent, string name, string text, Action action)
    {
        var button = new Button { Name = name, Text = text, CustomMinimumSize = new(0, 42), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        parent.AddChild(button); button.Pressed += () => { if (!_exiting && !IsModalOpen) action(); }; return button;
    }
    private static void SetFocusOrder(params Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Button previous = buttons[(i + buttons.Length - 1) % buttons.Length], next = buttons[(i + 1) % buttons.Length];
            buttons[i].FocusNeighborTop = buttons[i].GetPathTo(previous);
            buttons[i].FocusPrevious = buttons[i].GetPathTo(previous);
            buttons[i].FocusNeighborBottom = buttons[i].GetPathTo(next);
            buttons[i].FocusNext = buttons[i].GetPathTo(next);
        }
    }
    private Button? ActiveFocus() => Screen == "session" ? Settings.SettingsButton : Screen == "multiplayer" ? _back : _solo;
    private void FocusCurrent() => Callable.From(() =>
    {
        Control? control = ActiveFocus();
        if (!_exiting && !IsModalOpen && control is not null && control.IsVisibleInTree()) control.GrabFocus();
    }).CallDeferred();

    public void ShowMenu(string feedback = "")
    {
        Friends.Close();
        Settings.Close(false);
        if (_join.Visible) DeclineJoin();
        RemoveTabletop();
        Screen = "menu";
        _scenery.Visible = true; _environment.Environment = _menuEnvironment; _menuCamera.Current = true;
        _menuRoot.Visible = true; _startMenu.Visible = true; _multiplayerMenu.Visible = false;
        Settings.SettingsButton.Visible = false;
        _steamStatus.Visible = true;
        _feedback.Text = feedback;
        _host.Disabled = false;
        FocusCurrent();
    }
    public void ShowMultiplayer(string feedback = "")
    {
        if (_tabletop is not null) return;
        Screen = "multiplayer";
        _startMenu.Visible = false; _multiplayerMenu.Visible = true;
        Settings.SettingsButton.Visible = true;
        _steamStatus.Visible = true;
        SetPlatformFeedback(feedback);
        FocusCurrent();
    }
    public void SetPlatformFeedback(string feedback, bool busy = false)
    {
        _feedback.Text = feedback;
        _host.Disabled = busy;
    }
    public void SetSteamIdentity(string? name) => _steamStatus.Text = string.IsNullOrWhiteSpace(name) ? "Open Steam to log in" : name.Trim();
    public void ShowSession()
    {
        Friends.Close();
        Settings.Close(false);
        if (_join.Visible) DeclineJoin();
        RemoveTabletop();
        Screen = "session";
        _menuRoot.Visible = false; _scenery.Visible = false; _menuCamera.Current = false; _environment.Environment = null;
        Settings.SettingsButton.Visible = true;
        _steamStatus.Visible = false;
        _tabletop = new Tabletop(session, this) { Name = "Tabletop" }; AddChild(_tabletop);
        FocusCurrent();
    }
    private void RemoveTabletop()
    {
        if (_tabletop is null) return;
        RemoveChild(_tabletop); _tabletop.QueueFree(); _tabletop = null;
    }
    private void RequestHost()
    {
        if (IsModalOpen || _exiting || _host.Disabled) return;
        if (HostRequested is null) { SetPlatformFeedback("Start Steam and sign in before hosting or joining."); return; }
        HostRequested();
    }
    public void RequestInvite()
    {
        if (IsModalOpen || _exiting || !session.CanInvite) return;
        InviteRequested?.Invoke();
    }
    public void ReturnToMenu()
    {
        if (IsModalOpen || _exiting) return;
        session.ReturnToMenu();
    }
    public void OpenJoinConfirmation(Action accept, Action decline)
    {
        if (_exiting || _join.Visible) { decline(); return; }
        Friends.Close();
        Settings.Close(false);
        _joinAccepted = accept; _joinDeclined = decline;
        _join.PopupCenteredClamped(new(500, 190), 0.9f);
        _join.GetCancelButton().GrabFocus();
    }
    internal void CancelJoinConfirmation()
    {
        if (_join.Visible) DeclineJoin();
    }
    private void AcceptJoin()
    {
        Action? accept = _joinAccepted;
        _joinAccepted = _joinDeclined = null;
        _join.Hide(); accept?.Invoke();
        FocusCurrent();
    }
    private void DeclineJoin()
    {
        Action? decline = _joinDeclined;
        _joinAccepted = _joinDeclined = null;
        _join.Hide(); decline?.Invoke();
        FocusCurrent();
    }
    public void RequestExit()
    {
        if (_exiting) return;
        _exiting = true;
        Friends.Close();
        Settings.Close(false);
        DeclineJoin();
        session.RequestExit();
    }
    public void StopAudio()
    {
        if (!GodotObject.IsInstanceValid(_music)) return;
        _music.Stop(); _music.Stream = null;
    }
    public bool BlocksWorldInput(Vector2 mouse) => IsModalOpen || Settings.BlocksWorldHover(mouse);
    public override void _Input(InputEvent @event)
    {
        // Embedded subwindows receive their own input before this root viewport.
        // Consume only events routed outside the dialog, keeping native focus inside.
        // Nonexclusive dialogs let the main window deliver native close requests.
        if (!IsModalOpen) return;
        if (@event is InputEventMouseButton { Pressed: true } or InputEventKey { Pressed: true })
            (_join.Visible ? _join : Friends.IsOpen ? Friends : Settings.Dialog).GrabFocus();
        GetViewport().SetInputAsHandled();
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (_exiting || IsModalOpen || !@event.IsActionPressed("ui_cancel", false)) return;
        if (Screen == "multiplayer") ShowMenu(); else Settings.Open(ActiveFocus());
        GetViewport().SetInputAsHandled();
    }
    public override void _ExitTree()
    {
        GetWindow().CloseRequested -= RequestExit;
        StopAudio();
    }

    public object ObserveUi(string id, string? screenshot, int colors)
    {
        var targets = new Dictionary<string, object>();
        foreach (Button button in new[] { _solo, _multiplayer, _menuSettings, _exit, _host, _back }) ObserveControl(targets, button.Name, button);
        ObserveControl(targets, "Settings", Screen == "menu" ? _menuSettings : Settings.SettingsButton);
        ObserveControl(targets, "SteamStatus", _steamStatus);
        if (Settings.IsOpen)
        {
            ObserveControl(targets, "AudioTab", Settings.Categories.GetTabBar(), Settings.Categories.GetTabBar().GetTabRect(1));
            ObserveControl(targets, "AboutTab", Settings.Categories.GetTabBar(), Settings.Categories.GetTabBar().GetTabRect(2));
            ObserveControl(targets, "Volume", Settings.VolumeSlider);
            ObserveControl(targets, "CheckUpdate", Settings.CheckUpdateButton);
            ObserveControl(targets, "DownloadUpdate", Settings.DownloadUpdateButton);
            ObserveControl(targets, "CloseSettings", Settings.Dialog.GetOkButton());
        }
        if (_join.Visible)
        {
            ObserveControl(targets, "AcceptJoin", _join.GetOkButton());
            ObserveControl(targets, "DeclineJoin", _join.GetCancelButton());
        }
        Friends.Observe(this, targets);
        int master = AudioServer.GetBusIndex("Master");
        var fields = new Dictionary<string, object?>
        {
            ["Id"] = id,
            ["Screen"] = Screen,
            ["Revision"] = -1L,
            ["SelectedSlot"] = -1,
            ["Connected"] = session.Connected,
            ["SettingsOpen"] = Settings.IsOpen,
            ["JoinConfirmationOpen"] = _join.Visible,
            ["FriendsOpen"] = Friends.IsOpen,
            ["FriendCount"] = Friends.FriendCount,
            ["InviteStatus"] = Friends.Status,
            ["MasterVolume"] = Settings.MasterVolume,
            ["BuildVersion"] = BuildInfo.DisplayVersion,
            ["UpdateStatus"] = Settings.UpdateStatus,
            ["MasterGain"] = AudioServer.GetBusVolumeLinear(master),
            ["MasterMuted"] = AudioServer.IsBusMute(master),
            ["Display"] = DisplayServer.GetName(),
            ["AudioDriver"] = AudioServer.GetDriverName(),
            ["UserDataPath"] = ProjectSettings.GlobalizePath("user://"),
            ["Renderer"] = RenderingServer.GetVideoAdapterName(),
            ["Models"] = _sceneryModels,
            ["Materials"] = 0,
            ["MusicLoaded"] = _music.Stream is not null,
            ["MusicPlaying"] = _music.Playing,
            ["MusicInstance"] = _music.GetInstanceId(),
            ["MusicPosition"] = _music.GetPlaybackPosition(),
            ["Width"] = GetWindow().Size.X,
            ["Height"] = GetWindow().Size.Y,
            ["NativeWindow"] = DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, GetWindow().GetWindowId()),
            ["Targets"] = targets,
            ["Screenshot"] = screenshot,
            ["Colors"] = colors,
            ["FocusedControl"] = Friends.IsOpen ? Friends.FocusedControl : GetViewport().GuiGetFocusOwner()?.Name.ToString() ?? "",
            ["FeedbackText"] = _feedback.Text,
            ["SteamStatus"] = _steamStatus.Text
        };
        _tabletop?.AppendUiObservation(fields, targets);
        return fields;
    }
    internal void ObserveControl(Dictionary<string, object> targets, string name, Control control, Rect2? local = null)
    {
        Rect2 rect = local is { } area ? new Rect2(control.GetGlobalTransformWithCanvas() * area.Position, area.Size) : control.GetGlobalRect();
        Vector2 point = rect.GetCenter();
        if (control.GetViewport() is Window window && window != GetWindow()) point += window.Position;
        point = GetViewport().GetFinalTransform() * point;
        targets[name] = new { X = point.X, Y = point.Y, Visible = control.IsVisibleInTree(), Enabled = control is not BaseButton button || !button.Disabled };
    }

    private void CreateBackground()
    {
        _scenery = new Node3D { Name = "MenuScenery" }; AddChild(_scenery);
        _menuEnvironment = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new("a7c4c2"), AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = new("f2f0df"), AmbientLightEnergy = 0.2f, TonemapMode = Godot.Environment.ToneMapper.Linear };
        _environment = new WorldEnvironment { Environment = _menuEnvironment }; _scenery.AddChild(_environment);
        _scenery.AddChild(new DirectionalLight3D { RotationDegrees = new(-55, -25, 0), LightEnergy = 0.35f, ShadowEnabled = true });
        _menuCamera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = 28, Position = new(18, 22, 30), Current = true }; _scenery.AddChild(_menuCamera); _menuCamera.LookAt(Vector3.Zero);
        for (int row = -3; row <= 3; row++) for (int column = -2; column <= 2; column++)
            SceneryModel(column == 2 ? "hex_river_B" : "hex_grass", VillageLayout.Hex(column, row), VillageLayout.TerrainScale);
        SceneryModel("building_home_A_blue", VillageLayout.Hex(-2, 2), 0.8f);
        SceneryModel("building_tower_A_blue", VillageLayout.Hex(-2, 3), 0.7f);
        SceneryModel("building_windmill_blue", VillageLayout.Hex(1, -3), 0.8f);
        SceneryModel("trees_A_small", VillageLayout.Hex(-2, -2), VillageLayout.TerrainScale);
        SceneryModel("trees_A_small", VillageLayout.Hex(1, -2), VillageLayout.TerrainScale);
        SceneryModel("rock_single_C", VillageLayout.Hex(0, 3), VillageLayout.TerrainScale);
    }
    private void SceneryModel(string model, Vector3 position, float scale)
    {
        using PackedScene asset = GD.Load<PackedScene>($"res://Assets/KayKit/Medieval/{model}.gltf");
        var node = asset.Instantiate<Node3D>(); node.Position = position; node.Scale = Vector3.One * scale; _scenery.AddChild(node); _sceneryModels++;
    }
}
