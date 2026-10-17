using Game.Core;
using Godot;

namespace Game;

// Client-side presentation. Gameplay interactions submit ordinary commands.
public partial class Tabletop : Node3D
{
    private readonly IGameSession game;
    private readonly GameApplication? _application;
    private GameApplication Application => _application ?? throw new InvalidOperationException("World presentation requires its application host.");

    public Tabletop(IGameSession game, GameApplication application) : this(game) => _application = application;
    // The receiving HUD can be hosted independently of world/renderer startup.
    // This controller remains outside the SceneTree; its real Controls own GUI input.
    internal Tabletop(IGameSession game) => this.game = game;
    private readonly LandscapeAssets _landscapeAssets = new();
    private readonly Dictionary<int, VillageLandscape> _landscapes = [];
    private readonly Dictionary<int, UnitView> _units = [];
    private readonly Queue<(int Id, long DeathEndTick, double CombatTick, double VisualSeconds)> _deathCleanups = new();
    private MemberHealthBars _healthBars = null!;
    private Control _healthRoot = null!;
    private readonly Dictionary<string, Label> _values = [], _incomes = [];
    private Label _incomeHeading = null!, _incomeContext = null!, _upkeepLabel = null!, _upkeepValue = null!, _balanceLabel = null!, _balanceValue = null!;
    private GridContainer _compactUpkeep = null!;
    private Button _recovery = null!;
    private readonly Dictionary<int, Node3D> _boards = [];
    private readonly Dictionary<int, string> _boardKeys = [];
    private readonly Dictionary<int, Aabb?[]> _buildingBounds = [];
    private readonly Dictionary<string, StandardMaterial3D> _materials = [];
    private readonly Dictionary<float, ArrayMesh> _outlines = [];
    private HomeHealthBar _homeHealth = null!;
    private Camera3D _camera = null!;
    private TabletopCamera _navigation = null!;
    private Button _previousCity = null!, _nextCity = null!, _detailsButton = null!;
    private Label _cityName = null!, _counters = null!, _rosterDetails = null!;
    private PanelContainer _resources = null!;
    private UiDialog _detailsDialog = null!;
    private readonly List<Control> _emptyBuildCells = [];
    private readonly List<Label> _phaseRows = [];
    private Label _phase = null!;
    private Label _status = null!;
    private Label _stats = null!;
    private Label _detail = null!;
    private Label _feedback = null!;
    private HBoxContainer _roster = null!;
    private PanelContainer _panel = null!;
    private GridContainer _buildActions = null!;
    private HBoxContainer _buildGroups = null!;
    private readonly List<Button> _groupButtons = [];
    private string _constructionGroup = "Production";
    private Button _buyPlot = null!, _sell = null!, _rapidTower = null!, _bombardTower = null!;
    private HBoxContainer _marketActions = null!;
    private readonly Dictionary<Game.Core.Resource, Button> _trades = [];
    private Label _upkeep = null!, _reward = null!;
    private HBoxContainer _buildingActions = null!;

    private readonly Dictionary<Building, Button> _construction = [];
    private readonly Dictionary<UnitType, Button> _recruitment = [];

    private MeshInstance3D _selection = null!, _hoverMarker = null!;
    private int _hover = -1;
    private string _uiKey = "";
    private readonly HudInvalidation _hud = new();
    private Vector2 _frameSize;
    private float _framePanelHeight, _frameRightLane;
    private int _frameFocus;
    private Aabb _frameContent;
    private bool _wasConnected;
    private Button _mine = null!, _farm = null!, _barracks = null!, _upgrade = null!, _recruit = null!, _ranged = null!, _ready = null!, _pause = null!, _start = null!, _reconnect = null!, _fresh = null!, _invite = null!;
    private int _focus;
    private int _slot = -1;
    private long _revision = -1;
    private string _matchId = "";
    private ClientSettings _settings = null!;
    private string[] _unitBindings = [];
    private readonly CombatPlayback _playback = new();
    private CombatLayout? _combatLayout;
    private MatchSnapshot? _deploymentSource;
    private UnitState[] _deploymentMembers = [];
    private readonly Dictionary<int, MeleeStrike> _strikes = [];
    private int _playbackGeneration;
    private readonly Dictionary<int, string> _stockpileKeys = [];
    private readonly List<Node3D> _flags = [];
    private VillageFeedback _effects = null!;
    private int _effectFocus;

