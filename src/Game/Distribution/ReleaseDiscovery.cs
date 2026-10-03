using System.Net;
using System.Text;
using System.Text.Json;

namespace Game.Distribution;

public sealed record UpdateCandidate(ReleaseVersion Version, string DownloadUrl);
public sealed record UpdateDiscoveryResult(UpdateCandidate? Update, bool HasEligibleRelease);
public enum UpdateCheckState { Development, Checking, Busy, UpToDate, NoRelease, Available, Failed, Opened }
public sealed record UpdateCheckStatus(UpdateCheckState State, string Message, UpdateCandidate? Candidate = null);

public sealed class ManualUpdateChecker : IDisposable
{
    private readonly ReleaseIdentity? _identity;
    private readonly string _target;
    private readonly HttpClient _client;
    private readonly Func<string, bool> _open;
    private readonly CancellationTokenSource _lifetime = new();
    private int _active;
    private UpdateCandidate? _candidate;

    public ManualUpdateChecker(ReleaseIdentity? identity, string target, HttpClient client, Func<string, bool> open)
    {
        _identity = identity; _target = target; _client = client; _open = open;
    }

    public async Task<UpdateCheckStatus> Check(CancellationToken cancellation = default)
    {
        if (_identity is null) return new(UpdateCheckState.Development, "Development builds do not check for releases.");
        if (Interlocked.CompareExchange(ref _active, 1, 0) != 0) return new(UpdateCheckState.Busy, "An update check is already running.");
        try
        {
            using var bounded = CancellationTokenSource.CreateLinkedTokenSource(cancellation, _lifetime.Token);
            bounded.CancelAfter(TimeSpan.FromSeconds(12));
            ReleaseVersion installed = ReleaseVersion.TryParse(_identity.Version, out ReleaseVersion? version) ? version! : throw new InvalidDataException("Installed release version is invalid.");
            UpdateDiscoveryResult result = await ReleaseDiscovery.Discover(_client, installed, _identity.Repository, _target, bounded.Token);
            _candidate = result.Update;
            if (result.Update is not null) return new(UpdateCheckState.Available, $"{GameBrand.Title} {result.Update.Version.Value} is available.", result.Update);
            return result.HasEligibleRelease
                ? new(UpdateCheckState.UpToDate, $"{GameBrand.Title} is up to date.")
                : new(UpdateCheckState.NoRelease, "No packaged release is available for this update channel.");
        }
        catch (OperationCanceledException) when (!_lifetime.IsCancellationRequested && !cancellation.IsCancellationRequested)
        { return new(UpdateCheckState.Failed, "Could not check for updates: the request timed out."); }
        catch (OperationCanceledException)
        { return new(UpdateCheckState.Failed, "The update check ended before it completed."); }
        catch (Exception error) when (error is HttpRequestException or InvalidDataException or InvalidOperationException or JsonException)
        { return new(UpdateCheckState.Failed, "Could not check for updates. " + error.Message); }
        finally { Volatile.Write(ref _active, 0); }
    }

    public UpdateCheckStatus OpenUpdate()
    {
        if (_candidate is null) return new(UpdateCheckState.Failed, "Check for an available update first.");
        try
        {
            return _open(_candidate.DownloadUrl)
                ? new(UpdateCheckState.Opened, _target == "windows-x64"
                    ? $"Download opened. Close {GameBrand.Title}, then run the installer."
                    : $"Release page opened. Close {GameBrand.Title}, then run the versioned install script.", _candidate)
                : new(UpdateCheckState.Failed, "Could not open the download in your browser.", _candidate);
        }
        catch
        { return new(UpdateCheckState.Failed, "Could not open the download in your browser.", _candidate); }
    }

    public void Dispose() { _lifetime.Cancel(); _lifetime.Dispose(); _client.Dispose(); }
}

public static class ReleaseDiscovery
{
    public const int PageSize = 50;
    public const int MaximumPages = 3;
    public const int MaximumResponseBytes = 1024 * 1024;

