# Proposal

## Why

Odot's current gold-paid class ranks improve numbers but give armies few tactical choices. A personal research economy and exclusive specializations will let players develop distinct armies across a match while burn, poison and chill give those choices observable combat consequences.

## What Changes

- Add independent city research points, production progress, purchased technologies and exclusive branch choices. All persist across waves and reset on a fresh match; there is no account progression or team pool.
- Award each surviving city one research point exactly once per shared wave clear, including boss and final clears. Replace the Blacksmith with a two-level Research Tower: each production contributes one/two progress units, three progress units produce one point, and multiple towers add their output. Full three-production cycles therefore add one/two points per tower.
- Add a shallow shared catalog with personal purchases: foundations cost 3 points, specializations 6 and advanced improvements 9. Each class offers an exclusive fork; Fire/Frost is the magic fork. Technologies affect eligible existing and future units without healing or changing their individual levels.
- **BREAKING** Replace gold-paid Blacksmith research and its building-gated purchase command with technology-id purchases accessible without a selected plot or owned tower. Retain the former building's economic slot identity and two-level construction/upgrade role under the Research Tower name; update the wire protocol and reject incompatible peers.
- Introduce bounded authoritative burn, poison and chill. Burn refreshes without unbounded damage stacking, poison has capped stacks, and chill slows future movement and attack actions without altering already committed intervals. Preserve simultaneous impacts, transfers, pause, reconnect and current-state restoration.
- Add a research panel, point/progress and lock explanations, technology-aware unit inspection and restrained status indicators using bundled assets/code-native presentation.
- Preserve ordinary campaign and cooperative verification, adapting the existing research strategy and extending only relevant network/UI slices.

Freeze, stun, hard-control interruptions, new unit archetypes, healing, logistics/economic technology branches, defense technologies, elemental reaction chains, enemy-wave rebalance and permanent progression are outside this first change.

## Capabilities

### New Capabilities

- `city-research`: Personal match research income, towers, catalog, purchases, exclusive forks and persistent capabilities.
- `combat-status-effects`: Deterministic bounded burn, poison and chill, lifecycle, damage integration and reconstructable current state.

### Modified Capabilities

- `combat-archetypes`: Replace the small gold-paid rank system with technology-derived capabilities while retaining archetype identity and level scaling.
- `coop-city-match`: Replace the Blacksmith building role, integrate research progress and clear rewards, and retain purchased research through building sales.
- `ecs-unit-combat`: Include periodic damage and status application in same-tick ordering while retaining committed movement/attack deadlines.
- `city-tabletop`: Replace contextual Blacksmith controls with independent research inspection/purchases and authoritative technology/status presentation.
- `resumable-multiplayer`: Carry complete research/capability/status state and enforce compatible admission and retry-safe technology purchases.

## Impact

Numerical rules/catalogs, `World.cs`, combat actions/state/projection and configuration identities change in `src/Game.Core`; command/snapshot handling, HUD, inspector and status presentation change in `src/Game`. Existing xUnit authority, economy, combat, playback and campaign fixtures plus `tools/DevRunner/CampaignStrategy.cs` and affected network/UI slices need adaptation. README, gameplay and verification documentation need updated rules and evidence.

No dependency update, tool installation or new external assets is planned. The in-progress `optimize-core-simulation` change overlaps these files; apply must reconcile with its current stage-owned observations/caches and preserve its equivalence/performance contracts. This proposal changes planning artifacts only and does not alter that change or implementation files.
