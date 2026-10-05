# Finite native-pump and authored timing verdict (steering 014)

**Historical diagnosis, not current blocker status.** Steering 015 authorized and
proved the exclusive guest-service correction and safe progression handoff.
[Current ownership, before/after measurements and accepted four images](enet-guest-cadence.md)
supersede the historical stopping point below; retained failures/limits remain evidence.

## Verdict: native receive backlog plus late fixture admission (B + A)

The missing boundary evidence is now collected. The first large measured divergence is **before
managed graphical RPC receipt**. Decode, state application and presentation of received state are
fast; a separate application/presentation snapshot queue is not the explanation. Fixture sequencing
also consumes the authority milestone window. Earlier observer registration alone cannot remove
native delivery age, and immediate pause could prematurely freeze progression.

No production correction, poll/transport rewrite, coalescing, event dropping, asset/source change,
renewed progression budget or full combat/CI retry was performed. The single authored diagnostic
is **not game acceptance**. Required four paused near/far overview/close images remain absent.

## Checked-in execution and ownership

```sh
mise run test-native-pump
mise run test-ui --scenario combat --checkpoint admission --jobs 1
dotnet tools/DevRunner/bin/Debug/net10.0/DevRunner.dll inspect-admission-timing \
  --evidence-directory "$PWD/logs/20261005-092301-52c169a5/combat-worker/combat"
```

The native control and admission checkpoint are explicit diagnostics; neither enters unfiltered UI
or ordinary CI as another match. Native peers use the locked Godot .NET engine, actual loopback
ENet, ordinary `SceneMultiplayer`, reliable channel 1 snapshots, reliable channel 0 acknowledgements,
and the unchanged complete `SnapshotPayload` codec. Only the isolated headless control tree disables
automatic polling and deliberately delays its own poll/presentation admission. Product authority
speed and native automatic polling are unchanged. It does not establish native GPU/render behavior.

The authored scene uses the normal owned X11/Xvfb/llvmpipe Compatibility renderer and actual migrated
hierarchies. It uses ordinary six-unit recruitment and the pre-Ready speed-1/tick-zero barrier.
The diagnostic samples read-only live observations without pause or PNG acquisition; it does not
call `MeleeProgression` or restart its 12-second acceptance budget. One ordered driver owns each
child; observer Ready acknowledgement completes before its sole future-milestone wait starts.

`OwnedTimingTrace` is opt-in through supervisor-supplied owned evidence paths. Engine initialization
checks existing verification ownership. Records are bounded to 8,192 per owner, ordinal numbered,
without packet contents, credentials or new wire fields. Overflow is explicit and rejected by the
reporter. The process-frame callback exists only when enabled, and samples the automatic native
poll completion boundary at most once per physics-frame counter. Disabled product runs attach no
per-frame diagnostic callback. Authoritative encode/publish, managed RPC receipt/decode/application,
presentation acceptance, admitted/completed observations and ordered driver registration/request/ack
boundaries are recorded. No pause request occurs in the authored diagnostic; its production driver
request/ack hooks are ready to record ordinary supported pause commands in subsequently authorized
work, not invented pause timestamps here.

Clock provenance is **Linux `clock_gettime(CLOCK_MONOTONIC=1)`**, absolute nanoseconds, frequency
1,000,000,000. All four real-scene owners record the same kernel boot id and time namespace, so their
absolute timestamps align without subtracting independent process epochs. UTC is a contextual anchor,
not the subtraction clock. The reporter rejects mismatched clock provenance, missing owner closure,
nonmonotonic timestamps, unordered ordinals and overflow. Snapshot digests correlate publication with
receipt and decoded revision. Forced identical-state sends can repeat a digest: reported real-scene
latency uses the most recent preceding identical publication and is a conservative **lower bound**,
not an invented packet identity. The native fixed sequence has distinct ordered revisions/digests.

