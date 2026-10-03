# Spec Delta

## ADDED Requirements

### Requirement: Reproducible selected campaign performance evidence

Verification SHALL provide a checked-in command selecting one existing ordinary campaign by strategy, player count and seed, executing a separately reported warm-up and a bounded requested number of measured repetitions without Godot, Steam, graphical displays or unrelated test suites. Selection SHALL default to the existing frontline strategy with four players and seed 1. Invalid selections or repetition counts SHALL fail before campaign execution; failed gameplay assertions, cancellation or exceeded execution bounds SHALL return nonzero with attributable evidence. Each repetition SHALL own and dispose its simulation and retain the selected campaign's ordinary commands and gameplay assertions.

The command SHALL retain machine-readable non-secret evidence identifying source inputs, SDK/runtime, build configuration, measurement mode, selection, warm-up, iteration and concurrency. Measured samples SHALL report elapsed time, process CPU time, allocated managed bytes, GC collection deltas, combat ticks, unit workload and aggregate phase costs. Reports SHALL distinguish whole-workload metrics from combat, preparation/snapshot and diagnostic measurements and SHALL support CPU and allocation comparisons normalized by executed ticks. Process-wide collection scopes, instrumentation overhead and observed uncertainty SHALL be identified. Optional external traces SHALL identify their tool and collection mode; unavailable optional tools SHALL be reported unexecuted without substituting invented measurements or automatically installing software. Required built-in measurements SHALL work without external profilers. Profiling SHALL remain explicitly invoked and SHALL NOT add profiler dependencies or machine-dependent timing thresholds to normal CI. Profiling SHALL support explicit build-configuration selection with Release as its default while preserving the ordinary verification build configuration. Matched baseline and final runtime evidence SHALL include both solo and four-player frontline seed-1 campaigns; other supported selections SHALL remain available.

#### Scenario: Select the four-player campaign
- **WHEN** the default performance command is invoked on a prepared checkout
- **THEN** it executes only the frontline four-player seed-1 campaign with its ordinary assertions, separately records warm-up and measured repetitions, and disposes every owned match
- **AND** its evidence separates CPU, elapsed time, allocations, GC collection counts and simulated workload

#### Scenario: Reject invalid selection
- **WHEN** an unknown strategy, unsupported strategy/player combination or out-of-range repetition count is supplied
- **THEN** the command returns nonzero before starting a campaign and identifies the invalid argument

#### Scenario: Optional profiler is unavailable
- **WHEN** external tracing is requested but its user-managed prerequisites are absent
- **THEN** the trace is identified as unexecuted and the missing prerequisite is reported without installation
- **AND** built-in campaign measurements remain independently available and do not claim sampled stack or GC-pause evidence

#### Scenario: Compare before and after costs
- **WHEN** measurements from the same campaign, runtime, build configuration, repetition policy and measurement mode are compared across an optimization
- **THEN** the report identifies changed source inputs, executed ticks and command/outcome identity, and reports CPU and allocated-byte changes separately from elapsed time
- **AND** unmatched shared-CI, Debug/Release or instrumented/uninstrumented runs are identified as different conditions rather than equivalent benchmarks

### Requirement: Regression acceptance for optimized core execution

Cheap verification SHALL establish that optimized execution preserves pre-change deterministic ordered state, decisions, reservations, events, accepted requests and outcomes for bounded representative fixtures and ordinary campaigns. Session/match identity normalization SHALL NOT erase unit identity, tick ordering, configuration fingerprints, seed effects or command sequences. Verification SHALL cover same-tick mutations from requests, research, deaths, removals, admission and redistribution, including queued/reserve membership and dying-unit exclusion. City-local decision checks SHALL establish that relevant local changes invalidate stale observations while unrelated-city movement and health-only changes retain current unchanged-decision semantics. Existing fixed-seed strategies, serialized session flows, reversed-insertion traces, target/route ties, simultaneous impact, pause/resume, protected admission and terminal reward assertions SHALL remain required. Optimized single-target and splash selection, city exposure, defense targeting, temporary reachability, profile/quote reuse and command catalog lookups SHALL preserve their existing results, validation and deterministic ordering. Presentation optimizations SHALL preserve sampled motion/attack/death poses, event identity and consumption, reconnect baselines, focus restoration and current displayed values.

Returned snapshots SHALL remain detached from authoritative mutable state and from other snapshots. Regression checks SHALL exercise caller mutation of exposed nested unit, route, board, campaign, catalog and receipt arrays, verifying that later authority behavior and independently returned snapshots remain unchanged. Full serialized state comparison SHALL remain correctness coverage rather than an unreported per-tick cost inside performance measurements.

