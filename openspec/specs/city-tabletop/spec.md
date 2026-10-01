# city-tabletop Specification

## Purpose

Present authoritative cities and battles as a readable miniature 3D tabletop using the requested free KayKit assets while keeping interaction simple.

## Requirements

### Requirement: City interaction and match feedback
Clients SHALL expose building selection for empty owned slots, building upgrades, explicit Swordsman and Crossbowman barracks recruitment, ready/unready, and pause/resume through a compact control panel anchored to the bottom of the game window. The interface SHALL show costs, gold, food, city health, soldier count, building level, phase, turn within the three-turn cycle, wave out of three, readiness, connection status, and outcome. City inspection controls SHALL identify the player's own city and the observed city. Construction, upgrade, and recruitment actions SHALL be contextual to the directly selected world plot or building; without a selection, the panel SHALL display a prompt to select in the world and SHALL NOT offer an actionable default plot. The interface SHALL NOT include a duplicate numbered grid for selecting building slots. A cooperative lobby SHALL expose the current roster, identify the host where applicable, and expose the start action only to eligible players. Hosted lobbies SHALL offer Invite friends; solo play SHALL enter the first building turn directly. Lobby, match, outcome, and disconnected views SHALL offer Return to menu. Connection and host-loss feedback SHALL reflect the selected session mode. Unavailable actions SHALL be visibly disabled or explained, and server rejection SHALL be surfaced. A disconnected guest SHALL offer reconnect without discarding its private resume information while the original authority may still be available, and input SHALL remain disabled until resynchronization completes. An ended hosted session SHALL offer return to the start screen without suggesting authority migration.

#### Scenario: Recruit through the graphical interface
- **WHEN** a player clicks their barracks during an eligible building phase
- **THEN** the bottom panel displays its level and explicit recruitment actions for each soldier type and its displayed food cost
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
- **WHEN** the player selects their barracks and activates its enabled Crossbowman control
- **THEN** the accepted request adds a Crossbowman and displays the authoritative food deduction and army count
- **AND** observing a foreign barracks, becoming ready, pausing or entering combat disables both recruitment controls

### Requirement: Requested free asset palette
The game SHALL use the free KayKit Medieval Hexagon Pack as the primary building/environment palette, KayKit Prototype Bits for missing non-character objects and markers, and KayKit Resource Bits for resource visuals such as gold. Grass, river terrain, hills, trees, rocks, and medieval village props SHALL use matching ready-made assets from the free medieval pack. Farm visuals SHALL be represented by an appropriate free medieval building or prop, with a clear farm label. Food scenery SHALL use a medieval prop such as a sack with clear food labeling instead of the prototype can. Combat units SHALL use free rigged KayKit characters with compatible locomotion, melee, ranged shooting, hit and death clips and matching sword/crossbow accessories. Required character animations SHALL NOT be replaced by dummy models, scale pulses, or labeled markers. Paid tiers SHALL NOT be required. Asset choices SHALL NOT introduce additional gameplay currencies. The selected assets, required textures/buffers, included license texts, and recorded official source/version information SHALL be available from a clean checkout without runtime downloads.

#### Scenario: Free assets cover the match
- **WHEN** a contributor prepares and runs the graphical game from a clean checkout
- **THEN** buildings, grass plots, river, hills, and decorations use the medieval palette and combat units use rigged characters and their required animations without paid content
- **AND** food uses a labeled medieval prop and gold and food remain the only economy resources

#### Scenario: Clean checkout contains animated characters
- **WHEN** a contributor imports source or launches a packed graphical export without downloading assets at runtime
- **THEN** soldiers and enemies load their rigged models, matching weapons, textures and required animation clips
- **AND** the included provenance identifies immutable sources, file hashes and licenses for the selected character subset

### Requirement: Headless and exported operation
The authoritative dedicated server and automated headless roles, including a playing-host role without presentation, SHALL operate without graphics, audio, or visual asset instantiation. Ordinary exported graphical launches SHALL display the start screen and display the tabletop when a session is entered; explicit client launches SHALL retain direct connection behavior. Exported graphical clients SHALL include the selected assets, usable local/hosted controls, and the chosen native Steam dependencies. Dedicated-server exports SHALL continue to run and communicate correctly when visual resources are stripped. Local/ENet headless roles SHALL NOT depend on Steam initialization, account login, graphical preferences, or audio.

#### Scenario: Headless network match
- **WHEN** separate automated headless clients connect to a dedicated server
- **THEN** they can build, recruit, advance turns, and complete battles through the same game protocol without creating the tabletop view

#### Scenario: Exported graphical client
- **WHEN** the exported desktop client enters a session or uses explicit client arguments to connect to a server
- **THEN** it displays the board, selected KayKit models, and usable match controls without access to the source project

#### Scenario: Exported solo play without Steam
- **WHEN** the exported desktop client starts without a running Steam client and the player selects Single player
- **THEN** the tabletop and normal gameplay controls work without source-project access, a network socket, or an external server

### Requirement: Medieval landscape and visible battles
Graphical clients SHALL display each city as a bounded medieval countryside landscape with nine hexagonal building plots arranged in three staggered rows. The landscape SHALL include surrounding grass, a river along one side, wooded hills, rocks, and small village props, including when all building plots are empty. The battlefield SHALL be a readable grassy approach with no painted road markings or modern road slab. Decorations and terrain SHALL NOT occupy building slots or change movement, damage, economy, or city capacity. Players SHALL be able to inspect every player's city and battle, identify their own city, and distinguish soldiers from enemies. The visual representation SHALL follow authoritative state, including enemy transfers, casualties, and eliminated cities. The game SHALL NOT require manual unit control, movable terrain, or a board-folding effect.

