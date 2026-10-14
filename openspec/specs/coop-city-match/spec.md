# coop-city-match Specification

## Purpose

Provide a small cooperative city-defense match whose economic choices and automatic battles exercise authoritative multiplayer with a persistent state.

## Requirements

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

### Requirement: Material production and construction
Gold, food, wood, stone, metal and cloth SHALL be the six economy resources. Each living city SHALL receive configured base gold once per production turn. Gold Mines SHALL add gold, Farms food, Lumbermills wood, Stonecutters stone, Metal Mines metal and Weavers cloth. Producers SHALL create their output directly without consuming another resource. Gold Mine SHALL be the presentation name of the existing gold-producing Mine, distinct from Metal Mine. Construction SHALL atomically pay all displayed resource costs and occupy exactly one empty purchased owned plot during unpaused building or preparation for a living player who is not ready. Buildings present at production SHALL produce at their current level exactly once; sold producers SHALL contribute nothing. Settlement timing SHALL follow [preparation and battle controls](../../../docs/gameplay.md#preparation-and-battle-controls). Research Towers SHALL contribute city-research progress during those same actual production resolutions; research SHALL remain separate from the six material resources.

Lumbermills SHALL ordinarily cost 1 wood. Farms, Gold Mines, Stonecutters, Metal Mines, Weavers, Barracks and Archery Ranges SHALL cost 2 wood without gold. Arcanums SHALL cost 5 gold, 2 wood and 3 stone; Markets and Research Towers SHALL cost 2 wood and 2 stone without gold. Arrow Towers SHALL cost 4 gold and 3 wood; Catapult Towers SHALL cost 6 gold, 4 wood, 3 stone and 2 metal. These resource requirements SHALL NOT additionally require ownership of their producers, a base level, a talent, a wave milestone or a previous resource discovery. Arcanum construction SHALL NOT require cloth; recruiting a Mage SHALL require cloth. Costs, balances and arithmetic SHALL be nonnegative bounded whole amounts, and invalid or overflowing transactions SHALL fail without partial mutation. Starting grants, production and costs SHALL permit an ordinary opening that recruits and feeds defenders before wave one. Stone, metal and cloth SHALL begin at zero. At exactly zero wood, eligible owners SHALL additionally be able to explicitly select a quoted 4-gold recovery Lumbermill purchase on an empty purchased plot without owning a Market, including after selling a building to free space. A normal construction request SHALL NOT silently change its payment currency. Recovery SHALL be unavailable for other buildings, non-build actions, unknown payment choices or positive wood stock. Actual selected payment SHALL determine refundable investment; all normal ownership, phase, readiness, purchase, generation and retry guards SHALL apply.

The default city SHALL begin with 12 gold and 6 wood and zero other stocks, receive 2 base gold per production, and produce 1/2 gold, wood or stone at producer levels one/two, 4/8 metal or cloth, and 5/8 food. Food upkeep and food rewards SHALL retain their current units; research and combat values SHALL NOT be divided by the material scale. Caller-configured amounts SHALL be interpreted directly in the new units. Synchronized city state SHALL expose current projected per-resource production for the observed city, including base income and current producer levels, without mutating resources. Fallen and terminal cities SHALL expose zero projected income. If a custom configuration makes the total unrepresentable, the projection SHALL explicitly be unavailable rather than wrap or prevent snapshot publication; actual production SHALL retain atomic overflow rejection.

#### Scenario: Construct and produce
- **WHEN** a player buys a Gold Mine and Farm in purchased empty plots and the production ready check resolves
- **THEN** their displayed construction costs are deducted once and that city receives base gold, mine gold and farm food once
- **AND** another city's resources and buildings are unchanged by those purchases

#### Scenario: Invalid construction
- **WHEN** a request names a locked, occupied, foreign or out-of-range plot, an unknown building, an unaffordable purchase or an ineligible phase
- **THEN** it fails with an explanation and changes no resources, plot or building

#### Scenario: Wood cannot soft-lock construction
- **WHEN** a living city has no wood but can afford a Lumbermill and has or frees a purchased plot
- **THEN** it can explicitly select and build the recovery Lumbermill for 4 gold alone and receive wood at the next production

#### Scenario: Multi-resource purchase is atomic
- **WHEN** an Arcanum purchase has enough gold and wood but insufficient stone
- **THEN** it spends none of the resources and creates no building

#### Scenario: Build with stockpiled materials after selling a producer
- **WHEN** a city retains enough stone after selling its Stonecutter and buys an Arcanum
- **THEN** construction succeeds without a Stonecutter ownership check or any cloth charge
- **AND** later Mage recruitment still requires its displayed cloth cost

#### Scenario: New producers resolve once
- **WHEN** a living city owns a Stonecutter, Metal Mine and Weaver when a production turn resolves
- **THEN** their current-level stone, metal and cloth outputs are each added once without consuming gold, wood or food
- **AND** a disconnected living city follows the same production rule

#### Scenario: Basic opening does not spend gold
- **WHEN** a default city constructs its Farm, Metal Mine and Barracks for the first wave
- **THEN** construction spends its 6 starting wood and no gold
- **AND** spendable equipment, final settlement and upkeep follow the ordinary first-defense example in [gameplay](../../../docs/gameplay.md)

#### Scenario: Recovery charges only the selected quote
- **WHEN** an eligible zero-wood owner explicitly constructs a recovery Lumbermill and retries the accepted request
- **THEN** exactly 4 gold is paid once, its recorded investment is 4 gold, and a later eligible sale refunds 2 gold
- **AND** a standard build request with no wood or a recovery request with positive wood rejects without spending

#### Scenario: Income refreshes without speculative spending
- **WHEN** construction, upgrading or selling changes a living city's producer capacity
- **THEN** its synchronized projected production immediately matches the current producers and base income
- **AND** projecting, reconnecting or observing the value grants no resources

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

### Requirement: Simple building upgrades
Barracks, Archery Range and Arcanum SHALL each have five levels; Gold Mines, Farms, Lumbermills, Stonecutters, Metal Mines, Weavers, Research Towers, Arrow Towers and Catapult Towers SHALL have two levels; Markets SHALL have one level and no upgrade action. A player SHALL be able to spend the complete displayed resource quote to upgrade their own building by exactly one level up to its type-specific maximum during unpaused building or preparation while alive and not ready. Level-two producers SHALL produce more of their respective resource per plot; recruitment-building upgrades SHALL raise future recruits to the new building level and make their displayed total recruitment price nondecreasing, without food discounts. Positive recruitment cost components SHALL be rounded to nearest whole units with midpoint ties upward and remain positive; zero components SHALL remain zero. Prices SHALL be calculated from the archetype base cost and its configured growth curve, not from previously rounded prices. Default total prices SHALL increase at each level; custom positive small costs SHALL permit a nondecreasing total with rounding plateaus. Default level-one equipment costs SHALL be 2 metal for Swordsman, 3 metal for Berserker, 2 wood plus 1 metal for Crossbowman, and 3 cloth plus 1 gold for Mage. Tower upgrade paths and preserved legacy level upgrades SHALL follow the [gameplay tower contract](../../../docs/gameplay.md#defense-towers-on-army-plots). Research Tower level two SHALL contribute two research progress units per production instead of one; its level SHALL NOT gate technology purchases or grant a technology automatically. Barracks and Archery Range upgrades to level two SHALL cost 2 wood without gold; Arcanum upgrades to level two SHALL cost 4 gold and 2 wood. Recruitment-building upgrades from level two to three, three to four and four to five SHALL cost respectively 6 gold plus 2 wood plus 2 stone, 9 gold plus 2 wood plus 3 stone, and 14 gold plus 2 wood plus 4 stone. All basic producer upgrades SHALL cost 2 wood and 2 stone without gold. Research Tower upgrades SHALL cost 4 gold, 2 wood and 2 stone; tower quotes SHALL follow the same gameplay tower contract. Buildings SHALL retain their upgrades between turns and waves until sold. A maximum-level or unaffordable upgrade SHALL fail without spending resources.

#### Scenario: Upgrade a farm
- **WHEN** a player pays for a level-two farm before resolving the turn
- **THEN** that turn's production uses the increased food output and later turns retain the upgrade

#### Scenario: Upgrade a barracks
- **WHEN** a barracks has been upgraded and its owner manually recruits
- **THEN** one soldier is created at the building level for its increased displayed resource cost
- **AND** existing soldiers are unchanged by the upgrade

#### Scenario: Upgrade a tower
- **WHEN** the player pays for a level-two Arrow Tower or Catapult Tower during an editable stage
- **THEN** later attacks use its displayed upgraded profile without changing another tower or granting free attacks

#### Scenario: Maximum levels differ by building type
- **WHEN** a player requests another upgrade on a level-five Barracks or a level-two Farm
- **THEN** the request is rejected without changing resources or building state

#### Scenario: Costs are atomic and retries do not level twice
- **WHEN** an upgrade lacks one required resource or repeats an already accepted request
- **THEN** an unaffordable upgrade changes nothing and a retry causes no second charge or level increase

#### Scenario: Basic upgrades preserve early gold
- **WHEN** an eligible owner upgrades a basic producer or a level-one Barracks or Archery Range
- **THEN** its quoted wood and applicable stone are deducted with no gold charge
- **AND** higher recruitment levels and advanced upgrades retain their complete quoted gold costs

#### Scenario: Resolve small recruitment prices from the base
- **WHEN** default Swordsman recruitment is quoted at levels one through five
- **THEN** the metal prices are 2, 3, 4, 5 and 7, computed from the original 2-metal base
- **AND** every archetype retains zero food recruitment and its separate 1 or 2 food per-battle upkeep

Town hall SHALL use the independently quoted capacity and healing tracks in army-roster rather than a shared building-level upgrade.

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

### Requirement: Changed-state snapshot publication
The authority SHALL publish complete changed snapshots at the bounded combat cadence and promptly for pause or non-combat revisions. It SHALL NOT continually enqueue unchanged paused/building snapshots on the reliable channel. Welcome and command acknowledgment delivery SHALL still carry the complete current state, and current death cleanup SHALL remain observable as it changes authoritative revisions.

#### Scenario: Resume after a long shared pause
- **WHEN** clients inspect a paused battle and an eligible player resumes then pauses at a later action tick
- **THEN** the graphical and headless peers receive the later paused revision and agree on its authority tick
- **AND** repeated copies of the prior unchanged pause do not delay the revision behind a growing reliable snapshot queue

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

### Requirement: Gold-purchased plot expansion
A living, connected and unready owner SHALL be able to buy any one of their locked plots during unpaused building or preparation. Buying a plot SHALL pay the authoritative next expansion gold quote atomically and make that specific plot permanently usable for the match; it SHALL NOT construct a building. The default nine-plot board SHALL start with five purchased plots and offer four expansions at strictly increasing positive gold prices based on the number of previous purchases, not plot location. Default expansion prices SHALL be 5, 8, 12 and 18 gold. Plot purchases SHALL require no building, Market, resource other than gold or wave milestone. Already purchased, invalid, foreign or unaffordable requests SHALL fail without changes. Retries SHALL neither charge twice nor unlock another plot. Purchased land SHALL NOT be sold or refunded when its building is sold, and expansions SHALL NOT enlarge the combat board or grant terrain bonuses.

#### Scenario: Expand while every usable plot is full
- **WHEN** a city with all five starting slots occupied pays the next gold price on a locked plot
- **THEN** that plot becomes an empty sixth usable slot without replacing another building
- **AND** the next expansion has a higher displayed gold price

#### Scenario: Retain a purchased plot after a sale
- **WHEN** the building on a purchased expansion plot is sold
- **THEN** the plot remains usable and the next expansion price is unchanged

#### Scenario: Retry a plot purchase after reconnect
- **WHEN** a player retries an accepted plot purchase after reconnecting
- **THEN** the original selected plot remains purchased with one charge and no additional expansion

### Requirement: Building sales recover half the investment
An eligible owner SHALL be able to sell an occupied purchased plot's building during unpaused building or preparation while alive, connected and not ready, without owning a Market. The refund SHALL equal fifty percent of actual paid construction plus paid building-upgrade costs, summed then rounded down separately for each resource, returned in those original resources. Plot purchase prices, recruitment, research and Market trades SHALL NOT count as building investment. The complete refund SHALL be previewed and paid atomically with removing the building, its investment record and its building-specific production, recruitment, research production, trade access or tower state. The plot SHALL remain purchased. Existing soldiers SHALL retain identity, level, health and subsequent upkeep; purchased city technologies, banked research points and production progress SHALL persist without requiring a Research Tower to remain, and technology purchases SHALL remain available through independent research controls. No sale SHALL create production, free recruits or healing. A replacement building SHALL begin at level one with only its own new investment and no inherited tower action. Selling an empty, locked or foreign plot or selling while ineligible SHALL fail without changes. Duplicate requests SHALL NOT pay again or sell a replacement building.

#### Scenario: Refund an upgraded building
- **WHEN** construction and upgrades have actually paid a total of 55 gold, 25 wood and 15 stone and the building is sold
- **THEN** the city receives exactly 27 gold, 12 wood and 7 stone and the plot becomes empty but remains purchased
- **AND** prior plot, research and recruitment spending contributes nothing to the refund

#### Scenario: Sell a recruitment building
- **WHEN** a city sells its Barracks with living recruited soldiers
- **THEN** those soldiers keep their identities, individual levels and health and remain subject to future upkeep
- **AND** the city cannot recruit from that plot until a suitable new building is constructed

#### Scenario: Sell a producer before production
- **WHEN** a producer is sold during a building turn
- **THEN** the next production excludes that producer and the city retains its existing resource stockpile

#### Scenario: Retry after rebuilding the plot
- **WHEN** an accepted sale is retried after another building was placed on the same plot
- **THEN** the retry produces no further refund and leaves the new building and its investment intact

A Town hall storing living units SHALL reject sale without eviction or refund; an empty hall SHALL include both actual upgrade-track payments in its investment.

### Requirement: Market-gated resource sales
Owning at least one Market SHALL allow a living, connected, unready city to explicitly sell food, wood, stone, metal or cloth for gold during unpaused building or preparation. Each request SHALL select an owned Market, a known non-gold resource and a positive whole number of configured sale bundles. The frozen catalog SHALL declare a positive whole bundle size and positive gold return for each resource. Default sale bundles SHALL be 5 wood or stone for 1 gold, 5 metal or cloth for 2 gold, and 25 food for 1 gold, preserving economic exchange value when gold and materials shrink while food retains its units. The authority SHALL quote and atomically deduct the full resource amount and credit the corresponding gold; unknown resources, invalid quantities, insufficient stock, overflow, wrong ownership or an absent Market SHALL reject without changes. Duplicate commands SHALL NOT trade twice. Rates SHALL be fixed for the match, independent of wave, volume and number of Markets; Markets SHALL provide no production, automatic sales or upgrades. Buying resources with gold and trading between players SHALL be unavailable. Selling the last Market SHALL immediately remove access to resource sales without preventing building sales or plot purchases.

#### Scenario: Sell surplus cloth
- **WHEN** a city with a Market sells two cloth bundles at the displayed fixed quote
- **THEN** it loses exactly twice the cloth bundle size and gains exactly twice its gold return once
- **AND** other balances and another city's state are unchanged

#### Scenario: Market is required for resources but not buildings
- **WHEN** a city has no Market and attempts a resource sale followed by an otherwise eligible building sale
- **THEN** the resource sale is rejected without spending and the building sale succeeds at its displayed refund

#### Scenario: Selling the last Market disables trade
- **WHEN** a city's last Market is sold and a later resource-sale request arrives
- **THEN** the building refund has been paid once and the resource-sale request is rejected without changing stock

#### Scenario: Food exchange retains value in the new gold units
- **WHEN** an eligible default Market owner sells one food bundle
- **THEN** exactly 25 food is deducted and 1 gold credited atomically
- **AND** the next-battle forecast refreshes without charging upkeep

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

### Requirement: Exactly-once shared wave-clear rewards
A wave SHALL clear only when no living assigned or queued enemies remain across the match and at least one city survives. Each surviving city, including disconnected cities, SHALL receive 2 gold, 5 food and 1 wood for an ordinary clear or 4 gold, 10 food and 2 wood for a boss clear. Each surviving city SHALL additionally receive exactly one research point, including on boss and final clears, without a boss multiplier to research. Rewards SHALL be independent of kills, local clear order, number of inherited allocations and roster size. Fallen cities SHALL receive no reward. The reward SHALL be granted exactly once before moving to the next building phase or final victory, without running production or healing units/cities. All-cities-fallen defeat SHALL take precedence over simultaneous final enemy deaths, and neither ordinary defeat nor battle-stalled defeat SHALL grant a clear reward. Authority state SHALL retain the last rewarded wave and per-city awarded material and research amounts so reconnect and outcome views can reconstruct the result without replaying an economic action.

#### Scenario: Local clear waits for the team
- **WHEN** one city defeats its enemies while another city's enemies remain alive or queued
- **THEN** no city receives the wave-clear reward yet

#### Scenario: Disconnected survivor receives one reward
- **WHEN** a shared wave clears while a living city is disconnected and that player later reconnects
- **THEN** its resource balances include exactly one reward and its last-clear summary identifies the completed wave and amount
- **AND** repeated snapshots, command retries and death cleanup grant nothing further

#### Scenario: Checked reward capacity before battle entry
- **WHEN** a preparation ready check would start a battle whose eventual clear reward cannot fit a living city's bounded material or research balances after its food payment
- **THEN** readiness rejects atomically before food payment, formation or phase progression
- **AND** the city can spend resources before retrying; the authority never wraps a reward, drops part of it or substitutes a stalled battle for a successful clear

#### Scenario: Final reward precedes victory
- **WHEN** the final boss dies and a city survives
- **THEN** that city's balances and last-clear summary include the doubled reward in the victory state
- **AND** no twenty-first wave, production or later reward occurs

#### Scenario: Defeat coincides with the last kill
- **WHEN** the last city and last enemy die on the same tick
- **THEN** the match ends in defeat without a clear reward

#### Scenario: Exactly one research point on a shared clear
- **WHEN** a normal, boss or final wave clears with living connected and disconnected cities
- **THEN** each survivor receives one research point in the same guarded reward transition
- **AND** local clears, inherited enemy allocations, duplicate snapshots and cleanup grant no additional research
