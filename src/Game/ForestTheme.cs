using Godot;

namespace Game;

// Authored here, not baked into Blender resources. RGB axes form a real SDR 3D
// LUT (red=X, green=Y, blue=slice), generated once per graphical owner/theme.
internal sealed class ForestTheme : IDisposable
{
    internal const int LutSize = 16;
    private readonly Dictionary<int, ImageTexture3D> _luts = [];
    internal static Color Grade(Color color, int theme)
    {
        float luma = color.R * .2126f + color.G * .7152f + color.B * .0722f;
        float strength = theme == 0 ? 0 : theme == 1 ? .035f : .055f;
        float shadows = 4 * luma * (1 - luma) * (1 - luma);
        float highlights = 4 * luma * (1 - luma) * luma;
        Vector3 cool = theme == 1 ? new(-.3f, .25f, .35f) : new(.05f, -.2f, .6f);
        Vector3 warm = new(.45f, .18f, -.3f);
        // Bounded split-toning, not hue remapping; black/white remain exact.
        Vector3 tint = strength * (cool * shadows + warm * highlights);
        float green = theme == 0 ? 0 : Math.Max(0, color.G - Math.Max(color.R, color.B)) * .04f;
        return new(Math.Clamp(color.R + tint.X, 0, 1), Math.Clamp(color.G + tint.Y + green, 0, 1), Math.Clamp(color.B + tint.Z, 0, 1), color.A);
    }
    // Separate the CPU images from upload: Dummy/headless has no GPU texture
    // readback. The existing component proof checks these actual upload inputs.
    internal static Godot.Collections.Array<Image> LutSlices(int theme)
    {
        var slices = new Godot.Collections.Array<Image>();
        for (int blue = 0; blue < LutSize; blue++)
        {
            Image image = Image.CreateEmpty(LutSize, LutSize, false, Image.Format.Rgb8);
            for (int green = 0; green < LutSize; green++)
                for (int red = 0; red < LutSize; red++)
                    image.SetPixel(red, green, Grade(new(red / (LutSize - 1f), green / (LutSize - 1f), blue / (LutSize - 1f)), theme));
            slices.Add(image);
        }
        return slices;
    }
    internal ImageTexture3D Lut(int theme)
    {
        if (_luts.TryGetValue(theme, out ImageTexture3D? texture)) return texture;
        var slices = LutSlices(theme);
        try
        {
            texture = new ImageTexture3D();
            Error error = texture.Create(Image.Format.Rgb8, LutSize, LutSize, LutSize, false, slices);
            if (error != Error.Ok) { texture.Dispose(); throw new InvalidOperationException("Forest LUT: " + error); }
            _luts.Add(theme, texture);
            return texture;
        }
        finally { foreach (Image image in slices) image.Dispose(); }
    }
    internal static AtmosphereProfile Palette(AtmosphereProfile weather, int theme)
    {
        if (theme == 0) return weather;
        return weather with
        {
            Sun = weather.Sun.Lerp(new Color("ffe6ba"), theme == 1 ? .08f : .13f),
            Fill = weather.Fill.Lerp(new Color(theme == 1 ? "87c3c4" : "aaa4d3"), theme == 1 ? .18f : .25f),
            Haze = weather.Haze.Lerp(new Color(theme == 1 ? "9ccdcc" : "a6c9d2"), theme == 1 ? .15f : .24f)
        };
    }
    public void Dispose()
    {
        foreach (ImageTexture3D texture in _luts.Values) texture.Dispose();
        _luts.Clear();
    }
}
