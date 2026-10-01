using Godot;

namespace Game;

internal static class ApplicationTheme
{
    internal static readonly string[] ButtonStates = ["normal", "hover", "pressed", "disabled"];
    internal static readonly Color Ink = new("4a3226");
    internal static readonly Color FocusColor = new("166e80");
    internal static StyleBoxTexture Slice(string path, float left, float top, float right, float bottom, float padding = 10)
    {
        var style = new StyleBoxTexture
        {
            Texture = UiAssets.Texture(path),
            TextureMarginLeft = left,
            TextureMarginTop = top,
            TextureMarginRight = right,
            TextureMarginBottom = bottom,
            ContentMarginLeft = padding,
            ContentMarginRight = padding,
            ContentMarginTop = 6,
            ContentMarginBottom = 7
        };
        return style;
    }
    private static StyleBoxFlat Flat(Color color, float padding = 6) => new()
    {
        BgColor = color,
        BorderColor = new("5c4033"),
        BorderWidthLeft = 1,
        BorderWidthRight = 1,
        BorderWidthTop = 1,
        BorderWidthBottom = 1,
        CornerRadiusTopLeft = 5,
        CornerRadiusTopRight = 5,
        CornerRadiusBottomLeft = 5,
        CornerRadiusBottomRight = 5,
        ContentMarginLeft = padding,
        ContentMarginRight = padding,
        ContentMarginTop = padding,
        ContentMarginBottom = padding
    };
    internal static Theme Create()
    {
        var theme = new Theme { DefaultFontSize = 14 };
        foreach (string type in new[] { "Label", "RichTextLabel", "Button", "OptionButton", "TabBar", "TabContainer", "PopupMenu", "Window", "AcceptDialog", "CheckButton", "CheckBox" })
        {
            theme.SetColor(type == "RichTextLabel" ? "default_color" : type is "Window" or "AcceptDialog" ? "title_color" : "font_color", type, Ink);
            foreach (string name in new[] { "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_focus_color", "font_selected_color" }) theme.SetColor(name, type, Ink);
            theme.SetColor("font_disabled_color", type, new("8c8072"));
            theme.SetColor("font_unselected_color", type, new("705740"));
            theme.SetConstant("outline_size", type, 0);
        }
        StyleBoxTexture panel = Slice("cozy/panel_plain.svg", 20, 20, 20, 20, 16);
        panel.ContentMarginTop = 12; panel.ContentMarginBottom = 12;
        theme.SetStylebox("panel", "PanelContainer", panel);
        theme.SetStylebox("panel", "PopupMenu", panel);
        theme.SetStylebox("panel", "TooltipPanel", panel);
        theme.SetColor("font_color", "TooltipLabel", Ink);
        theme.SetStylebox("panel", "TabContainer", panel);
        theme.SetStylebox("panel", "AcceptDialog", panel);
        StyleBoxTexture window = Slice("cozy/panel_plain.svg", 20, 20, 20, 20, 12);
        window.ExpandMarginTop = 30;
        theme.SetStylebox("embedded_border", "Window", window);
        theme.SetStylebox("embedded_border", "AcceptDialog", window);
        theme.SetStylebox("embedded_unfocused_border", "Window", window);
        theme.SetStylebox("embedded_unfocused_border", "AcceptDialog", window);
        theme.SetColor("title_unfocused_color", "Window", Ink);
        theme.SetConstant("title_height", "Window", 28);
        theme.SetConstant("resize_margin", "Window", 5);
        foreach (string state in ButtonStates)
        {
            StyleBoxTexture style = Slice("derived/button_" + state + ".svg", 14, 14, 14, 18, 8);
            foreach (string type in new[] { "Button", "OptionButton" }) theme.SetStylebox(state, type, style);
            foreach (string type in new[] { "TabBar", "TabContainer" })
                theme.SetStylebox(state switch { "normal" => "tab_unselected", "hover" => "tab_hovered", "pressed" => "tab_selected", _ => "tab_disabled" }, type, style);
        }
        var focus = Flat(Colors.Transparent, 0); focus.BorderColor = FocusColor;
        focus.BorderWidthLeft = focus.BorderWidthRight = focus.BorderWidthTop = focus.BorderWidthBottom = 2;
        theme.SetStylebox("focus", "Button", focus); theme.SetStylebox("focus", "OptionButton", focus); theme.SetStylebox("tab_focus", "TabBar", focus); theme.SetStylebox("tab_focus", "TabContainer", focus);
        theme.SetStylebox("hover", "PopupMenu", Flat(new("f7c97e")));
        theme.SetIcon("arrow", "OptionButton", UiAssets.Texture("derived/dropdown_arrow.svg"));
        theme.SetIcon("close", "Window", UiAssets.Texture("derived/close.svg"));
        theme.SetIcon("close_pressed", "Window", UiAssets.Texture("derived/close.svg"));
        theme.SetConstant("arrow_margin", "OptionButton", 8);
        foreach (string type in new[] { "HSlider", "HScrollBar", "VScrollBar" })
        {
            theme.SetStylebox(type == "HSlider" ? "slider" : "scroll", type, Flat(new("8a7355"), 3));
            theme.SetStylebox(type == "HSlider" ? "grabber_area" : "grabber", type, Flat(new("e8a852"), 3));
            theme.SetStylebox(type == "HSlider" ? "grabber_area_highlight" : "grabber_highlight", type, Flat(new("f7c97e"), 3));
            theme.SetStylebox("grabber_pressed", type, Flat(new("d98f35"), 3));
            theme.SetStylebox("focus", type, focus);
        }
        foreach (string name in new[] { "grabber", "grabber_highlight", "grabber_disabled" }) theme.SetIcon(name, "HSlider", UiAssets.Texture("derived/slider_knob.svg"));
        theme.SetStylebox("background", "ProgressBar", Slice("derived/health_track.svg", 17, 17, 17, 17, 0));
        theme.SetStylebox("fill", "ProgressBar", Slice("derived/health_fill.svg", 11, 11, 11, 11, 0));
        return theme;
    }
}
