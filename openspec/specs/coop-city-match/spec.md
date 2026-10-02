# coop-city-match Specification

## Purpose

Provide a small cooperative city-defense match whose economic choices and automatic battles exercise authoritative multiplayer with a persistent state.

## Requirements

### Requirement: Fixed roster and personal cities
Cooperative play SHALL offer a lobby for up to four players and an explicit start action that locks the current roster. In a player-hosted lobby only the original host SHALL start the match; explicit dedicated-server sessions SHALL retain their existing connected-member start behavior. Selecting Single player SHALL create exactly one local player and constitute the explicit start of that local match, with no network admission. Each roster member SHALL begin with an independent city of exactly nine empty indexed building slots, positive city health, starting gold, and a built-in ranged defender outside those slots. Slot identities SHALL remain stable across graphical placement, upgrades, observation, and reconnects; the slots SHALL NOT require square geometry. Each city SHALL also have a separate declared combat hex board whose adjacency, capacity and permanent faction deployment protection govern autobattle. Decorative hex terrain outside that board SHALL NOT add adjacency bonuses, movement rules or building capacity; the combat board SHALL NOT create additional building slots. One player SHALL be sufficient for solo play and hosting; the cooperative match SHALL support at least two simultaneous players. Fresh players SHALL NOT join an already started match, but roster members SHALL be able to reconnect.

#### Scenario: Start a cooperative match
- **WHEN** the original host of a hosted lobby, or a connected member of a dedicated-server lobby, starts a match with two players present
- **THEN** both players receive distinct cities with nine available slots and the same configured starting resources and city health
- **AND** the first building turn starts with the roster locked

#### Scenario: New arrival after start
- **WHEN** a client without valid credentials for a roster member joins after the match starts
- **THEN** it receives a clear refusal and no new city or wave allocation is created

#### Scenario: Resume a city with hexagonal plots
- **WHEN** a roster member resumes a city containing buildings and upgrades
- **THEN** all nine authoritative slot identities retain their buildings and levels at the corresponding graphical plots
- **AND** surrounding decorative terrain provides no extra building slots or gameplay effects

#### Scenario: Solo selection starts one city
- **WHEN** a player selects Single player from the start screen
- **THEN** one local city enters the first building turn with the same configured rules and starting grant as a cooperative city
- **AND** no guest connection can add a city to that match

### Requirement: Gold and food economy
Each living city SHALL receive the configured base gold income once per production turn. Mines SHALL add gold, farms SHALL add food, and lumbermills SHALL add wood during that production. Gold, food and wood SHALL be the only spendable resources. Each construction SHALL occupy exactly one empty owned slot and deduct its displayed gold and wood costs atomically. Farm, Mine, Lumbermill, Barracks, Archery Range, Arcanum, Blacksmith, Arrow Tower and Catapult Tower SHALL be available building types. Construction SHALL be allowed during unpaused building or final preparation for a living player who is not ready. Buildings bought before production SHALL produce that turn; preparation SHALL produce no resources. Starting grants and costs SHALL permit ordinary recruitment before wave one and construction of a lumbermill without already owning wood. Resources SHALL NOT become negative; stone and iron SHALL NOT be spendable resources.

#### Scenario: Construct and produce
- **WHEN** a player buys a mine and a farm in empty slots and the ready check resolves
- **THEN** their gold purchase costs are deducted once and they receive base gold plus mine output and farm food once
- **AND** another player's resources and buildings are unchanged by the purchases

#### Scenario: Invalid construction
- **WHEN** a player requests an occupied or out-of-range slot, an unknown building, an unaffordable purchase, or construction outside the permitted phase
- **THEN** the request fails with an explanation and leaves authoritative resources and buildings unchanged

#### Scenario: Wood cannot soft-lock construction
- **WHEN** a living city has exhausted its wood but can afford a lumbermill
- **THEN** it can build that lumbermill without spending wood and receive wood at its next production

#### Scenario: Multi-resource purchase is atomic
- **WHEN** a city has enough gold but insufficient wood for a construction or upgrade
- **THEN** the entire purchase is rejected without spending either resource or changing a slot

### Requirement: Manual recruitment and persistent soldiers
A player SHALL recruit one soldier by explicitly selecting an owned recruitment building during unpaused building or preparation while alive and not ready. Barracks SHALL offer Swordsman and Berserker, Archery Range SHALL offer Crossbowman, and Arcanum SHALL offer Mage. Unknown or mismatched archetypes SHALL be rejected without spending. Each successful recruitment SHALL deduct the selected archetype's configured food and supplemental gold costs, including its recruitment building's upgrade reduction, and immediately add one soldier of that type to that city's army. Unknown soldier types SHALL be rejected atomically; requests without an explicit type SHALL select Swordsman. Recruitment buildings SHALL NOT automatically recruit during production, and gold SHALL NOT substitute for required food. Default recruitment SHALL remain Swordsman from Barracks. Living soldiers SHALL retain their identities, types and remaining health between waves; dead soldiers SHALL NOT return automatically.

