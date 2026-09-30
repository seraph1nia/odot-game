# client-updates Specification

## Purpose
Let players see their installed version, manually discover compatible newer GitHub releases, and download a full installer without interrupting gameplay.

## Requirements

### Requirement: Shared version and update controls
The shared graphical settings menu SHALL expose an About section showing the full installed version and a Check for updates control from the start screen and in-game. Update controls SHALL obey existing modal input isolation, preserve preferences and music lifetime, and SHALL NOT pause, leave, or modify a match. Headless roles SHALL NOT perform update requests or create these controls. Development builds without release identity SHALL explain that they are development builds rather than compare a fabricated release version.

#### Scenario: Check from an active match
- **WHEN** a player opens settings during a match and activates Check for updates
- **THEN** the check runs without blocking synchronization, changing readiness, or automatically leaving the match
- **AND** underlying game input remains blocked by the settings dialog

### Requirement: Manual bounded release discovery
Update discovery SHALL occur only on explicit player request, use public GitHub release information without credentials, and provide checking, up-to-date, update-available, or could-not-check feedback. Repeated clicks SHALL NOT create competing requests. Requests SHALL have bounded time and response size, and late results SHALL NOT update a disposed application. Offline access, timeouts, rate limits, malformed responses, or missing platform assets SHALL leave the game usable and SHALL NOT be presented as an up-to-date result.

#### Scenario: GitHub is unavailable
- **WHEN** an explicit update check encounters an offline connection, rate limit, or timeout
- **THEN** settings shows a recoverable check failure and the installed game remains usable

#### Scenario: No releases exist yet
- **WHEN** the public repository contains no eligible packaged release
- **THEN** the menu reports that no release is available without inventing an update

### Requirement: Semantic version and channel selection
Candidates SHALL have a valid supported SemVer tag and matching complete platform deliverables, and SHALL be selected by semantic precedence rather than lexical order or publication date. Build metadata SHALL NOT affect version precedence. Stable installations SHALL consider only published non-draft stable releases. Preview installations SHALL consider both previews and stable releases so a stable successor can replace a preview. Only a candidate with strictly greater precedence than the installed version SHALL be offered; no automatic downgrade or stable-to-preview switch SHALL occur.

#### Scenario: Numeric version comparison
- **WHEN** version `0.9.0` checks a feed containing `0.10.0` and `0.8.0`
- **THEN** it offers `0.10.0`

#### Scenario: Stable build sees a newer preview
- **WHEN** a stable installation checks a feed whose only newer version is a prerelease
- **THEN** that preview is not offered

#### Scenario: Preview becomes stable
- **WHEN** `0.1.0-beta.1` checks a feed containing packaged stable version `0.1.0`
- **THEN** it offers `0.1.0`

### Requirement: Browser-assisted full downloads
An available update SHALL show its version and a Download update action for the current platform. Activating it SHALL open the selected release's version-specific Windows installer download or Linux release page containing its archive and install script. Players SHALL be told to close the game and run the installer or script after downloading. The client SHALL NOT execute downloaded code, replace its own files, automatically exit, or install during gameplay. URLs SHALL remain within the configured GitHub repository's HTTPS release/download routes, and an external-open failure SHALL provide recoverable feedback.

#### Scenario: Download on Windows
- **WHEN** a Windows player activates Download update for a selected release
- **THEN** the default browser opens that version's Windows installer URL and the running game remains active

#### Scenario: Download on Linux
- **WHEN** a Linux player activates Download update for a selected release
- **THEN** the default browser opens that version's release page with its Linux install script and full archive

#### Scenario: Unexpected external URL
- **WHEN** release information supplies an unsupported or foreign download URL
- **THEN** the client reports a check failure instead of opening that URL
