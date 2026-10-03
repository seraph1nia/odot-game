---
change: poc-verification-command-guide
result: PASS
reviewer_task: poc-verification-command-guide-review1
shipper_task: poc-verification-command-guide-ship-r2
base: bdf6dc4c2882fc557fa3074196ec9c7a0aee391f
head: fe51da101d003479eaad3796a5c7b56ffb9e0ffd
spec_digest: 7ec147b34b7293c7cd25ddaa7419ee0e586d2a7cbf1efe43c6c285479bbbb9c2
code_digest: ff1975e4036bf5d6f8d4da869589db05c8a685b794fc55137231998cbbd89e03
canonical_inputs:
  openspec/specs/background-music/spec.md: d89f977b9cb41384c96a675009a1fb20544b02c108974e8fa251a018641b1f2a
  openspec/specs/city-tabletop/spec.md: a21410ee858b9b31c773c939e1cbad1d8fbc7d4d89fe2d7fd622f002b494c5d6
  openspec/specs/client-settings/spec.md: 674e52ef413b746502e472a543e5638e035a666223ed42c3c5ebb54c4122f3af
  openspec/specs/client-updates/spec.md: 6d5e9f6a1cecd17eb1536539893299116c10fe110af05473e81a96f5c9560793
  openspec/specs/combat-archetypes/spec.md: 447f188654a865d7deebceea2e4b21b2299b7dad4fa162d380c298a8b71a729d
  openspec/specs/coop-city-match/spec.md: 791b0988b4713a95fa72262540833df8d1a642190d5ccbbcee154bb87a3104c9
  openspec/specs/coop-verification/spec.md: f12e88c676363eb79d925a260ce626aa8b214e3769df68e68f23610626f3c058
  openspec/specs/desktop-installation/spec.md: 0f969e51b2d05a1913cbb04536c7970f62749a2cd46de150822aa35cdc1375e5
  openspec/specs/ecs-unit-combat/spec.md: 0c0ecb66598fae761d6bc0fc7c423b7b8016998229c8c692cea8e547f38ec1e0
  openspec/specs/game-feedback/spec.md: 31fba8bae3cba5fd0c19fa4fc6d098fe9e0f868f38dda4749658f5e99408e286
  openspec/specs/game-launcher/spec.md: 24ad22c7f503016ec0b0fac1f91b1c68b21c169375d7c4ae7a2a127cecde5ff0
  openspec/specs/linux-test-execution/spec.md: 0d2c7e598da92b5f43577d0685ebedba147088b0e6a6696b656ff0ee20bc4b98
  openspec/specs/resumable-multiplayer/spec.md: 6125cdacfff24ab1b220eedbc5368280493400f8a13e9d452612fba8da0ad797
  openspec/specs/steam-sessions/spec.md: d146e6d8d708b9089a62e4d77b0bdbc891436694813d391c5afe23b4f2695bd2
  openspec/specs/themed-ui/spec.md: 879da092ef66b80c431e0e9af2ed2e196cb0abb06968f616569a7d62a423572b
  openspec/specs/versioned-distribution/spec.md: d678fb71a17b06be01e0a10db2db4a19abb4cab362e02e95444bde7c4040ec21
attempt: 1
validation_passed: true
decisions_resolved: true
---

# Independent review receipt

Captured by planning-only task `poc-verification-command-guide-review-capture`
from the standalone Reviewer report at
`/home/bart/.local/share/firstmate/odot/data/poc-verification-command-guide-review1/report.md`.
This is a durable capture of the independent review, not a new review or implementation.
Approval is retained in [approval.md](approval.md), with canonical admission identities
in [admission.md](admission.md) and the distinct Shipper handoff in
[implementation-r2.md](implementation-r2.md). The approved spec digest and exact
reviewed production inputs are bound above. Historical admission used planning
head `ca5bdd9f4e538d1457f86b2d35caadb5bf9ccefc`; the authoritative review base is
`bdf6dc4c2882fc557fa3074196ec9c7a0aee391f`, with the same approved spec digest.
No repair was required. No archive, sync, merge or landing was performed.
Implementation landing still requires separate explicit user approval.

The following is the retained standalone independent Reviewer report. Its
commands and results were executed by the Reviewer, not by this capture worker.

RESULT: PASS
BLOCKING_FINDINGS:
None.

NON_BLOCKING_FINDINGS:
None introduced by this change. Canonical linux-test-execution still describes serial source UI/default desktop behavior that current source and AGENTS have superseded; this pre-existing reconciliation is explicitly excluded by the approved design. The new guide does not restate obsolete concurrency or desktop behavior. No runtime or quantified usability claim was made or validated.

