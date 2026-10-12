using Game.Core;
using Godot;

namespace Game;

public partial class Tabletop
{
    private Label _quoteDetails = null!;
    private Label _inspectionRoster = null!;
    private Label _armyDetails = null!;
    private Control _hudRoot = null!;
    private Action<UiDialog> _ownHudDialog = null!;
    private Func<bool> _hudInputBlocked = null!;
    private Action _hudInvite = null!;

    // Ordinary production receiving seam. Hosts supply existing dialog/modal/
    // invitation ownership; all state, Controls and action bindings stay here.
    internal void InitializeHud(Control root, Action<UiDialog> ownDialog, Func<bool> inputBlocked, Action invite)
    {
        if (_hudRoot is not null) throw new InvalidOperationException("HUD is already initialized.");
        _hudRoot = root; _ownHudDialog = ownDialog; _hudInputBlocked = inputBlocked; _hudInvite = invite;
        _healthRoot = new Control { Name = "UnitHealth", MouseFilter = Control.MouseFilterEnum.Ignore, ClipContents = true };
        root.AddChild(_healthRoot); _healthRoot.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        CreateHud(root);
    }
    private void CreateHud(Control root)
    {
        _resources = (PanelContainer)UiAssets.Component("resource_ledger", root);
        _resources.Name = "ResourceTable";
        _resources.MouseFilter = Control.MouseFilterEnum.Stop;
        _resources.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
        _resources.OffsetLeft = -300; _resources.OffsetRight = -12; _resources.OffsetTop = 12;
        var rows = new Godot.Collections.Array<Godot.Collections.Dictionary>();
        foreach (string name in new[] { "Gold", "Food", "Wood", "Stone", "Metal", "Cloth" })
            rows.Add(new() { ["name"] = name, ["stock"] = "0", ["income"] = "0" });
        _resources.Call("set_data", new Godot.Collections.Dictionary { ["title"] = "Resources", ["rows"] = rows });
        // Receiving-only safe area: the immutable woodland frame has 40px
        // decorated nine-slice corners, but the asset's generic inset is 16px.
        // Keep all ledger text inside the central area without changing artwork
        // or the theme inherited by unrelated panels.
        var frame = (StyleBoxTexture)_resources.GetThemeStylebox("panel").Duplicate();
        frame.ContentMarginLeft = frame.TextureMarginLeft;
        frame.ContentMarginTop = frame.TextureMarginTop;
        frame.ContentMarginRight = frame.TextureMarginRight;
        frame.ContentMarginBottom = frame.TextureMarginBottom;
        _resources.AddThemeStyleboxOverride("panel", frame);
        // Wrapped context can change its minimum after the deferred container
        // sort. Follow that native change rather than retaining a stale height.
        _resources.MinimumSizeChanged += () => Callable.From(ReflowResources).CallDeferred();
        var stack = _resources.GetNode<VBoxContainer>("Body");
        stack.AddThemeConstantOverride("separation", 10);
        var table = stack.GetChildren().OfType<GridContainer>().Single();
        table.AddThemeConstantOverride("h_separation", 16);
        table.AddThemeConstantOverride("v_separation", 4);
        table.GetChild<Label>(1).HorizontalAlignment = HorizontalAlignment.Right;
        table.GetChild<Label>(2).HorizontalAlignment = HorizontalAlignment.Right;
        _incomeHeading = table.GetChild<Label>(2);
        var labels = _resources.Get("value_labels").AsGodotDictionary();
        foreach (string name in new[] { "Gold", "Food", "Wood", "Stone", "Metal", "Cloth" })
        {
            var values = labels[name].AsGodotArray();
            _values[name] = values[0].As<Label>(); _incomes[name] = values[1].As<Label>();
        }
        _incomeContext = Text(stack, "", 11);
        Text(stack, "Upkeep", 12);
        var upkeep = new GridContainer { Name = "UpkeepTable", Columns = 2 };
        upkeep.AddThemeConstantOverride("h_separation", 16);
        upkeep.AddThemeConstantOverride("v_separation", 4); stack.AddChild(upkeep);
        _compactUpkeep = upkeep;
        _upkeepLabel = Text(upkeep, "Next battle", 12); _upkeepValue = Text(upkeep, "0 food", 12);
        _balanceLabel = Text(upkeep, "Food after payment", 12); _balanceValue = Text(upkeep, "0", 12);
        _upkeepValue.HorizontalAlignment = _balanceValue.HorizontalAlignment = HorizontalAlignment.Right;
        _upkeepValue.SizeFlagsHorizontal = _balanceValue.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        foreach (Label label in new[] { _upkeepLabel, _upkeepValue, _balanceLabel, _balanceValue }) label.AutowrapMode = TextServer.AutowrapMode.Off;
        _inspector = new UnitInspector(game, CanEdit); root.AddChild(_inspector);
        _homeHealth = new HomeHealthBar(); _healthRoot.AddChild(_homeHealth);
        _panel = new PanelContainer { Name = "BottomPanel", CustomMinimumSize = new(0, 180), MouseFilter = Control.MouseFilterEnum.Stop, GrowVertical = Control.GrowDirection.Begin };
        root.AddChild(_panel); _panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide); _panel.OffsetTop = -180;
        var columns = new HBoxContainer(); columns.AddThemeConstantOverride("separation", 12); _panel.AddChild(columns);
        var city = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsStretchRatio = .18f }; columns.AddChild(city);
        var selector = new HBoxContainer(); city.AddChild(selector);
        _previousCity = Button(selector, "<", () => CycleCity(-1)); _previousCity.Name = "PreviousCity";
        _cityName = Text(selector, "[YOU]", 12); _cityName.HorizontalAlignment = HorizontalAlignment.Center;
        _nextCity = Button(selector, ">", () => CycleCity(1)); _nextCity.Name = "NextCity";
        _detailsButton = Button(city, "Details", () => _detailsDialog.PopupCenteredClamped(new(650, 450), .9f)); _detailsButton.Name = "Details";
        _detailsButton.TooltipText = "Space: overview · Scroll: zoom · WASD / arrows or left drag: pan";
        _stats = Text(city, "", 12); _stats.Visible = false;
        _status = Text(city, "", 12); _status.Name = "SessionStatus";
        _feedback = Text(city, "", 12); _feedback.Name = "SessionFeedback";
        _roster = new HBoxContainer(); city.AddChild(_roster);
        _rosterDetails = Text(_roster, "", 11);
        _detailsDialog = new UiDialog { Title = "City details", Theme = root.Theme };
        _ownHudDialog(_detailsDialog);
        var details = _detailsDialog.Content;
        _inspectionRoster = Text(details, "", 13);
        _quoteDetails = Text(details, "", 13);
        _upkeep = Text(details, "", 14); _reward = Text(details, "", 14); _armyDetails = Text(details, "", 13);
        _detailsDialog.VisibilityChanged += () => { if (!_detailsDialog.Visible && _detailsButton.IsVisibleInTree()) _detailsButton.GrabFocus(); };
        var context = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsStretchRatio = 0.6f }; columns.AddChild(context);
        _detail = Text(context, "", 12); _detail.Name = "SelectedPlot"; _detail.CustomMinimumSize = Vector2.Zero;
        _buyPlot = Button(context, "Buy plot", () => ContextAction("buy-plot")); _buyPlot.Name = "BuyPlot";
        _buildGroups = new HBoxContainer(); context.AddChild(_buildGroups);
        foreach (string group in new[] { "Production", "Army", "Defense", "Trade" })
        {
            string selected = group;
            Button tab = Button(_buildGroups, group, () => { _constructionGroup = selected; _uiKey = ""; });
            tab.Name = group + "Choices"; tab.AddThemeFontSizeOverride("font_size", 12); _groupButtons.Add(tab);
        }
        _buildActions = new GridContainer { Columns = 3 }; context.AddChild(_buildActions);
        foreach (BuildingDefinition definition in Catalogs.Buildings(new()))
        {
            Building type = definition.Type;
            _construction[type] = Button(_buildActions, BuildingName(type), () => ContextAction("build", type));
            _construction[type].Name = type.ToString();
        }
        for (int i = 0; i < 9; i++) { var empty = new Control { CustomMinimumSize = new(0, 32), MouseFilter = Control.MouseFilterEnum.Ignore }; _emptyBuildCells.Add(empty); _buildActions.AddChild(empty); }
        _recovery = Button(context, BuildingName(Building.Lumbermill) + " · gold recovery", () => ContextAction("build", Building.Lumbermill, payment: ConstructionPayment.GoldRecovery));
        _recovery.Name = "LumbermillRecovery";
        _mine = _construction[Building.Mine]; _farm = _construction[Building.Farm]; _barracks = _construction[Building.Barracks];
        _buildingActions = new HBoxContainer(); context.AddChild(_buildingActions);
        _upgrade = Button(_buildingActions, "Upgrade", () => ContextAction("upgrade"));
        _sell = Button(_buildingActions, "Sell", () => ContextAction("sell")); _sell.Name = "Sell";
        _marketActions = new HBoxContainer(); context.AddChild(_marketActions);
        foreach (Game.Core.Resource resource in Enum.GetValues<Game.Core.Resource>().Where(r => r != Game.Core.Resource.Gold))
        {
            Game.Core.Resource selected = resource;
            Button trade = Button(_marketActions, resource.ToString(), () => { if (_slot >= 0 && CanEdit()) game.SendAction("trade", _slot, resource: selected, bundles: 1); });
            trade.Name = "Trade" + resource; _trades[resource] = trade;
        }
        var recruitment = new HBoxContainer(); context.AddChild(recruitment);
        foreach (UnitType type in Enum.GetValues<UnitType>())
        {
            UnitType role = type;
            _recruitment[type] = Button(recruitment, AssetCatalog.UnitName(type), () => ContextAction("recruit", soldierType: role));
            _recruitment[type].Name = type switch { UnitType.Swordsman => "Recruit", UnitType.Crossbowman => "RecruitRanged", _ => "Recruit" + type };
        }
        _recruit = _recruitment[UnitType.Swordsman]; _ranged = _recruitment[UnitType.Crossbowman];
        CreateResearch(city, root);
        CreateArmyControls(city, context, details);
        CreateSquadControls(city);
        var match = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsStretchRatio = .22f }; columns.AddChild(match);
        _phase = Text(match, "Lobby", 12);
        _counters = Text(match, "", 11);
        var phases = new VBoxContainer(); phases.AddThemeConstantOverride("separation", 0); match.AddChild(phases);
        for (int i = 0; i < 5; i++) _phaseRows.Add(Text(phases, "", 12));
        _start = Button(match, "Start match", () => game.SendAction("start"));
        var controls = new HBoxContainer(); match.AddChild(controls);
        _ready = Button(controls, "Ready", () => game.SendAction(Me()?.Ready == true ? "unready" : "ready"));
        _pause = Button(controls, "Pause", () => game.SendAction(game.State?.Paused == true ? "resume" : "pause"));
        _reconnect = Button(match, "Reconnect to my city", () => game.Connect());
        _fresh = Button(match, "Join lobby · fresh session", () => game.Connect(true));
        _invite = Button(match, "Invite friends", _hudInvite); _invite.Name = "Invite";
        foreach (var (button, name) in new[] { (_mine, "Mine"), (_farm, "Farm"), (_barracks, "Barracks"), (_upgrade, "Upgrade"), (_recruit, "Recruit"), (_ready, "Ready"), (_pause, "Pause"), (_start, "Start"), (_reconnect, "Reconnect"), (_fresh, "Fresh") }) button.Name = name;
        foreach (Button button in _panel.FindChildren("*", "Button", true, false).OfType<Button>())
        {
            button.AddThemeFontSizeOverride("font_size", 11);
            foreach (string state in ApplicationTheme.ButtonStates)
            {
                var style = (StyleBox)button.GetThemeStylebox(state).Duplicate();
                style.ContentMarginTop = 2; style.ContentMarginBottom = 2;
                button.AddThemeStyleboxOverride(state, style);
            }
        }
    }
    private void ReflowResources()
    {
        _resources.ResetSize();
        _resources.OffsetLeft = -12 - _resources.Size.X;
        _resources.OffsetRight = -12;
    }
    private void CycleCity(int direction)
    {
        int[] ids = game.State?.Players.Select(p => p.Id).Order().ToArray() ?? [];
        if (ids.Length < 2) return;
        CancelGesture(); _inspector.Close();
        _focus = ids[(Array.IndexOf(ids, _focus) + direction + ids.Length) % ids.Length];
        _slot = _hover = -1; _uiKey = ""; _navigation?.Reset();
        RefreshHud(_focus, _slot);
    }
}
