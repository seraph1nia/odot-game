# Proposal

## Why

The current three-wave match offers most building choices immediately and provides little long-term army progression. Extend it to twenty waves where resource production, limited building space and paid recruitment-building levels support stronger armies, with readable choices, modest clear rewards and two simple boss encounters.

## What Changes

- **BREAKING:** Replace the three hardcoded waves with twenty authored compositions. Each entry declares archetype, level, count and ordinary/boss identity; waves ten and twenty contain one strong boss per original roster allocation.
- Scale unit health and damage from per-archetype level-one bases with a shared initial `1.35^(level - 1)` curve. Ordinary allies and enemies share the same profiles; boss health/damage multipliers are explicit. Preserve combat timing, movement, capacity and roles across levels.
- **BREAKING:** Add stone, metal and cloth alongside gold, wood and food, with Stonecutter, Metal Mine and Weaver producers. Keep the existing gold-producing Mine, labeled Gold Mine. Construction and upgrades use material costs rather than producer-ownership, talent or wave unlocks: wooden infrastructure opens first, stone enables advanced construction and higher recruitment-building levels, soldiers require metal and mages require cloth.
- **BREAKING:** Start with five usable plots on the existing nine-plot board; buy the remaining plots individually for increasing gold prices. Sell buildings for 50% of their actual construction and upgrade investment, rounded down per resource and refunded in kind, without losing the purchased plot or existing soldiers/research.
- Add a Market occupying one plot, enabling explicit fixed-rate sales of food, wood, stone, metal and cloth for gold. Building sales and plot purchases do not require a Market. Markets have one level; selling the last Market immediately removes resource-selling access.
- **BREAKING:** Barracks, Archery Range and Arcanum gain five levels. Their level determines newly recruited units' level and rising material prices, replacing food discounts. Food becomes upkeep charged once at battle entry, with per-archetype costs and a visible preparation forecast. The shortage default keeps unfed soldiers as inactive reserves for that wave, with stronger units fed first and no food-based shared readiness block. Existing soldiers retain their own level and remaining health. Producers, towers and Blacksmith retain two levels; Blacksmith research remains separate.
- Award each surviving city 10 gold, 5 food and 5 wood once per ordinary wave clear; bosses award double. Include disconnected living cities and the final victory clear, with no reward for defeat or partial clears.
- Show all six resources, grouped construction choices with affordable actions highlighted and unavailable actions greyed with missing-cost explanations, locked plot purchase controls, building refunds, Market quotes, upkeep forecasts, wave progress out of twenty, boss rounds, recruit level/profile/cost, next upgrade effects and a durable last-clear reward summary through authoritative state.
- Rebalance the opening, city defense and existing strategy families for the longer match using cheap full-run simulations; extend existing network/UI slices only for transport and presentation defects they uniquely observe.
- Preserve three production turns plus final preparation per wave, persistent wounds, pause, original-roster pressure and redistribution. Defer talent trees, refined swords/ore/fibre chains, automatic free recruits and additional archetype unlock rules; retain the existing building-to-archetype mapping. No XP, individual leveling, boss abilities, shared raid battlefield or new external asset dependency. Reuse bundled free assets with distinct labels/props for the new economy.

## Capabilities

### New Capabilities

None; extend existing gameplay and presentation capabilities.

### Modified Capabilities

- `coop-city-match`: Six-resource production and construction, material recruitment, battle upkeep, expandable plots, Market trades, building sales, twenty-wave outcomes, persistent recruit levels and exactly-once clear rewards.
- `combat-archetypes`: Shared level scaling, explicit boss profiles, retained research after selling a Blacksmith and tower lifecycle after a sale.
- `city-tabletop`: Six-resource and upkeep feedback, grouped affordable/unaffordable choices, locked plot and sale controls, new building presentation, level-aware recruitment, twenty-wave/boss feedback and clear-reward summaries.
- `coop-verification`: Resource-graph and twenty-wave strategy coverage, transaction/upkeep/progression/reward invariants and focused transport/UI integration checks.

## Impact

Numerical work belongs in `src/Game.Core/World.cs`, resource/purchase catalogs, combat profile/configuration and authoritative snapshots. `src/Game/Tabletop.cs`, village/menu layout, resource visuals, unit inspection and diagnostics consume the same resolved catalogs; remove duplicated discount arithmetic in the UI and DevRunner. Implement resource transactions and upkeep before the full-run numerical balance gate, then complete protocol/UI integration. Bump the protocol from the version present at implementation time and update configuration identity. Update core/authority tests, existing DevRunner scenarios, README and gameplay/verification documentation without introducing dependencies or changing tool locks.

This change follows `rework-deterministic-hex-combat`. That active change still specifies three-wave outcomes and old balance expectations; preserve its spatial, deterministic, lifecycle and cooperative guarantees, then supersede its wave-count/economy assumptions with these deltas. Do not apply competing edits to its in-progress implementation or alter its planning artifacts as part of this proposal. All files created here are planning artifacts only.
