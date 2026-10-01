# Design

## Context

See proposal.md for motivation. `Tabletop` already owns an orthographic `Camera3D` with a 34-degree pitch and 28-degree azimuth. `FrameCamera()` fits landscape/roof/approach bounds above the HUD and caches viewport size, panel height and observed city. It currently assigns camera position and size whenever those inputs change. World picking uses `ProjectRayOrigin`/`ProjectRayNormal`; health bars use `UnprojectPosition` and refresh after framing. The HUD is in a separate `CanvasLayer`.

`GameApplication` blocks modal input, and tabletop selection uses `_UnhandledInput`. DevRunner has supervised one-shot `key` and `click` commands, fresh UI observations, private-display captures, and selectable economy/settings/reconnect/combat/package slices. A one-shot key press/release cannot verify continuous held movement without extending this input path. The completed `adopt-trio-ui-kit` change also touches the panel and health overlays; retain its styles and assertions.

The current city-tabletop spec guarantees a fully visible city. The delta explicitly retains that guarantee for the default/reset overview while allowing deliberate close views.

## Goals / Non-Goals

**Goals:** Isolate local navigation from fitted framing; use Godot's built-in projection/input facilities; keep cursor zoom, ground-plane movement, UI focus and lifecycle behavior predictable.

**Non-Goals:** Camera rotation/orbit, perspective rendering, edge scrolling, dragging, collision avoidance, saved camera preferences, custom keybinding UI, unit control, server camera synchronization, or a camera plugin. Start with immediate zoom/pan rather than adding easing that complicates cursor anchoring.

## Decisions

### 1. Compose the fitted overview with local navigation

Extract a small presentation controller under `src/Game` that owns relative zoom, city-relative ground-plane offset and held navigation. Keep the existing fit calculation as the base pose and size. Apply navigation each frame after any base framing update and before hover, UI target projection and health-bar sampling. A `Node3D` rig is optional; explicit base pose plus ground offset avoids unnecessary scene hierarchy changes. Do not place Godot or camera policy in `Game.Core`.

Window/HUD changes recompute the base and reapply relative adjustments. City switches, fresh matches and new tabletops reset them; same-match reconnection retains them. Navigation uses real frame delta independently of the combat playback clock, so paused/disconnected inspection continues. Camera movement never sends a session command.

Alternative: mutate the camera directly without separating its base pose. Existing resize/focus framing would overwrite user movement and make resets inconsistent.

### 2. Orthographic zoom uses a reference ground plane

Change `Camera3D.Size`, leaving rotation, height and projection fixed. At the wheel event's viewport position, intersect the camera ray with the city's reference plane at Y=0 before and after changing size, then add the horizontal difference to the local pan offset. This keeps that reference point under the cursor. Raised terrain and roofs are intentionally not the zoom anchor; using scene collision would require new colliders and can jump between roofs and ground.

Clamp the new zoom before computing compensation; when scale is already at its limit, return without translating. Apply travel limits after compensation; bounds take priority over perfect anchoring at the edge. Reject invalid/parallel intersections without applying a partially computed change. Use viewport coordinates for camera projection and convert supervised window coordinates through the existing stretch transform.

Proposed tuning defaults: overview is 1x, maximum magnification 3x, each wheel notch multiplies magnification by 1.12 (inverse for wheel down). These are presentation constants that can be tuned without changing the behavior contract. Overview is the zoom-out limit.

Alternative: move the camera toward the city or change FOV. Neither supplies orthographic zoom; centered Size changes would miss the confirmed cursor behavior.

### 3. Pan in the camera's horizontal frame with explicit bounds

Declare dedicated `camera_left/right/up/down` actions with physical WASD and arrow bindings, separate from UI navigation actions. Map input to camera-right and camera-forward directions projected onto the ground plane; normalize diagonal input, combine opposite keys to zero and do not double speed when equivalent bindings are held together. Multiply by delta and a speed proportional to current orthographic size for similar screen travel at different zoom levels.

