# Read-only workflow smoke report

Task: **setup-agent-workflow-poc-scout**
Role: FirstMate crewmate / Scout; no supervisory or implementation role
Inspection date: **2026-10-03**, local inspection through 08:43 UTC
Isolated worktree: **`/home/bart/.treehouse/odot-game-ab1b67/1/odot-game`**
Report: **`/home/bart/.local/share/firstmate/odot/data/setup-agent-workflow-poc-scout/report.md`**
Checkout HEAD: **`eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7`**, detached, clean
Deliverable: text report written; native completion gate **BLOCKED**, not passed.

All repository `file:line` references below resolve against the isolated worktree above. Findings use local files and Git metadata only. No online sources were consulted.

## Findings

**There is no eligible implementation change in the current roadmap.** Its first four entries are imported legacy completions; `setup-agent-workflow-poc` is blocked awaiting live acceptance; `add-city-research-and-status-effects` remains proposed without implementation approval. There is no `ready` entry. This is a static conclusion, not a newly executed `planning-next` result or a claim that current planning validation passed.

The local bootstrap is implemented and independently checkpoint-reviewed, but the workflow POC is incomplete: **15/26 task boxes are checked**, while live setup verification, Scout smoke acceptance, the five scenarios, restart, independent baseline acceptance and final/follow-up reporting remain open. The existence of this Scout report alone does not close any OpenSpec task or establish Pi/Herdr supervision acceptance.

Evidence: `planning/roadmap.yaml:3`, `planning/roadmap.yaml:35`, `planning/roadmap.yaml:44`; `openspec/changes/setup-agent-workflow-poc/tasks.md:27`; `planning/evidence/setup-agent-workflow-poc/handoff.md:19`.

## Local checkpoint and workflow state

The implemented checkpoint provides Git-backed ideas/roadmap/evidence, the C# planning validator and selector, five project workflow skills, ADR 0001, runtime templates, the thin managed-pane launcher, and live-scenario procedures. Git owns product truth; FirstMate owns execution. One explicitly approved change may be implemented at a time; review must be fresh and independent; one repair and a second fresh review are allowed; landing needs separate explicit approval. Existing OpenSpec skills remain the lifecycle path. (`docs/agent-workflow.md:3`; `docs/adr/0001-agent-workflow-authority.md:8`; `AGENTS.md:104`; `tools/AgentWorkflow/orchestrator.md:19`.)

The retained revision chain is:

| Revision | Local evidence and meaning |
| --- | --- |
| `4a70e16a152beee112a775b1f11f08da4ebc2277` | Clean bootstrap implementation base. |
| `df906227f336924c4bdeb12a6cabc44e814ab230` | Initial bootstrap implementation; checkpoint review 1 returned FAIL_IMPLEMENTATION. |
| `501021722a7e03fcde0cf9675db723f08a780707` | One repair; fresh checkpoint review 2 returned PASS for local bootstrap scope only. |
| `eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7` | This Scout's HEAD; records scoped checkpoint evidence, stopped Shipper and blocked roadmap state. |

Review 1 found that unreviewed production changes before a claimed landing could incorrectly clear dependent work. The repair binds landed production contents/modes to reviewed inputs and reuses native Herdr launcher identity checks. Review 2 reports no blocking finding in that checkpoint scope. Its reviewer was `/root/checkpoint_reviewer_2`, distinct from Shipper `/root/bootstrap_shipper`; review attempt 2 followed one repair. Neither review is a native live-scenario review or whole-change PASS. (`planning/evidence/setup-agent-workflow-poc/checkpoint-review-1.md:5`; `planning/evidence/setup-agent-workflow-poc/checkpoint-review-2.md:5`, `:9`, `:17`.)

The approval receipt records the user's local apply authorization and spec digest `ab621af466fb315725fcc8045cc19d7bdd4c09f751a988083b6e7652ce468dc2`; it grants no landing or trial implementation approval. Review 2 records the same spec digest and code digest `fd3171dff771350eb01bdbbb3cb512ee12bd33e252430fc5409c85afbc02eb70` at its reviewed head. These are retained receipt identities, not newly regenerated digests. (`planning/evidence/setup-agent-workflow-poc/approval.md:2`, `:11`; `planning/evidence/setup-agent-workflow-poc/checkpoint-review-2.md:18`.)

`git diff --name-status 501021722a7e03fcde0cf9675db723f08a780707 HEAD` lists only the workflow task file, review-2 receipt, handoff and roadmap. No production source differs between the reviewed checkpoint and this HEAD. The handoff says the original bootstrap Shipper stopped and the roadmap is blocked awaiting live acceptance. Current worker ownership was not independently inspected; no inference is made that unrelated workers or the other original-checkout task have stopped. (`planning/evidence/setup-agent-workflow-poc/handoff.md:10`, `:14`.)

