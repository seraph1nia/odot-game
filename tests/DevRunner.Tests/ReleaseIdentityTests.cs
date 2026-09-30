using System.Xml.Linq;
using DevRunner;
using Game.Distribution;
using Xunit;

namespace DevRunner.Tests;

public sealed class ReleaseIdentityTests
{
    private const string Commit = "0123456789abcdef0123456789abcdef01234567";

    [Fact]
    public void PreviewDefaultsToTestIdentityAndStableRequiresOwnAppId()
    {
        ReleaseIdentity preview = ReleaseIdentity.Create("v0.1.0-beta.1+build.1", Commit, "linux-x64", false, null);
        Assert.Equal("preview", preview.Channel); Assert.Equal(480u, preview.SteamAppId);
        Assert.Equal("10.0.401", preview.SdkVersion); Assert.Equal("10.0.12", preview.RuntimeVersion);
        Assert.Equal(preview, ReleaseIdentity.FromJson(preview.ToJson()));
        Assert.Throws<ArgumentException>(() => ReleaseIdentity.Create("v0.1.0", Commit, "linux-x64", false, null));
        Assert.Throws<ArgumentException>(() => ReleaseIdentity.Create("v0.1.0", Commit, "windows-x64", true, 480));
        Assert.Throws<ArgumentException>(() => ReleaseIdentity.Create("v0.1.0", Commit, "windows-x64", true, null));
        Assert.Equal("production", ReleaseIdentity.Create("v0.1.0", Commit, "windows-x64", true, 123456).SteamMode);
        Assert.Throws<ArgumentException>(() => ReleaseIdentity.Create("v0.1.0-beta.1", "short-sha", "linux-x64", false, null));
        Assert.Throws<ArgumentException>(() => ReleaseIdentity.Create("v0.1.0-beta.1", Commit, "arm64", false, null));
        Assert.Throws<ArgumentException>(() => ReleaseIdentity.FromJson((preview with { Channel = "stable" }).ToJson()));
        Assert.Throws<ArgumentException>(() => ReleaseIdentity.FromJson((preview with { Repository = "someone/else" }).ToJson()));
    }

    [Fact]
    public void TagValidationRunsDuringArgumentParsingBeforeAnyMutation()
    {
        Assert.Throws<ArgumentException>(() => Options.Parse(["export-client", "--tag", "v1.0.0"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["export-client", "--tag", "v1.0.0", "--steam-app-id", "480"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["export-client", "--tag", "v1.0.00-beta"]));
        Assert.Throws<ArgumentException>(() => Options.Parse(["ci", "--tag", "v0.1.0-beta.1"]));
        Options stable = Options.Parse(["export-client", "--tag", "v1.0.0", "--steam-app-id", "123456"]);
        Assert.True(stable.Production);
        Assert.False(Options.Parse(["export-client", "--tag", "v0.1.0-beta.1"]).Production);
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void StampingRetainsPlayerDataIdentityAndLocksAndIncludesMetadataInPackedResources(string newline)
    {
        string root = Path.Combine(Path.GetTempPath(), "odot-stamp-fixture-" + Guid.NewGuid().ToString("N"));
        string game = Path.Combine(root, "src", "Game");
        Directory.CreateDirectory(game);
        try
        {
            File.WriteAllText(Path.Combine(game, "project.godot"), "[application]\nconfig/name=\"Odot - Nine Tiles\"\nconfig/version=\"development\"\nconfig/use_custom_user_dir=true\nconfig/custom_user_dir_name=\"existing-data\"\n[dotnet]\nproject/assembly_name=\"Game\"\n");
            string projectPath = Path.Combine(game, "project.godot");
            File.WriteAllText(projectPath, File.ReadAllText(projectPath).Replace("\n", newline, StringComparison.Ordinal));
            File.WriteAllText(Path.Combine(game, "Game.csproj"), "<Project Sdk=\"Godot.NET.Sdk/4.7.2\"><PropertyGroup><RuntimeIdentifiers>linux-x64</RuntimeIdentifiers></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(game, "export_presets.cfg"), "[preset.0]\ninclude_filter=\"\"\nexclude_filter=\"packages*.lock.json\"\n");
            File.WriteAllText(Path.Combine(game, "packages.Debug.lock.json"), "locked fixture");
            ReleaseIdentity identity = ReleaseIdentity.Create("v0.1.0-beta.1+build.1", Commit, "linux-x64", false, null);
            ReleaseStaging.Stamp(root, identity);
            string project = File.ReadAllText(Path.Combine(game, "project.godot"));
            Assert.Contains("config/version=\"0.1.0-beta.1+build.1\"", project);
            Assert.Contains("config/name=\"Odot - Nine Tiles\"", project);
            Assert.Contains("config/custom_user_dir_name=\"existing-data\"", project);
            Assert.Contains("project/assembly_name=\"Game\"", project);
            Assert.DoesNotContain("config/version=\"development\"", project);
            XDocument csproj = XDocument.Load(Path.Combine(game, "Game.csproj"));
            Assert.Equal(identity.Version, csproj.Descendants("InformationalVersion").Single().Value);
            Assert.Equal("0.1.0.0", csproj.Descendants("AssemblyVersion").Single().Value);
            Assert.Equal(Commit, csproj.Descendants("AssemblyMetadata").Single().Attribute("Value")!.Value);
            Assert.Equal(identity, ReleaseIdentity.FromJson(File.ReadAllText(Path.Combine(game, "Distribution", "build-info.json"))));
            Assert.Contains("include_filter=\"Distribution/build-info.json\"", File.ReadAllText(Path.Combine(game, "export_presets.cfg")));
            Assert.Equal("locked fixture", File.ReadAllText(Path.Combine(game, "packages.Debug.lock.json")));
        }
        finally { Directory.Delete(root, true); }
    }
}
