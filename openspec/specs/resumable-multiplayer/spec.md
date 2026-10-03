# resumable-multiplayer Specification

## Purpose

Keep one authoritative cooperative match consistent across validated client commands, disconnections, reconnects, and pauses without losing city ownership.

## Requirements

### Requirement: Authoritative ownership and command validation
One authority SHALL own match phase, resources, buildings, armies, city health, enemy state, readiness, pause state, and outcomes. It SHALL run inside the graphical application for single player and a playing host, or inside a dedicated server for explicit server roles. Presentation and guest clients SHALL submit requests rather than authoritative changes. The authority SHALL derive ownership from an authenticated connection or explicitly bound local player, validate match/turn context, command values, costs, phase, and permitted actions, and return success or failure feedback. Host-local and solo actions SHALL pass through the same validation and command retry protection as guest actions. Incompatible protocols and malformed or excessive remote commands SHALL fail without disrupting other players. A player interface SHALL NOT modify another city or decide combat damage, production, elimination, or outcomes.

#### Scenario: Attempt to modify another city
- **WHEN** a player submits construction, upgrade, or recruitment targeting another player's city
- **THEN** the server refuses and both cities retain their existing state

#### Scenario: Stale or invalid request
- **WHEN** a client submits an action for an earlier turn, an unknown action, invalid parameters, or an incompatible protocol
- **THEN** it receives a clear failure and no authoritative game state is changed by that request

#### Scenario: Host action uses ordinary validation
- **WHEN** the hosting player attempts an unaffordable purchase or a change to a guest city
- **THEN** the authority rejects it without changing resources or buildings
- **AND** a valid host purchase produces the same acknowledgment and shared state behavior as a valid guest purchase

#### Scenario: Solo command retry
- **WHEN** the local player resubmits the same accepted recruitment request
- **THEN** the authority reports its prior result without deducting food or creating a soldier again

### Requirement: Stable identity and private resume credentials
Each player SHALL have an identity independent of the transport connection. The authority SHALL issue a private unguessable credential that can reclaim that identity in the same running match. Credentials SHALL NOT appear in shared snapshots, lobby metadata, or diagnostic logs. Only one active connection or bound local session SHALL control a player at a time. Invalid credentials SHALL NOT reclaim a city or silently create a replacement city after match start. Resume credentials SHALL be retained locally so restarting a guest process can reclaim its city; local development clients SHALL be able to use separate session storage. Steam resumes SHALL require the authenticated Steam identity associated with the credential; an identity claimed inside a client message SHALL NOT establish ownership. Resume storage SHALL distinguish transport, host endpoint, application, and match so a local or previous hosted session cannot silently replace another.

#### Scenario: Resume with a new connection
- **WHEN** a disconnected player reconnects with valid credentials through a different transport connection
- **THEN** they regain their original player identity and city without a new starting grant or enemy allocation

#### Scenario: Invalid or concurrent claim
- **WHEN** a client presents invalid credentials or credentials for a player still controlled by an active connection
- **THEN** the claim fails clearly and the legitimate owner's state and connection remain intact

#### Scenario: Reuse a credential against a new host session
- **WHEN** a guest presents a credential from an ended session to a newly created match at the same host
- **THEN** restoration is refused clearly without creating a replacement city or consuming a roster slot

### Requirement: Disconnected state retention
A disconnected guest player's city, resources, buildings, soldiers, health, elimination status, and original wave allocation SHALL remain in the authority's match state while that authority remains running. The city SHALL continue normal production and combat whenever the match advances unpaused. A guest disconnect SHALL clear that player's readiness without eliminating the city, transferring its enemies, or deleting its army. The original playing host leaving or exiting SHALL end its session; loss of the host SHALL disable guest gameplay and expose clear bounded failure feedback and Return to menu without continuing the simulation on a guest. Guest transport loss SHALL permit reconnect attempts to the same running original host. Server/host restart recovery, cross-host resumes, and host migration SHALL NOT be promised; an unavailable or replaced authority SHALL produce clear feedback rather than an apparent restoration.

#### Scenario: Disconnect during combat
- **WHEN** a guest player loses their connection while the match is unpaused
- **THEN** their army and defender continue fighting and their city remains a target
- **AND** enemies are redistributed only if the city's health reaches zero

#### Scenario: Resume after elimination
- **WHEN** a disconnected player's city falls and the player later reconnects
- **THEN** they receive the eliminated city and current match as an observer without reviving it

#### Scenario: Host exits during combat
- **WHEN** the original playing host exits during an active wave
- **THEN** guests stop accepting gameplay input and receive session-ended or connection-loss feedback with a return path
- **AND** no guest continues combat as a new authority

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
