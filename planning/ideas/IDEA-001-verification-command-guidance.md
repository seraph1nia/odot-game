---
id: IDEA-001
title: Easier verification command selection
status: promoted
created: 2026-10-03
related_changes: [poc-verification-command-guide]
supersedes: []
tags: [verification, documentation, developer-experience]
---

# Problem

The user wants to "make it easier to tell which Odot verification command I need."
The confusing situation and intended reader are not yet specified. During intake,
FirstMate relayed the human's shaping choice, "Documentation guide." That selects
an approach for further shaping, without approving a proposal or implementation.

# Desired outcome

A reader can choose an appropriate check for their immediate task, understand its
preparation and cost, and tell what a passing result establishes. The guidance
should preserve targeted checks, full task-boundary verification, isolated
execution and the distinction between partial and complete coverage.

# Current understanding

- [README](../../README.md#verification-and-exports) lists commands and explains
  coverage; [AGENTS](../../AGENTS.md#choose-checks-by-cost-and-affected-behavior)
  gives the cost-based execution policy. [Verification evidence](../../docs/verification.md)
  mixes current coverage and measurements with historical implementation records.
- `mise run test` runs engine-independent gameplay and runner checks, with
  restore/build preparation. `test-network --scenario NAME` selects a real-process
  check; `test-ui --scenario NAME` selects an owned-display graphical check.
  Standalone source selections prepare themselves. `test-ui --scenario exported-package`
  uses existing exports and never implicitly rebuilds them.
- Full `ci` includes required source gates, sequential Linux exports and package
  checks. Documentation-only work needs relevant consistency checks; a substantial
  implementation needs full before/after validation, reusing an unchanged baseline.
- Planning tasks start no Godot but depend on a runner restore/build through
  [mise.toml](../../mise.toml). A no-build investigation can use an available
  already-built runner's planning entry point; that is a scoped exception, not
  a new everyday command contract.
- [Canonical linux-test-execution](../../openspec/specs/linux-test-execution/spec.md)
  retains older examples, including serial source UI and the former development
  server arrangement. [speed-up-verification](../../openspec/changes/speed-up-verification/proposal.md)
  already covers command guidance and has a completed, unsynced concurrency delta.
  Current source and root guidance describe five source UI slices with bounded
  concurrency and a playable host plus guest for `dev`. Reconciliation is separate
  lifecycle work; this intake changes neither specifications nor command behavior.
- It is plausible that distributed explanations cause selection difficulty, but
  no concrete user incident or usability observation has established the cause.

# Questions

1. Which situation causes the confusion: choosing a tier after an edit, finding a
   scenario name, understanding preparation/prerequisites, or interpreting a pass?
2. Who should the first improvement serve: a human contributor, a coding agent,
   or both? Should the entry point be README, runner help, or another existing page?
3. For the selected documentation guide, would a short task-based path or a
   comprehensive command reference help most? Where should readers find it?
4. Should the initial scope cover routine rules/network/UI/CI/planning checks,
   with profiles, native packages and two-account Steam acceptance linked separately?

# Explored approaches

| Approach | Benefit | Trade-off |
| --- | --- | --- |
| Short task-based guide in an existing document | Makes common decisions easy to scan; small maintenance and consistency-check cost | Still requires judgment about affected behavior; examples must stay current |
| Command reference with coverage, preparation and evidence columns | Explains similar-looking source, package, planning and profiling commands | Longer page; duplicates facts unless linked to their existing owners |
| Improved runner help or a read-only recommender | Makes choices discoverable at invocation time | Production runner work; needs defined inputs and recommendation semantics, cheap tests, and a way to avoid presenting heuristics as full verification |
| First reconcile existing guidance and canonical-spec drift | Reduces contradictory descriptions and builds on existing work | Requires separately scoped lifecycle authority; may not solve the user's particular confusion |

The human selected a documentation guide while intake was running. If the
difficulty is routine tier selection, a narrow task-based guide is the
lowest-cost candidate within that approach. Its surface and content remain
undecided; the recommendation does not authorize implementation.

# Decisions

At scenario 1 intake, record `IDEA-001` as `exploring`. Active and archived idea directories contained
only their README files at intake, so this is the next unused stable ID.
One human shaping intervention selected "Documentation guide," relayed by
FirstMate in task inbox message `002.msg` at 2026-10-03T09:37:13Z. Retain
`exploring` at that stage: the audience, document location, content boundaries, acceptance
examples and roadmap position remain undecided. No proposal, implementation
approval or promotion was created. The questions above concern possible future
shaping; none requires an answer to finish this planning-only intake, which
FirstMate explicitly instructed the worker to finish without starting scenario 2.

On 2026-10-03, the separate scenario 2 brief authorized promotion into a small
task-based documentation proposal, [poc-verification-command-guide](../../openspec/changes/poc-verification-command-guide/proposal.md).
A fresh [ten-question preflight](../evidence/setup-agent-workflow-poc/scenario-2-proposal.md#standalone-proposal-preflight)
found no hard dependency, duplicate proposal or material product/architecture
decision blocking planning. Mark this idea `promoted`; the new roadmap item's
`source_ideas: [IDEA-001]` is reciprocal. `related_changes` now identifies the
proposal sourced by this idea; earlier verification/optimization/workflow overlap
references remain in the body and are not retroactively claimed as its outputs.
Recommend routine human/agent readers and a compact README entry point for later
review. Those are proposed defaults, not additional human decisions. No concrete
usability incident has been supplied, and no implementation approval is created.

# Relevant code

- [Task declarations](../../mise.toml), [runner entry point](../../tools/DevRunner/Program.cs),
  [command dispatch/help](../../tools/DevRunner/Runner.cs), and
  [option validation](../../tools/DevRunner/Options.cs).
- [Stable selectors and evidence](../../tools/DevRunner/Scenarios.cs),
  [network witnesses](../../tools/DevRunner/NetworkTests.cs),
  [source/package graphical scheduling](../../tools/DevRunner/PrivateDisplay.cs),
  [cheap partitions](../../tools/DevRunner/CheapTests.cs), and
  [CI gates](../../tools/DevRunner/Exports.cs).
- [Planning command](../../tools/DevRunner/Planning/PlanningCommand.cs),
  [selection tests](../../tests/DevRunner.Tests/HarnessTests.cs),
  [admission tests](../../tests/DevRunner.Tests/AdmissionTests.cs), and
  [idea/roadmap tests](../../tests/DevRunner.Tests/PlanningTests.cs).

# Related ideas

No prior active or archived ideas were found. Related active changes are
`speed-up-verification` (coverage and guidance overlap), `optimize-core-simulation`
(selected correctness/profile commands), and `setup-agent-workflow-poc`
(this scenario and planning commands). The first two are recorded as
`legacy_completed`; the workflow POC remains `blocked` awaiting live acceptance.
[enforce-strict-csharp](../../openspec/changes/enforce-strict-csharp/proposal.md)
and [scaffold-csharp-multiplayer](../../openspec/changes/scaffold-csharp-multiplayer/proposal.md)
explain existing build/format/task foundations.
[add-city-research-and-status-effects](../../openspec/changes/add-city-research-and-status-effects/proposal.md)
is an unapproved proposal that plans to extend relevant existing checks; it
does not already solve command selection.

See the [standalone scenario report](../evidence/setup-agent-workflow-poc/scenario-1-fuzzy-intake.md).
The scenario runbook's `poc-verification-command-guide` is a possible later trial
name in the scenario 1 record, not a proposal created by that intake. Scenario 2
now captures it separately as the promoted documentation proposal linked above.

# Promotion criteria

Identify a concrete confusing task and the intended reader; choose the surface
and scope; agree how to demonstrate improved selection without changing existing
verification guarantees. A fresh proposal-preflight Scout must assess overlap,
especially completed-but-unsynced `speed-up-verification`, before any proposal.
Only then consider coherent OpenSpec planning and reciprocal idea/roadmap links.
Implementation and landing still require their separate explicit authorizations.

Scenario 2 completes preflight and proposal capture under its narrower task-based
brief. Its proposal/design/tasks make the recommended scope and acceptance
examples concrete for review; the remaining audience/surface choices above are
review choices before implementation approval, not unresolved decisions needed
to complete this planning-only promotion.
