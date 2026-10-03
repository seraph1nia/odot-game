---
name: review-openspec-change
description: Independently verify one Odot OpenSpec implementation from a fresh read-only Codex Reviewer context and exact owned base/head diff.
---

Input: one complete change, approval, standalone Shipper handoff, immutable exact Git base/head and attempt1 or2. FirstMate dispatches a NEW Reviewer Scout with its OWN worktree; no Shipper transcript, promoted Scout or moving Shipper checkout. FirstMate spawn has no revision flag: assert the new worktree is clean, then git switch --detach <reviewed-head> only there, assert exact head and available base, and review base..head. Report inaccessible revisions as blocked, never substitute main silently.

Read proposal, design, deltas/requirements, tasks, canonical specs, ADRs, exact source diff and tests. Check spec conformance/coverage, correctness, adequate tests, regression risks, unnecessary scope and architecture consistency, including undocumented decisions and incomplete tasks. Use applicable checks and standalone file/line/requirement/reproduction evidence, not merely handoff assertions.

Production code stays read-only. Record tracked source before and after; build/log output is allowed only in owned ignored paths. Upstream Codex's native harness may bypass sandbox/approval; this is instruction/diff enforcement, not OS isolation. A dirty source diff invalidates a read-only review. Planning writers later capture reports; the Reviewer does not repair its own findings or archive/merge anything.

Output exact fields:

```text
RESULT: PASS | FAIL_IMPLEMENTATION | FAIL_SPEC | NEEDS_HUMAN_DECISION
BLOCKING_FINDINGS:
NON_BLOCKING_FINDINGS:
REQUIREMENT_EVIDENCE:
SPEC_DEVIATIONS:
RECOMMENDED_NEXT_ACTION:
```

Bind evidence to base/head, planning-inputs spec/code/canonical identities, fresh Reviewer task/session/worktree and distinct Shipper ID, attempt and commands/results. PASS needs complete tasks, applicable validation, resolved decisions and current reviewed inputs. Meaningful future source/spec changes invalidate it. Retain all failed reports.

FAIL_IMPLEMENTATION on review1 permits one Shipper repair and NEW review2; any second failure escalates to the user. FAIL_SPEC returns to OpenSpec update/replanning; NEEDS_HUMAN_DECISION returns to the user without inventing semantics. PASS permits gated lifecycle preparation using existing sync/archive skills; landing still needs separate explicit approval. The unfinished bootstrap checkpoint cannot get whole-change PASS while live acceptance tasks remain incomplete.
