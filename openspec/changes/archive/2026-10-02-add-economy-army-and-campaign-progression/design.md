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

All construction also costs gold. The catalog below contains the final implementation defaults, exercised by the finite strategy sample recorded below. `G/W/S/M` denote gold/wood/stone/metal; omitted components are zero. Producers directly create resources per production turn with no input conversion. Preserve the existing `Mine` identity for Gold Mine and use a distinct identity for Metal Mine.

| Building | Construction | Maximum level | Level 1 / 2 output or role |
| --- | --- | --- | --- |
| Lumbermill | 20 G | 2 | 5 / 10 wood |
| Farm | 20 G + 10 W | 2 | 5 / 8 food |
| Gold Mine | 20 G + 10 W | 2 | 5 / 10 gold |
| Stonecutter | 20 G + 10 W | 2 | 5 / 10 stone |
| Metal Mine | 20 G + 10 W | 2 | 20 / 40 metal |
| Weaver | 20 G + 10 W | 2 | 20 / 40 cloth |
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

Normal rewards are 10 gold/5 food/5 wood; waves ten and twenty double those values. A city surviving all rounds receives 220 gold/110 food/110 wood in total. Preflight every living city's balances after prospective upkeep against the upcoming clear reward during preparation readiness. An overflow rejects readiness without food payment or formation so resources can be spent before retrying. Normal gameplay cannot mutate stocks during combat, so the checked payout remains representable. Compute all surviving-city payout balances before committing any. Food shortage itself continues to reserve units rather than block readiness.

Production remains separate: three ordinary production turns and an income-free preparation stage before each wave. Granting the final reward does not create another build phase or production.

Alternative: pay on each kill or when a city's local enemy count reaches zero. Rejected because attribution and redistributed enemies complicate rewards and can cause premature/double payments.

### 8. Integrate complete state and readable controls

Protocol-eight RPC snapshot payloads use whole-message Brotli compression and base64, decoded before ordinary snapshot acceptance. Complete state remains unchanged, including welcome/acknowledgment state, quotes and event history; diagnostics still emit normal JSON. The four-city catalog round trip preserves every field and occupies less than one quarter of its plain JSON message size. Bound both encoded input and expanded snapshots to 16 MiB. Preserve reliable channels, three-tick combat publication, immediate changed paused revisions and existing cleanup limits. The rendered three-wave specialist setup exposed a reliable-message backlog: the client kept rendering and handled commands while approximately 350 ticks behind the authority. Packet-level ENet compression did not resolve it and was removed. Whole-message encoding is verified through cheap round-trip/expansion-limit checks and existing network/source/packed paths.

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

## Implementation evidence

- Prerequisite reconciliation at `ad771f7595db3a9db53a0a698bba281644597fcf` (2026-10-02): both `rework-deterministic-hex-combat` and `simplify-combat-math-and-flow` are archived and synced. The live core uses immutable `CombatUnit`/`CombatAction`, atomic `HexOccupancy` restoration, current objective ranking and configured defense splash. Preserve those interfaces and their added main-spec requirements. All overlapping delta blocks retain the synced scenario titles, including death-cleanup readiness, stalled outcomes, roles and protected entry. Clarified the boss requirement's obsolete ordinary-capacity wording to the explicit size-six contract. Strict change validation passed before implementation.
- Baseline `mise run ci` at the same revision failed (76.39s supervised CI; 81.19s including runner preparation). Evidence: `logs/20261002-094116-6cb10d7f/ci-summary.json`. Locked restore, formatting, build, import, 275 gameplay tests, 120 runner tests and five network scenarios passed. `authority-resume-victory` reached ordinary wave-three victory but timed out after 15000ms waiting for excessive-request rejection. Source UI, exports and package smokes were not executed. Task 1.2 remains open; no gameplay implementation began. Apply paused pending guidance on this existing baseline failure.
- Corrected baseline passed full `mise run ci` in 433.27s (437.31s with runner preparation), evidence `logs/20261002-094618-8056a636/ci-summary.json`: 275 gameplay/120 runner tests, all six network cases, all five source UI slices, sequential exports and both package smokes. Source inputs are revision above plus the supervised bounded ordinary-RPC burst fix in Main/NetworkTests. Cheap `mise run test` also passed before CI. No dependencies/tool locks changed.
- Size stage numerical gate: 283 gameplay tests and 120 runner tests passed. `HexPosition` now identifies a fixed `HexAnchor`, while profiles/current hex state/reservations carry Size; six distinct anchors guarantee a free anchor whenever positive-size capacity remains. No shaped masks remain. Ordinary defaults are two, boss identity resolves six, protocol v7/rules v3 fingerprint the owning projection change. Tests cover all size pairs/anchor arrangements, invalid values, mixed 2+2+2 and 1+2+3, boss research/transfer/serialization, atomic 4-size move/death and a blocked boss bypass with no cross-cell pooling. The existing formation queue already scanned all canonical actors and retained skipped ties; protected release prediction now uses size claims through the same atomic index.
- Size-dependent fixture reconciliation: paired ordinary role screen is now nine Swordsmen versus seven ordinary enemies, with identical support identities/seed 123 in both controls. Mage effective damage 1800 versus Crossbowman 1100 over the shared live interval; two secondary hits; clears 373 versus 439; remaining friendly HP 2200 versus 1000 (hundredths). Seven/eight-screen controls ceased to establish two successful clears after the rules fingerprint changed; the nine-screen fixture preserves every role assertion with normal placement and no stat/timing tuning. Cleared-forward ordinary-command transfer witnesses use seeds 1/2/8: old seed 4 no longer clears the receiving city before transfer. Cheap additional seed-0 melee detector needs wave three for its later opposite-side impact; mandatory graphical seed 1 retains a two-wave bound. These are finite fixture witnesses, not universal claims.
- Existing selected graphical combat slice passed: `mise run test-ui --scenario combat`, 33.12s case / 42.21s supervised including preparation; `logs/20261002-100455-4da608ed/`. No added expensive scenario; it uniquely proves imported-rig poses, distinct rendered anchors, route/body clearance and cues/lifecycle at new placements. Expected cost remains the existing ordinary setup and roughly 35–45s execution. Full feature CI remains due at task 10.2.
- Size-stage current-state rendering passed selected reconnect (30.15s case / 38.35s supervised, `logs/20261002-100608-4c4d8e34/`). Mandatory seed-1 `test-ui --scenario combat --checkpoint melee` passed (57.10s case / 65.34s supervised, `logs/20261002-101110-ff674639/`): four overview/close PNGs and 51 current live-node witnesses prove shared near/far simultaneous windup and later opposite-side landed impact. Earlier attempts exposed missing graphical-peer phase synchronization and the former 40s capture allowance; added synchronization and a bounded 65s capture allowance based on measured cost, keeping the two-wave bound and every rendered assertion. No new expensive scenario; final campaign integration will adapt its ordinary material/upkeep opening.

