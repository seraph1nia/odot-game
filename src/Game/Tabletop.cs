using Game.Core;
using Godot;

// Entirely client-side presentation. Every interaction submits an ordinary command.
public partial class Tabletop(Main game) : Node3D
{
    private const string Assets = "res://Assets/KayKit/";
    private readonly Dictionary<string, PackedScene> _assets = [];
    private readonly Dictionary<int, Node3D> _units = [];
    private readonly Dictionary<int, Node3D> _boards = [];
    private readonly Dictionary<int, string> _boardKeys = [];
    private readonly Dictionary<int, MeshInstance3D[]> _tiles = [];
    private readonly Dictionary<int, Label3D> _cityLabels = [];
    private Camera3D _camera = null!;
    private Label _phase = null!;
    private Label _status = null!;
    private Label _stats = null!;
    private Label _detail = null!;
    private Label _feedback = null!;
    private HBoxContainer _roster = null!;
    private readonly Button[] _slots = new Button[9];
    private Button _mine = null!, _farm = null!, _barracks = null!, _upgrade = null!, _recruit = null!, _ready = null!, _pause = null!, _start = null!, _reconnect = null!, _fresh = null!;
    private int _focus;
    private int _slot;
    private string _rosterKey = "";
    private long _revision = -1;
    private string _matchId = "";

