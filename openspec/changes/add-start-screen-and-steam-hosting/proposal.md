# Proposal

## Why

The desktop game currently connects immediately to a separately launched server, leaving players without a normal start screen or a self-contained solo/hosting flow. A shared authority model with Steam invitations will make one-player and cooperative play accessible while preserving repeatable local development and automated verification.

## What Changes

- **BREAKING**: A normal graphical launch opens a medieval start screen with Single player, Multiplayer, Settings, and Exit Game instead of automatically connecting to loopback. Explicit server/client development arguments retain their existing purpose.
- Run single player as one local authoritative match inside the graphical process, using the existing rules and validation without a socket, external server process, or Steam dependency.
- Add a playing host that owns the complete cooperative simulation; host-local and guest actions use the same validation, retry protection, and snapshot behavior. Hosted lobbies have up to four players and the host starts the match.
- Add private Steam lobbies, the standard Steam invite overlay, invitation handling before/after launch, and Steam relay connectivity without manual port forwarding. A guest can reconnect to the same running host; host exit ends the session without migration.
- Reuse the settings/music work from `add-background-music-and-settings` and give it application lifetime across menus, lobbies, and matches. Add Return to menu and proper session/application shutdown.
- Prefer stable, actively maintained Godot/Steam SDKs and built-in APIs. Exclude beta C# bindings, paused/abandoned peer adapters, engine forks, and custom NAT/relay/reliability code. Prove the supported integration in an early compatibility phase.
- Preserve ENet for localhost development and CI. Extend the runner to launch a playing host plus guests; verify core/session rules, real process lifecycle, graphical controls, and a separate real-Steam export scenario.
- Deliver everything as one OpenSpec change with seven ordered phases and explicit acceptance gates; do not treat a local substitute or skipped Steam check as completed Steam support.

## Capabilities

### New Capabilities

- `game-launcher`: Start screen, self-contained single player, shared settings/music access, menu/session transitions, and application exit.
- `steam-sessions`: Private hosted lobbies, native invites, authenticated Steam connections through relays, reconnect admission, and stable maintained integration dependencies.

### Modified Capabilities

- `resumable-multiplayer`: Authority inside a playing host or local match, equivalent command validation, identity binding, and explicit host-loss behavior.
- `coop-city-match`: One-player local matches and host-controlled start for hosted lobbies while retaining nine-slot economy/combat rules.
- `city-tabletop`: Mode-aware lobby/control feedback and presentation that starts only for a selected session while preserving direct world interaction and headless exports.
- `coop-verification`: Playing-host process coverage, Steam-independent development/CI, repeatable graphical E2E, separate real-Steam acceptance, and bounded session cleanup.

## Impact

- Refactor session setup and RPC ownership in `src/Game/Main.cs`; retain `Game.Core.Match`, commands, snapshots, and ledgers as the gameplay source of truth.
- Add a persistent graphical application/menu owner and integrate `Tabletop`, `ClientSettings`, existing music, and native Godot controls. The active settings change is an integration prerequisite, not duplicated work; its final preferences and loop contract remain authoritative.
- Add a pinned stable Steam integration, native redistribution/export metadata, application/lobby endpoint storage, and a small adapter around supported engine/SDK APIs. A real AppID or documented development AppID and two Steam test accounts/machines are external verification prerequisites; none is assumed to exist yet.
- Update `tools/DevRunner`, core/session tests, `mise.toml`, export smoke, README/gameplay/verification documents, and graphical test support. Linux x86_64 remains the tested export target; other platforms are not promised by this change.
- No host migration, persistent match saves, public matchmaking, gameplay balance changes, paid assets, cross-store networking, or publishing/deployment.
