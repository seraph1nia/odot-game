using Game.Core;

namespace Game;

internal interface ISteamFriends
{
    SteamFriend[] Friends();
    bool IsFriend(ulong id);
    bool SendInvite(ulong lobby, ulong recipient);
}

// Shared by live Steam and explicit offline UI fixtures. No transport abstraction.
internal sealed class SteamFriendInvitations(GameApplication application, ISteamFriends friends, Func<SteamInviteContext> context)
{
    private readonly SteamInvitationPicker _picker = new();
    private long _ticket;

    internal void Open()
    {
        SteamInviteContext current = context();
        if (!current.Hosting || current.Lobby == 0) return;
        application.Friends.Close();
        _ticket = _picker.Open(current);
        long ticket = _ticket;
        application.Friends.Open(() => Refresh(ticket), id => Send(ticket, id), _picker.Close);
    }

    private SteamFriend[] Refresh(long ticket)
    {
        SteamInviteContext current = context();
        if (!_picker.IsCurrent(ticket, current) || !current.LoggedIn)
            throw new InvalidOperationException("Open Steam and log in, then refresh to invite friends.");
        SteamFriend[] list = friends.Friends();
        _picker.SetFriends(ticket, list);
        return list;
    }

    private string Send(long ticket, ulong id)
    {
        SteamInviteResult result;
        try { result = _picker.Send(ticket, id, context(), friends.IsFriend, friends.SendInvite); }
        catch (Exception) { result = SteamInviteResult.Failed; }
        // No names, account IDs or credentials in shared diagnostics.
        Main.Emit(new("steam-invite", Message: result.ToString()));
        return result switch
        {
            SteamInviteResult.Sent => "Invitation sent. Waiting for your friend to join.",
            SteamInviteResult.Unavailable => "Open Steam and log in, then refresh to invite friends.",
            SteamInviteResult.NotFriend => "This friend is no longer available. Refresh and try again.",
            SteamInviteResult.Stale => "This game has ended or changed. Close invitations and host again.",
            SteamInviteResult.Busy => "An invitation is already being sent.",
            _ => "Steam could not send the invitation. Try again."
        };
    }

    internal void Process()
    {
        if (!application.Friends.IsOpen) return;
        SteamInviteContext current = context();
        if (!_picker.IsCurrent(_ticket, current)) Close();
        else application.Friends.SetAvailable(current.LoggedIn);
    }

    internal void Close() { _picker.Close(); application.Friends.Close(); }
}
