# Paid squad numerical baselines — provisional (2026-10-13)

These are **numerical measurements, not final redesigned balance or delivery
acceptance**. The landed three-preparation/speed/preview prerequisite is now
integrated in the working tree. The current integrated measurements below
supersede, but do not erase, the historical four-edit measurements.

## Exact scope and economics dependency

`PaidCampaignFixture` drives the production `Match` with default rules, ordinary
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

## Integrated default-price matrix: incomplete balance qualification

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
and 2 actual defeats**, not fourteen winning campaigns. Pure ranged remains an
unresolved paid-opening viability concern: even the real two-Lumbermill opening
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

## Paid investment controls

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
current timing; they are not final price calibration. Upgraded Hall investment
is 31 gold, 8 wood and 9 stone, versus the basic Hall's 5 gold, 2 wood and 2 stone.

The old mixed target (10 Swordsmen plus 2 each of three other roles) would need
seven homogeneous homes despite the six-home maximum. The new target is three
Swordsman groups plus one group of each other role (9/3/3/3). No combat rule or
price changed. Retained old-policy mixed seeds1/123 won in **4,301 / 4,357 ticks**
with **56 / 57 casualties**; new paid six-home targets won in **4,181 / 3,998 ticks**
with **37 / 34 casualties**. Different investment/reinforcement transcripts mean
this is a policy comparison, not a combat-engine speedup.

## Focused role/research/recovery evidence

- Same three-member numerical Swordsman group, 73 ticks, same equipment/food:
  Guardian leaves **6,600 vs 6,000 HP hundredths**, with **6 vs 6** enemy member
  impacts. Reduced injury does not invent a slower enemy cadence.
- Same three-member Mage group, 73 ticks, same equipment/food: Frost leaves
  **4,500 vs 2,000 HP hundredths**, with **3 vs 6** enemy member impacts. Actual
  group chill/status/actions provide the difference, not member-local AI.
- Earned Fire witnesses retain real prerequisite/point payments, support priority
  only when compatible purchased homes and food fit, and locked Frost refusal.
  Two policy seeds record live burn by waves **8 / 7**, at production counts
  **24 / 21**. This schedule is provisional on the unlanded phase flow.
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

No final three-preparation/pace/preview qualification, complete deep suite,
full-fast green, all-seed balance guarantee, new transport/package/native aiming/
animation/resource check, graphical process, full pipeline or publication is
claimed. Pure-ranged viability and cooperative Keep losses need explicit follow-up;
final economics-sensitive calibration awaits the actually landed prerequisite.
