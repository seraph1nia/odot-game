# Spec Delta

## MODIFIED Requirements

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
