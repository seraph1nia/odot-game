using System.Globalization;
namespace DevRunner;

internal sealed record Options(string Command, string Host, string Bind, int? Port, int StartupTimeout, int Timeout, string[] EngineArgs, string? SessionFile,
    int Jobs = 2, string? Scenario = null, string? EvidenceDirectory = null, string? WorkerToken = null,
    bool Production = false, uint? SteamAppId = null, bool Offline = false, bool Exported = false, int Guests = 1,
    string? SteamRole = null, ulong? Lobby = null, string? ReleaseTag = null, string ExportTarget = "linux-x64", string? InstalledClient = null)
{
    public static Options Parse(string[] args)
    {
        string command = args.FirstOrDefault() ?? "help";
        string host = "127.0.0.1";
        string bind = "127.0.0.1";
        int? port = null;
        int startup = 15000;
        int timeout = 180000;
        string? sessionFile = null;
        int jobs = 2;
        int guests = 1;
        string? scenario = null, evidence = null, workerToken = null;
        bool production = false, offline = false, exported = false;
        uint? steamAppId = null;
        string? steamRole = null;
        ulong? lobby = null;
        string? releaseTag = null;
        string exportTarget = command is "ci-windows" or "package-windows" ? "windows-x64" : "linux-x64";
        string? installedClient = null;
        var engineArgs = new List<string>();
        for (int i = 1; i < args.Length; i++)
        {
            string Value() => ++i < args.Length ? args[i] : throw new ArgumentException($"Missing value for {args[i - 1]}.");
            switch (args[i])
            {
                case "--host": host = Value(); break;
                case "--bind": bind = Value(); break;
                case "--port": port = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--startup-timeout-ms": startup = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--timeout-ms": timeout = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--session-file": sessionFile = Value(); break;
                case "--engine-arg": engineArgs.Add(Value()); break;
                case "--jobs": jobs = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--guests" when command == "dev": guests = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--scenario": scenario = Value(); break;
                case "--evidence-directory" when command == "_ui-worker": evidence = Value(); break;
                case "--worker-token" when command == "_ui-worker": workerToken = Value(); break;
                case "--production" when command is "export-client" or "package-linux" or "package-windows" or "verify-installed-linux" or "verify-installed-windows" or "release-preflight" or "assemble-release-assets": production = true; break;
                case "--steam-app-id" when command is "export-client" or "package-linux" or "package-windows" or "verify-installed-linux" or "verify-installed-windows" or "release-preflight" or "assemble-release-assets": steamAppId = uint.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--tag" when command is "export-client" or "package-linux" or "package-windows" or "verify-installed-linux" or "verify-installed-windows" or "release-preflight" or "assemble-release-assets": releaseTag = Value(); break;
                case "--installed-client" when command == "_ui-worker": installedClient = Value(); break;
                case "--target" when command is "export-client" or "check-steam-extension" or "release-preflight": exportTarget = Value(); break;
                case "--offline" when command == "check-steam-extension": offline = true; break;
                case "--exported" when command is "check-steam-extension" or "test-steam": exported = true; break;
                case "--role" when command == "test-steam": steamRole = Value(); break;
                case "--lobby" when command == "test-steam": lobby = ulong.Parse(Value(), NumberStyles.None, CultureInfo.InvariantCulture); break;
                case "--help": return new("help", host, bind, port, startup, timeout, [], sessionFile);
                default: throw new ArgumentException($"Unknown runner argument: {args[i]}");
            }
        }
        if (port is < 1 or > 65535 || startup <= 0 || timeout <= 0 || jobs <= 0)
            throw new ArgumentException("Port must be 1..65535; deadlines and --jobs must be positive.");
        if (guests is < 1 or > 3) throw new ArgumentException("--guests must be 1..3; the playing host occupies the fourth city.");
        if (sessionFile is not null && command != "client") throw new ArgumentException("--session-file belongs to the independent client command; dev/tests isolate their own files.");
        if (scenario is not null)
        {
            string[] names = command == "test-network" ? ScenarioNames.Network : command is "test-ui" or "_ui-worker" ? ScenarioNames.Ui : [];
            if (!names.Contains(scenario)) throw new ArgumentException($"Unknown --scenario '{scenario}' for {command}. Available: {string.Join(", ", names)}.");
        }
        if (port is not null && command == "test-network" && scenario is not null && scenario != "authority-resume-victory")
            throw new ArgumentException("--port pins authority-resume-victory; select that scenario or omit --port.");
        if (args.Contains("--jobs") && command != "test-network") throw new ArgumentException("--jobs belongs to test-network.");
        if (engineArgs.Count > 0 && command is not ("dev" or "client" or "play")) throw new ArgumentException("--engine-arg belongs to desktop dev/client/play tasks.");
        if (command is "test-network" or "test-ui" or "_ui-worker" or "ci" or "ci-source" or "ci-linux-package" or "ci-windows" && (host != "127.0.0.1" || bind != "127.0.0.1"))
            throw new ArgumentException("Verification owns loopback peers; use dev/client/server for other endpoints.");
        if (releaseTag is not null)
        {
            Game.Distribution.ReleaseVersion release = Game.Distribution.ReleaseVersion.FromTag(releaseTag);
            production |= !release.IsPreview;
        }
        if (command is "package-linux" or "package-windows" or "verify-installed-linux" or "verify-installed-windows" or "release-preflight" or "assemble-release-assets" && releaseTag is null)
            throw new ArgumentException(command + " requires --tag vVERSION.");
        SteamPackaging.ValidateAppId(production, steamAppId);
        _ = DesktopExport.For(exportTarget);
        if (command == "test-steam" && (steamRole is not ("host" or "guest") || lobby == 0 || steamRole == "host" && lobby is not null))
            throw new ArgumentException("test-steam requires --role host or guest; --lobby ID belongs to guest. Two machines/accounts must run the paired command.");
        return new(command, host, bind, port, startup, timeout, engineArgs.ToArray(), sessionFile, jobs, scenario, evidence, workerToken,
            production, steamAppId, offline, exported, guests, steamRole, lobby, releaseTag, exportTarget, installedClient);
    }
}
