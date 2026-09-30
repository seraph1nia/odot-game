# Design

## Context

See `proposal.md` for motivation and the two delta specs for behavior. `src/Game/Tabletop.cs` currently builds the entire client presentation: square box plots, a box lane with dashed markings, an orthographic camera, and a 320-pixel right sidebar with a duplicate slot grid. World picking intersects the ground plane and rounds into square rows/columns; it does not account for raised building silhouettes. The camera is positioned at `(18, 25, 25)` relative to a city and looks toward `(5, 0, 0)`, giving roughly 42 degrees of downward pitch.

The numerical core represents a city as nine indexed slots and combat as distance along a 12-unit lane. It has no square-grid adjacency or terrain simulation. The source runs at 1100x820 with the GL Compatibility renderer. Models are cached as imported scenes, but the existing `Model` helper independently normalizes each model's bounding box, which is unsuitable for joining terrain pieces at their authored proportions. Terrain currently appears only in graphical roles. Real widget input can be exercised through the existing graphical `click` automation command.

## Goals / Non-Goals

**Goals:**
- Keep this primarily a client presentation change with a small, fixed authored scene and reusable imported resources.
- Make the empty city visually complete, and keep all nine plots identifiable and selectable with fully upgraded buildings.
- Preserve the slot-to-command mapping, automatic battle distance, session recovery, and headless/export behavior.
- Avoid per-frame scene construction and unnecessary UI/material updates as the scenery grows.

**Non-Goals:**
- Hex adjacency/pathfinding, tactical terrain, new currencies, new building rules, or a network/snapshot migration.
- Procedural maps, animated water, additional render pipelines, custom shaders, custom raster art, or character animation/replacement packs.
- A general terrain editor or a broad UI framework refactor.

## Decisions

### 1. Render nine stable slots on an authored hex patch

Use three staggered rows of grass hexes for the existing slot IDs 0–8. A single client layout definition associates each ID with a center, top elevation, and hex footprint. Construction placement, selection feedback, and ground picking all consume that definition. Decorative tiles never enter the slot collection. This supersedes the square geometry constraints in both affected specs without changing `Game.Core`, wire data, or command semantics.

Assemble a bounded patch around those plots: grassy approach toward the home/defender, river at one side, small rock/tree clusters, and hills behind the village. Preserve the current numerical lane-to-world distance mapping and straight approach; the river remains alongside it, so bridge alignment or curved visual routes are unnecessary. Choose a composition whose tallest scenery is behind or outside the screen projection of interactive plots. Use the same patch for each city, translated to its city origin, retaining enough separation to avoid neighboring patches entering the focused view.

Keeping square plots would save some picking work, but would require custom transitions to fit the chosen terrain pack. A true hex gameplay grid would add unrelated rules and protocol work; indexed decorative hex presentation delivers the requested visual benefit without that complexity.

### 2. Expand the existing vendored palette and preserve terrain proportions

Select a small subset from the already pinned free Medieval Hexagon Pack commit recorded in `Assets/KayKit/manifest.json`, using its official repository. Verified candidates include `hex_grass`, `hex_grass_sloped_low`, river straight/bend/end variants, `hill_single_*` or `hills_*_trees`, `rock_single_*`, `trees_*`, and `sack`, `barrel`, or flags. Keep only the variants used in the final composition, with all glTF buffer/texture dependencies, license coverage, official source paths, retrieval metadata, and checksums. The selected river variants must form a continuous river with intentional ends at the patch edge.

Terrain uses one consistent scale derived from the base grass hex and preserves authored origins, top heights, and relative proportions. Do not pass terrain pieces through independent maximum-dimension normalization, which could shrink slopes or river variants and create cracks. Keep the current building normalization approach where useful, cache normalized bounds, and place buildings at the plot surface. Reuse imported materials and the pack's atlas; selection is a separate lightweight outline/marker rather than overwriting model materials. Replace the food can with a sack and keep explicit food labeling. Existing prototype units retain their recognizable team markers and health feedback.

Adding another pack or creating custom ground meshes would add visual matching and provenance work. The existing pack covers the terrain and prop needs directly.

### 3. Use explicit world selection with building-aware picking

Represent no selection explicitly. Clear selection when the match or focused city changes; revalidate it when the snapshot changes or synchronization completes. A click selects only; the bottom action buttons continue to send ordinary build/upgrade/recruit commands using the selected slot ID and existing eligibility checks.

Use camera rays against cached, normalized building selection bounds for the focused city, choosing the nearest valid intersection. If no building is hit, intersect the common plot surface and test the actual hex polygons. Refresh building bounds when construction/upgrades change the model. Exclude scenic props, units, home, and defender from slot targets. This allows clicking roofs and building bodies at the lower angle and prevents square rounding from accepting gaps between hexes. Keep these presentation queries independent of authoritative physics; a small client helper is preferable to introducing colliders throughout the environment.