Retained validation evidence includes a full before-CI PASS at the base and full after-CI retry PASS at `df906227...`, with total jobs 2 and UI cap 1. The initial after-CI attempt failed with unexplained exit 137; selected reconnect and the full retry subsequently passed. The repaired checkpoint reused unchanged game/network/UI/export coverage and reran affected cheap/static checks; **full CI was not executed at the repaired head**. Review 2 itself ran no tests/builds/restores. This Scout performed none either. (`planning/evidence/setup-agent-workflow-poc/bootstrap.md:9`, `:18`, `:20`; `planning/evidence/setup-agent-workflow-poc/handoff.md:6`; `planning/evidence/setup-agent-workflow-poc/checkpoint-review-2.md:22`.)

## First eligible change

**First eligible change: none.** If validation succeeds against these unchanged files, the selector's expected change is `null`; no CLI result is fabricated here.

| Queue order | Change | State | Exclusion |
| --- | --- | --- | --- |
| 1 | `scaffold-csharp-multiplayer` | `legacy_completed` | Historical 21/21 task completion; not ready work. |
| 2 | `enforce-strict-csharp` | `legacy_completed` | Historical 6/6 task completion; not ready work. |
| 3 | `speed-up-verification` | `legacy_completed` | Historical 27/27 task completion; pending canonical sync is separate. |
| 4 | `optimize-core-simulation` | `legacy_completed` | Apply-base 43/43 completion supersedes the earlier 13/43 snapshot. |
| 5 | `setup-agent-workflow-poc` | `blocked` | Earliest unfinished entry, awaiting live acceptance; checkpoint approval/PASS does not make it ready or complete. |
| 6 | `add-city-research-and-status-effects` | `proposed` | Complete planning and 0/40 implementation tasks recorded, but no implementation approval. |

Evidence: `planning/roadmap.yaml:3`, `:11`, `:19`, `:27`, `:35`, `:44`. Every entry currently has empty dependency, blocker and conflict arrays; the POC's operational blocker is expressed by its state and notes (`planning/roadmap.yaml:36`, `:42`). Empty `blocked_by` does not undo state `blocked`. All priorities are normal, and priority never overrides queue order or dependencies.

The checked-in selector requires state `ready` and rejects unfinished dependencies, blockers/conflicts and any queue implementation in progress; invalid planning prevents selection. (`tools/DevRunner/Planning/PlanningValidation.cs:80`; `planning/README.md:25`, `:50`.) The documented trial IDs are not roadmap entries and cannot be selected as if approved.

The roadmap-analysis skill normally runs `mise run planning-validate` and `mise run planning-next -- --json`. This brief forbids builds/restores. Both mise tasks depend on `runner`, which performs locked restore and build, so neither was executed. This is an explicit coverage gap; prior checkpoint command evidence is retained, but no current validator PASS is claimed. (`mise.toml:47`, `:50`, `:228`, `:234`; `.agents/skills/roadmap-analysis/SKILL.md:6`.)

## Prerequisites and blockers for fuzzy-idea intake

1. **Reconcile runtime ownership through the primary before future dispatch.** The Git queue contains no `in_progress` item and receipts say the bootstrap Shipper stopped, but current native workers, pending outcomes and watcher state must be reconciled by FirstMate. Preserve the other task in the original checkout and all unrelated processes/worktrees/sessions. This Scout performed no runtime or pool administration. (`AGENTS.md:106`; `docs/adr/0001-agent-workflow-authority.md:10`.)

2. **Verify the existing installed setup without rerunning bootstrap or overwriting it.** The brief supplies an installed-checkpoint premise. Git metadata confirms this isolated worktree uses common Git directory `/home/bart/.local/share/firstmate/odot/projects/odot-game/.git`, and its HEAD contains the bootstrap. However, tracked evidence contains only approval/bootstrap/handoff/checkpoint reviews, not a completed local installation or live smoke receipt; tasks 4.2 and 4.3 remain unchecked. Home configuration, registry delivery flags, trust and live relationships were not inspected here. Their verification/capture is a prerequisite for claiming acceptance, not a reason to reinstall or recreate anything. Required documented route: selected Odot home, clean recorded checkpoint clone, `odot-source` provenance without `origin`, Herdr backend, Codex harness, manual backlog, single static Codex dispatch objects, local-only and yolo off. (`docs/agent-workflow.md:24`, `:39`; `openspec/changes/setup-agent-workflow-poc/tasks.md:27`.)

