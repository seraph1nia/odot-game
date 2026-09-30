# Design

## Context

See `proposal.md` for motivation and the six delta specs for behavior. The project uses stock Godot .NET 4.7.2, .NET 10, GL Compatibility, and Linux x86_64 exports. `Game.Core.Match` is engine-independent and advances at 60 steps per second. Commands, snapshots, revision checks, and `CommandLedger` already implement the cooperative rules and retry protection.

`Main.Setup()` chooses mutually exclusive server/client roles. Only the server owns `Match`; only a graphical client creates `Tabletop`. The ENet setup has two reliable channels, RPCs assume authority peer 1, `Hello` currently rejects sender IDs <= 1, and `SendAction` requires a persisted remote session. Those assumptions prevent a playing host and socket-free solo play. The exported reconnect workaround removes/re-adds the RPC node to reset Godot's path cache; preserve or replace it only with equivalent exported-lifecycle proof.

`Tabletop` receives a concrete `Main`, reads snapshot/player/status fields, and calls ordinary actions. At initial inspection its settings and music belonged to the tabletop lifetime. The accepted `add-background-music-and-settings` implementation supplies their authoritative behavior, including first-launch volume of 50. Read and verify that baseline before moving presentation ownership; do not duplicate its specs or overwrite unrelated edits.

`DevRunner` already launches separate processes, drives stdin commands and real mouse events, records structured public events, checks bounded state predicates, isolates resume files, and cleans up owned children. CI covers formatting, 17 existing core cases, ENet lifecycle/redistribution/outcomes, Linux exports, and exported-role smoke. It currently has no repeatable graphical suite or Steam integration.

## Goals / Non-Goals

**Goals:**
- Keep one numerical simulation and one authority command pipeline across solo, playing-host, and dedicated-server roles.
- Use native Godot controls, input/focus, scenes, audio, preferences, RPCs, and ENet wherever they fit; use Steam's own lobby, invitation, identity, and relay facilities for online play.
- Pin the maintained upstream non-prerelease GodotSteam extension, retain the assessed development stability qualification, and make exported C# compatibility an early measured gate.
- Keep offline testing representative of the playing-host topology and distinguish it from real Steam acceptance.

**Non-Goals:**
- Engine forks or custom engine builds, prerelease/beta dependencies, adopting abandoned adapters, or rewriting the network reliability/NAT/relay stack.
- A general transport/plugin framework, custom friends browser, public matchmaking, prediction/rollback, Steam Cloud saves, host migration, persistent match restoration, or cross-store networking.
- Adding display/audio options beyond the existing settings change or changing gameplay balance and assets.

## Decisions

### 1. Commit to GodotSteam GDExtension and its native peer

Use the official **GodotSteam GDExtension with its native `SteamMultiplayerPeer`**, loaded by the stock .NET editor and ordinary Godot .NET export templates. Access the limited Steam surface from C# using built-in object interop (`Engine.GetSingleton`, `ClassDB.Instantiate`, `Call`, and signal connections). Small helpers own initialization, callbacks, lobby flow, peer construction, and cleanup. A short GDScript bridge is acceptable only where a native operation cannot be called cleanly from C#. The native peer supplies the Steam-to-Godot transport, and existing Godot RPCs carry the same handshake, commands, acknowledgments, snapshots, and session-ended messages. Do not write a custom `MultiplayerPeer`, message transport, SDK binding library, or automatic Steamworks.NET fallback.

The assessed package is **`v4.22.1-gde`**, published 2026-09-04, using Steamworks SDK 1.65. The official release API reports `prerelease: false`, its descriptor specifies Godot 4.4 minimum, and the Asset Library lists Linux x86_64 support. Canonical source on Codeberg is not archived and showed activity on 2026-09-25. The tagged registration source registers `Steam`, `SteamPacketPeer`, and `SteamMultiplayerPeer` under the GDExtension build; peer source uses SteamNetworkingSockets P2P and packet source configures lanes. This establishes availability and intended channel support, not observed remote delivery.

