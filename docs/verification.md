# Cooperative POC verification

## Combat math and flow simplification, 2026-10-02

The before baseline passed full `mise run ci` in 431.98 seconds: 256 gameplay
tests, 120 runner tests, all six network cases, source UI, sequential exports
and headless/graphical package smoke. Evidence:
`logs/20261002-082928-4fad0b89/ci-summary.json`, with `source-head.txt` and
`source-sha256.txt` recording the source inputs. No required stage was skipped.

`CombatDecisionRegressionTests` covers two review defects: a swordsman at cell
17 must replace its farther retained cell-2 objective with the shorter legal
approach to cell 8, and a defender configured with victim cap two/radius one
must damage an eligible adjacent secondary. Their recurring acceptance command
is `mise run test`; these numerical checks use C# and launch no Godot process.
Existing network/UI/export gates cover integration; these defects require no
new expensive scenario.

The implementation stores one immutable `CombatUnit` action component; snapshots
and occupancy evidence derive from it. Pure C# action transitions, shared
distance/initiative/seed ranking, reachability and victim selection can be tested
independently of ECS and `Match`. `CombatPolicyTests` compares integer BFS against
bounded reference relaxation, checks one expansion for seven opponents and at
most two for retained-episode repair, and covers changed objectives, arrival,
released footprints and local observations. `CombatActionTests` checks exact
deadlines and rejects conflicting fixture state atomically. `DefensePolicyTests`
checks full profiles, cap/radius, invalid primary, no friendly fire and exactly-once
unit/defender/tower contributions against a shared pre-damage view. Existing
pause, elimination, ordering, snapshot reconstruction, lifecycle and session
tests remain mandatory.

Combat rules version 2 changes seeded outcomes; SplitMix algorithm version 1,
protocol v6, serialized shape and all default numerical profiles remain unchanged.
The ordinary solo frontline/mixed/tower/research and two-to-four-city frontline
strategies still cover seeds 0, 1 and 123. The cleared-forward transfer witness
uses seeds 1, 4 and 8; all cleared-city, held-forward-footprint, bounded admission
and continued-progression assertions are retained. The simpler upgraded-farm/
barracks opening uses seed 1. C# witness probes confirmed those setups under the
new identity in `logs/20261002-082928-4fad0b89/rules-v2-strategy-witnesses.log`;
old witnesses no longer establish their specific battlefield preconditions.
These finite witnesses do not prove balance for every seed. The historical
rules-v1 measurements below remain historical evidence, not current traces.

`mise run test` passed 275 gameplay and 120 runner tests after the coherent
refactor (`logs/20261002-082928-4fad0b89/refactor-final-cheap.log`). No engine,
dependency, tool-lock, economy or campaign changes were needed.

Final full `mise run ci` passed in **421.81 seconds**: 275 gameplay tests, 120
runner tests, all six network scenarios, all five source UI slices, sequential
client/server exports, headless package smoke and graphical package smoke.
Evidence: `logs/20261002-092110-29c7d1ba/ci-summary.json`; source UI 236.60 seconds,
exported UI 111.14 seconds. The complete console log and source hashes are
`logs/20261002-082928-4fad0b89/refactor-final-ci.log` and
`refactor-source-sha256.txt`. Locked solution restore, solution formatting and
build passed. No required stages failed or were skipped; nothing was published.
Software-rendered UI evidence does not establish native GPU performance or
physical input/listening behavior; ordinary CI does not establish real-account
Steam acceptance.

## Deterministic hex combat implementation, 2026-10-02

Final full `mise run ci` **passed**: 256 gameplay tests, 120 runner tests, all six
network scenarios, all five source UI scenarios, sequential client/server exports,
headless package smoke and graphical package verification. Evidence:
`logs/20261002-080220-8a1264c2/ci-summary.json`; 445.58 seconds total, 249.22 seconds
source UI and 113.01 seconds graphical package checks. No stages failed or were
skipped; nothing was published. `source-inputs.json` records HEAD, tool versions
and the per-file build/asset inputs; combined SHA256 `42d3fd1b988f7d22736388c0da488a2632e4d042424be61e48116ff3a43698e6`. This passing
state is the unchanged before-baseline for the following progression change.
The chronology below retains earlier partial checks and corrected failures.

The full pre-combat-change baseline passed `mise run ci` in 392.77 seconds,
including source checks, every network/source UI scenario, sequential exports,
and headless/graphical package smoke. Evidence: `logs/20261001-195415-e3ddb2d9/`,
with `ci-summary.json` and `baseline-inputs.json`. Inputs: HEAD
`a8d1838571cbbcbf31548079227559790cbb9d57`, source SHA256
`d7660219bb4c62651d161db3470a8899640dc60a46acdcec5548dbefcc7e1c13`,
.NET SDK 10.0.401 and Godot 4.7.2.stable.mono.official.ed1daf0bf.
This is baseline evidence, not final acceptance of the new implementation.
The preceding aggregate UI timeout was repaired using the measured 215.35-second
suite: default full-source budget 300 seconds, selected slices 180 seconds,
explicit overrides preserved.

Earlier execution restrictions denied local Unix/UDP sockets, VSTest and semantic
formatting. Their in-process xUnit passes and unexecuted graphical attempts remain
historical evidence under `logs/hex-combat-20261002/`,
`logs/hex-melee-checkpoint-20261002/` and `logs/hex-lifecycle-traces-20261002/`.
The resumed environment permits owned socket binds and normal execution.
`dotnet restore Odot.slnx --locked-mode` and
`dotnet format Odot.slnx --no-restore` now pass. Normal `mise run test` passes
**256 gameplay tests and 120 runner tests**, with no failures or skips
(approximately 10 and 1 seconds). The final selected source preparation builds
the whole solution with zero warnings/errors. Final full CI now passes as recorded above.

The later DTO migration removes continuous unit coordinates, uses integer profile
ranges and discrete event impact poses, and reconstructs occupancy from complete
living/dying snapshots. Authority and playback tests cover history-free deaths,
seeded authority equivalence and terminal poses. The ordinary three-city transfer
fixture uses seeds 0/1/2: the receiving city clears with surviving enemy-forward-band
defenders, admits reinforcements within the original bound and reaches wave three.
These are deliberately chosen positioning fixtures, not a claim that every seed
ends a wave in those cells. Cheap evidence: `logs/hex-integration-20261002/cheap.log`.

The extended real-ENet `redistribution` slice passes in 38.41 seconds (runner
43.56 seconds), with seed 1, ordinary purchases, an eliminated observer reconnect,
unchanged identity/damage/recovery and 9/9 allocation assertions, then a second
transfer into a cleared forward-held battlefield, bounded admission, ensuing hits
and ordinary wave-three preparation. Current reservations/configuration are
reconstructed on received snapshots. Evidence: `logs/20261002-072815-36d9a5d3/`.
The preceding attempt `logs/20261002-072733-a9792b8b/` exposed a harness wait that
matched historical Building state; the corrected wait requires the later tick
and wave three. This selected pass is partial integration coverage.

The selected `authority-resume-victory` scenario also passes with a natural
paused casualty, full reservation reconstruction before/after process restart,
unchanged duplicate-request/credential protections and ordinary three-wave victory.
Evidence: `logs/20261002-072906-1ba16b0f/`; scenario 37.24 seconds, runner 42.19 seconds.

The selected ordinary network `defeat` check passes with all cities at zero HP,
`AllCitiesFallen` and no stall diagnostic: `logs/20261002-073127-79c35612/`,
9.40-second scenario / 14.30-second runner. Graphical `reconnect` passes with a
natural paused casualty, retained camera/health checks, same-process reconnect,
then an owned process restart using only that client's saved session. Current
death intervals reconstruct without historical sounds/effects; resume expires
the bodies and reconstructs released reservations. Evidence and inspected restart
PNG: `logs/20261002-073043-b9ed8c3c/reconnect/`; 29.12-second scenario,
37.51-second runner. This extends the existing scenario with one owned restart;
cheap reconstruction checks alone cannot detect graphical startup or sound replay.

The full combat visual fixture also fixes seed 1. Its movement witness now
accepts either world axis and samples the hand's skeleton-global transform,
including animated parent bones; the local attachment rotation can legitimately
remain constant throughout locomotion. The main timing correction uses the headless observer to pause at authoritative
tower-impact and casualty milestones while the graphical client renders and
captures frames. A graphical pause click also follows the fresh movement
observation before screenshots. These barriers avoid consuming the short fight
and death intervals during graphical work. The earlier full-combat failures
remain recorded at `logs/20261002-073214-ff980dc4/` (stale pause),
`logs/20261002-073347-53304b51/` and `logs/20261002-073531-40dcbb2b/`
(missed animation opportunities). The corrected full combat slice passes with
fresh live role/attack/hit observations, stationary recovery, a paused current
casualty, declared-tick model removal, reconstructed reservation release, camera/
resize checks and return to a fresh solo session. Evidence:
`logs/20261002-074659-36373d39/`; scenario 39.06 seconds, runner 47.50 seconds.
The full 60-second combat deadline and existing assertions remain in force.

After the discrete DTO/feedback migration and corrected skeletal observation,
the melee checkpoint passes again with all four overview/close PNGs inspected:
`logs/20261002-074808-77e5bb94/`, 33.22-second combat scenario / 41.54-second
runner. Fixed anchors, directional strike guides, near/far targets and retained
deaths remain visibly distinguishable. The subsequent full CI pass is recorded above.

The first final-CI attempt (`logs/20261002-074909-2d8e01b5/`) passed locked
restore, formatting, build, import and all 256 gameplay / 120 runner tests. It
stopped in concurrent network verification because the new reinforcement predicate
examined an old lobby snapshot before player two existed. The predicate now
requires second-wave Combat and a matching eliminated player. The corrected selected redistribution slice passes in 34.03 seconds
(39.00-second runner), `logs/20261002-075024-81c5dab6/`. The failed full attempt
and selected rerun are partial coverage, not final CI acceptance.

The next full run (`logs/20261002-075118-700f517e/`) passed all six network
scenarios, all source UI (233.89 seconds), both sequential exports and headless
package smoke, then failed the packed shooting observation after an unpaused
capture consumed its opportunity. Package smoke now uses seed 1 and observer-driven
tower/shot/death pause barriers with ordinary graphical resume controls, retaining
the compact live-rig, action and current-death assertions. It is checked against
existing exports; this runner-only correction does not implicitly rebuild them.

The corrected `test-ui --scenario exported-package` passes against those existing
exports: `logs/20261002-075929-fbc66dec/`, 108.50-second package scenario /
111.86-second runner, including both menu/solo viewports and compact combat.
Packed shooting/death PNGs were inspected. Live exported rigs retain typed hex
positions, timed routes/actions and current death intervals. Its added pause
barriers use the existing observer and ordinary resume input; no new authority,
privileged combat state or full graphical battle is introduced. The subsequent full CI pass includes this harness correction.

