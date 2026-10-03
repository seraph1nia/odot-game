# coop-city-match Specification

## Purpose

Provide a small cooperative city-defense match whose economic choices and automatic battles exercise authoritative multiplayer with a persistent state.

## Requirements

### Requirement: Fixed roster and personal cities
Cooperative play SHALL offer a lobby for up to four players and an explicit start action that locks the current roster. In a player-hosted lobby only the original host SHALL start the match; explicit dedicated-server sessions SHALL retain their existing connected-member start behavior. Selecting Single player SHALL create exactly one local player and constitute the explicit start of that local match, with no network admission. Each roster member SHALL begin with an independent city of nine stable indexed building plots, initially five empty usable slots and four locked expansion plots, positive city health, starting gold, and a built-in ranged defender outside those slots. Slot identities SHALL remain stable across graphical placement, upgrades, observation, and reconnects; the slots SHALL NOT require square geometry. Each city SHALL also have a separate declared combat hex board whose adjacency, capacity and permanent faction deployment protection govern autobattle. Decorative hex terrain outside that board SHALL NOT add adjacency bonuses, movement rules or building capacity; the combat board SHALL NOT create additional building slots. One player SHALL be sufficient for solo play and hosting; the cooperative match SHALL support at least two simultaneous players. Fresh players SHALL NOT join an already started match, but roster members SHALL be able to reconnect.

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
Gold, food, wood, stone, metal and cloth SHALL be the six economy resources. Each living city SHALL receive configured base gold once per production turn. Gold Mines SHALL add gold, Farms food, Lumbermills wood, Stonecutters stone, Metal Mines metal and Weavers cloth. Producers SHALL create their output directly without consuming another resource. Gold Mine SHALL be the presentation name of the existing gold-producing Mine, distinct from Metal Mine. Construction SHALL atomically pay all displayed resource costs and occupy exactly one empty purchased owned plot during unpaused building or preparation for a living player who is not ready. Buildings present at production SHALL produce at their current level exactly once; sold producers SHALL contribute nothing and preparation SHALL produce no resources. Research Towers SHALL contribute city-research progress during those same actual production resolutions; research SHALL remain separate from the six material resources.

Lumbermills SHALL require gold only. Farms, Gold Mines, Stonecutters, Metal Mines, Weavers, Barracks and Archery Ranges SHALL require gold and wood. Arcanums, Markets and Research Towers SHALL require gold, wood and stone. Arrow Towers SHALL require gold and wood; Catapult Towers SHALL require gold, wood, stone and metal. These resource requirements SHALL NOT additionally require ownership of their producers, a base level, a talent, a wave milestone or a previous resource discovery. Arcanum construction SHALL NOT require cloth; recruiting a Mage SHALL require cloth. Costs, balances and arithmetic SHALL be nonnegative bounded whole amounts, and invalid or overflowing transactions SHALL fail without partial mutation. Starting grants, production and costs SHALL permit an ordinary opening that recruits and feeds defenders before wave one. Stone, metal and cloth SHALL begin at zero; gold income and a gold-only Lumbermill SHALL keep foundational production recoverable without a Market, including by selling a building to free space.

#### Scenario: Construct and produce
- **WHEN** a player buys a Gold Mine and Farm in purchased empty plots and the production ready check resolves
- **THEN** their displayed construction costs are deducted once and that city receives base gold, mine gold and farm food once
- **AND** another city's resources and buildings are unchanged by those purchases

#### Scenario: Invalid construction
- **WHEN** a request names a locked, occupied, foreign or out-of-range plot, an unknown building, an unaffordable purchase or an ineligible phase
- **THEN** it fails with an explanation and changes no resources, plot or building

#### Scenario: Wood cannot soft-lock construction
- **WHEN** a living city has no wood but can afford a Lumbermill and has or frees a purchased plot
- **THEN** it can build that Lumbermill for gold alone and receive wood at the next production

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

