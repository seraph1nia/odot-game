# Spec Delta

## Purpose

Connect friends to a player-authoritative cooperative session through Steam's lobby, invitation, identity, and relay services with supported stable dependencies.

## ADDED Requirements

### Requirement: Private hosted Steam lobby
Hosting SHALL create one private invitation-only Steam lobby for up to four players including the host. The lobby SHALL identify the original hosting player, show synchronized admitted players and connection status, and offer Invite friends through Steam's standard overlay. Only the original host SHALL start a hosted match. Lobby metadata SHALL describe compatibility and lobby/match availability without exposing resume secrets or authoritative gameplay state.

#### Scenario: Host and invite a friend
- **WHEN** a Steam-connected player hosts a game and activates Invite friends
- **THEN** one private lobby is created and Steam's standard invitation interface targets that lobby
- **AND** an admitted friend appears alongside the host before the host starts the match

#### Scenario: Guest attempts to start
- **WHEN** a hosted guest requests match start
- **THEN** the request is refused without changing phase or allocating a wave

### Requirement: Invitation routing
The application SHALL handle accepted Steam lobby invitations both while running and when Steam launches it for an invitation. It SHALL validate the target lobby and host, resolve compatibility, and synchronize the match before enabling gameplay. Accepting an invitation at the start screen SHALL enter a joining flow directly. An invitation received during another session SHALL require an explicit accept/decline decision before leaving that session. Duplicate notifications SHALL NOT create multiple joins, and refusal or cancellation SHALL leave a usable screen.

#### Scenario: Launch from an invitation
- **WHEN** Steam launches the application with an accepted lobby invitation
- **THEN** the application routes to that lobby's host and shows connection progress rather than rejecting the launch arguments or connecting to loopback

#### Scenario: Invitation during solo play
- **WHEN** an invitation arrives while a solo match is active and the player declines the switch
- **THEN** the current solo session remains active and no foreign connection or city is created

### Requirement: Relay-backed gameplay connection
Steam multiplayer SHALL carry validated commands, acknowledgments, and complete snapshots between guests and the original host using Steam's supported authenticated networking and relay service. Players SHALL NOT need manual port forwarding or exchange IP addresses. Delivery SHALL preserve command retry protection, state revision ordering, and complete resynchronization; a connection or relay failure SHALL report an actionable error without inventing state or switching authority.

#### Scenario: Play across unrelated home networks
- **WHEN** two compatible exported clients using distinct Steam accounts connect by invitation on different NAT-protected networks without router changes
- **THEN** the guest joins the playing host and both observe accepted gameplay changes and combat
- **AND** the verification record identifies actual Steam connectivity rather than an ENet substitute

### Requirement: Authenticated admission and reconnect
The host SHALL associate each Steam player with the authenticated transport identity rather than trusting a claimed identity in a request. New players SHALL be admitted only while the hosted lobby has available capacity before match start. A disconnected roster member SHALL be able to reconnect to the same running host after start, including during pause or after elimination, using its private resume credential and the same authenticated Steam identity. Membership changes SHALL NOT create extra cities or prevent valid roster reconnects solely because fresh joins are closed.

#### Scenario: Resume after the roster locks
- **WHEN** a disconnected roster member accepts the running session's invitation and reconnects with its valid identity and credential
- **THEN** it receives its retained city and complete current match rather than a new city or fresh-join refusal

#### Scenario: Claim another player's credential
- **WHEN** a Steam peer authenticated as a different user presents another player's resume credential
- **THEN** the host refuses the claim and leaves the original player's city unchanged

### Requirement: Original host lifetime
The original hosting player SHALL remain the sole gameplay authority for the lifetime of its hosted session. Its orderly exit SHALL end the session and return guests to the start screen with an explanation. Loss of that host SHALL disable guest gameplay and provide bounded failure/connection feedback with a return path; the match SHALL NOT continue from guest state. Steam lobby ownership changes SHALL NOT trigger gameplay host migration or revive an ended session. A temporary guest transport failure SHALL retain the existing guest reconnect behavior while the original host remains available.

#### Scenario: Steam transfers lobby ownership
- **WHEN** the original host leaves and Steam assigns another lobby owner
- **THEN** remaining guests report that the hosted game ended and do not start an authoritative simulation

### Requirement: Supported stable integration dependencies
The selected production Steam integration SHALL use pinned non-prerelease dependencies with available source, documented licensing, and verified ongoing upstream maintenance. Beta bindings and paused or abandoned transport adapters SHALL NOT be required to develop or run the game. The selected integration SHALL work with the project's supported stock engine/.NET build and exported Linux client. Its native redistribution requirements SHALL be documented and reproducible without runtime downloads; multiplayer acceptance SHALL remain incomplete if this compatibility has not been demonstrated.

#### Scenario: Reject an unsupported integration candidate
- **WHEN** a proposed dependency is beta-only, paused, abandoned, or cannot run in the supported exported client
- **THEN** it is excluded from the production integration and an eligible stable SDK path is evaluated
- **AND** a failed or skipped experiment is not recorded as completed Steam support
