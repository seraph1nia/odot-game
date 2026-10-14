using Game.Core;
using Godot;

namespace Game;

// Settlement Keep: same compact, text-free style as arena objectives/members.
internal sealed partial class HomeHealthBar : Control
{
    private readonly UnitHealthBar _bar = new();
    private int _current, _maximum, _percent;
    internal HomeHealthBar()
    {
        MouseFilter = MouseFilterEnum.Ignore; Size = _bar.Size;
        AddChild(_bar);
    }
    internal void Sample(CityState? city, int maximum, Vector3 anchor, Camera3D camera, Rect2 world)
    {
        _current = city?.Health ?? 0; _maximum = HealthPoints.FromWhole(maximum);
        _percent = ProgressionPresentation.HealthPercent(_current, _maximum);
        _bar.Sample(_current, _maximum, anchor, city is { Eliminated: false }, camera, world);
        Position = _bar.Position; _bar.Position = Vector2.Zero;
        Visible = _bar.Visible;
    }
    internal object Observe(Transform2D screen) => new { Current = _current, Maximum = _maximum, Percent = _percent, Text = "", PercentageInside = true, Visible = IsVisibleInTree(), X = (screen * Position).X, Y = (screen * Position).Y, InputIgnored = true };
}
