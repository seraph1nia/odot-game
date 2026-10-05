using Godot;

namespace Game;

internal sealed record StaticGeometryReport(int Vertices, int Triangles, int Surfaces, int Transforms,
    double PositionError, double PositionAllowance, double NormalError, double TangentError, double UvError,
    int NonuniformTransforms, int NegativeTransforms);

// Static rendering buffers are the fidelity contract. Mesh.GetFaces() creates a
// collision TriangleMesh (including vertex welding), not a rendering snapshot.
internal static class StaticGeometry
{
    private static float MaxAxisValue(this Vector3 value) => Math.Max(value.X, Math.Max(value.Y, value.Z));
    private const double FloatUnit = 1.0 / 8388608; // binary32 epsilon, not Single.Epsilon
    // Uncompressed Godot meshes still store normal/tangent directions as
    // octahedral RG16UNORM. Eight lattice steps bound decode/normalize/encode.
    private const double DirectionAllowance = 8.0 / 65535;
    internal sealed record Key(Material Material, int Attributes);
    internal sealed class Surface
    {
        internal readonly List<Vector3> Positions = [], Normals = [], Tangents = [];
        internal readonly List<float> Handedness = [];
        internal readonly List<Vector2> Uvs = [], Uv2s = [];
        internal readonly List<Color> Colors = [];
        internal readonly List<int> Indices = [];
        internal readonly List<double> Allowances = [];
    }
    internal sealed class Snapshot
    {
        internal readonly Dictionary<Key, Surface> Surfaces = [];
        internal readonly List<(Mesh Mesh, int Surface, Transform3D Transform, Key Key)> Parts = [];
        internal int Nonuniform, Negative, Transforms;
        internal Vector3[] Positions => Surfaces.Values.SelectMany(s => s.Positions).ToArray();
    }
    private static bool Has(Godot.Collections.Array arrays, Mesh.ArrayType slot) => arrays[(int)slot].VariantType != Variant.Type.Nil;
    private static int[] Indices(Godot.Collections.Array arrays, int count) => Has(arrays, Mesh.ArrayType.Index)
        ? arrays[(int)Mesh.ArrayType.Index].AsInt32Array() : Enumerable.Range(0, count).ToArray();
    private static void Require(bool condition, string detail)
    {
        if (!condition) throw new InvalidOperationException("Static rendering fidelity: " + detail);
    }
    internal static Snapshot Capture(Node3D root, bool auditMirrors = false)
    {
        Require(root.FindChildren("*", "Skeleton3D", true, false).Count == 0
            && !root.FindChildren("*", "AnimationPlayer", true, false).OfType<AnimationPlayer>().Any(p => p.GetAnimationList().Any(a => a != "RESET")), "animated/skinned hierarchy");
        var snapshot = new Snapshot();
        void Visit(Node3D node, Transform3D parent)
        {
            Transform3D transform = parent * node.Transform;
            Require(transform.Origin.IsFinite() && transform.Basis.X.IsFinite() && transform.Basis.Y.IsFinite() && transform.Basis.Z.IsFinite()
                && double.IsFinite(transform.Basis.Determinant()) && Math.Abs(transform.Basis.Determinant()) > 1e-12, "nonfinite/singular transform");
            snapshot.Transforms++;
            Vector3 scale = new(transform.Basis.X.Length(), transform.Basis.Y.Length(), transform.Basis.Z.Length());
            if (Math.Abs(scale.X - scale.Y) > 1e-6 || Math.Abs(scale.Y - scale.Z) > 1e-6) snapshot.Nonuniform++;
            if (transform.Basis.Determinant() < 0) snapshot.Negative++;
            if (node is MeshInstance3D { Mesh: { } } meshNode) Require(meshNode.Mesh is ArrayMesh, "non-array imported mesh");
            if (node is MeshInstance3D { Mesh: ArrayMesh mesh } instance)
            {
                Require(instance.Skin is null && mesh.GetBlendShapeCount() == 0, "skin/blend shapes");
                // The ordinary renderer flips culling for mirrored instances.
                // AppendFrom does not preserve that per-instance state.
                Require(transform.Basis.Determinant() > 0 || auditMirrors, "mirrored instances require original authored hierarchy");
                for (int index = 0; index < mesh.GetSurfaceCount(); index++)
                {
                    Require(mesh.SurfaceGetPrimitiveType(index) == Mesh.PrimitiveType.Triangles, "non-triangle surface");
                    Godot.Collections.Array arrays = mesh.SurfaceGetArrays(index);
                    for (int custom = (int)Mesh.ArrayType.Custom0; custom <= (int)Mesh.ArrayType.Weights; custom++)
                        Require(arrays[custom].VariantType == Variant.Type.Nil, "custom/skinned vertex attributes");
                    int attributes = 0;
                    for (int slot = 0; slot <= (int)Mesh.ArrayType.TexUV2; slot++) if (arrays[slot].VariantType != Variant.Type.Nil) attributes |= 1 << slot;
                    Material material = instance.GetActiveMaterial(index) ?? throw new InvalidOperationException("Static rendering fidelity: missing material");
                    var key = new Key(material, attributes);
                    if (!snapshot.Surfaces.TryGetValue(key, out Surface? surface)) snapshot.Surfaces.Add(key, surface = new());
                    Vector3[] positions = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
                    int offset = surface.Positions.Count;
                    foreach (Vector3 position in positions)
                    {
                        Require(position.IsFinite(), "nonfinite source position");
                        Vector3 expected = transform * position;
                        Require(expected.IsFinite(), "nonfinite transformed position");
                        surface.Positions.Add(expected);
                        // Four multiply/add terms per component, with slack for
                        // binary32 CPU/native ordering; absolute near zero and
                        // relative to the actual transformed operand magnitudes.
                        double magnitude = transform.Origin.Abs().MaxAxisValue()
                            + transform.Basis.X.Abs().MaxAxisValue() * Math.Abs(position.X)
                            + transform.Basis.Y.Abs().MaxAxisValue() * Math.Abs(position.Y)
                            + transform.Basis.Z.Abs().MaxAxisValue() * Math.Abs(position.Z);
                        surface.Allowances.Add(8 * FloatUnit * Math.Max(1, magnitude));
                    }
                    if (Has(arrays, Mesh.ArrayType.Normal))
                        foreach (Vector3 normal in arrays[(int)Mesh.ArrayType.Normal].AsVector3Array())
                        {
                            Require(normal.IsFinite() && normal.LengthSquared() > 0, "invalid source normal");
                            surface.Normals.Add((transform.Basis.Inverse().Transposed() * normal).Normalized());
                        }
                    if (Has(arrays, Mesh.ArrayType.Tangent))
                    {
                        float[] tangents = arrays[(int)Mesh.ArrayType.Tangent].AsFloat32Array();
                        Require(tangents.Length == positions.Length * 4, "tangent cardinality");
                        for (int t = 0; t < tangents.Length; t += 4)
                        {
                            Vector3 tangent = new(tangents[t], tangents[t + 1], tangents[t + 2]);
                            Require(tangent.IsFinite() && float.IsFinite(tangents[t + 3]), "nonfinite source tangent");
                            surface.Tangents.Add((transform.Basis * tangent).Normalized()); surface.Handedness.Add(tangents[t + 3] * (transform.Basis.Determinant() < 0 ? -1 : 1));
                        }
                    }
                    if (Has(arrays, Mesh.ArrayType.TexUV)) surface.Uvs.AddRange(arrays[(int)Mesh.ArrayType.TexUV].AsVector2Array());
                    if (Has(arrays, Mesh.ArrayType.TexUV2)) surface.Uv2s.AddRange(arrays[(int)Mesh.ArrayType.TexUV2].AsVector2Array());
                    if (Has(arrays, Mesh.ArrayType.Color)) surface.Colors.AddRange(arrays[(int)Mesh.ArrayType.Color].AsColorArray());
                    int[] indices = Indices(arrays, positions.Length);
                    Require(indices.Length % 3 == 0 && indices.All(i => i >= 0 && i < positions.Length), "invalid source indices");
                    if (transform.Basis.Determinant() < 0)
                        for (int triangle = 0; triangle < indices.Length; triangle += 3) (indices[triangle], indices[triangle + 1]) = (indices[triangle + 1], indices[triangle]);
                    surface.Indices.AddRange(indices.Select(i => i + offset));
                    snapshot.Parts.Add((mesh, index, transform, key));
                }
            }
            foreach (Node3D child in node.GetChildren().OfType<Node3D>()) Visit(child, transform);
        }
        Visit(root, Transform3D.Identity);
        Require(snapshot.Surfaces.Count > 0, "empty geometry"); return snapshot;
    }
    internal static ArrayMesh Append(Snapshot snapshot)
    {
        var mesh = new ArrayMesh();
        foreach (Key key in snapshot.Surfaces.Keys)
        {
            using var surface = new SurfaceTool(); surface.Begin(Mesh.PrimitiveType.Triangles);
            foreach (var part in snapshot.Parts.Where(p => p.Key == key)) surface.AppendFrom(part.Mesh, part.Surface, part.Transform);
            surface.SetMaterial(key.Material); surface.Commit(mesh);
        }
        return mesh;
    }
    internal static ArrayMesh Consolidate(Snapshot snapshot)
    {
        Require(snapshot.Negative == 0, "mirrored instances require original authored hierarchy");
        ArrayMesh mesh = Append(snapshot);
        try { Validate(snapshot, mesh); return mesh; }
        catch { mesh.Dispose(); throw; }
    }
    private static ArrayMesh CodecControl(Snapshot snapshot)
    {
        // Independent direct-array upload, without AppendFrom or collision
        // welding. This measures the engine's actual attribute codec, including
        // coupled normal/tangent encoding, instead of guessing its error.
        var control = new ArrayMesh();
        foreach (Surface surface in snapshot.Surfaces.Values)
        {
            var arrays = new Godot.Collections.Array(); arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = surface.Positions.ToArray();
            if (surface.Normals.Count > 0) arrays[(int)Mesh.ArrayType.Normal] = surface.Normals.ToArray();
            if (surface.Tangents.Count > 0) arrays[(int)Mesh.ArrayType.Tangent] = surface.Tangents.SelectMany((t, i) => new[] { t.X, t.Y, t.Z, surface.Handedness[i] }).ToArray();
            if (surface.Uvs.Count > 0) arrays[(int)Mesh.ArrayType.TexUV] = surface.Uvs.ToArray();
            if (surface.Uv2s.Count > 0) arrays[(int)Mesh.ArrayType.TexUV2] = surface.Uv2s.ToArray();
            if (surface.Colors.Count > 0) arrays[(int)Mesh.ArrayType.Color] = surface.Colors.ToArray();
            arrays[(int)Mesh.ArrayType.Index] = surface.Indices.ToArray();
            control.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        }
        return control;
    }
    internal static StaticGeometryReport Validate(Snapshot snapshot, ArrayMesh mesh)
    {
        var result = Inspect(snapshot, mesh);
        Require(result.Errors.Length == 0, result.Errors.FirstOrDefault() ?? "unknown mismatch");
        return result.Report;
    }
    internal static (StaticGeometryReport Report, string[] Errors) Inspect(Snapshot snapshot, ArrayMesh mesh)
    {
        var errors = new List<string>();
        void Check(bool valid, string reason) { if (!valid && errors.Count < 16) errors.Add(reason); }
        using ArrayMesh codec = CodecControl(snapshot);
        Require(mesh.GetSurfaceCount() == snapshot.Surfaces.Count, "lost/additional surface or material assignment");
        int surfaceIndex = 0, vertices = 0, triangles = 0;
        double positionError = 0, allowance = 0, normalError = 0, tangentError = 0, uvError = 0;
        foreach (var (key, expected) in snapshot.Surfaces)
        {
            Require(mesh.SurfaceGetMaterial(surfaceIndex) == key.Material, "changed surface material assignment");
            Require(mesh.SurfaceGetPrimitiveType(surfaceIndex) == Mesh.PrimitiveType.Triangles, "changed primitive type");
            Godot.Collections.Array encodedControl = codec.SurfaceGetArrays(surfaceIndex);
            Godot.Collections.Array arrays = mesh.SurfaceGetArrays(surfaceIndex++);
            Vector3[] positions = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            Require(positions.Length == expected.Positions.Count, "lost/additional rendering vertices");
            Check(Indices(arrays, positions.Length).SequenceEqual(expected.Indices), "changed index/topology/winding or triangle material partition");
            for (int v = 0; v < positions.Length; v++)
            {
                Require(positions[v].IsFinite(), "nonfinite result position");
                Vector3 delta = (positions[v] - expected.Positions[v]).Abs();
                double error = delta.MaxAxisValue();
                Check(error <= expected.Allowances[v], $"displaced rendering vertex {v}: {error:R} > {expected.Allowances[v]:R}");
                positionError = Math.Max(positionError, error); allowance = Math.Max(allowance, expected.Allowances[v]);
            }
            Vector3[] normals = Has(arrays, Mesh.ArrayType.Normal) ? arrays[(int)Mesh.ArrayType.Normal].AsVector3Array() : [];
            Require(normals.Length == expected.Normals.Count, "changed normal cardinality");
            Vector3[] encodedNormals = Has(encodedControl, Mesh.ArrayType.Normal) ? encodedControl[(int)Mesh.ArrayType.Normal].AsVector3Array() : [];
            for (int n = 0; n < normals.Length; n++)
            {
                Require(normals[n].IsFinite(), "nonfinite normal");
                double error = normals[n].DistanceTo(expected.Normals[n]);
                double codecError = encodedNormals[n].DistanceTo(expected.Normals[n]);
                Check(error <= codecError + DirectionAllowance && normals[n].DistanceTo(encodedNormals[n]) <= DirectionAllowance, $"changed normal {n}: {error:R}, codec={codecError:R}"); normalError = Math.Max(normalError, error);
            }
            float[] tangents = Has(arrays, Mesh.ArrayType.Tangent) ? arrays[(int)Mesh.ArrayType.Tangent].AsFloat32Array() : [];
            Require(tangents.Length == expected.Tangents.Count * 4, "changed tangent cardinality");
            float[] encodedTangents = Has(encodedControl, Mesh.ArrayType.Tangent) ? encodedControl[(int)Mesh.ArrayType.Tangent].AsFloat32Array() : [];
            for (int t = 0; t < expected.Tangents.Count; t++)
            {
                Vector3 actual = new(tangents[t * 4], tangents[t * 4 + 1], tangents[t * 4 + 2]);
                Require(actual.IsFinite() && float.IsFinite(tangents[t * 4 + 3]), "nonfinite tangent");
                double error = actual.DistanceTo(expected.Tangents[t]);
                Vector3 encoded = new(encodedTangents[t * 4], encodedTangents[t * 4 + 1], encodedTangents[t * 4 + 2]);
                double codecError = encoded.DistanceTo(expected.Tangents[t]);
                Check(error <= codecError + DirectionAllowance && actual.DistanceTo(encoded) <= DirectionAllowance && tangents[t * 4 + 3] == expected.Handedness[t], $"changed tangent/handedness {t}: {error:R}, codec={codecError:R}"); tangentError = Math.Max(tangentError, error);
            }
            void Uvs(Mesh.ArrayType slot, List<Vector2> values)
            {
                Vector2[] actual = Has(arrays, slot) ? arrays[(int)slot].AsVector2Array() : [];
                Require(actual.Length == values.Count, "changed UV cardinality");
                for (int u = 0; u < actual.Length; u++)
                {
                    Require(actual[u].IsFinite() && values[u].IsFinite(), "nonfinite UV");
                    double error = actual[u].DistanceTo(values[u]);
                    Check(error == 0, "changed UV value"); uvError = Math.Max(uvError, error);
                }
            }
            Uvs(Mesh.ArrayType.TexUV, expected.Uvs); Uvs(Mesh.ArrayType.TexUV2, expected.Uv2s);
            Color[] colors = Has(arrays, Mesh.ArrayType.Color) ? arrays[(int)Mesh.ArrayType.Color].AsColorArray() : [];
            Require(colors.Length == expected.Colors.Count && colors.SequenceEqual(expected.Colors), "changed vertex colors");
            vertices += positions.Length; triangles += expected.Indices.Count / 3;
        }
        return (new(vertices, triangles, snapshot.Surfaces.Count, snapshot.Transforms, positionError, allowance, normalError, tangentError, uvError, snapshot.Nonuniform, snapshot.Negative), errors.ToArray());
    }
    internal static Vector3[] Positions(Node3D root)
    {
        // Placement also supports the unconsolidated authored hierarchy,
        // including mirrored nodes. Read every actual rendering vertex, not
        // collision faces welded by Mesh.GetFaces().
        var positions = new List<Vector3>();
        void Visit(Node3D node, Transform3D parent)
        {
            Transform3D transform = parent * node.Transform;
            if (node is MeshInstance3D { Mesh: { } mesh })
                for (int surface = 0; surface < mesh.GetSurfaceCount(); surface++)
                    foreach (Vector3 vertex in mesh.SurfaceGetArrays(surface)[(int)Mesh.ArrayType.Vertex].AsVector3Array())
                    {
                        Vector3 position = transform * vertex;
                        Require(position.IsFinite(), "nonfinite rendering placement vertex"); positions.Add(position);
                    }
            foreach (Node3D child in node.GetChildren().OfType<Node3D>()) Visit(child, transform);
        }
        Visit(root, Transform3D.Identity); Require(positions.Count > 0, "empty rendering placement geometry"); return positions.ToArray();
    }
}
