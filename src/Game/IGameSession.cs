using Game.Core;

namespace Game;

// Presentation reads accepted state and submits actions; it never owns authority.
public interface IGameSession
{
    MatchSnapshot? State { get; }
    int PlayerId { get; }
    bool Connected { get; }
    string Status { get; }
    string Feedback { get; }
    bool CanStart { get; }
    bool CanInvite { get; }
    int HostPlayerId { get; }
    long SendAction(string action, int slot = -1, Building building = Building.Empty, int city = 0);
    void Connect(bool fresh = false);
}
