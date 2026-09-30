# Design

## Context

See `proposal.md` for motivation and the two delta specs for behavior. The project uses C# with Godot .NET 4.7.2 and the GL Compatibility renderer. `Main.Setup()` creates `Tabletop` only for a graphical client, as a sibling of the shared RPC node. Reconnect temporarily removes/re-adds that RPC node, leaving the presentation alive. `Tabletop._Ready()` constructs a themed `CanvasLayer`, native containers, and a bottom panel. World clicks reach `_UnhandledInput`, while hover picking is performed separately in `_Process`.

The viewport starts at 1100x820 and uses `canvas_items` stretching with expanding aspect. `FrameCamera()` already responds to viewport/HUD dimensions. The development runner positions two native client windows and forwards optional engine arguments. Client exports include all resources; dedicated-server exports customize/strip resources. Neither current headless roles nor the numerical core needs graphical preferences or audio.

Durable tabletop specs still describe square plots; the active `prettify-medieval-tabletop` change and current source describe hex plots with bottom controls. This change adds independent capabilities and integrates with the current presentation without changing either layout contract or completing that other change's tasks.

The supplied folder contains WAV, Vorbis Ogg, MP3, and AAC/M4A versions. Inspection found 44.1 kHz stereo audio, a full duration of 221.746667 seconds, and artist metadata `LUShvalleySound`. The WAV contains an authored forward loop in its `smpl` chunk: start sample 1,548,939, end sample 6,195,530 (approximately 35.123333–140.488209 seconds). The Ogg has equivalent `LOOPSTART`/`LOOPLENGTH` comments but includes the later outro. No source/license document was present in this folder; record the supplied download and available attribution accurately without claiming a license such as CC0.

## Goals / Non-Goals

**Goals:**
- Keep Godot responsible for decoding, loop playback, modal focus/input, control layout, window management, and settings serialization.
- Keep presentation preferences and music outside RPC-node and match-state lifetimes.
- Make display and volume state visible, persistent, and usable through window changes and reconnects.

**Non-Goals:**
- Custom music scheduling, seamless-loop timers, decoder bindings, crossfades, dynamic battle soundtracks, or sound effects.
- Monitor display-mode switching, render-resolution scaling, quality presets, renderer selection, VSync controls, or monitor selection.
- A settings framework, cloud preferences, per-server preferences, separate music/effects sliders, or automatic multiplayer pause.

## Decisions

### 1. Use the WAV's authored loop through Godot's importer

Vendor `LVS04_11_Echoes_of_Valhalla_bpm82_loop.wav` from `/home/bart/Downloads/LVS04_11_Echoes_of_Valhalla_bpm82_loop/LVS04_11_Echoes_of_Valhalla_bpm82_loop/` into `src/Game/Assets/Music/`. Add provenance documentation with source filename, artist, checksum, available attribution/terms, and the measured loop information. Track the source and import configuration, not generated `.godot` data. Runtime loads the imported `res://` resource and never refers to Downloads.

Use the WAV import options **Detect from WAV** and **Quite OK Audio** compression. Verify that the imported `AudioStreamWav` retains a forward loop and the original sample boundaries; verify an actual compressed loop transition. Do not trim, normalize, downsample, or otherwise alter the source because doing so can alter boundary indices. Godot's importer owns interpretation of the WAV endpoint rather than application code correcting sample counts.

The Ogg is only about 2.4 MiB versus the WAV's 37.3 MiB source size, but Godot's Ogg looping exposes a start offset and loops at file end. Using it unchanged would play the outro; preparing a trimmed Ogg would add an asset-processing step. Native WAV looping is the preferred fit for the user's engine-first requirement. QOA reduces the imported/exported footprint; its resulting size and sound must be measured rather than assumed. See [Godot audio import documentation](https://docs.godotengine.org/en/stable/tutorials/assets_pipeline/importing_audio_samples.html).

### 2. Own music and settings in the graphical presentation

Create a small client settings helper, preferably `src/Game/ClientSettings.cs`, owned by `Tabletop`; it handles local preferences and applying built-in APIs. Keep the UI as a small native scene or controls using the existing C# construction convention. Add one non-positional `AudioStreamPlayer` as a presentation child outside any board reconstruction. Load and apply Master volume before starting it. Use the existing Master bus directly, with the music player initially mixed at -12 dB and Master defaulting to 100. This is a proposed conservative initial mix and can be reduced during listening verification without changing the slider's semantics.

