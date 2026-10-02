# Spec Delta

## MODIFIED Requirements

### Requirement: Shared archetypes and faction identity
Units SHALL independently identify faction, archetype, melee/ranged/magic class, positive unit level and ordinary/boss identity. Swordsman and Berserker SHALL be melee, Crossbowman ranged, and Mage magic. Equal archetype, unit level, research rank and explicit boss modifiers SHALL use identical maximum health, damage, capacity cost and legal footprints, initiative, integer hex range, move duration, windup, recovery, death duration and splash rules across factions. Faction SHALL determine allegiance, entry direction and presentation, not a hidden strength modifier. Swordsman SHALL be the durable baseline, Berserker a more damaging but less durable melee role, Crossbowman reliable single-target support, and Mage fragile slower splash support. Class SHALL NOT introduce a damage-counter multiplier in this change. Identity-based targeting, bounded hex occupancy, simultaneous damage, seeded tie-breaking and queued arrivals SHALL apply to every archetype. Character-type initiative SHALL use lower values first for equal-distance targeting and contested movement. Automatic placement SHALL preserve the declared forward melee/rear support policy and legal protected reinforcement fallback. Research SHALL continue to change only maximum health and damage; it SHALL NOT change capacity, initiative, range or action durations.

#### Scenario: Mirrored combat profiles
- **WHEN** an allied Mage and enemy skeleton Mage have equal unit level, research rank and boss modifiers
- **THEN** their complete numerical combat profiles agree and only faction/presentation and ownership differ

#### Scenario: Mixed formation
- **WHEN** either faction deploys a mixed army
- **THEN** its melee units enter ahead of its ranged and magic units without overlapping
- **AND** ranged and magic units retain their configured attacks when reached in melee

#### Scenario: Mixed capacity and initiative
- **WHEN** different archetypes share an allied hex and request actions
- **THEN** their type-defined footprints consume the declared capacity without overlap
- **AND** lower initiative receives contested movement priority while equal initiative uses the fight seed

### Requirement: Small bounded class research
A living player SHALL buy melee, ranged or magic research from an owned Blacksmith during editable unpaused building or preparation. Each class SHALL have two paid ranks: a level-one Blacksmith unlocks rank one and level two unlocks rank two. Each rank SHALL increase level-scaled unresearched maximum health and attack damage by five percent, additively for a maximum ten percent increase. Selling any or all Blacksmiths SHALL retain purchased ranks and their effects, without refunding research costs; further research SHALL require a currently owned Blacksmith of sufficient level. Rank SHALL be city-wide and persistent; duplicate Blacksmiths SHALL NOT grant extra ranks, free bonuses or stacking. Existing and future allied units of the class SHALL receive the rank. Existing current health SHALL remain unchanged when maximum health increases; research SHALL NOT heal. Research SHALL NOT modify built-in defense or buildable towers. Unit level SHALL be independent of class research rank. Research SHALL recalculate each unit from its own level and SHALL NOT replace that level with the city or building level. Enemy wave ranks SHALL be explicitly configured and SHALL NOT inherit destination research during redistribution. Fractional stat changes SHALL retain their intended small value instead of becoming zero or a whole extra damage point.

#### Scenario: Research affects existing and future troops
- **WHEN** a player buys melee rank one with a wounded Swordsman already alive
- **THEN** its maximum health and attack damage increase by five percent while its current health remains unchanged
- **AND** a subsequently recruited Berserker receives the same rank and begins at its researched maximum health

#### Scenario: Duplicate or unavailable research
- **WHEN** a player buys a third rank, retries a paid rank, or requests rank two with only level-one Blacksmiths
- **THEN** no unsupported rank or duplicate bonus is granted and no repeated charge occurs

#### Scenario: Research applies to mixed-level survivors
- **WHEN** a city buys melee rank one with surviving level-one and level-three Swordsmen
- **THEN** each gains five percent of its own level-scaled unresearched maximum health and damage
- **AND** both retain their individual levels and unchanged current health


#### Scenario: Sell and rebuild a Blacksmith
- **WHEN** a city sells its only level-two Blacksmith after buying research, then builds a level-one replacement
- **THEN** purchased ranks and their effects on existing and future units remain without another payment or bonus
- **AND** any still-unpurchased rank two requires upgrading the replacement Blacksmith

