using Godot;

namespace Game;

internal static class FramePersistenceProof
{
    internal static object Run()
    {
        string root = System.Environment.GetEnvironmentVariable("ODOT_OWNED_DATA") ?? throw new InvalidOperationException("Missing owned image proof path.");
        string path = System.IO.Path.Combine(root, "raw-frame-codec-control.png");
        var owner = new OwnedFrameCapture();
        byte[] pixels = Enumerable.Range(0, 64 * 64 * 4).Select(n => (byte)(n * 17)).ToArray();
        var acquired = owner.Acquire("codec-control", path, 64, 64, pixels, "tick=301;revision=44;camera=near;pose=attack");
        var persisted = owner.Persist(acquired.Id, (frame, raw) =>
        {
            using Image image = Image.CreateFromData(frame.Width, frame.Height, false, Image.Format.Rgba8, raw);
            if (image.SavePng(frame.Path) != Error.Ok) throw new InvalidOperationException("Native owned codec control could not save PNG.");
            using Image decoded = Image.LoadFromFile(frame.Path);
            decoded.Convert(Image.Format.Rgba8); return decoded.GetData();
        });
        if (persisted != acquired || owner.Pending != 0) throw new InvalidOperationException("Native codec changed acquired identity.");
        System.IO.File.Delete(path);
        return new { ImmutableRgbaRoundTrip = true, acquired.PixelHash, acquired.Metadata, OwnerThread = "Godot engine thread; no native object crosses a worker thread" };
    }
}