### Six-resource foundation evidence

`mise run test` passes 288 core and 120 runner tests after the size stage. Six-component payments, additions and multiplication validate every component before committing a result; a rejected production transition leaves every city, readiness, revision and production count unchanged. Refund arithmetic sums investment before component-wise flooring. Frozen economy quotes and fixed bundle rates join the published configuration identity. Combat retains its own component fingerprint for seed decisions, so economy-only price changes do not reroll combat. Tests cover mutable projection isolation, economy identity changes, integer JSON amounts, negative/overflow rejection and positive whole Market bundles. New Market execution and building definitions remain subsequent tasks.

### Resource graph and land transaction evidence

The thirteen-building catalog now resolves six direct outputs, stone-funded upgrades, recruitment maxima of five and a single-level Market. All quote/production/trade/expansion values are frozen in `EconomyConfiguration` and included in the published combined identity. Snapshots copy rates, expansion prices, instance generation, actual investment, refund and next-upgrade quote. The graph is stock-based; tests explicitly construct an Arcanum without a standing Stonecutter using stocked stone. The ordinary Catapult opening constructs Stonecutter, Metal Mine and Lumbermill and waits for three real productions, requiring no material grants.

Five/nine plots, four selected count-priced expansions, investment-based half refunds and explicit Market bundles are implemented. Instance-targeted commands reject stale generations. Existing production, army, research and tower assertions remain; authority transaction tests cover solo, host-local, hosted guest and dedicated guest execution with accepted retry deduplication. Readiness, pause, disconnection, elimination, insufficient stock/gold and refund/trade overflow reject without spending. Soldiers/research persist across sales and towers are removed before subsequent combat.

Verification: the core slice `dotnet test tests/Game.Core.Tests/Game.Core.Tests.csproj --no-restore --nologo --filter 'FullyQualifiedName!~OrdinaryStrategiesWinWithinBoundedSteps'` passes 280 tests; subsequently the focused `FullyQualifiedName~BuildingTransactionTests` slice passes all ten, including added eligibility coverage. All 120 runner tests pass. Formatting and `git diff --check` pass. These are partial integration coverage. The last full cheap run still failed eleven cases under the intermediate resource graph; the corrected ordinary Catapult case passes in the slice, while old ordinary campaign strategies remain an unresolved balance gate. They retain their original success/role/research assertions and receive no privileged resources. Their resource/land/army setup must be replaced as material recruitment, upkeep and twenty-wave progression land. No final CI or full twenty-wave strategy success is claimed.

### Army and campaign numerical evidence

