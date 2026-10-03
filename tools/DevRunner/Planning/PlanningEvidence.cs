using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DevRunner;

internal static partial class PlanningEvidence
{
    private static readonly string[] PlanningFiles = [".openspec.yaml", "proposal.md", "design.md", "tasks.md"];
    internal static bool Ready(string root, PlanningChange change)
    {
        foreach (string file in PlanningFiles) PlanningStore.SafePath(root, Path.GetRelativePath(root, Path.Combine(change.Directory, file)));
        if (!PlanningFiles.All(file => Nonempty(Path.Combine(change.Directory, file)))) return false;
        var metadata = PlanningYaml.Read(Path.Combine(change.Directory, ".openspec.yaml"));
        if (PlanningYaml.Text(metadata, "schema") != "spec-driven") return false;
        bool skipped = PlanningYaml.Optional(metadata, "skip_specs") is not null && PlanningYaml.Boolean(metadata, "skip_specs");
        string specs = Path.Combine(change.Directory, "specs");
        bool hasSpecs = Directory.Exists(specs) && PlanningStore.Files(root, specs, "spec.md").Any(Nonempty);
        return skipped ? !hasSpecs : hasSpecs;
    }

    internal static bool TasksComplete(PlanningChange change)
    {
        string path = Path.Combine(change.Directory, "tasks.md");
        if (!File.Exists(path)) return false;
        var tasks = TaskMarker().Matches(File.ReadAllText(path));
        return tasks.Count > 0 && tasks.All(match => match.Groups[1].Value.Equals("x", StringComparison.OrdinalIgnoreCase));
    }

    internal static string SpecDigest(string root, PlanningChange change)
    {
        var files = PlanningStore.Files(root, change.Directory, "*")
            .OrderBy(path => Path.GetRelativePath(change.Directory, path), StringComparer.Ordinal);
        var content = new StringBuilder();
        foreach (string path in files)
        {
            PlanningStore.SafePath(root, Path.GetRelativePath(root, path));
            string text = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
            if (Path.GetFileName(path) == "tasks.md") text = TaskMarker().Replace(text, "- [ ]");
            content.Append(Path.GetRelativePath(change.Directory, path).Replace('\\', '/')).Append('\n').Append(text).Append('\n');
        }
        return Hash(content.ToString());
    }

