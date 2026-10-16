# Milestone 1: roguelite replayability

Each match is a new twenty-wave run. The ordinary economy, persistent individual
soldiers, personal Worldtree, shared arena and 75%-baseline enemy profiles remain.
A run adds a personal starting doctrine, nineteen earned reward drafts and
seed-isolated variation of enemy entry ordering. These are numerical Core rules,
not presentation-only bonuses. No meta-progression or cross-match save is added.

## Choose a doctrine

Open the woodland **Doctrine / rewards** control in the lobby. Six native cards
show crest, category, benefits and drawback; **Choose** sends an authoritative
personal selection. The roster shows each owner's selection. The original host
still owns hosted Start. Graphical solo now waits for Start too, so its doctrine
can be chosen. Unselected owners default to The Warden; automated headless solo
still starts Warden immediately. Selections lock at start and allocations apply
once, never on reconnect or retry.

These are adjustments to the base 12 Gold, 6 Wood, zero other resources and 100
Keep HP. All resources, including Food, start at zero unless listed here.

| Doctrine | Category | Starting allocation and passive | Drawback |
| --- | --- | --- | --- |
| The Warden | Fortress / survival | +2 Stone; physical Watchpost +25% maximum HP | −2 Gold |
| The Merchant | Trade / flexibility | +4 Gold; first successful Market sale transaction each wave +1 Gold | −2 Wood |
| The Warmaster | Aggressive melee | +4 Metal, +6 Food; Swordsman/Berserker primary damage +10% | +1 Metal per Swordsman/Berserker recruit |
| The Arcanist | Magic / research | +3 Cloth, +3 Stone, +1 Research; Mage secondary damage +10% | −3 Gold |
| The Grovekeeper | Sustainable economy | +2 Wood; +1 Wood per production with any Lumbermill and +1 Food with any Farm, once each per city | +1 Metal per Swordsman/Berserker/Crossbowman, +1 Cloth per Mage recruit |
| The Huntsman | Ranged / positioning | +2 Metal; Crossbowman range +1 | Crossbowman maximum HP −10% |

Merchant applies once per transaction, not once per bundle, Market or owned
building. Failed sales consume no bonus. Doctrine equipment surcharges apply per
recruited member at every tier; authoritative quotes include them. Grovekeeper's
city bonuses are separate from the per-building production cards below.

## Earn and choose rewards

A successful shared clear of waves **1–19** pays the ordinary clear receipt and
then opens **RewardDraft**, before the next Preparation 1. Every living city has
three distinct authoritative offers. Choose exactly one. There is no production,
upkeep, recruitment, research-purchase or fourth spending window in this phase.
All twenty waves still have exactly sixty production settlements. Failure,
eliminated cities and final wave 20 do not receive drafts; ordinary final-clear
payment and victory remain.

The native **Choose reward** dialog shows current wave, rarity, complete effect,
stack limit, resource multiplier, chosen/awaiting state and upcoming enemy
forecast. The HUD/inspection exposes doctrine and acquired stacks. Close/Escape
never makes a choice; reopen through the HUD. A choice takes effect only after
an accepted snapshot, not optimistically on click. Paused, disconnected, stale,
foreign, unoffered and second choices are denied by the authority. Personal
command ledgers make retries exactly once. Snapshots retain offers, offer seed,
choice, doctrine, acquired stacks and battle-local run counters across ordinary
credential-bound reconnect to the same running authority.

Connected living cities must choose. Once they have, the authority gives each
absent living city its **first still-eligible offered card**, in offered order.
Disconnect alone can complete that barrier, including when everyone disconnects.
It does not start a new production or manufacture readiness. Shared pause also
pauses fallback; resuming reevaluates the barrier. Returning owners see the same
chosen card and do not receive another grant. This is deterministic fallback,
not host control of another owner's choice.

### Rarity, eligibility and stacking

Each offer slot rolls independently, removes the chosen ID, then rolls the next.
A seeded draw chooses within the rolled eligible tier. If that tier is empty,
choose the first nonempty allowed tier in Common → Uncommon → Relic order.

