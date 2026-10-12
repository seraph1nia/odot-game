using Game.Core;
using Godot;
using GDictionary = Godot.Collections.Dictionary;

namespace Game;

// Game-side adapter: the imported tree only emits intent. No acquired bits,
// currency, buffs or cached prices are ever mutated here.
public partial class WorldtreeResearch : VBoxContainer
{
    public Control Tree { get; private set; } = null!;
    public Label Description { get; private set; } = null!;
    public Label Feedback { get; private set; } = null!;
    public Action<TechnologyId>? Requested { get; set; }
    private MatchSnapshot? _state;
    private CityState? _city;
    private int _player;
    private bool _connected;
    private string _selected = "root", _detailsId = "root";
    private string[] _acquired = [];
    private string[] _available = [];
    private int _points = -1;
    private int _progress = -1;
    private (Phase? Phase, bool Paused, bool Ready, bool Fallen, int Player, bool Connected) _context;
    private string? _match;
    private int? _cityId;
    private PanelContainer _hoverDetails = null!;
    private Button? _detailsNode;
    private bool _restoringFocus;
    internal bool HoverDetailsVisible => _hoverDetails.Visible;
    internal Rect2 HoverDetailsRect => _hoverDetails.GetGlobalTransformWithCanvas() * new Rect2(Vector2.Zero, _hoverDetails.Size);
    public override void _Ready()
    {
        Tree = GD.Load<PackedScene>("res://UI/components/worldtree.tscn").Instantiate<Control>();
        AddChild(Tree);
        // Preserve pinned imported bytes and references, but remove their duplicate
        // footer from the visible composition. One shared hover/focus surface
        // owns both meaningful details and the existing purchase intent button.
        Tree.GetNode<Control>("SelectedNodeFooter").Hide();
        Tree.Get("points_label").As<Label>().Hide();
        _hoverDetails = new PanelContainer { Name = "ResearchHoverDetails", TopLevel = true, ZIndex = 20, CustomMinimumSize = new(300, 0), MouseFilter = MouseFilterEnum.Ignore };
        Tree.AddChild(_hoverDetails);
        var body = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore }; _hoverDetails.AddChild(body);
        Description = new Label { Name = "ResearchDescription", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(300, 0), MouseFilter = MouseFilterEnum.Ignore };
        body.AddChild(Description);
        Feedback = new Label { Name = "ResearchFeedback", AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore }; body.AddChild(Feedback);
        Button action = Tree.Get("action").As<Button>();
        action.Reparent(body);
        // The pinned component's button requests its selected node. Hover/focus
        // must not select under a covering modal; bind this same intent signal to
        // the currently described node instead, still guarded by authority.
        action.Disconnect(BaseButton.SignalName.Pressed, new Callable(Tree, "request_action"));
        action.Pressed += () => Tree.EmitSignal("node_action_requested", _detailsId);
        foreach (WorldtreeSlot slot in WorldtreeProjection.Slots)
        {
            Button button = Tree.Get("buttons").AsGodotDictionary()[slot.VisualId].As<Button>();
            button.MouseEntered += () => ShowDetails(slot.VisualId, button);
            button.FocusEntered += () => { if (!_restoringFocus) ShowDetails(slot.VisualId, button); };
        }
        _hoverDetails.Hide();
        Tree.Connect("node_selected", Callable.From<string>(id =>
        {
            _selected = _detailsId = id; DescribeSelection();
            _detailsNode = Tree.Get("buttons").AsGodotDictionary()[id].As<Button>();
            _hoverDetails.Show(); PlaceDetails();
        }));
        Tree.Connect("node_action_requested", Callable.From<string>(id =>
        {
            if (WorldtreeProjection.Find(id) is { Technology: not TechnologyId.None } slot
                && ProgressionPresentation.ResearchAccess(_state, _city, _player, _connected, slot.Technology).Available)
                Requested?.Invoke(slot.Technology);
        }));
        Tree.Get("action").As<Button>().Name = "PurchaseResearch";
        Update(null, null, 0, false);
    }
    public void Update(MatchSnapshot? state, CityState? city, int player, bool connected, string feedback = "")
    {
        Feedback.Text = feedback; Feedback.Visible = feedback.Length != 0;
        bool sameCity = _match == state?.MatchId && _cityId == city?.Id;
        _state = state; _city = city; _player = player; _connected = connected;
        WorldtreeState projected = WorldtreeProjection.Project(state, city, player, connected, _selected);
        var context = (state?.Phase, state?.Paused ?? false, city?.Ready ?? false, city?.Eliminated ?? false, player, connected);
        // Numerical ticks can change Revision without any research presentation
        // change. Do not replace identical snapshots: replacement kills the
        // imported component's bound child tween by contract.
        if (sameCity && _context == context && _points == projected.Points && _progress == (city?.Research.Progress ?? 0)
            && _acquired.SequenceEqual(projected.Acquired) && _available.SequenceEqual(projected.Available)) return;
        // Animate only an actual newly acknowledged purchase in this same city.
        // Reconnect/city changes reconstruct complete state, never replay history.
        string[] added = projected.Acquired.Except(_acquired).ToArray();
        string animate = sameCity && added.Length == 1 && WorldtreeProjection.Find(added[0])?.Technology != TechnologyId.None ? added[0] : "";
        var icons = new GDictionary();
        foreach (WorldtreeSlot slot in WorldtreeProjection.Slots) icons[slot.VisualId] = slot.Icon;
        var snapshot = new GDictionary
        {
            ["acquired"] = Strings(projected.Acquired),
            ["available"] = Strings(projected.Available),
            ["icons"] = icons,
            ["selected"] = projected.Selected,
            ["points"] = projected.Points
        };
        if (!Tree.Call("set_data", snapshot, animate).AsBool()) throw new InvalidOperationException("Invalid authoritative Worldtree projection.");
        _acquired = projected.Acquired; _available = projected.Available; _points = projected.Points; _progress = city?.Research.Progress ?? 0;
        _context = context; _match = state?.MatchId; _cityId = city?.Id;
        DescribeSelection();
    }
    private static Godot.Collections.Array Strings(IEnumerable<string> values)
    {
        var result = new Godot.Collections.Array(); foreach (string value in values) result.Add(value); return result;
    }
    private void DescribeSelection()
    {
        Description.Text = WorldtreeProjection.Description(_detailsId, _state, _city, _player, _connected);
        var action = Tree.Get("action").As<Button>(); action.Text = "Research";
        TechnologyId technology = WorldtreeProjection.Find(_detailsId)?.Technology ?? TechnologyId.None;
        action.Disabled = technology == TechnologyId.None || !ProgressionPresentation.ResearchAccess(_state, _city, _player, _connected, technology).Available;
        action.FocusMode = action.Disabled ? FocusModeEnum.None : FocusModeEnum.All;
        action.MouseFilter = action.Disabled ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;
        // Suppress native pointer tooltips: this surface is the only details route.
        for (int i = 0; i < WorldtreeProjection.Slots.Count; i++)
        {
            var slot = WorldtreeProjection.Slots[i];
            Button button = Tree.Get("buttons").AsGodotDictionary()[slot.VisualId].As<Button>();
            button.TooltipText = "";
            button.FocusNext = i + 1 < WorldtreeProjection.Slots.Count
                ? Tree.Get("buttons").AsGodotDictionary()[WorldtreeProjection.Slots[i + 1].VisualId].As<Button>().GetPath() : new NodePath();
            if (slot.VisualId == _detailsId && !action.Disabled)
            {
                action.FocusPrevious = button.GetPath(); action.FocusNext = button.FocusNext;
                button.FocusNext = action.GetPath();
            }
        }
    }
    internal void FocusRoot(bool preserveSelection)
    {
        _restoringFocus = preserveSelection;
        Tree.Get("buttons").AsGodotDictionary()["root"].As<Button>().GrabFocus();
        _restoringFocus = false;
    }
    private void ShowDetails(string id, Button button)
    {
        _detailsNode = button; _detailsId = id;
        DescribeSelection();
        _hoverDetails.Show();
        PlaceDetails();
    }
    private void PlaceDetails()
    {
        if (_detailsNode is null) return;
        _hoverDetails.ResetSize();
        Rect2 node = _detailsNode.GetGlobalRect(), viewport = _hoverDetails.GetCanvasTransform().AffineInverse() * GetViewport().GetVisibleRect();
        Vector2 position = new(node.End.X + 8, node.Position.Y);
        if (position.X + _hoverDetails.Size.X > viewport.End.X - 16) position.X = node.Position.X - _hoverDetails.Size.X - 8;
        _hoverDetails.GlobalPosition = position.Clamp(viewport.Position + Vector2.One * 16,
            (viewport.End - _hoverDetails.Size - Vector2.One * 16).Max(viewport.Position + Vector2.One * 16));
    }
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree() || !_hoverDetails.Visible || _detailsNode is null) return;
        PlaceDetails();
        Control? focus = GetViewport().GuiGetFocusOwner();
        bool focused = focus == _detailsNode || focus is not null && _hoverDetails.IsAncestorOf(focus);
        Rect2 socket = _detailsNode.GetGlobalTransformWithCanvas() * new Rect2(Vector2.Zero, _detailsNode.Size);
        Rect2 node = socket;
        for (Node? ancestor = _detailsNode.GetParent(); ancestor is not null; ancestor = ancestor.GetParent())
            if (ancestor is Control { ClipContents: true } clip)
                node = node.Intersection(clip.GetGlobalTransformWithCanvas() * new Rect2(Vector2.Zero, clip.Size));
        Rect2 panel = HoverDetailsRect;
        Vector2 pointer = GetViewport().GetMousePosition();
        Vector2 start = new(Math.Min(socket.End.X, panel.End.X), Math.Max(node.Position.Y, panel.Position.Y));
        Vector2 end = new(Math.Max(socket.Position.X, panel.Position.X), Math.Min(node.End.Y, panel.End.Y));
        bool transit = node.HasArea() && end.X > start.X && end.Y > start.Y && new Rect2(start, end - start).HasPoint(pointer);
        if (!focused && !node.HasPoint(pointer) && !panel.HasPoint(pointer) && !transit) _hoverDetails.Hide();
    }
}

