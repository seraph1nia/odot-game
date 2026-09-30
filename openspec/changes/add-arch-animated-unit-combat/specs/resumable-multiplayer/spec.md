# Spec Delta

## MODIFIED Requirements

### Requirement: Complete authoritative resynchronization
Initial joins and successful resumes SHALL receive complete current match state before gameplay input is enabled. The state SHALL include the stable roster and connection status, every city's economy/slots/upgrades/army/health, live enemies and their current destination/health, soldier and enemy types/profiles, formation positions and entry status, targets, movement state, action identities and simulation timing, phase and turn/wave counters, readiness, pause state, and outcome. State updates SHALL carry a monotonically increasing revision within the match, including changes while simulation time is paused. Clients SHALL discard stale updates and reconstruct their view from the server state rather than restoring an old local simulation.

The state SHALL also carry bounded, sequenced recent combat events sufficient to observe actions and deaths between regular snapshots, with retained final casualty position and type. Event identity SHALL be scoped to the match. Clients SHALL deduplicate overlapping event history and baseline newly joined/resumed clients at the current state rather than replaying older effects. A gap beyond retained history SHALL recover from the current state without synthesizing damage, resurrecting units or accumulating an unbounded backlog. Protocol admission SHALL reject builds incompatible with typed recruitment or this combat state before accepting gameplay commands.

#### Scenario: Reconnect after match progression
- **WHEN** turns or combat progressed while a player was absent
- **THEN** their resumed client displays the current authoritative state, including any losses or transfers, rather than the state from disconnection

#### Scenario: Reconnect during pause
- **WHEN** a player resumes while the match is paused
- **THEN** they receive the paused state and current connection/readiness state even though combat time has not advanced

#### Scenario: Snapshot interval contains a complete action
- **WHEN** an attack impact or casualty occurs between two snapshots received by an already synchronized client
- **THEN** retained event identity and timing let it present that action or death once even if the unit's current action or living presence has already changed

#### Scenario: Duplicate or out-of-order combat state
- **WHEN** a client receives overlapping recent events or an older state revision
- **THEN** it keeps its newest authoritative state and does not replay already consumed strikes, shots, hits or deaths

#### Scenario: Resume after the event window
- **WHEN** a client resumes after missing more combat than the retained event history covers
- **THEN** it receives current living units, profiles, positions and in-progress actions without replaying a historical effect backlog
- **AND** missing historical events do not affect health or outcomes

#### Scenario: Old combat protocol attempts admission
- **WHEN** a build without the compatible typed-recruitment and combat-state protocol attempts to join
- **THEN** the authority refuses clearly before binding that connection to gameplay


### Requirement: Whole-match pause and resume
Any connected roster member SHALL be able to request pause or resume during a building phase or battle. Pause SHALL freeze production, phase advancement, combat movement, attack timing, health changes, combat event progression, and outcomes; economic and ready actions SHALL be rejected while paused. Connection handling, resynchronization, and pause/resume requests SHALL remain active. Resume SHALL continue from the frozen state without consuming accumulated wall-clock time. Repeated pause/resume requests SHALL be harmless. A disconnect SHALL NOT itself pause or resume the match.

#### Scenario: Teammate pauses an ongoing fight
- **WHEN** a player disconnects and a remaining member pauses during combat
- **THEN** soldiers, enemies, defender attacks, and city health remain frozen until a member resumes
- **AND** the disconnected player can reconnect during the pause

#### Scenario: Resume after a long pause
- **WHEN** a paused match is resumed after a long wall-clock delay
- **THEN** combat continues with the remaining attack timers and positions rather than applying catch-up damage

#### Scenario: Pause retains a pending impact
- **WHEN** the authority pauses between an attack start and its impact tick
- **THEN** the pending action, its remaining windup/recovery, targets and event sequence remain frozen across steps and reconnect
- **AND** resuming permits the single pending impact at its remaining scheduled simulation time
