# Detailed ground consumer integration

## Inventory and provenance

The suite is copied byte-for-byte from `odot-game-assets` at
`ae7f76a28f7bdd8471aeacb8bb40596aec758bfe`, landed through
https://github.com/seraph1nia/odot-game-assets/pull/3. The existing
[manifest](../src/Game/Assets/Authored/manifest.json) is the canonical ID,
source-relative export path, destination-relative path, SHA-256, byte count and
embedded dependency inventory. All 44 previously bundled GLBs also match this
pin without changing their bytes. No Blender sources or reference art are copied.
Owner permission and its limitations remain in the [notice](../src/Game/Assets/Authored/NOTICE.md).

New GLBs, under `src/Game/Assets/Authored/environment/`:

- `hex_ground_moss.glb`
- `hex_ground_grass.glb`
- `hex_ground_dirt.glb`
- `hex_ground_leaf_litter.glb`
- `hex_ground_pine_duff.glb`
- `hex_path_straight.glb`
- `hex_path_turn_120.glb`
- `hex_path_turn_60.glb`
- `hex_path_end.glb`
- `hex_river_overlay_straight.glb`
- `hex_river_overlay_turn_120.glb`
- `hex_river_overlay_turn_60.glb`
- `hex_river_overlay_end.glb`

Only their 54 embedded PNG dependencies are extracted, following the existing
Godot `gltf/embedded_image_handling=1` packaging convention. The manifest's
`derived` records name each file and bind it to its parent GLB/image and exact
embedded bytes. Import sidecars and source UID sidecars are tracked; `.godot`,
compiled output, exports and evidence remain ignored. No source map regeneration,
new image encoding, external texture download or export-byte editing occurs.

## Live presentation

`AssetCatalog` registers the suite; `LandscapeAssets` loads and fidelity-checks
its imported static buffers; `VillageLandscape` batches the actual floor and
ribbon meshes. Menu and every city use this same consumer, not a review-only scene.
`DetailedGround` is a cosmetic coordinate/connector projection, not authority
terrain or a procedural river generator. The follow-up
[rich background composition](rich-background.md) records the current authored
runs, smaller ground patches, instanced woodland and before/after evidence.

- Every rendered lattice cell has exactly one floor. Fixed plots still use dirt;
  the established combat boundary retains its previous families, elevations and
  rotations. Elsewhere, deterministic weighted small patches interleave moss,
  grass clearings, leaf litter and pine duff instead of large west/east stripes.
  Six-way floor rotation is coordinate-only. Material transitions remain visible.
- A complete 34-cell surface watercourse uses straight, both turn classes and
  center ends. Its middle retains the original row-5 bridge/settlement-edge
  contact; its wooded flanks meander across rows 5–8. Only decorative channel
  floors follow its −.03 height. Dry former river-row cells return to the .18
  bank terrace rather than leaving a straight trench beneath the new bends.
- The south bridge walk still ends at `(1,6)` and retains its landing, thickness
  and bank steps. The same initial four trail cells now lead into a nine-cell
  winding run to a wayside shrine clearing at `(1,10)`. A separate six-cell
  woodland trail joins the existing bank lantern and mushroom clearing. No
  through-pieces overlap to invent junctions; neither trail crosses terrace
  height changes, plots, water or the combat approach.
- Passive clearing decoration samples the installed ribbon triangles for local
  contact. Buildings, plots, home/defender, combat units and projectile origins
  keep their original supporting heights/centers/scales. Camera, lighting,
  picking, collisions, clips, sockets, gameplay, network and saved state are
  unchanged. Ground detail is painted/normal-mapped, not displaced geometry.

### Placement and recessed-water boundary

The original flat-top radius 2.55 is converted through the **existing** +90°
Godot-Y rotation and `VillageLayout.TerrainScale`, retaining the pointy lattice,
ordering and terraced heights. Base and overlay share the exact transform origin
and scale; connector turns add positive multiples of 60° about Godot Y. Edge 0
therefore faces west on this lattice. Matching ports are straight edge midpoints,
not vertices. No extra vertical bias, priority, disabled depth test or water-above-
occupant rendering is used. Source base top is zero, bottom −0.36; path/river
minimum clearance is 0.006 source units (about 0.00408 world units).