    public static async Task<UpdateDiscoveryResult> Discover(HttpClient client, ReleaseVersion installed, string repository,
        string target, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidateRepository(repository);
        if (target is not ("linux-x64" or "windows-x64")) throw new ArgumentException("Unsupported update target.", nameof(target));
        var candidates = new List<UpdateCandidate>();
        bool hasEligible = false;
        for (int page = 1; page <= MaximumPages; page++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.github.com/repos/{repository}/releases?per_page={PageSize}&page={page}");
            request.Headers.UserAgent.ParseAdd("Odot-update-check");
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation);
            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.TooManyRequests)
                throw new InvalidOperationException("GitHub temporarily refused the update check.");
            response.EnsureSuccessStatusCode();
            byte[] bytes = await ReadBounded(response.Content, cancellation);
            int count = ParsePage(bytes, installed, repository, target, candidates, ref hasEligible);
            if (count < PageSize) return Select(installed, candidates, hasEligible);
        }
        throw new InvalidOperationException("GitHub release pagination exceeded the supported limit.");
    }

    internal static UpdateDiscoveryResult DiscoverFromPages(ReleaseVersion installed, string repository, string target, params string[] pages)
    {
        ValidateRepository(repository);
        if (pages.Length > MaximumPages) throw new InvalidOperationException("GitHub release pagination exceeded the supported limit.");
        var candidates = new List<UpdateCandidate>();
        bool hasEligible = false;
        foreach (string page in pages) ParsePage(Encoding.UTF8.GetBytes(page), installed, repository, target, candidates, ref hasEligible);
        return Select(installed, candidates, hasEligible);
    }

    private static UpdateDiscoveryResult Select(ReleaseVersion installed, List<UpdateCandidate> candidates, bool hasEligible)
    {
        UpdateCandidate? update = candidates.Where(candidate => installed.CanUpdateTo(candidate.Version))
            .OrderByDescending(candidate => candidate.Version, ReleaseVersionComparer.Instance).FirstOrDefault();
        return new(update, hasEligible);
    }

    private static int ParsePage(ReadOnlyMemory<byte> json, ReleaseVersion installed, string repository, string target,
        List<UpdateCandidate> candidates, ref bool hasEligible)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array) throw new InvalidDataException("GitHub release response was not an array.");
        int count = 0;
        foreach (JsonElement release in document.RootElement.EnumerateArray())
        {
            count++;
            if (RequiredBoolean(release, "draft")) continue;
            string tag = RequiredString(release, "tag_name");
            if (!tag.StartsWith('v') || !ReleaseVersion.TryParse(tag[1..], out ReleaseVersion? version)) continue;
            bool prerelease = RequiredBoolean(release, "prerelease");
            if (prerelease != version!.IsPreview) throw new InvalidDataException("Release channel disagrees with its SemVer tag.");
            if (!installed.IsPreview && version.IsPreview) continue;
            hasEligible = true;
            string releaseUrl = RequiredString(release, "html_url");
            string expectedPage = $"https://github.com/{repository}/releases/tag/{tag}";
            if (!string.Equals(releaseUrl, expectedPage, StringComparison.Ordinal)) throw new InvalidDataException("Release page URL is outside the configured repository.");
            string[] required = target == "windows-x64"
                ? [$"odot-{version.Value}-windows-x64-setup.exe"]
                : [$"odot-{version.Value}-linux-x64.tar.gz", $"odot-{version.Value}-linux-x64-install.sh"];
            if (!release.TryGetProperty("assets", out JsonElement assetList) || assetList.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("GitHub release response is missing assets.");
            var assets = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            foreach (JsonElement asset in assetList.EnumerateArray())
                if (!assets.TryAdd(RequiredString(asset, "name"), asset)) throw new InvalidDataException("GitHub release response contains duplicate assets.");
            foreach (string name in required)
            {
                if (!assets.TryGetValue(name, out JsonElement asset)) throw new InvalidDataException($"Release {tag} is missing {name}.");
                string url = RequiredString(asset, "browser_download_url");
                string expected = $"https://github.com/{repository}/releases/download/{tag}/{name}";
                if (!string.Equals(url, expected, StringComparison.Ordinal)) throw new InvalidDataException("Release asset URL is outside the configured repository.");
            }
            candidates.Add(new(version, target == "windows-x64" ? RequiredString(assets[required[0]], "browser_download_url") : releaseUrl));
        }
        return count;
    }

    private static string RequiredString(JsonElement value, string property)
        => value.TryGetProperty(property, out JsonElement item) && item.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(item.GetString())
            ? item.GetString()! : throw new InvalidDataException("GitHub release response is missing " + property + ".");
    private static bool RequiredBoolean(JsonElement value, string property)
        => value.TryGetProperty(property, out JsonElement item) && item.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? item.GetBoolean() : throw new InvalidDataException("GitHub release response is missing " + property + ".");
    private static void ValidateRepository(string repository)
    {
        if (repository.Split('/') is not [var owner, var name] || owner.Length == 0 || name.Length == 0
            || !owner.Concat(name).All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.'))
            throw new ArgumentException("Invalid update repository.", nameof(repository));
    }
    private static async Task<byte[]> ReadBounded(HttpContent content, CancellationToken cancellation)
    {
        if (content.Headers.ContentLength > MaximumResponseBytes) throw new InvalidDataException("GitHub release response is too large.");
        await using Stream input = await content.ReadAsStreamAsync(cancellation);
        using var output = new MemoryStream();
        var buffer = new byte[8192];
        while (true)
        {
            int read = await input.ReadAsync(buffer, cancellation);
            if (read == 0) return output.ToArray();
            if (output.Length + read > MaximumResponseBytes) throw new InvalidDataException("GitHub release response is too large.");
            output.Write(buffer, 0, read);
        }
    }

    private sealed class ReleaseVersionComparer : IComparer<ReleaseVersion>
    {
        public static readonly ReleaseVersionComparer Instance = new();
        public int Compare(ReleaseVersion? x, ReleaseVersion? y) => x is null ? y is null ? 0 : -1 : y is null ? 1 : x.ComparePrecedence(y);
    }
}
