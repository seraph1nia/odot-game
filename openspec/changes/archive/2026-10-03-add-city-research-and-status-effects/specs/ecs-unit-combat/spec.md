# Spec Delta

## MODIFIED Requirements

### Requirement: Deterministic targets and timed attack resolution

Combat SHALL advance at 60 simulation ticks per second. A ready deployed unit SHALL attack when a living deployed opponent in its destination is in range, selecting the smallest hex distance, then lowest target initiative, then a seeded random tie. Melee attacks SHALL require distance exactly one; ranged and magic attacks SHALL use integer hex ranges of at least one. Units SHALL reselect at the next attack boundary and SHALL NOT retain a farther target solely because it was previously selected. An attack SHALL lock its primary identity and have a unique action identity, start tick, positive windup, impact tick and positive recovery interval. Current chill SHALL be sampled only when committing a new action, and its resulting positive deadlines SHALL remain fixed. The attacker SHALL hold position through windup and recovery. At impact the authority SHALL validate living attacker, living target, destination and range, applying at most one hit per victim per action. All valid unit, defense and already-scheduled periodic status damage due on a tick SHALL accumulate before casualties. New on-hit status applications SHALL then affect surviving victims only, with no immediate recursive damage. An invalid primary SHALL cause a miss with normal recovery and no replacement hit. Hit reactions, projectiles and animation callbacks SHALL NOT alter numerical action timing.

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

### Requirement: Committed movement and single attackable location

A ready unit without any in-range opponent SHALL seek a legal route to an attack position. It SHALL rank currently reachable opponents by the fewest route steps to such a position, then target initiative and a seeded tie. Temporarily blocked objectives SHALL permit bounded waiting. Units SHALL NOT be assigned permanent combat lanes. For equal shortest approach positions, ranged/magic units SHALL prefer a position screened by a living friendly melee hex on a shortest path to the selected opponent; remaining equal goals SHALL prefer lower occupied/reserved capacity, then seeded ties. These preferences SHALL NOT override closest-target selection, a shorter route, in-range holding or committed actions. Accepted moves SHALL traverse one adjacent hex in a positive duration resolved from configured base timing and chill at movement commitment, retaining the actor's full size at the source and reserving its full size at the destination and required transit conflicts atomically. Move arbitration SHALL use lower actor initiative first and seeded ordering for equal initiative. A moving unit SHALL be attackable only at its source hex until arrival; arrival SHALL switch its attackable location to the destination and release source/transit reservations. Units SHALL NOT attack or change route during a committed step. Independent accepted actions SHALL be allowed to begin on the same tick without artificial initiative-based frame delays. A unit with an in-range opponent SHALL hold its position, including while recovering, rather than retreat to a preferred distance.

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

### Requirement: Explicit same-tick combat ordering

At each combat simulation tick the authority SHALL first complete due movement arrivals, recoveries and death-space releases; then gather already-scheduled periodic status damage and validate all due impacts against a common pre-damage state; then apply accumulated damage, expire due effects after any terminal periodic tick, commit canonically ordered new status applications only to surviving victims, and mark casualties; then resolve city elimination and transfers, and admit queued entries; then resolve normal match/wave results and fight limits; then gather ready decisions and arbitrate/start compatible actions. Units killed that tick SHALL NOT start a new action. Units completing movement that tick SHALL use their new location for impacts and later decisions. Initiative SHALL order conflicting action admission without changing simultaneous impact semantics. Event ordering SHALL be canonical and reproducible. After combat actions stop, cleanup ticks SHALL only expire retained deaths and resolve an eligible pending next-wave readiness barrier. Surviving temporary statuses SHALL clear at wave resolution, and cleanup SHALL apply no later status damage. Expired chill SHALL be absent before new action selection; positive health reduction from periodic damage SHALL count as health progress, while status applications without health loss SHALL not.

#### Scenario: Arrival precedes impact
- **WHEN** a target arrives in another hex on the same tick as a locked attack's impact
- **THEN** range validation uses its destination hex and produces the specified hit or miss

#### Scenario: Casualty does not start another action
- **WHEN** a unit's recovery ends on a tick where due damage kills it
- **THEN** it becomes dying without starting a new attack or move

#### Scenario: Released space is usable that tick
- **WHEN** a corpse's death interval expires while a ready ally waits for its position
- **THEN** later admission/decision phases on that tick can use the released capacity

#### Scenario: Periodic lethal damage does not suppress a due attack
- **WHEN** an otherwise eligible attacker has a valid impact due on the same tick that burn kills it
- **THEN** the due impact and burn resolve in the common damage phase before its casualty
- **AND** no new action starts after its death
