# coop-verification Specification

## Purpose

Verify the cooperative match's rules and real multiplayer lifecycle through the repository's existing bounded test tasks and export gates.

## Requirements

### Requirement: Engine-independent cooperative rules tests
The rules task SHALL verify economy/ownership validation, typed recruitment, multi-resource costs, building upgrades, bounded research, symmetric faction profiles, mage/tower splash, exactly-once production, ready eligibility, three-production wave cadence and income-free final preparation, persistent soldiers, city defense, simultaneous eliminations, current/future enemy redistribution with integer remainders, and victory/defeat. It SHALL additionally verify typed recruitment and invalid-type atomicity, range-based holding, legal shared-hex footprints/capacity, faction-exclusive reservations, crowded entry queues, closest/initiative/seeded targeting, fixed-tick movement/windup/impact/recovery/death, event deduplication and bounded history, stable combat identities and session cleanup. Tests SHALL include a deterministic mixed-army case with at least thirty-two soldiers and thirty-two enemies and check ordinary completion without battle-stalled defeat, board bounds, capacity/footprint separation and conservation. It SHALL verify that pause freezes simulation, moves, pending impacts, death-space releases, no-progress deadlines and events and resume does not catch up. Cheap tests SHALL compare seeded combat traces under reversed insertion order and different session identities, validate configuration, exercise exact scheduling/target/route ties and unchanged retries, prove same-tick arrival/impact/death ordering, and verify no-progress/duration defeat reasons and normal-result precedence. Fixed-seed crowded setups SHALL cover bounded waiting, cleanup and transfer without requiring graphical processes. Cleared-board transfer coverage SHALL assert actual protected-entry admission at its compatible release bound and ensuing engagement, not just conserved queues. Small paired normal-profile/formation fixtures SHALL establish frontline support protection, queued melee access, effective Mage contribution against ordinary two-victim clustering and Crossbowman single-target advantage. Required ordinary three-wave strategies and these role/admission/crowd checks SHALL run at the early gameplay gate before broad protocol/reconnect/package integration. Tests SHALL run without Godot, Steam, or a display and fail with nonzero exit status on violations. Shared authority/session tests SHALL verify equivalent solo, host-local, and guest request validation, authenticated identity binding, command retry protection, and stale-session rejection without changing gameplay rules.

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
- **WHEN** units share reserved positions illegally, exceed hex capacity, enter an opposing hex, cross conflicting routes, become permanently blocked in an ordinary reachable setup, or shoot beyond their configured hex range
- **THEN** a core rule test fails with the violated position, capacity, reservation, progress or range invariant

#### Scenario: Timing or lifecycle regression
- **WHEN** an attack damages twice, hits an invalid target, advances during pause, or an ended match leaks combat state into a fresh session
- **THEN** a cheap rule or authority lifecycle test fails without launching an engine process

#### Scenario: Fractional research has a small effect
- **WHEN** a low-damage unit receives a five-percent research rank
- **THEN** rule tests verify the intended small numerical increase without truncation to zero or rounding to a whole extra damage point

#### Scenario: Seed or initiative regression
- **WHEN** a target tie, contested move or equivalent route depends on storage traversal, rendering, or an unchanged retry
- **THEN** a cheap reproducibility test fails with the seed, decision and first differing combat tick

#### Scenario: Corpse releases capacity too early
- **WHEN** a casualty stops blocking before its authoritative death deadline or a moving casualty loses an endpoint reservation
- **THEN** a cheap lifecycle test fails without launching Godot

#### Scenario: Stalled battle is bounded
- **WHEN** a no-damage or cyclic movement fixture reaches its declared combat limit
- **THEN** a cheap test observes battle-stalled defeat at the exact tick with preserved living health

#### Scenario: Early gameplay gate catches a blocked reinforcement
- **WHEN** conservation holds but transferred enemies cannot deploy or attack on a previously cleared default battlefield
- **THEN** the early gate fails before broad wire/reconnect/package integration proceeds
- **AND** reaching the stalled-fight deadline cannot count as passing ordinary reinforcement admission

#### Scenario: Reinforcement bound accounts for fragmentation
- **WHEN** a cleared protected entry has multiple death deadlines and its earliest release does not fit any queued profile
- **THEN** a cheap fixture verifies the retained first-admission bound from cumulative legal footprint releases and actual admission at the compatible deadline
- **AND** retries cannot postpone the bound or require the whole overflow allocation to fit at once

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
One independently selectable source graphical combat slice SHALL exercise the actual melee and ranged recruitment controls and ordinary combat progression. It SHALL assert active locomotion, attack states, distinct fixed hex positions and coherent reserved movement routes, shot/hit event consumption, a casualty death sequence aligned to authoritative reservation expiry, freeze/resume behavior and fresh-session cleanup with rendered checkpoints and current presentation observations. Cheap tests SHALL own numerical and exhaustive lifecycle invariants; real-process network coverage SHALL extend existing combat/reconnect/redistribution checks to typed units, seeded configuration, movement/death state and conserved reservations. The existing graphical reconnect slice SHALL restore current moving/dying state without historical effects, and packed smoke SHALL sample the same hex/action mapping. These extensions SHALL retain cooperative assertions and independently owned setup; exhaustive seed/capacity/timing matrices SHALL remain cheap coverage. A short early sub-selection of the existing source combat slice SHALL prove explicit abstract melee using actual rigs in two occupied neighboring cells, including near/far targets and a simultaneous exchange, before layout scale is accepted. It SHALL use ordinary setup and progression with retained frames; coordinate separation or protocol agreement alone SHALL NOT establish visual readability. The graphical slice SHALL use owned setup, a single ordered driver per child, bounded observable waits and cleanup under the existing private-display harness, and run with the unfiltered source UI suite before exports. Packed graphical smoke SHALL check required rigged assets and clip bindings without repeating full headless battles. Its documented admission SHALL identify defects cheaper coverage misses, expected setup/runtime/maintenance cost, measured results and remaining platform limitations.

