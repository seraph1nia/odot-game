# Tasks

## 1. Music asset and graphical playback

- [ ] 1.1 Vendor the selected `LVS04_11_Echoes_of_Valhalla_bpm82_loop.wav` into `src/Game/Assets/Music/` and add provenance documentation recording artist, supplied source, checksum, available attribution/terms, and authored sample boundaries; verify the vendored file matches the download and no runtime path refers to Downloads.
- [ ] 1.2 Configure Godot's WAV import for Detect from WAV and QOA compression; run `mise run prepare`, inspect the imported forward-loop boundaries against samples 1,548,939 and 6,195,530, and record source/imported asset sizes and the import result in `docs/verification.md`.
- [ ] 1.3 Add one presentation-owned non-positional `AudioStreamPlayer` outside board reconstruction and the RPC node, with a restrained initial music mix; verify source-client playback starts while connecting, continues across city changes, shared pause, outcomes, disconnect/reconnect, and exit cleanup, and that headless roles create no player.
- [ ] 1.4 Listen across the first and a subsequent authored loop transition, using an ordinary engine playback seek near the boundary if needed; verify the intro is not repeated and the outro is excluded without playback gaps/clicks, and record listening observations separately from metadata assertions in `docs/verification.md`.

## 2. Preferences and Master audio

- [ ] 2.1 Add a small graphical-client settings helper using `ConfigFile` for Master volume, mode, and retained windowed size in `user://settings.cfg`; verify missing, unreadable, malformed, and individually invalid fields recover to valid defaults without touching resume credentials, and add the recovery observations to `docs/verification.md`.
- [ ] 2.2 Apply saved Master volume before music starts and use `AudioServer.SetBusVolumeLinear` with explicit zero mute; verify 0, 50, and 100 values on the Master bus, silence at zero, and continued playback position while volume is restored.
- [ ] 2.3 Save user changes after display selections, completed slider edits, menu close, and orderly exit where needed; verify restart restores valid values, write failure keeps live values with a concise menu error, and one client saving preferences does not change another running client's settings.
- [ ] 2.4 Document local defaults, preference storage, last-successful-save behavior across local windows, and the independence from multiplayer sessions in `docs/gameplay.md` or README; verify the documented change/restart/reconnect sequence matches the actual behavior.

## 3. Native settings menu and input

- [ ] 3.1 Add the top-left Settings button and an embedded themed `AcceptDialog` with native Graphics/Audio tabs, display dropdowns, a 0–100 integer Master slider, numeric value, save feedback, and Close control; verify both tabs and all controls are reachable in 1100x820 and 1280x720 windows.
- [ ] 3.2 Connect Esc opening through unhandled input with echo filtering and use the dialog's built-in closing/focus behavior; verify Esc opens/closes once per press, an open dropdown consumes Esc first, Close works, and settings remain accessible during connecting, lobby, combat, shared pause, outcomes, and disconnect.
- [ ] 3.3 Use native exclusive/transient input blocking and gate presentation hover while the menu is open or the Settings button is hovered; drive ordinary GUI/key events to verify menu clicks do not select buildings, spend resources, or activate underlying controls, and closing preserves the selected plot.
- [ ] 3.4 Verify a real two-client unpaused battle continues and receives snapshots while one client edits settings, with no pause/readiness request or second music player; document the menu controls and local-versus-shared pause behavior in `docs/gameplay.md` and record input/lifecycle results in `docs/verification.md`.

## 4. Display controls and resizing

- [ ] 4.1 Wire Windowed/Fullscreen through the root `Window.Mode` and windowed size through `Window.Size`; populate the planned presets using the current monitor's usable area, and verify fitting choices, native-size fullscreen, disabled fullscreen resolution selection, and restoration of the retained windowed size.
- [ ] 4.2 Handle manual resize and monitor-fit recovery using native notifications/queries; verify the menu reports actual sizes, fullscreen transitions do not overwrite the windowed preference, an oversized saved preference yields a usable window, and a clamped fallback works when ordinary presets cannot fit.
- [ ] 4.3 Honor explicit engine resolution/window-mode arguments over saved display preferences for their launch without persisting those overrides automatically or undoing runner window positions; verify a saved preference can coexist with a launch override and the normal two-window `dev` setup remains usable.
- [ ] 4.4 Keep the menu and essential bottom-panel controls reachable through native layout and existing camera reframing; inspect all offered sizes on the verification display, fullscreen, and a constrained window, make only necessary local layout adjustments, and document resolution/fullscreen semantics and observed limitations in the gameplay/verification docs.

## 5. Integration and exports

- [ ] 5.1 Run `mise run ci` to cover locked restore, formatting, compilation/import, core rules, real headless networking, and existing client/server export smoke checks; verify settings/music additions preserve the existing gates and headless roles neither load playback resources nor read/write graphical preferences.
- [ ] 5.2 Prepare a fresh source copy without generated caches or access to the original Downloads path; run the exported graphical client without `--path` and verify bundled music, authored looping, menu input, Master mute/restore, fullscreen/windowed changes, and persisted startup settings, plus the stripped dedicated-server startup.
- [ ] 5.3 Complete the integration record in `docs/verification.md` with actual source/export checks, graphical and listening evidence, imported/exported audio footprint, and unexecuted platform/device checks; verify the record distinguishes observed results from assumptions and that no user preferences, credentials, caches, or temporary verification artifacts are committed.
