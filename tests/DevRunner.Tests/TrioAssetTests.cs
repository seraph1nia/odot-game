using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace DevRunner.Tests;

public sealed class TrioAssetTests
{
    [Fact]
    public void BundledTrioFilesMatchRecordedProvenance()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Odot.slnx"))) directory = directory.Parent;
        string root = Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), "src", "Game", "Assets", "TrioUI");
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "manifest.json")));
        Assert.Equal("https://moonpunchstudio.itch.io/trio-ui-kit-free", manifest.RootElement.GetProperty("source").GetString());
        Assert.Equal("2.2", manifest.RootElement.GetProperty("version").GetString());
        HashSet<string> paths = [];
        foreach (JsonElement entry in manifest.RootElement.GetProperty("files").EnumerateArray())
        {
            string path = entry.GetProperty("path").GetString()!;
            Assert.True(paths.Add(path), "Duplicate asset provenance: " + path);
            Assert.False(Path.IsPathRooted(path)); Assert.DoesNotContain("..", path, StringComparison.Ordinal);
            Assert.Equal(entry.GetProperty("sha256").GetString(), Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, path)))));
            if (entry.TryGetProperty("derivedFrom", out JsonElement parent)) Assert.True(File.Exists(Path.Combine(root, parent.GetString()!)));
            else Assert.StartsWith("TrioUIKit_FreeSample_v2.2/", entry.GetProperty("archivePath").GetString(), StringComparison.Ordinal);
        }
        foreach (string svg in Directory.EnumerateFiles(root, "*.svg", SearchOption.AllDirectories))
            Assert.Contains(Path.GetRelativePath(root, svg).Replace(Path.DirectorySeparatorChar, '/'), paths);
    }
}
