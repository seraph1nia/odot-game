using Game.Core;
using Godot;

namespace Game;

// Entirely client-side presentation. Every interaction submits an ordinary command.
public partial class Tabletop(Main game) : Node3D
{
    private const string Assets = "res://Assets/KayKit/";
    private readonly Dictionary<string, PackedScene> _assets = [];
    private readonly Dictionary<int, Node3D> _units = [];
    private readonly Dictionary<int, Node3D> _boards = [];
    private readonly Dictionary<int, string> _boardKeys = [];
    private readonly Dictionary<int, Aabb?[]> _buildingBounds = [];
    private readonly Dictionary<string, Aabb> _modelBounds = [];
    private readonly Dictionary<string, StandardMaterial3D> _materials = [];
    private readonly Dictionary<float, ArrayMesh> _outlines = [];
    private readonly Dictionary<int, Label3D> _cityLabels = [];
    private Camera3D _camera = null!;
    private Label _phase = null!;
    private Label _status = null!;
    private Label _stats = null!;
    private Label _detail = null!;
    private Label _feedback = null!;
    private HBoxContainer _roster = null!;
    private PanelContainer _panel = null!;
    private HBoxContainer _buildActions = null!, _buildingActions = null!;
    private MeshInstance3D _selection = null!, _hoverMarker = null!;
    private int _hover = -1;
    private string _uiKey = "";
    private Vector2 _frameSize;
    private float _framePanelHeight;
    private int _frameFocus;
    private bool _wasConnected;
    private Button _mine = null!, _farm = null!, _barracks = null!, _upgrade = null!, _recruit = null!, _ready = null!, _pause = null!, _start = null!, _reconnect = null!, _fresh = null!;
    private int _focus;
    private int _slot = -1;
    private string _rosterKey = "";
    private long _revision = -1;
    private string _matchId = "";
    private ClientSettings _settings = null!;

