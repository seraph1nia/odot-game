using Godot;

namespace Game;

public partial class Tabletop
{
    private Vector2? _pressPosition;
    private Vector3? _dragAnchor;
    private bool _dragging;
    private bool BlocksWorld(Vector2 point) => application.BlocksWorldInput(point) || _resources.GetGlobalRect().HasPoint(point) || _inspector.Contains(point);
    private void CancelGesture() { _pressPosition = null; _dragAnchor = null; _dragging = false; _navigation?.Interrupt(); }
    public override void _Notification(int what)
    {
        if (what == NotificationWMWindowFocusOut || what == NotificationApplicationFocusOut) CancelGesture();
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key)
        {
            _navigation.ObserveKey(key);
            if (key.IsActionPressed("ui_focus_next") || key.IsActionPressed("ui_focus_prev")) CancelGesture();
        }
        if (application.IsModalOpen || !GetWindow().HasFocus()) { CancelGesture(); _inspector.Close(); return; }
        if (@event is InputEventMouseButton { Pressed: true } press)
        {
            if (!_inspector.Contains(press.Position)) _inspector.Close();
            if (!WorldArea().HasPoint(press.Position) || BlocksWorld(press.Position)) CancelGesture();
        }
        // Observe releases even when a control or the outside HUD consumes them.
        if (@event is InputEventMouseButton { Pressed: false, ButtonIndex: MouseButton.Left } release && _pressPosition is not null)
        {
            bool click = !_dragging && WorldArea().HasPoint(release.Position) && !BlocksWorld(release.Position);
            CancelGesture();
            if (click) SelectAt(release.Position);
            GetViewport().SetInputAsHandled();
        }
        if (@event is InputEventMouseMotion motion && _pressPosition is { } start && _dragAnchor is { } anchor)
        {
            if (!_dragging && motion.Position.DistanceTo(start) >= 6) { _dragging = true; _inspector.Close(); }
            if (_dragging) _navigation.Drag(anchor, motion.Position);
            GetViewport().SetInputAsHandled();
        }
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!application.IsModalOpen && @event.IsActionPressed("ui_cancel", false))
        {
            CancelGesture(); _inspector.Close(); _settings.Open(); GetViewport().SetInputAsHandled(); return;
        }
        if (application.IsModalOpen || !GetWindow().HasFocus()) return;
        if (Focus() is not null && @event is InputEventKey { Pressed: true, Echo: false } reset && reset.Keycode == Key.Space && GetViewport().GuiGetFocusOwner() is null)
        {
            CancelGesture(); _navigation.Reset(); GetViewport().SetInputAsHandled(); return;
        }
        if (Focus() is not null && @event is InputEventKey key && GetViewport().GuiGetFocusOwner() is not (LineEdit or TextEdit) && _navigation.Press(key))
        { GetViewport().SetInputAsHandled(); return; }
        if (Focus() is not null && @event is InputEventMouseButton { Pressed: true } wheel && wheel.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown && WorldArea().HasPoint(wheel.Position) && !BlocksWorld(wheel.Position))
        {
            _navigation.ZoomAt(wheel.Position, wheel.ButtonIndex == MouseButton.WheelUp, wheel.Factor > 0 ? wheel.Factor : 1);
            GetViewport().SetInputAsHandled(); return;
        }
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse || Focus() is null || !WorldArea().HasPoint(mouse.Position) || BlocksWorld(mouse.Position)) return;
        GetViewport().GuiReleaseFocus();
        _pressPosition = mouse.Position; _dragAnchor = _navigation.DragAnchor(mouse.Position); _dragging = false;
        GetViewport().SetInputAsHandled();
    }
    private void SelectAt(Vector2 point)
    {
        UnitView? unit = PickUnit(point);
        if (unit is not null) { _inspector.Select(unit.State); return; }
        if (!game.Connected) return;
        int selected = Pick(point);
        if (selected >= 0) _slot = selected;
    }
    private UnitView? PickUnit(Vector2 point)
    {
        Vector3 origin = _camera.ProjectRayOrigin(point), direction = _camera.ProjectRayNormal(point);
        return _units.Values.Where(view => view.Visible && !view.Dead && view.State.Health > 0 && view.State.Deployed && !_camera.IsPositionBehind(view.GlobalPosition))
            .Select(view => (View: view, Distance: VillageLayout.RayBounds(origin, direction, view.PickingBounds())))
            .Where(hit => hit.Distance is >= 0).OrderBy(hit => hit.Distance).ThenBy(hit => hit.View.State.Id).Select(hit => hit.View).FirstOrDefault();
    }
}
