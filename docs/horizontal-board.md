# Horizontal board and combat-start preview

## Presentation contract

The reference SVG has pointy-top hexes in horizontal staggered rows: a three-column
building district on the left, a river divider, and enemies above owned homes on
the right. Its labels, four enemy columns, lock counts and colors are not rules.
The game retains all nine indexed building plots, six purchased/locked homes per
owner, and the complete connected 63–252-cell arena.

`SharedCombatProjection` is the canonical pointy-top lattice, corner, containment,
formation and pose seam. Imported floor rotation, outlines, selection rays, home
positions, character roots, frozen aim, movement and projectile endpoints use that
same geometry. The old world lattice was already pointy-top; aligning the camera's
azimuth to zero makes its rows horizontal on screen instead of rotating only
outlines or swapping authoritative Q/R. A 32-degree tabletop pitch retains the
existing perspective style. No topology, cell ID, adjacency, path, range, target
order, seeded decision, ownership, cost or damage rule changes.

Preparation now includes the **actual shared arena**, including all co-op sides.
The focused city's buildings sit to its left. The existing imported animated river
connectors form a continuous nine-cell divider, with the existing timber bridge
across it. The menu's woodland scene retains its original composition. Purchased
weapons remain invulnerable and have no fabricated sentinel-HP bars. Existing
vivid resources, animation, lifebars and health-only overlays remain; no asset
bytes, shaders, global atmosphere or floating combat text were added or changed.

The preparation camera fits every actionable home, actual forecast body, physical
objective/purchased weapon and building silhouette, rather than padding for empty
outer fighting cells. Those cells are not deleted or made impassable; Deployment
and combat retain the complete arena overview and bounded navigation. Fitting
reserves the **measured** resource-ledger width (including its native growth),
bottom panel and 64px top chrome. Dialogs retain their existing modal input owner;
no doctrine/draft/lobby/reward UI contract changes. Clouds/backstops surround the
combined content and arena edges independently of weather/renderer preferences.
Ordinary zoom, cursor zoom, pan, reset and resize remain bounded.

At 1100×820 with a representative 353px ledger lane and 32% bottom allowance, the
native compact solo overview is **33.112** orthographic units versus **37.843** for
the former padded box through the *same new fitter*. This isolates content fitting,
not pixels. The previously published old-camera compact receipt was 34.753 with a
240px guessed lane; that is a different camera/UI input, not the counterfactual.
Co-op views must fit more real content and custom forecasts can widen the view;
there is no fixed enemy-count assumption or forced zoom that crops actionable
content.

## Divergence investigation and repair

This is combat-**start** parity, not prediction of later actions or outcomes.

- **Setup:** ordinary two-owner, seed123 numerical Match; real preparation
  manifest, native imported UnitViews, production camera/ray picker, three actual
  ready/production boundaries. No full application, graphical scene or transport.
- **Trigger:** final Ready consumes the manifest, creates live members, groups
  them into homogeneous squads and admits those squads to actual arena entry cells.
- **Earliest divergence:** `NextWaveFormation.Positions` packed raw individuals
  into six-column, 1.35-unit preview rows. It never consumed squad membership,
  formation slots or arena entry cells. History confirms it predates the shared
  arena (`78477f8`) and remained through the shared-arena change (`4430c3e`).
- **Masking path:** previous native coverage proved equal allocation counts and
  distinct figures, but not member transforms. Settlement hid owned fighters;
  Deployment/combat hid negative forecast figures. Thus the two layouts were not
  compared together.
- **Observed symptom:** the retained pre-change headless receipt
  `logs/20261015-213408-2e1fadc2` records preview IDs -1/-2 at
  `(-2.025,0,-8)` / `(-.675,0,-8)`, while their actual live counterparts 1/2 share
  cell1 with formation slots0/1 at `(-19.5,0,88.957695)` /
  `(-18.937084,0,89.93269)`. Absolute origin differences alone do not prove the
  cause: subtracting each pair's first member still gives `(1.35,0,0)` versus
  approximately `(.563,0,.975)`. Translation/camera/cloud changes cannot repair
  that intra-squad mismatch. No claim of graphical reproduction is made.

The independent packing path is removed. `BattleDeployment.Preview(snapshot)` is a
read-only projection using the same enemy compatibility and first-free admission
helpers as `CombatSimulation`. It uses the authority's ordered manifest, actual
capacity, persistent home assignments, individual formation slots and current food
forecast, including imminent final production. Stored/unfunded/fallen members do
not acquire visible field bodies. Recruits, deployment, research and funding
changes refresh from accepted snapshots; no optimistic units or extra preparation
settlement exists. Negative enemy forecast IDs remain distinct, ordered manifest
identities until the authority allocates positive live IDs. Queued overflow stays
nonspatial; types, profiles, origins, destinations and order are not invented.
The projection neither runs a simulator nor advances a tick or consumes RNG.

