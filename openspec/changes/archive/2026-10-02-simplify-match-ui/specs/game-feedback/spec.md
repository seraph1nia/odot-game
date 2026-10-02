# Spec Delta

## MODIFIED Requirements

### Requirement: Authoritative overhead unit health bars
Every visible, deployed, living friendly and enemy unit in the observed city SHALL show a compact Trio-styled health bar above its model, including units at full health. The fill SHALL represent current authoritative health divided by that unit's authoritative maximum health, clamped to the zero-to-one range, using the same health scale for both values. Maximum health SHALL include the unit's effective archetype and research rank rather than the health first observed by this client. Bars SHALL remain readable against the battlefield at supported window sizes and follow unit movement and camera/layout changes. Overhead unit names, role codes such as S L1, enemy prefixes and BOSS labels SHALL NOT be shown. Each bar SHALL show the authoritative unit level as a conventional Roman numeral immediately above its left part, not the research rank or recruitment-building level. Numerals SHALL remain readable and follow the same projection and visibility rules as the bar. Full name, role, faction and boss identification SHALL remain available through click inspection and army details independently of interpreting health-fill color. Bars SHALL be cosmetic and SHALL NOT intercept input, alter combat or introduce manual unit control.

#### Scenario: Observe a wounded ranked unit
- **WHEN** a client first receives or reconnects to a living unit with current health 525 and authoritative maximum health 1050 in the wire's integer health scale
- **THEN** its overhead bar shows half health immediately
- **AND** the client does not treat 525 as that unit's full health

#### Scenario: Follow visible allies and enemies
- **WHEN** living friendly and enemy units move in the focused city and the window changes between supported sizes
- **THEN** each bar stays above its corresponding visible model and retains a readable size
- **AND** full-health units also show a full bar with a Roman level marker, overhead names/codes are absent, and faction/role identification remains available in click inspection and army details

#### Scenario: Read a veteran's level
- **WHEN** a level-four veteran remains in the army after its recruitment building changes level
- **THEN** its health bar has IV immediately above the left part, with no overhead unit name or role code
- **AND** inspection uses that veteran's own level and authoritative current/max health and damage

