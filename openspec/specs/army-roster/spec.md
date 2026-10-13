# army-roster Specification

## Purpose

Give each city's persistent army an explicit purchased squad homes and useful veteran storage and recovery, independent of automatic combat movement and enemy placement.

## Requirements

### Requirement: Purchased physical battlefield homes

Each owner SHALL have six numbered army homes, two initially purchased, with the existing purchase-count quotes and arbitrary-order plot selection. A tower-free purchased home SHALL hold one homogeneous squad up to configurable ordinary capacity three, including partial/unfunded members; separate squads SHALL not share it. Recruitment/return SHALL find compatible capacity or an empty purchased home deterministically and skip tower-occupied plots. Explicit reinforcement SHALL target the selected compatible squad; explicit new-squad recruitment SHALL require an empty home. Homes SHALL remain preparation assignments independent of combat position. Buying a numbered plot SHALL unlock only that plot, preserve city land and not change movement topology. Valid owned deployment SHALL move the complete squad's home claim atomically, leaving no duplicate claim at its former home.

#### Scenario: Two starting homes
- **WHEN** six same-type members are recruited with default capacity three
- **THEN** two squads claim the two homes and a seventh recruit requires compatible freed capacity or another purchased home before payment

#### Scenario: Chosen locked home
- **WHEN** an eligible owner buys a higher-numbered locked plot
- **THEN** only that plot unlocks at the current purchase-count quote and retries charge once

#### Scenario: Move a home claim
- **WHEN** an eligible owner places a squad on an empty purchased tower-free deployment home
- **THEN** every field member moves its home assignment and subsequent recruitment cannot create another squad there

#### Scenario: Fill two homes
- **WHEN** six same-type members in two capacity-three squads are recruited onto two initial homes
- **THEN** three are assigned to the first tile and three to the second at distinct anchors
- **AND** a seventh recruit rejects without material payment unless another home is bought or an assignment is freed

#### Scenario: Buy a chosen locked army plot
- **WHEN** an eligible owner selects a locked army plot out of home order and pays the current army purchase-count quote
- **THEN** exactly that chosen plot becomes available, gold is charged once, and neither city land nor combat topology changes
- **AND** subsequent recruitment and Town hall Send retain fixed first-fit order and skip tower-occupied plots

### Requirement: Atomic persistent army actions

Only connected living unready owners during unpaused editable stages SHALL recruit, reinforce, regroup, deploy, store, return or retire owned members. Individual transfers SHALL retain identity, type, equipment level, HP, experience and recovery eligibility; regrouping SHALL use a compatible same-type squad or a legal empty purchased home. Town Hall insertion SHALL use first-fit individual storage slots, independent of combat/squad population. Full/foreign/stale/missing/incompatible destinations and unaffordable actions SHALL refuse without spending, healing, identity loss or partial assignment. Accepted retries SHALL cause no second mutation. Retirement SHALL remove only the selected member without a refund, free recruit or combat death animation. No army editing SHALL occur during combat.

#### Scenario: Reinforce a wounded veteran squad
- **WHEN** an eligible owner pays for one compatible replacement from a higher-level trainer
- **THEN** the new member enters the chosen squad at that trainer's level and zero experience
- **AND** veterans retain their IDs, equipment, injuries and experience

#### Scenario: Incompatible regroup
- **WHEN** an owner transfers a Mage into a Swordsman squad
- **THEN** the request refuses atomically and both rosters and balances remain unchanged

#### Scenario: Full Hall
- **WHEN** a Hall has no free individual slot
- **THEN** storage refuses without changing the member's home, health or resources

#### Scenario: Full Town hall
- **WHEN** an owner sends a soldier to a hall with no free individual storage slot
- **THEN** the request fails and its identity, wounds, field home and balances are unchanged

#### Scenario: Retire a veteran
- **WHEN** an eligible owner retires a wounded veteran
- **THEN** its living assignment disappears, its home becomes available and no equipment, gold or food is refunded

### Requirement: Survivor home restoration

Field survivors SHALL retain their purchased squad home and formation slot through combat and return after bounded cosmetic cleanup without first-fit regrouping, healing or payment, including final victory. Logical death SHALL free a destroyed squad's home/claims without waiting for cosmetics; the next wave SHALL still wait for prior cosmetic cleanup before formation. Stored and unfunded members SHALL remain distinct. Fallen-owner survivors SHALL retain identities in the eliminated roster, fight through the current battle if participating, and become inactive/nonparticipating in future waves rather than join another owner's army.

#### Scenario: Veteran restoration
- **WHEN** a surviving squad ends a battle away from its selected home
- **THEN** cleanup returns its same member IDs to that home/formation with unchanged wounds and equipment

