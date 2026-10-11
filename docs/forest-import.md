# Selected Dreamlike Forest resource import

## Pin and selection

This is resource migration ahead of Dreaming Forest game styling, not a theme,
terrain, renderer/effect/settings redesign or Ledger UI migration. Canonical
source is `https://github.com/seraph1nia/odot-game-assets.git`, actual inspected
revision **`12bc5ef02765c1d4b725013791f7420918aacd64`**. The accepted forest
publication is `cc723f4de0b988c32c9a38178cb9ded580dd28b8`, landed through
https://github.com/seraph1nia/odot-game-assets/pull/6. These are different
revision identities: the later clone includes unrelated UI work. Actual forest
sources/exports/maps/receipts have no diff from that accepted forest head and
were independently hashed, rather than presumed unchanged.

The [manifest](../src/Game/Assets/Authored/manifest.json) remains the canonical
source-to-destination inventory: `sourcePath` is source-repository-relative,
`path` is relative to `src/Game/Assets/Authored/`, and each file has exact SHA-256
and byte length. IDs are paths without `.glb`. The compact
[selection receipt](forest-import-receipt.json) adds accepted receipt hashes,
source `.blend` identities/selectors, old/new game hashes and protected exports.
No editable source, reference image, source tool or preview is distributed.

Only **six of the 22 refined exports** have current game consumers:

| ID (destination adds `.glb` under `src/Game/Assets/Authored/`) | Current consumer | Exact export bytes |
| --- | --- | ---: |
| `environment/components/forest_fern` | Existing deterministic woodland understory | 468,244 |
| `environment/components/forest_leaf_clump` | Existing deterministic woodland understory | 388,060 |
| `environment/components/forest_moss` | Existing deterministic woodland understory | 365,340 |
| `environment/components/forest_mushroom_small` | Existing small mushrooms/clearing and woodland understory | 1,614,404 |
| `environment/components/forest_spiral_tree` | Existing spiral canopy and passive scenery | 1,053,088 |
| `environment/hex_woodland_bridge` | Existing adapted timber bridge composition | 7,896,016 |

The first four derive from `sources/environment/shared_forest_kit.blend`, the
spiral tree from `sources/environment/shared_enchanted_kit.blend`, and the bridge
from `sources/environment/hex_woodland_bridge.blend`; exact source hashes and
collection selectors are in the receipt. Their GLBs already embed the required
component/material closure. There is no need to copy separate unused library
models. The three focal tile exports are **not used by this game**, so neither
them nor the six intentionally moved peripheral leaf transforms are imported.
All **51 other bundled exports** match the new pin byte-for-byte without
recopying them, including detailed floors, path/river overlays, buildings and rigs.

## Map and texture representation

Selected GLBs remain byte-identical to source. Their **84 embedded PNGs** are
extracted unchanged using the existing `gltf/embedded_image_handling=1`
representation. The manifest's `derived` entries bind every destination to its
parent GLB/image name and exact embedded bytes. No separate original scalar
roughness PNG is required: the GLB carries its portable packed roughness in
**G**, and that exact image is extracted. Original source scalar maps were
verified against accepted resource/channel receipts, not substituted for the
packed consumer image. BaseColor/normal/emission pixels, factors, sampler/UV
bindings and source provenance remain intact. All 36 refreshed embedded-map
import sidecars explicitly generate mipmaps, preserving the selected GLBs'
authored `LINEAR_MIPMAP_LINEAR` minification through headless imports and exports.

The whole existing inventory is now **57 GLBs / 327 derived PNGs**, plus four
standalone textures. The map refresh introduces 36 image paths, removes 31
obsolete extraction paths and their import sidecars, and leaves all surviving
image-path bytes unchanged. Existing GLB import UIDs are retained; Godot supplies
new map UID sidecars. Growth is **224,696 GLB bytes + 223,186 derived-map bytes**.
These are storage counts, not GPU-memory, batching or performance measurements.

Four texture-only resources are supplied for subsequent styling, with independent
`textures` manifest records rather than fictitious embedded-map parents:

| Canonical source export | Game destination (under `src/Game/Assets/Authored/`) |
| --- | --- |
| `exports/environment/decals/forest_decal_rune_circle.png` | `environment/decals/forest_decal_rune_circle.png` |
| `exports/environment/decals/forest_decal_moss_wash.png` | `environment/decals/forest_decal_moss_wash.png` |
| `exports/environment/decals/forest_decal_fairy_ring.png` | `environment/decals/forest_decal_fairy_ring.png` |
| `exports/environment/decals/forest_decal_spore_growth.png` | `environment/decals/forest_decal_spore_growth.png` |