    internal void SetWorkCounters(WorkCounters? work)
    {
        _playback.Work = work; work?.Support(WorkMetric.FullPoseSamples, WorkMetric.HudSectionRefreshes);
        foreach (UnitView view in _units.Values) view.Work = work;
    }
    public override void _Ready()
    {
        _unitBindings = UnitAssets.Validate(this);
        _effects = new VillageFeedback(); AddChild(_effects);
        _arenaView = new SharedArenaView { Name = "SharedArena", Visible = false }; AddChild(_arenaView);

        _camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Current = true, Far = 180 };
        AddChild(_camera);
        _navigation = new(_camera);
        _selection = Outline(this, 1.02f, "f3d995"); _selection.Visible = false;
        _hoverMarker = Outline(this, 0.97f, "e7eee0"); _hoverMarker.Visible = false;
        var canvas = new CanvasLayer(); AddChild(canvas);
        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore }; canvas.AddChild(root); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        root.Theme = Application.Theme;
        _settings = Application.Settings;
        InitializeHud(root, Application.AddDialog, () => Application.IsModalOpen, Application.RequestInvite);
    }
    public override void _ExitTree()
    {
        foreach (Control? dialog in new Control?[] { _detailsDialog, _researchDialog, _hallDialog, _squadDialog, _runDialog })
            if (dialog is not null && IsInstanceValid(dialog)) { dialog.Hide(); dialog.QueueFree(); }
    }
    private void ContextAction(string action, Building building = Building.Empty, UnitType soldierType = UnitType.Swordsman, TechnologyId technology = TechnologyId.None, ConstructionPayment payment = ConstructionPayment.Standard)
    {
        if (_slot < 0 || !CanEdit()) return;
        game.SendAction(action, _slot, building, soldierType: soldierType, technology: technology, payment: payment);
    }
    private bool CanEdit() => game.Connected && game.State is { Phase: Phase.Building or Phase.Preparation, Paused: false } && Me() is { Eliminated: false, Ready: false } && _focus == game.PlayerId;
    private static Label Text(Node parent, string value, int size)
    {
        var label = new Label { Text = value, AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size); parent.AddChild(label); return label;
    }
    private Button Button(Node parent, string text, Action action)
    {
        var button = new Button { Text = text, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; UiAssets.Decorate(button, text); parent.AddChild(button);
        button.Pressed += () => { if (!_hudInputBlocked()) action(); }; return button;
    }
    internal Vector3 AtmosphereOrigin => Center(_focus);
    internal VillageLandscape? AtmosphereLandscape => _landscapes.GetValueOrDefault(_focus);
    internal void AtmosphereProofView(string view)
    {
        Vector3 point = Center(_focus) + (view switch
        {
            "texture-archery" => VillageLayout.Slot(2),
            "texture-river" => VillageLayout.Hex(2, 4),
            "texture-forest" => VillageLayout.Hex(-3, 4),
            "forest" => VillageLayout.Hex(-4, 4),
            _ => VillageLayout.Hex(VillageLayout.BridgeColumn, VillageLayout.RiverRow)
        });
        _navigation.ProofView(view.StartsWith("texture-", StringComparison.Ordinal) ? "forest" : view, point, _panel.Size.Y / GetViewport().GetVisibleRect().Size.Y);
    }
    internal Node?[] DetailModals => [_hallDialog, _detailsDialog, _researchDialog, _squadDialog, _runDialog];
    internal bool DetailsOpen => GameApplication.SelectModal(DetailModals) is not null;
    private CityState? Me() => game.State?.Players.FirstOrDefault(p => p.Id == game.PlayerId);
    private CityState? Focus() => game.State?.Players.FirstOrDefault(p => p.Id == _focus);
    internal void AppendUiObservation(Dictionary<string, object?> fields, Dictionary<string, object> targets)
    {
        foreach (Button button in new[] { _start, _upgrade, _ready, _pause, _reconnect, _fresh, _invite, _previousCity, _nextCity, _detailsButton }.Concat(_speeds.Values).Concat(_construction.Values).Concat(_recruitment.Values).Concat(_trades.Values).Concat(_groupButtons).Concat(new[] { _buyPlot, _sell, _recovery, _rapidTower, _bombardTower }))
            Application.ObserveControl(targets, button.Name, button);
        Application.ObserveControl(targets, "ResourceTable", _resources);
        Application.ObserveControl(targets, "UpkeepTable", _compactUpkeep);
        if (_detailsDialog.Visible) Application.ObserveControl(targets, "CloseDetails", _detailsDialog.GetOkButton());
        fields["DetailsOpen"] = DetailsOpen;
        Application.ObserveControl(targets, "RunProgression", _openRun);
        fields["RunOpen"] = _runDialog.Visible;
        fields["RunText"] = _runDialog.Content.FindChildren("*", "Label", true, false).OfType<Label>().Select(l => l.Text).ToArray();
        foreach (Button choice in _runDialog.Choices)
            Application.ObserveControl(targets, "Run_" + choice.GetParent().GetParent().Name, choice);
        Application.ObserveControl(targets, "CloseRun", _runDialog.GetOkButton());
        ObserveResearch(targets);
        ObserveArmy(fields, targets);
        fields["StatusBadges"] = _units.Values.Where(v => v.Visible && v.StatusBadge.Length > 0).ToDictionary(v => v.State.Id, v => v.StatusBadge);
        fields["ResearchText"] = _researchDialog.Research.Description.Text;
        fields["DetailsText"] = _inspectionRoster.Text + "\n" + _upkeep.Text + "\n" + _reward.Text + "\n" + _armyDetails.Text + "\n" + _quoteDetails.Text;
        fields["ResourceOrder"] = _values.Keys.ToArray();
        _inspector.Observe(Application, fields, targets);
        foreach ((int id, UnitView view) in _units)
        {
            Aabb pickBounds = view.PickingBounds();
            Vector3 point = new(pickBounds.GetCenter().X, pickBounds.End.Y - .02f, pickBounds.GetCenter().Z);
            Vector2 position = _camera.UnprojectPosition(point);
            Vector2 screen = GetViewport().GetFinalTransform() * position;
            targets["Unit" + id] = new { X = screen.X, Y = screen.Y, Visible = view.Visible && !view.Dead && view.State.Health > 0 && view.State.Deployed && WorldArea().HasPoint(position) && !BlocksWorld(position) && PickUnit(position) == view, Enabled = !Application.IsModalOpen };
        }
        fields["WorldLabels"] = _boards.GetValueOrDefault(_focus)?.FindChildren("*", "Label3D", true, false).OfType<Label3D>()
            .Where(label => label.IsVisibleInTree() && label.Text.Length > 0).Select(label => label.Text).ToArray() ?? [];
        fields["LockedMarkers"] = _boards.TryGetValue(_focus, out Node3D? markersBoard) ? markersBoard.GetNodeOrNull<Node3D>("Buildings")?.GetChildren().OfType<Sprite3D>().Select(marker => marker.Name.ToString()).ToArray() ?? [] : [];

        for (int slot = 0; slot < ArmyConfiguration.TotalPlots; slot++)
        {
            Vector3 point = Center(_focus) + SlotPosition(slot);
            // Quote the exposed roof, not a volume center behind the next
            // authored building. Picking itself still uses original bounds.
            if (_buildingBounds.TryGetValue(_focus, out Aabb?[]? bounds) && bounds[slot] is { } box)
                point = Center(_focus) + new Vector3(box.GetCenter().X, box.End.Y, box.GetCenter().Z);
            Vector2 position = _camera.UnprojectPosition(point);
            Vector2 screen = GetViewport().GetFinalTransform() * position;
            targets["Plot" + slot] = new
            {
                X = screen.X,
                Y = screen.Y,
                Visible = Focus() is not null && !_camera.IsPositionBehind(point)
                && WorldArea().HasPoint(position) && !BlocksWorld(position) && Pick(position) == slot,
                Enabled = game.Connected && !Application.IsModalOpen
            };
        }
        if (_landscapes.TryGetValue(_focus, out VillageLandscape? landscape)) fields["Landscape"] = landscape.Observe(_camera, WorldArea());
        fields["Placements"] = _boards.TryGetValue(_focus, out Node3D? placementBoard) ? new[] { placementBoard.GetNodeOrNull<Node3D>("Buildings"), placementBoard.GetNodeOrNull<Node3D>("Stockpiles") }.OfType<Node3D>().SelectMany(root => root.FindChildren("*", "Node3D", true, false).OfType<Node3D>())
            .Where(n => n.HasMeta("foot") && n.GetParent()?.Name != "Scenery").Select(n =>
            {
                Vector3 contact = n.GetParent<Node3D>().GlobalTransform * LandscapeAssets.Contact(n) - Center(_focus);
                Vector3 anchor = n.GlobalPosition - Center(_focus);
                float support = VillageLayout.Surface(anchor);
                if (n.Name.ToString().StartsWith("Slot", StringComparison.Ordinal) && Focus()?.Slots[int.Parse(n.Name.ToString()[4..], System.Globalization.CultureInfo.InvariantCulture)].Type is Building.ArrowTower or Building.CatapultTower or Building.RapidArrowTower or Building.BombardTower or Building.ResearchTower && placementBoard.GetNodeOrNull<Node3D>("Buildings/Upgrade" + n.Name.ToString()[4..]) is { } basis && basis.GetMeta("asset").AsString() == AssetCatalog.Plinth) support = LandscapeAssets.TowerDeck(basis);
                return new { Name = n.Name.ToString(), Asset = n.GetMeta("asset").AsString(), X = anchor.X, Y = anchor.Y, Z = anchor.Z, Support = support, Top = LandscapeAssets.Bounds(n).End.Y, Contact = new[] { contact.X, contact.Y, contact.Z } };
            }).ToArray() : [];
        fields["Camera"] = _navigation.Observe((InArena ? CombatLayout.ArenaOrigin : Center(_focus)) + new Vector3(2, 0, -3), GetViewport().GetFinalTransform());
        fields["Revision"] = _revision;
        fields["MatchId"] = _matchId;
        fields["MatchPhase"] = game.State?.Phase ?? Phase.Lobby;
        fields["Wave"] = game.State?.Wave ?? 0;
        fields["Paused"] = game.State?.Paused ?? false;
        fields["TurnSerial"] = game.State?.TurnSerial ?? 0;
        fields["SelectedSlot"] = _slot;
        fields["Models"] = _landscapeAssets.Paths.Count();
        fields["LoadedModels"] = _landscapeAssets.Paths.Order().ToArray();
        fields["Materials"] = _materials.Count;
        fields["PhaseText"] = _phase.Text + "\n" + string.Join("\n", _phaseRows.Where(row => row.Visible).Select(row => row.Text)) + "\n" + _counters.Text;
        fields["PhaseRows"] = _phaseRows.Select(row => row.Text).ToArray();
        fields["ObservedCity"] = _focus;
        fields["CityIds"] = game.State?.Players.Select(city => city.Id).Order().ToArray() ?? [];
        fields["HomeHealth"] = _homeHealth.Observe(GetViewport().GetFinalTransform());
        fields["UpkeepText"] = _upkeep.Text; fields["RewardText"] = _reward.Text;
        fields["ConstructionGroup"] = _constructionGroup;
        fields["StatusText"] = _status.Text;
        fields["StatsText"] = _stats.Text + "\n" + string.Join("    ", _values.Values.Select(v => v.Text));
        fields["ResourceIcons"] = _values.Keys.ToDictionary(name => name, _ => UiAssets.Root + "art/woodland/resource_icons.svg");
        fields["ResourceBalances"] = _values.ToDictionary(pair => pair.Key, pair => int.Parse(pair.Value.Text, System.Globalization.CultureInfo.InvariantCulture));
        fields["ResourceIncome"] = _incomes.ToDictionary(pair => pair.Key, pair => pair.Value.Text);
        fields["IncomeLabel"] = _incomeHeading.Text; fields["IncomeContext"] = _incomeContext.Text;
        fields["CompactUpkeep"] = new[] { _upkeepLabel.Text, _upkeepValue.Text, _balanceLabel.Text, _balanceValue.Text };
        fields["ResourceRowsSingleLine"] = _values.Values.All(label => label.Size.Y <= label.GetThemeFont("font").GetHeight(label.GetThemeFontSize("font_size")) + 1);
        fields["HudHeight"] = _panel.Size.Y;
        fields["HudTop"] = (GetViewport().GetFinalTransform() * _panel.Position).Y;
        fields["HealthBars"] = _healthBars.Observe(GetViewport().GetFinalTransform());
        fields["SharedArenaVisible"] = _arenaView?.Visible == true;
        if (_arenaView is not null) fields["ArenaFrame"] = _arenaView.ObserveFrame(_camera);
        fields["SquadSummaries"] = _arenaView?.Observe(_camera, GetViewport().GetFinalTransform(), WorldArea()) ?? [];
        fields["FeedbackText"] = _feedback.Text;
        fields["DetailText"] = _detail.Text;
        fields["RosterText"] = _rosterDetails.Text;
        fields["UnitBindings"] = _unitBindings;
        fields["Units"] = _units.Values.OrderBy(u => u.State.Id).Select(u => u.Observe()).ToArray();
        fields["Strikes"] = _strikes.OrderBy(p => p.Key).Select(p => p.Value.Observe(_camera, GetViewport().GetFinalTransform())).ToArray();
        fields["CombatTick"] = _playback.Tick; fields["VisualSeconds"] = _playback.VisualSeconds;
        fields["DeathCleanups"] = _deathCleanups.Select(sample => new { sample.Id, sample.DeathEndTick, sample.CombatTick, sample.VisualSeconds }).ToArray();
        fields["EventCursor"] = _playback.EventCursor; fields["PlaybackGeneration"] = _playback.Generation;
        fields["Stockpiles"] = _boards.TryGetValue(_focus, out Node3D? observedBoard) ? new
        { Gold = observedBoard.GetNodeOrNull("Stockpiles/Gold")?.GetChildCount() ?? 0, Food = observedBoard.GetNodeOrNull("Stockpiles/Food")?.GetChildCount() ?? 0, Wood = observedBoard.GetNodeOrNull("Stockpiles/Wood")?.GetChildCount() ?? 0 } : null;
        fields["Effects"] = _effects.Observe();
        fields["PlotHeights"] = Enumerable.Range(0, 9).Select(i => SlotPosition(i).Y).ToArray();
        fields["BuildingVariants"] = _boards.TryGetValue(_focus, out Node3D? variantBoard) ? Enumerable.Range(0, 9)
            .Select(i => variantBoard.HasNode("Buildings/Upgrade" + i) ? 2 : variantBoard.HasNode("Buildings/Slot" + i) ? 1 : 0).ToArray() : [];
        fields["AmbientAngles"] = Array.Empty<float>();
    }
    private Vector3 Center(int id) => game.State?.Arena is not null ? CombatLayout.DistrictOrigin(game.State) : Vector3.Zero;
    internal void ReplayFocus(int city) => _focus = city;
    private Vector3 SlotPosition(int slot) => ArmyConfiguration.IsArmyPlot(slot) && game.State is { } state ? ArmyPlotView.PlotPosition(state, slot, _focus) : VillageLayout.Slot(slot);
    private SlotState? SelectedPlot(CityState? city) => city is null || _slot < 0 ? null
        : ArmyConfiguration.IsArmyPlot(_slot) ? city.Army?.Homes.FirstOrDefault(h => h.PlotId == _slot)?.Plot : city.Slots.ElementAtOrDefault(_slot);
    internal Dictionary<string, double>? ReplayCosts { get; set; }
    private long ReplayMark(string phase, long start)
    {
        if (ReplayCosts is null) return 0;
        ReplayCosts[phase] = System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        return System.Diagnostics.Stopwatch.GetTimestamp();
    }
    internal void ReplayZoom(float zoom)
    {
        if (_navigation.Zoom == zoom) return;
        _navigation.ZoomAt(WorldArea().GetCenter(), zoom > _navigation.Zoom, Math.Abs(Mathf.Log(zoom / _navigation.Zoom) / Mathf.Log(1.12f)));
    }
    public override void _Process(double delta)
    {
        long mark = ReplayCosts is null ? 0 : System.Diagnostics.Stopwatch.GetTimestamp();
        MatchSnapshot? state = game.State;
        if (state is not null && state.MatchId != _matchId)
        {
            foreach (Node3D n in _boards.Values.Concat<Node3D>(_units.Values)) n.QueueFree();
            ClearBars();
            _boards.Clear(); _landscapes.Clear(); _units.Clear(); _deathCleanups.Clear(); _boardKeys.Clear(); _buildingBounds.Clear(); _stockpileKeys.Clear(); _flags.Clear(); _effects.Clear();
            CancelGesture(); _inspector.Close();
            _hallDialog.Hide(); _hallSlot = -1; _storedUnit = 0; _hallKey = "";
            _squadDialog.Hide(); _selectedSquad = _selectedMember = 0; _squadRosterKey = ""; _showDeployment = false;
            _navigation.Reset(); _frameFocus = -1;
            _matchId = state.MatchId; _constructionGroup = "Production"; _revision = -1; _focus = game.PlayerId; _slot = -1; _hover = -1;
        }
        if (state is not null && state.Revision != _revision)
        {
            _revision = state.Revision;
            if (state.Phase == Phase.Combat) _showDeployment = false;
            if (_focus == 0 || !state.Players.Any(p => p.Id == _focus)) _focus = game.PlayerId;
            (game as Main)?.Timing?.Record("presentation-world-begin", state);
            UpdateWorld(state);
            _playback.Accept(state, !_wasConnected && game.Connected);
            (game as Main)?.Timing?.Record("presentation-state-accepted", state, info: new { Tick = _playback.Tick });
        }
        if (!_wasConnected && game.Connected && state is not null) _playback.Accept(state, baseline: true);
        if (_wasConnected != game.Connected) { _wasConnected = game.Connected; _slot = -1; _hover = -1; }
        mark = ReplayMark("WorldAcceptance", mark);
        if (Focus() is null || _slot >= ArmyConfiguration.TotalPlots) _slot = -1;
        string uiKey = $"{_revision}:{game.Connected}:{game.Status}:{game.Feedback}:{game.CanStart}:{game.CanInvite}:{_focus}:{_slot}:{game.PlayerId}:{game.HostPlayerId}:{_constructionGroup}";
        if (uiKey != _uiKey) { _uiKey = uiKey; RefreshHud(_focus, _slot); UpdateMarkers(); }
        mark = ReplayMark("Hud", mark);
        UpdateUnits(delta);
        mark = ReplayMark("Units", mark);
        FrameCamera();
        _navigation.Move(delta, Focus() is not null && GetWindow().HasFocus() && !Application.IsModalOpen && GetViewport().GuiGetFocusOwner() is not (LineEdit or TextEdit), WorldArea().Size.Y / GetViewport().GetVisibleRect().Size.Y);
        mark = ReplayMark("Camera", mark);
        if (!InArena && _landscapes.TryGetValue(_focus, out VillageLandscape? landscape))
            landscape.CoverBattlefield(BattlefieldFrame.GroundRect(_frameContent), new Rect2(_navigation.Boundary.Position - new Vector2(Center(_focus).X, Center(_focus).Z), _navigation.Boundary.Size));
        mark = ReplayMark("Landscape", mark);
        UpdateHealthBars();
        UpdateInspector(_units, _playback.Tick);
        _homeHealth.Sample(Focus(), game.State?.Rules.CityHealth ?? 100, Center(_focus) + VillageLandscape.Home + new Vector3(0, 2.7f, 0), _camera, WorldArea());
        _homeHealth.Visible = false; // SharedArenaView owns the one physical Keep in both phases.
        mark = ReplayMark("Overlays", mark);
        Vector2 mouse = GetViewport().GetMousePosition();
        int hover = game.Connected && !BlocksWorld(mouse) && !_panel.GetGlobalRect().HasPoint(mouse) ? Pick(mouse) : -1;
        if (hover != _hover) { _hover = hover; UpdateMarkers(); }
        ReplayMark("HoverPicking", mark);
    }
    internal void RefreshHud(int focus, int slot)
    {
        if (slot is < -1 or >= ArmyConfiguration.TotalPlots) throw new ArgumentOutOfRangeException(nameof(slot));
        _focus = focus; _slot = slot;
        UpdateUi();
    }
    private void UpdateUi()
    {
        if (_phase is null) return;
        MatchSnapshot? s = game.State; CityState? focus = Focus(); CityState? me = Me();
        var inputs = HudInvalidation.Capture(s, _focus, _slot, _constructionGroup, game.Connected, game.Status, game.Feedback, game.PlayerId, game.HostPlayerId, game.CanStart, game.CanInvite);
        RefreshResearch();
        RefreshRun();
        RefreshArmy();
        RefreshSquads();
        RefreshCombatSpeed();
        bool statusChanged = _hud.Refresh(HudSection.Status, inputs[HudSection.Status]);
        bool economyChanged = _hud.Refresh(HudSection.Economy, inputs[HudSection.Economy]);
        ProgressionView? progression = statusChanged || economyChanged ? ProgressionPresentation.Describe(s, focus) : null;
        if (statusChanged)
        {
            _playback.Work?.Add(WorkMetric.HudSectionRefreshes);
            _status.Text = game.Connected ? "" : game.Status;
            _status.Visible = _status.Text.Length != 0;
            _phase.Text = s?.Phase switch { Phase.Lobby => "Lobby", Phase.RewardDraft => s.Paused ? "PAUSED · Reward draft" : "Reward draft", Phase.Victory or Phase.Defeat => progression!.Phase, _ => s?.Paused == true ? "PAUSED" : "" };
            _phase.Visible = _phase.Text.Length != 0;
            _feedback.Text = game.Feedback.Contains("Match started", StringComparison.OrdinalIgnoreCase) ? "" : game.Feedback;
            _feedback.Visible = _feedback.Text.Length != 0;
        }
        if (_hud.Refresh(HudSection.Roster, inputs[HudSection.Roster]))
        {
            _playback.Work?.Add(WorkMetric.HudSectionRefreshes);
            _rosterDetails.Text = s is null ? "" : string.Join("\n", s.Players.OrderBy(p => p.Id).Select(p => $"P{p.Id}{(p.Id == game.HostPlayerId ? " · Host" : "")}{(p.Id == game.PlayerId ? " · You" : "")} · {RunCatalog.Doctrine(p.Run.Doctrine).Name} · {(p.Eliminated ? "Fallen" : !p.Connected ? "Away" : p.Ready ? "Ready" : "Here")}"));
            _inspectionRoster.Text = _rosterDetails.Text;
            _roster.Visible = s?.Phase == Phase.Lobby;
            _cityName.Text = _focus == game.PlayerId ? "[YOU]" : $"[P{_focus}]";
            _previousCity.Disabled = _nextCity.Disabled = s is null || s.Players.Length < 2;
        }
        if (economyChanged)
        {
            _playback.Work?.Add(WorkMetric.HudSectionRefreshes);
            _stats.Text = "";
            _resources.Visible = focus is not null;
            foreach (Game.Core.Resource resource in Enum.GetValues<Game.Core.Resource>()) _values[resource.ToString()].Text = (focus?.Resources.Amount(resource) ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
            EconomyView economy = ProgressionPresentation.Economy(s, focus, game.Connected);
            _incomeHeading.Text = economy.IncomeLabel; _incomeContext.Text = economy.Context; _incomeContext.Visible = economy.Context.Length > 0;
            foreach (Game.Core.Resource resource in Enum.GetValues<Game.Core.Resource>()) _incomes[resource.ToString()].Text = ProgressionPresentation.Income(focus?.ProductionIncome, resource);
            _upkeepLabel.Text = economy.UpkeepLabel; _upkeepValue.Text = economy.UpkeepValue;
            Callable.From(ReflowResources).CallDeferred();
            _balanceLabel.Text = economy.BalanceLabel; _balanceValue.Text = economy.BalanceValue;
            _upkeep.Text = progression!.Food; _reward.Text = progression.Reward;
            _armyDetails.Text = progression.Army + "\nEnemy allocation:\n" + string.Join("\n", s?.Enemies.Where(u => u.Destination == _focus).Select(u => $"#{u.Id} {AssetCatalog.UnitName(u.Type, u.Faction)} · {(u.Deployed ? "deployed" : "queued")} · {ProgressionPresentation.StatusText(u.Statuses, s.Tick)}") ?? []);
            _detailsDialog.DialogText = focus is null ? "" : RunPresentation.Inspection(focus);
            _counters.Text = s is null ? "" : $"Wave {s.Wave}/{s.TotalWaves} · {(s.Phase == Phase.RewardDraft ? "Reward draft · no production" : s.Phase == Phase.Combat ? "Combat" : "Preparation " + s.Turn)}{(s.WaveCatalog.FirstOrDefault(w => w.Number == s.Wave)?.IsBoss == true ? " · BOSS" : "")}";
            string[] phases = ProgressionPresentation.PhaseRows(s);
            for (int i = 0; i < _phaseRows.Count; i++) _phaseRows[i].Text = phases[i];
            foreach (Label row in _phaseRows) row.Visible = s?.Phase is Phase.Building or Phase.Preparation or Phase.Combat;
        }
        if (_hud.Refresh(HudSection.Context, inputs[HudSection.Context]))
        {
            _playback.Work?.Add(WorkMetric.HudSectionRefreshes);
            bool edit = CanEdit() && _slot >= 0;
            bool armyPlot = ArmyConfiguration.IsArmyPlot(_slot);
            ArmyHomeState? home = armyPlot ? focus?.Army?.Homes.FirstOrDefault(h => h.PlotId == _slot) : null;
            SlotState slot = SelectedPlot(focus) ?? new(Building.Empty, 0);
            TowerDefinition? tower = s?.TowerCatalog.FirstOrDefault(t => t.Type == slot.Type && t.Level == slot.Level);
            if (tower is not null) tower = RunEffects.Tower(tower, focus?.Run);
            BuildingDefinition? definition = s?.BuildingCatalog.FirstOrDefault(b => b.Type == slot.Type);
            _detail.Text = _slot < 0 ? "" : !slot.Purchased ? $"Locked plot {_slot + 1} · buy permanent land" : slot.Type == Building.Empty ? "" : (slot.Type == Building.TownHall ? "Town hall\n" : $"{AssetCatalog.BuildingName(slot.Type)} · L{slot.Level}/{definition?.MaximumLevel ?? 2}\n") + (definition?.Produces is Game.Core.Resource output ? ProgressionPresentation.ProducerBenefit(definition, slot.Level, research: focus!.Research, technologies: s!.TechnologyCatalog, run: focus.Run) : definition?.Recruits is not null ? $"Recruit level {slot.Level}; veterans keep their own level" : slot.Type == Building.TownHall ? $"Storage L{slot.CapacityLevel}/3 · healing L{slot.HealingLevel}/3 · open roster" : slot.Type == Building.Market ? "Sell resources in fixed bundles for gold" : slot.Type == Building.ResearchTower ? $"+{slot.Level} research per full three-production cycle" : TowerDamageDescription(tower));
            if (home is not null) _detail.Text = $"Army plot {home.Number} · {home.State} · " + (ArmyConfiguration.IsTower(home.Plot.Type) ? "whole squad home\n" : $"{home.Used}/{focus!.Army!.SquadCapacity} members\n") + _detail.Text;
            string explanation = !game.Connected ? "Waiting for a synchronized connection." : _focus != game.PlayerId && focus is not null ? "Observing · only your city can be edited." : me?.Eliminated == true ? "Your city has fallen · observe or pause." : s?.Paused == true ? "Match frozen · resume to continue." : me?.Ready == true ? "Unready to edit your city." : s?.Phase == Phase.Combat ? "Your army and defender fight automatically." : "";
            if (explanation.Length != 0) _detail.Text += "\n" + explanation;
            _buildActions.Visible = _slot >= 0 && slot.Purchased && slot.Type == Building.Empty && (home is null || home.Used == 0);
            _buyPlot.Visible = _slot >= 0 && !slot.Purchased;
            int expansions = focus?.Slots.Count(p => p.Purchased) - 5 ?? 0;
            ResourceCost plotCost = armyPlot ? UiArmy()?.HomeQuote(focus?.Army?.PurchasedHomes ?? 6) ?? default
                : s is not null && expansions < s.PlotPrices.Length ? new(s.PlotPrices[expansions]) : default;
            _buyPlot.Text = $"Buy plot · {plotCost.Gold} gold"; _buyPlot.Disabled = !edit || slot.Purchased || !me!.Resources.TryPay(plotCost, out _);
            _buyPlot.TooltipText = focus is null ? "" : CostExplanation(plotCost, focus.Resources);
            _buildGroups.Visible = _buildActions.Visible && !armyPlot;
            _detail.Visible = _detail.Text.Trim().Length != 0;
            int choices = _construction.Keys.Count(type => armyPlot ? ArmyConfiguration.IsTower(type) : BuildingGroup(type) == _constructionGroup);
            for (int i = 0; i < _emptyBuildCells.Count; i++) _emptyBuildCells[i].Visible = i < 9 - choices;
            foreach (Button tab in _groupButtons) tab.Modulate = tab.Text == _constructionGroup ? Colors.White : new Color(.7f, .7f, .7f);
            _buildingActions.Visible = _slot >= 0 && slot.Type != Building.Empty;

            foreach ((Building type, Button button) in _construction)
            {
                button.Visible = armyPlot ? ArmyConfiguration.IsTower(type) : BuildingGroup(type) == _constructionGroup;
                BuildingDefinition? build = s?.BuildingCatalog.FirstOrDefault(b => b.Type == type);
                button.Text = BuildingName(type) + (build?.Produces is not null ? " · " + ProgressionPresentation.ProducerBenefit(build, research: focus?.Research ?? default, technologies: s!.TechnologyCatalog, run: focus?.Run) : "");
                button.AddThemeFontSizeOverride("font_size", 11);
                button.Disabled = !edit || slot.Type != Building.Empty || home?.Used > 0 || build is null || !slot.Purchased || !me!.Resources.TryPay(build.Construction, out _);
                UiAssets.Cost(button, build?.Construction ?? default, stacked: true);
                button.TooltipText = explanation.Length > 0 ? explanation : build is null || focus is null ? "" : CostExplanation(build.Construction, focus.Resources);
            }
            BuildingDefinition? lumbermill = s?.BuildingCatalog.FirstOrDefault(b => b.Type == Building.Lumbermill);
            _recovery.Visible = !armyPlot && _buildActions.Visible && _constructionGroup == "Production" && focus?.Wood == 0 && lumbermill?.RecoveryConstruction is not null;
            ResourceCost recoveryCost = lumbermill?.RecoveryConstruction ?? default;
            _recovery.Text = $"{BuildingName(Building.Lumbermill)} · gold recovery · {ResourceText(recoveryCost)} · {(lumbermill is null ? "" : ProgressionPresentation.ProducerBenefit(lumbermill, research: focus?.Research ?? default, technologies: s!.TechnologyCatalog, run: focus?.Run))}";
            _recovery.Disabled = !edit || !_recovery.Visible || !me!.Resources.TryPay(recoveryCost, out _);
            _recovery.TooltipText = explanation.Length > 0 ? explanation : focus is null ? "" : "Explicit recovery at zero wood.\n" + CostExplanation(recoveryCost, focus.Resources);
            bool baseTower = slot.Type == Building.ArrowTower && slot.Level == 1;
            foreach ((Button button, Building path, string name) in new[] { (_rapidTower, Building.RapidArrowTower, "Rapid arrow tower"), (_bombardTower, Building.BombardTower, "Bombard tower") })
            {
                ResourceCost? quote = s?.BuildingCatalog.FirstOrDefault(b => b.Type == (path == Building.RapidArrowTower ? Building.ArrowTower : Building.CatapultTower))?.Upgrade;
                TowerDefinition? next = s?.TowerCatalog.FirstOrDefault(t => t.Type == path && t.Level == 2);
                if (next is not null) next = RunEffects.Tower(next, focus?.Run);
                button.Visible = baseTower;
                button.Text = name; UiAssets.Cost(button, quote ?? default, stacked: true);
                button.Disabled = !edit || !baseTower || quote is not { } cost || !me!.Resources.TryPay(cost, out _);
                button.TooltipText = next is null ? "" : TowerDamageDescription(next) + "\n" + (focus is null || quote is null ? "" : CostExplanation(quote.Value, focus.Resources));
            }
            _upgrade.Visible = slot.Type != Building.TownHall && !baseTower;
            _upgrade.Text = slot.UpgradeQuote is null ? "Maximum level" : $"Upgrade to L{slot.Level + 1}{(definition?.Produces is not null ? " · " + ProgressionPresentation.ProducerBenefit(definition, slot.Level, upgrade: true, research: focus!.Research, technologies: s!.TechnologyCatalog, run: focus.Run) : "")}\n ";
            _upgrade.Disabled = !edit || slot.UpgradeQuote is not ResourceCost upgrade || !me!.Resources.TryPay(upgrade, out _);
            UiAssets.Cost(_upgrade, slot.UpgradeQuote ?? default);
            _upgrade.TooltipText = definition?.Recruits is UnitType[] offers && focus is not null ? string.Join('\n', focus.RecruitmentQuotes.Where(q => offers.Contains(q.Type) && q.Level == slot.Level + 1).Select(q => $"Next: {AssetCatalog.UnitName(q.Type)} L{q.Level} · individual · HP {HealthPoints.Format(q.Profile.Health)} · damage {HealthPoints.Format(q.Profile.Damage)} · equipment {ResourceText(q.Cost)} · food upkeep {q.Upkeep}/battle")) : "Producer output and tower improvement follow the authoritative next-level quote.";
            if (slot.UpgradeQuote is ResourceCost nextUpgrade && focus is not null) _upgrade.TooltipText += "\n" + CostExplanation(nextUpgrade, focus.Resources);
            _sell.Text = "Sell · " + (slot.Refund == default ? "no refund" : ResourceText(slot.Refund)); _sell.Disabled = !edit || slot.Type == Building.Empty || slot.Type == Building.TownHall && focus!.Soldiers.Any(u => u.Assignment is { Stored: true } a && a.HallSlot == _slot && a.HallGeneration == slot.Generation);
            _sell.TooltipText = "Refund half the building's total paid construction and upgrades. Land, army, health and research remain.";
            _marketActions.Visible = slot.Type == Building.Market;
            foreach ((Game.Core.Resource resource, Button button) in _trades)
            {
                MarketRate? rate = s?.MarketRates.FirstOrDefault(r => r.Resource == resource);
                button.Text = rate is null ? resource.ToString() : $"{rate.Units} {resource}\n+{rate.Gold + (focus?.MarketSaleBonus ?? 0)} gold";
                button.Disabled = !edit || slot.Type != Building.Market || rate is null || me!.Resources.Amount(resource) < rate.Units;
                button.TooltipText = $"Stock: {focus?.Resources.Amount(resource) ?? 0} {resource}. Sell whole bundles for gold. Food sales change the next battle forecast.";
                if (resource == Game.Core.Resource.Food && focus is not null && rate is not null) button.TooltipText = ProgressionPresentation.FoodSalePreview(focus, rate, s?.Phase ?? Phase.Lobby);
            }
            foreach ((UnitType type, Button button) in _recruitment)
            {
                RecruitmentQuote? quote = focus?.RecruitmentQuotes.FirstOrDefault(q => q.Type == type && q.Level == slot.Level);
                button.Text = $"{AssetCatalog.UnitName(type)} L{slot.Level}\n ";
                button.Visible = definition?.Recruits?.Contains(type) == true;
                button.Disabled = !edit || !button.Visible || quote is null || !me!.Resources.TryPay(quote.Cost, out _)
                    || UiArmy()!.Find(focus!.Soldiers, focus.Army!.PurchasedHomes, plots: focus.Army.Homes.Select(h => h.Plot).ToArray(), type: type) is null;
                UiAssets.Cost(button, quote?.Cost ?? default);
                if (quote is not null) button.TooltipText = (UiArmy()!.Find(focus!.Soldiers, focus.Army!.PurchasedHomes, plots: focus.Army.Homes.Select(h => h.Plot).ToArray(), type: type) is null ? "Squad homes full · buy an empty plot, store or retire a member.\n" : "") + $"L{quote.Level} · HP {HealthPoints.Format(quote.Profile.Health)} · X/Y {HealthPoints.Format(quote.Profile.Damage)}/{HealthPoints.Format(quote.Profile.SecondaryDamage)} · food upkeep {quote.Upkeep}/battle\n" + CostExplanation(quote.Cost, focus!.Resources);
            }
            _quoteDetails.Text = _detail.Text + "\n" + string.Join("\n\n", _construction.Values.Concat(_recruitment.Values).Concat(_trades.Values).Append(_upgrade).Append(_rapidTower).Append(_bombardTower).Append(_buyPlot).Append(_recovery).Where(button => button.IsVisibleInTree() && button.TooltipText.Length > 0).Select(button => button.Text.Trim() + "\n" + button.TooltipText));
            foreach (Button button in _construction.Values.Concat(_recruitment.Values).Append(_upgrade).Append(_rapidTower).Append(_bombardTower))
                if (button.GetNodeOrNull<RichTextLabel>("Cost") is { } cost) cost.Modulate = button.Disabled ? new Color(1, 1, 1, 0.6f) : Colors.White;
        }
        if (_hud.Refresh(HudSection.Controls, inputs[HudSection.Controls]))
        {
            _playback.Work?.Add(WorkMetric.HudSectionRefreshes);
            bool live = game.Connected && s is not null;
            _ready.Text = me?.Ready == true ? "Unready" : s?.Phase == Phase.Preparation ? "Ready for battle" : "Ready";
            _ready.TooltipText = me?.Ready == true ? "Withdraw readiness and resume preparation edits."
                : s?.Phase == Phase.Preparation ? "Settle one production using current buildings, research and stored recovery, then pay food upkeep and enter battle."
                : "Settle one production using current buildings, research and stored recovery, then advance preparation.";
            _ready.Disabled = !live || s!.Phase is not (Phase.Building or Phase.Preparation) || s.Paused || me is null || me.Eliminated;
            _ready.Visible = s?.Phase != Phase.Lobby && game.Connected;
            _pause.Visible = game.Connected && s?.Phase != Phase.Lobby;
            _start.Visible = game.Connected && s?.Phase == Phase.Lobby && game.CanStart; _start.Disabled = !live || !game.CanStart;
            _invite.Visible = game.HostPlayerId == game.PlayerId && game.HostPlayerId != 0; _invite.Disabled = !live || !game.CanInvite;
            _invite.TooltipText = game.CanInvite ? "Invite friends through Steam" : "Invitations are unavailable for this session.";
            _pause.Text = s?.Paused == true ? "Resume" : "Pause";
            UiAssets.Decorate(_pause, s?.Paused == true ? "Resume" : "Pause");
            UiAssets.Decorate(_ready, me?.Ready == true ? "Unready" : "Ready");
            _pause.Disabled = !live || s!.Phase is not (Phase.Building or Phase.Preparation or Phase.Combat or Phase.RewardDraft);
            _reconnect.Visible = !game.Connected; _reconnect.Disabled = game.Status == "Connecting";
            _fresh.Visible = !game.Connected && game.Status != "Connecting" && (s is null || s.Phase == Phase.Lobby || game.Feedback.Contains("expired", StringComparison.Ordinal));
        }
    }
    private static string BuildingGroup(Building type) => type switch
    { Building.Barracks or Building.ArcheryRange or Building.Arcanum or Building.ResearchTower or Building.TownHall => "Army", Building.ArrowTower or Building.CatapultTower or Building.RapidArrowTower or Building.BombardTower => "Defense", Building.Market => "Trade", _ => "Production" };
    private static string TowerDamageDescription(TowerDefinition? tower) => tower is null ? "" :
        $"X/Y {HealthPoints.Format(tower.Damage)}/{HealthPoints.Format(tower.SecondaryDamage)} every {tower.CadenceTicks} ticks · range {tower.HexRange} hex · " +
        (tower.SecondaryDamage == 0 ? "single target · no splash" : "one target squad · every other living member");
    private static string BuildingName(Building type) => AssetCatalog.BuildingName(type);
    private static string ResourceText(ResourceCost cost) => string.Join(" / ", Enum.GetValues<Game.Core.Resource>().Where(r => cost.Amount(r) > 0).Select(r => $"{cost.Amount(r)} {r}"));
    private static string CostExplanation(ResourceCost cost, ResourceCost stocks) => ProgressionPresentation.CostExplanation(cost, stocks);
    private void UpdateMarkers()
    {
        _selection.Visible = !InArena && _slot >= 0;
        if (_selection.Visible) _selection.Position = Center(_focus) + SlotPosition(_slot) + new Vector3(0, 0.025f, 0);
        _hoverMarker.Visible = !InArena && _hover >= 0 && _hover != _slot;
        if (_hoverMarker.Visible) _hoverMarker.Position = Center(_focus) + SlotPosition(_hover) + new Vector3(0, 0.03f, 0);
    }
    private void FrameCamera()
    {
        Vector2 size = GetViewport().GetVisibleRect().Size;
        float hud = _panel.Size.Y, rightLane = Math.Max(240, _resources.Size.X + 24);
        Aabb content = InArena && _arenaView is not null ? _arenaView.Content : BattlefieldFrame.Content(game.State, _buildingBounds.GetValueOrDefault(_focus)?.OfType<Aabb>() ?? []);
        if (size == _frameSize && Math.Abs(hud - _framePanelHeight) < 0.5f && Math.Abs(rightLane - _frameRightLane) < .5f && _frameFocus == (InArena ? -2 : _focus) && content == _frameContent) return;
        foreach ((int id, Node3D board) in _boards) board.Visible = !InArena && id == _focus;
        if (_arenaView is not null) _arenaView.Visible = true;
        foreach (UnitView unit in _units.Values) unit.Visible = unit.State.Deployed;
        int frame = InArena ? -2 : _focus;
        if (_frameFocus != frame) _navigation.Reset();
        _frameSize = size; _framePanelHeight = hud; _frameRightLane = rightLane; _frameFocus = frame; _frameContent = content;
        Vector3 center = InArena ? CombatLayout.ArenaOrigin : Center(Math.Max(1, _focus));
        BattlefieldFrame.FitCamera(_camera, content, center, size, hud, rightLane);
        _navigation.Fit(new Rect2(BattlefieldFrame.GroundRect(content).Position + new Vector2(center.X, center.Z), BattlefieldFrame.GroundRect(content).Size), GetViewport().GetVisibleRect(), InArena || game.State?.Players.Length > 1);
        _arenaView?.CoverBoundary(_navigation.Boundary, new Rect2(BattlefieldFrame.GroundRect(content).Position + new Vector2(center.X, center.Z), BattlefieldFrame.GroundRect(content).Size));
    }
    private void UpdateWorld(MatchSnapshot state)
    {
        foreach (CityState city in state.Players)
        {
            if (!_boards.TryGetValue(city.Id, out Node3D? board))
            {
                board = new Node3D { Position = Center(city.Id) }; AddChild(board); _boards[city.Id] = board;
                board.Name = $"City{city.Id}"; board.Visible = city.Id == _focus;
                _landscapes[city.Id] = new VillageLandscape(board, _landscapeAssets, district: true);
                _buildingBounds[city.Id] = new Aabb?[ArmyConfiguration.TotalPlots];

            }
            board.Position = Center(city.Id);
            UpdateStockpiles(board, city);
            UpdateArmyMarkers(board, city, state);
            _landscapes[city.Id].SetPlots(city.Slots);
            string key = string.Join('|', city.Slots.Select(s => $"{s.Type}:{s.Level}:{s.Purchased}:{s.CapacityLevel}:{s.HealingLevel}"));
            if (_boardKeys.GetValueOrDefault(city.Id) != key)
            {
                Node? old = board.GetNodeOrNull("Buildings"); if (old is not null) { board.RemoveChild(old); old.QueueFree(); }
                var buildings = new Node3D { Name = "Buildings" }; board.AddChild(buildings);
                Array.Clear(_buildingBounds[city.Id], 0, ArmyConfiguration.PlotOffset);
                for (int i = 0; i < 9; i++)
                {
                    SlotState slot = city.Slots[i];
                    if (slot.Type == Building.Empty)
                    {
                        if (!slot.Purchased) buildings.AddChild(new Sprite3D { Name = "Locked" + i, Position = SlotPosition(i) + new Vector3(0, .6f, 0), Texture = UiAssets.Icon("Gold"), PixelSize = .025f, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, Shaded = false });
                        continue;
                    }
                    BuildingVisual visual = AssetCatalog.Building(slot.Type);
                    Node3D model = Model(buildings, visual.Path, SlotPosition(i), 1.9f);
                    model.Name = $"Slot{i}";
                    Aabb bounds = LandscapeAssets.Bounds(model);
                    if (slot.Level >= 2 || slot.Type == Building.TownHall && Math.Max(slot.CapacityLevel, slot.HealingLevel) >= 2)
                    {
                        Vector3 position = SlotPosition(i) + new Vector3(.85f, 0, .65f);
                        float size = .6f;
                        if (visual.RaisedUpgrade) { position = SlotPosition(i); size = 1.9f; }
                        Node3D addition = Model(buildings, visual.Upgrade, position, size); addition.Name = "Upgrade" + i;
                        if (visual.RaisedUpgrade)
                        {
                            model.Position = new Vector3(model.Position.X, LandscapeAssets.TowerDeck(addition), model.Position.Z);
                            bounds = LandscapeAssets.Bounds(model);
                        }
                        bounds = bounds.Merge(LandscapeAssets.Bounds(addition));
                    }
                    _buildingBounds[city.Id][i] = bounds;
                    if (!ArmyConfiguration.IsTower(slot.Type))
                        Label(buildings, new Vector3(SlotPosition(i).X, bounds.End.Y + .28f, SlotPosition(i).Z), slot.Type == Building.Lumbermill && slot.Level == 1 ? "" : slot.Type == Building.TownHall ? "Town hall" : $"{AssetCatalog.BuildingName(slot.Type)} L{slot.Level}", 20);
                    Node3D flag = Box(buildings, SlotPosition(i) + new Vector3(-0.6f, 1.1f, 0), new(.35f, .2f, .03f), city.Id == game.PlayerId ? "487ecc" : "a67152");
                    _flags.Add(flag);
                }
                _boardKeys[city.Id] = key;
            }
        }
    }
    internal void UpdateStockpiles(Node3D board, CityState city)
    {
        int gold = PresentationLimits.StockpileCount(city.Gold), food = PresentationLimits.StockpileCount(city.Food), wood = PresentationLimits.StockpileCount(city.Wood);
        string key = $"{gold}:{food}:{wood}";
        if (_stockpileKeys.GetValueOrDefault(city.Id) == key) return;
        Node? old = board.GetNodeOrNull("Stockpiles"); if (old is not null) { board.RemoveChild(old); old.QueueFree(); }
        var piles = new Node3D { Name = "Stockpiles" }; board.AddChild(piles);
        foreach (var (name, count, path, row) in new[] { ("Gold", gold, AssetCatalog.GoldCargo, 0), ("Food", food, AssetCatalog.Provisions, 1), ("Wood", wood, AssetCatalog.Timber, 2) })
        {
            var resource = new Node3D { Name = name }; piles.AddChild(resource);
            for (int n = 0; n < count; n++) Model(resource, path, VillageLayout.Hex(-2, row + 2) + new Vector3((n % 3 - 1) * .48f, 0, (n / 3 - .5f) * .5f), 0.45f);
        }
        _stockpileKeys[city.Id] = key;
    }
    private UnitView View(UnitState unit)
    {
        if (!_units.TryGetValue(unit.Id, out UnitView? view))
        { view = new UnitView(unit) { Work = _playback.Work }; AddChild(view); _units.Add(unit.Id, view); }
        return view;
    }
    internal int ReplayVisibleViews => _units.Values.Count(view => view.Visible);
    private void UpdateUnits(double delta)
    {
        if (_playbackGeneration != _playback.Generation)
        {
            foreach (UnitView view in _units.Values) view.QueueFree();
            foreach (MeleeStrike strike in _strikes.Values) strike.QueueFree(); _strikes.Clear();
            _combatLayout = game.State is null ? null : new CombatLayout(new HexBoard(game.State.Arena?.Board ?? game.State.Rules.Combat.Board)) { Arena = game.State.Arena };
            _units.Clear(); _deathCleanups.Clear(); ClearBars(); _effects.Clear(); _playbackGeneration = _playback.Generation;
        }
        _playback.Advance(delta, game.Connected);
        _flags.RemoveAll(f => !GodotObject.IsInstanceValid(f));
        foreach (Node3D flag in _flags) flag.Rotation = new(0, (float)Math.Sin(_playback.VisualSeconds * 2 + flag.Position.X) * 0.13f, 0);
        if (_effectFocus != _focus) { _effectFocus = _focus; _effects.Clear(); }
        bool audible = game.Connected && game.State is { Paused: false };
        foreach (Command request in game.DrainActionCues()) _effects.Action(request, _focus, _playback.VisualSeconds, audible, request.Slot >= 0 ? SlotPosition(request.Slot) : null, Center(_focus));
        _effects.Sample(_playback.VisualSeconds, audible);
        var current = _playback.Units().ToDictionary(u => u.Id);
        if (_combatLayout is null) return;
        if (game.State is { } accepted)
        {
            _combatLayout.Synchronize(accepted);
            _arenaView?.Synchronize(accepted, _combatLayout, accepted.Phase is Phase.Building or Phase.Preparation, game.PlayerId, _selectedSquad);
        }
        // Replace preparation poses with the projected combat-entry deployment,
        // including funding, persistent homes and every co-op sector. Cleanup
        // bodies remain on their existing playback clock until actually expired.
        if (game.State is { Phase: Phase.Building or Phase.Preparation } preparation)
        {
            if (!ReferenceEquals(preparation, _deploymentSource))
            { _deploymentSource = preparation; _deploymentMembers = BattleDeployment.Preview(preparation); }
            foreach (UnitState unit in _deploymentMembers) current[unit.Id] = unit;
        }
        _arenaView?.Sample(_playback, current.Values);
        foreach (UnitState unit in current.Values.Where(u => u.Deployed)) View(unit).Synchronize(unit);
        foreach (CombatEvent entry in _playback.Drain())
        {
            Vector3? source = entry.Unit is { Deployed: true } actor ? _units.GetValueOrDefault(actor.Id)?.StrikeOrigin ?? _combatLayout.Position(actor, entry.Tick) + new Vector3(0, .6f, 0)
                : entry.Tower is { } tower ? _arenaView?.WeaponOrigin(tower) : null;
            _effects.Combat(entry, InArena ? 0 : _focus, _playback.VisualSeconds, audible, source, _combatLayout.EventImpact(entry));
            if (entry.Unit is not null && (entry.Type is CombatEventType.Death or CombatEventType.Hit || entry.Type == CombatEventType.Impact && entry.Unit.Type == UnitType.Crossbowman))
                if (_units.TryGetValue(entry.Unit.Id, out UnitView? existing)) existing.Event(entry, _playback.VisualSeconds);
        }
        foreach ((int id, UnitView view) in _units.ToArray())
        {
            // Missing live IDs with a buffered death stay until its common-clock event.
            bool awaitingDeath = _playback.AwaitingDeath(id);
            if (view.Expired(_playback.Tick) || !current.ContainsKey(id) && !awaitingDeath)
            {
                if (view.Dead)
                {
                    // Timestamp actual removal, not a later verification probe.
                    // Bound diagnostic retention and reset it with playback/session identity.
                    if (_deathCleanups.Count == 64) _deathCleanups.Dequeue();
                    _deathCleanups.Enqueue((id, view.State.Hex!.DeathEndTick, _playback.Tick, _playback.VisualSeconds));
                }
                view.QueueFree(); _units.Remove(id); continue;
            }
            view.Sample(current.GetValueOrDefault(id), _playback.Tick, _playback.VisualSeconds, 0, _combatLayout, current,
                game.State?.Phase is Phase.Building or Phase.Preparation && !view.Dead);
            view.Visible &= view.State.Deployed;
        }
        foreach (int id in _strikes.Keys.Where(id => !_units.ContainsKey(id)).ToArray()) { _strikes[id].QueueFree(); _strikes.Remove(id); }
        foreach ((int id, UnitView view) in _units.Where(p => p.Key > 0 && p.Value.State.Class == UnitClass.Melee))
        {
            if (!_strikes.TryGetValue(id, out MeleeStrike? strike)) { strike = new(); AddChild(strike); _strikes.Add(id, strike); }
            strike.Sample(view, _units.GetValueOrDefault(view.State.TargetId), _playback.Tick, 0, _combatLayout);
        }
    }
    private void ClearBars() => _healthBars.Clear();
    private void UpdateHealthBars()
    {
        if (_combatLayout is not null)
            _healthBars.Sample(_units, _combatLayout, _playback.Tick, game.State?.Rules.Arena.SquadCapacity ?? 6, _camera, WorldArea());
        _arenaView?.UpdateHealthBars(_healthRoot, _camera, WorldArea());
        _healthRoot.Size = new(_frameSize.X, Math.Max(0, _frameSize.Y - _panel.Size.Y));
        _healthRoot.MoveChild(_homeHealth, -1);
    }
    private Node3D Model(Node3D parent, string path, Vector3 position, float size) => _landscapeAssets.Place(parent, path, position, size);
    private MeshInstance3D Box(Node3D parent, Vector3 position, Vector3 size, string color)
    {
        if (!_materials.TryGetValue(color, out StandardMaterial3D? material)) _materials[color] = material = new StandardMaterial3D { AlbedoColor = new(color), Roughness = 0.9f };
        var mesh = new MeshInstance3D { Mesh = new BoxMesh { Size = size }, Position = position, MaterialOverride = material }; parent.AddChild(mesh); return mesh;
    }
    private static Label3D Label(Node3D parent, Vector3 position, string text, int fontSize)
    {
        var label = new Label3D { Position = position, Text = text, FontSize = fontSize, PixelSize = 0.018f, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, Modulate = new("f9f2d5"), OutlineModulate = new("233a3f"), OutlineSize = 6 }; parent.AddChild(label); return label;
    }
    private MeshInstance3D Outline(Node3D parent, float scale, string color)
    {
        if (!_outlines.TryGetValue(scale, out ArrayMesh? outline))
        {
            var surface = new SurfaceTool(); surface.Begin(Mesh.PrimitiveType.Triangles);
            for (int edge = 0; edge < 6; edge++)
            {
                Vector3 outerA = VillageLayout.Corner(edge, scale);
                Vector3 outerB = VillageLayout.Corner(edge + 1, scale);
                Vector3 innerA = outerA * 0.965f, innerB = outerB * 0.965f;
                foreach (Vector3 v in new[] { outerA, innerB, outerB, outerA, innerA, innerB }) surface.AddVertex(v);
            }
            _outlines[scale] = outline = surface.Commit();
        }
        if (!_materials.TryGetValue(color, out StandardMaterial3D? material)) _materials[color] = material = new StandardMaterial3D { AlbedoColor = new(color), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, CullMode = BaseMaterial3D.CullModeEnum.Disabled };
        var marker = new MeshInstance3D { Mesh = outline, MaterialOverride = material, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off }; parent.AddChild(marker); return marker;
    }
    private int Pick(Vector2 mouse) => PickPlotRay(_camera.ProjectRayOrigin(mouse) - Center(_focus), _camera.ProjectRayNormal(mouse));
    internal int PickPlotRay(Vector3 origin, Vector3 direction)
    {
        if (InArena || Focus() is null) return -1;
        int selected = -1; float nearest = float.PositiveInfinity;
        if (_buildingBounds.TryGetValue(_focus, out Aabb?[]? bounds))
            for (int slot = 0; slot < bounds.Length; slot++)
                if (bounds[slot] is { } box && VillageLayout.RayBounds(origin, direction, box) is { } distance && distance < nearest) { selected = slot; nearest = distance; }
        if (Math.Abs(direction.Y) < 0.001f) return selected;
        for (int slot = 0; slot < ArmyConfiguration.TotalPlots; slot++)
        {
            float distance = (SlotPosition(slot).Y - origin.Y) / direction.Y;
            Vector3 point = origin + direction * distance;
            if (distance >= 0 && distance < nearest && VillageLayout.ContainsPoint(SlotPosition(slot), point)) { selected = slot; nearest = distance; }
        }
        if (selected >= 0) return selected;
        return -1;
    }
    private Rect2 WorldArea() => new(Vector2.Zero, new Vector2(GetViewport().GetVisibleRect().Size.X, _panel.Position.Y));
}
