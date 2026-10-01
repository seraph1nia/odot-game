# Design

## Context

See proposal.md for motivation and specs/city-tabletop/spec.md for behavior. `VillageLayout` defines staggered hex centers, nine slot identities and row/river elevations. `Tabletop.CreateLandscape` builds a 7x11 patch, while `GameApplication.CreateBackground` independently builds a 5x7 patch. Terrain uses imported origins and a common scale; `Tabletop.Model` instead normalizes each model's largest bound and centers its full bounds. Home/defender and stockpiles use world coordinates, and tower upgrades use a fixed 0.35-unit lift. These approaches do not consistently express supporting footprints or surface contacts.

The integrated `add-tabletop-camera-controls` change introduces bounded local zoom/pan and modifies framing/input observations. This change consumes its final camera envelope; it does not revise navigation bindings, anchoring, limits or lifecycle. Existing economy, launcher and package routes already own process/display setup, selection checks and PNG evidence. Menu tests explicitly require no match or connection before entry.

## Goals / Non-Goals

**Goals:** One deterministic starting landscape and placement convention for both graphical contexts; camera-independent playable bounds and view-dependent scenery coverage; bounded generation and reusable resources.

**Non-Goals:** Infinite world streaming, gameplay terrain, changes to authoritative movement/economy, new assets or dependencies, camera rotation, new menu controls, or carrying a live match into the menu.

## Decisions

### 1. Share landscape data and rendering under src/Game

Extend the layout with identifiable hex coordinates, terrain kinds/rotations, surface heights and static asset placements. Extract a shared landscape builder and model-placement helper consumed by menu and tabletop. Keep slot mapping stable. The shared starting definition includes home, defender, nine empty plot outlines, river/bridge, elevations and decorations. Dynamic buildings, stockpiles, labels and units remain tabletop-owned; menu scenery is a passive fresh instance. Share lighting parameters and asset scales, while allowing each context to frame its usable viewport.

Use deterministic placement from cell coordinates so extending coverage or returning to the menu cannot reshuffle scenery. Proposed visual tuning is sparse grass around plots and combat, with denser peripheral clusters. This density is an assumption for review, not a request for a procedural world generator.

Alternative: duplicate a larger patch in the menu. It retains placement/scale drift and makes future edits diverge.

### 2. Distinguish tile origins, footprints and support surfaces

Retain the medieval terrain's authored grid spacing and origins; a tile's full 3D bounds must not change its grid footprint. Represent surface height separately from slab origin, using inspected imported geometry and explicit metadata for grass, riverbank, terrace and slopes. Structures use their ground footprint center and bottom contact, with per-asset overrides where roofs, blades or projections skew aggregate bounds. Cache bounds and attachment metadata by asset path.

Place each structure on a selected hex and seat it at that surface. Small props use a hex anchor plus local offsets, querying the supporting surface at their actual footprint. Inspect whether hill/tree assets include their own terrain bases; classify them as tile replacements or overlays rather than blindly stacking overlapping slabs. Keep river rotations continuous as coverage extends, and grass on both banks.

Upgrade towers attach to the supporting base's measured/declared top at the same footprint center; replace the fixed lift. Resource groups receive dedicated clear hex anchors and bounded offsets, including on elevated cells. Update defender shot origins, labels, picking bounds and marker projections to follow their new anchors. Combat X/Z positions remain authoritative and unsnapped; maintain the clear approach surface and existing contact rendering.

Alternative: snap every object using aggregate AABB centers at the tile origin. This misplaces asymmetric assets, sinks props into raised surfaces and leaves fixed-height stacks incorrect.

### 3. Separate playable framing from scenery coverage

Keep overview fit bounds based on plots, full upgraded structures, home/defender and the authoritative battle approach. Never fit the camera to all newly generated scenery. After final framing, intersect viewport corner rays with the range of terrain support elevations, expand by a tile/prop safety margin, and map the conservative footprint to hex ranges. For tabletop, include the permitted pan envelope and largest overview scale; for the menu use its own full viewport. Recompute after resize/HUD changes. Finite zoom/pan keeps coverage finite.

