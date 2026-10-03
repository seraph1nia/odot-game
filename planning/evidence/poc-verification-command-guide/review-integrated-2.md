---
change: poc-verification-command-guide
result: PASS
reviewer_task: poc-verification-command-guide-integrated-review2
shipper_task: poc-verification-command-guide-ship-r2
base: 6a7a179501de631c4ec3f2e47b314ae0897436bc
head: 196c3e91c8f97bd32e4a97734920aedee81cf31d
spec_digest: 7ec147b34b7293c7cd25ddaa7419ee0e586d2a7cbf1efe43c6c285479bbbb9c2
code_digest: 8737b47eb2e6a798fdcc091ee0786a54e22a4cac1ff63af9c06e0b4ebbcb045b
canonical_inputs:
  openspec/specs/background-music/spec.md: d89f977b9cb41384c96a675009a1fb20544b02c108974e8fa251a018641b1f2a
  openspec/specs/city-research/spec.md: 20d764abbcb906f7e19b83374e442aec9f79ddb0471141aef37c30d05288d9c0
  openspec/specs/city-tabletop/spec.md: 5db83666994c9039e73270d17c145dabda10366d50d6cbef0acdfa358c2fb25a
  openspec/specs/client-settings/spec.md: 674e52ef413b746502e472a543e5638e035a666223ed42c3c5ebb54c4122f3af
  openspec/specs/client-updates/spec.md: 6d5e9f6a1cecd17eb1536539893299116c10fe110af05473e81a96f5c9560793
  openspec/specs/combat-archetypes/spec.md: e0d5b79b3f024e4cc1652587136b9c19cca210685f5184d0c2d734a9638eabca
  openspec/specs/combat-status-effects/spec.md: 0224822d5d934281f4c4721dda78eb766350c7a8e3a15984d586ad7aa5a50ced
  openspec/specs/coop-city-match/spec.md: 30771a00464f90a794b4c89bab6f2259a70f53b8e8e58efef458d824c0fb0d85
  openspec/specs/coop-verification/spec.md: f12e88c676363eb79d925a260ce626aa8b214e3769df68e68f23610626f3c058
  openspec/specs/desktop-installation/spec.md: 0f969e51b2d05a1913cbb04536c7970f62749a2cd46de150822aa35cdc1375e5
  openspec/specs/ecs-unit-combat/spec.md: bdb67876daf23b8925131d1f212255facd30b580c753a6b88ef7897a9ce5e1e9
  openspec/specs/game-feedback/spec.md: 31fba8bae3cba5fd0c19fa4fc6d098fe9e0f868f38dda4749658f5e99408e286
  openspec/specs/game-launcher/spec.md: 24ad22c7f503016ec0b0fac1f91b1c68b21c169375d7c4ae7a2a127cecde5ff0
  openspec/specs/linux-test-execution/spec.md: 0d2c7e598da92b5f43577d0685ebedba147088b0e6a6696b656ff0ee20bc4b98
  openspec/specs/resumable-multiplayer/spec.md: 8da875dc9aef166c9c501ac5aafc0b09829722d6b0c19edfc3c5a6b8f7c7c0e4
  openspec/specs/steam-sessions/spec.md: d146e6d8d708b9089a62e4d77b0bdbc891436694813d391c5afe23b4f2695bd2
  openspec/specs/themed-ui/spec.md: 879da092ef66b80c431e0e9af2ed2e196cb0abb06968f616569a7d62a423572b
  openspec/specs/versioned-distribution/spec.md: d678fb71a17b06be01e0a10db2db4a19abb4cab362e02e95444bde7c4040ec21
attempt: 2
validation_passed: true
decisions_resolved: true
landed_revision: 4c09a21c0e134b0cb5d744664820ac983644fb52
---

# Fresh integrated review receipt

This preserves the second independent Codex Reviewer PASS for the exact repaired integration head. The original native PASS remains in review-1.md; integration-review-fail.md preserves the intervening stale-receipt failure. No production file changed after this reviewed head.

