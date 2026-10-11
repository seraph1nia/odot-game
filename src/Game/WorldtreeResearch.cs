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
    public Func<bool>? RequestBlocked { get; set; }
    private MatchSnapshot? _state;
    private CityState? _city;
    private int _player;
    private bool _connected;
    private string _selected = "root";
    private string[] _acquired = [];
    private string[] _available = [];
    private int _points = -1;
    private int _progress = -1;
    private (Phase? Phase, bool Paused, bool Ready, bool Fallen, int Player, bool Connected) _context;
    private string? _match;
    private int? _cityId;
    public override void _Ready()
    {
        Tree = GD.Load<PackedScene>("res://UI/components/worldtree.tscn").Instantiate<Control>();
        AddChild(Tree);
        Description = new Label { Name = "ResearchDescription", AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsHorizontal = SizeFlags.ExpandFill, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(Description);
        Feedback = new Label { Name = "ResearchFeedback", AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore }; AddChild(Feedback);
        Tree.Connect("node_selected", Callable.From<string>(id => { _selected = id; DescribeSelection(); }));
        Tree.Connect("node_action_requested", Callable.From<string>(id =>
        {
            if (RequestBlocked?.Invoke() != true && WorldtreeProjection.Find(id) is { Technology: not TechnologyId.None } slot
                && ProgressionPresentation.ResearchAccess(_state, _city, _player, _connected, slot.Technology).Available)
                Requested?.Invoke(slot.Technology);
        }));
        Tree.Get("action").As<Button>().Name = "PurchaseResearch";
        Update(null, null, 0, false);
    }
    public void Update(MatchSnapshot? state, CityState? city, int player, bool connected, string feedback = "")
    {
        Feedback.Text = feedback;
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
        Description.Text = WorldtreeProjection.Description(_selected, _state, _city, _player, _connected);
        Tree.Get("points_label").As<Label>().Text = $"Research points  {_city?.Research.Points ?? 0} · progress {_city?.Research.Progress ?? 0}/3";
        var footer = Tree.Get("footer").As<Label>();
        TechnologyId technology = WorldtreeProjection.Find(_selected)?.Technology ?? TechnologyId.None;
        var node = _state?.TechnologyCatalog.FirstOrDefault(n => n.Id == technology);
        footer.Text = WorldtreeProjection.Title(_selected) + (node is null ? "" : $" · {node.Cost} research points") + "\n"
            + (technology == TechnologyId.None ? _selected is "root" or "trunk_1" or "trunk_2" ? "Established system · not a purchase" : "Inactive slot · not purchasable"
                : ProgressionPresentation.ResearchAccess(_state, _city, _player, _connected, technology).Reason);
        var action = Tree.Get("action").As<Button>(); action.Text = "Research";
        foreach (WorldtreeSlot slot in WorldtreeProjection.Slots)
            Tree.Get("buttons").AsGodotDictionary()[slot.VisualId].As<Button>().TooltipText = WorldtreeProjection.Description(slot.VisualId, _state, _city, _player, _connected);
    }
}

// Native Control host with modal confinement. The component itself keeps its
// unconfined host-boundary traversal; only this visible game modal owns Tab/Esc.
public partial class WorldtreeDialog : Control
{
    public WorldtreeResearch Research { get; private set; } = null!;
    public Button CloseButton { get; private set; } = null!;
    public Button SettingsButton { get; private set; } = null!;
    public Control? Invoker { get; set; }
    public Func<bool>? ChildModalOpen { get; set; }
    public Action? OpenSettings { get; set; }
    private PanelContainer _panel = null!;
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        var scrim = new ColorRect { Color = new Color(0, 0, 0, .7f), MouseFilter = MouseFilterEnum.Stop }; AddChild(scrim); scrim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _panel = new PanelContainer(); AddChild(_panel);
        var body = new VBoxContainer(); _panel.AddChild(body);
        var header = new HBoxContainer(); body.AddChild(header);
        header.AddChild(new Label { Text = "Worldtree · city research", SizeFlagsHorizontal = SizeFlags.ExpandFill });
        SettingsButton = new Button { Name = "ResearchSettings", Text = "Settings" }; header.AddChild(SettingsButton); SettingsButton.Pressed += () => OpenSettings?.Invoke();
        CloseButton = new Button { Name = "CloseResearch", Text = "Close" }; header.AddChild(CloseButton); CloseButton.Pressed += () => { if (ChildModalOpen?.Invoke() != true) Close(); };
        var scroll = new ScrollContainer { FollowFocus = true, SizeFlagsVertical = SizeFlags.ExpandFill }; body.AddChild(scroll);
        Research = new WorldtreeResearch { SizeFlagsHorizontal = SizeFlags.ExpandFill, RequestBlocked = () => ChildModalOpen?.Invoke() == true }; scroll.AddChild(Research);
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
        Hide();
        if (Invoker is not null && GodotObject.IsInstanceValid(Invoker) && Invoker.IsVisibleInTree() && GetWindow().HasFocus()) Invoker.GrabFocus();
    }
    public void FocusContent()
    {
        if (ChildModalOpen?.Invoke() == true) return;
        Research.Tree.Get("buttons").AsGodotDictionary()["root"].As<Control>().GrabFocus();
    }
    public override void _Input(InputEvent @event)
    {
        if (!Visible || ChildModalOpen?.Invoke() == true || @event is not InputEventKey { Pressed: true, Echo: false } key) return;
        if (key.IsActionPressed("ui_cancel")) { Close(); GetViewport().SetInputAsHandled(); return; }
        bool forward = key.IsActionPressed("ui_focus_next"), backward = key.IsActionPressed("ui_focus_prev");
        if (!forward && !backward) return;
        Control[] controls = FindChildren("*", "Control", true, false).OfType<Control>().Where(c => c.IsVisibleInTree() && c.FocusMode == FocusModeEnum.All && c is not BaseButton { Disabled: true }).ToArray();
        int current = Array.IndexOf(controls, GetViewport().GuiGetFocusOwner());
        if (controls.Length > 0) controls[(current + (backward ? -1 : 1) + controls.Length) % controls.Length].GrabFocus();
        GetViewport().SetInputAsHandled();
    }
}
