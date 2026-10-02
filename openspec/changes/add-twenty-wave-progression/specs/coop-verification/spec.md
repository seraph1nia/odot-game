# Spec Delta

## MODIFIED Requirements

### Requirement: Engine-independent cooperative rules tests
The rules task SHALL verify economy/ownership validation, typed recruitment, multi-resource costs, building upgrades, bounded research, symmetric faction profiles, mage/tower splash, exactly-once production, ready eligibility, three-production wave cadence and income-free final preparation, persistent soldiers, city defense, simultaneous eliminations, current/future enemy redistribution with integer remainders, and victory/defeat. It SHALL additionally verify typed recruitment and invalid-type atomicity, range-based holding, legal shared-hex footprints/capacity, faction-exclusive reservations, crowded entry queues, closest/initiative/seeded targeting, fixed-tick movement/windup/impact/recovery/death, event deduplication and bounded history, stable combat identities and session cleanup. Tests SHALL include a deterministic mixed-army case with at least thirty-two soldiers and thirty-two enemies and check ordinary completion without battle-stalled defeat, board bounds, capacity/footprint separation and conservation. It SHALL verify that pause freezes simulation, moves, pending impacts, death-space releases, no-progress deadlines and events and resume does not catch up. Cheap tests SHALL compare seeded combat traces under reversed insertion order and different session identities, validate configuration, exercise exact scheduling/target/route ties and unchanged retries, prove same-tick arrival/impact/death ordering, and verify no-progress/duration defeat reasons and normal-result precedence. Fixed-seed crowded setups SHALL cover bounded waiting, cleanup and transfer without requiring graphical processes. Cleared-board transfer coverage SHALL assert actual protected-entry admission at its compatible release bound and ensuing engagement, not just conserved queues. Small paired normal-profile/formation fixtures SHALL establish frontline support protection, queued melee access, effective Mage contribution against ordinary two-victim clustering and Crossbowman single-target advantage. Required ordinary twenty-wave strategies and these role/admission/crowd checks SHALL run at the early gameplay gate before broad protocol/reconnect/package integration. Economy coverage SHALL verify all six resources, the construction/material graph without producer ownership gates, zero-food recruitment, direct production, five purchased/nine total plots, escalating gold expansion quotes, locked-plot rejection, actual investment and per-resource half-refund rounding, retained land/army/research, fresh replacement instances, Market availability and fixed bundles, stale target/price rejection, checked arithmetic, and once-only per-battle upkeep including stronger-first/stable-identity allocation, skipped expensive units, inactive reserve persistence, capacity-queue distinction, exposed city siege and disconnected cities. Progression coverage SHALL verify all twenty authored compositions, mixed levels, both boss rounds, independent building and unit levels, rounded prices, unchanged veteran health, level-aware research, checked scaling, reward eligibility/deduplication, final reward ordering and no reward on defeat. Tests SHALL run without Godot, Steam, or a display and fail with nonzero exit status on violations. Shared authority/session tests SHALL verify equivalent solo, host-local, and guest request validation, authenticated identity binding, command retry protection, and stale-session rejection without changing gameplay rules.

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
The network task SHALL retain separate real headless dedicated-server/client scenarios and add a playing host with separate real guest processes using the normal session protocol over local networking. It SHALL validate at least two players, sender ownership, host-local validation, rejected actions, matching authoritative revisions, production and recruitment, automatic battles, and a terminal twenty-wave outcome. It SHALL cover a real guest disconnect and resumed process with a changed transport connection, pause/resume during combat, invalid credentials, duplicate/retried spending, current state restoration including unit levels, boss identity, all six balances, purchased plots, building investment/instance state, resolved transaction catalogs, upkeep forecasts/results and last-clear rewards, original-host termination, and fresh hosting after leaving. A targeted three-player scenario SHALL validate immediate redistribution to two survivors and future wave allocation. Scenario actions SHALL use normal requests; no client-only test message SHALL award resources, kill cities, or set authoritative combat state. Local scenarios SHALL run without Steam accounts, internet access, or Steam initialization.

