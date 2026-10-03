# Scenario 1: fuzzy verification-command intake

Date: 2026-10-03. Outcome: **intake captured; separate review/landing pending**.
Task `poc-scenario1-fuzzy-intake`: one planning-only logical Scout under native
local-only Ship delivery. [IDEA-001](../../ideas/IDEA-001-verification-command-guidance.md)
remains `exploring`. The human selected **Documentation guide** during intake.
No proposal, implementation approval, roadmap/task update, sync, archive, push or
landing occurred. This is not an independent PASS or whole-POC completion.

## Standalone findings and choices

The fuzzy problem is difficulty choosing an Odot verification command. No
concrete confusing task or intended reader was supplied. The desired outcome is
choosing a suitable check and understanding preparation, cost and coverage.
Distributed explanations plausibly contribute, but that is an inference, not an
observed usability incident. Checked-in behavior, without fresh game execution:

| Concern | Existing command | Distinction |
| --- | --- | --- |
| Frequent numerical/session/runner checks | `mise run test` | Both cheap C# suites; restore/build, no Godot |
| Affected transport/lifetime | `mise run test-network --scenario NAME` | Selected headless case; source preparation; partial coverage |
| Affected controls/recovery/rendering | `mise run test-ui --scenario NAME` | Selected owned-display source slice; preparation; partial coverage |
| Existing exports | `mise run test-ui --scenario exported-package` | Requires existing exports; no source preparation or implicit rebuild |
| Substantial implementation boundary | `mise run ci` | All required source gates, sequential Linux exports and package checks; reusable unchanged before baseline |
| Documentation/planning only | Relevant consistency checks; `mise run planning-validate` for planning | No automatic full game/export run; planning mise wrapper still restores/builds DevRunner |

