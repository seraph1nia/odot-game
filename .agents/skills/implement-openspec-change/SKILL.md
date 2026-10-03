---
name: implement-openspec-change
description: Implement exactly one explicitly approved Odot OpenSpec change as an isolated Codex Shipper, producing a revision-bound handoff without archiving or self-review.
---

Input: ONE change ID, current approval receipt, exact expected base, owned FirstMate worktree and scoped brief. The primary reconciles runtime work and records in_progress before dispatch; no other implementation change is admitted.

Read complete proposal/design/deltas/tasks/metadata, applicable canonical specs, AGENTS.md, ADRs, implementation and tests. Verify assumptions and clean isolated worktree/base before editing. Use existing openspec-apply-change instructions for authoritative contextFiles/task tracking. Implement only the approved requirements, add meaningful tests, run affected cheap/targeted checks and required broader repository validation, format changed C# after locked restore, and update completed task markers immediately. Record baseline/result evidence; no invented acceptance.

Do not silently edit requirements/design/canonical specs, invent product semantics, reorder roadmap, work unrelated backlog, archive the change or declare independent verification. If a specification/architecture assumption is invalid, stop affected work and return STATUS: SPEC_CHANGE_REQUIRED, identifying requirement, reproduction/evidence, unresolved rule and needed decision. The primary routes to openspec-update-change and renewed approval after material changes.

Commit the owned implementation checkpoint and produce the standalone handoff with exact fields:

```text
STATUS
CHANGE
SUMMARY
FILES_CHANGED
TESTS_RUN
VALIDATION_RESULTS
OPEN_QUESTIONS
SPEC_DEVIATIONS
KNOWN_RISKS
NEXT_ACTION
```

Include FirstMate task/session/worktree, exact Git base/head and planning-inputs identity, command exit/results and non-secret log paths. Retain limitations and incomplete tasks. Implementation completion is not PASS, archive or main landing.

On one FAIL_IMPLEMENTATION, primary sends structured findings through FirstMate's native steering channel. Repair only those findings, with relevant checks and new immutable head/handoff. One repair maximum; second review must use a new Reviewer. FAIL_SPEC routes to replanning, NEEDS_HUMAN_DECISION to the user; a second failed review escalates with no third autonomous repair. Preserve prior findings/attempts across restart.
