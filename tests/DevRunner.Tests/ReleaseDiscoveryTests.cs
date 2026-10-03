using System.Net;
using System.Text;
using Game.Distribution;
using Xunit;

namespace DevRunner.Tests;

public sealed class ReleaseDiscoveryTests
{
    [Fact]
    public void SelectsSemanticNewestAndStableDoesNotSeePreview()
    {
        var installed = ReleaseVersion.FromTag("v0.9.0");
        UpdateDiscoveryResult result = ReleaseDiscovery.DiscoverFromPages(installed, ReleaseIdentity.UpdateRepository, "windows-x64",
            Page(Release("v0.10.0"), Release("v0.8.0"), Release("v1.0.0-beta.1")));
        Assert.Equal("0.10.0", result.Update!.Version.Value);
        Assert.EndsWith("odot-0.10.0-windows-x64-setup.exe", result.Update.DownloadUrl);
    }

    [Fact]
    public void PreviewSeesStableAndNumericPrereleaseOrdering()
    {
        var installed = ReleaseVersion.FromTag("v0.1.0-beta.2");
        UpdateDiscoveryResult result = ReleaseDiscovery.DiscoverFromPages(installed, ReleaseIdentity.UpdateRepository, "linux-x64",
            Page(Release("v0.1.0-beta.10", linux: true), Release("v0.1.0", linux: true)));
        Assert.Equal("0.1.0", result.Update!.Version.Value);
        Assert.Equal("https://github.com/seraph1nia/odot-game/releases/tag/v0.1.0", result.Update.DownloadUrl);
    }

    [Fact]
    public void DraftsAndEqualOrOlderVersionsAreNotUpdates()
    {
        var installed = ReleaseVersion.FromTag("v1.0.0+local");
        UpdateDiscoveryResult result = ReleaseDiscovery.DiscoverFromPages(installed, ReleaseIdentity.UpdateRepository, "windows-x64",
            Page(Release("v2.0.0", draft: true), Release("v1.0.0+other"), Release("v0.9.0")));
        Assert.Null(result.Update);
        Assert.True(result.HasEligibleRelease);
        UpdateDiscoveryResult invalid = ReleaseDiscovery.DiscoverFromPages(installed, ReleaseIdentity.UpdateRepository, "windows-x64",
            Page(Release("vnot-semver", omitAsset: true)));
        Assert.False(invalid.HasEligibleRelease);
        Assert.Null(invalid.Update);
    }

    [Fact]
    public void RejectsMissingOrForeignAssetsAndPaginationOverflow()
    {
        var installed = ReleaseVersion.FromTag("v0.1.0-beta.1");
        Assert.Throws<InvalidDataException>(() => ReleaseDiscovery.DiscoverFromPages(installed, ReleaseIdentity.UpdateRepository, "windows-x64", Page(Release("v0.2.0", omitAsset: true))));
        Assert.Throws<InvalidDataException>(() => ReleaseDiscovery.DiscoverFromPages(installed, ReleaseIdentity.UpdateRepository, "windows-x64", Page(Release("v0.2.0", foreign: true))));
        Assert.Throws<InvalidOperationException>(() => ReleaseDiscovery.DiscoverFromPages(installed, ReleaseIdentity.UpdateRepository, "windows-x64", "[]", "[]", "[]", "[]"));
    }

    [Fact]
    public async Task HttpDiscoveryReportsErrorsAndBoundsResponse()
    {
        var installed = ReleaseVersion.FromTag("v0.1.0-beta.1");
        using var rateLimited = new HttpClient(new Handler(_ => new(HttpStatusCode.Forbidden)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => ReleaseDiscovery.Discover(rateLimited, installed, ReleaseIdentity.UpdateRepository, "linux-x64", default));
        using var oversized = new HttpClient(new Handler(_ => new(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[ReleaseDiscovery.MaximumResponseBytes + 1]) }));
        await Assert.ThrowsAsync<InvalidDataException>(() => ReleaseDiscovery.Discover(oversized, installed, ReleaseIdentity.UpdateRepository, "linux-x64", default));
        using var offline = new HttpClient(new AsyncHandler(_ => Task.FromException<HttpResponseMessage>(new HttpRequestException("offline"))));
        await Assert.ThrowsAsync<HttpRequestException>(() => ReleaseDiscovery.Discover(offline, installed, ReleaseIdentity.UpdateRepository, "linux-x64", default));
    }

