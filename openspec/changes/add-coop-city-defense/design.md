# Design

## Context

See `proposal.md` for motivation and the capability specs for the behavior contract. The completed scaffold already provides one Godot .NET project, plain C# rules in `src/Game.Core`, xUnit tests, separate ENet processes, JSON snapshots/events, a C# process runner, and gated Linux exports. `Main.cs` currently combines a `Node2D` renderer, movement RPCs, fixed-step server updates, session setup, and stdin-driven automated input. Its disconnect callback deletes the player; the transport peer ID is also the world identity. Those assumptions must change together.

`NetworkTests.cs`, `Child.cs`, and the export smoke checks consume `WorldSnapshot` through `GameEvent`. They must move with the new contract rather than keeping a second coin protocol. The runner already coordinates through events/deadlines and launches two graphical clients; preserve that infrastructure. The previous change is complete but unarchived and the main spec inventory is empty. Do not rewrite its historical artifacts. This change replaces the demo-specific behavior; if the previous change is later archived, its contradictory session/gameplay/test requirements need reconciliation before both sets become durable contracts.

## Goals / Non-Goals

**Goals:** Keep game state small and fully server-owned; make resumption independent of scene lifetime and transport identity; make freeze/resume exact; use the current engine, transport, task interface, and verified assets without adding a framework.

**Non-Goals:** Server disk persistence, accounts, encrypted/authenticated production transport, host migration, prediction/rollback, navigation or physics-driven combat, generic entity architecture, skeletal character animation, paid asset tiers, or balancing a finished commercial game. Resume is a possession-based credential for a POC on the existing local/LAN transport, not an account system.

## Decisions

### 1. Small match state and explicit phases

Replace coin rules with a `Match`-style core model (concrete names can follow existing conventions). Keep it numerical and engine-independent. It owns:

```text
Match
  match ID, phase, paused, revision, simulation tick
  wave index, building turn 1..3, unique turn serial 1..9, original roster
  Players
    stable player ID, connected, ready, eliminated
    gold, food, city health, slots[9] with type/level
    soldiers with ID, health, lane position, cooldown
  Enemies
    ID, origin player ID, destination player ID
    health, lane position, cooldown
```

Use `Lobby -> Building -> Combat -> Building ... -> Victory/Defeat`, with pause as an orthogonal flag. An authenticated lobby member can start with 1-4 connected players; lock their roster and initialize the first turn atomically. One running server owns one match until restarted; a rematch menu is deferred. New clients after start are refused, while existing members can resume as active players or eliminated observers. An explicit start avoids late connection timing changing enemy budgets. Supporting dynamic late joins would require additional progression/balance decisions and is outside this POC.

After the final required ready action, resolve the turn once: produce for every living city, clear readiness, then advance the building turn or spawn the wave after turn three. A player cannot spend while ready; unready restores editing. Disconnected players are excluded from the check but retain their city production. No connected living player means no building resolution. Combat continues without connections unless explicitly paused. Any connected roster member, including an eliminated observer, can pause/resume; there is no voting or exclusive pause owner.

### 2. Tune numbers in one rules definition

The confirmed gameplay is in the specs. The following are proposed starting balance values, not additional user decisions or a promise of final balance:

| Rule | Initial POC value |
| --- | --- |
| Starting city health / gold / food | 100 / 60 / 0 |
| Base gold per production turn | 10 |
| Construction: mine / farm / barracks | 20 gold each |
| Upgrade to level two | 20 gold per building; maximum level two |
| Mine output at levels one / two | 3 / 6 gold per turn |
| Farm output at levels one / two | 5 / 10 food per turn |
| Recruit one soldier at barracks levels one / two | 5 / 4 food |
| Soldier health / attack / interval | 10 / 4 / 1 second |
| Enemy health / attack / interval | 10 / 3 / 1 second |
| Built-in defender attack / interval | 1 / 1 second |
| Enemy allocations per original player, waves 1 / 2 / 3 | 4 / 6 / 8 |

Keep these in one immutable core rules definition used by the server and tests; send the relevant costs/output values with initialization for UI labels. Ordinary play uses the standard definition. Focused core tests can use a small explicit rule fixture; real network acceptance runs use normal requests and standard gameplay values rather than privileged state-changing RPCs.

Recruitment is immediate and manual: one accepted click is one new full-health soldier, constrained by available food and an owned barracks. A barracks upgrade discounts future recruits; it does not introduce another troop type or retroactively modify soldiers. Gold income arrives at turn resolution, so the starting gold allows meaningful first-turn purchases. There is no demolition, selling, healing, wall purchase, or barracks automation in this milestone. Surviving soldiers and city health do not heal automatically. This preserves the confirmed persistent army while keeping content to three building types and one soldier type.

