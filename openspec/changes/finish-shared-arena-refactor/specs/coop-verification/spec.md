# Spec Delta

## MODIFIED Requirements

### Requirement: Engine-independent cooperative rules tests

Fast C# coverage SHALL protect the current shared model: connected one-to-four-player geometry, separate squad/member identities, exclusive cells/static footprints, coordinated friendly movement/conflicts, physical objectives, uncapped X/Y including zero, reduced contribution after casualty, overkill, common simultaneous damage, immediate final-participant logical release and retained cosmetic state. It SHALL retain deterministic seeds/decisions/insertion-order checks, valid ranges/committed timing, bounded waiting/fight-result precedence, exact three-settlement/upkeep/forecast boundaries, individual progression/research/status/Hall recovery, atomic paid commands/refusals/retries and independent authority/session cleanup. Legacy city-transfer/body-size/corpse-entry/capped-splash assertions SHALL be replaced by these behaviors with recorded mappings; unrelated economic/security negatives SHALL remain. Small numerical encounters SHALL distinguish arithmetic inputs from ordinary paid play. Deep campaign/replay/stress matrices SHALL remain explicitly selected rather than appended to every edit.

#### Scenario: Legacy expectation replaced
- **WHEN** an old test expects a corpse to block a cell or a cap to truncate Y victims
- **THEN** its replacement protects immediate logical release or all eligible group victims and records the intentionally retired behavior

#### Scenario: Paid command regression
- **WHEN** regroup/recruit/return/deployment spends twice, stacks a home, heals a veteran or changes a foreign owner
- **THEN** a focused cheap regression fails without launching the engine

#### Scenario: Redistribution regression
- **WHEN** enemy transfer duplicates an enemy, restores its health, loses a remainder, or assigns an enemy to a fallen city
- **THEN** a rule test fails and identifies the violated invariant

#### Scenario: Progression regression
- **WHEN** repeated readiness creates extra production, a wave begins at the wrong turn, victory occurs before wave twenty, or a twenty-first wave starts
- **THEN** a rule test fails

#### Scenario: Host and guest validation diverge
- **WHEN** a host-local action bypasses an ownership, cost, phase, or retry check enforced for guests
- **THEN** a shared authority test fails and identifies the differing behavior

#### Scenario: Contact or range regression
- **WHEN** units share reserved positions illegally, stack separate squads in one cell, enter an opposing hex, cross conflicting routes, become permanently blocked in an ordinary reachable setup, or shoot beyond their configured hex range
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
- **WHEN** a final-participant casualty is observed
- **THEN** cheap lifecycle checks require immediate logical release and separately require its cosmetic body through the declared interval
- **AND** holding a logical cell for the clip or removing the body prematurely fails

#### Scenario: Stalled battle is bounded
- **WHEN** a no-damage or cyclic movement fixture reaches its declared combat limit
- **THEN** a cheap test observes battle-stalled defeat at the exact tick with preserved living health

#### Scenario: Early gameplay gate catches a blocked reinforcement
- **WHEN** a reachable shared-arena encounter conserves identities but queued/deployed groups cannot engage
- **THEN** cheap progression acceptance fails before native/package expansion
- **AND** BattleStalled cannot count as ordinary completion

#### Scenario: Reinforcement bound accounts for fragmentation
- **WHEN** an entry cell is logically released while cosmetic bodies remain
- **THEN** a queued squad can use one legal free cell immediately independent of body-size fragments/corpse deadlines
- **AND** canonical identities/claims remain conserved

#### Scenario: Reward or leveling regression
- **WHEN** death cleanup pays twice, a disconnected survivor misses its reward, an upgrade changes a veteran's level, or clients and authority disagree about recruitment cost
- **THEN** a cheap rule, catalog or authority test fails with the violated invariant

#### Scenario: Economy transaction regression
- **WHEN** construction spends only some required resources, a locked plot accepts a building, a refund includes land/research/recruits, or a Market trade works without a Market
- **THEN** a cheap rules or authority test fails with the incorrect resource, plot or transaction state

#### Scenario: Replacement and retry regression
- **WHEN** a sale retry pays twice or a delayed request aimed at a sold building changes its replacement
- **THEN** a cheap authority test fails and verifies the replacement investment and state remain intact

