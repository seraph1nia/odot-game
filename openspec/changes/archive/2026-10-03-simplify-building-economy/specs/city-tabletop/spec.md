## MODIFIED Requirements

### Requirement: Readable resource progression and contextual economy controls
Construction choices SHALL be grouped into production, recruitment and advanced/defensive buildings with stable ordering. Affordable eligible choices SHALL be visually emphasized; unavailable choices SHALL remain visible and greyed, with explicit Need N more resource explanations and producer names or the applicable ownership, readiness, pause or phase reason. Explanation SHALL remain accessible even when the purchase button is disabled and SHALL NOT rely only on color. Buildings SHALL NOT be hidden until a resource is discovered. Selected locked plots SHALL offer only their purchase quote and inspection; purchased empty plots SHALL offer construction. Selecting occupied plots SHALL expose their normal actions and an explicit Sell action with the exact refund and retained-army/research explanation. Markets SHALL expose fixed sale bundles, available stock and exact gold proceeds; food-sale previews SHALL update the projected upkeep balance. Producer choices SHALL show their complete text cost alongside their current per-turn output. Producer upgrades SHALL show current to next output with a consistent unit label. The explicit gold-paid Lumbermill recovery choice SHALL be visible only at zero wood, show its complete quoted gold price and follow normal eligibility; normal builds SHALL never silently substitute currency. Affordability, costs, refunds, rates, income and upkeep SHALL derive from authoritative state/catalogs and refresh after accepted actions, production, city switches and reconnects without guessing that a request succeeded. Layout SHALL keep all six resource balances and essential actions readable at 1100x820 and 1280x720 while preserving world selection and the nine-plot overview. Economically unavailable controls SHALL never become enabled solely by a presentation calculation.

#### Scenario: Explain an unavailable Arcanum
- **WHEN** an owner selects an empty purchased plot with enough gold and wood but insufficient stone
- **THEN** Arcanum remains visible and greyed with the missing stone amount and Stonecutter as its production source
- **AND** acquiring the required resources highlights the eligible action without requiring ownership of a Stonecutter or any cloth

#### Scenario: Purchase land through actual world selection
- **WHEN** the player selects a locked plot and explicitly activates its affordable purchase
- **THEN** authoritative acceptance makes that same plot usable and displays construction choices and the updated next expansion price
- **AND** selecting or hovering the plot alone spends nothing

#### Scenario: Sell a building and inspect the cleared plot
- **WHEN** the player activates the selected building's displayed Sell action
- **THEN** acceptance shows the exact resource refund, removes its model and actions and keeps that plot selected as usable empty land
- **AND** existing army and research remain visible without being refunded or healed

#### Scenario: Sell resources through a Market
- **WHEN** the player selects an owned Market and activates a valid displayed sale bundle
- **THEN** accepted resource and gold changes match the quote and duplicate observations replay no sale
- **AND** selling the last Market removes trading controls immediately

#### Scenario: Wider economy stays readable
- **WHEN** the six-resource view is displayed at either supported verification size with a selected building and the expanded city
- **THEN** balances, complete costs, upkeep and actions remain readable and reachable without horizontal clipping
- **AND** plots, home and battle approach retain the supported overview and click targets

#### Scenario: Understand a producer before purchase
- **WHEN** an owner inspects default Lumbermill construction or a level-one Lumbermill upgrade
- **THEN** construction shows 1 wood and +1 wood/turn, and upgrade shows 2 wood plus 2 stone and output 1 to 2 wood/turn
- **AND** missing materials and producer sources remain readable even when the action is disabled

### Requirement: Visible upkeep forecast and reserve participation
The observed city SHALL have a very small text-only Upkeep table directly below its resource table, without opening City details. During building and preparation, including while ready, its first row SHALL show Next battle food demand. When every soldier can be fed, the second row SHALL show Food after payment using current food minus projected payment. On shortage the second row SHALL instead show the exact number of soldiers that will sit out, with a textual cue rather than color alone. Empty armies SHALL show zero demand. Current food, projected payment and which soldiers would participate or sit out under the authoritative stronger-first, stable-identity allocation SHALL remain accessible through inspection. Pause and transport loss SHALL identify paused or stale synchronized values without suggesting another payment. Recruitment SHALL show equipment costs separately from per-battle food upkeep. A shortage SHALL be explained before Ready without disabling readiness solely for food. Accepted recruitment or food sales SHALL refresh the forecast. During combat the view SHALL distinguish participating soldiers, fed capacity-queued soldiers and unfed reserves, showing actual paid upkeep for that wave. The compact table SHALL label that receipt Paid this battle with its wave and reserve count; outcome views SHALL identify Last battle, and fresh sessions SHALL clear old receipts. Reserves SHALL remain in army details with retained level and health but SHALL NOT be rendered as deployed combatants, walking capacity queues or casualties. Reconnect SHALL restore the current status without replaying a feeding, death or recruitment effect.