### 3. Numerical battle lanes with deterministic attack ordering

Use one short one-dimensional approach lane per city and the existing 60 Hz server fixed-step path. Store movement as lane distances; clients map those distances into 3D. Soldiers advance toward the nearest assigned enemy; enemies advance toward the nearest soldier or, if none remain, the city. Start soldiers near the city and enemies at the lane entrance. Use bounded movement, a fixed melee distance, and integer-tick attack cooldowns. The built-in defender targets an assigned living enemy throughout combat, preferring the one closest to the city and then entity ID. Represent shots visually; damage is applied by the core, not projectile physics.

Evaluate movement and attacks in stable city/entity order. Determine attacks from the step's living actors, accumulate damage, then apply it together so iteration order does not grant one city a timing advantage. Remove dead actors, identify every zero-health city, stop their defenders, remove any remaining soldiers attached to those cities, and redistribute surviving enemies before the next simulation step. Defeat takes precedence when no cities remain, including simultaneous last-enemy/last-city deaths. At a nonfinal wave clear, return to building turn one; after wave three clear, enter victory.

For transferred enemies preserve ID, origin, health, and remaining attack cooldown, change destination, and place them at the receiving lane's entrance immediately. This entrance placement is a simple presentation/simulation default rather than physical travel between cities. Transfer to a lane that cleared earlier is valid: the shared wave remains active until all enemies are gone. At the next wave, reset surviving soldiers' lane positions/cooldowns for deployment but retain their health and IDs. This avoids navigation and cross-board physics synchronization while still showing individual units fighting.

For current attackers, sort transferred enemy IDs and assign round-robin across stable living player IDs. For future waves, spawn each living player's own baseline, then form one pool containing exactly one baseline allocation per eliminated original roster member and distribute that pool round-robin. This balances remainder counts and prevents recursively multiplying inherited future budgets. Simultaneous eliminated cities are all excluded before distribution. Disconnected living cities count as survivors and keep receiving enemies.

### 4. Stable sessions separate from transport peers

Keep ENet and the shared RPC node at `/root/Game`. The server adapter owns a small peer-to-player map and a private player-to-resume-credential mapping, separate from the public core snapshot. The server issues a cryptographically random credential through a reliable private welcome message, creates a stable sequential player ID, and publishes only that ID. On resume, validate the token and bind a new peer to the same player; never infer ownership from a claimed player ID. Refuse credentials already bound to an active peer. Clear the old mapping on disconnect and check that a disconnect callback still belongs to the active binding before changing connectivity.

Persist client credentials and the next command sequence in a small local session file scoped by endpoint and match ID. Write it atomically. Provide an explicit session-file option in the game and runner so two local clients do not share a token; `dev` assigns distinct paths and automated suites use isolated temporary directories. A restarted client can reuse that path; an in-window Reconnect control can reuse the in-memory/file session. Credential-bearing welcome messages are not `GameEvent` diagnostics, and neither file contents nor tokens are printed. There is no server database: a new server cannot restore the previous match and returns a clear expired/invalid-session response, with a deliberate fresh-join action available only in a lobby.

Full reconnect restoration is required even after client process restart; automatic reconnect retry/backoff is unnecessary for this POC. The user invokes reconnect or relaunches with the same session file. This adds session continuity without an account provider or host migration.

### 5. Reliable commands and independent snapshot revisions

Bump the protocol version and replace `Move` with reliable commands for start, build, upgrade, recruit, ready/unready, pause, and resume. Commands carry match ID, expected phase and unique turn serial (where relevant), a monotonic client sequence, and bounded typed parameters. The serial advances across all nine building turns; do not use only the repeating 1..3 display counter to reject stale actions. Ownership comes from the authenticated sender. Validate before mutation and acknowledge success/failure; cap message size and per-peer work so malformed or flooding clients cannot monopolize the server. A rejected economic action leaves gameplay unchanged.

Retain a processed-sequence high-water mark and a bounded recent-result cache per stable player on the server. Persist/reserve the next client sequence before sending. Reusing a processed sequence cannot execute again: return its cached result if available or an explicit already-processed result plus current state. Gaps are allowed, making client crash recovery straightforward. Rejected requests also consume their identity; a corrected user action gets a new sequence. Delayed commands for an old match/turn fail even when not previously processed. No silent replay of old ready state or spending on reconnect is allowed. Test resending the same economic command over a resumed connection.

