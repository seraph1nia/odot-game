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
        // The landed Worldtree has its own immutable inventory; do not relabel
        // those additional members as part of the original 46-member kit pin.
        using JsonDocument authored = JsonDocument.Parse(File.ReadAllText(Path.Combine(game, "Assets", "Authored", "manifest.json")));
        JsonElement worldtree = authored.RootElement.GetProperty("worldtree");
        Assert.Equal("https://github.com/seraph1nia/odot-game-assets.git", worldtree.GetProperty("source").GetString());
        Assert.Equal("cadf0373c33dcde6858f59f2873575da8665be2d", worldtree.GetProperty("revision").GetString());
        foreach (JsonElement entry in worldtree.GetProperty("files").EnumerateArray())
        {
            string path = entry.GetProperty("path").GetString()!;
            Assert.StartsWith("UI/", path, StringComparison.Ordinal);
            Assert.DoesNotContain("..", path, StringComparison.Ordinal);
            byte[] bytes = File.ReadAllBytes(Path.Combine(game, path));
            Assert.Equal(entry.GetProperty("bytes").GetInt32(), bytes.Length);
            Assert.Equal(entry.GetProperty("sha256").GetString(), Convert.ToHexStringLower(SHA256.HashData(bytes)));
            paths.Add(path[3..]); // Shared theme/font members retain both exact bindings.
        }
        foreach (string source in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Where(p => Path.GetExtension(p) is ".gd" or ".tscn" or ".tres" or ".svg" or ".ttf" or ".png"))
            Assert.Contains(Path.GetRelativePath(root, source).Replace(Path.DirectorySeparatorChar, '/'), paths);
        Assert.False(File.Exists(Path.Combine(root, "art", "hud.png")));
        foreach (JsonProperty removed in manifest.RootElement.GetProperty("supersededFiles").EnumerateObject())
            Assert.False(File.Exists(Path.Combine(game, removed.Name)), "Superseded pack member remains: " + removed.Name);
    }
}
