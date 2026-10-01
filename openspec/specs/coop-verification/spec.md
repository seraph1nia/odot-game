# coop-verification Specification

## Purpose

Verify the cooperative match's rules and real multiplayer lifecycle through the repository's existing bounded test tasks and export gates.

## Requirements

### Requirement: Engine-independent cooperative rules tests
The rules task SHALL verify economy/ownership validation, recruitment, upgrades, exactly-once production, ready eligibility, three-turn wave cadence, persistent soldiers, city defense, simultaneous eliminations, current/future enemy redistribution with integer remainders, and victory/defeat. It SHALL additionally verify typed recruitment and invalid-type atomicity, range-based stopping, body spacing, crowded entry queues, target invalidation, fixed-tick windup/impact/recovery, event deduplication and bounded history, stable combat identities and session cleanup. Tests SHALL include a deterministic mixed-army case with at least thirty-two soldiers and thirty-two enemies and check progress, bounds and separation. It SHALL verify that pause freezes simulation, pending impacts and events and resume does not catch up. Tests SHALL run without Godot, Steam, or a display and fail with nonzero exit status on violations. Shared authority/session tests SHALL verify equivalent solo, host-local, and guest request validation, authenticated identity binding, command retry protection, and stale-session rejection without changing gameplay rules.

#### Scenario: Redistribution regression
- **WHEN** enemy transfer duplicates an enemy, restores its health, loses a remainder, or assigns an enemy to a fallen city
- **THEN** a rule test fails and identifies the violated invariant

#### Scenario: Progression regression
- **WHEN** repeated readiness creates extra production, a wave begins at the wrong turn, or a fourth wave starts
- **THEN** a rule test fails

#### Scenario: Host and guest validation diverge
- **WHEN** a host-local action bypasses an ownership, cost, phase, or retry check enforced for guests
- **THEN** a shared authority test fails and identifies the differing behavior

#### Scenario: Contact or range regression
- **WHEN** units overlap, pass through opponents, become permanently blocked despite reachable targets, or shoot beyond their configured range
- **THEN** a core rule test fails with the violated position, spacing, progress or range invariant

#### Scenario: Timing or lifecycle regression
- **WHEN** an attack damages twice, hits an invalid target, advances during pause, or an ended match leaks combat state into a fresh session
- **THEN** a cheap rule or authority lifecycle test fails without launching an engine process

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
Network and graphical scenarios SHALL retain configurable endpoints, readiness and assertion deadlines, attributable non-secret diagnostics, and cleanup of only their owned processes and displays on success, failure, or interruption. Each automated client SHALL use isolated resume storage, and graphical verification SHALL use isolated preferences on a private display. Existing unavailable-server, stopped-server, occupied-port, and startup/child-failure checks SHALL remain meaningful under serial and bounded parallel network execution. Tests SHALL wait for observable session/state conditions rather than fixed sleeps. The existing task names SHALL remain, with a Linux `test-ui` task. Verification SHALL complete formatting, preparation, cooperative rules, all network scenarios and source graphical smoke before client/server exports. Independent checks SHALL be allowed to overlap after shared preparation, but a failed pre-export check SHALL prevent both exports. After successful exports, headless exported-role smoke and private-display exported-client graphical smoke SHALL gate overall success. Ordinary push/PR verification and `mise run ci` SHALL NOT upload artifacts, publish releases, or deploy. A separate manually published-release workflow SHALL build and attach distributable packages after lightweight tag/profile/identity/checksum checks without rerunning the test suites; this permission SHALL NOT extend to uploading verification logs, screenshots, or runtime data. Normal verification SHALL NOT require Steam login, accounts, relay availability, or internet for gameplay verification after dependencies are prepared. A separate explicitly invoked Steam verification SHALL report missing prerequisites or failed assertions distinctly; skipped or missing evidence SHALL NOT count as a Steam pass.

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
- **WHEN** an explicit release workflow builds both packages and passes its identity/checksum checks
- **THEN** its publication stage can upload only the selected distributables and public metadata

#### Scenario: Run CI without Steam
- **WHEN** ordinary verification runs on a prepared machine without Steam accounts or a Steam client
- **THEN** core, local process, UI, and exported local-role checks execute normally without claiming real-Steam acceptance

#### Scenario: Steam verification prerequisites are missing
- **WHEN** the separate Steam verification is requested without usable accounts, application access, or test machines
- **THEN** it reports the missing prerequisite without claiming a Steam pass or hiding a genuine attempted test failure

### Requirement: Recorded graphical and clean-checkout verification
Verification SHALL document and automate graphical start-screen navigation, solo entry, hosted local lobby/gameplay, direct hex-plot and building selection, purchase/upgrade/recruit controls, model/material loading, settings input isolation, typed recruitment, rigged locomotion/melee/shooting/hit/death animation, contact positions, automatic battles and transfers, pause/reconnect feedback, return to menu, and application exit. Graphical E2E SHALL use real input through rendered controls and assert displayed feedback plus authoritative results; screenshots or protocol-only automation SHALL NOT substitute for those UI assertions. Verification SHALL include fresh-source preparation and Linux client/server export smoke for solo and playing-host flows. Unexecuted platform or graphical checks SHALL be recorded as limitations rather than passed. Expensive coverage SHALL follow the documented risk/cost admission policy. Records SHALL distinguish rules, headless networking, private-display graphics and actual native display, GPU, physical input and listening observations; software rendering and silent audio SHALL NOT be claimed as native compositor, GPU-performance or audible-playback verification.