#### Scenario: Preview a food sale's consequence
- **WHEN** selling a food bundle would leave insufficient food for the whole army
- **THEN** the Market preview identifies the resulting payment and units that would sit out before the sale is activated
- **AND** acceptance updates the common upkeep forecast from authoritative state

#### Scenario: Start a battle underfed
- **WHEN** the player readies with a visible shortage and the shared battle starts
- **THEN** the displayed food deduction and participating/reserve army agree with the preview if no relevant inputs changed
- **AND** unfed soldiers remain inspectable without appearing on the active battlefield or playing a death animation

#### Scenario: Reconnect with reserves
- **WHEN** a client reconnects to a wave containing unfed soldiers
- **THEN** it sees the retained current payment and reserve status without paying food again or admitting those reserves to combat

#### Scenario: Read the food consequence without opening Details
- **WHEN** an observed city has 15 food and six living Swordsmen before battle
- **THEN** the visible Upkeep table shows Next battle 6 food and Food after payment 9
- **AND** a shortage replaces the second row with the exact sit-out count and does not disable Ready solely because of food

#### Scenario: Combat shows an actual payment
- **WHEN** preparation starts combat and the owner changes cities or reconnects
- **THEN** the table shows the observed city's actual wave-tagged paid food and reserve count
- **AND** the display grants no food, charges no upkeep and does not imply a new payment

### Requirement: Vertical resource table
Graphical match views SHALL show a narrow top-right three-column table with Resource, Stock and Income/turn columns, ordered Gold, Food, Wood, Stone, Metal and Cloth. All six rows SHALL show exact authoritative balances and projected production including zero, for the currently observed city, and SHALL refresh after synchronization, production, accepted spending, rewards, city switches and reconnect. Numeric stock and income columns SHALL be right-aligned; positive income SHALL use + and zero SHALL use 0. Building turns SHALL label income Income/turn, while preparation and combat SHALL identify Next building turn and communicate that Ready for battle grants no income. Fallen and terminal cities SHALL show zero future income with an inactive cue; an unrepresentable custom projection SHALL show an explicit unavailable value instead of a guessed amount. Pause and transport loss SHALL retain last synchronized values with paused/stale context. Income SHALL refresh after producer construction, upgrades or sales even without a production payment. The table SHALL contain no resource icons and SHALL NOT introduce additional resource types. Resource totals and purchased-land summaries SHALL NOT be repeated in the bottom HUD. Contextual costs, refunds, trade quotes and food upkeep SHALL remain accessible. Pointer input over the table SHALL NOT select or zoom the world behind it.

#### Scenario: Inspect and reconnect
- **WHEN** a player switches to a foreign city or reconnects to the currently observed city
- **THEN** every resource row immediately uses that city's current synchronized balance
- **AND** foreign-city spending stays unavailable and no historical earning animation is replayed

#### Scenario: Income agrees with current capacity
- **WHEN** a living default city owns a level-one Lumbermill and level-two Metal Mine
- **THEN** its table shows +2 gold, +1 wood, +8 metal and 0 income for other resources
- **AND** selling a producer updates income after authoritative acceptance without granting production

#### Scenario: Preparation does not promise production
- **WHEN** a city reaches Preparation after its third production
- **THEN** income is labeled Next building turn and Ready communicates battle entry without income
- **AND** combat uses future-production wording while terminal and fallen cities show no future income

#### Scenario: The combined stack fits supported layouts
- **WHEN** the resource and Upkeep tables and a unit inspector are visible at 1100x820 or 1280x720
- **THEN** their essential values fit inside the viewport, the inspector fits below the combined stack and above the approximately 180px HUD, and all nine plots and the battle approach remain visible and selectable at overview
- **AND** pointer input inside either table blocks world selection, zoom and drag initiation
