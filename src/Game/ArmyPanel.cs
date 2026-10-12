using Game.Core;
using Godot;

namespace Game;

public partial class Tabletop
{
    private Button _openHall = null!, _hallSend = null!, _hallRetire = null!, _hallCapacity = null!, _hallHealing = null!;
    private UiDialog _hallDialog = null!;
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
        _homeSummary = Text(details, "", 13);
        _openHall = Button(context, "Town hall roster", () =>
        {
            if (Focus()?.Slots.ElementAtOrDefault(_slot) is not { Type: Building.TownHall } hall) return;
            _hallSlot = _slot; _hallGeneration = hall.Generation; _storedUnit = 0; _hallKey = "";
            RefreshArmy(); _hallDialog.PopupCenteredClamped(new(700, 500), .9f);
        }); _openHall.Name = "OpenTownHall";
        _hallDialog = new UiDialog { Title = "Town hall", Theme = _hudRoot.Theme }; _ownHudDialog(_hallDialog);
        var body = _hallDialog.Content;
        _hallSummary = Text(body, "", 13);
        var upgrades = new HBoxContainer(); body.AddChild(upgrades);
        _hallCapacity = HallButton(upgrades, "Upgrade storage", () => HallAction("upgrade-capacity")); _hallCapacity.Name = "UpgradeHallCapacity";
        _hallHealing = HallButton(upgrades, "Upgrade healing", () => HallAction("upgrade-healing")); _hallHealing.Name = "UpgradeHallHealing";
        _hallRoster = new VBoxContainer(); body.AddChild(_hallRoster);
        _hallSelection = Text(body, "Select a stored unit", 13);
        var actions = new HBoxContainer(); body.AddChild(actions);
        _hallSend = HallButton(actions, "Send to battlefield", () => HallAction("send", _storedUnit)); _hallSend.Name = "HallSend";
        _hallRetire = HallButton(actions, "Retire · no refund", () => HallAction("retire", _storedUnit)); _hallRetire.Name = "HallRetire";
        _hallDialog.VisibilityChanged += () => { if (!_hallDialog.Visible) { _storedUnit = 0; if (_openHall.IsVisibleInTree()) _openHall.GrabFocus(); } };
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
        _homeSummary.Text = army is null ? "" : "Army plots (first-fit order):\n" + string.Join("\n", army.Homes.Select(h => $"{h.Number}. Tile {h.Cell} · {h.State} · {h.Used}/6 size"));
        _openHall.Visible = SelectedPlot(city)?.Type == Building.TownHall;
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
                    Button select = HallButton(_hallRoster, $"#{id} {ProgressionPresentation.UnitName(unit)} L{unit.Level} · size {unit.Size} · HP {HealthPoints.Format(unit.Health)}/{HealthPoints.Format(unit.Profile.Health)} · {(unit.RecoveryEligible ? "paid recovery eligible" : "no paid recovery")}", () => { _storedUnit = id; RefreshArmy(); });
                    select.Name = "HallUnit" + id; select.AddThemeFontSizeOverride("font_size", 12); _hallUnits.Add(select);
                }
                if (used < 6) Text(_hallRoster, $"Empty reserve space · {6 - used} size points", 12);
            }
        }
        UnitState? chosen = stored.FirstOrDefault(u => u.Id == _storedUnit);
        foreach (Button button in _hallUnits) button.Modulate = button.Name == "HallUnit" + _storedUnit ? Colors.White : new Color(.85f, .85f, .85f);
        _hallSelection.Text = chosen is null ? "Select a stored unit. Selection changes nothing." : $"Selected #{chosen.Id} · wounds and level are retained when sent.";
        bool fits = chosen is not null && config!.Find(city.Soldiers, army.PurchasedHomes, chosen.Size, plots: army.Homes.Select(h => h.Plot).ToArray()) is not null;
        _hallSend.Disabled = !CanEdit() || chosen is null || !fits; _hallRetire.Disabled = !CanEdit() || chosen is null;
        _hallSend.TooltipText = chosen is null ? "Select a stored unit." : !fits ? "Battlefield homes are full. Buy a space or retire/store a field unit first." : "Insert at the first fitting purchased tile and free anchor; no healing or payment.";
    }
    private void UpdateArmyMarkers(Node3D board, CityState city, MatchSnapshot state)
    {
        if (city.Army is not { } army) return;
        string key = string.Join('|', army.Homes.Select(h => $"{h.PlotId}:{h.Cell}:{h.Purchased}:{h.Used}:{h.Plot.Type}:{h.Plot.Level}:{h.Plot.Generation}"));
        Node3D? old = board.GetNodeOrNull<Node3D>("ArmyHomes");
        if (old is not null && old.GetMeta("key").AsString() == key) return;
        if (old is not null) { board.RemoveChild(old); old.QueueFree(); }
        var markers = new ArmyPlotView(city, state, _landscapeAssets); markers.SetMeta("key", key); board.AddChild(markers);
        ReceiveArmyPlotBounds(city.Id, markers);
    }
    internal void ReceiveArmyPlotBounds(int city, ArmyPlotView view)
    {
        if (!_buildingBounds.TryGetValue(city, out Aabb?[]? bounds)) _buildingBounds[city] = bounds = new Aabb?[ArmyConfiguration.TotalPlots];
        for (int index = 0; index < ArmyConfiguration.PlotCount; index++)
            bounds[ArmyConfiguration.PlotOffset + index] = view.Bounds[index];
    }
    private void ObserveArmy(Dictionary<string, object?> fields, Dictionary<string, object> targets)
    {
        foreach (Button button in new[] { _openHall, _hallCapacity, _hallHealing, _hallSend, _hallRetire }.Concat(_hallUnits)) Application.ObserveControl(targets, button.Name, button);
        Application.ObserveControl(targets, "CloseTownHall", _hallDialog.GetOkButton());
        fields["TownHallOpen"] = _hallDialog.Visible; fields["TownHallText"] = _hallSummary.Text;
        fields["SelectedStoredUnit"] = _storedUnit; fields["ArmyHomes"] = Focus()?.Army; fields["HomeText"] = _homeSummary.Text;
        fields["HomeMarkers"] = _boards.GetValueOrDefault(_focus)?.GetNodeOrNull<Node3D>("ArmyHomes")?.GetChildren().Select(n => n.Name.ToString()).ToArray() ?? [];
    }
}
