# Proposal

## Why

Odot was a provisional name, and the selected title is **The Common Watch**. The game's identity is cooperative settlement planning and shared automatic defense; its public name and introductory copy should make that promise clear without changing the game.

## What Changes

- Present The Common Watch in the graphical window, start/multiplayer menu, friend instructions, update messages, desktop-entry display name and installer display text.
- Introduce concise cooperative-defense positioning: build and provision individual villages, ready together, and survive automatic waves with a shared outcome. Retain solo as a supported experience.
- Update current player-facing README, gameplay and distribution descriptions, including a clear legacy-identity compatibility note.
- Keep the existing medieval countryside, text-first Trio UI, assets, controls and numerical rules. Fit the longer title at both supported reference sizes.
- Preserve legacy technical identifiers and paths: Godot data identity, solution/namespaces, executables, archives, installer GUID/location, launcher/desktop filenames, Windows shortcut/group paths, repository/update source, Steam identity, environment variables and protocol/credentials. No migration or separate release is required.
- Extend existing lightweight and launcher/package coverage rather than introduce another expensive scenario.

## Capabilities

### New Capabilities

- `game-branding`: Consistent public title and cooperative-defense positioning, independent of stable technical identity.

### Modified Capabilities

None. Existing launcher, desktop-installation and themed-ui contracts continue to apply unchanged; this adds a public-identity contract rather than replacing their behavioral requirements.

## Impact

Presentation in `src/Game/GameApplication.cs`, friend and update copy, packaging display strings in `tools/Distribution/Odot.iss` and `tools/DevRunner/ReleasePackaging.cs`, relevant documentation and existing tests/observations. `project.godot` retains its legacy `config/name` for user-data compatibility; a graphical window-title override supplies the public name. No dependencies, asset provenance, rules, networking formats, production Steam operations or release versions change.

## Non-goals

No logo, new art/music, invented lore, broad visual redesign, gameplay rebalance, camera change, repository rename, namespace sweep, identifier/data migration, external name-clearance claim, release publication or competing version bump. Historical OpenSpec changes and asset provenance are not retrospectively renamed.
