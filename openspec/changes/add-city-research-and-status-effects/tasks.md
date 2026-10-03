# Tasks

## 1. Establish the implementation boundary

- [ ] 1.1 Reconcile this change's overlapping files with the current `optimize-core-simulation` implementation and preserve unrelated working-tree edits; verify the resulting source/plan inventory records actual actor-observation, profile-cache and playback ownership before modifications.
- [ ] 1.2 Run and retain a successful full `mise run ci` before baseline, or verify reuse eligibility against unchanged source/environment inputs; record command, result, input identity and ignored `logs/<run-id>/` evidence without treating filtered coverage as a full pass.

## 2. Frozen research catalog and permanent capabilities

- [ ] 2.1 Add typed technology identifiers and a frozen fifteen-node catalog with 3/6/9 prices, prerequisites, exclusive groups and eligible types in `src/Game.Core`; verify cheap catalog tests reject duplicate/unknown ids, cycles, inconsistent exclusive descendants and unsafe numerical definitions.
- [ ] 2.2 Define immutable personal research state with whole points, remainder zero through two and owned nodes; verify cheap tests cover zero fresh state, independent cities, derived permanent locks, purchase affordability and checked income overflow.
- [ ] 2.3 Add a pure technology-to-unit capability/profile resolver implementing foundations and replacement mastery effects; verify mixed-level wounded veterans retain current health/level, future recruits receive the same capabilities, mastery does not double-stack and faction equality holds for equal configured capabilities.
- [ ] 2.4 Freeze research prices/output in economy identity and capabilities/status numerical rules in combat identity; verify cheap identity/isolation tests show price-only changes do not reroll combat configuration and detached returned catalogs cannot mutate authority.
- [ ] 2.5 Document the first tree, exclusive fork rules, numerical candidates and 18-point path in `docs/gameplay.md`; verify its identifiers, prices and resolved benefits match the catalog rather than the removed rank model.

## 3. Research Towers and authoritative income

- [ ] 3.1 Replace the Blacksmith enum/catalog role with Research Tower at the same explicit value, keeping other building identities and existing two-level prices/refunds/generation guards; verify cheap building transaction tests cover construction, upgrade, sale/replacement and absence from defensive actor state.
- [ ] 3.2 Integrate one/two progress units per tower into actual production with city-wide thirds conversion and all-city atomic preflight; verify cheap tests cover full cycles, additive towers, partial construction, between-turn upgrades, retained sale carry, disconnected cities and overflow without partial production/readiness changes.
- [ ] 3.3 Add one research point and receipt evidence to the guarded shared-clear transition and preparation preflight; verify ordinary/boss/final clears, disconnected survivors, no local-clear payment, inherited allocations, defeat precedence and no retry/cleanup double payment using existing campaign/resource fixtures.
- [ ] 3.4 Document Research Tower production and baseline rewards in README and `docs/gameplay.md`; verify displayed +1/+2 per full cycle differs from per-turn progress and research remains separate from all six Market resources.

## 4. Owned purchases, recruitment and protocol

- [ ] 4.1 Route technology-id purchases through owner/stage/alive/pause/ready guards before selected-slot validation and remove the legacy rank purchase path; verify local/playing-host/dedicated authority fixtures accept towerless purchases and reject foreign, stale, unknown, locked, insufficient and ineligible requests atomically.
- [ ] 4.2 Commit point deduction, acquired node and eligible-unit profile refresh together, and apply capabilities on recruitment; verify duplicate ledger retries, same-tick cache freshness, permanent sibling locks, sale without access loss and unchanged wounded veteran health in cheap authority/progression tests.
- [ ] 4.3 Extend city/match snapshots with research balances, progress, detached catalog, purchase eligibility and actual reward receipts; verify wire round trips, fresh-match reset, reconnect state and nested-array mutation isolation with engine-free tests.
- [ ] 4.4 Adapt `Main.SendAction`, checked-in automated input parsing and the shared command model to technology identifiers; update the protocol version and ENet/Steam metadata through the shared constant; verify old peer rejection and local/remote command serialization with cheap tests and command help consistency.
- [ ] 4.5 Update `tools/DevRunner/CampaignStrategy.cs` research policy and legacy rank fixtures to earn points and acquire a real melee path through ordinary commands; verify applicable `mise run test` campaign coverage retains all existing strategy families, seeds, co-op sizes, sixty productions, twenty waves and paid upkeep, with technology/income evidence.

## 5. Pure bounded status policies

- [ ] 5.1 Add validated immutable burn/poison/chill rules and bounded per-unit state with captured potency, source/action/stack identities and absolute deadlines; verify cheap tests cover invalid periods/durations/caps, overflow-safe scaled potency and no live-source dependency.
- [ ] 5.2 Implement burn application/refresh and periodic scheduling; verify repeated hits preserve one stream and next tick, strongest potency/later expiry apply, first damage waits one period and a terminal expiry tick damages once.
- [ ] 5.3 Implement at-most-three independently timed poison stacks and canonical earliest-expiry refresh; verify fourth/multi-source hits, tie ordering, unchanged next ticks, independent expiry and source death under reversed input order.
- [ ] 5.4 Implement strongest-only chill, positive upward-rounded duration calculation and expiry-at-action-boundary behavior; verify already committed move/windup/recovery remains unchanged and subsequent eligible actions sample current chill without freeze/stun/initiative changes.
- [ ] 5.5 Document burn/poison/chill application, refresh, caps, captured damage and timing in `docs/gameplay.md`; verify examples against pure-policy test fixtures and omit any promise of freeze, stun, elemental reactions or healing.

