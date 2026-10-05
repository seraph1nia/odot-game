using System.Buffers.Binary;
using Game;
using Game.Core;
using Xunit;

namespace DevRunner.Tests;

public sealed class AuthoredAssetTests
{
    private static readonly string[] Clips = ["attack", "death", "hit", "idle", "run", "walk"];
    private static string DirectoryPath
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Odot.slnx"))) directory = directory.Parent;
            return Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), "src", "Game", "Assets", "Authored");
        }
    }
    [Fact]
    public void RequiredExportDistributionMatchesImmutableProvenanceAndEmbeddedDependencies()
    {
        IReadOnlyDictionary<string, GlbSummary> models = AuthoredAssets.ValidateDistribution(Path.GetDirectoryName(DirectoryPath)!);
        Assert.Equal(AssetCatalog.RequiredPaths, models.Keys.Order(StringComparer.Ordinal));
        Assert.All(models.Values, m => { Assert.True(m.Meshes > 0); Assert.True(m.Materials > 0); });
        AuthoredAssetManifest manifest = AuthoredAssets.ReadManifest(DirectoryPath);
        Assert.Equal(AssetCatalog.Revision, manifest.Revision);
        Assert.Equal("the repo is my own work, so yes, use it", manifest.Permission);
    }
    [Fact]
    public void EveryCanonicalRoleUsesDedicatedAvailableGeometryAndTruthfulNames()
    {
        IReadOnlyDictionary<string, GlbSummary> models = AuthoredAssets.Validate(DirectoryPath);
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
    [Fact]
    public void EightExportedRigsContainRequiredClipsAndActualEquippedSocketChildren()
    {
        IReadOnlyDictionary<string, GlbSummary> models = AuthoredAssets.Validate(DirectoryPath);
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