The locked engine [SceneTree source](https://github.com/godotengine/godot/blob/4.7.2-stable/scene/main/scene_tree.cpp)
polls multiplayer immediately before `process_frame`. The trace records the completion boundary,
not a nonexistent public native poll-entry callback or a fabricated auto-poll duration. Explicit
native poll entry/exit times are available in the isolated manual-pump control.

## Native conservation control

Evidence: `logs/20261005-092152-4464f23c/native-pump/`.

Both principal modes send the **same 80 fixed complete snapshots**, four numerical ticks apart,
at a test-only 50ms publication schedule. The ordinary mode polls on a roughly 10ms owned timer;
the delayed mode polls/admits presentation every 350ms. This is a fixed synthetic transport feed,
not a change to gameplay authority timing. All received payload digests, revisions and event cursors
must equal the published ordered sequence; all ten channel-0 acknowledgement sequences must match.
Complete payload digests include the combat event content, not only the latest cursor.

| Result | Ordinary | Delayed 350ms |
|---|---:|---:|
| Published / received snapshots | 80 / 80 | 80 / 80 |
| Acknowledgements | 10, exact order | 10, exact order |
| Payload/revision/event-cursor conservation | exact | exact |
| Publish-to-RPC latency min/median/max | 13.87 / 13.95 / 15.13 ms | 20.08 / 609.77 / 1,476.90 ms |
| Maximum sampled applied simulation age | 0 s | 1 s (60 ticks) |
| Mode elapsed | 4.45 s | 6.46 s |

Delayed polling builds an ordered backlog; it does not transparently replace old complete state
with the newest state. At this finite load, it neither loses nor reorders the issued records.
This headless result alone cannot quantify real rendering or identify an individual native socket,
fragment, reliable-window or service-loop bottleneck.

A third bounded cancellation control stops admission after eight snapshots, requests cancellation,
drains all eight exact snapshots and the one issued acknowledgement, observes cancellation, and
then closes peers/APIs/subtrees in `finally`. Every mode records `owned-native-teardown` followed by
its closed trace footer. Runner cancellation/timeout also awaits the owned child/scope; it never
kills an unrelated endpoint. Normal control exit, native cleanup and supervisor cleanup passed.

## Single authored diagnostic and first meaningful divergence

Evidence: `logs/20261005-092301-52c169a5/combat-worker/combat/`.
The owned scenario passed its **diagnostic** assertions in 51.24 seconds, including ordinary setup;
its 40 read-only samples are not screenshots or twelve-observation/four-acquisition acceptance.
`timing-verdict-evidence.json` is generated by the checked-in inspector after owner cleanup.

| Boundary/cost | Measured result |
|---|---:|
| Graphical publication → managed RPC receipt, combat payloads | **2,759–13,300 ms**, median lower bound 12,265 ms |
| Headless observer publication → managed RPC receipt | **1.01–6.73 ms**, median 2.68 ms |
| Graphical decode | 0.84–6.17 ms, median 1.29 ms |
| Graphical decode completion → state application | 0.0035–0.0338 ms, median 0.0053 ms |
| Applied state → presentation acceptance | 0.53–121.97 ms, median 0.91 ms |
| Presentation world update/acceptance | 0.10–120.71 ms, median 0.14 ms |
| RPC receipt → next sampled automatic poll completion | 0.88–16.22 ms, median 2.00 ms |
| Authority encode | 0.29–4.13 ms, median 0.77 ms |
| Actual live post-draw wait | 165.96–185.18 ms, median 169.17 ms |
| Fresh observation construction | 8.16–16.31 ms, median 8.36 ms |

For the **same first combat payload**, tick5/revision29, the graphical client reaches managed RPC
receipt **2,759.37ms after publication**, while the ordinary headless observer reaches it in
**5.70ms**. That is the first quantified combat divergence; it precedes graphical decode/application,
progression PNG work and eventual serialized milestone admission. Startup/building publications also
show variable graphical delays, but they are not used as a steady combat throughput control.

Near the end, tick170/revision194 reaches the graphical managed receiver **13,300.22ms** after its
publication; the observer has received tick964. At the last rendered observation the authoritative
published/applied stage is still Combat: presentation/applied tick **166**, authority tick **956**,
gap **790 ticks**. This is a timestamp-aligned gap, not a comparison of unrelated cleanup endpoints.

The report finds 65 graphical snapshot receipts and 265 observer receipts against 269 publications;
every received digest/revision correlates and revision order is retained. The diagnostic ends before
catch-up and intentionally tears down its owned session. Its undelivered tail is **not evidence of
packet loss or permission to discard records**; only the fully drained native control establishes
complete conservation. No product records were explicitly dropped/coalesced.

The concrete coupling is that normal native multiplayer service is admitted by the render/process
loop, while the dedicated authority publishes combat state independently. The controlled delayed-pump
case reproduces ordered backlog with the same reliable modes and codec. The real-scene large delay
is already present before managed RPC receipt; downstream costs cannot account for seconds of age.
We do not claim a packet-level native root cause or distinguish OS receive queue, reliable-window
feedback, fragmentation and individual native service work without additional hooks. In particular,
there is no justification to blame slow decoding, rewrite transport, drop events or flatten assets.

## Early observation without freezing movement

- Pre-Ready registration **floor** is revision21, Preparation/tick0.
- The single headless waiter actually starts at authority tick118, after Ready ack and graphical
  combat synchronization. This is not falsely described as an active waiter before Ready.
- It finds a live shared windup at **tick302/revision326**. No pause/resume command is sent.
- At that timestamp the graphical applied state is **tick9/revision33**.
- The diagnostic's actual imported live frames show **MovementAdvanced=true, AttackAdvanced=true**;
  `Paused=false`; 23 live-node contact witnesses are retained. The authority witness did not freeze
  the separate movement observations. No claim is made that these diagnostic samples meet the
  original twelve-observation/four-capture timing or paused near/far requirements.
- A separate registration control sampled after the previous run's 10.6336s progression-plus-I/O
  duration sees authority **tick765/revision789**, past its early shared opportunities. This new
  timestamp is a controlled admission comparison, not a recovered clock from the historical run.

Thus early observation can collect a real live authority witness without pausing movement, while
actual graphical movement remains observable. It does **not** make a tick302 screenshot possible
from current graphical tick9, nor make an old witness safe to pause later. Current-authority,
terminal, action, receipt and frame freshness guards remain necessary.

## Smallest supported correction candidate — not implemented

Report before another combat retry: a fixture-only earlier waiter is insufficient. The smallest
candidate supported by the divergence is an **owner-thread ENet native pump-cadence correction**,
with no wire, buffering, renderer, asset or numerical changes. For example, evaluate service from
existing guest fixed-physics admission rather than relying solely on a slow render/process cadence.
This is a proposed separately authorized correction/control, not an enabled fix or a claim that
physics service will automatically meet the graphical deadline. Preserve every snapshot/event,
acknowledgement, sequence, retry and session/endpoint owner; no cross-thread Godot work or blind
additional-poll loop. Steam and authorities must retain their separate existing ownership.

Only once that candidate is causally proven should the observer coordination be narrowed: capture
its floor at the existing pre-Ready barrier, arm its one ordered wait immediately after its Ready
ack rather than after graphical synchronization/progression/persistence, and request a supported
current-authority pause only after safe progression handoff and a still-live eligible action.
The complete twelve observations, immutable four progression acquisitions, separate 12/30-second
contracts, four actual near/far overview/close views, camera/tick/pixel identity, before-wave3/terminal,
death/cleanup and frozen gameplay guards remain requirements, not negotiable diagnostic outcomes.

**Current blocker: `authored-native-receive-backlog`.** Firstmate must authorize/evaluate the narrow
correction candidate before production poll code or another combat retry. This instruction's one
native control and one real authored diagnostic are consumed; no renewed experiment/budget follows.

## Validation and remaining scope

- Cheap tests **702 passed**, preserving the previous 698: `logs/20261005-093155-33522d0f/`.
  Added controls cover bounded ordinal/digest/clock ownership, selection isolation, cross-process
  clock mismatch rejection and closed-owner digest-correlated reporting.
- Locked formatting, strict OpenSpec validation and diff checks accompany this continuation.
- All native modes and the owned real-scene peers/display/runtime cleaned up; traces closed without
  overflow. No developer preferences/desktop, source exports, licenses, UI/audio or numerical rules
  changed. The source checkout remains read-only at `a640d721`.
- Full after-CI/package evidence, final environment acceptance, four near/far captures, migration
  completion, archive/commit and no-mistakes handoff remain incomplete.