3. **Use a real owned managed primary pane and trusted integrations.** The runbook records FirstMate revision `e31bc6e620ca532c2e0e0b72f3fd7c0869a12270`, Pi 1.0.0, Herdr 0.9.1, Codex 0.160.0/Codex integration v8 and Treehouse 3.1.1; Pi integration v9 was later confirmed. These are historical local observations, not fresh runtime compatibility checks. Live primary readiness requires actual inherited `HERDR_ENV=1` and session/pane/tab/workspace/socket identity, user-managed Pi login/trust, native session startup and Pi's `fm_watch_arm_pi` extension. Setting a session name alone is insufficient. No lifecycle action was taken under this unguarded brief. (`docs/agent-workflow.md:7`, `:9`, `:57`; `planning/evidence/setup-agent-workflow-poc/bootstrap.md:15`.)

4. **Keep the auxiliary gaps explicit and stay on a supported route.** The current worker's PATH lookup found all six tools below absent. None was invoked or installed; no daemon/socket was probed.

   | Tool | Recorded requirement / effect |
   | --- | --- |
   | `no-mistakes` | Upstream floor >=1.46.0; pipeline-dependent routes unavailable. This report uses no pipeline. |
   | `gh-axi` | Upstream floor >=0.1.29; dependent GitHub routes unavailable. No GitHub operation occurred. |
   | `chrome-devtools-axi` | Missing browser auxiliary; browser-dependent work unavailable. |
   | `tasks-axi` | Floor >=0.2.6 plus captain-hold contract; task-tool paths and this brief's native completion gate unavailable. Manual operational backlog is the selected project route, but does not exempt the upstream completion gate. |
   | `quota-axi` | Upstream floor >=0.1.51; quota-dependent routes unavailable; single static profiles avoid quota-array dispatch. |
   | `lavish-axi` | Absent from PATH. The launch brief already declares missing/below-floor compatibility and requires a text report. No numeric Lavish floor was established here, and no visual review was performed. |

   Evidence: `docs/agent-workflow.md:9`; `planning/evidence/setup-agent-workflow-poc/bootstrap.md:11`; task-local `launch-brief.md:105`; `/home/bart/projects/personal/firstmate/bin/fm-tasks-axi-lib.sh:45`. The selected manual/static/local route does not exercise every universal bootstrap tool, so missing GitHub/browser/quota tools alone do not prove local text intake impossible. GitHub authentication remains an upstream primary requirement according to the runbook; fresh authentication status was not checked.

5. **Supply the actual fuzzy problem and keep investigation separate from promotion.** The scenario prompt is “I have an idea: make it easier to tell which Odot verification command I need.” Future intake should use `project-intake`, read AGENTS, planning, ideas, ADRs, README, verification documentation, relevant runner source/tests, canonical Linux verification and all relevant active changes; return evidence, alternatives, assumptions, questions and promotion criteria without production edits. (`docs/agent-workflow-scenarios.md:19`; `.agents/skills/project-intake/SKILL.md:6`.)

6. **Capture a durable idea through the allowed planning workflow.** Both active/archive idea directories currently contain only their README, so IDEA-001 appears unused in this snapshot; recheck before capture. Use the exact template frontmatter and a stable ID/status, normally `exploring`, rather than inventing a ready change. Native Scout report collection precedes a narrowly authorized planning-only capture worker; planning landing requires separate explicit approval. A later proposal requires a NEW preflight Scout and all ten overlap/dependency questions, complete OpenSpec artifacts, reciprocal source links and applicable validation. No idea, proposal or worker was created by this report. (`planning/templates/idea.md:1`; `planning/README.md:19`; `.agents/skills/project-intake/SKILL.md:10`; `.agents/skills/proposal-preflight/SKILL.md:6`, `:21`.)

## Missing or conflicting evidence

- **Live installation/observation acceptance is missing from tracked receipts.** Actual FirstMate/Herdr task/session/workspace/tab/pane identities and primary trust/watch/collection evidence must be retained before marking 4.2/4.3 complete. This report records its actual task ID and worktree but has not inspected runtime identities. It cannot establish all of task 4.3 by itself.
- **Current deterministic validation is unexecuted** because the checked-in mise route builds/restores. No alternate validator, temporary fixture or bypass was created.
- **A verification-guidance conflict exists:** canonical `openspec/specs/linux-test-execution/spec.md:85` still requires unfiltered source UI serial execution, while AGENTS/README and the completed-but-unsynced verification delta describe bounded parallel UI (`README.md:116`; `openspec/changes/speed-up-verification/specs/linux-test-execution/spec.md:43`). Future intake/preflight must surface this before stating a consistent guarantee. No sync or silent resolution was performed.
- **Proposal-time missing-runtime and partial-optimization statements are historical**, explicitly superseded by apply-time design/receipt annotations (`openspec/changes/setup-agent-workflow-poc/proposal.md:32`; `design.md:20`; `planning/evidence/setup-agent-workflow-poc/bootstrap.md:7`, `:11`). They do not justify pretending the current workflow has no bootstrap, or treating later runtime acceptance as passed.

