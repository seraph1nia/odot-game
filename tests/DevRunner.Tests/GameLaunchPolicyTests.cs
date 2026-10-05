using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class GameLaunchPolicyTests
{
    public static IEnumerable<object[]> GraphicalRoles()
    {
        foreach (string role in new[] { "server", "playing-host", "client", "menu", "solo" })
            foreach (bool headless in new[] { false, true })
                foreach (bool exported in new[] { false, true })
                    yield return [role, headless, exported];
    }

    [Theory]
    [MemberData(nameof(GraphicalRoles))]
    public async Task ActualGraphicalArgvAlignsEveryEnetPeerButNotLocalRoles(string role, bool headless, bool exported)
    {
        string root = RepositoryRoot();
        var evidence = new Evidence(root, Path.Combine(root, "logs", "launch-policy-" + Guid.NewGuid().ToString("N")));
        await using var scope = new ScenarioScope("graphical", evidence, graphical: true, runtimeRoot: evidence.Directory);
        var runner = new Runner(Options.Parse(["_ui-worker", "--scenario", "economy", "--startup-timeout-ms", "60000"]), CancellationToken.None, evidence, scope, root);
        // This is the argv builder called by StartGameRole, not a source-text proxy.
        var args = runner.GameArguments("ui-peer", role, headless, 7000, exported ? "packed-game" : null);
        Assert.Equal(role is "server" or "playing-host" or "client" ? ["60000"] : Array.Empty<string>(), Values(args, "--connect-timeout-ms"));
        Assert.Equal(!exported, args.Contains("--path"));
        Assert.Equal(headless, args.Contains("--headless"));
        Assert.Equal(role == "client", args.Contains("--session-file"));
        Assert.Equal(role != "menu", args.Contains("--" + role));
        Assert.Single(args, a => a == "--supervised");
        Assert.Contains("--", args);
        if (role is "server" or "playing-host" or "client")
        {
            // Reconnect/restart and explicit-short-timeout peers use the same boundary.
            var explicitArgs = runner.GameArguments("ui-peer", role, headless, 7000, exported ? "packed-game" : null,
                "--connect-timeout-ms", "1000");
            Assert.Equal(["1000"], Values(explicitArgs, "--connect-timeout-ms"));
            Assert.Equal(args, runner.GameArguments("ui-peer", role, headless, 7000, exported ? "packed-game" : null));
        }
    }

    [Theory]
    [InlineData("dev", "playing-host", false, true)]
    [InlineData("dev", "client", false, true)]
    [InlineData("client", "client", false, false)]
    [InlineData("server", "server", true, false)]
    [InlineData("play", "menu", false, false)]
    [InlineData("test-network", "server", true, true)]
    [InlineData("test-network", "client", true, true)]
    [InlineData("test-native-pump", "menu", true, true)]
    [InlineData("test-steam", "menu", false, true)]
    public async Task OrdinaryDevSteamNetworkAndNativeArgvRetainsPublicDefault(string command, string role, bool headless, bool owned)
    {
        string root = RepositoryRoot();
        var evidence = new Evidence(root, Path.Combine(root, "logs", "launch-policy-" + Guid.NewGuid().ToString("N")));
        await using var scope = new ScenarioScope("ordinary", evidence, runtimeRoot: evidence.Directory);
        string[] input = command == "test-steam" ? [command, "--role", "host", "--startup-timeout-ms", "60000"] : [command, "--startup-timeout-ms", "60000"];
        var runner = new Runner(Options.Parse(input), CancellationToken.None, evidence, owned ? scope : null, root);
        Assert.Empty(Values(runner.GameArguments("peer", role, headless, 7000), "--connect-timeout-ms"));
        Assert.Equal(["1000"], Values(runner.GameArguments("peer", role, headless, 7000, extra: ["--connect-timeout-ms", "1000"]), "--connect-timeout-ms"));
    }

    [Fact]
    public async Task GraphicalPolicyUsesTheDeclaredAllowanceNotANewConstant()
    {
        string root = RepositoryRoot();
        var evidence = new Evidence(root, Path.Combine(root, "logs", "launch-policy-" + Guid.NewGuid().ToString("N")));
        await using var scope = new ScenarioScope("graphical", evidence, graphical: true, runtimeRoot: evidence.Directory);
        var runner = new Runner(Options.Parse(["_ui-worker", "--scenario", "reconnect"]), CancellationToken.None, evidence, scope, root);
        Assert.Equal(["15000"], Values(runner.GameArguments("observer", "client", true, 7000), "--connect-timeout-ms"));
    }

    private static string[] Values(List<string> args, string option) => args.Select((value, index) => (value, index)).Where(p => p.value == option).Select(p => args[p.index + 1]).ToArray();
    private static string RepositoryRoot()
    {
        var path = new DirectoryInfo(Environment.CurrentDirectory);
        while (path is not null && !File.Exists(Path.Combine(path.FullName, "Odot.slnx"))) path = path.Parent;
        return path?.FullName ?? throw new InvalidOperationException("Missing repository root.");
    }
}
