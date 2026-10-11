# Testing overhaul audit and coverage ledger (2026-10-11)

## Inputs and limits

Audited baseline `d7f1728`, equal to actual `origin/main` at task start (not the
older approximate inventory). Tracked source inventory: **75** C# files / 8,011
lines in Game.Core.Tests; **49** / 6,508 in DevRunner.Tests; shared
`tests/InProcessTests.cs`; **65** / 8,543 in DevRunner. Helpers/profile entrypoints
are included in file counts, not mistaken for test methods. Original discovery:
**504 core + 638 runner = 1,142 rows**, including two Windows-only skips on Linux.

The grouping below maps behavior and production seams, rather than ranking test
value by length. Every baseline test/helper and runner source is assigned to a
coherent group. The audit used test methods, fixtures, their production consumers,
scenario registrations and historical evidence. It does not assert that a small
C# test replaces native rendering. No forbidden graphical/live desktop/Vulkan
baseline was rerun. No toolchain/driver upgrade, release or branch-rule mutation.

## Core groups

Names below are relative to `tests/Game.Core.Tests`; `Tests.cs` suffix is omitted
where unambiguous. Costs are qualitative unless an actual receipt is named.

| Sources / helpers | Production behavior / meaningful defect | Dependencies, overlap and disposition |
| --- | --- | --- |
| Resource, EconomyCatalog, BuildingTransaction, SettlementCommand, CommandConstruction, World | Atomic six-resource payments/refunds, overflow, producer/recruit quotes, editing guards, stale generation/turn, ready/pause/retry | Small deterministic Match/Authority inputs. Keep A. Exact authored default prices are intentional product contract, not merely test counts. Replace graphical arithmetic coverage with these existing assertions, not copied implementations. |
| ArmyRoster, ArmyRecovery, ArmyProgression, ArmyCommand, ArmyPresentation, ArmyProgression's fixtures | First-fit homes, capacity, hall storage/healing, paid food, veteran identity/wounds, reconnect retry | Keep A: boundaries and serialized authority results detect actual resource grants, early healing, duplicate spend and ownership regressions. Not equivalent to a native roster button. |
| CityResearch, ResearchLifecycle, ResearchWitness, ResearchedRole | Earned points, exclusive paths, persistent effects/statuses and resolved future actions | Keep A including earned early-game witness; no new graphical campaign. Small deterministic effect inputs plus bounded ordinary setup. |
| CombatAction, CombatConfiguration, CombatDecisionRegression, CombatLifecycle, CombatLimit, CombatPolicy, Combat, CombatFixture, DefensePolicy, SplashTower, StatusEffect, UnitSize | Impact/movement deadlines, target locks, splash/status ordering, death reservation release, transfer/admission, stall limits, faction/size/overflow | Keep A: deterministic ECS actions/outcomes and negative paths. CrowdedBattle rows are slower (~3s each in first new source run) but enforce progress/separation/determinism; not deleted just for cost. Core in-process parallelism remains disabled for static Arch registries. |
| HexBattle, HexBoard, HexOccupancy, HexRouting, RoutingScoreEfficiency, DecisionEfficiency, SeededDecision | Routing/topology/occupancy, feasible targets, stable seeded ties, search work bounds | Keep A. Malformed/boundary cases and stable mixer vectors are valuable small tests. Independent algorithm oracle used only for well-defined bounded graph behavior. |
| AuthoritySession, AuthorityCombat, VillageAuthority, AuthoritativeQuery | Local/dedicated/host permissions, protocol/rate/malformed/credential failures, detached snapshots/current units | Keep A. Real transport not proven here; essential C cases remain. Serialization/insertion-order outcomes protect more than fixture construction. |
| Campaign, CampaignDiagnostics, CampaignDiagnosticsTests, CampaignFixture, CampaignAcceptance, CampaignTests.Strategies, SessionCampaign, SessionFlow, SessionFlowTests, VillageStrategy, EconomyArmyFixture, ArmyBalance | Rewards/boss/terminal/allocation transitions; ordinary paid strategies and historical causal openings | Direct Campaign transition/defeat/reward and early cooperative SessionFlow/VillageStrategy cases stay A. **21 strategy rows + 1 serialized campaign + 19 balance/calibration rows** move to explicit E (`Tier=Deep`). Full balance matrices no longer routine. Expensive causal opening fixtures kept where they protect capacity/retry/ordinary funding; no grants used to fake network readiness. |
| CombatPlayback, AssetPresentation, ProgressionPresentation, PresentationLimits, PresentationEfficiency, PacingEquivalence, SnapshotEfficiency, SnapshotPayload | Pause/transport baselines, effect deduplication, current displayed profiles, health fractions, deep copies, Brotli bounds, ordinary tick equivalence | Keep A. Strong replacements for graphical calculations, but no claim to prove rendered geometry or physical audio. |
| SteamInvitationPicker, SteamLobbyRules, SteamOperationState | Account/lobby/original-host/session namespace, stale async callbacks, consent generation and retry | Keep A; deterministic native-boundary logic. Real invitation/relay needs separate two-account qualification, not local GUI automation. |
| AssemblyInfo, Profiling/ProfileProgram, ProfileOptions, Measurements, OwnedProfileProcess, ProfileTests, WorkCounterTests, CampaignTrace, SnapshotProfile | Static lifetime isolation, explicit profile selection, cancellation/cleanup bounds, truthful metrics | Keep fast ownership/count correctness A. Profiling commands stay explicit diagnostics, not PR benchmarks. Shared in-process fallback still uses actual locked xUnit executor. |
| Profiling/ReferenceTraces, ReferenceTraceTests, CombatReplayFixtureTests, LargeBattle, LargeBattleFixtureTests | Ordered replay golden hashes; synthetic stress input construction | Replay/golden rows move E (2 rows). Delete LargeBattleFixtureTests (4 rows) which pinned synthetic dimensions/roster counts and mostly validated the fixture against itself. Actual `test-scale` still runs production combat for 600 ticks and asserts identity conservation, faction/capacity/unique reservations, reconstruction, event bounds, admission, attacks and deaths. HexBoard tests retain malformed graph validation. No stress correctness guarantee is claimed by ordinary A. |

