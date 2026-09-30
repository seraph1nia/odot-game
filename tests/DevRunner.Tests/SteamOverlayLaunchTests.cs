using System.Buffers.Binary;
using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class SteamOverlayLaunchTests : IDisposable
{
    private readonly string _home = Path.Combine(Path.GetTempPath(), "odot-overlay-" + Guid.NewGuid().ToString("N"));

    [Theory]
    [InlineData("play", "menu", false, false, false, true)]
    [InlineData("test-steam", "menu", false, false, false, true)]
    [InlineData("play", "menu", true, false, false, false)]
    [InlineData("play", "menu", false, true, false, false)]
    [InlineData("play", "menu", false, false, true, false)]
    [InlineData("dev", "playing-host", false, false, false, false)]
    [InlineData("test-ui", "menu", false, true, false, false)]
    [InlineData("check-steam-extension", "menu", false, false, false, false)]
    [InlineData("play", "solo", false, false, false, false)]
    public void OnlyRealSteamMenuLaunchesAreEligible(string command, string role, bool headless, bool graphical, bool disabled, bool expected)
        => Assert.Equal(expected, SteamOverlayLaunch.IsEligible(command, role, headless, graphical, disabled));

    [Fact]
    public void MissingLibraryLeavesExistingPreloadsToTheCaller()
        => Assert.Null(SteamOverlayLaunch.Preload(_home, "existing.so"));

    [Fact]
    public void NativeLibraryIsAppendedWithoutChangingExistingEntries()
    {
        string library = Overlay(".local/share/Steam");
        Assert.Equal(library, SteamOverlayLaunch.Preload(_home, null));
        Assert.Equal("first.so second.so:third.so:" + library, SteamOverlayLaunch.Preload(_home, "first.so second.so:third.so"));
    }

    [Fact]
    public void ExistingSteamInjectedRendererIsPreservedExactly()
    {
        string library = Overlay(".steam/root");
        string existing = "other.so:" + library + " final.so";
        Assert.Equal(existing, SteamOverlayLaunch.Preload(_home, existing));
    }

    [Theory]
    [InlineData(1, 62, 64)]
    [InlineData(2, 183, 64)]
    [InlineData(2, 62, 20)]
    public void WrongArchitectureAndTruncatedLibrariesAreSkipped(byte elfClass, ushort machine, int length)
    {
        Overlay(".steam/root", elfClass, machine, length);
        Assert.Null(SteamOverlayLaunch.Preload(_home, null));
        string fallback = Overlay(".local/share/Steam");
        Assert.Equal(fallback, SteamOverlayLaunch.Preload(_home, null));
    }

    [Fact]
    public void NonElfAndDirectoriesAreSkipped()
    {
        string library = Overlay(".steam/root");
        File.WriteAllText(library, new string('x', 64));
        Assert.Null(SteamOverlayLaunch.Preload(_home, null));
        File.Delete(library); Directory.CreateDirectory(library);
        Assert.Null(SteamOverlayLaunch.Preload(_home, null));
    }

    private string Overlay(string root, byte elfClass = 2, ushort machine = 62, int length = 64)
    {
        string library = Path.Combine(_home, root, "ubuntu12_64", "gameoverlayrenderer.so");
        Directory.CreateDirectory(Path.GetDirectoryName(library)!);
        byte[] header = new byte[64];
        header[0] = 0x7f; header[1] = (byte)'E'; header[2] = (byte)'L'; header[3] = (byte)'F';
        header[4] = elfClass; header[5] = 1; header[6] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(16), 3);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(18), machine);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(20), 1);
        File.WriteAllBytes(library, header[..length]);
        return library;
    }

    public void Dispose() { if (Directory.Exists(_home)) Directory.Delete(_home, true); }
}