The standard configuration fingerprint is
`8c0080f3af737848ff96499b7c810cee14fa9618cb6ba6d608fb40b1416ec501`.
All 21 ordinary strategy/seed cases at seeds 0/1/123 retain three-wave victory,
nine productions and the investment rules. No-investment seeds 0/1/123 lose at
ticks 481/523/547 through zero city health and `AllCitiesFallen`, never a stall.
The paired seven-Swordsman screen fixture credits Mage 20 effective damage
against Crossbowman's 15 over their common live interval through tick 325.
Mage records three secondary hits, clears at 325 versus 403 and preserves 18
friendly health versus 8. Recruitment remains seven food/two gold versus five
food/no gold: a role comparison, not equal-cost superiority. Against one ordinary
Mage opponent, the isolated Crossbowman clears at tick 139 and retains four
health; Mage clears at 175 in a simultaneous death. Screened support hits at 79
without support damage; its unscreened control first takes damage at 103.
These finite fixtures establish the required contributions, not universal balance.

Each transferred archetype encounters defenders occupying all three neutral
enemy-front cells. Free protected rear cells admit immediately at transfer tick
27. Fragmented retained deaths yield the original admission bound of 48 for
cost-one support and 72 for cost-two melee, unchanged during retries. Actual
identity/profile/health/recovery, a landed attack, ensuing health reduction and
ordinary completion at tick 126 or 138 are checked. Defenders cannot occupy the
protected row. Cumulative mask-release coverage refuses melee at the first
fragmented expiry. Conserved queues and BattleStalled do not count as admission.

The 32-vs-32 fixtures compare complete normalized snapshots every tick through
cleanup, including deaths, reservations, decisions/routes, defense and outcomes.
Only match ID/revision are normalized. Reverse insertion, unrelated match IDs
and additional observation/serialization on one side preserve the trace.
The mixed battle exercises Mage secondary selection, resolves at 790 and finishes
cleanup at 838. All-melee resolves through ordinary city-health defeat at 1456.
These disable the defender: fingerprint
`237144aab3b3c9ef2710304b3c5803a2fc73f8dc71dfd3f2a877d6685f3622bc`.
The 80-per-side queue resolves at 2620 with `AllCitiesFallen` and 154 deaths.
The accelerated zero-damage 64-per-side history stress serializes 3,810,327 JSON
characters and 2,674 retained events; this is stress size, not production
bandwidth acceptance. Routing fixtures cover detours, feasible ranking, screened
support, capacity preferences, transit conflicts, stable waits and revision-cache
invalidation. A natural moving casualty dies at tick 19, freezes 18 movement
ticks and keeps both endpoint/transit locks through death end 67. Dying transfer
is rejected without mutation; disposal clears locks/events/decisions for a fresh
combat. Existing simultaneous elimination, transfer recovery, rapid readiness,
pause, terminal frozen pose and cleanup-income guards also pass.

The early rendered gate **passes**:
`mise run test-ui --scenario combat --checkpoint melee`.
Accepted evidence is `logs/20261002-063429-e14c58ca/`, including timing summaries,
owned peer/engine logs, `combat/combat-melee-witnesses.json`, four PNGs and matching
observations. The selected checkpoint launches the ordinary authority with
explicit combat seed 1 and uses real two-Farm/Barracks controls to recruit six
Swordsmen over three productions. There are no grants, custom placements or
combat setters. Optional authority `--combat-seed` arguments support repeatable
diagnostics; other launches still generate one seed. Guests consume it.

The headless peer observes action milestones and sends ordinary pause requests
while the graphical client continues fresh probes. One ordered driver per child
and awaited cancellation keep ownership intact. Actual live nodes prove the
shared simultaneous windup at tick 214 (impact 223) and later near-side landed
impact at tick 253. Camera-only inspection leaves paused poses/actions unchanged.
Each phase has an inspected overview and close frame. The explicit dark target
guide and warm progress/landed stroke identify stationary miniature-to-target
attacks; landed target flashes remain distinct from windup/misses. Imported rigs,
weapons and fixed shared anchors are visible. Screen-sized role labels prevent
overview text from disappearing. Retained dying models remain visibly separate.

The recorded 86 fresh observations contain 119 moving-body samples covering ten
moving identities and six dying identities; minimum observed root separation is
0.802 world units, including visible deaths. The checked-in clearance assertion
passes throughout. Four-frame inspection and these finite route samples establish
this representative setup, not every possible customized footprint/animation.
Largest ordinary snapshot observed here is 104,557 JSON characters. Review metrics
are retained in `combat/review-metrics.json`; they supplement the checked-in
acceptance rather than replacing it.

| Selected gate phase | Seconds |
| --- | ---: |
| Locked restore | 0.91 |
| Solution build | 3.57 |
| Godot import | 2.99 |
| Combat setup, live assertions, four captures and cleanup | 34.21 |
| Source graphical phase including owned display | 37.53 |
| Runner total | 45.19 |

The checkpoint remains bounded at 40 seconds inside the 60-second combat case.
It extends the existing peers/display/input/capture setup, with no new full-match
scenario. Its defects are actual imported-model mapping, target attribution,
shared-cell cue/role readability and rendered clearance; core tests cannot see
them. Recurring maintenance is the current action/node detector and four captures.
Cheap seed-0/1/123 opportunities remain additional coverage: windup/impact
866/935, 211/250 and 547/803, with ordinary wave-two progression for two seeds.
Those opportunities do not claim identical capture pairs for every generated
seed. A failed generated-seed run is retained at `logs/20261002-060218-25981097/`.

The first seeded real run (`logs/20261002-061806-d9405af4/`) captured windup but
failed later pause synchronization: the authority repeatedly enqueued unchanged
approximately 100 KB paused snapshots, delaying the graphical peer's later state.
Changed-revision publication fixes this while preserving three-tick combat cadence,
prompt pause/non-combat changes, welcomes and command acknowledgments. The next
run passed at `logs/20261002-062710-a58fa22b/`; image review then required clearer
windup guides/overview role labels. The label/guide run passed at
`logs/20261002-063139-5eab6e5d/`; final stroke layering is the accepted run above.
All attempts awaited owned peer/display cleanup; no developer state or desktop
was used. Rendering uses private X11, llvmpipe and Dummy audio and does not
establish physical input, native GPU/compositor performance or audible quality.

The early numerical and rendered gate now accepts the current board/profile
candidates for integration. Comprehensive snapshot/history/reconnect/network/
package coverage, obsolete continuous DTO removal, final tuning and full CI
remain pending. Historical evidence below does not complete those obligations.

Verified on 2026-09-30 on Linux x86_64 using .NET SDK 10.0.401 and Godot 4.7.2 .NET. Historical desktop checks below used real X11 windows under KDE Wayland with the Compatibility renderer and an AMD Radeon RX 6800. The current recurring workflow uses private Xvfb displays and Mesa software graphics. Platform and remote-run coverage is recorded in the dated sections below.

## Rules and real processes

The core xUnit suite passes 17 cases. They cover one-to-four-player initialization, snapshot serialization, fixed rosters, atomic owned-slot spending, recruitment and upgrades, disconnected production, ready/unready, unique turn guards, retry identity, bounded deterministic combat, weak defense, simultaneous damage/deaths, immediate enemy conservation and remainder distribution, original-roster future allocations, persistent soldier/city damage, nine production turns, victory/defeat precedence and pause without catch-up. A pause/disconnect/resume regression proves that an already eligible ready check resolves once on resume.

The final real ENet suite passes using separate headless Godot processes and standard balance. It verifies:

- Two authenticated cities, start/build/upgrade/production/recruitment, sender ownership, foreign/invalid/occupied/unaffordable/wrong-phase/stale-turn requests, ready editing restrictions and unready.
- Malformed and excessive requests without disrupting the other client, old protocol refusal, fixed-roster late-join refusal, invalid/expired credentials and concurrent credential claims.
- Client B exits during combat; the city remains and combat advances. Client A pauses; a replacement B process gets a different ENet peer but the same city, economy, buildings, army and frozen gameplay. Replaying accepted recruitment before and after restart spends once. Pause revisions and connections remain observable; resume continues the simulation.
- A normal two-client farm-upgrade/barracks strategy wins all three waves, retains army state and refuses further turns.
- Three clients under-defend one city until it falls in actual combat. Surviving clients agree on conserved enemy IDs/remaining damage and changed destinations. The next wave allocates 18 enemies, 9 per survivor. The fallen player resumes as an observer and can pause/resume without reviving.
- An ordinary no-building strategy ends in defeat; unavailable/stopped servers, occupied ports, missing readiness and exited children produce bounded feedback.

There is no resource-grant, damage, kill-city or teleport RPC. Automated clients send normal requests through the same handlers as the UI. Network/selected UI suites default to 180 seconds; full serial source UI and source/full CI default to 300 seconds, with a 15-second startup deadline and endpoint/timeout overrides. Network, export-smoke and dev sessions use separate owned temporary directories and clean them on exit. Diagnostics contain public identity/state and command results, never resume tokens.

## Clean source, CI and exports

The ordered `mise run ci` pipeline passes after the final reconnect fix (about 128 seconds): locked restore, formatting, build/import, 17 rules cases, the complete cooperative network suite, template verification, Linux client/server exports and exported gameplay smoke. The smoke starts a match, buys a farm/barracks, produces food and recruits through the normal protocol. Both exports are gated by the checks; the workflow still has no upload, release, publishing or deployment step.

The first verification copy contained only tracked/nonignored source, with no build/import caches. Full CI passed there. After the lifecycle fix, the final CI ran against the updated identical source; source hashes did not change through import/export. Generated `.godot`, `bin` and `obj` directories were then removed from that owned copy and final fresh-source preparation passed again. Asset manifest SHA-256 checks and every external glTF buffer/texture dependency were checked independently. The original minimal vendored subset was approximately 1 MB; no runtime downloads are needed.

The verified client/server outputs are available in ignored `dist/client` and `dist/server`. Graphical exported clients run from their packed resources with no `--path` source argument. Dedicated-server resource stripping preserves the normal gameplay protocol; headless roles do not instantiate cameras, UI or KayKit models. Asset import and normal headless scenarios have no visual-loading errors. The deliberate occupied-port failure retains its expected engine diagnostic.

## Graphical interaction and lifecycle

Rendered source windows were driven with Godot mouse-input events through their actual controls: Start, world plot/building ray selection and bottom contextual controls, farm purchase/upgrade, barracks purchase, explicit recruitment, Ready and shared Pause/Resume. They produced normal server acknowledgments and matching resource/army/phase state in the other window. Ordinary economic commands continued longer battles. Captures were inspected for original model materials, nine indexed hex plots, resource props, distinct blue soldiers/red enemies, defender shots, automatic movement/casualties, fallen cities and terminal presentation.

Source clients retained an open frozen scene across disconnect and resumed with their session. Exported clients additionally exercised the visible Reconnect button during a paused battle and repeated reconnects in the lobby. The final lifecycle has no signal errors in those runs. This required untracking the shared RPC node through a real tree exit before replacing its transport; the view stays as a sibling and `/root/Game` is restored. This avoids the export template's RPC cache cleanup mismatch without patching the engine or changing the protocol.

The graphical source losing strategy intentionally stopped recruiting for one city, showed its elimination and the survivor's increased wave pressure, and ended at a shared defeat screen. The graphical export scenario used three original cities, let one fall, and had the survivors add a second upgraded farm and upgrade their barracks to pay for the redistributed later waves. It also checked the eliminated observer's board/outcome and viewing another player's city with editing disabled.

