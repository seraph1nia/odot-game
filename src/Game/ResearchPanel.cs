using Game.Core;
using Godot;

namespace Game;

public partial class Tabletop
{
    private UiDialog _researchDialog = null!;
    private Label _researchBalance = null!;
    private UnitClass _researchClass;
    private Button _openResearch = null!;
    private long _researchRevision = -1;
    private int _researchFocus;
    private bool _researchConnected;
    private TechnologyId _researchSelected;
    private Control _researchQuote = null!;
    private readonly Dictionary<TechnologyId, Button> _technologyButtons = [];
    private readonly Dictionary<TechnologyId, Button> _researchNodes = [];
    private readonly Dictionary<TechnologyId, PanelContainer> _researchCards = [];
    private readonly Dictionary<TechnologyId, Line2D> _researchLinks = [];

    private void CreateResearch(Control cityControls)
    {
        Button open = _openResearch = Button(cityControls, "Research", () =>
        {
            RefreshResearch(force: true);
            _researchDialog.Invoker = _openResearch;
            _researchDialog.PopupCenteredClamped(new(960, 650), .9f);
        });
        open.Name = "Research";
        _researchDialog = new UiDialog { Title = "Personal research", Theme = application.Theme };
        application.AddDialog(_researchDialog);
        VBoxContainer body = _researchDialog.Content;
        _researchBalance = Text(body, "", 14);
        Text(body, "Inspect a node or purchase directly. Specializations are exclusive; mastery replaces its earlier effect.", 12);
        var roles = new HBoxContainer(); body.AddChild(roles);
        foreach (UnitClass role in Enum.GetValues<UnitClass>())
        {
            UnitClass selected = role;
            Button tab = ResearchButton(roles, role.ToString(), () =>
            {
                _researchClass = selected; _researchSelected = TechnologyId.None; RefreshResearch(force: true);
            });
            tab.Name = "Research" + role;
        }
        var scroll = new ScrollContainer { Name = "ResearchTreeScroll", CustomMinimumSize = new(0, 320), FollowFocus = true };
        body.AddChild(scroll);
        var canvas = new Control { CustomMinimumSize = new(900, 310), MouseFilter = Control.MouseFilterEnum.Ignore };
        scroll.AddChild(canvas);
        // The upstream mini allocator assumes a 1-point shared root/three chains.
        // Our actual catalog has three 3-point class roots and exclusive forks.
        // Adapt its center-out native presentation, never its local spending rules.
        TechnologyDefinition[] definitions = new ResearchCatalog().Definitions().ToArray();
        Dictionary<TechnologyId, Vector2> centers = ResearchCenters(definitions);
        foreach (TechnologyDefinition node in definitions)
        {
            var link = new Line2D { Points = node.Prerequisite == TechnologyId.None ? [] : [centers[node.Prerequisite], centers[node.Id]], Width = 2, DefaultColor = new("706953") };
            canvas.AddChild(link); _researchLinks[node.Id] = link;
        }
        foreach (TechnologyDefinition node in definitions)
        {
            TechnologyId id = node.Id;
            var card = new PanelContainer { Position = centers[id] - new Vector2(88, 44), Size = new(176, 88), ThemeTypeVariation = "InsetPanel" };
            canvas.AddChild(card); _researchCards[id] = card;
            var contents = new VBoxContainer(); card.AddChild(contents);
            var inspect = new Button { Name = "Node" + id, ClipText = true, CustomMinimumSize = new(164, 36), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            contents.AddChild(inspect); _researchNodes[id] = inspect;
            void Select() { _researchSelected = id; RefreshResearch(force: true); }
            inspect.Pressed += Select; inspect.FocusEntered += Select;
            Button buy = ResearchButton(contents, "", () => PurchaseResearch(id));
            buy.Name = "Tech" + id;
            buy.AddThemeFontSizeOverride("font_size", 12);
            foreach (string state in ApplicationTheme.ButtonStates)
            {
                var style = (StyleBox)buy.GetThemeStylebox(state).Duplicate();
                style.ContentMarginTop = style.ContentMarginBottom = 2;
                buy.AddThemeStyleboxOverride(state, style);
            }
            _technologyButtons.Add(id, buy);
        }
        _researchQuote = UiAssets.Component("quoted_action", body);
        _researchQuote.Connect("requested", Callable.From<string>(id =>
        {
            if (Enum.TryParse(id, out TechnologyId technology)) PurchaseResearch(technology);
        }));
    }
    internal static Dictionary<TechnologyId, Vector2> ResearchCenters(TechnologyDefinition[] definitions)
    {
        var centers = new Dictionary<TechnologyId, Vector2>();
        foreach (UnitClass role in Enum.GetValues<UnitClass>())
        {
            TechnologyDefinition[] nodes = definitions.Where(n => n.Class == role).ToArray();
            centers[nodes.Single(n => n.Tier == 1).Id] = new(450, 250);
            TechnologyDefinition[] forks = nodes.Where(n => n.Tier == 2).OrderBy(n => n.Id).ToArray();
            for (int i = 0; i < forks.Length; i++)
            {
                centers[forks[i].Id] = new(i == 0 ? 250 : 650, 150);
                centers[nodes.Single(n => n.Prerequisite == forks[i].Id).Id] = new(i == 0 ? 100 : 800, 50);
            }
        }
        return centers;
    }
    private void PurchaseResearch(TechnologyId id)
    {
        if (CanEdit() && ProgressionPresentation.ResearchAccess(game.State, Focus(), game.PlayerId, game.Connected, id).Available)
            game.SendAction("research-tech", technology: id);
    }
    private static Button ResearchButton(Node parent, string text, Action action)
    {
        var button = new Button { Text = text, ClipText = true, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        parent.AddChild(button); button.Pressed += action; return button;
    }
    private void RefreshResearch(bool force = false)
    {
        if (_researchDialog is null) return;
        CityState? city = Focus(); MatchSnapshot? state = game.State;
        if (!force && (!_researchDialog.Visible || _researchRevision == state?.Revision && _researchFocus == _focus && _researchConnected == game.Connected)) return;
        _researchRevision = state?.Revision ?? -1; _researchFocus = _focus; _researchConnected = game.Connected;
        _researchBalance.Text = city is null ? "Waiting for city" : $"{city.Research.Points} research points · progress {city.Research.Progress}/3\nTowers: +{city.Slots.Where(s => s.Type == Building.ResearchTower).Sum(s => s.Level)} progress per production · +1 point per shared clear";
        if (state is null) return;
        if (!state.TechnologyCatalog.Any(n => n.Id == _researchSelected && n.Class == _researchClass))
            _researchSelected = state.TechnologyCatalog.First(n => n.Class == _researchClass && n.Tier == 1).Id;
        Dictionary<TechnologyId, Vector2> centers = ResearchCenters(state.TechnologyCatalog);
        foreach ((TechnologyId id, Button button) in _technologyButtons)
        {
            TechnologyDefinition? node = state.TechnologyCatalog.FirstOrDefault(t => t.Id == id);
            bool visible = node?.Class == _researchClass;
            _researchCards[id].Visible = visible;
            _researchCards[id].Position = centers[id] - new Vector2(88, 44);
            if (_researchLinks.TryGetValue(id, out Line2D? link))
            {
                link.Visible = visible && node?.Prerequisite != TechnologyId.None;
                link.Points = node is null || node.Prerequisite == TechnologyId.None ? [] : [centers[node.Prerequisite], centers[id]];
            }
            TechnologyEligibility eligibility = ProgressionPresentation.ResearchAccess(state, city, game.PlayerId, game.Connected, id);
            button.Disabled = !eligibility.Available;
            TechnologyDefinition? sibling = state.TechnologyCatalog.FirstOrDefault(t => node?.Tier == 2 && t.Tier == 2 && t.Class == node.Class && t.Id != id);
            string requirement = node?.Prerequisite is null or TechnologyId.None ? "" : " · requires " + TechnologyIds.Name(node.Prerequisite);
            string choice = sibling is null ? "" : " · permanently locks " + sibling.Name + " + its mastery";
            string description = node is null ? TechnologyIds.Name(id) : $"{node.Name} · {node.Cost} points{choice}\n{ProgressionPresentation.CapabilityText(node.Benefit)}\n{string.Join(", ", node.EligibleTypes.Select(type => AssetCatalog.UnitName(type)))}{requirement}";
            string stateText = eligibility.Available ? "Available" : eligibility.Reason.StartsWith("Permanently locked", StringComparison.Ordinal) ? "Permanently locked"
                : eligibility.Reason.StartsWith("Needs ", StringComparison.Ordinal) ? "Unfunded" : eligibility.Reason.StartsWith("Requires ", StringComparison.Ordinal) ? "Requires parent"
                : eligibility.Reason.Length <= 20 ? eligibility.Reason : "Unavailable now";
            button.Text = node is null ? "Unavailable" : $"Buy · {node.Cost} points\n{stateText}";
            button.TooltipText = description + "\n" + eligibility.Reason;
            Button inspect = _researchNodes[id];
            inspect.Text = node?.Name ?? TechnologyIds.Name(id);
            inspect.TooltipText = button.TooltipText;
            inspect.ThemeTypeVariation = id == _researchSelected ? "PrimaryButton" : "";
            if (link is not null)
            {
                bool owned = city?.Research.Has(id) == true;
                link.Width = owned ? 4 : 2;
                link.DefaultColor = new(owned ? "23889b" : eligibility.Available ? "b88d44" : "706953");
            }
            if (id == _researchSelected)
                _researchQuote.Call("set_data", new Godot.Collections.Dictionary
                {
                    ["id"] = id.ToString(),
                    ["title"] = "Purchase " + node?.Name,
                    ["quote"] = description,
                    ["enabled"] = eligibility.Available,
                    ["reason"] = eligibility.Reason,
                    ["variant"] = "PrimaryButton"
                });
        }
    }
    private void ObserveResearch(Dictionary<string, object> targets)
    {
        application.ObserveControl(targets, "Research", _openResearch);
        if (!_researchDialog.Visible) return;
        application.ObserveControl(targets, "CloseResearch", _researchDialog.GetOkButton());
        foreach (Button button in _researchDialog.FindChildren("*", "Button", true, false).OfType<Button>())
            if (button.Name.ToString().StartsWith("Tech", StringComparison.Ordinal) || button.Name.ToString().StartsWith("Research", StringComparison.Ordinal) || button.Name.ToString().StartsWith("Node", StringComparison.Ordinal))
                application.ObserveControl(targets, button.Name.ToString(), button);
    }
}
