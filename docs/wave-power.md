# Wave power: 25% lower enemy strength

## Baseline and rule

The baseline is landed shared-arena revision
`4430c3ed1d2de402ae40f404d82404036e076959`, **before** this rebalance.
`tests/Game.Core.Tests/WavePowerBaseline.cs` freezes its twenty compositions,
levels, rewards and combat coefficients, independently of the current default
catalog. Baseline runs explicitly use enemy strength 100%; current runs use 75%.
Evidence filenames include that percentage so one cannot overwrite the other.
This is not a comparison against already-reduced defaults.

`WavePower` is a small additive balance score, not a simulator or a prediction
of victory. One original level-one Swordsman is one point. For an individual
member with actual leveled HP `H`, primary damage `D`, and cadence `T` ticks:

```text
member power = (H / 40 + (D / T) / (10 / 60)) / 2
wave power   = original player count × sum(entry member count × member power)
```

HP measures durability; damage/cadence measures sustained primary offense. The
50/50 weighting gives neither axis an extra bonus. The resolved profile applies
level growth `1.35^(level-1)`, original-base whole-stat rounding, and the existing
boss/rank rules. There are no invented range, initiative, splash-victim, research
or adaptive-difficulty modifiers. Ranged reach and Mage within-squad splash still
matter in real combat; their situational value is deliberately not simulated.
Counts are **individual members**, not squads (capacity three). Co-op multiplies
by the original 1–4-owner roster, including pressure inherited from fallen owners.

The resulting level-one archetype modifiers, relative to the original Swordsman:

| Type | Original HP | Primary damage / cadence | Base power modifier | Role rationale |
| --- | ---: | ---: | ---: | --- |
| Swordsman (S) | 40 | 10 / 60 | 1 | Reference melee |
| Berserker (B) | 30 | 15 / 72 | 1 | Less durability, more primary offense |
| Crossbowman (C) | 40 | 12 / 60 | 1.1 | Same durability, 20% more primary offense |
| Mage (M) | 25 | 12 / 90 | .7125 | Fragile, slower primary cycle; splash is situational |
| Swordsman boss (!) | 320 | 20 / 60 | 5 | Existing 8× HP and 2× damage, one member |

At later levels use the actual rounded stats, not a previously rounded score
multiplied repeatedly. For example S3 has 73 HP and 18 damage: power 1.8125,
not exactly the nominal 1.8225 growth contribution. Boss weighting likewise
uses its actual stats, rather than assuming its multiplier is always 5× an
ordinary leveled score.

`Rules.EnemyPowerPercent = 75` scales **only Skeleton** max HP and primary,
secondary and structural damage once, after normal profile resolution. Counts,
levels, ranks, timing, range, grouping, player stats/technologies, prices, food,
production and rewards do not change. This keeps all roles and single-member
bosses intact; taking 25% off counts cannot do that (four archetypes at W4,
or one boss, cannot be rounded into fractional members).

The score is linear in HP and damage. Therefore:

```text
P_after = sum(n × (.75 H/40 + .75 (D/T)/(10/60))/2) = .75 P_before
```

**This does not mean exactly 25% less experiential difficulty.** Scaling BOTH
HP and damage can compound combat outcomes: an HP×DPS threat product becomes
`.75² = .5625` (43.75% lower), and casualty thresholds/first shots/targeting can
change dramatically. The chosen score is the explicit additive budget requested
here, not that product and not a playability certificate. Paid encounters below
measure actual outcomes separately rather than relabeling the score as experience.

## Every configured wave

`n Sℓ` means `n` actual members at equipment level `ℓ`. Composition, levels,
base archetype modifiers above, and member counts are identical before/after;
the enemy strength modifier is **100% → 75% on every row**. Powers are per
original owner; multiply both columns by 2/3/4 for co-op, with the same change.
Values displayed to six decimals; tests compare to twelve decimal places.

