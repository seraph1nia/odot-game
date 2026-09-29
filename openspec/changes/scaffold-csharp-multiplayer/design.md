# Design

## Context

See `proposal.md` for motivation and scope. Inspection found only the existing `mise.toml` (Node 24 and OpenSpec 1.13.2), OpenSpec configuration, and generated skills. There are no source files, tests, workflows, or existing capability specs to migrate. The selected deployment model is desktop clients and a headless Godot server; the first CI platform is Linux x86_64.

Godot C# requires the .NET edition rather than the standard engine distribution. The mise `godot` registry entry currently points to the standard Aqua package. Mise's GitHub backend supports explicit per-platform asset selection and retaining archive contents, so use that route for the official .NET distribution. Version choice is a tooling implementation detail: select a stable Godot 4 release and a compatible supported .NET SDK/runtime, then record exact resolved versions and ensure the engine, Godot SDK package, and export templates agree.

## Goals / Non-Goals

**Goals:**
- Keep game rules independent of rendering and networking, with the authoritative server invoking the same rules exercised by fast tests.
- Make interactive development and headless verification use the same startup and transport paths.
- Provide repeatable task execution across desktop hosts, with CI checking and exporting on Linux.
- Keep the dependency and process-management surface small enough for a first scaffold.

**Non-Goals:**
- A reusable entity/component framework, dependency-injection container, transport abstraction framework, or engine-independent standalone server.
- Cross-platform deterministic physics, prediction, reconciliation, or rollback; this demo uses simple numerical movement and collection rules.
- Production network simulation, performance benchmarks, or a full graphical UI automation suite.
- Cross-platform export matrices or distribution infrastructure in the initial CI workflow.

## Decisions

### 1. One Godot project with a small rules library

Use a root .NET solution containing:

```text
src/Game/           Godot project, C# adapter code, scenes, export presets
src/Game.Core/      Plain C# movement, world state, pickups and scores
tests/Game.Core.Tests/  Rules tests
tools/DevRunner/    Small C# console supervisor and network test driver
```

`Game` references `Game.Core`; the core has no Godot dependency. Keep the other projects outside the Godot project directory to avoid implicit source discovery and unnecessary asset imports. Godot scene files describe composition, while attached C# files have matching class/file names. Use native drawing primitives or tiny source assets for the demo. Add a compatibility renderer, input map, and a single bootstrap scene. Shared .NET configuration enables nullable checks, analyzers, and consistent formatting.

This gives rules tests no native-engine dependency and avoids duplicating rules between two Godot projects. Keeping everything inside the game assembly would reduce project count but make ordinary rule tests depend on engine hosting. A separate server project would add scene/protocol coordination without helping the agreed headless model.

### 2. Explicit startup roles and ENet transport

Select roles through user arguments after Godot's `--` separator: `--server` or `--client`, plus `--bind`, `--host`, and `--port` as applicable. A graphical launch without a role defaults to a client on loopback. Dedicated-server exports default to the server role. An explicit role overrides that default; conflicting role flags fail. `--headless` changes display behavior only and never determines the networking role, since automated clients also run headlessly. Reject malformed arguments and report listen/connect failures clearly.

Use `ENetMultiplayerPeer` and C# RPCs on a shared network node with an identical path and RPC contract in both roles. The server owns the world; a client sends only its intended direction to the server. Resolve player identity from the RPC sender, and only accept authoritative state messages from the server. Include a small protocol version in initial session setup to fail incompatible clients clearly.

Use reliable session setup/state initialization messages, unreliable ordered movement intent and periodic snapshots. A snapshot contains its simulation tick, player identities/positions/scores, and coin generation/position. Send full snapshots in this tiny world to make late joins and recovery from packet loss simple; clients ignore stale ticks. Use a fixed 60 Hz server step and initially send snapshots at 20 Hz. Reject non-finite inputs, cap direction magnitude, accept only one current intent per peer, and reset intent after a short input-silence timeout. A packet flood must not advance the simulation faster than the server tick.

The built-in transport minimizes networking dependencies. A standalone .NET backend would require its own protocol and simulation integration; the user chose headless Godot. Custom low-level packets offer more control but are unnecessary for a two-player demo. See [Godot multiplayer](https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html) for C# RPC support and matching node-path requirements.

### 3. Simple reproducible simulation

