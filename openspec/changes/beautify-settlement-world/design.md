# Design

## Context

See proposal.md for motivation and the city-tabletop delta for behavior. The actual landed base is `0ce098de4f35982e30a2e7638c0b8107d06102c6`. Default authority combat cells occupy columns -1..1, rows -5..1. `CombatLayout` maps them through shared `VillageLayout.Hex`; tower effects use actual measured building bounds. Latest user choice is **Fixed building positions**. Those mappings and every plot/model center/scale must remain unchanged.

The initial concurrent before-CI failed the existing 15s graphical startup. Approved isolated economy passed. The one graphical-cap-one full before-CI passed all 967 cheap tests, six network cases, economy and reconnect, then the owned .NET worker segfaulted in `libclrjit.so`; launcher/combat/exports/packages did not finish. A later approved isolated settings check passed; it does not explain the crash or retroactively pass that producer. Preserve these limits in verification records and complete final full acceptance on the changed head.

## Goals / Non-Goals

**Goals:** a composed settlement perimeter with timber paths that actually reach the bridge, restrained woodland and mushroom clearings, grounded contacts, readable growth, and matched measured costs.

**Non-Goals:** plot relocation or reduced building scales, new authority hexes/crossings, a road through combat, shifted tower/projectile anchors, unit/light/camera changes, source asset production, new engine framework or numerical optimization.

## Decisions

1. Keep `VillageLayout`, `CombatLayout`, lighting, camera fit, dynamic structures and stockpiles unchanged. Add only non-battle `VillageLandscape` composition and its observations. Retain all battle-side static props and existing exterior placement for rows <=1. Avoid the alternative of phase/type-dependent layout switching, which would violate fixed physical plots or true shot origins.
2. Use the existing straight authored stream and replacing bridge, not invented curves whose water edges would not join. Add a connected bank-side timber path using the already bundled bridge-plank resource; its landmarks explain where the bridge leads. Walkways sit on their actual support surfaces and avoid plot/building footprints. Reuse the existing shared static-geometry/instancing seam only if its fidelity validation succeeds; no speculative global flattening or cache work.
3. Compose a small number of irregular grove/mushroom pockets beyond the plot perimeter instead of scatter-everywhere density. Tall new props stay well outside combat and plot sightlines; new cosmetic geometry must not cast shadows into the battle boundary. Grouping is decorative and stable; no resource/economy meaning is attached.
4. Reuse existing economy/launcher/countryside acceptance owners for contacts/connections, picking, menu parity and actual early/grown overview/close captures. Extend cheap semantic controls before adding expensive setup. A read-only authored-scale replay supplies matched render/resource measurements and identical controlled inputs, not live gameplay acceptance; the owned real combat fixture separately supplies current pose/receipt/capture/persistence evidence.
5. Compare the protected battle region under the exact same scripted state/camera, plus installed transform/placement/unit observations and real current battle assertions. Scope permitted pixel differences to non-battle presentation; numerical references and asset fidelity are not waived. Capture-state differences in ordinary live tests are not called identical-state comparisons.

## Risks / Trade-offs

- [Software rendering and additional authored surfaces can consume existing case budgets] → bound prop/path count, reuse instancing only with faithful geometry, measure the same input/backend/resolution, and retain all original timeouts. No native-GPU/FPS promises.
- [New foliage occludes roofs or casts into combat] → keep tall pockets beyond playable perimeter, disable shadow casting on new non-battle decorations, inspect both supported sizes and verify all nine actual roof/plot targets as growth changes.
- [Paths meet the bridge at the wrong elevation] → observe actual installed transforms/bounds and bridge deck endpoints, check contacts and inspect close captures, not a placement-plan screenshot.
- [Before evidence remains partial after native worker failure] → retain failed producer, reuse completed applicable slices, obey finite diagnostic authorizations; final changed-head full CI/no-mistakes coverage remains mandatory.

## Migration Plan

No save/protocol migration: presentation only. Existing runtime state identities and immutable assets remain compatible. Revert presentation changes to restore prior composition without converting game data. No publishing, version bump or automatic merge.
