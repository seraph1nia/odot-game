# Proposal

## Why

The completed verification-speed change records 252.82s full CI and 55.37s for its cooperative C# partition, but those overlapping verification timings do not measure runtime efficiency. Repeated world scans, snapshots, decision preparation and presentation sampling need low-risk optimization, with separate before/after evidence for actual game performance, large-fight scalability and verification speed.

## What Changes

- Establish and retain unoptimized baselines before changing runtime or campaign-harness behavior, then rerun the same workloads after optimization. Report runtime performance and verification speed separately, including absolute values, percentage changes, workload identity, coverage and uncertainty.
- Add bounded checked-in campaign, large-battle and presentation measurements, plus isolated snapshot/serialization/compression measurements. Use built-in CPU, elapsed, managed allocation and GC counters; optional operation counters explain work without pretending to count every CPU instruction.
- Add an independently selectable engine-free scalability test with one 2,048-unit mixed fight on a supported 256-cell board, substantial simultaneous deployment and queued reinforcement, plus smaller comparable samples. This synthetic workload supplements ordinary campaigns; it does not change default gameplay or establish balance.
- Replace snapshot-based enemy bookkeeping with internal authoritative queries for living membership, destinations, transfers and engagement progress.
- Reuse stable ordered unit views within explicitly bounded simulation stages instead of repeatedly collecting and sorting the ECS world.
- Construct snapshots efficiently: filter units before projecting, reuse city soldier projections for forecasts, and avoid redundant immutable catalog/definition construction while preserving detached public snapshots.
- Reuse each campaign decision's snapshot for commands and before-state assertions; retain all 21 strategy combinations, ordinary commands, serialized campaign coverage and required economic/combat evidence with compact success output and bounded failure diagnostics.
- Share exact city-local battlefield observations across actors and index opponents by city/faction during an action stage, retaining actor-specific decision sequences and relevant invalidation.
- Include single-target splash short-circuiting, shared city exposure/defense lookups, exact best-target selection without sorting irrelevant candidates, cheaper temporary BFS storage, immutable profile/quote reuse and direct internal catalog lookup.
- Remove duplicate pose sampling, repeated playback index construction and per-view event-history scans; avoid hidden-city skeletal work, cache animation/bone/name and route-length data, and refresh HUD sections only when their displayed inputs change.
- Verify deterministic state/event equivalence, snapshot isolation and mutation-stage freshness; record per-area and overall performance results.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `coop-verification`: Add separate runtime/test-speed comparisons, deterministic efficiency counters, large-battle acceptance, reproducible presentation measurements and regression acceptance for optimized simulation/projection/presentation paths. Existing numerical and visible gameplay contracts remain unchanged.

## Impact

Affected areas are `src/Game.Core/World.cs`, economy/profile/collection/projection/decision/playback helpers, `src/Game` unit/tabletop/layout presentation and transport measurement boundaries, `tests/Game.Core.Tests`, runner tests, checked-in verification/profile entry points, README and `docs/verification.md`. The plan builds on the implemented but not yet archived/synced `speed-up-verification` change and preserves its pacing, bounded evidence, coverage and process budgets. The large test and repeated profiling are explicitly selected implementation acceptance, not additional full graphical battles or mandatory benchmark repetitions in ordinary CI.

No gameplay balance, precision, seed algorithm, wire schema, authority policy, render appearance, default concurrency or simulation speed change is proposed. Presentation implementation is now in scope. ECS replacement, approximate AI, protocol redesign, new dependencies and automatic tool installation remain out of scope. Planning runs no performance tests; the apply phase must capture runtime and verification baselines before optimizations and repeat both afterward, retaining the required full before/after correctness gates.