## Runner and linked production-boundary groups

Relative to `tests/DevRunner.Tests`; listed names omit `Tests.cs`.

| Sources | Production behavior / realistic defect | Disposition / cost / coupling |
| --- | --- | --- |
| Harness, Admission, GraphicalShutdown, Diagnostic, CurrentObservation, GuestPollOwnership, VerificationPacing, GameLaunchPolicy | Bounded admission, sibling cancellation/awaited exit, unrelated process preservation, owned preferences/session/endpoint, redaction, newest-arrival state, argv role/pacing contracts | Keep A. These shell/process tests launch no Godot; they protect real ownership/cleanup, not just a scheduling constant. Consolidate old three-row economy-allowance table into one explicit deep-vs-smoke invariant and assert headless budget bounds rather than obsolete 900s policy. |
| SessionFile, NetworkOpening, UpkeepReceipt, ResearchReadiness | Credential namespace/atomic persistence; true surviving city eligibility, post-clear restoration, receipt/stage freshness | Keep A. Deterministic production Match and file behavior catches failures missed by stateless config tests. |
| BurnAdmission, CasualtyAdmission, MeleeVisualProof, MeleeObservationInterface, CombatProgressionProof, CombatRecoveryObservation, CombatVisualTiming, CameraPanExpectation | Quote-to-receipt aging, current vs historical state, one ordered waiter/driver, finite attempts, unowned pause refusal, motion/timing layers | Keep A negative/control assertions. Some fixture setup is heavier, but removal would weaken actual cancellation/ownership guarantees. They do not run real graphical processes. Further tiny-state fixture reduction is possible without changing acceptance invariants. |
| CombatPoseDiagnostic, CombatRecoveryTrace, OwnedFrameCapture, OwnedTimingTrace, PresentationOwnershipWitness, PresentationProfileOptions, ProtectedProjection, NativeBoundaryControlReceipt, RoofFamilyEvidence, LandscapeBoundaryProof | Bounded/fresh non-secret evidence, exact input/frame identity, cleanup/failure propagation, inability to borrow stale proof | Keep A. Extensive diagnostic code remains selected E machinery; tests protect truthful evidence and ownership. No new verifier/control plane is introduced. Retained evidence does not itself qualify pixels. |
| Graphics, AudioMixProgress, StaticRenderKey | Actual production preference normalization/effective fallback, original OS argv, clock wrap, cache invalidation | Keep A including small boundaries. New B exercises ConfigFile + real component signal paths; A does not establish a native popup or audible playback. |
| AssetInventory, AuthoredAsset, TrioAsset, DetailedGround, LandscapeChecks, VillageWalk | Immutable provenance, GLB parsing/truncation, rig/socket/material dependencies, route/plot/terrain eligibility | Keep A. Hashes establish asset provenance, not arbitrary config text. Consolidate repeated immutable GLB parse/hash into a single lazy read for the three authored-asset methods; preserve all mapping/rig/negative assertions. New B imports/plays real scenes and constructs actual buffers. |
| DesktopExport, LinuxPackaging, PackagingConfiguration, ReleaseAssetAssembly, ReleaseDiscovery, ReleaseIdentity, ReleaseVersion, SteamOverlayLaunch | SemVer/tag/own-AppID, checksum/payload/allowlist, trusted URL/update cancellation, actual isolated install/reinstall/upgrade/uninstall, legacy identity and overlay argv | Keep A: security/compatibility and executable installer tests deserve retention, even when assertions mention stable identities. Fake archive tests assert production installer effects but do not qualify real exported application/Windows installer. |
| WorkflowPolicy, RulePartition | Exact YAML substrings and synthetic rows built from the same partition list | Delete 2 rows / 77 lines. WorkflowPolicy forbade manual workflow selection and froze whitespace; it did not execute GitHub semantics. Pins, release-only publishing and existing job identities remain in actual workflow/review. RulePartition merely reconstructed its own 21-row list; standard xUnit `Tier=Deep` and actual TRX executed counts replace custom class-name partitions. No protocol/game behavior removed. |
| New PortLease | Separate-process exclusion of a port lease and reuse after disposal | Add one cheap Linux kernel-flock assertion against production PortAllocator; explicit native-platform skip elsewhere. Actual bind ownership/failure remains C. |