#### Scenario: Click a barracks to recruit
- **WHEN** a player activates their barracks with enough food
- **THEN** the food cost is deducted once and one soldier is added
- **AND** passing a later production turn alone does not recruit another soldier

#### Scenario: Missing barracks or food
- **WHEN** a player requests recruitment from a mismatched building slot or without sufficient food
- **THEN** neither resources nor army state changes

#### Scenario: Army survives a battle
- **WHEN** a wave ends with some soldiers alive and some killed
- **THEN** only the surviving soldiers remain for the next building phase with their remaining health

#### Scenario: Recruit each weapon type
- **WHEN** a player with enough food explicitly recruits a Swordsman from their Barracks and a Crossbowman from their Archery Range
- **THEN** one unit of each selected type is created with its configured combat profile and each request spends its displayed food cost once
- **AND** each recruitment building's upgrade reduces its own food cost

#### Scenario: Unknown recruitment type
- **WHEN** a request contains an unknown soldier type or retries an already accepted typed recruitment
- **THEN** an unknown type changes no resources or army state and an accepted retry creates no additional soldier or charge

#### Scenario: Default recruitment remains melee
- **WHEN** an otherwise valid current-protocol recruitment request omits its soldier type
- **THEN** it creates one Swordsman for the configured melee cost

#### Scenario: Recruit specialist units
- **WHEN** a player recruits a Crossbowman from an Archery Range or a Mage from an Arcanum with all required resources
- **THEN** exactly one unit of the selected archetype is created and its displayed costs are deducted once
- **AND** a retry cannot create another unit or charge

### Requirement: Simple building upgrades
Each building SHALL have two levels. A player SHALL be able to spend the displayed gold and wood to upgrade their own level-one building during unpaused building or preparation while alive and not ready. Level-two mines, farms and lumbermills SHALL produce more of their respective resources; level-two recruitment buildings SHALL reduce their food recruitment cost while retaining positive costs. Tower upgrades SHALL improve their configured attack profile. Blacksmith level two SHALL unlock the second research rank without granting it automatically. Buildings SHALL retain their upgrades between turns and waves. A maximum-level or unaffordable upgrade SHALL fail without spending resources.

#### Scenario: Upgrade a farm
- **WHEN** a player pays for a level-two farm before resolving the turn
- **THEN** that turn's production uses the increased food output and later turns retain the upgrade

#### Scenario: Upgrade a barracks
- **WHEN** a barracks has been upgraded and its owner manually recruits
- **THEN** one soldier is created for the reduced food cost
- **AND** existing soldiers are unchanged by the upgrade

#### Scenario: Upgrade a tower
- **WHEN** the player pays for a level-two Arrow Tower or Catapult Tower during an editable stage
- **THEN** later attacks use its displayed upgraded profile without changing another tower or granting free attacks

### Requirement: Ready checks and three-turn wave cadence
Each wave SHALL be preceded by exactly three building/production turns. Connected living players SHALL explicitly set ready; they SHALL be able to withdraw readiness before resolution. Ready players SHALL NOT construct, upgrade, research, or recruit until they withdraw readiness. Disconnected and eliminated players SHALL NOT block the ready check. A building turn SHALL resolve only when at least one living player is connected and every connected living player is ready. Resolution SHALL apply production exactly once to every living city, including disconnected cities, and reset readiness. After the third production turn, the match SHALL enter an editable preparation stage with readiness cleared. Preparation SHALL permit normal owned economic actions without production. A second ready check following the same connected/living eligibility SHALL start combat once any prior wave's authoritative death cleanup is complete and SHALL NOT grant income or create a fourth production turn. Readiness during cleanup SHALL remain recorded; cleanup completion SHALL resolve the eligible check once without requiring a repeated ready command. Snapshots SHALL distinguish preparation from production stages and commands from prior stages SHALL be stale.

#### Scenario: Third turn starts a wave
- **WHEN** all required players become ready for the third building turn
- **THEN** production occurs once and final preparation starts
- **AND** a ready request from the resolved production stage is rejected and cannot produce resources again or start combat
- **AND** the wave starts only when the subsequent preparation ready check resolves