Evidence: [README](../../../README.md#verification-and-exports),
[AGENTS](../../../AGENTS.md#choose-checks-by-cost-and-affected-behavior),
[mise.toml](../../../mise.toml), [Program](../../../tools/DevRunner/Program.cs),
[Runner](../../../tools/DevRunner/Runner.cs), [Options](../../../tools/DevRunner/Options.cs),
[Scenarios](../../../tools/DevRunner/Scenarios.cs),
[PrivateDisplay](../../../tools/DevRunner/PrivateDisplay.cs),
[CheapTests](../../../tools/DevRunner/CheapTests.cs),
[NetworkTests](../../../tools/DevRunner/NetworkTests.cs), and
[CI gates](../../../tools/DevRunner/Exports.cs).
[HarnessTests](../../../tests/DevRunner.Tests/HarnessTests.cs),
[AdmissionTests](../../../tests/DevRunner.Tests/AdmissionTests.cs), and
[PlanningTests](../../../tests/DevRunner.Tests/PlanningTests.cs) cover selection,
ownership and planning semantics; inspected, not executed.

Source has six network selectors and five source UI slices, default concurrency
two and coverage-preserving serial overrides. `dev` opens desktop host/guest
windows; private UI checks own Linux displays. Selected success is partial
coverage. Steam, profiles and native package qualification have separate scope.
[Verification evidence](../../../docs/verification.md) mixes current and historical
counts/timings; its final optimization record reports 27.655s cheap / 240.414s
full CI commands on those measured inputs, not portable duration guarantees.

[Canonical linux-test-execution](../../../openspec/specs/linux-test-execution/spec.md)
already requires cost-based guidance. Older examples retain four network groups,
serial UI and the former `dev` arrangement.
[speed-up-verification](../../../openspec/changes/speed-up-verification/proposal.md)
already covers guidance; its completed, unsynced
[Linux delta](../../../openspec/changes/speed-up-verification/specs/linux-test-execution/spec.md)
explains current concurrency. Future preflight must address overlap and separate
canonical-sync authority. Other inspected relevant active work:
[optimize-core-simulation](../../../openspec/changes/optimize-core-simulation/proposal.md)
(selected correctness/profiles), [enforce-strict-csharp](../../../openspec/changes/enforce-strict-csharp/proposal.md)
and [scaffold-csharp-multiplayer](../../../openspec/changes/scaffold-csharp-multiplayer/proposal.md)
(task/style foundations). These four roadmap entries are `legacy_completed`.
[add-city-research-and-status-effects](../../../openspec/changes/add-city-research-and-status-effects/proposal.md)
is unapproved `proposed`, with relevant check extensions rather than a selection
solution. [setup-agent-workflow-poc](../../../openspec/changes/setup-agent-workflow-poc/proposal.md)
remains `blocked`; existing tasks are untouched. Active/archived idea directories
contained only READMEs, so `IDEA-001` was unused.
[ADR 0001](../../../docs/adr/0001-agent-workflow-authority.md) and
[planning conventions](../../README.md) govern authority/promotion.

| Explored option | Benefit | Trade-off |
| --- | --- | --- |
| Short task-based documentation guide | Fast common decisions; low maintenance | Needs agreed examples and links to policy |
| Comprehensive reference | Compares coverage/preparation/evidence | Longer page; repeated facts can drift |
| Runner help/recommender | Discoverable at invocation | Production work and undefined semantics; not the selected direction |
| Reconcile guidance/spec drift first | Fewer contradictions | Separate lifecycle authority; may not solve this confusion |

FirstMate relayed the human choice **Documentation guide** at 09:37:13Z. Audience,
location, content boundaries and acceptance examples remain open. Useful questions:

- Is the confusion tier choice after an edit, scenario names, prerequisites, or
  whether a pass is enough? Which concrete task should the guide solve?
- Is the reader a human contributor, coding agent, or both?
- Prefer a short README path, an existing verification section, or a separate
  reference page? Should Steam/profile/platform tasks be linked separately?

Recommendation: shape those choices with FirstMate; a short task-based section
in an existing document is a low-cost candidate within the selected approach.
Then use a fresh proposal-preflight Scout before proposing anything. The later
runbook name `poc-verification-command-guide` was not created. These future choices
do not gate intake and were not fabricated as answered.

## Identities, timing and actual interventions

| Field | Observed value |
| --- | --- |
| Task / branch | `poc-scenario1-fuzzy-intake` / `fm/poc-scenario1-fuzzy-intake` |
| Herdr session / workspace | `odot-poc` / `w4` |
| Herdr tab / pane / endpoint | `w4:t2` / `w4:p2` / `odot-poc:w4:p2` |
| Treehouse worktree | `/home/bart/.treehouse/odot-game-ab1b67/1/odot-game` |
| Inspection base and initial HEAD/main | `eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7` |
| Initial idea head | `79758fd757d2850ed93704e636c27ddfd9b3470a` |
| Human-shaped idea head | `1f47c9a4a17a795ddeec968a8e3653813df41099` |
| Final receipt head | Follows the idea head on this branch; exact final hash is in the native report. Resolve with `git rev-parse fm/poc-scenario1-fuzzy-intake`. |
| Git report | `planning/evidence/setup-agent-workflow-poc/scenario-1-fuzzy-intake.md` |
| Native standalone report | `/home/bart/.local/share/firstmate/odot/data/poc-scenario1-fuzzy-intake/report.md` |

Environment exposed session/workspace/tab/pane; inbox `001.msg` supplied endpoint.
`pwd -P` and `git rev-parse --show-toplevel` proved isolation before branching.
No Herdr lifecycle audit or commands occurred.

Observed setup: branch reflog 09:30:45Z to baseline 09:30:55Z, **10s**;
isolation/branch execution approximately 0.2s. Pre-dispatch setup was not observable.
Validation/inventory checkpoint 09:42:47Z: **722s (12m02s)** elapsed. At 09:46:29Z,
**944s (15m44s)** had elapsed; final delivery duration is in the native report.
Investigation and receipt preparation were **slower than expected**, as FirstMate
explicitly observed when requesting wrap-up. This is a supervision/cost finding,
not a claim of efficient intake.

Three durable supervisor interventions, acknowledged into `handled/`:
`001.msg` at 09:30:52Z supplied identities; `002.msg` at 09:37:13Z relayed the
human choice and intake boundary; `003.msg` at 09:42:49Z said investigation was
sufficient and requested concise completion. **One human shaping intervention**;
automated reminders are not additional human product decisions. Raw non-secret
references: those files under
`/home/bart/.local/share/firstmate/odot/state/poc-scenario1-fuzzy-intake.inbox/handled/`
and the task's native `.status` file. No whole conversations were copied.

## Validation, unchanged production and retained owners

Read-only inspection used `rg`, `cat` and targeted `sed`. This worktree had no
DevRunner bin directory. The registered clone had a prebuilt DLL; inspected
planning dispatch reads the current worktree before engine/evidence creation.
Locked `dotnet --version`: `10.0.401`.

```text
DLL: /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll
SHA256: c3d72c52cc31073baaa583ab1e8bf31f71479f05e6b947662ce73cfe2d380718
Command: dotnet <that-DLL> planning-validate --json
Before and after capture: exit 0
{"valid":true,"change":null,"diagnostics":[],"exclusions":[]}
```

Template heading comparison and Git whitespace checks passed. This is consistency
with a recorded prebuilt input, not fresh compilation. No game/unit tests, CI,
Godot, exports, build, restore, installs, remote/browser operations or delegates
ran. No mise planning wrappers ran because they build/restore; selection was
outside intake. OpenSpec strict validation was not run: no OpenSpec edits or
worktree-local built installation. These omissions are not game passes.

```sh
git diff --exit-code eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7 -- . ':(exclude)planning'
set -o pipefail
git ls-tree -r HEAD | rg -v $'\tplanning/' | sha256sum
```

Production diff: **empty, exit 0**. Before/after tracked-entry SHA-256 outside
`planning/`: `041872afb79ef8df0426c5f7d8aaf1273d2aef6ef4174af57856d1008f4903ec`.
This includes source/tests/runner, root docs/config and OpenSpec. An unsupported
`ls-tree` exclusion attempt produced an empty-stream hash that was discarded;
the corrected command above succeeded. A receipt-rewrite patch was also rejected
before editing anything; the file rewrite was corrected. Neither touched production.

Native completion inventory used explicit authorized Odot home/state/data overrides:
`FM_HOME=/home/bart/.local/share/firstmate/odot`,
`FM_STATE_OVERRIDE=/home/bart/.local/share/firstmate/odot/state`,
`FM_DATA_OVERRIDE=/home/bart/.local/share/firstmate/odot/data` and
`/home/bart/projects/personal/firstmate/bin/fm-captain-hold.sh complete poc-scenario1-fuzzy-intake --none`.
Exit 0: `complete: poc-scenario1-fuzzy-intake captain-call inventory reviewed`.
No held decision gates intake; future shaping questions remain explicit.

Only the allowed idea and this report are delivered. The native report records
final committed head, clean state and fast-forward ancestry. Branch, worktree,
reports and Herdr owners remain for FirstMate's separate review/landing and native
cleanup. No game peers, displays, runtime data or background jobs were created;
no worktree return/removal or Herdr shutdown occurred. Unrelated production,
preferences, endpoints, workers and sessions remain untouched.
