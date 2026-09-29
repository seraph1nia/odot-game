# Tasks

## 1. Managed tools and repository foundation

- [x] 1.1 Extend `mise.toml` with a compatible stable Godot 4 .NET distribution and .NET SDK, retaining existing Node/OpenSpec entries; configure explicit host asset selection and record exact resolved versions/checksums in a tool lock. Verify tool resolution selects the .NET archive for Linux x86_64, Windows x86_64, and macOS, excludes prereleases, and preserves GodotSharp beside the executable.
- [x] 1.2 Create the root .NET solution and `src/Game`, `src/Game.Core`, `tests/Game.Core.Tests`, and `tools/DevRunner` projects, with compatible target frameworks, matched `Godot.NET.Sdk`, SDK selection, nullable/analyzer settings, and NuGet locks. Verify locked restore and solution compilation with the declared tools and confirm the core/test projects have no Godot dependency.
- [x] 1.3 Add `.editorconfig`, `.gitattributes`, `.gitignore`, and initial README setup/structure documentation; verify source assets/project identifiers stay tracked while engine caches, .NET outputs, logs, local settings, and exports are ignored, and document user-run tool installation and OS prerequisites.

## 2. Engine-independent gameplay rules

- [x] 2.1 Implement fixed-step world state, player session creation/removal, and bounded movement; add rules tests for valid movement, diagonal speed, non-finite/oversized inputs, room bounds, and session removal, and verify they pass without starting Godot.
- [x] 2.2 Implement coin generations, overlap collection, one-point awards, stable same-step tie-breaking, and replacement spawning; add tests for single pickups, competing players, no repeated collection in one step, and valid non-overlapping replacement positions, and verify each expected score/state transition.
- [x] 2.3 Add seeded world initialization and deterministic fixed-input tests, expose `mise run test`, and document rules and test invocation; verify identical seeds/input sequences produce identical core state and that a deliberately violated rule assertion returns a nonzero test status.

## 3. Godot bootstrap, authoritative networking, and demo

- [x] 3.1 Create the bootstrap scene, compatibility renderer, explicit role/endpoint parsing, and tool preflight/project-preparation tasks with C# build and headless import. Verify a headless server binds loopback, an explicit headless client does not listen, malformed flags fail clearly, and missing C# support or SDK mismatches are actionable.
- [x] 3.2 Implement the shared ENet/RPC contract, sender-based player ownership, protocol compatibility check, fixed server stepping, input-silence reset, and ticked full snapshots/late-join initialization. Verify two separate headless clients connect, receive the same world, and a malformed input or spoofed player command cannot change another player's authoritative state.
- [x] 3.3 Render the room, distinct players, coin, local-player indication, and scoreboard; wire WASD/arrow input and connection lifecycle feedback. Verify two graphical clients show synchronized movement/pickups, a late join receives existing scores, disconnect removes the player, and unavailable/stopped servers produce visible feedback.
- [x] 3.4 Document engine startup flags, controls, session lifecycle, and networking authority in README; verify the documented server/client examples and headless role selection against the implemented bootstrap.

## 4. Local process orchestration

- [x] 4.1 Implement the C# supervisor with safe argument forwarding, readiness events, attributable stdout/stderr, configurable deadlines, cancellation, and owned-process-tree cleanup. Verify startup failure, interruption, and unexpected child exit return the expected status and leave no supervised children running; an occupied port must not cause unrelated process termination.
- [x] 4.2 Expose `mise run dev`, `mise run server`, and `mise run client`, with preparation and endpoint arguments; verify `dev` waits for server readiness then opens two distinct client windows, stopping it cleans up all children, and independently launched roles connect on a non-default port.
- [x] 4.3 Document local commands, address/port overrides, logging, startup deadlines, and cleanup behavior; verify examples from the repository root with no IDE configuration and review Windows/macOS process and path handling for portable .NET APIs.

## 5. Automated network verification

- [x] 5.1 Add explicit automated-client mode, local bot directives that generate normal movement inputs, and machine-readable lifecycle/snapshot events. Verify automated clients use the production ENet/RPC path and report received state without bypassing server movement or scoring.
- [x] 5.2 Implement `mise run test-network`: start a seeded headless server and client A, move/score, late-join B, then verify movement observed by both, exactly one point for a coin generation, matching scores/coin state, and removal after B disconnects. Verify successful runs need no display/audio, compare matching snapshot ticks or stable states, and clean up all processes.
- [x] 5.3 Add unavailable-server, server-stop, readiness-timeout, and port-collision checks, and verify missing synchronization or a child crash causes a bounded nonzero result with useful logs and full cleanup. Document network-test inputs, deadlines, diagnostics, and exit codes in README.

## 6. CI verification and Linux exports

- [x] 6.1 Implement checksum-verified preparation of the locked engine's matching .NET export templates and Linux x86_64 client/dedicated-server export presets/tasks; verify preparation is idempotent, mismatched templates fail clearly, and both exported roles start from their output directories using no editor files.
- [x] 6.2 Implement `mise run ci` as ordered formatting/locked-restore/compilation/import checks, rules tests, network tests, then template preparation and client/server exports with bounded exported-role smoke checks. Verify a deliberately failing rules check and a failing network check each prevent both export operations, and an export or package-startup failure makes the pipeline fail.
- [x] 6.3 Add GitHub Actions for pushes/pull requests on Linux with declared locked tools, read-only repository permissions, and a job timeout; call `mise run ci` and verify the workflow contains no artifact uploads, release operations, package/container publishing, or deployment steps. Document the pipeline order, local reproduction, and workspace-only build outputs.

## 7. Complete scaffold verification

- [x] 7.1 Run the documented setup/preparation and full verification pipeline from a clean checkout with the declared tools; verify there are no dependencies on editor-created caches, machine-specific paths, undeclared runtimes, or untracked source identifiers, and verify generated output does not dirty tracked source.
- [x] 7.2 Run a complete interactive two-window session plus the headless network suite and Linux export smoke checks; confirm every capability scenario is covered by automated assertions or recorded manual verification, and report any desktop host checks that could not be executed rather than claiming them as passed.
