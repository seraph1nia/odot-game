# Spec Delta

## MODIFIED Requirements

### Requirement: Event-driven combat feedback
Visible combat SHALL use archetype-appropriate sword/axe impacts, crossbow projectiles, magic sparks, skeleton rattles and tower effects with short sounds. Melee feedback SHALL supply restrained explicit attacker-to-target windup intent, a short local sword/axe strike accent and landed target-side impact, including far-side adjacent-hex occupants; it SHALL NOT depict an opaque continuous hand-to-target beam or imply physical weapon reach. Misses SHALL remain visually distinct from landed hits and SHALL NOT show landed target-side impact. Effects SHALL use match-scoped action/event identity and the same presentation clock as unit poses. Overlapping snapshots SHALL NOT duplicate effects. Initial admission, reconnect or event-history gaps SHALL baseline the cursor without replaying old hits, deaths or sounds. Current moving units and unexpired dying bodies SHALL be restored from complete state at their authoritative elapsed progress; an expired corpse SHALL NOT be recreated from event history. Restoring a current death pose SHALL NOT restart its interval or play its historical rattle. Pause or transport loss SHALL freeze battle effects and stop active combat sound voices without resuming historical sounds on recovery. Switching observed cities SHALL NOT play the other city's backlog. Death SHALL override incompatible living actions; visual projectiles SHALL remain cosmetic. Headless roles SHALL create no audio players or visual effects.

#### Scenario: Reconnect after a skeleton casualty
- **WHEN** a client reconnects after a skeleton death occurred
- **THEN** it reconstructs living current state and, only while the authority still retains that dying body, its current death pose without replaying the death rattle or creating an expired historical corpse

#### Scenario: Crowded battle
- **WHEN** many impacts occur together in the observed city
- **THEN** voice and visual-effect counts remain bounded while all authoritative damage is preserved
- **AND** melee intention remains linked to each current locked target without large opaque beam geometry

#### Scenario: History gap during death
- **WHEN** event history no longer contains the start of a currently unexpired death
- **THEN** the client still reconstructs its elapsed death pose from current state
- **AND** no old impact, death sound or secondary effect is replayed

#### Scenario: Missed sword strike
- **WHEN** a locked sword primary becomes invalid before impact
- **THEN** its current windup intent and rigged swing remain attributable to that action but its miss cue is distinguishable from a landed strike
- **AND** no landed target-side flash or extra damage is manufactured