The same native CombatLayout/UnitView/SharedArenaView components render preparation
and combat entry. Preparation clears stale cosmetic attack/hit/facing interpolation
when sampling its idle entry pose. Dying bodies retain their playback owner.
Watchposts show their real forthcoming reset; **no Keep exception is needed**:
its actual footprint, art and persistent health are shared too. Purchased tower
state and ownership are unchanged. No wire/protocol/fingerprint change is required.
The only authority refactor is extraction of the unchanged compatibility and
first-free entry predicates into the projection seam; this is not a second spawn
or authority implementation.

## Coverage admission and receipts

Tier A now executes actual Match preparation/entry, codec round trips, projection,
member/manifest identities and profiles, partial funding, research, stored members,
reassigned homes, custom mixed/overflow and boss catalogs, fallen-origin
redistribution, and 1..4-owner adjacency/vertices/centers/containment. These are
controlled numerical supplies, **not earned campaign/balance qualification**.
The previous cheaper count-only fixture could not catch the geometry divergence.

The existing Tier B `arena` and `receiving` owners now exercise actual imported
roots/facing, all co-op/enemy rig picking, physical home rays, bridge/ribbon ports,
continuous native river owners, exact purchased-tower art/roof-picking bounds,
cloud resources, safe camera fit and extreme
bounded input at representative 1100×820, 1280×720, 1600×900, 1920×1080,
2560×1080 and portrait 820×1100 sizes. Native member transforms are compared across
actual authoritative entry **at the same tick**, not after combat has moved.
The component owns its temporary nodes/settings and uses the existing runner;
no new supervisor or full-game journey was added. Native arena work remains about
8 seconds locally; no graphical processes were launched.

Implementation receipts use installed locked SDK10.0.401 / Godot .NET4.7.2,
locked NuGet restore, acquisition disabled and process-local
`DOTNET_ROLL_FORWARD=LatestMajor`. Generated logs/import/build outputs stay ignored.
Final implementation receipts (publication/CI qualification belongs to the outer
no-mistakes run):

| Check | Evidence | Result |
| --- | --- | --- |
| Targeted `BattleDeploymentTests` | `logs/20261015-220449-09fe2761` | 10 passed; 2.11s; filtered/partial |
| `test-fast` | `logs/20261015-220503-5364cc14` | 748 core + 661 runner passed; 2 Windows-only skipped; `Tier=Deep` excluded; 64.15s |
| `test-engine --scenario arena` | `logs/20261015-221122-5dbb211c` | 7.71s component / 12.77s total |
| `test-engine --scenario receiving` | `logs/20261015-221044-09db841a` | 24.25s component / 30.49s total |
| `test-engine --scenario settings` | `logs/20261015-214845-d9189356` | 4.96s component / 11.72s total |
| `test-engine --scenario animation` | `logs/20261015-215247-c4385149` | 5.33s component / 10.38s total |

New failures were not waived:

1. First preparation import (`logs/20261015-213338-c410c0ec`) exited0 but reported
   stale/unrecognized resource UIDs while rebuilding this worktree's inherited
   generated import state. The runner correctly refused it. The completed import
   changed the generated inputs; the next preparation and baseline component
   passed. No asset/dependency lock or acquisition policy was changed.
2. Initial mixed-catalog projection emitted grouped rather than manifest order
   (`logs/20261015-214043-7fe5e618`, two failed / seven passed). Backfilling an
   earlier Sword group with a later-level Sword moved it ahead of intervening
   Mages in the returned array. Returning through the original manifest identities
   repairs order without changing the actual grouping/admission rule. The retained
   regression executes that interleaving/overflow case.
3. Native receiving with the final low pitch
   (`logs/20261015-215822-cac6d963`) correctly picked a nearer front figure when a
   fixture aimed through it at a back figure's body center. The smallest
   counterfactual keeps camera, figures and nearest-ray policy unchanged and moves
   only the sample to the exposed head/roof. The final receipt records center=-3,
   exposed=-1, expected=-1, and the arena fixture checks exposed-head picking among
   **all** own/co-op/enemy rigs. Observation targets now quote that exposed surface;
   picking never selects through a nearer model. This does not prove rendered
   readability or change authoritative placement/formation.

4. Extending the numerical fixture to include a genuinely stored member initially
   omitted the Hall's stone supply (`logs/20261015-220401-2ce740de`, five failed /
   five passed). Supplying explicit synthetic stone, like its already explicit
   gold/wood/metal, lets the ordinary paid Hall/store commands run. The final
   targeted and broad passes include the stored/funded/reserve lifecycle checks;
   no cost or guard was changed to make the fixture pass.

No new deadline, retry, synchronization override, graphics waiver, install or
unrelated-process cleanup was introduced.

## Explicit unexecuted qualification

No rendered in-game artistic acceptance, pixels, physical mouse/compositor,
GPU/cloud occlusion or performance, full menu/controller/session journey, Steam
invitations, transport, package export, Windows-native or deep campaign check was
run for this implementation. Dummy native components prove resources, input rays,
transforms and bounds, not final artistic readability. In particular, Dummy has no
reliable MultiMesh transform readback (the established settings component retains
that counterfactual); floor counts/resources and coordinate helper proofs are not
claimed as rendered terrain-buffer proof. Graphical acceptance requires separate
explicit authorization and remains unexecuted, not silently approved.
