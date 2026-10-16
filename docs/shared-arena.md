# Shared arena combat

## Numerical ownership

`CombatSimulation` keeps persistent individual health, equipment level, experience, assignment and status
in its existing Arch member store. `CombatSquad` owns the only movement, targeting
and attack cycle for each homogeneous group. Member snapshots project that shared
cycle for animation; members never choose routes or enemy groups independently.
Ordinary squads have three members by default (`Rules.Arena.SquadCapacity`). Bosses
are the same actor with one member. Stable member and squad IDs are separate.

`SharedArena` creates adjacent seven-column, nine-row sectors for one to four
players (63–252 connected hexes). Six deployment homes belong to each player;
Keeps, forward Watchposts and nine possible purchased-tower footprints are reserved
static cells. Reserving an unpurchased weapon footprint also prevents later tower
construction from invalidating a deployed squad. Settlement plots remain separate.
Deployment ownership is a preparation rule, not a movement barrier during battle.

## Impacts and movement

Every living participating member contributes one X/Y strike to the selected
enemy squad. Seeded primary choice uses the member identity and committed attack
sequence. X hits that primary, Y hits every other pre-impact living member. Either
channel can be zero. Contributions are gathered before any same-tick health loss
or death is applied; overkill stays on its original member. Research modifies both
direct channels. Burn/poison/chill apply only to members actually damaged.
Structure damage is a separate per-member profile, not a fake one-member HP pool.

Squad chill is the rounded-up mean across living participating members, including
healthy members' zero contribution. Watchpost destruction applies an owning-squad
20% direct damage reduction for 240 ticks by default, once per wave; no movement
or attack-disable penalty. Watchposts reset between waves; Keep wounds persist.

`SquadMovement` admits a unique deterministic winner per destination. Friendly
chains and exchanges/cycles commit together at a common arrival tick; unrelated
moves keep their own deadlines. Opposing exchanges are refused. Squads keep their
source logical position until arrival. A logically dead squad stops occupying
immediately; its members' cosmetic bodies can continue falling.

In-range opponents are engaged before routing. Enemies also consider physical
Watchposts and Keeps, even while defenders elsewhere remain alive. Preferred
sector biases objective choice rather than isolating combat. Falling Keeps stop
owner weapons and future participation/development, but surviving owner squads
continue the current battle. Redirecting pressure does not teleport actors.
All-Keeps defeat is checked before wave clear/victory.

## Roster and compatibility

