# Independent local checkpoint review — attempt 1

RESULT: FAIL_IMPLEMENTATION

Scope: local bootstrap only. This is not a native FirstMate live Reviewer or whole-change acceptance. Fresh Reviewer `/root/checkpoint_reviewer` inspected detached worktree `.cache/agent-workflow-review`, base `4a70e16a152beee112a775b1f11f08da4ebc2277`, head `df906227f336924c4bdeb12a6cabc44e814ab230`.

## BLOCKING_FINDINGS

B1: Completion accepts unreviewed production changes made before landing. `tools/DevRunner/Planning/PlanningEvidence.cs:106` activates historical checks for completed/archived; lines 134–139 check only landing ancestry. The recorded landing's production inputs are never compared with the reviewed head. This violates the workflow delta spec's current independent PASS/material-change invalidation requirement and planning documentation.

Independent reproduction used the actual checked-in CLI with an ignored diagnostic repository under the review worktree's `logs/checkpoint-review/landing-fixture`:

1. Verified `first` with valid receipt: `planning-next --json` returned exit 0, valid true, change null; dependent `second` remained blocked.
2. Committed an unreviewed same-feature source change after PASS and before claimed landing: verified selection correctly returned exit 1, stale code digest.
3. Changed only destination state to completed and added that unreviewed descendant as landed_revision, preserving original review identities/digests: exit 0, valid true, selected second.

Fixture reviewed head `175b526dd6f6193736cde2fa67d78dba67e13d33`; unreviewed claimed landing `77b5f4e8b7330a6ee31352d7fa9591fd411eab57`. Bind production contents at the recorded landing to the reviewed implementation. Preserve genuine historical completion after separately authorized later changes. Add a cheap regression covering the review-to-landing transition.

## NON_BLOCKING_FINDINGS

- Launcher session/socket/current-pane checks are narrower than native FirstMate's `fm_backend_herdr_launcher_identity` contract (`bin/backends/herdr.sh:1746`), which also verifies canonical socket and pane/tab/workspace agreement. Consider reusing that native helper before live acceptance. No destructive failure was independently demonstrated.
- Required after-CI was pending retry at review time; the initial UI exit137 failure cannot establish successful complete validation.
- Live scenarios and final acceptance remain incomplete, accurately represented by open tasks.

## REQUIREMENT_EVIDENCE

- Fresh read-only context separate from Shipper; exact reviewed head unchanged before/after, clean porcelain, tracked/staged diffs empty. Ignored build/diagnostic outputs only; no Godot/full-CI launch.
- Input identities: spec_digest `ab621af466fb315725fcc8045cc19d7bdd4c09f751a988083b6e7652ce468dc2`; code_digest `7984fb3ca2769ecd29359ff06db836b039050d4cb118dc15ed910564b902fff4`. Planning-inputs enumerated all 16 canonical spec hashes.
- Locked runner-test restore PASS; focused PlanningTests/AgentLauncherTests 41 passed, 0 failed/skipped; planning-inputs and planning-next JSON PASS; strict OpenSpec validation PASS; both shell syntax checks PASS.
- Five role skills, authority ADR/root guidance and overlay describe Git product truth, explicit approval, fresh independent review, bounded repair, separate landing approval and runtime reconciliation.
- Supplied FirstMate source at `e31bc6e620ca532c2e0e0b72f3fd7c0869a12270` independently inspected; no-origin/base preservation and origin reset behavior match runbook. No live acceptance is inferred.

## SPEC_DEVIATIONS

B1 is an implementation deviation. Apply-time snapshot correction is factual reconciliation without new product semantics. Local bootstrap is distinct from live FirstMate acceptance; no whole-change PASS.

## RECOMMENDED_NEXT_ACTION

Return B1 to Shipper for the one allowed repair, targeted regression/checks and a new immutable checkpoint, then a second fresh Reviewer. Complete required after-CI. Release bootstrap implementation ownership and record blocked awaiting live acceptance before trial implementations. Preserve failed evidence; no archive/completion/Atomic/automatic landing.
