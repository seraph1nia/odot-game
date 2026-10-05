# authored-asset-distribution Specification

## Purpose

Make the game's complete 3D presentation reproducible offline from owner-permitted authored exports without changing its authoritative identities or mechanics.

## Requirements

### Requirement: Complete authored export distribution
All active and bundled in-game 3D assets and their dependent materials, textures, buffers and clips SHALL come from available game-ready `odot-game-assets` exports. The distribution SHALL contain only required exports, their import metadata and accurate provenance identifying the immutable source revision, source-relative paths and byte hashes. Owner permission SHALL be recorded without inventing an open-source license; unrelated external reference images, editable sources and scratch archives SHALL NOT be distributed. Dormant legacy 3D bundles and dead import mappings SHALL be removed. UI/audio, historical documentation and code-driven non-asset indicators/effects SHALL remain independently identifiable and SHALL NOT be presented as migrated 3D assets. Missing required exports SHALL fail presentation validation rather than loading a legacy or primitive replacement.

#### Scenario: Offline source and package loading
- **WHEN** a clean checkout is prepared or a graphical package is launched without the asset-source checkout
- **THEN** every required building, character, equipped weapon, projectile, stockpile, upgrade attachment and terrain/scenery model loads from the recorded authored exports with its material and animation dependencies
- **AND** neither distribution contains legacy 3D bundles or external reference/editable-source files

#### Scenario: Verify copied export provenance
- **WHEN** the required asset inventory is validated
- **THEN** recorded source paths resolve to the permitted immutable revision and copied bytes match their hashes
- **AND** the notice records the owner's exact permission without claiming permission for unrelated third-party material or resolving Trio UI rights

### Requirement: Presentation identities preserve gameplay compatibility
Player-visible names SHALL describe the selected silhouettes and their existing roles honestly. Canonical building/unit/faction IDs, session/save/protocol identities, numerical rules and frozen authoritative traces SHALL remain compatible. Bakery SHALL continue the food-producer role, Woodcutter hut the wood-producer role, Magic academy the magic-recruitment role, Bombard tower the existing area-tower role, Knight the sword role and Archer the friendly ranged role. Town hall capacity, paid recovery, stored selection, transfer, survivor homes and retire SHALL retain their authoritative guards and effects.

#### Scenario: Substitute a producer and ranged unit
- **WHEN** a player constructs a Bakery or recruits an Archer through the existing eligible controls
- **THEN** names and previews match the authored models while accepted requests and synchronized state retain Farm and Crossbowman respectively
- **AND** costs, output, upkeep, targeting, damage and previously saved/session state retain their existing meaning

#### Scenario: Use the authored Town hall roster
- **WHEN** a player selects the authored Town hall and transfers, recovers, returns or retires a soldier
- **THEN** controls and truthful model/name previews retain the existing stable storage/home identities, quotes and eligibility
- **AND** appearance neither grants recovery nor bypasses occupancy, ownership, phase or generation guards

### Requirement: Authored rig integration is cosmetic
All eight faction/role combinations SHALL load their authored skinned rigs, original materials, equipped socket bindings and idle/walk/run/attack/hit/death clips. Loop and one-shot behavior SHALL follow explicit presentation metadata. Pose sampling SHALL align the authored attack marker to authoritative impact, retain fixed endpoints and suppress horizontal root motion; imported callbacks SHALL NOT invoke gameplay. Actual required sockets and visible equipment following them SHALL be validated. Pause, reconnect, repeated events and death cleanup SHALL retain the existing animation and combat assertions.

#### Scenario: Sample a paused authoritative impact
- **WHEN** an authored attack is sampled at its declared impact and the shared battle is paused
- **THEN** the rig uses its mapped attack marker and matching socket-bound equipment, frozen until resume
- **AND** health changes, targeting, movement reservations and numerical impact deadlines remain authority-owned

### Requirement: Source-preserving game-side render corrections
Game-side render/import corrections SHALL preserve authored source bytes, provenance and supported geometry/material/UV/rig/animation/interaction semantics. Ordinary export updates SHALL invalidate existing scene/terrain/bounds caches using source/import content, engine/backend and cache implementation version, without bespoke per-revision mesh lists or a second manually maintained asset set. Static derived rendering SHALL require executable buffer/topology/material/attribute and actual-render controls, including updates/cache invalidation and unsupported-state rejection, before same-scene quantitative cost evidence. Fewer draw calls without useful practical latency and faithful appearance SHALL NOT justify enabling a candidate. Original imported geometry SHALL remain the faithful representation for unsupported or rejected optimizations; experimental branches without a continuing concrete purpose SHALL NOT ship.

#### Scenario: Receive a changed authored export
- **WHEN** an ordinary copied export/provenance update is processed through the locked importer
- **THEN** cached scene geometry, terrain templates and placement bounds cannot reuse a stale content/import identity
- **AND** source bytes, accurate provenance, native resource sharing and original interaction semantics remain intact

#### Scenario: Observe actual live poses without input settling
- **WHEN** an owned ordered read-only live observation is admitted
- **THEN** it observes a distinct actual completed render with a fresh id and current stage/authority/camera/pose/health/equipment data, without reusing a cached observation or substituting a later screenshot
- **AND** input/layout observations retain their appropriate settling barriers, while twelve live observations and four raw captures remain within twelve seconds and exact immutable persistence alone may use a separately fully awaited thirty-second maximum
- **AND** walking/attack/death/liveness/cleanup and required near/far milestone witnesses remain unchanged; partial progression or live coverage cannot substitute for missing required captures

### Requirement: Cost-aware owned migration evidence
Verification SHALL retain full before/after CI results and owned graphical captures covering both supported resolutions, empty and populated villages, active near/far combat and roster controls. Required export loading, complete packaged inventory, labels/role mapping, clip/socket behavior and terrain adjacency/contact SHALL have executable consumer-based checks. Render/resource observations SHALL be recorded with hardware/software limitations; recommendations SHALL distinguish observed gaps from speculative features and SHALL NOT authorize their implementation.

#### Scenario: Review a completed migration
- **WHEN** the migration is handed off
- **THEN** inspectable before/after evidence, full CI outcomes, focused consumer checks and measured resource/render costs accompany it
- **AND** finite prioritized follow-ups identify benefit, evidence, approximate effort, asset/product dependencies and deliberately unimplemented work
