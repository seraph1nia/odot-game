STATUS: LOCAL_BOOTSTRAP_IMPLEMENTED; WHOLE_CHANGE_INCOMPLETE
CHANGE: setup-agent-workflow-poc
SUMMARY: Durable project planning, deterministic C# YAML validator/selector, approval/review receipts, five reusable skills, authority ADR, native runtime templates/thin managed-pane launcher and five scenario/fault procedures are implemented. No live runtime acceptance, self-review PASS or whole-change completion is claimed.
FILES_CHANGED: AGENTS.md; README.md; mise.toml; .agents/skills/{project-intake,proposal-preflight,roadmap-analysis,implement-openspec-change,review-openspec-change}/SKILL.md; planning/; docs/adr/0001-agent-workflow-authority.md; docs/agent-workflow.md; docs/agent-workflow-scenarios.md; tools/AgentWorkflow/; tools/DevRunner/Planning/; tools/DevRunner/{Program.cs,Exports.cs,DevRunner.csproj,packages.lock.json}; tests/DevRunner.Tests/{PlanningFixture.cs,PlanningTests.cs,AgentLauncherTests.cs,packages.lock.json}; bootstrap OpenSpec task markers and primary-reconciled factual snapshot annotations.
TESTS_RUN: one bounded local checkpoint repair; fresh review2 PASS by static inspection only; locked solution restore; dotnet format Odot.slnx --no-restore and --verify-no-changes; strict build; 40 focused planning/launcher regressions followed by additional path-symlink regression in full cheap suite; mise run test; planning-validate/next JSON commands; bash syntax checks; defect-patch dry-run/source-checkout refusal; strict OpenSpec validation; full mise run ci baseline and after attempt.
VALIDATION_RESULTS: Baseline fullCI PASS at clean base4a70e16 (249.42s runner). Full cheap PASS402 gameplay +185runner, rules29.97s, evidence logs/20261003-073827-88a5d48f. Planning/OpenSpec/syntax/format/build gates PASS. Initial after fullCI FAILED182.01s runner/185.62s command, reconnect unexpected137, with later coverage unexecuted (logs/20261003-074006-dc678c13). Affected reconnect PASS65.66s without edits (logs/20261003-074536-edafdaf1). Full retry mise run ci --ui-jobs 1 PASS427.70s runner/429.34s command at df906227f336924c4bdeb12a6cabc44e814ab230, full required coverage (logs/20261003-074655-41250255). One subsequent small reviewer repair reran changed cheap/static/lifecycle/launcher checks:402 gameplay+187runner PASS27.82s (logs/20261003-075729-c82bbe54),43 focused tests PASS after final native-helper fixture/mode assertion, strict build/format/OpenSpec/JSON/syntax/patch checks PASS. Unchanged Game/Core/network/UI/export/package checks are reused from that complete full run per repository policy; no claim fullCI executed on repaired head. Graphical cap1 versus baseline2 is recorded without a performance comparison.
OPEN_QUESTIONS: User-installed Pi Herdr integrationv9 availability now confirmed; actual managed primary/trust/watch behavior and all live acceptance remain unexecuted. Universal auxiliary gaps remain reported but supported manual/static-Codex/local-only route does not exercise them. Runtime product decisions and trial implementation/landing approvals must come from the user. The initial exit137 cause remains unproven; selected/full reruns passed without game/test semantic changes or unrelated process kills.
SPEC_DEVIATIONS: None in production behavior. Primary reconciled stale factual task1.2/design snapshot against actual43/43 optimization completion, retaining earlier planning history and same apply authorization. Bootstrap implementation via local isolated Codex worker is not evidence of live FirstMate acceptance.
KNOWN_RISKS: Read-only worker policy is instruction/diff enforcement, not sandbox containment. Digests/receipts check consistency, not cryptographic user/agent identity. Native spawn with origin resets to upstream default, so selected owned checkpoint clone intentionally uses odot-source provenance/no origin. Completed receipts validate historical reviewed/landed inputs; current verified admission rejects stale inputs. Canonical sync hash changes need independent evidence reconciliation/review rather than silently rewriting receipts. Failed-CI resource/signal cause is unproven; system memory pressure was observed, no engine stack/error explains137.
NEXT_ACTION: Fresh independent local checkpoint review2 PASS is retained separately; Shipper /root/bootstrap_shipper is stopped and roadmap is blocked awaiting live acceptance. Prepare only selected empty FM_HOME/clean checkpoint clone. No further tests/builds/runtime checks under latest user instruction. Start primary later inside real owned Herdr pane with user-managed trust; live scenarios, whole-change acceptance, merge/archive and Atomic remain gated.

