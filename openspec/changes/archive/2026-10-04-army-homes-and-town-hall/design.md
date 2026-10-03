# Design

## Context

See proposal.md for motivation. The completed exploration used 30 paid solo samples at seeds 0/1/123: twenty-unit frontline and twelve-unit controls all won; no-tier controls lost W14/W14/W8, six-unit controls W4/W3/W7. Seed-one frontline produced 465 food, earned 110, paid 360, sold 175 and retained 40; acquired 77 units with 59 casualties and retained 282 gold. Current formation is automatic, units share one ECS-owned health, food is paid once before battle, and cleanup retains death claims for 48 ticks. Capacity therefore adds a growth/placement choice rather than promising a harder campaign. Baseline and new behavior must be distinguished.

## Goals / Non-Goals

**Goals:** One persistent identity/health owner; an authoritative roster assignment separate from transient combat location; deterministic insertion and restoration; independently quoted reserve/recovery investment; atomic resource and health changes; current-state UI/reconnect evidence and ordinary acquisition controls.

**Non-Goals:** Enemy/AoE/boss or general food rebalance; individual level upgrades; saves; drag framework; automatic overflow, starvation/debt or retirement refunds. Field expansion never depends on Town hall upgrades.

## Decisions

### Homes and roster seam

Use existing allied forward cells in center/owner-left/owner-right order, then rear cells in that order. First two forward cells are purchased initially. Insert first-fit by free size and choose forwardmost free anchor then ID. This deliberately packs rather than spreads allied setup. Purchase order is fixed, next quote uses purchase count, and maximum is six homes. These are roster homes, not terrain ownership; all movement cells remain legal under existing faction rules.

An authority-owned city roster maintains unit-ID assignments to field home or Town hall building generation/storage spot, plus completed-battle paid-identity recovery eligibility. Current health/profile remains in CombatUnit: no shadow health in roster/UI. Unit snapshots expose detached assignments. Stored units remain living army members but cannot form/deploy/target/attack/screen. Food Reserve is a separate temporary battle lifecycle, not persistent storage.

Capture homes before battle; form allied roster actors at those exact positions and form enemies normally. On wave resolution preserve outcomes immediately and schedule restoration after all retained deaths expire. During cleanup, edit the roster without stealing combat claims; restore all current living field assignments at the cleanup boundary. This also handles newly recruited/transferred units, unfed field units and final victory. Elimination removes field and stored units; no ownership transfer or resurrection.

### Economy candidates and independent hall tracks

Start field-price calibration at 5/8/12/18 and compare one later-steeper candidate 5/8/16/24. First expansion remains affordable before the observed six-unit failure window; the second creates twelve normal positions. Baseline subtraction leaves ample late gold, so compare actual field quality, retirement/storage/upgrade choices, not only final Victory. The nineteen checked-in paid cases select **5/8/12/18** as final bounded field prices: both curves keep all twelve main seed/family samples viable; the steeper control mostly delays later field purchases without proving better difficulty. Candidate frontline reaches its last home in W6 across all three seeds, reserve rotation in W9/W9/W10 while paying 43 gold for hall/tracks and reducing casualties. Initial-home-only and no-tier controls still lose. No arbitrary early price spike or enemy/food adjustment is justified; see verification.md for actual data and limitations.

Town hall consumes one existing city plot. Initial construction is 5 gold/2 wood/2 stone, analogous to an advanced support investment rather than a free home extension. Capacity levels 1..3 are 6/12/18 size points (three/six/nine ordinary stored units), upgrades 8 gold/2 wood/2 stone and 12 gold/2 wood/3 stone. Healing levels 1..3 are 5/10/15 percent per production, upgrades 6 gold/2 wood/2 stone and 12 gold/2 wood/3 stone. Three full eligible production resolutions heal up to 15/30/45 percent before capping, making recovery significant without immediate reset. These tracks are bounded and independent; both paid investments contribute to existing half building-sale refund accounting. Occupied-hall sale rejects. Several halls can each own their own bounded inventory, paid plot footprint and quotes; no global free storage pool.