#### Scenario: Observe a same-tick transfer
- **WHEN** an enemy changes destination and is admitted without a new tick between observations
- **THEN** its current destination, identity and reservations appear exactly once with preserved health/profile/recovery and no stale former-city membership
- **AND** queued arrivals continue to prevent premature wave completion and dying units do not become living transfers

#### Scenario: Caller mutates a returned snapshot
- **WHEN** a caller modifies nested arrays in one returned snapshot
- **THEN** subsequent authoritative steps and independent snapshots retain the unmodified configured rules, units, decisions and receipts

#### Scenario: Local observation inputs change
- **WHEN** a local movement, casualty, transfer or admission changes decision-relevant battlefield inputs
- **THEN** subsequent decisions use the current local inputs
- **AND** unrelated-city movement or a health-only change does not cause a new route or tie choice solely through observation invalidation

#### Scenario: Optimize repeated state construction
- **WHEN** the same bounded setup, seed and ordered ordinary requests run before and after collection/projection changes
- **THEN** normalized ordered state/events and command outcomes agree with retained pre-change reference evidence
- **AND** all existing strategy combinations and required cooperative assertions remain in ordinary CI

### Requirement: Compact campaign evidence with attributable failures

Successful ordinary campaigns SHALL retain compact evidence for the required six-resource economy, spending/production, recruitment/progression, upkeep participation, rewards, wave ticks, casualties, remaining resources and city health. Detailed recent action/wave context SHALL be bounded and available on failure; explicit trace mode SHALL identify its greater retention cost. Assertions SHALL execute independently of output verbosity. Failure diagnostics SHALL identify strategy, player count, seed, configuration, wave/tick and the failed condition without credentials. Reducing formatted output SHALL NOT reduce acceptance coverage or make a missing campaign result appear passed.

#### Scenario: Run with compact output
- **WHEN** a required campaign runs with routine diagnostics
- **THEN** every ordinary assertion executes and its compact success evidence retains the required economic/progression/combat facts
- **AND** it does not need a formatted full-army trace for each successful action

#### Scenario: Fail after recent ordinary actions
- **WHEN** a campaign assertion fails after spending, recruitment or combat progression
- **THEN** bounded recent context and campaign identity identify the failed condition and the command returns nonzero
- **AND** optional detailed trace output is distinguishable from routine evidence and remains non-secret


### Requirement: Separate before and after runtime and verification baselines

Implementation acceptance SHALL retain successful unoptimized baseline measurements before runtime or campaign-harness optimizations and repeat the same workload families afterward. Measurement scaffolding and correctness fixtures SHALL be established and validated first, with a recorded source identity. Required families SHALL include ordinary solo/cooperative runtime, large-fight runtime, snapshot/transport processing, client presentation, cheap verification and complete CI. Historical CI timings SHALL NOT replace missing runtime or matched verification-speed baselines. A changed fixture, instrumentation schema or measurement policy SHALL require both sides to be recollected or the comparison to be explicitly reported as non-comparable.

The report SHALL separate runtime CPU, elapsed time, allocation, operation and frame costs from testing wall time, setup, diagnostics, admission waits and export costs. It SHALL identify test inventory changes, preserve existing coverage and report common-workload comparisons alongside raw suite totals when coverage grows. Default process budgets, gameplay speed and verification build configuration SHALL remain unchanged. Required missing, failed or timed-out measurements SHALL remain incomplete rather than passed. No speedup SHALL be claimed from reduced coverage, changed seeds/outcomes, different executed work or counter suppression.

Reports SHALL retain raw evidence and provide baseline/after absolute values, absolute and percentage deltas, sample counts, medians/ranges where repetitions exist, and uncertainty/limitations. Zero baselines SHALL have no invented percentage; tail percentiles SHALL identify their sampled distribution. Runtime and testing-speed conclusions SHALL be independently stated, including regressions or inconclusive results. A successful correctness gate SHALL NOT imply a performance improvement.

#### Scenario: Establish baselines before optimization
- **WHEN** the measurement scaffolding and fixtures are ready on the unoptimized implementation
- **THEN** the required runtime families, cheap tests and full CI are executed and their source, configuration, workload, environment, coverage and timing evidence are retained before optimizations begin
- **AND** the earlier historical full-CI record is labelled separately

#### Scenario: Complete the optimized comparison
- **WHEN** optimization implementation is ready for acceptance
- **THEN** the same runtime workload families, large-fight test, cheap suite and full CI execute again and the report presents separate runtime and testing-speed tables
- **AND** gains from compact diagnostics or faster strategy setup are not attributed to combat or rendered frame execution

