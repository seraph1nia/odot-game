namespace Game;

public partial class Tabletop
{
    private UnitInspector _inspector = null!;
    internal void UpdateInspector(IReadOnlyDictionary<int, UnitView> units, double tick)
    {
        if (_hudInputBlocked()) { _inspector.Close(); return; }
        _inspector.Place(_hudRoot.GetViewport().GetVisibleRect().Size, _resources.GetGlobalRect().End.Y, _panel.Position.Y);
        if (_inspector.UnitId is not { } id) return;
        if (!units.TryGetValue(id, out UnitView? view) || view.Dead || !view.Visible || view.State is not { Health: > 0, Deployed: true } unit) { _inspector.Close(); return; }
        _inspector.Sample(unit, tick);
        _inspector.ConfigureActions(Me(), CanEdit(), UiArmy());
    }
}
