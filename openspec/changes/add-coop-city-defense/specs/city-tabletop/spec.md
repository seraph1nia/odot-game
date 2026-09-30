# Spec Delta

## Purpose

Present authoritative cities and battles as a readable miniature 3D tabletop using the requested free KayKit assets while keeping interaction simple.

## ADDED Requirements

### Requirement: Square tabletop and visible battles
Graphical clients SHALL display square 3x3 city boards on a planar tabletop with an angled view and a simple battlefield for each city. Buildings SHALL occupy square logical slots regardless of the asset pack's hexagonal theme. Players SHALL be able to inspect every player's board and battle, identify their own city, and distinguish soldiers from enemies. The visual representation SHALL follow authoritative state, including enemy transfers, casualties, and eliminated cities. The first version SHALL NOT require manual unit control, movable terrain, or a board-folding effect.

#### Scenario: Watch a redistributed wave
- **WHEN** another city falls during a wave
- **THEN** clients show that city's elimination and the transferred enemies fighting at their authoritative destinations
- **AND** the player can inspect the surviving cities while combat continues

### Requirement: City interaction and match feedback
Clients SHALL expose building selection for empty owned slots, building upgrades, explicit barracks recruitment, ready/unready, and pause/resume. The interface SHALL show costs, gold, food, city health, soldier count, building level, phase, turn within the three-turn cycle, wave out of three, readiness, connection status, and outcome. A lobby SHALL expose the current roster and start action. Unavailable actions SHALL be visibly disabled or explained, and server rejection SHALL be surfaced. A disconnected graphical client SHALL offer reconnect without discarding its session, and input SHALL remain disabled until resynchronization completes.

#### Scenario: Recruit through the graphical interface
- **WHEN** a player selects their barracks during an eligible building phase
- **THEN** they can explicitly recruit for the displayed food cost and see the authoritative resource and army changes after acceptance

#### Scenario: Pause and reconnect feedback
- **WHEN** the match is paused and a client reconnects
- **THEN** it displays the paused match, retained city, roster connectivity, and a resume control
- **AND** it does not display frozen gameplay as active progression

### Requirement: Requested free asset palette
The first version SHALL use the free KayKit Medieval Hexagon Pack as the primary building/environment palette, KayKit Prototype Bits for missing objects, temporary units, and markers, and KayKit Resource Bits for resource visuals such as gold. Farm visuals SHALL be represented by an appropriate free medieval building or prop, with a clear farm label. Missing food or character models SHALL use Prototype Bits or simple labeled markers rather than requiring paid tiers or hunting for new packs. Asset choices SHALL NOT introduce additional gameplay currencies. The selected assets, required textures/buffers, included license texts, and recorded official source/version information SHALL be available from a clean checkout without runtime downloads.

#### Scenario: Free assets cover the match
- **WHEN** a contributor prepares and runs the graphical game from a clean checkout
- **THEN** buildings and environment use the medieval palette and placeholders cover missing unit/food visuals without paid content
- **AND** gold and food remain the only economy resources

### Requirement: Headless and exported operation
The authoritative server and automated headless clients SHALL operate without graphics, audio, or visual asset instantiation. Exported graphical clients SHALL include the selected assets and display the tabletop. Dedicated-server exports SHALL continue to run and communicate correctly when visual resources are stripped.

#### Scenario: Headless network match
- **WHEN** separate automated headless clients connect to a dedicated server
- **THEN** they can build, recruit, advance turns, and complete battles through the same game protocol without creating the tabletop view

#### Scenario: Exported graphical client
- **WHEN** the exported desktop client connects to a server
- **THEN** it displays the board, selected KayKit models, and usable match controls without access to the source project
