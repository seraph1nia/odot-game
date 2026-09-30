using Game.Distribution;
using Godot;
using System.Reflection;

namespace Game;

internal static class BuildInfo
{
    public static ReleaseIdentity? Identity { get; } = Load();
    public static string DisplayVersion => Identity?.Version ?? "Development build";

    private static ReleaseIdentity? Load()
    {
        const string path = "res://Distribution/build-info.json";
        if (!Godot.FileAccess.FileExists(path)) return null;
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read)
            ?? throw new InvalidOperationException("Packaged build identity cannot be read.");
        ReleaseIdentity identity = ReleaseIdentity.FromJson(file.GetAsText());
        string? assemblyVersion = typeof(BuildInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        string? assemblyCommit = typeof(BuildInfo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(attribute => attribute.Key == "SourceCommit")?.Value;
        if (assemblyVersion != identity.Version || assemblyCommit != identity.SourceCommit
            || System.Environment.Version.ToString() != identity.RuntimeVersion
            || ProjectSettings.GetSetting("application/config/version").AsString() != identity.Version)
            throw new InvalidOperationException("Packaged build identity differs from application/assembly version.");
        return identity;
    }
}
