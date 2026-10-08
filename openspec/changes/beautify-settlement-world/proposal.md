# Proposal

## Why

The landed authored landscape leaves the village as a small dense island in a repetitive meadow. Give its non-battle surroundings a deliberate, connected woodland-village composition without undoing performance work or changing gameplay.

## What Changes

- Preserve all nine physical plot/building centers and scales, home/defender, real tower shot origins and the battle approach. The latest decision is **Fixed building positions**; increased center-to-center spacing is not part of delivery.
- Connect the settlement edge to the existing authored river crossing with a restrained timber footpath and bank-side clearing; compose woodland and mushroom pockets with readable negative space and focal points.
- Reuse integrated immutable authored models/materials and deterministic game-side placement. No asset production, dependency update, gameplay or release.
- Extend existing owned acceptance coverage for connections, supporting contacts, unobstructed plot picking and battle boundary controls; retain real current captures and matched bounded render/resource diagnostics.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `city-tabletop`: fixed-layout non-battle countryside composition with connected settlement paths and deliberate woodland/mushroom clearings, while retaining unchanged battle presentation and existing plot interactions.

## Impact

Presentation work is scoped to `src/Game/VillageLandscape.cs` and its existing observation/placement seam. Verification reuses `tools/DevRunner` owned process/display/input/capture infrastructure and cheap semantic controls. `src/Game.Core`, combat layout/units, global lighting/camera/hex mapping, source asset workspace, provenance and tool/dependency locks remain unchanged. Documentation records the failed native before-CI and subsequent selected evidence honestly; full final acceptance is still required.
