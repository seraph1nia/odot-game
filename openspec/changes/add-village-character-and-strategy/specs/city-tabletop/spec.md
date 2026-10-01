# Spec Delta

## MODIFIED Requirements

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

### Requirement: Medieval landscape and visible battles
Graphical clients SHALL display each city as a bounded medieval countryside landscape with nine hexagonal building plots arranged in three staggered rows. The landscape SHALL include surrounding grass, a river along one side, wooded hills, rocks, and small village props, including when all building plots are empty. A raised rear terrace, lower riverbank and wooded slopes SHALL create visible elevation differences; edge mountains, a bridge and appropriate building props SHALL enrich the scenery without hiding plots or battles. Terrain elevation SHALL remain cosmetic and unit rendering SHALL meet its visible ground surface without changing authoritative contact or targeting. The battlefield SHALL be a readable grassy approach with no painted road markings or modern road slab. Decorations and terrain SHALL NOT occupy building slots or change movement, damage, economy, or city capacity. Players SHALL be able to inspect every player's city and battle, identify their own city, and distinguish soldiers from enemies. The visual representation SHALL follow authoritative state, including enemy transfers, casualties, and eliminated cities. The game SHALL NOT require manual unit control, movable terrain, or a board-folding effect.

#### Scenario: Attractive empty starting village
- **WHEN** a match starts with all nine plots empty
- **THEN** the player sees a coherent grass landscape with the home, defender, river, wooded hills, rocks, and village props
- **AND** all nine buildable plots and the battle approach remain distinguishable from scenery

#### Scenario: Watch a redistributed wave
- **WHEN** another city falls during a wave
- **THEN** clients show that city's elimination and the transferred enemies fighting at their authoritative destinations
- **AND** the player can inspect the surviving cities while combat continues

#### Scenario: Upgraded buildings are recognizable
- **WHEN** a building is upgraded to level two
- **THEN** its structure or surrounding props visibly distinguish the upgrade without relying only on uniform scaling
- **AND** clicking its visible roof or raised plot still selects the same stable slot