## 6. Combat integration and current-state restoration

- [ ] 6.1 Insert periodic contributions and canonically ordered on-hit applications into the existing common impact/damage stages; verify pure/engine-free combat tests cover actual splash victims, invalid-primary misses, zero-damage sources, no periodic recursion and status damage simultaneous with lethal ordinary impacts.
- [ ] 6.2 Apply Guardian reduction once per incoming contribution and capability-based direct damage, extending combat configuration/accumulation and chilled-approach safety validation; verify fractional small hits stay positive, unsafe configurations reject before spending and default no-progress/wave limits remain intact.
- [ ] 6.3 Sample chill only at new action creation and preserve effective authoritative action/pose/reservation projections; verify `CombatActionTests`, occupancy and reversed-insertion fixtures include expiry during movement/windup, slowed positive recovery and no free reservation/action changes.
- [ ] 6.4 Preserve statuses/captured potency across transfers, process afflicted queued enemies without spatial/targeting eligibility, remove active scheduling on death and clear effects at wave resolution; verify queue death-before-admission, no admission double tick, no source-death purge, reserves, post-result cleanup and ordinary defeat/victory precedence.
- [ ] 6.5 Extend unit/event snapshots and fixture restoration validation with detached capability/status state and final periodic casualty evidence; verify JSON round trips, snapshot mutation rejection/isolation, paused deadlines, history-gap/current-state reconstruction and no repeated effects in `CombatPlaybackTests`.
- [ ] 6.6 Add small paired researched-role fixtures using default profiles and ordinary equipment/upkeep to establish effective burn/poison damage and chill/Guardian benefits; verify applicable `mise run test` checks preserve original unresearched Mage/Crossbow/frontline assertions and record costs, actual damage, timing and remaining health without an exhaustive option matrix.
- [ ] 6.7 Update `docs/gameplay.md` and `docs/verification.md` with same-tick ordering, transfer/queued status rules and cheap coverage limitations; verify these descriptions match current snapshot fields and checked-in test commands.

## 7. Research controls and status presentation

- [ ] 7.1 Add a focused scrolling research panel using the existing theme, with a no-selection Research control, authoritative points/progress/costs, visible prerequisites and permanent sibling-lock reasons; verify cheap presentation/eligibility tests cover own/foreign city, ready/pause/disconnection and acceptance-only feedback.
- [ ] 7.2 Remove contextual rank controls and implement distinct bundled Research Tower structures with readable output/upgrade/sale details; verify layout/presentation tests retain stable plot selection, correct labels/refunds, structural level differentiation and existing asset provenance.
- [ ] 7.3 Extend `ProgressionPresentation`, army/city details and `UnitInspector` to show actual capabilities, active strengths/stack counts and remaining simulated timing, including afflicted queued-enemy summaries without world bodies; verify cheap presentation tests cover actual profile values, terminal/fresh resets and no unsupported freeze/stun descriptions.
- [ ] 7.4 Add restrained status indicators driven by the shared playback clock, preserving rigged actions/health bars/Roman markers; verify cheap playback/focus tests cover pause/loss freeze, expiry/death disposal, city switches, history gaps and reconnect without old flashes/sounds.
- [ ] 7.5 Update README and `docs/gameplay.md` with research-panel access, exclusive purchase behavior and readable status interpretation; verify no-selected-plot purchases and read-only foreign-city inspection are accurately described.

## 8. Selected cooperative and graphical acceptance

- [ ] 8.1 Review existing authority serialization/resume evidence and extend the smallest necessary `redistribution` or `authority-resume-victory` witness for real transported research/status deadlines and exactly-once receipts; verify the chosen checked-in `mise run test-network --scenario <existing-id>` passes with retained cooperative assertions, owned cleanup and documented incremental risk/cost, without adding another full campaign.
- [ ] 8.2 Register an independently selectable `research` checkpoint within existing source `combat` UI coverage, preserving all five source ids and existing melee selection; implement owned ordinary-command point earning and actual-input Fire purchase/Frost lock/current burn inspection with fresh PNG evidence; verify `mise run test-ui --scenario combat --ui-checkpoint research` within 120-second setup and 30-second feature bounds, with no injected research balance/external probe and speed-one graphical barriers.
- [ ] 8.3 Integrate that checkpoint's assertions into default combat coverage while reusing setup/advancement, and verify existing reconnect baselining shows current research/effects without replay; run the affected existing `combat`/`reconnect` slices only when their relevant inputs changed and retain owner cleanup on failure/cancellation.
- [ ] 8.4 Record the actual added network/UI timing, setup, maintenance scope and software-rendering limitations in `docs/verification.md` and scenario risk descriptions; verify documentation distinguishes tested defaults from untested branch combinations and no new full graphical campaign is required.

## 9. Final integration gates

- [ ] 9.1 Perform the repository's locked restore and format changed C# with `dotnet format Odot.slnx --no-restore`; verify applicable `mise run test` and affected previously unresolved source slices pass, reusing successful checks until relevant inputs change.
- [ ] 9.2 Run final full `mise run ci`, including all source gates and sequential client/server exports followed by package smoke; retain result and evidence under ignored logs and verify no new dependency/tool install, publishing, upload or unrelated process mutation occurred.
- [ ] 9.3 Validate the completed implementation against all research/status delta scenarios, fixed 1/+1/+2 income and 3/6/9 costs, branch exclusivity and deferred freeze/stun scope; verify `openspec validate add-city-research-and-status-effects --strict` passes, docs/commands/evidence agree and only delivered tasks are checked complete, without archiving or starting another workflow.
