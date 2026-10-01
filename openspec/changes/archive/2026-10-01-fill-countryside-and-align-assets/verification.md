# Countryside verification

## Before baseline

Full `mise run ci` passed in 246.02s on 2026-10-01, before this implementation changed source. Evidence: `logs/20261001-185736-043325d8/ci-summary.json`. Declared tools were .NET 10.0.401 and Godot 4.7.2 .NET, owned Linux X11 with Mesa software rendering and Dummy audio. Locked restore, formatting, build/import, 140 core and 108 runner tests, six network scenarios, five source UI slices, sequential exports and both package smokes passed. Graphical package scenario took 52.80s.

The completed camera implementation exposes travel (6,8) and retains the orthographic overview fit. Existing uncommitted changes, camera artifacts and the separate combat proposal are preserved. No tool installation, dependency/asset update, publishing or desktop automation is part of this work.

## Asset inspection

Bundled grass geometry spans authored Y=-1..0 and uses its surface as origin. River spans the same slab interval, with decorative lowering supplied by the layout. Hill/tree models are overlays beginning around Y=0 rather than full hex slabs. Model footprint contacts use the lowest transformed mesh vertices, so windmill fan and catapult-arm child transforms cannot skew the ground center. The tower base central deck is authored Y=1.4, below battlement tips at Y=1.5; support attachment uses the deck instead of a fixed lift.

## Iteration and final results

Cheap tests passed: 140 core and 111 runner cases, including three new independent acceptance-check fixtures. The first economy attempt (`logs/20261001-190732-ef07504e`) exposed excessive individual terrain draw cost: coverage/contact assertions passed, but a short held camera check hit a travel boundary before release. Owned peers/display cleaned up. Terrain now uses cached multimeshes, grouped into small spatial regions for culling, and installed transforms are cached after changes so observations stay cheap. This retains full view coverage rather than hiding exposed edges behind a cap.

The next economy run passed in 58.38s scenario / 67.85s total (`logs/20261001-191020-28826cb7`), before the final spatial batching refinement. All nine plot selections, raised resource contacts, centered building footprints, central-deck Catapult upgrade, read-only foreign roofs, resize/reset and camera limit coverage passed. Source camera/resized/stacked frames were inspected: terrain reaches all edges, river joins continue and the bridge crosses the river. Example installed counts: 2016 tiles at the first navigation capture, 2244 after resize and 2444 after observed-city travel, all from imported mesh instances. The overview base size remains 43.32361 in these captures.

Spatially batched launcher passed in 74.94s scenario / 83.75s total, `logs/20261001-191405-4da2dfca`. Both supported sizes passed actual menu/solo placement, scale, core terrain orientation and bridge parity, plus no-session/no-connection and return-after-construction checks. Menu and solo frames were inspected. Shared scene instances remain passive; existing native-close, music, settings, invitation fixtures and owned cleanup assertions passed.

The first full after-CI (`logs/20261001-191603-47df1543`) passed restore/format/build/import, all cheap/network gates, economy and reconnect, then stopped at Settings in 156.15s. Owned camera observations show a valid D pan from Z=-6.9113 reaching Z=-8: the previous harness compared its displacement to unbounded travel. The assertion now independently computes normalized screen-relative displacement from observed yaw and elapsed movement, then clamps each axis to the declared travel limits. Interior speed checks remain; two cheap fixtures verify single-axis clipping, diagonal normalization and equivalent bindings. No camera policy or limits changed.

Corrected Settings passed in 25.01s scenario / 33.39s total (`logs/20261001-192156-ae795a24`), preserving native focus, modal/dropdown priority, interruption, owned preferences and lifecycle checks. Cheap tests passed at 140 core / 113 runner cases.

A second full after-CI (`logs/20261001-192330-cd059f54`) passed locked preparation, 140 core/113 runner tests, all six network scenarios, economy (58.65s), reconnect (20.91s) and settings (23.88s). The aggregate source UI deadline expired at 180s during launcher; owned cleanup completed. Filled terrain raises software-rendering cost across existing probes even with cached 8-by-8 multimeshes. The final full run uses the existing `--timeout-ms 300000` option for the whole serial graphical suite, retaining every scenario and assertion. This is a measured timeout requirement, not a default-budget pass.

The five-minute full run (`logs/20261001-193224-c9200412`) passed all six network scenarios plus economy (58.65s), reconnect (21.09s), settings (23.88s) and launcher (75.55s), then exposed Combat's oldest-corpse selection race. Observations show the selected older corpse disappeared normally while newer death poses froze correctly on pause. The existing Combat driver now requires a fresh death pose, selects the youngest casualty and uses that fresh observation's actual Pause coordinates without an intervening frame probe. Normal protocol acknowledgement and all death freeze/cleanup assertions remain. Cheap checks still pass 140/113.

Corrected Combat passed in 33.19s scenario / 41.58s selected-check total (`logs/20261001-193916-3f260e88`), including the same casualty's paused pose, shared clock freeze, bounded cleanup and fresh-session reset.

Full final `mise run ci --timeout-ms 300000` **passed** in 395.00s, evidence `logs/20261001-194033-eb0b391f/ci-summary.json`: locked solution restore, required formatting, build/import, native offline probes, 140 core / 113 runner tests, all six network cases (43.66s), all five source UI slices (216.32s including display), sequential Linux client/server exports, headless package smoke (1.09s) and graphical exported-package smoke (100.48s scenario / 103.77s including display). No checks were filtered or skipped within the required Linux CI set. The default 180-second aggregate source UI budget did not pass; the explicit five-minute override is required on this measured machine. Selected slices still fit their ordinary deadline. No runner default, tool lock, asset provenance or numerical rule changed.

| Existing slice | Before (seconds) | Final (seconds) |
| --- | ---: | ---: |
| Economy | 29.57 | 59.21 |
| Reconnect | 12.64 | 20.91 |
| Settings | 12.55 | 24.53 |
| Launcher | 37.86 | 75.29 |
| Combat | 17.38 | 33.07 |
| Exported package | 52.80 | 100.48 |

These are measured single runs under llvmpipe/two Mesa threads, not native GPU benchmarks. Added rendering affects unchanged probes as well as the small added assertions; no new graphical scenario, extra battle or process/display setup was added. Source UI increased by 103.01s and packed graphical scenario by 47.68s. Full CI increased by 148.98s.

Final inspected PNGs include source `economy/camera-resized.png` and `economy/economy-upgraded-roof.png`, plus packed `exported-package/menu-1100x820.png`, `menu-1280x720.png`, `solo-1100x820.png` and `packed-camera-navigation.png`. They show continuous river joins/bridge, filled corners, centered/supporting structures and readable controls. Installed observations report 18 shared static placements and 83 matching central terrain transforms. Packed menu at 1100×820 uses 952 terrain instances/26 batches; initial gameplay navigation uses 2016/43, and the widened observed-city envelope reaches 2444/57 (X=-69..70.5, Z=-67.55..64.95). The unchanged gameplay overview base size is 43.32361. Contact checks cover ordinary terrain Y=0/.5/1 and the declared tower deck; all nine plot identities remain selectable.

Final review confirms scene construction stays in `src/Game`, `Main` still guards it for headless/dedicated roles, and authority rules remain unchanged. Unrelated existing workspace changes and archived camera/UI work are preserved. Locked solution restore and `dotnet format Odot.slnx --no-restore` completed; strict change validation passes. Exports remain ignored local outputs, with no upload/publication.

Native compositor/GPU performance, physical input, listening quality, Windows execution and paired Steam accounts were not executed by this local Linux verification; silent audio and software-rendered evidence do not establish those behaviors.
