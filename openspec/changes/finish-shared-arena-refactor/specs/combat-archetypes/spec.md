# Spec Delta

## MODIFIED Requirements

### Requirement: Shared archetypes and faction identity

Ordinary combat actors SHALL be homogeneous squads up to configured capacity three; bosses SHALL use the same actor architecture with exactly one member. Each member SHALL independently retain faction, archetype, class, positive equipment level, boss modifiers, health and capabilities. Swordsman/Berserker SHALL be melee, Crossbowman ranged and Mage magic. Equal type/level/research/status/boss inputs SHALL yield equal HP, X/Y, structural damage, initiative, range and timing across factions. Faction SHALL define allegiance and presentation, not hidden strength. One actor SHALL claim one hex regardless of population. Foundations SHALL modify individual maximum HP/direct damage; specializations SHALL retain Assault, Guardian, Precision, Fire, Venom and Frost behavior. Type/range/timing and equipment SHALL not change because a home is bought, a member is regrouped or presentation scales a boss.

#### Scenario: Mirrored Crossbowmen
- **WHEN** allied and enemy Crossbowmen have equal level/capabilities/modifiers
- **THEN** their complete numerical profiles agree, including any small default buff

#### Scenario: Single-member boss
- **WHEN** a boss enters combat
- **THEN** it uses one squad cell and the ordinary movement/attack cycle with one contributing member

#### Scenario: Mirrored combat profiles
- **WHEN** an allied Mage and enemy skeleton Mage have equal unit level, research stat modifiers, capabilities, active statuses and boss modifiers
- **THEN** their complete numerical combat profiles agree and only faction/presentation and ownership differ

#### Scenario: Mixed formation
- **WHEN** mixed archetypes deploy as separate homogeneous squads
- **THEN** no group mixes archetypes or overlaps another cell and ranged/magic attacks retain their profile at melee distance
- **AND** chosen allied preparation homes are preserved

#### Scenario: Mixed capacity and initiative
- **WHEN** different archetype squads request one destination
- **THEN** only one actor can claim it and lower actor initiative then seeded ties arbitrate
- **AND** no body-size stacking permits both to arrive

#### Scenario: Allied homes do not change role statistics
- **WHEN** a city inserts different roles by first-fit purchased home order
- **THEN** those exact assignments determine initial allied placement without moving existing support to another tile
- **AND** each role retains its shared numerical profile, attack and ordinary movement rules

### Requirement: Bounded deterministic splash

Every group attack SHALL use two independent configurable nonnegative channels: X to one deterministic primary member and Y to every other pre-impact living participating member of the selected group. Each living participating attacker SHALL contribute once using its individual profile and stable member/action identity; attackers may choose different primaries within the one locked group. Zero X or Y SHALL be permitted. Victim-count caps and cross-hex splash SHALL NOT limit or replace this model. Damage SHALL not affect neighboring squads, allies, stored/unfunded/queued/dying members. Overkill SHALL stay on its original victim. All same-tick contributions SHALL resolve before casualties. Loss of the entire target group SHALL miss without group retargeting; a partial casualty SHALL leave remaining living group members eligible for the impact's seeded primary choice. Structural attacks SHALL use separate per-member damage, not X/Y or a fake member HP pool.

#### Scenario: Zero-primary cleave
- **WHEN** a strike with X=0 and positive Y hits a five-member enemy group
- **THEN** its primary receives no direct damage and all four other living participants receive Y once
- **AND** no adjacent group is damaged

#### Scenario: One attacker survives
- **WHEN** two attackers die before the next eligible impact of their squad
- **THEN** only the surviving participating member contributes its own resolved profile

#### Scenario: Overkill
- **WHEN** primary damage exceeds one member's HP
- **THEN** surplus damage is not redistributed to another member

#### Scenario: Cluster exceeds the splash cap
- **WHEN** a configured positive-Y strike hits five pre-impact living enemy group members
- **THEN** the primary receives X and all four other members receive Y without a victim cap
- **AND** reversed storage yields identical contributions and victims

#### Scenario: Primary transfers before impact
- **WHEN** a locked enemy group redirects its preferred sector during windup
- **THEN** redirection alone does not invalidate its physical group target
- **AND** impact still checks actual living participants and physical range without teleportation

