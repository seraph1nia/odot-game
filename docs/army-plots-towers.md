# Army plots and tower authority batch

## Contract and owners

Baseline main source: `1d97f8a814009ab7e5665579cea518e302eacb62`.
Gameplay controls and concrete values are in
[gameplay](gameplay.md#defense-towers-on-army-plots).

- `City.ArmyPlots` owns the persistent army `SlotState` instances; command IDs
  and compatibility are documented in [session ownership](sessions.md).
- `SettlementCommands` owns direct `buy-plot`, paid construction, exclusive
  `upgrade-tower`, selling and all rejection/payment transitions. Existing
  Match ownership/phase/ready/pause guards and AuthoritySession retry ledgers
  remain the admission boundary. [Session ownership](sessions.md) documents
  the legacy wire alias and explicit upgrade action.
- `ArmyConfiguration` owns placement order, quotes and projections. Production
  recruit/Send paths pass the actual per-plot ownership/type array; arbitrary
  purchase order never becomes first-fit order. A tower consumes one whole squad
  home (three ordinary member places by default), not a size budget. Its shared
  arena weapon occupies that home's physical deployment cell until sold.
- `CombatConfiguration`, DefenseActor/CombatSimulation and `SquadDamage` own
  timing, within-target-group X/Y damage, resistance, target filtering and
  attribution. Rapid arrows contribute X only; Catapult/Bombard contribute X/Y
  to one selected squad, without cross-hex splash or victim caps. No cosmetic
  projectile applies numerical damage.
- Production `ArmyPlotView`, Tabletop selection/picking/inspection/action
  bindings and snapshots expose the changes. Artwork is the existing main
  arrow/bombard/plinth palette; no source asset bytes, dependencies or locks
  changed. Both new variants also use the existing city-plot drawing path.

## Paid-capacity and upgrade ledger

[Gameplay](gameplay.md#purchased-army-homes-and-town-hall-rotation) owns the
paid-capacity, tower-path, legacy-profile and refund rules and concrete values.
[Session ownership](sessions.md) owns wire admission, command expectations,
legacy aliases and configuration/decision identity. This batch receipt records
causal evidence and executed scope rather than a second current rule table.

Current shared-squad rules are **combat 7 / protocol 14**. `UnitTiming`,
`DefenseTiming`, runtime member profiles and tower catalog entries contain no
size/cap/radius authoring fields. Full compatibility covers all six actual X/Y,
range and cycle profiles. The landed independent base-actor decision namespace
still prevents optional Rapid/Bombard timing edits from rerolling unchanged
squads/base weapons; its current inputs contain no legacy size/cap/radius metadata.
Old snapshots/replays are recoverably refused, not silently normalized. Historical
versions and numerical receipts below do not describe current wire compatibility.

## Historical pre-squad causal diagnosis — retained failures, not refreshed goldens

The following receipts qualified the landed tower batch against the former
individual-actor game. Their size/radius/capped-victim transcripts are historical,
not the current [shared-arena combat contract](shared-arena.md). Paid numbered
plot purchase/ownership, exclusive upgrade paths, refunds and native modal/input
ownership remain; numerical actor/target/damage semantics are deliberately migrated.

The first implementation unintentionally accelerated legacy Arrow L2 and gave
legacy Catapult a one-hex radius. Its changed full combat fingerprint was also
used as the seeded salt for **every** actor, rerolling units/defenders even when
no new feature was used. The initial broad failure receipt is
`logs/20261012-191851-0deb48dc/`: historical first-clear tick 433 became 343,
with changed casualties, late-pause eligibility and resource availability.
That run failed and cancelled core coverage; it is not a passing receipt.

A partial correction stabilized unit/defender decisions but still coupled
legacy tower ties to the full new identity and still changed the legacy
Catapult footprint. The disconfirming receipt
`logs/20261012-192215-5e566793/` retained **7 core failures** (473 passes):
RulesVersion assertion 5 versus 6; six historical economy variants with
1551 versus 1641 terminal timing, Defeat versus continued Building, eliminated
city differences, and growth stock 7 versus 10 metal. Runner then passed 649,
with two Windows-specific skips. None of those failures was waived.

The correction in the local receipts below separated the explicit new variants
from the four old profiles and retained the original legacy decision namespace;
[session ownership](sessions.md) owns the current configuration/decision identity
contract. The original economy fixture assertions, paid prices, casualty
identities, outcomes and resource goldens remain unchanged. The intentional
RulesVersion assertion now verifies version 6 **and the complete exact six-profile
set**, configured legacy timing/splash,
rapid non-splash and new bombard radius, rather than weakening the assertion.

The retained paired-regression receipt uses seeds 0, 1, 123 and the historical
economy seed 14056307608042553509. This receipt predates the review correction
that includes the actor decision fingerprint in full compatibility identity;
its full-identity hash and counterfactual targets below are historical outputs,
not current-head fingerprints or expected counterfactual selections. It changed
only new Rapid/Bombard timing/radius metadata, kept the same three deployed
enemies/base tower/positions/sequence/price, and compared the first actual tower
selection. The one-condition counterfactual changed
**only** the selector salt back to full configuration identity:

| Seed | Original namespace / actual target | Full-identity counterfactual target |
| --- | --- | --- |
| 0 | 1 / 1 | 3 |
| 1 | 2 / 2 | 3 |
| 123 | 1 / 1 | 2 |
| 14056307608042553509 | 3 / 3 | 1 |

Earliest changed action is the initial attack selection at numerical tick 0 in
this minimal direct-action fixture, before any projectile impact or resource
change. Both sides paid the same 4 G + 3 W and retain 8 G/3 W; their base profile
is 500 hundredths damage, 12 windup, 60 cadence, one target, zero radius.
The preserved legacy salt is
`79d304d3da6d850ff756b29178073f1d48f34d35259e183e4028e84cc991d693`.
For the explicit counterfactual metadata (Rapid 3/17 ticks; Bombard 20/80,
cap 4, radius 2), the receipt's full identity was
`0050759aa39220ec3b97cb7f6ee1b413e2eb294d5ca6f7c75f8ef0718739508c`.
These values are diagnostic inputs, not shipping balance or known-seed branches.
Actual JSON/profile/stock outputs are in the targeted TRX below. A paired
180-step production Match replay also retains identical unit and defender/base
arrow state when only the new variant metadata changes.

## Executed verification

Preinstalled Linux x86_64 SDK **10.0.401** and Godot .NET **4.7.2-stable** were
selected explicitly through their existing executable paths, with
`MISE_AUTO_INSTALL=0`, locked restore and no tool acquisition. Matching installed
export templates were inspected, not installed. No alternate tool roots used.

| Receipt | Selected coverage | Result |
| --- | --- | --- |
| `logs/20261012-193345-201ec925/` | `test-core --filter 'FullyQualifiedName~ArmyPlotTowerTests\|FullyQualifiedName~EconomyArmyFixtureTests\|FullyQualifiedName~DefensePolicyTests\|FullyQualifiedName~SplashTowerTests\|FullyQualifiedName~CommandConstructionTests'` | 48 passed, zero skipped; 3.41s. Partial targeted coverage, including unchanged historical accounting and causal outputs |
| `logs/20261012-193636-4685d599/` | `test-engine --scenario receiving` | Passed; 14.30s including source preparation, 9.28s component. Existing native receiving/modal checks plus new army receiver |
| `logs/20261012-193650-decfd88c/` | `test-fast`, excludes `Tier=Deep` | 484 core and 649 runner passed; two Windows-native argument skips; 20.85s completed-binary suite |

Locked restore, solution compilation and `dotnet format Odot.slnx --no-restore`
also passed. A first source import encountered unrecognized UIDs in the initial
local import cache (`logs/20261012-191333-3af9692b/`); subsequent normal import
resolved the cache without source asset changes. Do not count that failed
preparation as an engine pass.

Tier A proves arbitrary direct purchases, stale/foreign/unfunded guards, every
army plot, full-footprint unit exclusion, first-fit after purchase/store/Send/
retirement/sale, generation/level conflicts, actual payments/refunds, compressed
snapshot load, serialized remote requests/retries and credential resume.
Clustered/adjacent/separated/foreign/friendly/dying targets verify authoritative
new bombard radius versus arrow single-target damage, one hit per victim,
attribution and dead/transferred-primary misses.

Tier B hosts only the real HUD, ArmyPlotView and native camera in the existing
minimal owned EngineTests scene. It loads current fonts/UI/tower resources,
uses production roof/ground-ray picking and selection, dispatches actual native
Control input for purchase/build/both upgrades, verifies real authority results,
stale rejection feedback and delayed acknowledgment, and disables recruitment
when all owned plots have towers. It never starts Tabletop._Ready, Main, a world
or an application session. It does not prove a full world mouse gesture, rendered
presentation, GPU pixels, compositor focus or a live multiplayer UI session.

## Limits and remaining gates

No full application/world/campaign, graphical/private-display/screenshot, live
Steam pairing, package/export/real transport process or release qualification
was executed for this batch. The existing graphical fixtures were updated to
select real army plots instead of the removed button, but were not run. The
pure serialized authority fixture is not a network-delivery witness. Snapshot
load and live credential resume are the existing persistence boundary; no new
authoritative disk-save facility is claimed. New tower/army balance is not
qualified by the unchanged finite legacy openings or fast tests.

Full no-mistakes review/test/lint/docs/publication/CI is a separate subsequent
handoff gate; these local receipts alone are not a ready PR or merge approval.
