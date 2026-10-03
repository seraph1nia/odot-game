# Design

## Context

See proposal.md for motivation. The current economy uses bounded integer `ResourceCost` values, frozen authority-owned catalogs and a combined configuration fingerprint. Most basic buildings cost 20 gold plus 10 wood; Lumbermills cost gold alone. Recruitment prices use the 1.35 growth curve rounded to multiples of five and validate increasing totals. Food is already expressed as 1 or 2 per living soldier per battle. `Campaign.Default()` authors material rewards separately from `Rules`.

The UI resource panel is a two-column text grid. Forecasts and actual upkeep receipts are already synchronized but shown through the Details dialog. The unit inspector positions itself below the resource panel. `HudInvalidation` tracks stocks and forecasts but does not track the producer slots/catalog as economy-panel inputs. Existing specs explicitly require gold in basic construction, multiples-of-five rounding, original reward values, and a two-column table. The deltas replace those rules while retaining their other scenarios and guarantees. Existing text-only requirements remain applicable.

## Goals / Non-Goals

**Goals:** Use one authoritative set of small whole-resource quotes for commands, snapshots, UI and verification; distinguish production capacity from a payment that occurs now; make food consequences visible with minimal additional height.

**Non-Goals:** Fractional resources, recurring building upkeep, resource imports, new combat/research mechanics, new assets, live conversion of running matches or a broad HUD redesign.

## Decisions

### 1. Author new defaults in the authority

Scale gold, wood, stone, metal and cloth defaults by five, then apply the approved gold removals. Food is an explicit exception. This is actual game accounting; a display-only divisor would obscure affordability and create fractional balances.

| Default | New value |
| --- | --- |
| Starting grant | 12 gold, 6 wood; other stocks zero |
| Base production | 2 gold/turn |
| Gold Mine, Lumbermill, Stonecutter output | 1 / 2 per turn |
| Metal Mine, Weaver output | 4 / 8 per turn |
| Farm output | 5 / 8 food per turn |
| Ordinary clear reward | 2 gold, 1 wood, 5 food, 1 research |
| Boss clear reward | 4 gold, 2 wood, 10 food, 1 research |
| Expansion prices | 5, 8, 12, 18 gold |

| Building | Construction | Upgrade L1 to L2 |
| --- | --- | --- |
| Lumbermill | 1 wood | 2 wood + 2 stone |
| Farm, Gold Mine, Stonecutter, Metal Mine, Weaver | 2 wood | 2 wood + 2 stone |
| Barracks, Archery Range | 2 wood | 2 wood |
| Arcanum | 5 gold + 2 wood + 3 stone | 4 gold + 2 wood |
| Research Tower | 2 wood + 2 stone | 4 gold + 2 wood + 2 stone |
| Market | 2 wood + 2 stone | None |
| Arrow Tower | 4 gold + 3 wood | 4 gold + 2 wood + 2 stone |
| Catapult Tower | 6 gold + 4 wood + 3 stone + 2 metal | 4 gold + 2 wood + 2 stone + 1 metal |

Recruitment-building upgrades L2 to L3, L3 to L4 and L4 to L5 retain scaled advanced prices: respectively 6 gold + 2 wood + 2 stone, 9 gold + 2 wood + 3 stone, and 14 gold + 2 wood + 4 stone. Preserve maximum levels and archetype offerings. Research points/progress and combat values do not use the resource divisor. Custom `Rules` amounts are already in the new units; never silently divide caller-provided values.

### 2. Keep food and Market value consistent

Retain 1/2 food upkeep, Farm output and food rewards. Dividing food alone would cut affordable army size; fractional upkeep would defeat readability.

Use the smallest whole bundles preserving existing economic exchange value after the unit change: 5 wood or stone for 1 gold; 5 metal or cloth for 2 gold; 25 food for 1 gold. Gold and materials both shrink, leaving the material ratios unchanged. Only gold shrinks for food, so its bundle must grow. Accept the coarser minimum sale quantity and show it explicitly; do not round fractional proceeds into a favorable exchange or introduce residual-credit accounting.

Keep actual-investment refunds at half, rounded down per resource after summing. A 1-wood Lumbermill refunds zero until further investment; display the exact refund. This accepted granularity is preferable to fractional balances or a special refund rule.

### 3. Use whole-unit recruitment quotes

Level-one costs are Swordsman 2 metal, Berserker 3 metal, Crossbowman 2 wood + 1 metal, Mage 3 cloth + 1 gold. Resolve every level from the original base with the existing 1.35 curve and midpoint-up nearest-whole rounding (`multiple: 1`). Keep positive components positive and zero components zero. Default total prices increase across all five levels. For valid custom small costs, permit a nondecreasing total rather than reject a plateau caused by rounding; health/damage still improve independently. Avoid arbitrary price increments, which would distort material ratios and the authored growth curve.

### 4. Make recovery a quoted, explicit purchase

Normal Lumbermill construction pays 1 wood. At exactly zero wood an eligible owner may explicitly choose a recovery Lumbermill for 4 gold on an empty purchased plot. Gold income continues without a producer and an occupied city can sell a building to free space, preserving a recovery path. Recovery is available only for Lumbermills and only when current wood is zero; it grants no land or resources.