The army/economy/campaign core slice passes 53 tests, and all eight size regressions still pass after progression. Immutable unit levels, default 40/10 Swordsman progression, midpoint-up base-derived scaling, fixed boss multipliers and subsequent fractional research are resolved in the core. All four archetypes at levels one through six remain symmetric between factions; enemy level six is supported when safe. Five-level material prices use nearest-five positive components and no food recruitment. City quotes include research-adjusted profiles. Research, rebuilding, sales, formation, actual deaths and transfers preserve level, modifiers and wounds.

Food allocation is a pure forecast plus once-only battle-entry commit after corpse cleanup. Tests cover stronger-first allocation, stable identity ties, skipping unaffordable units, reserves with no claims/screens/actions, return next wave, fed queues paying once, zero armies, disconnected living cities, pause, readiness withdrawal and accepted retries. A paused dedicated-authority reconnect retains resources, profile/level, participation and the wave-tagged receipt; replaying accepted battle-ready consumes no additional food.

The immutable twenty-entry catalog replaces old count/role switches. Tests compare every authored composition, ordered expansion for one through four original players, default bosses, frozen projection isolation and invalid/unsafe definitions. Guarded shared rewards precede progression/victory, include absent survivors and exclude fallen/stalled outcomes. A deliberately isolated twenty-transition test removes enemies to verify 60 productions and gross 220 gold/110 food/110 wood; it is not ordinary-command balance evidence. Final victory has no next food forecast, twenty-first wave, extra upkeep or income. Preparation preflights bounded reward capacity after prospective feeding; overflow rejects readiness atomically instead of wrapping/dropping a payout. This safety decision is reflected in the delta spec.

Full cheap acceptance remains red while existing short-campaign, food-only and old-stat fixtures/strategies are adapted; the last intermediate full run reported 68 failures, 244 passes. Numerical fixtures may use an explicit compact authored catalog for isolated transitions. Default ordinary strategy, network victory and final CI must still exercise the actual twenty-wave data. No universal balance, UI completion or completed feature CI is claimed by the focused slice.

Additional campaign transfer coverage passes all twelve campaign checks: a wounded inherited level-three boss deploys after transfer with a full size-six claim at each movement endpoint, and the next wave still creates five enemies per original roster member (fifteen total) for the sole survivor. Previously resolved food receipts remain unchanged through transfer.


Economy balance evidence uses the shared ordinary-command policy in `tools/DevRunner/CampaignStrategy.cs`: four solo families and frontline co-op with two, three and four players, each at seeds 0, 1 and 123 (21 complete campaigns). All reached wave-twenty Victory with all original cities alive, 60 real productions and paid battle upkeep. Initial stone/metal/cloth were zero. Final equipment outputs are Metal Mine/Weaver 20/40 per production; other producer outputs, enemy waves, ordinary health/damage, boss multipliers, plot prices and Market rates remain as authored. Earlier trials exposed insufficient replacements and late Market construction; the final policies retain more troops and establish Markets earlier.

The policy builds Farm/Metal Mine/Barracks/Lumbermill/Stonecutter on the first five plots, recruits six paid Swordsmen for the first battle, then upgrades producers and the Barracks and purchases actual locked plots for its branch. Mixed adds Weaver/Arcanum/Archery Range; towers adds Arrow/Catapult defenses; research adds Blacksmith and both melee ranks. All retain recurring equipment supply and use quoted Market bundles. After wave thirteen, with at least 120 stone stock and level-five Barracks, they sell the upgraded Stonecutter for its actual half refund and build a Gold Mine on the retained plot. Land, troops and research remain. Recruitment logs verify no food deduction, material charges, later metal/cloth recruits, full-land expansion, producer upgrades, trades and reconfiguration.

A producer upgrade adds one level-one output while saving a plot and gold compared with purchasing another plot and producer, but requires stone. At current rates, a level-one Metal Mine can sell its whole 20-metal output for eight gold versus a Gold Mine's five gold; this consumes equipment stock and requires an additional paid Market. Markets and Gold Mines therefore have different land and supply costs. The receipt/action logs record stocks before/after commands, investment, individual army levels/health, forecast/actual rations, casualties, rewards, city health and per-wave ticks.

This finite sample establishes viable ordinary strategies; it is not universal balance proof. The separate cleared-frontage reinforcement witness uses seed 8 with its funded L2 opening and one weak-city replacement. Earlier size-only seeds 1/2/8 are historical input-specific evidence, not promises for the new authored wave composition. The Mage/Crossbowman role witness uses six ordinary screen units against six ordinary enemies at seed 123, identical identities and the shared live interval. Mage equipment is 15 cloth/5 gold and upkeep two; Crossbow equipment is five metal/ten wood and upkeep one. Both clear normally; effective support damage is 6400 versus 4000 hundredths, clearance 253 versus 283 ticks, and surviving army HP 17500 versus 16000 hundredths. No inflated enemy HP, seeded clustering or altered default profiles are used.