| Cleared wave | Common | Uncommon | Relic | Resource multiplier |
| --- | ---: | ---: | ---: | ---: |
| 1–4 | 75% | 25% | 0% | ×1 |
| 5–6 | 60% | 40% | 0% | ×1 |
| 7–9 | 60% | 35% | 5% | ×2 |
| 10–13 | 45% | 40% | 15% | ×2 |
| 14–19 | 45% | 40% | 15% | ×3 |

Relics never appear before cleared wave 7. Each relic is unique and a city may
hold at most two. At-cap cards and immediate effects that cannot apply safely
are excluded. Field Surgeons is excluded when both Keep and living army are
already full. Future recruit/unlock passives remain meaningful without the
matching building today; they don't grant free buildings or bypass unlocks.
Ancient Codex requires at least two remaining clears. Iron Harvest requires at
least one remaining wave with three enemy members across the original roster;
kills cannot accumulate across waves. Offers, selection and absent-player fallback
use these same campaign-aware checks.
If an offer pool cannot supply three distinct eligible cards, fail visibly rather
than silently duplicate or invent cards. Default finite runs retain three offers.

All same-stat run percentages **add before one scale**, rather than multiply per
card. Research foundation is the existing prior stage. HP, damage and periodic
potency use checked integer hundredths with positive ceiling, bounded products
and conversion; zero stays zero. Attack cadence reduces the **full cycle**, with
windup and recovery both remaining positive. Existing soldiers retain absolute
current HP when maxima increase, with no free healing, compounding or revival;
future recruits receive their resolved maximum. A lowered maximum clamps current
HP. A living Watchpost retains its absolute wounds after a maximum increase;
a destroyed one stays destroyed until the next ordinary battle reset. Purchased
towers remain invulnerable and acquire no new HP system.

## All 36 reward cards

`C/U/R` denote Common, Uncommon and Relic. Limits are per run and per city.

| ID | Card | Tier / limit | Exact effect |
| --- | --- | --- | --- |
| E01 | The Lumber Guild | C / 1 | Every Lumbermill +1 Wood per production |
| E02 | Bountiful Harvest | C / 1 | Every Farm +1 Food per production |
| E03 | Golden Veins | C / 1 | Every Gold Mine +1 Gold per production |
| E04 | Master Stonecutters | C / 1 | Every Stonecutter +1 Stone per production |
| E05 | Deep Mining | U / 1 | Every Metal Mine +1 Metal per production |
| E06 | The Weaver's Pact | U / 1 | Every Weaver +1 Cloth per production |
| A01 | Hardened Shields | C / 2 | Swordsman maximum HP +10% per stack |
| A02 | Berserker's Fury | C / 2 | Berserker primary damage +12% per stack |
| A03 | Precision Fletching | C / 2 | Crossbowman primary damage +10% per stack |
| A04 | Arcane Barrage | C / 2 | Mage secondary damage +12% per stack |
| A05 | Veteran Drills | U / 1 | Every friendly soldier maximum HP +5% |
| A06 | Efficient Rations | U / 1 | Berserker/Mage upkeep −1 Food per funded member, minimum 1 |
| T01 | Reinforced Watchposts | C / 2 | Physical Watchpost maximum HP +20% per stack |
| T02 | Sharpened Shafts | C / 2 | Arrow/Rapid Arrow damage +10% per stack |
| T03 | Heavy Ammunition | C / 2 | Catapult/Bombard primary and secondary damage +10% per stack |
| T04 | Eagle's Eye | U / 1 | Arrow/Rapid Arrow range +1 |
| T05 | Rapid Mechanisms | U / 1 | Arrow/Rapid Arrow full cadence −10% |
| T06 | Quick Winches | U / 1 | Catapult/Bombard full cadence −10% |
| M01 | Spell Focus | C / 2 | Mage primary damage +10% per stack |
| M02 | Arcane Endurance | C / 2 | Mage maximum HP +12% per stack |
| M03 | Frost Runes | C / 1 | Mage Chill +10 percentage points, cap 50%, existing duration |
| M04 | Ember Sigil | U / 1 | Mage Burn +10 percentage points, cap 40%, existing duration |
| M05 | Venom-Tipped Bolts | U / 1 | Crossbowman Poison +5 percentage points, cap 20%, existing stack cap/duration |
| M06 | Elemental Conduit | U / 1 | Friendly primary damage +8% if target squad already has Burn, Poison or Chill before impact; once regardless of status count |
| F01 | Timber Shipment | C / 2 | Immediately +4 Wood × multiplier |
| F02 | Emergency Provisions | C / 2 | Immediately +8 Food × multiplier |
| F03 | Equipment Shipment | C / 2 | Immediately +3 Metal and +2 Cloth × multiplier |
| F04 | Field Surgeons | U / 1 | Immediately heal Keep up to 10 HP and each living owned soldier by 20% of missing HP, including storage/reserve, no resurrection |
| F05 | Forgotten Knowledge | U / 1 | Immediately +2 personal Research, no multiplier |
| F06 | Royal Treasury | C / 2 | Immediately +5 Gold × multiplier |
| R01 | Echoing Warhorn | R / 1 | First committed attack action of each friendly squad per battle: primary and secondary +25% |
| R02 | Ashen Quiver | R / 1 | Crossbowman hits also Burn for 8% of actual damaging hit, existing Burn timing |
| R03 | Iron Harvest | R / 1 | Successful clear: +1 Metal per 3 enemy members killed by that city's soldiers, cap 5 Metal |
| R04 | Crystal Prism | R / 1 | Mage secondary damage +20% |
| R05 | Ancient Codex | R / 1 | +1 Research every second successful clear after acquisition; acquisition clear excluded |
| R06 | Guardian Roots | R / 1 | Keep structural damage −15% while its actual Watchpost is alive |

