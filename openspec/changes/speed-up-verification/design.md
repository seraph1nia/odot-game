# Design

## Context

See `proposal.md` for motivation. The implementation already separates numerical simulation in `Game.Core`, Godot session/presentation in `Game`, and process supervision in `DevRunner`. `Match.Step()` and `AuthoritySession.Step()` expose fixed ticks; `AuthoritySession.Request()` accepts serialized ordinary commands and explicit time. `VillageStrategyTests` contains 21 twenty-wave combinations in one xUnit v2 class. Process authority progression calls one step per Godot physics callback. Snapshots are published after three callbacks, encoded for ENet and separately serialized for supervised stdout.

`CiSource()` prepares once, overlaps rules/tooling/network, then runs source UI. Network scheduling uses two workers, but source UI schedules five slices serially under one display. `MenuUiScenario()` repeats menu/solo and friends routes at both supported sizes and a hosted route. The exported case calls combat setup and then this entire menu scenario. Each fresh UI probe waits two process frames and a completed draw. `Child` auto-flushes every line, stores snapshots in a 512-event history and an unbounded channel, and duplicates protocol output already present in engine logs.

Existing evidence: `logs/20261002-175915-9de304ce/ci-summary.json` records 779.99s, with C# rules 68.08s, network 166.75s, source UI 391.69s and exported UI 187.89s. Source slices were economy 118.68s, reconnect 55.75s, settings 25.35s, launcher 125.50s and combat 63.09s. That run retains approximately 4 GB, 2.7 GB in the campaign network case. The later packed extension passed in 193.17s supervised at `logs/20261002-184754-a7ba964c`; its source identities differ in two driver files. These are historical observations, not new benchmarks or promises.

The existing specs require a real-process twenty-wave outcome, serial source UI and early gameplay ordering. The delta explicitly changes these while preserving complete C# strategy coverage and required source-before-export gates. Separate GitHub source/Linux/Windows jobs already overlap in separate workspaces; this design does not make them share generated output or change their existing independent scheduling. Preserve all existing working-tree changes, including completed compact UI work and the 300-second package timeout.

## Goals / Non-Goals

**Goals:**
- Give every retained assertion a named owner and remove repeated expensive execution only after its replacement passes.
- Make elapsed time depend primarily on meaningful boundary checks, not simulated seconds or repeated setup clicks.
- Keep serial diagnosis, stable selectors, isolated ownership, bounded cleanup and trustworthy failure evidence.
- Aim for 180–300s warm local full verification and substantially smaller evidence. Record cold hosted timings separately; the target is not a hard pass/fail deadline.

**Non-Goals:**
- Change game balance, fixed tick rules, interactive speed, dependencies, wire payload structure or Steam acceptance.
- Replace native input/rendering checks with snapshot assertions, make test hooks grant state, or introduce a general mocking/automation framework.
- Add exhaustive visual matrices, another campaign sample, new external test tools or publication steps.

## Decisions

### 1. Establish assertion ownership before pruning

Add a concise checked-in coverage table to `docs/verification.md`, organized by behavior and existing scenario/helper. For each moved assertion, name the C# test and retained boundary witness. Keep numerical/exhaustive behavior in `Game.Core` tests, transport/lifetime in real ENet cases, and input/focus/projection/assets in graphical cases. An assertion that couples an actual click to authoritative state remains graphical; arithmetic behind it is not a reason to repeat eight equivalent clicks.

| Existing work | New primary owner | Retained expensive witness |
| --- | --- | --- |
| Complete campaign, bosses, final rewards | Existing 21 strategy cases plus one serialized authority campaign | Early-wave authoritative progression and recovery |
| Economic retries, six resources, land/replacement, upkeep restoration | Serialized authority flows | Actual ENet retry/reconnect plus representative control families |
| Simultaneous elimination and protected redistribution | Existing C# invariants and authority flow | Three-player real transport transfer/next-wave allocation |
| Detailed HUD/inspector numbers and labels | Existing pure presentation tests, small extracted mappings only where justified | Rendered bindings, clipping, picking and lifecycle |
| Menu/solo/friends/host transitions | C# state policy where already separable | One source route per unique native focus/lifecycle behavior |
| Packed resources and bindings | Asset inventory/provenance checks plus short packed run | Real exported launch, ordinary purchase, current animation and PNG |