#### Scenario: Coverage or measurement inputs differ
- **WHEN** new regressions increase test inventory or a fixture/instrumentation/build setting changes
- **THEN** the report identifies that difference and distinguishes raw suite totals from a valid common-workload comparison
- **AND** a changed runtime workload cannot be presented as the original workload's speedup

### Requirement: Deterministic algorithmic work evidence

Checked-in profiling commands SHALL offer opt-in operation counters with documented, versioned semantics and per-workload ownership. Evidence SHALL separately report world scans/materializations, units examined, sorts and elements sorted, unit projections/copied route data, local observations and examined inputs, target/splash candidate checks, reachability searches and cell/edge visits, occupancy/admission checks, profile resolutions/cache behavior, snapshot/codec calls and bytes, playback index/event work, pose evaluations and HUD refreshes. Unsupported measurements SHALL be explicitly unavailable. Counts SHALL describe executed semantic work, not claim a count of every calculation, CPU instruction, floating-point operation or energy usage; categories SHALL NOT be added into an unexplained efficiency score.

Evidence SHALL include total and per-tick costs, active-unit-ticks and applicable normalized costs, plus deployed/queued/reserve/dying workload distributions. Counter-enabled and counter-disabled runs SHALL preserve identical ordered gameplay results. Timing comparisons SHALL use matched counter modes and report counter-disabled timing separately from work-count runs and measured instrumentation overhead. Routine gameplay SHALL leave detailed counters disabled. Cheap deterministic tests SHALL verify counter semantics/reset/isolation and important eliminated work without hardware-dependent timing assertions.

#### Scenario: Explain eliminated single-target work
- **WHEN** a validated cap-one attack resolves with work counters enabled
- **THEN** its exact primary victim and event outcome match the established reference and no additional splash candidates are examined
- **AND** matched baseline evidence distinguishes the earlier candidate work from the optimized bypass

#### Scenario: Counters preserve behavior
- **WHEN** the same fixture runs with counters disabled and enabled
- **THEN** ordered state/events, choices and outcomes agree while only diagnostic work evidence differs
- **AND** a new match starts with independent reset counts

#### Scenario: Compare growth across army sizes
- **WHEN** the supported fixed-window scale samples run with identical measurement semantics
- **THEN** their evidence reports operation categories and timing alongside actual deployed/queued populations and active-unit-ticks
- **AND** a large nominal queue cannot be reported as an equally large simultaneous combat population

### Requirement: Large fixed-window battle scalability acceptance

Verification SHALL provide an independently selectable engine-free large-battle test and repeatable profile command. The large fixture SHALL contain one battlefield with at least 2,048 ordinary mixed actors, 1,024 per faction, on a supported 256-cell board with legal capacity, anchors and protected entries. The fixture SHALL use ordinary unit size, level-one profiles and action/death timings, deterministic identities/seed and adequate test-owned upkeep. Synthetic setup SHALL be identified separately from ordinary-command economy/balance evidence and SHALL NOT change default gameplay or create privileged process/UI commands.

The fixture SHALL execute 600 ordinary fixed ticks with no scripted casualties or movement during the window. It SHALL establish at least 256 simultaneously deployed actors, including at least 128 from each faction at the same observation, nonempty queues, landed attacks, casualties and subsequent admission of initially queued actors. Verification SHALL check capacity, anchor, faction, transit and identity conservation, queue progress and bounded event history at deterministic checkpoints. Initial/checkpoint/final reference evidence SHALL preserve ordering and seed/configuration identities across optimization. Setup, correctness hashing and assertions SHALL be measured separately from stepping. Matching profiles SHALL also support 128- and 512-actor samples with the same board, proportions, seed and 600-tick horizon. A smaller sample that terminates naturally earlier SHALL report its actual ticks and terminal point without no-op padding; the 2,048-actor case SHALL exercise the full window.

Large-battle acceptance SHALL execute before and after optimization; it SHALL remain explicitly selectable outside routine cheap-test repetitions and full graphical scenarios. Normal cheap coverage SHALL retain small fixture/counter regressions, and existing ordinary completion and campaign coverage SHALL remain mandatory. The large fixture SHALL have a 600-second wall bound per execution and bounded aggregate repetitions, with nonzero failure/cancellation and awaited cleanup. A timeout, idle setup, reduced actor count or shortened window SHALL NOT count as a successful large-fight comparison. Evidence SHALL explicitly identify a fixed-window result, not claim complete battle resolution, default-board capacity, ordinary balance or native rendering performance.

