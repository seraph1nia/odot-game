using Godot;

namespace Game;

// One graphical lifetime owns the environment and sun for menu AND every city.
// Nothing here reads authority state, steps simulation or consumes gameplay RNG.
internal sealed partial class AtmosphereController : Node3D
{
    internal Godot.Environment Environment { get; private set; } = null!;
    private DirectionalLight3D _sun = null!;
    private ProceduralSkyMaterial _sky = null!;
    private GpuParticles3D _rain = null!, _leaves = null!;
    private FogVolume? _riverMist;
    private float _uploadedWind;
    private Color? _uploadedSkyHaze;
    private long _skyUploads;
    private GraphicsPreferences _preferences = new(), _effective = new();
    private AtmosphereProfile _current = AtmosphereProfile.For(0), _target = AtmosphereProfile.For(0);
    private double _time;
    private float? _proofTime;
    private bool _seekParticles;
    internal Vector3 VillageOrigin { get; set; }
    internal static bool ForwardPlus => RenderingServer.GetCurrentRenderingMethod() == "forward_plus";

    public override void _Ready()
    {
        Environment = VillageLighting.Environment();
        _sky = new ProceduralSkyMaterial
        {
            SkyTopColor = new("90afcb"),
            SkyHorizonColor = new("c5d2cf"),
            GroundBottomColor = new("607262"),
            GroundHorizonColor = new("c5d2cf"),
            SunAngleMax = 12
        };
        Environment.BackgroundMode = Godot.Environment.BGMode.Sky;
        Environment.Sky = new Sky { SkyMaterial = _sky, RadianceSize = Sky.RadianceSizeEnum.Size128 };
        Environment.ReflectedLightSource = Godot.Environment.ReflectionSource.Sky;
        Environment.AmbientLightSource = Godot.Environment.AmbientSource.Sky;
        Environment.AmbientLightSkyContribution = .45f;
        Environment.TonemapMode = Godot.Environment.ToneMapper.Filmic;
        Environment.TonemapWhite = 3;
        Environment.AdjustmentEnabled = true;
        Environment.AdjustmentSaturation = .96f;
        Environment.FogMode = Godot.Environment.FogModeEnum.Depth;
        Environment.FogDepthBegin = 48; Environment.FogDepthEnd = 135; Environment.FogDepthCurve = 1.5f;
        Environment.FogHeight = -.5f; Environment.FogSkyAffect = .35f;
        Environment.VolumetricFogLength = 55; Environment.VolumetricFogDetailSpread = 2;
        Environment.VolumetricFogAnisotropy = .25f; Environment.VolumetricFogEmissionEnergy = .03f;
        Environment.VolumetricFogTemporalReprojectionEnabled = true;
        AddChild(new WorldEnvironment { Name = "WoodlandEnvironment", Environment = Environment });
        _sun = VillageLighting.Sun(); _sun.Name = "WoodlandSun";
        _sun.DirectionalShadowMode = DirectionalLight3D.ShadowMode.Parallel4Splits;
        _sun.DirectionalShadowSplit1 = .12f; _sun.DirectionalShadowSplit2 = .3f; _sun.DirectionalShadowSplit3 = .6f;
        _sun.DirectionalShadowBlendSplits = true; _sun.ShadowBias = .035f; _sun.ShadowNormalBias = 1.2f;
        _sun.LightAngularDistance = .7f; _sun.LightVolumetricFogEnergy = .6f;
        AddChild(_sun);
        _rain = Particles("BoundedRain", true); _leaves = Particles("DriftingLeaves", false);
        if (ForwardPlus)
        {
            _riverMist = new FogVolume
            {
                Name = "RiverMist",
                Size = new(14, 1.2f, 2),
                Material = new FogMaterial { Density = .012f, Albedo = new("b7c8ce"), HeightFalloff = 2.5f, EdgeFade = .35f },
                Visible = false
            };
            AddChild(_riverMist);
        }
        Apply(_preferences);
        GD.Print($"Graphics actual: method={RenderingServer.GetCurrentRenderingMethod()}; driver={RenderingServer.GetCurrentRenderingDriverName()}; GPU={RenderingServer.GetVideoAdapterName()}; vendor={RenderingServer.GetVideoAdapterVendor()}; device={RenderingServer.GetVideoAdapterType()}; display={DisplayServer.GetName()}");
    }

