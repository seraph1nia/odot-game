using System.Xml.Linq;
using Game.Distribution;

namespace DevRunner;

internal static class ReleaseStaging
{
    // Call only on an owned source snapshot. Never rewrite the working project.
    public static void Stamp(string root, ReleaseIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        string game = Path.Combine(root, "src", "Game");
        string project = Path.Combine(game, "project.godot");
        string content = File.ReadAllText(project).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!content.Contains("[application]\n", StringComparison.Ordinal))
            throw new InvalidOperationException("Missing Godot application section.");
        var lines = content.Split('\n').ToList();
        int application = lines.IndexOf("[application]");
        int end = lines.FindIndex(application + 1, line => line.StartsWith('['));
        if (end < 0) end = lines.Count;
        for (int i = end - 1; i > application; i--)
            if (lines[i].StartsWith("config/version=", StringComparison.Ordinal)) lines.RemoveAt(i);
        lines.Insert(application + 1, "config/version=\"" + identity.Version + "\"");
        File.WriteAllText(project, string.Join('\n', lines));

        string csproj = Path.Combine(game, "Game.csproj");
        XDocument document = XDocument.Load(csproj);
        ReleaseVersion version = ReleaseVersion.FromTag("v" + identity.Version);
        document.Root!.Add(new XElement("PropertyGroup",
            new XElement("Version", identity.Version),
            new XElement("AssemblyVersion", version.NumericVersion),
            new XElement("FileVersion", version.NumericVersion),
            new XElement("InformationalVersion", identity.Version),
            new XElement("IncludeSourceRevisionInInformationalVersion", "false")));
        document.Root.Add(new XElement("ItemGroup", new XElement("AssemblyMetadata",
            new XAttribute("Include", "SourceCommit"), new XAttribute("Value", identity.SourceCommit))));
        document.Save(csproj);
        Directory.CreateDirectory(Path.Combine(game, "Distribution"));
        File.WriteAllText(Path.Combine(game, "Distribution", "build-info.json"), identity.ToJson() + "\n");
        // JSON files are not imported resources; explicitly include our metadata.
        string presets = Path.Combine(game, "export_presets.cfg");
        File.WriteAllText(presets, File.ReadAllText(presets).Replace("include_filter=\"\"",
            "include_filter=\"Distribution/build-info.json\"", StringComparison.Ordinal));
    }
}
