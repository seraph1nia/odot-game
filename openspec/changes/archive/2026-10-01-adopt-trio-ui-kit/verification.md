# Verification

## Before baseline

Reused the full successful `mise run ci` at `logs/20261001-071039-07264336/ci-summary.json` (220.54s): 133 core / 107 runner tests, six network scenarios, five source UI slices, sequential client/server exports and headless/graphical package smoke. At apply start there were no implementation/dependency/tool lock diffs and no source/tools/tests input modification times after that pass. SDK 10.0.401 and Godot 4.7.2 .NET still match the declared locks in the same full-access Linux environment. Subsequent edits were documentation/OpenSpec archival/planning only.

No prerequisites were installed. Existing unrelated archive/spec/documentation changes are retained.

## Free archive licensing blocker

Downloaded the official `TrioUIKit_FreeSample_v2.2.zip` through itch.io's free download flow. Archive SHA-256: `db8103aa47880b4c784a272867cc0257e5bc32ddcfd1c8fccee04ed0699e2f33`. Actual inventory: 30 Cozy widget SVGs, 40 icon SVGs and matching 1x PNGs. Suitable panels, four button states, health bars, settings/play/health/weapon/resource icons are present.

The bundled `TrioUIKit_FreeSample_v2.2/README.txt` grants unlimited personal/commercial use but also says: “Do not resell or redistribute the files themselves.” The public page describes use permission without this qualification. Raw asset bundling in source, required by the approved clean-checkout/provenance plan, therefore needs clarification before proceeding. No kit files have been copied into project assets and no project code has changed. Task 1.2 remains incomplete.

Apply paused at 1/18 tasks pending a source redistribution permission clarification or an explicit plan revision. The author has not been contacted: external messages require user authorization. The downloaded archive/extraction are temporary local inspection files only.

## Permission resolved

The user confirmed: 'it's fine, I have permission, I will lock them in a private repo later.' This authorizes source bundling for the requested change. Upstream terms and this project-specific permission context are retained in the asset README/manifest. Implementation resumed without publishing or contacting anyone.

## Iteration evidence

- Trio archive subset imported successfully with locked Godot 4.7.2 .NET; all 40 selected original/derived provenance entries verified. Derived button text removal, health track/fill extraction, dropdown/close raster sizes and slider knob extraction are documented in the manifest.
- Cheap tests passed: 140 core and 107 runner tests, including wounded ranked health fractions, zero/overfull clamping and invalid maximum failure.
- Selected launcher passed 40.10s (48.85s with preparation/display), `logs/20261001-174625-30b68dac`. Actual textured panels, four button states, focus border, menu icons and complete control bounds were checked at both sizes, including owned friends, join confirmation, modal/keyboard/session/exit assertions. Menu and friends PNGs were visually inspected.
- First selected economy and combat passed, respectively 20.73s (`logs/20261001-175144-fc4afd07`) and 13.20s (`logs/20261001-175222-ab1e02ee`), but screenshot inspection caught vertically wrapped resource labels expanding the panel. This visual defect was corrected with non-wrapping numeric/name labels and a new bounded-HUD/single-line assertion. Those runs are partial iteration evidence, not final layout acceptance. The combat slice already checked both factions' full bars, damage fractions, pause freeze, death hiding and fresh-session cleanup.

- Corrected economy passed 22.22s (30.94s with prepare/display), `logs/20261001-175343-a707e8d8`, with exact authoritative cost text, resource icons, non-wrapping labels, bounded HUD and nine selectable plots. Its corrected PNG was visually reviewed.
- Settings iteration exposed TabContainer's internal theme variation and its icon sizing. Both TabBar/TabContainer styles/colours are now coordinated, tab icons are explicitly bounded, and inactive embedded windows use the kit border. The native resolution popup opens with no keyboard-focused item; the checked-in driver now observes the first and second Down selections before Enter rather than assuming Home navigation.
- Selected settings passed 8.37s (17.05s with preparation/display), `logs/20261001-180035-93fb0640`: actual popup/tab/slider paths, Escape consuming only the open dropdown, real 1280x720 resolution change, About/update, Master mute and owned preference restart. Dropdown/audio frames visually inspected at both sizes. Earlier failed iterations retained evidence/owned cleanup.

- Expanded combat passed 15.44s (23.92s with preparation/display), `logs/20261001-180237-973b8ce4`: both factions' overhead bars, authoritative full/damaged fractions, pause/death cleanup, fresh session reset and native 1280x720 reprojection while models/health remain frozen. The resized/paused frame was visually reviewed. This adds only several native control/probe waits to the existing first-wave slice.
- Reconnect passed 10.70s (19.05s with preparation/display), `logs/20261001-180356-e0f48e42`, retaining city identity/cooperative gameplay checks and asserting each restored bar's current/max/fraction against the resumed snapshot without duplicates or historical effects.

All feature groups are implemented and their relevant selected slices pass. Graphical export presets explicitly include the Trio README, supplied terms and provenance manifest; packed menu/HUD probes check these resources alongside actual textures. Server export configuration and authority initialization remain isolated from UI loading.

## Final integration evidence

Full `mise run ci` passed in 224.90s, evidence `logs/20261001-180607-c286409c/ci-summary.json`: locked restore, formatting, warning-free build/import, 140 core tests, 108 runner tests (including all Trio manifest hashes), all six network scenarios (43.61s), all five source UI slices (98.91s), sequential Linux client/server exports, headless package smoke (1.09s) and graphical exported-package smoke (50.02s; 53.33s including display overhead). Changed C# was also formatted with `dotnet format Odot.slnx --no-restore` before CI.

Final source slice times: economy 22.41s, reconnect 10.68s, settings 8.26s, launcher 38.92s and combat 15.33s. Compared with the unchanged before baseline, full CI increased by 4.36s; this is one measured run, not a performance benchmark. No additional scenario or complete battle was introduced.

Actual packed menu/HUD/bar textures and bundled provenance were asserted successfully; stripped dedicated server smoke starts without presentation. Visually reviewed `exported-package/menu-1100x820.png` and `exported-package/packed-shooting.png` from the final run in addition to the selected source frames above. Panels retain rounded borders, resource values stay on one line, costs remain visible and overhead bars retain role/faction markers. Compact bars can overlap when units converge; they do not change unit positions or capture input. Software rendering establishes this owned UI path, not native GPU/compositor performance or physical listening/input quality.

All 18 tasks are complete. No dependencies, tool locks, user preferences or external services were changed; no assets/packages were published. OpenSpec validation and documentation consistency checks complete the change without rerunning the already-passing game suite for documentation-only edits.
