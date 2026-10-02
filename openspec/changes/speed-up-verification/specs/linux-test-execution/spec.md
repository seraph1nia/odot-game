# Spec Delta

## MODIFIED Requirements

### Requirement: Shared preparation before verification
A standalone network or UI invocation SHALL prepare the game before starting checks. Within a `ci` invocation, locked solution restore, game compilation and source resource import SHALL each run once before source verification, with their outputs reused by the source checks. Checks SHALL NOT implicitly repeat game preparation. Concurrent verification SHALL begin only after preparation completes; build, import and export operations sharing generated output SHALL NOT run concurrently. Rules-only `mise run test` SHALL continue to run without starting Godot.

Prepared C# checks, network scenarios and source graphical slices SHALL support bounded overlap under one explicit process-scenario admission budget. Default CI admission SHALL permit at most two expensive scenarios total, with at most two graphical workers. A serial override SHALL retain identical required coverage. C# campaign workers SHALL have a separately reported finite CPU budget. On failure or cancellation, required source tasks SHALL stop admitting work, cancel siblings and await owned cleanup before reporting failure or permitting exports. Separate GitHub job workspaces SHALL remain independent and required; their preparation/output SHALL NOT be treated as a shared mutable workspace.

#### Scenario: CI reuses its prepared game
- **WHEN** `mise run ci` executes formatting, rules, network and source UI checks
- **THEN** its execution report shows one locked solution restore, one game compilation and one source resource import before the checks that require them, excluding separately identified supervisor bootstrap and release export publishing

#### Scenario: Preparation fails
- **WHEN** compilation or source resource import fails
- **THEN** verification does not start against stale outputs and the invocation fails

#### Scenario: Source checks overlap safely
- **WHEN** prepared CI schedules network and graphical scenarios together
- **THEN** active expensive scenarios stay within the reported shared limit
- **AND** a failing required check cancels siblings and prevents local client/server exports

### Requirement: Rendered UI smoke through ordinary controls
The source graphical smoke SHALL retain five selectable slices. `launcher` owns application navigation, modal focus and exit; `combat` owns live rig/action/hex rendering. The remaining slices are: `economy` exercises Start, hex plot/building selection and a representative purchase/upgrade/recruitment flow; `reconnect` exercises the actual Reconnect control and retained identity; `settings` exercises menu input blocking and a representative volume preference persisted across an owned restart. Actions SHALL use input events routed through the actual UI. Checks SHALL observe authoritative gameplay results from a separate peer, verify graphical scene and model/material loading, and capture rendered frames at named checkpoints after rendering completes. Source clients SHALL be checked before CI exports. A bounded exported-package slice SHALL verify packed resources, one ordinary-control purchase and a rendered checkpoint without a source `--path` after successful exports. Local observation/capture hooks SHALL NOT bypass controls by invoking their action callbacks or introduce gameplay-state-mutating test RPCs. Missing required controls, failed assertions, absent frames or unexpected engine errors SHALL fail the task. Existing headless gameplay assertions SHALL remain; recurring UI smoke SHALL NOT duplicate their complete match flows or require every simple feature/option combination.

Supporting setup SHALL use ordinary protocol requests where the setup action is not itself the input assertion. Repeated resource trades, recruitment or full menu routes SHALL NOT be driven graphically merely to establish game-flow arithmetic already owned by C#. The packed slice SHALL sample required model/clip/material/theme resources and one representative live animation without calling the complete source launcher/friends/hosted sequence. Resizing one owned prepared client SHALL provide supported-size layout evidence where equivalent to separate fresh setup, preserving the documented native-dropdown limitation and actual source Settings input coverage.

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
Graphical end-to-end coverage SHALL be organized into small named vertical slices with owned setup, feature-specific input/assertions and cleanup. `mise run test-ui --scenario NAME` SHALL execute only the selected slice, without executing earlier slices or the complete network/CI suite as prerequisites. Each slice SHALL establish its required match state through ordinary gameplay requests and SHALL be runnable in isolation. Without a selection, `test-ui` SHALL run all source UI slices with two graphical workers by default and identical serial coverage with `--jobs 1`; CI SHALL require all source UI slices and the exported-package slice at their prescribed gates. The exported-package slice SHALL also be selectable individually against existing exports, with clear failure for missing packages and without implicitly rebuilding exports. Unknown names SHALL fail before game processes start, and selected runs SHALL be identified as partial coverage.

Each concurrently active graphical worker SHALL own a distinct X11 display, authorization file, window manager, endpoints and client storage. `--jobs` SHALL accept one or two graphical workers and reject unsupported values before game processes start. A selected slice SHALL use one worker. One ordered driver per child SHALL be retained; sharing a child event reader or focusing windows on another worker's display SHALL NOT be used to achieve concurrency. The supervisor SHALL collect per-slice evidence without shared summary or renderer-file collisions and await display/peer cleanup before releasing a worker slot.

#### Scenario: Reconnect is tested during a reconnect change
- **WHEN** a developer runs `mise run test-ui --scenario reconnect`
- **THEN** the task creates the required peers/state, tests the real Reconnect control and retained identity, and cleans up without running economy, settings, other UI slices or the full network suite

#### Scenario: Slice order is changed
- **WHEN** a source UI slice runs first or is selected by itself
- **THEN** it succeeds without depending on files, match state or prior actions from another slice