#### Scenario: Upkeep transition regression
- **WHEN** recruitment spends food, ready/unready charges upkeep, prior-death cleanup charges early, reconnect charges twice or a disconnected city's shortage follows different rules, or an unfed reserve deploys or screens city health
- **THEN** cheap rules and authority tests identify the wrong payment boundary or participation result without launching Godot

### Requirement: Real-process cooperative network verification

Existing bounded headless dedicated/playing-host/guest scenarios SHALL exercise normal ownership/commands, matching revisions, ordinary early paid progression, pause, real disconnect/process resume, credential/retry safety, current squad/member/arena/objective restoration and original-host loss. Solo/local authority SHALL remain equivalent to remote validation. Selected fallen-owner stored and active-field witnesses SHALL require their exact nonempty conjunctions, future exclusions and later active impacts where applicable; one SHALL not substitute for the other. Controlled authority fixtures SHALL be explicitly named, owned/headless/loopback constrained, disallowed for guests before session creation and labeled separately from earned balance. They SHALL not become public state mutation RPCs or a general loader. Protocol/state negatives SHALL refuse before partial live/credential mutation. Clients SHALL not award resources, kill cities or own combat. Real Steam acceptance SHALL remain a separate scope.

#### Scenario: Active fallen fighter resumes
- **WHEN** the selected controlled active-field witness reconnects a guest after Keep loss
- **THEN** it restores nonempty fighting members/claims/actions and receives their later real victim-bearing impact on the original host

#### Scenario: Ordinary paid cooperation
- **WHEN** normal headless peers recruit/pay/ready through an early wave
- **THEN** accepted progression and complete shared state agree without a controlled combat-state fixture

#### Scenario: Resume a paused battle
- **WHEN** one client disconnects during combat, another pauses, and a replacement process resumes the disconnected player's session
- **THEN** both clients agree on the frozen authoritative gameplay state, the returning player's retained army/city, and resumed progression

#### Scenario: Retry an accepted economic action
- **WHEN** the network scenario resends a previously accepted recruitment command after reconnecting
- **THEN** clients observe one material/gold deduction, no recruitment food deduction and one recruited soldier for that command

#### Scenario: Redistribute in real combat
- **WHEN** a Keep falls through normal combat in a three-client scenario
- **THEN** surviving clients observe nonteleporting enemy group preference redirection without duplication and the fallen original allocation in the next wave

#### Scenario: Playing host terminates
- **WHEN** the host process exits while a separate guest is connected
- **THEN** the guest receives ended-session or bounded connection-loss feedback and does not start its own simulation

#### Scenario: Fresh host after returning to menu
- **WHEN** a process ends a hosted session and hosts again
- **THEN** the runner observes a new match identity and no effect from old requests or connections

#### Scenario: Restore the expanded economy
- **WHEN** a city buys a plot, upgrades and sells a building and executes a Market sale through ordinary requests before reconnecting
- **THEN** peers agree on all six balances, permanent plot state, current building instances/refund quotes and the next expansion price
- **AND** retries of accepted transactions change none of those values a second time

#### Scenario: Reconnect across the upkeep boundary
- **WHEN** a disconnected living city crosses from preparation into combat and later resumes
- **THEN** its retained army and latest upkeep result agree with the authority and other clients
- **AND** the resumed process neither repays food nor changes this wave's already-resolved participation

### Requirement: Bounded combat presentation coverage

Cheap projection and minimal headless native components SHALL protect actor/member mapping, imported rigs/clips/socket/projectile resources, exact health/status, frozen physical death with advancing clips, cleanup, squad controls, deployment picking and all-owner camera/opaque bounds. Arena styling checks SHALL exercise actual loaded ground/dressing resources, bounded reuse/lifecycle and unobstructed targets across one-to-four-player geometry. These SHALL not establish pixels, choreography, physical input, sound quality or GPU performance. Graphical combat acceptance SHALL remain one explicitly authorized selected scenario with normal paid setup, real controls, current authoritative milestones, overview/close frames and owned cleanup. Its moving/dead visuals SHALL obey immediate logical release rather than corpse capacity. It SHALL remain independently selectable and separate from ordinary headless CI.

#### Scenario: Native styling boundary
- **WHEN** the minimal arena component receives repeated compatible state at supported viewport sizes
- **THEN** actual models/resources/picking/camera coverage and bounded static reuse are checked without claiming visual quality

