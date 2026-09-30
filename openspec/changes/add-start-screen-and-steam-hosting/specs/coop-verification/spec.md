# Spec Delta

## MODIFIED Requirements

### Requirement: Engine-independent cooperative rules tests
The rules task SHALL verify economy/ownership validation, recruitment, upgrades, exactly-once production, ready eligibility, three-turn wave cadence, persistent soldiers, city defense, simultaneous eliminations, current/future enemy redistribution with integer remainders, and victory/defeat. It SHALL verify that pause freezes simulation and resume does not catch up. Tests SHALL run without Godot, Steam, or a display and fail with nonzero exit status on violations. Shared authority/session tests SHALL verify equivalent solo, host-local, and guest request validation, authenticated identity binding, command retry protection, and stale-session rejection without changing gameplay rules.

#### Scenario: Redistribution regression
- **WHEN** enemy transfer duplicates an enemy, restores its health, loses a remainder, or assigns an enemy to a fallen city
- **THEN** a rule test fails and identifies the violated invariant

#### Scenario: Progression regression
- **WHEN** repeated readiness creates extra production, a wave begins at the wrong turn, or a fourth wave starts
- **THEN** a rule test fails

#### Scenario: Host and guest validation diverge
- **WHEN** a host-local action bypasses an ownership, cost, phase, or retry check enforced for guests
- **THEN** a shared authority test fails and identifies the differing behavior

### Requirement: Real-process cooperative network verification
The network task SHALL retain separate real headless dedicated-server/client scenarios and add a playing host with separate real guest processes using the normal session protocol over local networking. It SHALL validate at least two players, sender ownership, host-local validation, rejected actions, matching authoritative revisions, production and recruitment, automatic battles, and a terminal three-wave outcome. It SHALL cover a real guest disconnect and resumed process with a changed transport connection, pause/resume during combat, invalid credentials, duplicate/retried spending, current state restoration, original-host termination, and fresh hosting after leaving. A targeted three-player scenario SHALL validate immediate redistribution to two survivors and future wave allocation. Scenario actions SHALL use normal requests; no client-only test message SHALL award resources, kill cities, or set authoritative combat state. Local scenarios SHALL run without Steam accounts, internet access, or Steam initialization.

#### Scenario: Resume a paused battle
- **WHEN** one client disconnects during combat, another pauses, and a replacement process resumes the disconnected player's session
- **THEN** both clients agree on the frozen authoritative gameplay state, the returning player's retained army/city, and resumed progression

#### Scenario: Retry an accepted economic action
- **WHEN** the network scenario resends a previously accepted recruitment command after reconnecting
- **THEN** clients observe one food deduction and one recruited soldier for that command

#### Scenario: Redistribute in real combat
- **WHEN** an under-defended city falls in a three-client scenario through normal combat
- **THEN** the surviving clients observe its remaining enemies transferred without duplication and its allocation included in the next wave

#### Scenario: Playing host terminates
- **WHEN** the host process exits while a separate guest is connected
- **THEN** the guest receives ended-session or bounded connection-loss feedback and does not start its own simulation

#### Scenario: Fresh host after returning to menu
- **WHEN** a process ends a hosted session and hosts again
- **THEN** the runner observes a new match identity and no effect from old requests or connections

### Requirement: Bounded lifecycle and continued CI gates
Network and graphical scenarios SHALL retain configurable endpoints, readiness and assertion deadlines, attributable non-secret diagnostics, and cleanup of only their owned processes on success, failure, or interruption. Automated clients SHALL use isolated resume and graphical preference storage. Existing unavailable-server, stopped-server, occupied-port, and startup/child-failure checks SHALL remain meaningful. The existing task names and Linux CI ordering SHALL retain checks and cooperative tests before client/server exports and exported-role smoke verification, without publishing or deployment. Normal CI SHALL NOT require Steam login, accounts, relay availability, or internet for gameplay verification after dependencies are prepared. A separate explicitly invoked Steam verification SHALL report missing prerequisites or failed assertions distinctly; skipped Steam coverage SHALL NOT count as a passing Steam test. Tests SHALL wait for observable session/state conditions rather than fixed sleeps.

