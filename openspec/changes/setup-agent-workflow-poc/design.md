# Design

## Context

See `proposal.md` for motivation. Inspection on 2026-10-03 found:

| Surface | Existing evidence | Gap/adaptation |
| --- | --- | --- |
| OpenSpec | Local root, spec-driven schema, CLI 1.13.2, six lifecycle skills, 16 canonical capabilities | Keep the root/schema/skills; add one workflow capability |
| Active work | `optimize-core-simulation` has 13/43 tasks checked; `add-city-research-and-status-effects` has only metadata | Register conservatively; never infer approval from a directory |
| Completed active work | `speed-up-verification`, `enforce-strict-csharp`, `scaffold-csharp-multiplayer` have all tasks checked | Record legacy completion, not independent POC verification or automatic archive permission |
| Working copy | Many modified/untracked game, test, runner and documentation files; an existing Windows CI worktree | Preserve all edits and existing worktree owners; clean worker bases cannot include uncommitted changes implicitly |
| Planning | No `planning/`, project workflow skills or `docs/adr/` | Add the requested layout and a small evidence directory |
| Tooling | Locked .NET 10.0.401, Godot .NET 4.7.2, Node 24.21.0, cheap runner xUnit suite | Extend DevRunner, not Game.Core; preserve strict build and verification ownership |
| Runtime | Herdr 0.9.1/protocol 22 running; Codex CLI 0.160.0; Codex Herdr integration current v8 | Pi, Treehouse and FirstMate checkout not found; Pi Herdr integration absent |
| FirstMate prerequisites | Git, jq and gh available; no `no-mistakes`, `gh-axi`, `chrome-devtools-axi`, `tasks-axi`, `quota-axi` on PATH | Upstream bootstrap declares these auxiliary tools; report missing prerequisites even when the selected local-only route does not exercise all of them |

