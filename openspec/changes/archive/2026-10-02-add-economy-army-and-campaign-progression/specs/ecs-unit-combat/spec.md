# Spec Delta

## MODIFIED Requirements

### Requirement: Bounded lane contact and formation spacing

Each living city SHALL have a bounded authoritative hex battlefield with declared adjacency and exactly six size capacity per hex. Every unit SHALL have a frozen integer size from one through six. All ordinary archetypes SHALL use size two, bosses size six, and no current archetype SHALL use size one. Same-team units SHALL share a hex whenever their occupied and reserved sizes total at most six; a geometrical footprint shape or rendered anchor arrangement SHALL NOT reject an otherwise legal fit. Occupancy and reservations in one hex SHALL belong to at most one team, allies or opponents, including dying bodies and move reservations. Units SHALL NOT exceed capacity, enter an opposing occupied/reserved hex, exchange places through one another or leave the board. Distinct graphical anchors SHALL preserve readability without adding a numerical capacity restriction. Initial formation and transferred arrivals SHALL obey the same size rules and permanent faction-specific deployment protection. Melee SHALL start forward with rear placement allocated to support; placement SHALL spread across columns before packing a cell. Overflow SHALL queue without taking the other tier's allocated setup capacity. Queued actors SHALL retain their destination army/enemy membership without duplication, deletion, spatial claims, targeting or attacks. Board adjacency SHALL determine movement and distance; rendered geometry, scenery collision and player orders SHALL NOT determine combat. Blocked units SHALL wait without locomotion. Stationary occupancy and its matching reservation SHALL count the same actor once per cell.

#### Scenario: Opposing melee units meet
- **WHEN** a Swordsman and an enemy approach from opposite entrances
- **THEN** they stop in distinct adjacent hexes and can attack at range one
- **AND** subsequent steps cannot merge their hexes or carry them through each other

#### Scenario: Allies share capacity
- **WHEN** a six-capacity hex contains configured same-team units of sizes two, two and one
- **THEN** the occupied size is five and another configured size-one ally can fit regardless of visual anchor layout
- **AND** a size-two arrival and every opposing arrival are refused while that occupancy remains

#### Scenario: Tens of units converge
- **WHEN** at least thirty-two soldiers and thirty-two enemies converge on one city battlefield
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


### Requirement: Committed movement and single attackable location

A ready unit without any in-range opponent SHALL seek a legal route to an attack position. It SHALL rank currently reachable opponents by the fewest route steps to such a position, then target initiative and a seeded tie. Temporarily blocked objectives SHALL permit bounded waiting. Units SHALL NOT be assigned permanent combat lanes. For equal shortest approach positions, ranged/magic units SHALL prefer a position screened by a living friendly melee hex on a shortest path to the selected opponent; remaining equal goals SHALL prefer lower occupied/reserved capacity, then seeded ties. These preferences SHALL NOT override closest-target selection, a shorter route, in-range holding or committed actions. Accepted moves SHALL traverse one adjacent hex in a positive configured number of ticks, retaining the actor's full size at the source and reserving its full size at the destination and required transit conflicts atomically. Move arbitration SHALL use lower actor initiative first and seeded ordering for equal initiative. A moving unit SHALL be attackable only at its source hex until arrival; arrival SHALL switch its attackable location to the destination and release source/transit reservations. Units SHALL NOT attack or change route during a committed step. Independent accepted actions SHALL be allowed to begin on the same tick without artificial initiative-based frame delays. A unit with an in-range opponent SHALL hold its position, including while recovering, rather than retreat to a preferred distance.

#### Scenario: Competing movement requests
- **WHEN** two ready allies require the same remaining destination capacity
- **THEN** the lower-initiative unit reserves it and begins moving while the other waits
- **AND** independent movers can begin on that same tick

#### Scenario: Attack during movement
- **WHEN** a unit moves from A to B starting at tick 100 with arrival at tick 130
- **THEN** impacts before tick 130 evaluate it at A and impacts on tick 130 evaluate it at B
- **AND** it is never attackable at both locations

#### Scenario: Move and opposing reservation conflict
- **WHEN** opposing movers compete for an empty hex or request conflicting transit routes
- **THEN** arbitration grants only a compatible faction/route set and rejected moves acquire no partial reservations

