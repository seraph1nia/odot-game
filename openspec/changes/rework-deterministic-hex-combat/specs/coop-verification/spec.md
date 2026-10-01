# Spec Delta

## MODIFIED Requirements

### Requirement: Engine-independent cooperative rules tests
The rules task SHALL verify economy/ownership validation, typed recruitment, multi-resource costs, building upgrades, bounded research, symmetric faction profiles, mage/tower splash, exactly-once production, ready eligibility, three-production wave cadence and income-free final preparation, persistent soldiers, city defense, simultaneous eliminations, current/future enemy redistribution with integer remainders, and victory/defeat. It SHALL additionally verify typed recruitment and invalid-type atomicity, range-based holding, legal shared-hex footprints/capacity, faction-exclusive reservations, crowded entry queues, closest/initiative/seeded targeting, fixed-tick movement/windup/impact/recovery/death, event deduplication and bounded history, stable combat identities and session cleanup. Tests SHALL include a deterministic mixed-army case with at least thirty-two soldiers and thirty-two enemies and check ordinary completion without battle-stalled defeat, board bounds, capacity/footprint separation and conservation. It SHALL verify that pause freezes simulation, moves, pending impacts, death-space releases, no-progress deadlines and events and resume does not catch up. Cheap tests SHALL compare seeded combat traces under reversed insertion order and different session identities, validate configuration, exercise exact scheduling/target/route ties and unchanged retries, prove same-tick arrival/impact/death ordering, and verify no-progress/duration defeat reasons and normal-result precedence. Fixed-seed crowded setups SHALL cover bounded waiting, cleanup and transfer without requiring graphical processes. Cleared-board transfer coverage SHALL assert actual protected-entry admission at its compatible release bound and ensuing engagement, not just conserved queues. Small paired normal-profile/formation fixtures SHALL establish frontline support protection, queued melee access, effective Mage contribution against ordinary two-victim clustering and Crossbowman single-target advantage. Required ordinary three-wave strategies and these role/admission/crowd checks SHALL run at the early gameplay gate before broad protocol/reconnect/package integration. Tests SHALL run without Godot, Steam, or a display and fail with nonzero exit status on violations. Shared authority/session tests SHALL verify equivalent solo, host-local, and guest request validation, authenticated identity binding, command retry protection, and stale-session rejection without changing gameplay rules.

#### Scenario: Redistribution regression
- **WHEN** enemy transfer duplicates an enemy, restores its health, loses a remainder, or assigns an enemy to a fallen city
- **THEN** a rule test fails and identifies the violated invariant

#### Scenario: Progression regression
- **WHEN** repeated readiness creates extra production, a wave begins at the wrong turn, or a fourth wave starts
- **THEN** a rule test fails

#### Scenario: Host and guest validation diverge
- **WHEN** a host-local action bypasses an ownership, cost, phase, or retry check enforced for guests
- **THEN** a shared authority test fails and identifies the differing behavior

#### Scenario: Contact or range regression
- **WHEN** units share reserved positions illegally, exceed hex capacity, enter an opposing hex, cross conflicting routes, become permanently blocked in an ordinary reachable setup, or shoot beyond their configured hex range
- **THEN** a core rule test fails with the violated position, capacity, reservation, progress or range invariant

#### Scenario: Timing or lifecycle regression
- **WHEN** an attack damages twice, hits an invalid target, advances during pause, or an ended match leaks combat state into a fresh session
- **THEN** a cheap rule or authority lifecycle test fails without launching an engine process

#### Scenario: Fractional research has a small effect
- **WHEN** a low-damage unit receives a five-percent research rank
- **THEN** rule tests verify the intended small numerical increase without truncation to zero or rounding to a whole extra damage point

#### Scenario: Seed or initiative regression
- **WHEN** a target tie, contested move or equivalent route depends on storage traversal, rendering, or an unchanged retry
- **THEN** a cheap reproducibility test fails with the seed, decision and first differing combat tick

