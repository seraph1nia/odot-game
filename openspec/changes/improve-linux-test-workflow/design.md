# Design

## Context

See `proposal.md` for motivation and the two delta specs for behavior contracts. The current C# supervisor already owns real Godot processes, stdin commands, bounded event waits and cleanup. `NetworkTests()` awaits authority/resume/victory, redistribution, defeat and failure scenarios sequentially; `_testSessions` is mutable runner-wide state. `Child.WaitFor()` consumes one channel, so competing consumers on a single child are unsafe even though independent peers can run concurrently.

`Ci()` restores the solution, checks formatting, then calls `Prepare()`, which repeats preflight/restore before build/import. Existing rules take roughly 115 ms of assertion execution in saved logs; the recorded historical full CI is about 128 seconds. These are context, not a fresh performance baseline. All existing automated peers and export smoke are headless. `Main` creates `Tabletop` only outside headless mode; real UI input therefore requires a graphical client. The supervised `click` command already injects Godot mouse events without moving a physical pointer. The settings/music implementation is active work in another change and must not be redesigned here.

The repository pins Godot .NET 4.7.2 and .NET SDK 10.0.401, uses GL Compatibility and has Linux x86_64 exports and Ubuntu 24.04 CI. Xvfb and its wrapper were not found on the inspected local PATH. The documentation supports the architecture, but this project's software-rendered smoke must still be executed.

## Goals / Non-Goals

**Goals:** Keep verification off the desktop; preserve real peers and UI event routing; make concurrency, ownership, preparation and evidence explicit within the existing supervisor.

**Non-Goals:** Migrate to another language or third-party Godot testing framework; accelerate simulation or change balance/protocol; build a container or cross-platform display abstraction; treat software rendering or Dummy audio as hardware/native-display/listening verification. Source editing during a build is not made transactional by a private display. A reusable scenario harness within the existing C# runner is in scope.

## Decisions

### 1. Keep the existing C# runner and add explicit verification entry points

Add `test-ui` to the runner and mise. Add `--jobs N` and `--scenario NAME` to network execution; default to two workers and retain `--jobs 1`. Use stable scenario ids `authority-resume-victory`, `redistribution`, `defeat`, and `failure-cases`. CI always runs the full set; a filtered invocation prints selected coverage. Validate options before preparation/child launch. Existing `--port` continues to pin the authority scenario when included; reject a filtered combination that cannot honor that option rather than silently ignoring it.

Add `--scenario NAME` to `test-ui` as well. Initial source slice ids are `economy`, `reconnect`, and `settings`; unfiltered `test-ui` runs all three serially. `test-ui --scenario exported-package` runs only the reduced package smoke against existing `dist/client` and `dist/server` outputs after tool/display/package preflight, without preparing source or implicitly exporting. Missing packages fail with the required preceding export task. CI runs the source set before exports and the package slice after them, without inheriting a developer's filter.

Extract preparation phases and verification methods so standalone tasks prepare safely and CI invokes already-prepared checks. No default skip-build flag that could hide stale C# is needed. Alternative: migrate to a Godot test plugin. The current process driver already covers the required protocol and would still need display supervision after such a migration.

### 2. Give each scenario its own ownership scope and serialize each peer's driver

Replace reliance on mutable `_testSessions` with explicit run/scenario/client contexts carrying temporary paths, endpoint, evidence prefix, cancellation and child environment. The scope retains a client's data across its intentional restart and disposes peers before deleting files. Give each engine process an explicit unique `--log-file` so Godot's own default log rotation cannot collide, as well as the existing supervisor logs.

A bounded scheduler starts at most `jobs` independent scenario bodies. On the first failure, stop scheduling, cancel active siblings, await their cleanup and preserve the original condition instead of reporting only secondary cancellation. Keep one command/assertion stream per child: do not parallelize calls into the same `Action()` or `WaitFor()` consumer. Start the long authority and redistribution scenarios first so their real-time battles overlap. Automatic port allocation remains loopback-only; coordinate candidate allocation within the run and retry automatic bind collisions within the readiness budget, while explicit ports fail without touching their owners. External allocation races remain possible and must produce attributable failure.

