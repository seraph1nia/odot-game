# Design

## Context

See `proposal.md` for motivation. `World.cs` currently centralizes gold/food, three building types, uniform 20-gold construction/upgrades, and three ready checks immediately followed by combat. A 3-gold mine repays a 20-gold investment in seven productions; farm level two doubles food; production nine cannot be spent. `CityState` and `Rules` feed UI costs directly.

The completed animated-combat work supplies an authoritative Arch world, fixed 60 Hz steps, bounded lane contact, typed unit DTOs, 20 Hz snapshots, buffered events and a shared presentation clock. Its main-spec synchronization and archive completed during this proposal; the deltas now use that refreshed baseline. Current `UnitType.Enemy` conflates role with faction, and deployment/targeting/presentation branch on it. `UnitView` selects Knight/Rogue and sword/crossbow clips; `UnitAssets.Bind` requires the same clip set on both. `Tabletop` creates fixed gold/sack props and scales level-two buildings. `VillageLayout` places every plot at Y=0 and selection intersects a common ground plane.

The real harness already has network `authority-resume-victory`, `redistribution`, `defeat`, `failure-cases`, `solo-session`, `playing-host-lifecycle` and source UI `economy`, `reconnect`, `settings`, `launcher`, `combat`; `exported-package` exercises existing exports. Keep those owners and selectors. Steam invitation work is independent and currently modifies shared UI/runner files.

## Goals / Non-Goals

**Goals:** Keep all numerical behavior in core, make new roles deterministic on every authority mode, keep fractional upgrades modest, expose the whole economy through ordinary controls, and make presentation reconstructible from current state and bounded events.

**Non-Goals:** Terrain collision/navigation, manual orders, physics projectiles, a class counter triangle, an economy ECS conversion, upkeep, automatic healing, persistent saves or mixed-protocol compatibility. No new dependency, engine or SDK version is required.

## Decisions

### 1. Typed catalogs and three resources

Keep city/building ownership as ordinary core state and units in the existing Arch world. Introduce core catalogs for building costs/output/upgrades, recruitment eligibility/costs, archetype profiles and research. Prices are typed gold/food/wood amounts, validated before any mutation; snapshots carry the authoritative catalogs and balances. Avoid duplicated UI switch formulas and client-selected numeric stats. Preserve omitted recruit type as Swordsman, but reject incompatible selected buildings.

Gold buys investments and research; food remains required for every recruited unit; wood pays construction/towers. Do not add resource conversions, upkeep, storage limits or costs for producing resources. A lumbermill must cost zero wood so exhausting wood does not block wood production.

The following values are an implementation tuning seed, not measured balance results. Record intentional changes to these values with strategy evidence; role, resource and progression contracts remain fixed.

| Building | Build gold / wood | Level-one role | Level-two role |
| --- | ---: | --- | --- |
| Farm | 20 / 10 | 5 food per production | 8 food |
| Mine | 20 / 0 | 5 gold per production | 10 gold |
| Lumbermill | 20 / 0 | 5 wood per production | 10 wood |
| Barracks | 20 / 10 | Swordsman / Berserker | Food cost -1 |
| Archery Range | 20 / 10 | Crossbowman | Food cost -1 |
| Arcanum | 25 / 10 | Mage | Food cost -1 |
| Blacksmith | 20 / 10 | Unlock paid rank one | Unlock paid rank two |
| Arrow Tower | 20 / 15 | 2 damage / 60 ticks | 3 damage / 60 ticks |
| Catapult Tower | 30 / 20 | 3 damage / 120 ticks, up to 3 victims | 4 damage, same cadence/cap |

Start with 60 gold, 20 wood, no food, 100 city health and 10 base gold per production. Seed upgrades at 20 gold/10 wood, research rank one at 10 gold and rank two at 15 gold per class. A mine repays its gold cost in four productions; the upgrade repays its gold component in four, with wood as an additional construction-budget cost. Wood payback is a construction budget rather than a fictitious gold exchange rate; evidence must show when its investment unlocks useful spending. Do not force filling all nine slots; competing unlocks/income/towers/research should make plot choice relevant.