#### Scenario: Review the completed milestone
- **WHEN** implementation is reported complete
- **THEN** its verification record distinguishes automated rule/network checks from actual graphical/export observations and any checks that could not be run

#### Scenario: Repeat UI verification without desktop interaction
- **WHEN** a developer or Linux CI runs the graphical smoke tasks
- **THEN** assertions and rendered checkpoints run on a private display, identify their evidence, and do not require desktop focus or a physical screen

#### Scenario: Build through rendered controls
- **WHEN** graphical E2E clicks a plot and the displayed construction button
- **THEN** the UI shows the correct selected plot and cost, the authority acknowledges the purchase, and another player's view agrees on the resulting state

#### Scenario: Exit via the displayed button
- **WHEN** graphical E2E activates Exit Game on the start screen
- **THEN** the actual process exits within its deadline and leaves no owned gameplay process

#### Scenario: Rigged resources fail in a package
- **WHEN** a required character, weapon, skeleton binding or clip is missing or unusable in the packed client
- **THEN** graphical package verification fails rather than silently substituting a dummy or relying on source assets

### Requirement: Real Steam multiplayer acceptance
A development milestone MAY close after passing local rules/authority, ENet process, source graphical, Linux export and single-account native compatibility checks when the owner explicitly defers friend testing. Its verification record SHALL identify the deferred real-Steam and release checks, and closure/archive SHALL NOT count as a Steam acceptance pass. When real Steam acceptance is requested, its separate run SHALL use compatible exported clients and distinct authenticated Steam accounts on separate machines. It SHALL verify private lobby creation, native overlay invitations, accepted invitations both while running and from launch, admission/ownership, shared gameplay, guest reconnect during pause, fresh-join refusal after start, and original-host exit. Development sessions SHALL default to AppID 480; genuine Steam cold-launch verification of this game's executable SHALL use its own configured AppID/launch registration and SHALL remain explicitly incomplete when unavailable. For that real Steam acceptance run, at least one connection SHALL run across different NAT-protected networks without manual port forwarding and record actual relay-route evidence. Local substitutes, explicit development launch arguments, and mocked invitation callbacks SHALL NOT count as proof of actual Steam transport, overlay, identity, relay behavior, or Steam launching the executable. Completion SHALL record extension provenance/hash, version and stability qualifications, engine/runtime, account/machine prerequisites without secrets, executed scenarios, and remaining limitations.

#### Scenario: Steam acceptance covers an exported session
- **WHEN** two exported clients on different networks complete the Steam invitation and gameplay route
- **THEN** the record identifies the original host and guest, matching accepted gameplay state, real Steam relay connectivity, paused reconnect, and host-ended feedback
- **AND** a missing or failed part remains explicitly incomplete

#### Scenario: AppID 480 session passes before game launch registration exists
- **WHEN** two development exports verify real invitations, shared gameplay, reconnect, and relay connectivity using AppID 480 without this game's own registered launch configuration
- **THEN** those observed checks are recorded as passing independently of production AppID setup
- **AND** genuine Steam cold-launch acceptance remains incomplete rather than being inferred from launch-argument tests

#### Scenario: Close a locally verified development milestone
- **WHEN** local and single-account compatibility gates pass and the owner explicitly excludes friend testing from the milestone
- **THEN** the development change can be synced and archived with the real-account, relay, own-AppID cold-launch and release-qualification checks recorded as deferred/unverified
- **AND** the record does not claim those checks passed or require friend testing to close that milestone

### Requirement: Bounded combat presentation coverage
One independently selectable source graphical combat slice SHALL exercise the actual melee and ranged recruitment controls and ordinary combat progression. It SHALL assert active locomotion, attack states, coherent contact positions, shot/hit event consumption, a casualty death sequence, freeze/resume behavior and fresh-session cleanup with rendered checkpoints and current presentation observations. Cheap tests SHALL own numerical and exhaustive lifecycle invariants; real-process network coverage SHALL extend existing combat/reconnect checks to typed units and action state. The graphical slice SHALL use owned setup, a single ordered driver per child, bounded observable waits and cleanup under the existing private-display harness, and run with the unfiltered source UI suite before exports. Packed graphical smoke SHALL check required rigged assets and clip bindings without repeating full headless battles. Its documented admission SHALL identify defects cheaper coverage misses, expected setup/runtime/maintenance cost, measured results and remaining platform limitations.

#### Scenario: Selected graphical combat slice
- **WHEN** only the combat presentation slice is selected on a prepared supported machine
- **THEN** it runs from its own fresh state, observes the required animation/contact milestones, retains non-secret PNG/log/timing evidence and releases its owned peers and display
- **AND** it does not depend on another slice, require a full three-wave graphical match or manipulate authoritative state through a test-only command

#### Scenario: Headless behavior stays graphical-independent
- **WHEN** typed armies battle on a stripped dedicated-server export or an automated headless role
- **THEN** the same state and attack rules advance without creating animated models, graphics, audio or a Steam session
