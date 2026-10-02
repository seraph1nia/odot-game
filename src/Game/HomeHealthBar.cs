using Game.Core;
using Godot;

namespace Game;

internal sealed partial class HomeHealthBar : Control
{
    private readonly ProgressBar _bar = new() { ShowPercentage = false, Step = 0, MouseFilter = MouseFilterEnum.Ignore, Size = new(100, 18) };
    private readonly Label _text = new() { HorizontalAlignment = HorizontalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore, Size = new(100, 18), VerticalAlignment = VerticalAlignment.Center };
    private int _current, _maximum, _percent;
    internal HomeHealthBar()
    {
        MouseFilter = MouseFilterEnum.Ignore; Size = new(100, 18);
        _bar.AddThemeStyleboxOverride("background", new StyleBoxTexture { Texture = UiAssets.Texture("derived/health_track.svg") });
        _bar.AddThemeStyleboxOverride("fill", new StyleBoxTexture { Texture = UiAssets.Texture("derived/health_fill.svg") });
        _text.AddThemeFontSizeOverride("font_size", 12);
        _text.AddThemeColorOverride("font_color", Colors.White); _text.AddThemeColorOverride("font_outline_color", new("182d36")); _text.AddThemeConstantOverride("outline_size", 3);
        AddChild(_bar); AddChild(_text);
    }
    internal void Sample(CityState? city, int maximum, Vector3 anchor, Camera3D camera, Rect2 world)
    {
        _current = city?.Health ?? 0; _maximum = HealthPoints.FromWhole(maximum);
        _percent = ProgressionPresentation.HealthPercent(_current, _maximum);
        _bar.Value = PresentationLimits.HealthFraction(_current, _maximum) * 100;
        _text.Text = $"{_percent}%{(city?.Eliminated == true ? " · Fallen" : "")}";
        Position = camera.UnprojectPosition(anchor) - new Vector2(50, 18);
        Visible = city is not null && !camera.IsPositionBehind(anchor) && world.Encloses(new Rect2(Position, Size));
    }
    internal object Observe(Transform2D screen) => new { Current = _current, Maximum = _maximum, Percent = _percent, Text = _text.Text, PercentageInside = _text.Position == Vector2.Zero, Visible = IsVisibleInTree(), X = (screen * Position).X, Y = (screen * Position).Y, InputIgnored = true };
}
