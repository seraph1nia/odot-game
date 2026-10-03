# Spec Delta

## MODIFIED Requirements

### Requirement: Complete authoritative resynchronization
Initial joins and successful resumes SHALL receive complete current match state before gameplay input is enabled. The state SHALL include the stable roster and connection status, every city's economy/slots/upgrades/army/health/research points/production progress/purchased technologies/exclusive choices, live enemies and their current destination/health, soldier and enemy types/profiles, formation positions and entry status, targets, movement state, action identities and simulation timing, technology-derived capabilities, active burn/poison/chill strengths, bounded stack identities and absolute application/expiry/next-damage timing, phase and turn/wave counters, readiness, pause state, and outcome. State updates SHALL carry a monotonically increasing revision within the match, including changes while simulation time is paused. Clients SHALL discard stale updates and reconstruct their view from the server state rather than restoring an old local simulation.

The state SHALL also carry bounded, sequenced recent combat events sufficient to observe actions and deaths between regular snapshots, with retained final casualty position and type. Event identity SHALL be scoped to the match. Clients SHALL deduplicate overlapping event history and baseline newly joined/resumed clients at the current state rather than replaying older effects. A gap beyond retained history SHALL recover from the current state without synthesizing damage, resurrecting units or accumulating an unbounded backlog. Protocol admission SHALL reject builds incompatible with typed technology purchases, research or this status-aware combat state before accepting gameplay commands.

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

#### Scenario: Reconnect restores research and periodic effects
- **WHEN** a city reconnects after earning points and choosing Fire while a current enemy remains poisoned
- **THEN** the complete state restores balances, partial progress, purchased nodes, Frost lock and the enemy's current stack deadlines before enabling input
- **AND** the client neither grants points nor reapplies historical poison damage

### Requirement: Command retry safety
The same logical command SHALL mutate authoritative state at most once, including when retried after a lost response or a reconnect. The server SHALL retain duplicate protection with the stable player identity, report duplicate/stale requests without another mutation, and reject delayed commands whose match or turn no longer matches. Clients SHALL preserve command identity across retransmission. A genuinely new user action SHALL use a new command identity.

#### Scenario: Lost recruitment response
- **WHEN** recruitment succeeds but the response is lost and the player retries the same request after resuming
- **THEN** exactly one soldier exists from that request and its food cost was deducted once

#### Scenario: Delayed ready request
- **WHEN** a ready request from a resolved turn arrives during a later turn
- **THEN** it does not mark the player ready or trigger production for the later turn

#### Scenario: Lost technology purchase response
- **WHEN** a technology purchase succeeds and the same request is retried after a lost response or reconnect
- **THEN** it retains exactly one point deduction, one acquired node and its original exclusive lock
- **AND** a genuinely new request for that acquired or conflicting node is rejected without mutation
