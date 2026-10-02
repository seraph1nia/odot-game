# Spec Delta

## MODIFIED Requirements

### Requirement: City interaction and match feedback
Clients SHALL expose construction on empty purchased plots, locked-plot purchases, building upgrades and sales, explicit building-specific archetype recruitment, Blacksmith research, Market sales, ready/unready, and pause/resume through a compact control panel anchored to the bottom of the game window. The interface SHALL show complete construction/upgrade/recruitment/research costs, building refund and Market quotes, gold, food, wood, stone, metal and cloth in a separate top-right resource table; next expansion cost only with a selected locked plot; current army upkeep demand, food forecast, class research ranks and soldier details through compact contextual inspection; city health at the home through a percentage health bar; building level in occupied-plot details; phase including final preparation through a small vertical stage indicator with compact turn and wave counters and boss indication; readiness through Ready/Unready; and exceptional connection status and outcome, distinguishing all-cities-fallen defeat from battle-stalled defeat. A stalled result SHALL explain that the battle could not finish, rather than claim every city fell. City inspection controls SHALL identify the player's own city and the observed city using a very small `< [YOU] >` selector in the existing city-navigation location, with text left/right buttons and a concise player identifier when observing another city. Cycling SHALL visit roster cities in stable player order, wrap at either end, clear plot selection and fit the new city overview; arrows SHALL be disabled when only one city is available. Lobby roster/host/readiness information SHALL remain available in the lobby without restoring the verbose player bar during gameplay. Construction, plot purchase, building sale, upgrade, research, Market trade and recruitment actions SHALL be contextual to the directly selected world plot or building; without a selection, the panel SHALL NOT offer an actionable default plot. Routine select-a-plot and Open plot X / choose a building prompts SHALL be omitted; meaningful disabled-action reasons and occupied-building details SHALL remain accessible. The interface SHALL NOT include a duplicate numbered grid for selecting building slots. A cooperative lobby SHALL expose the current roster, identify the host where applicable, and expose the start action only to eligible players. Hosted lobbies SHALL offer Invite friends; solo play SHALL enter the first building turn directly. Lobby, match, outcome, and disconnected views SHALL offer Return to menu through Settings and a cancelable confirmation, with no Return to menu control in the bottom HUD. The bottom HUD SHALL omit ODOT, P1/Your City headings, City/Army summary rows, purchased land counts, resource summary rows, the verbose P1/You/Here bar and routine Match started text. Server errors, actionable feedback, pause/disconnection and outcomes SHALL remain visible when relevant. Connection and host-loss feedback SHALL reflect the selected session mode. Unavailable actions SHALL be visibly disabled or explained, and server rejection SHALL be surfaced. A disconnected guest SHALL offer reconnect without discarding its private resume information while the original authority may still be available, and input SHALL remain disabled until resynchronization completes. An ended hosted session SHALL offer return to the start screen without suggesting authority migration.

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
- **THEN** the panel displays the battle-stalled reason and Settings offers confirmed return to menu
- **AND** it does not describe surviving cities as fallen

### Requirement: Requested free asset palette
The game SHALL use the free KayKit Medieval Hexagon Pack as the primary building/environment palette, KayKit Prototype Bits for missing non-character objects and markers, and KayKit Resource Bits for resource visuals such as gold. Grass, river terrain, hills, trees, rocks, and medieval village props SHALL use matching ready-made assets from the free medieval pack. Farm visuals SHALL be represented by an appropriate free medieval building or prop, with a clear farm label. Food scenery SHALL use a recognizable medieval prop such as a sack without a resource text label instead of the prototype can. Combat units SHALL use free rigged Adventurers and Skeletons characters with compatible locomotion, melee, ranged shooting, casting, hit and death clips and matching weapons. Berserker SHALL use the free Barbarian and Mage the free Mage; Skeleton Warrior, Minion, Rogue and Mage SHALL supply their enemy counterparts. Required character animations SHALL NOT be replaced by dummy models, scale pulses, or labeled markers. Paid tiers SHALL NOT be required. Gold, food, wood, stone, metal and cloth SHALL be the six economy resources. Stonecutter, Metal Mine, Weaver and Market SHALL have identifiable labeled representations using the bundled free palette; Gold Mine and Metal Mine SHALL be distinguishable. Text resource names in the top-right table and contextual cost/production details SHALL distinguish stone, metal and cloth without adding resource text labels on the board or claiming that decorative assets grant resources. The board SHALL omit Gold / Food / Wood and other stockpile resource text labels while retaining authoritative physical stockpiles. Resource Bits SHALL supply free gold and wood props; free medieval sacks/grain SHALL supply food. Paid food, coin, character or source tiers SHALL NOT be required. The selected assets, required textures/buffers, included license texts, and recorded official source/version information SHALL be available from a clean checkout without runtime downloads.

