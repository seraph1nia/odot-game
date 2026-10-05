using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class AssetInventoryTests
{
    [Theory]
    [InlineData("")]
    [InlineData(".remap")]
    [InlineData(".import")]
    public void SourceAndPackedMappingFilesProjectExactlyTheSameRequiredInventory(string suffix)
    {
        string[] required = AssetCatalog.RequiredPaths.Select(path => AssetCatalog.Root + path).ToArray();
        string[] observed = required.Select(path => path + suffix).Concat(["res://Assets/TrioUI/README.md", "res://Assets/Authored/manifest.json"]).ToArray();
        Assert.Equal(required, AssetCatalog.InstalledModels(observed));
        Assert.Equal(required, AssetCatalog.InstalledModels(observed.Concat(required)));
        Assert.Throws<InvalidDataException>(() => AssetCatalog.InstalledModels(observed.Skip(1)));
    }

    [Theory]
    [InlineData("res://Assets/KayKit/Medieval/farm.glb")]
    [InlineData("res://Assets/KayKit/Medieval/farm.glb.import")]
    [InlineData("res://Assets/KayKit/Medieval/farm.glb.remap")]
    [InlineData("res://Assets/KayKit/Medieval/farm.bin")]
    [InlineData("res://Assets/Authored/unused.glb.import")]
    [InlineData("res://Assets/unregistered.fbx.remap")]
    public void PackedAndSourceProjectionRejectEveryLegacyOrUnregisteredBundle(string extra)
    {
        var observed = AssetCatalog.RequiredPaths.Select(path => AssetCatalog.Root + path + ".import").Append(extra);
        InvalidDataException error = Assert.Throws<InvalidDataException>(() => AssetCatalog.InstalledModels(observed));
        Assert.Contains("unexpected:", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownOrRepeatedMappingSuffixCannotPretendToSupplyAMissingModel()
    {
        string[] required = AssetCatalog.RequiredPaths.Select(path => AssetCatalog.Root + path).ToArray();
        Assert.Throws<InvalidDataException>(() => AssetCatalog.InstalledModels(required.Skip(1).Append(required[0] + ".import.remap")));
        Assert.Throws<InvalidDataException>(() => AssetCatalog.InstalledModels(required.Skip(1).Append(required[0] + ".unknown")));
    }
}
