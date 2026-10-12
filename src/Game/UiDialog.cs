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
    internal Func<WorldtreeDialog?>? ResearchHost { get; set; }
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
            if (!IsInsideTree()) return;
            UiDialog? foreground = ForegroundDialog();
            foreach (UiDialog dialog in GetParent().GetChildren().OfType<UiDialog>()) dialog.SetInputScope(dialog == foreground);
            ResearchHost?.Invoke()?.SetInputScope(foreground is null);
            Control? focus = GuiGetFocusOwner();
            if (!Visible && foreground is not null && (focus is null || !foreground.IsAncestorOf(focus)))
                (foreground.Confirmation ? foreground.GetCancelButton() : foreground.GetOkButton()).GrabFocus();
        };
        Hide();
    }
    internal Button GetOkButton() => _shell.Call("get_ok_button").As<Button>();
    internal Button GetCancelButton() => _shell.GetNode<Button>("Center/Panel/Body/Footer/Stay");
    internal Control? GuiGetFocusOwner() => GetViewport().GuiGetFocusOwner();
    internal void SetInputScope(bool enabled) => _shell.SetProcessInput(enabled && Visible);
    private UiDialog? ForegroundDialog() => GetParent().GetChildren().OfType<UiDialog>().LastOrDefault(dialog => dialog.IsVisibleInTree());
    internal bool CanRestoreFocus() => GodotObject.IsInstanceValid(this) && IsInsideTree() && !IsQueuedForDeletion() && ForegroundDialog() is null && ResearchHost?.Invoke()?.IsVisibleInTree() != true;
    internal void PopupCenteredClamped(Vector2I minimum, float ratio = .9f)
    {
        Control? invoker = Invoker ?? GetViewport().GuiGetFocusOwner();
        if (invoker is not null && (!GodotObject.IsInstanceValid(invoker) || invoker.IsQueuedForDeletion() || !invoker.IsInsideTree()))
            invoker = null;
        _shell.Set("title", Title);
        _shell.Set("ok_button_text", OkButtonText);
        _shell.Set("invoker", invoker is null ? default(Variant) : Variant.From(invoker));
        GetCancelButton().Text = CancelButtonText;
        // GUI hit testing follows sibling order, not CanvasItem z_index. Settings
        // is created before the menu; each active overlay must own the front scope.
        MoveToFront();
        Show();
        ReflowCenteredClamped(minimum, ratio);
        _shell.Call("popup_centered_clamped", (Vector2I)Panel.CustomMinimumSize, ratio);
        GetCancelButton().Visible = Confirmation;
    }
    public override void _Input(InputEvent @event)
    {
        if (!IsVisibleInTree() || !_shell.IsProcessingInput() || ForegroundDialog() != this || _shell.Call("owns_popup", _shell).AsBool()) return;
        Control? focus = GuiGetFocusOwner();
        if (focus is null || !IsAncestorOf(focus))
        {
            if (@event is not InputEventKey) return;
            focus = Confirmation ? GetCancelButton() : GetOkButton();
            focus.GrabFocus();
        }
        foreach (var (action, side) in new[] { ("ui_left", Side.Left), ("ui_right", Side.Right), ("ui_up", Side.Top), ("ui_down", Side.Bottom) })
        {
            if (!@event.IsActionPressed(action, allowEcho: true)) continue;
            if (focus.GetFocusNeighbor(side).ToString() == ".") focus.SetFocusNeighbor(side, "");
            Control? neighbor = focus.FindValidFocusNeighbor(side);
            if (neighbor is not null && !IsAncestorOf(neighbor)) focus.SetFocusNeighbor(side, ".");
        }
    }
    internal void ReflowCenteredClamped(Vector2I minimum, float ratio = .9f)
    {
        Vector2 bound = ((Vector2)minimum).Min(GetViewportRect().Size * ratio);
        Scroll.CustomMinimumSize = new(0, Math.Max(80, bound.Y - 150 - FooterContent.GetCombinedMinimumSize().Y));
        Panel.CustomMinimumSize = bound;
    }
}
