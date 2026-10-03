---
name: proposal-preflight
description: Run a fresh Codex Scout overlap and dependency analysis before creating or materially revising an Odot OpenSpec proposal.
---

FirstMate dispatches a NEW Codex Scout, even after prior intake. Read AGENTS.md, all canonical specs relevant to the idea, ALL relevant active changes (inventory the whole active tree to find them), roadmap, related ideas, ADRs and actual source/tests. Produce a standalone preflight report answering:

1. Does active work already solve some/all of this?
2. Is it a duplicate idea/change?
3. Does it contradict an active proposal?
4. What must be implemented first?
5. What later work depends on this?
6. Should it merge with another proposal?
7. Should it split?
8. Is a project-wide ADR needed?
9. Where does it belong in roadmap order and why?
10. Which existing proposals become obsolete?

Give evidence and distinguish hard dependencies from preferences. Surface material product/architecture decisions to the user; do not settle conflicts silently or rewrite existing proposals. Feature-local design belongs in OpenSpec; cross-project architectural rules require a new/superseding ADR.

Only AFTER preflight, prepare the proposal using existing openspec-propose, or reconcile existing artifacts through openspec-update-change. These are planning-only operations. A planning-only logical Scout on a native Ship delivery contract captures artifacts, reciprocal idea/source links and roadmap state. Successful promotion requires complete planning artifacts, source idea status promoted, coherent queue/dependencies and planning-validate plus strict OpenSpec validation. Proposed does not mean approved for implementation; existing approved order is preserved unless the user makes a new decision. No production code edits, runtime-backlog prioritization or arbitrary promotion to ready.

Worked overlap: an idea for city research must inspect add-city-research-and-status-effects; explicitly identify its existing requirements, decide whether an uncovered aspect belongs there, and recommend merge/update or a truly separate dependency. Do not create a duplicate because the idea uses different words. A fuzzy verification-command idea first needs shaped scope; if small documentation changes are agreed, preflight still checks linux-test-execution and completed-but-unsynced verification changes.
