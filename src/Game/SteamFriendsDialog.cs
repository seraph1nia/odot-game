using Game.Core;
using Godot;

namespace Game;

internal sealed partial class SteamFriendsDialog : AcceptDialog
{
    private VBoxContainer _rows = null!;
    private ScrollContainer _scroll = null!;
    private Label _status = null!;
    private Button _refresh = null!;
    private readonly Dictionary<ulong, Button> _buttons = [];
    private Func<SteamFriend[]>? _query;
    private Func<ulong, string>? _send;
    private Action? _closed;
    private Control? _returnFocus;
    private bool _available = true, _busy;
    private long _opening;

    internal bool IsOpen => Visible;
    internal string Status => _status.Text;
    internal int FriendCount => _buttons.Count;
    internal Action? FocusFallback { get; set; }

    public override void _Ready()
    {
        Title = "Invite Steam friends"; OkButtonText = "Close";
        Transient = true; Exclusive = false; DialogCloseOnEscape = true;
        var content = new VBoxContainer(); content.AddThemeConstantOverride("separation", 10); AddChild(content);
        content.AddChild(new Label { Text = "Your friend needs their own copy of Odot running.", CustomMinimumSize = new(400, 0), AutowrapMode = TextServer.AutowrapMode.WordSmart });
        _refresh = new Button { Name = "RefreshFriends", Text = "Refresh", CustomMinimumSize = new(0, 38) }; content.AddChild(_refresh);
        _refresh.Pressed += Refresh;
        _scroll = new ScrollContainer { CustomMinimumSize = new(0, 230), SizeFlagsVertical = Control.SizeFlags.ExpandFill, HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, VerticalScrollMode = ScrollContainer.ScrollMode.Auto, FollowFocus = true };
        content.AddChild(_scroll);
        _rows = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; _scroll.AddChild(_rows);
        _status = new Label { Name = "InviteStatus", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new(400, 55) }; content.AddChild(_status);
        GetOkButton().Name = "CloseFriends";
        UiAssets.Decorate(GetOkButton(), "Back");
        VisibilityChanged += () =>
        {
            if (Visible) return;
            _opening++; _query = null; _send = null; _busy = false;
            Action? closed = _closed; _closed = null; closed?.Invoke();
            Control? focus = _returnFocus; _returnFocus = null;
            Callable.From(() =>
            {
                if (focus is not null && GodotObject.IsInstanceValid(focus) && focus.IsVisibleInTree()) focus.GrabFocus();
                else FocusFallback?.Invoke();
            }).CallDeferred();
        };
    }

    internal void Open(Func<SteamFriend[]> query, Func<ulong, string> send, Action closed)
    {
        Close(); _opening++;
        _query = query; _send = send; _closed = closed;
        _returnFocus = GetParent().GetViewport().GuiGetFocusOwner();
        _available = true; _busy = false;
        PopupCenteredClamped(new(560, 420), 0.9f);
        Refresh();
        _refresh.GrabFocus();
    }

    internal void Close() { if (Visible) Hide(); }

    private void Refresh()
    {
        if (_query is null || _busy) return;
        foreach (Node row in _rows.GetChildren()) { _rows.RemoveChild(row); row.QueueFree(); }
        _buttons.Clear(); _scroll.ScrollVertical = 0;
        try
        {
            SteamFriend[] list = _query().DistinctBy(friend => friend.Id).OrderByDescending(friend => friend.Online)
                .ThenBy(friend => friend.Name, StringComparer.OrdinalIgnoreCase).ThenBy(friend => friend.Id).ToArray();
            var duplicateNames = list.GroupBy(friend => friend.Name, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1).Select(group => group.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var collidingSuffixes = list.GroupBy(friend => (friend.Name.ToUpperInvariant(), friend.Id % 1000000))
                .Where(group => group.Count() > 1).Select(group => group.Key).ToHashSet();
            foreach (SteamFriend friend in list)
            {
                var row = new HBoxContainer(); _rows.AddChild(row);
                string name = friend.Name;
                if (duplicateNames.Contains(friend.Name)) name = collidingSuffixes.Contains((friend.Name.ToUpperInvariant(), friend.Id % 1000000))
                    ? $"{friend.Name} ({friend.Id})" : $"{friend.Name} (…{friend.Id % 1000000:D6})";
                var label = new Label
                {
                    Text = name + " — " + friend.Presence,
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                    ClipText = true,
                    TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
                    TooltipText = name + " — " + friend.Presence
                };
                row.AddChild(label);
                var button = new Button { Name = "InviteFriend" + friend.Id, Text = "Invite", CustomMinimumSize = new(82, 38) };
                row.AddChild(button); _buttons[friend.Id] = button;
                long opening = _opening;
                button.Pressed += () => Send(friend.Id, opening);
            }
            _status.Text = list.Length == 0 ? "No Steam friends found. Add a friend in Steam, then refresh." : "Choose a friend to invite to your game.";
            _available = true; SetAvailable(true);
        }
        catch (Exception) { SetAvailable(false); }
        _refresh.GrabFocus();
    }

    private void Send(ulong recipient, long opening)
    {
        if (!Visible || opening != _opening || !_available || _busy || _send is null) return;
        _busy = true; UpdateButtons(); _status.Text = "Sending invitation…";
        // Keep repeated input queued in this frame from issuing a second native send.
        Callable.From(() =>
        {
            if (!Visible || opening != _opening || _send is null) return;
            _status.Text = _send(recipient); _busy = false; UpdateButtons();
            if (_buttons.TryGetValue(recipient, out Button? button) && !button.Disabled) button.GrabFocus();
        }).CallDeferred();
    }

    internal void SetAvailable(bool available)
    {
        if (!available) _status.Text = "Open Steam and log in, then refresh to invite friends.";
        // A refresh is required before re-enabling stale rows after login loss.
        if (!available || _available) _available = available;
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        foreach (Button button in _buttons.Values) button.Disabled = !_available || _busy;
        _refresh.Disabled = _busy;
    }

    internal void Observe(GameApplication application, Dictionary<string, object> targets)
    {
        if (!Visible) return;
        application.ObserveControl(targets, "RefreshFriends", _refresh);
        application.ObserveControl(targets, "CloseFriends", GetOkButton());
        foreach (var (id, button) in _buttons)
        {
            application.ObserveControl(targets, "InviteFriend" + id, button);
        }
    }

    internal string FocusedControl => GetViewport().GuiGetFocusOwner()?.Name.ToString() ?? "";
}
