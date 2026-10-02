# Nine Tiles POC

One to four players share a match, each owning nine indexed building slots displayed as three staggered rows of hex plots and a separate grassy battle approach. Single player starts one city immediately; hosted multiplayer waits for the original host to click Start. Join the roster before start. The roster is fixed after start; returning players can resume, fresh players cannot join. Return to menu and start again for a fresh local/hosted match; restart a dedicated server for another match.

Each city starts with 100 health, 60 gold, **30 wood**, and no food. The wood grant is tuned from the initial 20 seed to preserve an accessible upgraded-farm/barracks opening. Gold, food and wood are the only spendable resources; there is no upkeep or storage limit. Every living city, including an absent player's city, receives 10 base gold per production plus its buildings' outputs. Costs are atomic: an unaffordable command spends nothing.

| Building | Build gold / wood | Level 1 → 2 output or role |
| --- | ---: | --- |
| Farm | 20 / 10 | 5 → 8 food per production |
| Mine | 20 / 0 | 5 → 10 gold per production |
| Lumbermill | 20 / 0 | 5 → 10 wood per production |
| Barracks | 20 / 10 | Swordsman and Berserker |
| ArcheryRange | 20 / 10 | Crossbowman |
| Arcanum | 25 / 10 | Mage |
| Blacksmith | 20 / 10 | Class research |
| ArrowTower | 20 / 15 | 5 → 7 damage every second |
| CatapultTower | 30 / 20 | 6 → 8 damage every two seconds; up to 3 victims |

Every building upgrades once for 20 gold and 10 wood. Production upgrades use the explicit output above. Recruitment buildings save one food per recruit at level two, always retaining a positive cost. The Lumbermill requires no wood to build, so a city can replenish an empty wood stock.

Click an outlined plot or a building roof to select its stable slot, then choose an action in the bottom panel. Clicking only selects. Recruitment appears only at the matching building; Blacksmith shows Melee, Ranged and Magic research. City tabs clear selection and permit inspection of other players' cities. Ready prevents editing; Unready restores it until all connected living players are ready. A ready check grants production once and clears readiness. After production three, **Preparation** allows construction, upgrades, research and recruitment using that income. **Ready for battle** then starts combat without granting more resources. Three waves have exactly nine productions and three preparation checks. The phase and stage serial guard reject delayed commands, including old ready commands.

Skeleton enemies allocate 4, 6 and 8 per original player over the waves. Roles cycle by original allocation index: wave one Swordsman/Berserker; wave two adds Crossbowman; wave three adds Mage. The built-in defender remains a separate weak city-wide 2-damage shot every second outside the nine slots. Unit and tower impacts share one simultaneous damage accumulator. Surviving units keep identity and remaining health between waves; cities retain damage too.

A city falls at zero health. Its remaining attackers immediately move to the entrances of surviving cities' lanes, retaining identity, health and attack cooldown. They are split evenly, with remainders assigned in player-ID order; a previously cleared city can receive them. If two cities fall in the same step, neither receives transfers. Disconnected living cities still count as survivors. Future waves include one baseline allocation per original player, with fallen players' allocations divided among survivors. For example, after one of three cities falls, wave two gives each survivor 9 enemies (18 total). Inheriting enemies does not multiply future allocations. Eliminated players can observe, pause and resume. All players share victory after wave three if a city survives; all cities falling ends the match in defeat, including simultaneous final enemy deaths.

A verified opening is Farm in slot 1, Barracks in slot 2, then upgrade Farm using the starting 60 gold/30 wood. Recruit affordable Swordsmen after productions, including Preparation, before becoming ready. The checked-in ordinary-command strategy fixtures also cover mixed armies, tower investment and class research, and a common frontline strategy for one through four players. Food production takes priority over specialist/research investments. A tower opening can start with an ArrowTower or CatapultTower using ordinary starting resources. Ready with no investments remains a losing strategy.