REQUIREMENT_EVIDENCE:
Review identity:
- Reviewer task: poc-verification-command-guide-review1; session: 01a10159-a780-7880-bea4-b8699d94bba9; attempt: 1.
- Fresh Reviewer worktree: /home/bart/.treehouse/odot-game-ab1b67/3/odot-game. git worktree list --porcelain proves this is separate from registered primary /home/bart/.local/share/firstmate/odot/projects/odot-game and distinct Shipper slot 2; poisoned slot 1 was not used.
- Shipper task: poc-verification-command-guide-ship-r2; standalone report read, transcript not read.
- Exact base: bdf6dc4c2882fc557fa3074196ec9c7a0aee391f; exact reviewed delivery head: fe51da101d003479eaad3796a5c7b56ffb9e0ffd.
- git switch --detach fe51da101d003479eaad3796a5c7b56ffb9e0ffd: exit 0; git rev-parse HEAD matched; git rev-parse bdf6dc4c2882fc557fa3074196ec9c7a0aee391f succeeded. No main substitution.
- Approval/admission and implementation-r2.md read. Approval quotes “Approve both” for the README-only change with separate landing approval. Historical admission planning head differs from the launch base, as disclosed; immutable approved spec identity matches current review inputs.
- Approved base spec_digest: 7ec147b34b7293c7cd25ddaa7419ee0e586d2a7cbf1efe43c6c285479bbbb9c2; code_digest: fd3171dff771350eb01bdbbb3cb512ee12bd33e252430fc5409c85afbc02eb70.
- Reviewed head spec_digest: 7ec147b34b7293c7cd25ddaa7419ee0e586d2a7cbf1efe43c6c285479bbbb9c2; code_digest: ff1975e4036bf5d6f8d4da869589db05c8a685b794fc55137231998cbbd89e03. Fresh planning-inputs output matches launch and final Shipper identities.

Requirements and task completion:
- Task 1.1: README.md:115 places “Choose a check” first under Verification and exports; README.md:121-126 has exactly six approved task rows with commands, preparation, relative cost and coverage. Read-only command/source verification:
  * Cheap test: mise.toml:52-54 locks restores and builds both gameplay and runner suites, then dispatches test-rules. No Godot setup is invoked.
  * Networking: mise.toml:117-121 forwards arguments; Runner.cs:38 calls Prepare before NetworkTests; Runner.cs:148-160 locks solution restore/build/import. Scenarios.cs:11 and Options.cs:85-88 recognize redistribution; NetworkTests.cs:85-110 selects only it, and :209-240 checks live enemy transfer identity/damage/recovery on real peers.
  * Source UI: mise.toml:123-127 forwards arguments; Runner.cs:40-43 checks private-display prerequisites and prepares source. Scenarios.cs:12/Options.cs:85-88 recognize reconnect; PrivateDisplay.cs:87-96 selects only that slice; UiTests.cs:466-507 checks actual Reconnect input, identity and reconstructed presentation.
  * Exported UI: Runner.cs:42 bypasses Prepare for exported-package; PrivateDisplay.cs:80-85 requires existing client and server executables and explicitly never exports. PrivateDisplay.cs:89-96 selects the package slice; UiTests.cs:242 records packed-resource/input coverage. The mise runner dependency still builds DevRunner, consistent with the guide's specific claim about source/package preparation.
  * Full CI: mise.toml:162-166 dispatches ci; Exports.cs:129-156 gates exports on all source partitions; :173-185 exports client then server sequentially and checks headless/graphical packages.
  * Documentation/planning: mise.toml:228-232 depends on runner (locked restore/build at :48-50). Program.cs:11-12 runs planning directly without constructing game Runner. planning/README.md:43-53 explains the ordinary wrapper and no Godot.
- Task 1.2: README.md:117 links policy, :126 planning, :128 selector/evidence detail, :130 setup/profiles/native packages/Steam. All eight added local paths/anchors independently resolved with read-only Python heading checks. README.md:128 preserves partial coverage, before/after substantial-task gates, unchanged baseline reuse, normal CI coverage and repeat conditions. :130 preserves user-managed prerequisites through Setup, unexecuted versus failed, and software graphics/audio limitations. AGENTS.md:17-34, README setup/reference, docs/verification.md:1607 onward and canonical cost-based guidance agree with these claims. No timing promises or scope-expanding routes are added.
- Task 2.1: Applicable documentation/planning validation passes (commands below). No build, restore, gameplay/unit/network/UI/CI/export/install/benchmark command executed during review. Documentation-only scope does not require those runs.
- Task 2.2: all four tasks at openspec/changes/poc-verification-command-guide/tasks.md:8,9,13,14 are checked and justified by content, validation and exact delivery receipt. Base..head contains exactly README.md (19 added lines), tasks.md (four checkbox toggles), and planning/evidence/poc-verification-command-guide/implementation-r2.md (37-line handoff). No game, runner, tests, AGENTS, canonical spec, configuration/lock, other proposal, unrelated doc, roadmap marker, sync/archive or landing edit occurs. Evidence-only successor is explicitly identified by the handoff and external final delivery receipt.