RESULT: PASS
BLOCKING_FINDINGS:
None.

NON_BLOCKING_FINDINGS:
Canonical linux-test-execution still contains pre-existing serial source-UI and old desktop-dev descriptions. The approved design leaves speed-up-verification canonical reconciliation separate; this guide does not restate those details. This review grants no whole-POC verification or runtime acceptance.

REQUIREMENT_EVIDENCE:
Identity and immutable review setup:
- Fresh independent Reviewer task: poc-verification-command-guide-integrated-review2; attempt 2. Session: current newly dispatched Codex session; harness UUID was not supplied. No Shipper conversation or prior PASS was reused.
- Distinct original Shipper: poc-verification-command-guide-ship-r2, session 01a10153-b16e-7691-aef9-c25f53c30e4a, slot 2, as recorded in implementation-r2.md.
- Reviewer worktree: /home/bart/.treehouse/odot-game-ab1b67/6/odot-game. pwd, clean git status and git worktree list --porcelain establish own detached slot, separate from registered clone /home/bart/.local/share/firstmate/odot/projects/odot-game and other worker/developer checkouts. Shared git common directory is expected; no other checkout was switched or written.
- Exact base: 6a7a179501de631c4ec3f2e47b314ae0897436bc.
- Exact reviewed head: 196c3e91c8f97bd32e4a97734920aedee81cf31d.
- git cat-file -t confirmed candidate already local, so no fetch was needed. git switch --detach succeeded; git rev-parse HEAD matched exact candidate, base resolved, and git merge-base BASE HEAD equalled BASE. No substitution of moving main.
- Raw docs/images/export-transfer.png SHA-256 before reviewing and git show HEAD:docs/images/export-transfer.png | sha256sum both equal fffece35d8fcd3a95894f1d49868da079bc0d694d75cf714a5bf947346a96d3f.
- Read reviewer and captain-hold skills, approved proposal/design/tasks/metadata, approval/admission/handoff, prior integrated failure and historical receipt; current relevant canonical requirements, README, AGENTS, planning conventions, ADR 0001, source dispatch/preparation/selectors/export gates and full relevant diff/history.

Current planning-inputs identities (permitted prebuilt runner from candidate cwd, exit 0):
head: 196c3e91c8f97bd32e4a97734920aedee81cf31d
spec_digest: 7ec147b34b7293c7cd25ddaa7419ee0e586d2a7cbf1efe43c6c285479bbbb9c2
code_digest: 8737b47eb2e6a798fdcc091ee0786a54e22a4cac1ff63af9c06e0b4ebbcb045b
canonical_inputs count: 18
- openspec/specs/background-music/spec.md: d89f977b9cb41384c96a675009a1fb20544b02c108974e8fa251a018641b1f2a
- openspec/specs/city-research/spec.md: 20d764abbcb906f7e19b83374e442aec9f79ddb0471141aef37c30d05288d9c0
- openspec/specs/city-tabletop/spec.md: 5db83666994c9039e73270d17c145dabda10366d50d6cbef0acdfa358c2fb25a
- openspec/specs/client-settings/spec.md: 674e52ef413b746502e472a543e5638e035a666223ed42c3c5ebb54c4122f3af
- openspec/specs/client-updates/spec.md: 6d5e9f6a1cecd17eb1536539893299116c10fe110af05473e81a96f5c9560793
- openspec/specs/combat-archetypes/spec.md: e0d5b79b3f024e4cc1652587136b9c19cca210685f5184d0c2d734a9638eabca
- openspec/specs/combat-status-effects/spec.md: 0224822d5d934281f4c4721dda78eb766350c7a8e3a15984d586ad7aa5a50ced
- openspec/specs/coop-city-match/spec.md: 30771a00464f90a794b4c89bab6f2259a70f53b8e8e58efef458d824c0fb0d85
- openspec/specs/coop-verification/spec.md: f12e88c676363eb79d925a260ce626aa8b214e3769df68e68f23610626f3c058
- openspec/specs/desktop-installation/spec.md: 0f969e51b2d05a1913cbb04536c7970f62749a2cd46de150822aa35cdc1375e5
- openspec/specs/ecs-unit-combat/spec.md: bdb67876daf23b8925131d1f212255facd30b580c753a6b88ef7897a9ce5e1e9
- openspec/specs/game-feedback/spec.md: 31fba8bae3cba5fd0c19fa4fc6d098fe9e0f868f38dda4749658f5e99408e286
- openspec/specs/game-launcher/spec.md: 24ad22c7f503016ec0b0fac1f91b1c68b21c169375d7c4ae7a2a127cecde5ff0
- openspec/specs/linux-test-execution/spec.md: 0d2c7e598da92b5f43577d0685ebedba147088b0e6a6696b656ff0ee20bc4b98
- openspec/specs/resumable-multiplayer/spec.md: 8da875dc9aef166c9c501ac5aafc0b09829722d6b0c19edfc3c5a6b8f7c7c0e4
- openspec/specs/steam-sessions/spec.md: d146e6d8d708b9089a62e4d77b0bdbc891436694813d391c5afe23b4f2695bd2
- openspec/specs/themed-ui/spec.md: 879da092ef66b80c431e0e9af2ed2e196cb0abb06968f616569a7d62a423572b
- openspec/specs/versioned-distribution/spec.md: d678fb71a17b06be01e0a10db2db4a19abb4cab362e02e95444bde7c4040ec21
Independent read-only Python reconstruction of normalized change artifact concatenation and sorted production path/SHA-256 input concatenation reproduced both digests using the algorithm in PlanningEvidence.cs:30-54. All 18 current canonical files independently recomputed to the hashes above; actual glob file count and reported count match. Approval's spec_digest remains current. Old fe51da1 PASS is historical and stale for these production/canonical inputs.

