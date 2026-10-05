# Spec Delta

## MODIFIED Requirements

### Requirement: Requested free asset palette
The game SHALL use owner-permitted authored `odot-game-assets` exports as its complete building, character, equipment, prop and environment palette, without runtime downloads or legacy 3D fallbacks. Buildings and scenery SHALL retain the exported materials and use appropriate available substitutes where a one-to-one match does not exist. Farm's food-producing model SHALL be a labeled Bakery, Lumbermill's wood-producing model a labeled Woodcutter hut, Arcanum a labeled Magic academy and CatapultTower a labeled Bombard tower without changing authoritative roles. Gold Mine and Metal Mine, Stonecutter, Weaver, Market and Town hall SHALL have identifiable labeled representations. Friendly sword and ranged soldiers SHALL use Knight and Archer names with compatible authored equipment; enemy labels SHALL describe their actual bone, hooded or horned silhouettes. Required idle, locomotion, attack, hit and death animations SHALL NOT be replaced by dummy models, scale pulses or labeled markers. Gold, food, wood, stone, metal and cloth SHALL remain the six economy resources. Text resource names and contextual details SHALL distinguish resources without adding stockpile labels to the board or claiming decorations grant resources. Bounded physical stockpiles SHALL use distinct authored cargo/provision/wood substitutes, documented honestly rather than claiming unavailable gold bars or sacks. Selected exports and all required embedded materials/textures/buffers/clips, applicable notices and immutable source/hash records SHALL be available from a clean checkout. UI/audio and original code-driven markers/effects SHALL remain outside this 3D migration.

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
Graphical clients SHALL display each city as a coherent fantasy countryside landscape with nine stable hexagonal building plots arranged in three staggered rows, with purchased and locked plots visibly distinct and surrounding authored terrain continuing beyond the visible world edges. The landscape SHALL include a soft meadow palette, a continuous river along one settlement edge, a matching authored bridge, deliberately clustered woodland, rocky outcrops and small village props, including when all building plots are empty. Shallow stepped wooded terraces and a lower riverbank SHALL create cosmetic elevation differences using available authored terrain rather than retaining legacy slopes/mountains or generating replacement meshes. The battlefield SHALL be a readable grassy approach with no painted road markings or modern road slab. Unit rendering SHALL meet its visible ground surface without changing authoritative contact or targeting. Decorations and terrain SHALL NOT occupy building slots, obstruct combat/selection/HUD or change movement topology, damage, economy or city capacity. Players SHALL be able to inspect every player's city and battle, identify their own city, and distinguish soldiers from enemies. The visual representation SHALL follow authoritative state, including enemy transfers, casualties, and eliminated cities. The game SHALL NOT require manual unit control, movable terrain, or a board-folding effect.

#### Scenario: Attractive empty starting village
- **WHEN** a match starts with all nine plots empty
- **THEN** the player sees a coherent authored meadow/forest landscape with the home, defender, continuous river/bridge, wooded terraces, rocks and village props continuing beyond the visible world edges
- **AND** five initially usable plots, four locked expansion plots and the battle approach remain distinguishable from scenery

#### Scenario: Watch a redistributed wave
- **WHEN** another city falls during a wave
- **THEN** clients show that city's elimination and the transferred enemies fighting at their authoritative destinations
- **AND** the player can inspect the surviving cities while combat continues

#### Scenario: Upgraded buildings are recognizable
- **WHEN** a building is upgraded from level one to level two or higher
- **THEN** its structure or surrounding authored props visibly distinguish the upgrade without relying only on uniform scaling
- **AND** clicking its visible roof or raised plot still selects the same stable slot

### Requirement: Continuous countryside view coverage
At 1100x820 and 1280x720 and after supported viewport or HUD changes, all visible world area SHALL be covered by the authored countryside without exposing the terrain patch perimeter, its exterior slab walls or the flat environment background beyond it. Coverage SHALL include every permitted camera zoom and pan position when navigation is available. Extending scenery SHALL NOT enlarge the fitted overview bounds so as to shrink the playable village; at the default/reset overview all nine plots, home, defender and the battle approach SHALL remain visible above the HUD. Authored river edges and terrain transitions SHALL join coherently throughout visible coverage, and tall scenery SHALL remain clear of plot and battle sightlines. Terrain SHALL reuse imported geometry/shared material resources and bounded chunked instancing rather than instantiate a separate full scene per ordinary meadow cell.

#### Scenario: Fill the overview after resizing
- **WHEN** the client displays or resizes the default city overview at either supported verification size
- **THEN** countryside covers the entire visible world area with its outer perimeter offscreen
- **AND** the plots and battle approach remain readable and selectable above the HUD

#### Scenario: Cover camera travel limits
- **WHEN** the player navigates to a permitted zoom or pan limit
- **THEN** the visible world area remains filled with coherently joined countryside
- **AND** Reset view restores the complete playable composition
