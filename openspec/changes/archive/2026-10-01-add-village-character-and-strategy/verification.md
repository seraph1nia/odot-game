# Verification

## Apply start: 2026-10-01

Task 1.1 is complete: refreshed the synced/archived animated-combat baseline, checked retained scenarios, and passed `openspec validate add-village-character-and-strategy --strict`. Unrelated Steam invitation changes and archives were preserved. This was the planning state before the full-access implementation resumed.

## Before-CI blocker

`mise run ci` failed before source build/import or tests, during locked solution restore. Evidence: `logs/20261001-053253-ba2c6d5b/ci-summary.json`, `restore.json` and the empty child restore log. The full runner reported restore exit 1; .NET 10.0.401, Godot 4.7.2 .NET, pinned GodotSteam files and private-display executable prerequisites were available.

Direct diagnostic restore identified denied MSBuild worker IPC: `System.Net.Sockets.SocketException (13): Permission denied` while binding the named-pipe server's local socket. Latest diagnostic worker traces were `/tmp/MSBuildTemp8p5kL3/MSBuild_pid-175_f30133fae40f486fbadcd1410e716848.failure.txt` and `/tmp/MSBuildTempMmQ9Ue/MSBuild_pid-184_521a6d22de814b6c96a3d501b6c2cb7f.failure.txt`.

`dotnet restore Odot.slnx --locked-mode -m:1` avoided parallel-worker IPC, but failed for `src/Game/Game.csproj` with NU1900 (warning treated as error): package vulnerability data could not be obtained from `https://api.nuget.org/v3/index.json`. Four of five projects were up-to-date. No audit, lock, warning or repository verification policy was weakened.

The earlier full pass at `logs/20260930-212920-6880c928/` used a different execution environment; the current sandbox denies local socket operations and restricts network access. It is therefore not reused as an unchanged source/environment baseline. At that point task 1.2 remained incomplete and the apply workflow awaited an execution environment that permits required local process sockets and NuGet access. No tools or OS packages were installed or upgraded, and no developer preferences were changed.

## Remaining coverage

The following records supersede this initial blocked state.

## Full-access before baseline

`mise run ci` passed the full required set in 205.52 seconds on 2026-10-01 with
network concurrency two. Evidence: `logs/20261001-053630-fab0f74e`. Locked
restore, formatting, build/import, cheap rules/tooling checks, all source
network/UI scenarios, sequential Linux client/server exports, headless package
smoke and graphical package smoke passed. No prerequisites were installed.
The earlier restricted-environment failures are resolved by the user's permission
change. This is the before baseline for implementation.

## Implementation checks so far

- Cheap suite passed 120 core and 107 runner tests after catalogs, symmetric roles,
  preparation, fixed-point ranks, splash/towers, four strategy families, action cue
  deduplication, stockpile thresholds and PCM generation. The 32-versus-32 fixture
  includes all four friendly roles and verifies reversed-storage determinism.
- Selected economy UI passed in 7.99 seconds of scenario execution (18.02 seconds
  including source preparation/display work), evidence
  `logs/20261001-055910-0fdc868c`. Actual input selected all nine elevated plots and
  built Farm/Barracks/ArcheryRange, recruited both early roles, and validated all
  eight real rig/socket/role-clip bindings and ten weapons.
- Starting wood is provisionally tuned 20 → 30 to retain the ordinary
  upgraded-farm plus Barracks opening (three 10-wood purchases); construction and
  production do not grant hidden subsidies. Research fixture prioritizes food
  production before spending on ranks after a research-first ordering lost.
- Network iteration caught a driver race across Building → Preparation. A current
  phase/serial observation barrier precedes the income-free preparation check.
  Passing final selected/full evidence remains required.

Selected `authority-resume-victory` passed 33.52 seconds (40.69 including prepare),
evidence `logs/20261001-061422-67986690`. Driver adds a second ordinary Farm as
resources allow, upgrades food production, recruits during Preparation, and
retains all credential/identity/pause/restart/retry/host-stop assertions. A defeat
now fails the wave-clear expectation immediately instead of waiting for timeout.

Selected `redistribution` passed 21.37 seconds (27.94 including prepare), evidence
`logs/20261001-063103-afc4f423`: real city elimination, role/faction/rank/profile/
recovery retention, equal transfer snapshots, observer resume and future 18/9/9
pressure are preserved.

Selected expanded `combat` UI passed 13.57 seconds (23.66 including preparation
and display), evidence `logs/20261001-062553-1a7f5cbd`: ordinary first-wave
Mage/Berserker recruitment, tower-city observation/Catapult impact, real sword/
axe/cast/hit/death poses, weapons, contact, voice/pool bounds, paused effect/fan
positions, cleanup and fresh-session reset. The earlier interpolation-chord
overlap is covered by a cheap regression and one shared safe-frame fallback;
endpoints remain separated and no body-specific interpolation is introduced.
New animation markers were measured from hand motion in the pinned clips:
axe frame 23 at 30 fps (1.6333-second clip), cast extension frame 8
(0.9333-second clip), on both faction rigs.

Latest cheap pass: 133 core tests and 107 runner tests, including eight local/
guest role recruitment/research retry cases and duplicate Blacksmith gating.
All 155 vendored/provenance entries pass SHA-256 verification.

