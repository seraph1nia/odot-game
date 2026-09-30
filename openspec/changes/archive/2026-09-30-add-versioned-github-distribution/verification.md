# Distribution implementation evidence

## Baseline (2026-09-30)

Reused the full passing `mise run ci` at
`logs/20260930-152422-64802053/ci-summary.json`: 145.92 seconds,
55 core and 51 runner tests, all six network scenarios, all four source UI
slices, ordered Linux exports, native offline probes, headless package smoke
and graphical package smoke. Source revision: `1b08ef2` (`steam dev`).
Implementation/tool/lock inputs have no workspace diff from that revision.
The hosting milestone closure record in `docs/verification.md` confirms no
game code changed after this run. Existing documentation/spec archive edits
are preserved. Environment/tool inputs remain unchanged.

Hosting is now archived at
`openspec/changes/archive/2026-09-30-add-start-screen-and-steam-hosting/`.
Reconciled the distribution verification delta with the synced requirement:
retained observable-state waits, no Steam/accounts/relay/internet dependency
for local gameplay checks, distinct Steam prerequisite/failure reporting and
the missing-Steam-prerequisites scenario. Other hosting requirements remain
untouched. Real-account, relay, cold-launch and release-stability acceptance
remain unverified; local packaging does not complete them.

`openspec validate add-versioned-github-distribution --strict` passed.

## Version policy

`mise run test` passed: 55 core + 84 runner tests. The 33 added cases cover
SemVer precedence, stable/preview transitions, build metadata, arbitrary-length
numeric prerelease identifiers, malformed tags and the supported assembly
component boundary (0..65534). No Godot processes start in these tests.

## Tagged identity and source regression

Added three identity/staging tests; `mise run test` now passes 55 core + 87
runner cases. `dotnet restore Odot.slnx --locked-mode` and
`dotnet format Odot.slnx --no-restore` passed; solution build has zero warnings
or errors. Locks and engine/SDK pins are unchanged.

An owned source snapshot under `.cache/distribution-identity-wyug1d3g/` has its
own temporary Git repository and fixture tags; none were created or pushed in
the real repository. Checked-in `export-client --tag` ran against an exact
clean fixture tag, built/imported/exported its staging copy and verified the
packaged identity through the supervised `--build-info-probe` route.
Final fixture `v0.1.0-beta.1+fixture.3` is commit
`8a8de5d568f78b35d0d7f8629e85590541be244d` and passed in 17.27 seconds.
Its metadata distinguishes SDK 10.0.401 from packaged runtime 10.0.12;
runtime, full version and source commit agree with actual assembly/runtime
and Godot application information. Output and evidence remain ignored.
Detailed command output: `logs/distribution-identity-export-final.log`.
Fixture runner evidence:
`.cache/distribution-identity-wyug1d3g/logs/20260930-161033-1d6f5c9c/`.

Full `mise run ci` passed in 151.74 runner seconds (153.07 mise seconds) at
`logs/20260930-160428-0130162f/`, including the ordinary source development
identity probe, 55 core + 87 runner cases, all six network scenarios, all four
source UI slices, ordered client/server exports, offline native probes and
both package smoke routes. A final repeat after the narrow SDK/runtime
metadata correction passed in **149.21 runner seconds** (150.54 mise seconds)
at `logs/20260930-161230-4a9ea8a5/`, with the same complete coverage. Command
output is `logs/distribution-identity-final-ci.log`. Reuse this successful
baseline on resumption if source/environment inputs remain unchanged.

## Windows dependency inventory — initial Linux-only assessment

Retained the three Windows x86_64 native DLLs from the existing assessed
archive after validating its SHA-256. Manifest and descriptor now include
their exact hashes/paths; engine, SDK, extension pin and existing Linux
payloads remain unchanged. PE inspection confirms x86_64 and imports only
Windows system DLLs plus the retained `steam_api64.dll`. The pinned stock
template archive contains Windows x86_64 .NET debug/release executables.

Native Windows loading and installer/runtime acceptance remain unexecuted.
This session is Linux x86_64, with no native Windows environment attached;
Wine does not satisfy the planned native Windows checks. Task 2.1 is left
unchecked pending that evidence. Requested guidance on ordering the planned
GitHub Windows workflow before this validation, or waiting for a Windows
environment. No prerequisites were installed and no GitHub action, visibility
change, push or publication occurred. The repository is still private as of
the read-only preflight; public visibility is a publication prerequisite,
not a reason to change visibility during implementation.

## Native Windows CI requested by the owner

