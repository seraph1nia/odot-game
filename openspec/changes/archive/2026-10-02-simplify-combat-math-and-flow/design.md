# Design

## Context

See proposal.md for the four review findings. `CombatSimulation` currently owns the Arch world, reads `UnitState` projections, plans movement, commits reservations, schedules attacks, resolves simultaneous damage and emits snapshots/events. `HexUnitState` contains action kind, sequence and interval; `UnitAttack` repeats attack identity/timing and target; `UnitTarget` repeats target identity. `Read`, `Seed`, transfer, death and wave cleanup must reconcile them.

`HexRouting.Find` performs BFS and selects a complete route for each opponent. `StartActions` then ranks those results but can skip comparison for a retained objective. The current `Visited` array prevents backward movement during an unchanged objective episode; temporary transit tokens are deliberately ignored during long-term reachability and checked at commit. Defense scheduling/resolution uses `TowerState.Slot < 0` and nullable profiles to distinguish the defender. The current protocol is 6. Engine-independent xUnit coverage already exists; the review baseline passed 256 core and 120 runner tests, while two temporary C# probes reproduced the objective and defender-splash defects.

## Goals / Non-Goals

**Goals:** Fewer independent representations and branches, explicit invariants, integer decision mathematics, one search shared by opponents, and cheap isolated tests alongside ECS integration tests. Preserve existing cooperative, spatial and lifecycle guarantees.

**Non-Goals:** Replacing Arch, introducing a general event-sourcing/reducer framework, generic policy injection, changing board geometry or default balance, adding navigation physics, or implementing twenty-wave progression. Rendering adapters consume numerical state and do not own transitions.

## Decisions

### 1. Make action variants authoritative and snapshots derived

Use a closed set of immutable C# action variants with validated constructors/factories: waiting, committed movement, attack windup and recovery. A shared action identity is monotonically increasing per unit; retain an attack-specific ordinal only where existing animation/event identities require it. Those two counters have distinct meanings and are stored once each. An attack target is an explicit unit-or-city value, avoiding boolean/id combinations. Move data owns its destination, transition and interval; attack data owns its locked primary, start, impact and ready deadlines. Recovery carries any attack outcome/history necessary for the existing presentation projection. An impact transitions to recovery exactly once, including on a miss.

The spatial/lifecycle component owns identity, position, admission and death/freeze metadata, not a second action enum/interval. Dying and terminally frozen units retain the action pose needed to reconstruct reservations and presentation but cannot act. Transfer cancels committed movement/pending impact and preserves the remaining ready deadline in a recovery action; arrival in a receiving battlefield does not reset that deadline. Queued and dying status remains orthogonal to action history because both require existing deadlines/poses.

Remove authoritative `UnitTarget` and `UnitAttack` once the new action owns their information. `HexUnitState`/`UnitState` wire fields can remain as immutable projections when that preserves compatibility, but simulation code must never read them back as competing sources of truth. Use typed fixture factories to seed canonical numerical states; validate imported test states before mutating ECS or occupancy. Reservation ownership derives from current action identity; occupancy remains an auxiliary index, independently reconstructible from complete current states.

Alternative: extract more synchronization helpers around existing records. Rejected because that retains the invalid combinations and multi-write obligation. A single huge record of optional fields is also rejected: it merely relocates the ambiguity.

### 2. Separate pure rules from the ordered authority driver

Place small focused C# rules under `src/Game.Core/Combat` for action transitions, target/goal scoring and impact victim selection. Inputs are immutable, non-null numerical values with explicit required profiles and locations. Functions return a transition, decision or impact contribution; they do not access ECS, emit events or mutate cities. Avoid copying the whole world into a new authoritative store. Arch remains the only live unit store, and a tick-local read view is discarded after use.

Keep the existing tick ordering: due arrivals/recoveries/death expiry; collect all valid unit and defense impacts against the same living view; accumulate checked integer damage; apply health changes/casualties; elimination/redistribution/admission/limits; plan compatible new actions; emit canonical evidence. The authority driver commits accepted actions and their reservations together with all validation preceding mutation. A rejected reservation must leave the canonical action unchanged. Pure rules decide eligibility; the occupancy transaction owns conflicts.

