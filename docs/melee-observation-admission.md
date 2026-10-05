# Bounded melee observer/admission diagnosis (steering 013)

## Historical result and boundary (steering 013)

Steering 014 subsequently authorized and collected the missing native/clock evidence. See
[native-pump-timing.md](native-pump-timing.md) for the **B + A** verdict, timestamp-aligned
native receive backlog, early unpaused witness and proposed correction. The findings below
remain the evidence-limited conclusion at the end of 013, not the latest blocker.

The retained failure has **both a late milestone observer and lagging graphical applied state**.
The former is established by fixture control flow; the latter is established by observations and
retained snapshots. Their relative contribution cannot be recovered completely from compact logs.
There is not yet a demonstrated safe correction that preserves the live progression contract.
No gameplay/render/transport correction or additional Godot scene was attempted under this steering.

Seven new inexpensive controls pass alongside the unchanged 691-test baseline: **698 tests** in
`logs/20261005-085931-e3ce95d0/` (198 tooling, 478 general, 13 solo/session, nine cooperative).
Formatting and strict OpenSpec validation accompany this diagnosis. These are **diagnostic controls**,
not a passing causal regression for an implemented correction or near/far capture acceptance.
The conditional authorization for one corrected focused rerun has therefore not been exercised.

## Retained chronology: what is and is not known

Current evidence: `logs/20261005-081600-f50c4eb4/combat-worker/combat/`.
Protocol logs retain arrival **order**, revisions, ticks, phases and command results, but not an
arrival timestamp. `ChildEvents` also has no per-arrival clock. Progression timestamps are supervisor
response times; `ProbeCosts` measures engine acquisition work and identifies actual process frames.
No before/after-decode, network-enqueue or pause-registration timestamp was recorded. Do not infer
an exact authority tick at an observation time from `wall seconds × 60`: speed 1 establishes step
batching, not uninterrupted wall-clock throughput under CPU contention.

| Ordered event | Retained fact |
|---|---|
| Before final Ready | Authority acknowledges speed 1 at Preparation, revision 21, tick zero. No milestone waiter is registered. |
| Final Ready | Observer ack revision 24 / Combat / tick zero. Graphical client subsequently applies revision 24 / tick zero. |
| First progression sample | Supervisor response 08:16:46.284979 UTC, received revision 32, presentation tick 8. |
| Last walking sample/raw frame | 08:16:48.0468605 UTC, revision 50, presentation tick 26. |
| First attack sample | 08:16:52.1863627 UTC, revision 88, presentation tick 64. The preceding interval includes waiting for an eligible action, not just PNG or draw work. |
| Last progression sample/raw frame | 08:16:53.9387327 UTC, revision 100, presentation tick 76. |
| Progression completion | Twelve observations/four acquisitions: 9.0671637 s; movement and attack advancement pass. |
| Ordered persistence | Four exact acquired frames persisted in 1.5664156 s. Graphical log lines 1176–1179 follow its applied revision 104 / tick 80. Their original acquired ticks remain 15, 26, 68 and 76. |
| Milestone observation starts | Only after all the above, and after another live probe. `revision = Latest(observer).Revision` becomes a new future-only floor. Exact floor/time is not retained. |
| Live graphical witnesses | 21 witnesses; impact identities include ticks 73, 103, 133, 313, 344, 403 and 404. All six coverage flags occur. Witness records lack observation id/tick/time, so the precise pause-request opportunity cannot be reconstructed from them. |
| Pause/camera commands | **No pause or resume ack exists in this run.** Code issues pause only when the independent observer finds a future authoritative milestone. It never enters the paused milestone camera/capture branch; no required overview/close frames exist. |
| Terminal guard | Milestone task returns non-Combat authority state; the unchanged phase/wave assertion fails. Observer sees Defeat, revision 997, tick 973. Graphical pre-cleanup evidence is revision 463 / tick 439 / Combat. |
| Cleanup | Owned processes/display are awaited. The graphical transcript continues applying ticks 443/447 after its retained pre-cleanup ring; this does not turn tick439 into a timestamp-matched latency measurement. |

During progression the presented tick equals the received revision minus the initial combat revision
24 for these samples. There is no hundreds-of-ticks **presentation-versus-applied-state** gap in this
series. The first/last responses span 7.6538 seconds and advance 68 applied ticks, about 8.88 ticks per
wall second. This is an observed graphical applied rate, **not** an isolated network, authority,
codec or GPU throughput measurement.

## Known successful control and fixture comparison

Historical success: `logs/20261003-223032-a9cdd2df/combat-worker/combat/`.
It completed the actual windup/impact overview/close views. Progression took 4.1471609 seconds;
first/last samples were ticks 58/265, about 58.93 applied ticks per wall second between their response
timestamps. Observer paused at tick **405**, resumed, paused at **425**, then resumed. These are real
current-authority acknowledgements, not replayed historical pauses.

Both fixtures serialize progression before milestone observation. The old fixture at landed
`0332c44` also does so; the current path additionally awaits ordered immutable persistence before
registering the milestone task. Old progression wrote PNGs inline. The pre-Ready speed barrier is
new and must remain: the earlier post-entry barrier was itself too late for authored rendering.
`UiTests` calls `MeleeArmy` to completion before `MeleeCheckpoint`; progression and milestone
observers do not share initial combat admission. There is no pre-Ready milestone registration.

The historical successful run is **not an identical numerical/environment control**: although both
record seed 1, its configuration fingerprint differs from the latest run (A begins
114770940252459570 versus 4774486509227455414). It establishes that the old serialized interface once
worked and provides command order, not an interchangeable timing trace or proof that current
seed-1 milestones must occur at 405/425. The current pure replay reaches its first shared windup at
301 and terminal Defeat at 973. No frozen references were changed in this diagnosis.

