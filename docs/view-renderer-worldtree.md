# Compact battlefield, renderer selection and Worldtree

## Product changes

`BattlefieldFrame` derives presentation content from every authority combat cell
and formation anchor, all nine buildable plots, home/defender silhouettes and
actual building bounds. It retains upgraded-roof allowance and does not alter
Game.Core topology, army/enemy allocation, routes, selection or economy. The
former padded X/Z box is replaced by this narrower content box. Camera fitting
still reserves the right-side HUD/inspection lane and bottom HUD.

The battlefield's passive terrain now covers a fixed content-derived region,
not a growing camera-footprint plus double-travel envelope. A separate opaque,
unshaded grey perimeter uses continuous exterior slabs and overlapping cloud
lobes, independently of renderer, weather and atmospheric-effects preferences.
Outside decorative scatter is excluded; the four magical accents remain inside
real compact terrain and the existing accent rig refreshes their moved sites.
No artwork was imported or edited; the pinned assets and provenance remain intact.

All navigation paths share a clamp: full native projection at ground/roof heights
must stay within the perimeter envelope, the view-center ground point must stay
inside the cloud bank, and local travel stays bounded. Resize/HUD/content refits
rebuild that envelope. Zoom-out stops at the fitted overview, rather than exposing
more dead terrain. This is presentation framing, not deletion of combat space.

Worldtree's HUD opener shows `Worldtree (X)` from the focused city's authoritative
unspent balance, even when the dialog is closed. Costs are not deducted by intent.
The native imported component remains byte-identical: the adapter hides its old
footer/points section and moves the existing action into one hover/focus surface.
There is no extra bottom description or native duplicate tooltip. Meaningful
costs/effects and actual denials are concise bullets; prerequisite/availability
boilerplate and healing/level disclaimers are absent. Hover/focus previews do not
change the canonical selected node underneath a covering modal. Existing topology,
tweens, inactive sockets, scrolling, keyboard traversal, purchase intent and
foreground suspension/current-authority restoration remain.

## Established renderer mechanism, not a hardware diagnosis

Forward+ / Vulkan has already been the real desktop fresh default since the landed
renderer batch (`d7f1728`): `project.godot` requests `forward_plus`/`vulkan`, and the
preferences default agrees. Ordinary play/dev does not force Compatibility; owned
graphical verification deliberately does. Saved Compatibility and explicit CLI
fallbacks are preserved. The previous label actually said **next launch**, not
next release; it referred to process startup, not an unavailable product feature.
The label is now simply Renderer and status names active method/driver, saved
request, draft request and any Apply/restart requirement.

The established settings defect combined native selection and change-only
persistence: OptionButton did not allow re-selection, and a same-value selection
would not mark the startup override dirty even if signaled. Selecting Vulkan again
while its saved settings value was already zero therefore could not repair startup. A stale Compatibility override could therefore survive Apply.
The smallest disconfirming counterfactual changes only explicit selection intent:
the actual native popup `IndexPressed` handler (one connection; `IdPressed` has
none), with the same desired value + Apply, now writes the native startup override.
Turning only `AllowReselect` off reproduces the retained stale override; Cancel
leaves that fixture untouched. Untouched startup overrides are never silently reset.

The capability gate has always required the **active `forward_plus` method**, not
just a Vulkan API/label. Selecting Vulkan cannot change a running Compatibility
or Mobile method into Forward+. Previously unsupported-active controls also made
next-launch fog configuration inaccessible. A pending Forward+ request now permits
configuration with an explicit next-launch explanation, while effective resources
still use the active method's fallback. The supported active method can enable the
actual Environment fog resource; Compatibility cannot. Atmospheric effects Off
or fog intensity Off still disables volumetric fog deliberately.