## Supervisor, engine and scenario audit

All runner sources are covered by these responsibilities (relative to
`tools/DevRunner`). Existing mechanisms are reused rather than replaced.

| Source group | Behavior / regression / cost decision |
| --- | --- |
| Program, Runner, Options, CheapTests, Scenarios, Child, ChildEvents, DiagnosticText | CLI selection, locked preflight/preparation, actual completed binaries, deadlines, ordered waits, cancellation, secret redaction and owned cleanup. Delete RulePartitions; keep scheduler/Child. Standard .NET filters + TRX zero-match failure. Exclusive handle leases replace process-local-only port candidates; OS bind remains final authority. |
| EditorImports, Exports, DesktopExport, SteamChecks, SteamOverlayLaunch, WindowsChecks, TaggedExports, ReleasePackaging, ReleaseStaging, ReleaseAssetAssembly, InstalledPackages | Source/import/export identity/runtime/Steam dependency and install lifecycle. Keep independent native Linux/Windows headless package protections; sequential output mutation. Remove automatic graphical package call only, retaining selectable D/E qualification. Tagged release/allowlist/checksum/upload safeguards unchanged. |
| NetworkTests, LocalSessionTests, ResearchNetworkTests, ResearchReadyBarrier, CampaignStrategy | All six real headless protocols retained. Default short failure/solo/playing-host cases protect actual transport, bound-host ownership, retry/resume/identity, channel progress and orderly/abrupt shutdown. Dedicated wave-eight research/resume, multi-wave redistribution and defeat remain explicit E/selected C; arithmetic and terminal reward already covered cheaply. No full twenty-wave process campaign. |
| AssetFidelity, AuthoredAssets, AssetCosts, LandscapeChecks | Provenance and actual static geometry buffer/placement corruption checks. B assets reuses production AssetFidelityProbe in minimal scene rather than a full menu; legacy `test-assets` still supported. No copied numerical tests relocated into engine. |
| PrivateDisplay, NativeWindowClose, DesktopVulkan, AtmosphereTests, PresentationProfiles, PackedUiTests, SteamTests | Owned display/native surface/process cleanup and selected package/render/Steam witnesses. Keep explicitly gated, no PR/main/deep workflow launches them. Missing prerequisites nonzero/unexecuted. Preserve native device/no-desktop-fallback constraints. |
| UiTests, MenuUiTests, EconomyUiTests, ArmyUiTests, CombatUiTests, ResearchUiTests, FriendsUiTests, GraphicsUiTests, CameraUiTests, InspectionUiTests, MeleeUiTests, CoordinatedMelee, CombatReplayFixture | Legacy giant graphical journeys mostly repeat core arithmetic and early battles. Retain explicit `deep` economy/launcher/packed routes; default launcher/economy/packed becomes one new UiSmoke application with real controls, one production, no battle/server/observer, one frame. Remove appended research + fresh-solo journey from full combat: research is independently selected; solo freshness already A/C and launcher. Graphical correctness NOT requalified here. |
| BurnAdmission, CasualtyAdmission, CameraPanExpectation, CombatFailureEvidence, CombatPoseDiagnostic, CombatProgressionProof, CombatRecoveryObservation, CombatRecoveryTrace, MeleeAdmission, MeleeVisualProof | Keep finite receipt/current-state guards and bounded failure evidence; existing cheap fault tests protect their interfaces. They service explicit E rather than PR. Do not remove ownership/negative assertions to make an expensive journey appear faster. |
| AtmosphereReturnDiagnostic, CameraTravelDiagnostic, MeleeAdmissionDiagnostic, MeleeTimingReport, PopupLifecycleDiagnostic, StartupObservationDiagnostic, VanillaPopupDiagnostic | Explicit diagnostic experiments only, not acceptance/default CI. No additional live diagnosis was started. Historical findings remain historical, no speculative runtime/host repairs. |
| New EngineTests and src/Game/Verification/EngineTests | Three selectable minimal headless cases against production ClientSettings, UnitAssets/AnimationPlayer/Skeleton and AssetFidelityProbe. Each owns user state/logs/marker and a 60s process bound; real signal/persistence and pose outcomes, no GameApplication/Main session. |

