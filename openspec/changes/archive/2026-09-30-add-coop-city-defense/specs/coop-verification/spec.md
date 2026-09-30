# Spec Delta

## Purpose

Verify the cooperative match's rules and real multiplayer lifecycle through the repository's existing bounded test tasks and export gates.

## ADDED Requirements

### Requirement: Engine-independent cooperative rules tests
The rules task SHALL verify economy/ownership validation, recruitment, upgrades, exactly-once production, ready eligibility, three-turn wave cadence, persistent soldiers, city defense, simultaneous eliminations, current/future enemy redistribution with integer remainders, and victory/defeat. It SHALL verify that pause freezes simulation and resume does not catch up. Tests SHALL run without Godot or a display and fail with nonzero exit status on violations. Coin-room-specific expectations SHALL be replaced by the cooperative match requirements.

#### Scenario: Redistribution regression
- **WHEN** enemy transfer duplicates an enemy, restores its health, loses a remainder, or assigns an enemy to a fallen city
- **THEN** a rule test fails and identifies the violated invariant

#### Scenario: Progression regression
- **WHEN** repeated readiness creates extra production, a wave begins at the wrong turn, or a fourth wave starts
- **THEN** a rule test fails

### Requirement: Real-process cooperative network verification
The network task SHALL launch separate real headless server/client processes using the normal game protocol and validate at least two clients, sender ownership, rejected actions, matching authoritative revisions, production and recruitment, automatic battles, and a terminal three-wave outcome. It SHALL cover a real disconnect and resumed client process with a changed transport connection, pause/resume during combat, invalid credentials, duplicate/retried spending, and current state restoration. A targeted three-client scenario SHALL validate visible immediate redistribution to two survivors and future wave allocation. Scenario actions SHALL use normal requests; no client-only test message SHALL award resources, kill cities, or set authoritative combat state.

#### Scenario: Resume a paused battle
- **WHEN** one client disconnects during combat, another pauses, and a replacement process resumes the disconnected player's session
- **THEN** both clients agree on the frozen authoritative gameplay state, the returning player's retained army/city, and resumed progression

#### Scenario: Retry an accepted economic action
- **WHEN** the network scenario resends a previously accepted recruitment command after reconnecting
- **THEN** clients observe one food deduction and one recruited soldier for that command

#### Scenario: Redistribute in real combat
- **WHEN** an under-defended city falls in a three-client scenario through normal combat
- **THEN** the surviving clients observe its remaining enemies transferred without duplication and its allocation included in the next wave

### Requirement: Bounded lifecycle and continued CI gates
Network scenarios SHALL retain configurable endpoints, readiness and assertion deadlines, attributable non-secret diagnostics, and cleanup of only their owned processes on success, failure, or interruption. Each automated client SHALL use isolated resume storage. Existing unavailable-server, stopped-server, occupied-port, and startup/child-failure checks SHALL remain meaningful. The existing task names and Linux CI ordering SHALL remain: checks and cooperative tests before client/server exports and exported-role smoke verification, without publishing or deployment.

#### Scenario: Missing reconnect or stalled battle
- **WHEN** the resumed client never synchronizes or a required battle transition does not occur
- **THEN** verification fails within its documented deadline, identifies the missing condition, and cleans up its server, clients, and temporary session files

#### Scenario: Cooperative tests fail
- **WHEN** a cooperative rules or network assertion fails
- **THEN** CI fails before either deliverable export is performed

### Requirement: Recorded graphical and clean-checkout verification
The change SHALL document a graphical multiplayer check of square-slot selection, purchase/upgrade/recruit controls, model/material loading, automatic battles and transfers, pause/reconnect feedback, and victory/defeat presentation. Verification SHALL include a fresh-source preparation and Linux client/server export smoke check. Unexecuted platform or graphical checks SHALL be recorded as limitations rather than passed.

#### Scenario: Review the completed milestone
- **WHEN** implementation is reported complete
- **THEN** its verification record distinguishes automated rule/network checks from actual graphical/export observations and any checks that could not be run
