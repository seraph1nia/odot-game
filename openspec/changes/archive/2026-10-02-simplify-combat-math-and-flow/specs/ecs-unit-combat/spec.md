# Spec Delta

## ADDED Requirements

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

