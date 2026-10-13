# Reconstruction qualification checkpoint

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
`logs/20261013-094045-ade12e54/`. One frontage witness remains blocked below.
These are partial checks, not a complete suite or inherited waiver.

## Remaining blocker: actual occupied cleared-forward band

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
free resources or outcome acceptance was used to hide this. The same frontage
obstacle has now failed twice; implementation stops for firstmate assistance.
A coherent legal tier-before-refill receiving schedule could be investigated
next (current policy fills twelve low-tier troops early, leaving no refill slots
for later tier upgrades), or existing direct occupied-front admission coverage
could be examined for an explicitly authorized replacement. Neither alternative
is implemented or claimed equivalent at this checkpoint.

## Limits / next step

Last source build passed. Locks/tools/assets remain pinned. All evidence and
checkpoint source are task-local. The guard refusal stamped 1791884334 in the
status file is preserved; fresh actual `pwd -P` and Git top-level both resolve to
`/home/bart/.treehouse/odot-game-b83784/2/odot-game`, on the named task branch.
This driver has not invoked a pipeline from `/home/bart` or any other checkout.
A future supported no-mistakes invocation must explicitly enter this worktree.

Complete fast verification, native receiving component, bounded transport/package,
no-mistakes, push/PR/CI remain unexecuted for the completed rebuilt batch. No
full application/world/UI journey, graphical process/private display/screenshot,
GPU qualification or live Steam is authorized or claimed. Fix/dispose the
frontage blocker through firstmate, then finish grouped qualification and one
fresh owning no-mistakes handoff; do not publish the current checkpoint as green.
