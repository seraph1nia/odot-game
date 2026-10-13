# Spec Delta

## MODIFIED Requirements

### Requirement: Fixed roster and personal cities

The lobby SHALL support one through four players and an explicit start locking its roster. Only the original playing host SHALL start its hosted lobby; a dedicated lobby SHALL retain connected-member start permissions. Single player SHALL start exactly one local city without network admission. Each owner SHALL retain nine stable settlement slots, five initially purchased and four locked, its own economy/research, positive Keep health and one built-in defender represented by a Watchpost. All original owners SHALL share one connected combat arena with adjacent sectors; settlement slots SHALL not become combat capacity. Scenery SHALL grant no topology/economy bonuses. New players SHALL be refused after start; roster members SHALL reconnect to the same city.

#### Scenario: Shared roster
- **WHEN** a hosted two-player lobby starts
- **THEN** two independent cities and adjacent combat sectors are created with equal configured starting grants and one locked roster

#### Scenario: Fresh join after start
- **WHEN** a connection lacks valid original-roster credentials after start
- **THEN** it is refused without creating a city, sector or enemy allocation

#### Scenario: Start a cooperative match
- **WHEN** the original host of a hosted lobby, or a connected member of a dedicated-server lobby, starts a match with two players present
- **THEN** both players receive distinct cities with five usable slots and four locked plots and the same configured starting resources and city health
- **AND** the first building turn starts with the roster locked

#### Scenario: New arrival after start
- **WHEN** a client without valid credentials for a roster member joins after the match starts
- **THEN** it receives a clear refusal and no new city or wave allocation is created

#### Scenario: Resume a city with hexagonal plots
- **WHEN** a roster member resumes a city containing buildings and upgrades
- **THEN** all nine authoritative slot identities retain their purchase status, buildings, levels and refundable investment at the corresponding graphical plots
- **AND** surrounding decorative terrain provides no extra building slots or gameplay effects

#### Scenario: Solo selection starts one city
- **WHEN** a player selects Single player from the start screen
- **THEN** one local city enters the first building turn with the same configured rules and starting grant as a cooperative city
- **AND** no guest connection can add a city to that match

#### Scenario: Locked land is not an empty building slot
- **WHEN** a player attempts to construct on a locked plot
- **THEN** construction fails without spending resources even if that player can afford the building
- **AND** inspecting or buying that plot retains its stable world identity

### Requirement: Manual recruitment and persistent soldiers

A living connected unready owner SHALL pay the complete per-member material/gold quote at the selected owned trainer during unpaused editable stages. Barracks SHALL offer Swordsman/Berserker, Archery Range Crossbowman and Arcanum Mage; omitted type SHALL default to Swordsman and unknown/mismatched types SHALL refuse atomically. Trainer level SHALL determine only the new recruit's equipment level/profile/quote. Swordsman/Berserker SHALL require metal, Crossbowman metal/wood and Mage cloth/gold; food SHALL be separate battle upkeep. Buildings/production SHALL grant no free recruits, and gold SHALL not silently replace materials. Capacity/type/home SHALL be preflighted under army-roster before spending; reinforcement/new-squad choice SHALL be explicit. Veterans SHALL retain IDs, levels, injuries and experience; replacements SHALL begin at zero experience and not heal veterans. Every surviving funded field participant SHALL earn one service point on successful shared clear, including current-wave fallen-owner fighters; storage/unfunded reserves SHALL earn none. Service SHALL not automatically upgrade equipment.

#### Scenario: Higher-level reinforcement
- **WHEN** an eligible owner recruits into a compatible partial squad from a level-two trainer
- **THEN** exactly one level-two member is paid/added and existing veterans retain identity, wounds and equipment

#### Scenario: Retry
- **WHEN** an accepted reinforcement is retried
- **THEN** no second member, payment or experience is created

#### Scenario: Click a barracks to recruit
- **WHEN** a player activates their barracks with all required materials
- **THEN** the quoted recruitment resources are deducted once without spending food and one soldier is added
- **AND** passing a later production turn alone does not recruit another soldier

#### Scenario: Missing barracks or food
- **WHEN** a player requests recruitment from a mismatched building slot or without sufficient required materials
- **THEN** neither resources nor army state changes
- **AND** missing food alone does not reject an otherwise affordable material recruitment; the resulting army remains subject to battle upkeep

#### Scenario: Army survives a battle
- **WHEN** a wave ends with some soldiers alive and some killed
- **THEN** only the surviving soldiers remain for the next building phase with their remaining health