// Native Control host with modal confinement. The component itself keeps its
// unconfined host-boundary traversal; only this visible game modal owns Tab/Esc.
public partial class WorldtreeDialog : Control
{
    public WorldtreeResearch Research { get; private set; } = null!;
    public Button CloseButton { get; private set; } = null!;
    public Control? Invoker { get; set; }
    private PanelContainer _panel = null!;
    private bool _suspended;
    private Control? _suspendedFocus;
    internal void SetInputScope(bool enabled)
    {
        if (!enabled)
        {
            if (_suspended || !IsVisibleInTree()) return;
            _suspended = true;
            _suspendedFocus = GetViewport().GuiGetFocusOwner();
            Hide();
        }
        else if (_suspended)
        {
            _suspended = false;
            Control? focus = _suspendedFocus;
            _suspendedFocus = null;
            if (!IsInsideTree() || IsQueuedForDeletion()) return;
            Show();
            if (focus is not null && IsInstanceValid(focus) && !focus.IsQueuedForDeletion() && focus.IsVisibleInTree() && IsAncestorOf(focus)
                && focus.FocusMode != FocusModeEnum.None && focus is not BaseButton { Disabled: true }) focus.GrabFocus();
            else FocusContent(preserveSelection: true);
        }
    }
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        var scrim = new ColorRect { Color = new Color(0, 0, 0, .7f), MouseFilter = MouseFilterEnum.Stop }; AddChild(scrim); scrim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _panel = new PanelContainer(); AddChild(_panel);
        var body = new VBoxContainer(); _panel.AddChild(body);
        var header = new HBoxContainer(); body.AddChild(header);
        header.AddChild(new Label { Text = "Worldtree · city research", SizeFlagsHorizontal = SizeFlags.ExpandFill });
        CloseButton = new Button { Name = "CloseResearch", Text = "Close" }; header.AddChild(CloseButton); CloseButton.Pressed += Close;
        var scroll = new ScrollContainer { FollowFocus = true, SizeFlagsVertical = SizeFlags.ExpandFill }; body.AddChild(scroll);
        Research = new WorldtreeResearch { SizeFlagsHorizontal = SizeFlags.ExpandFill }; scroll.AddChild(Research);
        Resized += Layout;
        Layout(); Hide();
    }
    private void Layout()
    {
        if (_panel is null) return;
        Vector2 dimensions = new(Math.Min(1120, Math.Max(200, Size.X - 32)), Math.Min(800, Math.Max(200, Size.Y - 32)));
        _panel.Size = dimensions; _panel.Position = (Size - dimensions) / 2;
    }
    public void Open() { Show(); MoveToFront(); Layout(); FocusContent(); }
    public void Close()
    {
        _suspended = false;
        _suspendedFocus = null;
        Hide();
        if (Invoker is not null && GodotObject.IsInstanceValid(Invoker) && Invoker.IsVisibleInTree() && GetWindow().HasFocus()) Invoker.GrabFocus();
    }
    private void FocusContent(bool preserveSelection = false) => Research.FocusRoot(preserveSelection);
    public override void _Input(InputEvent @event)
    {
        if (!IsVisibleInTree() || @event is not InputEventKey { Pressed: true } key) return;
        Control? focus = GetViewport().GuiGetFocusOwner();
        if (focus is null || !IsAncestorOf(focus))
        {
            FocusContent();
            focus = GetViewport().GuiGetFocusOwner();
        }
        foreach (var (action, side) in new[] { ("ui_left", Side.Left), ("ui_right", Side.Right), ("ui_up", Side.Top), ("ui_down", Side.Bottom) })
        {
            if (!key.IsActionPressed(action, allowEcho: true) || focus is null) continue;
            if (focus.GetFocusNeighbor(side).ToString() == ".") focus.SetFocusNeighbor(side, "");
            Control? neighbor = focus.FindValidFocusNeighbor(side);
            if (neighbor is not null && !IsAncestorOf(neighbor)) focus.SetFocusNeighbor(side, ".");
        }
        if (key.Echo) return;
        if (key.IsActionPressed("ui_cancel")) { Close(); GetViewport().SetInputAsHandled(); return; }
        bool forward = key.IsActionPressed("ui_focus_next"), backward = key.IsActionPressed("ui_focus_prev");
        if (!forward && !backward) return;
        Control[] controls = FindChildren("*", "Control", true, false).OfType<Control>().Where(c => c.IsVisibleInTree() && c.FocusMode == FocusModeEnum.All && c is not BaseButton { Disabled: true }).ToArray();
        int current = Array.IndexOf(controls, GetViewport().GuiGetFocusOwner());
        Control? next = backward ? focus?.FindPrevValidFocus() : focus?.FindNextValidFocus();
        if (next is not null && IsAncestorOf(next)) next.GrabFocus();
        else if (controls.Length > 0) controls[(current + (backward ? -1 : 1) + controls.Length) % controls.Length].GrabFocus();
        GetViewport().SetInputAsHandled();
    }
}
