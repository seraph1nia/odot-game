# Proposal

## Why

The deterministic hex rework has sound integer geometry but represents actions in overlapping components and mixes graph search, target selection and mutation. Review of commits `9dc9413` and `08e5760` also reproduced two numerical defects in C# tests: retained movement objectives bypass closer available opponents, and the defender ignores accepted splash configuration.

## What Changes

- Replace competing unit action representations with one typed authoritative action and pure C# transition rules; derive occupancy ownership, deadlines, targeting and snapshot fields from that state.
- Reevaluate movement objectives at eligible boundaries and relevant battlefield changes, preserving committed actions and stable choices when inputs are unchanged.
- Share one actor-specific reachability search across candidate opponents, score targets before selecting a goal, and reconstruct only the winning route.
- Give defenders and towers explicit identities and complete resolved profiles, applying accepted splash settings through the common impact/damage rules.
- Add focused C# unit and simulation regressions for both reproduced defects, transition invariants, routing correctness and deterministic retries. Numerical acceptance runs without Godot.
- Preserve default profile values, formation, protected admission, simultaneous impacts, death reservations, pause, redistribution recovery and cooperative match semantics. Intentional behavior changes are corrected objective selection and honoring non-default defender splash.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `ecs-unit-combat`: Explicit current-action consistency and movement objective reevaluation at eligible boundaries, with stable unchanged retries.
- `combat-archetypes`: Complete configured defense profiles, including defender victim cap and splash radius.
- `coop-verification`: Engine-independent C# acceptance for numerical transitions, decision policies and the two regressions.

## Impact

Primary changes belong in `src/Game.Core/Combat`, the combat projection in `World.cs`, and `tests/Game.Core.Tests`. Existing playback, rendering and diagnostic consumers may need mechanical adaptation to derived snapshot fields; numerical decisions remain in Game.Core. Preserve existing wire fields as derived projections where practical; if the serialized contract changes, increment the protocol from its implementation-time version. Increment combat rules identity for corrected decision semantics, retaining the pinned random mixer algorithm.

No new dependency, tool, asset or test framework is needed. This follows the archived `rework-deterministic-hex-combat` and is independent of `add-economy-army-and-campaign-progression` (formerly `add-twenty-wave-progression`); do not implement that change's economy, levels, bosses or wave count here, or edit its artifacts. The working-tree archive/spec synchronization is existing user work and must remain intact.
