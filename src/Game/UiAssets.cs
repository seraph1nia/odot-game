using Game.Core;
using Godot;

namespace Game;

internal static class UiAssets
{
    internal const string Root = "res://UI/";
    private static readonly Dictionary<string, Texture2D> Textures = [];
    internal static Texture2D Texture(string path)
    {
        if (!Textures.TryGetValue(path, out Texture2D? texture))
            Textures[path] = texture = GD.Load<Texture2D>(Root + path) ?? throw new InvalidOperationException("Missing woodland UI asset: " + path);
        return texture;
    }
    internal static Texture2D? Icon(string meaning)
    {
        string[] names = ["Gold", "Food", "Wood", "Stone", "Metal", "Cloth"];
        int index = Array.IndexOf(names, meaning);
        return index < 0 ? null : new AtlasTexture { Atlas = Texture("art/woodland/resource_icons.svg"), Region = new(index * 32, 0, 32, 32) };
    }
    internal static Control Component(string name, Node parent)
    {
        var control = GD.Load<PackedScene>(Root + "components/" + name + ".tscn").Instantiate<Control>();
        parent.AddChild(control);
        return control;
    }
    internal static void Decorate(Button button, string meaning)
    {
        button.Icon = null;
        button.ThemeTypeVariation = meaning switch
        {
            "Singleplayer" or "Start" or "Host" or "AcceptJoin" => "PrimaryButton",
            "Ready" or "Unready" => "ReadyButton",
            "Pause" or "Resume" => "MagicButton",
            "Exit" or "ReturnToMenu" => "DangerButton",
            _ => ""
        };
    }
    internal static void Cost(Button button, ResourceCost cost, bool stacked = false)
    {
        RichTextLabel? label = button.GetNodeOrNull<RichTextLabel>("Cost");
        if (label is null)
        {
            label = new RichTextLabel { Name = "Cost", BbcodeEnabled = true, ScrollActive = false, MouseFilter = Control.MouseFilterEnum.Ignore };
            label.AddThemeFontSizeOverride("normal_font_size", 11);
            button.AddChild(label); label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
            label.OffsetLeft = 4; label.OffsetRight = -4; label.OffsetTop = -24; label.OffsetBottom = -3;
        }
        List<string> parts = [];
        if (cost.Gold > 0) parts.Add($"{cost.Gold} gold");
        if (cost.Food > 0) parts.Add($"{cost.Food} food");
        if (cost.Wood > 0) parts.Add($"{cost.Wood} wood");
        if (cost.Stone > 0) parts.Add($"{cost.Stone} stone");
        if (cost.Metal > 0) parts.Add($"{cost.Metal} metal");
        if (cost.Cloth > 0) parts.Add($"{cost.Cloth} cloth");
        label.Modulate = button.Disabled ? new Color(1, 1, 1, 0.6f) : Colors.White;
        string caption = string.Join(" · ", parts);
        label.Text = "[center]" + caption + "[/center]";
        if (!stacked) return;

        // Construction titles use the Button's native shaped/wrapped text box.
        // Reserve a separate caption line in its content margins, so the native
        // minimum size includes BOTH lines rather than an ignored child overlay.
        const int padding = 6, gap = 4, horizontalPadding = 8;
        Font font = label.GetThemeFont("normal_font");
        int fontSize = label.GetThemeFontSize("normal_font_size");
        float height = Mathf.Ceil(font.GetHeight(fontSize));
        button.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        button.TextOverrunBehavior = TextServer.OverrunBehavior.NoTrimming;
        button.CustomMinimumSize = new(Mathf.Ceil(font.GetStringSize(caption, fontSize: fontSize).X) + horizontalPadding * 2, 0);
        foreach (string state in ApplicationTheme.ButtonStates)
        {
            var style = (StyleBox)button.GetThemeStylebox(state).Duplicate();
            style.ContentMarginLeft = style.ContentMarginRight = horizontalPadding;
            style.ContentMarginTop = padding;
            style.ContentMarginBottom = padding + height + gap;
            button.AddThemeStyleboxOverride(state, style);
        }
        label.AutowrapMode = TextServer.AutowrapMode.Off;
        label.OffsetLeft = horizontalPadding; label.OffsetRight = -horizontalPadding;
        label.OffsetTop = -padding - height; label.OffsetBottom = -padding;
    }
}
