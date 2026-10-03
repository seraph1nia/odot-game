---
change: replace-change-id
result: PASS
reviewer_task: replace-fresh-reviewer-task
shipper_task: replace-shipper-task
base: replace-exact-base-commit
head: replace-exact-reviewed-commit
spec_digest: replace-with-planning-inputs-spec-digest
code_digest: replace-with-planning-inputs-code-digest
canonical_inputs: {}
attempt: 1
validation_passed: false
decisions_resolved: false
---

RESULT: PASS | FAIL_IMPLEMENTATION | FAIL_SPEC | NEEDS_HUMAN_DECISION
BLOCKING_FINDINGS:
NON_BLOCKING_FINDINGS:
REQUIREMENT_EVIDENCE:
SPEC_DEVIATIONS:
RECOMMENDED_NEXT_ACTION:

# Review identity and checks

Record fresh Reviewer context/task/session/worktree, exact base/head, current canonical_inputs mapping from planning-inputs, read-only source before/after, command/results and relevant requirements. Give attributable file/line/requirement/reproduction evidence for each failure. attempt=1 or2; never fabricate PASS from template fields. Failed reports remain retained even when the roadmap points to the later PASS. Add landed_revision only after separately approved native landing, with actual committed identity. Never use the Shipper as reviewer.
