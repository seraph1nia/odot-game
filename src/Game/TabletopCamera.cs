using Godot;

namespace Game;

// Local presentation state, applied on top of the city's fitted overview.
internal sealed class TabletopCamera(Camera3D camera)
{
    private const float MaximumZoom = 3, WheelStep = 1.12f;
    internal static readonly Vector2 Travel = new(6, 8);
    private static readonly (string Action, Vector2 Direction)[] Actions =
    [
        ("camera_left", Vector2.Left), ("camera_right", Vector2.Right),
        ("camera_up", Vector2.Up), ("camera_down", Vector2.Down)
    ];
    private readonly Dictionary<Key, Vector2> _held = [];
    private readonly HashSet<Key> _down = [], _blocked = [];
    private static readonly bool TravelTrace = System.Environment.GetEnvironmentVariable("ODOT_CAMERA_TRAVEL_TRACE") == "1";
    private Transform3D _base, _fittedBase;
    private float _baseSize;
    private Vector2 _travel = Travel;
    private Rect2 _boundary, _viewport, _content;
    internal Rect2 Boundary => _boundary;
    internal Vector3? ViewCenter => Ground(_viewport.GetCenter());
    internal float Zoom { get; private set; } = 1;
    internal Vector2 Pan { get; private set; }
    internal double MovementSeconds { get; private set; }
    internal Vector2 MovementDirection { get; private set; }