They total **97,676 bytes**: four source/export-identical 512×512 8-bit RGBA PNGs,
straight sRGB RGB and linear coverage alpha, transparent borders and peak alpha
≤0.42. Imports are lossless, mipmapped, not premultiplied, with transparent-border
RGB rewriting disabled. Alpha is not emission, roughness or wind. No native Decal,
projector, receiver filter or runtime placement is added. Source PNG top is +Y
(right +X, normal +Z); through the existing glTF conversion it is −Z (normal +Y).
Suggested source-unit footprints are rune 1.2–2.0, moss 0.5–1.2, fairy 0.8–1.6,
spore 0.3–0.7; future placement must independently qualify bounded receiving depth.

## Effective zero emission correction

Texture existence does not establish actual emission. Source caps legally retain
an emissive texture with omitted emissiveFactor, which defaults to **[0,0,0]**.
The locked Godot `ed1daf0bf` glTF importer instead sets emission white whenever an
emissive texture exists without an explicit factor. Actual imported `mushroom_blue`
was enabled, white, energy1, Multiply, with the original unused texture. This was
not a mistaken getter interpretation: one authorized native matched swatch fixture
rendered it visibly blue in a black/no-light/no-glow environment, while changing
only that material's emission color to black produced exactly black sampled pixels.
Original and unchanged duplicate positive gills remained visibly emissive. This
isolated fixture is diagnostic history, not game acceptance or a replacement export.

The supported [post-import hook](../src/Game/Import/ZeroEmission.gd) is attached
**only to the small-mushroom and woodland-bridge GLBs**. It reads their immutable
GLB JSON, resolves used material indices and unique exact source names, and sets
only the original shared material's emission color to black when the authoritative
factor is zero and an emissive texture exists. Ambiguous/missing bindings fail.
It does not guess from a cap-name allowlist, replace/duplicate resources, disable
all emission, delete unused textures, alter any other channel or edit source bytes.
Positive gill/crystal/rune/window/lantern factors remain untouched. The hook runs
before scene serialization, so exported `.scn` resources carry the same correction
without runtime resource mutation or access to the source repository.

The existing headless asset proof now decodes the four imported texture resources,
checks dimensions/real coverage/borders, and inspects actual imported leaf normals,
G-channel roughness, nonemitting bodies and both black/lit gill-mask pixels. It
truthfully checks absent normals on moss/caps/gills. The mipmap regression guard
checks actual loaded image mip availability and records mip counts for all bound
albedo/roughness/normal/emission/metallic/AO textures across the six selected scenes,
including ground materials and retained unused emission textures, plus the four
decals. This extends the existing tier-B resource proof because tier-A container
checks cannot observe Godot's imported texture mip levels; it adds no process or
session and only bounded image reads to the existing component check.
Source and existing client
packages use the same proof:

```sh
mise run test                        # pure C# and immutable container/inventory checks
mise run test-assets                 # owned headless component/import/geometry proof
mise run export-client               # sequential headless editor export; no game journey
mise run export-server               # sequential headless editor export; no game journey
mise run test-assets --exported      # existing client only; no prepare/re-export/UI/session
```

The exported option requires an existing Linux client and does not implicitly
rebuild it or start server/client gameplay. It retains the same one ordered child,
owned runtime, 60s headless proof bound, error checks, rendering-buffer fidelity,
ground/bridge checks and cleanup. Missing prerequisites are unexecuted/nonzero.
No test scheduling, full-CI policy, graphical input/assertion or deadline overhaul
is part of this import.

## Protected contracts and handoff

Actual selected old/new GLB numerical POSITION/NORMAL/UV/other attributes, indices,
primitive material slots and full node/scene identities compare exactly. No mesh,
base/contact/collision geometry, floor/river/bridge/trail route, fixed plot, path,
picking, rig/clip/action/socket, camera, deterministic ECS/network/save or gameplay
input changes. No new wind channel was delivered or invented. Existing UVs and
leaf-material eligibility remain; current renderer/weather/quality/comfort behavior
and continuous woodland/river/bridge/trails are preserved. Game styling, LUTs,
accent lights, fauna/effects, shaders and UI-package migration remain separate.

## Evidence and current policy boundary

