# Authored static geometry fidelity

**Current continuation:** [Runtime rendering and live cadence](runtime-rendering.md)
records steering 012, official Godot guidance, the rejected opaque-static candidate,
the retained live-admission correction and historical melee capture failures.
[ENet guest cadence](enet-guest-cadence.md#early-observer-and-exact-paused-images)
owns the subsequent passing four-view proof; the [migration report](authored-migration.md#retained-acceptance-evidence)
separates pre-review acceptance from changed review inputs.
Sections below retain historical causal evidence, not current execution permissions.
Both disabled instancing/batching experiments and their native diagnostic branches
have been retired from compiled gameplay into ignored audit scratch. Source-authoring
recommendations below are superseded by the current game-side-only scope.

## Decision: defer optional scene consolidation

Buildings and props retain their original imported authored hierarchies rather
than undergoing optional scene consolidation. Live terrain and the timber walk
use fidelity-validated imported-buffer consolidation; terrain uses chunked
instances. [Detailed-ground integration](detailed-ground.md#placement-and-recessed-water-boundary)
owns the current floor/overlay placement and bridge consumer adaptation.
This is not a legacy fallback: all required geometry remains authored content.
No original GLB bytes or frozen numerical references were changed.

The proposed `0.0001` bounds waiver was removed. Two failed graphical starts are
retained at `logs/20261004-203745-7e2ec128/` and
`logs/20261004-204040-d8909ede/`. Matching face/material counts did **not** prove
fidelity. The Arrow tower also contains mirrored instances: flattening them
without preserving per-instance culling, index winding and tangent handedness is
unsupported and explicitly rejected by the consolidation interface.

## Executable causal check

Run `mise run test-assets`. It restores/builds/imports normally, then runs one
owned, bounded headless Godot process through the same `StaticGeometry` interface
used by terrain. It needs no graphical display, Steam account or asset checkout.
CI invokes this check after its single shared build/import. Ordinary `mise run
test` remains engine-free. Evidence includes the JSON rendering-buffer report and
supervised process logs; owner cleanup precedes runtime-data removal.

The successful characterization is
`logs/20261004-212349-25f64761/authored-static-fidelity/rendering-buffer-fidelity.json`.
It covers all 36 required non-character exports and translated/rotated/uniform
and nonuniform synthetic hierarchies. Every rendering vertex is compared in the
same flattened import coordinate space. Index sequence/triangle partition,
material identity, vertex cardinality, normals, tangents/handedness, UV/UV2 and
vertex colors are checked. Nonfinite/singular transforms and unsupported
animated/skinned/custom attributes remain hard failures.

Fourteen regressions exercise unchanged near-zero buffers, displacement, scale,
index winding, lost triangles, lost vertices, changed normal/UV/material,
hierarchy corruption, mirrored instances, nonfinite hierarchy, an actual authored
animated/skinned character and nonuniform normal-transform corruption. These are
real engine-buffer tests, not source-text or counts-only assertions.

## Measured findings and allowance

- Across the measured candidates, **maximum rendering-position and UV errors
  were zero**. The old `Mesh.GetFaces()` guard instead inspected collision
  triangle geometry with vertex welding. Its near-zero minimum changed from
  `-0.15256499` to `-0.15259999`, although the actual rendering positions did not
  move. Placement now reads all rendering vertices, not welded collision faces.
- `SurfaceTool.AppendFrom` does not preserve inverse-transpose normals under all
  imported nonuniform transforms. Archery range maximum normal-vector error was
  **0.7730**, Barracks **1.0595**, Town hall **0.8550**, and the bridge **0.4524**.
  The independently uploaded codec control had errors around `0.00001–0.0001`,
  not these large deviations. Original hierarchies are therefore retained;
  increasing a position allowance would conceal an unrelated rendering defect.
- Meadow/stream retained **252/416 rendering vertices**, **132/216 triangles**
  and **4/5 material surfaces** respectively, with exact index/material/UV
  correspondence. Normal-vector errors were `0.0000614/0.0000965`; tangent errors
  `0.0001071/0.0001118`. Their independent direct-array codec controls distinguish
  coupled normal/tangent re-encoding from changed geometry.
- Position validation uses a per-vertex binary32 arithmetic bound, not a fixed
  visual waiver: `8 × 2^-23 × max(1, sum of absolute transform operands)`.
  Four multiply/add terms per component motivate the arithmetic margin. The
  terrain maximum allowance is **0.0000036654 import units**, or
  **0.0000024897 world units** at scale `1.7320508 / 2.55`; measured displacement
  remains **zero**. No gameplay/contact tolerance was changed.
- Direction comparison uses the independently uploaded expected-array codec
  result plus a bounded RG16 octahedral lattice margin (`8 / 65535`), checking
  both total error and distance from that codec control. UVs/colors, topology,
  cardinality, material identity and tangent handedness do not receive a
  displacement waiver. Actual nonuniform-normal/culling defects still fail.

Headless buffer fidelity is not graphical acceptance. The subsequent focused
owned launcher passed at both sizes:
`logs/20261004-212445-f5ee9577/` (136.96-second scenario). Original hierarchies
retain a significant render cost; software observations previously showed about
10–12k draw calls and 332 MB textures. These are integration risks, not claims
about native GPU performance.

## Outstanding graphical cost concern

The first authored melee attempt
`logs/20261004-212803-17829ac5/` passed actual walking/attack/equipment progression,
but its speed-1 barrier acknowledged at authority tick **404**, after the
unchanged seed-1 shared windup at **301**. The existing barrier now precedes
combat Ready, with an actual Preparation/tick-zero assertion.

The affected retry `logs/20261004-213735-00b656c2/` verified that pre-entry barrier,
and its retained ten frames prove movement and attack advancement, but the
12-second progression capture budget expired at **12.0755 seconds** before all
twelve observations/four PNGs completed. Near/far captures remain unexecuted,
not passing. No deadline, gameplay assertion or numerical reference was relaxed.
Full after-CI and final migration acceptance remain outstanding.

## Bounded static capture-cost diagnosis (steering 008)

`mise run test-ui --scenario launcher --checkpoint assets --jobs 1` passed in
`logs/20261004-215852-161eed99/` (21.91-second owned scenario). This is a
selected diagnostic, not ordinary launcher or combat acceptance. One ordinary
solo scene with Bakery, Metal mine and Barracks stayed at 1280×720, unchanged
camera/revision and combat tick zero, for four lightweight and four PNG probes.
Owned window/display/runtime cleanup completed.

- Actual renderer totals: **8,512 calls, 8,841 objects, 601,202 primitives**;
  texture memory **331,228,203 bytes**; observed software rate **3–6 FPS**.
- Lightweight response: **522.9–525.5 ms**; PNG response **761.5–786.9 ms**.
  Two-frame/post-draw wait: **309.8–312.9 ms**; viewport readback **1.2–2.9 ms**;
  PNG encoding plus filesystem write **233.6–234.9 ms**; ordinary observation
  construction with enabled geometry inventory **55.5–80.7 ms**. JSON evidence
  write was **0.5–0.9 ms** after the first 26.5-ms serialization warmup.
- Geometry inventory groups count visible-in-tree submissions *before* renderer
  frustum/occlusion/shadow passes, not actual per-group GPU draw attribution.
  Pines: **796 mesh nodes/1,393 surfaces**, three material identities; decorated
  bridge **849/869**, Metal mine **453/493**, Barracks **407/439**. CPU process
  monitor ranged **204–475 ms**; this is not an isolated GPU frame timer.
- Authority/client revision equality and tick zero establish an unchanged static
  control only, not advancing combat/liveness acceptance. The mixed 12-second
  progression deadline includes frame admission, ordinary waits, observations
  and captures; it has **not** been increased or relabeled capture-only.

A conservative native-instancing candidate for repeated outer pines/boulders is
retained **unvalidated and disabled by default** in `RepeatedScenery.cs`. It keeps
original nodes/buffers/materials and adds chunked native MultiMeshes, with intended
actual transform/resource/layer/duplicate-render negative controls. No successful
before/after candidate run exists: two build preparations failed (local command
variable names, then `RequireOwnedUiWorker` call arity). The latter method takes
no arguments; that call was subsequently corrected under steering 009 without
an overload or ownership-guard change. At this initial stop, neither the
instancing candidate nor its controls were accepted.

No source GLBs, gameplay timing, witness count, cleanup assertions or fidelity
allowances were changed. Source-asset optimization is a separate recommendation:
reduce authored repeated-scene object/material fragmentation in a separately
approved source task, or measure an existing lighter tree substitute (for example
forest spiral tree) before changing the approved art. Neither recommendation is
implemented by this diagnostic.

### Steering 009: compilation recovered, native readback proof blocked

The existing zero-argument ownership guard is now called correctly. Cheap
`dotnet build Odot.slnx --no-restore` passed with zero warnings/errors. Subsequent
proof preparation found the exact Godot property spelling `GIMode` (not
`GiMode`); after correcting it against the locked API docs, the cheap solution
build passed again. No guard removal, overload, protocol permission widening,
tool installation or authority change was used.

The disabled candidate was tightened to the two explicit passive source paths,
excluding skinned/animated/hidden/overridden-material and mirrored/nonuniform
instances. Original scene nodes remain. Explicit conservative union AABBs and
copied culling/LOD/GI/shadow settings accompany unchanged imported mesh references
and exact transform readback assertions. The added headless proof attempts real
source placements, retention and corruption controls; those controls are **not
accepted**, because the proof stops before completion.

Two failed owned headless attempts are retained:
`logs/20261004-222604-979c617f/` and
`logs/20261004-222746-ed01e025/`. The detailed second failure reports member zero:
expected basis diagonal **0.29166666**, actual MultiMesh readback **identity**.
Original/current transform equality, mesh identity, explicit bounds, GI, LOD,
cull margin, occlusion setting, layers and shadows all report true; visibility
is correctly original=true/batch=false while disabled. The transform assertion
still hard-fails. This may be a headless RenderingServer storage/readback
limitation rather than changed rendered geometry, but that explanation has not
been independently established and must not be treated as a pass.

Under the repeated-obstacle rule, no further engine preparation, graphical
before/after comparison, melee or full CI started. The next finite alternative
for Firstmate consideration is to keep the existing headless array-fidelity
checks and validate renderer-backed MultiMesh readback/control behavior inside
the already-owned static graphical slice; do not replace exact checks with
counts or a positional waiver. Until that is authorized/proven, native instancing
stays disabled and the new headless check is known failing. No measured candidate
cost reduction or rendered equivalence was claimed at that stop.

### Steering 010: independent backend cause established; cost gate still unmet

The checked-in independent `MultiMeshControl` creates a BoxMesh outside the
candidate, explicitly sets 3D transform format/count, then assigns a nonidentity
uniform scale, rotation and translation. It reads back before and after two
process frames (and post-draw on a real renderer), and after swapping/restoring
its mesh. The same implementation ran under both owned backends:

- `logs/20261004-224045-9589c638/`: **headless**, empty renderer identity,
  format Transform3D/count 1. Every read returns identity, not the supplied
  transform (scale 0.73, rotation 0.19/0.31/−0.2, translation
  17.125/−0.005/−21.375). Synchronization and mesh reassignment do not repair it.
  Existing 36-source array-fidelity checks and fourteen corruption regressions
  passed; native MultiMesh transform proof is explicitly **unexecuted here**.
  This does not retroactively turn either old failed native proof into a pass.
- `logs/20261004-224059-5e81f1b5/`: **X11 / llvmpipe (LLVM 23.1.1, 256 bits)**.
  Every independent readback matches exactly, before/after synchronization and
  after mesh reassignment. The renderer-backed candidate then passed its fifteen
  actual source-member control, exact matrices/buffer identities, union bounds,
  retained hierarchy and layer/shadow checks. Wrong identity/scale/translation,
  mesh replacement, bounds, layers, shadows and duplicate rendering are rejected;
  mirrored/nonuniform/animated/skinned/hidden/material-overridden source remains
  original. No engine error was reported. Owned process/display cleanup passed.

Thus the specific identity-readback failure is a **headless backend capability
limitation**, not setter order, format/count, source transform space or resource
lifetime in the renderer-backed path. Native assertions remain renderer-backed
in `launcher/assets`, never silently replaced with intended CPU transform tables.

The same single owned ordinary static scene/camera at 1280×720 was sampled before
and after activating the candidate, with four lightweight and four PNG probes in
each mode. Camera/authority revision/combat tick remained unchanged.

| Observation | Original | Candidate |
|---|---:|---:|
| Actual draw calls | 8,512 | 7,705 (−9.48%) |
| Actual primitives | 601,202 | 636,487 (+5.87%) |
| Texture bytes | 331,228,203 | unchanged |
| Buffer bytes, settled | 40,243,206 | unchanged (both modes retain candidate buffers for comparison) |
| Lightweight response, settled | 505–549 ms | 528–553 ms |
| PNG response | 770–787 ms | 776–798 ms |
| Last probe frame/post-draw wait | 315.2 ms | 315.4 ms |
| Last PNG encode/write | 233.8 ms | 234.0 ms |
| Observed software FPS | 4–6 | 3–6 |

The first mode-transition probe cost 849 ms. Process monitors include observation,
encoding and frame workload (216–465 ms original, 238–483 ms candidate), not an
isolated GPU timer. Conservative chunk union bounds submit more offscreen
primitives than original per-object culling. The reduced submission count did
**not** produce a useful capture/frame latency improvement; no native GPU claim
or combat-budget improvement follows.

Inspected `static-cost-7.png` and `static-cost-15.png`: overall silhouettes,
placement, trees, rocks, water, shadows and controls appear equivalent at the
captured overview. Exact full-frame comparison is **not zero**: 88 changed RGBA
channels / 30 pixels, within X403–517/Y320–355 over the populated buildings.
Other candidate PNGs differ by 85–91 channels; maximum channel delta is 170.
These small differences are recorded, not waived as exact pixel fidelity or
attributed to an unproven cause. World geometry/material/settings assertions
pass independently; visual acceptance beyond this static view remains limited.

**Decision:** candidate cost gate is unmet. Keep the original authored rendering
and the candidate disabled; ordinary gameplay no longer allocates hidden candidate
batches. Only the owned static cost environment exercises it. No melee or full CI
started and no mixed 12-second deadline changed.

Finite next bottleneck/recommendations, not implemented:
1. A separately authorized asset-source optimization should address fragment
   count in the bridge (869 observed surfaces) and populated building hierarchies
   (Metal mine 493, Barracks 439, Arrow tower 371), preserving the approved art,
   materials/geometry/interaction. This needs source-authoring approval and new
   provenance/fidelity evidence, not another game-side flattening waiver.
2. A separately admitted capture-only I/O design may address the measured ~234-ms
   PNG cost, but cannot fix ~315-ms frame admission/render workload by relabeling
   the mixed progression deadline. Preserve authority and all twelve live
   observations/four witnesses if that seam is changed.
3. Existing forest spiral-tree/boulder source alternatives could be measured for
   composition/resource trade-offs in an approved art decision; they are not
   assumed cheaper, and cannot replace bridge/building requirements by omission.

### Steering 011: exact frame acquisition/persistence seam; live cadence blocker

The former `MeleeProgression` linked 12-second watchdog owned phase waits,
twelve fresh UI probes, four viewport readbacks, synchronous PNG encoding/writing,
per-capture JSON evidence writes and movement/attack assertions. It was a **mixed**
wall-clock contract; the 12.0755-second old failure remains failed.

The new owned verification-only seam preserves twelve live observations/four
actual raw viewport acquisitions within the original 12 seconds and retains
phase predicates, movement/attack/equipment, health/contact and all later stage,
pause/death/cleanup requirements. Only persistence of the exact already-acquired
frames moves outside that bound:

- `Image.GetData()` is documented by the locked Godot API as returning a **copy**.
  `OwnedFrameCapture` clones and privately retains that RGBA8 byte array, dimensions,
  SHA-256, unique observation ID and the serialized acquisition metadata, including
  camera, revision, tick and live poses. It has four-frame capacity and rejects
  duplicate IDs/paths, missing frames and out-of-order persistence.
- Raw observations explicitly have `Screenshot=null` plus an acquired receipt:
  no PNG existence is claimed. The runner later asks the same engine owner to
  reconstruct an Image from those bytes and save it, without sampling a later
  viewport or changing metadata. The native PNG is decoded and compared **byte
  for byte** before the exact acquired receipt is acknowledged and the existing
  rendered-frame check can claim file persistence.
- No native Image/Texture/scene reference crosses a thread; creation, encoding
  and decoding remain on the Godot owning thread. No additional encoder, package
  or worker framework was introduced. The ordered child driver awaits every
  persistence receipt and evidence write under a separate **30-second maximum**
  I/O watchdog, including acquired-frame cleanup on live failure/cancellation.
  It cannot clear ownership while a persistence callback is active. Scope/process
  cleanup remains awaited if persistence fails or its bounded wait expires.
- The original offline-menu ownership guard retains its original role restriction.
  Raw-capture commands separately require the complete owned X11 worker identity,
  verification marker and this scope's explicit evidence directory. Authority
  speed/timing is untouched. The pre-Ready speed-one Preparation/tick-zero barrier
  remains. The optional expensive geometry inventory was already opt-in to
  `launcher/assets`; it was not enabled in advancing combat, so its measured
  55–81 ms diagnostic overhead was **not** mislabeled combat savings. No mutable
  live observations or filesystem inventories were cached.

Cheap checks passed: `logs/20261004-231514-0c319b86/` contains **685 tests** (13
solo/session, nine cooperative, 478 general, 185 tooling). Six new tests include
controlled logical persistence delay/authority advancement without captured
pixel/tick/revision/pose changes, slow/missing live failures at the unchanged
12 seconds, duplicate/order rejection, corrupt/missing PNG failure, cancellation
and guarded ownership release. The native copied-RGBA/PNG decode control passed
in `logs/20261004-231553-2550caad/`, on the real Godot image API without a display;
this proves encoding identity, not gameplay or viewport throughput.

Exactly one authorized owned melee attempt followed:
`logs/20261004-231754-3dffd17b/` (**failed**, 40.43-second scenario). It retained
all twelve live observations/four raw captures in **7.7891 seconds**, then
persisted all four PNGs with identical decoded pixels and original metadata in
**1.6466 seconds** (individual native encoding/decode acknowledgements
225–235 ms). Combined capture/evidence time was **9.4356 seconds**, but the
runner explicitly does **not** claim the former mixed contract as acceptance.
The four persisted raw frame ticks/revisions are **130/154, 233/257, 365/389,
465/489**. Attack progression passed; walking progression **failed**:

`owned normal-speed frames show the same committed walking action advancing directly`

Observed ticks were **64, 98, 130, 166, 197, 233, 301, 333, 365, 401, 433, 465**.
The 31–36-tick sequential cadence exceeds a 30-tick committed walking action;
none of these pairs proves that same live action advancing. Frame/post-draw waits
were **350–364 ms**, ordinary observation construction **8–9 ms**, and actual
software draw calls **9,101–10,127**. Thus PNG deferral solved the mixed-deadline
pressure in this attempt, but it did **not** solve the live rendering/observation
cadence required for movement acceptance. The failure was not waived or replaced
by attack success, intended transforms or pictures. `combat-melee-witnesses.json`
remains Coverage=None/Complete=false; the four progression PNGs are **not** the
missing near/far milestone captures. Owned peers/display/runtime cleanup completed.

**Stop decision:** no additional startup, renderer/capture experiment, relaxed
witness or automatic budget retry. Instancing remains disabled; source GLBs and
all uncommitted work stay preserved. Final environment/roster/full CI remains
outstanding.

A separate captain-approved source-fragment optimization would need to preserve
all source silhouettes/materials/UVs/rigs/attachments while reducing bridge and
building mesh-object fragmentation. Candidate scope: the existing bridge and
most-used populated producer/barracks/defender exports, with authored-transform,
normal/tangent/culling/shadow/interaction fidelity controls and new immutable
export provenance. Do not change terrain topology, combat durations or poses to
obtain throughput. Measured need: observe the same 30-tick/0.5-second action more
than once, requiring effective fresh-observation intervals **below 500 ms**, with
margin rather than the observed ~533–600 ms cadence; a proposed diagnostic target
of ≤400 ms provides 20% margin, not a new gameplay contract. Count reductions
alone are insufficient (the pine/boulder experiment proved that). Source editing
is **not authorized or implemented** by this steer.