# Identity and scope

Worker: /root/bootstrap_shipper, stopped after handoff; single local bootstrap Codex Shipper delegated by primary; not a native FirstMate task/session.
Owned branch/worktree: work/agent-workflow-bootstrap, .cache/agent-workflow-bootstrap.
Base: 4a70e16a152beee112a775b1f11f08da4ebc2277.
Exact checkpoint head: report from Git when this artifact is committed; the parent receives the immutable commit identity separately and retains it in checkpoint review/setup evidence.
Repair count:1. Fresh local checkpoint review1 returned FAIL_IMPLEMENTATION (B1); complete standalone report retained as checkpoint-review-1.md. One repair prevents unreviewed pre-landing production changes from clearing dependencies while preserving valid post-landing history; native exact socket/pane/tab/workspace proof now reused. Fresh static-only review2 PASS at 501021722a7e03fcde0cf9675db723f08a780707 is retained as checkpoint-review-2.md. Neither local checkpoint review is live POC acceptance.
Progress:18/26 OpenSpec tasks complete. Task4.4 checkpoint bookkeeping,4.2/4.3 native setup/Scout smoke and5.2 real fuzzy intake are complete. Scenario2 proposal is running; implementation/review, repair, ambiguity, full restart/final/Atomic acceptance remain open. See native-launch.md, native-completion.md, scenario-1-fuzzy-intake.md and restart-intake.md for actual execution evidence.
The original checkout, unrelated Windows CI worktree, preferences, credentials and sessions remain preserved. During the bootstrap coding checkpoint no tools/packages were installed; the only deliberate dependency addition was locked YamlDotNet16.3.0 in DevRunner and its transitive runner-test lock. Later the user explicitly authorized tasks-axi0.2.6 installation for native completion, documented in native-completion.md.

# Live continuation after the original worker handoff

The original fields above describe the stopped local bootstrap Shipper's results,
not the subsequent Pi execution. The selected Pi/Herdr/Codex/Treehouse stack is
now live. A native Scout smoke completed and cleaned up; real intake task
poc-scenario1-fuzzy-intake delivered Git head fe42f72766ec839ecead121f87d63c0123532e1d
after1208s, remaining unlanded/retained. The user selected Documentation guide.
A fresh Pi conversation recovered its live worker without duplication and repeated
deterministic next-work selection. No trial implementation approval, production
review/repair or main landing exists yet. The two captured intake files were
replayed onto this owned bootstrap feature branch; neither project main changed.

Follow-up launcher correction requires tasks-axi at startup and removes the
incorrect optional-route warning for it; the existing fake-bin fixture and setup
instructions were kept coherent. Shell syntax, whitespace and prebuilt planning
validation passed. Tests/builds were not rerun under the user's no-tests request;
the earlier checkpoint PASS is historical, not an independent review of these
follow-up edits. Native Codex state classification still has a codex-unverified
gap, and one presentation restart-binding warning occurred; actual Herdr/status/
report evidence remains available. Full live acceptance and fresh final review
remain required. Continue scenario2; implementation and main landing each await
their explicit user authorization once the concrete work is reviewable.
