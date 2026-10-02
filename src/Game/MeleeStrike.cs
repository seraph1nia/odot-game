using Game.Core;
using Godot;

namespace Game;

// A current-action tabletop cue. It never applies damage, moves a model or
// consumes random choices. Baselines restore its progress without new sounds.
internal sealed partial class MeleeStrike : Node3D
{
    public const int TrailTicks = 12;
    private readonly MeshInstance3D _guide = new() { Mesh = new BoxMesh(), CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
    private readonly MeshInstance3D _connection = new() { Mesh = new BoxMesh(), CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
    private readonly MeshInstance3D _impact = new() { Mesh = new SphereMesh { Radius = .09f, Height = .18f } };
    private readonly StandardMaterial3D _material = new() { ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, NoDepthTest = false };
    private readonly StandardMaterial3D _guideMaterial = new() { AlbedoColor = new("24414a"), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
    private UnitState? _actor;
    private Vector3 _source, _target;
    public override void _Ready()
    { AddChild(_guide); _guide.MaterialOverride = _guideMaterial; AddChild(_connection); AddChild(_impact); _connection.MaterialOverride = _material; _impact.MaterialOverride = _material; }
    public void Sample(UnitView attacker, UnitView? target, double tick, int focus, CombatLayout layout)
    {
        _actor = attacker.State; tick = _actor.Hex?.FrozenTick ?? tick;
        Visible = attacker.Visible && !attacker.Dead && _actor.Destination == focus && _actor.TargetId != 0 && !_actor.TargetCity
            && _actor.AttackSequence > 0 && _actor.Hex?.Action is UnitActionKind.Windup or UnitActionKind.Recovery
            && tick >= _actor.ActionStartTick && tick < _actor.ImpactTick + TrailTicks;
        if (!Visible) return;
        Vector3 source = attacker.StrikeOrigin;
        bool sameCity = target is not null && target.State.Destination == _actor.Destination;
        Vector3 destination = sameCity ? target!.GlobalPosition + new Vector3(0, .5f, 0)
            : _actor.Hex?.FrozenAim is { } aim ? layout.AimPosition(_actor, aim) + new Vector3(0, .5f, 0) : source + attacker.GlobalBasis.Z * 1.5f;
        _source = source; _target = destination;
        bool landed = _actor.AttackLanded == true && tick >= _actor.ImpactTick;
        _material.AlbedoColor = tick < _actor.ImpactTick ? new Color("dfb263") : landed ? new Color("fff1b5") : new Color("829197");
        float reach = tick < _actor.ImpactTick ? (float)Math.Clamp((tick - _actor.ActionStartTick) / Math.Max(1, _actor.ImpactTick - _actor.ActionStartTick), .05, 1) : landed ? 1 : .65f;
        // A full dark guide keeps the locked target readable throughout windup;
        // the warm inner stroke shows its progress. Neither denotes a landed hit.
        _guide.GlobalPosition = (source + destination) / 2;
        if (source.DistanceSquaredTo(destination) > .00001f) _guide.LookAt(destination, Vector3.Up);
        ((BoxMesh)_guide.Mesh).Size = new(.09f, .09f, Math.Max(.01f, source.DistanceTo(destination)));
        Vector3 end = source.Lerp(destination, reach);
        _connection.GlobalPosition = (source + end) / 2 + Vector3.Up * .075f;
        if (source.DistanceSquaredTo(end) > .00001f) _connection.LookAt(end + Vector3.Up * .075f, Vector3.Up);
        ((BoxMesh)_connection.Mesh).Size = new(.055f, .055f, Math.Max(.01f, source.DistanceTo(end)));
        _impact.Visible = landed && sameCity; _impact.GlobalPosition = destination;
    }
    public object Observe(Camera3D camera, Transform2D screenTransform)
    {
        Vector2 sourceScreen = screenTransform * camera.UnprojectPosition(_source), targetScreen = screenTransform * camera.UnprojectPosition(_target);
        return new
        {
            Id = _actor?.Id,
            _actor?.TargetId,
            _actor?.AttackSequence,
            _actor?.ImpactTick,
            _actor?.AttackLanded,
            Visible,
            ImpactVisible = _impact.Visible && Visible,
            SourceX = _source.X,
            SourceZ = _source.Z,
            TargetX = _target.X,
            TargetZ = _target.Z,
            SourceScreenX = sourceScreen.X,
            SourceScreenY = sourceScreen.Y,
            TargetScreenX = targetScreen.X,
            TargetScreenY = targetScreen.Y
        };
    }
}
