using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class LinuxPackagingTests
{
    private static readonly string[] SystemLibraries = ["/usr/lib/libm.so.6", "/lib/x86_64-linux-gnu/libm.so.6", "/usr/lib64/libm.so.6"];

    [Fact]
    public async Task InstallerSafelyInstallsReinstallsUpgradesAndUninstallsInOwnedPaths()
    {
        if (!OperatingSystem.IsLinux()) return;
        string root = Path.Combine(Path.GetTempPath(), "odot installer " + Guid.NewGuid().ToString("N"));
        string install = Path.Combine(root, "installed app"), bin = Path.Combine(root, "user bin"), apps = Path.Combine(root, "applications");
        Directory.CreateDirectory(root);
        string retained = Path.Combine(root, "player-data.txt");
        await File.WriteAllTextAsync(retained, "keep");
        try
        {
            (string firstArchive, string firstHash) = await Archive(root, "0.1.0-beta.1", "first");
            string firstScript = await Script(root, "0.1.0-beta.1", Path.GetFileName(firstArchive), firstHash);
            var environment = Environment(install, bin, apps, firstArchive);
            await Run(firstScript, environment, succeeds: true);
            Assert.Equal("0.1.0-beta.1", CurrentVersion(install));
            Assert.True(File.Exists(Path.Combine(apps, "odot.desktop")));
            Assert.Contains("Exec=\"" + install + "/launcher\"", await File.ReadAllTextAsync(Path.Combine(apps, "odot.desktop")));
            string systemLibrary = SystemLibraries.First(File.Exists);
            string overlay = Path.Combine(root, "Steam fixture", "gameoverlayrenderer.so");
            Directory.CreateDirectory(Path.GetDirectoryName(overlay)!); File.Copy(systemLibrary, overlay);
            var launchEnvironment = new Dictionary<string, string>(environment) { ["ODOT_STEAM_OVERLAY"] = overlay, ["LD_PRELOAD"] = systemLibrary };
            launchEnvironment.Remove("ODOT_STEAM_DISABLED");
            (int launchCode, string launchOutput, _) = await Capture(Path.Combine(install, "launcher"), launchEnvironment, "one", "two words");
            Assert.Equal(0, launchCode);
            Assert.Contains(systemLibrary + ":" + overlay, launchOutput);
            Assert.Contains("one two words", launchOutput);
            launchEnvironment["ODOT_STEAM_OVERLAY"] = Path.Combine(root, "missing-overlay.so");
            (_, string missingOutput, _) = await Capture(Path.Combine(install, "launcher"), launchEnvironment);
            Assert.Contains(systemLibrary, missingOutput);
            await Run(firstScript, environment, succeeds: true);

            (string secondArchive, string secondHash) = await Archive(root, "0.2.0-beta.1", "second");
            string secondScript = await Script(root, "0.2.0-beta.1", Path.GetFileName(secondArchive), secondHash);
            environment["ODOT_ARCHIVE_URL"] = new Uri(secondArchive).AbsoluteUri;
            await Run(secondScript, environment, succeeds: true);
            Assert.Equal("0.2.0-beta.1", CurrentVersion(install));
            Assert.Equal("second", await File.ReadAllTextAsync(Path.Combine(install, "current", "marker")));

            string invalid = await Script(root, "0.3.0-beta.1", Path.GetFileName(secondArchive), new string('0', 64));
            await Run(invalid, environment, succeeds: false);
            Assert.Equal("0.2.0-beta.1", CurrentVersion(install));
            environment["ODOT_ARCHIVE_URL"] = new Uri(Path.Combine(root, "missing.tar.gz")).AbsoluteUri;
            await Run(secondScript, environment, succeeds: false);
            Assert.Equal("0.2.0-beta.1", CurrentVersion(install));

            await Run(Path.Combine(bin, "odot-uninstall"), environment, succeeds: true);
            Assert.False(Directory.Exists(install));
            Assert.False(File.Exists(Path.Combine(bin, "odot")));
            Assert.False(File.Exists(Path.Combine(bin, "odot-uninstall")));
            Assert.False(File.Exists(Path.Combine(apps, "odot.desktop")));
            Assert.Equal("keep", await File.ReadAllTextAsync(retained));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void InstallerBakesPublicReleaseUrlAndNativeOverlayPolicy()
    {
        string script = Runner.LinuxInstaller("1.2.3", "odot-1.2.3-linux-x64.tar.gz", new string('a', 64));
        Assert.Contains("https://github.com/seraph1nia/odot-game/releases/download/v1.2.3/odot-1.2.3-linux-x64.tar.gz", script);
        Assert.Contains("sha256sum -c -", script);
        Assert.Contains("ODOT_STEAM_DISABLED", script);
        Assert.Contains("LD_PRELOAD", script);
        Assert.Contains("exec \"$GAME\" \"$@\"", script);
    }

    [Fact]
    public void InnoCompilerAcquisitionIsPinned()
    {
        using JsonDocument pin = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root(), "tools", "Distribution", "inno-setup.json")));
        Assert.Equal(Runner.InnoSetupVersion, pin.RootElement.GetProperty("version").GetString());
        Assert.Equal(Runner.InnoSetupChecksum, pin.RootElement.GetProperty("sha256").GetString());
        Assert.Equal("https://github.com/jrsoftware/issrc/releases/download/is-6_7_3/innosetup-6.7.3.exe", pin.RootElement.GetProperty("url").GetString());
    }

    private static Dictionary<string, string> Environment(string install, string bin, string apps, string archive) => new()
    {
        ["ODOT_INSTALL_TESTING"] = "1",
        ["ODOT_ARCHIVE_URL"] = new Uri(archive).AbsoluteUri,
        ["ODOT_INSTALL_ROOT"] = install,
        ["ODOT_BIN_DIR"] = bin,
        ["ODOT_APPLICATIONS_DIR"] = apps,
        ["ODOT_STEAM_DISABLED"] = "1"
    };

    private static string CurrentVersion(string install)
        => Path.GetFileName(new DirectoryInfo(Path.Combine(install, "current")).ResolveLinkTarget(false)!.FullName);

    private static async Task<string> Script(string root, string version, string archive, string hash)
    {
        string path = Path.Combine(root, "install-" + version + "-" + Guid.NewGuid().ToString("N") + ".sh");
        await File.WriteAllTextAsync(path, Runner.LinuxInstaller(version, archive, hash));
        return path;
    }

    private static async Task<(string Path, string Hash)> Archive(string root, string version, string marker)
    {
        string path = Path.Combine(root, "odot-" + version + "-linux-x64.tar.gz");
        await using (var output = File.Create(path))
        await using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize))
        using (var tar = new TarWriter(gzip, leaveOpen: false))
        {
            Add(tar, version, "odot.x86_64", "#!/bin/sh\nprintf '%s\\n' \"${LD_PRELOAD:-}\"\nprintf '%s\\n' \"$*\"\n", 493);
            Add(tar, version, "odot.pck", "fixture", 420);
            Add(tar, version, "build-info.json", "{\"version\":\"" + version + "\"}", 420);
            Add(tar, version, "marker", marker, 420);
        }
        await using var input = File.OpenRead(path);
        return (path, Convert.ToHexString(await SHA256.HashDataAsync(input)).ToLowerInvariant());
    }

    private static void Add(TarWriter tar, string version, string name, string content, int mode)
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var entry = new PaxTarEntry(TarEntryType.RegularFile, "odot-" + version + "/" + name) { DataStream = stream, Mode = (UnixFileMode)mode };
        tar.WriteEntry(entry);
        stream.Dispose();
    }

    private static async Task Run(string script, IReadOnlyDictionary<string, string> environment, bool succeeds)
    {
        (int code, string stdout, string stderr) = await Capture("/bin/sh", environment, script);
        Assert.True(succeeds == (code == 0), $"exit={code}\nstdout={stdout}\nstderr={stderr}");
    }

    private static async Task<(int Code, string Output, string Error)> Capture(string executable, IReadOnlyDictionary<string, string> environment, params string[] arguments)
    {
        var start = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        foreach ((string key, string value) in environment) start.Environment[key] = value;
        using Process process = Process.Start(start)!;
        string stdout = await process.StandardOutput.ReadToEndAsync(), stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, stdout, stderr);
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Odot.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
