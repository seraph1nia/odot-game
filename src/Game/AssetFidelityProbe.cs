using System.Text.Json;
using Game.Core;
using Godot;

namespace Game;

// Checked-in, owned, headless consumer of exactly the runtime consolidation API.
internal static class AssetFidelityProbe
{
    internal static Task Run(Node owner)
    {
        try
        {
            var pacing = new VerificationPacing(System.Environment.GetEnvironmentVariable("ODOT_OWNED_DATA"),
                System.Environment.GetEnvironmentVariable("ODOT_VERIFICATION_MARKER"), System.Environment.GetEnvironmentVariable("ODOT_VERIFICATION_TOKEN"));
            if (DisplayServer.GetName() != "headless" || !pacing.Owned) throw new InvalidOperationException("Asset fidelity requires an owned headless verification process.");
            RunGeometry(owner);
        }
        catch (Exception error) { Main.Emit(new GameEvent("error", Message: error.Message)); owner.GetTree().Quit(1); }
        return Task.CompletedTask;
    }
    private static void RunGeometry(Node owner)
    {
        RunGlyphDuplicateRegression();
        var reports = new List<object>();
        foreach (string path in AssetCatalog.RequiredPaths.Where(p => !p.StartsWith("characters/", StringComparison.Ordinal)))
        {
            Node3D original = GD.Load<PackedScene>(AssetCatalog.Root + path).Instantiate<Node3D>();
            try
            {
                StaticGeometry.Snapshot control = StaticGeometry.Capture(original, auditMirrors: true);
                using ArrayMesh combined = StaticGeometry.Append(control);
                var assessment = StaticGeometry.Inspect(control, combined);
                StaticGeometryReport report = assessment.Report;
                if (path is AssetCatalog.Meadow or AssetCatalog.Stream) StaticGeometry.Validate(control, combined);
                double scale = path.StartsWith("buildings/", StringComparison.Ordinal)
                    ? 1.9 / Math.Max(control.Positions.Max(p => p.X) - control.Positions.Min(p => p.X), control.Positions.Max(p => p.Z) - control.Positions.Min(p => p.Z)) : VillageLayout.TerrainScale;
                // Diagnose the old collision-face guard without treating welded
                // collision vertices as the authoritative rendering buffers.
                var collision = new List<Vector3>();
                void Faces(Node3D node, Transform3D parent)
                {
                    Transform3D transform = parent * node.Transform;
                    if (node is MeshInstance3D { Mesh: { } mesh }) collision.AddRange(mesh.GetFaces().Select(p => transform * p));
                    foreach (Node3D child in node.GetChildren().OfType<Node3D>()) Faces(child, transform);
                }
                Faces(original, Transform3D.Identity); Vector3[] welded = combined.GetFaces();
                Vector3 a = collision.Aggregate((a, b) => a.Min(b)), b = welded.Aggregate((a, b) => a.Min(b));
                reports.Add(new
                {
                    Path = path,
                    Report = report,
                    Errors = assessment.Errors,
                    Disposition = path is AssetCatalog.Meadow or AssetCatalog.Stream ? "validated terrain batching" : "optional consolidation deferred; original authored hierarchy retained",
                    PlacementScale = scale,
                    WorldPositionError = report.PositionError * scale,
                    WorldPositionAllowance = report.PositionAllowance * scale,
                    CollisionMinimumControl = new[] { a.X, a.Y, a.Z },
                    CollisionMinimumCombined = new[] { b.X, b.Y, b.Z }
                });
            }
            catch (InvalidOperationException error) when (path is not (AssetCatalog.Meadow or AssetCatalog.Stream))
            {
                Vector3[] positions = StaticGeometry.Positions(original);
                reports.Add(new { Path = path, Disposition = "optional consolidation rejected; original authored hierarchy retained", Reason = error.Message, OriginalVertices = positions.Length });
            }
            finally { original.Free(); }
        }
        var regressions = new List<string>();
        Node3D fixture = Fixture();
        try
        {
            StaticGeometry.Snapshot control = StaticGeometry.Capture(fixture);
            using ArrayMesh combined = StaticGeometry.Consolidate(control);
            regressions.Add("near-zero rendering buffers round-trip");
            void Reject(string name, Action action)
            {
                bool rejected = false;
                try { action(); } catch (InvalidOperationException) { rejected = true; }
                if (!rejected) throw new InvalidOperationException("Fidelity negative regression accepted: " + name);
                regressions.Add(name);
            }
            ArrayMesh Mutate(Action<Godot.Collections.Array> change)
            {
                Godot.Collections.Array arrays = combined.SurfaceGetArrays(0); change(arrays);
                var mesh = new ArrayMesh(); mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
                mesh.SurfaceSetMaterial(0, combined.SurfaceGetMaterial(0)); return mesh;
            }
            foreach (string mutation in new[] { "displacement", "scale", "winding", "lost-triangle", "lost-vertex", "normal", "uv" })
            {
                using ArrayMesh changed = Mutate(arrays =>
                {
                    if (mutation == "lost-vertex")
                    {
                        arrays[(int)Mesh.ArrayType.Vertex] = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array()[..^1];
                        arrays[(int)Mesh.ArrayType.Normal] = arrays[(int)Mesh.ArrayType.Normal].AsVector3Array()[..^1];
                        arrays[(int)Mesh.ArrayType.Tangent] = arrays[(int)Mesh.ArrayType.Tangent].AsFloat32Array()[..^4];
                        arrays[(int)Mesh.ArrayType.TexUV] = arrays[(int)Mesh.ArrayType.TexUV].AsVector2Array()[..^1];
                        arrays[(int)Mesh.ArrayType.Index] = arrays[(int)Mesh.ArrayType.Index].AsInt32Array()[..3];
                    }
                    else if (mutation is "displacement" or "scale")
                    {
                        Vector3[] values = arrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
                        for (int i = 0; i < values.Length; i++) values[i] = mutation == "scale" ? values[i] * 1.01f : values[i] + new Vector3(.01f, 0, 0);
                        arrays[(int)Mesh.ArrayType.Vertex] = values;
                    }
                    else if (mutation is "winding" or "lost-triangle")
                    {
                        int[] values = arrays[(int)Mesh.ArrayType.Index].AsInt32Array();
                        if (mutation == "winding") (values[0], values[1]) = (values[1], values[0]); else values = values[..^3];
                        arrays[(int)Mesh.ArrayType.Index] = values;
                    }
                    else if (mutation == "normal")
                    { Vector3[] values = arrays[(int)Mesh.ArrayType.Normal].AsVector3Array(); values[0] = -values[0]; arrays[(int)Mesh.ArrayType.Normal] = values; }
                    else
                    { Vector2[] values = arrays[(int)Mesh.ArrayType.TexUV].AsVector2Array(); values[0] += Vector2.One * .1f; arrays[(int)Mesh.ArrayType.TexUV] = values; }
                });
                Reject(mutation, () => StaticGeometry.Validate(control, changed));
            }
            using var wrongMaterial = new StandardMaterial3D();
            combined.SurfaceSetMaterial(0, wrongMaterial);
            Reject("material assignment", () => StaticGeometry.Validate(control, combined));
            combined.SurfaceSetMaterial(0, control.Surfaces.Keys.Single().Material);
            fixture.Position += new Vector3(.01f, 0, 0);
            Reject("hierarchy transform corruption", () => StaticGeometry.Validate(StaticGeometry.Capture(fixture), combined));
            fixture.Scale = new(-1, 1, 1);
            Reject("mirrored hierarchy explicitly requires original geometry", () => StaticGeometry.Consolidate(StaticGeometry.Capture(fixture, auditMirrors: true)));
            fixture.Scale = Vector3.One; fixture.Position = new(float.NaN, 0, 0);
            Reject("nonfinite hierarchy", () => StaticGeometry.Capture(fixture));
            Node3D skinned = GD.Load<PackedScene>(AssetCatalog.Root + "characters/knight.glb").Instantiate<Node3D>();
            try { Reject("actual animated/skinned authored character", () => StaticGeometry.Capture(skinned)); } finally { skinned.Free(); }
        }
        finally { fixture.Free(); }
        Node3D transformed = Fixture();
        try
        {
            transformed.Transform = new(Basis.FromEuler(new Vector3(.19f, .31f, -.2f)).Scaled(Vector3.One * .73f), new(17.125f, -.000017f, -21.375f));
            using ArrayMesh mesh = StaticGeometry.Consolidate(StaticGeometry.Capture(transformed));
            reports.Add(new { Path = "translated-rotated-uniform-hierarchy", Report = StaticGeometry.Validate(StaticGeometry.Capture(transformed), mesh) });
            transformed.Scale = new(1.5f, .75f, 2.3f);
            var control = StaticGeometry.Capture(transformed);
            using ArrayMesh nonuniform = StaticGeometry.Append(control);
            var assessment = StaticGeometry.Inspect(control, nonuniform);
            if (assessment.Errors.Length == 0) throw new InvalidOperationException("Nonuniform AppendFrom defect unexpectedly absent; review optional consolidation deferral.");
            reports.Add(new { Path = "translated-rotated-nonuniform-hierarchy", Report = assessment.Report, Errors = assessment.Errors, Disposition = "original hierarchy retained; transformed normals reject consolidation" });
            regressions.Add("nonuniform transform normal corruption rejected; source retained");
        }
        finally { transformed.Free(); }
        object framePersistence = FramePersistenceProof.Run();
        Main.Emit(new GameEvent("asset-fidelity", Message: JsonSerializer.Serialize(new { Reports = reports, Regressions = regressions, FramePersistence = framePersistence }, WireJson.Options)));
        owner.GetTree().Quit();
    }
    private static void RunGlyphDuplicateRegression()
    {
        foreach (string description in new[] { "", "Native nonempty glyph description" })
        {
            var parent = new Node3D();
            using var material = new StandardMaterial3D();
            using var changedMaterial = new StandardMaterial3D();
            var original = new Label3D
            {
                Name = "OriginalGlyphFixture",
                Text = "Metal mine L1",
                EditorDescription = description,
                Font = ThemeDB.FallbackFont,
                MaterialOverride = material,
                Position = new Vector3(1, 2, 3)
            };
            // Metadata is a stored Variant property, allowing a real type mismatch
            // without a typed Label3D setter coercing the test input back to String.
            original.SetMeta("glyph_fidelity_type", 1);
            parent.AddChild(original);
            Transform3D transform = original.Transform;
            var copy = (Label3D)original.Duplicate(0);
            try
            {
                void Observe(string name, string stage)
                {
                    using Variant expected = original.Get(name);
                    using Variant actual = copy.Get(name);
                    GD.Print("Native glyph duplicate " + name + " " + stage + ": " + JsonSerializer.Serialize(new
                    {
                        OriginalType = expected.VariantType.ToString(),
                        OriginalValue = GD.VarToStr(expected),
                        CopyType = actual.VariantType.ToString(),
                        CopyValue = GD.VarToStr(actual),
                        OriginalPixelSizeBits = BitConverter.SingleToInt32Bits(original.PixelSize),
                        CopyPixelSizeBits = BitConverter.SingleToInt32Bits(copy.PixelSize)
                    }, WireJson.Options));
                }
                void RequireOriginalUntouched()
                {
                    using Variant metadata = original.GetMeta("glyph_fidelity_type");
                    if (original.GetParent() != parent || original.GetIndex() != 0 || original.Name != "OriginalGlyphFixture"
                        || original.Transform != transform || !original.Visible || original.Text != "Metal mine L1"
                        || original.EditorDescription != description || original.Font != ThemeDB.FallbackFont
                        || original.MaterialOverride != material || original.MaterialOverlay is not null
                        || metadata.VariantType != Variant.Type.Int || metadata.AsInt64() != 1)
                        throw new InvalidOperationException("Native glyph duplicate regression changed the original.");
                }
                Observe("editor_description", "faithful duplicate");
                RequireOriginalUntouched();
                PixelGlyphCompletion.RequireDuplicateProperties(original, copy);
                GD.Print("PASS: native faithful duplicate, including " + JsonSerializer.Serialize(description) + " String");
                void Reject(string name, Action mutate, Action restore)
                {
                    string? rejection = null;
                    try
                    {
                        mutate();
                        Observe(name, "actual mismatch");
                        try { PixelGlyphCompletion.RequireDuplicateProperties(original, copy); }
                        catch (InvalidDataException error) { rejection = error.Message; }
                    }
                    finally { restore(); }
                    if (rejection != "Native glyph duplicate property differs: " + name)
                        throw new InvalidOperationException("Native glyph duplicate regression did not reject its actual changed property: " + name);
                    RequireOriginalUntouched();
                    PixelGlyphCompletion.RequireDuplicateProperties(original, copy);
                    GD.Print("PASS: rejected " + name + "; exact duplicate restored and original untouched");
                }
                Reject("editor_description", () => copy.EditorDescription = "different", () => copy.EditorDescription = original.EditorDescription);
                Reject("text", () => copy.Text = "different glyphs", () => copy.Text = original.Text);
                Reject("metadata/glyph_fidelity_type", () => copy.SetMeta("glyph_fidelity_type", 1.0), () => copy.SetMeta("glyph_fidelity_type", 1));
                Reject("pixel_size", () => copy.PixelSize = MathF.BitIncrement(original.PixelSize), () => copy.PixelSize = original.PixelSize);
                Reject("material_override", () => copy.MaterialOverride = changedMaterial, () => copy.MaterialOverride = material);
                GD.Print("PASS: native glyph duplicate stored-property fidelity, original immutability and real property/resource mismatch rejection");
            }
            finally { copy.Free(); parent.Free(); }
        }
    }
    private static Node3D Fixture()
    {
        var root = new Node3D();
        var arrays = new Godot.Collections.Array(); arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = new Vector3[] { new(-1.965f, -.152565f, -1.415f), new(2.122075f, .000031f, 2.1958f), new(.12345f, .72f, -.321f), new(.2f, .41f, .62f) };
        Vector3 normal = new Vector3(.23f, .68f, .91f).Normalized(), tangent = normal.Cross(Vector3.Up).Normalized();
        arrays[(int)Mesh.ArrayType.Normal] = Enumerable.Repeat(normal, 4).ToArray();
        arrays[(int)Mesh.ArrayType.Tangent] = Enumerable.Range(0, 4).SelectMany(_ => new[] { tangent.X, tangent.Y, tangent.Z, 1f }).ToArray();
        arrays[(int)Mesh.ArrayType.TexUV] = new Vector2[] { Vector2.Zero, Vector2.Right, Vector2.One, Vector2.Down };
        arrays[(int)Mesh.ArrayType.Index] = new[] { 0, 1, 2, 1, 3, 2 };
        var mesh = new ArrayMesh(); mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays); mesh.SurfaceSetMaterial(0, new StandardMaterial3D());
        root.AddChild(new MeshInstance3D { Mesh = mesh }); return root;
    }
}