#### Scenario: Recruit each weapon type
- **WHEN** a player with all required materials explicitly recruits a Swordsman from their Barracks and a Crossbowman from their Archery Range
- **THEN** one unit of each selected type is created with its configured combat profile and each request spends its complete displayed recruitment cost once
- **AND** each recruitment building's upgrade raises the level and displayed price of future recruits

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

#### Scenario: Recruit beside a veteran
- **WHEN** a city upgrades its Barracks from level one to level two and recruits a Swordsman
- **THEN** the new Swordsman has level-two health and damage and costs the displayed level-two price
- **AND** a surviving level-one Swordsman retains its identity, level and current health

#### Scenario: Recruitment levels belong to individual buildings
- **WHEN** a city owns level-one and level-three Barracks
- **THEN** selecting each recruits at that building's level and price
- **AND** the city does not receive a global level or discount from either building

### Requirement: Automatic battles and city defense

All sectors SHALL use one squad-based automatic battle, with equal resolved archetype profiles across factions and authored per-member wave type/level/count/boss inputs. Members SHALL not independently route or choose target groups. Squads SHALL engage groups in physical range, otherwise approach legal reachable groups/objectives, and hold through committed windup/recovery. Crossbowman SHALL remain X-only and continue its ranged profile when reached in melee; Mage SHALL use the within-group X/Y contract. Keep/Watchpost attacks SHALL depend on physical range and structural profile, not the absence of every defending soldier. Watchposts SHALL be the existing defender's physical origin, reset to configured full HP for living owners each wave, and stop that weapon when destroyed. Destruction SHALL apply one nonstacking owning-participant-squad Rally disruption per wave: default 20 percent less direct damage for 240 ticks, without movement/attack disablement or resource theft. Keep HP SHALL persist between waves. Purchased towers SHALL operate physically and stop on Keep loss. Combat SHALL forbid development/army editing while allowing shared pause and local inspection/navigation.

#### Scenario: Keep exposed through a flank
- **WHEN** an enemy reaches valid Keep range while friendly squads remain elsewhere
- **THEN** its normal committed structure attack can damage the Keep without a global army shield

#### Scenario: Watchpost breaks
- **WHEN** a Watchpost reaches zero during a wave
- **THEN** its defender stops and only that owner's current participating squads receive one bounded direct-damage disruption
- **AND** a surviving owner's next wave restores the Watchpost and clears disruption

#### Scenario: Exposed city fights back
- **WHEN** the last soldier dies while enemies remain and its physical Watchpost is still alive
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
- **WHEN** a living enemy Mage or Crossbowman group redirects after Keep loss
- **THEN** type, level, origin, member HP, status and committed timing remain unchanged
- **AND** no destination technologies, teleportation or fresh target shot are introduced

#### Scenario: Queued defender arrives during city windup
- **WHEN** a friendly squad enters range of an enemy already winding up on a Keep
- **THEN** the committed Keep attack validates physical target/range rather than a global defender-exposure shield
- **AND** the next eligible target selection considers the new nearby squad

#### Scenario: Reinforce a previously cleared city
- **WHEN** a neighboring Keep falls while the shared battle remains active
- **THEN** living redirected enemies approach from their actual cells and friendly squads may intercept
- **AND** identities and health remain conserved and progress does not depend on corpse transfer admission or BattleStalled

### Requirement: Elimination and immediate enemy redistribution

Zero Keep HP SHALL eliminate its owner, stop weapons/future development, deny city/army/readiness requests and retain the fallen roster for inspection. Current participating survivors SHALL continue fighting to the current battle's result. Surviving enemy squads preferring fallen sectors SHALL redirect toward living owners in deterministic balanced group allocation after simultaneous eliminations are excluded. Redirection SHALL preserve member count/origin/HP/profile, physical source, current transition and committed timing; it SHALL not teleport, duplicate, heal or cancel a move solely to rebuild city-local admission. Connected fallen owners SHALL spectate and retain shared pause/resume. After the battle, their survivors SHALL receive no future deployment/upkeep/production/healing.

#### Scenario: Active fallen-owner survivor
- **WHEN** a Keep falls with an owned field squad alive and another Keep survives
- **THEN** the squad can land subsequent current-wave impacts while owner spending/readiness/weapons are disabled

#### Scenario: Several Keeps fall together
- **WHEN** two Keeps fall on the same tick
- **THEN** neither is selected as a redirect destination and every surviving enemy group/member remains present exactly once

#### Scenario: Transfer living attackers
- **WHEN** a Keep falls with living enemy groups and two other Keeps remain
- **THEN** groups redirect preferences in stable balanced group allocation with every member retained exactly once
- **AND** different partial-group populations need not yield equal member counts and actors are not teleported

