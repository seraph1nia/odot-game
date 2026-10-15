# Vivid overgrown exports and in-game ambient motion

## Immutable input and selection

The game consumes `odot-game-assets` main revision
**`9a69b514e9fd7b101bf75d358e202da822bbe2c0`**, the merged
[asset revision](https://github.com/seraph1nia/odot-game-assets/pull/12).
The source main API and merged state were checked before importing. The inspected
source `catalog/associations.json`, `docs/ambient-motion/README.md` and actual GLB
animation/material data agree: ordinary object-transform clips named `ambient`,
not particles, a shader extension or a skeleton animation.

All **57 existing selected GLBs** differ from the previous 3D pin and are replaced
byte-for-byte, not only the files in the final motion diff. This includes the
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
additive `imageIndex` identifies each exact embedded payload and validation checks
both its original name and the importer's collision suffix. Both maps survive;
no GLB is renamed/re-exported and no map is silently overwritten. Extracted
texture sidecars enable mipmap generation, retaining the existing forest-distance
sampling policy on the newly named maps. Existing source and surviving import
UIDs remain in place. The separately pinned Worldtree/UI
members, permission scope, owner-permission text and license notices are unchanged.

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
assertion. Tier B extends the existing minimal `animation` component child with
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

### Executed receipts (Linux, locked .NET 10.0.401 / Godot .NET 4.7.2)

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