The focused test drives actual Settings signals -> Apply -> owned `settings.cfg`
and `renderer.cfg`, then awaits the child and intentionally restarts the **same
minimal component client**, reusing only that client's state. Native startup reads
`forward_plus`/`vulkan`, then `gl_compatibility`/`opengl3`; production atmosphere
resource application enables/disables fog accordingly. On headless Dummy these
are reported configuration/method values with an empty GPU name and no Rendering
Device, **not proof that a physical Vulkan GPU starts or renders volumetric fog**.
The user's particular saved files, device, active method and launch flags were not
inspected; the demonstrated stale-override mechanism is not claimed as their
established personal hardware cause.

## Local implementation receipts (2026-10-12)

Implementation began from `1d97f8a814009ab7e5665579cea518e302eacb62` in the isolated
feature worktree. These are bounded implementation receipts, not final PR-head
qualification; integration with later main work and final CI belong to the outer
pipeline. No shared restore/build/import outputs were mutated concurrently.

Locked installed tools, verified read-only against the prior same-tool receipts:

- SDK 10.0.401: `/home/bart/.local/share/mise/dotnet-root/dotnet`, SHA-256
  `01d89e0a0191052bfea616cd4ce624c8faf13b05bbddf7f64499c23e2a9d9269`.
- Godot .NET 4.7.2.stable.mono.official.ed1daf0bf:
  `/home/bart/.local/share/mise/installs/godot-dotnet/4.7.2-stable/godot`, SHA-256
  `fc1bd592b5e771c65d52968f99835b4f2b7fe7b7cbc141e202cb63e01c95ec4d`.
- Existing matching `4.7.2.stable.mono` templates were inspected, not acquired or
  used for a local export. No tool/package install, global setting change, alternate
  root, graphical process, display, application/world/session or campaign launch.

Successful process-local invocation used those directories on PATH, all three
`MISE_AUTO_INSTALL`, `MISE_EXEC_AUTO_INSTALL`, `MISE_TASK_RUN_AUTO_INSTALL` flags
set to `0`, `DOTNET_ROOT` and `DOTNET_ROOT_X64` at that same installed root, and
`DOTNET_ROLL_FORWARD=LatestMajor`. SDK/dependency locks remain unchanged. Child
invocation records in engine logs retain executable, original engine argv, roots,
effective roll-forward policy and actual `.NET 10.0.12` runtime. The existing
`Child` owner inherits that environment; `ScenarioScope` overlays per-client
XDG/owned state and does not replace/clear these runtime variables. Owned children
are awaited and runtime data removed by the existing supervisor. Exact built-member
hashes and post-await runtime absence checks are retained in
`logs/view-qualification-inputs.json`; these are implementation inputs, not a
claim of final commit/PR-head qualification.

Measured camera control counterfactual uses the **same production native fitter**,
aspect and 32% HUD allowance; only former padded bounds versus current content
bounds differ. It is not a graphical composition benchmark:

| Native window | Former orthographic size | Compact size |
| --- | ---: | ---: |
| 1100×820 | 40.402 | 34.753 |
| 1280×720 | 40.402 | 34.753 |
| 1920×1080 | 40.402 | 34.753 |
| 2560×1080 | 40.402 | 34.753 |
| 820×1100 | 60.287 | 43.447 |

At these inputs the passive component populated 228 floor cells instead of a
camera-dependent growing envelope. All authority anchors and buildable plots fit
the content bounds; native projection checks keep their bounding silhouette in
the usable viewport. Extreme cursor-centered zoom/drag, long keyboard-pan intents,
reset and resize cannot breach the full projection envelope or view-center cloud
bank. Perimeter resource construction verifies disabled transparency, alpha 1 and
four continuous exterior slabs. Native accent nodes refresh to bounded sites.

The standalone MultiMesh setter/getter counterfactual on Dummy returns `(0,0,0)`
for requested `(7,0,11)`, with no Rendering Device. Thus native emitted floor
**counts/resources** are demonstrated, but stored MultiMesh placement cannot be
read back honestly here. Content/coordinate retention is helper proof; native
buffer placement and rendered coverage remain unqualified.

