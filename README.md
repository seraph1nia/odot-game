# The Common Watch

Basic buildings use small wood costs; gold remains for land and advanced buildings. The top-right resource table shows exact Stock and Income/production, with a compact Upkeep table underneath. Food stays at its original scale. See [gameplay defaults](docs/gameplay.md).


**Build and provision your village. Hold together against automatic waves.**

The Common Watch is a one-to-four-player cooperative settlement-planning and city-defense POC in C# and Godot. Each player starts with five of nine indexed hex plots, manages six resources, equips and feeds persistent individual soldiers in homogeneous squads, and readies up with teammates for automatic battles on one connected shared arena. Survive twenty waves together: fallen cities pass enemy pressure to survivors, and one surviving city secures victory for everyone. Solo uses the same rules with a local authority; multiplayer uses the original playing host or an explicit dedicated server. Guests render authoritative snapshots and can resume retained cities against the same running authority.

The name describes a watch kept for everyone's homes: separate villages, shared defense. The woodland countryside and tabletop presentation support that premise; this is not competitive conquest or direct shooter combat.

### Stable technical identity

The Common Watch was previously named Odot. Technical names remain unchanged for compatibility: `Odot.slnx`, namespaces, `ODOT_*` variables, repository/release URLs, `odot` executables and package names. Godot's internal project name remains `Odot - Nine Tiles` so existing preferences and private credentials keep their original user-data path; the graphical window and menus use The Common Watch. No data migration, protocol change, version bump or separate release is implied. See [desktop distribution](docs/distribution.md) for installation display names and legacy launch aliases.

## License and contributions

Odot's original code and documentation are licensed under the [GNU General Public License v3.0 only](LICENSE) (`GPL-3.0-only`). Copyright (c) 2026 Odot contributors. You may use, modify and redistribute them under that license; distributed derivatives must comply with its source-sharing requirements.

Development is maintainer-led, and external contributions and pull requests are not accepted. Write access, including branch creation in this repository, is limited to authorized maintainers. You may fork the project and develop your own branches under the applicable licenses.

Third-party materials retain their own terms and are excluded from Odot's GPL grant. See [authored 3D provenance](docs/assets.md), the [historical KayKit notices](src/Game/Assets/KayKit/README.md), [GodotSteam and Valve runtime notices](src/Game/addons/godotsteam/README.odot.md), [woodland UI/font notices](src/Game/UI/README.md), [Worldtree notices](src/Game/Assets/Authored/NOTICE.md#living-worldtree-ui), and [music provenance](src/Game/Assets/Music/README.md). The supplied music has no documented redistribution license; its inclusion does not grant permission to reuse or redistribute it.

## Setup

