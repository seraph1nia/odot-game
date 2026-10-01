# Spec Delta

## Purpose

Give all Odot-owned graphical menus and match controls a consistent free Trio UI presentation with meaningful icons and readable, accessible interactions.

## ADDED Requirements

### Requirement: Bundled free Trio UI assets
Graphical clients SHALL use assets from the free Trio UI kit published by moonpunchstudio at https://moonpunchstudio.itch.io/trio-ui-kit-free. Selected assets SHALL be available from a clean checkout and in graphical exports without runtime asset downloads or a paid kit requirement. Source/version information, archive and selected-file hashes, and redistribution/use permission evidence SHALL accompany the bundled assets. Headless roles and stripped dedicated-server exports SHALL remain operable without loading these graphical resources.

#### Scenario: Run an offline graphical export
- **WHEN** a player launches an exported client without internet access and opens its menus and a solo match
- **THEN** kit-styled controls, mapped icons and unit health bars load from bundled resources
- **AND** no asset download or paid content is required

#### Scenario: Run a stripped server
- **WHEN** a dedicated-server export runs with graphical resources stripped
- **THEN** its normal authority and network behavior works without loading UI kit assets

### Requirement: Consistent presentation across owned menus
The start screen, multiplayer entry, cooperative lobby, in-match HUD and contextual actions, pause controls, outcome and disconnected/reconnect states, Settings Graphics/Audio/About pages and update controls, friend-invitation list, and join confirmation SHALL share one coordinated Trio-based panel, button and control style. Controls absent from the free sample SHALL use matching presentation derived from the same palette and spacing. Normal, hovered, pressed, disabled, selected and keyboard-focused states SHALL remain distinguishable where applicable. The refresh SHALL preserve each screen's existing actions, feedback, modality and session behavior. Platform-owned Steam UI and operating-system window decorations are outside this contract.

#### Scenario: Move from menus into a session
- **WHEN** a player visits the start screen, multiplayer entry, a hosted lobby and match controls
- **THEN** their panels and controls share the chosen kit styling while the existing hosting, readiness and session actions remain available under their existing eligibility rules

#### Scenario: Open owned dialogs
- **WHEN** a player opens Settings including About, the friend-invitation list, or join confirmation
- **THEN** the dialog surface, tabs, buttons, dropdowns, sliders and scroll controls have coordinated styling and visible keyboard focus
- **AND** existing modal input protection and focus restoration continue to work

#### Scenario: Display a paused or disconnected match
- **WHEN** a match pauses, ends, or loses its connection
- **THEN** the corresponding controls and messages retain the shared styling
- **AND** pause/resume, return and reconnect remain governed by current authoritative state and session mode

### Requirement: Meaningful icons with readable information
Graphical clients SHALL use bundled kit icons for actions, resources, health, army or research information only where the available icon has a clear matching meaning. The same concept SHALL use the same icon across screens and cost displays. Actions SHALL retain readable text labels and applicable explanatory tooltips; resource totals and costs SHALL retain exact numeric values and identifiable resource names. A concept without a suitable free icon SHALL retain a readable text representation instead of using a misleading icon or requiring additional paid assets. Icons SHALL NOT change hit targets, command meaning, action eligibility or authoritative values.

#### Scenario: Identify a resource and its cost
- **WHEN** the player views resource totals and an eligible construction, recruitment or research action
- **THEN** each mapped resource uses the same icon and retains its name and exact authoritative amount or cost
- **AND** unsupported resource icons fall back to explicit text

#### Scenario: Navigate an icon-enhanced action
- **WHEN** a player navigates controls using the keyboard or inspects a disabled action
- **THEN** labels, focus and disabled feedback remain understandable without inferring the action solely from an icon

### Requirement: Readable responsive menus and HUD
Kit panels and controls SHALL scale without visibly stretched borders or clipped essential labels. At 1100x820 and 1280x720 and after supported display changes, essential actions, status, costs and modal controls SHALL remain reachable. The bottom HUD SHALL continue to leave the city's nine plots, home and battle approach visible and selectable. Decorative images and information-only icons SHALL NOT intercept world or control input; modal surfaces SHALL continue to block underlying actions.

#### Scenario: Use the minimum supported layouts
- **WHEN** the player views menus, settings and contextual build/recruit/research controls at 1100x820 or 1280x720
- **THEN** essential controls and values remain readable and inside their usable viewport or intentional scroll region
- **AND** all nine world plots and the battle approach fit above the bottom HUD

#### Scenario: Click through decorative imagery
- **WHEN** the player clicks a selectable world plot near an information-only icon or health bar while no modal is open
- **THEN** the decoration does not consume the plot input
- **AND** opening a modal continues to prevent underlying world and match actions
