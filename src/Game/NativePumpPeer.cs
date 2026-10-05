using Game.Core;
using Godot;

namespace Game;

// Same reliable channel separation and payload codec as Main; test tree only.
internal sealed partial class NativePumpPeer : Node
{
    internal OwnedTimingTrace Trace = null!;
    internal readonly List<(long Revision, string Digest, long Cursor)> Snapshots = [];
    internal readonly List<long> Receipts = [];
    internal readonly List<long> ReceivedAt = [];
    internal readonly CombatPlayback Playback = new();
    internal MatchSnapshot? Applied;
    internal AuthoritySession? Session;
    internal MatchSnapshot? WelcomeState;
    internal int StablePlayer;
    internal int Welcomes;
    private string _credential = "";
    private string _attempt = "";
    internal readonly List<CommandResult> Results = [];
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Hello(int version, string credential, string match, string attempt)
    {
        if (Session is null) return;
        AdmissionResult admission = Session.Admit(Multiplayer.GetRemoteSenderId(), version, credential, Time.GetTicksMsec(), expectedMatchId: match);
        if (!admission.Accepted) throw new InvalidOperationException("Native control admission rejected: " + admission.Message);
        RpcId(Multiplayer.GetRemoteSenderId(), MethodName.Welcome, admission.PlayerId, admission.Credential, SnapshotPayload.Encode(admission.State!), false, 0, attempt);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Welcome(int player, string credential, string payload, bool mayStart, int host, string attempt)
    {
        if (attempt != _attempt) throw new InvalidOperationException("Native control stale welcome.");
        WelcomeState = SnapshotPayload.Decode(payload)!; _credential = credential; StablePlayer = player; Welcomes++;
        Trace.Record("control-welcome", WelcomeState, info: new { Player = player, Attempt = Welcomes });
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Request(string json)
    {
        if (Session is null) return;
        CommandResult result = Session.Request(Multiplayer.GetRemoteSenderId(), json, Time.GetTicksMsec()) ?? throw new InvalidOperationException("Unauthenticated native request.");
        RpcId(Multiplayer.GetRemoteSenderId(), MethodName.Acknowledged, System.Text.Json.JsonSerializer.Serialize(result, WireJson.Options), SnapshotPayload.Encode(Session.Snapshot()));
    }
    internal void Join(string match)
    {
        _attempt = Guid.NewGuid().ToString("N");
        RpcId(1, MethodName.Hello, WireJson.ProtocolVersion, _credential, match, _attempt);
    }
    internal void SendRequest(Command command) => RpcId(1, MethodName.Request, System.Text.Json.JsonSerializer.Serialize(command, WireJson.Options));
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 1)]
    private void Snapshot(string payload)
    {
        long received = OwnedTimingTrace.Now();
        string digest = OwnedTimingTrace.Digest(payload);
        Trace.Record("control-rpc-receive", Applied, digest);
        MatchSnapshot state = SnapshotPayload.Decode(payload)!;
        Trace.Record("control-decode-complete", state, digest);
        if (WelcomeState is not null && state.MatchId != WelcomeState.MatchId) throw new InvalidOperationException("Native snapshot crossed session identity.");
        Snapshots.Add((state.Revision, digest, state.EventSequence)); ReceivedAt.Add(received);
        Applied = state; Playback.Accept(state);
        Trace.Record("control-state-applied", state, digest);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Acknowledged(string result, string payload)
    {
        CommandResult receipt = System.Text.Json.JsonSerializer.Deserialize<CommandResult>(result, WireJson.Options)!;
        MatchSnapshot state = SnapshotPayload.Decode(payload)!;
        Receipts.Add(receipt.Sequence); Results.Add(receipt);
        Trace.Record("control-ack", state, OwnedTimingTrace.Digest(payload), new { receipt.Sequence, receipt.Accepted });
    }
    internal void Publish(int peer, string payload, CommandResult? result)
    {
        RpcId(peer, MethodName.Snapshot, payload);
        if (result is not null) RpcId(peer, MethodName.Acknowledged, System.Text.Json.JsonSerializer.Serialize(result, WireJson.Options), payload);
    }
}