The bottom panel consumes pointer events; the world background root ignores them, so only unhandled world clicks reach picking. Cache hover/selection state and change the marker only when the target changes. A small outline or asset marker identifies empty interactive plots and the active selection without a yellow tile wash.

Ground-only picking would select the plot behind a tall building's roof. Whole-scene mesh colliders would add setup and collision filtering for scenery that has no gameplay role. Cached building bounds plus exact plot footprints are sufficient for nine targets.

### 4. Frame a lower orthographic camera above a compact bottom panel

Start at 34 degrees above the ground, retaining the current general diagonal azimuth. Move the camera and target together when reframing so pitch remains stable. Derive framing from the usable screen rectangle above the measured HUD height and the city/battle bounds; remove the old rightward framing bias. Refresh framing on viewport/HUD size or city focus changes instead of issuing an unchanged camera transform every frame. Keep the existing viewport and canvas rather than adding a second rendered viewport.

Use a full-width bottom `PanelContainer` with compact native containers, aiming for approximately 180–220 pixels at the default size. Arrange resources and city tabs on the left, selected building details/actions centrally, and phase/Ready/Pause/Start on the right, with a compact connection/feedback row. Reconnect and fresh-session controls remain reachable when applicable. Allow wrapping/reflow at 1280x720; essential actions and rejection messages must remain accessible. With no selection, contextual actions are hidden or disabled and a world-click prompt takes their place. Contextual visibility must not unexpectedly resize the HUD while a player clicks an action.

Reduce long permanent instructions and repetitive floating labels; keep selected building details in the panel while preserving clear farm identity, ownership, unit affiliation/health, defender attacks, and outcomes. Use warm neutral panel accents with strong text contrast and simple readable button states. Retain one directional light with restrained shadows and ambient daylight; tune exposure so grass and roof colors remain recognizable.

A perspective camera or a draggable camera would add projection and interaction choices beyond the requested small angle change. A lower fixed orthographic composition is easier to keep readable and predictable.

### 5. Keep scenery fixed and measure the graphical cost

Build each city's terrain once when its board is created and free it with that board. Continue updating units from snapshots and rebuild buildings only when their slot state changes. Share packed scenes, meshes, textures, and immutable materials; avoid per-frame bounds traversal, UI text rewrites, and tile material mutation when values have not changed. Start with a modest scene budget of roughly 60–80 terrain pieces and at most 30–40 decorative objects per city; reduce density when simpler clusters provide the same composition. These are tuning budgets, not extra gameplay capacity.

Keep the existing GL Compatibility renderer, a single shadow-casting sun, and static imported water. Ordinary instances are adequate at this scale; use instancing batches for repeated foliage only if profiling shows a draw-call bottleneck. Avoid committing to a batching framework before measuring.

Before implementation, record a reproducible graphical baseline on the available host, then compare the new four-city scene at the same renderer, resolution, and comparable combat population. Record frame-time samples and resource/node or draw-call counts where available, and confirm scenic population does not grow as snapshots arrive. Aim for 60 FPS on the verification host and report the actual hardware/renderer and results rather than claiming a portable guarantee. If the scenery is the bottleneck, first reduce decorative density and shadow casters. Input correctness and plot readability remain acceptance gates regardless of performance.

## Risks / Trade-offs

- Lower camera hides rear buildings -> Keep scenery out of plot projections, use building-aware picking, and inspect all-nine level-two layouts at both verification sizes.
- Imported tile origins or independently scaled pieces leave seams -> Preserve a common terrain scale and verify grass/river/sloped joins in the rendered scene before assembling the full patch.
- Hex appearance suggests extra rules -> Keep nine stable slots, exclude decorative tiles from picking, and document that terrain and adjacency have no gameplay effects.
- A compact panel loses lifecycle feedback -> Explicitly verify lobby, observing, ready/unready, pause, disconnected/reconnecting, fallen city, and victory/defeat states.
- Four static city patches increase render cost -> Keep a bounded authored layout, shared resources, and measured density/shadow tuning on the existing renderer.
- Click verification through injected engine input misses device behavior -> Exercise ordinary pointer interaction where available and state which input path was actually verified.

## Migration Plan

No session, protocol, or gameplay-state migration is required. Implement assets and presentation, update README/gameplay/asset mapping documentation, and replace representative graphical captures with the hex scene. Run existing formatting, core/network, import, and export gates, plus the specified graphical selection/layout/performance checks. Verify the exported client loads the expanded palette and the stripped dedicated server still avoids visual instantiation. Rollback consists of reverting the client presentation/assets and accompanying spec/documentation changes; retained server slot identities remain compatible.
