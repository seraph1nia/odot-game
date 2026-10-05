using System.Text.Json;
using Game.Core;
using Godot;
using Environment = System.Environment;

namespace Game;

internal static class NativePumpControl
{
    internal static async Task Run(Node owner)
    {
        try
        {
            var ownership = new VerificationPacing(Environment.GetEnvironmentVariable("ODOT_OWNED_DATA"),
                Environment.GetEnvironmentVariable("ODOT_VERIFICATION_MARKER"), Environment.GetEnvironmentVariable("ODOT_VERIFICATION_TOKEN"));
            string evidence = Environment.GetEnvironmentVariable("ODOT_PUMP_EVIDENCE") ?? throw new InvalidOperationException("Missing pump evidence.");
            if (!ownership.Owned || DisplayServer.GetName() != "headless") throw new InvalidOperationException("Native pump control requires owned headless execution.");
            int port = int.Parse(Environment.GetEnvironmentVariable("ODOT_PUMP_PORT")!, System.Globalization.CultureInfo.InvariantCulture);
            using Match match = Prepared();
            var states = new List<MatchSnapshot>();
            for (int index = 0; index < 80; index++) { for (int step = 0; step < 4; step++) match.Step(); states.Add(match.Snapshot()); }
            string[] payloads = states.Select(s => SnapshotPayload.Encode(s)).ToArray();
            var reports = new List<object>(); var proofs = new List<(bool Candidate, int Delay, int Count, double Median, double Maximum, string[] Digests)>();
            owner.GetTree().MultiplayerPoll = false; // ONLY this isolated control tree.
            foreach (int delay in new[] { 0, 350 }) reports.Add(await Mode(owner, port, evidence, payloads, states, delay, proofs));
            reports.Add(await Mode(owner, port, evidence, payloads, states, 350, proofs, candidate: true));
            // Cancellation stops new admission, drains every already-issued
            // record/receipt, then closes native owners in Mode's finally.
            reports.Add(await Mode(owner, port, evidence, payloads[..8], states.Take(8).ToList(), 350, proofs, cancelAfterAdmission: true, candidate: true));
            var original = proofs.Single(p => !p.Candidate && p.Delay == 350); var corrected = proofs.Single(p => p.Candidate && p.Count == 80);
            if (!original.Digests.SequenceEqual(corrected.Digests) || corrected.Median >= original.Median / 2 || corrected.Maximum >= original.Maximum / 2)
                throw new InvalidOperationException("Matched candidate receive correction failed: " + JsonSerializer.Serialize(new { original.Median, original.Maximum, CandidateMedian = corrected.Median, CandidateMaximum = corrected.Maximum, SameDigests = original.Digests.SequenceEqual(corrected.Digests), Reports = reports }, WireJson.Options));
            Main.Emit(new("native-pump", Message: JsonSerializer.Serialize(new { ReceiveCorrectionProven = true, Reports = reports, Limitation = "Headless native transport/application/playback control, not real renderer or game acceptance." }, WireJson.Options)));
            owner.GetTree().Quit();
        }
        catch (Exception error) { Main.Emit(new("error", Message: error.ToString())); owner.GetTree().Quit(1); }
    }
    // Selectable native timeout counterfactual, not a receive-service correction.
    internal static async Task Liveness(Node owner)
    {
        bool automatic = owner.GetTree().MultiplayerPoll;
        try
        {
            var ownership = new VerificationPacing(Environment.GetEnvironmentVariable("ODOT_OWNED_DATA"),
                Environment.GetEnvironmentVariable("ODOT_VERIFICATION_MARKER"), Environment.GetEnvironmentVariable("ODOT_VERIFICATION_TOKEN"));
            string evidence = Environment.GetEnvironmentVariable("ODOT_PUMP_EVIDENCE") ?? throw new InvalidOperationException("Missing pump evidence.");
            if (!ownership.Owned || DisplayServer.GetName() != "headless") throw new InvalidOperationException("Native liveness control requires owned headless execution.");
            int port = int.Parse(Environment.GetEnvironmentVariable("ODOT_PUMP_PORT")!, System.Globalization.CultureInfo.InvariantCulture);
            string matchId = Guid.NewGuid().ToString("N");
            owner.GetTree().MultiplayerPoll = false; // One manual owner per original API, only this isolated test tree.
            var reports = new List<(string Digest, object Report)>();
            // Same native traffic and 15s withheld guest service; only SetTimeout varies.
            reports.Add(await LivenessMode(owner, port, evidence, matchId, 10000, 15000, true));
            reports.Add(await LivenessMode(owner, port, evidence, matchId, 60000, 15000, true));
            // Disconfirming no-stall controls and explicit failure-case policy.
            reports.Add(await LivenessMode(owner, port, evidence, matchId, 10000, 500, false));
            reports.Add(await LivenessMode(owner, port, evidence, matchId, 60000, 500, false));
            reports.Add(await LivenessMode(owner, port, evidence, matchId, 1000, 2500, true));
            if (reports.Select(report => report.Digest).Distinct().Count() != 1)
                throw new InvalidOperationException("Native liveness arms did not publish matched baseline traffic.");
            Main.Emit(new("native-pump", Message: JsonSerializer.Serialize(new
            {
                FixtureLivenessAlignmentProven = true,
                Reports = reports.Select(report => report.Report).ToArray(),
                Limitation = "Injected headless service withholding proves native fixture timeout alignment, not hosted stall duration, service root cause or graphical Start acceptance."
            }, WireJson.Options)));
            owner.GetTree().Quit();
        }
        catch (Exception error) { Main.Emit(new("error", Message: error.ToString())); owner.GetTree().Quit(1); }
        finally { owner.GetTree().MultiplayerPoll = automatic; }
    }

