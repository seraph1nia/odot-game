# Real-project POC acceptance

These are supervised manual acceptance through the ONE Pi/FirstMate primary, using native dispatch and real Odot history. They are not ordinary credential-bearing CI, scripted transcripts, mocked worker reports or a custom controller. Until executed, each outcome is **UNEXECUTED**. Read [setup/start/observe commands](agent-workflow.md) first. Missing runtime, trust or owned pane prerequisites stop dependent acceptance and remain recorded.

Start from the independently reviewed committed bootstrap checkpoint in the owned registered clone. Release the bootstrap Shipper and record setup-agent-workflow-poc as blocked awaiting live acceptance before any trial implementation. Do not mark this unfinished bootstrap a completed trial dependency. Trials note the installed checkpoint prerequisite instead. Only one trial is in_progress; own worktrees/bases/branches remain separate. Explicitly approve each first implementation and each native landing. An investigation or proposal request is not implementation approval.

Every scenario report under planning/evidence/setup-agent-workflow-poc/ records date, outcome, elapsed/setup time, human interventions, actual FirstMate task IDs, returned Herdr session/workspace/tab/pane IDs, Treehouse worktree/base/head, standalone reports/validation commands, source before/after and cleanup/retained owners. Raw non-secret logs are ignored logs/<run-id>/. FirstMate retains pending outcomes until durable capture; native teardown/cleanup awaits owned workers and preserves unrelated developer/CI/session state.

| Scenario | Concrete IDs | Expected cost; meaningful defect covered |
| --- | --- | --- |
| Fuzzy intake | IDEA-001 (or next unused stable ID) | 5–15min, 1 Scout + planning capture; catches premature production implementation |
| Proposal | poc-verification-command-guide | 5–15min, NEW preflight Scout + planning capture; catches overlap/dependency/idea-link omissions |
| Straight implementation | poc-verification-command-guide | 10–20min, 1 Shipper + NEW Reviewer; cheap docs/planning checks, no new Godot run for docs alone; catches context/scoping/receipt/landing confusion |
| Defect/repair | poc-validator-dependency-diagnostic | 15–30min, Shipper + two NEW Reviewers + one repair; cheap C# tests; catches actual reviewer/repair routing missed by deterministic validator fixtures alone |
| Spec ambiguity | poc-idea-summary-order | 5–15min, scoped Shipper stopped for missing rule; catches silently invented product decisions |

No extra game/UI/network scenario is admitted. Unit tests already check validator correctness; they cannot demonstrate live independent worker contexts or supervision. Keep these trials small and sequential rather than repeating full game battles or adding an option matrix.

## 1. Fuzzy idea

Primary prompt:

```text
I have an idea: make it easier to tell which Odot verification command I need.
```

Expected: identify intake; valuable NEW Codex Scout reads AGENTS/README/docs/verification, relevant runner entrypoints and linux-test-execution plus all relevant active changes. Return useful options/questions before assuming a solution. Planning-only Scout captures the real idea/report without source edits. Record production diff empty, relevant links, actual supervised completion and useful human choices. A manufactured preflight report is not evidence.

## 2. Proposal

After shaping the idea as a narrow documentation guide, use:

```text
Turn IDEA-001 into a proposal named poc-verification-command-guide.
```

Use the actual unused idea ID from scenario1. NEW proposal-preflight Scout answers all ten questions including completed-but-unsynced speed-up-verification overlap. Existing OpenSpec propose/update creates complete artifacts, proposed roadmap item and reciprocal promoted source links through planning capture. `openspec validate poc-verification-command-guide --strict` and `mise run planning-validate` must pass. Do not mark ready from a proposal request.

## 3. Straight implementation

The primary presents the concrete proposed docs change. User approval prompt:

```text
I approve implementing poc-verification-command-guide as proposed. Implement it.
```

Record current approval/digests, mark exactly one in_progress and dispatch a NEW native Codex Shipper with expected base, exact change, validation policy and handoff. Own isolated worktree modifies only the guide scope. Record consistency/planning/OpenSpec checks; docs-only work does not rerun the full game exports. The Shipper commits exact head, then NEW read-only Reviewer Scout detaches that head in its OWN clean worktree and checks requirements/diff/tests. Capture current PASS and before/after source identity.

