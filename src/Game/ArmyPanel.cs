using Game.Core;
using Godot;

namespace Game;

public partial class Tabletop
{
    private Button _buyHome = null!, _openHall = null!, _hallSend = null!, _hallRetire = null!, _hallCapacity = null!, _hallHealing = null!;
    private AcceptDialog _hallDialog = null!;
    private Label _hallSummary = null!, _hallSelection = null!, _homeSummary = null!;
    private VBoxContainer _hallRoster = null!;
    private readonly List<Button> _hallUnits = [];
    private int _hallSlot = -1, _storedUnit;
    private long _hallGeneration;
    private string _hallKey = "";
    private ArmyConfiguration? _armyConfiguration;
    private CombatFingerprint _armyFingerprint;
    private ArmyConfiguration? UiArmy()
    {
        if (game.State is not { } state) return null;
        if (_armyConfiguration is null || _armyFingerprint != state.ConfigurationFingerprint)
        {
            _armyConfiguration = new(state.Rules.Army, new(state.Rules.Combat.Board)); _armyFingerprint = state.ConfigurationFingerprint;
        }
        return _armyConfiguration;
    }
    private void CreateArmyControls(Node city, Node context, Node details)
    {
        _buyHome = Button(city, "Buy unit space", () => { if (CanEdit()) game.SendAction("buy-home"); }); _buyHome.Name = "BuyHome";
        _homeSummary = Text(details, "", 13);
        _openHall = Button(context, "Town hall roster", () =>
        {
            if (Focus()?.Slots.ElementAtOrDefault(_slot) is not { Type: Building.TownHall } hall) return;
            _hallSlot = _slot; _hallGeneration = hall.Generation; _storedUnit = 0; _hallKey = "";
            RefreshArmy(); _hallDialog.PopupCenteredClamped(new(700, 500), .9f);
        }); _openHall.Name = "OpenTownHall";
        _hallDialog = new AcceptDialog { Title = "Town hall", Theme = application.Theme, Transient = true, Exclusive = false }; AddChild(_hallDialog);
        var scroll = new ScrollContainer { CustomMinimumSize = new(630, 370) }; _hallDialog.AddChild(scroll);
        var body = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; scroll.AddChild(body);
        _hallSummary = Text(body, "", 13);
        var upgrades = new HBoxContainer(); body.AddChild(upgrades);
        _hallCapacity = HallButton(upgrades, "Upgrade storage", () => HallAction("upgrade-capacity")); _hallCapacity.Name = "UpgradeHallCapacity";
        _hallHealing = HallButton(upgrades, "Upgrade healing", () => HallAction("upgrade-healing")); _hallHealing.Name = "UpgradeHallHealing";
        _hallRoster = new VBoxContainer(); body.AddChild(_hallRoster);
        _hallSelection = Text(body, "Select a stored unit", 13);
        var actions = new HBoxContainer(); body.AddChild(actions);
        _hallSend = HallButton(actions, "Send to battlefield", () => HallAction("send", _storedUnit)); _hallSend.Name = "HallSend";
        _hallRetire = HallButton(actions, "Retire · no refund", () => HallAction("retire", _storedUnit)); _hallRetire.Name = "HallRetire";
        _hallDialog.VisibilityChanged += () => { if (!_hallDialog.Visible) { _storedUnit = 0; _openHall.GrabFocus(); } };
    }
    private static Button HallButton(Node parent, string text, Action action)
    {
        var button = new Button { Text = text, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; parent.AddChild(button);
        button.Pressed += action; return button;
    }
    private void HallAction(string action, int unit = 0)
    {
        if (!CanEdit() || Focus()?.Slots.ElementAtOrDefault(_hallSlot) is not { Type: Building.TownHall } hall || hall.Generation != _hallGeneration) return;
        game.SendAction(action, _hallSlot, unitId: unit);
    }
    private void RefreshArmy()
    {
        CityState? city = Focus(); ArmyState? army = city?.Army; ArmyConfiguration? config = UiArmy();
        ResourceCost? homePrice = army is null ? null : config?.HomeQuote(army.PurchasedHomes);
        _buyHome.Visible = army is not null && game.State?.Phase != Phase.Lobby;
        _buyHome.Text = homePrice is { } price ? $"Buy unit space · {price.Gold} gold" : "All six unit spaces purchased";
        _buyHome.Disabled = !CanEdit() || homePrice is not { } cost || !city!.Resources.TryPay(cost, out _);
        _buyHome.TooltipText = !CanEdit() ? "Edit your unready, unpaused city during Building or Preparation." : "Buy the next physical six-size home. Town hall upgrades never buy battlefield space.";
        _homeSummary.Text = army is null ? "" : "Battlefield homes (first-fit order):\n" + string.Join("\n", army.Homes.Select((h, i) => $"{i + 1}. Tile {h.Cell} · {(h.Purchased ? $"{h.Used}/6 size" : "locked")}"));
        _openHall.Visible = city?.Slots.ElementAtOrDefault(_slot)?.Type == Building.TownHall;
        _openHall.Disabled = !game.Connected;
        if (_hallSlot < 0 || city?.Slots.ElementAtOrDefault(_hallSlot) is not { Type: Building.TownHall } selected || selected.Generation != _hallGeneration)
        {
            if (_hallDialog.Visible) _hallDialog.Hide(); return;
        }
        TownHallState hall = army!.Halls.Single(h => h.Slot == _hallSlot);
        UnitState[] stored = city.Soldiers.Where(u => u.Assignment is { Stored: true } a && a.HallSlot == _hallSlot && a.HallGeneration == _hallGeneration).OrderBy(u => u.Assignment!.Tile).ThenBy(u => u.Assignment!.Anchor).ToArray();
        _hallSummary.Text = $"Town hall · plot {_hallSlot + 1}\nStorage L{hall.CapacityLevel}/3 · {stored.Sum(u => u.Size)}/{hall.Capacity} size points\nHealing L{hall.HealingLevel}/3 · {hall.HealingPercent}% current max HP per actual production (rounded up).\nStored units pay battle food but never fight. Recovery requires food paid in the last completed battle.";
        void Upgrade(Button button, ResourceCost? quote, string track, int level)
        {
            button.Text = quote is { } q ? $"{track} L{level + 1} · {ResourceText(q)}" : $"{track} complete";
            button.Disabled = !CanEdit() || quote is not { } price || !city.Resources.TryPay(price, out _);
            button.TooltipText = quote is { } next ? CostExplanation(next, city.Resources) : "Maximum track level.";
        }
        Upgrade(_hallCapacity, hall.CapacityQuote, "Storage", hall.CapacityLevel); Upgrade(_hallHealing, hall.HealingQuote, "Healing", hall.HealingLevel);
        string key = $"{_focus}:{_hallSlot}:{_hallGeneration}:{hall.Capacity}:" + string.Join(';', stored.Select(u => $"{u.Id}:{u.Health}:{u.Profile.Health}:{u.Level}:{u.Assignment}:{u.RecoveryEligible}"));
        if (key != _hallKey)
        {
            _hallKey = key; _hallUnits.Clear();
            foreach (Node child in _hallRoster.GetChildren()) { _hallRoster.RemoveChild(child); child.QueueFree(); }
            for (int tile = 0; tile < hall.Capacity / 6; tile++)
            {
                UnitState[] claims = stored.Where(u => u.Assignment!.Tile == tile).ToArray(); int used = claims.Sum(u => u.Size);
                Text(_hallRoster, $"Reserve tile {tile + 1} · {used}/6 size · {6 - used} free", 12);
                foreach (UnitState unit in claims)
                {
                    int id = unit.Id;
                    Button select = HallButton(_hallRoster, $"#{id} {unit.Type} L{unit.Level} · size {unit.Size} · HP {HealthPoints.Format(unit.Health)}/{HealthPoints.Format(unit.Profile.Health)} · {(unit.RecoveryEligible ? "paid recovery eligible" : "no paid recovery")}", () => { _storedUnit = id; RefreshArmy(); });
                    select.Name = "HallUnit" + id; select.AddThemeFontSizeOverride("font_size", 12); _hallUnits.Add(select);
                }
                if (used < 6) Text(_hallRoster, $"Empty reserve space · {6 - used} size points", 12);
            }
        }
        UnitState? chosen = stored.FirstOrDefault(u => u.Id == _storedUnit);
        foreach (Button button in _hallUnits) button.Modulate = button.Name == "HallUnit" + _storedUnit ? Colors.White : new Color(.85f, .85f, .85f);
        _hallSelection.Text = chosen is null ? "Select a stored unit. Selection changes nothing." : $"Selected #{chosen.Id} · wounds and level are retained when sent.";
        bool fits = chosen is not null && config!.Find(city.Soldiers, army.PurchasedHomes, chosen.Size) is not null;
        _hallSend.Disabled = !CanEdit() || chosen is null || !fits; _hallRetire.Disabled = !CanEdit() || chosen is null;
        _hallSend.TooltipText = chosen is null ? "Select a stored unit." : !fits ? "Battlefield homes are full. Buy a space or retire/store a field unit first." : "Insert at the first fitting purchased tile and free anchor; no healing or payment.";
    }
    private static void UpdateArmyMarkers(Node3D board, CityState city, MatchSnapshot state)
    {
        if (city.Army is not { } army) return;
        Node3D? old = board.GetNodeOrNull<Node3D>("ArmyHomes");
        if (old is not null && old.GetMeta("purchased").AsInt32() == army.PurchasedHomes)
        { old.Visible = state.Phase is Phase.Building or Phase.Preparation; return; }
        if (old is not null) { board.RemoveChild(old); old.QueueFree(); }
        var markers = new Node3D { Name = "ArmyHomes", Visible = state.Phase is Phase.Building or Phase.Preparation };
        markers.SetMeta("purchased", army.PurchasedHomes); board.AddChild(markers);
        foreach ((ArmyHomeState home, int index) in army.Homes.Select((h, i) => (h, i)))
        {
            HexCoordinate coordinate = state.Rules.Combat.Board.Cells.Single(c => c.Id == home.Cell).Coordinate;
            Vector3 position = VillageLayout.Hex(coordinate.Column, coordinate.R);
            Label3D label = Label(markers, position + new Vector3(0, .12f, .95f), $"Space {index + 1} · {(home.Purchased ? "6 size" : "locked")}", 18);
            label.Name = "Home" + index; label.Modulate = home.Purchased ? new Color("c7e6c5") : new Color("b9b5aa");
            if (!home.Purchased) markers.AddChild(new Sprite3D { Name = "LockedHome" + index, Position = position + new Vector3(0, .35f, 0), Texture = UiAssets.Icon("Gold"), PixelSize = .014f, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, Shaded = false });
        }
    }
    private void ObserveArmy(Dictionary<string, object?> fields, Dictionary<string, object> targets)
    {
        foreach (Button button in new[] { _buyHome, _openHall, _hallCapacity, _hallHealing, _hallSend, _hallRetire }.Concat(_hallUnits)) application.ObserveControl(targets, button.Name, button);
        application.ObserveControl(targets, "CloseTownHall", _hallDialog.GetOkButton());
        fields["TownHallOpen"] = _hallDialog.Visible; fields["TownHallText"] = _hallSummary.Text;
        fields["SelectedStoredUnit"] = _storedUnit; fields["ArmyHomes"] = Focus()?.Army; fields["HomeText"] = _homeSummary.Text;
        fields["HomeMarkers"] = _boards.GetValueOrDefault(_focus)?.GetNodeOrNull<Node3D>("ArmyHomes")?.GetChildren().Select(n => n.Name.ToString()).ToArray() ?? [];
    }
}
