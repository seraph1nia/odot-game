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

## CI coverage

`Verify and build` retains the existing `source`, `linux-package`,
`windows-package` job identifiers. PRs and main pushes use the same bounded
headless coverage: formatting/compilation, fast C# (excluding `Tier=Deep`), three
production Godot component cases, three short transport/lifecycle cases, offline
native extension/build identity, and native Linux/Windows export/runtime probes.
Linux exports mutate sequentially; OS jobs have independent workspaces. Source
prepares once before overlapping already-built core/runner binaries with bounded
network workers. There is no Xvfb install or graphical source/package launch.

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

The [woodland UI migration](ui-migration.md) replaces the live Trio pack with
native resources and production service bindings. The former pack's notices and
derivation records are preserved in [the Trio archive](asset-history/trio-ui/README.md)
and its `manifest.json`; older Trio receipts are not acceptance for this replacement.

`test-engine --scenario settings` now includes the woodland resource, quoted
intent, modal/input and Settings-service checks in the existing minimal
`EngineTests` scene. Its existing `Child`/`ScenarioScope` supervisor owns isolated
preferences, marker validation, engine logs and a bounded child, not the main
scene, a match, a display or screenshots. The former `test-ui-resources` command,
probe and entry point were removed after folding their meaningful checks into
this routine engine selection (also selected by source CI). The recorded
**38-check** pass predates this rebase and remains historical component evidence,
not current-head or rendered acceptance. See the migration document for receipts
and remaining coverage; current-head gates belong to the outer validation phases.

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
construction. They do not prove native popup/focus behavior, compositor/GPU
performance, final pixels or audible playback. Transport peers are real headless
application processes, not component tests. A minimal explicit graphical smoke
retains menu/solo/build/recruit/return/Exit and one capture; it does not replace
all old graphical acceptance assertions. Those journeys remain explicit E.

Cross-process cooperating runners lease port candidates through kernel-backed
exclusive file handles; the OS still owns actual UDP binding. Unrelated programs
can take a candidate between selection and bind: bounded server readiness retries
preserve their owner. Owned data, cancellation and shutdown protections remain.
Shared preparation/export commands are never safe to parallelize in one worktree;
use one prepared runner invocation, not concurrent `mise` invocations.

See the audit for implementation-time selected receipts and remaining risks;
final-head verification belongs to the outer validation phases.
