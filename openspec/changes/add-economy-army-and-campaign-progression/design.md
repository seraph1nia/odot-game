# Design

## Context

See proposal.md for motivation. The current rules have three waves, nine immediately usable plots, gold/food/wood, two-level buildings and food recruitment discounts. `World`, `Tabletop` and DevRunner duplicate parts of those assumptions. The earlier version of this plan extended recruitment levels and waves while retaining that economy; this revision replaces its construction and food-price assumptions with resource production, plot expansion, sales and battle upkeep.

`rework-deterministic-hex-combat` is archived and synced. Follow its integrated interfaces and preserve seeded combat, protected entry, death cleanup, actual melee readability, changed-state publication and cooperative guarantees. Replace shape-dependent footprints with a six-size budget before economy or campaign work. Reconcile complete overlapping requirement blocks against the synced revision. Coordinate overlapping interfaces with `simplify-combat-math-and-flow` according to landing order; its action-state and target-selection refactor remains a separate change.

## Goals / Non-Goals

**Goals:** Resolve costs, profiles, refunds, trade quotes and upkeep in the numerical core; make material production and occupied plots the main progression constraints; establish ordinary-command twenty-wave evidence before broad protocol/UI integration. New branches should be discoverable even when unaffordable.

**Non-Goals:** Talent trees, account progression, a global base level, wave-gated purchases, ore/fibre/sword processing, resource imports or player-to-player trading, automatic sales, free recruits, new archetype unlocks, individual XP/upgrades, retroactive leveling/healing, changing production cadence, boss abilities, shared raid combat, new external assets or save migration. The unfinished default-unit/unlock suggestion is deferred: Barracks still offers Swordsman and Berserker at level one, Archery Range Crossbowman, Arcanum Mage. Building levels affect new recruit strength only. Battle upkeep replaces food recruitment costs; it is not an extra recruitment charge.

## Decisions

### 0. Establish size-based combat before progression

Each actor has a frozen integer Size from 1 through 6; every combat hex has capacity six. Ordinary Swordsman, Berserker, Crossbowman and Mage use size two, regardless of faction, level or research. Boss identity overrides size to six exactly once during profile resolution. Sizes one, three, four and five are supported configuration/test values; there is no current size-one roster entry. Towers and the built-in defender remain outside combat capacity. Keep size separate from visual scale, HP/damage multipliers, upkeep and building plots.

Replace authored neighboring-position footprints and fragmentation-based feasibility with integral size reservations. Any same-team mix whose occupied/reserved size totals at most six fits the cell; render anchors cannot reject that fit. Keep deterministic distinct actor anchors and stable poses for shared cells without moving stationary units for free or changing attack distance. Graphic layout is derived from authoritative membership/placement identity and cannot become an extra occupancy gate. Keep cell distances, faction protection and existing conservative transit conflicts.

Count a stationary actor once per cell. A committed move retains Size at the source and atomically reserves Size at the destination plus existing transit conflicts; rejected moves acquire nothing. Until arrival only the source is attackable. Arrival releases source/transit reservations and retains destination ownership. Death while moving retains both endpoint size claims and transit conflicts until death expiry; stationary death retains one claim. Do not double count state and its matching reservation. Release at the existing same-tick phase, preserve pause and cleanup barriers, and release old claims on redistribution before destination admission. Preserve size, health, identity and recovery through transfer; queued actors have no spatial claims but remain counted as living allocation.

Preserve support-first rear allocation, forward melee, spread-before-packing and seeded tier/initiative queue ordering. Scan that canonical order for fitting actors rather than let a non-fitting size-six actor block a fitting size-two actor. A skipped actor retains identity and tie state. This work-conserving rule does not promise admission ahead of permanently occupying living allies; retain finite crowd/progress bounds and prove ordinary completion. Protected entry uses actual per-cell free size, not aggregate free size across several cells. On a cleared board compute the first-admission deadline from cumulative whole-actor death releases: the earliest tick when any queued size fits one owner-protected cell. It cannot slide on unchanged retries. A boss requires an entirely free cell and a usable protected-to-engagement route.