The receipts in this section are historical evidence from before rebasing onto
`c2775a3a977513bc04e3b5c1541997e07c2fce83` and before the mipmap review fix.
They do not qualify the current source, imported resources or packages.
Fresh affected source/import/export/package bindings and broader headless checks
belong to the outer validation phases; old bindings must not be reused for changed
inputs.

Before-import full CI passed at `logs/20261011-071721-071b3a1e/ci-summary.json`,
1111.12s runner / 1117.72s command, unchanged declared admission and complete
source/network/UI/sequential-export/package coverage. The pre-policy native source
eight-view/control/bootstrap check passed at `logs/20261011-075256-980437fc`,
30.17s owned / 36.48s command: actual Forward+/Vulkan1.4.354, RX6800/RADV NAVI21,
DiscreteGpu, X11, owned Windowed PID/native handle and isolated preferences.
All eight game-only PNGs were inspected: shaded blue caps with localized bright
gill/spore detail, cool leaves/rigid trunks, continuous river/deck/path, readable
HUD and combat bodies/health bars; weather remains restrained. RGB8→RGBA8 import
warnings are retained, not engine errors. The swatch fixture is separately bound
under `logs/dreamlike-import/emission-control/` and `emission-diagnosis.json`.

**Captain subsequently revoked game-starting graphical tests.** No further GUI,
native scenic/package or full graphical CI runs are authorized. The in-flight
after-CI `logs/20261011-075429-789529f0` was gracefully cancelled through its exact
owned supervisor, exit130 / 81.82s runner. All recorded owned game/runner/Openbox/
Xvfb PIDs and runtime roots were confirmed gone; those displays' windows were
therefore gone. `logs/dreamlike-import/captain-stop-cleanup.json` records cleanup.
That run is cancelled/partial, not an after-CI pass. Earlier native evidence is
historical and does not certify final headless-only input changes or exports.
Pre-rebase headless results on the narrowed component-only implementation
(before mipmap restoration):

| Check | Result / retained evidence |
| --- | --- |
| Pure C# | 504 core + 645 runner passed; two native-Windows cases unexecuted on Linux; 45.55s, `logs/20261011-080014-33f04e79` |
| Source headless import/component/geometry proof | Passed 12.33s, `logs/20261011-080101-784e20db` |
| Fresh Linux client export | Passed 11.44s, `logs/20261011-080122-16074672` |
| Existing fresh client headless component proof | Passed 6.28s, `logs/20261011-080143-1e6fdb9b` |
| Fresh Linux server export | Passed 12.51s, `logs/20261011-080158-4ccb6c26` |

The historical source and real exported-client fidelity JSON both witness nonemitting caps
with retained unused textures, positive localized gills, correct PBR presence /
absence, decoded transparent PNG coverage, and existing geometry/ground checks.
Historical source→destination hashes matched all 57 GLBs, 327 embedded extractions
and four standalone PNGs in `logs/dreamlike-import/final-source-destination.json`.
Historical executable/PCK/DLL and relevant source/import bindings are retained
separately in `logs/dreamlike-import/final-artifact-bindings.json`; despite their
`final` filenames, these are not current-head receipts. Formatting and whitespace
checks passed on those earlier inputs. No extra graphical view or whole-game
session ran after revocation. These focused checks do not establish current CI
or waive publication gates.

The landed [required workflow](../.github/workflows/ci.yml) now selects
`ci-source`, `ci-linux-package` and `ci-windows`, all headless, as described in
[verification](verification.md). The earlier publication-policy conflict is
resolved by that landed testing overhaul, not by this resource import.
Current source/package checks and forge publication results remain the outer
pipeline's responsibility.

Other retained failures: stale generated UIDs after renamed maps required moving
owned `.godot` aside (preserved, not deleted); the first channel fixture incorrectly
required normals on moss/caps/gills and was corrected against actual source data;
the source cap-emission mismatch then failed before the supported correction.
An intermediate graphical-observation proof field exposed default JsonElement
serialization failures in six cheap fixtures; that superseded UI integration was
removed when coverage moved entirely to the existing headless source/package owner.
None of these failures is silently relabeled. No new runner crash occurred.

Software/headless proof is not native aesthetic/performance/compositor/input/audio
qualification. Final exported graphical views and complete graphical CI are
**unexecuted under current policy**, not waived by passing headless checks.
Ordinary required CI is headless and makes no graphical qualification claim;
this import must not invoke old graphical CI or skip no-mistakes gates.
