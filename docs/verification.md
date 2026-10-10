# Cooperative POC verification

## Research controls wall-clock allowance, 2026-10-10

Source run `38039688672`, job `114177279552`, tested head
`65d90ab6141d9f95ea688688899432cdf8488f3b`. The prior full-combat timer
correction was present. Research setup passed in **56.52s**; controls reached
successful transport/identity/research/status recovery at **29.36s**, then their
own **30s** timer cancelled the remaining current-baseline/no-replay check at
**30.04s**. All seven pose proofs passed. Combat exited after **304.73s** including
cleanup; source took **1231.89s**, below the unchanged **1500s** hosted budget.
This is a nested controls timeout, not evidence that the uncompleted assertions
passed, and not a demonstrated stale-request, renderer or import failure.
Provider logs and the original failure artifact are retained under
`logs/source-research-timeout-diagnosis/`.

Authorized correction: the shared research feature allowance is **60s**, with
its **120s** setup, wave-ten/action bounds and all behavioral assertions retained.
Retained local controls passes took **19.63s/20.05s**; the hosted **29.36s** path
was still progressing. Doubling the old feature allowance gives headroom for
serial input/render/transport work rather than weakening its predicates. The
selected research route now inherits its worker budget instead of a redundant
**150s** inner timer. Its standalone default is **210s** (120 + 60 + 30s overhead).
Standalone full combat defaults to **420s**, giving the observed roughly 305s
hosted path room for remaining assertions, fresh-session checks and clean exit.
Unfiltered UI remains **600s**, source/full CI **900s**, complete economy **1080s**,
army **420s**, and melee's **70s** inner bound is unchanged. Explicit worker
budgets and parent cancellation remain authoritative on every route.

Removed only the redundant `research-fire-frost-lock` PNG/global asset check:
the preceding fresh lock observation still requires disabled Frost, permanent
lock text and the exact six-point deduction after actual Fire input. The
`research-current-burn` PNG retains rendered asset/landscape coverage and all
current badge/inspection, pause/disconnect freeze, actual Reconnect, identity,
research/status deadlines, current cursor, no historical flashes/sounds and
resume assertions remain. No ordinary paid setup or unique behavior is dropped.
Estimated extra 30s controls allowance plus final cleanup remains within the
hosted job's approximately 268s observed headroom; this is planning, not hosted
completion proof. Actual assertion failures, stale-match rejections, native
crashes and corrupt imports remain failures, not timing passes.

The prior **708.05s** full local pass, subsequent **122.23s** failed full attempt
(stale observer pause in reconnect), and **203.41s** selected combat pass remain
historical/partial evidence as recorded below. No product/reconnect correction
is claimed by this allowance change, and no failed attempt has been reclassified.

After-change locked restore, formatting/format verification and **504 core +
595 runner** tests passed with zero failures/skips,
`logs/20261010-155229-7ceaecba/`. Executable option tests check both standalone
and worker defaults, forwarding, smaller/larger explicit overrides and unchanged
melee budgets; they do not inspect implementation text. Selected
`mise run test-ui --scenario combat --checkpoint research --startup-timeout-ms
60000` passed without a timeout override: **92.27s** case / **103.04s** runner,
controls **21.03s**, `logs/20261010-155313-c10b50de/`.

The single final `mise run ci --startup-timeout-ms 60000 --ui-jobs 2` attempt
passed in **736.40s**, `logs/20261010-155504-f78a0297/`: formatting, locked
restore/build/import, static fidelity, all **1099** cheap tests (zero failures/
skips), all six network and five source UI cases, sequential Linux exports and
headless/graphical package smokes. Source UI took **656.28s**; full combat
**202.87s**, research setup/controls **38.94s/18.52s**. Admission reports maximum
two scenarios/two displays. Reconnect passed **100.30s** in this attempt, which
does not establish that the earlier stale-pause defect is repaired. The current
burn/inspection and packed gameplay PNGs were opened and inspected. Owned cleanup
and error checks passed; no real desktop/user preferences were used.

This is complete local after-change coverage, not reproduction of hosted timing
or final-head merge readiness. It uses Linux X11/llvmpipe (LLVM 23.1.1) and Dummy
audio, not hosted hardware or native GPU/input/listening evidence. Only evidence/
documentation updates followed verification. Final-published-head required
Source/Linux/Windows/CodeRabbit verdicts remain the outer executor's responsibility;
no push, provider retry, merge or pipeline-control action was performed.

## Full combat inherits its worker deadline, 2026-10-10

Source run `38037663441`, job `114171384708`, failed at head
`3be920477140072b15e4aa82e76e03da1d70586b`. Combat's inner hard-coded **300s**
timer cancelled it despite CI forwarding a **900s** worker budget. All seven
pose proofs and research setup (**55.20s**) passed; research controls were
cancelled after **24.42s**, before their own **30s** bound. Combat exited after
**302.84s**, including cleanup. This is not a hosted job-limit cancellation or
an observed renderer/import failure. Raw provider logs and the original combat
failure artifact are retained in `logs/source-combat-timeout-diagnosis/`.

Removed only the full-combat timer branch. Full combat now inherits the display
supervisor/worker deadline: **900s** in source/full CI, **600s** in unfiltered
source UI, and the unchanged **300s** standalone default. Explicit overrides
remain authoritative. Selected melee/research retain their **70s/150s** linked
bounds; research's **120s setup/30s feature** bounds, every assertion, economy
allowances, other scenario deadlines and the hosted **25-minute** limit remain
unchanged. The risk description now reports the inherited budget accurately.

The unchanged before-baseline is `logs/20261010-080756-8d9d19fd/` (**708.05s**
full local CI). After the correction, locked restore/format and **504 core +
593 runner** tests passed with zero failures/skips,
`logs/20261010-084910-8fecb0ba/`. The full local CI attempt remains **failed**:
`logs/20261010-085003-882c2937/`, **122.23s**, rejected an observer `pause`
with `Stale match, phase or turn.` in the unchanged reconnect setup, before
combat admission, then cancelled siblings. No unrelated reconnect correction
or repeated full-CI attempt was made in this fix round.

Selected `mise run test-ui --scenario combat --startup-timeout-ms 60000
--timeout-ms 900000` passed complete combat, research/reconnect and fresh-session
coverage: **203.41s** case / **211.92s** runner,
`logs/20261010-085220-e8fbb88a/`. Research setup/controls took **41.29s/20.05s**.
This selected local pass did not reproduce the hosted 300s timing failure and
is not complete after-CI coverage or hosted readiness. Final-head required
checks and the separate local reconnect failure remain unresolved.

## Hosted overlapping economy wall-clock allowance, 2026-10-10

Source run `38015276254`, job `114104047930`, failed at published head
`22de8e87eafec8cd5100124ec8190b32210c6781`: economy's display supervisor
cancelled after **900.28s**, not a GitHub cancellation. Cooperative economy
completed at approximately **604s**; the nested army reached its second paid
battle at approximately **296s** before parent cancellation. Full-hall and
expansion/retirement captures passed, but final recovery/capacity/exit did not
complete. Combat was progressing when sibling cancellation stopped it after
**38.02s**; no pose-failure artifact had been reached. These remain failures.
Raw logs and diagnosis are retained in `logs/source-economy-timeout-diagnosis/`.

The earlier serial hosted economy passed in **678.79s**, including approximately
**229s** of army. With two displays, reconnect grew **139.89→213.33s**, settings
**98.87→140.15s**, and launcher **345.97→477.70s**. This supports a contention
allowance, not a claim of equivalent hardware or proof that every timeout is
benign. The exact current-head army selector also completed both ordinary paid
clears, all recovery assertions and five captures locally in **134.72s**,
`logs/20261010-022055-5af82c90/`; that partial local pass is not hosted acceptance.

Authorized adjustment: **complete economy 1080s**, **nested/selected army 420s**.
The CI economy worker was 900s; standalone complete economy was 300s (600s in
unfiltered source UI). They now resolve the same scenario-specific allowance at
the display boundary and forward it explicitly to the worker. Standalone army
was parent-bounded at 300s and now uses 420s, matching its nested bound. Explicit
`--timeout-ms` remains authoritative, including smaller budgets. Every unrelated
timeout, startup/phase/research barrier, two-scenario/two-display cap, and the
hosted **25-minute** source limit remains unchanged.

The measured 604s cooperative path plus 420s army allowance leaves about **56s**
for worker overhead within 1080s. Other hosted graphical cases slowed by roughly
38–53%; 229s × 1.53 is approximately 350s, below the 420s army bound. Adding 180s
to the failed hosted job's approximately 1056s leaves approximately **264s** below
1500s. These are bounded planning estimates, not completion measurements; any
remaining assertion failure, nested timeout or job-budget failure remains a
blocker and must be diagnosed rather than reported as passing.

**Coverage change: none.** The repeated paid opening is not removed: cooperative
economy owns exact displayed balances/receipts, real shortage and paused
reconnect, research/producer retention, foreign controls, two viewport layouts,
camera/roof/plot picking and land/Market inputs. Solo army owns stable inspector
ids, hidden stored actors, full field/hall atomic rejection, purchased physical
homes, first-fit Send, no-refund Retire, independent hall tracks, both paid clears,
funded-but-nonparticipating storage, exact 5%/10% production recovery, five PNGs
and clean exit. Existing `ArmyRosterTests`, `ArmyRecoveryTests`,
`ArmyCommandTests`, `ArmyBalanceTests` and `EconomyArmyFixtureTests` independently
exercise rules/paid setup/wire guards, but cannot replace these real UI witnesses.
No tests, assertions, captures, paid setup or product behavior are dropped.

The unchanged product/suite before-baseline is the retained full local CI pass
`logs/20261010-014527-05d2a93c/` (**704.00s**). Allowance-only changes require a
new full local pass and actual final-published-head Source/Linux/Windows/CodeRabbit
verdicts; neither older evidence nor a local pass establishes merge readiness.

Working-tree verification: locked restore/format and **504 core + 593 runner**
tests passed (zero failures/skips), including executable option/deadline routing
and explicit-override checks, `logs/20261010-073223-1a65ff74/`. Complete selected
`mise run test-ui --scenario economy --startup-timeout-ms 60000` passed all
cooperative and army behavior: **411.20s** case / **419.73s** runner,
`logs/20261010-074241-7f357b1b/`. No timeout override, injected state or skipped
assertions were used. All five army captures and clean exit completed.

**Two prior full local CI attempts remain failed.** Both used
`mise run ci --startup-timeout-ms 60000 --ui-jobs 2`:

- `logs/20261010-073441-48d617b0/`, **417.07s**: native SIGSEGV/exit134 in
  `libgallium-26.2.4-arch3.1.so` during `army-opening` capture, economy case
  **351.85s**. Godot also emitted a caller-thread notification error. This was
  before both old deadlines; no managed-code cause was established. All cheap
  and six network cases, reconnect and settings passed. Launcher was cancelled;
  combat and exports were not reached.
- `logs/20261010-074949-48c1e115/`, **376.81s**: later army/menu clients could not
  load the generated music `.sample` (`ERR_FILE_CORRUPT`), although earlier
  clients loaded music successfully. Launcher failed its real music-loaded
  assertion; economy was cancelled. The generated file's recorded modification
  time predates both attempts; this does not establish why native loading failed.
  All cheap/six network cases, reconnect and settings passed. Combat and exports
  were not reached.

Complete selected economy passed between these attempts; the second full run
was a post-failure recheck, not evidence suppression. No renderer settings,
imports, audio, dependency versions, tolerances or failure handling were changed.
Do not manufacture a passing full verdict or bypass the source gate for exports.
Console logs are retained as `allowance-{final-ci,selected-economy,
final-ci-second}.log` beside the original hosted failure. No provider retry,
push or other pipeline phase was performed.

### Bounded native/resource follow-up

Follow-up evidence is retained under
`logs/source-economy-timeout-diagnosis/native-resource-followup/`, including the
original allowance patch, source/input hashes, WAV/import metadata copies,
importer receipt, kernel messages, native core and bounded backtraces. The
preceding cheap and **411.20s** selected economy proofs were reused unchanged;
no separate cheap/economy repeat was run merely for a new agent invocation.

The exact generated music sample returned **OS EIO** when read. Kernel logs at
the failed clients' startup times report **Btrfs checksum failure** for its
matching inode **1970354**, offset **7479296**. The source WAV still matches its
recorded SHA-256 `3783f4deed1ecd7933a17538a1d880ffb04319f6e11f34dc66f2df7a05ab70b4`;
source MD5 matches the import receipt, and tracked import settings are unchanged.
The sample's old mtime/ctime predates both failures. This establishes an
unreadable generated artifact, not an importer writer race, stale-cache claim
or product audio defect. The underlying corruption mechanism and why earlier
clients could read it are not established.

A same-filesystem rename retained the **original inode and unreadable extent**
in evidence before preparation; a whole-file/prefix copy failed EIO, so no
complete recovered-byte hash is claimed. Only this demonstrated invalid generated
destination was moved: no source reset, other cache clearing or force-import
workaround. Normal `mise run prepare --startup-timeout-ms 60000`, using the
existing owned editor environment and `--editor --import`, reimported one asset
and passed (**7.77s** runner, `logs/20261010-080545-d48d7893/`). The new sample is
readable, **7,900,059 bytes**, SHA-256
`651e0a6adb0958c7ad122528d2a490f6d9874906cd903fb9d454d0ec5d161a1b`;
its MD5 `5b27a19b53f0142bedd356496e934975` exactly matches the earlier importer
receipt. This reconstructs the artifact, not the host filesystem.

The separately retained crash core identifies **llvmpipe-1** as the faulting
thread at the logged libgallium offsets, with the ordinary seed-1 army argv and
unchanged X11/OpenGL/Dummy renderer setup. The native fault is real; these
unsymbolized frames and the preceding caller-thread notification do **not**
prove a driver/engine bug, managed off-thread call or a causal relationship to
the later sample corruption. No small product correction was demonstrated, and
no renderer/audio/dependency settings or error handling were changed.

After repairing that relevant generated input and re-tracing all allowance
callers/overrides, **one** final normal
`mise run ci --startup-timeout-ms 60000 --ui-jobs 2` passed in **708.05s**,
`logs/20261010-080756-8d9d19fd/`. It includes locked restore, formatting/build/import,
static fidelity, **504 core + 593 runner** tests (zero failures/skips), all six
network and five source UI cases, sequential Linux exports, headless package
smoke and graphical package smoke. Source UI took **628.14s**; economy completed
**429.80s** case / **433.18s** worker, including all five army captures and clean
exit. Admission evidence reports maximum **two scenarios/two displays**. The
actual packed gameplay PNG was inspected. Product/test input hashes were
unchanged through this run, and the regenerated sample's hash/inode/timestamps
were unchanged after exports and both package checks. No matching kernel resource
errors appeared during this final run.

This is genuine full **local** coverage on the retained allowance delta, not a
claim that the unexplained native crash or host storage integrity is resolved.
The earlier failed attempts remain failed; host checksum errors need owner
attention outside this worktree. Required final-published-head
**Source/Linux/Windows/CodeRabbit** verdicts remain outstanding and belong to the
outer executor. No merge readiness, waiver, publication or provider retry is
claimed.

## Fixed-position settlement beautification, 2026-10-07

