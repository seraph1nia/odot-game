using Game.Core;
using Godot;

namespace Game;

public partial class Tabletop
{
    private Label _quoteDetails = null!;
    private Label _inspectionRoster = null!;
    private Label _armyDetails = null!;
    private void CreateHud(Control root)
    {
        _resources = new PanelContainer { Name = "ResourceTable", MouseFilter = Control.MouseFilterEnum.Stop };
        root.AddChild(_resources);
        _resources.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
        _resources.OffsetLeft = -272; _resources.OffsetRight = -12; _resources.OffsetTop = 12;
        var stack = new VBoxContainer(); stack.AddThemeConstantOverride("separation", 5); _resources.AddChild(stack);
        var table = new GridContainer { Columns = 3 }; table.AddThemeConstantOverride("v_separation", 3); stack.AddChild(table);
        Text(table, "Resource", 12); Text(table, "Stock", 12).HorizontalAlignment = HorizontalAlignment.Right;
        _incomeHeading = Text(table, "Income/turn", 11); _incomeHeading.HorizontalAlignment = HorizontalAlignment.Right;
        foreach (Game.Core.Resource resource in new[] { Game.Core.Resource.Gold, Game.Core.Resource.Food, Game.Core.Resource.Wood, Game.Core.Resource.Stone, Game.Core.Resource.Metal, Game.Core.Resource.Cloth })
        {
            Text(table, resource.ToString(), 13);
            Label amount = Text(table, "0", 13); amount.HorizontalAlignment = HorizontalAlignment.Right;
            _values[resource.ToString()] = amount;
            Label income = Text(table, "0", 13); income.HorizontalAlignment = HorizontalAlignment.Right;
            _incomes[resource.ToString()] = income;
        }
        _incomeContext = Text(stack, "", 11);
        Text(stack, "Upkeep", 12);
        var upkeep = new GridContainer { Name = "UpkeepTable", Columns = 2 }; upkeep.AddThemeConstantOverride("v_separation", 2); stack.AddChild(upkeep);
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
        _status = Text(city, "", 12); _feedback = Text(city, "", 12);
        _roster = new HBoxContainer(); city.AddChild(_roster);
        _rosterDetails = Text(_roster, "", 11);
        _detailsDialog = new AcceptDialog { Title = "City details", Theme = application.Theme, Transient = true, Exclusive = false };
        AddChild(_detailsDialog);
        var scroll = new ScrollContainer { CustomMinimumSize = new(600, 350) }; _detailsDialog.AddChild(scroll);
        var details = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; scroll.AddChild(details);
        _inspectionRoster = Text(details, "", 13);
        _quoteDetails = Text(details, "", 13);
        _upkeep = Text(details, "", 14); _reward = Text(details, "", 14); _armyDetails = Text(details, "", 13);
        _detailsDialog.VisibilityChanged += () => { if (!_detailsDialog.Visible) _detailsButton.GrabFocus(); };
        var context = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsStretchRatio = 0.6f }; columns.AddChild(context);
        _detail = Text(context, "", 12); _detail.CustomMinimumSize = Vector2.Zero;
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
            _construction[type] = Button(_buildActions, type.ToString(), () => ContextAction("build", type));
            _construction[type].Name = type.ToString();
        }
        for (int i = 0; i < 9; i++) { var empty = new Control { CustomMinimumSize = new(0, 32), MouseFilter = Control.MouseFilterEnum.Ignore }; _emptyBuildCells.Add(empty); _buildActions.AddChild(empty); }
        _recovery = Button(context, "Lumbermill · gold recovery", () => ContextAction("build", Building.Lumbermill, payment: ConstructionPayment.GoldRecovery));
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
            _recruitment[type] = Button(recruitment, type.ToString(), () => ContextAction("recruit", soldierType: role));
            _recruitment[type].Name = type switch { UnitType.Swordsman => "Recruit", UnitType.Crossbowman => "RecruitRanged", _ => "Recruit" + type };
        }
        _recruit = _recruitment[UnitType.Swordsman]; _ranged = _recruitment[UnitType.Crossbowman];
        CreateResearch(city);
        CreateArmyControls(city, context, details);
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
        _invite = Button(match, "Invite friends", application.RequestInvite); _invite.Name = "Invite";
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
    private void CycleCity(int direction)
    {
        int[] ids = game.State?.Players.Select(p => p.Id).Order().ToArray() ?? [];
        if (ids.Length < 2) return;
        CancelGesture(); _inspector.Close();
        _focus = ids[(Array.IndexOf(ids, _focus) + direction + ids.Length) % ids.Length];
        _slot = _hover = -1; _uiKey = ""; _navigation.Reset();
    }
}