#### Scenario: Shared hex receives bounded splash
- **WHEN** a homogeneous target squad shares one cell
- **THEN** each contributing attacker hits one primary and all other living participants with its X/Y
- **AND** adjacent groups, allies and dying members are ineligible

### Requirement: Buildable city defenses

Existing purchased plot quotes, refunds and exclusive Rapid Arrow/Bombard paths SHALL remain. Each purchased tower and built-in defender SHALL have its own action identity, windup/cadence and physical origin/range in the shared arena. The built-in defender SHALL originate at the owner's Watchpost, not add a second free weapon. Static weapon/objective cells SHALL be excluded from squad occupancy; an army-plot tower SHALL also exclude that home's assignments until sold. Weapons SHALL select living enemy groups by physical range/distance and seeded ties regardless of origin sector. Each weapon SHALL contribute one X/Y strike; arrows SHALL remain X-only and Catapult/Bombard SHALL use within-group channels. Purchased towers SHALL remain indestructible in this change and cease after Keep loss without sale/refund or fabricated destruction. Sold/rebuilt weapons SHALL have fresh action state.

#### Scenario: Foreign-origin enemy in tower range
- **WHEN** a foreign-origin group enters a living owner's tower range
- **THEN** it is eligible for that tower's ordinary attack without changing its ownership/origin

#### Scenario: Keep loss
- **WHEN** an owner's Keep falls
- **THEN** its weapons stop while purchased tower buildings remain retained and no new refund is granted

#### Scenario: Multiple towers and city elimination
- **WHEN** two towers are built and their city falls during combat
- **THEN** they cease attacking and cannot damage enemies transferred to a surviving city
- **AND** the built-in defender remains separately accounted for

#### Scenario: Pause during a catapult windup
- **WHEN** the match pauses before a Catapult Tower impact
- **THEN** its impact and recovery timers remain frozen and resume from their remaining duration

#### Scenario: Defense targeting is consistent
- **WHEN** weapons in different physical cells inspect the same enemy groups
- **THEN** each uses its own physical range/distance and reproducible seeded ties
- **AND** slot position matters geometrically without adding hidden damage bonuses

#### Scenario: Sell and replace a tower
- **WHEN** a tower is sold between waves and another tower is built on the same plot
- **THEN** the old tower can produce no attacks and the new level-one tower has its own investment and fresh action lifecycle
- **AND** neither built-in defense nor another tower changes

### Requirement: Measurable frontline and support roles

Bounded shared-arena encounters SHALL retain meaningful melee, ranged and magic behavior. Melee SHALL hold/intercept at adjacent range; Crossbowman SHALL retain superior sustained primary-only damage to Mage at default cadence; Mage's positive Y SHALL contribute effective damage against multiple living members. Normal single-target and clustered-group comparisons SHALL disclose profiles, participating counts and material/upkeep costs, without artificial victim caps, inflated HP or friendly-fire evidence. A bounded full-force numerical encounter SHALL show six Crossbowmen in two three-member squads surviving the default first-wave composition without melee, with arithmetic setup explicitly separated from earned payments. A small default profile improvement SHALL support this obligation while preserving faction symmetry and other roles. Pure-Crossbow paid opening/progression SHALL remain recorded diagnostic evidence deferred by the owner, not a landing acceptance obligation or a campaign viability claim. This SHALL NOT promise every composition/deployment wins twenty waves.

#### Scenario: Pure ranged opening
- **WHEN** an ordinary paid Crossbowman-only policy runs with default rules on retained seeds 1 and 123
- **THEN** its actual paid strength, casualties and defeats remain recorded as deferred balance diagnostics
- **AND** separately labeled normal-profile six-member encounters at both seeds clear with a living Keep and ranged survivors without melee or BattleStalled

#### Scenario: Distinct damage roles
- **WHEN** default Mage and Crossbowman profiles are compared against one member and a multi-member group at their actual cadence
- **THEN** Crossbowman retains its single-target advantage and Mage's actual Y victims demonstrate its group role

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

Published frozen weapon profiles SHALL match damage channels, physical range and positive windup/cadence actually used. Default arrows and Watchpost defender SHALL use X-only; Catapult/Bombard SHALL damage all other eligible living members of the selected group with configured Y. Victims SHALL not be capped or selected across cells. Every weapon SHALL preserve committed target-group validity, simultaneous resolution, pause and miss/recovery semantics. Catalogs, forecasts and inspection SHALL expose the same effective channels/range/timings.

