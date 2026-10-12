# Desktop renderer, woodland atmosphere and graphics preferences

> Current testing policy is [headless development/PR verification](verification.md).
> The renderer campaign receipts and earlier full-graphical CI requirements are
> historical; the command reference below describes current entry points.
> Graphical commands now require `ODOT_ALLOW_GRAPHICAL_TESTS=1` and explicit selection; PR/main does not execute them. No native/rendered receipts
> are recertified by the testing overhaul.

## Qualification status

The steering022 campaign below predates the no-mistakes review and test fixes;
it is historical evidence, not qualification of the current head. Later changes
correct graphics confirmation ordering, original Windows launch-argument handling,
client-only export prerequisites and explicit resize persistence. The latest
runner-only repair reuses fresh Settings observations and asserts the Audio
slider's actual post-click baseline without changing deadlines or product behavior.

For the implementation in `96612ee7aa5ab2ab47d2001346cd3603af1fb666`, retained
Linux results are **71 affected runner tests passed**, with **2 native Windows
argument tests skipped**, and both Settings selections passed: `settings/graphics`
**153.51s** scenario / **161.40s** command, and complete `settings` **168.29s** /
**176.24s**. Evidence is under `logs/settings-fixed/`,
`logs/settings-full-fixed/` and `logs/settings-fix-unit.txt`; the full selection
includes the original camera/HUD/modal/focus prelude as well as graphics, display,
audio, invalid saved values and same-city restart assertions. Earlier actual
180s Settings failures remain in `logs/settings-reproduce/` and
`logs/settings-trace/`, distinct from the earlier 30-minute agent invocation cutoff.
These selected results are not a complete source UI or CI pass, native Windows
execution, or recertification of native/export receipts. At that change, full CI and changed-input native/export qualification belonged
to the outer validation phases; that documentation pass ran no behavioral checks.
The historical source selection included all five UI cases. Today's default
headless policy is linked above; selected renderer proof remains separate.

## Historical qualification — steering022

Captain explicitly parked debugger acquisition, archive-integrity and root-cause
investigation in favor of **one final game-verification campaign** on the corrected
implementation, using existing game/test tools. That campaign passed. Earlier
failures, cancellations and contradictory diagnostic-artifact identities remain
retained; these results do **not** explain either managed-host crash or establish
that the runtime, machine or diagnostic archives were repaired. No suspect debugger
archive was parsed, extracted or executed during this campaign.

| Historical campaign inputs | Result and retained evidence |
| --- | --- |
| Fresh client preparation/export | Passed, `logs/20261010-204151-080f751b` |
| Native exported eight-view/control/bootstrap/override acceptance | Passed **31.76s**, `logs/20261010-204253-b051d9f4` |
| Native exported popup reopening/Escape/reselection/Apply/return/Exit | Passed **19.10s**, `logs/20261010-204359-9859b544` |
| Native source eight-view/control/bootstrap/override acceptance | Passed **30.08s**, `logs/20261010-204900-08f735bc` |
| Compatibility scenic eight-view/Apply/return/Exit | Passed **33.44s** owned / **41.98s** command, `logs/20261010-205007-8ad19c6b` |
| Complete required CI | Passed **804.29s**, `logs/20261010-205104-d590a723` |

The native export records actual **Forward+/Vulkan, AMD Radeon RX 6800 / RADV
NAVI21, DiscreteGpu, X11, Windowed**, with owned PID/native handle and isolated
settings. All eight exported game-only PNGs were inspected: authored palettes,
leaf details, rigid trunks/architecture, continuous painted river/banks/bridge,
subtle mist/evening, visible rain, readable HUD and paid-combat bodies/health bars.
All eight campaign Compatibility PNGs were also inspected: brighter software
lighting than native High, with readable combat/UI silhouettes, continuous banks
and bridge and restrained weather. Native RGB8→RGBA8 texture conversion warnings
remain visible, separately from engine errors. Source/native and software evidence
are not interchangeable performance or compositor/input/listening certifications.

Full CI retained the original **900s aggregate, jobs2/ui-jobs2**, all assertions and
startup/input bounds. It passed **618 runner + 504 gameplay tests**, all six network
and five source UI scenarios, sequential client/server exports, headless package
smoke and graphical package smoke. Corrected settings passed **154.87s**; full
camera/input/modal/cooperative behavior remains covered by the original scenarios.
Source and executable/PCK/Game.dll bindings before native acceptance and after the
final CI exports match; preparation did not substitute an older popup/AA artifact.
Campaign command/exit/binding records are private ignored evidence under
`logs/atmosphere-final022/`. Owned peers, windows, displays and temporary runtime
state were drained. No runner crash recurred in this campaign; intermittent faults
remain an explicit limitation, not an exoneration or accepted failing check.

The chronological evidence below records the status **at each historical boundary**.
Its pending/blocker statements do not supersede the qualification status above.
Forensic work is parked; publication remains the separate same-worker no-mistakes
handoff, with no manual push/PR/merge/release or waived checks.

## Later forest resource import

The [selected forest import](forest-import.md) follows this landed renderer work.
It preserves the atmosphere controller, renderer/weather/quality/preferences
behavior while updating only selected authored resources and source-faithful
import handling. Its current test policy and qualification boundary are separate
from the historical native campaigns below; Dreaming Forest styling is not part
of that import.

## Forest art direction

See [forest art direction](forest-art.md) for the themes, Art Style preferences,
optional magic rig and its qualification boundary. It extends this same
atmosphere owner; the [headless verification plan](forest-art-verification-plan.md)
selects the change-specific checks.

## Renderer selection

First-run desktop play/dev/client use Godot's native **Forward+ / Vulkan**. No
custom renderer or simulation weather is involved. Windows/Linux need a working
Vulkan driver and a supported GPU; Compatibility remains the OpenGL 3.3 fallback.
The engine can fall back on unsupported hardware: diagnostics report the **actual**
method, driver, adapter/vendor, API and device type, not the requested label.
Settings shows the running method, driver and adapter separately from the next-launch
preference. A fallback is not a Vulkan qualification.

```sh
mise run play                              # project default, or saved desired renderer
mise run dev --renderer compatibility      # host + guest, transient OpenGL override
mise run play --renderer forward-plus      # transient Vulkan override
mise run play --exported --renderer compatibility # existing Linux client; no rebuild
mise run play --exported --target windows-x64 --renderer compatibility # native Windows client
# Direct source/export engine arguments also work:
dist/client/odot.x86_64 --rendering-method gl_compatibility --rendering-driver opengl3
# Windows: dist/windows-client/odot.exe --rendering-method gl_compatibility --rendering-driver opengl3
```

`--renderer forward-plus|compatibility` belongs to graphical desktop launches.
Servers, network scenarios and engine-free rules tests have no graphical renderer
selection. Existing private-X11 source/package/UI checks explicitly select
Compatibility/OpenGL, regardless of project defaults or user preference.

## Settings behavior

