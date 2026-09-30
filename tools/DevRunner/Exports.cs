using Game.Core;
using System.IO.Compression;
using System.Security.Cryptography;

namespace DevRunner;

internal sealed partial class Runner
{
    private const string TemplateChecksum = "92f8681e349ef1f90891b792da95e3b2b0bd1ed610b78018c58feb2d87e15a9d";
    private const string TemplateUrl = "https://github.com/godotengine/godot/releases/download/4.7.2-stable/Godot_v4.7.2-stable_mono_export_templates.tpz";

    private static string TemplateDirectory()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string data = OperatingSystem.IsWindows() ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            : OperatingSystem.IsMacOS() ? Path.Combine(home, "Library", "Application Support")
            : Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(home, ".local", "share");
        return Path.Combine(data, OperatingSystem.IsLinux() ? "godot" : "Godot",
            "export_templates", EngineVersion + ".stable.mono");
    }

    private async Task PrepareTemplates()
    {
        await Preflight();
        string directory = TemplateDirectory();
        string marker = Path.Combine(directory, ".odot-checksum");
        if (File.Exists(marker) && (await File.ReadAllTextAsync(marker, cancellation)).Trim() == TemplateChecksum
            && File.Exists(Path.Combine(directory, "linux_release.x86_64")) && File.Exists(Path.Combine(directory, "version.txt")))
        {
            string installedVersion = (await File.ReadAllTextAsync(Path.Combine(directory, "version.txt"), cancellation)).Trim();
            if (installedVersion != EngineVersion + ".stable.mono")
                throw new InvalidOperationException($"Installed template version mismatch: {installedVersion}; remove {directory} and prepare again.");
            Console.WriteLine("Matching checksum-verified .NET export templates are ready.");
            return;
        }
        string cache = Path.Combine(_root, ".cache");
        Directory.CreateDirectory(cache);
        string archive = Path.Combine(cache, $"godot-{EngineVersion}-mono-templates.tpz");
        if (!File.Exists(archive))
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            await using var input = await http.GetStreamAsync(TemplateUrl, cancellation);
            string partial = archive + ".partial";
            try
            {
                await using (var output = File.Create(partial)) await input.CopyToAsync(output, cancellation);
                File.Move(partial, archive, true);
            }
            finally { if (File.Exists(partial)) File.Delete(partial); }
        }
        await using (var stream = File.OpenRead(archive))
        {
            string checksum = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellation)).ToLowerInvariant();
            if (checksum != TemplateChecksum)
                throw new InvalidOperationException($"Export-template checksum mismatch. Remove {archive} and retry preparation.");
        }
        string staging = Path.Combine(cache, "templates-" + Guid.NewGuid().ToString("N"));
        try
        {
            ZipFile.ExtractToDirectory(archive, staging);
            string source = Path.Combine(staging, "templates");
            string version = (await File.ReadAllTextAsync(Path.Combine(source, "version.txt"), cancellation)).Trim();
            if (version != EngineVersion + ".stable.mono")
                throw new InvalidOperationException($"Template version mismatch: {version}.");
            Directory.CreateDirectory(directory);
            foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                string target = Path.Combine(directory, Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, true);
            }
            await File.WriteAllTextAsync(marker, TemplateChecksum + Environment.NewLine, cancellation);
            Console.WriteLine($"Installed verified .NET templates into {directory}");
        }
        finally { if (Directory.Exists(staging)) Directory.Delete(staging, true); }
    }

    private async Task Export(bool server)
    {
        if (!OperatingSystem.IsLinux() || System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture != System.Runtime.InteropServices.Architecture.X64)
            throw new InvalidOperationException("The initial export task targets Linux x86_64; cross-platform export CI is a later milestone.");
        string kind = server ? "server" : "client";
        string directory = Path.Combine(_root, "dist", kind);
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
        Directory.CreateDirectory(directory);
        await Execute("export-" + kind, "godot", "--headless", "--path", GameDirectory,
            "--export-release", server ? "Linux Server" : "Linux Client", Path.Combine(directory, "odot.x86_64"));
        if (!File.Exists(Path.Combine(directory, "odot.x86_64"))) throw new InvalidOperationException($"Missing {kind} export executable.");
    }

    private async Task ExportSmoke()
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(options.StartupTimeout * 2);
        _testSessions = Path.Combine(Path.GetTempPath(), "odot-export-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testSessions);
        try
        {
            int port = FreePort();
            await using var server = StartGame("exported-server", true, true, port, Path.Combine(_root, "dist", "server", "odot.x86_64"));
            await server.WaitFor(e => e.Type == "ready", "exported server readiness", options.StartupTimeout, deadline.Token);
            await using var client = StartGame("exported-client", false, true, port, Path.Combine(_root, "dist", "client", "odot.x86_64"), "--automated");
            await client.WaitFor(e => e.Type == "connected", "exported client connection", options.StartupTimeout, deadline.Token);
            await Action(client, "start", deadline.Token);
            await Action(client, "build 0 farm", deadline.Token);
            await Action(client, "build 1 barracks", deadline.Token);
            await Action(client, "ready", deadline.Token);
            GameEvent recruited = await Action(client, "recruit 1", deadline.Token);
            Require(State(recruited).Players.Single().Soldiers.Length == 1 && State(recruited).Players.Single().Food == 0,
                "exported roles start, construct, produce and recruit through the normal protocol");
        }
        finally { Directory.Delete(_testSessions, true); _testSessions = null; }
    }

    private async Task Ci()
    {
        await Preflight();
        await Execute("restore", "dotnet", "restore", "Odot.slnx", "--locked-mode");
        await Execute("format", "dotnet", "format", "Odot.slnx", "--verify-no-changes", "--no-restore");
        await Prepare();
        await Execute("rules", "dotnet", "test", "tests/Game.Core.Tests/Game.Core.Tests.csproj", "--no-restore", "--no-build", "--nologo");
        await NetworkTests();
        // Keep these calls sequential. Any failed check above must prevent both exports.
        await PrepareTemplates();
        await Export(false);
        await Export(true);
        await ExportSmoke();
        Console.WriteLine("CI checks and Linux exports passed. Outputs remain in dist/; nothing was uploaded or published.");
    }
}
