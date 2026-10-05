using Godot;

namespace Game;

// Diagnostic submission inventory, before renderer frustum/occlusion/shadow
// passes. Actual total calls/primitives come from RenderingServer separately.
internal static class RenderInventory
{
    internal static object Observe(Node root)
    {
        var groups = new Dictionary<string, (int Nodes, long Surfaces, long Triangles, long Instances, HashSet<ulong> Materials)>();
        var triangles = new Dictionary<ulong, long>();
        long Triangles(Mesh mesh)
        {
            if (triangles.TryGetValue(mesh.GetInstanceId(), out long cached)) return cached;
            long count = 0;
            for (int surface = 0; surface < mesh.GetSurfaceCount(); surface++)
            {
                var arrays = mesh.SurfaceGetArrays(surface);
                int elements = arrays[(int)Mesh.ArrayType.Index].VariantType == Variant.Type.Nil
                    ? arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array().Length : arrays[(int)Mesh.ArrayType.Index].AsInt32Array().Length;
                count += elements / 3;
            }
            triangles[mesh.GetInstanceId()] = count; return count;
        }
        void Visit(Node node, string group)
        {
            if (node.HasMeta("asset")) group = node.GetMeta("asset").AsString();
            if (node is GeometryInstance3D geometry && geometry.IsVisibleInTree())
            {
                Mesh? mesh = geometry is MeshInstance3D instance ? instance.Mesh : geometry is MultiMeshInstance3D multi ? multi.Multimesh?.Mesh : null;
                if (mesh is not null)
                {
                    var current = groups.GetValueOrDefault(group);
                    current.Materials ??= [];
                    long count = geometry is MultiMeshInstance3D batch ? batch.Multimesh.InstanceCount : 1;
                    current.Nodes++; current.Instances += count; current.Surfaces += mesh.GetSurfaceCount(); current.Triangles += Triangles(mesh) * count;
                    for (int surface = 0; surface < mesh.GetSurfaceCount(); surface++)
                    {
                        Material? material = geometry is MeshInstance3D materialInstance ? materialInstance.GetActiveMaterial(surface) : mesh.SurfaceGetMaterial(surface);
                        if (material is not null) current.Materials.Add(material.GetInstanceId());
                    }
                    groups[group] = current;
                }
            }
            foreach (Node child in node.GetChildren()) Visit(child, group);
        }
        Visit(root, "terrain/code-markers/unclassified");
        return groups.OrderByDescending(g => g.Value.Surfaces).Select(g => new { Group = g.Key, g.Value.Nodes, g.Value.Surfaces, g.Value.Triangles, g.Value.Instances, Materials = g.Value.Materials.Count }).ToArray();
    }
}
