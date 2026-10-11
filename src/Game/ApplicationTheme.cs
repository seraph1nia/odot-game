using Godot;

namespace Game;

internal static class ApplicationTheme
{
    internal static readonly string[] ButtonStates = ["normal", "hover", "pressed", "disabled"];
    internal static readonly Color Ink = new("f2e6cb");
    internal static readonly Color FocusColor = new("8acce4");

    // The accepted woodland Theme owns native styles, font metrics and focus states.
    // Keep application/session/settings ownership here, not in the asset showcase.
    internal static Theme Create() => GD.Load<Theme>(UiAssets.Root + "theme/ledger.tres")
        ?? throw new InvalidOperationException("Missing woodland UI theme.");
}