Applied tuning: starting wood is 30, preserving the ordinary upgraded Farm/Barracks opening. All four solo strategy families and the one-through-four-player frontline fixtures win with this value; detailed ledgers and results are recorded in `verification.md`. Other seed costs and shared faction profiles remain unchanged.

### 2. An explicit income-free preparation phase

Add `Phase.Preparation`. The flow per wave is:

```text
Building 1 --> production --> Building 2 --> production
     --> Building 3 --> production --> Preparation --> Combat
```

`ResolveReady` grants income only in Building; after its third resolution it clears readiness and enters Preparation. Preparation accepts the same economic actions, then its ready check begins combat without income. Keep displayed production turn at 3 during preparation and label Ready as "Ready for battle". Increment the stale-command stage serial on every editable-stage transition; it is no longer a production counter. Track production count explicitly and assert exactly nine over the match. Guests never infer income from turn labels.

Reuse the existing connected/living ready eligibility, nobody-connected wait, disconnect readiness clearing and pause semantics in both editable phases. Capture `ExpectedPhase` plus stage serial in requests so a retried third-turn Ready cannot accidentally start battle. An additional fourth production was rejected because it lengthens and inflates the economy instead of fixing spending access.

### 3. Separate faction, archetype and class

Replace enemy-specific role checks with an independent faction value. Archetypes are Swordsman, Berserker, Crossbowman and Mage; their classes are Melee, Ranged, Magic. Owner/origin/destination remain existing stable identities, not faction-derived transport ids. Profile lookup is shared across factions and includes research rank. Recruitment cost is separate from combat profile so enemy spawn does not need a fake zero-cost profile.

Seed human-readable profiles as follows; all speeds/ranges use existing numerical lane units.

| Archetype | HP | Damage | Food / gold | Range | Speed | Windup / cadence ticks |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Swordsman | 10 | 4 | 5 / 0 | 0.55 | 1.0 | 12 / 60 |
| Berserker | 8 | 6 | 6 / 0 | 0.55 | 1.1 | 18 / 72 |
| Crossbowman | 8 | 3 | 5 / 0 | 3.0 | 1.0 | 18 / 60 |
| Mage | 6 | 2 | 7 / 2 | 3.0 | 1.0 | 24 / 90 |

Mage splash starts with radius 0.75 and at most three victims including its primary. Both factions deploy melee ahead of ranged/magic and use the same contact/target/recovery rules. Initially all enemy research ranks are zero; they never copy survivor-city research on transfer. Spawn composition from a stable ordered archetype cycle: wave one Swordsman/Berserker, wave two adds Crossbowman, wave three adds Mage. Keep starting allocations 4/6/8 as a tuning seed. Assign archetypes by original-owner allocation index before distributing fallen-owner allocations; redistribution changes destination only, not composition or total pressure. Do not silently add faction-specific stat bonuses while tuning difficulty.

### 4. Research precision and persistent health

Use integer hundredths for unit and city health/damage, including defender/tower damage, with checked conversion helpers and human-unit formatting in the UI. A rank multiplier is `(100 + 5 * rank) / 100` applied once to base values; retain exact hundredth results for the seeded integer base values. Two ranks are additive +10%, not multiplicative +10.25%. Carry the scale contract in DTO/profile fields and update protocol consumers together. Resources remain ordinary integers. This avoids rounding a 3-damage attack's +5% into either no improvement or +33%.

Research ranks belong to the city, separately from Blacksmith slots. Level two unlocks rank two; additional Blacksmiths do not create independent bonuses. Accepted research updates allied units of that class without changing identity, current health, body, target or timing deadlines. New recruits start at their researched maximum health; surviving wounded units are not healed. Since purchases cannot occur during combat, no in-flight attack needs a research recalculation policy. Unknown class, capped rank, missing unlock or insufficient resources rejects atomically.

### 5. Towers and bounded splash share impact resolution

Keep the built-in defender independent. Add one core tower combat state per built tower, identified by city/slot with a monotonic attack sequence. It has target, impact and recovery deadlines but no ECS collision body. Towers select deployed enemies anywhere in their city's lane; position remains cosmetic, so a player cannot accidentally build a useless out-of-range tower on a rear plot. They cannot be individually attacked and stop with city elimination.

