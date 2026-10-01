# Tasks

## 1. Baseline and coordinated contracts

- [x] 1.1 Refresh the completed animated-combat main specs after the user's sync, preserve all current scenarios in this change's deltas, and verify `openspec validate add-village-character-and-strategy --strict`; preserve unrelated Steam invitation edits.
- [x] 1.2 Record a full before `mise run ci` baseline in this change's `verification.md`, or document reuse of an unchanged successful source/environment baseline; report missing user-managed prerequisites without installing them.
- [x] 1.3 Add faction/archetype/class, resource cost, catalog, research-rank, preparation and tower/event DTO contracts; bump the next available wire protocol and update automation consumers together; verify snapshot/command round trips and stale-version rejection with cheap tests.
- [x] 1.4 Introduce checked integer-hundredth health/damage helpers and human-value formatting across unit/city/defender profiles; verify +5%/+10%, simultaneous fractional damage, bounds and overflow validation through `mise run test`, and document the scale contract beside the DTOs.

## 2. Economy and usable production

- [x] 2.1 Implement wood grants/output, nine building catalogs and atomic gold/wood construction/upgrades; verify insufficient-resource/ownership/slot rejection, zero-wood lumbermill access and configured output using cheap core/authority tests.
- [x] 2.2 Implement the income-free Preparation phase and separate production count/stage serial; verify exactly nine productions, three preparation checks, spend-before-battle, stale ready, pause/disconnect/nobody-connected behavior with core tests.
- [x] 2.3 Implement building-specific recruitment and reduced positive food costs, including Mage supplemental gold; verify every valid role/building pair, mismatches, defaults, retries and local/guest parity using cheap tests.
- [x] 2.4 Adapt ordinary runner ready/recruit drivers to the catalog and preparation phase; verify `mise run test` and selected `mise run test-network --scenario authority-resume-victory`, preserving owned processes and cooperative assertions.
- [x] 2.5 Update `docs/gameplay.md` for gold/food/wood, unlock buildings, preparation and costs; verify examples against catalogs and a normal-command opening fixture.

## 3. Symmetric roles and research

- [x] 3.1 Replace enemy-role branches in core combat with independent faction/archetype/class; add Berserker/Mage and mixed formation placement on both sides; verify equal-rank profile parity, 32-versus-32 contact/progress and reversed-storage determinism with cheap tests.
- [x] 3.2 Implement class research with two additive 5% ranks, Blacksmith level gating, persistent city ranks and unchanged existing current health; verify no healing, no duplicate-building stacking, capped ranks, future recruits and atomic retry behavior through core/authority tests.
- [x] 3.3 Implement deterministic archetype composition by original allocation index and explicit enemy ranks; verify original-roster totals, future allocations and transfer retention of role/rank/health/recovery using cheap tests and selected `mise run test-network --scenario redistribution`.
- [x] 3.4 Update combat/gameplay documentation with four roles, classes, shared faction rules and research; verify displayed human stats and examples match the authoritative profiles.

## 4. Splash and towers

- [x] 4.1 Implement capped Mage splash with primary validation, same-destination opponent filtering and distance/ID ordering through the simultaneous-damage accumulator; verify caps, ties, friendly/queued exclusion, dead/transferred primary misses and mutual casualties with cheap combat tests.
- [x] 4.2 Add Arrow/Catapult tower state, independent stable attack identities, city-wide targeting, timed impacts and upgrades while retaining built-in defense; verify pause, recovery, elimination, simultaneous damage and no unit-body interference with cheap tests.
- [x] 4.3 Extend bounded combat events/playback for tower sources, impact positions and victim lists; verify DTO round trips, overlap deduplication, history-gap baselines and payload bounds with cheap playback tests.
- [x] 4.4 Document tower/splash rules in `docs/gameplay.md` and verify a checked-in ordinary-command tower opening can reach a real impact without privileged state changes.

## 5. Measured balance

- [x] 5.1 Add reusable checked-in ordinary-command frontline, mixed-army, tower-heavy and research-heavy strategy fixtures; verify each completes a default solo match within bounded steps and record resource ledger, casualties, health and wave duration through cheap tests.
- [x] 5.2 Tune the design's seed costs/output/shared profiles/compositions to make all four strategy families viable, preserve a no-investment loss and successful shared strategies for one through four players; verify finite strategy coverage with `mise run test` and record measured changes in `verification.md`.
- [x] 5.3 Refresh normal network scenario spending plans for the measured defaults without removing elimination, future-pressure, host lifecycle or retry assertions; verify affected selectable network cases and publish the ordinary winning examples in `docs/gameplay.md`.

