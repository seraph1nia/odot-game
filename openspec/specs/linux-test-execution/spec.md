# linux-test-execution Specification

## Purpose

Provide repeatable Linux verification that preserves desktop usability while running isolated graphical checks, bounded concurrent network scenarios, and attributable local and CI diagnostics.

## Requirements

### Requirement: Private-display Linux graphical execution
The repository SHALL expose `mise run test-ui` on Linux x86_64 to run graphical clients on an owned private virtual display with rendering active and silent audio. The task SHALL require no physical display or GPU and SHALL NOT create windows, move the pointer, inject input, or change focus on the user's desktop. Missing display or graphics prerequisites SHALL fail clearly with nonzero status without falling back to the user's display or installing dependencies. `mise run dev` and the interactive client task SHALL retain their desktop-window behavior. Windows/macOS private-display execution SHALL be outside this change's supported scope.

#### Scenario: Graphical verification while the desktop is in use
- **WHEN** a developer runs `mise run test-ui` while working in another desktop application
- **THEN** game rendering and automated input occur on the private display without disturbing the desktop's windows, pointer or focus

#### Scenario: No desktop session is available
- **WHEN** graphical verification runs on a Linux CI host with the declared software graphics and virtual-display prerequisites
- **THEN** the graphical clients render and complete checks without access to a physical screen, GPU or audio device

#### Scenario: Required virtual display cannot start
- **WHEN** a prerequisite is missing or the private display fails readiness
- **THEN** the task exits unsuccessfully with the missing prerequisite or readiness condition and does not launch clients on any inherited desktop display

#### Scenario: Interactive development remains graphical
- **WHEN** the developer runs `mise run dev`
- **THEN** the normal headless server and two playable desktop client windows start as before

### Requirement: Isolated runtime state and owned cleanup
Each verification run and independent scenario SHALL own distinct session, preference, engine-log and temporary display state. Client preferences and runtime output SHALL NOT overwrite the developer's normal game data. Deliberate client restart SHALL retain only that client's owned state for the scenario. Verification SHALL clean up only its owned processes, displays and temporary state on success, assertion failure, timeout, child exit or interruption, while retaining attributable non-secret evidence in ignored local output.

#### Scenario: Test changes settings and restarts a client
- **WHEN** a graphical scenario changes a setting and restarts its client using the same owned data directory
- **THEN** the setting persists for that client, another scenario's settings remain independent, and the developer's real preference and session files remain unchanged

#### Scenario: Verification is interrupted
- **WHEN** verification receives an interruption while peers and a private display are active
- **THEN** all owned peers and display processes exit within bounded cleanup time, owned temporary state is removed, and unrelated displays and processes remain running

### Requirement: Bounded parallel network scenarios
`mise run test-network` SHALL support `--jobs` with a positive integer worker limit and `--scenario` selecting one named scenario. With no scenario selected it SHALL execute every existing network scenario, using two workers by default and serial execution with `--jobs 1`. Independent scenarios SHALL use distinct endpoints and storage; actions dependent on another action within a scenario SHALL retain their required order. Invalid job values and unknown scenario names SHALL fail before game processes start. Explicit endpoint collisions SHALL fail without terminating the endpoint's owner. Parallel execution SHALL preserve all existing normal-protocol gameplay and lifecycle assertions.

#### Scenario: Complete suite uses bounded overlap
- **WHEN** the full network suite runs with two workers
- **THEN** independent scenarios overlap, no more than two scenarios run at once, each uses isolated peers and state, and all four scenario groups are reported

#### Scenario: Serial diagnosis preserves coverage
- **WHEN** the same full network suite runs with `--jobs 1`
- **THEN** it executes the same scenario groups and assertions in sequence

#### Scenario: One scenario is selected
- **WHEN** a developer selects a valid scenario name
- **THEN** only that scenario executes and the output clearly identifies the run as selected rather than a complete-suite pass

#### Scenario: A worker fails
- **WHEN** one active network scenario fails or times out
- **THEN** the suite returns nonzero, cancels pending work, cleans up all active owned peers, and reports the originating scenario and failed condition

### Requirement: Shared preparation before verification
A standalone network or UI invocation SHALL prepare the game before starting checks. Within a `ci` invocation, locked solution restore, game compilation and source resource import SHALL each run once before source verification, with their outputs reused by the source checks. Checks SHALL NOT implicitly repeat game preparation. Concurrent verification SHALL begin only after preparation completes; build, import and export operations sharing generated output SHALL NOT run concurrently. Rules-only `mise run test` SHALL continue to run without starting Godot.