Any connected roster member can pause or resume the whole match. Pause freezes movement, attack cooldowns, health, production and turn progression. It also rejects economy/readiness commands. Networking stays active, and absent players can reconnect. Disconnecting itself never pauses or deletes a city: production/combat continue, readiness clears, and disconnected players do not block the ready check. With no connected living players, building waits; combat keeps running unless paused.

Gold, food and wood are the only resources. Walls, selling, healing, trades, reinforcements, save recovery, host migration, server restart recovery and matchmaking are outside this POC. Rules and typed catalogs are centralized in `src/Game.Core`; the server sends costs/output values in snapshots so the UI uses the actual rules.

The bottom panel keeps city resources/health/army and inspection tabs on the left, selected building details/costs in the middle, and match controls on the right. Start is available to the original playing host in a hosted lobby (or connected players on a dedicated server); Ready/Unready and Pause/Resume follow the authoritative phase. Connection and rejection messages appear along the bottom. A disconnected window offers Reconnect to my city; a synchronized resume retains the city and starts with no selected plot. Expired credentials expose the fresh-session lobby action. Fallen players can inspect all cities and pause/resume; victory/defeat remains visible to the entire roster.

![Blacksmith research, terrace plots and current resource stocks](images/source-building.png)

![Observed level-two Catapult with structural base](images/source-upgraded-tower.png)

![Paused specialist combat with imported Mage and Skeleton rigs](images/source-specialist-combat.png)

![Paused combat after reconnecting in the exported client](images/export-paused-resume.png)

## Music and local settings

Press **Esc** or click **Settings** in the top-left to open Graphics and Audio.
Press Esc again or **Close** to return; Esc dismisses an open dropdown first.
Opening settings blocks gameplay input in your window while the shared match and
network updates continue. Use **Pause whole match** separately to pause everyone.
One application music player continues across menu, solo/hosted sessions, connecting, pauses/outcomes and reconnects.

Graphics offers **Windowed** and **Fullscreen**. Windowed resolution changes the
window's size, with 1100×820, 1280×720, 1600×900, and 1920×1080 offered when they
fit your monitor's usable area. Fullscreen uses the current monitor's native size;
the windowed selector is disabled until you return to Windowed, restoring your
previous size. Manual resizing is shown as a custom size. Saved sizes that no
longer fit fall back to a usable size. Explicit engine launch arguments such as
`--resolution`, `--fullscreen`, and `--windowed` take precedence for that launch;
those overrides do not become saved preferences until you edit display settings.

Audio provides a live **Master volume** slider from **0–100**. Zero mutes all
audio; raising it restores music at its ongoing position. First-launch defaults
are Windowed 1100×820 and Master 50, with the music itself mixed at -12 dB.
The bundled Echoes of Valhalla track plays its intro once and repeats its authored
loop through Godot's WAV playback.

Preferences are stored separately from multiplayer credentials in
`user://settings.cfg` (on a standard Linux desktop,
`~/.local/share/godot/app_userdata/Odot - Nine Tiles/settings.cfg`). Display
selections, completed slider edits, menu closing, and pending changes on orderly
exit save them. Restart restores valid values; reconnect retains the current
ones. Invalid/missing fields use defaults. A failed save displays a message in
the menu while your current settings keep working.

Two running local clients keep independent live preferences. The last successful
save becomes the shared OS user's defaults for later launches; it never updates
the other running window. Servers and headless clients ignore graphical settings
and do not create music players. Music provenance and import details are recorded
in [the asset README](../src/Game/Assets/Music/README.md).


## Start screen and session lifetime

A normal graphical launch opens **Single player**, **Multiplayer**, **Settings**
and **Exit Game**, over static bundled medieval scenery. Keyboard arrows/Tab and
Enter work alongside pointer input. The menu creates no match or gameplay socket.
The bottom of the start screen and Multiplayer view shows your Steam display
name, or **Open Steam to log in** when Steam is unavailable or offline. Host game
checks the current login each time and warns you to open Steam and log in before
creating a lobby.

