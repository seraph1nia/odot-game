# Design

## Context

See proposal.md for motivation. Landed revision f37830a includes original authored hierarchies, validated terrain batches and exclusive ENet guest service. Prior static flattening and pine instancing reduced draws without latency benefit and are rejected. Ordinary numerical profiling and a 600-frame owned presentation replay already exist.

## Goals / Non-Goals

**Goals:** measure actual residual costs on the landed inputs; remove demonstrably redundant work with exact outcomes and bounded lifecycle ownership.

**Non-Goals:** assets or optimized exports, visual quality/content changes, renderer/backend changes, transport redesign, approximate simulation, broad engine abstraction, test deadline relaxation.

## Decisions

- Reuse the existing replay/private-display/process harness. Add an independently selected authored-scale replay, not another E2E campaign. It is needed to distinguish script/rig scaling from original scene/software-render cost that cheap unit tests cannot observe. Keep 600 frames, one warm-up and serial repetitions; budget approximately 2–4 minutes per process and no default-CI matrix expansion.
- Use ordinary profiles and fixed ticks for numerical measurement, detached deterministic snapshots for read-only presentation. Hash every input script and source inventory. Separate loading/transition frames from settled frame distributions and report render/physics monitors as software diagnostics, not isolated native GPU proof.
- Inspect and measure stage-owned spatial range selection and per-frame unchanged camera/terrain recalculation first. Only adopt candidates after actual measurement and exact seeded target/tie/control checks. Do not repeat failed static batching experiments.
- Keep no more than three independent rounds. A remaining native rendering bottleneck is a reportable limit, not permission to alter authored materials/shadows or pre-render a flat background.

## Measured outcome

Two rounds were completed. Stage-local range grouping was rejected and removed;
its construction cost outweighed saved visits. Existing routing goal-score minima
were the accepted seam: allocation-free exact loops stop at proven positive-step
1/static-distance 0 bounds without changing cache ownership or seeded ties. Small
ordinary campaigns and the fixed 2048-stored-actor window improved; native authored
rendering still dominates frames. No camera/terrain cache or background layer was
adopted. See [the report](../../../docs/authored-performance.md) for controlled
inputs, distributions, exact current-frame comparisons and complete gate evidence.

## Risks / Trade-offs

- [Software backend dominates wall time] → report managed and native costs separately; do not imply CPU wins solve native GPU budgets.
- [Spatial query changes seeded ties] → compare exact candidate/selection results across seeds, graph topology, lifecycle, faction and city; retain canonical identity tie ordering.
- [Cached camera-derived state goes stale] → include transform, projection, viewport/world rectangle and landscape position/travel in invalidation; keep fresh evidence uncached.
- [Pre-existing reconnect admission failure] → preserve current receipt evidence (pause518, focused corpse expired512), no assertion weakening or broad fixture rewrite. Any later validation correction needs a matched causal control.

## Migration Plan

No data migration. Changes use existing seams and can be reverted without touching assets, saves or wire identities. Final full CI still gates delivery; the failed before baseline is explicitly not a green complete pass.
