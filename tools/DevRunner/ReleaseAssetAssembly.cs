using System.Security.Cryptography;
using System.Text.Json;
using Game.Distribution;

namespace DevRunner;

internal static class ReleaseAssetAssembly
{
    public static async Task Assemble(string directory, string tag, string expectedCommit, CancellationToken cancellation = default)
    {
        ReleaseVersion version = ReleaseVersion.FromTag(tag);
        string linuxArchive = $"odot-{version.Value}-linux-x64.tar.gz";
        string linuxScript = $"odot-{version.Value}-linux-x64-install.sh";
        string windowsInstaller = $"odot-{version.Value}-windows-x64-setup.exe";
        string[] inputs = [linuxArchive, linuxScript, windowsInstaller, "build-info.linux-x64.json", "build-info.windows-x64.json", "SHA256SUMS.linux-x64", "SHA256SUMS.windows-x64"];
        string[] actual = Directory.Exists(directory) ? Directory.EnumerateFiles(directory).Select(path => Path.GetFileName(path)!).Order(StringComparer.Ordinal).ToArray() : [];
        if (!actual.SequenceEqual(inputs.Order(StringComparer.Ordinal), StringComparer.Ordinal))
            throw new InvalidOperationException("Release transfer set is missing, unexpected, or contains non-public files.");
        await VerifyManifest(directory, "linux-x64", [linuxArchive, linuxScript], cancellation);
        await VerifyManifest(directory, "windows-x64", [windowsInstaller], cancellation);
        ReleaseIdentity linux = ReleaseIdentity.FromJson(await File.ReadAllTextAsync(Path.Combine(directory, "build-info.linux-x64.json"), cancellation));
        ReleaseIdentity windows = ReleaseIdentity.FromJson(await File.ReadAllTextAsync(Path.Combine(directory, "build-info.windows-x64.json"), cancellation));
        if (linux.Version != version.Value || windows.Version != version.Value || linux.SourceCommit != expectedCommit || windows.SourceCommit != expectedCommit
            || linux.Target != "linux-x64" || windows.Target != "windows-x64" || linux.Channel != windows.Channel || linux.Repository != windows.Repository
            || linux.SteamMode != windows.SteamMode || linux.SteamAppId != windows.SteamAppId)
            throw new InvalidOperationException("Platform build identities disagree with the selected tag, commit, or profile.");
        var metadata = new
        {
            linux.Version,
            linux.SourceCommit,
            linux.Channel,
            linux.Repository,
            Targets = new[] { Target(linux), Target(windows) }
        };
        await File.WriteAllTextAsync(Path.Combine(directory, "build-info.json"), JsonSerializer.Serialize(metadata, Evidence.JsonOptions) + "\n", cancellation);
        foreach (string name in inputs.Where(name => name.StartsWith("build-info.", StringComparison.Ordinal) || name.StartsWith("SHA256SUMS.", StringComparison.Ordinal)))
            File.Delete(Path.Combine(directory, name));
        string[] publicFiles = [linuxArchive, linuxScript, windowsInstaller, "build-info.json"];
        var lines = new List<string>();
        foreach (string name in publicFiles) lines.Add(await Hash(Path.Combine(directory, name), cancellation) + "  " + name);
        await File.WriteAllLinesAsync(Path.Combine(directory, "SHA256SUMS"), lines, cancellation);
        string[] final = Directory.EnumerateFiles(directory).Select(path => Path.GetFileName(path)!).Order(StringComparer.Ordinal).ToArray();
        if (!final.SequenceEqual(publicFiles.Append("SHA256SUMS").Order(StringComparer.Ordinal), StringComparer.Ordinal))
            throw new InvalidOperationException("Final public release allowlist is inconsistent.");
    }

    private static object Target(ReleaseIdentity identity) => new
    {
        identity.Target,
        identity.EngineVersion,
        identity.SdkVersion,
        identity.RuntimeVersion,
        identity.SteamMode,
        identity.SteamAppId
    };

    private static async Task VerifyManifest(string directory, string target, string[] expected, CancellationToken cancellation)
    {
        string[] lines = await File.ReadAllLinesAsync(Path.Combine(directory, $"SHA256SUMS.{target}"), cancellation);
        var names = new List<string>();
        foreach (string line in lines)
        {
            string[] parts = line.Split("  ", 2, StringSplitOptions.None);
            if (parts.Length != 2 || !expected.Contains(parts[1], StringComparer.Ordinal)
                || !File.Exists(Path.Combine(directory, parts[1])) || await Hash(Path.Combine(directory, parts[1]), cancellation) != parts[0])
                throw new InvalidOperationException("Release checksum manifest is invalid for " + target + ".");
            names.Add(parts[1]);
        }
        if (!names.Order(StringComparer.Ordinal).SequenceEqual(expected.Order(StringComparer.Ordinal), StringComparer.Ordinal))
            throw new InvalidOperationException("Release checksum manifest is incomplete for " + target + ".");
    }

    private static async Task<string> Hash(string path, CancellationToken cancellation)
    {
        await using Stream stream = File.OpenRead(path);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellation)).ToLowerInvariant();
    }
}