## Application, presentation and native pump inspection

- `Main.Snapshot` synchronously decodes and accepts a same-match nonolder revision, then emits its
  supervised snapshot. `SetState` replaces the current state. There is no game-side snapshot queue.
- `Main.Acknowledged` delivers command results independently and prevents an older ack snapshot
  from replacing a newer channel-1 state. Identity/retry/receipt semantics remain untouched.
- `Tabletop._Process` reads the current `game.State`, accepts a changed revision, updates the world
  and advances shared `CombatPlayback`. It does not drain old snapshots one per render. Playback
  retains combat **events** intentionally; they are not interchangeable disposable state records.
- `SnapshotPayload` and `CombatPlayback` have no diff from `0332c44`. Decoder, world update, node
  animation and native poll/receive are potential measured boundaries, not proven hot paths.
- Official locked [SceneMultiplayer source](https://github.com/godotengine/godot/blob/4.7.2-stable/modules/multiplayer/scene_multiplayer.cpp)
  polls its peer and dispatches all available peer packets. Locked
  [ENet peer source](https://github.com/godotengine/godot/blob/4.7.2-stable/modules/enet/enet_multiplayer_peer.cpp)
  services the host and drains checked events. Its bundled
  [ENet protocol](https://github.com/godotengine/godot/blob/4.7.2-stable/thirdparty/enet/protocol.c)
  can receive up to 256 datagrams in an incoming-service loop. Inspection does **not** justify an
  assumption of exactly one snapshot consumed per graphical frame, nor a blind extra-poll rewrite.
  Exact source copies/hashes remain in ignored `.art-audit/`.

Actual arrival/application timing is missing, so this inspection cannot assign the observed lag to
socket backlog, native dispatch, decode, frame scheduling or authority scheduling. It excludes the
specific claim that our presentation is draining an explicit old-state queue. No state, combat event,
acknowledgement or receipt was dropped/coalesced.

## Executable controls and their limits

`tests/DevRunner.Tests/MeleeVisualProofTests.cs` adds five controls using the real match rules,
`ChildEvents`, `CombatPlayback`, ordinary commands and immutable raw-frame owner:

1. A pre-Ready registration floor finds a current seed-1 published windup at tick304; a controlled
   registration after tick638 finds none before Defeat973. Tick638 models a delay, **not** a recovered
   authority timestamp from the failed run. The test publishes at four-tick intervals, not real ENet.
2. A current live pause preserves its actual milestone through 1200 attempted steps, delayed
   presentation and ordered persistence. Out-of-order persistence fails. This proves ordinary pause
   ownership, **not** that concurrent live progression can still acquire twelve frames within 12s.
3. Accepting every arrived snapshot while delaying presentation reaches the newest accepted tick
   (within playback's existing three-tick correction), rejects an older revision, and retains/drains
   combat events. This is an application/playback control, **not** a native transport benchmark.
4. A historical retained milestone can satisfy a generic history predicate. A same-phase/turn pause
   quote constructed from it is accepted but pauses **current tick638**, not historical tick301.
   Match/phase/turn validation deliberately does not validate the quote's tick/revision. A successful
   pause receipt alone is not proof that the intended action is still current.
5. Current terminal authority rejects both an old Combat pause quote (stale context) and a fresh
   terminal quote (match finished). A graphical Combat label cannot override that authority guard.

`tests/DevRunner.Tests/MeleeObservationInterfaceTests.cs` adds two owned shell protocol controls,
without Godot or a real PNG. A mis-tagged response cannot satisfy a fresh probe id. A response with
the correct fresh id can legitimately contain old applied state: request freshness is **not** state
freshness. Neither control supplies units, witnesses, raw frames or screenshot acceptance.
Existing raw-frame corruption/order/missing/ownership controls remain unchanged.

A correct milestone coordinator must validate current phase/wave and the actual pause ack, then
require the rendered paused tick/action/view to agree before capture. It must never accept the
historical-history example as a current action, relabel an old frame or count progression PNGs as
milestone views. The controls expose these hazards; no new production acceptance waiver was added.

## Why a minimal pre-Ready rewrite is not yet admitted as a fix

Moving milestone waiting earlier removes one demonstrated admission defect. It does not by itself
remove applied-state lag. Worse, an early pause can reach the graphical client during progression,
freeze its current action, and prevent the required same-action walking/attack advancement. Running
two client probe drivers concurrently would violate ordered acquisition/persistence ownership.
Registering a task before Ready also must not introduce competing consumers for the observer's
Ready acknowledgement and future pause receipt. These are coordination requirements, not reasons
to extend the 12/30-second split or slow simulation.

**Proposed next bounded scope, requiring Firstmate direction:** retain the original scenes and add
owned, non-secret monotonic boundary evidence for authority publication, RPC arrival/decode/apply,
presentation revision/tick and observer registration/pause request/ack. Pair that with a small
checked-in native delayed-pump control against a normally pumped peer, preserving every event and
command receipt. Use it to identify the first measured divergence before proposing a transport or
coordination correction. If evidence instead permits a coordination-only fix, use one ordered
observer driver and one ordered graphical driver registered from the existing tick-zero barrier;
prove progression/pause handoff, current-authority/receipt/frame matching and delayed persistence
without new bounds before the conditional single focused rerun.

**Blocker remains `authored-melee-milestone-admission`.** No second live scene, rendering experiment,
full after-CI, archive, commit, completion or no-mistakes handoff is claimed. Source bytes/hashes,
licenses, UI/audio, numerical rules and all historical/uncommitted work remain preserved.
