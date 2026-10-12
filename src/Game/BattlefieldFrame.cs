using Game.Core;
using Godot;

namespace Game;

// Presentation bounds only. All graph cells/anchors and all nine buildable plots
// survive; no numerical routes, formations or placement rules are shortened.
internal static class BattlefieldFrame
{
    internal const float Ground = -1.5f, Roof = 5.2f, Margin = 6;
    internal static Aabb Content(MatchSnapshot? state, IEnumerable<Aabb> buildings)
    {
        var points = new List<Vector3> { VillageLandscape.Home, VillageLandscape.Defender };
        HexBoardDefinition board = state?.Rules.Combat.Board ?? HexBoardDefinition.Default();
        foreach (var cell in board.Cells)
            foreach (var anchor in board.Anchors)
                points.Add(VillageLayout.Hex(cell.Coordinate.Column, cell.Coordinate.R) + new Vector3(anchor.AnchorX / 1000f, 0, anchor.AnchorForward / 1000f));
        points.AddRange(Enumerable.Range(0, 9).Select(VillageLayout.Slot));
        // Model/plot silhouette allowance, including queued army/enemy entry
        // anchors, rather than just units that happen to be deployed this tick.
        Vector3 min = points.Aggregate((a, b) => a.Min(b)) - new Vector3(VillageLayout.Radius, 0, VillageLayout.Radius);
        Vector3 max = points.Aggregate((a, b) => a.Max(b)) + new Vector3(VillageLayout.Radius, 0, VillageLayout.Radius);
        min.Y = Ground; max.Y = Roof;
        Aabb bounds = new(min, max - min);
        foreach (Aabb building in buildings) bounds = bounds.Merge(building);
        return bounds;
    }
    internal static void FitCamera(Camera3D camera, Aabb content, Vector3 center, Vector2 size, float hud)
    {
        float pitch = Mathf.DegToRad(34), azimuth = Mathf.DegToRad(28);
        Vector3 direction = new(Mathf.Sin(azimuth) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Cos(azimuth) * Mathf.Cos(pitch));
        Vector3 target = center;
        camera.Position = target + direction * 45; camera.LookAt(target);
        Vector3 right = camera.GlobalBasis.X, up = camera.GlobalBasis.Y;
        Vector3[] corners = Enumerable.Range(0, 8).Select(content.GetEndpoint).ToArray();
        float minX = corners.Min(p => p.Dot(right)), maxX = corners.Max(p => p.Dot(right));
        float minY = corners.Min(p => p.Dot(up)), maxY = corners.Max(p => p.Dot(up));
        float usable = Math.Max(1, size.Y - hud), width = Math.Max(1, size.X - 240);
        camera.Size = Math.Max((maxY - minY + 1.2f) * size.Y / usable, (maxX - minX + 1.2f) * size.Y / width);
        target = center + right * ((minX + maxX) / 2 + camera.Size * 120 / size.Y) + up * ((minY + maxY) / 2 - camera.Size * hud / size.Y / 2);
        camera.Position = target + direction * 45; camera.LookAt(target);
    }
    internal static Rect2 GroundRect(Aabb box) => new(new Vector2(box.Position.X, box.Position.Z), new Vector2(box.Size.X, box.Size.Z));
    internal static Rect2 Footprint(Camera3D camera, Rect2 viewport)
    {
        var points = new List<Vector2>();
        foreach (Vector2 corner in new[] { viewport.Position, new(viewport.End.X, viewport.Position.Y), viewport.End, new(viewport.Position.X, viewport.End.Y) })
            foreach (float height in new[] { Ground, Roof })
            {
                Vector3 origin = camera.ProjectRayOrigin(corner), direction = camera.ProjectRayNormal(corner);
                Vector3 point = origin + direction * ((height - origin.Y) / direction.Y);
                points.Add(new(point.X, point.Z));
            }
        Vector2 min = points.Aggregate((a, b) => a.Min(b)), max = points.Aggregate((a, b) => a.Max(b));
        return new(min, max - min);
    }
}
