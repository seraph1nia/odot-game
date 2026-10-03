# Spec Delta

## MODIFIED Requirements

### Requirement: Shared archetypes and faction identity
All ordinary archetypes SHALL have size two and bosses SHALL have size six. Size SHALL be an integer from one through six; no current ordinary archetype SHALL use size one. Units SHALL independently identify faction, archetype, melee/ranged/magic class, positive unit level and ordinary/boss identity. Swordsman and Berserker SHALL be melee, Crossbowman ranged, and Mage magic. Equal archetype, unit level, research stat modifiers, technology capabilities, active statuses and explicit boss modifiers SHALL use identical maximum health, damage, unit size, initiative, integer hex range, move duration, windup, recovery, death duration and splash rules across factions. Faction SHALL determine allegiance, entry direction and presentation, not a hidden strength modifier. Swordsman SHALL be the durable baseline, Berserker a more damaging but less durable melee role, Crossbowman reliable single-target support, and Mage fragile slower splash support. Class SHALL NOT introduce a damage-counter multiplier in this change. Identity-based targeting, bounded hex occupancy, simultaneous damage, seeded tie-breaking and queued arrivals SHALL apply to every archetype. Character-type initiative SHALL use lower values first for equal-distance targeting and contested movement. Automatic placement SHALL preserve the declared forward melee/rear support policy and legal protected reinforcement fallback. Class foundations SHALL change only maximum health and direct damage. Specializations SHALL grant the declared direct-damage, damage-reduction or burn/poison/chill capability. Technology and status effects SHALL NOT change capacity, initiative, range or base action durations; active chill SHALL modify only newly committed action durations under the combat-status-effects contract.

#### Scenario: Mirrored combat profiles
- **WHEN** an allied Mage and enemy skeleton Mage have equal unit level, research stat modifiers, capabilities, active statuses and boss modifiers
- **THEN** their complete numerical combat profiles agree and only faction/presentation and ownership differ

#### Scenario: Mixed formation
- **WHEN** either faction deploys a mixed army
- **THEN** its melee units enter ahead of its ranged and magic units without overlapping
- **AND** ranged and magic units retain their configured attacks when reached in melee

#### Scenario: Mixed capacity and initiative
- **WHEN** different archetypes share an allied hex and request actions
- **THEN** their sizes sum to at most six and each actor has a distinct rendered anchor
- **AND** lower initiative receives contested movement priority while equal initiative uses the fight seed

### Requirement: Shared geometric unit progression
Each archetype SHALL declare level-one maximum health and damage. The initial shared health/damage growth multiplier SHALL be 1.35; an archetype with no explicit override SHALL use that shared multiplier. For each stat, level L SHALL resolve from its original base as round(base times multiplier raised to L minus one), to the nearest whole point with midpoint ties upward. Level one SHALL equal the declared base. Explicit boss multipliers SHALL apply after level scaling, and validated technology-derived stat factors SHALL then apply without rounding away their fractional effect. Legacy authored enemy ranks SHALL remain explicit numerical modifiers and SHALL NOT inherit city technologies; no gold-paid player rank purchase remains. Levels SHALL NOT alter movement, initiative, range, size, attack timing or splash rules. Ordinary faction identity SHALL NOT modify numerical strength. Authored enemy levels SHALL be independent of the recruit-building cap, subject to configuration validation of representable health/damage and safe combat accumulation. Invalid levels, invalid multipliers, unknown archetypes and overflowing profiles SHALL be rejected before a match starts or an economic action spends resources. Equivalent configuration and inputs SHALL resolve identical profiles.

#### Scenario: Baseline Swordsman progression
- **WHEN** the default Swordsman base is 40 health and 10 damage and its level increases from one through five
- **THEN** the unresearched ordinary health values are 40, 54, 73, 98 and 133 and damage values are 10, 14, 18, 25 and 33
- **AND** movement, attack cadence and ordinary size two are unchanged

#### Scenario: An authored wave mixes arbitrary supported levels
- **WHEN** one wave contains level-one Crossbowmen and a level-six Swordsman
- **THEN** each uses its own level-scaled profile even though recruitment buildings stop at level five

#### Scenario: Unsafe profile is rejected
- **WHEN** configuration contains a zero or negative level, a nonpositive or invalid growth multiplier, or a profile that would overflow valid combat arithmetic
- **THEN** configuration is rejected before starting combat or charging a recruitment purchase

## REMOVED Requirements

### Requirement: Small bounded class research
**Reason**: Replaced by personal point-funded foundations, exclusive specializations and advanced technologies.
**Migration**: Replace gold-paid class/rank commands and Blacksmith-level purchase gating with technology-id purchases under city-research. Rename the building role to Research Tower, retain its plot/investment mechanics, and update fixtures/UI/protocol. Existing wounds and unit levels remain unchanged; fresh matches use the new research model without live-session or save migration.
