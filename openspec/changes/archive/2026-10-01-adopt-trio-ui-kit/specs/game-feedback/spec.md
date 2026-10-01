# Spec Delta

## ADDED Requirements

### Requirement: Authoritative overhead unit health bars
Every visible, deployed, living friendly and enemy unit in the observed city SHALL show a compact Trio-styled health bar above its model, including units at full health. The fill SHALL represent current authoritative health divided by that unit's authoritative maximum health, clamped to the zero-to-one range, using the same health scale for both values. Maximum health SHALL include the unit's effective archetype and research rank rather than the health first observed by this client. Bars SHALL remain readable against the battlefield at supported window sizes and follow unit movement and camera/layout changes. Friendly/enemy and role identification SHALL remain available independently of interpreting health-fill color. Bars SHALL be cosmetic and SHALL NOT intercept input, alter combat or introduce manual unit control.

#### Scenario: Observe a wounded ranked unit
- **WHEN** a client first receives or reconnects to a living unit with current health 525 and authoritative maximum health 1050 in the wire's integer health scale
- **THEN** its overhead bar shows half health immediately
- **AND** the client does not treat 525 as that unit's full health

#### Scenario: Follow visible allies and enemies
- **WHEN** living friendly and enemy units move in the focused city and the window changes between supported sizes
- **THEN** each bar stays above its corresponding visible model and retains a readable size
- **AND** full-health units also show a full bar and faction/role identification remains available

### Requirement: Health bar visibility and lifetime
Health bars SHALL reflect the same observed unit state and presentation lifetime as their models. Shared pause or transport loss SHALL freeze their last observed health and placement with battle presentation until synchronization resumes. Units outside the observed city, undeployed arrivals, units behind the camera or outside its visible world area, and death visuals SHALL NOT show living health bars. Switching cities, reconnecting and enemy redistribution SHALL reconstruct the correct visible bars from current state without duplicates or historical damage playback. Session end and fresh-match transitions SHALL release previous bars, with the number of live bars bounded by visible living units. Headless roles SHALL create no health-bar presentation.

#### Scenario: Pause after damage
- **WHEN** a visible unit has lost health and the shared match pauses or transport is lost
- **THEN** its health bar and unit presentation retain their last observed state without simulated damage or cosmetic progression
- **AND** recovery uses newly synchronized state without replaying old damage

#### Scenario: Observe a casualty
- **WHEN** a running graphical client observes a unit's authoritative death
- **THEN** the unit's living health bar disappears immediately while its bounded death visual can finish
- **AND** no bar remains after the unit visual is removed

#### Scenario: Change the observed city and restart
- **WHEN** the player switches to another city, receives a redistributed enemy, or starts a fresh session
- **THEN** bars correspond only to the current visible living units and their current health
- **AND** no bar is duplicated or retained from the previous city or match