Core world state stores players, scores, the active coin generation, and room bounds. Movement consumes validated directions and a fixed step, clamps positions, and checks coin overlap without relying on engine physics. Evaluate all pickup candidates against the coin that existed at the start of the step; select the lowest player ID on a tie, award one point, and create the next coin only after selection. Do not collect a replacement coin again in that step. Seed coin placement for repeatable tests, and choose valid spawn positions that avoid immediate overlap with players.

The server creates/removes session players and publishes complete current state. A disconnected player's score is removed, matching the session-only lifecycle spec. Clients render snapshots and show a sorted scoreboard, local-player indication, and connection status. WASD and arrow keys control each focused client window. Interpolation can smooth rendering, but the first milestone requires no local prediction.

Pure numerical rules give predictable tests and straightforward authority. Engine physics would be appropriate for richer collision gameplay later; no such requirement exists yet. Same-seed determinism applies to the core's fixed-step input sequence, not peer assignment or wall-clock network scheduling.

### 4. Mise manages tools; NuGet manages application dependencies

Keep existing tool entries. Add the .NET SDK via the short `dotnet` registry name and a clearly named alias backed by official Godot GitHub releases. Configure explicit stable .NET/`mono` asset selection for Linux x86_64, Windows x86_64, and universal macOS builds. Preserve the accompanying GodotSharp directory when exposing a stable executable name. Avoid automatic asset guessing, which can select the standard engine or templates instead of the .NET editor.

Use major-version tool declarations consistent with inherited repository guidance, and commit a tool lock recording exact resolved release versions, asset URLs, and checksums for the supported hosts. Stable-only Godot resolution must exclude development, beta, and release-candidate tags. Match `Godot.NET.Sdk` to that locked engine release. Align a root `global.json` and project target framework with the selected compatible SDK/runtime so tests and the engine run from the declared toolset without undocumented runtime roll-forward. Lock NuGet dependencies and use locked restores in CI.

Matching .NET export templates are versioned data, not an independent global executable. Provide an explicit, idempotent template-preparation task that downloads the locked release's official template archive, verifies its checksum, and installs it into the correct Godot template directory. Keep downloads and installed templates out of Git. Tool preflight reports versions, missing C# support, and SDK/engine/template mismatches. Project preparation restores dependencies, compiles C#, and runs the editor's headless import command; import alone does not substitute for a C# build.