Alternative: a generalized command/effect bus or a second whole-match simulator. Rejected because it adds orchestration and another state ownership boundary. Extract only cohesive rules with meaningful tests, not pass-through wrappers.

### 3. Search once for target ranking, then construct the selected route

For an actor's immutable decision view, construct traversable cells from faction protection and available compatible footprint masks. Keep transit conflicts out of the graph; they remain temporary commit constraints. Run a full BFS from the source with integer distances and canonical predecessor lists. Unlike the current target-specific search, do not stop when one opponent's attack range is reached. Every opponent and the exposed-city endpoint is scored against this same reachability map.

Target score is lexicographic `(minimum reachable approach steps, target initiative)`; seeded choice resolves exact ties over canonical identities. Reachable objectives outrank blocked objectives. Preserve the current bounded blocked-objective fallback using static approach distance, initiative and a stable seeded tie; static movement distances must respect faction protection rather than use paths through forbidden cells. A blocked result is explicit, not an empty route interpreted as several different states.

After choosing the target, consider only its shortest reachable attack cells. Apply support screening, lower occupied/reserved capacity and canonical seeded goal-footprint choice in that order. Reconstruct only the selected route from the predecessor DAG. Distance decreases by exactly one along predecessor links, proving a reconstructed route is finite and simple. Canonical footprints and seeded keys keep the result independent of dictionary/ECS traversal. Remove attack range from the footprint-availability cache key because it does not affect footprint legality.

Keep anti-cycling episode history separate from objective scoring. All opponents use the unrestricted actor map so a new objective is not hidden by the former objective's visited cells. When the winner remains the same, retain a valid shortest cached suffix; if repair is necessary, constrain the winning route against that episode's visited cells. If those exclusions change reachability, allow one additional actor search for that selected episode only, shared by its candidate goals. If no episode-safe route exists, wait with a bounded retry. When the winner changes or its attackable cell changes, reset episode history before constructing its route. Thus the normal evaluation uses one search regardless of opponent count, with a bounded second search only for retained-episode repair; no per-opponent BFS or losing-route construction survives.

Alternative: run a complete route query per target and hide it behind a helper. Rejected because it preserves duplicate work and interleaves policy with path construction. Deleting visited history outright is rejected because it would abandon existing anti-oscillation coverage.

### 4. Treat decision caches as optimizations, not targeting rules

At arrival/recovery completion and relevant local battlefield changes, a ready actor first checks current in-range targets, then current approach scores across eligible opponents. A retained objective has no priority over a strictly better score. Committed move/windup/recovery remains unchanged until its declared boundary.

An unchanged retry preserves decision sequence/generation, scheduling rank, objective, route choice and the random key. A timer reaching its retry deadline alone does not create a new random episode. Changes in the actor's source/action boundary, opponents' identity/location/eligibility, compatible footprint availability or support screening invalidate the relevant evaluation; unrelated cities and rendering do not. Track those inputs explicitly in a small decision observation or local revision, rather than using a global occupancy revision as a substitute for target changes. If a cached suffix and winner still satisfy current ranking and legality, reuse them. When relevant inputs change, evaluate with stable keys; advance generation for a changed objective episode, not simply for observing a frame.

This clarifies existing requirements: ranking currently reachable opponents and preserving unchanged retries both remain mandatory. The current retained-objective shortcut violates the first; fixing it does not require rerolling the second.

### 5. Resolve complete defense profiles and reuse impact policy

Introduce an explicit defense identity distinguishing the built-in defender from a tower slot and retaining city/slot ownership. Resolve a non-null defense profile containing damage, windup, recovery, victim cap and hex radius from frozen configuration. The built-in defender uses every field in `CombatSettings.Defender`, plus configured defender damage; defaults stay single-target. Use the same attack timing and victim-selection rules for units and defenses, with explicit differences for range origin and exposed-city primary-only damage.