The physical pointer/device path on this KDE Wayland host was not independently verified: graphical widget checks used injected Godot input. These are actual rendered-widget and normal-RPC checks, distinct from headless gameplay tests. Windows/macOS graphics, network impairment, server restart recovery, account security and host migration are outside the executed POC checks; recovery is limited to the same running server.

## Launch and cleanup

The actual `mise run dev --port 17633` task was started twice. Both runs prepared the project, started one headless server and two connected graphical clients with distinct fresh session files. SIGTERM stopped only the owned children, removed their temporary sessions and released the port; a second run reached a fresh lobby instead of reusing expired credentials.

Independent `mise run server --port 17634` and `mise run client --port 17634 --session-file .sessions/role-verification.json` commands were exercised. The client bought a farm with ordinary requests, exited, and was relaunched with its exact command while the server stayed alive. Resume verification compared the public stable identity, changed peer and retained purchase. Final shutdown/session cleanup affects only those owned verification processes/files.

## Coverage map

| Contract | Evidence |
| --- | --- |
| Gold/food, nine slots, upgrades and manual soldiers | Core cases, real ENet economic actions, rendered controls |
| Three-turn cadence and three-wave outcomes | Core schedule/winning strategy, real two-client victory and losing strategy, graphical outcomes |
| Persistent damage and automatic city defense | Core timing/simultaneous damage cases and observed battles/defender shots |
| Current/future redistribution | Core 3/2 and 9/9 cases; real under-defended three-client match and graphical fallen-city/lane observations |
| Stable ownership and retry safety | Real foreign-city rejection, duplicate spending, restarted client and observer resume |
| Whole-match freeze and complete reconnect state | Core pause regression, paused ENet restart, rendered source/export reconnect |
| Requested free assets and offline imports | Vendored official subsets, licenses, checksums, dependency check, fresh import and rendered exports |
| Existing workflow and cleanup | Ordered CI, headless/export smoke, repeated dev and independent roles, owned-process/session checks |

Representative inspected captures: [source building/production](images/source-building.png), [shared defeat](images/source-defeat.png), [exported paused resume](images/export-paused-resume.png), [exported victory](images/export-victory.png), and [inspection of the fallen city](images/export-observer.png). The original exported survivors won with 100 city health and nine remaining soldiers each; the final medieval full-city run had three survivors with 100 health and seven soldiers each, while the fallen full-city player received the same victory.

## Medieval presentation baseline (before implementation)

Captured on 2026-09-30 before editing presentation code: Linux 7.2.8, Ryzen 5 5600X, Radeon RX 6800, Mesa 26.2.3, Godot 4.7.2 .NET, X11 under KDE Wayland, GL Compatibility. One graphical client and three headless clients used the ordinary protocol against a separate headless server. Each city bought a farm, upgraded it, bought a barracks, produced three times, and recruited twice before wave one. The battle was paused at tick 120: four cities, eight soldiers, sixteen enemies, two buildings per city; all cities had 100 health. No gameplay override or grants were used.

