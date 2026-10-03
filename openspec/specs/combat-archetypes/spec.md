# combat-archetypes Specification

## Purpose

Give automatic battles distinct, readable combat roles while sharing numerical profiles between allied adventurers and enemy skeletons.

## Requirements

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

### Requirement: Measurable frontline and support roles

Default profiles and automatic formation SHALL demonstrate distinct roles in small paired fixtures using normal profiles and placement policy. A nearer living melee screen SHALL protect support through its initial attacks, with an otherwise identical unscreened control taking earlier support damage. A crowded mixed formation SHALL give a queued melee unit actual access to an attack while opponents remain. A normal clustered fixture with at least two actual splash victims SHALL compare one Mage with one Crossbowman on the same other-unit setup and seed; the Mage SHALL deliver greater effective non-overkill support damage over a shared interval when both supports are alive and improve clear time or surviving friendly/city health. An isolated single-target control SHALL retain superior Crossbowman sustained damage. Evidence SHALL include per-role action/victim counts, actual material/gold recruitment costs and separate food upkeep, remaining health and outcome; a winning mixed strategy containing an ineffective Mage SHALL NOT satisfy this requirement. Comparisons SHALL disclose the different recruitment costs and SHALL NOT claim universal equal-cost superiority or rely on manually fabricated clustering, inflated target health or friendly-fire damage.

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

### Requirement: Complete configured defense behavior

Every built-in defender and buildable tower SHALL apply its validated damage, windup, recovery, maximum victim count and splash radius from the frozen combat configuration. A built-in defender SHALL retain a distinct action identity and remain outside building-slot capacity. Accepted defender victim/radius settings SHALL NOT be silently replaced with single-target defaults. Default built-in defender and Arrow Tower profiles SHALL remain single-target. All defenses SHALL use the existing locked-primary, distance/initiative/seeded victim selection and common simultaneous-damage rules: only living deployed enemies in the same city allocation are eligible, each action hits a victim at most once, and an invalid primary causes a miss without secondary damage.

#### Scenario: Configured defender hits an eligible secondary
- **WHEN** a built-in defender configured with victim cap two and radius one impacts a valid primary with an eligible second enemy one hex away
- **THEN** both enemies receive the configured damage once on the impact tick
- **AND** no other victim exceeds the cap or radius

#### Scenario: Default defender remains single-target
- **WHEN** the default built-in defender impacts a primary inside an enemy cluster
- **THEN** only the primary receives its ordinary configured damage

#### Scenario: Configured splash cannot survive an invalid primary
- **WHEN** a splash-configured defender's primary dies or transfers before impact
- **THEN** the action misses without secondary damage and retains its original recovery deadline

#### Scenario: Unit and defense impacts remain simultaneous
- **WHEN** a unit and a defense have valid lethal contributions due on the same tick
- **THEN** all valid contributions are accumulated before casualties prevent subsequent actions
- **AND** changing storage traversal cannot remove a due valid contribution

### Requirement: Equipment and upkeep costs in role comparisons
The retained paired frontline/support and Mage/Crossbowman role comparisons SHALL disclose complete material/gold recruitment costs and separate per-battle food upkeep at the compared unit levels. Their ordinary battlefield setup SHALL satisfy the applicable upkeep rule before combat and SHALL identify any shortage effect rather than attributing it to archetype balance. Zero food recruitment SHALL NOT be reported as a food-free army. Existing effective two-victim Mage, Crossbowman single-target, shared-seed, normal-formation and no-universal-equal-cost-superiority guarantees SHALL remain required.

#### Scenario: Compare supports with the new economy
- **WHEN** the paired Mage and Crossbowman support fixtures compare equal unit levels under normal paid upkeep
- **THEN** evidence includes cloth/gold versus metal/wood recruitment and each unit's food upkeep
- **AND** the Mage still demonstrates its clustered contribution while Crossbowman retains the isolated single-target advantage

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
