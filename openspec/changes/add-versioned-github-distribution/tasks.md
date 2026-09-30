# Tasks

This checklist implements the confirmed public-repository distribution design. No repository visibility change, tag push, or actual publication is authorized by creating this plan. Keep pending real-Steam acceptance in `add-start-screen-and-steam-hosting` separate from local package verification. Tests and documentation land with each implementation group; missing local tools/OS prerequisites are reported rather than installed.

## 1. Baseline and release identity

- [x] 1.1 Record or reuse a successful full `mise run ci` baseline for unchanged source/environment, and reconcile the overlapping hosting-change verification delta without discarding its scenarios; verify the baseline evidence and the resulting planning/spec consistency before implementation.
- [x] 1.2 Implement engine-independent SemVer parsing/comparison, supported numeric metadata limits and stable/preview selection outside numerical `Game.Core`; link it into runner tests as appropriate and verify cheap cases for 0.9/0.10 ordering, beta.2/beta.10, preview-to-stable, build metadata, malformed versions and unsupported numeric ranges with `mise run test`.
- [x] 1.3 Add generated release metadata and version stamping for tagged exports while preserving development fallbacks, tracked config, locks, application user-data identity and protocol version; verify a tagged staging build's Godot/assembly/build metadata agrees on version/commit and ordinary source launch remains a development build.
- [x] 1.4 Document tag syntax, version identity, preview-versus-production packaging and external own-AppID/public-repository prerequisites in distribution documentation; verify the examples match the implemented validation and do not imply that AppID 480 proves Steam cold launch.

## 2. Windows export and installer

- [x] 2.1 Retain checksum-verified Windows x86_64 native Steam files from the assessed upstream release and extend descriptor/provenance/licenses; verify their manifest hashes and stock .NET template/native-load compatibility, reporting a concrete blocker if that release has no compatible Windows payload.
- [x] 2.2 Add Windows desktop/production presets and declared `win-x64` runtimes for game/core, intentionally regenerate affected Debug/ExportRelease locks, and preserve Linux exports; verify locked restores for both targets and complete Windows export contents without an engine/SDK version update.
- [ ] 2.3 Declare and pin Inno Setup's compiler version and acquisition checksum for Windows builds with a clear local prerequisite check; verify the pinned compiler can build the checked-in installer definition and ordinary tasks do not install or upgrade local tools.
- [ ] 2.4 Implement the per-user installer, stable installation identity, Start Menu entry, running-game handling, owned payload cleanup and uninstaller; verify install/upgrade/uninstall behavior using the actual installer in an ephemeral Windows test profile, including removal of a stale managed payload file and preservation of player data.
- [ ] 2.5 Document Windows install/upgrade/removal, supported OS prerequisites, Steam access and unsigned-installer limitations; verify the instructions identify the correct executable and do not require Godot/.NET developer tools.

## 3. Linux archive, installation and launch

- [ ] 3.1 Package the complete Linux client with correct modes/notices and generate a version-specific install script with the fixed archive URL/hash; verify archive inventory, permissions, baked checksum and final asset hashes against a tagged fixture.
- [ ] 3.2 Implement tool/architecture preflight, owned staging, checksum validation, path-safe extraction, serialized installation and atomic version activation; verify an isolated script test covers a fresh install, identical-version reinstall, full upgrade, paths with spaces, invalid hash and failed download without damaging the previous installation.
- [ ] 3.3 Add the stable launcher, desktop entry, installation-path overrides and owned uninstall command; verify they target the installed version, preserve settings/sessions, remove only owned paths, and permit verification without reassigning HOME or touching the developer's normal installation.
- [ ] 3.4 Carry the existing native Linux Steam overlay policy into the packaged launcher while honoring disabled Steam, inherited preload and forwarded arguments; verify fixture coverage for available/missing overlay and run an installed offline solo launch without opening real Steam UI.
- [ ] 3.5 Document the download-and-run install script, full upgrades, uninstalling, native Steam client expectations and supported Linux graphics prerequisites; verify example asset names and commands match generated scripts and missing prerequisites are not silently installed.

## 4. Settings and manual updates

