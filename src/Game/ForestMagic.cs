using Godot;

namespace Game;

// A single bounded rig beneath the atmosphere owner. Repositions existing nodes
// to the viewed landscape, never one rig per city/object or one light per particle.
internal sealed partial class ForestMagic : Node3D
{
    internal const uint GroundReceiverLayer = 1u << 19;
    internal static bool DecalsSupported => AtmosphereController.ForwardPlus && RenderingServer.GetRenderingDevice() is not null;
    internal sealed record Site(string Name, Vector3 Ground, Color Color, string Decal, float Footprint);
    private VillageLandscape? _landscape;
    private IReadOnlyList<Site>? _sites;
    private readonly List<OmniLight3D> _lights = [];
    private readonly List<Decal> _decals = [];
    private readonly List<GpuParticles3D> _particles = [];
    private GraphicsPreferences _settings = new();
    private int _level;
    internal IReadOnlyList<OmniLight3D> Lights => _lights;
    internal IReadOnlyList<Decal> Decals => _decals;
    internal IReadOnlyList<GpuParticles3D> Emitters => _particles;

    public override void _Ready()
    {
        Name = "ForestMagic";
        for (int i = 0; i < 4; i++)
        {
            var light = new OmniLight3D
            {
                Name = "Accent" + i,
                OmniRange = i == 1 ? 3 : 2.4f,
                LightEnergy = 0,
                ShadowEnabled = false,
                LightVolumetricFogEnergy = 0,
                DistanceFadeEnabled = true,
                DistanceFadeBegin = 45,
                DistanceFadeLength = 12,
                Visible = false
            };
            AddChild(light); _lights.Add(light);
        }
        _particles.Add(Particles("Fireflies", 24, 8, .045f, new("e4d9a3"), 911));
        _particles.Add(Particles("Pollen", 16, 10, .025f, new("b4d6cf"), 912));
        _particles.Add(Particles("Wisp", 2, 12, .085f, new("b9b2db"), 913));
    }
    internal static Decal Projection(Site site) => new()
    {
        Name = site.Name.Replace(' ', '_') + "Mark",
        Position = site.Ground,
        // Native -Y projection, PNG top corresponds to -Z. Only the supported
        // terrace surface is in this thin volume; units/props are not receivers.
        Size = new(site.Footprint, .12f, site.Footprint),
        CullMask = GroundReceiverLayer,
        TextureAlbedo = GD.Load<Texture2D>("res://Assets/Authored/environment/decals/forest_decal_" + site.Decal + ".png"),
        Modulate = new(1, 1, 1, .55f),
        AlbedoMix = .65f,
        NormalFade = .8f,
        UpperFade = 0,
        LowerFade = 0,
        EmissionEnergy = 0,
        DistanceFadeEnabled = true,
        DistanceFadeBegin = 40,
        DistanceFadeLength = 12,
        Visible = false
    };
    internal void View(VillageLandscape? landscape)
    {
        if (_landscape == landscape && _sites == landscape?.MagicSites) return;
        _landscape = landscape; _sites = landscape?.MagicSites;
        // No world-space leftovers after a city switch, even if the old node
        // has already been queued for deletion. All particles use local coords.
        foreach (GpuParticles3D emitter in _particles) { emitter.Emitting = false; emitter.Visible = false; emitter.Restart(); }
        foreach (Decal decal in _decals) { RemoveChild(decal); decal.Free(); }
        _decals.Clear();
        if (landscape is not null)
        {
            Position = landscape.GlobalPosition;
            for (int i = 0; i < landscape.MagicSites.Count; i++)
            {
                Site site = landscape.MagicSites[i];
                _lights[i].Position = site.Ground + new Vector3(0, i == 1 ? 1.2f : .7f, 0);
                _lights[i].LightColor = site.Color;
                if (DecalsSupported) { Decal decal = Projection(site); AddChild(decal); _decals.Add(decal); }
            }
            _particles[0].Position = landscape.MagicSites[0].Ground + Vector3.Up * .9f;
            _particles[1].Position = landscape.MagicSites[2].Ground + Vector3.Up * .8f;
            _particles[2].Position = landscape.MagicSites[3].Ground + Vector3.Up;
        }
        Apply(_settings);
    }
    internal void Apply(GraphicsPreferences settings)
    {
        _settings = settings.Clone(); _level = settings["magic"];
        if (settings["quality"] == 0) _level = Math.Min(_level, 1);
        bool active = _landscape is not null && _level > 0;
        float strength = _level == 2 ? 1 : .5f;
        for (int i = 0; i < _lights.Count; i++)
        {
            _lights[i].Visible = active && (settings["quality"] != 0 || i < 2);
            _lights[i].LightEnergy = .22f * strength;
            if (_landscape is not null)
                _lights[i].LightColor = _landscape.MagicSites[i].Color.Lerp(new Color("dfd0b4"), settings["forest_theme"] == 0 ? .6f : settings["forest_theme"] == 1 ? .2f : 0);
        }
        for (int i = 0; i < _decals.Count; i++)
        {
            _decals[i].Visible = active && (_level == 2 || i is 0 or 3);
            _decals[i].Modulate = new(1, 1, 1, _level == 2 ? .55f : .3f);
        }
        for (int i = 0; i < _particles.Count; i++)
        {
            var material = (StandardMaterial3D)_particles[i].DrawPass1.SurfaceGetMaterial(0);
            Color color = new(i == 0 ? "e4d9a3" : i == 1 ? "b4d6cf" : "b9b2db");
            material.AlbedoColor = color.Lerp(new Color("dfd0b4"), settings["forest_theme"] == 0 ? .5f : 0);
        }
        UpdateParticles(null);
    }
    public override void _Process(double delta)
    {
        // Pure presentation visibility, not gameplay time or per-projector motion.
        Camera3D? camera = GetViewport().GetCamera3D();
        UpdateParticles(camera);
    }
    private void UpdateParticles(Camera3D? camera)
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            GpuParticles3D emitter = _particles[i];
            bool near = camera is null || camera.GlobalPosition.DistanceTo(emitter.GlobalPosition) < 52;
            bool active = _landscape is not null && _level > 0 && _settings["quality"] != 0
                && _settings["reduce_motion"] == 0 && (i < 2 || _level == 2) && near;
            emitter.Visible = active; emitter.Emitting = active;
            emitter.AmountRatio = _level == 2 ? 1 : .5f;
        }
    }
    private GpuParticles3D Particles(string name, int amount, double lifetime, float size, Color color, uint seed)
    {
        // Shared native quad per behavior, not particle nodes/lights. Tiny soft
        // sprites, no turbulence/attractors/collisions or expensive transparent fog.
        using Image image = Image.CreateEmpty(16, 16, false, Image.Format.Rgba8);
        for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
        {
            float radius = new Vector2((x - 7.5f) / 7.5f, (y - 7.5f) / 7.5f).Length();
            image.SetPixel(x, y, new Color(1, 1, 1, Math.Max(0, 1 - radius) * .8f));
        }
        var curve = new Curve(); curve.AddPoint(new(0, 0)); curve.AddPoint(new(.2f, .8f)); curve.AddPoint(new(.55f, .5f)); curve.AddPoint(new(.8f, .8f)); curve.AddPoint(new(1, 0));
        var material = new StandardMaterial3D
        {
            AlbedoColor = color,
            AlbedoTexture = ImageTexture.CreateFromImage(image),
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            VertexColorUseAsAlbedo = true,
            BillboardMode = BaseMaterial3D.BillboardModeEnum.Particles,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled
        };
        var emitter = new GpuParticles3D
        {
            Name = name,
            Amount = amount,
            Lifetime = lifetime,
            Emitting = false,
            Visible = false,
            UseFixedSeed = true,
            Seed = seed,
            LocalCoords = true,
            FixedFps = 20,
            VisibilityAabb = new(new(-3, -1.5f, -3), new(6, 5, 6)),
            DrawPass1 = new QuadMesh { Size = new(size, size), Material = material },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            ProcessMaterial = new ParticleProcessMaterial
            {
                EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
                EmissionBoxExtents = new(1.1f, .45f, 1.1f),
                Gravity = Vector3.Zero,
                Direction = name == "Pollen" ? Vector3.Up : new(.5f, .2f, .3f),
                Spread = 70,
                InitialVelocityMin = .025f,
                InitialVelocityMax = name == "Pollen" ? .08f : .12f,
                OrbitVelocityMin = name == "Pollen" ? 0 : -.025f,
                OrbitVelocityMax = name == "Pollen" ? 0 : .025f,
                ScaleMin = .65f,
                ScaleMax = 1.1f,
                AlphaCurve = new CurveTexture { Curve = curve }
            }
        };
        AddChild(emitter); return emitter;
    }
    internal object Observe() => new
    {
        Level = _level,
        Lights = _lights.Count(l => l.Visible),
        Decals = _decals.Count(d => d.Visible),
        Emitters = _particles.Count(p => p.Emitting),
        ParticleBudget = _particles.Where(p => p.Emitting).Sum(p => p.Amount * p.AmountRatio),
        DecalsSupported
    };
}
