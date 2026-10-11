# Worldtree research in The Common Watch

## Reachable game feature

Open **Worldtree** in the city controls (the stable observation target remains
`Research`), without selecting a plot. Select a native socket by click or native
Tab/Enter, read its description below the tree, and press **Research** in the
selected-node footer. The footer shows the actual cost and authority-provided
reason. Acquired, prerequisite-locked, unaffordable and inactive nodes remain
inspectable. Foreign cities and disconnected/paused/ready/fallen cities are
read-only. Close/Escape restores the invoker when the window is focused. Settings
uses the existing settings owner; its child-modal guard prevents underlying
purchase/close callbacks. The game modal confines Tab/Shift-Tab; the imported
component itself still supports native traversal into/out of its host.

`ResearchPanel.cs` binds the game session to `WorldtreeDialog` and
`WorldtreeResearch.cs`. The native Control overlay is outside the city layout
containers. `GameApplication.IsModalOpen` already incorporates
`Tabletop.DetailsOpen`, so this panel also interrupts held movement and blocks
world picking/drag/zoom. The panel does not pause the cooperative match. A bounded
scroll host retains descriptions and the canonical 900×560 minimum component.
Physical control/branch identities survive selection, snapshots and resize.

## Bounded node design

This is an extension of existing **personal city research**, not a new skill-point
economy, hero class, respec system or generalized progression framework. The
existing 15 combat technology IDs, prices, prerequisites, exclusive choices and
benefits remain intact. Foundations cost 3 research points, specializations 6,
and masteries 9 by default. Foundations grant +5% health/direct damage to their
class. Guardian reduces incoming damage 10%/20%, Assault and Precision increase
direct damage 10%/20%, Fire burns at 20%/30% captured damage per tick, Venom
poisons at 10%/15% per stack tick, and Frost adds 20%/40% to future action duration.
Masteries replace their specialization's potency, rather than stacking it twice.
Research does not heal wounds or change unit levels.

Three optional village improvements occupy existing extra terminal slots:

| Name / stable technology ID | Prerequisite | Default cost | Actual benefit |
| --- | --- | ---: | --- |
| Stonecraft / `stonecraft` (16) | Guardian (and Melee Foundation) | 9 research | Each Stonecutter gains +1 stone per actual production at either level: 1→2 / 2→3 by default |
| Managed Forestry / `managed-forestry` (17) | Precision (and Ranged Foundation) | 9 research | Each Woodcutter hut (`Lumbermill`) gains +1 wood per actual production at either level: 1→2 / 2→3 |
| Irrigation / `irrigation` (18) | Frost (and Magic Foundation) | 9 research | Each Bakery (`Farm`, canonical food producer) gains +2 food per actual production at either level: 5→7 / 8→10 |

These production offshoots connect defensive masonry, ranged wood supplies and
water magic to existing useful village producers. They are late optional
investments (18 total points including the 3+6 prerequisite chain), available
independently of buying combat mastery. Their modest whole-resource increments
are per producer, additive across plots, neither retroactive nor free income
without a producer. Selling removes that producer's output, not the research;
rebuilding restores its researched output. Choosing Assault, Venom or Fire
permanently locks the corresponding Guardian, Precision or Frost production
offshoot, just as it already locks the other combat mastery. These are bounded
initial balance choices, not a newly claimed twenty-wave balance qualification.

The root and two trunk sockets are explicitly **established systems, not
purchases**: the village's existing base-gold production, one research point per
shared surviving-city clear, and research towers' existing progress→points
conversion. Their visual acquired state means these systems are established when
a city exists, not that a technology was purchased. They add no new acquired bits
or prerequisite gates to historical combat research. Three extra terminals are
truthfully **inactive**, with no price, benefit, availability or request.

### Fixed visual → authoritative mapping

`WorldtreeProjection.Slots` is the explicit mapping; artwork IDs never become
save/protocol IDs. No graph is rebuilt or inferred from an arbitrary catalog.

| Canonical visual ID | Gameplay identity |
| --- | --- |
| `root`, `trunk_1`, `trunk_2` | Established village, shared knowledge, research towers (not technologies) |
| `primary_left` | `melee-foundation` |
| `secondary_left_a` / `terminal_left_a_1` | `guardian` / `guardian-mastery` |
| `terminal_left_a_2` | `stonecraft` |
| `secondary_left_b` / `terminal_left_b_1` | `assault` / `assault-mastery` |
| `terminal_left_b_2` | Inactive |
| `primary_center` | `magic-foundation` |
| `secondary_center_a` / `terminal_center_a_1` | `fire` / `fire-mastery` |
| `terminal_center_a_2` | Inactive |
| `secondary_center_b` / `terminal_center_b_1` | `frost` / `frost-mastery` |
| `terminal_center_b_2` | `irrigation` |
| `primary_right` | `ranged-foundation` |
| `secondary_right_a` / `terminal_right_a_1` | `venom` / `venom-mastery` |
| `terminal_right_a_2` | Inactive |
| `secondary_right_b` / `terminal_right_b_1` | `precision` / `precision-mastery` |
| `terminal_right_b_2` | `managed-forestry` |

## Authority, deterministic effects and persistence

The component receives a complete snapshot: actual points, all 24 icon bindings,
selected visual ID, established/owned IDs and current available IDs projected
from the observed authoritative city. Its presentation-only action signal maps
back to a `TechnologyId` and calls the existing `IGameSession.SendAction` /
`research-tech` path. It never spends points, sets owned bits or applies effects.
Normal command results and success cues remain with the existing session owners;
the panel displays the session's real command feedback even when a rejection
changes no state revision. A rejection is not converted into a cosmetic purchase.

