RESULT: FAIL_IMPLEMENTATION
BLOCKING_FINDINGS:
1. Candidate lifecycle evidence incorrectly remains verified against a stale PASS. `planning/roadmap.yaml:52-61` declares `poc-verification-command-guide` verified and points to `planning/evidence/poc-verification-command-guide/review-1.md:1-30`, which binds head fe51da101d003479eaad3796a5c7b56ffb9e0ffd, code_digest ff1975e4036bf5d6f8d4da869589db05c8a685b794fc55137231998cbbd89e03 and the old canonical input set. Current candidate code_digest is 8737b47eb2e6a798fdcc091ee0786a54e22a4cac1ff63af9c06e0b4ebbcb045b. The permitted planning-validate command exits 1 with `valid:false`, diagnostics `["planning/roadmap.yaml [poc-verification-command-guide]: PASS code_digest is stale."]`, exclusions `[]`. PlanningEvidence.cs:104-107 requires current code inputs for verified entries; lines 118-131 also require every current canonical hash. The old receipt lacks city-research/combat-status-effects and has outdated city-tabletop/combat-archetypes/coop-city-match/ecs-unit-combat/resumable-multiplayer hashes. This is an implementation/lifecycle integration defect, not a new product decision. `tools/DevRunner/Exports.cs:134-138` checks this validation before source CI preparation, so this candidate cannot pass its checked-in CI gate. No CI was executed; this consequence follows directly from the source.

NON_BLOCKING_FINDINGS:
No defect found in the six-row README guide itself. Canonical linux-test-execution still contains older serial source-UI and dedicated desktop-dev descriptions; the approved design explicitly leaves pre-existing speed-up-verification canonical reconciliation separate. The guide does not restate those stale details. No usability, native GPU, audible playback or runtime performance claim is made.

REQUIREMENT_EVIDENCE:
Review identity and revision setup:
- Fresh independent Reviewer task/session: poc-verification-command-guide-integrated-review, this newly dispatched Codex review session (harness session UUID not supplied). Attempt: fresh integration review; prior review1 is historical, not reused. Distinct original Shipper: poc-verification-command-guide-ship-r2, session 01a10153-b16e-7691-aef9-c25f53c30e4a, slot 2.
- Own detached worktree: /home/bart/.treehouse/odot-game-ab1b67/5/odot-game. `pwd`, `git rev-parse --show-toplevel` and `git worktree list --porcelain` establish isolation from registered clone /home/bart/.local/share/firstmate/odot/projects/odot-game and other worker slots. No developer checkout was switched or written.
- Exact base: 6a7a179501de631c4ec3f2e47b314ae0897436bc.
- Exact candidate/head: b70c0c71958ea6fb16f75461a13599b28675822f.
- Initial worktree clean. `git cat-file -t` confirmed candidate already local, so no fetch required. `git switch --detach` succeeded, `git rev-parse HEAD` matched candidate, `git rev-parse` base succeeded, and `git merge-base BASE HEAD` equalled BASE. No moving-main substitution.
- Before content review, raw export-transfer.png and `git show HEAD:docs/images/export-transfer.png | sha256sum` both gave fffece35d8fcd3a95894f1d49868da079bc0d694d75cf714a5bf947346a96d3f.
- Read required reviewer and captain-hold skills, proposal/design/tasks/metadata, approval/admission/implementation handoff, historical receipt, relevant canonical requirements, AGENTS, ADR 0001, planning conventions, full relevant base..head diff, README references and current command implementation. No Shipper conversation was used.

