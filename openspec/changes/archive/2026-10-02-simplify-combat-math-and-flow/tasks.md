# Tasks

## 1. Establish the numerical regression boundary

- [x] 1.1 Record source/environment inputs and a successful full `mise run ci` before baseline, reusing an existing success only when inputs match; retain run evidence and identify any missing prerequisite as unexecuted without installing tools.
- [x] 1.2 Add permanent C# tests reproducing the cell-17 retained-cell-2 versus closer-cell-8 objective defect and defender cap-two/radius-one secondary-damage defect; verify each fails for its intended assertion against the pre-fix implementation.
- [x] 1.3 Record the regression scenarios and numerical scope in `docs/verification.md`; verify the documented cheap command is `mise run test` and no new engine-based scenario is required for these defects.

## 2. Consolidate action state and pure transitions

- [x] 2.1 Introduce validated canonical waiting/move/windup/recovery action variants and explicit unit/city targets under `src/Game.Core/Combat`; verify C# tests reject invalid intervals/identities and exercise exact completion, miss and recovery boundaries.
- [x] 2.2 Migrate unit action scheduling and impact resolution to pure transition rules consumed by the authority driver; verify same-tick arrival/impact, simultaneous lethal strikes and recovery-end lethal damage tests preserve declared ordering.
- [x] 2.3 Migrate occupancy commit/reconstruction to derive reservation identity from the canonical action; verify rejected moves leave action and reservations unchanged and moving/dead states independently reconstruct matching reservations.
- [x] 2.4 Migrate transfer, admission, death, wave stop/reform and session disposal to canonical actions, preserving recovery deadlines and frozen poses; verify existing combat lifecycle, authority combat, protected-admission and session-lifetime tests pass.
- [x] 2.5 Derive legacy snapshot/event timing and target fields from the canonical state, adapt `CombatFixture` to validated canonical factories, and delete superseded `UnitTarget`, `UnitAttack` and action synchronization writes; verify no numerical path reads a derived projection back as authoritative state and playback/fixture tests pass.
- [x] 2.6 Document action ownership and tick order in the repository's gameplay/architecture documentation; run `mise run test` and verify examples agree with the single-action model and retained death/transfer semantics.

## 3. Share graph search and correct objective ranking

- [x] 3.1 Extract an actor reachability query with integer BFS distances and canonical predecessor lists, sharing it across all opponents and ignoring temporary transit-token conflicts until commit; verify distances against an independent bounded C# reference search and protected-cell/footprint legality tests.
- [x] 3.2 Extract lexicographic target scoring and goal selection, applying screening/capacity preferences only after selecting the target and reconstructing only its winning route; verify existing distance/initiative/seeded targeting, feasible-detour and screening tests plus C# route adjacency/simple-path assertions.
- [x] 3.3 Replace the retained-objective shortcut with reevaluation at eligible action boundaries and relevant local target/occupancy changes; verify the review objective regression passes and add arrival/entry and blocked-to-feasible C# integration cases that select a newly better opponent without interrupting a committed action.
- [x] 3.4 Preserve unchanged decision keys/ranks and retained-episode anti-cycling history; reset previous-episode exclusions on a changed objective and use a bounded winner-only repair search when necessary. Verify unchanged blocked retries, visited-cell repair, new-objective traversal and unrelated-city changes cannot produce inappropriate rerolls or partial moves.
- [x] 3.5 Remove per-opponent BFS/losing-route construction and unused attack-range dimensions from availability caching; verify focused search accounting establishes one ordinary reachability expansion per actor evaluation and at most one additional retained-episode repair, independent of opponent count.
- [x] 3.6 Document the target/goal ordering, local cache invalidation and bounded repair exception in gameplay/verification documentation; run `mise run test` and verify ordinary crowded completion, screening and role-comparison guarantees remain meaningful.

## 4. Resolve complete defense profiles

- [x] 4.1 Introduce explicit defender/tower identities and complete frozen defense profiles; verify profile tests retain default damage/timing and expose configured defender victim cap/radius without nullable fallbacks.
- [x] 4.2 Use shared pure attack/victim rules for defense start/impact and remove negative-slot branching from numerical flow, confining legacy mapping to a projection boundary if needed; verify the defender splash regression passes alongside default single-target, cap/radius, invalid-primary and no-friendly-fire checks.
- [x] 4.3 Verify unit, defender and tower contributions use the same checked simultaneous accumulator with canonical actor keys; add C# integration assertions for same-tick lethal contributions, pause, elimination and exactly-once victims while retaining existing tower/splash coverage.
- [x] 4.4 Document complete defense configuration and unchanged defaults in gameplay/verification documentation; run `mise run test` and verify accepted settings and fingerprint inputs match effective impact behavior.

## 5. Integrate derived snapshots and rules identity

- [x] 5.1 Increment combat rules identity for corrected objective semantics and keep the seeded mixer algorithm/version pinned unless its algorithm changes; verify canonical fingerprint and RNG golden-vector tests and fixed-seed reversed-insertion traces pass.
- [x] 5.2 Adapt playback, layout, animation and diagnostics mechanically to canonical-state projections where needed; preserve the existing serialized shape where practical, otherwise increment the implementation-time protocol and update all consumers. Verify C# serialization/current-state reconstruction and stale-protocol refusal tests pass.
- [x] 5.3 Update README and `docs/verification.md` with the final numerical boundaries, commands and compatibility implications; verify no rendering adapter makes numerical choices, no second runtime action model remains, and no twenty-wave/economy changes entered scope.

## 6. Complete the coherent implementation gate

- [x] 6.1 Restore `Odot.slnx` in locked mode and run `dotnet format Odot.slnx --no-restore` for changed C#; verify formatting passes without dependency/tool-lock changes.
- [x] 6.2 Run full `mise run ci` after the coherent implementation, using selected existing redistribution/combat/reconnect slices only for relevant diagnosis; record the complete run result and attributable evidence, preserving all cooperative assertions and reporting missing prerequisites as unexecuted.
- [x] 6.3 Review the final diff against all four original findings and the delta scenarios; verify competing action stores, retained-objective priority, per-opponent graph searches and null defense fallbacks are removed, and final documentation accurately distinguishes cheap numerical coverage from network/UI/export coverage.
