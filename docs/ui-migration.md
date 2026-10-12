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
or restored research scope remains, including fallback restoration. These receiving-boundary regressions
live in `src/Game/Verification/EngineTests.Ui.cs`.
The game still explicitly blocks picking/held-world gestures/commands while a
modal is open. Application-hosted dialogs share the adapter's visibility boundary:
the last visible sibling owns its key scope, while Settings, Friends, Join, Leave,
Details and Town hall all suspend the Tabletop research host. Closing an inner
dialog restores only the remaining foreground dialog; research stays hidden until
the last covering dialog closes. Its selected state and valid native focus target
are preserved without a separate invitation/leave exception.
The Tabletop research visibility boundary refreshes the current accepted session
projection on both ordinary opening and restoration, before focus returns; a
purchase control newly disabled by that snapshot uses the existing root fallback.
The receiving fixture constructs the actual persistent Settings button, nested
leave Controls and application invitation above the later HUD subtree, without
starting the application or using Steam credentials.
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

The shared-foreground-host review fix based on `325c99c` passed focused headless
`receiving` (**153 checks**, 8.84s component / 24.32s with preparation) and
`settings` (**79 checks**, 1.96s / 6.82s), each with
`--startup-timeout-ms 60000`. Receipts are under
`logs/review-r4/{receiving,settings}/`; `logs/review-r4/focused.log` records starting
HEAD/tree and freshly verified installed SDK/engine identities. The receiving
case adds the real persistent Settings-button route, foreground native keys,
nested production leave cancellation, both covering-dialog close orders,
acknowledged peer pause and deferred-focus protection. Earlier invitation,
authoritative-refresh and live HUD/research checks remain, as do the Settings,
Friends and replacement-confirmation cases. Isolated Join/Leave scope subscriptions
were replaced by the existing adapter's foreground-sibling decision and research
host binding. Changed C# was formatted after locked restore. No complete C#/lint
suite, other engine selections, transport/package/hosted CI, real Steam or
graphical verification ran in this review round; these selected passes remain
partial, not final-head delivery clearance.

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

## Construction and Resources spacing

Follow-up receiving-only fix, based on main `1d97f8a`. The supplied cropped
building-options and Resources images show collisions/edge crowding; their
2235×249 and 720×696 crop dimensions do **not** establish a full viewport or
reproduce its scale. No new screenshot or graphical/full-game run was authorized.

Native baseline `receiving` measured DejaVuSerif at the existing 11px override:
14px line height, 11px ascent and 3px descent. Construction buttons were 32px
high (native minimum 32), including a blank title line; the independently anchored
cost Control started at y=8 and had height 21. A settled real GridContainer with
the same inherited theme, font override and old receiving margins reproduces
Magic academy's shaped first title line at y=2..16 and cost line at y=8..22:
**8px vertical intersection**, not just intersecting Control bounds. The previous
non-woodland receiving code also used this overlay/blank-line technique; history
alone therefore does not prove the migration uniquely caused the screenshot.

An initial standalone counterfactual assertion failed. Assigning its size before
font/style overrides clamped it to 54px while the eventual minimum was 32px;
its title line was y=13..27 and cost y=30..44, unlike the measured production
allocation. It was rejected, not treated as native instability or waived. The
retained counterfactual uses the actual container's settled native minimum.

Construction now reserves a caption line using the real caption font height in
Button content margins; native title shaping/wrapping and minimum size remain
Godot-owned. For the current catalog, title lines occupy y=6..20 and cost lines
y=24..38, with **4px gap**, **6px vertical** and **8px horizontal interior padding**,
and a 44px minimum height. Caption width also contributes to the minimum, so
costs are complete rather than ellipsized. Font sizes, captions/actions and
normal/disabled semantics are retained; unrelated quote consumers are unchanged.

Resources keeps the original frame/immutable asset bytes but overrides only the
receiving panel's content inset from 16px to its 40px decorated nine-slice corners.
Stock/income headings align with their distinct right-aligned value columns;
resource/upkeep rows have 4px separation and stack sections 10px. Deferred native
minimum changes resize the panel after context wrapping instead of retaining an
oversized intermediate allocation. Starting stocks settle at 288×340, disconnected
context at 288×364, and six maximum-int stocks at 343×340, all with 40px safe inset.
The shared receiving reflow now also re-pins the right edge after resetting the
native minimum, retaining the 12px viewport inset for both economy refreshes and
deferred minimum changes. The original spacing receipt below checked panel-local
safe bounds and vertical fit, not horizontal viewport containment; its 343px
allocation could extend beyond the viewport before this reflow correction.

Focused receipt: `logs/20261012-182938-0082b1bf`, `test-engine --scenario receiving`,
10.04s component / 16.53s including preparation. Four windows (1100×820,
1280×720, 1920×1080, 2200×1640) exercise production canvas stretch approximately
1×, 0.878×, 1.317×, 2×; starting, disconnected and maximum-int stock states
exercise all Army/Production construction labels/costs, enabled/disabled states,
complete resource/footer numbers, shaped title/caption boxes, native minima,
content bounds, safe inset, column alignment and fit above the bottom HUD.
The receiving fixture additionally checks whole-frame viewport containment and
the retained right inset through starting/max-int stock growth and shrinkage,
combined disconnected/max-int state, and real numerical Preparation/Combat phase
transitions with ordinary, disconnected and paused context. These additions do
not turn the original receipt into evidence for the corrected reflow.
The existing authority/acknowledgment/rejection/research/modal/input checks pass.
Fast C# receipt: `logs/20261012-182955-3eb480c9`, 466 core + 649 runner passed,
2 Windows-only skips, 22.52s; excludes Deep. Locked restore and C# format completed
with installed .NET 10.0.401 / Godot .NET 4.7.2 and auto-install disabled.

Review-fix receipt on `49cc54e2` plus the reflow working diff:
`logs/review-ledger-reflow-final/test-engine-summary.json`, selected `receiving`,
11.15s component / 28.58s including locked preparation. All four windows retained
12px logical right inset through 288×340, 288×364, 343×340 and 343×364 Building
allocations and return to starting stocks. At the restored 1920×1080 window
(logical viewport 1457×820), actual Preparation/Combat income/upkeep text settled
at 326×364, grew to 381×364 for max-int stocks (also with paused context), then
shrank to 326×364, retaining the same inset and viewport containment. Existing
construction/authority/modal/input/disposal assertions passed. Phase placement
was checked only at that restored window size; phase label readability/pixels
were not qualified. The first attempt, `logs/review-ledger-reflow`, failed at
compilation because the fixture used an inaccessible `City.Resources` setter;
no engine case ran. The fixture was corrected to assign public stock fields and
the focused case rerun, without a waiver. Changed C# files were formatted with
`--no-restore --include`; no full test/lint suite or other engine case ran in this
review round. Installed tool versions and auto-install prohibitions were retained.

These are local receipts, not final published-head CI acceptance.
Headless shaped-line/control geometry does **not** prove artistic readability,
rendered pixels/GPU, physical input, a full UI/session, or the screenshot's unknown
viewport. Appearance, full-game journeys, graphical/private-display testing,
other engine selections and package/transport qualification were not locally
run for this follow-up. Current-head outer validation owns broader delivery gates.