Requirements and task coverage:
- Task 1.1: README.md:117-132 adds Choose a check first under Verification and exports. Exactly six task rows match design.md:26-40; they distinguish preparation, relative cost and selected/full coverage. mise.toml:48-54 provides locked runner/test preparation without Godot. Runner.cs:38-43 prepares standalone network/source UI, bypasses source preparation for exported-package, and checks its existing packages. Scenarios.cs:11-12 registers redistribution/reconnect/exported-package; NetworkTests.cs:99 onward selects redistribution without unrelated cases. UiTests.cs:469-510 checks actual Reconnect input, new transport with retained identity/state and restored rendered observations. PrivateDisplay.cs:80-85 checks both existing dist client/server executables without implicit export. Exports.cs:129-185 gates full local Linux source checks before sequential client/server exports and headless/graphical package checks. No execution-time promise or new expensive check is introduced.
- Task 1.2: Independent link/heading checks resolved all eight new guide links: AGENTS execution policy, planning conventions, command details, verification speed/evidence, Setup, runtime profiles, native distribution checks, and external Steam prerequisites. README.md:119 directs readers to affected selectors; :130 preserves partial/full distinction, before/after substantial-task CI, reusable unchanged source/environment baseline, complete normal CI triggers and justified reruns. :132 preserves user-managed locked prerequisites, unexecuted versus failed outcomes, software graphics/audio limits and separate profile/native/paired-Steam routes. This meets canonical linux-test-execution cost-based guidance and current ownership/coverage policy. Research changes already on developer main are preserved in surrounding README/docs/tooling; the guide makes no claim that reconnect alone covers all research/status behaviors.
- Task 2.1: Strict OpenSpec and planning validation pass on this exact candidate. .openspec.yaml explicitly skips specs; no capability behavior/delta is introduced. Ordinary planning wrapper's locked restore/build dependency at mise.toml:228 onward is described accurately; Program.cs:11-12 routes the permitted direct planning commands without game Runner. Documentation-only scope warrants consistency checks, not game/export validation.
- Task 2.2: All four task markers complete. Full base..head diff is 66 files, 3133 insertions and 20 deletions, including separate workflow bootstrap/planning integration. It is not an overall README-only diff. Guide implementation e4c8379 changes only README (19 added lines) and four task checkboxes; fe51da1 adds only implementation handoff. Additional skills/docs, planning runner/tests/YamlDotNet locks and CI planning gate derive from separate bootstrap/integration history (df90622,5010217,95a3321,daa62e2), not this guide. Base..head has no src, gameplay-unit-test or canonical-spec changes. This focused guide review does not retrospectively certify unfinished workflow live acceptance.
- Repair verification: git diff --name-only b70c0c71958ea6fb16f75461a13599b28675822f HEAD returns exactly planning/evidence/poc-verification-command-guide/integration-review-fail.md and planning/roadmap.yaml. Diff resets guide state verified to in_progress, removes verification binding, and records stale history/review requirement at roadmap.yaml:52-61. Historical review-1.md is unchanged. New failed-report copy is byte-identical to the prior standalone report. No production, change requirement or canonical input changed in repair. Current code_digest matches prior integrated candidate; approved spec_digest is unchanged. planning-next correctly admits no new change while this item is in_progress.

