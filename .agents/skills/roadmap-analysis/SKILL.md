---
name: roadmap-analysis
description: Explain or choose the next approved Odot OpenSpec change deterministically and reconcile FirstMate work before implementation dispatch.
---

Run project commands in the Odot checkout (the primary uses ODOT_PROJECT_ROOT because Pi starts in FirstMate). Read AGENTS.md, planning/README.md, roadmap, applicable approvals/reviews, selected change and relevant ADRs. Run planning-validate and planning-next --json through the checked-in mise tasks. Invalid planning means no selection. Report selected ID plus its approval/spec identity and earlier exclusions. change:null is a valid “no eligible work” result. Never reorder queue, let priority override dependency order, or replace deterministic results with intuition.

“What’s next?” is read-only. “Do the next thing” authorizes exactly the resolved eligible change for this request; persist actual approval/scope with planning-inputs. If selection or specification materially changes, return the changed decision to the user before dispatch. “Implement X” requires a complete change and explicit current authorization; do not infer approval from tasks/CLI in-progress status.

Before ANY dispatch, reconcile FirstMate native task records/watch state and visible owned worker identities against Git roadmap/evidence. A fresh primary reconstructs the same approvals, open outcomes and one-repair bound. Existing runtime Shipper or in_progress item prevents a second implementation, including trial changes. Resolve inconsistent/missing state rather than creating replacements blindly. This skill does not add a lock service or supervise workers itself.

Use implement-openspec-change for exactly one Shipper and review-openspec-change for fresh verification. No eligible work? Explain dependencies/blockers/unapproved proposals and ask only the material product choice. Do not turn legacy completion into fabricated independent PASS.