Alternative: simply remove slow scenarios or make them optional. Rejected because speed must preserve meaningful acceptance. Reusing giant fixtures across unrelated slices would also impair isolation and selectability.

### 2. Use a small ordinary-command C# flow driver

Extend `tests/Game.Core.Tests` with a shared test helper for local/remote accepted commands, sequence numbers, explicit test clock, serialized request delivery, snapshot round-trips and bounded `StepUntil`. Keep expectations in the tests rather than encoding expected game rules in the driver. Route remote actions through `AuthoritySession.Request`; bind/disconnect/re-admit peers through the production session APIs. Use the existing `CampaignStrategy` and normal catalogs/resources for one complete authority campaign; retain all 21 existing Match-based strategy samples rather than duplicating them at the session layer.

Cover a disconnect across upkeep, frozen/resumed state, accepted economic retries, stale replacement requests, boss/level/size wire identity, redistribution and final reward ordering. Explicit test time advances independently of simulation steps so rate-limit assertions do not rely on sleeps or weaken production limits. Setup uses ordinary accepted actions for integrated flows; narrow existing numerical fixtures remain legitimate in their own unit tests.

Alternative: move flows into headless Godot tests written in C#. They would still pay process startup, transport and real-time pacing costs, and cannot substitute for engine-independent acceptance.

### 3. Shorten process/UI paths around their unique boundaries

Keep all six network selectors. `authority-resume-victory` becomes the dedicated-authority/real-peer recovery and early-clear witness, with its description explicitly pointing to complete C# victory coverage. Retain invalid credentials, sender ownership, economic retry and rate-burst checks; do not replay twenty waves or boss acquisition just to repeat flow logic. Keep redistribution, defeat, failure cases, solo and host lifecycle as independent selectable cases. Require terminal snapshot serialization in C# and actual transport closure/feedback in retained process cases.

Keep source `economy`, `reconnect`, `settings`, `launcher`, `combat`. Drive supporting purchases/trades/recruits via ordinary protocol requests when their input route is already represented. Each distinct retained control family still gets an actual-input witness, and an independent observer confirms the authoritative effect. Reuse fresh observations after a rendered state change instead of probing immediately again; keep fresh ids and explicit post-draw barriers for layout/capture. Do not speed up by lowering rendering or substituting direct callbacks for real controls.

Launcher performs each native modal/focus/session boundary once; resize the same prepared client for size-specific geometry rather than repeating whole routes. Settings retains its intentional preference restart. Combat retains actual melee/ranged input, rigs, locomotion/impact/death, freeze and cleanup witnesses; supporting multi-wave material acquisition is ordinary accelerated setup. Reconnect retains the actual recovery control and current-state reconstruction.

Replace the exported branch's call to the full `MenuUiScenario()` with a small owned packed route: menu assets and solo entry, one ordinary purchase, required rigs/clip bindings, one live representative animation, supported-size layout captures and owned exit. Inventory/binding assertions cover asset breadth without replaying every animation route. Retain all source visual proof and the headless exported solo/host checks. Preserve the recorded exported native-dropdown limitation: native owned window resize can establish packed geometry; source Settings continues to exercise actual resolution controls. Installed-package verification reuses the focused packed route for archive/resource independence without silently changing release/Steam acceptance.

Alternative: retain identical source/package scenarios and only run them concurrently. This spends CPU on repeated flow evidence and keeps maintenance tied to long scripts.

### 4. Accelerate fixed steps with an owned scheduler

