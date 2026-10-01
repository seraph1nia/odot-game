# Spec Delta

## MODIFIED Requirements

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
