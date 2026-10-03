# Spec Delta

## Purpose

Give each city independent research choices that develop its army during one cooperative match, with modest shared-clear income and optional plot-consuming Research Towers.

## ADDED Requirements

### Requirement: Personal match research state
Every city SHALL begin a fresh match with zero research points, zero production progress and no purchased technologies. Points, progress and technologies SHALL belong to that city and persist across its waves, disconnects and reconnects. There SHALL be no team pool, point transfer, account unlock or cross-match progression. Eliminated cities SHALL retain inspectable research state but SHALL receive no new income and SHALL make no purchases. Research SHALL remain separate from the six tradeable equipment/economy resources.

#### Scenario: Independent cities and fresh matches
- **WHEN** one city acquires research and another city has made no research purchases
- **THEN** only the first city's balance, technology choices and eligible units change
- **AND** a fresh match clears both cities' former research state

#### Scenario: Disconnected owner
- **WHEN** a living owner disconnects while production and shared clears continue
- **THEN** its city receives normal research income and retains purchases
- **AND** reconnect restores those amounts without another starting grant

### Requirement: Production-based research towers
A Research Tower SHALL occupy one purchased owned plot, have two levels and replace the former Blacksmith building role. At each actual production, each level-one tower SHALL add one progress unit and each level-two tower two. Multiple towers SHALL add their contributions. Every three accumulated progress units SHALL convert into one whole research point, retaining a city-wide remainder of zero through two. Only towers owned at that production's resolution SHALL contribute; construction, upgrades, preparation, combat, pause and wall-clock waiting SHALL NOT grant progress on their own. Upgrading SHALL affect only subsequent productions. Selling towers SHALL preserve banked points, partial progress and purchased technologies and SHALL remove only future tower contribution; rebuilding SHALL grant no bonus. Towers SHALL neither attack nor occupy combat capacity.

#### Scenario: Full cycle and additive towers
- **WHEN** a city owns one level-one tower and one level-two tower through three production resolutions
- **THEN** the city gains three research points from towers in total
- **AND** neither final preparation nor battle entry adds another tower payment

#### Scenario: Partial cycle and sale
- **WHEN** a level-one tower contributes at two productions, is sold, and a replacement later contributes at one production
- **THEN** the city's retained two progress units and new one unit convert into exactly one point
- **AND** sale and replacement themselves grant no research

#### Scenario: Upgrade between productions
- **WHEN** a tower is level one for the first production and level two for the next two
- **THEN** its contributions total five progress units, producing one point with two units retained

### Requirement: Exactly-once clear research income
Each surviving city SHALL receive one research point on each shared wave clear, including boss and final clears. This SHALL use the shared reward transition, be independent of kills, local clear order, inherited allocations and roster size, and include disconnected survivors. Boss clears SHALL NOT multiply the research reward. Defeat SHALL grant no clear point. Authoritative latest-clear evidence SHALL include the actual research award and wave. Retries, snapshots and cleanup SHALL NOT pay again. Research arithmetic SHALL be nonnegative, bounded and checked; production and reward preflight SHALL include points/progress so failure changes no city's research, material balance, readiness transition, upkeep or phase partially.

#### Scenario: Boss and final clear
- **WHEN** a city survives a shared boss clear, including wave twenty
- **THEN** its material rewards use their existing boss multiplier while its research reward is exactly one point
- **AND** the final point is present in victory without another production or payment

#### Scenario: Local clear and inherited attackers
- **WHEN** a city clears its original enemies but enemies still remain elsewhere or transfer in
- **THEN** it receives no early research reward
- **AND** the eventual shared clear pays each survivor exactly one point

#### Scenario: Research overflow is atomic
- **WHEN** production or prospective clear income would overflow a city's research state
- **THEN** the corresponding ready transition rejects before any city's income, food payment or phase changes

### Requirement: Catalog-driven technology purchases
All cities SHALL use the same finite technology catalog with stable identifiers, prerequisites, eligible unit types, exclusivity groups, effects and point prices. Foundation nodes SHALL cost three points, specializations six and advanced improvements nine. Purchases SHALL require an authenticated living owner during unpaused editable building/preparation while not ready. They SHALL require no selected plot, Research Tower, material payment or recruitment-building ownership. Unknown identifiers, unmet prerequisites, insufficient points, already purchased nodes, conflicting choices, foreign ownership and stale match/stage context SHALL reject without mutation. A purchase SHALL atomically spend its price, record the node and update eligible units. Retried commands SHALL not charge or apply twice.

#### Scenario: Purchase without a tower
- **WHEN** an eligible living city with three earned points and no Research Tower buys a foundation
- **THEN** three research points are spent, material stocks are unchanged and the foundation is acquired

#### Scenario: Rejection and retry
- **WHEN** a player retries a successful purchase or requests a prerequisite-locked, conflicting or unaffordable node
- **THEN** no extra points are deducted and no extra capability is applied

### Requirement: Exclusive army specializations
The initial tree SHALL have independent melee, ranged and magic foundations. Each foundation SHALL increase its class's own level-scaled maximum health and direct attack damage by five percent without healing. Melee SHALL fork into Guardian damage reduction or Assault direct damage; ranged into Venom poison or Precision direct damage; magic into Fire burn or Frost chill. Each six-point specialization SHALL immediately grant its defining benefit. Each nine-point descendant SHALL improve that chosen benefit. Purchasing one specialization SHALL lock its sibling and that sibling's descendants for the rest of the match, without respec. Different class forks SHALL remain combinable. The first catalog SHALL contain no freeze/stun promise or inaccessible placeholder for either mechanic.

#### Scenario: Fire choice is permanent and personal
- **WHEN** a city purchases Fire after its magic foundation
- **THEN** its existing and future Mages gain burn and its Frost branch becomes unavailable
- **AND** another city can independently choose Frost and the Fire city can still choose Venom for ranged units

#### Scenario: Existing wounded and future units
- **WHEN** a city purchases a technology with wounded mixed-level eligible veterans and later recruits another eligible unit
- **THEN** each veteran gains its own level-appropriate capability while retaining identity, level and current health
- **AND** the new recruit begins with that city's purchased capabilities and its appropriate maximum health

#### Scenario: No benefit to enemy recipients or defenses
- **WHEN** enemies transfer into a researched city or the city builds a defensive tower
- **THEN** neither inherits that city's unit technologies
- **AND** Research Towers affect income only and ordinary defenses retain their authored combat behavior
