# Spec Delta

## Purpose

Keep one authoritative cooperative match consistent across validated client commands, disconnections, reconnects, and pauses without losing city ownership.

## ADDED Requirements

### Requirement: Authoritative ownership and command validation
The server SHALL own match phase, resources, buildings, armies, city health, enemy state, readiness, pause state, and outcomes. Clients SHALL submit requests rather than authoritative changes. The server SHALL derive ownership from the authenticated connection, validate match/turn context, command values, costs, phase, and permitted actions, and return success or failure feedback. Incompatible protocols and malformed or excessive commands SHALL fail without disrupting other players. A client SHALL NOT modify another city or decide combat damage, production, elimination, or outcomes.

#### Scenario: Attempt to modify another city
- **WHEN** a player submits construction, upgrade, or recruitment targeting another player's city
- **THEN** the server refuses and both cities retain their existing state

#### Scenario: Stale or invalid request
- **WHEN** a client submits an action for an earlier turn, an unknown action, invalid parameters, or an incompatible protocol
- **THEN** it receives a clear failure and no authoritative game state is changed by that request

### Requirement: Stable identity and private resume credentials
Each player SHALL have an identity independent of the transport connection. The server SHALL issue a private unguessable credential that can reclaim that identity in the same running match. Credentials SHALL NOT appear in shared snapshots or diagnostic logs. Only one active connection SHALL control a player at a time. Invalid credentials SHALL NOT reclaim a city or silently create a replacement city after match start. Resume credentials SHALL be retained locally so restarting a client process can reclaim its city; two local clients SHALL be able to use separate session storage.

#### Scenario: Resume with a new connection
- **WHEN** a disconnected player reconnects with valid credentials through a different transport connection
- **THEN** they regain their original player identity and city without a new starting grant or enemy allocation

#### Scenario: Invalid or concurrent claim
- **WHEN** a client presents invalid credentials or credentials for a player still controlled by an active connection
- **THEN** the claim fails clearly and the legitimate owner's state and connection remain intact

### Requirement: Disconnected state retention
A disconnected player's city, resources, buildings, soldiers, health, elimination status, and original wave allocation SHALL remain in the server's match state. The city SHALL continue normal production and combat whenever the match advances unpaused. A disconnect SHALL clear that player's readiness without eliminating the city, transferring its enemies, or deleting its army. Server restart recovery and cross-server resumes SHALL NOT be promised by this POC; an unavailable or replaced server SHALL produce clear client feedback rather than an apparent restoration.

#### Scenario: Disconnect during combat
- **WHEN** a player loses their connection while the match is unpaused
- **THEN** their army and defender continue fighting and their city remains a target
- **AND** enemies are redistributed only if the city's health reaches zero

#### Scenario: Resume after elimination
- **WHEN** a disconnected player's city falls and the player later reconnects
- **THEN** they receive the eliminated city and current match as an observer without reviving it

### Requirement: Complete authoritative resynchronization
Initial joins and successful resumes SHALL receive complete current match state before gameplay input is enabled. The state SHALL include the stable roster and connection status, every city's economy/slots/upgrades/army/health, live enemies and their current destination/health, phase and turn/wave counters, readiness, pause state, and outcome. State updates SHALL carry a monotonically increasing revision within the match, including changes while simulation time is paused. Clients SHALL discard stale updates and reconstruct their view from the server state rather than restoring an old local simulation.

#### Scenario: Reconnect after match progression
- **WHEN** turns or combat progressed while a player was absent
- **THEN** their resumed client displays the current authoritative state, including any losses or transfers, rather than the state from disconnection

#### Scenario: Reconnect during pause
- **WHEN** a player resumes while the match is paused
- **THEN** they receive the paused state and current connection/readiness state even though combat time has not advanced

### Requirement: Command retry safety
The same logical command SHALL mutate authoritative state at most once, including when retried after a lost response or a reconnect. The server SHALL retain duplicate protection with the stable player identity, report duplicate/stale requests without another mutation, and reject delayed commands whose match or turn no longer matches. Clients SHALL preserve command identity across retransmission. A genuinely new user action SHALL use a new command identity.

#### Scenario: Lost recruitment response
- **WHEN** recruitment succeeds but the response is lost and the player retries the same request after resuming
- **THEN** exactly one soldier exists from that request and its food cost was deducted once

#### Scenario: Delayed ready request
- **WHEN** a ready request from a resolved turn arrives during a later turn
- **THEN** it does not mark the player ready or trigger production for the later turn

### Requirement: Whole-match pause and resume
Any connected roster member SHALL be able to request pause or resume during a building phase or battle. Pause SHALL freeze production, phase advancement, combat movement, attack timing, health changes, and outcomes; economic and ready actions SHALL be rejected while paused. Connection handling, resynchronization, and pause/resume requests SHALL remain active. Resume SHALL continue from the frozen state without consuming accumulated wall-clock time. Repeated pause/resume requests SHALL be harmless. A disconnect SHALL NOT itself pause or resume the match.

#### Scenario: Teammate pauses an ongoing fight
- **WHEN** a player disconnects and a remaining member pauses during combat
- **THEN** soldiers, enemies, defender attacks, and city health remain frozen until a member resumes
- **AND** the disconnected player can reconnect during the pause

#### Scenario: Resume after a long pause
- **WHEN** a paused match is resumed after a long wall-clock delay
- **THEN** combat continues with the remaining attack timers and positions rather than applying catch-up damage
