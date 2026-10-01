# Tasks

## 1. Baseline and free asset provenance

- [x] 1.1 Record a successful full `mise run ci` before baseline, reusing existing evidence only when source/environment inputs are unchanged; verify the report covers the complete suite and record its evidence path. Report missing user-managed prerequisites without installing them.
- [x] 1.2 Obtain the official free archive, inspect its actual SVG/PNG/widget/icon inventory and included terms, select one warm/cozy or supplied default palette, and bundle only the required free files under `src/Game/Assets/TrioUI/`; verify imports in the locked Godot .NET version and list explicit text fallbacks for unsupported icon concepts.
- [x] 1.3 Add the asset README, license/permission evidence and provenance manifest with archive version/hash, selected-file hashes, original paths, nine-slice metadata and any derived variants; verify every bundled file against the manifest and confirm clean-checkout availability with no runtime downloader.

## 2. Shared theme and outer menus

- [x] 2.1 Extend `ApplicationTheme` and add a cached presentation asset/icon catalog with verified texture paths and semantic mappings; verify textured nine-slice panels/buttons and distinct normal/hover/pressed/disabled/focus states through actual menu observations and frames.
- [x] 2.2 Coordinate dialog/window, tab, popup/dropdown, slider, scrollbar and tooltip styling with the free palette, including matching primitives for missing widgets; verify Settings Graphics/Audio/About, open dropdowns and master-volume controls through the existing `settings` slice, retaining native input routing and persistence assertions.
- [x] 2.3 Apply appropriate icons and palette adjustments to `GameApplication` start/multiplayer controls, Steam identity/status footer, Settings entry points and update buttons; extend existing launcher/settings observations to report actual resource assignments and control bounds, then verify pointer/keyboard navigation, modal blocking and text readability at 1100x820 and 1280x720 with `mise run test-ui --scenario launcher` and the affected settings coverage.
- [x] 2.4 Style dynamic friend-invitation rows and join confirmation with appropriate icons and clear labels; extend the existing owned friend/join fixtures and verify modal focus, refresh/invite/close actions and layout through the launcher slice with `ODOT_STEAM_DISABLED=1`, without opening real Steam UI.
- [x] 2.5 Document the chosen shared palette, supported icon meanings and text fallbacks in the asset README, and update launcher/settings coverage descriptions in `docs/verification.md` and scenario risk metadata; verify documentation matches the observed controls and existing selectable commands.

## 3. In-match menus and information

- [x] 3.1 Update `Tabletop` resource totals and construction/recruitment/research costs to use consistent icon/name/value presentation where mappings exist, keeping exact authoritative values, contextual selection, tooltip detail and stable selectors; extend existing economy assertions and verify accepted/rejected/foreign-city actions with `mise run test-ui --scenario economy` and applicable `mise run test` checks.
- [x] 3.2 Apply kit styling and appropriate icons to city inspection/roster, lobby start/invite, ready/unready, pause/resume, return, outcome and disconnect/reconnect controls; verify states and eligibility through the existing launcher/economy/reconnect coverage, retaining cooperative and host-loss assertions.
- [x] 3.3 Adjust spacing, wrapping, panel sizing and camera framing as necessary for kit padding and icons; extend relevant existing checkpoints to verify full essential control bounds, unambiguous values and nine selectable plots/full battle approach at 1100x820 and 1280x720, and inspect captured PNGs for stretched borders or clipped costs.
- [x] 3.4 Update `docs/verification.md` and economy/reconnect scenario risk descriptions for the added HUD/state assertions; verify recorded commands remain checked in and no additional matches or option matrices were introduced.

## 4. Overhead unit health bars

- [x] 4.1 Implement a reusable Trio-styled, input-transparent health-bar Control using shared textures/theme resources and authoritative current/max health; verify full, wounded/ranked, zero and clamped fractions with meaningful cheap assertions if a calculation helper is extracted, including 525/1050 = 0.5, and report invalid maximum health as a presentation failure.
- [x] 4.2 Integrate one projected bar per visible living unit with `Tabletop` ownership and the sampled `UnitView.State`, replacing overhead numeric health while retaining role/faction identification; extend unit/UI observations with actual bar current/max/fraction, visibility and screen bounds, then verify both factions, full and damaged bars and camera/window reprojection within the existing `combat` slice.
- [x] 4.3 Reconcile health-bar visibility and cleanup for death, undeployed/offscreen/unfocused units, pause, transport loss, city switching, redistribution, reconnect and fresh sessions; extend existing combat/reconnect observations and assertions without new battle setup, and verify freeze/restored fractions, no stale/duplicate bars, immediate casualty hiding and owned teardown using `mise run test-ui --scenario combat` and `mise run test-ui --scenario reconnect`.
- [x] 4.4 Document overhead-bar coverage, bounded ownership and visual inspection limits in `docs/verification.md`, update existing combat/reconnect risk descriptions, and inspect representative combat frames for readability/overlap and retained faction identity; verify docs correspond to executed assertions and retained PNG evidence.

## 5. Integration and export verification

- [x] 5.1 Confirm graphical exports include all selected Trio resources and stripped server exports do not load them; verify source and package paths through normal CI export/headless/graphical smoke stages, including representative menu/HUD and bar resource checks in existing package coverage where source-only checks would miss packaging defects.
- [x] 5.2 After locked solution restore, run `dotnet format Odot.slnx --no-restore` for changed C# and complete applicable cheap checks plus the final full `mise run ci`; record complete-suite results, timings, owned logs/PNG paths and visual review findings, without repeating already-passing slices unless relevant inputs changed or a concern remains.
