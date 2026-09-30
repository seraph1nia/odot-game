# Spec Delta

## MODIFIED Requirements

### Requirement: Linux CI parity and evidence
The Linux verification workflow SHALL provision or verify the declared virtual-display and software-graphics prerequisites and execute the same verification entry point used locally. Every required graphical check SHALL execute in verification rather than silently skip when prerequisites are unavailable. Output SHALL identify run and scenario names, results, elapsed phase/scenario/total times, configured concurrency, actual graphical renderer and paths to local logs/screenshots. The verification record SHALL compare available serial-baseline and parallel-result network timings at substantial task boundaries against unchanged gameplay rules and comparable environment/preparation state, record source/runner revisions and any comparability limits, distinguish test execution from preparation/export time, and explicitly identify any unexecuted checks. Repeated full suites solely to populate benchmark samples SHALL NOT be a default development requirement. Screenshots and diagnostics SHALL remain in ignored workspace output without automatic upload, publishing or deployment. Ordinary push/PR verification and `mise run ci` SHALL remain non-publishing. A separate manually published-release workflow SHALL be allowed to build, transfer and publish distributable packages, checksums, and public build metadata after lightweight identity/checksum gates without rerunning verification; it SHALL NOT transfer or publish verification screenshots, logs, preferences, or credentials.

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