#### Scenario: Missing reconnect or stalled battle
- **WHEN** the resumed client never synchronizes or a required battle transition does not occur
- **THEN** verification fails within its documented deadline, identifies the missing condition, and cleans up its server, clients, and temporary session files

#### Scenario: Cooperative tests fail
- **WHEN** a cooperative rules or network assertion fails
- **THEN** CI fails before either deliverable export is performed

#### Scenario: Run CI without Steam
- **WHEN** ordinary CI runs on a prepared machine without Steam accounts or a Steam client
- **THEN** core, authority, local process, and exported local-role checks can run and fail normally on regressions

#### Scenario: Steam verification prerequisites are missing
- **WHEN** the separate Steam verification is requested without usable accounts, application access, or test machines
- **THEN** it reports the missing prerequisite without claiming a Steam pass or hiding a genuine attempted test failure

### Requirement: Recorded graphical and clean-checkout verification
Verification SHALL document and automate graphical start-screen navigation, solo entry, hosted local lobby/gameplay, direct hex-plot and building selection, purchase/upgrade/recruit controls, model/material loading, settings input isolation, automatic battles and transfers, pause/reconnect feedback, return to menu, and application exit. Graphical E2E SHALL use real input through rendered controls and assert displayed feedback plus authoritative results; screenshots or protocol-only automation SHALL NOT substitute for those UI assertions. Verification SHALL include fresh-source preparation and Linux client/server export smoke for solo and playing-host flows. Unexecuted platform or graphical checks SHALL be recorded as limitations rather than passed.

#### Scenario: Review the completed milestone
- **WHEN** implementation is reported complete
- **THEN** its verification record distinguishes automated rule/network checks from actual graphical/export observations and any checks that could not be run

#### Scenario: Build through rendered controls
- **WHEN** graphical E2E clicks a plot and the displayed construction button
- **THEN** the UI shows the correct selected plot and cost, the authority acknowledges the purchase, and another player's view agrees on the resulting state

#### Scenario: Exit via the displayed button
- **WHEN** graphical E2E activates Exit Game on the start screen
- **THEN** the actual process exits within its deadline and leaves no owned gameplay process

## ADDED Requirements

### Requirement: Real Steam multiplayer acceptance
A separate Steam acceptance run SHALL use compatible exported clients and distinct authenticated Steam accounts on separate machines. It SHALL verify private lobby creation, native overlay invitations, accepted invitations both while running and from launch, admission/ownership, shared gameplay, guest reconnect during pause, fresh-join refusal after start, and original-host exit. Development sessions SHALL default to AppID 480; genuine Steam cold-launch verification of this game's executable SHALL use its own configured AppID/launch registration and SHALL remain explicitly incomplete when unavailable. At least one connection SHALL run across different NAT-protected networks without manual port forwarding and record actual relay-route evidence. Local substitutes, explicit development launch arguments, and mocked invitation callbacks SHALL NOT count as proof of actual Steam transport, overlay, identity, relay behavior, or Steam launching the executable. Completion SHALL record extension provenance/hash, version and stability qualifications, engine/runtime, account/machine prerequisites without secrets, executed scenarios, and remaining limitations.

#### Scenario: Steam acceptance covers an exported session
- **WHEN** two exported clients on different networks complete the Steam invitation and gameplay route
- **THEN** the record identifies the original host and guest, matching accepted gameplay state, real Steam relay connectivity, paused reconnect, and host-ended feedback
- **AND** a missing or failed part remains explicitly incomplete

#### Scenario: AppID 480 session passes before game launch registration exists
- **WHEN** two development exports verify real invitations, shared gameplay, reconnect, and relay connectivity using AppID 480 without this game's own registered launch configuration
- **THEN** those observed checks are recorded as passing independently of production AppID setup
- **AND** genuine Steam cold-launch acceptance remains incomplete rather than being inferred from launch-argument tests
