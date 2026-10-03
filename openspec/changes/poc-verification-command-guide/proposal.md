# Proposal

## Why

[IDEA-001](../../../planning/ideas/IDEA-001-verification-command-guidance.md)
records difficulty choosing an Odot verification command and the human's
**Documentation guide** shaping choice. Existing policy and command references
already explain behavior; a short task-based entry point can make routine
selection easier without changing verification guarantees.

## What Changes

- Propose a compact "Choose a check" section at the start of
  [README's Verification and exports](../../../README.md#verification-and-exports),
  for human contributors and coding agents working on routine edits. Audience
  and placement are recommendations for review, not recorded human decisions.
- Map six common tasks to existing checks: numerical/runner edits, affected
  networking, affected rendered controls/recovery, existing exported packages,
  substantial implementation boundaries, and documentation/planning-only edits.
  Explain preparation, relative cost and what a pass covers in brief terms.
- Link existing prerequisites, selectors, coverage/evidence, planning conventions
  and [AGENTS policy](../../../AGENTS.md#choose-checks-by-cost-and-affected-behavior)
  rather than repeat their full detail. Keep profiles, native package checks and
  paired Steam acceptance as separate routes.
- Preserve frequent applicable cheap checks, selected expensive coverage, full
  before/after substantial-task gates, reusable unchanged baselines and ordinary
  CI's complete required gates. Documentation-only work needs consistency checks.

## Capabilities

### New Capabilities

None. This is documentation discoverability only.

### Modified Capabilities

None. `.openspec.yaml` declares `skip_specs: true`: no spec-level behavior changes
or delta specs. [Canonical linux-test-execution](../../specs/linux-test-execution/spec.md)
already requires cost-based guidance. Completed-but-unsynced
[speed-up-verification](../speed-up-verification/proposal.md) remains separate
canonical-reconciliation work; this proposal does not perform or require its sync.

## Impact

Later implementation is limited to the compact README addition and the ordinary
change task/evidence bookkeeping. Game, runner, tests, command syntax/defaults,
locks, AGENTS and canonical specifications require no edits. Add no test suite,
new documentation platform, recommender or dependency.

The [fresh ten-question preflight and restart evidence](../../../planning/evidence/setup-agent-workflow-poc/scenario-2-proposal.md)
finds no hard dependency or semantic conflict. Append the roadmap item as
`proposed`, with reciprocal IDEA-001 links; preserve existing queue order.
Future research also edits README, so integrate its wording when that separately
approved work lands. The installed workflow checkpoint supports this planning
trial; the blocked whole POC is not a completed dependency.

This request authorizes proposal capture only. All implementation tasks remain
unchecked. No implementation approval, independent PASS, sync, archive or landing
is created; implementation requires a new explicit approval for this proposal.