### Requirement: Manual recruitment and persistent soldiers
A player SHALL recruit one soldier by explicitly selecting an owned recruitment building during unpaused building or preparation while alive and not ready. Barracks SHALL offer Swordsman and Berserker, Archery Range SHALL offer Crossbowman, and Arcanum SHALL offer Mage. Unknown or mismatched archetypes SHALL be rejected without spending. Each successful recruitment SHALL deduct the selected archetype's configured material and supplemental gold costs, resolved for the selected recruitment building's level, and immediately add one soldier of that type and building level to that city's army. Unknown soldier types SHALL be rejected atomically; requests without an explicit type SHALL select Swordsman. Swordsmen and Berserkers SHALL require metal, Crossbowmen SHALL require metal and wood, and Mages SHALL require cloth and gold. Recruitment SHALL spend no food; food SHALL be reserved for battle upkeep. Recruitment buildings SHALL NOT grant free soldiers when constructed, upgraded or during production, and gold SHALL NOT substitute for required materials. Every building SHALL retain its existing archetype offerings at level one; higher levels SHALL improve new recruits without adding archetype unlocks. Default recruitment SHALL remain Swordsman from Barracks. Living soldiers SHALL retain their identities, types, individual levels and remaining health between waves; upgrading a recruitment building SHALL NOT change or heal existing soldiers; dead soldiers SHALL NOT return automatically.

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
Barracks, Archery Range and Arcanum SHALL each have five levels; Gold Mines, Farms, Lumbermills, Stonecutters, Metal Mines, Weavers, Research Towers, Arrow Towers and Catapult Towers SHALL have two levels; Markets SHALL have one level and no upgrade action. A player SHALL be able to spend the complete displayed resource quote to upgrade their own building by exactly one level up to its type-specific maximum during unpaused building or preparation while alive and not ready. Level-two producers SHALL produce more of their respective resource per plot; recruitment-building upgrades SHALL raise future recruits to the new building level and increase their displayed total recruitment price, without food discounts. Positive recruitment cost components SHALL be rounded to multiples of five with midpoint ties upward; zero components SHALL remain zero. Prices SHALL be calculated from the archetype base cost and its configured growth curve, not from previously rounded prices. Tower upgrades SHALL improve their configured attack profile. Research Tower level two SHALL contribute two research progress units per production instead of one; its level SHALL NOT gate technology purchases or grant a technology automatically. Recruitment upgrades to level two SHALL cost gold and wood; upgrades to levels three through five SHALL also require stone. Producer, Research Tower and tower upgrades SHALL require stone as part of their displayed costs. Buildings SHALL retain their upgrades between turns and waves until sold. A maximum-level or unaffordable upgrade SHALL fail without spending resources.

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

### Requirement: Ready checks and three-turn wave cadence
Each wave SHALL be preceded by exactly three building/production turns. Connected living players SHALL explicitly set ready; they SHALL be able to withdraw readiness before resolution. Ready players SHALL NOT construct, upgrade, research, recruit, buy plots, sell buildings or trade resources until they withdraw readiness. Disconnected and eliminated players SHALL NOT block the ready check. A building turn SHALL resolve only when at least one living player is connected and every connected living player is ready. Resolution SHALL apply production exactly once to every living city, including disconnected cities, and reset readiness. After the third production turn, the match SHALL enter an editable preparation stage with readiness cleared. Preparation SHALL permit normal owned economic actions without production. A second ready check following the same connected/living eligibility SHALL start combat once any prior wave's authoritative death cleanup is complete, resolve this wave's food upkeep exactly once before army formation, and SHALL NOT grant income or create a fourth production turn. Insufficient food SHALL use the battle-upkeep reserve rule rather than add a shared readiness blocker. Readiness during cleanup SHALL remain recorded; cleanup completion SHALL resolve the eligible check once without requiring a repeated ready command. Snapshots SHALL distinguish preparation from production stages and commands from prior stages SHALL be stale.

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
- **THEN** the player can recruit, construct, upgrade, research, buy plots, sell buildings or trade with available resources in preparation
- **AND** completing the preparation ready check starts that wave without further income

#### Scenario: Preparation survives pause and reconnect
- **WHEN** a city reconnects during paused preparation
- **THEN** it sees current resources, readiness and preparation state, with economic actions disabled until resume

#### Scenario: Ready while previous deaths finish
- **WHEN** players finish the next preparation ready check before the previous wave's death reservations expire
- **THEN** their readiness remains set and no extra income, upkeep payment or battle is created
- **AND** the next wave begins once cleanup completes if the ordinary ready eligibility still holds