#### Scenario: Corpse releases capacity too early
- **WHEN** a casualty stops blocking before its authoritative death deadline or a moving casualty loses an endpoint reservation
- **THEN** a cheap lifecycle test fails without launching Godot

#### Scenario: Stalled battle is bounded
- **WHEN** a no-damage or cyclic movement fixture reaches its declared combat limit
- **THEN** a cheap test observes battle-stalled defeat at the exact tick with preserved living health

#### Scenario: Early gameplay gate catches a blocked reinforcement
- **WHEN** conservation holds but transferred enemies cannot deploy or attack on a previously cleared default battlefield
- **THEN** the early gate fails before broad wire/reconnect/package integration proceeds
- **AND** reaching the stalled-fight deadline cannot count as passing ordinary reinforcement admission

#### Scenario: Reinforcement bound accounts for fragmentation
- **WHEN** a cleared protected entry has multiple death deadlines and its earliest release does not fit any queued profile
- **THEN** a cheap fixture verifies the retained first-admission bound from cumulative legal footprint releases and actual admission at the compatible deadline
- **AND** retries cannot postpone the bound or require the whole overflow allocation to fit at once

### Requirement: Bounded combat presentation coverage
One independently selectable source graphical combat slice SHALL exercise the actual melee and ranged recruitment controls and ordinary combat progression. It SHALL assert active locomotion, attack states, distinct fixed hex positions and coherent reserved movement routes, shot/hit event consumption, a casualty death sequence aligned to authoritative reservation expiry, freeze/resume behavior and fresh-session cleanup with rendered checkpoints and current presentation observations. Cheap tests SHALL own numerical and exhaustive lifecycle invariants; real-process network coverage SHALL extend existing combat/reconnect/redistribution checks to typed units, seeded configuration, movement/death state and conserved reservations. The existing graphical reconnect slice SHALL restore current moving/dying state without historical effects, and packed smoke SHALL sample the same hex/action mapping. These extensions SHALL retain cooperative assertions and independently owned setup; exhaustive seed/capacity/timing matrices SHALL remain cheap coverage. A short early sub-selection of the existing source combat slice SHALL prove explicit abstract melee using actual rigs in two occupied neighboring cells, including near/far targets and a simultaneous exchange, before layout scale is accepted. It SHALL use ordinary setup and progression with retained frames; coordinate separation or protocol agreement alone SHALL NOT establish visual readability. The graphical slice SHALL use owned setup, a single ordered driver per child, bounded observable waits and cleanup under the existing private-display harness, and run with the unfiltered source UI suite before exports. Packed graphical smoke SHALL check required rigged assets and clip bindings without repeating full headless battles. Its documented admission SHALL identify defects cheaper coverage misses, expected setup/runtime/maintenance cost, measured results and remaining platform limitations.

#### Scenario: Selected graphical combat slice
- **WHEN** only the combat presentation slice is selected on a prepared supported machine
- **THEN** it runs from its own fresh state, observes the required animation/hex-position/death-deadline milestones, retains non-secret PNG/log/timing evidence and releases its owned peers and display
- **AND** it does not depend on another slice, require a full three-wave graphical match or manipulate authoritative state through a test-only command

#### Scenario: Headless behavior stays graphical-independent
- **WHEN** typed armies battle on a stripped dedicated-server export or an automated headless role
- **THEN** the same state and attack rules advance without creating animated models, graphics, audio or a Steam session

#### Scenario: Reconnect observes a current death
- **WHEN** the existing reconnect slice synchronizes during an unexpired authoritative death
- **THEN** the rendered body samples its current pose without a historical sound and disappears at its declared deadline
- **AND** restored living units retain matching hex positions and action progress

#### Scenario: Early visual gate catches empty-air melee
- **WHEN** fixed-anchored models swing without a readable strike-to-target relationship across neighboring cells
- **THEN** the short source combat proof remains failed even if numerical range and occupancy assertions pass
- **AND** cue/anchor/spacing candidates are corrected before accepting the board layout
