# Spec Delta

## MODIFIED Requirements

### Requirement: Bounded lifecycle and continued CI gates
Network and graphical scenarios SHALL retain configurable endpoints, readiness and assertion deadlines, attributable non-secret diagnostics, and cleanup of only their owned processes and displays on success, failure, or interruption. Each automated client SHALL use isolated resume storage, and graphical verification SHALL use isolated preferences on a private display. Existing unavailable-server, stopped-server, occupied-port, and startup/child-failure checks SHALL remain meaningful under serial and bounded parallel network execution. The existing task names SHALL remain, with a Linux `test-ui` task. Verification SHALL complete formatting, preparation, cooperative rules, all network scenarios and source graphical smoke before client/server exports. Independent checks SHALL be allowed to overlap after shared preparation, but a failed pre-export check SHALL prevent both exports. After successful exports, headless exported-role smoke and private-display exported-client graphical smoke SHALL gate overall success. Ordinary push/PR verification and `mise run ci` SHALL NOT upload artifacts, publish releases, or deploy. A separate explicitly triggered versioned distribution workflow SHALL be allowed to publish distributable packages only after its source and package verification gates pass; this permission SHALL NOT extend to uploading verification logs, screenshots, or runtime data. Normal verification SHALL NOT require Steam login or real Steam service availability after dependencies are prepared. Real Steam acceptance SHALL remain separate and skipped or missing evidence SHALL NOT count as a Steam pass.

#### Scenario: Missing reconnect or stalled battle
- **WHEN** the resumed client never synchronizes or a required battle transition does not occur
- **THEN** verification fails within its documented deadline, identifies the missing condition, and cleans up its server, clients, and temporary session files

#### Scenario: Cooperative tests fail
- **WHEN** a cooperative rules or network assertion fails
- **THEN** verification fails before either deliverable export is performed

#### Scenario: Source UI verification fails
- **WHEN** private-display startup, rendering or a required source UI assertion fails
- **THEN** verification returns nonzero before either export and cleans up the owned graphical clients and display

#### Scenario: Exported UI verification fails
- **WHEN** the exported graphical client cannot load its packed presentation or fails a required graphical smoke assertion
- **THEN** verification returns nonzero with attributable diagnostics even if source checks and headless exported-role smoke passed

#### Scenario: Ordinary CI succeeds
- **WHEN** normal push/PR verification or `mise run ci` succeeds
- **THEN** outputs remain local to its workspace and it performs no uploads or publication

#### Scenario: Versioned distribution succeeds
- **WHEN** an explicit release workflow passes all required source and installed-package checks
- **THEN** its publication stage can upload only the selected distributables and public metadata

#### Scenario: Run CI without Steam
- **WHEN** ordinary verification runs on a prepared machine without Steam accounts or a Steam client
- **THEN** core, local process, UI, and exported local-role checks execute normally without claiming real-Steam acceptance
