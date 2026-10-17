# AI-generated textures — corrected game POC

Open **[the fresh three-stage gallery](correction/review.html)** offline: original,
rejected initial POC, and corrected actual-game captures, including normal-scale
overviews, close views and native Archery crops.

The current immutable asset pin is **`60ce86f04c02419d9c09d30c2e25931345bf2bf3`**,
landed in [asset PR 15](https://github.com/seraph1nia/odot-game-assets/pull/15),
whose parent is the initial POC `2d88d355393f145aedfc19fa198960dac741c6fb`.
Forge main and the supplied read-only repository's `origin/main` were independently
resolved to that exact commit during this review. No feature-branch pin, source
rebuild, export-byte edit, terrain shader or gameplay change is used.

## Correction and acceptance status

The initial texture stage had a legitimate appearance regression: a conspicuous
dark hex grid and large repeating stones/flowers. Its successful import and GPU
execution did **not** establish artistic acceptance. No exception or waiver is
being applied. Fresh actual-game comparisons and strengthened picking verification
have now executed on the authorized RX6800 route. The implementation-worker's
pixel inspection finds that the correction removes the added enclosing dark grid
and reduces the dominant whole-tile landmarks; this is a bounded visual judgment,
not an automated artistic pass or production certification.

The bounded upstream correction changes only five ground Base Color bindings:
moss, grass, dirt, leaf litter and pine duff. Smaller overlapping rotated/flipped
paint motifs replace whole-tile landmarks; source-family palette means and weak,
luminance-matched edge hue correction replace the constant dark perimeter.
Foundation, four path ribbons, four river ribbons and Archery remain byte-identical
to the initial delivery. Geometry, UVs, material identities, dimensions (radius
2.55, top Z=0, bottom Z=-.36), animation, connectors and rendering contracts survive.

- [Correction handoff](correction-handoff.json), content identity
  `62ba705d9dd5e26d4ed6fd0b0c7e64337fc064629148420737f55a5e9057c8a7`.
- [Correction import receipt](correction-import-receipt.json): all 57 GLB hashes
  before/after; exactly five change. The manifest and `AssetCatalog.Revision`
  advance together; all 569 indexed derived records bind exact embedded PNGs.
  Only five superseded color maps/sidecars are removed. Surviving sidecars/UIDs,
  mipmaps, standalone textures and the separate Worldtree/UI pin are preserved.
- [Upstream correction diagnosis, matched triples and preservation reports](https://github.com/seraph1nia/odot-game-assets/tree/60ce86f04c02419d9c09d30c2e25931345bf2bf3/docs/ai-textures/seams).
  These are source-stage evidence, **not actual-game acceptance**.

Pine input still depicts broad leaves, not convincing pine needles. Low native
input resolution (~135–171px), close-view softness, quiet/detailed path connector
bands, the thin foundation lip and modest close-only Archery improvement remain
limitations. A fixed map still repeats: only actual normal-scale and close game
views can establish whether the conspicuous regression is sufficiently reduced.
No sustained frame-time, software-renderer repair or production-art claim follows
from resource assignment or snapshots.

### Direct actual-game inspection

| View | Original → initial → corrected observation |
| --- | --- |
| Normal fitted settlement | Sparse repeating cushion stamps → conspicuous dark grid/large flowers → finer painted field with no added enclosing same-family line. HUD outlines, labels, blue factions and bars remain readable. The corrected floor is richer than original without the initial whole-tile emphasis. |
| Forest / mixed neighbors | Large pale rocks, daisies and leaves no longer occupy identical whole-cell compositions. Smaller painted marks remain visible; close views are softer. Same-family neighbors read as a field rather than outlined tiles. Cross-family brown/green boundaries remain angular and visible, as on the original; this is not neighbor-aware blending. |
| Dirt / building plots | Smaller pebbles retain earth detail beneath the original pale plot outlines. Repeated dirt is much less dominated by large identical stones. |
| Path / connected river | Ground no longer adds the initial dark grid beside the exact unchanged ribbons. Path quiet/detailed bands remain visible close up. Blue water/banks, bends, bridge and endpoints remain coherent; native animation is separately proven by the component. |
| Leaf litter / pine duff | Smaller litter strokes and a darker pine-family mean reduce whole-swatch emphasis. Pine still does not convincingly depict needles. |
| Archery / foundation | Exact initial POC bytes remain. Close Archery grain/stone/blue-cloth detail is modest; overview difference remains negligible. Foundation remains a thin dark side lip, not a demonstrated normal-scale visual upgrade. |

Normal menu and settlement overviews and permitted close views are retained.
Native pixels were inspected directly; no browser review is claimed. Ordinary
height-step sidewalls, plot outlines and cross-family borders were not mistaken
for the removed baked dark same-family perimeter. Fixed-map repetition can still
be found on careful close inspection; the conspicuous initial regression is not
observed at normal scale in these fresh captures.

## Historical evidence — preserved, not corrected passes

[Initial report](initial-review.md) preserves the original implementation-worker
observations and execution receipts. Its unqualified acceptance language is
superseded by this report and the R1 appearance finding.
[Historical gallery](review.html), `before/`, `after/`, `settlement-pair.png`,
[evidence.json](evidence.json), [checks.json](checks.json),
[asset-handoff.json](asset-handoff.json) and [import-receipt.json](import-receipt.json)
remain original/initial POC artifacts, not photographs or checks of the correction.
Their source heads, DLL/manifest/image hashes and GPU identities are unchanged.
The old post-capture picking assertion could accept an unchanged selection and is
not proof of a successful input-driven transition.

## Focused review-round verification

The existing owned-window `TextureWitness` now establishes plot 0 before selecting
plot 2 for paid Archery construction. After restoring the overview it proves the
transition 2 → 0, sends a missed world click, confirms it cannot satisfy selection
of plot 2, then proves 0 → 2 with a genuine plot click. `texture-picking.json`
records the observed sequence. No gameplay selection setter, modal bypass,
additional journey, timeout relaxation or CI GPU dependency is introduced.

This strengthens existing tier E coverage because unit/resource checks cannot
prove graphical ray picking after photo framing. It reuses seed 1, normal solo
construction, ordered Child events, isolated per-client preferences and awaited
cleanup. Expected incremental cost is a few probes/clicks in the same window;
actual execution is recorded below.

[Fresh evidence](correction/evidence.json) binds all 33 retained viewport PNGs to
asset/manifest/DLL/image hashes, complete actual graphics/camera fields, projected
ground-camera footprint and owned PID/native-window identity. All three stages
match those settings exactly: 1100×820 Windowed X11, RX6800/RADV discrete Vulkan
1.4.354 Forward+, seed 1, High/Woodland/4×MSAA, exposure 1, fair weather and cosmetic
time 12. Actual native window IDs can be reused after the prior owned process
exits; each frame authenticates its own PID. Ambient animation/particles remain
real-time, not identical-pose or pixel-equality targets.

These photograph the **uncommitted review-fixed working tree based on
`c8800772ee8bc3a055ccdee848d98301a0510d1a`**, not that committed head alone or a
future pipeline head. Per-stage C#/project source hashes, patch hashes and actual
built DLL hashes are retained. Only coherent assets/catalog revision were switched
for comparison; corrected inputs and their generated cache were restored afterward.
Historical pixels remain untouched. Every fresh run observes picking **2 → 0 → 0
(miss) → 2**, normal paid construction, return and clean owned Exit.

[Review checks](correction/checks.json) retain exact summaries and log hashes:

| Scope | Actual result / run |
| --- | --- |
| Corrected targeted byte/placement admission | 43 passed, zero skips; `20261017-021641-7ae4cfe0` |
| Corrected imported geometry / corruption | Passed 5.48s component; `20261017-021645-b997258e` |
| Corrected actual production animation | Passed 5.23s component; `20261017-021708-bd004d7a` |
| Corrected owned GPU photos / picking / normal solo | Passed 26.89s graphical scope; `20261017-021800-9709682d` |
| Fresh original geometry / animation / GPU | Passed; `20261017-022105-be89330a`, `20261017-022131-54c16105`, `20261017-022142-b2e7c2dd` (GPU 26.71s) |
| Fresh initial geometry / animation / GPU | Passed; `20261017-022216-236e57d3`, `20261017-022242-cd996a5c`, `20261017-022253-7dfc9a3c` (GPU 26.97s); successful execution does not waive its rejected appearance |
| Formatting | Locked restore, then `dotnet format Odot.slnx --no-restore --include src/Game/AssetCatalog.cs tools/DevRunner/TextureWitness.cs`; passed |
| Provenance / packaging | Import dry-run reports zero changes; all fourteen immutable source/export hashes match successor handoff. Exact three-stage camera/graphics/footprint matching precedes packaging. |

Each geometry proof retains **42 orientations, 72 full-profile neighbor joins,
40 base/overlay stacks**, five ground bounds/foundation, path/river straight,
turn60, turn120 and end shapes, UVs/normals/materials, banks/width/elevation and
bridge adaptation. Existing negative corruption cases remain strict. The minimal
animation component tests actual imported production players/target transforms,
automatic water advancement and loop closure, not just changed PNGs.

**Retained failure:** the first fresh original headless static probe
`20261017-021929-fbba4b40` emitted its fidelity report but exited **134** during
shutdown, with no emitted engine error or failed assertion explaining it. The
owned child was awaited and its generated cache preserved. One explicit identical
source/cold-import repeat passed; no code/assertion/deadline/backend was changed.
This does not establish the cause or repair the pre-existing headless/software
instability. The corrected pin's targeted/component/GPU runs passed first time.
No missing-map or shader error was observed in the successful scopes. Existing
RGBFloat → RGBAFloat curve-texture warnings remain documented, not suppressed.

### Resource observations

Relative to initial POC, the five GLBs grow **9,822,492 → 11,848,892 bytes**
(+2,026,400); all 57 GLBs grow **192,885,536 → 194,911,936 bytes**. Derived PNG
bytes grow **64,920,488 → 66,946,880** (+2,026,392). These are more detailed PNGs
compressing less, not additional decoded images: 569 map records and upstream
decoded RGBA8 estimates are unchanged from initial delivery (no mips/driver
allocation included). GLB-embedded and extracted PNG bytes are not two runtime
texture allocations.

| One observed solo frame | Original | Initial | Corrected |
| --- | ---: | ---: | ---: |
| Draw calls | 3,548 | 3,548 | 3,548 |
| Primitives | 806,558 | 806,558 | 806,558 |
| Texture bytes | 1,126,842,432 | 1,129,726,016 | 1,129,726,016 |
| Buffer bytes | 122,789,140 | 122,789,140 | 122,789,140 |
| Nodes / resources | 10,939 / 4,615 | 10,939 / 4,617 | 10,939 / 4,617 |

These include cold resource work, readback/PNG encoding and debug traversal.
Instantaneous FPS/process counters are not controlled steady-state measurements;
viewport GPU timing is unavailable. No frame-time, physical-VRAM or speedup claim
is made. Full repository tests/lint, packages, CI, other GPUs/platforms, sustained
performance, controller/audio and Steam multi-machine qualification remain outside
this review round and belong to their separate phases.

Use the locked installed .NET SDK 10.0.401 and Godot .NET 4.7.2, locked restores,
and existing tools only. Focused admission/geometry/animation checks precede any
GPU subprocess; full test/lint/publication phases belong to the outer pipeline.

```sh
mise run test-runner --filter 'FullyQualifiedName~AuthoredAssetTests|FullyQualifiedName~DetailedGroundTests|FullyQualifiedName~LandscapeChecksTests'
mise run test-assets --startup-timeout-ms 60000
mise run test-engine --scenario animation --startup-timeout-ms 60000
# Only with the explicitly authorized RX6800/X11 surface and readable auth path:
env -u WAYLAND_DISPLAY DISPLAY=:0 XAUTHORITY=/run/user/1000/xauth_qKdKuN \
  xdpyinfo -queryExtensions
env -u WAYLAND_DISPLAY DISPLAY=:0 XAUTHORITY=/run/user/1000/xauth_qKdKuN \
  ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-vulkan --desktop \
  --checkpoint textures --startup-timeout-ms 60000
```

Do not inspect/copy authentication-file contents, change display configuration,
use another backend or capture unrelated desktop windows. Missing prerequisites
are unexecuted, not passes. Run `mise run play` for the normal interactive preview
when those prerequisites are available; Singleplayer → Start → Archery Range.

## Reproduce the pin and roll back coherently

`python3 tools/Assets/import_ai_textures.py <read-only-assets-repo>` defaults to
dry-run. Verify forge main first; `--apply` imports only the known landed successor
from immutable Git objects, checks handoff identity and the entire current
inventory, and refuses unknown pins/changed bytes. It admits the original,
initial and corrected known pins, never a private branch. Update the catalog
revision with the manifest; preserve surviving sidecars and mipmaps. The importer
never opens or writes `.blend` sources.

For a warm pin switch, preserve the owned generated `src/Game/.godot` beneath
ignored worktree-local `logs/` before cold preparation, avoiding stale removed-map
UID dependencies. Do not change developer preferences or delete authored sidecars.

A coherent revert of the correction returns catalog, full manifest, the five
GLBs, five derived color maps and sidecars together from
**`60ce86f04c02419d9c09d30c2e25931345bf2bf3` →
`2d88d355393f145aedfc19fa198960dac741c6fb`**. Reverting the original POC import
as well returns the fourteen GLBs/dependencies to
**`133c7cdeb368cada408d1c95c9414e08488371a9`**. Preserve later edits and historical
captures; never swap a binary under a stale manifest/hash or force-reset later
work. Re-run focused hash/import checks after rollback. No upstream rewrite,
release, deployment, default-branch push or automatic publishing is involved.