Current identities, from the permitted candidate planning-inputs command (exit 0):
- head: b70c0c71958ea6fb16f75461a13599b28675822f
- spec_digest: 7ec147b34b7293c7cd25ddaa7419ee0e586d2a7cbf1efe43c6c285479bbbb9c2
- code_digest: 8737b47eb2e6a798fdcc091ee0786a54e22a4cac1ff63af9c06e0b4ebbcb045b
Independent read-only Python SHA-256 reconstruction of normalized change artifacts and sorted production file/hash inputs reproduced both digests. All 18 canonical hashes were independently recomputed and matched planning-inputs:
- `openspec/specs/background-music/spec.md`: `d89f977b9cb41384c96a675009a1fb20544b02c108974e8fa251a018641b1f2a`
- `openspec/specs/city-research/spec.md`: `20d764abbcb906f7e19b83374e442aec9f79ddb0471141aef37c30d05288d9c0`
- `openspec/specs/city-tabletop/spec.md`: `5db83666994c9039e73270d17c145dabda10366d50d6cbef0acdfa358c2fb25a`
- `openspec/specs/client-settings/spec.md`: `674e52ef413b746502e472a543e5638e035a666223ed42c3c5ebb54c4122f3af`
- `openspec/specs/client-updates/spec.md`: `6d5e9f6a1cecd17eb1536539893299116c10fe110af05473e81a96f5c9560793`
- `openspec/specs/combat-archetypes/spec.md`: `e0d5b79b3f024e4cc1652587136b9c19cca210685f5184d0c2d734a9638eabca`
- `openspec/specs/combat-status-effects/spec.md`: `0224822d5d934281f4c4721dda78eb766350c7a8e3a15984d586ad7aa5a50ced`
- `openspec/specs/coop-city-match/spec.md`: `30771a00464f90a794b4c89bab6f2259a70f53b8e8e58efef458d824c0fb0d85`
- `openspec/specs/coop-verification/spec.md`: `f12e88c676363eb79d925a260ce626aa8b214e3769df68e68f23610626f3c058`
- `openspec/specs/desktop-installation/spec.md`: `0f969e51b2d05a1913cbb04536c7970f62749a2cd46de150822aa35cdc1375e5`
- `openspec/specs/ecs-unit-combat/spec.md`: `bdb67876daf23b8925131d1f212255facd30b580c753a6b88ef7897a9ce5e1e9`
- `openspec/specs/game-feedback/spec.md`: `31fba8bae3cba5fd0c19fa4fc6d098fe9e0f868f38dda4749658f5e99408e286`
- `openspec/specs/game-launcher/spec.md`: `24ad22c7f503016ec0b0fac1f91b1c68b21c169375d7c4ae7a2a127cecde5ff0`
- `openspec/specs/linux-test-execution/spec.md`: `0d2c7e598da92b5f43577d0685ebedba147088b0e6a6696b656ff0ee20bc4b98`
- `openspec/specs/resumable-multiplayer/spec.md`: `8da875dc9aef166c9c501ac5aafc0b09829722d6b0c19edfc3c5a6b8f7c7c0e4`
- `openspec/specs/steam-sessions/spec.md`: `d146e6d8d708b9089a62e4d77b0bdbc891436694813d391c5afe23b4f2695bd2`
- `openspec/specs/themed-ui/spec.md`: `879da092ef66b80c431e0e9af2ed2e196cb0abb06968f616569a7d62a423572b`
- `openspec/specs/versioned-distribution/spec.md`: `d678fb71a17b06be01e0a10db2db4a19abb4cab362e02e95444bde7c4040ec21`

Requirement/task evidence:
- 1.1: README.md:117-132 places Choose a check first under Verification and exports, contains exactly six approved task rows, and expresses preparation, relative cost and coverage. Numerical/runner check maps to mise.toml:52-54 (locked restores/builds of cheap suites, no Godot). Network redistribution is registered at Scenarios.cs:11 and selected by NetworkTests.cs:99; Runner.cs:38 and :148 onward prepare standalone source before real headless peers. Reconnect is registered at Scenarios.cs:12; Runner.cs:40-43 prepares standalone source and checks display prerequisites; UiTests.cs:469-510 exercises actual Reconnect, identity, exact state and current rendered restoration. Exported-package bypasses source preparation at Runner.cs:42, requires both existing executables at PrivateDisplay.cs:80-85 and never implicitly exports. Full CI source scheduling and sequential client/server/headless/graphical package gates remain in Exports.cs:129-185. Planning mise wrapper depends on locked runner restore/build at mise.toml:48-50,228-232; Program.cs:11-12 dispatches planning directly without game Runner. The README describes these commands accurately, although this candidate's lifecycle defect presently prevents its full CI from proceeding.
- 1.2: All eight guide local links/anchors independently resolved: AGENTS policy, planning README, verification-command-details, verification speed evidence, Setup, simulation-performance, distribution, external Steam prerequisites. README.md:119 directs readers to affected selectors; :130 preserves partial/full gate distinction, full substantial-task before/after checks, unchanged baseline reuse, normal CI and repeat policy. :132 preserves user-managed locked prerequisites, unexecuted/failed distinction, private graphics/audio limitations and separate runtime/native/paired-Steam routes. These agree with current AGENTS and canonical cost/ownership/coverage requirements. No timing promises or selector matrix was added.
- 2.1: Strict change validation succeeds but current planning validation fails as above. The historical task checkbox is not evidence of current successful integration validation.
- 2.2: All four implementation checkboxes are complete. Full base..head diff has 65 files, 3067 insertions and 20 deletions because the candidate also imports the agent-workflow POC. Those additions must not be described as a README-only overall integration diff. Guide implementation commits e4c8379 and fe51da1 change exactly README (19 added lines), this change's four task checkboxes, and implementation-r2 handoff. Guide content survives integration, while current-main research content is preserved. Base..head contains no src, gameplay-test or canonical-spec changes. Additional planning commands/tests/YamlDotNet locks, agent skills/docs and the CI planning gate trace to the separate workflow bootstrap/integration history (df90622,5010217,95a3321,daa62e2), not the guide implementation. No game/runner/test behavior change attributable to this guide was found. The separate workflow integration is not granted a whole-POC PASS by this review.