## Significant coverage changes

Counts in this ledger are the executed implementation snapshot `70ba7e3`, before
later review regression rows were added. They are not current-head discovery or
qualification; do not update receipt counts without executing their owner suites.

- Default A excludes **43** existing deep rows: 21 strategy, 1 serialized campaign,
  19 balance and 2 golden/reference replay rows. All remain executable with
  `test-campaign`; direct boss/victory/defeat/reward/transfer and early deterministic
  outcomes remain A. `Tier` traits are the sole tier selection source.
- Delete **6** rows: two runner policy/self-catalog facts, four synthetic fixture
  facts/theory rows. Consolidate a three-row timeout-policy test to one behavior
  fact (two fewer rows). Add one port exclusion row. All-C# total goes from
  **1,142 to 1,135** (500 core + 635 runner; Linux executes 633 runner and skips 2).
  Default A: **457 core + 633 runner pass**, with two native Windows skips.
- New B: **3 independently selected cases**, not counted as xUnit rows. Settings
  verifies default gain, actual slider signal/mute/gain, draft/Apply graphics,
  dialog lifecycle and new-component reload. Animation verifies imported equipment,
  skeleton pose advancement and loop policy for all 8 faction/role rigs. Assets
  exercises production geometry construction and corruption rejection.
- Default C reduces six journeys to three without deleting the other journeys.
  Full explicit headless deep still runs all six. PR/main no longer waits on
  routine wave-eight research, multiwave redistribution or graphical journeys.
- D minimal smoke code replaces default economy/launcher/packed acceptance only
  when graphically authorized; legacy journey bodies remain E. Headless-only
  verification does not claim these unexecuted controls/captures passed.
