# Tasks

## 1. Baseline and pinned rigged assets

- [ ] 1.1 Record the existing workspace changes and run `mise run ci` before implementation, or reuse a successful baseline only with unchanged source/environment inputs; verify its complete coverage, evidence paths and result are recorded, and report missing user-managed prerequisites without installing tools.
- [ ] 1.2 Vendor Knight/Rogue and the required sword/crossbow/arrow subset from the design's pinned official commit, including buffers/textures and CC0 license; update the KayKit manifest/README and verify every selected file's immutable source/hash and referenced dependency while retaining existing asset provenance.
- [ ] 1.3 Define the required clip/rig/weapon mapping and validate it after the ordinary command-line import using a checked-in C# asset validation path exposed through existing UI observation; verify skeletons, imported animation names, hand attachments and original materials resolve, and record that rendered pose/strike tuning is completed in group 6 rather than claiming an import is visual acceptance.

## 2. Arch ownership and core fixture seams

- [ ] 2.1 Add pinned Arch 2.1.0 to `Game.Core` and intentionally regenerate only affected dependency graphs/locks for the current solution and game configurations, preserving Linux/Windows RIDs and tool locks; verify subsequent ordinary locked restores and strict builds succeed and the dependency diff contains no unrelated upgrades.
- [ ] 2.2 Introduce the combat components, one private world per match and stable-ID lookup; replace mutable soldier/enemy list ownership with ECS projections and adapt dependent city/snapshot access; verify existing economy, roster and recruitment tests through `mise run test` without a second writable unit store or any Godot dependency in the core.
- [ ] 2.3 Replace direct list-mutation test setup with a controlled internal combat fixture builder and dispose owned matches; add identity-reuse, query-order and match/session teardown regressions, verifying stable wire identities and world/registry release on repeated end, fresh sessions and startup/exit cleanup.
- [ ] 2.4 Document world ownership, fixed-thread system execution, read-only projections and test fixture usage in the core/gameplay documentation; verify documented boundaries match source and the strict solution build.

## 3. Formation entry, movement and contact

- [ ] 3.1 Implement authoritative forward/lateral positions, body radii, bounded corridor formations and explicit entry queues for recruited armies and spawned/transferred enemies; verify cheap fixtures for initial placement, over-capacity arrivals, destination accounting, wave persistence and bounds with no overlapping active spawn bodies.
- [ ] 3.2 Implement shared read-buffer targeting, stable tie-breaking, valid engagement retention/front-opponent interception, range stopping and deterministic constrained local motion with swept body contact; verify opposing units cannot cross or overlap and friendly blockers produce waiting or bounded local movement with zero walking state when stationary.
- [ ] 3.3 Add contact regressions for 32 soldiers versus 32 enemies, target death, narrow blocked formations and redistribution into an occupied approach; verify per-tick separation within `1e-6`, bounds, reproducibility under reordered storage, conservation and bounded combat progress through `mise run test`, with mixed-profile coverage added once group 4 introduces ranged rules.
- [ ] 3.4 Document the corridor/radius/formation rules and entry-queue semantics in `docs/gameplay.md`; verify they preserve decorative terrain, nine city slots and the existing redistribution/outcome contract.

## 4. Typed recruitment and fixed-tick attacks

- [ ] 4.1 Add validated Swordsman/Crossbowman/enemy profiles and explicit recruitment type, with Swordsman as the omitted-type default and barracks reductions applying to either type; verify owned/foreign, unknown type, insufficient food, ready, paused, combat and duplicate request cases atomically through core/authority tests.
- [ ] 4.2 Implement attack start, windup, impact and recovery, single-target ranged impacts, target/range/destination revalidation and simultaneous damage application; verify exact impact/cadence ticks, mutual kills, target-loss cancellation, shooting-range holds and close-contact Crossbowman behavior without visual callbacks or projectile physics.
- [ ] 4.3 Integrate city-target impacts, the existing weak defender cadence, casualties, eliminated armies and enemy transfers while retaining remaining recovery; verify elimination precedence, current/future remainder allocation, disconnected-city combat and surviving type/health across waves using existing regressions and `mise run test`.
- [ ] 4.4 Keep the three-wave winning-economy regression, add a mixed-army strategy test and extend the 32-vs-32 contact fixture to both soldier profiles; verify normal termination, crowded mixed-army progress and contact invariants, and record any deliberate starting-profile adjustment and battle duration evidence in gameplay documentation rather than weakening cooperative assertions.

## 5. Snapshots, events and protocol consumers