    public override void _Ready()
    {
        var environment = new WorldEnvironment { Environment = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new("a6c6ce"), AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = new("eff2df"), AmbientLightEnergy = 0.45f, TonemapMode = Godot.Environment.ToneMapper.Linear } };
        AddChild(environment);
        AddChild(new DirectionalLight3D { RotationDegrees = new(-55, -25, 0), LightEnergy = 0.8f, ShadowEnabled = true });
        _camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = 29, Current = true, Far = 500 };
        AddChild(_camera);
        var canvas = new CanvasLayer(); AddChild(canvas);
        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore }; canvas.AddChild(root); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var theme = new Theme { DefaultFontSize = 16 };
        var panel = new StyleBoxFlat { BgColor = new("182e35"), CornerRadiusTopLeft = 14, CornerRadiusBottomLeft = 14, ContentMarginLeft = 18, ContentMarginRight = 18, ContentMarginTop = 18, ContentMarginBottom = 18 };
        theme.SetStylebox("panel", "PanelContainer", panel);
        foreach (string state in new[] { "normal", "hover", "pressed", "disabled" })
        {
            var style = new StyleBoxFlat { BgColor = new(state switch { "hover" => "497b77", "pressed" => "56877c", "disabled" => "253c42", _ => "345b60" }), CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6, ContentMarginTop = 9, ContentMarginBottom = 9, ContentMarginLeft = 8, ContentMarginRight = 8 };
            theme.SetStylebox(state, "Button", style);
        }
        root.Theme = theme;
        var side = new PanelContainer(); root.AddChild(side); side.AnchorLeft = 1; side.AnchorRight = 1; side.AnchorBottom = 1; side.OffsetLeft = -320;
        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled }; side.AddChild(scroll);
        var box = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; box.AddThemeConstantOverride("separation", 8); scroll.AddChild(box);
        Text(box, "ODOT  /  NINE TILES", 24);
        Text(box, "Build together. Hold your ground.", 13);
        _status = Text(box, "Connecting", 14);
        _reconnect = Button(box, "Reconnect to my city", () => game.Connect());
        _fresh = Button(box, "Join lobby with a fresh session", () => game.Connect(true));
        _phase = Text(box, "Lobby", 20);
        _start = Button(box, "Start match", () => game.SendAction("start"));
        _ready = Button(box, "Ready", () => game.SendAction(Me()?.Ready == true ? "unready" : "ready"));
        _pause = Button(box, "Pause match", () => game.SendAction(game.State?.Paused == true ? "resume" : "pause"));
        _roster = new HBoxContainer(); box.AddChild(_roster);
        _stats = Text(box, "", 16);
        Text(box, "Select a square on your board", 14);
        var grid = new GridContainer { Columns = 3 }; box.AddChild(grid);
        for (int i = 0; i < 9; i++)
        {
            int index = i; _slots[i] = Button(grid, (i + 1).ToString(), () => { _slot = index; UpdateUi(); }); _slots[i].CustomMinimumSize = new(86, 45);
        }
        _detail = Text(box, "", 14);
        var builds = new HBoxContainer(); box.AddChild(builds);
        _mine = Button(builds, "Mine", () => game.SendAction("build", _slot, Building.Mine));
        _farm = Button(builds, "Farm", () => game.SendAction("build", _slot, Building.Farm));
        _barracks = Button(builds, "Barracks", () => game.SendAction("build", _slot, Building.Barracks));
        _upgrade = Button(box, "Upgrade", () => game.SendAction("upgrade", _slot));
        _recruit = Button(box, "Recruit soldier", () => game.SendAction("recruit", _slot));
        _feedback = Text(box, "", 14);
        Text(box, "Three turns → produce → automatic battle\nThree waves to win. Survivors keep damage.\nA fallen city's attackers go to its allies.\nPause freezes the entire match.", 13);
        var hint = new Label { Text = "YOUR CITY • 3 × 3\nClick a tile to build. Click a barracks to recruit.", Position = new(24, 20) }; hint.AddThemeColorOverride("font_color", new("18373d")); hint.AddThemeFontSizeOverride("font_size", 18); root.AddChild(hint);
    }
    private static Label Text(Node parent, string value, int size)
    {
        var label = new Label { Text = value, AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        label.AddThemeFontSizeOverride("font_size", size); parent.AddChild(label); return label;
    }
    private static Button Button(Node parent, string text, Action action)
    {
        var button = new Button { Text = text, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; parent.AddChild(button); button.Pressed += action; return button;
    }
    private CityState? Me() => game.State?.Players.FirstOrDefault(p => p.Id == game.PlayerId);
    private CityState? Focus() => game.State?.Players.FirstOrDefault(p => p.Id == _focus);
    private static Vector3 Center(int id) => new((id - 1) * 32, 0, 0);
    private static Vector3 SlotPosition(int slot) => new((slot % 3 - 1) * 3, 0, 4 + slot / 3 * 3);
    public override void _Process(double delta)
    {
        MatchSnapshot? state = game.State;
        if (state is not null && state.MatchId != _matchId)
        {
            foreach (Node3D n in _boards.Values.Concat(_units.Values)) n.QueueFree();
            _boards.Clear(); _units.Clear(); _boardKeys.Clear(); _tiles.Clear(); _cityLabels.Clear();
            _matchId = state.MatchId; _revision = -1; _focus = game.PlayerId;
        }
        if (state is not null && state.Revision != _revision)
        {
            _revision = state.Revision;
            if (_focus == 0 || !state.Players.Any(p => p.Id == _focus)) _focus = game.PlayerId;
            UpdateWorld(state);
        }
        UpdateUi();
        Vector3 center = Center(Math.Max(1, _focus));
        _camera.Position = center + new Vector3(18, 25, 25); _camera.LookAt(center + new Vector3(5, 0, 0));
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
                int id = city.Id; Button(_roster, $"P{id}\n{(city.Eliminated ? "Fallen" : !city.Connected ? "Away" : city.Ready ? "Ready" : "Here")}", () => { _focus = id; UpdateUi(); });
            }
        }
        _stats.Text = focus is null ? "Up to four players. Start when everyone joins." : $"P{focus.Id}{(_focus == game.PlayerId ? " • YOUR CITY" : " • OBSERVING")}\nGold {focus.Gold}    Food {focus.Food}\nCity {focus.Health}/{s!.Rules.CityHealth}    Army {focus.Soldiers.Length}";
        bool live = game.Connected && s is not null;
        bool edit = live && s!.Phase == Phase.Building && !s.Paused && me is { Eliminated: false, Ready: false } && _focus == game.PlayerId;
        SlotState slot = focus?.Slots[_slot] ?? new(Building.Empty, 0);
        for (int i = 0; i < 9; i++)
        {
            SlotState tile = focus?.Slots[i] ?? new(Building.Empty, 0);
            _slots[i].Text = (i == _slot ? "• " : "") + (tile.Type == Building.Empty ? (i + 1).ToString() : $"{tile.Type.ToString()[0]}{tile.Level}");
            _slots[i].TooltipText = tile.Type == Building.Empty ? $"Slot {i + 1}" : $"{tile.Type} • Level {tile.Level}";
            _slots[i].Disabled = focus is null;
        }
        Rules r = s?.Rules ?? new();
        _detail.Text = slot.Type == Building.Empty ? $"Empty slot {_slot + 1}\nEach building costs {r.BuildCost} gold." : $"{slot.Type} • Level {slot.Level}\n" + (slot.Type switch { Building.Mine => $"+{r.MineOutput * slot.Level} gold each turn", Building.Farm => $"+{r.FarmOutput * slot.Level} food each turn", _ => $"Click Recruit: {r.RecruitCost - Math.Max(0, slot.Level - 1)} food per soldier" });
        if (me?.Eliminated == true) _detail.Text += "\nYour city has fallen. You can observe and pause.";
        else if (s?.Paused == true) _detail.Text += "\nMatch frozen. Resume to continue.";
        else if (me?.Ready == true) _detail.Text += "\nUnready to edit your city.";
        else if (s?.Phase == Phase.Combat) _detail.Text += "\nYour army and defender fight automatically.";
        _mine.Disabled = _farm.Disabled = _barracks.Disabled = !edit || slot.Type != Building.Empty || me!.Gold < r.BuildCost;
        _upgrade.Text = $"Upgrade • {r.UpgradeCost} gold"; _upgrade.Disabled = !edit || slot.Level != 1 || me!.Gold < r.UpgradeCost;
        _recruit.Text = $"Recruit soldier • {r.RecruitCost - Math.Max(0, slot.Level - 1)} food"; _recruit.Disabled = !edit || slot.Type != Building.Barracks || me!.Food < r.RecruitCost - Math.Max(0, slot.Level - 1);
        _ready.Text = me?.Ready == true ? "Unready • edit my city" : "Ready • finish this turn";
        _ready.Disabled = !live || s!.Phase != Phase.Building || s.Paused || me is null || me.Eliminated;
        _start.Visible = s?.Phase == Phase.Lobby; _start.Disabled = !live;
        _pause.Text = s?.Paused == true ? "Resume whole match" : "Pause whole match";
        _pause.Disabled = !live || s!.Phase is not (Phase.Building or Phase.Combat);
        _reconnect.Visible = !game.Connected; _reconnect.Disabled = game.Status == "Connecting";
        _fresh.Visible = !game.Connected && game.Status != "Connecting" && (s is null || s.Phase == Phase.Lobby || game.Feedback.Contains("expired", StringComparison.Ordinal));
        foreach ((int id, MeshInstance3D[] tiles) in _tiles)
            for (int i = 0; i < 9; i++) ((StandardMaterial3D)tiles[i].MaterialOverride).AlbedoColor = new(id == _focus && i == _slot ? "e9c96d" : id == game.PlayerId ? "7fa58a" : "829ca2");
    }
    private void UpdateWorld(MatchSnapshot state)
    {
        foreach (CityState city in state.Players)
        {
            if (!_boards.TryGetValue(city.Id, out Node3D? board))
            {
                board = new Node3D { Position = Center(city.Id) }; AddChild(board); _boards[city.Id] = board;
                MeshInstance3D[] tiles = new MeshInstance3D[9];
                for (int i = 0; i < 9; i++) tiles[i] = Box(board, SlotPosition(i) + new Vector3(0, -0.16f, 0), new(2.88f, 0.3f, 2.88f), "7fa58a");
                _tiles[city.Id] = tiles;
                Box(board, new(0, -0.28f, 7), new(10, 0.3f, 10), "526c67");
                Box(board, new(0, -0.15f, -6), new(4, 0.3f, 12.5f), "b2a68a");
                for (int z = -12; z <= 0; z += 2) Box(board, new(0, 0.025f, z), new(0.15f, 0.04f, 0.8f), "e4d8b8");
                Model(board, "Medieval/building_home_A_blue.gltf", new(0, 0, 1), 1.8f);
                Model(board, "Medieval/building_tower_A_blue.gltf", new(-3, 0, 0), 1.4f);
                Model(board, "Resource/Gold_Bars.gltf", new(4, 0, 1), 0.65f);
                Model(board, "Prototype/Can_A.gltf", new(4, 0, 3), 0.6f);
                Model(board, "Medieval/tree_single_A.gltf", new(-5.5f, 0, 7), 1.8f);
                Box(board, Vector3.Zero, new(0.06f, 0.06f, 1), "e8c44a").Name = "DefenderShot";
                Label3D title = Label(board, new(0, 3.7f, 1), "CITY", 32); _cityLabels[city.Id] = title;
            }
            var beam = board.GetNode<MeshInstance3D>("DefenderShot");
            UnitState? target = state.Enemies.Where(e => e.Destination == city.Id).OrderBy(e => e.Position).ThenBy(e => e.Id).FirstOrDefault();
            beam.Visible = !city.Eliminated && target is not null && city.DefenderCooldown > state.Rules.AttackTicks - 8;
            if (beam.Visible)
            {
                Vector3 from = new(-3, 1.1f, 0); Vector3 to = new((target!.Id % 7 - 3) * 0.43f, 0.4f, -(float)target.Position);
                beam.Position = (from + to) / 2; beam.LookAt(board.ToGlobal(to)); ((BoxMesh)beam.Mesh).Size = new(0.06f, 0.06f, from.DistanceTo(to));
            }
            _cityLabels[city.Id].Text = $"P{city.Id}{(city.Id == game.PlayerId ? " • YOU" : "")}  ♥ {city.Health}\n{(city.Eliminated ? "FALLEN" : "Defender • 1 damage / second")}";
            string key = string.Join('|', city.Slots.Select(s => $"{s.Type}:{s.Level}"));
            if (_boardKeys.GetValueOrDefault(city.Id) != key)
            {
                Node? old = board.GetNodeOrNull("Buildings"); if (old is not null) { board.RemoveChild(old); old.QueueFree(); }
                var buildings = new Node3D { Name = "Buildings" }; board.AddChild(buildings);
                for (int i = 0; i < 9; i++)
                {
                    SlotState slot = city.Slots[i]; if (slot.Type == Building.Empty) continue;
                    string name = slot.Type switch { Building.Mine => "mine", Building.Farm => "windmill", _ => "barracks" };
                    Model(buildings, $"Medieval/building_{name}_blue.gltf", SlotPosition(i), slot.Level == 2 ? 2.0f : 1.7f);
                    Label(buildings, SlotPosition(i) + new Vector3(0, 2.2f, 0), $"{slot.Type} {slot.Level}", 32);
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
            Label(node, new(0, 1.1f, 0), "", 28);
        }
        node.Position = Center(city) + new Vector3((unit.Id % 7 - 3) * 0.43f, 0, -(float)unit.Position);
        node.GetChildren().OfType<Label3D>().Single().Text = $"{(enemy ? "E" : "S")} {unit.Health}";
        // Snapshot cooldown drives the attack pulse: frozen snapshots mean frozen animation.
        node.Scale = Vector3.One * (unit.Cooldown > (game.State?.Rules.AttackTicks ?? 60) - 6 ? 1.12f : 1);
    }
    private Node3D Model(Node3D parent, string path, Vector3 position, float size)
    {
        if (!_assets.TryGetValue(path, out PackedScene? scene)) _assets[path] = scene = GD.Load<PackedScene>(Assets + path);
        var wrapper = new Node3D { Position = position }; parent.AddChild(wrapper);
        Node3D model = scene.Instantiate<Node3D>(); wrapper.AddChild(model);
        Aabb? bounds = null;
        void Visit(Node3D n, Transform3D transform)
        {
            Transform3D t = transform * n.Transform;
            if (n is MeshInstance3D mesh) { Aabb b = t * mesh.GetAabb(); bounds = bounds is null ? b : bounds.Value.Merge(b); }
            foreach (Node3D child in n.GetChildren().OfType<Node3D>()) Visit(child, t);
        }
        Visit(model, Transform3D.Identity);
        if (bounds is { } aabb)
        {
            float factor = size / Math.Max(aabb.Size.X, Math.Max(aabb.Size.Y, aabb.Size.Z));
            model.Scale *= factor; model.Position -= new Vector3(aabb.GetCenter().X, aabb.Position.Y, aabb.GetCenter().Z) * factor;
        }
        return wrapper;
    }
    private static MeshInstance3D Box(Node3D parent, Vector3 position, Vector3 size, string color)
    {
        var mesh = new MeshInstance3D { Mesh = new BoxMesh { Size = size }, Position = position, MaterialOverride = new StandardMaterial3D { AlbedoColor = new(color), Roughness = 0.9f } }; parent.AddChild(mesh); return mesh;
    }
    private static Label3D Label(Node3D parent, Vector3 position, string text, int fontSize)
    {
        var label = new Label3D { Position = position, Text = text, FontSize = fontSize, PixelSize = 0.012f, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, Modulate = new("f9f2d5"), OutlineModulate = new("233a3f"), OutlineSize = 6 }; parent.AddChild(label); return label;
    }
    public override void _UnhandledInput(InputEvent input)
    {
        if (input is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse) return;
        Vector3 origin = _camera.ProjectRayOrigin(mouse.Position); Vector3 direction = _camera.ProjectRayNormal(mouse.Position);
        if (Math.Abs(direction.Y) < 0.001) return;
        Vector3 point = origin + direction * (-origin.Y / direction.Y) - Center(_focus);
        int column = (int)Math.Floor((point.X + 4.5) / 3); int row = (int)Math.Floor((point.Z - 2.5) / 3);
        if (column is >= 0 and < 3 && row is >= 0 and < 3) { _slot = row * 3 + column; UpdateUi(); }
    }
}
