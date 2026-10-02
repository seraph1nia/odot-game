# city-tabletop Specification

## Purpose

Present authoritative cities and battles as a readable miniature 3D tabletop using the requested free KayKit assets while keeping interaction simple.

## Requirements

### Requirement: City interaction and match feedback
Clients SHALL expose construction on empty purchased plots, locked-plot purchases, building upgrades and sales, explicit building-specific archetype recruitment, Blacksmith research, Market sales, ready/unready, and pause/resume through a compact control panel anchored to the bottom of the game window. The interface SHALL show complete construction/upgrade/recruitment/research costs, building refund and Market quotes, gold, food, wood, stone, metal, cloth, purchased plot count, next expansion cost, current army upkeep demand and food balance, class research ranks, city health, soldier count, building level, phase including final preparation, turn within the three-turn cycle, wave out of twenty and whether it is a boss wave, readiness, connection status, and outcome, distinguishing all-cities-fallen defeat from battle-stalled defeat. A stalled result SHALL explain that the battle could not finish, rather than claim every city fell. City inspection controls SHALL identify the player's own city and the observed city. Construction, plot purchase, building sale, upgrade, research, Market trade and recruitment actions SHALL be contextual to the directly selected world plot or building; without a selection, the panel SHALL display a prompt to select in the world and SHALL NOT offer an actionable default plot. The interface SHALL NOT include a duplicate numbered grid for selecting building slots. A cooperative lobby SHALL expose the current roster, identify the host where applicable, and expose the start action only to eligible players. Hosted lobbies SHALL offer Invite friends; solo play SHALL enter the first building turn directly. Lobby, match, outcome, and disconnected views SHALL offer Return to menu. Connection and host-loss feedback SHALL reflect the selected session mode. Unavailable actions SHALL be visibly disabled or explained, and server rejection SHALL be surfaced. A disconnected guest SHALL offer reconnect without discarding its private resume information while the original authority may still be available, and input SHALL remain disabled until resynchronization completes. An ended hosted session SHALL offer return to the start screen without suggesting authority migration.

#### Scenario: Recruit through the graphical interface
- **WHEN** a player clicks their recruitment building during an eligible building or preparation phase
- **THEN** the bottom panel displays its level and explicit recruitment actions for each archetype that building unlocks and its authoritative recruit level, resulting health/damage, rounded material/gold costs and separate per-battle food upkeep
- **AND** activating recruitment shows the authoritative resource and army changes after acceptance

#### Scenario: Pause and reconnect feedback
- **WHEN** the match is paused and a client reconnects
- **THEN** the bottom panel displays the paused match, retained city, roster connectivity, and a resume control
- **AND** it does not display frozen gameplay as active progression

#### Scenario: No plot has been selected
- **WHEN** a synchronized player first views a city or switches to another city
- **THEN** the panel asks them to click a plot or building and no economic action can target a previously selected plot
- **AND** resources, city inspection, and available match controls remain accessible

#### Scenario: Inspect another player's city
- **WHEN** a player switches to another city's view and selects a building there
- **THEN** the panel shows the observed city's building and resource information and visibly prevents spending or recruitment on that city

#### Scenario: Recruit a ranged soldier through controls
- **WHEN** the player selects their Archery Range and activates its enabled Crossbowman control
- **THEN** the accepted request adds a Crossbowman and displays the authoritative material deduction and army count without a food recruitment charge
- **AND** observing a foreign recruitment building, becoming ready, pausing or entering combat disables its recruitment controls

#### Scenario: Inspect research and preparation
- **WHEN** a player selects their Blacksmith in final preparation
- **THEN** the panel shows melee, ranged and magic ranks, next-rank effects and authoritative costs
- **AND** Ready is labeled to start battle rather than produce resources

#### Scenario: Preview a recruitment-building upgrade
- **WHEN** the player selects a recruitment building below level five
- **THEN** the panel shows its current recruit level, next level, upgrade cost and the next level's material recruitment price, health/damage and per-battle upkeep
- **AND** it explains that existing soldiers retain their level, shows no food-discount claim and disables an unavailable upgrade

#### Scenario: Inspect maximum recruitment level
- **WHEN** the player selects a level-five recruitment building
- **THEN** recruitment remains available under normal eligibility while upgrading is shown as complete
- **AND** a level-two production building or tower and a level-one Market remain at their own maxima


