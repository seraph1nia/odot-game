# Odot: Nine Tiles

Basic buildings use small wood costs; gold remains for land and advanced buildings. The top-right resource table shows exact Stock and Income/turn, with a compact Upkeep table underneath. Food stays at its original scale. See [gameplay defaults](docs/gameplay.md).


A small cooperative city-defense POC in C# and Godot. Each player starts with five of nine indexed hex plots, equips persistent leveled soldiers, and survives twenty automatic waves. Solo uses a local authority; multiplayer uses the original playing host or an explicit dedicated server. Guests render authoritative snapshots and can resume retained cities against the same running authority.

## License and contributions

Odot's original code and documentation are licensed under the [GNU General Public License v3.0 only](LICENSE) (`GPL-3.0-only`). Copyright (c) 2026 Odot contributors. You may use, modify and redistribute them under that license; distributed derivatives must comply with its source-sharing requirements.

Development is maintainer-led, and external contributions and pull requests are not accepted. Write access, including branch creation in this repository, is limited to authorized maintainers. You may fork the project and develop your own branches under the applicable licenses.

Third-party materials retain their own terms and are excluded from Odot's GPL grant. See the [KayKit asset notices](src/Game/Assets/KayKit/README.md), [GodotSteam and Valve runtime notices](src/Game/addons/godotsteam/README.odot.md), and [music provenance](src/Game/Assets/Music/README.md). The supplied music has no documented redistribution license; its inclusion does not grant permission to reuse or redistribute it.

## Setup

