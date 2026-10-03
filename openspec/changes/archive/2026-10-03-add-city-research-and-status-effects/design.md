# Design

## Context

See `proposal.md` for motivation and the delta specs for the behavior contracts. `World.cs` currently stores city `ResearchRanks`, exposes rank quotes, routes `research` through selected-slot/generation validation, and recalculates existing unit profiles. `Catalogs.cs` declares a two-level Blacksmith; `EconomyConfiguration.cs` freezes its prices and two gold-paid rank quotes. There are three production resolutions per wave and a separately guarded shared-clear reward with preparation-time overflow preflight. Six material resources and nine plots already provide the investment constraints.

Combat lives in one Arch-backed authority. `CombatUnit` separates profile, location, action and decision state; `CombatActions` gives committed moves and attacks fixed positive deadlines. `CombatSimulation.Advance` completes arrivals/recovery and accumulates due impacts before casualties. Transfers cancel old actions and reservations while retaining health and remaining recovery. `CombatPlayback` baselines reconnect/history gaps and samples a common snapshot clock. `TabletopHud`, `Tabletop`, `UnitInspector` and `ProgressionPresentation` currently expose contextual class-rank controls and authoritative profiles. The CLI input path in `Main.cs` and `CampaignStrategy.cs` also construct the old rank command. `WireJson.ProtocolVersion` is currently 8; gameplay payload changes require compatible admission.

The in-progress `optimize-core-simulation` change is editing many of these files and developing stage-owned actor observations, profile/quote caching and playback indexes. Apply must reconcile against the actual implementation at that time; this plan does not change its artifacts or claim its measurements cover new status work.

## Goals / Non-Goals

**Goals:** Keep personal research and capabilities authoritative, use exact bounded integer arithmetic, make temporary effects independent of permanent purchases, preserve existing movement reservations and seeded decision rules, and expose enough current state for headless verification and graphical restoration.

**Non-Goals:** A generic scriptable ability engine, event-bus damage middleware, player-directed actions, respec, external content/dependencies, or redesigning the existing ECS/process harness. Freeze/stun need a later interruption design; no field or UI in this change promises them. The first tree has unit-class branches only, rather than adding healing, armor penetration, defense ammunition or producer bonuses without their supporting rules.

## Decisions

### 1. Research is a dedicated city state and frozen finite catalog

Add a small research rules/state module under `src/Game.Core`, with typed stable technology identifiers, tier, prerequisites, exclusive group and eligible unit types. Store owned identifiers and the integer point/progress balance in the city; derive choice locks from purchased identifiers so a second mutable choice ledger cannot disagree. Expose detached catalog/state projections and authoritative eligibility/rejection reasons. Validate uniqueness, prerequisite acyclicity, exclusive sibling reachability, prices and effect parameters at match construction.

Keep research separate from `ResourceCost` and `Resource`: it cannot be sold at Markets, counted in refunds, substituted for equipment or accidentally added to six-resource production. Use pure quote/eligibility and contribution calculations, then commit through the existing authority path. Compared with adding a seventh resource or arbitrary effect callbacks, this retains a small auditable domain model. Unknown technology ids are invalid, never a default foundation.

Numerical planning defaults below are explicit first-catalog candidates, not claims of proven balance. They may be tuned during apply using cheap paired/campaign evidence without changing the agreed 1-point baseline, +1/+2 tower income, 3/6/9 tier prices, exclusivity or bounded effect semantics.

### 2. Preserve economic building identity while replacing its role

Replace `Building.Blacksmith` with `Building.ResearchTower` at the same explicit enum value; preserve every other building value. Do not introduce JSON enum aliases or a second legacy purchase path. Keep the current construction quote (20 gold, 10 wood, 10 stone by default), upgrade quote (20 gold, 10 wood, 10 stone), maximum level two, purchased-plot rules, investment accounting and generation guards. These inherited prices are a planning default, not a new economic requirement.

Reuse bundled medieval tower/advanced-building pieces with a distinct Research Tower name and structural level-two treatment; the renderer must not interpret it as an Arrow/Catapult defense. Remove rank purchase quotes and Blacksmith-level gating. A global Research control enables towerless purchases; selecting a tower still shows its upgrade, sale and output. This replacement avoids keeping two research currencies and two mandatory research buildings.

### 3. Accumulate thirds at production, award baseline points at clears

