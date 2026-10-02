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
    internal int? UnitId { get; private set; }
    internal UnitInspector()
    {
        Name = "UnitInspector"; MouseFilter = MouseFilterEnum.Stop; Visible = false;
        var box = new VBoxContainer(); box.AddThemeConstantOverride("separation", 2); AddChild(box);
        box.AddChild(_name);
        var container = new SubViewportContainer { CustomMinimumSize = new(192, 80), Stretch = true, MouseFilter = MouseFilterEnum.Ignore }; box.AddChild(container); container.AddChild(_preview);
        var camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = 2.3f, Position = new(2.5f, 1.5f, 4), Current = true }; _preview.AddChild(camera); camera.Transform = camera.Transform.LookingAt(new(0, .65f, 0));
        _preview.AddChild(new DirectionalLight3D { RotationDegrees = new(-45, -30, 0), LightEnergy = 1.2f });
        _preview.AddChild(new WorldEnvironment { Environment = new Godot.Environment { AmbientLightSource = Godot.Environment.AmbientSource.Color, AmbientLightColor = Colors.White, AmbientLightEnergy = .7f } });
        box.AddChild(_description); box.AddChild(_health); box.AddChild(_stats);
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
    internal void Sample(UnitState unit)
    {
        _sample = unit;
        _name.Text = ProgressionPresentation.UnitName(unit); _description.Text = ProgressionPresentation.UnitDescription(unit);
        _health.Value = PresentationLimits.HealthFraction(unit.Health, unit.Profile.Health) * 100;
        _stats.Text = ProgressionPresentation.UnitStats(unit);
    }
    internal void Place(Vector2 viewport, float resourceBottom, float hudTop)
    {
        Size = new(220, 0);
        Position = new(viewport.X - 232, resourceBottom + Math.Max(4, (hudTop - resourceBottom - GetCombinedMinimumSize().Y) / 2));
    }
    internal void Close()
    {
        UnitId = null; _sample = null; Visible = false;
        if (_model is not null) { _preview.RemoveChild(_model); _model.QueueFree(); _model = null; }
    }
    internal void Observe(GameApplication application, Dictionary<string, object?> fields, Dictionary<string, object> targets)
    {
        application.ObserveControl(targets, "UnitInspector", this);
        fields["InspectedUnit"] = _sample is { } unit ? new { unit.Id, Name = _name.Text, Description = _description.Text, StatsText = _stats.Text, Fraction = _health.Value / 100, unit.Level, unit.Health, MaximumHealth = unit.Profile.Health, Damage = unit.Profile.Damage, unit.Size, unit.IsBoss, unit.Faction, Preview = UnitAssets.Character(unit.Type, unit.Faction) } : null;
    }
}
