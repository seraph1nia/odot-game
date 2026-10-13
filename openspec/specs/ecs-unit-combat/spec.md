# ecs-unit-combat Specification

## Purpose

Keep automatic unit battles deterministic and readable through connected squad contact, timed attacks, stable identities and complete combat lifecycle handling.

## Requirements

### Requirement: Bounded lane contact and formation spacing

Combat SHALL use one connected hex arena for the original one-to-four-player roster, with adjacent player sectors, owned preparation homes, enemy entry cells and reserved physical structure footprints. The default geometry SHALL provide seven columns by nine rows per sector. Every living participating squad SHALL occupy exactly one exclusive logical cell, independent of its member count. Separate squads SHALL NOT stack. Only individual members of the same homogeneous squad SHALL share cosmetic formation offsets. Squads SHALL NOT occupy static structure/weapon cells or opposing occupied cells. Preparation ownership SHALL NOT form a movement barrier during combat. Member size budgets and city-isolated battlefields SHALL NOT govern combat. Queued actors SHALL retain membership without claims, attacks or targeting until legal admission.

#### Scenario: Connected four-player battle
- **WHEN** a four-player wave begins
- **THEN** all allied and enemy squads share one connected 252-cell arena and can reach neighboring sectors through legal cells
- **AND** the nine settlement plots per owner remain independent of combat topology

#### Scenario: Partial squad keeps its cell
- **WHEN** two members of a three-member squad die
- **THEN** its survivor retains that squad's exclusive cell and contributes only its own attacks
- **AND** another squad cannot occupy the cell until the last participant dies or the squad moves legally

#### Scenario: Opposing melee units meet
- **WHEN** a Swordsman and an enemy approach from opposite entrances
- **THEN** they stop in distinct adjacent hexes and can attack at range one
- **AND** subsequent steps cannot merge their hexes or carry them through each other

#### Scenario: Allies share capacity
- **WHEN** three funded same-type members belong to one squad
- **THEN** they share its one exclusive logical cell at distinct cosmetic offsets
- **AND** a separate friendly or enemy squad cannot stack there

#### Scenario: Tens of units converge
- **WHEN** a bounded connected-arena fixture groups at least thirty-two members on each side
- **THEN** exclusive squad claims, queue membership and identities remain conserved and reachable groups engage
- **AND** ordinary completion does not depend on BattleStalled

#### Scenario: Blocker dies
- **WHEN** the final participating member of a front squad dies with allies waiting
- **THEN** the squad releases its logical claims immediately while member cosmetics finish their death intervals

#### Scenario: Transfer enters an occupied approach
- **WHEN** living enemies redirect after Keep loss toward a congested neighboring sector
- **THEN** they retain current physical cells/actions and seek legal approaches through the connected arena
- **AND** no transfer deployment or corpse-release bound teleports them into that sector

#### Scenario: Three ordinary units share a hex
- **WHEN** three ordinary members of one homogeneous squad occupy a cell
- **THEN** they own one squad claim and a different actor cannot fit regardless of cosmetic spacing

#### Scenario: Boss consumes the whole hex
- **WHEN** a single-member boss squad occupies or commits to a cell
- **THEN** it has the same exclusive claim rule as an ordinary squad without a size-six budget

#### Scenario: Mixed sizes are not fragmented
- **WHEN** an ordinary squad and single-member boss request one free destination
- **THEN** one deterministic actor wins the exclusive destination
- **AND** body-size totals or visual offset shapes do not admit both actors

### Requirement: Deterministic targets and timed attack resolution

Combat SHALL advance at 60 simulation ticks per second using one squad-owned movement/target/attack cycle. Ready squads SHALL engage eligible in-range enemy groups before routing. Enemies SHALL also consider physically reachable Watchposts and Keeps while defenders elsewhere remain alive. Selection SHALL use frozen objective priorities, distance/reachability and seeded ties; sector preference SHALL bias structural objectives without excluding foreign opponents. An attack SHALL lock one target group or structure, action identity, start, impact and recovery deadlines. Living participating members SHALL contribute their resolved X/Y or structural profiles against the common pre-impact state. Committed actions SHALL retain deadlines; invalid group/structure/range SHALL produce one miss without a replacement target. Every due unit, weapon and periodic contribution SHALL accumulate before casualties. Animation and projectiles SHALL own no damage.

