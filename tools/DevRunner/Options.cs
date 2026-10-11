using System.Globalization;
namespace DevRunner;

internal sealed record Options(string Command, string Host, string Bind, int? Port, int StartupTimeout, int Timeout, string[] EngineArgs, string? SessionFile,
    int Jobs = 2, string? Scenario = null, string? EvidenceDirectory = null, string? WorkerToken = null,
    bool Production = false, uint? SteamAppId = null, bool Offline = false, bool Exported = false, int Guests = 1,
    string? SteamRole = null, ulong? Lobby = null, string? ReleaseTag = null, string ExportTarget = "linux-x64", string? InstalledClient = null,
    string? UiCheckpoint = null, int SimulationSpeed = 4, bool Trace = false, int UiJobs = 2,
    int ProfileIterations = 3, int ProfileFrames = 600, string ProfileConfiguration = "Debug", bool ProfileWorkCounters = false, string? ProfileBaseline = null, string? PixelOwnershipRequest = null, string? BoundaryEvidence = null, bool ExplicitTimeout = false, string? Renderer = null, string? Filter = null)
{
    public const int ArmyUiTimeout = 420000;

    // Hosted two-display economy reached army battle two at 900s. Keep its
    // complete paid paths bounded without extending sibling or explicit budgets.
    public int UiTimeout(string scenario) => !ExplicitTimeout && scenario == "economy" && UiCheckpoint is not (null or "smoke")
        ? UiCheckpoint == "army" ? ArmyUiTimeout : 1080000
        : Timeout;

    // Used only by the owned renderer-preference bootstrap witness; not a CI option.
    internal bool UseSavedRenderer { get; init; }
    // Explicit optional authority; never a fallback for mandatory private CI.
    internal bool DesktopSurface { get; init; }
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
        bool trace = false, desktopSurface = false;
        int profileIterations = 3, profileFrames = 600;
        string profileConfiguration = "Debug";
        string? profileBaseline = null, pixelOwnershipRequest = null, boundaryEvidence = null;
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
        string? checkpoint = null, renderer = null, filter = null;
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
                case "--renderer": renderer = Value(); break;
                case "--simulation-speed": simulationSpeed = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--trace": trace = true; break;
                case "--ui-jobs": uiJobs = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--jobs": jobs = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--guests" when command == "dev": guests = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--scenario": scenario = Value(); break;
                case "--filter" when command is "test-rules" or "test-fast" or "test-core" or "test-runner" or "test-campaign": filter = Value(); break;
                case "--iterations" when command == "profile-presentation": profileIterations = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--frames" when command is "profile-presentation" or "_ui-worker": profileFrames = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--configuration" when command is "profile-presentation" or "_ui-worker": profileConfiguration = Value(); break;
                case "--work-counters" when command is "profile-presentation" or "_ui-worker": profileWorkCounters = true; break;
                case "--baseline" when command is "profile-presentation" or "_ui-worker": profileBaseline = Path.GetFullPath(Value()); break;
                case "--pixel-ownership-request" when command is "profile-presentation" or "_ui-worker": pixelOwnershipRequest = Path.GetFullPath(Value()); break;
                case "--boundary-evidence" when command is "profile-presentation" or "_ui-worker": boundaryEvidence = Path.GetFullPath(Value()); break;
                case "--ui-checkpoint" when command is "test-ui" or "test-atmosphere" or "test-vulkan" or "_ui-worker":
                case "--checkpoint" when command is "test-ui" or "test-atmosphere" or "test-vulkan" or "_ui-worker": checkpoint = Value(); break;
                case "--evidence-directory": evidence = Path.GetFullPath(Value()); break;
                case "--worker-token" when command == "_ui-worker": workerToken = Value(); break;
                case "--production" when command is "export-client" or "package-linux" or "package-windows" or "verify-installed-linux" or "verify-installed-windows" or "release-preflight" or "assemble-release-assets": production = true; break;
                case "--steam-app-id" when command is "export-client" or "package-linux" or "package-windows" or "verify-installed-linux" or "verify-installed-windows" or "release-preflight" or "assemble-release-assets": steamAppId = uint.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--tag" when command is "export-client" or "package-linux" or "package-windows" or "verify-installed-linux" or "verify-installed-windows" or "release-preflight" or "assemble-release-assets": releaseTag = Value(); break;
                case "--installed-client" when command == "_ui-worker": installedClient = Value(); break;
                case "--target" when command is "export-client" or "check-steam-extension" or "release-preflight" or "play" or "client": exportTarget = Value(); break;
                case "--offline" when command == "check-steam-extension": offline = true; break;
                case "--desktop" when command == "test-vulkan": desktopSurface = true; break;
                case "--exported" when command is "check-steam-extension" or "test-steam" or "test-vulkan" or "test-atmosphere" or "test-assets" or "_ui-worker" or "play" or "client": exported = true; break;
                case "--role" when command == "test-steam": steamRole = Value(); break;
                case "--lobby" when command == "test-steam": lobby = ulong.Parse(Value(), NumberStyles.None, CultureInfo.InvariantCulture); break;
                case "--help": return new("help", host, bind, port, startup, timeout, [], sessionFile);
                default: throw new ArgumentException($"Unknown runner argument: {args[i]}");
            }
        }
        // CI's ordinary worker budget; economy's measured allowance is resolved
        // separately at the display boundary, not applied to every source gate.
        if (!explicitTimeout && command is "ci" or "ci-source" or "test-deep") timeout = 300000;
        else if (!explicitTimeout && command == "profile-presentation") timeout = 600000;
        else if (!explicitTimeout && command is "test-ui" or "_ui-worker" && scenario is null) timeout = 600000;
        // Research's 120s setup + 60s controls need startup/cleanup headroom.
        // Full standalone combat also includes the measured ~305s hosted path.
        else if (!explicitTimeout && command is "test-ui" or "_ui-worker" && scenario == "combat" && checkpoint is null or "research")
            timeout = checkpoint == "research" ? 210000 : 420000;
        else if (!explicitTimeout && (command is "test-network" or "ci-linux-package" || command is "test-ui" or "_ui-worker" && scenario == "exported-package")) timeout = 300000;
        if (port is < 1 or > 65535 || startup <= 0 || timeout <= 0 || jobs <= 0)
            throw new ArgumentException("Port must be 1..65535; deadlines and --jobs must be positive.");
        if (guests is < 1 or > 3) throw new ArgumentException("--guests must be 1..3; the playing host occupies the fourth city.");
        if (sessionFile is not null && command != "client") throw new ArgumentException("--session-file belongs to the independent client command; dev/tests isolate their own files.");
        if (scenario is not null)
        {
            string[] names = command == "test-network" ? ScenarioNames.Network : command == "test-ui" ? ScenarioNames.Ui : command == "_ui-worker" ? [.. ScenarioNames.Ui, "combat-playback", "authored-scale", "atmosphere"] : command == "profile-presentation" ? ["combat-playback", "authored-scale"] : command == "test-steam" ? ["direct-invite"] : command == "test-native-pump" ? ["peer-liveness"] : command == "test-engine" ? ["settings", "animation", "assets"] : [];
            if (!names.Contains(scenario)) throw new ArgumentException($"Unknown --scenario '{scenario}' for {command}. Available: {string.Join(", ", names)}.");
        }
        if (checkpoint is not null && !(checkpoint is "deep" or "smoke" && scenario is "economy" or "launcher" or "exported-package" || scenario == "combat" && checkpoint is "melee" or "research" or "admission" || scenario == "economy" && checkpoint is "army" or "travel-diagnostic" or "travel" || scenario == "launcher" && checkpoint == "assets" || scenario == "settings" && checkpoint is "display" or "graphics" || checkpoint == "return-diagnostic" && (command == "test-atmosphere" || command == "_ui-worker" && scenario == "atmosphere") || checkpoint is "startup-diagnostic" or "popup-diagnostic" or "vanilla-popup" or "vanilla-native" && command == "test-vulkan" && desktopSurface))
            throw new ArgumentException("--checkpoint melee|research|admission belongs to --scenario combat; army|travel|travel-diagnostic belongs to --scenario economy; assets belongs to --scenario launcher; display|graphics belongs to --scenario settings; return-diagnostic belongs to test-atmosphere.");
        if (port is not null && command == "test-network" && scenario is not null && scenario != "authority-resume-victory")
            throw new ArgumentException("--port pins authority-resume-victory; select that scenario or omit --port.");
        if (args.Contains("--jobs") && command is not ("test-network" or "test-ui" or "ci" or "ci-source" or "test-rules" or "test-fast" or "test-core" or "test-runner" or "test-campaign" or "test-deep")) throw new ArgumentException("--jobs belongs to verification commands.");
        if (command == "test-ui" && jobs > 2 || uiJobs is < 1 or > 2) throw new ArgumentException("Graphical workers must be 1..2.");
        if (args.Contains("--ui-jobs") && command is not ("ci" or "ci-source")) throw new ArgumentException("--ui-jobs belongs to CI source verification.");
        if (profileIterations is < 1 or > 10 || profileFrames != 600 || profileConfiguration != "Debug")
            throw new ArgumentException("Presentation profiles require 1..10 iterations, exactly 600 frames and the locked Debug build.");
        if (command == "profile-presentation" && scenario is not ("combat-playback" or "authored-scale")) throw new ArgumentException("Select --scenario combat-playback|authored-scale.");
        if (profileBaseline is not null && (scenario != "authored-scale" || !Directory.Exists(profileBaseline))) throw new ArgumentException("--baseline requires authored-scale and an existing measured capture directory.");
        if (pixelOwnershipRequest is not null && (scenario != "authored-scale" || profileBaseline is not null || !File.Exists(pixelOwnershipRequest))) throw new ArgumentException("Pixel ownership requires authored-scale, one retained request and no comparison/acceptance mode.");
        if (boundaryEvidence is not null && (profileBaseline is null || pixelOwnershipRequest is not null || !File.Exists(boundaryEvidence)))
            throw new ArgumentException("--boundary-evidence requires the original measured --baseline and no ownership reconstruction mode.");
        if (simulationSpeed is < 1 or > 8) throw new ArgumentException("--simulation-speed must be 1..8.");
        if ((args.Contains("--simulation-speed") || trace) && command is not ("test-network" or "test-ui" or "_ui-worker" or "ci" or "ci-source" or "ci-linux-package"))
            throw new ArgumentException("Simulation pacing/trace belongs to owned verification.");
        if (renderer is not null && (renderer is not ("forward-plus" or "compatibility") || command is not ("dev" or "client" or "play" or "_ui-worker")))
            throw new ArgumentException("--renderer forward-plus|compatibility belongs to graphical desktop launches; ordinary CI always owns Compatibility.");
        if (renderer is not null && engineArgs.Any(a => a is "--rendering-method" or "--rendering-driver")) throw new ArgumentException("Use --renderer or raw engine renderer flags, not both.");
        if (engineArgs.Count > 0 && command is not ("dev" or "client" or "play")) throw new ArgumentException("--engine-arg belongs to desktop dev/client/play tasks.");
        if (command is "test-network" or "test-deep" or "test-ui" or "_ui-worker" or "ci" or "ci-source" or "ci-linux-package" or "ci-windows" && (host != "127.0.0.1" || bind != "127.0.0.1"))
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
            profileIterations, profileFrames, profileConfiguration, profileWorkCounters, profileBaseline, pixelOwnershipRequest, boundaryEvidence, explicitTimeout, renderer, filter)
        { DesktopSurface = desktopSurface };
    }
}
