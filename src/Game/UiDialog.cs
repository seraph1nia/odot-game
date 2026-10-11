using Godot;

namespace Game;

// Game-owned adapter to the production Control modal. No service or authority lives
// in the kit; callers retain cancellation, persistence, lifetime and world guards.
internal partial class UiDialog : Control
{
    private Control _shell = null!;
    private Label _message = null!;
    private bool _synchronizing;
    private string _dialogText = "";
    internal string Title { get; set; } = "Dialog";
    internal string OkButtonText { get; set; } = "Close";
    internal string CancelButtonText { get; set; } = "Cancel";
    internal bool Confirmation { get; set; }
    internal Control? Invoker { get; set; }
    internal event Action? Confirmed;
    internal event Action? Canceled;
    internal VBoxContainer FooterContent { get; private set; } = null!;
    internal VBoxContainer Content => _shell.Call("get_content").As<VBoxContainer>();
    internal ScrollContainer Scroll => _shell.Call("get_scroll").As<ScrollContainer>();
    internal PanelContainer Panel => _shell.Call("get_panel").As<PanelContainer>();
    internal Button HeaderClose => _shell.GetNode<Button>("Center/Panel/Body/Header/Cancel");
    internal string DialogText
    {
        get => _dialogText;
        set
        {
            _dialogText = value;
            if (_message is not null) { _message.Text = value; _message.Visible = value.Length != 0; }
        }
    }
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _shell = UiAssets.Component("modal", this);
        // Always emit intent before closure: Apply must precede Cancel-on-hide.
        _shell.Set("confirmation", true);
        _shell.Connect("confirmed", Callable.From(() => Confirmed?.Invoke()));
        _shell.Connect("canceled", Callable.From(() => Canceled?.Invoke()));
        _shell.Connect("closed", Callable.From(Hide));
        _message = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, Visible = false, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        Content.AddChild(_message);
        var body = _shell.GetNode<VBoxContainer>("Center/Panel/Body");
        FooterContent = new VBoxContainer { Name = "GameFooter", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        body.AddChild(FooterContent); body.MoveChild(FooterContent, body.GetChildCount() - 2);
        DialogText = _dialogText;
        VisibilityChanged += () =>
        {
            if (!Visible && !_synchronizing)
            {
                _synchronizing = true;
                _shell.Call("close");
                _synchronizing = false;
            }
        };
        Hide();
    }
    internal Button GetOkButton() => _shell.Call("get_ok_button").As<Button>();
    internal Button GetCancelButton() => _shell.GetNode<Button>("Center/Panel/Body/Footer/Stay");
    internal Control? GuiGetFocusOwner() => GetViewport().GuiGetFocusOwner();
    internal void SetInputScope(bool enabled) => _shell.SetProcessInput(enabled && Visible);
    internal void PopupCenteredClamped(Vector2I minimum, float ratio = .9f)
    {
        Invoker ??= GetViewport().GuiGetFocusOwner();
        _shell.Set("title", Title);
        _shell.Set("ok_button_text", OkButtonText);
        _shell.Set("invoker", Invoker);
        GetCancelButton().Text = CancelButtonText;
        // GUI hit testing follows sibling order, not CanvasItem z_index. Settings
        // is created before the menu; each active overlay must own the front scope.
        MoveToFront();
        Show();
        Vector2 bound = ((Vector2)minimum).Min(GetViewportRect().Size * ratio);
        // A bounded body, independent header/footer; never grow over hit targets.
        Scroll.CustomMinimumSize = new(0, Math.Max(80, bound.Y - 150 - FooterContent.GetCombinedMinimumSize().Y));
        _shell.Call("popup_centered_clamped", (Vector2I)bound, ratio);
        GetCancelButton().Visible = Confirmation;
    }
}
