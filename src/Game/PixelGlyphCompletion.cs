using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;

namespace Game;

// Fixed six-pixel follow-up only. Original Label3D nodes/fonts/shaders are used
// unchanged in an empty transparent owned target, then restored in finally.
internal static class PixelGlyphCompletion
{
    internal static object Describe(Tabletop scene, Camera3D camera, Label3D label)
    {
        static float[] V(Vector3 v) => [v.X, v.Y, v.Z];
        static float[] C(Color c) => [c.R, c.G, c.B, c.A];
        Font font = label.Font ?? ThemeDB.FallbackFont;
        return new
        {
            Node = scene.GetPathTo(label).ToString(),
            label.Text,
            Font = new { Class = font.GetClass(), font.ResourcePath, Name = font.GetFontName(), Style = font.GetFontStyleName(), Fallback = label.Font is null },
            label.FontSize,
            label.PixelSize,
            label.OutlineSize,
            Modulate = C(label.Modulate),
            OutlineModulate = C(label.OutlineModulate),
            label.NoDepthTest,
            label.Shaded,
            label.DoubleSided,
            label.FixedSize,
            Billboard = label.Billboard.ToString(),
            AlphaCut = label.AlphaCut.ToString(),
            label.AlphaScissorThreshold,
            label.AlphaHashScale,
            label.RenderPriority,
            label.OutlineRenderPriority,
            Global = new { Origin = V(label.GlobalPosition), X = V(label.GlobalBasis.X), Y = V(label.GlobalBasis.Y), Z = V(label.GlobalBasis.Z) },
            ApproximateCameraDepth = -camera.GlobalBasis.Z.Dot(label.GlobalPosition - camera.GlobalPosition)
        };
    }
    internal static void RequireMineTitle(Label3D[] labels)
    {
        if (labels.Length != 1) throw new InvalidDataException("Frame399 requires exactly its recorded unique mine title; no candidate may be dropped.");
        Label3D label = labels[0];
        Node3D? mine = label.GetParent().GetNodeOrNull<Node3D>("Slot2");
        if (label.GetParent().Name != "Buildings" || mine is null || !mine.HasMeta("asset")
            || mine.GetMeta("asset").AsString() != "buildings/metal_mine.glb" || label.Text != "Metal mine L1"
            || label.Position != new Vector3(mine.Position.X, LandscapeAssets.Bounds(mine).End.Y + .28f, mine.Position.Z))
            throw new InvalidDataException("Current recorded glyph is not the original native Slot2/mine-title identity/pose.");
    }
    internal static JsonNode DescribeMapped(Tabletop scene, Camera3D camera, Label3D label)
    {
        JsonNode description = JsonNode.Parse(JsonSerializer.Serialize(Describe(scene, camera, label), Game.Core.WireJson.Options))!;
        description["Identity"] = JsonNode.Parse(JsonSerializer.Serialize(scene.GlyphIdentity(label), Game.Core.WireJson.Options));
        return description;
    }
    internal static async Task Run(Tabletop scene, string witnessPath, string outputPrefix, string frozenObservation)
    {
        JsonNode witness = JsonNode.Parse(File.ReadAllText(witnessPath))!;
        int targetFrame = witness["Frame"]!.GetValue<int>();
        bool remaining = targetFrame is 499 or 599;
        if (targetFrame is not (299 or 399 or 499 or 599) || witness["Pixels"] is not JsonArray samples || samples.Count is < 1 or > 64 || !remaining && samples.Count != 6)
            throw new InvalidDataException("Glyph completion requires the exact six-pixel native witness.");
        string[] names = samples.SelectMany(p => p!["Unknown"]!.AsArray())
            .Select(u => u!["Node"]!.GetValue<string>()).Distinct(StringComparer.Ordinal).ToArray();
        if (!remaining && names.Length != (targetFrame == 299 ? 2 : 1)) throw new InvalidDataException("Glyph completion requires the complete frame-specific recorded candidate set.");
        Label3D[] labels = names.Select(n => scene.GetNodeOrNull<Label3D>(n) ?? throw new InvalidDataException("Recorded label unavailable: " + n)).ToArray();
        if (targetFrame == 399) RequireMineTitle(labels);
        Camera3D originalCamera = scene.GetViewport().GetCamera3D() ?? throw new InvalidDataException("Missing native camera.");
        object[] metadata = labels.Select(label => remaining ? (object)DescribeMapped(scene, originalCamera, label) : Describe(scene, originalCamera, label)).ToArray();
        JsonNode descriptions = JsonNode.Parse(JsonSerializer.Serialize(metadata, Game.Core.WireJson.Options))!;
        string[]? roles = remaining ? descriptions.AsArray().Select(d => d!["Identity"]!["Key"]!.GetValue<string>()).ToArray() : null;
        if (remaining) File.WriteAllText(outputPrefix + ".descriptors.json", descriptions.ToJsonString());
        if (remaining && labels.Length == 0)
        {
            if (witness["GlyphInventory"] is not JsonArray) throw new InvalidDataException("Missing complete visible glyph inventory.");
            File.WriteAllText(outputPrefix + ".glyphs.json", JsonSerializer.Serialize(new
            {
                Schema = "six-pixel-glyph-coverage-v1",
                Frame = targetFrame,
                InputDigest = witness["InputDigest"]!.GetValue<string>(),
                Labels = Array.Empty<object>(),
                RoleIdentities = roles,
                Captures = Array.Empty<object>(),
                NoPossibleGlyphs = true,
                GlyphInventory = witness["GlyphInventory"],
                Restored = true,
                CameraExact = true,
                ControlsRestored = true,
                Method = "complete current visible geometry/control candidate inventory reports no possible glyph; no target/capture/mutation fabricated"
            }, Game.Core.WireJson.Options));
            return;
        }
        int[][] pixels = samples.Select(p => p!["Pixel"]!.AsArray().Select(v => v!.GetValue<int>()).ToArray()).ToArray();
        var saved = labels.Select(label => new
        {
            Label = label,
            Parent = label.GetParent(),
            Index = label.GetIndex(),
            label.Name,
            Local = label.Transform,
            Global = label.GlobalTransform,
            label.Visible
        }).ToArray();
        Viewport source = scene.GetViewport();
        var target = new SubViewport
        {
            Name = "OwnedSixPixelGlyphTarget",
            Size = new Vector2I((int)source.GetVisibleRect().Size.X, (int)source.GetVisibleRect().Size.Y),
            TransparentBg = true,
            OwnWorld3D = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            Msaa3D = source.Msaa3D,
            ScreenSpaceAA = source.ScreenSpaceAA,
            UseTaa = source.UseTaa,
            UseDebanding = source.UseDebanding
        };
        scene.AddChild(target);
        var camera = new Camera3D
        {
            Transform = originalCamera.GlobalTransform,
            Projection = originalCamera.Projection,
            Size = originalCamera.Size,
            Near = originalCamera.Near,
            Far = originalCamera.Far,
            KeepAspect = originalCamera.KeepAspect,
            HOffset = originalCamera.HOffset,
            VOffset = originalCamera.VOffset,
            CullMask = originalCamera.CullMask,
            Current = true
        };
        target.AddChild(camera);
        var captures = new List<object>(); byte[]? empty = null;
        bool cameraExact = false;
        async Task Capture(string name)
        {
            await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using Image image = target.GetTexture().GetImage(); image.Convert(Image.Format.Rgba8);
            if (image.SavePng(outputPrefix + "." + name + ".png") != Error.Ok) throw new IOException("Glyph witness persistence failed.");
            byte[] data = image.GetData();
            if (name == "empty")
            {
                empty = data;
                cameraExact = camera.GlobalTransform == originalCamera.GlobalTransform && target.GetVisibleRect().Size == source.GetVisibleRect().Size
                    && pixels.All(p => camera.ProjectRayOrigin(new(p[0] + .5f, p[1] + .5f)) == originalCamera.ProjectRayOrigin(new(p[0] + .5f, p[1] + .5f))
                        && camera.ProjectRayNormal(new(p[0] + .5f, p[1] + .5f)) == originalCamera.ProjectRayNormal(new(p[0] + .5f, p[1] + .5f)));
                if (!cameraExact) throw new InvalidDataException("Glyph target camera/projection differs at the six samples.");
            }
            int alphaPixels = 0, changedPixels = 0;
            for (int i = 0; i < data.Length; i += 4)
            {
                if (data[i + 3] != 0) alphaPixels++;
                if (empty is not null && !data.AsSpan(i, 4).SequenceEqual(empty.AsSpan(i, 4))) changedPixels++;
            }
            captures.Add(new
            {
                Name = name,
                Width = image.GetWidth(),
                Height = image.GetHeight(),
                AlphaPixels = alphaPixels,
                ChangedPixels = changedPixels,
                PixelHash = Convert.ToHexString(SHA256.HashData(data)),
                Samples = pixels.Select(p => new
                {
                    Pixel = p,
                    NeighborhoodMaximumAlpha = Enumerable.Range(-2, 5).SelectMany(y => Enumerable.Range(-2, 5).Select(x => data[((p[1] + y) * image.GetWidth() + p[0] + x) * 4 + 3])).Max(),
                    Rgba = data.AsSpan((p[1] * image.GetWidth() + p[0]) * 4, 4).ToArray().Select(b => (int)b).ToArray(),
                    ChangedFromEmpty = !data.AsSpan((p[1] * image.GetWidth() + p[0]) * 4, 4).SequenceEqual(empty!.AsSpan((p[1] * image.GetWidth() + p[0]) * 4, 4))
                }).ToArray()
            });
        }
        bool restored = false;
        try
        {
            await Capture("empty");
            for (int i = 0; i < labels.Length; i++)
            {
                Label3D label = labels[i]; label.Reparent(target, keepGlobalTransform: true); label.GlobalTransform = saved[i].Global;
                await Capture("label" + i);
                label.Reparent(saved[i].Parent, keepGlobalTransform: true); label.Transform = saved[i].Local;
                saved[i].Parent.MoveChild(label, saved[i].Index);
            }
            if (targetFrame == 299)
            {
                for (int i = 0; i < labels.Length; i++) { labels[i].Reparent(target, keepGlobalTransform: true); labels[i].GlobalTransform = saved[i].Global; }
                await Capture("both");
            }
        }
        finally
        {
            foreach (var item in saved)
            {
                if (item.Label.GetParent() != item.Parent) item.Label.Reparent(item.Parent, keepGlobalTransform: true);
                item.Label.Transform = item.Local; item.Label.Visible = item.Visible;
                item.Parent.MoveChild(item.Label, item.Index);
            }
            scene.RemoveChild(target); target.Free();
            restored = saved.All(s => s.Label.GetParent() == s.Parent && s.Label.Transform == s.Local && s.Label.GlobalTransform == s.Global && s.Label.Visible == s.Visible && s.Label.Name == s.Name && s.Label.GetIndex() == s.Index);
        }
        await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        var fields = new Dictionary<string, object?>(); var uiTargets = new Dictionary<string, object>(); scene.AppendUiObservation(fields, uiTargets);
        string current = JsonSerializer.Serialize(new { InputDigest = witness["InputDigest"]!.GetValue<string>(), Frame = targetFrame, Fields = fields }, Game.Core.WireJson.Options);
        File.WriteAllText(outputPrefix + ".restored-observation.json", current);
        bool controlsRestored = JsonNode.DeepEquals(JsonNode.Parse(frozenObservation), JsonNode.Parse(current));
        using Image full = source.GetTexture().GetImage();
        if (full.SavePng(outputPrefix + ".restored.png") != Error.Ok) throw new IOException("Restored view persistence failed.");
        File.WriteAllText(outputPrefix + ".glyphs.json", JsonSerializer.Serialize(new
        {
            Schema = "six-pixel-glyph-coverage-v1",
            Frame = targetFrame,
            InputDigest = witness["InputDigest"]!.GetValue<string>(),
            Labels = metadata,
            MineTitleIdentity = targetFrame == 399 ? "Slot2/mine-title" : null,
            RoleIdentities = roles,
            Captures = captures,
            Restored = restored,
            NodeRestoration = saved.Select(s => new
            {
                OriginalName = s.Name.ToString(),
                ActualName = s.Label.Name.ToString(),
                ParentSame = s.Label.GetParent() == s.Parent,
                SiblingSame = s.Label.GetIndex() == s.Index,
                LocalSame = s.Label.Transform == s.Local,
                GlobalSame = s.Label.GlobalTransform == s.Global,
                VisibleSame = s.Label.Visible == s.Visible
            }).ToArray(),
            CameraExact = cameraExact,
            ControlsRestored = controlsRestored,
            Method = "original native Label3D nodes, unchanged fonts/materials/flags/global transforms, same camera/projection/resolution; only temporary reparenting into empty transparent owned world",
            Limits = "glyph-only native raster coverage (including outlines) without roof depth occlusion; depth-tested labels can contribute only if covered and closer than the opaque roof; no mask correction or acceptance"
        }, Game.Core.WireJson.Options));
        if (!restored || !controlsRestored) throw new InvalidDataException("Glyph isolation did not restore exact node/control state.");
    }
}
