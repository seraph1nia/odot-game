# AI-generated textures — historical initial game POC

This report preserves the initial implementation observations and receipts, not
current acceptance or usage guidance. The initial appearance was rejected, and
its post-photo picking assertion could pass without a selection transition. See
[the current correction report](README.md) for the superseding visual judgment,
strengthened picking evidence, reproduction, preview and rollback instructions.
The original observations below are retained as history, not corrected passes.

Open **[review.html](review.html)** directly, offline. It retains original/new
Godot viewport images, an actual-size settlement overview, permitted 3× views,
native-pixel Archery crops and a downloadable side-by-side overview PNG.
The gallery uses the existing asset review's olive/parchment palette and serif
hierarchy, not a new catalog service. No browser bridge inspection is claimed.

## Result and artistic limits

**Implemented; local RX 6800 / RADV Forward+ Vulkan acceptance passed.** The
painted material stage is now used by the real menu and solo settlement, not
only by a Blender preview. Source geometry, material identities, transforms,
connectors, native water animation and the horizontal board/Roguelite rules are
unchanged. This is a visible surface-detail improvement at near/medium range,
**not an unqualified artistic improvement at every distance or production-ready
texture certification**.

Implementation-worker inspection of both actual GPU capture sets:

| View / material | Observed benefit and remaining issue |
| --- | --- |
| Fitted actual settlement overview | Earth, leaves and water read more richly than the sparse original stamps. Unit health bars, construction outlines, building labels and faction blue remain legible. The repeated hex perimeter is **more visible**, not less; the original is quieter. |
| Moss / grass | Painted moss/rock and grass/flower detail is clearly visible near the forest and bridge. Large daisies repeat in the same orientation. Illustrated stones are flat color, not real relief. |
| Dirt | Pebbles read clearly under the nine building plots. Considerably busier and warmer; pale plot outlines still separate the gameplay controls. Identical pebble patterns are obvious on repeated cells. |
| Leaf litter / pine duff | Richer litter than before, but pine duff is insufficiently distinct from leaf litter. This is a known input-art limitation, not a claimed pine-specific improvement. |
| Mixed neighboring hexes | No square atlas boundaries were observed. The source-baked .18m shared-color edge fade survives, reducing sharp fragments, but a dark hex grid and repeated whole-tile patches remain conspicuous. It does **not** solve cross-family blending completely. |
| Foundation | The thin side lip remains at the same height and depth. Its new masonry is mostly too small/dark to judge from the game camera. Do not infer a visible game-view improvement from successful assignment; the upstream matched sidewall close-up is the useful material evidence. |
| Path | Connected bends/endcaps remain above the floor and readable. Interior detail is richer, but the deliberately quiet connector regions produce visible alternating soft/detailed bands. Original rounded ends and bank-side timber remain unchanged. |
| River / bridge | Blue water, narrow banks and curved joins remain coherent, including beneath the original bridge. New static color streaks are visible. Native shimmer still plays; this is not a new fluid simulation or a claim that painted highlights themselves flow. |
| Archery | Actual paid construction loads and is pickable. Wood/stone/blue-cloth variation is visible in the native-pixel close crop without duplicated shingles or a new target decal. At fitted overview the difference is negligible; preserved overgrowth dominates. It is acceptable for this restrained POC, not a dramatic building upgrade. |

The ground swatches were only ~135–171 native pixels before enlargement. Softness,
baked painted highlights and repeated density are retained defects, not missing
texture imports. The existing asset-stage blending already narrows the ground
edge and preserves quiet ribbon connectors/banks. No extra game shader, neighbor
state, UV remapping, mesh generation or terrain redesign is justified by these
captures. Further art refinement should improve the source maps and land on
asset main first, not silently mutate distributed exports in the game.

## Exact provenance and scope

Asset main was independently resolved through the forge to
**`2d88d355393f145aedfc19fa198960dac741c6fb`**, with parent
`133c7cdeb368cada408d1c95c9414e08488371a9`. Local `origin/main` agrees; the asset
feature result `102de2042734a4bffeff54642e483f249de0cd70` is its ancestor.
The asset delivery is https://github.com/seraph1nia/odot-game-assets/pull/14.
No guessed merge SHA or private-branch export is imported.

- Previous pin: **`133c7cdeb368cada408d1c95c9414e08488371a9`**.
- New pin: **`2d88d355393f145aedfc19fa198960dac741c6fb`**.
- [Original handoff](asset-handoff.json) content identity:
  `702a679862bb25a1e4f7152b2ea97190ba1c3b14b467398dbaa71af725011635`.
- [Import receipt](import-receipt.json): exact hashes/bytes of all 57 selected
  GLBs before and after; exactly 14 change (five bases, four paths, four rivers,
  Archery). All other 43 GLBs, four standalone forest decals, Worldtree/UI pin,
  permission text and license notices are unchanged.