Expanded economy passed 21.57s (32.09 including preparation/display), evidence
`logs/20261001-064813-75249ddb`. The ordinary route recruits both early roles,
spends third production before combat, builds a Lumbermill and Blacksmith,
researches Melee, observes an owned Catapult upgrade, clicks all nine terrace
plots/roofs and verifies foreign controls. Stockpile observations count real
resource child nodes; upgrade observations detect actual added structures.
The HUD has a stable 270px reservation to prevent contextual panel reflow from
moving camera projection during city-switch clicks. This fixes the observed
stale-coordinate selection without test-only retries or sleeps.

Master/settings passed 7.35s (15.63 total), evidence
`logs/20261001-065217-cdf9c7de`: actual Home/Right slider input mutes the Master
bus, restores the chosen owned value, and preserves it across process restart.
Reconnect passed 10.60s (18.83 total), evidence
`logs/20261001-065441-6a3ba540`: same identity/gameplay, living rig reconstruction,
no corpses/active effects/voices or old accepted-command replay.

## Finite ordinary balance evidence

Detailed checked-in-fixture output (including per-stage gold/wood/food/army/HP)
is retained at `logs/village-balance-evidence/strategies.log`. All seven cases
passed without privileged commands or grants. Durations below are fixed steps,
not wall-clock performance.

| Strategy / players | Wave steps 1 / 2 / 3 | Recruits | Casualties | Survivors | City HP |
| --- | --- | ---: | ---: | ---: | --- |
| Frontline / 1 | 320 / 501 / 551 | 19 | 12 | 7 | 100 |
| Mixed / 1 | 481 / 549 / 480 | 17 | 13 | 4 | 100 |
| Towers / 1 | 433 / 673 / 793 | 0 | 0 | 0 | 65 |
| Research / 1 | 320 / 501 / 517 | 19 | 12 | 7 | 100 |
| Frontline / 2 | 320 / 501 / 538 | 38 | 24 | 14 | 100,100 |
| Frontline / 3 | 320 / 501 / 551 | 57 | 36 | 21 | 100,100,100 |
| Frontline / 4 | 320 / 501 / 538 | 76 | 48 | 28 | 100,100,100,100 |

Mixed explicitly recruits all four roles; research explicitly buys a class rank;
tower strategy fires real towers. Every case has nine productions and three
Preparation checks. Existing ordinary empty-investment defeat and redistribution
coverage remain. Only wood changed from the seed (20 → 30); no faction-only
health/damage/cadence bonus was introduced. These are finite viable candidates,
not an exhaustive balance or difficulty claim.

## Final integration corrections

The first final CI attempt stopped at source launcher after 111.09s,
evidence `logs/20261001-065919-3a667e66`. All 133 core/107 runner tests,
six network scenarios and economy/reconnect/settings UI passed. The launcher
still attempted ranged recruitment from a Barracks and expected the old
upgraded-Farm food yield. Its ordinary opening now builds an Archery Range
and spends two real productions, checking each recruitment control at its
own eligible building before spending food. Existing session/music/preferences,
consent, friends and process-exit assertions remain.

The selected launcher retry exposed a sparse frame-sampling false rejection:
`logs/20261001-070821-9186877b/launcher/friends-list-1100x820.png` visibly
contains the complete friends dialog and village, but its 20-by-20 grid sampled
only 11 colours. Capture now samples an 80-by-80 grid, retaining the existing
12-colour minimum and file/dimension checks. This samples narrow text/geometry
even when a modal covers most scenery; it does not waive frame validation.

Selected launcher then passed 35.39s (47.20 including preparation/display),
evidence `logs/20261001-070946-0fdaf1e7`. Both 1100x820 and 1280x720
recruitment/friends/native-exit paths, consent decisions, menu/session audio and
owned preferences, host endpoint release and guest cleanup pass.

## Final full acceptance

`mise run ci` passed its full required set in **220.54s** (221.86s including
entry-point overhead), evidence **`logs/20261001-071039-07264336`** and its
`ci-summary.json`, `ui-source-summary.json`, `ui-package-summary.json`.
Locked restore, format, build and offline source import passed with no tool or
dependency updates. All **133 core tests and 107 runner tests** passed.

| Coverage | Seconds | Result |
| --- | ---: | --- |
| All six network scenarios | 43.54 | Passed |
| Source economy | 21.47 | Passed |
| Source reconnect | 10.31 | Passed |
| Source settings | 7.14 | Passed |
| Source launcher | 36.90 | Passed |
| Source combat | 13.10 | Passed |
| Source UI suite, including display overhead | 92.21 | Passed |
| Sequential Linux client / server exports | 7.91 / 4.91 | Passed |
| Headless exported smoke | 1.09 | Passed |
| Graphical exported-package case | 49.64 | Passed |
| Exported UI suite, including display overhead | 52.93 | Passed |

Packed checkpoints include all eight faction/role rig bindings and ten weapons,
a real rendered Catapult with current-city effects routed through Master,
stockpiles, locomotion and shooting. Existing packed launcher/session/friends
coverage is retained; this adds a brief first-wave presentation slice rather
than another complete graphical match. Outputs remain ignored under `dist/`.
Documentation's three village screenshots were refreshed from this final source
run. All 155 provenance hashes and strict OpenSpec validation pass.

All 37 task outcomes are implemented and verified. The final design records the
measured 30-wood tuning; recruitment scenarios match their unlock buildings.
All six delta capabilities were synced to main specs and the completed change
was archived on 2026-10-01. Local Linux X11/software OpenGL and
Dummy audio evidence does not establish Windows runtime, native GPU/compositor
performance, physical input or listening quality. Real two-account Steam checks
remain separate and unexecuted; no publishing or developer preference changes
were performed.
