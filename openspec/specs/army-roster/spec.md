# army-roster Specification

## Purpose

Give each city's persistent army an explicit paid spatial budget and useful veteran storage and recovery, independent of automatic combat movement and enemy placement.

## Requirements

### Requirement: Purchased physical battlefield homes
Each city SHALL start with two unlocked physical home tiles of six size points each and SHALL offer four further homes bought independently for 5, 8, 12 and 18 gold. The ordered domain SHALL be the existing allied forward cells in center/owner-left/owner-right order, followed by its protected rear cells in that order. Anchors SHALL use forwardmost then identity order. Recruitment and sending a stored unit to the field SHALL choose the first purchased tile with enough unassigned size and its first free anchor; free size in different tiles SHALL NOT be pooled. Assignment SHALL persist independently of current combat position, food eligibility and deployment. Living field-assigned units, including unfed and queued units, SHALL consume home capacity; stored and dead units SHALL not. Buying homes SHALL NOT change movement topology or prevent movement through unpurchased home cells. Purchases SHALL follow existing owner, phase, readiness, pause, stale price and retry guards and SHALL be permanent for the match.

#### Scenario: Fill two homes
- **WHEN** six ordinary size-two units are recruited onto two initial homes
- **THEN** three are assigned to the first tile and three to the second at distinct anchors
- **AND** a seventh recruit rejects without material payment unless another home is bought or an assignment is freed

#### Scenario: Buy the next home
- **WHEN** an eligible owner pays the next home purchase quote
- **THEN** exactly that next home becomes available, gold is charged once, and neither city land nor combat topology changes

### Requirement: Atomic persistent army actions
Eligible living unready owners SHALL retire or transfer a selected living owned unit only during unpaused Building or Preparation. Retirement SHALL permanently remove the unit without refunds, free recruits, healing, starvation damage or a combat death animation. Field-to-hall and hall-to-field transfers SHALL preserve identity, tier, current health and capabilities, and change one assignment atomically. Hall insertion SHALL use first-fit size tiles and anchors. A full, absent, foreign, stale or otherwise ineligible destination or unit SHALL reject without spending, loss, partial assignment or automatic overflow. Capacity SHALL be preflighted before recruitment payment. Duplicate accepted requests SHALL not repeat creation, removal, transfer or payment. No combat-time editing SHALL be allowed.

#### Scenario: Full Town hall
- **WHEN** an owner sends a soldier to a hall with no fitting storage tile
- **THEN** the request fails and its identity, wounds, field home and balances are unchanged

#### Scenario: Retire a veteran
- **WHEN** an eligible owner retires a wounded veteran
- **THEN** its living assignment disappears, its home becomes available and no equipment, gold or food is refunded

### Requirement: Survivor home restoration
Each field-assigned unit SHALL retain its exact home tile and anchor through battle. After shared battle completion and retained death cleanup, each surviving field unit SHALL return there without re-running first-fit, healing, payment, recruitment or damage. Death SHALL free the living roster assignment immediately but SHALL retain its ordinary combat reservations through expiry. New assignments during cleanup SHALL NOT steal retained combat reservations. The next battle SHALL still wait for cleanup. Final victory SHALL also restore surviving field units. Stored and unfed units SHALL remain distinct and reconnect SHALL restore both persistent assignment and current combat/cleanup state.

#### Scenario: Surviving mover
- **WHEN** a unit ends a battle away from its home while corpses retain claims
- **THEN** its recorded home remains unchanged and restoration waits for death cleanup
- **AND** completion returns that same identity to its exact starting anchor without healing

### Requirement: Independently upgraded plot-consuming Town hall
A Town hall SHALL occupy one purchased city plot, initially cost 5 gold, 2 wood and 2 stone, and have three independently quoted capacity levels and three healing levels. Capacity SHALL be 6, 12 and 18 size points in six-point storage tiles; its upgrades SHALL cost respectively 8 gold/2 wood/2 stone and 12 gold/2 wood/3 stone. Healing SHALL be 5, 10 and 15 percent of each eligible stored unit's current maximum HP per production; its upgrades SHALL cost respectively 6 gold/2 wood/2 stone and 12 gold/2 wood/3 stone. Each upgrade SHALL improve only its selected track and affect subsequent actions/resolutions. Hall sale SHALL reject while any living unit is stored; an empty hall SHALL follow existing actual-building-investment sale rules. Capacity upgrades SHALL NOT buy battlefield homes. No free units or level upgrades SHALL result from building or upgrading the hall.

#### Scenario: Upgrade healing independently
- **WHEN** an owner pays the next healing quote
- **THEN** the hall's healing percentage increases once, storage capacity and field homes remain unchanged, and no immediate heal occurs

#### Scenario: Sell an occupied hall
- **WHEN** a hall with a stored living unit is sold
- **THEN** the sale rejects without refund, eviction or unit loss

### Requirement: Paid recovery on real production only
A stored living unit SHALL heal only if its food was paid in the most recently completed battle. A field-funded survivor moved into storage SHALL retain that eligibility; an unfunded unit SHALL not heal until subsequently funded. Initial-cycle units SHALL have no completed-battle eligibility, and new full-health recruits SHALL receive no special free-heal authorization. Each actual production SHALL heal each eligible stored unit once by ceiling(current maximum integer HP times percentage divided by 100), capped at maximum. Current researched maximum SHALL be used; identity, tier and capabilities SHALL not change. Healing SHALL require presence in the hall at resolution and follow the hall's current healing track. Three production turns per wave SHALL mean three possible heals. Preparation, combat, toggles, pause, duplicate requests, reconnect and wall-clock waiting SHALL grant no heal. Failed production preflight SHALL change no health or other income partially. Elimination SHALL remove stored armies under existing city elimination behavior.

#### Scenario: Paid veteran rotates into storage
- **WHEN** a food-funded survivor is stored before the next three production resolutions
- **THEN** those resolutions each apply its hall's percentage once, capped at maximum
- **AND** sending it out or toggling readiness grants no additional heal

#### Scenario: Unpaid reserve
- **WHEN** a stored unit was unfunded in the most recently completed battle
- **THEN** production retains its wounds until it is funded in a later completed battle

### Requirement: Field-first all-owned food accounting
Existing battle-entry upkeep SHALL charge all living field and stored units once, field first, then stored, using descending level and ascending stable identity within each group. A funded stored unit SHALL remain nonparticipating and occupy no combat claims. Unfunded field units SHALL remain ordinary inactive food reserves while retaining their homes. Food forecasting and actual wave receipts SHALL distinguish funded identities, field participants, stored identities and unfunded identities. Clear completion SHALL establish the paid-identity eligibility for the next production cycle without charging again. No per-production upkeep, debt, starvation or readiness blocker SHALL be introduced.

#### Scenario: Stored elite does not displace the field
- **WHEN** one food is available for a lower-level field Swordsman and a higher-level stored Swordsman
- **THEN** the field soldier is funded and participates, the stored soldier is unfunded and remains stored, and one food is paid once
