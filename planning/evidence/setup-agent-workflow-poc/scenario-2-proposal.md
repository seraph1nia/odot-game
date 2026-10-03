# Scenario 2: verification-command documentation proposal

Date: 2026-10-03. Task: `poc-scenario2-proposal`. Role: fresh Codex
proposal-preflight Scout using native local-only Ship delivery for planning
capture. This report records proposal preparation, not implementation, independent
implementation PASS, landing or whole-POC completion.

## Standalone proposal preflight

The assigned scope promotes [IDEA-001](../../ideas/IDEA-001-verification-command-guidance.md)
into `poc-verification-command-guide`, a small task-based documentation proposal.
The only recorded human shaping choice is **Documentation guide** (scenario 1,
FirstMate inbox `002.msg`, 2026-10-03T09:37:13Z). The task brief now specifies
small task-based scope. Audience and surface recommendations below are proposals
for review, not additional human decisions or implementation authorization.

| # | Question | Answer and evidence |
| --- | --- | --- |
| 1 | Does active work already solve some/all of this? | Some. [Canonical cost guidance](../../../openspec/specs/linux-test-execution/spec.md), [AGENTS](../../../AGENTS.md#choose-checks-by-cost-and-affected-behavior), [README](../../../README.md#verification-and-exports) and [verification records](../../../docs/verification.md#verification-speed-implementation-coverage-ownership) already explain tiers, preparation, coverage and cost. Completed-but-unsynced [speed-up-verification](../../../openspec/changes/speed-up-verification/proposal.md) updated those facts. A short task-to-check entry point is the remaining discoverability improvement proposed here; no usability incident has proved its effect. |
| 2 | Is it a duplicate idea/change? | No separate duplicate idea exists. There is wording/content overlap with speed-up-verification's command-guidance task, but its 27/27 tasks are complete and this proposal adds only a compact navigation path to existing guidance. It does not reimplement that change or duplicate its coverage map. |
| 3 | Does it contradict an active proposal? | No proposed command or policy changes. The canonical Linux spec still says serial UI, four network groups and the former dev arrangement; the completed [Linux delta](../../../openspec/changes/speed-up-verification/specs/linux-test-execution/spec.md), current source and root guidance describe bounded graphical overlap, six network selectors and playable host/guest dev. Record this existing drift explicitly; do not reproduce obsolete examples or silently sync specifications. |
| 4 | What must be implemented first? | No hard implementation dependency. The installed workflow checkpoint and committed intake are present. Canonical sync, research, optimization and whole setup-agent-workflow-poc completion are not prerequisites for documenting existing behavior. Prefer checking current source and guidance at apply time to avoid stale examples; that is consistency work, not a dependency edge. |
| 5 | What later work depends on this? | Runbook scenario 3 can use this concrete proposal after separate explicit implementation approval. No existing production proposal has a hard dependency on this guide. The eventual POC acceptance report consumes scenario evidence without making this trial depend on completed bootstrap acceptance. |
| 6 | Should it merge with another proposal? | No. Keep this small docs trial independently reviewable. Do not reopen completed speed-up-verification, fold it into broader [research](../../../openspec/changes/add-city-research-and-status-effects/proposal.md), or treat workflow acceptance as documentation implementation. Shared README edits with future research need ordinary integration attention, not a semantic conflict edge. |
| 7 | Should it split? | No. One compact task-based README addition, linked to existing owners, is one coherent documentation slice. Canonical synchronization, runner help/recommendation behavior, benchmark work and broader references are excluded rather than added as subprojects. |
| 8 | Is a project-wide ADR needed? | No new architectural decision. [ADR 0001](../../../docs/adr/0001-agent-workflow-authority.md) already owns planning/execution/approval boundaries. A local documentation organization recommendation belongs in this proposal/design. |
| 9 | Where does it belong in roadmap order and why? | Append as unapproved `proposed` after the existing entries, preserving their order. `depends_on`, `blocked_by` and `conflicts_with` are empty: no unmet semantic dependency or decision blocks proposal preparation. Order among unapproved proposals is provisional, not a product-priority choice. Note the installed checkpoint without claiming the blocked POC is completed. |
| 10 | Which existing proposals become obsolete? | None. Preserve all six active changes and their actual roadmap states. The guide neither supersedes verification-speed/optimization contracts nor changes research or workflow acceptance. |

Read-only inventory: `scaffold-csharp-multiplayer` (21/21),
`enforce-strict-csharp` (6/6), `speed-up-verification` (27/27),
`optimize-core-simulation` (43/43) are `legacy_completed`;
`setup-agent-workflow-poc` is `blocked` with incomplete live acceptance;
`add-city-research-and-status-effects` is unapproved `proposed` (0/40).
CLI task-presence labels do not grant lifecycle approval. Related ideas, the
whole active tree, relevant delta/canonical verification requirements, planning
conventions, orchestrator, ADR and scenario runbook were inspected read-only.

Source checks confirmed the proposal's examples against [mise tasks](../../../mise.toml),
[planning dispatch](../../../tools/DevRunner/Program.cs),
[runner commands](../../../tools/DevRunner/Runner.cs),
[option validation](../../../tools/DevRunner/Options.cs),
[selectors/admission/evidence](../../../tools/DevRunner/Scenarios.cs),
[cheap partitions](../../../tools/DevRunner/CheapTests.cs),
[network witnesses](../../../tools/DevRunner/NetworkTests.cs),
[private UI selection](../../../tools/DevRunner/PrivateDisplay.cs) and
[source/export gates](../../../tools/DevRunner/Exports.cs).
[Harness tests](../../../tests/DevRunner.Tests/HarnessTests.cs),
[admission tests](../../../tests/DevRunner.Tests/AdmissionTests.cs) and
[planning tests](../../../tests/DevRunner.Tests/PlanningTests.cs) were inspected,
not executed. Planning dispatch uses the calling worktree before constructing
engine/evidence infrastructure. Ordinary mise planning wrappers build/restore
DevRunner; the authorized prebuilt-DLL invocation is this task's no-build
validation exception, not proposed everyday documentation.

## Recommended proposal and review choices

Recommend serving human contributors and coding agents doing routine edits,
with a short task-based section at the start of README's Verification and exports
section. Six examples cover cheap rules/runner checks, affected networking,
affected rendered controls/recovery, existing exports, substantial implementation
boundaries and documentation/planning-only changes. Link prerequisites, current
selector lists, evidence and the full policy rather than duplicating their detail.
Profiles, platform packages and paired Steam acceptance remain separate links.

These routine audience/surface recommendations remain reviewable. No material
product or architecture decision is needed to finish planning; implementation
approval and landing approval remain future, separate authorizations.

## Fresh-primary restart evidence carried forward from scenario 1

This is supervisor-supplied historical evidence from the [scenario 1 native
report](/home/bart/.local/share/firstmate/odot/data/poc-scenario1-fuzzy-intake/report.md),
supplement after its Git receipt commit. The worker did not restart or inspect
Herdr, arm a watcher, run next-work selection or recover workers.

| Field | Recorded value |
| --- | --- |
| Prior Pi session record | `2026-10-03T08-28-52-015Z_01a100e1-5dae-73a7-819f-6766da4684f7.jsonl` |
| Recovered task / endpoint | `poc-scenario1-fuzzy-intake` / `odot-poc:w4:p2` |
| Worktree / branch | `/home/bart/.treehouse/odot-game-ab1b67/1/odot-game` / `fm/poc-scenario1-fuzzy-intake` |
| Recovery observation head | `1f47c9a4a17a795ddeec968a8e3653813df41099`, receipt staged |
| Final committed intake head | `fe42f72766ec839ecead121f87d63c0123532e1d` |
| Registered clean HEAD before/between/after both selections | `eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7` |
| Recovery / watcher | Existing local-only/yolo-off task recovered; no duplicate dispatch or implementation. Watcher extension-owned; `fm_watch_arm_pi` reported unchanged because the extension already owned an arm child. |

Two identical no-build `planning-next --json` calls ran from the registered
project root using its already-built runner. Both returned:

```json
{"valid":true,"change":null,"diagnostics":[],"exclusions":["scaffold-csharp-multiplayer: state legacy_completed","enforce-strict-csharp: state legacy_completed","speed-up-verification: state legacy_completed","optimize-core-simulation: state legacy_completed","setup-agent-workflow-poc: state blocked","add-city-research-and-status-effects: state proposed"]}
```

Raw non-secret provenance named in the native report:
`/home/bart/.local/share/firstmate/odot/state/poc-scenario1-fuzzy-intake.inbox/handled/004.msg`.
The task brief and native supplement supplied this evidence; no other task inbox
or endpoint namespace was inspected. Scenario 1's [Git report](scenario-1-fuzzy-intake.md)
and all three intake commits remain intact.

## Capture and validation receipt

Preflight was captured before `openspec new change`. OpenSpec resolved the nearest
root to the worker worktree and used configured `spec-driven` schema. Its
instructions permit `skip_specs: true` for docs with no spec-level behavior
change. Final status: **3/3 planning artifacts complete, specs skipped**; four
implementation tasks remain unchecked. This is planning readiness, not approval.

Seven allowed Git files form this delivery:

- `openspec/changes/poc-verification-command-guide/.openspec.yaml`, `proposal.md`,
  `design.md`, `tasks.md`: docs-only metadata, motivation/scope, concrete six-task
  examples and the small implementation/consistency checklist. No delta specs.
- `planning/ideas/IDEA-001-verification-command-guidance.md`: promoted, reciprocal
  new-change link; historical overlap references and shaping evidence retained.
- `planning/roadmap.yaml`: one appended `proposed` item, `source_ideas: [IDEA-001]`,
  no hard dependencies/blockers/conflicts/approval/verification receipts. Existing
  queue entries and order unchanged.
- This scenario 2 preflight/restart/capture report only; other POC evidence and
  active proposals are unchanged.

Both required commands ran from
`/home/bart/.treehouse/odot-game-ab1b67/2/odot-game` and passed:

```text
openspec validate poc-verification-command-guide --strict
Exit 0: Change 'poc-verification-command-guide' is valid
INFO: skip_specs declares no spec-level behavior changes; zero deltas accepted.

dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-validate --json
Exit 0: {"valid":true,"change":null,"diagnostics":[],"exclusions":[]}
```

Observed SDK: `10.0.401`; OpenSpec executable resolved to installed `1.13.2`.
Prebuilt DLL SHA-256:
`c3d72c52cc31073baaa583ab1e8bf31f71479f05e6b947662ce73cfe2d380718`.
This establishes consistency using that existing binary, not fresh compilation.
The authorized no-build call replaces the runbook's ordinary building mise wrapper
for this worker only. `change:null` here is validator output, not a new selection
run. The repeated selection result above is historical supervisor evidence.

`git diff --check` passed. A one-off read-only Python Markdown-link inspection
passed all 55 relative file/anchor links across the idea, evidence and three
new Markdown artifacts. It is documentation consistency, not a recurring game
acceptance check. The tracked diff outside the seven allowed files is empty.
No production source, runner, tests, root guidance/docs, canonical specs, locks,
other changes or source scenario 1 evidence were edited. No install, restore,
build, game/unit test, Godot, CI, export, pipeline, browser/remote operation or
additional worker ran; there is no fresh runtime acceptance claim.

| Identity | Value |
| --- | --- |
| Worktree / branch | `/home/bart/.treehouse/odot-game-ab1b67/2/odot-game` / `fm/poc-scenario2-proposal` |
| Exact branch base | `fe42f72766ec839ecead121f87d63c0123532e1d` from committed intake, not from main |
| Default main ancestry | `eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7`, preserved ancestor |
| Observed Herdr environment | session `odot-poc`, workspace `w5`, tab `w5:t2`, pane `w5:p2` |
| Endpoint derived from that environment | `odot-poc:w5:p2`; no lifecycle/endpoint inspection performed |
| Branch creation | 2026-10-03T09:54:53Z (native branch reflog); isolation checks preceded it |
| Final immutable head and elapsed delivery time | Recorded after commit in the native task-local report below; the shared branch resolves that head |
| Native report | `/home/bart/.local/share/firstmate/odot/data/poc-scenario2-proposal/report.md` |

FirstMate's launch brief supplied planning-only authority. No additional human
intervention or shaping answer was received by this worker during capture.
The task inbox was absent at observed checkpoints; later messages, if any, must
be handled before terminal delivery. Native completion inventory is recorded in
the final native report. No material decision is held for completing planning;
audience/surface wording remains for proposal review and explicit implementation
approval. **Implementation approval was not created.**

Branch/worktree/report owners are retained for FirstMate. No game peers, owned
displays, runtime-data owners or background jobs were created; no Herdr lifecycle,
watcher management or worktree cleanup occurred. Commit and clean named-branch
checks precede the terminal local-only done line; separate review and landing
remain pending. Main and all scenario 1 commits are retained without rewriting.
