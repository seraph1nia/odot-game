# Spec Delta

## MODIFIED Requirements

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

## ADDED Requirements

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
