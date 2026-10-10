# Rich live woodland composition

This follows the [detailed-ground consumer](detailed-ground.md), not an asset
inventory refresh. `DetailedGround`, `VillageLayout.GroundTransform` and
`VillageLandscape` still own the cosmetic lattice, placements and rendering.
Menu and city views use the same consumer. No GLB, embedded map, import sidecar,
manifest, existing asset ID, dependency lock or asset-repository file changed.
Normal Godot import also supplied the previously missing C# source UID sidecar
for `NativeBoundaryControls.Receipt.cs`; it is retained as required source-import
metadata, not an art/asset change.

## What is composed

- Weighted coordinate-only patches give the woodland many smaller irregular
  sections, interleaving moss, pine duff, leaf litter and grassy openings. Patch
  centers, influence and adjacency vary at a roughly three-cell scale; it is
  neither an alternating checkerboard nor independent per-cell family noise.
  Fixed settlement dirt receivers remain unchanged. Outside the protected
  approach, hashed six-way rotation breaks the old repeated diagonal alignment.
- One complete **34-cell watercourse** bends repeatedly through the western and
  eastern woods. Its legible central straight reach is retained beneath the
  existing timber bridge and village-edge walk. Both authored corner classes
  and two tucked rocky center ends are used, with no overlapping junctions.
  Only decorative channel floors descend to −.03; dry former row-5 cells meet
  the .18 bank terrace. Bases and ribbons share exact X/Z, height and scale.
  The old recessed stream/foam/fall remains removed from this bridge consumer.
- The bank path keeps its first four cells and continues as a **nine-cell**
  winding route to the wayside shrine. A separate **six-cell** woodland trail
  leads from the existing bank lantern to the mushroom clearing. Both stay on
  the .18 terrace, outside plots and water; no connector crosses a height step.
  The lantern's decorative contact rises to the actual path surface. The six
  orthogonal timber walks, deck landing, foundation courses and supports remain
  unchanged. Two former grove decorations move out of the new channel cells.
- Deterministic woodland clusters have variable canopy size/yaw, grouped
  undergrowth, loose fringes and deliberately empty pockets. The existing spiral
  tree, pine, fern, leaf clump, moss, boulder and mushroom exports are reused.
  Grass clearings are lighter than forest patches; different ground families
  favor fitting undergrowth. Props are excluded from the actual route corridors
  with conservative width/footprint clearance and kept on one supporting terrace.
  The dressed envelope is columns −12..12, rows 3..14. The exterior still has
  its previous sparse decorative coverage, with correct decorative floor contact.

The established combat receiver envelope (including boundary hex footprints)
retains its original floor families, heights and six-way transforms. The new
population's full footprints stay south of Z=7, not merely its origins. Original
battle-side props remain untouched. Buildings, all nine plot centers/scales,
home/defender, towers/projectile origins, occupants, camera/lighting, picking,
collisions, authority rules, gameplay RNG, networking and saves are unchanged.
No simulated fauna, active scenery lights or added decorative shadows appear.

## Shared rendering and executable checks

New scatter uses spatially bounded `MultiMesh` groups, sharing each **original
imported mesh part** and material. Nonuniform local transforms stay on the
instances; no vertex/normal/tangent flattening or copied meshes/textures are
introduced. Unit-size templates are normalized by the existing asset placement
owner, then freed. Two 16-cell-wide scatter buckets bound the finite dressed
area without the first attempt's unnecessarily numerous eight-cell buckets.
Coverage grows deterministically and monotonically only when the camera coverage
envelope grows, never per frame or on an ordinary redraw. Authored connector plans
and clearance corridors are cached once; conservative broad phases skip only
remote corridors/ribbon cells. Nearby contacts still sample actual triangles and
use the same full segment-width test. Cheap equivalence tests include close port
boundaries; these changes do not relax geometric assertions.

Cheap tests execute patch selection, all connector combinations, complete route
adjacency/height/ends/non-intersection, retained bridge cells and battle receivers,
and deterministic scatter distribution/clearance. Negative observation fixtures
reject wrong/missing/duplicate route cells, displaced ports, full-profile height
mismatches, buried overlays, recessed bridge components, wrong scatter contacts,
plot obstruction and corrupted native part transforms. Asset hashes and original
fidelity/negative corruption tests remain mandatory.