Maintain a deterministic cell map and cached assets; grow the covered range only when required, retaining the visited viewport envelope to avoid rebuilding while navigating. Batch imported terrain into spatial multimeshes for culling, without per-frame reconstruction. Invisible cities remain hidden as today. Never arbitrarily cap generated ranges in a way that exposes a boundary: use culling and resource reuse if cost needs reduction. Include elevation transitions and object overhang in margin calculations so low camera angles do not reveal slab perimeter walls. Internal terrace/riverbank elevation faces are intentional; the forbidden walls are the exterior patch perimeter.

Alternative: simply enlarge the fixed rectangle. It may cover current screenshots but offers no guarantee after resizing or navigation. Infinite streaming is unnecessary for bounded views.

### 4. Integrate with the camera change through final view geometry

Use current camera transforms, usable world rectangle, overview size and travel limits rather than duplicate constants from the planning draft. Implement after the relevant camera framing/input interfaces stabilize, or reconcile local edits before testing. Preserve default/reset composition and camera input policy. The countryside delta changes only the landscape requirement, avoiding an overlapping rewrite of the camera change's composition requirement.

### 5. Extend existing verification by regression value

The defects are mismatched menu/game layouts, floating/sunken or off-center structures and exposed scenery edges. Core numerical tests cannot observe imported geometry, projections or packed asset loading. Add cheap runner geometry/data checks where pure coverage and placement calculations can be tested without starting Godot, using the existing test infrastructure and keeping presentation policy out of Game.Core. Do not test the same formula against itself.

Extend economy with representative raised placement, tower stacking, full-city picking and coverage at overview and navigation extremes; extend launcher with menu/starting-layout parity, resize and return-to-menu cleanliness. Add compact shared-landscape and coverage checks to the existing exported-package route. Observations expose actual instantiated placements/support bounds, layout identity, generated cells and final camera/world rectangle. Assert conservative world coverage independently from observed cell polygons and camera rays. Retain PNG checkpoints and inspect terrain joins, contact and sightlines: geometry assertions alone do not establish visual quality. Preserve actual input, fresh observation ids, ordered child ownership, cooperative assertions and existing cleanup.

No new full-match scenario or exhaustive matrix is needed. No extra process/display setup is added. Measured software-rendering execution grows substantially because the countryside fills previously empty pixels and existing probes await rendered frames; the final full check uses the existing five-minute timeout override for the serial UI suite. Maintenance is limited to shared geometry observations and representative asset contacts; retained verification evidence records costs and the default three-minute deadline failure. Source/export screenshots must establish packed parity. Software rendering does not establish native GPU performance.

## Risks / Trade-offs

- [More tiles increase import-independent runtime cost] -> Cache scenes/materials, update coverage only on range changes, hide inactive city boards and record model counts plus scenario timings.
- [Asymmetric bounds miscenter structures or overlap slabs] -> Inspect bundled geometry, declare footprint/support overrides and verify representative elevated/stacked assets with rendered evidence.
- [Tall peripheral assets obscure the village] -> Keep protected plot/combat regions and their camera sightlines clear; inspect full-city and combat captures.
- [Concurrent camera changes invalidate coverage assumptions] -> Derive coverage from final transforms/envelope and rerun affected checks only after relevant integration inputs change.
- [Shared menu rendering accidentally creates a session] -> Keep landscape construction independent of authority/state and retain existing launcher no-match/no-connection checks.

## Migration Plan

No save, protocol or dependency migration is needed. Establish/reuse an unchanged full CI baseline before implementation, integrate shared placement then coverage/menu rendering, and run affected cheap/UI checks during development. Use locked restore and required formatting, followed by full CI including sequential exports and package checks. Update README/verification with behavior, cost, evidence and limitations. Rollback the shared landscape and consumer changes together to restore prior presentation without altering saved preferences or match rules.