Introduce runner `--simulation-speed N` for applicable verification commands, integer 1–8, default 4 for headless flow/setup and 1 for actual graphical witnesses. Explicit 1 supports normal-speed diagnosis. Keep the controller in engine/presentation code; numerical `Step()` and catalogs are unchanged. Each physics callback runs at most N consecutive ordinary steps, with commands processed before the batch and a bounded yield to the engine between batches. Never jump `Tick`, scale damage/cooldowns, or catch up after pause. A small C# scheduling policy can be tested without Godot for exact step count, pause and cancellation boundaries.

Restrict pacing configuration and setup-to-witness switches to the supervisor's owned launch marker/token and local stdin; reject these controls on unowned launches and through gameplay RPCs. Reuse scoped runtime marker/credential ownership rather than trusting `--supervised` alone. The setup driver waits for a pacing acknowledgment with revision/tick, issues normal pause where a stable barrier is needed and resumes at speed 1 before animation/input measurements. Headless and graphical-host setup use the same ordinary numerical steps. Ordinary interactive `dev`, `play`, server and client launches remain speed 1.

Keep ENet snapshot rate bounded in wall time rather than emitting per accelerated tick batch. Publish commands, pause, phase/terminal transitions and explicit checkpoint barriers promptly; retain bounded current combat event history and ensure a required witness is observed before accelerating past it. Avoid altering the production wire format. The existing renderer follows `Match.StepsPerSecond` and clamps presentation delta, so actual animation witnesses must run at 1 rather than trying to stretch the render clock to match setup acceleration. Rate limits, connect deadlines and cleanup remain on their existing wall clock.

Alternative: global engine time scale or changing tick constants. One-step-per-callback authority code does not use delta as a step multiplier; those changes can alter presentation without establishing equivalent simulation progression. An unbounded fast-forward loop would starve transport and cancellation.

### 5. Bound C# and process concurrency separately

For C# campaign coverage, split the large strategy class by stable strategy families with a shared helper and disjoint case identity. Use at most two campaign process partitions against already compiled output; partition filters must account for every existing combination exactly once, and the general rules partition must exclude those same campaign cases. Keep the remaining unit/session checks in the required cheap command. This avoids depending on parallel creation/destruction of Arch's global world registry for additional campaign throughput. Report actual partitions/case counts; preserve test discovery and `test-in-process` compatibility, with serial execution acceptable for that fallback. Do not parallelize steps inside a match.

For expensive scenarios, extend scheduling with one shared CI admission limit, default two. Standalone network retains default two and positive `--jobs`; standalone UI accepts `--jobs 1..2`, default two, with one worker for a selected slice. CI exposes `--jobs` for the total expensive-scenario budget and `--ui-jobs 1..2` as its graphical cap. C# process partitions have their own maximum-two CPU budget rather than silently inheriting the process-scenario count.

Refactor `PrivateDisplay` so scheduling occurs in the parent and each admitted graphical scenario launches its own Xvfb/Xauthority/Openbox worker. The worker executes only that selected scenario. Give renderer files, summaries, child logs and runtime tokens worker/scenario-specific paths; merge reports in the supervisor. Keep one ordered driver per child. Release admission only after peer/display cleanup completes. Do not share displays to gain concurrency: native focus and input tests rely on window ownership.

After preparation, `CiSource()` overlaps required C#, network and source graphical work under these limits. Use linked cancellation and await every task's cleanup on first failure; a raw `Task.WhenAll` alone is insufficient for failure cancellation. Local `VerificationGate` still prevents exports after any source failure. Exports remain sequential. Preserve the existing GitHub jobs' distinct output workspaces and overall required status; no attempt to share imports or add artifact uploads.

Alternative: unlimited workers or one shared graphical display. Software rendering already uses two Mesa threads per client, and extra processes can slow all checks. Sharing focus makes failures timing dependent. Partitioned C# campaigns trade a little host startup for controlled isolation and auditable coverage.

### 6. Separate assertion delivery from retained diagnostics

Keep authoritative snapshots available to live assertions; reduce diagnostic retention without sampling away protocol checks. Replace the accumulating full-snapshot notification queue with bounded recent-state storage and wake notifications for the single ordered driver. Preserve command results, errors, lifecycle and requested checkpoint events as distinct reliable evidence; overflow must fail visibly rather than silently drop them. Existing exact-revision witnesses must have an explicit barrier or sufficient bounded history, not rely on an unbounded backlog.

