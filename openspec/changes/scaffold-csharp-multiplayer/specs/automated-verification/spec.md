# Spec Delta

## Purpose

Make gameplay rules and real client/server behavior verifiable from the command line and gate CI export builds on successful checks without publishing deliverables.

## ADDED Requirements

### Requirement: Fast gameplay rules tests
`mise run test` SHALL run automated gameplay-rule checks without starting Godot or requiring graphics. Checks SHALL cover input validation, movement bounds, coin pickup, simultaneous pickup handling, scoring, and deterministic outcomes for the same initial state and input sequence. A failed assertion SHALL produce a nonzero exit code.

#### Scenario: Gameplay checks pass
- **WHEN** a contributor runs the rules test task on valid source
- **THEN** checks execute without engine processes and report success only when every assertion passes

#### Scenario: Gameplay regression
- **WHEN** movement or pickup behavior violates a checked rule
- **THEN** the rules test task fails and identifies the violated expectation

### Requirement: Real network integration verification
`mise run test-network` SHALL launch a real headless server and two automated headless Godot clients through the normal networking path. It SHALL verify joining, movement observed by both clients, a single score award for a coin generation, matching score and coin state, late-join initialization, and disconnect removal. The test SHALL require neither a display nor an audio device and SHALL be reproducible with controlled inputs and initial world state.

#### Scenario: Network verification passes
- **WHEN** the network test runs with working source and dependencies
- **THEN** it observes and asserts the required state transitions across separate networked processes
- **AND** it reports success only after all expectations pass

#### Scenario: Synchronization breaks
- **WHEN** clients connect but do not receive matching authoritative state after movement or collection
- **THEN** the network test exits unsuccessfully rather than treating connection alone as success

### Requirement: Bounded test lifecycle and diagnostics
Automated process verification SHALL use configurable endpoints, readiness checks, startup and assertion deadlines, and process cleanup on success, failure, or interruption. It SHALL avoid blindly killing unrelated processes or relying on arbitrary startup sleeps. Failed runs SHALL provide attributable logs and a useful failed expectation or timeout explanation.

#### Scenario: Missing readiness or stalled synchronization
- **WHEN** a process never becomes ready or a required state update never arrives
- **THEN** the test fails within its documented deadline and cleans up all processes it started
- **AND** logs identify the process and the condition that failed

#### Scenario: Port already occupied
- **WHEN** the selected test endpoint is already in use
- **THEN** the test chooses another available endpoint or fails clearly without terminating the process occupying it

### Requirement: CI export builds follow passing verification
GitHub Actions SHALL run on pull requests and pushes using the repository's declared tool versions. `mise run ci` SHALL run formatting checks, compile the code needed for tests, execute rules tests and network verification, and only then export Linux x86_64 client and dedicated-server builds. A failure in any preceding check SHALL prevent export builds. Export failures SHALL fail the workflow.

#### Scenario: Passing CI run
- **WHEN** every formatting, compilation, rules, and network check succeeds
- **THEN** CI exports both Linux deliverables successfully in its workspace

#### Scenario: A test fails
- **WHEN** a rules assertion or network verification fails
- **THEN** CI fails and neither client nor server export runs

### Requirement: Build-only CI output
The initial CI workflow SHALL keep exported outputs within the runner workspace. It SHALL NOT upload build artifacts, create or update releases, publish packages or containers, or deploy either role. The workflow SHALL use read-only repository permissions.

#### Scenario: Successful exports
- **WHEN** CI finishes exporting the client and server
- **THEN** no downloadable artifact, release, published package, deployment, or repository mutation is created by the workflow