The existing `Countryside` checks now inspect every actual installed route port
and **full bank/water/path edge profile**, not just requested centers. Scatter
observations recover placements from actual native instance matrices and check
all emitted part matrices, surface contact, route exclusion and projected plot
selection clearance. Existing menu/city parity, timber underside/orthogonality,
building contact and normal picking/action assertions remain. No new expensive
scenario, process or exhaustive graphical connector matrix was added: the
existing launcher/assets route owns three ordinary-input view captures and real
picking, while cheap tests own composition variants and the existing headless
asset proof owns every orientation and base/overlay stack.

## Evidence and limitations

Before source edits, full `mise run ci --jobs 1 --ui-jobs 1` passed at
`logs/20261009-223933-be290030/ci-summary.json` (**931.34s**) on
`606b102f790f8dec0551f262b418d7b7f3f54cbc` (also retained in
`logs/rich-background/before-head.txt`). It
covers 504 core / 581 runner tests, fidelity/placement, all six network cases,
all five source UI slices, sequential Linux exports and both package smokes.

The unchanged selected route, `mise run test-ui --scenario launcher --checkpoint
assets`, passed in **56.76s** at `logs/20261009-225515-faf3cc55`. Its genuine
settlement, bank-trail and forest images/observations were copied before changes
to read-only `logs/rich-background/before/`; `before.sha256` binds the immutable
files. These are ordinary 1280×720 source gameplay views, with Farm, Metal Mine
and Barracks at the same initial non-battle state, fitted overview and the same
bounded-input ~1.97× detail views. No desktop automation or external SceneTree
probe was used.

Implementation failures are retained, not reclassified as passing:

- `implementation-test-1.log` rejected obsolete straight-row/four-cell fixtures;
  `implementation-test-2.log` caught authored coordinate adjacency mistakes.
  The fixtures now enforce complete routes and stronger geometry checks.
- `logs/20261009-230432-9ce6a589` failed the new scatter guard: one low leaf clump
  had Z=6.909. Full-footprint exclusion, not a weaker guard, fixed it. This
  attempt's resource observations also led to coarser finite scatter buckets.
- `logs/20261009-230912-07a20b22` failed existing contact assertions: the original
  lantern was 8.15mm below the newly installed woodland path. It now resolves
  actual cosmetic ribbon contact; the assertion was not relaxed.
- Intermediate selected passes at `logs/20261009-231113-fff52a7a` and
  `logs/20261009-231521-0dac2b09` preceded the final dry-bank correction and do
  not validate that later input. Inspection retained the unchanged combat
  receivers and removed the old straight dry trench after the water bends.
  Attempt patches/hash receipts and exact final inputs remain under
  `logs/rich-background/`; all attempts use the repository's locked .NET
  10.0.401 / Godot .NET 4.7.2 and owned Mesa llvmpipe/Dummy audio execution.

- The first after-CI attempt, `logs/20261009-232043-d92af68f`, was deliberately
  cancelled through its owned runner's registered SIGTERM handler, with cleanup
  awaited (**159.08s**, aggregate cancelled/failed, no exports). Its C# and
  fidelity gates passed, but it is **not** a full pass. Checking the capture
  state receipts exposed different automatically chosen combat seeds in the
  earlier selected comparisons. Cosmetic placement ignores that seed, but the
  comparison contract requires it to match. The existing asset view fixture now
  pins the genuine before seed rather than weakening that requirement.

- The serial retry at `logs/20261009-232533-b9edf291` timed out on the unchanged
  **900s aggregate admission limit** (**933.49s** total). Core/fidelity and four
  source UI slices passed, but combat and the remaining network case were not
  completed; exports were not admitted. It is a failed partial run, not a full
  pass. Repeated route construction and distant triangle-probe work were then
  removed by bounded caching/broad phases without changing any placement or
  assertion. A one-display cached-source attempt at
  `logs/20261009-234826-dc5119d3` was deliberately cancelled early (**133.34s**),
  with cleanup awaited, to select the supported default two-display admission.
  No deadline, case, assertion or coverage requirement was lowered.

### Final matched views and costs

`mise run test-ui --scenario launcher --checkpoint assets` passed in **71.43s**
at `logs/20261009-234713-44e301d0`. Its final immutable images/observations are
under `logs/rich-background/after-final/`, bound by `after-final.sha256`:

| Ordinary view | Before and after file names (in their respective immutable directories) | Inspected visible change |
| --- | --- | --- |
| Settlement overview | `detailed-ground-settlement.png` | Small interleaved clearings/woodland replace the huge uniform eastern ground; more layered groves and understory frame the unchanged nine plots. Both wooded river bends and the separate western trail are visible. |
| Bridge / curved river / subsection close view | `detailed-ground-bank-trail.png` | The unchanged deck meets continuous surface water, which visibly bends south instead of running straight offscreen. The path winds onward between small moss/litter/grass sections and varied-height tree/rock/fern groups, with an open readable route corridor. |
| West forest / settlement close view | `detailed-ground-forest.png` | The western grove has overlapping varied-height canopy, rocks, moss and mushroom fringes; a woodland trail bounds the next clearing. Buildings, plot outlines and picking targets remain readable. |

All six actual PNGs were opened and inspected at their rendered gameplay scale.
The bounded correction pass addressed full-footprint exclusion, the lantern's
ribbon contact, excessive batch granularity and the obsolete dry straight trench;
no missing maps, broken same-height joins or floating new props remained in the
representative inspected views. This is consumer evidence, not approval of every
possible camera point or an untouched source-art reference match.

`logs/rich-background/final-resource-comparison.json` records exact equality of
all three camera/resolution/phase/wave/turn/revision/selection/combat-tick/city
observations. The before and final after authority receipts use combat seed
**877325505053986740**; the entire final serialized authority state differs only
in the fresh session's `MatchId`. The coordinate-only cosmetic map has no seed
input. The image hash receipts and retained input receipts bind the actual
files and source/tool inputs (`cached-final-inputs.sha256` is the final source
receipt); intermediate captures are retained but are not substituted for this
final matched proof.

At the matched overview, both versions render **2,544 floor cells**. Current
terrain has **211 batches / 18 surfaces**, versus **111 / 13** before. The fuller
finite woodland has **729 newly grouped props**, replacing sparse original
scatter inside that envelope: 45 pines, 34 spiral trees,
147 ferns, 148 leaf clumps, 142 moss clumps, 130 boulders and 83 mushrooms. These
use **330 batches**, **165 original shared meshes**, and **15,287 native mesh-part
instances** (parts, not 15,287 semantic props). All actual emitted part transforms
match the intended imported transforms with observed error **0**.

| Matched overview renderer counter | Before | After | Delta |
| --- | ---: | ---: | ---: |
| Draw calls | 9,400 | 9,737 | +337 (+3.6%) |
| Rendered primitives | 552,206 | 799,991 | +247,785 (+44.9%) |
| Texture bytes | 432,940,023 | 432,940,023 | 0 |
| Buffer bytes | 40,320,862 | 41,957,674 | +1,636,812 (+4.1%) |
| Scene nodes | 13,667 | 14,597 | +930 |
| Resources | 12,953 | 12,953 | 0 |

The two close views have draw-call deltas +349 (+4.9%) and +96 (+1.2%), with the
same texture/resource counts and similar +1.64MB buffer cost. These are existing
whole-client software-renderer counters, not isolated native GPU benchmarks or
peak-memory measurements. More geometry is an explicit cost of this fuller
composition; no measured FPS gain or performance qualification is claimed.

The final applicable cheap run (`logs/20261009-234634-c0ef0713`, **37.13s**)
passed **504 core / 586 runner tests**.

**Full final after gate passed:** `mise run ci --jobs 2 --ui-jobs 2`,
`logs/20261009-235042-58b9ad75/ci-summary.json`, **737.52s**. This is the complete
required set under the supported default total/scenery-display admission budget:
locked restore, formatting, strict compilation/import, all **504 core / 586 runner
tests**, unchanged static fidelity/placement/negative regressions, all **six network
cases**, all **five source UI slices**, sequential Linux client/server exports,
headless package smoke and graphical exported-package smoke. Source UI completed
in **651.35s**; the deliberately retained serial timeout above is not reclassified
as passing. Normal expensive-scenario limits remained 900s, with no assertion,
case, admission-owner or cleanup waiver. Those results bind the C# and lock
hashes in `cached-final-inputs.sha256`; subsequent documentation and diagnostic
`Scope` wording corrections are not covered by that run.

The final packed `exported-package-worker/exported-package/packed-layout-1280x720.png`
was also opened and inspected: the richer groves, patchwork floors and winding
river/path are present in the real package while buildings and actual battle
occupants retain the clear approach and supporting centers. This adds no new
scenario or graphical match.

Material-family boundaries and hex bank steps intentionally remain visible; this
is not blended terrain, displaced soil, a new procedural river generator or a
new world map. Native compositor/GPU FPS, peak memory, physical input and listening
quality are not established by these software-rendered diagnostic frames and
renderer counters. Fine undergrowth naturally becomes subtle at fitted overview;
protected combat ground and exterior sparse coverage intentionally stay quieter.
