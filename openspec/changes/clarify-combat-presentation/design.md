# Design

## Context

See proposal.md for motivation. The scout used unchanged source `0e1d4b7`, seed-one owned melee evidence and exact core diagnostics. Twenty of 25 observed ring routes had backward-progress segments; total ring travel was 137.22 versus 73.31 straight world units within unchanged deadlines. A windup frame had ten stationary visible rigs and seven crossing dark strike bars. Existing route caches and pose sampling optimizations intentionally preserved these aesthetics.

`CombatLayout` is the position/facing seam, `UnitView` owns imported rigs and manual pose seeking, and `MeleeStrike` owns current-action cues. Numerical `CombatSimulation`, `HexRouting` and `HexOccupancy` never consult graphical geometry. Complete current snapshots reconstruct movements and corpses; `CombatPlayback` is a short common clock, not a delayed replay buffer. Renderer frames, numerical ticks and rig clip seconds are separate.

## Goals / Non-Goals

**Goals:** remove presentation detours and sprinting; make current attack intention, weapon swing and landed result individually recognizable; keep numerical trace references identical; concentrate engine-independent timing decisions behind a small pure presentation helper so tests exercise the same calculations as rigs.

**Non-Goals:** changing core action/target/occupancy/status rules, authoritative tick rate, collision admission, role balance, formation anchors, wire identities, verification batching, renderer FPS, delayed replay, shared slow motion, new assets/dependencies, economy, rebranding or releases. No claim of native frame pacing or listening quality.

## Decisions

### 1. Direct interpolation of existing committed endpoints

Replace the ring/portal route with one linear interpolation between declared anchors. Use authoritative elapsed/start/end ticks, FrozenTick and FrozenMoveTicks for terminal/current dying movement. No action is lengthened, merged, retargeted or given a new numerical route. Moving miniatures can overlap any other model; settled miniatures remain at distinct anchors. This deliberately removes the old visual-clearance constraint rather than touching numerical claims. Avoid collision steering, cosmetic detours or random offsets, all of which would restore the confusing movement.

### 2. Pure timing policies, bounded transitions

Add a small engine-independent presentation timing module in `src/Game`, linked into cheap tooling tests without a Godot runtime. It owns move progress, bounded locomotion/action envelopes and shortest-angle facing interpolation. It returns calculated values, never mutates combat state or consumes randomness.

Use the already bundled walking clip. Derive a bounded idle/walk envelope from the committed move clock and attack/hit blends from authoritative action or presentation-effect timing. Manual AnimationTree sampling currently advances by zero; do not add nonzero OneShot fades that would require a second independent engine clock or leave fades unadvanced. Explicit sampled blend weights preserve marker seeking and give cheap pause/endpoint tests. Keep existing action identity/deduplication and immediate death precedence. Facing transitions use only advancement of the shared combat tick, not wall delta or camera probes, so pause/loss is stable. Admission/reconnect initialize at the current desired direction; hidden views retain logical sampling.

### 3. Intent is not a laser

Replace the long opaque hand-to-target boxes with bounded dashed ground-space intention and a compact target pointer during windup. Keep actual sampled target attribution, including far-side anchors. At impact use a short local slice/chop accent near the attacking weapon and a compact landed target-side flash; a miss has no landed flash. No lunge, stretched weapon, replacement projectile or physical reach requirement. Reuse unshaded material/mesh ownership and current-action reconstruction; keep existing impact trail bounds. Observations expose cue style/phase and motion/blend evidence rather than only asserting some mesh exists.

### 4. Preserve cheap authoritative references; extend one rendered slice

Retain the five checked-in ReferenceTraces hashes and existing reversed-storage/committed-action tests. Add targeted tests of move fraction, endpoint/frozen poses, walking/envelopes, facing freeze/wrap and settled-versus-transit clearance. The old contact assertion becomes a settled-anchor assertion, not an indiscriminate pass: opposing/ally settled roots and settled corpses still cannot collapse. Transit overlap is expected and independently tested.

Extend the seed-one melee checkpoint within its existing ordinary setup and owners. The incremental defect is a rendered rig still sprinting, detouring, snapping or drawing a beam despite correct pure calculations/static action endpoints: cheap timing tests and four paused PNGs cannot establish intervening rendered progression. Capture a bounded sequence (at most 12 fresh rendered frames/observations) across committed movement and live attack progression, with wall timestamps, tick/action/position/heading/cue evidence and normal-speed barriers. Expected incremental cost is several seconds of owned capture, no additional display/process/setup; retain the current 65-second feature deadline unless measured evidence justifies a small explicit change. Keep four overview/close near/far windup/impact captures, cooperative authority agreement, pause/current-death/health assertions, actual input and awaited cleanup. No separate scenario, option matrix or graphical 32-vs-32 battle. Sequence setup and capture costs are reported separately from source/CI runtime; no frame throughput claim is inferred.

## Risks / Trade-offs

- [Crossing models obscure one another] -> Requested transit overlap is explicit; settled anchors/health bars remain, no invisible numerical collision change. Inspect retained owned frames; report limitations honestly.
- [Blends obscure a strike marker] -> Explicit envelope reaches full attack weight at impact, cheap marker tests and imported-rig gate; death overrides all living layers.
- [Facing changes on pause or reconnect] -> Tick-driven advancement with baseline initialization, frozen current state, existing pause/focus/reconnect assertions.
- [Subtle cues disappear at overview or lose far-side attribution] -> Existing near/far shared-hex proof plus observations and inspected overview/close PNGs; bound dash/pointer geometry rather than making another thick beam.
- [Frame capture distorts observed pace] -> Retain wall timestamps and simulated ticks, no slows/skipped ticks, no native smoothness/performance inference; finite sequence is owned evidence, not physical-desktop automation.
- [Work expands into slow-motion/gameplay] -> Keep all authority files/rule fingerprints and wire identities unchanged. Any later pacing/turn change needs another proposal.

## Migration Plan

Capture a full before CI baseline before code edits. Implement and test the pure presentation slice, run cheap checks and selected melee/reconnect checks after relevant changes, format changed C# after locked restore, then run full after CI with sequential exports/package smokes. Record evidence and limitations in checked-in verification documentation. Sync the two completed capability deltas without archiving unrelated changes. Commit only intended source/tests/docs/OpenSpec artifacts on `fm/odot-explore-autobattle`; scratch probes/logs remain untracked. The authorized delivery pipeline owns later push/validation; firstmate alone merges passing checks. Rollback restores presentation and its specific visual-clearance contract together; no save/protocol/dependency migration is needed.
