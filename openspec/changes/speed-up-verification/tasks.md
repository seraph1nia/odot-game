# Tasks

## 1. Coverage ownership and baseline evidence

- [x] 1.1 Add the assertion ownership/migration table to `docs/verification.md`, naming current helpers, C# replacements and retained transport/rendered witnesses; verify all six network and five source UI selectors, packed coverage and cooperative assertions are accounted for before deleting checks.
- [x] 1.2 Record source/environment identity and the existing 779.99s full-gate and 193.17s selected packed evidence, including their comparability limits; at implementation start verify whether an unchanged successful baseline can be reused, otherwise follow the repository's substantial-task baseline command, without rerunning suites solely to populate benchmarks.

## 2. C# session flows and campaign concurrency

- [x] 2.1 Add a small test-only authority flow driver for ordinary local/serialized remote commands, sequences, explicit clock, wire round-trips and bounded direct ticks; verify acceptance/rejection, tick bounds and deterministic reproduction using engine-independent xUnit checks.
- [x] 2.2 Add flow checks for reconnect across upkeep, pause/resume, economic retry/stale replacement, retained six-resource/level/size state and cooperative redistribution; verify each mapped replacement with applicable cheap tests and identify existing assertions reused rather than duplicated.
- [x] 2.3 Add one ordinary serialized-authority twenty-wave campaign with boss snapshot round-trips and final reward/no-wave-21 assertions using the existing strategy; verify bounded completion without sleep, grants, Godot or a display and retain all existing 21 strategy combinations.
- [x] 2.4 Split strategy families into disjoint campaign identities with a shared helper and at most two isolated test-process partitions after one build; verify discovery covers each existing combination exactly once, the general partition excludes them, and `mise run test` plus the in-process fallback preserve complete coverage.
- [x] 2.5 Document cheap flow ownership, campaign partitions/limits and seed failure evidence in README/verification guidance; verify documented commands and reported counts match the implemented test entry points and applicable cheap checks pass before expensive assertions move.

## 3. Bounded diagnostic delivery and retention

- [x] 3.1 Separate recent authoritative state from reliable command/error/lifecycle events in `Child`, replacing unbounded snapshot backlog with bounded state and single-driver wake notifications; verify fresh-id probes, exact checkpoint observations, ordered acknowledgements, early exits and visible event overflow with lightweight runner tests.
- [x] 3.2 Add buffered compact transcripts, a per-child recent full-state ring capped at 16 snapshots/16 MiB, checkpoint retention and failure/timeout/cancellation dumps; verify truncation reporting, final flush and attributable error context with runner tests using owned synthetic children rather than game battles.
- [x] 3.3 Prevent duplicate full snapshot transcripts in engine/supervisor output, retain bounded explicit engine diagnostics and redact credentials before every persistence path; verify errors still fail checks and secret fixtures never reach logs, including abrupt child failure and trace-mode output.
- [x] 3.4 Report evidence bytes, pacing, simulated ticks and actual worker/admission limits and document routine versus opt-in trace evidence; verify summary schema and documented defaults with cheap tests and inspect evidence from the next already-required affected scenario, without an extra campaign run for log sizing alone.

## 4. Owned fixed-tick acceleration

- [x] 4.1 Add the bounded stepping policy and verification `--simulation-speed 1..8` option with default 4 for headless/setup and 1 for graphical timing witnesses; verify invalid arguments, exact intervening steps, pause/resume without catch-up and ordinary/accelerated normalized-state/event equivalence in C#.
- [x] 4.2 Wire pacing into `Main` only for supervisor-owned verification launches, with scoped marker/token validation and local acknowledged setup-to-witness switches; verify unowned/guest requests cannot change pacing and ordinary interactive launches retain speed 1 using cheap policy tests and the affected owned process case.
- [x] 4.3 Bound snapshot publication in wall time while promptly delivering commands, phase/terminal changes and explicit checkpoints; verify transport continues processing, rate/connection deadlines remain wall-clock based and required transient witnesses remain observable in selected network/reconnect checks.
- [x] 4.4 Add normal-speed barriers to graphical drivers before animation/input timing assertions and document pacing/diagnosis commands; verify selected combat and reconnect witnesses observe acknowledged speed 1 and retain actual post-draw evidence, without running unrelated source slices.