Commands/results:
- openspec validate poc-verification-command-guide --strict: exit 0, valid; skip_specs accepts zero deltas.
- dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-validate --json: exit 0; valid:true, diagnostics:[], exclusions:[].
- dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-inputs poc-verification-command-guide: exit 0; exact head/spec/code identities above, canonical hashes below.
- Read-only Git base..head diff/stat/name allowlist, direct Python eight-link/six-row/four-task checks: exit 0.
- git diff --check bdf6dc4c2882fc557fa3074196ec9c7a0aee391f HEAD: exit 0.

Read-only proof:
- Before review: initial and detached-head git status --porcelain=v1 empty. Worktree PNG sha256sum and git show HEAD:docs/images/export-transfer.png | sha256sum both match fffece35d8fcd3a95894f1d49868da079bc0d694d75cf714a5bf947346a96d3f before content review.
- After validation/inspection: all 864 tracked worktree files independently checked with git hash-object against git rev-parse HEAD:path; every committed blob matches. Final git status --porcelain=v1 and git diff HEAD -- empty, exact HEAD unchanged; PNG still matches. No project files written, no repairs or source mutations. Only external authorized report/status/lifecycle output written.
- Steering inbox absent at setup; no pending messages to acknowledge.
- Elapsed to report: 166 seconds from setup epoch 1791024030 (plus initial skill/setup reading before timestamp).

Canonical planning-input identities (all match approval admission table and unchanged base files):
- openspec/specs/background-music/spec.md: d89f977b9cb41384c96a675009a1fb20544b02c108974e8fa251a018641b1f2a
- openspec/specs/city-tabletop/spec.md: a21410ee858b9b31c773c939e1cbad1d8fbc7d4d89fe2d7fd622f002b494c5d6
- openspec/specs/client-settings/spec.md: 674e52ef413b746502e472a543e5638e035a666223ed42c3c5ebb54c4122f3af
- openspec/specs/client-updates/spec.md: 6d5e9f6a1cecd17eb1536539893299116c10fe110af05473e81a96f5c9560793
- openspec/specs/combat-archetypes/spec.md: 447f188654a865d7deebceea2e4b21b2299b7dad4fa162d380c298a8b71a729d
- openspec/specs/coop-city-match/spec.md: 791b0988b4713a95fa72262540833df8d1a642190d5ccbbcee154bb87a3104c9
- openspec/specs/coop-verification/spec.md: f12e88c676363eb79d925a260ce626aa8b214e3769df68e68f23610626f3c058
- openspec/specs/desktop-installation/spec.md: 0f969e51b2d05a1913cbb04536c7970f62749a2cd46de150822aa35cdc1375e5
- openspec/specs/ecs-unit-combat/spec.md: 0c0ecb66598fae761d6bc0fc7c423b7b8016998229c8c692cea8e547f38ec1e0
- openspec/specs/game-feedback/spec.md: 31fba8bae3cba5fd0c19fa4fc6d098fe9e0f868f38dda4749658f5e99408e286
- openspec/specs/game-launcher/spec.md: 24ad22c7f503016ec0b0fac1f91b1c68b21c169375d7c4ae7a2a127cecde5ff0
- openspec/specs/linux-test-execution/spec.md: 0d2c7e598da92b5f43577d0685ebedba147088b0e6a6696b656ff0ee20bc4b98
- openspec/specs/resumable-multiplayer/spec.md: 6125cdacfff24ab1b220eedbc5368280493400f8a13e9d452612fba8da0ad797
- openspec/specs/steam-sessions/spec.md: d146e6d8d708b9089a62e4d77b0bdbc891436694813d391c5afe23b4f2695bd2
- openspec/specs/themed-ui/spec.md: 879da092ef66b80c431e0e9af2ed2e196cb0abb06968f616569a7d62a423572b
- openspec/specs/versioned-distribution/spec.md: d678fb71a17b06be01e0a10db2db4a19abb4cab362e02e95444bde7c4040ec21

SPEC_DEVIATIONS:
None. Approved proposal/design/tasks and skip_specs metadata are preserved; only authorized checkbox bookkeeping changes their files. No unresolved product/architecture decision gates the report.

RECOMMENDED_NEXT_ACTION:
FirstMate may capture this revision-bound independent PASS and prepare the gated lifecycle steps. Keep the implementation unmerged until separate explicit landing approval. Any material source/spec change invalidates this PASS. No repair is required.

Completion gate: FM_HOME=/home/bart/.local/share/firstmate/odot FM_STATE_OVERRIDE=/home/bart/.local/share/firstmate/odot/state FM_DATA_OVERRIDE=/home/bart/.local/share/firstmate/odot/data /home/bart/projects/personal/firstmate/bin/fm-captain-hold.sh complete poc-verification-command-guide-review1 --none: exit 0; captain-call inventory reviewed, no unresolved decisions. Final checkpoint epoch 1791024203; elapsed 173 seconds from recorded setup. Final source status remains clean; inbox absent.
