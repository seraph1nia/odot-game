# coop-city-match Specification

## Purpose

Provide a small cooperative city-defense match whose economic choices and automatic battles exercise authoritative multiplayer with a persistent state.

## Requirements

### Requirement: Fixed roster and personal cities
The game SHALL offer a lobby for up to four players and an explicit start action that locks the current roster. Each roster member SHALL begin with an independent square 3x3 board of nine empty building slots, positive city health, starting gold, and a built-in ranged defender outside those slots. One player SHALL be sufficient for local testing; the match SHALL support at least two simultaneous players. Fresh players SHALL NOT join an already started match, but roster members SHALL be able to reconnect.

#### Scenario: Start a cooperative match
- **WHEN** a connected lobby member starts a match with two players present
- **THEN** both players receive distinct cities with nine available slots and the same configured starting resources and city health
- **AND** the first building turn starts with the roster locked

#### Scenario: New arrival after start
- **WHEN** a client without valid credentials for a roster member joins after the match starts
- **THEN** it receives a clear refusal and no new city or wave allocation is created

### Requirement: Gold and food economy
Each living city SHALL receive the configured base gold income once per production turn. Mines SHALL add gold and farms SHALL add food during that production. Mines, farms, and barracks SHALL each cost gold to construct and occupy exactly one empty slot on the requesting player's board. Construction SHALL be allowed only during an unpaused building phase for a living player who is not ready. Buildings SHALL produce in the turn they are purchased. Resources SHALL NOT become negative; wood, stone, and iron SHALL NOT be spendable resources in this POC.

#### Scenario: Construct and produce
- **WHEN** a player buys a mine and a farm in empty slots and the ready check resolves
- **THEN** their gold purchase costs are deducted once and they receive base gold plus mine output and farm food once
- **AND** another player's resources and buildings are unchanged by the purchases

#### Scenario: Invalid construction
- **WHEN** a player requests an occupied or out-of-range slot, an unknown building, an unaffordable purchase, or construction outside the permitted phase
- **THEN** the request fails with an explanation and leaves authoritative resources and buildings unchanged

### Requirement: Manual recruitment and persistent soldiers
A player SHALL recruit one soldier by explicitly activating one of their own barracks during an unpaused building phase while alive and not ready. Each successful recruitment SHALL deduct the barracks' configured food cost and immediately add a soldier to that city's army. Barracks SHALL NOT automatically recruit during production, and gold SHALL NOT substitute for food. Living soldiers SHALL retain their identities and remaining health between waves; dead soldiers SHALL NOT return automatically.

#### Scenario: Click a barracks to recruit
- **WHEN** a player activates their barracks with enough food
- **THEN** the food cost is deducted once and one soldier is added
- **AND** passing a later production turn alone does not recruit another soldier

#### Scenario: Missing barracks or food
- **WHEN** a player requests recruitment from a non-barracks slot or without sufficient food
- **THEN** neither resources nor army state changes

#### Scenario: Army survives a battle
- **WHEN** a wave ends with some soldiers alive and some killed
- **THEN** only the surviving soldiers remain for the next building phase with their remaining health

### Requirement: Simple building upgrades
Each building SHALL have two levels. A player SHALL be able to spend gold to upgrade their own level-one building during an unpaused building phase while alive and not ready. Level-two mines and farms SHALL produce more of their respective resources; a level-two barracks SHALL reduce the food cost of manual recruitment. Buildings SHALL retain their upgrades between turns and waves. A maximum-level or unaffordable upgrade SHALL fail without spending resources.

#### Scenario: Upgrade a farm
- **WHEN** a player pays for a level-two farm before resolving the turn
- **THEN** that turn's production uses the increased food output and later turns retain the upgrade

#### Scenario: Upgrade a barracks
- **WHEN** a barracks has been upgraded and its owner manually recruits
- **THEN** one soldier is created for the reduced food cost
- **AND** existing soldiers are unchanged by the upgrade

### Requirement: Ready checks and three-turn wave cadence
Each wave SHALL be preceded by exactly three building/production turns. Connected living players SHALL explicitly set ready; they SHALL be able to withdraw readiness before resolution. Ready players SHALL NOT construct, upgrade, or recruit until they withdraw readiness. Disconnected and eliminated players SHALL NOT block the ready check. A building turn SHALL resolve only when at least one living player is connected and every connected living player is ready. Resolution SHALL apply production exactly once to every living city, including disconnected cities, and reset readiness. After the third production turn, the wave SHALL begin automatically with no extra building phase before combat.

#### Scenario: Third turn starts a wave
- **WHEN** all required players become ready for the third building turn
- **THEN** production occurs once and the next wave starts
- **AND** repeat ready requests do not produce resources again or spawn another wave

#### Scenario: Disconnected city participates
- **WHEN** one player is disconnected and the remaining connected living players finish a ready check
- **THEN** all living cities receive production and the match advances without requiring input from the disconnected player

#### Scenario: Nobody is connected and alive
- **WHEN** a building phase has no connected living player
- **THEN** it waits without automatically resolving empty ready checks

### Requirement: Automatic battles and city defense
Each city SHALL have an automatic battlefield with one soldier type and one enemy type. Soldiers SHALL fight without player orders. While alive, each city's built-in defender SHALL repeatedly attack living enemies assigned to that city at low damage even if its army has been wiped out. Enemies without a defending soldier SHALL attack city health. Army loss alone SHALL NOT eliminate the city. Building, recruitment, and upgrading SHALL be unavailable during combat. City health SHALL carry between waves without automatic healing.

#### Scenario: Exposed city fights back
- **WHEN** the last soldier dies while enemies remain
- **THEN** the city remains alive while its health is positive, enemies attack it, and its built-in defender continues damaging those enemies

#### Scenario: Remaining army needs no control
- **WHEN** a wave begins with soldiers present
- **THEN** they fight automatically and the server determines damage, deaths, and the battle result

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
Each of three escalating waves SHALL contain a configured baseline enemy allocation for every original roster member. A living member SHALL receive their own allocation; allocations of eliminated members SHALL be split among the remaining living cities in stable order with balanced integer remainders. Original allocations SHALL be counted once regardless of how many eliminations have occurred. The match SHALL enter shared victory once wave three has no living enemies and at least one city remains. It SHALL enter defeat as soon as no living city remains, taking precedence over simultaneous enemy deaths. No fourth wave or further economic turns SHALL run after an outcome. Eliminated connected players SHALL be able to observe the rest of the match.

#### Scenario: Fallen city retains future wave pressure
- **WHEN** one of three original cities has fallen and the next wave allocates six enemies per original city
- **THEN** the two surviving cities receive nine enemies each for a total of eighteen

#### Scenario: Complete the third wave
- **WHEN** the third wave's final enemy dies and a city survives
- **THEN** all clients receive a shared victory outcome and the match stops advancing

#### Scenario: Last city falls
- **WHEN** the last living city reaches zero health
- **THEN** the match becomes defeat without attempting to divide enemies among an empty set of survivors