#### Scenario: Free assets cover the match
- **WHEN** a contributor prepares and runs the graphical game from a clean checkout
- **THEN** buildings, grass plots, river, hills, and decorations use the medieval palette and combat units use rigged characters and their required animations without paid content
- **AND** food uses a recognizable unlabeled medieval prop, physical stockpiles remain visible, and all six resources and new buildings are identifiable through the resource table, contextual details and bundled free assets

#### Scenario: Clean checkout contains animated characters
- **WHEN** a contributor imports source or launches a packed graphical export without downloading assets at runtime
- **THEN** soldiers and enemies load their rigged models, matching weapons, textures and required animation clips
- **AND** the included provenance identifies immutable sources, file hashes and licenses for the selected character subset

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
- **THEN** input-transparent health bars and Roman level numerals remain readable above the models without overhead unit names or role codes, and clicking a unit exposes its role/name in inspection
- **AND** the directional melee guide still identifies its locked target during windup

#### Scenario: Far-side melee target is readable
- **WHEN** an anchored melee occupant attacks an eligible far-side occupant in an adjacent shared hex
- **THEN** the directional strike cue identifies the actual locked target and its landed impact at the declared tick
- **AND** the miniature remains at its footprint anchor and the display reads as an abstract tabletop attack

#### Scenario: Verify neighboring occupied cells before fixing scale
- **WHEN** the initial board/action model reaches its early gameplay gate
- **THEN** a checked-in short source combat check captures actual bundled rigs in two occupied adjacent cells with near/far targets and a simultaneous exchange
- **AND** readable linked attacks, health/role identification and shared positions are inspected before board spacing/footprint layout and broad presentation integration are accepted

### Requirement: Readable unit progression and wave rewards
The graphical match view SHALL expose an observed army's unit levels as Roman numerals above the left part of their health bars, with full names, descriptions, faction, boss identification and level in click inspection and army details rather than overhead name/role codes. Displayed health and damage SHALL use authoritative resolved profiles. Army inspection SHALL show authoritative unit size, with normal units at size two and bosses at size six. Mixed-level soldiers SHALL remain distinguishable through Roman level markers or grouped army details without adding individual upgrade controls. The view SHALL show the latest completed wave's actual gold, food and wood reward for the observed city, including on the final victory screen. A summary SHALL identify its wave and SHALL NOT imply a second payment on repeated snapshots, city switches or reconnect. A fresh match SHALL clear the prior match's reward summary. Unit levels SHALL NOT require new character assets or modify unit size through visual scaling.

#### Scenario: Observe a boss
- **WHEN** the observed city receives a boss allocation or a transferred boss
- **THEN** the overhead bar shows its Roman level and click inspection identifies it as a boss with its level, authoritative current/maximum health, damage per attack and size six

#### Scenario: Restore the reward summary
- **WHEN** a player reconnects after wave ten clears
- **THEN** the current balances and latest summary show that city's actual wave-ten reward without playing another payment effect

#### Scenario: View mixed-level survivors
- **WHEN** level-one veterans and level-three recruits belong to the observed army
- **THEN** army details distinguish their levels and use their own profiles rather than displaying all soldiers at the current building level

## ADDED Requirements

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
Graphical match views SHALL show a narrow top-right two-column table with Resource name and Amount columns, ordered Gold, Food, Wood, Stone, Metal and Cloth. All six rows SHALL show exact authoritative balances including zero, for the currently observed city, and SHALL refresh after synchronization, production, accepted spending, rewards, city switches and reconnect. The table SHALL contain no resource icons and SHALL NOT introduce additional resource types. Resource totals and purchased-land summaries SHALL NOT be repeated in the bottom HUD. Contextual costs, refunds, trade quotes and food upkeep SHALL remain accessible. Pointer input over the table SHALL NOT select or zoom the world behind it.

#### Scenario: Inspect and reconnect
- **WHEN** a player switches to a foreign city or reconnects to the currently observed city
- **THEN** every resource row immediately uses that city's current synchronized balance
- **AND** foreign-city spending stays unavailable and no historical earning animation is replayed

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
