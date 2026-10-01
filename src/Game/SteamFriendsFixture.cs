using Game.Core;

namespace Game;

// Constructed only behind Main's owned, Steam-disabled UI-worker guard.
internal sealed class SteamFriendsFixture : ISteamFriends
{
    internal string Mode { get; set; } = "list";
    internal bool LoggedIn => Mode != "unavailable";
    public SteamFriend[] Friends()
    {
        if (!LoggedIn) throw new InvalidOperationException("Offline fixture");
        if (Mode == "empty") return [];
        return [new(101, "Sam", "Online", true), new(202, "Sam", "Online", true),
            .. Enumerable.Range(0, 30).Select(index => new SteamFriend((ulong)(300 + index), "Friend with a long display name " + index, "Offline", false))];
    }
    public bool IsFriend(ulong id) => Friends().Any(friend => friend.Id == id);
    public bool SendInvite(ulong lobby, ulong recipient)
    {
        Main.Emit(new("ui-friend-invite", Message: $"{lobby}:{recipient}"));
        return Mode != "failure";
    }
}
