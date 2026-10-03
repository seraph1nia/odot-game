## MODIFIED Requirements

### Requirement: Clickable unit inspection
Every visible, deployed, living friendly and enemy unit in the observed city, including bosses, SHALL be selectable by a short left click on its model. Selecting a unit SHALL open one nonmodal centre-right popup with a visual preview of that unit, its readable name, short role description, level, faction and boss status where applicable, current/max health as X/Y plus a health bar, and damage per attack. Size SHALL remain inspectable including boss size six. Displayed numeric stats SHALL use the current authoritative resolved unit profile, including level/research/boss modifiers, with readable health/damage scaling; descriptions SHALL NOT imply unsupported mechanics. The popup SHALL refresh from observed authoritative state without changing combat, readiness, plot selection or spending. Shared pause and transport loss SHALL freeze its last observed values until synchronization updates them. Headless roles SHALL create no inspector or model preview.

The popup SHALL remain inside the usable viewport below the resource panel and above the bottom HUD. Clicking inside it SHALL keep it open and block underlying world input. Clicking elsewhere SHALL dismiss it and preserve the clicked target's ordinary eligible action exactly once. Clicking another unit SHALL replace the contents directly. A completed camera drag SHALL NOT select a unit or open a popup; starting a drag SHALL dismiss an existing popup. Unit hit selection SHALL choose the nearest visible unit at the clicked location rather than a plot behind it, with stable tie-breaking. Death visuals and undeployed/reserve units SHALL NOT be world-click inspection targets. Selection SHALL clear on death/removal from the observed city, city change, session/fresh-match replacement, or opening a blocking modal. Reconnect SHALL refresh the same surviving unit's stats or clear invalid selection. Source and exported graphical clients SHALL offer the same interaction.

#### Scenario: Inspect a wounded ranked unit
- **WHEN** the player short-clicks a visible unit with current health 525 and authoritative maximum health 1050 in the integer wire scale
- **THEN** the centre-right popup shows its model, name, description, level and resolved per-attack damage, with human-readable health 5.25/10.5 and half health fill
- **AND** the overhead marker shows the unit's Roman level with no unit name/code and no gameplay command is submitted

#### Scenario: Switch and dismiss inspection
- **WHEN** the player clicks another living unit and then clicks a plot or HUD control outside the popup
- **THEN** the first click replaces the popup with that unit's details and the outside click closes it
- **AND** the plot/control receives its normal eligible action once without click-through behind the inspector

#### Scenario: Drag over units
- **WHEN** a player drags from a unit past the camera gesture threshold
- **THEN** the camera pans, the inspector stays closed, and neither unit nor plot selection changes

#### Scenario: Refresh and clear inspected state
- **WHEN** the inspected unit receives synchronized damage, is paused or disconnected, and later dies or leaves the observed city
- **THEN** the popup uses the new authoritative health, freezes values during pause/loss, and closes when selection becomes invalid
- **AND** changing cities or replacing the match does not retain the old popup or preview

Eligible owned field unit inspection SHALL expose Retire and Send to Town hall actions with capacity/phase/ownership reasons. Stored units SHALL be selected from the Town hall roster and offer Send to battlefield and Retire with the same authority guards. No drag infrastructure SHALL be required.


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

Battlefield home markers, the ordered home overview and explicit purchase control SHALL expose the next authoritative gold purchase quote, occupied/available size and stable first-fit destination. A selected Town hall SHALL display occupied/available storage spots and independently quoted capacity/healing upgrades, current percentage and paid-recovery eligibility. Selection SHALL never spend or transfer on its own; full destinations and occupied-hall sale SHALL be explained.


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

Demand SHALL include stored units and identify field/storage food separately. Funded stored units SHALL not be described or rendered as participating combatants. The roster SHALL explain that recovery uses food funded in the last completed battle and occurs only at actual production.