Commands/results:
- `openspec validate poc-verification-command-guide --strict`: exit 0, valid; skip_specs explicitly accepts zero deltas.
- `dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-inputs poc-verification-command-guide`: exit 0, current identities recorded above.
- `dotnet /home/bart/.local/share/firstmate/odot/projects/odot-game/tools/DevRunner/bin/Debug/net10.0/DevRunner.dll planning-validate --json`: exit 1, exact stale code_digest diagnostic above. It was run once; no source repair attempted.
- Read-only Git diff/stat/name/history and direct text reads; independent Python link/row/task/digest/blob checks: succeeded. `git diff --check BASE HEAD`: exit 0.
- No mise wrapper, tests, game processes, Godot, CI, exports, build, restore, install, sync, archive, merge, push or other worker was run.

Source read-only proof:
- Initial, detached-candidate and final `git status --porcelain=v1` empty; HEAD stays exact candidate.
- All 891 tracked working-tree files were compared with committed blobs using `git hash-object -- FILE` against `git rev-parse HEAD:FILE`: zero mismatches. PNG byte integrity was independently confirmed before review. No production/planning source edits made. Writes limited to requested external report/status and expressly requested completion lifecycle attestation.
- Exact steering inbox was absent/empty at setup and final review checkpoint; no messages awaited acknowledgment.
- Elapsed through report construction: 210 seconds from setup epoch 1791025305. No human decisions were discovered. Captain-hold completion inventory: --none; command result is recorded below before done.

SPEC_DEVIATIONS:
No guide specification deviation found. Approved spec_digest still matches approval; no capability delta is introduced. The blocking failure is current lifecycle evidence/validation, contrary to planning/README.md:33,58 and ADR 0001's current-PASS/validation gates, not a requirement ambiguity.

RECOMMENDED_NEXT_ACTION:
Do not merge/archive this exact candidate. Have an authorized planning writer retain the old receipt as historical and reset the integrated guide's verified admission to a state suitable for fresh review (for example in_progress without an authoritative stale verification binding), then rerun planning validation and obtain a fresh independent review on that exact repaired candidate. Capture a fresh PASS with its actual candidate code_digest and all 18 canonical inputs only after validation succeeds; do not rewrite the old PASS's identities or waive the gate. README content needs no repair based on this review. This report is a fresh FAIL_IMPLEMENTATION for b70c0c71958ea6fb16f75461a13599b28675822f, not a merge/archive or reuse of fe51da1's stale PASS. No unresolved product/architecture decision gates report completion.

Completion attestation: `FM_HOME=/home/bart/.local/share/firstmate/odot FM_STATE_OVERRIDE=/home/bart/.local/share/firstmate/odot/state FM_DATA_OVERRIDE=/home/bart/.local/share/firstmate/odot/data /home/bart/projects/personal/firstmate/bin/fm-captain-hold.sh complete poc-verification-command-guide-integrated-review --none` exited 0: `complete: poc-verification-command-guide-integrated-review captain-call inventory reviewed`. Final tracked-blob comparison again matched all 891 files, clean status, exact HEAD unchanged, no pending inbox messages. Total elapsed through completion: 236 seconds.
