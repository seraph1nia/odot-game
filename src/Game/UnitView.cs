using Game.Core;
using Godot;

namespace Game;

// A pose sampler, never a gameplay actor. Authority owns motion and impacts.
internal sealed partial class UnitView(UnitState initial) : Node3D
{
    private AnimationPlayer _player = null!;
    private Skeleton3D _skeleton = null!;
    private AnimationTree _tree = null!;
    private readonly AnimationNodeAnimation _attackClip = new();
    private bool _attacking, _hitting;
    private double? _facingTick;
    private double _walkingBlend, _attackBlend, _hitBlend;
    private double _hitAt = -1, _shotAt = -1;
    private Node3D _model = null!;
    private Node3D _shot = null!;
    private MeshInstance3D _marker = null!;
    private long _effectSequence;
    private int _shotCount;
    private int _rootBone, _handBone;
    private readonly Dictionary<string, double> _lengths = new(StringComparer.Ordinal);
    private string _attackName = "";
    private static readonly StringName LocomotionBlend = "parameters/locomotion/blend_position", LocomotionSeek = "parameters/locoseek/seek_request",
        AttackSeek = "parameters/attackseek/seek_request", HitSeek = "parameters/hitseek/seek_request", DeathBlend = "parameters/death/blend_amount",
        DeathSeek = "parameters/deathseek/seek_request",
        AttackBlend = "parameters/attack/blend_amount", HitBlend = "parameters/hit/blend_amount";
    private Label3D _statusBadge = null!;
    public string StatusBadge => _statusBadge?.Text ?? "";
    public UnitState State { get; private set; } = initial;
    public bool Dead => State.Hex?.Lifecycle == UnitLifecycle.Dying;
    public string Clip { get; private set; } = "idle";
    public double PoseSeconds { get; private set; }
    public bool Expired(double tick) => Dead && tick >= State.Hex!.DeathEndTick;
    public Vector3 StrikeOrigin => _skeleton.GlobalTransform * _skeleton.GetBoneGlobalPose(_handBone).Origin;