#### Scenario: CI reuses its prepared game
- **WHEN** `mise run ci` executes formatting, rules, network and source UI checks
- **THEN** its execution report shows one locked solution restore, one game compilation and one source resource import before the checks that require them, excluding separately identified supervisor bootstrap and release export publishing

#### Scenario: Preparation fails
- **WHEN** compilation or source resource import fails
- **THEN** verification does not start against stale outputs and the invocation fails

### Requirement: Rendered UI smoke through ordinary controls
The initial source graphical smoke SHALL consist of three high-value slices: `economy` exercises Start, hex plot/building selection and a representative purchase/upgrade/recruitment flow; `reconnect` exercises the actual Reconnect control and retained identity; `settings` exercises menu input blocking and a representative volume preference persisted across an owned restart. Actions SHALL use input events routed through the actual UI. Checks SHALL observe authoritative gameplay results from a separate peer, verify graphical scene and model/material loading, and capture rendered frames at named checkpoints after rendering completes. Source clients SHALL be checked before CI exports. A bounded exported-package slice SHALL verify packed resources, one ordinary-control purchase and a rendered checkpoint without a source `--path` after successful exports. Local observation/capture hooks SHALL NOT bypass controls by invoking their action callbacks or introduce gameplay-state-mutating test RPCs. Missing required controls, failed assertions, absent frames or unexpected engine errors SHALL fail the task. Existing headless gameplay assertions SHALL remain; recurring UI smoke SHALL NOT duplicate their complete match flows or require every simple feature/option combination.

#### Scenario: UI purchase has an authoritative effect
- **WHEN** the test selects a hex plot and clicks its purchase control
- **THEN** the server accepts the ordinary request, another peer observes the expected building and resource deduction, and the graphical client produces a rendered checkpoint

#### Scenario: Settings block gameplay input
- **WHEN** the test opens settings and injects gameplay input behind the menu
- **THEN** no gameplay action is accepted from that input, menu controls remain usable, and the changed preference survives the owned client restart

#### Scenario: Exported presentation has packed resources
- **WHEN** CI runs the graphical export smoke after both exports succeed
- **THEN** the exported client loads its UI and required assets from the package, completes ordinary-control smoke assertions on the private display, and does not depend on a source project path

### Requirement: Independently selectable UI vertical slices
Graphical end-to-end coverage SHALL be organized into small named vertical slices with owned setup, feature-specific input/assertions and cleanup. `mise run test-ui --scenario NAME` SHALL execute only the selected slice, without executing earlier slices or the complete network/CI suite as prerequisites. Each slice SHALL establish its required match state through ordinary gameplay requests and SHALL be runnable in isolation. Without a selection, `test-ui` SHALL run all source UI slices serially; CI SHALL require all source UI slices and the exported-package slice at their prescribed gates. The exported-package slice SHALL also be selectable individually against existing exports, with clear failure for missing packages and without implicitly rebuilding exports. Unknown names SHALL fail before game processes start, and selected runs SHALL be identified as partial coverage.

#### Scenario: Reconnect is tested during a reconnect change
- **WHEN** a developer runs `mise run test-ui --scenario reconnect`
- **THEN** the task creates the required peers/state, tests the real Reconnect control and retained identity, and cleans up without running economy, settings, other UI slices or the full network suite

#### Scenario: Slice order is changed
- **WHEN** a source UI slice runs first or is selected by itself
- **THEN** it succeeds without depending on files, match state or prior actions from another slice

#### Scenario: All source UI coverage is required
- **WHEN** the unfiltered source UI task or CI's source graphical gate executes
- **THEN** all named source slices run and are reported, and any required slice failure fails the gate

### Requirement: Cost-based test execution guidance
Root `AGENTS.md` and `README.md` SHALL describe when to run each test tier. Cheap engine-independent unit tests SHALL be the frequent development check when applicable; expensive network and UI checks SHALL be selected by affected scenario during incremental implementation and diagnosis. A full verification run SHALL be required before and after a substantial implementation task, with an existing successful baseline reusable when the source and environment have not changed. Small edits, individual checklist items and documentation-only changes SHALL NOT automatically trigger full-suite reruns. Successful targeted checks SHALL NOT be repeated against unchanged inputs. CI SHALL still run the full required suite on its normal workflow triggers. The instructions SHALL distinguish targeted coverage from full-suite success and show individual test commands.