Represent fractional production progress using integers: `total = previousRemainder + sum(tower.Level)`, `earned = total / 3`, `remainder = total % 3`. Check intermediate and point addition bounds before committing all living cities. One full cycle yields +1/+2 points for a level-one/two tower; mixed towers and upgrades use their actual levels at each resolution. City-wide carry prevents sale from erasing legitimately earned partial progress; construction and sale do not grant progress.

Include research in `ResolveReady` production preflight and preparation reward preflight. Shared clear adds exactly one point through `LastRewardedWave`, with a research field in each latest-clear receipt; boss multiplication continues for material rewards only. Fresh matches start at zero. Disconnected living cities follow ordinary production/reward rules. Compared with paying tower income at the end of battle, production accumulation avoids last-minute construction receiving an entire cycle's benefit and allows earned points to be spent in preparation.

### 4. Small exclusive tree, with immediate capabilities

Use these fifteen stable nodes, with one foundation and two paths per class:

| Foundation (3) | Specialization (6) | Advanced node (9) | First numerical candidate |
| --- | --- | --- | --- |
| `melee-foundation` | `guardian` | `guardian-mastery` | Incoming damage reduction 10%, replaced by 20% at mastery |
| `melee-foundation` | `assault` | `assault-mastery` | Direct damage bonus 10%, replaced by 20% at mastery |
| `ranged-foundation` | `venom` | `venom-mastery` | Poison potency 10%, replaced by 15% of source direct damage per stack tick |
| `ranged-foundation` | `precision` | `precision-mastery` | Direct damage bonus 10%, replaced by 20% at mastery |
| `magic-foundation` | `fire` | `fire-mastery` | Burn potency 20%, replaced by 30% of source direct damage per tick |
| `magic-foundation` | `frost` | `frost-mastery` | New action durations +20%, replaced by +40% at mastery |

Each foundation grants +5% maximum health and direct damage to its class. The shared first prerequisite is not bought twice. Sibling specializations and their descendants are permanently locked after choice; different classes remain independent. Advanced nodes replace the earlier benefit rather than add another copy. A selected path costs 18 points; towerless full survival supplies 20 baseline points over the match, enough for one complete path. A continuously active level-one/two tower would add 20/40 points, but real late construction earns less. This arithmetic is a pacing check, not proof that research-only armies survive.

Derive an immutable `UnitCapabilities` value from purchased nodes and unit type. Resolve base level/boss scaling, the foundation factor and any direct-damage bonus in declared order with the existing hundredths scale and checked wide intermediates; recompute from bases instead of repeatedly mutating the last profile. Guardian reduces each incoming direct or periodic contribution once, using upward rounding of the remaining amount, so small positive hits remain positive. Technologies do not heal or level veterans, alter upkeep or affect defenses. Remove city-owned paid ranks; legacy explicitly authored enemy ranks may remain numerical profile inputs but cannot purchase or inherit the player's tree. Enemy default waves and boss rules remain unchanged.

### 5. Route purchases independently of building slot validation

Replace the class/rank input with a technology-id purchase action, for example `research-tech <technology-id>` in the checked-in child command interface. Process it after owner/match/stage/pause/alive/ready guards and before slot validation. Reject ambiguous legacy `research <slot> <class>` requests under the new protocol. Preserve the existing stable-player `CommandLedger` retry behavior; check all point, node and profile changes before committing them atomically. Recruitment receives the owning city's derived capabilities, and a successful purchase refreshes every existing eligible living unit without changing its current health.

Do not tie purchases to a tower's generation: selling every tower removes production, not the ability to spend baseline points. Existing build/upgrade/sale/trade commands retain their generation checks. Adapt `Main.SendAction`, automated command parsing and the shared ordinary-command campaign policy together so no hidden legacy bypass survives.

### 6. Bounded status values, independent from permanent capabilities

Extend authoritative combat units with immutable bounded status state rather than building a second actor store. Burn is one record; poison is at most three records; chill is one record. Use captured potency and source/action identity, applied tick, absolute expiry and next periodic tick. A stack identity includes original application identity and remains stable when refreshed. Captured damage survives source removal; no live attacker lookup is needed for periodic ticks.

Initial candidates are burn period 60 ticks/lifetime 180 ticks, poison period 60/lifetime 360 ticks and cap three, chill lifetime 180 ticks. Compute periodic potency from source resolved direct damage at application using upward-rounded integer percentages; a zero-damage source grants no periodic effect. Burn refresh uses maximum potency, later expiry and unchanged next tick. Poison under cap adds a stack; at cap refresh the earliest expiry/stable identity with maximum potency and unchanged next tick. Chill refresh uses maximum duration penalty and later expiry, capped at 50% additional duration. Sources cannot create an unbounded set of per-source burn or chill instances.

