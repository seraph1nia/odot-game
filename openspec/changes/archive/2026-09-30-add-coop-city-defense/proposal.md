# Proposal

## Why

The working coin-room scaffold proves basic server authority but discards players on disconnect. A tiny cooperative city-defense match will make ownership, validated economic commands, persistent armies, reconnecting into an ongoing simulation, and whole-match pause the focus of the second multiplayer milestone.

## What Changes

- **BREAKING**: Replace the default coin-room gameplay and its movement/score wire contract with a round-based city-defense match; retain the existing headless Godot server, C# rules separation, ENet transport, development commands, and verification/export pipeline.
- Give each player a square 3x3 board, city health, base gold income each turn, and gold-purchased mines, farms, and barracks. Mines produce gold; farms produce food. Players click a barracks to spend food on a persistent soldier. Include one simple upgrade per building.
- Advance each building turn through a ready check, apply production once, and fight automatically after every three turns. End after three escalating waves: surviving cities win together; losing all cities is defeat.
- Give every city a weak built-in ranged defender outside its nine slots. Soldiers and enemies fight automatically; enemies attack an exposed city. City health reaching zero eliminates its player. Immediately split its living attackers among surviving cities and redistribute its full future wave allocation as well.
- Preserve disconnected cities and armies. Reconnecting to the same running server reclaims a stable player identity and receives the current authoritative match. Connected players can pause/resume the whole match, including combat; connection handling remains active while paused.
- Present a small angled 3D tabletop with planar square boards and simple battle lanes. Use free KayKit Medieval Hexagon assets for buildings/environment, Prototype Bits for missing objects and unit placeholders, and Resource Bits for resource props. The asset palette does not add wood, stone, or iron currencies.
- Extend rules tests and real-process network verification to cover economic authority, round progression, combat redistribution, paused-state consistency, and reconnecting without lost state or duplicate commands.

## Capabilities

### New Capabilities

- `coop-city-match`: Nine-slot economy, manual recruitment, upgrades, three-turn ready/production cycles, automatic combat, persistent troops, elimination, redistribution, and three-wave outcomes.
- `resumable-multiplayer`: Stable match ownership, validated/idempotent commands, authoritative snapshots, disconnection/reconnection, and whole-match pause/resume.
- `city-tabletop`: Readable 3D tabletop presentation, city interactions and match controls, and the requested free KayKit asset palette on a square grid.
- `coop-verification`: Engine-independent rule coverage and bounded real-network verification of the cooperative match using the existing development/CI task interface.

### Modified Capabilities

None: `openspec list --specs --json` currently returns no durable specs. The completed but unarchived `scaffold-csharp-multiplayer` change describes the previous demo; this change explicitly supersedes its coin gameplay, fresh-session reconnect behavior, and coin-specific test expectations. Its tooling and CI guarantees continue. Leave that historical change untouched; reconcile those superseded requirements if it is later promoted to main specs.

## Impact

Implementation will replace the rules in `src/Game.Core/World.cs`, update snapshot/event contracts in `src/Game.Core/Diagnostics.cs`, and refactor `src/Game/Main.cs` plus `src/Game/Scenes/Main.tscn` into a thin network/bootstrap layer and client-only world/UI scenes. Add selected assets under `src/Game/Assets/`, preserving their included licenses and recording provenance. Update `tests/Game.Core.Tests`, `tools/DevRunner/NetworkTests.cs`, session-path support in runner options/orchestration, relevant export smoke checks, README, and verification notes.

No new engine, transport, backend service, paid asset, or game framework is required. Proposed POC defaults are a small pre-match lobby with an explicitly started fixed roster, one soldier/enemy type, one lane per city, no new players after start, and resume credentials scoped to one running server/match. Server restart recovery, accounts, host migration, matchmaking, walls, resource transfers, reinforcements, navigation/physics combat, adjacency bonuses, board folding effects, and additional resource mechanics are deferred.
