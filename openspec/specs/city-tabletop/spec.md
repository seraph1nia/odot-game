# city-tabletop Specification

## Purpose

Present authoritative cities and battles as a readable miniature 3D tabletop using the requested free KayKit assets while keeping interaction simple.

## Requirements

### Requirement: City interaction and match feedback
Clients SHALL expose building selection for empty owned slots, building upgrades, explicit building-specific archetype recruitment and Blacksmith research, ready/unready, and pause/resume through a compact control panel anchored to the bottom of the game window. The interface SHALL show gold/wood construction costs, recruitment/research costs, gold, food, wood, class research ranks, city health, soldier count, building level, phase including final preparation, turn within the three-turn cycle, wave out of three, readiness, connection status, and outcome. City inspection controls SHALL identify the player's own city and the observed city. Construction, upgrade, research and recruitment actions SHALL be contextual to the directly selected world plot or building; without a selection, the panel SHALL display a prompt to select in the world and SHALL NOT offer an actionable default plot. The interface SHALL NOT include a duplicate numbered grid for selecting building slots. A cooperative lobby SHALL expose the current roster, identify the host where applicable, and expose the start action only to eligible players. Hosted lobbies SHALL offer Invite friends; solo play SHALL enter the first building turn directly. Lobby, match, outcome, and disconnected views SHALL offer Return to menu. Connection and host-loss feedback SHALL reflect the selected session mode. Unavailable actions SHALL be visibly disabled or explained, and server rejection SHALL be surfaced. A disconnected guest SHALL offer reconnect without discarding its private resume information while the original authority may still be available, and input SHALL remain disabled until resynchronization completes. An ended hosted session SHALL offer return to the start screen without suggesting authority migration.

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

### Requirement: Requested free asset palette
The game SHALL use the free KayKit Medieval Hexagon Pack as the primary building/environment palette, KayKit Prototype Bits for missing non-character objects and markers, and KayKit Resource Bits for resource visuals such as gold. Grass, river terrain, hills, trees, rocks, and medieval village props SHALL use matching ready-made assets from the free medieval pack. Farm visuals SHALL be represented by an appropriate free medieval building or prop, with a clear farm label. Food scenery SHALL use a medieval prop such as a sack with clear food labeling instead of the prototype can. Combat units SHALL use free rigged Adventurers and Skeletons characters with compatible locomotion, melee, ranged shooting, casting, hit and death clips and matching weapons. Berserker SHALL use the free Barbarian and Mage the free Mage; Skeleton Warrior, Minion, Rogue and Mage SHALL supply their enemy counterparts. Required character animations SHALL NOT be replaced by dummy models, scale pulses, or labeled markers. Paid tiers SHALL NOT be required. Gold, food and wood SHALL be the only gameplay currencies. Resource Bits SHALL supply free gold and wood props; free medieval sacks/grain SHALL supply food. Paid food, coin, character or source tiers SHALL NOT be required. The selected assets, required textures/buffers, included license texts, and recorded official source/version information SHALL be available from a clean checkout without runtime downloads.

#### Scenario: Free assets cover the match
- **WHEN** a contributor prepares and runs the graphical game from a clean checkout
- **THEN** buildings, grass plots, river, hills, and decorations use the medieval palette and combat units use rigged characters and their required animations without paid content
- **AND** food uses a labeled medieval prop and gold, food and wood are the only economy resources

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
Graphical clients SHALL display each city as a medieval countryside landscape with nine hexagonal building plots arranged in three staggered rows and surrounding terrain continuing beyond the visible world edges. The landscape SHALL include surrounding grass, a river along one side, wooded hills, rocks, and small village props, including when all building plots are empty. A raised rear terrace, lower riverbank and wooded slopes SHALL create visible elevation differences; peripheral mountains, a bridge and appropriate building props SHALL enrich the scenery without hiding plots or battles. Terrain elevation SHALL remain cosmetic and unit rendering SHALL meet its visible ground surface without changing authoritative contact or targeting. The battlefield SHALL be a readable grassy approach with no painted road markings or modern road slab. Decorations and terrain SHALL NOT occupy building slots or change movement, damage, economy, or city capacity. Players SHALL be able to inspect every player's city and battle, identify their own city, and distinguish soldiers from enemies. The visual representation SHALL follow authoritative state, including enemy transfers, casualties, and eliminated cities. The game SHALL NOT require manual unit control, movable terrain, or a board-folding effect.