At application tick `t`, periodic ticks begin at `t + period`; ticks exactly at expiry resolve before removal. Repeated hits never reset the pending periodic deadline. Canonically sort pending applications by victim id, effect kind, source id and attack identity before merging; this makes cap/refresh behavior independent of ECS traversal. No periodic tick causes another on-hit application. Compared with component-per-stack entities or arbitrary callback handlers, bounded values simplify projection, cleanup and deterministic tests.

### 7. Preserve same-tick and committed-action semantics

Keep the existing mutation stages, inserting status work explicitly:

```text
Complete due arrivals/recovery and release expired corpses
  --> Gather due periodic damage and validate due attacks from common state
  --> Accumulate damage, including declared per-victim damage reduction
  --> Apply health changes, process terminal expiry, commit statuses to survivors
  --> Mark casualties; resolve elimination/transfers/admission
  --> Resolve normal outcomes and unchanged progress limits
  --> Sample chill; select and commit new compatible actions
```

An attacker dying from poison that tick still contributes an already-due valid impact. New statuses create no immediate periodic damage; doomed victims receive no new scheduling. Track actual periodic health reduction in the destination's engagement progress; refreshes/chill do not extend stall allowances. Expand configuration safety bounds for maximum modified direct damage, one burn/three poison contributions, reduction arithmetic and maximum chilled approach/cadence. Reject unsafe profiles before charging purchases. Do not weaken existing no-progress or maximum-wave limits.

For chill use `ceil(baseTicks * (100 + penalty) / 100)` for new move, windup and recovery durations separately. Committed intervals never change on apply/expiry; movement admission therefore needs no new reservation shape or pausable transit state. Existing action-sequence and ready-deadline projections stay authoritative. This boundary-sampled slow is intentional; immediate mid-move slow or interruption would require substantially different occupancy contracts.

### 8. Transfers, wave cleanup and complete projections

Transfers retain absolute status deadlines and captured strengths while cancelling old actions through the existing transfer policy. An already afflicted queued reinforcement continues status ticks in its current destination; it is still not targetable, cannot act and owns no space. Process those periodic ticks from bounded internal affected-actor state without treating queued units as deployed attack candidates. Re-admission never schedules a second tick. Death removes active scheduling and leaves only final evidence needed by retained events; source death does not purge another target's captured effects. Wave resolution clears survivors' temporary effects without health restoration; corpse cleanup applies no post-result damage. New unfed reserves begin clean, as do fresh recruits and fresh matches.

Include point/remainder/purchases, detached technology catalog and authoritative eligibility in city/match projections; include derived capabilities, active status records and committed effective timing in unit projections and relevant events. Carry final periodic casualty state so a tick between snapshots is visible. Keep bounded sequenced history and current-state baselining; event history never serves as restoration input. Extend projection/restoration validation at existing fixture boundaries and ensure nested arrays do not alias live state or another snapshot.

Technology costs and tower output enter the economy configuration identity; capability eligibility and numerical combat effects enter the combat identity, so changing prices alone does not reroll combat ties. Protocol admission changes with the payload; update ENet and Steam metadata/tests through the existing shared version constant. There is no live match, disk-save or cross-version migration: old peers are refused before gameplay binding, and all new sessions begin fresh.

Refresh profile/recruitment quote caches on technology changes even at the same simulation tick. Status applications/expiry invalidate status-dependent projections and new-action timing without rerolling unchanged target/route episodes or invalidating unrelated cities. Reuse stage-owned observations and canonical candidate ordering from the optimization change; do not key invalidation only by tick. Bounded per-unit statuses avoid global attacker-to-victim scans.

### 9. Present a research panel and current status evidence

Create a focused research panel module in `src/Game` rather than expanding the already broad `Tabletop.cs` with every node handler. Use the existing application theme, scrolling and modal/input conventions. The Research control is available without a world selection, reads the observed city's state and enables only eligible owned purchases. Keep locked alternatives visible, including the permanent sibling-lock text on the purchase action; no additional permission dialog is needed. Show points and a 0/3, 1/3 or 2/3 progress value, actual per-production tower contributions, tier prices and acquired role effects. The panel itself does not pause the match.

Extend `ProgressionPresentation`/`UnitInspector` with actual capabilities and current effect summaries, rather than claiming abilities from character type alone. Draw restrained code-native status badges/particles using the shared sampled tick, preserving rigged animation, Roman levels and bars. Freeze indicator countdowns on pause/loss and reconstruct active indicators on reconnect/focus changes. Do not replay application flashes/sounds from baselined history or modify authority health from VFX. Keep native resource names and asset provenance; compose Research Tower levels from already vendored pieces.

