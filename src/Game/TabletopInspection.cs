using Game.Core;

namespace Game;

public partial class Tabletop
{
    private UnitInspector _inspector = null!;
    private void UpdateInspector()
    {
        if (application.IsModalOpen) { _inspector.Close(); return; }
        _inspector.Place(GetViewport().GetVisibleRect().Size, _resources.GetGlobalRect().End.Y, _panel.Position.Y);
        if (_inspector.UnitId is not { } id) return;
        UnitState? unit = game.State?.Players.SelectMany(city => city.Soldiers).Concat(game.State.Enemies).FirstOrDefault(unit => unit.Id == id);
        if (unit is null || unit.Health <= 0 || !unit.Deployed || unit.Destination != _focus || !_units.TryGetValue(id, out UnitView? view) || view.Dead || !view.Visible) { _inspector.Close(); return; }
        _inspector.Sample(unit);
    }
}
