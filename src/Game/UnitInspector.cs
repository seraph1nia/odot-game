using Game.Core;
using Godot;

namespace Game;

// Static preview and snapshot data. This viewport owns no gameplay actors or effects.
internal sealed partial class UnitInspector : PanelContainer
{
    private readonly Label _name = new(), _description = new(), _stats = new();
    private readonly ProgressBar _health = new() { ShowPercentage = false, Step = 0, CustomMinimumSize = new(0, 12), MouseFilter = MouseFilterEnum.Ignore };
    private readonly SubViewport _preview = new() { Size = new(192, 80), OwnWorld3D = true, TransparentBg = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Once };
    private Node3D? _model;
    private UnitState? _sample;
    private readonly Button _retire = new() { Name = "RetireUnit", Text = "Retire · no refund" }, _store = new() { Name = "StoreUnit", Text = "Send to Town hall" };
    private readonly OptionButton _hall = new() { Name = "StoreDestination" };
    private string _hallKey = "";
    internal int? UnitId { get; private set; }
    internal UnitInspector(IGameSession game, Func<bool> canEdit)
    {
        Name = "UnitInspector"; MouseFilter = MouseFilterEnum.Stop; Visible = false;
        var root = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill }; root.AddThemeConstantOverride("separation", 2); AddChild(root);
        root.AddChild(_name);
        // Actions stay outside the scrolling profile so short supported windows
        // never advertise an enabled button clipped behind the bottom HUD.
        root.AddChild(_hall); root.AddChild(_store); root.AddChild(_retire);
        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        root.AddChild(scroll);
        var box = new VBoxContainer { CustomMinimumSize = new(196, 0), SizeFlagsHorizontal = SizeFlags.ExpandFill }; box.AddThemeConstantOverride("separation", 2); scroll.AddChild(box);
        var container = new SubViewportContainer { CustomMinimumSize = new(192, 80), Stretch = true, MouseFilter = MouseFilterEnum.Ignore }; box.AddChild(container); container.AddChild(_preview);
        var camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = 2.3f, Position = new(2.5f, 1.5f, 4), Current = true }; _preview.AddChild(camera); camera.Transform = camera.Transform.LookingAt(new(0, .65f, 0));
        _preview.AddChild(new DirectionalLight3D { RotationDegrees = new(-45, -30, 0), LightEnergy = 1.2f });
        _preview.AddChild(new WorldEnvironment { Environment = new Godot.Environment { AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = Colors.White, AmbientLightEnergy = .7f } });
        box.AddChild(_description); box.AddChild(_health); box.AddChild(_stats);
        _retire.Pressed += () => { if (canEdit() && _sample is { } unit && unit.Owner == game.PlayerId) game.SendAction("retire", unitId: unit.Id); };
        _store.Pressed += () => { if (canEdit() && _sample is { } unit && unit.Owner == game.PlayerId && _hall.ItemCount > 0) game.SendAction("store", _hall.GetSelectedId(), unitId: unit.Id); };
        foreach (Label label in new[] { _name, _description, _stats })
        {
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart; label.MouseFilter = MouseFilterEnum.Ignore; label.AddThemeFontSizeOverride("font_size", 12);
        }
    }
    internal bool Contains(Vector2 point) => Visible && GetGlobalRect().HasPoint(point);
    internal void Select(UnitState unit)
    {
        Close(); UnitId = unit.Id;
        _model = UnitAssets.Instantiate(UnitAssets.Character(unit.Type, unit.Faction));
        _model.Scale = Vector3.One * .43f; _preview.AddChild(_model);
        var rig = UnitAssets.Bind(_model, unit.Type); UnitAssets.Equip(_model, rig.Skeleton, unit.Type, unit.Faction);
        rig.Player.Play("Idle"); rig.Player.Advance(0); rig.Player.Pause();
        _preview.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
        Sample(unit); Visible = true;
    }
    internal void Sample(UnitState unit, double tick = 0)
    {
        _sample = unit;
        _name.Text = ProgressionPresentation.UnitName(unit); _description.Text = ProgressionPresentation.UnitDescription(unit);
        _health.Value = PresentationLimits.HealthFraction(unit.Health, unit.Profile.Health) * 100;
        _stats.Text = ProgressionPresentation.UnitStats(unit, tick);
        if (unit.Assignment is { } assignment) _stats.Text += $"\nHome tile {assignment.Tile} · anchor {assignment.Anchor}\n{(unit.RecoveryEligible ? "Last completed battle food paid" : "No completed paid recovery receipt")}";
    }
    internal void ConfigureActions(CityState? city, bool editable, ArmyConfiguration? army)
    {
        bool owned = _sample is { Assignment: { Stored: false } } unit && unit.Owner == city?.Id;
        _retire.Visible = _store.Visible = _hall.Visible = owned;
        _retire.Disabled = !editable || !owned;
        _retire.TooltipText = editable ? "Permanently remove this unit. No resources refunded." : "Edit your unready, unpaused city during Building or Preparation.";
        string key = string.Join(';', city?.Army?.Halls.Select(h => $"{h.Slot}:{h.Generation}:{h.CapacityLevel}") ?? []);
        if (key != _hallKey)
        {
            int selected = _hall.ItemCount > 0 ? _hall.GetSelectedId() : -1;
            _hallKey = key; _hall.Clear();
            foreach (TownHallState hall in city?.Army?.Halls ?? []) _hall.AddItem($"Town hall · plot {hall.Slot + 1} · {hall.Capacity} size", hall.Slot);
            int index = _hall.GetItemIndex(selected); if (index >= 0) _hall.Select(index);
        }
        int slot = _hall.ItemCount > 0 ? _hall.GetSelectedId() : -1;
        TownHallState? destination = city?.Army?.Halls.FirstOrDefault(h => h.Slot == slot);
        bool fits = owned && city is not null && destination is not null
            && army?.Find(city.Soldiers, city.Army!.PurchasedHomes, _sample!.Size, destination.Slot, destination.Generation, destination.Capacity) is not null;
        _store.Disabled = !editable || !fits; _hall.Disabled = !editable || destination is null;
        _store.TooltipText = destination is null ? "Build a Town hall on a purchased plot first." : !fits ? "This Town hall has no fitting size capacity." : "Keep identity, level and wounds in reserve. Still consumes battle food.";
    }
    internal void Place(Vector2 viewport, float resourceBottom, float hudTop)
    {
        float available = Math.Max(1, hudTop - resourceBottom - 8);
        Size = new(260, Math.Min(450, available));
        Position = new(viewport.X - 272, resourceBottom + 4);
    }
    internal void Close()
    {
        UnitId = null; _sample = null; Visible = false;
        if (_model is not null) { _preview.RemoveChild(_model); _model.QueueFree(); _model = null; }
    }
    internal void Observe(GameApplication application, Dictionary<string, object?> fields, Dictionary<string, object> targets)
    {
        application.ObserveControl(targets, "UnitInspector", this);
        foreach (Button button in new[] { _retire, _store }) application.ObserveControl(targets, button.Name, button);
        application.ObserveControl(targets, "StoreDestination", _hall);
        fields["InspectedUnit"] = _sample is { } unit ? new { unit.Id, Name = _name.Text, Description = _description.Text, StatsText = _stats.Text, Fraction = _health.Value / 100, unit.Level, unit.Health, MaximumHealth = unit.Profile.Health, Damage = unit.Profile.Damage, unit.Size, unit.IsBoss, unit.Faction, unit.Assignment, unit.RecoveryEligible, Preview = UnitAssets.Character(unit.Type, unit.Faction) } : null;
    }
}
