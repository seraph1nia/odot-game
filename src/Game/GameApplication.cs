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
    private Label _brandTitle = null!, _brandPositioning = null!;
    private AudioStreamPlayer _music = null!;
    private Tabletop? _tabletop;
    private ConfirmationDialog _leave = null!;
    private long _leaveGeneration = -1;
    private ConfirmationDialog _join = null!;
    private Action? _joinAccepted, _joinDeclined;
    private bool _exiting;
    private readonly LandscapeAssets _landscapeAssets = new();
    private VillageLandscape _landscape = null!;

    public Action? HostRequested { get; set; }
    public Action? InviteRequested { get; set; }
    public Theme Theme { get; private set; } = null!;
    public ClientSettings Settings { get; private set; } = null!;
    internal SteamFriendsDialog Friends { get; private set; } = null!;
    public string Screen { get; private set; } = "menu";
    public bool IsModalOpen => Settings.IsOpen || _join.Visible || _leave.Visible || Friends.IsOpen || _tabletop?.DetailsOpen == true;

    public override void _Ready()
    {
        GetWindow().Title = GameBrand.Title;
        Theme = ApplicationTheme.Create();
        var canvas = new CanvasLayer { Name = "ApplicationUi", Layer = 10 }; AddChild(canvas);
        var root = new Control { Name = "ApplicationControls", Theme = Theme, MouseFilter = Control.MouseFilterEnum.Ignore };
        canvas.AddChild(root); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        Settings = new ClientSettings { Name = "ClientSettings", FocusFallback = ActiveFocus }; AddChild(Settings);
        Settings.Initialize(root, Theme);
        Settings.ReturnRequested = RequestReturn;
        using var music = GD.Load<AudioStreamWav>("res://Assets/Music/LVS04_11_Echoes_of_Valhalla_bpm82_loop.wav");
        _music = new AudioStreamPlayer { Name = "BackgroundMusic", Stream = music, Bus = "Master", VolumeDb = -12, Autoplay = true };
        _music.TreeExiting += _music.Stop;
        AddChild(_music);
        CreateBackground();
        _menuRoot = new CenterContainer { Name = "Menu", MouseFilter = Control.MouseFilterEnum.Ignore };
        root.AddChild(_menuRoot); _menuRoot.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var panel = new PanelContainer { Name = "MenuPanel", CustomMinimumSize = new(460, 0) }; _menuRoot.AddChild(panel);
        var content = new VBoxContainer(); content.AddThemeConstantOverride("separation", 14); panel.AddChild(content);
        _brandTitle = new Label { Name = "BrandTitle", Text = GameBrand.Title, HorizontalAlignment = HorizontalAlignment.Center };
        _brandTitle.AddThemeFontSizeOverride("font_size", 32); content.AddChild(_brandTitle);
        _brandPositioning = new Label { Name = "BrandPositioning", Text = GameBrand.Positioning, HorizontalAlignment = HorizontalAlignment.Center };
        content.AddChild(_brandPositioning);
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
        _steamStatus.AddThemeColorOverride("font_color", ApplicationTheme.Ink);
        root.AddChild(_steamStatus);
        _steamStatus.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        _steamStatus.OffsetLeft = 14; _steamStatus.OffsetRight = -14; _steamStatus.OffsetTop = -46; _steamStatus.OffsetBottom = -14;
        _join = new ConfirmationDialog { Name = "JoinConfirmation", Title = "Join your friend?", DialogText = "Leave the current game and join your friend's invitation? Your current solo or hosted game will end.", OkButtonText = "Join game", CancelButtonText = "Stay here", Transient = true, Exclusive = false, Theme = Theme, DialogCloseOnEscape = true };
        AddChild(_join);
        UiAssets.Decorate(_join.GetOkButton(), "AcceptJoin");
        UiAssets.Decorate(_join.GetCancelButton(), "DeclineJoin");
        _leave = new ConfirmationDialog { Name = "LeaveConfirmation", Title = "Return to menu?", OkButtonText = "Return to menu", CancelButtonText = "Cancel", Transient = true, Exclusive = false, Theme = Theme, DialogCloseOnEscape = true };
        AddChild(_leave);
        _leave.GetCancelButton().Name = "CancelReturn"; _leave.GetOkButton().Name = "ConfirmReturn";
        _leave.Confirmed += ConfirmReturn;
        _leave.Canceled += CancelReturn;
        _leave.CloseRequested += CancelReturn;
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
        UiAssets.Decorate(button, name); parent.AddChild(button); button.Pressed += () => { if (!_exiting && !IsModalOpen) action(); }; return button;
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
        _leave.Hide(); _leaveGeneration = -1;
        Settings.Close(false);
        if (_join.Visible) DeclineJoin();
        RemoveTabletop();
        Screen = "menu"; Settings.InSession = false;
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
        Screen = "multiplayer"; Settings.InSession = false;
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
        => ShowTabletop(session);
    internal Tabletop ShowReplay(IGameSession replay)
    { ShowTabletop(replay); return _tabletop!; }
    private void ShowTabletop(IGameSession presentation)
    {
        Friends.Close();
        _leave.Hide(); _leaveGeneration = -1;
        Settings.Close(false);
        if (_join.Visible) DeclineJoin();
        RemoveTabletop();
        Screen = "session"; Settings.InSession = true;
        _menuRoot.Visible = false; _scenery.Visible = false; _menuCamera.Current = false; _environment.Environment = null;
        Settings.SettingsButton.Visible = true;
        _steamStatus.Visible = false;
        _tabletop = new Tabletop(presentation, this) { Name = "Tabletop" }; AddChild(_tabletop);
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
    private void RequestReturn()
    {
        if (_exiting || Screen != "session" || !Settings.IsOpen || _leave.Visible || !session.HasSession) return;
        _leaveGeneration = session.SessionGeneration;
        _leave.DialogText = session.LeaveWarning;
        _leave.PopupCenteredClamped(new(500, 190), 0.9f);
        _leave.GetCancelButton().GrabFocus();
    }
    private void CancelReturn()
    {
        _leaveGeneration = -1; _leave.Hide();
        if (!_exiting && Screen == "session" && Settings.IsOpen) Settings.ReturnButton.GrabFocus();
    }
    private void ConfirmReturn()
    {
        long generation = _leaveGeneration;
        _leaveGeneration = -1; _leave.Hide();
        if (_exiting || Screen != "session" || generation != session.SessionGeneration || !session.HasSession) return;
        Settings.Close(false);
        session.ReturnToMenu();
    }
    public void OpenJoinConfirmation(Action accept, Action decline)
    {
        if (_exiting || _join.Visible) { decline(); return; }
        Friends.Close();
        _leave.Hide(); _leaveGeneration = -1;
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
        _leave.Hide(); _leaveGeneration = -1;
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
        if (_tabletop?.DetailsOpen == true) { _tabletop.FocusDetails(); GetViewport().SetInputAsHandled(); return; }
        if (@event is InputEventMouseButton { Pressed: true } or InputEventKey { Pressed: true })
            (_leave.Visible ? _leave : _join.Visible ? _join : Friends.IsOpen ? Friends : Settings.Dialog).GrabFocus();
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

    public Dictionary<string, object?> ObserveUi(string id, string? screenshot, int colors)
    {
        var targets = new Dictionary<string, object>();
        foreach (Button button in new[] { _solo, _multiplayer, _menuSettings, _exit, _host, _back }) ObserveControl(targets, button.Name, button);
        ObserveControl(targets, "Settings", Screen == "menu" ? _menuSettings : Settings.SettingsButton);
        ObserveControl(targets, "SteamStatus", _steamStatus);
        ObserveControl(targets, "BrandTitle", _brandTitle);
        ObserveControl(targets, "BrandPositioning", _brandPositioning);
        if (Settings.IsOpen)
        {
            ObserveControl(targets, "GraphicsTab", Settings.Categories.GetTabBar(), Settings.Categories.GetTabBar().GetTabRect(0));
            ObserveControl(targets, "DisplayMode", Settings.DisplaySelector);
            ObserveControl(targets, "Resolution", Settings.ResolutionSelector);
            ObserveControl(targets, "AudioTab", Settings.Categories.GetTabBar(), Settings.Categories.GetTabBar().GetTabRect(1));
            ObserveControl(targets, "AboutTab", Settings.Categories.GetTabBar(), Settings.Categories.GetTabBar().GetTabRect(2));
            ObserveControl(targets, "Volume", Settings.VolumeSlider);
            ObserveControl(targets, "CheckUpdate", Settings.CheckUpdateButton);
            ObserveControl(targets, "DownloadUpdate", Settings.DownloadUpdateButton);
            ObserveControl(targets, "ReturnToMenu", Settings.ReturnButton);
            ObserveControl(targets, "CloseSettings", Settings.Dialog.GetOkButton());
        }
        if (_join.Visible)
        {
            ObserveControl(targets, "AcceptJoin", _join.GetOkButton());
            ObserveControl(targets, "DeclineJoin", _join.GetCancelButton());
        }
        if (_leave.Visible)
        {
            Vector2 close = GetViewport().GetFinalTransform() * ((Vector2)_leave.Position + new Vector2(_leave.Size.X - 8, -14));
            targets["CloseReturn"] = new { X = close.X, Y = close.Y, Width = 14, Height = 14, Visible = true, Enabled = true };
            ObserveControl(targets, "ConfirmReturn", _leave.GetOkButton());
            ObserveControl(targets, "CancelReturn", _leave.GetCancelButton());
        }
        Friends.Observe(this, targets);
        int master = AudioServer.GetBusIndex("Master");
        var fields = new Dictionary<string, object?>
        {
            ["Id"] = id,
            ["AuthoredProvenanceBundled"] = Godot.FileAccess.FileExists(AssetCatalog.Root + "manifest.json") && Godot.FileAccess.FileExists(AssetCatalog.Root + "NOTICE.md"),
            ["InstalledModels"] = UnitAssets.InstalledModels(),
            ["UiProvenanceBundled"] = Godot.FileAccess.FileExists(UiAssets.Root + "manifest.json") && Godot.FileAccess.FileExists(UiAssets.Root + "UPSTREAM-README.txt") && Godot.FileAccess.FileExists(UiAssets.Root + "README.md"),
            ["PanelTexture"] = (Theme.GetStylebox("panel", "PanelContainer") as StyleBoxTexture)?.Texture?.ResourcePath,
            ["ButtonTextures"] = ApplicationTheme.ButtonStates.Select(state => (Theme.GetStylebox(state, "Button") as StyleBoxTexture)?.Texture?.ResourcePath).ToArray(),
            ["FocusBorder"] = (Theme.GetStylebox("focus", "Button") as StyleBoxFlat)?.BorderWidthTop,
            ["SliderTexture"] = Theme.GetIcon("grabber", "HSlider").ResourcePath,
            ["Screen"] = Screen,
            ["WindowTitle"] = GetWindow().Title,
            ["BrandTitle"] = _brandTitle.Text,
            ["BrandPositioning"] = _brandPositioning.Text,
            ["BrandTextFits"] = new[] { _brandTitle, _brandPositioning }.All(label => label.GetMinimumSize().X <= label.Size.X && label.GetMinimumSize().Y <= label.Size.Y),
            ["Revision"] = -1L,
            ["SelectedSlot"] = -1,
            ["Connected"] = session.Connected,
            ["SettingsOpen"] = Settings.IsOpen,
            ["ResolutionFocused"] = Settings.ResolutionSelector.GetPopup().GetFocusedItem(),
            ["ResolutionSelected"] = Settings.ResolutionSelector.Selected,
            ["DropdownOpen"] = Settings.DisplaySelector.GetPopup().Visible || Settings.ResolutionSelector.GetPopup().Visible,
            ["DropdownTexture"] = (Settings.DisplaySelector.GetPopup().GetThemeStylebox("panel") as StyleBoxTexture)?.Texture?.ResourcePath,
            ["TabTextures"] = new[] { Settings.Categories.GetTabBar().GetThemeStylebox("tab_selected"), Settings.Categories.GetTabBar().GetThemeStylebox("tab_unselected") }.OfType<StyleBoxTexture>().Select(box => box.Texture.ResourcePath).ToArray(),
            ["DialogTexture"] = ((_leave.Visible ? _leave : Settings.IsOpen ? Settings.Dialog : _join.Visible ? _join : Friends).GetThemeStylebox("panel", "AcceptDialog") as StyleBoxTexture)?.Texture?.ResourcePath,
            ["ReturnConfirmationOpen"] = _leave.Visible,
            ["ReturnWarning"] = _leave.DialogText,
            ["ConfirmationFocus"] = _leave.GuiGetFocusOwner()?.Name.ToString() ?? "",
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
            ["RenderCosts"] = new
            {
                DrawCalls = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalDrawCallsInFrame),
                Objects = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalObjectsInFrame),
                Primitives = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalPrimitivesInFrame),
                TextureBytes = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TextureMemUsed),
                BufferBytes = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.BufferMemUsed),
                Nodes = Performance.GetMonitor(Performance.Monitor.ObjectNodeCount),
                Resources = Performance.GetMonitor(Performance.Monitor.ObjectResourceCount),
                FramesPerSecond = Performance.GetMonitor(Performance.Monitor.TimeFps),
                ProcessMilliseconds = Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000,
                PhysicsMilliseconds = Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess) * 1000
            },
            ["Models"] = _landscapeAssets.Paths.Count(),
            ["Landscape"] = _landscape.Observe(_menuCamera, GetViewport().GetVisibleRect()),
            ["Materials"] = 0,
            ["MusicLoaded"] = _music.Stream is not null,
            ["MusicPlaying"] = _music.Playing,
            ["MusicInstance"] = _music.GetInstanceId(),
            ["MusicPosition"] = _music.GetPlaybackPosition(),
            ["Width"] = GetWindow().Size.X,
            ["Height"] = GetWindow().Size.Y,
            ["WindowFocused"] = GetWindow().HasFocus(),
            ["NativeWindow"] = DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, GetWindow().GetWindowId()),
            ["Targets"] = targets,
            ["Screenshot"] = screenshot,
            ["Colors"] = colors,
            ["FocusedControl"] = Friends.IsOpen ? Friends.FocusedControl : GetViewport().GuiGetFocusOwner()?.Name.ToString() ?? "",
            ["FeedbackText"] = _feedback.Text,
            ["SteamStatus"] = _steamStatus.Text
        };
        _tabletop?.AppendUiObservation(fields, targets);
        if (System.Environment.GetEnvironmentVariable("ODOT_ASSET_COSTS") == "1") { fields["RenderGroups"] = RenderInventory.Observe(this); }
        return fields;
    }
    internal void ObserveControl(Dictionary<string, object> targets, string name, Control control, Rect2? local = null)
    {
        Rect2 rect = local is { } area ? new Rect2(control.GetGlobalTransformWithCanvas() * area.Position, area.Size) : control.GetGlobalRect();
        Vector2 point = rect.GetCenter();
        if (control.GetViewport() is Window window && window != GetWindow()) point += window.Position;
        point = GetViewport().GetFinalTransform() * point;
        Vector2 scale = GetViewport().GetFinalTransform().Scale;
        targets[name] = new
        {
            X = point.X,
            Y = point.Y,
            Width = rect.Size.X * scale.X,
            Height = rect.Size.Y * scale.Y,
            Visible = control.IsVisibleInTree(),
            Enabled = control is not BaseButton button || !button.Disabled,
            Icon = (control as Button)?.Icon?.ResourcePath ?? "",
            Text = (control as Button)?.Text ?? "",
            Tooltip = control.TooltipText,
            CostText = control.GetNodeOrNull<RichTextLabel>("Cost")?.GetParsedText() ?? ""
        };
    }

    private void CreateBackground()
    {
        _scenery = new Node3D { Name = "MenuScenery" }; AddChild(_scenery);
        _menuEnvironment = VillageLighting.Environment();
        _environment = new WorldEnvironment { Environment = _menuEnvironment }; _scenery.AddChild(_environment);
        _scenery.AddChild(VillageLighting.Sun());
        _menuCamera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = 28, Position = new(18, 22, 30), Current = true }; _scenery.AddChild(_menuCamera); _menuCamera.LookAt(Vector3.Zero);
        _landscape = new VillageLandscape(_scenery, _landscapeAssets);
        _landscape.Cover(_menuCamera, GetViewport().GetVisibleRect(), Vector2.Zero);
    }
    public override void _Process(double delta)
    {
        if (_leave.Visible && (Screen != "session" || _leaveGeneration != session.SessionGeneration)) CancelReturn();
        if (_scenery.Visible) _landscape.Cover(_menuCamera, GetViewport().GetVisibleRect(), Vector2.Zero);
    }
}
