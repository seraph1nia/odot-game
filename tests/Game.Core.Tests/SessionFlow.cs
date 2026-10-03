using System.Text.Json;
using DevRunner;
using Game.Core;
using Xunit;

namespace Game.Core.Tests;

// Ordinary requests and fixed steps only. Time is explicit and independent of ticks.
internal sealed class SessionFlow : IDisposable
{
    private readonly Dictionary<int, long> _sequences = [];
    private readonly Dictionary<int, (int Peer, string Credential)> _players = [];
    public AuthoritySession Session { get; }
    public ulong Time { get; private set; }
    public MatchSnapshot State => Session.Snapshot();

    public SessionFlow(AuthorityPolicy policy = AuthorityPolicy.PlayingHost, int players = 2, ulong seed = 1)
    {
        Session = new(policy, combatSeed: seed);
        for (int index = policy == AuthorityPolicy.Dedicated ? 0 : 1; index < players; index++)
        {
            int peer = index + 2;
            AdmissionResult admitted = Session.Admit(peer, WireJson.ProtocolVersion, "", AdvanceClock());
            Assert.True(admitted.Accepted, admitted.Message);
            _players.Add(admitted.PlayerId, (peer, admitted.Credential));
        }
    }

    public ulong AdvanceClock() => Time = checked(Time + 100);
    public Command Command(int player, EconomyAction action, MatchSnapshot? before = null)
    {
        long sequence = _sequences.GetValueOrDefault(player) + 1;
        _sequences[player] = sequence;
        return action.Command(before ?? State, player, sequence);
    }
    public CommandResult Send(int player, Command command)
    {
        string payload = JsonSerializer.Serialize(command, WireJson.Options);
        return player == Session.LocalPlayerId ? Session.RequestLocal(payload)
            : Session.Request(_players[player].Peer, payload, AdvanceClock())!;
    }
    public Command Act(int player, EconomyAction action, MatchSnapshot? before = null)
    {
        Command command = Command(player, action, before);
        Assert.True(Send(player, command).Accepted, $"P{player}: {action}");
        return command;
    }
    public void Disconnect(int player) => Assert.Equal(player, Session.Disconnect(_players[player].Peer));
    public void Rebind(int player, int peer)
    {
        AdmissionResult restored = Session.Admit(peer, WireJson.ProtocolVersion, _players[player].Credential, AdvanceClock());
        Assert.True(restored.Accepted, restored.Message);
        Assert.Equal(player, restored.PlayerId);
        _players[player] = (peer, restored.Credential);
    }
    public Command? Invest(int player, string family = "frontline")
    {
        Command? recruitment = null;
        for (int actions = 0; actions < 100; actions++)
        {
            MatchSnapshot before = State;
            EconomyAction? decision = CampaignStrategy.Next(before, player, family);
            if (decision is null) return recruitment;
            Command command = Act(player, decision, before);
            if (decision.Action == "recruit") recruitment = command;
        }
        throw new InvalidOperationException("Economy policy exceeded its action bound.");
    }
    public void Prepare()
    {
        for (int turn = 0; turn < 5 && Session.Phase is Phase.Building or Phase.Preparation; turn++)
        {
            foreach (CityState city in State.Players.Where(c => c.Connected && !c.Eliminated && !c.Ready))
            {
                Invest(city.Id);
                Act(city.Id, new("ready"));
            }
            Session.Step();
        }
        Assert.Equal(Phase.Combat, Session.Phase);
    }
    public MatchSnapshot StepUntil(Func<MatchSnapshot, bool> predicate, int maximum = 10000)
    {
        for (int step = 0; step <= maximum; step++)
        {
            MatchSnapshot state = State;
            if (predicate(state)) return state;
            if (step < maximum) Session.Step();
        }
        throw new InvalidOperationException($"Flow exceeded {maximum} ticks at wave {State.Wave}, tick {State.Tick}.");
    }
    public static MatchSnapshot RoundTrip(MatchSnapshot state)
    {
        MatchSnapshot copy = Game.SnapshotPayload.Decode(Game.SnapshotPayload.Encode(state))!;
        Assert.Equal(JsonSerializer.Serialize(state, WireJson.Options), JsonSerializer.Serialize(copy, WireJson.Options));
        return copy;
    }
    public static string Gameplay(MatchSnapshot state) => JsonSerializer.Serialize(state with
    {
        Revision = 0,
        Players = state.Players.Select(c => c with { Connected = false, Ready = false }).ToArray()
    }, WireJson.Options);
    public void Dispose() => Session.Dispose();
}