#### Scenario: Disconnected city participates
- **WHEN** one player is disconnected and the remaining connected living players finish a ready check
- **THEN** all living cities receive production and the match advances without requiring input from the disconnected player

#### Scenario: Nobody is connected and alive
- **WHEN** a building phase has no connected living player
- **THEN** it waits without automatically resolving empty ready checks

#### Scenario: Spend the last production before battle
- **WHEN** the third production of wave three grants food and wood
- **THEN** the player can recruit, construct, upgrade or research with them in preparation
- **AND** completing the preparation ready check starts that wave without further income

#### Scenario: Preparation survives pause and reconnect
- **WHEN** a city reconnects during paused preparation
- **THEN** it sees current resources, readiness and preparation state, with economic actions disabled until resume

#### Scenario: Ready while previous deaths finish
- **WHEN** players finish the next preparation ready check before the previous wave's death reservations expire
- **THEN** their readiness remains set and no extra income or battle is created
- **AND** the next wave begins once cleanup completes if the ordinary ready eligibility still holds

### Requirement: Automatic battles and city defense
Each city SHALL have an automatic bounded hex battlefield with Swordsman, Berserker, Crossbowman and Mage archetypes on both factions, classified as melee, ranged and magic. Equal archetype and research rank SHALL have identical combat statistics and attack rules across factions. Enemies SHALL use skeleton presentation without hidden faction stat bonuses. Each wave SHALL have a deterministic configured composition, progressing from melee to mixed threats while preserving original-roster allocation and redistribution. Melee units SHALL approach melee range; ranged and magic units SHALL approach their longer attack range and hold while a valid target remains in range. Each type SHALL use its own authoritative health, damage, capacity footprint, initiative, integer hex range and move/windup/recovery/death durations from the frozen combat configuration. Melee SHALL attack distinct adjacent hexes at range one. Ready units SHALL choose the closest in-range opponent, then lowest initiative and a seeded tie; units without an in-range target SHALL approach a usable attack position or wait when blocked. Units SHALL hold through attack windup and recovery and SHALL NOT automatically retreat when reached in melee. Crossbow hits SHALL be single-target attacks with no friendly fire; their cosmetic projectile SHALL NOT require physical collision or impart knockback. Mage splash SHALL obey the bounded combat-archetypes contract. Soldiers SHALL fight without player orders. While alive, each city's built-in defender SHALL repeatedly attack living enemies assigned to that city at low damage even if its army has been wiped out. Enemies without a living deployed defending soldier SHALL approach the declared city-defense anchor and attack city health using their own range and attack timing. Legal siege positions SHALL keep city health reachable without requiring occupation of protected allied deployment cells. Reinforcements arriving after that city cleared its enemies SHALL use guaranteed protected entry and reach engagement within the declared admission bound even when defenders hold neutral enemy-forward cells. City impacts SHALL revalidate that exposure and destination; an invalid city primary SHALL miss without retargeting. Queued and dying units SHALL NOT attack or be selected. Army loss alone SHALL NOT eliminate the city. Building, recruitment, research and upgrading SHALL be unavailable during combat. Buildable towers SHALL supplement the built-in defender without occupying combat capacity and SHALL stop attacking when their city falls. City health SHALL carry between waves without automatic healing.

#### Scenario: Exposed city fights back
- **WHEN** the last soldier dies while enemies remain
- **THEN** the city remains alive while its health is positive, enemies attack it, and its built-in defender continues damaging those enemies

#### Scenario: Remaining army needs no control
- **WHEN** a wave begins with soldiers present
- **THEN** they fight automatically and the server determines damage, deaths, and the battle result

#### Scenario: Ranged support fires behind melee contact
- **WHEN** a mixed army meets enemies with a valid shooting target beyond melee reach
- **THEN** Crossbowmen stop at shooting range and attack while Swordsmen advance to melee contact
- **AND** each hit resolves at its authoritative impact tick without requiring player orders or visual projectile collision

#### Scenario: Ranged soldier is reached
- **WHEN** an enemy reaches a Crossbowman after the melee screen is lost
- **THEN** the enemy can damage that soldier at melee range and the Crossbowman continues using its shooting profile without passing through the enemy

#### Scenario: Transferred specialist retains its role
- **WHEN** an enemy Mage or Crossbowman transfers after a city falls
- **THEN** it retains identity, archetype, rank, health, origin and remaining attack recovery
- **AND** its old destination windup is cancelled and no destination research changes its profile

#### Scenario: Queued defender arrives during city windup
- **WHEN** an enemy starts attacking an exposed city and a living defending soldier deploys before impact
- **THEN** exposure validation makes that locked city attack miss with normal recovery
- **AND** a later ready decision can select the deployed soldier

