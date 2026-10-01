# Spec Delta

## MODIFIED Requirements

### Requirement: Shared archetypes and faction identity
Units SHALL independently identify faction, archetype and melee/ranged/magic class. Swordsman and Berserker SHALL be melee, Crossbowman ranged, and Mage magic. Equal archetype and research rank SHALL use identical maximum health, damage, capacity cost and legal footprints, initiative, integer hex range, move duration, windup, recovery, death duration and splash rules across factions. Faction SHALL determine allegiance, entry direction and presentation, not a hidden strength modifier. Swordsman SHALL be the durable baseline, Berserker a more damaging but less durable melee role, Crossbowman reliable single-target support, and Mage fragile slower splash support. Class SHALL NOT introduce a damage-counter multiplier in this change. Identity-based targeting, bounded hex occupancy, simultaneous damage, seeded tie-breaking and queued arrivals SHALL apply to every archetype. Character-type initiative SHALL use lower values first for equal-distance targeting and contested movement. Automatic placement SHALL preserve the declared forward melee/rear support policy and legal protected reinforcement fallback. Research SHALL continue to change only maximum health and damage; it SHALL NOT change capacity, initiative, range or action durations.

#### Scenario: Mirrored combat profiles
- **WHEN** an allied Mage and enemy skeleton Mage have equal research rank
- **THEN** their complete numerical combat profiles agree and only faction/presentation and ownership differ

#### Scenario: Mixed formation
- **WHEN** either faction deploys a mixed army
- **THEN** its melee units enter ahead of its ranged and magic units without overlapping
- **AND** ranged and magic units retain their configured attacks when reached in melee

#### Scenario: Mixed capacity and initiative
- **WHEN** different archetypes share an allied hex and request actions
- **THEN** their type-defined footprints consume the declared capacity without overlap
- **AND** lower initiative receives contested movement priority while equal initiative uses the fight seed

### Requirement: Bounded deterministic splash
Mage and Catapult Tower attacks SHALL apply damage only at the authoritative impact tick. Each attack SHALL have one locked primary target and a configured nonnegative integer hex radius and positive maximum victim count. Splash SHALL affect only living deployed opponents in the same destination, with no friendly fire or damage to another city's units. Eligible secondary victims SHALL be selected by hex distance from the primary's authoritative impact hex, then lowest initiative and seeded ties. Opponents sharing the primary hex SHALL have splash distance zero. Default Mage tuning SHALL make effective two-victim splash a useful clustered combat role without requiring every attack to hit its maximum cap; its isolated single-target sustained damage SHALL remain below the Crossbowman's. Moving opponents SHALL use their single authoritative attackable hex, and queued or dying bodies SHALL be ineligible. An invalid primary target SHALL cause a miss without retargeting or secondary damage. Every attack SHALL damage each chosen victim at most once, and all same-tick damage SHALL be accumulated before casualties. An enemy Mage attacking exposed city health SHALL apply one primary hit without secondary splash. Cosmetic projectiles SHALL NOT determine impact or damage.

#### Scenario: Cluster exceeds the splash cap
- **WHEN** more eligible opponents than the configured cap stand inside the impact radius
- **THEN** only the primary and the closest permitted secondary victims take damage
- **AND** reversed storage order yields the same victims and damage

#### Scenario: Primary transfers before impact
- **WHEN** a Mage's locked primary target transfers away before impact
- **THEN** the attack misses, damages no former neighbors and retains its normal recovery

#### Scenario: Shared hex receives bounded splash
- **WHEN** more living opponents share the primary impact hex than the victim cap permits
- **THEN** the primary is included once and permitted secondaries use initiative and seeded ties
- **AND** unused capacity positions, allied units and dying bodies receive no damage

### Requirement: Buildable city defenses
Arrow and Catapult Towers SHALL each occupy one owned building slot and have two paid levels. Arrow Towers SHALL provide regular single-target shots and Catapult Towers slower capped splash. Each tower and the built-in defender SHALL have its own deterministic action identity and configured positive windup/recovery, with city-wide access to living deployed enemies assigned to its city. They SHALL select at each attack boundary by hex distance from the same declared city-defense anchor, then lowest target initiative and seeded ties; their building-slot position SHALL NOT change that ordering. Their locked attacks SHALL participate in the common same-tick damage accumulator. Slot position SHALL NOT introduce an adjacency or terrain bonus. Towers SHALL NOT occupy combat capacity or transit space or be separate enemy attack targets; enemies attacking an exposed city SHALL damage city health. The weak built-in defender SHALL remain outside the nine slots and independent of buildable towers. Pause SHALL freeze tower attacks; elimination SHALL stop all that city's towers. An invalid target SHALL miss without granting a replacement shot.

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

## ADDED Requirements

### Requirement: Measurable frontline and support roles

Default profiles and automatic formation SHALL demonstrate distinct roles in small paired fixtures using normal profiles and placement policy. A nearer living melee screen SHALL protect support through its initial attacks, with an otherwise identical unscreened control taking earlier support damage. A crowded mixed formation SHALL give a queued melee unit actual access to an attack while opponents remain. A normal clustered fixture with at least two actual splash victims SHALL compare one Mage with one Crossbowman on the same other-unit setup and seed; the Mage SHALL deliver greater effective non-overkill support damage over a shared interval when both supports are alive and improve clear time or surviving friendly/city health. An isolated single-target control SHALL retain superior Crossbowman sustained damage. Evidence SHALL include per-role action/victim counts, actual recruitment gold/food, remaining health and outcome; a winning mixed strategy containing an ineffective Mage SHALL NOT satisfy this requirement. Comparisons SHALL disclose the different recruitment costs and SHALL NOT claim universal equal-cost superiority or rely on manually fabricated clustering, inflated target health or friendly-fire damage.

#### Scenario: Frontline protects the Crossbowman
- **WHEN** paired ordinary formations run with a nearer melee screen and with that screen absent
- **THEN** the screened support completes its initial attacks before taking the damage observed earlier in the unscreened control
- **AND** enemy target identities demonstrate why the screen provided protection

#### Scenario: Mage contributes against ordinary clustering
- **WHEN** normal enemy placement and combat create a two-or-more-victim cluster and the paired support comparison runs
- **THEN** the Mage's real secondary hits provide the required effective damage and clear-time or surviving-health benefit
- **AND** replacing the Mage with a Crossbowman is compared under the same seed and disclosed costs

#### Scenario: Crossbowman retains its single-target role
- **WHEN** the two support profiles attack an isolated valid target at their declared full cadence
- **THEN** Crossbowman sustained damage exceeds Mage primary-only damage
- **AND** profiles remain identical across factions at equal research rank
