using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace DevRunner.Tests;

public sealed class WoodlandUiAssetTests
{
    [Fact]
    public void ProductionPayloadMatchesExactAcceptedMemberAndProvenanceBindings()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Odot.slnx"))) directory = directory.Parent;
        string game = Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), "src", "Game");
        string root = Path.Combine(game, "UI");
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "manifest.json")));
        Assert.Equal("https://github.com/seraph1nia/odot-game-assets.git", manifest.RootElement.GetProperty("source").GetString());
        Assert.Equal("12bc5ef02765c1d4b725013791f7420918aacd64", manifest.RootElement.GetProperty("revision").GetString());
        Assert.Equal("ui/preview/UI/", manifest.RootElement.GetProperty("sourceRoot").GetString());
        HashSet<string> paths = [];
        foreach (JsonProperty entry in manifest.RootElement.GetProperty("files").EnumerateObject())
        {
            string path = entry.Name;
            Assert.True(paths.Add(path), "Duplicate payload member: " + path);
            Assert.False(Path.IsPathRooted(path)); Assert.DoesNotContain("..", path, StringComparison.Ordinal);
            Assert.Equal(entry.Value.GetString(), Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, path)))));
            Assert.DoesNotContain("fixture", path, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("prototype", path, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Equal(46, paths.Count);
        Assert.Contains("fonts/LICENSE-DejaVu.txt", paths);
        Assert.Contains("components/modal.tscn", paths);
        Assert.Contains("components/skill_tree.tscn", paths);
        foreach (string source in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Where(p => Path.GetExtension(p) is ".gd" or ".tscn" or ".tres" or ".svg" or ".ttf" or ".png"))
            Assert.Contains(Path.GetRelativePath(root, source).Replace(Path.DirectorySeparatorChar, '/'), paths);
        Assert.False(File.Exists(Path.Combine(root, "art", "hud.png")));
        foreach (JsonProperty removed in manifest.RootElement.GetProperty("supersededFiles").EnumerateObject())
            Assert.False(File.Exists(Path.Combine(game, removed.Name)), "Superseded pack member remains: " + removed.Name);
    }
}