A home holds one homogeneous squad, not a numerical body-size budget. Persistent
home indexes are 0–5, distinct from physical arena cell IDs. Buy any numbered
locked army plot at the current purchase-count quote; total ownership never grants
unbought intervening homes. A paid tower can occupy an empty owned home instead
of a squad. [Army-home and tower rules](gameplay.md#defense-towers-on-army-plots)
own placement exclusion, immediate home release on sale and deferred physical
footprint refresh during cleanup. Base arrow upgrades
retain the exclusive Rapid Arrow/Bombard paths, prices and individual weapon
windup/cadence. Arrows use X only; Catapult/Bombard use within-group X/Y, not
cross-hex radius/victim caps. The catalog, native preview and selected inspection
show the actual channels and range.

Deployment to an empty
owned purchased home transfers the entire squad's home claim; subsequent new
recruitment cannot stack another actor on that destination. Recruitment
and reinforcement still pay equipment per individual at the recruitment building's
level. An explicitly selected matching squad can be reinforced; a new squad needs
a free home. Member transfers preserve individual IDs, equipment level, HP, experience and eligibility.
Each surviving paid participant earns one service experience point on a successful
clear, including survivors whose Keep fell during that wave. Reserve, stored and
unfunded members do not earn it. Experience is persistent battle service, not a
free equipment upgrade; reinforcement recruits a new level-appropriate member
with zero experience and never heals or replaces a veteran's identity.
Town Hall capacity and healing remain individual-soldier rules, independent of
squad population. Food funds members in the existing field-first, veteran-first,
stable-ID order; an empty-funded squad creates no combat occupancy.

[Session compatibility](sessions.md) owns current wire/rules/flow versions and
older-peer refusal through the existing protocol-mismatch response. Both independently advanced predecessor
branches used protocol14 for incompatible schemas; numeric equality was not
compatibility and neither predecessor is silently admitted. Snapshots carry the connected arena, squads, individual
members, authoritative formation slots, physical structures and events. Combat
profiles author X/Y, structural damage, range and timing—not member size, victim
caps or cross-hex radius. One squad claims one cell, including a one-member boss.
The old city-transfer admission DTO and cosmetic-death admission bound are removed.

Receiving explicitly refuses old/unversioned combat settings, retired authoring
fields, legacy size-budget boards, missing arena data and inconsistent member/actor
projections. The integrated full rules fingerprint includes battle-flow identity;
the required forecast array, distinct negative forecast IDs and actual configured
profiles are validated alongside shared geometry and member/squad identity.
Native-consumed catalogs, trainer quotes, city construction, Hall tracks/slots/
generations, production and food forecasts are checked against reconstructed
rules-derived projections at that same boundary. Genuine partial funding remains
valid when the last participant dies but unfunded reserves remain; reserves
project the squad cycle without becoming independent attackers. Pending windups
and latched primary aim survive target loss without requiring a live target.
Welcome, acknowledgment and snapshot delivery use the same codec plus
compatibility boundary before live state or reconnect credentials change. Failure
explains how to rejoin a compatible authority or start a fresh match; it does not
invent squads or silently reinterpret old fields. Session files are credentials
for reconnecting to the same running authority, not saved matches. Old combat
snapshot/replay inputs are refused rather than migrated.

## Verification scope

The initial domain implementation is covered by targeted pure C# tests:
`SharedArenaTests` (1–4 player connected topology/static footprints, all X/Y
channels including zero, member-count contribution, chill mean, deterministic
movement exchanges/conflicts/death release) and `SharedCombatTests` (actual
production simultaneous deaths, overkill, paid homogeneous recruitment/partial
food funding, physical Keep access with living distant allies, fallen-owner
survivors/future reserves, and bounded normal solo defense with unique occupancy).
Additional `SharedCombatTests` isolate Watchpost break/reset, local once-per-wave
Rally and direct-damage scaling, cross-sector rescue of an already damaged Keep,
and simultaneous last-enemy/last-Keep defeat precedence. `SharedStateTests` cover
real paid veteran reinforcement, stable member/group/formation identity through
serialization, deployment home-claim transfer and explicit legacy refusal.

## Eliminated-owner receiving and resume

The selected `test-network --scenario eliminated-resume` uses genuine headless
ENet peers on the original playing host. Its ordinary paid prefix completes two
waves with all Keeps alive and stores a served veteran. During W3 the
`fallen-stored`/`protected-stored` fixture removes all but one enemy, relocates
squads and sets the third Keep to one actual enemy structural impact versus
100 HP (see [`CombatReceivingEncounter`](../src/Game.Core/Combat/CombatReceivingEncounter.cs)). This is controlled elimination,
not a default-paid eliminated-stored campaign witness. The next allocation is W4.

The selector verifies accepted command accounting, exact paused roster/HP/injury/
action/claim/event/rule agreement, same-process reconnect and credential-file
restart. The stored city and recruitment ledger persist without a fresh city or
second payment. Fallen-owner ready/build/recruit/retire/research requests refuse,
including retries. Only living owners advance production/research; future enemies
from fallen origins choose surviving destinations. Spectators retain shared pause/
resume. Host end disables requests and emits a headless return event, which is
not graphical menu/focus acceptance. The earned prefix, controlled elimination
and real transport/resume evidence are separate; default-paid combined
eliminated-stored acceptance remains unestablished.

**The ENet survivor is stored, not an actively fighting fallen-owner unit.** Active
survivor continuation is separately covered by a controlled production
`World.Step`/compressed-receiving encounter: paid IDs/formations plus explicit
arithmetic poses, a wounded member and the one-impact Keep threshold described
above. Keep loss preserves current
source/action/claims and individual HP; a later fallen-owner strike lands, service
persists, and its members become nonparticipating reserves next wave. The same-tick
last-enemy/last-Keep codec witness still requires defeat, no reward and valid claims.
These inputs are not earned balance or active fallen-fighter ENet evidence.
The independent controlled ENet witness below now closes that specific active-field
receiving gap; it does not turn this paid stored-spectator selector into active proof.

The minimal native `arena` component receives compressed compatible snapshots,
retains the fallen roster, denies actual disabled readiness input and guarded
investment callbacks, and leaves another owner's city independently editable.
Its delayed session adapter is explicitly not transport. Exact executed receipts,
first failures, terminal owned PID/exit evidence and remaining gaps are in the
[testing audit](testing-audit.md#shared-squad-eliminated-owner-receiving-and-resume-2026-10-13).
Malformed/legacy compressed shapes remain direct codec negatives; old-protocol
handshake refusal is real ENet. Complete end-user networking, packages, final timing
and native artistic acceptance remain separate.

### Controlled active-field receiving

`test-network --scenario active-fallen` runs a fixed seed1 numerical encounter on
existing production playing-host/ENet/session/codec owners. Two identified
six-member field rosters start through actual commands; the named owned fixture
explicitly supplies synthetic tower materials, physical poses, a 33-HP member,
one retained enemy and the same one-impact Keep threshold. Its paired
disconfirming control changes only that Keep to 100 HP. This is **controlled numerical/transport proof, not earned
injury/economy/balance, full-game or complete network/reconnect acceptance**.
The one-shot source-defined seam requires an owned supervised headless loopback
playing host, seed1 and a named case; an owned guest is actually refused before
session/peer creation. It is not a general state loader or network command.

The peer actually receives nonempty active fallen fields, exact member/HP/squad/
source/action/claim/event/config agreement at an accepted pause, new-peer and
credential-file process restoration on the same original host, and a later
landed field impact with nonempty victims. Fallen-owner weapons stop (same-tick
simultaneous effects remain legal), city commands and refused retries cannot
mutate state, and the living owner alone produces/advances while those veterans
become next-wave nonparticipating reserves without claims. Original-host end
disables requests. Existing paid/codec/malformed/legacy/same-tick defeat/Watchpost/
actual abrupt-host negatives remain separate retained witnesses. No missed live
window or stored veteran can satisfy this selector's active-field guards.

## Provisional paid campaign measurements

[Paid squad baselines](paid-squad-baselines.md) retain historical four-edit and
integrated three-END inputs separately. The current flow is already landed:
exactly three editable preparations/production settlements, final END immediately
entering combat, accepted global speed and authoritative next-wave preview.
Historical ten-win/four-W1-loss diagnostics are not current-source acceptance.

The linked baselines retain the pre-rebalance recovery decision: full-six
Crossbow arithmetic survival was required, while paid pure-ranged progression
was deferred. Those opening and attrition losses are historical, not outcomes
under current enemy profiles. [Wave power](wave-power.md#headless-encounter-evidence)
owns the current finite paired paid measurements and their limitations;
rescue attribution and earlier diagnostics remain in the historical baselines.

## Native presentation

The production native squad panel provides member HP/equipment/service/status
inspection, paid reinforcement/new recruitment, compatible regrouping, individual
Town Hall storage/return and retirement. All actions use the normal session command
and accepted snapshot path. Roster focus survives ordinary authoritative updates;
no optimistic member appears before acknowledgment. [Army-home restoration](gameplay.md#purchased-army-homes-and-town-hall-rotation)
owns accepted cleanup-time edits, persistent eligibility, retained physical claims
and the final settlement barrier. Allies can be inspected but not edited. Deployment toggles between the settlement and the connected arena;
select an owned squad, close the panel, then pick an empty highlighted purchased
home. Combat presents all owners together, with local camera navigation only.

`SharedArenaView` owns physical imported Keeps, Watchposts and purchased towers,
owned tiles and objective overlays; `MemberHealthBars` owns member controls.
Both consume `CombatPlayback` under the
[health-overlay contract](../openspec/specs/game-feedback/spec.md#requirement-authoritative-overhead-unit-health-bars).
The arena camera derives bounds and travel from the complete arena, not old
per-city framing. Formation slots persist in snapshots, while animation, member
offsets and short melee lunges remain cosmetic. Melee windup uses the imported attack animation and
attached, aligned weapon/socket motion, not a dashed target guide. The local
strike accent stays bounded near the attacker; target-side contact requires an
actual landed impact. `StrikeAim` is detached observation of the squad-owned
seeded intent/realized primary, not member AI or a damage input. The same existing
X/Y selection supplies that observation; a primary death cannot silently point
recovery at another member. At a wave stop, `StopActions` captures each member's
selected primary or exact Keep/Watchpost pose at that authoritative tick, before
freezing the squad. That detached aim remains immutable even if a still-live
primary later moves; stopping does not retarget or change committed cycles.
Existing TargetId/FrozenAim fields carry it without new rules/profile/wire shape
or a second simulator. Unneeded live-target frozen poses are not repeated into
bounded event payloads.

Native cue projection includes the selected member's formation. Launch predicts
only its already committed transition through the impact deadline—not later
moves or a guaranteed hit; landed effects use the actual pre-damage primary pose.
Death freezes physical position/facing, including a melee lunge sampled at
the recorded death tick; the attack blend ends without pulling the cosmetic root
back to its cell center. The shared paused/playback clock advances the imported death clip until ordinary expiry/cleanup. Coarse stockpile
props update only across quantity bands; exact resource labels still consume
accepted snapshots without optimistic spending. The existing `animation` minimal
component now checks actual AnimationTree/bones/socket/projectile/health/status/
formation/death/cleanup/stock resources, a real lethal-impact live-to-dead
melee transition, terminal windup against a moving primary, and a real numerical
committed-miss control. Member and objective bars consume the
[shared playback projection](gameplay.md#deterministic-hex-combat), including
consecutive pending member boundaries and effective damage. Rally remains
buffered diagnostic and inspection state. These are headless resource/node
proofs, not pixels or choreography.
The opaque native base covers the projected safe camera
boundary. Keep fall disables defenses without inventing tower destruction.

The existing headless component runner has a focused `test-engine --scenario arena`
selector. It owns minimal native arena/camera/Controls/imported-model/rig components
and a numerical receiving adapter. It is **not** real transport, a full application
journey, pixel/artistic readability, rendered choreography or GPU proof. Complete
campaign balance, authority/transport behavior beyond the selected receipts above,
and packaged boundaries still need their own separately recorded qualification;
filtered checks remain partial.


## Woodland arena styling and native proof

`SharedArenaView` batches the pinned DetailedGround Grass/Moss/Dirt meshes with
their actual materials at the unchanged logical contact plane. Thin quiet hex
outlines preserve exact picking; owned deployment home markers follow the
[gameplay guidance](gameplay.md#purchased-army-homes-and-town-hall-rotation). Far-edge
spiral trees and bounded low boulder/fern side clusters stay outside playable
cells/objectives. An opaque green earth backstop covers the safe projected
camera boundary; imported asset bytes/manifests are unchanged. No terrain
bonus, collision, new shader/light system or authority randomness is introduced.

Content-derived fitting remains independent of decoration. Static batches and
dressing survive ordinary revisions and deployment toggles, while fresh layout/
session replacement releases them. Native arena proof covers1..4players,
actual flat imported contacts, every home pick, overview/portrait/extreme-pan
coverage, bounded resource reuse and four-player-to-solo cleanup. Objective
bars, model/ruin visibility and retained owner seals distinguish state; purchased
towers remain intact after Keep fall.

Expanded native receiving proof exercises new recruitment, reinforcement,
regrouping, Hall storage/return and retirement through ordinary paid commands
and delayed acknowledgment, plus foreign/fallen guards and focus retention.
`logs/20261013-215524-42288d50` passes the arena component. This is genuine
resource/node/control verification, not rendered appearance, pixel readability,
physical sound/input or a complete application/network journey. Graphical
acceptance is requested separately only after this implementation is reviewable.


The finishing native pass also corrects field recruitment/new-group/Hall return
callers that retained a positional size argument after it became a Hall slot.
Controls now expose actual field capacity, and the component asserts enabled
availability before pressing them. A same-lobby join refreshes native arena
width/physical footprints instead of retaining its first solo welcome geometry.
The compact unwrapped speed strip retains accepted-only global speed semantics;
it no longer stretches over the arena. Camera observation/picking follows the
active arena rather than a hidden city. The earlier squad headings used short
weapon roles and larger glyphs before the health-only overlay replacement.


Before that replacement, authorized graphical `combat` acceptance passed on the
finishing source
(`logs/20261013-225736-51c0ad8c`,150.25s). Retained real two-owner all-sector
frames cover overview, movement/inspection, pause/camera/resize and casualties;
actual live nodes demonstrate sword/axe/cast/hit, damage and corpse cleanup.
Dense overview labels can overlap; close inspection provides precise individual
health/service/equipment. Four-owner resource/layout coverage is headless native,
not rendered four-owner acceptance. Software OpenGL/private display samples do
not establish GPU performance or every composition's readability.


The explicit seed8 redistribution transport fixture now uses two paid six-Sword
armies and a third sector with one ordinarily funded Arrow Tower and no field
soldiers. The old zero-investment third sector lost its Keep under the final
profile; that failure is retained in the audit. This fixture qualifies paid wire
agreement/resume and next allocation, while uninvested-sector earned rescue is
established by the separate four-owner witness. Full compatibility validation
occurs once at remote decoding, before credential/state installation; trusted
authority publication uses the same presentation setter without rebuilding
configuration and claims on every publish.
