# desktop-installation Specification

## Purpose
Let friends install, upgrade, launch, and remove the game on Windows and Linux without development tooling or administrator privileges.

## Requirements

### Requirement: Windows per-user installation
The Windows installer SHALL install the complete client under a stable per-user application location without requesting administrator privileges, create a Start Menu shortcut, and register an uninstaller. Subsequent versions SHALL target the same installation identity and location. The installer SHALL explain that the running game must close before files are replaced and SHALL NOT terminate unrelated applications. Installation SHALL NOT require Steam login, and the installed start screen and solo SHALL work without Steam.

#### Scenario: First installation
- **WHEN** a standard Windows user runs the installer
- **THEN** the client, application shortcut, and uninstaller are installed without elevation
- **AND** the shortcut opens the ordinary game start screen

#### Scenario: Install while the game is running
- **WHEN** an upgrade cannot replace files because the game is running
- **THEN** installation requests closure or reports the condition rather than reporting a successful upgrade with mixed files

### Requirement: Linux per-user installation
The Linux install script SHALL install a version-specific complete client under a stable per-user application location and create an application-menu entry and launcher. It SHALL support reinstalling or upgrading without `sudo`, installing OS packages, or changing Steam settings. It SHALL validate architecture and required installation tools, verify the downloaded archive against its version-specific checksum, and complete staging before switching the launcher to the new version. An interrupted or invalid download SHALL leave a previous installation usable. Its launcher SHALL prepare the supported native Steam overlay before graphics initialization when available, preserve inherited launch environment and arguments, and still launch without Steam.

#### Scenario: Install a selected release
- **WHEN** a Linux x86_64 user runs the downloaded version-specific install script with its required tools available
- **THEN** it installs that exact release and its application-menu entry launches the normal game

#### Scenario: Download fails during upgrade
- **WHEN** the requested archive is unavailable, truncated, has the wrong checksum, or installation is interrupted before activation
- **THEN** installation returns a failure and the existing launcher still targets the previous complete installation

#### Scenario: Steam overlay is unavailable
- **WHEN** an installed Linux client starts without a usable native Steam overlay
- **THEN** the game still opens and offers solo and existing recoverable multiplayer feedback

### Requirement: Full-package replacement and player data preservation
Upgrades SHALL replace the application's managed payload with the selected full release, remove obsolete managed payload files, and preserve preferences and private session files outside that payload. Uninstalling SHALL remove owned application files and shortcuts while preserving player data by default. Release versions SHALL NOT change the application's user-data identity or imply that an ended multiplayer authority can be resumed. Installation and removal SHALL affect only their owned paths.

#### Scenario: Upgrade after changing settings
- **WHEN** a player installs a newer version after saving local settings and a private session credential
- **THEN** the new client retains those files, reports the new version, and has no obsolete managed dependency files from the earlier payload

#### Scenario: Remove and reinstall
- **WHEN** the player uninstalls and later reinstalls the game
- **THEN** owned application files and shortcuts are removed and recreated while the retained player data remains available
