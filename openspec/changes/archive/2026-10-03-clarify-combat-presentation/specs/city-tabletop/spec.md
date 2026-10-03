# Spec Delta

## MODIFIED Requirements

### Requirement: Authoritative animated combat presentation
Graphical clients SHALL show idle, walking, sword attack, ranged shooting, hit and death states appropriate to the unit type and authoritative action. Displayed positions SHALL map authoritative hex identities and fixed positions/footprints to declared anchors, and facing SHALL derive from authoritative targets and moves with restrained presentation-clock-driven transitions. Each accepted adjacent move SHALL travel in a straight line between its declared source and destination anchors using its authoritative start/end ticks and the shared snapshot clock. Transient visual overlap with moving, standing or retained dying models SHALL be permitted during a committed move; it SHALL NOT change numerical capacity, reservations, route decisions, attackable location, arrival deadlines, targets or outcomes. Settled models SHALL remain at distinct declared anchors; presentation SHALL NOT displace stationary neighbors, manufacture combat offsets, extrapolate beyond authoritative time or make queued/blocked units walk. Locomotion and living action transitions SHALL use bounded blends that freeze with battle presentation and preserve the authored impact marker. Adjacent-hex melee SHALL deliberately depict abstract tabletop attacks: the actual rigged sword slice or axe chop and facing at its fixed anchor SHALL be accompanied by a restrained directional windup intent cue, a short local strike accent and target-side impact feedback only on a landed authoritative hit. Intent SHALL identify the locked target without an opaque continuous hand-to-target beam. This SHALL remain readable for near-side and far-side occupants of a shared adjacent hex without claiming physical blade contact. Misses SHALL NOT show landed-target feedback. Unlinked generic sparks or a swing with no readable attacker/target relationship SHALL NOT satisfy the melee contract. Attack presentation SHALL NOT stretch weapons or slide a unit away from its footprint anchor to imply contact. Root motion and animation callbacks SHALL NOT move authoritative units, apply damage or release capacity. Effects SHALL be keyed to match and action identity so repeated or stale snapshots do not replay a strike, shot or hit. A casualty SHALL stop all living presentation immediately and play its death pose from authoritative death start/end ticks and recorded final movement progress. Its non-interactive visual SHALL remain coherent with retained authoritative death reservations and SHALL be removed at the sampled death-end deadline or immediately on session replacement. Current dying bodies SHALL be reconstructed from complete snapshots without requiring historical death events. Missing required assets or clips SHALL be reported as a presentation failure. Attack animations SHALL be aligned to authoritative impact timing, and any short visual projectile SHALL be cosmetic. Death SHALL take visual precedence over hit, hit over an idle pose, and hit playback SHALL NOT cancel a valid authoritative attack. Rendering cadence SHALL remain independent of the unchanged numerical tick rate; this presentation SHALL NOT introduce a slower authority clock, delayed replay or serialized combat turns.

#### Scenario: Stop and swing at contact
- **WHEN** moving Swordsmen reach their authoritative positions in adjacent combat hexes
- **THEN** they face their targets, stop locomotion and play a rigged sword attack with explicit directional intent, a local strike accent and target impact aligned with the authoritative hit
- **AND** health changes follow the authority rather than an animation callback

#### Scenario: One shot across repeated snapshots
- **WHEN** a Crossbowman attack appears in multiple snapshots
- **THEN** its shooting animation and cosmetic shot are triggered once for that action identity and later snapshots update their progress

#### Scenario: Casualty finishes dying
- **WHEN** a visible soldier or enemy is killed and disappears from the living army snapshot
- **THEN** its death sequence plays from the recorded final position/progress without participating in targeting or living army counts, while its positions remain reserved until death end
- **AND** its visual is freed at the authoritative death-end pose or immediately on session replacement

#### Scenario: Pause freezes a posed battle
- **WHEN** combat is paused during movement, an attack, a hit, a projectile or a death sequence
- **THEN** positions, facing, rig blends and effects freeze while connection and pause/resume controls remain responsive
- **AND** resume continues their remaining progress without wall-clock catch-up

#### Scenario: Reconnect to a current battle
- **WHEN** a client receives its complete current state after reconnecting
- **THEN** it reconstructs living units and unexpired dying bodies at their current hex positions and action/death progress without replaying historical hits, shots or death sounds, restarting deaths or reviving dead units
- **AND** events from the former connection or another match cannot create duplicate visuals

#### Scenario: Shared hex has separate visible positions
- **WHEN** several allied units occupy one combat hex
- **THEN** settled models stand at distinct declared anchors matching their legal footprints
- **AND** a committed mover travels directly to its reserved destination without displacing stationary neighbors, even if its visual crosses another model

#### Scenario: Retained death still needs visual clearance
- **WHEN** a visible casualty retains its authoritative positions or transit reservations until death end
- **THEN** settled living models and settled casualties keep distinct declared anchors while a committed mover may cross the frozen death pose visually
- **AND** graphical settled-clearance assertions include visible settled dying bodies, and neither visual overlap nor animation releases the casualty's complete numerical positions/transit claims before death end

#### Scenario: Frame rate does not order combat
- **WHEN** compatible attacks and moves begin on the same authority tick
- **THEN** clients sample each action from that tick irrespective of local frame rate
- **AND** initiative does not add presentation-only delays that change the declared action timing

#### Scenario: Overview retains readable unit roles
- **WHEN** a player observes shared combat hexes at the supported overview scale
- **THEN** input-transparent health bars and Roman level numerals remain readable above models without overhead names or role codes, and clicking a unit exposes its role/name in inspection
- **AND** restrained directional melee intent identifies its locked target during windup without opaque crossing beams

#### Scenario: Far-side melee target is readable
- **WHEN** an anchored melee occupant attacks an eligible far-side occupant in an adjacent shared hex
- **THEN** directional intent identifies the actual locked target and its landed impact at the declared tick
- **AND** the miniature remains at its footprint anchor and the display reads as an abstract tabletop attack

#### Scenario: Verify neighboring occupied cells before fixing scale
- **WHEN** the board/action presentation is verified at the owned rendered gate
- **THEN** a checked-in short source combat check captures actual bundled rigs in occupied adjacent cells with near/far targets and a simultaneous exchange
- **AND** linked intent/strike/impact cues, health identification and shared anchors are inspected alongside a bounded frame sequence with combat ticks and action identities

#### Scenario: Direct committed step has no visual detour
- **WHEN** a living unit completes an accepted adjacent move
- **THEN** each sampled position lies on the source-to-destination segment at the declared elapsed fraction with no backward progress or ring detour
- **AND** it uses walking locomotion while numerical source attackability, reservations and arrival timing are unchanged