Validate sizes 1–6, capacity six, same-team accounting, protected routes and checked accumulation before combat. Include size defaults, resolved boss size and the changed occupancy semantics in rules/configuration identity; adapt snapshots/projections with owning changes and bump protocol when needed. Run cheap mixed-size, move/death, transfer, queue, seed, crowd and role gates before economic work. Recheck effective Mage clustering with all support at size two; preserve timing, damage order and role guarantees, documenting allowed balance adjustments rather than weakening acceptance.

Alternative: keep legal footprint masks and only rename CapacityCost. Rejected because a sufficient total size could still fail due to fragmentation. Size is the sole capacity test within an otherwise legal cell; faction, deployment and transit restrictions still apply.

### 1. Resolve a six-resource graph without producer ownership gates

Use a bounded integral resource value across gold, wood, food, stone, metal and cloth. Use checked sums/products for payments, production, refunds, trades, upkeep and rewards. A transaction first validates the entire quote and resulting balances, then commits all effects. Freeze the economy catalog alongside the progression/combat configuration and fingerprint the combined rules. Resource source labels are explanatory metadata, never unlock flags.

```text
Base gold --> Lumbermill --> Wood --> Farm --------> Food --> Battle upkeep
                              |----> Metal Mine --> Metal --> Soldiers
                              |----> Weaver ------> Cloth --> Mages
                              |----> Stonecutter -> Stone --> Advanced buildings
                              |                              and higher levels
                              +----> Barracks / Archery Range

Gold + Wood + Stone --> Arcanum --> Recruit with Cloth + Gold
Gold + Wood + Stone --> Market  --> Sell surplus --> Gold --> Buy plots
Gold + Wood         --> Gold Mine ----------------> Gold
Sell building       --> Half paid materials back + reusable purchased plot
```

All construction also costs gold. The starting catalog below is a tuning candidate, not measured balance. `G/W/S/M` denote gold/wood/stone/metal; omitted components are zero. Producers directly create resources per production turn with no input conversion. Preserve the existing `Mine` identity for Gold Mine and use a distinct identity for Metal Mine.

| Building | Construction | Maximum level | Level 1 / 2 output or role |
| --- | --- | --- | --- |
| Lumbermill | 20 G | 2 | 5 / 10 wood |
| Farm | 20 G + 10 W | 2 | 5 / 8 food |
| Gold Mine | 20 G + 10 W | 2 | 5 / 10 gold |
| Stonecutter | 20 G + 10 W | 2 | 5 / 10 stone |
| Metal Mine | 20 G + 10 W | 2 | 5 / 10 metal |
| Weaver | 20 G + 10 W | 2 | 5 / 10 cloth |
| Barracks | 20 G + 10 W | 5 | Swordsman, Berserker |
| Archery Range | 20 G + 10 W | 5 | Crossbowman |
| Arcanum | 25 G + 10 W + 15 S | 5 | Mage; no cloth construction cost |
| Blacksmith | 20 G + 10 W + 10 S | 2 | Existing paid class research |
| Arrow Tower | 20 G + 15 W | 2 | Existing single-target defense |
| Catapult Tower | 30 G + 20 W + 15 S + 10 M | 2 | Existing capped splash defense |
| Market | 20 G + 10 W + 10 S | 1 | Fixed-rate explicit resource sales |

Start from 60 gold, 30 wood and zero food/stone/metal/cloth, with 10 base gold per production. Retune starting gold/wood, outputs and positive prices through the balance gate if needed; do not insert free stone/metal/cloth to bypass the intended chains. A Lumbermill never needs wood, stone or metal. The existing gold-only Mine construction changes to gold and wood, avoiding a second bootstrap building that bypasses the wood branch. Buildings check stock, not whether a producer remains standing. Selling a Stonecutter after stockpiling stone therefore does not disable stone-funded construction.

