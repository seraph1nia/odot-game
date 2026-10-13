# Three preparations, global combat speed and the next wave

## Behavior and integration boundaries

[Gameplay controls](gameplay.md#preparation-and-battle-controls) own the numbered
preparations, settlement/spending boundary, global speed choices and next-wave
inspection. Serialized `Phase.Building` retains its enum identity for turns 1/2;
`Phase.Preparation` denotes final turn 3. The HUD changes stage names, not building
identities. `Match.ResolveReady` preflights resource/research/recovery and final
upkeep/reward capacity before mutation.

`Main` feeds physics delta to the existing `AuthoritySession.Advance` owner.
`Match.Step` is still one deterministic 60-Hz simulated tick: damage, movement,
projectile ordering, statuses and cadence are unchanged at equivalent simulated
time. Each admitted wall slice is capped at 100 ms/six steps; idle and paused
time create no debt. Death cleanup outside combat retains its prior rate.
Verification acceleration remains a separate, owned harness-only multiplier.
Delivered snapshots own selected state; input does not optimistically toggle an
unaccepted rate. Guests never advance local rules. Playback and presentation
share the accepted rate; authority permissions follow [session ownership](sessions.md).

`Match.PlanWave` expands the authoritative campaign once per wave into
`MatchSnapshot.NextWave`; `BeginWave` consumes that manifest. Type, level, rank,
boss flag, origin, destination and resolved profile are shared, including fallen
roster allocations. Lifetime-unique negative forecast IDs cannot collide with
live positive IDs allocated at combat entry. Forecasts consume no RNG, resources,
combat ECS/occupancy, targets or attacks. Refresh/reconnect preserve the manifest.
The ordinary rig-bounds picker and inspector consume these forecast profiles.
`BattlefieldFrame` includes actual formation positions in its content-derived
framing owner; modal and army plot/tower controls remain. No artwork or provenance
bytes change.

The authoritative `FoodForecast` and `BattleFood.Project` own final-production
projection. A food-sale preview applies the complete existing quote (resource
`Units` debit **and** `Gold` proceeds) before optional final production. Invalid
stock, overflow or missing income produces unavailable without mutation. This is
not a new trade rule. Independent tests compare ordinary commands and funding
receipts, rather than manufacturing expected stock by calling the projection.

## Compatibility and persistence

[Session compatibility](sessions.md) owns peer/version admission, battle-flow
snapshot rejection and the credential-only persistence boundary.
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
broader failed checks/corrections and the authorized occupied-frontage witness
split. The default-rule paid progression proves actual earned transfer/survivor
identity; focused numerical admission establishes genuinely nonempty occupied
source and preserves its positions, capacity, ownership and incoming identity.
The historical incidental seed-position coupling is not an admission defect.

The [completed local qualification checkpoint](battle-flow-reconstruction-coverage.md#completed-local-qualification--delivery-handoff)
owns exact historical passes, failed receipts, and unexecuted transport/package/
rendered checks. They are not current pipeline-head or delivery qualification.
