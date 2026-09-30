# Proposal

## Why

The current battle loop moves static dummy units toward the same point, so soldiers overlap and the displayed lateral offsets do not participate in combat. Integrating Arch into the existing engine-independent core will support clearer automatic battles with reliable contact, distinct melee and ranged soldiers, and rigged animation driven by authoritative state.

## What Changes

- Keep Godot and the existing solo, playing-host, dedicated-server, ENet and GodotSteam session paths; introduce pinned Arch ECS for soldier/enemy simulation in `Game.Core`.
- Keep one bounded battle approach per city, adding authoritative formation positions, body spacing, deterministic targeting, attack-range stopping and recovery after a target dies or an enemy transfers. Support battles with tens of units without manual control.
- Add explicitly recruitable swordsmen and crossbowmen, with separate validated profiles for health, damage, range and attack timing. Preserve food-only recruitment, barracks upgrades and surviving armies across waves.
- Replace combat dummies with vendored free rigged KayKit characters and idle, locomotion, sword attack, crossbow shooting, hit and death clips. Build the presentation bindings in C#; show facing, health and faction clearly and retain a dying visual briefly after authoritative removal.
- Add snapshot positions, action identity/timing and bounded recent combat events so animation remains coherent across snapshot intervals, pause, reconnect and session replacement. Damage remains a fixed-tick authority decision.
- **BREAKING:** revise the wire protocol for typed recruitment and combat presentation state; reject incompatible builds at admission. Internal mutable unit collections become ECS-owned state, requiring updated test fixtures and snapshot consumers.
- Extend cheap core coverage and selected existing network checks; add one independently selectable, bounded graphical combat slice and packed-asset assertions through the existing harness.

## Capabilities

### New Capabilities

- `ecs-unit-combat`: Match-owned Arch simulation with stable identities, deterministic lane contact and attack timing, bounded lifecycle, and engine-independent verification seams.

### Modified Capabilities

- `coop-city-match`: Explicit melee/ranged recruitment and automatic combat while preserving economy, persistent health, city defense, wave cadence, redistribution and outcomes.
- `city-tabletop`: Typed recruitment controls, free rigged combat assets and snapshot-driven animation that agrees with authoritative positions.
- `resumable-multiplayer`: Complete combat resynchronization, event identity and compatible protocol admission without guest simulation or duplicate animation effects.
- `coop-verification`: Meaningful ECS/contact/timing tests and small real-process/presentation checks, including packaged rigged assets.

## Impact

Affected areas are `src/Game.Core/World.cs`, authority lifetime and DTOs; `src/Game/Main.cs` command/snapshot delivery; `src/Game/Tabletop.cs` and new unit presentation classes; vendored character assets/provenance; gameplay/authority tests; and the existing `DevRunner` network/UI harness and documentation. Add Arch 2.1.0 to the core and intentionally refresh affected NuGet locks for the existing Linux/Windows runtime configurations, without changing the locked Godot/.NET toolchain. Coordinate against the existing distribution work rather than overwriting it. The first asset proof must establish compatible rigs and required clips before the full presentation is wired; new assets are a required deliverable, not a placeholder fallback.