All producer and Blacksmith level-two upgrades initially cost 20 G + 10 W + 10 S. Arrow Tower level two costs the same; Catapult level two adds 5 M. This gives stone a continuing spatial-efficiency use. Market has a single level because this scope contains no second-level benefit; do not invent an empty upgrade or additional trade-rate progression just to retain the old universal two-level assumption.

Alternative: long processing chains or a mandatory town level. Deferred because they multiply setup plots and early choices. Stone unlocks the advanced construction branch naturally, and metal/cloth remain ongoing recruitment sinks.

### 2. Treat land, building investment and stock trades separately

Keep the existing nine indexed world plots and camera bounds. Initially IDs 0 through 4 are purchased; IDs 5 through 8 are locked but selectable. Any locked plot can be purchased, with price determined by the count already bought: initial candidates 25/40/60/90 gold. A purchase is permanent for the match, creates no building, has no wave or Market prerequisite and does not enlarge the combat board. The five/nine layout is this revision's concrete initial scope; balance may tune prices and economy, while changing plot counts requires an explicit layout/spec revision before implementation continues. There are thirteen building types for nine plots, so not every city can retain every branch.

Store a building instance's accumulated actually paid construction/upgrade resource vector. On sale, sum first and refund `floor(investment[resource] / 2)` independently for all resources. Do not infer investment from the current level or include land, recruits, research or Market trades. Example: total 55 G + 25 W + 15 S returns 27 G + 12 W + 7 S. Remove the instance and its ongoing behavior atomically; keep its plot purchased. A replacement starts level one with a new investment record and fresh defense identity. Retain city research and surviving soldiers without refunds, healing or level changes. Further research requires a currently owned sufficiently upgraded Blacksmith.

The existing command ledger guards accepted retries. New requests targeting a building instance must identify the expected instance/generation so a delayed command cannot sell, upgrade, recruit from or trade through a replacement on the same plot. Revalidate owner, connectivity, phase, readiness, resource balances and target instance at execution. A plot-purchase quote also includes current expansion count; reject a stale price basis instead of silently spending a higher price. No action is allowed during combat, pause, terminal state or elimination. Selling the last Market immediately disables stock trades; the Market is never required to sell a building or buy land.

Market rates are frozen explicit whole bundles. Initial candidates: 5 wood, food or stone sells for 1 gold; 5 metal or cloth sells for 2 gold. Requests specify a known resource and a positive whole bundle count; calculate exact total stock and gold with checked arithmetic. Reject fractional/zero/negative/unknown/overflowing requests and never clip an unaffordable request to a smaller trade. Multiple Markets do not stack rates or income, and there is no buy direction or automatic end-turn conversion. Low sale returns leave a role for Gold Mines; record economic evidence rather than assuming that relation is balanced. Building-sale rounding cannot manufacture profit by splitting investment, and no automatic free recruit can be farmed through rebuilds.

Alternative: refund gold-equivalent value or recalculate half the current catalog price. Rejected because either would create hidden conversion rules and could disagree with what the player paid. Full refunds would remove the intended cost of reorganizing limited land.

### 3. Charge food at the battle boundary

Food is not deducted for recruitment. Give each archetype a positive per-battle upkeep quote, initially Swordsman 1, Berserker 2, Crossbowman 1 and Mage 2 food. Keep upkeep constant across levels in this scope: stronger recruits cost more equipment while becoming more efficient per occupied army slot and ration. Enemy allocations, bosses, towers and the built-in defender cost no city food. Existing and newly recruited living soldiers owe upkeep each wave, independent of which recruitment buildings still exist.

