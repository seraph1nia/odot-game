# Odot: Nine Tiles

A small cooperative city-defense POC in C# and Godot. Each player builds on nine indexed hex plots, recruits persistent soldiers, and survives three automatic waves. A separate headless server owns every resource, action and combat result; clients can resume their cities after disconnecting.

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

- `src/Game`: Godot scenes, C# networking/bootstrap, tabletop and controls; open `project.godot` in the .NET editor.
- `src/Game.Core`: Numerical match rules with no engine dependency.
- `tests/Game.Core.Tests`: Fast rules tests.
- `tools/DevRunner`: C# process supervision, integration checks and export preparation.
- `openspec`: Change planning and capability specifications.

Track source scenes/assets, source resource UID sidecars, project files, export presets and dependency/tool locks. Generated `.godot` data, `bin`, `obj`, `dist`, logs and local overrides are ignored. Use UTF-8/LF and `dotnet format Odot.slnx` for C# formatting. Commit the lockfiles with dependency updates.

`Odot.slnx` is the repository solution. `src/Game/Game.sln` is the small engine-local solution required by Godot's C# editor/exporter; it includes the game and its core dependency, while tests and tooling stay outside the Godot import root. The explicit game target framework prevents the editor from inserting its default framework during import.

The game keeps separate Debug and ExportRelease NuGet locks because Godot includes its editor assembly only in development builds. The game and core declare the Linux export runtime in advance so publishing does not rewrite their locks. Deliberate dependency updates require `dotnet restore --force-evaluate -p:RestoreLockedMode=false`; regenerate the game export lock with `-p:Configuration=ExportRelease -r linux-x64` as well, then run `ci` and review the resulting lock changes.

## Gameplay

Read [the rules and a tested winning strategy](docs/gameplay.md). Gold buys mines, farms and barracks; food recruits soldiers by clicking a barracks. Upgrade buildings once. All connected living players click Ready to produce; every third production starts an automatic battle. Soldiers and city damage persist. Fallen cities send their living attackers and future allocations to surviving teammates. Clear three waves to win.

The graphical client uses vendored free KayKit assets in a grass-and-river hex landscape with a lower angled view. Click a plot or visible building, then use the bottom panel to Build, Upgrade or Recruit. Scenery is decorative; the nine slots and automatic battles keep the same rules. Roster tabs inspect any city; only your own can be edited. Ready locks editing until Unready. Any connected member, including an eliminated observer, can pause/resume the entire match. Costs and unavailable-action explanations come from authoritative state.

## Development commands

```sh
mise run dev
mise run server --bind 127.0.0.1 --port 7001
mise run client --host 127.0.0.1 --port 7001 --session-file .sessions/player-a.json
mise run test
mise run test-network
mise run ci
```

`dev` builds/imports once, starts a headless server, awaits readiness, and opens two positioned client windows. Wait for both players in the lobby, then click **Start match**. Click a plot or building directly and use the bottom controls. Stop the terminal command with Ctrl+C to stop its children. Closing a window also ends the supervised session. Individual role commands prepare the project themselves and accept the shown endpoint arguments. Default bind/host is loopback and default port is 7000. Bind to a LAN interface explicitly to play on another machine, using that server address on the client and allowing its UDP port.

`mise run prepare` checks tools, builds C# and imports resources. `mise run build` compiles only. `mise run check` verifies formatting. Direct Godot startup uses a role after the engine argument separator:

```sh
godot --headless --path src/Game -- --server --port 7001
godot --path src/Game -- --client --host 127.0.0.1 --port 7001
```

Run `prepare` first for direct engine commands. Headless is a display mode: automated clients also use it. It never selects server role. The dedicated-server export defaults to server role. Conflicting role flags and malformed arguments fail clearly. The shared RPC node is `/root/Game` in both roles. Protocol v2 replaces the coin demo. Reliable requests carry match/phase/unique turn serial and command sequence; the authenticated sender determines city ownership. The server validates costs and eligibility before atomically changing state. Stable-player command ledgers prevent duplicate spending even after reconnect. Complete initial/resume snapshots enable input only after synchronization. Revision-ordered full snapshots use reliable channel one at 20 Hz because complete multi-city state exceeds the unreliable MTU. Combat uses fixed 60 Hz numerical steps, without physics or client prediction; visuals are created only by graphical clients.

## Verification and exports

`test` runs only core rules tests without Godot. `test-network` launches separate real ENet peers and verifies sender ownership, economy/upgrades/recruitment, readiness, automatic battles, pause, client-process restart/resume, duplicate spending, locked-roster/protocol/session rejection, three-wave victory, actual city elimination and enemy redistribution, future allocations, observer resume and defeat. Actions use the ordinary protocol and standard balance; no RPC awards resources or alters health. Failure cases cover unavailable/stopped servers, occupied ports, missing readiness and child exit.

The runner logs each process separately under ignored `logs/` and prints failed conditions with process output. Startup readiness has a 15-second deadline; the suite defaults to 180 seconds. Override with `--startup-timeout-ms` and `--timeout-ms` on network/dev tasks. Network tests default to a dynamically selected loopback UDP port; pass `--port` for a specific endpoint. An occupied explicit port fails without killing its owner. Graphical clients show connecting, connected, connection-failed and server-disconnected status.

`ci` performs locked restore, formatting checks, compilation/import, core tests and the network suite **before** exporting Linux x86_64 client and dedicated-server builds. It fails immediately on a failed check and checks that the exported programs start. GitHub Actions runs the same task on pushes and pull requests with read-only permissions. Outputs stay in the runner workspace: **no artifact uploads, releases, publishing or deployment**.

```sh
mise run prepare-templates
mise run export-client
mise run export-server
```

Matching .NET export templates are downloaded explicitly, checksum-verified and installed in Godot's user template directory. This is engine data, separate from mise tools and NuGet dependencies. Repeating preparation is safe. Exports go to ignored `dist/client` and `dist/server`. Individual export tasks are available for local iteration; CI applies the test gate before invoking them. Windows/macOS clients are desktop targets, with Linux the first export/CI platform.

See [POC verification](docs/verification.md) for the checks performed and desktop hosts still needing runtime verification.

## Reconnect and local sessions

Resume works against the **same running server**. Disconnect clears readiness but retains your board, resources, army, damage and original wave allocation. Your city keeps producing and fighting; teammates can pause before you return. Disconnected and eliminated players do not block readiness. With nobody connected and alive, the building phase waits. Combat continues unless paused.

For independent windows/processes use separate files:

```sh
mise run server --port 7001
mise run client --port 7001 --session-file .sessions/player-a.json
mise run client --port 7001 --session-file .sessions/player-b.json
```

After a client exits, relaunch its exact client command to recover the city. A disconnected open window offers Reconnect. Start with all players present; fresh joins after start are refused. If the server was replaced, the credential is expired and the client explains it. Use Join lobby with a fresh session deliberately to join the new lobby. There is no server save, account login, host migration or rematch; restart the server for another match.

The runner assigns different temporary paths to each `dev` run's A/B windows and uses isolated temporary paths for network tests. The direct game accepts `--session-file`; its default is endpoint-scoped under Godot's user data directory, so two direct clients must specify different files. Private session files contain a credential and next command sequence, are written atomically, and must not be committed. Credentials never appear in shared snapshots or event logs. A sequence is reserved before sending; retries preserve identity, rejected identities cannot be repurposed, and new actions reserve new identities. This is a local/LAN prototype with possession-based resume credentials, not a production account service.

See [asset provenance and mapping](src/Game/Assets/KayKit/README.md). Assets are available from a clean checkout; runtime never downloads them. Only gold and food are gameplay resources.
