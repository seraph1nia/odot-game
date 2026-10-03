# Simulation and calculation scaling review

The review separates authority work, public snapshot/codec work, client presentation
and verification overhead. A faster test suite does not establish a faster game.
Source findings below describe the instrumented, unoptimized baseline; the selected
remedies are implemented. [Measured results](#measured-results-2026-10-03) report
runtime and verification speed separately, with [verification guidance](verification.md)
covering commands, regression ownership and retained evidence.

Let `N` be all stored units, `L` living units, `D` deployed targetable units, `C`
cities (at most four), `B` board cells, `Q` queued units, `A` actors making a new
decision, `E` retained events and `V` visible deployed views. Default boards have
21 cells; supported synthetic scale coverage uses 256. Lifecycle matters: a large
queue and hundreds of moving/attacking actors impose different costs.

| Interaction / calculation | Unoptimized scaling and behavior | Pain point / selected remedy |
| --- | --- | --- |
| Unit-world collection | Collect ECS components, sort by stable id: `O(N log N)` per call | Repeated stage calls allocate and sort identical membership. Share stage-owned values and indexes; refresh after mutations. ECS remains authority. |
| Cleared cities, redistribution, remaining enemies | Each public enemy query first projects every living unit; repeated per city and tick | Use narrow living identity/destination queries. Include queued/reserve enemies, exclude dying bodies; observe cleared cities before transfers. |
| Engagement deadlines | Rebuild enemy DTOs to obtain distinct city ids | Reuse narrow authoritative membership. Preserve health-progress and stall precedence exactly. |
| Cleanup / arrivals / recovery | Several full collections despite unchanged ids | Share a cleanup/advance view; update immutable component values and refresh after removals. Retain simultaneous arrival ordering. |
| Attack impact validity | Builds id map but rescans all deployed units for city exposure for every attack | Precompute exposure by city once; consult only for city-target attacks. Keep all same-tick damage accumulated before deaths. |
| Single-target splash | Scans all deployed actors and builds candidate list even for cap one | Validate arguments, return primary immediately; no candidate work is needed. |
| Multi-target splash | Global filtering plus repeated full candidate sorting, up to victim cap | Use city/faction group; select exact best distance/initiative ties in canonical id order. Preserve primary-first victims and seeded generation. |
| Defender / tower impacts | Linear search of all deployed actors for each defense target | Share impact-stage id lookup and local skeleton groups. Existing nine-slot bound makes tower count small. |
| Defender / tower target selection | Full candidate sorting even though only the best group matters | One pass finds best group, then sort its exact identity ties. Keep seeded selection for a single tie too. |
| Waiting actor opponents / range | Each actor filters global `D`, then tests local opponents | Build ordered city/faction groups once per action stage. Range still uses exact integer hex distance. |
| Battlefield observations | Every deciding actor copies local actors/reservations after scanning global arrays | One immutable local payload per city/stage, separate actor sequence. Health-only and unrelated-city changes retain decisions. |
| Reachability | BFS `O(B + board edges)` per necessary actor search; converts fresh maps to frozen dictionaries | Ordinary privately owned read-only dictionary storage avoids construction cost. Retain all shortest predecessors and exclusions. |
| Objective approach scoring | Repeated goal/static-distance scans per opponent: approximately `O(A × local opponents × B)` | Reuse equal target-cell/kind scoring inside one search and static goal/screen inputs. Do not persist occupancy-dependent routes across mutations. |
| Route / goal / anchor arbitration | Sort canonical ties and commit movement in initiative/rank/id order | Tie sorting is necessary; preserve it. No approximate path or different random draw. |
| Protected entry admission | Sort `Q`, inspect bands/free anchors; repeated when queues cannot enter | Reuse stage views and fixed band data. Event-driven scheduling is deferred because reservation/death/transfer invalidation is more complex. |
| Elimination / transfer | Walk affected members, cancel actions, release reservations, preserve wounds/recovery | Necessary work. Narrow membership reduces unrelated projections; never change surviving-city remainder distribution. |
| Profile resolution / research | Repeat decimal growth, boss multipliers, fixed-point conversion and rank arithmetic for identical inputs | Cache only successfully validated immutable type/rank/boss/level combinations within configuration. Unsupported/overflowing values still reject. |
| Recruitment / construction / market / plots | Small catalogs and nine slots; public arrays sometimes rebuilt for one lookup | Direct private catalog lookup; retain exact six-resource payment, rounding, instance generation and retry rules. No parallel or approximate economy. |
| Food forecast / wave setup | Sort soldiers by level then id and compute integer upkeep | Reuse soldier projection within one snapshot/stage. Scoped reuse avoids stale food/membership/phase caches. Preserve unfed priority. |
| Complete snapshot | Per city projects full living world twice, then enemy and dying passes; copies board/campaign/catalogs | One canonical authoritative view; project matching units once, reuse city soldiers for payload/forecast; private immutable templates, detached returned arrays. |
| JSON / Brotli / Base64 | Linear in payload size plus compression cost; repeated wire snapshots contain definitions/history | Measure fixed-input stages separately. Protocol/compression changes are deferred; snapshot construction savings must not be credited to codec timing. |
| Playback acceptance / sampling | Rebuild current/previous indexes, ids and sorted sampled output every frame | Build owned ordered indexes on accepted inputs; sample time-dependent transitions each frame. Preserve event gaps, baseline reset and death deadlines. |
| Pending death retention | Each view searches event history: worst `O(view count × E)` | Maintain a bounded lookup of queued death events while accepting/draining. Retain exact event cursor semantics. |
| Visible rig pose | Full animation-tree evaluation twice per deployed unit/frame; repeated clip/bone/name lookup | Apply ordered events before one final visible sample; cache binding data. Preserve event source positions, aim, attack markers and death poses. |
| Hidden city views | Same skeletal work as visible city | Keep logical state, effects/cursors and cleanup, skip full hidden evaluation; sample current pose on return without replaying effects. |
| Route presentation (measured revision) | Cached points, but recomputes segment distances for every position/facing evaluation | The measured optimization cached exact segment/cumulative lengths. That route cache has since been retired; see the [current presentation contract](gameplay.md#deterministic-hex-combat). |
| HUD | Any revision refreshes all five sections, including quotes/buttons on every combat update | Invalidate by displayed inputs: resources/army/food, phase, roster/connection, selection/quotes and controls/status. Same-tick command changes must refresh immediately. |
| Campaign strategy harness | Rebuilds snapshot for policy and again for command/assertions; projected level-five search every tick | Reuse one before snapshot per decision; narrow equivalent participation observation. Attribute to preparation/assertions, not simulation. |
| Campaign diagnostics | Formats detailed successful transactions/army/receipts; investment assertions inside interpolation | Assert independently; compact complete successes, bounded recent failure records, explicit detailed trace. Preserve all 21 strategy rows and serialized campaign. |

## Precision and building blocks

Authoritative integer/fixed-point health, six resource balances, decimal level
growth and seeded target/route choices remain exact. Replacing them with rounded
floats, approximate target distances or probabilistic observation hashes risks
different outcomes and cooperative divergence. Their arithmetic is not the first
scaling bottleneck; repeated collections, DTOs and candidate enumeration are.
Presentation already interpolates float geometry and clip time. Cache identical
geometry and binding values rather than introducing further approximation.

No new library is needed for the selected fixes. `Dictionary`, `HashSet`, arrays,
read-only wrappers and stage-owned groups fit the existing single-threaded
authority. Keep long-lived frozen board/catalog maps; use ordinary maps for
short-lived BFS. Microsoft's [FrozenDictionary guidance](https://learn.microsoft.com/en-us/dotnet/api/system.collections.frozen.frozendictionary-2?view=net-10.0)
identifies its higher construction cost and intended infrequent-construction,
frequent-lookup use. [Generic collection guidance](https://learn.microsoft.com/en-us/dotnet/standard/collections/commonly-used-collection-types)
supports choosing collections for access patterns; concurrent collections would
not remove repeated work in this serial simulation.

Rig evaluation is already manual (`AnimationTree.CallbackModeProcess.Manual`),
so sampling once and skipping hidden rigs can remove actual engine work. Godot's
[AnimationMixer API](https://docs.godotengine.org/en/stable/classes/class_animationmixer.html)
documents explicit `advance` for manual processing. Imported assets, renderer and
locked packages remain unchanged.

ECS replacement, SIMD, pooling public DTO arrays, reverse reservation indexes,
event-driven admission, persistent route caches, parallel simulation, dense-id
assumptions and transport redesign are follow-ups requiring measured justification
and stronger ownership/mutation proofs. Existing Arch 2.1.0 and locked .NET/Godot
are sufficient for this change.

## Measurement boundaries

The frozen baseline is retained in
`logs/core-simulation-comparison/unoptimized-source.tar.gz` with archive hash,
Git base and patch. Command wall times include restore/build/runner preparation;
profile worker times separate setup, stepping, assertions, preparation and
diagnostics. Counter-disabled timings and separate operation-count runs use the
same seeds, frames and fixtures. Counts are semantic visits/builds/searches, not
CPU instructions or a combined efficiency score.

Mandatory scale acceptance is 2,048 actors over 600 normal ticks, with opposing
deployment, real hits/casualties and admissions from the original queue. The
128/512 cases use the same board/composition/window and report natural termination.
Presentation uses 600 scripted frames on an owned software-rendered display;
managed update distributions and whole-process CPU have different scopes.
Software rendering cannot establish native GPU/compositor performance.

<!-- measured-results:start -->

## Measured results, 2026-10-03

All 43 implementation tasks are delivered. The frozen ordered references, every campaign command/wave digest, all scale checkpoints/events, snapshot payloads and presentation input script match. Counter modes preserve those outcomes. Final xUnit coverage is 402 gameplay plus 144 runner tests, including all 21 strategy rows and the serialized twenty-wave campaign; the in-process fallback and full CI passed.

The 2,048-actor stress window improved most: median stepping 119.56→7.70 seconds (93.56% less), with cumulative managed allocation 68.14→6.82 GB (89.98% less). Solo/four-player campaign stepping fell 30.69%/26.05%. Managed presentation updates fell 50.51%. Cheap verification fell 47.21%, while full CI was practically unchanged (1.20% less in one trial). Large snapshot projection regressed; whole software-rendered process CPU barely changed. These are separate outcomes.

Runtime tables use counter-disabled, isolated serial workers, one separately retained warm-up and three measured repetitions. Parent command restore/build/setup times are excluded from worker tables. Each worker includes its own JIT; warm-up warms filesystem/runtime caches rather than persisting a process. Cells show median (minimum–maximum), absolute after-minus-before delta and percentage delta. Managed allocation is cumulative across threads, not peak heap/RSS or native memory. CPU includes process user/system work. Phase values are monotonic wall time, with boundary/clock costs included.

### Simulation runtime

**Combat stepping, seconds**

| Workload (n=3) | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- |
| Solo campaign | 1.97 (1.84–2.04) | 1.36 (1.29–1.38) | -0.60 | -30.69% |
| Four-player campaign | 3.02 (2.89–3.03) | 2.24 (2.23–2.27) | -0.79 | -26.05% |
| Scale 128 | 1.46 (1.43–1.55) | 0.70 (0.69–0.70) | -0.76 | -52.20% |
| Scale 512 | 53.43 (52.69–54.14) | 5.00 (4.98–5.00) | -48.43 | -90.65% |
| Scale 2048 | 119.56 (117.26–123.18) | 7.70 (7.45–7.96) | -111.86 | -93.56% |

**Whole workload elapsed, seconds**

| Workload (n=3) | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- |
| Solo campaign | 2.43 (2.30–2.52) | 1.77 (1.70–1.79) | -0.66 | -27.25% |
| Four-player campaign | 3.93 (3.83–3.94) | 2.79 (2.79–2.84) | -1.14 | -29.09% |
| Scale 128 | 2.11 (2.03–2.16) | 1.34 (1.31–1.34) | -0.76 | -36.30% |
| Scale 512 | 54.17 (53.44–54.87) | 5.82 (5.79–5.86) | -48.35 | -89.26% |
| Scale 2048 | 120.92 (118.63–124.54) | 8.85 (8.58–9.10) | -112.07 | -92.68% |

**Whole process CPU, seconds**

| Workload (n=3) | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- |
| Solo campaign | 2.94 (2.78–2.94) | 1.98 (1.89–2.02) | -0.96 | -32.50% |
| Four-player campaign | 5.07 (5.04–5.18) | 3.66 (3.65–3.67) | -1.40 | -27.69% |
| Scale 128 | 2.39 (2.36–2.45) | 1.35 (1.31–1.35) | -1.04 | -43.42% |
| Scale 512 | 55.22 (54.58–55.94) | 6.89 (6.86–6.94) | -48.32 | -87.52% |
| Scale 2048 | 121.83 (119.63–125.43) | 9.92 (9.66–10.14) | -111.91 | -91.86% |

**Whole process managed allocation, decimal MB**

| Workload (n=3) | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- |
| Solo campaign | 954.60 (953.25–954.79) | 420.01 (419.77–425.90) | -534.59 | -56.00% |
| Four-player campaign | 5,338.02 (5,319.01–5,339.32) | 1,483.72 (1,473.46–1,483.95) | -3,854.30 | -72.20% |
| Scale 128 | 434.10 (433.25–434.14) | 374.08 (374.08–374.10) | -60.02 | -13.83% |
| Scale 512 | 10,156.14 (10,154.20–10,156.20) | 4,642.19 (4,638.59–4,661.28) | -5,513.95 | -54.29% |
| Scale 2048 | 68,135.09 (68,094.44–68,137.17) | 6,823.94 (6,822.50–6,851.29) | -61,311.15 | -89.98% |

Campaign normalization uses the exact accepted command/wave outcomes and actual combat ticks. Whole-process CPU/allocation include preparation, assertions, setup and evidence; stepping uses its separate phase.

| Campaign | Combat ticks | Step ms/tick before→after | Process CPU ms/tick before→after | Managed B/tick before→after |
| --- | --- | --- | --- | --- |
| Solo | 8077 | 0.2435→0.1688 | 0.3640→0.2457 | 118,187.1→52,000.8 |
| Four-player | 8857 | 0.3413→0.2524 | 0.5721→0.4137 | 602,689.6→167,519.8 |

The scale cases all executed 600 ticks and remained in combat. Population evidence is identical across repetitions and modes. Initial/final reservations, transit/faction/anchor capacity, ids, deaths, queued progress and bounded event history are asserted by the checked-in fixture, independently reconstructing reservation values.

| Actors | Peak deployed | Mean deployed | Mean/peak queued | Mean/peak dying | Active-unit-ticks | Step ms/tick before→after | Step µs/active-unit-tick before→after |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 128 | 128 | 61.77 | 0.00/0 | 9.19/25 | 37059 | 2.438→1.165 | 39.47→18.87 |
| 512 | 485 | 346.61 | 17.46/128 | 25.82/63 | 207967 | 89.047→8.329 | 256.91→24.03 |
| 2048 | 665 | 596.04 | 1372.99/1664 | 17.98/43 | 357626 | 199.265→12.834 | 334.31→21.53 |

Reserve population stayed zero. The large case initially deployed 192 per faction with 1,664 queued; peak deployment was 665 and mean 596.04. It recorded 1,226 landed impacts, 238 deaths and 478 admissions from the initial queue; final deployment/queue/dying populations were 624/1,186/42. The separate correctness command passed the same fixed window (one run): stepping 125.34→7.76s, CPU 127.42→9.95s, allocation 68.12→6.81GB. It establishes scalability over this window, not full battle resolution or ordinary economy/balance.

Whole process CPU/allocation can also be normalized by the recorded actual ticks; for 2,048 actors they fell 203.042→16.528 CPU ms/tick and 113.558→11.373 managed MB/tick. Those figures include setup/assertion/hash/evidence costs; the separate stepping row excludes them. The synthetic board has 256 cells, whereas ordinary boards have 21. Nominal actor count must not be mistaken for simultaneous deployed population.

### Snapshot projection and unchanged codec

Each phase performs 25 operations per measured worker; three workers per input. Codec phases reuse the same already captured payload, separately from projection. JSON/compressed byte counts, wire values, schema, enum values, compression settings and bounds are identical. Table elapsed values are totals for 25 operations.

| Input | Phase | Before ms | After ms | Δ ms | Δ % |
| --- | --- | --- | --- | --- | --- |
| ordinary | projection | 2.33 (2.29–2.37) | 1.21 (1.17–1.25) | -1.12 | -47.98% |
| ordinary | json-serialize | 8.92 (8.70–8.98) | 8.74 (8.66–8.75) | -0.18 | -2.03% |
| ordinary | json-deserialize | 48.85 (48.34–48.89) | 48.70 (48.56–49.58) | -0.15 | -0.30% |
| ordinary | brotli-compress-base64 | 3.77 (3.72–3.78) | 3.67 (3.64–3.70) | -0.09 | -2.52% |
| ordinary | brotli-decompress-base64 | 5.16 (5.02–5.25) | 5.67 (5.64–5.72) | +0.50 | +9.78% |
| ordinary | codec-encode | 12.09 (11.91–13.87) | 11.94 (11.90–11.97) | -0.14 | -1.20% |
| ordinary | codec-decode | 48.57 (47.90–48.93) | 48.43 (48.36–48.66) | -0.14 | -0.29% |
| large | projection | 33.47 (31.65–34.29) | 38.82 (31.78–40.45) | +5.35 | +15.99% |
| large | json-serialize | 326.23 (319.69–338.90) | 297.14 (288.80–301.38) | -29.09 | -8.92% |
| large | json-deserialize | 821.52 (757.09–830.21) | 752.19 (743.03–769.55) | -69.33 | -8.44% |
| large | brotli-compress-base64 | 24.05 (23.94–24.23) | 24.01 (23.87–24.10) | -0.04 | -0.16% |
| large | brotli-decompress-base64 | 51.00 (50.83–52.20) | 51.68 (49.00–54.49) | +0.68 | +1.34% |
| large | codec-encode | 199.30 (192.19–207.80) | 392.93 (352.62–393.64) | +193.63 | +97.15% |
| large | codec-decode | 404.52 (399.94–412.64) | 396.67 (388.86–402.06) | -7.85 | -1.94% |

| Input | Metric | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- | --- |
| ordinary | projection CPU ms | 2.32 (2.28–2.36) | 1.21 (1.17–1.25) | -1.11 | -47.95% |
| ordinary | projection managed MB | 1.09 (1.09–1.09) | 0.61 (0.61–0.70) | -0.48 | -44.39% |
| ordinary | unchanged payload bytes | 48,770 JSON | 5,061 compressed | 0 | 0% |
| large | projection CPU ms | 33.37 (31.55–33.95) | 44.95 (38.51–61.84) | +11.58 | +34.71% |
| large | projection managed MB | 76.05 (76.05–76.05) | 27.86 (27.86–27.86) | -48.19 | -63.37% |
| large | unchanged payload bytes | 2,283,388 JSON | 69,185 compressed | 0 | 0% |

Ordinary projection improved. Large projection elapsed increased 15.99% in this final set despite substantially lower allocation. Detached retained event decision/victim arrays now have explicit ownership; their copying remains required, but no isolated trace establishes the cause of this regression. The earlier single intermediate sample was faster, so it is not used to hide this final regression. CPU, ranges and unchanged fixed-input codec costs above delimit the result; removing internal DTOs does not establish a codec speedup. JSON/Brotli phases remain substantial costs for large payloads, and transport redesign is deferred.

### Client presentation

The identical ordinary-command input and 600-frame delta/focus script use locked Debug Godot, owned X11/Mesa llvmpipe LLVM 23.1.1, Dummy audio, native max-fps 30 and scripted delta 1/60. Setup, timer calibration and PNG capture are outside the timed frame window. Process CPU/elapsed include engine/software rendering between updates; managed updates measure only the Tabletop update. Percentiles are nearest-rank order statistics from 600 frame-update samples per repetition, with the table showing the median/range of three such statistics. They are not percentiles estimated from three whole-run values.

| Metric (n=3) | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- |
| Whole frame-window elapsed s | 55.3756 (54.9781–55.4193) | 54.6004 (54.5567–54.7283) | -0.78 | -1.40% |
| Whole frame-window process CPU s | 105.5688 (104.9918–105.6187) | 104.6288 (104.5408–104.7044) | -0.94 | -0.89% |
| Managed allocated MB | 37.5611 (37.5497–37.5639) | 11.7542 (11.7437–11.7551) | -25.81 | -68.71% |
| Sum of managed frame updates ms | 1,020.0987 (981.4209–1,023.2562) | 504.7976 (501.7870–505.9567) | -515.30 | -50.51% |
| Managed frame update p50 ms | 1.5401 (1.5084–1.5490) | 0.7551 (0.7480–0.7571) | -0.79 | -50.97% |
| Managed frame update p95 ms | 2.7649 (2.7615–2.7884) | 1.1752 (1.1687–1.1876) | -1.59 | -57.50% |
| Managed frame update p99 ms | 3.9074 (3.8023–4.2264) | 1.9410 (1.9000–1.9666) | -1.97 | -50.33% |

Every frame in the counter-enabled replay asserted exactly one final full pose evaluation for each visible retained view, with zero evaluations of hidden rigs. Existing combat/reconnect/economy slices passed, including paused focus away/return with unchanged visible bones/time/ids and no historical cues, live imported attacks/contact, death expiry, current health and recovery. Managed work/allocation improved; whole software-rendered process CPU changed only −0.89%, with overlapping ranges. No native GPU/compositor, physical input or audible-quality result is inferred.

### Executed semantic work

Separate counter-enabled samples use schema 1 (one measured run per co-op/scale/presentation size; solo uses three). These count defined visits/builds/calls rather than all calculations or CPU instructions. Categories overlap and are never added into a score; unavailable categories remain null. Setup/assertion/evidence sites are included where they execute. The same input/state/event outcomes were verified with counters on and off.

| Workload | Category | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- | --- |
| Four-player campaign | WorldViews | 152,815 | 41,645 | -111,170 | -72.7481% |
| Four-player campaign | UnitVisits | 11,769,832 | 4,261,501 | -7,508,331 | -63.7930% |
| Four-player campaign | Sorts | 842,423 | 333,866 | -508,557 | -60.3684% |
| Four-player campaign | SortElements | 13,563,321 | 4,895,258 | -8,668,063 | -63.9081% |
| Four-player campaign | UnitProjections | 6,678,595 | 65,011 | -6,613,584 | -99.0266% |
| Four-player campaign | RouteElementsCopied | 8,099,586 | 60,941 | -8,038,645 | -99.2476% |
| Four-player campaign | VisitedElementsCopied | 1,677,837 | 54,063 | -1,623,774 | -96.7778% |
| Four-player campaign | ObservationBuilds | 333,483 | 30,587 | -302,896 | -90.8280% |
| Four-player campaign | ObservationActorVisits | 28,448,300 | 608,433 | -27,839,867 | -97.8613% |
| Four-player campaign | ObservationReservationVisits | 34,637,763 | 748,622 | -33,889,141 | -97.8387% |
| Four-player campaign | OpponentVisits | 31,573,643 | 1,482,909 | -30,090,734 | -95.3033% |
| Four-player campaign | RangeCandidates | 1,485,977 | 1,485,977 | +0 | +0.0000% |
| Four-player campaign | SplashCandidates | 291,661 | 2,651 | -289,010 | -99.0911% |
| Four-player campaign | BfsSearches | 41,468 | 41,468 | +0 | +0.0000% |
| Four-player campaign | BfsDequeues | 383,003 | 383,003 | +0 | +0.0000% |
| Four-player campaign | BfsEdges | 1,587,807 | 1,587,807 | +0 | +0.0000% |
| Four-player campaign | OccupancyChecks | 914,371 | 844,783 | -69,588 | -7.6105% |
| Four-player campaign | QueuedCandidates | 6,425 | 6,425 | +0 | +0.0000% |
| Four-player campaign | ProfileResolutions | 112,355 | 775 | -111,580 | -99.3102% |
| Four-player campaign | ProfileCacheMisses | 112,355 | 3 | -112,352 | -99.9973% |
| Four-player campaign | ProfileCacheHits | unavailable | 772 | N/A | N/A |
| Four-player campaign | MatchSnapshots | 1,315 | 828 | -487 | -37.0342% |
| 2,048 actors | WorldViews | 6,679 | 3,041 | -3,638 | -54.4692% |
| 2,048 actors | UnitVisits | 13,269,286 | 6,401,655 | -6,867,631 | -51.7558% |
| 2,048 actors | Sorts | 38,339,433 | 204,932 | -38,134,501 | -99.4655% |
| 2,048 actors | SortElements | 63,235,164 | 7,963,453 | -55,271,711 | -87.4066% |
| 2,048 actors | UnitProjections | 4,804,898 | 27,752 | -4,777,146 | -99.4224% |
| 2,048 actors | RouteElementsCopied | 818,612 | 23,169 | -795,443 | -97.1697% |
| 2,048 actors | VisitedElementsCopied | 105,100 | 133,845 | +28,745 | +27.3501% |
| 2,048 actors | ObservationBuilds | 262,043 | 600 | -261,443 | -99.7710% |
| 2,048 actors | ObservationActorVisits | 157,865,696 | 357,626 | -157,508,070 | -99.7735% |
| 2,048 actors | ObservationReservationVisits | 174,847,367 | 396,472 | -174,450,895 | -99.7732% |
| 2,048 actors | OpponentVisits | 158,684,124 | 79,340,495 | -79,343,629 | -50.0010% |
| 2,048 actors | RangeCandidates | 79,341,713 | 79,341,713 | +0 | +0.0000% |
| 2,048 actors | SplashCandidates | 780,326 | 0 | -780,326 | -100.0000% |
| 2,048 actors | BfsSearches | 51,486 | 51,486 | +0 | +0.0000% |
| 2,048 actors | BfsDequeues | 375,337 | 375,337 | +0 | +0.0000% |
| 2,048 actors | BfsEdges | 2,164,388 | 2,164,388 | +0 | +0.0000% |
| 2,048 actors | OccupancyChecks | 221,145,165 | 817,485 | -220,327,680 | -99.6303% |
| 2,048 actors | QueuedCandidates | 826,320 | 826,320 | +0 | +0.0000% |
| 2,048 actors | ProfileResolutions | 2,288 | 2,068 | -220 | -9.6154% |
| 2,048 actors | ProfileCacheMisses | 2,288 | 16 | -2,272 | -99.3007% |
| 2,048 actors | ProfileCacheHits | unavailable | 2,052 | N/A | N/A |
| 2,048 actors | MatchSnapshots | 12 | 12 | +0 | +0.0000% |

| Presentation category | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- |
| PlaybackIndexBuilds | 1,196 | 93 | -1,103 | -92.22% |
| PlaybackEventVisits | 500,255 | 6,210 | -494,045 | -98.76% |
| FullPoseSamples | 17,562 | 4,353 | -13,209 | -75.21% |
| HudSectionRefreshes | 465 | 26 | -439 | -94.41% |
| Sorts | 1,889 | 279 | -1,610 | -85.23% |
| SortElements | 25,456 | 3,381 | -22,075 | -86.72% |

The large case still examined 79,341,713 range candidates and performed 51,486 BFS searches: exact choices/search semantics remain intact. Admission still considered all 826,320 queued candidates; revision-scoped availability reuse removed repeated occupancy/sorting work without skipping queue order. These residual candidate/search and snapshot/codec costs are the strongest follow-up measurement targets. The selected fixes use existing BCL collections and exact scoped caches; no new package, approximate rule, probabilistic observation hash, persistent occupancy route cache or simulation parallelism was needed. Dense buffers, broader indexes, transport changes and native rendering investigation remain separate follow-ups with additional proof requirements.

**Instrumentation overhead.** Representative solo counter-on/off executions both have three measured repetitions. Raw observed differences are not corrections to runtime figures:

| Revision | Metric | Counters off | Counters on | Observed Δ | Observed Δ % |
| --- | --- | --- | --- | --- | --- |
| Before | elapsed ms | 2,429.82 (2,299.23–2,522.87) | 2,173.97 (2,130.67–2,520.92) | -255.84 | -10.53% |
| Before | CPU ms | 2,940.07 (2,780.40–2,942.89) | 2,518.59 (2,430.44–2,933.15) | -421.47 | -14.34% |
| Before | managed MB | 954.60 (953.25–954.79) | 960.83 (960.51–963.60) | +6.23 | +0.65% |
| After | elapsed ms | 1,767.69 (1,697.83–1,785.79) | 1,827.96 (1,775.96–1,890.01) | +60.28 | +3.41% |
| After | CPU ms | 1,984.44 (1,887.13–2,018.07) | 2,025.94 (1,974.52–2,122.01) | +41.50 | +2.09% |
| After | managed MB | 420.01 (419.77–425.90) | 432.83 (432.82–432.86) | +12.82 | +3.05% |

Negative/overlapping elapsed results are inconclusive about overhead; iterator/JIT paths and machine variation differ. No negative or guessed overhead is subtracted. Per-execution 100,000 core-boundary and 250 metric-read calibrations, plus 100,000 presentation timer pairs, are retained separately; they measure those API calls, not every instrumentation cost. Runtime performance comparisons use counters off. Optional dotnet-trace/dotnet-counters were unavailable and were not installed; no sampled stacks or GC-pause timings are claimed.

### Verification speed

The refreshed unoptimized test/CI baseline uses an isolated copy of the archived game with only the same corrected ordinary-speed pending-attack witness as the final runner. Untimed preparation warms build/import inputs. Original baselines and the first failed optimized CI remain retained, with the race and source audit documented in [verification guidance](verification.md#matched-network-witness-and-refreshed-verification-baselines). Locked tools, Debug configuration, two rule processes, total expensive budget two, graphical cap two, setup speed four and ordinary witness speed one are unchanged. The after inventory adds 19 cheap regressions and two focus clicks/probes in the existing combat slice.

| Task (one trial each) | Scope | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- | --- |
| test | Complete checked-in command s | 52.391 | 27.655 | -24.74 | -47.21% |
| test | Runner wall s | 49.267 | 24.528 | -24.74 | -50.21% |
| test | Retained evidence MB | 0.003 | 0.003 | -0.00 | -6.70% |
| ci | Complete checked-in command s | 243.333 | 240.414 | -2.92 | -1.20% |
| ci | Runner wall s | 241.953 | 239.033 | -2.92 | -1.21% |
| ci | Retained evidence MB | 113.419 | 103.390 | -10.03 | -8.84% |

| Task | Owned phase/suite | Before s | After s | Δ s | Δ % |
| --- | --- | --- | --- | --- | --- |
| test | phase/rules-solo | 17.581 | 9.390 | -8.19 | -46.59% |
| test | phase/rules-general | 23.588 | 15.092 | -8.50 | -36.02% |
| test | phase/tooling | 1.480 | 1.395 | -0.08 | -5.72% |
| test | phase/rules-cooperative | 49.261 | 15.671 | -33.59 | -68.19% |
| test | suite/rules | 49.264 | 24.524 | -24.74 | -50.22% |
| ci | phase/restore | 0.984 | 0.980 | -0.00 | -0.36% |
| ci | phase/format | 15.705 | 17.155 | +1.45 | +9.23% |
| ci | phase/build | 0.973 | 0.973 | -0.00 | -0.08% |
| ci | phase/import | 2.922 | 2.926 | +0.00 | +0.13% |
| ci | network/authority-resume-victory | 13.148 | 12.664 | -0.48 | -3.68% |
| ci | phase/rules-solo | 18.317 | 9.521 | -8.80 | -48.02% |
| ci | network/redistribution | 26.481 | 25.920 | -0.56 | -2.12% |
| ci | phase/rules-general | 25.629 | 15.925 | -9.70 | -37.86% |
| ci | phase/tooling | 1.680 | 1.463 | -0.22 | -12.89% |
| ci | phase/rules-cooperative | 52.202 | 16.592 | -35.61 | -68.22% |
| ci | suite/rules | 52.202 | 25.448 | -26.75 | -51.25% |
| ci | ui-worker/reconnect | 43.312 | 42.924 | -0.39 | -0.89% |
| ci | network/defeat | 2.773 | 2.781 | +0.01 | +0.28% |
| ci | network/failure-cases | 3.475 | 3.570 | +0.09 | +2.72% |
| ci | ui-worker/settings | 30.547 | 30.553 | +0.01 | +0.02% |
| ci | network/solo-session | 0.829 | 0.826 | -0.00 | -0.41% |
| ci | ui-worker/economy | 94.870 | 93.664 | -1.21 | -1.27% |
| ci | network/playing-host-lifecycle | 5.570 | 5.944 | +0.37 | +6.72% |
| ci | suite/network | 113.002 | 112.276 | -0.73 | -0.64% |
| ci | ui-worker/combat | 60.214 | 61.238 | +1.02 | +1.70% |
| ci | ui-worker/launcher | 81.157 | 81.242 | +0.09 | +0.11% |
| ci | suite/source-ui | 189.163 | 187.813 | -1.35 | -0.71% |
| ci | phase/export-client | 8.915 | 6.910 | -2.01 | -22.49% |
| ci | phase/export-server | 4.931 | 3.915 | -1.02 | -20.60% |
| ci | suite/export-smoke | 1.363 | 1.368 | +0.00 | +0.32% |
| ci | ui-worker/exported-package | 15.301 | 15.195 | -0.11 | -0.70% |
| ci | suite/exported-ui | 15.302 | 15.195 | -0.11 | -0.70% |

Evidence bytes are the runner-owned inventory at summary time, excluding parent command stdout and later writes; the tiny cheap-run byte delta is not treated as an efficiency gain. Gate phases overlap; summing them would misstate total CI time. Cheap task wall time fell 47.21% with the larger inventory. Full CI command wall fell only 1.20% in one trial, so overall CI is treated as practically unchanged; source UI/process/rendering remains its critical path. This comparison includes runtime and harness changes together, not an isolated attribution of every second to diagnostics. Required network, five source UI slices, sequential client/server exports, headless package smoke and graphical package smoke all passed; nothing was published.

Campaign harness attribution uses the same profiled command/wave outcomes. Diagnostic phase boundaries deliberately differ because investment assertions moved out of formatting; that phase is labelled diagnostic/assertion rather than a pure formatting microbenchmark.

| Campaign | Harness/evidence metric | Before | After | Δ | Δ % |
| --- | --- | --- | --- | --- | --- |
| Solo | setup | 109.95 (108.62–110.97) | 111.76 (111.69–112.46) | +1.82 | +1.65% |
| Solo | preparation | 102.14 (102.09–104.46) | 87.80 (87.67–88.26) | -14.34 | -14.04% |
| Solo | assertion | 72.10 (70.73–78.55) | 26.89 (26.70–27.03) | -45.21 | -62.71% |
| Solo | diagnostic-assertion | 16.22 (15.88–16.50) | 14.20 (14.16–14.40) | -2.02 | -12.48% |
| Solo | evidence | 158.50 (158.22–161.96) | 156.20 (156.16–156.42) | -2.30 | -1.45% |
| Solo | diagnostic Lines | 204 | 83 | -121.00 | -59.31% |
| Solo | diagnostic Characters | 156,202 | 97,140 | -59,062.00 | -37.81% |
| Four-player | setup | 108.26 (108.23–117.21) | 112.41 (111.83–112.66) | +4.15 | +3.83% |
| Four-player | preparation | 345.25 (342.76–352.44) | 200.18 (197.79–202.38) | -145.07 | -42.02% |
| Four-player | assertion | 258.50 (256.17–270.93) | 45.18 (44.92–45.82) | -213.32 | -82.52% |
| Four-player | diagnostic-assertion | 21.70 (21.50–22.02) | 18.07 (18.05–18.20) | -3.62 | -16.70% |
| Four-player | evidence | 175.02 (174.81–176.14) | 176.40 (175.13–177.16) | +1.39 | +0.79% |
| Four-player | diagnostic Lines | 808 | 326 | -482.00 | -59.65% |
| Four-player | diagnostic Characters | 622,811 | 388,062 | -234,749.00 | -37.69% |

Success summaries retain six-resource spending/income, production, recruitment/progression, food and reward/upkeep receipts, terminal stocks, wave ticks and casualties. Failure formatting expands only the latest 64 domain records; explicit ODOT_CAMPAIGN_TRACE=1 enables successful transactions. Investment validation runs at both verbosity levels. The final in-process fallback passed 402 gameplay plus 144 runner tests, with ordinary xUnit discovery and all strategy/serialized rows retained.

### Retained evidence and limitations

Source freezes, exact command metadata/logs, every raw sample/warm-up, source/machine/runtime/build identities, phase summaries and the independent before/after audit are retained under ignored logs/core-simulation-comparison/. The runtime source inventory stayed constant within each side; all required families succeeded. The unoptimized verification recheck source differs only in the matched network witness and excludes ignored developer sessions, using fresh owned credentials/state. Frozen reference constants were not regenerated.

| Family | Before evidence directory | After evidence directory |
| --- | --- | --- |
| campaign-cooperative-counters | `logs/20261003-000022-profile-campaign-8819a196` | `logs/20261003-013557-profile-campaign-951dc5b5` |
| campaign-cooperative | `logs/20261002-234054-profile-campaign-302d1d8d` | `logs/20261003-012948-profile-campaign-05cb86ba` |
| campaign-solo-counters | `logs/20261003-000011-profile-campaign-84d63f94` | `logs/20261003-013548-profile-campaign-55861506` |
| campaign-solo | `logs/20261002-234042-profile-campaign-c9da69b7` | `logs/20261003-012939-profile-campaign-f52f05a7` |
| ci | `logs/core-simulation-comparison/unoptimized-recheck/logs/20261003-011643-1c2c3561` | `logs/20261003-012537-756aaa9c` |
| large-correctness | `logs/20261002-234145-test-scale-ba7f8245` | `logs/20261003-013017-test-scale-81b386da` |
| presentation-counters | `logs/20261003-000649-16a5407b` | `logs/20261003-013648-fc0806e2` |
| presentation | `logs/20261002-235551-a55cf94c` | `logs/20261003-013133-023c5755` |
| scale-counters | `logs/20261003-000049-profile-scale-652cefcc` | `logs/20261003-013613-profile-scale-b7938bbc` |
| scale | `logs/20261002-234353-profile-scale-c0ee58bf` | `logs/20261003-013027-profile-scale-1e913f4e` |
| snapshots-counters | `logs/20261003-000031-profile-snapshots-6960f453` | `logs/20261003-013604-profile-snapshots-f70f8bf6` |
| snapshots | `logs/20261002-234112-profile-snapshots-ff93a5c7` | `logs/20261003-013001-profile-snapshots-81efd4a7` |
| test | `logs/core-simulation-comparison/unoptimized-recheck/logs/20261003-011553-368961d6` | `logs/20261003-012511-b9237525` |

The raw calculated comparison is [analysis.json](../logs/core-simulation-comparison/analysis.json); ordered equivalence/source/mode checks are [equivalence.json](../logs/core-simulation-comparison/equivalence.json). The optimized source archive hash and baseline audit are retained beside them. Results describe this locked toolchain and machine, with three runtime repetitions and one gate trial per side; they are not universal latency guarantees. Fixed-window scale and software-rendered presentation do not establish complete stress-battle balance or native GPU/audio performance.
