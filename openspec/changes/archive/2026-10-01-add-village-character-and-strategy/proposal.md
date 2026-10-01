# Proposal

## Why

Odot's village has attractive assets but little visible activity, only three buildable buildings, and an economy dominated by an early upgraded farm. Expand its character and strategic choices together so terrain, stockpiles, troops, defenses and upgrades make a short cooperative match more readable and varied.

## What Changes

- Enrich the landscape with a raised rear terrace, lower riverbank, wooded slopes, edge mountains, bridges and building-specific props. Elevation remains cosmetic; keep nine stable plots and the bounded battle approach.
- Use only free, officially sourced KayKit assets: Medieval Hexagon, Adventurers, Skeletons and Resource Bits. Add animated Barbarian/Berserker and Mage allies, matching skeleton enemies, and appropriate weapons; paid food props are replaced by free medieval grain/sacks.
- Add melee, ranged and magic classifications with four archetypes: Swordsman, Berserker, Crossbowman and Mage. Both factions share the same combat profiles at equal research rank; mage attacks have bounded splash.
- **BREAKING**: Expand the economy to gold, food and wood, with a lumbermill and distinct construction, recruitment and research costs. Rebalance income investments and farm upgrades against the nine-production match.
- Expand construction to nine building types: Farm, Mine, Lumbermill, Barracks, Archery Range, Arcanum, Blacksmith, Arrow Tower and Catapult Tower. Add two building levels, tower upgrades and two ranks of small class research bonuses; duplicate blacksmiths do not multiply research.
- **BREAKING**: Add a spending/preparation stage after each wave's third production, followed by an explicit ready-to-battle check. Production still occurs exactly nine times; final-turn resources become usable.
- Render bounded stockpiles that grow and shrink with authoritative resource amounts, and structural/prop changes for building upgrades.
- Add restrained construction, recruitment, combat and ambient feedback, including sound, sparks, dust, flags and windmills. Replay-safe combat effects follow the existing event history and pause/reconnect lifecycle.
- Extend existing cheap, network, graphical and package checks with focused coverage and recorded balance evidence.

## Capabilities

### New Capabilities

- `combat-archetypes`: Faction-independent unit roles, symmetric profiles, class research, bounded splash and buildable tower combat.
- `game-feedback`: Local action sounds, event-driven battle effects, ambient motion and bounded resource stockpiles.

### Modified Capabilities

- `coop-city-match`: Three-resource economy, expanded construction/recruitment, research and upgrades, final preparation, and mixed enemy waves.
- `city-tabletop`: Free skeleton/adventurer presentation, terraced scenery, contextual expanded controls and visible upgrade differentiation.
- `coop-verification`: Cost-conscious regression coverage and reproducible ordinary-gameplay balance evidence.
- `ecs-unit-combat`: Preserve timed attack invariants while allowing a bounded splash attack to hit each selected victim once.

## Impact

Rules and deterministic simulation change in `src/Game.Core/World.cs` and `Combat/`; DTOs, protocol version, automation commands and consumers change together. `src/Game/Tabletop.cs`, `VillageLayout.cs`, `UnitAssets.cs`, `UnitView.cs` and graphical audio/effects need expanded presentation. Vendored asset subsets and provenance grow; no engine, SDK or dependency upgrade is planned.

Update core tests, `tools/DevRunner` scenarios, gameplay/verification documentation and source/export checks. Preserve local/playing-host/dedicated parity, elimination and redistribution, shared pause, resume credentials, owned private displays and offline exports. Build on completed `add-arch-animated-unit-combat`, whose deltas were synced and archived during this planning session; preserve unrelated Steam invitation work.

Manual unit orders, terrain combat modifiers, extra plots, stone/iron currencies, resource upkeep, trading, healing buildings, paid content and publishing are outside this change.
