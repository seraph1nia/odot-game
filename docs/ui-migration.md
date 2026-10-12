# Live woodland UI migration

## Source and runtime boundary

The game consumes the accepted native kit from `odot-game-assets`, incorporating
its woodland HUD/menu presentation and the landed living Worldtree, not the
superseded flat Ledger palette. The receiving runtime selects 22 original kit
members; eight unused portable component triples, including the compact local
allocator, are not shipped. The [original 46-member inventory](asset-history/woodland-ui/README.md)
preserves exact upstream hashes and provenance separately, without upstream edits.
The [kit manifest](../src/Game/UI/manifest.json) owns
the exact source revision, selected member hashes and source-relative paths,
API/inventory/migration/coverage/woodland bindings, theme generator, original seal
source, reference hashes and catalog package identity. See
[notices and acquisition boundary](../src/Game/UI/README.md),
[woodland provenance](../src/Game/UI/woodland-art.json) and the complete
[font license](../src/Game/UI/fonts/LICENSE-DejaVu.txt). The additional Worldtree
members retain their separate exact asset-main revision/inventory under
[the authored manifest](../src/Game/Assets/Authored/manifest.json) and
[Worldtree import contract](worldtree.md#immutable-import-contract); the original
46-member kit provenance is not expanded or relabeled to hide that second binding.

Game import/export and live UI checks are the receiving-project qualification;
source-member integrity does not establish runtime acceptance. `hud.png` remains
upstream reference context, not runtime artwork. The menu seal remains the
original 192px transparent PNG, fixed at 48px; vignette and atlas icons retain
aspect. Branch slices stay 40px, parchment slices 24px horizontal/16px vertical,
blue focus remains independent of ownership/selection. Godot imports SVGs/font locally without installing tools.

## Discovered live consumers

| Surface | Actual consumer and binding | Authority/services retained |
|---|---|---|
| Start/multiplayer menus | `GameApplication`: woodland Theme, original seal, pointer-inert vignette, primary/danger variants, cream serif labels and dark branch pane | Existing solo, Steam host/offline feedback, navigation, music and Exit |
| Settings/graphics/audio/about | `ClientSettings`: production `modal.tscn` via game-owned `UiDialog`, native tabs/sliders/dropdowns/scroll, bounded body and stable action/footer composition | Current renderer capability gates, live Apply/Cancel/reset, display preview/rollback, transient CLI overrides versus committed preferences, volume/update/save services |
| Invite/leave confirmations, friends | `GameApplication`, `SteamFriendsDialog`: same Control shell and native controls | Existing lifetime/generation guards, Cancel-default, invitation query/send identities, busy state and async invalidation; no local Steam lobby/invitation during verification |
| Six-resource HUD and upkeep | `TabletopHud`: production `resource_ledger.tscn`; game binds its native stock/income labels and appends authoritative upkeep/context rows | Existing exact six-resource order, economy projection, forecast/receipt distinctions; no kit economy calculation |
| Village/building/economy/market/recruitment | `TabletopHud`, `Tabletop`, `UiAssets`: shared native woodland styles, textual costs/reasons/quotes, native groups and selectors | Original plot/ownership/phase/ready/connection checks, server costs/eligibility, build/upgrade/sale/trade/recruitment semantics and opaque command identities |
| Research | `ResearchPanel`, `WorldtreeResearch`, `WorldtreeDialog`: imported living tree, native inspect/purchase Controls, descriptions and focus-follow scroll | Landed protocol-12 catalog and labels, actual 3/6/9-point foundations/specializations/masteries and village production offshoots, permanent sibling locks, authoritative points/requirements and session `research-tech` action |
| City Details, Town hall/army roster | `TabletopHud`, `ArmyPanel`: native production modal, scrollable live summaries/roster/track controls | Existing physical tile capacities, paid recovery, storage/healing independent quotes, stale hall generation and occupied-sale/transfer/retirement guards |
| Unit inspection | `UnitInspector`: inherited woodland Theme and native health/scroll/actions | Existing real model preview, picking, level/profile/status clocks and authoritative store/retire requests |
| Match/lobby/reconnect/combat/feedback | `Tabletop`: native woodland Ready green/Pause violet and standard text controls, branch-framed HUD | Existing start/readiness/cooperative pause/reconnect/fresh-session services and authoritative phase counters; settings never pauses the match |
| Projected unit/home health and locked land/home markers | `UnitHealthBar`, `HomeHealthBar`, `UiAssets`: native Theme progress styles and original resource atlas cells | Same bounded camera projection, Roman levels, exact percentage/fraction and input-transparent clipping; no restored ambient labels |
| Tooltips/progress/scroll/native popups | Shared production Theme and existing native widgets | Native tooltip/input/wheel semantics and engine-owned widgets/system fallbacks, not a replacement framework |

`UiDialog` is a receiving-game adapter to the existing production Control shell,
not a competing framework or session/settings service. The shell owns visible
Tab/Shift-Tab/Esc, popup-first handling, scrim, native scroll and synchronous
invoker return. The adapter validates invokers on each opening, promotes the
active dialog by sibling order and keeps directional focus inside the foreground
scope. Before keyboard activation it repairs missing or external focus, while
owned native popups retain their own routing. Layout-only resize reflow must not
reopen, promote or refocus a dialog or reactivate a suspended input scope.
Friends/Settings deferred restoration runs only when no foreground sibling modal
remains, including fallback restoration. These receiving-boundary regressions
live in `src/Game/Verification/EngineTests.Ui.cs`.
The game still explicitly blocks picking/held-world gestures/commands while a
modal is open. Leave confirmation temporarily suspends the underlying Settings
key scope; closure restores it. Join confirmation suspends the Tabletop research
host by hiding its scope, preserving the selected native focus target; every
invitation dismissal restores that scope before the existing service callback.
The Tabletop research visibility boundary refreshes the current accepted session
projection on both ordinary opening and restoration, before focus returns; a
purchase control newly disabled by that snapshot uses the existing root fallback.
The receiving fixture constructs the actual application invitation above the
later HUD subtree, without starting the application or using Steam credentials.
Dialogs are outside layout containers under the
application's viewport-sized overlay. Tabletop teardown frees its owned overlays.
Settings footer composition remains extensible for actual later controls; no
Forest Theme/Magical Effects feature or speculative settings service is added.

### Research adaptation, not an invented allocation economy

The upstream portable compact tree has a one-point shared root and exactly three
independent chains, with local allocation. The actual game instead has three
class foundations costing three points each, each with two exclusive
specialization/mastery chains costing six/nine points. Feeding game data to that
allocator would change rules or pretend it matches. It is therefore excluded
from the receiving runtime, with its original hashes retained in the historical
inventory and its canonical upstream source unchanged.
The landed Worldtree instead projects the actual catalog and acknowledged acquired
set into the immutable living tree; structural roots are established systems,
not purchases. Locked/owned nodes remain inspectable; the native purchase control
and production session bridge independently enforce real eligibility. Full
prerequisite, sibling-lock, price and benefit copy remains in descriptions/tooltips.
The landed village production offshoots and protocol-12 compatibility boundary
remain unchanged by this migration. There is no new skill currency, one-point
root, respec, local spend, prediction or research rule invented to fit a demo.

## Removed-resource evidence

The tracked files under `src/Game/Assets/TrioUI/` (selected Cozy/icon/derived
SVGs, import recipes and pack notices/manifest) are removed after replacing every
live consumer and fallback. The manifest's `supersededFiles` owns their exact
old path/SHA-256 inventory; cheap inventory tests require every old member absent
and every copied production member byte-identical. Native presentation assertions now sample actual loaded styles/font/seal/icons/progress
and distinct hover/pressed/disabled/focus/tab states, rather than asserting old
Trio paths. Existing action/control/economy/cooperative/renderer assertions and
scenario admission/deadlines are not weakened. Old UI notices remain attributable
in [the historical notice archive](asset-history/trio-ui/README.md) and git history;
no unrelated 3D assets, licenses, music, engine widgets, geometry or
simulation/network/save rules are removed. Live correctness is established by
receiving-game interaction/loading checks, not this path inventory alone.

## Verification and limitations

Initial unchanged-source full `mise run ci` ran on
`d7f1728b17cf01e90bfa2f7eee6c49aa4606c9fa` and failed after 949.24s in existing combat
casualty admission: the match advanced from wave 3 Combat to wave 4 Building before
an eligible current pause could be admitted. Evidence remains under
`logs/20261011-071927-7242f8e8/`; this is a baseline failure, not a UI regression or a
runner crash. No blind retry, gameplay change or weakened casualty assertion was
used. The first migration Settings attempt retained native-path errors caused by
renaming kit-owned footer controls plus a missing not-yet-written bundled README;
those receiving-adapter issues were corrected without changing upstream resources.

Final cheap gameplay/runner suites passed in 39.28s
(`logs/20261011-081324-89985bcb/`): 504 core and 636 runner tests; two existing
Windows-specific runner tests were skipped on Linux. Locked restore, strict build,
formatting and exact production/removed-member integrity checks passed. Final
`dotnet format --verify-no-changes --no-restore` passed; OpenSpec strict validation
passed all 27 current items. These documentation-only receipt updates reuse the
unchanged relevant runtime/member bytes; they are not new engine execution. The initial
corrected Settings slice passed in 176.21s
(`logs/20261011-074558-e3852546/`), including actual native menu selection, graphics
Apply/Cancel/reset/capability/CLI-override/persistence, world input containment,
keyboard, window resizing and restart. Subsequent layout/source edits require fresh
relevant evidence; that first selected pass is not final-head acceptance.

### Current headless-only qualification (captain policy, 2026-10-11)

The later pipeline Test receipt at `01c05c8` includes a tool-acquisition incident.
The [retained incident audit](toolchain-incident.md) separates earlier selected
passes, failed/download-touched attempts and the final package consumer. Current
locks are restored, but residual tool-state and final-input provenance remain
unresolved; that receipt is not publication clearance.

After the captain prohibited tests that launch the actual graphical game, no more
source/exported graphical journeys, displays, captures or old full-CI tasks were
started. Completed owned workers/game PIDs were checked absent and their runtime/
display scopes removed; the interrupted second launcher command never launched.
Earlier evidence remains historical/partial, **not final-source visual acceptance**.

The first launcher migration attempt (`logs/20261011-075111-9a32d3d1/`) reached
actual menu/multiplayer captures but failed to reach the Audio slider: Settings
was created before the later menu sibling. Canvas z-index draws the modal above
it but does not reorder GUI picking. `UiDialog` now moves the active scope to the
front. The focused headless regression creates exactly that later blocking sibling
and proves actual Settings Audio-tab pointer input plus native slider keyboard
input reach the intended controls. No blind live-game rerun followed the failure.

The former `mise run test-ui-resources` fixture recorded a historical pre-rebase
pass of **38 checks** in 8.19s (`logs/20261011-081831-3e821258/`): all ten production
scenes, actual branch/font/seal,
four native button states and blue focus, opaque quote intent and disabled reason,
real-catalog bounded compact node footprints/prerequisite direction for all three
classes, modal pointer/Tab/Esc/outside blocking/front-sibling routing, actual Settings
composition/audio keyboard/service/graphics draft/Apply/persistence and unchanged
transient renderer overrides. Native header/footer rectangles are measured at
1100×820 and 1280×720. This is native headless component/input/service evidence,
**not rendered visual readability or full-game acceptance**. The earlier fixture
attempt (`logs/20261011-080053-bbef46f4/`) failed because it added UI to a root busy
initializing; the corrected fixture owned its UI children directly. Its logs remain.

Live resource/modal/Settings checks now live in the existing minimal `EngineTests`
scene under `mise run test-engine --scenario settings`, including routine source
CI selection. The subsequent selected-subset cleanup removes eight unused
portable scene loads and two portable-only quote assertions. Actual quotes,
denials and action intent remain exercised through the real C# HUD and Worldtree
in `receiving`. These historical counts are not current selection counts.
The redundant probe/launcher/SceneTree-entry/task chain was removed. The landed supervisor supplies isolated preferences,
marker validation, engine logs, deadline and awaited cleanup. Added headless
regressions cover nested confirmation scope across Settings resize, current
per-open invokers after old controls are freed, invalid explicit invokers and
external Settings-control cleanup before reload.

The review-fix worktree based on `41738cdf` passed the focused
`test-engine --scenario settings --startup-timeout-ms 60000` selection:
**56 assertions** (the original settings checks, preserved 38 UI checks and added
ownership/lifetime/resize regressions), component 1.09s, runner including locked
restore/build/headless import 14.57s. Receipt:
`logs/20261011-102513-59899b65/test-engine-summary.json` and
`engine-settings/engine.log` beneath that run directory. Changed C# was formatted
with `dotnet format Odot.slnx --no-restore --include ...`. An initial preparation
attempt stopped at worktree-local mise trust; process-local path authorization
avoided user configuration changes. The first build then caught nullable invoker
marshalling, corrected before the passing run. Animation/assets, full C#/lint,
transport/package/CI and graphical checks were not run in this review-fix round;
those required outer gates remain separate from this partial component receipt.

The subsequent directional-scope review fix, based on `5302dce`, passed the same
focused headless selection with **75 assertions**, component 1.56s and runner
including locked preparation 15.29s:
`logs/20261011-104104-f0a858c3/test-engine-summary.json` and its
`engine-settings/engine.log`. It retains the earlier 56 checks and adds native
internal traversal, embedded dropdown selection, Settings TabBar arrows, the
ordinary confirmation Down/Enter path, and all four outgoing directional paths
with attempted suspended Apply/Cancel activation. Changed C# was formatted;
production kit bytes were untouched. This is review-fix worktree evidence, not
final-head full-suite/package/CI or graphical acceptance; those checks were not
run in this round.

The replacement-focus review fix, based on `2e80c0b`, passed that focused
headless selection with **89 assertions**, component 2.14s and runner including
locked preparation 17.54s:
`logs/20261011-105747-25ca250e/test-engine-summary.json` and its
`engine-settings/engine.log`. All earlier 75 checks remain. The added checks close
actual Friends/Settings components and open a replacement before deferred
restoration drains, cover live and fallback targets, require Enter to activate
Stay without underlying activation, retain ordinary restoration, and test the
receiving boundary with external/missing keyboard focus. Changed C# was formatted;
kit bytes were unchanged. This remains partial review-fix worktree evidence, not
final-head full-suite, package, CI, Steam-pair or graphical acceptance; none of
those checks ran in this round.

The invitation/subset review-fix worktree based on `e104f748` passed focused
headless `receiving` (**102 checks**, 7.91s component / 13.75s with preparation),
`settings` (**79 checks**, 1.96s / 6.90s), and the filtered
`WoodlandUiAssetTests` contract (**one test**, zero skips). Receipts are under
`logs/review-r1-r2/{receiving-final,settings-final,inventory}/`; raw command output
and verified installed SDK/engine hashes are retained in that same evidence root.
The lower Settings count removes only eight unused portable scene loads and two
portable quote assertions; live HUD quotes/actions and Worldtree research authority
remain exercised by receiving. Cross-subtree invitation tests cover native keys,
pointer blocking, dismissal/restoration and the resumed real research sender.
The shared pointer fixture now injects viewport-local coordinates, including at
stretched sizes, and keyboard assertions retain the existing frame barrier.
Earlier attempts in this round are preserved separately: new inventory code first
failed CA1861, then fixture development exposed mismatched pointer coordinates
and an immediate keyboard assertion without that barrier. Each was corrected
before the selected passes; no deadline or expected state was relaxed. Changed
C# was formatted after locked restore. Full suites/lint, animation/assets/standalone
Worldtree, transport/export/package/hosted CI, Steam pairing and graphical checks
were not executed in this review-fix round; the outer gates remain authoritative.

The restoration-freshness review fix based on `3b01bae` passed the single focused
headless `test-engine --scenario receiving --startup-timeout-ms 60000` selection:
**114 checks**, 8.00s component / 22.95s including locked preparation. Evidence is
`logs/review-r3/receiving/test-engine-summary.json` and its
`engine-receiving/engine.log`; `logs/review-r3/focused.log` retains starting
HEAD/tree and freshly verified installed SDK/engine identities. The new checks
cover a peer resume and opposite pause during coverage, disabled-focus fallback,
no denied Enter spend, and an accepted specialization acknowledgment delivered
while hidden. All earlier receiving/input/authority checks remain. Changed C# was
formatted after locked restore. No full C#/lint suites, other engine selections,
transport/package/hosted CI, real Steam or graphical checks ran in this round;
this selected pass is partial, not final publication clearance.

Final village/building/army/research/combat/multiplayer live-game journeys and
refreshed screenshots, native GPU/compositor/performance/physical input/listening,
exported presentation, paired Steam delivery, HiDPI/localization/RTL/tiny-window/
controller support remain **unexecuted on the final source** under the new policy.
The earlier Settings pass does not certify subsequent layout edits. Original
graphical journeys remain explicit, authorized selections rather than ordinary CI;
the landed [testing policy](verification.md) owns headless engine/transport/package
selection and process admission/deadlines. Client export presets include the
current UI manifest/notices/font license and retain the guarded minimal
`Verification/EngineTests.tscn` required by the landed exported Worldtree probe;
the retired `Verification/ui_resources.gd` launcher remains excluded;
packaged runtime/visual execution remains unexecuted by this review, not inferred
from filters. `.github/workflows/ci.yml` invokes the landed headless `ci-source`,
`ci-linux-package` and `ci-windows` gates; no graphical qualification is selected
by those ordinary gates.
No fullscreen or unrelated desktop capture is used for this migration.