Developer setup documents user-run tool installation; project tasks do not silently install or upgrade global tools. CI bootstraps the locked tools in its disposable runner and prepares templates explicitly. No new task manager, container runtime, or global formatter is needed: mise exposes tasks, .NET handles builds/formatting, and NuGet holds test dependencies. See [Mise GitHub backend](https://mise.jdx.dev/dev-tools/backends/github.html), [Mise .NET support](https://mise.jdx.dev/lang/dotnet.html), and [Godot C# setup](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html).

### 5. A small C# supervisor owns process lifecycle

Use .NET process APIs in `tools/DevRunner`, with argument lists instead of shell command strings. This provides a single cross-platform place to supervise children, read stdout/stderr, handle cancellation, and terminate process trees. It adds no third-party orchestration dependency. Shell background jobs would need differing signal and cleanup behavior on Windows and Unix; Docker would add a development prerequisite without benefiting this scaffold.

Tasks expose these responsibilities:

| Task | Responsibility |
|---|---|
| `dev` | Prepare game, start server, await readiness, open two positioned client windows; stop all children on cancellation or unexpected exit |
| `server` | Prepare and start one headless server with configurable bind/port |
| `client` | Prepare and start one graphical client with configurable host/port |
| `test` | Restore and execute only engine-independent rules tests |
| `test-network` | Prepare game and run bounded network verification with headless processes |
| `ci` | Run the ordered verification/export pipeline described below |
| `build`, `check`, `prepare`, `prepare-templates`, `export-client`, `export-server` | Reusable supporting operations with documented inputs and failure codes |

Expose argument forwarding in mise and document examples for connecting to another local server. Emit server readiness only after successful socket binding and world initialization. Prefix human-readable logs by role, and use a distinct prefix followed by JSON for machine-readable lifecycle/state events in automated mode. Retain diagnostics in ignored local log files and print failure details in CI console output. Give graceful exits a short deadline, then terminate only process trees created by the supervisor. Default startup deadline is 15 seconds and network-suite deadline is 60 seconds, both configurable. A port collision fails clearly or retries another port within the deadline; no fixed sleep implies readiness.

### 6. Layered verification through the normal game path

Use a lightweight C# test project (xUnit via NuGet) for meaningful rules assertions: speed limiting, room boundaries, non-finite input, same-step competing pickups, score increments, valid replacement coins, and reproducible state for fixed seed/input sequence. Avoid tests that merely mirror file layout or constants.

For integration, the supervisor starts the server with a controlled seed and an automated client A. Test-mode clients accept local scenario directives and turn them into normal movement input; they never teleport players, award scores, or alter server state directly. Events report the authoritative snapshots actually received over ENet. After A moves and collects a coin, start client B and assert it receives the existing world and score. With both present, command A to move and collect the next coin, stop its input, and assert both observe the position change, matching score/coin generation, and exactly one point for that generation. Disconnect B and assert A receives its removal. This sequence covers late join and shared state with two clients total.

Compare reports for the same snapshot tick/generation or a stable stopped state, rather than demanding equal positions at arbitrary wall-clock instants. Fail on child crashes, timeouts, failed assertions, or missing events, and always clean up. Include an unavailable-server case to verify bounded client failure and a server-stop case to verify disconnect feedback. Automated headless launch uses the same bootstrap, RPCs, and rules as graphical clients. Human visual verification of two windows supplements the headless tests.

An engine testing plugin would add a dependency before we need extensive scene tests. Real separate processes catch connection, RPC-path, import, and headless-startup problems that mocked network tests would miss.

### 7. Verification gates Linux exports and produces no published output

Use one GitHub Actions workflow on pushes and pull requests, with `contents: read`, explicit job timeout, and managed tool setup. Call `mise run ci` so local and CI ordering stays identical. The pipeline performs locked restore, formatting checks, compilation and Godot import required by tests, rules tests, and the full network suite. Only after all pass does it prepare matching templates and export release Linux x86_64 client and dedicated-server outputs into ignored `dist/client/` and `dist/server/` directories. Abort on the first failed phase; do not express exports as independent parallel prerequisites or use failure-tolerant steps.

Compilation before tests is necessary to execute C#; the user's requested post-test build gate applies to deliverable exports. Check export exit codes and launch each exported role for a bounded startup smoke check after both exports, so a broken package fails CI. Use the dedicated-server export feature and avoid references to stripped client-only assets. No artifact-upload action, GitHub release operation, package/container push, or deployment step is included. Logs stay in the workflow console and outputs disappear with the runner. A separate release workflow can be designed later.

See [Godot dedicated server exports](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_dedicated_servers.html) for headless execution and resource stripping.

### 8. Track portable source and ignore generated data

Add `.editorconfig`, `.gitattributes`, and `.gitignore`; retain source scenes, assets, Godot UID sidecars where generated for source resources, `export_presets.cfg`, the solution, project files, tool/dependency locks, and SDK selection. Ignore `.godot/`, `bin/`, `obj/`, `dist/`, diagnostic logs, local overrides, and export credentials. Document commands, controls, default endpoint, architecture, platform prerequisites, and setup troubleshooting in README. No engine binaries or build outputs are checked into the repository. Existing OpenSpec context/configuration and generated skills are left intact.

## Risks / Trade-offs

- [Godot .NET distribution has platform-specific archive layout] --> Explicit asset patterns and retained supporting files, version preflight, and a clean Linux CI checkout; document the initial supported host architectures.
- [SDK, framework, engine package, and templates can drift] --> Resolve a compatible stable set, record exact versions, restore from locks, and fail mismatches before launch/export.
- [Headless tests miss visual layout and input focus issues] --> Add a two-window manual check and keep rendering simple; headless clients still exercise networking and rules.
- [Scheduling can make network tests flaky] --> Seed the world, coordinate phases through events, compare matching snapshots, and use bounded condition waits rather than timing assumptions.
- [Simple server-owned movement has visible latency on remote connections] --> Keep this milestone focused on local verification; interpolation is allowed, while prediction is a later design decision.
- [Linux-only CI leaves other desktop hosts less verified] --> Use portable .NET process APIs and explicit platform assets, document Windows/macOS setup, and avoid claiming automated certification outside Linux.
- [Successful CI builds cannot be downloaded] --> This is deliberate user scope: inspect logs and reproduce exports locally; introduce uploads or releases only in a later change.

## Migration Plan

This is an additive greenfield scaffold with no saved data, existing protocol, or deployed service to migrate. Implement tooling/source foundations first, then gameplay and process orchestration, then verification and export configuration. Validate from a clean checkout before considering the scaffold complete. Rollback removes the introduced source/tooling/workflow files and restores `mise.toml`; existing OpenSpec setup remains intact. There is no deployment step.
