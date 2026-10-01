using Game.Core;
using Xunit;

namespace Game.Core.Tests;

public sealed class SteamInvitationPickerTests
{
    private static readonly SteamInviteContext Host = new(42, 7, true, true);
    private static readonly SteamFriend[] Friends = [new(101, "Sam", "Online", true), new(202, "Sam", "Offline", false)];

    [Fact]
    public void DuplicateNamesSendOnlyToSelectedIdAndNativeResultDoesNotAdmitAnyone()
    {
        var picker = new SteamInvitationPicker(); long ticket = picker.Open(Host);
        Assert.True(picker.SetFriends(ticket, Friends));
        var calls = new List<(ulong Lobby, ulong Recipient)>();
        Assert.Equal(SteamInviteResult.Sent, picker.Send(ticket, 202, Host, _ => true,
            (lobby, recipient) => { calls.Add((lobby, recipient)); return true; }));
        Assert.Equal([(42UL, 202UL)], calls);
        Assert.Equal(SteamInviteResult.Failed, picker.Send(ticket, 101, Host, _ => true, (_, _) => false));
        Assert.Equal(SteamInviteResult.Sent, picker.Send(ticket, 101, Host, _ => true, (_, _) => true));
    }

    [Theory]
    [InlineData(43UL, 7L, true, true, SteamInviteResult.Stale)]
    [InlineData(42UL, 8L, true, true, SteamInviteResult.Stale)]
    [InlineData(42UL, 7L, false, true, SteamInviteResult.Stale)]
    [InlineData(42UL, 7L, true, false, SteamInviteResult.Unavailable)]
    public void ChangedSessionOrLoginCannotSend(ulong lobby, long generation, bool hosting, bool loggedIn, SteamInviteResult expected)
    {
        var picker = new SteamInvitationPicker(); long ticket = picker.Open(Host); picker.SetFriends(ticket, Friends);
        Assert.Equal(expected, picker.Send(ticket, 101, new(lobby, generation, hosting, loggedIn), _ => true,
            (_, _) => throw new InvalidOperationException("Must not send")));
    }

    [Fact]
    public void OldOpeningCannotSendEvenWhenReopenedForSameLobbyAndGeneration()
    {
        var picker = new SteamInvitationPicker(); long old = picker.Open(Host); picker.SetFriends(old, Friends);
        picker.Close(); long current = picker.Open(Host); picker.SetFriends(current, Friends);
        Assert.False(picker.SetFriends(old, Friends));
        Assert.Equal(SteamInviteResult.Stale, picker.Send(old, 101, Host, _ => true, (_, _) => throw new InvalidOperationException()));
    }

    [Fact]
    public void RefreshedOrRemovedFriendIsRejectedBeforeNativeSend()
    {
        var picker = new SteamInvitationPicker(); long ticket = picker.Open(Host); picker.SetFriends(ticket, Friends);
        Assert.Equal(SteamInviteResult.NotFriend, picker.Send(ticket, 101, Host, _ => false, (_, _) => throw new InvalidOperationException()));
        picker.SetFriends(ticket, [Friends[1]]);
        Assert.Equal(SteamInviteResult.NotFriend, picker.Send(ticket, 101, Host, _ => true, (_, _) => throw new InvalidOperationException()));
        Assert.Equal(SteamInviteResult.NotFriend, picker.Send(ticket, 0, Host, _ => true, (_, _) => throw new InvalidOperationException()));
    }

    [Fact]
    public void ReentrantActivationSendsOnceAndExceptionAllowsRetry()
    {
        var picker = new SteamInvitationPicker(); long ticket = picker.Open(Host); picker.SetFriends(ticket, Friends);
        int calls = 0;
        Assert.Equal(SteamInviteResult.Sent, picker.Send(ticket, 101, Host, _ => true, (_, _) =>
        {
            calls++;
            Assert.Equal(SteamInviteResult.Busy, picker.Send(ticket, 202, Host, _ => true, (_, _) => throw new InvalidOperationException()));
            return true;
        }));
        Assert.Equal(1, calls);
        Assert.Throws<InvalidOperationException>(() => picker.Send(ticket, 101, Host, _ => true, (_, _) => throw new InvalidOperationException()));
        Assert.Equal(SteamInviteResult.Sent, picker.Send(ticket, 101, Host, _ => true, (_, _) => true));
    }

    [Fact]
    public void ClosingDuringFriendshipQueryInvalidatesTheSend()
    {
        var picker = new SteamInvitationPicker(); long ticket = picker.Open(Host); picker.SetFriends(ticket, Friends);
        Assert.Equal(SteamInviteResult.Stale, picker.Send(ticket, 101, Host, _ => { picker.Close(); return true; },
            (_, _) => throw new InvalidOperationException()));
    }
}