The [change verification record](../openspec/changes/beautify-settlement-world/verification.md)
owns the fixed-position scope, retained failures, protected-boundary proofs,
matched costs and live capture inventories. Its
[historical local acceptance section](../openspec/changes/beautify-settlement-world/verification.md#historical-local-acceptance-at-53779df)
binds earlier post-review CI/native/glyph receipts to their tested source head;
its [accepted finishing scope](../openspec/changes/beautify-settlement-world/verification.md#accepted-finishing-scope-after-coderabbit-followup)
owns validation requirements after the later followup. See
[authored diagnostics](authored-performance.md#protected-boundary-and-native-investigation-modes)
for the retained comparison and investigation command contracts.

## Authored 3D migration, 2026-10-05

The [migration report](authored-migration.md#retained-acceptance-evidence) owns
before/after CI outcomes, source/package costs, retained failures and follow-up
decisions. Its complete migration pass precedes review commit `8eeabc9`; it is not
full acceptance of that commit's changed inputs. [Current casualty admission](current-casualty-admission.md#status)
owns the freshness correction's focused evidence and remaining acceptance status.
[ENet guest cadence](enet-guest-cadence.md#early-observer-and-exact-paused-images)
owns the separate four-view melee proof, which ordinary full-CI combat does not replace.

These owned Linux software-rendered results retain their stated platform,
physical-input, listening and Steam limitations; no speculative source-project
optimization or additional feature is implied.

## Settlement-command architecture hardening, 2026-10-04

Behavior-preserving request construction, internal city-edit dispatch and scoped
persistent placement retain protocol 11, paid roster/recovery rules, numerical
configuration, dependency/asset locks and all five frozen reference digests.
`CommandConstructionTests` independently checks contextual fields, defaults,
wire parity and stale request/retry behavior. `SettlementCommandTests` passed
five paid authority/placement characterizations before extraction; the recurring
acceptance command is `mise run test`, without a Godot process.

Full before-CI on landed `8f3d62e` passed in **350.61s** (**481 gameplay / 173 runner**), evidence
`logs/20261004-043222-241a9359/ci-summary.json`. The first focused
`authority-resume-victory` run failed in **6.71s** (runner **12.30s**),
`logs/20261004-050537-85a32d77/`: seed `11669866211869037831` left two full
homes after policy refill, but the fixture unconditionally recruited a Crossbowman.
`EconomyArmyFixtureTests.RecoveredNetworkSeedPaysForRoomBeforeExplicitRangedRecruitAndKeepsRetries`
replays ordinary paid production/battle setup on both landed baseline and
refactored source (**1.0420s / 1.0384s**). It preserves exact veteran ids,
wounds/assignments, atomic full-home rejection, quoted 5-gold expansion,
2-wood/1-metal equipment payment and original accepted/refused identities through
retries/rebind. The existing network slice now reuses `EnsureFieldRoom` before
its explicit recruit and payment snapshot; no grants, retirement, weakened
assertions, gameplay change or additional expensive scenario.

Corrected `mise run test-network --scenario authority-resume-victory` passed
in **26.12s** (runner **31.76s**), `logs/20261004-051549-3ee514e9/`.
`mise run test-ui --scenario economy --checkpoint army` passed in **52.72s**
(runner **61.01s**), `logs/20261004-051622-e0e5fb0b/`. Inspected baseline/after
full-hall PNGs are byte-identical; after opening, expansion/retirement and
paid-recovery frames retain readable controls, physical locks and veteran ids.

Full after `mise run ci` passed in **341.79s**: **494 gameplay / 173 runner**,
zero failures/skips; all six network cases, five source UI slices, sequential
Linux client/server exports, headless package smoke and graphical package smoke.
Evidence: `logs/20261004-051738-7303260d/ci-summary.json`; source UI **290.50s**,
exports **5.94s / 3.85s**, headless smoke **1.30s**, graphical package **15.25s**.
The packed 1280×720 frame was inspected. Owned peers/displays/data cleanup was
awaited with no cleanup error; no desktop sessions or preferences were used.
This is Linux x86_64 X11/llvmpipe/Dummy-audio evidence, not native GPU/compositor,
physical input/listening, Windows/macOS qualification or paired Steam acceptance.
Only the private review HTML's optional browser-render audit is explicitly
unverified/deferred for missing owned browser prerequisites; all required game
checks executed. No uploads or publishing occurred.

## Economy, army and campaign implementation baseline, 2026-10-02

At source revision `ad771f7595db3a9db53a0a698bba281644597fcf`, the initial
full CI attempt failed only in `authority-resume-victory`: its 80 individually
pumped stdin requests did not reliably exceed 64 requests in the authority's
one-second rate window. Evidence: `logs/20261002-094116-6cb10d7f/ci-summary.json`.
The supervised guest now accepts bounded `raw-burst <1..256> <json>` input,
sending ordinary Request RPCs in one frame; this grants no gameplay state and
changes no authority limit. The existing case sends 192 null requests and
requires the same rate rejection and continued usability of the other peer.
Cheap authority tests still verify exact rate limits, malformed/oversized
rejection, no state mutation and next-window recovery. No expensive case was
added; the existing network setup/runtime cost is retained. Corrected full CI
passed in 433.27 seconds, with all source checks, six network cases, five source
UI slices, both exports and both package smokes. Evidence:
`logs/20261002-094618-8056a636/ci-summary.json`.

## Size-based combat gate, 2026-10-02

Rules v3/protocol v7 replace shaped footprints with per-actor size claims and
six fixed render anchors. Ordinary defaults use size two and explicit bosses
size six. `UnitSizeTests` covers all size pairs and anchor arrangements, mixed
2+2+2/1+2+3, invalid sizes, profile/research/transfer/wire retention, full-size
atomic moves/death and work-conserving protected admission without cross-cell
pooling. All 283 gameplay and 120 runner tests passed.

Default stats/timings are unchanged at this stage. The paired normal role
fixture uses nine Swordsmen against seven enemies at seed 123 in both controls:
Mage effective damage 1800 versus Crossbowman 1100 (hundredths) in the shared
live interval, two secondary hits, clear ticks 373 versus 439 and remaining
friendly health 2200 versus 1000. Ordinary clear/transfer witnesses use seeds
1/2/8; the earlier seed-4 witness no longer establishes a cleared receiver under
the new fingerprint. These finite fixtures retain all role/progression assertions.

Selected source combat passed in 33.12s (42.21s supervised including preparation),
evidence `logs/20261002-100455-4da608ed/`. Selected reconnect passed in 30.15s
(38.35s supervised), `logs/20261002-100608-4c4d8e34/`. They uniquely prove imported
rigs, fixed-anchor/route clearance and restored current moving/dying rendering;
no new expensive scenario was added.

The independent seed-1 melee checkpoint passed in 57.10s (65.34s supervised),
`logs/20261002-101110-ff674639/`, retaining 51 live-node witnesses and four
near-windup/far-impact overview/close PNGs. A preceding attempt exposed a phase
sync race between observer and graphical peer; next-stage requests now await
current peer state. Its capture deadline is 65s after measuring the former 40s
allowance expire after the first capture. The required seed-1 proof remains
bounded to two waves; the additional cheap seed-0 detector sample needs wave
three for its opposite-side impact. Software rendering does not establish
native GPU/input/listening quality. Final feature CI is still required.

## Combat math and flow simplification, 2026-10-02

The before baseline passed full `mise run ci` in 431.98 seconds: 256 gameplay
tests, 120 runner tests, all six network cases, source UI, sequential exports
and headless/graphical package smoke. Evidence:
`logs/20261002-082928-4fad0b89/ci-summary.json`, with `source-head.txt` and
`source-sha256.txt` recording the source inputs. No required stage was skipped.

`CombatDecisionRegressionTests` covers two review defects: a swordsman at cell
17 must replace its farther retained cell-2 objective with the shorter legal
approach to cell 8, and a defender configured with victim cap two/radius one
must damage an eligible adjacent secondary. Their recurring acceptance command
is `mise run test`; these numerical checks use C# and launch no Godot process.
Existing network/UI/export gates cover integration; these defects require no
new expensive scenario.

The implementation stores one immutable `CombatUnit` action component; snapshots
and occupancy evidence derive from it. Pure C# action transitions, shared
distance/initiative/seed ranking, reachability and victim selection can be tested
independently of ECS and `Match`. `CombatPolicyTests` compares integer BFS against
bounded reference relaxation, checks one expansion for seven opponents and at
most two for retained-episode repair, and covers changed objectives, arrival,
released footprints and local observations. `CombatActionTests` checks exact
deadlines and rejects conflicting fixture state atomically. `DefensePolicyTests`
checks full profiles, cap/radius, invalid primary, no friendly fire and exactly-once
unit/defender/tower contributions against a shared pre-damage view. Existing
pause, elimination, ordering, snapshot reconstruction, lifecycle and session
tests remain mandatory.

Combat rules version 2 changes seeded outcomes; SplitMix algorithm version 1,
protocol v6, serialized shape and all default numerical profiles remain unchanged.
The ordinary solo frontline/mixed/tower/research and two-to-four-city frontline
strategies still cover seeds 0, 1 and 123. The cleared-forward transfer witness
uses seeds 1, 4 and 8; all cleared-city, held-forward-footprint, bounded admission
and continued-progression assertions are retained. The simpler upgraded-farm/
barracks opening uses seed 1. C# witness probes confirmed those setups under the
new identity in `logs/20261002-082928-4fad0b89/rules-v2-strategy-witnesses.log`;
old witnesses no longer establish their specific battlefield preconditions.
These finite witnesses do not prove balance for every seed. The historical
rules-v1 measurements below remain historical evidence, not current traces.

`mise run test` passed 275 gameplay and 120 runner tests after the coherent
refactor (`logs/20261002-082928-4fad0b89/refactor-final-cheap.log`). No engine,
dependency, tool-lock, economy or campaign changes were needed.

Final full `mise run ci` passed in **421.81 seconds**: 275 gameplay tests, 120
runner tests, all six network scenarios, all five source UI slices, sequential
client/server exports, headless package smoke and graphical package smoke.
Evidence: `logs/20261002-092110-29c7d1ba/ci-summary.json`; source UI 236.60 seconds,
exported UI 111.14 seconds. The complete console log and source hashes are
`logs/20261002-082928-4fad0b89/refactor-final-ci.log` and
`refactor-source-sha256.txt`. Locked solution restore, solution formatting and
build passed. No required stages failed or were skipped; nothing was published.
Software-rendered UI evidence does not establish native GPU performance or
physical input/listening behavior; ordinary CI does not establish real-account
Steam acceptance.

## Deterministic hex combat implementation, 2026-10-02

Final full `mise run ci` **passed**: 256 gameplay tests, 120 runner tests, all six
network scenarios, all five source UI scenarios, sequential client/server exports,
headless package smoke and graphical package verification. Evidence:
`logs/20261002-080220-8a1264c2/ci-summary.json`; 445.58 seconds total, 249.22 seconds
source UI and 113.01 seconds graphical package checks. No stages failed or were
skipped; nothing was published. `source-inputs.json` records HEAD, tool versions
and the per-file build/asset inputs; combined SHA256 `42d3fd1b988f7d22736388c0da488a2632e4d042424be61e48116ff3a43698e6`. This passing
state is the unchanged before-baseline for the following progression change.
The chronology below retains earlier partial checks and corrected failures.

The full pre-combat-change baseline passed `mise run ci` in 392.77 seconds,
including source checks, every network/source UI scenario, sequential exports,
and headless/graphical package smoke. Evidence: `logs/20261001-195415-e3ddb2d9/`,
with `ci-summary.json` and `baseline-inputs.json`. Inputs: HEAD
`a8d1838571cbbcbf31548079227559790cbb9d57`, source SHA256
`d7660219bb4c62651d161db3470a8899640dc60a46acdcec5548dbefcc7e1c13`,
.NET SDK 10.0.401 and Godot 4.7.2.stable.mono.official.ed1daf0bf.
This is baseline evidence, not final acceptance of the new implementation.
The preceding aggregate UI timeout was repaired using the measured 215.35-second
suite: default full-source budget 300 seconds, selected slices 180 seconds,
explicit overrides preserved.

Earlier execution restrictions denied local Unix/UDP sockets, VSTest and semantic
formatting. Their in-process xUnit passes and unexecuted graphical attempts remain
historical evidence under `logs/hex-combat-20261002/`,
`logs/hex-melee-checkpoint-20261002/` and `logs/hex-lifecycle-traces-20261002/`.
The resumed environment permits owned socket binds and normal execution.
`dotnet restore Odot.slnx --locked-mode` and
`dotnet format Odot.slnx --no-restore` now pass. Normal `mise run test` passes
**256 gameplay tests and 120 runner tests**, with no failures or skips
(approximately 10 and 1 seconds). The final selected source preparation builds
the whole solution with zero warnings/errors. Final full CI now passes as recorded above.

The later DTO migration removes continuous unit coordinates, uses integer profile
ranges and discrete event impact poses, and reconstructs occupancy from complete
living/dying snapshots. Authority and playback tests cover history-free deaths,
seeded authority equivalence and terminal poses. The ordinary three-city transfer
fixture uses seeds 0/1/2: the receiving city clears with surviving enemy-forward-band
defenders, admits reinforcements within the original bound and reaches wave three.
These are deliberately chosen positioning fixtures, not a claim that every seed
ends a wave in those cells. Cheap evidence: `logs/hex-integration-20261002/cheap.log`.

The extended real-ENet `redistribution` slice passes in 38.41 seconds (runner
43.56 seconds), with seed 1, ordinary purchases, an eliminated observer reconnect,
unchanged identity/damage/recovery and 9/9 allocation assertions, then a second
transfer into a cleared forward-held battlefield, bounded admission, ensuing hits
and ordinary wave-three preparation. Current reservations/configuration are
reconstructed on received snapshots. Evidence: `logs/20261002-072815-36d9a5d3/`.
The preceding attempt `logs/20261002-072733-a9792b8b/` exposed a harness wait that
matched historical Building state; the corrected wait requires the later tick
and wave three. This selected pass is partial integration coverage.

The selected `authority-resume-victory` scenario also passes with a natural
paused casualty, full reservation reconstruction before/after process restart,
unchanged duplicate-request/credential protections and ordinary three-wave victory.
Evidence: `logs/20261002-072906-1ba16b0f/`; scenario 37.24 seconds, runner 42.19 seconds.

The selected ordinary network `defeat` check passes with all cities at zero HP,
`AllCitiesFallen` and no stall diagnostic: `logs/20261002-073127-79c35612/`,
9.40-second scenario / 14.30-second runner. Graphical `reconnect` passes with a
natural paused casualty, retained camera/health checks, same-process reconnect,
then an owned process restart using only that client's saved session. Current
death intervals reconstruct without historical sounds/effects; resume expires
the bodies and reconstructs released reservations. Evidence and inspected restart
PNG: `logs/20261002-073043-b9ed8c3c/reconnect/`; 29.12-second scenario,
37.51-second runner. This extends the existing scenario with one owned restart;
cheap reconstruction checks alone cannot detect graphical startup or sound replay.

The full combat visual fixture also fixes seed 1. Its movement witness now
accepts either world axis and samples the hand's skeleton-global transform,
including animated parent bones; the local attachment rotation can legitimately
remain constant throughout locomotion. The main timing correction uses the headless observer to pause at authoritative
tower-impact and casualty milestones while the graphical client renders and
captures frames. A graphical pause click also follows the fresh movement
observation before screenshots. These barriers avoid consuming the short fight
and death intervals during graphical work. The earlier full-combat failures
remain recorded at `logs/20261002-073214-ff980dc4/` (stale pause),
`logs/20261002-073347-53304b51/` and `logs/20261002-073531-40dcbb2b/`
(missed animation opportunities). The corrected full combat slice passes with
fresh live role/attack/hit observations, stationary recovery, a paused current
casualty, declared-tick model removal, reconstructed reservation release, camera/
resize checks and return to a fresh solo session. Evidence:
`logs/20261002-074659-36373d39/`; scenario 39.06 seconds, runner 47.50 seconds.
The full 60-second combat deadline and existing assertions remain in force.

After the discrete DTO/feedback migration and corrected skeletal observation,
the melee checkpoint passes again with all four overview/close PNGs inspected:
`logs/20261002-074808-77e5bb94/`, 33.22-second combat scenario / 41.54-second
runner. Fixed anchors, directional strike guides, near/far targets and retained
deaths remain visibly distinguishable. The subsequent full CI pass is recorded above.

The first final-CI attempt (`logs/20261002-074909-2d8e01b5/`) passed locked
restore, formatting, build, import and all 256 gameplay / 120 runner tests. It
stopped in concurrent network verification because the new reinforcement predicate
examined an old lobby snapshot before player two existed. The predicate now
requires second-wave Combat and a matching eliminated player. The corrected selected redistribution slice passes in 34.03 seconds
(39.00-second runner), `logs/20261002-075024-81c5dab6/`. The failed full attempt
and selected rerun are partial coverage, not final CI acceptance.

The next full run (`logs/20261002-075118-700f517e/`) passed all six network
scenarios, all source UI (233.89 seconds), both sequential exports and headless
package smoke, then failed the packed shooting observation after an unpaused
capture consumed its opportunity. Package smoke now uses seed 1 and observer-driven
tower/shot/death pause barriers with ordinary graphical resume controls, retaining
the compact live-rig, action and current-death assertions. It is checked against
existing exports; this runner-only correction does not implicitly rebuild them.

The corrected `test-ui --scenario exported-package` passes against those existing
exports: `logs/20261002-075929-fbc66dec/`, 108.50-second package scenario /
111.86-second runner, including both menu/solo viewports and compact combat.
Packed shooting/death PNGs were inspected. Live exported rigs retain typed hex
positions, timed routes/actions and current death intervals. Its added pause
barriers use the existing observer and ordinary resume input; no new authority,
privileged combat state or full graphical battle is introduced. The subsequent full CI pass includes this harness correction.

The standard configuration fingerprint is
`8c0080f3af737848ff96499b7c810cee14fa9618cb6ba6d608fb40b1416ec501`.
All 21 ordinary strategy/seed cases at seeds 0/1/123 retain three-wave victory,
nine productions and the investment rules. No-investment seeds 0/1/123 lose at
ticks 481/523/547 through zero city health and `AllCitiesFallen`, never a stall.
The paired seven-Swordsman screen fixture credits Mage 20 effective damage
against Crossbowman's 15 over their common live interval through tick 325.
Mage records three secondary hits, clears at 325 versus 403 and preserves 18
friendly health versus 8. Recruitment remains seven food/two gold versus five
food/no gold: a role comparison, not equal-cost superiority. Against one ordinary
Mage opponent, the isolated Crossbowman clears at tick 139 and retains four
health; Mage clears at 175 in a simultaneous death. Screened support hits at 79
without support damage; its unscreened control first takes damage at 103.
These finite fixtures establish the required contributions, not universal balance.

Each transferred archetype encounters defenders occupying all three neutral
enemy-front cells. Free protected rear cells admit immediately at transfer tick
27. Fragmented retained deaths yield the original admission bound of 48 for
cost-one support and 72 for cost-two melee, unchanged during retries. Actual
identity/profile/health/recovery, a landed attack, ensuing health reduction and
ordinary completion at tick 126 or 138 are checked. Defenders cannot occupy the
protected row. Cumulative mask-release coverage refuses melee at the first
fragmented expiry. Conserved queues and BattleStalled do not count as admission.

The 32-vs-32 fixtures compare complete normalized snapshots every tick through
cleanup, including deaths, reservations, decisions/routes, defense and outcomes.
Only match ID/revision are normalized. Reverse insertion, unrelated match IDs
and additional observation/serialization on one side preserve the trace.
The mixed battle exercises Mage secondary selection, resolves at 790 and finishes
cleanup at 838. All-melee resolves through ordinary city-health defeat at 1456.
These disable the defender: fingerprint
`237144aab3b3c9ef2710304b3c5803a2fc73f8dc71dfd3f2a877d6685f3622bc`.
The 80-per-side queue resolves at 2620 with `AllCitiesFallen` and 154 deaths.
The accelerated zero-damage 64-per-side history stress serializes 3,810,327 JSON
characters and 2,674 retained events; this is stress size, not production
bandwidth acceptance. Routing fixtures cover detours, feasible ranking, screened
support, capacity preferences, transit conflicts, stable waits and revision-cache
invalidation. A natural moving casualty dies at tick 19, freezes 18 movement
ticks and keeps both endpoint/transit locks through death end 67. Dying transfer
is rejected without mutation; disposal clears locks/events/decisions for a fresh
combat. Existing simultaneous elimination, transfer recovery, rapid readiness,
pause, terminal frozen pose and cleanup-income guards also pass.

The early rendered gate **passes**:
`mise run test-ui --scenario combat --checkpoint melee`.
Accepted evidence is `logs/20261002-063429-e14c58ca/`, including timing summaries,
owned peer/engine logs, `combat/combat-melee-witnesses.json`, four PNGs and matching
observations. The selected checkpoint launches the ordinary authority with
explicit combat seed 1 and uses real two-Farm/Barracks controls to recruit six
Swordsmen over three productions. There are no grants, custom placements or
combat setters. Optional authority `--combat-seed` arguments support repeatable
diagnostics; other launches still generate one seed. Guests consume it.

The headless peer observes action milestones and sends ordinary pause requests
while the graphical client continues fresh probes. One ordered driver per child
and awaited cancellation keep ownership intact. Actual live nodes prove the
shared simultaneous windup at tick 214 (impact 223) and later near-side landed
impact at tick 253. Camera-only inspection leaves paused poses/actions unchanged.
Each phase has an inspected overview and close frame. The explicit dark target
guide and warm progress/landed stroke identify stationary miniature-to-target
attacks; landed target flashes remain distinct from windup/misses. Imported rigs,
weapons and fixed shared anchors are visible. Screen-sized role labels prevent
overview text from disappearing. Retained dying models remain visibly separate.

The recorded 86 fresh observations contain 119 moving-body samples covering ten
moving identities and six dying identities; minimum observed root separation is
0.802 world units, including visible deaths. The checked-in clearance assertion
passes throughout. Four-frame inspection and these finite route samples establish
this representative setup, not every possible customized footprint/animation.
Largest ordinary snapshot observed here is 104,557 JSON characters. Review metrics
are retained in `combat/review-metrics.json`; they supplement the checked-in
acceptance rather than replacing it.

| Selected gate phase | Seconds |
| --- | ---: |
| Locked restore | 0.91 |
| Solution build | 3.57 |
| Godot import | 2.99 |
| Combat setup, live assertions, four captures and cleanup | 34.21 |
| Source graphical phase including owned display | 37.53 |
| Runner total | 45.19 |

The checkpoint remains bounded at 40 seconds inside the 60-second combat case.
It extends the existing peers/display/input/capture setup, with no new full-match
scenario. Its defects are actual imported-model mapping, target attribution,
shared-cell cue/role readability and rendered clearance; core tests cannot see
them. Recurring maintenance is the current action/node detector and four captures.
Cheap seed-0/1/123 opportunities remain additional coverage: windup/impact
866/935, 211/250 and 547/803, with ordinary wave-two progression for two seeds.
Those opportunities do not claim identical capture pairs for every generated
seed. A failed generated-seed run is retained at `logs/20261002-060218-25981097/`.

The first seeded real run (`logs/20261002-061806-d9405af4/`) captured windup but
failed later pause synchronization: the authority repeatedly enqueued unchanged
approximately 100 KB paused snapshots, delaying the graphical peer's later state.
Changed-revision publication fixes this while preserving three-tick combat cadence,
prompt pause/non-combat changes, welcomes and command acknowledgments. The next
run passed at `logs/20261002-062710-a58fa22b/`; image review then required clearer
windup guides/overview role labels. The label/guide run passed at
`logs/20261002-063139-5eab6e5d/`; final stroke layering is the accepted run above.
All attempts awaited owned peer/display cleanup; no developer state or desktop
was used. Rendering uses private X11, llvmpipe and Dummy audio and does not
establish physical input, native GPU/compositor performance or audible quality.

The early numerical and rendered gate now accepts the current board/profile
candidates for integration. Comprehensive snapshot/history/reconnect/network/
package coverage, obsolete continuous DTO removal, final tuning and full CI
remain pending. Historical evidence below does not complete those obligations.

Verified on 2026-09-30 on Linux x86_64 using .NET SDK 10.0.401 and Godot 4.7.2 .NET. Historical desktop checks below used real X11 windows under KDE Wayland with the Compatibility renderer and an AMD Radeon RX 6800. The current recurring workflow uses private Xvfb displays and Mesa software graphics. Platform and remote-run coverage is recorded in the dated sections below.

## Rules and real processes

The core xUnit suite passes 17 cases. They cover one-to-four-player initialization, snapshot serialization, fixed rosters, atomic owned-slot spending, recruitment and upgrades, disconnected production, ready/unready, unique turn guards, retry identity, bounded deterministic combat, weak defense, simultaneous damage/deaths, immediate enemy conservation and remainder distribution, original-roster future allocations, persistent soldier/city damage, nine production turns, victory/defeat precedence and pause without catch-up. A pause/disconnect/resume regression proves that an already eligible ready check resolves once on resume.

The final real ENet suite passes using separate headless Godot processes and standard balance. It verifies:

- Two authenticated cities, start/build/upgrade/production/recruitment, sender ownership, foreign/invalid/occupied/unaffordable/wrong-phase/stale-turn requests, ready editing restrictions and unready.
- Malformed and excessive requests without disrupting the other client, old protocol refusal, fixed-roster late-join refusal, invalid/expired credentials and concurrent credential claims.
- Client B exits during combat; the city remains and combat advances. Client A pauses; a replacement B process gets a different ENet peer but the same city, economy, buildings, army and frozen gameplay. Replaying accepted recruitment before and after restart spends once. Pause revisions and connections remain observable; resume continues the simulation.
- A normal two-client farm-upgrade/barracks strategy wins all three waves, retains army state and refuses further turns.
- Three clients under-defend one city until it falls in actual combat. Surviving clients agree on conserved enemy IDs/remaining damage and changed destinations. The next wave allocates 18 enemies, 9 per survivor. The fallen player resumes as an observer and can pause/resume without reviving.
- An ordinary no-building strategy ends in defeat; unavailable/stopped servers, occupied ports, missing readiness and exited children produce bounded feedback.

There is no resource-grant, damage, kill-city or teleport RPC. Automated clients send normal requests through the same handlers as the UI. Network and selected economy/packed UI default to 300 seconds; other selected UI slices default to 180 seconds, full serial source UI to 600 seconds, and source/full CI to 900 seconds, with a 15-second startup deadline and endpoint/timeout overrides. Network, export-smoke and dev sessions use separate owned temporary directories and clean them on exit. Diagnostics contain public identity/state and command results, never resume tokens.

## Clean source, CI and exports

The ordered `mise run ci` pipeline passes after the final reconnect fix (about 128 seconds): locked restore, formatting, build/import, 17 rules cases, the complete cooperative network suite, template verification, Linux client/server exports and exported gameplay smoke. The smoke starts a match, buys a farm/barracks, produces food and recruits through the normal protocol. Both exports are gated by the checks; the workflow still has no upload, release, publishing or deployment step.

The first verification copy contained only tracked/nonignored source, with no build/import caches. Full CI passed there. After the lifecycle fix, the final CI ran against the updated identical source; source hashes did not change through import/export. Generated `.godot`, `bin` and `obj` directories were then removed from that owned copy and final fresh-source preparation passed again. Asset manifest SHA-256 checks and every external glTF buffer/texture dependency were checked independently. The original minimal vendored subset was approximately 1 MB; no runtime downloads are needed.

The verified client/server outputs are available in ignored `dist/client` and `dist/server`. Graphical exported clients run from their packed resources with no `--path` source argument. Dedicated-server resource stripping preserves the normal gameplay protocol; headless roles do not instantiate cameras, UI or KayKit models. Asset import and normal headless scenarios have no visual-loading errors. The deliberate occupied-port failure retains its expected engine diagnostic.

## Graphical interaction and lifecycle

Rendered source windows were driven with Godot mouse-input events through their actual controls: Start, world plot/building ray selection and bottom contextual controls, farm purchase/upgrade, barracks purchase, explicit recruitment, Ready and shared Pause/Resume. They produced normal server acknowledgments and matching resource/army/phase state in the other window. Ordinary economic commands continued longer battles. Captures were inspected for original model materials, nine indexed hex plots, resource props, distinct blue soldiers/red enemies, defender shots, automatic movement/casualties, fallen cities and terminal presentation.

Source clients retained an open frozen scene across disconnect and resumed with their session. Exported clients additionally exercised the visible Reconnect button during a paused battle and repeated reconnects in the lobby. The final lifecycle has no signal errors in those runs. This required untracking the shared RPC node through a real tree exit before replacing its transport; the view stays as a sibling and `/root/Game` is restored. This avoids the export template's RPC cache cleanup mismatch without patching the engine or changing the protocol.

The graphical source losing strategy intentionally stopped recruiting for one city, showed its elimination and the survivor's increased wave pressure, and ended at a shared defeat screen. The graphical export scenario used three original cities, let one fall, and had the survivors add a second upgraded farm and upgrade their barracks to pay for the redistributed later waves. It also checked the eliminated observer's board/outcome and viewing another player's city with editing disabled.

The physical pointer/device path on this KDE Wayland host was not independently verified: graphical widget checks used injected Godot input. These are actual rendered-widget and normal-RPC checks, distinct from headless gameplay tests. Windows/macOS graphics, network impairment, server restart recovery, account security and host migration are outside the executed POC checks; recovery is limited to the same running server.

## Launch and cleanup

The actual `mise run dev --port 17633` task was started twice. Both runs prepared the project, started one headless server and two connected graphical clients with distinct fresh session files. SIGTERM stopped only the owned children, removed their temporary sessions and released the port; a second run reached a fresh lobby instead of reusing expired credentials.

Independent `mise run server --port 17634` and `mise run client --port 17634 --session-file .sessions/role-verification.json` commands were exercised. The client bought a farm with ordinary requests, exited, and was relaunched with its exact command while the server stayed alive. Resume verification compared the public stable identity, changed peer and retained purchase. Final shutdown/session cleanup affects only those owned verification processes/files.

## Coverage map

| Contract | Evidence |
| --- | --- |
| Gold/food, nine slots, upgrades and manual soldiers | Core cases, real ENet economic actions, rendered controls |
| Three-turn cadence and three-wave outcomes | Core schedule/winning strategy, real two-client victory and losing strategy, graphical outcomes |
| Persistent damage and automatic city defense | Core timing/simultaneous damage cases and observed battles/defender shots |
| Current/future redistribution | Core 3/2 and 9/9 cases; real under-defended three-client match and graphical fallen-city/lane observations |
| Stable ownership and retry safety | Real foreign-city rejection, duplicate spending, restarted client and observer resume |
| Whole-match freeze and complete reconnect state | Core pause regression, paused ENet restart, rendered source/export reconnect |
| Requested free assets and offline imports | Vendored official subsets, licenses, checksums, dependency check, fresh import and rendered exports |
| Existing workflow and cleanup | Ordered CI, headless/export smoke, repeated dev and independent roles, owned-process/session checks |

Representative inspected captures: [source building/production](images/source-building.png), [shared defeat](images/source-defeat.png), [exported paused resume](images/export-paused-resume.png), [exported victory](images/export-victory.png), and [inspection of the fallen city](images/export-observer.png). The original exported survivors won with 100 city health and nine remaining soldiers each; the final medieval full-city run had three survivors with 100 health and seven soldiers each, while the fallen full-city player received the same victory.

## Medieval presentation baseline (before implementation)

Captured on 2026-09-30 before editing presentation code: Linux 7.2.8, Ryzen 5 5600X, Radeon RX 6800, Mesa 26.2.3, Godot 4.7.2 .NET, X11 under KDE Wayland, GL Compatibility. One graphical client and three headless clients used the ordinary protocol against a separate headless server. Each city bought a farm, upgraded it, bought a barracks, produced three times, and recruited twice before wave one. The battle was paused at tick 120: four cities, eight soldiers, sixteen enemies, two buildings per city; all cities had 100 health. No gameplay override or grants were used.

An external temporary SceneTree probe loaded the unchanged main scene and sampled frame deltas and Godot Performance monitors. VSync was disabled, FPS uncapped, and the same graphical window was resized between samples. Loading, resize and capture were excluded. Baseline captures and raw samples are in `/tmp/odot-pretty-check/baseline-*`; the probe is verification tooling, not shipped game code. Monitor values are snapshots and can lag by a second (see [Godot Performance documentation](https://docs.godotengine.org/en/stable/classes/class_performance.html)). These frame deltas measure host frame cadence, not isolated GPU time.

| Resolution | Samples | Mean / p95 frame ms | Scene nodes / resources | Draw calls / rendered objects / primitives |
| --- | ---: | --- | --- | --- |
| 1100x820 | 70,604 | 0.781 / 0.919 | 508 / 54 | 302 / 655 / 34,294 |
| 1280x720 | 12,425 | 0.790 / 0.941 | 508 / 54 | 302 / 655 / 34,294 |

Baseline scenery comprised the square slab/plots, marked lane, and existing props. The final scene must be compared with the same four-city population and rendering options.

## Medieval presentation verification

The client now uses three staggered rows of nine stable hex plots, a 34° orthographic camera, a fixed 220-unit bottom panel, and the expanded free medieval palette. The panel is about 193 physical pixels tall at 1280x720 under canvas scaling. `canvas_items` with expanding aspect uses the full wider window; the earlier baseline retained its original letterboxed aspect. Slots, balance, numerical lane positions, network data and session identities are unchanged.

Source graphical checks used the existing `click x y` command, which injects mouse events through the actual rendered controls and ordinary RPC path. The external probe converted projected/control coordinates through Godot's viewport stretch transform for resized windows (see [Viewport transforms](https://docs.godotengine.org/en/stable/classes/class_viewport.html)). Verified all nine empty centers without spending; a scenic click and panel background retained selection; farm construction, its level-two roof/upgrade, barracks construction and explicit recruitment spent the authoritative gold/food once. Ready disabled editing, Unready restored it. Combat, pause and disconnected states disabled contextual spending. Observation disabled foreign spending; city tabs cleared selection. Paused reconnect retained city, economy, buildings and soldiers, and required a new selection.

Grass, slope and alternating river joins were inspected after import; the river exits at both patch ends. An empty scene already includes wooded hills, rocks, trees, home/defender and labeled gold/food with a sack. Imported atlas/materials remain intact; a separate thin outline marks the nine plots, hover and selection. No duplicate slot-selector UI remains. All 44 manifest file hashes and glTF external dependencies were checked, with existing CC0 license coverage and pinned official sources. No runtime asset downloads occur.

The final performance comparison ran after compilation/CI activity finished, with the same rendering options and four-city population as the baseline: two buildings and two soldiers per city, sixteen enemies, paused wave one. Raw samples/captures remain in `/tmp/odot-pretty-check/final-*` and `combat-*`.

| Window | Samples | Mean / p95 frame ms | Nodes / resources | Draw calls / rendered objects / primitives |
| --- | ---: | --- | --- | --- |
| 1100x820 | 15,675 | 0.652 / 0.751 | 1,188 / 82 | 248 / 517 / 32,559 |
| 1280x720 | 15,422 | 0.663 / 0.765 | 1,188 / 82 | 276 / 545 / 33,807 |

Both exceed the 60 FPS target on this verification host. Each city has 77 terrain pieces and 14 decorative asset instances; all four patches are retained, while only the focused city and its units render. Terrain does not cast shadows; one restrained sun lights the scene. Packed scenes, imported meshes/atlas, outline meshes, immutable marker materials and normalized model bounds are reused. The four patches contain 772 scenic descendants, identical across the two samples. Node/resource population increased from the simpler baseline; visible draw calls and measured frame cadence improved. This is an uncapped desktop-host sample, not a portable hardware guarantee or isolated GPU benchmark. Physical device input and other OS graphics remain outside the executed checks.

The final `mise run ci` completed in 127.86 seconds with formatting, locked restore/build/import, all 17 core tests, the real ENet lifecycle/ownership/transfer/outcome suite, Linux client/server exports and ordinary exported build/production/recruit smoke passing. A temporary external headless-server probe recorded two scene nodes, two resources, zero render objects/draw calls, no camera, no controls and no terrain; normal headless clients were also exercised throughout the ENet suite.

A four-client ordinary economy route filled the focused city's nine plots by wave three: start with a farm and two mines, buy the barracks before wave one, upgrade the farm before wave two, add mines as income allows, and spend food explicitly. All nine bodies and roofs selected their own plots at 1100x820 and 1280x720; the actual source selection-marker positions also confirmed every slot, including repeated mine models. A second city supplied separate level-two farm, barracks and mine roof cases with observation spending disabled. All nine production turns and city switches retained the same 772 scenic descendants. Inspected full-city and combat captures at both sizes show the entire approach, home/defender and plots above the stable bottom panel.

The normal exported graphical window was verified through injected mouse events and native X11 captures, with OCR checking the displayed building type/level after every full-city roof click. It loaded from its packed resources without a source `--path`. Construction, upgrade and recruitment used actual contextual buttons and normal acknowledgments. During a paused wave-three battle, the visible Reconnect control retained all nine buildings, upgraded farm, economy and army, cleared selection, and allowed selecting the restored roof. City observation displayed the three upgraded building types. When the weak full city fell, the living attackers transferred to the surviving cities; the paused P2 capture shows its incoming enemy and the tower's shot directed along the grassy approach. The eliminated full-city player received the shared victory. The stripped server export handled the entire run and reconnect.

Updated captures include [empty landscape](images/source-empty.png), [empty lobby at 1280x720](images/source-empty-wide.png), [full city at 1280x720](images/source-building-wide.png), [wide combat](images/source-combat-wide.png), and [exported enemy transfer](images/export-transfer.png), in addition to the replaced build, reconnect, observer and victory images above.

A replaced-server check produced the expired-session refusal, exposed the visible fresh-session action, and successfully joined the new lobby. A second normal four-client run readied without building and rendered the shared defeat/fallen-roster screen. The current defeat/victory, paused/reconnecting, lobby, observation and Ready/Unready captures all use the bottom controls; earlier square/sidebar images have been replaced.

The frozen transfer snapshot placed enemy 116 (6 health) in P2 at distance 11.9333; projecting that authoritative destination put its red marker at approximately pixel (671, 174) in the inspected capture. A pixel-region check confirmed the red marker there, and P2's defender cooldown/visible shot remained consistent with that snapshot. Final source selection-marker audits covered every actual slot at both resolutions.

## Background music and settings verification

On 2026-09-30, `mise run prepare` compiled and imported the music with Godot
4.7.2 .NET. The unchanged source WAV is 39,118,090 bytes (SHA-256 recorded in
`Assets/Music/README.md`); its hash matches the supplied download. The checked-in
import settings use Detect from WAV (`edit/loop_mode=0`) and QOA
(`compress/mode=2`), with trim/normalize/downsample disabled. An external resource
probe confirmed stereo, 44,100 Hz, QOA format 3, forward loop 1, begin sample
1,548,939 and end sample 6,195,530. The imported resource is 7,900,059 bytes
(7.53 MiB), including 7,899,640 audio-data bytes. These metadata checks establish
bounds/format, independently of listening observations.

Rendered source checks used an external SceneTree probe and ordinary injected
mouse/key events. Both tabs, dropdowns, slider, numeric value and Close were
reachable at 1100×820, 1280×720, 1600×900 and 1920×1080; an 800×600 manually
resized window retained accessible settings/bottom controls and displayed a
custom size. Fullscreen reached the native 3200×1800 monitor size, disabled the
resolution selector, and returned to the retained 1280×720 window. Root window
size/mode APIs handle these changes; physical display modes are not switched.

Recovery checks restarted graphical clients with missing, malformed, individually
invalid, partially valid, oversized and valid-fullscreen configurations. Invalid
fields recovered independently; valid volume/height values survived invalid
neighboring fields. Oversized saved dimensions recovered to 1100×820. Replacing
`settings.cfg` with a directory exercised unreadable settings and failed writes:
startup used valid defaults, the new live volume continued working, and the menu
showed a save-error message. Godot itself logged the malformed ConfigFile parse
error, while startup remained successful. Preferences and resume credentials
used separate paths. Verification used an isolated `XDG_DATA_HOME` under `/tmp`.

The requested default was updated to Master 50 during implementation; fresh
startup restored 0.5 linear Master gain before autoplay. Native bus probes also
verified 0/50/100, explicit mute at zero, and an advancing music position during
mute/restore. Normal graphical shutdown was checked with verbose engine output
and exited without leaked music/playback resources after disposing the temporary
managed source-stream reference. Probe-only resource references are not the
normal client lifecycle.

Restart restored a manually resized 800×600 preference. An explicit
`--resolution 1280x720 --windowed` launch used that size; audio edits and closing
retained the saved 800×600 display preference for the next normal launch. Godot's
`OS.GetCmdlineArgs()` omits consumed display options and the hosted .NET argv is
empty, so the supported Linux target reads its own original `/proc/self/cmdline`
for override detection. Other OS override detection is unverified and needs a
platform-specific source of original arguments before those exports are offered.

An actual game-bus recording made with Godot's `AudioEffectRecord`, using a normal
seek to 138 seconds, crossed the loop end and continued near 40 seconds rather
than playing the outro/repeating the intro. The owner listened to
`/tmp/odot-settings-check/source-loop.wav` and confirmed the transition was
seamless, without a click or gap. This listening result is separate from the
imported-boundary assertions above; no custom playback scheduling was added.

A separate rootful Xwayland desktop with an actual 800×600 virtual monitor
verified the no-presets-fit case: the client chose 768×536 after decoration
allowance, offered only that custom size, and kept both the settings dialog and
bottom controls in the rendered window. The existing desktop monitor/settings
were not changed. The ordinary desktop's window manager may clamp a requested
window position into its usable area; preference restoration does not call a
position setter.

A second running graphical client retained its own bus gain and window size
while the first saved a new volume. Ordinary Esc/button/Close checks verified
opening, closing, held-key echo rejection, and dropdown-first dismissal. In a
real two-client battle, snapshots/ticks advanced in both clients with settings
open, while the number of match acknowledgments stayed unchanged. Clicking a
menu-covered world location and an underlying Farm control sent no purchase,
spent no gold, and preserved the selected plot when the dialog closed.

The final `mise run ci` passed in 126.10 seconds: locked restore, formatting, build/import,
17 core tests, real ENet lifecycle/ownership/outcome checks, client/server exports,
and the ordinary exported build/production/recruit smoke checks.

Shared pause/resume, source disconnect/reconnect, and the terminal outcome each
retained the same playing AudioStreamPlayer instance. Settings remained usable
through those transitions, and city inspection retained the music instance.
A verbose normal exit confirmed cleanup. Headless source-server and client
probes each reported zero AudioStreamPlayers, zero ClientSettings nodes and no
cached music resource, with DisplayServer `headless`; deliberately invalid
preferences remained byte-for-byte unchanged and were not parsed.

Representative captures: [source Graphics at 1280×720](images/settings-graphics.png) and
[exported Audio at 1280×720, Master 50](images/settings-audio.png). Further temporary input, recovery,
small-monitor, combat and outcome records are in `/tmp/odot-settings-check/`.

A fresh copy excluded `.godot`, bin/obj, Git, sessions and output directories.
With the original project and Downloads directory hidden in a temporary mount
namespace, `mise run prepare` completed in 7.61 seconds and new Linux client/server
exports completed in 9.28/9.34 seconds. After the final cleanup/export-filter
adjustments, fresh client/server exports completed in 10.21/8.98 seconds.
The generated QOA audio was recreated from the vendored source/import settings.
This checks preparation independently of
existing caches and original downloaded files.

The normal `mise run dev --port 17659` log confirmed one headless server and two
connected graphical clients with distinct fresh sessions. Owned SIGTERM shutdown
cleaned up its children and released the port. This check kept mise using its
installed tool directory while isolating Godot preferences with `XDG_DATA_HOME`.
Preference restoration never sets window positions.

The fresh client pack is 8,523,840 bytes and contains the 7,900,059-byte QOA
resource plus its 185-byte import mapping. The packed resource's SHA-256 is
`651e0a6adb0958c7ad122528d2a490f6d9874906cd903fb9d454d0ec5d161a1b`,
matching the fresh imported resource. The dedicated-server pack is 623,308 bytes
and contains no music entries: its export filter explicitly excludes
`Assets/Music/*`, since Godot's dedicated-server resource stripping alone retained
audio. The ordinary exported server accepted the graphical client's session.

The fresh graphical export ran without `--path`, with the original project,
Downloads, and fresh source copy hidden. Native mouse/key events opened the
top-left Settings button and both tabs, changed 1100×820 to 1280×720, selected
Fullscreen and returned to Windowed, closed/reopened through Esc, and used Close.
Restart restored 1280×720 and Master 50. A private PulseAudio null-sink capture
routed only this game's output: Master 0 produced zero-valued PCM, and restoring
50 produced audible PCM while the player continued. No microphone or other
application audio was captured.

The nested rootful Xwayland display has no window manager, so its fullscreen
request changed the menu/saved mode while retaining physical window dimensions.
Native-monitor fullscreen sizing was observed in the source desktop check above.
Godot also logged embedded-window focus/tree signal connection diagnostics during
these exported mode transitions; menu and restart checks still passed. These
backend diagnostics and native Wayland behavior remain follow-up checks.

The same normal export ran through its first and second natural authored loop
boundaries (approximately 140.49 and 245.85 seconds after playback begins).
Private game-output captures are
`/tmp/odot-settings-check/export-natural-loop-1.wav` and
`/tmp/odot-settings-check/export-natural-loop-2.wav`, each containing 6.32 seconds
of nonzero PCM. The owner listened to both and confirmed both transitions sound
seamless, without a click or gap. Together with the imported/packed boundary
checks and the source listening result, this verifies playback of the intro once
and the repeating authored section, excluding the outro. Listening observations
are distinct from metadata assertions.

All owned verification clients, server, nested display, and temporary audio sink
were shut down after the checks. Generated preferences, sessions, caches, logs,
and test tools remain outside the tracked change.

Platform/device coverage is Linux X11 on this machine, including a nested
Xwayland display. Windows, macOS, native Wayland, other audio devices, and
cross-monitor moves were not exercised. Some exported shutdowns reported one
resource still in use (and two ObjectDB instances) despite successful exit and
explicit player stop; source normal shutdown was clean in the observed run.
This remaining cleanup diagnostic is recorded separately from functional checks.

## Linux testing workflow: before-change baseline

On 2026-09-30, the unchanged clean working tree at
`c474363cfed39891f7ef5437b375dd6f77ba2c99` passed `mise run ci` on
CachyOS Linux x86_64 using the locked Godot .NET 4.7.2 and SDK 10.0.401.
Mise reported **130.17 seconds** including supervisor bootstrap, formatting,
repeated preparation, 17 xUnit cases (115 ms assertion execution), all four
sequential network groups, Linux client/server exports and headless exported
ordinary-protocol smoke. No graphical client was launched by this baseline.
The console record is in ignored `logs/test-workflow-baseline/ci.log`.

Process-log creation/last-write timestamps give approximate phase bounds:
pre-network preparation/bootstrap/rules 15.45 seconds; network peers from
12:11:45.063 to 12:13:30.583 CEST, **105.52 seconds**; subsequent template
checking/exports/exported smoke about 9.2 seconds. Approximate network group
bounds are authority/resume/victory 40.70 seconds, redistribution 33.43 seconds,
defeat 29.25 seconds and failure cases 2.14 seconds. These are filesystem-based
estimates, not instrumented phase measurements, and include owned-peer cleanup
gaps. Existing build/import and export-template caches were available; the final
fresh-source result must report that preparation difference and added UI cost
separately. This single baseline is reusable while those inputs remain unchanged;
additional complete runs are not required solely to collect benchmark samples.

### Targeted implementation checks

The owned Xvfb configuration was exercised with inherited `DISPLAY` and
`WAYLAND_DISPLAY` removed. `glxinfo -B` reported Mesa 26.2.3, llvmpipe
(LLVM 22.1.8), OpenGL 4.6 and no hardware acceleration. Source clients report
X11 and the same software renderer; assertions additionally check the actual
Dummy audio driver. This is rendering/input-state evidence, not audible
playback, native compositor or GPU-performance verification.

The selected `failure-cases` network scenario passed in 2.89 seconds, preserving
unavailable-server, occupied-port, missing-readiness and exited-child assertions
and verifying automatic bind retry without disturbing the occupied socket.
Cheap xUnit checks passed 17 core cases and 19 runner cases; runner assertion
execution took 179 ms. These include worker bounds/serial scheduling,
first-failure cancellation with awaited sibling cleanup, selection validation,
owned data/child cleanup, stale UI request ids, missing controls/captures,
unexpected engine errors, export barriers and orphaned-wrapper group cleanup.

The individually selected `settings` slice passed in 5.75 seconds (9.05 seconds
including private-display startup): actual modal input blocked a farm purchase,
actual slider/key input changed Master 50 to 51, and restarting the client
preserved its identity and preference in owned `user://` storage. Inspected
1100×820 checkpoints contain the settings controls and loaded landscape.
Evidence is in ignored `logs/20260930-110055-2eee2b0a/`.

Graphical implementation checks exposed a real WAV/playback shutdown diagnostic.
Supervised graphical quit now stops/releases the player and observes two mixer
cycles before exiting, with a bounded deadline and errors still treated as
failures. This follows the asynchronous fade/deletion behavior in
[Godot's audio server](https://github.com/godotengine/godot/blob/master/servers/audio/audio_server.cpp)
and the buffered threaded
[Dummy driver](https://github.com/godotengine/godot/blob/master/servers/audio/audio_driver_dummy.cpp).
The final settings check exited without that resource error. Display cleanup also
awaits/drains its owned process group before deleting temporary shader caches,
covering Xvfb's delayed exit after the wrapper's cleanup trap.

### Final fresh-source gate and comparison

The implemented working-tree code on top of baseline revision
`c474363cfed39891f7ef5437b375dd6f77ba2c99` passed `mise run ci` from the owned
fresh-source copy `/tmp/odot-workflow-clean-lIYgj3`, with inherited `DISPLAY` and
`WAYLAND_DISPLAY` removed. The copy initially contained no `.godot`, `bin`, `obj`
or `dist` outputs. Host NuGet/tool/template caches remained available. The sorted
source-file hash manifest has SHA-256
`0d57ab3a2747390e68ac72869fe8a15097b60151b655c17242be81a241205936`;
every copied source/lock file matched its before hash after verification. Final
documentation edits follow this run and do not change tested code.

Mise measured **112.32 seconds**, including supervisor bootstrap; the CI runner
measured **109.21 seconds**. The trace contains exactly one explicit solution
restore, build and source import. Godot's separate release publishing remains
part of each export. Evidence is retained in ignored `logs/test-workflow-after/`
(copied from the fresh run), with console output in `logs/workflow-clean-ci.log`
and source hashes in `logs/workflow-clean-before.sha256` and
`logs/workflow-clean-hashes.log`. Verified exports were copied back to `dist/`.

| Phase | Elapsed seconds | Coverage/overlap |
| --- | ---: | --- |
| Locked restore / formatting / build / source import | 1.06 / 7.18 / 1.60 / 3.82 | Serial preparation |
| Gameplay / runner xUnit commands | 1.18 / 1.32 | 17 / 19 cases, overlapping network; assertion times 128 / 207 ms |
| Complete network set | 63.00 | Two workers; all four preserved groups |
| Complete source UI set | 16.72 | Three serial slices, including display startup/cleanup |
| Client / server export | 4.46 / 3.54 | Sequential after successful source gates |
| Headless exported-role smoke | 1.01 | Ordinary build/production/recruit protocol |
| Graphical exported-package smoke | 6.55 | Includes separate private-display startup/cleanup |

The authority and redistribution groups began together and took 40.75 and
33.63 seconds. Defeat began when redistribution finished and took 29.34 seconds;
failure cases began when authority finished and took 2.87 seconds. The network
critical path therefore fell from the approximate serial baseline of 105.52
seconds to 63.00 seconds (about 40% less elapsed time). Gameplay rules and core
tests are unchanged; review of the network diff confirms every prior assertion
is retained, with added owned-error checking and automatic bind-race coverage.
No simulation acceleration was used. This is one observed boundary comparison,
not a repeated-sample benchmark: baseline network bounds came from filesystem
timestamps, and fresh preparation differs from the baseline's generated caches.

New graphical gates add 23.27 seconds including their two display lifecycles.
Despite that added coverage, measured total time was 17.85 seconds below the
130.17-second baseline. Bootstrap, formatting, imports, templates and host load
can vary; this result does not promise the same reduction on other machines.

### Expensive-slice value and observed cost

Each UI slice owns a fresh server, one graphical client and one headless observer;
settings also restarts its own graphical client. Ordinary input produces server
acknowledgments and independently observed state. All final 1100×820 screenshots
were inspected: imported landscape/materials and controls are visible, economy
shows an upgraded farm/barracks/recruit, settings shows Master 51, reconnect
restores the connected city, and the package shows its purchased farm.

| Selected slice | Risk missed by cheaper coverage | Final slice seconds |
| --- | --- | ---: |
| economy | World/control picking and input-to-authority routing; numerical/protocol checks cannot click rendered objects | 4.64 |
| reconnect | Actual recovery button and retained scene/identity; headless resume lacks the presentation/control boundary | 3.32 |
| settings | Modal click leakage and owned preference persistence through actual slider/key input and restart | 5.46 |
| exported-package | Packed UI/model/music-resource omissions and input; source checks cannot establish export completeness | 3.24 |

Slice times include peer setup/cleanup, but exclude roughly 3.1–3.3 seconds of
display lifecycle and standalone source preparation. These are observed local
costs, not deadlines. Maintenance centers on shared selectors/observations and
the normal protocol fixtures; the package slice reuses the economy helpers.
Extend these cases only for a concrete uncovered risk, rather than adding every
building, volume or display option. Prior battle/pause/transfer/outcome screenshots
and listening observations above remain historical targeted evidence. Current
recurring graphical smoke does not replay those complete matches or listen to
music; complete normal-protocol battles/outcomes remain network assertions.

### Ownership, faults and desktop distinction

Selected source slices ran independently before the final gate. Unknown options,
missing controls/captures, stale response ids, unexpected engine errors, scheduler
failure attribution/cancellation and export barriers have cheap fixture coverage.
Additional representative investigations exercised the same checked-in commands:

- An intentional Xvfb executable startup failure returned nonzero in 3.16 seconds
  with an owned-display readiness diagnostic and no desktop fallback
  (`logs/workflow-display-fault.log`).
- An intentional source-import failure returned nonzero before any scenario peers
  or network checks started (`logs/workflow-prepare-fault.log`).
- A 4-second selected authority/network deadline returned nonzero; its scenario
  cleanup completed at 4.45 seconds with several peers active. A 700-ms graphical
  deadline completed display cleanup at 0.73 seconds. Unrelated processes remained
  alive and all listed runtime directories were removed
  (`logs/workflow-network-timeout.log`, `logs/workflow-ui-timeout.log`).
- SIGTERM during selected settings with server, client and observer connected
  returned 130, removed both scenario/display runtime directories and preserved
  an unrelated process (`logs/workflow-interrupt.log`). Group cleanup fixtures
  also cover a wrapper that exits before its descendants.
- Selecting exported-package before exports existed failed clearly without
  creating packages or build/import caches (`logs/workflow-missing-package.log`).

These are diagnostic fault injections, not additional recurring game suites.
Final CI left no live owned display-group members or scenario runtime directories.
The developer's normal `settings.cfg` matched its saved SHA-256 before and after
selected settings, desktop dev and final CI; only owned preferences were edited.
Separate engine/supervisor logs identify each peer, including intentional
occupied-port errors, without logging resume credentials.

`dev` separately opened two normal `Odot - Nine Tiles (DEBUG)` windows on host
display `:0`, while the private reconnect slice completed in 3.43 seconds without
adding desktop windows. Its desktop client list remained the same two windows;
focus could move to the user's ordinary application during the run. During all
final source UI slices, a read-only X11 observer recorded the same host pointer
coordinates (3239,1426), focus (0x200000) and empty X11 client list before/after;
normal Wayland desktop applications remained running. The observer only queried
state and injected no desktop input. Test input uses Godot events inside its
private X11 connection. Those checks establish desktop isolation on this host;
physical device interaction and native compositor behavior are separate coverage.

Source and exported graphical clients reported X11, Dummy audio, loaded music
and llvmpipe with OpenGL 4.6. Both exited without the prior WAV/playback resource
error after supervised mixer draining. The authored WAV/QOA import format is
unchanged; current tests assert resource loading, gain/settings and lifecycle,
not audible quality or loop boundaries. Historical normal-close diagnostics
above are not erased by this supervised-quit verification.

For current hosted workflow commands and prerequisites, see
[Verification command details](../README.md#verification-command-details).
The hosted Actions job was not run from this workspace at this baseline.
Windows/macOS, native Wayland/GPU behavior, physical input and new listening
checks were not executed as part of this change.

## Strict C# compilation

On 2026-09-30, full `mise run ci` passed before strict enforcement changes in
110.56 seconds including supervisor bootstrap (runner: 107.58 seconds), and
after implementation in 109.32 seconds (runner: 106.53 seconds). Both used the
locked SDK 10.0.401 and Godot .NET 4.7.2 on this Linux host with available build,
import, NuGet and export-template caches. The final working tree is based on
`c49127317f575d1f8c5ae470cefd86f7620e9c5b`; only documentation/planning updates
followed the final run. This is a single before/after correctness gate, without
a performance claim or a new fresh-source/platform check.

All projects retain nullable reference checking, implicit usings and warnings
as errors, with explicit recommended SDK analyzers and build-time style
enforcement added. Findings were fixed without rule exclusions or suppressions:
invariant machine-facing numbers/timestamps, namespaced Godot types, cached
JSON serializer options, specific exceptions, matching override parameters and
checked native process-group signal results. Linux ESRCH is handled as an
already-exited owned group; other signaling errors fail explicitly.

Temporary source probes verified ordinary compilation rejects nullable returns
with CS8603, a code-quality violation with CA2201 and a mutable field eligible
for readonly with IDE0044. Formatting verification rejected a whitespace probe
without rewriting it. The probes were removed before final CI; their diagnostic
logs remain in ignored `logs/strict-csharp/`. A checked-in cheap regression
verifies runner numeric arguments under a culture with a different positive
sign. Final strict compilation reported zero warnings/errors; 17 core and
20 runner xUnit cases passed, including the existing descendant cleanup fixture.

Every existing integration gate passed: all four headless network scenarios,
the three source private-display UI slices, sequential Linux client/server
exports, ordinary-protocol exported smoke and graphical exported-package smoke.
The final network set took 62.48 seconds, source UI 16.24 seconds, exports
4.86/3.38 seconds and package smoke 0.84/6.66 seconds. Comparable baseline phases
were 62.89, 16.81, 4.65/3.54 and 1.09/6.65 seconds respectively; host/cache timing
variation applies. No expensive scenarios or dependency/tool lock changes were
added. Existing graphical/native-device coverage limitations remain.

Baseline evidence is in ignored `logs/20260930-114515-b476dd8a/`; final evidence,
including timing JSON and PNG checkpoints, is in
`logs/20260930-115317-d18ee2b9/`. Console copies are
`logs/strict-csharp/baseline-ci.log` and `logs/strict-csharp/final-ci.log`.
The OpenSpec sync records the strict contract in `linux-test-execution`;
specification and strict change validation pass.

## Launcher and Steam integration baseline

Before integration on 2026-09-30, the clean settings/strict-C# baseline passed
`mise run ci` in 110.05 seconds (runner: 106.81 seconds). Evidence is in
`logs/20260930-120146-8f7918d7/`; console output was captured in
`/tmp/odot-steam-baseline-ci.log`. All 17 core and 20 runner cases, four network
scenarios, three source UI slices, both Linux exports and package checks passed.
This includes the settings modal/input/restart check; existing native-device and
listening limitations above still apply.

The launcher will reuse `ClientSettings.Initialize`, `Open`, `IsOpen`, and
`BlocksWorldHover`, its Graphics/Audio controls and `user://settings.cfg`.
Preferences load and Master gain applies before the tabletop starts its single
`BackgroundMusic` player. First-launch volume remains 50, zero mutes the Master
bus while playback continues, and window overrides are not saved as preferences
unless edited. Dialog close currently restores the tabletop Settings button;
moving to application lifetime must restore an available current-screen control.
The existing WAV/import, authored intro/loop and -12 dB music gain stay intact.
Supervised shutdown stops/releases audio and drains the mixer within its bounded
deadline. Application transitions must preserve that lifecycle contract.

### Steam dependency, import diagnosis and packaging

The Linux x86_64 extension files are pinned in
`src/Game/addons/godotsteam/manifest.json`, with the archive and per-file hashes,
upstream MIT notice and a separate Valve-runtime attribution. The upstream
non-prerelease release and publisher unstable designation are both retained in
`README.odot.md`. The descriptor keeps the official entry point and Linux paths;
automatic SDK initialization is disabled. Small C# native helpers serve the
compatibility probe and application integration.

Checked-in diagnostic commands are:

```sh
mise run check-steam-extension --offline
mise run check-steam-extension
mise run export-client
mise run check-steam-extension --exported --offline
mise run check-steam-extension --exported
```

Source checks prepare and perform three additional editor imports, recreating
only the derived extension startup list each time with an owned fresh global
documentation cache. Asset imports stay cached. Exported
checks use the existing client package copied outside the source tree. `--offline`
loads the native classes without SDK initialization. Online checks require a
running, signed-in Steam client with access to the configured application;
development defaults to 480 and accepts `ODOT_STEAM_APP_ID`. They check native
host/channel configuration, C# casting/assignment, local signal marshaling,
callback polling and close/disposal/shutdown. The locally emitted diagnostic
signal creates no Steam lobby/invitation and is not remote callback evidence.
SDK diagnostic account identifiers are redacted before runner log retention.
These commands establish single-account compatibility only, never remote channel,
overlay or relay acceptance.

`export-client --steam-app-id ID` writes an explicit package `steam-app.cfg`;
default development packages use 480 without an AppID file. `export-client
--production --steam-app-id ID` selects a separate production-feature preset and
requires a positive non-480 ID before export begins. Its Steam initialization
reads the explicit package configuration instead of the development environment
override/default. Both presets exclude source AppID/config files and retain
licenses alongside native libraries. Cheap tests cover missing/480/zero package
IDs. An isolated production packaging fixture with non-480 ID 123456 passed;
that arbitrary fixture tests configuration/exclusion only, with no SDK
initialization or application ownership claim. Production Steam runtime and
genuine launch registration remain unexecuted.

On 2026-09-30 the first repository `check-steam-extension --offline` native
import exited **139/SIGSEGV**, after editor initialization/script discovery. Its
record is `logs/20260930-121259-861e272a/import.json`, with the engine output and
`import-backtrace.log` alongside it. The owned failed process was PID 205627;
the system retained its core. Subsequent exact-source/core mapping identified
Godot's deferred `EditorHelp::_gen_extensions_docs` accessing cleared document
data during shutdown. This also reproduces without C# or SDK initialization.
Upstream reports include [Godot #111048](https://github.com/godotengine/godot/issues/111048)
and [#111645](https://github.com/godotengine/godot/issues/111645).

The runner now seeds Godot's ordinary generated `.godot/extension_list.cfg`
before editor import/export and uses a fresh owned `XDG_CACHE_HOME` each time.
Both measures avoid the diagnosed deferred documentation path. It uses stock
Godot with no editor plugin, fixed delay, custom build or automatic retry.
Diagnosis is retained in `logs/steam-import-diagnosis/`, independent counterfactual
summaries in `logs/odot-import-repro-k5n9_ahl/` and
`logs/odot-import-fix-check-ko1oma5o/`, and final results in `logs/steam-import-fix/`.

A subsequent full `mise run ci` using the populated import cache passed in
108.56 seconds (runner: 106.57 seconds), with zero compilation warnings/errors,
17 core and 24 runner tests, all existing network/source-UI/package gates, and
new source/isolated-export offline native-load probes. Evidence is in
`logs/20260930-121604-1c33fce0/`; console records are copied into
`logs/steam-integration/`. The exported release library and Valve runtime matched
their pinned hashes and the license was present. The later successful import is
one subsequent result, **not** proof of a resolved initial-import crash or
fresh-source reliability. The later checks below supersede the single-account
lifecycle limitation, while real account-to-account delivery, invitations and
relay routes remain pending.

A final isolated fresh-source CI run passed in **111.75 seconds** (runner), with
17 core and 26 runner tests, all network/source-UI/export/package gates and
source/export offline native-load probes. Evidence is
`logs/steam-import-fix/20260930-124911-c087e29d/`; `tested-source.json` records
source hashes. An earlier attempt ran out of space in owned `/tmp` fixtures;
only those fixtures were moved to ignored cache before rerunning. This result,
fresh-source preparation, repeated extension discovery imports and single-account
online source/export SDK lifecycle checks establish observed import-fix coverage.
Online probes verify initialization on 480, typed local signal marshaling,
callbacks, native host assignment, channel 1 configuration, close/disposal and
shutdown; they create no lobby or invitation. Retained package guard/exclusion/
fixture logs are in `logs/steam-import-fix/packaging/`.

### External Steam prerequisites and deferred acceptance

Initial development uses **AppID 480 (Spacewar)**; registering or purchasing a
production AppID is not required. Each friend needs a running Steam client,
a distinct signed-in authorized account with access to 480, and a compatible
Linux x86_64 build with the pinned extension/runtime. Two separate machines are
required for real peer checks; different NAT networks are required for relay
acceptance. One machine/account can run the compatibility probe but cannot prove
remote reliable channels, authenticated remote identity, invitations or relay.

The user explicitly excluded testing with friends from this milestone and
authorized its local completion, spec sync and archive on 2026-09-30. Local core,
ENet, graphical and export checks pass without Steam. Unexecuted real-Steam
checks remain unchecked in the archived OpenSpec checklist;
missing prerequisites are never recorded as a pass. `mise run dev` remains the
playable desktop host-and-guest command, with no Steam login dependency.

Steam genuinely cold-launching this game's executable requires the game's own
configured AppID, correct Steam launch registration, and application access for
both accounts. Spacewar 480 may launch its own executable; manually passing
`+connect_lobby` exercises argument routing only. The non-480 packaging fixture
does not satisfy own-AppID launch/runtime acceptance. The upstream publisher's
**unstable** designation remains an independent release qualification until
the outstanding acceptance checks are observed.

### Verification layers for application sessions

1. `mise run test` exercises shared authority, start/admission policy, action
   validation, retries, identity/namespace boundaries and delayed Steam operation
   guards without Godot, display or Steam accounts.
2. `mise run test-network` uses real headless Godot processes. Existing outcome
   cases remain, alongside selectable `solo-session` and `playing-host-lifecycle`.
   Solo preserves an occupied endpoint while starting/building/restarting without
   a socket. Playing-host checks cover local/guest policy, retained paused city,
   retry after process restart, snapshot/ack progress, host end and old credentials.
   `redistribution` uses a playing host and retained eliminated guest; dedicated
   outcome, ownership, failure and resume cases remain in the suite.
3. `mise run test-ui --scenario launcher` uses real pointer/keyboard input and
   read-only geometry on owned Xvfb with Mesa/Dummy audio. It checks menu, settings,
   solo, music continuity, fresh sessions, hosted controls and actual termination.
   `exported-package` repeats representative application/gameplay transitions from
   packed Linux resources. Native-close requests target the exact owned child PID
   and observed main X11 window, never the developer's desktop.
4. `mise run test-steam --role host|guest [--lobby ID] [--exported]` runs one side
   of a real pair on separate signed-in machines. A normal desktop is required for
   the Steam overlay; this is separate from ordinary CI. The command exercises
   ordinary purchases/readiness/combat, channel-0 acknowledgments during snapshots
   and a paused checkpoint. Native connection evidence uses whitelisted
   `getConnectionInfo` state/authentication/relay flags/relay POP, never raw IP or
   identity dictionaries. Compare both records' match and paused tick/revision.

Compare gameplay at a common revision or after ordinary authoritative pause.
Two moving clients' latest snapshots can legitimately differ. Connection/readiness
changes are normalized only for retained-gameplay comparisons, never for admission
or pause assertions. No resource grants, forced outcomes, alternate peer or mock
Steam connection counts as real transport evidence.

No transport fault interceptor is needed: cheap shared-ledger tests and real
request replay after guest process restart prove a lost acknowledgment cannot
double-charge. Old-match high-sequence requests and canceled callback operations
exercise fresh-session isolation, alongside actual disconnect/restart tests.
The recurring harness keeps one ordered driver per child and bounded waits.

Absent Steam prerequisites produce a nonzero result explicitly marked
`unexecuted`. A no-display prerequisite check was verified at
`logs/20260930-134244-3c90e4f7/`; it starts no SDK, lobby or gameplay process.
Missing graphical prerequisites also report `unexecuted` with a nonzero exit,
without desktop fallback. An available display that fails rendering or assertions
reports `failed`. Successful local
CI never closes the external Steam gates. See [session ownership](sessions.md).


### Final local launcher/session verification

On 2026-09-30, fresh isolated source `mise run ci` passed in **148.29 seconds**
(runner; mise total 151.93 seconds). The snapshot excluded generated import,
build, export and session state. Its hashes and code comparison are retained in
`logs/sessions-final-ci/tested-source.json` and `source-comparison.json`.
Full evidence is `logs/sessions-final-ci/20260930-142111-40a095c1/`:

- 50 core and 35 runner xUnit cases passed, with zero compilation warnings/errors.
- All six real-process network scenarios passed in 62.39 seconds, retaining
  dedicated outcomes/failures and adding solo and playing-host lifetime checks.
- All four source UI slices passed in 34.76 seconds; launcher took 17.28 seconds.
- Ordinary Linux client/server exports, pinned native files/notices, offline
  extension loading, exported gameplay/reconnect and graphical package checks
  passed. Exported UI took 24.23 seconds including private-display ownership.

Earlier integrated CI also passed in 142.28 seconds at
`logs/20260930-140223-313eb9d9/`. Stricter graphical guest coverage then exposed
an orderly-close defect: immediately closing the ENet peer discarded the queued
reliable session-end message. Logical teardown now happens immediately while the
old transport drains until guests disconnect or the existing deadline expires.
A fresh session waits for that transport; Exit waits before SDK/audio disposal.
The final process tests require `session-ended` on orderly leave and separately
exercise abrupt host death.

Native close with Settings open initially failed because Godot's exclusive
child window suppresses the parent's close event. Settings and invitation dialogs
now use nonexclusive embedded windows with a root input guard. Actual pointer/Tab
checks prove world input stays blocked, while a native parent close works.
The final source and exported hosted UI checks open Settings in both windows,
close the host's exact owned native window, require guest return to a usable menu
with Settings dismissed, preserve music/preferences and explanatory host feedback,
then use the guest's visible Exit button. This extends the earlier test that
accepted ordinary transport loss and used a headless guest.

After the drain fix, actual `mise run dev` was checked again on an owned private
X11 desktop: two mapped host/guest windows, admitted guest, native host close,
guest `session-ended` and menu, mise exit zero, and all owned descendants/display
processes/runtime removed. Evidence is
`logs/odot-session-work-6xvk322k/dev-drain-smoke/` (11.52 seconds overall).
This command opens ordinary desktop windows for users; its diagnostic private
display does not change the default command behavior or use their preferences.

A missing-tools `check-ui-prerequisites` invocation exited nonzero and reported
`unexecuted` before starting any display/gameplay child. Evidence is
`logs/sessions-final-ci/20260930-142427-11c62bb3/`. Missing Steam/normal desktop
was likewise recorded unexecuted above. Available displays with rendering or
feature failures still report failed; neither case becomes a pass.

Before the final scope revision, the original checklist had **31/41 tasks complete** locally. Task 1.6, Steam tasks 5.2–5.4 and 5.6,
and all phase-7 acceptance tasks remain unchecked. Shared identity/resume policies
and canceled/late callback guards are tested locally, but SDK-authenticated remote
binding, real warm invitations, native channels, relay/NAT behavior, actual Steam
host departures, own-AppID cold launch and release stability require the deferred
external evidence. No real lobby or invitation was sent during local checks.


### Linux invitation overlay launch fix

The first real `mise run play` observation initialized AppID 480 and created a
hosted lobby, but neither Invite friends nor Shift+Tab opened the overlay.
Read-only inspection of that game process found Steam's client library mapped
and no `gameoverlayrenderer.so`. This distinguishes lobby/SDK availability from
an injected graphical overlay. No game input, desktop setting or process was
changed during diagnosis.

[Valve's Linux FAQ](https://partner.steamgames.com/doc/store/application/platforms/linux#4)
requires preloading the overlay renderer when launching outside Steam.
`SteamOverlayLaunch` now resolves a readable ELF64 x86-64 shared library from
known native Steam client roots and merges it with existing `LD_PRELOAD` for the
game child only. It applies to ordinary `play` and source/exported paired
`test-steam` menu launches; local/headless/private-display/probe/import/build
processes remain unaffected. No library is downloaded or installed. Missing
native Steam libraries produce launcher feedback and leave local play available.

The game uses the pinned extension's `isOverlayEnabled` and `overlay_toggled`
APIs; both signatures were checked against
[the selected release source](https://codeberg.org/godotsteam/godotsteam/src/tag/v4.22.1-gde/godotsteam.cpp).
Invite reports unavailable/loading overlay state, tracks activation after a
request with a five-second bound, and clears pending feedback on session leave.
An activation callback reports an active overlay, not proof that an invitation
was sent or received. Offline native-load probes check the real extension's
local typed overlay signal marshaling without opening Steam UI or initializing
the SDK. Online compatibility probes additionally query overlay availability.

Cheap verification passes 50 core and 51 runner tests, including 16 new cases
for launch eligibility, preload preservation, missing libraries and incompatible
ELF headers. Isolated evidence is
`logs/odot-session-work-6xvk322k/overlay/`; root tests and subsequent CI records
are retained in `logs/steam-overlay-fix/`. The installed native renderer was
found and validated. Real overlay visibility requires restarting the prior
unpreloaded process; a local marshaling test or injected library alone does not
close the pending real invitation/Steam acceptance tasks.

Full `mise run ci` after the overlay fix passed in **145.34 runner seconds**
(mise: 146.64 seconds), with all 101 unit tests, six network scenarios, source UI,
Linux exports, source/export offline typed overlay callbacks and package UI.
Evidence is `logs/20260930-144650-de55471d/`. Local dev launch eligibility is
covered by the cheap tests; its previously verified playable host/guest behavior
is preserved. No real overlay activation or invitation is claimed by this run.

### Steam login presentation and remaining local policy checks

On 2026-09-30, task **5.5** completed with authenticated playing-host policy
coverage for paused/eliminated retained-city resume, preserved retry ledgers,
wrong-account/token refusal, fresh-player rejection after roster lock, and old
credentials/commands rejected by a replacement match at the same original host.
These tests supply trusted identity at the transport boundary. Actual native SDK
peer identity mapping remains in unchecked task 5.4.

Pending consent now expires independently of create/join operations when its
session generation changes, dismissing the native dialog. Old accept/decline
delegates cannot consume a replacement invitation for the same lobby. The added
cheap cases preserve current-generation consent and verify future offers remain
usable. Isolated core evidence: `logs/steam-local-policy-validation/verification.md`.

The existing launcher slice now exercises actual native invitation controls with
Steam disabled: duplicate consent preserves the original, outside input stays
blocked, Decline retains the match/selection, and Accept returns through ordinary
session cleanup with music/preferences retained. Only the incoming presentation
boundary is triggered by an owned, supervised offline-X11 command; no SDK/lobby,
foreign match, forced gameplay outcome, or direct decision callback is used.
This closes local portions of tasks 5.3/5.6 without checking their real-Steam
acceptance requirements. It adds about 0.7 seconds to the existing launcher,
without another child or battle. Source and exported routes retain screenshots.

The menu and Multiplayer view show a bottom Steam persona/login label. Owned
offline UI checks at both sizes verify "Open Steam to log in", clickable Host
warning, no substitute session, and usable Back/solo/Settings/Exit. Native method
availability is checked in offline source/export probes. A separate single-account
`mise run check-steam-extension --exported` passed in 0.50 runner seconds at
`logs/20260930-152708-3d3b2712/`: current login and real persona lookup succeeded
without printing the name or creating a lobby/invitation. This is API compatibility
evidence, not friend/relay acceptance. Login checks also cover already-initialized
SDKs; review corrected offline-to-online callback subscription and rechecks before
consent leaves a current match. Host remains usable while a previous peer drains.

Full `mise run ci` passed in **145.92 runner seconds** (mise: 147.25 seconds),
with **55 core + 51 runner tests**, all six network scenarios (62.39 seconds),
all four source UI slices (34.36 seconds; launcher 18.02 seconds), Linux
client/server exports, native offline probes, package smoke and exported UI
(25.15 seconds). Evidence: `logs/20260930-152422-64802053/`. The earlier failed
attempt caught a static-member analyzer requirement during compilation; it was
corrected before the passing build and full suite. `mise run dev` retains its
Steam-independent playing host and guest route, exercised by existing process
and graphical hosted regression checks.

Original checklist progress is **31/41**. Tasks 1.6, 5.2–5.4, 5.6 and all phase-7
items retain their unverified real-Steam, own-AppID, or release-qualification
evidence requirements as deferred checks outside the closed local milestone. No friend
invitation, remote Steam admission, real departure, or relay route was verified
by these local checks.

### Local milestone closure and archive decision

On 2026-09-30 the user explicitly decided not to test with friends for this
change and requested updating, syncing and archiving all locally executable work.
The completed development milestone uses the passing core/ENet/source UI/Linux
export gates and single-account compatibility evidence recorded above. No game
code changed during closure; the same passing CI inputs remain applicable.

The original ten unchecked external acceptance items are retained in
`openspec/changes/archive/2026-09-30-add-start-screen-and-steam-hosting/tasks.md`
as deferred audit entries. They do not block this user-authorized local archive.
The synced specs retain the intended online behavior and distinguish local
milestone completion from actual Steam or release acceptance. Friend invitations,
remote authenticated identity/channels, relay/NAT routing, real Steam departures
and owner reassignment, own-AppID cold launch, and release stability remain
**unverified**. The existing paired runner can be used later if requested.
## Versioned distribution verification

On 2026-09-30 the distribution implementation added SemVer-tagged Linux and
Windows clients, a per-user Inno Setup installer, a versioned Linux install
script, installed-package runner slices, manual GitHub release discovery and a
browser-assisted full-download action. After a locked restore and formatting,
the suites pass 55 gameplay and 105 runner tests. The runner cases include
isolated Linux install/reinstall/upgrade/failure/uninstall fixtures, native
overlay launch policy, bounded deterministic HTTP fixtures and final release
asset allowlist/hash/identity failures. No live GitHub release or Steam account
is needed for those cases.

`mise run test-ui --scenario settings` passed in 14.49 runner seconds at
`logs/20260930-183338-a843d1da/`. It retained the existing modal/gameplay and
preference assertions while verifying the About controls and accurate
development-build update feedback on owned X11/Mesa software rendering.

The ordinary `Verify and build` workflow runs only for pull requests targeting
`main` and pushes to `main`. It runs the full source checks, Linux packaging
and native Windows package checks in parallel on separate runners; all three
jobs must pass. It has no upload permission. Native Windows
installer qualification remains accepted as deferred.
The separate `Build published release` workflow follows the owner's final
preference to skip all gameplay, network, UI and installation tests: it performs
lightweight public/tag/profile preflight, builds both tagged packages in
parallel, and attaches only the installer, archive, install script, combined
checksums and public build metadata after identity/hash consistency checks.
Selected `verify-installed-linux` and `verify-installed-windows` commands remain
available for deliberate package qualification without implicit rebuilds.

The release workflow has not published anything. The repository was still
private during implementation, and no real tag or release was created. Windows
installer, graphical interaction and genuine paired Steam
invitation/relay/cold-launch acceptance remain unclaimed.

The pinned-action nonpublishing run
https://github.com/seraph1nia/odot-game/actions/runs/36763314446 passed its source
gate and complete tagged Linux path: package build, ordinary exported checks,
installed graphical solo checks and owned uninstall cleanup. Its Windows export
and build identity passed, but ISCC printed its usage and produced no installer.
The owner accepted Windows installer qualification as deferred. The ordinary
CI workflow retains its native Windows export/build checks. Release workflow
work continues separately.


## Arch animated combat acceptance (2026-09-30)

This change retains Godot, the current authority roles, six network scenarios and
all existing cooperative assertions. It adds Arch 2.1.0 state ownership, typed
recruitment and rigged C# presentation. Protocol v4 refuses older builds.
The change’s detailed evidence ledger is
[verification.md](../openspec/changes/archive/2026-10-01-add-arch-animated-unit-combat/verification.md).

Final `mise run ci` passed in 183.78s (185.12s including runner preparation),
with evidence under `logs/20260930-205517-0bd9989c/`. It includes 80 core and
107 runner tests, all six network scenarios, all five source UI slices,
sequential Linux client/server exports and headless/graphical package smoke.
Source UI measured 60.89s including private-display startup; the combat case
measured 13.74s. Packed graphical smoke measured 31.88s, or 35.18s including
private-display startup. Locked restore, strict build and format verification
passed before source checks; all source gates passed before exports.

`mise run test-ui --scenario combat` is the fifth default source UI slice. Its
admitted risk is missing/wrong imported rigs, non-moving bones, clock drift,
visible contact, duplicated cosmetic effects, corpse leaks and stale session
views: cheap numerical tests and headless peers cannot inspect those Godot nodes.
It owns a dedicated authority, one graphical client and one headless observer,
recruits one Swordsman and one Crossbowman through actual controls, and samples
only the first wave. Setup uses ordinary farm/upgrade/barracks/ready commands.
A 60-second case deadline bounds observable waits. Expected preparation plus
execution was 20–40 seconds; selected execution measured 13.76s, or 23.45s with
shared source preparation/private-display startup. Maintaining it adds one
scenario, clip bindings and read-only pose/event observations, not a battle matrix.

The existing reconnect slice now recruits the same small army and pauses an
in-progress attack before transport recovery: selected execution measured 11.06s.
Packed smoke validates both rigs, seven required clips, materials and three
weapons, then samples short locomotion/shooting without repeating a complete
match. Economy and launcher still assert ordinary input, including both
recruitment controls at 1100×820 and 1280×720. Package-only selection consumes
existing exports and does not prepare/rebuild source.

Cheap coverage owns exact attack ticks, mutual deaths, invalid target/source,
shooting range/close contact, upgraded costs and atomic rejection, 32-vs-32 pure
and mixed contact/determinism/progress, entry queues/transfers, wave strategy,
registry disposal, 120-tick/4096-event retention and playback baselines/gaps.
The history stress fixture uses zero damage and a deliberately fast three-tick
cadence; its payload bound is not a claim of bandwidth performance at that scale.

Evidence remains under ignored `logs/<run-id>/`: non-secret owned engine logs,
timing JSON, PNGs and fresh UI observation IDs. Scopes await peers and private
displays before removing client data, and preserve other processes/preferences.
These Linux x86_64 checks use owned X11, llvmpipe and Dummy audio. They establish
software-rendered skeleton/control behavior, not native compositor/GPU cadence,
physical input, audible quality, Windows animation/export execution or real
paired Steam/overlay/relay acceptance. Those native checks remain unexecuted for
this change; previously recorded Windows qualification applies to earlier work.

## Direct Steam friends picker verification

The `add-direct-steam-friend-invites` change replaces overlay recipient selection
with an Odot friends dialog and the pinned native lobby invitation API. Existing
overlay qualification above is historical evidence, not a prerequisite for this
picker. The host selects a Steam account; an accepted send request is distinct
from invitation receipt, guest consent, native admission and relay routing.

`mise run test-ui --scenario launcher` extends its existing menu/host checks with
owned Steam-disabled fixtures at 1100×820 and 1280×720: duplicate names, a large
scrolling list, actual selection, sent/rejected outcomes, empty/unavailable
refresh, modal input, focus, incoming consent, session replacement and native
exit. Cheap invitation-policy tests cover account identity, generation/lobby
validation, removed friends and duplicate/reentrant sends. Fixtures cannot create
a real lobby, send a real invitation or initialize Steam.

Separate real acceptance uses `mise run test-steam --role guest|host --scenario
direct-invite [--exported] --timeout-ms 600000` on two Linux machines/accounts.
Start the guest first and keep both compatible AppID 480 builds running. The host
manually selects the agreed friend in Odot; the guest accepts in Steam. The
selected case requires the accepted send event and actual warm join callback,
then retains the existing ordinary shared gameplay/native connection checks.
`--lobby` is refused for this case so argument routing cannot count as delivery.
No ordinary CI command sends live invitations. Real paired acceptance remains
unexecuted for this change until two-account evidence is captured. Native GPU,
physical input, listening quality, Windows runtime and production cold launch
remain outside local software-rendered fixture evidence.

See [change verification](../openspec/changes/archive/2026-10-01-add-direct-steam-friend-invites/verification.md)
for retained before/after records and measured UI cost.


## Village character and strategy acceptance (2026-10-01)

The archived change's [verification record](../openspec/changes/archive/2026-10-01-add-village-character-and-strategy/verification.md) records the full before baseline and final coverage. This change adds catalog-driven gold/food/wood spending and nine building choices, income-free Preparation, symmetric four-role factions, class research in hundredths, splash/towers and bounded presentation. Cheap coverage owns costs, rank arithmetic/retry/no-heal, role/building parity, preparation/disconnects, contact and finite ordinary strategy families. It avoids a graphical option matrix.

Existing graphical cases were extended for concrete rendering risks. Economy exercises ordinary Lumbermill/Blacksmith/research/preparation actions, all nine terrace plots, a structural Catapult upgrade and actual pile node counts; these could fail while numerical snapshots stay correct. Combat uses only first-wave Mage/Berserker plus an observed tower city to expose missing cast/axe rigs, projectile alignment, contact interpolation, audio/effect pool bounds and frozen ambient/death clocks. Reconnect baselines living views/piles/effects without historical cues. Settings checks synthesized cues' Master routing/mute and restores its owned chosen preference. Packed smoke loads every specialist rig/socket/role clip/weapon, a real Catapult and resource piles, then briefly samples tower/locomotion/shooting. Its standalone selection still consumes existing exports without source preparation or implicit rebuild.

No new network/UI scenario was added: existing names, one ordered child driver, fresh probe IDs, ordinary protocol assertions, owned X11/XDG/endpoints, frame captures and awaited cleanup remain. Iteration failures retained owned evidence and cleaned up. The interpolation regression also gained a cheap test: unsafe contact chords hold one common prior frame instead of overlapping bodies. A stable 270px HUD reservation prevents camera target shifts when contextual controls change.

Selected execution costs: economy 21.57s (old 4.64s, +16.93s), combat 13.57s (previous animated-combat 13.76s), settings 7.35s. Preparation/display totals are recorded separately: economy 32.09s, combat 23.66s, settings 15.63s. Thus the additional specialist/tower assertion path replaces the old combat setup rather than repeating a full graphical battle. Core strategy logs record resource ledgers, recruits/casualties, health and per-wave steps; they prove these finite winning candidates, not exhaustive balance. Starting wood changed from seed 20 to 30 to retain the upgraded Farm/Barracks opening; remaining shared profiles stay symmetric.

All KayKit additions come from immutable official free-pack sources, with licenses, hashes and extracted-texture relationships in the asset manifest. Resource food uses medieval sacks rather than paid Resource Bits food. Sounds are original synthesized PCM (22,050Hz mono/16-bit, 0.12s cubic envelope and fixed-seed noise) and are instantiated only by graphical presentation. Dummy audio and software OpenGL assertions establish routing, lifecycle and mute, not physical listening quality, native compositor/GPU performance or Steam account acceptance. No dependencies/tools were upgraded and no package was published.

Final full `mise run ci` passed in 220.54s, evidence `logs/20261001-071039-07264336`: 133 core tests, 107 runner tests, all six network scenarios (43.54s), all five source UI slices (92.21s), sequential Linux client/server exports, headless package smoke (1.09s) and graphical exported-package smoke (49.64s; 52.93s with display overhead). Source launcher now recruits at the appropriate buildings using ordinary production and retains its invitation/lifecycle assertions. Frame capture samples an 80-by-80 grid so modal text and narrow geometry are represented while retaining the existing minimum-colour/file/dimension checks. Strict change validation and all 155 asset hashes pass; documentation frames come from this final CI run.

## Trio UI and overhead health verification

`adopt-trio-ui-kit` uses the existing launcher, settings, economy, reconnect and combat slices. No new scenario or full graphical battle is added. Launcher/friends/join checkpoints report actual kit style paths and full control bounds at 1100×820 and 1280×720, retaining keyboard/modal/session/exit assertions. Settings retains native dropdown, audio and owned preference checks with kit tabs/dialog/slider styling. Economy verifies real gold/food icon assignments, explicit wood text, readable resource costs and world plots above the stable bottom HUD.

Combat observations expose each live overhead bar's sampled current/effective maximum health, fraction, fill width, screen bounds, shared texture paths and input-transparent state. Existing first-wave sampling covers both factions, full/damaged bars, shared pause, casualty hiding, fresh-session cleanup and actual 1280×720 reprojection while paused. The native dropdown driver observes the first two Down selections before Enter because pointer-opened popups initially have no keyboard-focused item. Reconnect checks bars reconstructed from current living state without duplicate or historical damage playback. Package smoke uses the same assertions so source-only texture references cannot hide export omissions. Cheap tests cover wounded ranked fractions and clamping/invalid maximums; gameplay health values and wire schemas are unchanged.

The free Cozy subset, original terms, user-confirmed repository permission and derived-button/bar operations are recorded in `src/Game/Assets/TrioUI/README.md` and `manifest.json`. No runtime downloads, paid assets or tool upgrades are required. Bounded bars share textures and use camera projection without per-unit render viewports. The HUD reservation is now 300px to accommodate textured borders and costs. Frame inspection checks border stretch, icon semantics, contrast and crowding; software X11/Dummy audio evidence does not establish native GPU/compositor, physical input or listening quality.

Full after-CI passed in 224.90s, `logs/20261001-180607-c286409c/ci-summary.json`: 140 core / 108 runner tests, six network scenarios (43.61s), five source UI slices (98.91s), sequential Linux client/server exports, headless package smoke (1.09s) and graphical package smoke (50.02s; 53.33s including display overhead). Actual packed textures/provenance/bar fractions passed, and final exported menu/combat frames were inspected. Detailed iteration evidence and measured slice costs are retained in `openspec/changes/archive/2026-10-01-adopt-trio-ui-kit/verification.md`.

## Tabletop camera navigation

The camera uses the existing orthographic fit as its base. Wheel input anchors zoom to the reference ground plane beneath the cursor, with travel bounds taking priority at the edge. Held physical WASD/arrows pan in the camera's ground frame, with normalized diagonals and real frame delta independent of battle playback. Reset view restores all plots/full approach above the HUD; city/fresh-match transitions reset local adjustments, while resize and same-match reconnection retain them. Camera state is local, temporary presentation.

Existing economy/settings/reconnect/combat/package slices are extended for projection and input regressions rather than adding a separate full battle. Supervised wheel and held-key events go through ordinary Godot input; helpers release held keys in finally. Window focus checks validate the observed child PID and touch only that child or a temporary owned focus-recipient window on the private X11 display. Modal, text/dropdown and HUD input retain priority. Bar health/world anchors freeze with paused playback while screen placement follows local camera movement. See [the change verification record](../openspec/changes/add-tabletop-camera-controls/verification.md) for before/after results, measured cost and captured frames.

Full before/after CI passed in 227.54s / 244.03s, with final evidence in `logs/20261001-184547-db4a4966/ci-summary.json`: 140 core / 108 runner tests, all six network scenarios, five source UI slices, sequential Linux exports, headless smoke and graphical packed smoke. Final economy/reconnect/settings/combat slice times were 29.28s / 12.91s / 12.17s / 17.19s (before: 22.40s / 10.77s / 8.55s / 15.23s); package smoke was 51.70s (before: 51.04s). Source UI including display grew by 12.16s; timings are individual runs, not native performance measurements. Source resized/paused and packed navigation frames were inspected. Native GPU/compositor performance, physical input, listening quality, Windows runtime and paired Steam acceptance remain outside this local verification.


## Continuous countryside and supporting hexes

`fill-countryside-and-align-assets` reuses economy, launcher and exported-package cases. Economy observes instantiated footprint contacts on the three terrace levels, tower/base deck attachment, every plot's selection, foreign-city roof picking and complete coverage at overview, resize and camera limits. Launcher compares actual static positions/scales, central terrain transforms/rotations, nine plot outlines and bridge placement between menu and fresh solo; return after construction retains the passive starting landscape without match objects. Packed checks reuse these assertions and existing setup. No additional battle or option matrix is added.

Five cheap runner tests cover bounded camera movement as well as rejecting undersized scenery ranges, invalid view points, sunken/floating/off-center contacts and mismatched menu placements/terrain/scales. Graphical observations read installed mesh transforms and ground footprints; PNG inspection complements geometry checks for joins, contact and unobstructed sightlines. Godot multimeshes batch repeated terrain into 8-by-8 regions for culling, using cached imported meshes/materials; coverage only grows when the viewport/envelope needs more rings and never determines overview scale. Headless roles still instantiate no scenery.

Before CI passed in 246.02s, evidence `logs/20261001-185736-043325d8`. Iteration evidence and final measured slice costs are recorded in [the change verification record](../openspec/changes/archive/2026-10-01-fill-countryside-and-align-assets/verification.md). All graphical checks use owned Linux X11, software OpenGL and Dummy audio; they establish rendered input/projection/lifecycle behavior, not native GPU/compositor performance, physical input, audible quality, Windows execution or paired Steam acceptance.

Full final `mise run ci --timeout-ms 300000` passed in 395.00s (`logs/20261001-194033-eb0b391f/ci-summary.json`): 140 core / 113 runner tests, all six network cases, all five source UI slices, sequential Linux exports and both package smokes. Source UI took 216.32s (before 113.31s), and graphical packed smoke took 100.48s (before 52.80s). The default 180-second source suite deadline was exceeded; the existing five-minute override retains all coverage. Individual economy/launcher costs grew from 29.57/37.86s to 59.21/75.29s. Filled pixels increase software-rendering cost even with batching; these runs do not measure native performance. Final source and packed menu/solo/navigation frames were inspected. Shared observations contain 18 static placements/83 core transforms, with 952 menu terrain instances and 2016–2444 gameplay instances in 26/43–57 spatial batches. The gameplay overview fit remains 43.32361.

## Progression economy and land stage (2026-10-02)

The six-resource foundation passed `mise run test` (288 core, 120 runner) before the new construction graph. After adding the thirteen-building graph and land transactions, the core slice excluding the not-yet-adapted `OrdinaryStrategiesWinWithinBoundedSteps` campaign gate passed 280 tests. The ten focused `BuildingTransactionTests`, including added readiness/pause/disconnection/elimination coverage, and all 120 runner tests passed. New cheap coverage owns production overflow atomicity, frozen quote identity, stocked-material construction, disconnected six-resource income, selected expansion, actual investment/refund, stale generations, Market bundles and authority retry deduplication. Existing ordinary Catapult setup now funds materials through three producer buildings and three real productions. There are no new network/UI scenarios or additional source/package runs for this numerical stage.

This is partial coverage: full cheap campaign strategies do not yet pass the intermediate economy, and the complete feature CI gate remains pending. Preserve their success, role and research assertions while adapting real material/land/upkeep strategies in the later balance stage. These results do not establish twenty-wave balance or final UI integration.

## Army and twenty-wave core stage (2026-10-02)

The combined `CampaignTests`, `ArmyProgressionTests`, `ResourceTests`, `EconomyCatalogTests`, `BuildingTransactionTests` and `VillageAuthorityTests` slice passes 53 checks; `UnitSizeTests` passes all eight after progression. Cheap coverage includes level-six enemy validity, base-derived whole/nearest-five rounding, boss/research order, material quotes, wounded veterans, death/transfer identity, food reserves, fed queues, cleanup barriers and a paused dedicated-authority resume with readiness retry. Default composition and roster expansion, absent-survivor rewards, boss redistribution, all-fallen/stall precedence and final victory balances are verified. The forced-clear twenty-transition case verifies accounting only and does not prove balance.

The full cheap run at the intermediate army boundary was red (68 failures, 244 passes) because old math fixtures and food/three-wave strategies still need adaptation. The full campaign strategy gate, runner/UI integration and final complete CI remain pending. No new expensive scenarios were added for these core rules.


Economy balance evidence uses the shared ordinary-command policy in `tools/DevRunner/CampaignStrategy.cs`: four solo families and frontline co-op with two, three and four players, each at seeds 0, 1 and 123 (21 complete campaigns). All reached wave-twenty Victory with all original cities alive, 60 real productions and paid battle upkeep. Initial stone/metal/cloth were zero. Final equipment outputs are Metal Mine/Weaver 20/40 per production; other producer outputs, enemy waves, ordinary health/damage, boss multipliers, plot prices and Market rates remain as authored. Earlier trials exposed insufficient replacements and late Market construction; the final policies retain more troops and establish Markets earlier.

The policy builds Farm/Metal Mine/Barracks/Lumbermill/Stonecutter on the first five plots, recruits six paid Swordsmen for the first battle, then upgrades producers and the Barracks and purchases actual locked plots for its branch. Mixed adds Weaver/Arcanum/Archery Range; towers adds Arrow/Catapult defenses; research adds Blacksmith and both melee ranks. All retain recurring equipment supply and use quoted Market bundles. After wave thirteen, with at least 120 stone stock and level-five Barracks, they sell the upgraded Stonecutter for its actual half refund and build a Gold Mine on the retained plot. Land, troops and research remain. Recruitment logs verify no food deduction, material charges, later metal/cloth recruits, full-land expansion, producer upgrades, trades and reconfiguration.

A producer upgrade adds one level-one output while saving a plot and gold compared with purchasing another plot and producer, but requires stone. At current rates, a level-one Metal Mine can sell its whole 20-metal output for eight gold versus a Gold Mine's five gold; this consumes equipment stock and requires an additional paid Market. Markets and Gold Mines therefore have different land and supply costs. The receipt/action logs record stocks before/after commands, investment, individual army levels/health, forecast/actual rations, casualties, rewards, city health and per-wave ticks.

This finite sample establishes viable ordinary strategies; it is not universal balance proof. The separate cleared-frontage reinforcement witness uses seed 8 with its funded L2 opening and one weak-city replacement. Earlier size-only seeds 1/2/8 are historical input-specific evidence, not promises for the new authored wave composition. The Mage/Crossbowman role witness uses six ordinary screen units against six ordinary enemies at seed 123, identical identities and the shared live interval. Mage equipment is 15 cloth/5 gold and upkeep two; Crossbow equipment is five metal/ten wood and upkeep one. Both clear normally; effective support damage is 6400 versus 4000 hundredths, clearance 253 versus 283 ticks, and surviving army HP 17500 versus 16000 hundredths. No inflated enemy HP, seeded clustering or altered default profiles are used.


Protocol/runner progression evidence, 2026-10-02: protocol v8 carries six balances, permanent land, instance generations/investment/refunds, leveled profiles, Size/boss identity, all quotes, forecasts/upkeep receipts and authored wave/reward projections. Rules v4 own the new profile formula. The shared ordinary process strategy completed `mise run test-network --scenario authority-resume-victory --timeout-ms 300000` in 183.98 seconds (190.22 including preparation), `logs/20261002-121941-5361d2be/`. This is selected coverage: actual wave-twenty outcome, final boss receipt, paused disconnected typed-army restoration, exact equipment retry, old-peer refusal, roster/ownership/rate checks and fresh authority credential refusal. Fresh retries for plot purchases, sales and Market bundles have since been added and remain subject to the next run/final CI.

`mise run test-network --scenario redistribution` passed in 26.13 seconds (33.14 including preparation), `logs/20261002-123446-23d26713/`. It uses the same ordinary seed-eight investment policy as the cheap cleared-frontage witness and retains typed level/boss/size identity, recovery, original-roster pressure, protected admission, engagement and ordinary completion. Guest B is admitted before launching observer C so the numerical city's identity matches the authored seeded witness. The prior concurrent-admission attempt `logs/20261002-122901-6d684ef3/` was intentionally cancelled after the wrong city-id ordering invalidated that witness; SIGTERM invoked the runner's cancellation handler, awaited owned children and removed owned runtime state. No unrelated endpoint/process/display was touched.

The twenty-wave network path exceeded the old 180-second allowance. Default network budget is now 300 seconds, retaining roughly 63% headroom over the measured path. After the measured 156-second economy slice, source/full CI budget is 900 seconds for network followed by all source UI, sequential exports and package smoke; full serial source UI is 600 seconds, selected economy/packed UI 300 seconds and other selected UI 180 seconds. Cheap runner timeout, cancellation, child disposal and scheduling tests passed all 120 cases after the budget changes. The actual graphical economy run currently extends the existing slice through early wave-three preparation; its first trial exposed a tower-only observer defeat, `logs/20261002-123933-bc1e67ad/`, and was cleaned up. The revised observer equips ordinary soldiers before selling its Barracks to establish the Catapult on retained land. No resource grants or extra scenario were introduced.


The revised `mise run test-ui --scenario economy` passed in 156.26 seconds (166.64 including preparation/display ownership), `logs/20261002-124448-892010ee/`. Actual input verifies six funded first-wave units and exactly six food paid at Ready, real stone production, retained wounded/researched veterans after Blacksmith sale, full initial land before selected expansion, Market stock/gold and food-preview refresh, mixed L1/L2 recruits, producer sale/fresh-generation rebuild, current stockpile thresholds and the existing supported-deck/terrace/roof/camera ownership assertions. Six PNGs were retained; `economy-materials-market-land.png` was inspected. This is two battles through early wave-three preparation, not a full graphical campaign. A second expanded-Market frame at 1100x820 and the latest summary/actual-receipt projection are included in subsequent verification. The initial trial's tower-only observer was replaced with ordinary soldiers and a retained-army Barracks-to-Catapult sale; it now survives normally. Pure `ProgressionPresentationTests` pass five current-state cases, including combat receipts versus stale forecasts, final victory/reward retention, mixed-level wounded veterans, queues/reserves, repeated snapshots/city switching/fresh sessions, paused boss identity and truthful stalled defeat.


## Final progression integration checks (2026-10-02)

The second full-CI attempt, `logs/20261002-140744-01add55f/`, passed all 333 gameplay/presentation/transport tests, 120 runner tests and six network scenarios. The real twenty-wave `authority-resume-victory` path took 162.73 seconds; fresh plot/sale/trade retries use an ordinary pause after the accepted transaction, compare its retained city state (excluding tick-relative cooldown displays before pause), compare the complete frozen snapshot after retry, then resume. This preserves spending, permanent land, investment, army/profile/research state and receipts. The first CI attempt, `logs/20261002-140215-03887120/`, exposed an overbroad retry comparison while legitimate post-clear death cleanup advanced the tick. Both attempts awaited owned cleanup. Redistribution passed in 26.04 seconds with unchanged food/upkeep receipts across transfer. Solo, playing-host lifecycle, defeat and failure cases also passed. The second attempt stopped at the initial economy HUD-height assertion; this is partial coverage until the completed full gate below.

The specialist slice's uncompressed reliable messages measured approximately 90–125 KB. Temporary engine traces showed the client kept rendering and handled the ordinary command while about 350 ticks behind the authority; its acknowledgment and paused cleanup state arrived late. Whole-message Brotli/base64 RPC payloads now preserve complete JSON state while bounding encoded/expanded input to 16 MiB. Two cheap checks prove exact four-city catalog/state round trip, an encoded size below one quarter of plain JSON and bounded decompression. Packet-level compression did not resolve the delay and was removed; temporary traces are removed. The existing three-tick publication, reliable channels, authority stepping and visual cleanup bounds are unchanged. UI probes screen unrelated old observation ids before deserializing, retain exact fresh-id validation, and use the same 512-event history bound for UI as ordinary events.

With whole-message payloads, all specialist attack/contact/casualty/frozen-pose/release assertions, including cleanup within two unpaused seconds, passed in the 51.15-second iteration `logs/20261002-135709-1cbbdb9f/`; that run then failed the fresh-session HUD-height bound. The compact forecast/spacing keeps all demand/payment/fed/reserve data and separate resource rows. The seed-one `mise run test-ui --scenario combat --checkpoint melee` passed in 27.54 seconds (38.18 overall), `logs/20261002-135946-cad7d473/`, with 56 live-node witnesses and four near-impact/far-windup overview/close PNGs. The near-impact overview frame was inspected. Tower captures now focus the observer city before battle and require a fresh current-battle impact instead of matching a previous wave's history. No full graphical campaign or new expensive scenario was added.


The compact `mise run test-ui --scenario economy` passed in 93.44 seconds (103.51 overall), `logs/20261002-141623-20c8877e/`. Both expanded-Market frames at 1100x820 and 1280x720 retain all five resource-sale controls within bounds, six resource balances, purchased land/next price, forecast demand/payment/fed/reserve count, actual last reward and contextual exact refund. The 1100x820 frame was inspected. Real inputs also retain the existing picking/surface/roof/camera checks, two funded battles, unchanged recruitment food, wounded researched veterans after sale/rebuild, mixed L1/L2 army and authoritative food-sale forecast refresh. Pure current-state projections still pass all five cases after compacting the forecast. Cost-label dimming now follows the newly accepted affordability state on the same refresh.

The completed reconnect slice passed in 47.06 seconds (59.56 overall), `logs/20261002-144732-9328c0cf/`, including current dying bodies, retained paused state, identity, adjusted camera, actual Barracks picking and owned process restart. The economy opening moved Barracks from plot one to plot two; the reconnect picker now targets that building at its current plot. It retains the panned-view selection check before resetting the camera. The final specialist combat slice passed in 51.06 seconds (63.35 overall), `logs/20261002-145132-f2a37ae3/`, including the fresh-session reset and countryside/HUD bounds. Fresh-session input now waits for settled HUD height and covered countryside within the existing bounded observation deadline, instead of asserting during an intermediate menu-to-game layout frame. These selected passes remain partial coverage until the complete CI gate.

The fourth CI attempt, `logs/20261002-145300-056e2c77/`, passed cheap tests, all six network scenarios and economy/reconnect/settings, then exposed the launcher fixture's food-only recruitment assumption. The launcher now uses normal Farm/Metal Mine/Lumbermill production, constructs Barracks with produced gold, and sells the Metal Mine at its actual refund to fund Archery Range after three ordinary production turns. Both actual recruitment inputs, both viewport sizes, modal routing, fresh-session reset, invitation fixtures and hosted endpoint ownership remain checked, without any battle setup. The selected launcher passed in 90.66 seconds (103.71 overall), `logs/20261002-150122-cbaffdd3/`. Its first Farm purchase now compares both gold and wood against the resolved construction quote. The final combat fresh-session PNG from `logs/20261002-145132-f2a37ae3/` was inspected and retains six resources and five-open/four-locked land above the bounded panel.

The fifth full gate was cancelled with owned cleanup after exposing a readiness observation race: its final Ready acknowledgment reported Preparation, but the next authority tick entered Combat before peers received that intermediate revision. The match cleared normally into wave eight while the runner waited for the obsolete preparation revision. `Advance` retains monotonic revision/turn checks and accepts a strictly later turn after Preparation. The selected real twenty-wave `authority-resume-victory` check then passed in 159.03 seconds (167.91 overall), `logs/20261002-150924-6036ad59/`, including final reward, economic retries, resume, rate rejection and expired-session refusal. No authority rules or spending assertions were changed.

`mise run ci-linux-package` passed in 154.74 seconds (157.86 overall), `logs/20261002-152313-3adf6c0f/`, after sequential client/server exports. Headless package smoke passed in 1.31 seconds with normal Metal Mine production, resolved material spending and unchanged food during recruitment. The packed graphical scenario passed in 135.64 seconds (138.98 including owned display), retaining early wave-two actual food payment, six balances, land, level labels, prior clear reward, tower/locomotion/shooting and packed launcher/hosted-session checks at both sizes. `packed-locomotion.png` was inspected. Both source and packed paths retain the same numerical rules and ordinary material setup. The Windows solo smoke was likewise adapted to Metal Mine production, but no Windows runtime was executed here. The sixth full CI had passed all source checks and exports before its old headless smoke setup failed; that remains partial evidence until the next complete gate.


### Completed full progression gate

Full `mise run ci` passed in **640.75 seconds** (642.14 overall), [`logs/20261002-152630-eb9ef5de/ci-summary.json`](../logs/20261002-152630-eb9ef5de/ci-summary.json). All 43 change tasks are complete. The tested tree is the uncommitted feature work based on `ad771f7595db3a9db53a0a698bba281644597fcf`, with .NET SDK 10.0.401, Godot .NET 4.7.2 and unchanged dependency/tool locks. Protocol is eight and combat rules version four. Resolved default configuration fingerprint is `e2e6e9c9a6c2cd8046d3cda43dbc8b39aa3f5e9025f6e2d7ff1849365d8bef56` (the snapshot represents its four 64-bit parts numerically). Ordinary strategy samples use seeds 0, 1 and 123; the existing redistribution witness uses seed eight, and graphical combat uses seed one.

The complete gate passed 333 core/presentation/transport tests (68.12s), 120 runner tests (1.76s), and all six network scenarios: authority-resume-victory 161.18s, redistribution 26.23s, defeat 7.64s, failure-cases 3.31s, solo-session 0.86s and playing-host-lifecycle 6.03s. It passed all five serial source UI slices: economy 92.69s, reconnect 46.20s, settings 25.45s, launcher 90.16s and combat 51.76s; source UI including display ownership took 309.58s. Sequential client/server exports passed in 4.93s/3.93s, headless package smoke in 1.46s and graphical package smoke in 134.40s (137.69s including owned display). Source and packed PNGs, per-process engine logs and phase/scenario summaries remain under that ignored run directory. Both Market viewport frames, melee near-impact overview, final fresh solo and packed locomotion frames were inspected in the selected runs recorded above. The separate seed-one melee checkpoint passed in 27.54s and is retained as additional partial coverage beyond this full gate.

This complete gate supersedes the staged pending/red records above. No resources were granted to strategies or process/UI setup. All owned processes, endpoints, displays and temporary preferences were cleaned up. There was no upload, deployment or publication. The 21 numerical strategy campaigns remain a finite balance sample; software OpenGL/private X11 with Dummy audio establishes rendered assertions, not native compositor/GPU performance, physical input or listening quality. Native Windows runtime and real paired Steam/account checks were not executed here.


## Compact match UI implementation (2026-10-02)

The pre-implementation full `mise run ci` baseline passed in 653.22s at
`logs/20261002-170019-5010fcaa`, with source commit
`0909e72e3cf757c94b8113b705379ec6d995a4f8` and only OpenSpec planning edits at
its start. Locked .NET 10.0.401 and Godot 4.7.2 .NET were used. All source
checks, sequential Linux exports, headless package smoke and graphical package
smoke passed. A separate user-owned `Options.cs` package deadline edit is
preserved during implementation.

Existing economy, reconnect, settings, launcher, combat and exported-package
helpers now drive Space reset, city arrows, text controls and confirmed return.
The economy camera helper checks actual left-button motion, bounded travel,
outside release, modal interruption and resource-table click/wheel protection.
Existing combat pauses provide living units for centre-right inspection; no
additional match is prepared. Each inspection adds fresh probes and one frame,
checks resolved health/damage/level/size, plot retention, interior protection,
outside dismissal and drag suppression. Source combat resizes the same paused
fixture; packed smoke reuses its existing pause. Cheap projection tests cover
Roman subtractive notation, percentage rounding and all five turn stages.

The bottom HUD reserves about 180px at both supported sizes. Resources are a
text table in Gold/Food/Wood/Stone/Metal/Cloth order; Details retains upkeep,
receipts, roster and army information. Locked plots use the bundled gold marker;
physical stockpiles remain. Home health uses a transparent percentage overlay,
and living units retain transparent health bars with Roman levels. Settings
leave confirmation defaults to Cancel and explains solo, host or guest effects.
These checks need UI coverage because pure snapshot tests cannot establish
Godot input routing, embedded-window focus, projection, clipping or packed assets.
Selected results and final full-CI evidence are recorded below once complete.

Selected economy coverage passed in 101.49s at
`logs/20261002-172914-e50ea862`; reconnect passed in 47.21s at
`logs/20261002-173139-13f84300`. The initial launcher pass took 96.87s at
`logs/20261002-172525-cfab5f2d`. These are partial coverage of their recorded
inputs; later assertions extend guest resume and host confirmation. The first
economy iteration caught a preview-camera scene-tree initialization error;
its corrected run passed. A Settings driver was adjusted to accept Space
activating its focused Close button while still asserting no camera reset.
The inspection frame in reconnect confirms the bounded preview and text stats
fit between the resource table and bottom HUD at 1100x820.

The expanded launcher passed in 115.10s at
`logs/20261002-173533-d87f93f4`: confirmed guest leave retains its city and music,
then one intentional owned restart resumes it with its private credentials.
The existing host fixture also confirms return and creates a fresh host before
its retained native-close assertions. Relative to the 90.02s baseline launcher,
this adds 25.08s across both source sizes, modal checks, captures and the guest
restart. Reconnect's transport-loss inspection assertions passed in 49.91s at
`logs/20261002-173842-c77fc5f7`. The first expanded combat action-count
assertion was corrected to use authoritative command sequence numbers, because
the runner's bounded event history can evict old acknowledgements during probes.
The 180 logical-pixel HUD reservation renders as about 180 physical pixels at
1100x820 and 158 at 1280x720 under existing canvas scaling, preserving the 60%
reduction from the former 300 logical pixels.

Final selected source checks passed: economy 115.15s
(`logs/20261002-174909-9e4e6e71`), Settings 25.19s
(`logs/20261002-174101-30941572`), combat 62.83s
(`logs/20261002-174136-2de5c6d6`) and launcher 122.43s
(`logs/20261002-174249-d3c0e25e`). Launcher checks use actual confirmation title
close, Escape and Cancel at both sizes, and end the host while the guest's
confirmation is open. Combat verifies live inspector damage, outside Pause
execution by command sequence, and cleanup for a removed selected unit. The
reconnect case verifies frozen inspector data on transport loss. Exact table
balances are awaited after accepted building/preparation actions and checked
again on reconnect, including zero metal/cloth/stone cases. Physical stockpile
counts and cooperative food/reward assertions remain.

All 349 rules tests passed before the final additional inspection-format test;
120 runner tests passed separately. The affected pure presentation slice then
passed all 22 tests, including a veteran's resolved ranked boss profile, fractional
health, damage, size and name. Boss numerical presentation is checked cheaply;
graphical inspection reuses deployed early-wave allies/enemies rather than
replaying ten waves to obtain a boss. Preview, phase, resource, camera, return and
unit picking helpers are also used by the packed-client smoke. Software-rendered
frames and Dummy audio establish these presentation paths, not native GPU,
physical-device performance or listening quality. No scenario or tool dependency
was added, and the checks retain owned X11/XDG/endpoints and awaited cleanup.

The first integration run at `logs/20261002-175407-3581280b` was intentionally
canceled after its 350 rules, 120 runner and all network checks passed. A crowded
1280x720 combat frame revealed that a unit bar could cover the home percentage
above the home bar. The percentage now sits inside the home bar and the home
control is last in its overlay layer; resource/HUD panels still draw above it.
The supervised runner completed owned child/display cleanup on cancellation.
That 248.40s partial run is not a successful full-CI result. Final integration
restarts with consistent source inputs for this visual correction.

Full integration passed in 779.99s at `logs/20261002-175915-9de304ce`:
locked restore 0.89s, format verification 15.39s, build 1.49s, import 2.91s,
350 gameplay tests and 120 runner tests, all six network scenarios (166.75s),
all source UI (391.69s), sequential client/server exports (5.91s/3.91s),
headless package smoke (1.36s) and graphical package smoke (187.89s including
worker supervision). Source UI timings were economy 118.68s, reconnect 55.75s,
Settings 25.35s, launcher 125.50s and combat 63.09s; the packed scenario itself
took 184.59s. `source-inputs.json` records commit, locked tool versions and SHA-256
hashes for 544 source/test/tool/lock/asset files. This includes the percentage
inside the home bar and correct overlay ordering. Nothing was published.

A final verification-only extension resizes the already paused packed combat
fixture to 1280x720 and repeats unit inspection there, supplementing its 1100x820
frame and both source-size frames. It changes only the packed branch of the
existing combat driver; game source, assets and verified exports stay identical.
The selected exported-package rerun uses those existing exports without source
preparation. Its owned setup and battle are unchanged; the added resize, probes
and PNG catch packed viewport/clipping differences that pure projection checks
cannot establish.

That additional check at `logs/20261002-181553-9dcf8182` passed its
1100x820/1280x720 inspection and other feature assertions, but failed overall
in 195.54s because Godot reported nonexistent native focus-signal connections
during Settings resize. A retry with explicit Settings-open/closed barriers at
`logs/20261002-182316-071d9572` reproduced those engine errors and was canceled
through the owned runner, which awaited private-display/peer cleanup. Neither
run is a successful acceptance result. An attempted fix deferred recentering its
existing embedded window instead of reopening it during the resolution popup's
close callback. This game-source attempt required fresh full integration;
the earlier successful integration still records the earlier source inputs.

Source combat passed after recentering (63.85s scenario / 74.50s standalone,
`logs/20261002-182559-b5df24b4`). Full integration at
`logs/20261002-182754-9ac421cb` passed 350 gameplay tests, 120 runner tests,
all networks and all source UI, exports and headless package smoke, but failed
graphical package acceptance (193.79s scenario; 792.33s total) on the same focus
error. A checked-in-driver diagnostic at `logs/20261002-184135-8f854cfa`
localized it to Enter accepting the resolution choice; it was canceled through
the owned runner after reproduction. Repositioning alone was insufficient.
Deferring resolution application also reproduced the error in the selected
packed run at `logs/20261002-184404-ee50da7c`, which was canceled through the
owned runner. These timing/recentering changes were reverted.

This matches upstream [Godot issue #89657](https://github.com/godotengine/godot/issues/89657),
a release-template OptionButton/Popup connection defect absent in editor runs.
The final packed inspector size check uses checked-in XResizeWindow input on
the supervisor-owned display, with observed native window/PID ownership checked
before mutation. It still exercises actual window resize, reprojection, inspector
picking/dismissal and PNG/control bounds at both supported sizes. Settings-driven
resolution selection retains source coverage; exported native-dropdown diagnostics
remain an upstream limitation, and are not suppressed in the engine-error checker.
No engine, export mode, dependency or Settings display behavior was changed.
All game/assets/locks/tests match the successful `20261002-175915-9de304ce`
full-CI input manifest. The final two-size extension changes only
`CombatUiTests.cs` and `NativeWindowClose.cs`, so that full implementation result
is reused with a fresh selected packed check and runner checks for the extension.

Final selected package acceptance passed at `logs/20261002-184754-a7ba964c`:
189.88s scenario / 193.17s supervised graphical suite / 194.56s standalone task.
Both `packed-unit-inspection.png` (1100x820) and
`packed-unit-inspection-resized.png` (1280x720) contain the unit preview, stats,
Roman bars, home percentage, resource table and compact HUD, with actual picking,
interior/outside dismissal, drag and Space assertions repeated at both sizes.
Shooting/death and solo/host/guest confirmation/leave checks also passed; no engine
errors were ignored. Relative to the earlier 184.59s packed scenario, this run
added 5.29s including ordinary run variability; the extension adds one native
resize, existing inspection probes and one PNG, with no additional battle/setup.
Formatting and all 120 runner tests (550ms test time) passed for these driver
changes. `source-inputs.json` records their final hashes; comparing with the
successful full-CI manifest identifies only the two driver files listed above.
The verified client export was regenerated from the identical accepted game
inputs after reverting the unsuccessful timing fixes. Documentation/task-only
updates do not change those acceptance inputs. No publishing was performed.


## Verification speed implementation: coverage ownership

Implementation starts from the accepted compact-UI state. The full gate at
`logs/20261002-175915-9de304ce` passed in 779.99s with 350 gameplay and 120
runner tests. The subsequent selected packed acceptance at
`logs/20261002-184754-a7ba964c` passed in 193.17s supervised and verified the
only two changed driver inputs (`CombatUiTests.cs`, `NativeWindowClose.cs`).
Before this implementation, all 544 hashes in that later manifest matched the
current checkout; the full-gate manifest differed only in those two drivers.
Locked .NET 10.0.401 and Godot 4.7.2 .NET also matched. These records together
are reused as the successful before baseline. Planning/archive/documentation
changes do not alter their test inputs. The original full gate is historical;
the selected extension is partial coverage, not a second full-suite result.

Assertions move only after their replacement passes. The following map names
coverage owners; implementation results below distinguish pending from verified
migration. No seed/player sample or cooperative invariant is removed.

| Current scenario/helper | Flow/math owner | Retained boundary witness |
| --- | --- | --- |
| `authority-resume-victory`, `RecruitAll`, twenty-wave loop | `SessionFlowTests`, `SessionCampaignTests`, all 21 strategy combinations | Real ENet ownership, early clear, transport reconnect/retry/rate rejection and stopped/stale server |
| `redistribution`, `ReinforcementInvestment` | `SessionFlowTests.SerializedThreeCityTransferPreservesIdentityAndNextWaveAllocation`, existing `VillageStrategyTests` protected-admission and combat reservation checks | Three actual peers, immediate transfer agreement, next-wave allocation and resumed eliminated observer |
| `defeat` | Existing no-investment and terminal-reason C# checks | Real automatic city fall and terminal feedback |
| `failure-cases` | Existing `HarnessTests` and authority validation checks | Actual unavailable socket, bind collision retaining owner, readiness failure and child exit |
| `solo-session`, `playing-host-lifecycle` | Existing `AuthoritySessionTests` local/remote lifecycle checks, `SessionFlowTests` | Socketless Godot authority and real host/guest delivery, host end, guest reconnect and fresh session |
| `economy`, `EconomyDetails` | Economy/authority/strategy C# arithmetic, generations, materials, upkeep and rewards | Each distinct purchase/upgrade/recruit/sale/trade/land control family, observer effect, disabled/foreign control mapping, picking and both-size bounds |
| `reconnect` | C# session rebind/upkeep/retry and `CombatPlaybackTests` reconstruction | Actual Reconnect control, retained local view, current moving/dying rendering and fresh selection |
| `settings` | Existing pure preference/volume mapping checks | Native modal/focus/input isolation, slider/tab/dropdown, intentional owned preference restart |
| `launcher`, `MenuRoute`, `FriendsUiScenario`, `HostedMenuRoute` | Existing Steam picker/policy and authority lifecycle tests | Actual menu/solo/friends/host/guest routes and native modal/focus/exit once; resize existing fixtures for layout |
| `combat`, `MixedArmy`, `SpecialistArmy` | Existing combat math/event/occupancy/presentation C# checks | Actual melee/ranged controls, live imported rigs/actions/contact/death, freeze, inspector/picking and cleanup at normal speed |
| `exported-package`, installed graphical route | Asset inventory/provenance and C# flow/binding coverage | Standalone packed menu/solo/purchase, rig/clip/material load, one live animation, both-size layout and owned exit; headless packed solo/host roles retained |

Normal-speed visual checks still prove input/rendering boundaries. C# flow
acceptance does not claim ENet, compositor, GPU or physical-device acceptance.
The 180–300s warm local gate is a target until measured. Cold hosted preparation
and Linux/Windows jobs remain separately reported.


The initial C# replacement slice passed all six new session cases (4s execution).
The complete `mise run test` then passed 356 gameplay tests partitioned as 334
general, 12 ordinary solo strategies plus one serialized session campaign, and
nine cooperative campaigns; 121 runner tests also passed. Evidence:
`logs/20261002-200540-86127d50/`; the C# suite took 51.23s including its two
bounded hosts. The prior recorded rules phase was 68.08s while overlapping real
network processes; workload and contention differ, so this is acceptance evidence
and a directional timing observation, not a controlled benchmark.

## Verification speed implementation evidence (2026-10-02)

The retained `authority-resume-victory` selection passed in 11.59s (17.69s including preparation), `logs/20261002-201545-0e09e6c7`. It keeps dedicated-authority ENet ownership/refusals, ordinary equipment and two early clears, exact accepted recruitment retry, frozen process recovery, rate-burst rejection, stopped-host feedback and expired-session refusal. Full twenty-wave and boss rewards are owned by the passing serialized C# campaign and all 21 existing strategy combinations. This selected run is partial coverage. It uses default four-step setup batching; engine/supervisor snapshot duplication is removed.

Current admission is two expensive scenarios total during CI, with at most two graphical workers. Network and graphical scenarios release admission only after owned cleanup; first failure cancels siblings and awaits cleanup before source failure reaches the export gate. Every graphical worker owns one selected scenario, Xvfb/Xauthority/Openbox, endpoint/storage roots and report directory. Serial graphical admission preserves the same five selectors. Cheap C# processes have their separate maximum-two budget; core test collections are serial inside each process, including the in-process fallback, so extra campaign throughput does not depend on concurrent Arch registry lifetimes. All shared restore/build/import/export mutations remain sequential.

Routine full-state retention is at most 16 snapshots and 16 MiB per child; reliable lifecycle/result/error metadata has a separate 512-event bound whose unobserved overflow fails visibly. Wake notifications have capacity one, because the driver reads retained state rather than consuming a snapshot backlog. Compact transcripts buffer routine writes and flush errors/results and awaited shutdown. Explicit engine transcripts omit protocol frames and cap at 8 MiB. Checkpoint dumps report truncation, simulated ticks, pacing and bytes; credentials are redacted, including nested message fields and full `--trace` output. Full trace is optional and can exceed routine limits.

`--simulation-speed 1..8` configures supervisor-owned authorities only; default setup speed is 4, interactive speed is 1. The owned data marker and token are checked locally. Guest/RPC requests cannot configure stepping. Every accelerated callback iterates ordinary steps and yields to Godot; pause accumulates no catch-up. Publication is bounded to one routine combat snapshot per 50ms wall time, with phase transitions, command results and explicit checkpoints published promptly. Rate limits, connection and cleanup deadlines still use wall time. Graphical action/animation witnesses acknowledge speed 1 before entering their measured battle.

Source economy retains actual build, readiness, equipment/research, sell, buy-land, upgrade and Market control families, plus cooperative observation, exact balances and supported-size geometry. Repeated recruit/trade/production setup uses accepted ordinary commands. Launcher/friends boundaries execute once, resizing prepared windows for second-size geometry; Settings retains its preference restart and native resolution controls. Packed/installed graphical checks load menu assets, enter solo, purchase through actual input, load all rig/clip bindings, observe one live animation, capture both sizes and terminate through Exit. Broad source animation/application proofs and headless packaged role checks remain separate required owners. Native owned resize establishes packed layout; it does not claim native dropdown completeness.

The affected selected economy route passes in 91.08s (94.40s with owned display), `logs/20261002-202721-298c3b1a`. Relative to accepted historical 118.68s this is directional improvement; host compiler contention and serial selected execution differ from the final shared-budget gate. A preceding concurrent attempt exposed an owned-window startup focus race; the camera driver now awaits actual native focus before world input. That failed attempt cancelled the sibling and cleaned up both owners, and is not counted as passing coverage.

The first integrated gate `logs/20261002-203428-ca27374c` failed before exports on same-process host restart. It passed all 359 C# checks, 137 runner checks, economy, reconnect and Settings; launcher/combat were cancelled, not passed. Bounded result/state storage needed arrival ordering across match revision resets and one event identity per acknowledgment. A fresh-host predicate now requires a non-null retained state; the corresponding cheap regression and all 138 runner checks pass. Selected `playing-host-lifecycle` then passes in 6.18s, `logs/20261002-204236-35f2b358`.

A cancellation audit found one graphical runtime directory after the forced worker bound, with no live owner. It was reclaimed only after verifying owners had stopped. Graphical runtime data now lives beneath the parent-owned display root, so awaited process-group cleanup precedes removal even when a worker cannot finish its own finally. A synthetic abrupt-wrapper test covers the nested ownership and cleanup.

The second integrated attempt, `logs/20261002-204713-6aec0cd1`, was rejected by the engine-error gate on startup XML/decompression diagnostics in the reconnect observer, despite completing the feature assertions. All owned runtime roots were absent after cancellation. The selected reconnect rerun with identical code/native dependencies passed in 37.74s (41.07s with private display), `logs/20261002-205441-2d7edbfa`; the startup error did not reproduce. The two-error pattern is consistent with the [godot-cpp documentation decompression/load path](https://github.com/godotengine/godot-cpp/blob/master/src/godot.cpp#L284), an inference rather than an established root cause. No engine errors are suppressed and no dependency/cache outside owned state is changed. Retain this intermittent native startup observation as a limitation.

The final required `mise run ci` passed at
`logs/20261002-205614-c7ee8d08`: **252.82s** supervised / 254.20s task wall time.
It passed 359 gameplay checks (337 general, 13 solo/session campaign, nine
cooperative campaigns), 139 runner checks, all six network selectors, all five
source UI selectors, sequential client/server exports and both headless and
graphical package gates. Locked restore, solution formatting, build and import
also passed. No engine errors were ignored. All 25 recorded owned runtime roots
were absent after completion.

| Tier | Historical full gate | Final full gate |
| --- | ---: | ---: |
| Entire required gate | 779.99s | 252.82s |
| C# partitions | 68.08s | 55.37s |
| Network suite, including admission waits | 166.75s | 119.03s |
| Source UI suite, including admission waits | 391.69s | 199.95s |
| Exported graphical suite | 187.89s | 15.63s |
| Retained evidence | 4,224,801,963 bytes | about 108,800,000 bytes |

The observed full-gate reduction is 67.6% (3.09 times faster), within the
180–300s warm target. These are ordinary acceptance runs, not controlled
benchmarks: coverage ownership, host contention and scheduler overlap changed.
The earlier selected two-size package extension took 193.17s; it remains partial
coverage and is not substituted for the historical full-gate package time.
Routine evidence decreased by about 97.4%; captures and bounded full states remain
available. `ci-summary.json` reports bytes before writing its own summary and the
subsequent input manifest, accounting for a small difference from directory size.

`source-admission.json` records actual maxima of two expensive scenarios and two
graphical workers. C# started alongside the two initial network cases, then
economy overlapped redistribution and reconnect; launcher and combat later ran on
separate owned displays. Suite durations include waiting for the shared admission
budget and cannot be added to obtain wall time. Source gates finished before the
7.94s client export, 3.94s server export, 1.47s headless package smoke and 15.63s
graphical package smoke. Preparation mutated shared outputs only once.

Owned authority setup used speed 4; fresh acknowledgments switched graphical
animation/input witnesses and transport transient checks to speed 1. Final state
dumps show combat at tick 1324, reconnect at tick 590 and packed animation at tick
22 with speed 1; the accelerated defeat authority reached tick 313 at speed 4.
These are retained last observations, not a count of every tick executed across
all restarted peers. C# equivalence checks compare each intervening ordinary step
at speeds 1, 4 and 8, including pause/resume without catch-up.

`source-inputs.json` records 563 source/asset/lock/workflow hashes, all unchanged
from the during-run manifest when checked after completion. Numerical
`src/Game.Core` inputs match the accepted before baseline. Locked SDK/Godot
versions are unchanged. The environment used `UseSharedCompilation=false` to
avoid an observed busy shared compiler service without stopping a developer
service; this is a comparability limitation, not a dependency change. Existing
workflow job identities and the 300-second Linux-package default are retained.
Serial scheduler/option and cancellation coverage is established by runner tests;
no second full serial game run was performed solely for timing comparison.

The final `mise run test-in-process` fallback also passed the same 359 gameplay
checks (79.95s) and 139 runner checks (0.57s), with no skipped cases or Godot
processes. This is complete cheap coverage and partial coverage of the full gate;
it verifies the final serial core collection policy and socketless entry point.

### Core simulation measurement setup (2026-10-03)

Before implementation, all 563 hashes in
`logs/20261002-205614-c7ee8d08/source-inputs.json` matched. The Linux kernel/glibc,
12 logical processors, locked SDK 10.0.401 and Godot .NET 4.7.2 also matched.
All seven owned-display executables are available. Optional `dotnet-trace` and
`dotnet-counters` are absent; no tools or dependencies were installed. Inspection
evidence is `logs/20261003-core-simulation-environment-audit/audit.json`.
The completed `speed-up-verification` coverage remains required: 21 ordinary
strategy rows, serialized campaign, six network selectors, five source UI
selectors, sequential exports and both package gates. Default budgets remain two
cheap processes, two expensive scenarios, at most two owned graphical workers,
setup speed four and timing-witness speed one. UseSharedCompilation remains false
for comparable runs. The historical 252.82s CI / 55.37s C# figures are verification
evidence only. Fresh runtime and verification-speed baselines are collected after
measurement fixtures are validated, before any optimization.

The selected commands are `profile-campaign`, `test-scale`, `profile-scale`,
`profile-snapshots` and `profile-presentation` in the README. Engine-free commands
restore locked dependencies and prepare only the selected core-test executable
outside measurement. The no-argument executable still runs the existing xUnit
framework in process; ordinary test-host discovery and all campaign identities
remain unchanged. A profile runs one separate warm-up and 1..10 measured
executions (default three), serially, in distinct owned processes. Each has a
600-second wall deadline; timeout/cancellation kills only its worker, awaits exit
and reports nonzero. Warm-up warms filesystem/runtime caches; fresh workers still
include their own JIT costs. `test-scale` has one execution and no warm-up.

`odot-profile-v1` identity files record full source/asset/lock hashes, SDK/runtime,
OS/CPU/machine, configuration, seed, workload selection, iteration/bound/concurrency
and counter mode. Samples contain monotonic elapsed milliseconds, process CPU
milliseconds, managed allocated bytes and generation 0/1/2 collection deltas.
[Process CPU](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.totalprocessortime?view=net-10.0)
includes user and system time across the process. [Managed allocation](https://learn.microsoft.com/en-us/dotnet/api/system.gc.gettotalallocatedbytes?view=net-10.0)
excludes native allocations; precise sampling has cost and is used at coarse
boundaries rather than at individual calculations. Collection counts are not
pause durations. Summaries exclude warm-up and give count/median/min/max, without
tail-percentile claims for three whole-workload samples.

Campaign wall phases distinguish setup, actual fixed-step calls, preparation,
assertions, diagnostics and evidence/disposal. Existing investment assertions
inside diagnostic interpolation are initially labelled `diagnostic-assertion`;
they are retained at every verbosity. Phase changes use timestamps and bound
checks, with no per-operation timestamps. Each worker reports excluded calibration
for 100,000 alternating phase boundaries and 250 coarse metric reads, plus its
boundary count. Calibration is a cost estimate, never subtracted from samples. Total CPU/allocation includes the
harness; stepping wall time excludes preparation, snapshots used for evidence,
hashing and diagnostic formatting. Retained command/per-wave/final digests
normalize only the random session id. Golden short traces preserve ids, seed,
route/target order, events, transfer, simultaneous impact/death, pause/resume and
terminal cleanup; the fixed replay has separate authority-input/playback-output
references.

Work-counter schema 1 reports independent semantic categories, not instructions,
FLOPs or a total number of calculations. Unsupported owner categories remain
null; supported but unused sites are zero. Counters are off in normal gameplay,
owned by one match/playback and reset independently. They count authoritative
view builds/visits, ordered materializations, projections and copied decisions,
local observation inputs, opponent/range/splash candidates, dynamic actor BFS
search/dequeue/edge visits, occupancy/admission, profile resolutions, snapshots,
codec calls/bytes, playback indexes/event visits, rig pose samples and HUD refresh
work. HUD section counts refer to status/feedback, roster/focus, resources/progression,
economy context and session controls; each full baseline HUD refresh updates all
five. Static board precomputation is reported under setup timing. Counting runs
include fixture/evidence observations, so deterministic isolated regressions own
claims about eliminated bookkeeping work. Counter-enabled timings are separated
from counter-disabled timing samples; matched overhead is reported explicitly.

The large fixture is synthetic: one validated 32-column-by-8-row offset-hex board,
256 cells, six anchors/capacity, protected opposite rear rows, neighboring neutral
front bands and ordinary siege links. Both factions have equal counts of all four
roles, level-one size-two profiles and normal timings; only army setup/funding is
test-owned. Sizes 128/512/2,048 use the same board, proportions and 600-tick horizon.
Smaller natural completions report actual ticks without padding. The largest
case must run all 600 normal ticks, with at least 256 simultaneous deployed actors
including 128 per faction, nonempty queues, landed attacks, casualties and actual
admission of initially queued actors. Checkpoints verify faction/capacity/anchor/
transit ownership, identity conservation and history bounds. Ordered events are
hashed before history truncation; setup/assertions/hashing are separate from timed
stepping. Population peaks/means and deployed-actor-tick totals disclose active
work. This is fixed-window stress acceptance, not twenty-wave balance, battle
completion, default-board capacity or graphical performance. Full CI alone does
not establish this large-scale acceptance.

The initial unoptimized feasibility trial
`logs/20261002-223108-test-scale-d43a5609` passed all 600 ticks: initial deployment
was 192 per faction, final deployment 624, landed attacks 1,226 and deaths 238,
with a retained queue. It is fixture feasibility, not a frozen comparable timing
baseline. Setup/measurement regressions passed; the fixture will be frozen with the complete
measurement scaffolding. Size, deployment thresholds and window are not reduced
afterward.

Snapshot profiles perform 25 operations per phase for both ordinary/large inputs.
Projection is measured from authority; JSON serialize/deserialize and Brotli plus
Base64 compress/decompress use fixed DTO/byte inputs. Stage implementations are
checked against the unchanged engine-independent production codec, and complete
encode/decode is measured too. No wire schema, enum, compression setting or 16 MiB
bound changes. Projection improvements are not credited as codec improvements.

Presentation profiling is an explicit diagnostic replay on one owned private
display, not an added default UI scenario. Checked-in ordinary commands generate
600 fixed-delta frames with movement, landed impacts, deaths/expiry, pause/resume,
hidden-city focus and return. The normal playback/Tabletop/imported rigs consume
the identical serialized input digest. Initialization and final PNG capture are
outside samples. Frame distributions contain 600 managed update observations;
process CPU/allocation over the replay also includes rendering/native callbacks
and waiting. Native max-fps is 30, scripted delta is 1/60; actual software-rendered
frame throughput can be lower. Neither CPU totals nor these managed samples claim
native GPU/compositor/physical-input/listening performance. Missing prerequisites
are unexecuted/nonzero; errors fail without suppression or desktop fallback.

The first graphical trial `logs/20261002-225802-ce8fc041` processed the script but
failed on an audio-resource shutdown leak: the diagnostic called immediate tree
quit instead of the normal bounded audio/exit lifecycle. It is failed evidence,
not a baseline pass; the diagnostic now uses the existing exit path.

The library choice remains built-in collections. Microsoft's
[FrozenDictionary guidance](https://learn.microsoft.com/en-us/dotnet/api/system.collections.frozen.frozendictionary-2?view=net-10.0)
describes a higher construction cost in exchange for repeated lookup performance.
Immutable board/catalog indexes keep that lifetime; short-lived BFS results will
use privately owned ordinary maps after matched baseline collection. No library
upgrade, new dependency, approximate authoritative calculation or persistent
cross-stage route cache is introduced by the measurement scaffolding.

The corrected graphical trial `logs/20261002-230408-3ee925a1` passed one warm-up
and one measured replay with the same input digest, software renderer and owned
shutdown. The selected Debug solo campaign smoke
`logs/20261002-230409-profile-campaign-53d528b0` also passed warm-up plus one
measured execution with all ordinary campaign assertions, command/per-wave/final
evidence and separate wall phases. These are harness trials under overlapping
host activity, not the frozen Release/three-repetition runtime baseline.

### Frozen unoptimized runtime baselines, 2026-10-03

All eleven runtime/counting families share 588 source/asset/lock input hashes. Engine-free source digest: `0416B71F30A1E1AC936CF827616806AE9D8C25B6477ACDB33D7F4F839440CDF7`. The full source archive, Git base and SHA-256 are retained under `logs/core-simulation-comparison/`. No runtime or harness optimization precedes these samples. The initial 527-test/full-CI harness validation passed. Final frozen verification baselines also passed: `mise run test` 55.895s command / 52.805s runner (383 gameplay plus 144 tooling tests), and `mise run ci` 236.807s command / 235.43s runner, all six network scenarios, five source UI slices, sequential exports and package gates. Evidence: `logs/20261003-001028-ec11bcd2/` for cheap checks and `logs/20261003-001122-2c31b307/` for CI. Historical 252.82s/55.37s remain separate. The runtime baselines and final frozen verification gate passed before any optimization group began.

Each timing family has one separate warm-up and three serial measured repetitions. Values below are medians; raw samples and min/max are retained in each evidence directory. Managed allocations are cumulative allocated bytes over the run, not peak heap size.

| Workload | Whole elapsed ms | Process CPU ms | Managed allocated MB (decimal) | Stepping ms | Evidence |
| --- | ---: | ---: | ---: | ---: | --- |
| campaign-solo | 2429.82 | 2940.07 | 954.60 | 1966.79 | `logs/20261002-234042-profile-campaign-c9da69b7` |
| campaign-cooperative | 3933.85 | 5067.42 | 5338.02 | 3022.91 | `logs/20261002-234054-profile-campaign-302d1d8d` |
| scale 128 | 2105.23 | 2389.82 | 434.10 | 1462.56 | `logs/20261002-234353-profile-scale-c0ee58bf` |
| scale 512 | 54172.27 | 55216.00 | 10156.14 | 53428.08 | `logs/20261002-234353-profile-scale-c0ee58bf` |
| scale 2048 | 120916.63 | 121825.17 | 68135.09 | 119559.20 | `logs/20261002-234353-profile-scale-c0ee58bf` |

The 2,048-actor correctness selection passed 600 normal ticks: peak deployment 665, 357,626 active-unit-ticks, 1,226 landed impacts, 238 deaths and 478 admissions from the original queue. Every scale repetition and counting run matched the initial/checkpoint/event digests. Initial deployment was 192 actors per faction with 1,664 queued; final deployed population was 624 with 1,186 queued. This is a fixed-window stress result.

| Fixed snapshot input, 25 operations | Projection elapsed ms | Projection CPU ms | Projection allocated MB | Evidence |
| --- | ---: | ---: | ---: | --- |
| ordinary | 2.33 | 2.32 | 1.09 | `logs/20261002-234112-profile-snapshots-ff93a5c7` |
| large | 33.47 | 33.37 | 76.05 | `logs/20261002-234112-profile-snapshots-ff93a5c7` |

Presentation (`logs/20261002-235551-a55cf94c`): managed update p50/p95/p99 medians 1.5401/2.7649/3.9074 ms across three 600-frame executions; summed managed updates 1020.10 ms, whole-process elapsed 55375.65 ms, CPU 105568.77 ms and allocated 37.56 MB. Build is locked Debug, renderer llvmpipe LLVM 23.1.1, native max-fps 30 with scripted delta 1/60. The input's final authority tick is 361; pause and post-clear frames remain part of the identical script. Process metrics include software rendering between updates. Setup, the 100,000-timer calibration and final PNG capture are excluded.

Counting runs retain separate evidence:
- `campaign-cooperative-counters`: `logs/20261003-000022-profile-campaign-8819a196`.
- `campaign-solo-counters`: `logs/20261003-000011-profile-campaign-84d63f94`.
- `presentation-counters`: `logs/20261003-000649-16a5407b`.
- `scale-counters`: `logs/20261003-000049-profile-scale-652cefcc`.
- `snapshots-counters`: `logs/20261003-000031-profile-snapshots-6960f453`.

For the four-player campaign, baseline counts include 152,815 full world views, 11,769,832 unit visits, 6,678,595 DTO projections, 333,483 observations and 31,573,643 opponent visits. Presentation counted 1,196 playback index builds, 500,255 event entries examined, 17,562 full poses and 465 HUD section refreshes. Counts include workload setup/evidence sites where stated; they cannot be summed into CPU instructions.

Counter-on solo elapsed median was 2,173.97 ms (2,130.67..2,520.92), versus counter-off 2,429.82 ms (2,299.23..2,522.87), with about 6.23 MB additional managed allocation. The observed negative elapsed difference is inconclusive about overhead: counting changes iterator/JIT paths and these ranges overlap. Do not subtract a negative or guessed overhead from runtime measurements. Timing comparisons use counter-off only. The retained core boundary/metric-read calibrations and presentation clock calibration quantify their explicit measurement calls independently.

[Interaction-by-interaction source review and precision/library decisions](simulation-performance.md).

### Authoritative view ownership and intermediate evidence

Cleanup collects one canonical id-ordered ECS view; advance reads committed immutable values after arrivals/recovery and kills after the simultaneous damage stage. Admission reuses its membership and reads post-placement values for first-admission bounds. Narrow enemy membership observes cleared cities before transfer, refreshes after redistribution, and supplies count/engagement destinations without DTO projection. Direct entry points always collect fresh input, including mutations at the same tick. Stage arrays are independently owned and never back public DTOs/events.

`AuthoritativeQueryTests`, lifecycle/action/limit/transfer tests and all five ordered reference digests passed; the complete cheap gate passed 385 gameplay plus 144 tooling checks (`logs/20261003-001753-f076e769`). Selected four-player campaign and 128-actor counts matched baseline workload digests. Intermediate counter-off campaign stepping was 2475.84 ms and allocation 2286.13 MB (`logs/20261003-001840-profile-campaign-6f691d5b`); baseline medians were 3022.91 ms and 5338.02 MB. Counting evidence `logs/20261003-001848-profile-campaign-c224cd58` and `logs/20261003-001856-profile-scale-61ba7018` is separate. These single measured repetitions establish directional attribution for the query area; final three-repetition comparisons remain required.

### Exact decision/search ownership and intermediate evidence

City/faction arrays belong to one action/impact stage and retain canonical ids. Observation payloads are built lazily once per relevant city/stage; exact value equality can reuse a prior immutable payload, while actor decision sequences remain separate. Relevant movement/casualty/admission/transfer changes compare unequal; health-only and unrelated-city inputs retain previous semantics. Shared id lookup/exposure and local splash preserve simultaneous damage. Cap one validates cap/radius then performs no splash enumeration. Exact best distance/initiative groups retain stable identity ties and the original seeded choose, including singleton groups.

BFS stores privately owned ordinary dictionaries through read-only wrappers, with every shortest predecessor intact. Approach values are memoized only inside a single search, keyed by faction/source/range/target kind/cell; identity and initiative remain distinct. Screening is local and cached by goal cell only inside one route call. A bounded board-owned cache reuses immutable goal-cell sets keyed by faction/range/target kind/cell; all-goal and legal-goal sets stay distinct for direct callers. It clears with the routing lifetime. No approximate route, observation hash, new seed call or persistent occupancy route cache is used. Long-lived frozen configuration maps remain.

All five ordered references, independent shortest-parent/ownership checks, reverse-order ties, configured defense splash and the complete 533-test cheap gate passed (`logs/20261003-002300-9e429d40`). Intermediate counter-off stepping: co-op 2150.76 ms, 512 actors 30559.36 ms, versus baseline medians 3022.91 and 53428.08 ms. Allocation was 5460.63 MB for 512 actors versus 10156.14 MB. Evidence `logs/20261003-002425-profile-campaign-377593f5`, `logs/20261003-002441-profile-scale-4d6ef99c`; separate counts `logs/20261003-002433-profile-campaign-ca20d8fd`, `logs/20261003-002546-profile-scale-9d0a95f1`. Co-op observation builds fell 333483→30587, examined actors 28448300→608433 and opponent visits 31573643→1482909. Search counts/ordered workload digests remain identical. These one-repetition area samples are directional; final repeated results govern performance claims.

### Snapshot/profile ownership and intermediate evidence

One canonical ECS view supplies every matching unit projection once to a complete snapshot. Each city reuses its soldiers for payload/forecast; standalone city snapshots remain detached and terminal match forecasts stay null. Food setup uses narrow authoritative soldier values. Private board/campaign/catalog templates avoid repeated lookup/sorting; returned neighbors, entries, recruits, quote arrays, routes/visited arrays and receipt ids remain independent. Returned event victims/decision arrays are detached from retained event history as well.

Profiles cache only successful type/rank/boss/level validation, including existing decimal/overflow/boss/research ordering. Quote caches are bounded to 27 supported rank combinations for one configuration owner; switching that owner clears them. Match disposal clears profile/quote caches. Wounded current HP stays unchanged by research. Commands read private building/research/wave data directly. City-distance lookup is immutable exact precomputation; its setup cost is measured.

The admission entry view belongs to one call, keyed by city/faction/size/front-or-rear. It shares exact availability (including no available position) only while occupancy revision is unchanged and clears after every placement. Every queued candidate is still considered in its original order; there is no event-driven scheduling or cross-stage admission cache. Cheap capacity/same-tick removal tests and all ordered references passed.

Complete cheap coverage passed 393 gameplay plus 144 tooling tests (`logs/20261003-003306-e81fb233`). Selected snapshot evidence `logs/20261003-003414-profile-snapshots-6142bc14`, separate counts `logs/20261003-003425-profile-snapshots-27a82fa1` matched ordinary/large input digests and exact JSON/compressed bytes. Each fixed-input codec phase is reported independently from projection; wire schema, enum values, Brotli settings and 16-MiB bounds are unchanged. Final repeated measurements remain required.

### Playback, presentation and HUD ownership

Playback owns detached, ordered current/previous units and their id union at accepted-snapshot boundaries. Frame sampling retains the same buffered admission/action/death policy, with pending deaths maintained as accept/drain counts. Baseline, gaps and fresh matches clear both queue and lookup. `PresentationEfficiencyTests`, existing `CombatPlaybackTests` and the five frozen ordered references own this behavior, including mutations of incoming routes/events and sampled outputs.

Views synchronize logical state, apply ordered events, then evaluate each visible rig once. Hidden views retain position/clip/death/effect state and cleanup, and seek their current pose when returning. Animation lengths/bones/names belong to the imported rig binding. See the [scaling review](simulation-performance.md) for the measured route-cache optimization and its subsequent retirement. The counter-enabled 600-frame replay asserts one full pose sample per visible retained view on every frame; counter-disabled measurements avoid those diagnostic assertions. All imported weapons, contact markers and animation assets remain unchanged.

HUD sections compare exact owned displayed values rather than revision alone or probabilistic hashes. Status, roster, economy, selected-slot context and controls refresh independently. Tokens include resources, research/quotes, slots, participation/food receipts, displayed army health/profile, focus, phase, connection and feedback. Movement and unchanged snapshots do not refresh those sections. Pure invalidation/mutation checks own same-tick updates; ordinary economy/reconnect and combat UI slices own actual controls and displayed balances.

The existing combat slice adds two city-selector clicks and fresh probes during its existing pause. It checks current visible bones/positions after return, unchanged visible ids/time, and no historical cues. Rendered animation checks select visible units because hidden skeletal evaluation is deliberately skipped; binding/authority/cleanup checks retain both cities. The subsequent resize baseline is taken after this focus visit, since the other city's formerly hidden rigs have now been sought. This adds no battle, matrix, scenario or display, remains under the existing 180-second bound, and catches stale rig/cue behavior that pure checks cannot observe. Existing reconnect coverage still checks current dying bodies, health and no historical effects after transport/process recovery.

### Campaign diagnostics and preparation attribution

Each strategy decision captures one before snapshot for policy, command and assertions, then captures fresh inputs after an accepted action. Serialized `SessionFlow.Invest` uses the same captured before state while retaining normal JSON requests and round-trip assertions. Earliest deployed participating level-five detection uses an authoritative scalar query; queued/reserve/dying cases are tested against the old projected predicate with no DTO projection.

Investment aggregation validates every addition regardless of verbosity. `CampaignDiagnosticsTests` checks all six resources, invalid/overflowing investments, lazy successful transaction formatting and a 64-record recent failure bound. Success output retains each preparation's exact stocks, army health/progression/participation, food demand/payment and reward/upkeep receipts, checked six-resource economic-command spending/income totals, production count and terminal stocks/receipts, plus final wave ticks, casualties, recruitment/trades/sales/expansions and earliest level-five wave. Failures expand recent domain records; credentials/wire payloads never enter this buffer. Set `ODOT_CAMPAIGN_TRACE=1` for detailed per-command output; profile identity records this switch. Keep it unset for matched performance comparisons.

The 21 strategy rows (four solo families at seeds 0/1/123 and frontline cooperative sizes 2/3/4 at those seeds), serialized twenty-wave campaign, ordinary xUnit discovery and no-argument in-process fallback remain required. Preparation/assertion/diagnostic/evidence phases explain harness changes separately from combat stepping; whole process CPU/allocation include all these phases. No extra parallelism or pacing is used to obtain gains.


### Matched network witness and refreshed verification baselines

The first optimized CI attempt exposed an existing transport race: a pending attack observed at tick 432 resolved before the pause acknowledgement at tick 436 at accelerated setup speed. Evidence `logs/20261003-010838-a472b864/` remains a failed gate, with cancellation and awaited cleanup. The resume slice now acknowledges ordinary speed around its pending/death witness, selects an impact at least six ticks ahead, then restores the configured setup speed while paused. Pending attack, typed recruitment, current deaths, exact frozen state and retry/recovery assertions remain unchanged. The selected corrected case passed (`logs/20261003-011354-3dad686b/`). No privileged command or arbitrary sleep was added.

For comparable test/CI timing, the archived unoptimized source was extracted into an isolated ignored checkout. Only the identical `NetworkTests.cs` witness change was applied; core, fixtures, codec, presentation, strategy and diagnostics remain unoptimized. Generated developer sessions are excluded from the archive and never copied; checks use owned fresh state. An untimed `prepare` warms restored/build/import inputs before measured commands. All compared locked tools, rule/expensive/UI budgets and default setup/witness pacing match. Audit: `logs/core-simulation-comparison/baseline-recheck-inputs.json`.

The refreshed before gates passed: `mise run test` 52.391s command / 49.267s runner, 383 gameplay plus 144 tooling checks; `mise run ci` 243.333s command / 241.953s runner, all six network/five source UI slices and sequential exports/headless/graphical package gates. Evidence lives under `logs/core-simulation-comparison/unoptimized-recheck/logs/20261003-011553-368961d6/` and `20261003-011643-1c2c3561/`. These are the final verification-speed denominators. Original frozen gate timings (55.895s/236.807s command) and the initial optimized pass/failing CI are retained separately under `original-before-gate/` and `initial-after-gate/`; they are not substituted into the final comparison.

### Final integration and performance results

All required runtime/counting families completed with stable source inputs per side and identical ordered outcomes: solo/four-player campaign commands and twenty wave checkpoints, all scale populations/checkpoints/events, fixed snapshot values/bytes and the 600-frame presentation script. Frozen references were not regenerated. Optimized runtime identity contains 597 hashes, digest `C8E0B868853DFA30B98C7BB986F5D575854CFD097D86B7EC08BB86289CFF1059`; both source archives and SHA-256 manifests remain under `logs/core-simulation-comparison/`. Exact comparisons and the independent audit are `analysis.json` and `equivalence.json` there.

Final timed cheap coverage passed 402 gameplay (380 general, 13 solo/serialized, 9 cooperative) plus 144 runner tests: 27.655s command / 24.528s runner (`logs/20261003-012511-b9237525/`). Full CI passed all six network scenarios, five source UI slices, sequential client/server exports and headless/graphical package smoke: 240.414s command / 239.033s runner (`logs/20261003-012537-756aaa9c/`). Locked restore/build/format gates passed. The final in-process fallback also passed 402+144 without test-host sockets (`logs/core-simulation-comparison/in-process-final.log`). Added coverage is 19 cheap regressions plus the bounded focus observations in the existing combat slice. All 39 runtime roots recorded in final command logs were removed after awaited cleanup.

The complete [runtime and verification comparison](simulation-performance.md#measured-results-2026-10-03) reports absolute deltas, percentages, three-sample medians/ranges, actual populations/per-tick costs, frame percentiles, every codec phase, work counts, overhead and one-trial gate limitations. Large stepping fell 119.56→7.70s over the identical 600 ticks; allocation fell 68.14→6.82GB. Cheap task wall fell 47.21%; whole CI changed only −1.20%, treated as practically unchanged. Managed presentation work improved while whole software-rendered process CPU barely changed. Large snapshot projection regressed 15.99% elapsed in the final samples despite lower allocation; it is explicitly retained in the report. No runtime benefit is inferred from CI success alone, and no native GPU/audio or complete stress-battle result is claimed.

## Research and status implementation verification

The before gate `mise run ci` passed at source `4a70e16a152beee112a775b1f11f08da4ebc2277` in 246.81 seconds, evidence `logs/20261003-071823-44909780/`. The clean source inventory and ownership reconciliation are retained under `logs/research-implementation/before.json`. This feature extends ECS-owned immutable actors and stage views, capability-aware profile caching, ownership-keyed recruitment quotes, and detached playback indexes. It adds no dependencies or assets.

Cheap research/status checks cover thirds income, overflow preflight, independent cities, catalog validity and locks, replacement mastery, wounds/levels, typed purchase/ledger/wire behavior, burn refresh/terminal ticks, poison cap/order/independent expiry, captured potency, chill boundaries, common damage, queued transfers, detached state and paused/history-gap playback. Existing ordinary campaign families, seeds, co-op sizes and upkeep assertions remain. Complete reference hashes are refreshed for the new protocol and combat fingerprint; they are a current research/status regression baseline, not a claim that the old rules fingerprint was unchanged. The fixed cooperative cleared-frontage witness now uses seed 90, selected with the complete original cooperative assertions intact. An old seed-zero detector fixture's city-owner error is corrected.

`mise run test-ui --scenario combat --ui-checkpoint research` selects the ordinary earned-point research witness independently. It uses the existing combat display and peer owners, at most wave ten, a 120-second setup bound and a 30-second feature bound. The default combat slice includes it after its existing specialist/action/casualty assertions, reusing the existing match. The melee selection and all five source ids remain available. Research opening pays ordinary plot/building/equipment/upkeep quotes and replaces its Stonecutter only after retaining enough stock; there is no injected point balance. Its incremental defects are global actual-input routing, acknowledged Fire purchase, visible permanent Frost lock, current burn badge and inspection, and frozen countdown evidence. Other branches use cheap numerical coverage rather than a graphical option matrix.

The final cheap gate passed 432 gameplay checks (410 general, 13 solo/serialized, nine cooperative) and 144 tooling checks in 28.34 seconds: `logs/20261003-091142-a6d64c85/`. Locked restore and `dotnet format Odot.slnx --no-restore` passed. The interrupted earlier cheap run has no complete result and is not counted as coverage.

Selected acceptance evidence:

| Command / slice | Result and measured cost | Evidence |
| --- | --- | --- |
| `test-network --scenario authority-resume-victory` | Passed; case 32.14s, task 38.36s. Earned research and active captured burn survive a paused owned process restart; purchase retry does not charge again. | `logs/20261003-081304-e60e52b9/` |
| `test-ui --scenario combat --ui-checkpoint research` | Passed; ordinary setup 37.13s, controls/status/reconnect 10.70s, case 53.64s, worker 57.02s, task 65.57s. Actual Reconnect restores the current paused badge/deadlines without historical flashes or sounds. | `logs/20261003-091219-03091f97/` |
| `test-ui --scenario economy` | Passed; case 96.51s, worker 99.89s, task 105.75s. Earned foundation, upgraded Research Tower, sale retention, Market and producer-rebuild assertions remain. | `logs/20261003-095752-3f76ecbb/` |
| `test-ui --scenario reconnect` | Passed; case 38.86s, worker 42.23s, task 47.44s. Existing picking, camera, death/body reconstruction and current-state recovery assertions remain. | `logs/20261003-091339-a9bd2769/` |
| `test-ui --scenario combat` | Passed; case 103.45s, worker 106.81s, task 113.02s. Research setup 28.82s and controls 9.49s. Retains existing specialist/action/pause/focus/health/casualty assertions and includes research with actual recovery in the same match. | `logs/20261003-093522-7b5266d7/` |

The before authority case took 13.81s and combat worker 64.87s; the observed increases are 18.33s and 41.94s respectively. These single runs are acceptance costs, not isolated performance benchmarks. The reused default match needs ordinary wave-three resupply: two Berserkers cost 40 metal and four food, and temporary recruitment buildings replace producers after their existing stock assertions. The Sword attack witness retains a fresh actual frame before pause; live health inspection chooses a currently damaged visible enemy. These timing choices preserve every existing role and casualty assertion. The default witness also demonstrates a towerless Fire purchase at wave ten. Selected research instead uses upgraded towers and reaches its purchase at wave eight.

Maintenance stays within the existing authority/combat/economy/reconnect scenarios, ordinary investment policy, observation selectors and owned peer/display cleanup. There is no extra scenario id, display, full graphical campaign or branch matrix. Cheap numerical tests own other branches and status combinations. The research panel and burn inspector PNGs were reviewed at 1100×820; the default combined checkpoint also retains 1280×720 evidence. Owned X11/software OpenGL/Dummy-audio frames establish rendered controls, rigs and labels; they do not establish native compositor/GPU performance, physical input or listening quality.

The first after CI attempt (`logs/20261003-091445-2e9a1ba6/`) failed in economy setup, with cancellation and owned cleanup completed. Building the Research Tower left five stone, so its ten-stone upgrade could not be clicked yet. The ordinary setup now waits for the next Stonecutter production before upgrading: the first tower production contributes one progress and the second two, funding the same foundation purchase alongside two shared-clear points. A selected follow-up (`logs/20261003-091753-e70fd7a3/`) passed that purchase/sale and exposed a later setup trade requiring unavailable metal, since the tower replaced two mine productions. Three remaining metal bundles and two surplus stone bundles now fund the final producer rebuild through ordinary Market quotes. No price, point grant or assertion was weakened. These failed runs are retained separately from the final gate.

An intermediate CI retry (`logs/20261003-092327-e5adf72b/`) was intentionally cancelled, with awaited supervised cleanup, to restore the economy witness's exact health comparison using a fresh pre-purchase veteran baseline after its intervening battle. Its partial passes are not a full gate result; final CI owns that strengthened assertion.

The next CI attempt (`logs/20261003-092502-8f3e3d55/`) passed rules, all six network cases and economy/reconnect/settings, then found a combat witness ordering race: live inspection and pose collection could finish after the stronger ordinarily equipped army ended the battle, leaving no future casualty. The default slice now captures its fresh focused-city casualty immediately after the existing pause/resume, checks frozen death and declared cleanup, then completes live inspection/recovery. A selected trial (`logs/20261003-093248-bf4fa29e/`) showed that awaiting graphical acknowledgement before observing death delayed pause to 23 ticks after the casualty, exceeding its unchanged 0.35-second freshness limit. As in the existing packed witness, the observer's ordered death-wait/pause driver is now armed before the client's Resume click. Actual rendered role frames collected during these stages remain evidence; no required animation, damage, death or cleanup assertion is dropped. Failed/partial CI attempts do not count as final coverage.

The following default-parallel CI (`logs/20261003-093804-6db596d8/`) encountered a missing fresh settings UI response with the native display dropdown open. No engine error was recorded; the cause remains unestablished. The unchanged selected settings case passed in 27.98s (worker 31.35s, task 36.62s), evidence `logs/20261003-094247-5fd4eaa9/`. Final full CI uses the supported `--ui-jobs 1` graphical cap, retaining the total expensive budget of two and every scenario/assertion/deadline. This verifies complete coverage with serial graphical admission; it does not establish that the intermittent default-parallel timeout is resolved.

The first serial-UI attempt (`logs/20261003-094407-cec85e81/`) found that fixed repeat-trade counts still assumed a particular number of replacement recruits after the unresearched second battle. Economy setup now uses bounded ordinary trades from actual surplus metal/stone/cloth/food and the published Market rates to meet quoted upgrade/rebuild gold costs. It reserves the next recruit's equipment, forecast upkeep and the required graphical food-sale bundle. A subsequent selected trial (`logs/20261003-095427-84c7376f/`) also exposed that the earlier chosen veteran could die in that intervening battle; the exact purchase/sale health witness now selects a living wounded survivor immediately before buying. The actual Metal/Food button witnesses, equipment charges, mixed levels, exact wounds, expansion and producer-rebuild assertions remain. It grants no privileged resources and adds no battle or scenario.

The next full attempt (`logs/20261003-100042-753361d8/`) passed economy at 102.44s and all rules/network scenarios, then a restarted reconnect client exited 139 with `Invalid Program: attempted to call a UnmanagedCallersOnly method from managed code.` No useful stack trace was emitted. The same unchanged reconnect slice passed at 38.78s (worker 42.15s, task 47.68s), evidence `logs/20261003-100609-59b1c275/`. A [Godot issue reports the same failure message](https://github.com/godotengine/godot/issues/82414); that similarity does not establish the cause here. No tool, dependency, deadline or assertion was changed to obtain the recheck. Native/runtime stability remains a limit of this evidence.


Final full `mise run ci --ui-jobs 1` passed in **435.31s** (436.88s command wall), evidence `logs/20261003-100929-587a10b4/ci-summary.json`. The complete set includes 432 gameplay plus 144 tooling checks, all six network scenarios, all five source UI scenarios, sequential client/server exports, headless package smoke and graphical exported-package smoke. Total expensive admission remains two; graphical admission is one. This is complete coverage with that cap, not a default-parallel stability claim or a directly comparable timing benchmark against the 246.81s before run.

Final worker timings: economy 99.96s, reconnect 41.28s, settings 31.23s, launcher 82.38s, combat 102.43s; research's reused setup/control phases were 28.73s/9.38s. Network coverage took 43.54s and source UI 379.97s including admission. Client/server exports took 9.00s/3.95s, headless package smoke 1.56s and exported graphical worker 15.28s. Locked restore/build/import and format verification passed. Source identity is retained in `logs/research-implementation/after-inputs.json`; failed/cancelled attempts remain separate. No dependencies/tools were changed, no developer preferences or unrelated processes were altered, and no package was published or uploaded.

Strict OpenSpec validation passed for `add-city-research-and-status-effects`, and all forty implementation tasks are complete. All 25 runtime roots recorded in final CI output were removed after owned cleanup; code hashes still matched the gate-start manifest. Only documentation and OpenSpec artifacts changed after the successful gate. All seven capability deltas were synced to the main specs, which passed strict validation, and the change was [archived on 2026-10-03](../openspec/changes/archive/2026-10-03-add-city-research-and-status-effects/).


## Simplified building economy verification

The fresh before `mise run ci` attempt failed after 49.19s in the concurrently edited workflow launcher's `AgentLauncherTests`, with supervised cancellation of network/UI work. Evidence: `logs/20261003-114046-f3e45efa/ci-summary.json`. The workflow proof of concept was subsequently removed in another user session; that removal is preserved. This failed attempt is not a successful baseline. Earlier successful CI at `logs/20261003-105110-3185d5b0/` used different source inputs and is not substituted for it.

Cheap checks cover exact default quotes, the funded six-Swordsman opening, whole equipment rounding/custom plateaus, exchange/reward atomicity, explicit zero-wood recovery eligibility and actual-investment refunds, ledger retries/wire defaults, nullable income capacity/overflow, income invalidation, phase-aware upkeep and exact food-sale forecasts. All existing 21 twenty-wave solo/cooperative campaigns and their paid-equipment/upkeep, trading, full expansion and city-survival assertions remain. Combat event/output reference hashes are unchanged; two snapshot reference hashes changed for the new economy and projection fields.

The existing `economy` graphical slice adds actual recovery input, authoritative income checks after construction/upgrade/sale, the before/after exact food-sale shortage, current-wave paid receipt, foreign-city and paused/disconnected/reconnected observations, and inspector bounds at 1280×720 and 1100×820. The combined resource/upkeep panel rejects world wheel/drag initiation. Pure tests cannot establish rendered control routing, full panel bounds or reconnect label refresh. Setup adds one bounded ordinary wave-three clear and two productions to fund the shortage sale; it adds no scenario, peer, display or full graphical campaign. Maintenance uses existing selectors, fresh observations, ordinary quotes and owned cleanup.

Selected `mise run test-ui --scenario economy` passed: case 104.74s, suite 113.29s, command 115.28s, evidence `logs/20261003-120036-2c0ae305/`. Recovery/income, food shortage and both inspector-size PNGs were inspected. A subsequent compact-column adjustment and upkeep input-protection extension are verified by the final integration gate. Frames use owned X11, software OpenGL and Dummy audio; native GPU/compositor performance, physical input and listening quality are outside this evidence.

Locked solution restore and `dotnet format Odot.slnx --no-restore` passed without dependency/tool lock changes. Final standalone cheap checks passed 438 gameplay checks (416 general, 13 solo/serialized, nine cooperative) and 145 runner checks in 27.19s runner time. Evidence: `logs/20261003-120731-17d41407/` and `logs/economy-final-cheap.log`.

The first after CI attempt failed in 38.16s (`logs/20261003-120815-0fda736c/ci-summary.json`): the authority/recovery network fixture assumed plot four remained empty after the first opening. Lower basic costs now fund its Stonecutter earlier. The Crossbowman witness buys plot seven at the ordinary authority quote and builds its Archery Range there, retaining the producer and all exact equipment, duplicate/restart, pending-action and research assertions. Failed partial coverage is not counted as the final gate; cancellation awaited owned peer/display cleanup.

The selected authority/recovery network recheck passed in 22.62s (27.70s task), evidence `logs/20261003-121001-76e2f76c/`. The next full CI passed all cheap tests and all six network cases, then its randomly seeded economy setup lost the added wave-three battle; evidence `logs/20261003-121057-a05f0726/`, 152.13s failed gate. The original setup only required two clears. Economy now freezes seed `14056307608042553509`, captured from the successful selected three-clear witness, so its ordinary paid army, veteran wounds and bounded shortage setup are reproducible. No production, combat profile, equipment price or acceptance assertion changes. This UI seed is fixture evidence, not additional balance proof; the unchanged 21-campaign seed sample owns balance coverage.

### Final integration follow-up, 2026-10-03

The task-5.3 full recheck at source `0e1d4b7a64f431448294d3020affa534b1933664`
failed in **173.95s** before exports. It passed all 438 gameplay and 145 runner
checks, all six network cases, reconnect and settings, but the launcher route
could not activate Crossbowman recruitment: after replacing its Metal Mine with
an Archery Range it had one wood, while the new equipment quote requires two.
Economy was cancelled, not passed; combat had not run. Evidence:
`logs/20261003-122729-dd6e1cd5/ci-summary.json` and
`logs/economy-final-integration/ci.log`.

The minimal correction sells the launcher's Farm through an ordinary accepted
command after its third production, refunding the missing one wood. This route
starts no battle and requires no further production; its 15 food still covers
the two soldiers. Actual Farm purchase, Metal Mine sale, Archery Range purchase,
both recruitment controls, menu/solo transitions, hosted lifecycle, both-size
geometry and cleanup assertions remain. The new cheap
`LauncherOpeningFundsMeleeAndRangedRecruitmentBeforeBattle` regression freezes
that complete paid opening, three productions, final stocks (18 gold, 15 food,
zero wood, nine metal), both archetypes and two-food demand. A discarded trial
adding a second Lumbermill failed world picking at plot one in the selected
launcher route (`logs/20261003-123702-92bcb730/`); that trial is not shipped or
counted as passing coverage. The final refund-funded selected launcher passed:
case **77.74s**, worker **81.08s**, task **86.27s**, evidence
`logs/20261003-124126-507e605b/`. Its paused solo PNG was inspected. Cheap checks
passed **439 gameplay and 145 runner cases** in **27.87s** runner time,
`logs/20261003-124015-bc1737be/`.

The next full attempt (`logs/20261003-124328-e7f537e7/ci-summary.json`) failed in
**248.71s**, before exports. It passed all cheap/network checks and source
economy, reconnect, settings and launcher. In particular, the fixed-seed economy
worker passed in **124.76s** with the additional wave-three clear and shortage
setup. Its recovery/income, food-shortage and both-size inspector PNGs were
inspected: the compact stack and scrollable inspector remain separated from the
bottom HUD, and the shortage reads three soldiers sitting out at two food versus
five demand. These passes verify the previously unconfirmed seed and compact
column/input-protection changes, but the failed full run is not final acceptance.
Combat failed while opening the live damaged-enemy inspector; by the deadline
all enemies were gone. Selecting the lowest-health live target after resuming
allowed it to die between probe and input. The existing casualty pause now owns
selection and opening, then the ordinary observer Resume preserves the open
inspector for live damage/casualty assertions. Earlier actual graphical
Pause/Resume and outside-Pause dismissal, death freezing, declared cleanup,
recovery and animation assertions remain; no deadline, gameplay rule, scenario,
peer or display was added.

Owned cancellation/cleanup completed for these failed and selected runs.
Independent evidence inspection found no remaining owned runtime directories or
live owned display groups/game processes; reports are
`logs/economy-final-integration/failed-ci-cleanup.json`, `trial-cleanup.json`,
`launcher-cleanup.json` and `second-ci-cleanup.json`. These diagnostic inspections
supplement, rather than replace, the checked-in runner's awaited cleanup.

At this stage, the combat follow-up was **partial, not accepted**. Cheap checks
after moving selection passed 439 gameplay and 145 runner cases in 28.61s,
`logs/20261003-125045-ff9ede4d/`. The first selected trial
(`logs/20261003-125127-02ad6e8e/`, case 68.83s) opened the paused damaged-unit
inspector but failed declared death cleanup: the diagnostic mistakenly sent
`pause` rather than the explicit `resume` command, and a historical unpaused
snapshot satisfied its wait. The correction sends `resume`, then an ordinary
rejected command to obtain a fresh authoritative acknowledgment before requiring
unpaused state. That selected trial (`logs/20261003-125427-bfb5aeca/`, case
56.23s) passed declared death-model removal and reservation release but failed
the unchanged two-unpaused-second visual cleanup bound. It therefore does not
establish the complete combat assertions. Owned cleanup reports are
`logs/economy-final-integration/combat-trial-cleanup.json` and
`combat-resume-cleanup.json`. At that point task 5.3 remained unchecked; exports
and package smokes were not executed by either full attempt above. Neither attempt establishes
archive readiness or final integration success.

### Combat cleanup diagnosis and retained bound

The initiating trigger for the inspection failure was resuming a short, already
wounded fight before choosing its lowest-health enemy. The masking condition was
probe/input scheduling: the enemy could die before the native click. Paused
selection followed by explicit ordinary Resume preserves that target's identity
until inspection opens, without changing combat or extending its life. Actual
live damage or its ensuing casualty is still required after Resume.

The later two-second symptom was a separate measurement defect. Historical code
at `cef9674` measured `cleaned.VisualSeconds - casualty.VisualSeconds` at the
first *read* of an absent model, not at removal. `CombatPlayback` freezes visual
time during pause; `Tabletop.UpdateUnits` removes expired views at its current
sampled tick. In the failed trial the read reached tick 953 versus death end 910,
and all sampled frames agreed with the declared model/tick boundary. This alone
did not establish the earlier removal instant, so temporary renderer-side
instrumentation was added and the same selected source case was reproduced.

Disconfirming evidence is retained: the instrumented unchanged case passed at
`logs/20261003-130158-8f3fda76/` (case 91.09s, task 100.00s), with actual watched
removal at tick 916/end 910 and 0.8 unpaused visual seconds. The smallest
counterfactual changed **only the verification read**: ordinary observer ticks
postponed it until at least death end + 120. That run
(`logs/20261003-130447-3a10a5da/`, case 56.97s) failed the original read-time
assertion although the renderer removed unit 44 at tick 977/end 976 after
**0.8 seconds**, while the later read was tick 1104 after **2.793 seconds** with
no model present. Exact values are retained in
`logs/economy-final-integration/delayed-read-diagnosis.json`; the instrumented
engine logs retain actual removal events. This counterfactual accounts for both
the passed and failed paths without blaming paused-time accounting or changing
the accepted two-second limit. It does not claim every possible runtime stall
will satisfy that limit.

The permanent correction retains up to 64 renderer-side death-removal witnesses
(id, declared deadline, actual sampled tick and visual time), cleared on fresh
session or playback generation. Fresh UI observations require the watched model
absent, its current-session witness at/after its deadline, and its **actual
removal within the same exact two-unpaused-second bound**. Existing per-probe
model/deadline equivalence and authoritative reservation release remain. The
controlled read delay and temporary console instrumentation were removed.
Four cheap runner cases cover delayed-read independence and wire retention,
exact two seconds versus 2.001-second rejection, and absent/wrong-id/wrong-deadline,
premature, still-rendered or negative-time witnesses. No timeout or acceptance
assertion was dropped or relaxed; no new scenario, display or graphical match
was admitted. The bounded diagnostic observation adds no gameplay/protocol state.
The existing fresh-session UI assertion additionally checks witness reset.

Locked restore and C# formatting passed. Cheap checks passed **439 gameplay and
149 runner cases** in **26.48s**, `logs/20261003-130857-54f16118/`. The corrected
selected combat passed: case **92.01s**, worker **95.32s**, task **100.88s**,
`logs/20261003-130943-daf5ab22/`. Its `combat-death-cleanup.json` records unit six
removed at tick 915/end 910 after 0.8 seconds; the fresh probe arrived at tick 927
after 1.0 seconds. Live inspection, animation/recovery, casualty, research and
fresh-session assertions passed. Owned cleanup for the diagnostic, delayed-read
and corrected checks is retained in `combat-diagnostic-cleanup.json`,
`combat-delayed-cleanup.json` and `combat-proof-cleanup.json` under
`logs/economy-final-integration/`; none retained owned runtimes or live processes.

### Final full gate: passed

Full `mise run ci` passed in **296.18s** (**297.53s** including mise bootstrap):
`logs/20261003-131156-23e7ae2e/ci-summary.json`. The complete console record is
`logs/economy-final-integration/accepted-ci.log`. This is the final task-5.3
acceptance, not a substitution of earlier partial or diagnostic runs. Tested
source is based on `0e1d4b7a64f431448294d3020affa534b1933664` plus the documented
launcher/combat verification corrections. The per-file source/asset hashes in
`logs/economy-final-integration/accepted-source-sha256.txt` were unchanged
through the complete gate (`accepted-source-unchanged.txt`). Locked .NET SDK
10.0.401 and Godot .NET 4.7.2 were used; no tool/dependency locks changed.

The gate passed **439 gameplay checks** (417 general, 13 solo/serialized and nine
cooperative) and **149 runner checks**, without failures or skips. All 21
ordinary twenty-wave strategy samples and their cooperative paid-army assertions
remain. All six existing network scenarios and all five existing source UI
scenarios passed under the shared two-scenario budget, graphical cap two and
owned setup simulation speed four; actual graphical timing witnesses use speed
one. Source preparation occurred once. Every source gate preceded the sequential
client/server exports; both headless and graphical package smoke passed.

| Final phase/scenario | Seconds |
| --- | ---: |
| Locked restore / format verification / build / import | 1.00 / 17.84 / 1.03 / 2.94 |
| Cheap C# suite, concurrent partitions | 27.77 |
| Network authority-resume-victory / redistribution | 22.54 / 22.67 |
| Network defeat / failure-cases / solo-session / playing-host-lifecycle | 2.78 / 3.57 / 0.92 / 6.10 |
| Network suite including shared admission | 108.20 |
| Source economy case / owned worker | 111.44 / 114.74 |
| Source reconnect case / owned worker | 38.30 / 41.61 |
| Source settings case / owned worker | 27.24 / 30.54 |
| Source launcher case / owned worker | 83.16 / 86.47 |
| Source combat case / owned worker | 102.03 / 105.35 |
| Source UI including shared admission | 242.62 |
| Sequential client / server export | 6.07 / 5.00 |
| Headless package smoke | 1.56 |
| Graphical package case / owned worker | 12.90 / 16.22 |

The final economy shortage and both-size inspector PNGs and packed animation PNG
were inspected under this run's `economy-worker/economy/` and
`exported-package-worker/exported-package/`. They preserve the compact readable
resource/upkeep stack, scrollable inspector bounds, real food consequence and
packed wave-tagged payment. The combat removal witness records unit six removed
at tick 911/end 910 after **0.6 unpaused visual seconds**, not a probe-time guess:
`combat-worker/combat/combat-death-cleanup.json`.

The runner awaited every owned child/display cleanup before reporting success.
Independent final evidence inspection checked **25 owned runtime directories**,
**six owned display process groups** and **74 child cleanup checkpoints**, finding
no remaining runtime directories or live owned game/runner/display processes.
Evidence: `logs/economy-final-integration/accepted-cleanup.json`. Only this run's
owners were inspected; no developer preferences or unrelated processes were
changed. Generated outputs/evidence remain ignored, and nothing was uploaded or
published. Later documentation/task/archive edits do not change the tested game
inputs, so another local full game/export run is unnecessary.

Coverage is Linux x86_64 on owned X11/software OpenGL with Dummy audio. It does
not establish native GPU/compositor performance, physical input or listening
quality, native Windows packaging/installer qualification, real two-account
Steam acceptance, or release publishing. Those external/prerequisite-dependent
checks and their archived records remain unchanged.

### Hosted source cancellation diagnosis, 2026-10-03

GitHub run `37129710193`, source job `111222216058`, reviewed head
`cc710107ba1cdf9c4c0cbfffac9f77f3e661cc9e`, was cancelled by the **15-minute job
limit**, not by an operator or a superseding push. The check annotation explicitly
reports the maximum execution time. Retrieved annotations, job metadata and the
complete source log are retained in ignored `logs/ci-cancellation-diagnosis/`.
Locked restore, formatting, build/import, all 439 gameplay and 149 runner checks,
five source UI scenarios (575.47s including admission), and five network scenarios
passed. Redistribution reached its transfer identity/damage/recovery/food assertions
but never completed the subsequent exact-revision peer wait. This is incomplete
network coverage, not a passing source verdict. Linux and Windows package jobs
on that head independently passed.

The exact transfer comparison now uses an ordinary acknowledged pause instead of
requiring a guest to retain one transient unpaused combat revision. It still
compares complete serialized snapshots exactly, then resumes before the existing
wave-clear, observer recovery, future allocation and protected-admission assertions.
No assertion or scenario was removed. The source workflow limit is now 20 minutes,
leaving setup, preparation and awaited owned cleanup outside the runner's existing
900-second source budget; runner limits, concurrency and rerun policy are unchanged.

Locked restore and `dotnet format Odot.slnx --no-restore` passed. Standalone cheap
checks passed 439 gameplay and 149 runner checks in 26.05s
(`logs/20261003-145417-d679f4d6/`). Selected redistribution passed in 22.58s
(31.65s including preparation), `logs/20261003-145452-24ce0e34/`.
The complete affected workflow command,
`mise run ci-source --startup-timeout-ms 60000`, then passed in **258.57s**
(**259.93s** including mise bootstrap):
`logs/20261003-145537-e816d7cd/ci-source-summary.json`. It includes locked
restore/format/build/import, all 439+149 C# checks, all six network cases and all
five source UI cases, with two total expensive slots and graphical cap two.
Network took 114.99s including admission (redistribution 22.57s); source UI took
235.54s. Owned economy/reconnect/settings/launcher/combat workers took
113.81/44.65/31.51/84.65/99.15s. No source stage failed or was skipped.

The successful command awaited child/display cleanup. Independent evidence
inspection found all **19 owned runtime directories** absent, **60 child cleanup
checkpoints**, and no live processes referencing those owned runtime/evidence paths.
Report: `logs/ci-cancellation-diagnosis/cleanup.json`. Verification data stayed
inside this worktree; no developer preferences or unrelated processes changed.
This source-only run does not claim fresh export/package coverage or a replacement
GitHub check. Unchanged package coverage remains the passing hosted jobs and the
full local gate recorded above. The outer pipeline must publish the corrected
head and obtain its replacement passing GitHub source verdict; no check was waived.

### Hosted combat locomotion timing repair, 2026-10-03

GitHub run `37135250968`, source job `111238369073`, head
`c2298c21f76f01dda2f4e3dacae546299a9bf89f`, failed the combat slice at
`actual locomotion pose`; launcher was cancelled by the resulting suite cleanup.
The source fixture resumed after its paused Catapult capture, before a pacing
acknowledgment and the first locomotion probe. Those round trips could consume
the remaining movement window. The local before slice passed (91.58s scenario,
104.40s runner), so the hosted deadline was not reproduced locally.

The fixture now retains the tower pause through the return-to-own-city and first
locomotion observation, then resumes before the existing position-and-bone-change
assertion. A live observation assertion requires that first source witness to be
paused. No deadline, retry, gameplay rule, scenario or existing assertion changed.
Locked restore, formatting and all 439 gameplay / 149 runner checks passed.
Selected combat passed in 94.18s (101.95s runner), with evidence at
`logs/20261003-161700-2a5bb311/`.

The exact affected hosted command,
`mise run ci-source --startup-timeout-ms 60000`, passed in **272.60s**
(**275.10s** including mise bootstrap), with locked restore/format/build/import,
all 439+149 C# checks, all six network scenarios and all five source UI slices.
Evidence: `logs/20261003-161907-64fa0421/ci-source-summary.json`; console logs
and cleanup inspection: `logs/ci-combat-diagnosis/`. Network took 113.90s
including admission; source UI took 246.89s. Combat's owned worker took 100.99s
and passed both the paused first witness and ensuing live movement/pose checks.
All 19 owned runtime directories were absent after awaited cleanup, with 60 child
cleanup checkpoints and no remaining processes referencing their runtime/evidence
paths or owned display-worker groups (`cleanup.json`).

This is Linux private-X11/software-OpenGL/Dummy-audio source coverage, not a new
full export/package gate or a replacement hosted verdict. No packages were rebuilt,
published or uploaded. The outer executor still owns publication and the required
GitHub checks on the corrected head.

## Clarified combat presentation verification

The [gameplay presentation contract](gameplay.md#deterministic-hex-combat) owns direct committed steps, pose transitions and melee cue semantics. The rendered gate retains the 0.4-world-unit clearance assertion for settled roots, including settled corpses; committed transit (including a frozen transit casualty) is exempt from visual clearance, not numerical reservation checks.

Cheap tests exercise the same pure presentation timing module used by rendering: clamped direct positions, monotone segment progress, frozen movement, walk envelopes that never select sprint, shortest-angle/frozen facing, full attack weight at the unchanged authored marker, reduced filtered hit layering and terminal precedence. Runner regressions reject collapsed settled roots, old beam cue styles, invalid target linkage, detoured/sprinting live motion and static/different-identity attack samples. All five existing `ReferenceTraceTests` hashes remain unchanged: target/route ties and transfers, simultaneous impact/death/admission, ordinary playback input/output and pause/resume terminal cleanup. The first cheap iteration failed only a new exact floating-point marker assertion; the assertion now checks twelve decimal places without changing the underlying pose calculation. Complete cheap checks passed 438 core and 161 tooling tests in 26.57s (`logs/20261003-135740-bb87a344/`). No numerical gameplay, wire identity, assets, tools or dependencies changed.

The existing independent command remains `mise run test-ui --scenario combat --checkpoint melee --simulation-speed 1 --trace`. It launches the ordinary authority with combat seed 1 and owns the same two peers and display. A paid Farm/Metal Mine/Barracks opening equips six Swordsmen, with ordinary recruitment continuing into wave two when needed; the proof must finish before wave three. It retains all near/far/shared/simultaneous assertions and the four paused overview/close windup/impact captures, and adds no scenario or graphical campaign. A bounded progression phase retains at most twelve fresh live-node observations and four PNGs, with wall timestamps, combat ticks, action identities, positions, facing, actual tree blend parameters and imported hand-bone poses in `combat-progression.json`. Each light probe and PNG observes a fresh rendered frame through the existing protocol; they are not animation commands or a replay buffer. The meaningful extra regression is a renderer still detouring/sprinting, using a beam or failing to advance its imported rig despite correct pure timing/endpoints.

PNG capture/encoding on software rendering is slower than a thirty-tick move. The first sequence attempt (`logs/20261003-135846-60ac0620/`, failed) captured only widely separated PNGs and could not prove one movement identity advancing. The checked-in witness now pairs fresh lightweight node observations before PNG capture, preserving authority speed one and every numerical tick; it never slows the game to manufacture a witness. A selected pass (`logs/20261003-140347-930ad46e/`) retained twelve observations and four progression PNGs in 4.09s, plus the four near/far paused captures and 36 linked witnesses. Case/worker/task/command times were 36.44/36.63/45.28/47.29s. Direct progression and overview/close windup/impact frames were inspected; the first warm-tinted intention was too faint at overview, so final rendering uses darker disconnected ground dashes, not thicker spanning bars. The final darker-cue check passed: `logs/20261003-141156-9f42eccb/`, twelve fresh progression observations/four PNGs in 5.83s, 30 linked witnesses and all four paused overview/close captures. Case/worker/task/command times were 37.50/37.71/46.42/47.82s. All eight final progression/near-far PNGs were inspected; the close windup shows separated high-contrast dashes and the impact shows local accents rather than full-span geometry.

The selected reconnect check passed in 38.38s (worker 38.58s, task 46.76s, command 48.13s), evidence `logs/20261003-140553-97ea24a7/`. Camera/disconnected inspection, actual reconnect input, current death/body reconstruction, frozen rig/position/heading state and complete numerical reservations remain asserted.

Before implementation, full `mise run ci` at `0e1d4b7` failed in 168.41s (`logs/20261003-134019-2fcd8804/`): launcher could not select `RecruitRanged` after economy setup. All six network cases, settings and cheap partitions passed; economy was cancelled and remaining source/export/package coverage was incomplete. The independent frozen-reference test passed (`logs/autobattle-ship/before-reference.log`). This is a recorded failed baseline, not a substitute passing gate. Economy fixture corrections are owned by main and must be reconciled normally before final complete validation, rather than duplicated by this slice.

Owned X11/software OpenGL/Dummy audio establishes actual geometry, current rigs, linked cues and sampled clock progression. PNG timestamps include capture/encoding stalls and are not a frame-throughput benchmark; sparse images alone cannot establish real-time perceptual smoothness. Transient model overlap can still obscure a body, and overview cues remain deliberately small. Native GPU/compositor performance, physical input, listening quality and universal crowd readability are not established. No captain manual review is required for this automated delivery gate.

The full selectable default combat slice also passed: case 97.14s, worker 97.36s, task 105.49s and command 106.93s, `logs/20261003-141244-952f6bdb/`. It retains the existing ranged/Mage/axe/hit/tower/health/inspection/pause/focus/casualty/research/reconnect/fresh-session assertions, rather than substituting the all-Sword checkpoint for those roles. The two presentation deltas were synced to `city-tabletop` and `game-feedback`; strict change and all eighteen main-spec validations passed. Source checks here are selected coverage, not a complete after-CI/package result.

The after `mise run ci --ui-jobs 1` attempt at presentation commit `8f38cfa` failed in 54.69s, `logs/20261003-142036-a90253d8/`. `authority-resume-victory` rejected a `ready` during the transition from Preparation to Combat at wave seven: `Only living cities can act during building.` This message also guards invalid phases; the retained trace has **both cities alive at full health**, not eliminated. At seed `17837719233072821886`, accepted repeated Ready requests at ticks 2254/2258/2263/2267 were followed by automatic battle entry at 2269 and the rejection at 2271. The before successful network case used a different seed (`5635161317642887838`). Gameplay core, authority scheduling and this network/research driver are unchanged by the presentation slice. Restore/format/build/import, solo/cooperative/tooling partitions and redistribution passed, but general rules and source UI were cancelled and exports/package smokes were not reached. Awaited owner cleanup completed. This failed/partial gate is not accepted as final integration coverage; normal synchronization of main's pending fixture corrections and complete passing CI remain required.

A final witness review tightened attack progression to require an equipped rig actually sampling one of the four attack clips; hit-only or unarmed bone changes cannot prove an attack. Its negative cheap regressions passed with all 161 tooling tests. The affected owned melee recheck passed (`logs/20261003-142830-078a120b/`): twelve observations/four progression PNGs in 4.17s, 28 linked witnesses, all four overview/close captures, case/worker/task/command 37.56/37.81/45.54/46.94s. No rendering or numerical code changed for that assertion-only follow-up. Main remained at `0e1d4b7` when that integration hold was recorded; neither fixture had been patched here at that point.

### Research readiness fixture diagnosis and bounded correction

Firstmate follow-up `003` authorized diagnosis of the distinct readiness failure, not duplication of the pending launcher/economy correction. The operator-visible failure is the checked-in CI/network command aborting during ordinary earned research setup, before exports/package smoke. The original research driver and authority were replayed through `mise run test-network --scenario authority-resume-victory --trace`, temporarily supplying only the retained failing combat seed `17837719233072821886` to the existing server launcher. No gameplay, readiness driver, timing defaults, deadline or assertion was changed for reproduction. The temporary network seed pin was subsequently removed; the cheap regression deliberately retains that seed. The correction is the barrier, not a permanent network seed selection.

The failure reproduced in `logs/20261003-144245-0a19840a/`: case 23.34s, runner 32.17s, command 37.60s. The retained A trace shows an accepted Ready at tick **2266**, revision **2465**, Preparation/turn serial **34**, both cities already ready, and one retained death ending at **2269**. At tick **2271**, revision **2472**, Combat/serial **35**, a fresh Ready is rejected with the same message. Both cities remain living; the message's phase guard, not elimination, rejects it.

- **Initiating trigger:** the fixture treats the last accepted Ready acknowledgement as resolution of the submitted stage (`observed serial >= acknowledgement serial`). While death cleanup blocks battle entry, that predicate accepts the still-pending Preparation snapshot, so the loop submits another Ready rather than awaiting the already-requested transition.
- **Masking condition:** when cleanup has finished before the final Ready, that acknowledgement already contains the next serial and the same predicate happens to work. Whether the retained-death window overlaps the fast setup/Ready loop exposes the defect; this is not a slower-authority or longer-deadline requirement.
- **Visible symptom:** automatic cleanup enters Combat between iterations, and the next ordinary Ready string is encoded against current Combat state and rejected. Thus the error need not be a stale-command rejection and does not establish a dead city.

History/blame places the acknowledgement-only barrier in the research driver added at `cef9674`, before this presentation work. The proven core path (`CleanupBarrierAndAcceptedReadinessRetryDoNotChargeFoodEarly` and ordinary strategy loops) explicitly lets the clock complete cleanup while readiness remains accepted; strategy loops do not keep readying already-ready cities. The earliest erroneous fixture divergence is declaring the pending acknowledgement resolved, not damage, target selection, peer restart or research purchase.

The smallest counterfactual requires **observed serial > submitted stage serial**, retaining the acknowledgement revision high-water mark, and awaits that result on each of the same two peers. `ResearchReadyBarrier` is the shared engine-free predicate used by the driver and cheap regression. `ResearchReadinessTests` uses ordinary paid two-city commands and a real first-wave clear, without combat setters or privileged grants. Its retained-body case fails with the old predicate and passes with the corrected one; a cleanup-completed control passes with both. It checks repeated accepted Ready/revision changes do not resolve the stage or charge upkeep, the actual final death deadline does resolve it, both cities remain living, and the obsolete next Ready produces the exact reported rejection. Immediate resolution must also pass at the acknowledgement's own serial (not wait for yet another stage), while older peer revisions, foreign matches and unadvanced observations are rejected. Red/green logs: `logs/research-readiness/{red,green}.log`, two tests, no Godot process.

With only that synchronization correction and the same diagnostic seed, the existing complete network scenario passed all retained ownership/refusal, retry, research/active burn, pause/restart, cooperative agreement, rate limit and endpoint/lifecycle assertions: `logs/20261003-145154-f1347cad/`, case 30.89s, runner 35.56s, command 37.00s. All owners cleaned up. Disconfirming checks reject elimination as a cause, retain correct cleanup/upkeep behavior, and show that already-completed cleanup needs no artificial wait. Neither numerical authority nor the 120-second/wave-ten research bound was changed. This is a fixture synchronization correction, not a product readiness-policy change or another expensive scenario.

After removal of the diagnostic network seed pin, the ordinary unseeded scenario also passed: `logs/20261003-150149-a6198f09/`, case 30.44s, runner 35.08s, command 37.18s. Changed C# was formatted after locked solution preparation. Complete cheap checks then passed all **438 core / 163 tooling** tests, including unchanged frozen references, in 27.72s: `logs/20261003-150816-90a07bc8/`. Final test-only strengthening uses fresh command sequences and asserts exactly one stage advancement plus forecast-matched, once-only upkeep on both cities. Its first compile flagged the nullable forecast; asserting the required forecast is present resolves that diagnostic without weakening the payment check (`logs/research-readiness/forecast-nullability.log`). All 163 tooling tests passed afterward (`logs/research-readiness/final-tooling.log`). These are selected network/cheap results, not complete integration/export/package coverage. At that point main still resolved to `0e1d4b7`: the independent economy launcher correction was a **known synchronization wait**. No competing economy fix, publishing or delivery-pipeline launch was made. The subsequent synchronization and complete gate below clear that wait.

### Final synchronized presentation gate: passed

Firstmate follow-up `004` cleared the dependency after the economy correction landed in main at `227edbf` ([PR #1](https://github.com/seraph1nia/odot-game/pull/1)). The feature branch was safely rebased onto that commit. Only documentation conflicts needed manual resolution: both histories and the current presentation contract were retained. Range-diff preserves the research correction `cd91530` equivalently as `c49b361`, and the equipped-attack assertion follow-up is unchanged. Upstream launcher, paused inspection, measured death-removal and exact paused redistribution assertions were retained rather than duplicated. Compared with that base, numerical authority, core tests/frozen references, `Main.cs`, dependency/tool locks, assets and the ordinary `NetworkTests.cs` are unchanged.

Full **`mise run ci --ui-jobs 1` passed** at tested head `c49b361`: **461.27s** runner / **464.06s** command, evidence `logs/20261003-155344-5a5bd3c3/ci-summary.json`, console `logs/autobattle-final/ci.log`. All **439 core / 167 tooling tests** passed without failures or skips, including all five unchanged frozen hashes. All six network cases, all five source UI cases, sequential client/server exports, headless package smoke and graphical package smoke passed. Source was restored/built/imported once for CI, with two total expensive slots, graphical cap one and setup batching four; graphical timing barriers still acknowledge speed one. The longer wall time versus earlier cap-two records is not a comparable performance measurement.

| Final phase/scenario | Seconds |
| --- | ---: |
| Locked restore / format / build / import | 1.04 / 21.13 / 1.87 / 3.04 |
| Complete cheap suite | 36.58 |
| Authority/research/recovery / redistribution | 32.87 / 23.32 |
| Defeat / failure cases / solo / playing-host lifecycle | 3.04 / 3.80 / 1.07 / 7.05 |
| Complete network suite | 47.84 |
| Economy / reconnect / settings owned workers | 128.07 / 44.12 / 30.48 |
| Launcher / default combat owned workers | 81.43 / 97.23 |
| Complete source UI including admission | 404.65 |
| Sequential client / server exports | 5.89 / 4.90 |
| Headless / graphical package smoke | 1.30 / 15.69 |

Because upstream changed shared tabletop observation/cleanup code, the affected bounded melee checkpoint was rechecked after synchronization, not assumed equivalent from the old selected pass: `logs/20261003-160610-bc3f0e44/`, case **37.02s**, worker **40.36s**, task **45.22s**, command **46.61s**. Twelve fresh progression observations and four progression PNGs took **4.18s**, proving advancing committed movement and equipped imported attack bones; all four near/far overview/close captures and **28** linked witnesses passed. All eight PNGs were inspected directly. They retain disconnected windup intention, local strike accents, distinct standing anchors and actual rig motion; overview cues remain small and overlapping health bars can still obscure individual miniatures. Capture timing is not a frame-rate benchmark.

Both commands awaited their owned peers/displays and runtime cleanup before success. Scratch diagnostics were preserved under ignored `logs/autobattle-final/scout-scratch/`, not committed. The two synced main requirement/scenario blocks still exactly match their deltas, and strict change/main-spec validation passes with upstream economy requirements retained. Final completion/archive edits change documentation/planning only, not these tested game inputs, so no additional full game/export repeat is needed. Coverage remains owned Linux X11/software OpenGL/Dummy audio: native compositor/GPU pacing, physical input, listening quality, universal crowd readability, native Windows packaging and real paired Steam are not established. No release, upload, publication or merge was performed by these local checks. Only the completed `clarify-combat-presentation` change was archived to `openspec/changes/archive/2026-10-03-clarify-combat-presentation/`, with all 20 tasks complete; unrelated changes were left in place. Strict validation passed before archive and all eighteen main specs/four remaining active changes passed afterward. PR review/publishing and hosted CI belong to the authorized no-mistakes delivery pipeline, not this local acceptance record.

## Windows release publishing restoration — 2026-10-03

Commit `448a973` commented out the native Windows release job and replaced the
shared two-platform asset assembler with a Linux-only shell path. The earlier
hosted failure at
https://github.com/seraph1nia/odot-game/actions/runs/36763314446 occurred **before
compilation**: `Capture(ISCC.exe, "/?")` treated ISCC's nonzero help exit as a
failed version check. Windows export and packaged identity had already passed.
The first restoration probe also established on native Windows that ISCC's
executable version resource reports `0.0.0`:
https://github.com/seraph1nia/odot-game/actions/runs/37142779711.
The fix therefore reads the **loaded compiler engine** banner from a successful
nonquiet compilation (as implemented in upstream `ISCC.dpr`), retaining the
6.7.3 acquisition checksum and rejecting another/missing compiler version
before writing the platform manifest. Neither help failures nor zero-valued
executable metadata are accepted as version evidence. The subsequent native run
https://github.com/seraph1nia/odot-game/actions/runs/37143381683
successfully compiled the intended setup executable in 58.578 seconds, then
exposed Windows file-sharing rules when reopening the still-owned transcript.
The final fix awaits compiler disposal before reading its exact engine banner;
this keeps the ordinary process owner and cleanup rather than weakening the
version check. Hosted release qualification then passed at
https://github.com/seraph1nia/odot-game/actions/runs/37143811632:
both native packages, two-platform transfer/assembly, exact public allowlist and
all four final file checksums. GitHub confirmed the public attachment step was
skipped. Ordinary source/Linux/native-Windows CI passed at
https://github.com/seraph1nia/odot-game/actions/runs/37143811622.
The final checkout-only follow-up disables persisted Git credentials in all
four release/qualification jobs; token permission scopes and the explicit
release-only upload credential remain unchanged.

The release workflow again requires both native packages, transfers their exact
allowlisted artifacts, assembles matching two-target public metadata and checks
all final SHA-256 entries before upload. Release-specific safety, channel and
Steam checks remain unchanged. Relevant PRs exercise those same jobs with local
qualification tags; the public release upload is event-guarded and skipped.
No production release/tag/asset is created or modified by qualification.

Local locked solution restore, `mise run test` (439 core and 172 runner tests),
`dotnet format Odot.slnx --no-restore` and `git diff --check` passed. Test evidence:
`logs/20261003-180047-76d06586/`. Cheap assertions cover the pinned compiler version,
both-platform workflow dependency/transfer/upload guards, exact public file set,
final manifest bytes, both metadata targets, and rejection of corrupt Windows
bytes or mismatched identities. No native Windows execution occurred locally.
Hosted PR checks are the required native package-build evidence; their URLs and
verdicts belong in the PR completion report. Actual installation, upgrades,
uninstallation, Windows graphical interaction and real Steam accounts remain
outside this release-only qualification. See [distribution](distribution.md)
for the later-release route and why the existing Linux-only release is not
silently backfilled.

## Army homes and Town hall (2026-10-03)

The approved roster feature gives two six-size homes initially, four independent gold purchases, persistent first-fit assignments, explicit retirement/storage and two bounded Town hall tracks. It changes allied formation, not enemy/AoE/boss rules, movement topology, unit level upgrades or food rates. Protocol **11** carries unit-targeted commands, expected home-count/track-level quotes, generation-specific assignments, completed-paid eligibility and field/stored funding; there is no release-version change or dependency update.

### Cheap behavior and ordinary paid calibration

Locked restore and whole-solution formatting followed by `mise run test` passed **479 gameplay / 171 runner tests**, **36.87s**, `logs/20261003-215757-18ffcc0c/`, console `logs/army-balance/cheap-final.log`. Coverage includes nonpooled mixed sizes, stable wounds/identity, atomic full rejection, retirement/retry/ownership/phase/ready/pause/generation/quote guards, independent investments/refunds, exact home return after death cleanup including victory, stored elimination, all-owned field-first food, first-cycle/new-ID denial, three real production heals, current-max ceiling/capping, overflow rollback and detached projections. A paid stored guest is serialized/resumed through `AuthoritySession`, retaining wounds and eligibility; Ready/unready, pause/wait and a retried production cannot duplicate recovery.

`ArmyBalanceTests` is checked-in cheap acceptance, not a temporary graphical probe. Nineteen deterministic normally paid cases retain per-wave field/storage/tier/attack usage, home dates, gold/materials, food, casualty/retirement and recovery metrics. Evidence: `logs/army-balance/calibration.log` (19 cases, **10.00s**) and extracted `calibration-samples.json`. Seeds below are **0 / 1 / 123**; healing values use displayed HP, while raw output retains integer hundredths.

| Paid family/control | Outcome | Casualties | Retired | Final home purchase wave | Upkeep | Final gold |
| --- | --- | --- | --- | --- | --- | --- |
| Candidate frontline, 5/8/12/18 | Victory ×3 | 65 / 62 / 68 | 0 / 0 / 0 | 6 / 6 / 6 | 325 / 323 / 323 | 247 / 248 / 244 |
| Steeper frontline, 5/8/16/24 | Victory ×3 | 65 / 62 / 69 | 0 / 0 / 0 | 6 / 6 / 7 | 325 / 323 / 319 | 242 / 242 / 240 |
| Candidate reserve rotation | Victory ×3 | 45 / 51 / 50 | 2 / 2 / 3 | 9 / 9 / 10 | 382 / 379 / 371 | 194 / 188 / 191 |
| Steeper reserve rotation | Victory ×3 | 45 / 51 / 44 | 2 / 2 / 3 | 9 / 10 / 10 | 382 / 362 / 385 | 185 / 179 / 182 |
| Initial homes only, tier progression retained | Defeat W14 / W4 / W7 | 34 / 8 / 14 | 18 / 4 / 10 | No expansion | 84 / 24 / 42 | 162 / 28 / 66 |
| Full homes, no tier progression | Defeat W7 / W13 / W13 | 35 / 97 / 98 | 0 / 0 / 0 | 5 / 5 / 5 | 96 / 202 / 202 | 38 / 128 / 127 |

Candidate reserve pays **43 gold** for construction plus both complete hall tracks, versus **43 gold** for all field homes (steeper homes cost 53). It records **48/49/46 Stores**, **41/41/38 Sends** and **2873.25/2619.70/2547.70 HP** recovered, delaying full field growth while reducing equipment replacements. Its seed-one no-track-upgrade control also wins: 82 recruited, five retired, 57 casualties, sixteen Stores/eight Sends, 708.20 HP recovered, 367 food paid, final gold 228. Upgraded reserve is meaningful but not compulsory. All these cases have zero unfed waves and no stalled outcomes; food has not become a demonstrated universal difficulty constraint. Gross full-campaign food remains 465 produced plus 110 rewarded. These are finite policy comparisons, not isolated proof that healing alone causes each casualty difference or that capacity fixes authored wave difficulty. Final field prices remain **5/8/12/18**: the steeper control mostly postpones later purchases without establishing a better difficulty result.

The ordinary opening proof clears with six paid units and can afford the first 5-gold home at W2. Existing 21 paid solo/cooperative campaigns retain their economic, role, research and all-city-survival assertions. The cleared-frontage witness keeps its occupied-band/admission/inherited-allocation checks using seed **109**, ordinary stone production and paid L3 receiving-city equipment; no defender strength or enemy allocation is injected.

### Same-seed economy fixture diagnosis, not a wave rebalance

The changed formation made an old thin graphical preparation obsolete. Original successful before-CI evidence `logs/20261003-185828-ba485153/` shows more surviving veterans and six metal available before W3 refill. Current failed evidence `logs/20261003-210008-28abaf6f/` retains seed **14056307608042553509**, acknowledgements, ticks and PNG/state observations. Main survivors after W1 are IDs 7/9/11 with 30/10/40 HP; after W2 they are 11/22/24 with 30/10/10 HP. The W3 UI transaction recruits one 56.70-HP L2 unit, leaving only **two metal**, insufficient for another three-metal L2 recruit. Its refill helper therefore does nothing. The observer sold its Barracks and keeps five wounded L1 units totaling 130 HP. Both have two homes and fund every living unit; missing food or a protocol/return/healing defect is not the explanation. W3 has **three L1 Swordsmen and two L1 Crossbowmen per city**, not an AoE enemy. Earlier changed contact/casualties alter the available replacement budget.

`EconomyArmyFixtureTests` cheaply replays the paid input order at recorded cleanup ticks 476/481 and 845/853/869/872, checking exact identities, tiers, wounds, homes, resources, enemy composition and receipts. It reproduces the two actual defeats exactly: **tick 1551** for the original no-Arrow preparation and **1732** with the guessed paid Arrow support. Empty third homes alone still lose at **1551**; earlier material supply without recruiting also loses. Main-only refill clears but loses the observer, rejecting that superficially green correction. Ordinary campaign policy at the same seed supplies a passing W3 comparator. Seven cases pass in **1.95s**, `logs/army-balance/economy-diagnosis/checked-replay-second.log`; scratch diagnostics remain ignored and do not replace these recurring checks.

The minimal correction changes paid preparation only: start the existing four-gold recovery Lumbermill before production one, build the existing two-wood Stonecutter before production three, and pay the Metal Mine's **2 W/2 S** upgrade after W2 production one. Its next two outputs add eight metal. Keep the original research/wounds/sale/rebuild/Market witnesses and structural L2 Catapult; remove the unsuccessful guessed Arrow addition. Main buys its **5-gold third home** and pays nine metal for three more L2 troops, reaching seven funded actors with **276.80 HP**. The observer buys its **5-gold plot**, builds/upgrades a Barracks for **4 wood total**, recruits four L2 units for **12 metal**, and buys its **5-gold third home**, reaching nine actors with **346 HP**. Exact pre-entry stocks are main **G23/W1/F18/S2/M1/C0**, observer **G10/W0/F43/S2/M7/C0**. The cheap green path clears at **tick 1216**, both cities **100/100**, pays **7/9 food** once and awards the original shared reward. No funds/units/healing are injected; no clear/reward, stock, food, tower, cooperative or cleanup assertion is softened.

### Owned controls and admission cost

`mise run test-ui --scenario economy --checkpoint army` is an independently selectable owned two-wave slice, also appended to complete economy coverage. It catches physical control routing, hidden actor/roster behavior, exact return and real labels/layout that cheap arithmetic cannot establish; one solo window, normal resources, fresh observation IDs and bounded PNG captures keep setup/maintenance bounded. Its first successful scenario took **53.01s**, command 63.92s, `logs/20261003-202840-302d3171/`. It caught enabled inspector actions clipped behind the bottom HUD; action controls now stay outside the scrolling profile with explicit bounds checks. A prior hidden construction-choice failure was fixed by opening the actual production group rather than bypassing input.

After the demonstrated preparation correction, complete selected economy **including fresh army controls** passed **162.95s** scenario / **171.49s** task / **173.73s** command, `logs/20261003-214615-63814980/`, console `logs/army-balance/ui-economy-causal.log`. It retains all three paid clears, exact current receipt/shortage, actual reconnect with unchanged stocks, inspector bounds at both 1280×720 and 1100×820, tower/foreign controls and cleanup assertions, then full field/hall rejection, independent upgrades, first-fit Send, no-refund Retire, funded storage and exact production recovery. PNG evidence includes `economy-food-shortage.png`, both inspector sizes and `army-{opening,expansion-retirement,hall-full,paid-recovery}.png`. The two failed owned runs were retained and cleaned up rather than reported as passes.

#### Affected role fixtures and complete gate

The first after-CI attempt (`logs/20261003-220109-cab115d5/`, `after-ci.log`, **88.95s**) is retained as failed/partial: cheap partitions and live authority/recovery plus redistribution passed, but reconnect attempted an affordable seventh Crossbowman with both homes full. The recovered seed **2723540745024433499** cheap paid replay asserts its six surviving wounds (**10/20/20/40/40/20 HP**), full-capacity atomic rejection, then acceptance after the quoted **5-gold** third home, unchanged veterans and exact stocks **G20/W2/F24/M8 → G15/W0/F24/M7**. Mixed/specialist UI preparation now buys physical room when needed, through actual `BuyHome` input for controls and ordinary quoted commands for automated companions. Affected reconnect passed **37.18s**, `logs/20261003-221143-f427122a/`.

The first affected combat run (`logs/20261003-221231-11b531a4/`) reached the fresh casualty checkpoint but paused at tick **769** with no living damaged opponent in the focused city; every such enemy was full-health and only the foreign city had damage. Waiting for new damage while frozen cannot satisfy the next inspector step. A cheap semantic barrier test rejects that captured prerequisite shape and foreign/zero-health/expired/old-casualty controls. The pause barrier now requires **both** the fresh retained corpse and living wounded opponent before freezing. It retains the original fresh pose, live damaged-enemy inspector, damage-or-casualty update, exact death-release/visual cleanup and role/research assertions; there is no timeout inflation, test removal or injected wound. The affected full combat source slice passed **91.95s**, `logs/20261003-222740-3b52c1a4/`.

**Full before/after CI passed 292.32s / 342.11s**, `logs/20261003-185828-ba485153/` and `logs/20261003-223504-02c3e88e/ci-summary.json`; consoles `logs/army-balance/{before-ci,after-ci-second}.log`. After CI restores locked, verifies formatting/build/import, executes **481 gameplay / 172 runner** tests (cheap suite **38.47s**), all six network scenarios (shared admission elapsed **120.79s**) and all five source UI scenarios (shared admission elapsed **287.49s**), then sequential client/server exports **8.92s/3.87s**, headless package smoke **1.30s** and graphical packed smoke **12.05s** (**15.35s** with display ownership). Source case times: economy **165.28s**, reconnect **41.95s**, settings **27.26s**, launcher **80.65s**, combat **91.40s**; individual cases overlap and admitted suite elapsed includes budget waits, not just network execution. Owned cancellation/peer/display cleanup completes before scopes are released. Final source army and packed PNGs were inspected. No publish/upload/release occurs.

Only three ordinarily paid replay/pause-terminal hashes changed, causally from bounded purchased allied setup and explicit roster/protocol state; the two isolated numerical combat hashes are byte-exact. Protocol is **11**, combat rules remain **5**, and tool/NuGet locks plus asset provenance are unchanged. All rendering uses owned Linux X11/software OpenGL/Dummy audio; native compositor/GPU performance, physical input, listening quality, native Windows packaging and paired Steam are outside this record. Price calibration establishes paid options and challenging controls, not a universal balance guarantee. Final documentation/spec sync needs consistency checks, not another unchanged full export run.

### 2026-10-05 CI upkeep receipt and native counterfactual correction

Hosted source run `37339665585`, job `111863206002`, failed the economy
current-wave compact upkeep wait after 415.53s. The retained console establishes
the failed wait, not the exact final graphical/authority state; the run exposes
no downloadable artifacts. The driver nevertheless had a reproducible receipt
window defect: it synchronized the graphical client after final Ready while the
battle kept advancing. Once that wave ends, waiting longer cannot restore its
current-battle label. Cheap executable regression coverage advances an ordinary
paid match past battle completion, then contrasts ordinary pause retaining the
actual receipt through 10,000 attempted ticks and ordinary resume progressing
without a second payment. Negative controls reject wrong session, revision,
phase, pause state, wave, turn, city, connection and any of the four HUD values.

The existing headless observer now pauses immediately after its accepted final
Ready, before graphical synchronization. Both command receipts must remain in
Combat/W3 in the same session/turn; the fresh rendered HUD must match the frozen
revision, city and complete actual upkeep. Then the observer resumes and the
existing clear, shortage and reconnect assertions continue. No gameplay rules,
assets, simulation speeds, production transport or timeout constants changed.
The selected economy command used `--timeout-ms 900000` (the existing CI budget)
instead of the standalone 300000ms invocation default: retained constrained
runs exhausted 300s, and this complete CI economy worker took 300.60s. The finite
Start/probe allowance remains 60000ms in these invocations; native modes remain
20s each and the native diagnostic remains 60s overall. Ordinary public/dev,
Steam and explicit-short launch policies are unchanged.

The previously malformed native liveness control now flushes only the guest's
pending retry before withholding; authority admission and guest delivery cannot
occur before the schedule. A fixed diagnostic seed gives every arm identical
baseline snapshot bytes. The serviced authority proves disconnect; only after
guest service resumes are retained deliveries checked, without assuming queued
UDP records must disappear or requiring an unserviced owner's timeout event.
`mise run test-native-pump --scenario peer-liveness --startup-timeout-ms 60000`
passed all five arms, `logs/20261005-170818-1ffdffc8/`: old
`SetTimeout(32,5000,10000)` disconnected at 5645.44ms during 15s withholding;
fixture `SetTimeout(32,30000,60000)` survived the same schedule, conserved the
pending retry and accepted the next pause. Both no-stall arms conserved delivery;
explicit `SetTimeout(32,500,1000)` disconnected at 1504.14ms during 2.5s withholding.
Actual admission/receipt session, sequence, revision, event cursor, service counts
and teardown are retained. This proves whole-session owned-fixture native
liveness alignment, **not** the hosted stall duration or receive-service root
cause. The original native conservation/reconnect/pause/cancellation-drain
control also passed, `logs/20261005-172418-06430830/`, without another poll owner.
The earlier failed control and all unreached-arm records remain historical
failures, not retroactive passes.

All 795 cheap tests passed (500 gameplay/295 runner). Selected economy passed
281.56s, `logs/20261005-170859-693ab9c6/`. Complete local
`mise run ci --startup-timeout-ms 60000` then passed **546.39s**,
`logs/20261005-171424-d97ea622/ci-summary.json`, console
`logs/ci-receipt-full.log`: locked restore/format/build/import/static fidelity,
all cheap partitions, all six network cases, all five source UI cases, sequential
client/server exports, headless package smoke and graphical package smoke.
Its `economy-worker/economy/upkeep-command-receipts.json` records session
`75ea7d5fa1c148c9bbbd6a8402dcef52`, Ready sequence35/revision968/tick872 and
pause sequence36/revision973/tick876; `upkeep-frame.json` is the same frozen
revision/W3/turn15/city1 and displays `7 food` with zero sit-outs. Awaited owned
cleanup passed. No other implementation inputs changed after this full pass.
This is locally verified repair evidence, not exact-final-head hosted readiness:
the outer pipeline still owns publication and all hosted source/Linux/Windows
package/required checks for PR #8. Software-rendered silent Linux checks do not
establish native GPU/compositor performance, listening quality or paired Steam.

## Deterministic network recovery opening, 2026-10-05

Hosted head `5dcc148c5f8c4f43e18c5ad21c34f308b7d4dd82`, run
`37348523905` / source job `111893175861`, failed on B's `buy-plot 7`:
`Only living cities can act during building.` This is not timeout exhaustion.
The retained hosted console lacks session/revision/city-liveness payloads;
GitHub reported zero run artifacts, and the named operator metadata file was
not present in this worktree. The exact hosted combat seed therefore remains
unknown. The unchanged local selected case passed at a different random seed,
`logs/20261005-173614-297a4b67/`; it did not disprove the hosted failure.

`NetworkOpeningTests` executes the same ordinary paid opening. Seed `96`
reproduces a team clear to Building/W2 at tick853 with A living and B eliminated;
B's plot purchase receives the exact hosted refusal without any state mutation.
The fixture previously selected an OS-random combat seed but required B's
subsequent paid construction, typed recruitment and recovery. Its dedicated
server now uses seed `16366921918512773030`, recovered from the prior complete
`logs/20261005-171424-d97ea622/` pass, including owned bind retries. First and
second clears explicitly require both cities living. Cheap coverage retains the
negative seed, checks both fixed-seed cities live and verifies actual plot and
building payment. Production matches still use their ordinary seed policy.

The first fixed-seed slice exposed a separate exact-retry measurement race:
`logs/20261005-174111-d9e4f665/`. B's home purchase at revision440/tick388 had one
remaining body; pause at revision444/tick391 followed ordinary final-body cleanup
and veteran home restoration. Placement/action state changed, not stocks or
health. `RecruitAll` now awaits same-match, non-older, body-free state before
investment, at the shared boundary for both `Advance` passes and the Steam
opening caller. It does not mask placement, weaken exact city comparisons or
change cleanup timing. The executable regression checks ordinary restoration
preserves stocks, identities, wounds and assignments, then verifies a settled
purchase remains exact through delayed pause. Steam paired execution was not
repeated; its opening has no post-clear bodies and passes this barrier immediately.

Final selected `authority-resume-victory` passed **32.09s**,
`logs/20261005-174345-40c16bf6/`; its whole-scenario recovery, retry, research and
refusal assertions remain. All three new cheap cases passed. Complete local
`mise run ci --startup-timeout-ms 60000` passed **539.80s**, evidence
`logs/20261005-174440-1deb528f/ci-summary.json`, console
`logs/ci-network-full.log`: all **798** cheap tests (500 gameplay/298 runner),
six network scenarios, five source UI cases, formatting/build/import/static
fidelity, sequential Linux client/server exports and both package smokes.
The complete run's network recovery case passed **29.58s**. Its economy proof
retains session `d0cd737691dd48fb936f5e5a2fddbee2`, Ready sequence35/revision968/
tick872, Pause sequence36/revision973/tick876, and the fresh same-session frozen
revision973/W3/turn15/city1 HUD displaying `7 food` paid and zero sit-outs.
Current repair hashes are recorded in `logs/ci-network-verified-inputs.json`.
All historical failures remain retained, including the initial bounded seed diagnostic that
found no counterexample in seeds0..63; the expanded diagnostic found seed96.

No timeout values changed in this repair, and no numerical gameplay, speed,
assets, wire protocol, production transport or poll ownership changed. Prior
native five-arm and conservation proofs are reused: all their recorded
implementation hashes still match `logs/ci-receipt-verified-inputs.json`.
The prior 546.39s complete pass is the before baseline, not new hosted evidence.
This final local pass does not establish exact-final-head hosted readiness;
publication and all hosted source/Linux/Windows package/required checks remain
owned by the outer executor. PR #8 is not claimed ready to merge.

## Research fixture and hosted graphical admission finalization, 2026-10-06

PR #9's published head `1e1b79a80d2ce76b8725e334adf449f09785281d`
failed hosted source run `37389015488`, job `112029377239`, when an ordinary
research pause was refused with `Stale match, phase or turn.` The retained
current-observation and frozen-receipt boundaries correctly refuse capture when
the witness no longer belongs to the current combat/session. They are not
relaxed to make this check pass.

The narrow setup correction recruits affordable Mages before optional Swordsman
refills in `CampaignStrategy.ResearchWitness`. Its only process callers are
`TransportedResearch` and `ResearchCheckpoint`; public gameplay and the separate
`CampaignStrategy.Next` frontline profiling policy are unchanged. Desired count
limits, prices, normal commands, configured seeds, natural waves, assets and game
rules remain unchanged. **Paid command order, first-fit home placement, combat
dynamics and witness timing/wave change**: the constrained candidate captured a
burn at wave six rather than the previous wave-eight six-tick window. Whole
fixture input/outcome digests and realized counts are not claimed identical.
This is verification setup, not an additional game/FPS optimization.

The preserved two-file patch hashes to
`bee201ca35e6b5547d0abf822084ccdcb3986927a837b12bd3851b46c8da8602`.
`logs/ci-stale-pause/red-tests.log` records two executable recruitment-order
failures against the old policy (expected Mage, actual Swordsman), for seeds one
and `16366921918512773030`; `green-tests.log` records all seven ResearchWitness
controls passing. The completed candidate full local CI is reused, not rerun:
`logs/ci-stale-pause/after-ci.log` and
`logs/20261006-000924-9a5a958d/ci-summary.json` finalize at 00:18:16Z, after the
previous agent cut, with **532.22s, 504 gameplay and 348 runner tests**, all six
network and five source UI scenarios, authored fidelity, sequential Linux
client/server exports and headless/graphical package smokes passing. The exact
fixture patch and other production inputs remain unchanged during finalization;
its tool versions are .NET 10.0.401 and Godot 4.7.2 .NET. This evidence is local
candidate coverage, not hosted final-head readiness or inherited new-head package
success.

`BurnAdmissionTests` now adds a cheap executable pair using the actual paid
research opening and natural combat. The timely ordinary pause returns the exact
accepted receipt/sequence and observed target with the same complete burn record;
ordinary steps cannot advance its frozen tick/revision. Delaying that quoted
request until a natural clear advances phase and turn produces the actual stale
refusal, without a pause or state mutation, and admission throws without capture,
resume or rearm. Existing target/effect substitution, current ownership,
three-attempt rearm and cancellation controls remain in force. Production's
ordinary action wrapper rejects a refused command even before receipt admission.
No predictive model, production capture API or timeout increase is introduced.

The hosted source invocation alone now adds supported `--ui-jobs 1`, retaining
default `--jobs 2`, all five UI and six network cases and existing 120s research,
150s army and 900s CI bounds. Scheduler/default semantics and Linux/Windows
package commands are unchanged. `AdmissionTests` executes the command's option
contract; the existing scheduler control verifies the shared total budget and
single graphical slot with awaited cleanup. Normalized workflow meaning was
checked against the prior configuration: only the source command's graphical cap
changes; all other jobs, steps, triggers, permissions and deadlines are identical.
Finalization evidence under `logs/ci-fixture-finalization/` records locked restore,
zero-warning solution build, formatting and **80 affected cheap controls passing**,
plus `workflow-semantics.json`. No new full game/export run was launched.

The earlier constrained two-CPU/UI-two source attempt remains **failed** at army
cancellation (`candidate-source-two-cpus.log`), not green and not an established
assertion failure, OOM or performance regression. The earlier replacement-server
native crash remains unresolved historical evidence. The prior local UI-one pass
and this candidate UI-two pass have backend/load confounds; serial hosted UI is
not a proven cure. All required source/Linux/Windows and other required hosted
checks must complete on the exact corrected published head. The outer executor
owns that publication/validation; PR #9 is not yet claimed ready to merge.

## Hosted army wall-clock allowance correction, 2026-10-06

Hosted source run `37394971214`, job `112048648887`, at head
`04d440a00cf25beeec293187fcbca89db5c63c0c` failed despite serial UI admission.
The retained failure report records army entry at `00:45:37.996267Z` and the last
of three exact production-recovery assertions at **149.302s**, only **698ms**
before its 150s boundary (`00:48:07.996267Z`). Cancellation then cleaned up the
owned worker with exit **130**. The final Pick/OpenTownHall/UpgradeHallCapacity
input, independent-track assertion, fourth `army-paid-recovery` PNG, Close and
clean exit were not completed; three earlier frames are not passing coverage.
This is progressing ordinary input interrupted by a nested runner timer, not an
established gameplay/CPU regression or an assertion failure. Serial scheduling
alone was therefore insufficient to establish hosted completion.

The authorized correction changes only `ArmyUiScenario`'s existing linked
wall-clock allowance from **150s to 300s**, shared by standalone
`economy --checkpoint army` and the complete economy route after cooperative
peer retirement. The parent token can still cancel it earlier. Research stays
**120s**, outer worker/source CI **900s**, and provider job limits are unchanged.
All three recovery iterations, exact tick/effect/death/retry/receipt assertions,
ordinary game clock, assets, cleanup and all five UI/six network cases remain
required. The allowance is not a game speed, FPS, receipt-freshness or measured
CPU-performance guarantee and does not revise historical performance/fidelity
claims. Original failed logs remain failure evidence, not superseded passes.

Unchanged numerical/fidelity/receipt/phase-setup baseline evidence above is
reused; no redundant complete local source/export run is required for this
wall-time-only edit. New-head hosted source, Linux/Windows packages and every
required check remain outstanding until the outer executor obtains them. The
fourth fresh recovery capture and all remaining cases must actually finish; this
change does not waive the failed source check. Any 300s cancellation, assertion
failure or outer-limit exhaustion must be reported with the earliest failure,
remaining work and exact timer, not followed by another allowance increase.