Resolve upkeep once for every living city, including disconnected cities, when preparation actually transitions into combat, after the previous wave's death-cleanup barrier. Production turns, setting/withdrawing ready, cleanup waits, snapshots and reconnects never charge upkeep. Publish demand/available food throughout the editable phase and preserve a latest wave-tagged payment result. Recompute previews after recruitment and food sales; readiness freezes a city's economic actions. Wave-clear food is available for the next battle only. A zero-army city owes zero; a corpse owes nothing; a capacity-queued living participating soldier still owes its upkeep.

Shortage default selected for this plan: unfed units sit out the wave; food shortage never blocks the shared ready check. Allocate available food deterministically by descending unit level, then ascending stable unit ID. Walk that order once, paying a unit's whole upkeep if affordable; otherwise leave that unit unfed and continue to later cheaper units. No partial feeding, debt, random allocation, damage penalty or starvation damage. This prefers stronger recruits and keeps ties stable without introducing player-managed deployment orders. The preview uses precisely the same allocation and identifies which units will sit out; it recomputes after economic changes without committing payments.

Unfed soldiers remain living members of their city with unchanged identity, level and health, but are inactive reserves for this wave. They have no occupied or reserved size, queue admission, target, attack or city-screening effect. Keep this status distinct from capacity-queued fed soldiers, which participate and may deploy later without another payment. Resolve food allocation before formation and admit only participants; clear any former formation state for reserves without death events. All-unfed armies expose their city to normal enemy siege, while the built-in defender and towers continue normally. Feeding is fixed for the entire shared wave, including later redistributed enemies; clearing one local battlefield does not reconsider reserves or refund rations. At the next preparation reserves are eligible for the next food forecast, and the next battle recomputes participation. City elimination cannot activate or transfer its reserves; match/session cleanup releases them with the rest of that city's state.

Retain the latest upkeep wave, actual food paid, participating IDs and unfed IDs for each living city at entry, together with current participation status. This is a bounded current-wave receipt, not an ever-growing history. Rendering/reconnect does not replay feeding or turn reserves into deaths. Pure combat-role fixtures establish normally fed participants; ordinary strategy fixtures must fund them through actual economic actions.

Alternative: block readiness on insufficient food. Rejected as the default because a disconnected underfed city could prevent the team from starting combat. A temporary damage penalty was not selected because it adds a second strength modifier and obscures how much army the food supply supports. The sit-out policy and stronger-first allocation are explicit design defaults selected during reconciliation; the user confirmed replacing recruitment food with upkeep, but did not separately choose the shortage policy.

### 4. Keep level, research and boss status separate

Add positive `Level` and explicit ordinary/boss profile identity to ECS unit identity and live/dying/snapshot representations. A building upgrade changes only its slot level; a recruit captures that level at creation. Two Barracks at different levels remain useful because either can produce at its own price. Existing soldiers retain level and wounds between waves. Research continues to affect existing and future units by class, recomputing from each unit's own level without healing it.

Resolve stats in this order:

```text
archetype level-one base
  --> round(base * growth^(level - 1)) to whole points, ties upward
  --> explicit boss multiplier (ordinary = 1)
  --> research multiplier (1 + 0.05 * rank)
  --> existing integer-hundredths representation
```

Use a decimal/rational representation for the initial 1.35 multiplier, checked arithmetic and deterministic rounding. Do not repeatedly scale rounded prior levels or use rendering floats to resolve profiles. Validate configured levels/multipliers and all resolved profiles before the match starts; guard exponent work and reject overflow rather than wrapping. Validate worst-case simultaneous damage accumulation including bosses and the existing admitted actor bounds. Enemy levels are not capped by the five-level recruitment UI: a configured level-six entry is valid if its resolved profile is safe. Freeze the resolved configuration and include the progression parameters and resolved catalogs in its versioned identity.

