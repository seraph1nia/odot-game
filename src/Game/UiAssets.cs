using Game.Core;
using Godot;

namespace Game;

internal static class UiAssets
{
    internal const string Root = "res://Assets/TrioUI/";
    private static readonly Dictionary<string, Texture2D> Textures = [];
    internal static Texture2D Texture(string path)
    {
        if (!Textures.TryGetValue(path, out Texture2D? texture))
            Textures[path] = texture = GD.Load<Texture2D>(Root + path) ?? throw new InvalidOperationException("Missing Trio UI asset: " + path);
        return texture;
    }
    internal static Texture2D? Icon(string meaning) => meaning switch
    {
        "Settings" or "MenuSettings" => Texture("icons/icon_gear.svg"),
        "Singleplayer" or "Start" or "Ready" or "Resume" or "AcceptJoin" => Texture("icons/icon_play.svg"),
        "Back" or "ReturnToMenu" or "DeclineJoin" => Texture("cozy/arrow_left.svg"),
        "Host" or "City" => Texture("icons/icon_home.svg"),
        "Inspect" => Texture("icons/icon_map.svg"),
        "Health" => Texture("icons/icon_heart.svg"),
        "Audio" => Texture("icons/icon_sound_on.svg"),
        "Gold" => Texture("icons/icon_gold_pile.svg"),
        "Food" or "Farm" => Texture("icons/icon_bread.svg"),
        "Army" or "Barracks" => Texture("icons/icon_helmet.svg"),
        "Swordsman" or "Melee" => Texture("icons/icon_sword.svg"),
        "Berserker" => Texture("icons/icon_axe.svg"),
        "Crossbowman" or "Ranged" or "ArcheryRange" or "ArrowTower" => Texture("icons/icon_bow.svg"),
        "Mage" or "Magic" or "Arcanum" => Texture("icons/icon_staff_magic.svg"),
        "Upgrade" or "ResearchTower" => Texture("icons/icon_buff.svg"),
        "Mine" => Texture("icons/icon_gold_pile.svg"),
        "CatapultTower" => Texture("icons/icon_shield.svg"),
        _ => null
    };
    internal static void Decorate(Button button, string meaning)
    {
        button.Icon = null;
    }
    internal static void Cost(Button button, ResourceCost cost)
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
        label.Text = "[center]" + string.Join(" · ", parts) + "[/center]";
    }
}