## Shared completion gate

Read and applied the report-review policy from [captain-hold-lifecycle SKILL.md](/home/bart/projects/personal/firstmate/.agents/skills/captain-hold-lifecycle/SKILL.md:25): “After inventorying the whole report and review surface, run `bin/fm-captain-hold.sh complete`”.

The reviewed surface leaves **no newly discovered present product/architecture choice requiring a captain decision**. Prerequisite gaps are factual blockers; future scenario approvals and the intentionally ambiguous future trial are not decisions opened by this smoke task. No captain-held backlog task was created or closed.

The mechanical gate **cannot be passed within this brief**. Static source inspection shows `complete` requires compatible `tasks-axi`, currently absent; it can acquire a metadata lock and append `decisions_reviewed`/`decision_keys` to the task's `.meta` file. The brief permits outside writes only to this report/status, plus explicit inbox acknowledgements; no metadata or backlog mutation is authorized. The gate was not invoked, bypassed or claimed successful. (`/home/bart/projects/personal/firstmate/bin/fm-captain-hold.sh:364`, `:1738`, `:1746`, `:1787`.)

FirstMate should resolve the completion prerequisite/scope conflict through its native owner before declaring this task done. This worker will leave a keyed **blocked** status, retain the report and stop; it will not install tools, change home bootstrap state, create holds, or administer shared infrastructure.

## Inspection evidence and scope assurance

Representative commands actually run and relevant outputs:

| Read-only command | Result |
| --- | --- |
| `pwd` | `/home/bart/.treehouse/odot-game-ab1b67/1/odot-game` |
| `git status --short --branch` | `## HEAD (no branch)`, no changes |
| `git status --porcelain=v1` | Empty |
| Final `git status --short --branch` after report creation | `## HEAD (no branch)`, no changes |
| `rg -c '^- \[x\]'` and `rg -c '^- \['` against the workflow task file | 15 checked tasks, 26 total |
| `git rev-parse --show-toplevel --git-common-dir HEAD` | Worktree above; common Git directory above; `eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7` |
| `git log -6 --format='%H %s'` | Confirms current bookkeeping, repaired checkpoint, initial implementation and base chain |
| `git diff --name-status 501021722a7e03fcde0cf9675db723f08a780707 HEAD` | Four workflow planning/receipt/task files only, listed above |
| `rg --files --hidden --no-ignore planning/ideas planning/archive/ideas planning/evidence/setup-agent-workflow-poc` | Two idea README files and five checkpoint/approval/handoff receipts; no live scenario/setup receipts |
| `cat`, `nl -ba`, `sed -n`, `rg -n` over the cited repository documents, skills, selector and local completion-gate source | Evidence summarized with file/line references above |
| Shell `command -v` lookup for the six auxiliary names | Each prints `absent from PATH`; no auxiliary executable invoked |
| `ls -ld` of the exact task inbox | Inbox absent at inspection checkpoints; no messages to acknowledge |
| `date +%s`, `date -u` | Numeric status timestamp and UTC inspection date; no background work |

Initial broad file probes encountered absent optional `.firstmate`, `.pi` and `.mise.toml` paths; actual guidance/config was found in tracked docs, skills and `mise.toml`. A shell glob for `Planning*.cs` matched nothing; `rg --files` located the actual `tools/DevRunner/Planning/` directory. These were inspection path misses, not evidence that required planning artifacts are absent.

The only deliberate writes are this requested report and the exact task status stream, using the brief's prescribed native ledger notification hook. There were **no project/game/production/configuration edits; tests, builds or restores; game/editor processes; package/tool installs; upstream/network/global changes; commits, branches or pull requests; implementation/trial/review dispatches; merges, syncs or archives; Herdr/Treehouse/pool/session/watcher administration; unrelated process kills; or inspection of other homes' endpoint namespaces**. The other original-checkout task and all unrelated owners were left untouched. No background shell or monitor was started.

Recommendation to FirstMate: retain this report, resolve only the native completion-gate blocker under the correct authority, and stop this Scout. Any later fuzzy-intake acceptance should start from the existing checkpoint through the primary with the prerequisites above; this report authorizes no follow-on action.