### Requirement: Automatic battles and city defense
Each city SHALL have an automatic bounded hex battlefield with Swordsman, Berserker, Crossbowman and Mage archetypes on both factions, classified as melee, ranged and magic. Equal archetype, unit level, research rank and explicit boss modifiers SHALL have identical combat statistics and attack rules across factions. Enemies SHALL use skeleton presentation without hidden faction stat bonuses. Each wave SHALL have a deterministic authored composition whose entries declare archetype, positive unit level, positive count and explicit boss identity. A wave SHALL support entries of different levels and SHALL NOT derive a unit level implicitly from its wave number. Default ordinary waves SHALL progress from melee to mixed threats while preserving original-roster allocation and redistribution. Melee units SHALL approach melee range; ranged and magic units SHALL approach their longer attack range and hold while a valid target remains in range. Each type SHALL use its own authoritative health, damage, unit size, initiative, integer hex range and move/windup/recovery/death durations from the frozen combat configuration. Melee SHALL attack distinct adjacent hexes at range one. Ready units SHALL choose the closest in-range opponent, then lowest initiative and a seeded tie; units without an in-range target SHALL approach a usable attack position or wait when blocked. Units SHALL hold through attack windup and recovery and SHALL NOT automatically retreat when reached in melee. Crossbow hits SHALL be single-target attacks with no friendly fire; their cosmetic projectile SHALL NOT require physical collision or impart knockback. Mage splash SHALL obey the bounded combat-archetypes contract. Soldiers SHALL fight without player orders. While alive, each city's built-in defender SHALL repeatedly attack living enemies assigned to that city at low damage even if its army has been wiped out. Enemies without a living deployed participating defending soldier SHALL approach the declared city-defense anchor and attack city health using their own range and attack timing. Legal siege positions SHALL keep city health reachable without requiring occupation of protected allied deployment cells. Reinforcements arriving after that city cleared its enemies SHALL use guaranteed protected entry and reach engagement within the declared admission bound even when defenders hold neutral enemy-forward cells. City impacts SHALL revalidate that exposure and destination; an invalid city primary SHALL miss without retargeting. Capacity-queued and dying units SHALL NOT attack or be selected. Unfed reserves SHALL neither deploy nor attack, be targeted, reserve capacity or screen city health; they SHALL remain distinct from fed units queued for capacity. Army loss alone SHALL NOT eliminate the city. Construction, recruitment, research, upgrades, plot purchases, building sales and Market trades SHALL be unavailable during combat. Buildable towers SHALL supplement the built-in defender without occupying combat capacity and SHALL stop attacking when their city falls. City health SHALL carry between waves without automatic healing.

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
- **THEN** it retains identity, archetype, level, rank, boss identity/modifiers, health, origin and remaining attack recovery
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

### Requirement: Future allocations and twenty-wave outcome
Each of twenty waves SHALL contain a configured baseline enemy allocation for every original roster member. A living member SHALL receive their own allocation; allocations of eliminated members SHALL be split among the remaining living cities in stable order with balanced integer remainders. Original allocations SHALL be counted once regardless of how many eliminations have occurred. The match SHALL enter shared victory once wave twenty has no living enemies and at least one city remains. It SHALL enter defeat as soon as no living city remains, taking precedence over simultaneous enemy deaths, with an all-cities-fallen reason. If living enemies remain after ordinary health/outcome resolution and an active engagement's no-progress limit or the wave's maximum combat duration expires, the whole match SHALL instead enter defeat with a battle-stalled reason, identifying the triggering city when applicable, limit and tick. Protected deployment SHALL handle ordinary reinforcement admission independently of this fallback. Battle-stalled defeat SHALL NOT change surviving health, mark living cities eliminated, award victory or redistribute fabricated casualties. Ordinary wave completion on the deadline tick SHALL take precedence over a limit, while ordinary health defeat SHALL retain precedence over victory. No twenty-first wave, further economic turns or combat actions SHALL run after an outcome. Dying bodies SHALL NOT delay the ordinary gameplay outcome, and their bounded authoritative cleanup SHALL continue without new combat actions; a nonterminal next wave SHALL NOT reuse reserved positions before cleanup completes. Eliminated connected players SHALL be able to observe the rest of the match.

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
- **WHEN** a three-player original roster reaches wave ten with two cities surviving
- **THEN** exactly three bosses are created, one for each original roster allocation, and the fallen city's boss is assigned by the ordinary stable remainder policy
- **AND** those bosses remain individual units and can accumulate at one destination

