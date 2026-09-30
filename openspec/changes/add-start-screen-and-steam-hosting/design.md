# Design

## Context

See `proposal.md` for motivation and the six delta specs for behavior. The project uses stock Godot .NET 4.7.2, .NET 10, GL Compatibility, and Linux x86_64 exports. `Game.Core.Match` is engine-independent and advances at 60 steps per second. Commands, snapshots, revision checks, and `CommandLedger` already implement the cooperative rules and retry protection.

`Main.Setup()` chooses mutually exclusive server/client roles. Only the server owns `Match`; only a graphical client creates `Tabletop`. The ENet setup has two reliable channels, RPCs assume authority peer 1, `Hello` currently rejects sender IDs <= 1, and `SendAction` requires a persisted remote session. Those assumptions prevent a playing host and socket-free solo play. The exported reconnect workaround removes/re-adds the RPC node to reset Godot's path cache; preserve or replace it only with equivalent exported-lifecycle proof.

`Tabletop` receives a concrete `Main`, reads snapshot/player/status fields, and calls ordinary actions. Its current settings and music belong to the tabletop lifetime. `add-background-music-and-settings` is still active and supplies their authoritative behavior, including the recently changed first-launch volume of 50. Consume its finished implementation before moving presentation ownership; do not duplicate its specs, overwrite its edits, or assume it has completed because source files exist.

`DevRunner` already launches separate processes, drives stdin commands and real mouse events, records structured public events, checks bounded state predicates, isolates resume files, and cleans up owned children. CI covers formatting, 17 existing core cases, ENet lifecycle/redistribution/outcomes, Linux exports, and exported-role smoke. It currently has no repeatable graphical suite or Steam integration.

## Goals / Non-Goals

**Goals:**
- Keep one numerical simulation and one authority command pipeline across solo, playing-host, and dedicated-server roles.
- Use native Godot controls, input/focus, scenes, audio, preferences, RPCs, and ENet wherever they fit; use Steam's own lobby, invitation, identity, and relay facilities for online play.
- Depend on stable maintained releases and make exported C# compatibility an early measured gate.
- Keep offline testing representative of the playing-host topology and distinguish it from real Steam acceptance.

**Non-Goals:**
- Engine forks or custom engine builds, prerelease/beta dependencies, adopting abandoned adapters, or rewriting the network reliability/NAT/relay stack.
- A general transport/plugin framework, custom friends browser, public matchmaking, prediction/rollback, Steam Cloud saves, host migration, persistent match restoration, or cross-store networking.
- Adding display/audio options beyond the existing settings change or changing gameplay balance and assets.

## Decisions

### 1. Select a stable integration through a bounded compatibility proof

Prefer the current stable **GodotSteam native integration and maintained native multiplayer peer**, when both work with the stock .NET editor/templates. Access the limited Steam surface from C# using Godot's built-in object interop (`Engine.GetSingleton`, `ClassDB`, `Call`, and signal connections) behind a small project-owned adapter. A short GDScript bridge is acceptable only if a native operation cannot be called cleanly from C#; it contains platform glue, never game rules. This avoids depending on the beta full C# binding package or maintaining thousands of SDK bindings.

Prove these exact points before selecting that path: native extension loading on Linux, Steam initialization/callbacks and identity, lobby/overlay APIs, creation and disposal of the native peer as a Godot `MultiplayerPeer`, command and snapshot delivery with the existing two-channel semantics, and source plus exported operation on stock 4.7.2/.NET 10. A custom-engine-only peer does not pass. Stable API access without a compatible peer is insufficient to claim a drop-in RPC transport.

If that path fails, use the official **Steamworks.NET non-Unity stable release** with a thin project-owned `SteamNetworkingSockets` message adapter. The official release endpoint currently reports `2025.164.1`, published 2026-08-02, `prerelease: false`; upstream shows activity in August 2026. This is evidence of current eligibility, not proof of this game's runtime/export compatibility. Recheck maintenance and pin the exact tested stable tag, source hash, SDK/native redistribution files, and license during implementation. Do not use an unofficial package masquerading as the official distribution.

The fallback uses Valve's connection-oriented P2P sockets, authenticated remote identity, reliable message delivery, callbacks, and disposal. It carries the same versioned Hello/Welcome/Reject/Command/Ack/Snapshot/session-ended messages into the shared session dispatcher. It does not implement a new reliability protocol or a custom `MultiplayerPeer`; ENet continues to deliver those operations through native Godot RPCs. Use SDK-supported lanes if needed for equivalent command/snapshot scheduling; prove progress under snapshot traffic rather than pretending an unsupported channel exists. Select one production Steam stack, not both.

Rejected candidates: the currently open-beta GodotSteam C# bindings, Expresso's paused peer, older C# peer examples without verified maintenance, and arbitrary forks. GodotSteam's archived GitHub mirrors are not proof of abandonment: its canonical development moved to Codeberg, and maintainers announced 4.22 for Godot 4.7.2 in August and a peer-close fix in September. Check the canonical stable release and maintenance evidence rather than old tutorials.

