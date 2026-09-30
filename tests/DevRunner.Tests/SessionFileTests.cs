using System.Text.Json;
using Game;
using Xunit;

namespace DevRunner.Tests;

public sealed class SessionFileTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "odot-session-test-" + Guid.NewGuid().ToString("N"));
    private string PathName => Path.Combine(_directory, "session.json");

    [Fact]
    public void LegacyEnetFileResumesOnlyItsEndpoint()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(PathName, "{\"Endpoint\":\"localhost:7000\",\"MatchId\":\"match-a\",\"Token\":\"private-test-token\",\"NextSequence\":8}");
        var same = new SessionFile(PathName, "localhost:7000"); same.Load();
        Assert.Equal("match-a", same.Value?.MatchId); Assert.Equal(8, same.Reserve());
        var foreign = new SessionFile(PathName, "localhost:7001"); foreign.Load(); Assert.Null(foreign.Value);
        var steam = new SessionFile(PathName, "localhost:7000", "steam", "480", "original-host"); steam.Load(); Assert.Null(steam.Value);
    }

    [Theory]
    [InlineData("enet", "480", "host-a", "lobby-a")]
    [InlineData("steam", "123456", "host-a", "lobby-a")]
    [InlineData("steam", "480", "host-b", "lobby-a")]
    [InlineData("steam", "480", "host-a", "lobby-b")]
    public void SteamCredentialsCannotCrossAnyNamespace(string transport, string application, string host, string endpoint)
    {
        var owner = new SessionFile(PathName, "lobby-a", "steam", "480", "host-a"); owner.Welcome("match-a", "private-test-token");
        var foreign = new SessionFile(PathName, endpoint, transport, application, host); foreign.Load();
        Assert.Null(foreign.Value);
        Assert.Contains("private-test-token", File.ReadAllText(PathName));
    }

    [Fact]
    public void NewMatchResetsSequenceAndDiagnosticsOmitPrivateCredential()
    {
        var store = new SessionFile(PathName, "lobby-a", "steam", "480", "host-a"); store.Welcome("match-a", "private-test-token");
        Assert.Equal(1, store.Reserve()); store.Welcome("match-a", "private-test-token"); Assert.Equal(2, store.Reserve());
        store.Welcome("match-b", "new-private-test-token"); Assert.Equal(1, store.Reserve());
        Assert.DoesNotContain("private-test-token", store.Value!.ToString());
        Assert.Equal("match-b", JsonSerializer.Deserialize<LocalSession>(File.ReadAllText(PathName))!.MatchId);
    }

    public void Dispose() { if (Directory.Exists(_directory)) Directory.Delete(_directory, true); }
}
