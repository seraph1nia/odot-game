using Godot;

namespace Game;

// Verification-only stripped native controls. No ClientSettings/GameApplication,
// material, match, draft, refresh or persistence handlers participate.
public partial class VanillaPopupFixture : Node
{
    private AcceptDialog _dialog = null!;
    private OptionButton _choice = null!;
    private Button _open = null!, _exit = null!;
    private int _selected;
    internal Action ExitRequested { get; init; } = null!;
    public override void _Ready()
    {
        Window root = GetWindow(); root.Title = "The Common Watch — owned vanilla popup diagnostic";
        root.GuiEmbedSubwindows = true;
        root.Msaa3D = Viewport.Msaa.Msaa4X;
        var settings = new Node { Name = "ClientSettings" }; AddChild(settings);
        _dialog = new AcceptDialog { Name = "SettingsDialog", Title = "Vanilla popup diagnostic", Transient = true, Exclusive = false, OkButtonText = "Close" };
        settings.AddChild(_dialog);
        var content = new VBoxContainer(); _dialog.AddChild(content);
        var tabs = new TabContainer { CustomMinimumSize = new(0, 180) }; content.AddChild(tabs);
        var page = new MarginContainer { Name = "Graphics" }; tabs.AddChild(page);
        var scroll = new ScrollContainer { FollowFocus = true, CustomMinimumSize = new(0, 360) }; page.AddChild(scroll);
        var column = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; scroll.AddChild(column);
        _choice = new OptionButton { Name = "Graphics_renderer", CustomMinimumSize = new(280, 32) }; column.AddChild(_choice);
        _choice.AddItem("Forward+ / Vulkan"); _choice.AddItem("Compatibility / OpenGL");
        _choice.GetPopup().ForceNative = System.Environment.GetEnvironmentVariable("ODOT_VANILLA_POPUP_NATIVE") == "1";
        // Sole observation receipt: no selection/refresh/focus/connection mutations.
        _choice.ItemSelected += index => _selected = (int)index;
        _open = new Button { Text = "Open vanilla settings", Position = new(12, 12) }; AddChild(_open);
        _open.Pressed += () => _dialog.PopupCenteredClamped(new(620, 650), .9f);
        _exit = new Button { Text = "Exit diagnostic", Position = new(12, 70) }; AddChild(_exit);
        _exit.Pressed += () => ExitRequested();
    }
    internal Dictionary<string, object?> Observe(string id, string? screenshot, int colors)
    {
        Window root = GetWindow(); PopupMenu popup = _choice.GetPopup();
        object Target(Control control)
        {
            Rect2 rect = control.GetGlobalRect(); Vector2 center = rect.GetCenter();
            if (control.GetViewport() is Window window && window != root) center += window.Position;
            center = root.GetFinalTransform() * center;
            return new { X = center.X, Y = center.Y, Width = rect.Size.X, Height = rect.Size.Y, Visible = control.IsVisibleInTree(), Enabled = true };
        }
        object State(Window window) => new
        {
            Id = window.GetInstanceId(),
            Parent = window.GetParent()?.GetInstanceId(),
            Alive = IsInstanceValid(window),
            InTree = window.IsInsideTree(),
            Queued = window.IsQueuedForDeletion(),
            window.Visible,
            Embedded = window.IsEmbedded(),
            window.GuiEmbedSubwindows,
            Focus = window.GetSignalConnectionList(Window.SignalName.FocusEntered).Count,
            Exit = window.GetSignalConnectionList(Node.SignalName.TreeExited).Count
        };
        GD.Print("ODOT_VANILLA_POPUP " + System.Text.Json.JsonSerializer.Serialize(new
        {
            At = DateTimeOffset.UtcNow,
            Engine = Engine.GetVersionInfo()["string"].AsString(),
            Debug = OS.IsDebugBuild(),
            Editor = OS.HasFeature("editor"),
            Root = State(root),
            Dialog = State(_dialog),
            Popup = State(popup)
        }));
        return new()
        {
            ["Id"] = id,
            ["Screenshot"] = screenshot,
            ["Colors"] = colors,
            ["Width"] = root.Size.X,
            ["Height"] = root.Size.Y,
            ["ProcessId"] = System.Environment.ProcessId,
            ["WindowFocused"] = root.HasFocus(),
            ["WindowMode"] = root.Mode.ToString(),
            ["NativeWindow"] = DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, root.GetWindowId()),
            ["SettingsOpen"] = _dialog.Visible,
            ["GraphicsDropdownOpen"] = popup.Visible,
            ["GraphicsPopupFocus"] = new Dictionary<string, int> { ["renderer"] = popup.GetFocusedItem() },
            ["GraphicsDraft"] = new Dictionary<string, int> { ["renderer"] = _selected },
            ["Targets"] = new Dictionary<string, object> { ["Settings"] = Target(_open), ["Graphics_renderer"] = Target(_choice), ["CloseVanilla"] = Target(_dialog.GetOkButton()), ["Exit"] = Target(_exit) },
            ["Graphics"] = new
            {
                Method = RenderingServer.GetCurrentRenderingMethod(),
                Driver = RenderingServer.GetCurrentRenderingDriverName(),
                Gpu = RenderingServer.GetVideoAdapterName(),
                DeviceType = RenderingServer.GetVideoAdapterType().ToString(),
                RenderingDevice = RenderingServer.GetRenderingDevice() is not null,
                Display = DisplayServer.GetName(),
                Msaa = root.Msaa3D.ToString()
            }
        };
    }
}
