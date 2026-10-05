# Authored rendering and live observation cadence

## Current scope and outcome

Steering 012 authorizes game-side optimization only. Source exports remain byte-identical at
`a640d721065233dfbf488747694cd95110d4a907`; the source checkout is clean and read-only.
No GLB, texture, rig, clip, socket, numerical rule, frozen reference or license was changed.
Earlier suggestions to optimize the separate asset project are superseded, not authorized work.

Buildings do **not** change authoritative walking: it still lasts 30 ticks / 0.5 simulation
seconds. The previous graphical samples were 31–36 ticks apart, so they could not establish
advancement of one committed action. Render workload, observation admission, presentation
progress and evidence persistence must be measured separately.

The finite investigation rejected static batching on practical cost grounds and retained
read-only live admission. Its original failed melee attempt and subsequent diagnosis below
remain historical evidence. Separately authorized steering 015 proved exclusive owner-thread
ENet guest service and a safe early observer handoff; **the four-view melee gate now passes**.
See [current service/capture evidence](enet-guest-cadence.md) and the
[migration report](authored-migration.md) for final environment/full-CI/package status.
Steering 016 resolved [current-authority casualty admission](current-casualty-admission.md)
without weakening its predicate/deadline; full after-CI including both package smokes
passed `logs/20261005-122036-c53487dd/` in553.42s. This is not permission for another
rendering optimization or renewed melee budget. Earlier stopping points below are historical.

## Official guidance and the actual backend

The owned engine log identifies **Godot 4.7.2 stable .NET**, official `ed1daf0bf`;
`project.godot` selects **GL Compatibility**. Measurements use owned X11/Xvfb and llvmpipe,
not the developer's desktop or a native GPU.

Official references were consulted through their read-only source mirrors after the documentation
web host returned HTTP 403. The tutorial references below are current guidance; the implementation
reference is the exact installed engine tag. Copies are retained in ignored `.art-audit/`.

