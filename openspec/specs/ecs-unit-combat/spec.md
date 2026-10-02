# ecs-unit-combat Specification

## Purpose

Keep automatic unit battles deterministic and readable through reliable lane contact, timed attacks, stable identities and complete combat lifecycle handling.

## Requirements

### Requirement: Bounded lane contact and formation spacing

Each living city SHALL have a bounded authoritative hex battlefield with declared adjacency and fixed positions within each hex. Each unit type SHALL declare an integer capacity cost; placement SHALL occupy a legal footprint of that many positions. Allied units SHALL be allowed to share a hex at distinct positions within capacity. Occupancy and reservations in one hex SHALL belong to at most one faction, including dying bodies. Units SHALL NOT share a position, exceed capacity, enter an opposing occupied/reserved hex, exchange places through one another, or leave the board. Initial formation and transferred arrivals SHALL use these same rules and permanent faction-specific deployment protection. Melee SHALL start in the forward band with rear placement allocated to support; placement SHALL spread across available columns before packing a cell. Overflow SHALL queue without taking the other tier's allocated setup positions. Arrivals awaiting room SHALL remain accounted for in their destination's army or enemy allocation without duplication, deletion, targeting or attacking. Board adjacency SHALL determine movement and distance; rendered geometry, scenery collision and player movement orders SHALL NOT determine combat. Blocked units SHALL wait without locomotion.

#### Scenario: Opposing melee units meet
- **WHEN** a Swordsman and an enemy approach from opposite entrances
- **THEN** they stop in distinct adjacent hexes and can attack at range one
- **AND** subsequent steps cannot merge their hexes or carry them through each other

#### Scenario: Allies share capacity
- **WHEN** a six-position hex contains two allies of capacity cost two and one ally of cost one
- **THEN** five positions are owned by distinct legal footprints and another cost-one ally can fit
- **AND** a cost-two arrival and every opposing arrival are refused while that occupancy remains

#### Scenario: Tens of units converge
- **WHEN** at least thirty-two soldiers and thirty-two enemies converge on one city battlefield
- **THEN** deployed footprints, movement reservations and entry queues remain bounded and conserved
- **AND** reachable opponents engage and the ordinary default battle completes without a stalled-fight defeat

#### Scenario: Blocker dies
- **WHEN** a front unit dies with allies waiting behind it
- **THEN** it immediately becomes ineligible for attacks and targeting while its footprint remains reserved through its death interval
- **AND** its positions become available at the authoritative death-end tick, allowing combat to continue

#### Scenario: Transfer enters an occupied approach
- **WHEN** living attackers are reassigned to a city whose entrance positions are occupied
- **THEN** all transferred identities belong to that destination immediately and use their protected rear deployment fallback when the forward band is unavailable
- **AND** waiting arrivals remain counted in the active wave and admission occurs within the protected-footprint release bound rather than relying on a stalled-fight outcome

### Requirement: Deterministic targets and timed attack resolution

Combat SHALL advance at 60 simulation ticks per second. A ready deployed unit SHALL attack when a living deployed opponent in its destination is in range, selecting the smallest hex distance, then lowest target initiative, then a seeded random tie. Melee attacks SHALL require distance exactly one; ranged and magic attacks SHALL use integer hex ranges of at least one. Units SHALL reselect at the next attack boundary and SHALL NOT retain a farther target solely because it was previously selected. An attack SHALL lock its primary identity and have a unique action identity, start tick, positive windup, impact tick and positive recovery interval. The attacker SHALL hold position through windup and recovery. At impact the authority SHALL validate living attacker, living target, destination and range, applying at most one hit per victim per action. All valid unit and defense damage due on a tick SHALL accumulate before casualties. An invalid primary SHALL cause a miss with normal recovery and no replacement hit. Hit reactions, projectiles and animation callbacks SHALL NOT alter numerical action timing.

#### Scenario: Closest target precedes initiative
- **WHEN** a ready ranged unit can attack a high-initiative enemy one hex away and a low-initiative enemy two hexes away
- **THEN** it selects the enemy one hex away