Combat math runs entirely in C# under `src/Game.Core`: one canonical action per
squad, individual member health/status/experience, shared integer graph search and complete physical defense profiles. `mise run test`
verifies transitions, targeting, simultaneous impacts and lifecycle behavior
without starting Godot. See
[gameplay rules and compatibility](docs/gameplay.md#deterministic-hex-combat) and
[shared arena, squads and compatibility](docs/shared-arena.md), and
[verification evidence](docs/verification.md).

Install [mise](https://mise.jdx.dev/), Git, and the OS prerequisites for [.NET](https://learn.microsoft.com/dotnet/core/install/) and [Godot](https://docs.godotengine.org/en/stable/about/system_requirements.html). Desktop clients default to native Forward+ / Vulkan on supported GPUs/drivers; Compatibility / OpenGL 3.3 remains the fallback. A display is required for play. Default tests and PR/main CI are headless. Graphical qualification is explicitly selected and gated, not a prerequisite for routine development. See [renderer, atmosphere and graphics settings](docs/atmosphere.md) for switching, restart-later preferences and the separate optional native Vulkan smoke. [Forest art direction](docs/forest-art.md) documents Dreaming/Enchanted/Natural themes and Off/Subtle/Full magical accents; these local settings are independent of weather and quality.

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

Use targeted `test-core`/`test-runner` during implementation, broader `test-fast` once coherent, and `test-engine`/`test-network` only for relevant engine/transport changes. Formatting is `mise run check`. `mise run ci` is now headless source plus sequential Linux exports/package checks, not a required before/after development tax. See [verification](docs/verification.md) and the [testing command skill](.agents/skills/testing-commands/SKILL.md).

`Odot.slnx` is the repository solution. `src/Game/Game.sln` is the small engine-local solution required by Godot's C# editor/exporter; it includes the game and its core dependency, while tests and tooling stay outside the Godot import root. The explicit game target framework prevents the editor from inserting its default framework during import.

The game keeps separate Debug and ExportRelease NuGet locks because Godot includes its editor assembly only in development builds. The game and core declare the `linux-x64` and `win-x64` export runtimes in advance so publishing does not rewrite their locks. Deliberate dependency updates require `dotnet restore --force-evaluate -p:RestoreLockedMode=false`; regenerate the game export lock with `-p:Configuration=ExportRelease` as well, then run `ci` and review the resulting lock changes.

## Gameplay

Read [the rules and tested campaign strategies](docs/gameplay.md). Six resources fund the city: gold, wood, food, stone, metal and cloth. Recruitment spends equipment materials; food is paid once before each battle, with unfed veterans kept in reserve. Recruitment buildings reach level five, producers and defenses level two, and Markets level one. Purchase locked plots for gold, sell buildings for half their actual investment, or build a Market to sell resource bundles. Prepare together across three stages, each END settling production, with automatic combat immediately after the third—no fourth spending window. Select the global combat speed and inspect the authoritative upcoming formation before battle. See [preparation settlement, combat speed and next-wave inspection](docs/gameplay.md#preparation-and-battle-controls) for timing and controls. Unit levels, wounds, city damage and research persist. Fallen Keeps redirect objective preference without teleporting living attackers, retain their share of future enemy pressure, and leave surviving owner squads fighting for the current battle. Clear all twenty waves, including bosses at ten and twenty, to win. See [wave power and paid encounter evidence](docs/wave-power.md) for the current enemy-strength rule and its balance limitations.

Each city earns one personal research point per shared wave clear. Research Towers add +1/+2 per full three-production cycle, retaining partial progress; multiple towers add together. Open **Worldtree** without selecting a plot to inspect the native living tree and buy foundations, exclusive specializations and masteries, plus village production offshoots. See [Worldtree controls and details](docs/worldtree.md#reachable-game-feature) for the HUD point balance, hover/keyboard purchase route and narrower-window behavior. Points and choices reset each match; sale preserves them. Fire gives burn, Venom poison, and Frost slows future actions. Inspect a character for its current effects; see [status inspection](docs/gameplay.md#personal-research-and-temporary-statuses). Freeze/stun are deferred; incompatible older peers are refused.

The graphical client uses owner-permitted immutable exports from `odot-game-assets` in a detailed woodland hex landscape with a lower angled view. Its live menus/settings/HUD/dialogs use the pinned woodland native UI kit, original seal and licensed serif font; the former Trio UI pack is removed. See [UI migration and source bindings](docs/ui-migration.md). [Asset mappings](docs/assets.md) document honest cosmetic substitutes (Bakery, Woodcutter hut, Knight and Archer) while canonical rule/save/protocol identities remain unchanged. Click a plot or visible building, then use the bottom panel to Build, Upgrade or Recruit. Settlement scenery is decorative and the nine plot identities remain stable; combat uses the connected squad arena described in [shared arena combat](docs/shared-arena.md). Roster tabs inspect any city; only your own can be edited. Ready locks editing until Unready. Any connected member, including an eliminated observer, can pause/resume the entire match. Costs and unavailable-action explanations come from authoritative state.

## Development commands

```sh
mise run dev
mise run dev --guests 3
mise run play
mise run server --bind 127.0.0.1 --port 7001
mise run client --host 127.0.0.1 --port 7001 --session-file .sessions/player-a.json
mise run test-fast
mise run test-core --filter 'FullyQualifiedName~HexRoutingTests'
mise run test-runner
mise run test-engine --scenario settings # settings + native UI resources/modal/input/services
mise run test-network --scenario playing-host-lifecycle
# Explicit headless deep qualification, not the default development loop:
mise run test-campaign
mise run test-network --scenario redistribution
mise run test-network --scenario eliminated-resume # controlled stored-spectator case; scope in docs/shared-arena.md
mise run test-network --scenario active-fallen # controlled active-field ENet resume/impact; NOT balance/full-session acceptance
mise run test-deep
mise run test-network --jobs 1
mise run ci-source
mise run ci-linux-package
# Graphical checks require separate authorization; do not run under headless-only tasks.
```

`dev` builds/imports once and opens two playable desktop windows: one playing host and one guest, using ENet without Steam. `--guests 1..3` changes the guest count; the host owns the first city. Wait for the roster, then click **Start match** in the host window. Each process has isolated temporary preferences and credentials. Closing a window or Ctrl+C stops only supervised peers. `play` opens the normal start screen. Individual roles prepare themselves and keep endpoint arguments. Default bind/host is loopback and default port is 7000. Bind a LAN interface explicitly for ENet play on another machine, allowing its UDP port.

In the tabletop, scroll up/down over the world to zoom in/out toward the ground point under the cursor (overview to 3×). Hold **WASD or arrow keys** to pan around the observed settlement or complete shared arena. Click the world to release HUD keyboard focus when using arrows. The angle stays fixed and travel is bounded. **Space** restores the complete overview. Left-click and drag the world to pan; a short click selects a plot or unit. Switching settlements, entering/leaving the arena or starting a fresh match resets the view; changing the observed city while remaining in the shared arena does not. Resizing and reconnecting to the same match/view retain relative adjustments. Camera state is local and is not saved across launches. The top-right table shows the observed city’s six resource stocks and income, with compact upkeep underneath; use the small city arrows and Details for cooperative inspection and army allocation. Click a living unit for its model preview, name, description, level, health and damage; click elsewhere to dismiss it. Characters and damageable Keeps/Watchposts have small text-free lifebars; purchased invulnerable towers have neither floating text nor lifebars. Counts, levels and statuses remain in panels and inspection; see the [health-overlay contract](openspec/specs/game-feedback/spec.md#requirement-authoritative-overhead-unit-health-bars). Return to menu is in Settings and asks for confirmation. You can inspect while paused or disconnected. HUD scrolling, dialogs, text entry and focused UI navigation take priority; window focus loss or a dialog stops held movement until you release and press again.

Click a numbered **army plot** and use its **Buy plot** quote, **Squads**, the individual inspector and **Town hall roster** to manage your army. The native squad panel provides paid reinforcement/new squads, member transfers, storage/return and retirement; **Deployment** opens owned arena-home picking. An empty owned army plot can hold a defense tower instead of a squad; click a base arrow tower to choose **Rapid arrow tower** or **Bombard tower**. See [shared arena controls](docs/shared-arena.md#native-presentation) and [army plots, retirement, storage and recovery](docs/gameplay.md#purchased-army-homes-and-town-hall-rotation) for controls, capacity, costs and production cadence.

The village uses a tighter, content-fitted overview and compact authored woodland surrounded by opaque grey cloud banks. Camera navigation stays within that envelope; see [battlefield framing](docs/view-renderer-worldtree.md#battlefield-framing) for the presentation boundary. [Detailed moss, grass, dirt, leaf-litter and pine-duff floors](docs/detailed-ground.md) replace the old live meadow; [smaller interleaved woodland patches, a meandering surface stream and winding clearing trails](docs/rich-background.md) use the actual imported suite with normal depth testing. Buildings, the home and defender align to supporting hex footprints; resource groups and props sit on their local terrain, and upgraded towers attach to the base's central deck. The nine plots and grassy battle approach stay clear. A timber settlement-edge walk joins the authored river bridge and grounded bank steps, then meets a winding dirt trail to a wayside shrine; another trail leads from the bank lantern into a mushroom clearing. Varied shared-mesh tree groups, loose ferns, leaf clumps, moss, rocks and mushroom pockets fill the wooded flanks while leaving routes, clearings and plot sightlines open. Plot and building positions/scales stay fixed—this composition does not increase building-center spacing or alter the battle area. The start screen and multiplayer entry use the same empty starting landscape assets, scales and lighting, with their own full-background coverage rather than the battlefield cloud boundary; returning to the menu clears match buildings and units. Terrain and camera changes remain cosmetic: city capacity, slot identities, movement, targeting and spending follow the authority.


`mise run prepare` checks tools, builds C# and imports resources. `mise run build` compiles only. `mise run check` verifies formatting. Direct Godot startup uses a role after the engine argument separator:

```sh
godot --headless --path src/Game -- --server --port 7001
godot --path src/Game -- --client --host 127.0.0.1 --port 7001
godot --path src/Game
godot --path src/Game -- --playing-host --bind 127.0.0.1 --port 7001
godot --headless --path src/Game -- --solo
```

Run `prepare` first for direct commands. Headless is a display mode; explicit `--solo`, `--playing-host`, `--server` and `--client` roles bypass the menu. The dedicated export defaults to server role. Conflicting roles/malformed arguments fail clearly. The shared RPC node is `/root/Game`. See [session ownership and compatibility](docs/sessions.md) for the current protocol, roster command guards and synchronized army/recovery state. Snapshots include the seed, configuration fingerprint, board/actions, current dying bodies, squad reservations, physical structures, authoritative next-wave forecast and outcome reason; rebuilding current state does not require historical effects. Older versions cannot join. Complete snapshot RPC payloads use bounded whole-message Brotli encoding; ordinary diagnostics remain JSON. Reliable channel 0 carries requests, acknowledgments and lifecycle messages; channel 1 carries changed, revision-ordered complete snapshots at up to 20 Hz during combat, with immediate paused/noncombat changes. Unchanged paused states are not repeatedly queued. Shared `AuthoritySession` validates ownership, costs, phase/turn, retries, resume and start policy for local/remote requests. Only authority advances combat; guests render snapshots. [Battle-flow integration](docs/battle-flow.md#behavior-and-integration-boundaries) owns fixed simulated ticks and wall-time admission. Root ENet guests service their original native API through one owner-thread fixed-physics driver instead of render-coupled automatic admission; authorities, solo/playing hosts, Steam and other APIs retain original service. No intermediate snapshots/events are coalesced or dropped. See [service ownership and measured proof](docs/enet-guest-cadence.md). Steam identity comes from the native peer, while ENet retains possession-based private resume credentials. No client prediction is used.

## Verification and exports

### Choose a check

Choose the smallest tier that can catch the changed behavior. The repository's
[testing strategy](.agents/skills/testing-strategy/SKILL.md) owns that decision;
[testing commands](.agents/skills/testing-commands/SKILL.md) owns exact selectors,
filter syntax, preparation and cleanup. A filtered pass is partial coverage.

| Change | Suggested check |
| --- | --- |
| Numerical rules / subsystem | `mise run test-core --filter 'FullyQualifiedName~Research'` |
| Runner ownership / cancellation | `mise run test-runner` |
| Coherent implementation | `mise run test-fast`, formatting |
| Godot controls / resources / animation | `mise run test-engine --scenario settings`; [component selectors](.agents/skills/testing-commands/SKILL.md#focused-headless-engine-and-bounded-transport) |
| Real transport / session lifetime | `mise run test-network --scenario playing-host-lifecycle` |
| Export/package boundary | `mise run ci-linux-package` / native Windows `ci-windows` |
| Explicit campaign qualification | `mise run test-campaign` |
| Documentation only | Link/content/planning consistency; no automatic engine/export run |

[Setup](#setup) owns locked tools and user-managed prerequisites. Missing
prerequisites are **unexecuted**; assertions are **failed**. Runtime profiles,
installer/real Steam qualification and rendered acceptance remain separate.
See [current verification and measurements](docs/verification.md).

### Verification command details

For exact default selections, filters, deadlines, serial debugging and explicitly
authorized graphical checkpoints, use the [testing command reference](.agents/skills/testing-commands/SKILL.md).
Do not run simultaneous tasks that mutate shared build/import/export outputs.
[Current verification](docs/verification.md#ci-coverage) owns CI job coverage,
artifact policy and the separate weekly/manual headless deep workflow.

```sh
mise run prepare-templates
mise run export-client
mise run export-client --target windows-x64
mise run export-server
mise run ci-linux-package
```

Matching .NET export templates are downloaded explicitly, checksum-verified and installed in Godot's user template directory. This is engine data, separate from mise tools and NuGet dependencies. Repeating preparation is safe. Linux exports go to ignored `dist/client` and `dist/server`; Windows desktop exports go to `dist/windows-client`. Individual export tasks are available for local iteration; local `mise run ci` applies the test gate before invoking them. Linux can cross-export Windows for inventory inspection; native launch verification requires Windows x86_64.

On native Windows, `mise run ci-windows --startup-timeout-ms 60000` performs locked restore, formatting, build/import, offline native Steam source/export probes, package/runtime inventory checks and a bounded standalone solo launch. It uses isolated APPDATA/LOCALAPPDATA and removes SDK discovery from exported processes. Local `mise run ci` gates sequential Linux exports/package checks on headless source success with one source preparation; `ci-linux-package` alone does not run the source gates. See [CI coverage](docs/verification.md#ci-coverage) for hosted job selection and uploaded evidence.

The separate `Build published release` workflow distributes packages only when the owner publishes an existing GitHub release; release-automation/package PRs also exercise its same build/transfer/assembly path with ephemeral local tags and no release publication. It checks the exact SemVer tag and public repository, then builds native Linux/Windows packages in parallel without rerunning the test suites. Its final job alone checks package identities/checksums and attaches the installer, archive, install script, checksum manifest and public build metadata. Release runs never publish logs or player state and never overwrite an existing version. See [distribution documentation](docs/distribution.md) for friend-facing installation, manual updates, supported Linux systems and the release procedure.

For optional `test-atmosphere` / `test-vulkan`, existing-client `--exported` checks, authorized windowed native execution and focused Settings checkpoints, see [renderer verification](docs/atmosphere.md#verification-and-visual-evidence). [Qualification status](docs/atmosphere.md#qualification-status) distinguishes current selected coverage from historical full/native/export receipts.

Selected exported graphical checks use existing exports, never implicit rebuilding.
Every verification owns preferences/cache, credentials, endpoints and engine logs;
restarts reuse only their client's state, cleanup awaits every child, and unrelated
owners remain untouched. Failure evidence is bounded and redacted. See
[testing hygiene](.agents/skills/testing-hygiene/SKILL.md) before adding/removing
coverage and [flaky diagnosis](.agents/skills/diagnose-flaky-tests/SKILL.md) before
changing deadlines. Current measurements and unexecuted qualification are recorded
in [verification](docs/verification.md); historical receipts are explicitly separate.

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

See [asset provenance and mapping](docs/assets.md). Assets are available from a clean checkout; runtime never downloads them. All six resources are gameplay balances; bundled props and explicit labels distinguish their producers.


## Steam development and acceptance

`mise run play` opens Single player, Multiplayer, Settings and Exit Game.
Multiplayer creates a private Steam lobby for four players including its original
host. **Invite friends** opens The Common Watch's Steam friends picker. Choose a friend's
Invite button to send a lobby invitation directly, then have them accept it in
Steam. Names and presence come from Steam; Refresh reloads the list. "Invitation
sent" means Steam accepted the send request; a friend appears in the roster only
after joining. Watch and Remote Play are separate Steam features.

Both players need compatible The Common Watch builds, separate signed-in Steam accounts, and
**The Common Watch already running** when testing AppID 480. Single player needs no socket or
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
friends in The Common Watch. Pairing tests ordinary shared actions, readiness,
combat, channel acknowledgments and a paused checkpoint. Compare both records'
match/revision/tick and native connection diagnostics; a direct route cannot
count as relay proof. Missing prerequisites are unexecuted, never passed.
To verify the direct picker invitation rather than joining by an ID, run this
on two Linux machines/accounts (guest starts first, then host selects the agreed
friend in The Common Watch). Omit `--exported` to use source clients:

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


Animated combat is protected by pure C# timing/state tests and focused headless
`test-engine --scenario animation` against production imported rigs. Explicit
owned-display combat acceptance remains available but is not PR/main CI coverage.
Research is a separate checkpoint, not appended to combat's remaining deadline.
See [verification](docs/verification.md) for executed scope and limitations.

The early hex-melee proof is selected with `mise run test-ui --scenario combat
--checkpoint melee` with explicit graphical authorization. See [shared-arena presentation](docs/shared-arena.md#native-presentation) for current
physical windup/local-strike cues and the [testing command reference](.agents/skills/testing-commands/SKILL.md)
for selection and ownership. [Historical combat presentation receipts](docs/verification-history.md#clarified-combat-presentation-verification)
retain the earlier individual-actor setup and cues, not current squad acceptance.
Authority application arguments accept `--combat-seed <unsigned-64-bit>` for
repeatable diagnostics; guests cannot select the seed. Without it, a match
generates its seed once. No seed editor is added to the interface.


The [testing command reference](.agents/skills/testing-commands/SKILL.md) owns
fast/deep selection, the socketless fallback, concurrency and setup pacing.
Interactive launches remain at ordinary simulation speed.

Routine transcripts contain compact revision/tick/phase results, buffered writes, a recent full-state ring capped at 16 snapshots/16 MiB, and engine diagnostics capped at 8 MiB per child. Failure, timeout, cancellation and cleanup retain redacted state checkpoints; truncation is explicit. `--trace` retains full protocol output for diagnosis with greater disk cost. Summaries report evidence bytes; child checkpoint reports include ticks, pacing and transcript bytes. The default explicitly selected packed graphical smoke uses one offline application, build/recruit, return/Exit and one frame; legacy packed/installed journeys remain deep qualification. Headless package/runtime probes remain required; neither they nor silent Dummy audio establish rendered or listening quality.

Runtime profiles are separate from verification speed. Select a workload explicitly:

```sh
mise run profile-campaign --strategy frontline --players 4 --seed 1 --iterations 3 --configuration Release
mise run test-scale --scenario large-battle --seed 1
mise run profile-scale --scenario large-battle --sizes 128,512,2048 --seed 1 --iterations 3 --configuration Release
mise run profile-snapshots --scenario ordinary-and-large --iterations 3 --configuration Release
# Graphical profiles: explicitly authorized only, never under a headless-only task.
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run profile-presentation --scenario combat-playback --frames 600 --iterations 3 --configuration Debug
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run profile-presentation --scenario authored-scale --frames 600 --iterations 1 --configuration Debug
```

See the [interaction scaling review](docs/simulation-performance.md),
[authored-era performance loop](docs/authored-performance.md) and
[recorded performance evidence](docs/verification.md) for measurement scopes,
baselines, efficiency counts and the separate verification-speed comparison.

Add `--work-counters` for a separate counting run. Profiles use serial owned
executions with a separate warm-up and retain raw evidence in ignored `logs/`.
Engine-free profiles default to Release; presentation uses the existing locked
Debug Godot build and an owned software-rendered display. `test-scale` runs the
synthetic 2,048-member, 600-tick correctness window once, without warm-up. It is
explicit stress qualification, separate from routine `test`/CI and graphical coverage. See [verification guidance](docs/verification.md)
for phase boundaries, counter meanings, scope and comparison limitations.

Campaign checks retain compact complete summaries and expand their latest 64
domain records on failure. Set `ODOT_CAMPAIGN_TRACE=1` for detailed successful
transactions; leave it unset for matched performance measurements.
