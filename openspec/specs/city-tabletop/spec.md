# city-tabletop Specification

## Purpose

Present authoritative cities and battles as a readable miniature 3D tabletop using the declared owner-permitted authored asset palette while keeping interaction simple.

## Requirements

### Requirement: City interaction and match feedback
Clients SHALL expose construction on empty purchased plots, locked-plot purchases, building upgrades and sales, explicit building-specific archetype recruitment, independent technology research, Market sales, ready/unready, and pause/resume through a compact control panel anchored to the bottom of the game window. The interface SHALL show complete construction/upgrade/recruitment/research costs, building refund and Market quotes, gold, food, wood, stone, metal and cloth in a separate top-right resource table; next expansion cost only with a selected locked plot; current army upkeep demand, food forecast, purchased technologies and soldier details through compact contextual inspection; city health at the home through the [health-overlay contract](../game-feedback/spec.md#requirement-authoritative-overhead-unit-health-bars); building level in occupied-plot details; phase including final preparation through a small vertical stage indicator with compact turn and wave counters and boss indication; readiness through Ready/Unready; and exceptional connection status and outcome, distinguishing all-cities-fallen defeat from battle-stalled defeat. A stalled result SHALL explain that the battle could not finish, rather than claim every city fell. City inspection controls SHALL identify the player's own city and the observed city using a very small `< [YOU] >` selector in the existing city-navigation location, with text left/right buttons and a concise player identifier when observing another city. Cycling SHALL visit roster cities in stable player order, wrap at either end, clear plot selection and fit the new city overview; arrows SHALL be disabled when only one city is available. Lobby roster/host/readiness information SHALL remain available in the lobby without restoring the verbose player bar during gameplay. Construction, plot purchase, building sale, upgrade, Market trade and recruitment actions SHALL be contextual to the directly selected world plot or building; without a selection, the panel SHALL NOT offer an actionable default plot. A separate Worldtree control SHALL open the observed city's technology tree without a plot selection or owned Research Tower; only eligible own-city purchases SHALL be enabled. Routine select-a-plot and Open plot X / choose a building prompts SHALL be omitted; meaningful disabled-action reasons and occupied-building details SHALL remain accessible. The interface SHALL NOT include a duplicate numbered grid for selecting building slots. A cooperative lobby SHALL expose the current roster, identify the host where applicable, and expose the start action only to eligible players. Hosted lobbies SHALL offer Invite friends; solo play SHALL enter the first building turn directly. Lobby, match, outcome, and disconnected views SHALL offer Return to menu through Settings and a cancelable confirmation, with no Return to menu control in the bottom HUD. The bottom HUD SHALL omit ODOT, P1/Your City headings, City/Army summary rows, purchased land counts, resource summary rows, the verbose P1/You/Here bar and routine Match started text. Server errors, actionable feedback, pause/disconnection and outcomes SHALL remain visible when relevant. Connection and host-loss feedback SHALL reflect the selected session mode. Unavailable actions SHALL be visibly disabled or explained, and server rejection SHALL be surfaced. A disconnected guest SHALL offer reconnect without discarding its private resume information while the original authority may still be available, and input SHALL remain disabled until resynchronization completes. An ended hosted session SHALL offer return to the start screen without suggesting authority migration.

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
- **WHEN** a player opens Worldtree in final preparation without selecting a plot
- **THEN** the research panel shows points, progress, technologies, prerequisites, exclusive locks and authoritative point costs
- **AND** Ready communicates the final settlement boundary defined in [gameplay controls](../../../docs/gameplay.md#preparation-and-battle-controls)

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
The game SHALL use owner-permitted authored `odot-game-assets` exports as its complete building, character, equipment, prop and environment palette, without runtime downloads or legacy 3D fallbacks. Buildings and scenery SHALL retain the exported materials and use appropriate available substitutes where a one-to-one match does not exist. Farm's food-producing model SHALL be a labeled Bakery, Lumbermill's wood-producing model a Woodcutter hut identifiable through contextual controls (see [presentation titles](../../../docs/assets.md#names-versus-authoritative-identities)), Arcanum a labeled Magic academy and CatapultTower a Bombard tower identified through contextual controls without changing authoritative roles. Gold Mine and Metal Mine, Stonecutter, Weaver, Market and Town hall SHALL have identifiable labeled representations. Friendly sword and ranged soldiers SHALL use Knight and Archer names with compatible authored equipment; enemy inspection names SHALL describe their actual bone, hooded or horned silhouettes. Required idle, locomotion, attack, hit and death animations SHALL NOT be replaced by dummy models, scale pulses or labeled markers. Gold, food, wood, stone, metal and cloth SHALL remain the six economy resources. Text resource names and contextual details SHALL distinguish resources without adding stockpile labels to the board or claiming decorations grant resources. Bounded physical stockpiles SHALL use distinct authored cargo/provision/wood substitutes, documented honestly rather than claiming unavailable gold bars or sacks. Selected exports and all required embedded materials/textures/buffers/clips, applicable notices and immutable source/hash records SHALL be available from a clean checkout. UI/audio and original code-driven markers/effects SHALL remain outside this 3D migration.

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

Each settlement SHALL retain nine stable purchased/locked hex plots, its authored woodland ground/river/bridge/terraces/props and existing economy interactions. Combat SHALL transition to one complete connected battle diorama for all owners, using the same woodland/tabletop palette. Keeps, forward Watchposts, purchased towers, owned squad formations and enemy groups SHALL be physically identifiable with non-color-only ownership/status cues. Arena ground SHALL have restrained authored grass/moss/earth variation and dressed exterior edges, while active hexes/routes remain open. Scenery SHALL never change topology, numerical capacity, damage or economy, obscure deployment/picking or act as an invisible obstacle. Keep/Watchpost state SHALL follow accepted authority updates. Purchased inactive towers SHALL not appear destroyed merely because the owner fell. No manual RTS controls, movable terrain or board folding SHALL be added.

#### Scenario: Enter a four-player battle
- **WHEN** final preparation starts combat
- **THEN** every client sees the same connected four-sector arena with distinguishable objectives and all-owner formations
- **AND** each client may inspect/pan independently without changing combat

#### Scenario: Woodland styling
- **WHEN** the arena is shown in deployment or combat
- **THEN** varied grounded surfaces and restrained exterior vegetation/rocks make it belong to the settlement's woodland world
- **AND** active routes, figures, cues and owned deployment cells remain readable and selectable

#### Scenario: Attractive empty starting village
- **WHEN** a match starts with all nine plots empty
- **THEN** the player sees a coherent authored woodland landscape with the home, defender, continuous river/bridge, wooded terraces, rocks and village props continuing beyond the visible world edges
- **AND** five initially usable plots, four locked expansion plots and the battle approach remain distinguishable from scenery

#### Scenario: Watch a redistributed wave
- **WHEN** a Keep falls during shared combat
- **THEN** clients show its fallen state and nonteleporting enemy preference redirection from current physical poses
- **AND** surviving owned squads continue the current wave

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

The settlement SHALL retain its lower angled orthographic view and fitted nine-plot overview. Arena default/reset framing SHALL derive from the entire connected playable board and physical objectives, not one city or decorative perimeter extent. At 1100x820 and 1280x720, the complete playable arena SHALL fit above the HUD; larger/portrait changes SHALL preserve bounded navigation and usable controls. Local close zoom/pan SHALL permit inspecting engagements while reset restores complete composition. Tall dressing SHALL stay outside critical sightlines. Health/deployment cues and contextual owner/status inspection SHALL remain readable at overview and close views.

#### Scenario: Complete arena overview
- **WHEN** a client enters or resets a four-player battle at a supported size
- **THEN** all sectors and physical objectives fit in the usable world area and the HUD does not conceal required controls

#### Scenario: Inspect a fully built city
- **WHEN** a city has buildings in all nine plots, including level-two buildings, at either supported verification size and the camera is at its default or reset overview
- **THEN** each building can be selected in the world and the battle approach remains visible above the bottom panel
- **AND** resources, contextual actions, and match controls remain readable and reachable

#### Scenario: Observe four-player combat
- **WHEN** a four-player match enters combat or another owner is inspected
- **THEN** the one complete arena remains the battle view with all-owner formations/objectives visible
- **AND** scenery does not conceal fighting or alter authoritative destinations

### Requirement: Authoritative animated combat presentation

Members SHALL animate from their squad's authoritative source, committed transition and shared attack cycle with stable individual formation offsets; bosses SHALL use one larger single-member model. Member cosmetics SHALL never route independently or mutate logical positions. Locomotion SHALL sample the declared committed segment and stop while blocked. Adjacent melee SHALL show imported sword/axe animation, restrained facing/local strike accent and readable attacker-to-target windup/landed feedback; opaque beams, stretched weapons and distant unlinked swings SHALL not substitute. Shots SHALL originate from actual member/weapon sockets and land only on authoritative impact evidence; launch prediction SHALL use only already committed target motion. Individual HP/status/formation and seeded primary observations SHALL remain distinct. A realized primary's death SHALL not visually retarget recovery. Simultaneous actions SHALL use one shared playback clock independent of frame rate.

Casualties SHALL immediately stop living presentation and retain frozen physical position/facing while their imported death clip advances to authoritative expiry. Their logical claims SHALL already be released according to the squad death contract; visual overlap with a subsequently legal mover SHALL not create a numerical blocker or displacement. Reconnect/history gaps SHALL restore current elapsed bodies/actions without historical effects. Pause/transport loss SHALL freeze battle playback while local camera projection remains usable. Missing required rigs/clips SHALL be explicit failures, not dummy replacements. Session replacement SHALL release all old nodes/effects.

#### Scenario: Coordinated group
- **WHEN** a three-member squad commits a move and attack
- **THEN** all three animate the same actor cycle at their own formation/socket positions with independently received health

#### Scenario: Frozen body, advancing clip
- **WHEN** the final member dies during movement
- **THEN** its body stays at its frozen physical pose while its imported death clip advances until expiry
- **AND** a legal mover can use the released cell without cosmetic collision changing the authority

#### Scenario: Landed and missed shots
- **WHEN** a member's committed group attack is observed repeatedly or its target group dies before impact
- **THEN** each action cue is consumed once and a miss produces no landed victim flash or damage

#### Scenario: Stop and swing at contact
- **WHEN** moving Swordsmen reach their authoritative positions in adjacent combat hexes
- **THEN** they face their targets, stop locomotion and play a rigged sword attack with explicit directional intent, a local strike accent and target impact aligned with the authoritative hit
- **AND** health changes follow the authority rather than an animation callback

#### Scenario: One shot across repeated snapshots
- **WHEN** a Crossbowman attack appears in multiple snapshots
- **THEN** its shooting animation and cosmetic shot are triggered once for that action identity and later snapshots update their progress

#### Scenario: Casualty finishes dying
- **WHEN** a visible member dies and leaves the living population
- **THEN** its frozen physical pose plays the remaining death clip without active claims or targeting
- **AND** expiry/session replacement frees the cosmetic body

#### Scenario: Pause freezes a posed battle
- **WHEN** combat is paused during movement, an attack, a hit, a projectile or a death sequence
- **THEN** positions, facing, rig blends and effects freeze while connection and pause/resume controls remain responsive
- **AND** resume continues their remaining progress without wall-clock catch-up

#### Scenario: Reconnect to a current battle
- **WHEN** a client receives its complete current state after reconnecting
- **THEN** it reconstructs living units and unexpired dying bodies at their current hex positions and action/death progress without replaying historical hits, shots or death sounds, restarting deaths or reviving dead units
- **AND** events from the former connection or another match cannot create duplicate visuals

#### Scenario: Shared hex has separate visible positions
- **WHEN** several living members of one homogeneous squad share its hex
- **THEN** their stable formation slots provide distinct local poses while the actor has one exclusive claim
- **AND** the committed group movement does not displace neighbors numerically

#### Scenario: Retained death still needs visual clearance
- **WHEN** an unexpired casualty remains on a logically released cell
- **THEN** its frozen physical pose remains coherent while a subsequently legal mover may cross it visually
- **AND** the overlap creates no blocker, position correction or damage

#### Scenario: Frame rate does not order combat
- **WHEN** compatible attacks and moves begin on the same authority tick
- **THEN** clients sample each action from that tick irrespective of local frame rate
- **AND** initiative does not add presentation-only delays that change the declared action timing

#### Scenario: Overview retains readable unit roles
- **WHEN** the player views a shared battle at overview
- **THEN** the [health-overlay contract](../game-feedback/spec.md#requirement-authoritative-overhead-unit-health-bars) governs compact bars and contextual member details
- **AND** windup intent remains attributable without opaque crossing beams

#### Scenario: Far-side melee target is readable
- **WHEN** an anchored melee occupant attacks an eligible far-side occupant in an adjacent shared hex
- **THEN** directional intent identifies the actual locked target and its landed impact at the declared tick
- **AND** the miniature remains at its formation anchor and the display reads as an abstract tabletop attack

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

WASD/arrow navigation SHALL remain equivalent, frame-rate independent and bounded without changing camera orientation/height or speeding diagonal motion. Settlement navigation SHALL use its existing city bounds; arena navigation SHALL use content-derived shared-arena bounds and permit crossing sectors rather than inherit the old city travel clamp. Releasing a key SHALL stop movement.

#### Scenario: Cross-sector camera travel
- **WHEN** a client zooms and pans across a four-player arena
- **THEN** it can inspect each sector within the safe boundary and stopping input stops camera motion

#### Scenario: Hold and release a direction
- **WHEN** the player holds D or right arrow during world interaction and then releases it
- **THEN** the view pans right while held and stops on release
- **AND** the camera does not rotate or change height

#### Scenario: Move diagonally and reach the edge
- **WHEN** the player holds two perpendicular pan directions and continues toward the travel boundary
- **THEN** diagonal speed does not exceed cardinal speed and the view stops at the boundary

### Requirement: Local camera input and lifecycle

Navigation SHALL remain local and issue no gameplay commands. Modal dialogs, consuming focused controls, text entry and lost focus SHALL take priority and clear held gestures/keys without stale resumption. Paused, disconnected and outcome arena views SHALL permit navigation with frozen combat state. Health/status/inspection projections SHALL follow camera/layout changes for all currently visible owners.

#### Scenario: Inspect during pause
- **WHEN** a client pans over another sector during pause
- **THEN** camera and labels reproject while member health, actions and physical battle state remain frozen

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

A nonrepeating eligible Space press SHALL restore the fitted overview of the currently active settlement or complete arena, with compact help and no redundant Reset button. UI Space handling SHALL not also reset the world. Fresh match/view transition SHALL initialize fitting and clear navigation; resize/HUD changes SHALL recompute content-derived fitting while preserving relative zoom/pan within current bounds. Reconnect within the same match/view SHALL retain local adjustments; they SHALL not persist across launches.

#### Scenario: Reset shared battle
- **WHEN** Space is pressed after close inspection of a foreign sector
- **THEN** the complete arena overview returns without changing selection, resources or combat

#### Scenario: Reset a close view
- **WHEN** the player presses Space during eligible world interaction after zooming and panning
- **THEN** the observed settlement or shared arena's fitted overview is restored with all plots and the battle approach above the HUD
- **AND** world selection and resources are unchanged

#### Scenario: Switch city or match
- **WHEN** the observed settlement or shared arena changes or a fresh match replaces the current match
- **THEN** the new city's overview is shown without inheriting the previous city's pan or zoom

#### Scenario: Resize an adjusted view
- **WHEN** the window or HUD size changes while the player has adjusted the view
- **THEN** the relative zoom and active-view-relative pan are preserved within limits against the recalculated base framing
- **AND** Space still restores a fully fitted overview

#### Scenario: Reconnect within the same view
- **WHEN** the client reconnects to the same match and observed settlement or shared arena without replacing its tabletop
- **THEN** its camera adjustments are retained and rebuilt world targets remain selectable

#### Scenario: Space belongs to a focused control or modal
- **WHEN** the player presses Space while a consuming UI control is focused or a modal is open
- **THEN** the UI handles the key and the camera remains unchanged
- **AND** holding Space does not repeatedly reset or trigger a gameplay action

### Requirement: World interaction after camera navigation
In the shared arena, member/structure inspection and eligible owned deployment picking SHALL use the active arena geometry and retain the same local input guards. Hover and selection SHALL resolve the currently visible plot or building to its existing slot after camera navigation. Navigation SHALL NOT spend resources, change world selection or enable editing a foreign city. Screen-space overlays SHALL remain aligned with their world anchors and SHALL NOT intercept world input. Source and exported graphical clients SHALL provide the same camera controls; headless roles SHALL NOT instantiate camera presentation.

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

Settlement coverage SHALL retain compact authored countryside and its opaque boundary at all permitted navigation/viewport/HUD positions. Arena coverage SHALL provide coherent woodland ground and an opaque dressed diorama edge/backstop over every permitted shared camera footprint, without exposing bare cut edges or the flat environment background. Playable content/objectives SHALL determine fitting; outside scenery SHALL not inflate the default overview or travel. Ground/dressing SHALL reuse imported geometry/materials with bounded instancing and shall not rebuild static content on ordinary HP/action snapshots.

#### Scenario: Extreme arena pan
- **WHEN** the camera reaches a permitted pan/zoom limit or the window changes size
- **THEN** visible ground and the opaque perimeter still cover the world footprint without revealing unfinished exterior space

#### Scenario: Fill the overview after resizing
- **WHEN** the client displays or resizes the default city overview at either supported verification size
- **THEN** compact countryside and its opaque cloud boundary cover the visible world area without exposing cut terrain edges
- **AND** the plots and battle approach remain readable and selectable above the HUD

#### Scenario: Cover camera travel limits
- **WHEN** the player navigates to a permitted zoom or pan limit
- **THEN** coherently joined countryside and the cloud boundary retain coverage, and the camera stays within the boundary
- **AND** Reset view restores the complete playable composition

### Requirement: Menu shares the starting countryside
The start screen and multiplayer entry backdrop SHALL reuse the starting village's terrain composition, elevations, home, defender, nine empty plots with the starting five-purchased/four-locked appearance, river, bridge and static decoration assets, with consistent asset scale, materials and lighting. Menu framing SHALL fill the background around its readable controls using viewport-derived countryside coverage, rather than the compact battlefield cloud boundary. Menu scenery SHALL remain non-interactive and SHALL NOT create a gameplay session, connect to an authority or expose match actions. Returning to the menu SHALL restore the empty starting countryside without retaining a previous match's purchased expansions, constructed buildings or units. Source and exported graphical clients SHALL provide this same presentation; headless roles SHALL remain free of visual instantiation.

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

Squad/member inspection SHALL expose each individual's actual equipment level, service experience, current/max HP, X/Y/structural damage, status and participating/reserve/storage state. Normal population and one-member boss identity SHALL replace obsolete size-two/size-six combat capacity descriptions. Mixed-level veterans/recruits SHALL remain distinguishable in details without individual paid-upgrade controls. World health overlays SHALL follow [game-feedback](../game-feedback/spec.md#requirement-authoritative-overhead-unit-health-bars). Actual latest wave material/research rewards SHALL identify wave/owner and restore without another payment on reconnect or repeated updates, including victory. Fresh matches SHALL clear reward state.

#### Scenario: Partial veteran group
- **WHEN** a wounded level-one veteran and paid level-three recruit share a squad
- **THEN** details show their separate levels/HP/profiles/service rather than the trainer's level or a shared HP pool

#### Scenario: Observe a boss
- **WHEN** a single-member boss squad is visible
- **THEN** inspection identifies its owner/faction, boss identity, individual level and effective HP/damage
- **AND** its larger model does not imply size-six numerical capacity

#### Scenario: Restore the reward summary
- **WHEN** a player reconnects after wave ten clears
- **THEN** the current balances and latest summary show that city's actual wave-ten reward without playing another payment effect

#### Scenario: View mixed-level survivors
- **WHEN** level-one veterans and level-three recruits belong to the observed army
- **THEN** army details distinguish their levels and use their own profiles rather than displaying all soldiers at the current building level

### Requirement: Readable resource progression and contextual economy controls
City construction choices SHALL be grouped into production, recruitment and advanced/defensive buildings with stable ordering; army-plot actions SHALL follow the `docs/gameplay.md#purchased-army-homes-and-town-hall-rotation` (gameplay army/tower controls). Affordable eligible choices SHALL be visually emphasized; unavailable choices SHALL remain visible and greyed, with explicit Need N more resource explanations and producer names or the applicable ownership, readiness, pause or phase reason. Explanation SHALL remain accessible even when the purchase button is disabled and SHALL NOT rely only on color. Buildings SHALL NOT be hidden until a resource is discovered. Selected locked plots SHALL offer only their purchase quote and inspection; purchased empty plots SHALL offer construction appropriate to their city/army role; squad-occupied army plots SHALL NOT offer construction even with unused member places. Selecting building-occupied plots SHALL expose their normal actions and an explicit Sell action with the exact refund and retained-army/research explanation. Markets SHALL expose fixed sale bundles, available stock and exact gold proceeds; food-sale previews SHALL update the projected upkeep balance. Producer choices SHALL show their complete text cost alongside their current per-turn output. Producer upgrades SHALL show current to next output with a consistent unit label. The explicit gold-paid Lumbermill recovery choice SHALL be visible only at zero wood, show its complete quoted gold price and follow normal eligibility; normal builds SHALL never silently substitute currency. Affordability, costs, refunds, rates, income and upkeep SHALL derive from authoritative state/catalogs and refresh after accepted actions, production, city switches and reconnects without guessing that a request succeeded. Layout SHALL keep all six resource balances and essential actions readable at 1100x820 and 1280x720 while preserving world selection and the nine-plot overview. Economically unavailable controls SHALL never become enabled solely by a presentation calculation.

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

The ordered home overview and explicit purchase control SHALL expose the next authoritative gold purchase quote, occupied/available squad member places and stable first-fit destination. A selected Town hall SHALL display occupied/available storage spots and independently quoted capacity/healing upgrades, current percentage and paid-recovery eligibility. Selection SHALL never spend or transfer on its own; full destinations and occupied-hall sale SHALL be explained.

### Requirement: Visible upkeep forecast and reserve participation
The observed city SHALL have a very small text-only Upkeep table directly below its resource table, without opening City details. During building and preparation, including while ready, its first row SHALL show Next battle food demand. When every soldier can be fed, the second row SHALL show Food after payment using authoritative forecast availability minus projected payment. Forecast availability SHALL follow `docs/gameplay.md#deterministic-hex-combat` (gameplay's upkeep projection). On shortage the second row SHALL instead show the exact unfunded count with a textual cue rather than color alone: soldiers that will sit out for a field-only army, or Unfunded soldiers when storage is present, without suggesting funded stored units fight. Empty armies SHALL show zero demand. Current food, projected payment and which soldiers would participate or sit out under the authoritative field-first allocation defined in [army-roster](../army-roster/spec.md) SHALL remain accessible through inspection. Pause and transport loss SHALL identify paused or stale synchronized values without suggesting another payment. Recruitment SHALL show equipment costs separately from per-battle food upkeep. A shortage SHALL be explained before Ready without disabling readiness solely for food. Accepted recruitment or food sales SHALL refresh the forecast. During combat the view SHALL distinguish participating soldiers, funded field participants and unfed reserves, showing actual paid upkeep for that wave. The compact table SHALL label that receipt Paid this battle with its wave and reserve count; outcome views SHALL identify Last battle, and fresh sessions SHALL clear old receipts. Reserves SHALL remain in army details with retained level and health but SHALL NOT be rendered as deployed combatants, walking capacity queues or casualties. Reconnect SHALL restore the current status without replaying a feeding, death or recruitment effect.

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
- **WHEN** an observed city has authoritative forecast availability of 15 food and six living Swordsmen before battle
- **THEN** the visible Upkeep table shows Next battle 6 food and Food after payment 9
- **AND** a shortage replaces the second row with the exact sit-out count and does not disable Ready solely because of food

#### Scenario: Combat shows an actual payment
- **WHEN** preparation starts combat and the owner changes cities or reconnects
- **THEN** the table shows the observed city's actual wave-tagged paid food and reserve count
- **AND** the display grants no food, charges no upkeep and does not imply a new payment

Demand SHALL include stored units and identify field/storage food separately. Funded stored units SHALL not be described or rendered as participating combatants. The roster SHALL explain that recovery uses food funded in the last completed battle and occurs only at actual production.

### Requirement: Mouse drag tabletop panning
Graphical clients SHALL support left-click-and-drag camera panning starting within the eligible visible world area. The view SHALL follow the dragged ground plane while preserving the angled orthographic projection, zoom and active settlement/shared-arena content-derived travel bounds. Movement below a small consistent screen-space threshold SHALL remain a normal click selecting the same plot/building; once the threshold is exceeded the gesture SHALL pan without selecting a plot on press or release, submitting commands or spending resources. Dragging SHALL work over plots, buildings and scenery. HUD surfaces including the resource table, Settings and other modal dialogs SHALL block drag initiation. A modal opening, window focus loss, city/session replacement or button release SHALL end the gesture without stale movement resuming. Drag navigation SHALL remain available during pause, transport loss and outcome when a city view exists. Screen-space bars and markers SHALL follow the camera and remain input-transparent. Source and exported graphical clients SHALL behave alike.

#### Scenario: Drag across a building
- **WHEN** the player left-drags from a visible building beyond the drag threshold and releases over another plot
- **THEN** the camera pans within its bounds and neither plot selection nor authoritative state changes
- **AND** a subsequent short click still selects its intended plot normally

#### Scenario: Interrupt a drag
- **WHEN** a modal opens or the window loses focus during a drag
- **THEN** camera movement stops immediately and reopening game input does not resume the old gesture
- **AND** dragging from a HUD surface or modal never moves the world camera

### Requirement: Compact construction and turn progression
Construction choices SHALL occupy a three-column, three-row area with the existing Production, Army, Defense and Trade category selection retained above it. Smaller groups SHALL NOT create fake actionable buildings. Every choice SHALL retain readable text costs, stable ordering, affordability and accessible unavailable-action explanations. The compact vertical stage sequence and counters SHALL use [gameplay's preparation labels](../../../docs/gameplay.md#preparation-and-battle-controls), highlighting the current authoritative stage with a non-color cue. Production SHALL remain a settlement event, not an invented persistent phase. Small wave/total counters and boss identification SHALL remain alongside the sequence. Ready/Unready and Pause/Resume SHALL sit side by side below it with existing authoritative eligibility. Lobby, victory and defeat SHALL have clear state-specific presentation rather than highlighting an active stage incorrectly.

#### Scenario: Resolve final preparation
- **WHEN** authoritative readiness resolves final preparation
- **THEN** the stage highlight and counters follow the accepted snapshot and the [gameplay settlement boundary](../../../docs/gameplay.md#preparation-and-battle-controls)
- **AND** no extra editable stage is implied or introduced

#### Scenario: Browse construction
- **WHEN** a player selects an owned purchased empty plot and changes category
- **THEN** choices occupy at most the three-by-three area beneath the category selection with complete text costs and accessible disabled reasons
- **AND** selecting a category does not build anything or change the selected plot

### Requirement: Vertical resource table
Graphical match views SHALL show a narrow top-right three-column table with Resource, Stock and phase-aware income columns defined in [gameplay controls](../../../docs/gameplay.md#preparation-and-battle-controls), ordered Gold, Food, Wood, Stone, Metal and Cloth. All six rows SHALL show exact authoritative balances and projected production including zero, for the currently observed city, and SHALL refresh after synchronization, production, accepted spending, rewards, city switches and reconnect. Numeric stock and income columns SHALL be right-aligned; positive income SHALL use + and zero SHALL use 0. Income headings and Ready explanations SHALL follow that gameplay owner rather than imply an income-free preparation. Fallen and terminal cities SHALL show zero future income with an inactive cue; an unrepresentable custom projection SHALL show an explicit unavailable value instead of a guessed amount. Pause and transport loss SHALL retain last synchronized values with paused/stale context. Income SHALL refresh after producer construction, upgrades or sales even without a production payment. Resource icon presentation SHALL follow [themed-ui](../themed-ui/spec.md#requirement-text-first-native-controls-with-original-scan-aids); the table SHALL NOT introduce additional resource types. Resource totals and purchased-land summaries SHALL NOT be repeated in the bottom HUD. Contextual costs, refunds, trade quotes and food upkeep SHALL remain accessible. Pointer input over the table SHALL NOT select or zoom the world behind it.

#### Scenario: Inspect and reconnect
- **WHEN** a player switches to a foreign city or reconnects to the currently observed city
- **THEN** every resource row immediately uses that city's current synchronized balance
- **AND** foreign-city spending stays unavailable and no historical earning animation is replayed

#### Scenario: Income agrees with current capacity
- **WHEN** a living default city owns a level-one Lumbermill and level-two Metal Mine
- **THEN** its table shows +2 gold, +1 wood, +8 metal and 0 income for other resources
- **AND** selling a producer updates income after authoritative acceptance without granting production

#### Scenario: Preparation communicates settlement
- **WHEN** a city reaches final preparation
- **THEN** income and Ready wording follow the [gameplay controls](../../../docs/gameplay.md#preparation-and-battle-controls)
- **AND** combat uses future-production wording while terminal and fallen cities show no future income

#### Scenario: The combined stack fits supported layouts
- **WHEN** the resource and Upkeep tables and a unit inspector are visible at 1100x820 or 1280x720
- **THEN** their essential values fit inside the viewport, the inspector fits below the combined stack and above the approximately 180px HUD, and all nine plots and the battle approach remain visible and selectable at overview
- **AND** pointer input inside either table blocks world selection, zoom and drag initiation

### Requirement: Clean plot and home markers
Unpurchased plots SHALL display a compact bundled gold/buy marker instead of Locked land text. Selecting a locked plot SHALL expose its exact purchase quote and existing explicit purchase action; hovering or selecting the marker SHALL NOT spend resources. The marker SHALL disappear after authoritative purchase. Home and arena health overlays SHALL follow [game-feedback's health and lifetime contract](../game-feedback/spec.md#requirement-authoritative-overhead-unit-health-bars). City ownership SHALL remain identifiable through the compact city selector.

#### Scenario: Purchase marked land
- **WHEN** a player selects a gold-marked locked plot and explicitly confirms its affordable purchase action
- **THEN** authoritative acceptance removes the marker from that same plot and exposes construction choices
- **AND** the selected slot remains the same and the top-right balance updates once

#### Scenario: Show wounded city health
- **WHEN** a synchronized city has half its authoritative maximum health
- **THEN** the home bar shows half fill without percentage or player/heart/raw-health text
- **AND** the bar remains correctly positioned and does not consume plot input after camera movement

### Requirement: Clickable unit inspection

Every visible deployed living member and boss across the shared arena SHALL be short-click inspectable with stable nearest-visible-hit selection and no gameplay command. The existing nonmodal popup SHALL show the actual model/name/role/owner/faction/boss identity, level, current/max HP and resolved damage channels, structure damage and range/timing where relevant; it SHALL not describe retired body-size capacity. It SHALL refresh from accepted state, retain frozen values during pause/loss and fit below resource tables/above HUD. Input inside the popup SHALL block underlying world input; outside click SHALL dismiss and deliver the ordinary target action once. Another member click SHALL replace content directly. Dragging SHALL dismiss/avoid inspection. Death/undeployed/reserve members SHALL not be world targets; rosters SHALL permit their appropriate inspection. Session replacement, removal/death or invalid selection SHALL clear the popup, while same-match surviving reconnect SHALL refresh it. Owned army actions SHALL use phase/ownership/capacity guards; foreign members SHALL remain read-only.

#### Scenario: Foreign member
- **WHEN** a player clicks another owner's member in the arena
- **THEN** its current details are shown without enabling deployment, regrouping or spending for that owner

#### Scenario: Corpse releases a cell
- **WHEN** a cosmetic corpse overlaps a later legally occupying member
- **THEN** the corpse is not a world-click target and the living member remains selectable

#### Scenario: Inspect a wounded ranked unit
- **WHEN** a visible member with current525/max1050HP in hundredths is short-clicked
- **THEN** the popup shows its actual identity/profile and health5.25/10.5 at half fill
- **AND** squad context and individual level remain available without submitting a command

#### Scenario: Switch and dismiss inspection
- **WHEN** the player clicks another living unit and then clicks a plot or HUD control outside the popup
- **THEN** the first click replaces the popup with that unit's details and the outside click closes it
- **AND** the plot/control receives its normal eligible action once without click-through behind the inspector

#### Scenario: Drag over units
- **WHEN** a player drags from a unit past the camera gesture threshold
- **THEN** the camera pans, the inspector stays closed, and neither unit nor plot selection changes

#### Scenario: Refresh and clear inspected state
- **WHEN** the inspected unit receives synchronized damage, is paused or disconnected, and later dies or leaves the observed settlement or shared arena
- **THEN** the popup uses the new authoritative health, freezes values during pause/loss, and closes when selection becomes invalid
- **AND** changing cities or replacing the match does not retain the old popup or preview

### Requirement: Readable personal research tree
The Worldtree research panel SHALL use the native living component and the [fixed visual-to-authority mapping](../../../docs/worldtree.md#fixed-visual--authoritative-mapping). It SHALL show the observed city's whole points, partial production progress, tower contribution, purchased nodes, prerequisites, eligible unit roles or producers, effects and exact costs. Exclusive siblings and descendants SHALL remain visible with a readable permanent-lock explanation; the purchase action SHALL explicitly name the sibling branch it locks without requiring a second confirmation dialog. Unaffordable, prerequisite-locked, foreign-city, ready, paused, disconnected, combat and eliminated purchases SHALL be disabled with non-color-only reasons. Viewing research SHALL not pause or advance the shared match. The panel SHALL block underlying world input, fit supported window sizes using scrolling where needed, dismiss cleanly, refresh from acknowledged authoritative state and show no successful purchase effect before acceptance. Research Towers SHALL be identified by name and distinguishable two-level structures using the bundled palette; contextual output SHALL explain one/two points per full three-production cycle rather than per turn.

#### Scenario: First purchase without a research building
- **WHEN** a towerless eligible owner opens Worldtree with three points and no selected plot
- **THEN** its affordable foundation can be purchased through actual controls and updates after acknowledgment
- **AND** the same tree is read-only when observing another city

#### Scenario: Permanent branch lock is visible
- **WHEN** an owner inspects Fire and then successfully purchases it
- **THEN** its purchase description identifies the Frost lock and Frost/descendants remain visible with the permanent reason afterward
- **AND** rejected or retried requests produce no duplicate success cue

### Requirement: Technology and status inspection
Unit inspection and army details SHALL describe actual acquired capabilities and current burn, poison and chill with authoritative strengths, poison stack count, remaining simulation duration and effective committed action timings where relevant. Status presentation SHALL follow [gameplay's inspection guidance](../../../docs/gameplay.md#personal-research-and-temporary-statuses), without owning damage scheduling or altering rig materials permanently. Poisoned queued units SHALL remain inspectable through army/enemy state without adding a world body. Death, effect expiry, city switches and fresh sessions SHALL clear or reconstruct indicator state; reconnect/history gaps SHALL reconstruct currently active effects without old sounds or application flashes. Individual inspection details, health overlays, rigged actions and single-location squad movement SHALL remain readable. Chill descriptions SHALL promise slower future actions and SHALL NOT imply freeze, stun or interruption.

#### Scenario: Inspect poison and chilled timing
- **WHEN** a visible unit has two poison stacks and a committed chilled attack
- **THEN** its inspection shows the actual stacks and fixed action timing from authoritative state
- **AND** pausing freezes remaining effect time and reconnect restores it without replaying applications

### Requirement: Native squad preparation controls

The normal native squad panel SHALL offer per-owner roster/member inspection and eligible owned new-squad recruitment, selected compatible paid reinforcement, member regrouping, Town Hall storage/return and retirement. A Deployment control SHALL expose owned purchased tower-free homes using the [gameplay deployment markers](../../../docs/gameplay.md#purchased-army-homes-and-town-hall-rotation). Selecting an owned squad and an available home SHALL submit the ordinary authority request; foreign owners SHALL be inspectable only. Accepted snapshots SHALL determine roster/placement and preserve ordinary native focus; rejected/pending actions SHALL not invent members, spend optimistically or move a squad locally. Ready/pause/combat/disconnection/elimination SHALL disable editing with accessible reasons.

#### Scenario: Delayed reinforcement
- **WHEN** an owner requests an affordable reinforcement before acknowledgment arrives
- **THEN** the roster retains its old membership until accepted state arrives and a rejection creates no optimistic recruit

#### Scenario: Deployment ownership
- **WHEN** a player selects a squad and an owned available home
- **THEN** accepted deployment updates its complete claim, while foreign/locked/tower-occupied destinations refuse without movement or spending

### Requirement: Restrained arena dressing and lifecycle

Arena dressing SHALL use already bundled pinned woodland exports and original native markers without requiring new source assets or editing imported material/mesh bytes. Playable ground SHALL remain planar/coherently grounded at the existing numerical projection. Tall trees/rocks SHALL stay outside active cells/objectives/entry and deployment sightlines; smaller accents SHALL be restrained. Deployment grid/ownership cues SHALL be prominent during placement and subdued during combat. Structure wounds/destruction/inactivity SHALL be readable without obscuring units. Dressing SHALL be deterministic for equivalent arena geometry, independent of combat RNG, bounded across one-to-four-player layouts, reused across revisions and released on fresh-match/view teardown. Headless roles SHALL instantiate none of it.

#### Scenario: Repeated combat updates
- **WHEN** ordinary HP/action snapshots update a four-player battle
- **THEN** static ground/dressing is reused with bounded resources and no changes to numerical cells/claims

#### Scenario: New match
- **WHEN** the client replaces a four-player battle with a solo match
- **THEN** old dressing/labels/models release and the solo composition uses its own geometry
