# ADR 0001: Agent execution and project truth

Date: 2026-10-03
Status: Accepted for the explicitly authorized single-project POC; live viability remains unproven.

Odot already owns source, tests and OpenSpec guarantees. We need disposable workers without transferring product decisions into another backlog or conversation.

Use one Pi/FirstMate Orchestrator and fresh Codex Scouts, Shippers and Reviewers. FirstMate owns spawn, session, supervision, recovery, Treehouse isolation and guarded landing; Herdr makes workers observable. Use existing facilities rather than introducing a controller, database, RPC channel, tracker or upstream fork. FirstMate's private manual backlog describes current execution and links Git artifacts.

Git owns ideas, ordered/dependency-aware roadmap, OpenSpec lifecycle and requirements, architectural decisions, implementation and evidence. Conversation alone grants no reconstructible state. Re-entering a primary requires project reads and reconciliation of live owned workers before new dispatch. One implementation change can be active at once, including trial changes. The primary delegates substantial implementation and durable planning writes; a Scout planning writer remains logically a Scout with a narrow native Ship delivery contract.

First implementation requires explicit user approval bound to one change's requirements. Shippers cannot invent semantics, silently alter specs, reorder the roadmap, archive themselves or verify themselves. Genuine ambiguities stop affected work and route to replanning/user. A fresh read-only Reviewer checks the exact immutable diff/requirements; one failure permits one repair and a second fresh review, then escalation. FAIL_SPEC routes to OpenSpec update; NEEDS_HUMAN_DECISION routes to the user. Current independent PASS, validation, complete tasks and no unresolved decision gate sync/archive/completion. Archive can be prepared automatically; landing is separately and explicitly approved. Unlanded prepared bookkeeping cannot clear dependencies. Preserve imported historical completion without inventing reviewer receipts.

Worktree isolation and reviewer instructions do not provide an OS security boundary; upstream Codex workers may run with bypassed sandbox/approval flags. Before/after tracked-source assertions and revision receipts detect unintended reviewer edits. No automatic merge autonomy, parallel implementation, Atomic or custom messaging in the baseline. Missing tools are user-managed and unexecuted acceptance stays visible.

Feature-local design stays in OpenSpec design artifacts. New project-wide rules belong in new ADRs that supersede this decision explicitly rather than rewriting history. Acceptance of this architecture for an experiment does not claim the five real-session trials passed.
