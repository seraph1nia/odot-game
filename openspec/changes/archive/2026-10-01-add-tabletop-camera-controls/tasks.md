# Tasks

## 1. Camera navigation and rendered input evidence

- [x] 1.1 Establish or reuse an unchanged full `mise run ci` before baseline and record its source/environment inputs and evidence; report missing prerequisites without installing tools.
- [x] 1.2 Separate fitted overview framing from local camera state in `src/Game`, implement ground-plane cursor zoom and normalized delta-based bounded panning, and declare physical WASD/arrow camera actions; verify locked restore/build and existing overview assertions still pass at 1100x820 and 1280x720.
- [x] 1.3 Extend supervised input with wheel coordinates and key-down/key-up while preserving one-shot key commands, add camera/world-area observations and accurate projected plot visibility, and add helpers that release held keys in finally; verify actual input events change observations and fresh probes remain ordered through the existing child driver.
- [x] 1.4 Extend the existing economy slice for interior cursor anchoring, zoom-limit no-ops, representative WASD/arrow holds and releases, normalized diagonals, travel bounds and moved roof/foreign-city selection without unintended spending; run `mise run test` for affected runner changes and `mise run test-ui --scenario economy`, preserve cooperative assertions and capture evidence.
- [x] 1.5 Document wheel direction, ground-plane cursor anchoring, keyboard controls and bounded local navigation in README.md; verify the text matches implemented actions and update the economy risk description and verification coverage/cost notes.

## 2. UI priority and recoverable view lifecycle

- [x] 2.1 Implement eligible unhandled key tracking, release handling, window/modal focus gates and interrupted-key disarming; preserve dropdown/text/button navigation and release ordinary HUD focus on world interaction. Extend existing settings assertions for HUD/modal wheel suppression, consumed arrows and interruption/release behavior; verify with `mise run test-ui --scenario settings` using owned input/focus and no real lobby UI.
- [x] 2.2 Add the themed Reset view control and controls hint, preserve local navigation across window/HUD resizing, and reset on different-city/fresh-match/tabletop transitions; extend economy checkpoints for reset, city switching and resized/reset layout at both supported sizes and run the affected economy slice.
- [x] 2.3 Preserve adjustments on same-match/same-city reconnect and keep available city navigation independent of connection and gameplay pause; extend the existing reconnect slice with adjusted-view restoration and world picking, and verify `mise run test-ui --scenario reconnect`.
- [x] 2.4 Document Reset view, city/match reset, resize/reconnect retention and UI focus priority in README.md; update settings/reconnect risk descriptions and coverage notes, checking consistency with the spec deltas and captured evidence.

## 3. Frozen overlays and packed parity

- [x] 3.1 Ensure health bars and hover/selection use the final camera transform every frame independently of combat playback; extend the existing paused combat checkpoint with camera navigation, stable health/world anchors, reprojected bar bounds and visibility, and verify `mise run test-ui --scenario combat` without adding another battle.
- [x] 3.2 Extend the existing exported-package slice with a compact actual-input zoom/pan/reset/selection assertion; verify against current exports via `mise run test-ui --scenario exported-package` after sequential explicit exports or within final CI, without implicit source preparation or package rebuilding by that slice.
- [x] 3.3 Update combat/package risk descriptions and docs/verification.md with projection/paused-state coverage, added timing cost, captured PNG/log locations and software-rendering limitations; verify retained role/faction, health, cleanup and cooperative assertions.

## 4. Integration gate

- [x] 4.1 Restore `Odot.slnx` in locked mode, format changed C# with `dotnet format Odot.slnx --no-restore`, and run full `mise run ci` after the coherent implementation; record complete source/network/UI/export/package results and evidence, reusing targeted passes only while their relevant inputs remain unchanged.
- [x] 4.2 Validate change artifacts with `openspec validate add-tabletop-camera-controls --strict`, review final code/docs/spec consistency, and confirm camera state is presentation-only and headless checks create no camera/UI; record any unexecuted coverage explicitly before marking implementation complete.
