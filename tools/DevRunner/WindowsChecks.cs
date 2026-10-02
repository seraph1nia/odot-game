using Game.Core;
using System.Text.Json;

namespace DevRunner;

internal sealed partial class Runner
{
    private static void WindowsStandaloneEnvironment(Dictionary<string, string?> environment)
    {
        environment["ODOT_STEAM_DISABLED"] = "1";
        environment["PATH"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32");
        foreach (string name in new[] { "DOTNET_ROOT", "DOTNET_ROOT_X64", "DOTNET_ROOT(x86)", "DOTNET_SHARED_STORE", "DOTNET_ADDITIONAL_DEPS" })
            environment[name] = null;
        environment["DOTNET_MULTILEVEL_LOOKUP"] = "0";
    }

    private async Task WindowsCi()
    {
        if (!OperatingSystem.IsWindows() || System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture != System.Runtime.InteropServices.Architecture.X64)
            throw new VerificationPrerequisiteException("ci-windows requires native Windows x86_64. Use the Windows validation workflow; Wine is not native verification.");
        await Preflight();
        await VerifySteamFiles();
        await Execute("restore", "dotnet", "restore", "Odot.slnx", "--locked-mode");
        await Execute("format", "dotnet", "format", "Odot.slnx", "--verify-no-changes", "--no-restore");
        await BuildAndImport();
        await BuildIdentityProbe(null);
        await SteamExtensionProbe(exported: false, offline: true);
        await PrepareTemplates();
        await Export(false);
        string payload = Path.Combine(_root, "dist", "windows-client");
        await _evidence.Measure("windows-package-inventory", "package", () => ValidateWindowsPackage(payload));
        await SteamExtensionProbe(exported: true, offline: true);
        await BuildIdentityProbe(null, Path.Combine(payload, "odot.exe"));
        // One bounded startup/economy slice catches runtime/PCK/managed-load failures
        // that a cross-export or unit fixture cannot establish. No rendered battle.
        await _evidence.Measure("windows-offline-solo", "package", () => WindowsSolo(payload));
        Console.WriteLine("Native Windows source/export Steam compatibility and standalone offline solo passed. No installer/graphics/real Steam acceptance claimed; no uploads or publication.");
    }

    private static Task ValidateWindowsPackage(string directory)
    {
        foreach (string name in new[] { "odot.exe", "odot.pck", "libgodotsteam.windows.template_release.x86_64.dll", "steam_api64.dll",
            "licenses/godotsteam/license.md", "licenses/godotsteam/manifest.json", "licenses/godotsteam/README.odot.md" })
            if (!File.Exists(Path.Combine(directory, name))) throw new InvalidOperationException("Missing Windows package file: " + name);
        foreach (string name in new[] { "Game.dll", "Game.Core.dll", "GodotSharp.dll", "hostfxr.dll", "hostpolicy.dll", "coreclr.dll", "System.Private.CoreLib.dll" })
            if (!Directory.EnumerateFiles(directory, name, SearchOption.AllDirectories).Any())
                throw new InvalidOperationException("Missing Windows managed/runtime payload: " + name);
        string config = Directory.EnumerateFiles(directory, "Game.runtimeconfig.json", SearchOption.AllDirectories).Single();
        using var json = JsonDocument.Parse(File.ReadAllText(config));
        bool bundled = json.RootElement.GetProperty("runtimeOptions").GetProperty("includedFrameworks").EnumerateArray()
            .Any(framework => framework.GetProperty("name").GetString() == "Microsoft.NETCore.App" && framework.GetProperty("version").GetString() == "10.0.12");
        Require(bundled, "Windows export bundles the matching .NET runtime");
        return Task.CompletedTask;
    }

    private async Task WindowsSolo(string payload)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(options.StartupTimeout * 2);
        await using var owned = new ScenarioScope("windows-offline-solo", _evidence);
        var worker = new Runner(options, deadline.Token, _evidence, owned);
        await using var solo = worker.StartGameRole("windows-solo", "solo", true, 0, Path.Combine(payload, "odot.exe"));
        await solo.WaitFor(e => e.Type == "ack" && e.Result?.Accepted == true && e.State?.Phase == Phase.Building,
            "standalone Windows solo starts through normal authority", options.StartupTimeout, deadline.Token);
        await worker.SessionAction(solo, "build 0 farm", deadline.Token);
        await worker.SessionAction(solo, "build 1 barracks", deadline.Token);
        await worker.SessionAction(solo, "build 2 metalmine", deadline.Token);
        await worker.SessionAction(solo, "ready", deadline.Token);
        GameEvent recruited = await worker.SessionAction(solo, "recruit 1", deadline.Token);
        Require(State(recruited).Players.Single().Soldiers.Length == 1, "Windows standalone runtime/PCK and normal solo requests work without Steam or SDK PATH");
        await owned.DisposeAsync(); owned.CheckErrors();
    }
}
