# Proposal

## Why

Friends need installable Windows and Linux clients without setting up Godot, .NET, or development tools. Versioned GitHub Releases and a simple update check will make sharing and upgrading the game repeatable.

## What Changes

- Build Windows x86_64 and Linux x86_64 clients from validated SemVer tags using the locked Godot .NET engine, SDK, templates, and dependencies.
- Publish a Windows Inno Setup installer, a Linux archive and install script, checksums, and build metadata to Releases in `seraph1nia/odot-game`. The owner will make this repository public; this change does not change repository visibility.
- Provide installation without administrator privileges, application shortcuts, repeatable full-package upgrades, and uninstalling that preserves preferences and private session files.
- Add version information and a manual Check for updates action in the shared settings menu. Download update opens the platform's version-specific download in a browser; players run the installer or script themselves.
- Support stable and preview versions without silently moving stable installations onto previews. Friends-only preview builds can retain development AppID 480; production releases retain the own-AppID requirement.
- Preserve Steam multiplayer, offline solo, and developer ENet roles. Add the pinned Windows Steam binaries and package a Linux launcher that handles overlay initialization before graphics startup.
- Introduce an explicitly triggered release workflow with publication after verification and package checks; ordinary push/PR verification and `mise run ci` remain non-publishing.

## Capabilities

### New Capabilities

- `versioned-distribution`: Tagged client builds, version identity, platform packages, release gates, and GitHub publication.
- `desktop-installation`: Per-user Windows/Linux installation, shortcuts, full-package upgrades, and removal.
- `client-updates`: Displayed build version, manual release discovery, SemVer/channel selection, and browser-assisted downloads.

### Modified Capabilities

- `coop-verification`: Explicitly distinguish non-publishing verification from the separately gated release workflow while retaining lifecycle, source, package, and Steam acceptance obligations.
- `linux-test-execution`: Preserve local evidence and ordinary CI's no-upload policy while allowing distributable packages through the separate release workflow.

## Impact

Changes affect `src/Game` settings/application presentation, export presets and version metadata; game/core runtime identifiers and intentionally regenerated NuGet locks; pinned Steam extension files/provenance; `tools/DevRunner` packaging and verification; installer resources under `tools`; mise tasks/tool declarations; `.github/workflows`; and distribution/verification documentation. Inno Setup adds a pinned build-time tool, not a player prerequisite. Pure release/update policy tests belong with runner tests, leaving numerical gameplay rules unchanged. This builds on the locally verified, archived `add-start-screen-and-steam-hosting` milestone and must preserve its implementation and deferred real-Steam acceptance record.
