using System.Security.Cryptography;
using DevRunner;
using Game.Distribution;
using Xunit;

namespace DevRunner.Tests;

public sealed class ReleaseAssetAssemblyTests
{
    [Fact]
    public async Task AssemblesOnlyMatchingAllowlistedAssets()
    {
        string directory = await Fixture();
        try
        {
            await ReleaseAssetAssembly.Assemble(directory, "v0.1.0-beta.1", new string('a', 40));
            Assert.Equal(["SHA256SUMS", "build-info.json", "odot-0.1.0-beta.1-linux-x64-install.sh", "odot-0.1.0-beta.1-linux-x64.tar.gz", "odot-0.1.0-beta.1-windows-x64-setup.exe"],
                Directory.EnumerateFiles(directory).Select(path => Path.GetFileName(path)!).Order(StringComparer.Ordinal));
            string[] expected = ["odot-0.1.0-beta.1-linux-x64.tar.gz", "odot-0.1.0-beta.1-linux-x64-install.sh", "odot-0.1.0-beta.1-windows-x64-setup.exe", "build-info.json"];
            Assert.Equal(await Task.WhenAll(expected.Select(name => Line(directory, name))),
                await File.ReadAllLinesAsync(Path.Combine(directory, "SHA256SUMS")));
            using var metadata = System.Text.Json.JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(directory, "build-info.json")));
            Assert.Equal(new string('a', 40), metadata.RootElement.GetProperty("SourceCommit").GetString());
            Assert.Equal(["linux-x64", "windows-x64"], metadata.RootElement.GetProperty("Targets").EnumerateArray()
                .Select(target => target.GetProperty("Target").GetString()));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task RejectsMissingChangedAndMismatchedInputs()
    {
        string missing = await Fixture(); File.Delete(Path.Combine(missing, "odot-0.1.0-beta.1-linux-x64.tar.gz"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReleaseAssetAssembly.Assemble(missing, "v0.1.0-beta.1", new string('a', 40)));
        Directory.Delete(missing, true);
        string changed = await Fixture(); await File.AppendAllTextAsync(Path.Combine(changed, "odot-0.1.0-beta.1-windows-x64-setup.exe"), "changed");
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReleaseAssetAssembly.Assemble(changed, "v0.1.0-beta.1", new string('a', 40)));
        Directory.Delete(changed, true);
        string mismatch = await Fixture();
        var wrong = ReleaseIdentity.Create("v0.1.0-beta.1", new string('b', 40), "windows-x64", false, null);
        await File.WriteAllTextAsync(Path.Combine(mismatch, "build-info.windows-x64.json"), wrong.ToJson());
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReleaseAssetAssembly.Assemble(mismatch, "v0.1.0-beta.1", new string('a', 40)));
        Directory.Delete(mismatch, true);
    }

    private static async Task<string> Fixture()
    {
        string directory = Path.Combine(Path.GetTempPath(), "odot-assets-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        string archive = "odot-0.1.0-beta.1-linux-x64.tar.gz", script = "odot-0.1.0-beta.1-linux-x64-install.sh", installer = "odot-0.1.0-beta.1-windows-x64-setup.exe";
        await File.WriteAllTextAsync(Path.Combine(directory, archive), "linux");
        await File.WriteAllTextAsync(Path.Combine(directory, script), "script");
        await File.WriteAllTextAsync(Path.Combine(directory, installer), "windows");
        await File.WriteAllTextAsync(Path.Combine(directory, "build-info.linux-x64.json"), ReleaseIdentity.Create("v0.1.0-beta.1", new string('a', 40), "linux-x64", false, null).ToJson());
        await File.WriteAllTextAsync(Path.Combine(directory, "build-info.windows-x64.json"), ReleaseIdentity.Create("v0.1.0-beta.1", new string('a', 40), "windows-x64", false, null).ToJson());
        await File.WriteAllLinesAsync(Path.Combine(directory, "SHA256SUMS.linux-x64"), [await Line(directory, archive), await Line(directory, script)]);
        await File.WriteAllLinesAsync(Path.Combine(directory, "SHA256SUMS.windows-x64"), [await Line(directory, installer)]);
        return directory;
    }
    private static async Task<string> Line(string directory, string name)
    {
        await using Stream stream = File.OpenRead(Path.Combine(directory, name));
        return Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLowerInvariant() + "  " + name;
    }
}