#### Scenario: Attractive empty starting village
- **WHEN** a match starts with all nine plots empty
- **THEN** the player sees a coherent grass landscape with the home, defender, river, wooded hills, rocks, and village props continuing beyond the visible world edges
- **AND** all nine buildable plots and the battle approach remain distinguishable from scenery

#### Scenario: Watch a redistributed wave
- **WHEN** another city falls during a wave
- **THEN** clients show that city's elimination and the transferred enemies fighting at their authoritative destinations
- **AND** the player can inspect the surviving cities while combat continues

#### Scenario: Upgraded buildings are recognizable
- **WHEN** a building is upgraded to level two
- **THEN** its structure or surrounding props visibly distinguish the upgrade without relying only on uniform scaling
- **AND** clicking its visible roof or raised plot still selects the same stable slot

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
Graphical clients SHALL use a slightly lower angled orthographic view that reveals building faces and terrain depth. At the default or reset overview at the default 1100x820 window size and at 1280x720, the focused city's nine plots, home/defender, and full battle approach SHALL fit in the world area above the bottom panel and every plot SHALL be selectable. Deliberate zoom or pan SHALL be allowed to place some world objects outside the visible world area, with Reset view restoring the complete composition. Tall scenery SHALL NOT conceal buildable plots or active combat in the overview, and the panel SHALL keep essential actions and feedback accessible without horizontal clipping.

#### Scenario: Inspect a fully built city
- **WHEN** a city has buildings in all nine plots, including level-two buildings, at either supported verification size and the camera is at its default or reset overview
- **THEN** each building can be selected in the world and the battle approach remains visible above the bottom panel
- **AND** resources, contextual actions, and match controls remain readable and reachable

#### Scenario: Observe four-player combat
- **WHEN** a four-player match reaches combat and the player switches between cities
- **THEN** each focused city starts at a consistent overview with its soldiers, attackers, and defender feedback visible
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

### Requirement: Cursor-anchored tabletop zoom
Graphical clients SHALL support scroll-wheel zoom in and out over the visible world area. Wheel up SHALL zoom in and wheel down SHALL zoom out while retaining the angled orthographic projection. The ground-plane point beneath the cursor SHALL remain at the same screen position during zoom unless maintaining that anchor would exceed the camera's travel bounds. Zoom SHALL have finite close and overview limits; input beyond a limit SHALL leave the view unchanged. Scrolling over the HUD or an open dialog SHALL NOT zoom the world.

#### Scenario: Zoom toward an off-center point
- **WHEN** the player scrolls up over an off-center world point with room inside the travel bounds
- **THEN** the world appears larger and the ground-plane point under the cursor stays at the same screen position
- **AND** scrolling down reverses the scale change without changing camera orientation

#### Scenario: Reach a zoom limit
- **WHEN** the player continues scrolling in a direction after reaching that zoom limit
- **THEN** neither camera scale nor position changes

#### Scenario: Scroll over controls
- **WHEN** the player scrolls over the bottom panel or while a dialog is open
- **THEN** the world camera remains unchanged and the UI retains its normal scrolling behavior

### Requirement: Keyboard tabletop panning
Graphical clients SHALL support continuous camera panning while WASD or arrow keys are held during world interaction. W/up and S/down SHALL move the view toward the top and bottom of the screen across the tabletop ground plane; A/left and D/right SHALL move left and right. Equivalent bindings SHALL have equivalent effects, diagonals SHALL NOT move faster than cardinal directions, and movement SHALL be independent of frame rate. Camera travel SHALL be bounded around the observed city while preserving camera height and orientation.