#### Scenario: Selected graphical combat slice
- **WHEN** only the combat presentation slice is selected on a prepared supported machine
- **THEN** it runs from its own fresh state, observes the required animation/hex-position/death-deadline milestones, retains non-secret PNG/log/timing evidence and releases its owned peers and display
- **AND** it does not depend on another slice, require a full three-wave graphical match or manipulate authoritative state through a test-only command

#### Scenario: Headless behavior stays graphical-independent
- **WHEN** typed armies battle on a stripped dedicated-server export or an automated headless role
- **THEN** the same state and attack rules advance without creating animated models, graphics, audio or a Steam session

#### Scenario: Reconnect observes a current death
- **WHEN** the existing reconnect slice synchronizes during an unexpired authoritative death
- **THEN** the rendered body samples its current pose without a historical sound and disappears at its declared deadline
- **AND** restored living units retain matching hex positions and action progress

#### Scenario: Early visual gate catches empty-air melee
- **WHEN** fixed-anchored models swing without a readable strike-to-target relationship across neighboring cells
- **THEN** the short source combat proof remains failed even if numerical range and occupancy assertions pass
- **AND** cue/anchor/spacing candidates are corrected before accepting the board layout

#### Scenario: Independently selected early melee proof
- **WHEN** `test-ui --scenario combat --checkpoint melee` is selected
- **THEN** the ordinary authority uses explicit seed 1 and ordinary two-Farm/Barracks recruitment supplies six Swordsmen, with ordinary progression into wave two permitted and a stop before wave three
- **AND** a shared simultaneous windup and later opposite near/far landed impact retain overview and close-view PNGs linked to actual actor/target nodes and action identities
- **AND** ordinary pause requests are timed from authoritative action milestones while live graphical observations establish the rendered proof
- **AND** the bounded checkpoint retains seed/configuration and live-node witnesses on success or failure and uses the existing owned combat setup and cleanup
- **AND** cheap seeded opportunity checks do not substitute for imported-rig, cue-readability or route-clearance evidence

### Requirement: Recorded strategy and focused graphical coverage
Verification SHALL compare checked-in ordinary-gameplay frontline, mixed-army, tower-heavy and research-heavy strategies in solo matches and retain a shared winning strategy across one through four players without privileged resource or casualty commands. Evidence SHALL record costs, production/spending stages, recruited archetypes, wave duration, casualties, remaining resources and city health. At least one reproducible ordinary strategy in each family SHALL win the default three-wave solo match; a no-investment strategy SHALL still lose. Cooperative success, elimination and redistribution assertions SHALL remain mandatory. These finite strategies SHALL NOT be reported as proof that every possible build is balanced. Graphical coverage SHALL extend existing selectable economy, combat, reconnect, settings and exported-package cases where practical; new expensive cases SHALL document their unique defect, missed cheaper coverage and expected cost before admission.

#### Scenario: New option has no viable strategy
- **WHEN** a default tower-heavy or research-heavy strategy cannot win despite correct ordinary actions
- **THEN** implementation records the result and adjusts profiles or costs before reporting balanced completion
- **AND** existing cooperative assertions are not removed to obtain a pass

#### Scenario: Observe a graphical specialist attack
- **WHEN** private-display combat coverage observes a Mage cast or a Catapult Tower impact
- **THEN** it checks actual rendered poses/effects against current authoritative events and captures attributable frames
- **AND** an overlapping snapshot or reconnect does not replay an old sound or effect

### Requirement: Isolated numerical action and decision acceptance

The cheap rules task SHALL include executable C# acceptance checks for current-action consistency, exact transition deadlines, rejected-reservation atomicity, legal shortest approaches, current objective ranking, stable unchanged retries and complete configured defense behavior. These checks SHALL run without Godot processes, Steam, a display, external numerical scripts or graphical probes. Small numerical policies SHALL be exercisable independently of a running match, with simulation-level checks establishing that the authority uses the same rules. Failure evidence SHALL identify the violated invariant and relevant seed, action, target or tick. Existing cooperative, ordinary strategy, role, crowded-completion and protected-admission coverage SHALL remain mandatory; new tests SHALL NOT substitute for those checks.

#### Scenario: Retained farther objective regression
- **WHEN** current eligible approach ranking is overridden by a previously selected farther objective
- **THEN** a cheap C# regression fails with the expected and actual objective identities
- **AND** an arrival or entry integration fixture exercises the same behavior through normal tick sequencing

#### Scenario: Defender configuration regression
- **WHEN** accepted defender victim cap or radius is ignored at impact
- **THEN** a cheap C# regression fails with expected and actual victim damage

#### Scenario: Shortest routes match an independent reference
- **WHEN** bounded numerical boards contain different legal footprint availability and opposing protected cells
- **THEN** C# checks compare approach distances against an independent reference search
- **AND** selected routes contain adjacent legal cells, fitting footprints and no repeated cell within the reconstructed route

#### Scenario: Equivalent decision inputs produce equivalent evidence
- **WHEN** fixed-seed canonical numerical setups use reversed insertion order or repeat unchanged blocked decisions
- **THEN** checks establish matching choices, action identities, reservations, impact victims and normalized traces

#### Scenario: Numerical checks run without the engine
- **WHEN** the repository's cheap rules task executes with only its declared .NET dependencies available
- **THEN** action, decision and defense regressions execute without launching Godot or requiring a graphical environment
- **AND** a violated numerical invariant produces a nonzero test result
