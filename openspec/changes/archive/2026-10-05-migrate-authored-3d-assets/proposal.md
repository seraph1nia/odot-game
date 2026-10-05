# Proposal

## Why

The completed art audit identified repetitive yellow-green terrain, small settlement silhouettes and duplicated producer models as major presentation weaknesses. The owner has authorized replacing all in-game 3D assets with available authored `odot-game-assets` exports and subsequently polishing the environment.

## What Changes

- Replace every active 3D model and its support content, including Town hall, roster previews, equipment, projectiles, stockpiles, upgrade attachments and countryside; remove dormant bundled legacy 3D files and imports rather than retaining a fallback.
- Vendor only needed game-ready exports from revision `a640d721065233dfbf488747694cd95110d4a907`, with exact file hashes and owner-permitted provenance. Do not invent an open-source license or copy external reference images/editable sources.
- Explicitly map presentation labels to available silhouettes: Bakery retains Farm's food-producing role, Woodcutter hut retains Lumbermill's wood-producing role, Magic academy retains Arcanum's magic-recruitment role, Bombard tower retains CatapultTower's numerical profile, Knight/Archer retain Swordsman/Crossbowman identities.
- Adapt authored six-clip rigs and equipped sockets in presentation only. Preserve canonical IDs, save/session/protocol compatibility, numerical combat and frozen traces, roster/storage/healing/return/retire rules and existing selection/control contracts.
- After replacement works, compose a softer authored meadow/forest landscape with deliberate clusters, a continuous river/bridge, grounded appropriately scaled buildings and unobstructed playable space. Keep render-only elevation and decoration outside simulation.
- Record full before/after CI, focused owned graphical evidence at both supported resolutions, import/binding/completeness checks and practical render costs, then deliver finite evidence-based follow-up recommendations.

## Capabilities

### New Capabilities

- `authored-asset-distribution`: Complete, owner-permitted, offline 3D export provenance and compatibility-preserving presentation mapping.

### Modified Capabilities

- `city-tabletop`: Replace the mandated KayKit palette with authored export silhouettes and materials, preserve controls/animation contracts, and update countryside composition to available source terrain.

## Impact

Graphical presentation (`UnitAssets`, `UnitView`, inspector, shared village landscape, tabletop construction/stockpiles, menu lighting and labels), vendored 3D content, typed asset validation and existing runner checks/documentation. No tool/dependency update, version bump, release, UI/audio migration, source-project mutation, new gameplay feature or merge authorization. Trio UI public-source rights remain independently unanswered.
