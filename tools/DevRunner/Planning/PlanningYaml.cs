using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace DevRunner;

internal static class PlanningYaml
{
    internal static YamlMappingNode Read(string path, bool frontmatter = false)
    {
        string content = File.ReadAllText(path);
        if (content.Length > 512_000) throw new InvalidDataException($"{path}: planning artifact exceeds 512 KB.");
        if (frontmatter)
        {
            string[] lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            int end = Array.IndexOf(lines, "---", 1);
            if (lines[0] != "---" || end < 1) throw new InvalidDataException($"{path}: missing YAML frontmatter delimiters.");
            content = string.Join('\n', lines[1..end]);
        }
        var parser = new Parser(new StringReader(content));
        while (parser.MoveNext())
        {
            if (parser.Current is AnchorAlias || parser.Current is NodeEvent node && (!node.Anchor.IsEmpty || !node.Tag.IsEmpty))
                throw new InvalidDataException($"{path}: YAML aliases, anchors and explicit tags are unsupported.");
        }
        var yaml = new YamlStream();
        yaml.Load(new StringReader(content));
        if (yaml.Documents.Count != 1 || yaml.Documents[0].RootNode is not YamlMappingNode mapping)
            throw new InvalidDataException($"{path}: expected one YAML mapping document.");
        return mapping;
    }

    internal static void Fields(YamlMappingNode map, params string[] allowed)
    {
        foreach (var key in map.Children.Keys)
            if (key is not YamlScalarNode scalar || scalar.Value is null || !allowed.Contains(scalar.Value, StringComparer.Ordinal))
                throw new InvalidDataException($"unknown field '{key}'.");
    }

    internal static YamlNode? Optional(YamlMappingNode map, string key) => map.Children.TryGetValue(new YamlScalarNode(key), out var value) ? value : null;
    internal static YamlNode Required(YamlMappingNode map, string key) => Optional(map, key) ?? throw new InvalidDataException($"missing '{key}'.");
    internal static string Text(YamlMappingNode map, string key) => Scalar(Required(map, key), key);
    internal static string? OptionalText(YamlMappingNode map, string key) => Optional(map, key) is { } node ? Scalar(node, key) : null;
    internal static string Scalar(YamlNode node, string label) => node is YamlScalarNode { Value: { } text }
        && text is not ("null" or "~") ? text : throw new InvalidDataException($"'{label}' must be a string scalar.");
    internal static string[] List(YamlMappingNode map, string key)
    {
        if (Required(map, key) is not YamlSequenceNode sequence) throw new InvalidDataException($"'{key}' must be an array.");
        string[] values = sequence.Children.Select(node => Scalar(node, key)).ToArray();
        if (values.Distinct(StringComparer.Ordinal).Count() != values.Length) throw new InvalidDataException($"duplicate value in '{key}'.");
        return values;
    }
    internal static int Integer(YamlMappingNode map, string key) => int.TryParse(Text(map, key), NumberStyles.None, CultureInfo.InvariantCulture, out int value)
        ? value : throw new InvalidDataException($"'{key}' must be an integer.");
    internal static bool Boolean(YamlMappingNode map, string key) => Text(map, key) switch
    {
        "true" => true,
        "false" => false,
        _ => throw new InvalidDataException($"'{key}' must be true or false.")
    };
}