#### Scenario: Multiple cities fall together
- **WHEN** two Keeps fall on the same combat tick and one remains
- **THEN** all surviving enemy groups preferring either fallen owner redirect to the living owner
- **AND** no eliminated owner is selected and member identities/HP remain intact

### Requirement: Future allocations and twenty-wave outcome

Each of twenty authored waves SHALL retain one baseline allocation per original roster member. Fallen-owner future allocations SHALL be split among living preferred sectors with stable balanced integer remainders and unchanged original counts, including disconnected living owners. A wave SHALL clear only with no living/queued enemies and at least one Keep alive. Wave twenty SHALL yield shared victory; no living Keep SHALL yield AllCitiesFallen defeat before simultaneous clear. Existing positive no-health-progress/wave limits SHALL yield explicit BattleStalled only after ordinary results, without fabricated damage/casualties/rewards. No wave twenty-one, later development or new combat actions SHALL run after an outcome. Dying bodies SHALL not delay that outcome; bounded cosmetic cleanup SHALL continue, and next formation SHALL wait for cleanup. Fallen connected players SHALL remain observers.

#### Scenario: Future pressure preserved
- **WHEN** one of three original owners has fallen before a six-enemy-per-origin wave
- **THEN** the two living preferred sectors receive nine original members each, eighteen total

#### Scenario: Last enemy and Keep
- **WHEN** the last enemy and last Keep die together
- **THEN** defeat takes precedence and no clear reward is granted

#### Scenario: Final clear
- **WHEN** the wave-twenty final enemy dies with a surviving Keep
- **THEN** final rewards precede victory and only bounded cosmetic cleanup may continue

#### Scenario: Fallen city retains future wave pressure
- **WHEN** one of three original cities has fallen and the next wave allocates six enemies per original city
- **THEN** the two surviving cities receive nine enemies each for a total of eighteen

#### Scenario: Complete the twentieth wave
- **WHEN** the twentieth wave's final enemy dies and a city survives
- **THEN** all clients receive a shared victory outcome and combat actions and match progression stop
- **AND** unexpired deaths finish their bounded authoritative cleanup without changing the outcome

#### Scenario: Last city falls
- **WHEN** the last living city reaches zero health
- **THEN** the match becomes defeat without attempting to divide enemies among an empty set of survivors

#### Scenario: Stalled battle ends the match
- **WHEN** a declared combat limit expires with living enemies and living cities after that tick's normal result checks
- **THEN** every player observes defeat with a battle-stalled reason and the same diagnostic limit/tick
- **AND** current city/unit health is preserved and no later wave or economic turn runs

#### Scenario: Complete the third wave
- **WHEN** wave three clears with a living city
- **THEN** the shared clear reward is granted and wave four begins its first building turn

#### Scenario: Boss pressure includes fallen cities
- **WHEN** a three-owner original roster reaches wave10 with two living Keeps
- **THEN** three original single-member boss squads are assigned among living preferred sectors by stable remainders
- **AND** each occupies its own physical cell and preserves the ordinary actor architecture

### Requirement: Once-per-battle food upkeep and inactive reserves

Food SHALL be paid once per living individual owned by a living city at actual final settlement/combat entry, including disconnected living cities and stored soldiers, after prior cosmetic cleanup. Default per-type upkeep SHALL remain positive and independent of equipment/research. Field members SHALL be considered first, then stored, each ordered by descending level and stable ID; unaffordable members SHALL be skipped without debt/starvation and cheaper members considered. Only funded field members SHALL participate; a zero-funded squad SHALL have no occupancy. Towers/defender/enemies/bosses/dead members SHALL owe no city food. Shortage SHALL not block battle. Stored/unfunded members SHALL retain HP/identity without targets, claims, attacks or shielding. Redirected enemies and continuing fallen-owner fighters SHALL not be charged again. Fallen owners SHALL owe no future upkeep. Forecasts SHALL match actual final-settlement funding and expose field/stored demand and funded/unfunded/participant identities; retries, reconnects, pause and cleanup SHALL not charge again.

#### Scenario: Partial squad
- **WHEN** final settlement funds only two of three field members
- **THEN** one squad cell contains two participants and the unfunded member remains a living reserve

#### Scenario: Storage is not combat
- **WHEN** a stored veteran is funded at battle entry
- **THEN** its payment is recorded for recovery but it receives no deployment or claim

#### Scenario: Pay for the whole army once
- **WHEN** preparation completes with enough food for every living soldier
- **THEN** the city pays exactly its summed upkeep once and its funded field-assigned soldiers participate, including any waiting for formation capacity
- **AND** funded stored soldiers remain noncombatants with their paid identities retained for completion-based recovery
- **AND** subsequent admission, repeated ready requests and reconnect do not deduct food again