#### Scenario: Reinforce a previously cleared city
- **WHEN** a city cleared its allocated enemies, its surviving defenders stand at the neutral enemy-forward band, and living enemies transfer from another fallen city
- **THEN** assignment is immediate, protected rear cells supply admission within their release bound and actual attacks resume
- **AND** identities, health and remaining recovery are conserved and the check fails if progress depends on BattleStalled

### Requirement: Elimination and immediate enemy redistribution
A city SHALL be eliminated when its health reaches zero. Its defender SHALL stop, its owner SHALL lose building/recruitment/readiness actions, and its surviving attackers SHALL immediately be divided among all remaining living cities, including disconnected ones. Transferred enemies SHALL retain their identity and remaining health, with no duplication or revival of killed enemies. Integer remainders SHALL be assigned in stable player order, with allocations differing by at most one. All cities eliminated in one combat step SHALL be excluded before that step's redistribution. A city that cleared its own enemies SHALL receive redistributed enemies while the shared wave remains active.

#### Scenario: Transfer living attackers
- **WHEN** one city falls with five enemies alive and two other cities remain
- **THEN** the remaining cities receive three and two of those enemies in stable order immediately
- **AND** each transferred enemy retains its health and is present exactly once

#### Scenario: Multiple cities fall together
- **WHEN** two cities fall in the same combat step and one city remains
- **THEN** every surviving attacker from both fallen cities is assigned to the remaining city
- **AND** none is assigned to either eliminated city

### Requirement: Future allocations and three-wave outcome
Each of three escalating waves SHALL contain a configured baseline enemy allocation for every original roster member. A living member SHALL receive their own allocation; allocations of eliminated members SHALL be split among the remaining living cities in stable order with balanced integer remainders. Original allocations SHALL be counted once regardless of how many eliminations have occurred. The match SHALL enter shared victory once wave three has no living enemies and at least one city remains. It SHALL enter defeat as soon as no living city remains, taking precedence over simultaneous enemy deaths, with an all-cities-fallen reason. If living enemies remain after ordinary health/outcome resolution and an active engagement's no-progress limit or the wave's maximum combat duration expires, the whole match SHALL instead enter defeat with a battle-stalled reason, identifying the triggering city when applicable, limit and tick. Protected deployment SHALL handle ordinary reinforcement admission independently of this fallback. Battle-stalled defeat SHALL NOT change surviving health, mark living cities eliminated, award victory or redistribute fabricated casualties. Ordinary wave completion on the deadline tick SHALL take precedence over a limit, while ordinary health defeat SHALL retain precedence over victory. No fourth wave, further economic turns or combat actions SHALL run after an outcome. Dying bodies SHALL NOT delay the ordinary gameplay outcome, and their bounded authoritative cleanup SHALL continue without new combat actions; a nonterminal next wave SHALL NOT reuse reserved positions before cleanup completes. Eliminated connected players SHALL be able to observe the rest of the match.

#### Scenario: Fallen city retains future wave pressure
- **WHEN** one of three original cities has fallen and the next wave allocates six enemies per original city
- **THEN** the two surviving cities receive nine enemies each for a total of eighteen

#### Scenario: Complete the third wave
- **WHEN** the third wave's final enemy dies and a city survives
- **THEN** all clients receive a shared victory outcome and combat actions and match progression stop
- **AND** unexpired deaths finish their bounded authoritative cleanup without changing the outcome

#### Scenario: Last city falls
- **WHEN** the last living city reaches zero health
- **THEN** the match becomes defeat without attempting to divide enemies among an empty set of survivors

#### Scenario: Stalled battle ends the match
- **WHEN** a declared combat limit expires with living enemies and living cities after that tick's normal result checks
- **THEN** every player observes defeat with a battle-stalled reason and the same diagnostic limit/tick
- **AND** current city/unit health is preserved and no later wave or economic turn runs

### Requirement: Changed-state snapshot publication
The authority SHALL publish complete changed snapshots at the bounded combat cadence and promptly for pause or non-combat revisions. It SHALL NOT continually enqueue unchanged paused/building snapshots on the reliable channel. Welcome and command acknowledgment delivery SHALL still carry the complete current state, and current death cleanup SHALL remain observable as it changes authoritative revisions.

#### Scenario: Resume after a long shared pause
- **WHEN** clients inspect a paused battle and an eligible player resumes then pauses at a later action tick
- **THEN** the graphical and headless peers receive the later paused revision and agree on its authority tick
- **AND** repeated copies of the prior unchanged pause do not delay the revision behind a growing reliable snapshot queue
