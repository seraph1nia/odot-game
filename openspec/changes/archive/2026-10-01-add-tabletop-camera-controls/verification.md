# Camera controls verification

## Before baseline

Full `mise run ci` passed in 227.54s before camera implementation, evidence `logs/20261001-181802-ec545ab5/ci-summary.json`. This includes locked restore/format/build/import, 140 core and 108 runner tests, six network scenarios, five source UI slices, sequential Linux client/server exports, headless package smoke and graphical package smoke (51.04s scenario, 54.35s with display).

Baseline HEAD: `a8d1838571cbbcbf31548079227559790cbb9d57`; workspace includes prior approved village/Trio changes. Combined sorted source/config SHA256 (tracked plus nonignored untracked files, excluding Markdown, docs and OpenSpec): `03d0fb38ba1ce9c83157b4a1260be617168d65c9f84307bcbb5d4fa68002d004`. Tools: .NET 10.0.401 and Godot 4.7.2 .NET; owned Linux X11, llvmpipe and Dummy audio.

## Regression admission

Extend existing economy/settings/reconnect/combat/package slices; no new scenario or battle. Graphical input routing, cursor anchoring, fitted-view caching, roof picking and frozen-overlay reprojection can fail while numerical/network assertions pass. Expected cost: bounded input and fresh-probe waits within existing setup, plus owned key cleanup; maintain camera observations beside existing selectors.

## Iteration evidence

- First economy iteration caught reversed arrow bindings (right arrow had the left action). The declared Godot key constants were corrected; the second selected economy passed in 29.28s, `logs/20261001-183002-f9456294` (37.70s with preparation/display). It checks off-center ground anchoring, normalized delta-based D/right/diagonal holds and release, both zoom-limit no-ops, bounded travel, reset/city switch and foreign upgraded roof selection. Existing construction/recruit/research/cooperative assertions remain. The camera-navigation frame was visually inspected: Reset view and costs fit the HUD, and the world moves without changing its angle. Earlier compiler/analyzer failures were corrected without dependency changes.

- Cheap `mise run test` passed: 140 core and 108 runner tests. Camera changes add no gameplay arithmetic or new dependencies.

- Settings focus iteration found that Openbox immediately refocused the client when X11 focus was assigned to the private root. The checked-in fixture now creates an override-redirect 1x1 owned focus window and disposes it after the check, validating the observed client PID before focus changes. Actual focus loss/recovery, held-key interruption/rearming, HUD/modal wheel suppression, slider arrows, adjusted-view resize/reset at 1280x720 and fresh-process overview all passed in `logs/20261001-183400-b4eab58d`.

- Settings passed in 11.35s (20.38s with preparation/display), `logs/20261001-183400-b4eab58d`. Reconnect passed in 12.65s (21.06s with preparation/display), `logs/20261001-183459-bfd39942`, preserving camera adjustments across local disconnect and same-city/match resumption and allowing held panning while disconnected; retained identity, current fractions, no duplicates/history and restored picking remain asserted.

- Combat passed in 17.48s (25.83s with preparation/display), `logs/20261001-183553-225178f2`. Paused zoom/pan reprojects visible bars while poses, world positions and health fractions remain frozen; the existing damage/casualty/cleanup assertions remain. Fresh solo after an adjusted battle restores the overview. The paused-camera frame was visually inspected.

- First full after-CI (`logs/20261001-183721-3184a9f6`) passed cheap/network and economy/reconnect/settings, including the new resize checkpoints (28.82s / 12.45s / 11.05s), but launcher exposed the driver sending a world click to an unfocused host while the guest owned focus. The driver now focuses only the observed owned client before nonmodal control/world clicks, as physical clicking would. Keyboard eligibility was also refined to let unconsumed WASD pan with ordinary HUD button focus; consumed arrows and text-entry/modals remain blocked. Selected launcher/settings and full CI will verify these corrections; the failed run is partial coverage.

- Refined settings passed in 12.18s (22.59s with preparation/display), `logs/20261001-184327-99e0f410`: unconsumed W works with HUD button focus, consumed arrows do not pan, modal/window interruption and native dropdown/audio/preference assertions still pass.

- Corrected launcher passed in 40.98s (49.35s with preparation/display), `logs/20261001-184419-07c23463`, retaining real host/guest input, native-close/session lifecycle, owned invitation/consent fixtures and both-size frame checks. Cheap runner tests passed again after the input/focus harness changes (108 cases, no build/restore mutations).

## Final integration evidence

Full after `mise run ci` passed in 244.03s, evidence `logs/20261001-184547-db4a4966/ci-summary.json`: locked restore, formatting, warning-free build/import, 140 core and 108 runner tests, all six network scenarios (44.43s), all five source UI slices (112.79s with display), sequential Linux client/server exports, headless package smoke (1.19s), and graphical package smoke (51.70s scenario / 55.01s including display). Camera input works through the packed client and existing exported selection/cooperative checks remain. The packed-camera-navigation frame was visually inspected alongside resized source and paused-combat frames.

Measured before/after slice cost (seconds, same declared tools and owned software display):

| Slice | Before | After | Difference |
| --- | ---: | ---: | ---: |
| economy | 22.40 | 29.28 | +6.88 |
| reconnect | 10.77 | 12.91 | +2.14 |
| settings | 8.55 | 12.17 | +3.62 |
| launcher | 40.36 | 37.92 | -2.44 |
| combat | 15.23 | 17.19 | +1.96 |
| exported-package | 51.04 | 51.70 | +0.66 |

Full CI increased by 16.49s in these two runs. Timings include ordinary run variation and are not a performance benchmark. No additional scenario or graphical battle was introduced. Frame/probe evidence includes `economy/camera-navigation.png`, `economy/camera-resized.png`, `economy/economy-camera-roof.png`, `combat/combat-camera-paused.png` and `exported-package/packed-camera-navigation.png` beneath the final run; corresponding observation JSON and engine logs are retained.

Presentation stays in `src/Game`: the controller never sends session commands, stores no camera preferences and has no authority dependency. Main's existing graphical/non-dedicated guard owns all application/tabletop instantiation, so headless roles never construct the controller or UI. No tool/dependency locks, asset provenance, unrelated preferences or external services were changed by this feature. C# was formatted with `dotnet format Odot.slnx --no-restore`; strict OpenSpec validation and `git diff --check` pass.

Owned Linux X11, llvmpipe and Dummy audio checks establish synthetic Godot input routing, projection and lifecycle. Native compositor/GPU performance, physical input, listening quality, Windows runtime and paired Steam account acceptance remain unexecuted for this change. Ground-plane cursor anchoring can differ from elevated roofs; bounded travel takes priority at the edge. Health bars can overlap when units converge, as before.

All 14 tasks are complete. Final documentation/task updates do not change runtime inputs, so no additional game/export run is required.
