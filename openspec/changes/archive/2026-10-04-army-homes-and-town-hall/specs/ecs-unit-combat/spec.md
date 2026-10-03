## MODIFIED Requirements

### Requirement: Bounded lane contact and formation spacing

Each living city SHALL have a bounded authoritative hex battlefield with declared adjacency and exactly six size capacity per hex. Every unit SHALL have a frozen integer size from one through six. All ordinary archetypes SHALL use size two, bosses size six, and no current archetype SHALL use size one. Same-team units SHALL share a hex whenever their occupied and reserved sizes total at most six; a geometrical footprint shape or rendered anchor arrangement SHALL NOT reject an otherwise legal fit. Occupancy and reservations in one hex SHALL belong to at most one team, allies or opponents, including dying bodies and move reservations. Units SHALL NOT exceed capacity, enter an opposing occupied/reserved hex, exchange places through one another or leave the board. Distinct graphical anchors SHALL preserve readability without adding a numerical capacity restriction. Initial formation and transferred arrivals SHALL obey the same size rules and permanent faction-specific deployment protection. In automatic enemy and isolated non-roster formation, melee SHALL start forward with rear placement allocated to support; placement SHALL spread across columns before packing a cell. Overflow SHALL queue without taking the other tier's allocated setup capacity. City-roster allied setup SHALL instead occupy persistent purchased first-fit homes under army-roster, independent of class; purchased homes SHALL NOT constrain later movement topology. Queued actors SHALL retain their destination army/enemy membership without duplication, deletion, spatial claims, targeting or attacks. Board adjacency SHALL determine movement and distance; rendered geometry, scenery collision and player orders SHALL NOT determine combat. Blocked units SHALL wait without locomotion. Stationary occupancy and its matching reservation SHALL count the same actor once per cell.

#### Scenario: Opposing melee units meet
- **WHEN** a Swordsman and an enemy approach from opposite entrances
- **THEN** they stop in distinct adjacent hexes and can attack at range one
- **AND** subsequent steps cannot merge their hexes or carry them through each other

#### Scenario: Allies share capacity
- **WHEN** a six-capacity hex contains configured same-team units of sizes two, two and one
- **THEN** the occupied size is five and another configured size-one ally can fit regardless of visual anchor layout
- **AND** a size-two arrival and every opposing arrival are refused while that occupancy remains

#### Scenario: Tens of units converge
- **WHEN** an isolated non-roster workload has at least thirty-two soldiers and thirty-two enemies converging on one city battlefield
- **THEN** deployed size reservations, movement reservations and entry queues remain bounded and conserved
- **AND** reachable opponents engage and the ordinary default battle completes without a stalled-fight defeat

#### Scenario: Blocker dies
- **WHEN** a front unit dies with allies waiting behind it
- **THEN** it immediately becomes ineligible for attacks and targeting while its full size remains reserved through its death interval
- **AND** its positions become available at the authoritative death-end tick, allowing combat to continue

#### Scenario: Transfer enters an occupied approach
- **WHEN** living attackers are reassigned to a city whose entrance positions are occupied
- **THEN** all transferred identities belong to that destination immediately and use their protected rear deployment fallback when the forward band is unavailable
- **AND** waiting arrivals remain counted in the active wave and admission occurs within the protected size-release bound rather than relying on a stalled-fight outcome

#### Scenario: Three ordinary units share a hex
- **WHEN** three ordinary same-team units occupy one hex
- **THEN** their size-two claims total six and another arrival cannot fit until enough size is released

#### Scenario: Boss consumes the whole hex
- **WHEN** a size-six boss occupies or reserves a hex
- **THEN** no other actor can occupy or reserve that hex until the boss claim is released

#### Scenario: Mixed sizes are not fragmented
- **WHEN** a legal same-team hex contains size two and an actor of size four requests admission
- **THEN** admission can use the remaining four capacity without requiring a neighboring-position footprint

### Requirement: Specified automatic formation