`Match` and `AuthoritySession` retain authenticated ownership, phase/turn,
readiness, pause, duplicate-sequence ledger and reconnect guards.
`SettlementCommands` validates the live catalog's affordability/prerequisites/
exclusive choice/acquired state before purchasing. Invalid acquired chains or
exclusive combinations are rejected; new bits append to the existing ulong
without renumbering historical IDs. Production remains an atomic preflight in
`Match`, with the same checked whole-resource arithmetic and overflow behavior.
`EconomyConfiguration.Production` applies the actual acquired per-producer
bonuses; `City.Snapshot.ProductionIncome` calls that same owner. Old combat
capabilities remain unchanged by production purchases. The economic fingerprint
includes new prices and effects; production-only nodes do not alter combat
fingerprinting/seed rules. No wire shape replacement, new RPC or new currency.

Existing research state and new optional catalog benefit properties travel
through the existing JSON/Brotli snapshot codec. Defaults preserve old research
state; no migration resets or wipes progress. Reconnecting to the same running
authority restores the complete city snapshot. **There is still no durable match
save, server-restart recovery or host migration**; only existing client session
credentials/preferences are saved. Serialization restoration tests are not a
claim of a new disk-save feature or mixed-version peer certification.

A newly acknowledged acquisition animates only its incoming module, with sap
before socket/foliage. Other modules and other tree instances retain independent
state. Identical tick snapshots are not resent to the component (replacement
would cancel its bound tween). Context changes reconstruct full current state;
there is no historical awakening replay on city/match switches. Replacement or
freeing cancels the owned tween. Wood, root details, sockets, icons, sap and
foliage remain the original native component, not a painted background substitute.

## Immutable import contract

Source: `https://github.com/seraph1nia/odot-game-assets.git`, actual merged main
`cadf0373c33dcde6858f59f2873575da8665be2d`, inspected read-only at
`/home/bart/.treehouse/odot-game-assets-86a3a5/1/odot-game-assets`.
The game's existing [authored manifest](../src/Game/Assets/Authored/manifest.json)
owns the separate `worldtree` pin, all 73 exact copied member SHA-256/byte counts,
and hashes of source API, Worldtree docs/validation, original art provenance,
portable payload inventory and current portable ZIP. All copied source members
are byte-identical; no asset repository edits or art rebuilds occurred.

Imported only the component scene/two scripts/UIDs, canonical JSON, 23 original
branch SVGs, separate root/foliage/socket/halo/twig/forest SVGs, 30 editable icon
SVGs, and the scene Theme's two SVG/font/license dependencies. Receiving-project
`.import` sidecars are generated by the locked Godot importer; its runtime caches
are not source payload. Both required JSON resources are explicitly included in
all four client export filters, alongside the font license. Dynamic illustrations
are covered by `all_resources`, then actually loaded by the package probe.
References, boards, generators, manual authoring sources, prototypes, showcase
scripts/tests, ZIPs and caches are not copied into game runtime.

Original editable SVG artwork is authored in odot-game-assets. Its source
provenance distinguishes original programmatic canonical handoff from AI-generated
concept boards; none of those boards is runtime artwork or a third-party paid
pack. Owner-requested integration is recorded without inventing an open-source
license. The unmodified DejaVu Serif font retains its bundled redistribution
license. The old compact 10-node asset-library tree and all its assets/tests remain
untouched and unimported.

## Verification and limits

Exact implementation commands, results, retained failures, changed source hashes,
resource/import identities and Linux executable/PCK/managed member identities
are in [worldtree-validation.json](worldtree-validation.json). The final delivery
pipeline must bind fresh checks to its actual published head; the initial dirty
worktree receipts are not relabeled as later-head qualification.

- Tier A: `WorldtreeResearchTests` exercises actual per-producer forecasts and
  income, purchases, duplicates, stale phase/turn, affordability, prerequisites,
  exclusive locks, invalid state/overflow, all three authority policies, retained
  authority reconnect, JSON/Brotli restoration and fixed projection gates.
- Tier B: `test-engine --scenario worldtree` uses the existing minimal owned
  headless SceneTree, loading both JSON resources, all 30 icons, actual snapshots,
  native selection/action signals, independent child awakening, unchanged-tick
  tween preservation, native Tab/Enter host escape/re-entry and modal confinement,
  Settings host callbacks, resize, disconnection and free-during-awakening.
- Package: `test-engine --scenario worldtree --exported` loads that same component
  owner from the newly exported client. Locked release templates prohibit scene
  overrides, so an early deferred `--engine-component-probe worldtree` dispatch
  selects the minimal scene **before** application/session/peer/renderer setup.
  Source/package CI runs the component owner; native Windows package execution is
  owned by the existing Windows CI job, not claimed from Linux.
- Bounded headless playing-host lifecycle protects existing ownership/retry/resume
  after the entry-point change. No new full-game journey is added for arithmetic.

**Unexecuted:** graphical Worldtree game appearance, screenshots, physical input,
native GPU/compositor/performance, listening, tiny windows, localization/RTL/HiDPI,
Steam paired invites, durable save/restart, deep twenty-wave balance and native
Windows execution locally. Settings itself retains its separate production
headless test owner; this component case verifies the actual host callbacks/guard,
not rendered child-window stacking. The retained optional graphical research
checkpoint's selectors follow this native panel, but it was not run. Standalone
asset captures and previous game's graphical receipts do not certify this game
integration.
