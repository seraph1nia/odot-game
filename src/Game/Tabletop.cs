using Game.Core;
using Godot;

namespace Game;

// Client-side presentation. Gameplay interactions submit ordinary commands.
public partial class Tabletop(IGameSession game, GameApplication application) : Node3D
{
    private readonly LandscapeAssets _landscapeAssets = new();
    private readonly Dictionary<int, VillageLandscape> _landscapes = [];
    private readonly Dictionary<int, UnitView> _units = [];
    private readonly Queue<(int Id, long DeathEndTick, double CombatTick, double VisualSeconds)> _deathCleanups = new();
    private readonly Dictionary<int, UnitHealthBar> _healthBars = [];
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
    private AcceptDialog _detailsDialog = null!;
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
    private Button _buyPlot = null!, _sell = null!;
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
    private float _framePanelHeight;
    private int _frameFocus;
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
    private readonly Dictionary<int, MeleeStrike> _strikes = [];
    private int _playbackGeneration;
    private readonly Dictionary<int, string> _stockpileKeys = [];
    private readonly List<(Node3D Node, Vector3 Rotation)> _windmills = [];
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
        AddChild(new WorldEnvironment { Environment = new Godot.Environment { BackgroundMode = Godot.Environment.BGMode.Color, BackgroundColor = new("a7c4c2"), AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = new("f2f0df"), AmbientLightEnergy = 0.2f, TonemapMode = Godot.Environment.ToneMapper.Linear } });
        AddChild(new DirectionalLight3D { RotationDegrees = new(-55, -25, 0), LightEnergy = 0.35f, ShadowEnabled = true, DirectionalShadowMaxDistance = 65 });
        _camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Current = true, Far = 180 };
        AddChild(_camera);
        _navigation = new(_camera);
        _selection = Outline(this, 1.02f, "f3d995"); _selection.Visible = false;
        _hoverMarker = Outline(this, 0.97f, "e7eee0"); _hoverMarker.Visible = false;
        var canvas = new CanvasLayer(); AddChild(canvas);
        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore }; canvas.AddChild(root); root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        root.Theme = application.Theme;
        _healthRoot = new Control { Name = "UnitHealth", MouseFilter = Control.MouseFilterEnum.Ignore, ClipContents = true };
        root.AddChild(_healthRoot); _healthRoot.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        _settings = application.Settings;
        CreateHud(root);
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
        button.Pressed += () => { if (!application.IsModalOpen) action(); }; return button;
    }
    internal bool DetailsOpen => _detailsDialog?.Visible == true || _researchDialog?.Visible == true;
    internal void FocusDetails() { if (_researchDialog.Visible) _researchDialog.GrabFocus(); else _detailsDialog.GrabFocus(); }
    private CityState? Me() => game.State?.Players.FirstOrDefault(p => p.Id == game.PlayerId);
    private CityState? Focus() => game.State?.Players.FirstOrDefault(p => p.Id == _focus);
    internal void AppendUiObservation(Dictionary<string, object?> fields, Dictionary<string, object> targets)
    {
        foreach (Button button in new[] { _start, _upgrade, _ready, _pause, _reconnect, _fresh, _invite, _previousCity, _nextCity, _detailsButton }.Concat(_construction.Values).Concat(_recruitment.Values).Concat(_trades.Values).Concat(_groupButtons).Concat(new[] { _buyPlot, _sell, _recovery }))
            application.ObserveControl(targets, button.Name, button);
        application.ObserveControl(targets, "ResourceTable", _resources);
        application.ObserveControl(targets, "UpkeepTable", _compactUpkeep);
        if (_detailsDialog.Visible) application.ObserveControl(targets, "CloseDetails", _detailsDialog.GetOkButton());
        fields["DetailsOpen"] = DetailsOpen;
        ObserveResearch(targets);
        fields["StatusBadges"] = _units.Values.Where(v => v.Visible && v.StatusBadge.Length > 0).ToDictionary(v => v.State.Id, v => v.StatusBadge);
        fields["ResearchText"] = _researchBalance.Text;
        fields["DetailsText"] = _inspectionRoster.Text + "\n" + _upkeep.Text + "\n" + _reward.Text + "\n" + _armyDetails.Text + "\n" + _quoteDetails.Text;
        fields["ResourceOrder"] = _values.Keys.ToArray();
        _inspector.Observe(application, fields, targets);
        foreach ((int id, UnitView view) in _units)
        {
            Vector3 point = view.GlobalPosition + new Vector3(0, .7f, 0);
            Vector2 position = _camera.UnprojectPosition(point);
            Vector2 screen = GetViewport().GetFinalTransform() * position;
            targets["Unit" + id] = new { X = screen.X, Y = screen.Y, Visible = view.Visible && !view.Dead && view.State.Health > 0 && view.State.Deployed && WorldArea().HasPoint(position) && !BlocksWorld(position), Enabled = !application.IsModalOpen };
        }
        fields["LockedMarkers"] = _boards.TryGetValue(_focus, out Node3D? markersBoard) ? markersBoard.GetNodeOrNull<Node3D>("Buildings")?.GetChildren().OfType<Sprite3D>().Select(marker => marker.Name.ToString()).ToArray() ?? [] : [];

        for (int slot = 0; slot < 9; slot++)
        {
            Vector3 point = Center(_focus) + SlotPosition(slot);
            if (_buildingBounds.TryGetValue(_focus, out Aabb?[]? bounds) && bounds[slot] is { } box) point = Center(_focus) + box.GetCenter();
            Vector2 screen = GetViewport().GetFinalTransform() * _camera.UnprojectPosition(point);
            targets["Plot" + slot] = new { X = screen.X, Y = screen.Y, Visible = Focus() is not null && !_camera.IsPositionBehind(point) && WorldArea().HasPoint(_camera.UnprojectPosition(point)) && !_resources.GetGlobalRect().HasPoint(_camera.UnprojectPosition(point)), Enabled = game.Connected && !application.IsModalOpen };
        }
        if (_landscapes.TryGetValue(_focus, out VillageLandscape? landscape)) fields["Landscape"] = landscape.Observe(_camera, WorldArea());
        fields["Placements"] = _boards.TryGetValue(_focus, out Node3D? placementBoard) ? new[] { placementBoard.GetNodeOrNull<Node3D>("Buildings"), placementBoard.GetNodeOrNull<Node3D>("Stockpiles") }.OfType<Node3D>().SelectMany(root => root.FindChildren("*", "Node3D", true, false).OfType<Node3D>())
            .Where(n => n.HasMeta("foot") && n.GetParent()?.Name != "Scenery").Select(n =>
            {
                Vector3 contact = n.GetParent<Node3D>().GlobalTransform * LandscapeAssets.Contact(n) - Center(_focus);
                Vector3 anchor = n.GlobalPosition - Center(_focus);
                float support = VillageLayout.Surface(anchor);
                if (n.Name.ToString().StartsWith("Slot", StringComparison.Ordinal) && Focus()?.Slots[int.Parse(n.Name.ToString()[4..], System.Globalization.CultureInfo.InvariantCulture)].Type is Building.ArrowTower or Building.CatapultTower or Building.ResearchTower && placementBoard.GetNodeOrNull<Node3D>("Buildings/Upgrade" + n.Name.ToString()[4..]) is { } basis && basis.GetMeta("asset").AsString().EndsWith("tower_base_blue.gltf", StringComparison.Ordinal)) support = LandscapeAssets.TowerDeck(basis);
                return new { Name = n.Name.ToString(), Asset = n.GetMeta("asset").AsString(), X = anchor.X, Y = anchor.Y, Z = anchor.Z, Support = support, Contact = new[] { contact.X, contact.Y, contact.Z } };
            }).ToArray() : [];
        fields["Camera"] = _navigation.Observe(Center(_focus) + new Vector3(2, 0, -3), GetViewport().GetFinalTransform());
        fields["Revision"] = _revision;
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
        fields["ResourceIcons"] = _values.Keys.ToDictionary(name => name, _ => "");
        fields["ResourceBalances"] = _values.ToDictionary(pair => pair.Key, pair => int.Parse(pair.Value.Text, System.Globalization.CultureInfo.InvariantCulture));
        fields["ResourceIncome"] = _incomes.ToDictionary(pair => pair.Key, pair => pair.Value.Text);
        fields["IncomeLabel"] = _incomeHeading.Text; fields["IncomeContext"] = _incomeContext.Text;
        fields["CompactUpkeep"] = new[] { _upkeepLabel.Text, _upkeepValue.Text, _balanceLabel.Text, _balanceValue.Text };
        fields["ResourceRowsSingleLine"] = _values.Values.All(label => label.Size.Y <= label.GetThemeFont("font").GetHeight(label.GetThemeFontSize("font_size")) + 1);
        fields["HudHeight"] = _panel.Size.Y;
        fields["HudTop"] = (GetViewport().GetFinalTransform() * _panel.Position).Y;
        fields["HealthBars"] = _healthBars.OrderBy(p => p.Key).Select(p => p.Value.Observe(p.Key, GetViewport().GetFinalTransform())).ToArray();
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
        fields["AmbientAngles"] = _windmills.Where(w => GodotObject.IsInstanceValid(w.Node)).Select(w => w.Node.Rotation.Z).ToArray();
    }
    private static Vector3 Center(int id) => new((id - 1) * 40, 0, 0);
    internal void ReplayFocus(int city) => _focus = city;
    private static Vector3 SlotPosition(int slot) => VillageLayout.Slot(slot);
    public override void _Process(double delta)
    {
        MatchSnapshot? state = game.State;
        if (state is not null && state.MatchId != _matchId)
        {
            foreach (Node3D n in _boards.Values.Concat<Node3D>(_units.Values)) n.QueueFree();
            ClearBars();
            _boards.Clear(); _landscapes.Clear(); _units.Clear(); _deathCleanups.Clear(); _boardKeys.Clear(); _buildingBounds.Clear(); _stockpileKeys.Clear(); _windmills.Clear(); _flags.Clear(); _effects.Clear();
            CancelGesture(); _inspector.Close();
            _navigation.Reset(); _frameFocus = -1;
            _matchId = state.MatchId; _constructionGroup = "Production"; _revision = -1; _focus = game.PlayerId; _slot = -1; _hover = -1;
        }
        if (state is not null && state.Revision != _revision)
        {
            _revision = state.Revision;
            if (_focus == 0 || !state.Players.Any(p => p.Id == _focus)) _focus = game.PlayerId;
            UpdateWorld(state);
            _playback.Accept(state, !_wasConnected && game.Connected);
        }
        if (!_wasConnected && game.Connected && state is not null) _playback.Accept(state, baseline: true);
        if (_wasConnected != game.Connected) { _wasConnected = game.Connected; _slot = -1; _hover = -1; }
        if (Focus() is not { Slots.Length: 9 }) _slot = -1;
        string uiKey = $"{_revision}:{game.Connected}:{game.Status}:{game.Feedback}:{game.CanStart}:{game.CanInvite}:{_focus}:{_slot}:{game.PlayerId}:{game.HostPlayerId}:{_constructionGroup}";
        if (uiKey != _uiKey) { _uiKey = uiKey; UpdateUi(); UpdateMarkers(); }
        UpdateUnits(delta);
        FrameCamera();
        _navigation.Move(delta, Focus() is not null && GetWindow().HasFocus() && !application.IsModalOpen && GetViewport().GuiGetFocusOwner() is not (LineEdit or TextEdit), WorldArea().Size.Y / GetViewport().GetVisibleRect().Size.Y);
        if (_landscapes.TryGetValue(_focus, out VillageLandscape? landscape)) landscape.Cover(_camera, WorldArea(), TabletopCamera.Travel);
        UpdateHealthBars();
        UpdateInspector();
        _homeHealth.Sample(Focus(), game.State?.Rules.CityHealth ?? 100, Center(_focus) + VillageLandscape.Home + new Vector3(0, 2.7f, 0), _camera, WorldArea());
        Vector2 mouse = GetViewport().GetMousePosition();
        int hover = game.Connected && !BlocksWorld(mouse) && !_panel.GetGlobalRect().HasPoint(mouse) ? Pick(mouse) : -1;
        if (hover != _hover) { _hover = hover; UpdateMarkers(); }
    }
    private void UpdateUi()
    {
        if (_phase is null) return;
        MatchSnapshot? s = game.State; CityState? focus = Focus(); CityState? me = Me();
        var inputs = HudInvalidation.Capture(s, _focus, _slot, _constructionGroup, game.Connected, game.Status, game.Feedback, game.PlayerId, game.HostPlayerId, game.CanStart, game.CanInvite);
        RefreshResearch();
        bool statusChanged = _hud.Refresh(HudSection.Status, inputs[HudSection.Status]);
        bool economyChanged = _hud.Refresh(HudSection.Economy, inputs[HudSection.Economy]);
        ProgressionView? progression = statusChanged || economyChanged ? ProgressionPresentation.Describe(s, focus) : null;
        if (statusChanged)
        {
            _playback.Work?.Add(WorkMetric.HudSectionRefreshes);
            _status.Text = game.Connected ? "" : game.Status;
            _status.Visible = _status.Text.Length != 0;
            _phase.Text = s?.Phase switch { Phase.Lobby => "Lobby", Phase.Victory or Phase.Defeat => progression!.Phase, _ => s?.Paused == true ? "PAUSED" : "" };
            _phase.Visible = _phase.Text.Length != 0;
            _feedback.Text = game.Feedback.Contains("Match started", StringComparison.OrdinalIgnoreCase) ? "" : game.Feedback;
            _feedback.Visible = _feedback.Text.Length != 0;
        }
        if (_hud.Refresh(HudSection.Roster, inputs[HudSection.Roster]))
        {
            _playback.Work?.Add(WorkMetric.HudSectionRefreshes);
            _rosterDetails.Text = s is null ? "" : string.Join("\n", s.Players.OrderBy(p => p.Id).Select(p => $"P{p.Id}{(p.Id == game.HostPlayerId ? " · Host" : "")}{(p.Id == game.PlayerId ? " · You" : "")} · {(p.Eliminated ? "Fallen" : !p.Connected ? "Away" : p.Ready ? "Ready" : "Here")}"));
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
            _balanceLabel.Text = economy.BalanceLabel; _balanceValue.Text = economy.BalanceValue;
            _upkeep.Text = progression!.Food; _reward.Text = progression.Reward;
            _armyDetails.Text = progression.Army + "\nEnemy allocation:\n" + string.Join("\n", s?.Enemies.Where(u => u.Destination == _focus).Select(u => $"#{u.Id} {u.Type} · {(u.Deployed ? "deployed" : "queued")} · {ProgressionPresentation.StatusText(u.Statuses, s.Tick)}") ?? []);
            _detailsDialog.DialogText = "";
            _counters.Text = s is null ? "" : $"Wave {s.Wave}/{s.TotalWaves} · Turn {s.Turn}/3{(s.WaveCatalog.FirstOrDefault(w => w.Number == s.Wave)?.IsBoss == true ? " · BOSS" : "")}";
            string[] phases = ProgressionPresentation.PhaseRows(s);
            for (int i = 0; i < _phaseRows.Count; i++) _phaseRows[i].Text = phases[i];
            foreach (Label row in _phaseRows) row.Visible = s?.Phase is Phase.Building or Phase.Preparation or Phase.Combat;
        }
        if (_hud.Refresh(HudSection.Context, inputs[HudSection.Context]))
        {
            _playback.Work?.Add(WorkMetric.HudSectionRefreshes);
            bool edit = CanEdit() && _slot >= 0;
            SlotState slot = _slot >= 0 && focus is not null ? focus.Slots[_slot] : new(Building.Empty, 0);
            TowerDefinition? tower = s?.TowerCatalog.FirstOrDefault(t => t.Type == slot.Type && t.Level == slot.Level);
            BuildingDefinition? definition = s?.BuildingCatalog.FirstOrDefault(b => b.Type == slot.Type);
            _detail.Text = _slot < 0 ? "" : !slot.Purchased ? $"Locked plot {_slot + 1} · buy permanent land" : slot.Type == Building.Empty ? "" : $"{BuildingName(slot.Type)} · L{slot.Level}/{definition?.MaximumLevel}\n" + (definition?.Produces is Game.Core.Resource output ? ProgressionPresentation.ProducerBenefit(definition, slot.Level) : definition?.Recruits is not null ? $"Recruit level {slot.Level}; veterans keep their own level" : slot.Type == Building.Market ? "Sell resources in fixed bundles for gold" : slot.Type == Building.ResearchTower ? $"+{slot.Level} research per full three-production cycle" : $"{HealthPoints.Format(tower?.Damage ?? 0)} damage · up to {tower?.VictimCap} targets");
            string explanation = !game.Connected ? "Waiting for a synchronized connection." : _focus != game.PlayerId && focus is not null ? "Observing · only your city can be edited." : me?.Eliminated == true ? "Your city has fallen · observe or pause." : s?.Paused == true ? "Match frozen · resume to continue." : me?.Ready == true ? "Unready to edit your city." : s?.Phase == Phase.Combat ? "Your army and defender fight automatically." : "";
            if (explanation.Length != 0) _detail.Text += "\n" + explanation;
            _buildActions.Visible = _slot >= 0 && slot.Purchased && slot.Type == Building.Empty;
            _buyPlot.Visible = _slot >= 0 && !slot.Purchased;
            int expansions = focus?.Slots.Count(p => p.Purchased) - 5 ?? 0;
            ResourceCost plotCost = s is not null && expansions < s.PlotPrices.Length ? new(s.PlotPrices[expansions]) : default;
            _buyPlot.Text = $"Buy plot · {plotCost.Gold} gold"; _buyPlot.Disabled = !edit || slot.Purchased || !me!.Resources.TryPay(plotCost, out _);
            _buyPlot.TooltipText = focus is null ? "" : CostExplanation(plotCost, focus.Resources);
            _buildGroups.Visible = _buildActions.Visible;
            _detail.Visible = _detail.Text.Trim().Length != 0;
            int choices = _construction.Keys.Count(type => BuildingGroup(type) == _constructionGroup);
            for (int i = 0; i < _emptyBuildCells.Count; i++) _emptyBuildCells[i].Visible = i < 9 - choices;
            foreach (Button tab in _groupButtons) tab.Modulate = tab.Text == _constructionGroup ? Colors.White : new Color(.7f, .7f, .7f);
            _buildingActions.Visible = _slot >= 0 && slot.Type != Building.Empty;

            foreach ((Building type, Button button) in _construction)
            {
                button.Visible = BuildingGroup(type) == _constructionGroup;
                BuildingDefinition? build = s?.BuildingCatalog.FirstOrDefault(b => b.Type == type);
                button.Text = BuildingName(type) + (build?.Produces is not null ? " · " + ProgressionPresentation.ProducerBenefit(build) : "") + "\n ";
                button.AddThemeFontSizeOverride("font_size", 11);
                button.Disabled = !edit || slot.Type != Building.Empty || build is null || !slot.Purchased || !me!.Resources.TryPay(build.Construction, out _);
                UiAssets.Cost(button, build?.Construction ?? default);
                button.TooltipText = explanation.Length > 0 ? explanation : build is null || focus is null ? "" : CostExplanation(build.Construction, focus.Resources);
            }
            BuildingDefinition? lumbermill = s?.BuildingCatalog.FirstOrDefault(b => b.Type == Building.Lumbermill);
            _recovery.Visible = _buildActions.Visible && _constructionGroup == "Production" && focus?.Wood == 0 && lumbermill?.RecoveryConstruction is not null;
            ResourceCost recoveryCost = lumbermill?.RecoveryConstruction ?? default;
            _recovery.Text = $"Lumbermill · gold recovery · {ResourceText(recoveryCost)} · {(lumbermill is null ? "" : ProgressionPresentation.ProducerBenefit(lumbermill))}";
            _recovery.Disabled = !edit || !_recovery.Visible || !me!.Resources.TryPay(recoveryCost, out _);
            _recovery.TooltipText = explanation.Length > 0 ? explanation : focus is null ? "" : "Explicit recovery at zero wood.\n" + CostExplanation(recoveryCost, focus.Resources);
            _upgrade.Text = slot.UpgradeQuote is null ? "Maximum level" : $"Upgrade to L{slot.Level + 1}{(definition?.Produces is not null ? " · " + ProgressionPresentation.ProducerBenefit(definition, slot.Level, upgrade: true) : "")}\n ";
            _upgrade.Disabled = !edit || slot.UpgradeQuote is not ResourceCost upgrade || !me!.Resources.TryPay(upgrade, out _);
            UiAssets.Cost(_upgrade, slot.UpgradeQuote ?? default);
            _upgrade.TooltipText = definition?.Recruits is UnitType[] offers && focus is not null ? string.Join('\n', focus.RecruitmentQuotes.Where(q => offers.Contains(q.Type) && q.Level == slot.Level + 1).Select(q => $"Next: {q.Type} L{q.Level} · size {q.Profile.Size} · HP {HealthPoints.Format(q.Profile.Health)} · damage {HealthPoints.Format(q.Profile.Damage)} · equipment {ResourceText(q.Cost)} · food upkeep {q.Upkeep}/battle")) : "Producer output and tower improvement follow the authoritative next-level quote.";
            if (slot.UpgradeQuote is ResourceCost nextUpgrade && focus is not null) _upgrade.TooltipText += "\n" + CostExplanation(nextUpgrade, focus.Resources);
            _sell.Text = "Sell · " + (slot.Refund == default ? "no refund" : ResourceText(slot.Refund)); _sell.Disabled = !edit || slot.Type == Building.Empty;
            _sell.TooltipText = "Refund half the building's total paid construction and upgrades. Land, army, health and research remain.";
            _marketActions.Visible = slot.Type == Building.Market;
            foreach ((Game.Core.Resource resource, Button button) in _trades)
            {
                MarketRate? rate = s?.MarketRates.FirstOrDefault(r => r.Resource == resource);
                button.Text = rate is null ? resource.ToString() : $"{rate.Units} {resource}\n+{rate.Gold} gold";
                button.Disabled = !edit || slot.Type != Building.Market || rate is null || me!.Resources.Amount(resource) < rate.Units;
                button.TooltipText = $"Stock: {focus?.Resources.Amount(resource) ?? 0} {resource}. Sell whole bundles for gold. Food sales change the next battle forecast.";
                if (resource == Game.Core.Resource.Food && focus is not null && rate is not null) button.TooltipText = ProgressionPresentation.FoodSalePreview(focus, rate);
            }
            foreach ((UnitType type, Button button) in _recruitment)
            {
                RecruitmentQuote? quote = focus?.RecruitmentQuotes.FirstOrDefault(q => q.Type == type && q.Level == slot.Level);
                button.Text = $"{type} L{slot.Level}\n ";
                button.Visible = definition?.Recruits?.Contains(type) == true;
                button.Disabled = !edit || !button.Visible || quote is null || !me!.Resources.TryPay(quote.Cost, out _);
                UiAssets.Cost(button, quote?.Cost ?? default);
                if (quote is not null) button.TooltipText = $"L{quote.Level} · size {quote.Profile.Size} · HP {HealthPoints.Format(quote.Profile.Health)} · damage {HealthPoints.Format(quote.Profile.Damage)} · food upkeep {quote.Upkeep}/battle\n" + CostExplanation(quote.Cost, focus!.Resources);
            }
            _quoteDetails.Text = _detail.Text + "\n" + string.Join("\n\n", _construction.Values.Concat(_recruitment.Values).Concat(_trades.Values).Append(_upgrade).Append(_buyPlot).Append(_recovery).Where(button => button.IsVisibleInTree() && button.TooltipText.Length > 0).Select(button => button.Text.Trim() + "\n" + button.TooltipText));
            foreach (Button button in _construction.Values.Concat(_recruitment.Values).Append(_upgrade))
                if (button.GetNodeOrNull<RichTextLabel>("Cost") is { } cost) cost.Modulate = button.Disabled ? new Color(1, 1, 1, 0.6f) : Colors.White;
        }
        if (_hud.Refresh(HudSection.Controls, inputs[HudSection.Controls]))
        {
            _playback.Work?.Add(WorkMetric.HudSectionRefreshes);
            bool live = game.Connected && s is not null;
            _ready.Text = me?.Ready == true ? "Unready" : s?.Phase == Phase.Preparation ? "Ready for battle" : "Ready";
            _ready.TooltipText = s?.Phase == Phase.Preparation ? "Enter battle and pay food upkeep; no production income." : "Advance one building turn and collect production.";
            _ready.Disabled = !live || s!.Phase is not (Phase.Building or Phase.Preparation) || s.Paused || me is null || me.Eliminated;
            _ready.Visible = s?.Phase != Phase.Lobby && game.Connected;
            _pause.Visible = game.Connected && s?.Phase != Phase.Lobby;
            _start.Visible = game.Connected && s?.Phase == Phase.Lobby && game.CanStart; _start.Disabled = !live || !game.CanStart;
            _invite.Visible = game.HostPlayerId == game.PlayerId && game.HostPlayerId != 0; _invite.Disabled = !live || !game.CanInvite;
            _invite.TooltipText = game.CanInvite ? "Invite friends through Steam" : "Invitations are unavailable for this session.";
            _pause.Text = s?.Paused == true ? "Resume" : "Pause";
            UiAssets.Decorate(_pause, s?.Paused == true ? "Resume" : "Pause");
            UiAssets.Decorate(_ready, me?.Ready == true ? "Unready" : "Ready");
            _pause.Disabled = !live || s!.Phase is not (Phase.Building or Phase.Preparation or Phase.Combat);
            _reconnect.Visible = !game.Connected; _reconnect.Disabled = game.Status == "Connecting";
            _fresh.Visible = !game.Connected && game.Status != "Connecting" && (s is null || s.Phase == Phase.Lobby || game.Feedback.Contains("expired", StringComparison.Ordinal));
        }
    }
    private static string BuildingGroup(Building type) => type switch
    { Building.Barracks or Building.ArcheryRange or Building.Arcanum or Building.ResearchTower => "Army", Building.ArrowTower or Building.CatapultTower => "Defense", Building.Market => "Trade", _ => "Production" };
    private static string BuildingName(Building type) => type switch
    { Building.Mine => "Gold mine", Building.MetalMine => "Metal mine", Building.ArcheryRange => "Archery range", Building.ResearchTower => "Research tower", Building.ArrowTower => "Arrow tower", Building.CatapultTower => "Catapult tower", _ => type.ToString() };
    private static string ResourceText(ResourceCost cost) => string.Join(" / ", Enum.GetValues<Game.Core.Resource>().Where(r => cost.Amount(r) > 0).Select(r => $"{cost.Amount(r)} {r}"));
    private static string CostExplanation(ResourceCost cost, ResourceCost stocks) => ProgressionPresentation.CostExplanation(cost, stocks);
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
        foreach (UnitView unit in _units.Values) unit.Visible = unit.State.Deployed && unit.State.Destination == _focus;
        if (_frameFocus != _focus) _navigation.Reset();
        _frameSize = size; _framePanelHeight = hud; _frameFocus = _focus;
        Vector3 center = Center(Math.Max(1, _focus));
        float pitch = Mathf.DegToRad(34), azimuth = Mathf.DegToRad(28);
        Vector3 direction = new(Mathf.Sin(azimuth) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Cos(azimuth) * Mathf.Cos(pitch));
        Vector3 target = center + new Vector3(0, 0, -0.5f);
        _camera.Position = target + direction * 45; _camera.LookAt(target);
        Vector3 right = _camera.GlobalBasis.X, up = _camera.GlobalBasis.Y;
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity, minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
        // Includes terrain edges, fully upgraded roofs and the entire authoritative approach.
        foreach (float x in new[] { -10.5f, 12f }) foreach (float z in new[] { -15f, 15f }) foreach (float y in new[] { -1.5f, 5.2f })
        {
            Vector3 p = new(x, y, z); float px = p.Dot(right), py = p.Dot(up);
            minX = Math.Min(minX, px); maxX = Math.Max(maxX, px); minY = Math.Min(minY, py); maxY = Math.Max(maxY, py);
        }
        float usable = Math.Max(1, size.Y - hud);
        float width = Math.Max(1, size.X - 240);
        _camera.Size = Math.Max((maxY - minY + 1.2f) * size.Y / usable, (maxX - minX + 1.2f) * size.Y / width);
        target = center + right * ((minX + maxX) / 2 + _camera.Size * 120 / size.Y) + up * ((minY + maxY) / 2 - _camera.Size * hud / size.Y / 2);
        _camera.Position = target + direction * 45; _camera.LookAt(target);
        _navigation.Fit();
    }
    private void UpdateWorld(MatchSnapshot state)
    {
        foreach (CityState city in state.Players)
        {
            if (!_boards.TryGetValue(city.Id, out Node3D? board))
            {
                board = new Node3D { Position = Center(city.Id) }; AddChild(board); _boards[city.Id] = board;
                board.Name = $"City{city.Id}"; board.Visible = city.Id == _focus;
                _landscapes[city.Id] = new VillageLandscape(board, _landscapeAssets);
                _buildingBounds[city.Id] = new Aabb?[9];

            }
            UpdateStockpiles(board, city);
            _landscapes[city.Id].SetPlots(city.Slots);
            string key = string.Join('|', city.Slots.Select(s => $"{s.Type}:{s.Level}:{s.Purchased}"));
            if (_boardKeys.GetValueOrDefault(city.Id) != key)
            {
                Node? old = board.GetNodeOrNull("Buildings"); if (old is not null) { board.RemoveChild(old); old.QueueFree(); }
                var buildings = new Node3D { Name = "Buildings" }; board.AddChild(buildings);
                Array.Clear(_buildingBounds[city.Id]);
                for (int i = 0; i < 9; i++)
                {
                    SlotState slot = city.Slots[i];
                    if (slot.Type == Building.Empty)
                    {
                        if (!slot.Purchased) buildings.AddChild(new Sprite3D { Name = "Locked" + i, Position = SlotPosition(i) + new Vector3(0, .6f, 0), Texture = UiAssets.Icon("Gold"), PixelSize = .025f, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, Shaded = false });
                        continue;
                    }
                    string name = slot.Type switch { Building.Mine or Building.MetalMine => "mine", Building.Stonecutter => "blacksmith", Building.Weaver => "lumbermill", Building.Market => "stage_C", Building.Farm => "windmill", Building.Lumbermill => "lumbermill", Building.ArcheryRange => "archeryrange", Building.Arcanum => "church", Building.ResearchTower => "tower_A", Building.ArrowTower => "tower_B", Building.CatapultTower => "tower_catapult", _ => "barracks" };
                    string path = slot.Type == Building.Market ? "Medieval/building_stage_C.gltf" : $"Medieval/building_{name}_blue.gltf";
                    Node3D model = Model(buildings, path, SlotPosition(i), 1.7f);
                    model.Name = $"Slot{i}";
                    Aabb bounds = LandscapeAssets.Bounds(model);
                    if (slot.Level >= 2)
                    {
                        string prop = slot.Type switch { Building.Farm => "building_grain", Building.Mine => "building_scaffolding", Building.Lumbermill => "resource_lumber", Building.ArcheryRange => "target", Building.Arcanum => "building_stage_C", Building.ResearchTower => "building_tower_base_blue", Building.Barracks => "weaponrack", _ => "building_tower_base_blue" };
                        Vector3 position = SlotPosition(i) + new Vector3(0.75f, 0, 0.6f);
                        float size = 0.7f;
                        if (slot.Type is Building.ArrowTower or Building.CatapultTower or Building.ResearchTower) { position = SlotPosition(i); size = 1.9f; }
                        Node3D addition = Model(buildings, $"Medieval/{prop}.gltf", position, size); addition.Name = "Upgrade" + i;
                        if (slot.Type is Building.ArrowTower or Building.CatapultTower or Building.ResearchTower)
                        {
                            model.Position = new Vector3(model.Position.X, LandscapeAssets.TowerDeck(addition), model.Position.Z);
                            bounds = LandscapeAssets.Bounds(model);
                        }
                        bounds = bounds.Merge(LandscapeAssets.Bounds(addition));
                    }
                    _buildingBounds[city.Id][i] = bounds;
                    Label(buildings, SlotPosition(i) + new Vector3(0, 2.15f, 0), $"{BuildingName(slot.Type)} L{slot.Level}", 20);
                    if (slot.Type == Building.MetalMine) Model(buildings, "Medieval/rock_single_C.gltf", SlotPosition(i) + new Vector3(.7f, 0, -.5f), .55f);
                    if (slot.Type == Building.Farm)
                    {
                        Node3D fan = model.FindChildren("*fan*", "Node3D", true, false).OfType<Node3D>().Single();
                        _windmills.Add((fan, fan.Rotation));
                    }
                    Node3D flag = Box(buildings, SlotPosition(i) + new Vector3(-0.6f, 1.1f, 0), new(.35f, .2f, .03f), city.Id == game.PlayerId ? "487ecc" : "a67152");
                    _flags.Add(flag);
                }
                _boardKeys[city.Id] = key;
            }
        }
    }
    private void UpdateStockpiles(Node3D board, CityState city)
    {
        int gold = PresentationLimits.StockpileCount(city.Gold), food = PresentationLimits.StockpileCount(city.Food), wood = PresentationLimits.StockpileCount(city.Wood);
        string key = $"{gold}:{food}:{wood}";
        if (_stockpileKeys.GetValueOrDefault(city.Id) == key) return;
        Node? old = board.GetNodeOrNull("Stockpiles"); if (old is not null) { board.RemoveChild(old); old.QueueFree(); }
        var piles = new Node3D { Name = "Stockpiles" }; board.AddChild(piles);
        foreach (var (name, count, path, row) in new[] { ("Gold", gold, "Resource/Gold_Bars.gltf", 0), ("Food", food, "Medieval/sack.gltf", 1), ("Wood", wood, "Resource/Wood_Log_Stack.gltf", 2) })
        {
            var resource = new Node3D { Name = name }; piles.AddChild(resource);
            for (int n = 0; n < count; n++) Model(resource, path, VillageLayout.Hex(2, row + 1) + new Vector3((n % 3 - 1) * .48f, 0, (n / 3 - .5f) * .5f), 0.45f);
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
            _combatLayout = game.State is null ? null : new CombatLayout(new HexBoard(game.State.Rules.Combat.Board));
            _units.Clear(); _deathCleanups.Clear(); ClearBars(); _effects.Clear(); _playbackGeneration = _playback.Generation;
        }
        _playback.Advance(delta, game.Connected);
        _windmills.RemoveAll(w => !GodotObject.IsInstanceValid(w.Node));
        _flags.RemoveAll(f => !GodotObject.IsInstanceValid(f));
        foreach (var windmill in _windmills) windmill.Node.Rotation = windmill.Rotation + new Vector3(0, 0, (float)_playback.VisualSeconds * 0.6f);
        foreach (Node3D flag in _flags) flag.Rotation = new(0, (float)Math.Sin(_playback.VisualSeconds * 2 + flag.Position.X) * 0.13f, 0);
        if (_effectFocus != _focus) { _effectFocus = _focus; _effects.Clear(); }
        bool audible = game.Connected && game.State is { Paused: false };
        foreach (Command request in game.DrainActionCues()) _effects.Action(request, _focus, _playback.VisualSeconds, audible);
        _effects.Sample(_playback.VisualSeconds, audible);
        var current = _playback.Units().ToDictionary(u => u.Id);
        if (_combatLayout is null) return;
        foreach (UnitState unit in current.Values.Where(u => u.Deployed)) View(unit).Synchronize(unit);
        foreach (CombatEvent entry in _playback.Drain())
        {
            Vector3? source = entry.Unit is { Deployed: true } actor ? _combatLayout.Position(actor, entry.Tick) + new Vector3(0, .6f, 0)
                : entry.Tower is { Slot: >= 0 } tower && _buildingBounds.GetValueOrDefault(tower.City)?[tower.Slot] is { } towerBounds
                    ? Center(tower.City) + new Vector3(towerBounds.GetCenter().X, towerBounds.End.Y - .2f, towerBounds.GetCenter().Z) : null;
            _effects.Combat(entry, _focus, _playback.VisualSeconds, audible, source, _combatLayout.EventImpact(entry));
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
            view.Sample(current.GetValueOrDefault(id), _playback.Tick, _playback.VisualSeconds, _focus, _combatLayout, current);
        }
        foreach (int id in _strikes.Keys.Where(id => !_units.ContainsKey(id)).ToArray()) { _strikes[id].QueueFree(); _strikes.Remove(id); }
        foreach ((int id, UnitView view) in _units.Where(p => p.Value.State.Class == UnitClass.Melee))
        {
            if (!_strikes.TryGetValue(id, out MeleeStrike? strike)) { strike = new(); AddChild(strike); _strikes.Add(id, strike); }
            strike.Sample(view, _units.GetValueOrDefault(view.State.TargetId), _playback.Tick, _focus, _combatLayout);
        }
    }
    private void ClearBars()
    {
        foreach (UnitHealthBar bar in _healthBars.Values) { _healthRoot.RemoveChild(bar); bar.QueueFree(); }
        _healthBars.Clear();
    }
    private void UpdateHealthBars()
    {
        foreach (int id in _healthBars.Keys.ToArray())
            if (!_units.TryGetValue(id, out UnitView? view) || view.Dead || !view.Visible || !view.State.Deployed)
            { UnitHealthBar bar = _healthBars[id]; _healthRoot.RemoveChild(bar); bar.QueueFree(); _healthBars.Remove(id); }
        _healthRoot.Size = new(_frameSize.X, Math.Max(0, _frameSize.Y - _panel.Size.Y));
        Rect2 world = new(Vector2.Zero, _healthRoot.Size);
        foreach (var (id, view) in _units)
        {
            if (view.Dead || !view.Visible || !view.State.Deployed) continue;
            if (!_healthBars.TryGetValue(id, out UnitHealthBar? bar)) { bar = new UnitHealthBar { Name = "Health" + id }; _healthBars[id] = bar; _healthRoot.AddChild(bar); }
            bar.Sample(view, _camera, world);
        }
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
        if (Math.Abs(direction.Y) < 0.001f) return selected;
        for (int slot = 0; slot < 9; slot++)
        {
            float distance = (SlotPosition(slot).Y - origin.Y) / direction.Y;
            Vector3 point = origin + direction * distance;
            if (distance >= 0 && distance < nearest && VillageLayout.Contains(slot, point)) { selected = slot; nearest = distance; }
        }
        if (selected >= 0) return selected;
        return -1;
    }
    private Rect2 WorldArea() => new(Vector2.Zero, new Vector2(GetViewport().GetVisibleRect().Size.X, _panel.Position.Y));
}
