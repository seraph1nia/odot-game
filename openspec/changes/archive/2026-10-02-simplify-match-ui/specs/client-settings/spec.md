# Spec Delta

## MODIFIED Requirements

### Requirement: Local modal interaction
While settings are open, underlying gameplay controls and world selection SHALL NOT receive player input. Closing settings SHALL restore game input without changing the previously selected plot solely because the menu was used. Opening, closing, and editing settings SHALL NOT request pause/resume, alter readiness, or change authoritative match state. Network synchronization and presentation updates SHALL continue while the menu is open. The Return to menu action SHALL open confirmation without leaving; only an explicit confirm SHALL invoke session leave. Canceling or dismissing confirmation SHALL keep Settings open and restore focus to its return action. The confirmation SHALL participate in the same modal protection as Settings and prevent input from reaching underlying Settings, HUD or world controls.

#### Scenario: Click settings over a building
- **WHEN** the player opens settings and clicks a menu control with a building or gameplay control underneath it
- **THEN** only the settings control handles the input and no world selection, spending, or match action occurs

#### Scenario: Configure display during combat
- **WHEN** one player changes settings while an unpaused multiplayer battle is running
- **THEN** the battle and synchronization continue for both players and the other client's live preferences remain unchanged

## ADDED Requirements

### Requirement: Session return confirmation
Settings SHALL expose a text-only Return to menu action while a graphical session exists, including lobby, connecting, gameplay, pause, outcome and disconnected states. Start-screen and multiplayer-entry Settings SHALL omit it. Activating it SHALL show a modal with Cancel and Return to menu buttons and clear consequences: solo play loses unsaved game progress, a host ends the hosted session for everyone, and a guest leaves while its city remains at the running host. Cancel SHALL be the default focus; Escape and closing the confirmation SHALL cancel. Opening or canceling SHALL NOT pause, change readiness, spend resources or dispose the session. Confirmation SHALL preserve existing local preference and music lifetimes and tolerate session end while the dialog is open.

#### Scenario: Cancel leaving solo play
- **WHEN** the player chooses Return to menu from session Settings and cancels, presses Escape or closes its confirmation
- **THEN** the same session and selected plot remain, Settings stays open with valid focus and no leave action occurs

#### Scenario: Confirm leaving
- **WHEN** the player explicitly confirms Return to menu
- **THEN** the existing session leave behavior runs exactly once, Settings and confirmation close, and focus moves to an available start-screen control

#### Scenario: Open Settings outside a session
- **WHEN** the player opens Settings from the start screen or multiplayer entry
- **THEN** display/audio/about controls work and no Return to menu action is shown
