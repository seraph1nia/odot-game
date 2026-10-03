using System.Globalization;
namespace DevRunner;

internal sealed record Options(string Command, string Host, string Bind, int? Port, int StartupTimeout, int Timeout, string[] EngineArgs, string? SessionFile,
    int Jobs = 2, string? Scenario = null, string? EvidenceDirectory = null, string? WorkerToken = null,
    bool Production = false, uint? SteamAppId = null, bool Offline = false, bool Exported = false, int Guests = 1,
    string? SteamRole = null, ulong? Lobby = null, string? ReleaseTag = null, string ExportTarget = "linux-x64", string? InstalledClient = null,
    string? UiCheckpoint = null, int SimulationSpeed = 4, bool Trace = false, int UiJobs = 2,
    int ProfileIterations = 3, int ProfileFrames = 600, string ProfileConfiguration = "Debug", bool ProfileWorkCounters = false)
{
    public static Options Parse(string[] args)
    {
        string command = args.FirstOrDefault() ?? "help";
        string host = "127.0.0.1";
        string bind = "127.0.0.1";
        int? port = null;
        int startup = 15000;
        int timeout = 180000;
        bool explicitTimeout = false;
        string? sessionFile = null;
        int jobs = 2, simulationSpeed = 4, uiJobs = 2;
        bool trace = false;
        int profileIterations = 3, profileFrames = 600;
        string profileConfiguration = "Debug";
        bool profileWorkCounters = false;
        int guests = 1;
        string? scenario = null, evidence = null, workerToken = null;
        bool production = false, offline = false, exported = false;
        uint? steamAppId = null;
        string? steamRole = null;
        ulong? lobby = null;
        string? releaseTag = null;
        string exportTarget = command is "ci-windows" or "package-windows" ? "windows-x64" : "linux-x64";
        string? installedClient = null;
        string? checkpoint = null;
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
                case "--timeout-ms": timeout = int.Parse(Value(), CultureInfo.InvariantCulture); explicitTimeout = true; break;
                case "--session-file": sessionFile = Value(); break;
                case "--engine-arg": engineArgs.Add(Value()); break;
                case "--simulation-speed": simulationSpeed = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--trace": trace = true; break;
                case "--ui-jobs": uiJobs = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--jobs": jobs = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--guests" when command == "dev": guests = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--scenario": scenario = Value(); break;
                case "--iterations" when command == "profile-presentation": profileIterations = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--frames" when command is "profile-presentation" or "_ui-worker": profileFrames = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--configuration" when command is "profile-presentation" or "_ui-worker": profileConfiguration = Value(); break;
                case "--work-counters" when command is "profile-presentation" or "_ui-worker": profileWorkCounters = true; break;
                case "--checkpoint" when command is "test-ui" or "_ui-worker": checkpoint = Value(); break;
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
        // Full source UI is serial. The measured 184s campaign and 156s economy
        // slices need separate bounded headroom within the complete CI budget.
        if (!explicitTimeout && command is "ci" or "ci-source") timeout = 900000;
        else if (!explicitTimeout && command == "profile-presentation") timeout = 600000;
        else if (!explicitTimeout && command is "test-ui" or "_ui-worker" && scenario is null) timeout = 600000;
        else if (!explicitTimeout && (command is "test-network" or "ci-linux-package" || command is "test-ui" or "_ui-worker" && scenario is "economy" or "exported-package")) timeout = 300000;
        if (port is < 1 or > 65535 || startup <= 0 || timeout <= 0 || jobs <= 0)
            throw new ArgumentException("Port must be 1..65535; deadlines and --jobs must be positive.");
        if (guests is < 1 or > 3) throw new ArgumentException("--guests must be 1..3; the playing host occupies the fourth city.");
        if (sessionFile is not null && command != "client") throw new ArgumentException("--session-file belongs to the independent client command; dev/tests isolate their own files.");
        if (scenario is not null)
        {
            string[] names = command == "test-network" ? ScenarioNames.Network : command == "test-ui" ? ScenarioNames.Ui : command == "_ui-worker" ? [.. ScenarioNames.Ui, "combat-playback"] : command == "profile-presentation" ? ["combat-playback"] : command == "test-steam" ? ["direct-invite"] : [];
            if (!names.Contains(scenario)) throw new ArgumentException($"Unknown --scenario '{scenario}' for {command}. Available: {string.Join(", ", names)}.");
        }
        if (checkpoint is not null && (scenario != "combat" || checkpoint != "melee"))
            throw new ArgumentException("--checkpoint melee belongs to --scenario combat.");
        if (port is not null && command == "test-network" && scenario is not null && scenario != "authority-resume-victory")
            throw new ArgumentException("--port pins authority-resume-victory; select that scenario or omit --port.");
        if (args.Contains("--jobs") && command is not ("test-network" or "test-ui" or "ci" or "ci-source")) throw new ArgumentException("--jobs belongs to network/UI/source verification.");
        if (command == "test-ui" && jobs > 2 || uiJobs is < 1 or > 2) throw new ArgumentException("Graphical workers must be 1..2.");
        if (args.Contains("--ui-jobs") && command is not ("ci" or "ci-source")) throw new ArgumentException("--ui-jobs belongs to CI source verification.");
        if (profileIterations is < 1 or > 10 || profileFrames != 600 || profileConfiguration != "Debug")
            throw new ArgumentException("Presentation profiles require 1..10 iterations, exactly 600 frames and the locked Debug build.");
        if (command == "profile-presentation" && scenario != "combat-playback") throw new ArgumentException("Select --scenario combat-playback.");
        if (simulationSpeed is < 1 or > 8) throw new ArgumentException("--simulation-speed must be 1..8.");
        if ((args.Contains("--simulation-speed") || trace) && command is not ("test-network" or "test-ui" or "_ui-worker" or "ci" or "ci-source" or "ci-linux-package"))
            throw new ArgumentException("Simulation pacing/trace belongs to owned verification.");
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
        if (command == "test-steam" && scenario == "direct-invite" && lobby is not null)
            throw new ArgumentException("direct-invite requires accepting an actual invitation; omit --lobby.");
        return new(command, host, bind, port, startup, timeout, engineArgs.ToArray(), sessionFile, jobs, scenario, evidence, workerToken,
            production, steamAppId, offline, exported, guests, steamRole, lobby, releaseTag, exportTarget, installedClient, checkpoint, simulationSpeed, trace, uiJobs,
            profileIterations, profileFrames, profileConfiguration, profileWorkCounters);
    }
}
