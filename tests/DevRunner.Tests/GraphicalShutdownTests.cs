using DevRunner;
using Xunit;

namespace DevRunner.Tests;

public sealed class GraphicalShutdownTests
{
    [Fact]
    public async Task CompletedPhaseRetiresAllPeersBeforeIndependentPhaseWithoutLosingOwnership()
    {
        string root = Path.Combine(Environment.CurrentDirectory, "logs", "shutdown-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var scope = new ScenarioScope("graphical", new Evidence(root), graphical: true, runtimeRoot: root);
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            Child Peer(string name, string onQuit = "") => scope.Own(new Child(name, "/bin/sh",
                ["-c", "printf 'ODOT_EVENT {\"Type\":\"ready\"}\\n'; read -r command; [ \"$command\" = quit ] || exit 1; " + onQuit + "exit 0"],
                root, game: true, evidenceDirectory: scope.EvidenceDirectory));
            string clientReceipt = Path.Combine(scope.Directory, "client-exited");
            string observerReceipt = Path.Combine(scope.Directory, "observer-exited");
            Child server = Peer("server", $"[ -f '{clientReceipt}' ] && [ -f '{observerReceipt}' ] || exit 9; ");
            Child client = Peer("client", $"touch '{clientReceipt}'; ");
            Child observer = Peer("observer", $"touch '{observerReceipt}'; ");
            foreach (Child peer in scope.Children) await peer.WaitFor(e => e.Type == "ready", "phase readiness", 2000, deadline.Token);

            // Matched legacy control: retiring only the client leaves both unused peers alive.
            await client.QuitGame(deadline.Token);
            await client.DisposeAsync();
            Assert.False(observer.HasExited);
            Assert.False(server.HasExited);

            await scope.CompleteGames(deadline.Token);
            Assert.All(scope.Children, peer =>
            {
                Assert.True(peer.HasExited);
                Assert.Equal(0, peer.ExitCode);
                Assert.True(File.Exists(peer.LogPath));
                Assert.True(File.Exists(peer.LogPath + ".states.json"));
            });
            Assert.Equal(3, scope.Children.Count);
            Assert.True(Directory.Exists(scope.Directory));
            await scope.CompleteGames(deadline.Token); // Idempotent, without deleting runtime or evidence.

            Child independent = Peer("independent");
            await independent.WaitFor(e => e.Type == "ready", "independent phase readiness", 2000, deadline.Token);
            Assert.False(independent.HasExited);
            Assert.All(scope.Children.Take(3), peer => Assert.True(peer.HasExited));
            await scope.QuitGames(deadline.Token);
            await scope.DisposeAsync();
            await scope.DisposeAsync();
            scope.CheckErrors();
            Assert.Equal(0, independent.ExitCode);
            Assert.False(Directory.Exists(scope.Directory));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task SuccessfulGraphicalShutdownDoesNotKillSlowCleanExit()
    {
        string root = Path.Combine(Environment.CurrentDirectory, "logs", "shutdown-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var scope = new ScenarioScope("graphical", new Evidence(root), graphical: true, runtimeRoot: root);
            Child child = scope.Own(new Child("slow-clean-exit", "/bin/sh",
                ["-c", "printf 'ODOT_EVENT {\"Type\":\"ready\"}\\n'; read -r command; [ \"$command\" = quit ] || exit 1; sleep 3; exit 0"],
                root, game: true, evidenceDirectory: scope.EvidenceDirectory));
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await child.WaitFor(e => e.Type == "ready", "fixture readiness", 2000, deadline.Token);
            await scope.CompleteGames(deadline.Token);
            await scope.DisposeAsync();
            scope.CheckErrors();
            Assert.Equal(0, child.ExitCode);
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(7, false)]
    [InlineData(0, true)]
    public async Task OrderlyShutdownDoesNotWaiveExitFailuresOrEngineErrors(int exitCode, bool engineError)
    {
        string root = Path.Combine(Environment.CurrentDirectory, "logs", "shutdown-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var scope = new ScenarioScope("graphical", new Evidence(root), graphical: true, runtimeRoot: root);
            string script = "printf 'ODOT_EVENT {\"Type\":\"ready\"}\\n'; read -r command; "
                + (engineError ? "echo 'ERROR: fixture engine error' >&2; " : "") + "exit " + exitCode;
            Child child = scope.Own(new Child("failed-exit", "/bin/sh", ["-c", script], root, game: true, evidenceDirectory: scope.EvidenceDirectory));
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await child.WaitFor(e => e.Type == "ready", "fixture readiness", 2000, deadline.Token);
            await Assert.ThrowsAsync<InvalidOperationException>(() => scope.CompleteGames(deadline.Token));
            // Retired children remain checked, but are not sent quit again.
            await Assert.ThrowsAsync<InvalidOperationException>(() => scope.CompleteGames(deadline.Token));
            await scope.DisposeAsync();
            Assert.Equal(exitCode, child.ExitCode);
            Assert.Throws<InvalidOperationException>(scope.CheckErrors);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task StalledShutdownStillHonorsScenarioCancellationAndEmergencyCleanup()
    {
        string root = Path.Combine(Environment.CurrentDirectory, "logs", "shutdown-fixture-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await using var scope = new ScenarioScope("graphical", new Evidence(root), graphical: true, runtimeRoot: root);
            Child child = scope.Own(new Child("stalled-exit", "/bin/sh",
                ["-c", "printf 'ODOT_EVENT {\"Type\":\"ready\"}\\n'; read -r command; sleep 30"],
                root, game: true, evidenceDirectory: scope.EvidenceDirectory));
            await child.WaitFor(e => e.Type == "ready", "fixture readiness", 2000, CancellationToken.None);
            using var deadline = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => scope.CompleteGames(deadline.Token));
            await scope.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(child.HasExited);
            Assert.False(Directory.Exists(scope.Directory));
            Assert.NotEqual(0, child.ExitCode);
            Assert.Throws<InvalidOperationException>(scope.CheckErrors);
        }
        finally { Directory.Delete(root, true); }
    }
}