#### Scenario: Hold and release a direction
- **WHEN** the player holds D or right arrow during world interaction and then releases it
- **THEN** the view pans right while held and stops on release
- **AND** the camera does not rotate or change height

#### Scenario: Move diagonally and reach the edge
- **WHEN** the player holds two perpendicular pan directions and continues toward the travel boundary
- **THEN** diagonal speed does not exceed cardinal speed and the view stops at the boundary

### Requirement: Local camera input and lifecycle
Camera navigation SHALL be local presentation state and SHALL NOT submit gameplay commands or alter authoritative state. Modal dialogs, focused UI controls consuming navigation keys, text entry and an unfocused game window SHALL take priority over camera navigation. Losing focus or opening a modal SHALL stop held camera movement without stale movement resuming when input returns. Camera navigation SHALL remain usable over an available city view during shared pause, transport loss and outcome screens. Projected unit health bars SHALL follow camera changes even when combat playback is frozen.

#### Scenario: UI receives navigation keys
- **WHEN** a text field, dropdown or other focused UI control consumes a camera-bound key
- **THEN** that control responds normally and the camera does not move from that key

#### Scenario: Interrupt a held direction
- **WHEN** the window loses focus or a modal opens while a pan key is held
- **THEN** camera movement stops and does not resume until a new eligible press after release

#### Scenario: Inspect a paused or disconnected battle
- **WHEN** the player navigates an available paused or disconnected city view
- **THEN** camera navigation and health-bar projection respond while combat poses, health and authoritative gameplay remain frozen

### Requirement: Recoverable camera overview
Graphical clients SHALL expose a visible Reset view control that restores the fitted overview of the observed city. Entering a tabletop, switching to a different city or receiving a fresh match SHALL initialize the overview and clear held navigation. Resizing the window or changing HUD height SHALL recompute the fitted base view and preserve the player's relative zoom and city-relative pan within current limits. Reconnecting to the same match and observed city in the same tabletop SHALL retain camera adjustments; camera state SHALL NOT persist across application launches.

#### Scenario: Reset a close view
- **WHEN** the player activates Reset view after zooming and panning
- **THEN** the observed city's fitted overview is restored with all plots and the battle approach above the HUD
- **AND** world selection and resources are unchanged

#### Scenario: Switch city or match
- **WHEN** the observed city changes or a fresh match replaces the current match
- **THEN** the new city's overview is shown without inheriting the previous city's pan or zoom

#### Scenario: Resize an adjusted view
- **WHEN** the window or HUD size changes while the player has adjusted the view
- **THEN** the relative zoom and city-relative pan are preserved within limits against the recalculated base framing
- **AND** Reset view still restores a fully fitted overview

#### Scenario: Reconnect within the same view
- **WHEN** the client reconnects to the same match and observed city without replacing its tabletop
- **THEN** its camera adjustments are retained and rebuilt world targets remain selectable

### Requirement: World interaction after camera navigation
Hover and selection SHALL resolve the currently visible plot or building to its existing slot after camera navigation. Navigation SHALL NOT spend resources, change world selection or enable editing a foreign city. Screen-space overlays SHALL remain aligned with their world anchors and SHALL NOT intercept world input. Source and exported graphical clients SHALL provide the same camera controls; headless roles SHALL NOT instantiate camera presentation.

#### Scenario: Select a building after moving
- **WHEN** the player zooms and pans and then clicks a visible building roof
- **THEN** the same authoritative slot is selected and its contextual controls are displayed
- **AND** resources are spent only after an explicit eligible gameplay action

#### Scenario: Inspect a foreign city closely
- **WHEN** the player navigates a foreign city's view and selects one of its visible plots
- **THEN** the observed plot is identified while spending controls remain unavailable