No autoload is needed for the current single presentation lifetime. Keeping these objects under `Tabletop` naturally preserves them across RPC reconnects and avoids accidental audio creation on servers or headless clients. Avoid static resource preloads or preference initialization outside the existing graphical-client gate. Music continues through shared pauses because the server pauses its numerical simulation; opening settings never sets `SceneTree.Paused`. See [AudioStreamPlayer](https://docs.godotengine.org/en/stable/classes/class_audiostreamplayer.html).

### 3. Assemble an embedded modal dialog from existing controls

Place a labeled Settings `Button` at the top-left of the existing full-screen UI root. Use an embedded `AcceptDialog` with a `TabContainer`, named Graphics and Audio child controls, native containers, `OptionButton`s, an `HSlider`, a numeric volume label, and a save-error label. Reuse the presentation theme and set the standard dialog acceptance button's text to Close. Keep subwindows embedded; use the dialog's transient/exclusive behavior to block parent-window input while leaving processing and networking running.

Open through the built-in `ui_cancel` action (Esc) from unhandled input and the Settings button. Ignore key echo and mark the opening input handled. Use `AcceptDialog`'s built-in `DialogCloseOnEscape` behavior (`ui_close_dialog`) to close. Allow native dropdown popups to consume Esc first. Restore focus to a usable game control when closed and verify that the same key event cannot close and immediately reopen the menu. This avoids writing a modal manager or keyboard navigation system. See [AcceptDialog](https://docs.godotengine.org/en/stable/classes/class_acceptdialog.html) and [TabContainer](https://docs.godotengine.org/en/stable/classes/class_tabcontainer.html).

Native modal input blocking protects world clicks and bottom controls, but `_Process` currently runs hover picking regardless of GUI ownership. Gate and clear world hover while the dialog is visible, and exclude the Settings button's region from hover picking. Keep the selected plot unchanged. Clamp dialog dimensions to the current visible area and let native containers arrange controls through resizing; avoid hand-positioning individual fields.

### 4. Use the root Window for display settings

Use `GetWindow().Mode` and `GetWindow().Size` for changes, rather than platform bindings or custom display-mode enumeration. Use standard `Window.ModeEnum.Fullscreen`, which fills the current monitor at its native size, and Windowed mode. Keep the selected windowed size separately while Fullscreen is active; disable its dropdown in that mode, and restore it after leaving Fullscreen.

Offer 1100x820, 1280x720, 1600x900, and 1920x1080 when they fit the current screen's usable rectangle with reasonable decoration allowance. Use `DisplayServer` only to query the current screen and available area. If none of the presets fit, provide a clamped usable windowed size and keep the UI reachable. Reflect the actual current size after manual resizing, including a custom entry when appropriate. Use native window size-change notifications to refresh the menu and preserve the existing camera reframe behavior. Preserve the selected windowed size during mode transitions rather than recording the fullscreen dimensions as a windowed preference.

Defaults follow the current Windowed 1100x820 configuration. Honor explicit engine `--resolution`, `--fullscreen`, or `--windowed` launch arguments over saved display preferences for that launch, so developer/verification launches remain controllable. Do not silently persist a launch override until the user changes a display setting. Do not reposition the window during preference restoration, which would undo the runner's two-window positions and require platform-specific behavior. See [Window](https://docs.godotengine.org/en/stable/classes/class_window.html) and [DisplayServer](https://docs.godotengine.org/en/stable/classes/class_displayserver.html).

### 5. Apply Master volume with AudioServer

Use a 0–100 `HSlider` with step 1 and map its value to the Master bus with `AudioServer.SetBusVolumeLinear(busIndex, value / 100f)`. Explicitly mute that bus at zero and unmute it for positive values; do not stop or restart playback to mute. Update the numeric label immediately. Using the Master bus ensures future sound effects are included automatically; changing only the music player's volume would not implement a Master control. No custom decibel or loudness curve is needed. See [AudioServer](https://docs.godotengine.org/en/stable/classes/class_audioserver.html).

### 6. Persist simple values with ConfigFile

Store `[audio] master_volume` and `[graphics] mode`, `window_width`, and `window_height` in `user://settings.cfg`. Load once for each graphical presentation, validate fields independently, and apply valid preferences before playback. Accept only Windowed/Fullscreen, integer volume 0–100, and positive usable window dimensions; use defaults for invalid fields and fit display values to the current monitor. A missing or unreadable file is recoverable.

Apply user changes immediately and save after display selections, menu close, and completed slider edits; avoid disk writes on every slider motion. Check save errors and expose a concise message while retaining the live values. Connect resize changes to the retained windowed size, saving it at the next ordinary save or orderly exit. Keep preferences independent of `SessionFile` and credentials. No watchers or live cross-process synchronization are needed: two existing clients keep independent runtime values, while the last successful save becomes the shared user's defaults for subsequent launches. See [ConfigFile](https://docs.godotengine.org/en/stable/classes/class_configfile.html).

## Risks / Trade-offs

- Large source WAV increases repository size -> Keep one source format, use native QOA compression, and record the imported/exported size. The choice avoids runtime loop code and a separate asset conversion pipeline.
- QOA playback or importer boundary handling could make a seam audible -> Inspect the imported boundaries and listen across the first and subsequent loop transitions in both source and exported clients before accepting the asset.
- Resolution changes may expose existing HUD clipping -> Verify 1100x820, 1280x720, every newly offered preset on the available display, fullscreen, and a constrained window; make only the local container adjustments needed to keep essential controls reachable.
- Shared user defaults can be saved by either development client -> Keep running values independent, document last-successful-save behavior, and isolate or back up user preferences during verification.
- Platform-specific window behavior and embedded editor runs can differ -> Validate native desktop windows on the available host and record unexecuted Windows/macOS checks. Avoid promising physical display-mode switching.
- The downloaded folder does not include a license document -> Record available source/artist information and supplied royalty-free provenance without inventing license terms; retain original terms if available during asset vendoring.
- Concurrent tabletop work edits the same presentation file -> Integrate the settings entry point against the current bottom-panel/hex presentation and avoid overwriting or expanding that change.

## Migration Plan

Implement as an additive graphical-client change, vendor/import the music, then verify ordinary input, persistence, and native-window behavior. Run the existing preparation, formatting, core/network, and client/server export checks. Record graphical and listening observations separately from headless checks in `docs/verification.md`, including unexecuted platforms. A fresh checkout must prepare/export/play without the original download folder.

No protocol or session migration is required. Existing users without `settings.cfg` receive valid defaults. To roll back, remove the presentation integration and bundled music; the unused preferences file can remain without affecting older clients or resume credentials.