#### Scenario: Equal distance uses lower initiative
- **WHEN** two in-range opponents have the same hex distance but different initiative
- **THEN** the lower-initiative opponent is selected

#### Scenario: Target changes between attacks
- **WHEN** a closer opponent becomes available while an attack is winding up
- **THEN** the current attack retains its locked primary
- **AND** the next attack selects the closest eligible opponent using the current state

#### Scenario: Simultaneous lethal strikes
- **WHEN** two living opponents deliver lethal valid impacts on the same tick
- **THEN** both hits resolve and both units become dying, regardless of initiative

#### Scenario: Target disappears during windup
- **WHEN** a locked target dies, transfers or leaves range before impact
- **THEN** the attack misses without damaging a replacement and retains its scheduled recovery
- **AND** the attacker can select another target when ready

#### Scenario: Different storage traversal
- **WHEN** identical canonical units, seed, configuration and ordered requests are simulated with different internal insertion or traversal orders
- **THEN** ordered locations, health, targets, attack identities and outcome agree at each tick

#### Scenario: Splash respects impact identity
- **WHEN** one splash attack impacts three eligible victims
- **THEN** each victim receives at most one hit for that action on its impact tick
- **AND** duplicate snapshots cannot apply additional damage

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

### Requirement: Committed movement and single attackable location

A ready unit without any in-range opponent SHALL seek a legal route to an attack position. It SHALL rank currently reachable opponents by the fewest route steps to such a position, then target initiative and a seeded tie. Temporarily blocked objectives SHALL permit bounded waiting. Units SHALL NOT be assigned permanent combat lanes. For equal shortest approach positions, ranged/magic units SHALL prefer a position screened by a living friendly melee hex on a shortest path to the selected opponent; remaining equal goals SHALL prefer lower occupied/reserved capacity, then seeded ties. These preferences SHALL NOT override closest-target selection, a shorter route, in-range holding or committed actions. Accepted moves SHALL traverse one adjacent hex in a positive configured number of ticks, retaining the source footprint and reserving the destination footprint and required transit conflicts atomically. Move arbitration SHALL use lower actor initiative first and seeded ordering for equal initiative. A moving unit SHALL be attackable only at its source hex until arrival; arrival SHALL switch its attackable location to the destination and release source/transit reservations. Units SHALL NOT attack or change route during a committed step. Independent accepted actions SHALL be allowed to begin on the same tick without artificial initiative-based frame delays. A unit with an in-range opponent SHALL hold its position, including while recovering, rather than retreat to a preferred distance.

#### Scenario: Competing movement requests
- **WHEN** two ready allies require the same remaining destination footprint
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

### Requirement: Seeded fight reproducibility

The authority SHALL retain and expose a non-secret combat seed and versioned immutable combat configuration. Equivalent canonical setup, seed, board/configuration version and ordered gameplay inputs SHALL produce identical ordered combat states, decisions, events and outcome on supported runtimes, independent of entity storage order, session GUID, rendering cadence and cosmetic effects. All random combat choices SHALL use a specified reproducible algorithm with stable decision identities and canonical candidate ordering. Exact target, scheduling and route ties SHALL use seeded randomness; deterministic identity order SHALL only canonicalize candidates and resolve algorithmic collisions. Re-observing or retrying an unchanged blocked decision SHALL NOT reroll its choice. Reconnect SHALL preserve authoritative decision progress; guests SHALL NOT independently choose combat actions.

#### Scenario: Complete combat trace under reversed insertion
- **WHEN** the same canonical units, seed, rules and requests run with reversed entity insertion order
- **THEN** their hexes, footprints, reservations, health, targets, actions, dying bodies, events and outcome agree at every simulation tick

#### Scenario: Exact target tie
- **WHEN** equal-distance opponents have equal initiative
- **THEN** the choice is reproducible from the fight seed and decision identity
- **AND** an unchanged retry preserves that choice while specified alternate seeds can choose a different tied candidate

