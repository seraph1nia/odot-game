# Authored 3D migration: evidence and follow-up decisions

## Delivered integration

The initial migration used only selected game-ready exports from
`odot-game-assets` revision `a640d721065233dfbf488747694cd95110d4a907` for bundled
3D content. The measurements and assessments below describe that migration,
not the later [landed quality refresh](assets.md#landed-quality-refresh).
[Provenance and substitutions](assets.md) distinguish presentation
names from unchanged canonical selectors/enums/save/protocol identities. Exact
owner approval is recorded, not invented licensing. Source/editable/reference
content was neither modified nor copied; existing UI/audio and notices remain.

- Initial migration inventory: 44 byte-identical GLBs: **63,548,504 bytes**;
  270 byte-identical extracted maps:
  **25,167,952 bytes**. Total copied model/map data: **88,716,456 bytes**, versus
  **37,159,085 bytes** for legacy model/buffer/map content. These are source bytes,
  not installed-package or resident-memory estimates.
- 71 legacy scenes plus 166 buffer/atlas/import supports removed, including dormant
  export content. Historical notices and `docs/asset-history/kaykit-manifest.json`
  remain. Distribution checks reject missing/unregistered/legacy 3D and packed
  `.import`/`.remap` discrepancies; no fallback exists. Shared source/packed path
  projection accepts only those mapping suffixes and still requires the exact44 models.
- All eight actual skinned/equipped 19-bone rigs use imported idle/walk/run/attack/
  hit/death clips. Authored frame13 attack peaks map to unchanged authority impact;
  root horizontal suppression, death precedence, event/attachment identity remain.
- Every economic building, home, defender, stockpile and upgrade uses documented
  source roles/substitutes and measured terrain/deck contacts. Gold crates, provisions
  barrels and timber planks are substitutes, not promised bars/sacks/logs.
- Rotated/scaled authored multi-surface terrain retains the numerical pointy-top
  anchors. Continuous straight settlement-edge river, one complete bridge-cell
  substitution, shallow terraces, rocky/woodland clusters and common menu/match
  lighting use original geometry/materials. Only fidelity-validated terrain batches;
  unsafe static flattening/ineffective instancing are retired, not hidden toggles.
- Source/import/imported/engine/backend/version identities invalidate existing
  scene/terrain/bounds caches. There is no second generated asset pipeline.
- [Exclusive ENet guest service and early observer handoff](enet-guest-cadence.md)
  remove the demonstrated transport/admission failure without changing simulation,
  channels, security, receipts or intermediate snapshot/event conservation.

## Visual assessment, distinct from green assertions

Owned software-rendered views at 1100×820 and 1280×720 show a coherent chunky
woodland/fantasy family: timber architecture, stone bases, blue/gold friendly rigs,
darker horned/hooded opposition and a warm parchment UI. The dedicated Metal mine,
Weaver, Market and Town hall improve role distinction over duplicate old silhouettes.
The meadow/stream/bridge share ground/water contacts; sparse clusters retain battle
and plot sightlines. Full-size and close combat show equipped geometry and distinct
intent/impact cues rather than dummy animation pulses.

Limitations remain visible: the meadow is vivid/textured beside comparatively
simple pine silhouettes; Town hall/roof detail is dense at overview; nearby large
building volumes can overlap a center ray. Read-only selectors now quote an exposed
roof and verify that the ordinary picker resolves to its own plot, rather than
advertising an occluded volume-center point. No geometry/collision shrink was used.
Names such as Knight/Archer describe the costumes; original mechanical profiles
remain, including the canonical enemy `Skeletons` identity. These are deliberate
substitutions, not a claim of one-to-one thematic fidelity.

### Retained acceptance evidence

**Input boundary:** the table records migration acceptance before review commit
`8eeabc9`. That commit tightens rendered death-pose freshness and removes unused
renderer surfaces; the earlier full-CI pass is historical for those changed inputs,
not a complete-gate result for the reviewed head. [Current casualty admission](current-casualty-admission.md#status)
owns the correction's focused verification and remaining acceptance status. The
native-service and four-view melee inputs remain unchanged.

| Check | Evidence | Result |
|---|---|---|
| Clean-main full before CI | `logs/20261004-184946-7b57e3b1/` | passed, source/network/UI/exports/package |
| Pre-review final cheap tests | `logs/20261005-121856-cb2c2053/` and final CI | **750 passed**: 500 gameplay/250 tooling; original baseline retained |
| Native conservation/cadence/lifecycle | `logs/20261005-103819-bd0dd052/` | exact 80/80 snapshots, 15 receipts, retry/reconnect/pause/cancel/owner guards |
| Single real timing comparison | `logs/20261005-100632-5b96b17c/` | receive median12.26s→1.17s; final aligned gap790→8 ticks; startup transient remains |
| Focused melee acceptance | `logs/20261005-102101-8a020ed8/` | 12 observations/four acquisitions4.35s; exact persistence1.47s; all six flags/four paused views |
| Final static buffers/PNG | `logs/20261005-103918-fd8cb4a7/` | passed; unsafe normal/winding flattening remains rejected |
| Original static cost | `logs/20261005-104128-886cb582/` | measured, no candidate enabled |
| Final roster independent slice | `logs/20261005-105320-f29d8cc4/` | passed99.38s, store/return/retire/paid recovery/independent upgrades |
| Full source combat | `logs/20261005-111135-31569449/` | passed156.80s, all roles/death/pause/resize/research/fresh session |
| Authorized current casualty correction | `logs/20261005-120254-70454a9b/` | source combat151.61s; actual pause797/rev892, focused damaged enemy/fresh body, normal inspection and cleanup |
| Selected graphical package | `logs/20261005-121957-3388ee29/` | passed20.34s; exact imported inventory, rigs/animation/equipment, ordinary input at both sizes |
| **Pre-review final full after CI** | **`logs/20261005-122036-c53487dd/`** | **passed553.42s**, all source/network/UI, sequential exports and both package smokes |

**The pre-review complete gate passed; no required stage was skipped.** Before CI took344.53s;
after553.42s. Source UI487.50s includes economy307.08s, combat161.81s,
launcher153.15s, reconnect70.39s and settings48.91s (owned slices overlap).
Network172.54s, sequential client/server exports4.90s/3.84s, headless smoke1.30s
and graphical package23.60s are separately retained in `ci-summary.json`.
These are actual warm-build software-verification costs, not native load/frame budgets.

Historical migration failures remain evidence, not waivers for changed review inputs: occluded plot selector
(`logs/20261005-104225-8acbe650/`), legacy Berserker clip predicate
(`logs/20261005-105822-31653932/`) and current casualty admission
(`logs/20261005-111934-a7cc3f3b/`,469.03s). [The narrow latest-state/accepted-receipt
correction](current-casualty-admission.md) preserves its predicate, normal commands,
focused city and original deadline, with at most three attempts and owned cancel/await.
Final CI independently retains the eligible797/revision892 casualty frame and declared
cleanup, not a historical image or foreign-city substitution.

The next full attempt (`logs/20261005-120634-a312ed37/`,536.16s) passed all source
checks/exports/headless smoke but exposed the packed `.glb.import` projection defect.
Ten executable inventory negatives corrected the path representation, not the required
set. Selected package acceptance preceded the final full pass. No failure is erased;
no assertion/deadline was relaxed, and no asset optimization was added.

## Practical resource/render cost

Final original-scene static diagnostic: **8,512 draw calls, 8,841 objects,
601,202 primitives, 331,228,203 texture bytes, 40,188,198 buffer bytes,
11,385 scene nodes, 13,063 resources**, software FPS6. Same static 1280×720 camera,
Bakery/Barracks/Metal mine, tick0. Standard probe responses516–524ms versus live
364–394ms; post-draw waits306–309ms versus153–155ms. Optional inventory contributes
56–85ms to those diagnostic observations and is not enabled in ordinary combat.
Source texture bytes and importer/GPU representations are different measures.

| Comparable retained measure | Legacy before | Authored after |
|---|---:|---:|
| Source model/buffer/map bytes | 37,159,085 | 88,716,456 |
| Source declared meshes/material slots (all selected models) | 166 / 75 in71 scenes | 6,273 / 677 in44 scenes |
| Client PCK bytes | 22,807,524 | 45,367,988 |
| Server PCK bytes | 14,902,068 | 37,460,848 |
| Engine executable bytes (each role) | 73,672,920 | 73,672,920 |
| Source1280×720 starting village tiles / terrain batches | 2,544 / 65 | 2,544 / 63 |
| Source populated economy tiles / terrain batches | 2,244 / 50 | 2,244 / 48 |

Source mesh/material totals count declared glTF objects/slots, not unique native
resources or submitted draws. Baseline did not instrument native buffer/draw/node
monitors, so no invented legacy values or claimed draw-speed improvement are supplied.
Its existing `Models`/`Materials` fields count representative visual roots/first-mesh
materials, not the complete imported hierarchy, and are not used as that proxy.

Required44 imported `.scn` files total **16,771,199 bytes**; the270 extracted-map
`.ctex` files total **20,302,060 bytes**. Those specific importer outputs are not total
cache or resident memory. Complete final export trees measure client214,729,850 and
server206,822,710 bytes, including runtime/native/UI/audio files; no comparable
before tree total was recorded. PCKs still carry export metadata/notices/non-3D
content; server size is not evidence of graphical instantiation. Actual package
loading verifies all required scenes and embedded dependencies without source access.

Terrain reuses nine surfaces (meadow4/stream5) in bounded MultiMeshes; the complete
bridge remains original. Native attribution at the original static diagnostic reports
pine796mesh nodes/1,393surfaces/3materials, bridge849/869/57, Metal mine453/493/35,
Barracks407/439/35 and Bakery292/292/16. Repeated geometry/material resources remain
shared through native scene loading; unsupported static surfaces/LOD/rigs keep their
hierarchies. Static probes and both final package frames retain full inventory and
terrain/placement evidence. The final populated economy additionally observes10,739
draws/752,726primitives/19,514nodes, emphasizing that static fixture cost is not a
maximum populated-scene estimate.

This is an expensive scene on llvmpipe, not proof that native GPU performance is
acceptable or unacceptable. The finite static candidate reduced calls23% without
useful latency gain and changed image channels; pine instancing reduced calls9.5%
but increased primitives5.9% without useful gain. Neither ships. Exact topology,
UV/material/normal/handedness constraints were not waived. See
[runtime investigation](runtime-rendering.md) and [fidelity](asset-fidelity.md).

## Finite prioritized follow-ups (not implemented/authorized additions)

| Priority | Recommendation and observed benefit | Rough effort / dependency |
|---|---|---|
| P1 | Qualify actual native GPU/compositor and startup latency before setting a performance target. Detailed imported building/bridge/pine hierarchies and ~331MB texture residency justify profiling; fewer draw calls alone already failed as a proxy. | 1–2 days for a representative-device profile and frame/load budget; requires chosen hardware/platforms. Any further rendering/source-export optimization needs a separate fidelity/cost proposal. |
| P1 | Clarify creator redistribution terms for existing Trio UI and music before claiming a generally reusable/public asset license. Owner approval of continued use is not third-party creator clearance. | Half-day inventory/contact plus creator response; independent legal/product decision, no pack purchase or assumed GPL relicensing. |
| P2 | Prioritize owner-authored semantic stockpile props (gold, food, timber) in the evolving asset project if the substitutes confuse players. Dedicated silhouettes would remove the current documented cargo/provisions/plank ambiguity. | 1–3 artist days; separate asset-project authorization and normal immutable export/provenance update. Not necessary to alter economy roles. |
| P2 | Evaluate a coherent low-detail foliage/ground treatment against these actual overview/close images: textured vivid meadow, simple pine and detailed architecture have different visual densities. Retain battle visibility and source material fidelity until an art decision exists. | 1–2 art-review days; owner export/art direction, followed by measured import/render comparison. No unapproved recolor or decimation now. |
| P3 | Review faction/name/weapon expectations with a small playtest: Knight/bow-Archer and costumed horned/hooded enemies now differ from canonical Swordsman/Crossbowman/Skeletons terms. Tooltips explain the substitution, but fiction could be clarified. | Half-day copy/playtest; product/localization decision. Any profile/timing/faction identity changes are explicitly outside this migration. |
| P3 | Review roof-label density and target readability on native displays after the four paused captures. A presentation-only label policy could reduce overview clutter without hiding combat/equipment cues. | 1 day prototype/review; preserve real picking, health/camera/control tests and both resolutions. |

No additional feature, pack acquisition, source authoring, optimized asset set,
Steam acceptance, native Windows/macOS qualification, physical input or audible
quality is claimed. All owned verification uses private data/displays/Dummy audio;
no developer preferences, credentials, desktop surface or unrelated endpoint is modified.
