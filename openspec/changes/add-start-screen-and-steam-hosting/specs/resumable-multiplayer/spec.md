# Spec Delta

## MODIFIED Requirements

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
