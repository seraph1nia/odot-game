# Spec Delta

## MODIFIED Requirements

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