Selected commands use the existing DevRunner binary/SceneTree; locked restore,
formatting and compilation passed. Local latest successful evidence:

- `test-engine --scenario settings`: `logs/20261012-200550-18dd1ae6`, **4.52s**
  component including two awaited minimal restarts / **10.19s** total. Actual
  persistence, cancellation, stale re-selection repair, active resource policy,
  projection/input/resources, compact passive landscape and native dialog checks.
- `test-engine --scenario worldtree`: `logs/20261012-192553-4241d01c`, **7.05s**
  component / **11.74s** total. Real imported resources, signals/tweens, single
  surface, keyboard action/host/modal traversal, constrained scroll and teardown.
- `test-engine --scenario receiving`: `logs/20261012-192331-faae68be`, **8.91s**
  component / **14.78s** total. Actual HUD/session sender with numerical authority
  and delayed acknowledgments, 30→27→21 point balances, rejected/stale requests,
  costs/locks/inactive states and retained foreground restoration. The receiving
  adapter is not a live transport/session.
- `test-core --filter 'FullyQualifiedName~WorldtreeResearchTests'`:
  `logs/20261012-194353-3d58bd54`, eight passed, including concise copy and actual
  prerequisite/phase denials. This filtered pass is partial.
- `test-fast`: `logs/20261012-194355-b9df5ec4`, **21.62s**, 466 core + 649 runner
  passed; two Windows-only cases skipped, `Tier=Deep` excluded.

### New failures preserved, diagnosed and corrected

These are new failures, not an inherited accepted crash or waived success:

1. A worker-added `DOTNET_ROLL_FORWARD=Disable` prevented the runner from choosing
   installed runtime 10.0.12 for its 10.0.0 request. The subsequent `LatestPatch`
   invocation reached Godot but overrode its shipped net8.0 bootstrap runtimeconfig's
   **LatestMajor** policy. `logs/20261012-191003-0c82390e/import.json` records hostfxr's
   8.0.0 refusal, exit **134** and native **signal 11**. Same executable/root/SHA,
   corrected process-local LatestMajor, and no installed-file edits passed import
   (`logs/20261012-191355-4b3dba64`). This is invocation cause, not a product/GPU
   incompatibility verdict. The refused preparation did not execute its engine case.
2. The initial projection fixture reserved the left side while the real ledger is
   on the right. Correcting that fixture to production layout produced the concrete
   retained projections; no camera invariant or deadline was weakened.
3. Initial hover code selected nodes on MouseEntered underneath a covering embedded
   window. Separating preview from native click selection retained the modal's
   consumed-input negative path. Removing the old bottom text also made a 620px
   fixture fit without scrolling; a genuinely constrained 520px host now exercises
   native wheel scrolling instead of assuming a scrollbar always exists.
4. The hover information container initially intercepted clicks on another socket.
   Its non-action background now ignores picking; disabled actions also ignore it.
   Real receiving clicks, scope restoration and purchases passed after the fix.
5. `logs/20261012-193226-6fa27bc3` rejected the new native-transform assertion on
   Dummy and recorded resource errors during failed teardown. The standalone
   setter/getter establishes the unavailable readback boundary rather than faking
   coordinates. Static derived woodland materials retained authored textures past
   RenderingServer teardown; the short-lived fixture now releases its owned cache
   after consumers are freed and runs managed finalizers while the server is alive.
   The corrected passive component and all owned restarts exit without engine errors.
6. `logs/20261012-194810-f617ad3b` rejected an empty synthetic keyboard path: the
   fixture supplied only physical keycode, so production polling saw no pressed
   state. Supplying both logical/physical codes and awaiting native input dispatch
   makes both states true and exercises the real `Move` path, without an input-state
   override. `logs/20261012-195359-59892d7e` then caught a default-framing comparison
   retaining the previous pan. The fixture now separately checks confined resize
   with retained pan/zoom and resets only for the default-view comparison. Production
   resize still retains local state; no camera invariant or deadline was weakened.