    internal void Fit(Rect2 content, Rect2 viewport, bool sharedArena = false)
    {
        _fittedBase = _base = camera.Transform; _baseSize = camera.Size; _viewport = viewport; _content = content.Grow(BattlefieldFrame.Margin);
        _travel = sharedArena ? content.Size / 2 : Travel;
        // HUD compensation can put the fitted ground center beyond the content,
        // including while the solo lobby's panel is still laying out. Retain that
        // valid overview for every view; otherwise content and footprint limits
        // can conflict. The projected opaque boundary stays the hard travel limit.
        if (Ground(viewport.GetCenter()) is { } fittedCenter)
            _content = _content.Expand(new(fittedCenter.X, fittedCenter.Z));
        // Include the complete default native projection, even behind the HUD.
        // This also handles narrow/wide resize without exposing a perimeter end.
        _boundary = BattlefieldFrame.Footprint(camera, viewport).Merge(content.Grow(BattlefieldFrame.Margin)).Grow(1);
        Apply();
    }
    internal void Reset()
    {
        Zoom = 1; Pan = Vector2.Zero; Interrupt(); Apply();
    }
    internal void ProofView(string view, Vector3 point, float hudFraction)
    {
        Zoom = view is "forest" or "river" ? 3 : 1; Pan = Vector2.Zero;
        _base = _fittedBase;
        if (Zoom == 3)
        {
            Vector3 direction = _fittedBase.Basis.Z;
            camera.Position = point + direction * 45; camera.LookAt(point);
            camera.Position -= camera.Basis.Y * (_baseSize / Zoom) * hudFraction / 2;
            _base = camera.Transform;
        }
        Interrupt(); Apply();
    }
    internal void Interrupt()
    {
        _blocked.UnionWith(_down); _held.Clear(); MovementDirection = Vector2.Zero;
    }
    internal void ObserveKey(InputEventKey key)
    {
        Key code = key.PhysicalKeycode == Key.None ? key.Keycode : key.PhysicalKeycode;
        if (TravelTrace) GD.Print($"ODOT_TRAVEL key={code} pressed={key.Pressed} frame={Engine.GetProcessFrames()} utc={DateTimeOffset.UtcNow:O}");
        if (!key.Pressed) { _held.Remove(code); _down.Remove(code); _blocked.Remove(code); }
        else if (Actions.Any(a => key.IsAction(a.Action))) _down.Add(code);
    }
    internal bool Press(InputEventKey key)
    {
        Key code = key.PhysicalKeycode == Key.None ? key.Keycode : key.PhysicalKeycode;
        if (!key.Pressed || key.Echo || _blocked.Contains(code)) return false;
        foreach (var (action, direction) in Actions)
            if (key.IsActionPressed(action)) { _held[code] = direction; return true; }
        return false;
    }
    internal void Move(double delta, bool eligible, float worldFraction)
    {
        // The OS may have delivered a release while this window was unfocused.
        foreach (Key code in _down.Where(k => !Input.IsPhysicalKeyPressed(k)).ToArray())
        { _down.Remove(code); _blocked.Remove(code); _held.Remove(code); }
        if (!eligible) { Interrupt(); return; }
        Vector2 direction = Vector2.Zero;
        foreach (Vector2 value in _held.Values.Distinct()) direction += value;
        MovementDirection = direction.Normalized();
        if (MovementDirection != Vector2.Zero)
        {
            Vector3 right = camera.Basis.X; right.Y = 0; right = right.Normalized();
            Vector3 forward = -camera.Basis.Z; forward.Y = 0; forward = forward.Normalized();
            Vector3 movement = (right * MovementDirection.X - forward * MovementDirection.Y)
                * camera.Size * worldFraction * 0.5f * (float)delta;
            Pan += new Vector2(movement.X, movement.Z);
            MovementSeconds += delta;
        }
        Apply();
        if (TravelTrace && _down.Count != 0) GD.Print(FormattableString.Invariant($"ODOT_TRAVEL frame={Engine.GetProcessFrames()} delta={delta:R} eligible={eligible} physicalD={Input.IsPhysicalKeyPressed(Key.D)} direction={MovementDirection} pan={Pan} seconds={MovementSeconds:R} size={camera.Size:R} fraction={worldFraction:R} yaw={camera.Rotation.Y:R} utc={DateTimeOffset.UtcNow:O}"));
    }
    internal void ZoomAt(Vector2 cursor, bool inward, float factor)
    {
        float next = Mathf.Clamp(Zoom * Mathf.Pow(WheelStep, (inward ? 1 : -1) * factor), 1, MaximumZoom);
        if (Mathf.IsEqualApprox(next, Zoom) || Ground(cursor) is not { } before) return;
        float previous = Zoom; Zoom = next; Apply();
        if (Ground(cursor) is not { } after) { Zoom = previous; Apply(); return; }
        Pan += new Vector2(before.X - after.X, before.Z - after.Z);
        Apply();
    }
    internal Vector3? DragAnchor(Vector2 cursor) => Ground(cursor);
    internal void Drag(Vector3 anchor, Vector2 cursor)
    {
        if (Ground(cursor) is not { } current) return;
        Pan += new Vector2(anchor.X - current.X, anchor.Z - current.Z);
        Apply();
    }
    private Vector3? Ground(Vector2 cursor) => Plane.PlaneXZ.IntersectsRay(camera.ProjectRayOrigin(cursor), camera.ProjectRayNormal(cursor));
    private void Apply()
    {
        if (_baseSize <= 0) return;
        camera.Transform = _base;
        camera.Size = _baseSize / Zoom;
        Rect2 footprint = BattlefieldFrame.Footprint(camera, _viewport);
        Vector3 aim = Ground(_viewport.GetCenter()) ?? _base.Origin;
        Vector2 center = new(aim.X, aim.Z);
        Vector2 minimum = (-_travel).Max(_boundary.Position - footprint.Position).Max(_content.Position - center);
        Vector2 maximum = _travel.Min(_boundary.End - footprint.End).Min(_content.End - center);
        Pan = Pan.Clamp(minimum, maximum);
        camera.Position += new Vector3(Pan.X, 0, Pan.Y);
    }
    internal object Observe(Vector3 reference, Transform2D screen)
    {
        Vector2 point = screen * camera.UnprojectPosition(reference);
        return new
        {
            Zoom,
            PanX = Pan.X,
            PanZ = Pan.Y,
            BaseSize = _baseSize,
            camera.Size,
            Height = camera.Position.Y,
            Rotation = new[] { camera.Rotation.X, camera.Rotation.Y, camera.Rotation.Z },
            ReferenceX = point.X,
            ReferenceY = point.Y,
            MovementSeconds,
            DirectionX = MovementDirection.X,
            DirectionY = MovementDirection.Y
        };
    }
}
