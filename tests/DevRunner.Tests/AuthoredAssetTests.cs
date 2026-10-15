using System.Buffers.Binary;
using System.Security.Cryptography;
using Game;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class AuthoredAssetTests
{
    private static readonly string[] Clips = ["attack", "death", "hit", "idle", "run", "walk"];
    // Immutable distribution inputs; parse/hash once for the independent mapping
    // and rig assertions instead of rereading every GLB three times.
    private static readonly Lazy<IReadOnlyDictionary<string, GlbSummary>> Models = new(() =>
        AuthoredAssets.ValidateDistribution(Path.GetDirectoryName(DirectoryPath)!));
    private static string DirectoryPath
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Odot.slnx"))) directory = directory.Parent;
            return Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), "src", "Game", "Assets", "Authored");
        }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SourceAndExistingPackageComponentProofUseOnlyOwnedHeadlessArgv(bool exported)
    {
        Options options = Options.Parse(exported ? ["test-assets", "--exported"] : ["test-assets"]);
        Assert.Equal(exported, options.Exported);
        Assert.False(options.DesktopSurface);
        var runner = new Runner(options, CancellationToken.None);
        List<string> args = runner.GameArguments("asset-probe", "menu", true, 0, exported ? "packed-client" : null, "--asset-fidelity-probe");
        Assert.Contains("--headless", args);
        Assert.Contains("--asset-fidelity-probe", args);
        Assert.Equal(!exported, args.Contains("--path"));
        Assert.DoesNotContain("--rendering-method", args);
        Assert.DoesNotContain("--client", args);
        Assert.DoesNotContain("--server", args);
        Assert.True(Runner.OwnsVerificationCommand(options.Command));
        Assert.Throws<ArgumentException>(() => Options.Parse(["test-assets", "--desktop"]));
    }

    [Fact]
    public void RequiredExportDistributionMatchesImmutableProvenanceAndEmbeddedDependencies()
    {
        IReadOnlyDictionary<string, GlbSummary> models = Models.Value;
        Assert.Equal(AssetCatalog.RequiredPaths, models.Keys.Order(StringComparer.Ordinal));
        Assert.All(models.Values, m => { Assert.True(m.Meshes > 0); Assert.True(m.Materials > 0); });
        AuthoredAssetManifest manifest = AuthoredAssets.ReadManifest(DirectoryPath);
        Assert.Equal(AssetCatalog.Revision, manifest.Revision);
        Assert.Equal("the repo is my own work, so yes, use it", manifest.Permission);
    }
    // Tier A byte/provenance admission catches a modified or missing imported
    // resource before an engine launch. Tier B separately loads these textures,
    // JSON and controls. Read-only members only; subsecond expected cost.
    [Fact]
    public void WorldtreeRuntimeMembersMatchPinnedOriginalSourceAndExcludeAuthoringPayload()
    {
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(DirectoryPath, "manifest.json")));
        var worldtree = document.RootElement.GetProperty("worldtree");
        Assert.Equal("https://github.com/seraph1nia/odot-game-assets.git", worldtree.GetProperty("source").GetString());
        Assert.Equal(40, worldtree.GetProperty("revision").GetString()!.Length);
        string game = Path.GetFullPath(Path.Combine(DirectoryPath, "..", ".."));
        var members = worldtree.GetProperty("files").EnumerateArray().ToArray();
        foreach (var member in members)
        {
            string path = member.GetProperty("path").GetString()!;
            Assert.StartsWith("UI/", path); Assert.DoesNotContain("..", path);
            Assert.DoesNotContain("prototype", path); Assert.DoesNotContain("reference", path); Assert.DoesNotContain("tests", path);
            Assert.Equal("ui/preview/" + path, member.GetProperty("sourcePath").GetString());
            byte[] bytes = File.ReadAllBytes(Path.Combine(game, path));
            Assert.Equal(member.GetProperty("bytes").GetInt64(), bytes.LongLength);
            Assert.Equal(member.GetProperty("sha256").GetString(), Convert.ToHexStringLower(SHA256.HashData(bytes)));
        }
        Assert.Equal(30, members.Count(m => m.GetProperty("path").GetString()!.StartsWith("UI/art/worldtree/icons/", StringComparison.Ordinal)));
        Assert.Contains(members, m => m.GetProperty("path").GetString() == "UI/art/worldtree/topology.json");
        Assert.Contains(members, m => m.GetProperty("path").GetString() == "UI/art/worldtree/modules.json");
    }
    [Fact]
    public void EveryCanonicalRoleUsesDedicatedAvailableGeometryAndTruthfulNames()
    {
        IReadOnlyDictionary<string, GlbSummary> models = Models.Value;
        foreach (Building type in Enum.GetValues<Building>().Where(b => b != Building.Empty))
        {
            BuildingVisual visual = AssetCatalog.Building(type);
            Assert.Contains(visual.Path, models.Keys);
            Assert.Contains(visual.Upgrade, models.Keys);
        }
        Assert.Equal("Bakery", AssetCatalog.BuildingName(Building.Farm));
        Assert.Equal("Woodcutter hut", AssetCatalog.BuildingName(Building.Lumbermill));
        Assert.Equal("Magic academy", AssetCatalog.BuildingName(Building.Arcanum));
        Assert.Equal("Bombard tower", AssetCatalog.BuildingName(Building.CatapultTower));
        Assert.NotEqual(AssetCatalog.Building(Building.Mine).Path, AssetCatalog.Building(Building.MetalMine).Path);
        Assert.NotEqual(AssetCatalog.Building(Building.Weaver).Path, AssetCatalog.Building(Building.Lumbermill).Path);
        Assert.Equal("Town hall", AssetCatalog.BuildingName(Building.TownHall));
        Assert.Throws<ArgumentOutOfRangeException>(() => AssetCatalog.Building((Building)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => AssetCatalog.Unit((UnitType)999, Faction.Adventurers));
    }
    // Tier A catches stale selected exports/clip contracts cheaply. Actual
    // imported looping, hierarchy motion and material binding belong to Tier B.
    [Fact]
    public void SelectedBuildingsAndWaterContainTheirSourceAmbientLoopWithoutUnitRigs()
    {
        var paths = AssetCatalog.RequiredPaths.Where(p => p.StartsWith("buildings/", StringComparison.Ordinal)
            || p.StartsWith("environment/hex_river_overlay_", StringComparison.Ordinal)
            || p is AssetCatalog.Stream or AssetCatalog.Bridge);
        foreach (string path in paths)
        {
            GlbSummary model = Models.Value[path];
            Assert.Equal(["ambient"], model.Clips);
            Assert.Empty(model.Joints);
            Assert.Equal(1 / 24.0, model.ClipStarts["ambient"], 8);
            Assert.Equal(97 / 24.0, model.ClipLengths["ambient"], 8);
        }
    }

    [Fact]
    public void EightExportedRigsContainRequiredClipsAndActualEquippedSocketChildren()
    {
        IReadOnlyDictionary<string, GlbSummary> models = Models.Value;
        HashSet<string> characters = [];
        foreach (Faction faction in Enum.GetValues<Faction>())
            foreach (UnitType type in Enum.GetValues<UnitType>())
            {
                UnitVisual visual = AssetCatalog.Unit(type, faction);
                Assert.True(characters.Add(visual.Path));
                GlbSummary model = models[visual.Path];
                Assert.Equal(Clips, model.Clips.Order(StringComparer.Ordinal));
                foreach (string bone in new[] { "root", "hand.L", "hand.R", "weapon_socket.L", "weapon_socket.R", visual.StrikeSocket }) Assert.Contains(bone, model.Joints);
                Assert.Equal(AssetCatalog.AttackKeyStart, model.ClipStarts["attack"], 8);
                Assert.Equal(AssetCatalog.AttackKeyEnd, model.ClipLengths["attack"], 8);
                Assert.Equal(AssetCatalog.AttackMarker, (model.ClipStarts["attack"] + model.ClipLengths["attack"]) / 2, 8);
                Assert.Contains(visual.MainProp, model.Children[visual.MainSocket]);
                if (visual.OffhandProp.Length > 0) Assert.Contains(visual.OffhandProp, model.Children[visual.OffhandSocket]);
            }
        Assert.Equal(8, characters.Count);
        Assert.Equal("Archer", AssetCatalog.UnitName(UnitType.Crossbowman));
        Assert.Equal("Hooded crossbowman", AssetCatalog.UnitName(UnitType.Crossbowman, Faction.Skeletons));
        Assert.Equal("A ranged fighter armed with a bow.", AssetCatalog.UnitDescription(UnitType.Crossbowman, Faction.Adventurers));
        Assert.Equal("A ranged fighter armed with a crossbow.", AssetCatalog.UnitDescription(UnitType.Crossbowman, Faction.Skeletons));
    }
    [Theory]
    [InlineData("missing-receipt")]
    [InlineData("duplicate-receipt")]
    [InlineData("foreign-source")]
    [InlineData("unsafe-destination")]
    [InlineData("corrupt-bytes")]
    [InlineData("wrong-dimensions")]
    [InlineData("opaque-format")]
    public void StandaloneTextureAdmissionRejectsInventoryProvenanceAndFormatCorruption(string corruption)
    {
        AuthoredTextureFile[] textures = AuthoredAssets.ReadManifest(DirectoryPath).Textures!;
        string temporary = Path.Combine(Path.GetTempPath(), "odot-textures-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            foreach (AuthoredTextureFile texture in textures)
            {
                string destination = Path.Combine(temporary, texture.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(Path.Combine(DirectoryPath, texture.Path), destination);
            }
            AuthoredAssets.ValidateTextures(temporary, textures);
            AuthoredTextureFile[] changed = textures.ToArray();
            if (corruption == "missing-receipt") changed = changed[..^1];
            else if (corruption == "duplicate-receipt") changed = [.. changed, changed[0]];
            else if (corruption == "foreign-source") changed[0] = changed[0] with { SourcePath = "sources/environment/decals/other.png" };
            else if (corruption == "unsafe-destination") changed[0] = changed[0] with { Path = "../other.png" };
            else
            {
                string path = Path.Combine(temporary, changed[0].Path);
                byte[] bytes = File.ReadAllBytes(path);
                if (corruption == "corrupt-bytes") bytes[^1] ^= 1;
                else if (corruption == "wrong-dimensions") BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(16), 256);
                else bytes[25] = 2; // RGB without coverage alpha is not a decal resource.
                File.WriteAllBytes(path, bytes);
                if (corruption != "corrupt-bytes") changed[0] = changed[0] with { Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes)) };
            }
            Assert.Throws<InvalidDataException>(() => AuthoredAssets.ValidateTextures(temporary, changed));
            Assert.Throws<InvalidDataException>(() => AuthoredAssets.ValidateTextures(temporary, null));
        }
        finally { Directory.Delete(temporary, true); }
    }

    [Fact]
    public void ExportReaderRejectsTruncationPointersAndWrongContainerVersion()
    {
        byte[] arrow = File.ReadAllBytes(Path.Combine(DirectoryPath, AssetCatalog.Arrow));
        Assert.Throws<InvalidDataException>(() => AuthoredAssets.Inspect(arrow.AsSpan()[..^1]));
        Assert.Throws<InvalidDataException>(() => AuthoredAssets.Inspect("version https://git-lfs.github.com/spec/v1"u8));
        BinaryPrimitives.WriteUInt32LittleEndian(arrow.AsSpan(4), 1);
        Assert.Throws<InvalidDataException>(() => AuthoredAssets.Inspect(arrow));
    }
}
