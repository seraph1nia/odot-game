using System.Text.Json;
namespace DevRunner;

internal static class PlanningCommand
{
    internal static int Run(string root, string command, string[] arguments, TextWriter output)
    {
        if (command == "planning-inputs")
        {
            if (arguments.Length != 1) throw new ArgumentException("planning-inputs requires exactly one change ID.");
            var state = PlanningStore.Read(root);
            if (!state.Changes.TryGetValue(arguments[0], out var change)) throw new ArgumentException("Unknown OpenSpec change.");
            string specs = PlanningStore.SafePath(root, "openspec/specs");
            var canonical = Directory.Exists(specs) ? PlanningStore.Files(root, specs, "spec.md")
                .ToDictionary(path => Path.GetRelativePath(root, path).Replace('\\', '/'), path => Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))), StringComparer.Ordinal)
                : new Dictionary<string, string>(StringComparer.Ordinal);
            output.WriteLine(JsonSerializer.Serialize(new
            {
                change = change.Id,
                head = PlanningEvidence.Git(root, "rev-parse", "HEAD").Trim(),
                spec_digest = PlanningEvidence.SpecDigest(root, change),
                code_digest = PlanningEvidence.CodeDigest(root),
                canonical_inputs = canonical
            }));
            return 0;
        }
        if (arguments.Any(argument => argument != "--json") || arguments.Length > 1) throw new ArgumentException("Planning commands accept only optional --json.");
        var result = PlanningValidation.Check(root);
        bool next = command == "planning-next";
        if (arguments.Length == 1)
            output.WriteLine(JsonSerializer.Serialize(new { valid = result.Valid, change = next ? result.Change : null, diagnostics = result.Diagnostics, exclusions = next ? result.Exclusions : [] }));
        else
        {
            foreach (string diagnostic in result.Diagnostics) output.WriteLine(diagnostic);
            if (result.Valid) output.WriteLine(next ? $"Next: {result.Change ?? "none"}" : "Planning valid.");
            if (next) foreach (string exclusion in result.Exclusions) output.WriteLine(exclusion);
        }
        return result.Valid ? 0 : 1;
    }
}