    internal static string CodeDigest(string root)
    {
        string[] files = Git(root, "ls-files", "-z", "--cached", "--others", "--exclude-standard").Split('\0', StringSplitOptions.RemoveEmptyEntries);
        var content = new StringBuilder();
        foreach (string relative in files.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            if (relative.StartsWith("planning/", StringComparison.Ordinal) || relative.StartsWith("openspec/", StringComparison.Ordinal)) continue;
            string path = PlanningStore.SafePath(root, relative);
            content.Append(relative).Append('\n');
            content.Append(File.Exists(path) ? Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))) : "DELETED").Append('\n');
        }
        return Hash(content.ToString());
    }

    internal static string CodeDigest(string root, string revision)
    {
        var content = new StringBuilder();
        string[] entries = Git(root, "ls-tree", "-rz", "--full-tree", revision).Split('\0', StringSplitOptions.RemoveEmptyEntries);
        foreach (string entry in entries.OrderBy(entry => entry[(entry.IndexOf('\t') + 1)..], StringComparer.Ordinal))
        {
            int separator = entry.IndexOf('\t');
            string path = entry[(separator + 1)..];
            if (path.StartsWith("planning/", StringComparison.Ordinal) || path.StartsWith("openspec/", StringComparison.Ordinal)) continue;
            string[] identity = entry[..separator].Split(' ');
            if (identity[1] != "blob" || identity[0] == "120000") throw new InvalidDataException($"unsupported reviewed production object '{path}'.");
            content.Append(path).Append('\n').Append(Convert.ToHexStringLower(SHA256.HashData(GitBytes(root, "cat-file", "blob", identity[2])))).Append('\n');
        }
        return Hash(content.ToString());
    }

    internal static string[] CheckApproval(PlanningState state, RoadmapItem item, PlanningChange change)
    {
        try
        {
            var map = Receipt(state.Root, item.Change, item.Approval);
            PlanningYaml.Fields(map, "change", "approved_by", "approved_at", "authorization", "spec_digest");
            if (PlanningYaml.Text(map, "change") != item.Change) throw new InvalidDataException("approval belongs to another change.");
            foreach (string field in new[] { "approved_by", "approved_at", "authorization" })
                if (string.IsNullOrWhiteSpace(PlanningYaml.Text(map, field))) throw new InvalidDataException($"approval {field} cannot be empty.");
            if (PlanningYaml.Text(map, "spec_digest") != SpecDigest(state.Root, change)) throw new InvalidDataException("approval spec_digest is stale.");
            return [];
        }
        catch (Exception error) when (error is InvalidDataException or IOException or YamlDotNet.Core.YamlException or UnauthorizedAccessException)
        { return [$"planning/roadmap.yaml [{item.Change}]: {error.Message}"]; }
    }

    internal static string[] CheckReview(PlanningState state, RoadmapItem item, PlanningChange change)
    {
        try
        {
            var map = Receipt(state.Root, item.Change, item.Verification);
            PlanningYaml.Fields(map, "change", "result", "reviewer_task", "shipper_task", "base", "head", "spec_digest", "code_digest", "canonical_inputs", "attempt", "validation_passed", "decisions_resolved", "landed_revision");
            if (PlanningYaml.Text(map, "change") != item.Change || PlanningYaml.Text(map, "result") != "PASS") throw new InvalidDataException("current independent PASS required.");
            string reviewer = PlanningYaml.Text(map, "reviewer_task"), shipper = PlanningYaml.Text(map, "shipper_task");
            if (string.IsNullOrWhiteSpace(reviewer) || string.IsNullOrWhiteSpace(shipper) || reviewer == shipper) throw new InvalidDataException("distinct nonempty reviewer_task/shipper_task required.");
            if (PlanningYaml.Integer(map, "attempt") is not (1 or 2)) throw new InvalidDataException("review attempt must be 1 or 2.");
            if (!PlanningYaml.Boolean(map, "validation_passed") || !PlanningYaml.Boolean(map, "decisions_resolved") || !TasksComplete(change))
                throw new InvalidDataException("successful validation, complete tasks and resolved decisions required.");
            if (PlanningYaml.Text(map, "spec_digest") != SpecDigest(state.Root, change)) throw new InvalidDataException("PASS spec_digest is stale.");
            string code = PlanningYaml.Text(map, "code_digest");
            bool historical = item.State is "completed" or "archived";
            if (!historical && code != CodeDigest(state.Root)) throw new InvalidDataException("PASS code_digest is stale.");
            string head = PlanningYaml.Text(map, "head"), baseRevision = PlanningYaml.Text(map, "base");
            foreach (string revision in new[] { head, baseRevision })
                if (!Revision().IsMatch(revision) || Git(state.Root, "rev-parse", "--verify", revision + "^{commit}").Trim() != revision)
                    throw new InvalidDataException($"unavailable exact Git revision '{revision}'.");
            Git(state.Root, "merge-base", "--is-ancestor", baseRevision, head);
            if (code != CodeDigest(state.Root, head)) throw new InvalidDataException("PASS code_digest does not match reviewed head.");
            if (!historical) Git(state.Root, "diff", "--exit-code", head, "--", ".", ":(exclude)planning", ":(exclude)openspec");
            string[] untracked = Git(state.Root, "ls-files", "--others", "--exclude-standard").Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (!historical && untracked.Any(path => !path.StartsWith("planning/", StringComparison.Ordinal) && !path.StartsWith("openspec/", StringComparison.Ordinal)))
                throw new InvalidDataException("PASS cannot cover uncommitted production inputs.");
            string canonical = PlanningStore.SafePath(state.Root, "openspec/specs");
            if (PlanningYaml.Required(map, "canonical_inputs") is not YamlDotNet.RepresentationModel.YamlMappingNode inputs) throw new InvalidDataException("canonical_inputs must be a file/hash mapping.");
            string[] canonicalFiles = historical
                ? Git(state.Root, "ls-tree", "-r", "--name-only", head, "--", "openspec/specs").Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Where(path => path.EndsWith("/spec.md", StringComparison.Ordinal)).Select(path => Path.Combine(state.Root, path)).ToArray()
                : Directory.Exists(canonical) ? PlanningStore.Files(state.Root, canonical, "spec.md").ToArray() : [];
            foreach (string file in canonicalFiles)
            {
                string relative = Path.GetRelativePath(state.Root, file).Replace('\\', '/');
                if (!historical) PlanningStore.SafePath(state.Root, relative);
                byte[] bytes = historical ? GitBytes(state.Root, "show", head + ":" + relative) : File.ReadAllBytes(file);
                if (PlanningYaml.OptionalText(inputs, relative) != Convert.ToHexStringLower(SHA256.HashData(bytes))) throw new InvalidDataException($"PASS canonical input '{relative}' is missing or stale.");
            }
            if (inputs.Children.Count != canonicalFiles.Length) throw new InvalidDataException("PASS canonical_inputs includes missing/noncanonical files.");
            if (item.State is "completed" or "archived")
            {
                string landed = PlanningYaml.Text(map, "landed_revision");
                if (!Revision().IsMatch(landed) || Git(state.Root, "rev-parse", "--verify", landed + "^{commit}").Trim() != landed)
                    throw new InvalidDataException("completion requires a recorded landed Git revision.");
                if (CodeDigest(state.Root, landed) != code) throw new InvalidDataException("landed production inputs differ from reviewed head; independent review required.");
                Git(state.Root, "diff", "--exit-code", head, landed, "--", ".", ":(exclude)planning", ":(exclude)openspec");
                Git(state.Root, "merge-base", "--is-ancestor", head, landed);
                Git(state.Root, "merge-base", "--is-ancestor", landed, "HEAD");
                Git(state.Root, "merge-base", "--is-ancestor", landed, "refs/heads/main");
            }
            return [];
        }
        catch (Exception error) when (error is InvalidDataException or IOException or YamlDotNet.Core.YamlException or UnauthorizedAccessException)
        { return [$"planning/roadmap.yaml [{item.Change}]: {error.Message}"]; }
    }

    private static YamlDotNet.RepresentationModel.YamlMappingNode Receipt(string root, string change, string? relative)
    {
        if (relative is null || !relative.StartsWith($"planning/evidence/{change}/", StringComparison.Ordinal) || Path.GetExtension(relative) != ".md")
            throw new InvalidDataException("receipt path must be planning/evidence/<change>/*.md.");
        return PlanningYaml.Read(PlanningStore.SafePath(root, relative), true);
    }
    internal static string Git(string root, params string[] arguments) => Encoding.UTF8.GetString(GitBytes(root, arguments));
    private static byte[] GitBytes(string root, params string[] arguments)
    {
        var start = new ProcessStartInfo("git") { WorkingDirectory = root, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidDataException("cannot start git for lifecycle identity checks.");
        using var buffer = new MemoryStream();
        Task stdout = process.StandardOutput.BaseStream.CopyToAsync(buffer);
        Task<string> stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(15000)) { process.Kill(true); process.WaitForExit(); throw new InvalidDataException("Git lifecycle check timed out."); }
        if (process.ExitCode != 0) throw new InvalidDataException($"Git lifecycle check failed: {stderr.GetAwaiter().GetResult().Trim()}");
        stdout.GetAwaiter().GetResult();
        return buffer.ToArray();
    }
    private static bool Nonempty(string path) => File.Exists(path) && new FileInfo(path).Length > 0;
    private static string Hash(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    [GeneratedRegex(@"(?m)^\s*- \[([ xX])\]", RegexOptions.CultureInvariant)]
    private static partial Regex TaskMarker();
    [GeneratedRegex("^[a-f0-9]{40}$", RegexOptions.CultureInvariant)]
    private static partial Regex Revision();
}
