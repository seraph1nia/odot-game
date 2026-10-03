## MODIFIED Requirements

### Requirement: Shared archetypes and faction identity
All ordinary archetypes SHALL have size two and bosses SHALL have size six. Size SHALL be an integer from one through six; no current ordinary archetype SHALL use size one. Units SHALL independently identify faction, archetype, melee/ranged/magic class, positive unit level and ordinary/boss identity. Swordsman and Berserker SHALL be melee, Crossbowman ranged, and Mage magic. Equal archetype, unit level, research stat modifiers, technology capabilities, active statuses and explicit boss modifiers SHALL use identical maximum health, damage, unit size, initiative, integer hex range, move duration, windup, recovery, death duration and splash rules across factions. Faction SHALL determine allegiance, entry direction and presentation, not a hidden strength modifier. Swordsman SHALL be the durable baseline, Berserker a more damaging but less durable melee role, Crossbowman reliable single-target support, and Mage fragile slower splash support. Class SHALL NOT introduce a damage-counter multiplier in this change. Identity-based targeting, bounded hex occupancy, simultaneous damage, seeded tie-breaking and queued arrivals SHALL apply to every archetype. Character-type initiative SHALL use lower values first for equal-distance targeting and contested movement. Automatic enemy and isolated non-roster placement SHALL preserve the declared forward melee/rear support policy and legal protected reinforcement fallback. City-roster allied units SHALL instead start at persistent purchased first-fit homes under army-roster, without class-based repacking or a profile change. Class foundations SHALL change only maximum health and direct damage. Specializations SHALL grant the declared direct-damage, damage-reduction or burn/poison/chill capability. Technology and status effects SHALL NOT change capacity, initiative, range or base action durations; active chill SHALL modify only newly committed action durations under the combat-status-effects contract.

#### Scenario: Mirrored combat profiles
- **WHEN** an allied Mage and enemy skeleton Mage have equal unit level, research stat modifiers, capabilities, active statuses and boss modifiers
- **THEN** their complete numerical combat profiles agree and only faction/presentation and ownership differ

#### Scenario: Mixed formation
- **WHEN** either faction automatically deploys an isolated non-roster mixed army, or an enemy mixed army enters a city
- **THEN** its melee units enter ahead of its ranged and magic units without overlapping
- **AND** ranged and magic units retain their configured attacks when reached in melee

#### Scenario: Mixed capacity and initiative
- **WHEN** different archetypes share an allied hex and request actions
- **THEN** their sizes sum to at most six and each actor has a distinct rendered anchor
- **AND** lower initiative receives contested movement priority while equal initiative uses the fight seed

#### Scenario: Allied homes do not change role statistics
- **WHEN** a city inserts different roles by first-fit purchased home order
- **THEN** those exact assignments determine initial allied placement without moving existing support to another tile
- **AND** each role retains its shared numerical profile, attack and ordinary movement rules
