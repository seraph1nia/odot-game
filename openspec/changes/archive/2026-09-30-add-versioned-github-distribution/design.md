# Design

## Context

See `proposal.md` for motivation. The owner confirmed Windows/Linux clients, installers, manual full-download updates, continued Steam multiplayer, and making `seraph1nia/odot-game` public themselves.

The current `Exports.cs` supports only Linux x86_64, downloads checksum-pinned .NET templates, exports complete client/server directories, and checks native dependencies. `Ci()` prepares source once, gates sequential exports on rules/network/source UI, then checks exported roles and presentation. `.github/workflows/ci.yml` runs that task with read-only permissions and no uploads. `Game.csproj` and `Game.Core.csproj` declare only `linux-x64`; locks distinguish Debug and ExportRelease. The pinned Steam extension retains only Linux binaries. The Linux developer runner prepares the overlay before Godot creates graphics, but directly running the exported executable does not inherit that helper.

`ClientSettings` supplies one application-owned modal with Graphics/Audio controls and stores preferences under `user://settings.cfg`. There is no release identity or updater. Runner tests already link an engine-independent game source file, allowing release-policy testing without starting Godot.

The `add-start-screen-and-steam-hosting` local milestone was archived on 2026-09-30 after the owner explicitly deferred friend testing. Its Steam dependency qualification and real remote/invitation/relay/cold-launch acceptance remain unverified in the archived audit and verification record. This change extends its implemented packaging and settings surfaces. The two modified verification deltas clarify publication scope; reconcile the distribution delta with the hosting requirements now synced to main, preserving source/package gates, observable-state waits and separate Steam prerequisite/failure reporting.

## Goals / Non-Goals

**Goals:** Keep the playable Godot export as the payload; add a small installation layer; make every asset attributable to one tag and commit; keep installation usable without developer tools; preserve player data across upgrades.

**Non-Goals:** An embedded self-updater, differential patches, background polling, a custom launcher service, new online transport, Steam store/depot publishing, dedicated-server distribution, macOS/ARM builds, or changes to game balance. Signing certificates and Steam application registration are external prerequisites, not automatically provisioned resources.

## Decisions

### 1. One release identity derived from a validated tag

Use `vMAJOR.MINOR.PATCH[-prerelease][+metadata]`; validate it in a reusable engine-independent policy helper before any mutation. Store that helper with game distribution support, outside numerical `Game.Core`, and link it into runner/test projects as needed using the established source-link pattern. Avoid a new updater framework or mismatched parsing in shell/C#.

Carry full SemVer, source SHA, update repository, channel, target, engine/runtime versions, and Steam packaging mode in generated build metadata. Stamp full version into Godot application version, assembly informational version and installer display information. Numeric Windows file/assembly versions use `MAJOR.MINOR.PATCH.0`; reject release numeric components exceeding the representable platform limits rather than truncating. SemVer metadata does not change update precedence. Use a development fallback with disabled release comparisons for ordinary source/untagged exports. Do not version the Godot application name or user-data directory. Do not equate package SemVer with `WireJson.ProtocolVersion`.

Generate/stamp metadata in owned build output or a staging project copy; preserve tracked configuration and locks. Linux source verification and final exports receive the same version inputs. Build and export mutations remain ordered within their workspace.

### 2. Explicit preview and production packages

Prerelease tags default to preview/test packages with AppID 480 and release notes that explain both friends must launch the game first. Allow an explicit own AppID when testing that configuration. Non-prerelease tags select production packaging and retain the existing own non-480 AppID validation and exclusion of development AppID files. Stable/production qualification remains subject to the existing Steam acceptance requirements; a version string is not evidence of completed acceptance. The first shareable build can therefore be `v0.1.0-beta.1` without registering a production AppID.

Add Windows x86_64 export presets and the corresponding Steam libraries from the assessed upstream release archive; record retained file hashes, licensing and descriptor entries. If the assessed archive lacks a compatible Windows build, report that concrete dependency blocker rather than silently choosing a new release. Extend explicit RuntimeIdentifiers to `win-x64` and intentionally regenerate the affected configuration locks with both declared targets, then resume locked restores. Pin the exact Inno Setup compiler version/checksum during implementation. Keep engine/SDK versions unchanged.

Prefer native Windows packaging on a Windows GitHub runner: this enables real installer installation/uninstallation checks and avoids relying on Wine. Linux keeps the existing graphics harness.

### 3. Windows Inno Setup, Linux archive and installer script

Public assets for version `V`:

```text
odot-V-windows-x64-setup.exe
odot-V-linux-x64.tar.gz
odot-V-linux-x64-install.sh
SHA256SUMS
build-info.json
```

