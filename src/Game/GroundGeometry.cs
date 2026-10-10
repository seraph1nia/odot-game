using Godot;

namespace Game;

// Rendering-buffer interfaces for the thin ground suite. This is not collision
// or gameplay support: only passive bank-clearing decoration uses Surface().
internal static class GroundGeometry
{
    internal const float Apothem = 2.2083648f;
    internal static Vector3 Normal(int edge) => new(-Mathf.Sin(edge * Mathf.Pi / 3), 0, -Mathf.Cos(edge * Mathf.Pi / 3));
    internal static Vector3[] Profile(Mesh mesh, int canonicalEdge)
    {
        Vector3 normal = Normal(canonicalEdge);
        var points = new List<Vector3>();
        for (int surface = 0; surface < mesh.GetSurfaceCount(); surface++)
            points.AddRange(mesh.SurfaceGetArrays(surface)[(int)Mesh.ArrayType.Vertex].AsVector3Array()
                .Where(p => Math.Abs(p.Dot(normal) - Apothem) < .0001f));
        Vector3[] profile = points.Distinct().ToArray();
        if (profile.Length < 5) throw new InvalidOperationException("Missing detailed-ground edge profile: " + canonicalEdge);
        return profile;
    }
    internal static Vector3 Port(Mesh mesh, Transform3D transform, int canonicalEdge)
    {
        Vector3 midpoint = Normal(canonicalEdge) * Apothem;
        Vector3 center = Profile(mesh, canonicalEdge).MinBy(p => new Vector2(p.X - midpoint.X, p.Z - midpoint.Z).LengthSquared());
        return transform * center;
    }
    internal static float? Surface(Mesh mesh, Transform3D transform, Vector3 point)
    {
        Vector3 local = transform.AffineInverse() * point;
        Aabb bounds = mesh.GetAabb();
        if (local.X < bounds.Position.X || local.X > bounds.End.X || local.Z < bounds.Position.Z || local.Z > bounds.End.Z) return null;
        float? height = null;
        for (int surface = 0; surface < mesh.GetSurfaceCount(); surface++)
        {
            var arrays = mesh.SurfaceGetArrays(surface);
            Vector3[] vertices = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            int[] indices = arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 a = vertices[indices[i]], b = vertices[indices[i + 1]], c = vertices[indices[i + 2]];
                Vector2 u = new(b.X - a.X, b.Z - a.Z), v = new(c.X - a.X, c.Z - a.Z), q = new(local.X - a.X, local.Z - a.Z);
                float determinant = u.Cross(v);
                if (Math.Abs(determinant) < 1e-8f) continue;
                float s = q.Cross(v) / determinant, t = u.Cross(q) / determinant;
                if (s < -1e-5f || t < -1e-5f || s + t > 1.00001f) continue;
                float y = (transform * new Vector3(local.X, a.Y + s * (b.Y - a.Y) + t * (c.Y - a.Y), local.Z)).Y;
                height = Math.Max(height ?? float.NegativeInfinity, y);
            }
        }
        return height;
    }
    internal static bool Recessed(Node node)
    {
        // Godot retains glTF typed node extras in metadata/extras, not as
        // individual metadata keys. Keep the canonical component IDs intact.
        if (!node.HasMeta("extras")) return false;
        var extras = node.GetMeta("extras").AsGodotDictionary();
        return extras.TryGetValue("kit_asset", out Variant value) && value.AsString() is
            "forest_hex_stream" or "forest_foam" or "forest_waterfall";
    }
    internal static int AdaptBridge(Node3D bridge)
    {
        // Remove the incompatible recessed substrate, never hide a floor above
        // the new base. Other imported bridge timbers/props keep their original
        // transforms. No source resource or unrelated bridge instance is edited.
        Node3D[] recessed = bridge.FindChildren("*", "Node3D", true, false).OfType<Node3D>()
            .Where(Recessed).ToArray();
        if (recessed.Length != 5) throw new InvalidOperationException("Pinned bridge recessed component inventory changed.");
        foreach (Node3D node in recessed) { node.GetParent().RemoveChild(node); node.Free(); }
        MeshInstance3D grass = bridge.FindChildren("Fine*scattered*woodland*grasses", "MeshInstance3D", true, false).OfType<MeshInstance3D>().Single();
        grass.GetParent().RemoveChild(grass); grass.Free();
        return recessed.Length;
    }
}
