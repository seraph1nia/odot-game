# Authored-era performance loop

## Later fixed-position scenery cost, 2026-10-07

The [beautification verification record](../openspec/changes/beautify-settlement-world/verification.md#complete-matched-renderresource-measurement)
owns the later scenery measurements and their verification-cost accounting;
these are separate from the numerical optimization below. Its
[historical local acceptance section](../openspec/changes/beautify-settlement-world/verification.md#historical-local-acceptance-at-53779df)
owns earlier post-review source-bound receipts and retained failures; the
[accepted finishing scope](../openspec/changes/beautify-settlement-world/verification.md#accepted-finishing-scope-after-coderabbit-followup)
owns requirements after the later followup, without relabeling historical
measurements as final-head evidence.

## Scope and decision

Landed input: `f37830a` (authored migration). Original GLBs/maps, provenance,
notices, UI/audio, numerical references, saves and protocol remain unchanged.
Reviewed [migration](authored-migration.md), [rendering](runtime-rendering.md),
[fidelity](asset-fidelity.md), [ENet cadence](enet-guest-cadence.md) and earlier
[simulation measurements](simulation-performance.md) before experiments.

**Two independent rounds, one accepted:** replace routing goal-minimum LINQ
pipelines with exact loops and stop at their mathematically optimal lower bounds.
Positive reachable steps exclude zero, so their lower bound is **1**; static
movement distance includes zero, so its lower bound is **0**. Empty/unreachable
sets still produce `int.MaxValue`. Existing ownership, reachability caches,
canonical ties, seeded choices and complete authoritative outcomes are unchanged.
No approximate distance, persistent spatial index, new framework or dependency.

Headline: **2048 stored-actor stepping −22.4%, cumulative managed allocation
−42.8%**; ordinary solo/four-player stepping −9.4% /−12.7%. Twelve matched authored
PNG/observation pairs are exact. Native rendering remains the frame bottleneck;
no causal graphical FPS or native-GPU gain is claimed.

A stage-local cell/range target cache was tested first and **rejected**: its
construction/allocation cost outweighed saved range tests. It and its candidate
controls are removed from compiled source, retained only in ignored audit logs.
Previous rejected static flattening/pine instancing were not repeated. A flat
pre-rendered background cannot establish the existing depth/shadow/picking/camera
contract; no content, material, shadow, asset or camera tradeoff was adopted.

## Controlled inputs and measurement boundaries

- AMD Ryzen 5 5600X, 6 cores/12 logical CPUs; Linux x86_64, kernel 7.2.8-1-cachyos.
  Locked SDK **10.0.401**, runtime **10.0.12**, Godot **4.7.2 .NET ed1daf0bf**.
- Graphical: owned Xvfb/Xauthority/Openbox/X11, Mesa 26.2.4, llvmpipe LLVM 23.1.1,
  Compatibility/OpenGL3, `LP_NUM_THREADS=2`, Dummy audio, 1100×820, max-fps 30.
  **Software-rendered diagnostics are not native GPU/compositor qualification.**
- Numerical: Release, serial isolated workers, one separate warm-up and **three
  measured repetitions**, seed 1, unchanged 600-tick 128/512/2048-actor synthetic
  256-cell battles. Stored count is not deployed count. Ordinary solo/four-player
  frontline campaigns also use three repetitions and complete command/wave digests.
- Presentation: Debug, **one warm-up plus one measured 600-frame replay per side**,
  six views × overview/close × 50 frames; first 10 of each view/scale are transition
  frames, remaining 40 supply nearest-rank p50/p95/p99. Scripted delta 1/60 does not
  change authority speed. Actual frame intervals use a monotonic wall clock, not
  Godot's clamped engine delta. Managed update timing excludes diagnostic sampling
  and PNG persistence; whole-window time includes diagnostic sampling and intermediate
  capture barriers, but excludes the final PNG/observation capture.
- Pressure preset names 16/64/256 describe recipe scaling, **not actor counts**.
  Ordinary board/home limits are retained: settlements have 6/12/18 friendly rigs
  and 2/5/9 buildings; initial combat populations are 14/44/146 (friendly plus
  8/32/128 enemies, including queues). Per-frame `StoredUnits` counts living units
  in the latest snapshot, including queued/stored units but excluding dying bodies;
  `VisibleViews` counts visible retained rigs, including dying views. Neither is a
  deployed-living count; lifecycle/deployment details remain in the input snapshots.
  Ordinary commands build/buy/recruit; funding is diagnostic-owned.
- Every input script and source inventory is hashed. Authored script digest:
  `0D3C9F044497DD911B86FADA9932015ACB58CDF2E7D4C6CDA641D7395673470F`.
  Original ordinary replay serialization/digest is preserved by omitting new
  optional diagnostic fields when unused. All measured scale checkpoints, event
  hashes, initial inputs and entire campaign outcomes match exactly between sides.
- Managed allocation is cumulative process allocation, not peak heap/RSS. Native
  texture/buffer/static monitors have different scopes. The replay's cached first
  process-info RSS sample is not a per-scale or peak estimate and is excluded from
  memory comparisons; that misleading per-frame field is removed from the command.
  Render monitor values describe the preceding engine draw;
  they are not independently attributed physical-GPU time or per-asset draw costs.

Commands (selected diagnostics, not substitutes for ordinary UI/network acceptance):

```sh
mise run profile-presentation --scenario authored-scale --iterations 1
mise run profile-scale --scenario large-battle --sizes 128,512,2048 --iterations 3
mise run profile-campaign --strategy frontline --players 1 --iterations 3
mise run profile-campaign --strategy frontline --players 4 --iterations 3
mise run profile-snapshots --scenario ordinary-and-large --iterations 1
mise run test-native-pump
mise run test-scale --scenario large-battle --seed 1 --work-counters
```

The new authored replay reuses existing process/display ownership and bounded
600-frame scheduling. It catches script/native-render attribution and camera-scale
mistakes that cheap tests cannot measure, costs approximately 2.5 minutes per worker,
and does not expand default CI into a graphical population matrix. Twelve fresh
rendered views and current pose/camera/placement observations are retained per
worker; no earlier image stands in for a later frame.

## Protected-boundary and native investigation modes

The authored-scale command also supports two distinct owned diagnostic modes:

- Add `--baseline PATH` to compare every fresh capture with the matching retained
  measured capture directory containing `profile.json`, PNGs and observations.
  Add `--boundary-evidence REQUEST.json` only with that baseline to bind retained
  native/glyph witnesses to their finite frame/view/zoom identities. Evidence
  hashes and current source/buffer/camera/pose/physical-state guards must match;
  unavailable or stale inputs fail rather than generating a replacement before.
  Warmup and measured executions run the full script with the same manifest;
  request `GlyphCompletion`/`BoundaryControls` flags do not dispatch producers in
  this comparison mode. Fresh native attestation checks the retained glyph witness
  against current labels. Extra exact/native verification is recorded in
  `BoundaryValidation` and included in raw elapsed/
  cumulative allocations, except the final capture/proof outside those aggregates.
- Add `--pixel-ownership-request REQUEST.json` for one ordinary rendered-prefix
  investigation without warmup; it cannot be combined with either comparison
  option. `TargetFrame` defaults to 299 and permits only 299/399/499/599.
  `RemainingViews` permits full-script continuation only for 499/599 and is
  required for 599. The retained request defines authorized samples and optional
  native glyph/control work; this is not a general renderer classification mode.

`profile-identity.json` records `TargetFrame`, `RemainingViews` and
`RequestedScriptedFrames` for investigations. Execution is unconfirmed until the
owned worker succeeds and the prefix/full-script receipt matches the requested
count; only then is `ExecutedScriptedFrames` emitted. Even a 600-frame investigation
is not complete replay acceptance. These modes do not replace live gameplay/UI
or full CI, establish an exact coplanar tile winner, or qualify native GPU performance.

## Numerical before/after (accepted round)

Median stepping seconds, with measured min–max in parentheses:

| Workload | Before | After | Change |
|---|---:|---:|---:|
| Solo campaign | 1.415 (1.372–1.422) | 1.282 (1.264–1.287) | −9.40% |
| Four-player campaign | 2.301 (2.187–2.481) | 2.010 (1.998–2.124) | −12.65% |
| Scale128 | 0.734 (0.723–0.741) | 0.655 (0.651–0.681) | −10.74% |
| Scale512 | 5.868 (5.782–5.917) | 4.533 (4.304–4.617) | −22.75% |
| Scale2048 | 8.157 (8.152–8.229) | 6.331 (6.127–7.488) | −22.39% |

Whole-worker medians include setup, assertions/hash/evidence, not just stepping:

| Workload | Wall seconds before→after | CPU seconds before→after | Cumulative managed MB before→after |
|---|---:|---:|---:|
| Solo | 1.921→1.744 | 2.159→1.944 | 392.17→379.50 (−3.23%) |
| Four-player | 3.121→2.736 | 4.081→3.615 | 1469.50→1414.04 (−3.77%) |
| 128 | 1.434→1.362 | 1.446→1.389 | 396.09→373.45 (−5.72%) |
| 512 | 6.737→5.428 | 7.901→6.557 | 5126.84→3210.67 (−37.38%) |
| 2048 | 9.516→7.686 | 10.622→8.815 | 7304.00→4180.96 (−42.76%) |

The 2048 window remains 600 normal ticks with original queue/admission, impacts,
deaths, reservations and state/event digests. It still performs roughly 78.10 million
range tests and 55,093 BFS searches. The final directed `test-scale` exactly matches
all original input/checkpoint/event/outcome digests **and every work counter**.
Those coarse counters do not count removed goal-enumerator allocations or skipped
minimum reads. No simulation speed, wave, profile, target or tie changes improve
these numbers. Exact cheap
controls compare the new minima with full independent enumeration across graph
detours, blocked/unreachable goals, zero distances, both factions, city/unit goals,
all source/target cells and relevant ranges, including reused identities/initiative.
Frozen ordered references are unchanged.

## Frame, native rendering and resource costs

Matched single-run frame-wall p50/p95/p99 milliseconds and managed-update p50:

| View | Zoom | Visible rigs | Wall before | Wall after | Managed before→after |
|---|---:|---:|---:|---:|---:|
| Small settlement | 1 | 6 | 176.03 /188.58 /191.87 | 156.58 /160.65 /164.13 | .687→.618 |
| Small settlement | 3 | 6 | 126.24 /136.15 /138.39 | 112.34 /115.52 /117.57 | .622→.599 |
| Small combat | 1 | 5–6 | 175.15 /179.58 /185.66 | 155.95 /159.71 /163.51 | .685→.597 |
| Small combat | 3 | 5 | 122.50 /125.54 /139.20 | 110.26 /112.07 /112.84 | .630→.567 |
| Medium settlement | 1 | 12 | 215.54 /236.74 /333.74 | 194.96 /198.94 /269.80 | .960→.852 |
| Medium settlement | 3 | 12 | 170.06 /174.91 /175.52 | 153.78 /156.72 /157.37 | .970→.860 |
| Medium combat | 1 | 34–36 | 275.79 /293.48 /296.67 | 244.73 /249.94 /251.38 | 2.226→1.934 |
| Medium combat | 3 | 29–34 | 224.62 /236.82 /242.57 | 198.13 /204.25 /205.23 | 1.938→1.719 |
| Large settlement | 1 | 18 | 282.21 /292.89 /320.03 | 250.03 /258.24 /262.55 | 1.334→1.175 |
| Large settlement | 3 | 18 | 208.09 /217.86 /218.92 | 184.93 /189.55 /191.70 | 1.283→1.174 |
| Large combat | 1 | 44–46 | 341.73 /351.87 /361.80 | 303.08 /308.81 /315.06 | 2.932→2.519 |
| Large combat | 3 | 44–46 | 272.59 /283.94 /286.00 | 242.47 /248.44 /250.57 | 2.883→2.471 |

Before whole frame window 138.36s, process CPU 260.82s, cumulative managed 95.84MB;
replay input/tabletop setup 1.99s excludes earlier engine launch/menu/JIT work.
Managed settled update allocations range 7.95–86.38KB/frame. Large overview combat
spends roughly 1.51ms on units and 1.18ms on overlays; unchanged landscape coverage
is about .010–.012ms, camera about .019–.024ms and hover picking .068–.077ms. Caching
terrain footprints or replacing a few square roots would not address the measured
bottleneck. Hidden rig evaluation already has an existing skip seam.

Physics reports **zero active 3D objects and collision pairs**, with approximately
.04–.06ms engine physics cost in this replay. This is no evidence of expensive
static-asset collision processing. Picker math is presentation-only; no collision
bounds or hit rules changed. Native rendering dominates, with overview draws rising
from roughly 9k to 25k across these scenes. Texture residency is approximately 331MB;
representative buffers 48.5–58.9MB; nodes 10,770–19,946; native static memory 155–218MB.
Those are engine counters, not source bytes or additive resident-memory totals.

After frame window 122.86s, CPU 233.55s, cumulative managed 95.73MB, setup 1.59s.
All 600 stored/visible counts match; **all twelve freshly captured PNGs and complete
camera/pose/placement observations are byte-identical between sides**. Native and
managed times move together in this single-run comparison. The routing method is
not part of the timed scripted presentation update, so these lower software-frame
times cannot be credited to the numerical optimization. No causal graphical FPS or
native-GPU gain is established.

Fixed-input codec control (25 operations/phase, one measured worker after separate
warm-up): ordinary 58,205 JSON bytes /6,092 compressed; large 2,656,815 /72,560.
Below are phase wall milliseconds and cumulative managed MB, not 2,048-actor
simultaneously-deployed network acceptance or simulation stepping:

| Phase | Ordinary ms / MB | Large ms / MB |
|---|---:|---:|
| Projection | 2.20 /.969 | 41.95 /34.988 |
| JSON serialize | 10.80 /2.035 | 333.98 /75.874 |
| JSON deserialize | 56.58 /3.734 | 878.92 /93.470 |
| Brotli/base64 compress | 4.03 /1.023 | 25.88 /11.587 |
| Brotli/base64 decompress | 6.14 /3.441 | 60.31 /211.561 |
| Full encode | 14.42 /3.062 | 420.26 /87.456 |
| Full decode | 56.72 /7.173 | 457.65 /305.072 |

Unchanged native service's focused post-change control conserved 80/80 snapshots,
all ordered payload digests and 15 receipts under both ordinary/delayed polling.
Publication→RPC median was 13.93ms ordinary, 621.27ms with an intentionally delayed
physics poll, 13.90ms with the already-landed receive correction. Its unrelated
`Candidate` label names the historical native correction, **not this routing
change**. Maximum applied snapshot age was 0 /1 /.0667 simulation seconds. Retry,
stable player, native pause/resume, bounded paused services, frame deduplication and
cancellation drain passed. These headless controls isolate the transport boundary,
not real graphical acceptance, and establish no new network optimization.

## Rejected round and retained failures

Range-cache medians regressed stepping 128 +22.3%, 512 +12.8%, 2048 +1.9%; ordinary
solo/four-player stepping +1.3%/+9.6%. Large managed allocation was essentially
unchanged, while ordinary allocation increased. Exact outcomes passed, but the
practical cost gate did not. No range-cache branch, toggle or allocation ships.

Landed full before-CI (`logs/20261005-181348-c76606bc/`) failed after 107.15s at
reconnect's `pause retains a current casualty`. Focused-city body 6 lived 464→512;
the ordinary accepted pause was 518, with only foreign-city body 22 remaining.
This is retained historical-witness/current-receipt divergence, not an optimization
regression or proof of a new transport cause. No deadline/assertion was relaxed.

The first new diagnostic used a pre-tree viewport timer and failed before samples;
setup moved to Ready. Another diagnostic retained a 134.54s warmup but mislabeled
clamped engine delta as wall frame time. Its remaining workers were cancelled
through the owned runner and cleanup awaited; corrected wall-clock input produced
the accepted baseline above. These are diagnostic correctness fixes, not FPS wins.

An initial ordinary-replay reference check caught newly serialized default fields;
omitting unused optional fields restored the existing frozen digest without
regenerating references. One round 1 cheap run reported a transient Weaver provenance
hash failure. Working-tree/HEAD hashes and manifest agreed immediately afterward,
and the focused strict check and subsequent full cheap suite passed. Its cause is
**not established**; no asset bytes or provenance guard were changed.

## Final acceptance

Full local Linux CI passed **791.87s**, within its unchanged 900s bound:
`mise run ci --jobs 2 --ui-jobs 1`, evidence `logs/20261005-192806-a973f26d/`.
Strict authored-static fidelity, 502 gameplay +299 runner tests, all six network
cases, all five source UI slices, ordered client/server exports, headless package
smoke and private-display graphical package smoke passed. Reconnect's original
assertions/admission remain unchanged; its earlier failure is **not declared fixed**.
The graphical cap differs from the failed before run, but coverage does not. These
complete-versus-failed CI totals are **not a runtime or verification-speed gain**.
No deadline increase, personal state/display, assertion waiver, asset/provenance
rewrite or publish step. Native Windows forge CI is separate from this local gate.

## Evidence and remaining limits

- Instrumented baseline source archive: `logs/authored-performance/`, SHA256
  `a8de76e18c3692bb8a50a948f8aa3734a2e95eb7d7bebd117c9417e66d9f3e58`.
- Numerical before: scale `logs/20261005-183656-profile-scale-9d984456/`, solo
  `logs/20261005-190132-profile-campaign-e2a5dd1f/`, co-op
  `logs/20261005-190142-profile-campaign-8cc2b879/`.
- Rejected range round: `logs/20261005-190207-profile-scale-404db0ff/` and
  `logs/authored-performance/retired-range-cache/`.
- Accepted after: scale `logs/20261005-190936-profile-scale-53785476/`, solo
  `logs/20261005-191037-profile-campaign-afc04011/`, co-op
  `logs/20261005-191046-profile-campaign-e1767eee/`.
- Authored before: `logs/20261005-185145-b76a8e66/`; first failed/partial diagnostics
  `logs/20261005-184307-944781b3/` and `logs/20261005-184651-922ab617/` remain.
- Cheap accepted head: `logs/20261005-191100-c44eacd5/`, **502 gameplay +299 runner**.
- Authored after: `logs/20261005-191318-8e79a616/`; codec
  `logs/20261005-191751-profile-snapshots-002578ea/`; native service
  `logs/20261005-191801-fc1f719c/`.
- Directed counter/correctness window: `logs/20261005-194808-test-scale-d6ef772f/`,
  matched with `logs/20261005-184308-profile-scale-5e3910ad/`.
- Selected actual melee: `logs/20261005-191828-dba800cf/`; complete shared/near/far,
  windup/impact/simultaneous coverage with fresh frames, progression live 2.36s and
  evidence I/O 1.30s. The old combined 12s contract is not claimed; original separate
  live 12s /I/O 30s bounds are unchanged.

Stop after two rounds: native authored rendering remains the largest measured
frame bottleneck. Actual GPU/compositor qualification and any source-fragment/art
work require separate representative hardware/asset-project scope. Remaining range,
BFS and snapshot/codec costs are not hidden by the allocation win. Software frames
and silent state checks do not establish physical input, audible quality, native
platform performance or arbitrary-load stress-battle resolution. The recorded full
source/network/UI/export/package pass is not replaced by a filtered profile or
cheap pass, nor is it native Windows/GPU, Steam or audible-quality qualification.
