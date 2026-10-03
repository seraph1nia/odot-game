STATUS: IMPLEMENTATION_COMPLETE — committed implementation checkpoint; awaiting independent review.
CHANGE: poc-verification-command-guide
SUMMARY: Added a compact six-row Choose a check guide at the start of README's Verification and exports section. Preserved preparation, partial coverage, baseline reuse, substantial-task/full CI gates, user-managed prerequisites and software-rendering/audio limits. All four tasks are complete.

Task: poc-verification-command-guide-ship-r2
Session: 01a10153-b16e-7691-aef9-c25f53c30e4a
Worktree: /home/bart/.treehouse/odot-game-ab1b67/2/odot-game
Branch: fm/poc-verification-command-guide-ship-r2
Exact base: bdf6dc4c2882fc557fa3074196ec9c7a0aee391f
Exact implementation head: e4c8379c016d13f9e8512d22d4e8dbe8ab94a214
This Git handoff is committed in an evidence-only successor to that implementation head; the external task report records the exact final delivery head. Review the full base-to-delivery diff. No production content changes occur in the evidence successor.

Approved planning-inputs identity: head bdf6dc4c2882fc557fa3074196ec9c7a0aee391f; spec_digest 7ec147b34b7293c7cd25ddaa7419ee0e586d2a7cbf1efe43c6c285479bbbb9c2; code_digest fd3171dff771350eb01bdbbb3cb512ee12bd33e252430fc5409c85afbc02eb70.
Implementation planning-inputs identity: head e4c8379c016d13f9e8512d22d4e8dbe8ab94a214; spec_digest 7ec147b34b7293c7cd25ddaa7419ee0e586d2a7cbf1efe43c6c285479bbbb9c2; code_digest ff1975e4036bf5d6f8d4da869589db05c8a685b794fc55137231998cbbd89e03. Canonical inputs remain unchanged and are retained in the ignored implementation-inputs.json log.
Raw PNG proof before editing: worktree sha256sum and git show HEAD:docs/images/export-transfer.png | sha256sum both equaled fffece35d8fcd3a95894f1d49868da079bc0d694d75cf714a5bf947346a96d3f. Starting Git status was clean; physical path and Git toplevel both identified slot 2, outside the registered primary checkout.

FILES_CHANGED:
- README.md: guide, eight guidance links and a command-details subheading.
- openspec/changes/poc-verification-command-guide/tasks.md: four completed checkbox markers only.
- planning/evidence/poc-verification-command-guide/implementation-r2.md: this handoff.

TESTS_RUN: No gameplay, unit, network/UI suites, Godot, CI, restore/build, exports, benchmarks or installs. Documentation/planning-only consistency checks were used as authorized.
VALIDATION_RESULTS:
- openspec list --json; openspec status --change poc-verification-command-guide --json; openspec instructions apply --change poc-verification-command-guide --json: exit 0; local root, spec-driven schema, ready state, initially 0/4 tasks; all returned context files read.
- dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-inputs poc-verification-command-guide: exit 0 before editing, exact approved identity above; exit 0 at implementation head, identity above.
- openspec validate poc-verification-command-guide --strict: exit 0 before and after task bookkeeping; valid, skip_specs accepted with zero deltas.
- dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-validate --json: exit 0 before and after task bookkeeping; valid:true, diagnostics:[], exclusions:[]. Used the already-built permitted entry point rather than mise's restore/build wrapper.
- Direct Python read-only link/content assertions: exit 0; all eight new local paths/anchors resolve, six task rows and seven expected command examples present, changed-file allowlist passes.
- Read-only comparison against mise.toml, Program.cs, Runner.cs, Options.cs, Scenarios.cs, PrivateDisplay.cs, Exports.cs and relevant selector assertions: examples match dispatch/options, locked preparation, registered selectors, missing-package gate and full source/export sequencing. AGENTS, README, planning conventions, ADR 0001 and applicable canonical linux-test-execution guidance read. Existing completed-delta/canonical concurrency drift remains separate.
- git diff --check: exit 0. Base-to-implementation diff contains README and task markers only; production diff outside README is empty. Final delivery scope is checked again after the handoff commit.

Elapsed: 170 seconds from the first recorded setup timestamp (epoch 1791023658) to handoff preparation; final elapsed recorded in external report.
Non-secret logs: logs/poc-verification-command-guide-ship-r2/openspec-validation.log, planning-validation.json, implementation-inputs.json and implementation.diff (ignored, local to this worktree).
OPEN_QUESTIONS: None gating the implementation handoff. Independent review and later landing approval remain separate steps.
SPEC_DEVIATIONS: None. No canonical sync for speed-up-verification, archive, roadmap/lifecycle-state edit or other change implementation.
KNOWN_RISKS: Command examples were verified by source inspection, not runtime execution. No quantified usability result or native GPU/input/audio acceptance is claimed. Historical admission receipts name an earlier planning head; this launch brief supplied the authoritative current base, whose digests were verified. Prior stopped attempts are not implementation evidence.
NEXT_ACTION: FirstMate arranges a fresh read-only Reviewer for the exact final delivery diff and current inputs. This Shipper handoff is not independent PASS, archive, sync or landing. Retain the local unmerged branch until separately approved.
