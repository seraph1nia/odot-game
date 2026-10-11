namespace Game;

// Local presentation policy. Engine-free so validation, presets and capability
// fallback are cheap to test; never serialized into a match or a network packet.
internal sealed record GraphicsOption(string Key, string Label, string Group, string[] Choices, int Default);

internal sealed class GraphicsPreferences
{
    internal static readonly GraphicsOption[] Options =
    [
        new("quality", "Graphics quality", "Display and performance", ["Low", "Medium", "High", "Ultra", "Custom"], 1),
        new("renderer", "Renderer (next launch)", "Display and performance", ["Forward+ / Vulkan", "Compatibility / OpenGL"], 0),
        new("vsync", "VSync", "Display and performance", ["Off", "On"], 1),
        new("fps", "FPS limit", "Display and performance", ["30", "60", "120", "Unlimited"], 1),
        new("scale", "Render scale", "Display and performance", ["70%", "85%", "100%"], 2),
        new("aa", "Anti-aliasing", "Display and performance", ["Off", "FXAA", "TAA", "MSAA 2×", "MSAA 4×"], 3),
        new("shadows", "Shadow quality", "Display and performance", ["Off", "Low", "Medium", "High"], 2),
        new("ao", "Ambient occlusion", "Display and performance", ["Off", "Low", "High"], 0),
        new("indirect", "Indirect lighting", "Display and performance", ["Off", "On"], 0),
        new("atmosphere", "Atmospheric effects", "Atmosphere and weather", ["Off", "On"], 1),
        new("weather", "Weather preset", "Atmosphere and weather", ["Clear morning", "Soft overcast", "Misty forest", "Gentle rain", "Evening / dusk"], 0),
        new("fog", "Fog intensity", "Atmosphere and weather", ["Off", "Low", "Medium", "High"], 1),
        new("volumetric", "Volumetric fog", "Atmosphere and weather", ["Off", "On"], 1),
        new("particles", "Weather particles", "Atmosphere and weather", ["Off", "On"], 1),
        new("rain", "Rain intensity", "Atmosphere and weather", ["Low", "Medium", "High"], 0),
        new("wind", "Wind strength", "Atmosphere and weather", ["Off", "Gentle", "Moderate", "Strong"], 1),
        new("foliage", "Foliage animation", "Atmosphere and weather", ["Off", "On"], 1),
        new("glow", "Bloom / glow", "Atmosphere and weather", ["Off", "Subtle", "Normal"], 1),
        new("reduce_motion", "Reduce environmental motion", "Accessibility and comfort", ["Off", "On"], 0),
        new("reduce_weather", "Reduce weather effects", "Accessibility and comfort", ["Off", "On"], 0),
        new("reduce_effects", "Reduced visual effects", "Accessibility and comfort", ["Off", "On"], 0)
    ];
    private readonly Dictionary<string, int> _values = Options.ToDictionary(o => o.Key, o => o.Default);
    internal IReadOnlyDictionary<string, int> Values => _values;
    internal int Exposure { get; private set; } = 100;
    internal int this[string key] => _values[key];
    internal string DesiredMethod => this["renderer"] == 0 ? "forward_plus" : "gl_compatibility";
    internal GraphicsPreferences Clone()
    {
        var copy = new GraphicsPreferences { Exposure = Exposure };
        foreach (var entry in _values) copy._values[entry.Key] = entry.Value;
        return copy;
    }
    internal static GraphicsPreferences Load(Func<string, object?> read)
    {
        var result = new GraphicsPreferences();
        foreach (GraphicsOption option in Options)
            if (read(option.Key) is long number && number >= 0 && number < option.Choices.Length) result._values[option.Key] = (int)number;
        if (read("exposure") is long exposure && exposure is >= 80 and <= 120) result.Exposure = (int)exposure;
        return result;
    }
    internal void Edit(string key, int value)
    {
        GraphicsOption option = Options.Single(o => o.Key == key);
        if (value < 0 || value >= option.Choices.Length) throw new ArgumentOutOfRangeException(nameof(value));
        if (key == "quality" && value != 4) { Preset(value); return; }
        _values[key] = value;
        // Backend, weather and comfort are independent player choices, not tiers.
        if (key is "vsync" or "fps" or "scale" or "aa" or "shadows" or "ao" or "indirect" or "fog" or "volumetric" or "particles" or "rain" or "wind" or "foliage" or "glow") _values["quality"] = 4;
    }
    internal void SetExposure(int value)
    {
        Exposure = Math.Clamp(value, 80, 120); _values["quality"] = 4;
    }
    private void Preset(int tier)
    {
        _values["quality"] = tier;
        _values["scale"] = tier == 0 ? 1 : 2;
        _values["aa"] = new[] { 1, 3, 4, 2 }[tier];
        _values["shadows"] = new[] { 1, 2, 3, 3 }[tier];
        _values["ao"] = new[] { 0, 0, 1, 2 }[tier];
        _values["indirect"] = tier == 3 ? 1 : 0;
        _values["fog"] = tier < 2 ? 1 : 2;
        _values["volumetric"] = tier == 0 ? 0 : 1;
        _values["particles"] = tier == 0 ? 0 : 1;
        _values["rain"] = tier < 2 ? 0 : tier - 1;
        _values["wind"] = tier == 0 ? 0 : 1;
        _values["foliage"] = tier == 0 ? 0 : 1;
        _values["glow"] = tier == 0 ? 0 : 1;
        Exposure = 100;
    }
    internal static bool Supported(string key, int value, bool forwardPlus) => forwardPlus || !(key is "volumetric" or "indirect" && value != 0 || key == "aa" && value is 1 or 2);
    internal GraphicsPreferences Effective(bool forwardPlus)
    {
        var result = Clone();
        if (!forwardPlus)
        {
            result._values["volumetric"] = 0; result._values["indirect"] = 0;
            // The new 2x Medium/default path is disproportionately costly in
            // Compatibility (owned llvmpipe matched control: ~1.3s -> ~0.2s).
            // Screen-space AA is not supported by the actual Compatibility backend
            // (locked engine's viewport_set_screen_space_aa warning). Use Off,
            // retaining desired values and explicit High 4x / Ultra MSAA fallback.
            if (result["aa"] is 1 or 3) result._values["aa"] = 0;
            else if (result["aa"] == 2) result._values["aa"] = 3;
        }
        if (this["atmosphere"] == 0)
            foreach (string key in new[] { "fog", "volumetric", "particles", "wind", "foliage", "glow" }) result._values[key] = 0;
        if (this["reduce_motion"] == 1) { result._values["wind"] = 0; result._values["foliage"] = 0; }
        if (this["reduce_effects"] == 1) { result._values["glow"] = 0; result._values["indirect"] = 0; }
        return result;
    }
}
