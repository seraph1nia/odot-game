---
name: testing-commands
description: Run odot-game tests, targeted .NET filters, headless component/transport checks, or explicit deep/package qualification; load when choosing an exact verification command.
---

# Commands and coverage

Run from repository root using the locked SDK/Godot .NET and locked NuGet restore.
Do not install prerequisites yourself. Task descriptions and runner summaries
name the selected coverage; evidence is ignored `logs/<run-id>/`.

## Fast C# (no Godot process)

```sh
mise run test-fast                         # same default coverage as mise run test
mise run test-core
mise run test-runner
mise run test-core --filter 'FullyQualifiedName~HexRoutingTests'
mise run test-core --filter 'FullyQualifiedName~HexRoutingTests.BlockedRouteCannotEnterOccupiedGoalAndBecomesAvailableWhenBlockersLeave'
mise run test-core --filter 'FullyQualifiedName~Research'
mise run test-runner --filter 'FullyQualifiedName~PortLeaseTests'
mise run test-fast --jobs 1                # serial binaries; same coverage
```

Use standard VSTest expressions (`FullyQualifiedName`, `&`, `|`, parentheses).
Fast commands exclude `Tier=Deep`; choose `test-campaign` to select those tests.
A filter executing zero tests across selected projects fails, even though VSTest
normally exits zero. TRX reports retain actual executed counts. Skipped native
platform tests are reported, not silently counted as exercised.

Tasks restore/build `Odot.slnx` once before parallel completed test binaries.
Core remains serialized within its own process because Arch ECS has static
registry state; runner xUnit classes use their existing independent fixtures.
Core/runner binaries execute concurrently (default `--jobs 2`). For already
prepared inputs, avoid build/restore mutation with:

```sh
dotnet tools/DevRunner/bin/Debug/net10.0/DevRunner.dll test-core --filter 'FullyQualifiedName~HexRoutingTests'
```

`test-in-process` is an explicit socketless fallback executing ALL discovered C#
tests, including deep campaigns, with no targeted filtering. It is not the default
fast command. Profiling commands in `mise.toml` remain separate diagnostics.

## Focused headless engine and bounded transport

```sh
mise run test-engine
mise run test-engine --scenario settings
mise run test-engine --scenario animation
mise run test-engine --scenario assets
mise run test-engine --scenario worldtree
mise run test-engine --scenario receiving # actual HUD/session actions + Worldtree sender bridge
mise run test-engine --scenario arena     # native shared arena, deployment/squad controls and receiving adapter
mise run test-engine --scenario worldtree --exported # existing native client; no source preparation/export
mise run test-network                      # failure-cases, solo-session, playing-host-lifecycle
mise run test-network --scenario failure-cases
mise run test-network --scenario playing-host-lifecycle
mise run test-network --scenario redistribution  # explicit deeper paid transfer journey
mise run test-network --scenario eliminated-resume # earned two-wave veteran/storage prefix; controlled W3 elimination; real ENet resume/denial, next allocation W4 (NOT earned eliminated-stored balance)
mise run test-network --scenario active-fallen # fixed numerical active fighters, exact ENet resume/impact + Keep-health control; NOT earned balance
mise run test-network --jobs 1             # same default cases, serial
```