Balance gate completed: `mise run test` passed all 326 gameplay cases (66 seconds), including the 21 complete campaigns and new graph comparison; its initial runner leg exposed seed-zero ordered-capture setup. After ordinary observer investment was added only to that optional detector sample, `dotnet test tests/DevRunner.Tests --no-restore` passed all 120 runner cases (0.509 seconds). Mandatory seed-one and seed-123 openings are unchanged. Full campaign receipt/action evidence is retained in `logs/progression-balance/campaign-strategies-3.trx`; subsequent aggregate gameplay assertions also verify later material recruitment, food conservation, full-land expansion and producer upgrades. The successful seed-eight reinforcement witness preserves cleared frontage, protected first-admission deadline, actual transferred engagement and next-wave completion. No stalled result is counted as a win. All source projects built with zero warnings/errors after the initial client/protocol projection updates. Process/UI verification and final full CI remain pending.


Protocol/runner progression evidence, 2026-10-02: protocol v8 carries six balances, permanent land, instance generations/investment/refunds, leveled profiles, Size/boss identity, all quotes, forecasts/upkeep receipts and authored wave/reward projections. Rules v4 own the new profile formula. The shared ordinary process strategy completed `mise run test-network --scenario authority-resume-victory --timeout-ms 300000` in 183.98 seconds (190.22 including preparation), `logs/20261002-121941-5361d2be/`. This is selected coverage: actual wave-twenty outcome, final boss receipt, paused disconnected typed-army restoration, exact equipment retry, old-peer refusal, roster/ownership/rate checks and fresh authority credential refusal. Fresh retries for plot purchases, sales and Market bundles have since been added and remain subject to the next run/final CI.

`mise run test-network --scenario redistribution` passed in 26.13 seconds (33.14 including preparation), `logs/20261002-123446-23d26713/`. It uses the same ordinary seed-eight investment policy as the cheap cleared-frontage witness and retains typed level/boss/size identity, recovery, original-roster pressure, protected admission, engagement and ordinary completion. Guest B is admitted before launching observer C so the numerical city's identity matches the authored seeded witness. The prior concurrent-admission attempt `logs/20261002-122901-6d684ef3/` was intentionally cancelled after the wrong city-id ordering invalidated that witness; SIGTERM invoked the runner's cancellation handler, awaited owned children and removed owned runtime state. No unrelated endpoint/process/display was touched.

The twenty-wave network path exceeded the old 180-second allowance. Default network budget is now 300 seconds, retaining roughly 63% headroom over the measured path. Source/full CI budget is 900 seconds to accommodate the longer network path followed by source UI, exports and package smoke; full serial source UI is 600 seconds, selected economy/packed UI is 300 seconds and other selected UI remains 180 seconds. Cheap runner timeout, cancellation, child disposal and scheduling tests passed all 120 cases after the budget changes. The actual graphical economy run currently extends the existing slice through early wave-three preparation; its first trial exposed a tower-only observer defeat, `logs/20261002-123933-bc1e67ad/`, and was cleaned up. The revised observer equips ordinary soldiers before selling its Barracks to establish the Catapult on retained land. No resource grants or extra scenario were introduced.


### Completed feature gate (2026-10-02)

All 43 implementation tasks are complete. Full `mise run ci` passed in 640.75 seconds (642.14 overall), `logs/20261002-152630-eb9ef5de/ci-summary.json`, on the uncommitted feature tree based on `ad771f7595db3a9db53a0a698bba281644597fcf`. It passed 333 gameplay/presentation/transport tests, 120 runner tests, all six network scenarios, all five source UI slices, sequential client/server Linux exports, headless package smoke and graphical package smoke. The real twenty-wave authority path took 161.18 seconds; source UI took 309.58 seconds and exported UI 137.69 seconds. Final plot/sale/trade retries, pause/reconnect/process restart, food receipts, levels, ownership and cleanup checks all pass. Earlier incomplete/red records above describe staged implementation and are superseded by this complete gate.

Final integration corrections preserve ordinary gameplay: launcher/package fixtures produce metal before recruitment; launcher uses three production turns and an actual building sale without starting a battle. The readiness observer accepts a strictly later turn when Preparation has already advanced after the Ready acknowledgment, retaining revision/turn guards. Reconnect selects its Barracks at its new plot two, and fresh-session combat waits for settled layout within its existing observation deadline. Reliable complete snapshots use bounded whole-message Brotli/base64 serialization at protocol eight, with exact-roundtrip and decompression-limit tests. No numerical constraints, cooperative assertions or cleanup deadlines were waived. Detailed iterations, seed/configuration identity, costs, frame inspection and limitations are recorded in `docs/verification.md`. Finite campaign samples do not prove universal balance; Linux software rendering does not establish Windows runtime, native GPU/compositor performance, physical input, listening quality or paired Steam behavior.