Initial level-one health/damage candidates are Swordsman 40/10, Berserker 30/15, Crossbowman 30/10 and Mage 25/12. Keep each archetype's established timings, initiative and range, with size two for ordinary units and size six for bosses independent of level. Preserve the predecessor's Mage effective two-victim benefit and Crossbowman single-target advantage; the Mage's slower cadence is essential. Swordsman progression at levels 1–5 is HP 40/54/73/98/133 and damage 10/14/18/25/33. Bosses apply 8x HP and 2x damage after this rounding: initial boss profiles are 584 HP/36 damage at level three and 1064 HP/66 damage at level five, before research (default enemy rank zero).

Alternative: reuse research rank as level. Rejected because research is city/class-wide, bounded to two ranks and affects survivors, whereas the user's building choice creates persistent mixed-level armies. An arbitrary per-character formula engine adds complexity without an immediate gameplay need.

### 5. Replace wave switches with an authored catalog

Introduce immutable wave definitions with a stable wave number, boss marker, ordered spawn entries and clear reward. Spawn entries contain archetype, level, count, explicit research rank (initially zero) and boss profile identity. Expand entries in stable order once per original roster member. Existing origin/destination ownership and stable remainder redistribution then apply to individual units, including bosses. Do not regenerate boss modifiers or level during transfer.

Starting composition candidates follow below. `S`, `B`, `C` and `M` mean Swordsman, Berserker, Crossbowman and Mage; `4 S1` means four level-one Swordsmen. These are per original player, not per surviving city. They are the concrete starting data for tuning, not claimed winning-balance evidence.

| Wave | Entries |
| --- | --- |
| 1 | 4 S1 |
| 2 | 3 S1, 1 B1 |
| 3 | 3 S1, 2 C1 |
| 4 | 3 S1, 1 B1, 1 C1, 1 M1 |
| 5 | 3 S2, 1 C1 |
| 6 | 3 S2, 1 B2, 1 C1 |
| 7 | 3 S2, 2 C2, 1 M1 |
| 8 | 3 S2, 2 B2, 2 C2, 1 M2 |
| 9 | 3 S3, 1 B2, 1 C2, 1 M2 |
| 10 | 1 S3 boss |
| 11 | 3 S3, 2 C2 |
| 12 | 3 S3, 1 B3, 2 C3 |
| 13 | 3 S3, 2 C3, 1 M3 |
| 14 | 3 S3, 2 B3, 2 C3, 1 M3 |
| 15 | 3 S4, 1 C3, 1 M3 |
| 16 | 3 S4, 1 B4, 2 C4 |
| 17 | 3 S4, 2 C4, 1 M4 |
| 18 | 3 S4, 2 B4, 2 C4, 1 M4 |
| 19 | 3 S5, 1 B4, 2 C4, 1 M4 |
| 20 | 1 S5 boss |

Validate exactly twenty ordered default definitions, positive counts/levels, known archetypes, and one boss with no escorts on rounds ten/twenty. Derive total wave count and allocation count from the catalog rather than preserving `WaveOne/Two/Three` switches. Numerical fixtures can use explicit compact catalogs for isolated transitions, but acceptance strategies and the existing network victory path must exercise the real default twenty-wave catalog. Do not add a player-facing shortened campaign or privileged advance-wave command.

Alternative: derive monster level and count from wave number. Rejected because it prevents the requested mixed-level waves and combines two difficulty curves that should be independently tunable. One boss for the whole team would require shared combat/target ownership, so use one per original allocation in the existing city model.

### 6. Publish level-aware recruitment and upgrade quotes

Recruitment-building levels remain 1 through 5. Resolve each archetype's material/gold price from its original base with the shared 1.35 growth, rounding each positive component to the nearest multiple of five with midpoint ties upward; preserve zero components and validate no positive component rounds to zero. Never compound previously rounded values. Require strictly increasing total prices for the default per-level catalog even if an individual component plateaus.

| Recruit | Level-one equipment price | Food per battle |
| --- | --- | --- |
| Swordsman | 10 metal | 1 |
| Berserker | 15 metal | 2 |
| Crossbowman | 5 metal + 10 wood | 1 |
| Mage | 15 cloth + 5 gold | 2 |