An external temporary SceneTree probe loaded the unchanged main scene and sampled frame deltas and Godot Performance monitors. VSync was disabled, FPS uncapped, and the same graphical window was resized between samples. Loading, resize and capture were excluded. Baseline captures and raw samples are in `/tmp/odot-pretty-check/baseline-*`; the probe is verification tooling, not shipped game code. Monitor values are snapshots and can lag by a second (see [Godot Performance documentation](https://docs.godotengine.org/en/stable/classes/class_performance.html)). These frame deltas measure host frame cadence, not isolated GPU time.

| Resolution | Samples | Mean / p95 frame ms | Scene nodes / resources | Draw calls / rendered objects / primitives |
| --- | ---: | --- | --- | --- |
| 1100x820 | 70,604 | 0.781 / 0.919 | 508 / 54 | 302 / 655 / 34,294 |
| 1280x720 | 12,425 | 0.790 / 0.941 | 508 / 54 | 302 / 655 / 34,294 |

Baseline scenery comprised the square slab/plots, marked lane, and existing props. The final scene must be compared with the same four-city population and rendering options.

## Medieval presentation verification

The client now uses three staggered rows of nine stable hex plots, a 34° orthographic camera, a fixed 220-unit bottom panel, and the expanded free medieval palette. The panel is about 193 physical pixels tall at 1280x720 under canvas scaling. `canvas_items` with expanding aspect uses the full wider window; the earlier baseline retained its original letterboxed aspect. Slots, balance, numerical lane positions, network data and session identities are unchanged.

Source graphical checks used the existing `click x y` command, which injects mouse events through the actual rendered controls and ordinary RPC path. The external probe converted projected/control coordinates through Godot's viewport stretch transform for resized windows (see [Viewport transforms](https://docs.godotengine.org/en/stable/classes/class_viewport.html)). Verified all nine empty centers without spending; a scenic click and panel background retained selection; farm construction, its level-two roof/upgrade, barracks construction and explicit recruitment spent the authoritative gold/food once. Ready disabled editing, Unready restored it. Combat, pause and disconnected states disabled contextual spending. Observation disabled foreign spending; city tabs cleared selection. Paused reconnect retained city, economy, buildings and soldiers, and required a new selection.

Grass, slope and alternating river joins were inspected after import; the river exits at both patch ends. An empty scene already includes wooded hills, rocks, trees, home/defender and labeled gold/food with a sack. Imported atlas/materials remain intact; a separate thin outline marks the nine plots, hover and selection. No duplicate slot-selector UI remains. All 44 manifest file hashes and glTF external dependencies were checked, with existing CC0 license coverage and pinned official sources. No runtime asset downloads occur.

The final performance comparison ran after compilation/CI activity finished, with the same rendering options and four-city population as the baseline: two buildings and two soldiers per city, sixteen enemies, paused wave one. Raw samples/captures remain in `/tmp/odot-pretty-check/final-*` and `combat-*`.

| Window | Samples | Mean / p95 frame ms | Nodes / resources | Draw calls / rendered objects / primitives |
| --- | ---: | --- | --- | --- |
| 1100x820 | 15,675 | 0.652 / 0.751 | 1,188 / 82 | 248 / 517 / 32,559 |
| 1280x720 | 15,422 | 0.663 / 0.765 | 1,188 / 82 | 276 / 545 / 33,807 |

Both exceed the 60 FPS target on this verification host. Each city has 77 terrain pieces and 14 decorative asset instances; all four patches are retained, while only the focused city and its units render. Terrain does not cast shadows; one restrained sun lights the scene. Packed scenes, imported meshes/atlas, outline meshes, immutable marker materials and normalized model bounds are reused. The four patches contain 772 scenic descendants, identical across the two samples. Node/resource population increased from the simpler baseline; visible draw calls and measured frame cadence improved. This is an uncapped desktop-host sample, not a portable hardware guarantee or isolated GPU benchmark. Physical device input and other OS graphics remain outside the executed checks.

The final `mise run ci` completed in 127.86 seconds with formatting, locked restore/build/import, all 17 core tests, the real ENet lifecycle/ownership/transfer/outcome suite, Linux client/server exports and ordinary exported build/production/recruit smoke passing. A temporary external headless-server probe recorded two scene nodes, two resources, zero render objects/draw calls, no camera, no controls and no terrain; normal headless clients were also exercised throughout the ENet suite.

A four-client ordinary economy route filled the focused city's nine plots by wave three: start with a farm and two mines, buy the barracks before wave one, upgrade the farm before wave two, add mines as income allows, and spend food explicitly. All nine bodies and roofs selected their own plots at 1100x820 and 1280x720; the actual source selection-marker positions also confirmed every slot, including repeated mine models. A second city supplied separate level-two farm, barracks and mine roof cases with observation spending disabled. All nine production turns and city switches retained the same 772 scenic descendants. Inspected full-city and combat captures at both sizes show the entire approach, home/defender and plots above the stable bottom panel.

The normal exported graphical window was verified through injected mouse events and native X11 captures, with OCR checking the displayed building type/level after every full-city roof click. It loaded from its packed resources without a source `--path`. Construction, upgrade and recruitment used actual contextual buttons and normal acknowledgments. During a paused wave-three battle, the visible Reconnect control retained all nine buildings, upgraded farm, economy and army, cleared selection, and allowed selecting the restored roof. City observation displayed the three upgraded building types. When the weak full city fell, the living attackers transferred to the surviving cities; the paused P2 capture shows its incoming enemy and the tower's shot directed along the grassy approach. The eliminated full-city player received the shared victory. The stripped server export handled the entire run and reconnect.

Updated captures include [empty landscape](images/source-empty.png), [empty lobby at 1280x720](images/source-empty-wide.png), [full city at 1280x720](images/source-building-wide.png), [wide combat](images/source-combat-wide.png), and [exported enemy transfer](images/export-transfer.png), in addition to the replaced build, reconnect, observer and victory images above.

A replaced-server check produced the expired-session refusal, exposed the visible fresh-session action, and successfully joined the new lobby. A second normal four-client run readied without building and rendered the shared defeat/fallen-roster screen. The current defeat/victory, paused/reconnecting, lobby, observation and Ready/Unready captures all use the bottom controls; earlier square/sidebar images have been replaced.

The frozen transfer snapshot placed enemy 116 (6 health) in P2 at distance 11.9333; projecting that authoritative destination put its red marker at approximately pixel (671, 174) in the inspected capture. A pixel-region check confirmed the red marker there, and P2's defender cooldown/visible shot remained consistent with that snapshot. Final source selection-marker audits covered every actual slot at both resolutions.

## Background music and settings verification

On 2026-09-30, `mise run prepare` compiled and imported the music with Godot
4.7.2 .NET. The unchanged source WAV is 39,118,090 bytes (SHA-256 recorded in
`Assets/Music/README.md`); its hash matches the supplied download. The checked-in
import settings use Detect from WAV (`edit/loop_mode=0`) and QOA
(`compress/mode=2`), with trim/normalize/downsample disabled. An external resource
probe confirmed stereo, 44,100 Hz, QOA format 3, forward loop 1, begin sample
1,548,939 and end sample 6,195,530. The imported resource is 7,900,059 bytes
(7.53 MiB), including 7,899,640 audio-data bytes. These metadata checks establish
bounds/format, independently of listening observations.

Rendered source checks used an external SceneTree probe and ordinary injected
mouse/key events. Both tabs, dropdowns, slider, numeric value and Close were
reachable at 1100×820, 1280×720, 1600×900 and 1920×1080; an 800×600 manually
resized window retained accessible settings/bottom controls and displayed a
custom size. Fullscreen reached the native 3200×1800 monitor size, disabled the
resolution selector, and returned to the retained 1280×720 window. Root window
size/mode APIs handle these changes; physical display modes are not switched.

Recovery checks restarted graphical clients with missing, malformed, individually
invalid, partially valid, oversized and valid-fullscreen configurations. Invalid
fields recovered independently; valid volume/height values survived invalid
neighboring fields. Oversized saved dimensions recovered to 1100×820. Replacing
`settings.cfg` with a directory exercised unreadable settings and failed writes:
startup used valid defaults, the new live volume continued working, and the menu
showed a save-error message. Godot itself logged the malformed ConfigFile parse
error, while startup remained successful. Preferences and resume credentials
used separate paths. Verification used an isolated `XDG_DATA_HOME` under `/tmp`.

The requested default was updated to Master 50 during implementation; fresh
startup restored 0.5 linear Master gain before autoplay. Native bus probes also
verified 0/50/100, explicit mute at zero, and an advancing music position during
mute/restore. Normal graphical shutdown was checked with verbose engine output
and exited without leaked music/playback resources after disposing the temporary
managed source-stream reference. Probe-only resource references are not the
normal client lifecycle.

Restart restored a manually resized 800×600 preference. An explicit
`--resolution 1280x720 --windowed` launch used that size; audio edits and closing
retained the saved 800×600 display preference for the next normal launch. Godot's
`OS.GetCmdlineArgs()` omits consumed display options and the hosted .NET argv is
empty, so the supported Linux target reads its own original `/proc/self/cmdline`
for override detection. Other OS override detection is unverified and needs a
platform-specific source of original arguments before those exports are offered.

An actual game-bus recording made with Godot's `AudioEffectRecord`, using a normal
seek to 138 seconds, crossed the loop end and continued near 40 seconds rather
than playing the outro/repeating the intro. The owner listened to
`/tmp/odot-settings-check/source-loop.wav` and confirmed the transition was
seamless, without a click or gap. This listening result is separate from the
imported-boundary assertions above; no custom playback scheduling was added.

A separate rootful Xwayland desktop with an actual 800×600 virtual monitor
verified the no-presets-fit case: the client chose 768×536 after decoration
allowance, offered only that custom size, and kept both the settings dialog and
bottom controls in the rendered window. The existing desktop monitor/settings
were not changed. The ordinary desktop's window manager may clamp a requested
window position into its usable area; preference restoration does not call a
position setter.

A second running graphical client retained its own bus gain and window size
while the first saved a new volume. Ordinary Esc/button/Close checks verified
opening, closing, held-key echo rejection, and dropdown-first dismissal. In a
real two-client battle, snapshots/ticks advanced in both clients with settings
open, while the number of match acknowledgments stayed unchanged. Clicking a
menu-covered world location and an underlying Farm control sent no purchase,
spent no gold, and preserved the selected plot when the dialog closed.

The final `mise run ci` passed in 126.10 seconds: locked restore, formatting, build/import,
17 core tests, real ENet lifecycle/ownership/outcome checks, client/server exports,
and the ordinary exported build/production/recruit smoke checks.

Shared pause/resume, source disconnect/reconnect, and the terminal outcome each
retained the same playing AudioStreamPlayer instance. Settings remained usable
through those transitions, and city inspection retained the music instance.
A verbose normal exit confirmed cleanup. Headless source-server and client
probes each reported zero AudioStreamPlayers, zero ClientSettings nodes and no
cached music resource, with DisplayServer `headless`; deliberately invalid
preferences remained byte-for-byte unchanged and were not parsed.

Representative captures: [source Graphics at 1280×720](images/settings-graphics.png) and
[exported Audio at 1280×720, Master 50](images/settings-audio.png). Further temporary input, recovery,
small-monitor, combat and outcome records are in `/tmp/odot-settings-check/`.

A fresh copy excluded `.godot`, bin/obj, Git, sessions and output directories.
With the original project and Downloads directory hidden in a temporary mount
namespace, `mise run prepare` completed in 7.61 seconds and new Linux client/server
exports completed in 9.28/9.34 seconds. After the final cleanup/export-filter
adjustments, fresh client/server exports completed in 10.21/8.98 seconds.
The generated QOA audio was recreated from the vendored source/import settings.
This checks preparation independently of
existing caches and original downloaded files.

The normal `mise run dev --port 17659` log confirmed one headless server and two
connected graphical clients with distinct fresh sessions. Owned SIGTERM shutdown
cleaned up its children and released the port. This check kept mise using its
installed tool directory while isolating Godot preferences with `XDG_DATA_HOME`.
Preference restoration never sets window positions.

The fresh client pack is 8,523,840 bytes and contains the 7,900,059-byte QOA
resource plus its 185-byte import mapping. The packed resource's SHA-256 is
`651e0a6adb0958c7ad122528d2a490f6d9874906cd903fb9d454d0ec5d161a1b`,
matching the fresh imported resource. The dedicated-server pack is 623,308 bytes
and contains no music entries: its export filter explicitly excludes
`Assets/Music/*`, since Godot's dedicated-server resource stripping alone retained
audio. The ordinary exported server accepted the graphical client's session.

The fresh graphical export ran without `--path`, with the original project,
Downloads, and fresh source copy hidden. Native mouse/key events opened the
top-left Settings button and both tabs, changed 1100×820 to 1280×720, selected
Fullscreen and returned to Windowed, closed/reopened through Esc, and used Close.
Restart restored 1280×720 and Master 50. A private PulseAudio null-sink capture
routed only this game's output: Master 0 produced zero-valued PCM, and restoring
50 produced audible PCM while the player continued. No microphone or other
application audio was captured.

The nested rootful Xwayland display has no window manager, so its fullscreen
request changed the menu/saved mode while retaining physical window dimensions.
Native-monitor fullscreen sizing was observed in the source desktop check above.
Godot also logged embedded-window focus/tree signal connection diagnostics during
these exported mode transitions; menu and restart checks still passed. These
backend diagnostics and native Wayland behavior remain follow-up checks.

The same normal export ran through its first and second natural authored loop
boundaries (approximately 140.49 and 245.85 seconds after playback begins).
Private game-output captures are
`/tmp/odot-settings-check/export-natural-loop-1.wav` and
`/tmp/odot-settings-check/export-natural-loop-2.wav`, each containing 6.32 seconds
of nonzero PCM. The owner listened to both and confirmed both transitions sound
seamless, without a click or gap. Together with the imported/packed boundary
checks and the source listening result, this verifies playback of the intro once
and the repeating authored section, excluding the outro. Listening observations
are distinct from metadata assertions.

All owned verification clients, server, nested display, and temporary audio sink
were shut down after the checks. Generated preferences, sessions, caches, logs,
and test tools remain outside the tracked change.

Platform/device coverage is Linux X11 on this machine, including a nested
Xwayland display. Windows, macOS, native Wayland, other audio devices, and
cross-monitor moves were not exercised. Some exported shutdowns reported one
resource still in use (and two ObjectDB instances) despite successful exit and
explicit player stop; source normal shutdown was clean in the observed run.
This remaining cleanup diagnostic is recorded separately from functional checks.

## Linux testing workflow: before-change baseline

On 2026-09-30, the unchanged clean working tree at
`c474363cfed39891f7ef5437b375dd6f77ba2c99` passed `mise run ci` on
CachyOS Linux x86_64 using the locked Godot .NET 4.7.2 and SDK 10.0.401.
Mise reported **130.17 seconds** including supervisor bootstrap, formatting,
repeated preparation, 17 xUnit cases (115 ms assertion execution), all four
sequential network groups, Linux client/server exports and headless exported
ordinary-protocol smoke. No graphical client was launched by this baseline.
The console record is in ignored `logs/test-workflow-baseline/ci.log`.

Process-log creation/last-write timestamps give approximate phase bounds:
pre-network preparation/bootstrap/rules 15.45 seconds; network peers from
12:11:45.063 to 12:13:30.583 CEST, **105.52 seconds**; subsequent template
checking/exports/exported smoke about 9.2 seconds. Approximate network group
bounds are authority/resume/victory 40.70 seconds, redistribution 33.43 seconds,
defeat 29.25 seconds and failure cases 2.14 seconds. These are filesystem-based
estimates, not instrumented phase measurements, and include owned-peer cleanup
gaps. Existing build/import and export-template caches were available; the final
fresh-source result must report that preparation difference and added UI cost
separately. This single baseline is reusable while those inputs remain unchanged;
additional complete runs are not required solely to collect benchmark samples.

### Targeted implementation checks

The owned Xvfb configuration was exercised with inherited `DISPLAY` and
`WAYLAND_DISPLAY` removed. `glxinfo -B` reported Mesa 26.2.3, llvmpipe
(LLVM 22.1.8), OpenGL 4.6 and no hardware acceleration. Source clients report
X11 and the same software renderer; assertions additionally check the actual
Dummy audio driver. This is rendering/input-state evidence, not audible
playback, native compositor or GPU-performance verification.

The selected `failure-cases` network scenario passed in 2.89 seconds, preserving
unavailable-server, occupied-port, missing-readiness and exited-child assertions
and verifying automatic bind retry without disturbing the occupied socket.
Cheap xUnit checks passed 17 core cases and 19 runner cases; runner assertion
execution took 179 ms. These include worker bounds/serial scheduling,
first-failure cancellation with awaited sibling cleanup, selection validation,
owned data/child cleanup, stale UI request ids, missing controls/captures,
unexpected engine errors, export barriers and orphaned-wrapper group cleanup.

The individually selected `settings` slice passed in 5.75 seconds (9.05 seconds
including private-display startup): actual modal input blocked a farm purchase,
actual slider/key input changed Master 50 to 51, and restarting the client
preserved its identity and preference in owned `user://` storage. Inspected
1100×820 checkpoints contain the settings controls and loaded landscape.
Evidence is in ignored `logs/20260930-110055-2eee2b0a/`.

Graphical implementation checks exposed a real WAV/playback shutdown diagnostic.
Supervised graphical quit now stops/releases the player and observes two mixer
cycles before exiting, with a bounded deadline and errors still treated as
failures. This follows the asynchronous fade/deletion behavior in
[Godot's audio server](https://github.com/godotengine/godot/blob/master/servers/audio/audio_server.cpp)
and the buffered threaded
[Dummy driver](https://github.com/godotengine/godot/blob/master/servers/audio/audio_driver_dummy.cpp).
The final settings check exited without that resource error. Display cleanup also
awaits/drains its owned process group before deleting temporary shader caches,
covering Xvfb's delayed exit after the wrapper's cleanup trap.

### Final fresh-source gate and comparison

The implemented working-tree code on top of baseline revision
`c474363cfed39891f7ef5437b375dd6f77ba2c99` passed `mise run ci` from the owned
fresh-source copy `/tmp/odot-workflow-clean-lIYgj3`, with inherited `DISPLAY` and
`WAYLAND_DISPLAY` removed. The copy initially contained no `.godot`, `bin`, `obj`
or `dist` outputs. Host NuGet/tool/template caches remained available. The sorted
source-file hash manifest has SHA-256
`0d57ab3a2747390e68ac72869fe8a15097b60151b655c17242be81a241205936`;
every copied source/lock file matched its before hash after verification. Final
documentation edits follow this run and do not change tested code.

Mise measured **112.32 seconds**, including supervisor bootstrap; the CI runner
measured **109.21 seconds**. The trace contains exactly one explicit solution
restore, build and source import. Godot's separate release publishing remains
part of each export. Evidence is retained in ignored `logs/test-workflow-after/`
(copied from the fresh run), with console output in `logs/workflow-clean-ci.log`
and source hashes in `logs/workflow-clean-before.sha256` and
`logs/workflow-clean-hashes.log`. Verified exports were copied back to `dist/`.

| Phase | Elapsed seconds | Coverage/overlap |
| --- | ---: | --- |
| Locked restore / formatting / build / source import | 1.06 / 7.18 / 1.60 / 3.82 | Serial preparation |
| Gameplay / runner xUnit commands | 1.18 / 1.32 | 17 / 19 cases, overlapping network; assertion times 128 / 207 ms |
| Complete network set | 63.00 | Two workers; all four preserved groups |
| Complete source UI set | 16.72 | Three serial slices, including display startup/cleanup |
| Client / server export | 4.46 / 3.54 | Sequential after successful source gates |
| Headless exported-role smoke | 1.01 | Ordinary build/production/recruit protocol |
| Graphical exported-package smoke | 6.55 | Includes separate private-display startup/cleanup |

The authority and redistribution groups began together and took 40.75 and
33.63 seconds. Defeat began when redistribution finished and took 29.34 seconds;
failure cases began when authority finished and took 2.87 seconds. The network
critical path therefore fell from the approximate serial baseline of 105.52
seconds to 63.00 seconds (about 40% less elapsed time). Gameplay rules and core
tests are unchanged; review of the network diff confirms every prior assertion
is retained, with added owned-error checking and automatic bind-race coverage.
No simulation acceleration was used. This is one observed boundary comparison,
not a repeated-sample benchmark: baseline network bounds came from filesystem
timestamps, and fresh preparation differs from the baseline's generated caches.

New graphical gates add 23.27 seconds including their two display lifecycles.
Despite that added coverage, measured total time was 17.85 seconds below the
130.17-second baseline. Bootstrap, formatting, imports, templates and host load
can vary; this result does not promise the same reduction on other machines.

### Expensive-slice value and observed cost

Each UI slice owns a fresh server, one graphical client and one headless observer;
settings also restarts its own graphical client. Ordinary input produces server
acknowledgments and independently observed state. All final 1100×820 screenshots
were inspected: imported landscape/materials and controls are visible, economy
shows an upgraded farm/barracks/recruit, settings shows Master 51, reconnect
restores the connected city, and the package shows its purchased farm.

| Selected slice | Risk missed by cheaper coverage | Final slice seconds |
| --- | --- | ---: |
| economy | World/control picking and input-to-authority routing; numerical/protocol checks cannot click rendered objects | 4.64 |
| reconnect | Actual recovery button and retained scene/identity; headless resume lacks the presentation/control boundary | 3.32 |
| settings | Modal click leakage and owned preference persistence through actual slider/key input and restart | 5.46 |
| exported-package | Packed UI/model/music-resource omissions and input; source checks cannot establish export completeness | 3.24 |

Slice times include peer setup/cleanup, but exclude roughly 3.1–3.3 seconds of
display lifecycle and standalone source preparation. These are observed local
costs, not deadlines. Maintenance centers on shared selectors/observations and
the normal protocol fixtures; the package slice reuses the economy helpers.
Extend these cases only for a concrete uncovered risk, rather than adding every
building, volume or display option. Prior battle/pause/transfer/outcome screenshots
and listening observations above remain historical targeted evidence. Current
recurring graphical smoke does not replay those complete matches or listen to
music; complete normal-protocol battles/outcomes remain network assertions.

### Ownership, faults and desktop distinction

Selected source slices ran independently before the final gate. Unknown options,
missing controls/captures, stale response ids, unexpected engine errors, scheduler
failure attribution/cancellation and export barriers have cheap fixture coverage.
Additional representative investigations exercised the same checked-in commands:

- An intentional Xvfb executable startup failure returned nonzero in 3.16 seconds
  with an owned-display readiness diagnostic and no desktop fallback
  (`logs/workflow-display-fault.log`).
- An intentional source-import failure returned nonzero before any scenario peers
  or network checks started (`logs/workflow-prepare-fault.log`).
- A 4-second selected authority/network deadline returned nonzero; its scenario
  cleanup completed at 4.45 seconds with several peers active. A 700-ms graphical
  deadline completed display cleanup at 0.73 seconds. Unrelated processes remained
  alive and all listed runtime directories were removed
  (`logs/workflow-network-timeout.log`, `logs/workflow-ui-timeout.log`).
- SIGTERM during selected settings with server, client and observer connected
  returned 130, removed both scenario/display runtime directories and preserved
  an unrelated process (`logs/workflow-interrupt.log`). Group cleanup fixtures
  also cover a wrapper that exits before its descendants.
- Selecting exported-package before exports existed failed clearly without
  creating packages or build/import caches (`logs/workflow-missing-package.log`).

These are diagnostic fault injections, not additional recurring game suites.
Final CI left no live owned display-group members or scenario runtime directories.
The developer's normal `settings.cfg` matched its saved SHA-256 before and after
selected settings, desktop dev and final CI; only owned preferences were edited.
Separate engine/supervisor logs identify each peer, including intentional
occupied-port errors, without logging resume credentials.

`dev` separately opened two normal `Odot - Nine Tiles (DEBUG)` windows on host
display `:0`, while the private reconnect slice completed in 3.43 seconds without
adding desktop windows. Its desktop client list remained the same two windows;
focus could move to the user's ordinary application during the run. During all
final source UI slices, a read-only X11 observer recorded the same host pointer
coordinates (3239,1426), focus (0x200000) and empty X11 client list before/after;
normal Wayland desktop applications remained running. The observer only queried
state and injected no desktop input. Test input uses Godot events inside its
private X11 connection. Those checks establish desktop isolation on this host;
physical device interaction and native compositor behavior are separate coverage.

Source and exported graphical clients reported X11, Dummy audio, loaded music
and llvmpipe with OpenGL 4.6. Both exited without the prior WAV/playback resource
error after supervised mixer draining. The authored WAV/QOA import format is
unchanged; current tests assert resource loading, gain/settings and lifecycle,
not audible quality or loop boundaries. Historical normal-close diagnostics
above are not erased by this supervised-quit verification.

The Ubuntu 24.04 workflow explicitly provisions Xvfb/Xauthority/X11/Mesa/Openbox
and executes the same required `mise run ci`, retaining its 15-minute job limit,
locked toolchain and read-only permissions. No checks silently skip, and no
upload/publish/deploy steps were added. The hosted Actions job was not run from
this workspace. Windows/macOS, native Wayland/GPU behavior, physical input and
new listening checks were not executed as part of this change.

## Strict C# compilation

On 2026-09-30, full `mise run ci` passed before strict enforcement changes in
110.56 seconds including supervisor bootstrap (runner: 107.58 seconds), and
after implementation in 109.32 seconds (runner: 106.53 seconds). Both used the
locked SDK 10.0.401 and Godot .NET 4.7.2 on this Linux host with available build,
import, NuGet and export-template caches. The final working tree is based on
`c49127317f575d1f8c5ae470cefd86f7620e9c5b`; only documentation/planning updates
followed the final run. This is a single before/after correctness gate, without
a performance claim or a new fresh-source/platform check.

All projects retain nullable reference checking, implicit usings and warnings
as errors, with explicit recommended SDK analyzers and build-time style
enforcement added. Findings were fixed without rule exclusions or suppressions:
invariant machine-facing numbers/timestamps, namespaced Godot types, cached
JSON serializer options, specific exceptions, matching override parameters and
checked native process-group signal results. Linux ESRCH is handled as an
already-exited owned group; other signaling errors fail explicitly.

Temporary source probes verified ordinary compilation rejects nullable returns
with CS8603, a code-quality violation with CA2201 and a mutable field eligible
for readonly with IDE0044. Formatting verification rejected a whitespace probe
without rewriting it. The probes were removed before final CI; their diagnostic
logs remain in ignored `logs/strict-csharp/`. A checked-in cheap regression
verifies runner numeric arguments under a culture with a different positive
sign. Final strict compilation reported zero warnings/errors; 17 core and
20 runner xUnit cases passed, including the existing descendant cleanup fixture.

Every existing integration gate passed: all four headless network scenarios,
the three source private-display UI slices, sequential Linux client/server
exports, ordinary-protocol exported smoke and graphical exported-package smoke.
The final network set took 62.48 seconds, source UI 16.24 seconds, exports
4.86/3.38 seconds and package smoke 0.84/6.66 seconds. Comparable baseline phases
were 62.89, 16.81, 4.65/3.54 and 1.09/6.65 seconds respectively; host/cache timing
variation applies. No expensive scenarios or dependency/tool lock changes were
added. Existing graphical/native-device coverage limitations remain.

Baseline evidence is in ignored `logs/20260930-114515-b476dd8a/`; final evidence,
including timing JSON and PNG checkpoints, is in
`logs/20260930-115317-d18ee2b9/`. Console copies are
`logs/strict-csharp/baseline-ci.log` and `logs/strict-csharp/final-ci.log`.
The OpenSpec sync records the strict contract in `linux-test-execution`;
specification and strict change validation pass.

## Launcher and Steam integration baseline

Before integration on 2026-09-30, the clean settings/strict-C# baseline passed
`mise run ci` in 110.05 seconds (runner: 106.81 seconds). Evidence is in
`logs/20260930-120146-8f7918d7/`; console output was captured in
`/tmp/odot-steam-baseline-ci.log`. All 17 core and 20 runner cases, four network
scenarios, three source UI slices, both Linux exports and package checks passed.
This includes the settings modal/input/restart check; existing native-device and
listening limitations above still apply.

The launcher will reuse `ClientSettings.Initialize`, `Open`, `IsOpen`, and
`BlocksWorldHover`, its Graphics/Audio controls and `user://settings.cfg`.
Preferences load and Master gain applies before the tabletop starts its single
`BackgroundMusic` player. First-launch volume remains 50, zero mutes the Master
bus while playback continues, and window overrides are not saved as preferences
unless edited. Dialog close currently restores the tabletop Settings button;
moving to application lifetime must restore an available current-screen control.
The existing WAV/import, authored intro/loop and -12 dB music gain stay intact.
Supervised shutdown stops/releases audio and drains the mixer within its bounded
deadline. Application transitions must preserve that lifecycle contract.

### Steam dependency, import diagnosis and packaging

The Linux x86_64 extension files are pinned in
`src/Game/addons/godotsteam/manifest.json`, with the archive and per-file hashes,
upstream MIT notice and a separate Valve-runtime attribution. The upstream
non-prerelease release and publisher unstable designation are both retained in
`README.odot.md`. The descriptor keeps the official entry point and Linux paths;
automatic SDK initialization is disabled. Small C# native helpers serve the
compatibility probe and application integration.

Checked-in diagnostic commands are:

```sh
mise run check-steam-extension --offline
mise run check-steam-extension
mise run export-client
mise run check-steam-extension --exported --offline
mise run check-steam-extension --exported
```

Source checks prepare and perform three additional editor imports, recreating
only the derived extension startup list each time with an owned fresh global
documentation cache. Asset imports stay cached. Exported
checks use the existing client package copied outside the source tree. `--offline`
loads the native classes without SDK initialization. Online checks require a
running, signed-in Steam client with access to the configured application;
development defaults to 480 and accepts `ODOT_STEAM_APP_ID`. They check native
host/channel configuration, C# casting/assignment, local signal marshaling,
callback polling and close/disposal/shutdown. The locally emitted diagnostic
signal creates no Steam lobby/invitation and is not remote callback evidence.
SDK diagnostic account identifiers are redacted before runner log retention.
These commands establish single-account compatibility only, never remote channel,
overlay or relay acceptance.

`export-client --steam-app-id ID` writes an explicit package `steam-app.cfg`;
default development packages use 480 without an AppID file. `export-client
--production --steam-app-id ID` selects a separate production-feature preset and
requires a positive non-480 ID before export begins. Its Steam initialization
reads the explicit package configuration instead of the development environment
override/default. Both presets exclude source AppID/config files and retain
licenses alongside native libraries. Cheap tests cover missing/480/zero package
IDs. An isolated production packaging fixture with non-480 ID 123456 passed;
that arbitrary fixture tests configuration/exclusion only, with no SDK
initialization or application ownership claim. Production Steam runtime and
genuine launch registration remain unexecuted.

On 2026-09-30 the first repository `check-steam-extension --offline` native
import exited **139/SIGSEGV**, after editor initialization/script discovery. Its
record is `logs/20260930-121259-861e272a/import.json`, with the engine output and
`import-backtrace.log` alongside it. The owned failed process was PID 205627;
the system retained its core. Subsequent exact-source/core mapping identified
Godot's deferred `EditorHelp::_gen_extensions_docs` accessing cleared document
data during shutdown. This also reproduces without C# or SDK initialization.
Upstream reports include [Godot #111048](https://github.com/godotengine/godot/issues/111048)
and [#111645](https://github.com/godotengine/godot/issues/111645).

The runner now seeds Godot's ordinary generated `.godot/extension_list.cfg`
before editor import/export and uses a fresh owned `XDG_CACHE_HOME` each time.
Both measures avoid the diagnosed deferred documentation path. It uses stock
Godot with no editor plugin, fixed delay, custom build or automatic retry.
Diagnosis is retained in `logs/steam-import-diagnosis/`, independent counterfactual
summaries in `logs/odot-import-repro-k5n9_ahl/` and
`logs/odot-import-fix-check-ko1oma5o/`, and final results in `logs/steam-import-fix/`.

A subsequent full `mise run ci` using the populated import cache passed in
108.56 seconds (runner: 106.57 seconds), with zero compilation warnings/errors,
17 core and 24 runner tests, all existing network/source-UI/package gates, and
new source/isolated-export offline native-load probes. Evidence is in
`logs/20260930-121604-1c33fce0/`; console records are copied into
`logs/steam-integration/`. The exported release library and Valve runtime matched
their pinned hashes and the license was present. The later successful import is
one subsequent result, **not** proof of a resolved initial-import crash or
fresh-source reliability. The later checks below supersede the single-account
lifecycle limitation, while real account-to-account delivery, invitations and
relay routes remain pending.

A final isolated fresh-source CI run passed in **111.75 seconds** (runner), with
17 core and 26 runner tests, all network/source-UI/export/package gates and
source/export offline native-load probes. Evidence is
`logs/steam-import-fix/20260930-124911-c087e29d/`; `tested-source.json` records
source hashes. An earlier attempt ran out of space in owned `/tmp` fixtures;
only those fixtures were moved to ignored cache before rerunning. This result,
fresh-source preparation, repeated extension discovery imports and single-account
online source/export SDK lifecycle checks establish observed import-fix coverage.
Online probes verify initialization on 480, typed local signal marshaling,
callbacks, native host assignment, channel 1 configuration, close/disposal and
shutdown; they create no lobby or invitation. Retained package guard/exclusion/
fixture logs are in `logs/steam-import-fix/packaging/`.

### External Steam prerequisites and deferred acceptance

Initial development uses **AppID 480 (Spacewar)**; registering or purchasing a
production AppID is not required. Each friend needs a running Steam client,
a distinct signed-in authorized account with access to 480, and a compatible
Linux x86_64 build with the pinned extension/runtime. Two separate machines are
required for real peer checks; different NAT networks are required for relay
acceptance. One machine/account can run the compatibility probe but cannot prove
remote reliable channels, authenticated remote identity, invitations or relay.

The user explicitly excluded testing with friends from this milestone and
authorized its local completion, spec sync and archive on 2026-09-30. Local core,
ENet, graphical and export checks pass without Steam. Unexecuted real-Steam
checks remain unchecked in the archived OpenSpec checklist;
missing prerequisites are never recorded as a pass. `mise run dev` remains the
playable desktop host-and-guest command, with no Steam login dependency.

Steam genuinely cold-launching this game's executable requires the game's own
configured AppID, correct Steam launch registration, and application access for
both accounts. Spacewar 480 may launch its own executable; manually passing
`+connect_lobby` exercises argument routing only. The non-480 packaging fixture
does not satisfy own-AppID launch/runtime acceptance. The upstream publisher's
**unstable** designation remains an independent release qualification until
the outstanding acceptance checks are observed.

### Verification layers for application sessions

1. `mise run test` exercises shared authority, start/admission policy, action
   validation, retries, identity/namespace boundaries and delayed Steam operation
   guards without Godot, display or Steam accounts.
2. `mise run test-network` uses real headless Godot processes. Existing outcome
   cases remain, alongside selectable `solo-session` and `playing-host-lifecycle`.
   Solo preserves an occupied endpoint while starting/building/restarting without
   a socket. Playing-host checks cover local/guest policy, retained paused city,
   retry after process restart, snapshot/ack progress, host end and old credentials.
   `redistribution` uses a playing host and retained eliminated guest; dedicated
   outcome, ownership, failure and resume cases remain in the suite.
3. `mise run test-ui --scenario launcher` uses real pointer/keyboard input and
   read-only geometry on owned Xvfb with Mesa/Dummy audio. It checks menu, settings,
   solo, music continuity, fresh sessions, hosted controls and actual termination.
   `exported-package` repeats representative application/gameplay transitions from
   packed Linux resources. Native-close requests target the exact owned child PID
   and observed main X11 window, never the developer's desktop.
4. `mise run test-steam --role host|guest [--lobby ID] [--exported]` runs one side
   of a real pair on separate signed-in machines. A normal desktop is required for
   the Steam overlay; this is separate from ordinary CI. The command exercises
   ordinary purchases/readiness/combat, channel-0 acknowledgments during snapshots
   and a paused checkpoint. Native connection evidence uses whitelisted
   `getConnectionInfo` state/authentication/relay flags/relay POP, never raw IP or
   identity dictionaries. Compare both records' match and paused tick/revision.

Compare gameplay at a common revision or after ordinary authoritative pause.
Two moving clients' latest snapshots can legitimately differ. Connection/readiness
changes are normalized only for retained-gameplay comparisons, never for admission
or pause assertions. No resource grants, forced outcomes, alternate peer or mock
Steam connection counts as real transport evidence.

No transport fault interceptor is needed: cheap shared-ledger tests and real
request replay after guest process restart prove a lost acknowledgment cannot
double-charge. Old-match high-sequence requests and canceled callback operations
exercise fresh-session isolation, alongside actual disconnect/restart tests.
The recurring harness keeps one ordered driver per child and bounded waits.

Absent Steam prerequisites produce a nonzero result explicitly marked
`unexecuted`. A no-display prerequisite check was verified at
`logs/20260930-134244-3c90e4f7/`; it starts no SDK, lobby or gameplay process.
Missing graphical prerequisites also report `unexecuted` with a nonzero exit,
without desktop fallback. An available display that fails rendering or assertions
reports `failed`. Successful local
CI never closes the external Steam gates. See [session ownership](sessions.md).


### Final local launcher/session verification

On 2026-09-30, fresh isolated source `mise run ci` passed in **148.29 seconds**
(runner; mise total 151.93 seconds). The snapshot excluded generated import,
build, export and session state. Its hashes and code comparison are retained in
`logs/sessions-final-ci/tested-source.json` and `source-comparison.json`.
Full evidence is `logs/sessions-final-ci/20260930-142111-40a095c1/`:

- 50 core and 35 runner xUnit cases passed, with zero compilation warnings/errors.
- All six real-process network scenarios passed in 62.39 seconds, retaining
  dedicated outcomes/failures and adding solo and playing-host lifetime checks.
- All four source UI slices passed in 34.76 seconds; launcher took 17.28 seconds.
- Ordinary Linux client/server exports, pinned native files/notices, offline
  extension loading, exported gameplay/reconnect and graphical package checks
  passed. Exported UI took 24.23 seconds including private-display ownership.

Earlier integrated CI also passed in 142.28 seconds at
`logs/20260930-140223-313eb9d9/`. Stricter graphical guest coverage then exposed
an orderly-close defect: immediately closing the ENet peer discarded the queued
reliable session-end message. Logical teardown now happens immediately while the
old transport drains until guests disconnect or the existing deadline expires.
A fresh session waits for that transport; Exit waits before SDK/audio disposal.
The final process tests require `session-ended` on orderly leave and separately
exercise abrupt host death.

Native close with Settings open initially failed because Godot's exclusive
child window suppresses the parent's close event. Settings and invitation dialogs
now use nonexclusive embedded windows with a root input guard. Actual pointer/Tab
checks prove world input stays blocked, while a native parent close works.
The final source and exported hosted UI checks open Settings in both windows,
close the host's exact owned native window, require guest return to a usable menu
with Settings dismissed, preserve music/preferences and explanatory host feedback,
then use the guest's visible Exit button. This extends the earlier test that
accepted ordinary transport loss and used a headless guest.

After the drain fix, actual `mise run dev` was checked again on an owned private
X11 desktop: two mapped host/guest windows, admitted guest, native host close,
guest `session-ended` and menu, mise exit zero, and all owned descendants/display
processes/runtime removed. Evidence is
`logs/odot-session-work-6xvk322k/dev-drain-smoke/` (11.52 seconds overall).
This command opens ordinary desktop windows for users; its diagnostic private
display does not change the default command behavior or use their preferences.

A missing-tools `check-ui-prerequisites` invocation exited nonzero and reported
`unexecuted` before starting any display/gameplay child. Evidence is
`logs/sessions-final-ci/20260930-142427-11c62bb3/`. Missing Steam/normal desktop
was likewise recorded unexecuted above. Available displays with rendering or
feature failures still report failed; neither case becomes a pass.

Before the final scope revision, the original checklist had **31/41 tasks complete** locally. Task 1.6, Steam tasks 5.2–5.4 and 5.6,
and all phase-7 acceptance tasks remain unchecked. Shared identity/resume policies
and canceled/late callback guards are tested locally, but SDK-authenticated remote
binding, real warm invitations, native channels, relay/NAT behavior, actual Steam
host departures, own-AppID cold launch and release stability require the deferred
external evidence. No real lobby or invitation was sent during local checks.


### Linux invitation overlay launch fix

The first real `mise run play` observation initialized AppID 480 and created a
hosted lobby, but neither Invite friends nor Shift+Tab opened the overlay.
Read-only inspection of that game process found Steam's client library mapped
and no `gameoverlayrenderer.so`. This distinguishes lobby/SDK availability from
an injected graphical overlay. No game input, desktop setting or process was
changed during diagnosis.

[Valve's Linux FAQ](https://partner.steamgames.com/doc/store/application/platforms/linux#4)
requires preloading the overlay renderer when launching outside Steam.
`SteamOverlayLaunch` now resolves a readable ELF64 x86-64 shared library from
known native Steam client roots and merges it with existing `LD_PRELOAD` for the
game child only. It applies to ordinary `play` and source/exported paired
`test-steam` menu launches; local/headless/private-display/probe/import/build
processes remain unaffected. No library is downloaded or installed. Missing
native Steam libraries produce launcher feedback and leave local play available.

The game uses the pinned extension's `isOverlayEnabled` and `overlay_toggled`
APIs; both signatures were checked against
[the selected release source](https://codeberg.org/godotsteam/godotsteam/src/tag/v4.22.1-gde/godotsteam.cpp).
Invite reports unavailable/loading overlay state, tracks activation after a
request with a five-second bound, and clears pending feedback on session leave.
An activation callback reports an active overlay, not proof that an invitation
was sent or received. Offline native-load probes check the real extension's
local typed overlay signal marshaling without opening Steam UI or initializing
the SDK. Online compatibility probes additionally query overlay availability.

Cheap verification passes 50 core and 51 runner tests, including 16 new cases
for launch eligibility, preload preservation, missing libraries and incompatible
ELF headers. Isolated evidence is
`logs/odot-session-work-6xvk322k/overlay/`; root tests and subsequent CI records
are retained in `logs/steam-overlay-fix/`. The installed native renderer was
found and validated. Real overlay visibility requires restarting the prior
unpreloaded process; a local marshaling test or injected library alone does not
close the pending real invitation/Steam acceptance tasks.

Full `mise run ci` after the overlay fix passed in **145.34 runner seconds**
(mise: 146.64 seconds), with all 101 unit tests, six network scenarios, source UI,
Linux exports, source/export offline typed overlay callbacks and package UI.
Evidence is `logs/20260930-144650-de55471d/`. Local dev launch eligibility is
covered by the cheap tests; its previously verified playable host/guest behavior
is preserved. No real overlay activation or invitation is claimed by this run.

### Steam login presentation and remaining local policy checks

On 2026-09-30, task **5.5** completed with authenticated playing-host policy
coverage for paused/eliminated retained-city resume, preserved retry ledgers,
wrong-account/token refusal, fresh-player rejection after roster lock, and old
credentials/commands rejected by a replacement match at the same original host.
These tests supply trusted identity at the transport boundary. Actual native SDK
peer identity mapping remains in unchecked task 5.4.

Pending consent now expires independently of create/join operations when its
session generation changes, dismissing the native dialog. Old accept/decline
delegates cannot consume a replacement invitation for the same lobby. The added
cheap cases preserve current-generation consent and verify future offers remain
usable. Isolated core evidence: `logs/steam-local-policy-validation/verification.md`.

The existing launcher slice now exercises actual native invitation controls with
Steam disabled: duplicate consent preserves the original, outside input stays
blocked, Decline retains the match/selection, and Accept returns through ordinary
session cleanup with music/preferences retained. Only the incoming presentation
boundary is triggered by an owned, supervised offline-X11 command; no SDK/lobby,
foreign match, forced gameplay outcome, or direct decision callback is used.
This closes local portions of tasks 5.3/5.6 without checking their real-Steam
acceptance requirements. It adds about 0.7 seconds to the existing launcher,
without another child or battle. Source and exported routes retain screenshots.

The menu and Multiplayer view show a bottom Steam persona/login label. Owned
offline UI checks at both sizes verify "Open Steam to log in", clickable Host
warning, no substitute session, and usable Back/solo/Settings/Exit. Native method
availability is checked in offline source/export probes. A separate single-account
`mise run check-steam-extension --exported` passed in 0.50 runner seconds at
`logs/20260930-152708-3d3b2712/`: current login and real persona lookup succeeded
without printing the name or creating a lobby/invitation. This is API compatibility
evidence, not friend/relay acceptance. Login checks also cover already-initialized
SDKs; review corrected offline-to-online callback subscription and rechecks before
consent leaves a current match. Host remains usable while a previous peer drains.

Full `mise run ci` passed in **145.92 runner seconds** (mise: 147.25 seconds),
with **55 core + 51 runner tests**, all six network scenarios (62.39 seconds),
all four source UI slices (34.36 seconds; launcher 18.02 seconds), Linux
client/server exports, native offline probes, package smoke and exported UI
(25.15 seconds). Evidence: `logs/20260930-152422-64802053/`. The earlier failed
attempt caught a static-member analyzer requirement during compilation; it was
corrected before the passing build and full suite. `mise run dev` retains its
Steam-independent playing host and guest route, exercised by existing process
and graphical hosted regression checks.

Original checklist progress is **31/41**. Tasks 1.6, 5.2–5.4, 5.6 and all phase-7
items retain their unverified real-Steam, own-AppID, or release-qualification
evidence requirements as deferred checks outside the closed local milestone. No friend
invitation, remote Steam admission, real departure, or relay route was verified
by these local checks.

### Local milestone closure and archive decision

On 2026-09-30 the user explicitly decided not to test with friends for this
change and requested updating, syncing and archiving all locally executable work.
The completed development milestone uses the passing core/ENet/source UI/Linux
export gates and single-account compatibility evidence recorded above. No game
code changed during closure; the same passing CI inputs remain applicable.

The original ten unchecked external acceptance items are retained in
`openspec/changes/archive/2026-09-30-add-start-screen-and-steam-hosting/tasks.md`
as deferred audit entries. They do not block this user-authorized local archive.
The synced specs retain the intended online behavior and distinguish local
milestone completion from actual Steam or release acceptance. Friend invitations,
remote authenticated identity/channels, relay/NAT routing, real Steam departures
and owner reassignment, own-AppID cold launch, and release stability remain
**unverified**. The existing paired runner can be used later if requested.
## Versioned distribution verification

On 2026-09-30 the distribution implementation added SemVer-tagged Linux and
Windows clients, a per-user Inno Setup installer, a versioned Linux install
script, installed-package runner slices, manual GitHub release discovery and a
browser-assisted full-download action. After a locked restore and formatting,
the suites pass 55 gameplay and 105 runner tests. The runner cases include
isolated Linux install/reinstall/upgrade/failure/uninstall fixtures, native
overlay launch policy, bounded deterministic HTTP fixtures and final release
asset allowlist/hash/identity failures. No live GitHub release or Steam account
is needed for those cases.

`mise run test-ui --scenario settings` passed in 14.49 runner seconds at
`logs/20260930-183338-a843d1da/`. It retained the existing modal/gameplay and
preference assertions while verifying the About controls and accurate
development-build update feedback on owned X11/Mesa software rendering.

The ordinary `Verify and build` workflow runs only for pull requests targeting
`main` and pushes to `main`. It runs the full source gate before parallel Linux
and native Windows package checks and has no upload permission. Native Windows
installer qualification remains accepted as deferred.
The separate `Build published release` workflow follows the owner's final
preference to skip all gameplay, network, UI and installation tests: it performs
lightweight public/tag/profile preflight, builds both tagged packages in
parallel, and attaches only the installer, archive, install script, combined
checksums and public build metadata after identity/hash consistency checks.
Selected `verify-installed-linux` and `verify-installed-windows` commands remain
available for deliberate package qualification without implicit rebuilds.

The release workflow has not published anything. The repository was still
private during implementation, and no real tag or release was created. Windows
installer, graphical interaction and genuine paired Steam
invitation/relay/cold-launch acceptance remain unclaimed.

The pinned-action nonpublishing run
https://github.com/seraph1nia/odot-game/actions/runs/36763314446 passed its source
gate and complete tagged Linux path: package build, ordinary exported checks,
installed graphical solo checks and owned uninstall cleanup. Its Windows export
and build identity passed, but ISCC printed its usage and produced no installer.
The owner accepted Windows installer qualification as deferred. The ordinary
CI workflow retains its native Windows export/build checks. Release workflow
work continues separately.


## Arch animated combat acceptance (2026-09-30)

This change retains Godot, the current authority roles, six network scenarios and
all existing cooperative assertions. It adds Arch 2.1.0 state ownership, typed
recruitment and rigged C# presentation. Protocol v4 refuses older builds.
The change’s detailed evidence ledger is
[verification.md](../openspec/changes/archive/2026-10-01-add-arch-animated-unit-combat/verification.md).

Final `mise run ci` passed in 183.78s (185.12s including runner preparation),
with evidence under `logs/20260930-205517-0bd9989c/`. It includes 80 core and
107 runner tests, all six network scenarios, all five source UI slices,
sequential Linux client/server exports and headless/graphical package smoke.
Source UI measured 60.89s including private-display startup; the combat case
measured 13.74s. Packed graphical smoke measured 31.88s, or 35.18s including
private-display startup. Locked restore, strict build and format verification
passed before source checks; all source gates passed before exports.

`mise run test-ui --scenario combat` is the fifth default source UI slice. Its
admitted risk is missing/wrong imported rigs, non-moving bones, clock drift,
visible contact, duplicated cosmetic effects, corpse leaks and stale session
views: cheap numerical tests and headless peers cannot inspect those Godot nodes.
It owns a dedicated authority, one graphical client and one headless observer,
recruits one Swordsman and one Crossbowman through actual controls, and samples
only the first wave. Setup uses ordinary farm/upgrade/barracks/ready commands.
A 60-second case deadline bounds observable waits. Expected preparation plus
execution was 20–40 seconds; selected execution measured 13.76s, or 23.45s with
shared source preparation/private-display startup. Maintaining it adds one
scenario, clip bindings and read-only pose/event observations, not a battle matrix.

The existing reconnect slice now recruits the same small army and pauses an
in-progress attack before transport recovery: selected execution measured 11.06s.
Packed smoke validates both rigs, seven required clips, materials and three
weapons, then samples short locomotion/shooting without repeating a complete
match. Economy and launcher still assert ordinary input, including both
recruitment controls at 1100×820 and 1280×720. Package-only selection consumes
existing exports and does not prepare/rebuild source.

Cheap coverage owns exact attack ticks, mutual deaths, invalid target/source,
shooting range/close contact, upgraded costs and atomic rejection, 32-vs-32 pure
and mixed contact/determinism/progress, entry queues/transfers, wave strategy,
registry disposal, 120-tick/4096-event retention and playback baselines/gaps.
The history stress fixture uses zero damage and a deliberately fast three-tick
cadence; its payload bound is not a claim of bandwidth performance at that scale.

Evidence remains under ignored `logs/<run-id>/`: non-secret owned engine logs,
timing JSON, PNGs and fresh UI observation IDs. Scopes await peers and private
displays before removing client data, and preserve other processes/preferences.
These Linux x86_64 checks use owned X11, llvmpipe and Dummy audio. They establish
software-rendered skeleton/control behavior, not native compositor/GPU cadence,
physical input, audible quality, Windows animation/export execution or real
paired Steam/overlay/relay acceptance. Those native checks remain unexecuted for
this change; previously recorded Windows qualification applies to earlier work.

## Direct Steam friends picker verification

The `add-direct-steam-friend-invites` change replaces overlay recipient selection
with an Odot friends dialog and the pinned native lobby invitation API. Existing
overlay qualification above is historical evidence, not a prerequisite for this
picker. The host selects a Steam account; an accepted send request is distinct
from invitation receipt, guest consent, native admission and relay routing.

`mise run test-ui --scenario launcher` extends its existing menu/host checks with
owned Steam-disabled fixtures at 1100×820 and 1280×720: duplicate names, a large
scrolling list, actual selection, sent/rejected outcomes, empty/unavailable
refresh, modal input, focus, incoming consent, session replacement and native
exit. Cheap invitation-policy tests cover account identity, generation/lobby
validation, removed friends and duplicate/reentrant sends. Fixtures cannot create
a real lobby, send a real invitation or initialize Steam.

Separate real acceptance uses `mise run test-steam --role guest|host --scenario
direct-invite [--exported] --timeout-ms 600000` on two Linux machines/accounts.
Start the guest first and keep both compatible AppID 480 builds running. The host
manually selects the agreed friend in Odot; the guest accepts in Steam. The
selected case requires the accepted send event and actual warm join callback,
then retains the existing ordinary shared gameplay/native connection checks.
`--lobby` is refused for this case so argument routing cannot count as delivery.
No ordinary CI command sends live invitations. Real paired acceptance remains
unexecuted for this change until two-account evidence is captured. Native GPU,
physical input, listening quality, Windows runtime and production cold launch
remain outside local software-rendered fixture evidence.

See [change verification](../openspec/changes/archive/2026-10-01-add-direct-steam-friend-invites/verification.md)
for retained before/after records and measured UI cost.


## Village character and strategy acceptance (2026-10-01)

The archived change's [verification record](../openspec/changes/archive/2026-10-01-add-village-character-and-strategy/verification.md) records the full before baseline and final coverage. This change adds catalog-driven gold/food/wood spending and nine building choices, income-free Preparation, symmetric four-role factions, class research in hundredths, splash/towers and bounded presentation. Cheap coverage owns costs, rank arithmetic/retry/no-heal, role/building parity, preparation/disconnects, contact and finite ordinary strategy families. It avoids a graphical option matrix.

Existing graphical cases were extended for concrete rendering risks. Economy exercises ordinary Lumbermill/Blacksmith/research/preparation actions, all nine terrace plots, a structural Catapult upgrade and actual pile node counts; these could fail while numerical snapshots stay correct. Combat uses only first-wave Mage/Berserker plus an observed tower city to expose missing cast/axe rigs, projectile alignment, contact interpolation, audio/effect pool bounds and frozen ambient/death clocks. Reconnect baselines living views/piles/effects without historical cues. Settings checks synthesized cues' Master routing/mute and restores its owned chosen preference. Packed smoke loads every specialist rig/socket/role clip/weapon, a real Catapult and resource piles, then briefly samples tower/locomotion/shooting. Its standalone selection still consumes existing exports without source preparation or implicit rebuild.

No new network/UI scenario was added: existing names, one ordered child driver, fresh probe IDs, ordinary protocol assertions, owned X11/XDG/endpoints, frame captures and awaited cleanup remain. Iteration failures retained owned evidence and cleaned up. The interpolation regression also gained a cheap test: unsafe contact chords hold one common prior frame instead of overlapping bodies. A stable 270px HUD reservation prevents camera target shifts when contextual controls change.

Selected execution costs: economy 21.57s (old 4.64s, +16.93s), combat 13.57s (previous animated-combat 13.76s), settings 7.35s. Preparation/display totals are recorded separately: economy 32.09s, combat 23.66s, settings 15.63s. Thus the additional specialist/tower assertion path replaces the old combat setup rather than repeating a full graphical battle. Core strategy logs record resource ledgers, recruits/casualties, health and per-wave steps; they prove these finite winning candidates, not exhaustive balance. Starting wood changed from seed 20 to 30 to retain the upgraded Farm/Barracks opening; remaining shared profiles stay symmetric.

All KayKit additions come from immutable official free-pack sources, with licenses, hashes and extracted-texture relationships in the asset manifest. Resource food uses medieval sacks rather than paid Resource Bits food. Sounds are original synthesized PCM (22,050Hz mono/16-bit, 0.12s cubic envelope and fixed-seed noise) and are instantiated only by graphical presentation. Dummy audio and software OpenGL assertions establish routing, lifecycle and mute, not physical listening quality, native compositor/GPU performance or Steam account acceptance. No dependencies/tools were upgraded and no package was published.

Final full `mise run ci` passed in 220.54s, evidence `logs/20261001-071039-07264336`: 133 core tests, 107 runner tests, all six network scenarios (43.54s), all five source UI slices (92.21s), sequential Linux client/server exports, headless package smoke (1.09s) and graphical exported-package smoke (49.64s; 52.93s with display overhead). Source launcher now recruits at the appropriate buildings using ordinary production and retains its invitation/lifecycle assertions. Frame capture samples an 80-by-80 grid so modal text and narrow geometry are represented while retaining the existing minimum-colour/file/dimension checks. Strict change validation and all 155 asset hashes pass; documentation frames come from this final CI run.

## Trio UI and overhead health verification

`adopt-trio-ui-kit` uses the existing launcher, settings, economy, reconnect and combat slices. No new scenario or full graphical battle is added. Launcher/friends/join checkpoints report actual kit style paths and full control bounds at 1100×820 and 1280×720, retaining keyboard/modal/session/exit assertions. Settings retains native dropdown, audio and owned preference checks with kit tabs/dialog/slider styling. Economy verifies real gold/food icon assignments, explicit wood text, readable resource costs and world plots above the stable bottom HUD.

Combat observations expose each live overhead bar's sampled current/effective maximum health, fraction, fill width, screen bounds, shared texture paths and input-transparent state. Existing first-wave sampling covers both factions, full/damaged bars, shared pause, casualty hiding, fresh-session cleanup and actual 1280×720 reprojection while paused. The native dropdown driver observes the first two Down selections before Enter because pointer-opened popups initially have no keyboard-focused item. Reconnect checks bars reconstructed from current living state without duplicate or historical damage playback. Package smoke uses the same assertions so source-only texture references cannot hide export omissions. Cheap tests cover wounded ranked fractions and clamping/invalid maximums; gameplay health values and wire schemas are unchanged.

The free Cozy subset, original terms, user-confirmed repository permission and derived-button/bar operations are recorded in `src/Game/Assets/TrioUI/README.md` and `manifest.json`. No runtime downloads, paid assets or tool upgrades are required. Bounded bars share textures and use camera projection without per-unit render viewports. The HUD reservation is now 300px to accommodate textured borders and costs. Frame inspection checks border stretch, icon semantics, contrast and crowding; software X11/Dummy audio evidence does not establish native GPU/compositor, physical input or listening quality.

Full after-CI passed in 224.90s, `logs/20261001-180607-c286409c/ci-summary.json`: 140 core / 108 runner tests, six network scenarios (43.61s), five source UI slices (98.91s), sequential Linux client/server exports, headless package smoke (1.09s) and graphical package smoke (50.02s; 53.33s including display overhead). Actual packed textures/provenance/bar fractions passed, and final exported menu/combat frames were inspected. Detailed iteration evidence and measured slice costs are retained in `openspec/changes/archive/2026-10-01-adopt-trio-ui-kit/verification.md`.

## Tabletop camera navigation

The camera uses the existing orthographic fit as its base. Wheel input anchors zoom to the reference ground plane beneath the cursor, with travel bounds taking priority at the edge. Held physical WASD/arrows pan in the camera's ground frame, with normalized diagonals and real frame delta independent of battle playback. Reset view restores all plots/full approach above the HUD; city/fresh-match transitions reset local adjustments, while resize and same-match reconnection retain them. Camera state is local, temporary presentation.

Existing economy/settings/reconnect/combat/package slices are extended for projection and input regressions rather than adding a separate full battle. Supervised wheel and held-key events go through ordinary Godot input; helpers release held keys in finally. Window focus checks validate the observed child PID and touch only that child or a temporary owned focus-recipient window on the private X11 display. Modal, text/dropdown and HUD input retain priority. Bar health/world anchors freeze with paused playback while screen placement follows local camera movement. See [the change verification record](../openspec/changes/add-tabletop-camera-controls/verification.md) for before/after results, measured cost and captured frames.

Full before/after CI passed in 227.54s / 244.03s, with final evidence in `logs/20261001-184547-db4a4966/ci-summary.json`: 140 core / 108 runner tests, all six network scenarios, five source UI slices, sequential Linux exports, headless smoke and graphical packed smoke. Final economy/reconnect/settings/combat slice times were 29.28s / 12.91s / 12.17s / 17.19s (before: 22.40s / 10.77s / 8.55s / 15.23s); package smoke was 51.70s (before: 51.04s). Source UI including display grew by 12.16s; timings are individual runs, not native performance measurements. Source resized/paused and packed navigation frames were inspected. Native GPU/compositor performance, physical input, listening quality, Windows runtime and paired Steam acceptance remain outside this local verification.


## Continuous countryside and supporting hexes

`fill-countryside-and-align-assets` reuses economy, launcher and exported-package cases. Economy observes instantiated footprint contacts on the three terrace levels, tower/base deck attachment, every plot's selection, foreign-city roof picking and complete coverage at overview, resize and camera limits. Launcher compares actual static positions/scales, central terrain transforms/rotations, nine plot outlines and bridge placement between menu and fresh solo; return after construction retains the passive starting landscape without match objects. Packed checks reuse these assertions and existing setup. No additional battle or option matrix is added.

Five cheap runner tests cover bounded camera movement as well as rejecting undersized scenery ranges, invalid view points, sunken/floating/off-center contacts and mismatched menu placements/terrain/scales. Graphical observations read installed mesh transforms and ground footprints; PNG inspection complements geometry checks for joins, contact and unobstructed sightlines. Godot multimeshes batch repeated terrain into 8-by-8 regions for culling, using cached imported meshes/materials; coverage only grows when the viewport/envelope needs more rings and never determines overview scale. Headless roles still instantiate no scenery.

Before CI passed in 246.02s, evidence `logs/20261001-185736-043325d8`. Iteration evidence and final measured slice costs are recorded in [the change verification record](../openspec/changes/archive/2026-10-01-fill-countryside-and-align-assets/verification.md). All graphical checks use owned Linux X11, software OpenGL and Dummy audio; they establish rendered input/projection/lifecycle behavior, not native GPU/compositor performance, physical input, audible quality, Windows execution or paired Steam acceptance.

Full final `mise run ci --timeout-ms 300000` passed in 395.00s (`logs/20261001-194033-eb0b391f/ci-summary.json`): 140 core / 113 runner tests, all six network cases, all five source UI slices, sequential Linux exports and both package smokes. Source UI took 216.32s (before 113.31s), and graphical packed smoke took 100.48s (before 52.80s). The default 180-second source suite deadline was exceeded; the existing five-minute override retains all coverage. Individual economy/launcher costs grew from 29.57/37.86s to 59.21/75.29s. Filled pixels increase software-rendering cost even with batching; these runs do not measure native performance. Final source and packed menu/solo/navigation frames were inspected. Shared observations contain 18 static placements/83 core transforms, with 952 menu terrain instances and 2016–2444 gameplay instances in 26/43–57 spatial batches. The gameplay overview fit remains 43.32361.
