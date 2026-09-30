using System.Formats.Tar;
using Game.Distribution;

namespace DevRunner;

internal sealed partial class Runner
{
    private async Task TaggedExport()
    {
        ReleaseVersion version = ReleaseVersion.FromTag(options.ReleaseTag);
        string commit = await Capture("git", "rev-parse", "--verify", "refs/tags/" + version.Tag + "^{commit}");
        string head = await Capture("git", "rev-parse", "HEAD");
        if (commit != head) throw new InvalidOperationException("Check out the requested tag before building its release.");
        string dirty = await Capture("git", "status", "--porcelain", "--untracked-files=all");
        if (dirty.Length != 0) throw new InvalidOperationException("Tagged exports require a clean checkout; source must agree with its tag.");
        DesktopExport layout = DesktopExport.For(options.ExportTarget);
        if (options.ExportTarget == "windows-x64" && !OperatingSystem.IsWindows())
            throw new VerificationPrerequisiteException("Tagged Windows export requires native Windows for its packaged identity probe. Untagged --target windows-x64 permits cross-export inventory checks.");
        ReleaseIdentity identity = ReleaseIdentity.Create(version.Tag, commit, options.ExportTarget, options.Production, options.SteamAppId);
        string output = Path.Combine(_root, "dist", "releases", version.Value, identity.Target);
        if (Directory.Exists(output)) throw new InvalidOperationException($"Release output already exists: {output}. Remove the local output explicitly before rebuilding.");
        string staging = Path.Combine(_root, ".cache", "release-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        try
        {
            string archive = Path.Combine(staging, "source.tar");
            await Execute("release-source", "git", "archive", "--format=tar", "--output=" + archive, commit);
            string source = Path.Combine(staging, "source");
            Directory.CreateDirectory(source);
            TarFile.ExtractToDirectory(archive, source, overwriteFiles: false);
            ReleaseStaging.Stamp(source, identity);
            var worker = new Runner(options, cancellation, _evidence, root: source);
            await worker.Prepare();
            await worker.PrepareTemplates();
            await worker.Export(false);
            string payload = Path.Combine(source, "dist", layout.Kind(options.Production));
            await File.WriteAllTextAsync(Path.Combine(payload, "build-info.json"), identity.ToJson() + "\n", cancellation);
            await worker.BuildIdentityProbe(identity, Path.Combine(payload, layout.Executable));
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            Directory.Move(payload, output);
            Console.WriteLine($"Tagged {identity.Channel} client {identity.Version} ({identity.SourceCommit}) exported to {output}. Nothing uploaded or published; full release gates remain separate.");
        }
        finally { if (Directory.Exists(staging)) Directory.Delete(staging, true); }
    }

    private async Task BuildIdentityProbe(ReleaseIdentity? expected, string? exported = null)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(options.StartupTimeout);
        await using var owned = new ScenarioScope("build-identity", _evidence);
        var args = new List<string> { "--headless", "--log-file", Path.Combine(owned.EvidenceDirectory, "engine.log") };
        if (exported is null) args.AddRange(["--path", GameDirectory]);
        args.AddRange(["--", "--build-info-probe"]);
        await using var child = owned.Own(new Child("build-identity", exported ?? "godot", args, _root,
            game: true, quiet: true, workingDirectory: exported is null ? _root : Path.GetDirectoryName(exported),
            environment: owned.EnvironmentFor("probe"), evidenceDirectory: owned.EvidenceDirectory));
        var observation = await child.WaitFor(e => e.Type == "build-info", "packaged application/assembly identity", options.StartupTimeout, deadline.Token);
        if (expected is null) Require(observation.Message == "Development build", "untagged source retains development identity");
        else Require(ReleaseIdentity.FromJson(observation.Message!) == expected, "packaged source commit and version agree with release metadata");
        Require(await child.WaitExit(deadline.Token) == 0, "build identity probe exits successfully");
        await owned.DisposeAsync(); owned.CheckErrors();
    }
}