    public override void _Ready()
    {
        AddChild(new WorldEnvironment { Environment = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new("a7c4c2"), AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = new("f2f0df"), AmbientLightEnergy = 0.2f, TonemapMode = Godot.Environment.ToneMapper.Linear } });
        AddChild(new DirectionalLight3D { RotationDegrees = new(-55, -25, 0), LightEnergy = 0.35f, ShadowEnabled = true, DirectionalShadowMaxDistance = 65 });
        _camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Current = true, Far = 180 };
        AddChild(_camera);
        _selection = Outline(this, 1.02f, "f3d995"); _selection.Visible = false;
        _hoverMarker = Outline(this, 0.97f, "e7eee0"); _hoverMarker.Visible = false;
        var canvas = new CanvasLayer(); AddChild(canvas);
        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore }; canvas.AddChild(root); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var theme = new Theme { DefaultFontSize = 15 };
        theme.SetColor("font_color", "Label", new("f4ecd9"));
        theme.SetColor("font_color", "Button", new("f4ecd9"));
        theme.SetColor("font_disabled_color", "Button", new("99a59b"));
        theme.SetStylebox("panel", "PanelContainer", new StyleBoxFlat { BgColor = new("243c38"), BorderColor = new("899c78"), BorderWidthTop = 2, CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12, ContentMarginLeft = 20, ContentMarginRight = 20, ContentMarginTop = 12, ContentMarginBottom = 10 });
        foreach (string state in new[] { "normal", "hover", "pressed", "disabled", "focus" })
        {
            var style = new StyleBoxFlat { BgColor = new(state switch { "hover" => "58765d", "pressed" => "6c805f", "disabled" => "2e4540", "focus" => "4c6956", _ => "405e4e" }), BorderColor = new("a5b68a"), BorderWidthBottom = state == "hover" ? 2 : 0, CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5, ContentMarginTop = 7, ContentMarginBottom = 7, ContentMarginLeft = 10, ContentMarginRight = 10 };
            theme.SetStylebox(state, "Button", style);
        }
        root.Theme = theme;
        _settings = new ClientSettings { Name = "ClientSettings" }; AddChild(_settings); _settings.Initialize(root, theme);
        using var music = GD.Load<AudioStreamWav>("res://Assets/Music/LVS04_11_Echoes_of_Valhalla_bpm82_loop.wav");
        var musicPlayer = new AudioStreamPlayer { Name = "BackgroundMusic", Stream = music, Bus = "Master", VolumeDb = -12, Autoplay = true };
        musicPlayer.TreeExiting += musicPlayer.Stop;
        AddChild(musicPlayer);
        _panel = new PanelContainer { Name = "BottomPanel", MouseFilter = Control.MouseFilterEnum.Stop };
        root.AddChild(_panel); _panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide); _panel.OffsetTop = -220;
        var box = new VBoxContainer(); box.AddThemeConstantOverride("separation", 8); _panel.AddChild(box);
        var columns = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill }; columns.AddThemeConstantOverride("separation", 24); box.AddChild(columns);
        var city = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsStretchRatio = 0.32f }; columns.AddChild(city);
        Text(city, "ODOT  /  THE COUNTRYSIDE", 16);
        _stats = Text(city, "Waiting for the village…", 14); _stats.CustomMinimumSize = new(0, 52);
        _roster = new HBoxContainer(); city.AddChild(_roster);
        var context = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsStretchRatio = 0.4f }; columns.AddChild(context);
        _detail = Text(context, "Click a plot or building in the world", 15); _detail.CustomMinimumSize = new(0, 64);
        _buildActions = new HBoxContainer(); context.AddChild(_buildActions);
        _mine = Button(_buildActions, "Mine", () => ContextAction("build", Building.Mine));
        _farm = Button(_buildActions, "Farm", () => ContextAction("build", Building.Farm));
        _barracks = Button(_buildActions, "Barracks", () => ContextAction("build", Building.Barracks));
        _buildingActions = new HBoxContainer(); context.AddChild(_buildingActions);
        _upgrade = Button(_buildingActions, "Upgrade", () => ContextAction("upgrade"));
        _recruit = Button(_buildingActions, "Recruit", () => ContextAction("recruit"));
        var match = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsStretchRatio = 0.28f }; columns.AddChild(match);
        _phase = Text(match, "Lobby", 17); _phase.CustomMinimumSize = new(0, 44);
        _start = Button(match, "Start match", () => game.SendAction("start"));
        _ready = Button(match, "Ready", () => game.SendAction(Me()?.Ready == true ? "unready" : "ready"));
        _pause = Button(match, "Pause match", () => game.SendAction(game.State?.Paused == true ? "resume" : "pause"));
        _reconnect = Button(match, "Reconnect to my city", () => game.Connect());
        _fresh = Button(match, "Join lobby · fresh session", () => game.Connect(true));
        foreach (var (button, name) in new[] { (_mine, "Mine"), (_farm, "Farm"), (_barracks, "Barracks"), (_upgrade, "Upgrade"), (_recruit, "Recruit"),
            (_ready, "Ready"), (_pause, "Pause"), (_start, "Start"), (_reconnect, "Reconnect"), (_fresh, "Fresh") }) button.Name = name;
        var footer = new HBoxContainer(); footer.AddThemeConstantOverride("separation", 20); box.AddChild(footer);
        _status = Text(footer, "Connecting", 13); _status.SizeFlagsStretchRatio = 0.32f;
        _feedback = Text(footer, "", 13); _feedback.SizeFlagsStretchRatio = 0.68f; _feedback.CustomMinimumSize = new(0, 32);
    }
    private void ContextAction(string action, Building building = Building.Empty)
    {
        if (_slot < 0 || !CanEdit()) return;
        game.SendAction(action, _slot, building);
    }
    private bool CanEdit() => game.Connected && game.State is { Phase: Phase.Building, Paused: false } && Me() is { Eliminated: false, Ready: false } && _focus == game.PlayerId;
    private static Label Text(Node parent, string value, int size)
    {
        var label = new Label { Text = value, AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size); parent.AddChild(label); return label;
    }
    private static Button Button(Node parent, string text, Action action)
    {
        var button = new Button { Text = text, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; parent.AddChild(button); button.Pressed += action; return button;
    }
    private CityState? Me() => game.State?.Players.FirstOrDefault(p => p.Id == game.PlayerId);
    private CityState? Focus() => game.State?.Players.FirstOrDefault(p => p.Id == _focus);
    internal void StopAudio()
    {
        var player = GetNodeOrNull<AudioStreamPlayer>("BackgroundMusic");
        if (player is null) return;
        player.Stop(); player.Stream = null;
        player.QueueFree();
    }
    internal object ObserveUi(string id, string? screenshot, int colors)
    {
        var targets = new Dictionary<string, object>();
        void ControlTarget(string name, Control control, Rect2? local = null)
        {
            Rect2 rect = local is { } area ? new Rect2(control.GetGlobalTransformWithCanvas() * area.Position, area.Size) : control.GetGlobalRect();
            Vector2 point = rect.GetCenter();
            if (control.GetViewport() is Window window && window != GetWindow()) point += window.Position;
            point = GetViewport().GetFinalTransform() * point;
            targets[name] = new { X = point.X, Y = point.Y, Visible = control.IsVisibleInTree(), Enabled = control is not BaseButton button || !button.Disabled };
        }
        foreach (Button button in new[] { _start, _farm, _barracks, _upgrade, _recruit, _ready, _reconnect }) ControlTarget(button.Name, button);
        ControlTarget("Settings", _settings.SettingsButton);
        if (_settings.IsOpen)
        {
            ControlTarget("AudioTab", _settings.Categories.GetTabBar(), _settings.Categories.GetTabBar().GetTabRect(1));
            ControlTarget("Volume", _settings.VolumeSlider);
            ControlTarget("CloseSettings", _settings.Dialog.GetOkButton());
        }
        for (int slot = 0; slot < 9; slot++)
        {
            Vector3 point = Center(_focus) + SlotPosition(slot);
            if (_buildingBounds.TryGetValue(_focus, out Aabb?[]? bounds) && bounds[slot] is { } box) point = Center(_focus) + box.GetCenter();
            Vector2 screen = GetViewport().GetFinalTransform() * _camera.UnprojectPosition(point);
            targets["Plot" + slot] = new { X = screen.X, Y = screen.Y, Visible = Focus() is not null, Enabled = game.Connected };
        }
        int master = AudioServer.GetBusIndex("Master");
        using AudioStream? music = GetNode<AudioStreamPlayer>("BackgroundMusic").Stream;
        return new
        {
            Id = id,
            Revision = _revision,
            SelectedSlot = _slot,
            Connected = game.Connected,
            SettingsOpen = _settings.IsOpen,
            MasterVolume = _settings.MasterVolume,
            MasterGain = AudioServer.GetBusVolumeLinear(master),
            MasterMuted = AudioServer.IsBusMute(master),
            Display = DisplayServer.GetName(),
            AudioDriver = AudioServer.GetDriverName(),
            UserDataPath = ProjectSettings.GlobalizePath("user://"),
            Renderer = RenderingServer.GetVideoAdapterName(),
            Models = _assets.Count,
            Materials = _materials.Count,
            MusicLoaded = music is not null,
            Width = GetWindow().Size.X,
            Height = GetWindow().Size.Y,
            Targets = targets,
            Screenshot = screenshot,
            Colors = colors
        };
    }
    private static Vector3 Center(int id) => new((id - 1) * 40, 0, 0);
    private static Vector3 SlotPosition(int slot) => VillageLayout.Slot(slot);
    public override void _Process(double delta)
    {
        MatchSnapshot? state = game.State;
        if (state is not null && state.MatchId != _matchId)
        {
            foreach (Node3D n in _boards.Values.Concat(_units.Values)) n.QueueFree();
            _boards.Clear(); _units.Clear(); _boardKeys.Clear(); _buildingBounds.Clear(); _cityLabels.Clear();
            _matchId = state.MatchId; _revision = -1; _focus = game.PlayerId; _slot = -1; _hover = -1; _rosterKey = "";
        }
        if (state is not null && state.Revision != _revision)
        {
            _revision = state.Revision;
            if (_focus == 0 || !state.Players.Any(p => p.Id == _focus)) _focus = game.PlayerId;
            UpdateWorld(state);
        }
        if (_wasConnected != game.Connected) { _wasConnected = game.Connected; _slot = -1; _hover = -1; }
        if (Focus() is not { Slots.Length: 9 }) _slot = -1;
        string uiKey = $"{_revision}:{game.Connected}:{game.Status}:{game.Feedback}:{_focus}:{_slot}";
        if (uiKey != _uiKey) { _uiKey = uiKey; UpdateUi(); UpdateMarkers(); }
        FrameCamera();
        Vector2 mouse = GetViewport().GetMousePosition();
        int hover = game.Connected && !_settings.BlocksWorldHover(mouse) && !_panel.GetGlobalRect().HasPoint(mouse) ? Pick(mouse) : -1;
        if (hover != _hover) { _hover = hover; UpdateMarkers(); }
    }
    private void UpdateUi()
    {
        if (_phase is null) return;
        MatchSnapshot? s = game.State; CityState? focus = Focus(); CityState? me = Me();
        _status.Text = $"{game.Status}  •  You: P{game.PlayerId}";
        _phase.Text = s is null ? "Waiting for server" : s.Phase switch { Phase.Victory => "VICTORY • three waves held", Phase.Defeat => "DEFEAT • all cities fell", _ => $"{(s.Paused ? "PAUSED • " : "")}{s.Phase}\nWave {s.Wave}/3 • Turn {s.Turn}/3" };
        _feedback.Text = game.Feedback;
        string roster = s is null ? "" : string.Join('|', s.Players.Select(p => $"{p.Id}:{p.Connected}:{p.Ready}:{p.Eliminated}"));
        if (roster != _rosterKey)
        {
            _rosterKey = roster;
            foreach (Node n in _roster.GetChildren()) { _roster.RemoveChild(n); n.QueueFree(); }
            if (s is not null) foreach (CityState city in s.Players)
            {
                int id = city.Id; Button tab = Button(_roster, $"P{id}{(id == game.PlayerId ? " · You" : "")}\n{(city.Eliminated ? "Fallen" : !city.Connected ? "Away" : city.Ready ? "Ready" : "Here")}", () => { _focus = id; _slot = -1; _hover = -1; _rosterKey = ""; }); tab.AddThemeFontSizeOverride("font_size", 13); tab.TooltipText = id == _focus ? "Viewing this city" : "Inspect city";
            }
        }
        _stats.Text = focus is null ? "Up to four players. Start when everyone joins." : $"P{focus.Id}{(_focus == game.PlayerId ? " • YOUR CITY" : " • OBSERVING")}\nGold {focus.Gold}    Food {focus.Food}\nCity {focus.Health}/{s!.Rules.CityHealth}    Army {focus.Soldiers.Length}";
        bool live = game.Connected && s is not null;
        bool edit = CanEdit() && _slot >= 0;
        SlotState slot = _slot >= 0 && focus is not null ? focus.Slots[_slot] : new(Building.Empty, 0);
        Rules r = s?.Rules ?? new();
        _detail.Text = _slot < 0 ? "Click a plot or building in the world\nSelect first, then choose an action below." : slot.Type == Building.Empty ? $"Empty slot {_slot + 1}\nEach building costs {r.BuildCost} gold." : $"{slot.Type} • Level {slot.Level}\n" + (slot.Type switch { Building.Mine => $"+{r.MineOutput * slot.Level} gold each turn", Building.Farm => $"+{r.FarmOutput * slot.Level} food each turn", _ => $"Click Recruit: {r.RecruitCost - Math.Max(0, slot.Level - 1)} food per soldier" });
        string explanation = !game.Connected ? "Waiting for a synchronized connection." : _focus != game.PlayerId && focus is not null ? "Observing · only your city can be edited." : me?.Eliminated == true ? "Your city has fallen · observe or pause." : s?.Paused == true ? "Match frozen · resume to continue." : me?.Ready == true ? "Unready to edit your city." : s?.Phase == Phase.Combat ? "Your army and defender fight automatically." : "";
        if (explanation.Length != 0) _detail.Text += "\n" + explanation;
        _buildActions.Visible = _slot < 0 || slot.Type == Building.Empty;
        _buildingActions.Visible = _slot >= 0 && slot.Type != Building.Empty;
        _mine.Text = $"Mine · {r.BuildCost}g"; _farm.Text = $"Farm · {r.BuildCost}g"; _barracks.Text = $"Barracks · {r.BuildCost}g";
        _mine.Disabled = _farm.Disabled = _barracks.Disabled = !edit || slot.Type != Building.Empty || me!.Gold < r.BuildCost;
        _upgrade.Text = $"Upgrade · {r.UpgradeCost}g"; _upgrade.Disabled = !edit || slot.Level != 1 || me!.Gold < r.UpgradeCost;
        _recruit.Text = $"Recruit · {r.RecruitCost - Math.Max(0, slot.Level - 1)} food"; _recruit.Disabled = !edit || slot.Type != Building.Barracks || me!.Food < r.RecruitCost - Math.Max(0, slot.Level - 1);
        _ready.Text = me?.Ready == true ? "Unready · edit city" : "Ready · finish turn";
        _ready.Disabled = !live || s!.Phase != Phase.Building || s.Paused || me is null || me.Eliminated;
        _ready.Visible = s?.Phase != Phase.Lobby && game.Connected;
        _pause.Visible = game.Connected && s?.Phase != Phase.Lobby;
        _start.Visible = game.Connected && s?.Phase == Phase.Lobby; _start.Disabled = !live;
        _pause.Text = s?.Paused == true ? "Resume whole match" : "Pause whole match";
        _pause.Disabled = !live || s!.Phase is not (Phase.Building or Phase.Combat);
        _reconnect.Visible = !game.Connected; _reconnect.Disabled = game.Status == "Connecting";
        _fresh.Visible = !game.Connected && game.Status != "Connecting" && (s is null || s.Phase == Phase.Lobby || game.Feedback.Contains("expired", StringComparison.Ordinal));
    }
    private void UpdateMarkers()
    {
        _selection.Visible = _slot >= 0;
        if (_selection.Visible) _selection.Position = Center(_focus) + SlotPosition(_slot) + new Vector3(0, 0.025f, 0);
        _hoverMarker.Visible = _hover >= 0 && _hover != _slot;
        if (_hoverMarker.Visible) _hoverMarker.Position = Center(_focus) + SlotPosition(_hover) + new Vector3(0, 0.03f, 0);
    }
    private void FrameCamera()
    {
        Vector2 size = GetViewport().GetVisibleRect().Size;
        float hud = _panel.Size.Y;
        if (size == _frameSize && Math.Abs(hud - _framePanelHeight) < 0.5f && _frameFocus == _focus) return;
        foreach ((int id, Node3D board) in _boards) board.Visible = id == _focus;
        foreach (Node3D unit in _units.Values) unit.Visible = Math.Abs(unit.Position.X - Center(_focus).X) < 20;
        _frameSize = size; _framePanelHeight = hud; _frameFocus = _focus;
        Vector3 center = Center(Math.Max(1, _focus));
        float pitch = Mathf.DegToRad(34), azimuth = Mathf.DegToRad(28);
        Vector3 direction = new(Mathf.Sin(azimuth) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Cos(azimuth) * Mathf.Cos(pitch));
        Vector3 target = center + new Vector3(0, 0, -0.5f);
        _camera.Position = target + direction * 45; _camera.LookAt(target);
        Vector3 right = _camera.GlobalBasis.X, up = _camera.GlobalBasis.Y;
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity, minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
        // Includes terrain edges, fully upgraded roofs and the entire authoritative approach.
        foreach (float x in new[] { -10.5f, 12f }) foreach (float z in new[] { -15f, 15f }) foreach (float y in new[] { -1.5f, 3.8f })
        {
            Vector3 p = new(x, y, z); float px = p.Dot(right), py = p.Dot(up);
            minX = Math.Min(minX, px); maxX = Math.Max(maxX, px); minY = Math.Min(minY, py); maxY = Math.Max(maxY, py);
        }
        float usable = Math.Max(1, size.Y - hud);
        _camera.Size = Math.Max((maxY - minY + 1.2f) * size.Y / usable, (maxX - minX + 1.2f) * size.Y / size.X);
        target = center + right * ((minX + maxX) / 2) + up * ((minY + maxY) / 2 - _camera.Size * hud / size.Y / 2);
        _camera.Position = target + direction * 45; _camera.LookAt(target);
    }
    private void UpdateWorld(MatchSnapshot state)
    {
        foreach (CityState city in state.Players)
        {
            if (!_boards.TryGetValue(city.Id, out Node3D? board))
            {
                board = new Node3D { Position = Center(city.Id) }; AddChild(board); _boards[city.Id] = board;
                board.Name = $"City{city.Id}"; board.Visible = city.Id == _focus;
                CreateLandscape(board);
                _buildingBounds[city.Id] = new Aabb?[9];
                Model(board, "Medieval/building_home_A_blue.gltf", new(0, 0, 1), 1.8f);
                Model(board, "Medieval/building_tower_A_blue.gltf", new(-3, 0, 0), 1.4f);
                Model(board, "Resource/Gold_Bars.gltf", new(4, 0, 1), 0.65f);
                Model(board, "Medieval/sack.gltf", new(4, 0, 2.6f), 0.65f);
                Label(board, new(4, 0.9f, 1.7f), "Gold / Food", 22);
                Box(board, Vector3.Zero, new(0.06f, 0.06f, 1), "e8c44a").Name = "DefenderShot";
                Label3D title = Label(board, new(0, 2.7f, 1), "CITY", 26); _cityLabels[city.Id] = title;
            }
            var beam = board.GetNode<MeshInstance3D>("DefenderShot");
            UnitState? target = state.Enemies.Where(e => e.Destination == city.Id).OrderBy(e => e.Position).ThenBy(e => e.Id).FirstOrDefault();
            beam.Visible = !city.Eliminated && target is not null && city.DefenderCooldown > state.Rules.AttackTicks - 8;
            if (beam.Visible)
            {
                Vector3 from = new(-3, 1.1f, 0); Vector3 to = new((target!.Id % 7 - 3) * 0.43f, 0.4f, -(float)target.Position);
                beam.Position = (from + to) / 2; beam.LookAt(board.ToGlobal(to)); ((BoxMesh)beam.Mesh).Size = new(0.06f, 0.06f, from.DistanceTo(to));
            }
            _cityLabels[city.Id].Text = $"P{city.Id}{(city.Id == game.PlayerId ? " • YOU" : "")}  ♥ {city.Health}\n{(city.Eliminated ? "FALLEN" : "Home · Defender")}";
            string key = string.Join('|', city.Slots.Select(s => $"{s.Type}:{s.Level}"));
            if (_boardKeys.GetValueOrDefault(city.Id) != key)
            {
                Node? old = board.GetNodeOrNull("Buildings"); if (old is not null) { board.RemoveChild(old); old.QueueFree(); }
                var buildings = new Node3D { Name = "Buildings" }; board.AddChild(buildings);
                Array.Clear(_buildingBounds[city.Id]);
                for (int i = 0; i < 9; i++)
                {
                    SlotState slot = city.Slots[i]; if (slot.Type == Building.Empty) continue;
                    string name = slot.Type switch { Building.Mine => "mine", Building.Farm => "windmill", _ => "barracks" };
                    Node3D model = Model(buildings, $"Medieval/building_{name}_blue.gltf", SlotPosition(i), slot.Level == 2 ? 2.0f : 1.7f);
                    model.Name = $"Slot{i}";
                    Aabb bounds = _modelBounds[$"Medieval/building_{name}_blue.gltf"];
                    _buildingBounds[city.Id][i] = model.Transform * model.GetChild<Node3D>(0).Transform * bounds;
                    if (slot.Type == Building.Farm) Label(buildings, SlotPosition(i) + new Vector3(0, 2.15f, 0), "Farm", 23);
                }
                _boardKeys[city.Id] = key;
            }
        }
        var active = new HashSet<int>();
        foreach (CityState city in state.Players) foreach (UnitState soldier in city.Soldiers) UpdateUnit(soldier, city.Id, false, active);
        foreach (UnitState enemy in state.Enemies) UpdateUnit(enemy, enemy.Destination, true, active);
        foreach (int removed in _units.Keys.Where(id => !active.Contains(id)).ToArray()) { _units[removed].QueueFree(); _units.Remove(removed); }
    }
    private void UpdateUnit(UnitState unit, int city, bool enemy, HashSet<int> active)
    {
        active.Add(unit.Id);
        if (!_units.TryGetValue(unit.Id, out Node3D? node))
        {
            node = new Node3D(); AddChild(node); _units[unit.Id] = node;
            Model(node, "Prototype/Dummy_Base.gltf", Vector3.Zero, 0.65f);
            Box(node, new(0, 0.62f, 0), new(0.4f, 0.12f, 0.4f), enemy ? "bb4b43" : "487ecc");
            Label(node, new(0, 1.05f + unit.Id % 2 * 0.55f, 0), "", 20);
        }
        node.Visible = city == _focus;
        node.Position = Center(city) + new Vector3((unit.Id % 7 - 3) * 0.43f, 0, -(float)unit.Position);
        node.GetChildren().OfType<Label3D>().Single().Text = $"{(enemy ? "E" : "S")}{unit.Health}";
        // Snapshot cooldown drives the attack pulse: frozen snapshots mean frozen animation.
        node.Scale = Vector3.One * (unit.Cooldown > (game.State?.Rules.AttackTicks ?? 60) - 6 ? 1.12f : 1);
    }
    private Node3D Model(Node3D parent, string path, Vector3 position, float size)
    {
        if (!_assets.TryGetValue(path, out PackedScene? scene)) _assets[path] = scene = GD.Load<PackedScene>(Assets + path);
        var wrapper = new Node3D { Position = position }; parent.AddChild(wrapper);
        Node3D model = scene.Instantiate<Node3D>(); wrapper.AddChild(model);
        if (!_modelBounds.TryGetValue(path, out Aabb aabb))
        {
            Aabb? bounds = null;
            void Visit(Node3D n, Transform3D transform)
            {
                Transform3D t = transform * n.Transform;
                if (n is MeshInstance3D mesh) { Aabb b = t * mesh.GetAabb(); bounds = bounds is null ? b : bounds.Value.Merge(b); }
                foreach (Node3D child in n.GetChildren().OfType<Node3D>()) Visit(child, t);
            }
            Visit(model, Transform3D.Identity);
            _modelBounds[path] = aabb = bounds ?? new Aabb(Vector3.Zero, Vector3.One);
        }
        float factor = size / Math.Max(aabb.Size.X, Math.Max(aabb.Size.Y, aabb.Size.Z));
        model.Scale *= factor; model.Position -= new Vector3(aabb.GetCenter().X, aabb.Position.Y, aabb.GetCenter().Z) * factor;
        return wrapper;
    }
    private MeshInstance3D Box(Node3D parent, Vector3 position, Vector3 size, string color)
    {
        if (!_materials.TryGetValue(color, out StandardMaterial3D? material)) _materials[color] = material = new StandardMaterial3D { AlbedoColor = new(color), Roughness = 0.9f };
        var mesh = new MeshInstance3D { Mesh = new BoxMesh { Size = size }, Position = position, MaterialOverride = material }; parent.AddChild(mesh); return mesh;
    }
    private static Label3D Label(Node3D parent, Vector3 position, string text, int fontSize)
    {
        var label = new Label3D { Position = position, Text = text, FontSize = fontSize, PixelSize = 0.018f, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, Modulate = new("f9f2d5"), OutlineModulate = new("233a3f"), OutlineSize = 6 }; parent.AddChild(label); return label;
    }
    private Node3D Terrain(Node3D parent, string name, Vector3 position, float rotation = 0)
    {
        string path = $"Medieval/{name}.gltf";
        if (!_assets.TryGetValue(path, out PackedScene? scene)) _assets[path] = scene = GD.Load<PackedScene>(Assets + path);
        Node3D model = scene.Instantiate<Node3D>();
        model.Position = position; model.Scale = Vector3.One * VillageLayout.TerrainScale; model.RotationDegrees = new(0, rotation, 0); parent.AddChild(model);
        // Grass/river bottoms don't need to cast shadows; hills and buildings still do.
        foreach (MeshInstance3D mesh in model.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>()) mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        return model;
    }
    private void CreateLandscape(Node3D board)
    {
        var scenery = new Node3D { Name = "Scenery" }; board.AddChild(scenery);
        for (int row = -5; row <= 5; row++) for (int column = -3; column <= 3; column++)
        {
            bool slope = column == -3 && row >= 2;
            string name = column == 3 ? "hex_river_B" : slope ? "hex_grass_sloped_low" : "hex_grass";
            Terrain(scenery, name, VillageLayout.Hex(column, row), column == 3 ? (Math.Abs(row) % 2 == 1 ? 60 : 240) : slope ? 180 : 0);
        }
        // Tall clusters stay at the far left rim, away from plots and the battle approach.
        foreach ((int column, int row) in new[] { (-3, -4), (-3, -1), (-3, 2), (-3, 4) })
            Terrain(scenery, "hills_A_trees", VillageLayout.Hex(column, row) + new Vector3(0, row >= 2 ? 0.5f : 0, 0), row * 60);
        foreach ((int column, int row) in new[] { (2, -4), (2, -1), (-2, 5) }) Terrain(scenery, "trees_A_small", VillageLayout.Hex(column, row), row * 60);
        foreach ((int column, int row) in new[] { (-2, -3), (2, -3), (2, 4), (-2, 1) }) Terrain(scenery, "rock_single_C", VillageLayout.Hex(column, row));
        Terrain(scenery, "hill_single_A", VillageLayout.Hex(2, 5));
        Model(scenery, "Medieval/barrel.gltf", new(2.4f, 0, 1.6f), 0.45f);
        Model(scenery, "Medieval/sack.gltf", new(2.8f, 0, 1.9f), 0.35f);
        for (int slot = 0; slot < 9; slot++) Outline(scenery, 0.92f, "e0dbaf").Position = SlotPosition(slot) + new Vector3(0, 0.018f, 0);
    }
    private MeshInstance3D Outline(Node3D parent, float scale, string color)
    {
        if (!_outlines.TryGetValue(scale, out ArrayMesh? outline))
        {
            var surface = new SurfaceTool(); surface.Begin(Mesh.PrimitiveType.Triangles);
            for (int edge = 0; edge < 6; edge++)
            {
                float a = Mathf.DegToRad(30 + edge * 60), b = a + Mathf.Pi / 3;
                Vector3 outerA = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * VillageLayout.Radius * scale;
                Vector3 outerB = new Vector3(Mathf.Cos(b), 0, Mathf.Sin(b)) * VillageLayout.Radius * scale;
                Vector3 innerA = outerA * 0.965f, innerB = outerB * 0.965f;
                foreach (Vector3 v in new[] { outerA, innerB, outerB, outerA, innerA, innerB }) surface.AddVertex(v);
            }
            _outlines[scale] = outline = surface.Commit();
        }
        if (!_materials.TryGetValue(color, out StandardMaterial3D? material)) _materials[color] = material = new StandardMaterial3D { AlbedoColor = new(color), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, CullMode = BaseMaterial3D.CullModeEnum.Disabled };
        var marker = new MeshInstance3D { Mesh = outline, MaterialOverride = material, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off }; parent.AddChild(marker); return marker;
    }
    private int Pick(Vector2 mouse)
    {
        if (Focus() is null) return -1;
        Vector3 origin = _camera.ProjectRayOrigin(mouse) - Center(_focus), direction = _camera.ProjectRayNormal(mouse);
        int selected = -1; float nearest = float.PositiveInfinity;
        if (_buildingBounds.TryGetValue(_focus, out Aabb?[]? bounds))
            for (int slot = 0; slot < bounds.Length; slot++)
                if (bounds[slot] is { } box && VillageLayout.RayBounds(origin, direction, box) is { } distance && distance < nearest) { selected = slot; nearest = distance; }
        if (selected >= 0) return selected;
        if (Math.Abs(direction.Y) < 0.001f) return -1;
        Vector3 point = origin + direction * (-origin.Y / direction.Y);
        for (int slot = 0; slot < 9; slot++) if (VillageLayout.Contains(slot, point)) return slot;
        return -1;
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_settings.IsOpen && @event.IsActionPressed("ui_cancel", false))
        {
            _settings.Open(); GetViewport().SetInputAsHandled(); return;
        }
        if (_settings.IsOpen) return;
        if (!game.Connected || @event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse) return;
        int selected = Pick(mouse.Position);
        if (selected >= 0) _slot = selected;
    }
}