Mage/catapult impact validates its locked primary first. On success, gather opponents in that destination around the primary's impact position, keep the primary, then distance/ID-sort secondaries and cap the victim list. Apply damage to that list once through the shared simultaneous-damage accumulator. On primary death/transfer/range invalidation, emit a miss and no secondary damage. Catapult seed windup is 30 ticks, splash radius 0.75 and victim cap three; arrow windup is 12. An enemy Mage attacking exposed city health applies one normal primary hit, without secondary unit splash. Combat events explicitly distinguish unit and tower sources and carry impact position and bounded victim information. Do not fabricate tower-as-unit records. Existing transfer cancellation, death precedence and defeat-over-mutual-kill outcomes remain mandatory.

### 6. Verified free models and terrain with correct selection

Reuse the currently pinned free pack subsets; vendor only additional needed files, buffers/textures and licenses from official immutable sources. The exploration verified free Barbarian/Mage, Skeleton Warrior/Minion/Rogue/Mage, blacksmith, archery range, church, lumbermill, tower/catapult, mountains, slopes, bridge, grain, targets and props in official inventories. Those listings establish availability, not a successful Godot rig import. Check each selected GLB's required role-specific clips and sockets during implementation. No paid hulking Barbarian, Engineer/Druid, Skeleton Golem/Necromancer, food/coin extras or source files are needed. Keep asset versions stable unless a documented missing free clip requires a specific alternative.

Replace the universal animation requirement with role bindings: shared idle/walk/run/hit/death, plus sword, axe, shot or cast clips and measured impact markers. Preserve AnimationTree composition, root-motion suppression, clock alignment and bounded corpse cleanup. Supply skeleton-specific weapons and restrained faction markers. No fallback dummy is acceptable for a required character.

Represent plot positions as horizontal coordinates plus authored terrace height. Keep the combat approach flat or gently shaped through a pure graphical ground-height mapping, used consistently by live units, corpses and projectiles. Scenery never changes numerical lane geometry. Replace flat-plane plot picking with per-plot ray/surface intersection; include building AABBs, selection rings and camera bounds at actual height. Check roof/plot selection on all nine plots after upgrades/reconnect. Put bridges and dense props outside the lane; preserve its grassy readability.

Level-two buildings get added props or structures: scaffolding/annexes, weapon racks, extra grain/logs, tower attachments and flags. Use scene-local pivots for windmill blades; if a free model is monolithic, animate only a verified separable mesh or use the free windmill/prop combination without distorting the building. Ambient presentation can use restrained procedural movement; it never changes core positions.

### 7. Stockpiles and local feedback lifecycle

Replace static props with a bounded stockpile presenter per resource. Seed thresholds 0, 1-19, 20-49, 50+ and arrange 0/1/3/6 objects; tune these thresholds for readability without changing resource rules. Use Resource Bits gold/wood and medieval food sacks/grain. Update only on tier changes; labels show exact values. City focus/reconnect reconstructs current tiers without past earning effects.

Action puffs/chimes key on accepted match/command sequence, after acknowledgement. Reuse combat playback's deduplicated cursor and clock for impacts, sparks, bolts, skeleton rattles and tower projectiles. Only the observed city emits combat sounds; focus changes do not drain a backlog. Add a bounded graphical effect pool and at most eight concurrent effect voices. Drop excess cosmetic voices, never authority events. Pause/loss freezes visual clocks and stops transient voices; resume starts only new sounds. Session replacement frees pools, clocks/cursors and ambience.

Use small locally synthesized PCM cues with documented oscillator/envelope/noise parameters, avoiding another asset pack or unclear sound licensing. Keep code in presentation, and route all audio through existing Master. Background music retains its player, position and authored loop. No new settings fields/sliders are needed. Silent automation verifies routing/mute/voice counts; listening quality remains a manual observation.

### 8. Verification by regression value