#### Scenario: Authorized visual inspection
- **WHEN** the owner explicitly authorizes one combat presentation check
- **THEN** retained all-sector overview and close melee/ranged/death/structure views are checked for woodland cohesion, readable targets and unclipped controls
- **AND** unexecuted visual coverage remains visibly unverified

#### Scenario: Selected graphical combat slice
- **WHEN** only the combat presentation slice is selected on a prepared supported machine
- **THEN** it runs from its own fresh state, observes the required animation/hex-position/death-deadline milestones, retains non-secret PNG/log/timing evidence and releases its owned peers and display
- **AND** it does not depend on another slice, require a full twenty-wave graphical match or manipulate authoritative state through a test-only command

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
- **THEN** ordinary authority seed 1 and an ordinary material-funded, fed melee opening provide simultaneous near/far attack opportunities within a documented bounded early-wave setup
- **AND** a shared simultaneous windup and later opposite near/far landed impact retain overview and close-view PNGs linked to actual actor/target nodes and action identities
- **AND** ordinary pause requests are timed from authoritative action milestones while live graphical observations establish the rendered proof
- **AND** the bounded checkpoint retains seed/configuration and live-node witnesses on success or failure and uses the existing owned combat setup and cleanup
- **AND** cheap seeded opportunity checks do not substitute for imported-rig, cue-readability or route-clearance evidence, and the setup does not require a full twenty-wave graphical match

#### Scenario: Focused progression UI coverage
- **WHEN** explicitly selected deep economy and reconnect journeys exercise resource-gated construction, plot purchase, building/Market sales, a recruitment-building upgrade, a subsequent material-funded recruit, upkeep resolution and an ordinary early-wave clear
- **THEN** real controls, current observations and retained frames verify displayed costs/refunds/lock state, disabled-action explanations, material recruitment without a food charge, battle-start food handling and restored economy/level/upkeep/last-clear state
- **AND** they do not need to replay twenty waves graphically to prove these mappings

### Requirement: Recorded strategy and focused graphical coverage

Landing this refactor SHALL prioritize a small reproducible acceptance sample over exhaustive balance. Existing passing frontline/mixed/tower/research/reserve paid solo campaign controls SHALL remain protected. A bounded complete-force encounter SHALL demonstrate six Crossbowmen in two three-member squads surviving the default first-wave composition with normal profiles and no melee, without arithmetic resources being reported as earned economy. Retained seeds1/123 SHALL be used and actual profile/deployment/health disclosed. Pure-Crossbow paid opening/progression SHALL be retained as diagnostic evidence and owner-approved deferred balancing, not required landing acceptance. A viable paid cooperative strategy SHALL clear twenty waves for one through four players in a finite declared seed sample. A bounded cooperative rescue SHALL include physical cross-sector travel and a landed allied impact on an enemy threatening another surviving owner's Keep before a team clear; zero foreign impacts or mere surviving identities SHALL not satisfy rescue.

Measurements SHALL disclose real commands/quotes/refunds, production gates, materials, food funding, population, casualties, Keep/Watchpost damage and outcomes. Diagnostic losses SHALL remain losses even if accounting assertions pass. Late pure-ranged campaign losses SHALL be recorded for future balancing rather than automatically expand this finishing change; full-six ranged survival and basic cooperative viability SHALL remain required. No grants, hidden fourth edit, seed substitution or waived rescue assertion SHALL hide failures. Repeated-seed/profile tuning SHALL stop once these bounded obligations and regression checks hold; universal equal-cost strength or every-policy twenty-wave wins SHALL not be claimed.

#### Scenario: Ranged paid acceptance
- **WHEN** the all-Crossbow policy runs at seeds 1 and 123 with actual resource/food payments
- **THEN** actual outcomes remain diagnostic and failed rows are preserved as deferred balance work
- **AND** separately disclosed normal-profile full-six encounters meet survival assertions without melee or BattleStalled

#### Scenario: Cooperation is observable
- **WHEN** an ordinary paid rescue encounter clears
- **THEN** recorded source/movement/impact/victim/origin evidence attributes actual support against another owner's threat
- **AND** team clear without that evidence remains a failed rescue witness

