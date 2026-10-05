using System.Security.Cryptography;

namespace Game;

// Verification only. One ordered engine owner acquires copied raw pixels,
// then persists exactly those frames; no worker thread or scene references.
internal sealed class OwnedFrameCapture
{
    internal sealed record Receipt(string Id, string Path, int Width, int Height, string PixelHash, string Metadata);
    private sealed record Entry(Receipt Receipt, byte[] Pixels);
    private readonly Queue<Entry> _pending = [];
    private readonly HashSet<string> _ids = [];
    private bool _persisting;
    internal int Pending => _pending.Count;
    internal Receipt Acquire(string id, string path, int width, int height, byte[] rgba, string metadata)
    {
        if (_persisting || _pending.Count >= 4 || _ids.Contains(id) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(metadata)
            || width <= 0 || height <= 0 || width > 1280 || height > 820 || rgba.Length != checked(width * height * 4)
            || _pending.Any(e => e.Receipt.Path == path)) throw new InvalidOperationException("Duplicate, invalid or excessive raw frame acquisition.");
        byte[] pixels = (byte[])rgba.Clone();
        var receipt = new Receipt(id, path, width, height, Convert.ToHexString(SHA256.HashData(pixels)), metadata);
        _pending.Enqueue(new(receipt, pixels)); _ids.Add(id); return receipt;
    }
    internal Receipt Persist(string id, Func<Receipt, byte[], byte[]> encodeWriteAndDecode)
    {
        if (_persisting || !_pending.TryPeek(out Entry? entry) || entry.Receipt.Id != id) throw new InvalidOperationException("Missing, duplicate or out-of-order raw frame persistence.");
        _persisting = true;
        try
        {
            byte[] decoded = encodeWriteAndDecode(entry.Receipt, (byte[])entry.Pixels.Clone());
            if (!decoded.AsSpan().SequenceEqual(entry.Pixels)) throw new InvalidOperationException("Missing or corrupt PNG: decoded pixels differ from acquired frame.");
            _pending.Dequeue(); return entry.Receipt;
        }
        finally { _persisting = false; }
    }
    internal void Clear()
    {
        if (_persisting) throw new InvalidOperationException("Frame ownership cannot be released during persistence.");
        _pending.Clear(); _ids.Clear();
    }
    internal static void RequireLiveCompletion(TimeSpan elapsed, int observations, int captures)
    {
        if (elapsed > TimeSpan.FromSeconds(12) || observations != 12 || captures != 4)
            throw new InvalidOperationException("Live progression requires twelve observations/four actual raw frames within 12 seconds.");
    }
}