#### Scenario: Run the large fight
- **WHEN** the large-battle selection executes with its default seed
- **THEN** a 2,048-actor mixed fight advances 600 normal ticks with the required simultaneous opposing deployment, queued reinforcement, impacts and casualties
- **AND** checkpoint invariants and normalized pre-change reference results pass without launching Godot, Steam or a display

#### Scenario: Large army fails to engage
- **WHEN** the fixture allocates the requested actors but does not meet deployment, impact, casualty or queued-admission requirements
- **THEN** scalability acceptance fails even if identities remain conserved and the command finishes within its wall bound

#### Scenario: Large run times out or is interrupted
- **WHEN** a large test/profile exceeds its bound or receives cancellation
- **THEN** it returns nonzero, retains attributable partial evidence and disposes every owned simulation
- **AND** the before/after report identifies the missing result rather than substituting a smaller passing case

### Requirement: Isolated snapshot and transport performance evidence

Verification SHALL provide a checked-in measurement command for ordinary and large snapshot fixtures with recorded identical input digests and bounded warm-up/repetitions. It SHALL distinguish state projection, serialization/deserialization, compression/decompression and payload size/allocation measurements. Codec-only samples SHALL reuse already captured inputs so projection work is not misattributed. Measurements SHALL use the current wire schema and codec without requiring an engine process, private credentials or a live network. Existing round-trip, payload-bound and snapshot-mutation coverage SHALL remain required. Reports SHALL distinguish retained bytes from physical network throughput and SHALL NOT infer lower transport latency from compression ratio alone.

#### Scenario: Compare the same snapshot payload
- **WHEN** baseline and optimized snapshot/transport profiles run
- **THEN** each input's identity, unit/event workload, projection cost, codec costs and sizes are reported separately with matching wire values
- **AND** removing internal projections cannot be misreported as a change in codec speed

### Requirement: Reproducible presentation efficiency acceptance

Verification SHALL provide explicitly selected before/after presentation measurements using an identical bounded ordinary-combat snapshot/event stream and a fixed frame-delta/focus script through the normal presentation path. Each measured repetition SHALL cover 600 scripted frames after separately reported warm-up, including movement, attacks, death expiry, pause/resume and a return to a previously hidden city. The replay SHALL retain source/input identities, frame settings, renderer, CPU/allocation measurements, managed frame-update distributions and work counts. Source presentation SHALL use its existing locked build configuration on both sides and identify any build distinction from engine-free runtime profiles without adding or upgrading dependency locks. Setup and correctness captures SHALL be outside timed frames. Private-display execution SHALL use existing owned process/display/storage lifecycle and budgets; missing prerequisites SHALL be unexecuted/nonzero without desktop fallback. This profile SHALL NOT become a new default graphical scenario or a graphical repetition of the synthetic large fight.

Regression acceptance SHALL establish at most one full final pose evaluation per visible actor per frame, no unnecessary skeletal evaluation for hidden cities, and current correct poses/effects when focus returns. Hidden actors SHALL retain required logical state/event consumption and expire normally; historical impacts, sounds and strikes SHALL NOT replay on focus or reconnect. Cached playback, animation, route and HUD data SHALL remain correct across pause, snapshots, match changes, deaths, selection, research and resource/participation changes. Cheap tests SHALL own pure playback/invalidation checks; actual rig/focus behavior SHALL extend existing selectable combat/reconnect coverage with fresh observations, actual input and retained frames. Software-rendered measurements SHALL NOT be represented as native compositor/GPU performance.

#### Scenario: Focus returns after offscreen combat
- **WHEN** combat advances in a hidden city and the user focuses that city again
- **THEN** current motion/attack/death poses and health/participation values appear immediately, expired bodies remain removed and no historical effects replay
- **AND** work evidence shows hidden skeletal evaluation was avoided and visible final poses are sampled at most once per frame

#### Scenario: Repeated snapshots do not rebuild unchanged frame inputs
- **WHEN** multiple frames sample the same accepted snapshot pair
- **THEN** time-dependent poses remain correct without rebuilding the unchanged snapshot indexes or scanning event history independently for every view
- **AND** subsequent current-state changes still refresh the affected playback and displayed values

#### Scenario: Compare client costs
- **WHEN** the same presentation stream and frame/focus script run before and after optimization
- **THEN** the report shows independently measured client CPU, allocation and frame-update changes with workload-equivalence evidence
- **AND** campaign or CI wall-time reductions are not substituted for presentation measurements