    private static async Task<(string Digest, object Report)> LivenessMode(Node owner, int port, string evidence, string matchId, int timeout, int duration, bool withhold)
    {
        using var trace = new OwnedTimingTrace(Path.Combine(evidence, $"native-liveness-{timeout}-{withhold}.jsonl"), true);
        using var serverPeer = new ENetMultiplayerPeer(); using var guestPeer = new ENetMultiplayerPeer();
        using var serverApi = new SceneMultiplayer(); using var guestApi = new SceneMultiplayer();
        var serverRoot = new Node { Name = "LivenessServer" }; var guestRoot = new Node { Name = "LivenessGuest" };
        owner.AddChild(serverRoot); owner.AddChild(guestRoot);
        owner.GetTree().SetMultiplayer(serverApi, serverRoot.GetPath()); owner.GetTree().SetMultiplayer(guestApi, guestRoot.GetPath());
        using var session = new AuthoritySession(AuthorityPolicy.Dedicated, matchId: matchId, combatSeed: 1);
        var server = new NativePumpPeer { Name = "Endpoint", Trace = trace, Session = session }; var guest = new NativePumpPeer { Name = "Endpoint", Trace = trace };
        serverRoot.AddChild(server); guestRoot.AddChild(guest);
        long started = OwnedTimingTrace.Now(), limit = started + 20_000_000_000;
        long disconnectedAt = 0; int disconnected = 0, guestDisconnected = 0;
        int stablePeer = 0; long serverPolls = 0, guestPolls = 0;
        serverApi.PeerDisconnected += id => { disconnected++; disconnectedAt = OwnedTimingTrace.Now(); trace.Record("liveness-authority-disconnected", session.Snapshot(), info: new { Peer = id }); session.Disconnect((int)id); };
        guestApi.ServerDisconnected += () => { guestDisconnected++; trace.Record("liveness-guest-disconnected"); };
        void Poll(bool serviceGuest)
        {
            if (OwnedTimingTrace.Now() > limit) throw new TimeoutException("Native liveness mode exceeded its finite 20s bound.");
            if (serverApi.Poll() != Error.Ok) throw new InvalidOperationException("Authority poll failed.");
            serverPolls++;
            if (serviceGuest) { if (guestApi.Poll() != Error.Ok) throw new InvalidOperationException("Guest poll failed."); guestPolls++; }
        }
        async Task Tick(bool serviceGuest) { Poll(serviceGuest); await owner.ToSignal(owner.GetTree().CreateTimer(.01), SceneTreeTimer.SignalName.Timeout); }
        async Task Until(Func<bool> condition) { while (!condition()) await Tick(true); }
        try
        {
            if (serverPeer.CreateServer(port, 1) != Error.Ok) throw new InvalidOperationException("Owned liveness bind failed; existing endpoint left intact.");
            serverApi.MultiplayerPeer = serverPeer;
            if (guestPeer.CreateClient("127.0.0.1", port) != Error.Ok) throw new InvalidOperationException("Liveness connection failed.");
            guestApi.MultiplayerPeer = guestPeer;
            await Until(() => serverApi.GetPeers().Length == 1 && guestApi.GetUniqueId() > 1);
            stablePeer = guestApi.GetUniqueId();
            using (ENetPacketPeer connection = serverPeer.GetPeer(stablePeer)) connection.SetTimeout(32, Math.Max(1, timeout / 2), timeout);
            using (ENetPacketPeer connection = guestPeer.GetPeer(1)) connection.SetTimeout(32, Math.Max(1, timeout / 2), timeout);
            guest.Join(matchId); await Until(() => guest.Welcomes == 1);
            Command start = Command.FromSnapshot(guest.WelcomeState!, 2000, "start", guest.StablePlayer);
            guest.SendRequest(start); await Until(() => guest.Results.Count == 1);
            CommandResult accepted = guest.Results.Single();
            if (!accepted.Accepted || session.Snapshot().Phase != Phase.Building) throw new InvalidOperationException("Ordinary native Start was not admitted.");
            MatchSnapshot state = session.Snapshot(); string payload = SnapshotPayload.Encode(state);
            var record = (state.Revision, OwnedTimingTrace.Digest(payload), state.EventSequence);
            server.Publish(stablePeer, payload, null); await Until(() => guest.Snapshots.Count == 1);
            if (guest.Snapshots.Single() != record) throw new InvalidOperationException("Baseline message/session/event cursor changed.");
            // Flush ONLY the guest's outgoing retry. Do not service the authority
            // or deliver its acknowledgment until the matched schedule begins.
            guest.SendRequest(start);
            if (guestApi.Poll() != Error.Ok) throw new InvalidOperationException("Guest retry flush failed.");
            guestPolls++;
            if (guest.Results.Count != 1 || server.Admitted.Count != 1) throw new InvalidOperationException("Retry was not genuinely pending before withholding.");
            server.Publish(stablePeer, payload, null);
            long stallStart = OwnedTimingTrace.Now(), pollsBefore = guestPolls;
            trace.Record("liveness-schedule-begin", state, record.Item2, new { Timeout = timeout, Duration = duration, Withhold = withhold, Peer = stablePeer, start.Sequence });
            while (OwnedTimingTrace.Now() - stallStart < duration * 1_000_000L) await Tick(!withhold);
            long pollsDuring = guestPolls - pollsBefore;
            bool expectDisconnect = withhold && timeout < duration;
            if ((disconnected == 1) != expectDisconnect || disconnected > 1 || (withhold && pollsDuring != 0))
                throw new InvalidOperationException($"Liveness counterfactual contradicted: timeout={timeout}, withheld={withhold}, disconnected={disconnected}, guestPollsDuring={pollsDuring}.");
            if (server.Admitted.Count != 2 || server.Admitted[1].Result != accepted
                || server.Admitted[1].State.MatchId != matchId || server.Admitted[1].State.Revision != state.Revision)
                throw new InvalidOperationException("Pending retry did not reach the same authority ledger/state.");
            trace.Record("liveness-guest-service-resumed", session.Snapshot(), info: new { GuestPollsBeforeResume = guestPolls, AuthorityDisconnected = disconnected });
            if (expectDisconnect)
            {
                // The serviced authority proves native peer loss. The unserviced
                // guest may retain already-sent UDP records; do not invent their
                // loss or require its own independent timeout to have elapsed.
                await Tick(true); await Tick(true);
            }
            else
            {
                await Until(() => guest.Snapshots.Count == 2 && guest.Results.Count == 2);
                if (disconnected != 0 || guestDisconnected != 0 || guestApi.GetUniqueId() != stablePeer || guest.Welcomes != 1)
                    throw new InvalidOperationException("Surviving peer changed connection identity.");
            }
            if (guest.Snapshots.Count is < 1 or > 2 || guest.Results.Count is < 1 or > 2 || guestDisconnected > 1
                || !guest.Snapshots.SequenceEqual(Enumerable.Repeat(record, guest.Snapshots.Count))
                || guest.Results.Any(result => result != accepted)
                || guest.ResultStates.Any(receipt => receipt.MatchId != matchId || receipt.Revision != state.Revision
                    || receipt.EventSequence != state.EventSequence || receipt.Phase != Phase.Building) || trace.Truncated)
                throw new InvalidOperationException("Native delivery changed message/session/event/receipt provenance.");
            int retryReceipts = guest.Results.Count;
            if (!expectDisconnect)
            {
                guest.SendRequest(Command.FromSnapshot(session.Snapshot(), 2001, "pause", guest.StablePlayer));
                await Until(() => guest.Results.Count == 3);
                MatchSnapshot paused = guest.ResultStates[^1];
                if (!guest.Results[^1].Accepted || guest.Results[^1].Sequence != 2001 || !session.Paused
                    || paused.MatchId != matchId || paused.Phase != Phase.Building || !paused.Paused || paused.Revision != session.Snapshot().Revision)
                    throw new InvalidOperationException("Surviving peer could not execute its next ordinary command.");
            }
            return (record.Item2, new
            {
                TimeoutMilliseconds = timeout,
                NativeMinimumMilliseconds = Math.Max(1, timeout / 2),
                TimeoutLimit = 32,
                DurationMilliseconds = duration,
                WithheldService = withhold,
                AuthorityDisconnected = disconnected,
                GuestDisconnected = guestDisconnected,
                DisconnectAfterScheduleMilliseconds = disconnectedAt == 0 ? (double?)null : (disconnectedAt - stallStart) / 1e6,
                Session = matchId,
                Peer = stablePeer,
                StartSequence = start.Sequence,
                Revision = state.Revision,
                EventCursor = state.EventSequence,
                Digest = record.Item2,
                Received = guest.Snapshots.Count,
                Receipts = guest.Results.Count,
                ServerPolls = serverPolls,
                GuestPolls = guestPolls,
                GuestPollsDuringSchedule = pollsDuring,
                PendingBeforeSchedule = true,
                RetryReceiptsAfterResume = retryReceipts,
                PendingConserved = retryReceipts == 2 && guest.Snapshots.Count == 2,
                ReceiptProvenance = guest.Results.Select((result, index) => new
                {
                    result.Sequence,
                    result.Accepted,
                    guest.ResultStates[index].MatchId,
                    guest.ResultStates[index].Revision,
                    guest.ResultStates[index].EventSequence,
                    guest.ResultStates[index].Phase,
                    guest.ResultStates[index].Paused
                }).ToArray(),
                AuthorityProvenance = server.Admitted.Select(receipt => new
                {
                    receipt.Result.Sequence,
                    receipt.Result.Accepted,
                    receipt.State.MatchId,
                    receipt.State.Revision,
                    receipt.State.EventSequence,
                    receipt.State.Phase,
                    receipt.State.Paused
                }).ToArray(),
                ExpectedDisconnect = expectDisconnect,
                Seconds = (OwnedTimingTrace.Now() - started) / 1e9
            });
        }
        finally
        {
            guestPeer.Close(); serverPeer.Close();
            guestApi.MultiplayerPeer = new OfflineMultiplayerPeer(); serverApi.MultiplayerPeer = new OfflineMultiplayerPeer();
            owner.GetTree().SetMultiplayer(null!, guestRoot.GetPath()); owner.GetTree().SetMultiplayer(null!, serverRoot.GetPath());
            guestRoot.Free(); serverRoot.Free(); trace.Record("owned-native-teardown");
        }
    }