Combat math runs entirely in C# under `src/Game.Core`: one canonical action per
unit, shared integer graph search and complete defense profiles. `mise run test`
verifies transitions, targeting, simultaneous impacts and lifecycle behavior
without starting Godot. See
[gameplay rules and compatibility](docs/gameplay.md#deterministic-hex-combat) and
[verification evidence](docs/verification.md).

Install [mise](https://mise.jdx.dev/), Git, and the OS prerequisites for [.NET](https://learn.microsoft.com/dotnet/core/install/) and [Godot](https://docs.godotengine.org/en/stable/about/system_requirements.html). Linux graphical clients need OpenGL 3.3 and a display. Rules/network tests and servers are headless; graphical verification uses an owned virtual display and software OpenGL without a physical screen, GPU or audio device.

```sh
mise trust
mise install --locked dotnet godot-dotnet node
mise run test
```

Tools are declared in `mise.toml` and resolved in `mise.lock`: Godot **.NET** 4.7.2 and .NET SDK 10.0.401. Do not substitute the standard Godot edition or move the executable away from its GodotSharp files. The SDK selection in `global.json` is exact. NuGet dependencies are declared in the projects and checked with locked restores against the repository's configured nuget.org feed. Task execution never silently installs or upgrades global tools.

The initial host configurations cover Linux x86_64, Windows x86_64, and macOS Intel/Apple Silicon. Ordinary CI verifies Linux and native Windows x86_64 distribution paths; native Windows installer qualification remains deferred. macOS is not a release target. Node/OpenSpec remain available for planning; neither is a gameplay dependency. Mise's core .NET backend uses its upstream installer; SDK archive metadata in the lock comes from Microsoft's release manifest. Refresh that metadata when deliberately changing SDK versions.

Private-display UI verification currently supports Linux x86_64. Install its OS packages yourself; runner tasks report missing prerequisites and never install them or fall back to desktop windows:

```sh
# CachyOS / Arch
sudo pacman -S --needed xorg-server-xvfb xorg-xauth xorg-xdpyinfo mesa-utils mesa libglvnd openbox
# Ubuntu 24.04
sudo apt-get install xvfb xauth x11-utils mesa-utils libgl1-mesa-dri libglx-mesa0 openbox
mise run check-ui-prerequisites
```

The runner needs `setsid` from util-linux (normally present), and verifies Mesa software OpenGL >= 3.3 on its own X11 display. Executable preflight starts neither Godot nor a display. Graphical clients use Dummy audio; native compositor, GPU performance, physical input and listening still need targeted manual checks.

## Source layout

- `src/Game`: Godot scenes, C# networking/bootstrap, tabletop and controls; open `project.godot` in the .NET editor.
- `src/Game.Core`: Numerical match rules with no engine dependency.
- `tests/Game.Core.Tests`: Fast rules tests.
- `tests/DevRunner.Tests`: Cheap scheduler, ownership, selection, UI-response and gate regressions.
- `tools/DevRunner`: C# process supervision, integration checks and export preparation.
- `openspec`: Change planning and capability specifications.

Track source scenes/assets, source resource UID sidecars, project files, export presets and dependency/tool locks. Generated `.godot` data, `bin`, `obj`, `dist`, logs and local overrides are ignored. Use UTF-8/LF and `dotnet format Odot.slnx --no-restore` after a locked restore for C# formatting. Commit the lockfiles with dependency updates.

All C# projects inherit strict compilation from `Directory.Build.props`: nullable reference checking and implicit usings are enabled, and compiler/analyzer warnings fail the build. The locked SDK supplies the recommended .NET analyzers (`AnalysisLevel=latest-recommended`) without an extra NuGet dependency. Style analysis also runs during compilation; `.editorconfig` requires file-scoped namespaces and readonly fields where possible, and the formatting check rejects whitespace drift. Fix diagnostics rather than weakening enforcement or suppressing findings.

The verification loop is `mise run check` (locked restore and `dotnet format --verify-no-changes`), `mise run build`, then `mise run test`, followed by the affected `test-network`/`test-ui` slice. `mise run ci` requires formatting, strict compilation, both xUnit suites, all Godot network/source UI checks and both exported-package smoke checks, while sharing source preparation and gating exports as described below.

`Odot.slnx` is the repository solution. `src/Game/Game.sln` is the small engine-local solution required by Godot's C# editor/exporter; it includes the game and its core dependency, while tests and tooling stay outside the Godot import root. The explicit game target framework prevents the editor from inserting its default framework during import.

The game keeps separate Debug and ExportRelease NuGet locks because Godot includes its editor assembly only in development builds. The game and core declare the `linux-x64` and `win-x64` export runtimes in advance so publishing does not rewrite their locks. Deliberate dependency updates require `dotnet restore --force-evaluate -p:RestoreLockedMode=false`; regenerate the game export lock with `-p:Configuration=ExportRelease` as well, then run `ci` and review the resulting lock changes.

## Gameplay

Read [the rules and tested campaign strategies](docs/gameplay.md). Six resources fund the city: gold, wood, food, stone, metal and cloth. Recruitment spends equipment materials; food is paid once before each battle, with unfed veterans kept in reserve. Recruitment buildings reach level five, producers and defenses level two, and Markets level one. Purchase locked plots for gold, sell buildings for half their actual investment, or build a Market to sell resource bundles. All connected living players click Ready for each of three productions, then Ready again in income-free Preparation to begin battle. Unit levels, wounds, city damage and research persist. Fallen cities transfer living attackers and retain their share of future enemy pressure. Clear all twenty waves, including bosses at ten and twenty, to win.

Each city earns one personal research point per shared wave clear. Research Towers add +1/+2 per full three-production cycle, retaining partial progress; multiple towers add together. Open **Research** without selecting a plot to buy 3/6/9-point foundations, exclusive specializations and masteries. Points and choices reset each match; sale preserves them. Fire gives burn, Venom poison, and Frost slows future actions. Inspector text and status badges show current effects. Freeze/stun are deferred; incompatible older peers are refused.

The graphical client uses vendored free KayKit assets in a grass-and-river hex landscape with a lower angled view. Click a plot or visible building, then use the bottom panel to Build, Upgrade or Recruit. Scenery is decorative; the nine slots and automatic battles keep the same rules. Roster tabs inspect any city; only your own can be edited. Ready locks editing until Unready. Any connected member, including an eliminated observer, can pause/resume the entire match. Costs and unavailable-action explanations come from authoritative state.

## Development commands

```sh
mise run dev
mise run dev --guests 3
mise run play
mise run server --bind 127.0.0.1 --port 7001
mise run client --host 127.0.0.1 --port 7001 --session-file .sessions/player-a.json
mise run test
mise run test-network
mise run test-network --scenario redistribution
mise run test-network --jobs 1
mise run test-ui --scenario economy
mise run test-ui --scenario reconnect
mise run test-ui --scenario settings
mise run test-ui --scenario launcher
mise run test-ui --scenario combat
mise run test-ui --scenario combat --checkpoint melee
mise run test-ui --scenario combat --ui-checkpoint research
mise run ci
```

`dev` builds/imports once and opens two playable desktop windows: one playing host and one guest, using ENet without Steam. `--guests 1..3` changes the guest count; the host owns the first city. Wait for the roster, then click **Start match** in the host window. Each process has isolated temporary preferences and credentials. Closing a window or Ctrl+C stops only supervised peers. `play` opens the normal start screen. Individual roles prepare themselves and keep endpoint arguments. Default bind/host is loopback and default port is 7000. Bind a LAN interface explicitly for ENet play on another machine, allowing its UDP port.

In the tabletop, scroll up/down over the world to zoom in/out toward the ground point under the cursor (overview to 3×). Hold **WASD or arrow keys** to pan around the observed city. Click the world to release HUD keyboard focus when using arrows. The angle stays fixed and travel is bounded. **Space** restores the complete overview. Left-click and drag the world to pan; a short click selects a plot or unit. Switching cities or starting a fresh match resets the view; resizing and reconnecting to the same city/match retain relative adjustments. Camera state is local and is not saved across launches. The top-right table shows the observed city’s six resource stocks and income, with compact upkeep underneath; use the small city arrows and Details for cooperative inspection and army allocation. Click a living unit for its model preview, name, description, level, health and damage; click elsewhere to dismiss it. Unit bars show Roman levels and the home bar shows health percent. Return to menu is in Settings and asks for confirmation. You can inspect while paused or disconnected. HUD scrolling, dialogs, text entry and focused UI navigation take priority; window focus loss or a dialog stops held movement until you release and press again.

The village sits in continuous medieval countryside that fills the world view, including permitted zoom/pan positions and window changes. Buildings, the home and defender align to supporting hex footprints; resource groups and props sit on their local terrain, and upgraded towers attach to the base's central deck. The nine plots and grassy battle approach stay clear. Open grass near the village gives way to trees, hills and rocks farther out. The start screen and multiplayer entry show that same empty starting countryside, with matching asset scales and lighting; returning to the menu clears match buildings and units. Terrain and camera changes remain cosmetic: city capacity, slot identities, movement, targeting and spending follow the authority.


`mise run prepare` checks tools, builds C# and imports resources. `mise run build` compiles only. `mise run check` verifies formatting. Direct Godot startup uses a role after the engine argument separator:

```sh
godot --headless --path src/Game -- --server --port 7001
godot --path src/Game -- --client --host 127.0.0.1 --port 7001
godot --path src/Game
godot --path src/Game -- --playing-host --bind 127.0.0.1 --port 7001
godot --headless --path src/Game -- --solo
```

Run `prepare` first for direct commands. Headless is a display mode; explicit `--solo`, `--playing-host`, `--server` and `--client` roles bypass the menu. The dedicated export defaults to server role. Conflicting roles/malformed arguments fail clearly. The shared RPC node is `/root/Game`. Protocol v10 retains handshake attempt/match isolation and hosted permissions and carries six resources, permanent land, building investment/generations, leveled size-based armies, upkeep and campaign receipts alongside deterministic hex actions and retained death bodies. Snapshots include the seed, configuration fingerprint, board/actions, current dying bodies, reservations, admission bounds and outcome reason; rebuilding current state does not require historical effects. Older versions cannot join. Complete snapshot RPC payloads use bounded whole-message Brotli encoding; ordinary diagnostics remain JSON. Reliable channel 0 carries requests, acknowledgments and lifecycle messages; channel 1 carries changed, revision-ordered complete snapshots at up to 20 Hz during combat, with immediate paused/noncombat changes. Unchanged paused states are not repeatedly queued. Shared `AuthoritySession` validates ownership, costs, phase/turn, retries, resume and start policy for local/remote requests. Only authority advances combat at fixed 60 Hz; guests render snapshots. Steam identity comes from the native peer, while ENet retains possession-based private resume credentials. No client prediction is used.

## Verification and exports

### Choose a check

For routine edits, choose the affected slice using the [execution policy](AGENTS.md#choose-checks-by-cost-and-affected-behavior). These examples show relative cost and coverage; choose another current selector when it better matches the change.

| Task | Check | Preparation, cost and coverage |
| --- | --- | --- |
| Change numerical rules or runner logic | `mise run test` | Locked restore/build of cheap C# gameplay and runner suites; no Godot. Run frequently when applicable. |
| Diagnose affected networking or transfer | `mise run test-network --scenario redistribution` | Standalone source preparation and real headless peers; selected transfer coverage. |
| Change rendered controls or recovery | `mise run test-ui --scenario reconnect` | Standalone source preparation and owned Linux private display; selected recovery controls/rendering coverage. |
| Check existing exported presentation | `mise run test-ui --scenario exported-package` | Existing client/server exports and private-display prerequisites required; selected packed UI coverage, with no source preparation or implicit package rebuild. |
| Start/finish a substantial implementation | `mise run ci` | Full local Linux source gates, ordered client/server exports and headless/graphical package checks; reuse a successful before baseline when source/environment inputs are unchanged. |
| Edit documentation or an OpenSpec proposal only | Relevant link/content consistency; `openspec validate CHANGE-ID --strict` for changed proposals | No automatic game/export run. |

A filtered pass is partial coverage and cannot replace a required full gate. Run full CI before and after a substantial implementation task; normal CI triggers retain all required gates. Repeat a successful check only for changed relevant inputs, failure or an unresolved concern. See the [selector and preparation details below](#verification-command-details) and [coverage, timings and retained log/PNG evidence](docs/verification.md#verification-speed-implementation-evidence-2026-10-02).

[Setup](#setup) owns the locked tools and user-managed OS/private-display prerequisites. Missing prerequisites are **unexecuted**; assertion/rendering failures are **failed**. Software-rendered frames and silent audio assertions do not establish native compositor/GPU performance, physical input or listening quality. [Runtime profiles](docs/simulation-performance.md), [native package checks](docs/distribution.md) and [paired Steam acceptance](docs/verification.md#external-steam-prerequisites-and-deferred-acceptance) remain separate routes.

### Verification command details

`test` runs cheap xUnit gameplay and runner tests without Godot. `test-network` launches separate real headless ENet peers and preserves ownership, economy, readiness, battle, pause/resume, retry, transfer, observer and early-clear/defeat coverage; complete victory and boss flows run in C#. Stable ids are `authority-resume-victory`, `redistribution`, `defeat`, `failure-cases`, `solo-session` and `playing-host-lifecycle`. The new slices cover socketless solo and original playing-host lifetime. Independent cases run with two workers by default; `--jobs 1` runs the same assertions serially. `--scenario NAME` runs only that case and reports selected coverage. `--port` pins the authority case, so it is rejected with other selected cases. Failure checks include unavailable/stopped servers, occupied ports, automatic bind retry, readiness deadlines and child exit; unrelated owners are preserved.

`test-ui` runs five source slices with at most two workers, each on its own Xvfb/Xauthority/Openbox display: `economy` checks picking, six-resource costs, equipment recruitment, upkeep, land expansion, Market trades and sale/rebuild input, `reconnect` checks the actual recovery control, and `settings` checks modal input blocking plus one persisted volume change. `launcher` checks menu, offline feedback, settings/music continuity, solo, return/fresh session and actual Exit at 1100×820 and 1280×720. `combat` checks mixed rigged units, contact, attack/effect/death poses, pause and fresh-session cleanup. Each has fresh peers/data and can run alone with `--scenario`; no earlier slice or full match is required. Rendering uses X11, Mesa software OpenGL, at most 30 FPS/two Mesa threads and silent Dummy audio. Physics remains at 60 Hz. Screenshots follow completed rendering and accompany authoritative assertions. The C# scenario harness shares ownership, waits, input/capture helpers and cleanup between local and CI checks; recurring verification needs no pasted Python or external temporary SceneTree probes.

The runner logs each process separately under ignored `logs/` and prints failed conditions with process output. Startup readiness has a 15-second deadline. Network suites and selected economy/packed/default-combat UI slices default to 300 seconds; other selected UI slices, including the research and melee checkpoints, default to 180 seconds. Full source UI defaults to 600 seconds and source/full CI to 900 seconds, bounding all required C# campaigns and material-funded graphical setup. Override with `--startup-timeout-ms` and `--timeout-ms` on network/UI tasks; dev also accepts the startup deadline. Explicit deadlines are preserved. See [verification evidence](docs/verification.md) for measured costs. Network tests default to a dynamically selected loopback UDP port; pass `--port` for a specific endpoint. An occupied explicit port fails without killing its owner. Graphical clients show connecting, connected, connection-failed and server-disconnected status.

Standalone network/source UI tasks prepare safely. `ci` restores the solution, checks formatting and compiles/imports once, overlaps cheap C# partitions, bounded network cases and source UI under a shared expensive-scenario budget (default `--jobs 2`, graphical cap `--ui-jobs 2`), and requires all source UI slices **before** Linux client/server exports. Exports remain ordered. Headless exported-role smoke and the minimal private-display exported-package slice gate final success. A failed source gate prevents both exports. GitHub Actions provisions graphics prerequisites and runs source checks, Linux packaging and native Windows packaging in parallel on separate runners; all three jobs must pass. The workflow runs only for pull requests targeting `main` and pushes to `main`, with read-only repository permissions. Outputs stay in the workspace: **no uploads, releases, publishing or deployment**.

```sh
mise run prepare-templates
mise run export-client
mise run export-client --target windows-x64
mise run export-server
mise run test-ui --scenario exported-package
```

Matching .NET export templates are downloaded explicitly, checksum-verified and installed in Godot's user template directory. This is engine data, separate from mise tools and NuGet dependencies. Repeating preparation is safe. Linux exports go to ignored `dist/client` and `dist/server`; Windows desktop exports go to `dist/windows-client`. Individual export tasks are available for local iteration; local `mise run ci` applies the test gate before invoking them. Linux can cross-export Windows for inventory inspection; native launch verification requires Windows x86_64.

On native Windows, `mise run ci-windows --startup-timeout-ms 60000` performs locked restore, formatting, build/import, offline native Steam source/export probes, package/runtime inventory checks and a bounded standalone solo launch. It uses isolated APPDATA/LOCALAPPDATA and removes SDK discovery from exported processes. The ordinary `Verify and build` workflow runs `ci-source`, `ci-linux-package` and native Windows checks in parallel on separate runners at the same commit. All three jobs must pass for workflow success. Local `mise run ci` retains its complete Linux checks with one source preparation. This ordinary workflow has no uploads or publishing.

The separate `Build published release` workflow responds only when the owner publishes an existing GitHub release. It checks the exact SemVer tag and public repository, then builds native Linux/Windows packages in parallel without rerunning the test suites. Its final job alone checks package identities/checksums and attaches the installer, archive, install script, checksum manifest and public build metadata. Release runs never publish logs or player state and never overwrite an existing version. See [distribution documentation](docs/distribution.md) for friend-facing installation, manual updates, supported Linux systems and the release procedure.

The selected `exported-package` slice uses existing exports without source preparation or implicit rebuilding, and fails clearly if either executable is missing. It checks packed resources, one real UI purchase, launcher/solo transitions and rendered checkpoints. Every verification scenario owns temporary XDG preferences/cache, sessions, ports and explicit engine logs; restart reuses its client's owned state. Evidence remains in ignored `logs/<run-id>/`, with phase/scenario JSON timings, renderer information, logs and PNGs. Owned runtime/display state is removed on success, failure, timeout or interruption. Diagnostic selected runs are partial coverage, not full-suite passes.

During development, run applicable cheap `test` checks frequently and choose the affected network/UI slice when it adds useful evidence. Run full `ci` before and after a substantial feature/change, reusing an unchanged successful baseline; an edit or checklist item is not a new full-suite boundary. Do not repeat passed checks on unchanged inputs. Documentation-only edits need documentation/plan consistency checks. CI still requires every gate on its normal triggers.

Add expensive tests only for a meaningful regression/risk that cheaper or existing checks miss. Document that gap and expected runtime/setup/maintenance cost alongside the scenario. Prefer a small independent vertical slice or an extension to an existing case. A simple option does not automatically warrant E2E coverage; avoid feature/option matrices and graphical duplication of full headless match flows. The initial slices protect actual picking/control routing, visible reconnect recovery, modal/persistence boundaries and packed-resource loading.

The cheap suites cover gameplay/presentation/transport and runner xUnit cases; see the [final economy integration gate](docs/verification.md#final-full-gate-passed) for recorded counts and coverage. Native Windows source/export validation has passed on GitHub-hosted `windows-2025`; actual release-installer verification is available as an explicitly selected package-qualification task. Timing observations are single runs rather than portable benchmarks; see [POC verification](docs/verification.md) for evidence and limitations. Real Steam peer/invitation/relay acceptance remains separate. [AGENTS.md](AGENTS.md) carries the execution/admission policy for coding agents.

## Reconnect and local sessions

Resume works against the **same running server**. Disconnect clears readiness but retains your board, resources, army, damage and original wave allocation. Your city keeps producing and fighting; teammates can pause before you return. Disconnected and eliminated players do not block readiness. With nobody connected and alive, the building phase waits. Combat continues unless paused.

For independent windows/processes use separate files:

```sh
mise run server --port 7001
mise run client --port 7001 --session-file .sessions/player-a.json
mise run client --port 7001 --session-file .sessions/player-b.json
```

After a client exits, relaunch its exact client command to recover the city. A disconnected open window offers Reconnect. Start with all players present; fresh joins after start are refused. If the server was replaced, the credential is expired and the client explains it. Use Join lobby with a fresh session deliberately to join the new lobby. There is no server save/restart recovery or host migration. Return to menu and host again for a fresh hosted match; restart a dedicated server for another match.

The runner assigns different temporary paths to each `dev` run's A/B windows and uses isolated temporary paths for network tests. The direct game accepts `--session-file`; its default is endpoint-scoped under Godot's user data directory, so two direct clients must specify different files. Private session files contain a credential and next command sequence, are written atomically, and must not be committed. Credentials never appear in shared snapshots or event logs. A sequence is reserved before sending; retries preserve identity, rejected identities cannot be repurposed, and new actions reserve new identities. ENet uses possession-based resume credentials. Steam credentials also require the SDK-authenticated account, original host, application and lobby/match namespace.

See [asset provenance and mapping](src/Game/Assets/KayKit/README.md). Assets are available from a clean checkout; runtime never downloads them. All six resources are gameplay balances; bundled props and explicit labels distinguish their producers.


## Steam development and acceptance

`mise run play` opens Single player, Multiplayer, Settings and Exit Game.
Multiplayer creates a private Steam lobby for four players including its original
host. **Invite friends** opens Odot's Steam friends picker. Choose a friend's
Invite button to send a lobby invitation directly, then have them accept it in
Steam. Names and presence come from Steam; Refresh reloads the list. "Invitation
sent" means Steam accepted the send request; a friend appears in the roster only
after joining. Watch and Remote Play are separate Steam features.

Both players need compatible Odot builds, separate signed-in Steam accounts, and
**Odot already running** when testing AppID 480. Single player needs no socket or
Steam login. Explicit ENet/headless roles skip SDK initialization. The picker
works without Steam's overlay. Linux desktop launch commands still preload the
native overlay renderer when available for other Steam UI; imports/builds,
local `dev` roles and private-display checks are unaffected. See
[Valve's Linux FAQ](https://partner.steamgames.com/doc/store/application/platforms/linux#4)
for overlay support when launching outside Steam.

The pinned official [GodotSteam GDExtension](src/Game/addons/godotsteam/README.odot.md)
uses its native `SteamMultiplayerPeer` through small C# helpers and Godot RPCs.
Desktop exports include Linux and Windows x86_64 Steam libraries. Development defaults to **AppID 480**;
`ODOT_STEAM_APP_ID` overrides it. Production export requires an explicit own
non-480 AppID. `ODOT_STEAM_DISABLED=1 mise run play` exercises offline feedback.

```sh
mise run check-steam-extension --offline
mise run check-steam-extension
mise run export-client
mise run check-steam-extension --exported
mise run export-client --production --steam-app-id YOUR_GAME_APP_ID
# Run on separate machines/accounts after both are signed in:
mise run test-steam --role host --exported --timeout-ms 600000
mise run test-steam --role guest --lobby PUBLIC_LOBBY_ID --exported --timeout-ms 600000
```

The host command prints the public lobby ID; alternatively join using Invite
friends in Odot. Pairing tests ordinary shared actions, readiness,
combat, channel acknowledgments and a paused checkpoint. Compare both records'
match/revision/tick and native connection diagnostics; a direct route cannot
count as relay proof. Missing prerequisites are unexecuted, never passed.
To verify the direct picker invitation rather than joining by an ID, run this
on two Linux machines/accounts (guest starts first, then host selects the agreed
friend in Odot). Omit `--exported` to use source clients:

```sh
mise run test-steam --role guest --scenario direct-invite --exported --timeout-ms 600000
mise run test-steam --role host --scenario direct-invite --exported --timeout-ms 600000
```

This selected case requires Steam's accepted send result on the host and actual
warm invitation acceptance callback on the guest before ordinary authenticated
admission and gameplay assertions. The runner never selects or invites a friend
automatically. Real two-account direct invitation acceptance remains unexecuted
until those records are captured; offline UI fixtures and API probes do not prove
delivery. Earlier archived Steam acceptance limitations remain documented.
The publisher labels this extension **unstable** despite its non-prerelease tag;
that release qualification remains pending. See [verification](docs/verification.md)
for prerequisites, own-AppID cold launch, packaging, coverage and limitations.


Animated combat has a selectable owned-display check: `mise run test-ui --scenario
combat`. It recruits a small mixed army through actual controls, checks moving
bones, paused poses, melee/shoot/hit/death and fresh-session cleanup through
ordinary paid battles. Full source CI includes this fifth slice; packed smoke also exercises
short combat and rig/weapon bindings. Reconnect tests restore paused current
state without historical effects. See [combat rules](docs/gameplay.md) and
[verification](docs/verification.md) for scope and measured evidence.

The early hex-melee proof is selected with `mise run test-ui --scenario combat
--checkpoint melee`. See [combat presentation verification](docs/verification.md#clarified-combat-presentation-verification)
for its paid setup, bounded normal-speed progression and paused captures, linked
near/far cues, settled-anchor clearance and intentional transit overlap.
Authority application arguments accept `--combat-seed <unsigned-64-bit>` for
repeatable diagnostics; guests cannot select the seed. Without it, a match
generates its seed once. No seed editor is added to the interface.


Cheap C# verification (`mise run test`) restores/builds once and runs disjoint
already-built campaign/general/tooling partitions with at most two test hosts.
The ordinary sample remains 21 twenty-wave strategies; one additional serialized
authority campaign checks boss and terminal wire state. Session flows use ordinary
accepted requests and bounded direct ticks, not wall-clock sleeps or resource
grants. `mise run test-in-process` retains the same discovered tests without host
sockets. Seed/configuration and first failing wave/tick identify flow failures.

Verification accepts `--simulation-speed 1..8` (default 4 for owned headless/setup authority). Each callback executes consecutive ordinary fixed ticks; gameplay rules and interactive launches keep their normal speed. Graphical animation witnesses explicitly acknowledge speed 1. Use `mise run test-network --scenario redistribution --simulation-speed 1` for pacing diagnosis, `mise run test-ui --jobs 1` for serial graphical coverage, or `mise run ci --jobs 1 --ui-jobs 1` for serial expensive admission. Selected graphical slices always own one worker.

Routine transcripts contain compact revision/tick/phase results, buffered writes, a recent full-state ring capped at 16 snapshots/16 MiB, and engine diagnostics capped at 8 MiB per child. Failure, timeout, cancellation and cleanup retain redacted state checkpoints; truncation is explicit. `--trace` retains full protocol output for diagnosis with greater disk cost. Summaries report evidence bytes; child checkpoint reports include ticks, pacing and transcript bytes. The focused packed/installed graphical route checks menu, solo, one purchase, archive rig/clip bindings, a live animation, both supported sizes and Exit. Source slices own detailed combat and application transitions; headless package role checks remain required.

Runtime profiles are separate from verification speed. Select a workload explicitly:

```sh
mise run profile-campaign --strategy frontline --players 4 --seed 1 --iterations 3 --configuration Release
mise run test-scale --scenario large-battle --seed 1
mise run profile-scale --scenario large-battle --sizes 128,512,2048 --seed 1 --iterations 3 --configuration Release
mise run profile-snapshots --scenario ordinary-and-large --iterations 3 --configuration Release
mise run profile-presentation --scenario combat-playback --frames 600 --iterations 3 --configuration Debug
```

See the [interaction scaling review](docs/simulation-performance.md) and
[recorded performance evidence](docs/verification.md) for measurement scopes,
baselines, efficiency counts and the separate verification-speed comparison.

Add `--work-counters` for a separate counting run. Profiles use serial owned
executions with a separate warm-up and retain raw evidence in ignored `logs/`.
Engine-free profiles default to Release; presentation uses the existing locked
Debug Godot build and an owned software-rendered display. `test-scale` runs the
synthetic 2,048-actor, 600-tick correctness window once, without warm-up. It is
mandatory for this optimization's before/after acceptance and is separate from
routine `test`/CI and graphical coverage. See [verification guidance](docs/verification.md)
for phase boundaries, counter meanings, scope and comparison limitations.

Campaign checks retain compact complete summaries and expand their latest 64
domain records on failure. Set `ODOT_CAMPAIGN_TRACE=1` for detailed successful
transactions; leave it unset for matched performance measurements.