Standalone source engine/network tasks prepare once (locked restore/build/import).
The exported engine selection requires `--scenario worldtree` and an existing
native client package; it does not prepare source or implicitly export. CI shares
source preparation before overlapping completed C# binaries and headless network
workers. Engine cases run separately and serially, each with fresh owned preferences.
They instantiate production components in a minimal scene, never Main, the full
application or world. See the coverage ledger for
[`settings` resources/input/services](../../../docs/testing-audit.md#woodland-component-consolidation)
and [`receiving` HUD/actions/foreground research scope](../../../docs/testing-audit.md#combined-tabletop-receiving-coverage-2026-10-12),
and [Worldtree coverage](../../../docs/worldtree.md#verification-and-limits) for
its source/package probe boundary. Network scenarios use real headless application
peers; they are integration, not component tests.

One worker owns each child's ordered event reader. Ports use cross-process lease
handles plus ordinary bind-failure detection; unrelated endpoint owners are never
killed. Each scenario owns temporary XDG data/config/cache, resume credentials and
engine logs; restart reuses only its client's state. Every child is awaited on
failure/cancellation before runtime removal. `--startup-timeout-ms` is readiness,
`--timeout-ms` is the bounded scenario/suite allowance, not a retry policy.
Engine components default to 60000 ms each; an explicit `--timeout-ms` replaces
that budget, including readiness and process exit. Their `--startup-timeout-ms`
wait ends at the minimal component scene's readiness event.
For C# and network phases the default budget is 180000 ms and 300000 ms,
respectively; `ci-source`, `ci` and `test-deep` use 300000 ms per C#/network
phase, not one end-to-end command deadline. Preparation and cleanup are separate.
Readiness defaults to 15000 ms; hosted commands explicitly use 60000 ms.
Network/deep verification rejects non-loopback host and bind addresses.
Owned network setup accepts `--simulation-speed 1..8` (default 4); `--trace`
retains full protocol diagnostics at greater disk cost. These options are not
accepted by `test-deep`; select `test-network` for such diagnosis.
Never run two preparation/export tasks concurrently in one worktree.

## Explicit deep, CI and packages (headless)

```sh
mise run test-campaign
mise run test-campaign --filter 'FullyQualifiedName~FrontlineCampaignTests'
mise run test-deep                         # all deep C# + all eight retained headless network journeys
mise run ci-source --startup-timeout-ms 60000
mise run ci-linux-package --startup-timeout-ms 60000
mise run ci                               # sequential headless source, exports, package probes
```

Deep paid campaigns/calibration/reference replays are expensive compared with A.
`test-deep` launches no graphics. `test-network --scenario authority-resume-victory`
selects the retained deeper dedicated/research/resume journey. `defeat` is also
selectable. Package commands require existing matching verified export templates;
export outputs mutate sequentially. Normal export automation can acquire the
checksum-locked templates; this is not permission for an agent to download/install
under a task's prerequisite prohibition. Verify matching existing templates first
or report missing prerequisites instead of invoking acquisition.
Native `mise run ci-windows --startup-timeout-ms 60000` verifies Windows source,
export, bundled runtime inventory and headless offline solo on Windows only.

Release commands remain `release-preflight --tag`, `package-linux --tag`,
`package-windows --tag`, `assemble-release-assets --tag`, and explicitly selected
`verify-installed-linux|verify-installed-windows --tag`; see
[distribution](../../../docs/distribution.md) for identity, own-AppID and compiler
prerequisites. Installed qualification contains graphical checks: NOT headless
and NOT allowed under a headless-only task. These release/platform commands were
not executed by the overhaul; do not claim otherwise.

## Graphical qualification — explicit authorization only; UNEXECUTED in overhaul

These commands are supported selections, NOT verified graphical receipts:

```sh
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-ui --scenario launcher --checkpoint smoke
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-ui --scenario economy --checkpoint deep
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-ui --scenario combat --checkpoint research
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-ui --scenario combat --checkpoint melee
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-ui --scenario exported-package --checkpoint smoke
```

Launcher/economy/exported-package default to the same minimal one-application
smoke; select `deep` for legacy journeys. Other retained source selectors are
`reconnect`, `settings`, `combat`; their focused checkpoints remain in `Options.cs`
and runner help. No unfiltered graphical suite. Source prepares once; exported
selection uses existing packages without rebuilding. Private Linux displays own
Xvfb/Xauthority/Openbox, Compatibility/software OpenGL and Dummy audio; never use
a desktop fallback. Optional atmosphere/Vulkan/presentation profiles also require
the explicit graphical gate. Do not set it under headless-only authorization.

Other retained specialist entry points are `test-assets` (the legacy headless
asset probe), `test-native-pump` ([native diagnostic](../../../docs/native-pump-timing.md)),
`check-steam-extension` / `test-steam` ([Steam usage](../../../README.md#steam-development-and-acceptance)),
and `test-atmosphere` / `test-vulkan` ([renderer qualification](../../../docs/atmosphere.md#verification-and-visual-evidence)).
`check-ui-prerequisites` checks executable availability without launching Godot or
a display. Profiling/stress selections are documented in
[simulation performance](../../../docs/simulation-performance.md), not default tests.

For actual execution receipts and timing scope see
[verification](../../../docs/verification.md), not these copy-paste examples.