| Wave | Before = after members/levels | Count | Before power | After power | Change |
| ---: | --- | ---: | ---: | ---: | ---: |
| 1 | 4 S1 | 4 | 4.000000 | 3.000000 | -25% |
| 2 | 3 S1, 1 B1 | 4 | 4.000000 | 3.000000 | -25% |
| 3 | 3 S1, 2 C1 | 5 | 5.200000 | 3.900000 | -25% |
| 4 | 3 S1, 1 B1, 1 C1, 1 M1 | 6 | 5.812500 | 4.359375 | -25% |
| 5 | 3 S2, 1 C1 | 4 | 5.225000 | 3.918750 | -25% |
| 6 | 3 S2, 1 B2, 1 C1 | 5 | 6.570833 | 4.928125 | -25% |
| 7 | 3 S2, 2 C2, 1 M1 | 6 | 7.787500 | 5.840625 | -25% |
| 8 | 3 S2, 2 B2, 2 C2, 1 M2 | 8 | 10.725000 | 8.043750 | -25% |
| 9 | 3 S3, 1 B2, 1 C2, 1 M2 | 6 | 9.216667 | 6.912500 | -25% |
| 10 | 1 S3! | 1 | 9.100000 | 6.825000 | -25% |
| 11 | 3 S3, 2 C2 | 5 | 8.387500 | 6.290625 | -25% |
| 12 | 3 S3, 1 B3, 2 C3 | 6 | 11.275000 | 8.456250 | -25% |
| 13 | 3 S3, 2 C3, 1 M3 | 6 | 10.770833 | 8.078125 | -25% |
| 14 | 3 S3, 2 B3, 2 C3, 1 M3 | 8 | 14.395833 | 10.796875 | -25% |
| 15 | 3 S4, 1 C3, 1 M3 | 5 | 10.745833 | 8.059375 | -25% |
| 16 | 3 S4, 1 B4, 2 C4 | 6 | 15.341667 | 11.506250 | -25% |
| 17 | 3 S4, 2 C4, 1 M4 | 6 | 14.650000 | 10.987500 | -25% |
| 18 | 3 S4, 2 B4, 2 C4, 1 M4 | 8 | 19.583333 | 14.687500 | -25% |
| 19 | 3 S5, 1 B4, 2 C4, 1 M4 | 7 | 19.629167 | 14.721875 | -25% |
| 20 | 1 S5! | 1 | 16.600000 | 12.450000 | -25% |

### Rounding, minimums, bosses and receipts

Stats use hundredth-HP integer fixed point. Every default rank-zero leveled stat
is a multiple of 100, so multiplying by 3/4 is **exact**, including fractional
whole HP: S1 becomes 30 HP / 7.5 primary / 3.75 structural; S3 boss becomes
438 HP / 27 primary; S5 boss becomes 798 HP / 49.5 primary. Zero channels remain
zero. Boss 8×/2× relationships, one-member/one-squad architecture, level progression
and all non-stat rules remain unchanged. There is no minimum-count exception.

Custom percentages/ranks/capabilities can produce non-divisible fixed-point
amounts: integer division discards less than .01 HP/damage per channel; positive
health has a one-fixed-point minimum. Such custom inputs need not be exactly 75%
in this model. All configured waves are rank zero and have no such error. Tiny
repeating-decimal cadence arithmetic is not a gameplay rounding difference.

Forecast and live creation share faction-aware profile resolution. Cache keys
include faction; received wounded HP/profiles are preserved, not scaled again.
Rules include the percentage in full compatibility identity but **not** actor
random-choice identity, preserving deterministic seeded namespaces. Reloaded
rules keep 75 rather than stacking another reduction. Missing historical
strength fields are refused, not silently reinterpreted as a new default.

## Headless encounter evidence

The existing `PaidCampaignFixture` issues ordinary accepted commands, validates
actual prices/production/upkeep/individual continuity, and drives production
fixed steps. No grants, HP setters, alternate economy or simulator are used in
these balance measurements. First-wave commands and player rosters/profiles are
identical across each pair, everyone is fed, and subsequent investment responds
to actual earned outcomes. No acceptance assertions were relaxed.

The seven bounded opening pairs measured:

| Paid policy / owners / seed | W1 ticks before → after | Lost soldier HP | Deaths | Keep HP lost | Cleared horizon |
| --- | ---: | ---: | ---: | ---: | --- |
| Frontline / 1 / 1 | 253 → 193 | 120 → 67.5 | 0 → 0 | 0 → 0 | 3 → 3 |
| Frontline / 1 / 123 | 343 → 223 | 130 → 52.5 | 1 → 0 | 0 → 0 | 3 → 3 |
| Ranged / 1 / 1 | 289 → 229 | 90 → 37.5 | 0 → 0 | 0 → 0 | 1 → 1 |
| Ranged / 1 / 123 | 1393 → 289 | 120 → 75 | 3 → 0 | 100 → 0 | 0 → 1 |
| Support / 2 / 1 | 254 → 253 | 240 → 137.5 | 1 → 1 | 0 → 0 | 3 → 3 |
| Support / 3 / 1 | 313 → 194 | 360 → 195 | 7 → 0 | 0 → 0 | 3 → 3 |
| Support / 4 / 1 | 343 → 223 | 510 → 210 | 7 → 0 | 0 → 0 | 3 → 3 |

Each current opening has strictly less first-wave soldier HP loss, no additional
casualties or Keep loss, and at least the baseline progress. This is a finite
numerical easier-opening witness, not a universal strategy or rendered-play test.
The ranged seed123 loss remains in the baseline; the current paid victory does
not erase earlier [paid squad diagnostics](paid-squad-baselines.md).

Selected later paid melee/mixed/ranged horizons and exact execution receipts are
recorded in [the verification audit](testing-audit.md#wave-power-rebalance-2026-10-14).
Broader repeated-seed twenty-wave balance, arbitrary poor openings and experiential
graphical acceptance remain outside this qualification. No graphical tests,
asset changes, release, installation or player-side compensation accompany it.
