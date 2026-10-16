# Verification

The current strategy is **headless by default, targeted by changed behavior**.
There is no full-CI-before/after requirement for development tasks.

## Entry points

One owner per topic, rather than competing copied policies:
- [Testing strategy](../.agents/skills/testing-strategy/SKILL.md): tiers and verification selection.
- [Testing commands](../.agents/skills/testing-commands/SKILL.md): runnable filters, component/transport/package selections and concurrency.
- [Testing hygiene](../.agents/skills/testing-hygiene/SKILL.md): admission/removal criteria.
- [Flaky diagnosis](../.agents/skills/diagnose-flaky-tests/SKILL.md): evidence and controlled discriminants.
- [Audit and coverage ledger](testing-audit.md): test groups, significant removals/replacements and failure history.
- [Historical receipts](verification-history.md): old measured runs, not current policy.
- [Historical battle-flow reconstruction coverage](battle-flow-reconstruction-coverage.md): pre-squad paid/occupied witness mapping and local receipts; current replacements are in the audit.

## Roguelite milestone 1 evidence (2026-10-15)

Implementation scope and exact rules are in [roguelite](roguelite.md); the
[ownership map](roguelite-plan.md) identifies the canonical seams; [session
ownership](sessions.md) owns compatibility versions. The native dialog is a production
woodland `UiDialog`/Button consumer, not a verification-only screen. The initial
2026-10-15 receipt below predates the review and verification-adapter repairs; its
counts are historical, not final-head coverage. It qualifies numerical, component
and selected transport boundaries, **not rendered appearance, physical controller
usability, GPU, real Steam or Windows acceptance**. The later local hardware receipt
is recorded separately below.

| Check | Executed coverage | Evidence / result |
| --- | --- | --- |
| `dotnet format Odot.slnx --no-restore`; `dotnet build Odot.slnx --no-restore` | Changed C# formatted; entire solution, locked prepared dependencies | Zero warnings/errors |
| prepared `DevRunner.dll test-fast` | All fast core + runner; excludes `Tier=Deep` | `logs/20261015-222823-2b653edd`: **795 core + 663 runner passed**; 2 existing Windows-native argument tests skipped on Linux; 66.54s |
| `mise run test-engine --scenario receiving` | Real six-card doctrine and three-card reward Controls, normal intent/ack bridge, close neutrality, locked state; existing ledger/Worldtree/army/forecast consumers | `logs/20261015-222931-f7d5b759`: passed, 23.69s component / 28.23s total |
| `mise run test-engine --scenario arena` | Existing shared arena rigs, buffered member/objective bars, Rally, physical bought towers and cleanup with Warden-adjusted Watchpost maximum | `logs/20261015-220200-08b29ae6`: passed, 6.78s component / 13.86s total |
| `mise run test-network` | Default bounded real ENet `failure-cases`, `solo-session`, `playing-host-lifecycle`; extended existing lifecycle earns a clear, drafts, retries, resumes and disconnect-only fallback | `logs/20261015-215844-7787b258`: all three passed, 17.73s total |
| `mise run test-network --scenario authority-resume-victory` | Selected deeper ENet paid typed recruitment/retry, combat pause/restart, reward-aware ordinary progression, point-funded Fire and owned Mage potency, exact active status resume | `logs/20261015-221945-b9276eb7`: passed, 38.99s scenario / 44.66s total; **not** a new full twenty-wave process qualification despite the historical selector name |
| prepared `test-campaign --filter 'FullyQualifiedName~FrontlineCampaignTests\|FullyQualifiedName~SessionCampaignTests'` | Three ordinary twenty-wave solo seeds 0/1/123 plus serialized two-city authority including both bosses, nineteen drafts and terminal reward | `logs/20261015-220221-1565c68b`: 4 passed, 6.73s |
| prepared `test-campaign --filter 'FullyQualifiedName~MixedCampaignTests\|FullyQualifiedName~TowersCampaignTests\|FullyQualifiedName~ResearchCampaignTests\|FullyQualifiedName~CooperativeCampaignTests'` | Mixed/tower/research solo and 2/3/4-player cooperative complete ordinary campaigns at seeds 0/1/123 | `logs/20261015-220245-6b33d0e6`: 18 passed, 80.22s; selected deep coverage, not the entire deep suite |