- [ ] 4.1 Add engine-independent GitHub release parsing, eligible asset/URL validation, bounded pagination and channel/version selection; verify runner tests cover drafts, invalid tags, missing assets, foreign URLs, equal/older versions, preview-to-stable and pagination-limit/error outcomes using deterministic fixtures.
- [ ] 4.2 Add an About section to the shared settings dialog showing build version, manual Check for updates and recoverable status; verify the existing settings slice still enforces modality, preference persistence and accessible controls at supported sizes while development builds identify themselves accurately.
- [ ] 4.3 Implement asynchronous bounded HTTP discovery with one active request and application-lifetime cancellation; verify owned HTTP fixtures cover success, offline/error/rate-limit feedback, repeated clicks and disposal without requiring live GitHub or Steam accounts.
- [ ] 4.4 Add the validated browser handoff to the exact Windows installer or Linux release page with close-and-install instructions; verify a captured test opener receives the expected version-specific URL, opening failure is recoverable, and the running session is neither exited nor modified.
- [ ] 4.5 Document stable/preview update behavior, manual full downloads and offline checks; verify the instructions match the installed channel policy and accurately distinguish checking/downloading from installation.

## 5. Release preparation and installed-package verification

- [ ] 5.1 Refactor/reuse the existing verification gate for a non-publishing local release-build task with tagged version/platform/profile inputs; verify ordinary `mise run ci` retains its full coverage/default exports, Linux release preparation restores/builds/imports once, and source failure prevents release exports.
- [ ] 5.2 Register `installed-linux` and `installed-windows` slices through the C# scenario/child harness and a checked-in install-verification task; verify each is independently selectable, uses existing packages without implicit rebuild, and documents its missing-runtime/shortcut/data-loss risk plus expected setup/runtime cost.
- [ ] 5.3 Run Linux installation checks in owned install paths/XDG state and reuse exported-package graphical assertions against the installed executable; verify packed resources, normal input, rendered evidence and cleanup, with failed checksum/upgrade cases established by cheap deterministic payload fixtures.
- [ ] 5.4 Run the actual Windows installer in an ephemeral CI user profile or dedicated VM, verify shortcuts/uninstaller, retained data, native/runtime hashes and a bounded headless solo/native-load launch, then uninstall; verify absence of the isolated-profile prerequisite is unexecuted with nonzero exit and never mutates the developer's normal registry/Start Menu.
- [ ] 5.5 Document local release preparation and selected installation commands in README/verification docs, and update the no-publication guidance to distinguish the separate release workflow; verify examples are reproducible and records distinguish Windows graphical/real-Steam checks from the automated coverage actually executed.

## 6. GitHub release workflow

- [ ] 6.1 Add a manually published-release trigger, public-visibility/version/profile preflight, exact tagged checkout and per-version concurrency; verify the preflight rejects invalid tags, private visibility and missing production AppID without creating a release or changing repository visibility.
- [ ] 6.2 Wire lightweight preflight before parallel native Linux and Windows package builds using declared locked tools and checked-in commands; verify the release workflow skips gameplay/UI/network/installation suites as requested, both builds use the same tagged commit, build/identity failures prevent attachment, and jobs never mutate common output concurrently.
- [ ] 6.3 Transfer only allowlisted built packages/public metadata, assemble final SHA256SUMS/build-info and verify shared source commit/version/profile; verify negative fixtures reject a missing asset, mismatched identity or changed hash and no logs/screenshots/player data enter the transfer set.
- [ ] 6.4 Add a least-privilege GitHub CLI stage that attaches the complete built asset set to the owner's existing published release, requires its preview flag to match the tag and refuses replacing existing assets; verify publication decisions with preflight/fixture checks and a nonpublishing branch run rather than publishing during implementation tests.
- [ ] 6.5 Document repository-public prerequisite, release invocation/retry, preview friend launch instructions and production qualification in the release guide; verify ordinary push/PR workflow and `mise run ci` still have no upload/publication steps, and the guide explains that actual publication is a separate requested action.

## 7. Integration acceptance

- [ ] 7.1 Format changed C# with `dotnet format Odot.slnx --no-restore` after the intentional locked restore and run full `mise run ci` once at completion; verify recorded results preserve every cooperative/network/source UI/package gate and unchanged successful slices are not redundantly repeated.
- [ ] 7.2 Prepare a complete preview release set from one source revision and run both installed-package slices plus the non-publishing release workflow path; verify all final asset hashes/versions, player-data retention, update candidate selection and both platform prerequisite records agree, leaving unsupported Windows graphics and real Steam acceptance explicitly unexecuted where applicable.
- [ ] 7.3 Validate the final OpenSpec change and reconcile overlapping verification requirements with the hosting change's current synced state; verify `openspec validate add-versioned-github-distribution --strict` passes and no real-Steam gate is marked complete solely from local packaging evidence.