Alternative: background several whole `test-network` commands. This repeats preparation and lacks a shared worker bound, cancellation and coverage report.

### 3. Own a private Linux X11 display with software OpenGL

Use `xvfb-run --auto-servernum` to provide display allocation, Xauthority and wrapper cleanup. Supervise a runner-internal UI worker under the wrapper, forwarding signal cancellation and bounding process-tree cleanup. Detect display readiness using its own X11 connection, not a sleep. Run a small window manager, Openbox, on that private display for predictable client sizing and window behavior; it must be owned by the same scope. Runtime checks use fixed screen dimensions initially 1920x1080x24 and a known window size. Resolution/fullscreen option matrices are excluded from recurring smoke; targeted checks can be added when a meaningful display regression justifies them.

The worker must validate its private-display context before launching clients. Its process environment contains the wrapper-provided `DISPLAY`/`XAUTHORITY`; remove inherited `WAYLAND_DISPLAY` and explicitly pass `--display-driver x11 --rendering-method gl_compatibility --rendering-driver opengl3 --audio-driver Dummy`. Never fall back to the host display or `--headless` for graphical checks. Preflight OpenGL on the private display with `glxinfo` and require OpenGL 3.3. Force Mesa software rendering using `LIBGL_ALWAYS_SOFTWARE=true`; report the actual renderer. Begin with one graphical scenario, `--max-fps 30`, and `LP_NUM_THREADS=2`, retaining normal physics ticks. These limits aim to preserve desktop responsiveness and require timing validation; they are not a GPU benchmark.

Local prerequisites: Xvfb/xvfb-run, xauth, xdpyinfo, glxinfo, Mesa GL/GLX software drivers and Openbox. Document distro package equivalents, e.g. Ubuntu's `xvfb`, `xauth`, `x11-utils`, `mesa-utils`, `libgl1-mesa-dri`, `libglx-mesa0`, and `openbox`. Local tasks never install them. The CI workflow explicitly provisions/verifies them on its disposable Ubuntu runner. Alternative: Xvfb in a container. A container adds packaging without improving display separation for this Linux scope. A hidden desktop window still shares the user's display and focus.

### 4. Isolate preferences and audio without changing interactive development

Apply temporary per-client `XDG_DATA_HOME`, `XDG_CONFIG_HOME` and `XDG_CACHE_HOME` through `ProcessStartInfo.Environment` for test children, while keeping required system runtime/tool variables. Use explicit session and engine-log paths. Prove the effective Godot `user://` path is under owned storage before changing preferences. Each client gets its own data home; resume/restart reuses that client's scope. Keep artifacts under ignored `logs/<run-id>/` after runtime state is removed.

Graphical test clients use `--supervised`, not `--automated`: the latter intentionally quits on disconnect and would defeat the real Reconnect-button check. Headless observers retain their existing automated behavior. Servers and network-only clients remain headless and audio-free. Dummy audio prevents test playback on the host; assert audio/menu state without claiming audible output. Ordinary `dev` and client launches inherit the user's desktop environment and preferences exactly as before.

Alternative: mute the user's audio bus or reuse the normal preference file. Both would affect the developer outside the test and introduce shared state.

### 5. Add narrow local UI observation and capture hooks, with real input for actions

Extend the supervised stdin driver only as needed for key events, read-only control bounds/window/audio/presentation observations, and named screenshot capture. Give controls stable node names when missing. Project plot positions through the active camera for world picks; use current control bounds for clicks instead of fixed desktop coordinates. Route actions through `Input.ParseInputEvent` so selection, menu modality and normal control handlers are exercised; do not emit button signals or call purchase/settings callbacks directly. Helpers for a headless teammate can send ordinary protocol requests.