Commands and actual results:
- openspec validate poc-verification-command-guide --strict: exit 0, valid, skip_specs accepted with zero deltas.
- dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-inputs poc-verification-command-guide: exit 0, exact current identities above.
- dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-validate --json: exit 0, valid:true, change:null, diagnostics:[], exclusions:[].
- dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-next --json: exit 0, valid:true, change:null, diagnostics:[]; all eight queue entries excluded, each citing an implementation in_progress and its own state. No additional work selected or dispatched.
- Read-only Git diff/stat/name/history and text checks; independent Python digest/hash/link/row/task/retained-report/committed-blob assertions: exit 0. git diff --check BASE HEAD: exit 0.
- No mise wrapper, unit/game/network/UI test, Godot, CI, build, restore, export, install, sync, archive, merge, push, remote mutation, delegated worker or Herdr lifecycle action was run.

Read-only proof and timing:
- Initial and detached-candidate git status --porcelain=v1 empty. All 892 tracked working-tree blobs independently compared using git hash-object -- FILE versus git rev-parse HEAD:FILE: zero mismatches. HEAD remains exact candidate. No production or planning source was edited. Writes limited to requested external report/status and explicitly required completion attestation.
- Exact steering inbox absent/empty at setup and review checkpoint; no pending messages to acknowledge.
- Setup epoch 1791025692; elapsed through report construction 183 seconds. Fresh direct planning-input reads remained identical; no runtime/environment acceptance is claimed.

SPEC_DEVIATIONS:
None for the approved guide. No unresolved product or architecture decision gates this report. Existing canonical drift is separate, explicitly preserved scope.

RECOMMENDED_NEXT_ACTION:
This is the fresh second-review PASS for integration candidate 196c3e91c8f97bd32e4a97734920aedee81cf31d against exact base 6a7a179501de631c4ec3f2e47b314ae0897436bc, still not a merge/archive. FirstMate may have an authorized planning writer capture this exact receipt/current identities and proceed with applicable lifecycle gates. Preserve both historical PASS and integrated FAIL; do not rewrite their identities. Landing remains subject to separate explicit approval and authorized execution; this Reviewer performs neither landing nor lifecycle preparation. Meaningful later production/spec changes invalidate this PASS. Whole-POC live acceptance remains incomplete.

Completion attestation: FM_HOME=/home/bart/.local/share/firstmate/odot FM_STATE_OVERRIDE=/home/bart/.local/share/firstmate/odot/state FM_DATA_OVERRIDE=/home/bart/.local/share/firstmate/odot/data /home/bart/projects/personal/firstmate/bin/fm-captain-hold.sh complete poc-verification-command-guide-integrated-review2 --none exited 0: complete: poc-verification-command-guide-integrated-review2 captain-call inventory reviewed. Final check confirmed 892 committed blob matches, clean status, exact head unchanged and no pending inbox messages. Total elapsed 206 seconds.
