# ecs-unit-combat Specification

## Purpose

Keep automatic unit battles deterministic and readable through reliable lane contact, timed attacks, stable identities and complete combat lifecycle handling.

## Requirements

### Requirement: Bounded lane contact and formation spacing
Each living city SHALL retain a bounded battle approach, with authoritative longitudinal and lateral unit positions. Active living bodies SHALL remain inside its bounds and separated by at least the sum of their configured radii, subject only to a documented numerical tolerance. Friendly units SHALL queue or move around blocked neighbors within that approach; opposing units SHALL stop at body contact or attack range rather than pass through one another. Initial placement and transferred arrivals SHALL preserve spacing; arrivals awaiting room SHALL remain in their destination's army or enemy allocation without being duplicated, deleted, or able to attack before entering. No scenery collision, terrain adjacency, or player movement order SHALL determine these numerical rules. Stationary blocked units SHALL NOT appear to keep walking through a neighbor.

#### Scenario: Opposing melee units meet
- **WHEN** a swordsman and an enemy approach each other from opposite ends of a city battlefield
- **THEN** they stop within melee reach without overlapping or exchanging sides and fight automatically
- **AND** repeated combat steps do not push them through one another

#### Scenario: Tens of units converge
- **WHEN** at least thirty-two soldiers and thirty-two enemies converge on one city approach
- **THEN** active bodies remain bounded and separated, with stable queues or local movement around occupied space
- **AND** reachable opponents engage and the battle makes progress without a permanent contact deadlock

#### Scenario: Blocker dies
- **WHEN** a front unit dies while living units wait behind it
- **THEN** the freed space becomes available, remaining units acquire reachable opponents, and combat continues

#### Scenario: Transfer enters an occupied approach
- **WHEN** living attackers are reassigned to a city whose arrival area is occupied
- **THEN** all transferred identities belong to that destination immediately and enter without overlapping existing units
- **AND** waiting arrivals remain accounted for in the active wave

### Requirement: Deterministic targets and timed attack resolution
Combat SHALL advance at the declared fixed simulation rate with stable identity-based tie-breaking independent of unit storage traversal. Units SHALL retain a valid target while engaging it and reacquire when it dies, leaves their destination, or becomes invalid. Each attack SHALL have an identity, start tick, impact tick and recovery interval derived from the authoritative weapon profile. At impact the authority SHALL validate the attacker, target, destination and range, apply at most one hit to each victim for that attack; single-target attacks SHALL have one victim and splash attacks SHALL use their configured bounded victim set, and accumulate all valid damage for the tick before removing casualties. A target that dies or transfers before impact SHALL NOT receive an orphaned hit. Hit reactions and visual projectiles SHALL NOT change attack timing or decide damage. Equivalent initial state and ordered requests on the same supported runtime SHALL produce equivalent ordered snapshots and events.

#### Scenario: Simultaneous lethal strikes
- **WHEN** two living opponents deliver lethal valid impacts on the same tick
- **THEN** both hits resolve before casualties are removed and both units die

#### Scenario: Target disappears during windup
- **WHEN** a selected target dies or transfers before an attack's impact tick
- **THEN** the obsolete attack does not damage another target and the attacker can acquire a valid target for a later attack

#### Scenario: Different storage traversal
- **WHEN** identical units and requests are simulated with different internal insertion or traversal orders
- **THEN** the ordered positions, health, targets, attack identities and outcome agree at each tick

#### Scenario: Splash respects impact identity
- **WHEN** one splash attack impacts three eligible victims
- **THEN** each victim receives at most one hit for that attack identity on the authoritative impact tick
- **AND** duplicate snapshots cannot cause another application of damage

### Requirement: Stable combat identities and session lifetime
Soldiers and enemies SHALL have match-scoped stable identities that are never reused within the match. Surviving soldiers SHALL retain identity, type and remaining health between waves. Redistribution SHALL retain enemy identity, type, health, origin and remaining recovery while invalidating actions tied to the former destination. Combat state and event history SHALL have one authority owner per match and SHALL be released on session end or application exit. A fresh match SHALL start with independent units and events; delayed updates from an ended match SHALL NOT affect it. Presentation nodes and transient transport identities SHALL NOT serve as authoritative unit identities.

#### Scenario: Storage reuses an internal slot
- **WHEN** a casualty is removed and a later unit is created
- **THEN** the new unit receives a different stable identity and cannot inherit the casualty's target or presentation events

#### Scenario: Fresh session after battle
- **WHEN** a process ends a match containing living units and recent combat events and starts another match
- **THEN** the new match contains only its own roster, recruited units and events, and the old match's combat resources are released
