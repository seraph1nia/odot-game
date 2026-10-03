using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace DevRunner;

internal static partial class PlanningStore
{
    internal static PlanningState Read(string root)
    {
        root = Path.GetFullPath(root);
        var roadmap = PlanningYaml.Read(SafePath(root, "planning/roadmap.yaml"));
        PlanningYaml.Fields(roadmap, "version", "queue");
        if (PlanningYaml.Integer(roadmap, "version") != 1) throw new InvalidDataException("planning/roadmap.yaml: unsupported version (expected 1).");
        if (PlanningYaml.Required(roadmap, "queue") is not YamlSequenceNode queue) throw new InvalidDataException("planning/roadmap.yaml: queue must be an array.");
        var entries = queue.Children.Select(node => ReadItem(node as YamlMappingNode ?? throw new InvalidDataException("queue item must be a mapping."))).ToArray();
        var ideas = new List<PlanningIdea>();
        foreach (string directory in new[] { "planning/ideas", "planning/archive/ideas" })
        {
            string path = SafePath(root, directory);
            if (!Directory.Exists(path)) continue;
            foreach (string file in Directory.GetFiles(path, "*.md").Order(StringComparer.Ordinal))
            {
                if (Path.GetFileName(file) == "README.md") continue;
                SafePath(root, Path.GetRelativePath(root, file));
                try
                {
                    var map = PlanningYaml.Read(file, true);
                    PlanningYaml.Fields(map, "id", "title", "status", "created", "related_changes", "supersedes", "tags");
                    string id = PlanningYaml.Text(map, "id");
                    if (!IdeaId().IsMatch(id)) throw new InvalidDataException($"invalid idea ID '{id}'.");
                    if (string.IsNullOrWhiteSpace(PlanningYaml.Text(map, "title"))) throw new InvalidDataException("empty title.");
                    if (!DateOnly.TryParseExact(PlanningYaml.Text(map, "created"), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
                        throw new InvalidDataException("created must be YYYY-MM-DD.");
                    PlanningYaml.List(map, "supersedes");
                    PlanningYaml.List(map, "tags");
                    ideas.Add(new(id, PlanningYaml.Text(map, "status"), PlanningYaml.List(map, "related_changes"), PlanningYaml.List(map, "supersedes"), Path.GetRelativePath(root, file)));
                }
                catch (Exception error) when (error is InvalidDataException or YamlDotNet.Core.YamlException)
                { throw new InvalidDataException($"{Path.GetRelativePath(root, file)}: {error.Message}", error); }
            }
        }
        var changes = new Dictionary<string, PlanningChange>(StringComparer.Ordinal);
        string active = SafePath(root, "openspec/changes");
        if (Directory.Exists(active))
        {
            foreach (string directory in Directory.GetDirectories(active).Order(StringComparer.Ordinal))
            {
                string name = Path.GetFileName(directory);
                if (name == "archive") continue;
                AddChange(root, changes, name, directory, false);
            }
            string archive = SafePath(root, "openspec/changes/archive");
            if (Directory.Exists(archive))
                foreach (string directory in Directory.GetDirectories(archive).Order(StringComparer.Ordinal))
                {
                    string name = Path.GetFileName(directory);
                    if (name.Length < 12 || !DateOnly.TryParseExact(name[..10], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _) || name[10] != '-')
                        throw new InvalidDataException($"{directory}: archive directory must be YYYY-MM-DD-change-id.");
                    AddChange(root, changes, name[11..], directory, true);
                }
        }
        return new(root, entries, [.. ideas], changes);
    }

    private static RoadmapItem ReadItem(YamlMappingNode map)
    {
        PlanningYaml.Fields(map, "change", "state", "priority", "source_ideas", "depends_on", "blocked_by", "conflicts_with", "notes", "approval", "verification");
        string change = PlanningYaml.Text(map, "change");
        if (!ChangeId().IsMatch(change)) throw new InvalidDataException($"planning/roadmap.yaml: invalid change ID '{change}'.");
        return new(change, PlanningYaml.Text(map, "state"), PlanningYaml.Text(map, "priority"), PlanningYaml.List(map, "source_ideas"),
            PlanningYaml.List(map, "depends_on"), PlanningYaml.List(map, "blocked_by"), PlanningYaml.List(map, "conflicts_with"), PlanningYaml.Text(map, "notes"),
            PlanningYaml.OptionalText(map, "approval"), PlanningYaml.OptionalText(map, "verification"));
    }

    private static void AddChange(string root, Dictionary<string, PlanningChange> changes, string id, string directory, bool archived)
    {
        SafePath(root, Path.GetRelativePath(root, directory));
        if (!ChangeId().IsMatch(id)) throw new InvalidDataException($"{directory}: invalid change ID.");
        if (!changes.TryAdd(id, new(id, directory, archived))) throw new InvalidDataException($"openspec/changes: ambiguous active/archive identity '{id}'.");
    }

    internal static IEnumerable<string> Files(string root, string directory, string pattern)
    {
        SafePath(root, Path.GetRelativePath(root, directory));
        if (!Directory.Exists(directory)) yield break;
        foreach (string file in Directory.GetFiles(directory, pattern).Order(StringComparer.Ordinal))
        {
            SafePath(root, Path.GetRelativePath(root, file));
            yield return file;
        }
        foreach (string child in Directory.GetDirectories(directory).Order(StringComparer.Ordinal))
            foreach (string file in Files(root, child, pattern)) yield return file;
    }

    internal static string SafePath(string root, string relative)
    {
        if (relative.Split('/', '\\').Any(segment => segment is "." or "..")) throw new InvalidDataException($"unsafe repository path '{relative}'.");
        string path = Path.GetFullPath(Path.Combine(root, relative));
        string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (Path.IsPathRooted(relative) || !path.StartsWith(prefix, StringComparison.Ordinal)) throw new InvalidDataException($"unsafe repository path '{relative}'.");
        string current = root;
        foreach (string segment in Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar))
        {
            current = Path.Combine(current, segment);
            if ((File.Exists(current) || Directory.Exists(current) || new FileInfo(current).LinkTarget is not null) && File.GetAttributes(current).HasFlag(FileAttributes.ReparsePoint))
                throw new InvalidDataException($"symlink planning/input path '{relative}' is unsupported.");
        }
        return path;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex ChangeId();
    [GeneratedRegex("^IDEA-[0-9]{3,}$", RegexOptions.CultureInvariant)]
    private static partial Regex IdeaId();
}