### Food and recovery clock

Forecast and pay all living owned units once at battle entry, field-first, then descending level/ascending ID per destination group. A stored funded ID is paid but not participating; field underfunding retains inactive reserve behavior. Projection exposes demand/payment/funded/participants/stored/unfunded, avoiding an implicit deployment assertion.

At a completed shared wave, record the exact paid IDs as authorization for the next building cycle. A field-funded survivor sent into a hall retains authorization; a newly recruited ID is not authorized merely by appearing in storage. First cycle has no completed receipt and grants no healing. An unpaid stored unit cannot heal until funded in a subsequently completed battle. Healing occurs only when actually stored at an actual production, using ceil(maximum integer hundredths * current percentage / 100), capped at maximum. Research changes the current maximum, not current HP or authorization. Production preflight computes all material/research/health changes before any commit. Preparation, Ready/unready, upgrades, send, pause, observation/reconnect and wall clock create no heal. Food receipts do not replay on local clear, transfer or cleanup.

### Commands and presentation

Add unit-targeted retire/store/send commands and explicit home/capacity-track/healing-track upgrades. Retain authenticated owner, editable phase, ready/pause, match/turn, hall-generation, purchase-count, expected track-level quote, sequence and retry checks. Preflight destination capacity before paying/creating; transfer updates one assignment or nothing. Retire removes living identity without combat death/resurrection/refund. Missing/foreign/full hall rejects. Recruit defaults to field only, without implicit overflow.

Use inspector buttons and a selected Town hall roster of stable storage tile/anchor slots; clicking a stored entry selects that same unit for Send/Retire. Purchased/locked home tiles have direct selectors and next-price/size details. No action runs on selection. Display field/store upkeep split, current paid receipt/recovery eligibility and three-production healing cadence. Restore persistent model positions after cleanup from authoritative state, not animation callbacks. Extend existing observation selectors and ordinary input paths rather than creating a second UI automation system.

### Verification and compatibility

Add cheap behavioral tests before core changes, then selected economy/combat/reconnect and ordinary protocol delivery. Extend existing expensive scenarios only for actual controls/rendered return and restoration that cheap tests miss; reuse owned setup/display, stable selectors and fresh observation IDs. No full graphical campaign or option matrix. Owned comparison campaigns record gold alternatives, field/storage sizes, tier composition, wounded/recovered HP, casualties, retirements and actual paid demand; include a challenging control and inherited enemy allocation.

Configuration identity/protocol changes must be explicit. Existing isolated numerical combat/reference fixtures remain unchanged; references affected by deliberate city-roster formation gain new, named expectations/evidence rather than bulk digest refresh. Full before/after CI gates source, sequential exports and packages. No dependency/tool lock changes or release version bump.

## Risks / Trade-offs

- First-fit packing changes contact/screens and increases AoE exposure -> measure ordinary opening and mixed controls at shared seeds, without weakening role or cooperative assertions.
- Healing preserves power and reduces replacement costs -> charge all-owned upkeep, require completed paid-ID eligibility and record actual investment/recovery, keeping wave tuning separate.
- Full veterans can prevent higher-tier recruitment -> explicit retire/store choices, no automatic eviction or disguised individual upgrade.
- Cleanup and reconnect may show duplicate locations/heals -> one assignment/health owner, restoration barrier, detached current-state snapshots and exact repeat/reconnect tests.
- Capacity and recovery compete with already abundant gold -> bounded field/storage maxima and paid city footprint; compare candidate price shapes rather than claim the gold sink guarantees difficulty.

## Migration Plan

No persisted match/save migration exists. New processes use the new protocol and configuration; mismatched peers refuse clearly. Rollback is source revision rollback, not conversion of running armies. Preserve nonsecret measurements and update current gameplay/verification docs, then sync/archive only this change after its tasks and full gates pass.