### Requirement: Buildable city defenses
Arrow and Catapult Towers SHALL each occupy one purchased owned building slot and have two paid levels. Arrow Towers SHALL provide regular single-target shots and Catapult Towers slower capped splash. Each tower and the built-in defender SHALL have its own deterministic action identity and configured positive windup/recovery, with city-wide access to living deployed enemies assigned to its city. They SHALL select at each attack boundary by hex distance from the same declared city-defense anchor, then lowest target initiative and seeded ties; their building-slot position SHALL NOT change that ordering. Their locked attacks SHALL participate in the common same-tick damage accumulator. Slot position SHALL NOT introduce an adjacency or terrain bonus. Towers SHALL NOT occupy combat capacity or transit space or be separate enemy attack targets; enemies attacking an exposed city SHALL damage city health. The weak built-in defender SHALL remain outside the nine slots and independent of buildable towers. Selling a tower SHALL remove its defense state and future attacks; rebuilding on that plot SHALL NOT inherit pending actions or replay old effects. Pause SHALL freeze tower attacks; elimination SHALL stop all that city's towers. An invalid target SHALL miss without granting a replacement shot.

#### Scenario: Multiple towers and city elimination
- **WHEN** two towers are built and their city falls during combat
- **THEN** they cease attacking and cannot damage enemies transferred to a surviving city
- **AND** the built-in defender remains separately accounted for

#### Scenario: Pause during a catapult windup
- **WHEN** the match pauses before a Catapult Tower impact
- **THEN** its impact and recovery timers remain frozen and resume from their remaining duration

#### Scenario: Defense targeting is consistent
- **WHEN** two towers in different building slots begin an attack against the same enemy setup
- **THEN** both use the declared city-defense distance and initiative policy
- **AND** exact ties use each defense action's reproducible seeded decision rather than storage order

#### Scenario: Sell and replace a tower
- **WHEN** a tower is sold between waves and another tower is built on the same plot
- **THEN** the old tower can produce no attacks and the new level-one tower has its own investment and fresh action lifecycle
- **AND** neither built-in defense nor another tower changes

## ADDED Requirements

### Requirement: Equipment and upkeep costs in role comparisons
The retained paired frontline/support and Mage/Crossbowman role comparisons SHALL disclose complete material/gold recruitment costs and separate per-battle food upkeep at the compared unit levels. Their ordinary battlefield setup SHALL satisfy the applicable upkeep rule before combat and SHALL identify any shortage effect rather than attributing it to archetype balance. Zero food recruitment SHALL NOT be reported as a food-free army. Existing effective two-victim Mage, Crossbowman single-target, shared-seed, normal-formation and no-universal-equal-cost-superiority guarantees SHALL remain required.

#### Scenario: Compare supports with the new economy
- **WHEN** the paired Mage and Crossbowman support fixtures compare equal unit levels under normal paid upkeep
- **THEN** evidence includes cloth/gold versus metal/wood recruitment and each unit's food upkeep
- **AND** the Mage still demonstrates its clustered contribution while Crossbowman retains the isolated single-target advantage

### Requirement: Shared geometric unit progression
Each archetype SHALL declare level-one maximum health and damage. The initial shared health/damage growth multiplier SHALL be 1.35; an archetype with no explicit override SHALL use that shared multiplier. For each stat, level L SHALL resolve from its original base as round(base times multiplier raised to L minus one), to the nearest whole point with midpoint ties upward. Level one SHALL equal the declared base. Explicit boss multipliers SHALL apply after level scaling, and the existing additive research factor SHALL then apply without rounding away its fractional effect. Levels SHALL NOT alter movement, initiative, range, footprint, attack timing or splash rules. Ordinary faction identity SHALL NOT modify numerical strength. Authored enemy levels SHALL be independent of the recruit-building cap, subject to configuration validation of representable health/damage and safe combat accumulation. Invalid levels, invalid multipliers, unknown archetypes and overflowing profiles SHALL be rejected before a match starts or an economic action spends resources. Equivalent configuration and inputs SHALL resolve identical profiles.

#### Scenario: Baseline Swordsman progression
- **WHEN** the default Swordsman base is 40 health and 10 damage and its level increases from one through five
- **THEN** the unresearched ordinary health values are 40, 54, 73, 98 and 133 and damage values are 10, 14, 18, 25 and 33
- **AND** movement, attack cadence and footprint are unchanged

#### Scenario: An authored wave mixes arbitrary supported levels
- **WHEN** one wave contains level-one Crossbowmen and a level-six Swordsman
- **THEN** each uses its own level-scaled profile even though recruitment buildings stop at level five

#### Scenario: Unsafe profile is rejected
- **WHEN** configuration contains a zero or negative level, a nonpositive or invalid growth multiplier, or a profile that would overflow valid combat arithmetic
- **THEN** configuration is rejected before starting combat or charging a recruitment purchase
