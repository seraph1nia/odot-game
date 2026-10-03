# Verification and calibration

## Complete gates

- Unchanged before CI: **292.32s**, `logs/20261003-185828-ba485153/`; console `logs/army-balance/before-ci.log`.
- Complete after CI: **342.11s**, `logs/20261003-223504-02c3e88e/ci-summary.json`; console `logs/army-balance/after-ci-second.log`. Locked restore, formatting, build/import, **481 gameplay / 172 runner tests**, all six network and five source UI scenarios, sequential Linux client/server exports, headless and graphical package smoke passed. Owned peer/display cleanup awaited; no publishing.
- Strict change validation and all **20** main specifications pass. The five synced capabilities retain all delta scenarios and unrelated main requirements. Tool/NuGet locks, assets/provenance and release version are unchanged; protocol **11**, combat rules **5**.

## Independently selectable feature evidence

`mise run test-ui --scenario economy --checkpoint army` owns a solo client, ordinarily paid two-wave setup, real purchase/inspector/hall input, full-destination rejection, exact home return, all-owned food, independent upgrades and three real production heals. It catches delivery/layout/authority defects cheap projections cannot; four frames retain the controls. First passing slice: **53.01s**, `logs/20261003-202840-302d3171/`. Complete economy includes that fresh slice and all pre-existing cooperative stock/food/tower/reconnect assertions: selected pass **162.95s**, `logs/20261003-214615-63814980/`; final CI economy **165.28s**. No graphical full campaign or option matrix was added.

## Paid choices

Checked-in `ArmyBalanceTests` executes **19** ordinary paid cases at seeds **0/1/123**; `logs/army-balance/calibration.log`, extracted `calibration-samples.json`. Twelve main cases compare frontline/reserve with **5/8/12/18** and **5/8/16/24**: all clear without shortage or stall. Selected bounded prices remain **5/8/12/18**; the steeper curve mostly delays expansion, not a demonstrated difficulty improvement. Candidate frontline casualties **65/62/68** versus reserve **45/51/50**. Reserve invests **43 gold** in hall/tracks, heals **2873.25/2619.70/2547.70 HP**, and Sends **41/41/38** while delaying last home to W**9/9/10** versus frontline W**6/6/6**. Two-home-only controls lose W**14/4/7**; tier-one controls W**7/13/13**. Seed-one reserve without hall upgrades still wins, so track investment is useful but not mandatory. Detailed per-wave resource, tier, attackers, casualties, retirements, rotation, eligibility and invariant measurements are summarized in `docs/verification.md`.

## Demonstrated fixture corrections, not weakened gates

- Economy seed **14056307608042553509**: checked-in cheap replay reproduces original defeat at **1551**, guessed paid Arrow defeat at **1732**, and disconfirms empty-home-only/material-without-recruitment fixes. Main-only growth loses the observer. Earlier ordinary producer setup, paid Mine upgrade, third homes and L2 recruitment clear at **1216**, both cities **100/100**, exact upkeep **7/9 food**. Preserve all clear/reward/food/stock/tower/cooperative assertions. Steering inbox 004 authorized this bounded diagnosis; no enemy/food/healing/overflow grants.
- Initial after CI stopped at reconnect's seventh mixed-role recruit in full homes, `logs/20261003-220109-cab115d5/`. Same-seed **2723540745024433499** cheap replay proves atomic rejection then affordable **5-gold** third-home acceptance, retained wounds/claims and exact **G20/W2/F24/M8 → G15/W0/F24/M7**. Owned reconnect passes **37.18s**, `logs/20261003-221143-f427122a/`.
- Combat paused tick **769** on a fresh corpse but no living wounded opponent in the focused city, then waited for damage while frozen. Cheap semantic controls prove both prerequisites; pause now requires their conjunction. Original freshness/pose, live damaged-enemy inspection, damage-or-casualty update and exact cleanup bounds remain. Owned full combat passes **91.95s**, `logs/20261003-222740-3b52c1a4/`, then **91.40s** in complete CI.

Only three paid replay/terminal hashes changed, causally from purchased allied setup/roster state. Isolated `route-target-ties-transfer` and `simultaneous-impact-death-queued-admission` hashes remain byte-exact. The seed-109 paid L3 cleared-frontage witness retains every cooperative admission/attack/ordinary-result assertion.

## Limits

This finite deterministic evidence establishes affordable paid growth and meaningful storage/retirement/upgrade choices, not universal difficulty or balance. No enemy, AoE, boss, food-rate, save, drag-and-drop or release framework is included. Owned Linux X11/software OpenGL/Dummy audio verifies rendered/input delivery, not native compositor/GPU performance, physical input or listening quality. Windows runtime and paired distinct-account/machine Steam acceptance are not claimed.