## 6. Free assets and animated bindings

- [x] 6.1 Vendor required free Barbarian/Mage and four Skeleton models, weapons, textures/buffers and licenses from immutable official sources; verify hashes/free-tier membership and update the asset manifest/README without replacing unrelated provenance.
- [x] 6.2 Add role-specific animation/socket bindings and measured sword/axe/shot/cast markers, preserving root-motion suppression and death cleanup; verify each real imported rig/clip through the checked-in binding command and existing combat UI observations rather than temporary scene probes.
- [x] 6.3 Vendor the free additional buildings, tower/catapult, slopes/mountains/bridge, building props and Resource Bits wood subset; verify clean offline import and complete manifest relationships for every selected resource.
- [x] 6.4 Update `UnitView` and tabletop unit observation for faction, archetype, class and fractional human health; verify actual imported poses/materials and no silent dummy fallback in selected `mise run test-ui --scenario combat`.

## 7. Village geometry and controls

- [x] 7.1 Add terrace/riverbank heights, edge scenery and building-specific dressing with a presentation ground mapping; verify coherent unit/corpse/projectile placement and readable empty/full village frames in existing economy/combat UI cases.
- [x] 7.2 Implement elevated plot/roof ray selection, height-aware bounds/rings and camera framing; verify all nine stable slots and upgraded roofs through actual input, including foreign-city read-only and reconnect checks.
- [x] 7.3 Replace uniform level-two scaling with structural/prop variants for all building types; verify upgrade snapshots and rendered geometry observations in the economy UI case and document asset derivatives in the manifest/README.
- [x] 7.4 Expand contextual construction, typed recruitment and Blacksmith controls with catalog costs/ranks, resource HUD and explicit preparation labels; verify accepted actions and disabled/foreign/ready/paused cases using selected economy UI plus normal protocol assertions, without a duplicate plot grid.
- [x] 7.5 Update `docs/gameplay.md` controls and screenshots from owned evidence; verify documented selection/research/preparation actions match actual selectors and displayed feedback.

## 8. Stockpiles, effects and audio

- [x] 8.1 Implement bounded gold/food/wood stockpile tiers tied to observed authoritative city amounts; verify zero/threshold/saturation and focus/reconnect reconstruction with cheap mapping checks plus economy UI node observations and frames.
- [x] 8.2 Add deduplicated accepted-action puffs/flourishes/cues and pooled battle projectiles/sparks/rattles/tower effects; verify command/event replay, history gaps, fresh sessions and pause/loss cleanup using cheap playback checks plus existing combat/reconnect UI cases.
- [x] 8.3 Add bounded synthesized sound cues/ambience routed through Master and preserve continuous music; verify mute/voice cap/headless absence and no reconnect replay through cheap lifecycle checks and the settings/combat slices; document synthesis parameters and listening limitations.
- [x] 8.4 Add verified windmill pivots, waving flags and restrained ambient motion on the shared graphical clock; verify visible motion and frozen pause/loss checkpoints in existing UI evidence without changing numerical terrain rules.

## 9. Integration and completion evidence

- [x] 9.1 Finalize existing selectable network/UI extensions with attributable current observations and frames, register risk descriptions and record actual incremental costs in `docs/verification.md`; verify one driver per child, fresh observation ids and awaited owned cleanup on failure/cancellation with lightweight harness checks.
- [x] 9.2 Extend existing exported-package smoke with brief specialist/tower/stockpile/audio-binding checkpoints; verify packed assets and presentation without repeating a full graphical battle or implicitly rebuilding packages when the slice is selected.
- [x] 9.3 Perform locked solution restore and format changed C# with `dotnet format Odot.slnx --no-restore`; verify final full `mise run ci`, retaining source gates, sequential client/server exports and headless/graphical package smoke, and record actual results/timings/limitations in `verification.md`.
- [x] 9.4 Review proposal/design/spec/task consistency against final catalogs and evidence, verify strict OpenSpec validation and all required task outcomes, and report local/software-rendered/listening/Steam/platform coverage accurately without publishing or changing developer preferences.
