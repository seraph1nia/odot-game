using Godot;

namespace Game;

public partial class Tabletop
{
    private WorldtreeDialog _researchDialog = null!;
    internal WorldtreeDialog? ResearchDialog => _researchDialog;
    private Button _openResearch = null!;
    private long _researchRevision = -1;
    private int _researchFocus;
    private bool _researchConnected;
    private string _researchFeedback = "";
    private void CreateResearch(Control cityControls, Control overlayHost)
    {
        _openResearch = Button(cityControls, "Worldtree", () => { RefreshResearch(force: true); _researchDialog.Open(); });
        _openResearch.Name = "Research";
        _researchDialog = new WorldtreeDialog
        {
            Name = "WorldtreeResearchDialog",
            Theme = _hudRoot.Theme,
            Invoker = _openResearch
        };
        overlayHost.AddChild(_researchDialog);
        _researchDialog.Research.Requested = technology =>
        {
            // Existing session sender captures current match/phase/turn and returns
            // its real command result through the ordinary feedback path.
            if (CanEdit()) game.SendAction("research-tech", technology: technology);
        };
    }
    private void RefreshResearch(bool force = false)
    {
        if (_researchDialog is null) return;
        var state = game.State;
        if (!force && (!_researchDialog.Visible || _researchRevision == state?.Revision && _researchFocus == _focus && _researchConnected == game.Connected && _researchFeedback == game.Feedback)) return;
        _researchRevision = state?.Revision ?? -1; _researchFocus = _focus; _researchConnected = game.Connected;
        _researchFeedback = game.Feedback;
        _researchDialog.Research.Update(state, Focus(), game.PlayerId, game.Connected, game.Feedback);
    }
    private void ObserveResearch(Dictionary<string, object> targets)
    {
        Application.ObserveControl(targets, "Research", _openResearch);
        if (!_researchDialog.Visible) return;
        Application.ObserveControl(targets, "CloseResearch", _researchDialog.CloseButton);
        foreach (Button button in _researchDialog.FindChildren("*", "Button", true, false).OfType<Button>())
            Application.ObserveControl(targets, button.Name.ToString(), button);
    }
}
