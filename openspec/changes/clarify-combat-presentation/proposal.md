# Proposal

## Why

Automatic battles have correct numerical action boundaries but look chaotic: committed one-hex moves use long ring detours and sprint poses, while stationary melee attacks show crossing opaque hand-to-target stripes. An owned seed-one melee witness and disposable trace comparisons isolate these presentation causes without requiring a new combat model.

## What Changes

- Draw each existing committed move directly between its declared anchors, accepting intentional transient model overlap while preserving distinct standing anchors and every numerical reservation.
- Use walking with bounded, presentation-clock-driven locomotion/attack/hit blends and restrained facing transitions. Preserve authored strike markers, terminal death precedence and paused/reconnected reconstruction.
- Replace opaque melee beams with restrained directional windup intent, the actual sword/axe swing, a short local strike accent and landed target-side impact. Keep near/far shared-hex target attribution and visually distinct misses.
- Extend the existing independently selectable melee checkpoint with a bounded owned frame sequence and live-node motion/cue assertions. Add cheap motion/blend/clearance regressions and retain exact authoritative trace references.
- Keep numerical tick rate, attacks, target selection, path decisions, occupancy/capacity, statuses, action identities, concurrency and outcomes unchanged. Shared slow motion, delayed replay, lower rendering FPS and coarse numerical turns are not part of this slice.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `city-tabletop`: direct committed-step visual choreography, intentional transient overlap, walking and restrained action transitions, melee intent/strike/impact language and owned time-series evidence.
- `game-feedback`: archetype-appropriate linked melee cues without an opaque beam aesthetic, preserving action identity and landed/missed distinctions.

## Impact

Presentation seams in `src/Game/CombatLayout.cs`, `UnitView.cs`, `MeleeStrike.cs` and small pure presentation timing helpers; existing melee/contact observation and checkpoint code in `tools/DevRunner`; cheap tests, gameplay/verification documentation and the two capability deltas. No new dependency or asset, no protocol/schema/rules-version change, no economy/release/rebrand work. Full before/after CI and affected selectable UI coverage remain required. Software-rendered frames establish geometry and timed node progression, not native GPU/compositor or listening quality.