On 2026-09-30 a temporary C# project using stock Godot .NET 4.7.2/.NET 10 passed these source and isolated Linux release-export checks: load the Steam singleton/native peer, initialize AppID 480, call SDK methods, connect/disconnect a C# lobby signal, instantiate the peer as `Godot.MultiplayerPeerExtension`/`MultiplayerPeer`, set channel 1, create a native host with peer ID 1, assign it to `Multiplayer.MultiplayerPeer`, close/dispose it, and shut down the SDK. The exported native files loaded away from the source tree and the process exited 0. The downloaded `godotsteam-4.22.1-gdextension-plugin-4.4.zip` had SHA-256 `2b12b3499434c50da16104a0d22b725aee15cc5cd41223c1cea825bae59bfa8f`. These are preliminary compatibility observations; implementation must reproduce them with pinned files and retain durable sanitized logs.

Two qualifications remain recorded: the publisher's Asset Store listing marks this version unstable despite the non-prerelease upstream tag, and one initial headless-editor import exited 139 although later imports and export passed. The user accepted committing to this extension for development after reviewing those findings. This is not a claim that the warning or crash is resolved. Phase 1 repeats editor/shutdown checks and records the version's stability assessment; release acceptance requires observed lifecycle and multiplayer checks. Do not introduce beta C# bindings or abandoned adapters to work around failures. If the native extension cannot satisfy a gate, record the blocker and leave the affected tasks incomplete; changing the stack requires a new explicit decision.

Default Steam development runs and development exports to **AppID 480** via the platform helper's `steamInitEx(480, false)` or equivalent documented extension settings. This is a development choice, not an upstream implicit default. Keep automatic initialization disabled so local/ENet/headless tests need no Steam client, and poll callbacks through the application owner when Steam is initialized. Allow an explicit AppID override for actual game testing. Release packaging must require the game's configured AppID and exclude development AppID files/settings.

AppID 480 permits initial integration without owning a production AppID. It does not establish Steam launching this game's executable: Steam resolves an AppID to its installed application. Test `+connect_lobby` parsing with explicit development launches, but record that as routing coverage, not a real cold-launch invitation pass. Full Steam cold-launch acceptance requires this game's own registered launch configuration.

