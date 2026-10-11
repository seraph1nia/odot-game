# Live woodland UI migration

## Source and runtime boundary

The game consumes the accepted native kit from `odot-game-assets`, incorporating
its compact skill tree and later woodland HUD/menu presentation, not the
superseded flat Ledger palette. The [manifest](../src/Game/UI/manifest.json) owns
the exact source revision, production member hashes and source-relative paths,
API/inventory/migration/coverage/woodland bindings, theme generator, original seal
source, reference hashes and catalog package identity. See
[notices and acquisition boundary](../src/Game/UI/README.md),
[woodland provenance](../src/Game/UI/woodland-art.json) and the complete
[font license](../src/Game/UI/fonts/LICENSE-DejaVu.txt).

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
| Research | `ResearchPanel`: compact native center-out fork per actual class, native inspect and purchase Buttons, decorative links, two-axis focus-follow scroll; production quoted-action detail | The actual 3/6/9-point roots/specializations/masteries, permanent sibling locks, effects replacing prior effects, authoritative points/requirements and `research-tech` action |
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
key scope; closure restores it. Dialogs are outside layout containers under the
application's viewport-sized overlay. Tabletop teardown frees its owned overlays.
Settings footer composition remains extensible for actual later controls; no
Forest Theme/Magical Effects feature or speculative settings service is added.

### Research adaptation, not an invented allocation economy

The portable `skill_tree.tscn` has a one-point shared root and exactly three
independent chains, with local allocation. The actual game instead has three
class foundations costing three points each, each with two exclusive
specialization/mastery chains costing six/nine points. Feeding game data to that
allocator would change rules or pretend it matches. It is therefore retained as
an unchanged production payload member, **not instantiated as game authority**.
The game adapts its compact center-out native Buttons/links/inspection/scroll
presentation to the real catalog. Inspect controls remain focusable for locked
and owned nodes; separate purchase controls reflect real eligibility. Existing
`Tech*` selectors still purchase their exact opaque technology; full effects,
prerequisite and sibling-lock copy remain in quotes/tooltips. There is no new
skill currency, one-point root, respec, local spend, prediction or new research rule.

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

Those checks now live in the existing minimal `EngineTests` scene under
`mise run test-engine --scenario settings`, including routine source CI selection.
The redundant probe/launcher/SceneTree-entry/task chain was removed, not its
meaningful assertions. The landed supervisor supplies isolated preferences,
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

Final village/building/army/research/combat/multiplayer live-game journeys and
refreshed screenshots, native GPU/compositor/performance/physical input/listening,
exported presentation, paired Steam delivery, HiDPI/localization/RTL/tiny-window/
controller support remain **unexecuted on the final source** under the new policy.
The earlier Settings pass does not certify subsequent layout edits. Original
graphical journeys remain explicit, authorized selections rather than ordinary CI;
the landed [testing policy](verification.md) owns headless engine/transport/package
selection and process admission/deadlines. Client export presets include the
current UI manifest/notices/font license and exclude `Verification/*`;
packaged runtime/visual execution remains unexecuted by this review, not inferred
from filters. `.github/workflows/ci.yml` invokes the landed headless `ci-source`,
`ci-linux-package` and `ci-windows` gates; no graphical qualification is selected
by those ordinary gates.
No fullscreen or unrelated desktop capture is used for this migration.
