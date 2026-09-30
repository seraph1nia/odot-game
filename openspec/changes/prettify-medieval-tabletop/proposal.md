# Proposal

## Why

The current village sits on plain square boxes beside a lane with painted road markings, while a tall sidebar repeats the board as a numbered selector. A focused visual pass can make the game feel like a medieval countryside diorama and simplify interaction by using more of the matching free assets already chosen for the project.

## What Changes

- Replace the right sidebar with a compact bottom control panel containing resources, city inspection tabs, contextual construction/upgrade/recruitment actions, match controls, and connection feedback.
- Remove the duplicate numbered 3x3 selector. Select a plot or building directly in the world; use restrained hover/selection feedback and show relevant actions only after selection.
- Lower the orthographic camera to approximately 33–35 degrees above the ground, framing the village and battlefield in the space above the panel.
- Present the nine existing building slots as three staggered rows of hexagonal grass plots, retaining stable slot identities and existing authoritative commands.
- Replace the box platform and marked road with an authored landscape of grass, a clear grassy battle approach, a river along one side, wooded hills, rocks, and small medieval props.
- Vendor a small additional subset of the free KayKit Medieval Hexagon Pack, with its textures/buffers and provenance, rather than creating custom terrain art or adding asset packs.
- Keep scenery static and bounded, reuse visual resources, and verify the four-player scene on the existing GL Compatibility renderer.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `city-tabletop`: Replace the square tabletop presentation with an asset-built hex village landscape, a lower readable camera, direct world selection, and bottom controls; expand the existing free environment palette.
- `coop-city-match`: Remove the square 3x3 geometry constraint from personal cities while retaining exactly nine indexed building slots, ownership, the fixed roster, and the existing economy and battles.

## Impact

- Primary implementation area: `src/Game/Tabletop.cs`, including terrain placement, model instantiation, camera framing, picking, and UI layout. Small client-side helpers are allowed where they keep this code clear.
- Asset additions and documentation: `src/Game/Assets/KayKit/`, its manifest and README, plus gameplay/verification documentation and representative captures.
- This deliberately supersedes the current square-board requirements in both affected capabilities. It does not change the network protocol, saved session identity, balance, nine-slot snapshot shape, or automatic combat model.
- Existing core/network tests and source/client/server export checks remain applicable; graphical verification must cover the new picking and layout.
- No new runtime dependencies, paid assets, procedural world generation, terrain gameplay, manual unit control, or character-animation work are included.