Publish an optional recovery construction quote on the building definition and add a command payment choice defaulting to normal construction, e.g. `ConstructionPayment.Standard` / `GoldRecovery`. The authority checks the requested mode and current eligibility, atomically pays the selected quote, and records that actual payment in investment. Never infer a different currency from a failed normal request. Reject unknown modes and recovery choices on non-build actions or other building types. Carry the choice through ordinary runner action construction and retry serialization. This explicit option prevents UI and authority disagreement about the price paid.

### 5. Publish income and render a compact stack

Add nullable `ResourceCost? ProductionIncome` to synchronized city state, computed using the same frozen economy that resolves production. It is base gold plus current producer outputs for living cities; fallen and terminal cities expose zero. Null explicitly means unrepresentable income for a custom configuration, and the UI displays Unavailable. The UI formats the supplied value without rebuilding an economy locally. Snapshot projection must remain read-only; use checked arithmetic and keep unrepresentable projection from wrapping a value or crashing publication.

Use a VBox containing the resource grid and small Upkeep grid inside the existing resource panel/input boundary. Resource order stays Gold, Food, Wood, Stone, Metal, Cloth, with text-only headers Resource, Stock, Income/turn and right-aligned numeric columns. Show positive income with + and zero as 0. Outside Building, label it Next building turn and clarify that Ready for battle grants no income; outcomes show no future income. Pause and transport loss retain synchronized values with a paused/stale cue, and forecasts must not imply a payment during either state.

The two-row Upkeep table shows Next battle demand and Food after payment (`Available - Paid`) while building/preparing. With a shortage, replace the second row with the exact number of soldiers that will sit out; keep projected payment and allocation details accessible in Details. Combat/outcome uses the latest wave-tagged actual payment and reserve count, clearly labeled Paid this battle or Last battle. Empty armies show zero; fresh sessions clear old receipts. Food trade previews continue to identify a projected shortage before committing.

Compact spacing must leave space for the unit inspector at 1100x820 and 1280x720. Pass the combined resource/upkeep bottom to inspector placement, retaining all essential stats through an intentional scroll surface if necessary. Preserve the approximately 180px bottom panel, the narrow resource panel and nine-plot overview. The entire stack blocks world clicks, zoom and drag initiation.

Add income, relevant producer/catalog state and combined panel observations to HUD invalidation and test observation data. Construction, upgrading or sale must refresh income even when unrelated balances stay the same. City switching/reconnect use the observed city consistently. Producer buttons show complete cost and current benefit; upgrade context shows current to next output. Accessible disabled reasons use Need N more resource plus its producer source, alongside ownership/readiness/phase reasons. Keep text names and existing category ordering.

### 6. Verify through existing slices

Run full `mise run ci` before implementation (reuse a valid unchanged baseline), cheap `mise run test` throughout, selected `mise run test-ui --scenario economy` for graphical diagnosis, and full CI after the coherent implementation. Restore locked, format changed C# using `dotnet format Odot.slnx --no-restore`, and retain ignored logs, timings and screenshots.

Extend existing core economy, resource, building transaction, army progression and presentation/invalidation coverage for zero-gold basics, exact new quotes, recovery modes/investment/retries, converted Market value, food invariants, income projection and refunds. Adapt shared `CampaignStrategy` reserve thresholds and ordinary openings to the new units; food reserves retain their current units. The cheap full campaign acceptance cases remain the balance gate, including cooperative assertions and disclosed role costs.

Extend the existing economy UI slice for fresh income/forecast observations after actual input, recovery selection, food-sale shortage, combat receipt, city switches, reconnect and inspector placement at both reference sizes. Cheap tests cannot catch overlap, incorrect control wiring or world input passing through the new stack. Reuse owned setup and existing checkpoints; add only a bounded layout visit where the current slice lacks a reference size. Expected cost is additional observations/captures and a short viewport transition, with no new scenario process/display or full graphical campaign. Ordinary network CI covers the protocol change; no additional network/UI scenario is admitted.

## Risks / Trade-offs

- Removing gold permits earlier expansion and upgrades -> retain ordinary twenty-wave strategy and cooperative balance acceptance; report failures before any further gameplay scope change.
- Small units make refunds and trades coarse -> expose exact quotes and verify economic value without hidden fractional accounting.
- The new stack can crowd the inspector -> verify complete bounds and actual input at both supported sizes, keeping compact typography and spacing.
- HUD caching can leave income stale -> track income and producer inputs and test accepted build/upgrade/sale updates independently of stocks.
- Snapshot income can exceed integer bounds under custom rules -> use the existing checked arithmetic and an explicit unavailable projection for invalid capacity; maintain atomic production rejection.

## Migration Plan

Ship rules, authority, graphical client and DevRunner together. Include recovery quotes/payment semantics and the new income projection in configuration identity where appropriate; increment the protocol version in `Diagnostics.cs` because command/snapshot semantics change. Preserve the separate combat fingerprint and algorithm version since combat calculations do not change. Old peers must receive the existing clear compatibility refusal. No persisted match/save conversion is required; active matches are not migrated. Rollback restores the complete prior version and begins a fresh match. Update `docs/gameplay.md`, README and verification evidence with the new defaults and coverage limits.