Single player binds one local city and uses the same validated actions and rules
as multiplayer. Multiplayer offers **Host game** and **Back**. With Steam running
and signed in, Host game creates a private invitation-only lobby; use **Invite
friends** in the bottom panel. Steam’s overlay must be enabled; Shift+Tab
checks whether it can open. If it is unavailable or an invitation dialog does
not activate, the bottom panel explains the problem. Only the original host
starts the match. Missing
Steam/login/access reports recoverable feedback; solo, settings, Back and Exit
remain available. The local `mise run dev` host/guest flow uses ENet independently.

**Return to menu** ends unsaved solo/host gameplay and clears selection and
pending actions; preferences and music persist. Guests keep private credentials
for the same running host. An invitation during a match asks before leaving;
declining preserves it. Returning players can resume after the roster locks,
including pause or elimination, using their retained credential and Steam account.
Fresh players cannot join after start. A new host session has a new match and
refuses old credentials.

The original host owns the whole hosted session. Its orderly leave ends the
session for guests; unexpected loss disables guest actions and offers bounded
reconnect/return feedback. A Steam lobby owner change never migrates gameplay.
**Exit Game** and the window's native close control stop the session, music,
network/Steam resources and the game process. Closing settings restores valid
focus and changes neither readiness nor shared pause.

## Deterministic hex combat

Each authoritative `Match` owns one private Arch 2.1.0 world. A single immutable
`CombatUnit` component owns identity, health, location/lifecycle, decision state
and a closed waiting/moving/windup/recovery action. The action owns its timing,
sequence and explicit unit/city target once. Attack ordinals remain separate
from movement sequences for animation identity. Serialized timing/target fields,
city armies and reservation indexes are projections of that authority. IDs are
stable and never reused. All session modes use the same synchronous 60 Hz rules;
guests render complete snapshots. Engine animation never applies damage.

The default board has 21 cells, three columns over terrain rows -5 through 1,
independent of the nine building plots in rows 2 through 4. Each hex contains
six fixed positions. Capacity costs are two for melee and one for support;
compatible footprint masks decide placement, including fragmentation. Allies
can share a hex; opposing factions cannot share occupied or reserved cells.
Range uses shortest-path distance on the authored axial graph, never model
geometry. A separate city distance anchor links the neutral home frontier and
provides no movement shortcut. Board scale and anchors passed the early rendered gate. Neighboring rendered centers are approximately
three world units apart; footprint anchors use integer thousandths of a world
unit for presentation only. Melee deliberately uses tabletop presentation:
rigged swings at fixed anchors, a directional connection to the sampled target,
then an authoritative impact or miss cue. It does not manufacture physical
weapon contact or move an attacking model between footprints.

Each side's rear row is permanently protected against opposing placement and
transit, but occupants remain normally attackable. Setup allocates rear support
first, then forward melee, activates together and queues overflow. Within each
tier use initiative and seeded ties; choose the least used compatible cell,
then center/owner-relative left/right, then its forwardmost compatible footprint.
Combat admission tries forward capacity then protected rear for melee; support
uses rear capacity. No permanent lanes or free intra-hex rearrangement exist.
Reinforcements transferred into a cleared city can enter protected rear cells
while defenders hold its neutral forward band. If retained enemy deaths block
entry, cumulative releases of actual masks determine the fixed first-admission
bound; the first unrelated expiry or total free capacity is insufficient.
Further overflow still queues normally. A conserved queue without ensuing
engagement does not establish progression.

| Archetype | Health | Damage | Food / gold | Capacity | Initiative | Hex range | Move ticks | Windup / recovery | Death ticks |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Swordsman | 10 | 4 | 5 / 0 | 2 | 10 | 1 | 30 | 12 / 48 | 48 |
| Berserker | 8 | 6 | 6 / 0 | 2 | 20 | 1 | 27 | 18 / 54 | 48 |
| Crossbowman | 8 | 3 | 5 / 0 | 1 | 30 | 3 | 30 | 18 / 42 | 48 |
| Mage | 6 | 4 | 7 / 2 | 1 | 40 | 3 | 30 | 24 / 66 | 48 |