    private GpuParticles3D Particles(string name, bool rain)
    {
        var material = new StandardMaterial3D
        {
            AlbedoColor = rain ? new Color(.65f, .78f, .85f, .4f) : new Color("a9b56b"),
            Transparency = rain ? BaseMaterial3D.TransparencyEnum.Alpha : BaseMaterial3D.TransparencyEnum.Disabled,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled
        };
        var particles = new GpuParticles3D
        {
            Name = name,
            Amount = rain ? 160 : 18,
            Lifetime = rain ? 2.2 : 7,
            Emitting = false,
            UseFixedSeed = true,
            Seed = rain ? 704u : 705u,
            LocalCoords = false,
            FixedFps = 30,
            VisibilityAabb = new(new(-13, -12, -13), new(26, 24, 26)),
            DrawPass1 = new QuadMesh { Size = rain ? new(.018f, .3f) : new(.06f, .1f), Material = material },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            ProcessMaterial = new ParticleProcessMaterial
            {
                EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
                EmissionBoxExtents = new(10, 3, 10),
                Direction = rain ? Vector3.Down : new Vector3(1, -.3f, .3f),
                Spread = rain ? 3 : 35,
                Gravity = rain ? new(0, -8, 0) : new(0, -.08f, 0),
                InitialVelocityMin = rain ? 6 : .2f,
                InitialVelocityMax = rain ? 8 : .5f,
                ScaleMin = .7f,
                ScaleMax = 1.1f
            }
        };
        AddChild(particles); return particles;
    }