If neither supported path passes, record the specific blocker and continue only independent local phases. Steam-dependent tasks and full-change acceptance remain unchecked; do not substitute a beta dependency or call a skipped test a pass.

Primary references, inspected 2026-09-30:
- [GodotSteam maintainer release and peer-fix announcements](https://steamcommunity.com/groups/godotsteam/announcements).
- [GodotSteam C# bindings explicitly marked open beta](https://github.com/LauraWebdev/GodotSteam_CSharpBindings).
- [Expresso maintainer pause and channel limitation](https://github.com/expressobits/steam-multiplayer-peer).
- [Official Steamworks.NET standalone release](https://github.com/rlabrecque/Steamworks.NET/releases/tag/2025.164.1) and [non-Unity support](https://steamworks.github.io/).
- [Godot high-level multiplayer](https://docs.godotengine.org/en/4.7/tutorials/networking/high_level_multiplayer.html) and [cross-language scripting](https://docs.godotengine.org/en/4.7/tutorials/scripting/cross_language_scripting.html).
- [Valve P2P sockets](https://partner.steamgames.com/doc/api/ISteamNetworkingSockets), [lobbies/invitations](https://partner.steamgames.com/doc/features/multiplayer/matchmaking), and [Steam initialization](https://partner.steamgames.com/doc/sdk/api#SteamAPI_Init).

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

Publish only version, original host SteamID, session identifier, and lobby/in-progress state as discovery metadata. Never publish credentials, commands, armies, or changing snapshots in lobby data. The admitted roster comes from authoritative gameplay handshake; membership alone does not grant city ownership. Validate peer identity from the SDK's authenticated connection and retain it with the issued resume credential. A different Steam account cannot reclaim that credential.

Keep the original host identity immutable even if Steam transfers lobby ownership. At start, stop new-city admission in the authority and advertise in-progress status; do not blindly set lobby joinability false because that can also block legitimate roster reentry. Valid reconnects use the persisted original host endpoint/credential and can rejoin the private lobby or connect directly to the known original host. Remove refused fresh entrants from the join flow without allocating city state. Reject mismatched versions and wrong-host messages before accepting state.

Use `CreateListenSocketP2P`/`ConnectP2P` through the selected maintained integration, initialize relay access, and collect public connection-route diagnostics for the real-network check. Keep secrets out of route/session logs. A real Steam invitation proves discovery; relay route evidence proves transport. These are separate assertions.

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

The real Steam gate requires actual invitations at runtime and cold launch, authenticated admission, ordinary purchases/readiness/combat, paused guest reconnect, late fresh-join rejection, and host exit. At least one run uses different NAT networks without router changes and proves an actual relay route using supported connection diagnostics or configuration. A mock lobby or localhost ENet result cannot satisfy it. Missing accounts/AppID/machines leave that phase incomplete; normal CI still runs.

## Risks / Trade-offs

- Native peer/API compatibility is not established -> Phase 1 proves the exact stock-engine/export combination; use the specified stable standalone SDK fallback if necessary, with no beta dependencies or engine fork.
- SDK glue can grow into a second protocol -> Preserve one message/authority dispatcher, retain native ENet RPC delivery, and limit Steam glue to SDK lifecycle and delivery. Bump the shared protocol version only when observable message/handshake compatibility changes, with explicit old-version refusal.
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
| 1. Integration prerequisites and compatibility | Accepted settings baseline; eligible stable SDK/peer selected by source/export proof; AppID/account prerequisites recorded |
| 2. Shared authority/session boundary | Solo/host/guest commands share validation, ledgers, and snapshots; original core and dedicated ENet regressions pass |
| 3. Application menu and solo play | Four-button native menu, shared settings/music, offline one-player match, repeatable leave/exit |
| 4. Playing host and local development | Graphical/headless host plus real guests over ENet, host-only start, repeatable local developer launch and lifecycle checks |
| 5. Steam discovery and gameplay | Private lobby/native invites, warm/cold invitation routing, authenticated relay transport and resume, host-end behavior |
| 6. Automated regression and graphical E2E | Bounded core/process/UI scenarios, isolated storage, clean source/export local checks, meaningful failure diagnostics |
| 7. Real Steam/export acceptance and docs | Exported clients on separate accounts/networks prove invites, relay route, shared gameplay/reconnect/host exit; limitations recorded |

Existing explicit CLI server/client usage remains valid; normal desktop default changes to the menu. Existing ENet resume files remain accepted for compatible running matches; Steam uses its own namespace. No match save migration is required. Rollback can remove the launcher/Steam adapter and return the bootstrap to explicit client behavior while preserving the existing numerical core and settings work. Dependency/setting changes are committed only with the corresponding implementation, not during this planning step.

Implementation needs an AppID with suitable test access (or a clearly separated documented development AppID), two distinct Steam accounts, and two test machines. These are external prerequisites to supply during Phase 1/7, not reasons to replace online acceptance with a local test. Production AppID configuration is separate from development test values; do not ship a development `steam_appid.txt` or hardcode the public sample AppID as the released game's identity.