#### Scenario: Mutual lethal squad exchange
- **WHEN** two living squads have valid lethal impacts due on the same tick
- **THEN** all pre-impact living contributors deliver their strikes before both casualties are resolved
- **AND** traversal order cannot cancel the second squad's attack

#### Scenario: Locked group disappears
- **WHEN** the last participating member of the locked enemy squad dies before impact
- **THEN** the attack misses without switching to another group and retains its original recovery

#### Scenario: Closest target precedes initiative
- **WHEN** a ready ranged unit can attack a high-initiative enemy one hex away and a low-initiative enemy two hexes away
- **THEN** it selects the enemy one hex away

#### Scenario: Equal distance uses lower initiative
- **WHEN** opponent groups are equally distant at a ready targeting boundary
- **THEN** the frozen squad objective/distance/seeded tie policy selects reproducibly
- **AND** the former per-member target-initiative rule does not replace that policy; contested movement still uses actor initiative

#### Scenario: Target changes between attacks
- **WHEN** a closer opponent becomes available while an attack is winding up
- **THEN** the current attack retains its locked primary
- **AND** the next attack selects the closest eligible opponent using the current state

#### Scenario: Simultaneous lethal strikes
- **WHEN** two living opponents deliver lethal valid impacts on the same tick
- **THEN** both hits resolve and both units become dying, regardless of initiative

#### Scenario: Target disappears during windup
- **WHEN** the locked enemy group loses its final living participant or leaves physical range before impact
- **THEN** the attack misses without damaging another group and retains its scheduled recovery
- **AND** the next ready boundary may choose a new compatible target

#### Scenario: Different storage traversal
- **WHEN** identical canonical units, seed, configuration and ordered requests are simulated with different internal insertion or traversal orders
- **THEN** ordered locations, health, targets, attack identities and outcome agree at each tick

#### Scenario: Splash respects impact identity
- **WHEN** several member contributions from one committed group action hit the same victim
- **THEN** each contributor resolves its own X/Y once and repeated observations cannot add any contribution

### Requirement: Stable combat identities and session lifetime

Squad and member identities SHALL be separate match-scoped identities that are never reused. Living roster members SHALL retain archetype, equipment level, HP, service experience and ownership across waves. Regrouping SHALL preserve member identity while changing only its legal group/assignment. Redirection after Keep loss SHALL preserve enemy group/member identities, physical source, health, profiles and already committed timing; it SHALL change future objective preference without teleportation or new deployment admission. Dying members SHALL not redirect or count as living wave enemies. Session end SHALL release combat actors, claims, dying bodies and event state. Fresh sessions SHALL be independent; presentation/transport identities SHALL not replace authoritative identities.

#### Scenario: Keep falls during enemy movement
- **WHEN** an enemy squad is moving as its preferred Keep falls
- **THEN** its source and committed destination/deadline remain unchanged while future objective preference redirects to a surviving Keep
- **AND** no member is duplicated, healed or teleported

#### Scenario: Fresh session
- **WHEN** a match ends with living squads and retained deaths and a fresh match starts
- **THEN** only the fresh match's identities, actors, claims and effects exist

#### Scenario: Storage reuses an internal slot
- **WHEN** a completed casualty is removed and a later unit is created
- **THEN** the new unit has a different identity and cannot inherit its actions, tie choices or reservations

#### Scenario: Moving enemy transfers
- **WHEN** an enemy squad redirects while moving as its preferred Keep falls
- **THEN** its source, destination, arrival deadline and member HP stay unchanged
- **AND** new objective preference is evaluated only at the next eligible decision

#### Scenario: Fresh session after battle
- **WHEN** a process ends a match with living units, moving units, dying bodies and recent events and starts another match
- **THEN** the new match contains only its own roster and units and the old match's combat resources are released