The 57 focused Core cases cover catalog/caps and all six starting allocations,
additive per-building production with actual Worldtree offshoots, profile/status
stacking, quotes/upkeep, all multiplier/rarity boundaries, eight bounded 19-draft
eligibility walks, all nineteen real Match draft transitions/sixty settlements,
negative ownership/stale/paused/duplicate/null-action guards, bounded doctrine
preview and atomic rejected-start allocation, zero-connected
fallback, detached/malformed codec states, independent/reproducible seed streams,
exact authored encounter budget and the then-present ordering-disable option.
That option was subsequently removed; the current authored-budget comparison
uses the campaign definition directly, as specified in [roguelite rules](roguelite.md#seeded-encounter-boundary-and-compatibility). Controlled combat
poses test one-action Warhorn, pre-impact Conduit/within-squad splash, actual-hit
Ashen/Poison coexistence, lethal simultaneous and refreshed periodic potency
credit after source death, Keep Roots at actual same-tick Watchpost destruction,
noncompounding HP, living stored/reserve Surgeons, and all four tower variants'
full cadence/range/damage. Immediate reward and Codex/Harvest clear effects include
bounded preflight and actual receipt assertions. Controlled-result cases are
explicitly **not earned balance evidence**; the 22 complete paid campaigns and
real transport journeys supply the narrower earned evidence above.

Preserved diagnostic failures under ignored `logs/roguelite-*.txt` distinguish
stale pre-doctrine stocks/phase assertions from product boundaries. The receiving
first-open empty-card binding was fixed. Inherited generated Godot import data
was archived under `logs/roguelite-inherited-import-cache/` and rebuilt, without
changing tracked assets. The selected research process driver originally used a
three-building shortcut inconsistent with its cheap paid-opening fixture; it now
shares the ordinary SupportOpening and reserves one normal settlement's stocks
for its typed purchase, without extra income/windows. New passive Burn initially
satisfied the old research witness too early; admission now requires purchased
Fire plus owned Mage potency, with a focused driver regression. No deadlines,
retry counts, production count, enemy tuning, locks or graphical CI gates changed.

At that initial checkpoint, graphical and native-GPU journeys were unexecuted.
Updated specialist progression drivers are not passes merely because they compile.
Existing historical evidence below remains scoped to its original source, rules
and inputs.

### Review and local hardware follow-up (2026-10-16)

Retained evidence root for this gate is
`/home/bart/.no-mistakes/evidence/01M51SPXW3F3MKCATXEB8M6QDT/`.
`round6-receipt.json` binds selected checks to source `8692e71`: 96 Core and 110
runner cases; settings/camera, receiving and arena components; ordinary
playing-host lifecycle and paid Fire/Mage status resume; and paid final-draft
ENet campaigns for all four roster sizes. The final-draft journeys check A04/R04
absence and explicit refusal with unused stack/relic capacity, the connected
barrier and absent-player fallback, without a fourth settlement. These are
selected regressions, not an unfiltered suite or every card acquired live.

At exact clean source **`76e6a3bb441ec1df61e6391731ca34ee80fe0a96`**,
`round10-committed-gpu/source-receipt.json` and `receipt.json` bind source, binary
hashes, actual renderer and owned-window identity to three passed source journeys:

| Journey | Coverage | Owned journey time |
| --- | --- | ---: |
| Solo smoke | Real menu → Lobby → Start, paid Farm/Barracks/Metal Mine, production/recruitment, return and Exit | 27.65s |
| Six doctrines | All six native selections and exact starting allocations, fresh sessions, locked inspection resized to 1280×720 | 44.80s |
| Earned campaign | Paid seed-1 twenty-wave victory, nineteen native drafts, sixty settlements, all twenty forecast/entry comparisons, locked doctrine, Escape/keyboard reopening and invalid/duplicate refusals | 50.93s |

Every retained GPU journey authenticates **Forward+ / Vulkan RenderingDevice,
AMD Radeon RX 6800 (RADV NAVI21), DiscreteGpu, X11, Windowed**, and the owned
child PID/native window. The receipt includes game-only PNG/JSON pairs and records
visual review of the solo frame, resized doctrines and final draft. Owned processes
exited and runtime data was removed. Eleven adapter-boundary regressions passed
with no skips at that head; earlier numerical/headless/ENet evidence was preserved,
not rerun or counted as fresh. [Renderer verification](atmosphere.md#verification-and-visual-evidence)
owns the explicit hardware command selection; hosted CI remains headless and
GPU-independent. Dummy audio and synthetic input do not qualify listening,
physical controllers, general GPU performance or other hardware/platforms.

**Software-renderer reliability remains unresolved.** Retained llvmpipe SIGSEGV
and malformed command-bin traversal occurred even with `LP_NUM_THREADS=0`.
`round5-renderer/retained-bins.txt`, `revalidation-native-crash-backtrace.txt` and
`round6-receipt.json` retain the failure and diagnosis limits. Later software
passes and this separately authorized hardware acceptance do not establish a
Mesa fix. Product rendering defaults, software-lane assertions and CI protections
were not relaxed to obtain the hardware result.

Still unexecuted for this milestone: unfiltered deep qualification, specialist
network scenarios outside the recorded selections, package/export/installed
acceptance, Windows/macOS, physical controller/audio and multi-account Steam
invitations. The hardware receipt qualifies only the tested local environment,
not a release, hosted graphical CI or other renderers.

## CI coverage

`Verify and build` retains the existing `source`, `linux-package`,
`windows-package` job identifiers. PRs and main pushes use the same bounded
headless coverage: formatting/compilation, fast C# (excluding `Tier=Deep`), six
production Godot component cases (settings, animation, assets, Worldtree,
Tabletop receiving bindings and the shared arena), three short transport/lifecycle cases, offline
native extension/build identity, and native Linux/Windows export/runtime probes.
Linux exports mutate sequentially; OS jobs have independent workspaces. Source
prepares once before overlapping already-built core/runner binaries with bounded
network workers. There is no Xvfb install or graphical source/package launch.
The [Worldtree integration](worldtree.md#verification-and-limits) also loads its
actual production component/game adapter from the exported client in the existing
Linux/Windows package jobs; [exact implementation receipts](worldtree-validation.json)
and the [delivery plan](worldtree-verification-plan.md) distinguish measured
headless resource/input/state coverage from unexecuted graphical acceptance.

`Deep headless qualification` runs weekly or manually, and supports a single
network scenario. It runs only C# campaigns and headless network journeys. It
never selects graphical journeys. Ordinary CI green does not claim full campaign,
rendered input, native GPU, Steam-pair, installer or release qualification.

`Build published release` still gates exact clean tags, own-AppID stable builds,
native platform packaging, checksum/identity validation, allowlisted asset
assembly and release-only upload without overwrite. No branch-protection or
publishing settings were changed. At audit time the forge's main protection
response contained no required status-context list; existing CI job identities
are nevertheless preserved. Current forge permissions/checks must be read again
at delivery. No release was published or triggered in this overhaul.

Artifacts are optional allowlisted non-secret summaries, TRX and bounded redacted
engine diagnostics; absent files warn rather than causing a second CI error.
Other child logs/evidence remain under ignored `logs/<run-id>/`; credentials and
temporary owned runtime state must not be uploaded.

## Woodland UI migration evidence

The [woodland UI migration](ui-migration.md#verification-and-limitations) owns
receipts and remaining coverage for the replacement of the live Trio pack;
older Trio receipts are not acceptance for it. The
[coverage ledger](testing-audit.md#woodland-component-consolidation) owns the
consolidation into the existing minimal settings engine case, and
[testing commands](../.agents/skills/testing-commands/SKILL.md#focused-headless-engine-and-bounded-transport)
owns selection and supervision. Current-head gates belong to the outer validation
phases, not historical component receipts.

## Vivid assets and ambient playback evidence

The [import/runtime report](vivid-ambient-import.md#verification-boundary) owns
executed building/water playback coverage and its limitations. It separates the
initial vivid-pin receipts from optimized-successor verification; neither is a
substitute for final-head pipeline/CI qualification.

## Framing, renderer and Worldtree evidence

[Batch findings and local evidence](view-renderer-worldtree.md) distinguish native
headless camera/control/resource checks from unexecuted rendered/GPU/live-session
acceptance, and retain the new failed runtime-invocation receipt and its correction.
These local implementation receipts are not final PR-head or CI qualification.

## Army plot and tower batch (2026-10-12)

[Authority/interaction receipt and causal diagnosis](army-plots-towers.md) records
stable numbered plot acquisition, whole-plot tower exclusion, the two explicit
upgrade profiles, preserved legacy decision/economy behavior and honest limits.
Local fast coverage passed 484 core + 649 runner cases with two Windows-only
skips; the selected native `receiving` component also passed. These receipts do
not claim full-world input, rendered presentation, network delivery or export
qualification, and do not substitute for subsequent current-head PR/CI gates.

## Overhaul measurements (2026-10-11)

Baseline source: `d7f1728` (the actual `origin/main` head when audited).
After receipts below describe implementation `70ba7e3`, before the later
component-deadline and deep-endpoint review fixes; they are not final-head test
counts or qualification. The audit links targeted selection receipts separately.
Locked .NET SDK 10.0.401, Godot .NET 4.7.2, Linux x86_64. Local timings include
warm dependency/build state; hosted historical timings have different machines
and cache state. They are **not** a matched end-to-end speedup measurement.

| Receipt | Actual coverage | Wall clock |
| --- | --- | --- |
| Local original `mise run test` | 504 core passed; 636 runner passed + 2 Windows skips; 4 partitions, max 2 processes | 44.87s including preparation; runner suite 39.24s |
| New default `mise run test` | 457 core + 633 runner passed, 2 Windows skips; max 2 completed binaries | 20.42s suite; 22.31s task |
| New `test-fast --jobs 1` | same counts, serial completed binaries | 29.87s runner; 31.83s task |
| Implementation headless source gate | same fast C#; settings/animation/assets; failure/solo/playing-host transport | 56.10s runner; 58.43s task including runner build |
| Explicit headless `test-deep` | all 43 deep C# rows + all 6 retained headless network cases | 81.20s runner; 83.57s task; network suite 42.55s |
| New headless Linux package gate | ordered client/server exports, normal connected build/production/recruit smoke, offline native extension | 20.32s runner; 21.65s task |
| Current-main historical hosted source run `38120639473` | old C# + all 6 network + all 5 graphical cases | 1132.17s source; C# 53.03s; network 428.00s; UI 1066.59s |

The first coherent source run (57.35s, 461 core) preceded removal of four
fixture-self-check rows and shared immutable GLB parse. Implementation receipts
above include those changes. Default local C# task changed from 44.87s to 22.31s with
**deliberately narrower routine coverage**, not faster production simulation.
Initial stale timeout-policy assertions failed during refactor and were updated
to the new bounded/explicit behavior, not suppressed. No graphical/local desktop/Vulkan
baseline or after run was executed. Graphics smoke/deep code is selectable but
unqualified under this task's headless-only boundary. Native Windows/release/
installer/real Steam verification was not locally executed.

Implementation source evidence: `logs/20261011-083602-cc8c8d0b/ci-source-summary.json`.
Default C# evidence: `logs/20261011-084125-249c70c1/test-fast-summary.json`.
Serial C# evidence: `logs/20261011-083745-8a8614b7/test-fast-summary.json`.
Deep evidence: `logs/20261011-082802-40cee91b/test-deep-summary.json`.
The deep run used one C# binary followed by network budget 2 (phase starts show
concurrent admissions); its old `Jobs=1` summary metadata was corrected afterward.
Local package evidence: `logs/20261011-082044-21fa1637/ci-linux-package-summary.json`.
Original C# evidence: `logs/20261011-080905-42bb1fa4/test-rules-summary.json`.

## Limitations

Headless Godot tests exercise real ConfigFile persistence/audio-bus signals,
imported scenes/equipment/AnimationPlayer skeleton changes and rendering buffer
construction. The settings case also routes synthetic pointer/keyboard input
through Control modals and embedded popups. These do not prove native-window
popup/desktop focus behavior, compositor/GPU performance, final pixels or audible
playback. Transport peers are real headless application processes, not component
tests. A minimal explicit graphical smoke retains menu/solo/build/recruit/return/Exit
and one capture; it does not replace
all old graphical acceptance assertions. Those journeys remain explicit E.

Cross-process cooperating runners lease port candidates through kernel-backed
exclusive file handles; the OS still owns actual UDP binding. Unrelated programs
can take a candidate between selection and bind: bounded server readiness retries
preserve their owner. Owned data, cancellation and shutdown protections remain.
Shared preparation/export commands are never safe to parallelize in one worktree;
use one prepared runner invocation, not concurrent `mise` invocations.

See the audit for implementation-time selected receipts and remaining risks;
final-head verification belongs to the outer validation phases.


## Shared arena recovery

[Shared arena](shared-arena.md) owns current actors, objectives, native controls,
woodland styling; [session ownership](sessions.md) owns current compatibility. [Paid baselines](paid-squad-baselines.md)
retain pre-rebalance full-six Crossbow arithmetic survival, the owner-deferred
paid ranged openings, ten genuine solo wins and the finite1..4owner cooperative
sample. [Wave power](wave-power.md#headless-encounter-evidence) owns the current
paired paid measurements. The [audit](testing-audit.md#wave-power-rebalance-2026-10-14)
records rebalance receipts and unexecuted coverage; earlier recovery, four-edit
and pre-trace receipts remain historical. The
[health-overlay contract](../openspec/specs/game-feedback/spec.md#requirement-authoritative-overhead-unit-health-bars)
owns current presentation requirements; the
[health-only overlay receipt](testing-audit.md#health-only-actor-overlays-2026-10-14)
records focused headless regressions and unexecuted rendered acceptance. Cosmetic
death overlap grants no logical claim. Component/node correctness remains
separate from explicitly authorized rendered combat and platform packages.