These defaults passed the early strategy, role, admission and rendered gate. Both factions use equal profiles at equal research rank. Research
adds 5% of base maximum HP/damage per rank up to rank two, in checked integer
hundredths; it changes no spatial/timing attributes and never heals survivors.

At a new attack select the closest in-range living deployed opponent, then the
lowest target initiative, then a seeded exact tie. Melee requires distance one.
Ranged units hold when anything is in range. Otherwise one integer BFS per
actor scores all opponents by reachable attack-position steps, target initiative
and a seeded exact tie. If none is reachable, legal static movement distance
ranks blocked objectives. Every eligible action boundary reevaluates all targets;
a retained objective has no priority over a newly shorter approach. Only the
winning target gets a route. Equal shortest support goals prefer a friendly melee
screen, then less used capacity, then a seeded footprint choice. Shortest-path
predecessors reconstruct a simple route without searching for each opponent.

Exact local target and footprint observations invalidate a blocked decision;
unrelated cities and nonlethal health changes preserve its key and scheduling
rank. Temporary transit conflicts are checked at atomic move commit. Unchanged
episodes retain their visited cells to avoid walking back through congestion;
only repairing that winning episode may require one extra BFS. Changing the
objective clears the old exclusions. Unchanged blocked retries preserve choices
and wait for local changes. Rendering frames never trigger numerical decisions.

A move starting at tick 100 and ending at 130 locks its source, destination and
transit conflicts together. Before tick 130 attacks see only the source; at 130
arrival commits before impacts, so attacks see only the destination. No attack
or route change occurs mid-step. Lower actor initiative wins conflicting moves;
seeded ranks resolve equal initiative, while independent actions start together.
Each tick expires retained deaths, completes arrivals and recoveries, resolves
all due impacts against the same pre-damage view, applies checked accumulated
damage, retains new deaths, admits queued arrivals and finally starts actions.
An unsuccessful reservation commit leaves the actor waiting with no partial move.

An attack started at T impacts at T + windup and finishes at T + windup +
recovery. The unit stays anchored through both intervals. All due unit/defense
impacts share one snapshot and damage accumulator before deaths; initiative
cannot cancel a simultaneous lethal exchange. Invalid locked targets cause
misses with normal recovery. Mage splash has radius one hex and cap
two, primary first, then distance/initiative/seeded ties. Catapult radius is
zero with cap three. Friendly, queued and dying units are excluded. City damage
is one primary hit. Defender and towers have independent positive windup/recovery,
using the shared city distance anchor regardless of plot. Candidate defender
damage is 2 per second; Arrow levels deal 5/7 per second and Catapult 6/8 per two
seconds. These values are tuned separately from unit approach speed.
Defender and tower identities are explicit. Every defense receives a complete
frozen damage/windup/recovery/victim-cap/radius profile and uses the same pure
attack-deadline and victim-selection rules as units. The default defender remains
single-target (cap one, radius zero); configured splash applies to its actual
impact rather than only its fingerprint.

Death immediately removes a unit from living counts, actions and targeting, but
retains its footprint until death end. A transit death freezes route progress and
retains both endpoint and transit locks. Death at tick 200 with duration 48
releases space at tick 248. Wave completion is immediate; bounded cleanup can
continue outside combat, and a ready next-wave preparation waits for that cleanup
without extra income. Pause freezes the authority's action/death clock.
Unexpired bodies remain in the separate current dying collection at their
original battlefield during reinforcement transfer; they never transfer as
living attackers. Expiry removes the body and all of its reservations together.
Ending or replacing a session disposes those retained locks and events rather
than carrying them into a fresh battlefield.

