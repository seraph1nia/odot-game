using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;

namespace Game;

// Receiver SETUP only, never a visibility/classification exemption. Select a
// real installed battle grass triangle with a protected clear native footprint.
internal static class NativeTerrainReceiver
{
    internal static MultiMeshInstance3D Select(Tabletop scene, JsonNode observation, string output)
    {
        Camera3D camera = scene.GetViewport().GetCamera3D()!;
        int height = (int)scene.GetViewport().GetVisibleRect().Size.Y;
        float hudTop = observation["Fields"]!["HudTop"]!.GetValue<float>();
        MultiMeshInstance3D[] owners = scene.FindChildren("*", "Node3D", true, false).OfType<VillageLandscape>().Where(l => l.IsVisibleInTree())
            .SelectMany(l => l.NativeGrassOwners()).Where(m => m.IsVisibleInTree()).ToArray();
        Aabb[] blockers = scene.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>()
            .Where(m => m.IsVisibleInTree()).Select(m => m.GlobalTransform * m.GetAabb()).ToArray();
        Rect2 viewport = scene.GetViewport().GetVisibleRect();
        static float[] V(Vector3 p) => [p.X, p.Y, p.Z];
        static float Edge(Vector2 p, Vector2 a, Vector2 b) => Math.Abs((b - a).Cross(p - a)) / a.DistanceTo(b);
        foreach (MultiMeshInstance3D owner in owners)
        {
            MultiMesh instances = owner.Multimesh;
            for (int instance = 0; instance < instances.InstanceCount; instance++)
            {
                Transform3D world = owner.GlobalTransform * instances.GetInstanceTransform(instance);
                if (world.Origin.X is < -8 or > 8 || world.Origin.Z is < -13 or > 1) continue;
                for (int surface = 0; surface < instances.Mesh.GetSurfaceCount(); surface++)
                {
                    Godot.Collections.Array arrays = instances.Mesh.SurfaceGetArrays(surface);
                    Vector3[] vertices = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
                    int[] indices = arrays[(int)Mesh.ArrayType.Index].VariantType == Variant.Type.Nil ? Enumerable.Range(0, vertices.Length).ToArray() : arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
                    for (int triangle = 0; triangle + 2 < indices.Length; triangle += 3)
                    {
                        Vector3 a = world * vertices[indices[triangle]], b = world * vertices[indices[triangle + 1]], c = world * vertices[indices[triangle + 2]];
                        Vector2 pa = camera.UnprojectPosition(a), pb = camera.UnprojectPosition(b), pc = camera.UnprojectPosition(c);
                        Vector2 center = (pa + pb + pc) / 3;
                        var pixel = new Vector2(Mathf.Floor(center.X) + .5f, Mathf.Floor(center.Y) + .5f);
                        if (!viewport.Grow(-10).HasPoint(pixel) || pixel.Y >= hudTop - 10 || !LandscapeBoundaryProof.ProtectedPixel(observation["Fields"]!["Camera"]!, height, (int)pixel.X, (int)pixel.Y)) continue;
                        float margin = Math.Min(Edge(pixel, pa, pb), Math.Min(Edge(pixel, pb, pc), Edge(pixel, pc, pa)));
                        if (margin < 8) continue; // actual >=16px protected footprint, not an RGB tolerance
                        Vector3 origin = camera.ProjectRayOrigin(pixel), direction = camera.ProjectRayNormal(pixel);
                        Vector3 e1 = b - a, e2 = c - a, cross = direction.Cross(e2); float determinant = e1.Dot(cross);
                        if (Math.Abs(determinant) < 1e-10f) continue;
                        Vector3 relative = origin - a, q = relative.Cross(e1);
                        float u = relative.Dot(cross) / determinant, v = direction.Dot(q) / determinant, depth = e2.Dot(q) / determinant;
                        if (u < 0 || v < 0 || u + v > 1 || depth < 0) continue;
                        bool clear = true;
                        // Conservative blocker exclusion across the whole sample
                        // footprint; any uncertain AABB refuses this SETUP point.
                        for (int y = -7; y <= 7 && clear; y += 7) for (int x = -7; x <= 7 && clear; x += 7)
                        {
                            Vector2 sample = pixel + new Vector2(x, y); Vector3 ray = camera.ProjectRayOrigin(sample), normal = camera.ProjectRayNormal(sample);
                            clear = !blockers.Any(box => VillageLayout.RayBounds(ray, normal, box) is { } hit && hit <= depth);
                        }
                        if (!clear) continue;
                        Material? material = owner.MaterialOverride ?? instances.Mesh.SurfaceGetMaterial(surface);
                        File.WriteAllText(output, JsonSerializer.Serialize(new
                        {
                            Schema = "actual-native-battle-grass-receiver-v1",
                            Owner = scene.GetPathTo(owner).ToString(),
                            Asset = AssetCatalog.Meadow,
                            Instance = instance,
                            Surface = surface,
                            Triangle = triangle / 3,
                            Pixel = new[] { (int)pixel.X, (int)pixel.Y },
                            WorldTriangle = new[] { V(a), V(b), V(c) },
                            ScreenTriangle = new[] { new[] { pa.X, pa.Y }, new[] { pb.X, pb.Y }, new[] { pc.X, pc.Y } },
                            RayOrigin = V(origin),
                            RayDirection = V(direction),
                            Depth = depth,
                            Barycentric = new[] { 1 - u - v, u, v },
                            ProjectedEdgeMargin = margin,
                            Material = material?.ResourceName.ToString(),
                            MaterialPath = material?.ResourcePath,
                            OriginalOverride = owner.MaterialOverride?.ResourceName.ToString(),
                            Method = "existing installed grass owner; native current-camera triangle/ray/protected projection; conservative mesh blocker footprint; actual captured >=100 protected changes still REQUIRED"
                        }, Game.Core.WireJson.Options));
                        return owner;
                    }
                }
            }
        }
        throw new InvalidDataException("No existing native battle grass receiver with clear projected/ray/triangle protected footprint.");
    }
}