#### Scenario: Ranged unit is reached
- **WHEN** an opponent enters range one of a ranged unit
- **THEN** the ranged unit holds and uses its normal ranged attack profile rather than repeatedly retreating

#### Scenario: Destination capacity is reserved atomically
- **WHEN** two size-two allies request a cell with only two free size and conflicting admission
- **THEN** the winning move claims exactly two size there and the rejected move acquires no endpoint or transit claim
- **AND** the moving actor retains its source size until arrival and is counted once per endpoint


### Requirement: Authoritative death occupancy and current-state restoration

Lethal damage SHALL immediately cancel a unit's ability to act or be targeted and remove it from living army/enemy counts. Its dying state SHALL retain final position, action progress at death, start/end ticks and position reservations until its configured positive death duration expires. Death during movement SHALL freeze that movement and retain both endpoint and transit reservations until death expiry, preventing a visually occupied route from being reused. Space release SHALL depend only on the authority's simulation clock. Complete snapshots SHALL include all current moving and dying state needed to reconstruct occupancy and poses without historical events. Pause SHALL freeze action/death deadlines and no-progress clocks. A completed wave SHALL allow bounded authoritative death cleanup after combat actions cease and SHALL NOT reuse its board for the next wave before cleanup completes.

#### Scenario: Death animation finishes on schedule
- **WHEN** a unit dies at tick 200 with a configured 48-tick death duration
- **THEN** it cannot attack or be selected from tick 200 and retains its positions through tick 247
- **AND** tick 248 releases those positions even on a headless authority

#### Scenario: Death interrupts movement
- **WHEN** lethal damage occurs midway through an accepted move
- **THEN** the unit does not arrive, its pose freezes at the recorded move progress and both endpoint/transit reservations remain until death end

#### Scenario: Reconnect during a death interval
- **WHEN** a client synchronizes after death start but before death end
- **THEN** it reconstructs the current dying body at its elapsed pose and observes the same remaining reservations
- **AND** it does not replay the death sound or restart the death interval

### Requirement: Validated combat configuration and bounded progression

Capacity six, integer sizes one through six, type initiative, integer hex range, move duration, windup, recovery, death duration, bounded retry timing, splash cap/radius and fight limits SHALL come from validated configuration frozen for a fight. Invalid durations, invalid sizes or size claims, unusable protected entrances/frontiers or malformed board connectivity SHALL be rejected before combat. The virtual city-defense distance anchor SHALL be attackable from legal neutral siege positions without entering opposing protected cells and SHALL NOT provide a transit shortcut. Route/target decisions SHALL occur at action boundaries and relevant board-state changes rather than rendering frames. Movement SHALL advance along a committed route toward an attack position; unchanged blocking SHALL cause a bounded wait instead of repeated rerolls or locomotion. Every active city engagement SHALL have a positive configurable no-health-progress limit, and every wave SHALL have a positive maximum combat duration. Only actual unit/city health reduction in the engagement SHALL reset its no-progress deadline; movement, misses, zero-damage hits and redistribution SHALL NOT keep resetting it. A newly active engagement SHALL receive its declared initial allowance. At a limit, if normal health defeat or wave completion has not already resolved that tick, the whole match SHALL enter defeat with a battle-stalled reason and diagnostic city/limit/tick, without artificial damage or casualty creation. Cleanup after a resolved wave SHALL NOT trigger a stalled-fight defeat. Ordinary cleared-board reinforcement admission SHALL pass its protected-entry bound and reach engagement without consuming the stall allowance; conserving an indefinitely queued allocation SHALL NOT satisfy progression acceptance.

#### Scenario: Walking cannot extend a stalled battle
- **WHEN** units continue moving or waiting but their city engagement has no actual health reduction for its configured allowance
- **THEN** the entire match ends as defeat with a battle-stalled reason at the deterministic deadline
- **AND** surviving city and unit health are preserved

#### Scenario: Progress elsewhere cannot hide a stuck city
- **WHEN** one active city makes health progress while another reaches its own no-progress deadline
- **THEN** the match ends with a battle-stalled reason identifying the stuck city

#### Scenario: Final result on a limit tick
- **WHEN** the last living enemy dies on the same tick that a fight limit would expire and at least one city remains
- **THEN** ordinary wave completion takes precedence over the limit
- **AND** if all cities also fall, ordinary health defeat takes precedence over victory

