# Three preparations, global combat speed and the next wave

## Behavior and integration boundaries

**Preparation 1 → Preparation 2 → Preparation 3 → Combat** replaces the former
four preparation checks. Serialized `Phase.Building` remains value 1 (turns 1/2)
and `Phase.Preparation` remains value 2 (turn 3). Actual building names remain.
Each preparation's Ready settles exactly one ordinary production using current
buildings, research and eligible stored army. Final Ready settles production 3,
pays upkeep once and immediately enters combat. Final construction, research and
healing upgrades participate; there is no fourth spending window, queued recruit,
batched earlier income or compensating balance change. Resource/research/recovery
and final upkeep/reward capacity preflight precede mutation.

The ordinary Farm/Metal Mine/Barracks opening spends eight metal from the first
two productions on **four paid Swordsmen**, retains ten food in final editing,
and enters combat with eleven food/four metal after final settlement/upkeep.
Two ordinary homes still fit six size-two units. The lost spending window, not
price or capacity, prevents this particular opening filling them.

The native top control is **1x / 1.5x / 2x**, default **1x**:

| Selection | Former combat pace | Simulated ticks per wall second |
| --- | ---: | ---: |
| 1x | 0.5 | 30 |
| 1.5x | 0.75 | 45 |
| 2x | 1.0 | 60 |

`Main` feeds physics delta to the existing `AuthoritySession.Advance` owner.
`Match.Step` is still one deterministic 60-Hz simulated tick: damage, movement,
projectile ordering, statuses and cadence are unchanged at equivalent simulated
time. Each admitted wall slice is capped at 100 ms/six steps; idle and paused
time create no debt. Death cleanup outside combat retains its prior rate.
Verification acceleration remains a separate, owned harness-only multiplier.
Playing-host speed changes require the original host; solo uses its owner and
dedicated uses authenticated connected-player authority. Delivered snapshots own
selected state; input does not optimistically toggle an unaccepted rate. Guests
never advance local rules. Playback and presentation share the accepted rate.

`Match.PlanWave` expands the authoritative campaign once per wave into
`MatchSnapshot.NextWave`; `BeginWave` consumes that manifest. Type, level, rank,
boss flag, origin, destination and resolved profile are shared, including fallen
roster allocations. Lifetime-unique negative forecast IDs cannot collide with
live positive IDs allocated at combat entry. Forecasts consume no RNG, resources,
combat ECS/occupancy, targets or attacks. Refresh/reconnect preserve the manifest.
The existing MAIN unit rigs display idle gold-marked formations with NEXT/level
badges and a wave heading. The ordinary rig-bounds picker and native inspector
show their real profiles without army actions. Transition removes stale forecast
figures/inspection. `BattlefieldFrame` includes actual formation positions in its
landed content-derived framing owner; modal and army plot/tower controls remain.
No artwork or provenance bytes change.

The authoritative `FoodForecast` and `BattleFood.Project` own final-production
projection. A food-sale preview applies the complete existing quote (resource
`Units` debit **and** `Gold` proceeds) before optional final production. Invalid
stock, overflow or missing income produces unavailable without mutation. This is
not a new trade rule. Independent tests compare ordinary commands and funding
receipts, rather than manufacturing expected stock by calling the projection.

## Compatibility and persistence

Wire protocol **14** refuses older peers at admission, including MAIN's former
protocol 13. Battle-flow snapshot version 2 distinguishes numbered preparation
semantics from legacy four-check snapshots; missing versions deserialize as 0
and the codec refuses them, Building turn 3 and invalid speed values. Historical
snapshots are not silently migrated or authoritative saves. `SessionFile` stores
private running-session credentials/sequence only, not match state; its schema
is unchanged. Reconnect to the same matching running host retains pace/forecast.
Shared-arena integration can consume these landed phase, snapshot and authority
interfaces later; no unpublished squads architecture is imported here.

## Preservation-first rebuild custody

