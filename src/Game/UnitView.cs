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
    private long _attackSequence;
    private double _hitAt = -1, _shotAt = -1;
    private Node3D _model = null!;
    private Node3D _shot = null!;
    private MeshInstance3D _marker = null!;
    private long _effectSequence;
    private int _shotCount;
    private int _rootBone, _handBone;
    private readonly Dictionary<string, double> _lengths = new(StringComparer.Ordinal);
    private string _attackName = "";
    private bool _poseSkipped;
    private static readonly StringName LocomotionBlend = "parameters/locomotion/blend_position", LocomotionSeek = "parameters/locoseek/seek_request",
        AttackSeek = "parameters/attackseek/seek_request", HitSeek = "parameters/hitseek/seek_request", DeathBlend = "parameters/death/blend_amount",
        DeathSeek = "parameters/deathseek/seek_request", AttackRequest = "parameters/attack/request", HitRequest = "parameters/hit/request",
        AttackActive = "parameters/attack/active", HitActive = "parameters/hit/active";
    public UnitState State { get; private set; } = initial;
    public bool Dead => State.Hex?.Lifecycle == UnitLifecycle.Dying;
    public string Clip { get; private set; } = "Idle";
    public double PoseSeconds { get; private set; }
    public bool Expired(double tick) => Dead && tick >= State.Hex!.DeathEndTick;
    public Vector3 StrikeOrigin => _skeleton.GlobalTransform * _skeleton.GetBoneGlobalPose(_handBone).Origin;

    public override void _Ready()
    {
        Node3D model = _model = UnitAssets.Instantiate(UnitAssets.Character(State.Type, State.Faction));
        model.Scale = Vector3.One * 0.43f; AddChild(model);
        (_player, _skeleton) = UnitAssets.Bind(model, State.Type);
        _rootBone = _skeleton.FindBone("root"); _handBone = _skeleton.FindBone("handslot.r");
        _attackName = UnitAssets.AttackClip(State.Type);
        foreach (string clip in new[] { "Idle", "Running_A", "Death_A", "Hit_A", _attackName }) _lengths[clip] = _player.GetAnimation(clip).Length;
        UnitAssets.Equip(model, _skeleton, State.Type, State.Faction);
        var locomotion = new AnimationNodeBlendSpace1D { MinSpace = 0, MaxSpace = 1 };
        locomotion.AddBlendPoint(Locomotion("Idle"), 0, -1, "idle");
        locomotion.AddBlendPoint(Locomotion("Walking_A"), 0.8f, -1, "walk");
        locomotion.AddBlendPoint(Locomotion("Running_A"), 1, -1, "run");
        var hit = new AnimationNodeOneShot { FadeInTime = 0, FadeOutTime = 0, FilterEnabled = true };
        Animation hitAnimation = _player.GetAnimation("Hit_A");
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
        blend.AddNode("attack", new AnimationNodeOneShot { FadeInTime = 0, FadeOutTime = 0 });
        blend.AddNode("hitclip", new AnimationNodeAnimation { Animation = "Hit_A" }); blend.AddNode("hitseek", new AnimationNodeTimeSeek());
        blend.AddNode("hit", hit);
        blend.AddNode("deathclip", new AnimationNodeAnimation { Animation = "Death_A" }); blend.AddNode("deathseek", new AnimationNodeTimeSeek());
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
        _shot = UnitAssets.Instantiate("arrow.gltf"); _shot.Scale = Vector3.One * 0.35f; _shot.Visible = false; AddChild(_shot);
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
        Position = layout.Position(state, tick);
        Rotation = new(0, layout.Facing(state, tick, units), 0);
        bool moving = !Dead && state.Hex?.HoldsTransit == true;
        double locomotionTime = moving ? tick / Match.StepsPerSecond : seconds;
        string clip; double pose;
        if (Dead) { clip = "Death_A"; pose = CombatPlayback.DeathPose(state, tick, _lengths[clip]); }
        else if (_hitAt >= 0 && seconds - _hitAt < 0.15) { clip = "Hit_A"; pose = (seconds - _hitAt) * 2; }
        else if (state.AttackSequence > 0 && state.TargetId != 0 && tick >= state.ActionStartTick && tick < state.ReadyTick)
        {
            clip = _attackName;
            // Role-specific measured KayKit markers map windup onto the attack pose,
            // then sample recovery through the rest of the clip by the next ready tick.
            double length = _lengths[clip];
            pose = CombatPlayback.AttackPose(state, tick, length);
        }
        else
        {
            clip = moving ? "Running_A" : "Idle";
            pose = locomotionTime % 0.8 / 0.8 * _lengths[clip];
        }
        Clip = clip; PoseSeconds = Math.Clamp(pose, 0, _lengths[clip] - 0.0001);
        _marker.Visible = !Dead;
        _shot.Visible = !Dead && _shotAt >= 0 && seconds - _shotAt < 0.12;
        _shot.Position = new(0, 0.5f, 0.25f + (float)Math.Max(0, seconds - _shotAt) * 12);
        if (!Visible) { _poseSkipped = true; return; }
        if (_poseSkipped)
        {
            _tree.Set(AttackRequest, (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _tree.Set(HitRequest, (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _attacking = _hitting = false; _poseSkipped = false;
        }
        _tree.Set(LocomotionBlend, moving ? 1 : 0);
        _tree.Set(LocomotionSeek, locomotionTime % 0.8);
        bool attacking = !Dead && state.AttackSequence > 0 && state.TargetId != 0 && tick >= state.ActionStartTick && tick < state.ReadyTick;
        bool hitting = !Dead && _hitAt >= 0 && seconds - _hitAt < 0.15;
        if (attacking && state.AttackSequence != _attackSequence) { _attacking = false; _attackSequence = state.AttackSequence; }
        OneShot(AttackRequest, attacking, ref _attacking);
        OneShot(HitRequest, hitting, ref _hitting);
        _tree.Set(AttackSeek, Math.Min(_lengths[_attackName] - 0.0001, CombatPlayback.AttackPose(state, tick, _lengths[_attackName])));
        _tree.Set(HitSeek, Math.Clamp((seconds - _hitAt) * 2, 0, 0.6665));
        _tree.Set(DeathBlend, Dead ? 1 : 0);
        double deathLength = _lengths["Death_A"];
        _tree.Set(DeathSeek, Dead ? Math.Min(deathLength - .0001, CombatPlayback.DeathPose(state, tick, deathLength)) : 0);
        Work?.Add(WorkMetric.FullPoseSamples);
        _tree.Advance(0);
        Vector3 rootPose = _skeleton.GetBonePosePosition(_rootBone);
        _skeleton.SetBonePosePosition(_rootBone, new(0, rootPose.Y, 0));
    }
    private static AnimationNodeAnimation Locomotion(string clip) => new()
    { Animation = clip, UseCustomTimeline = true, TimelineLength = 0.8, StretchTimeScale = true, LoopMode = Animation.LoopModeEnum.Linear };
    private void OneShot(StringName request, bool active, ref bool previous)
    {
        if (active != previous)
            _tree.Set(request, (int)(active ? AnimationNodeOneShot.OneShotRequest.Fire : AnimationNodeOneShot.OneShotRequest.Abort));
        previous = active;
    }
    public object Observe()
    {
        Transform3D pose = _skeleton.GetBoneGlobalPose(_handBone);
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
            BoneX = pose.Origin.X,
            BoneY = pose.Origin.Y,
            BoneZ = pose.Origin.Z,
            BoneRotation = pose.Basis.GetRotationQuaternion().ToString(),
            WeaponAttached = _skeleton.GetChildren().OfType<BoneAttachment3D>().Any(),
            EffectSequence = _effectSequence,
            ShotVisible = _shot.Visible,
            AttackActive = _tree.Get(AttackActive).AsBool(),
            HitActive = _tree.Get(HitActive).AsBool(),
            InteractionEnabled = false
        };
    }
}
