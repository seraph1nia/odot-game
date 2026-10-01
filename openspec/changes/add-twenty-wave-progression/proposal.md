# Proposal

## Why

The current three-wave match and recruitment discounts provide little long-term army progression. Extend the match to twenty waves with readable unit levels, stronger paid recruits, modest clear rewards and two simple boss encounters, while retaining the cooperative city-defense loop.

## What Changes

- **BREAKING:** Replace the three hardcoded waves with twenty authored compositions. Each entry declares archetype, level, count and ordinary/boss identity; waves ten and twenty contain one strong boss per original roster allocation.
- Scale unit health and damage from per-archetype level-one bases with a shared initial `1.35^(level - 1)` curve. Ordinary allies and enemies share the same profiles; boss health/damage multipliers are explicit. Preserve combat timing, movement, capacity and roles across levels.
- **BREAKING:** Barracks, Archery Range and Arcanum gain five levels. Their level determines newly recruited units' level and rising, rounded prices, replacing food discounts. Existing soldiers retain their own level and remaining health. Other buildings retain two levels and Blacksmith research remains separate.
- Award each surviving city 10 gold, 5 food and 5 wood once per ordinary wave clear; bosses award double. Include disconnected living cities and the final victory clear, with no reward for defeat or partial clears.
- Show wave progress out of twenty, boss rounds, recruit level/profile/cost, next building-upgrade effects and a durable last-clear reward summary through authoritative state.
- Rebalance the opening, city defense and existing strategy families for the longer match using cheap full-run simulations; extend existing network/UI slices only for transport and presentation defects they uniquely observe.
- Preserve three production turns plus final preparation per wave, persistent wounds, readiness, pause, original-roster pressure and redistribution. No XP, individual leveling, boss abilities, new currencies, shared raid battlefield or new assets.

## Capabilities

### New Capabilities

None; extend existing gameplay and presentation capabilities.

### Modified Capabilities

- `coop-city-match`: Twenty-wave outcomes, authored mixed-level allocations, recruitment-building progression, persistent recruit levels and exactly-once clear rewards.
- `combat-archetypes`: Shared level scaling, explicit boss profiles and interaction with existing class research.
- `city-tabletop`: Level-aware recruitment and upgrade controls, twenty-wave/boss feedback and clear-reward summaries.
- `coop-verification`: Twenty-wave strategy and outcome coverage, progression/reward invariants and focused transport/UI integration checks.

## Impact

Numerical work belongs in `src/Game.Core/World.cs`, `Catalogs.cs`, the combat profile/configuration/ECS identity types and authoritative snapshots. `src/Game/Tabletop.cs`, unit inspection/presentation and diagnostics consume the same resolved catalogs; remove duplicated discount arithmetic in the UI and DevRunner. Bump the protocol from the version present at implementation time and update configuration identity. Update core/authority tests, existing DevRunner scenarios, README and gameplay/verification documentation without introducing dependencies or changing tool locks.

This change follows `rework-deterministic-hex-combat`. That active change still specifies three-wave outcomes and old balance expectations; preserve its spatial, deterministic, lifecycle and cooperative guarantees, then supersede its wave-count/economy assumptions with these deltas. Do not apply competing edits to its in-progress implementation or alter its planning artifacts as part of this proposal. All files created here are planning artifacts only.
