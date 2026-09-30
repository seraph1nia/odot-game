# Repository guidance

Odot is a C# Godot cooperative game with a separate authoritative server.
Keep numerical rules in `src/Game.Core`, engine/presentation code in `src/Game`,
and verification/process supervision in `tools/DevRunner`. Use `Odot.slnx` for
the repository; Godot also needs its smaller `src/Game/Game.sln`.

Use the declared, locked Godot **.NET** and SDK versions. Local installation of
tools and OS packages is user-managed: report missing prerequisites rather than
installing/upgrading them. Keep NuGet restores locked except during an intentional
dependency update. Preserve tool/dependency locks and asset provenance. Generated
`.godot`, `bin`, `obj`, `dist`, sessions and logs stay untracked. Format changed
C# with `dotnet format Odot.slnx --no-restore` after restoring the solution.

## Choose checks by cost and affected behavior

- Run applicable cheap `mise run test` checks frequently while changing rules or
  runner infrastructure. This includes xUnit gameplay unit tests and lightweight
  runner tests; it starts no Godot processes.
- During implementation or diagnosis, choose the affected expensive case:
  `mise run test-network --scenario redistribution` or
  `mise run test-ui --scenario reconnect`, for example. Do not run unrelated
  matches or the full suite as a prerequisite for a selected slice.
- Run full `mise run ci` before and after a substantial implementation task.
  A coherent feature or major cross-component change is a task boundary; an
  individual edit, checkbox or formatting pass is not. Reuse a successful before
  baseline when its source/environment inputs are unchanged. Record the result
  and evidence; a filtered pass is partial coverage.
- Once a check passes, repeat it only for changed relevant inputs, a failure or
  an unresolved concern. Documentation-only edits need documentation/planning
  consistency checks, not another full game/export run. Normal CI triggers still
  require the complete suite.

## Admit expensive tests by regression value

Before adding a network/UI case, identify the meaningful defect it would catch,
why cheap or existing coverage misses it, and its expected execution, setup and
maintenance cost. Prefer extending existing coverage or adding a cheap unit
assertion. When E2E is justified, implement the smallest independently selectable
vertical slice with owned setup, feature assertions and cleanup. A simple option
does not automatically need E2E coverage. Avoid exhaustive option matrices or
repeating complete headless battles graphically. Preserve existing cooperative
assertions.

Use the reusable C# harness: `Scenarios.cs` owns scheduling/data/evidence,
`Child.cs` owns processes and bounded event waits, `NetworkTests.cs` and
`UiTests.cs` define scenarios, and `PrivateDisplay.cs` owns graphical execution.
Register stable names and risk descriptions. Reuse ordinary protocol assertions,
fresh UI observation ids, current selectors, actual input and frame capture.
Keep one ordered driver per child; its event reader is not for competing
consumers. Recurring acceptance checks must run from checked-in commands, without
pasted Python, external temporary SceneTree probes or physical desktop automation.
One-off diagnostic scripts may inspect evidence but do not replace those checks.

## Execution and ownership

Network ids: `authority-resume-victory`, `redistribution`, `defeat`,
`failure-cases`. Default concurrency is two independent scenarios;
`--jobs 1` keeps the same coverage serially. Preserve endpoint owners on bind
failures. UI source ids: `economy`, `reconnect`, `settings`; unfiltered source UI
runs all three serially. `test-ui --scenario exported-package` checks existing
exports without preparing source or implicitly rebuilding packages.

Private-display verification currently supports Linux x86_64 with the README's
Xvfb/Xauthority/X11/Mesa/Openbox prerequisites. Use owned X11 displays, software
OpenGL and Dummy audio; missing prerequisites/rendering must fail, with no
desktop fallback. `mise run dev` deliberately opens two playable desktop windows.
Keep that development behavior.

Verification owns temporary per-client XDG data/config/cache, credentials,
endpoints and explicit engine logs. An intentional restart reuses only its
client's owned state. Await owned peers/display cleanup before removing runtime
data, including on assertion failure, cancellation or timeout. Never change the
developer's preferences or kill unrelated processes. Retain non-secret logs,
timing JSON and PNG evidence under ignored `logs/<run-id>/`; never log credentials.

Standalone source tasks prepare safely. Within CI, restore/build/import source
once; overlap cheap tests with bounded network scenarios, then run source UI.
All source checks gate the sequential client/server exports, followed by
headless package smoke and graphical package smoke. Do not parallelize mutations
of shared build/import/export output. CI has no publishing/upload/deployment step.
Software-rendered frames and silent audio-state assertions do not establish
native compositor/GPU performance, physical input or listening quality.

See [README.md](README.md) for commands and
[docs/verification.md](docs/verification.md) for timings, coverage and limitations.
