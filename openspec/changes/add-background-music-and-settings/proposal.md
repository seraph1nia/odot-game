# Proposal

## Why

The graphical game currently has no background music or player settings. Adding the downloaded medieval track and a small graphics/audio menu will let players set a comfortable volume and window size using Godot's existing audio, window, UI, and configuration APIs.

## What Changes

- Add continuous background music from graphical-client startup through lobby, gameplay, shared pause, outcomes, and reconnects, using the downloaded Echoes of Valhalla WAV with its authored intro and loop boundaries.
- Vendor the chosen source music and record its provenance; import it using Godot's WAV loop detection and QOA compression, and play it with a non-positional `AudioStreamPlayer`.
- Open an embedded settings dialog with Esc or a Settings button in the top-left corner; close it with Esc or Close. Use native Godot containers and controls for Graphics and Audio tabs.
- Offer windowed resolution presets and Windowed/Fullscreen mode. Fullscreen uses the monitor's native size; returning to Windowed restores the selected windowed size.
- Add a live 0–100 Master volume slider with a numeric value and silence at zero, affecting all game audio through the Master bus.
- Persist preferences locally with Godot `ConfigFile`, restore them on graphical startup, and recover safely from missing or invalid values.
- Keep settings local to the graphical client. Opening the menu blocks underlying gameplay input without pausing the shared match; dedicated servers and headless clients create neither settings UI nor music playback.

## Capabilities

### New Capabilities

- `background-music`: Vendored music, authored looping, graphical-client playback lifetime, and offline exported playback.
- `client-settings`: Settings access, modal input handling, display controls, Master volume, and local preference persistence.

### Modified Capabilities

None. These additions preserve the existing tabletop interaction, server-authoritative pause, reconnect, and headless-operation requirements. The square-versus-hex presentation differences in the active `prettify-medieval-tabletop` change are outside this change.

## Impact

- Graphical implementation: `src/Game/Tabletop.cs` plus small C# presentation/settings helpers or scenes, using the existing graphical-client bootstrap in `src/Game/Main.cs`.
- Assets/configuration: a new `src/Game/Assets/Music/` directory with source WAV, import settings, and provenance documentation; Godot project/export configuration only where required for embedded UI and asset inclusion.
- Documentation and verification: settings usage, music provenance, graphical/input/loop/persistence checks, and source/client/server export checks in the existing docs and tasks.
- No additional runtime libraries, networking messages, gameplay rules, renderer changes, custom audio decoder, custom loop scheduler, or operating-system display-mode implementation.
