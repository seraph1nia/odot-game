# Spec Delta

## MODIFIED Requirements

### Requirement: Bounded lifecycle and continued CI gates
Network and graphical scenarios SHALL retain configurable endpoints, readiness and assertion deadlines, attributable non-secret diagnostics, and cleanup of only their owned processes and displays on success, failure, or interruption. Each automated client SHALL use isolated resume storage, and graphical verification SHALL use isolated preferences on a private display. Existing unavailable-server, stopped-server, occupied-port, and startup/child-failure checks SHALL remain meaningful under serial and bounded parallel network execution. The existing task names SHALL remain, with an added Linux `test-ui` task. CI SHALL complete formatting, preparation, cooperative rules, all network scenarios and source graphical smoke before client/server exports. Independent checks SHALL be allowed to overlap after shared preparation, but a failed pre-export check SHALL prevent both exports. After successful exports, headless exported-role smoke and private-display exported-client graphical smoke SHALL gate overall success, without artifact uploads, publishing or deployment.

#### Scenario: Missing reconnect or stalled battle
- **WHEN** the resumed client never synchronizes or a required battle transition does not occur
- **THEN** verification fails within its documented deadline, identifies the missing condition, and cleans up its server, clients, and temporary session files

#### Scenario: Cooperative tests fail
- **WHEN** a cooperative rules or network assertion fails
- **THEN** CI fails before either deliverable export is performed

#### Scenario: Source UI verification fails
- **WHEN** private-display startup, rendering or a required source UI assertion fails
- **THEN** CI returns nonzero before either export and cleans up the owned graphical clients and display

#### Scenario: Exported UI verification fails
- **WHEN** the exported graphical client cannot load its packed presentation or fails a required graphical smoke assertion
- **THEN** CI returns nonzero with attributable diagnostics even if source checks and headless exported-role smoke passed

### Requirement: Recorded graphical and clean-checkout verification
The repository SHALL provide repeatable Linux source and exported-client graphical smoke on a private virtual display. Initial source coverage SHALL include representative hex plot/building selection, purchase/upgrade/recruit controls, model/material loading, reconnect feedback and settings modality/preference persistence, organized as independently selectable economy, reconnect and settings slices. Exported coverage SHALL use a minimal packed-resource/ordinary-input slice rather than duplicate every source flow. Verification SHALL also document observed pause feedback, automatic battles and transfers and victory/defeat presentation, distinguishing recurring smoke assertions from prior, manual or targeted checks. Adding expensive coverage SHALL follow a documented risk/cost admission policy rather than enumerate every simple feature or option. Verification SHALL include a fresh-source preparation and Linux client/server export smoke check. Records SHALL distinguish rules, headless networking, private-display graphics and actual native display, GPU, physical input and listening observations. Unexecuted platform, graphical or device checks SHALL be recorded as limitations rather than passed; private-display software rendering and silent audio SHALL NOT be claimed as native compositor, GPU-performance or audible-playback verification.

#### Scenario: Review the completed milestone
- **WHEN** implementation is reported complete
- **THEN** its verification record distinguishes automated rule/network checks from actual graphical/export observations and any checks that could not be run

#### Scenario: Repeat UI verification without desktop interaction
- **WHEN** a developer or Linux CI runs the graphical smoke tasks
- **THEN** assertions and rendered checkpoints run on a private display, identify their evidence, and do not require desktop focus or a physical screen