    [Fact]
    public async Task ManualCheckSerializesRequestsAndOpensOnlyValidatedUrl()
    {
        ReleaseIdentity identity = ReleaseIdentity.Create("v0.1.0-beta.1", new string('a', 40), "windows-x64", false, null);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = new HttpClient(new AsyncHandler(async token => { entered.SetResult(); return await release.Task.WaitAsync(token); }));
        string? opened = null;
        using var checker = new ManualUpdateChecker(identity, "windows-x64", client, url => { opened = url; return true; });
        Task<UpdateCheckStatus> first = checker.Check();
        await entered.Task;
        Assert.Equal(UpdateCheckState.Busy, (await checker.Check()).State);
        release.SetResult(new(HttpStatusCode.OK) { Content = new StringContent(Page(Release("v0.2.0-beta.1")), Encoding.UTF8, "application/json") });
        Assert.Equal(UpdateCheckState.Available, (await first).State);
        UpdateCheckStatus handoff = checker.OpenUpdate();
        Assert.Equal(UpdateCheckState.Opened, handoff.State);
        Assert.Equal("https://github.com/seraph1nia/odot-game/releases/download/v0.2.0-beta.1/odot-0.2.0-beta.1-windows-x64-setup.exe", opened);
        using var failureClient = new HttpClient(new Handler(_ => new(HttpStatusCode.OK) { Content = new StringContent(Page(Release("v0.2.0-beta.1")), Encoding.UTF8, "application/json") }));
        using var failure = new ManualUpdateChecker(identity, "windows-x64", failureClient, _ => false);
        Assert.Equal(UpdateCheckState.Available, (await failure.Check()).State);
        Assert.Equal(UpdateCheckState.Failed, failure.OpenUpdate().State);
    }

    [Theory]
    [InlineData("windows-x64", false, "Download opened. Close The Common Watch, then run the installer.")]
    [InlineData("linux-x64", true, "Release page opened. Close The Common Watch, then run the versioned install script.")]
    public async Task PublicUpdateCopyRetainsLegacyRequestAndAssetIdentity(string target, bool linux, string openedMessage)
    {
        ReleaseIdentity identity = ReleaseIdentity.Create("v0.1.0-beta.1", new string('c', 40), target, false, null);
        using var client = new HttpClient(new Handler(request =>
        {
            Assert.Equal("Odot-update-check", request.Headers.UserAgent.ToString());
            Assert.StartsWith("https://api.github.com/repos/seraph1nia/odot-game/releases?", request.RequestUri!.AbsoluteUri);
            return new(HttpStatusCode.OK) { Content = new StringContent(Page(Release("v0.2.0-beta.1", linux: linux)), Encoding.UTF8, "application/json") };
        }));
        using var checker = new ManualUpdateChecker(identity, target, client, _ => true);
        UpdateCheckStatus available = await checker.Check();
        Assert.Equal(UpdateCheckState.Available, available.State);
        Assert.Equal("The Common Watch 0.2.0-beta.1 is available.", available.Message);
        Assert.Equal(openedMessage, checker.OpenUpdate().Message);

        using var currentClient = new HttpClient(new Handler(_ => new(HttpStatusCode.OK) { Content = new StringContent(Page(Release("v0.1.0-beta.1", linux: linux)), Encoding.UTF8, "application/json") }));
        using var current = new ManualUpdateChecker(identity, target, currentClient, _ => true);
        UpdateCheckStatus upToDate = await current.Check();
        Assert.Equal(UpdateCheckState.UpToDate, upToDate.State);
        Assert.Equal("The Common Watch is up to date.", upToDate.Message);
    }

    [Fact]
    public async Task DisposalCancelsAnOwnedRequestWithoutAResultRace()
    {
        ReleaseIdentity identity = ReleaseIdentity.Create("v0.1.0-beta.1", new string('b', 40), "linux-x64", false, null);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var client = new HttpClient(new AsyncHandler(async token => { entered.SetResult(); await Task.Delay(Timeout.Infinite, token); return new(); }));
        var checker = new ManualUpdateChecker(identity, "linux-x64", client, _ => true);
        Task<UpdateCheckStatus> pending = checker.Check();
        await entered.Task;
        checker.Dispose();
        Assert.Equal(UpdateCheckState.Failed, (await pending).State);
    }

    private static string Page(params string[] releases) => "[" + string.Join(',', releases) + "]";
    private static string Release(string tag, bool draft = false, bool linux = false, bool omitAsset = false, bool foreign = false)
    {
        string version = tag[1..];
        string repository = ReleaseIdentity.UpdateRepository;
        string page = $"https://github.com/{repository}/releases/tag/{tag}";
        string[] names = linux ? [$"odot-{version}-linux-x64.tar.gz", $"odot-{version}-linux-x64-install.sh"] : [$"odot-{version}-windows-x64-setup.exe"];
        if (omitAsset) names = [];
        string assets = string.Join(',', names.Select(name => $$"""{"name":"{{name}}","browser_download_url":"https://github.com/{{(foreign ? "someone/else" : repository)}}/releases/download/{{tag}}/{{name}}"}"""));
        bool preview = version.Contains('-');
        return $$"""{"tag_name":"{{tag}}","draft":{{draft.ToString().ToLowerInvariant()}},"prerelease":{{preview.ToString().ToLowerInvariant()}},"html_url":"{{page}}","assets":[{{assets}}]}""";
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(response(request));
    }
    private sealed class AsyncHandler(Func<CancellationToken, Task<HttpResponseMessage>> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => response(cancellationToken);
    }
}