## 5. Focused network and graphical witnesses

- [x] 5.1 Shorten `authority-resume-victory` to real dedicated-authority/peer ownership, retry, reconnect and ordinary early-clear witnesses, preserving its selector and rate-burst/failure checks; verify the selected case and the passing C# terminal/boss replacements, and update its risk/coverage description.
- [x] 5.2 Trim source economy supporting trades/recruits/progression to ordinary protocol setup while retaining each distinct actual control family, authoritative observer effects and supported-size geometry; verify selected economy coverage and update its mapping/risk description with measured timing.
- [x] 5.3 Remove repeated complete launcher/friends routes across viewports, reusing prepared-client resizing for geometry while retaining unique native modal/focus, solo/host/guest leave, resume and exit witnesses; verify selected launcher and affected settings coverage and document the preserved preference restart and native-dropdown limitation.
- [x] 5.4 Trim supporting combat/reconnect setup using accepted ordinary actions and acceleration, retain live rig/action/contact/death, picking/modal/freeze/recovery assertions and fresh captures; verify affected combat/reconnect selections and their normal-speed barriers, and update measured risk descriptions.
- [x] 5.5 Replace full packed combat-plus-menu replay with a focused packed launch/solo/purchase, asset/clip bindings, representative live animation, two-size layout and owned exit route; reuse it for installed-client graphical verification where applicable, verify selected `exported-package` against existing exports and package-inventory checks, and document source/package ownership without weakening headless package role checks.

## 6. Isolated graphical workers and shared admission

- [x] 6.1 Extend scenario admission for a shared expensive-scenario budget with linked first-failure cancellation and cleanup-before-slot-release; verify maximum active counts, serial coverage, sibling cancellation and source-failure prevention of exports using lightweight scheduler/gate tests.
- [x] 6.2 Move graphical scheduling to the parent, launching one selected worker per owned Xvfb/Xauthority/Openbox display with distinct endpoints, storage and report paths; verify isolation/path/summary behavior cheaply and run affected independent graphical slices with two workers to establish native focus separation and owned cleanup.
- [x] 6.3 Add standalone UI `--jobs 1..2`, CI total `--jobs` and graphical-cap `--ui-jobs 1..2` validation, keeping selected UI single-worker and existing network argument compatibility; verify defaults, serial override and invalid combinations in cheap option tests and update help/README in this group.
- [x] 6.4 Overlap prepared C#, network and source UI under the reported shared budget in `CiSource`, preserving one locked preparation, required local source gates and sequential exports; verify no mutation overlaps and failure/cancellation awaits all owners using cheap gate tests and affected source integration.
- [x] 6.5 Reconcile root guidance, README, mise entry points and relevant workflow arguments with new coverage/concurrency/pacing/evidence behavior; verify documentation consistency, all existing required GitHub job identities, the 300-second package default and absence of new dependency/install/upload/publishing actions.

## 7. Final integration and performance record

- [x] 7.1 Restore the solution in locked mode and format changed C# with `dotnet format Odot.slnx --no-restore`, then complete the final required `mise run ci`; verify all C# partitions, six network selectors, five source graphical slices, sequential exports and headless/graphical package gates pass with unchanged gameplay rules and owned cleanup.
- [x] 7.2 Record final source/environment inputs, actual overlap, per-tier wall time, simulated ticks/pacing and evidence volume alongside the historical baselines; verify moved assertions have passing owners, classify any serial/concurrency checks as partial coverage and report whether the 180–300s warm target was reached without repeating full CI solely for benchmarking.
