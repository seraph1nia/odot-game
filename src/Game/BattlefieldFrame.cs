using Game.Core;
using Godot;

namespace Game;

// Settlement presentation bounds only. Combat lives in SharedArenaView, whose
// independent complete content bounds never crop numerical routes or cells.
internal static class BattlefieldFrame
{
    internal const float Ground = -1.5f, Roof = 5.2f, Margin = 6;
    internal static Aabb Content(MatchSnapshot? state, IEnumerable<Aabb> buildings)
    {
        var points = new List<Vector3> { VillageLandscape.Home, VillageLandscape.Defender };
        points.AddRange(Enumerable.Range(0, 9).Select(VillageLayout.Slot));
        if (state is not null)
            points.AddRange(Enumerable.Range(ArmyConfiguration.PlotOffset, ArmyConfiguration.PlotCount).Select(id => ArmyPlotView.PlotPosition(state, id)));
        if (state?.Arena is { } arena)
        {
            Vector3 origin = CombatLayout.ArenaOrigin - CombatLayout.DistrictOrigin(state);
            // All actionable homes/objectives plus actual forecast bodies. Empty
            // outer battle cells remain in the shared arena and its overview;
            // they need not force the settlement camera away from its controls.
            int[] cells = arena.Sectors.SelectMany(s => s.DeploymentCells).Concat(state.Structures.Select(s => s.Cell))
                .Concat(BattleDeployment.Preview(state).Where(u => u.Deployed).Select(u => u.Hex!.Position.Cell)).Distinct().ToArray();
            points.AddRange(cells.Select(id =>
            {
                System.Numerics.Vector3 p = SharedCombatProjection.CellPosition(arena.Board, id);
                return origin + new Vector3(p.X, p.Y, p.Z);
            }));
        }
        // Complete persistent settlement/forecast silhouettes, not live combat
        // positions: SharedArenaView owns the independent complete arena bounds.
        Vector3 min = points.Aggregate((a, b) => a.Min(b)) - new Vector3(VillageLayout.Radius, 0, VillageLayout.Radius);
        Vector3 max = points.Aggregate((a, b) => a.Max(b)) + new Vector3(VillageLayout.Radius, 0, VillageLayout.Radius);
        min.Y = Ground; max.Y = Roof;
        Aabb bounds = new(min, max - min);
        foreach (Aabb building in buildings) bounds = bounds.Merge(building);
        return bounds;
    }
    internal static void FitCamera(Camera3D camera, Aabb content, Vector3 center, Vector2 size, float hud, float rightLane = 240)
    {
        float pitch = Mathf.DegToRad(32), azimuth = 0;
        Vector3 direction = new(Mathf.Sin(azimuth) * Mathf.Cos(pitch), Mathf.Sin(pitch), Mathf.Cos(azimuth) * Mathf.Cos(pitch));
        Vector3 target = center;
        camera.Position = target + direction * 45; camera.LookAt(target);
        Vector3 right = camera.GlobalBasis.X, up = camera.GlobalBasis.Y;
        Vector3[] corners = Enumerable.Range(0, 8).Select(content.GetEndpoint).ToArray();
        float minX = corners.Min(p => p.Dot(right)), maxX = corners.Max(p => p.Dot(right));
        float minY = corners.Min(p => p.Dot(up)), maxY = corners.Max(p => p.Dot(up));
        const float chrome = 64;
        float usable = Math.Max(1, size.Y - hud - chrome), width = Math.Max(1, size.X - rightLane);
        camera.Size = Math.Max((maxY - minY + 1.2f) * size.Y / usable, (maxX - minX + 1.2f) * size.Y / width);
        target = center + right * ((minX + maxX) / 2 + camera.Size * rightLane / 2 / size.Y) + up * ((minY + maxY) / 2 + camera.Size * (chrome - hud) / size.Y / 2);
        camera.Position = target + direction * 45; camera.LookAt(target);
        // Orthographic framing can grow beyond the fixed camera distance in
        // portrait. Keep the entire surround height range inside the clip planes;
        // retreating along the view axis changes depth, not screen composition.
        float nearest = float.PositiveInfinity, farthest = float.NegativeInfinity;
        foreach (Vector2 pixel in new[] { Vector2.Zero, size })
            foreach (float height in new[] { Ground, Roof })
            {
                Vector3 start = camera.ProjectRayOrigin(pixel), ray = camera.ProjectRayNormal(pixel);
                Vector3 hit = start + ray * ((height - start.Y) / ray.Y);
                float depth = (camera.GlobalPosition - hit).Dot(camera.GlobalBasis.Z);
                nearest = Math.Min(nearest, depth); farthest = Math.Max(farthest, depth);
            }
        float retreat = Math.Max(0, camera.Near + 1 - nearest);
        camera.Position += direction * retreat;
        camera.Far = Math.Max(camera.Far, farthest + retreat + 1);
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
