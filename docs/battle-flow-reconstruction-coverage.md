# Reconstruction qualification checkpoint

This is historical pre-squad reconstruction evidence. Its city-transfer admission,
protected-rear/size-budget assertions and protocol-14 receipts are not current
contracts. See [shared-arena receiving and resume](shared-arena.md#eliminated-owner-receiving-and-resume)
for current boundaries and [the coverage replacement ledger](testing-audit.md#coherent-admission-profile-and-compatibility-replacement-2026-10-13)
for the retained purposes and their squad-era replacements. Measured failures and
receipts below retain their original inputs.

The growth disposition is resolved: firstmate authorized option 1 on
2026-10-13, retaining the single-mine/four-paid-unit early/late control but
rebuilding its causal experiment. This is new implementation, not recovered
round3 dirty bytes. Original failed-run custody remains unchanged.

## Six-mode causal replacement

`EconomyArmyFixtureTests.PaidPreparationSeparatesEmptyCapacityProductionRefillAndCooperativeGrowth`
now runs all six modes as one bounded comparison, instead of six independent
rows against obsolete ticks/IDs. Every construction, upgrade, recruitment,
purchase and sale records its published quote, actual before/after resources
and genuinely created owned IDs. Each readiness records both cities' actual
production/upkeep. No final income is spent before combat.

It proves:

- Empty bought homes do not equip or repack troops: no-arrow/home-only armies,
  terminal ticks and health agree; the empty expansion is actually paid.
- Early investment funds an upgraded mine and greater stock, but producer-only
  and the baseline field the same four-unit wounded/level-2 army and both lose.
- Producer/refill/growth share the identical player-1 pre-intervention resources
  and unit state. Refill equips six, growth equips nine in **each city** with
  three actually purchased homes per city and strictly greater damage strength.
- Both refill and growth clear wave 3 under the new paid schedule; growth keeps
  strictly greater total city health and more survivors. The former required
  elimination in refill-only is not fabricated.
- The arrow branch pays its construction, waits for production 2 before paying
  the Catapult upgrade, and retains the same unit armies as no-arrow. It clears
  with player 1 fallen, player 2 alive and no surviving soldiers. This is a
  genuine structural effect, not a decorative mode name.

Residual policy differences are explicit: growth's receiving city buys/builds
and upgrades its trainer early in wave-3 preparation, rather than taking the
optional Catapult upgrade. Other modes retain that structural upgrade. The
shared wave-2 refill is paid after production 2, not a common second opening
mine. The historical Research Tower/Market replacement subchain is removed
from this economic-control experiment only.

Latest focused paid selection passed **11 cases**, zero failures/skips:
`logs/20261013-092908-dea5e4ef/`. It includes the six-mode comparative case,
three other economy fixture cases, three reconstructed paid schedule cases
and four full-sale transaction cases. Earlier failed diagnostics, including
the first rebuilt comparative attempt's arrow outcome mismatch, remain in
`logs/reconstruction/` and their original runner evidence directories.

## Removed-chain coverage mapping

| Removed/changed experimental purpose | Independent production coverage retained/rebuilt |
| --- | --- |
| Actual research production, carrying progress across tower sale, atomic overflow | `CityResearchTests.ProductionUsesActualTowersAndRetainsCarryAcrossSalesWithAtomicOverflow` |
| Technology purchase authority, stale/foreign/ready/disconnected nonmutation | `ResearchLifecycleTests.ForeignStaleReadyAndDisconnectedTechnologyRequestsRejectWithoutMutation`; `CityResearchTests.PurchaseGuardsApplyToEveryAuthorityPolicy` |
| Research sale retains veteran identity/wounds/technology, removes access | `BuildingTransactionTests.SalesKeepVeteransAndResearchButRemoveTowerAndResearchAccess` |
| No speculative level/healing refresh after technology purchase | `CityResearchTests.TowerlessPurchaseRefreshesQuotesWithoutHealingOrChangingLevelAndDetachesSnapshots` |
| Actual material funding, paid plots/research progress/Market trade and both-city trainer timing together | New `PaidPreparationScheduleTests.PaidResearchLandMarketAndReceivingTrainerFundTheFinalMixedVeteranArmy`; default rules/real commands, three clears, no grants |
| Sale/trade quotes, instance/ownership, overflow and ledger deduplication | `BuildingTransactionTests.SaleRefundsActualSummedInvestmentAndRejectsReplacementTargets`, `RefundAndTradeOverflowRejectWithoutRemovingInstanceOrStock`, `MarketUsesOwnedInstancesExactBundlesAndLedgerDeduplication`, `OrdinaryAuthorityRequestsDeduplicateNewTransactions` |
| Complete quoted food debit/gold proceeds before final production, atomic unavailable/nonmutation | New `BattleFoodTransactionTests.CompleteSaleMatchesAuthorityWithoutMutatingProjection`, `GoldOverflowDistinguishesSaleRejectionFromLaterProductionRejection` |
| Actual six-unit physical capacity, rejection/retry after seventh role and resume | `EconomyArmyFixtureTests.RecoveredReconnectSeedNeedsAnAffordableHomeBeforeItsSeventhMixedRole`, `RecoveredNetworkSeedPaysForRoomBeforeExplicitRangedRecruitAndKeepsRetries`; distinct paid capacity witnesses, not the single-mine growth control |
| Independent storage/healing tracks, completed funding, no immediate healing | New `PaidPreparationScheduleTests.PaidHallAndIndependentUpgradeTracksRetainThreeLaterRecoverySettlements`; `ArmyRecoveryTests.FieldFirstFoodIncludesStoredButNeverMakesStoredCombatants`, `ActualProductionHealsOnlyCompletedPaidStoredIdsAndUsesCeiling`, `UnderfedStoredIdsDoNotHealAndProductionPreflightCannotPartiallyHeal` |
| Final readiness income/research/recovery/overflow and no fourth editable stage | `BattleFlowTests.ExactlyThreePreparationsPreserveOperationsAndPerProductionValues`, `FinalProductionPreflightsIncomeProgressAndRewardWithoutPartialSettlement`, `FinalStonecutterSettlesOnceAcrossRetryCodecAndReconnect` |

The retained native economy consumer uses a paid Research Tower on purchased
plot 5 instead of replacing its only mine, earns Foundation before final editing,
then replaces the tower with Market. The receiving trainer is built/upgraded
before its final recruits; its Stonecutter refund funds the structural Catapult
witness. The new paid schedule case proves default-rule material affordability
and actual upkeep/survival, not graphical input, screenshots or a complete native
session. Graphical consumers are inspected/compiled only.

## Broader failures and coherent corrections

First rebuilt full fast run `logs/20261013-092929-ed17456b/` failed: runner
648 passed/1 failed/2 platform skips, core cancelled after recording additional
failures. The accepted UI asset hash caught the port's modified resource-ledger
script. It is now restored **byte-for-byte to MAIN**; the receiving C# caller
sets the already-supported `income_heading` field instead. No asset/provenance
manifest is changed, refreshed or imported from unpublished sources.

The next serial full fast run `logs/20261013-093352-f4623e43/` failed: core
488 passed/20 failed, no skips; runner unexecuted after core failure. Corrections
were grouped by changed preparation boundary:

- Ordinary army opening asserts four paid troops and actual free physical room.
- Fallen-city reward test retains the receipt from the last completed living
  wave (8), rather than awarding the city after it falls during wave 9.
- The independent three-city session transfer witness pays a distinct six-unit
  opening and retains actual funded identities/next-wave allocation assertions.
- Quote/authority role cases pay enough real research capacity before final
  editing instead of trying to spend production 3 after combat entry. Their
  declared high-stock immutable authoring is unchanged; no combat journey is
  needed to prove the authority seam.
- Worldtree purchases use captured contextual commands and paid research capacity
  earning three points before final editing. Market transaction accounting is
  two ordinary productions minus construction plus the actual sale refund.
- The Catapult-only timing witness pays two stone producers, obtains actual
  producer refunds, and pays its final-preparation tower before production 3.
- MAIN's tower metadata counterfactual uses three readiness transitions, not four.
- Mage priority uses an ordinary paid frontline partner and verifies that both
  Mage and melee equipment are actually affordable when choosing support.

Focused requalification of those changed classes passed **72/73**, no skips,
`logs/20261013-094045-ade12e54/`. One frontage witness remained blocked at that
checkpoint. This was partial coverage, not a complete suite or inherited waiver.

## Preserved frontage setup failures

`VillageStrategyTests.OrdinaryThreeCityProgressionReinforcesAClearedOccupiedForwardBand(seed:109)`
has a deliberately preserved `Assert.NotEmpty(held)` against real friendly
positions at transfer. Its original four-paid-unit setup defeated before its
first next-wave proof. A paid six-unit opening with later Stonecutter investment
restored first clear, receiving-city clear, enemy transfer and the prior living
roster, but **no friendly occupies the required enemy front band at transfer**.

- First corrected witness: `logs/20261013-094045-ade12e54/`, transfer tick 1088,
  player 1 has nine soldiers/no enemies, held set empty.
- Second bounded correction retained the historical two-city insertion order
  using real paid two-unit batches during final preparation, not guessed IDs.
  `logs/20261013-094519-34d6509c/`: transfer tick 1037, player 1 has eight soldiers/
  no enemies, held set still empty. The focused case fails; admission/survival
  assertions following that guard have not executed.

No production change, guessed ID, seed reroll, weakened frontage/admission guard,
free resources or outcome acceptance hid these failures. Implementation stopped
at `d0d4a40284195257ef31b442d5784e57458f957d` for firstmate assistance. The empty
held setup is an observed setup failure, **not** an established admission defect.
Original failed receipts and their unexecuted downstream assertions remain so.

## Authorized split: paid progression and genuinely occupied production admission

Firstmate's `paid-frontage-witness` disposition on 2026-10-13 rejects further
speculative tier/refill-order changes to force seed 109's incidental positions.
The combined fixture is replaced by two independently established contracts:

1. `VillageStrategyTests.PaidThreeCityProgressionPreservesClearedTransferIdentityAndCompletes(seed:109)`
   runs the same bounded legal six-paid-unit specialized default-rule opening
   and earned investment. Every construction, upgrade, home purchase, sale and
   recruit checks actual quoted resource debit/refund. Actual births record
   owner/type/level; both cities' upkeep covers their real funded IDs. Wave 2
   allocates twelve enemies, including four from fallen city 3, after exactly
   six production events. At actual transfer tick 1037, city 1 is cleared with
   eight real paid soldiers at cells 11–14, **not** enemy-front cells 4–6. Incoming
   origin/owner/type/profile/identity and positive bounded health are retained;
   friendly wounds/profile/home assignment are preserved. The ordinary admission
   bound executes, the wave clears into wave 3 without defeat, and nonempty
   survivors retain their paid birth identities. This is not an occupied-front
   proof and is no longer named or claimed as one.
2. `HexBattleTests.RedistributionIntoOccupiedClearedFrontageAdmitsEngagesAndCompletes`
   strengthens the existing focused production boundary, across four incoming
   roles with and without retained death leases. It deliberately seeds three
   living city-2 defenders in cells 4–6 with normal already-committed attack
   deadlines. Its explicit **`Assert.NotEmpty(held)`** additionally verifies that
   every seeded source ID is actually deployed there before the real city-1
   elimination/transfer. It checks unchanged living source positions, health,
   ownership and profile through transfer and first admission; incoming exact
   identity/health/profile/deadline, legal destination/owner/faction and placement;
   per-cell capacity; exact first admitted ID; actual landed engagement and
   health progress; completion, nonempty owned surviving source identities, and
   no friendly occupation of the protected enemy rear. It uses the existing
   deterministic numerical setup (`CampaignFixture.Three(first:1)`, defender
   damage zero), **not** default-rule paid recruitment or a campaign proof.

### Earliest divergence and bounded counterfactual

The paid path derives deployment/movement from purchases and ordinary combat;
its actual cleared waiting soldiers remain in cells 11–14 at transfer. The
focused occupied path explicitly establishes source positions 4–6 at tick 25
before elimination/transfer at tick 27, with existing normal committed attacks.
That spatial setup is the earliest purposeful divergence, before admission is
called, not a failure to honor already-occupied source space.

`HexBattleTests.OccupiedSourceRatherThanSeededPaidOutcomeDeterminesLegalIncomingBand`
runs one smallest practical counterfactual through the same production helper:
same incoming Swordsman and source IDs/ownership/health/profiles/deadlines, same
elimination and admission tick 27, but the source positions move to their own
front instead. Held source 3 admits into legal enemy rear cell 2; held source 0
admits into legal enemy front cell 5. Both complete with survivors, and the occupied
case preserves all source positions. The eight-role/death cases admit at tick
27 or the actual compatible death release at tick 48, engage and complete. No
product defect is demonstrated; no admission, movement, cadence, death lease,
economy, squad or balance implementation is changed to make the tests pass.

### Meaningful old assertion / negative coverage inventory

| Original combined witness guard | Concrete replacement / negative coverage |
| --- | --- |
| Paid Farm/mines/recovery Lumbermill/Barracks and six funded recruits; no final income spent | New named paid progression checks accepted contextual commands, published costs/refunds and actual resource deltas, birth IDs, first-entry metal 12/food 9, six paid upkeep per equipped city |
| First clear → wave 2; city 3 eliminated; twelve actually allocated enemies | Same paid progression retains these guards, adds production count 6 and per-living-city count 6 plus fallen-origin count 4; `SessionFlowTests.SerializedThreeCityTransferPreservesIdentityAndNextWaveAllocation` proves serialized allocation |
| Receiving-city elimination and a cleared surviving city before actual transfer | Same paid progression preserves the bounded actual elimination/transfer and no own-city enemies; nonempty living paid roster retained |
| Genuinely nonempty deployed occupied enemy-front source | Strengthened eight-case `RedistributionIntoOccupiedClearedFrontageAdmitsEngagesAndCompletes`, not an assumption about the paid seed; explicit nonempty/all-ID guard and unchanged held positions through transfer/admission |
| Admission exists, meets first bound, and is a deployed unit from another origin | Both replacements; focused boundary additionally asserts exact transferred/admitted ID, profile/health/deadline, owner/faction/destination, capacity and legal band |
| Combat completes → wave 3 / no defeat | Paid progression retains exact wave-3 result and proves paid owned survivor IDs; focused numerical admission proves actual landed attack/health progress, completion, positive surviving source identity and legal final positions |
| Capacity and faction/ownership protection; no invalid or partial reservations | Retained `HexOccupancyTests.SharedSizesAndFactionProtectionApplyEvenWhenCapacityIsFree`, `SourceDestinationAndTransitReserveAtomicallyAndIndependentMovesCanStartTogether`, `ReconstructionRejectsConflictsWithoutMutatingTheCurrentIndexAndTransferReleasesAllLocks`; occupied boundary asserts actual capacity/ownership and protected rear exclusion |
| Malformed topology/anchors/deployment, overflow and non-owning paid placement | Retained `HexBoardTests.MalformedTopologyAnchorsAndDeploymentAreRejected`, `CombatConfigurationTests.OverflowAndFractionalHexRangesAreRejected`, `ArmyRosterTests.PaidRecruitmentUsesFirstFitAndRejectsFullWithoutPayment`, `RetireAndHallTransfersPreserveIdentityWoundsAndRejectFullDestinations`; ordinary authority foreign-city/nonmutation guards remain independently tested |
| Compatible cumulative death leases / bounded queued admission | Retained `HexBattleTests.ClearedProtectedEntryBoundUsesCumulativeCompatibleDeathReleases`; strengthened role/death cases forbid early deployment, pin the first bound and first admitted ID |

Only the historical seed-position coupling is retired. The nonempty occupied
contract is explicitly established, stronger downstream protection executes, and
no old meaningful contract is silently deleted, skipped or accepted arbitrarily.

## Opt-in redistribution consumer migration (review R1)

The retained `test-network --scenario redistribution` driver now consumes
`CampaignStrategy.RedistributionPreparation`, also executed by the existing
`PaidThreeCityProgressionPreservesClearedTransferIdentityAndCompletes(seed:109)`
Tier A witness. Both equipped cities pay for Farm, two Metal Mines and the
GoldRecovery Lumbermill during Preparation 1; after two settlements they build
Barracks and recruit six units in alternating two-unit batches. Preparation 3
settles before combat with twelve metal and nine food per equipped city and
six actual funded IDs, with no fourth spending opportunity. Wave 2 sells the
receiving city's second mine for its actual refund, builds its Stonecutter,
uses the existing earned investment policy and recruits one partner unit in
both Preparations 2 and 3, rather than only the last preparation.

The driver checks actual quoted debits/refunds and paid birth identities,
three/six production counts and funded identities, original twelve-enemy
allocation (four fallen-origin enemies, six per living destination), eliminated
observer resume/refusal, cleared-city transfer identity/damage, bounded first
admitted identity, reservation reconstruction, engagement and nonempty owned
survivors. Existing child/event-reader, cancellation, endpoint and runtime
ownership are unchanged. Preparation synchronization waits for the actual
settlement, including ordinary final-preparation death cleanup; it does not
require an intermediate acknowledgment phase to remain visible.

Only the obsolete seed-front positioning coupling is retired from this driver.
Its genuinely nonempty occupied-source guard maps to the eight role/death cases
in `HexBattleTests.RedistributionIntoOccupiedClearedFrontageAdmitsEngagesAndCompletes`
and the same-input occupied/empty counterfactual
`OccupiedSourceRatherThanSeededPaidOutcomeDeterminesLegalIncomingBand`.
The negative ownership, capacity, malformed placement, atomic reservation and
cumulative death-lease mappings above remain unchanged. No source positions,
resources, units, balance, movement or death leases are forced or changed.

Coverage admission: the observable regression is an obsolete four-unit paid
transport opening defeating before next-wave allocation, compounded by the
historical occupied-front setup guard. The existing pure witness had its own
correct transcript but could not detect the stale driver schedule. Sharing the
schedule makes the existing deterministic Tier A case execute the consumer's
actual actions and assert published payments, every settlement and the earned
transfer/survivor result. This adds no journey or test case; estimated extra
cost is negligible relative to its existing bounded two-wave simulation. Pure
execution does not prove automation parsing, peer publication timing or live
transport. The retained native redistribution scenario is **unexecuted**, not
proven equivalent to the pure witness; no full native campaign/application/world/
UI or graphical journey is authorized or claimed. Historical failed receipts
and prior exact-head qualifications below are unchanged, not current-head passes.

Focused review-fix verification on top of
`0b423f52197529c28b32c8ee94183d6b85f599e7` passed **11/11**, zero failures/skips:
the paid seed-109 progression, eight occupied role/death cases, occupied/empty
counterfactual and cumulative compatible death-release admission case. Exact
selection was `FullyQualifiedName~VillageStrategyTests.PaidThreeCityProgressionPreservesClearedTransferIdentityAndCompletes|FullyQualifiedName~HexBattleTests.RedistributionIntoOccupiedClearedFrontageAdmitsEngagesAndCompletes|FullyQualifiedName~HexBattleTests.OccupiedSourceRatherThanSeededPaidOutcomeDeterminesLegalIncomingBand|FullyQualifiedName~HexBattleTests.ClearedProtectedEntryBoundUsesCumulativeCompatibleDeathReleases`.
Evidence is `logs/review-r1/tests.log` and `redistribution.trx`; measured test
duration was 796 ms. Sequential locked solution restore used installed package
cache plus an empty worktree-local feed, with no package acquisition. Targeted
`dotnet format Odot.slnx --no-restore --include` for the three changed C# files
and `dotnet build tools/DevRunner/DevRunner.csproj --no-restore` passed; driver
build had zero warnings/errors in 6.62 s. Restore/format/build logs are beside
the test receipt. The initial SDK version check was refused by mise's untrusted
worktree configuration; a process-local `MISE_TRUSTED_CONFIG_PATHS` allowed the
locked 10.0.401 SDK without changing global trust or installing tools. This
partial selection is not a full test/lint, native scenario or pipeline approval.

## Completed local qualification / delivery handoff

The following receipts and delivery status describe the original pre-pipeline
reconstruction handoff. They are not qualifications of the active pipeline's
current head; the later review-fix selection is recorded separately above.

- Initial split focused selection passed **39/39**, no skips:
  `logs/20261013-100435-64ca71b4/`.
- Full fast C# `test-fast --jobs 1` passed **509 Core**, **649 Runner**, zero
  failures: `logs/20261013-100509-3f689a65/`. Two Windows-native argument cases
  are explicit platform skips on Linux. No Godot/graphics in this selection.
  The immutable asset/provenance check now passes with restored MAIN bytes.
- Headless `test-engine --scenario receiving` passed in **27.45 s**, including
  its sequential locked restore/build/import and **22.58 s** component phase:
  `logs/20261013-100705-74a8bf19/`. Existing owned harness, minimal Controls/rigs,
  no Main/application/world. This exercises landed army plot/tower controls,
  resource ledger, Worldtree/modal bindings and the new `BattleFlow` component:
  real speed pointer/keyboard/ack/disconnect behavior, Settings separation,
  actual imported idle enemy rigs, native bounds picking and inspector,
  forecast-inclusive content framing, all three actual Ready settlements and
  final construction, manifest consumption and stale preview cleanup.
- Final focused selection after strengthening all paid quote/refund receipts and
  logging actual source positions passed **39/39**, zero skips/failures:
  `logs/20261013-100950-0a755712/`. This is the same named split/occupancy/board
  selection; not another full-suite claim. No relevant native product inputs
  changed after the successful receiving run.
- Final `dotnet format Odot.slnx --no-restore` and sequential solution build
  passed, zero warnings/errors. An earlier combined format/build command hit
  its external 180 s limit with an empty build log and no remaining owned child;
  it is **not** a pass. A process-local `UseSharedCompilation=false` build then
  passed in 26.87 s without touching the unrelated long-lived compiler server.
  This discriminates shared compiler use, not a proven diagnosis or deadline
  waiver; historical failure remains `logs/reconstruction/build13.log` and
  caller evidence. Final nonshared build took 6.59 s.

Locks/tools/assets remain pinned. All evidence and checkpoint source are task-
local. The guard refusal stamped 1791884334 remains preserved; actual `pwd -P`
and Git top-level both resolve to the named task worktree. This driver has not
invoked a pipeline from `/home/bart` or any other checkout. A future supported
no-mistakes invocation must explicitly enter this worktree.

No live ENet transport, export/package probes, no-mistakes, push/PR/CI have run
for this completed batch. C# authority requests/retry/resume/codec/protocol
rejection and the headless receiving boundary are qualified; they are not a live
transport or published package claim. Existing old packages cannot qualify the
new protocol 14 build. No full application/world/UI journey, graphical process/
private display/screenshot, GPU qualification, desktop focus or live Steam is
authorized or claimed. Native imported rig/input/geometry proof is not pixel/
aesthetic acceptance. Delivery still requires the one fresh owning no-mistakes
handoff and exact-head non-draft PR with mandatory CI green, without a waiver.