### Requirement: Committed movement and single attackable location

Squads SHALL commit one adjacent move at a time with positive configured duration. Arbitration SHALL choose one destination winner using lower actor initiative and seeded ties. Friendly dependency chains, exchanges and cycles SHALL commit together with a common arrival deadline; unrelated moves SHALL retain their independent durations. Opposing exchanges and conflicts with nonmoving occupants/pending destinations SHALL be refused without partial claims. A moving squad SHALL remain attackable at its source until arrival, then switch once to its destination. No attack or route change SHALL interrupt a committed step. Ready squads with eligible in-range targets SHALL hold and attack, including ranged squads reached in melee.

#### Scenario: Friendly exchange
- **WHEN** friendly squads request each other's cells legally
- **THEN** both commit a coordinated exchange and arrive together with exclusive final occupancy
- **AND** an unrelated shorter move completes at its own deadline

#### Scenario: Opposing exchange
- **WHEN** opposing squads request each other's occupied cells
- **THEN** both exchanges are refused and their previous claims/actions remain intact

#### Scenario: Impact on arrival
- **WHEN** a target's move arrives on an attack's impact tick
- **THEN** that tick's range validation uses the destination, while earlier impacts use only the source

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
- **WHEN** two friendly squads request the same free adjacent destination
- **THEN** one winner acquires a coherent transition while the other preserves its standing action
- **AND** their source positions remain attackable until their declared arrivals

### Requirement: Seeded fight reproducibility

The authority SHALL expose a non-secret combat seed and immutable versioned configuration. Equivalent canonical roster, arena, seed, rules and ordered gameplay inputs SHALL produce identical ordered actor/member states, claims, events and outcomes independent of storage traversal, session GUID and presentation cadence. Seeded target, contribution-primary, movement and route ties SHALL use stable decision identities and canonical candidate ordering. Unchanged blocked observations SHALL not reroll a decision. Reconnect SHALL preserve current decision progress; clients SHALL never choose authoritative actions or consume combat randomness for scenery.

#### Scenario: Equivalent observations
- **WHEN** equivalent fights use different storage order, session GUIDs or frame rates
- **THEN** normalized actions, victims, claims and outcomes agree

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

Lethal member damage SHALL immediately remove that member's contribution and targeting eligibility. The last living participant's death SHALL immediately release the squad's logical source/transit claims and prevent further actions. Cosmetic deaths SHALL retain final member position/facing, frozen movement progress and positive start/end deadlines without blocking any logical cell. A death clip SHALL advance on the shared presentation clock while physical position remains frozen. Complete current state SHALL restore retained deaths without old events, restarting intervals or replaying sounds. Pause SHALL freeze simulation deadlines; wave outcomes SHALL not wait for bodies. Bounded post-wave cosmetic cleanup SHALL complete before the next formation is initialized.

#### Scenario: Last member dies while moving
- **WHEN** the final participant dies during a committed move
- **THEN** logical claims are released on that death tick, another legal squad may use the space, and the casualty remains at its frozen cosmetic pose until expiry

#### Scenario: Reconnect during death
- **WHEN** a peer restores a current unexpired casualty
- **THEN** it samples the remaining death interval without creating claims, restarting the clip or replaying the death sound

#### Scenario: Death animation finishes on schedule
- **WHEN** the last member dies at tick200 with death duration48
- **THEN** logical claims release at200 and the body remains frozen physically with its sampled clip through247
- **AND** tick248 removes the body even if the authority is headless

#### Scenario: Death interrupts movement
- **WHEN** the last participant takes lethal damage midway through a committed move
- **THEN** it never arrives and its frozen body creates no endpoint/transit claims

#### Scenario: Reconnect during a death interval
- **WHEN** a client synchronizes after member death start but before expiry
- **THEN** current pose and remaining cosmetic timing restore without logical claims
- **AND** no old sound or restarted interval occurs

### Requirement: Validated combat configuration and bounded progression

