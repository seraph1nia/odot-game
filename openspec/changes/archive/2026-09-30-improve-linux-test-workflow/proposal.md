# Proposal

## Why

Graphical verification currently opens clients on the developer's desktop and can interrupt ordinary computer use. Automated networking is already headless, but its four independent scenarios run sequentially and CI repeats preparation, leaving avoidable latency while graphical coverage lacks a repeatable CI task.

## What Changes

- Add `mise run test-ui` for bounded Linux graphical checks on an owned Xvfb display with Mesa software OpenGL, Dummy audio, temporary preferences/sessions, screenshots, and automatic cleanup. The task must never use the developer's desktop display as a fallback.
- Implement expensive UI checks as small independent vertical slices selected by `test-ui --scenario NAME`, so work on one feature does not require clicking through the entire game. Keep the existing xUnit rules tests as the frequent cheap check.
- Make recurring verification repository-owned through a reusable C# scenario harness with named cases, shared fixtures/input helpers, bounded assertions, capture and cleanup. Replace reliance on temporary external probes or pasted Python orchestration for recurring acceptance checks; retain xUnit and the existing real-process supervisor.
- Set a higher bar for new expensive tests: document the meaningful regression caught, why cheaper coverage is insufficient, and expected cost. Start with economy, reconnect and settings source slices plus a minimal exported-package slice; avoid exhaustive feature/option matrices and duplicate long gameplay coverage.
- Run independent headless network scenarios with configurable bounded concurrency, defaulting to two workers; add scenario selection and a serial mode for diagnosis and timing comparisons. Preserve all current protocol, gameplay and failure coverage.
- Prepare C# and Godot resources once per verification invocation and reuse them across checks. Keep build/import/export mutations ordered; parallelize independent verification after preparation.
- Run the source UI smoke in Linux CI before deliverable exports, then check the exported client on the private display after exports. Keep the existing rule/network gates and headless exported-role smoke.
- Record phase/scenario timings, concurrency, renderer, screenshots and actual verification results. Document Linux prerequisites and the remaining native display, GPU and listening checks.
- Finish implementation with a final update of root `AGENTS.md` and `README.md` against the verified workflow, including cost-based test-selection and test-addition guidance: frequent cheap unit checks, affected individual network/UI slices during development, full suites before and after substantial implementation tasks, and risk/maintenance justification for new expensive coverage. Include private-display requirements, named harness commands and CI gates; create the root `AGENTS.md` if it is still absent.
- Preserve `mise run dev` and interactive clients on the user's desktop. This change targets Linux x86_64 locally and Ubuntu CI; it adds neither a container requirement nor Windows/macOS private-display support.
- Defer simulation acceleration until separately validated; speed improvements here come from concurrency, shared preparation and bounded graphical work.

## Capabilities

### New Capabilities

- `linux-test-execution`: Private-display graphical tasks, isolated test state, reusable named scenario harness, risk/cost-based coverage and execution, shared preparation, bounded parallel execution, attributable evidence, and Linux CI integration.

### Modified Capabilities

- `coop-verification`: Extend lifecycle and CI gates to parallel network and private-display UI verification; replace the milestone-only graphical recording requirement with repeatable graphical smoke plus explicitly recorded manual and clean-source evidence. Update the obsolete square-slot wording to the current hex plots.

## Impact

- `tools/DevRunner`: scenario scheduling, per-scenario ownership, child environment/log overrides, virtual-display supervision, UI checks, shared preparation and CI ordering.
- `src/Game/Main.cs` and presentation integration: narrow supervised local input/observation/capture hooks as needed; normal gameplay continues through existing controls and RPCs.
- `mise.toml`, `.github/workflows/ci.yml`, root `AGENTS.md`, `README.md`, and `docs/verification.md`: new task, Linux prerequisite setup, agent/contributor execution guidance, execution contracts and measured evidence.
- Linux graphical prerequisites include Xvfb, Xauthority/display utilities, Mesa OpenGL and a small private window manager for consistent window behavior. Local tasks report missing dependencies without installing tools; CI provisions its disposable runner explicitly.
- No gameplay balance/protocol changes, test RPCs that mutate authority, platform expansion, artifact uploads, publishing or deployment. Existing background-music/settings work remains a separate change; tests exercise the implemented presentation without claiming listening or native-display verification.
