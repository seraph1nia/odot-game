using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;

namespace Game;

// Four owned captured counterfactuals in the same frozen view. No synthetic
// pixels, production asset mutations, alternate light rig or timeout changes.
internal static class NativeBoundaryControls
{
    internal static async Task Run(Tabletop scene, string requestPath, string currentJson, string output)
    {
        JsonNode request = JsonNode.Parse(File.ReadAllText(requestPath))!, current = JsonNode.Parse(currentJson)!;
        JsonNode before = JsonNode.Parse(File.ReadAllText(request["BeforeObservation"]!.GetValue<string>()))!;
        using Image original = Image.LoadFromFile(request["BeforePng"]!.GetValue<string>()); original.Convert(Image.Format.Rgba8);
        byte[] beforePixels = original.GetData(); int width = original.GetWidth(), height = original.GetHeight();
        var results = new List<object>();
        async Task<Image> Capture(string name)
        {
            await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            Image image = scene.GetViewport().GetTexture().GetImage(); image.Convert(Image.Format.Rgba8);
            if (image.SavePng(output + "." + name + ".png") != Error.Ok) { image.Dispose(); throw new IOException("Native control capture failed."); }
            return image;
        }
        using Image baseline = await Capture("positive");
        NonDefenseRoofEvidence evidence = NativeRoofAttestation.Build(scene, requestPath, currentJson, baseline, output);
        int protectedPixels = LandscapeBoundaryProof.Verify(before, current, width, height, beforePixels, baseline.GetData(), evidence);
        results.Add(new { Name = "legitimate-preexisting-roof-family", Result = "passed", ProtectedPixels = protectedPixels });
        byte[] fixedPixels = baseline.GetData();
        async Task Reject(string name, Action install, Action restore)
        {
            try
            {
                install();
                using Image changed = await Capture(name);
                byte[] pixels = changed.GetData(); int changedProtected = 0;
                for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                    if (LandscapeBoundaryProof.ProtectedPixel(current["Fields"]!["Camera"]!, height, x, y)
                        && !pixels.AsSpan((y * width + x) * 4, 4).SequenceEqual(fixedPixels.AsSpan((y * width + x) * 4, 4))) changedProtected++;
                if (changedProtected < 100) throw new InvalidDataException("Native counterfactual failed to affect a meaningful protected raster: " + name);
                string? rejection = null;
                try { LandscapeBoundaryProof.Verify(before, current, width, height, beforePixels, pixels, evidence); }
                catch (InvalidDataException e) { rejection = e.Message; }
                if (rejection is null) throw new InvalidDataException("Native mutation was not rejected: " + name);
                results.Add(new { Name = name, Result = "rejected", ChangedProtectedPixels = changedProtected, Reason = rejection });
            }
            finally { restore(); }
            using Image restored = await Capture(name + "-restored");
            LandscapeBoundaryProof.Verify(before, current, width, height, beforePixels, restored.GetData(), evidence);
        }
        try
        {
            UnitView unit = scene.FindChildren("*", "Node3D", true, false).OfType<UnitView>().First(u => u.IsVisibleInTree());
            MeshInstance3D mesh = unit.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>().First(m => m.IsVisibleInTree() && m.Mesh is not null);
            Material? unitMaterial = mesh.MaterialOverride;
            using var magenta = new StandardMaterial3D { AlbedoColor = new Color(1, 0, 1), ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
            await Reject("protected-unit-alteration", () => mesh.MaterialOverride = magenta, () => mesh.MaterialOverride = unitMaterial);
            MultiMeshInstance3D terrain = NativeTerrainReceiver.Select(scene, current, output + ".terrain-receiver.json");
            Material? terrainMaterial = terrain.MaterialOverride;
            await Reject("protected-terrain-alteration", () => terrain.MaterialOverride = magenta, () => terrain.MaterialOverride = terrainMaterial);
            if (terrain.MaterialOverride != terrainMaterial) throw new InvalidDataException("Actual terrain owner/material did not restore.");
            Camera3D camera = scene.GetViewport().GetCamera3D()!;
            Vector3 target = unit.GlobalPosition + new Vector3(0, .7f, 0);
            using var cube = new BoxMesh { Size = new Vector3(1.5f, 1.5f, 1.5f) };
            var occluder = new MeshInstance3D
            {
                Name = "OwnedOutsidePropOcclusionControl",
                Mesh = cube,
                MaterialOverride = magenta,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };
            occluder.SetMeta("pocket", "temporary diagnostic-only new outside prop");
            await Reject("new-outside-prop-occludes-protected-unit", () => { scene.AddChild(occluder); occluder.GlobalPosition = target + camera.GlobalBasis.Z * 2; },
                () => { if (occluder.GetParent() is not null) scene.RemoveChild(occluder); occluder.Free(); });
            DirectionalLight3D sun = scene.FindChildren("*", "DirectionalLight3D", true, false).OfType<DirectionalLight3D>().First(l => l.IsVisibleInTree() && l.ShadowEnabled);
            var caster = new MeshInstance3D { Name = "OwnedShadowOnlyControl", Mesh = cube, CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly };
            Vector3 receiver = new(-4, .05f, -8);
            await Reject("real-new-shadow-on-protected-receiver", () =>
                { scene.AddChild(caster); caster.GlobalPosition = receiver + sun.GlobalBasis.Z * (2 / sun.GlobalBasis.Z.Y); },
                () => { if (caster.GetParent() is not null) scene.RemoveChild(caster); caster.Free(); });
        }
        finally
        {
            var fields = new Dictionary<string, object?>(); var targets = new Dictionary<string, object>(); scene.AppendUiObservation(fields, targets);
            JsonNode restored = JsonNode.Parse(JsonSerializer.Serialize(new { InputDigest = current["InputDigest"]!.GetValue<string>(), Frame = 299, Fields = fields }, Game.Core.WireJson.Options))!;
            bool exact = JsonNode.DeepEquals(current, restored);
            File.WriteAllText(output + ".controls.json", JsonSerializer.Serialize(new
            {
                Schema = "owned-native-boundary-controls-v1",
                Results = results,
                ExactObservedRestoration = exact,
                Method = "same frozen frame299/camera/light; actual material overrides, new visible occluder and shadow-only native caster; every mutation restored before next capture",
                Complete = results.Count == 5 && exact
            }, Game.Core.WireJson.Options));
        }
        if (results.Count != 5) throw new InvalidDataException("Native boundary controls incomplete.");
    }
}
