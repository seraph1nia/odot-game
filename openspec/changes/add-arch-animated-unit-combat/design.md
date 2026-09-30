# Design

## Context

See [proposal.md](proposal.md) for motivation. `src/Game.Core/World.cs` currently owns a 60 Hz simulation using mutable soldier/enemy lists and a one-dimensional position. Both sides approach a target's center even while attacking. `Tabletop.UpdateUnit` renders static `Dummy_Base.gltf`, adds an ID-derived lateral offset and pulses scale from cooldown. Consequently numerical contact and visible contact disagree.

`Main._PhysicsProcess` steps only the authority and sends snapshots every three combat ticks; guests render snapshots. Shared `AuthoritySession` validation, stable player credentials, command ledgers, whole-match pause, redistribution and session-generation guards already exist. Preserve these boundaries. Core tests currently inject and mutate `Unit` lists directly and must gain deliberate ECS fixture seams. The only checked-in scene is the application root; tabletop creation is C# driven.

The existing specifications require one soldier type and placeholder characters. This change deliberately revises those requirements. The workspace also contains ongoing Linux/Windows distribution work: keep both runtime identifiers, conditional lock files and export behavior, and preserve that work when adding dependencies.

## Goals / Non-Goals

**Goals:** Make unit state authoritative and engine-independent; make contact, timing and visual action progress agree; support dozens of units; keep the existing cooperative lifecycle and verification meaningful.

**Non-Goals:** Engine migration, Steamworks.NET adoption, an ECS rewrite of UI/economy/session handling, free battlefield navigation, terrain physics, manual unit orders, rollback networking, native GPU performance claims or automatic distribution publishing. Existing GodotSteam remains the Steam adapter.

## Decisions

### 1. One Arch world per authoritative match

