using System.Globalization;
using System.Text.Json;
using Game.Core;

namespace DevRunner;

internal sealed record SteamConnectionEvidence(string Transport, string Role, string MatchId, int ConnectionState, bool Authenticated, bool Relay, uint RelayPop);
internal sealed class SteamPrerequisiteException(string message) : VerificationPrerequisiteException(message);

internal sealed partial class Runner
{
    private static readonly JsonSerializerOptions SteamEvidenceJson = new() { PropertyNameCaseInsensitive = true };
    // Each side runs on its own machine/account. No mock or ENet substitution.
    // Overlay acceptance and genuine Steam cold launch remain separate manual checks.
    private async Task TestSteam()
    {
        if (!OperatingSystem.IsLinux() || System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture != System.Runtime.InteropServices.Architecture.X64
            || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
            throw new SteamPrerequisiteException("Steam paired verification unexecuted: requires Linux x86_64, a normal desktop and signed-in Steam. Run --role host and --role guest on two machines/accounts.");
        if (options.Exported)
        {
            await VerifySteamFiles();
            if (!File.Exists(Path.Combine(_root, "dist", "client", "odot.x86_64")))
                throw new SteamPrerequisiteException("Steam pair unexecuted: missing client export; run mise run export-client first.");
        }
        else await Prepare();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation); deadline.CancelAfter(options.Timeout);
        await _evidence.Measure("steam-pair-" + options.SteamRole, "external", async () =>
        {
            await using var owned = new ScenarioScope("steam-pair", _evidence);
            string? executable = null;
            if (options.Exported)
            {
                string package = Path.Combine(owned.Directory, "package");
                CopySteamPackage(Path.Combine(_root, "dist", "client"), package);
                executable = Path.Combine(package, "odot.x86_64");
            }
            var worker = new Runner(options, deadline.Token, _evidence, owned);
            await worker.SteamPairSide(executable, deadline.Token);
            await owned.DisposeAsync(); owned.CheckErrors();
        });
        Console.WriteLine("Paired native channel/gameplay slice passed. Compare both records' MatchId and paused state. Overlay invites, cold launch, resume/host loss and release acceptance require their own observed checks.");
    }

    private static void CopySteamPackage(string source, string targetDirectory)
    {
        foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            string target = Path.Combine(targetDirectory, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!); File.Copy(file, target);
            if (OperatingSystem.IsLinux()) File.SetUnixFileMode(target, File.GetUnixFileMode(file));
        }
    }

    private async Task SteamPairSide(string? executable, CancellationToken token)
    {
        Child client = StartGameRole("steam-" + options.SteamRole, "menu", false, 0, executable);
        await client.WaitFor(e => e.Type == "menu", "Steam application start screen", options.StartupTimeout, token);
        var startupEvents = new HashSet<GameEvent>(client.History(), ReferenceEqualityComparer.Instance);
        if (options.SteamRole == "host")
        {
            await client.Send("steam-host");
            GameEvent lobby = await client.WaitFor(e => !startupEvents.Contains(e) && e.Type is "steam-lobby" or "steam-unavailable", "Steam host availability/private lobby", options.StartupTimeout, token);
            if (lobby.Type == "steam-unavailable") throw new SteamPrerequisiteException("Steam pair unexecuted (prerequisite): " + lobby.Message);
            Console.WriteLine("Ask your friend to run mise run test-steam --role guest --lobby " + lobby.Message + (options.Exported ? " --exported" : "") + "; or use Invite friends and accept through Steam.");
        }
        else
        {
            Console.WriteLine("Join through a Steam invitation, or the explicitly supplied development lobby argument. No invitation is sent by the runner.");
            if (options.Lobby is { } lobby) await client.Send("steam-join " + lobby.ToString(CultureInfo.InvariantCulture));
        }
        await client.WaitFor(e => e.Type == "connected" || e.Type == "steam-unavailable" && !startupEvents.Contains(e), "authenticated native Steam admission", options.Timeout, token);
        if (!client.History().Any(e => e.Type == "connected")) throw new SteamPrerequisiteException("Steam pair unexecuted (prerequisite): " + client.History().Last().Message);
        MatchSnapshot joined = await Observe(client, s => s.Players.Count(p => p.Connected) >= 2, "two real Steam players admitted", token);
        if (options.SteamRole == "host") await Action(client, "start", token);
        else await Action(client, "start", token, false);
        await Observe(client, s => s.Phase == Phase.Building, "original host starts shared match", token);
        await Economy(client, token);
        await Observe(client, s => s.Players.Count(p => p.Slots[0].Type == Building.Farm && p.Slots[1].Type == Building.Barracks) >= 2, "both sides see ordinary purchases", token);
        for (int turn = 1; turn <= 3; turn++)
        {
            await Observe(client, s => s.Phase == Phase.Building && s.Turn == turn, "shared build turn " + turn, token);
            await RecruitAll(client, token);
            await Action(client, "ready", token);
            await Observe(client, s => s.Phase != Phase.Building || s.Turn > turn, "both ready advance turn " + turn, token);
        }
        MatchSnapshot combat = await Observe(client, s => s.Phase == Phase.Combat, "real shared combat", token);
        // Invalid ordinary requests still require channel-0 responses during snapshots.
        for (int request = 0; request < 6; request++) await Action(client, "unknown", token, false);
        if (options.SteamRole == "host") await Action(client, "pause", token);
        MatchSnapshot paused = await Observe(client, s => s.Paused, "original host pauses shared authority", token);
        Require(paused.MatchId == joined.MatchId && paused.Revision >= combat.Revision, "same native match progresses and pauses");
        GameEvent route = await client.WaitFor(e => e.Type == "steam-connection"
            && JsonSerializer.Deserialize<SteamConnectionEvidence>(e.Message!, SteamEvidenceJson) is { ConnectionState: 3, Authenticated: true } connection
            && connection.MatchId == paused.MatchId, "native authenticated connection diagnostics", options.StartupTimeout, token);
        var diagnostic = JsonSerializer.Deserialize<SteamConnectionEvidence>(route.Message!, SteamEvidenceJson)
            ?? throw new InvalidOperationException("Missing Steam native route evidence.");
        Require(diagnostic.Transport == "native-steam" && diagnostic.MatchId == paused.MatchId && diagnostic.Authenticated && diagnostic.ConnectionState == 3,
            "supported native peer reports authenticated Steam connection");
        await File.WriteAllTextAsync(Path.Combine(_scope!.EvidenceDirectory, "paired-state-" + options.SteamRole + ".json"), JsonSerializer.Serialize(new { Role = options.SteamRole, Match = paused, Connection = diagnostic }, Evidence.JsonOptions), token);
        Console.WriteLine(diagnostic.Relay ? "Observed Steam relay POP: " + diagnostic.RelayPop : "Native direct route observed; relay acceptance remains unexecuted.");
        // Leave the paused application available for the friend's observation/manual checks.
        Console.WriteLine("Paused checkpoint saved. Continue manual reconnect/host-end checks, then close the window or Ctrl-C. The overall command is bounded by --timeout-ms.");
        Require(await client.WaitExit(token) == 0, "Steam side exits cleanly");
    }
}