Base: landed MAIN `f9f02b6bb44e247a7a124699905649cf2d04bc94`, including the
content-derived view and army-plot/tower changes. `origin/main` identity was
confirmed read-only before porting. Source: verified committed history
`755c0986bdf6f0bab709e2f55f152cdb49f7aac8`,
`f2282fdebd9764d8583366bc3b5b92c010c9320b`, and
`1a6cfe59846b94ee4fe7b1643265a4eef22c9edb`. A task-private bare verification
mirror imported only the specifically preserved bundle; its SHA-256 was
`4817b0d0512a890d5562bc564d6bd210dacd2daa7c321177391558ae8c658549` and fsck
passed. The read-only archive ref matched the exact preserved commit. Porting
used a three-way committed-behavior patch, deliberately combining conflicts
with MAIN's army plots, tower controls and native picking/framing.

The failed run `01M4RT87GRFZS5C902ZQY7YXF3` remains terminal failed/unpublished.
Its original copy, branches, archive refs, caller, evidence and run records are
unchanged. Complete round3 dirty source is unavailable: retained edit/native
records are recipes/evidence, **not** recovered post-format source. Full-sale,
paid specialist/hall schedules and current-MAIN integration here are newly
reconstructed implementation, not an assertion that dirty source was recovered.
Historical compiler exit139/core, later retry build0/focused Core1/Runner0,
phase cutoff exit143 and refused custody recovery are failures, not exemptions
or a Test waiver. Fresh validation of this branch is required.

## Initial reconstruction checkpoint

Task-local raw evidence lives under ignored `logs/reconstruction/` and the
runner's separately named `logs/<run-id>/` directories. Locked SDK 10.0.401,
Godot.NET 4.7.2-stable, matching assets and all locks remain unchanged. Explicit
process-local tool roots and no-auto-install flags are used; compiler admission
uses `DOTNET_PROCESSOR_COUNT=2`, without claiming that explains the historical
GC-thread SIGSEGV or permits future crashes.

- Locked restore and sequential solution build passed (zero warnings/errors).
- Initial focused selection: **53 passed/10 failed**, no skips; failures exposed
  unfinished six-mode phase/tick goldens and unavailable historical sale-test
  construction. Failed evidence: `logs/20261013-090031-099e6e84/`.
- New sale tests use a declared immutable two-stone-output boundary fixture to
  legally build the Market before preparation 3, not a product rebalance.
  Subsequent paid/sale focused selection passed those four cases, both new paid
  specialist/hall schedules and the other three economy fixture cases; **9
  passed/6 failed** total in `logs/20261013-090706-7e82a42b/`. The six remaining
  cases were the growth experiment; this is partial qualification only.
- Latest experiment diagnostic: **3 passed/3 failed**,
  `logs/20261013-091105-7b3d1f32/`. This is not a completed batch or validation.

The experiment retains a single-Metal-Mine, four-paid-unit opening with actual
owned friendly IDs/payment receipts and early-versus-late Lumbermill investment.
A common, fully paid **wave-2** refill after production 2 keeps the receiving city
alive; it adds no second opening mine or free equipment. The attempted replacement
retains the original mine rather than replacing it with Research Tower/Market
sales; independent research/transaction/recovery negatives remain elsewhere.
The initial checkpoint required an explicit disposition, not a blanket refresh.
Its unavailable arrow upgrade, changed refill outcome and incomplete both-city
expansion are historical failed attempts, preserved in the
[decision note](battle-flow-reconstruction-decision.md).

Firstmate subsequently authorized a coherent causal replacement. The
[reconstruction coverage checkpoint](battle-flow-reconstruction-coverage.md)
records its passing paid selection, cross-mode strength/damage/survivor proofs,
independent negative-coverage mapping, legal retained-native material schedules,
broader failed checks/corrections, and the remaining occupied-frontage blocker.
No completed full qualification, pipeline, push, PR or CI is claimed. Native
receiving and transport/package checks have not yet run in this reconstruction.
No full application/world/UI journey, graphical display, screenshot, GPU,
desktop focus or live Steam qualification is authorized or claimed. A minimal
headless native component can prove imported resources, input/picking and geometry,
not that the formation renders attractively or a full network session works.
