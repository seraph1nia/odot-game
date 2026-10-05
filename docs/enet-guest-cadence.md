# ENet guest service and melee observer handoff

Steering 015 authorized this bounded integration correction after the
[B + A boundary diagnosis](native-pump-timing.md). Neither authored exports nor
render quality, numerical ticks, channels, wire/security identities or event
retention changed. The dedicated authority still advances independently at 60 Hz.

## One owner, on the engine thread

Locked Godot 4.7.2 references were inspected before choosing the hook:

- [SceneTree](https://github.com/godotengine/godot/blob/4.7.2-stable/scene/main/scene_tree.cpp):
  ordinary multiplayer polling occurs before `process_frame`; multiplayer API
  replacement/poll-setting changes require the main thread.
- [SceneMultiplayer](https://github.com/godotengine/godot/blob/4.7.2-stable/modules/multiplayer/scene_multiplayer.cpp)
  and [RPC implementation](https://github.com/godotengine/godot/blob/4.7.2-stable/modules/multiplayer/scene_rpc_interface.cpp):
  the original API owns native service, packet/RPC processing and sender identity.
- Installed GodotSharp 4.7.2 API XML: `MultiplayerApi.Poll` executes callbacks in
  its caller's context; `SceneTree.MultiplayerPoll` normally admits automatic
  service. `ProcessMode.Always` admits processing even while the tree is paused.
- Godot's [documented API-extension passthrough example](https://github.com/godotengine/godot-docs/blob/master/classes/class_multiplayerapiextension.rst)
  forwards peer access, signals, RPC and object configuration to an inner
  `SceneMultiplayer`. The tutorial is current guidance; the installed signatures
  and locked engine implementation determine this integration.

`EnetGuestPump` wraps **the same** `SceneMultiplayer` only for an ordinary root
ENet guest. Its automatic `_Poll` admission does no native work. One always-enabled
owner-thread driver calls the original API once per fixed-physics frame, before
ordinary game physics. This is not another concurrent poll, manual packet drain,
busy loop or background native thread. **Global `MultiplayerPoll` remains enabled**;
custom APIs, authorities, solo/playing hosts, Steam and unsupported modes retain
original behavior. Original signal handlers use the original signal API, while
extension signals are forwarded as documented. Peer/channel/security/codec/RPC
configuration and remote sender identity stay original.

`GuestPollOwnership` rejects released/stale-generation, foreign-thread and reentrant
service before native work. An inner-API ownership registry rejects a second owner;
repeated serviced-frame admission does no native work. Session replacement,
disconnect, error and exit release the lease, stop the driver and restore the
original API if this owner still owns that root. Replacement APIs are not overwritten.
Destruction is deferred when release occurs on an active native callback stack.
The always-enabled driver continues networking while paused; it does not advance
rules or presentation clocks.

## Native matched proof

Checked-in command: `mise run test-native-pump`.
Final evidence: `logs/20261005-103819-bd0dd052/native-pump/`.
Initial proof: `logs/20261005-100016-9e041243/native-pump/`.

The isolated headless control uses the same 80 complete fixed payloads at 50ms
publication spacing. Original delayed service/presentation admission is 350ms;
candidate delayed render/presentation admission is also 350ms, with exclusively
fixed-physics native service. Every digest, revision, complete combat-event content,
cursor and acknowledgement sequence is conserved in order. Real `AuthoritySession`
admission, exact duplicate-command receipt, ordinary pause/resume and credential
resume preserve stable player/match identity across a different native peer.
Credentials/payload contents are never logged.

| Final mode | Original ordinary | Original delayed | Candidate delayed |
|---|---:|---:|---:|
| Complete snapshots received/published | 80/80 | 80/80 | 80/80 |
| Acknowledgements, including lifecycle/retries | 15 | 15 | 15 |
| Publication → managed RPC min/median/max | 13.66/13.93/15.03 ms | 18.81/621.29/1476.87 ms | 6.93/13.89/20.83 ms |
| Maximum sampled applied simulation age | 0 ticks | 60 ticks | 0 ticks |
| Native candidate services / suppressed delayed admissions | — | — | 275 / 13 |
| Native services during tree pause | — | — | 20 |

The checked-in gate rejects conservation failure and requires both median and
maximum receive age to improve by at least half against matched delayed original
service. The eight-payload cancellation subset stops publication, drains eight
snapshots/six issued receipts including lifecycle work, observes cancellation, then
closes all owned peers/APIs/subtrees. It does not claim its shorter feed exercised
the tree-pause interval. Stale/double-owner and repeated actual driver-frame negatives
run natively; foreign-thread/reentrant/release-stack guards run cheaply.

Retained development failure `logs/20261005-103522-a8f3e48c/` delivered all records
but failed the newly explicit age gate (candidate median ~1352ms/max2809ms). The
added duplicate-frame negative had injected a new poll during connection admission,
contrary to the exclusive-driver control. It now repeats the **actual already serviced
frame**, never injecting native work or changing connection admission. No failure
was deleted or age threshold weakened; the corrected strict control above passed.
This does not isolate packet-level reliability/fragment timing or prove arbitrary-load
performance.

## One real before/after diagnostic

Same authored workload, unchanged 40-sample/18s read-only diagnostic:
`mise run test-ui --scenario combat --checkpoint admission --jobs 1`.
After: `logs/20261005-100632-5b96b17c/combat-worker/combat/` (43.13s scenario).
Before: `logs/20261005-092301-52c169a5/combat-worker/combat/`.

| Real graphical result | Before | After |
|---|---:|---:|
| Publication → RPC median lower bound | 12,264.52ms | 1,166.77ms |
| Minimum / maximum lower bound | 2,759.37 / 13,300.22ms | 2.79 / 6,016.96ms |
| Last aligned authority/presentation gap | 790 ticks | 8 ticks |
| Last retained authority / graphical tick | 956 / 166 | 683 / 683 |

This proves a useful correction, **not zero latency**: a startup transient remains.
The guest recorded 1787 exclusive services and 224 suppressed automatic admissions,
then restored ownership. Existing downstream decode/application remains fast.
Clock alignment, repeated-digest lower bounds, closed owners and bounded records
are verified by `MeleeTimingReport`; shutdown tails are not alleged drops. This
owned llvmpipe experiment does not establish native GPU/compositor performance.

## Early observer and exact paused images

`CoordinatedMelee` records the floor at pre-Ready Preparation/tick0 and arms its one
ordered headless wait immediately after **that observer's own** Ready acknowledgement,
before graphical synchronization. It collects live witnesses without pausing during
progression. `MeleeAdmission` permits an ordinary current-authority pause only after
12 actual observations/four immutable acquisitions complete within 12s, and only
when current match/revision/tick/phase/turn and eligible action agree. Historical or
terminal evidence cannot authorize that pause. Exact progression persistence remains
fully awaited under the separate 30s maximum; no progression PNG substitutes for a
milestone view. Cancellation awaits the watcher before child teardown.

Four cheap coordination controls cover pre-Ready/Ready/handoff ownership, delayed
peer/render/PNG work, stale/wrong-match/terminal rejection, frozen-frame tick/revision/
phase and unchanged counts/bounds. Cheap coverage at the handoff proof: **708**, preserving 702. Final migration
controls extend this to750; see the migration report and resolved current casualty admission.

The one authorized focused acceptance passed:
`logs/20261005-102101-8a020ed8/combat-worker/combat/`.

- Scenario 63.51s, inside the original 70s scenario and 65s checkpoint limits.
- Twelve live observations/four raw acquisitions **4.3500s**; exact ordered PNG
  persistence **1.4698s**; movement/attack/equipped-bone advancement pass.
- Observer floor revision21/tick0; own Ready ack revision24/tick0. First current
  eligible request revision385/tick361 after handoff, actual pause revision387/tick362.
- **40 live witnesses; Shared/Near/Far/Simultaneous/Windup/Impact; Complete=true.**
- Far windup overview/close: actual pause tick362/revision387; near impact
  overview/close: actual pause tick504/revision535. Four distinct fresh acquisition
  ids and exact RGBA pixel hashes accompany camera zoom1/3 and action identities.
  Owner-thread persistence redecodes the same bytes; each frame must agree with
  current nonterminal paused authority. See `combat-melee-*.json/png` and
  `melee-handoff-timing.jsonl`.

The receive-backlog and milestone-admission blockers are resolved for this owned
acceptance. Complete migration additionally requires the full source/environment,
roster, export and package gates documented in the migration report; this focused
pass does not waive them.