Gate extra hooks behind local supervision, not network RPCs. They must not award resources, set health or change match state. Keep local probe payloads separate from the shared gameplay protocol where possible, with request ids so an old observation cannot satisfy a new wait. No role, transport or authoritative rule changes are required.

Implement source smoke as independent vertical slices, each using one rendered client, one headless observer and a separate server with fresh owned match/session state. `economy` covers Start, plot/building picks and one coherent farm purchase/upgrade and barracks/recruit flow, catching broken picking, control routing and authoritative UI integration. `reconnect` covers a local transport disconnect followed by the visible Reconnect control and retained stable identity, catching presentation/recovery faults that headless protocol checks cannot see. `settings` covers menu modality and one representative Master-volume change persisted across an owned client restart, catching accidental gameplay input behind a modal and preference-path leakage. These are risk-driven integration slices, not exhaustive building, slider, display-option or gameplay-state matrices. Share setup helpers, not mutable matches or dependencies on earlier slices. Use ordinary protocol requests for minimal setup, then actual input for the behavior asserted by the selected slice. Reconnect may start in building phase; a paused-combat reconnect remains covered by the existing network scenario, avoiding a repeated long graphical battle.

When running the full source UI set, reuse the owned private display/window manager and prepared source resources, but dispose each slice's peers/data before starting the next. A single selected slice performs only its setup, feature assertions and cleanup; it must not call the full UI/network suite or CI to obtain prerequisites. Retain complete battle/victory/redistribution/defeat coverage in the network suite rather than repeating every long match graphically.

Capture building, settings and restored-connection checkpoints after `RenderingServer.frame_post_draw`. Check dimensions and meaningful image content together with loaded presentation/control observations and absence of unexpected resource/engine errors; keep screenshots for inspection. A capture alone is not a passed UI assertion. The exported smoke checks packed UI/model/music-resource loading, one ordinary-control purchase with an authoritative result and a rendered checkpoint, against the exported dedicated server without a source `--path`. Its value is catching package-only resource failures; it does not repeat every source UI flow or the existing headless exported gameplay smoke. Pixel-perfect screenshot baselines and full music listening remain outside this change.

Alternative: drive only existing text gameplay commands. That would test RPCs again but miss actual controls, world picking and settings input blocking.

### 6. Prepare once and keep CI's export barrier explicit

Use this ordering; preparation and exports remain serial because they mutate shared generated output:

```text
preflight + locked solution restore
                  |
                format
                  |
             build + import
                  |
          +-------+--------+
          |                |
        rules      network (2 workers)
          |                |
          +-------+--------+
                  |
       source UI slices (private display, serial)
                  |
        all pre-export checks passed
                  |
       templates -> client export -> server export
                  |
         headless exported-role smoke
                  |
         private-display exported UI smoke
```

The short rule run can overlap the network suite; run software-rendered UI after network verification by default to limit CPU contention. The CI supervisor bootstrap performed by mise and Godot release publishing during exports are separate measured phases; they do not justify repeating source preparation. Keep locked restores, the existing 15-minute CI job budget, the 15-second peer startup default and the 180-second network suite deadline. UI execution gets its own bounded suite deadline; it cannot consume an already-expired network timer. All failed source checks prevent exports; failed exported checks prevent overall success.

Alternatives: independent parallel export jobs would weaken the gate and contend for Godot/.NET caches; a full Windows/macOS matrix exceeds the requested scope. Existing tool declarations for those platforms are left intact without new support work.

### 7. Report evidence and measure the actual improvement

Add structured run/scenario result and timing summaries plus concise console output under the owned ignored evidence directory. Include selected coverage, jobs, start/end times, elapsed preparation/check/export phases, renderer, viewport and screenshot paths. Explicitly record expected occupied-port failure diagnostics; any unrelated Godot error or unexpected child exit fails its scenario. Do not log credentials or private preference/session contents.

