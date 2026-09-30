# Tasks

## 1. Baseline and asset preparation

- [x] 1.1 Before changing presentation code, capture a reproducible four-city graphical baseline at 1100x820 and 1280x720 using ordinary gameplay; record host/renderer, scenario population, frame-time samples, and available node/resource/draw-call counts in `docs/verification.md` so the final scene can be compared on the same setup.
- [x] 1.2 Vendor only the chosen grass/slope, river, hill/tree/rock, and village-prop assets from the existing pinned official Medieval Hexagon Pack version; update the asset manifest and README, and verify every checksum and glTF buffer/texture dependency resolves locally with license coverage.
- [x] 1.3 Add a terrain instantiation path with a common scale and preserved authored origins while retaining appropriate building normalization; verify grass, river, and selected slope joins render without seams or material loss after `mise run prepare`.

## 2. Hex village landscape

- [x] 2.1 Define the nine stable slot IDs as three staggered rows of hex centers/footprints and use that layout for building placement at the terrain surface; verify all nine empty plots and level-one/level-two buildings match the corresponding snapshot slots without changing core rules or wire data.
- [x] 2.2 Replace the square base and marked road with the bounded grass landscape and clear battle approach; add the side river, wooded hills, rocks, and restrained village props, and inspect the empty starting scene for connected terrain, intentional river ends, nine distinguishable plots, and visible home/defender.
- [x] 2.3 Replace the food can with a labeled medieval sack, preserve clear farm/resource/unit identities, and verify soldiers, enemies, defender shots, and transferred enemies remain at their authoritative destinations on the grassy approach.
- [x] 2.4 Create scenery only with each board and reuse imported resources; verify repeated snapshot updates and city switching do not increase scenic population, and that normal headless server/client startup instantiates no graphical terrain.
- [x] 2.5 Update the asset README and gameplay description for the authored hex landscape and purely decorative terrain; verify they describe nine indexed slots, existing gold/food, and unchanged automatic battles without square-board or road assumptions.

## 3. Direct plot and building selection

- [x] 3.1 Implement focused-city camera-ray picking against cached building bounds, falling back to exact hex footprints; verify clicks on all nine plot centers and each building's visible body/roof select the intended slot, including after level-two upgrades.
- [x] 3.2 Introduce an explicit no-selection state, clear it on match/focus changes, and revalidate on state restoration; verify no contextual action can spend against an implicit or previously observed slot and resumed buildings retain the correct slot mapping.
- [x] 3.3 Replace per-frame tile tinting with restrained hover/selection feedback that preserves imported materials; verify selected plots/buildings remain identifiable and hovering scenery or gaps does not move selection or spend resources.
- [x] 3.4 Exercise construction, upgrade, and explicit recruitment through rendered world clicks and contextual buttons using the existing normal command/input paths; verify authoritative acknowledgments and resource changes, observed-city restrictions, and disabled actions while ready, paused, disconnected, or in combat, and record the input path and results in `docs/verification.md`.
- [x] 3.5 Update `docs/gameplay.md` to explain clicking a plot/building and then activating the contextual action; verify the documented build/upgrade/recruit sequence works without using the duplicate slot selector.

## 4. Bottom controls and lower camera

- [x] 4.1 Replace the right sidebar and numbered slot grid with the bottom panel, grouped resource/city information, contextual actions, match controls, and lifecycle feedback; verify all original costs/status fields and Start, Ready/Unready, Pause/Resume, Reconnect, and fresh-session actions remain reachable in their applicable states.
- [x] 4.2 Make the bottom panel consume pointer input and keep its height stable across contextual changes; verify button clicks never select terrain beneath the panel, no-selection prompts are clear, and server rejection text remains visible.
- [x] 4.3 Lower the orthographic pitch to approximately 34 degrees and frame against the usable rectangle above the measured panel, updating on focus/size changes; verify all nine fully built plots, home/defender, and the full approach fit at 1100x820 and 1280x720 with no scenic occlusion or horizontal panel clipping.
- [x] 4.4 Tune native panel styling, daylight, restrained shadows, and floating-label density; verify readable button states, farm identity, selected building details, resource labels, unit health/affiliation, and outcomes in inspected empty/full-city and combat captures at both verification sizes.
- [x] 4.5 Recheck world picking after the final framing and resizing, including level-two roofs and city tabs; verify each selects the correct slot and that switching cities resets selection without affecting ownership.
- [x] 4.6 Update README launch/interaction instructions, gameplay screenshots, and graphical verification notes for bottom controls and the lower hex scene; verify the documented route covers lobby, observation, ready/unready, pause, disconnected/reconnecting, fallen cities, and victory/defeat without obsolete sidebar instructions.

## 5. Integrated verification

- [x] 5.1 Compare the completed four-city scene with the recorded baseline at the same host/renderer, resolutions, and comparable combat population; measure frame times and scenic population stability, tune density/shadow casters if needed, and record actual performance and limitations in `docs/verification.md`.
- [x] 5.2 Run `mise run ci` and verify formatting, locked build/import, existing core tests, real network lifecycle checks, Linux client/server exports, and export smoke all pass with the expanded vendored palette.
- [x] 5.3 Inspect and interact with the exported graphical client, including a full hex city, combat, city observation, and paused reconnect; verify offline asset loading, correct roof/plot selection, retained authoritative state, and bottom-panel feedback, and confirm the stripped dedicated-server export still operates without visual resource instantiation.
