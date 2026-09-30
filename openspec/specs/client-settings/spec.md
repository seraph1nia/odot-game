# client-settings Specification

## Purpose

Let desktop players control their local display and master audio through an accessible settings menu, retaining preferences between launches without affecting the shared match.

## Requirements

### Requirement: Settings access and categories
Graphical clients SHALL provide a Settings button anchored in the top-left corner. Pressing Esc while settings are closed or activating that button SHALL open the same in-game menu with Graphics and Audio tabs. Esc while settings are open and a visible Close control SHALL close the menu; an open dropdown SHALL consume Esc to dismiss itself first. Holding Esc SHALL NOT repeatedly toggle it. The menu SHALL remain available while connecting, in the lobby, during gameplay, while paused, after an outcome, and while disconnected.

#### Scenario: Open and close with keyboard
- **WHEN** the player presses Esc from the game, releases it, and presses it again with settings open
- **THEN** the first press opens settings and the second closes it
- **AND** holding either press does not cause repeated toggles

#### Scenario: Use settings without a server connection
- **WHEN** a disconnected graphical player clicks the top-left Settings button
- **THEN** the same Graphics and Audio controls remain available and usable

### Requirement: Local modal interaction
While settings are open, underlying gameplay controls and world selection SHALL NOT receive player input. Closing settings SHALL restore game input without changing the previously selected plot solely because the menu was used. Opening, closing, and editing settings SHALL NOT request pause/resume, alter readiness, or change authoritative match state. Network synchronization and presentation updates SHALL continue while the menu is open.

#### Scenario: Click settings over a building
- **WHEN** the player opens settings and clicks a menu control with a building or gameplay control underneath it
- **THEN** only the settings control handles the input and no world selection, spending, or match action occurs

#### Scenario: Configure display during combat
- **WHEN** one player changes settings while an unpaused multiplayer battle is running
- **THEN** the battle and synchronization continue for both players and the other client's live preferences remain unchanged

### Requirement: Windowed resolution and fullscreen mode
Graphics SHALL offer Windowed and Fullscreen modes, with changes applied immediately. Windowed mode SHALL offer the existing 1100x820 size and common presets including 1280x720, 1600x900, and 1920x1080 when they fit the current display's available area. Fullscreen SHALL use the current monitor's native size without changing its display mode. The windowed resolution selector SHALL be visibly inactive in Fullscreen. Returning to Windowed SHALL restore the selected windowed size. Restored preferences that no longer fit the display SHALL fall back to a usable windowed size. The menu and essential game controls SHALL remain accessible at the offered sizes, including after a manual window resize.

#### Scenario: Choose a windowed size
- **WHEN** the player selects an available 1280x720 preset in Windowed mode
- **THEN** the client window changes to that size and the settings controls and essential game controls remain reachable

#### Scenario: Return from fullscreen
- **WHEN** the player enters Fullscreen from a selected windowed size and later returns to Windowed
- **THEN** Fullscreen fills the current monitor at its native size
- **AND** Windowed restores the selected size if it still fits

#### Scenario: Start on a smaller display
- **WHEN** a saved windowed size cannot fit the current display's available area
- **THEN** the client starts with a usable size and shows controls consistent with the effective display settings

### Requirement: Live master volume
Audio SHALL expose a Master volume slider with integer values from 0 to 100 and a visible numeric value. Slider changes SHALL apply immediately to all game audio in the current client. Zero SHALL silence all game audio; raising the slider SHALL restore audio without restarting the music. The first-launch Master value SHALL be 50, with the background track mixed at a restrained level.

#### Scenario: Mute and restore music
- **WHEN** a player moves Master volume to 0 and later raises it to 50
- **THEN** audio is silent at 0 and becomes audible at the new level immediately
- **AND** the music continues from its ongoing playback position rather than restarting

### Requirement: Local preference persistence and recovery
The client SHALL persist Master volume, display mode, and the selected windowed size in local user storage and restore valid values on subsequent graphical launches. Preferences SHALL be independent of multiplayer resume credentials and remain intact through reconnects. Missing, unreadable, or invalid saved preferences SHALL use valid defaults without preventing startup. A persistence failure SHALL leave live settings usable and surface a concise save-error message in the settings menu. Already-running clients SHALL NOT change their live settings when another process saves preferences.

#### Scenario: Restart after changing settings
- **WHEN** the player changes volume and display settings, closes the client, and launches it again without conflicting explicit display arguments
- **THEN** valid saved preferences are restored and the music starts at the restored Master level

#### Scenario: Recover from invalid settings
- **WHEN** preferences contain an invalid mode, malformed resolution, or out-of-range volume
- **THEN** the client uses valid defaults for invalid values and starts successfully

#### Scenario: Unable to save preferences
- **WHEN** the local preferences file cannot be written
- **THEN** the chosen values continue working for the current client and the menu explains that saving failed

### Requirement: Headless settings isolation
Dedicated servers and headless clients SHALL NOT create the settings menu or apply, save, or depend on graphical/audio preferences. Introducing settings SHALL preserve their existing startup, network verification, and stripped export behavior.

#### Scenario: Run with unavailable graphical preferences
- **WHEN** headless multiplayer roles run without a display, audio device, or valid client preferences file
- **THEN** they operate normally and do not change that file