Only after PASS/tasks/checks/decision gate prepare OpenSpec archive using existing lifecycle behavior once; canonical sync hash changes require explicit independent evidence rebind/review of equivalent guarantees. Request separately explicit landing approval for the concrete prepared branch. FirstMate guarded local landing refuses dirty/diverged targets; no unrequested merge. Record actual landed revision/main ancestry and completed/archived roadmap; branch-only readiness cannot clear dependencies.

## 4. Reviewer detects a defect and repair is bounded

Prepare/approve `poc-validator-dependency-diagnostic`: a small improvement explaining unfinished dependency exclusions. Preserve deterministic selection requirements from agent-project-workflow; extend an existing cheap test rather than inventing E2E. Native Shipper task/branch begins `poc-dependency-diagnostic`, producing e.g. `fm/poc-dependency-diagnostic-001`. Pause its checkout after initial immutable committed implementation; initial code must be clean.

The primary explicitly labels controlled fault injection in its acceptance record, without giving the new Reviewer the defect location. The checked-in procedure refuses this repository's own source checkout, dirty/nonmatching heads and branches outside the owned trial naming contract:

```sh
bash tools/AgentWorkflow/inject-dependency-fault "$TRIAL_WORKTREE" "$INITIAL_SHIPPER_HEAD"
# Primary requests native owned Shipper/checkpoint commit of labelled fixture only.
# Record original head and injected head; never land the injected head as completion.
```

The fixture changes only dependency eligibility, so cheap selection tests catch it. This intentionally inserted regression is not a naturally missed model error. NEW Reviewer receives usual exact requirements/diff and actual injected head, discovers the violation from its own tests/review and returns attributable FAIL_IMPLEMENTATION. Native fm-send returns structured findings to the same Shipper (fm-control relaunch if stopped), which repairs once and runs relevant tests. NEW review2 checks the repaired immutable head. PASS or second failure/escalation is retained with both reports/attempts. Confirm no fixture defect appears in production or final prepared branch; retain original/injected/repaired revisions for evidence. If a naturally discovered equally meaningful defect occurs, record that instead with equivalent live repair/review evidence.

## 5. Real unresolved specification rule

Prepare the intentionally under-specified `poc-idea-summary-order` trial from actual recorded ideas. Canonical intake statuses/IDs exist, but no product rule guarantees the visible summary order. The trial proposal says “list ideas” without deciding creation-date versus stable-ID ordering. Use two real idea records where those orders differ; a planning-only capture may add the second genuine idea after intake. Do not forge product approvals or fabricate those reports.

```text
Prepare poc-idea-summary-order to show a concise idea list. Keep the unanswered ordering choice explicit.
```

After user approval for the scoped implementation investigation, NEW Shipper reads specs/real records, identifies undefined ordering, stops affected implementation and returns SPEC_CHANGE_REQUIRED with concrete evidence/options. Primary routes to OpenSpec update and a user product decision; material revised semantics need renewed implementation approval. Record no invented order/affected code edit. Park/block unresolved trial accurately; do not archive it as verified completion. If a natural real assumption ambiguity appears instead, retain equivalent evidence and actual unresolved rule.

## Restart and repeat selection

While a worker or review outcome is pending, re-enter a fresh Pi primary through the managed Herdr startup. Do not destroy its owner or clear receipts to simulate recovery. Read Git approval/handoff/failed reports/attempt counts, reconcile native worker identities/watch state, and observe no duplicate implementation. Run `mise run planning-next -- --json` twice against identical Git/runtime inputs and retain identical ID/exclusions; priority/order remain unchanged. Record how many human interventions and time the restart required.

After all five scenarios and restart pass, finish their workers, resume bootstrap acceptance bookkeeping, collect independent fresh baseline acceptance review and report limitations. Only then prepare the Atomic comparison experiment. No Atomic artifact/installation is part of incomplete baseline acceptance.