Read-only upstream research used [FirstMate](https://github.com/kunchenguid/firstmate), its [configuration](https://github.com/kunchenguid/firstmate/blob/main/docs/configuration.md), [Herdr adapter](https://github.com/kunchenguid/firstmate/blob/main/docs/herdr-backend.md), script headers and role guidance; [Pi CLI](https://github.com/earendil-works/pi/blob/main/packages/coding-agent/docs/cli.md); and [Herdr integration docs](https://herdr.dev/docs/integrations/). Observed upstream main revisions were FirstMate `d719ef3d9abdd11b0a8aea57c74de074feca99b9` and Pi `a276dabe57911253350bffb93cb7d7aff6a73261`. These are research references, not a claim of installed compatibility. Applying must record the actual supplied checkout/tool versions and verify their help/contracts before launch.

### Proposal preflight

This bootstrap preflight was performed in the current planning session because the requested FirstMate Scout runtime is not installed. It is repository discovery, not evidence that the future live proposal scenario passed.

1. **Existing solution:** no active change provides idea intake, roadmap validation or agent orchestration. Existing OpenSpec skills provide artifact lifecycle operations and must be reused.
2. **Duplicates:** no idea backlog exists; no workflow proposal duplicate was found among all five pre-existing active changes.
3. **Contradictions:** no game semantics change is needed. Current canonical Linux verification still says serial source UI; implemented `speed-up-verification` describes bounded parallel UI. Preserve existing artifacts and current repository instructions; this POC must not silently reconcile that unrelated pending sync.
4. **Prerequisites:** user-provided Pi/FirstMate/Treehouse and auxiliary prerequisites, credentials/integrations, and a clean recorded worker base. Existing optimization work must finish or be explicitly reconciled before another implementation is admitted.
5. **Dependents:** later workflow improvements and the Atomic comparison depend on this baseline's actual acceptance, not merely its task file existing.
6. **Merge:** keep separate from game optimization/research; there is no relevant orchestration proposal to merge into.
7. **Split:** keep one coherent bootstrap change, with incremental tasks and small separate live trial changes; do not build a reusable multi-project platform.
8. **ADR:** yes: execution/product-truth ownership, review independence and explicit merge authority are project-wide rules.
9. **Placement:** initially proposed behind existing implementation; ready/implementation transition requires explicit user approval. Preserve recorded existing order where known; do not invent relative product priority for the unfinished research proposal.
10. **Obsolete work:** none of the five active changes becomes obsolete. Task completion alone does not authorize their archive.

## Goals / Non-Goals

**Goals:** one conversational entrypoint; disposable scoped workers; reconstructible project state; deterministic validation/selection; measured real-session acceptance.

**Non-Goals:** Atomic integration, Beads/Linear/Agent Mail/Squad/BMAD, databases, a new event bus, upstream forks, parallel implementation of changes, unattended merges or a security isolation product. Worktrees separate edits but are not an OS sandbox.

## Decisions

### 1. External FirstMate checkout/home, project-owned configuration templates

Keep FirstMate outside Odot. Run Pi from its checkout so upstream project instructions and tracked Pi watcher extensions load naturally. Use an Odot-specific `FM_HOME`, named `HERDR_SESSION=odot-poc`, and a registered clean clone/base of Odot under FirstMate's project directory. Preserve the existing developer checkout; do not stash, reset, copy unrelated dirty files, or kill its sessions. Record how the clean registered clone relates to the original checkout and which Git revision it contains.

Store a small launcher, configuration templates and orchestrator overlay under `tools/AgentWorkflow/`; add setup/runbook documentation under `docs/agent-workflow.md`. Proposed command: `mise run agent-primary`, accepting local `ODOT_FIRSTMATE_ROOT` and `ODOT_FIRSTMATE_HOME` paths from ignored `mise.local.toml` or environment. The launcher checks prerequisites and project identity, sets the existing upstream environment variables, then invokes Pi. It must not spawn/supervise workers itself. Initialize only an explicitly selected POC home; refuse conflicting configuration instead of overwriting it. No repository paths, credentials or normal Herdr settings are hard-coded into tracked templates.

Home configuration:

```text
config/backend       herdr
config/crew-harness  codex
data/projects.md     Odot registered with local-only delivery, yolo off
```

Use upstream registry tooling/format from the supplied checkout. Do not treat `firstmate` as an executable requirement: its checkout and `bin/fm-*.sh` scripts are the interface. Prefer local-only delivery to avoid PR publication during this experiment. FirstMate's native guarded local landing is a clean fast-forward and refuses dirty/diverged targets. Any later PR route also retains explicit merge approval.

If differentiated routing is useful, the minimal supported dispatch template is:

```json
{
  "rules": [
    {"when": "Odot Scout: intake, investigation or proposal preflight", "use": {"harness": "codex"}},
    {"when": "Odot Shipper: exactly one approved OpenSpec change", "use": {"harness": "codex"}},
    {"when": "Odot Reviewer: independent read-only OpenSpec verification", "use": {"harness": "codex"}}
  ],
  "default": {"harness": "codex"}
}
```

Use single profile objects; no model pin, quota-array selection, typed resolver service or custom role dispatch API. Upstream dispatch requires an explicit resolved harness when this file exists; the primary passes `--harness codex`. Approval is enforced by project workflow instructions and recorded approval, not an inert dispatch hint.

Herdr uses the named owned session and existing Codex integration. The user supplies Pi's integration/trust/login setup. Never update the normal Herdr configuration or global credential stores. Startup documents the concrete session attach command and worker workspace/tab identities after the pinned backend actually creates them; do not assume an older workspace naming scheme.

Alternatives rejected: vendoring/forking FirstMate; implementing spawn/watch/recovery in DevRunner; using Herdr's separate worktree API alongside Treehouse.

### 2. Reconcile role and instruction boundaries with a small overlay

Append a tracked Odot orchestrator instruction file through Pi's existing `--append-system-prompt <path>` option, and load Odot skills using Pi's supported `--skill <directory>` argument. The overlay instructs the primary to read the project guidance, ADR, roadmap and selected change; delegate substantial investigation/implementation; and keep user discussion separate from worker contexts. Avoid copying upstream instruction files or watcher extensions into Odot.

FirstMate currently defaults to more concurrency and permits promoting an existing Scout. The explicit project rules narrow implementation to one change and require fresh implementation/review contexts. A completed preflight Scout is not promoted into the Shipper. Upstream Scout tasks produce `data/<id>/report.md` and scratch work, not landed project changes. The primary collects this report; durable idea/proposal/roadmap capture uses a planning-only Codex worker with a FirstMate Ship delivery contract and the logical Scout role. Its brief authorizes only planning paths and carries the report; planning authorization never grants production implementation. This is a task shape, not a fifth logical role. Durable planning changes use the same guarded, explicitly approved landing path.

The primary does not directly rewrite project files or AGENTS.md. Bootstrap changes to root guidance are part of this authorized change's implementation worker; later planning writers cannot edit root guidance. FirstMate's runtime task records may point to idea/change/report IDs but cannot decide product order. Reconcile runtime work with Git state on startup; discrepancies stop new implementation until explained.

### 3. Git-backed planning model with small lifecycle records

Create the requested five skills and retain existing OpenSpec skills unchanged. `planning/README.md` documents truth ownership, idea frontmatter/body, schema/states, routing, evidence and approvals. Empty ideas/archive directories use a tracked README or placeholder. Idea IDs remain stable when archived; reject duplicate IDs across both locations.

Use the user's version-1 queue fields verbatim: `change`, `state`, `priority`, `source_ideas`, `depends_on`, `blocked_by`, `conflicts_with`, `notes`. Add optional `approval` and `verification` paths to small Markdown records under `planning/evidence/<change>/`. These records hold actual user authorization, reviewed revision/input digests, worker task/session IDs, handoff, review findings and validation results. Store large/non-secret raw logs under ignored `logs/<run-id>/`; durable reports include command/result summaries so losing logs does not erase lifecycle decisions.

States: `proposed`, `ready`, `in_progress`, `blocked`, `verified`, `completed`, `archived`, `legacy_completed`. `ready` means the user has approved implementation for the recorded specification inputs. `verified` means independently reviewed but not necessarily landed. New `completed` means verified work is landed; `archived` additionally resolves to its archived OpenSpec directory. Legacy completion describes imported historical work only and is never eligible. Preserve prior `in_progress` work with an explicit migration note; do not fabricate an approval/review receipt. This blocks POC trial implementation until the existing work is reconciled.

`blocked_by` lists unresolved human-readable blockers; remove one only with a recorded resolution. `conflicts_with` references change IDs and remains unresolved while the referenced work is not completed/archived/legacy-completed; explicitly record any other conflict resolution. Priority is descriptive and does not change selection. Archived dependency references resolve through an exact indexed change ID from the dated archive directory; ambiguous archive identity fails validation. Dependencies may refer to historical archived changes outside the queue; active dependencies must be queue entries.

Do not migrate every historical change into the ordered queue. Seed the five active entries conservatively from inspected artifacts, and the bootstrap proposal as proposed; completed active changes use `legacy_completed`. Keep unfinished proposals unapproved. No source ideas are invented for existing work. Record established order/dependencies only; validator rejects unresolved guesses presented as ready work.

An ADR records ownership, single-change admission, independent review and merge posture. Feature-specific design remains in its change; future ADRs supersede historical decisions.

### 4. Small deterministic C# validator and selector

Add a focused planning component in `tools/DevRunner`, separate from game/display lifecycle code. Use a maintained YAML parser, preferably a deliberately added locked YamlDotNet package, rather than a homemade YAML grammar or reliance on OpenSpec's transitive Node packages. Update only affected NuGet locks during this intentional dependency addition; all subsequent restores stay locked. Read strict typed fields, disallow duplicate keys/unknown schema versions, identify malformed idea frontmatter and constrain paths/IDs to the repository. Diagnostics are stable and actionable.

Expose proposed `mise run planning-validate` and `mise run planning-next` commands with human output and optional `--json`; neither invokes an LLM, FirstMate, network or Godot. The selector validates first, then returns the first eligible item and explanations for earlier exclusions. No eligible work is a successful result with `change: null`; invalid state is nonzero. Check full OpenSpec planning readiness through files and metadata, honoring legitimate `skip_specs`; do not mistake status/task existence for requirement completion. Validation need not require complete planning for unshaped/proposed placeholders.

Use cheap xUnit fixture tests for graph/order/cycle/reference/state/idea/archive rules, deterministic selection, unresolved conflicts, no eligible items and stale lifecycle evidence. Include planning validation in the existing CI source gate and cheap tests without introducing agent credentials into ordinary CI. Static validation can verify receipt content and digest consistency; it cannot prove an agent or user identity cryptographically. The primary separately reconciles live FirstMate implementation work before dispatch. No custom runtime locks/worker state machine are added.

Alternatives rejected: LLM-only ordering; external product tracker; a dedicated service/database; a second validator in another language.

### 5. Workflow routes and revision-bound completion

| User intent | Route |
| --- | --- |
| Idea / help me think | project-intake; direct discussion or fresh Scout; persist idea/report |
| Research | fresh Scout with standalone report |
| What ideas | read idea artifacts and summarize |
| Turn idea into proposal | fresh proposal-preflight Scout; existing OpenSpec propose/update; planning capture; validate |
| Update proposal | existing OpenSpec update; invalidate affected approval/PASS; reconcile links/order |
| What's next | roadmap-analysis using planning-next, without mutation |
| Do the next thing / implement X | resolve one complete approved change; record explicit authorization; Shipper |
| Check X | fresh Reviewer against exact owned diff and artifacts |
| Archive X | current PASS/tasks/validation/decision gate; existing sync/archive; update roadmap |

Implementation handoff and review output use the exact requested fields/outcomes. Reviewer starts as a fresh FirstMate Scout task, using its own isolated worktree at the implementation revision and immutable base/head diff. Provide the proposal/artifact inputs and handoff as documents, not the Shipper transcript. Never run the Reviewer concurrently against a moving Shipper checkout. Test-generated `bin/obj/logs` are permitted, but compare tracked source before/after to prove no production edits. Durable review results are captured by a scoped planning writer or included in the owned change branch.

`FAIL_IMPLEMENTATION` returns evidence through FirstMate's existing steering channel to the Shipper; resume that Shipper or recover it through upstream controls if needed. Exactly one repair and a second new Reviewer are allowed. Store attempt counts and reviewed inputs in Git evidence and link them from FirstMate's operational task; preserve any pending receipt before a restart. A second failure, `FAIL_SPEC` or `NEEDS_HUMAN_DECISION` returns to the primary/user. A material spec update requires renewed implementation approval. Runtime supervision events are not a substitute for these durable findings.

PASS covers base/head implementation identity and digests of specifications/tasks/canonical inputs. Later production/spec changes invalidate it. Lifecycle-only receipt/roadmap/archive movement can preserve it when the checked requirement contents and reviewed code remain identical. Archive uses existing OpenSpec lifecycle skills; do not run sync twice when archive already handles it. An archive-only planning worker may prepare bookkeeping after PASS without further product approval, but does not acquire merge authority. Completion is reported separately for the reviewed branch, archived artifact and landed default branch; dependencies clear only after actual landed completion, not branch-local preparation.

### 6. Incremental bootstrap and live acceptance

The initial apply builds configuration/docs/skills/validator; this is bootstrap work, not evidence that the Pi orchestration loop already works. Missing prerequisites permit independent local implementation progress but keep live scenario tasks incomplete. Record full `mise run ci` before/after this substantial DevRunner change, reusing a baseline only for unchanged inputs; use cheap/targeted checks while editing. Planning/docs-only trial changes need consistency checks, not repeated game exports. Do not add unrelated Godot UI/network scenarios to test an agent workflow.

Before implementing any trial change, finish the bootstrap coding checkpoint, collect an independent checkpoint review and release its active Shipper. Record the bootstrap roadmap state as `blocked`, awaiting live acceptance, rather than keeping an implementation in progress during trial work. This checkpoint is not PASS/completion for the entire bootstrap change: its live acceptance tasks remain open. Trials run sequentially on the recorded bootstrap checkpoint with its approval, and do not claim a completed hard dependency on the unfinished bootstrap. Their notes identify the installed checkpoint as a test prerequisite. After trials finish, resume bootstrap acceptance bookkeeping and final review. Thus no two changes have active implementation workers or `in_progress` state simultaneously.

The runbook will supply exact launcher/observer commands and the following primary-session prompts, concrete trial change IDs and checked-in fixture procedures. Run on real Odot history with owned branches/worktrees; request explicit approval for each first implementation and each landing. One implementation at a time includes trial changes.

| Scenario | Concrete experiment | Required evidence |
| --- | --- | --- |
| 1: fuzzy idea | “I have an idea: make it easier to tell which Odot verification command I need.” | Scout reads AGENTS/README/verification docs/specs; useful options/questions; durable idea; unchanged production diff |
| 2: proposal | Shape that idea, then “Turn IDEA-X into a proposal.” | New preflight Scout, all ten answers, small documentation change, roadmap/source links, validator success |
| 3: simple implementation | Approve a small documentation-only improvement from scenario 2; “Implement X.” | Isolated Shipper, consistency checks, fresh Reviewer PASS, gated archive and explicit landing, roadmap update |
| 4: review catches defect | One small validator-diagnostic trial; after implementation, inject a controlled next-selection violation that ignores an unfinished dependency in its owned worktree | Real Reviewer checks requirement/diff without being told defect location; detects failure; Shipper repairs; fresh review 2 PASS or escalation; fault labelled and never landed |
| 5: specification problem | An owned trial for an idea-summary improvement encounters an undefined ordering decision: idea creation date and stable idea ID imply different visible orders in real idea records | Shipper identifies the missing product rule before affected implementation; OpenSpec update/user decision route; no invented ordering or affected production edit |

Scenario 4 uses a checked-in selectable fixture/fault-injection procedure, not a permanently broken production change. A naturally discovered suitable defect can substitute with equivalent evidence. Scenario 5 uses an actual unanswered product rule in a scoped trial proposal rather than ambiguity in the already explicit next-change contract. Label any intentionally under-specified fixture; prefer a naturally discovered unresolved assumption with equivalent evidence. Park unresolved trial work accurately and retain its report; do not archive it through the verified-completion route or label it implemented.

Repeat planning-next against identical Git state and re-enter a fresh Pi primary while a worker/outcome is pending. Verify the same selection, recovered worker identity, approvals/repair bounds and no duplicate implementation. Record scenario duration, setup/human effort and prerequisite limitations. These supervised live exercises are manual acceptance through one primary, not ordinary credential-bearing CI; recurring fixture/check commands are checked in.

After all acceptance passes, write `docs/agent-workflow-atomic-experiment.md`: compare the same one or two small changes on equivalent clean bases with Codex Shipper → fresh Reviewer → one repair versus Atomic's deterministic checks/fresh or parallel review/bounded repair. Measure meaningful defect detection, false findings, evidence completeness, restart success and human interventions/time. Adopt only after a material observed improvement outweighs setup/maintenance cost. Do not install or integrate Atomic in this change.

## Risks / Trade-offs

- [Upstream tools change rapidly] → Record actual versions/checkout commit, validate help and integrations, retain setup evidence; fail clearly on unsupported contracts.
- [FirstMate bootstrap brings more prerequisites than expected] → Report its actual tool surface; local-only/single profiles minimize exercised complexity without claiming prerequisites are absent or creating substitutes.
- [Dirty source differs from worker base] → Preserve ongoing work, record a clean authoritative revision, and never import unrelated edits implicitly; landing can remain blocked until the target is clean.
- [Prompt permissions are not an OS sandbox] → Narrow briefs, separate worktrees, read-only Reviewer source checks and structured evidence; do not claim hard containment.
- [Receipt/roadmap drift on restart] → Validate tracked records and reconcile FirstMate runtime before selecting/dispatching; preserve outstanding branch reports.
- [Fault injection looks like naturally caught model error] → Label controlled defects and report the narrower evidence claim honestly.
- [PASS does not mean main is updated] → Separate verification/archive preparation from explicit guarded landing and completed dependencies.

## Migration Plan

1. Preserve the current worktree and inventory; implement bootstrap on an isolated recorded base after an apply request.
2. Add conservative planning state, ADR/skills and cheap validator/selector with regression fixtures.
3. Add templates/launcher/runbook and check missing-prerequisite failure behavior without installing anything.
4. When user-managed prerequisites and an eligible clean base are available, configure only the owned home/session, load upstream extensions, verify a visible scoped worker and execute the five scenarios serially.
5. Independently review bootstrap/trial changes, retain durable reports, and report verified versus unexecuted work. Only after complete baseline acceptance prepare the Atomic experiment.

Rollback removes only this change's tracked additions and configuration from its owned POC home/session after upstream-owned workers finish or are safely recovered. Preserve unlanded branches/reports and existing developer/Windows CI worktrees. No upstream source modification or global preference rollback is required.
