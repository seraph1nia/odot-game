using Godot;
using System.Text.Json;

namespace Game;

public partial class ClientSettings
{
    private static readonly bool TracePopup = System.Environment.GetEnvironmentVariable("ODOT_GRAPHICS_POPUP_TRACE") == "1";
    private void AttachPopupTrace(OptionButton control)
    {
        if (!TracePopup || control.Name != "Graphics_renderer") return;
        PopupMenu popup = control.GetPopup();
        popup.AboutToPopup += () => TracePopupBoundary("about-to-popup", popup);
        popup.VisibilityChanged += () => TracePopupBoundary("visibility-changed", popup);
        popup.PopupHide += () => TracePopupBoundary("popup-hide", popup);
        popup.TreeExited += () => TracePopupBoundary("popup-tree-exited", popup);
        control.ItemSelected += _ => TracePopupBoundary("item-selected-after-refresh", popup);
        _window.FocusEntered += () => TracePopupBoundary("root-focus-entered", popup);
        _dialog.FocusEntered += () => TracePopupBoundary("dialog-focus-entered", popup);
    }
    internal void TracePopupObservation()
    {
        if (TracePopup) TracePopupBoundary("ui-observation", _graphicsControls["renderer"].GetPopup());
    }
    private void TracePopupBoundary(string boundary, PopupMenu popup)
    {
        // Read-only, opt-in, owned popup diagnosis. Preserve native Variant callable
        // identity when querying membership; managed Callable cannot represent every
        // C++ custom callable. An empty release-build name is not invalidity proof.
        object WindowState(Window window) => new
        {
            Name = window.Name.ToString(),
            Id = window.GetInstanceId(),
            Parent = window.GetParent()?.GetInstanceId(),
            Alive = IsInstanceValid(window),
            InTree = window.IsInsideTree(),
            Queued = window.IsQueuedForDeletion(),
            Visible = window.Visible,
            Embedded = window.IsEmbedded(),
            ForceNative = window.ForceNative,
            EmbedChildren = window.GuiEmbedSubwindows,
            Focused = window.HasFocus(),
            window.Transient,
            window.Exclusive,
            FocusConnections = Connections(window, Window.SignalName.FocusEntered),
            ExitConnections = Connections(window, Node.SignalName.TreeExited)
        };
        GD.Print("ODOT_POPUP " + JsonSerializer.Serialize(new
        {
            Boundary = boundary,
            At = DateTimeOffset.UtcNow,
            Process = System.Environment.ProcessId,
            Engine = Engine.GetVersionInfo()["string"].AsString(),
            Debug = OS.IsDebugBuild(),
            Editor = OS.HasFeature("editor"),
            Method = RenderingServer.GetCurrentRenderingMethod(),
            FocusOwner = _dialog.GuiGetFocusOwner()?.GetInstanceId(),
            Root = WindowState(_window),
            Dialog = WindowState(_dialog),
            Popup = WindowState(popup),
            NativeMenu = popup.IsNativeMenu(),
            Item = popup.GetFocusedItem(),
            Draft = _draftGraphics["renderer"]
        }));
    }
    private static object[] Connections(Window window, StringName signal)
    {
        return window.GetSignalConnectionList(signal).Select(connection => new
        {
            Callable = connection["callable"].ToString(),
            Type = connection["callable"].VariantType.ToString(),
            Connected = window.Call("is_connected", signal, connection["callable"]).AsBool(),
            Flags = connection["flags"].AsInt64()
        }).Cast<object>().ToArray();
    }
}