- Shared builds/import/exports remain serialized. Already-built core/runner
  processes overlap; network admission defaults to two. No global test selector
  registry, new framework, fleet manager, retries or desktop automation introduced.

## Woodland component consolidation

The migration's former `test-ui-resources` command, launcher, probe and SceneTree
entry were redundant with the landed EngineTests owner. Their meaningful 38
resource/modal/input/Settings-service assertions now execute in the existing
`settings` engine case, selected by ordinary source CI. All ten production scenes,
font/seal/branch/button states, supplied quote intent/disabled reason, real-catalog
footprints/prerequisites, outside-click blocking, Tab/Escape, later-menu sibling
picking, Audio input/service, draft/Apply/persistence and both window bounds remain.
The original settings default/mute/gain/reload assertions also remain.

Additional B regressions catch freed implicit and invalid explicit invokers on
reusable modals, Settings resize stealing the nested confirmation's order/focus/
input scope, and incomplete externally parented control cleanup before reload.
Pure C# cannot instantiate those Godot nodes or route viewport input; the minimal
fixture uses deterministic controls and synthetic process-local input, three-frame
layout waits, existing owned preferences/marker/logs and the same 60s bound. Added
cost is estimated at a few frame waits, not a measured full-game journey. The
historical 38-check receipt is not current-head proof. No graphical readability,
desktop focus, physical input, audio listening or transport claim is added.

Directional-scope coverage strengthens that same B fixture: native Down/Enter
from the confirmation footer and explicit outgoing Left/Right/Up/Down neighbors
must not focus or activate the suspended Settings Apply/Cancel controls, commit
preferences or discard the pending draft. Internal directional traversal, native
TabBar arrows and an embedded OptionButton popup retain their input behavior;
the existing real audio-slider keyboard check remains. The earlier Tab/Escape/
resize checks missed viewport directional fallback, and cheaper C# cannot route
Godot GUI events. This uses the same deterministic modal/Settings controls, owned
preferences and bounded frame waits; incremental cost is estimated, not measured
against a matched baseline. Embedded headless popup input is not native-window or
desktop focus qualification. No production payload bytes or gameplay rules change.

## Failure history

