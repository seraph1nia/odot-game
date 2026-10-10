# city-tabletop Specification

## Purpose

Present authoritative cities and battles as a readable miniature 3D tabletop using the declared owner-permitted authored asset palette while keeping interaction simple.

## Requirements

### Requirement: City interaction and match feedback
Clients SHALL expose construction on empty purchased plots, locked-plot purchases, building upgrades and sales, explicit building-specific archetype recruitment, independent technology research, Market sales, ready/unready, and pause/resume through a compact control panel anchored to the bottom of the game window. The interface SHALL show complete construction/upgrade/recruitment/research costs, building refund and Market quotes, gold, food, wood, stone, metal and cloth in a separate top-right resource table; next expansion cost only with a selected locked plot; current army upkeep demand, food forecast, purchased technologies and soldier details through compact contextual inspection; city health at the home through a percentage health bar; building level in occupied-plot details; phase including final preparation through a small vertical stage indicator with compact turn and wave counters and boss indication; readiness through Ready/Unready; and exceptional connection status and outcome, distinguishing all-cities-fallen defeat from battle-stalled defeat. A stalled result SHALL explain that the battle could not finish, rather than claim every city fell. City inspection controls SHALL identify the player's own city and the observed city using a very small `< [YOU] >` selector in the existing city-navigation location, with text left/right buttons and a concise player identifier when observing another city. Cycling SHALL visit roster cities in stable player order, wrap at either end, clear plot selection and fit the new city overview; arrows SHALL be disabled when only one city is available. Lobby roster/host/readiness information SHALL remain available in the lobby without restoring the verbose player bar during gameplay. Construction, plot purchase, building sale, upgrade, Market trade and recruitment actions SHALL be contextual to the directly selected world plot or building; without a selection, the panel SHALL NOT offer an actionable default plot. A separate Research control SHALL open the observed city's technology tree without a plot selection or owned Research Tower; only eligible own-city purchases SHALL be enabled. Routine select-a-plot and Open plot X / choose a building prompts SHALL be omitted; meaningful disabled-action reasons and occupied-building details SHALL remain accessible. The interface SHALL NOT include a duplicate numbered grid for selecting building slots. A cooperative lobby SHALL expose the current roster, identify the host where applicable, and expose the start action only to eligible players. Hosted lobbies SHALL offer Invite friends; solo play SHALL enter the first building turn directly. Lobby, match, outcome, and disconnected views SHALL offer Return to menu through Settings and a cancelable confirmation, with no Return to menu control in the bottom HUD. The bottom HUD SHALL omit ODOT, P1/Your City headings, City/Army summary rows, purchased land counts, resource summary rows, the verbose P1/You/Here bar and routine Match started text. Server errors, actionable feedback, pause/disconnection and outcomes SHALL remain visible when relevant. Connection and host-loss feedback SHALL reflect the selected session mode. Unavailable actions SHALL be visibly disabled or explained, and server rejection SHALL be surfaced. A disconnected guest SHALL offer reconnect without discarding its private resume information while the original authority may still be available, and input SHALL remain disabled until resynchronization completes. An ended hosted session SHALL offer return to the start screen without suggesting authority migration.

#### Scenario: Recruit through the graphical interface
- **WHEN** a player clicks their recruitment building during an eligible building or preparation phase
- **THEN** the bottom panel displays its level and explicit recruitment actions for each archetype that building unlocks and its authoritative recruit level, resulting health/damage, rounded material/gold costs and separate per-battle food upkeep
- **AND** activating recruitment shows the authoritative resource and army changes after acceptance

#### Scenario: Pause and reconnect feedback
- **WHEN** the match is paused and a client reconnects
- **THEN** the compact match view identifies pause and the observed city, keeps roster connectivity accessible through inspection, and shows a resume control
- **AND** it does not display frozen gameplay as active progression

#### Scenario: No plot has been selected
- **WHEN** a synchronized player first views a city or switches to another city
- **THEN** the panel omits routine selection instructions and no economic action can target a previously selected plot
- **AND** resources, city inspection, and available match controls remain accessible