Swordsman metal costs at levels 1-5 are 10/15/20/25/35. Mage cloth costs are 15/20/25/35/50 and gold 5/5/10/10/15. These replace the previous food price examples. Material bases remain tunable within their required resource types and positive-cost/increasing-total rules. Upkeep is a separate quote and is not rounded to fives or multiplied by 1.35.

| Recruitment upgrade | Gold | Wood | Stone |
| --- | --- | --- | --- |
| 1 to 2 | 20 | 10 | 0 |
| 2 to 3 | 30 | 10 | 10 |
| 3 to 4 | 45 | 10 | 15 |
| 4 to 5 | 70 | 10 | 20 |

Costs have no producer, talent, base-level or wave gate. Recruitment captures the selected building's level and the city's research rank. Selling/upgrading that building never changes veterans. Publish resolved current/next prices, upkeep, profiles, maximum levels and all producer outputs for authority, UI and runner use. Research prices and its rank-one/rank-two Blacksmith requirement remain unchanged, distinct from recruit levels.

Alternative: also unlock new archetypes with these upgrades. Deferred because the original suggestion was incomplete and it would combine two power jumps without an agreed mapping. Existing type-specific recruitment choices remain.

### 7. Reward one authoritative shared transition

After damage, eliminations, transfers and admission, resolve all-cities-fallen defeat first. If no living/queued enemies remain and at least one city survives, award that wave's configured reward to every living city before changing phase or incrementing the wave. Keep `LastRewardedWave` and the actual per-city reward result in authoritative state. Require the completed wave to be newer than the reward guard; command retries, repeated stepping and post-combat death cleanup cannot re-enter the payout. A local clear and a stalled defeat are not successful clear transitions.

Retain only the latest reward receipt (match/wave, recipient IDs and amounts), not an unbounded reward event log. Snapshot it together with resources. That gives reconnect and victory views the same result without asking clients to infer resource deltas, which may also contain later purchases or production. Clients show the latest summary for the observed city and suppress historical payment effects on initial/reconnect baselines. A dead city has no receipt for a clear it did not survive. A fresh match has no receipt.

Normal rewards are 10 gold/5 food/5 wood; waves ten and twenty double those values. A city surviving all rounds receives 220 gold/110 food/110 wood in total. Production remains separate: three ordinary production turns and an income-free preparation stage before each wave. Granting the final reward does not create another build phase or production.

Alternative: pay on each kill or when a city's local enemy count reaches zero. Rejected because attribution and redistributed enemies complicate rewards and can cause premature/double payments.

### 8. Integrate complete state and readable controls

Version the wire contract from the completed prerequisite's version. Complete state and ordinary diagnostics must carry all six balances, purchased/locked plot state, building instance/investment and refund quotes, construction/recruit/upgrade catalogs, Market rates, upkeep quotes/results, unit levels/boss identity, total waves and last-clear receipts. Preserve them through pause, reconnect and authority session lifetimes; reject old peers rather than interpreting missing resources, levels or land state as zero/default. Fresh sessions reset purchases, balances, investment, receipts and action identities. Preserve changed-state publication rather than introducing a reliable-channel stream of unchanged previews.

Keep stable groups and ordering for production, recruitment and advanced/defensive buildings. Highlight affordable eligible actions; retain greyed unavailable choices with missing amounts, source building names and relevant phase/ownership reasons. Do not hide a branch until first production or rely on tooltips on disabled controls for its only explanation. On locked plots show land purchase; on purchased empty plots show construction; on buildings show their actual actions plus the exact sale refund. A selected Market exposes stock bundles and gold proceeds. Keep food balance, next-battle demand and the effect of recruitment/food sales visible. Refresh from accepted authoritative state, never optimistic balance mutation or copied arithmetic.

