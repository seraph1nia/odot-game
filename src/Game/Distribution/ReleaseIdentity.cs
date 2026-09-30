using System.Text.Json;

namespace Game.Distribution;

public sealed record ReleaseIdentity(string Version, string SourceCommit, string Channel, string Repository,
    string Target, string EngineVersion, string SdkVersion, string RuntimeVersion, string SteamMode, uint SteamAppId)
{
    public const string UpdateRepository = "seraph1nia/odot-game";

    public static ReleaseIdentity Create(string tag, string commit, string target, bool production, uint? appId)
    {
        ReleaseVersion version = ReleaseVersion.FromTag(tag);
        ArgumentNullException.ThrowIfNull(commit);
        if (commit.Length != 40 || !commit.All(c => char.IsAsciiHexDigit(c) && !char.IsAsciiLetterUpper(c)))
            throw new ArgumentException("Release source must be a full lowercase 40-character Git commit.", nameof(commit));
        if (target is not ("linux-x64" or "windows-x64")) throw new ArgumentException("Unsupported release target.", nameof(target));
        if (!version.IsPreview && !production) throw new ArgumentException("Stable tags require production packaging.", nameof(production));
        if (appId == 0 || production && appId is null or 480)
            throw new ArgumentException("Production requires this game's own non-480 AppID.", nameof(appId));
        return new(version.Value, commit, version.Channel, UpdateRepository, target, "4.7.2", "10.0.401", "10.0.12",
            production ? "production" : "test", appId ?? 480);
    }

    public string ToJson() => JsonSerializer.Serialize(this);

    public static ReleaseIdentity FromJson(string json)
    {
        ReleaseIdentity result = JsonSerializer.Deserialize<ReleaseIdentity>(json)
            ?? throw new ArgumentException("Missing build identity.", nameof(json));
        ReleaseIdentity expected = Create("v" + result.Version, result.SourceCommit, result.Target,
            result.SteamMode == "production", result.SteamAppId);
        if (result != expected) throw new ArgumentException("Build identity has unsupported or inconsistent metadata.", nameof(json));
        return result;
    }
}