A fight retains a non-secret 64-bit seed and immutable configuration fingerprint.
Combat rules version is two; the decision mixer stays at algorithm version one.
Corrected targeting changes seeded outcomes, while protocol v6 and its serialized
shape remain unchanged. The same canonical setup, seed, rules/algorithm version and ordered accepted
commands reproduce the same normalized trace. Version-one SplitMix decision
keys separate targeting, movement ranks, route/goal, formation and splash choices.
Credentials, session GUIDs and cosmetic randomness are separate. Core strategy
acceptance samples seeds 0, 1 and 123; this finite sample is not universal balance
or deadlock proof. Paired role tests demonstrate effective splash contribution, frontline protection
and queued melee access; see the measured comparisons in `docs/verification.md`.

The default no-health-progress allowance is 3,600 ticks per active city and the
wave duration bound is 18,000 combat ticks. Only actual health reduction resets
progress; walking, misses and admissions cannot extend it. Normal completion and
all-cities-fallen defeat take precedence. An unresolved limit ends the match as
`BattleStalled` defeat with unchanged surviving health. Numerical lifecycle, limit and early rendered checks pass. Final full integration acceptance passes; see `docs/verification.md` for evidence.

Knight, Barbarian, Rogue and Mage and the four corresponding free Skeleton rigs use imported skeletons and role weapons at `handslot.r`. Programmatic AnimationTrees retain synchronized idle/walk/run, timed attack one-shots, filtered hit layers and terminal death. Sword uses horizontal slice, Berserker two-handed chop, Crossbowman ranged shot and Mage Spellcast_Shoot. Horizontal root motion is suppressed. Sword strike is 0.40 clip seconds, crossbow release 0.43, axe descent 23/30 (0.767) and cast extension 8/30 (0.267), mapped onto authoritative impact and recovery. Cosmetic effects never apply damage. The hex presentation migration aligns death clips and retained bodies to authoritative death intervals, including cleanup outside combat and reconnect. The early melee proof and current-death reconnect checks pass; final source/package integration passes.

Terraces, riverbank, bridge, mountain edges, flags, grain/racks/scaffolding and structural tower bases provide village detail. Raised surfaces, building bounds and selection rings use per-plot heights. The combat approach stays flat. Level two adds distinct structures/props instead of enlarging the whole building. The windmill rotates its separate authored fan node; flags use restrained procedural motion on the same presentation clock.

Gold bars, food sacks and free wood logs represent stocks with bounded instance tiers: zero → 0, 1–19 → 1, 20–49 → 3, 50+ → 6 per resource. HUD amounts remain exact. Switching cities and reconnecting reconstruct current stocks without replaying earnings.

Accepted local construction/recruitment/research commands cue once by match and sequence; retries and rejections do not repeat success puffs. Battle sparks, projectiles and skeleton rattles use the buffered event cursor. Focus changes/reconnect/gaps discard historical effects. Effects cap at 64 and audio at eight voices, dropping cosmetic overload only. All cues route through Master; pause/loss stop voices, and session teardown removes them while the application music continues. Original sounds use 22,050 Hz mono signed 16-bit PCM, 0.12-second cubic decay, sine frequencies 110/220/330/440/660/880 Hz and fixed-seed noise transients. Village ambience is a restrained synthesized cue every 12 unpaused seconds. Silent Dummy-audio verification checks lifecycle/routing/mute state, not listening quality. Headless roles instantiate none of this presentation.

A shared, bounded snapshot clock interpolates all bodies with the same fraction,
without extrapolating beyond authority. A 120-tick, at-most-4096-record event
history carries final casualty states. Initial admission, reconnect and detected
history gaps baseline the event cursor at the current high-water mark: only living
current poses are reconstructed. Overlapping snapshots deduplicate effects;
revision and match guards reject stale state. A fresh session clears all live/dead
views, pose buffers and cursors.

New entry visuals wait for their deployment snapshot’s common clock before appearing, preserving separation from interpolated neighbors. Surviving units return to idle after combat while death visuals finish independently.

The 32-vs-32 fixtures verify body separation every step, finite progress and identical serialized results with reversed entity storage, including all four friendly roles against mixed skeleton melee. Ordinary strategy fixtures retain the real defender and default resources and record resources, recruits/casualties, city HP and bounded battle duration.
