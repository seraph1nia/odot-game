# Verification

## Before implementation

- Workspace clean at commit `d131d94fb0dceb58879c7eb6668ec092045cb1ef`; existing distribution work is part of that baseline.
- `mise run ci` passed: 152.89 seconds for the runner pipeline (156.74 seconds including runner preparation).
- Evidence: `logs/20260930-192757-942ad9b5/`, including `ci-summary.json`, source/package PNGs and owned engine logs.
- Coverage: 55 core tests, 105 runner tests, all six network scenarios, four source UI slices, native-extension offline probe, Linux client/server exports, headless package smoke and graphical packed-resource/menu smoke.
- .NET 10.0.401 and Godot 4.7.2 .NET. Private X11 display, llvmpipe and Dummy audio. Windows execution and real paired Steam acceptance are outside this baseline.

## Implementation evidence

Results are added as the corresponding task checks pass. An imported rig alone is not rendered animation acceptance.

- Asset subset: all manifest hashes, including previously vendored assets, verified; original CC0 source files retained.
- `mise run test-ui --scenario economy` passed after adding the C# rig/clip/material validator: 16.69 seconds including preparation, 5.12 seconds for the UI scenario. Evidence: `logs/20260930-193611-aac44d13/`. Both characters and three weapons pass binding validation; movement/strike/death poses remain to be verified during presentation integration.

- Arch dependency diff checked structurally: existing package versions/content hashes are unchanged; the six affected lock files contain only Arch 2.1.0/transitives and the Game.Core edge. Debug and ExportRelease restores deliberately regenerated for this dependency addition; subsequent locked restore and strict solution builds pass.
- `mise run test` passed at the first core/playback milestone: 71 core and 105 runner cases. Extra acceptance fixtures and observation/scheduling regressions are being added before final CI.
- `mise run test-ui --scenario economy` with animated views passed: evidence `logs/20260930-201720-f4ef4cdd`, scenario 5.20s. Inspected the control layout and imported Knight with original materials.
- First complete selected combat run passed: `logs/20260930-202433-2194ec7b`, 13.21s scenario / 22.21s prepared runner pipeline. Locomotion, real bone variation, sword/crossbow bindings, melee/shot/hit/death, pose freeze, bounded corpse cleanup and fresh solo cleanup passed. PNGs inspected; later tightening adds contact assertions and a paused corpse check before final acceptance.
- Selected network `authority-resume-victory` passed in 31.28s, evidence `logs/20260930-202817-0b6cd66e`: typed Crossbowman spending/retry, pending attack state across paused process restart and all prior cooperative assertions retained.
- Selected network `redistribution` passed in 33.82s, evidence `logs/20260930-203248-6a35f416`: conserved type/profile/recovery, cancelled old action and separated active entries, plus all prior conservation/observer/next-wave assertions.
- Selected graphical `reconnect` passed in 11.06s, evidence `logs/20260930-203450-0d052608`: ordinary mixed recruitment and pause, actual Reconnect button, identical restored gameplay, living ranged rig/current action and high-water baseline with no historical effects or corpses.
- Development fixes: corrected an expected gold value in a new fixture, UI string-enum deserialization, an omitted ready/snapshot barrier in new UI setup, and a constant-array analyzer warning. The occupied-transfer fixture originally counted bodies despite valid combat casualties; its final setup uses zero damage to isolate entry conservation. Earlier failing runs are retained in logs; they are not acceptance passes.

- Final cheap iteration: `mise run test` passed all 80 core and 107 runner cases (19s/206ms assertion execution). Includes corrected profile health in crowded fixtures, occupied entry conservation with retained recovery, exact 4096-event burst eviction, shared-clock deployment visibility, idle wave-end poses, close-contact ranged rules and impact-marker seeking within floating-point precision.
- The blended AnimationTree selected combat run passed in 13.94s (`logs/20260930-204954-d366d2be`), with actual attack/hit node activation observations. Godot warned about unnamed blend-space points; names were added before final CI. No missing rig/clip, method-track or engine error was accepted. Fresh session and paused corpse assertions remain enabled.
- Added one 60s case-wide deadline, synchronized locomotion blend space, attack one-shots, filtered upper-body hit layer and terminal death override, plus restrained faction discs preserving original character materials.

- Final metrics captured in `logs/arch-combat-metrics/combat-metrics.trx`: 32-vs-32 melee clears in 1220 ticks with 7 survivors; mixed clears in 869 ticks with 19 survivors. Equivalent reversed storage produces identical ordered state/events every tick. No profile/budget nerf was used. The zero-damage, 3-tick-cadence history stress snapshot at tick 360 is 2,158,692 JSON characters with 3748 retained events (sequence 4672–8419); separate burst coverage exercises the exact 4096-record cap and expiry. This is stress serialization evidence, not a network bandwidth/performance qualification.

## Final acceptance

`mise run ci` passed with the completed dependency locks and implementation.
Evidence: `logs/20260930-205517-0bd9989c/ci-summary.json`,
`ui-source-summary.json`, `ui-package-summary.json`, owned process logs and PNGs.
Runner pipeline: 183.78s; including mise runner preparation: 185.12s.

| Gate | Result and measured time |
| --- | --- |
| Locked restore, format verification, strict build, source import | Passed; 0.91s / 10.24s / 1.18s / 2.93s |
| Cheap rules and runner suites | Passed; 80 core and 107 runner tests; 20.78s / 1.22s phases |
| Six network scenarios, two concurrent workers | Passed; 60.86s suite |
| Five source UI slices, serial | Passed; 60.89s including private-display startup |
| Economy / reconnect / settings / launcher / combat | Passed; 5.68s / 11.33s / 6.50s / 20.34s / 13.74s |
| Sequential Linux client / stripped-server exports | Passed after all source gates; 4.91s / 3.94s |
| Headless exported smoke | Passed; 1.09s |
| Selected exported-package graphical smoke | Passed; 31.88s case, 35.18s including private-display startup |

