using System.Text.Json;

namespace DevRunner;

// A failure-only whitelist for the synthetic default combat test, not a log uploader.
internal static class CombatFailureEvidence
{
    internal const long MaximumFileBytes = 32 * 1024 * 1024, MaximumTotalBytes = 128 * 1024 * 1024;
    internal static readonly string[] Milestones = ["combat-catapult", "combat-locomotion", "combat-unit-inspection",
        "combat-paused", "combat-camera-paused", "combat-unit-inspection-resized", "combat-resized", "combat-casualty"];
    internal static readonly string[] JsonFiles = ["combat-casualty-admission.json", "combat-casualty-pause.json",
        "combat-death-cleanup.json", "combat-pose-proof.json"];
    internal sealed record FileResult(string Name, string Status, long Bytes, string? FirstMissingField);

    public static void Collect(string directory, string clientState, string observerState, string authorityState)
    {
        string destination = Path.Combine(directory, "pose-failure");
        Directory.CreateDirectory(destination);
        var files = JsonFiles.Select(name => (Source: Path.Combine(directory, name), Name: name))
            .Concat(Milestones.SelectMany(name => new[] { (Path.Combine(directory, name + ".png"), name + ".png"),
                (Path.Combine(directory, name + "-observation.json"), name + "-observation.json") }))
            .Concat(new[] { (clientState, "ui-client.states.json"), (observerState, "ui-observer.states.json"), (authorityState, "ui-server.states.json") });
        var results = new List<FileResult>();
        long total = 0;
        foreach (var (source, name) in files)
        {
            string status = "retained";
            string? missing = null;
            long bytes = 0;
            try
            {
                if (Path.GetDirectoryName(Path.GetFullPath(source)) != Path.GetFullPath(directory)) status = "outside-owned-evidence";
                else if (!File.Exists(source)) status = "missing-file";
                else if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0) status = "excluded-link";
                else if (new FileInfo(source).Length > MaximumFileBytes) status = "file-byte-limit";
                else
                {
                    byte[] content = File.ReadAllBytes(source);
                    if (name.EndsWith(".json", StringComparison.Ordinal))
                    {
                        string redacted = DiagnosticText.Redact(System.Text.Encoding.UTF8.GetString(content));
                        using JsonDocument json = JsonDocument.Parse(redacted);
                        string[] required = name.EndsWith(".states.json", StringComparison.Ordinal) ? ["States", "RecentUi"]
                            : name.EndsWith("-observation.json", StringComparison.Ordinal) ? ["Id", "CombatTick", "Revision", "Units", "Camera"]
                            : name == "combat-pose-proof.json" ? ["Entry", "Exit", "Provenance", "RecoveryPrevious", "RecoveryCurrent"]
                            : name == "combat-casualty-admission.json" ? ["Events"]
                            : name == "combat-casualty-pause.json" ? ["MatchId", "Tick", "Revision"] : ["Removal"];
                        missing = required.FirstOrDefault(field => !json.RootElement.TryGetProperty(field, out _));
                        content = System.Text.Encoding.UTF8.GetBytes(redacted);
                    }
                    if (content.Length > MaximumFileBytes || total + content.Length > MaximumTotalBytes) status = "bundle-byte-limit";
                    else
                    {
                        File.WriteAllBytes(Path.Combine(destination, name), content);
                        bytes = content.Length;
                        total += bytes;
                    }
                }
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
            {
                status = "unreadable-or-invalid-json";
            }
            results.Add(new(name, status, bytes, missing));
            if (status != "retained" || missing is not null)
                Console.WriteLine($"POSE evidence: {name}: {status}; first missing field={missing ?? "none"}");
        }
        string manifest = JsonSerializer.Serialize(new
        {
            Contract = "Synthetic default combat pose failure; explicit whitelist only; no raw logs, credentials, IPC, Xauthority or environment.",
            MaximumFileBytes,
            MaximumTotalBytes,
            TotalBytes = total,
            Files = results,
            FirstMissing = results.FirstOrDefault(file => file.Status != "retained" || file.FirstMissingField is not null)
        }, Evidence.JsonOptions);
        File.WriteAllText(Path.Combine(destination, "manifest.json"), manifest);
        Console.WriteLine($"POSE evidence: {results.Count(file => file.Status == "retained")}/{results.Count} files, {total} bytes; manifest={Path.Combine(destination, "manifest.json")}");
    }
}