- [3D performance](https://docs.godotengine.org/en/stable/tutorials/performance/optimizing_3d_performance.html)
  ([official source](https://github.com/godotengine/godot-docs/blob/master/tutorials/performance/optimizing_3d_performance.rst)):
  automatic identical-mesh instancing is **Forward+ only**, not Compatibility. Frustum culling is
  automatic; manual MultiMeshes trade individual culling for all-or-none bounds. Transparent
  geometry must retain back-to-front ordering. This rules out expecting automatic batching here.
- [GPU optimization](https://github.com/godotengine/godot-docs/blob/master/tutorials/performance/gpu_optimization.rst):
  reuse materials/shaders and reduce draw/state changes, but also measure fragment, geometry and
  shadow costs. Fewer calls alone are not a successful optimization.
- [CPU optimization](https://github.com/godotengine/godot-docs/blob/master/tutorials/performance/cpu_optimization.rst):
  measure the bottleneck and repeat comparable samples; distinguish render/driver costs from
  scripts and node traversal. Process/FPS monitors are not isolated GPU timings.
- [Resources](https://github.com/godotengine/godot-docs/blob/master/tutorials/scripting/resources.rst):
  repeated scene instances normally share loaded meshes/materials/textures. Preserve that sharing,
  rather than deep-duplicating imported resources for each building.
- [Import configuration](https://github.com/godotengine/godot-docs/blob/master/tutorials/assets_pipeline/importing_3d_scenes/import_configuration.rst)
  and [advanced import](https://github.com/godotengine/godot-docs/blob/master/tutorials/assets_pipeline/importing_3d_scenes/advanced_import_settings.rst):
  imported LODs and shadow meshes are existing engine facilities, not permission to omit source
  geometry. Retain them. No external material extraction, manually maintained second asset set,
  lightmap bake, reduced shadow quality or source edits were introduced.
- [MultiMeshes](https://github.com/godotengine/godot-docs/blob/master/tutorials/performance/using_multimesh.rst):
  grouped bounds lose per-instance culling. The earlier pine candidate reduced calls 9.48%, but
  increased primitives 5.87% and did not improve latency. Its failure is retained, not repeated.
- [Locked Compatibility shader](https://github.com/godotengine/godot/blob/4.7.2-stable/drivers/gles3/shaders/scene.glsl):
  nonuniform model transforms use inverse-transpose directions, including tangents/binormals,
  and normalize before interpolation. Consequently, naive `AppendFrom` flattening or a generic
  normal/tangent recipe does not establish source rendering fidelity.

Lighting/materials and original static hierarchies stay unchanged. Occlusion/lighting quality
changes would require their own meaningful fidelity and cost proof; they are not hidden settings
changes in this investigation.

## Finite static-fragment candidate: not shipped

A per-scene direct-array candidate grouped only compatible opaque static fragments by shared
material/attribute state. It used verified rendering buffers, inverse-transpose directions,
mirrored handedness/winding, exact UV/UV2/colors/index/material correspondence and codec controls.
It retained original nodes, interactions, collision, attachments and unsupported geometry.
Tangent-space materials, transparency, native LODs, animation/skins, independent visibility,
nondefault render state and unsupported shader features remained original.

The cache prototype shared immutable meshes and invalidated on source/import content, engine,
backend and optimizer version. Synthetic source/import updates proved that changed geometry
could not reuse the old mesh. Native controls rejected duplicates, unsupported materials/state
and actual LOD-bearing surfaces. No per-revision mesh lists or source authoring were used.

Cheap controls passed in `logs/20261005-075608-1b505929/`. The owned same-camera static comparison
passed in `logs/20261005-075702-edeef6a3/` (28.55-second scenario), including native positive/negative
controls and source/cache-update controls. Bakery, Metal mine and Barracks stayed at 1280×720,
unchanged camera/revision and tick zero. The evidence is a characterization pass, not acceptance
of the candidate for gameplay.

| Measurement | Original imported scenes | Static candidate |
|---|---:|---:|
| Draw calls | 8,512 | 6,553 (−23.01%) |
| Objects submitted | 8,841 | 6,882 |
| Primitives | 601,202 | 601,202 |
| Texture bytes | 331,228,203 | unchanged |
| Buffer bytes, both modes with candidate allocated | 42,660,720 | unchanged |
| Settled standard probe response | 493 ms | 507 ms |
| Standard two-frame/post-draw wait | 306–310 ms | 301–303 ms (after transition) |
| Render viewport CPU, representative settled frames | 151–153 ms | 148–153 ms |
| PNG encode/write | 233–235 ms | 233–235 ms |

The native viewport CPU/GPU monitors were explicitly enabled in this owned diagnostic. Their
llvmpipe values are software-renderer measurements, not native GPU performance claims; a single
candidate live sample also reported 211 ms. There is no meaningful practical render/probe gain.
The original buffer baseline previously measured 40,243,206 bytes: keeping candidate buffers
also has a cost, even when before/after modes share that allocation.

The two overview PNGs (`static-cost-7.png` / `static-cost-15.png`) were inspected. Composition,
source silhouettes, water and controls look alike, but the exact comparison reports **76 changed
RGBA channels, maximum delta 127**. This is recorded, not explained away or declared pixel-exact.

**Decision:** do not enable the candidate on call-count/fidelity-only evidence. It and the earlier
pine experiment, their native controls and temporary toggles were removed from compiled gameplay
and retained under ignored `.art-audit/retired-static-batching/` and
`.art-audit/retired-instancing/`. No hidden batch allocations or disabled gameplay branches ship.
The ordinary checked-in asset-fidelity and static cost commands remain purposeful verification.

## Retained correction: live admission, not simulation timing

Standard input/layout probes retain two future `ProcessFrame` barriers plus `FramePostDraw`.
Read-only live probes now wait for the **actual current draw** after command admission in
`Main._PhysicsProcess` and child visual processing, without those unnecessary future-frame waits.
They require owned display/verification/evidence authorization and a fresh ordered request id.
The twelve progression observations additionally require strictly increasing actual process-frame
numbers. Observations remain freshly constructed; raw capture/pose/health/equipment checks and
immutable RGBA/hash/metadata persistence are unchanged. No later frame is resampled.

The same static comparison measured live admission on the **original hierarchy**, independently
of batching: **367–386 ms response**, versus 493 ms for a settled standard response. Live post-draw
wait was 153–155 ms, versus 306–310 ms with the future-frame barrier. The opt-in geometry inventory
still costs 57–77 ms; ordinary combat does not enable it. This admits the existing focused proof,
not a claim that a building became faster or a walking action changed duration.

The selected static cost command now compares standard/live admission on original scenes only:

```sh
mise run test-ui --scenario launcher --checkpoint assets --jobs 1
```

`LandscapeAssets` also retains a small source/import/engine/backend/version-aware **in-memory**
identity for its existing scene, terrain and bounds caches. Changed content clears derived bounds
and terrain, reloading through native `ReplaceDeep`; initial loads use `Reuse` so ordinary scene
instances share resources. No persistent generated asset/cache bundle is added. Normal copied
export/provenance updates still go through the locked importer and exact distribution checks;
this is not hot reimporting Blender/source content or rewriting assets.

Six deterministic cache-key assertions extend the cheap suite to **691 passing tests**:
`logs/20261005-081457-341b235b/`. Changed C# was formatted with locked `dotnet format Odot.slnx
--no-restore`. Full before-CI remains the unchanged clean-main baseline; after-CI is not claimed.

## Single affected melee attempt: progression passes, capture gate fails

`mise run test-ui --scenario combat --checkpoint melee --jobs 1` ran once after the cost/admission
proof. Evidence: `logs/20261005-081600-f50c4eb4/combat-worker/combat/`.

- All **12 observations / four actual raw viewport frames** acquired in **9.0672 seconds**, inside
  the unchanged live **12-second** bound. Ordered exact persistence took **1.5664 seconds**, inside
  the separately awaited **30-second maximum**. Combined time was 10.6336 seconds; the old mixed
  contract is not retroactively claimed.
- **MovementAdvanced=true; AttackAdvanced=true**, with actual imported bone/equipment checks.
  Sample ticks: `8, 11, 15, 18, 22, 26, 64, 64, 68, 72, 76, 76`. Each response has a distinct
  increasing process frame; repeated ticks are fresh renders, not reused observations.
  Post-draw waits were **166–173 ms**, ordinary observation construction **8–9 ms**.
- The pre-Ready Preparation/tick-zero speed-1 barrier remains. Both owned peer evidence files
  record simulation speed 1. No authoritative stepping, action duration or numerical reference
  was changed to obtain the pass.
- Twenty-one live contact witnesses cover **Shared, Near, Far, Simultaneous, Windup, Impact**, but
  **Captures=[]; Complete=false**. Four progression PNGs do **not** replace the required four
  overview/close near/far milestone captures.
- The scenario fails at **49.94 seconds** on the unchanged assertion:
  `ordinary melee proof completes before wave three or a terminal result`.
  Cleanup evidence ends with observer authority at tick **973 / terminal phase**, while the
  graphical client's received state is still tick **439 / Combat**. This establishes a remaining
  admission/presentation/authority-lag concern, not its complete causal explanation.
- Owned server/guest/window/display cleanup completed. No second startup, renewed deadline,
  assertion waiver or full CI followed.

**Historical blocker: `authored-native-receive-backlog` (resolved by steering 015).** Steering 014 supplied the missing bounded
native/clock evidence: [native-pump-timing.md](native-pump-timing.md). The graphical client takes
2.76–13.30 seconds from publication to managed RPC receipt, against the headless observer's
1.01–6.73 milliseconds; downstream decode/application/presentation are fast. The finite verdict
is native receive backlog plus late fixture admission (B + A). An early unpaused observer found
a live tick302 windup while the graphical state was tick9; diagnostic walking/attack advancement
still passed. No fix, full melee retry or four-view acceptance was performed. The proposed narrow
owner-thread ENet pump-cadence correction requires separate authorization. Cheap tests now pass
702, preserving 698; one native control and one real authored diagnostic are consumed.

Historically, steering 013 admitted a bounded observer/admission diagnosis without another live scene. Its retained chronology,
comparison limits, seven executable controls (698 cheap tests passed) and proposed next scope are
in [melee-observation-admission.md](melee-observation-admission.md). Both serialized observer
registration and graphical applied-state lag are established; the compact logs lack the boundary
clocks needed to identify the first network/application divergence. A pre-Ready pause alone can
also freeze progression prematurely, so no safe correction or conditional rerun is claimed.
Preserve the passing walking/capture seam and all original near/far, stage, liveness, death,
equipment and cleanup assertions. The historical 013/014 stopping restrictions are superseded
only by the explicit 015 integration/acceptance authorization; no new asset/renderer optimization
is included. Current completion/package status is in the migration report.
