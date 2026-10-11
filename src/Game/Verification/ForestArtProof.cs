using Godot;

namespace Game.Verification;

// Tier B: actual imported receivers/native resources and lifecycle, which pure
// C# cannot observe. Reuses the existing source/package asset child (~seconds,
// not a new journey). Dummy/headless does NOT certify rendered appearance.
internal static class ForestArtProof
{
    internal static object Run(Node owner)
    {
        static void Check(bool value, string message)
        {
            if (!value) throw new InvalidOperationException("Forest art: " + message);
        }
        using var themes = new ForestTheme();
        using var environment = VillageLighting.Environment();
        for (int theme = 0; theme < 3; theme++)
        {
            ImageTexture3D lut = themes.Lut(theme);
            Check(lut.GetWidth() == 16 && lut.GetHeight() == 16 && lut.GetDepth() == 16 && !lut.HasMipmaps(), "real 16-cube SDR LUT without mips");
            Check(ReferenceEquals(lut, themes.Lut(theme)), "LUT reused rather than allocated per frame");
            // Dummy has no Texture3D.GetData GPU readback; inspect the native
            // image inputs used by the production upload, not pretend pixels.
            var slices = ForestTheme.LutSlices(theme);
            try
            {
                Check(slices.Count == 16, "LUT upload inputs contain all blue slices");
                foreach (var (red, green, blue) in new[] { (0, 0, 0), (15, 15, 15), (2, 8, 13), (12, 5, 3) })
                {
                    Color actual = slices[blue].GetPixel(red, green), expected = ForestTheme.Grade(new(red / 15f, green / 15f, blue / 15f), theme);
                    Check(Math.Abs(actual.R - expected.R) < .005f && Math.Abs(actual.G - expected.G) < .005f && Math.Abs(actual.B - expected.B) < .005f,
                        "native LUT upload images follow RGB axes and grading within RGB8 quantization");
                }
            }
            finally { foreach (Image slice in slices) slice.Dispose(); }
            environment.AdjustmentEnabled = true; environment.AdjustmentColorCorrection = lut;
            Check(environment.AdjustmentColorCorrection == lut, "native Environment binds 3D correction");
            Check(ForestTheme.Grade(Colors.Black, theme) == Colors.Black && ForestTheme.Grade(Colors.White, theme) == Colors.White, "grading preserves endpoints");
            foreach (Color color in new[] { new Color("e0dbaf"), new Color("757d78"), Colors.Red, Colors.Green, Colors.Blue })
            {
                Color graded = ForestTheme.Grade(color, theme);
                Check(Math.Abs(color.R - graded.R) < .06 && Math.Abs(color.G - graded.G) < .06 && Math.Abs(color.B - graded.B) < .06, "bounded gameplay-category shifts");
            }
            for (int weather = 0; weather < 5; weather++)
            {
                AtmosphereProfile original = AtmosphereProfile.For(weather), palette = ForestTheme.Palette(original, theme);
                Check(original.Fog == palette.Fog && original.Rain == palette.Rain && original.SunEnergy == palette.SunEnergy && original.Wind == palette.Wind,
                    "themes retain weather intensity, light energy and motion");
            }
        }
        environment.AdjustmentColorCorrection = null;
        var controller = new AtmosphereController(); owner.AddChild(controller);
        try
        {
            var preferences = new GraphicsPreferences();
            controller.Apply(preferences);
            Check(controller.Environment.AdjustmentColorCorrection is ImageTexture3D && controller.GetChildren().OfType<WorldEnvironment>().Count() == 1,
                "existing atmosphere owner binds theme with one WorldEnvironment");
            preferences.Edit("forest_theme", 0); preferences.Edit("magic", 0); controller.Apply(preferences);
            Check(controller.GetChildren().OfType<WorldEnvironment>().Count() == 1 && controller.Magic.Lights.Count == 4,
                "live theme Apply reuses environment and bounded effect owner");
        }
        finally { owner.RemoveChild(controller); controller.Free(); }
        var holder = new Node3D(); owner.AddChild(holder);
        try
        {
            var landscape = new VillageLandscape(holder, new LandscapeAssets());
            var magic = new ForestMagic(); holder.AddChild(magic);
            var camera = new Camera3D { Position = new(18, 22, 30), Projection = Camera3D.ProjectionType.Orthogonal, Size = 28 };
            holder.AddChild(camera); camera.LookAt(Vector3.Zero);
            landscape.Cover(camera, new Rect2(0, 0, 1100, 820), Vector2.Zero);
            Check(landscape.GetChildren().OfType<MultiMeshInstance3D>().Any(batch => (batch.Layers & ForestMagic.GroundReceiverLayer) != 0), "installed opaque terrain batches opt into decal receiving without mesh replacement");
            Check(landscape.MagicSites.Count == 4 && magic.Lights.Count == 4 && magic.Emitters.Count == 3, "bounded rig and fixed focal sites");
            foreach (ForestMagic.Site site in landscape.MagicSites)
            {
                Check(DetailedGround.ClearOfRoutes(site.Ground.X, site.Ground.Z, site.Footprint / 2), "decal footprint clear of river and trail: " + site.Name);
                int row = (int)Math.Round(site.Ground.Z / VillageLayout.RowStep);
                int column = (int)Math.Round((site.Ground.X - Math.Abs(row % 2) * VillageLayout.HalfWidth) / 3);
                Transform3D transform = VillageLayout.GroundTransform(column, row, DetailedGround.FloorTurns(column, row));
                Mesh ground = new LandscapeAssets().Terrain(DetailedGround.Base(column, row));
                foreach (var offset in new[] { Vector3.Zero, new Vector3(-.5f, 0, -.5f), new Vector3(.5f, 0, -.5f), new Vector3(.5f, 0, .5f), new Vector3(-.5f, 0, .5f) })
                {
                    Vector3 point = site.Ground + offset * site.Footprint;
                    float? surface = GroundGeometry.Surface(ground, transform, point);
                    Check(surface is not null && Math.Abs(surface.Value - site.Ground.Y) < .001f, "thin projection has real flat support at all corners: " + site.Name);
                }
                // Construction verifies supported native properties, not active
                // Decal drawing on Dummy/Compatibility. Live rig never allocates
                // those unsupported nodes there.
                Decal decal = ForestMagic.Projection(site);
                try { Check(decal.TextureAlbedo is not null && decal.TextureEmission is null && decal.Size.Y == .12f && decal.CullMask == ForestMagic.GroundReceiverLayer, "projection preserves supplied coverage and receiver restriction"); }
                finally { decal.Free(); }
            }
            var settings = new GraphicsPreferences(); settings.Edit("magic", 2);
            magic.View(landscape); magic.Apply(settings.Effective(false));
            Check(magic.Decals.Count == 0, "unsupported live Compatibility/Dummy rig allocates no decals");
            Check(magic.Lights.All(l => l.Visible && !l.ShadowEnabled && l.DistanceFadeEnabled), "native lights are visible, shadowless and distance bounded");
            Check(magic.Emitters.Sum(p => p.Amount) == 42 && magic.Emitters.All(p => p.LocalCoords && p.UseFixedSeed && p.CastShadow == GeometryInstance3D.ShadowCastingSetting.Off), "42 shared GPU particles, fixed seeds and local lifetime");
            Color dreamingAccent = magic.Lights[0].LightColor;
            settings.Edit("forest_theme", 0); magic.Apply(settings.Effective(false));
            Check(magic.Lights[0].LightColor != dreamingAccent && magic.Emitters.All(p => p.Emitting), "theme recolors only owned accents without changing effect selection");
            settings.Edit("forest_theme", 2); magic.Apply(settings.Effective(false));
            int children = magic.GetChildCount();
            magic.View(landscape); Check(magic.GetChildCount() == children, "repeat view does not duplicate nodes");
            settings.Edit("magic", 0); magic.Apply(settings.Effective(false));
            Check(magic.Lights.All(l => !l.Visible) && magic.Emitters.All(p => !p.Emitting && !p.Visible), "Off hides live particles immediately, not just new emission");
            settings.Edit("magic", 1); magic.Apply(settings.Effective(false));
            Check(magic.Emitters[0].Emitting && magic.Emitters[0].AmountRatio == .5f && !magic.Emitters[2].Emitting, "Subtle halves density and removes rare wisp");
            settings.Edit("reduce_motion", 1); magic.Apply(settings.Effective(false));
            Check(magic.Emitters.All(p => !p.Emitting) && magic.Lights.Any(l => l.Visible), "comfort stops magic motion without removing focal light");
            settings.Edit("reduce_motion", 0); settings.Edit("quality", 0); magic.Apply(settings.Effective(false));
            Check(magic.Emitters.All(p => !p.Emitting) && magic.Lights.Count(l => l.Visible) == 2, "Low bounds optional workload");
            var second = new VillageLandscape(holder, new LandscapeAssets()) { Position = new(100, 0, 0) };
            magic.View(second); Check(magic.Position == second.GlobalPosition && magic.GetChildCount() == children, "city switch reuses rig at new origin");
            magic.View(null); Check(magic.Lights.All(l => !l.Visible) && magic.Emitters.All(p => !p.Visible), "return/absent landscape suppresses owned effects");
            ulong id = magic.GetInstanceId(); holder.RemoveChild(magic); magic.Free();
            Check(!GodotObject.IsInstanceIdValid(id), "effect subtree released on owner disposal");
        }
        finally { owner.RemoveChild(holder); holder.Free(); }
        return new { Themes = 3, LutSize = ForestTheme.LutSize, AccentLights = 4, MaximumDecals = 4, MaximumParticles = 42, Scope = "headless native resource/property and lifecycle proof; not rendered pixels or GPU performance" };
    }
}
