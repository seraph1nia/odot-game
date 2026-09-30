# Spec Delta

## MODIFIED Requirements

### Requirement: Engine-independent cooperative rules tests
The rules task SHALL verify economy/ownership validation, recruitment, upgrades, exactly-once production, ready eligibility, three-turn wave cadence, persistent soldiers, city defense, simultaneous eliminations, current/future enemy redistribution with integer remainders, and victory/defeat. It SHALL additionally verify typed recruitment and invalid-type atomicity, range-based stopping, body spacing, crowded entry queues, target invalidation, fixed-tick windup/impact/recovery, event deduplication and bounded history, stable combat identities and session cleanup. Tests SHALL include a deterministic mixed-army case with at least thirty-two soldiers and thirty-two enemies and check progress, bounds and separation. It SHALL verify that pause freezes simulation, pending impacts and events and resume does not catch up. Tests SHALL run without Godot, Steam, or a display and fail with nonzero exit status on violations. Shared authority/session tests SHALL verify equivalent solo, host-local, and guest request validation, authenticated identity binding, command retry protection, and stale-session rejection without changing gameplay rules.

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
- **WHEN** units overlap, pass through opponents, become permanently blocked despite reachable targets, or shoot beyond their configured range
- **THEN** a core rule test fails with the violated position, spacing, progress or range invariant

#### Scenario: Timing or lifecycle regression
- **WHEN** an attack damages twice, hits an invalid target, advances during pause, or an ended match leaks combat state into a fresh session
- **THEN** a cheap rule or authority lifecycle test fails without launching an engine process


### Requirement: Recorded graphical and clean-checkout verification
Verification SHALL document and automate graphical start-screen navigation, solo entry, hosted local lobby/gameplay, direct hex-plot and building selection, purchase/upgrade/recruit controls, model/material loading, settings input isolation, typed recruitment, rigged locomotion/melee/shooting/hit/death animation, contact positions, automatic battles and transfers, pause/reconnect feedback, return to menu, and application exit. Graphical E2E SHALL use real input through rendered controls and assert displayed feedback plus authoritative results; screenshots or protocol-only automation SHALL NOT substitute for those UI assertions. Verification SHALL include fresh-source preparation and Linux client/server export smoke for solo and playing-host flows. Unexecuted platform or graphical checks SHALL be recorded as limitations rather than passed. Expensive coverage SHALL follow the documented risk/cost admission policy. Records SHALL distinguish rules, headless networking, private-display graphics and actual native display, GPU, physical input and listening observations; software rendering and silent audio SHALL NOT be claimed as native compositor, GPU-performance or audible-playback verification.

#### Scenario: Review the completed milestone
- **WHEN** implementation is reported complete
- **THEN** its verification record distinguishes automated rule/network checks from actual graphical/export observations and any checks that could not be run

#### Scenario: Repeat UI verification without desktop interaction
- **WHEN** a developer or Linux CI runs the graphical smoke tasks
- **THEN** assertions and rendered checkpoints run on a private display, identify their evidence, and do not require desktop focus or a physical screen

#### Scenario: Build through rendered controls
- **WHEN** graphical E2E clicks a plot and the displayed construction button
- **THEN** the UI shows the correct selected plot and cost, the authority acknowledges the purchase, and another player's view agrees on the resulting state

#### Scenario: Exit via the displayed button
- **WHEN** graphical E2E activates Exit Game on the start screen
- **THEN** the actual process exits within its deadline and leaves no owned gameplay process

#### Scenario: Rigged resources fail in a package
- **WHEN** a required character, weapon, skeleton binding or clip is missing or unusable in the packed client
- **THEN** graphical package verification fails rather than silently substituting a dummy or relying on source assets


## ADDED Requirements

### Requirement: Bounded combat presentation coverage
One independently selectable source graphical combat slice SHALL exercise the actual melee and ranged recruitment controls and ordinary combat progression. It SHALL assert active locomotion, attack states, coherent contact positions, shot/hit event consumption, a casualty death sequence, freeze/resume behavior and fresh-session cleanup with rendered checkpoints and current presentation observations. Cheap tests SHALL own numerical and exhaustive lifecycle invariants; real-process network coverage SHALL extend existing combat/reconnect checks to typed units and action state. The graphical slice SHALL use owned setup, a single ordered driver per child, bounded observable waits and cleanup under the existing private-display harness, and run with the unfiltered source UI suite before exports. Packed graphical smoke SHALL check required rigged assets and clip bindings without repeating full headless battles. Its documented admission SHALL identify defects cheaper coverage misses, expected setup/runtime/maintenance cost, measured results and remaining platform limitations.

#### Scenario: Selected graphical combat slice
- **WHEN** only the combat presentation slice is selected on a prepared supported machine
- **THEN** it runs from its own fresh state, observes the required animation/contact milestones, retains non-secret PNG/log/timing evidence and releases its owned peers and display
- **AND** it does not depend on another slice, require a full three-wave graphical match or manipulate authoritative state through a test-only command

#### Scenario: Headless behavior stays graphical-independent
- **WHEN** typed armies battle on a stripped dedicated-server export or an automated headless role
- **THEN** the same state and attack rules advance without creating animated models, graphics, audio or a Steam session
