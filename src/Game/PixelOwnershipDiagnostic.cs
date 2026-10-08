using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;

namespace Game;

// Finite retained frame/sample investigations, not an object-id renderer,
// classifier waiver or acceptance check. Unknown contributions remain ambiguous.
internal static class PixelOwnershipDiagnostic
{
    private sealed record Hit(string Node, string Asset, string Owner, bool NewDecoration, int Surface, int Triangle,
        float Depth, float[] Point, float[] Barycentric, float[][] TriangleWorld, float[][] TriangleScreen, float PixelEdgeMargin, string BufferHash,
        string Material, string Transparency, string Cull, bool OpaqueUnmodified, int Priority);
    private sealed record Unknown(string Node, string Reason, int[] Pixels);
    // Authorized query scope only; never consulted by pixel acceptance.
    private static readonly string[] Frame499Points = ["139,573", "138,574", "154,589", "152,591", "151,592"];
    internal static void Inspect(Tabletop scene, string requestPath, string currentJson, Image image, string output)
    {
        JsonNode request = JsonNode.Parse(File.ReadAllText(requestPath))!;
        JsonNode before = JsonNode.Parse(File.ReadAllText(request["BeforeObservation"]!.GetValue<string>()))!;
        JsonNode after = JsonNode.Parse(File.ReadAllText(request["AfterObservation"]!.GetValue<string>()))!;
        JsonNode current = JsonNode.Parse(currentJson)!;
        int[][] pixels = request["Pixels"]!.AsArray().Select(p => p!.AsArray().Select(n => n!.GetValue<int>()).ToArray()).ToArray();
        int targetFrame = request["TargetFrame"]?.GetValue<int>() ?? 299;
        bool bounded = targetFrame == 499 ? pixels.Select(p => string.Join(",", p)).Order().SequenceEqual(Frame499Points.Order())
            : targetFrame == 599 ? pixels.Length is > 0 and <= 64 && pixels.All(p => p.Length == 2 && p[0] >= 2 && p[0] < image.GetWidth() - 2 && p[1] >= 2 && p[1] < image.GetHeight() - 2
                && LandscapeBoundaryProof.ProtectedPixel(current["Fields"]!["Camera"]!, image.GetHeight(), p[0], p[1]))
            : pixels.Length == 6 && !pixels.Any(p => p.Length != 2 || p[0] is < 138 or > 157 || p[1] is < 570 or > 574);
        if (!bounded || targetFrame is not (299 or 399 or 499 or 599)
            || before["Frame"]!.GetValue<int>() != targetFrame || after["Frame"]!.GetValue<int>() != targetFrame || current["Frame"]!.GetValue<int>() != targetFrame)
            throw new InvalidDataException("Ownership is bounded to the independently authorized retained frame/sample binding.");
        var mismatches = new List<string>();
        foreach (string key in new[] { "InputDigest", "Frame" })
            if (!JsonNode.DeepEquals(before[key], after[key]) || !JsonNode.DeepEquals(after[key], current[key])) mismatches.Add(key);
        foreach (string key in new[] { "Camera", "Units", "Strikes", "PlotHeights", "BuildingVariants", "HomeHealth", "CombatTick", "VisualSeconds", "MatchId", "MatchPhase", "Wave", "TurnSerial", "HudHeight", "HudTop" })
            if (!JsonNode.DeepEquals(before["Fields"]![key], after["Fields"]![key]) || !JsonNode.DeepEquals(after["Fields"]![key], current["Fields"]![key])) mismatches.Add(key);
        static JsonNode Placements(JsonNode observation)
        {
            JsonNode copy = observation["Fields"]!["Placements"]!.DeepClone();
            foreach (JsonNode? p in copy.AsArray())
                if (p!["Name"]!.GetValue<string>().StartsWith("@Node3D@", StringComparison.Ordinal)) p.AsObject().Remove("Name");
            return copy;
        }
        if (!JsonNode.DeepEquals(Placements(before), Placements(after)) || !JsonNode.DeepEquals(Placements(after), Placements(current))) mismatches.Add("Placements");
        foreach (string key in new[] { "CoreTerrain", "River", "Bridge", "Plots" })
            if (!JsonNode.DeepEquals(before["Fields"]!["Landscape"]![key], current["Fields"]!["Landscape"]![key])) mismatches.Add("Landscape." + key);
        if (targetFrame >= 499 && (!JsonNode.DeepEquals(before["Fields"]!["ArmyHomes"], after["Fields"]!["ArmyHomes"]) || !JsonNode.DeepEquals(after["Fields"]!["ArmyHomes"], current["Fields"]!["ArmyHomes"]))) mismatches.Add("ArmyHomes");
        if (mismatches.Count > 0)
        {
            File.WriteAllText(output, JsonSerializer.Serialize(new { Schema = "six-pixel-native-ownership-v1", Result = "ambiguous", MissingObservation = "Restored native view differs from retained controls", Mismatches = mismatches }, Game.Core.WireJson.Options));
            return;
        }
        JsonNode beforeIdentity = JsonNode.Parse(File.ReadAllText(request["BeforeIdentity"]!.GetValue<string>()))!;
        JsonNode afterIdentity = JsonNode.Parse(File.ReadAllText(request["AfterIdentity"]!.GetValue<string>()))!;
        var immutableInputs = new SortedDictionary<string, string>(StringComparer.Ordinal);
        string repository = Path.GetFullPath(Path.Combine(ProjectSettings.GlobalizePath("res://"), "../.."));
        foreach (var entry in beforeIdentity["Inputs"]!.AsObject().Where(p => p.Key.StartsWith("src/Game/Assets/Authored/", StringComparison.Ordinal)
            || targetFrame >= 499 && p.Key is "src/Game/ArmyPanel.cs" or "src/Game.Core/Combat/HexBoard.cs" or "src/Game/AssetCatalog.cs"
            || p.Key is "src/Game/Tabletop.cs" or "src/Game/TabletopCamera.cs" or "src/Game/VillageLayout.cs" or "src/Game/CombatLayout.cs" or "src/Game/VillageLighting.cs" or "src/Game/UnitView.cs" or "src/Game/UnitAssets.cs" or "src/Game/LandscapeAssets.cs" or "src/Game/project.godot" or "global.json" or "mise.lock"))
        {
            string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(repository, entry.Key))));
            if (hash != entry.Value!.GetValue<string>() || hash != afterIdentity["Inputs"]![entry.Key]!.GetValue<string>()) throw new InvalidDataException("Native geometry/projection provenance changed: " + entry.Key);
            immutableInputs.Add(entry.Key, hash);
        }
        Camera3D camera = scene.GetViewport().GetCamera3D() ?? throw new InvalidDataException("No active native viewport camera.");
        Vector2[] centers = pixels.Select(p => new Vector2(p[0] + .5f, p[1] + .5f)).ToArray();
        Vector3[] origins = centers.Select(camera.ProjectRayOrigin).ToArray(), directions = centers.Select(camera.ProjectRayNormal).ToArray();
        var hits = pixels.Select(_ => new List<Hit>()).ToArray(); var unknown = new List<Unknown>();
        static float[] V(Vector3 v) => [v.X, v.Y, v.Z];
        bool New(Node node)
        {
            for (Node? n = node; n is not null && n != scene; n = n.GetParent())
                if (n.HasMeta("pocket") || n.HasMeta("foundation_count")) return true;
            return false;
        }
        (string Asset, string Owner) Identity(Node node)
        {
            for (Node? n = node; n is not null && n != scene; n = n.GetParent())
            {
                if (n is UnitView unit) return ("character", "unit:" + unit.State.Id);
                if (n.HasMeta("asset")) return (n.GetMeta("asset").AsString(), n.Name.ToString());
            }
            return ("generated/terrain", node.Name.ToString());
        }
        void Surface(Node3D node, Mesh mesh, int surface, Transform3D transform, Material? material, Skin? skin = null, Skeleton3D? skeleton = null)
        {
            Godot.Collections.Array arrays = mesh.SurfaceGetArrays(surface);
            Vector3[] vertices = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            int[] indices = arrays[(int)Mesh.ArrayType.Index].VariantType == Variant.Type.Nil ? Enumerable.Range(0, vertices.Length).ToArray() : arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
            string hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { Vertices = vertices.Select(V), Indices = indices }, Game.Core.WireJson.Options)));
            if (skin is not null)
            {
                if (skeleton is null || !(skeleton.GlobalTransform.AffineInverse() * transform).IsEqualApprox(Transform3D.Identity))
                { unknown.Add(new(scene.GetPathTo(node).ToString(), "Unresolved mesh-to-skeleton transform", Enumerable.Range(0, pixels.Length).ToArray())); return; }
                int[] bones = arrays[(int)Mesh.ArrayType.Bones].AsInt32Array(); float[] weights = arrays[(int)Mesh.ArrayType.Weights].AsFloat32Array();
                int influences = bones.Length / vertices.Length;
                if (influences is not (4 or 8) || weights.Length != bones.Length) throw new InvalidDataException("Unexpected native skin buffers.");
                for (int v = 0; v < vertices.Length; v++)
                {
                    Vector3 posed = Vector3.Zero;
                    for (int j = 0; j < influences; j++)
                    {
                        int offset = v * influences + j; if (weights[offset] == 0) continue;
                        int bind = bones[offset], bone = skin.GetBindBone(bind);
                        if (bone < 0) bone = skeleton.FindBone(skin.GetBindName(bind));
                        posed += (skeleton.GetBoneGlobalPose(bone) * skin.GetBindPose(bind) * vertices[v]) * weights[offset];
                    }
                    vertices[v] = posed;
                }
            }
            vertices = vertices.Select(v => transform * v).ToArray();
            Vector3 min = vertices.Aggregate((a, b) => a.Min(b)), max = vertices.Aggregate((a, b) => a.Max(b));
            Aabb box = new(min, max - min);
            var possible = Enumerable.Range(0, pixels.Length).Where(p => VillageLayout.RayBounds(origins[p], directions[p], box) is not null).ToArray();
            if (possible.Length == 0) return;
            var (asset, owner) = Identity(node);
            BaseMaterial3D? standard = material as BaseMaterial3D;
            bool plain = standard is not null && standard.Transparency == BaseMaterial3D.TransparencyEnum.Disabled
                && !standard.NoDepthTest && !standard.Grow && standard.BillboardMode == BaseMaterial3D.BillboardModeEnum.Disabled && material!.NextPass is null
                && (node is not GeometryInstance3D geometry || geometry.MaterialOverlay is null);
            for (int triangle = 0; triangle + 2 < indices.Length; triangle += 3)
            {
                Vector3 a = vertices[indices[triangle]], b = vertices[indices[triangle + 1]], c = vertices[indices[triangle + 2]];
                Vector2 pa = camera.UnprojectPosition(a), pb = camera.UnprojectPosition(b), pc = camera.UnprojectPosition(c);
                float winding = (pb.X - pa.X) * (pc.Y - pa.Y) - (pb.Y - pa.Y) * (pc.X - pa.X);
                bool front = winding * Math.Sign(transform.Basis.Determinant()) > 0;
                if (standard?.CullMode == BaseMaterial3D.CullModeEnum.Back && !front || standard?.CullMode == BaseMaterial3D.CullModeEnum.Front && front) continue;
                foreach (int pixel in possible)
                {
                    Vector3 e1 = b - a, e2 = c - a, cross = directions[pixel].Cross(e2); float det = e1.Dot(cross);
                    if (Math.Abs(det) < 1e-10f) continue;
                    Vector3 relative = origins[pixel] - a; float u = relative.Dot(cross) / det; Vector3 q = relative.Cross(e1);
                    float v = directions[pixel].Dot(q) / det, depth = e2.Dot(q) / det;
                    if (u < 0 || v < 0 || u + v > 1 || depth < 0) continue;
                    static float Edge(Vector2 p, Vector2 x, Vector2 y) => Math.Abs((y - x).Cross(p - x)) / x.DistanceTo(y);
                    float margin = Math.Min(Edge(centers[pixel], pa, pb), Math.Min(Edge(centers[pixel], pb, pc), Edge(centers[pixel], pc, pa)));
                    hits[pixel].Add(new(scene.GetPathTo(node).ToString(), asset, owner, New(node), surface, triangle / 3, depth,
                        V(origins[pixel] + directions[pixel] * depth), [1 - u - v, u, v], [V(a), V(b), V(c)], [[pa.X, pa.Y], [pb.X, pb.Y], [pc.X, pc.Y]], margin, hash, material?.ResourceName.ToString() ?? "missing",
                        standard?.Transparency.ToString() ?? "unknown shader", standard?.CullMode.ToString() ?? "unknown", plain, material?.RenderPriority ?? 0));
                }
            }
        }
        foreach (MeshInstance3D mesh in scene.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>().Where(n => n.IsVisibleInTree() && n.Mesh is not null))
            for (int s = 0; s < mesh.Mesh.GetSurfaceCount(); s++)
                Surface(mesh, mesh.Mesh, s, mesh.GlobalTransform, mesh.GetActiveMaterial(s), mesh.Skin, mesh.Skin is null ? null : mesh.GetNodeOrNull<Skeleton3D>(mesh.Skeleton));
        foreach (MultiMeshInstance3D batch in scene.FindChildren("*", "MultiMeshInstance3D", true, false).OfType<MultiMeshInstance3D>().Where(n => n.IsVisibleInTree() && n.Multimesh?.Mesh is not null))
        {
            MultiMesh instances = batch.Multimesh;
            for (int i = 0; i < instances.InstanceCount; i++)
            {
                Transform3D transform = batch.GlobalTransform * instances.GetInstanceTransform(i); Aabb box = transform * instances.Mesh.GetAabb();
                if (!origins.Where((o, p) => VillageLayout.RayBounds(o, directions[p], box) is not null).Any()) continue;
                for (int s = 0; s < instances.Mesh.GetSurfaceCount(); s++) Surface(batch, instances.Mesh, s, transform, batch.MaterialOverride ?? instances.Mesh.SurfaceGetMaterial(s));
            }
        }
        var glyphInventory = new List<object>();
        foreach (GeometryInstance3D glyph in scene.FindChildren("*", "GeometryInstance3D", true, false).OfType<GeometryInstance3D>().Where(n => n.IsVisibleInTree() && n is not MeshInstance3D && n is not MultiMeshInstance3D))
        {
            Aabb box = glyph.GetAabb(); Transform3D pose = glyph.GlobalTransform;
            if (glyph is Label3D { Billboard: not BaseMaterial3D.BillboardModeEnum.Disabled } || glyph is SpriteBase3D { Billboard: not BaseMaterial3D.BillboardModeEnum.Disabled })
                pose = new(camera.GlobalBasis.Scaled(pose.Basis.Scale), pose.Origin);
            Vector2[] corners = Enumerable.Range(0, 8).Select(i => camera.UnprojectPosition(pose * box.GetEndpoint(i))).ToArray();
            int[] possible = Enumerable.Range(0, pixels.Length).Where(p => centers[p].X >= corners.Min(v => v.X) && centers[p].X <= corners.Max(v => v.X) && centers[p].Y >= corners.Min(v => v.Y) && centers[p].Y <= corners.Max(v => v.Y)).ToArray();
            if (targetFrame >= 499) glyphInventory.Add(new
            {
                Node = scene.GetPathTo(glyph).ToString(),
                Class = glyph.GetClass(),
                Bounds = corners.Select(p => new[] { p.X, p.Y }).ToArray(),
                PossiblePixels = possible,
                Descriptor = glyph is Label3D label ? PixelGlyphCompletion.DescribeMapped(scene, camera, label) : null
            });
            if (possible.Length > 0) unknown.Add(new(scene.GetPathTo(glyph).ToString(), "Generated glyph/sprite raster not exposed as a rendering array", possible));
        }
        foreach (Control overlay in scene.FindChildren("*", "Control", true, false).OfType<Control>().Where(c => c.IsVisibleInTree()
            && c is Label or BaseButton or Panel or PanelContainer or TextureRect or ColorRect or ProgressBar))
        {
            int[] possible = Enumerable.Range(0, pixels.Length).Where(i => overlay.GetGlobalRect().HasPoint(centers[i])).ToArray();
            if (possible.Length > 0) unknown.Add(new(scene.GetPathTo(overlay).ToString(), "2D control raster overlaps this sample", possible));
        }
        image.Convert(Image.Format.Rgba8); byte[] rgba = image.GetData();
        using Image beforeImage = Image.LoadFromFile(request["BeforePng"]!.GetValue<string>());
        using Image afterImage = Image.LoadFromFile(request["AfterPng"]!.GetValue<string>());
        beforeImage.Convert(Image.Format.Rgba8); afterImage.Convert(Image.Format.Rgba8);
        byte[] beforeRgba = beforeImage.GetData(), afterRgba = afterImage.GetData();
        byte[] Color(byte[] data, int[] p) => data.AsSpan((p[1] * image.GetWidth() + p[0]) * 4, 4).ToArray();
        File.WriteAllText(output, JsonSerializer.Serialize(new
        {
            Schema = "six-pixel-native-ownership-v1",
            Frame = targetFrame,
            InputDigest = current["InputDigest"]!.GetValue<string>(),
            Controls = "retained before/after and restored native pose/camera/placement/terrain fields exact; native automatic counters normalized only",
            BeforeGeometry = "preexisting native geometry reconstructed from identical retained controls and immutable imported assets; new decoration omitted only from the before candidate set, never an after exemption",
            ImmutableInputs = immutableInputs,
            NativeCamera = new { Origin = V(camera.GlobalPosition), X = V(camera.GlobalBasis.X), Y = V(camera.GlobalBasis.Y), Z = V(camera.GlobalBasis.Z), camera.Size },
            Near = camera.Near,
            Far = camera.Far,
            AssumedD24WorldStep = (camera.Far - camera.Near) / 16777215,
            GlyphInventory = targetFrame >= 499 ? glyphInventory : null,
            Limitations = "ray/triangle/pixel-center evidence, not a GPU object-id/depth readback; skinned buffers use native bones/binds only with identity relative transform; glyph ambiguity is retained; no mask correction or acceptance",
            Pixels = pixels.Select((p, i) => new
            {
                Pixel = p,
                RayOrigin = V(origins[i]),
                RayDirection = V(directions[i]),
                BeforeRgba = Color(beforeRgba, p),
                AfterRgba = Color(afterRgba, p),
                RestoredRgba = Color(rgba, p),
                BeforeCandidates = hits[i].Where(h => !h.NewDecoration).OrderBy(h => h.Depth).Take(12).ToArray(),
                AfterCandidates = hits[i].OrderBy(h => h.Depth).Take(12).ToArray(),
                Unknown = unknown.Where(u => u.Pixels.Contains(i)).ToArray()
            }).ToArray()
        }, Game.Core.WireJson.Options));
    }
}