#### Scenario: Presentation does not consume combat randomness
- **WHEN** equivalent fights use different session GUIDs and clients observe them at different frame rates
- **THEN** their normalized combat traces and outcomes remain identical

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

Capacity/footprints, type initiative, integer hex range, move duration, windup, recovery, death duration, bounded retry timing, splash cap/radius and fight limits SHALL come from validated configuration frozen for a fight. Invalid durations, illegal footprints, unusable protected entrances/frontiers or malformed board connectivity SHALL be rejected before combat. The virtual city-defense distance anchor SHALL be attackable from legal neutral siege positions without entering opposing protected cells and SHALL NOT provide a transit shortcut. Route/target decisions SHALL occur at action boundaries and relevant board-state changes rather than rendering frames. Movement SHALL advance along a committed route toward an attack position; unchanged blocking SHALL cause a bounded wait instead of repeated rerolls or locomotion. Every active city engagement SHALL have a positive configurable no-health-progress limit, and every wave SHALL have a positive maximum combat duration. Only actual unit/city health reduction in the engagement SHALL reset its no-progress deadline; movement, misses, zero-damage hits and redistribution SHALL NOT keep resetting it. A newly active engagement SHALL receive its declared initial allowance. At a limit, if normal health defeat or wave completion has not already resolved that tick, the whole match SHALL enter defeat with a battle-stalled reason and diagnostic city/limit/tick, without artificial damage or casualty creation. Cleanup after a resolved wave SHALL NOT trigger a stalled-fight defeat. Ordinary cleared-board reinforcement admission SHALL pass its protected-entry bound and reach engagement without consuming the stall allowance; conserving an indefinitely queued allocation SHALL NOT satisfy progression acceptance.

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
- **WHEN** configuration specifies a zero-duration move, an attack with no recovery, or a footprint larger than every entry position permits
- **THEN** the authority rejects that configuration before starting combat

### Requirement: Explicit same-tick combat ordering

At each combat simulation tick the authority SHALL first complete due movement arrivals, recoveries and death-space releases; then validate all due impacts against a common state and apply their accumulated damage; then mark casualties, resolve city elimination and transfers, and admit queued entries; then resolve normal match/wave results and fight limits; then gather ready decisions and arbitrate/start compatible actions. Units killed that tick SHALL NOT start a new action. Units completing movement that tick SHALL use their new location for impacts and later decisions. Initiative SHALL order conflicting action admission without changing simultaneous impact semantics. Event ordering SHALL be canonical and reproducible. After combat actions stop, cleanup ticks SHALL only expire retained deaths and resolve an eligible pending next-wave readiness barrier.

#### Scenario: Arrival precedes impact
- **WHEN** a target arrives in another hex on the same tick as a locked attack's impact
- **THEN** range validation uses its destination hex and produces the specified hit or miss

#### Scenario: Casualty does not start another action
- **WHEN** a unit's recovery ends on a tick where due damage kills it
- **THEN** it becomes dying without starting a new attack or move

#### Scenario: Released space is usable that tick
- **WHEN** a corpse's death interval expires while a ready ally waits for its position
- **THEN** later admission/decision phases on that tick can use the released capacity

### Requirement: Protected deployment and eventual cleared-board admission

Each faction SHALL have permanently protected deployment cells with legal entry footprints for every allowed unit profile. Opponents SHALL NOT occupy, reserve or transit those cells at any time, including after the battlefield clears. Protection SHALL restrict occupation only; deployed occupants SHALL remain fully attackable with ordinary health/range/timing, and protection SHALL grant no damage immunity. Combat admissions SHALL use available owner-protected positions when their forward formation band is blocked. In a previously cleared enemy allocation, surviving defenders SHALL NOT be able to block protected enemy entry. With any fitting protected footprint free, at least one queued reinforcement SHALL deploy in the current admission phase. If only unexpired death reservations prevent fitting entry, first admission SHALL occur no later than the first death-release tick that supplies a compatible footprint. The authority SHALL calculate and retain this first-admission bound from actual footprint masks and cumulative complete reservation releases at existing death deadlines; free-capacity totals or the first unrelated corpse expiry SHALL NOT substitute. The bound SHALL be no later than the latest relevant retained death deadline and SHALL NOT advance on unchanged retries. This guarantee SHALL apply to first admission; further overflow SHALL obey ordinary capacity and queue rules. Queued enemies SHALL remain counted during that finite delay. Default reinforcement fixtures SHALL establish subsequent attack/health progress and ordinary completion, not merely conserve identities or reach BattleStalled.