#### Scenario: Measurement is not a win
- **WHEN** a diagnostic row ends in AllCitiesFallen
- **THEN** its accounting may pass but its reported outcome remains defeat and does not satisfy survival acceptance

#### Scenario: New option has no viable strategy
- **WHEN** a default tower-heavy or research-heavy strategy cannot win despite correct ordinary actions
- **THEN** implementation records the result and adjusts profiles or costs before reporting balanced completion
- **AND** existing cooperative assertions are not removed to obtain a pass

#### Scenario: Observe a graphical specialist attack
- **WHEN** private-display combat coverage observes a Mage cast or a Catapult Tower impact
- **THEN** it checks actual rendered poses/effects against current authoritative events and captures attributable frames
- **AND** an overlapping snapshot or reconnect does not replay an old sound or effect

#### Scenario: Twenty-wave balance evidence
- **WHEN** the checked-in fixed-seed strategy sample runs
- **THEN** frontline, mixed-army, tower-heavy and research-heavy solo strategies each demonstrate a complete ordinary-command twenty-wave win and a shared strategy wins with one through four players
- **AND** an ordinary strategy can afford, recruit and field a level-five unit before wave twenty, no-investment play loses through city-health defeat, and battle-stalled defeat never substitutes for a successful run

#### Scenario: Resource graph and first-wave viability
- **WHEN** ordinary strategies start with five usable plots and zero stone, metal and cloth
- **THEN** a checked-in opening can produce equipment, recruit and feed defenders before wave one
- **AND** later mixed/advanced paths demonstrate cloth/metal consumption, stone-funded upgrades and paid expansion or half-refund reconfiguration without hidden grants or capacity

#### Scenario: Spatial and food pressure across the campaign
- **WHEN** the twenty-wave strategy sample runs the integrated economy
- **THEN** evidence records producer upgrades versus plot purchases, Market versus Gold Mine investment and food supply against persistent armies
- **AND** nominal strategy success cannot omit upkeep, bypass required materials or silently enlarge the board

### Requirement: Isolated numerical action and decision acceptance

Small C# checks SHALL exercise legal shared shortest approaches, current objective selection, stable unchanged decisions, exclusive squad source/transit reconstruction, atomic refusal, exact deadlines, X/Y and physical defense profiles without Godot/display/Steam or a second simulator. Simulation-level checks SHALL establish production use of the same policies. Reversed insertion and seeded controls SHALL preserve normalized actors/victims/claims. Relevant failure evidence SHALL identify seed, action/target, tick and violated invariant. These checks SHALL complement paid/native/transport acceptance rather than replace it.

#### Scenario: Shared routing
- **WHEN** bounded connected arenas contain static/opposing blockage and foreign-sector reachable targets
- **THEN** reference distances and production routes agree on adjacent legal approaches

#### Scenario: Configured group damage
- **WHEN** a weapon has positive Y and more than three eligible group members
- **THEN** all actual secondary members receive the frozen configured damage without a retained victim cap

#### Scenario: Retained farther objective regression
- **WHEN** current eligible approach ranking is overridden by a previously selected farther objective
- **THEN** a cheap C# regression fails with the expected and actual objective identities
- **AND** an arrival or entry integration fixture exercises the same behavior through normal tick sequencing

#### Scenario: Defender configuration regression
- **WHEN** published physical weapon X/Y/range/timing differs from actual impact behavior
- **THEN** a cheap regression fails with the expected/actual group victims or timing
- **AND** old victim-cap/radius settings are refused rather than silently reinterpreted

#### Scenario: Shortest routes match an independent reference
- **WHEN** bounded connected boards have static/opposing blockage
- **THEN** C# compares legal approach distances against an independent reference
- **AND** routes are adjacent and exclusive without legacy protected-cell or size-fit assumptions

#### Scenario: Equivalent decision inputs produce equivalent evidence
- **WHEN** fixed-seed canonical numerical setups use reversed insertion order or repeat unchanged blocked decisions
- **THEN** checks establish matching choices, action identities, reservations, impact victims and normalized traces

#### Scenario: Numerical checks run without the engine
- **WHEN** the repository's cheap rules task executes with only its declared .NET dependencies available
- **THEN** action, decision and defense regressions execute without launching Godot or requiring a graphical environment
- **AND** a violated numerical invariant produces a nonzero test result