#### Scenario: Resume a paused battle
- **WHEN** one client disconnects during combat, another pauses, and a replacement process resumes the disconnected player's session
- **THEN** both clients agree on the frozen authoritative gameplay state, the returning player's retained army/city, and resumed progression

#### Scenario: Retry an accepted economic action
- **WHEN** the network scenario resends a previously accepted recruitment command after reconnecting
- **THEN** clients observe one material/gold deduction, no recruitment food deduction and one recruited soldier for that command

#### Scenario: Redistribute in real combat
- **WHEN** an under-defended city falls in a three-client scenario through normal combat
- **THEN** the surviving clients observe its remaining enemies transferred without duplication and its allocation included in the next wave

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

### Requirement: Recorded strategy and focused graphical coverage
Verification SHALL compare checked-in ordinary-gameplay frontline, mixed-army, tower-heavy and research-heavy strategies in solo matches and retain a shared winning strategy across one through four players without privileged resource or casualty commands. Evidence SHALL record costs across all six resources, production/spending stages, plot purchases, building sales and exact refunds, Market trades, recruited archetypes and levels, building upgrade timing, upkeep demand/payment and participation, wave rewards, boss outcomes, wave duration, casualties, remaining resources and city health. At least one reproducible ordinary strategy in each family SHALL win the default twenty-wave solo match; a no-investment strategy SHALL still lose. Cooperative success, elimination and redistribution assertions SHALL remain mandatory. These finite strategies SHALL NOT be reported as proof that every possible build is balanced. Graphical coverage SHALL extend existing selectable economy, combat, reconnect, settings and exported-package cases where practical; new expensive cases SHALL document their unique defect, missed cheaper coverage and expected cost before admission.

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

### Requirement: Bounded combat presentation coverage
One independently selectable source graphical combat slice SHALL exercise the actual melee and ranged recruitment controls and ordinary combat progression. It SHALL assert active locomotion, attack states, distinct fixed hex positions and coherent reserved movement routes, shot/hit event consumption, a casualty death sequence aligned to authoritative reservation expiry, freeze/resume behavior and fresh-session cleanup with rendered checkpoints and current presentation observations. Cheap tests SHALL own numerical and exhaustive lifecycle invariants; real-process network coverage SHALL extend existing combat/reconnect/redistribution checks to typed units, seeded configuration, movement/death state and conserved reservations. The existing graphical reconnect slice SHALL restore current moving/dying state without historical effects, and packed smoke SHALL sample the same hex/action mapping. These extensions SHALL retain cooperative assertions and independently owned setup; exhaustive seed/capacity/timing matrices SHALL remain cheap coverage. A short early sub-selection of the existing source combat slice SHALL prove explicit abstract melee using actual rigs in two occupied neighboring cells, including near/far targets and a simultaneous exchange, before layout scale is accepted. It SHALL use ordinary setup and progression with retained frames; coordinate separation or protocol agreement alone SHALL NOT establish visual readability. The graphical slice SHALL use owned setup, a single ordered driver per child, bounded observable waits and cleanup under the existing private-display harness, and run with the unfiltered source UI suite before exports. Packed graphical smoke SHALL check required rigged assets and clip bindings without repeating full headless battles. Its documented admission SHALL identify defects cheaper coverage misses, expected setup/runtime/maintenance cost, measured results and remaining platform limitations.

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
- **WHEN** the existing economy and reconnect slices exercise resource-gated construction, plot purchase, building/Market sales, a recruitment-building upgrade, a subsequent material-funded recruit, upkeep resolution and an ordinary early-wave clear
- **THEN** real controls, current observations and retained frames verify displayed costs/refunds/lock state, disabled-action explanations, material recruitment without a food charge, battle-start food handling and restored economy/level/upkeep/last-clear state
- **AND** they do not need to replay twenty waves graphically to prove these mappings