    public override void _Ready()
    {
        Node3D model = _model = UnitAssets.Instantiate(UnitAssets.Character(State.Type, State.Faction));
        model.Scale = Vector3.One * 0.43f; AddChild(model);
        (_player, _skeleton) = UnitAssets.Bind(model, State.Type);
        _rootBone = _skeleton.FindBone("root"); _handBone = _skeleton.FindBone(AssetCatalog.Unit(State.Type, State.Faction).StrikeSocket);
        _attackName = UnitAssets.AttackClip(State.Type);
        foreach (string clip in new[] { "idle", "walk", "death", "hit", _attackName }) _lengths[clip] = _player.GetAnimation(clip).Length;
        UnitAssets.Equip(model, _skeleton, State.Type, State.Faction);
        var locomotion = new AnimationNodeBlendSpace1D { MinSpace = 0, MaxSpace = 1 };
        locomotion.AddBlendPoint(Locomotion("idle"), 0, -1, "idle");
        locomotion.AddBlendPoint(Locomotion("walk"), 0.8f, -1, "walk");
        locomotion.AddBlendPoint(Locomotion("run"), 1, -1, "run");
        var hit = new AnimationNodeBlend2 { FilterEnabled = true };
        Animation hitAnimation = _player.GetAnimation("hit");
        for (int track = 0; track < hitAnimation.GetTrackCount(); track++)
        {
            NodePath path = hitAnimation.TrackGetPath(track);
            string bone = path.GetSubNameCount() > 0 ? path.GetSubName(0).ToString() : "";
            if (bone.Contains("spine", StringComparison.Ordinal) || bone.Contains("arm", StringComparison.Ordinal)
                || bone.Contains("hand", StringComparison.Ordinal) || bone.Contains("head", StringComparison.Ordinal)
                || bone.Contains("neck", StringComparison.Ordinal)) hit.SetFilterPath(path, true);
        }
        _attackClip.Animation = _attackName;
        var blend = new AnimationNodeBlendTree();
        blend.AddNode("locomotion", locomotion); blend.AddNode("locoseek", new AnimationNodeTimeSeek());
        blend.AddNode("attackclip", _attackClip); blend.AddNode("attackseek", new AnimationNodeTimeSeek());
        blend.AddNode("attack", new AnimationNodeBlend2());
        blend.AddNode("hitclip", new AnimationNodeAnimation { Animation = "hit" }); blend.AddNode("hitseek", new AnimationNodeTimeSeek());
        blend.AddNode("hit", hit);
        blend.AddNode("deathclip", new AnimationNodeAnimation { Animation = "death" }); blend.AddNode("deathseek", new AnimationNodeTimeSeek());
        blend.AddNode("death", new AnimationNodeBlend2());
        blend.ConnectNode("locoseek", 0, "locomotion"); blend.ConnectNode("attackseek", 0, "attackclip");
        blend.ConnectNode("attack", 0, "locoseek"); blend.ConnectNode("attack", 1, "attackseek");
        blend.ConnectNode("hitseek", 0, "hitclip"); blend.ConnectNode("hit", 0, "attack"); blend.ConnectNode("hit", 1, "hitseek");
        blend.ConnectNode("deathseek", 0, "deathclip"); blend.ConnectNode("death", 0, "hit"); blend.ConnectNode("death", 1, "deathseek");
        blend.ConnectNode("output", 0, "death");
        _tree = new AnimationTree { TreeRoot = blend, CallbackModeProcess = AnimationMixer.AnimationCallbackModeProcess.Manual };
        model.AddChild(_tree); _tree.AnimPlayer = _tree.GetPathTo(_player); _tree.Active = true;
        _marker = new MeshInstance3D
        {
            Position = new(0, 0.015f, 0),
            Mesh = new CylinderMesh { TopRadius = 0.21f, BottomRadius = 0.21f, Height = 0.025f, RadialSegments = 12 },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new(State.Faction == Faction.Skeletons ? "bb4b43" : "487ecc"), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded }
        };
        AddChild(_marker);
        _statusBadge = new Label3D { Name = "StatusBadge", Position = new(0, 3.5f, 0), FontSize = 32, PixelSize = .018f, VerticalAlignment = VerticalAlignment.Bottom, Billboard = BaseMaterial3D.BillboardModeEnum.Enabled, Modulate = new Color("b8f5ef"), OutlineModulate = new Color("233a3f"), OutlineSize = 3, NoDepthTest = true }; AddChild(_statusBadge);
        _shot = UnitAssets.Instantiate(AssetCatalog.Arrow); _shot.Scale = Vector3.One * .35f; _shot.RotationDegrees = new(0, -90, 0); _shot.Visible = false; AddChild(_shot);
    }
    internal Aabb PickingBounds()
    {
        Aabb? bounds = null;
        foreach (MeshInstance3D mesh in _model.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>().Where(mesh => mesh.IsVisibleInTree()))
        {
            Aabb box = mesh.GlobalTransform * mesh.GetAabb();
            bounds = bounds is { } existing ? existing.Merge(box) : box;
        }
        return bounds ?? new Aabb(GlobalPosition, new Vector3(.5f, 1.2f, .5f));
    }
    public void Event(CombatEvent entry, double seconds)
    {
        if (entry.Unit is null) return;
        _effectSequence = entry.Sequence;
        switch (entry.Type)
        {
            case CombatEventType.Hit: if (!Dead) { _hitAt = seconds; _hitting = false; } break;
            case CombatEventType.Death: State = entry.Unit; break;
            case CombatEventType.Impact when entry.Unit.Type == UnitType.Crossbowman: _shotAt = seconds; _shotCount++; break;
        }
    }
    internal WorkCounters? Work { get; set; }
    internal void Synchronize(UnitState state) => State = state;
    public void Sample(UnitState? current, double tick, double seconds, int focus, CombatLayout layout, IReadOnlyDictionary<int, UnitState> units)
    {
        if (current is not null) State = current;
        UnitState state = State;
        tick = state.Hex?.FrozenTick ?? tick;
        Visible = PresentationLimits.SamplesPose(state, focus);
        _statusBadge.Text = Dead ? "" : ProgressionPresentation.StatusBadge(state.Statuses, tick);
        _statusBadge.Visible = !Dead && _statusBadge.Text.Length > 0;
        _statusBadge.Modulate = state.Statuses.Burn is { } burn && tick < burn.ExpiresTick ? new Color("ffb078")
            : state.Statuses.Poison.Any(p => tick < p.ExpiresTick) ? new Color("b6ef78") : new Color("b8f5ef");
        Position = layout.Position(state, tick);
        float desired = layout.Facing(state, tick, units);
        Rotation = new(0, _facingTick is { } previousTick ? CombatVisualTiming.Facing(Rotation.Y, desired, Dead ? 0 : tick - previousTick) : desired, 0);
        _facingTick = tick;
        _walkingBlend = CombatVisualTiming.Walking(state.Hex, tick);
        _attackBlend = CombatVisualTiming.Attack(state, tick);
        _hitBlend = CombatVisualTiming.Hit(seconds, _hitAt, Dead, _attackBlend > 0);
        bool moving = !Dead && state.Hex?.HoldsTransit == true;
        double locomotionTime = moving ? tick / Match.StepsPerSecond : seconds;
        string clip; double pose;
        if (Dead) { clip = "death"; pose = CombatPlayback.DeathPose(state, tick, _lengths[clip]); }
        else if (_hitAt >= 0 && seconds - _hitAt < 0.15) { clip = "hit"; pose = (seconds - _hitAt) / .15 * _lengths[clip]; }
        else if (state.AttackSequence > 0 && state.TargetId != 0 && tick >= state.ActionStartTick && tick < state.ReadyTick)
        {
            clip = _attackName;
            // The authored frame-13 peak maps to the unchanged authority impact;
            // recovery samples the rest of this export by its committed ready tick.
            double length = _lengths[clip];
            pose = AssetCatalog.AttackPose(state, tick, length);
        }
        else
        {
            clip = moving ? "walk" : "idle";
            pose = locomotionTime % 0.8 / 0.8 * _lengths[clip];
        }
        Clip = clip; PoseSeconds = Math.Clamp(pose, 0, _lengths[clip] - 0.0001);
        _marker.Visible = !Dead;
        _shot.Visible = !Dead && _shotAt >= 0 && seconds - _shotAt < 0.12;
        _shot.Position = new(0, 0.5f, 0.25f + (float)Math.Max(0, seconds - _shotAt) * 12);
        _attacking = !Dead && state.AttackSequence > 0 && state.TargetId != 0 && tick >= state.ActionStartTick && tick < state.ReadyTick;
        _hitting = !Dead && _hitAt >= 0 && seconds - _hitAt < .15;
        if (!Visible) return;
        _tree.Set(LocomotionBlend, _walkingBlend);
        _tree.Set(AttackBlend, _attackBlend);
        _tree.Set(HitBlend, _hitBlend);
        _tree.Set(LocomotionSeek, locomotionTime % 0.8);
        _tree.Set(AttackSeek, Math.Min(_lengths[_attackName] - 0.0001, AssetCatalog.AttackPose(state, tick, _lengths[_attackName])));
        _tree.Set(HitSeek, Math.Clamp((seconds - _hitAt) / .15 * _lengths["hit"], 0, _lengths["hit"] - .0001));
        _tree.Set(DeathBlend, Dead ? 1 : 0);
        double deathLength = _lengths["death"];
        _tree.Set(DeathSeek, Dead ? Math.Min(deathLength - .0001, CombatPlayback.DeathPose(state, tick, deathLength)) : 0);
        Work?.Add(WorkMetric.FullPoseSamples);
        _tree.Advance(0);
        Vector3 rootPose = _skeleton.GetBonePosePosition(_rootBone);
        _skeleton.SetBonePosePosition(_rootBone, new(0, rootPose.Y, 0));
    }
    private static AnimationNodeAnimation Locomotion(string clip) => new()
    { Animation = clip, UseCustomTimeline = true, TimelineLength = 0.8, StretchTimeScale = true, LoopMode = Animation.LoopModeEnum.Linear };
    public object Observe()
    {
        Transform3D pose = _skeleton.GetBoneGlobalPose(_handBone);
        var equipment = UnitAssets.EquipmentPose(_model, _skeleton, State.Type, State.Faction);
        return new
        {
            State.Id,
            State.Type,
            State.Faction,
            State.Class,
            State.Rank,
            State.Level,
            State.IsBoss,
            State.Size,
            State.Participating,
            State.Destination,
            State.Deployed,
            State.Health,
            MaximumHealth = State.Profile.Health,
            HumanHealth = HealthPoints.Format(State.Health),
            State.AttackSequence,
            Dead,
            Visible,
            Clip,
            PoseSeconds,
            ShotCount = _shotCount,
            State.ActionStartTick,
            State.ImpactTick,
            State.ReadyTick,
            State.Hex,
            State.AttackLanded,
            X = Position.X,
            Z = Position.Z,
            Heading = Rotation.Y,
            WalkingBlend = Visible ? _tree.Get(LocomotionBlend).AsDouble() : _walkingBlend,
            AttackBlend = Visible ? _tree.Get(AttackBlend).AsDouble() : _attackBlend,
            HitBlend = Visible ? _tree.Get(HitBlend).AsDouble() : _hitBlend,
            BoneX = pose.Origin.X,
            BoneY = pose.Origin.Y,
            BoneZ = pose.Origin.Z,
            BoneRotation = pose.Basis.GetRotationQuaternion().ToString(),
            WeaponAttached = UnitAssets.Equipped(_model, _skeleton, State.Type, State.Faction),
            EquipmentAligned = equipment.Aligned,
            EquipmentRotation = equipment.Rotation,
            EffectSequence = _effectSequence,
            ShotVisible = _shot.Visible,
            AttackActive = _attacking,
            HitActive = _hitting,
            InteractionEnabled = false
        };
    }
}