Squad capacity, connected topology, structure footprints, X/Y and structural damage, ranges, initiative, movement/attack/death timings, status settings and positive fight limits SHALL be validated before combat and frozen for the match. Zero X or Y SHALL be valid; negative values, incompatible legacy boards and unsafe arithmetic SHALL be refused. Every active preferred-sector engagement and wave SHALL have bounded no-health-progress/duration handling. Actual member/structure health reduction SHALL count as progress; movement, misses, zero damage and redirection SHALL not fabricate progress. Normal health defeat or wave completion on a deadline tick SHALL precede stalled defeat. Stalled defeat SHALL preserve surviving health and identify its limit/tick without artificial casualties. Queued actors SHALL not falsely clear a wave.

#### Scenario: Invalid configuration
- **WHEN** configuration contains a nonpositive action duration, negative channel, duplicate footprint or incompatible legacy size-budget board
- **THEN** the authority refuses before a match starts or resources are spent

#### Scenario: Limit and normal result coincide
- **WHEN** the last enemy dies on a fight-limit tick
- **THEN** normal wave clear wins over the limit if a Keep survives, and all-Keeps-fallen defeat wins if none survives

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
- **WHEN** configuration has a nonpositive move/recovery, invalid squad capacity or disconnected/shared-footprint-invalid board
- **THEN** the authority refuses it before combat instead of applying the retired size/protected-entry schema

### Requirement: Explicit same-tick combat ordering

The authority SHALL complete due arrivals/recoveries/cosmetic expiry, gather due periodic damage and impacts against common pre-damage state, apply accumulated member/structure damage, commit surviving-victim effects and casualties, resolve Keep losses and nonteleporting redirection/admission, resolve normal outcomes/fight limits, then start compatible ready actions. A unit killed by due periodic damage SHALL still deliver an otherwise valid due impact. Casualties SHALL start no new action. Event order SHALL be canonical. Post-result cleanup SHALL expire bodies without new attacks, status damage, production or rewards.

#### Scenario: Periodic lethal damage and due impact
- **WHEN** burn kills a member on its valid impact tick
- **THEN** the due impact and burn both resolve before death and no new attack begins

#### Scenario: Logical release is immediate
- **WHEN** the last member dies and a compatible mover is waiting
- **THEN** the later decision phase may use the released cell without waiting for the death animation

#### Scenario: Arrival precedes impact
- **WHEN** a target arrives in another hex on the same tick as a locked attack's impact
- **THEN** range validation uses its destination hex and produces the specified hit or miss

#### Scenario: Casualty does not start another action
- **WHEN** a unit's recovery ends on a tick where due damage kills it
- **THEN** it becomes dying without starting a new attack or move

#### Scenario: Released space is usable that tick
- **WHEN** the last member dies while a ready ally needs its source cell
- **THEN** that tick's later legal admission/decision phase can use the released logical space before corpse expiry

#### Scenario: Periodic lethal damage does not suppress a due attack
- **WHEN** an otherwise eligible attacker has a valid impact due on the same tick that burn kills it
- **THEN** the due impact and burn resolve in the common damage phase before its casualty
- **AND** no new action starts after its death

### Requirement: Specified automatic formation

Enemy wave entries SHALL become homogeneous squads up to configured ordinary capacity; bosses SHALL each form one single-member squad. Members SHALL retain authored counts, levels and origin allocations. Squads SHALL enter distinct available configured enemy cells in deterministic order, with overflow queued without claims. Ally setup SHALL use persistent purchased squad homes and explicit owned preparation placement, preserving member formation slots. Different archetypes SHALL never be silently combined into one squad. Deployment zones SHALL be setup rules rather than permanent faction-protected combat barriers.

#### Scenario: Authored four-member entry
- **WHEN** an ordinary four-member entry forms with capacity three
- **THEN** it creates one three-member squad and one one-member squad with exactly four identities

#### Scenario: Persistent formation
- **WHEN** an owned squad starts another wave with surviving veterans and funded replacement members
- **THEN** it retains its chosen purchased home and each member's valid formation slot without class-based repacking or healing