**The old recessed stream is not elevation-compatible.** This consumer removes
its bridge instance's tagged recessed stream, three foam groups, waterfall and
old scatter-grass mesh before adding the new floor/ribbon. It does not retain a
hidden competing floor. The bridge timbers, deck, supports, props, position and
scale remain imported and unchanged; the surface water is below the deck with
ordinary depth testing. Original bridge/stream export bytes and unrelated
recessed compositions are untouched. The old meadow/stream stay registered for
existing import/fidelity regressions, but neither is live village terrain.
There is no new-to-old water transition, crossing/junction, volumetric water or
new map generator in this change.

## Executable verification

`mise run test` includes exact inventory/hash/dependency checks and focused
executable mapping tests: every 15 unordered pair and six ends in both families,
typed GLB connector metadata, reversed pairs, invalid inputs, signed-row opposite
neighbors, bounded floor selection, protected combat receivers, complete adjacent
non-intersecting flat routes, and stable varied scatter with route clearances.

`mise run test-assets` extends the existing owned headless Godot fidelity probe.
It checks actual imported native-root versus batched rotations, **42 distinct
pair/end placements**, **72 full-profile neighbor joins**, all **40** canonical
base/overlay stacks, normal opaque depth testing and the bridge adaptation/deck
clearance. These are consumer rendering buffers, not source-grep assertions or
external temporary SceneTree probes. Existing vertex/material/normal/UV and
negative corruption regressions remain mandatory.

The existing `Countryside` assertions, shared by ordinary source and exported UI
checks, require actual installed five-family bounds, one floor per cell, continuous
surface-water ports and full edge profiles, no recessed bridge water, joined live
dirt corners/ends and positive clearance, grounded decorations and instanced
scatter with plot/route clearance, unchanged building centers, menu/city
parity and ordinary picking. Timber checks inspect actual emitted bases and
bottom rendering-vertex height ranges across all six walks, including stacked
courses, requiring horizontal orthonormal unit axes and contact with ground or
the lower course while preserving the authored underside relief. This catches
shear that center-only support checks miss. Cheap runner tests reject the actual
south-bank 3D tangent and both floating and penetrating underside profiles;
source/package UI exercises real emitted geometry through the same assertions.
The headless dummy renderer returns identity `MultiMesh` transforms, so it cannot
prove this instance-level regression. The existing launcher/assets slice owns
its graphical execution; no additional scenario or process is added. Existing
cooperative assertions are retained.

For representative views, use the existing selected asset route:

```sh
mise run test-ui --scenario launcher --checkpoint assets
```

After its unchanged static-cost samples, it captures the populated settlement,
bank trail/stream/bridge and west forest using ordinary bounded camera input at
supported gameplay zoom, then verifies building picking/selection. This adds no
new match/display/setup: a few probes and three captures extend the owned route.
The meaningful risk is a loaded-but-hidden floor, missing maps or bad occupied
contact that numerical/GLB tests cannot see. Other orientation classes are honestly
covered by the headless diagnostic, not fabricated live-map views. Full `mise run
ci` still gates all source cases, sequential exports and headless/graphical package
smoke; selected checks are partial coverage.

### Retained evidence

The following author-recorded runs predate the timber-basis review correction.
They do not validate its changed C# inputs; the outer pipeline owns final full
validation. Fresh review-correction evidence is limited to:

- Nine selected `LandscapeChecksTests` / `VillageWalkTests` passed, including
  rejection of the original 3D south-bank tangent on both courses
  (`logs/timber-walk-unit-final.log`).
- Launcher/assets passed in 59.05s
  (`logs/20261009-215212-afbe54c6`). Its bank-trail observation contains all six
  emitted walk batches: the south batch has 14 foundations / 19 total planks,
  unit horizontal axes and underside heights matching each support/course.
  The genuine `launcher-worker/launcher/detailed-ground-bank-trail.png` was
  inspected; the unchanged bridge landing, curved four-cell trail and surface
  stream remain visible. Ordinary building picking passed afterward.