#### Scenario: Stalled defeat is truthful
- **WHEN** the authority reports defeat due to a combat limit while city health remains positive
- **THEN** the panel displays the battle-stalled reason and ordinary return-to-menu controls
- **AND** it does not describe surviving cities as fallen

### Requirement: Requested free asset palette
The game SHALL use the free KayKit Medieval Hexagon Pack as the primary building/environment palette, KayKit Prototype Bits for missing non-character objects and markers, and KayKit Resource Bits for resource visuals such as gold. Grass, river terrain, hills, trees, rocks, and medieval village props SHALL use matching ready-made assets from the free medieval pack. Farm visuals SHALL be represented by an appropriate free medieval building or prop, with a clear farm label. Food scenery SHALL use a medieval prop such as a sack with clear food labeling instead of the prototype can. Combat units SHALL use free rigged Adventurers and Skeletons characters with compatible locomotion, melee, ranged shooting, casting, hit and death clips and matching weapons. Berserker SHALL use the free Barbarian and Mage the free Mage; Skeleton Warrior, Minion, Rogue and Mage SHALL supply their enemy counterparts. Required character animations SHALL NOT be replaced by dummy models, scale pulses, or labeled markers. Paid tiers SHALL NOT be required. Gold, food, wood, stone, metal and cloth SHALL be the six economy resources. Stonecutter, Metal Mine, Weaver and Market SHALL have identifiable labeled representations using the bundled free palette; Gold Mine and Metal Mine SHALL be distinguishable. New resource labels and suitable bundled props SHALL distinguish stone, metal and cloth without claiming that decorative assets grant resources. Resource Bits SHALL supply free gold and wood props; free medieval sacks/grain SHALL supply food. Paid food, coin, character or source tiers SHALL NOT be required. The selected assets, required textures/buffers, included license texts, and recorded official source/version information SHALL be available from a clean checkout without runtime downloads.

#### Scenario: Free assets cover the match
- **WHEN** a contributor prepares and runs the graphical game from a clean checkout
- **THEN** buildings, grass plots, river, hills, and decorations use the medieval palette and combat units use rigged characters and their required animations without paid content
- **AND** food uses a labeled medieval prop and all six resources and new buildings are identifiable using bundled free assets

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
Graphical clients SHALL display each city as a medieval countryside landscape with nine stable hexagonal building plots arranged in three staggered rows, with purchased and locked plots visibly distinct and surrounding terrain continuing beyond the visible world edges. The landscape SHALL include surrounding grass, a river along one side, wooded hills, rocks, and small village props, including when all building plots are empty. A raised rear terrace, lower riverbank and wooded slopes SHALL create visible elevation differences; peripheral mountains, a bridge and appropriate building props SHALL enrich the scenery without hiding plots or battles. Terrain elevation SHALL remain cosmetic and unit rendering SHALL meet its visible ground surface without changing authoritative contact or targeting. The battlefield SHALL be a readable grassy approach with no painted road markings or modern road slab. Decorations and terrain SHALL NOT occupy building slots or change movement, damage, economy, or city capacity. Players SHALL be able to inspect every player's city and battle, identify their own city, and distinguish soldiers from enemies. The visual representation SHALL follow authoritative state, including enemy transfers, casualties, and eliminated cities. The game SHALL NOT require manual unit control, movable terrain, or a board-folding effect.

#### Scenario: Attractive empty starting village
- **WHEN** a match starts with all nine plots empty
- **THEN** the player sees a coherent grass landscape with the home, defender, river, wooded hills, rocks, and village props continuing beyond the visible world edges
- **AND** five initially usable plots, four locked expansion plots and the battle approach remain distinguishable from scenery

#### Scenario: Watch a redistributed wave
- **WHEN** another city falls during a wave
- **THEN** clients show that city's elimination and the transferred enemies fighting at their authoritative destinations
- **AND** the player can inspect the surviving cities while combat continues

#### Scenario: Upgraded buildings are recognizable
- **WHEN** a building is upgraded from level one to level two or higher
- **THEN** its structure or surrounding props visibly distinguish the upgrade without relying only on uniform scaling
- **AND** clicking its visible roof or raised plot still selects the same stable slot

### Requirement: Direct world selection
A player SHALL select a building slot by clicking its visible hexagonal plot or the visible building occupying it. Both targets SHALL resolve to the same authoritative slot identity, including while locked and after purchase, construction, upgrading, sale and reconnecting. Hover and selection feedback SHALL be restrained and SHALL NOT obscure the terrain or imported building materials. Clicking a plot or building SHALL select it without automatically spending resources. Clicking scenery, gaps outside buildable plots, or the bottom panel SHALL NOT select a different world slot or cause an unintended gameplay action.