Send private initial/resume snapshots reliably; continue sending small complete periodic snapshots reliably on the existing separate ordered snapshot channel and immediately after important transitions. Increment `revision` on public changes including connectivity, ready, pause/resume, and gameplay, independently of the simulation tick. Clients compare match ID and revision, ignore stale updates, and enable input only after initialization/resumption. The server remains the sole source of resources, damage, destinations, and outcomes. Full snapshots are simpler than partial entity replication for four nine-slot cities and a few dozen enemies.

Pausing rejects building/recruitment/readiness requests and stops all simulation counters and timers. The server continues polling ENet and sending connection/pause changes. Resume restores the fixed-step simulation without accumulating time spent paused. The renderer snaps to frozen authoritative positions and stops attack animations while paused; it also stops extrapolating when disconnected. A simulation-tick-only freshness check would lose pause/reconnect updates, hence the separate revision.

### 6. Thin bootstrap and client-only tabletop

Refactor the common root to a plain `Node` attached to `Main.cs`, retaining its name/path and identical RPC contracts on both roles. Keep network/bootstrap responsibilities there or in a small adjacent session helper. Put the world view under a client-only `Node3D` scene with an orthographic angled `Camera3D`, flat square boards, short lanes, simple lighting, and selected models. Add a `CanvasLayer`/Control interface for roster, costs/resources, health, selected-slot actions, turn/wave, readiness, pause, reconnect, and outcome. Separate the snapshot-driven view from authority; headless roles never instantiate the camera, models, or UI.

Arrange up to four boards compactly and allow roster selection to focus/inspect a city. Only owned slots expose build/upgrade/recruit actions. Ray picking or explicit slot controls use stable slot indices 0-8, not model/mesh IDs. A selected barracks offers a clearly labeled Recruit button: clicking the building selects it; the explicit action spends food. Simple offsets can distinguish multiple units sharing lane positions. No free camera controller, unit commands, skeletal animations, or folding terrain are needed. Keep the Compatibility renderer and existing engine/tool versions.

### 7. Selected free assets, with provenance and fallbacks

Use a small copied subset of models plus all referenced materials/textures/buffers under `src/Game/Assets/KayKit/`, rather than vendoring every format and source file or adding an editor plugin. Prefer the pack's Godot-ready glTF/GLB files, wrap scale/orientation in project scenes, and keep logical square terrain separate from decorative hex meshes. Buildings: use the free mine and barracks models; represent a farm with a free windmill/agricultural prop or a labeled medieval substitute. A house or tower can represent the city/built-in defender outside the slots. Prototype Bits supplies differently colored unit/defender markers and missing props; Resource Bits supplies gold material/ore props. Food can use a labeled Prototype Bits marker.

The official Medieval pack lists character units as extra-tier content; the creator's Resource Bits announcement places food/money props in its extra tier. Therefore no promised character or food model depends on paid downloads. Do not use unrelated packs or a third-party mirror containing paid tiers. Verify the selected downloaded free archive's actual contents/license before committing the subset. Record official URL, pack/version, archive checksum or official repository commit, and the selected file mapping in an asset manifest with the included license texts. Once vendored, development, CI, and the running game need no asset download service. Asset acquisition happens during implementation, not this planning workflow.

Sources checked during proposal preparation:

- [Medieval Hexagon official repository and free/extra contents](https://github.com/KayKit-Game-Assets/KayKit-Medieval-Hexagon-Pack-1.0).
- [Prototype Bits official repository and CC0 license](https://github.com/KayKit-Game-Assets/KayKit-Prototype-Bits-1.0).
- [Resource Bits official free download tier](https://kaylousberg.itch.io/resource-bits/purchase).
- [Creator's Resource Bits description and free/extra distinction](https://www.patreon.com/kaylousberg/posts/resource-bits-124790362?l=fr-FR).

### 8. Verify the ordinary protocol, including interruption

Replace coin-specific tests and snapshots in the core tests and `DevRunner`, preserving existing startup/failure checks and `Child` lifecycle management. Add core tests for economics, three-turn cadence, damaged persistent troops, defender attrition, pause timers, balanced redistribution/conservation, simultaneous city deaths, fixed original wave budgets, and terminal outcomes. Core fixtures can directly construct edge cases; their tests should assert outcomes and invariants rather than private implementation structure.

Drive real network scenarios through ordinary stdin-to-client-to-RPC commands: start, purchase, upgrade, recruit, ready, pause, resume, disconnect, and reconnect. The resume scenario kills/stops client B, reuses B's isolated session path in a replacement process, verifies changed peer identity with retained player identity, compares frozen gameplay fields while allowing connectivity/revision changes, replays a prior recruitment sequence, and resumes combat. A three-client scenario leaves one city under-defended while the other two recruit normally, then observes actual elimination, immediate transfer, and the next wave's redistributed count. Include a standard-rules complete three-wave victory run and defeat coverage. No test-only network message writes resources or health.

Keep all waits condition-based and bounded. The existing 60-second whole-suite budget may be too short for several real-time battles; use a documented 180-second default for the expanded network task while retaining the existing timeout override and 15-second startup limit. The implementation should tune balance within the stated structure if the ordinary economic strategy cannot reliably exercise the required paths. Do not speed up only the test server or replace network assertions with core-only results.

Update exported-role smoke checks to start a solo match, buy a farm/barracks, advance turns, and observe production/recruitment through the exported protocol. A headless smoke check cannot prove model loading, so separately inspect a graphical source and exported client. Record the actual manual/graphical observations and platform limitations in `docs/verification.md`. Run the existing CI pipeline after the new tests; preserve its sequencing and build-only outputs.

## Risks / Trade-offs

- [The new game can accidentally inherit coin-specific assumptions] -> Replace the full rules/wire/test path together, keep tooling stable, bump protocol, and explicitly record the old spec conflict.
- [A reconnect or delayed command spends resources twice] -> Stable identity, reserved command sequences, retained high-water marks, turn guards, and real retry/reconnect verification.
- [An apparently frozen game still applies damage or ignores new snapshots] -> Simulation-based cooldowns, no wall-clock catch-up, an independent revision, and pause comparisons that exclude connectivity-only changes.
- [Elimination chains lose or multiply enemies] -> Exclude simultaneous deaths first, retain enemy IDs/health, pool original future allocations once, and test conservation and integer remainders.
- [Any member can resume a teammate's pause] -> This is the simple POC policy; show who paused/resumed. Voting or moderation can be a later requirement.
- [A disconnected city loses while the match runs] -> Keep its actual state, allow teammate pause, show connection status, and resume into the current result rather than reviving it.
- [Free packs lack a matching farm, food item, or animated soldier] -> Use the explicitly requested placeholder pack and clear labels; preserve pack provenance and avoid paid dependencies.
- [The larger presentation breaks stripped server exports] -> Load visuals only for graphical clients and smoke-test exported headless gameplay as well as the graphical client.
- [Initial balance makes acceptance scenarios slow or unwinnable] -> Keep all values together, prove a standard-rules three-wave strategy, and tune numbers without expanding mechanics.

## Migration Plan

Implement on the existing completed scaffold. Add the new core model and meaningful rules checks first, then switch the network/bootstrap and runner contract, then add the client tabletop/assets, and finish real-process and graphical/export verification. Until the wire switch is complete, intermediate code may not pass the old coin integration suite; do not claim the milestone complete before the new suite and CI pass.

There is no saved server state to migrate. Existing running clients are incompatible with the new protocol and must restart against the updated server. Preserve old source in version control rather than keeping a second demo mode. Rollback restores the prior coin rules/scenes/contracts/tests together and removes the new selected assets/session UI. Do not alter the previous change's task history or perform deployment, spec archiving, or main-spec synchronization as part of implementation.

Implementation decision: full multi-city snapshots exceed ENet's unreliable MTU even early in recruitment. Periodic full snapshots use reliable channel one, with revision checks retained, avoiding fragmented unreliable delivery and warning spam in this small POC. Command/private welcome traffic remains on channel zero.

Balance verification: enemy damage is 3 per second. At the proposed value of 2, the weak defender could almost clear wave one alone and the under-defended transfer scenario could not exercise elimination during that wave. The farm-upgrade/barracks strategy remains the standard acceptance strategy.

Client lifecycle compatibility: the installed Godot export templates leave a bound RPC path-cache tree-exit connection behind when a live RPC node changes transport. Before replacing a client transport, briefly remove/re-add the bootstrap node so its actual tree-exit untracks that cache, then restore its client callbacks and current-scene reference. Keep the tabletop as a client-only sibling so controls and frozen state survive. The node returns to `/root/Game`; no engine patch or protocol change is needed. Verify repeated in-window reconnects and server-loss feedback in exported clients, which can expose engine-cache issues absent from headless/source checks.