#### Scenario: Spread before filling a frontline hex
- **WHEN** multiple enemy squads enter available configured enemy cells
- **THEN** each deployed group has a separate cell in deterministic order
- **AND** no two squads pack one cell and members retain their group formation slots

#### Scenario: Keep support behind initial melee
- **WHEN** an owner arranges melee and support squads in its purchased preparation homes
- **THEN** the chosen legal placements persist into combat without silent role-based repacking
- **AND** range and action profiles remain unchanged

#### Scenario: Mixed formation provides melee access
- **WHEN** a mixed reachable encounter contains a queued melee group and deployed support
- **THEN** a released legal entry cell admits the queued group and it reaches an attack within the declared bound
- **AND** support is not deleted or moved for free and BattleStalled is not success

#### Scenario: Larger queued actor does not block a fitting arrival
- **WHEN** ordinary and boss squads await available configured entry cells
- **THEN** each requires one complete exclusive cell regardless of body size
- **AND** canonical admission retains waiting identities and admits available actors without pooled size accounting

### Requirement: Consistent current action and reservation evidence

Each squad SHALL expose one consistent current action identity/kind/target/timing interval, and member projections SHALL agree with that shared cycle while retaining individual health/status/formation. Reconstructed claims SHALL use squad identities and current source/transit state, never repeated member cosmetics. Rejected moves SHALL preserve old actions and acquire no partial claim. An impact SHALL resolve once and retain recovery after a miss. Stored/unfunded/queued/dead actors SHALL have no active claims. Invalid received actor/member/geometry projections SHALL be rejected atomically before replacing valid live state.

#### Scenario: Received movement
- **WHEN** a peer restores a moving three-member squad
- **THEN** it reconstructs one source and one coherent committed transition, not three independent movers

#### Scenario: Invalid received shape
- **WHEN** a member references a missing squad or two living squads claim one source
- **THEN** admission fails before current live state or credentials change

#### Scenario: Rejected movement leaves the standing action intact
- **WHEN** a standing unit requests a move whose footprint or transit token conflicts with an existing reservation
- **THEN** its current action identity, position and timing remain unchanged
- **AND** no source, destination or transit reservation is partially acquired

#### Scenario: Impact resolves once into recovery
- **WHEN** a unit's locked impact deadline is reached with an invalid primary
- **THEN** one missed impact is reported and the unit retains its original recovery deadline
- **AND** later ticks and repeated observations cannot apply the same impact again

#### Scenario: Moving casualty reconstructs from current state
- **WHEN** a complete snapshot contains a squad whose last member died during movement
- **THEN** its current frozen cosmetic progress restores without live source/transit claims
- **AND** it cannot arrive or impact during retained death presentation

#### Scenario: Transfer preserves recovery without preserving the old attack
- **WHEN** an enemy squad redirects during recovery after its preferred Keep falls
- **THEN** the committed cycle and remaining ready deadline remain unchanged without a fresh shot or teleport
- **AND** future selection re-evaluates current compatible targets at readiness

### Requirement: Current movement objective ranking at eligible boundaries

Ready squads without in-range targets SHALL evaluate currently reachable groups and permitted physical objectives. Shorter legal approaches SHALL precede retained farther routes; ties SHALL follow frozen objective preferences and seeded ordering. Each step SHALL be adjacent, bounded and legal. Committed moves/windup/recovery SHALL finish unchanged before re-evaluation. Unchanged blocking SHALL preserve decision identity and cause bounded waiting, not locomotion or rerolls. Relevant shared-arena changes SHALL be considered even when they occur in another sector. Ordinary reachable encounters SHALL engage without depending on BattleStalled.

#### Scenario: Cross-sector opportunity
- **WHEN** a ready allied squad can reach a foreign-origin threat through the connected arena
- **THEN** its owner/destination does not exclude that threat from legal approach selection

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
- **WHEN** a distant owner changes state that does not change a blocked squad's eligible targets/approaches
- **THEN** the blocked decision identity and seeded choice remain stable
- **AND** a relevant reachable shared-arena change is considered regardless of owner