- [ ] 5.1 Add serializable scalar formation coordinates, deployment/movement/target/profile/action fields and match-scoped event sequence/history with final casualty state; verify full JSON round trips, stable ordering, 120-tick/4096-record bounds, detectable history gaps and recorded payload sizes for the crowded fixture in cheap tests.
- [ ] 5.2 Emit ordered start/result/hit/death events and preserve pending attacks/events through pause, roster changes and redistribution; verify a short action and removal remain observable across snapshot intervals, pause advances no timing/event state, and session/world teardown clears history.
- [ ] 5.3 Advance the wire protocol to the next unused version and update authority, host/guest/solo consumers and automation parsing together, retaining `recruit <slot>` and adding an explicit Crossbowman form; verify protocol refusal, typed retry safety, channel-order/revision guards and stale-session rejection with core/runner tests and selected `authority-resume-victory` coverage.
- [ ] 5.4 Implement the presentation event cursor/state buffer with initial/resume baselining, overlapping-history deduplication, late-action seeking and gap recovery; verify its pure policy tests cover duplicate/out-of-order state, historical deaths, transfers and new-match resets, and document the protocol update and lack of mixed-version admission.

## 6. C# soldier presentation and recruitment controls

- [ ] 6.1 Build `UnitView` and programmatic AnimationTree bindings for idle, actual-motion locomotion, melee/shooting, hit and terminal death using the pinned asset mapping; verify imported clips animate the actual skeleton and attached weapons in owned rendered checkpoints, with root motion disabled and no gameplay method tracks.
- [ ] 6.2 Bind view positions/facing and clip strike/release progress to authoritative positions, target/action ticks and the common bounded interpolation clock; verify visible contact has no ID-derived offset or crossing, melee strikes and shots align with impacts, and clip-marker tuning is recorded without changing damage through rendering.
- [ ] 6.3 Retain non-interactive death visuals for at most two unpaused presentation seconds, including wave-end cleanup; freeze pose/effect/cleanup clocks on pause or unsynchronized transport loss and reset all views/cursors/buffers on session replacement; verify node observations and checkpoints show frozen progress, resumed remainder, death completion and clean fresh sessions.
- [ ] 6.4 Add distinct Swordsman/Crossbowman recruitment controls with authoritative costs and existing selection/ownership/phase guards; verify both controls route through ordinary commands, display accepted/rejected results and fit at 1100x820 and 1280x720, extending existing `economy` input assertions where relevant.
- [ ] 6.5 Extend read-only UI observations with actual unit clip/pose progress, rendered position, action/event cursor and active/death status, plus required asset binding diagnostics; verify observations match live Godot nodes and fresh probe IDs, and update asset/gameplay/README descriptions from dummy pulses to the implemented animated behavior.

## 7. Selected real-process and graphical coverage

- [ ] 7.1 Extend `authority-resume-victory` with normal typed recruitment and pending/current action state restoration, and `redistribution` with profile/recovery/active-entry spacing checks; verify each selected `mise run test-network --scenario ...` run retains its existing cooperative assertions, bounded waits and owned cleanup without new full-match scenarios.
- [ ] 7.2 Register source UI id `combat`, its risk admission and default source selection in the existing runner; implement the small fresh ordinary-gameplay setup, real melee/ranged recruitment input, animation/contact/shot/hit/death checkpoints, pending-attack pause/resume and return/fresh-session cleanup; verify `mise run test-ui --scenario combat` passes independently through the shared owned-display harness without a full three-wave match or test-only authority mutations.
- [ ] 7.3 Extend the existing graphical reconnect slice with restored living unit/profile/action and event-baseline observations using normal gameplay setup; verify selected `reconnect` retains its actual recovery-control assertion and does not replay historical effects or revive casualties.
- [ ] 7.4 Extend existing packed graphical smoke with required rigged resource/binding assertions and a short combat checkpoint using reusable setup; verify `mise run test-ui --scenario exported-package` uses existing exports without source preparation/rebuild, while stripped-server/headless smoke remains independent of graphics, audio and Steam.
- [ ] 7.5 Land runner tests for source/selected/package scenario scheduling and new observation payloads where they protect real routing/coverage behavior; run `mise run test` and document the new selectable command, defect/cost admission, measured setup/runtime, cleanup/evidence and platform limitations in README and `docs/verification.md`.

## 8. Integrated acceptance

- [ ] 8.1 Restore the solution under the completed locks, format changed C# with `dotnet format Odot.slnx --no-restore`, and run final `mise run ci`; verify cheap suites, all six network scenarios, all five source UI slices and sequential client/server exports plus headless/graphical package smoke gate overall success, preserving the current distribution work and reporting any unexecuted checks accurately.
- [ ] 8.2 Review every delta scenario against delivered tests or explicit graphical observations and record final evidence, timing, balance/profile decisions and native-platform/Steam limitations in this change's verification record; verify all acceptance tasks are supported by results, generated outputs remain ignored and no unrelated workspace change was overwritten.
