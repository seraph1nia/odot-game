# Proposal

## Why

Ordinary paid production reaches twenty units by wave five while one upgraded Farm sustains roughly twenty-nine Swordsmen; gold and field ownership rarely constrain growth. The accepted exploration recommends an explicit spatial army budget and useful veteran rotation, while recognizing that capacity alone does not fix authored wave difficulty.

## What Changes

- **BREAKING**: Give allied armies two persistent six-size-capacity battlefield home tiles and four independently gold-purchased additions. Recruitment and transfers use ordered first-fit placement; combat movement topology and enemy formation remain separate.
- Retain unit identity, wounds, tier and exact home through combat; surviving field units return after retained death cleanup.
- Add selected-unit retirement without refunds and atomic transfers to/from a plot-consuming Town hall.
- Add size-based Town hall reserve capacity and independently priced storage/healing upgrades. Healing is percentage-of-current-maximum HP at actual production only, authorized by that unit's paid food in the most recently completed battle.
- Charge field and stored living units at existing battle entry, field-first and then level/identity order within each group. Storage is distinct from unfed combat reserves.
- Expose capacity purchases, retire/store/send, reserve roster and independent upgrade quotes through simple selection/buttons, with authoritative disabled reasons.
- Preserve cooperative ownership, retransmission, enemy redistribution, combat/death occupancy and locked dependencies. No enemy/AoE/boss tuning, starvation, debt, retirement refunds, drag framework or saves.

## Capabilities

### New Capabilities
- `army-roster`: Persistent physical homes, paid capacity, atomic army transfers, retirement and food-authorized Town hall recovery.

### Modified Capabilities
- `coop-city-match`: Recruitment capacity, independent Town hall upgrades, all-owned field-first upkeep and occupied-hall sale guard.
- `ecs-unit-combat`: Persistent allied starting formation and survivor return, preserving ordinary enemy formation and combat reservations.
- `combat-archetypes`: Limit automatic class-based setup promises to enemy/non-roster formation; allied first-fit homes do not alter shared role statistics.
- `city-tabletop`: Selectable home capacity and unit/Town hall actions, roster and healing/food explanations.

## Impact

Core economy/roster/production/combat integration; typed commands and complete snapshots; graphical inspector/building/HUD rendering and input; shared paid campaign policies and relevant owned economy/combat/reconnect verification. Protocol/configuration identity changes deliberately; no package/version/release change. Exploration evidence is summarized in design; implementation must measure ordinary expansion/rotation choices, not only Victory.
