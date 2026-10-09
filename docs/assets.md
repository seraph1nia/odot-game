# Authored 3D assets and presentation identities

## Migration status

The distribution contains byte-identical authored GLBs and extracted PNG maps;
the [manifest](../src/Game/Assets/Authored/manifest.json) owns the current inventory
and per-file byte counts. All 71 legacy active/dormant models and their 166
buffer/atlas/import supports are removed;
historical notices and the old manifest remain attributable, outside runtime 3D.
Actual imported rigs, populated village, roster, combat/death/pause/reconnect and
four required near/far images have retained owned evidence. Acceptance outcomes
and their pre-review input boundary are tracked in the [migration report](authored-migration.md#retained-acceptance-evidence),
not inferred from a focused pass. [Current casualty admission](current-casualty-admission.md#status)
records the subsequent freshness correction's verification status. See [runtime rendering](runtime-rendering.md) and
[guest service/observer handoff](enet-guest-cadence.md) for measured costs and the
integration correction; neither rejected optimization candidate ships.

## Source and permission

Selected GLBs under `src/Game/Assets/Authored/` are copied byte-for-byte from
https://github.com/seraph1nia/odot-game-assets.git at the revision pinned in the
[manifest](../src/Game/Assets/Authored/manifest.json). It records exact
source-relative export paths, SHA-256, byte counts and embedded dependencies.
`NOTICE.md` records owner permission exactly: **the repo is my own work, so yes,
use it**. This is not an invented open-source license or permission to copy
reference images. Only required game-ready exports are distributed; no editable
Blender sources, reference archives, catalog files or source-repository caches.
UI/audio retain their independent notices. The user separately approved
continued existing third-party UI use with the exact words **yes, 3rd party ui is
fine too**. This is user approval, not independent verification of creator-granted
redistribution rights or a new license; no UI/audio replacement is included.

## Landed quality refresh

The [landed asset upgrade](https://github.com/seraph1nia/odot-game-assets/pull/2)
changes 11 of the 44 exports already used by the game. The remaining 33 match
that revision without copying new bytes. No unused catalog models are added.
The existing [manifest](../src/Game/Assets/Authored/manifest.json) is the
source-to-destination ID/path/SHA-256 inventory: each ID is its `path` without
`.glb`, `sourcePath` is relative to the pinned source repository, and the game
destination is `src/Game/Assets/Authored/<path>`.

| Changed game-used IDs | Export refinements |
| --- | --- |
| `buildings/archery_range`, `buildings/town_hall` | Taller archery lookout and better-separated hall wings; quieter, length-aligned timber grain |
| `buildings/metal_mine`, `buildings/research_tower`, `buildings/weaver` | Timber cleanup; Weaver's intended blue/cream canopy restored by the source exporter |
| `characters/knight`, `characters/evil_ranged_unit` | Shorter/broader Knight plume, compact hood, neck accent and equipped crossbow bolt feathers |
| `characters/evil_berserker_unit`, `characters/evil_mage_unit`, `characters/evil_melee_unit` | Effective authored equipment/body material colors restored |
| `environment/hex_woodland_bridge` | Contact-facing foam crescents propagated into the existing bridge tile |

All 268 retained PNGs are unchanged embedded-image bytes. Two scratch-only
`painted_stone_dark_color` extractions (Archery range and Town hall) and their
import sidecars are removed because the new GLBs no longer use them. No new
texture dependencies, source re-exports or loader changes are needed. Existing
resource paths/UIDs, placements, runtime scale policy, tower/projectile origins,
rig/clip/socket interfaces, picking/collisions, camera/lighting and numerical,
save/session/network identities remain unchanged. Appearance is intentionally
updated; prior-art pixel equality is not an acceptance requirement.

### Refresh verification (2026-10-08)

Full local `mise run ci` passed before (765.19s,
`logs/20261008-182545-562fda95/ci-summary.json`) and after (594.35s,
`logs/20261008-184851-b91dcdb3/ci-summary.json`). The final gate includes 504
core and 573 runner tests, actual authored-static fidelity, six network cases,
five source UI slices, ordered Linux client/server exports and headless/graphical
package smoke. Post-import hashes still match all 44 pinned source GLBs and all
268 embedded map extractions; existing compatibility assertions were not weakened.

Normal settlement `economy-materials-market-land.png` and
`army-expansion-retirement.png`, and combat `combat-camera-paused.png`,
`combat-unit-inspection.png` and `research-current-burn.png` were inspected in
that final run's economy/combat evidence directories. The hall wings, taller
archery lookout, colored timber/roofs and equipped faction models load without
observed missing textures, new placement clipping or broken attachments.
Small plume/feather/grain/foam details remain subtle at normal overview scale;
this is representative game-view inspection, not whole-catalog art acceptance
or native GPU/compositor/performance proof.

Two earlier after-gates remain failed evidence: stale generated editor UIDs for
the removed maps stopped import (`logs/20261008-184158-dff1189e`), then reconnect's
paid setup lost its observer city (`logs/20261008-184423-3546fd00`). Stale caches
were retained outside the import root; the unchanged reconnect slice passed
in 79.19s (`logs/20261008-184658-21b13756`) before the final full pass. No gameplay,
fixture, importer or assertion changes were made to obtain that pass.

## Names versus authoritative identities

Presentation names describe the selected authored silhouettes. Requests, stable
control selectors, enums, saved/session state, profile calculations, recruitment
quotes, economic costs/output and numerical traces retain their existing IDs.

| Visible building | Canonical identity | Existing role retained |
| --- | --- | --- |
| Bakery | `Farm` | Food production |
| Woodcutter hut | `Lumbermill` | Wood production, including explicit zero-wood gold recovery |
| Gold mine | `Mine` | Gold production |
| Metal mine | `MetalMine` | Metal production, now a distinct source silhouette |
| Stonecutter | `Stonecutter` | Stone production |
| Weaver | `Weaver` | Cloth production, no longer a duplicate woodcutter |
| Barracks | `Barracks` | Sword/axe recruitment |
| Archery range | `ArcheryRange` | Ranged recruitment |
| Magic academy | `Arcanum` | Magic recruitment |
| Research tower | `ResearchTower` | Personal research progress |
| Arrow tower | `ArrowTower` | Existing ranged defense profile |
| Bombard tower | `CatapultTower` | Existing area-defense profile, not new cannon mechanics |
| Market | `Market` | Quoted resource-bundle sales |
| Town hall | `TownHall` | Stable storage, separate capacity/healing upgrades and guarded roster transfers/retire |

| Friendly name | Enemy name | Canonical unit type |
| --- | --- | --- |
| Knight | Boneguard | `Swordsman` |
| Berserker | Horned berserker | `Berserker` |
| Archer | Hooded crossbowman | `Crossbowman` |
| Mage | Horned mage | `Mage` |

The canonical enemy faction remains `Skeletons` for compatibility even where the
source costume is hooded or horned. Friendly ranged units now visibly carry bows;
this does not alter their existing ranged targeting, damage, windup or upkeep.
Berserkers use authored dual axes rather than promising a two-handed silhouette.
Boss status, Roman levels, size, authoritative health/damage and current
capabilities remain independent of the cosmetic name and mesh.

## Available substitutes and grounding

The fixed health-bearing home uses Tree house; the fixed defender uses Arrow
tower. These are reserved scenery, not extra buildable economic roles. Gold
stockpiles use cargo crates, food uses provisions barrels and wood uses timber
planks: these are honest available substitutes, not newly authored gold bars,
sacks or logs. Counts remain authoritative projections bounded by the existing
stockpile cap. Level upgrades use available provision/cargo/training/book props
and a measured stone plinth where a tower needs a supporting deck.

Source terrain is flat-top, radius 2.55, meadow surface zero, foundation bottom
-0.36. The adapter rotates it 90 degrees and scales it to existing
pointy-top anchors without changing numerical hex topology. Its stream is
straight, so the decorative river runs along a compatible settlement-edge row;
a complete woodland bridge replaces one stream cell rather than overlaying
another water/ground tile. Shallow stepped meadow terraces and rocky woodland
outcrops replace unavailable legacy slopes/mountains. Terrain batching combines
only existing imported material surfaces and reuses chunked instances.

## Authored animation and equipment contract

All eight role/faction exports contain the shared 19-joint `chibi_v1` rig with
`root`, hands and `weapon_socket.L/R`. Required clips are `idle`, `walk`, `run`,
`attack`, `hit`, `death`; locomotion is looping and actions are one-shot in the
presentation adapter. Original equipped child geometry and socket transforms
are retained, including shields/offhand axes, the Archer's right-hand arrow and
the enemy mage's right-hand flame. Staffs, bows and the enemy crossbow are on the
left socket; sword/main axe are on the right. Casting can use the free/flaming
right hand as its strike origin without moving the staff out of its authored
left socket.

Measured **GLB** attack keys start at `1/24` seconds and end at `25/24` seconds;
the 24-frame-interval attack pulse peaks at frame 13, `13/24` seconds. Do not
assume the source's one-second pose interval starts at zero. The presentation
adapter must align that authored pose to unchanged authoritative impact ticks;
it must not alter numerical attack timings or refresh frozen trace digests.
Actual Godot consumers validate the imported skins/materials, all six clips,
19 bones, equipped geometry/socket alignment and changing bone/equipment poses.
Owned progression and paused four-view captures complement cheap container checks. Horizontal root motion and
imported gameplay callbacks remain forbidden.

## Verification evidence

The [migration report](authored-migration.md#retained-acceptance-evidence) owns
full/selected outcomes, counts, costs and limitations; [current casualty admission](current-casualty-admission.md#status)
owns the review correction's evidence. The archived
[asset map](../openspec/changes/archive/2026-10-05-migrate-authored-3d-assets/asset-map.json)
records legacy scene/support removal dispositions.

`mise run test` runs cheap manifest/GLB byte-contract checks, typed catalog/role
projections, exported clip/socket/equipment hierarchies and corrupt/truncated/LFS
container rejection alongside existing numerical/runner suites. Actual engine
buffer fidelity and graphical pose/placement acceptance have separate scopes;
see [static fidelity](asset-fidelity.md) and
[the four-view proof](enet-guest-cadence.md#early-observer-and-exact-paused-images).
