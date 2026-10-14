# Paid squad numerical baselines — provisional (2026-10-13)

These are **historical numerical measurements, not current balance or delivery
acceptance**. They precede the enemy-strength rebalance and use the original
100% enemy profiles. The three-preparation measurements below supersede, but
do not erase, the earlier four-edit measurements. See [wave power](wave-power.md#headless-encounter-evidence)
for current paired paid encounters; the losses below remain historical evidence.

## Exact scope and economics dependency

`PaidCampaignFixture` drove the production `Match` with the then-default rules, ordinary
accepted commands and fixed steps. It grants no money, materials, research,
health, members or waves. Every investment checks the actual projected quote,
resource delta, unchanged unrelated owners and persistent veteran identity/HP/
equipment. Every wave verifies per-member food receipts, exclusive reconstructed
squad claims, legal homogeneous purchased homes, individual Hall capacity and
survivor service XP. Stored members have no deployment pose or logical claim.
Commands and per-wave entry/exit states are retained as credential-free JSON.

Integrated battle flow2 permits **exactly three edits/ENDs**. The third END
settles income/healing/food and enters Combat immediately, with no fourth
post-income investment window. The historical checkpoint permitted that fourth
edit and therefore cannot certify integrated purchases or outcomes. No missing
income/edit was synthesized, and no price/damage/profile/production rule was
tuned to make the current measurements pass.

Selected horizon: default twenty waves, seeds **1 and 123**, at most 100,000
combat steps/run, 100 paid decisions/edit, three integrated readiness gates/wave and
120 ordinary cosmetic cleanup steps. Short Tier A paid witnesses stop earlier;
`RequestedHorizon` distinguishes those receipts. Full runs use `Tier=Deep` and
existing runner selection/admission. Numerical encounters are explicitly
arithmetic fixtures, not earned research or rendered proof.

## Historical initial three-preparation matrix (before recovery)

`test-campaign --filter 'FullyQualifiedName~SquadCampaignBaselineTests'`,
`logs/20261013-193823-db03264e`: **13 verification passes / 1 failure**, 35s
binary duration, fourteen complete numerical transcripts. These outcomes are
**ten twenty-wave victories and four genuine W1 defeats**, not thirteen wins.
The cooperative seed123 row additionally fails the required foreign-origin
impact witness. All four defeats are AllCitiesFallen. Default protocol15,
rules7/flow2, exactly three ENDs; existing policies/seeds/horizon unchanged.

| Policy | Seed | Outcome | Combat ticks | Individual casualties | Foreign impacts | Funded Hall healing (hundredths) |
| --- | ---: | --- | ---: | ---: | ---: | ---: |
| Frontline | 1 | Victory W20 | 4675 | 41 | 0 | 0 |
| Frontline | 123 | Victory W20 | 4600 | 38 | 0 | 0 |
| Mixed | 1 | Victory W20 | 4015 | 35 | 0 | 0 |
| Mixed | 123 | Victory W20 | 4426 | 37 | 0 | 0 |
| Ranged | 1 | AllCitiesFallen W1 | 794 | 1 | 0 | 0 |
| Ranged | 123 | AllCitiesFallen W1 | 763 | 1 | 0 | 0 |
| Cooperative rescue | 1 | AllCitiesFallen W1 | 2894 | 8 | 6 | 0 |
| Cooperative rescue | 123 | AllCitiesFallen W1 | 2025 | 8 | 0 | 0 |
| Research | 1 | Victory W20 | 4887 | 35 | 0 | 0 |
| Research | 123 | Victory W20 | 4951 | 38 | 0 | 0 |
| Reserve | 1 | Victory W20 | 4531 | 27 | 0 | 175255 |
| Reserve | 123 | Victory W20 | 4615 | 25 | 0 | 204720 |
| Towers | 1 | Victory W20 | 4679 | 36 | 0 | 0 |
| Towers | 123 | Victory W20 | 4862 | 44 | 0 | 0 |

Raw paid commands, payments, settlements, identities and entry/exit states:
`logs/shared-arena-task/battle-integration/final-qualification/campaigns/`.
The ranged opening now funds one Crossbowman, not the old two. Both cooperative
support owners fund four members; eight casualties are all eight actual members.
These measurements justify further earned policy/balance investigation, not
subsidies, extra spending gates, different seeds or relaxed rescue assertions.
No current paired restricted-home/tier/basic-Hall or final repeated-seed/stress/
package acceptance follows from this selective failed batch.

## Historical four-edit outcomes (superseded, retained evidence)

Health values are integer hundredths. Ticks are fixed simulation ticks, not
wall-clock time. `Injured` sums wounded survivors at each wave end, so the same
veteran may appear in several waves. Recovery is total real production healing
in hundredths; casualty counts are individual deaths, not lost squads. Full
receipts also retain lost member HP, per-wave purchases/stocks/equipment and
failure reason.

| Approach | Players | Seed | Actual outcome | Combat ticks | Casualties | Injured wave-end observations | Keep damage | Watchpost damage | Food paid | Hall healing |
| --- | ---: | ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Melee | 1 | 1 | Victory W20 | 4,615 | 37 | 175 | 0 | 0 | 324 | 0 |
| Melee | 1 | 123 | Victory W20 | 4,584 | 38 | 192 | 0 | 0 | 324 | 0 |
| Mixed | 1 | 1 | Victory W20 | 4,181 | 37 | 144 | 0 | 0 | 414 | 0 |
| Mixed | 1 | 123 | Victory W20 | 3,998 | 34 | 162 | 0 | 0 | 414 | 0 |
| Pure ranged opening | 1 | 1 | AllCitiesFallen W1 | 854 | 2 | 0 | 10,000 | 5,000 | 2 | 0 |
| Pure ranged opening | 1 | 123 | AllCitiesFallen W1 | 823 | 2 | 0 | 10,000 | 5,000 | 2 | 0 |
| Cooperative rescue approach | 3 | 1 | Team Victory W20 | 9,342 | 149 | 345 | 20,000 | 20,000 | 601 | 0 |
| Cooperative rescue approach | 3 | 123 | Team Victory W20 | 8,296 | 137 | 367 | 10,000 | 10,000 | 633 | 0 |
| Guardian research | 1 | 1 | Victory W20 | 4,771 | 35 | 188 | 0 | 0 | 298 | 0 |
| Guardian research | 1 | 123 | Victory W20 | 4,741 | 37 | 196 | 0 | 0 | 299 | 0 |
| Reserve/Hall | 1 | 1 | Victory W20 | 4,631 | 25 | 217 | 0 | 0 | 380 | 216,460 |
| Reserve/Hall | 1 | 123 | Victory W20 | 4,712 | 24 | 269 | 0 | 0 | 387 | 239,470 |
| Purchased Arrow/Catapult towers | 1 | 1 | Victory W20 | 4,954 | 41 | 153 | 0 | 0 | 245 | 0 |
| Purchased Arrow/Catapult towers | 1 | 123 | Victory W20 | 4,795 | 43 | 159 | 0 | 0 | 245 | 0 |

The diagnostic matrix's **14 passing verification rows mean 12 actual victories
and 2 actual defeats**, not fourteen winning campaigns. Pure ranged was an
unresolved paid-opening viability concern at this checkpoint: even the real two-Lumbermill opening
fields only two Crossbowmen, loses both, then loses its Watchpost/Keep. The first
attempt used the generic melee producer order, fielded no recruits and fell at
764 ticks for both seeds. Its failure is retained rather than called ranged
combat qualification. The later 854/823-tick losses are not hidden or tuned away.

Cooperative rescue means cross-owner fighting, not guaranteed preservation of
all Keeps. The third owner fields no army. Seed1 ends with owners1 and3 fallen;
seed123 ends with owner3 fallen. Surviving armies contribute **686 / 607 landed
member impacts against foreign-origin enemies** respectively. Those counts use
attacker `Impact` events and actual victim IDs; `Hit` projects the damaged member
and cannot supply attacker attribution. No graphical or actual spectator
transport claim follows from this numerical result.

## Historical four-edit paid investment controls

Same seeds, default prices and the same legal command driver; controls impose
choices rather than altering the simulator/economy.

| Choice | Seed | Outcome | Combat ticks | Casualties | Food paid | Keep / Watchpost damage | Hall gold / healed hundredths |
| --- | ---: | --- | ---: | ---: | ---: | --- | --- |
| Only two melee homes | 1 | AllCitiesFallen W7 | 2,282 | 11 | 42 | 10,000 / 5,000 | 0 / 0 |
| Only two melee homes | 123 | AllCitiesFallen W14 | 4,504 | 22 | 84 | 10,000 / 5,000 | 0 / 0 |
| Only L1 melee equipment | 1 | AllCitiesFallen W14 | 5,231 | 76 | 221 | 10,000 / 5,000 | 0 / 0 |
| Only L1 melee equipment | 123 | AllCitiesFallen W16 | 5,770 | 93 | 257 | 10,000 / 5,000 | 0 / 0 |
| Basic Hall, no track upgrades | 1 | Victory W20 | 4,676 | 29 | 414 | 0 / 0 | 5 / 133,300 |
| Basic Hall, no track upgrades | 123 | Victory W20 | 4,618 | 31 | 410 | 0 / 0 | 5 / 126,805 |
| Upgraded Hall | 1 | Victory W20 | 4,631 | 25 | 380 | 0 / 0 | 31 / 216,460 |
| Upgraded Hall | 123 | Victory W20 | 4,712 | 24 | 387 | 0 / 0 | 31 / 239,470 |

Normal melee victory, capacity bounds, actual equipment differences, no forbidden
Hall upgrades, paid Hall investment, actual healing and no stall remain acceptance
assertions. These controls demonstrate meaningful investment effects under
historical four-edit timing; they are not final price calibration. Upgraded Hall investment
is 31 gold, 8 wood and 9 stone, versus the basic Hall's 5 gold, 2 wood and 2 stone.

The old mixed target (10 Swordsmen plus 2 each of three other roles) would need
seven homogeneous homes despite the six-home maximum. The new target is three
Swordsman groups plus one group of each other role (9/3/3/3). No combat rule or
price changed. Retained old-policy mixed seeds1/123 won in **4,301 / 4,357 ticks**
with **56 / 57 casualties**; new paid six-home targets won in **4,181 / 3,998 ticks**
with **37 / 34 casualties**. Different investment/reinforcement transcripts mean
this is a policy comparison, not a combat-engine speedup.

## Initial role/research/recovery evidence (before recovery)

- Same three-member numerical Swordsman group, 73 ticks, same equipment/food:
  Guardian leaves **6,600 vs 6,000 HP hundredths**, with **6 vs 6** enemy member
  impacts. Reduced injury does not invent a slower enemy cadence.
- Same three-member Mage group, 73 ticks, same equipment/food: Frost leaves
  **4,500 vs 2,000 HP hundredths**, with **3 vs 6** enemy member impacts. Actual
  group chill/status/actions provide the difference, not member-local AI.
- Earned Fire witnesses retain real prerequisite/point payments, support priority
  only when compatible purchased homes and food fit, and locked Frost refusal.
  Two policy seeds record live burn by waves **8 / 7**, at production counts
  **24 / 21**. This is the historical four-edit schedule, not the integrated
  three-preparation qualification.
- A first-clear wounded veteran at **2,000/4,000 HP hundredths** cannot initially
  buy its Hall: insufficient earned stone refuses without mutation. A real END
  earns the material; the paid Hall costs **5 gold / 2 wood / 2 stone**, stores the
  same veteran without healing, and the next actual production heals **200**.
  Returning preserves identity, HP, equipment and service.
- Paid reserve seed123 reaches a legal Hall individual slot6 by the fifth wave.
  The original field-home lookup crashed there. Restoration **and** wave entry
  now keep stored actors off-grid, without relaxing field home indexes or
  creating occupancy, food participation or free recovery.

## Evidence and limits

The replacement/failure map and exact counts are in the
[testing audit](testing-audit.md#paid-strategy-research-and-provisional-campaign-replacement-2026-10-13).
Ignored evidence is under `logs/shared-arena-task/strategy-batch/`: full accepted
commands, current-wave-only food receipts, per-wave member injuries/HP loss,
healing/investment, structures, defaults and restricted choices. Earlier failures
and inaccurate initial attacker-counter interpretation remain explicitly retained
alongside the corrected receipt, not overwritten or used as final measurements.

These initial receipts do not establish final integration or delivery acceptance.
The recovery sections below own the later ranged, cooperative and solo policy
outcomes; the [testing audit](testing-audit.md#shared-arena-recovery-current-source-checkpoint-2026-10-13)
owns subsequent integration, native, transport, package and delivery receipts.
Finite policy samples are not an all-seed balance guarantee.

## Historical recovery ranged schedule (2026-10-13)

The initial candidate was shared HP35/X11, preserving range3, cadence60,
windup18, Y0, structural3 and equipment1metal/2wood/upkeep1. The ordinary
policy owns this schedule; actual quote checks remain in PaidCampaignFixture.

W1 gate1: Farm0 (2wood), MetalMine1 (2wood), Lumbermills3/4 (1wood each),
then the explicit zero-wood gold recovery Lumbermill2 (4gold). END1/2 each
produce3wood/4metal/5food and2basegold. Gate3 starts at6wood/8metal/10food:
sell Farm0 and MetalMine1 for1wood each and Lumbermill2 for0wood, construct
ArcheryRange2 for2wood, recruit three L1 Crossbowmen for6wood/3metal. END3
produces2wood from the retained mills, grants2gold and pays3food. No fourth
editing stage and no melee/grants are used. A clear adds2gold/1wood/5food.

If all three survive W1, W3 is the first planned paid full-force wave:
W2 starts at3wood, rebuild Farm0 for2wood; END1/2 add4wood, so five available
wood buy two recruits (five total). END3 plus clear leave4wood/27food/3metal
before W3, funding the sixth L1 member at its first editable gate. Extra wood
funds replacements only as available; trainer/upgrades wait until after W3.

Initial HP35/X11 execution (`logs/20261013-214453-8ca9f539`) lost W1 at both
seeds: three paid members, all casualties, Keep100HP lost, ticks1093/1274.
The erroneous W2 six-member expectation was corrected to the ledger above,
not counted as a win. The second bounded candidate, HP40/X12, was then selected,
with all other profiles/costs/timing unchanged; its results follow below.


### Owner-approved bounded ranged acceptance

The owner chose to prioritize the refactor, require full-six ranged survival and
defer the pure-Crossbow paid opening/progression requirement. HP40/X12 is final;
there is no further tuning. Both normal-profile six-member/two-squad W1 controls
clear with a living Keep and ranged survivors at seeds1/123. This fixture uses
explicit arithmetic equipment/food stocks and normal paid commands/upkeep, not
earned opening economy. Profile/faction/level/full-identity checks plus the
expanded existing encounter passed46/46 (`logs/20261013-214950-9120fc76`).

Actual paid HP40/X12 diagnostics (`logs/20261013-214629-a99ba82c`) remain:
seed123 loses all three members and Keep atW1/tick1393. Seed1 clearsW1/tick289
with three wounded survivors, clearsW2/tick673 but loses all five entrants,
then fallsW3/tick793 with only three paid replacements. No later full-six paid
force was earned. These defeats are deferred balance work, never a landing win.


## Historical funded cooperative recovery (2026-10-13)

The existing policy's `support` opening buys Farm0, MetalMines1/4 and explicit
zero-wood4gold Lumbermill3. END1/2 earn16metal/10food/2wood. Before END3, sell
temporary Mine4 for1wood, build Barracks2 for2wood and buy six Swordsmen for
12metal. END3 earns4metal/5food/1wood and charges6food. Wave2 replaces that
Mine with a Stonecutter; ordinary frontline progression then owns development.
No enemy count, price, seed, topology or combat routing was changed.

All-investing support at seed1 wins twenty waves at roster widths1/2/3/4, with
6/12/18/24 paid W1 members. Four rows passed in the bounded selection
`logs/20261013-215619-5e330302`; its fifth row failed only the test helper's
incorrect terminal-outcome expectation on a one-wave witness. That helper was
corrected to permit Building for a short requested horizon and require a defeat
reason only for actual Defeat; full-horizon terminal/rescue guards remain strict.
The four-owner witness then passed independently
(`logs/20261013-215909-44031d0e`).

Retained three-owner rescue seeds1/123 now have19/17 genuine W1 foreign impacts
and both strict guards passed (`logs/20261013-215350-29f82791`, whose separate
four-owner row exposed the short-horizon helper error). Seed1 winsW20; seed123
still loses atW19, an explicit finite-sample limitation, not a universal victory.
In the four-owner seed1 W1 witness, three supporters recruit18paid members and
the last owner intentionally invests nothing. It clears normally with15
attributed rescue impacts. At tick584 P3 members16/17/18 in squad6 physically
reach cell192 and hit enemy34 threatening P4 while its Keep has55HP. Travel
records retain every adjacent cell arrival and each landed event/member/victim
identity. This is earned help, separate from controlled active/stored ENet fixtures.
Final Keep survival is not substituted for that live impact predicate.


### Historical final funded solo controls

The same ordinary six-member opening now precedes optional infrastructure for
frontline/mixed/tower/research/reserve families. At seeds1/123 all ten controls
winW20; both ranged rows retain genuine losses (W3/W1). The twelve accounting
rows passed (`logs/20261013-220931-174f4cb1`), which means ten victories, not
twelve. Underfunded intermediate seed123 defeats remain historical diagnostic
evidence under `logs/shared-arena-recovery/solo-controls/`; a changed full
profile identity can rerank ties even in encounters without ranged enemies.
Prices/waves/seeds/routing remain unchanged; payment priority, not another
stat adjustment, restores funded controls.