Start/resolve/stop defense actions by typed identity; confine any legacy negative-slot mapping to a snapshot adapter. Remove null-profile fallbacks and repeated tower `Single` lookups from action resolution. Canonical defense ordering and distinct actor-kind keys preserve independent random streams and existing `DefenderShot` evidence where consumers need it. The common accumulator still applies all damage before any casualty is removed.

Alternative: hardcode defender cap/radius or reject all non-default values. Rejected because the current authoring contract already accepts, validates and fingerprints them. Honoring those fields fixes the reproduced defect without changing defaults.

### 6. Verify rules in C# and use existing integration gates

Extend the existing xUnit projects with focused action factories and assertions. Pure transition tests cover invalid state construction, exact move/impact/recovery boundaries, miss recovery, transfer and frozen death poses; ECS tests cover transaction rejection and reconstruction. Decision tests cover closer/newly reachable opponents, initiative ties, blocked-to-feasible changes, retained-route repair and unchanged retries. Compare BFS shortest paths to a small independent C# reference search on bounded boards; use fixed seeds and reversed canonical insertion, not statistical/flaky random tests. Defense tests cover default single-target behavior, configured splash cap/radius, invalid primaries and simultaneous lethal unit/defense contributions.

Reproduce the review cases as permanent regressions: swordsman cell 17 retaining an objective in cell 2 must prefer an available shorter approach to an opponent in cell 8; defender cap 2/radius 1 must damage an eligible adjacent secondary once. Add natural arrival/entry scenarios so the objective defect is tested beyond direct fixture seeding. Preserve existing screening, role comparisons, protected admission, crowded completion, pause and session-lifetime checks.

Run `mise run test` during implementation. The action-storage/wire consumer refactor is substantial: run full `mise run ci` before and after the coherent change, reusing a matching recorded successful baseline when available. Use the existing selected redistribution network and combat/reconnect UI slices only when relevant integration changes justify diagnosis. No new network/UI scenario is proposed: cheap C# tests detect the numerical defects directly, and existing integration gates cover transport and presentation. Missing tools remain user-managed and count as unexecuted, not passed.

## Risks / Trade-offs

- [Action model loses historical presentation data] → Preserve derived attack ordinal, locked/frozen aim, outcome and timing fields; exercise playback, dying reconstruction and terminal freezes with existing C# tests.
- [Fresh ranking undermines stable blocked decisions] → Separate meaningful input changes from retry ticks; preserve keys and anti-cycling episodes; test both new-opponent selection and unchanged waiting.
- [One unrestricted BFS conflicts with visited-cell repair] → Rank with shared unrestricted reachability, then repair only the winner with a bounded additional search; document the exception instead of hiding target-specific searches.
- [Corrected objectives change outcomes for some seeds] → Preserve default profiles and required role/strategy guarantees; update expectations only with numerical evidence, and increment combat rules version. Historical traces are not promised compatible across rules versions.
- [Snapshot adaptation becomes a second model] → Project once at the boundary; do not import the projection into runtime rules. Remove superseded components and synchronization code in the same coherent refactor.
- [Adjacent twenty-wave proposal touches profiles] → Complete this canonical boundary first or rebase against the implementation-time source; preserve that change's independent scope and planning artifacts.

## Migration Plan

First retain both reproduced failures as C# regressions and capture the required before baseline. Introduce canonical action/rule values, migrate the authority and occupancy, then share routing and defense policies. Keep the existing wire shape as a derived adapter where possible; if fields/types change on the wire, update all consumers and increment `WireJson.ProtocolVersion` from its current implementation-time value. Increment `CombatSettings.RulesVersion` for corrected semantics, recompute fingerprints through the existing canonical serializer, and keep `SeededDecision.AlgorithmVersion` and mixer golden vectors unchanged unless its algorithm actually changes.

This is source migration for fresh matches; there is no persistent match save/replay migration. Do not maintain two runtime action models or a live dual-simulation path. Rollback reverts the coherent source change and its rules/protocol identity together. Finish with formatted C#, recorded cheap/full verification, and updated architectural/coverage documentation.