#### Scenario: Defenders hold the enemy forward band
- **WHEN** a city cleared its enemies and surviving defenders occupy the neutral forward enemy-entry cells when reinforcements transfer in
- **THEN** defenders have not occupied or reserved protected enemy cells and the first fitting reinforcement deploys there in the transfer tick's admission phase
- **AND** deployed reinforcements engage normally without requiring defender withdrawal, fabricated damage or BattleStalled

#### Scenario: Protected entry contains only retained deaths
- **WHEN** transferred enemies await protected entry positions held by unexpired enemy death reservations on a cleared board
- **THEN** the next compatible release tick provides an explicit first-admission bound
- **AND** admission occurs by that tick and the allocation cannot falsely complete while queued

#### Scenario: Early corpse expiry does not supply a fitting footprint
- **WHEN** the first protected-entry death expiry leaves fragmented positions that fit none of the queued profiles, but a later expiry supplies a legal footprint
- **THEN** the first-admission bound is the later compatible release tick, considering cumulative releases of complete reservations
- **AND** unchanged admission retries retain that bound and at least one reinforcement deploys at that tick

#### Scenario: Protected positions are still attackable
- **WHEN** a deployed reinforcement stands in its protected hex adjacent to a defending opponent
- **THEN** each unit can attack the other using its ordinary range-one rule and positive attack timers
- **AND** the defender cannot move into or reserve the protected hex after the reinforcement dies

#### Scenario: City siege respects protected deployment
- **WHEN** the defending army is gone and enemy melee approaches the city
- **THEN** a legal neutral siege position puts the virtual city target within range one
- **AND** enemy city attacks require no occupation of protected allied deployment cells

### Requirement: Specified automatic formation

At wave setup the authority SHALL first allocate rear-band placement to ranged/magic support, then forward-band placement to melee, activating the completed formation together. Within a tier, lower initiative then seeded actor ties SHALL determine placement order. Each placement SHALL choose a compatible cell with the least occupied/reserved capacity, then the declared owner-relative column order; within that cell it SHALL choose the forwardmost legal footprint then declared footprint index. Support SHALL NOT fill spare frontline cells during setup, and melee overflow SHALL NOT displace allocated support. Excess units SHALL remain queued. During combat queued melee SHALL prefer legal forward-band capacity then its protected rear fallback, while queued support SHALL use protected rear capacity; queue ordering SHALL use melee/support tier, initiative and persistent seeded ties. Ready deployed units SHALL follow ordinary closest-target/route rules across columns, hold when in range and make no free intra-hex rearrangements.

#### Scenario: Spread before filling a frontline hex
- **WHEN** compatible empty forward cells exist in three declared columns and at least three melee units can deploy
- **THEN** one melee unit is assigned to each column before another is packed into a used frontline hex
- **AND** repeated equivalent setup/seed and reversed storage yield the same assigned footprints

#### Scenario: Keep support behind initial melee
- **WHEN** a mixed army fits its declared forward and rear bands
- **THEN** melee occupies the forward band and support occupies the rear band at distinct legal positions
- **AND** support does not fill an unused forward position and receives no earlier action merely because its placement was allocated first

#### Scenario: Mixed formation provides melee access
- **WHEN** a crowded mixed formation includes queued melee and deployed support while opponents remain
- **THEN** available legal capacity admits a queued melee unit and it reaches an attack position within the fixture's declared bound
- **AND** admission does not delete support, move it for free or depend on BattleStalled

### Requirement: Consistent current action and reservation evidence