Primary references, inspected 2026-09-30:
- [Official GDExtension release](https://codeberg.org/godotsteam/godotsteam/releases/tag/v4.22.1-gde) and [release metadata](https://codeberg.org/api/v1/repos/godotsteam/godotsteam/releases/tags/v4.22.1-gde).
- [Godot Asset Library compatibility/distribution entry](https://godotengine.org/asset-library/asset/2445) and [publisher Asset Store usage/stability warning](https://store.godotengine.org/asset/godotsteam/godotsteam-gdextension/).
- [Tagged native class registration](https://codeberg.org/godotsteam/godotsteam/src/tag/v4.22.1-gde/register_types.cpp), [peer implementation](https://codeberg.org/godotsteam/godotsteam/src/tag/v4.22.1-gde/godotsteam_multiplayer_peer.cpp), and [packet/lane implementation](https://codeberg.org/godotsteam/godotsteam/src/tag/v4.22.1-gde/steam_packet_peer.cpp).
- [Godot ClassDB](https://docs.godotengine.org/en/4.7/classes/class_classdb.html), [high-level multiplayer](https://docs.godotengine.org/en/4.7/tutorials/networking/high_level_multiplayer.html), and [C# calls/signals](https://docs.godotengine.org/en/4.7/tutorials/scripting/cross_language_scripting.html).
- [Valve development AppID example and launch behavior](https://partner.steamgames.com/doc/sdk/api), [P2P sockets](https://partner.steamgames.com/doc/api/ISteamNetworkingSockets), and [lobbies/invitations](https://partner.steamgames.com/doc/features/multiplayer/matchmaking).

### 2. Separate authority, session delivery, and application lifetime

Extract a small engine-independent authority/session helper around the existing `Match` and ledgers, preferably in `Game.Core`. It owns players, credentials, authenticated bindings, host designation, command validation, and session disposal. Network adapters supply trusted connection identity and monotonic rate-limit time; the UI never supplies an authoritative sender. Keep snapshots and numerical stepping unchanged.

Use a narrow session-facing interface for `Tabletop`: current snapshot, local player, connection/role/status/feedback, send action, reconnect, and leave. ENet RPCs become delivery handlers calling the helper. The host's local player has an explicit local binding and ledger; it does not call remote `Hello` or need a loopback socket. Solo joins one local player and submits the normal start command. Hosted mode joins the local host but waits in a lobby. Dedicated mode joins no local player and retains the existing connected-member start permission.

Only the authority calls `Match.Step()` from the fixed physics tick. A playing host exposes snapshots to its own presentation and broadcasts them to guests. Guest presentations render accepted snapshots, never simulate economy or damage. All request paths use identical ownership/cost/phase/turn and retry validation and acknowledgments; host-only hosted-start permission belongs in the shared session policy.

```text
Persistent graphical application
  +-- Native start screen / settings / one music player
  +-- Current session
        +-- Local authority (solo or host) --> Match + ledgers
        +-- Remote delivery (ENet or Steam) --> Same command dispatcher
        +-- Tabletop --> Snapshot and validated actions

Dedicated server
  +-- ENet delivery --> Same authority --> Match + ledgers
```

A Godot autoload or root-owned persistent graphical node is suitable for shared presentation. Keep headless creation explicitly gated; do not use an unconditional autoload that instantiates music/settings on servers. This small boundary is enough; a service container or interchangeable engine layer is unnecessary.

### 3. Use native screens and reuse bundled medieval scenery

Normal graphical startup creates the persistent presentation and four-button menu without a session. Use `Control`, `PanelContainer`, native containers/buttons, shared theme, and Godot focus navigation. Reuse a static decorative patch from the vendored palette or a bundled existing capture; instantiate no numerical match or four-city backdrop. Do not generate new artwork, introduce animation/shaders, or run background combat.

Single player immediately enters the first building turn. Multiplayer opens Host game/Back and explains invitation joining; successful Host opens a private lobby with roster, Invite friends, Start, and Return to menu. Show errors inline without requiring restart. Keep transport names, SDK versions, and development selectors out of the player flow. Explicit development CLI roles bypass the menu intentionally.

Move `ClientSettings` and the single existing `AudioStreamPlayer` to graphical application lifetime, preserving the final settings implementation and saved file. Share the same menu from the start screen and in-game entry point. Existing input blocking/Esc/dropdown behavior remains, and focus restoration must target the active screen rather than a freed tabletop button. Opening settings or the invite overlay does not automatically pause multiplayer. Add Return to menu in lobby/game/outcome/disconnected screens and preserve bottom-panel composition at both existing verification sizes.

### 4. Keep Steam discovery separate from gameplay admission

Use `ISteamMatchmaking.CreateLobby` with private visibility and capacity four and `ISteamFriends.ActivateGameOverlayInviteDialog` for invitations. Poll SDK callbacks from the application thread through one platform owner. Steam is optional: local/ENet/headless paths skip platform initialization; normal Steam-launched graphical applications can initialize/poll it to receive invitations, and Host initializes it if necessary. Failed initialization remains recoverable and leaves solo/settings usable. Disable plugin auto-initialization where it would impose Steam on local tests.

Route both `GameLobbyJoinRequested_t` and `+connect_lobby` launch arguments into one join operation; use `ISteamApps.GetLaunchCommandLine` where needed. Before switching away from an active solo/host/guest session, use a native confirmation dialog identifying the consequence; declined invitations do nothing. Deduplicate the pending lobby target and validate its application/protocol/host metadata.

Publish only the game's identifier, protocol/version, original host SteamID, session identifier, and lobby/in-progress state as discovery metadata. Validate the game identifier before connecting because AppID 480 is shared by unrelated development projects. Never publish credentials, commands, armies, or changing snapshots in lobby data. The admitted roster comes from authoritative gameplay handshake; membership alone does not grant city ownership. Obtain the authenticated remote Steam identity from the native peer's `get_steam_id_for_peer_id` and retain it with the issued resume credential. A different Steam account cannot reclaim that credential.

Keep the original host identity immutable even if Steam transfers lobby ownership. At start, stop new-city admission in the authority and advertise in-progress status; do not blindly set lobby joinability false because that can also block legitimate roster reentry. Valid reconnects use the persisted original host endpoint/credential and can rejoin the private lobby or connect directly to the known original host. Remove refused fresh entrants from the join flow without allocating city state. Reject mismatched versions and wrong-host messages before accepting state.

Use the extension's `create_host`/`create_client` native-peer methods with a fixed virtual port and assign the peer to Godot's multiplayer API only after host/client creation succeeds. These methods use SteamNetworkingSockets P2P internally. Initialize relay access through the extension and collect supported public connection-route diagnostics for the real-network check. Configure enough native channels for the existing reliable channel 0 requests/acks and channel 1 snapshots, and verify their delivery under load. Keep secrets out of route/session logs. A real Steam invitation proves discovery; relay route evidence proves transport. These are separate assertions.

### 5. Make session transitions explicit and bounded

Use one active-session generation/epoch to reject callbacks, packets, and pending commands from a disposed session. Entering a new session clears the visible snapshot/selection and pending requests, not persisted preferences. Session disposal unsubscribes callbacks/signals, closes peers/sockets, leaves lobby membership, removes the tabletop, and resets the Godot multiplayer peer to offline in the proper order. Preserve the exported RPC-node reset behavior until verified alternatives exist.

Guest Return to menu marks the guest disconnected on the running authority and retains private resume data. Orderly host Return to menu/exit sends a session-ended notice where possible, closes admission/transport, and discards authority. Guests show an explanation and return to the menu. Unexpected connection loss freezes guest input and exposes bounded reconnect/return controls; clients never elect a replacement host. Temporary guest failures can reconnect to the running original host.

Namespace persisted endpoints by transport and app: ENet address/port versus Steam AppID/original host SteamID/lobby reference, with the authoritative MatchId retained separately. Validate MatchId during resume; old credentials cannot silently join a new match at the same host. Keep command sequences with their session and original request identity. No migration of a match to a new server is implemented.

Exit Game and native close share cleanup and ultimately `SceneTree.Quit()`. Attempt settings save, but do not keep a process alive forever waiting for platform callbacks or network flushing. No external gameplay server is spawned for solo or hosted play. Source startup errors from an explicit headless role continue producing useful nonzero exits.

### 6. Extend existing verification instead of creating a second harness

Retain `mise run test`, `test-network`, `dev`, `server`, `client`, and `ci`. Add an explicit playing-host launch role and transport option; keep existing `--host` as an ENet endpoint argument rather than overloading it as a role. `dev` starts one graphical playing host plus one graphical guest by default, with a bounded option for up to three guests. Give each process isolated resume and settings directories using supported Godot user-storage facilities. Headless playing-host tests select the same session role without presentation.

Reuse `Child.WaitFor`, structured events, temporary storage, and owned-process disposal. Add missing public menu/session/role/ended events, not secret credentials. Read-only diagnostic queries can identify named visible controls and geometry to drive input through `Input.ParseInputEvent`; they must not activate button callbacks directly, select hidden controls, or mutate match state. No resource grants, forced deaths, or test-only gameplay RPCs.

Coverage has four layers: engine-independent rules/authority tests; real playing-host plus guest process scenarios on ENet; source/exported graphical E2E with a display; real Steam E2E with two accounts/machines. Add `test-ui` and `test-steam` tasks using the same runner/support. Core/local/export smoke remain normal CI gates with no Steam login. Graphical E2E is a gate where a configured native/virtual display exists and an explicit task elsewhere; absence is reported as unexecuted, never passed. Real Steam checks use paired exported processes with logs from both machines, manual overlay clicks where the external Steam UI requires them, and machine/account prerequisites supplied externally.

Use bounded state predicates and compare snapshots at a common revision or paused tick; do not demand simultaneous latest snapshots from moving combat. Target dropped acknowledgment/retry and delayed stale-message behavior at the shared message boundary with a small test-only delivery interceptor if needed. Its scope is dropping/delaying delivery, not replacing authority or adding a fault framework. Existing real disconnect/restart and invalid-message tests remain primary network evidence.

The real Steam gate requires actual invitations at runtime and cold launch, authenticated admission, ordinary purchases/readiness/combat, paused guest reconnect, late fresh-join rejection, and host exit. AppID 480 is the default for real development sessions between accounts; a production AppID is not a prerequisite to start that work. Authentic Steam cold launch of this executable requires its own AppID/launch configuration; explicit `+connect_lobby` test launches do not satisfy that assertion. At least one run uses different NAT networks without router changes and proves an actual relay route using supported connection diagnostics or configuration. A mock lobby or localhost ENet result cannot satisfy it. Missing accounts/machines or the game's cold-launch configuration leave the relevant tasks incomplete; normal CI still runs.

## Risks / Trade-offs

- Preliminary native host/C# export compatibility passed, but remote traffic and lifecycle stability remain unproven -> Phase 1 repeats source/editor/export checks, investigates the initial import crash, and verifies both channels between real accounts; keep failures visible without switching stacks.
- Upstream tag is non-prerelease but the publisher marks the distribution unstable -> Development selection is explicit; retain that qualification, assess the pinned version during implementation, and require observed lifecycle and multiplayer acceptance before release claims.
- Platform helpers can grow into a second transport -> Keep native ENet and Steam peers delivering the same Godot RPCs; limit helpers to platform lifecycle, lobby flow, identity lookup, and native peer setup. Bump the shared protocol version only when observable message/handshake compatibility changes, with explicit old-version refusal.
- Two clients may share local preference storage -> Isolate all automated graphical processes and restore no real user settings during tests.
- Concurrent settings work changes the same presentation files -> Finish and read its accepted implementation first; this change moves lifetime but does not redefine volume, resolution, or loop behavior.
- Steam callbacks may outlive their lobby/session -> Use generation checks and symmetrical subscriptions/disposal, exercising repeated enter/leave and pending-operation exit.
- A stalled or crashed host cannot preserve its match -> Explain the fixed host lifetime and offer return/reconnect without host election or save claims.
- Steam testing depends on external accounts and network access -> Keep it separate from ordinary CI and explicitly record missing prerequisites and failed assertions.
- Native export packaging can work in-editor but fail on a clean machine -> Pin native files/hashes/licenses and run exports away from the source tree before acceptance.

## Migration Plan

All work belongs to this one change. Each phase's checks must pass before dependent work; Phases 2-4 may proceed if external Steam prerequisites delay Phase 1's real connection proof, but Phases 5 and 7 remain gated by the supported integration.

| Phase | Deliverable and gate |
|---|---|
| 1. Integration prerequisites and compatibility | Accepted settings baseline; pinned GodotSteam GDExtension/native peer; reproduced source/export checks and real channel proof; AppID 480/account prerequisites recorded |
| 2. Shared authority/session boundary | Solo/host/guest commands share validation, ledgers, and snapshots; original core and dedicated ENet regressions pass |
| 3. Application menu and solo play | Four-button native menu, shared settings/music, offline one-player match, repeatable leave/exit |
| 4. Playing host and local development | Graphical/headless host plus real guests over ENet, host-only start, repeatable local developer launch and lifecycle checks |
| 5. Steam discovery and gameplay | Private lobby/native invites, warm/cold invitation routing, authenticated relay transport and resume, host-end behavior |
| 6. Automated regression and graphical E2E | Bounded core/process/UI scenarios, isolated storage, clean source/export local checks, meaningful failure diagnostics |
| 7. Real Steam/export acceptance and docs | Exported clients on separate accounts/networks prove invites, relay route, shared gameplay/reconnect/host exit; limitations recorded |

Existing explicit CLI server/client usage remains valid; normal desktop default changes to the menu. Existing ENet resume files remain accepted for compatible running matches; Steam uses its own namespace. No match save migration is required. Rollback can remove the launcher/Steam helpers and return the bootstrap to explicit client behavior while preserving the existing numerical core and settings work. Dependency/setting changes are committed only with the corresponding implementation, not during this planning step.

Steam development defaults to AppID 480 and needs two distinct Steam accounts and two test machines for remote acceptance. This game's own configured AppID is needed for genuine Steam cold-launch and production verification, not for initial development. These external prerequisites do not justify replacing online acceptance with a local test. Development exports may use explicit 480 configuration or a documented development-only `steam_appid.txt`; production packages must exclude that file and must not fall back to 480 when their own AppID is missing.
