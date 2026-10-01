# Proposal

## Why

The fixed tabletop overview makes it difficult to inspect buildings and battles closely. Players should be able to zoom toward the mouse cursor and move around the observed city using familiar keyboard controls.

## What Changes

- Add scroll-wheel zoom anchored to the cursor's point on the tabletop ground plane.
- Add held WASD and arrow-key panning relative to the view, with a fixed camera angle and bounded travel around the observed city.
- Keep the existing fitted overview as the default and provide a reset-view control; reset on city or match changes and preserve relative adjustments on window/HUD resizing.
- Give HUD controls, dialogs, text entry and window focus priority over camera input.
- Preserve world selection and projected health bars as the view moves, with camera state owned locally by each graphical client.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `city-tabletop`: Add local camera navigation and qualify the existing full-city composition requirement as applying to the default/reset overview, since deliberate zoom and pan can place objects outside the view.
- `game-feedback`: Clarify that paused health bars retain their health and world anchors while screen placement continues to follow local camera navigation.

## Impact

Changes are concentrated in `src/Game/Tabletop.cs`, a small presentation camera controller, Godot input actions in `src/Game/project.godot`, and existing application focus/input integration. Extend supervised input/observations in `src/Game/Main.cs` and existing DevRunner UI slices for rendered camera, selection and overlay evidence. Update player controls and verification documentation. No gameplay rules, network messages, camera plugin, asset downloads or dependency updates are needed.
