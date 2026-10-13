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
of a squad. Its physical arena weapon then blocks that deployment cell, and
recruitment, return and deployment all skip it until sale. Base arrow upgrades
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

Wire protocol **14 / combat rules 7** refuses older peers with the existing
protocol-mismatch response. Snapshots carry the connected arena, squads, individual
members, authoritative formation slots, physical structures and events. Combat
profiles author X/Y, structural damage, range and timing—not member size, victim
caps or cross-hex radius. One squad claims one cell, including a one-member boss.
The old city-transfer admission DTO and cosmetic-death admission bound are removed.

Receiving explicitly refuses old/unversioned combat settings, retired authoring
fields, legacy size-budget boards, missing arena data and inconsistent member/actor
projections. Welcome, acknowledgment and snapshot delivery use the same codec plus
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

## Provisional paid campaign measurements

[Paid squad baselines](paid-squad-baselines.md) record actual legal investments,
individual attrition/food/healing, Keep/Watchpost pressure and battle lengths for
finite solo/cooperative seeds. They distinguish fourteen passing verification
rows from twelve actual victories and two pure-ranged defeats, and retain fallen
cooperative Keeps rather than equating team victory with complete rescue.
Economics-sensitive results are **provisional**: the current three Building
incomes plus fourth income-free Preparation edit are not the approved unlanded
three-preparation/speed/preview flow. No replacement phase or compensating
income/damage was implemented. Final timing integration and calibration remain
required; see the coverage ledger for exact executed scope.

## Native presentation

The production native squad panel provides member HP/equipment/service/status
inspection, paid reinforcement/new recruitment, compatible regrouping, individual
Town Hall storage/return and retirement. All actions use the normal session command
and accepted snapshot path. Roster focus survives ordinary authoritative updates;
no optimistic member appears before acknowledgment. Allies can be inspected but
not edited. Deployment toggles between the settlement and the connected arena;
select an owned squad, close the panel, then pick an empty highlighted purchased
home. Combat presents all owners together, with local camera navigation only.

`SharedArenaView` owns physical imported Keeps, Watchposts and purchased towers,
owned tiles and compact per-member HP/count/status summaries. Its camera derives
bounds and travel from the complete arena, not old per-city framing. Formation
slots persist in snapshots, while animation, member offsets and short melee
lunges remain cosmetic. The opaque native base covers the projected safe camera
boundary. Keep fall disables defenses without inventing tower destruction.

The existing headless component runner has a focused `test-engine --scenario arena`
selector. It owns minimal native arena/camera/Controls/imported-model/rig components
and a numerical receiving adapter. It is **not** real transport, a full application
journey, pixel/artistic readability, rendered choreography or GPU proof. Full
campaign balance, real authority transport/reconnect and packaged boundaries still
need their own separately recorded qualification; filtered checks remain partial.