Inno Setup uses `PrivilegesRequired=lowest`, a fixed installation AppId (distinct from Steam AppID), and `{localappdata}/Programs/Odot`. Keep the complete payload in an owned `game` subdirectory; shortcuts point into it. The upgrade closes or requests closure of this application, removes obsolete owned payload files, and installs the full new payload. Never delete the parent directory indiscriminately or touch Godot player data. Add a Start Menu shortcut and registered uninstaller; omit automatic launch during silent verification. Inno Setup was selected over NSIS/custom PowerShell because it already owns the ordinary install/shortcut/uninstall workflow.

Linux uses a POSIX-shell install script generated per release. Bake in the archive's exact GitHub URL and SHA-256 after assembling the archive; compute `SHA256SUMS` for all final assets afterward. The user downloads the script and runs `sh odot-V-linux-x64-install.sh`. Check `uname`, `curl`, `tar` and `sha256sum`, and report missing prerequisites without installing them. Use an owned temporary directory, bounded download, verified checksum and safe extraction that rejects absolute/escaping archive paths. Quote paths and support spaces in user directories.

Install under `${XDG_DATA_HOME:-$HOME/.local/share}/odot/versions/V`, with a stable `current` symlink. Create a launcher under `$HOME/.local/bin` and a desktop entry under the XDG applications directory. Support explicit install-root, launcher-directory and application-entry-directory overrides for owned verification without reassigning HOME or changing the developer's normal installation. Stage the complete new version and atomically replace the owned current link; reject unexpected ownership/path shapes. Serialize concurrent installer invocations with an owned lock. Retain the previously installed version until upgrade success; obsolete inactive versions can be removed by a later install after proving no running client uses them. Require the game to be closed before an ordinary upgrade, and never modify an active version's native/managed files in place. Install an `odot-uninstall` command that removes owned installation files, launcher and desktop entry, retaining Godot settings/sessions. The script handles identical-version reinstall as an idempotent operation and does not accept a corrupt existing directory as a completed install.

The Linux launcher follows the existing `SteamOverlayLaunch` policy: preserve a valid existing preload, otherwise find a native 64-bit Steam overlay in supported client locations, append it before starting graphics, and forward all arguments with `exec`. Honor `ODOT_STEAM_DISABLED`; absence of Steam must still launch the game. Test its behavior with fixtures, without opening Steam's invitation UI. Flatpak-specific Steam integration is outside the currently supported native-client setup and must be documented accurately.

An AppImage was considered but adds another build/runtime layer and does not itself supply update discovery. The install script supplies the requested desktop entry and repeatable installation directly.

### 4. Manual update discovery and a browser handoff

Add an About section below the existing settings tabs rather than changing their order; retain current selectors and modal behavior. Show version, Check for updates, status, and an initially hidden Download update action. Keep the dialog usable at supported window sizes. Requests belong to the application/settings lifetime and are asynchronous, bounded and cancelled on application disposal; closing and reopening settings must not start another request or accept a stale generation's result.

Query the public Releases list for `seraph1nia/odot-game` using Godot HTTP support with appropriate GitHub headers. Do not bundle credentials. Stable installations consider stable candidates; preview installations consider preview and stable candidates. Select the highest eligible SemVer newer than the installed version, including proper numeric prerelease precedence. Validate tag/prerelease consistency, repository-scoped HTTPS URLs and the required asset names. Use bounded pagination (100 releases/page, at most five pages, bounded total response size/time); if pagination cannot be completed within those limits, report could-not-check rather than claim current status. Do not use `/releases/latest` for preview selection: it excludes prereleases and does not implement our comparison policy.

On Windows, Download update opens the chosen installer's `browser_download_url`; on Linux it opens the version-specific release page so the user can download/run its install script. Display concise instructions to close the game before installation. The game does not download or execute replacement code and does not leave a hosted session automatically. Use `OS.ShellOpen` only after validating the configured GitHub route. Treat browser-open failures as recoverable. No automatic launch-time checks or preview opt-in toggle are needed: channel follows the installed build.

Velopack was considered for integrated installation/restart, but would require another runtime dependency and proof of Godot executable startup compatibility. It is unnecessary for the accepted browser-assisted workflow.

### 5. Release workflow separate from ordinary verification

Add `.github/workflows/release.yml` for version-tag pushes and manual retries selecting an existing tag. Resolve tag to commit before checkout, validate version/profile, and verify repository public visibility without changing it. Use concurrency per version to prevent competing publications. Ordinary push/PR workflow and `mise run ci` retain read-only permissions and no uploads.

Reuse/refactor the C# verification gate for a local release-build command that has no upload behavior: Linux restores/builds/imports once, runs the existing complete source gates, exports the selected release client and verification server sequentially, and tests final packages. Preserve normal CI's default export paths/profile. Ordinary GitHub verification runs only for pull requests targeting `main` and pushes to `main`, with a shared Linux source-validation job followed by independent Linux and native Windows package jobs in parallel. Linux client/server exports remain sequential within their workspace. Native Windows installer qualification remains deferred even though ordinary CI retains its export/build checks. Local `mise run ci` still prepares source once and runs the complete Linux gate sequentially; split commands report their partial coverage honestly.