Collect serial-baseline and parallel-result timings from the full-suite runs before and after substantial implementation, recording source/runner revisions, host, preparation state, unchanged gameplay coverage and per-scenario overlap. The baseline predates the new UI gates, so compare network time independently and report the added source/export UI cost. Reuse suitable existing boundary results; do not demand repeated full-network runs merely to calculate benchmark medians. Additional experiments during diagnosis should target the affected scenario or small scheduler workload. Saved historical timings are context, not an acceptance benchmark. Do not promise an arbitrary total speedup: the new graphical gates add coverage and may add time, while concurrency should reduce the network critical path. If evidence is inconclusive, record its limits without claiming improved performance.

Current xUnit tests all live in one class and their assertion time is tiny, so splitting classes for parallelism is not useful in this change. `--fixed-fps` removes real-time synchronization, but unbounded acceleration can outrun RPC/snapshot waits and distort connection/rate-limit behavior; defer it instead of adding it to these acceptance runs.

### 8. Use unit checks and selected slices between substantial task boundaries

Carry this execution policy into root `AGENTS.md` and README. A substantial task means a coherent feature/change or major implementation group that alters behavior across components; an individual edit or checklist checkbox is not automatically a new full-suite boundary. Record `mise run ci` before starting such a task and after it is complete, reusing an already successful before-result only if source and environment are unchanged. For this change, the existing CI is the before baseline and the final fresh-source CI in task 6.2 is the after run.

During implementation, use `mise run test` frequently for applicable rule changes and lightweight meaningful runner checks. Run only the relevant expensive case, such as `mise run test-network --scenario redistribution` or `mise run test-ui --scenario reconnect`, when developing or diagnosing that behavior. Implement and validate each UI vertical slice on its own before wiring the complete gate. Once checks pass, do not repeat them until related inputs change or a failure requires diagnosis. Documentation-only edits use documentation/plan checks. CI retains full coverage on workflow triggers; manual development cadence does not weaken its gate. No automated full CI runs after every checkbox, formatting pass or screenshot adjustment.

### 9. Make recurring checks a reusable repository-owned harness

Keep xUnit for engine-independent unit tests and grow DevRunner into a small typed scenario harness for real-process integration. Register stable scenario ids with descriptions, cost/risk rationale and bounded execution functions. Share owned peer fixtures, authoritative event assertions, event-driven waits, UI selectors/input/capture and failure reporting. Separate scenario definitions from process/display plumbing so adding a justified case requires a focused definition rather than copied launch scripts. Keep coverage selection and ownership contracts consistent across local and CI entry points.

Current verification notes describe external temporary SceneTree probes; no Python test suite or Python dependency is committed. Raw Python used during investigation may orchestrate processes or inspect evidence, but recurring acceptance checks must execute from a checkout using the named harness commands with no pasted Python, external `/tmp` probe assets or physical desktop automation. One-off investigation remains useful; its result does not replace a required repeatable check. Move only the probes needed by the accepted slices into supervision-gated, checked-in helpers, preserving the boundary between read-only observation and normal input/protocol actions.

Use small xUnit tooling checks for scheduler bounds, selection and first-failure cancellation where they catch meaningful concurrency bugs without Godot launches. Do not create tests that merely repeat implementation details or build a general test platform. A GdUnit/plugin migration would still require the existing multi-peer process/display ownership, add a second execution model and not directly remove the ad hoc orchestration; reconsider it separately if substantial engine-local unit coverage emerges.

### 10. Require value and a cheaper-coverage comparison for expensive additions

Document the expensive-test admission policy in AGENTS and README: identify the meaningful failure/regression, explain why existing unit/network coverage cannot catch it, estimate runtime/setup/maintenance cost, and choose the smallest independent slice. Prefer extending an existing case or a cheap unit assertion when sufficient. A simple control/option does not automatically merit E2E coverage; do not enumerate every feature combination. Preserve the current network assertions, but do not mirror them graphically. Add critical failure-path assertions selectively around risky boundaries such as cleanup and cancellation; use lightweight tooling checks for failure permutations when possible.

