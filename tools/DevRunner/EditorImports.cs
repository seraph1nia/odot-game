namespace DevRunner;

internal static class EditorImports
{
    // Godot 4.7.2 discovers new extensions too late for its automatic import quit.
    // Load them at startup instead, using the engine's ordinary generated list.
    internal static void SeedStartupExtensions(string project)
    {
        var paths = new List<string>();
        void Scan(string directory)
        {
            if (File.Exists(Path.Combine(directory, ".gdignore"))) return;
            foreach (string file in Directory.EnumerateFiles(directory, "*.gdextension"))
                if (!Path.GetFileName(file).StartsWith('.'))
                    paths.Add("res://" + Path.GetRelativePath(project, file).Replace('\\', '/'));
            foreach (string child in Directory.EnumerateDirectories(directory))
                if (!Path.GetFileName(child).StartsWith('.') && !File.Exists(Path.Combine(child, "project.godot"))) Scan(child);
        }
        Scan(project);
        string directory = Path.Combine(project, ".godot");
        Directory.CreateDirectory(directory);
        string list = Path.Combine(directory, "extension_list.cfg");
        string content = string.Concat(paths.Order(StringComparer.Ordinal).Select(path => path + "\n"));
        if (!File.Exists(list) || File.ReadAllText(list) != content) File.WriteAllText(list, content);
    }
}
