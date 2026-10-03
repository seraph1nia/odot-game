namespace DevRunner.Tests;

internal static class PackagingConfiguration
{
    public static Dictionary<string, List<string>> Sections(string text, char comment, StringComparer? comparer = null)
    {
        var sections = new Dictionary<string, List<string>>(comparer ?? StringComparer.Ordinal);
        List<string>? entries = null;
        foreach (string raw in text.Split('\n'))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line[0] == comment) continue;
            if (line[0] == '[' && line[^1] == ']')
            {
                string name = line[1..^1].Trim();
                if (!sections.TryGetValue(name, out entries)) sections.Add(name, entries = []);
            }
            else (entries ?? throw new FormatException("Entry outside a configuration section.")).Add(line);
        }
        return sections;
    }

    public static Dictionary<string, string> Assignments(IEnumerable<string> lines, char separator, bool inno = false)
    {
        var values = new Dictionary<string, string>(inno ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (string line in lines)
        {
            int index = line.IndexOf(separator);
            if (index <= 0) throw new FormatException("Invalid configuration assignment: " + line);
            string key = line[..index].Trim(), value = line[(index + 1)..].Trim();
            if (inno && value.StartsWith('"'))
            {
                if (!value.EndsWith('"') || value.Length < 2) throw new FormatException("Unterminated Inno value.");
                value = value[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal);
            }
            if (!values.TryAdd(key, value)) throw new FormatException("Duplicate configuration key: " + key);
        }
        return values;
    }

    public static Dictionary<string, string> InnoParameters(string line)
    {
        var parameters = new List<string>();
        bool quoted = false;
        int start = 0;
        for (int index = 0; index < line.Length; index++)
        {
            if (line[index] == '"') quoted = !quoted;
            if (line[index] != ';' || quoted) continue;
            parameters.Add(line[start..index]);
            start = index + 1;
        }
        if (quoted) throw new FormatException("Unterminated Inno parameter.");
        if (!string.IsNullOrWhiteSpace(line[start..])) parameters.Add(line[start..]);
        return Assignments(parameters, ':', inno: true);
    }

    public static Dictionary<string, List<string>> InnoSections(string text, IReadOnlyDictionary<string, string> definitions)
    {
        var activeLines = new List<string>();
        var conditions = new Stack<(bool Parent, bool Branch, bool HasElse)>();
        bool active = true;
        foreach (string raw in text.Split('\n'))
        {
            string line = raw.Trim();
            if (line.StartsWith('#'))
            {
                string[] directive = line.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
                string argument = directive.Length == 2 ? directive[1] : "";
                switch (directive[0])
                {
                    case "#ifdef":
                    case "#ifndef":
                        bool branch = definitions.ContainsKey(argument) == (directive[0] == "#ifdef");
                        conditions.Push((active, branch, false));
                        active &= branch;
                        break;
                    case "#else":
                        if (!conditions.TryPop(out var condition) || condition.HasElse) throw new FormatException("Unexpected #else.");
                        conditions.Push((condition.Parent, condition.Branch, true));
                        active = condition.Parent && !condition.Branch;
                        break;
                    case "#endif":
                        if (!conditions.TryPop(out var ended)) throw new FormatException("Unexpected #endif.");
                        active = ended.Parent;
                        break;
                    case "#error":
                        if (active) throw new FormatException(argument);
                        break;
                    default:
                        throw new FormatException("Unsupported Inno preprocessor directive: " + directive[0]);
                }
                continue;
            }
            if (!active || line.Length == 0 || line.StartsWith(';')) continue;
            foreach ((string name, string value) in definitions) line = line.Replace("{#" + name + "}", value, StringComparison.Ordinal);
            if (line.Contains("{#", StringComparison.Ordinal)) throw new FormatException("Unresolved Inno preprocessor expression.");
            activeLines.Add(line);
        }
        if (conditions.Count != 0) throw new FormatException("Unterminated Inno preprocessor condition.");
        return Sections(string.Join('\n', activeLines), ';', StringComparer.OrdinalIgnoreCase);
    }
}
