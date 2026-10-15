# Vivid overgrown exports and in-game ambient motion

## Immutable input and selection

The game consumes `odot-game-assets` main revision
**`133c7cdeb368cada408d1c95c9414e08488371a9`**, the landed runtime-optimized
successor to vivid/ambient revision `9a69b514e9fd7b101bf75d358e202da822bbe2c0`
([original asset revision](https://github.com/seraph1nia/odot-game-assets/pull/12)).
The exact pinned GitHub tree identifies every selected GLB and standalone decal;
the [runtime handoff](https://github.com/seraph1nia/odot-game-assets/blob/133c7cdeb368cada408d1c95c9414e08488371a9/docs/runtime-optimization/README.md#downstream-import-handoff)
owns the successor's representation contract. The inspected
source `catalog/associations.json`, `docs/ambient-motion/README.md` and actual GLB
animation/material data agree: ordinary object-transform clips named `ambient`,
not particles, a shader extension or a skeleton animation.

All **57 existing selected GLBs** match the successor pin byte-for-byte.
Relative to the initial vivid import, 15 buildings and the woodland bridge change;
the other 41 selections remain byte-identical. The initial vivid refresh included the
landed overgrowth/details and vivid painted palettes on selected buildings,
characters, forest floors, foliage and props. The selection and destination paths
are unchanged. There are **567 exact embedded PNG extractions** and four unchanged
standalone forest decals. No unused building, environment selection, Blender
source, preview, reference or catalog payload is shipped. The source has 16
animated buildings; the game uses 15 distinct building exports (Tavern has no
current gameplay/scenery consumer). The source has nine water selections; the
game retains its existing stream, woodland bridge and four surface-river exports.
Unused lantern bridge, foam and waterfall selections are not added.

[`manifest.json`](../src/Game/Assets/Authored/manifest.json) owns the complete
source-relative paths, SHA-256s, sizes, dependency/clip inventory and extracted
map identity. Five selected building GLBs contain two distinct embedded images
with the same `painted_leaf_roughness` name. glTF permits this; the locked Godot
importer appends the **source image index** to the later filename. The manifest's
mandatory, non-null integer `imageIndex` identifies each exact embedded payload and validation checks
both its original name and the importer's collision suffix. Both maps survive;
no GLB is renamed/re-exported and no map is silently overwritten. Extracted
texture sidecars enable mipmap generation, retaining the existing forest-distance
sampling policy on the newly named maps. Existing source and surviving import
UIDs remain in place. The separately pinned Worldtree/UI
members, permission scope, owner-permission text and license notices are unchanged.

## Optimized representation and tradeoff

The successor bakes stationary geometry into material/layout batches under each
animation/placement owner, retaining target Node3D owners and their ancestor TRS.
It reduces small moss topology and subpixel bevel segments upstream; the game
copies the immutable resulting GLBs, never re-exports or flattens them. Material
summaries are regenerated from the new bytes rather than retaining old indices.
All 567 indexed map records, extracted bytes and texture sidecars are unchanged.
The selected GLBs total **189,734,296 bytes**, versus **120,755,896 bytes** for the
initial vivid pin. This file-size tradeoff is explicitly accepted; it is not a
memory, GPU or rendered-performance acceptance.

Upstream scene-reachable mesh/surface counts include Academy 40/40, Barracks
35/35, Town hall 52/52, Arrow tower 32/32 and woodland bridge 85/85. Consumer
headless verification checks actual imported structure against bounded work
limits (64 meshes/surfaces per selected building, 90 for the bridge), exercises
production placement/instantiation, and records timing separately from rendering.
No additional game-side batching is introduced: the upstream representation
addresses the demonstrated thousands-of-native-nodes overhead.

Static mesh names are no longer attachment APIs. The existing timber-only bridge
adaptation still removes the five metadata-identified recessed component owners.
The scattered-grass batch and deck landing geometry are selected by their actual
pinned material contracts (`forest_leaf_light` and `wood_light.004`), not old
submesh names or material indices. The imported rest transforms and original
buffer/material resources remain unchanged by those consumer queries.

## Actual runtime owners

- `LandscapeAssets.Place` starts each building's actual imported ambient player
  after measuring/rest-grounding the original hierarchy. This covers ordinary
  tabletop construction/upgrade models, army-plot towers, the fixed Tree house
  and fixed Arrow tower. Shared Arrow/Bombard presentation mappings, enum IDs,
  picking, placement/size conventions and raised-upgrade support are unchanged.
- `LandscapeAssets.Native` starts the selected stream/surface-river clips. The
  live `VillageLandscape` installs its short surface watercourse as native scenes
  under the existing tile owners instead of flattening away the shimmer pivots.
  The source route bounds this to at most 34 consumers, and all native water
  geometry retains the former batches' shadowless policy. Floors, trails and
  passive scatter retain their existing batching. Repeated
  cover calls reuse each water consumer; tile removal frees its owned hierarchy.
- Connector/contact queries still use a cached **rest-pose** mesh. Only a detached
  query template loses its player; that mesh is never used as the visual water
  substitute. Actual visible water retains its authored hierarchy and packed
  color/roughness material binding. Each overlay moves just one eight-triangle
  interior shimmer strip; the base sheet, alpha and connector edges remain fixed.
- The woodland bridge deliberately retains the existing timber-only adaptation:
  recessed stream/foam/fall and substrate are removed at this consumer because
  they do not meet the surface river. Its obsolete recessed-water tracks are not
  played. The continuous surface water underneath uses the new loop. The dormant
  stream resource is independently verified; it is not newly placed in the world.

Imported animation libraries may prefix `ambient`. Playback resolves the actual
name, explicitly sets Godot `Animation.LoopMode.Linear` and calls `Play` on the
existing imported `AnimationPlayer`. Source keys span `1/24` to `97/24` seconds;
the imported duration includes the initial rest hold (~4.041667 seconds). Matching
rest endpoints are preserved. `SpeedScale = 1` uses ordinary presentation elapsed
seconds, not combat ticks, attack markers, simulation-speed multipliers or
network updates. No gameplay callback, unit action policy or general animation
subsystem is introduced. Existing camera, lighting, global theme/grading,
lifebars, invulnerable towers, shared arena, balance, ledger and Worldtree remain
outside this cosmetic change.

## Verification boundary

Tier A uses the existing manifest/embedded-byte admission and selected canonical
building/unit mappings, with an additional ambient clip timing/rig separation
assertion. Executable map regressions reject omitted/null, out-of-range, wrong-name,
wrong-duplicate and wrong-suffix identities, plus wrong hash/size, while admitting
both distinct payloads sharing a name. They call the same deserializer and indexed
map validator as distribution admission; no name-only fallback remains. Tier B extends the existing minimal `animation` component child with
production placement, actual idle-frame advancement, imported clip lookup,
localized changing transforms, fixed structural local transforms, loop seams,
repeat playback and native water material/geometry binding. It also instantiates
the production landscape to check that real water/home/defender consumers play
and repeated cover does not duplicate owners. Existing `assets`, `arena`,
`receiving` and `worldtree` selectors cover their respective preservation seams;
executed receipts are recorded below.

**Headless checks do not certify rendered in-game vividness, aesthetic quality,
readability, lighting interaction or GPU cost. Rendered in-game acceptance remains
unexecuted under this task's headless-only policy.** Standalone source/browser
receipts from the asset revision do not prove those game-view outcomes. No GUI,
actual-game graphical journey, desktop fallback or graphical acceptance was run.

### Initial vivid-pin receipts (Linux, locked .NET 10.0.401 / Godot .NET 4.7.2)

These historical receipts apply to `9a69b514e9fd7b101bf75d358e202da822bbe2c0`,
not to the optimized successor. Current-head authoritative validation belongs to
the outer pipeline; review-fix focused results are recorded separately below.

| Check | Evidence / result |
| --- | --- |
| Locked restore, build, C# formatting | `logs/vivid-restore.log`, `logs/vivid-build.log`, `logs/vivid-format.log`, final `--verify-no-changes` in `logs/vivid-final-format.log`; build: zero warnings/errors |
| Targeted provenance, mappings, rigs and ambient bytes | `logs/20261015-092130-e51370cc`; 15 passed after the final engine import |
| Broad fast C# | `logs/20261015-091453-1c15bcbd`; 738 core and 652 runner passed, two Windows-native argument tests skipped on Linux; deep tests excluded |
| Imported resources, rest/contact/connector geometry and materials | `logs/20261015-091752-4c5ba7b4`; `assets` passed, 6.51s component / 15.56s including preparation |
| Actual ambient/unit/melee playback | `logs/20261015-091807-72f9e5c0`; `animation` passed, 6.18s / 11.17s |
| Shared arena, member/objective lifebars, invulnerable purchased towers and squad controls | `logs/20261015-091818-3ae0526b`; `arena` passed, 6.88s / 11.88s |
| Native receiving/ledger actions and battle-flow allocation | `logs/20261015-091830-2284b292`; `receiving` passed, 23.51s / 28.51s |
| Existing independent living Worldtree consumer | `logs/20261015-091859-dbc95c28`; `worldtree` passed, 9.18s / 14.24s |

Preparation/import mutations were sequential. Tests use the existing owned
headless children and cleanup, never a new process supervisor. No transport,
package/export, native Windows, deep campaign or graphical qualification was run
locally for this cosmetic change. No tools, templates or OS packages were installed.

Retained failed development receipts are not counted as passes: the initial
byte-admission run exposed the five duplicate source image names; the first import
(`logs/20261015-090905-fc22afeb`) encountered stale generated editor UIDs for
removed maps (generated editor/UID caches retained outside the import root under
`logs/vivid-stale-generated`). New component assertions were corrected to compare
the imported **zero/end loop boundary**, not a fractional source key interpolated
by the importer, and to recognize actual direct mesh targets for water as well
as building pivots. Those intermediate failures are retained in
`logs/20261015-091107-3d6eee9b` and `logs/20261015-091307-483c4aa4`.
The first `assets` pass attempt (`logs/20261015-091550-c14c3504`) correctly rejected
new extracted textures without mipmaps; source bytes/assertions were preserved
and the texture import sidecars were corrected before the successful receipts.

### Optimized-pin focused review verification

Executed in the isolated review worktree with locked .NET 10.0.401 and Godot
4.7.2 .NET; all evidence paths below are worktree-local:

| Check | Evidence / result |
| --- | --- |
| Locked solution restore, changed-C# formatting, build | `logs/optimized-review/{restore,format,build}.log`; succeeded, zero build warnings/errors |
| `test-runner --filter FullyQualifiedName~AuthoredAssetTests` | `logs/20261015-190811-71c1a1bc`; 24 passed, zero skipped, 1.91s including runner overhead |
| `test-engine --scenario animation --startup-timeout-ms 180000` | `logs/20261015-190813-a00293ff`; passed, 5.24s component / 22.02s including sequential preparation/import |

All 15 selected buildings and all six selected water/bridge resources resolved
and played their actual loops, including changing localized targets, fixed
structural locals, closed seams and repeat playback. Production landscape water,
home/defender playback and repeated cover ownership passed. Imported building
counts range from 21 to 52 mesh/surface instances; the bridge has 85. Representative
production counts (including wrapper/player nodes) were Academy **46 nodes / 40
meshes / 40 surfaces**, Barracks **41/35/35**, Town hall **59/52/52**, Arrow tower
**39/32/32**, bridge **129/85/85**.

The diagnostic placement interval includes source/import hashing, resource load,
instantiation, initial geometry measurement, add-child and Play; it is **not** the
upstream warm instantiate-only interval or a rendered frame measurement. Example
first placements were Academy 167.01ms and Town hall 239.19ms. OS cache was not
flushed; these are single observations, not latency promises or FPS claims.

The first prerequisite invocation was blocked by mise's worktree trust check
before restore/tests. Retrying with process-local `MISE_TRUSTED_CONFIG_PATHS`
succeeded without changing global trust/configuration or installing tools.
Only the focused authored-byte tests and `animation` selector ran in this review
fix round. The `assets` selector's updated deck-preservation assertions, broader
fast tests, other engine components, transport, packages, native Windows and full
lint remain for the outer pipeline; rendering and memory/GPU acceptance remain
unexecuted. No graphical process or new runtime batching framework was used.