### Requirement: Coherent hex and surface placement
Structures, including the home and defender, SHALL be centered by their ground footprints on identified supporting hexes. Decorative groups and resource stockpiles SHALL be anchored to supporting hexes with deliberate local offsets that keep them clear of buildable plots and the battle approach. Assets SHALL meet their supporting terrain surfaces, including raised terrain; stacked structures SHALL meet their supporting asset surfaces without visible floating or unintended interpenetration. Placement SHALL preserve stable slot identities, plot/building selection and authoritative unit positions.

#### Scenario: Place structures on raised hexes
- **WHEN** the client displays starting structures or constructs a building on a raised plot
- **THEN** each structure's footprint is centered on its supporting hex and its base meets the visible supporting surface
- **AND** clicking a constructed building or its plot selects the same authoritative slot

#### Scenario: Stack an upgraded tower
- **WHEN** a tower receives a supporting base during an upgrade
- **THEN** the tower is centered over and seated on that base without a visible gap or unintended overlap
- **AND** the base and tower remain selectable as the same slot

#### Scenario: Display stockpiles and decorations
- **WHEN** resources change or the countryside is rendered
- **THEN** stockpiles and decorative groups remain seated on their local supporting surfaces
- **AND** they do not cover buildable plots or obstruct the battle approach

### Requirement: Continuous countryside view coverage
At 1100x820 and 1280x720 and after supported viewport or HUD changes, all visible world area SHALL be covered by the medieval landscape without exposing the terrain patch perimeter, its exterior slab walls or the flat environment background beyond it. Coverage SHALL include every permitted camera zoom and pan position when navigation is available. Extending scenery SHALL NOT enlarge the fitted overview bounds so as to shrink the playable village; at the default/reset overview all nine plots, home, defender and the battle approach SHALL remain visible above the HUD. River tiles and terrain transitions SHALL join coherently throughout visible coverage, and tall scenery SHALL remain clear of plot and battle sightlines.

#### Scenario: Fill the overview after resizing
- **WHEN** the client displays or resizes the default city overview at either supported verification size
- **THEN** countryside covers the entire visible world area with its outer perimeter offscreen
- **AND** the plots and battle approach remain readable and selectable above the HUD

#### Scenario: Cover camera travel limits
- **WHEN** the player navigates to a permitted zoom or pan limit
- **THEN** the visible world area remains filled with coherently joined countryside
- **AND** Reset view restores the complete playable composition

### Requirement: Menu shares the starting countryside
The start screen and multiplayer entry backdrop SHALL render the same terrain arrangement, elevations, home, defender, empty nine plots, river, bridge and static decorations as a fresh starting village, with consistent asset scale, materials and lighting. Menu framing SHALL fill the background around its readable controls and use the same landscape coverage rules for its viewport. Menu scenery SHALL remain non-interactive and SHALL NOT create a gameplay session, connect to an authority or expose match actions. Returning to the menu SHALL restore the empty starting countryside without retaining a previous match's constructed buildings or units. Source and exported graphical clients SHALL provide this same presentation; headless roles SHALL remain free of visual instantiation.

#### Scenario: Enter a solo match from the menu
- **WHEN** the player views the start screen and then starts a fresh solo match
- **THEN** the same starting countryside arrangement, assets and scales are recognizable in both views
- **AND** the menu has no match state while the match exposes its nine empty selectable plots

#### Scenario: Return after construction
- **WHEN** the player returns to the menu after constructing buildings in a match
- **THEN** the filled backdrop shows the empty starting village without the match's buildings or combat units
- **AND** menu controls remain usable without world selection or gameplay input

#### Scenario: Run the packed menu
- **WHEN** an exported graphical client displays its start screen and enters solo play without access to the source project
- **THEN** both views load the shared countryside and fill their visible world areas from bundled assets
