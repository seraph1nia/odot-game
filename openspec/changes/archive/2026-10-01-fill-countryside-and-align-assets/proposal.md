# Proposal

## Why

The fixed terrain patches leave the village and menu looking like floating islands. Mixed asset placement and separate menu scenery also weaken the visual connection between buildings, their supporting hexes, and the starting countryside.

## What Changes

- Center structures on identified hex footprints and seat buildings, decorative props and stacked upgrades on their actual supporting surfaces, with deliberate local offsets for small clusters.
- Fill the visible world with continuous countryside made from the bundled medieval assets, extending terrain beyond view edges without shrinking the village overview.
- Keep the nine plots and battle approach clear; use open grass nearby and progressively richer trees, hills, rocks and village props farther away as the proposed density default.
- Render the menu from the same empty starting-landscape definition, including the home, defender, terrain elevations, river and decorations, using consistent asset scales and lighting.
- Cover supported window changes and the permitted zoom/pan envelope from `add-tabletop-camera-controls`; preserve selection, overlays, headless behavior and authoritative rules.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `city-tabletop`: Replace the visibly bounded island presentation with continuous view coverage, require coherent hex/surface placement, and share the starting countryside with the menu.

## Impact

Presentation changes are concentrated in `src/Game/VillageLayout.cs`, `Tabletop.cs`, `GameApplication.cs` and reusable landscape/asset placement helpers under `src/Game`. Existing DevRunner economy, launcher and exported-package slices gain focused visual/geometry assertions and captures; verification documentation records cost and limits. No gameplay, protocol, dependency, paid asset or runtime download changes are needed. The active camera change touches framing and observations: integrate with its final camera bounds without editing its artifacts or changing navigation policy. The current landscape requirement explicitly calls the countryside bounded; the delta changes that visual contract while retaining a finite playable area.
