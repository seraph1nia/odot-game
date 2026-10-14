using Game.Core;
using Godot;

namespace Game;

// One inexpensive, input-transparent overlay owned by the current tabletop.
internal sealed partial class UnitHealthBar : Control
{
    private readonly Panel _track = new() { MouseFilter = MouseFilterEnum.Ignore };
    private readonly Control _clip = new() { ClipContents = true, MouseFilter = MouseFilterEnum.Ignore };
    private readonly Panel _fill = new() { MouseFilter = MouseFilterEnum.Ignore };
    internal int Current { get; private set; }
    internal int Maximum { get; private set; }
    internal double Fraction { get; private set; }
    internal UnitHealthBar(Vector2? size = null)
    {
        MouseFilter = MouseFilterEnum.Ignore; Size = size ?? new Vector2(46, 9);
        Theme theme = ApplicationTheme.Create();
        _track.AddThemeStyleboxOverride("panel", theme.GetStylebox("background", "ProgressBar"));
        _fill.AddThemeStyleboxOverride("panel", theme.GetStylebox("fill", "ProgressBar"));
        AddChild(_track); _track.Size = Size;
        AddChild(_clip); _clip.Position = new(1.5f, 1.5f); _clip.Size = Size - new Vector2(3, 3);
        _clip.AddChild(_fill); _fill.Size = _clip.Size;
    }
    // One fixed slot per member, packed once above the shared squad actor.
    // Stable slots prevent survivors' bars jumping together after a casualty.
    internal static Vector2 FormationOffset(int slot, int capacity)
    {
        int columns = Math.Min(3, capacity), rows = (capacity + columns - 1) / columns;
        return new((slot % columns - (columns - 1) / 2f) * 18, -(rows - 1 - slot / columns) * 9);
    }
    internal void Sample(UnitView view, Camera3D camera, Rect2 worldArea, Vector3? anchor = null, Vector2 offset = default)
    {
        Sample(view.State.Health, view.State.Profile.Health, anchor ?? view.GlobalPosition + new Vector3(0, view.State.IsBoss ? 2.1f : 1.35f, 0),
            view.IsVisibleInTree() && !view.Dead && view.State.Deployed, camera, worldArea, offset);
    }
    internal void Sample(int current, int maximum, Vector3 anchor, bool visible, Camera3D camera, Rect2 worldArea, Vector2 offset = default)
    {
        Current = current; Maximum = maximum;
        Fraction = PresentationLimits.HealthFraction(Current, Maximum);
        _clip.Size = new((Size.X - 3) * (float)Fraction, Size.Y - 3);
        Position = camera.UnprojectPosition(anchor) - new Vector2(Size.X / 2, Size.Y) + offset;
        Visible = visible && current > 0 && maximum > 0 && !camera.IsPositionBehind(anchor)
            && worldArea.Encloses(new Rect2(Position, Size));
    }
    internal object Observe(int id, Transform2D transform)
    {
        Rect2 rect = GetGlobalRect(); Vector2 p = transform * rect.Position;
        return new
        {
            Id = id,
            Role = "",
            Current,
            Maximum,
            Fraction,
            Visible = IsVisibleInTree(),
            X = p.X,
            Y = p.Y,
            Width = Size.X * transform.Scale.X,
            Height = Size.Y * transform.Scale.Y,
            FillWidth = _clip.Size.X,
            CapacityWidth = Size.X - 3,
            InputIgnored = MouseFilter == MouseFilterEnum.Ignore && _clip.MouseFilter == MouseFilterEnum.Ignore && _fill.MouseFilter == MouseFilterEnum.Ignore,
            Track = _track.GetThemeStylebox("panel").ResourcePath,
            Fill = _fill.GetThemeStylebox("panel").ResourcePath
        };
    }
}
