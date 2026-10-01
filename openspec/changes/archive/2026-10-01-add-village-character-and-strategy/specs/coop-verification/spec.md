# Spec Delta

## MODIFIED Requirements

### Requirement: Engine-independent cooperative rules tests
The rules task SHALL verify economy/ownership validation, typed recruitment, multi-resource costs, building upgrades, bounded research, symmetric faction profiles, mage/tower splash, exactly-once production, ready eligibility, three-production wave cadence and income-free final preparation, persistent soldiers, city defense, simultaneous eliminations, current/future enemy redistribution with integer remainders, and victory/defeat. It SHALL additionally verify typed recruitment and invalid-type atomicity, range-based stopping, body spacing, crowded entry queues, target invalidation, fixed-tick windup/impact/recovery, event deduplication and bounded history, stable combat identities and session cleanup. Tests SHALL include a deterministic mixed-army case with at least thirty-two soldiers and thirty-two enemies and check progress, bounds and separation. It SHALL verify that pause freezes simulation, pending impacts and events and resume does not catch up. Tests SHALL run without Godot, Steam, or a display and fail with nonzero exit status on violations. Shared authority/session tests SHALL verify equivalent solo, host-local, and guest request validation, authenticated identity binding, command retry protection, and stale-session rejection without changing gameplay rules.

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

#### Scenario: Fractional research has a small effect
- **WHEN** a low-damage unit receives a five-percent research rank
- **THEN** rule tests verify the intended small numerical increase without truncation to zero or rounding to a whole extra damage point


## ADDED Requirements

### Requirement: Recorded strategy and focused graphical coverage
Verification SHALL compare checked-in ordinary-gameplay frontline, mixed-army, tower-heavy and research-heavy strategies in solo matches and retain a shared winning strategy across one through four players without privileged resource or casualty commands. Evidence SHALL record costs, production/spending stages, recruited archetypes, wave duration, casualties, remaining resources and city health. At least one reproducible ordinary strategy in each family SHALL win the default three-wave solo match; a no-investment strategy SHALL still lose. Cooperative success, elimination and redistribution assertions SHALL remain mandatory. These finite strategies SHALL NOT be reported as proof that every possible build is balanced. Graphical coverage SHALL extend existing selectable economy, combat, reconnect, settings and exported-package cases where practical; new expensive cases SHALL document their unique defect, missed cheaper coverage and expected cost before admission.

#### Scenario: New option has no viable strategy
- **WHEN** a default tower-heavy or research-heavy strategy cannot win despite correct ordinary actions
- **THEN** implementation records the result and adjusts profiles or costs before reporting balanced completion
- **AND** existing cooperative assertions are not removed to obtain a pass

#### Scenario: Observe a graphical specialist attack
- **WHEN** private-display combat coverage observes a Mage cast or a Catapult Tower impact
- **THEN** it checks actual rendered poses/effects against current authoritative events and captures attributable frames
- **AND** an overlapping snapshot or reconnect does not replay an old sound or effect

