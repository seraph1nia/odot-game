# Spec Delta

## Purpose

Give the cooperative settlement-defense game a clear public identity as The Common Watch while preserving existing player data and technical compatibility.

## ADDED Requirements

### Requirement: Public game title
The graphical application SHALL present **The Common Watch** as its window and start/multiplayer menu title. Friend instructions, update messages, installer product/launch text and the Linux application-menu display name SHALL use the same public name. Legacy technical names SHALL remain only where compatibility, provenance or documented legacy launch aliases require them.

#### Scenario: Open the source or exported graphical client
- **WHEN** a player starts the ordinary client, enters multiplayer, or launches an explicit graphical development role
- **THEN** the application window is titled The Common Watch
- **AND** the start and multiplayer screens present the same public title when shown

#### Scenario: Install and discover the application
- **WHEN** a player installs a package containing the rebrand
- **THEN** installer product/launch text and the Linux desktop-entry display name identify The Common Watch
- **AND** stable command, filename and Windows shortcut/group aliases retain their documented legacy names

### Requirement: Cooperative settlement-defense positioning
The introductory menu and current player documentation SHALL describe building and provisioning villages for shared defense against automatic waves. The copy SHALL communicate cooperative play without implying competition, direct shooter combat, MMO scale, first-person controls or new mechanics. Solo SHALL remain presented as supported play. Existing countryside and UI assets SHALL remain in use without a new art or lore dependency.

#### Scenario: Learn the game's premise
- **WHEN** a player reads the start-screen positioning or current gameplay overview
- **THEN** the copy identifies village planning and defense together as the premise
- **AND** the documentation explains individual city ownership, automatic combat and shared victory with solo support

### Requirement: Rebrand preserves technical and player-data identity
The rebrand SHALL preserve existing preferences and private credentials in their existing locations without copying, deleting or migrating them. It SHALL preserve protocol, Steam, repository/update, package and installation identities, executable/launcher names, installer upgrade identity and install locations. The public title SHALL NOT imply save recovery, host migration or recovery against a replaced authority.

#### Scenario: Run with existing preferences and credentials
- **WHEN** a player launches a rebranded client with preferences and private credentials from a compatible previous build
- **THEN** the client uses the same player-data location and normal persistence behavior
- **AND** credentials remain subject to the existing same-running-authority resume rules

#### Scenario: Upgrade an existing installation
- **WHEN** the rebranded package replaces a compatible installed package
- **THEN** the existing upgrade identity, owned installation location and launcher targets remain stable
- **AND** no second application identity or competing release is created by the rebrand

### Requirement: Readable branded menus
The longer public title and cooperative positioning SHALL remain visible without clipping at 1100x820 and 1280x720. Existing menu actions, keyboard focus, settings and session transitions SHALL remain accessible and retain their existing behavior.

#### Scenario: Use both reference layouts
- **WHEN** the player views the start and multiplayer menus at either reference size and navigates into solo and back
- **THEN** the branded title and positioning fit their menu surface and the application window retains the public title
- **AND** existing navigation and session actions remain usable
