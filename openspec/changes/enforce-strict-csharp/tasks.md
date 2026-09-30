# Tasks

## 1. Strict policy and diagnostic fixes

- [x] 1.1 Record the unchanged full CI baseline; verify `mise run ci` passes and retain its console/timing evidence.
- [x] 1.2 Enable recommended SDK analyzers and build-time style enforcement in shared configuration, preserving nullable checking, implicit usings and warnings as errors; verify diagnostic probes fail with CS8603, CA2201 and IDE0044 and formatting drift is rejected.
- [x] 1.3 Fix all reported findings without exclusions or suppressions; verify strict solution compilation and both cheap xUnit suites, including the invariant-culture regression and existing owned-group cleanup fixture.
- [x] 1.4 Document the strict settings and verification loop in README; verify the commands correspond to the existing checked-in task/CI definitions.

## 2. Integration and specification consistency

- [x] 2.1 Run full final CI and record results in `docs/verification.md`; verify formatting, zero-warning compilation, all 37 xUnit cases, every network/source UI scenario, sequential exports and both package smoke checks pass.
- [x] 2.2 Sync the strict C# requirement and rejection/gate scenarios into `openspec/specs/linux-test-execution/spec.md`; verify `openspec validate --specs` and strict change validation succeed.