#### Scenario: All source UI coverage is required
- **WHEN** the unfiltered source UI task or CI's source graphical gate executes
- **THEN** all named source slices run and are reported, and any required slice failure fails the gate

#### Scenario: Parallel graphical focus is isolated
- **WHEN** two graphical slices execute concurrently and each changes native window focus
- **THEN** their display identities, storage and evidence remain distinct and neither changes the other slice's focus or the developer desktop

#### Scenario: Serial graphical diagnosis
- **WHEN** `test-ui --jobs 1` is selected
- **THEN** the same five source slices and required assertions execute serially and the report identifies the worker limit

### Requirement: Risk-based admission of expensive tests
Root `AGENTS.md` and `README.md` SHALL set a higher admission bar for expensive network and UI tests than cheap unit checks. Each new expensive scenario SHALL document the meaningful defect or risk it catches, why cheaper existing coverage is insufficient, and its expected execution/setup and maintenance cost. Coverage SHALL prefer the smallest independently selectable vertical slice and reuse existing checks when sufficient. Simple feature changes SHALL NOT automatically require a new E2E case, exhaustive option matrices or graphical duplication of existing headless match assertions. Existing network transport and cooperative coverage SHALL be preserved. Detailed game-flow assertions SHALL be allowed to move into passing C# acceptance coverage when a checked-in mapping names their replacement and retained boundary witnesses; removing an assertion without a replacement SHALL NOT count as optimization.

#### Scenario: A simple option is added
- **WHEN** a simple option is implemented and its relevant logic is adequately covered by cheap or existing checks
- **THEN** no new expensive E2E scenario is required solely to enumerate the option

#### Scenario: A cross-boundary regression needs UI coverage
- **WHEN** a meaningful input, recovery or packaging defect cannot be detected by cheaper existing coverage
- **THEN** a focused reusable slice or extension documents that gap and its cost, and can run individually without unrelated flows

### Requirement: Linux CI parity and evidence
The Linux verification workflow SHALL provision or verify the declared virtual-display and software-graphics prerequisites and execute the same verification entry point used locally. Every required graphical check SHALL execute in verification rather than silently skip when prerequisites are unavailable. Output SHALL identify run and scenario names, results, elapsed phase/scenario/total times, configured concurrency, actual graphical renderer and paths to local logs/screenshots. The verification record SHALL compare available serial-baseline and parallel-result network timings at substantial task boundaries against unchanged gameplay rules and comparable environment/preparation state, record source/runner revisions and any comparability limits, distinguish test execution from preparation/export time, and explicitly identify any unexecuted checks. Repeated full suites solely to populate benchmark samples SHALL NOT be a default development requirement. Screenshots and diagnostics SHALL remain in ignored workspace output without automatic upload, publishing or deployment. Ordinary push/PR verification and `mise run ci` SHALL remain non-publishing. A separate manually published-release workflow SHALL be allowed to build, transfer and publish distributable packages, checksums, and public build metadata after lightweight identity/checksum gates without rerunning verification; it SHALL NOT transfer or publish verification screenshots, logs, preferences, or credentials.

Routine evidence SHALL use bounded snapshot retention, compact event/command/checkpoint summaries and buffered writes with final flushing. Required engine errors, exit diagnostics, command results and failure-context snapshots SHALL remain attributable; credentials SHALL NOT be persisted. Complete snapshot transcripts SHALL NOT be duplicated in engine and supervisor logs by default. A finite recent-state window SHALL be retained for timeout, assertion failure and cancellation, with checkpoint snapshots and PNGs retained on success. Reports SHALL include source/environment identity, pacing, worker/admission limits, wall time, simulated ticks and evidence bytes, and distinguish existing historical baselines from newly measured comparable results. Detailed full traces SHALL be opt-in and SHALL NOT be required for routine acceptance. The warm local 180–300 second optimization target SHALL be reported as a target until measured; slower cold CI runs SHALL NOT be disguised by skipped coverage or larger undocumented deadlines.

#### Scenario: CI has no graphics prerequisites
- **WHEN** the workflow cannot provide a required graphical prerequisite
- **THEN** it fails visibly rather than reporting graphical verification as passed or skipped

#### Scenario: Performance is reviewed
- **WHEN** this change's implementation is reported verified
- **THEN** recorded comparable serial/parallel timings and coverage show the effect of bounded concurrency, and graphical timings are reported separately from network and export timings

#### Scenario: A graphical assertion fails
- **WHEN** a graphical assertion fails after rendering starts
- **THEN** diagnostics identify its scenario, failed condition and available screenshot/log paths without exposing resume credentials

#### Scenario: Release workflow transfers packages
- **WHEN** an explicit release workflow transfers tested client packages to its publication stage
- **THEN** verification evidence and temporary player data stay in the verification workspace and only allowlisted distributables are transferred

#### Scenario: Diagnostic output stays bounded
- **WHEN** a long flow emits repeated large snapshots
- **THEN** ordinary retained evidence uses the documented finite recent-state window and checkpoints rather than two complete snapshot transcripts
- **AND** a failure retains its recent non-secret state and all attributable error/result diagnostics

#### Scenario: Performance claims remain comparable
- **WHEN** optimization results are recorded
- **THEN** the record distinguishes preparation, C#, networking, source graphics, exports and package checks and identifies source/environment differences
- **AND** it reports evidence volume and any unmet performance target without dropping required assertions