The native Graphics tab has scrollable Display/performance, Atmosphere/weather,
[Art Style](forest-art.md#settings-and-palette) and Accessibility/comfort groups.
Existing display mode/resolution, Audio, About, kit styling, focus restoration
and selectors remain. Display changes preview immediately; **Apply graphics** keeps them and **Cancel graphics changes** restores
the last applied state. **Apply & close** keeps changes and closes only Settings;
Escape or the Settings header close discards unapplied graphics changes.
The [shared foreground dialog contract](ui-migration.md#discovered-live-consumers)
owns modal input and focus; closing the application window still exits the client.
A deliberate window resize outside Settings is saved, even when the initial launch supplied a transient display
override; untouched launch overrides and cancelled in-dialog previews are not saved.
Audio retains immediate application/save. Native keyboard focus follows the scroll
container.

Preferences use the existing `user://settings.cfg`: old `graphics`/`audio` keys
remain, new validated integer choices live in `visuals`. Missing, wrong-type and
out-of-range values independently use safe defaults. Exposure is limited to
80–120%. Changes are local, never part of snapshots, saves or multiplayer state.
The desired renderer is distinct from the running one. Applying it writes the
engine's native `user://renderer.cfg` project-settings override for **next launch**.
There is no automatic restart that could drop a live game. Finish/leave the session
and relaunch deliberately. CLI renderer/FPS/display overrides stay transient;
merely running a check or changing audio does not overwrite renderer preference.
Explicitly selecting the same renderer and applying it also writes that startup
override, repairing a stale override even when the saved settings value already
matches. Cancel does not perform that repair. The Renderer label has no release
promise; status separates active method/driver, saved request and draft/restart.
See [framing and renderer findings](view-renderer-worldtree.md) for the current
headless reproduction, corrected selection path and remaining GPU uncertainty.

VSync offers portable On/Off. Adaptive is deliberately not offered because the
engine has no portable present-mode support query; a driver can reject VSync
changes (for example Xvfb/Mesa), and diagnostics expose the actual mode. FPS choices
are 30/60/120/unlimited; explicit native `--max-fps` and `--disable-vsync` overrides
remain effective through Apply without becoming saved preferences. Original engine
arguments are read before the application `--` separator on Linux and Windows;
Windows runtime coverage is classified separately in the qualification status.
Render scale affects 3D only (70/85/100%), leaving the UI at native resolution.
AA is mutually exclusive Off/FXAA/TAA/MSAA 2×/4×. FXAA/TAA, SSIL and volumetric fog are
Forward+-only in this product. Unsupported active methods use a supported AA/fog
fallback and retain saved desired preferences. A pending Forward+ request can
configure next-launch features, but does not enable them in the active renderer;
the controls explain Apply/restart. With active and requested Compatibility,
unsupported choices remain disabled. Godot **4.7.2** also supports
Compatibility SSAO and glow; these use their native implementation, not overlays.
Compatibility's Low desired FXAA is unsupported and honestly falls back to **Off**;
Medium/default desired MSAA 2× also uses **Off** by the matched cost policy. Desired
values are retained, Forward+2×/FXAA remains unchanged, explicit supported High4× /
shadows/SSAO remains unchanged, and Ultra's TAA fallback remains supported MSAA2×.
Actual/applied versus desired-draft/effective labels explain these distinctions;
choice availability follows the active/pending Forward+ policy above. The previous
FXAA claim is withdrawn: property setter/getter state was not active capability proof. Authoritative renderer code at
locked engine hash `ed1daf0bf` returns before applying non-Off screen-space AA on
Compatibility. Matched MSAA-off cost proof and new actual Off witnesses are separate
from historical unsupported FXAA assertions.

Quality tiers change related rendering values, not renderer, weather or comfort:

| Tier | Cost policy |
| --- | --- |
| Low | 85% scale, FXAA (honest Off on Compatibility), 1024 shadows, depth/height fog, no AO/SSIL/volumetrics/particles/glow/wind |
| Medium (default) | 100% scale, desired MSAA 2× (Off on Compatibility by default cost policy), 2048 shadows, subtle fog/limited volumetrics, gentle foliage, low rain, subtle glow |
| High | MSAA 4×, 4096 shadows, subtle SSAO, volumetric/river mist, medium bounded rain |
| Ultra | TAA, high SSAO, optional SSIL, higher rain budget; still conservative fog/glow |

Individual performance/effect edits select Custom without resetting other current
values. Weather/renderer/accessibility remain independent. Reduce environmental
motion removes wind, drifting leaves and water motion. Reduce weather effects
quarters rain/airborne intensity. Reduced visual effects removes glow and SSIL.
Atmosphere Off disables cosmetic fog/weather/motion/glow without changing gameplay
or the chosen shadow/AA settings. For its interaction with magic, quality and
comfort settings, see [forest effect policy](forest-art.md#one-bounded-environmental-effect-rig).
Reset produces a cancellable default draft.

## Ownership and bounded effects

`GameApplication` owns one `AtmosphereController` for its whole graphical lifetime,
including the passive menu, solo, hosted/guest play, combat and city switching.
It extends the `VillageLighting` rig instead of creating competing environments
inside Tabletop. `GraphicsPreferences` is engine-free presentation policy; the
controller maps effective preferences onto actual viewport/environment/light
properties. No Game.Core numerical rules, clocks, terrain coordinates, picking,
collision, rigs, sockets or authored GLBs change.

Five manually selected profiles (clear morning, soft overcast, misty forest,
gentle rain, evening) smoothly blend sun/fill, fog, wind and precipitation. A
procedural native sky provides restrained cool illumination/reflections, warm
sunlight uses blended four-split directional shadows, and Filmic tonemapping plus
slight desaturation avoids clipped highlights. SSAO grounds props; SSIL is opt-in
via Ultra/Custom. Glow targets authored emissive objects with a high threshold,
not a fullscreen bloom wash.

Depth fog begins 48 metres from the camera and reaches bounded haze at 135 metres;
height fog is light and below the settlement surface. Forward+ uses conservative
volumetric volume-size/depth settings of 64 and a 55-metre volumetric distance.
A small river FogVolume exists **only** in Forward+ and follows the currently viewed city. The playable
center is not filled with dense volumes. Native GPU rain (120/240/360 bounded
particles) and a maximum 18 drifting leaves follow the camera's ground focus;
they cast no shadows and do not simulate every city/forest.

`WoodlandEligibility` explicitly allows actual leaf materials only on four
existing plant assets. `WoodlandMotion` caches derived lit materials and retains
original-resource links, albedo/PBR/normal textures, channel selection and UV
transforms. Bark, trunks, roots, luminous seeds, buildings, units and equipment
never sway. The shared shader bounds selected foliage to five centimetres laterally and two
centimetres in depth in world space, with a small culling margin. River material motion only adjusts
roughness/normal on cool water-colored pixels: no moving shoreline, UV scroll or
terrain displacement. Source/imported resources remain immutable; geometry is
reused. Only three shared shader uniforms are uploaded per frame; diagnostics
retain the last uploaded wind value rather than performing a forbidden runtime
global-uniform/GPU readback. Automatic weather progression is intentionally outside this implementation.
The later [forest art pass](forest-art.md) layers themes and optional magic on
this owner without changing authored art or weather ownership.

## Verification and visual evidence

```sh
mise run test                              # fast C# coverage
mise run test-engine --scenario settings    # headless component signals/persistence
# Graphical commands below require explicit authorization; not headless-only checks:
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-ui --scenario settings
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-ui --scenario settings --checkpoint graphics
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-ui --scenario settings --checkpoint display
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-ui --scenario economy --checkpoint travel
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-atmosphere         # Compatibility scenic evidence
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-vulkan             # native Vulkan hardware/surface
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-vulkan --exported  # existing client; no re-export
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-vulkan --desktop   # authorized windowed desktop
ODOT_ALLOW_GRAPHICAL_TESTS=1 mise run test-vulkan --desktop --exported
```

For default/deep selections and deadlines see the [testing command reference](../.agents/skills/testing-commands/SKILL.md).
`ci` coverage is owned by [current verification](verification.md#ci-coverage),
not the historical renderer receipts below.

By default both atmosphere commands use DevRunner's owned Xvfb/Xauthority/Openbox
display, client XDG state, ordered event driver, input/capture protocol, evidence and
bounded cleanup. Explicitly authorized `test-vulkan --desktop` instead uses the
existing Linux X11 surface with an owned windowed game, isolated client state and
game-viewport-only captures; it creates no display server/window manager and never
serves as a mandatory CI fallback. Both `--exported` checks require only the existing
Linux client executable, not a server export; missing clients are **unexecuted** and
neither command implicitly rebuilds them. Ordinary exported-package smoke retains
its client/server prerequisite. The Vulkan command does not set software-OpenGL
environment flags. It requires an actual Forward+ Vulkan RenderingDevice, physical
integrated/discrete GPU and real X11 window with captured pixels. CPU/software Vulkan, Dummy,
OpenGL fallback and missing prerequisites are **unexecuted**, never hardware green.
For Mesa hardware X11 presentation the owned display must expose DRI3; ordinary
Xvfb does not provide it. The native slice preflights this requirement and reports
unexecuted instead of letting Godot try another display. Each graphical game child
also owns its XDG runtime/socket namespace: unsetting WAYLAND_DISPLAY alone is
insufficient because libwayland can otherwise connect to the inherited wayland-0.
Initialization/shader/rendering errors fail. No desktop/display-server fallback,
profile modification, driver installation or machine reconfiguration is performed.
It is separate from required CI until a suitable runner is qualified.

Each command captures eight fixed 1100×820 views: menu landscape, village overview,
zoomed forest, river/bridge, misty morning, rain, evening and active first-wave
combat. The cosmetic clock is fixed at 12 seconds, particles use fixed seeds and
native seek/freeze, and combat uses ordinary paid construction/recruitment and a
paused authority snapshot. This is one small first wave, not another exhaustive
battle. Vulkan additionally witnesses an explicit safe return/exit, saved native
Compatibility startup, then a transient Vulkan override of that saved preference,
for both source and exported inputs. PNGs and matching actual renderer/settings
JSON live in ignored `logs/<run-id>/`. Inspect these images for ground depth,
material continuity, clear silhouettes, legible health bars/UI, mist/rain/dusk
readability and rigid-vs-foliage separation; initialization alone is not aesthetic
proof. This does not claim native compositor/input/listening quality or FPS/VRAM
benchmarks from software rendering or cumulative allocations.

## Requirement / milestone / evidence map

Implementation milestones below do not imply current-head acceptance. The
[qualification status](#qualification-status) owns that boundary; steering022
receipts refer to the historical pre-review campaign.

- [x] Shared renderer/environment ownership, explicit source/export overrides and
  truthful actual diagnostics implemented; qualification boundaries are recorded
  [above](#qualification-status).
- [x] Native sky/light/shadows/fog/volumetrics/river volume, manual weather,
  GPU rain/leaves, explicit leaf wind and restrained water/glow implemented.
- [x] Full scrollable graphics/preferences, quality/custom, supported combinations,
  Apply/Cancel, restart-later and comfort policy implemented.
- [x] Cheap validation/presets/capabilities/eligibility/launch/native-qualification
  controls: 604 runner tests plus 504 gameplay tests passed before later UI-harness
  additions (`logs/20261010-100620-a9b3d962`).
- [x] Before baseline: full CI/exports/package checks passed in 648.61 seconds,
  `logs/20261010-093637-df088641`. An earlier unchanged run failed one authored
  `hex_ground_moss.glb` provenance read (`logs/20261010-093402-254f1d70`); current
  clean source hash/length matched the manifest, the focused recheck and complete
  unchanged rerun passed. No asset/hash rewrite or cache repair was performed.
- [x] Compatibility settings and complete scenic command, unchanged 180s bounds:
  settings `logs/20261010-165044-c8406321`, **139.94s** owned / **150.62s** command;
  scenic `logs/20261010-165420-e884306f`, **32.89s** owned / **42.55s** command.
  All original controls, real High/quality/comfort/save/restart witnesses and
  eight fixed views plus real renderer Apply/safe return/exit passed. Subsequent
  exported-engine warning withdraws the specific active Compatibility FXAA claim;
  a correct supported fallback and rerun were pending at that boundary, then passed
  in steering010/022. Earlier 180s failures remain historical partial evidence,
  not relabeled passes.
- [x] Authentic source native Vulkan smoke, all eight inspected High views,
  real Apply/lifecycle and preference-bootstrap/transient-override proof:
  `logs/20261010-160141-1c1936ff`, 37.79s owned / 47.71s command.
- [x] Historical exported native Vulkan acceptance and eight-view inspection:
  [steering022](#historical-qualification--steering022).
- [x] Historical pre-review complete CI and exports/package evidence: steering022.
  This does not certify later changed inputs; see [qualification status](#qualification-status).

First Compatibility implementation attempt (`logs/20261010-101006-1a2838f3`) correctly
failed on unsupported fog-resource allocation and a forbidden runtime uniform
query. These are fixed by not allocating FogVolume/FogMaterial in Compatibility
and retaining the uploaded wind value; no error assertion was weakened. The next Compatibility attempt (`logs/20261010-101342-54feee6b`) rendered all eight
capture names without shader errors but timed out in the final settings witness:
new selector coordinates omitted the embedded dialog position. The selector now
reuses `GameApplication.ObserveControl`; close forest/bridge framing was also
corrected. Those selectors and fixed framing were revalidated in
`logs/20261010-103216-d5c59a8d`; all eight correctly framed captures and the actual
renderer-preference Apply assertion passed, but return/exit again exhausted the
unchanged 180-second command bound. The village probe reports a 3.22-second frame
wait and 1.68-second process sample on llvmpipe with forced High/4096 shadows,
MSAA 4× and Compatibility SSAO. These are retained software-workload samples,
not a hardware-performance benchmark. The suspected repeated sky horizon writes were corrected with an exact-value
cache only: every changed weather color is still uploaded, with no quantization,
transition suppression or clock/effect changes. The matching-input High
counterfactual (`logs/20261010-104408-3fd349ae`) proved only one upload across the
four stable Clear views, then changed palettes incremented the count; it still
reached the unchanged UI bound at **180.17s**. Village process/frame-wait samples
were **1857.31/3530.55ms**, versus the prior **1679.69/3222.64ms**. This disconfirms
an established sky-write explanation; no causal performance improvement is claimed.

The representative Compatibility policy was then used: **Medium**, MSAA 2×,
2048 shadows and AO off; native Forward+ evidence remains explicitly High. In
`logs/20261010-105112-9cefe67f` all eight captures, stable/changed sky checks and real
Apply passed again, but return/exit still hit the **180.13s** UI bound (full command
**192.39s**, including preparation/cleanup). Village process/frame-wait samples
were **1488.37/2973.99ms**; paused combat samples were **2643.44/5283.75ms**. All eight
Medium PNGs were inspected: UI, units, resource labels, building geometry and
bridge remain readable. These are software pictures, not native aesthetic acceptance.
The accepted bounded corrections did **not** solve the command deadline; stop and
report `compatibility-atmosphere-deadline` unresolved rather than raising timers,
dropping exit/input assertions or relabeling Medium/software as High/native.

Affected graphics/quality/owned-runtime/prerequisite unit tests passed **26/26**
(`logs/atmosphere-bounded-cheap-recheck.txt`); formatting and whitespace checks pass.
A preceding analyzer rejection (`CA1875`, use `Regex.Count`) was fixed and retained
in `logs/atmosphere-bounded-cheap.txt`. Requested High retains supported MSAA/shadows/AO
in policy tests; additional real settings coverage for High, smooth Overcast and
invalid saved AA/exposure is implemented but **unexecuted**. Settings/full CI/export
checks remain pending. No owned validation process remains.

Steering 004 authorized **one** focused Return/Exit diagnostic. The checked command
is `mise run test-atmosphere --ui-checkpoint return-diagnostic`: it reproduces the
same ordered manual presets, Medium renderer policy and paid paused first wave,
records modal/focus/window-transform boundaries, and includes a glow-only temporary
control restored before Apply/return/exit. It is explicitly **not** eight-view/native
acceptance. Initial argument admission was rejected before any engine/display;
this was corrected. The sole actual runtime (`logs/20261010-112140-14dc7126`) then
failed **before the diagnostic setup**, at the unchanged **15s** authentic-menu
UI observation deadline. The menu event was emitted; the requested UI response was
not received within that bound. No boundary PNG, glow counterfactual or Return/Exit
measurement was obtained. Full command **44.47s**, display/peers cleaned up. Thus
neither modal obstruction nor a particular effect has been demonstrated; no
accepted-behavior correction is justified from this diagnostic.

The independent existing settings scenario (`logs/20261010-112517-2d1646f2`) also
failed at **180.10s** after native camera/modal/input assertions and **before** the
new graphics quality/transition/load assertions. This is a wider Compatibility
regression symptom, not evidence that the new settings assertions passed. The
prechange settings probes typically sampled **177–284ms** process and **282–318ms**
frame waits, with different camera/state inputs; they establish prior usable
coverage, not a controlled causal comparison.

The independent `mise run test` (`logs/20261010-112828-f5707691`) failed: the cooperative
.NET test host **crashed** after two tests, and the sibling solo partition was
cancelled. No exception/signal explanation is in its captured output; do not
attribute it to product, filesystem or resource contention without evidence.
This does not supersede the earlier 26/26 affected graphics unit pass or establish
full cheap coverage. All current changes compile with zero warnings/errors and
format/whitespace checks pass. The focused investigation is exhausted; report
honest uncertainty and these concrete limits rather than retrying blind, expanding
profiling infrastructure, changing timeouts or claiming a checked shipping commit.
Full CI/export/native proof and ordinary handoff remain pending.

Steering 005 causal triage: retained system core **PID 3504912 / TID 3505065**, at
**11:28:33 UTC**, confirms **SIGSEGV / SEGV_MAPERR** in the `.NET BGC` thread,
`libcoreclr.so + 0x5d8cb7`. The crashing command is the cooperative Game.Core
`testhost.dll`, not Godot. Original terse logs do not identify the two completed
parameterized test names; no managed exception is supplied by the native trace.
The controlled filter contains `OrdinaryCooperativeStrategyWins(players, seed)`
for players 2/3/4 and seeds 0/1/123. Do not invent which case crashed.

Exactly **128** baseline/current controlled source/config/dependency-lock inputs
were verified byte-identical, copied to owned isolated roots and restored locked
without changing branch refs or shared build output. One serialized matched pair
ran the cooperative filter with detailed console/TRX/blame diagnostics: baseline
**9/9, 14.62s**; current **9/9, 16.26s**. All executable/core/test/dependency DLLs,
PDBs, runtimeconfig and deps files match SHA-256; only the coverage source-root path
mapping differs. Evidence and bindings: `logs/atmosphere-causal-triage/`. This is
partial isolated coverage, not the original parallel suite or current-machine
full-CI proof, and it does **not** erase the earlier crash.

Read-only host sampling at 11:35 UTC: 6.8GiB available RAM, 6.4GiB swap in use,
memory pressure averages zero, **88–95% I/O some pressure**, 75–89% full I/O pressure,
and low CPU pressure. Accessible kernel history around the runs contains the GC
segfault but no correlated OOM kill or Btrfs/EIO report; direct `dmesg` is permission
denied. No historical memory/process sampling exists for the exact crash. The
native failure is established, but its trigger (runtime, unchanged unsafe ECS,
filesystem/input corruption or contention) remains unresolved; no source fix,
GC override, repair or causal graphical claim is warranted.

Steering 006 records explicit Captain permission for **windowed desktop game
renderer tests until stopped**. `test-vulkan --desktop [--exported]` is an explicit
optional Linux X11 path through the same owned eight-view/controls/bootstrap smoke,
with isolated XDG state/socket namespace, Dummy audio, Steam disabled, windowed
1100×820 and game-viewport-only PNGs. It creates no display server/window manager
and never changes or captures the desktop or unrelated windows. Actual renderer,
driver, device, child PID, native window and Windowed mode are bound to evidence.
There is no automatic fallback: ordinary source/export Compatibility CI retains
its private software displays. Surface **authority** is now released; native proof
remains pending actual execution.

Authorized desktop source attempts: `logs/20261010-114453-d19d7bff` exposed an argv
routing defect: the direct adapter's null scenario selected Compatibility. Actual
RX 6800/radeonsi/OpenGL initialization was retained; this was not Vulkan proof.
The routing was narrowly corrected by binding the atmosphere scenario and recognizing
explicit DesktopSurface admission at the argv boundary. The changed-input run
`logs/20261010-115110-4becfcf7` authenticates native engine initialization:
**Forward+, Vulkan 1.4.354, AMD RX 6800 / RADV NAVI21, DiscreteGpu, X11**. However,
the menu UI observation again missed the unchanged **15s** bound (full command
**25.79s**). No captured pixels/native-window witness, eight frames or preference
bootstrap assertions completed; **native qualification is not passed**. Expected
native RGB8→RGBA8 import warnings are retained, not confused with shader errors.
The owned window/process/runtime were removed. Repeated startup obstruction is
reported rather than another blind source/native/full-CI run. Affected parser/
graphics tests passed **27/27** before the argv correction; later relevant tests
remain pending. No coherent checked commit or shipping pipeline has started.

Steering 007's sole bounded boundary trace is retained at
`logs/20261010-154322-318ee10f`. **Disconfirming result:** the same native menu
observation returned in **4718.89ms**, inside the original **15s** bound; the
separately labeled late diagnostic wait was **not used**. No effect/preference/
clock or startup-timeout correction was applied. Boundaries show the request
received at 15:43:36.046 UTC and dispatched at 15:43:40.573 UTC: **4527ms** before
main-thread dispatch. Two settling frames/post-draw then took about **7ms**;
selector construction **2ms**, graphics-popup flags **1.6ms**, all real renderer/
GPU/device/property diagnostic calls **3.3ms**, remaining snapshot construction
about **100ms**, serialization **35ms**, payload **56,696 bytes**. A second ordinary
game-only pixel request dispatched in **20ms**, settled/drew in **36ms**, and its
graphics reads took **0.05ms**. This rules out a persistent diagnostic-read,
serialization/payload or ownership obstruction in this observed run, not in the
untraced failures. The initial queued interval could be first-frame/shader work or
load; no narrow defect or falsifiable causal correction is established from it.

The one captured menu PNG was inspected and is readable. Its JSON authenticates
**Forward+/Vulkan, RX 6800/RADV DiscreteGpu, X11, Windowed, PID 999225, native window
16777218**, with client-owned isolated user data. Actual exit was clean. This is
**one partial native view**, not eight-view/controls/bootstrap/export or full-CI
acceptance. Diagnostic full command **24.04s**, owned UI lifetime **8.87s**.
No unrelated desktop/window capture occurred. Earlier timeouts and GC crash are
not erased by this success. Current build/format/whitespace checks pass.

The bounded round yielded no reproduced missing boundary and hence no demonstrated
startup source fix. Missing data is phase timing from an actual **timed-out** run;
only terse earlier failure logs exist. Stop and report that limitation rather than
claim the instrumentation repaired startup, widen the limit or run another blind
campaign. All final settings/Compatibility/native-eight-view/source-export/CI
acceptance and checked commit/handoff remain pending.

Earlier private-display native qualification was **blocked / not passed**:
`logs/20261010-102001-4b46a1eb` records an AMD RX 6800 / RADV physical Vulkan device,
but the owned Xvfb display has no DRI3 and Mesa rejected hardware graphics/present
queues. Despite explicit X11 and an unset WAYLAND_DISPLAY, Godot fell back to the
inherited default Wayland socket; the test rejected that actual display and cleaned
up its owned game/window/display. No Vulkan screenshot or native pass is claimed.
Graphical child runtime/socket isolation now prevents this fallback, DRI3 is
preflighted before native launch, and prerequisite exceptions retain their
unexecuted classification through scheduling. A qualified owned GPU surface is
needed for that private-display path; later explicitly authorized desktop evidence
below does not repair Xvfb's missing DRI3. No driver install or shared display-server
change was performed.

Steering 008 authorizes normal remaining implementation/verification end-to-end,
with unchanged limits, no speculative startup correction and retained uncertainty.
Minimal existing request/dispatch/frame/snapshot/serialization boundaries now remain
on owned graphics so a genuine recurrence has phase evidence. Native original
source attempt `logs/20261010-155045-7dcc0b30` passed startup and actual native
ownership, but the first fixed-preset acknowledgement timed out: the direct desktop
adapter omitted the existing verification-owner marker (private `_ui-worker`
provided it). Adding `test-vulkan`/`test-atmosphere` to the scoped marker-command
policy corrects this demonstrated routing defect without altering ownership guards,
atmosphere effects or timeouts. Focused ownership/parser/graphics tests **27/27**
passed; interactive `play`/`dev` are explicitly excluded.

`logs/20261010-155356-13251a9e` then captured all eight genuine native High views,
but real renderer Apply failed: the unchanged draft showed keyboard Home/Down had
selected item zero, not desired item one. An actual popup-open barrier exposed the
same mismatch in `logs/20261010-155701-aea5aed4`. Godot's pointer-opened PopupMenu
begins with no focused item; Home does not select the first item. The existing
Resolution helper already uses Down to focus item zero. Graphics now uses that
same ordinary input behavior with fresh popup-open, focused-item, closed/draft and
Apply witnesses. No synthetic item-selection callbacks or weakened assertions.

**Source native acceptance PASSED:** `logs/20261010-160141-1c1936ff`, **37.79s** owned
UI / **47.71s** command. Forward+/Vulkan 1.4.354, RX 6800/RADV NAVI21 DiscreteGpu,
X11, owned Windowed PID/native handle; all eight High fixed-clock game-only views,
paid first-wave paused combat, unchanged/changed sky-palette guarantees, shader-error
checks, real renderer choice/Apply without live renderer change, safe return/exit,
actual saved Compatibility next-launch bootstrap, then authentic transient Vulkan
override without overwriting the saved desired renderer all passed. Native
RGB8→RGBA8 hardware conversion warnings remain separately retained. All eight
prior captures with identical scenic inputs were inspected: readable menu/HUD,
forest leaf detail and rigid architecture, painted river/bridge boundaries, visible
rain strokes, subtle cooler mist and warmer evening, visible combat bodies/health
bars. Inputs differ from startup trace: High fixed scenic presets and gameplay /
settings lifecycle instead of menu-only diagnostic. This does not explain or erase
earlier intermittent 15s failures, nor the independent native GC crash.

**Mandatory full CI FAILED:** `logs/20261010-160638-09191035`, **464.27s** command.
Locked restore, format, build, import and authored-static fidelity passed;
**608/608** tooling and **504/504** gameplay passed with ordinary parallel rule
partitions. This does not explain/erase the earlier background-GC SIGSEGV.
Redistribution and authority-resume-victory passed. Compatibility economy failed
**396.57s** at existing bounded-city-travel predicate; reconnect and remaining
network cases were cancelled/drained, and settings/exports/packages did not run.
No timer, assertion, effect or quality workaround was applied. Source native
success is not a substitute for this software CI failure.

Exact last economy UI: Pan **(6, -6.005926)** versus required **(6, -8)**,
MovementSeconds **2.127666**, direction **(1, 0)**, connected session, settings and
confirmation/dropdown flags false, actual Compatibility/OpenGL3/llvmpipe, Medium
MSAA2×, shadows on, SSAO off, sky upload count one. Last completed request
`31eff90fb0994587b67304d47cff8b4e` received **16:14:11.931 UTC**, dispatched
**13.353**, post-draw **16.097**, response **16.135** (80,214 bytes); actual graphics
reads took about **0.02ms**. Final `118540b985824780858489a9b067ac88` received
**16:14:16.136**, dispatched **17.453**, post-draw **20.007**, then raised the
explicit application-closing exception during owned deadline cleanup, before
snapshot. Thus this trace does not reproduce the earlier startup-read failure or
establish an atmosphere/input/performance cause. Failed state and complete boundary
logs are retained; no blind same-case rerun or speculative optimization.

All owned peers/windows/displays/runtime were cleaned. Steering 009 authorizes the bounded software camera-travel diagnosis below. Exported native and
Compatibility scenic checks, real settings assertions, remaining full-CI gates,
final image inspection, coherent checked commit and same-worker shipping handoff
remain pending.

Steering 009's one bounded matched-input travel diagnosis is retained at
`logs/20261010-163007-ce35c1f2`, **94.71s** owned / **112.86s** command. The intended
MSAA2× attempt **FAILED the original 15s predicate** (**15.064s**); a separately
recorded post-release late observation eventually reached (6,-8), not acceptance.
Actual D down/up, physical key state, eligibility and each processed frame were
recorded. While D was down every frame remained eligible/physicalD=true; approximately
0.133s process deltas accompanied ~1.3s wall-frame intervals. Viewport/window
1100×820, HUD world fraction 640/820, base size 35.20043, yaw .48869216, overview
start (0,0), travel clamp (6,8), render scale1, focus/modal state, transient30FPS /
disabledVSync and desired settings match the failed path. Movement follows the
unchanged `CameraPanExpectation` using actual processed delta, not a wrong target,
resolution or transform. Baseline camera-navigation had the same dimensions,
projection and almost identical pan but no MSAA; 189ms process versus 1481ms current
is a workload observation, not by itself causal evidence.

The **one-effect control** changed only actual viewport MSAA2× to Disabled in the
owned runtime; preferences, renderer, shadows, glow, landscape, weather, animation /
input clocks and 15s predicate remained intact. Original target passed **2.203s**;
process **200.8ms** / frame wait **330.4ms**, versus intended **1311–1534ms** /
**2579–3073ms**. Restoring MSAA2× restored a **2560ms** frame wait. This isolates the
new multisample path as a meaningful contributor under matching software inputs,
not every possible performance cause or a native benchmark. The diagnostic command
still exits failed for its original failure; the late/counterfactual result cannot
turn it green. Its setup selects the same economy seed/peers/overview but omits
preceding camera exercises and the independent concurrent CI scenario; full parallel
CI remains required.

Narrow correction: Compatibility uses supported FXAA for the desired MSAA2× choice,
with truthful option/status/tooltip text; saved desired values and transient renderer
semantics remain intact. Forward+2×, requested High4×/shadows/SSAO and Ultra's existing
TAA capability fallback stay unchanged; no atmosphere/motion/glow/shadow disable,
clock, target, input-duration, terrain/ECS/navigation, deadline or concurrency change.
Affected pure graphics tests **28/28** passed. Original complete thorough
`CameraControls` selected with `test-ui --scenario economy --checkpoint travel`
**PASSED 84.06s** owned / **95.14s** command in
`logs/20261010-163847-8a8df2b0`: cursor zoom, drag, modal/table input protection,
cardinal/diagonal frame-delta movement, unchanged travel target and held-edge stop,
Space reset, both cities, actual resize and no gameplay commands all retained.
This is original focused fail-before/pass-after proof, not full economy/parallel CI.
Original settings and scenic then passed as above. Settings first exposed two
new harness expectation defects, retained separately: `logs/20261010-164042-b5964fce`
selected desired30FPS yet expected60FPS; its real engine cap was correctly30.
The fixture now deliberately requests120FPS and asserts exact desired120 / actual
transient30, including restart; quality presets retain the player's FPS preference.
`logs/20261010-164531-ea321c27` passed those new controls but the invalid-type restart
assertion still expected the former actualMSAA2 policy. It now checks exact default
desiredAA3, effectiveFXAA1, actual FXAA/disabledMSAA/noTAA and exposure1, without
weakening High/native AA assertions. ConfigFile wrong-type and independent comfort /
audio persistence remain real runtime witnesses. Current affected pure tests **28/28**
pass; all eight final source native High PNGs were explicitly inspected, not just
initialization. All eight final Compatibility PNGs were also inspected: brighter software lighting
than native High, but readable UI/body/health-bar silhouettes, continuous painted
water/bank textures, rigid bridge/architecture, visible leaf details/rain and subtle
cool mist/warm dusk. These are not hardware aesthetics/performance certification.

Normal full CI `logs/20261010-165738-ccfa0017` **FAILED 416.34s** at launcher **80.13s**,
not camera travel. **609 tooling + 504 gameplay**, all six network cases, parallel
settings **151.74s** and reconnect **100.03s** passed; economy was cancelled and
combat/exports/packages did not execute. The exact original-task integration defect:
`Main.RequireOwnedDisplay` required null WAYLAND_DISPLAY, while this branch correctly
isolated every graphical child's runtime with an absent owned socket path. The
existing offline consent fixture therefore rejected the owned display before input.
The gate now admits only the exact **absent** `no-wayland-fallback` path inside that
private display owner's runtime, retaining all supervised/offline/X11/worker-token /
Xauthority/role checks. Relative/inherited/different-owner/existing endpoints are
rejected by new adversarial pure tests; affected graphics tests **29/29** passed.
No consent or other original cooperative assertion was removed or relaxed.

Selected full launcher `logs/20261010-171032-ff39d32c` proves original failed consent
routing now passes actual Decline/Accept/Cancel/outside-input, retained match /
selection/music/preferences and no real Steam operation. However the full selected
suite then **CANCELLED at its original 180s admission** before friend-fixture coverage;
**176.89s** owned / **196.68s** command, not a full launcher pass. Read-only baseline
full-CI launcher measured **183.8686s**, already beyond selected180s. Aggregate
admission differences cannot establish a new rendering cause. No timer was raised
or standalone retry performed. Required full CI now resumes with its original900s
aggregate and two-job admission after the demonstrated ownership correction; all
full launcher/remaining source/export/package/native-export coverage remains
pending, with earlier failures/cancellations retained. Checked commit and shipping
handoff await actual complete required gates.

**Required full CI PASSED:** `logs/20261010-171632-8bbabd5d`, **860.62s** runner /
**862.83s** command, original900s aggregate / two expensive and two graphical jobs.
All **610 tooling + 504 gameplay**, six network cases, five source UI cases,
sequential client/server exports, headless package smoke and graphical package smoke
passed. Workers: economy **498.83s**, reconnect **158.21s**, settings **150.52s**,
launcher **223.29s**, combat **228.24s**; source UI suite **755.62s**. Export client /
server **8.93s / 3.92s**, package headless **1.56s**, graphical package **32.58s**.
This is genuine full current-machine coverage, not an explanation of the earlier
native GC crash or baseline/current isolated comparison. No publish/upload occurred.
The unsupported FXAA warning subsequently found below limits those new AA assertions;
all original tests/results/failures are retained, not silently recertified.

**Native exported acceptance FAILED:** `logs/20261010-173144-7f2ec70b`, **28.12s**
owned / **29.49s** command. All eight actual High Forward+/Vulkan RX6800/RADV views,
Windowed PID/native handle, real renderer Apply without live change, safe return /
actual exit, saved Compatibility bootstrap and transient Vulkan override assertions
executed. However four engine **ERRORs** occurred while closing the graphics popup:
`Attempt to disconnect a nonexistent connection` for SettingsDialog/root
`focus_entered` and `tree_exited`, empty callable. Owned cleanup correctly fails the
scenario despite zero process exit codes. These are not suppressible benign warnings
or a native export pass. No retry, engine/runtime alteration or speculative popup
workaround is authorized; bounded first-divergence diagnosis is requested.

This run also disconfirms the Compatibility FXAA capability claim: the actual engine
warns `Screen-space AA is only available when using the Forward+ or Mobile renderer`
in `viewport_set_screen_space_aa`. Read-only inspection finds the same warning in
previous full software CI. The viewport property returning `Fxaa` only confirms
requested state, not active support; the associated current menu/status/new runtime
assertions must be corrected rather than trusted. MSAA-off matching frame/input /
travel evidence is unaffected, but a genuinely supported renderer-aware AA fallback
must preserve desired preferences and requested High/native guarantees. The implementation remained **uncommitted/not ready** at this boundary; subsequent
steering010 corrections and bounded investigation follow below.


### Steering010: truthful Off and one source/export popup counterfactual

**Capability correction applied, not wholesale effects suppression:** desired
Compatibility FXAA/default MSAA2× now maps to actual Off; unsupported FXAA/TAA choices
are disabled, and UI distinguishes applied policy from desired/effective draft.
Desired saved/invalid-input handling, FPS/comfort, Forward+2×/FXAA and explicit High4×
remain intact, as does Ultra's supported MSAA2× fallback. Locked API XML documents
MSAA's cost and Compatibility SSAO. Authoritative code at actual engine hash
`ed1daf0bf` (`RendererViewport::viewport_set_screen_space_aa`) confirms Compatibility
rejects non-Off before touching render buffers. Retained read-only source fetches
and package/DLL/source hashes are under `logs/atmosphere-popup-*`.
Build/format passed; **34/34** affected pure graphics tests passed in
`logs/atmosphere-steering010-cheap.txt`, including all five desired/effective AA modes.
Original settings **PASSED 140.96s** owned / **150.85s** command,
`logs/20261010-175417-443ff2f6`, unchanged180s admission. Actual Low/Medium/invalid
ConfigFile restart witnesses now require disabled MSAA, disabled screen-space AA,
no TAA, desired values retained; explicit requested High4×/SSAO/shadows, smooth
Overcast, original display/focus/preferences/Apply/Cancel/comfort checks all remain.
No unsupported-AA warning appears in this corrected settings run. These checks do
not recertify the earlier full CI or establish native export acceptance.

**One smallest source/export native popup counterfactual, before popup fix:** new
`test-vulkan --desktop [--exported] --ui-checkpoint popup-diagnostic` uses owned solo,
same High village/device/Windowed/native keyboard choice, Apply, explicit return /
confirmation and displayed Exit. It preserves15s startup/input and180s scenario
bounds; no eight-view or full feature qualification claim. Opt-in read-only
`ODOT_POPUP` engine-log boundaries include parent/child ids/liveness/embedding/focus,
connection membership and selection order. Native Variant callable identity is
preserved in membership queries; an empty release method name is not invalidity.
Source **PASSED 18.24s** owned / **28.12s** command,
`logs/20261010-175004-dc482c3b`. Sequential actual re-export of AA-corrected/trace
sources passed **5.92s** export / **12.44s** command,
`logs/20261010-175032-387170b8`. Actual exported counterfactual **FAILED 17.00s** owned /
**18.38s** command, `logs/20261010-175045-72166c35`, the same four engine errors.
Package/executable/DLL hashes, engine `.official.ed1daf0bf`, current-source hashes and
parsed source/export traces bind the observations; no blind same-case retry follows.

**Located first divergence, no demonstrated application-side fix yet:** both builds
show live, in-tree, unqueued root/SettingsDialog/PopupMenu; identical embedded popup /
parent ownership and one added native focus/tree-exit connection per parent. After
real Enter, native focus returns to SettingsDialog. Debug/source removes those
connections before `popup_hide`, whereas release/export errors at
`Object::_disconnect` slot-map comparator lookup and leaves both connections attached.
This occurs **before** item-selected/draft `RefreshGraphics`, Apply, return or any
owner destruction; those do not explain the first failure. No duplicate application
connection or disposed owner is demonstrated. The installed hash's `Popup` connects
and reconstructs member-pointer callables during embedded parent deinitialization;
`CallableCustomMethodPointerBase` intentionally emits empty text in release builds,
so empty text is not evidence of a null/invalid callable. Hash/comparator/compiled
release identity cause is not established by these observations alone. We do not
assert a speculative engine bug, suppress errors, switch rendering/window behavior,
rewrite modal ownership, force exit or modify the locked runtime.

Native exported lifecycle remains **blocked**; all owned processes/windows/runtime
are drained. A further bounded causal scope is requested because no narrow safe
application integration correction has been demonstrated. Corrected full CI,
all-eight exported native acceptance/inspection, coherent checked commit and
same-worker no-mistakes handoff remained pending at this boundary; historical full
CI860.62s, earlier failures/cancellations and unrelated GC uncertainty stay distinct.

### Steering011: stripped native controls, local configuration and host stop

ONE stripped source/Debug-versus-export/Release comparison used an owned bootstrap
fixture with the same root/AcceptDialog/container/OptionButton parent shape, MSAA4×,
Windowed Vulkan RX6800/RADV and native pointer/Down/Enter dismissal. No
GameApplication/ClientSettings/Steam/material/match/draft-refresh/persistence handlers
were instantiated. Source `logs/20261010-180646-ef34e4ee` completed popup selection /
dismissal and dialog close without C++ disconnect errors, but **CANCELLED180.17s**
at a separate fixture Exit routing defect (literal `/root/Main` did not exist).
This is partial Debug evidence, not a full pass. Exit-only delegation to the existing
Main owner was corrected before export; popup code/shape/input was unchanged and
no second Debug comparison was performed. Locks/head/source/package hashes and the
exit-only difference are retained under `logs/atmosphere-vanilla-*`.
Actual stripped Release `logs/20261010-181008-96dd1fec` **FAILED2.22s**, reproducing
all four parent disconnect errors without game integration. This establishes the
narrow actual locked-engine embedded popup path, not an all-version engine claim or
a claim about empty callable validity. No engine build/patch/download/upgrade or
further comparator audit was performed.

The installed API documents `Window.ForceNative` as supported native window
ownership regardless of embedding. A single local configuration control retained
vanilla OptionButton/native pointer/Down/Enter/Close/Exit behavior with ForceNative
and **PASSED2.11s**, `logs/20261010-181315-b2fe66bb`, no engine errors or input change.
The game now sets only `ForceNative=true` on existing graphics/display/resolution
PopupMenus. Embedded settings dialog, OptionButton/ItemSelected/draft/selectors /
consumers remain; no dropdown framework, shadow UI or settings redesign was added.

Focused actual corrected export initially **FAILED25.34s**,
`logs/20261010-181723-2e38182b`, at the existing native-focus helper's private-display
guard, not C++ popup cleanup. Native popup ownership exposes focus recovery that
previous embedded popups did not. The existing Xlib **Focus only** now admits the
explicit authorized `test-vulkan --desktop` live child main window; `_NET_WM_PID`
must exactly match the observed owned process before focus changes. Resize/close /
defocus remain private-display-only. No unrelated focus/window/process/desktop
capture, new display manager or permissions expansion is involved.
Corrected original exported popup proof **PASSED30.68s** owned / **32.77s** command,
`logs/20261010-182041-127b0f37`: real selection/draft, reopen/Escape/reselection,
Apply, safe return/confirmation and displayed Exit; no engine errors. Format/build
and **37/37** pure graphics/admission tests also passed. The exact before failures
remain failures; focused success is not all-eight exported qualification or final CI.

**NEW MANAGED-SUPERVISOR STOP (cause unestablished):** original corrected native exported eight-view command
`logs/20261010-182227-0569555b` abruptly **FAILED28.49s** when the DevRunner .NET
supervisor itself received SIGSEGV. Only actual High native menu capture and ordinary
solo start executed; remaining seven views, renderer Apply/bootstrap/override and
subsequent source/Compatibility/final CI commands did not run. Retained system core
identifies supervisor **PID1985449**, **TID1985563 `.NET TP Worker`**, **SEGV_MAPERR**,
`libcoreclr.so+0x5d70a7`, at **18:22:52UTC**; details are in
`logs/atmosphere-export-runner-crash.txt`. This differs from the earlier background-GC
thread/boundary; a shared runtime does not establish shared cause or atmosphere
attribution. No retry, host repair, driver/runtime change or unchecked commit follows.

Supervisor death left only its owned exported game PID1985473/window56623106 alive.
Its last game-only observation, executable, owned XDG and native `_NET_WM_PID` were
verified before sending standard WM_DELETE_WINDOW. The game closed normally (no
forced termination); its temporary runtime was removed only after exit. No owned
peers/displays remain. Further bounded host scope is requested. Implementation stays
**uncommitted/not ready**; corrected full CI, all-eight native export/inspection and
same-worker pipeline remain pending despite truthful Off/settings and focused popup
success. Historical full CI860.62s and all prior failures/uncertainty are retained.

### Steering012 forensic return, pause013 and resume014

Before the explicit pause, bounded read-only analysis of only the two owned cores
completed using existing gdb/eu tools, with debuginfod and automatic scripts disabled.
Full dump material stays private (directory0700/files0600), ignored, and is not an
upload/publication artifact. Resume014 reconciles those retained outputs; no probe
or acceptance command was repeated.

The current supervisor core confirms **fault address0 / RCX0**, instruction
`mov (%rcx),%r9d`, `libcoreclr+0x5d70a7`; directly unwindable frames are in CoreCLR,
with deeper undecoded JIT/managed addresses. The earlier owned testhost/BGC core
also contains a null read using that instruction, but at **+0x5d8cb7** with a different
stack/thread/workload and no mapped Xlib. These similarities do not prove shared
cause, external host failure, or exonerate native interop. Dotnet host/CoreCLR/DAC
hashes exactly match the previously retained005 bindings. Runner DLL/PDB timestamps
precede the current crash, with runtimeconfig/deps/module/lock/head bindings retained;
no changed on-disk CLR bytes were demonstrated. That is not proof against transient
memory corruption or an existing code/runtime defect.

Child/ChildEvents reader locking/threading/disposal code is unchanged from baseline.
The recent Focus admission change exercises existing Xlib signatures/property reads /
XFree/lifetimes; reviewed LP64 widths did not demonstrate an overwrite or use-after-free.
The last menu observation reported unfocused, and Xlib is mapped in the crashed runner;
focus remains a relevant input, not a proven cause or proven-safe path. Absence of
Xlib frames on the immediate native fault stack cannot exclude earlier corruption.
Earlier passing focus/source/native/full CI paths likewise cannot dismiss an
intermittent fault with these changed inputs.

Accessible kernel evidence in **18:21:45–18:24:15UTC** confirms the same null read /
opcode and contains no correlated OOM/Btrfs/EIO entry. A resource sample collected
later at **18:34:42UTC** shows ~4.6GiB available RAM, near-zero memory pressure and
high I/O pressure; it does not establish conditions or causation at the crash time.
No unrelated process/core/private-data collection, installation, repair, stress run
or runtime alteration was undertaken. Detailed retained private findings:
`logs/atmosphere-crash012/findings.md`.

**Return: genuinely unknown intermittent managed-host fault.** No demonstrated
narrow defect, corrupt resource, independent transient or useful safe original-input
comparison discriminant warrants resumption. Replaying already-passing Focus or the
eight-view campaign would not distinguish the unresolved origins; the conditional
single reproduction was not spent. Matching DAC is present, but an available offline
managed-core analyzer/SOS and exact10.0.12 native symbols were not found, so the
managed allocation/call chain and heap attribution remain undecoded.

Concrete next option for MAIN: provide a matching already-available/user-managed
offline analyzer/SOS and native symbols for bounded decoding of these retained owned
cores, without changing the locked SDK/runtime. No download/install is performed by
this task. Alternatively MAIN must identify a specific new causal discriminant;
there is no repaired-host claim or implied permission for a blind acceptance retry.
The pause is resolved, but this independent substantive diagnostic blocker persists.
No owned test/window remains active; work stays uncommitted and all final changed-input
source/export/eight-view/Compatibility/full CI/package/shipping gates remain pending.

### Steering015: approved private tooling, decoded GC mechanism and backend limit

Captain-approved task-private official NuGet `dotnet-dump` / `dotnet-symbol`
**10.0.750501** acquisition resolves the previous tooling hold. The packaged net8
`rollForward:major` tools run on existing10.0.12; no additional runtime, global
installation, project/lock/SDK/Godot/driver change or dump/private-symbol upload.
Package provenance, versions/help/hashes and exact Microsoft public-symbol requests
are retained under `logs/atmosphere-crash012/diagnostics/`. Public CoreCLR build ID
`79945f51fb2612f13b7667a10a8fd29122664791` and JIT build ID
`f4d2e4bde8aa0a74fe2a063300924ecdb3de6731` match DWARF; downloaded public CoreCLR /
DAC/DBI bytes match installed10.0.12 hashes. Offline SOS uses local modules with
server downloads disabled and task-private HOME/cache; no private assembly identifiers
are submitted to symbol servers.

Exact native symbols decode the supervisor fault as `HasComponentSize/my_get_size`
in **GC root scanning / generation2 marking**, triggered by a **large string
allocation80687 characters /161400bytes**. The faulting TP worker scans another
managed thread4/OS1985465. The older testhost fault is the same null-method-table
size-read mechanism in **background marking/BGC**, not the same immediate stack or
proven shared origin. Adjacent +8-byte words in both invalid candidate objects point
to plausible MethodTables with self-consistent EEClass links. This is an observed
shape, not proof of an off-by-eight reference bug, native overwrite, GC/JIT defect
or machine fault; full managed heap verification and method attribution are absent.

Both cores' available SOS export resolver fails despite exact public/installed DACs:
`CLRDataCreateInstance0x80131c4f`, wrapper0x80004002. Official runtime source names
this **CORDBG_E_MISSING_DEBUGGER_EXPORTS**; finding/reading `g_dacTable` is required.
SOS reports that export missing, while native GDB resolves and reads the initialized
runner table. Exact local module/cache configuration and the supported cDAC option
also fail. Thus this is a concrete **offline backend limitation**, not evidence of
missing/changed runtime or an uninitialized crashed process. No tool patch, modified
core, version roulette, GC disabling, stress/reproduction or acceptance retry follows.

Private causal return: `logs/atmosphere-crash012/diagnostics/return015.md`.
Concrete next option for MAIN: provide/authorize task-private **LLDB native SOS**
(the verified NuGet plugin is present; LLDB is not), using the same exact offline
symbols/DAC. Official SOS support specifies LLDB on Linux; acquiring that native
debugger broadens the approved .NET-tool acquisition rather than changing runtime or
installing system packages. Alternatively provide a supported export-resolver path
for these retained cores. Acquisition is no longer the old unkeyed blocker, but
managed call-chain/heap attribution still blocks a justified correction/resumption.
No owned test/window is active, no commit/readiness claim, and all final gates stay
pending.

### Steering016: native backend scope resolved, official archive failure

MAIN authorizes ONE private LLDB/SOS attempt as a needed .NET diagnostic prerequisite.
Consulted actual mise registry/provider help and official current SOS support; no
verified LLDB registry distribution was identified, so used the permitted official
LLVM route. Existing NuGet SOS plugin is verified Linux ELF64/x86-64; LLDB support
requires>=3.9, prefers>=10. Official `llvm/llvm-project` release metadata selects
**LLVM23.1.3 Linux-X64**. No global installer/package or system/runtime change.

Official **1188398549-byte .tar.zst** matches published SHA256
`14d2f701eb68fb799001bdea6231048f3990690fa2f406d563555ff8f744daba`
both before and after the failure. The normal decoder first refuses the archive's
1GiB window versus128MiB default; after consulting actual help/error, the recommended
`--long=30` on the **same verified archive** fails **Restored data doesn't match
checksum / Unexpected EOF**. Partial listing/failures retained; conditional extraction
never ran. No extracted executable or native SOS attempt, CRC bypass, backend/version
roulette, core/tool patch, cache wipe or acceptance retry. Pre-decoder resource
snapshot has ~6.3GiB available RAM and621GiB storage; not a cause claim.

**New concrete acquisition blocker:** unchanged official whole-file hash does not
explain the decoder's internal checksum failure. No upstream/decoder/transient
attribution is established, and this does not prove a filesystem/RAM/external-host
fault or either GC crash's origin. Private return and exact logs:
`logs/atmosphere-crash012/diagnostics/lldb/return016.md`.
Next option for MAIN: a **same-version official .tar.xz** format control with
independent whole-file hash and XZ integrity checks, preserving the .zst evidence,
or upstream archive/decoder clarification. It was not downloaded/executed here;
the single native LLDB/SOS attempt remains unspent. Backend scope is resolved but
acquisition and undecoded managed-core/final-acceptance blockers remain. No owned
process/window active; uncommitted work and all final gates remain preserved/pending.

### Steering017: same-version XZ control, initial pass then decoding failure

One authorized official **LLVM23.1.3 Linux-X64 .tar.xz** control preserves the ZST
history and uses the same backend/version. The2016524848-byte archive matches the
independently published SHA256
`ca9fe385a5d9e2d88132dc0e23a00e9f87f38fa75f67207b5e457d208e71d013`
both before and after failure. **`xz --test` initially passes exit0**; this
independent pass is retained as disconfirming evidence. The subsequent ordinary
`tar -tJf` of that same archive fails **Compressed data is corrupt / Unexpected EOF**.
Only a partial listing was created: conditional selection/extraction never ran,
no debugger executed, and native LLDB/SOS's single attempt remains unspent.

**Stopped as instructed on the XZ decoding failure**, without a third-format /
re-download/decode loop, checksum bypass, partial executable use, cache wipe or
system/runtime change. The earlier integrity success cannot recertify the later
failed decoding. Upstream/decoder/memory/I/O origin remains unestablished; no
filesystem/RAM/external-host claim or causal leap to either GC crash. Exact private
return and retained output: `logs/atmosphere-crash012/diagnostics/lldb/return017.md`.

Concrete prerequisite: a trusted extractable private compatible LLDB distribution.
Next option for MAIN is an independently verified pre-extracted official bundle,
or separately bounded read-only artifact/decoder/data-path diagnosis with a useful
discriminator. No new SDK/runtime, random version or blind game retry is proposed.
System/library/plugin compatibility cannot be certified without safely acquiring
the debugger. No owned tests/windows active, all work/evidence preserved and
uncommitted, and remaining current-input renderer/source/export/full CI/shipping
gates are still pending.

### Steering018: one explicit decoder-path comparison (no causal repair)

Current exact executable/library/loader hashes, versions, nonsecret options and
limits are retained under `logs/atmosphere-crash012/diagnostics/lldb/path018/`:
GNUtar1.35, XZ/liblzma5.8.3, zstd1.5.7; XZ/TAR/loader override variables unset.
Historical `xz --test` and `tar -tJf` differ in operation/consumer; their actual child
argv, byte counts, signals and memory were not recorded, so current names/hashes
cannot retrospectively establish equivalence. An initial measurement wrapper assumes
absent `/usr/bin/time` and exits127 **before either decoder executes**; that failure /
empty-output hash is retained, not acceptance. Only that private measurement wrapper
is corrected to the existing Bash time keyword, not the original corruption.

ONE actual controlled pair in **20:02:55–20:03:15UTC** uses the same `/usr/bin/xz`
decompress/stdout/verbose flags/environment, file operand versus explicit PV-fed
stdin, normal default threads/memory, streamed byte count/hash and180s owned timeout.
File **all3 pipeline exits0 /10.095s**; stream **all4 exits0 /10.094s**, no signal or
timeout. Both validate the full **12415715840-byte** output, SHA256
`0f9c3919540d11b525ac39918b32a7b2d59ea5d2994568eb968c4f558ff1d773`.
Stream input count is2016524848; compressed archive official hash remains unchanged
before/after. No decompressed tar is persisted. Direct-file exact syscall counts /
peakRSS are unavailable (strace/time executable absent); no invented measurements.
Accessible exact-window kernel evidence has no correlated OOM/Btrfs/I/O-error entry;
RAM/pressure snapshots do not establish causes or peak process memory.

**Both controlled decodes pass identically**: a real full-byte integrity witness,
not a demonstrated file/stdin/environment defect, explanation of the old tar-consumer
failure, repaired-host/GC claim or game acceptance. No further decoder/tar attempt,
extraction or executable use follows this bounded return; native SOS attempt unspent.
Private return: `logs/atmosphere-crash012/diagnostics/lldb/path018/return018.md`.
Next option for MAIN: one authorized materialization of the decoded tar, requiring
this exact full-output hash/length and decoder0 before separately parsing/extracting
uncompressed tar, or a verified usable official bundle. This proposed acquisition
route isolates consumption while retaining integrity barriers; it is not an asserted
repair or implicit permission for further attempts. Old failures remain unexplained,
all work/evidence preserved/uncommitted, and final game/export/CI gates remain pending.

### Steering019: decoded-TAR materialization fails independent identity barrier

ONE authorized same official XZ materialization uses the already bound decoder /
normal environment,180s owned timeout, no download/version/system change. Input
matches published SHA256 before decoding. Decoder **exits0 /10.707s**, full internal
XZ checks; the stored private raw TAR has the exact expected **12415715840-byte**
length. Precondition618GiB storage/~7.1GiB available RAM, not causal evidence.

**First divergence is the independent stored-file hash:** required018 witness
`0f9c3919540d11b525ac39918b32a7b2d59ea5d2994568eb968c4f558ff1d773`
versus actual019
`b4d4557bab3f9b14d199b08448d5a2834e5e95fdb6ef787ebc1d5258d6f9bbd4`.
Materialization therefore **FAILS** despite decoder0/lengthmatch. Stopped immediately
without raw listing/extraction/native compatibility inspection, LLDB/SOS, another
decode/re-hash loop, partial executable use or artifact removal. Raw candidate and
all old controls/cores retained private/ignored; the native backend attempt unspent.

Private return: `logs/atmosphere-crash012/diagnostics/lldb/material019/return019.md`.
Decoder/storage/read/hash/transient origin is not distinguished; no host/upstream /
RAM/filesystem repair/GC attribution or safe game-resumption claim. Specific
prerequisite is independently trustworthy byte identity. Next option for MAIN is a
separately bounded read-only identity/comparison investigation of this retained raw
candidate/input/witness, without another decode, or a trusted independently verified
compatible official debugger. No system mutation or stress test is proposed. No
owned process/window active, work still uncommitted, all final qualification and
shipping gates pending.

### Steering020: independent retained identities, source digest disagreement

ONE bounded read-only identity comparison preserves the no-decode/no-parse/no-execute
barrier. GNU coreutils9.12 SHA-256 uses libcrypto; default CPython3.14.7 hashlib also
uses OpenSSL, so the existing **builtin `_sha2.sha256`** is selected for actual
independent implementation rather than claiming independence from wrapper names.
Exact executable/provider/library bindings and file/fd device/inode/size/mtime/ctime
before/after retained. An initial metadata `__file__` assumption fails before any
Python read; traceback retained and corrected only metadata/provider selection.
Actual GNU and builtin Python reads finish EOF/exit0, no read errno/stderr, no loop.

Stored RAW GNU/Python both agree on **b4d4557b…d6f9bbd4**, matching019 but not018's
**0f9c3919…ff1d773** decoded-stream witness. RAW device56/inode2177868/
12415715840bytes and timestamps remain unchanged across path/fd reads. Therefore
these two RAW checks do not demonstrate unstable raw reads; cross-stage mismatch
remains. More importantly SOURCE XZ independent digests disagree:
GNU **ca9fe385…08e71d013** (published/prior value) versus Python builtin
**062e70ba1681c120310bd6b444e1724c6f57744809819be55d88d91ec92f75e2**.
SOURCE device56/inode2173908/2016524848bytes and timestamps remain unchanged, no
symlink; the Python loop reads every byte to EOF. No further read/decode follows.

Actual018 successful command/stream/count/pipeline provenance is distinguished from
its failed absent-time wrapper's empty hash. Logical same-XZ/decoder binding is
coherent, but PV/hash-stdin versus regular-file storage/later hashing and timestamps
differ: physical byte equality cannot be assumed. No path/accounting error explaining
these results is demonstrated; no substitute golden hash. Accessible targeted kernel
20:09:10–20:09:25UTC around prior materialization has no Btrfs/I/O/OOM entry; this
cannot exonerate silent/transient paths or explain either GC crash.

**Trustworthy identity remains unproved**. Evidence supports a genuine reading /
hashing/data-integrity concern, not a filesystem/RAM/upstream/runtime/external-host
or shared-GC attribution. Private exact return:
`logs/atmosphere-crash012/diagnostics/lldb/identity020/return020.md`.
Smallest external prerequisite for MAIN/user-managed triage: an independent
known-good public-artifact reading/hashing path to adjudicate source digests and
cross-stage identity, or separately bounded validated artifact diagnostics. No
upgrade, installation, stress, repair or private dump upload implied; a pre-extracted
executable alone does not reconcile local identity concerns. No parse/LLDB/SOS/game
acceptance, native backend unspent, no owned processes/windows, all evidence preserved
and implementation uncommitted/final qualification pending.

### Steering021: last same-reader/same-buffer discriminator

ONE read-only source FD feeds the **exact same immutable chunks** to two existing
implementations in one process: GNU's actual imported `SHA256_Init/Update/Final`
from current **libcrypto/OpenSSL3.6.5**, and CPython3.14.7 builtin `_sha2.sha256`.
Installed header ABI/API/imports/provider hashes are bound; no new algorithm/tool,
acceleration/environment hack or separate read masquerading as common data.
This clarifies020: Python's default embedded OpenSSL3.5.8 and GNU's dynamic provider
are the same algorithm family, not identical modules/revisions; independence here
uses builtin SHA2 versus the exact native GNU provider.

Canonical **empty/abc/NIST56/million-a** known-answer vectors pass both implementations.
The one source read finishes EOF/exit0/no errors: **2016524848bytes,1924chunks** at
most1MiB,6.325s, unchanged source path/fd device/inode/size/mtime/ctime. **Both common
buffer algorithms return062e70ba…92f75e2**, matching020 builtin but conflicting with
published/prior GNU **ca9fe385…08e71d013**. Full actual vectors/provider/byte bindings:
`logs/atmosphere-crash012/diagnostics/lldb/common021/common-buffer.json`; private
bounded return: `common021/return021.md`. Old hashes/failures are not replaced.

This control finds no basic vector failure or same-chunk algorithm disagreement;
it does not establish why prior input/read/CLI hashes differed, repair acquisition /
host/upstream, or link either GC crash. **Stopped as instructed even on agreement**:
no further reader/control, decoder, parsing, extraction, SOS or game acceptance.
Minimum independent trusted-path prerequisite for MAIN/user-managed triage is to
adjudicate public artifact identity against a trustworthy reference/read/hash path,
reconciling source ca9/062e and RAW/stream b4d/0f9 before reliable local tool use.
No specific RAM/filesystem/hardware/runtime cause, upgrade/repair/stress or private
dump upload implied. Native backend attempt unspent; all files/core evidence private
and preserved, no owned processes/windows, work uncommitted/final gates pending.