#### Scenario: A shortage feeds stronger recruits first
- **WHEN** a city has two food, a field-assigned level-three Mage owing two, and a field-assigned level-one Swordsman owing one
- **THEN** the Mage participates, two food are deducted and the Swordsman sits out with unchanged identity, level and health
- **AND** the wave starts without requiring the city to buy more food

#### Scenario: Skip an unaffordable unit and feed a cheaper one
- **WHEN** that same city has only one food
- **THEN** the Mage sits out, the Swordsman participates and one food is deducted
- **AND** equal-level ties within each assignment group use stable identity and produce the same allocation under reversed enumeration order

#### Scenario: No fed defenders still permits siege
- **WHEN** a city's entire living army is unfed at battle entry
- **THEN** those reserves occupy no combat space and cannot prevent normal enemies from attacking city health
- **AND** its built-in defender and towers still act under ordinary rules

#### Scenario: Disconnected city has insufficient food
- **WHEN** connected living players finish preparation while a disconnected living city cannot feed its whole army
- **THEN** that city follows the same deterministic payment/reserve rule and does not block combat
- **AND** reconnect restores its actual receipt and unchanged reserve identities without another payment

#### Scenario: Prior death cleanup postpones payment
- **WHEN** final readiness occurs while prior cosmetic death cleanup remains incomplete
- **THEN** food remains unspent until cleanup ends and normal eligibility still permits actual combat entry
- **AND** logical claims were already released and cleanup still cannot create another settlement

#### Scenario: Reserves can return in the next wave
- **WHEN** an unfed soldier's living city later has enough food at the following battle entry
- **THEN** that same soldier can participate at its retained level and health after its upkeep is paid
- **AND** it was neither healed, killed nor recruited again while sitting out

### Requirement: Simple boss rounds

Waves ten and twenty SHALL each contain one single-member boss squad per original roster allocation, with no ordinary escorts. Default bosses SHALL remain level-three and level-five Swordsmen with eight times level-scaled HP and twice direct damage. Bosses SHALL use ordinary squad movement, targeting, range, attack/death rules and one exclusive cell, without special phases, summons or immunity. Redirection SHALL retain identity, health, level and modifiers without teleportation/reapplication. Wave ten SHALL continue and wave twenty SHALL yield victory only with a living Keep.

#### Scenario: Boss architecture
- **WHEN** a solo match enters wave ten
- **THEN** one level-three Swordsman member uses the ordinary squad cycle and exclusive cell with declared boss modifiers

#### Scenario: Solo boss wave
- **WHEN** a solo match enters wave ten
- **THEN** exactly one level-three Swordsman boss and no escorts are allocated
- **AND** it fights using ordinary combat actions with the configured increased health and damage

#### Scenario: Transfer a wounded boss
- **WHEN** a wounded boss's preferred Keep falls
- **THEN** it redirects preference with the same member/group identity, pose, health and modifiers

### Requirement: Ready checks and preparation wave cadence
Readiness eligibility, numbered stages, settlement, the final spending boundary and death-cleanup deferral SHALL follow `docs/gameplay.md#preparation-and-battle-controls` (preparation and battle controls). Snapshots SHALL preserve the serialized phase meanings and commands from prior stages SHALL be stale, under `docs/sessions.md` (session compatibility).

#### Scenario: Final readiness starts a wave
- **WHEN** all required players become ready in final preparation
- **THEN** settlement and combat entry follow the authoritative gameplay boundary
- **AND** retrying or delivering a stale ready request cannot repeat settlement or upkeep

#### Scenario: Disconnected city participates
- **WHEN** one player is disconnected and the remaining connected living players finish a ready check
- **THEN** all living cities receive production and the match advances without requiring input from the disconnected player

#### Scenario: Nobody is connected and alive
- **WHEN** a building phase has no connected living player
- **THEN** it waits without automatically resolving empty ready checks

#### Scenario: Final edits precede settlement
- **WHEN** a player makes eligible economic or army edits during final preparation
- **THEN** they follow the spending and settlement boundary in `docs/gameplay.md#preparation-and-battle-controls` (gameplay controls)
- **AND** no subsequent income-spending stage is exposed before combat

#### Scenario: Preparation survives pause and reconnect
- **WHEN** a city reconnects during paused preparation
- **THEN** it sees current resources, readiness and preparation state, with economic actions disabled until resume

#### Scenario: Ready while previous deaths finish
- **WHEN** players finish the next preparation ready check before the previous wave's cosmetic death cleanup completes
- **THEN** their readiness remains set and no extra income, upkeep payment or battle is created
- **AND** the next wave begins once cleanup completes if the ordinary ready eligibility still holds
