# game-launcher Specification

## Purpose

Let desktop players enter solo or hosted cooperative sessions from one start screen, reuse local presentation preferences, and leave or exit cleanly.

## Requirements

### Requirement: Start screen and explicit session launch
A normal graphical launch SHALL show Single player, Multiplayer, Settings, and Exit Game in the same application window, with a static medieval background drawn from the bundled asset palette. Showing the start screen SHALL NOT create a match or attempt a gameplay connection. Native pointer and keyboard navigation SHALL keep these controls accessible at 1100x820 and 1280x720 and after supported settings changes. Explicit server/client development launches SHALL retain their direct role behavior and headless roles SHALL NOT create the start screen.

#### Scenario: Launch without a server or Steam
- **WHEN** the desktop application starts normally with no server running and Steam unavailable
- **THEN** the four start-screen actions are visible and usable without a connection timeout or application failure
- **AND** no city or running match exists yet

#### Scenario: Launch an explicit development client
- **WHEN** a contributor supplies the existing explicit client role and endpoint arguments
- **THEN** the application attempts that connection directly instead of waiting for a start-screen selection

### Requirement: Self-contained single player
Selecting Single player SHALL create and start a fresh match with exactly one local player inside the graphical application, using the same economy, nine slots, recruitment, upgrades, ready cadence, pause, combat, and outcome rules as cooperative play. It SHALL NOT require a socket, external server process, Steam account, or internet connection. Gameplay SHALL use validated requests and current authoritative snapshots. Additional players SHALL NOT join that session.

#### Scenario: Start offline solo play
- **WHEN** a player activates Single player while offline
- **THEN** the first building turn opens with one city, nine empty plots, and the configured starting resources
- **AND** build, upgrade, recruit, ready, and pause/resume follow the normal match rules

#### Scenario: Start another solo match
- **WHEN** a player returns to the start screen and selects Single player again
- **THEN** a new match identity and fresh starting state are created without restoring the previous match or selection

### Requirement: Multiplayer entry and availability feedback
Multiplayer SHALL offer Host game and Back and explain that guests join through Steam invitations. Hosting SHALL open the hosted lobby only after creation succeeds. Unavailable Steam integration, failed initialization, unavailable account/app access, and connection failures SHALL produce clear recoverable feedback without disabling Single player, Settings, Back, or Exit Game. Choosing Steam multiplayer SHALL NOT silently create a local-only session or ask players to configure router ports.

The start screen and multiplayer entry SHALL display the signed-in player's Steam persona name at the bottom, or "Open Steam to log in" when Steam is unavailable or disconnected. Host game SHALL recheck current Steam login/online availability before requesting a lobby, including after earlier successful initialization. An unavailable login SHALL show a warning to open Steam and log in and SHALL NOT create a gameplay session.

#### Scenario: Steam is unavailable
- **WHEN** a player opens Multiplayer without usable Steam access
- **THEN** the application explains why hosting is unavailable and lets them return to the start screen
- **AND** it does not present a failed or local-only session as an online lobby

#### Scenario: Host without a Steam login
- **WHEN** the player selects Host game without a current Steam login/connection
- **THEN** the bottom status says "Open Steam to log in" and hosting shows an actionable warning to open Steam and log in
- **AND** no online lobby or local replacement session is created

#### Scenario: Show the current Steam player
- **WHEN** Steam is initialized and connected with a signed-in player
- **THEN** the start screen and multiplayer entry show the player's Steam persona name at the bottom
- **AND** periodic availability refresh and each hosting attempt use the extension's current login state rather than a cached initialization result

### Requirement: Shared local settings and music lifetime
The start-screen Settings action and the existing in-game Settings entry point SHALL open the same local Graphics and Audio menu and use the same persisted preferences. Settings and one music instance SHALL remain alive across the start screen, multiplayer entry, lobby, match, reconnect, and return to the start screen. Preferences SHALL be applied before music starts. Opening or editing settings SHALL block underlying input without changing authoritative readiness or pause state, and closing settings SHALL restore focus to an available control in the current screen. Existing authored-loop and preference semantics SHALL be preserved.

#### Scenario: Carry preferences into a match
- **WHEN** a player changes volume and window settings from the start screen and starts a match
- **THEN** the match uses those settings and the same music continues without a duplicate player or restarted intro

#### Scenario: Return while settings are open
- **WHEN** a session ends while its settings dialog is open
- **THEN** the dialog remains usable or closes to a valid start-screen control without referencing a destroyed game view
- **AND** no setting change pauses or resumes a shared match

### Requirement: Return to menu and fresh session isolation
Graphical lobby, match, outcome, and disconnected views SHALL provide Return to menu. Returning SHALL stop local gameplay presentation, release the current session's connections and lobby membership, and clear transient input, selection, and pending requests while preserving local preferences and valid private guest resume credentials. A host leaving SHALL end its hosted session; a guest leaving SHALL disconnect without deleting its retained city from a still-running host. Repeated leave operations and late callbacks SHALL NOT create duplicate sessions or apply state to a subsequent session. Leaving solo play SHALL discard the unsaved local match.

#### Scenario: Leave and host again
- **WHEN** the host returns to the start screen and hosts a new game in the same process
- **THEN** exactly one new authority and lobby are active with a new match identity and fresh roster
- **AND** old callbacks, snapshots, or commands cannot change the new session

#### Scenario: Guest returns to the menu
- **WHEN** a guest uses Return to menu while its host remains running
- **THEN** the guest sees the start screen and the host retains the disconnected guest's city
- **AND** valid reconnect information remains private and available for that running match

### Requirement: Application exit
Exit Game and native window close SHALL release owned gameplay connections, hosted lobbies, platform callbacks, and presentation resources, attempt ordinary preference persistence, and terminate the graphical process. Shutdown SHALL be bounded even when disconnected or a platform operation is pending. It SHALL NOT merely return to the start screen or leave a spawned gameplay process running.

#### Scenario: Exit from the start screen
- **WHEN** a player activates Exit Game
- **THEN** the application process terminates successfully and no owned session remains

#### Scenario: Close an active host window
- **WHEN** the hosting player closes the application window during a match
- **THEN** the host process exits and guests receive session-ended feedback without migrating authority
