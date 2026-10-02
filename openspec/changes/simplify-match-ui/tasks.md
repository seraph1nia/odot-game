# Tasks

## 1. Baseline and text-only presentation

- [ ] 1.1 Record the current source/environment inputs and a full `mise run ci` baseline before implementation, reusing an existing successful baseline only when those inputs match; verify all required stages passed and retain its evidence path while preserving unrelated working-tree edits.
- [ ] 1.2 Remove owned button/content/tab icons and embedded cost images through `UiAssets`, Settings and owned menu/friend/dialog surfaces while retaining Trio styles and the world-marker icon asset; update existing launcher/settings icon expectations and verify text costs, focus and disabled states with `mise run test-ui --scenario launcher` and `--scenario settings` as applicable to this group's changed inputs.

## 2. Compact HUD and resource placement

- [ ] 2.1 Add the top-right six-resource name/amount table and move totals out of the lower HUD; update current-state observations and existing economy assertions to verify exact balances including zero after spending, production, city switching and reconnect, and verify table input blocks the world using the existing economy/reconnect UI slices.
- [ ] 2.2 Replace the verbose city tabs with `< [YOU] >` navigation in their current location, stable wrapped roster cycling and single-city disabled arrows; adapt runner city-selection helpers and verify foreign-city edit protection, selection clearing, overview reset and retained cooperative inspection through existing economy and reconnect assertions.
- [ ] 2.3 Lay out construction as three columns and three row positions plus current categories, reduce the bottom panel to approximately 180px, and remove listed headings/summaries/empty-plot prompts/routine Match started text; add compact Details inspection for upkeep, rewards, roster and army data, retaining exact costs and disabled explanations. Verify existing economy flows and PNG/control-bound evidence at 1100x820 and 1280x720, including all plots and battle approach clear of the resource panel.
- [ ] 2.4 Project Building 1/2/3, Preparation and Combat into the compact highlighted phase column with small wave/turn/boss counters and side-by-side Ready/Pause; update relevant pure presentation assertions and run `mise run test`, then verify authoritative progression, unready and pause/resume through the existing economy/launcher UI flows.
- [ ] 2.5 Update HUD/Details coverage descriptions and timings in `docs/verification.md` using measured evidence from this group's applicable UI runs; verify documentation matches checked-in selectors and commands, and retain existing cooperative economy, food-reserve and reward assertions.

## 3. Camera keyboard and mouse interaction

- [ ] 3.1 Replace ResetView control with an eligible non-echo Space reset and shortcut help, adapting all runner reset helpers to actual key input; verify overview restoration, no gameplay commands, selection retention and consumed/modal Space priority through extended existing camera checks.
- [ ] 3.2 Add left-button pending-click/drag classification and ground-anchored bounded panning in `Tabletop`/`TabletopCamera`, including release outside the world and interruption on reset, focus, modal and city/session changes; extend checked-in actual-input support if needed and verify short-click selection, drag without selection/spending, travel bounds, interruption and subsequent clicks through existing economy/reconnect camera flows.
- [ ] 3.3 Update camera scenario risk descriptions and document left-drag/Space in README and verification coverage, preserving existing edits. Verify camera/UI changes with applicable cheap `mise run test` checks and `mise run test-ui --scenario economy` plus `--scenario reconnect` for changed relevant inputs; retain paused health-bar/camera assertions and source frame evidence.

## 4. Board marker and city health presentation

- [ ] 4.1 Replace Locked land labels with input-transparent bundled gold/buy markers and remove only stockpile resource labels; update observations and extend existing actual plot-purchase assertions to verify marker removal after acceptance, stable selection and unchanged physical authoritative stockpile tiers with the economy UI slice.
- [ ] 4.2 Replace the home identity/raw-health label with a projected percentage bar derived from matching authoritative current/maximum scales; add focused cheap checks for fraction/rounding where needed and extend existing UI observations to verify full/wounded/fallen health, camera alignment, input transparency and cleanup across city switch/reconnect using existing captures, without adding a full graphical battle solely for this bar.
- [ ] 4.3 Document the marker, home-bar and retained-stockpile coverage in `docs/verification.md`; verify supported-size source frames show these elements clearly and existing unit-health, placement, feedback and cooperative assertions still pass for changed inputs.

## 5. Confirmed return through Settings

- [ ] 5.1 Add session-only Return to menu in shared Settings and an application-owned confirmation with mode-appropriate warning, Cancel focus, Escape/close cancellation and modal input protection; expose current dialog targets/state and extend existing launcher/settings checks to verify cancel preserves session/selection/readiness, Settings focus is restored and non-session Settings omits the action.
- [ ] 5.2 Wire confirmed leave to the existing session lifecycle after ordered modal closure, guarding repeated/stale callbacks and session end while confirmation is open; adapt ReturnViaControl and other affected UI leave drivers and verify solo fresh isolation, one-time disposal, preference/music continuity and guest/host leave behavior through existing launcher/reconnect and lifecycle coverage rather than adding redundant network scenarios.
- [ ] 5.3 Update launcher/settings risk descriptions and return-flow documentation, recording why actual modal/focus assertions need UI coverage and the measured incremental setup/runtime cost; verify `mise run test-ui --scenario launcher` and `--scenario settings` after this group's relevant changes, preserving private owned state and Steam-disabled local menu checks.

## 6. Integration acceptance

- [ ] 6.1 Restore with existing dependency locks and format changed C# using `dotnet format Odot.slnx --no-restore`; verify applicable cheap tests and selected affected UI slices after any relevant final edits, without repeating unchanged successful checks.
- [ ] 6.2 Run full `mise run ci` after the coherent implementation, covering all source checks, sequential exports and package smokes; verify packed-client Space/drag, resource table, text controls, marker/home bar and confirmed return through the reused package helpers. Record complete results, timings, source inputs and PNG evidence, distinguishing filtered coverage and software-rendering limits; report missing prerequisites without installation and perform no publishing.
