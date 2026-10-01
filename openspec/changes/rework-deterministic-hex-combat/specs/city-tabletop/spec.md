# Spec Delta

## MODIFIED Requirements

### Requirement: City interaction and match feedback
Clients SHALL expose building selection for empty owned slots, building upgrades, explicit building-specific archetype recruitment and Blacksmith research, ready/unready, and pause/resume through a compact control panel anchored to the bottom of the game window. The interface SHALL show gold/wood construction costs, recruitment/research costs, gold, food, wood, class research ranks, city health, soldier count, building level, phase including final preparation, turn within the three-turn cycle, wave out of three, readiness, connection status, and outcome, distinguishing all-cities-fallen defeat from battle-stalled defeat. A stalled result SHALL explain that the battle could not finish, rather than claim every city fell. City inspection controls SHALL identify the player's own city and the observed city. Construction, upgrade, research and recruitment actions SHALL be contextual to the directly selected world plot or building; without a selection, the panel SHALL display a prompt to select in the world and SHALL NOT offer an actionable default plot. The interface SHALL NOT include a duplicate numbered grid for selecting building slots. A cooperative lobby SHALL expose the current roster, identify the host where applicable, and expose the start action only to eligible players. Hosted lobbies SHALL offer Invite friends; solo play SHALL enter the first building turn directly. Lobby, match, outcome, and disconnected views SHALL offer Return to menu. Connection and host-loss feedback SHALL reflect the selected session mode. Unavailable actions SHALL be visibly disabled or explained, and server rejection SHALL be surfaced. A disconnected guest SHALL offer reconnect without discarding its private resume information while the original authority may still be available, and input SHALL remain disabled until resynchronization completes. An ended hosted session SHALL offer return to the start screen without suggesting authority migration.

#### Scenario: Recruit through the graphical interface
- **WHEN** a player clicks their recruitment building during an eligible building or preparation phase
- **THEN** the bottom panel displays its level and explicit recruitment actions for each archetype that building unlocks and its displayed resource costs
- **AND** activating recruitment shows the authoritative resource and army changes after acceptance

#### Scenario: Pause and reconnect feedback
- **WHEN** the match is paused and a client reconnects
- **THEN** the bottom panel displays the paused match, retained city, roster connectivity, and a resume control
- **AND** it does not display frozen gameplay as active progression

#### Scenario: No plot has been selected
- **WHEN** a synchronized player first views a city or switches to another city
- **THEN** the panel asks them to click a plot or building and no build, upgrade, or recruitment can target a previously selected plot
- **AND** resources, city inspection, and available match controls remain accessible

#### Scenario: Inspect another player's city
- **WHEN** a player switches to another city's view and selects a building there
- **THEN** the panel shows the observed city's building and resource information and visibly prevents spending or recruitment on that city

#### Scenario: Recruit a ranged soldier through controls
- **WHEN** the player selects their Archery Range and activates its enabled Crossbowman control
- **THEN** the accepted request adds a Crossbowman and displays the authoritative food deduction and army count
- **AND** observing a foreign recruitment building, becoming ready, pausing or entering combat disables its recruitment controls

#### Scenario: Inspect research and preparation
- **WHEN** a player selects their Blacksmith in final preparation
- **THEN** the panel shows melee, ranged and magic ranks, next-rank effects and authoritative costs
- **AND** Ready is labeled to start battle rather than produce resources

#### Scenario: Stalled defeat is truthful
- **WHEN** the authority reports defeat due to a combat limit while city health remains positive
- **THEN** the panel displays the battle-stalled reason and ordinary return-to-menu controls
- **AND** it does not describe surviving cities as fallen

### Requirement: Authoritative animated combat presentation
Graphical clients SHALL show idle, locomotion, sword attack, ranged shooting, hit and death states appropriate to the unit type and authoritative action. Displayed positions SHALL map authoritative hex identities and fixed positions/footprints to declared anchors, and facing SHALL derive from authoritative targets and moves. Accepted movement SHALL sample its declared route and start/end ticks using the shared snapshot clock, including several allied units in a hex; queued or blocked units SHALL NOT walk. Presentation SHALL NOT extrapolate beyond authoritative time, move bodies through conflicting reserved routes, merge fixed positions or manufacture combat offsets. Adjacent-hex melee SHALL deliberately depict abstract tabletop attacks: the actual rigged swing and facing at its fixed anchor SHALL be accompanied by a short directional strike connection to the locked target and target-side impact feedback on a landed authoritative hit. This SHALL remain readable for near-side and far-side occupants of a shared adjacent hex without claiming physical blade contact. Misses SHALL NOT show landed-target feedback. Unlinked generic sparks or a swing with no readable attacker/target relationship SHALL NOT satisfy the melee contract. Attack presentation SHALL NOT stretch weapons or slide a unit away from its footprint anchor to imply contact. Root motion and animation callbacks SHALL NOT move authoritative units, apply damage or release capacity. Effects SHALL be keyed to match and action identity so repeated or stale snapshots do not replay a strike, shot or hit. A casualty SHALL stop all living presentation immediately and play its death pose from authoritative death start/end ticks and recorded final movement progress. Its non-interactive visual SHALL remain coherent with the retained authoritative death reservations and SHALL be removed at the sampled death-end deadline or immediately on session replacement. Current dying bodies SHALL be reconstructed from complete snapshots without requiring historical death events. Missing required assets or clips SHALL be reported as a presentation failure. Attack animations SHALL be aligned to authoritative impact timing, and any short visual projectile SHALL be cosmetic. Death SHALL take visual precedence over hit, hit over an idle pose, and hit playback SHALL NOT cancel a valid authoritative attack.

#### Scenario: Stop and swing at contact
- **WHEN** moving Swordsmen reach their authoritative positions in adjacent combat hexes
- **THEN** they face their targets, stop locomotion and play a rigged sword attack with an explicit directional strike cue and target impact aligned with the authoritative hit
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
- **THEN** the combat poses and effects freeze while connection and pause/resume controls remain responsive
- **AND** resume continues their remaining progress without wall-clock catch-up

#### Scenario: Reconnect to a current battle
- **WHEN** a client receives its complete current state after reconnecting
- **THEN** it reconstructs living units and unexpired dying bodies at their current hex positions and action/death progress without replaying historical hits, shots or death sounds, restarting deaths or reviving dead units
- **AND** events from the former connection or another match cannot create duplicate visuals

#### Scenario: Shared hex has separate visible positions
- **WHEN** several allied units occupy one combat hex
- **THEN** their models stand at distinct declared anchors matching their legal footprints
- **AND** moving models sample reserved routes without displacing stationary neighbors

#### Scenario: Frame rate does not order combat
- **WHEN** compatible attacks and moves begin on the same authority tick
- **THEN** clients sample each action from that tick irrespective of local frame rate
- **AND** initiative does not add presentation-only delays that change the declared action timing

#### Scenario: Far-side melee target is readable
- **WHEN** an anchored melee occupant attacks an eligible far-side occupant in an adjacent shared hex
- **THEN** the directional strike cue identifies the actual locked target and its landed impact at the declared tick
- **AND** the miniature remains at its footprint anchor and the display reads as an abstract tabletop attack

#### Scenario: Verify neighboring occupied cells before fixing scale
- **WHEN** the initial board/action model reaches its early gameplay gate
- **THEN** a checked-in short source combat check captures actual bundled rigs in two occupied adjacent cells with near/far targets and a simultaneous exchange
- **AND** readable linked attacks, health/role identification and shared positions are inspected before board spacing/footprint layout and broad presentation integration are accepted