### 10. Verification targets defects by cost

Cheap xUnit coverage owns income arithmetic, overflow/atomicity, catalog validation, branch locks, owned/foreign/paused/stale commands, retries, mixed-level wounded veterans, status refresh/caps/terminal ticks, source death, poison-plus-impact ordering, chill commitment, transfer queues, wave reset, reversed insertion, snapshot isolation and playback baseline. Exercise local/playing-host/dedicated command paths with the existing authority/session fixtures. Preserve all ordinary twenty-wave strategy families, seeds and cooperative sizes; update the research policy to earn points and buy an actual path through ordinary commands. Existing role comparisons still run without specializations to protect baseline identities. Add small paired researched-role fixtures for effective periodic damage and incoming-damage/control benefits; disclose equipment/upkeep and avoid exhaustive branch matrices.

Use existing source scenario names and owners. Extend `redistribution` or `authority-resume-victory` only to witness transported research/status state, transferred deadlines and exactly-once receipts the in-process fixtures cannot prove. Extend `combat` with one independently selectable `research` checkpoint; default combat includes its assertions, and the existing melee checkpoint remains intact. Reuse ordinary command setup and shared campaign advancement to earn points and unlock Fire; never inject a debug balance or external probe. That checkpoint owns one actual-input purchase, visible Frost lock, active burn indicator and fresh inspection/PNG evidence. Expected additional setup is a bounded early-wave research opening (at most wave ten); declare a 120-second setup bound and 30-second feature assertion bound before admitting it, measure actual cost, and reuse already completed setup in default combat where possible. No additional complete campaign, new UI source id, display owner or branch/status graphical matrix is justified. Other effects' exact arithmetic and restoration use cheap tests.

If current cheap coverage already proves a network concern, reuse it rather than adding an extra process case. Update scenario risk descriptions and keep one ordered child driver, fresh observation ids, speed-one graphical barriers and owner cleanup/evidence conventions. Full before/after CI is an apply-task boundary; planning runs only OpenSpec/document consistency checks. Successful targeted checks are reused until relevant inputs change.

## Risks / Trade-offs

- [Technology details were brainstorming examples] -> The fifteen-node first catalog and inherited tower prices are explicit planning defaults; review them as a finite design rather than a commitment to every future tree idea.
- [Research spending competes with survival through plots] -> Keep 1/+1/+2 income and 3/6/9 prices fixed, preserve the ordinary opening, and record research strategy points, tech acquisition waves, casualties and final outcomes before making balance claims.
- [Damage-over-time or slow makes a branch universally preferable] -> Use small paired default-profile fixtures, capped effect state and finite campaign evidence; tune numerical effect candidates rather than silently changing enemy waves or the agreed research economy.
- [Cached projections miss purchases/status changes] -> Same-tick mutation/isolation regressions and stage-specific freshness; retain ownership boundaries of the optimization change.
- [Poisoned queued transfer breaks bookkeeping] -> Continue periodic processing by current destination with untargetable queued lifecycle; test death-before-admission and final-wave/defeat precedence.
- [HUD growth obscures city controls or input] -> Separate scrolling panel, stable selector names, two supported viewport checks and one small owned graphical checkpoint.
- [Graphical research setup adds recurring cost] -> Explicit setup/assertion bounds, reuse existing advancement and no extra full campaigns; disclose measured incremental timing and partial coverage.

## Migration Plan

1. At apply start inspect/reconcile the current working tree and active optimization changes, then record a successful eligible full-CI baseline. Keep unrelated edits and locks intact.
2. Implement frozen catalogs, city income/state and protocol/command changes with cheap authority tests; adapt ordinary campaign policy and all legacy consumers together.
3. Implement pure bounded status policies, effective profile/action timing and combat stage integration; verify lifecycle, reversed-order and projection regressions before graphical work.
4. Implement research/status presentation and the justified selectable checkpoint; run affected source slices, locked restore/C# formatting and the required final full CI.
5. Update gameplay and verification documentation with actual evidence and limitations; validate planning/implementation consistency. Do not archive, publish or deploy as part of apply.

Rollback reverts this coherent gameplay/protocol change and its fixture adaptations together, preserving unrelated optimization work. Running old and new peers together is unsupported; restarting fresh matched-version sessions is the compatibility boundary.