#### Scenario: Select each empty hex plot
- **WHEN** the player clicks each of the nine empty plot centers in turn
- **THEN** each click selects exactly the corresponding slot and shows construction actions for a purchased plot or the gold purchase quote for a locked plot
- **AND** no resources are spent until the player explicitly activates construction or plot purchase

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

#### Scenario: Retained death still needs visual clearance
- **WHEN** a visible casualty retains its authoritative positions or transit reservations until death end
- **THEN** living models remain clear of its frozen death pose throughout that interval
- **AND** graphical clearance assertions include current visible dying bodies rather than filtering them out of the occupied battlefield

#### Scenario: Frame rate does not order combat
- **WHEN** compatible attacks and moves begin on the same authority tick
- **THEN** clients sample each action from that tick irrespective of local frame rate
- **AND** initiative does not add presentation-only delays that change the declared action timing

#### Scenario: Overview retains readable unit roles
- **WHEN** a player observes shared combat hexes at the supported overview scale
- **THEN** input-transparent role labels and health bars remain readable above the models
- **AND** the directional melee guide still identifies its locked target during windup

#### Scenario: Far-side melee target is readable
- **WHEN** an anchored melee occupant attacks an eligible far-side occupant in an adjacent shared hex
- **THEN** the directional strike cue identifies the actual locked target and its landed impact at the declared tick
- **AND** the miniature remains at its footprint anchor and the display reads as an abstract tabletop attack

#### Scenario: Verify neighboring occupied cells before fixing scale
- **WHEN** the initial board/action model reaches its early gameplay gate
- **THEN** a checked-in short source combat check captures actual bundled rigs in two occupied adjacent cells with near/far targets and a simultaneous exchange
- **AND** readable linked attacks, health/role identification and shared positions are inspected before board spacing/footprint layout and broad presentation integration are accepted

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
The start screen and multiplayer entry backdrop SHALL render the same terrain arrangement, elevations, home, defender, nine empty plots with the starting five-purchased/four-locked appearance, river, bridge and static decorations as a fresh starting village, with consistent asset scale, materials and lighting. Menu framing SHALL fill the background around its readable controls and use the same landscape coverage rules for its viewport. Menu scenery SHALL remain non-interactive and SHALL NOT create a gameplay session, connect to an authority or expose match actions. Returning to the menu SHALL restore the empty starting countryside without retaining a previous match's purchased expansions, constructed buildings or units. Source and exported graphical clients SHALL provide this same presentation; headless roles SHALL remain free of visual instantiation.

#### Scenario: Enter a solo match from the menu
- **WHEN** the player views the start screen and then starts a fresh solo match
- **THEN** the same starting countryside arrangement, assets and scales are recognizable in both views
- **AND** the menu has no match state while the match exposes its nine empty selectable plots with five usable and four offering expansion

#### Scenario: Return after construction
- **WHEN** the player returns to the menu after constructing buildings in a match
- **THEN** the filled backdrop shows the empty starting village without the match's buildings or combat units
- **AND** menu controls remain usable without world selection or gameplay input

#### Scenario: Run the packed menu
- **WHEN** an exported graphical client displays its start screen and enters solo play without access to the source project
- **THEN** both views load the shared countryside and fill their visible world areas from bundled assets

### Requirement: Readable unit progression and wave rewards
The graphical match view SHALL expose an observed army's unit levels, distinguish a boss with a boss label and level, and use authoritative resolved profiles for displayed health and damage. Army inspection SHALL show authoritative unit size, with normal units at size two and bosses at size six. Mixed-level soldiers SHALL remain distinguishable through labels or grouped army details without adding individual upgrade controls. The view SHALL show the latest completed wave's actual gold, food and wood reward for the observed city, including on the final victory screen. A summary SHALL identify its wave and SHALL NOT imply a second payment on repeated snapshots, city switches or reconnect. A fresh match SHALL clear the prior match's reward summary. Unit levels SHALL NOT require new character assets or modify unit size through visual scaling.

#### Scenario: Observe a boss
- **WHEN** the observed city receives a boss allocation or a transferred boss
- **THEN** the view identifies the boss and its level and displays its authoritative current and maximum health and size six

#### Scenario: Restore the reward summary
- **WHEN** a player reconnects after wave ten clears
- **THEN** the current balances and latest summary show that city's actual wave-ten reward without playing another payment effect