| Evidence | Initiator / symptom / disconfirming evidence | Action and remaining uncertainty |
| --- | --- | --- |
| Hosted [38009506421](https://github.com/seraph1nia/odot-game/actions/runs/38009506421), launcher 146.74s | `preferences and uninterrupted music survive return`; original assertion compared monotonically increasing loop playback positions. Missing combat artifact then produced **additional** upload error (`No files were found...`). Source product music lifetime not established broken; unchanged launcher also passed locally historically. | Main already corrected the bad position assumption in `fc65406`, retaining volume/instance/playing checks. Do not claim this overhaul made that fix. New B asserts actual persistence; new D uses instance/playback, not position monotonicity. Optional summary upload now warns if absent, fixing demonstrated masking CI failure. |
| Hosted [38037663441](https://github.com/seraph1nia/odot-game/actions/runs/38037663441), combat ~303s; [38039688672](https://github.com/seraph1nia/odot-game/actions/runs/38039688672), combat 304.73s/research-controls 30.04s | All pose flags were true before cancellation. History/source diff demonstrates hardcoded 300s inner combat deadline and a separate 30s research feature deadline exhausted while progressing. Generic exit130/cleanup are symptoms, not proof of a combat or host-memory defect. Current main already removed inner300 and uses controls60. | This overhaul does **not** blindly raise those timeouts or claim new reproduction. It separates research from combat entirely, preserves selected research bound and current receipt guards, and moves both off automatic CI. Remaining rendered baseline/reconnect races require an authorized selected reproduction, not assumed repaired. |
| Hosted [38011778155](https://github.com/seraph1nia/odot-game/actions/runs/38011778155), provider 25-minute aggregate source limit (recorded in main history) | Last active case progressing in combat after long earlier UI. This was aggregate scheduling/cost, not demonstrated engine hang. Later main hosted success 1132.17s shows strong hardware/timing dependence, not permanent failure. | Remove automatic UI/calibration journeys, not timeouts/parallelism safeguards. Default headless source measured under a minute locally; no portable CI-speedup claim until exact hosted after receipts. |
| Historical stale pause / corrupt imported sample / native llvmpipe faults in `verification-history.md` and prior commits | Several distinct boundaries, not one cause. A Btrfs read/checksum incident and a native renderer backtrace do not prove host RAM/runtime failure; cancellation/error masking can occur independently. | No host diagnosis or parked acquisition resumed. Keep current-state/receipt/cleanup negative tests, headless rendering-resource checks and retained failure evidence. Do not claim unexplained native faults repaired by changing test tiers. |

Cheap controlled discriminants were actual C# receipt/ownership assertions,
headless component persistence/animation and bounded transport, not forbidden
live graphical reruns. Repeat only changed relevant inputs or an unresolved
concern, never blind retry to green.

## Implementation selection receipts

[Verification measurements](verification.md#overhaul-measurements-2026-10-11)
owns the baseline, default/serial/source/deep/package timings, evidence paths and
comparison limits. Historical economy **767.20s**, launcher **380.86s**, combat
**270.27s** in hosted run `38120639473` support moving them off routine CI, not a
portable speedup claim. No graphical, native Windows, real Steam, actual
installer release or release-publishing qualification executed locally.

Targeted receipts at implementation `70ba7e3` (prepared .NET selectors use
checked-in runner commands):

| Command / selection | Actual outcome and count | Runner / task timing |
| --- | --- | --- |
| `test-core --filter 'FullyQualifiedName~HexRoutingTests.OccupiedDirectApproach'` | 1 passed | 1.30s |
| `test-core --filter 'FullyQualifiedName~HexRoutingTests'` | 4 passed | 1.31s |
| `test-core --filter 'FullyQualifiedName~Research'` | 40 passed | 3.32s |
| `test-runner --filter 'FullyQualifiedName~PortLeaseTests'` | 1 passed; actual different process cannot borrow/released handle becomes usable | 1.31s |
| `test-fast --filter 'FullyQualifiedName~HexRoutingTests.OccupiedDirectApproach'` | Passed 1 core / 0 runner; total matching selection correctly succeeds | 1.16s |
| `test-core --filter 'FullyQualifiedName~THIS_TEST_DOES_NOT_EXIST'` | **Expected negative: exit 1**, executed zero; cannot silently green | 1.08s |
| `test-runner --timeout-ms 1` | **Expected negative: exit 1**, bounded deadline before test admission; not a gameplay failure | 0.04s |
| `test-ui --scenario launcher` without permission | **Unexecuted exit 2** before any preparation/Godot/display launch | 0.00s |

Selectors use `dotnet tools/DevRunner/bin/Debug/net10.0/DevRunner.dll` after shared
preparation; their full logs/JSON are under `logs/testing-overhaul/selections/`.
The first refactor fast run failed two stale 900s allowance assertions; corrected
to enforce bounded headless routing/explicit deep exceptions, not suppressed.
Final source formatting and later final build passed with zero warnings/errors.

The deep receipt's summary-only reporting correction is recorded with its
[measurement](verification.md#overhaul-measurements-2026-10-11); no expensive
repeat was required for that reporting-only change.

Source/export once-only preparation and cleanup evidence stays under ignored local
logs; checked-in documents contain no credentials. At final pre-delivery read,
main protection still has no required status-context list and no rulesets;
requires one PR reviewer. Existing `source`, `linux-package`, `windows-package`
contexts are preserved. Forge PR/head/check results belong to delivery, not these
local timing receipts.

## Remaining risks / deliberately unclaimed guarantees

Legacy E graphical code is still substantial maintenance surface; its entrypoints
are no longer a routine tax, but retained assertions are not proven by this
headless run. Native popup/layout/input and application music continuity need
rendered qualification when authorized. Test component signals are not physical
pointer input. Package headless loading does not prove final scene pixels.
Windows installer/upgrade, own-AppID cold launch, real Steam invitation/relay and
native GPU proof remain independent. Synthetic fixture tests were removed, not
the actual stress harness; `test-scale` is explicit and not run here. No estimate
is presented as a measured result.