#### Scenario: Attractive empty starting village
- **WHEN** a match starts with all nine plots empty
- **THEN** the player sees a coherent grass landscape with the home, defender, river, wooded hills, rocks, and village props
- **AND** all nine buildable plots and the battle approach remain distinguishable from scenery

#### Scenario: Watch a redistributed wave
- **WHEN** another city falls during a wave
- **THEN** clients show that city's elimination and the transferred enemies fighting at their authoritative destinations
- **AND** the player can inspect the surviving cities while combat continues

### Requirement: Direct world selection
A player SHALL select a building slot by clicking its visible hexagonal plot or the visible building occupying it. Both targets SHALL resolve to the same authoritative slot identity, including after construction, upgrading, and reconnecting. Hover and selection feedback SHALL be restrained and SHALL NOT obscure the terrain or imported building materials. Clicking a plot or building SHALL select it without automatically spending resources. Clicking scenery, gaps outside buildable plots, or the bottom panel SHALL NOT select a different world slot or cause an unintended gameplay action.

#### Scenario: Select each empty hex plot
- **WHEN** the player clicks each of the nine empty plot centers in turn
- **THEN** each click selects exactly the corresponding slot and shows its available construction actions
- **AND** no resources are spent until a construction action is activated

#### Scenario: Click a raised building
- **WHEN** the player clicks the visible roof or body of a building, including a level-two building
- **THEN** its own slot is selected instead of a neighboring plot behind the building
- **AND** the panel shows the correct building type, level, and contextual actions

#### Scenario: Click controls over the world
- **WHEN** the player clicks a bottom-panel control with terrain beneath it on screen
- **THEN** only that control receives the click and the world selection is unchanged

### Requirement: Lower camera and readable composition
Graphical clients SHALL use a slightly lower angled orthographic view that reveals building faces and terrain depth while keeping every building plot selectable. At the default 1100x820 window size and at 1280x720, the focused city's nine plots, home/defender, and full battle approach SHALL fit in the world area above the bottom panel. Tall scenery SHALL NOT conceal buildable plots or active combat, and the panel SHALL keep essential actions and feedback accessible without horizontal clipping.

#### Scenario: Inspect a fully built city
- **WHEN** a city has buildings in all nine plots, including level-two buildings, at either supported verification size
- **THEN** each building can be selected in the world and the battle approach remains visible above the bottom panel
- **AND** resources, contextual actions, and match controls remain readable and reachable

#### Scenario: Observe four-player combat
- **WHEN** a four-player match reaches combat and the player switches between cities
- **THEN** each focused city is framed consistently with its soldiers, attackers, and defender feedback visible
- **AND** scenery does not conceal the combat or change the displayed authoritative destinations

### Requirement: Authoritative animated combat presentation
Graphical clients SHALL show idle, locomotion, sword attack, ranged shooting, hit and death states appropriate to the unit type and authoritative action. Displayed positions and facing SHALL derive from the battle's authoritative positions, targets and movement; presentation interpolation SHALL NOT visibly carry bodies through contact or manufacture lateral offsets unrelated to combat. Root motion and animation callbacks SHALL NOT move authoritative units or apply damage. Effects SHALL be keyed to match and action identity so repeated or stale snapshots do not replay a strike, shot or hit. A casualty observed by a running client SHALL leave combat immediately while its non-interactive visual completes a bounded death sequence and is cleaned up. Missing required assets or clips SHALL be reported as a presentation failure. Attack animations SHALL be aligned to authoritative impact timing, and any short visual projectile SHALL be cosmetic. Death SHALL take visual precedence over hit, hit over an idle pose, and hit playback SHALL NOT cancel a valid authoritative attack.

#### Scenario: Stop and swing at contact
- **WHEN** moving Swordsmen reach their authoritative melee contact positions
- **THEN** they face their targets, stop locomotion and play a sword attack whose strike aligns with the authoritative impact
- **AND** health changes follow the authority rather than an animation callback

#### Scenario: One shot across repeated snapshots
- **WHEN** a Crossbowman attack appears in multiple snapshots
- **THEN** its shooting animation and cosmetic shot are triggered once for that action identity and later snapshots update their progress

#### Scenario: Casualty finishes dying
- **WHEN** a visible soldier or enemy is killed and disappears from the living army snapshot
- **THEN** its death sequence plays from the retained final position without participating in targeting, contact or army counts
- **AND** its visual is freed after the sequence or immediately on session replacement

#### Scenario: Pause freezes a posed battle
- **WHEN** combat is paused during movement, an attack, a hit, a projectile or a death sequence
- **THEN** the combat poses and effects freeze while connection and pause/resume controls remain responsive
- **AND** resume continues their remaining progress without wall-clock catch-up

#### Scenario: Reconnect to a current battle
- **WHEN** a client receives its complete current state after reconnecting
- **THEN** it reconstructs living units at the current positions and action progress without replaying historical hits or shots or reviving dead units
- **AND** events from the former connection or another match cannot create duplicate visuals