### Requirement: Changed-state snapshot publication
The authority SHALL publish complete changed snapshots at the bounded combat cadence and promptly for pause or non-combat revisions. It SHALL NOT continually enqueue unchanged paused/building snapshots on the reliable channel. Welcome and command acknowledgment delivery SHALL still carry the complete current state, and current death cleanup SHALL remain observable as it changes authoritative revisions.

#### Scenario: Resume after a long shared pause
- **WHEN** clients inspect a paused battle and an eligible player resumes then pauses at a later action tick
- **THEN** the graphical and headless peers receive the later paused revision and agree on its authority tick
- **AND** repeated copies of the prior unchanged pause do not delay the revision behind a growing reliable snapshot queue

### Requirement: Once-per-battle food upkeep and inactive reserves
Food SHALL pay army upkeep at battle entry and SHALL NOT be a recruitment cost. Each recruit archetype SHALL declare a positive whole food upkeep amount per battle, independent of unit level and research. At the actual preparation-to-combat transition, after prior death cleanup and before formation, every living city's living soldiers SHALL be evaluated once, including disconnected cities. Enemies, bosses, towers, the built-in defender and dying/dead units SHALL owe no city food. Zero living soldiers SHALL cost zero. The authority SHALL process soldiers by descending unit level, then ascending stable identity; it SHALL fully pay and include each affordable soldier, otherwise skip that soldier and continue to later soldiers with the remaining food. No partial payment or negative food SHALL occur. A participant that waits in a capacity queue SHALL already be paid and SHALL NOT pay again on admission.

Unfed soldiers SHALL remain living inactive reserves for the entire shared wave, preserving identity, archetype, level, research and health. They SHALL have no active footprint, admission queue, targeting, attacks or city-screening effect, and no starvation damage or profile penalty. Existing building ownership SHALL NOT affect their upkeep or survival. Food shortage SHALL NOT prevent the shared battle from starting. At the next wave's preparation all surviving reserves SHALL be eligible for a fresh forecast, and its battle entry SHALL recalculate participation from current food and army. Local clearing and enemy redistribution SHALL NOT change that wave's food payment or reactivate reserves. A fallen city's reserves SHALL NOT join another city's army or return to battle.

Authority state SHALL expose a current forecast with full army demand, available food, projected payment and participant/unfed identities, plus a latest wave-tagged actual payment and participation receipt. Forecast evaluation SHALL NOT spend resources or mutate combat state. Production, readiness changes, cleanup waits, pause, reconnect, duplicate commands/snapshots and repeated stepping SHALL NOT charge again. Wave-clear food SHALL be usable at the next battle only; victory SHALL NOT create upkeep for another wave. New sessions SHALL clear prior forecasts, receipts and reserve status.

#### Scenario: Pay for the whole army once
- **WHEN** preparation completes with enough food for every living soldier
- **THEN** the city pays exactly its summed upkeep once and all those soldiers participate, including any waiting for formation capacity
- **AND** subsequent admission, repeated ready requests and reconnect do not deduct food again

#### Scenario: A shortage feeds stronger recruits first
- **WHEN** a city has two food, a level-three Mage owing two, and a level-one Swordsman owing one
- **THEN** the Mage participates, two food are deducted and the Swordsman sits out with unchanged identity, level and health
- **AND** the wave starts without requiring the city to buy more food

#### Scenario: Skip an unaffordable unit and feed a cheaper one
- **WHEN** that same city has only one food
- **THEN** the Mage sits out, the Swordsman participates and one food is deducted
- **AND** equal-level ties use stable identity and produce the same allocation under reversed storage order

#### Scenario: No fed defenders still permits siege
- **WHEN** a city's entire living army is unfed at battle entry
- **THEN** those reserves occupy no combat space and cannot prevent normal enemies from attacking city health
- **AND** its built-in defender and towers still act under ordinary rules

#### Scenario: Disconnected city has insufficient food
- **WHEN** connected living players finish preparation while a disconnected living city cannot feed its whole army
- **THEN** that city follows the same deterministic payment/reserve rule and does not block combat
- **AND** reconnect restores its actual receipt and unchanged reserve identities without another payment