    internal void Apply(GraphicsPreferences preferences)
    {
        _preferences = preferences.Clone(); _effective = preferences.Effective(ForwardPlus);
        _target = AtmosphereProfile.For(_effective["atmosphere"] == 0 ? 0 : _effective["weather"]);
        var viewport = GetViewport();
        viewport.Scaling3DScale = new[] { .7f, .85f, 1f }[_effective["scale"]];
        viewport.Msaa3D = _effective["aa"] switch { 3 => Viewport.Msaa.Msaa2X, 4 => Viewport.Msaa.Msaa4X, _ => Viewport.Msaa.Disabled };
        viewport.ScreenSpaceAA = _effective["aa"] == 1 ? Viewport.ScreenSpaceAAEnum.Fxaa : Viewport.ScreenSpaceAAEnum.Disabled;
        viewport.UseTaa = _effective["aa"] == 2;
        Engine.MaxFps = new[] { 30, 60, 120, 0 }[_effective["fps"]];
        // Native command-line admission owns the CI frame bound; never lift it.
        string[] launch = ClientSettings.EngineArguments();
        int fpsFlag = Array.IndexOf(launch, "--max-fps");
        if (fpsFlag >= 0 && fpsFlag + 1 < launch.Length && int.TryParse(launch[fpsFlag + 1], out int fpsOverride) && fpsOverride >= 0) Engine.MaxFps = fpsOverride;
        DisplayServer.WindowSetVsyncMode(_effective["vsync"] == 1 && !launch.Contains("--disable-vsync") ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
        int shadows = _effective["shadows"];
        _sun.ShadowEnabled = shadows != 0;
        _sun.DirectionalShadowMaxDistance = shadows == 1 ? 45 : 80;
        RenderingServer.DirectionalShadowAtlasSetSize(shadows < 2 ? 1024 : shadows == 2 ? 2048 : 4096, true);
        if (ForwardPlus) RenderingServer.DirectionalSoftShadowFilterSetQuality(shadows < 2 ? RenderingServer.ShadowQuality.SoftLow : RenderingServer.ShadowQuality.SoftMedium);
        Environment.SsaoEnabled = _effective["ao"] != 0;
        Environment.SsaoRadius = _effective["ao"] == 2 ? 1.2f : .8f; Environment.SsaoIntensity = .65f;
        Environment.SsilEnabled = _effective["indirect"] == 1;
        Environment.SsilIntensity = .35f;
        Environment.GlowEnabled = _effective["glow"] != 0;
        Environment.GlowIntensity = _effective["glow"] == 2 ? .25f : .12f;
        Environment.GlowBloom = 0; Environment.GlowHdrThreshold = 1.2f;
        Environment.TonemapExposure = _effective.Exposure / 100f;
        Environment.FogEnabled = _effective["fog"] != 0;
        Environment.VolumetricFogEnabled = _effective["volumetric"] == 1 && _effective["fog"] != 0;
        _rain.Amount = new[] { 120, 240, 360 }[_effective["rain"]];
        UpdateWeather(0);
    }

    public override void _Process(double delta)
    {
        if (_proofTime is null) _time += Math.Min(delta, .1);
        _current = _current.Blend(_target, 1 - Mathf.Exp(-(float)Math.Min(delta, .1) * 1.5f));
        UpdateWeather(_proofTime ?? (float)_time);
        Camera3D? camera = GetViewport().GetCamera3D();
        if (camera is not null)
        {
            Vector2 center = GetViewport().GetVisibleRect().Size / 2;
            Vector3 focus = Plane.PlaneXZ.IntersectsRay(camera.ProjectRayOrigin(center), camera.ProjectRayNormal(center)) ?? VillageOrigin;
            _rain.GlobalPosition = focus + Vector3.Up * 8; _leaves.GlobalPosition = focus + Vector3.Up * 3;
        }
        if (_riverMist is not null) _riverMist.Position = VillageOrigin + VillageLayout.Hex(0, VillageLayout.RiverRow) + new Vector3(0, -.5f, 0);
        if (_seekParticles)
        {
            _seekParticles = false;
            foreach (GpuParticles3D particles in new[] { _rain, _leaves })
            {
                particles.SpeedScale = 0; particles.Restart(true); particles.RequestParticlesProcess(3);
            }
        }
    }
    private void UpdateWeather(float time)
    {
        _sun.LightColor = _current.Sun; _sun.LightEnergy = _current.SunEnergy;
        _sun.RotationDegrees = new(_current.SunPitch, -25, 0);
        Environment.AmbientLightColor = _current.Fill; Environment.AmbientLightEnergy = _current.FillEnergy;
        // Identical native sky material writes can invalidate radiance work.
        // Keep every changed transition value; suppress only exact stable repeats.
        if (_uploadedSkyHaze != _current.Haze)
        {
            _sky.SkyHorizonColor = _current.Haze; _sky.GroundHorizonColor = _current.Haze;
            _uploadedSkyHaze = _current.Haze; _skyUploads++;
        }
        Environment.FogLightColor = _current.Haze; Environment.FogLightEnergy = .75f; Environment.FogSunScatter = .15f;
        float fog = new[] { 0, .55f, .8f, 1f }[_effective["fog"]];
        Environment.FogDensity = _current.Fog * fog;
        Environment.FogHeightDensity = .006f * fog;
        Environment.VolumetricFogAlbedo = _current.Haze;
        Environment.VolumetricFogDensity = (_effective["shadows"] < 3 ? .00065f : .0012f) * fog * (1 + _current.Mist * 2) * (1 + .03f * Mathf.Sin(time * .12f));
        if (_riverMist is not null) _riverMist.Visible = Environment.VolumetricFogEnabled && _current.Mist > .1f;
        float wind = new[] { 0, .35f, .65f, 1f }[_effective["wind"]] * _current.Wind;
        RenderingServer.GlobalShaderParameterSet("odot_weather_time", time);
        _uploadedWind = _effective["foliage"] == 1 ? wind : 0;
        RenderingServer.GlobalShaderParameterSet("odot_foliage_wind", _uploadedWind);
        RenderingServer.GlobalShaderParameterSet("odot_water_motion", _effective["atmosphere"] == 1 && _effective["reduce_motion"] == 0 ? 1f : 0f);
        float comfort = _effective["reduce_weather"] == 1 ? .25f : 1;
        float rain = _effective["particles"] == 1 ? _current.Rain * new[] { .35f, .65f, 1f }[_effective["rain"]] * comfort : 0;
        _rain.AmountRatio = Math.Max(.001f, rain); _rain.Emitting = rain > .01f;
        float leaves = _effective["particles"] == 1 && _effective["reduce_motion"] == 0 ? wind * comfort : 0;
        _leaves.AmountRatio = Math.Max(.001f, leaves); _leaves.Emitting = leaves > .01f;
    }
    internal void FixedProof(GraphicsPreferences settings, float time)
    {
        Apply(settings); _current = _target; _proofTime = time; _seekParticles = true; UpdateWeather(time);
    }
    internal object Observe() => new
    {
        DesiredRenderer = _preferences.DesiredMethod,
        Method = RenderingServer.GetCurrentRenderingMethod(),
        Driver = RenderingServer.GetCurrentRenderingDriverName(),
        Gpu = RenderingServer.GetVideoAdapterName(),
        Vendor = RenderingServer.GetVideoAdapterVendor(),
        DeviceType = RenderingServer.GetVideoAdapterType().ToString(),
        Api = RenderingServer.GetVideoAdapterApiVersion(),
        RenderingDevice = RenderingServer.GetRenderingDevice() is not null,
        Display = DisplayServer.GetName(),
        Settings = _preferences.Values,
        Effective = _effective.Values,
        RenderScale = GetViewport().Scaling3DScale,
        Msaa = GetViewport().Msaa3D.ToString(),
        Taa = GetViewport().UseTaa,
        Fxaa = GetViewport().ScreenSpaceAA.ToString(),
        FpsLimit = Engine.MaxFps,
        Vsync = DisplayServer.WindowGetVsyncMode().ToString(),
        Shadows = _sun.ShadowEnabled,
        Environment.SsaoEnabled,
        Environment.SsilEnabled,
        Environment.FogEnabled,
        Environment.FogDensity,
        Environment.VolumetricFogEnabled,
        Environment.VolumetricFogDensity,
        Environment.GlowEnabled,
        Environment.TonemapExposure,
        SunEnergy = _sun.LightEnergy,
        Wind = _uploadedWind, // Last uploaded uniform; no runtime global-parameter/GPU readback.
        Rain = _rain.Emitting,
        RainRatio = _rain.Emitting ? _rain.AmountRatio : 0,
        Leaves = _leaves.Emitting,
        LocalFog = _riverMist?.Visible ?? false,
        CosmeticTime = _proofTime ?? (float)_time,
        SkyUploads = _skyUploads
    };
}

internal sealed record AtmosphereProfile(Color Sun, Color Fill, Color Haze, float SunEnergy, float FillEnergy, float SunPitch, float Fog, float Mist, float Wind, float Rain)
{
    internal static AtmosphereProfile For(int preset) => preset switch
    {
        1 => new(new("e9e8dc"), new("c0cfdf"), new("b9c8ce"), .3f, .34f, -60, .26f, .15f, .8f, 0),
        2 => new(new("fff0d4"), new("c4d8df"), new("bfced0"), .42f, .32f, -50, .4f, 1, .5f, 0),
        3 => new(new("dfdfd6"), new("bfcedd"), new("aebfc8"), .28f, .34f, -60, .3f, .45f, 1, 1),
        4 => new(new("ffd1aa"), new("b8c8e3"), new("b4bcc9"), .36f, .3f, -28, .24f, .25f, .55f, 0),
        _ => new(new("fff0d7"), new("c9d9e8"), new("c1d2d2"), .52f, .3f, -55, .2f, 0, .75f, 0)
    };
    internal AtmosphereProfile Blend(AtmosphereProfile other, float weight) => new(Sun.Lerp(other.Sun, weight), Fill.Lerp(other.Fill, weight), Haze.Lerp(other.Haze, weight),
        Mathf.Lerp(SunEnergy, other.SunEnergy, weight), Mathf.Lerp(FillEnergy, other.FillEnergy, weight), Mathf.Lerp(SunPitch, other.SunPitch, weight),
        Mathf.Lerp(Fog, other.Fog, weight), Mathf.Lerp(Mist, other.Mist, weight), Mathf.Lerp(Wind, other.Wind, weight), Mathf.Lerp(Rain, other.Rain, weight));
}
