using System.Diagnostics;
using Xunit;
namespace DevRunner.Tests;

public sealed class AgentLauncherTests
{
    [Fact]
    public async Task InitializationOwnsOnlyEmptyHomeAndRejectsConflicts()
    {
        if (!OperatingSystem.IsLinux()) return;
        using var fixture = new PlanningFixture();
        string runtime = Path.Combine(fixture.Root, "runtime");
        fixture.Write("runtime/bin/fm-spawn.sh", "#!/bin/sh\n");
        fixture.Write("runtime/bin/fm-session-start.sh", "#!/bin/sh\n");
        Directory.CreateDirectory(Path.Combine(runtime, ".pi/extensions"));
        string home = Path.Combine(fixture.Root, "owned-home");
        var initialized = await Run(runtime, home, "--init");
        Assert.Equal(0, initialized.Code);
        Assert.Equal("herdr\n", File.ReadAllText(Path.Combine(home, "config/backend")));
        Assert.Contains("local-only", File.ReadAllText(Path.Combine(home, "data/projects.md")), StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(runtime, "config/backend")));
        var repeated = await Run(runtime, home, "--init");
        Assert.Equal(1, repeated.Code); Assert.Contains("refusing to overwrite", repeated.Error, StringComparison.Ordinal);
        File.WriteAllText(Path.Combine(home, "config/backend"), "tmux\n");
        var conflicting = await Run(runtime, home, "--check");
        Assert.Equal(1, conflicting.Code); Assert.Contains("Conflicting", conflicting.Error, StringComparison.Ordinal);
        Assert.Equal("tmux\n", File.ReadAllText(Path.Combine(home, "config/backend")));
    }

    [Fact]
    public async Task MissingSelectedRuntimeReturnsUnexecutedWithoutMutatingHome()
    {
        if (!OperatingSystem.IsLinux()) return;
        using var fixture = new PlanningFixture();
        string runtime = Path.Combine(fixture.Root, "runtime"), home = Path.Combine(fixture.Root, "owned-home");
        fixture.Write("runtime/bin/fm-spawn.sh", "#!/bin/sh\n"); fixture.Write("runtime/bin/fm-session-start.sh", "#!/bin/sh\n");
        Directory.CreateDirectory(Path.Combine(runtime, ".pi/extensions"));
        Assert.Equal(0, (await Run(runtime, home, "--init")).Code);
        var result = await Run(runtime, home, "--check", "/usr/bin:/bin");
        Assert.Equal(1, result.Code); Assert.Contains("UNEXECUTED", result.Error, StringComparison.Ordinal); Assert.Contains("pi", result.Error, StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(home, "state/pi-primary")));
    }

    [Fact]
    public async Task StartupPreservesSessionOwnershipAndAllowsApprovedDescendantCheckpoint()
    {
        if (!OperatingSystem.IsLinux()) return;
        using var fixture = new PlanningFixture();
        string runtime = Path.Combine(fixture.Root, "runtime"), home = Path.Combine(fixture.Root, "owned-home"), fakeBins = Path.Combine(fixture.Root, "fake-bins");
        fixture.Write("runtime/bin/fm-spawn.sh", "#!/bin/sh\n"); fixture.Write("runtime/bin/fm-session-start.sh", "#!/bin/sh\n");
        fixture.Write("runtime/bin/fm-project-mode.sh", "#!/bin/sh\nprintf 'local-only off\\n'\n");
        File.SetUnixFileMode(Path.Combine(runtime, "bin/fm-project-mode.sh"), UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        Directory.CreateDirectory(Path.Combine(runtime, ".pi/extensions"));
        foreach (string tool in new[] { "pi", "treehouse", "codex", "herdr", "gh", "node", "mise" })
        {
            string body = tool switch
            {
                "pi" => "printf '%s\\n' '--append-system-prompt --skill --session-dir'",
                "treehouse" => "printf '%s\\n' '--lease'",
                "herdr" => "printf 'Unexpected live control during ownership rejection\\n' >&2; exit 99",
                _ => "exit 0"
            };
            fixture.Write("fake-bins/" + tool, "#!/bin/sh\n" + body + "\n");
            File.SetUnixFileMode(Path.Combine(fakeBins, tool), UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
        // jq is not reached on these static/ownership paths, but must resolve.
        fixture.Write("fake-bins/jq", "#!/bin/sh\nexit 99\n"); File.SetUnixFileMode(Path.Combine(fakeBins, "jq"), UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        string checkpoint = fixture.Commit();
        Assert.Equal(0, (await Run(runtime, home, "--init")).Code);
        string project = Path.Combine(home, "projects/odot-game");
        PlanningEvidence.Git(fixture.Root, "clone", "-q", "--no-local", "--origin", "odot-source", fixture.Root, project);
        File.WriteAllText(Path.Combine(project, "Odot.slnx"), "<Solution />\n");
        PlanningEvidence.Git(project, "add", "Odot.slnx");
        PlanningEvidence.Git(project, "-c", "user.name=Fixture", "-c", "user.email=fixture@example.invalid", "commit", "-q", "-m", "approved descendant");
        string executablePath = fakeBins + ":/usr/bin:/bin";
        var check = await Run(runtime, home, "--check", executablePath, checkpoint);
        Assert.Equal(0, check.Code);
        var wrongSession = await Run(runtime, home, "start", executablePath, checkpoint, "foreign-session");
        Assert.Equal(1, wrongSession.Code); Assert.Contains("Inherited managed Herdr session differs", wrongSession.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("Unexpected live control", wrongSession.Error, StringComparison.Ordinal);
    }

    private static async Task<(int Code, string Error)> Run(string runtime, string home, string argument, string? executablePath = null, string? checkpoint = null, string? inheritedSession = null)
    {
        string root = Root();
        var start = new ProcessStartInfo("/bin/bash") { WorkingDirectory = root, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        start.ArgumentList.Add(Path.Combine(root, "tools/AgentWorkflow/agent-primary")); start.ArgumentList.Add(argument);
        start.Environment["ODOT_FIRSTMATE_ROOT"] = runtime; start.Environment["ODOT_FIRSTMATE_HOME"] = home;
        if (checkpoint is not null) start.Environment["ODOT_BOOTSTRAP_REVISION"] = checkpoint;
        if (inheritedSession is not null)
        {
            start.Environment["HERDR_ENV"] = "1"; start.Environment["HERDR_SESSION"] = inheritedSession;
            start.Environment["HERDR_PANE_ID"] = "w1:p1"; start.Environment["HERDR_TAB_ID"] = "w1:t1";
            start.Environment["HERDR_WORKSPACE_ID"] = "w1"; start.Environment["HERDR_SOCKET_PATH"] = "/unused-fixture.sock";
        }
        if (executablePath is not null) start.Environment["PATH"] = executablePath;
        using var process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync(), stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15)); await stdout;
        return (process.ExitCode, await stderr);
    }
    private static string Root()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Odot.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