#### Scenario: Prior death cleanup postpones payment
- **WHEN** all required players are ready but prior-wave death reservations have not expired
- **THEN** food remains unspent while the barrier waits
- **AND** cleanup completion charges once only if the normal ready eligibility still holds and combat actually starts

#### Scenario: Reserves can return in the next wave
- **WHEN** an unfed soldier's living city later has enough food at the following battle entry
- **THEN** that same soldier can participate at its retained level and health after its upkeep is paid
- **AND** it was neither healed, killed nor recruited again while sitting out

### Requirement: Gold-purchased plot expansion
A living, connected and unready owner SHALL be able to buy any one of their locked plots during unpaused building or preparation. Buying a plot SHALL pay the authoritative next expansion gold quote atomically and make that specific plot permanently usable for the match; it SHALL NOT construct a building. The default nine-plot board SHALL start with five purchased plots and offer four expansions at strictly increasing positive gold prices based on the number of previous purchases, not plot location. Plot purchases SHALL require no building, Market, resource other than gold or wave milestone. Already purchased, invalid, foreign or unaffordable requests SHALL fail without changes. Retries SHALL neither charge twice nor unlock another plot. Purchased land SHALL NOT be sold or refunded when its building is sold, and expansions SHALL NOT enlarge the combat board or grant terrain bonuses.

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

### Requirement: Market-gated resource sales
Owning at least one Market SHALL allow a living, connected, unready city to explicitly sell food, wood, stone, metal or cloth for gold during unpaused building or preparation. Each request SHALL select an owned Market, a known non-gold resource and a positive whole number of configured sale bundles. The frozen catalog SHALL declare a positive whole bundle size and positive gold return for each resource. The authority SHALL quote and atomically deduct the full resource amount and credit the corresponding gold; unknown resources, invalid quantities, insufficient stock, overflow, wrong ownership or an absent Market SHALL reject without changes. Duplicate commands SHALL NOT trade twice. Rates SHALL be fixed for the match, independent of wave, volume and number of Markets; Markets SHALL provide no production, automatic sales or upgrades. Buying resources with gold and trading between players SHALL be unavailable. Selling the last Market SHALL immediately remove access to resource sales without preventing building sales or plot purchases.

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

### Requirement: Simple boss rounds
Waves ten and twenty SHALL each contain exactly one boss per original roster allocation and no ordinary escorts. The first default boss SHALL be a level-three Swordsman and the final boss a level-five Swordsman. Each boss SHALL use eight times the level-scaled maximum health and twice the level-scaled damage, with ordinary Swordsman targeting, movement, range, attack cadence and death rules, with size six instead of the ordinary size two. Bosses SHALL have no special abilities, summons, phases or immunity. Clearing wave ten SHALL continue the match; clearing wave twenty SHALL end in shared victory if a city survives. A boss moved between cities SHALL retain its remaining health and modifiers without resetting or multiplying them.

#### Scenario: Solo boss wave
- **WHEN** a solo match enters wave ten
- **THEN** exactly one level-three Swordsman boss and no escorts are allocated
- **AND** it fights using ordinary combat actions with the configured increased health and damage

#### Scenario: Transfer a wounded boss
- **WHEN** a boss survives the fall of its destination city
- **THEN** it transfers once with unchanged identity, level, remaining health and profile

### Requirement: Exactly-once shared wave-clear rewards
A wave SHALL clear only when no living assigned or queued enemies remain across the match and at least one city survives. Each surviving city, including disconnected cities, SHALL receive 10 gold, 5 food and 5 wood for an ordinary clear or 20 gold, 10 food and 10 wood for a boss clear. Each surviving city SHALL additionally receive exactly one research point, including on boss and final clears, without a boss multiplier to research. Rewards SHALL be independent of kills, local clear order, number of inherited allocations and roster size. Fallen cities SHALL receive no reward. The reward SHALL be granted exactly once before moving to the next building phase or final victory, without running production or healing units/cities. All-cities-fallen defeat SHALL take precedence over simultaneous final enemy deaths, and neither ordinary defeat nor battle-stalled defeat SHALL grant a clear reward. Authority state SHALL retain the last rewarded wave and per-city awarded material and research amounts so reconnect and outcome views can reconstruct the result without replaying an economic action.

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