- Earlier review-only headless proof attempts failed and remain under
  `logs/20261009-214628-0a140a49`, `logs/20261009-214743-2eb37b93` and
  `logs/20261009-214830-f8ebed45`. Inspection corrected the bottom-profile
  assumption (the authored shadow strip is 0.55 mm below the wood) and exposed
  dummy-renderer identity instance reads. That unusable proof was removed,
  not counted as passing geometry coverage. No full CI or fresh package view
  was run in this bounded review round.

Original author-recorded evidence:

- Before baseline: first full run failed in existing settings with a native client
  stack-smashing termination/Broken pipe (`logs/20261009-201654-7edcb780`). The
  unchanged selected settings rerun passed (`logs/20261009-202212-5ea88c63`, 55.06s).
  The complete unchanged before gate then passed
  (`logs/20261009-202338-4d5b5ead/ci-summary.json`, 599.06s).
- Cheap implementation checks: 504 core / 579 runner tests passed
  (`logs/20261009-204047-8a06f1b2`, 40.28s).
- First consumer asset proof failed its bridge metadata assumption
  (`logs/20261009-204134-591a163e`). Actual imported resources store typed glTF
  fields in `metadata/extras`. The corrected proof passed
  (`logs/20261009-204343-c87331d7`, 11.92s), including the 40 stacks/72 joins.

- First graphical diagnostic failed the new capture caller's client-data-owner
  argument (`logs/20261009-204528-47eb221a`); its genuine settlement image and
  failure remain retained. Passing the existing `ui-cost` owner into the existing
  scope check fixed the caller, without weakening that check.
- Corrected selected views passed (`logs/20261009-204800-7ba95496`, 58.50s). The
  actual `launcher-worker/launcher/detailed-ground-settlement.png`,
  `detailed-ground-bank-trail.png` and `detailed-ground-forest.png` were inspected:
  dirt/tree/bank textures remain readable at overview and ~1.97× permitted zoom;
  the joined curved trail and rounded end are visible; surface water continues
  beneath the unchanged deck; buildings, boulders, mushrooms and trees remain
  depth-tested above their receivers. No missing maps or competing coplanar
  floors were observed. This is representative consumer inspection, not a
  native GPU result or a calibrated artistic verdict.

- The first after-CI failed an unchanged authored replay determinism unit test
  (`logs/20261009-205036-ff390708`); its unchanged isolated rerun passed. A second
  full after attempt passed six network cases and all five source UI slices, but
  its unchanged general-core testhost remained CPU-spinning for 828s. Only that
  owned CI runner was deliberately cancelled through its registered SIGTERM
  cleanup handler; its aggregate failed/cancelled outcome remains retained at
  `logs/20261009-205259-e3c66f79`, with exports not admitted. The bounded unchanged
  general-core rerun passed all 482 tests in 28.43s
  (`logs/ground-general-rerun.log`), producing no hang sequence or memory dump.
  No fixture, frozen digest, gameplay assertion or numerical rule was changed.
- **Full after gate passed:** `mise run ci --jobs 1 --ui-jobs 1`,
  `logs/20261009-211001-9b786e7b/ci-summary.json`, **915.61s**. Declared serial
  expensive admission keeps the same complete coverage: **504 core / 580 runner
  tests**, actual static fidelity/ground placement proof, six network cases,
  five source UI slices, ordered Linux client/server exports and headless plus
  graphical package smoke. This is not a filtered pass. Default-concurrency
  failed attempts above are not reclassified; their intermittent C# fixture
  behavior remains an unresolved diagnostic concern, not a terrain fix.
  Final source `combat-worker/combat/combat-unit-inspection.png` and packed
  `exported-package-worker/exported-package/packed-layout-1280x720.png` were also
  inspected: actual rigged occupants remain visible above the detailed floor,
  with grounded buildings and the same surface stream/trail in the package.
  The final source-to-destination hash receipt
  `logs/detailed-ground-source-destination-hashes.json` matches all 57 GLBs to
  the pinned source and existing inventory; the mandatory cheap gate also checks
  every extracted map against its immutable embedded bytes.

Owned software X11/Mesa llvmpipe and Dummy audio evidence does not establish
native compositor/GPU speed, physical input or listening quality. Millimeter
surface lips and source micro-detail naturally become subtle at fitted overview;
no original-art pixel equality or whole-catalog artistic approval is claimed.