The owner subsequently requested a Windows CI workflow and validation there,
authorizing a validation branch to resolve the native-host prerequisite above.
Created `ci/windows-distribution-20260930` from the existing source commit in an
ignored isolated worktree, preserving unrelated local documentation/spec edits.
No release tags, publication, visibility change or merge occurred.

Windows support adds `win-x64` alongside `linux-x64` and intentionally extends
the corresponding Debug/ExportRelease locks without dependency version changes.
Both Windows presets use stock checksum-verified Godot .NET templates. The new
`mise run ci-windows` command owns per-process APPDATA/LOCALAPPDATA, validates
native source/export callbacks, complete package/runtime inventory and exported
native hashes, and exercises a bounded headless offline-solo startup with normal
game requests. Exported processes remove SDK discovery and use a system-only
PATH. It does not claim installer, graphical Windows or real Steam acceptance.

Local verification passed 55 core + 89 runner tests, locked restores, formatting,
and the Windows cross-export inventory check at
`logs/20260930-171059-b60bb180/` (13.27 runner seconds). Full local Linux CI passed
in 151.37 runner seconds at `logs/20260930-171301-bc1fa816/`, preserving all
network/source UI/native/package gates. No local tools or OS packages were
installed or upgraded.

On Linux, `mise run ci-windows` correctly exits nonzero and records `unexecuted`
before starting engine processes, identifying native Windows x86_64 as the
missing prerequisite. Evidence: `logs/20260930-173946-395b7ac5/` and
`logs/windows-prerequisite-check.log`.

GitHub's full Linux CI passed for `f673a02efa7edd0a9f41ae92907c0e977f58891e`
in 339.16 runner seconds, including 55 core + 89 runner tests, all six network
and four source UI slices, ordered exports and both package routes:
https://github.com/seraph1nia/odot-game/actions/runs/36750386802
Local retained command log: `logs/windows-validation-linux-ci.log`.

The first native Windows run passed tool pins, upstream manifest hashes, locked
restore, formatting and compilation, then failed on the default 15-second cold
asset-import deadline. This is a failed run, not native package acceptance:
https://github.com/seraph1nia/odot-game/actions/runs/36750386820
The workflow now explicitly allows a bounded 60-second startup/import deadline
on its ephemeral Windows runner.

The retry's Linux source gate failed in the launcher native-close assertion:
https://github.com/seraph1nia/odot-game/actions/runs/36752060837
It reported `Audio mixer did not drain within the shutdown deadline`;
Windows was correctly skipped, not passed. Retained failure log:
`logs/windows-validation-linux-retry-failure.log`.

The owner's subsequent refinement replaces the two workflows with one
`Verify and build` workflow: `source` must succeed before `linux-package` and
`windows-package` run in parallel on independent exact-commit checkouts.
Linux exports remain sequential. Local `ci` reuses the split functions with
one source preparation and retains every original gate. Standalone split
commands identify partial coverage explicitly. No artifact transfer is needed.

The audio failure exposed elapsed-phase sampling: multiple mixer cycles can
occur between rendered frames without a decrease in time-since-last-mix.
Shutdown now observes advancement of the inferred mix timestamp, bounded by
clock samples around the actual driver query. Sampling/rounding uncertainty
cannot count as mixer progress, and the existing one-second bound stays.
The locked Godot implementation uses the same monotonic clock for both APIs:
https://raw.githubusercontent.com/godotengine/godot/4.7.2-stable/servers/audio/audio_server.cpp
https://raw.githubusercontent.com/godotengine/godot/4.7.2-stable/core/os/time.cpp
Three cheap deterministic regressions establish slow-frame progress, a stalled
mixer and sampling-delay uncertainty without adding another expensive scenario.
`mise run test` passed 55 core + 92 runner tests after locked restore/format.

Full local `mise run ci` passed in 153.62 runner seconds (155.03 mise seconds)
at `logs/20260930-175151-c0c3a9b6/`. Every source/network/private-display gate,
ordered Linux export, offline native probe and headless/graphical package check
passed, including launcher shutdown. Retained output:
`logs/parallel-ci-full-linux.log`. No source changes followed that pass.

## Native platform acceptance — Windows tasks 2.1 and 2.2 complete

The revised three-job workflow passed on exact source commit
`4050db5854176aa6083f3fc8e5df48983b984cc4`:
https://github.com/seraph1nia/odot-game/actions/runs/36754528804
All three jobs report success. `source` completed at 18:00:28 UTC; Linux and
Windows packaging started at 18:00:34 and 18:00:32, demonstrating their parallel
execution after the shared source gate. Retained output:
`logs/parallel-ci-github.log`; source-only output also remains at
`logs/parallel-ci-github-source.log`.

