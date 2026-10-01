# Spec Delta

## MODIFIED Requirements

### Requirement: Health bar visibility and lifetime
Health bars SHALL reflect the same observed unit state and presentation lifetime as their models. Shared pause or transport loss SHALL freeze their last observed health and world-space anchors with battle presentation until synchronization resumes, while screen placement and visibility SHALL continue to follow local camera and layout changes. Units outside the observed city, undeployed arrivals, units behind the camera or outside its visible world area, and death visuals SHALL NOT show living health bars. Switching cities, reconnecting and enemy redistribution SHALL reconstruct the correct visible bars from current state without duplicates or historical damage playback. Session end and fresh-match transitions SHALL release previous bars, with the number of live bars bounded by visible living units. Headless roles SHALL create no health-bar presentation.

#### Scenario: Pause after damage
- **WHEN** a visible unit has lost health and the shared match pauses or transport is lost
- **THEN** its health bar and unit presentation retain their last observed health and world-space state without simulated damage or cosmetic progression
- **AND** recovery uses newly synchronized state without replaying old damage

#### Scenario: Move the camera over frozen units
- **WHEN** the player zooms or pans while battle presentation is paused or disconnected
- **THEN** bars reproject above their frozen unit anchors and visibility follows the current world area
- **AND** unit poses, world positions and displayed health remain unchanged

#### Scenario: Observe a casualty
- **WHEN** a running graphical client observes a unit's authoritative death
- **THEN** the unit's living health bar disappears immediately while its bounded death visual can finish
- **AND** no bar remains after the unit visual is removed

#### Scenario: Change the observed city and restart
- **WHEN** the player switches to another city, receives a redistributed enemy, or starts a fresh session
- **THEN** bars correspond only to the current visible living units and their current health
- **AND** no bar is duplicated or retained from the previous city or match