#### Scenario: Invalid timing or capacity
- **WHEN** configuration specifies a zero-duration move, an attack with no recovery, or a size outside one through six or an unusable protected entry route
- **THEN** the authority rejects that configuration before starting combat

### Requirement: Protected deployment and eventual cleared-board admission

Each faction SHALL have permanently protected deployment cells and usable entry-to-engagement routes for every allowed unit size, including size six. Opponents SHALL NOT occupy, reserve or transit those cells, including after the battlefield clears. Protection SHALL restrict occupation only and SHALL grant no attack immunity. Admissions SHALL use owner-protected cells when the forward band is blocked. Surviving defenders SHALL NOT block protected enemy entry on a cleared board. If any queued actor fits the free size in one protected cell, at least one fitting reinforcement SHALL deploy in the current admission phase. Free capacity in different cells SHALL NOT be pooled. If retained deaths are the only obstacle, the first-admission bound SHALL be the earliest existing death-release tick at which cumulative complete actor releases supply enough free size in one legal cell for at least one queued actor. Visual position fragmentation SHALL NOT delay admission. The bound SHALL be no later than the latest relevant retained death deadline and SHALL NOT advance on unchanged retries. This guarantees first admission; subsequent overflow SHALL obey ordinary queues. Queued enemies SHALL remain counted during the delay. Default fixtures SHALL establish actual subsequent attack/health progress and ordinary completion rather than merely conserve identities or reach BattleStalled.

#### Scenario: Defenders hold the enemy forward band
- **WHEN** a city cleared its enemies and surviving defenders occupy the neutral forward enemy-entry cells when reinforcements transfer in
- **THEN** defenders have not occupied or reserved protected enemy cells and the first fitting reinforcement deploys there in the transfer tick's admission phase
- **AND** deployed reinforcements engage normally without requiring defender withdrawal, fabricated damage or BattleStalled

#### Scenario: Protected entry contains only retained deaths
- **WHEN** transferred enemies await protected entry positions held by unexpired enemy death reservations on a cleared board
- **THEN** the next compatible release tick provides an explicit first-admission bound
- **AND** admission occurs by that tick and the allocation cannot falsely complete while queued

#### Scenario: Early corpse expiry does not supply a fitting footprint
- **WHEN** the first protected-entry death expiry leaves insufficient free size in each cell for every queued actor, but a later expiry supplies enough size in one cell
- **THEN** the first-admission bound is the later compatible release tick, considering cumulative complete actor size releases
- **AND** unchanged admission retries retain that bound and at least one reinforcement deploys at that tick

#### Scenario: Protected positions are still attackable
- **WHEN** a deployed reinforcement stands in its protected hex adjacent to a defending opponent
- **THEN** each unit can attack the other using its ordinary range-one rule and positive attack timers
- **AND** the defender cannot move into or reserve the protected hex after the reinforcement dies

#### Scenario: City siege respects protected deployment
- **WHEN** the defending army is gone and enemy melee approaches the city
- **THEN** a legal neutral siege position puts the virtual city target within range one
- **AND** enemy city attacks require no occupation of protected allied deployment cells

#### Scenario: Free size cannot be pooled across cells
- **WHEN** a queued size-six boss has three free size in each of two protected cells
- **THEN** it remains queued until one legal cell has all six capacity available


### Requirement: Specified automatic formation

At wave setup the authority SHALL first allocate rear-band placement to ranged/magic support, then forward-band placement to melee, activating the completed formation together. Within a tier, lower initiative then seeded actor ties SHALL determine placement order. Each placement SHALL choose a compatible cell with the least occupied/reserved capacity, then the declared owner-relative column order; within that cell it SHALL assign a deterministic distinct graphical anchor without requiring a shaped footprint. Support SHALL NOT fill spare frontline cells during setup, and melee overflow SHALL NOT displace allocated support. Excess units SHALL remain queued. During combat queued melee SHALL prefer legal forward-band capacity then its protected rear fallback, while queued support SHALL use protected rear capacity; queue ordering SHALL use melee/support tier, initiative and persistent seeded ties, scanning past currently non-fitting actors for the first actor that fits a legal cell; skipped actors SHALL retain their identities and tie state. Ready deployed units SHALL follow ordinary closest-target/route rules across columns, hold when in range and make no free intra-hex rearrangements.

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