Source validation passed 55 core + 92 runner tests, all six network scenarios,
all four source UI slices, source native callback/development identity probes,
locked restore, formatting and strict compilation. The launcher native-close
case passed remotely after the mix-timestamp observation fix.

Native `windows-2025` passed matching engine/SDK pins and upstream hashes,
locked restores and compilation, source/export typed native Steam callbacks
with initialization disabled, complete executable/PCK/managed/runtime/notices
inventory, .NET runtime 10.0.12 configuration, packaged native hashes,
development identity and ordinary offline solo construction/production/recruit
requests. Package processes used owned APPDATA/LOCALAPPDATA and system-only
PATH without SDK discovery variables. `ci-windows` took 164.82 runner seconds;
its cold import took 25.43 seconds, export 44.77 seconds, and offline solo
1.02 seconds. Owned process/state cleanup completed successfully.

Linux packaging passed locked preparation, sequential client/server exports,
offline native/hash probes, headless cooperative package smoke and the private
display graphical package slice. Normal local `ci` retains the same full
Linux source/package checks and prepares source once. No tools/versions changed
after the passing local and remote runs; subsequent edits record evidence only.

## Installer, update and release implementation

The implementation now includes a pinned Inno Setup 6.7.3 definition, a
per-user Windows installer, a versioned Linux archive/install script, owned
install/uninstall verification commands, manual update discovery and an About
tab. The Linux fixture exercises fresh install, identical reinstall, upgrade,
paths with spaces, failed download, bad checksum, atomic current-version
retention, launcher arguments, inherited/native overlay preload and owned
uninstall while preserving external data. Deterministic update fixtures cover
SemVer/channel selection, drafts, incomplete/foreign assets, pagination bounds,
rate limits, response-size bounds, concurrent requests, disposal and captured
browser handoff. Final asset assembly has positive and missing/changed/mismatched
identity fixtures.

After formatting and locked restore, the cheap suites pass **55 core + 105
runner tests**. The selected settings graphical slice passed in 14.49 runner
seconds at `logs/20260930-183338-a843d1da/`, including modal isolation,
development version/update feedback, volume persistence, rendering and cleanup.
Strict OpenSpec validation passes.

The owner requested that the on-release workflow skip all test suites. The final
workflow therefore responds to a manually published empty release, performs
only public/tag/profile preflight, builds Linux and Windows packages in parallel,
then validates the allowlisted hashes and common identity before attaching five
public files. Gameplay, network, UI and Linux installation verification remain
in ordinary CI and explicitly selected tasks; the release workflow does not
rerun them. Ordinary CI runs only for pull requests targeting `main` and pushes
to `main`, retains read-only permissions and has no artifact upload. It retains
parallel Linux and native Windows package jobs after the source gate; Windows
installer qualification remains accepted as deferred.

A nonpublishing validation branch exercises the actual Linux package and install
path. Native Windows installer qualification is explicitly deferred. No real
release tag was pushed, no release was created, no asset was published, and
repository visibility remains private. The owner will make it public before
using the published-release workflow.

The completion `mise run ci` passed in **153.96 runner seconds** (157.41 mise
seconds) at `logs/20260930-184821-46fbe011/`. It covered 55 core + 105 runner
tests, all six network scenarios, all four source UI slices, sequential Linux
exports, native/headless package probes and the exported graphical package
slice. All upstream actions in both workflows are pinned to immutable commit
SHAs with their current release labels; the workflow policy tests reject a
floating `uses:` reference, an upload in ordinary CI or a test-suite command in
the release workflow.

The nonpublishing hosted run at
https://github.com/seraph1nia/odot-game/actions/runs/36763314446 used the pinned
actions. Its source gate passed in 3m13s. The tagged Linux package passed in
64.74s, the ordinary exported-package checks passed in 79.47s, and the actual
installed Linux graphical slice passed in 30.17s; `verify-installed-linux`
completed in 34.35s with owned paths and cleanup. The overall run is red only
because its now-deferred Windows diagnostic also ran on the earlier validation
revision.

The Windows runner successfully downloaded and checksum-verified Inno Setup
6.7.3, exported the tagged Windows client and verified its build identity. The
compiler invocation then failed by showing ISCC usage, so no installer was
created and no install/upgrade/uninstall claim is made. The owner accepted this
as deferred work while retaining the native Windows export/build job in ordinary
CI. Tasks 2.3, 2.4, 5.4 and the Windows portion of 7.2 remain unchecked. Release
workflow work continues separately.
