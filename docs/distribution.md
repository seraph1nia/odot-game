# Versioned desktop distribution

Distribution targets Windows x86_64 and Linux x86_64. The installer, update
controls and release workflow are being implemented in
`add-versioned-github-distribution`; the tagged export command below prepares
an attributed Linux payload only. It does not constitute a verified release.

## Versions and tagged payloads

Tags use `vMAJOR.MINOR.PATCH[-prerelease][+metadata]`, for example
`v0.1.0-beta.1` or `v1.0.0`. Numeric components must be 0..65534 so the same
version can be represented in assembly and Windows metadata. Leading zeroes
are invalid in numeric components and numeric prerelease identifiers.
Version text is limited to 200 characters. Build metadata is retained in the
displayed identity but does not affect update precedence.
The component limit follows [assembly metadata requirements](https://learn.microsoft.com/dotnet/api/system.reflection.assemblyversionattribute).

A prerelease tag defaults to preview/test packaging with Steam AppID **480**.
Friends must start the game themselves before accepting an invitation: 480
does not register Odot's executable for genuine Steam cold launch. Preview
does not mean real invitations, remote transport or relays have been verified.
Those acceptance checks remain separately recorded in the hosting milestone.

A stable tag selects production packaging and requires the game's own positive,
non-480 AppID. A preview can explicitly use production packaging and its own
AppID when testing that setup. Registering an AppID and qualifying Steam
release stability remain external prerequisites. The currently pinned
GodotSteam version is publisher-qualified as unstable, as recorded in its
provenance; a SemVer tag alone does not qualify it for production.

From a clean checkout of an **existing** tag:

```sh
mise run export-client --tag v0.1.0-beta.1
mise run export-client --tag v1.0.0 --steam-app-id YOUR_OWN_NUMERIC_APP_ID
```

The second command needs a real numeric AppID in place of the placeholder.
These commands create no tag and perform no upload. They reject an invalid
version, dirty checkout, tag/source mismatch or existing local output before
exporting. Output goes to `dist/releases/VERSION/linux-x64/`; delete that owned
local output explicitly if a rebuild is intended. Ordinary untagged exports
keep their existing paths and identify themselves as development builds.

Windows desktop/production exports use `--target windows-x64` (Linux remains
the default). Untagged cross-exports can be prepared on Linux for inventory
inspection. Tagged Windows exports run their actual packaged identity probe
and therefore require native Windows. Neither route creates an installer yet.

## Native Windows validation

`mise run ci-windows` requires Windows x86_64 and the same locked SDK/engine.
It builds/imports source, checks source and exported Steam native classes and
typed C# callbacks with Steam initialization disabled, exports the full desktop
client, checks PCK/runtime/assembly/notices, and runs a bounded offline solo
startup with ordinary build/production/recruitment requests. Package processes
use owned APPDATA/LOCALAPPDATA and a system-only PATH with SDK discovery
environment removed. No preferences, Steam login or real invitation UI is used.
Output stays in `dist/windows-client/`; local evidence stays in ignored logs.

The `Verify Windows client` workflow runs on branches matching
`ci/windows-distribution-*`, and can be manually retried after a push. It waits
for the existing `Verify and build` Linux workflow for the identical commit
to pass before using a native `windows-2025` runner. Build permissions are
read-only; there are no artifact upload, publication or deployment steps.
Manual discovery of a newly added workflow requires its definition on the
default branch; its branch-push trigger bootstraps validation before merging.

Risk admission: one Windows offline-solo startup catches missing native/runtime
or packed files that Linux cross-export inventory and unit tests miss. It reuses
the C# child/scope/evidence harness and normal game requests, with owned storage
and bounded cleanup; it adds no rendered battle or option matrix. Expected
execution is under a minute after preparation/export, with native Windows tool
acquisition confined to an ephemeral CI runner. Native Windows graphical/input
behavior, installer upgrade/uninstall/data retention and real paired Steam
acceptance remain separate unchecked requirements.

Tagged preparation reads the exact Git source snapshot, stamps an owned staging
copy, restores locked dependencies and exports with the locked engine/templates.
The full version agrees across Godot application settings, assembly information
and `build-info.json`. Metadata also carries the full source commit, target,
channel, engine/SDK/runtime pins, repository and Steam packaging identity. Numeric
assembly/file versions use `MAJOR.MINOR.PATCH.0`. Application name, user-data
location and multiplayer wire-protocol version stay independent of releases.

## Public releases

Public downloads will live in GitHub Releases in `seraph1nia/odot-game`.
The owner must make the repository public before publication; implementation
and verification do not change its visibility. Ordinary push/PR CI and
`mise run ci` do not upload or publish. Creating/pushing a release tag and
publishing an actual release are separate requested actions.