    private static Match Prepared()
    {
        var match = new Match(combatSeed: 1); match.Join(); match.Join();
        void Act(int city, string action, int slot = -1, Building building = Building.Empty)
        {
            CommandResult result = match.Apply(city, Command.FromSnapshot(match.Snapshot(), 1, action, city, slot, building));
            if (!result.Accepted) throw new InvalidOperationException(result.Message);
        }
        Act(1, "start"); Act(1, "build", 0, Building.Farm); Act(1, "build", 2, Building.MetalMine); Act(1, "build", 1, Building.Barracks);
        for (int turn = 0; turn < 3; turn++)
        {
            Act(1, "ready"); Act(2, "ready");
            while (match.Players[1].Soldiers.Count < 6 && match.Players[1].Resources.TryPay(match.Economy.Recruitment(UnitType.Swordsman), out _)) Act(1, "recruit", 1);
        }
        Act(1, "ready"); Act(2, "ready"); return match;
    }
    private static async Task<object> Mode(Node owner, int port, string evidence, string[] payloads, List<MatchSnapshot> states, int delay, List<(bool Candidate, int Delay, int Count, double Median, double Maximum, string[] Digests)> proofs, bool cancelAfterAdmission = false, bool candidate = false)
    {
        using var trace = new OwnedTimingTrace(Path.Combine(evidence, cancelAfterAdmission ? "native-pump-cancellation.jsonl" : candidate ? "native-pump-fixed.jsonl" : $"native-pump-{delay}.jsonl"), true);
        using var cancellation = new CancellationTokenSource();
        using var serverPeer = new ENetMultiplayerPeer(); using var guestPeer = new ENetMultiplayerPeer();
        using var serverApi = new SceneMultiplayer(); using var guestApi = new SceneMultiplayer();
        var serverRoot = new Node { Name = "PumpServer" }; var guestRoot = new Node { Name = "PumpGuest" };
        owner.AddChild(serverRoot); owner.AddChild(guestRoot);
        owner.GetTree().SetMultiplayer(serverApi, serverRoot.GetPath()); owner.GetTree().SetMultiplayer(guestApi, guestRoot.GetPath());
        using var session = new AuthoritySession(AuthorityPolicy.Dedicated, matchId: states[0].MatchId);
        var server = new NativePumpPeer { Name = "Endpoint", Trace = trace, Session = session }; var guest = new NativePumpPeer { Name = "Endpoint", Trace = trace };
        serverApi.PeerDisconnected += id => session.Disconnect((int)id);
        serverRoot.AddChild(server); guestRoot.AddChild(guest);
        long started = OwnedTimingTrace.Now(), limit = started + 20_000_000_000;
        var published = new List<(long Revision, string Digest, long Cursor)>(); var publishedAt = new List<long>(); var receipts = new List<long>();
        long pausedServicesStart = 0, pausedServices = 0; bool frameDeduplicated = false;
        var ages = new List<double>(); int index = 0; long nextPublish = 0, nextPoll = 0;
        EnetGuestPump? pump = null; Exception? pumpFailure = null; long generation = 1;
        bool previousPause = owner.GetTree().Paused;
        long candidateServices = 0, suppressed = 0;
        async Task PumpOnce()
        {
            if (OwnedTimingTrace.Now() > limit) throw new TimeoutException("Native pump control exceeded its finite 20s mode bound.");
            if (pumpFailure is not null) throw new InvalidOperationException("Candidate owner failed", pumpFailure);
            serverApi.Poll();
            if (OwnedTimingTrace.Now() >= nextPoll)
            {
                (pump as MultiplayerApi ?? guestApi).Poll();
                nextPoll = OwnedTimingTrace.Now() + delay * 1_000_000L;
            }
            await owner.ToSignal(owner.GetTree().CreateTimer(.01), SceneTreeTimer.SignalName.Timeout);
        }
        async Task Until(Func<bool> condition) { while (!condition()) await PumpOnce(); }
        void Acquire()
        {
            if (!candidate) return;
            pump = EnetGuestPump.Attach(guestRoot, guestApi, guestRoot.GetPath(), () => generation, error => pumpFailure = error, trace);
        }
        void Release()
        {
            if (pump is null) return;
            candidateServices += pump.Services; suppressed += pump.AutomaticAdmissionsSuppressed;
            pump.Release(); pump = null;
            if (owner.GetTree().GetMultiplayer(guestRoot.GetPath()) != guestApi) throw new InvalidOperationException("Guest API did not restore on release.");
        }
        try
        {
            if (serverPeer.CreateServer(port, 1) != Error.Ok) throw new InvalidOperationException("Owned control bind failed; existing endpoint left intact.");
            serverApi.MultiplayerPeer = serverPeer;
            if (guestPeer.CreateClient("127.0.0.1", port) != Error.Ok) throw new InvalidOperationException("Control connection failed.");
            guestApi.MultiplayerPeer = guestPeer; Acquire();
            if (pump is not null)
            {
                bool stale = false, duplicate = false;
                try { pump.Service(generation - 1, Engine.GetPhysicsFrames()); } catch (InvalidOperationException) { stale = true; }
                try { EnetGuestPump.Attach(guestRoot, guestApi, guestRoot.GetPath(), () => generation, error => pumpFailure = error); } catch (InvalidOperationException) { duplicate = true; }
                if (!stale || !duplicate) throw new InvalidOperationException("Candidate stale/double owner guards failed.");

            }
            await Until(() => serverApi.GetPeers().Length > 0 && guestApi.GetUniqueId() > 1);
            guest.Join(session.MatchId); await Until(() => guest.Welcomes == 1);
            int stablePlayer = guest.StablePlayer;
            if (pump is not null)
            {
                long serviced = pump.Services;
                // Repeat an ACTUAL driver-serviced frame, never inject a new
                // native poll from the timer or alter connection admission.
                frameDeduplicated = serviced > 0 && !pump.Service(generation, pump.LastServicedFrame) && pump.Services == serviced;
                if (!frameDeduplicated) throw new InvalidOperationException("Candidate serviced the same physics frame twice.");
            }
            while (guest.Snapshots.Count < payloads.Length || guest.Receipts.Count < payloads.Length / 8)
            {
                long now = OwnedTimingTrace.Now(); if (now > limit) throw new TimeoutException("Native pump control exceeded its finite 20s mode bound.");
                trace.Record("native-server-poll-enter"); serverApi.Poll(); trace.Record("native-server-poll-exit");
                if (now >= nextPoll)
                {
                    trace.Record("native-guest-poll-enter"); (pump as MultiplayerApi ?? guestApi).Poll(); trace.Record("native-guest-poll-exit");
                    nextPoll = now + delay * 1_000_000L;
                    guest.Playback.Advance(.1, true); guest.Playback.Drain();
                    if (guest.Applied is { } applied)
                    {
                        trace.Record("control-presentation", applied, info: new { Tick = guest.Playback.Tick });
                        ages.Add(index == 0 ? 0 : (states[index - 1].Tick - applied.Tick) / 60.0);
                    }
                }
                if (serverApi.GetPeers() is { Length: > 0 } peers && index < payloads.Length && now >= nextPublish)
                {
                    MatchSnapshot state = states[index]; string digest = OwnedTimingTrace.Digest(payloads[index]);
                    publishedAt.Add(OwnedTimingTrace.Now());
                    trace.Record("control-publish", state, digest); published.Add((state.Revision, digest, state.EventSequence));
                    CommandResult? receipt = (index + 1) % 8 == 0 ? new(index + 1, true, "control receipt") : null;
                    if (receipt is not null) receipts.Add(receipt.Sequence);
                    server.Publish(peers[0], payloads[index++], receipt); nextPublish = now + 50_000_000;
                    if (index == 30) { pausedServicesStart = pump?.Services ?? 0; owner.GetTree().Paused = true; trace.Record("control-tree-paused"); }
                    if (index == 36) { pausedServices = (pump?.Services ?? 0) - pausedServicesStart; owner.GetTree().Paused = false; trace.Record("control-tree-resumed"); }
                    if (cancelAfterAdmission && index == payloads.Length) { cancellation.Cancel(); trace.Record("control-cancellation-requested"); }
                }
                await owner.ToSignal(owner.GetTree().CreateTimer(.01), SceneTreeTimer.SignalName.Timeout);
            }
            if (!published.SequenceEqual(guest.Snapshots) || !receipts.SequenceEqual(guest.Receipts) || trace.Truncated)
                throw new InvalidOperationException("Native control lost/reordered/replaced a snapshot/event cursor/receipt or overflowed evidence.");
            // Actual AuthoritySession admission/ledger over the same native RPC
            // signatures: exact retry, ordinary pause/resume and stable reconnect.
            Command start = Command.FromSnapshot(guest.WelcomeState!, 1000, "start", guest.StablePlayer);
            guest.SendRequest(start); guest.SendRequest(start); await Until(() => guest.Receipts.Count == receipts.Count + 2);
            if (!guest.Results[^1].Accepted || guest.Results[^1] != guest.Results[^2]) throw new InvalidOperationException("Native retry changed its receipt.");
            Command pause = Command.FromSnapshot(session.Snapshot(), 1001, "pause", guest.StablePlayer);
            guest.SendRequest(pause); await Until(() => guest.Receipts.Count == receipts.Count + 3);
            if (!session.Paused) throw new InvalidOperationException("Native pause did not apply.");
            guest.SendRequest(Command.FromSnapshot(session.Snapshot(), 1002, "resume", guest.StablePlayer));
            await Until(() => guest.Receipts.Count == receipts.Count + 4);
            if (session.Paused) throw new InvalidOperationException("Native resume did not apply.");
            int oldPeer = guestApi.GetUniqueId(); Release(); guestPeer.Close(); guestApi.MultiplayerPeer = new OfflineMultiplayerPeer();
            await Until(() => serverApi.GetPeers().Length == 0);
            generation++;
            if (guestPeer.CreateClient("127.0.0.1", port) != Error.Ok) throw new InvalidOperationException("Native reconnect failed.");
            guestApi.MultiplayerPeer = guestPeer; Acquire();
            await Until(() => serverApi.GetPeers().Length > 0 && guestApi.GetUniqueId() > 1);
            guest.Join(session.MatchId); await Until(() => guest.Welcomes == 2);
            if (guest.StablePlayer != stablePlayer || guest.WelcomeState!.MatchId != states[0].MatchId || guestApi.GetUniqueId() == oldPeer)
                throw new InvalidOperationException("Native reconnect changed stable session/player or reused peer identity.");
            guest.SendRequest(start); await Until(() => guest.Receipts.Count == receipts.Count + 5);
            if (guest.Results[^1] != guest.Results[^5]) throw new InvalidOperationException("Reconnect lost the original retry receipt.");
            receipts.AddRange(new long[] { 1000, 1000, 1001, 1002, 1000 });
            if (!receipts.SequenceEqual(guest.Receipts)) throw new InvalidOperationException("Native lifecycle receipts lost/reordered.");
            Release();
            if (candidate && (candidateServices == 0 || suppressed == 0 || (payloads.Length >= 36 && pausedServices == 0)))
                throw new InvalidOperationException("Candidate did not demonstrate exclusive/paused service ownership.");
            double[] receiveAge = guest.ReceivedAt.Select((stamp, slot) => (stamp - publishedAt[slot]) / 1e6).Order().ToArray();
            double median = receiveAge[receiveAge.Length / 2];
            proofs.Add((candidate, delay, payloads.Length, median, receiveAge[^1], published.Select(p => p.Digest).ToArray()));
            if (cancellation.IsCancellationRequested)
            {
                try { cancellation.Token.ThrowIfCancellationRequested(); }
                catch (OperationCanceledException) { trace.Record("control-cancellation-observed-after-owned-drain"); }
            }
            return new
            {
                DelayMilliseconds = delay,
                Published = published.Count,
                Received = guest.Snapshots.Count,
                Receipts = guest.Receipts.Count,
                Conserved = true,
                Candidate = candidate,
                CandidateServices = candidateServices,
                SuppressedAutomaticAdmissions = suppressed,
                StablePlayer = stablePlayer,
                Reconnect = true,
                Retry = true,
                NativePauseResume = true,
                TreePauseService = payloads.Length >= 36 && (!candidate || pausedServices > 0),
                PausedCandidateServices = pausedServices,
                FrameDeduplicated = frameDeduplicated,
                PublicationToRpcMilliseconds = new { Minimum = receiveAge[0], Median = median, Maximum = receiveAge[^1] },
                CancellationRequested = cancellation.IsCancellationRequested,
                MaximumAppliedAgeSimulationSeconds = ages.Max(),
                Seconds = (OwnedTimingTrace.Now() - started) / 1e9,
                Digests = published.Select(p => p.Digest).ToArray()
            };
        }
        finally
        {
            owner.GetTree().Paused = previousPause; Release();
            guestPeer.Close(); serverPeer.Close();
            guestApi.MultiplayerPeer = new OfflineMultiplayerPeer(); serverApi.MultiplayerPeer = new OfflineMultiplayerPeer();
            owner.GetTree().SetMultiplayer(null!, guestRoot.GetPath()); owner.GetTree().SetMultiplayer(null!, serverRoot.GetPath());
            guestRoot.Free(); serverRoot.Free(); trace.Record("owned-native-teardown");
        }
    }
}
