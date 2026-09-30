using Game.Distribution;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task VerifyInstalledLinux()
    {
        PrivateDisplay.CheckPrerequisites();
        ReleaseVersion version = ReleaseVersion.FromTag(options.ReleaseTag);
        string release = Path.Combine(_root, "dist", "releases", version.Value);
        string archive = Path.Combine(release, $"odot-{version.Value}-linux-x64.tar.gz");
        string script = Path.Combine(release, $"odot-{version.Value}-linux-x64-install.sh");
        ReleaseIdentity identity = ReleaseIdentity.FromJson(await File.ReadAllTextAsync(Path.Combine(release, "build-info.linux-x64.json"), cancellation));
        if (!File.Exists(archive) || !File.Exists(script)) throw new InvalidOperationException("Existing Linux release assets are required; this verification never rebuilds them.");
        await VerifyManifest(release, "linux-x64");
        await using var owned = new ScenarioScope("installed-linux", _evidence);
        string install = Path.Combine(owned.Directory, "install root"), bin = Path.Combine(owned.Directory, "bin path"), apps = Path.Combine(owned.Directory, "applications path");
        string playerData = Path.Combine(owned.Directory, "retained-player-data");
        await File.WriteAllTextAsync(playerData, "preserve", cancellation);
        var environment = new Dictionary<string, string?>
        {
            ["ODOT_INSTALL_TESTING"] = "1",
            ["ODOT_ARCHIVE_URL"] = new Uri(archive).AbsoluteUri,
            ["ODOT_INSTALL_ROOT"] = install,
            ["ODOT_BIN_DIR"] = bin,
            ["ODOT_APPLICATIONS_DIR"] = apps,
            ["ODOT_STEAM_DISABLED"] = "1"
        };
        await RunOwned("linux-install", "/bin/sh", [script], environment);
        string launcher = Path.Combine(install, "launcher");
        Require(File.Exists(launcher) && File.Exists(Path.Combine(apps, "odot.desktop")) && File.Exists(Path.Combine(bin, "odot-uninstall")),
            "Linux install creates its stable launcher, desktop entry and uninstall command");
        await BuildIdentityProbe(identity, launcher);
        await InstalledSolo(launcher, "installed-linux-solo");
        var graphical = new Runner(options with { InstalledClient = launcher }, cancellation, _evidence);
        await graphical.UiTests("installed-linux");
        await RunOwned("linux-uninstall", "/bin/sh", [Path.Combine(bin, "odot-uninstall")], environment);
        Require(!Directory.Exists(install) && !File.Exists(Path.Combine(bin, "odot")) && !File.Exists(Path.Combine(bin, "odot-uninstall"))
            && !File.Exists(Path.Combine(apps, "odot.desktop")) && await File.ReadAllTextAsync(playerData, cancellation) == "preserve",
            "Linux uninstall removes owned application paths and preserves player data");
        await owned.DisposeAsync(); owned.CheckErrors();
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private async Task VerifyInstalledWindows()
    {
        ReleaseVersion version = ReleaseVersion.FromTag(options.ReleaseTag);
        string release = Path.Combine(_root, "dist", "releases", version.Value);
        string installer = Path.Combine(release, $"odot-{version.Value}-windows-x64-setup.exe");
        ReleaseIdentity identity = ReleaseIdentity.FromJson(await File.ReadAllTextAsync(Path.Combine(release, "build-info.windows-x64.json"), cancellation));
        if (!File.Exists(installer)) throw new InvalidOperationException("Existing Windows installer is required; this verification never rebuilds it.");
        await VerifyManifest(release, "windows-x64");
        await using var owned = new ScenarioScope("installed-windows", _evidence);
        string install = Path.Combine(owned.Directory, "installed app"), group = "Odot CI " + Guid.NewGuid().ToString("N");
        string playerData = Path.Combine(owned.Directory, "retained-player-data");
        await File.WriteAllTextAsync(playerData, "preserve", cancellation);
        string[] arguments = ["/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART", "/CLOSEAPPLICATIONS", "/FORCECLOSEAPPLICATIONS", "/DIR=" + install, "/GROUP=" + group];
        await RunOwned("windows-install", installer, arguments);
        string payload = Path.Combine(install, "game"), executable = Path.Combine(payload, "odot.exe");
        string shortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", group, "Odot.lnk");
        Require(File.Exists(executable) && File.Exists(shortcut) && File.Exists(Path.Combine(install, "unins000.exe")),
            "Windows installer creates the client, Start Menu shortcut and registered uninstaller");
        using (Microsoft.Win32.RegistryKey? uninstall = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{D875B866-CA9C-4C70-914F-DBBD6507EE41}_is1"))
            Require(uninstall?.GetValue("UninstallString") is string, "Windows installer registers its per-user uninstaller");
        await File.WriteAllTextAsync(Path.Combine(payload, "obsolete-managed-file"), "stale", cancellation);
        await using var runningScope = new ScenarioScope("windows-running-upgrade", _evidence);
        var runningWorker = new Runner(options, cancellation, _evidence, runningScope);
        Child running = runningWorker.StartGameRole("windows-running-game", "solo", true, 0, executable);
        await running.WaitFor(e => e.Type == "ack" && e.Result?.Accepted == true, "running installed game", options.StartupTimeout, cancellation);
        running.ExpectedFailure = true;
        await RunOwned("windows-upgrade", installer, arguments);
        await running.Exited.WaitAsync(TimeSpan.FromSeconds(10), cancellation);
        Require(running.HasExited, "Windows upgrade closes its running Odot payload before replacement");
        await runningScope.DisposeAsync(); runningScope.CheckErrors();
        Require(!File.Exists(Path.Combine(payload, "obsolete-managed-file")), "Windows full upgrade removes stale managed payload files");
        await ValidateWindowsPackage(payload);
        await BuildIdentityProbe(identity, executable);
        await InstalledSolo(executable, "installed-windows-solo");
        await RunOwned("windows-uninstall", Path.Combine(install, "unins000.exe"), ["/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART"]);
        Require(!File.Exists(executable) && !File.Exists(shortcut) && await File.ReadAllTextAsync(playerData, cancellation) == "preserve",
            "Windows uninstall removes owned payload/shortcut and preserves player data");
        await owned.DisposeAsync(); owned.CheckErrors();
    }

    private async Task InstalledSolo(string executable, string name)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(options.StartupTimeout * 2);
        await using var owned = new ScenarioScope(name, _evidence);
        var worker = new Runner(options, deadline.Token, _evidence, owned);
        await using var solo = worker.StartGameRole(name, "solo", true, 0, executable);
        await solo.WaitFor(e => e.Type == "ack" && e.Result?.Accepted == true && e.State?.Phase == Game.Core.Phase.Building,
            "installed offline solo starts", options.StartupTimeout, deadline.Token);
        await worker.SessionAction(solo, "build 0 farm", deadline.Token);
        await owned.DisposeAsync(); owned.CheckErrors();
    }

    private async Task VerifyManifest(string release, string target)
    {
        foreach (string line in await File.ReadAllLinesAsync(Path.Combine(release, $"SHA256SUMS.{target}"), cancellation))
        {
            string[] parts = line.Split("  ", 2, StringSplitOptions.None);
            if (parts.Length != 2 || !File.Exists(Path.Combine(release, parts[1])) || await Hash(Path.Combine(release, parts[1])) != parts[0])
                throw new InvalidOperationException("Release checksum manifest does not match " + (parts.Length == 2 ? parts[1] : "an entry") + ".");
        }
    }

    private async Task RunOwned(string name, string executable, IEnumerable<string> arguments, IReadOnlyDictionary<string, string?>? environment = null)
    {
        await _evidence.Measure(name, "package", async () =>
        {
            await using var child = new Child(name, executable, arguments, _root, environment: environment, evidenceDirectory: _evidence.Directory);
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation); deadline.CancelAfter(options.Timeout);
            int code = await child.WaitExit(deadline.Token);
            if (code != 0) throw new InvalidOperationException($"{name} exited {code}.\n{child.Tail()}");
        });
    }
}