#### Scenario: Incremental feature work
- **WHEN** an agent makes a small change within an ongoing substantial task
- **THEN** it runs applicable cheap unit checks and the affected individual expensive slice as needed, without rerunning the entire suite after that edit

#### Scenario: Substantial implementation task boundaries
- **WHEN** an agent starts and completes a substantial implementation task
- **THEN** it records the full-suite baseline before changes and a full-suite result after completion, reusing an unchanged successful baseline rather than running it redundantly

#### Scenario: Documentation-only update
- **WHEN** only testing instructions or planning documents change
- **THEN** verification checks the relevant documentation/planning consistency rather than automatically launching all game tests and exports

### Requirement: Reusable repository-owned verification harness
Recurring network and graphical acceptance checks SHALL be checked-in named scenarios with reusable setup, bounded waits/assertions, input/capture and owned cleanup. Local and CI commands SHALL use the same harness and report the selected scenario and failure condition. Required recurring checks SHALL run from a checkout without pasted Python commands, external temporary probe scripts or physical desktop automation; no additional Python test runtime SHALL be required. Existing xUnit engine-independent unit tests SHALL remain available as the cheap tier. One-off investigative scripts SHALL NOT substitute for required recurring acceptance checks.

#### Scenario: Reproduce a graphical failure from a checkout
- **WHEN** a developer reruns a failing named UI scenario with the declared prerequisites
- **THEN** the repository command performs its setup, input, assertions, capture and cleanup without reconstructing an external script

#### Scenario: Shared infrastructure supports another justified case
- **WHEN** a new scenario meets the expensive-test admission policy
- **THEN** it reuses the harness's fixtures, bounded assertions and cleanup rather than introducing another copied launch/probe script

### Requirement: Risk-based admission of expensive tests
Root `AGENTS.md` and `README.md` SHALL set a higher admission bar for expensive network and UI tests than cheap unit checks. Each new expensive scenario SHALL document the meaningful defect or risk it catches, why cheaper existing coverage is insufficient, and its expected execution/setup and maintenance cost. Coverage SHALL prefer the smallest independently selectable vertical slice and reuse existing checks when sufficient. Simple feature changes SHALL NOT automatically require a new E2E case, exhaustive option matrices or graphical duplication of existing headless match assertions. Existing network coverage SHALL be preserved.

#### Scenario: A simple option is added
- **WHEN** a simple option is implemented and its relevant logic is adequately covered by cheap or existing checks
- **THEN** no new expensive E2E scenario is required solely to enumerate the option

#### Scenario: A cross-boundary regression needs UI coverage
- **WHEN** a meaningful input, recovery or packaging defect cannot be detected by cheaper existing coverage
- **THEN** a focused reusable slice or extension documents that gap and its cost, and can run individually without unrelated flows

### Requirement: Linux CI parity and evidence
The Linux CI workflow SHALL provision or verify the declared virtual-display and software-graphics prerequisites and execute the same verification entry point used locally. Every required graphical check SHALL execute in CI rather than silently skip when prerequisites are unavailable. Output SHALL identify run and scenario names, results, elapsed phase/scenario/total times, configured concurrency, actual graphical renderer and paths to local logs/screenshots. The verification record SHALL compare available serial-baseline and parallel-result network timings at substantial task boundaries against unchanged gameplay rules and comparable environment/preparation state, record source/runner revisions and any comparability limits, distinguish test execution from preparation/export time, and explicitly identify any unexecuted checks. Repeated full suites solely to populate benchmark samples SHALL NOT be a default development requirement. Screenshots and diagnostics SHALL remain in ignored workspace output without automatic upload, publishing or deployment.

#### Scenario: CI has no graphics prerequisites
- **WHEN** the workflow cannot provide a required graphical prerequisite
- **THEN** it fails visibly rather than reporting graphical verification as passed or skipped

#### Scenario: Performance is reviewed
- **WHEN** this change's implementation is reported verified
- **THEN** recorded comparable serial/parallel timings and coverage show the effect of bounded concurrency, and graphical timings are reported separately from network and export timings

#### Scenario: A graphical assertion fails
- **WHEN** a graphical assertion fails after rendering starts
- **THEN** diagnostics identify its scenario, failed condition and available screenshot/log paths without exposing resume credentials
