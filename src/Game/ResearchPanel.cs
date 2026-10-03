using Game.Core;
using Godot;

namespace Game;

public partial class Tabletop
{
    private AcceptDialog _researchDialog = null!;
    private Label _researchBalance = null!;
    private UnitClass _researchClass;
    private Button _openResearch = null!;
    private long _researchRevision = -1;
    private int _researchFocus;
    private bool _researchConnected;
    private readonly Dictionary<TechnologyId, Button> _technologyButtons = [];
    private void CreateResearch(Control cityControls)
    {
        Button open = _openResearch = Button(cityControls, "Research", () => { RefreshResearch(force: true); _researchDialog.PopupCenteredClamped(new(650, 550), .9f); });
        open.Name = "Research";
        _researchDialog = new AcceptDialog { Title = "Personal research", Theme = application.Theme, Transient = true, Exclusive = false };
        AddChild(_researchDialog);
        var scroll = new ScrollContainer { CustomMinimumSize = new(600, 450) }; _researchDialog.AddChild(scroll);
        var body = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; scroll.AddChild(body);
        _researchBalance = Text(body, "", 14);
        var roles = new HBoxContainer(); body.AddChild(roles);
        foreach (UnitClass role in Enum.GetValues<UnitClass>())
        {
            UnitClass selected = role;
            Button tab = ResearchButton(roles, role.ToString(), () => { _researchClass = selected; RefreshResearch(force: true); }); tab.Name = "Research" + role;
        }
        foreach (TechnologyDefinition node in new ResearchCatalog().Definitions())
        {
            TechnologyId id = node.Id;
            Button buy = ResearchButton(body, node.Name, () => { if (CanEdit()) game.SendAction("research-tech", technology: id); });
            buy.Name = "Tech" + id; buy.Alignment = HorizontalAlignment.Left; _technologyButtons.Add(id, buy);
        }
    }
    private static Button ResearchButton(Node parent, string text, Action action)
    { var button = new Button { Text = text, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; button.AddThemeColorOverride("font_disabled_color", new Color("4e3f2e")); parent.AddChild(button); button.Pressed += action; return button; }
    private void RefreshResearch(bool force = false)
    {
        if (_researchDialog is null) return;
        CityState? city = Focus(); MatchSnapshot? state = game.State;
        if (!force && (!_researchDialog.Visible || _researchRevision == state?.Revision && _researchFocus == _focus && _researchConnected == game.Connected)) return;
        _researchRevision = state?.Revision ?? -1; _researchFocus = _focus; _researchConnected = game.Connected;
        _researchBalance.Text = city is null ? "Waiting for city" : $"{city.Research.Points} research points · progress {city.Research.Progress}/3\nTowers: +{city.Slots.Where(s => s.Type == Building.ResearchTower).Sum(s => s.Level)} progress per production · +1 point per shared clear";
        foreach ((TechnologyId id, Button button) in _technologyButtons)
        {
            TechnologyDefinition? node = state?.TechnologyCatalog.FirstOrDefault(t => t.Id == id);
            button.Visible = node?.Class == _researchClass;
            TechnologyEligibility eligibility = ProgressionPresentation.ResearchAccess(state, city, game.PlayerId, game.Connected, id);
            string gate = eligibility.Reason;
            button.Disabled = !eligibility.Available;
            TechnologyDefinition? sibling = state?.TechnologyCatalog.FirstOrDefault(t => node?.Tier == 2 && t.Tier == 2 && t.Class == node.Class && t.Id != id);
            string requirement = node?.Prerequisite is null or TechnologyId.None ? "" : " · requires " + TechnologyIds.Name(node.Prerequisite);
            string choice = sibling is null ? "" : " · locks " + sibling.Name + " + its mastery";
            button.Text = node is null ? TechnologyIds.Name(id) : $"{node.Name} · {node.Cost} points{choice}\n{ProgressionPresentation.CapabilityText(node.Benefit)}\n{string.Join(", ", node.EligibleTypes)}{requirement}\n{gate}";
            button.TooltipText = (node?.Prerequisite != TechnologyId.None ? "Requires " + TechnologyIds.Name(node?.Prerequisite ?? TechnologyId.None) + ". " : "")
                + (sibling is null ? "" : "Purchase permanently locks " + sibling.Name + " and its mastery. ") + gate;
        }
    }
    private void ObserveResearch(Dictionary<string, object> targets)
    {
        application.ObserveControl(targets, "Research", _openResearch);
        if (!_researchDialog.Visible) return;
        application.ObserveControl(targets, "CloseResearch", _researchDialog.GetOkButton());
        foreach (Button button in _researchDialog.FindChildren("*", "Button", true, false).OfType<Button>())
            if (button.Name.ToString().StartsWith("Tech", StringComparison.Ordinal) || button.Name.ToString().StartsWith("Research", StringComparison.Ordinal))
                application.ObserveControl(targets, button.Name.ToString(), button);
    }
}
