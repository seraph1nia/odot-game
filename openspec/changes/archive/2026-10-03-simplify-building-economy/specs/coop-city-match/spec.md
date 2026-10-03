## MODIFIED Requirements

### Requirement: Material production and construction
Gold, food, wood, stone, metal and cloth SHALL be the six economy resources. Each living city SHALL receive configured base gold once per production turn. Gold Mines SHALL add gold, Farms food, Lumbermills wood, Stonecutters stone, Metal Mines metal and Weavers cloth. Producers SHALL create their output directly without consuming another resource. Gold Mine SHALL be the presentation name of the existing gold-producing Mine, distinct from Metal Mine. Construction SHALL atomically pay all displayed resource costs and occupy exactly one empty purchased owned plot during unpaused building or preparation for a living player who is not ready. Buildings present at production SHALL produce at their current level exactly once; sold producers SHALL contribute nothing and preparation SHALL produce no resources. Research Towers SHALL contribute city-research progress during those same actual production resolutions; research SHALL remain separate from the six material resources.

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
- **WHEN** a default city constructs its Farm, Metal Mine and Barracks and resolves three productions
- **THEN** construction spends its 6 starting wood and no gold, production supplies 12 metal and 15 food, and six level-one Swordsmen can be equipped for 2 metal each
- **AND** battle entry pays the existing 6 food once

#### Scenario: Recovery charges only the selected quote
- **WHEN** an eligible zero-wood owner explicitly constructs a recovery Lumbermill and retries the accepted request
- **THEN** exactly 4 gold is paid once, its recorded investment is 4 gold, and a later eligible sale refunds 2 gold
- **AND** a standard build request with no wood or a recovery request with positive wood rejects without spending

#### Scenario: Income refreshes without speculative spending
- **WHEN** construction, upgrading or selling changes a living city's producer capacity
- **THEN** its synchronized projected production immediately matches the current producers and base income
- **AND** projecting, reconnecting or observing the value grants no resources

### Requirement: Simple building upgrades
Barracks, Archery Range and Arcanum SHALL each have five levels; Gold Mines, Farms, Lumbermills, Stonecutters, Metal Mines, Weavers, Research Towers, Arrow Towers and Catapult Towers SHALL have two levels; Markets SHALL have one level and no upgrade action. A player SHALL be able to spend the complete displayed resource quote to upgrade their own building by exactly one level up to its type-specific maximum during unpaused building or preparation while alive and not ready. Level-two producers SHALL produce more of their respective resource per plot; recruitment-building upgrades SHALL raise future recruits to the new building level and make their displayed total recruitment price nondecreasing, without food discounts. Positive recruitment cost components SHALL be rounded to nearest whole units with midpoint ties upward and remain positive; zero components SHALL remain zero. Prices SHALL be calculated from the archetype base cost and its configured growth curve, not from previously rounded prices. Default total prices SHALL increase at each level; custom positive small costs SHALL permit a nondecreasing total with rounding plateaus. Default level-one equipment costs SHALL be 2 metal for Swordsman, 3 metal for Berserker, 2 wood plus 1 metal for Crossbowman, and 3 cloth plus 1 gold for Mage. Tower upgrades SHALL improve their configured attack profile. Research Tower level two SHALL contribute two research progress units per production instead of one; its level SHALL NOT gate technology purchases or grant a technology automatically. Barracks and Archery Range upgrades to level two SHALL cost 2 wood without gold; Arcanum upgrades to level two SHALL cost 4 gold and 2 wood. Recruitment-building upgrades from level two to three, three to four and four to five SHALL cost respectively 6 gold plus 2 wood plus 2 stone, 9 gold plus 2 wood plus 3 stone, and 14 gold plus 2 wood plus 4 stone. All basic producer upgrades SHALL cost 2 wood and 2 stone without gold. Research Tower and Arrow Tower upgrades SHALL cost 4 gold, 2 wood and 2 stone; Catapult Tower upgrades SHALL additionally cost 1 metal. Buildings SHALL retain their upgrades between turns and waves until sold. A maximum-level or unaffordable upgrade SHALL fail without spending resources.

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