#### Scenario: Inspect another player's city
- **WHEN** a player switches to another city's view and selects a building there
- **THEN** contextual controls show the observed city's building information, the top-right table shows its resources, and spending or recruitment on that city is visibly prevented

#### Scenario: Recruit a ranged soldier through controls
- **WHEN** the player selects their Archery Range and activates its enabled Crossbowman control
- **THEN** the accepted request adds a Crossbowman and updates the resource table with the authoritative material deduction and army inspection with the new soldier without a food recruitment charge
- **AND** observing a foreign recruitment building, becoming ready, pausing or entering combat disables its recruitment controls

#### Scenario: Inspect research and preparation
- **WHEN** a player opens Research in final preparation without selecting a plot
- **THEN** the research panel shows points, progress, technologies, prerequisites, exclusive locks and authoritative point costs
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
- **THEN** the panel displays the battle-stalled reason and Settings offers confirmed return to menu
- **AND** it does not describe surviving cities as fallen

### Requirement: Requested free asset palette
The game SHALL use owner-permitted authored `odot-game-assets` exports as its complete building, character, equipment, prop and environment palette, without runtime downloads or legacy 3D fallbacks. Buildings and scenery SHALL retain the exported materials and use appropriate available substitutes where a one-to-one match does not exist. Farm's food-producing model SHALL be a labeled Bakery, Lumbermill's wood-producing model a Woodcutter hut identifiable through contextual controls (see [presentation titles](../../../docs/assets.md#names-versus-authoritative-identities)), Arcanum a labeled Magic academy and CatapultTower a labeled Bombard tower without changing authoritative roles. Gold Mine and Metal Mine, Stonecutter, Weaver, Market and Town hall SHALL have identifiable labeled representations. Friendly sword and ranged soldiers SHALL use Knight and Archer names with compatible authored equipment; enemy labels SHALL describe their actual bone, hooded or horned silhouettes. Required idle, locomotion, attack, hit and death animations SHALL NOT be replaced by dummy models, scale pulses or labeled markers. Gold, food, wood, stone, metal and cloth SHALL remain the six economy resources. Text resource names and contextual details SHALL distinguish resources without adding stockpile labels to the board or claiming decorations grant resources. Bounded physical stockpiles SHALL use distinct authored cargo/provision/wood substitutes, documented honestly rather than claiming unavailable gold bars or sacks. Selected exports and all required embedded materials/textures/buffers/clips, applicable notices and immutable source/hash records SHALL be available from a clean checkout. UI/audio and original code-driven markers/effects SHALL remain outside this 3D migration.

#### Scenario: Free assets cover the match
- **WHEN** a contributor prepares and runs the graphical game from a clean checkout
- **THEN** buildings, plots, river and decorations use the authored palette and combat units use authored rigged characters and their required animations without paid content or legacy fallback
- **AND** distinct unlabeled provision, cargo and timber props retain bounded physical stockpiles while all six resources and producer roles remain identifiable through the resource table and contextual details

#### Scenario: Clean checkout contains animated characters
- **WHEN** a contributor imports source or launches a packed graphical export without downloading assets at runtime
- **THEN** soldiers and enemies load their authored rigged models, matching equipped weapons, textures and required animation clips
- **AND** included provenance identifies immutable sources, hashes and owner permission without inventing a license or extending permission to reference files

### Requirement: Headless and exported operation
The authoritative dedicated server and automated headless roles, including a playing-host role without presentation, SHALL operate without graphics, audio, or visual asset instantiation. Ordinary exported graphical launches SHALL display the start screen and display the tabletop when a session is entered; explicit client launches SHALL retain direct connection behavior. Exported graphical clients SHALL include the selected assets, usable local/hosted controls, and the chosen native Steam dependencies. Dedicated-server exports SHALL continue to run and communicate correctly when visual resources are stripped. Local/ENet headless roles SHALL NOT depend on Steam initialization, account login, graphical preferences, or audio.

#### Scenario: Headless network match
- **WHEN** separate automated headless clients connect to a dedicated server
- **THEN** they can build, recruit, advance turns, and complete battles through the same game protocol without creating the tabletop view

#### Scenario: Exported graphical client
- **WHEN** the exported desktop client enters a session or uses explicit client arguments to connect to a server
- **THEN** it displays the board, selected authored models, and usable match controls without access to the source project

#### Scenario: Exported solo play without Steam
- **WHEN** the exported desktop client starts without a running Steam client and the player selects Single player
- **THEN** the tabletop and normal gameplay controls work without source-project access, a network socket, or an external server

### Requirement: Medieval landscape and visible battles
Graphical clients SHALL display each city as a coherent fantasy countryside landscape with nine stable hexagonal building plots arranged in three staggered rows, with purchased and locked plots visibly distinct and surrounding authored terrain continuing beyond the visible world edges. The landscape SHALL include a detailed woodland-ground palette, a continuous river along one settlement edge, a matching authored bridge, deliberately clustered woodland, rocky outcrops and small village props, including when all building plots are empty. Shallow stepped wooded terraces and a lower riverbank SHALL create cosmetic elevation differences using available authored terrain rather than retaining legacy slopes/mountains or generating replacement meshes. The battlefield SHALL be a readable grassy approach with no painted road markings or modern road slab. Unit rendering SHALL meet its visible ground surface without changing authoritative contact or targeting. Decorations and terrain SHALL NOT occupy building slots, obstruct combat/selection/HUD or change movement topology, damage, economy or city capacity. Players SHALL be able to inspect every player's city and battle, identify their own city, and distinguish soldiers from enemies. The visual representation SHALL follow authoritative state, including enemy transfers, casualties, and eliminated cities. The game SHALL NOT require manual unit control, movable terrain, or a board-folding effect.

#### Scenario: Attractive empty starting village
- **WHEN** a match starts with all nine plots empty
- **THEN** the player sees a coherent authored woodland landscape with the home, defender, continuous river/bridge, wooded terraces, rocks and village props continuing beyond the visible world edges
- **AND** five initially usable plots, four locked expansion plots and the battle approach remain distinguishable from scenery

#### Scenario: Watch a redistributed wave
- **WHEN** another city falls during a wave
- **THEN** clients show that city's elimination and the transferred enemies fighting at their authoritative destinations
- **AND** the player can inspect the surviving cities while combat continues

#### Scenario: Upgraded buildings are recognizable
- **WHEN** a building is upgraded from level one to level two or higher
- **THEN** its structure or surrounding authored props visibly distinguish the upgrade without relying only on uniform scaling
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
Graphical clients SHALL reset the fitted overview of the observed city on a non-repeating Space press during eligible world interaction and SHALL NOT display a Reset view button. Modal dialogs, text input, focused UI controls consuming Space and an unfocused window SHALL take priority; Space SHALL NOT both activate a focused control and reset the camera. A compact tooltip or help entry SHALL explain the shortcut. Entering a tabletop, switching to a different city or receiving a fresh match SHALL initialize the overview and clear held navigation. Resizing the window or changing HUD height SHALL recompute the fitted base view and preserve the player's relative zoom and city-relative pan within current limits. Reconnecting to the same match and observed city in the same tabletop SHALL retain camera adjustments; camera state SHALL NOT persist across application launches.

#### Scenario: Reset a close view
- **WHEN** the player presses Space during eligible world interaction after zooming and panning
- **THEN** the observed city's fitted overview is restored with all plots and the battle approach above the HUD
- **AND** world selection and resources are unchanged

#### Scenario: Switch city or match
- **WHEN** the observed city changes or a fresh match replaces the current match
- **THEN** the new city's overview is shown without inheriting the previous city's pan or zoom

#### Scenario: Resize an adjusted view
- **WHEN** the window or HUD size changes while the player has adjusted the view
- **THEN** the relative zoom and city-relative pan are preserved within limits against the recalculated base framing
- **AND** Space still restores a fully fitted overview

#### Scenario: Reconnect within the same view
- **WHEN** the client reconnects to the same match and observed city without replacing its tabletop
- **THEN** its camera adjustments are retained and rebuilt world targets remain selectable

#### Scenario: Space belongs to a focused control or modal
- **WHEN** the player presses Space while a consuming UI control is focused or a modal is open
- **THEN** the UI handles the key and the camera remains unchanged
- **AND** holding Space does not repeatedly reset or trigger a gameplay action

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
At 1100x820 and 1280x720 and after supported viewport or HUD changes, all visible world area SHALL be covered by the authored countryside without exposing the terrain patch perimeter, its exterior slab walls or the flat environment background beyond it. Coverage SHALL include every permitted camera zoom and pan position when navigation is available. Extending scenery SHALL NOT enlarge the fitted overview bounds so as to shrink the playable village; at the default/reset overview all nine plots, home, defender and the battle approach SHALL remain visible above the HUD. Authored river edges and terrain transitions SHALL join coherently throughout visible coverage, and tall scenery SHALL remain clear of plot and battle sightlines. Terrain SHALL reuse imported geometry/shared material resources and bounded chunked instancing rather than instantiate a separate full scene per ordinary floor cell.

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
The graphical match view SHALL expose an observed army's unit levels as Roman numerals above the left part of their health bars, with full names, descriptions, faction, boss identification and level in click inspection and army details rather than overhead name/role codes. Displayed health and damage SHALL use authoritative resolved profiles. Army inspection SHALL show authoritative unit size, with normal units at size two and bosses at size six. Mixed-level soldiers SHALL remain distinguishable through Roman level markers or grouped army details without adding individual upgrade controls. The view SHALL show the latest completed wave's actual gold, food, wood and research reward for the observed city, including on the final victory screen. A summary SHALL identify its wave and SHALL NOT imply a second payment on repeated snapshots, city switches or reconnect. A fresh match SHALL clear the prior match's reward summary. Unit levels SHALL NOT require new character assets or modify unit size through visual scaling.

#### Scenario: Observe a boss
- **WHEN** the observed city receives a boss allocation or a transferred boss
- **THEN** the overhead bar shows its Roman level and click inspection identifies it as a boss with its level, authoritative current/maximum health, damage per attack and size six

#### Scenario: Restore the reward summary
- **WHEN** a player reconnects after wave ten clears
- **THEN** the current balances and latest summary show that city's actual wave-ten reward without playing another payment effect

#### Scenario: View mixed-level survivors
- **WHEN** level-one veterans and level-three recruits belong to the observed army
- **THEN** army details distinguish their levels and use their own profiles rather than displaying all soldiers at the current building level

### Requirement: Readable resource progression and contextual economy controls
Construction choices SHALL be grouped into production, recruitment and advanced/defensive buildings with stable ordering. Affordable eligible choices SHALL be visually emphasized; unavailable choices SHALL remain visible and greyed, with explicit Need N more resource explanations and producer names or the applicable ownership, readiness, pause or phase reason. Explanation SHALL remain accessible even when the purchase button is disabled and SHALL NOT rely only on color. Buildings SHALL NOT be hidden until a resource is discovered. Selected locked plots SHALL offer only their purchase quote and inspection; purchased empty plots SHALL offer construction. Selecting occupied plots SHALL expose their normal actions and an explicit Sell action with the exact refund and retained-army/research explanation. Markets SHALL expose fixed sale bundles, available stock and exact gold proceeds; food-sale previews SHALL update the projected upkeep balance. Producer choices SHALL show their complete text cost alongside their current per-turn output. Producer upgrades SHALL show current to next output with a consistent unit label. The explicit gold-paid Lumbermill recovery choice SHALL be visible only at zero wood, show its complete quoted gold price and follow normal eligibility; normal builds SHALL never silently substitute currency. Affordability, costs, refunds, rates, income and upkeep SHALL derive from authoritative state/catalogs and refresh after accepted actions, production, city switches and reconnects without guessing that a request succeeded. Layout SHALL keep all six resource balances and essential actions readable at 1100x820 and 1280x720 while preserving world selection and the nine-plot overview. Economically unavailable controls SHALL never become enabled solely by a presentation calculation.

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

#### Scenario: Understand a producer before purchase
- **WHEN** an owner inspects default Lumbermill construction or a level-one Lumbermill upgrade
- **THEN** construction shows 1 wood and +1 wood/turn, and upgrade shows 2 wood plus 2 stone and output 1 to 2 wood/turn
- **AND** missing materials and producer sources remain readable even when the action is disabled

The ordered home overview and explicit purchase control SHALL expose the next authoritative gold purchase quote, occupied/available size and stable first-fit destination. A selected Town hall SHALL display occupied/available storage spots and independently quoted capacity/healing upgrades, current percentage and paid-recovery eligibility. Selection SHALL never spend or transfer on its own; full destinations and occupied-hall sale SHALL be explained.

### Requirement: Visible upkeep forecast and reserve participation
The observed city SHALL have a very small text-only Upkeep table directly below its resource table, without opening City details. During building and preparation, including while ready, its first row SHALL show Next battle food demand. When every soldier can be fed, the second row SHALL show Food after payment using current food minus projected payment. On shortage the second row SHALL instead show the exact unfunded count with a textual cue rather than color alone: soldiers that will sit out for a field-only army, or Unfunded soldiers when storage is present, without suggesting funded stored units fight. Empty armies SHALL show zero demand. Current food, projected payment and which soldiers would participate or sit out under the authoritative field-first allocation defined in [army-roster](../army-roster/spec.md) SHALL remain accessible through inspection. Pause and transport loss SHALL identify paused or stale synchronized values without suggesting another payment. Recruitment SHALL show equipment costs separately from per-battle food upkeep. A shortage SHALL be explained before Ready without disabling readiness solely for food. Accepted recruitment or food sales SHALL refresh the forecast. During combat the view SHALL distinguish participating soldiers, fed capacity-queued soldiers and unfed reserves, showing actual paid upkeep for that wave. The compact table SHALL label that receipt Paid this battle with its wave and reserve count; outcome views SHALL identify Last battle, and fresh sessions SHALL clear old receipts. Reserves SHALL remain in army details with retained level and health but SHALL NOT be rendered as deployed combatants, walking capacity queues or casualties. Reconnect SHALL restore the current status without replaying a feeding, death or recruitment effect.

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

#### Scenario: Read the food consequence without opening Details
- **WHEN** an observed city has 15 food and six living Swordsmen before battle
- **THEN** the visible Upkeep table shows Next battle 6 food and Food after payment 9
- **AND** a shortage replaces the second row with the exact sit-out count and does not disable Ready solely because of food

#### Scenario: Combat shows an actual payment
- **WHEN** preparation starts combat and the owner changes cities or reconnects
- **THEN** the table shows the observed city's actual wave-tagged paid food and reserve count
- **AND** the display grants no food, charges no upkeep and does not imply a new payment

Demand SHALL include stored units and identify field/storage food separately. Funded stored units SHALL not be described or rendered as participating combatants. The roster SHALL explain that recovery uses food funded in the last completed battle and occurs only at actual production.

### Requirement: Mouse drag tabletop panning
Graphical clients SHALL support left-click-and-drag camera panning starting within the eligible visible world area. The view SHALL follow the dragged ground plane while preserving the angled orthographic projection, zoom and existing city-relative travel bounds. Movement below a small consistent screen-space threshold SHALL remain a normal click selecting the same plot/building; once the threshold is exceeded the gesture SHALL pan without selecting a plot on press or release, submitting commands or spending resources. Dragging SHALL work over plots, buildings and scenery. HUD surfaces including the resource table, Settings and other modal dialogs SHALL block drag initiation. A modal opening, window focus loss, city/session replacement or button release SHALL end the gesture without stale movement resuming. Drag navigation SHALL remain available during pause, transport loss and outcome when a city view exists. Screen-space bars and markers SHALL follow the camera and remain input-transparent. Source and exported graphical clients SHALL behave alike.

#### Scenario: Drag across a building
- **WHEN** the player left-drags from a visible building beyond the drag threshold and releases over another plot
- **THEN** the camera pans within its bounds and neither plot selection nor authoritative state changes
- **AND** a subsequent short click still selects its intended plot normally

#### Scenario: Interrupt a drag
- **WHEN** a modal opens or the window loses focus during a drag
- **THEN** camera movement stops immediately and reopening game input does not resume the old gesture
- **AND** dragging from a HUD surface or modal never moves the world camera

### Requirement: Compact construction and turn progression
Construction choices SHALL occupy a three-column, three-row area with the existing Production, Army, Defense and Trade category selection retained above it. Smaller groups SHALL NOT create fake actionable buildings. Every choice SHALL retain readable text costs, stable ordering, affordability and accessible unavailable-action explanations. The compact vertical stage sequence SHALL list Building turn 1, Building turn 2, Building turn 3, Preparation and Combat, highlighting the current authoritative stage with a non-color cue. Production SHALL remain the result of resolving a building turn, not an invented persistent phase. Small wave/total and turn/3 counters and boss identification SHALL remain alongside the sequence. Ready/Unready and Pause/Resume SHALL sit side by side below it with existing authoritative eligibility. Lobby, victory and defeat SHALL have clear state-specific presentation rather than highlighting an active stage incorrectly.

#### Scenario: Resolve the third building turn
- **WHEN** authoritative readiness resolves building turn three into preparation
- **THEN** the stage highlight moves to Preparation, wave/turn counters match the snapshot and Ready communicates starting combat
- **AND** no fourth production turn is implied or introduced

#### Scenario: Browse construction
- **WHEN** a player selects an owned purchased empty plot and changes category
- **THEN** choices occupy at most the three-by-three area beneath the category selection with complete text costs and accessible disabled reasons
- **AND** selecting a category does not build anything or change the selected plot

### Requirement: Vertical resource table
Graphical match views SHALL show a narrow top-right three-column table with Resource, Stock and Income/turn columns, ordered Gold, Food, Wood, Stone, Metal and Cloth. All six rows SHALL show exact authoritative balances and projected production including zero, for the currently observed city, and SHALL refresh after synchronization, production, accepted spending, rewards, city switches and reconnect. Numeric stock and income columns SHALL be right-aligned; positive income SHALL use + and zero SHALL use 0. Building turns SHALL label income Income/turn, while preparation and combat SHALL identify Next building turn and communicate that Ready for battle grants no income. Fallen and terminal cities SHALL show zero future income with an inactive cue; an unrepresentable custom projection SHALL show an explicit unavailable value instead of a guessed amount. Pause and transport loss SHALL retain last synchronized values with paused/stale context. Income SHALL refresh after producer construction, upgrades or sales even without a production payment. The table SHALL contain no resource icons and SHALL NOT introduce additional resource types. Resource totals and purchased-land summaries SHALL NOT be repeated in the bottom HUD. Contextual costs, refunds, trade quotes and food upkeep SHALL remain accessible. Pointer input over the table SHALL NOT select or zoom the world behind it.

#### Scenario: Inspect and reconnect
- **WHEN** a player switches to a foreign city or reconnects to the currently observed city
- **THEN** every resource row immediately uses that city's current synchronized balance
- **AND** foreign-city spending stays unavailable and no historical earning animation is replayed

#### Scenario: Income agrees with current capacity
- **WHEN** a living default city owns a level-one Lumbermill and level-two Metal Mine
- **THEN** its table shows +2 gold, +1 wood, +8 metal and 0 income for other resources
- **AND** selling a producer updates income after authoritative acceptance without granting production

#### Scenario: Preparation does not promise production
- **WHEN** a city reaches Preparation after its third production
- **THEN** income is labeled Next building turn and Ready communicates battle entry without income
- **AND** combat uses future-production wording while terminal and fallen cities show no future income

#### Scenario: The combined stack fits supported layouts
- **WHEN** the resource and Upkeep tables and a unit inspector are visible at 1100x820 or 1280x720
- **THEN** their essential values fit inside the viewport, the inspector fits below the combined stack and above the approximately 180px HUD, and all nine plots and the battle approach remain visible and selectable at overview
- **AND** pointer input inside either table blocks world selection, zoom and drag initiation

### Requirement: Clean plot and home markers
Unpurchased plots SHALL display a compact bundled gold/buy marker instead of Locked land text. Selecting a locked plot SHALL expose its exact purchase quote and existing explicit purchase action; hovering or selecting the marker SHALL NOT spend resources. The marker SHALL disappear after authoritative purchase. The visible home SHALL display a compact health bar with X% text instead of P1/You, heart and raw health labels. Its fill SHALL be authoritative current city health divided by authoritative maximum city health, clamped to zero through one; its integer percentage SHALL use that fraction rounded to the nearest whole percent. Full health SHALL display 100% and a fallen city SHALL display 0% with a distinguishable fallen state. Home bars SHALL follow camera/layout changes, freeze their health on transport loss, refresh from synchronization, and remain input-transparent. City ownership SHALL remain identifiable through the compact city selector. Unit health bars SHALL retain their authoritative health and lifecycle behavior while replacing overhead names/codes with Roman level markers and exposing full identity/stats through click inspection.

#### Scenario: Purchase marked land
- **WHEN** a player selects a gold-marked locked plot and explicitly confirms its affordable purchase action
- **THEN** authoritative acceptance removes the marker from that same plot and exposes construction choices
- **AND** the selected slot remains the same and the top-right balance updates once

#### Scenario: Show wounded city health
- **WHEN** a synchronized city has half its authoritative maximum health
- **THEN** the home bar shows half fill and 50% without the former player/heart/raw-health label
- **AND** the bar remains correctly positioned and does not consume plot input after camera movement

### Requirement: Clickable unit inspection
Every visible, deployed, living friendly and enemy unit in the observed city, including bosses, SHALL be selectable by a short left click on its model. Selecting a unit SHALL open one nonmodal centre-right popup with a visual preview of that unit, its readable name, short role description, level, faction and boss status where applicable, current/max health as X/Y plus a health bar, and damage per attack. Size SHALL remain inspectable including boss size six. Displayed numeric stats SHALL use the current authoritative resolved unit profile, including level/research/boss modifiers, with readable health/damage scaling; descriptions SHALL NOT imply unsupported mechanics. The popup SHALL refresh from observed authoritative state without changing combat, readiness, plot selection or spending. Shared pause and transport loss SHALL freeze its last observed values until synchronization updates them. Headless roles SHALL create no inspector or model preview.

The popup SHALL remain inside the usable viewport below the resource panel and above the bottom HUD. Clicking inside it SHALL keep it open and block underlying world input. Clicking elsewhere SHALL dismiss it and preserve the clicked target's ordinary eligible action exactly once. Clicking another unit SHALL replace the contents directly. A completed camera drag SHALL NOT select a unit or open a popup; starting a drag SHALL dismiss an existing popup. Unit hit selection SHALL choose the nearest visible unit at the clicked location rather than a plot behind it, with stable tie-breaking. Death visuals and undeployed/reserve units SHALL NOT be world-click inspection targets. Selection SHALL clear on death/removal from the observed city, city change, session/fresh-match replacement, or opening a blocking modal. Reconnect SHALL refresh the same surviving unit's stats or clear invalid selection. Source and exported graphical clients SHALL offer the same interaction.

#### Scenario: Inspect a wounded ranked unit
- **WHEN** the player short-clicks a visible unit with current health 525 and authoritative maximum health 1050 in the integer wire scale
- **THEN** the centre-right popup shows its model, name, description, level and resolved per-attack damage, with human-readable health 5.25/10.5 and half health fill
- **AND** the overhead marker shows the unit's Roman level with no unit name/code and no gameplay command is submitted

#### Scenario: Switch and dismiss inspection
- **WHEN** the player clicks another living unit and then clicks a plot or HUD control outside the popup
- **THEN** the first click replaces the popup with that unit's details and the outside click closes it
- **AND** the plot/control receives its normal eligible action once without click-through behind the inspector

#### Scenario: Drag over units
- **WHEN** a player drags from a unit past the camera gesture threshold
- **THEN** the camera pans, the inspector stays closed, and neither unit nor plot selection changes

#### Scenario: Refresh and clear inspected state
- **WHEN** the inspected unit receives synchronized damage, is paused or disconnected, and later dies or leaves the observed city
- **THEN** the popup uses the new authoritative health, freezes values during pause/loss, and closes when selection becomes invalid
- **AND** changing cities or replacing the match does not retain the old popup or preview

Eligible owned field unit inspection SHALL expose Retire and Send to Town hall actions with capacity/phase/ownership reasons. Stored units SHALL be selected from the Town hall roster and offer Send to battlefield and Retire with the same authority guards. No drag infrastructure SHALL be required.

### Requirement: Readable personal research tree
The research panel SHALL show the observed city's whole points, partial production progress, tower contribution, purchased nodes, prerequisites, eligible unit roles, effects and exact costs. Exclusive siblings and descendants SHALL remain visible with a readable permanent-lock explanation; the purchase action SHALL explicitly name the sibling branch it locks without requiring a second confirmation dialog. Unaffordable, prerequisite-locked, foreign-city, ready, paused, disconnected, combat and eliminated purchases SHALL be disabled with non-color-only reasons. Viewing research SHALL not pause or advance the shared match. The panel SHALL block underlying world input, fit supported window sizes using scrolling where needed, dismiss cleanly, refresh from acknowledged authoritative state and show no successful purchase effect before acceptance. Research Towers SHALL be identified by name and distinguishable two-level structures using the bundled palette; contextual output SHALL explain one/two points per full three-production cycle rather than per turn.

#### Scenario: First purchase without a research building
- **WHEN** a towerless eligible owner opens Research with three points and no selected plot
- **THEN** its affordable foundation can be purchased through actual controls and updates after acknowledgment
- **AND** the same tree is read-only when observing another city

#### Scenario: Permanent branch lock is visible
- **WHEN** an owner inspects Fire and then successfully purchases it
- **THEN** its purchase description identifies the Frost lock and Frost/descendants remain visible with the permanent reason afterward
- **AND** rejected or retried requests produce no duplicate success cue

### Requirement: Technology and status inspection
Unit inspection and army details SHALL describe actual acquired capabilities and current burn, poison and chill with authoritative strengths, poison stack count, remaining simulation duration and effective committed action timings where relevant. Visible living afflicted units SHALL have restrained distinguishable indicators using text/shape as well as color; indicators SHALL follow the shared presentation clock without owning damage scheduling or altering rig materials permanently. Poisoned queued units SHALL remain inspectable through army/enemy state without adding a world body. Death, effect expiry, city switches and fresh sessions SHALL clear or reconstruct indicator state; reconnect/history gaps SHALL reconstruct currently active effects without old sounds or application flashes. Existing Roman level markers, health bars, rigged actions and single-location movement SHALL remain readable. Chill descriptions SHALL promise slower future actions and SHALL NOT imply freeze, stun or interruption.

#### Scenario: Inspect poison and chilled timing
- **WHEN** a visible unit has two poison stacks and a committed chilled attack
- **THEN** its inspection shows the actual stacks and fixed action timing and its indicators agree with authoritative state
- **AND** pausing freezes remaining effect time and reconnect restores it without replaying applications
