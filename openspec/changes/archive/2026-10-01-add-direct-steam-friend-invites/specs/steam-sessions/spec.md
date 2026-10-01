# Spec Delta

## MODIFIED Requirements

### Requirement: Private hosted Steam lobby
Hosting SHALL create one private invitation-only Steam lobby for up to four players including the host. The lobby SHALL identify the original hosting player, show synchronized admitted players and connection status, and offer Invite friends through an in-game Steam friends picker. Only the original host SHALL start a hosted match. Lobby metadata SHALL describe compatibility and lobby/match availability without exposing resume secrets or authoritative gameplay state.

#### Scenario: Host and invite a friend
- **WHEN** a Steam-connected player hosts a game and activates Invite friends
- **THEN** one private lobby exists and Odot opens its own Steam friends picker targeting that lobby
- **AND** selecting a friend's Invite action requests a direct Steam lobby invitation without requiring the overlay
- **AND** an admitted friend appears alongside the host before the host starts the match

#### Scenario: Guest attempts to start
- **WHEN** a hosted guest requests match start
- **THEN** the request is refused without changing phase or allocating a wave

## ADDED Requirements

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
