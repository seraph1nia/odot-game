# Tasks

## 1. Establish the implementation baseline

- [x] 1.1 Run full `mise run ci` before the substantial implementation, or document reuse of a successful baseline with unchanged source/environment inputs; verify prerequisite failures are reported without installing tools and retain the evidence path.

## 2. Rebase the economy and preserve paid army behavior

- [x] 2.1 Update default grants, base/producer outputs, construction, upgrades and expansion quotes in `World.cs`, `Catalogs.cs` and `EconomyConfiguration.cs` to the design tables; extend existing catalog/transaction tests for zero-gold basic construction and upgrades, positive advanced prices and the funded six-Swordsman opening, and verify with `mise run test`.
- [x] 2.2 Change equipment quotes to nearest-whole midpoint-up rounding from original bases and permit nondecreasing custom totals; adapt existing army progression tests to verify all default level prices, positive/zero components, custom plateaus and unchanged combat profiles and food upkeep using `mise run test`.
- [x] 2.3 Convert Market bundles and ordinary/boss material rewards while retaining food/research amounts; extend existing resource/campaign tests for exchange-value preservation, food forecast refresh, atomic spending/overflow and exactly-once final rewards, verified by `mise run test`.
- [x] 2.4 Adapt shared `CampaignStrategy` and `ResearchWitness` openings and gold/material reserve thresholds to new units while keeping food thresholds in their original units; verify all existing ordinary twenty-wave campaign acceptance cases and cooperative assertions through `mise run test`, including disclosed role recruitment/upkeep evidence.
- [x] 2.5 Update the building/output, expansion, recruitment, Market, reward and refund examples in `docs/gameplay.md` and the README economy description; verify every documented default agrees with the frozen catalog tests and the accepted food exception.

## 3. Add authoritative recovery and income projections

- [x] 3.1 Publish a frozen optional Lumbermill recovery construction quote and add an explicit standard/recovery command payment choice defaulting to standard; implement zero-wood eligibility, atomic actual-payment investment and rejection for invalid modes, wrong buildings/non-build actions, and verify existing building transaction tests cover recovery, positive-wood refusal, refunds, stale instances and duplicate/retry behavior with `mise run test`.
- [x] 3.2 Publish nullable per-city production income from the authority's current frozen economy; extend existing projection tests for all resources, producer levels and build/upgrade/sale changes, disconnected living cities, fallen/terminal zero, unrepresentable custom income and atomic production overflow, verified by `mise run test`.
- [x] 3.3 Include recovery prices and changed economy semantics in rules identity, update ordinary runner command serialization and increment protocol compatibility; verify wire round trips, retained command defaults, precise accepted-retry payment, old-peer refusal and unchanged combat fingerprint through existing cheap tests.
- [x] 3.4 Document the explicit 1-wood normal and 4-gold zero-wood recovery choices, actual-investment refunds, synchronized income meaning and fresh-version compatibility in gameplay documentation; verify examples match transaction/projection tests and include no automatic currency substitution claim.

## 4. Present readable income and upkeep

- [x] 4.1 Build the three-column text-only Resource/Stock/Income table and compact two-row Upkeep table in one input-blocking panel; update `TabletopHud.cs`, `Tabletop.cs` and `ProgressionPresentation.cs` for exact supplied values, phase labels, shortage count, actual wave-tagged combat/outcome receipts and fresh-session reset, verified by existing cheap presentation tests for normal, shortage, empty-army, foreign-city, paused/disconnected and terminal states.
- [x] 4.2 Update HUD invalidation for projected income, producer capacity, recovery quotes and paused/stale wording; extend existing invalidation tests to verify accepted build/upgrade/sale, observed-city changes and reconnect refresh without depending solely on resource-balance changes, and run `mise run test`.
- [x] 4.3 Add producer output beside complete text costs, current-to-next upgrade output, explicit accessible missing amounts/source and the zero-wood recovery choice; verify pure presentation/eligibility tests and ordinary command quotes agree and disabled controls retain readable reasons.
- [x] 4.4 Fit the combined stack and inspector at both reference sizes while retaining the narrow top-right panel, approximately 180px bottom HUD, nine-plot overview and world input blocking; add observable full bounds/labels/values to existing DevRunner UI observation records and verify the records serialize through cheap runner tests.
- [x] 4.5 Update gameplay documentation for Stock, phase-aware income and visible Upkeep meanings, including food-sale consequences and combat receipts; verify documented labels agree with presentation tests and no preparation income is promised.

## 5. Verify the affected graphical slice and final integration

- [x] 5.1 Extend the existing `economy` UI scenario using fresh observations, owned setup and actual input to check displayed income after construction/upgrade/sale, explicit recovery, food-sale shortage before/after acceptance, paid combat receipt, observed-city/reconnect refresh and inspector bounds at 1100x820 and 1280x720; retain existing cooperative/economy assertions and run `mise run test-ui --scenario economy`, inspecting the captured PNG evidence. Record the targeted defects and added bounded observation/layout cost in `docs/verification.md`; introduce no new scenario or full graphical campaign.
- [x] 5.2 Complete a locked solution restore and format changed C# with `dotnet format Odot.slnx --no-restore`, then run affected cheap checks for the resulting changed inputs; verify formatting and repository diff checks pass without changing dependency/tool locks or tracking generated output.
- [x] 5.3 Run full `mise run ci` after the coherent implementation, covering cheap tests, existing network/source UI scenarios and sequential exports/package smoke; record timings, evidence paths and actual coverage in `docs/verification.md`, and verify every owned process/display is cleaned up. Re-run a passed check only for changed relevant inputs or a concrete unresolved failure.
