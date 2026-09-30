using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class SteamLobbyRulesTests
{
    private static Dictionary<string, string> Lobby() => SteamLobbyRules.Advertisement(480, 42, Guid.NewGuid().ToString("N"), false);

    [Fact]
    public void DiscoveryPublishesOnlyGameProtocolAndSessionRoutingData()
    {
        Dictionary<string, string> metadata = Lobby();
        Assert.Equal(SteamLobbyRules.MetadataKeys.Order(StringComparer.Ordinal), metadata.Keys.Order(StringComparer.Ordinal));
        Assert.True(SteamLobbyRules.TryReadHost(metadata, 480, 42, out ulong host, out string match, out _));
        Assert.Equal(42UL, host); Assert.Equal(metadata["match"], match);
        metadata["state"] = "in-progress";
        Assert.True(SteamLobbyRules.TryReadHost(metadata, 480, 42, out _, out _, out _));
    }

    [Theory]
    [InlineData("game", "different-game")]
    [InlineData("protocol", "-1")]
    [InlineData("application", "123")]
    [InlineData("original_host", "0")]
    [InlineData("match", "not-a-match")]
    [InlineData("state", "ended")]
    public void ForeignMalformedOrEndedInvitationsNeverAuthorizeNativeConnection(string key, string value)
    {
        Dictionary<string, string> metadata = Lobby(); metadata[key] = value;
        Assert.False(SteamLobbyRules.TryReadHost(metadata, 480, 42, out ulong host, out string match, out string feedback));
        Assert.Equal(0UL, host); Assert.Empty(match); Assert.NotEmpty(feedback);
    }

    [Fact]
    public void SteamLobbyOwnerReassignmentCannotMigrateTheOriginalAuthority()
    {
        Dictionary<string, string> metadata = Lobby();
        Assert.False(SteamLobbyRules.TryReadHost(metadata, 480, 99, out _, out _, out _));
        Assert.Equal("42", metadata["original_host"]);
        Assert.False(SteamLobbyRules.TryReadHost(metadata, 481, 42, out _, out _, out _));
    }

    [Fact]
    public void LaunchArgumentsDeduplicateTargetsAndIgnoreMalformedValues()
    {
        Assert.Equal([123UL], SteamLobbyRules.InvitationTargets(["+connect_lobby", "123", "+connect_lobby", "123"]));
        Assert.Empty(SteamLobbyRules.InvitationTargets(["+connect_lobby", "0", "+connect_lobby", "-1", "+connect_lobby", "bad", "+connect_lobby"]));
    }
}