#### Scenario: Fallen owner's survivors
- **WHEN** a Keep falls while an owned participating squad survives
- **THEN** it continues the current battle and remains inspectable afterward
- **AND** it receives no future deployment or upkeep

#### Scenario: Surviving mover
- **WHEN** a unit ends a battle away from its home while cosmetic bodies remain without logical claims
- **THEN** its recorded home remains unchanged and restoration waits for death cleanup
- **AND** completion returns that same identity to its exact home and formation slot without healing

### Requirement: Independently upgraded plot-consuming Town hall

Town Hall SHALL occupy one purchased city plot, cost 5 gold/2 wood/2 stone initially, and retain independent capacity/healing tracks. Capacity SHALL be 6, 12 and 18 individual soldiers; capacity upgrades SHALL cost 8 gold/2 wood/2 stone and 12 gold/2 wood/3 stone. Healing SHALL be 5, 10 and 15 percent of each eligible stored member's current maximum HP per production; healing upgrades SHALL cost 6 gold/2 wood/2 stone and 12 gold/2 wood/3 stone. Tracks SHALL affect only subsequent actions/settlements, grant no immediate healing or units, and never purchase army homes. Occupied Hall sale SHALL refuse; empty Hall refunds SHALL use actual paid investment.

#### Scenario: Seventh stored member
- **WHEN** a base Hall with six stored members receives another storage request
- **THEN** it refuses, and paying the capacity upgrade permits that same seventh member without changing healing level

#### Scenario: Upgrade healing
- **WHEN** an owner pays the next healing quote
- **THEN** capacity is unchanged and no immediate heal occurs

#### Scenario: Upgrade healing independently
- **WHEN** an owner pays the next healing quote
- **THEN** the hall's healing percentage increases once, storage capacity and field homes remain unchanged, and no immediate heal occurs

#### Scenario: Sell an occupied hall
- **WHEN** a hall with a stored living unit is sold
- **THEN** the sale rejects without refund, eviction or unit loss

### Requirement: Paid recovery on real production only

Stored living members SHALL heal only when funded in the most recently completed battle and present in a living owner's Hall at actual production. Field-funded survivors moved to storage SHALL retain eligibility; unfunded/new members SHALL not receive invented eligibility. Each production SHALL heal once by ceiling(current researched maximum HP times Hall percentage divided by 100), capped at maximum, without changing identity/equipment/experience. Exactly three END settlements SHALL occur per wave under battle flow 2; editing, ready toggles, pause, reconnect, waiting, combat and duplicate requests SHALL grant no additional recovery. Failed settlement preflight SHALL be atomic. Elimination SHALL retain stored members but stop future healing/production.

#### Scenario: Funded wounded veteran
- **WHEN** a paid survivor is stored before three actual END settlements
- **THEN** each settlement applies one current Hall heal and sends/readiness/reconnect add none

#### Scenario: Eliminated stored veteran
- **WHEN** the stored member's Keep falls
- **THEN** its ID, injury, level and experience remain in the roster without further recovery

#### Scenario: Paid veteran rotates into storage
- **WHEN** a food-funded survivor is stored before the next three production resolutions
- **THEN** those resolutions each apply its hall's percentage once, capped at maximum
- **AND** sending it out or toggling readiness grants no additional heal

#### Scenario: Unpaid reserve
- **WHEN** a stored unit was unfunded in the most recently completed battle
- **THEN** production retains its wounds until it is funded in a later completed battle

### Requirement: Field-first all-owned food accounting

Battle entry SHALL fund each living owner's individual members once, field first then stored, descending equipment level then stable ID per group, skipping unaffordable members and continuing to cheaper ones. Only funded field members SHALL participate; stored/unfunded members SHALL have no active claims. A partially funded squad SHALL deploy only those participants; zero participants SHALL create no occupancy or attacks. Forecasts and actual wave receipts SHALL expose funded, field, stored and unfunded identities. Successful clear SHALL establish recovery eligibility without recharging. Fallen owners SHALL pay no future upkeep. No per-production upkeep, debt or readiness blocker SHALL be introduced.

#### Scenario: Partial funding
- **WHEN** a three-member squad can afford only two members' upkeep
- **THEN** it claims one cell with two contributors and retains the third as an unfunded roster member

#### Scenario: Field-first priority
- **WHEN** one food could fund a field Swordsman or a higher-level stored Swordsman
- **THEN** the field member is funded first and the stored member remains a noncombatant

#### Scenario: Stored elite does not displace the field
- **WHEN** one food is available for a lower-level field Swordsman and a higher-level stored Swordsman
- **THEN** the field soldier is funded and participates, the stored soldier is unfunded and remains stored, and one food is paid once
