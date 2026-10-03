# Proposal

## Why

Early construction mixes gold and materials while large resource amounts make costs harder to scan. Players also have to open City details to find the food forecast, making the consequences of recruitment and trading less visible.

## What Changes

- Make wood the ordinary cost for basic producers, Barracks and Archery Range; remove gold from basic producer upgrades and their first recruitment-building upgrades. Keep gold for land and advanced choices, with an explicit Lumbermill recovery purchase when wood is empty.
- **BREAKING**: Rebase gold, wood, stone, metal and cloth defaults to one fifth of their current amounts, including grants, production, retained gold costs, expansion and rewards. Keep food production, rewards and 1–2 food per-unit battle upkeep at their current scale. Use whole-unit recruitment rounding and preserve Market exchange value through whole bundles.
- Add Stock and Income/turn columns to the observed city's resource table and a small Upkeep table directly underneath. Distinguish future production, upcoming food payment and actual battle receipts.
- Show complete text costs alongside producer benefits, current-to-next upgrade output, and explicit missing amounts. Preserve text-only controls, resource order and the compact HUD.
- Extend existing cheap economy/presentation coverage and the selectable economy UI scenario, including supported window sizes and inspector placement.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `coop-city-match`: Smaller whole-resource defaults, material-led construction and upgrades, Lumbermill recovery, whole-unit recruitment rounding, converted Market bundles and reward amounts.
- `city-tabletop`: Three-column resource table, visible compact upkeep, phase-aware income wording, readable costs/benefits and supported layout behavior.

## Impact

Numerical changes belong in `src/Game.Core/World.cs`, `Catalogs.cs`, `EconomyConfiguration.cs`, `Campaign.cs` and authoritative snapshot/command definitions. Presentation affects `TabletopHud.cs`, `Tabletop.cs`, `ProgressionPresentation.cs`, `HudInvalidation.cs` and inspector layout. Protocol compatibility, economy fingerprints, existing core tests, ordinary campaign strategies, DevRunner UI observations, gameplay documentation and verification evidence must be updated. No new dependencies or asset downloads are needed.
