# Authored 3D assets and presentation identities

## Migration status

The distribution contains **44 byte-identical authored GLBs** (63,548,504 bytes)
and **270 byte-identical extracted PNG maps** (25,167,952 bytes). All 71 legacy
active/dormant models and their 166 buffer/atlas/import supports are removed;
historical notices and the old manifest remain attributable, outside runtime 3D.
Actual imported rigs, populated village, roster, combat/death/pause/reconnect and
four required near/far images have passing owned evidence. Full after-CI/package
results are tracked in the [migration report](authored-migration.md), not inferred
from a focused pass. See [runtime rendering](runtime-rendering.md) and
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

- Full required pre-migration CI passed on landed `0332c44`:
  `logs/20261004-184946-7b57e3b1/` (source/network/UI/client/server exports,
  headless package smoke and owned graphical package smoke).
- The concrete OpenSpec `asset-map.json` covers all 71 old scene files, their
  166 buffer/atlas/import support files, role substitutes and source export
  hashes; dormant bundles have explicit removal dispositions.
- `mise run test` runs cheap actual manifest/GLB byte-contract checks, typed
  catalog/role projections, all eight exported clip/socket/equipment hierarchies,
  corrupt/truncated/LFS-container rejection and existing numerical/runner suites.
- Final required set/eight equipped rigs and two-size launcher controls passed
  `logs/20261005-105822-31653932/launcher-worker/`.
- Final economy/roster passed in that same full-CI attempt; selected roster proof
  independently passed `logs/20261005-105320-f29d8cc4/` (99.38s).
- Full source combat, including cast/axe/sword/hit/recovery, death precedence,
  paused poses/focus return, resize, research and fresh-session cleanup passed
  `logs/20261005-111135-31569449/` (156.80s).
- Four actual near/far overview/close images and live motion/equipment advancement
  passed `logs/20261005-102101-8a020ed8/`; exact acquisition/persistence and current
  authority action/camera/pixel identities are recorded, not historical substitutions.
- Cheap C# coverage: **713** (13 solo/session, 9 cooperative, 478 general, 213 tooling),
  `logs/20261005-113824-f735c0b5/`, preserving the previous 702.
- Final actual static buffers/native PNG roundtrip passed
  `logs/20261005-103918-fd8cb4a7/`; original static-cost characterization passed
  `logs/20261005-104128-886cb582/`. Unsafe consolidation remains deferred with
  all normal/winding/material failures retained; source hierarchies are untouched.
- Software rendering does not prove native compositor/GPU performance, physical
  input, audio listening quality, Steam acceptance or unexecuted platform support.
  UI approval is not creator license clearance. Final full-CI/package status is
  reported explicitly in the migration report.