The initial three source slices have the rationale in decision 5, and the exported slice protects the distinct packaging boundary. Ready/Pause state rules remain covered by unit/network tests, with prior or targeted graphical observations recorded separately. Native display/fullscreen, multi-resolution and listening checks remain targeted manual observations unless a concrete regression warrants a new reusable slice. This prioritizes expected defects caught per runtime and maintenance cost rather than raw case count.

## Risks / Trade-offs

- [Software rendering consumes CPU and shaders need warmup] -> Cap FPS/Mesa threads, keep graphical scenarios serial, wait on readiness/rendered checkpoints, and record its cost independently.
- [A private X server alone does not model a desktop window manager] -> Run owned Openbox for predictable window behavior; retain explicit limits for KDE/Wayland, fullscreen/native monitors and physical input.
- [Inherited display or user-data paths leak into test children] -> Validate private X11 context and effective `user://` ownership before input/settings mutation; fail instead of falling back.
- [Concurrent scenario cancellation masks the original failure or leaks children] -> Per-scenario scopes, first-failure attribution and awaited bounded disposal; exercise interruption, timeout and worker-crash paths.
- [Polling observations miss transient state] -> Preserve event-driven waits, matching revisions and normal simulation timing; use request ids for local probes and one driver per child.
- [Current background-music/settings work is still in flight] -> Test the implemented controls without implementing missing features; reconcile node selectors against the final presentation and record unavailable feature dependencies as blockers rather than silently skipping required UI assertions.
- [Linux software graphics misses native platform/GPU/audio defects] -> Keep those limitations explicit in the verification record and retain targeted manual observations.

## Migration Plan

First validate Godot .NET rendering, a meaningful captured frame and a normal control click under the exact private-display configuration. Then add ownership/preparation and network scheduling, followed by reproducible UI smoke and CI integration. Preserve task names and existing assertions throughout. Update the durable specs by the usual sync/archive workflow only when implementation is complete; do not edit historical milestone artifacts as part of implementation.

After implementation and integration verification, make a final pass over root `AGENTS.md` and `README.md` so both describe the actual verified task names/options, Linux prerequisites, cost-based execution cadence, expensive-test admission policy, named reusable harness and independently selectable vertical slices, private-display isolation, evidence paths and CI gates. Create root `AGENTS.md` if absent, keeping it focused on repository guidance and preserving any existing instructions if it has been added meanwhile. Explicitly distinguish desktop `dev` from automated verification. Earlier task-level documentation stays current as work lands; this final pass reconciles both documents with the delivered behavior and recorded limitations, without another full-suite run solely for that documentation edit.

For regression diagnosis use `test-network --jobs 1` or a named scenario. If display prerequisites are missing, resolve them explicitly; never restore desktop-based tests as an automatic fallback. No data migration or deployment is involved.

## References

- [Godot command-line options](https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html): display/audio drivers, headless mode and FPS limits. The installed 4.7.2 `--help` confirms the selected flags.
- [Godot DisplayServer](https://docs.godotengine.org/en/stable/classes/class_displayserver.html): headless disables rendering/window management, so graphical checks require X11 on the private display.
- [Godot rendering architecture](https://docs.godotengine.org/en/stable/engine_details/architecture/internal_rendering_architecture.html): Compatibility uses desktop OpenGL 3.3.
- [Godot viewport capture](https://docs.godotengine.org/en/stable/tutorials/rendering/viewports.html): wait for `frame_post_draw` before reading the rendered image.
- [xvfb-run manual](https://manpages.debian.org/testing/xvfb/xvfb-run.1.en.html): automatic display selection, Xauthority and display process lifecycle.
- [Mesa environment variables](https://docs.mesa3d.org/envvars.html) and [LLVMpipe](https://docs.mesa3d.org/drivers/llvmpipe.html): software rendering and its CPU cost.
- [GitHub Ubuntu 24.04 runner inventory](https://github.com/actions/runner-images/blob/main/images/ubuntu/Ubuntu2404-Readme.md): current Xvfb availability; verify graphics utilities/drivers explicitly.