#### Scenario: Bombard versus five members
- **WHEN** a valid Bombard impact hits a five-member group
- **THEN** its primary receives X and all four others receive Y, while adjacent groups receive nothing

#### Scenario: Invalid locked group
- **WHEN** the last target member dies before a weapon impact
- **THEN** it reports one miss and retains recovery without acquiring a replacement shot

#### Scenario: Configured defender hits an eligible secondary
- **WHEN** a supported weapon profile with positive Y lands on a multi-member group
- **THEN** all its living secondary members receive the configured Y once
- **AND** adjacent groups receive no cross-cell splash and default Watchpost/arrow profiles remain X-only

#### Scenario: Default defender remains single-target
- **WHEN** the default built-in defender impacts a primary inside an enemy cluster
- **THEN** only the primary receives its ordinary configured damage

#### Scenario: Configured splash cannot survive an invalid primary
- **WHEN** the final member of a weapon's locked target group dies before impact
- **THEN** the shot misses without secondary damage and preserves recovery

#### Scenario: Unit and defense impacts remain simultaneous
- **WHEN** a unit and a defense have valid lethal contributions due on the same tick
- **THEN** all valid contributions are accumulated before casualties prevent subsequent actions
- **AND** changing storage traversal cannot remove a due valid contribution

### Requirement: Equipment and upkeep costs in role comparisons

Role and ranged-opening evidence SHALL disclose real per-member materials/gold, separate per-battle food, funded participant counts and actual deployment. Ordinary paid acceptance SHALL obey exactly three END settlements and spend no income before it exists. A one-member ranged opening SHALL not be called a full army. A mechanically valid losing diagnostic SHALL not be reported as strategy viability. Arithmetic profile comparisons SHALL be labeled separately from earned economy/campaign acceptance.

#### Scenario: Underfunded diagnostic
- **WHEN** a paid policy fields only one Crossbowman and loses
- **THEN** evidence reports its actual recruitment/payment/population and does not certify or disprove full-force survival

#### Scenario: Compare supports with the new economy
- **WHEN** the paired Mage and Crossbowman support fixtures compare equal unit levels under normal paid upkeep
- **THEN** evidence includes cloth/gold versus metal/wood recruitment and each unit's food upkeep
- **AND** the Mage still demonstrates its clustered contribution while Crossbowman retains the isolated single-target advantage

### Requirement: Shared geometric unit progression

Individual maximum HP and direct X/Y/structural damage SHALL resolve from original archetype bases with shared level multiplier 1.35 unless explicitly overridden, rounding nearest whole point with midpoint ties upward. Explicit boss and research modifiers SHALL apply at their declared stages without losing fractional research effects. Equipment level SHALL not alter squad capacity, movement, initiative, range, cadence or formation slot. Enemy authored levels SHALL remain independent of trainer maximum five and SHALL not acquire city research. Invalid level/multiplier or overflowing profile SHALL reject before gameplay/spending. Reinforcement SHALL create a new member at the selected trainer's level without upgrading/healing veterans.

#### Scenario: Existing Swordsman progression
- **WHEN** a default Swordsman increases from equipment level one through five
- **THEN** its HP remains 40/54/73/98/133 and X remains 10/14/18/25/33 with unchanged ordinary action timings

#### Scenario: Reinforcement after trainer upgrade
- **WHEN** a level-one wounded veteran's squad receives a paid level-three recruit
- **THEN** the recruit uses level-three scaling and the veteran retains its individual level/HP/experience

#### Scenario: Baseline Swordsman progression
- **WHEN** the default Swordsman base40HP/10X scales from levels1through5
- **THEN** HP40/54/73/98/133 and X10/14/18/25/33 retain ordinary timings
- **AND** each containing squad still has one exclusive cell

#### Scenario: An authored wave mixes arbitrary supported levels
- **WHEN** one wave contains level-one Crossbowmen and a level-six Swordsman
- **THEN** each uses its own level-scaled profile even though recruitment buildings stop at level five

#### Scenario: Unsafe profile is rejected
- **WHEN** configuration contains a zero or negative level, a nonpositive or invalid growth multiplier, or a profile that would overflow valid combat arithmetic
- **THEN** configuration is rejected before starting combat or charging a recruitment purchase