Retain all nine plot anchors, including selectable locked land, and distinguish lock state visually. The noninteractive menu mirrors the fresh five-open/four-locked landscape. Use existing bundled free building/resource props and explicit labels for Stonecutter, Metal Mine, Weaver and Market; retain provenance and do not download assets or replace required animated characters. Update the asset spec's former three-currency restriction. Validate six-resource HUD readability at both supported sizes without reducing the world overview to fit a taller panel. No duplicate numbered plot grid.

Show current/next recruit level, HP/damage, material cost and separate upkeep, plus mixed-level army details, boss status, wave out of twenty and the actual last reward, including victory. Level-two structural decoration persists at higher recruitment levels. Preserve stalled-result wording from the combat predecessor and all animation, camera, death and input-isolation contracts. Use authoritative actual state for temporary battle modifiers/status rather than silently rewriting base profiles.

Alternative: client-derived prices and snapshots containing only balances. Rejected because the existing discount duplication already drifts and cannot explain refunds, expansion prices or upkeep reliably.

### 9. Gate integration on the complete numerical economy

Implementation order is size-based combat and its numerical gate, resource/catalog foundations, land and transaction lifecycle, level/material recruitment and upkeep, authored waves/rewards, then full-run balance, followed by broad transport and presentation. Do not tune twenty waves against the old food recruitment economy and then add stone/upkeep afterward. Basic snapshot fields and cheap serialization checks can land with their owning state changes; the later integration gate covers real processes and UI.

Keep twenty rounds, both boss-only rounds, Swordsman 40/10, shared stat growth 1.35, rounding, boss multipliers, exactly-once rewards, 50% in-kind refunds, resource types/gates, five/nine land scope, six-capacity cells, ordinary size two, boss size six and retained combat semantics fixed. Tune ordinary enemy counts/mixes, starting gold/wood, outputs, positive purchase/upgrade/expansion/trade amounts, per-archetype upkeep, specialist bases and defense candidates within that contract. Record adjustments in this design and gameplay documentation; changing a fixed contract requires an explicit plan revision.

Start with city health 250, built-in defender damage 3, Arrow Tower damage 6/9 and Catapult damage 8/12, retaining action timings. The starting economy table is not a winning-balance claim. Prove a paid, fed opening before wave one, meaningful metal/cloth production, affordability of replacements, stone-funded level five before wave twenty and reasonable food supply through persistent army growth. Compare production upgrades versus additional plots, Market opportunity cost versus Gold Mines, and selling/rebuilding versus continued expansion. Keep the nine-plot limit under scrutiny; if no viable intended specialization fits, revise the documented land scope rather than hiding extra capacity.

Use cheap catalog/core/authority tests for six-resource atomicity and overflow, costs without producer ownership, bootstrap recovery, plot locks/price basis, investment/refund arithmetic, building replacement generations, Market availability/rates, stale/retried actions, upkeep timing/shortage, levels/research, wave composition, transfer and rewards. Preserve seeded traces, crowd/role/entry assertions and unchanged current health. Run checked-in frontline, mixed-army, tower-heavy and research-heavy ordinary-command strategies over a small fixed-seed sample through all twenty waves. Each family must win solo, a shared strategy must win with one through four players, and no-investment play must lose through city-health defeat. BattleStalled never counts as success. Evidence includes every resource, plot purchases, sales/refunds, trades, food demand/payment, army participation, upgrades, rewards, casualties and wave ticks. These samples are not universal balance proof.

Extend existing network `authority-resume-victory` for a real twenty-wave outcome with final boss/reward assertions, representative economic retries and full-state reconnect. Preserve `redistribution` and compare level/boss fields when present; exhaustive boss transfers belong in cheap tests without forcing that slice through ten waves. Update existing runner setup to afford materials and food through ordinary commands, consuming shared quotes. Measure deadlines and maintain bounded cleanup/concurrency.

