using Godot;

namespace Game;

internal static class ApplicationTheme
{
    internal static Theme Create()
    {
        var theme = new Theme { DefaultFontSize = 15 };
        theme.SetColor("font_color", "Label", new("f4ecd9"));
        theme.SetColor("font_color", "Button", new("f4ecd9"));
        theme.SetColor("font_disabled_color", "Button", new("99a59b"));
        theme.SetStylebox("panel", "PanelContainer", new StyleBoxFlat { BgColor = new("243c38"), BorderColor = new("899c78"), BorderWidthTop = 2, CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12, ContentMarginLeft = 20, ContentMarginRight = 20, ContentMarginTop = 12, ContentMarginBottom = 10 });
        foreach (string state in new[] { "normal", "hover", "pressed", "disabled", "focus" })
        {
            var style = new StyleBoxFlat { BgColor = new(state switch { "hover" => "58765d", "pressed" => "6c805f", "disabled" => "2e4540", "focus" => "4c6956", _ => "405e4e" }), BorderColor = new("a5b68a"), BorderWidthBottom = state == "hover" ? 2 : 0, CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5, ContentMarginTop = 7, ContentMarginBottom = 7, ContentMarginLeft = 10, ContentMarginRight = 10 };
            theme.SetStylebox(state, "Button", style);
        }
        return theme;
    }
}
