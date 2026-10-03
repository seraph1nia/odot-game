## MODIFIED Requirements

### Requirement: Manual recruitment and persistent soldiers
A player SHALL recruit one soldier by explicitly selecting an owned recruitment building during unpaused building or preparation while alive and not ready. Barracks SHALL offer Swordsman and Berserker, Archery Range SHALL offer Crossbowman, and Arcanum SHALL offer Mage. Unknown or mismatched archetypes SHALL be rejected without spending. Each successful recruitment SHALL deduct the selected archetype's configured material and supplemental gold costs, resolved for the selected recruitment building's level, and immediately add one soldier of that type and building level to that city's army only after preflighting a fitting purchased battlefield home under army-roster. Unknown soldier types SHALL be rejected atomically; requests without an explicit type SHALL select Swordsman. Swordsmen and Berserkers SHALL require metal, Crossbowmen SHALL require metal and wood, and Mages SHALL require cloth and gold. Recruitment SHALL spend no food; food SHALL be reserved for battle upkeep. Recruitment buildings SHALL NOT grant free soldiers when constructed, upgraded or during production, and gold SHALL NOT substitute for required materials. Every building SHALL retain its existing archetype offerings at level one; higher levels SHALL improve new recruits without adding archetype unlocks. Default recruitment SHALL remain Swordsman from Barracks. Living soldiers SHALL retain their identities, types, individual levels and remaining health between waves; upgrading a recruitment building SHALL NOT change or heal existing soldiers; dead soldiers SHALL NOT return automatically.

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

Recruitment SHALL reject a full home roster without payment; retirement and Town hall transfers SHALL follow army-roster and preserve the existing ownership/phase/session guards.


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

Town hall SHALL use the independently quoted capacity and healing tracks in army-roster rather than a shared building-level upgrade.


### Requirement: Once-per-battle food upkeep and inactive reserves
Food SHALL pay army upkeep at battle entry and SHALL NOT be a recruitment cost. Each recruit archetype SHALL declare a positive whole food upkeep amount per battle, independent of unit level and research. At the actual preparation-to-combat transition, after prior death cleanup and before formation, every living city's living soldiers, including Town hall storage, SHALL be evaluated once, including disconnected cities. Enemies, bosses, towers, the built-in defender and dying/dead units SHALL owe no city food. Zero living soldiers SHALL cost zero. The authority SHALL process soldiers by field-assigned first, then stored, with descending unit level and ascending stable identity within each group; it SHALL fully pay each affordable soldier and include only field-assigned funded soldiers, otherwise skip that soldier and continue to later soldiers with the remaining food. No partial payment or negative food SHALL occur. A participant that waits in a capacity queue SHALL already be paid and SHALL NOT pay again on admission.

Unfed soldiers SHALL remain living inactive reserves for the entire shared wave, preserving identity, archetype, level, research and health. They SHALL have no active footprint, admission queue, targeting, attacks or city-screening effect, and no starvation damage or profile penalty. Existing building ownership SHALL NOT affect their upkeep or survival. Food shortage SHALL NOT prevent the shared battle from starting. At the next wave's preparation all surviving reserves SHALL be eligible for a fresh forecast, and its battle entry SHALL recalculate participation from current food and army. Local clearing and enemy redistribution SHALL NOT change that wave's food payment or reactivate reserves. A fallen city's reserves SHALL NOT join another city's army or return to battle.

Authority state SHALL expose a current forecast with full army and field/stored demand, available food, projected payment and funded/participant/unfed identities, plus a latest wave-tagged actual payment, funded-identity and participation receipt. Forecast evaluation SHALL NOT spend resources or mutate combat state. Production, readiness changes, cleanup waits, pause, reconnect, duplicate commands/snapshots and repeated stepping SHALL NOT charge again. Wave-clear food SHALL be usable at the next battle only; victory SHALL NOT create upkeep for another wave. New sessions SHALL clear prior forecasts, receipts and reserve status.

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
- **WHEN** all required players are ready but prior-wave death reservations have not expired
- **THEN** food remains unspent while the barrier waits
- **AND** cleanup completion charges once only if the normal ready eligibility still holds and combat actually starts

#### Scenario: Reserves can return in the next wave
- **WHEN** an unfed soldier's living city later has enough food at the following battle entry
- **THEN** that same soldier can participate at its retained level and health after its upkeep is paid
- **AND** it was neither healed, killed nor recruited again while sitting out

Town hall storage SHALL remain independent of unfed reserve status. Funding/participation projections and next-cycle recovery eligibility SHALL follow army-roster, without additional food charges.


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
