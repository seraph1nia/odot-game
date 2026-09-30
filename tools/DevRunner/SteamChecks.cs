using System.Security.Cryptography;
using System.Text.Json;

namespace DevRunner;

internal static class SteamPackaging
{
    internal static void ValidateAppId(bool production, uint? appId)
    {
        if (appId == 0 || (production && appId is null or 480))
            throw new ArgumentException("Production requires --steam-app-id with this game's own AppID, not 480; AppIDs must be positive.");
    }
}

internal sealed partial class Runner
{
    private async Task VerifySteamFiles()
    {
        string directory = Path.Combine(GameDirectory, "addons", "godotsteam");
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(directory, "manifest.json"), cancellation));
        var root = manifest.RootElement;
        if (root.GetProperty("release").GetString() != "v4.22.1-gde" || root.GetProperty("prerelease").GetBoolean()
            || root.GetProperty("repository_archived").GetBoolean() || root.GetProperty("publisher_stability").GetString() != "unstable"
            || root.GetProperty("archive_sha256").GetString() != "2b12b3499434c50da16104a0d22b725aee15cc5cd41223c1cea825bae59bfa8f")
            throw new InvalidOperationException("Steam dependency pin/qualification changed; assess and update it intentionally.");
        foreach (var file in root.GetProperty("files").EnumerateObject())
        {
            string path = Path.GetFullPath(Path.Combine(directory, file.Name));
            if (!path.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new InvalidOperationException("Invalid Steam manifest file path.");
            await using var stream = File.OpenRead(path);
            string hash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellation)).ToLowerInvariant();
            if (hash != file.Value.GetString()) throw new InvalidOperationException($"Pinned Steam file hash mismatch: {file.Name}.");
        }
        Console.WriteLine("Pinned GodotSteam v4.22.1-gde files verified; publisher unstable qualification remains recorded.");
    }

    private async Task CheckSteamExtension()
    {
        DesktopExport layout = DesktopExport.For(options.ExportTarget);
        if (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture != System.Runtime.InteropServices.Architecture.X64
            || (layout.Target == "windows-x64" ? !OperatingSystem.IsWindows() : !OperatingSystem.IsLinux()))
            throw new VerificationPrerequisiteException("Steam extension verification requires a native x86_64 host matching --target.");
        if (options.Exported)
        {
            if (!File.Exists(Path.Combine(_root, "dist", layout.Kind(options.Production), layout.Executable)))
                throw new InvalidOperationException("Missing development client export; run mise run export-client first.");
            await VerifySteamFiles();
        }
        else
        {
            await Prepare();
            // Recreate derived startup metadata to exercise new extension discovery
            // even when asset imports are already cached. Never remove asset caches.
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                File.Delete(Path.Combine(GameDirectory, ".godot", "extension_list.cfg"));
                await Import("steam-editor-import-" + attempt);
            }
        }
        await SteamExtensionProbe(options.Exported, options.Offline);
        Console.WriteLine("Single-account extension compatibility only; remote channels, invites and relay acceptance remain unexecuted.");
    }

    private async Task SteamExtensionProbe(bool exported, bool offline)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(options.StartupTimeout);
        await using var owned = new ScenarioScope("steam-extension", _evidence);
        string executable = "godot";
        string working = _root;
        var args = new List<string> { "--headless" };
        if (exported)
        {
            DesktopExport layout = DesktopExport.For(options.ExportTarget);
            string source = Path.Combine(_root, "dist", layout.Kind(options.Production));
            working = Path.Combine(owned.Directory, "package");
            foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                string target = Path.Combine(working, Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
                if (OperatingSystem.IsLinux()) File.SetUnixFileMode(target, File.GetUnixFileMode(file));
            }
            executable = Path.Combine(working, layout.Executable);
            foreach (string name in new[] { layout.NativeLibrary, layout.SteamLibrary })
            {
                await using var original = File.OpenRead(Path.Combine(GameDirectory, "addons", "godotsteam", layout.NativeDirectory, name));
                await using var packaged = File.OpenRead(Path.Combine(working, name));
                byte[] originalHash = await SHA256.HashDataAsync(original, cancellation);
                byte[] packagedHash = await SHA256.HashDataAsync(packaged, cancellation);
                if (!originalHash.SequenceEqual(packagedHash))
                    throw new InvalidOperationException($"Exported Steam dependency differs from the pin: {name}.");
            }
            if (!File.Exists(Path.Combine(working, "licenses", "godotsteam", "license.md")))
                throw new InvalidOperationException("Exported Steam license is missing.");
        }
        else args.AddRange(["--path", GameDirectory]);
        args.AddRange(["--", offline ? "--steam-probe-offline" : "--steam-probe"]);
        var environment = owned.EnvironmentFor("probe");
        if (exported && OperatingSystem.IsWindows()) WindowsStandaloneEnvironment(environment);
        await using var child = owned.Own(new Child("steam-extension-probe", executable, args, _root,
            game: true, quiet: true, workingDirectory: working, environment: environment,
            evidenceDirectory: owned.EvidenceDirectory, sanitizeSteam: true));
        await child.WaitFor(e => e.Type == "steam-probe", offline ? "optional initialization" : "Steam extension lifecycle",
            options.StartupTimeout, deadline.Token);
        int code = await child.WaitExit(deadline.Token);
        Require(code == 0 && !child.HasEngineErrors, "native extension probe exits successfully without engine errors");
        await owned.DisposeAsync(); owned.CheckErrors();
    }
}