The existing exported-package scenario executed in CI against the freshly gated
exports. Read-only review of `Runner.cs` confirms the standalone
`mise run test-ui --scenario exported-package` entry checks existing packages
and bypasses source preparation. The final CI invocation exercised the same
selected UI worker and packed assertions; a redundant standalone rerun was not
required. No publishing, installation or deployment occurred.

Inspected final `combat/combat-casualty.png`,
`exported-package/packed-shooting.png` and `launcher/solo-1280x720.png`:
original character materials, attached weapons, faction markers, paused death
visuals and both recruitment controls are visible. Live-node pose and graph
activation assertions, together with successive bone samples, establish motion;
the static images alone do not establish animation timing.

## Delta scenario coverage review

All five delta specifications were reviewed against the delivered assertions.
The following groups include each scenario in the corresponding requirements.

| Delta requirement and scenario group | Acceptance evidence |
| --- | --- |
| `ecs-unit-combat`: opposing melee, tens converging, blocker death, occupied transfer | `CrowdedBattleIsSeparatedDeterministicAndMakesProgress` in both profile modes, large entry queues, melee stopping, dead-target cancellation and occupied-transfer fixtures assert separation/bounds/progress; network redistribution and rendered combat check deployed positions. |
| `ecs-unit-combat`: simultaneous lethal strikes, disappearing targets, different traversal | Exact windup/mutual-death, target death/transfer, retained cadence and reversed-storage state/event equality fixtures. |
| `ecs-unit-combat`: reused internal slot, fresh session | Monotonic public-ID and disposal/registry fixtures, authority end/replacement regressions, combat return/fresh-solo graphical cleanup. |
| `coop-city-match`: barracks clicks, missing resources, surviving army, both types, unknown/default type | Existing production/purchase/persistent-health tests plus typed atomic rejection, both-type ownership/readiness/pause/cost fixtures; economy and launcher use ordinary input; typed authority retry and network resume preserve identity and one charge. |
| `coop-city-match`: weak exposed defender, autonomous army, ranged support, reached ranged unit | Existing city-defense and winning-economy tests, mixed three-wave strategy, crowded mixed formation, exact ranged impact/hold and close-contact ranged fixtures. |
| `resumable-multiplayer`: progressed/paused reconnect, complete action between snapshots, overlapping/out-of-order history, expired window, old protocol | Full scalar/event JSON and history bounds fixtures, playback baseline/dedup/gap/late-action tests, existing protocol rejection and match/credential guards; authority-resume-victory and actual graphical reconnect assert restored living profiles/actions without historical effects or corpses. |
| `resumable-multiplayer`: teammate pause, long pause/resume, pending impact | Existing whole-match pause tests, pending-impact/event identity regression, bounded playback-clock tests including transport loss, paused attack and corpse pose assertions with resumed cleanup. |
| `city-tabletop`: recruitment/feedback, pause/reconnect, absent selection, foreign city, ranged controls | Existing economy/selection/ownership/reconnect assertions retained; both typed controls tested at 1100×820 and 1280×720 through ordinary commands. |
| `city-tabletop`: asset palette and clean-checkout animated resources | Pinned CC0 provenance/hashes and imported rig/material/clip/weapon validation; all five binding diagnostics asserted in source and exported graphical processes. |
| `city-tabletop`: contact swing, one shot across repeated snapshots, terminal casualty, frozen battle, current reconnect | Strike-marker policy tests and live attack/hit graph assertions; actual bone motion, rendered body separation, bounded shot counts, paused death freeze, ≤2s unpaused cleanup, fresh-session/event cursor and reconnect baselines. |
| `coop-verification`: redistribution/progression/host validation/contact/timing/lifetime regressions | 80 core tests, 107 runner tests and all six ordinary network scenarios pass; existing cooperative assertions preserved. |
| `coop-verification`: milestone evidence, private-display repeatability, rendered building/exit, packaged rigs | Before/after full CI, owned-display economy/settings/launcher/combat/reconnect, packed short combat plus original menu/solo/host/native-close checks; timing JSON, fresh observation IDs and PNGs retained. |
| `coop-verification`: independently selected bounded combat and graphical-independent headless behavior | Selected combat passes independently with ordinary first-wave setup and 60s deadline; scheduling/observation tests and full CI source selection pass; stripped-server/headless smoke uses no character loading or graphical/audio/Steam dependency. |

The default weapon profiles, economy and three-wave budgets were retained.
Recorded crowded-fixture simulated durations are 20.33s melee and 14.48s mixed;
they are deterministic fixture results, not native performance measurements.
The large stress snapshot remains a payload-sizing limitation to consider before
scaling normal multiplayer armies; this implementation does not claim bandwidth
qualification for that stress configuration.

Linux x86_64 acceptance used owned X11 displays, software OpenGL and Dummy audio.
Native Windows execution, compositor/GPU performance, physical input, listening
quality and genuine paired Steam/overlay/relay checks remain unexecuted for this
change. Earlier Windows/distribution qualification is preserved as earlier
evidence, not claimed as validation of these new animations.

Generated `.godot`, `bin`, `obj`, `dist` and `logs` remain ignored. Existing
distribution source and tool/version locks are preserved. The independently
appearing untracked `openspec/changes/add-direct-steam-friend-invites/` planning
directory was left untouched. No unrelated workspace change was overwritten.