Production cards add to Worldtree's corresponding per-building increments.
Status cards grant their application even without the exclusive research branch;
research exclusions stay intact. Ashen Burn and Venom Poison can coexist. Conduit
samples the whole enemy squad's **pre-impact** statuses; applications from that
same simultaneous tick do not create its bonus. It affects primary damage only.
All Mage/Catapult secondary damage remains inside the engaged enemy squad, never
an adjacent squad or area-radius attack.

Warhorn is one committed **squad action**, including that action's participating
members, not a separate bonus on every member's later attack. Battle reset clears
it. Lethal ticks apportion actual lost HP proportionally across simultaneous
contributions (largest remainders, stable attacker identity ties). Iron Harvest
credits the greatest actual applied contribution, lowest attacker member ID on a
tie; periodic potency retains its source owner. Weapons use separate deterministic
nonmember identities and don't earn soldier kill credit. One victim pays at most
one city. The clear receipt includes Harvest Metal and extra Codex Research.
Readiness preflights ordinary rewards, possible Harvest and the next Codex point
before paying anything; overflow never silently discards an earned card bonus.
Roots checks Watchpost state after same-tick structural damage: destruction removes
protection immediately; normal next-wave reset makes it available again.

## Seeded encounter boundary and compatibility

The authority keeps a run seed and independent pure reward/encounter namespaces.
Rewards key on cleared wave and stable city identity. Encounter variation keys on
wave/original allocation and permutes authored `SpawnEntry` ordering. It changes
formation/order only: **same member counts, types, levels, bosses, original-player
pressure and total numerical budget**, including inherited fallen-city pressure.
Single-entry waves cannot vary their order. This deliberately bounded first
milestone does not add new enemies, composition budgets, difficulty multipliers
or cosmetic RNG to combat. Existing combat decision RNG remains unchanged and unconsumed by drafts.

`MatchSnapshot.NextWave` is the reusable authority projection consumed by preview
and combat entry. Reconnect, choice arrival order and selected reward effects do
not reroll the encounter. Presentation must map its canonical identities and
coordinates; it must not assume a fixed roster or vertical world-space layout.

Protocol **16 / combat rules 7 / battle flow 3** refuses incompatible peers; there
is no saved-run migration or server-restart recovery. Catalogs participate in
rules identity. No dependency, asset provenance, tool lock or renderer changes
accompany this milestone. See [verification](verification.md)
for exact executed coverage and the explicitly unexecuted graphical/platform
acceptance boundary.
