# steam-sessions Specification

## Purpose

Connect friends to a player-authoritative cooperative session through GodotSteam GDExtension's native lobby, invitation, identity, and multiplayer peer APIs, with maintained pinned dependencies and AppID 480 development support.

## Requirements

### Requirement: Private hosted Steam lobby
Hosting SHALL create one private invitation-only Steam lobby for up to four players including the host. The lobby SHALL identify the original hosting player, show synchronized admitted players and connection status, and offer Invite friends through an in-game Steam friends picker. Only the original host SHALL start a hosted match. Lobby metadata SHALL describe compatibility and lobby/match availability without exposing resume secrets or authoritative gameplay state.

#### Scenario: Host and invite a friend
- **WHEN** a Steam-connected player hosts a game and activates Invite friends
- **THEN** one private lobby exists and the application opens its own Steam friends picker targeting that lobby
- **AND** selecting a friend's Invite action requests a direct Steam lobby invitation without requiring the overlay
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
Steam integration SHALL use the official GodotSteam GDExtension and its native `SteamMultiplayerPeer`, with pinned upstream non-prerelease dependencies, available source, documented licensing, and verified ongoing maintenance. Small C# interop helpers or minimal GDScript glue MAY manage initialization, callbacks, lobbies, native peer setup, identity lookup, and shutdown. Gameplay delivery SHALL use Godot's multiplayer API and RPCs through that peer. The project SHALL NOT implement its own Steam-to-Godot transport wrapper, require beta C# bindings or paused/abandoned peer adapters, or automatically switch to a different SDK. The extension SHALL work with the supported stock engine/.NET build and ordinary Linux export templates; its native redistribution SHALL be documented and reproducible without runtime downloads. Development MAY use the assessed extension after recording its publisher's unstable designation, preliminary compatibility evidence, and unresolved lifecycle checks. That development selection SHALL NOT count as resolved release stability or complete multiplayer acceptance; failed required checks SHALL remain incomplete. Closing a locally verified development milestone with explicitly deferred external checks SHALL NOT change their unverified status or establish release qualification.

#### Scenario: Reject an unsupported integration candidate
- **WHEN** a proposed dependency is beta-only, paused, abandoned, or cannot run in the supported exported client
- **THEN** it is excluded from the required integration and the unmet extension gate is reported without silently selecting another SDK
- **AND** a failed or skipped experiment is not recorded as completed Steam support

#### Scenario: Use the native peer from C#
- **WHEN** a supported development or exported Linux build hosts through GodotSteam
- **THEN** its small helpers initialize the extension and assign its native peer to Godot multiplayer after successful host creation
- **AND** ordinary RPC commands and snapshots use that peer without a project-owned Steam transport implementation

### Requirement: Development AppID and release separation
Steam-enabled development runs and development exports SHALL default to AppID 480 and SHALL permit an explicit configured AppID override. Initial development SHALL NOT require owning a production AppID. Local solo/ENet/headless paths SHALL remain usable without Steam initialization. Lobby validation SHALL check this game's identifier and protocol as well as host/session metadata so unrelated projects sharing AppID 480 are refused. Production packaging SHALL require the game's own configured AppID, SHALL NOT silently default to 480, and SHALL exclude development-only AppID files/settings. Explicit development launches with invitation arguments SHALL be distinguished from evidence that Steam itself cold-launched this game's executable.

#### Scenario: Develop with the default AppID
- **WHEN** two Steam-connected developers launch compatible development builds without an AppID override
- **THEN** the extension initializes with AppID 480 and hosting/invitation joining use the game's identified private lobby
- **AND** a lobby for another development game sharing AppID 480 is refused before gameplay admission

#### Scenario: Prepare a production package
- **WHEN** production packaging has no configured game AppID or still uses the development default
- **THEN** it fails with an actionable configuration error rather than publishing a package identified as AppID 480

#### Scenario: Exercise development invitation launch arguments
- **WHEN** a development process is explicitly launched with a valid invitation's `+connect_lobby` arguments
- **THEN** it exercises normal invitation routing
- **AND** this result is recorded as launch-argument coverage rather than a real Steam cold-launch invitation pass

### Requirement: Steam friends picker
The original host SHALL be able to browse Steam friends by display name and presence, refresh the list, select an individual friend to invite, and close the picker with Close or Escape. Selection SHALL distinguish friends with identical display names. The picker SHALL remain usable within supported window sizes and with keyboard navigation, SHALL block gameplay input while open, and SHALL restore appropriate focus when closed. An empty list or unavailable Steam connection SHALL show an explanation and a usable close or retry path. Guests and solo players SHALL NOT be offered the host's invitation picker.

#### Scenario: Refresh friends and select a recipient
- **WHEN** the host opens the picker and refreshes the Steam friends list
- **THEN** current friends and their presence are displayed with individual Invite actions
- **AND** inviting one of two identically named friends targets only the selected account

#### Scenario: No friends or Steam unavailable
- **WHEN** the friends list is empty or Steam becomes unavailable
- **THEN** the picker explains the condition without showing a successful invitation
- **AND** the player can close it and unavailable invitations cannot be sent

#### Scenario: Close without sending
- **WHEN** the host closes the picker with Close or Escape without selecting Invite
- **THEN** no invitation is sent and focus returns to the hosted game
- **AND** input handled by the picker does not trigger gameplay actions

### Requirement: Direct lobby invitation outcomes and lifetime
An Invite action SHALL request a Steam lobby invitation for the selected current friend and current original host's lobby. Before sending, the application SHALL revalidate its Steam login, host session, lobby, and selected friendship. Steam accepting the send request SHALL be reported as an invitation sent, not as recipient acceptance, connection, or admission. A rejected send request SHALL show a retryable failure without falsely reporting success or ending the match. Repeated activation while a send is being processed SHALL NOT send duplicate invitations. The picker and pending selections SHALL be invalidated when the host session ends or changes. Invitations SHALL NOT bypass compatibility validation, authenticated admission, roster capacity, or reconnect requirements.

#### Scenario: Steam accepts or rejects the send request
- **WHEN** the host explicitly invites a friend and Steam returns a send result
- **THEN** the picker reports sent only for an accepted request and reports failure otherwise
- **AND** the roster changes only after actual authenticated admission

#### Scenario: Stale selection after a session change
- **WHEN** the original session ends or is replaced while the picker or a selection is active
- **THEN** the picker closes and that selection cannot send to either the old lobby or its replacement

#### Scenario: Friend accepts while both development clients are running
- **WHEN** two distinct Steam accounts run compatible AppID 480 development clients and the guest accepts a direct lobby invitation
- **THEN** existing invitation routing connects the guest to the original host with its own city
- **AND** Steam overlay availability is not required to send the invitation

#### Scenario: Invitation does not override roster policy
- **WHEN** an invited friend attempts to join a full lobby or a match with fresh admission closed
- **THEN** normal admission policy refuses a new player with explanatory feedback
- **AND** an existing roster member remains eligible to resume using its authenticated identity and valid credential