Proposed defaults: travel about half the visible world height per second, with city-relative X/Z offsets limited to +/-6 and +/-8 world units from the fitted base. The existing bounds are roughly 22.5 by 30 world units; these limits provide modest exploration rather than free flight. Re-clamp on any framing update. Keep constants together for tuning. Clamp explicit local offset rather than using physics collision or inventing invisible walls.

### 4. Respect consumed input and clear interrupted holds

Wheel handling runs in `_UnhandledInput` with explicit world-area and modal checks. Track eligible pan presses through unhandled key events, so consumed dropdown/text/button arrows never enter camera movement. Observe key releases even when UI consumes them; clear movement on window focus loss, modal opening, view reset and tabletop replacement. Interrupted physical keys remain disarmed until released and newly pressed, preventing echoes or global held-key state from restarting movement. Ignore key echoes as new presses. Gate per-frame movement on window/modal focus as well as event routing.

Clicking the world should release ordinary HUD navigation focus so arrow panning becomes available; keep text entry, dropdown and dialog behavior intact. `Input.GetVector()` alone is insufficient: global polling does not honor consumed GUI events. Expose a themed Reset view HUD button and concise controls hint/tooltip, using normal selectors and ensuring the supported layouts still fit.

### 5. Extend existing graphical slices for regression evidence

Extend economy for off-center cursor anchoring, wheel limits, held/released WASD and arrows, diagonal speed, bounded travel, reset, resized framing, roof picking and foreign-city inspection. Extend existing settings coverage for wheel/key suppression and interruption; extend reconnect for same-view preservation. Add camera reprojection assertions to the existing paused combat checkpoint for health bars; distinguish frozen world anchors/health from screen placement that follows the camera. A compact zoom/pan/reset/pick assertion in the existing package slice establishes packed parity.

These catch rendered projection errors, consumed-input leaks, cached framing resets and stale overlay placement that gameplay/network tests cannot see. Reuse existing peers, setup, battles, protocol assertions and cleanup; add no separate full-match scenario. Expected additional cost is a small number of bounded input/observation waits (seconds per affected slice), no extra process/display setup. Maintenance is limited to camera observations and input helpers beside existing selectors.

Extend supervised input with wheel-at-position and distinct key-down/key-up events gated by supervised mode; preserve existing one-shot key behavior. Use actual `Input.ParseInputEvent`, not direct camera setters. Release held keys in cleanup/finally. Observe base/current size, zoom ratio, pan offset, fixed orientation, world area and a reference ground projection; retain fresh observation ids and rendered PNG evidence. Update projected plot target visibility to reflect the current world area instead of marking offscreen plots visible whenever a city exists. Stable assertions should compare ground-point projection within a small logical-pixel tolerance and held displacement within delta-aware tolerances, without relying on exact frame counts.

## Risks / Trade-offs

- [Cursor drift near terrain elevation or bounds] -> Define ground-plane anchoring explicitly and give travel limits priority; verify an interior off-center point and boundary cases separately.
- [Arrow keys pan after UI use or movement sticks after a dialog] -> Track only eligible presses, observe releases, disarm interrupted keys, and exercise existing modal/dropdown routing.
- [Resize/HUD changes erase navigation] -> Separate base framing from local state and verify resized/reset overview at both supported sizes.
- [Paused bars fail to follow the camera] -> Reproject overlays every frame independently of frozen playback; preserve sampled health and world pose.
- [Test runtime grows] -> Extend existing slices with bounded observations rather than new battles or an exhaustive binding/resolution matrix; retain cooperative assertions.

## Migration Plan

No saved-data, network or asset migration is required. New tabletop instances start with the existing overview. Implement against declared locked versions; restore locked dependencies and format changed C# with the repository command. Run full CI before and after the coherent implementation, reusing an unchanged successful baseline when available, with targeted UI slices during development. Record evidence and limitations in docs/verification.md. Reverting the controller/input additions restores the fixed view without altering match state.