Each combat unit SHALL expose one consistent current action identity, kind, target where applicable, and authoritative timing interval. Current-state snapshots, events, attack eligibility and reconstructed reservations SHALL agree on that action. A committed move SHALL own one source, destination and transition interval; an attack SHALL own one locked primary, impact deadline and recovery deadline. A miss SHALL resolve the impact once and retain normal recovery. Queued, dying and terminally frozen units SHALL NOT execute a pending action; retained pose and reservation evidence SHALL still reconstruct their declared frozen state. Invalid externally supplied numerical states SHALL be rejected before partially replacing valid unit state or reservations.

#### Scenario: Rejected movement leaves the standing action intact
- **WHEN** a standing unit requests a move whose footprint or transit token conflicts with an existing reservation
- **THEN** its current action identity, position and timing remain unchanged
- **AND** no source, destination or transit reservation is partially acquired

#### Scenario: Impact resolves once into recovery
- **WHEN** a unit's locked impact deadline is reached with an invalid primary
- **THEN** one missed impact is reported and the unit retains its original recovery deadline
- **AND** later ticks and repeated observations cannot apply the same impact again

#### Scenario: Moving casualty reconstructs from current state
- **WHEN** a unit dies during movement and a complete current-state snapshot is observed without historical events
- **THEN** its action identity, frozen progress and both endpoint/transit reservations agree
- **AND** it cannot arrive or impact while those reservations remain held until death expiry

#### Scenario: Transfer preserves recovery without preserving the old attack
- **WHEN** an enemy transfers during attack recovery
- **THEN** its old primary and pending action are cancelled while its remaining ready deadline is preserved
- **AND** receiving-field admission cannot allow it to attack before that deadline

### Requirement: Current movement objective ranking at eligible boundaries

A ready unit without an in-range opponent SHALL reevaluate all eligible opponents at action boundaries and relevant changes to its battlefield's target eligibility, attackable locations or compatible approach capacity. It SHALL prefer a currently reachable attack position requiring fewer steps, then lower target initiative, then a reproducible seeded exact tie. A retained objective or cached route SHALL NOT override a strictly better current target score. Committed movement, attack windup and recovery SHALL retain their declared completion semantics. If the selected objective episode remains unchanged, route repair SHALL preserve its anti-cycling history and bounded waiting; a changed objective episode SHALL NOT inherit exclusions belonging only to the previous objective. An unchanged blocked retry SHALL preserve its decision identity, seeded scheduling rank and choice; reaching a retry deadline or changing presentation cadence alone SHALL NOT create a new choice.

#### Scenario: Closer opponent becomes available between committed steps
- **WHEN** a swordsman at cell 17 completes a move while retaining an objective at cell 2 and an eligible opponent at cell 8 offers a shorter legal approach on the default board
- **THEN** the next eligible movement decision selects the opponent at cell 8
- **AND** the former committed step completes at its original arrival tick

#### Scenario: Closer opponent enters during windup
- **WHEN** a closer opponent becomes available while a unit is winding up or recovering
- **THEN** the unit completes its locked attack and recovery normally
- **AND** its next eligible decision uses current distance or approach ranking

#### Scenario: Previously blocked better approach becomes reachable
- **WHEN** compatible footprint space becomes available for a shorter approach to another opponent
- **THEN** a ready actor evaluates that opponent before committing another step toward a farther retained objective

#### Scenario: New objective needs a former episode's visited cell
- **WHEN** a changed best objective has a legal approach through a cell excluded only by the former objective episode
- **THEN** the former episode's exclusion does not hide that objective or its new legal route

#### Scenario: Identical blocked retries remain identical
- **WHEN** an actor retries an unchanged blocked decision across its retry deadlines
- **THEN** its decision identity, objective, seeded rank and route choice remain unchanged
- **AND** it waits without locomotion or new reservations

#### Scenario: Another city changes independently
- **WHEN** another city's units move while this actor's local decision inputs remain unchanged
- **THEN** that movement alone does not reroll this actor's blocked choice