- `src/Game/Assets/Authored/manifest.json` and `AssetCatalog.Revision` advance
  together. Extracted PNGs are exact embedded bytes, with indexed duplicate-name
  handling; 567 → **569** derived records. Superseded extractions/sidecars are
  removed, surviving source/import UIDs retained, new maps use mipmaps.
- No `.blend`, input ZIP, editable source, unused asset or upstream catalog
  payload is distributed. Original roughness/normal/rendering channels remain;
  only source-authored Base Color bindings change. Archery's previously unused
  blue-cloth UVs were already authored upstream; their first portable export is
  explicitly audited in the asset preservation report, not a game-side unwrap.

The authoritative upstream reports and all 37 matched GLB studio comparisons
remain [at the immutable asset revision](https://github.com/seraph1nia/odot-game-assets/tree/2d88d355393f145aedfc19fa198960dac741c6fb/docs/ai-textures).
They prove source preservation, not game pixels. The new gallery adds real-game
acceptance rather than relabeling those studio images.

## Capture identity and fairness

[Evidence](evidence.json) binds every retained PNG to its SHA-256, asset revision,
manifest SHA-256, actual built game DLL SHA-256 and observed graphics/camera/
resource state. Baseline game head **`cf339cc0eba000de7c28681d7b91cbf0d711a34b`**;
new-assets capture head **`65926ac6b5b2247b5f5894e2a65eea9fcd83898b`**.
Subsequent documentation/gallery commits do not retrospectively turn these into
captures of a later pipeline head. Baseline was imported and photographed before
the pin changed and is preserved here, not re-created with new maps.

Both runs authenticate the owned PID/native window, X11, Windowed 1100×820,
**AMD Radeon RX 6800 (RADV NAVI21)** / discrete GPU / Vulkan API **1.4.354** /
Forward+ RenderingDevice. Process-local DISPLAY/authentication-path handoff was
reverified; no credential contents, other windows or desktop pixels were read.
No display service, ACL, driver, global preference or CI configuration changed.

The same witness, seed 1, fair-weather preset, fixed cosmetic time 12, exposure 1,
High quality, Woodland grading, 4× MSAA, lighting and framing are used. Packaging
rejects differences in actual camera fields, projected ground-camera footprint,
viewport dimensions or complete observed graphics settings. Menu photo views
hide only the central menu controls and use size 12 at the original camera angle;
the actual installed landscape is reused, not a fabricated fixture. Solo uses
normal menu → Lobby → Start → paid Archery construction. The actual overview
retains the current horizontal arena and HUD; close views use permitted 3× zoom.

Ambient players/particles retain real elapsed-time playback, so corresponding
images do not promise identical moving poses or pixel equality. Successive river
frames show later process frames; other effects also move. The engine component
separately witnesses automatic animation advancement, all four actual river
players/targets, fixed non-target transforms and exact loop closure. Numerical
water-motion evidence is not inferred solely from changing pixels.

## Executed checks (headless first, GPU second)

All logs are retained locally under ignored `logs/`; exact run IDs are below.
These are scoped receipts, not a claim that every deep/platform check ran.

| Check | Actual result / evidence |
| --- | --- |
| Baseline cheap provenance/placement selection | 42 passed; `20261017-002135-b40373de` |
| Adapter selection + existing byte/placement checks | 43 passed each; `20261017-002325-4f8e4aac` (baseline adapter), `20261017-002838-c8c13acc` (new pin) |
| Baseline imported-buffer/ground proof | Passed 5.50s component; `20261017-002336-92332f7a` |
| Baseline actual GPU photo/paid-build/picking/return/Exit | Passed 25.67s graphical scope; `20261017-002635-78edd545` |
| New imported-buffer/ground proof | Passed 5.39s component; `20261017-002954-9d418d79` |
| New production animation component | Passed 5.24s component; `20261017-003006-f2dc21cc` |
| New actual GPU photo/paid-build/picking/return/Exit | Passed 25.65s graphical scope; `20261017-003036-0e315b3d` |
| Broad fast C# | **833 core + 664 runner passed**, **2 runner platform skips**; `20261017-003224-29c950d2`; 70.72s, excludes `Tier=Deep` |
| C# formatting | `dotnet format Odot.slnx --no-restore`, then `--verify-no-changes`, passed |
| Import re-admission | Scoped importer dry-run, apply, then unchanged second dry-run; exact 14 matches and all unrelated imports checked against immutable Git objects |
| Gallery consistency | Executable packaging checks actual capture matching, renderer/ownership, input manifest/head identity; static links/image hashes checked separately |

The existing buffer diagnostic covers **42 pair/end orientations, 72 full-profile
joins, 40 base/overlay stacks**, native roots versus query buffers, all five floor
bounds (radius 2.55, top 0, bottom −.36), both ribbon families and bridge adaptation.
It retains normal opaque depth testing and negative material/UV/normal corruption
regressions. River U-across/V-along mapping, original U≈.09375–.90625 water region,
width/banks/elevation and eight-triangle moving shimmer are preserved upstream
and exercised through the real game importer. The animation component checks
actual production clips/material bindings, automatic advancement and loop wrap,
including Archery. The existing headless horizontal-board component remains CI's
owner of the current divider transforms; no layout or rule assertion was weakened.

Live menu captures show neighboring mixed floors, winding paths, connected river
bends and path endpoints. Solo shows the current lowered curved divider and its
round end beneath the existing bridge. **Not every orientation is visible in a
single live composition**; exhaustive connector classes/rotations are the imported
geometry proof and upstream 37-pair gallery, not fabricated live-map views.

### Retained failures, not hidden retries

- First baseline graphical attempt (`20261017-002359-6fdd5bff`) captured all views
  but failed its verification driver's Space-reset assumption after diagnostic
  camera framing. The user reset correctly targets the current camera base.
  The driver now restores the original fitted photo view explicitly before
  ordinary picking. No product input behavior/deadline was changed. One corrected
  baseline run passed; the failed evidence remains retained.
- First post-pin warm prepare (`20261017-002841-57823c96`) exited zero but reported
  missing UIDs for removed maps through old generated imported scenes; it is a
  **failed** import, not a pass. Moving only editor/UID caches had been insufficient.
  The entire owned generated `.godot` tree was preserved under
  `logs/ai-textures-obsolete-import-cache/after-first-import-godot`, then a cold
  supported `mise run prepare` passed. Authored source/UID sidecars were not reset.
  Fresh imports and the subsequent component/GPU runs passed without exclusions.
- Existing RGBFloat → RGBAFloat curve-texture warnings occur in both GPU runs.
  No missing maps, shader compilation errors or engine errors were observed in
  the accepted scopes. The historical llvmpipe instability is **unresolved**;
  hardware success is not evidence of a software-renderer fix.

## Resource / performance observations

Exact remeasurement agrees with the asset-stage export estimate: the fourteen
GLBs grow **27,115,592 → 30,266,832 bytes** (+3,151,240, about 3.01 MiB). All 57
selected GLBs total **189,734,296 → 192,885,536 bytes**. Extracted PNG distribution
bytes grow **61,775,117 → 64,920,488** (+3,145,371); these are the same maps also
embedded in GLBs, not twice the runtime texture allocation. Upstream summed
embedded RGBA8 estimates are **149,946,368 → 152,043,520 bytes** (+2 MiB), excluding
mips/compression/deduplication/driver overhead.

The paired actual solo-overview observations report:

| Native rendering monitor | Before | After |
| --- | ---: | ---: |
| Draw calls (one observed frame) | 3,548 | 3,548 |
| Primitive count | 806,558 | 806,558 |
| Buffer bytes | 122,789,140 | 122,789,140 |
| Texture bytes | 1,126,842,432 | 1,129,726,016 |
| Nodes / resources | 10,939 / 4,615 | 10,939 / 4,617 |

The observed texture-counter delta is **2,883,584 bytes (2.75 MiB)** in this owned
process, consistent with two additional decoded maps and mips; this is not total
physical VRAM accounting. Menu-only texture bytes are unchanged at 683,892,800 in
the paired mixed view, before Archery loads. The snapshots include cold resource
work, readback/PNG encoding and expensive debug observation traversal. Their
instantaneous FPS (solo 16 versus 15) and process-time values **are not a controlled
steady-state benchmark**. Viewport GPU timing is zero/unavailable. No speedup,
no-regression frame-time claim, 60-fps guarantee or cross-GPU conclusion is made.

## Reproduction and rollback

Use [the current report](README.md#reproduce-the-pin-and-roll-back-coherently)
for coherent import and rollback instructions and its preceding section for
verification and preview commands. The importer now targets the corrected pin,
not this report's initial POC. Preserve the historical gallery and receipts;
never regenerate over them. `tools/Assets/texture_review.py` packages the historical
two-stage format only; use a separate worktree-local output directory if repackaging
retained initial captures.

## Remaining qualification

No release/publication/deployment, fresh package export, sustained GPU benchmark,
Windows/macOS/other GPU visual acceptance, controller, listening-quality/audio,
Steam friend invitations or multi-machine test was performed here. The POC did
not change those boundaries. Hosted no-mistakes/PR CI remains GPU-independent and
must validate the exact eventual published head separately. The committed local
capture receipt is not a claim that a later review/pipeline head was photographed.
