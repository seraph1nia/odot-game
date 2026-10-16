using Game.Core;
using Godot;

namespace Game;

// Real native woodland modal; consumes detached canonical view only. Selection
// emits intent, never resources/effects. Closing cannot pick a reward.
internal sealed partial class RunDialog : UiDialog
{
    private Label _status = null!, _forecast = null!;
    private GridContainer _cards = null!;
    private string _shape = "";
    internal event Action<string>? Selected;
    internal IReadOnlyList<Button> Choices => _cards.GetChildren().OfType<PanelContainer>().Select(p => p.GetNode<Button>("Body/Select")).ToArray();
    public override void _Ready()
    {
        base._Ready();
        _status = AddText(Content, "", 15);
        _forecast = AddText(Content, "", 15);
        _cards = new GridContainer { Name = "RunCards", Columns = 2, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _cards.AddThemeConstantOverride("h_separation", 12); _cards.AddThemeConstantOverride("v_separation", 12); Content.AddChild(_cards);
    }
    private static Label AddText(Node parent, string text, int size)
    {
        var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        label.AddThemeFontSizeOverride("font_size", size); parent.AddChild(label); return label;
    }
    internal void Update(RunView view)
    {
        Title = view.Title; _status.Text = view.Status; _forecast.Text = view.Forecast;
        string shape = string.Join('|', view.Cards.Select(c => c.Id));
        if (_shape != shape)
        {
            foreach (Node node in _cards.GetChildren()) { _cards.RemoveChild(node); node.QueueFree(); }
            _shape = shape;
            foreach (RunCardView card in view.Cards)
            {
                var panel = new PanelContainer { Name = card.Id, SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new(270, 0) };
                panel.AddThemeStyleboxOverride("panel", GetThemeStylebox("panel", "PanelContainer")); _cards.AddChild(panel);
                var body = new VBoxContainer { Name = "Body", SizeFlagsHorizontal = SizeFlags.ExpandFill }; panel.AddChild(body);
                AddText(body, card.Title, 21).Name = "Title"; AddText(body, card.Subtitle, 16).Name = "Category";
                AddText(body, card.Description, 15).Name = "Effect"; AddText(body, card.Footer, 14).Name = "Footer";
                var select = new Button { Name = "Select", Text = "Choose", FocusMode = FocusModeEnum.All };
                body.AddChild(select); string id = card.Id; select.Pressed += () => Selected?.Invoke(id);
            }
        }
        _cards.Columns = GetViewportRect().Size.X < 800 ? 1 : 2;
        foreach (RunCardView card in view.Cards)
        {
            var panel = _cards.GetNode<PanelContainer>(card.Id);
            panel.GetNode<Label>("Body/Title").Text = (card.Selected ? "✓ " : "") + card.Title;
            panel.GetNode<Label>("Body/Footer").Text = card.Footer;
            var button = panel.GetNode<Button>("Body/Select"); button.Disabled = !card.Enabled;
            button.Text = card.Selected ? "Selected" : card.Enabled ? "Choose" : "Locked / awaiting";
        }
    }
    internal void Open() => PopupCenteredClamped(new(960, 650));
}

public partial class Tabletop
{
    private RunDialog _runDialog = null!;
    private Button _openRun = null!;
    private bool _showDoctrines;
    private string _openedDraft = "";
    private void CreateRunControls(Control city, Control root)
    {
        _openRun = Button(city, "Doctrine / rewards", () => { _showDoctrines = game.State?.Phase != Phase.RewardDraft; RefreshRun(); _runDialog.Open(); });
        _openRun.Name = "RunProgression";
        _runDialog = new RunDialog { Name = "RunDialog", Theme = root.Theme, Invoker = _openRun };
        _ownHudDialog(_runDialog);
        _runDialog.Selected += id =>
        {
            if (game.State is null || !game.Connected) return;
            game.SendAction((_showDoctrines ? "doctrine:" : "reward:") + id);
        };
    }
    private void RefreshRun()
    {
        MatchSnapshot? state = game.State; CityState? city = state?.Players.FirstOrDefault(c => c.Id == game.PlayerId);
        if (_runDialog is null || state is null || city is null) return;
        bool draft = state.Phase == Phase.RewardDraft && !city.Eliminated;
        _openRun.Text = draft ? "Choose reward" : RunCatalog.Doctrine(city.Run.Doctrine).Name;
        string key = state.MatchId + ":" + city.Run.DraftWave;
        if (draft && _openedDraft != key) { _openedDraft = key; _showDoctrines = false; _runDialog.Update(RunPresentation.Project(state, city, game.PlayerId, game.Connected, false)); _runDialog.Open(); }
        else _runDialog.Update(RunPresentation.Project(state, city, game.PlayerId, game.Connected, _showDoctrines));
        if (!draft && !_showDoctrines) _runDialog.Hide();
    }
}
