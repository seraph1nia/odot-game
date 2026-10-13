using Godot;

namespace Game;

// Shared by ordinary and forecast units, with deterministic overlap resolution.
internal static class UnitPicking
{
    internal static UnitView? Pick(IEnumerable<UnitView> views, Camera3D camera, Vector2 point)
    {
        Vector3 origin = camera.ProjectRayOrigin(point), direction = camera.ProjectRayNormal(point);
        return views.Where(view => view.Visible && !view.Dead && view.State.Health > 0 && view.State.Deployed && !camera.IsPositionBehind(view.GlobalPosition))
            .Select(view => (View: view, Distance: VillageLayout.RayBounds(origin, direction, view.PickingBounds())))
            .Where(hit => hit.Distance is >= 0).OrderBy(hit => hit.Distance).ThenBy(hit => hit.View.State.Id).Select(hit => hit.View).FirstOrDefault();
    }
}
