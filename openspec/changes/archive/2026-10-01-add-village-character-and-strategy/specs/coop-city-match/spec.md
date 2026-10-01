# Spec Delta

## MODIFIED Requirements

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
Each wave SHALL be preceded by exactly three building/production turns. Connected living players SHALL explicitly set ready; they SHALL be able to withdraw readiness before resolution. Ready players SHALL NOT construct, upgrade, research, or recruit until they withdraw readiness. Disconnected and eliminated players SHALL NOT block the ready check. A building turn SHALL resolve only when at least one living player is connected and every connected living player is ready. Resolution SHALL apply production exactly once to every living city, including disconnected cities, and reset readiness. After the third production turn, the match SHALL enter an editable preparation stage with readiness cleared. Preparation SHALL permit normal owned economic actions without production. A second ready check following the same connected/living eligibility SHALL start combat and SHALL NOT grant income or create a fourth production turn. Snapshots SHALL distinguish preparation from production stages and commands from prior stages SHALL be stale.

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


### Requirement: Automatic battles and city defense
Each city SHALL have an automatic battlefield with Swordsman, Berserker, Crossbowman and Mage archetypes on both factions, classified as melee, ranged and magic. Equal archetype and research rank SHALL have identical combat statistics and attack rules across factions. Enemies SHALL use skeleton presentation without hidden faction stat bonuses. Each wave SHALL have a deterministic configured composition, progressing from melee to mixed threats while preserving original-roster allocation and redistribution. Melee units SHALL approach melee range; ranged and magic units SHALL approach their longer attack range and hold while a valid target remains in range. Each type SHALL use its own authoritative health, damage, speed, range, windup and recovery profile. Crossbow hits SHALL be single-target attacks with no friendly fire; their cosmetic projectile SHALL NOT require physical collision or impart knockback. Mage splash SHALL obey the bounded combat-archetypes contract. Soldiers SHALL fight without player orders. While alive, each city's built-in defender SHALL repeatedly attack living enemies assigned to that city at low damage even if its army has been wiped out. Enemies without a defending soldier SHALL attack city health. Army loss alone SHALL NOT eliminate the city. Building, recruitment, research and upgrading SHALL be unavailable during combat. Buildable towers SHALL supplement the built-in defender without occupying combat-body space and SHALL stop attacking when their city falls. City health SHALL carry between waves without automatic healing.

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
