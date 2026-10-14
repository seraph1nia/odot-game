using Game.Core;
using Godot;

namespace Game;

// A current-action tabletop cue, not a projectile or a physical reach claim.
// Baselines restore progress without new sounds, damage or random choices.
internal sealed partial class MeleeStrike : Node3D
{
    public const int TrailTicks = CombatVisualTiming.StrikeTrailTicks;
    private readonly MeshInstance3D _slice = new() { CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
    private readonly MeshInstance3D _impact = new() { Mesh = new SphereMesh { Radius = .09f, Height = .18f } };
    private readonly StandardMaterial3D _material = new() { ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, CullMode = BaseMaterial3D.CullModeEnum.Disabled };
    private UnitState? _actor;
    private Vector3 _source, _target;
    private string _phase = "none";
    public override void _Ready()
    {
        var arc = new List<Vector3>();
        const float radius = CombatVisualTiming.LocalStrikeRadius;
        Vector3 Point(float angle, float r) => new(Mathf.Sin(angle) * r, 0, -Mathf.Cos(angle) * r);
        for (int n = 0; n < 8; n++)
        {
            float a = -.9f + n * 1.8f / 8, b = -.9f + (n + 1) * 1.8f / 8;
            Quad(arc, Point(a, radius), Point(b, radius), Point(b, radius - .035f), Point(a, radius - .035f));
        }
        _slice.Mesh = Mesh(arc);
        AddChild(_slice); AddChild(_impact);
        _slice.MaterialOverride = _impact.MaterialOverride = _material;
    }
    private static void Quad(List<Vector3> points, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        => points.AddRange([a, b, c, a, c, d]);
    private static ArrayMesh Mesh(List<Vector3> vertices)
    {
        var arrays = new Godot.Collections.Array(); arrays.Resize((int)Godot.Mesh.ArrayType.Max);
        arrays[(int)Godot.Mesh.ArrayType.Vertex] = vertices.ToArray();
        var mesh = new ArrayMesh(); mesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, arrays);
        return mesh;
    }
    public void Sample(UnitView attacker, UnitView? target, double tick, int focus, CombatLayout layout)
    {
        _actor = attacker.State; tick = _actor.Hex?.FrozenTick ?? tick;
        _phase = CombatVisualTiming.MeleePhase(_actor, tick);
        Visible = attacker.Visible && (focus == 0 || _actor.Destination == focus) && _phase != "none";
        if (!Visible) return;
        Vector3 source = attacker.StrikeOrigin;
        bool sameCity = target is not null;
        Vector3 destination = sameCity ? target!.GlobalPosition + new Vector3(0, .5f, 0)
            : _actor.Hex?.FrozenAim is { } aim ? layout.AimPosition(_actor, aim) + new Vector3(0, .5f, 0) : source + attacker.GlobalBasis.Z * 1.5f;
        _source = source; _target = destination;
        _slice.Visible = _phase != "intent";
        _slice.GlobalPosition = source;
        _slice.GlobalBasis = attacker.GlobalBasis;
        _material.AlbedoColor = _phase == "landed" ? new Color("fff1b5") : new Color("829197");
        _impact.Visible = _phase == "landed" && sameCity; _impact.GlobalPosition = destination;
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
            CueStyle = "physical-windup-local-strike",
            Phase = _phase,
            StrikeVisible = _slice.Visible && Visible,
            StrikeRadius = CombatVisualTiming.LocalStrikeRadius,
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
