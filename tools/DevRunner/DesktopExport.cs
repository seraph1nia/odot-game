namespace DevRunner;

internal sealed record DesktopExport(string Target, string Preset, string Executable, string NativeDirectory, string NativeLibrary, string SteamLibrary)
{
    public static DesktopExport For(string target) => target switch
    {
        "linux-x64" => new(target, "Linux", "odot.x86_64", "linux64", "libgodotsteam.linux.template_release.x86_64.so", "libsteam_api.so"),
        "windows-x64" => new(target, "Windows", "odot.exe", "win64", "libgodotsteam.windows.template_release.x86_64.dll", "steam_api64.dll"),
        _ => throw new ArgumentException("--target must be linux-x64 or windows-x64.", nameof(target))
    };
    public string Kind(bool production) => (Target == "windows-x64" ? "windows-" : "") + (production ? "production-client" : "client");
    public string ClientPreset(bool production) => Preset + (production ? " Production Client" : " Client");
}