#### Scenario: View mixed-level survivors
- **WHEN** level-one veterans and level-three recruits belong to the observed army
- **THEN** army details distinguish their levels and use their own profiles rather than displaying all soldiers at the current building level

### Requirement: Readable resource progression and contextual economy controls
Construction choices SHALL be grouped into production, recruitment and advanced/defensive buildings with stable ordering. Affordable eligible choices SHALL be visually emphasized; unavailable choices SHALL remain visible and greyed, with readable missing resource amounts and producer names or the applicable ownership, readiness, pause or phase reason. Explanation SHALL remain accessible even when the purchase button is disabled and SHALL NOT rely only on color. Buildings SHALL NOT be hidden until a resource is discovered. Selected locked plots SHALL offer only their purchase quote and inspection; purchased empty plots SHALL offer construction. Selecting occupied plots SHALL expose their normal actions and an explicit Sell action with the exact refund and retained-army/research explanation. Markets SHALL expose fixed sale bundles, available stock and exact gold proceeds; food-sale previews SHALL update the projected upkeep balance. Affordability, costs, refunds, rates and upkeep SHALL derive from authoritative state/catalogs and refresh after accepted actions, production, city switches and reconnects without guessing that a request succeeded. Layout SHALL keep all six resource balances and essential actions readable at 1100x820 and 1280x720 while preserving world selection and the nine-plot overview. Economically unavailable controls SHALL never become enabled solely by a presentation calculation.

#### Scenario: Explain an unavailable Arcanum
- **WHEN** an owner selects an empty purchased plot with enough gold and wood but insufficient stone
- **THEN** Arcanum remains visible and greyed with the missing stone amount and Stonecutter as its production source
- **AND** acquiring the required resources highlights the eligible action without requiring ownership of a Stonecutter or any cloth

#### Scenario: Purchase land through actual world selection
- **WHEN** the player selects a locked plot and explicitly activates its affordable purchase
- **THEN** authoritative acceptance makes that same plot usable and displays construction choices and the updated next expansion price
- **AND** selecting or hovering the plot alone spends nothing

#### Scenario: Sell a building and inspect the cleared plot
- **WHEN** the player activates the selected building's displayed Sell action
- **THEN** acceptance shows the exact resource refund, removes its model and actions and keeps that plot selected as usable empty land
- **AND** existing army and research remain visible without being refunded or healed

#### Scenario: Sell resources through a Market
- **WHEN** the player selects an owned Market and activates a valid displayed sale bundle
- **THEN** accepted resource and gold changes match the quote and duplicate observations replay no sale
- **AND** selling the last Market removes trading controls immediately

#### Scenario: Wider economy stays readable
- **WHEN** the six-resource view is displayed at either supported verification size with a selected building and the expanded city
- **THEN** balances, complete costs, upkeep and actions remain readable and reachable without horizontal clipping
- **AND** plots, home and battle approach retain the supported overview and click targets

### Requirement: Visible upkeep forecast and reserve participation
During editable building and preparation the observed city's view SHALL show current food, total next-battle upkeep, projected food payment and which soldiers would participate or sit out under the authoritative stronger-first, stable-identity allocation. Recruitment SHALL show equipment costs separately from per-battle food upkeep. A shortage SHALL be explained before Ready without disabling readiness solely for food. Accepted recruitment or food sales SHALL refresh the forecast. During combat the view SHALL distinguish participating soldiers, fed capacity-queued soldiers and unfed reserves, showing actual paid upkeep for that wave. Reserves SHALL remain in army details with retained level and health but SHALL NOT be rendered as deployed combatants, walking capacity queues or casualties. Reconnect SHALL restore the current status without replaying a feeding, death or recruitment effect.

#### Scenario: Preview a food sale's consequence
- **WHEN** selling a food bundle would leave insufficient food for the whole army
- **THEN** the Market preview identifies the resulting payment and units that would sit out before the sale is activated
- **AND** acceptance updates the common upkeep forecast from authoritative state

#### Scenario: Start a battle underfed
- **WHEN** the player readies with a visible shortage and the shared battle starts
- **THEN** the displayed food deduction and participating/reserve army agree with the preview if no relevant inputs changed
- **AND** unfed soldiers remain inspectable without appearing on the active battlefield or playing a death animation

#### Scenario: Reconnect with reserves
- **WHEN** a client reconnects to a wave containing unfed soldiers
- **THEN** it sees the retained current payment and reserve status without paying food again or admitting those reserves to combat
