using System.Buffers.Binary;

namespace DevRunner;

// Valve's Linux overlay must be loaded before the engine creates its renderer.
// https://partner.steamgames.com/doc/store/application/platforms/linux
internal static class SteamOverlayLaunch
{
    internal static bool IsEligible(string command, string role, bool headless, bool privateDisplay, bool steamDisabled)
        => command is "play" or "test-steam" && role == "menu" && !headless && !privateDisplay && !steamDisabled;

    internal static string? Preload(string home, string? existing)
    {
        // Steam itself may already have injected its renderer. Keep that launch intact.
        foreach (string token in (existing ?? "").Split([':', ' ', '\t', '\n'], StringSplitOptions.RemoveEmptyEntries))
            if (Path.GetFileName(token) == "gameoverlayrenderer.so" && IsNativeOverlay(token)) return existing;

        foreach (string root in new[] { Path.Combine(home, ".steam", "root"), Path.Combine(home, ".steam", "steam"),
            Path.Combine(home, ".steam", "debian-installation"), Path.Combine(home, ".local", "share", "Steam") })
        {
            string library = Path.Combine(root, "ubuntu12_64", "gameoverlayrenderer.so");
            // LD_PRELOAD has no escaping for its whitespace/colon separators.
            if (library.Any(value => char.IsWhiteSpace(value) || value == ':') || !IsNativeOverlay(library)) continue;
            return string.IsNullOrWhiteSpace(existing) ? library : existing + ":" + library;
        }
        return null;
    }

    private static bool IsNativeOverlay(string path)
    {
        try
        {
            using var file = File.OpenRead(path);
            Span<byte> header = stackalloc byte[64];
            return file.ReadAtLeast(header, header.Length, throwOnEndOfStream: false) == header.Length
                && header[0] == 0x7f && header[1] == 'E' && header[2] == 'L' && header[3] == 'F'
                && header[4] == 2 && header[5] == 1 && header[6] == 1
                && BinaryPrimitives.ReadUInt16LittleEndian(header[16..]) == 3
                && BinaryPrimitives.ReadUInt16LittleEndian(header[18..]) == 62
                && BinaryPrimitives.ReadUInt32LittleEndian(header[20..]) == 1;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        { return false; }
    }
}