Extend existing `economy` UI coverage for a locked plot, missing stone explanation, actual expansion/build/sell/Market controls, an upgraded material recruit, an unchanged veteran and upkeep feedback. Extend existing combat/reconnect coverage only as needed to observe actual battle-start food handling and restored current state/rewards. Unique defects are incorrect input targets, stale controls/costs and mismatched rendering; cheap rules tests cannot prove those. Expected extra cost is several ordinary production/build stages, targeted inputs/observations/frames and bounded early battles needed to afford the new graph; determine and record the actual turn/wave budget and measured overhead during implementation rather than assume the former food-only setup still fits. No new expensive scenario or full twenty-wave graphical campaign is admitted. Keep existing melee readability, pause/death and cooperative assertions. In particular, retain the independently selectable seed-1 `combat --checkpoint melee` proof and its live-node/PNG witnesses; replace its obsolete two-Farm/Barracks food-funded six-Swordsman setup with an ordinary equipment-funded, fed opening and document its early-wave stop bound before running the graphical proof. Packed smoke samples the new controls/state from its owned setup after sequential exports; pure formatting/projection permutations stay cheap.

## Risks / Trade-offs

- [Thirteen building types exceed nine plots] -> Preserve specialization, upgrading for output per plot, permanent expansion and half-refund reconfiguration; prove mixed/tower/research paths with ordinary commands before UI integration.
- [Food rewards and farms cannot sustain persistent armies, or upkeep makes food irrelevant] -> Measure demand/payment across all waves; tune outputs/upkeep without restoring food recruitment or introducing healing.
- [Stone gates everything too early] -> Keep basic producers/recruitment and the first recruitment upgrade stone-free; retain gold-only wood bootstrap and test first-wave defense.
- [Market dominates Gold Mines or creates conversion exploits] -> Use frozen one-way bundles, disclose opportunity costs and test checked arithmetic, duplicate requests and building investment refunds.
- [Selling a tower/building leaves effects or stale commands] -> Remove its behavior, retain stable plot identity but change instance identity, and test reconnect/retry/replacement.
- [Size-two support changes crowding and splash opportunity] -> Run the small role/crowd/admission gate first, then retune the full campaign without relaxing capacity or role guarantees.
- [Overlapping combat refactor lands during implementation] -> Record the baseline revision and reconcile action/reservation interfaces before applying either change.
- [Boss action throughput and inherited allocations skew difficulty] -> Measure both rounds and retain original-roster pressure, health and modifiers through transfer.
- [Research resets a veteran to the current building level] -> Resolve each unit from its own immutable level, retaining current health and city research through building sales.
- [Larger state/UI and longer runs increase runtime] -> Keep exhaustive checks cheap, measure selected process slices, preserve owned cleanup and use one full-CI boundary before/after the coherent feature.
- [Predecessor changes are overwritten by complete delta blocks] -> Reconcile against its final synced revision, explicitly retaining death-cleanup readiness, stalled-result feedback, role evidence and changed-state publication.

## Migration Plan

Implement only on a later apply request against the integrated, synced combat prerequisite. Establish or reuse an unchanged full-CI baseline, then follow the dependency order above and in tasks.md. Update gameplay documentation with each owning stage. Bump protocol and rules identity together and replace obsolete three-wave, food-recruitment, universal two-level, three-resource and all-plots-usable assumptions across authority, UI, runner and tests. Preserve historical records and deliberate compact fixtures. Keep locked restore/tool inputs, format changed C# after restore and run full `mise run ci` at the feature boundary with source checks, sequential exports and both package smokes.

Running sessions are not migrated. Roll back economy state, action validation, profile catalogs, protocol and UI together if needed. This planning update renames the change, updates its artifacts and adds the existing ecs-unit-combat capability delta; it neither implements code nor syncs/archives main specs. Validate planning with `openspec validate add-economy-army-and-campaign-progression --strict`; game/export runs are for implementation.
