# combat-archetypes Specification

## Purpose

Give automatic battles distinct, readable combat roles while sharing numerical profiles between allied adventurers and enemy skeletons.

## Requirements

### Requirement: Shared archetypes and faction identity
Units SHALL independently identify faction, archetype and melee/ranged/magic class. Swordsman and Berserker SHALL be melee, Crossbowman ranged, and Mage magic. Equal archetype and research rank SHALL use identical maximum health, damage, movement speed, range, windup, cadence and splash rules across factions. Faction SHALL determine allegiance, entry direction and presentation, not a hidden strength modifier. Swordsman SHALL be the durable baseline, Berserker a more damaging but less durable melee role, Crossbowman reliable single-target support, and Mage fragile slower splash support. Class SHALL NOT introduce a damage-counter multiplier in this change. Identity-based targeting, bounded contact, simultaneous damage, deterministic tie-breaking and queued arrivals SHALL continue for every archetype.

#### Scenario: Mirrored combat profiles
- **WHEN** an allied Mage and enemy skeleton Mage have equal research rank
- **THEN** their complete numerical combat profiles agree and only faction/presentation and ownership differ

#### Scenario: Mixed formation
- **WHEN** either faction deploys a mixed army
- **THEN** its melee units enter ahead of its ranged and magic units without overlapping
- **AND** ranged and magic units retain their configured attacks when reached in melee

### Requirement: Bounded deterministic splash
Mage and Catapult Tower attacks SHALL apply damage only at the authoritative impact tick. Each attack SHALL have one locked primary target and a configured radius and maximum victim count. Splash SHALL affect only living deployed opponents in the same destination, with no friendly fire or damage to another city's units. Eligible secondary victims SHALL be selected by distance from the primary impact position and stable identity ties. An invalid primary target SHALL cause a miss without retargeting or secondary damage. Every attack SHALL damage each chosen victim at most once, and all same-tick damage SHALL be accumulated before casualties. An enemy Mage attacking exposed city health SHALL apply one primary hit without secondary splash. Cosmetic projectiles SHALL NOT determine impact or damage.

#### Scenario: Cluster exceeds the splash cap
- **WHEN** more eligible opponents than the configured cap stand inside the impact radius
- **THEN** only the primary and the closest permitted secondary victims take damage
- **AND** reversed storage order yields the same victims and damage

#### Scenario: Primary transfers before impact
- **WHEN** a Mage's locked primary target transfers away before impact
- **THEN** the attack misses, damages no former neighbors and retains its normal recovery

### Requirement: Small bounded class research
A living player SHALL buy melee, ranged or magic research from an owned Blacksmith during editable unpaused building or preparation. Each class SHALL have two paid ranks: a level-one Blacksmith unlocks rank one and level two unlocks rank two. Each rank SHALL increase base maximum health and base attack damage by five percent, additively for a maximum ten percent increase. Rank SHALL be city-wide and persistent; duplicate Blacksmiths SHALL NOT grant extra ranks, free bonuses or stacking. Existing and future allied units of the class SHALL receive the rank. Existing current health SHALL remain unchanged when maximum health increases; research SHALL NOT heal. Research SHALL NOT modify built-in defense or buildable towers. Enemy wave ranks SHALL be explicitly configured and SHALL NOT inherit destination research during redistribution. Fractional stat changes SHALL retain their intended small value instead of becoming zero or a whole extra damage point.

#### Scenario: Research affects existing and future troops
- **WHEN** a player buys melee rank one with a wounded Swordsman already alive
- **THEN** its maximum health and attack damage increase by five percent while its current health remains unchanged
- **AND** a subsequently recruited Berserker receives the same rank and begins at its researched maximum health

#### Scenario: Duplicate or unavailable research
- **WHEN** a player buys a third rank, retries a paid rank, or requests rank two with only level-one Blacksmiths
- **THEN** no unsupported rank or duplicate bonus is granted and no repeated charge occurs

### Requirement: Buildable city defenses
Arrow and Catapult Towers SHALL each occupy one owned building slot and have two paid levels. Arrow Towers SHALL provide regular single-target shots and Catapult Towers slower capped splash. Each tower SHALL have its own deterministic attack identity, timing and configured profile, with city-wide access to deployed enemies assigned to its city. Slot position SHALL NOT introduce an adjacency or terrain bonus. Towers SHALL NOT occupy unit-body space or be separate enemy attack targets; enemies attacking an exposed city SHALL damage city health. The weak built-in defender SHALL remain outside the nine slots and independent of buildable towers. Pause SHALL freeze tower attacks; elimination SHALL stop all that city's towers. An invalid target SHALL miss without granting a replacement shot.

#### Scenario: Multiple towers and city elimination
- **WHEN** two towers are built and their city falls during combat
- **THEN** they cease attacking and cannot damage enemies transferred to a surviving city
- **AND** the built-in defender remains separately accounted for

#### Scenario: Pause during a catapult windup
- **WHEN** the match pauses before a Catapult Tower impact
- **THEN** its impact and recovery timers remain frozen and resume from their remaining duration
