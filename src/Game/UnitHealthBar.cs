using Game.Core;
using Godot;

namespace Game;

// One inexpensive, input-transparent overlay owned by the current tabletop.
internal sealed partial class UnitHealthBar : Control
{
    private readonly TextureRect _track = new() { Texture = UiAssets.Texture("derived/health_track.svg"), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, MouseFilter = MouseFilterEnum.Ignore };
    private readonly Control _clip = new() { ClipContents = true, MouseFilter = MouseFilterEnum.Ignore };
    private readonly TextureRect _fill = new() { Texture = UiAssets.Texture("derived/health_fill.svg"), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, MouseFilter = MouseFilterEnum.Ignore };
    private readonly Label _role = new() { MouseFilter = MouseFilterEnum.Ignore, HorizontalAlignment = HorizontalAlignment.Left };
    internal int Current { get; private set; }
    internal int Maximum { get; private set; }
    internal double Fraction { get; private set; }
    internal UnitHealthBar()
    {
        MouseFilter = MouseFilterEnum.Ignore; Size = new(46, 9);
        AddChild(_track); _track.Size = Size;
        AddChild(_clip); _clip.Position = new(1.5f, 1.5f); _clip.Size = Size - new Vector2(3, 3);
        _clip.AddChild(_fill); _fill.Size = _clip.Size;
        _role.Position = new(0, -17); _role.Size = new(46, 16);
        _role.AddThemeFontSizeOverride("font_size", 10);
        _role.AddThemeConstantOverride("outline_size", 3);
        _role.AddThemeColorOverride("font_outline_color", new("182d36"));
        AddChild(_role);
    }
    internal void Sample(UnitView view, Camera3D camera, Rect2 worldArea)
    {
        _role.Text = ProgressionPresentation.UnitLabel(view.State);
        _role.AddThemeColorOverride("font_color", new(view.State.Faction == Faction.Skeletons ? "ffb4a4" : "b9dcff"));
        Current = view.State.Health; Maximum = view.State.Profile.Health;
        Fraction = PresentationLimits.HealthFraction(Current, Maximum);
        _clip.Size = new((Size.X - 3) * (float)Fraction, Size.Y - 3);
        Vector3 anchor = view.GlobalPosition + new Vector3(0, 1.35f, 0);
        Position = camera.UnprojectPosition(anchor) - new Vector2(Size.X / 2, Size.Y);
        Visible = view.Visible && !view.Dead && view.State.Deployed && !camera.IsPositionBehind(anchor)
            && worldArea.Encloses(new Rect2(Position - new Vector2(0, 17), new Vector2(Math.Max(Size.X, _role.GetMinimumSize().X), Size.Y + 17)));
    }
    internal object Observe(int id, Transform2D transform)
    {
        Rect2 rect = GetGlobalRect(); Vector2 p = transform * rect.Position;
        return new
        {
            Id = id,
            Role = _role.Text,
            Current,
            Maximum,
            Fraction,
            Visible = IsVisibleInTree(),
            X = p.X,
            Y = p.Y,
            Width = Size.X * transform.Scale.X,
            Height = Size.Y * transform.Scale.Y,
            FillWidth = _clip.Size.X,
            InputIgnored = MouseFilter == MouseFilterEnum.Ignore && _clip.MouseFilter == MouseFilterEnum.Ignore && _fill.MouseFilter == MouseFilterEnum.Ignore && _role.MouseFilter == MouseFilterEnum.Ignore,
            Track = _track.Texture.ResourcePath,
            Fill = _fill.Texture.ResourcePath
        };
    }
}
