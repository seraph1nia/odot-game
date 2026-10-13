using Game.Core;
using Godot;

namespace Game;

public partial class Tabletop
{
    private UiDialog _squadDialog = null!;
    private Button _openSquads = null!, _arenaToggle = null!, _reinforceSquad = null!, _newSquad = null!, _regroupMember = null!, _storeMember = null!, _releaseMember = null!, _retireMember = null!;
    private VBoxContainer _squadRoster = null!, _memberRoster = null!;
    private Label _squadSummary = null!, _memberSummary = null!;
    private OptionButton _recruitSource = null!, _regroupDestination = null!, _storeDestination = null!;
    private readonly Dictionary<int, Button> _squadButtons = [], _memberButtons = [];
    private int _selectedSquad, _selectedMember;
    private string _squadRosterKey = "";
    private bool _showDeployment;
    private SharedArenaView? _arenaView;
    private bool InArena => game.State?.Phase is Phase.Combat or Phase.Victory or Phase.Defeat || _showDeployment;
    private void CreateSquadControls(Node parent)
    {
        var row = new HBoxContainer(); parent.AddChild(row);
        _openSquads = Button(row, "Squads", () => { RefreshSquads(); _squadDialog.PopupCenteredClamped(new(760, 590), .9f); }); _openSquads.Name = "OpenSquads";
        _arenaToggle = Button(row, "Deployment", () => { _showDeployment = !_showDeployment; _frameFocus = -1; _slot = -1; CancelGesture(); }); _arenaToggle.Name = "ShowDeployment";
        _squadDialog = new UiDialog { Title = "Shared army · squads & veterans", Theme = _hudRoot.Theme, Invoker = _openSquads }; _ownHudDialog(_squadDialog);
        var body = _squadDialog.Content;
        _squadSummary = Text(body, "", 13);
        var lists = new HBoxContainer(); body.AddChild(lists);
        _squadRoster = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; lists.AddChild(_squadRoster);
        _memberRoster = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; lists.AddChild(_memberRoster);
        _memberSummary = Text(body, "Choose a squad or individual veteran.", 13);
        _recruitSource = new OptionButton { Name = "SquadRecruitSource" }; body.AddChild(_recruitSource);
        var recruitment = new HBoxContainer(); body.AddChild(recruitment);
        _reinforceSquad = Button(recruitment, "Paid reinforcement", () => RecruitSquad(false)); _reinforceSquad.Name = "ReinforceSquad";
        _newSquad = Button(recruitment, "Recruit new squad", () => RecruitSquad(true)); _newSquad.Name = "RecruitNewSquad";
        _regroupDestination = new OptionButton { Name = "RegroupDestination" }; body.AddChild(_regroupDestination);
        _regroupMember = Button(body, "Transfer selected member", () =>
        {
            if (CanEdit() && _selectedMember > 0 && _regroupDestination.ItemCount > 0)
                game.SendAction("regroup-member", unitId: _selectedMember, squadId: _regroupDestination.GetSelectedId() == 0 ? -1 : _regroupDestination.GetSelectedId());
        }); _regroupMember.Name = "RegroupMember";
        _storeDestination = new OptionButton { Name = "SquadStoreDestination" }; body.AddChild(_storeDestination);
        var memberActions = new HBoxContainer(); body.AddChild(memberActions);
        _storeMember = Button(memberActions, "Store veteran", () =>
        { if (CanEdit() && _selectedMember > 0 && _storeDestination.ItemCount > 0) game.SendAction("store", _storeDestination.GetSelectedId(), unitId: _selectedMember); }); _storeMember.Name = "SquadStoreMember";
        _releaseMember = Button(memberActions, "Return veteran", () =>
        {
            UnitState? member = Focus()?.Soldiers.FirstOrDefault(u => u.Id == _selectedMember);
            if (CanEdit() && member?.Assignment is { Stored: true } hall && _regroupDestination.ItemCount > 0)
                game.SendAction("send", hall.HallSlot, unitId: member.Id, squadId: _regroupDestination.GetSelectedId() == 0 ? -1 : _regroupDestination.GetSelectedId());
        }); _releaseMember.Name = "SquadReleaseMember";
        _retireMember = Button(memberActions, "Retire · no refund", () =>
        { if (CanEdit() && _selectedMember > 0) game.SendAction("retire", unitId: _selectedMember); }); _retireMember.Name = "SquadRetireMember";
        _squadDialog.VisibilityChanged += () => { if (_squadDialog.Visible) RefreshSquads(); else if (_squadDialog.CanRestoreFocus() && _openSquads.IsVisibleInTree()) _openSquads.GrabFocus(); };
        _recruitSource.ItemSelected += _ => RefreshSquads();
    }
    private void RecruitSquad(bool create)
    {
        if (!CanEdit() || game.State is not { } state || _recruitSource.ItemCount == 0) return;
        int encoded = _recruitSource.GetSelectedId(); int slot = encoded / 4; UnitType type = (UnitType)(encoded % 4);
        if (!create && state.Squads.FirstOrDefault(s => s.Id == _selectedSquad)?.Type != type) return;
        game.SendAction("recruit", slot, soldierType: type, squadId: create ? -1 : _selectedSquad);
    }
    internal void RefreshSquads()
    {
        if (_squadSummary is null || game.State is not { } state) return;
        _arenaToggle.Disabled = state.Phase is Phase.Lobby or Phase.Combat or Phase.Victory or Phase.Defeat;
        _arenaToggle.Text = _showDeployment ? "Village" : "Deployment";
        _openSquads.Disabled = state.Phase == Phase.Lobby;
        CityState? city = Focus();
        SquadState[] squads = state.Squads.Where(s => s.Owner == _focus && s.Members.Length > 0).ToArray();
        if (!squads.Any(s => s.Id == _selectedSquad)) { _selectedSquad = squads.FirstOrDefault()?.Id ?? 0; _selectedMember = 0; }
        SquadState? selected = squads.FirstOrDefault(s => s.Id == _selectedSquad);
        UnitState[] members = city?.Soldiers.Where(u => selected?.Members.Contains(u.Id) == true).OrderBy(u => u.Id).ToArray() ?? [];
        if (!members.Any(u => u.Id == _selectedMember)) _selectedMember = members.FirstOrDefault()?.Id ?? 0;
        string key = $"{state.MatchId}:{_focus}:{_selectedSquad}:{string.Join(',', squads.Select(s => s.Id))}:{string.Join(',', members.Select(m => m.Id))}";
        if (_squadRosterKey != key && _squadDialog.Visible)
        {
            _squadRosterKey = key;
            Control? focus = _squadDialog.GuiGetFocusOwner();
            string focusedName = focus is not null && (_squadRoster.IsAncestorOf(focus) || _memberRoster.IsAncestorOf(focus)) ? focus.Name.ToString() : "";
            _squadButtons.Clear(); _memberButtons.Clear();
            foreach (Node child in _squadRoster.GetChildren().Concat(_memberRoster.GetChildren()).ToArray()) { child.GetParent().RemoveChild(child); child.QueueFree(); }
            foreach (SquadState squad in squads)
            {
                int id = squad.Id;
                Button choice = Button(_squadRoster, $"#{id} {AssetCatalog.UnitName(squad.Type)} · {squad.Members.Length}/{state.Rules.Arena.SquadCapacity} · participating {squad.ParticipatingMembers.Length}", () => { _selectedSquad = id; _selectedMember = 0; RefreshSquads(); });
                choice.Name = "Squad" + id; _squadButtons.Add(id, choice);
            }
            foreach (UnitState member in members)
            {
                int id = member.Id;
                Button choice = Button(_memberRoster, $"Member #{id} · L{member.Level} · {HealthPoints.Format(member.Health)}/{HealthPoints.Format(member.Profile.Health)} HP", () => { _selectedMember = id; RefreshSquads(); }); choice.Name = "SquadMember" + id; _memberButtons.Add(id, choice);
            }
            if (focusedName.Length > 0)
                (_squadRoster.GetNodeOrNull<Button>(focusedName) ?? _memberRoster.GetNodeOrNull<Button>(focusedName))?.GrabFocus();
        }
        foreach (SquadState squad in squads)
            if (_squadButtons.TryGetValue(squad.Id, out Button? button))
            {
                button.Text = $"#{squad.Id} {AssetCatalog.UnitName(squad.Type)} · {squad.Members.Length}/{state.Rules.Arena.SquadCapacity} · {(squad.Hex.Lifecycle == UnitLifecycle.Stored ? "stored" : $"participating {squad.ParticipatingMembers.Length}")}";
                button.Modulate = SharedArenaView.OwnerColor(squad.Owner).Lerp(Colors.White, squad.Id == _selectedSquad ? .3f : 0);
            }
        foreach (UnitState member in members)
            if (_memberButtons.TryGetValue(member.Id, out Button? button))
                button.Text = $"Member #{member.Id} · L{member.Level} · {HealthPoints.Format(member.Health)}/{HealthPoints.Format(member.Profile.Health)} HP";
        int deployedHomes = squads.Count(s => s.Hex.Lifecycle != UnitLifecycle.Stored);
        _squadSummary.Text = $"P{_focus} · {deployedHomes}/{city?.Army?.PurchasedHomes ?? 0} squad homes · equipment and food paid per member.\nSelect a squad, then a veteran. Deployment: select a squad here or in the arena, close this panel, click a highlighted empty owned home.";
        UnitState? chosen = members.FirstOrDefault(u => u.Id == _selectedMember);
        _memberSummary.Text = chosen is null ? "Select a veteran." : $"Member #{chosen.Id} · squad #{chosen.SquadId}\n{ProgressionPresentation.UnitStats(chosen, state.Tick)}" + (selected is not null && state.Tick < selected.RallyDisruptedUntil ? $"\nRally disrupted: direct X/Y -{state.Rules.Arena.RallyDamageReductionPercent}% · {Math.Max(0, selected.RallyDisruptedUntil - state.Tick) / 60.0:0.0}s" : "");
        bool editable = CanEdit() && city is not null;
        // Preserve native selection while authoritative building/roster options change.
        void Options(OptionButton control, IEnumerable<(int Id, string Text)> options)
        {
            int selectedId = control.ItemCount > 0 ? control.GetSelectedId() : -1;
            (int Id, string Text)[] values = options.ToArray();
            if (control.ItemCount == values.Length && values.Select((v, i) => control.GetItemId(i) == v.Id && control.GetItemText(i) == v.Text).All(equal => equal)) return;
            control.Clear(); foreach (var value in values) control.AddItem(value.Text, value.Id);
            int index = control.GetItemIndex(selectedId); if (index >= 0) control.Select(index);
        }
        Options(_recruitSource, city?.Slots.SelectMany((slot, index) => state.BuildingCatalog.FirstOrDefault(b => b.Type == slot.Type)?.Recruits?.Select(type =>
            (index * 4 + (int)type, $"Plot {index + 1} · {AssetCatalog.UnitName(type)} L{slot.Level} · {ResourceText(city.RecruitmentQuotes.Single(q => q.Type == type && q.Level == slot.Level).Cost)}")) ?? []) ?? []);
        Options(_regroupDestination, new[] { (0, "Create new squad at a free home") }.Concat(squads.Where(s => s.Id != _selectedSquad && s.Hex.Lifecycle != UnitLifecycle.Stored && s.Type == chosen?.Type && s.Members.Length < state.Rules.Arena.SquadCapacity).Select(s => (s.Id, $"Squad #{s.Id} · {s.Members.Length}/{state.Rules.Arena.SquadCapacity}"))));
        Options(_storeDestination, city?.Army?.Halls.Select(h => (h.Slot, $"Town hall · plot {h.Slot + 1} · {h.Capacity} individual places")) ?? []);
        int encoded = _recruitSource.ItemCount > 0 ? _recruitSource.GetSelectedId() : -1;
        UnitType sourceType = (UnitType)(encoded % 4);
        RecruitmentQuote? quote = city is not null && encoded >= 0 ? city.RecruitmentQuotes.Single(q => q.Type == sourceType && q.Level == city.Slots[encoded / 4].Level) : null;
        bool affordable = quote is not null && city!.Resources.TryPay(quote.Cost, out _);
        _reinforceSquad.Disabled = !editable || !affordable || selected is null || selected.Hex.Lifecycle == UnitLifecycle.Stored || selected.Type != sourceType || selected.Members.Length >= state.Rules.Arena.SquadCapacity;
        _newSquad.Disabled = !editable || !affordable || UiArmy()!.Find(city!.Soldiers, city.Army!.PurchasedHomes,
            plots: city.Army.Homes.Select(h => h.Plot).ToArray(), type: sourceType, squadId: -1) is null;
        _retireMember.Disabled = !editable || chosen is null;
        _regroupMember.Disabled = !editable || chosen?.Assignment is not { Stored: false } || _regroupDestination.ItemCount == 0;
        _releaseMember.Disabled = !editable || chosen?.Assignment is not { Stored: true } || _regroupDestination.ItemCount == 0;
        _storeMember.Disabled = !editable || chosen?.Assignment is not { Stored: false } || _storeDestination.ItemCount == 0;
        _recruitSource.Disabled = _regroupDestination.Disabled = _storeDestination.Disabled = !editable;
    }
    private void SelectArena(Vector2 point)
    {
        UnitView? unit = PickUnit(point);
        if (unit is not null) { _selectedSquad = unit.State.Owner == game.PlayerId ? unit.State.SquadId : 0; _selectedMember = unit.State.Id; _inspector.Select(unit.State); return; }
        if (!CanEdit() || _arenaView is null || !_showDeployment) return;
        int cell = _arenaView.PickCell(_camera.ProjectRayOrigin(point), _camera.ProjectRayNormal(point));
        if (cell > 0 && _selectedSquad > 0) game.SendAction("deploy-squad", squadId: _selectedSquad, cell: cell);
    }
}