For automatic enemy and isolated non-roster formation, at wave setup the authority SHALL first allocate rear-band placement to ranged/magic support, then forward-band placement to melee, activating the completed formation together. Within a tier, lower initiative then seeded actor ties SHALL determine placement order. Each placement SHALL choose a compatible cell with the least occupied/reserved capacity, then the declared owner-relative column order; within that cell it SHALL assign a deterministic distinct graphical anchor without requiring a shaped footprint. Support SHALL NOT fill spare frontline cells during setup, and melee overflow SHALL NOT displace allocated support. Excess units SHALL remain queued. During combat queued melee SHALL prefer legal forward-band capacity then its protected rear fallback, while queued support SHALL use protected rear capacity; queue ordering SHALL use melee/support tier, initiative and persistent seeded ties, scanning past currently non-fitting actors for the first actor that fits a legal cell; skipped actors SHALL retain their identities and tie state. Ready deployed units SHALL follow ordinary closest-target/route rules across columns, hold when in range and make no free intra-hex rearrangements.

#### Scenario: Spread before filling a frontline hex
- **WHEN** compatible empty forward cells exist in three declared columns and at least three melee units can deploy
- **THEN** one melee unit is assigned to each column before another is packed into a used frontline hex
- **AND** repeated equivalent setup/seed and reversed storage yield the same assigned size reservations

#### Scenario: Keep support behind initial melee
- **WHEN** a mixed army fits its declared forward and rear bands
- **THEN** melee occupies the forward band and support occupies the rear band at distinct legal positions
- **AND** support does not fill an unused forward position and receives no earlier action merely because its placement was allocated first

#### Scenario: Mixed formation provides melee access
- **WHEN** a crowded mixed formation includes queued melee and deployed support while opponents remain
- **THEN** available legal capacity admits a queued melee unit and it reaches an attack position within the fixture's declared bound
- **AND** admission does not delete support, move it for free or depend on BattleStalled

#### Scenario: Larger queued actor does not block a fitting arrival
- **WHEN** a size-six actor precedes a size-two actor in the canonical queue and a legal cell has only two free size
- **THEN** the size-two actor is admitted and the size-six actor remains queued without lost identity or changed retry ties

Allied city-roster units SHALL instead use persistent purchased first-fit homes under army-roster. Combat movement and later legal admission SHALL retain ordinary capacity/team/protection rules. Existing spread and tier scenarios SHALL remain applicable to automatic enemy and isolated non-roster formation, not override chosen allied homes.


### Requirement: Stable combat identities and session lifetime

Soldiers and enemies SHALL have match-scoped stable identities that are never reused. Living soldiers SHALL retain identity, type and remaining health between waves. Redistribution SHALL retain a living enemy's identity, type, health, origin, effective profile and remaining attack recovery while cancelling its former destination's pending attack, move and route. Its old movement/position reservations SHALL be released as it is removed from that battlefield, and destination admission SHALL create new reservations only when legal. Dying enemies SHALL NOT transfer or count as living wave enemies; their death reservations SHALL remain with their former battlefield until expiry. Combat units, reservations, decision state, dying bodies and event history SHALL have one authority owner and SHALL be released on session end or application exit. Fresh matches SHALL have independent state; delayed updates SHALL NOT affect another match. Presentation nodes and transport identities SHALL NOT serve as authoritative combat identities.

#### Scenario: Storage reuses an internal slot
- **WHEN** a completed casualty is removed and a later unit is created
- **THEN** the new unit has a different identity and cannot inherit its actions, tie choices or reservations

#### Scenario: Moving enemy transfers
- **WHEN** a city's elimination transfers an enemy that has reserved a source and destination
- **THEN** both old movement reservations are released, the move is cancelled and the identity appears once in the receiving city's allocation
- **AND** its health, profile and remaining attack recovery are preserved

#### Scenario: Fresh session after battle
- **WHEN** a process ends a match with living units, moving units, dying bodies and recent events and starts another match
- **THEN** the new match contains only its own roster and units and the old match's combat resources are released

City-roster allied survivors SHALL restore their exact persistent home after retained death cleanup under army-roster, including final victory, without healing or reformation. Stored units SHALL retain identity/health while remaining noncombatants.