7. `logs/20261012-200239-3381cd05` rejected the repaired renderer assertion when
   the new fixture emitted popup `IdPressed`, which this OptionButton does not own.
   The native connection-list counterfactual establishes `IndexPressed=1` and
   `IdPressed=0`. The final test uses the actual handler, reproduces the old
   no-reselection behavior and proves the repair without directly synthesizing
   `ItemSelected` (which had masked native selection policy).

## Review R1: hover-only Research transit

The shared dismissal path now places the existing details surface before checking
its current viewport-space socket, panel and bounded horizontal transit strip.
Socket ownership respects ancestor clipping; clipping cannot enlarge the gap.
Native canvas transforms apply to hit geometry and viewport confinement. The
existing keyboard-focus owner and foreground suspension/restoration remain intact;
no timeout, duplicate panel or new popup supervisor was added.

Coverage admission: pure C# and the previous click/focus fixture missed the
observable defect: a frame between socket exit and panel entry hid the only
purchase surface. The existing Tier B Worldtree/receiving fixtures now use native
`Input.ParseInputEvent` motion through the actual laid-out gap, asserting the
viewport's recorded pointer position and retained visibility without socket
click/focus. They exercise both placement sides, genuine leave, another inactive
socket, translated canvas, native scrolling and resized placement, retaining
keyboard traversal and Settings/invitation/leave ownership checks. Receiving uses
the production sender, real Match denial/purchase and explicit delayed snapshot
acknowledgment; it does not substitute a local allocator or live transport.
Resources remain owned by the existing fixture and runner. Added setup is native
layout frames plus one purchase awakening, not another scene or game journey.

Focused review receipts (SDK 10.0.401, Godot .NET 4.7.2, process-local
`DOTNET_ROLL_FORWARD=LatestMajor`, locked restore; changed C# formatted):

- `logs/20261012-203604-c2a8a493`: compiled the saved production file from
  `d92ab4c265f94adb77031cca2e73c0ea0eeb27cb` against the new fixture. It failed the
  transit visibility assertion in 0.92s. Native socket end X=350.1732, panel start
  X=358.1732, pointer=(354.1732, 258.8866), panel hidden. This is an executed
  source-bound counterfactual, not a source-text assertion.
- `logs/20261012-203907-98994bbd`: final `test-engine --scenario worldtree
  --startup-timeout-ms 60000` passed, 7.71s component / 13.26s including preparation.
- `logs/20261012-203920-2e467cf4`: final `test-engine --scenario receiving
  --startup-timeout-ms 60000` passed, 13.47s component / 17.97s including preparation.
- Failed fixture attempts remain in `logs/20261012-203441-38ea4fc2` (GUI
  `Viewport.PushInput` did not update the native pointer queried by dismissal)
  and `logs/20261012-203612-f3b85fe8` (the chosen purported inactive socket was
  actually Managed Forestry). Correcting native input delivery and choosing the
  actual inactive sibling preserved the assertions, budgets and failure evidence.
  Intermediate corrected passes are not final-source receipts.

These are selected headless components, not a full test/lint gate. No complete
suite, graphical/full-application journey, transport, package, PR or CI phase was
executed in this review fix round. Native headless geometry/input dispatch does
not qualify physical mouse/compositor behavior or rendered appearance.

## Explicit unexecuted acceptance

No native artistic composition, opaque rendered pixels, physical Vulkan GPU
startup/performance, visible volumetric fog, full end-user UI/session, real Steam
invitations, graphical input/compositor, screenshots, transport journey, package
export or local Windows qualification was executed for this batch. Headless
reported Vulkan method/driver strings and resource flags cannot certify those
behaviors. Separate authorized graphical/native acceptance is still needed for
final cloud appearance, occlusion/picking readability and hardware fog behavior.
No new harness or desktop fallback is implied by that limitation.
