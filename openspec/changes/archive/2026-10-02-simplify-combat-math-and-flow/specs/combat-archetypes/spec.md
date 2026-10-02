# Spec Delta

## ADDED Requirements

### Requirement: Complete configured defense behavior

Every built-in defender and buildable tower SHALL apply its validated damage, windup, recovery, maximum victim count and splash radius from the frozen combat configuration. A built-in defender SHALL retain a distinct action identity and remain outside building-slot capacity. Accepted defender victim/radius settings SHALL NOT be silently replaced with single-target defaults. Default built-in defender and Arrow Tower profiles SHALL remain single-target. All defenses SHALL use the existing locked-primary, distance/initiative/seeded victim selection and common simultaneous-damage rules: only living deployed enemies in the same city allocation are eligible, each action hits a victim at most once, and an invalid primary causes a miss without secondary damage.

#### Scenario: Configured defender hits an eligible secondary
- **WHEN** a built-in defender configured with victim cap two and radius one impacts a valid primary with an eligible second enemy one hex away
- **THEN** both enemies receive the configured damage once on the impact tick
- **AND** no other victim exceeds the cap or radius

#### Scenario: Default defender remains single-target
- **WHEN** the default built-in defender impacts a primary inside an enemy cluster
- **THEN** only the primary receives its ordinary configured damage

#### Scenario: Configured splash cannot survive an invalid primary
- **WHEN** a splash-configured defender's primary dies or transfers before impact
- **THEN** the action misses without secondary damage and retains its original recovery deadline

#### Scenario: Unit and defense impacts remain simultaneous
- **WHEN** a unit and a defense have valid lethal contributions due on the same tick
- **THEN** all valid contributions are accumulated before casualties prevent subsequent actions
- **AND** changing storage traversal cannot remove a due valid contribution

