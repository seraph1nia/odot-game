# Odot: Coin Room

A tiny C# multiplayer game and a Godot repository scaffold. Desktop clients send movement intent to a separate headless Godot server, which owns positions, coin pickups and scores.

## Setup

Install [mise](https://mise.jdx.dev/), Git, and the OS prerequisites for [.NET](https://learn.microsoft.com/dotnet/core/install/) and [Godot](https://docs.godotengine.org/en/stable/about/system_requirements.html). Linux graphical clients need OpenGL 3.3 and a display; automated tests and servers need neither graphics nor audio.

```sh
mise trust
mise install --locked dotnet godot-dotnet node
mise run test
```

Tools are declared in `mise.toml` and resolved in `mise.lock`: Godot **.NET** 4.7.2 and .NET SDK 10.0.401. Do not substitute the standard Godot edition or move the executable away from its GodotSharp files. The SDK selection in `global.json` is exact. NuGet dependencies are declared in the projects and checked with locked restores against the repository's configured nuget.org feed. Task execution never silently installs or upgrades global tools.

The initial host configurations cover Linux x86_64, Windows x86_64, and macOS Intel/Apple Silicon. CI verifies Linux only. Node/OpenSpec remain available for planning; neither is a gameplay dependency. Mise's core .NET backend uses its upstream installer; SDK archive metadata in the lock comes from Microsoft's release manifest. Refresh that metadata when deliberately changing SDK versions.

## Source layout

- `src/Game`: Godot scenes, C# networking/bootstrap and drawing; open `project.godot` in the .NET editor.
- `src/Game.Core`: Numerical world rules with no engine dependency.
- `tests/Game.Core.Tests`: Fast rules tests.
- `tools/DevRunner`: C# process supervision, integration checks and export preparation.
- `openspec`: Change planning and capability specifications.

Track source scenes/assets, source resource UID sidecars, project files, export presets and dependency/tool locks. Generated `.godot` data, `bin`, `obj`, `dist`, logs and local overrides are ignored. Use UTF-8/LF and `dotnet format Odot.slnx` for C# formatting. Commit the lockfiles with dependency updates.

`Odot.slnx` is the repository solution. `src/Game/Game.sln` is the small engine-local solution required by Godot's C# editor/exporter; it includes the game and its core dependency, while tests and tooling stay outside the Godot import root. The explicit game target framework prevents the editor from inserting its default framework during import.

The game keeps separate Debug and ExportRelease NuGet locks because Godot includes its editor assembly only in development builds. The game and core declare the Linux export runtime in advance so publishing does not rewrite their locks. Deliberate dependency updates require `dotnet restore --force-evaluate -p:RestoreLockedMode=false`; regenerate the game export lock with `-p:Configuration=ExportRelease -r linux-x64` as well, then run `ci` and review the resulting lock changes.

## Rules

Players move at a bounded speed inside the room. Coin contact awards one point; the server respawns the coin and sends the resulting world to every client. Simultaneous pickups have a stable player-ID tie-break and award only one point. A departing player's avatar and score are removed; reconnecting starts a new session. Fixed-step rules and seeded coin placement keep automated tests reproducible.

## Development commands

```sh
mise run dev
mise run server --bind 127.0.0.1 --port 7001
mise run client --host 127.0.0.1 --port 7001
mise run test
mise run test-network
mise run ci
```

`dev` builds/imports once, starts a headless server, awaits readiness, and opens two positioned client windows. Focus a window and use **WASD or arrow keys** to control its player. Stop the terminal command with Ctrl+C to stop its children. Closing a window also ends the supervised session. Individual role commands prepare the project themselves and accept the shown endpoint arguments. Default bind/host is loopback and default port is 7000. Bind to a LAN interface explicitly to play on another machine, using that server address on the client and allowing its UDP port.

`mise run prepare` checks tools, builds C# and imports resources. `mise run build` compiles only. `mise run check` verifies formatting. Direct Godot startup uses a role after the engine argument separator:

```sh
godot --headless --path src/Game -- --server --port 7001
godot --path src/Game -- --client --host 127.0.0.1 --port 7001
```

Run `prepare` first for direct engine commands. Headless is a display mode: automated clients also use it. It never selects server role. The dedicated-server export defaults to server role. Conflicting role flags and malformed arguments fail clearly. The shared RPC node is `/root/Game` in both roles; clients send directions for the RPC sender's own session, and only server-authoritative snapshots update the world. The server steps at 60 Hz and broadcasts at 20 Hz; there is no client prediction in this first demo.

## Verification and exports

`test` runs only core rules tests without Godot. `test-network` launches separate real ENet peers, observes authoritative snapshots, checks late joining, movement, pickup/score agreement and disconnect cleanup, and exercises bounded failure cases. Automated clients move through the normal RPC path. No test teleports players or directly awards scores.

The runner logs each process separately under ignored `logs/` and prints failed conditions with process output. Startup readiness has a 15-second deadline; the suite defaults to 60 seconds. Override with `--startup-timeout-ms` and `--timeout-ms` on network/dev tasks. Network tests default to a dynamically selected loopback UDP port; pass `--port` for a specific endpoint. An occupied explicit port fails without killing its owner. Graphical clients show connecting, connected, connection-failed and server-disconnected status.

`ci` performs locked restore, formatting checks, compilation/import, core tests and the network suite **before** exporting Linux x86_64 client and dedicated-server builds. It fails immediately on a failed check and checks that the exported programs start. GitHub Actions runs the same task on pushes and pull requests with read-only permissions. Outputs stay in the runner workspace: **no artifact uploads, releases, publishing or deployment**.

```sh
mise run prepare-templates
mise run export-client
mise run export-server
```

Matching .NET export templates are downloaded explicitly, checksum-verified and installed in Godot's user template directory. This is engine data, separate from mise tools and NuGet dependencies. Repeating preparation is safe. Exports go to ignored `dist/client` and `dist/server`. Individual export tasks are available for local iteration; CI applies the test gate before invoking them. Windows/macOS clients are desktop targets, with Linux the first export/CI platform.

See [scaffold verification](docs/verification.md) for the checks performed and desktop hosts still needing runtime verification.