Routine snapshot evidence uses compact revision/tick/phase summaries and checkpoint snapshots. Retain at most 16 recent full snapshots per child, also capped at 16 MiB, evicting oldest states deterministically; flush the available ring on assertion failure, timeout or cancellation. Keep a recent-state snapshot on early exit and report any retention truncation. Bound verbose engine diagnostic files with a documented rotation/byte limit while preserving parsed error results and checkpoint context. Buffer ordinary writes and flush periodically and during awaited cleanup; required errors/results remain attributable on abrupt child failure.

Route protocol diagnostics so routine full snapshots do not land in both engine and supervisor transcripts. Keep explicit engine error/exit logs and sanitized command-result/checkpoint evidence. Apply credential redaction before either retention path. Detailed full traces remain an explicit opt-in diagnostic mode with its larger costs reported. Add evidence byte counts, simulation ticks, pacing and worker/admission limits to timing summaries.

Alternative: compress the same duplicated transcript after the run. This reduces disk retention but retains serialization, stdout parsing, flushing and allocation costs during execution. Removing snapshots before assertions would instead weaken acceptance.

## Risks / Trade-offs

- [Moved assertions leave a gap] -> Require a reviewed coverage map and passing C# substitutes before deletion; retain actual UI and real transport boundaries.
- [Acceleration skips a transient witness] -> Stop at explicit checkpoint/normal-speed barriers; preserve transition/event delivery and test ordinary/accelerated state equivalence.
- [Concurrent rendering saturates CPU] -> Default at most two admitted expensive scenarios; report actual overlap and keep serial overrides. Measure improvement before claiming the target.
- [Campaign partition filters miss or duplicate rows] -> Test disjoint/complete discovery and preserve the 21-case sample; do not reduce seeds/player counts.
- [Evidence reduction hides the failing tick] -> Bounded full-state ring, command/phase/tick summaries and attributable failure dump; overflow/truncation is explicit.
- [Startup and cold imports dominate hosted results] -> Keep cold and warm measurements separate and preserve explicit startup/timeout overrides.
- [Existing gameplay/protocol tests relied on incidental snapshot backlog] -> Replace that reliance with bounded revision/checkpoint observation before changing delivery storage.

## Migration Plan

1. Capture the assertion-owner table and input identities from existing records. During planning run no tests. At implementation start reuse a successful before baseline only if its source/environment inputs match; otherwise follow the repository's substantial-task baseline guidance.
2. Add and verify C# replacements, campaign partitions and compact evidence before pruning expensive paths. Keep source behavior and wire payloads unchanged.
3. Add pacing with speed-1 equivalence checks, then shorten selected network/UI/package paths. Validate only affected slices during incremental work.
4. Add per-display graphical concurrency and shared admission, verifying serial/parallel coverage and cleanup. Update commands/docs/spec guidance coherently.
5. Run the required final full gate after implementation. Record actual timing/evidence volume against comparable historical inputs; use necessary selected serial checks for concurrency concerns, not repeated full suites purely for benchmarking. Report unmet targets candidly.

Rollback restores the previous drivers/scheduling/pacing/evidence mode together while retaining C# regression tests. No saved-game, dependency, protocol or deployment migration is involved; no publication occurs.

## Open Questions

Implementation resolves the initial tuning questions with default setup speed 4,
normal-speed graphical barriers, a 16-snapshot/16 MiB recent-state ring and an
8 MiB routine transcript cap per stream. The full required local gate passed in
252.82s with about 109 MB retained evidence and actual admission maxima of two
expensive/two graphical scenarios. There is no acceptance reason to increase the
default to 8; hosted cold-run tuning remains separate from this warm result.
The startup XML/decompression error observed in one rejected attempt remains an
intermittent native limitation, documented with the passing unchanged rerun and
strict error gate in `docs/verification.md`.