Pin [Arch 2.1.0](https://www.nuget.org/packages/Arch/2.1.0) in `Game.Core`. Its .NET 6/.NET Standard 2.1 targets are compatible with the declared .NET 10 target. Use the [tagged 2.1.0 source](https://github.com/genaray/Arch/tree/v2.1.0) to verify actual APIs (`Arch.Core.World`), because the package README still includes prerelease instructions. Do not add ECS scheduling/source-generation extensions or background jobs for this slice.

Introduce `Game.Core/Combat/` with plain value components for stable unit ID/type/faction, city ownership/origin/destination, health/profile, position/radius, deployment status, target, movement and attack sequence/timing. `Match` owns the world and a private stable-ID-to-entity lookup. Arch handles never go on the wire. City resources, slots, readiness and match progression remain ordinary core state; soldier membership is projected from ECS ownership, not stored in a second mutable list.

Systems execute synchronously from `Match.Step` on the existing authority thread. Query results that influence rules are sorted by stable ID before processing; simultaneous damage is accumulated separately. Structural creation/removal occurs outside active queries. Snapshot arrays are immutable projections sorted by player/unit/event ID. Implement idempotent lifetime teardown through `Match` and `AuthoritySession.End`, including exit and failed/replaced startup. Verify the pinned world destruction API releases its registry entry as well as storage. Tests dispose their matches and use an internal fixture builder, available to the test assembly, to create legitimate combat state without exposing production mutation hooks or test-only network commands.

Alternative: attach ECS components to Godot nodes. That makes headless rules depend on presentation lifetimes. A full city ECS conversion adds cost without helping unit combat.

### 2. A bounded corridor with simple deterministic contact

Retain the 12-unit approach per city. Add a 3.4-unit-wide corridor with double-precision lateral and forward coordinates, 0.20-unit body radii and seven formation columns at 0.48-unit spacing. These are initial core rules, not scenery geometry. Spawn stable-ID-ordered formation rows with melee soldiers ahead of ranged soldiers. If an entry slot has no room, keep that entity in an explicit entry queue: it counts toward its army/wave but has no body, target or attack until placed. A transfer changes destination immediately and re-enters that destination's entry queue without changing identity, health, origin or remaining recovery.

Each tick reads the same initial living-state buffer for both factions. Acquire the nearest eligible opponent in the same destination, breaking ties by ID; keep an engaged target while valid. An opponent intercepting a melee route invalidates a target behind it so the front reachable opponent can be engaged. Compute movement toward the target only until attack range, then hold. Use deterministic local tangent alternatives around friendly blockers, chosen with stable ID tie-breaking and confined to the corridor. Resolve proposed moves with swept-circle contact against occupied/reserved paths, in stable ID order; shorten or reject moves that would cross a body. A blocked unit reports zero actual motion. Reject any final placement violating bounds or separation (tolerance `1e-6`) rather than relying on repeated corrective pushes. This favors reliable small battles over a complex crowd solver; add cheap congestion/progress fixtures before presentation integration.

No Godot collision bodies, navigation server or root motion determine combat. Range is center-to-center distance in authoritative corridor coordinates. Ranged attacks can pass friendly bodies and do not require line of sight. Pure one-dimensional overlapping stacks would preserve the display defect; unrestricted steering/navigation would exceed the selected lane scope.

### 3. Typed recruitment and a staged combat tick

Use `Swordsman` as the omitted-type default and `Crossbowman` as the explicit ranged type. Initial profiles below are proposed starting values, not observed balance results. Keep economy, wave allocations and the built-in defender's low-damage cadence. Barracks upgrades reduce either type's food cost by one. Use validated rules/profile data rather than client-selected numerical stats.

| Profile | Food, level 1 / 2 | Health | Damage | Range | Speed / second | Windup ticks | Start-to-start ticks |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Swordsman | 5 / 4 | 10 | 4 | 0.55 | 1.0 | 12 | 60 |
| Crossbowman | 5 / 4 | 8 | 3 | 3.0 | 1.0 | 18 | 60 |
| Enemy | wave allocation | 10 | 3 | 0.55 | 0.8 | 12 | 60 |

The defender retains its existing city-wide targeting and immediate timed damage; emit a corresponding cosmetic shot event. Crossbow shots are authoritative single-target impacts at the end of windup, with a short cosmetic bolt tracer rather than simulated physical projectiles. Crossbowmen hold when in range and may fire while enemies reach them; automatic retreat/kiting is outside this slice.

Tick order: admit queued entries; decrement recovery; select/validate targets from the read buffer; calculate and constrain motion; start eligible attacks after final positions are known; resolve scheduled impacts after validating source/target/destination/range; accumulate defender and unit damage; apply all damage simultaneously; emit hit/death records; remove casualties; eliminate cities and redistribute; evaluate the existing wave/outcome rules. In-range attacks schedule impact at `start + windup`; recovery ends at `start + cadence`. Lost targets consume the original recovery rather than delivering a hit to a substitute. A transfer cancels former-destination windups while preserving remaining recovery. City melee impacts follow the same timing. Hit visuals impose no gameplay stun.

Retain the existing winning-economy and cooperative assertions. If spatial contact changes battle duration or balance, fix the rule/profile defect or document an intentional profile adjustment with its evidence; do not relax tests merely to obtain a pass.

### 4. Current state plus bounded event history

Bump `WireJson.ProtocolVersion` from 3 to 4 unless the concurrent work has already advanced it, in which case choose the next unused version. Change typed commands, snapshots, automation parsing and consumers together. Preserve `recruit <slot>` as a melee default and add an explicit ranged form. No mixed-version compatibility is promised.

Keep longitudinal `Position` and add explicit lateral scalars to DTOs; do not serialize engine vectors or rely on public fields of numerical vector structs. Unit snapshots include type, profile-derived stats, entry status, actual movement, target kind/ID, per-unit attack sequence, action start/impact/recovery ticks and current action. Match snapshots include an event high-water sequence and a bounded recent stream of attack start/result, damage and death events. Each record carries match scope, event sequence, simulation tick, involved stable IDs, destination and enough final position/type information to render a removed casualty.

Retain the last 120 combat ticks of events, capped at 4096 records. Caps and the oldest retained sequence are explicit so a consumer detects a gap. Reset obsolete history at the next wave while event and attack identities remain monotonic within the match. Events do not expire through paused wall time. On initial join/resume, baseline the event cursor at the received high-water mark and reconstruct only current living/action state; do not play old shots or spawn historical corpses. During continuous synchronization, process newer retained events once, seek late effects to their current age, and discard expired effects. A history gap snaps to current state and resets effects; it never changes health. Unit destination changes cancel visual actions aimed at the former city. Existing match/revision and session-generation guards reject obsolete delivery.

This avoids losing short actions between 20 Hz snapshots without adding unreliable RPC effects or unbounded logs. No durable event sourcing or restart recovery is introduced.

### 5. Verified rigged assets and C# animation binding

Read-only inspection of the official [Adventurers repository](https://github.com/KayKit-Game-Assets/KayKit-Character-Pack-Adventures-1.0/tree/672074b73ba276876a19e8816ecdc5241817ab47) at commit `672074b73ba276876a19e8816ecdc5241817ab47` confirmed one skin and embedded textures in both `Knight.glb` and `Rogue.glb`. Each contains `Idle`, `Walking_A/B`, `Running_A/B`, `1H_Melee_Attack_Slice_Horizontal`, `2H_Ranged_Shoot`, `Hit_A/B` and `Death_A/B`, with `handslot.r` attachment nodes. The same source has sword, two-handed crossbow and arrow assets and a CC0 license. These exact files supply the initial subset; a second animation pack or paid tier is unnecessary. Import/attachment appearance remains an implementation proof, not an already executed Godot check.

Vendor the Knight for swordsmen/enemies and Rogue for crossbowmen, plus required weapon buffers/textures and license. Record immutable file URLs, hashes, retrieval date and any derivatives in the existing asset manifest. Keep terrain/resource provenance intact. Use original materials and restrained faction markers; disable/replace embedded accessories as needed so the displayed weapon matches the profile. Normalize against a reference rest pose and stable skeleton root rather than animated bounds.

Add `UnitView` and a C# clip mapping beside `Tabletop`. Programmatically build an `AnimationTree` using the imported `AnimationPlayer`: locomotion blending, melee/shoot one-shots, a limited hit layer, and a terminal death state. [Godot's animation documentation](https://docs.godotengine.org/en/stable/tutorials/animation/animation_tree.html) describes imported players, one-shot nodes, blending and time seeking. Validate required clip names and skeleton paths after command-line import. Keep root motion disabled and strip/ignore method tracks that could invoke gameplay.

Map clip strike/release markers to authoritative impact ticks by seeking/time scaling; exact normalized clip markers are visual tuning values measured in the asset proof. Locomotion uses actual constrained motion, with deterministic cosmetic clip variants. Snapshot interpolation has a small bounded delay, no extrapolation past contact, and the same clock for pose and effects. Pause, transport loss or unsynchronized state freezes that clock. Resume continues its remainder without catching up wall time; resynchronization seeks to current authoritative progress. Death removes the combat body immediately and leaves a non-interactive visual for at most two unpaused presentation seconds; it may finish during building/outcome even when combat tick is stationary. Pause also freezes this cleanup timer. Session replacement frees all views, effects, event cursors and interpolation buffers; unfocused city visuals continue coherent state or seek on focus.

Alternative: procedural dummy bobbing is cheaper but does not meet the selected rigged-animation scope. A physical projectile/animation-callback damage model would couple rules to rendering and fail dedicated-server parity.

### 6. Verification by defect and cost

- Cheap tests own profile/command validation, contact bounds/separation, 32-vs-32 mixed-army progress, congested entry and redistribution, attack cancellation and simultaneous damage, pause, deterministic snapshots/events, history gaps and session disposal. Extend existing fixtures and authority tests; retain the ordinary winning strategy and all cooperative regressions.
- Extend `authority-resume-victory` to recruit a ranged unit through normal requests and assert frozen/current typed action state on resume. Extend `redistribution` to assert identity/profile/recovery retention and spacing of active arrivals; preserve future allocations and observer coverage. No new full-match network scenario is needed.
- Add source UI id `combat` in `Scenarios.cs` and the source-selection path, executed serially with `economy`, `reconnect`, `settings` and `launcher`. It owns a server, one graphical client and an ordinary headless observer on `PrivateDisplay`; use existing `Child`, fresh probe IDs, controls and frame capture. Farm upgrade plus barracks and normal production funds one of each type before wave one. A four-enemy wave against this small army provides natural locomotion, melee/shooting, hit and casualty milestones. Drive readiness normally; never inject health/resources. Pause after observing a pending attack, assert frozen poses/effects, resume and wait for death cleanup, then return/start fresh and assert no stale views. Numerical/network tests cover reconnect intricacies; extend the existing graphical reconnect slice with current unit-state assertions rather than adding a second battle/reconnect matrix.
- Expose current per-view clip/pose time, rendered position, action/event cursor and active/death status through the existing UI observation response. These are read-only observations of actual nodes, not self-reported success flags. Compare to protocol state and capture rendered checkpoints. The new slice catches missing skeleton bindings, wrong clips, animation clocks, visual contact and cleanup that headless tests cannot inspect. Target 20–40 seconds including setup on software rendering, with a bounded battle deadline of 60 seconds; measure actual costs during apply. Maintenance adds one scenario, clip bindings and probe fields. If the ordinary strategy cannot reach these milestones, fix the fixture strategy through normal gameplay rather than adding test-only authority mutations.
- Extend `exported-package` with character/clip binding checks and one short combat checkpoint using its existing owned setup; do not repeat three waves. Packed-only omissions are not detected by source imports. Do not prepare source or rebuild exports when this slice is selected directly.

All source checks still gate sequential client/server exports, then headless and graphical package smoke. Run full `mise run ci` before and after implementation, using selected slices during iteration and `mise run test` for rule changes. Store evidence/timings in ignored run directories and record unavailable prerequisites/platforms honestly. Windows packaging work remains applicable; Linux software-rendered checks do not establish native Windows/GPU performance or real Steam multiplayer acceptance.

## Risks / Trade-offs

- [Congestion or stable-order movement bias] → Bounded swept contact, reproducible tie-breaking and progress fixtures across mixed formations, deaths and arrivals; optimize only after evidence identifies a problem.
- [Changed battle balance from spacing and windup] → Preserve economic/outcome contracts and winning-strategy regressions; record any deliberate profile adjustment and measured battle duration.
- [Assets have clips but import paths or weapons bind incorrectly] → Prove the pinned Knight/Rogue imports, clip poses and hand attachments before integrating all views; no silent dummy fallback.
- [Snapshots grow with events] → Explicit 120-tick/4096-record bounds, gap recovery, cheap payload-size checks for the 64-unit fixture and observed network delivery costs.
- [Arch static registration or stale Godot views survive sessions] → Owned idempotent world teardown, repeated session lifecycle tests and visible fresh-session cleanup assertions.
- [Concurrent distribution edits overlap project/lock files] → Refresh only intentional Arch dependency graphs for existing configurations and preserve current RID/export/release inputs.

## Migration Plan

1. Record the full pre-change CI baseline and establish pinned asset import/animation compatibility.
2. Add Arch and intentional dependency locks, then convert unit ownership and fixture seams while keeping authority/session behavior intact.
3. Implement contact, profiles and attack/event timing with cheap regressions; update protocol and all consumers together.
4. Vendor proven assets, integrate C# views and recruitment controls, then run selected network/UI checks and packed smoke through the normal gates.
5. Format C# with `dotnet format Odot.slnx --no-restore` after solution restore; run final full CI, record cost/evidence and platform limitations, and review all spec/task acceptance.

This is a coordinated source/export update, with no live data migration or deployment step. Rollback reverts this change's source, asset subset, dependency-lock additions and protocol together and rebuilds compatible exports; it does not revert unrelated distribution work. Existing resume credentials are valid only against their original running authority, which remains the current lifecycle contract.