Run full CI before and after implementation; reuse a before baseline only if source/environment inputs match. During planning, run spec validation only. During implementation, frequently run `mise run test`; select affected network/UI cases, and never mutate shared import/export outputs in parallel.

Cheap tests cover atomic gold/food/wood spending, zero-wood lumbermill access, nine productions plus three preparation stages, rank precision/no healing/no stacking, faction parity, mixed formation/contact, splash cap/order/misses/friendly exclusion, tower clocks/elimination, snapshot/retry guards and effect replay baselines. Extend `WorldTests`, `CombatTests`, `CombatPlaybackTests`, `AuthoritySessionTests` and lightweight runner policy tests. Do not repeat whole battles to test an individual cost option.

Add checked-in ordinary-command strategy fixtures for frontline, mixed army, tower-heavy and research-heavy play. Each family must have a winning default solo strategy; exercise the shared winning strategy for 1-4 players and retain an empty-city loss and real redistribution. Record finite strategy coverage, purchases, resource ledger, casualty counts, city health and wave duration in `verification.md` during apply. These are measured candidate strategies, not exhaustive balance proof.

Extend `authority-resume-victory` for preparation/current resources/research and `redistribution` for specialist faction/profile retention. Extend `economy` for wood costs/pile changes, raised plot/roof picking, specialist building/research controls and visual upgrades; extend `combat` for Berserker/Mage/skeleton bindings and tower/splash effects; reuse `reconnect` for replay baselines and `settings` for mute. Preserve normal selectors/input and one ordered child driver. Use real current node observations rather than success flags. Target additions of roughly 10-20 seconds to economy and 15-30 seconds to combat, measure actual execution/setup costs, and keep bounded waits. If natural ordinary play cannot expose a required render defect within these slices, admit the smallest separate selectable slice with a documented risk/cost rationale before adding it.

These graphical extensions catch missing rigs/cast clips, elevated pick targets, invisible stockpile changes, scaled-only upgrades, stale effect/audio replay and packed omissions that core tests cannot see. Package smoke adds short asset/binding/resource checkpoints, not another full graphical match. Keep XDG/session/endpoints/displays owned and cleanup awaited; retain non-secret JSON/log/PNG evidence under ignored logs. Source gates still precede sequential client/server exports and both package smokes.

## Risks / Trade-offs

- [More unlock buildings compete with only nine plots] -> Budget ordinary strategy fixtures before freezing defaults; preserve specialization and avoid a mandatory building for every role in every strategy.
- [Symmetric skeletons increase current wave strength] -> Tune composition/counts or shared profiles with evidence, never faction-only discounts.
- [Fractional health migration affects every combat consumer] -> Explicit scale contract, coordinated protocol bump, round-trip and boundary tests, human-readable UI/probe values.
- [Extra ready stage produces accidental duplicate income] -> Separate preparation phase and stage serial, authoritative production count, retry/disconnect tests.
- [Splash multiplies event load] -> Fixed victim cap, bounded event history/pools and observed payload/voice budgets; retain event-gap recovery.
- [Decorations obstruct picking or float units] -> Per-surface rays and ground-height mapping, representative rendered captures and all-nine-plot checks.
- [Concurrent spec synchronization changes copied requirements] -> Refresh predecessor specs and preserve their complete scenarios before final validation; do not rewrite or archive the predecessor here.

## Migration Plan

1. Confirm the synced/archived animated-combat baseline still matches the current main specs; this plan's deltas preserve its scenarios. Preserve unrelated Steam files.
2. Record the before CI baseline, implement catalogs/faction/precision/preparation with cheap checks, and bump the next available protocol version with all DTO/automation/UI consumers. Old and new processes are incompatible; no active match or credential migration is promised.
3. Integrate tower/splash/research rules, ordinary strategy fixtures and recorded tuning. Then vendor/prove free assets and build terrain, controls, stockpiles and feedback.
4. Run affected selectable checks, format changed C# after locked solution restore, then final full CI and record timings/limitations. Update gameplay and verification docs; packages remain local.
5. Rollback reverts this coordinated code/protocol/asset subset and rebuilds matching client/server packages, preserving unrelated work and developer preferences. No publishing or deployment is part of this change.
