# Authored 3D assets and presentation identities

## Migration status

The distribution contains **44 byte-identical authored GLBs** (63,548,504 bytes)
and **270 byte-identical extracted PNG maps** (25,167,952 bytes). All 71 legacy
active/dormant models and their 166 buffer/atlas/import supports are removed;
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
https://github.com/seraph1nia/odot-game-assets.git at
`a640d721065233dfbf488747694cd95110d4a907`. Their `manifest.json` records exact
source-relative export paths, SHA-256, byte counts and embedded dependencies.
`NOTICE.md` records owner permission exactly: **the repo is my own work, so yes,
use it**. This is not an invented open-source license or permission to copy
reference images. Only required game-ready exports are distributed; no editable
Blender sources, reference archives, catalog files or source-repository caches.
UI/audio retain their independent notices. The user separately approved
continued existing third-party UI use with the exact words **yes, 3rd party ui is
fine too**. This is user approval, not independent verification of creator-granted
redistribution rights or a new license; no UI/audio replacement is included.

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