Transfer only allowlisted distributables and public metadata to the publication job, verifying hashes and common tag/commit/profile there. Keep logs/screenshots/session data local. Build jobs have read-only repository permissions; only the final publication job receives `contents: write`. The owner manually publishes an empty release for an existing tag; that `published` event runs a lightweight preflight followed by parallel platform builds. Per the owner's final workflow preference, it does not rerun gameplay, UI, network, or installation suites; normal CI and selected package-validation tasks own those checks. GitHub CLI attaches the built bytes after identity/checksum consistency checks. This means the release is visible without binaries while builds run. Existing assets are refused rather than overwritten, including on a retry; a partially uploaded release must be repaired deliberately or superseded by a new version. Public release notes enumerate install/update instructions, supported platforms and Steam test qualification without private session details.

### 6. Checks proportional to the new boundaries

Cheap runner tests cover SemVer/channel ordering, invalid metadata/URLs, asset selection, failure states and publication gate decisions. Link engine-independent update policy into tests; fixtures avoid dependence on live GitHub or accounts. Extend the existing settings UI slice for the About/check control, modal behavior and one displayed result using an owned deterministic HTTP fixture. Capture browser handoff with an owned test opener rather than launching a real browser. Estimated incremental cost: one request/control checkpoint within the existing slice, not a new battle or scenario matrix.

Add independently selectable installed-package checks through the C# harness. Linux exercises install/upgrade/checksum failure/removal using explicit owned install-path overrides and temporary XDG player data, then reuses the exported-package graphical route against the installed payload. Windows executes its real installer silently against an owned path on an ephemeral CI user profile or dedicated test VM, checks shortcut/uninstaller/version/runtime/native files, launches a bounded headless solo/native-load check, upgrades and uninstalls. A local Windows invocation without an isolated profile/VM reports missing prerequisites rather than modifying the developer's installed game, Start Menu or uninstall registry. Use deterministic payload fixtures for installer failure/stale-file cases instead of exporting multiple complete games. Estimated additional setup: native Windows job/compiler plus one installed Linux graphical startup; target each installed slice at under a minute excluding build/export. Record measured timings and risk descriptions when implemented. This catches missing native/runtime files, broken shortcut targets and data loss which source/unit tests cannot establish.

Real Windows graphical behavior and paired Steam invitations/relay/cold launch remain separate observations; missing prerequisites are recorded as unexecuted, not passed. Existing real Steam acceptance is preserved, not duplicated into ordinary CI. Use the repository's full-CI baseline/final rule at the substantial implementation boundary; planning alone needs OpenSpec consistency validation, not a game run.

## Risks / Trade-offs

- Public releases expose repository source as well as binaries -> the owner explicitly chose public visibility and changes it themselves; workflow preflight checks it.
- Windows extension/template or lock support may fail -> validate native exports early and stop on the exact dependency/platform prerequisite.
- Unsigned Windows installers can show reputation warnings -> document this honestly; optional signing is a later external setup, not a guarantee of warning-free execution.
- GitHub limits/outages can block discovery -> manual checks, bounded requests, recoverable feedback, and existing installed play remain independent.
- Native Linux Steam overlay paths vary -> reuse current supported locations, preserve injected preload, and report unsupported client setups without breaking solo.
- Test AppID 480 cannot register Odot for real Steam cold launch -> preview notes require players to start first; production keeps own-AppID and existing qualification prerequisites.
- Overlapping in-flight verification specs can drop requirements when archived -> reconcile this change after the hosting change's spec sync, preserving both sets of scenarios.

## Migration Plan

Implement and verify local packaging/update controls before enabling publication. Update repository instructions to distinguish ordinary CI, local release preparation and the owner's explicit GitHub release publication. The owner makes the repository public before the first tagged release. Exercise a preview package and the workflow's package commands in a nonpublishing validation branch; creating the real tag and publishing the release remain subsequent owner actions. No preferences or resume schema migration is needed. Roll back a faulty release by publishing a fixed newer version; do not replace an already published version's bytes. Manual reinstalling an older package remains possible but is not offered as an update and does not imply gameplay/session backward compatibility.

Primary implementation references: [Inno Setup installation privileges](https://jrsoftware.org/ishelp/topic_setup_privilegesrequired.htm), [stable installation identity](https://jrsoftware.org/ishelp/topic_setup_appid.htm), [GitHub release API and prerelease behavior](https://docs.github.com/en/rest/releases/releases), [GitHub token permissions](https://docs.github.com/en/actions/concepts/security/github_token), [Godot HTTPRequest](https://docs.godotengine.org/en/stable/classes/class_httprequest.html), and [desktop URL opening](https://docs.godotengine.org/en/stable/classes/class_os.html#class-os-method-shell-open).
