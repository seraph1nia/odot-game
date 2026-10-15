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

## Army plot/tower coverage admission (2026-10-12)

`ArmyPlotTowerTests` adds Tier A against actual SettlementCommands/ArmyConfiguration,
DefenseActor/CombatSimulation and the snapshot/AuthoritySession boundary. Existing
catalog/roster tests could not catch arbitrary paid plot IDs, a tower allowing
unit Send/recruit into its full footprint, exclusive-path races, or a bombard
radius implemented only cosmetically. Fixed seeds, owned serialized ECS worlds
and numerical profiles keep this cheap. Four paired initial-selection rows and
an unchanged-base-tower replay catch metadata-only rerolls; original paid economy
fixture goldens remain unchanged. `DefensePolicyTests` now asserts the complete
six-profile set and configured behavior, not a permissive count/timing bound.

The existing `receiving` Tier B case now additionally hosts the real army receiver,
HUD and native camera with current main resources. It catches wrong roof/ground
picking, absent/optimistic/stale native purchase/build/upgrade bindings and full-
footprint recruitment availability without launching Main, a world or a session.
No scenario/supervisor/display or separate test framework was added. Graphical
fixtures now use actual army-plot selection instead of BuyHome; they remain
unexecuted, not replaced by a claim of graphical equivalence. No existing negative
coverage was removed. [Exact receipts, admission costs, failures and limits](army-plots-towers.md)
distinguish 48 targeted cases from the final broad fast 484/649 passes and two
platform skips; actual `receiving` component time was 9.28s.

## Shared-arena review regression coverage

Tier A additions exercise serialized local/remote squad-selector rejection and
retry ledgers (`ArmyCommandTests`), last-survivor retirement/regroup/storage while
a real periodic casualty retains its frozen body (`CombatLifecycleTests`), and
compressed malformed member/body/event/forecast projections (`SnapshotPayloadTests`).
Existing ownership, combat-death and codec tests omitted these particular inputs.
Expected outcomes are rejection without mutation, immediate release of empty
actor claims independent of body expiry, and refusal before snapshot admission.
Fixed seeds and bounded ECS fixtures own no engine processes; estimated cost is
seconds, not a new campaign or graphical qualification. Native Welcome credential
persistence is not independently exercised by these codec checks.

Removed the unused single-target `SharedArena.Route`; all former routing tests
now call production `Routes`. They retain blocked/static/cross-sector, shortest
adjacent path and invalid-origin/range cases, add a one-search multi-target work
assertion, and use an independent test BFS distance oracle. No routing behavior
coverage was dropped; these tests still do not prove native transport or pixels.

Review-phase local receipt: locked `dotnet restore Odot.slnx --locked-mode` and
`dotnet format Odot.slnx --no-restore --include <changed C# paths>` completed.
The initial `dotnet test tests/Game.Core.Tests/Game.Core.Tests.csproj --no-restore`
selection covered `ArmyCommandTests`, `CombatLifecycleTests`, `HexRoutingTests`,
`SharedArenaTests`, `SnapshotPayloadTests`, `SharedStateTests`, `SharedCombatTests`,
`CombatReceivingEncounterTests`, `SessionFlowTests` and `AuthorityCombatTests`:
95 passed, two failed because null DTO fields were omitted rather than delivered
as explicit JSON nulls. After correcting those inputs, the snapshot-only rerun
passed 40; after extending identity checks to body/member collisions, the final
snapshot-only rerun passed 42. No skips. TRX evidence is under
`logs/shared-arena-review/{shared-arena-review,snapshot-review-corrected,snapshot-review-final}.trx`.
These are filtered passes, not a complete suite or native qualification. Full
suite, engine, transport, graphical, package and delivery CI remain outer-phase
work. Initial shim trust refusal occurred before restore; process-scoped
`MISE_TRUSTED_CONFIG_PATHS` allowed execution without changing global trust.

## Receiving follow-up: partial funding and malformed siblings

`SnapshotReceivingLifecycleTests` adds two Tier A paid-command/ordinary-step
regressions with seed 1. One verifies reserve identities and released claims
through the last participant's real death and body expiry. The other uses declared
long Mage timing to witness another actor killing its committed target, unchanged
latched aim, nonattacking reserves, and the real missed impact/recovery. Both
round-trip actual frames through the compressed receiving codec, without seeding
poses, damage, windups or validation-only casualties. Their custom configuration
and funded recruitment are not default balance or paid campaign acceptance.
Existing partial-funding checks stopped at wave entry, and aim tests had no
reserve members, so they missed these receiving failures. Estimated cost is a
few seconds of bounded deterministic steps, with no engine or child resources.

`SnapshotPayloadTests` extends the same compressed boundary with every required
unit/defense timing null, nested rules/board/army/research/campaign nulls, duplicate
structures, member/body/squad/event structure-target references, event actors,
forecast ownership/status shape, and inconsistent member profiles/pending flags.
Expected refusal is recoverable `InvalidDataException` before admission, with
unchanged authoritative state and a subsequent valid decode still succeeding.
This does not independently exercise native Welcome or credential persistence;
Main's existing refusal path retains state/credentials, and decoding still occurs
before Welcome installs either. No blanket exception suppression or new admission
framework was added. Broader fast, native, transport and CI gates remain outside
this focused review phase. Shipped X/Y rules and paid/ranged acceptance are
unchanged; both current combat specs now describe shared-wave progress,
not obsolete per-city isolation. `CombatLimitTests` extends its existing numerical
progress check through fallen-owner redirection: the dead preference's engagement
is removed, and the remaining active deadlines still refresh globally. Match now
owns this shared-wave decision directly on any actual health-progress report,
instead of combat manufacturing a living-owner broadcast for its caller. Movement,
misses and zero damage still cannot refresh them, and maximum duration is unchanged.

Follow-up local receipt: locked solution restore and changed-file
`dotnet format Odot.slnx --no-restore --include ...` completed with process-scoped
mise trust. The focused non-deep core filter selected `SnapshotPayloadTests`,
`SnapshotReceivingLifecycleTests`, `SharedStateTests`, `SharedCombatTests`,
`AuthorityCombatTests`, `SessionFlowTests`, `CombatReceivingEncounterTests`,
`PaidStoredVeteranHistoryTests`, `CombatLimitTests`, `CombatLifecycleTests` and
`ArmyCommandTests`: 126 passed and two new fixture assumptions failed. The fallen
preference had already been redirected and removed; the one-owner target-death
case had reached victory and correctly frozen its windup. After following the
redirected engagement and using a second ordinary owner/allocation to keep combat
active, the lifecycle/deadline-only rerun passed all 10 cases (no skips). Evidence:
`logs/shared-arena-review-followup/receiving-followup.trx` and
`logs/shared-arena-review-followup/receiving-followup-corrected.trx`. The other
selected cases passed before these test-only corrections; this is combined
focused evidence, not a clean whole-suite run. Broader fast, engine, real transport,
native credentials, graphical, packages and exact-head delivery CI were not run
in this review phase. Prior failed receipts and paid balance limitations remain.

## Core groups

Names below are relative to `tests/Game.Core.Tests`; `Tests.cs` suffix is omitted
where unambiguous. Costs are qualitative unless an actual receipt is named.

| Sources / helpers | Production behavior / meaningful defect | Dependencies, overlap and disposition |
| --- | --- | --- |
| Resource, EconomyCatalog, BuildingTransaction, SettlementCommand, CommandConstruction, World | Atomic six-resource payments/refunds, overflow, producer/recruit quotes, editing guards, stale generation/turn, ready/pause/retry | Small deterministic Match/Authority inputs. Keep A. Exact authored default prices are intentional product contract, not merely test counts. Replace graphical arithmetic coverage with these existing assertions, not copied implementations. |
| ArmyRoster, ArmyRecovery, ArmyProgression, ArmyCommand, ArmyPresentation, ArmyProgression's fixtures | First-fit homes, capacity, hall storage/healing, paid food, veteran identity/wounds, reconnect retry | Keep A: boundaries and serialized authority results detect actual resource grants, early healing, duplicate spend and ownership regressions. Not equivalent to a native roster button. |
| CityResearch, ResearchLifecycle, ResearchWitness, ResearchedRole, WorldtreeResearch | Earned points, exclusive paths, persistent effects/statuses, resolved future actions, actual village output/overflow, additive state/codec, all authority modes and canonical Worldtree mapping | Keep A including earned early-game witness; no new graphical campaign. Small deterministic effect inputs plus bounded ordinary setup. |
| CombatAction, CombatConfiguration, CombatDecisionRegression, CombatLifecycle, CombatLimit, CombatPolicy, Combat, CombatFixture, DefensePolicy, SplashTower, StatusEffect, UnitSize | Impact/movement deadlines, target locks, splash/status ordering, death reservation release, transfer/admission, stall limits, faction/size/overflow | Keep A: deterministic ECS actions/outcomes and negative paths. CrowdedBattle rows are slower (~3s each in first new source run) but enforce progress/separation/determinism; not deleted just for cost. Core in-process parallelism remains disabled for static Arch registries. |
| HexBattle, HexBoard, HexOccupancy, HexRouting, RoutingScoreEfficiency, DecisionEfficiency, SeededDecision | Routing/topology/occupancy, feasible targets, stable seeded ties, search work bounds | Keep A. Malformed/boundary cases and stable mixer vectors are valuable small tests. Independent algorithm oracle used only for well-defined bounded graph behavior. |
| AuthoritySession, AuthorityCombat, VillageAuthority, AuthoritativeQuery | Local/dedicated/host permissions, protocol/rate/malformed/credential failures, detached snapshots/current units | Keep A. Real transport not proven here; essential C cases remain. Serialization/insertion-order outcomes protect more than fixture construction. |
| Campaign, CampaignDiagnostics, CampaignDiagnosticsTests, CampaignFixture, CampaignAcceptance, CampaignTests.Strategies, SessionCampaign, SessionFlow, SessionFlowTests, VillageStrategy, EconomyArmyFixture, ArmyBalance | Rewards/boss/terminal/allocation transitions; ordinary paid strategies and historical causal openings | Direct Campaign transition/defeat/reward and early cooperative SessionFlow/VillageStrategy cases stay A. **21 strategy rows + 1 serialized campaign + 19 balance/calibration rows** move to explicit E (`Tier=Deep`). Full balance matrices no longer routine. Expensive causal opening fixtures kept where they protect capacity/retry/ordinary funding; no grants used to fake network readiness. |
| CombatPlayback, AssetPresentation, ProgressionPresentation, PresentationLimits, PresentationEfficiency, PacingEquivalence, SnapshotEfficiency, SnapshotPayload | Pause/transport baselines, effect deduplication, current displayed profiles, health fractions, deep copies, Brotli bounds, ordinary tick equivalence | Keep A. Strong replacements for graphical calculations, but no claim to prove rendered geometry or physical audio. |
| SteamInvitationPicker, SteamLobbyRules, SteamOperationState | Account/lobby/original-host/session namespace, stale async callbacks, consent generation and retry | Keep A; deterministic native-boundary logic. Real invitation/relay needs separate two-account qualification, not local GUI automation. |
| AssemblyInfo, Profiling/ProfileProgram, ProfileOptions, Measurements, OwnedProfileProcess, ProfileTests, WorkCounterTests, CampaignTrace, SnapshotProfile | Static lifetime isolation, explicit profile selection, cancellation/cleanup bounds, truthful metrics | Keep fast ownership/count correctness A. Profiling commands stay explicit diagnostics, not PR benchmarks. Shared in-process fallback still uses actual locked xUnit executor. |
| Profiling/ReferenceTraces, ReferenceTraceTests, CombatReplayFixtureTests, LargeBattle, LargeBattleFixtureTests | Ordered replay golden hashes; synthetic stress input construction | Replay/golden rows move E (2 rows). Delete LargeBattleFixtureTests (4 rows) which pinned synthetic dimensions/roster counts and mostly validated the fixture against itself. Actual `test-scale` still runs production combat for 600 ticks and asserts identity conservation, faction/capacity/unique reservations, reconstruction, event bounds, admission, attacks and deaths. HexBoard tests retain malformed graph validation. No stress correctness guarantee is claimed by ordinary A. |

## Shared arena domain migration

`SharedArenaTests` and `SharedCombatTests` add Tier A production coverage for
connected 1–4-player sectors, reserved objectives/weapon footprints, uncapped X/Y
(including zero channels), member contribution/chill aggregation, deterministic
friendly exchanges/destination conflicts/death release, simultaneous squad/member
death and overkill, paid homogeneous recruitment/partial funding, physical Keep
access while distant defenders live, fallen-owner survivors and bounded solo
progress with exclusive occupancy. Legacy individual/city-isolation tests cannot
cover these actor contracts. Inputs use immutable seeded policies, explicit
combat member fixtures or actual paid Match commands; no engine/world process.
Estimated cost is seconds; the initial selected domain plus independent resource,
economy, building transaction and Worldtree regression run passed 50 tests in 2s.
This is partial domain coverage, not complete campaign/native/transport acceptance.
Migration is staged; remaining old individual/city-isolation tests are not evidence
for the new rules. No complete campaign/native/transport acceptance is implied.

### Reservation and persistent roster migration

Retired the unused `HexOccupancy`, `HexRouting`, `ReservationOwner`, city-filtered
`CombatStage`, and the per-member reconstruction overload. `CombatReservations`
now reconstructs current squad source claims and committed transits; `SharedArena`
and `SquadMovement` remain the live routing/admission owners. `HexReachability`
keeps its independent immutable graph-distance/parent checks.

- Replaced `HexOccupancyTests` and size-budget rows in `UnitSizeTests` with
  `SquadReservationTests`: exclusive sources, unique destinations, synchronized
  friendly exchange, malformed identity/size/anchor/timing/transition/lifecycle,
  inactive transit refusal, static footprint exclusion and immediate death release.
  Actual authority movement verifies source ownership through tick 29 and arrival
  at tick 30; no early destination claim or retained corpse transit.
- Replaced old `HexRoutingTests` and retired `RoutingScoreEfficiencyTests`' unused
  individual goal/cache policy. New real arena routes check shortest adjacent
  detours, static/opponent blockage, availability after release and cross-sector
  Keep approaches. No claim is made for the removed size-based ranged screen preference.
- `AuthorityCombatTests` preserves paid openings, complete wire round trips,
  decisions, bounded history, corpse resynchronization, all three authority modes,
  pause and disposal. Reconstruction uses received squad identities, not member
  cosmetics. Verified immutable topology/rules are reused across ticks instead of
  rebuilding all-pairs geometry twice per snapshot.
- `ArmyRoster/ArmyProgression/ArmyRecovery/CombatLifecycle` migrate home index versus
  physical cell and member versus squad identity assertions, while retaining full
  payment denial/nonmutation, independent hall upgrades/capacity, injuries, level,
  food priority, paid recovery eligibility, healing ceiling/overflow, reconnect
  retry, frozen death pose/expiry and registry disposal. Eliminated rosters retain
  inactive identities; settlement/return commands remain denied. Fallen-member
  redirection is explicitly rejected without mutation.
- Direct research modifies both X/Y channels but not the separate structural
  profile. Existing ranked-boss/research equivalence caught and repaired the
  accidental structural modifier. Defense events are retained by sequence during
  receiving tests, not by enlarging the authority's bounded history.

The coherent selected persistence/shared-domain batch passed **54 tests in 11s**.
The first retirement batch passed 47/48; its only failure was an already-in-range
route fixture, corrected to a source requiring movement. The final coherent
selection passed **103 tests in 13s**, including the genuine query work counters,
all affected persistence/recovery/authority classes and shared-domain checks.
These are Tier A numerical and codec proofs, not rendered/native transport or
complete paid balance evidence.
The retained broad diagnostic (341 passed / 148 failed) is still unresolved; no
blanket golden update or full-fast-green claim is made. Legacy synthetic profiling
and remaining actor-policy tests still require explicit migration.

### Landed tower integration

Integrated actual MAIN `f9f02b6bb44e247a7a124699905649cf2d04bc94` from
https://github.com/seraph1nia/odot-game/pull/22 by normal merge, preserving paid
numbered arbitrary-order plot purchases, both exclusive tower paths, refunds,
full configuration identity and host-loss/menu publication order. Squad home
claims skip locked and tower-occupied plots, not the first N cells inferred from
purchase count. Army-plot weapons occupy their corresponding shared deployment
cell until sold; city-plot weapons retain dedicated static cells. Native markers,
roof/ground picking, squad reinforcement, deployment highlights and Town Hall
return use the same accepted ownership projections.

Migrated landed tower damage/receiving cases from capped cross-hex/member targets
to committed squad targets and independently configured X/Y. Five-member Bombard
coverage catches a retained three-victim cap; adjacent groups/friendlies receive
no damage. A selected group survives member loss/sector redirection, but loss of
its final member misses without retargeting. Tower catalog/HUD tooltips expose
actual X/Y and physical range. Historical old-anchor transcript vectors were
replaced with complete rules/seed reconstruction and ordered squad-state/event
comparison, not refreshed hashes. Upgrade costs, identities, timing, denial,
retry and credential resume checks remain.

Post-merge paid/shared selection passed 21 tests in 5s. Integrated selection
passed 110/112 in 18s; the two obsolete individual transcript fixtures were then
migrated and passed their focused recheck (2 tests / 2s). These are selected proofs,
not a complete fast-suite pass. Whole solution compiled. Focused native `arena`
passed 2.97s (8.18s including preparation), and `receiving` passed 23.33s (41.80s
including preparation). Receiving first caught obsolete six-size/radius tooltip
claims; source and expectations were corrected to the actual squad profile. A
later harness cancellation occurred during preparation build before engine launch;
process-local non-shared compilation/node reuse off completed the recheck without
changing tools or touching an unrelated compiler. Existing runner owned cleanup
was retained. No graphical/application/campaign journey, real transport/package,
GPU/choreography or historical native-failure diagnosis is claimed. Three-phase
integration and full redesigned-game qualification remain pending.

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
| EngineTests and src/Game/Verification/EngineTests | Five selectable minimal headless cases against production ClientSettings, UnitAssets/AnimationPlayer/Skeleton, AssetFidelityProbe, imported Worldtree/game adapter (also loaded from actual client exports), and actual Tabletop HUD/session receiving bindings. Each owns user state/logs/marker and a 60s process bound; real signal/persistence/pose/authority outcomes, no GameApplication/Main startup. |

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
resource/modal/input/Settings-service assertions were consolidated into the existing
`settings` engine case, selected by ordinary source CI. The later selected-subset
cleanup removes eight unused portable scene loads and two portable-only quote
assertions. Both selected kit scenes, font/seal/branch/button states,
outside-click blocking, Tab/Escape, later-menu sibling picking, Audio input/service,
draft/Apply/persistence and both window bounds remain. The superseded compact
research-center footprint assertions are replaced by the landed `worldtree` case
and actual `receiving` research bridge checks, not revived as an obsolete surface.
The original settings default/mute/gain/reload assertions also remain.

The [framing/renderer/Worldtree batch](view-renderer-worldtree.md) extends these
existing cases, rather than adding a full-game journey: `settings` checks native
camera projections at five aspects, compact passive landscape resources and two
owned minimal-process renderer restarts; `worldtree` checks the single hover/focus
surface and its intent/input traversal; `receiving` retains real authority,
rejection and foreground-restoration checks while adding acknowledged HUD balances.
Native MultiMesh transform readback is unavailable on Dummy (a standalone setter/
getter counterfactual returns identity); native buffer placement and rendered
opacity remain unqualified. Coordinate retention is content/bounds/helper proof,
not a fabricated native readback. Duplicate bottom/footer/tooltip assertions are
replaced by the one visible surface; costs, locks, inactive nodes and modal negative
paths remain. The new short-lived passive fixture releases its owned derived
material cache before RenderingServer teardown.

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

Replacement-focus coverage uses actual SteamFriendsDialog and ClientSettings,
closing each then opening a confirmation before deferred callbacks drain. Live
return targets and hidden/freed targets requiring fallback must not steal the new
Stay focus; Enter must cancel only that confirmation. Ordinary invoker/fallback
restoration remains exercised, and external/missing keyboard focus must be
repaired by the foreground receiving adapter before activation. Earlier cases
placed focus inside the modal and missed this deferred ordering defect; pure C#
cannot exercise Godot focus dispatch. This remains B in the existing settings
selection with owned state, real controls and bounded frame waits. Added cost is
estimated at a few frame waits, not matched timing or graphical qualification.

The existing offline Steam/native-payload probe also consumes the woodland Theme,
font metrics, seal/branch textures, both selected kit Control scenes and bundled
JSON provenance/nonempty notices from the active `res://` root. Exported clients
retain the guarded `Verification/EngineTests.tscn` for the landed exported
Worldtree component dispatch, while excluding the retired ui-resources launcher.
This catches missing or unloadable
PCK resources/notices that the headless start/build/recruit protocol smoke never
uses; source inventory hashes and component checks cannot establish exported
resource resolution. It reuses the bounded `SteamProbe`/`SteamChecks` owner, creates
only immediately freed hidden Controls, and never starts GameApplication, a match
or a display. Setup uses ordinary source/export preparation and existing owned
preferences/cleanup. Incremental cost is estimated as resource loading, not a
new journey; this does not qualify rendered pixels or authoritative HUD bindings.
The original package exclusion of EngineTests predates the landed opt-in Worldtree
package probe. The combined client intentionally retains that minimal scene for
its guarded headless dispatch; the retired ui-resources launcher remains excluded.

## Battle-flow authority and component coverage (2026-10-12)

`BattleFlowTests` is Tier A: seeded same-combat comparisons at 0.5/0.75/1.0
wall rates, fixed equivalent simulated time, bounded catch-up, paused debt,
host/dedicated authority, retry/disconnect/resume, three-stage/three-production
settlement and atomic two-event overflow, stable forecast-to-spawn allocation/profile,
codec round trip and explicit historical fourth-phase refusal. Inputs are
numerical authorities, explicit wall slices and no children. Existing cheaper
arithmetic tests miss the authoritative admission/manifest boundary; expected
incremental cost is under a second per case (actual aggregate receipt is linked
below). Existing phase helpers now use three readiness checks rather than four;
playback fixtures explicitly select the former pace when testing tick boundaries.
No gameplay balance constants are altered to make a fixture pass.

The existing `receiving` Tier B selection additionally runs `EngineBattleFlow`:
real top native speed buttons, pointer and keyboard activation, delayed accepted
selection/disconnected denial, imported ordinary enemy rigs in the authoritative
forecast formation, the same production unit-bounds ray picker, ordinary unit
inspection with no army actions, live-membership exclusion and owned forecast
removal. This minimal Controls/rig component creates no Main, application,
Tabletop world, multiplayer peer or alternate assets. Pure A cannot prove native
input/resource/picking behavior. It reuses the existing owner/frame helpers and
60s bound, rather than adding another journey. Headless cannot certify cool
rendered presentation, GPU pixels, desktop focus or a complete UI-session
transition. [Reconstruction coverage](battle-flow-reconstruction-coverage.md)
owns reconciliation of the historical four-phase economy transcripts and exact
failed/passed receipts; an intermediate broad failure is not green coverage.

## Combined Tabletop receiving coverage (2026-10-12)

`test-engine --scenario receiving` is the fifth source Tier B selection, owned by
existing EngineTests. The production `Tabletop.InitializeHud` / `RefreshHud` seam
constructs the same stock/income/selection/feedback Controls and callbacks used
by the ordinary tabletop without calling its world/application startup. Host
callbacks retain production dialog/input/invitation ownership. A transport-only
IGameSession fixture captures commands with Command.FromSnapshot, applies them
to a real numerical Match, and explicitly delays acknowledgment; it owns no
pricing, purchasing, research allocation or presentation implementation.

Cheap rules, inventory loads and the standalone Worldtree adapter miss a wrong
focused-city sender, missing retained native labels, optimistic spending, or a
research tree disconnected from the production session bridge. Native viewport
input now exercises actual build/ready/pause/city/research Controls; negative
callbacks exercise foreign/disconnected/ready/paused/suspended denial. Real
accepted and stale rejected requests update stock, income and feedback; research
covers prerequisites, 3/6/9 quotes, exclusive sibling, production offshoot,
intent-before-acknowledgment, rejection without acquisition, Close/focus return,
resumed HUD commands and owned Control/controller disposal. Existing Settings
and landed Worldtree assertions remain separate owners, not duplicated journeys.
The obsolete compact-center layout helper and unused portable skill_tree runtime
member are removed; original upstream hashes remain separately archived and live
research authority is not replaced by a demo. Real HUD quote/denial/action checks
replace the removed verification-only portable quote coverage.

The construction/Resources spacing regression also strengthens this owner (see
[measured layout receipt](ui-migration.md#construction-and-resources-spacing)).
Real Button/TextParagraph/RichTextLabel metrics catch native title/cost line-box
intersection and a caption omitted from the Button minimum; real ledger labels
catch decorated-frame inset, column alignment, stock/upkeep intersection and
stale deferred minimum-size allocation. Whole-frame viewport containment and the
12px right inset are checked through minimum growth and shrinkage, including
combined disconnected/max-int stocks and numerical Preparation/Combat phase
transitions with paused/disconnected context; panel-local safe bounds alone missed
right-edge clipping. Four current window sizes use production canvas scaling,
starting/disconnected/max-int stocks exercise disabled/enabled controls and
complete values. A settled container reproduces the old allocation;
the unchanged asset component supplies the unsafe-inset counterfactual. No pure
C# test sees these native metrics. This adds bounded layout frames within the
same 60s component allowance, no new harness, process or full-game journey.
No existing authority/modal/input assertions are removed; pixels/readability,
GPU, session acceptance and the reported screenshot viewport remain unproven.

The cross-subtree invitation regression strengthens this B receiving fixture:
research opens through the production Tabletop callback, then the production
GameApplication invitation factory/opening boundary suspends it. Native Tab,
Shift-Tab, all four focus directions, Enter, Escape, header/pointer close and
external dismissal must affect only the invitation, never purchase or close
research. Dismissal restores research visibility, selection/focus, traversal and
ordinary purchase intent through the existing session sender. The application
CanvasLayer precedes the later HUD subtree as in production. No application,
world, Steam pairing or credentials are started. Sibling-only Settings/Friends
checks miss that input order; pure C# cannot route Godot native input. Existing
owned state, three-frame synchronization and the 60s bound are reused; estimated
incremental cost is a few dozen layout frames, not a second journey. These checks
do not certify desktop focus or rendered pixels.

The restoration-freshness regression extends the same receiving case with real
commands from two numerical-authority session adapters: paused Building research
is covered, the peer resumes and its accepted snapshot is delivered while hidden,
then native Stay must restore enabled purchase, current cost/description/feedback
and prior focus immediately. The opposite peer pause must disable purchase and
use the existing root-focus fallback without spending on Enter. A specialization
request sent before coverage is acknowledged while hidden; dismissal must project
its acquired node, spent points/progress, exclusive sibling lock and actual result
feedback. The prior constant-state invitation checks missed these transitions;
rules tests cannot exercise the visibility callback before focus restoration.
This remains Tier B with the same owned Controls, snapshot delivery, frame waits
and deadline. Estimated added cost is a few frame waits plus the already-owned
awakening, not a new application or transport journey. Ordinary opening and
invitation restoration now share the Tabletop visibility refresh boundary; the
opener no longer carries a duplicate refresh.

The foreground-host regression additionally opens the actual persistent Settings
button over research, exercises Tab/Shift-Tab, all four directional keys and Enter,
then opens the production leave Controls through Settings Return. Nested Escape,
Stay and header dismissal must restore only Settings, without closing or buying
research. A peer pause acknowledged during coverage must become the research
projection only when the last dialog closes. Both close orders are exercised,
including delayed Settings focus restoration while Leave still owns the scope.
Ordinary Settings close and the earlier invitation/replacement cases remain.
This is Tier B in the same receiving scene: earlier invitation-only and sibling
Settings fixtures missed the later HUD subtree's viewport input ordering, and
pure C# cannot route it. Deterministic native Controls, owned preferences, accepted
snapshots, existing frame barriers and the same deadline are reused. Incremental
cost is estimated at a few dozen frames; no full application, graphics, Steam
pairing or real transport is added. The host adapter now derives scopes from its
existing foreground sibling order, not independent Join/Leave subscriptions.

Observed implementation cost: receiving **62 checks, 7.07s component / 12.92s
including locked preparation**, `logs/20261012-082246-34c944ee/`; Settings **89
checks, 1.96s / 6.91s**, `logs/20261012-081718-ddd7dd77/`; landed Worldtree **74
checks, 6.98s / 11.89s**, `logs/20261012-081725-5a8b3774/`. Fast C# passed **466
core + 649 runner**, two existing native-Windows skips, in **19.92s** under
`logs/20261012-081615-bd78389b/`. These are focused user-owned combined-merge
implementation receipts, not final pipeline/package/hosted-CI qualification.

Failed preparation and fixture development attempts remain in ignored logs:
`nm-025-receiving-1` rejected an unregistered CLI selector; `-2` failed on UID
lookup/reimport errors despite native exit 0 (not a crash). The owned generated
filesystem scan cache was preserved and moved aside for a rescan; subsequent
import passed without a source/tool change. That is a bounded cache-input
observation, not an engine root-cause or historical T1 repair certificate.
`-3` exposed an incorrect fixture assumption about the real two-wood farm price;
`-4` through `-6` exposed stale fixture research snapshots lacking an authority
revision (building phase does not advance combat ticks). Fixture setup now uses
an owned-peer connection cycle to publish a genuine Match revision, not forged
snapshot fields or weakened production caching. `-7` passed; `-8` additionally
qualifies actual opener focus, sender resumption and explicit disposal. Each
retry had a changed relevant input or explicit diagnostic, not blind retry-to-green.

No Main/GameApplication, full world/campaign, display, screenshot or physical
input was launched for these checks. Rendered readability, native window focus,
real transport, local Windows, exported receiving HUD and all final package/CI
identities remain outside these implementation receipts. Original accepted T1
is still an unresolved historical failed native-import occurrence; no new-crash,
missing-coverage or publication waiver is implied.

## Worldtree review coverage adjustments

The existing Tier A Worldtree production fixture now also checks independent
expected researched labels at both levels and for upgrades (farm +7 and 7 → 10;
stone/wood +2 and 2 → 3). Income-only assertions could miss stale producer labels.
The native-client prerequisite fixture covers the exported component caller and
selects the current OS layout; fixture files do not qualify Windows execution.

The existing Tier A authority admission fixture rejects the concrete previous
protocol 11 at fresh and resume admission with no snapshot, credential or city
allocation, then admits/resumes a current-version guest with all three new catalog IDs. Steam invitation
coverage also rejects protocol-11 metadata. Relative-version rejection alone
could pass without advancing the compatibility boundary. These strengthen small
existing fixtures with negligible estimated incremental cost.

The existing Tier B Worldtree case passes real visible/hidden research and
embedded Window candidates through the same GameApplication modal selection and
input routing used in production. The earlier always-null routing fixture missed
the selection decision. Native mouse/wheel/keyboard GUI outcomes, embedded-window
outside-event consumption and the shared world-blocking predicate are exercised,
including world input resuming after close. Fixed small controls, one owned
embedded dialog and process-frame synchronization replace no journey; estimated
incremental cost is a few frames, not a new process. The unrequired Worldtree
nested-Settings route stays removed; the consolidated production Settings/modal
regressions above remain in their normal owner. Research authority/disconnection/
teardown negative paths remain. Full application, physical input, pixels and native
Windows package qualification are not established by these focused fixtures.

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

## Shared squad action, routing and status migration (2026-10-13)

A fresh Tier A diagnostic on the integrated landed-tower/squad head executed
**408 passed / 109 failed / 517 total**; runner tests were not selected. The
retained TRX is `logs/20261013-002324-d9cf5e7b/core-results/tests.trx`.
This is a migration diagnostic, not a green acceptance receipt.

`CombatTests` now uses six distinct physical homes with eighteen persistent
members in homogeneous groups, rather than dozens of independent soldiers
stacked at one fallback home. It retains full ordered state/event comparison
under reversed insertion and extra observation, identity conservation through
casualty cleanup, queued enemy conservation/admission, real typed reservation
reconstruction, mutual impact timing, cadence/recovery, paid transaction and
ownership negatives, pause/resynchronization, and bounded event-history/codec
checks. The crowded fixture is explicit numerical setup, not paid campaign
balance evidence. Elimination no longer generates deaths; the record-cap witness
instead exercises an actual burst of nonparticipating casualties and expiry.
Static terrain exclusion and exclusive squad claims replace size-budget anchors.

A genuine defect exposed by the lost-target witness was fixed: each contributing
member publishes a miss at its committed impact tick if the entire target group
is lost. Changing sector preference does not teleport the target or cancel the
cycle. Squad decision projections now expose the current in-range objective and
clear obsolete member-visited hints. `SharedArena.Routes` expands the current
physical graph once per deciding squad and scores all reachable objectives from
that immutable search. `CombatPolicyTests` checks the genuine BFS counter site,
committed-move stability, blocked target release, shared foreign-sector
engagement and immutable shortest-predecessor evidence. This fixes the earlier
per-opponent expansion cost without inventing old counters.

Unused `CombatDecisions`/`CombatRanking`/`BattlefieldObservation` and their
member/capped-victim/city-local policies are retired. Their test-only policy
comparisons do not protect an active simulator. Independent seeded decision
vectors, actual insertion/observation-invariant combat, graph malformed-input
coverage and the immutable BFS oracle remain. `DecisionEfficiencyTests` retains
the graph ownership/shortest-predecessor negative cases; it no longer claims
per-city observation caching or a victim-cap fast path.

`StatusEffectTests` retains burn/poison refresh, expiry/terminal periods, malformed
status rules, per-contribution Guardian rounding, lethal periodic simultaneous
strikes and wire detachment. It now attributes statuses to actual nonzero X/Y
member contributions, including independent zero channels, all damaged group
members and no status on undamaged members or lost groups. Sector redirection
retains member status/health and the current physical squad claim. Death clears
active scheduling while event `Periodic` retains causal attribution.
`ResearchLifecycleTests` checks mean chill across a three-member actor (one
40%-chilled member commits a 14% group slow) and unchanged committed timing after
refresh. Watchpost direct damage does not inherit Mage Y or harm another group.

Focused combat migration first passed 15/17; two fixture-only errors (an explicitly
reserved tower-cell origin and an inconsistent legacy profile-size DTO) were
corrected and their recheck passed 2/2. Status/research and shared-policy selections
passed. Final coherent targeted selection across changed actions, statuses,
routing, roster/recovery/progression, towers, authority, reservations and shared
state: **168 passed / 0 failed / 0 skipped, 31s**. Exact command/TRX:
`logs/shared-arena-task/shared-action-coherent.txt` and
`shared-action-coherent-results/coherent.trx`; targeted formatting passed.
Remaining broader fixtures, paid twenty-wave balance, real transport/package,
three-phase integration and native aiming/choreography are not qualified by this
selection. Earlier broad failures are retained, not relabeled green.

## Physical world, campaign and limit fixture migration (2026-10-13)

`CombatFixture.AtCityEdge` now places complete enemy groups at distinct actual
Keep approaches across the current connected arena. It neither splits members
between anchors nor stacks independent groups, and retains each actor's recovery
unless a test explicitly resets it. The soldier fixture uses a real owned
homogeneous home assignment. These are deliberate numerical fixtures, not paid
campaign or rendered-formation evidence.

`WorldTests` retains command ownership/payment/retry, production/readiness/pause,
member conservation, simultaneous army casualties without city elimination,
locked-tower cancellation, disconnected surviving players, future original-roster
allocation, death cleanup and gross campaign transitions. Physical routing
replaces old 21-cell positions. A newly present defender cannot magically shield
a committed in-range Keep strike. Redirected enemy squads keep source cells,
health and archetype-specific windup/recovery instead of teleporting to another
city entry. The built-in defender requires a real in-range group; the rapid-next-
wave casualty fixture now supplies that geometry rather than relying on a
city-wide shot. Same-tick last-enemy/last-Keep defeat remains independently covered.

`CampaignTests` preserves authored composition/order, original-roster quantities,
levels/boss progression, gross reward bounds/overflow/research, no duplicate or
local-clear reward and terminal nonmutation. A boss has one member and one source
claim of size one in shared namespace one. `HexBoardTests` migrates only obsolete
21-cell/six-anchor/hostile rear exclusion expectations; reciprocal adjacency,
all-pairs axial distance, deterministic edge IDs, detached authoring arrays and
all eight malformed-topology cases remain.

`CombatLimitTests` uses explicit fast movement and zero structural damage so its
500-tick no-health fixture is legally configured and physically incapable of
incidental health progress. Production allowance validation is not relaxed.
Shared health progress extends all current pressure deadlines, rather than
asserting an isolated city lane. Pause, exact duration/no-health deadline,
normal-result precedence and frozen terminal movement remain covered.

That frozen-movement witness caught a production defect: last-corpse cleanup
restored homes even after defeat, erasing the terminal pose. Cleanup now preserves
defeated actors' frozen position/action while retaining existing preparation and
victory roster restoration. The first fixture selection passed 37/39; the two
failures distinguished this real defect from a mixed-archetype fixture wrongly
assuming every strike had already landed. Initial coherent recheck passed 92/93
and caught accidental loss of victory restoration; corrected the phase scope,
not the existing paid recovery assertion. Final affected selection:
**93 passed / 0 failed / 0 skipped, 20s**, including shared state, authority,
roster/recovery/progression and lifecycle. Receipts:
`logs/shared-arena-task/physical-world-coherent-02.txt` and
`physical-world-coherent-02-results/physical.trx`. Formatting passed. These tests
exercise the currently landed phase flow; qualification of the approved new
three-preparation timing still awaits actual prerequisite integration. Remaining
broad failures and twenty-wave paid strategy/balance/transport/native/package
checks remain unexecuted since this seam.

## Coherent legacy combat-suite replacement (2026-10-13)

`HexBattleTests`, `SplashTowerTests` and `DefensePolicyTests` were replaced as one
subsystem against the current squad/X-Y/physical-objective owners, not repaired
assertion by assertion to retain individual combat. The retained initial broad
receipt executed **36 old rows: 4 passed / 32 failed** across these classes
(22 HexBattle, 8 SplashTower, 6 DefensePolicy). Those failures remain in
`logs/20261013-002324-d9cf5e7b/core-results/tests.trx`. The new classes execute
**27 focused rows**, with existing new suites owning overlapping protections.
No independent paid economy, authority, codec or persistent-member suite was
removed. `CombatFixture.Group` creates members through actual homogeneous grouping
then supplies an explicit numerical pose; it is not another combat simulator.

| Removed old contract | Replacement behavior / executable coverage | Retained negative control / limitation |
| --- | --- | --- |
| Individual size-budget anchors, automatic member packing and protected rear exclusion | SharedArena/SquadReservation/CombatTests: one squad claim, stable member slots, actual paid deployment, queue conservation, connected routes and progress | Static exclusion, duplicate/hostile/staggered claims, destination conflict and immediate death release; formation geometry remains separate native evidence |
| Cleared-city teleport, cumulative corpse leases and fixed protected-entry admission deadlines | SharedCombat/WorldTests: physical rescue, fallen-owner survivors, source-preserving group preference redirection, current-battle/future-participation distinction | No magical Keep shield, terminal defeat precedence, future eliminated command denial; obsolete admission DTO/runner consumers still require a separate migration |
| Member-local routing/cache guarantees and blocked size bands | CombatPolicy/SharedArena: one actual BFS per deciding squad, reachable target selection, released-cell reconsideration, immutable committed move | Independent graph oracle, static detours, malformed transitions, seeded observation/insertion invariance retained |
| Capped/cross-hex Mage splash and deterministic lowest-ID primary | SplashTower: ranked five-member X/Y profiles, all four zero-channel combinations, exact fractional HP and seeded primary within one group | Adjacent enemy group and friendly remain unchanged; only nonzero contributions appear as victims; StatusEffect covers actual damaged-member attribution |
| First member death or sector transfer invalidates an entire strike | SplashTower and ArmyPlotTower: group commitment survives member loss and nonteleporting preference redirection | Final group loss and out-of-range group miss without acquiring a nearby replacement; no free retarget or overkill transfer |
| Moving individual source/destination splash clusters | HexBattle/SplashTower: one committed group uses source through tick129 and destination at exact tick130; typed reconstruction matches actual claims | Single-target and X/Y range-invalid misses, unaffected neighboring groups, nonparticipating/dying members excluded |
| Death-facing tied to a live target and cosmetic-death occupancy | HexBattle: immutable frozen target pose, exact expiry, moving Watchpost objective and released source/transit | Target relocation cannot change a retained corpse; captured objective is actual Watchpost, not owning Keep; native artistic facing not proven |
| Recovery-boundary last-hit ordering | HexBattle/DefensePolicy/SharedCombat: lethal boundary cannot start another cycle; squad, Watchpost and tower contributions gathered against pre-impact members | Four committed impacts remain landed despite mutual deaths, actual health capped once, no duplicate defense impact or corpse claim |
| Old two-victim role-win/casualty goldens | HexBattle paired physical screen/exposed control plus current X/Y, cadence and member-contribution coverage | Same support identity/initial shot timing; protection changes received HP, not invented rate; repeated paid comparative twenty-wave balance is still pending |
| All towers/defender share a virtual city origin | DefensePolicy: distinct Watchpost/tower origins, physical source/type snapshot, all four paid tower cycles/ranges and shared pause | Range-invalid miss, no nearby replacement hit, no soldier body for a weapon, no duplicate impact |
| Cap/radius settings are the defense damage algorithm | DefensePolicy: actual six-profile catalog X/Y/range, configured cycle/fingerprint and actor identity | Invalid slot and cross-owner commit refuse; unused legacy profile/config cap/radius fields remain a separate retirement seam, not an active second simulator |
| Tower ownership disappears when its Keep falls | DefensePolicy/ArmyPlotTower/World: cease pending fire, retain paid purchase/type/generation and inactive physical weapon for spectator | No fictitious tower destruction/refund, eliminated sell refuses without mutation; paid upgrade/ownership/retry/resume/codec coverage retained |

New defense fixtures use accepted paid build/upgrade and readiness/pause commands
with declared numerical material stocks; they do not prove an earned early-game
Catapult opening. The component source is deliberately bounded (eight readiness
rounds, finite combat ticks), disposed on setup failure, and uses no child process.

Initial replacement selection: **25 passed / 1 failed / 26 total, 4s**. The failure
caught a real casualty projection regression: `FrozenAim` was no longer captured.
The necessary production fix captures a value target pose at death without a
live reference or logical lease. Source review then identified the moving
structure distinction; a new focused witness reproduced **Watchpost expected
cell32 / actual Keep cell60**. Routing now freezes the exact objective cell for
that case. A nullable-struct assertion compile error before that reproduction is
retained separately; it was not a runtime/tool crash. Initial coherent selection
passed 208/208, then final affected selection after the Watchpost fix passed
**209 / 0 failed / 0 skipped, 41s**, including the 27 replacements and their
consolidated shared/paid/authority/persistence/status regressions. Exact receipts:
`logs/shared-arena-task/combat-replacement.txt`, `casualty-objective-02.txt`,
`combat-replacement-coherent-02.txt` and their adjacent TRX directories. Source
replacement and the narrow production diff were inspected; targeted formatting
and `git diff --check` passed.

This is a coherent focused Tier A batch, not full-fast green. No broad retry,
engine process, real transport/package, paid deep campaign, graphical acceptance,
pipeline or publication was performed. The unlanded three-preparation work was
not imported, implemented or inferred from these current-phase fixtures.

## Coherent admission, profile and compatibility replacement (2026-10-13)

This is the bounded instruction-009 batch, not delivery qualification. The full
242-line initial reference inventory was reviewed and separated from unrelated
native UI/camera sizing, peer authentication and caller-side burn/casualty capture
admission. The final exact combat-contract inventory has only five references:
retired cap/radius names deliberately injected by codec refusal tests. No production
`AdmissionBound`, `TrackClearedAdmission`, `Admissions`, `UnitSpace`, profile/member
size, victim-cap or cross-hex-radius authoring remains. Typed `HexUnitState` and
`PositionReservation.Size` deliberately remain **one**, with malformed zero,
negative, stacked and overflow claims refused by `SquadReservationTests`.

| Retired contract / consumer | Current replacement and retained negative control |
| --- | --- |
| No-op cleared-city admission tracking/DTO and corpse-duration protected entry | Removed through CombatSimulation/Match snapshots/World. Real queued groups enter only free physical entry cells. `SnapshotEfficiencyTests.QueuedGroupsEnterOnlyAfterARealExclusiveSourceBecomesFree` uses eight real groups competing for seven entries: partial member removal cannot release a living source; final removal permits exactly one queued group. Existing HexBattle/SquadReservation tests own corpse release, immutable death pose, exact arrival, static-cell conflicts and malformed transit. |
| UnitSpace/Profile.Size, boss size six, capped/radius tower/Mage metadata, normalized but unused fingerprint inputs | `UnitTiming`, numerical X/Y/structure/range/cycle profiles and all six actual tower definitions. Current rules **7 / protocol 14**; full identity covers actual profiles. The landed independent base-actor salt still isolates optional Rapid/Bombard timing edits from unchanged squads/base weapons. Config overflow/rank/version/range/timing/capacity negatives, reconstruction and complete event/state observation-invariance remain. No historical digest was refreshed. |
| Legacy member-size recruitment, hall rotation and native inspector/rig observation | CampaignStrategy chooses empty or compatible homogeneous purchased non-tower homes using projected squad capacity; hall rotation counts individuals. ArmyConfiguration no longer takes an unused size argument. Settlement/roster/hall-generation/payment/retry/recovery guards remain. Inspector/hall labels and real rig observations carry squad/formation identity, not fictional member size. Existing Harness round-trip and minimal native arena witness verify the new observation binding. |
| Isolated third-city death/teleport, cleared occupied forward band and admission-bound journeys | Consolidated duplicated VillageStrategy journey into the paid SessionFlow three-city rescue/codec/veteran/upkeep/next-allocation test. The same old opening now physically rescues the uninvested city; no seed or balance rule was manipulated to force its old death. Existing SharedCombat cross-sector rescue/source continuity, Campaign fallen allocation, and AuthoritySession paused/eliminated credential/ledger tests retain deterministic negatives. The actual `redistribution` selector now owns three-peer agreement, paid rescue, ordinary pause/resume, credential restart and accepted-Ready 4/4/4 next allocation. It does **not** claim actual eliminated-spectator transport qualification. |
| Random-seed doomed-city runner fixture and home-index-as-hex comparisons | NetworkOpening is replaced as one policy subsystem: earned exact plot/trainer payment, physical owner/home mapping after cosmetic cleanup with persistent member/formation/XP continuity, and explicit fallen-owner policy plus authority refusal with an unchanged snapshot. Keep loss in that last cheap negative is test-owned; it is not an earned losing-strategy journey. |
| Capacity-six synthetic stress board, stacked unassigned terminal actors and old low-level trace goldens | LargeBattle now uses four-owner production geometry, explicitly stocked paid homes, legal homogeneous field groups and real queued enemy groups. Current 128-member counter-invariance input passes; 512/2048 stress/timings remain unqualified. ReferenceTraces uses real groups, legal two-owner sector redirection preserving pose/health/profile/cycle, exclusive reconstruction and homogeneous terminal groups. Historical exact digests are replaced by an observation-invariant repeat of the current profile inputs, not new goldens. Existing bounded replay test verifies paid homes, actual squad transit, member deaths, codec-compatible states/reconstructed claims, pause/resume and cosmetic expiry. |
| Silent unknown-field/default deserialization of old combat settings | Version is required in serialized CombatSettings; retired authoring/DTO fields are rejected by their existing typed owners. SnapshotPayload.DecodeForReceiving validates before Main's Welcome/ack/snapshot can change live state or credentials. Recoverable refusal directs the player to a compatible authority or fresh match. Codec tests inject old/missing/future versions, admission/size/cap/radius fields, duplicate member/group IDs, overlapping formation, missing squad identity, null populations and null arena geometry. Full valid codec round-trip, compression bounds and malformed Base64 guards remain. No save/economy migration framework was added. |

The snapshot projection owner also reuses the same immutable Arch view for squad
membership and individual projection, retaining the meaningful complete-snapshot
one-view/one-member-projection work guard. Retired individual sort/range counters
are unavailable (`null`), never fabricated zero. BFS edge accounting is checked
against the actual two-cell graph, not the old board's edge count.

Retained causal evidence:
- `admission-profile-build.txt`: CA1822 on a constant instance member-size property;
  obsolete DTO size and all consumers were removed, not suppressed. Build-02 retains
  seven genuine remaining compile consumers/old constructor errors; complete solution
  build-03 and later owned source preparation passed without warnings.
- `admission-profile-focused.txt` / adjacent TRX: **113 passed / 5 failed / 118**.
  Failures distinguished the paid third-city rescue, actual snapshot query work,
  obsolete contact fixture and retired counter/board assumptions. Focused corrected
  consumer selection passed **13/13, 5s**; coherent core selection passed
  **307 / 0 failed / 0 skipped, 49s**. It includes independent payment, authority,
  research, member persistence/recovery, simultaneous damage, compatibility and
  codec negatives, but excludes unrelated fast classes and full deep campaigns.
- `admission-profile-runner.txt`: **118 passed / 2 failed / 120**, old doomed-city
  and home-index-as-hex assertions. Coherent runner replacement passed
  **120 / 0 failed / 0 skipped, 6s** in `admission-profile-runner-02.txt`.
- Exactly two changed C# profile/replay input checks (Tier=Deep selectors only)
  passed **2/2, 11s**, `admission-profile-inputs.txt`. This is partial profile-input
  qualification, not a full deep campaign/stress pass.
- `admission-profile-engine.txt`, `logs/20261013-070525-40a83d9f`: minimal native
  arena/receiving/rig component passed **2.96s / 14.18s including preparation**.
  It exercises genuine imported rigs and observation IDs, not rendered pixels,
  complete application choreography, physical pointer input or GPU performance.
- `admission-profile-network.txt`, `logs/20261013-065404-21561616`: real three-peer
  rescue/agreement/resume passed before a late snapshot's allocation assertion
  failed (**17.93s / 23.02s**). Retained final Ready ack at tick1214 contained12
  members; the later host ring began at tick1366 with9 after legitimate casualties.
  The driver now binds initial allocation to that accepted Ready receipt. History
  remains bounded; no sleep, retry policy, speed or deadline was increased. The
  concrete correction passed **15.52s / 20.62s**, `admission-profile-network-02.txt`,
  `logs/20261013-070604-924b12fa`. Every owned child was awaited before cleanup.
- Final DTO review found that removed `UnitState.Size` still needed its existing
  owner to reject unknown member fields. Added strict member decoding plus three
  codec rows for member size and null cells/footprints (which must not escape as
  null-reference errors). Final locked solution build passed with **0 warnings /
  0 errors**; affected configuration/codec/SessionFlow selection passed **54 / 0
  failed / 0 skipped, 9s**, `admission-profile-review.txt` and adjacent TRX.
  Earlier narrowly affected final metadata parser selection passed **1/1, 71ms**
  in `admission-profile-final-metadata.txt`. These overlapping selections are not
  added to the coherent core/runner counts or called a full-fast rerun.

`ArmyBalanceTests` only received the necessary individual Hall-count consumer
migration in this batch. Its remaining old home-capacity/home-pose and paid
campaign outcome assumptions need coherent balance-fixture redesign and earned
qualification, not assertion-by-assertion repair. No complete deep campaign or
512/2048 stress qualification was executed.

Exact diff and reference inventory are retained under `logs/shared-arena-task/`.
Formatting and whitespace checks cover changed C#. No tool/dependency/asset lock,
developer preferences, model/session configuration, graphical process, full
pipeline, push or publication was changed. The three-preparation prerequisite is
still unlanded and was neither imported nor duplicated. Full-fast green, earned
repeated-seed twenty-wave balance, actual eliminated-spectator transport, packages,
full native acceptance and final delivery remain separate unfinished work.

## Paid strategy, research and provisional campaign replacement (2026-10-13)

Instruction011 authorizes this coherent numerical batch, **not final redesigned
balance**. The actual current source has three Building END incomes/heals and a
fourth income-free Preparation edit. The unlanded three-preparation/speed/preview
ship was not implemented, imported or simulated. Spending the third income in
that extra edit is a precise final-calibration dependency. See the
[measured paid baselines](paid-squad-baselines.md) for outcomes, resource/attrition
comparisons, timing dependency and unresolved pure-ranged/cooperative Keep risks.

| Retired fixture/contract | Replacement and preserved negative control |
| --- | --- |
| Recovered exact ticks476/481/845/872, HP/ID arrays, size-six homes and forced isolated-city winner/loser policy goldens in EconomyArmyFixture | One earned serialized two-owner opening verifies full homogeneous Sword homes refuse Crossbow support without mutation; real land/equipment payments, old refused request after expansion, accepted retries and credential rebind retain outcomes. Empty paid land creates/heals no member; deployment succeeds only to an available purchased home, and second occupancy refuses without mutation. No stock/HP/member setter is used. |
| Old home-index-as-hex placement and reserve cost hidden behind seeded treasury | PaidCampaignFixture uses accepted quotes/commands, default rules and reconstructed physical claims. An actual wounded first-clear veteran cannot afford its Hall before another real END; refusal leaves the entire snapshot unchanged. Paid storage, actual production healing and return preserve identity/HP/equipment/service. Individual Hall capacity and funded-but-nonparticipating stored members remain covered independently by ArmyRecovery/BattleFood/Settlement suites. |
| Guardian/Frost support stacked at the same member-anchor cell; duplicated insertion/status-expiry journeys | Two bounded 73-tick three-member group comparisons use distinct real cells and production actions/statuses. Guardian changes received injury without changing enemy impact count; Frost changes actual enemy impact count and retained member HP. These are explicit numerical fixtures, not free paid-campaign troops. ResearchLifecycle retains mean chill/immutable movement commitment/expiry, CombatTests retains insertion/observation/state/event invariance, StatusEffect retains status/periodic negatives. |
| Affordable Mage must be recruited even without a compatible homogeneous home | ResearchWitness preserves support priority when actual home, quote and food all fit; an incompatible Sword home never receives a Mage. Accepted investments validate exact resource/point payments and profile/HP nonhealing. Ordinary earned Fire/live burn plus prerequisite/exclusive Frost refusal remain; research-access/cue, codec, rank and overflow negatives are unchanged. |
| Alternative-economic-curve calibration and size-six/home-pose assertions in ArmyBalance | Default-price paired 2-home/L1/basic-Hall controls use the same paid driver and preserve normal melee victory, real restricted capacity/equipment, forbidden Hall upgrades, actual healing/investment and no stall assertions. No substitute economy or damage curve is authored. Four selected deep rows execute ten current-source campaigns, not a binary winner golden or final pricing certificate. |
| Mixed target10/2/2/2 implicitly requires seven homogeneous homes | Verification policy targets9/3/3/3, three Sword groups and one group per other role. All actual payment/ownership and six-home bounds still apply. Three retained legacy mixed acceptance rows still require full victory, recurring metal/cloth recruitment, all four roles, economic investments and normal production witnesses; their investments now share paid/claim guards. |
| New pure-ranged baseline accidentally measures an empty army | Retained first attempt fielded zero units and fell at764 ticks. A real purchased two-Lumbermill opening earns equipment wood and fields two paid Crossbowmen; both seeds still lose W1 (854/823 ticks). Baseline validation records these defeats rather than asserting they are victories, masking the issue or changing prices/damage/production. |
| Hall individual tile6 interpreted as a field home in restoration/wave entry | Actual paid reserve campaigns reproduced the production exception at W5/W7. Both existing pose owners now leave stored actors off-grid, with Stored lifecycle, no participation or claims. The bounded five-wave earned seed123 regression requires a real slot>=6, paid investment and actual healing; ordinary field-home index refusal is not relaxed. |

`PaidCampaignFixture` is a verification driver of the sole production Match, not
another simulator or child supervisor. Its accepted-command receipts, per-wave
entry/exit member states, food, healing and investment are recorded without
credentials. Current-wave food totals exclude stale receipts retained on fallen
owners. Attacker attribution uses landed member `Impact`/victim IDs, not `Hit`
(which projects the victim); initial zero counters are retained and explicitly
superseded by the corrected two-case receipt. Structured horizon metadata keeps a
five-wave regression distinct from a twenty-wave baseline.

First failures and selected receipts (all under `logs/shared-arena-task/strategy-batch/`):
- `initial.txt` / `initial-results/initial.trx`: unchanged legacy A **7 passed /
  12 failed / 19 total, 6s**. Failures were obsolete fixed tick/HP/admission poses
  and support priority without physical capacity; no golden was refreshed.
- `mixed-before.txt`: unchanged compiled old-policy mixed acceptance **3/3, 3s**,
  seeds0/1/123. Seeds1/123 before/after measured comparison is in the baseline doc.
- `build.txt`: xUnit Single-overload and constant receipt-property analyzers;
  `build-04.txt`: two collection assertions and formatting. Corrected source,
  not analyzer suppression. All final solution builds passed **0 warnings/errors**.
- `fixtures.txt`: first replacements **12 passed / 1 failed / 13, 11s**. The
  attempted Hall was genuinely unaffordable, so the witness now preserves refusal
  then earns its missing stone. `hall.txt`: focused corrected witness **1/1, 2s**.
- `baselines.txt`, `logs/20261013-080505-2b5742c3`: initial 14-row deep matrix
  **12 passed / 2 failed, 63.69s**; both reserve failures exposed stored tile lookup,
  not insufficient campaign damage. Two empty-ranged defeats were recorded, not
  campaign wins. `storage-ranged.txt`: **2 passed / 3 failed / 5, 3s**, exposing the
  second BeginWave pose mapping after the restoration-only fix. Both boundaries
  were corrected. `storage-ranged-02.txt`: affected **5/5, 6s**.
- `coherent-fast.txt`, `logs/20261013-082534-0e2f9092`: broader affected A
  **154 core / 49 runner passed, no failures/skips, 29.94s**, filtered economy,
  research, army, payment/resources, authority/session, reservations and lifecycle.
  This is not a full-fast run. After final helper/receipt guards,
  `final-fixtures.txt`: **14/14, 13s** (overlapping, not added to those counts).
- `final-baselines.txt`, `logs/20261013-083156-9ba5f0a1`: all 14 finite horizon
  rows passed **67.66s**. Actual outcomes were **12 victories / 2 W1 defeats**;
  test pass does not mean pure-ranged viability. This is not all deep coverage.
- `final-controls.txt`, `logs/20261013-083304-9020427c`: **7/7, 22.48s**,
  four paired control rows plus three retained strict mixed-victory rows.
- `final-rescue.txt`, `logs/20261013-083907-57daadcd`: corrected real attacker
  attribution **2/2, 50.25s**, 686/607 foreign-origin member impacts. Team victory
  still has **2 / 1 fallen owners** respectively; no all-Keeps rescue or actual
  eliminated-spectator transport claim is made.

Original/malformed/research/economy negative suites were not removed. New baseline
rows can record an honest defeat, while existing ordinary victory acceptance and
investment controls remain strict. Exact paid transcripts/per-wave facts and
original failures are retained. No damage/economy/phase authoring, tool/dependency/
asset locks, graphics, developer preferences, model/session settings, process
supervision, full pipeline, push or publication were changed. Source/diff review,
formatting and whitespace checks cover the checkpoint. Final economics-sensitive
recalibration, pure-ranged follow-up, complete deep/full-fast qualification,
eliminated-spectator/reconnect transport, packages and native aiming/animation/
resources remain unfinished.

## Shared-squad eliminated-owner receiving and resume (2026-10-13)

Instruction013 is an independent bounded authority/transport/receiving batch, not
final campaign or package acceptance. No unlanded battle-flow source, phase/speed/
preview authoring, prices, damage rules, subsidized ENet settlement, graphical
process, new supervisor or transport control plane was added. Default commands and
ordinary real headless peers remain owned by Runner/Child/ScenarioScope. Routine
network defaults are unchanged; `eliminated-resume` is an explicit selection and
joins the seven retained deep network cases.

| Boundary | Actual witness and limitation |
| --- | --- |
| Active fallen-owner squads/current-battle continuation | Strengthened existing SharedCombat production World.Step witness uses a paid two-owner roster, explicit arithmetic poses, a 33-HP member and 5-HP Keep. The destruction step preserves member IDs/HP/levels/service/formation and committed source/action/claims; compressed receiving reconstructs those claims. Fallen survivors actually land a later impact and retain earned service, then become nonparticipating reserves without next-wave claims. This controlled physical encounter is **not earned injury/balance or active-fighter ENet evidence**. |
| Simultaneous all-Keep defeat | Existing final-enemy/final-Keep witness now also receives through strict compression/compatibility: same-tick structure destruction/member death, AllCitiesFallen, no clear/reward and valid reconstructed claims. It remains deterministic A, not staged network simultaneity. |
| Real fallen spectator receiving/resume | `eliminated-resume` launches one playing host/two real ENet guests, seed1, two paid early waves plus next allocation, <=100 decisions/gate. The third owner buys a real member and Hall without a Farm; its stored member survives Keep loss off-grid. Three actual peers agree exactly on the paused members/HP/injuries/squads/actions/claims/events/config. Same-process peer reconnect and credential-file process restart restore the same fallen owner, with the accepted recruitment retry paid only once. **The ENet fallen survivor is stored, not an actively fighting unit; its initially full HP does not isolate wounded-Hall healing.** |
| Fallen city negatives/future continuity | Actual unpaused Building acknowledgments refuse readiness, construction, recruitment, retirement and research with unchanged complete gameplay, including duplicate refused requests. Living owners alone settle income/research and start W3; the fallen city's stocks/research/member HP/identity/equipment/status/service stay unchanged, with no new upkeep or claim. The fifteen-member initial roster contains fallen-origin enemies but no fallen destination. In-memory AuthoritySession/strict codec witness preserves the same independent boundary; it is not called real transport. |
| Malformed/legacy/ordering/payment | Real old-protocol admission and malformed requests refuse without mutation. Direct SnapshotPayload negatives retain retired fields, missing/malformed geometry, duplicate identity, compression and compatibility refusal. Existing authority/session/credential/retry/playback negatives remain, not replaced by a transport golden. Matching paused revisions and accepted Ready receipts are synchronization authorities; no sleep/retry/deadline/history or ordering-policy change. |
| Native receiving/resources | Existing minimal arena case delivers compressed snapshots through its delayed numerical adapter and exercises genuine native member controls, guarded reinforcement/regroup/retire callbacks, actual disabled Ready input, fallen explanation, pause privilege and independently editable living peer. Existing receiving case covers actual HUD/Worldtree/fonts/layout/resources after the same codec change. Neither adapter is real network or a complete application/world/UI journey; no rendered pixels, GPU, choreography or graphical menu claim. |
| Host end/loss and custody | New scenario checks orderly host end, spectator request disablement and headless menu event. Existing playing-host-lifecycle remains the real abrupt owned-host termination/guest return-path guard. Child now records actual ProcessId/HasExited/ExitCode/expected-failure/group ownership after its existing waits; admission, shutdown ordering/deadlines and runtime disposal are unchanged. Harness checks the terminal receipt and preservation of an unrelated process. |

Retained receipts under `logs/shared-arena-task/transport-batch/`:
- `probe*.txt` / corresponding TRX: finite paid W1–W3 witness selection did **not**
  find an active fallen-owner survivor. Diagnostic passes did not establish that
  behavior; the probe was replaced coherently, not shipped as a no-assertion test.
  No campaign calibration or rule tuning followed those observations.
- `build.txt`: actual PositionReservation API mismatch, xUnit collection analyzer
  and formatting; corrected without suppression. `authority.txt`: **1 passed /
  2 failed / 3**; real GoldRecovery requires zero wood and ordinary death cleanup
  precedes next combat. Paid construction order/cleanup barrier corrected, not
  production prices/readiness. `authority-02.txt`: **3/3, 5.10s**.
- `engine-arena.txt`, `logs/20261013-092912-0ebf1c99`: first component failure was
  deliberately emitting a disabled Ready signal, which bypasses native input.
  Readiness now uses genuine disabled-button input after closing the modal;
  guarded investment callbacks retain explicit signal negatives. `build-05.txt`
  retained a wrong UiDialog API name; actual GetOkButton used. `engine-arena-02.txt`,
  `logs/20261013-093340-392b62d8`: living peer was still Ready from the earlier
  native witness, so its build correctly refused. Explicit accepted Unready now
  preserves that independent guard. No gameplay fix or deadline increase.
- `coherent-fast.txt`, `logs/20261013-093706-12ccd6d2`: affected A **94 core +
  104 runner passed, zero failures/skips, 21.52s**. Includes the new paid spectator,
  active/last-Keep authority, session/codec/claims/playback, individual recovery/
  roster/payment and child/credential/selection guards. This is **not full-fast**.
- `network.txt`, `logs/20261013-092707-9d533f6a`: real selected eliminated-resume
  **18.31s / 23.34s including preparation**, no transport failure. Retained paused
  tick1566/revision1647/W2 shows owner3/member5 (4,000 HP hundredths), plus living
  wounded members as low as400 hundredths and five moving friendly squads;
  full source/action/claims/events/config are in the bounded child state receipts.
- `host-lifecycle.txt`, `logs/20261013-093621-2de5251b`: existing genuine playing-host
  lifecycle **12.59s / 17.69s including preparation**, including abrupt owned
  termination, expired credentials, post-loss command disablement and headless return.
- `engine-arena-03.txt`, `logs/20261013-093441-52d7ac3b`: corrected minimal component
  **3.33s / 8.40s including preparation**. `engine-receiving.txt`,
  `logs/20261013-093450-eec39557`: existing receiving component **24.07s / 29.16s**.
- After adding terminal PID evidence, `terminal-runner.txt`,
  `logs/20261013-094228-7ccc63b3`: affected runner **104/104, 2.55s**, overlapping
  earlier selection (not summed). `network-terminal.txt`,
  `logs/20261013-094231-e5f9b708`: same selected real ENet **21.59s / 26.68s**;
  `engine-terminal.txt`, `logs/20261013-094258-c6238443`: minimal arena **3.43s /
  8.69s**. Repeated only to qualify changed owned terminal receipts, not balance.
  Recorded owned PIDs exited and were absent afterward, runtime directories were
  removed; old-protocol child exit1 is explicitly expected, ordinary peers/components
  exit0. Full locked solution builds passed zero warnings/errors; exact diff,
  formatting and whitespace checks cover the checkpoint.
- Final source review caches the owned PID at spawn because error evidence can be
  requested after Process disposal. Harness now checks the original unexpected
  engine-error message and attributable terminal receipt, not just an exception
  type. `review-runner.txt`, `logs/20261013-094918-06193f4b`: affected runner
  **104/104, 2.81s**; final full solution build **0 warnings/errors, 7.20s**.
  Already-passed ENet/native gameplay inputs were unchanged and not repeated for
  this post-disposal diagnostic-only correction. `custody-review-02.txt` verifies
  recorded owned PIDs/runtime absence using absolute runtime paths only.

These overlapping selections do not establish complete networking/reconnect,
active fallen-fighter ENet capture, all-seed balance, full-fast/deep/stress/package
qualification or final-head CI. Native numerical-primary aiming/melee/animation/
resource seams and the actually landed phase/speed/preview prerequisite remain
independent work. Final paid calibration, including ranged viability/cooperative
Keep danger, awaits that prerequisite. No model/session/tool/dependency/asset/global
change, full pipeline, push, PR or publication occurred.

## Controlled active-field ENet and native presentation (2026-10-13)

Instruction014 closes a specific active fallen-fighter receiving gap, not complete
network/reconnect, earned balance or graphical acceptance. The independent
`active-fallen` selector joins eight retained deep network cases; the three
ordinary defaults stay unchanged. Its fixed seed1 production playing-host/ENet/
session/codec encounter has two identified six-member field rosters and explicitly
synthetic tower materials, one enemy, physical poses, a 33-HP member and 5-HP Keep.
The paired disconfirming control changes only that Keep to 100 HP. No seed search,
new simulator/supervisor/general loader/RPC mutation or phase implementation.

| Boundary | Executed proof / preserved limitation |
| --- | --- |
| Active fallen fighters, not stored substitutes | Actual peer sees a nonempty current field roster after Keep loss, exact authoritative member/HP/squad/source/action/claim/event/config agreement at accepted pause, new-peer and credential-file process resume on the original host, and a later landed field impact with nonempty victims. A missed live window fails. Keep-health control remains alive at its received impact. Controlled inputs are not earned injury/economy/balance. |
| Ownership/future/host negatives | Owned guest actually refuses the named fixture before peer/session creation. Fallen owner weapons stop (same-tick effects remain legal); actual unpaused city commands/refused retries preserve full gameplay. Living owner alone settles and advances, fallen fighters become next-wave Reserve with no claims. Original-host end disables requests. Existing paid spectator ledger, malformed/legacy codec, same-tick defeat, Watchpost disruption and real abrupt-host cases remain independent, not inferred from this encounter. |
| Numerical primary observation | Fixed 1-v-3 production encounter first reproduced representative member2 instead of actual seeded primary3. `StrikeAim` now records squad-owned intent/realized primary through existing TargetId/FrozenAim fields; death/recovery cannot visually retarget. Same X/Y selector/key, profiles, damage, rules7/protocol14 and full configuration identity stay unchanged. This observation is not member AI or damage input. |
| Native source/primary/arrival/projectile | Strengthened existing minimal `animation` case uses genuine imported rigs/AnimationTree/bones/socket and native projectile meshes. Zero-X/five-member uncapped Y-only impacts identify the actual primary/secondary victims. Launch predicts only already committed movement, actual impacts use pre-damage pose plus member formation; no future-move/hit guarantee or pixel/choreography claim. |
| Native individual health/status/death/resources | Real health control preserves hundredths/max/fraction/input transparency; individual Chill versus healthy squadmates and formation offsets remain distinct. A real other-squad kill produces a committed miss, not a forged cue. Corpse physical pose freezes while actual imported death skeleton advances on common playback clock, health/status hide, logical expiry and actual node release complete. Stock resources instantiate from unchanged immutable exports; accepted quote/codec updates coarse props at real band crossing, never optimistically or within-band needlessly. Existing receiving case covers exact resource labels/signals and native HUD/Worldtree. |
| Payload/custody | First broad selection caught 4,112,123 JSON characters against retained 4,000,000 bound. Unused live-target FrozenAim repetition was removed, not the bound enlarged. Existing ordered Child/ScenarioScope waits, PID/exit/runtime receipts remain; no new reader, shutdown/admission/deadline or history policy. |

Retained evidence root: `logs/shared-arena-task/presentation-batch/`.
- `encounter.txt`: **1 passed / 1 failed**, querying a now-expired historical
  destruction event at later impact. Capture the actual destruction barrier,
  not wider history; `encounter-02.txt`: **2/2, 2.69s including runner**.
- `primary-before.txt`: actual primary3 / representative2; `primary-after.txt`:
  three overlapping focused preflights passed **2.81s**. `native-before.txt`:
  fixture incorrectly expected a percentage in compact Chill badge; corrected
  actual per-member badge/state, not production UI. `native-before-02.txt` catches
  stale/group-center launch aim. `native-aim-after.txt` then catches frozen death
  skeleton. One-line UnitView clock distinction fixes it; measured pose0 to
  0.6458333134651184 at tick52 while physical (3,0,93.753845) stays identical,
  death interval28..76. `native-death-after.txt` retained a coarse-stockpile test
  assumption: six/four wood are intentionally the same band. Actual three paid
  Farm quotes cross wood6 to0; no resource tuning.
- `coherent-fast.txt`, `logs/20261013-110359-953d4856`: **332 core passed / 1
  failed / 333**, 51s; payload budget above fixed causally. Runner did **not**
  complete: legacy MeleeVisualProofTests used old one-sector board/city-filtered
  milestone/old exact windows and CombatProgressionProofTests used anchor3;
  cancellation retained alongside two unexecuted Windows platform rows. These
  graphical-validator input/production seams need coherent replacement, not
  assertion-by-assertion tuning. No full-fast pass is claimed.
- `bounded-history-02.txt`: budget/primary selection **2/2, 3.00s** after narrow
  payload correction. `affected-final.txt`, `logs/20261013-111522-c6d53920`:
  **261 core + 104 runner passed, zero failures/skips, 39.80s**, exact filter
  retained. Overlaps are not summed; this is partial affected-fast coverage.
- `network.txt`, `logs/20261013-102101-45a7baff`: original real controlled
  witness/control **22.70s / 28.08s including preparation**. After primary/payload
  changes plus guest gate negative, `network-final.txt`,
  `logs/20261013-111638-8e05d8f1`: **21.22s / 26.31s**. Expected guest exit1,
  ordinary peers exit0; actual same-host active-field receiving/impact, not a
  component adapter or complete end-user acceptance.
- `native-final.txt`, `logs/20261013-111432-84a250d3`: strengthened minimal
  animation **1.85s / 9.87s**. `arena-final.txt`,
  `logs/20261013-111954-fbe47fb1`: **3.43s / 8.60s**;
  `receiving-final.txt`, `logs/20261013-112004-1c442e15`: **24.06s / 29.35s**.
  Serialized locked preparation; no graphical/world/application acceptance.
- Final review computes each intent primary once per contributor rather than
  again inside a member-search predicate; numerical selection is unchanged.
  `review-last.txt`: **4 overlapping focused rows passed, 4.33s**;
  `build-last.txt`: **0 warnings/errors, 18.00s**. Custody checks verify recorded
  owned PIDs/runtime paths absent; immutable manifest check covers57 files with
  zero SHA/byte mismatches. Formatting/source/whitespace review passed.

Instruction015 subsequently reports battle-flow PR24 actually landed at
`a557e3f811ea294d348c6d5bc412a7b621d40149`, MAIN78477f8. This checkpoint still
qualifies the pre-integration head. Independently verify/merge that source next,
explicitly reconcile the two protocol14 schemas/identities/refusals, and rerun
actual final no-fourth-spending paid calibration (including ranged defeats and
cooperative Keep risks). Full-fast/deep/stress/packages/final-head CI and rendered
art/GPU remain unfinished; no first task done, owning pipeline or publication.

## Preservation-first battle-flow reconstruction coverage (2026-10-13)

Historical PR24 input coverage below is not qualification of this integrated
shared-squad head. Its individual-combat transcripts map to the coherent paid
squad replacements above; retired admission/size-budget contracts stay retired.

The obsolete six independent phase/tick/ID goldens in the paid growth transcript
are replaced by one six-mode same-seed Tier A comparison. It asserts actual
quote/payment/production receipts, empty-capacity non-equipment, early versus
late investment, identical player-1 pre-intervention controls, both-city paid
expansion, equipped strength, damage and survivors. It preserves the single-mine,
four-paid-unit opening; no product balances or enemy IDs are substituted. The
removed experimental Research Tower/Market chain maps to named independent
research/trade/payment/capacity/recovery/production/atomic tests and a new default-
rule paid joint schedule in the [coverage checkpoint](battle-flow-reconstruction-coverage.md).
Three new paid schedule cases and four complete-sale boundary cases use direct
C# rules/commands, not another application journey. The ordinary army opening
now proves four paid recruits with actual unused physical capacity; separate
paid six-unit/capacity and three-city transfer guards remain.

The [reconstruction coverage checkpoint](battle-flow-reconstruction-coverage.md)
owns the paid/occupied witness split, negative-coverage inventory and opt-in
redistribution consumer migration, including their exact failed/passed receipts
and unexecuted native qualification. Those historical receipts are not current
pipeline-head proof. Immutable MAIN resource-ledger bytes/provenance are retained;
the native C# receiving caller supplies its supported heading field.

## Integrated battle-flow/shared-squad paid reconstruction (2026-10-13)

Protocol15 explicitly supersedes **both** incompatible predecessor14 schemas;
combat rules7 and battle-flow2 remain explicit. Full compatibility identity now
includes battle-flow version without reusing it as the base actor decision salt.
Receiving validates configured forecast profiles and the required forecast array,
plus the existing strict squad/member/geometry/retired-field negatives. Historical
protocol14 receipts above are historical checkpoints, not integrated acceptance.

`PaidPreparationScheduleTests` reconstructs the **joint** default-rule paid
research/veteran/Market/trainer/exclusive-tower/field-capacity state, not a union
of independent passes. Fixed retained seed14056307608042553509, two actual clears,
exactly nine END settlements, two owners. Actual quotes/payments/refunds,
production/upkeep/research and all before/after member IDs/HP/profiles/levels/XP
are recorded. W3 final Preparation has wounded researched veterans and a genuine
Market in owner1, paid replacement L2 trainer/equipment and a numbered army
Rapid tower in owner2, and nine individually paid field members/three non-tower
capacity3 homes each. Final END immediately enters Combat. This is **entry-state
conjunction**, not W3 victory, a twenty-wave baseline or native/transport proof.

Preserved first whole schedule: its later W3 six/seven-member field loses owner1
Keep and **all thirteen fighters**, while owner2 alone clears. That failed
all-Keeps/veteran-preservation outcome is not relabeled success. The complete
replacement rearranges actual paid plots/refunds and proves the required
conjunction before that extra battle, not through grants, seed selection,
pre-spending final income, extra phase or rule/price/wave compensation. It preserves
research health/equipment/XP and foreign-owner nonmutation, full-sale forecast
parity, acquired research after producer sale, actual trainer/tower generations
and investment, duplicate/exclusive/stock/capacity atomic refusals. A Farm upgrade
cannot spend final income early, and is still refused in Combat once affordable.
The independent single-mine specialist control and actual Hall/healing/track
conjunction remain in the same class; no extra mine was added to the single-mine
experiment.

Exact source receipts: `logs/shared-arena-task/battle-integration/` holds both
pre-execution whole-schedule plans, failed source/ledger and successful complete
ledger. First whole attempt `20261013-134309-8d335067`: one failure, 8s. Replacement
`20261013-135207-27555451`: all **3 paid schedule tests pass**, 10.71s runner;
locked build zero warnings/errors, 4.38s. Later expanded affected filter
`20261013-135653-4f80839e`: **106 core passes / 4 failures of110**, plus **17 runner
passes**, 47.89s. These overlapping counts are not summed. Remaining failures
are the separately earned Hall timing, old reserve W5 slot6 deadline, a seed1
empty-land fixture assuming two surviving squads, and the stored spectator's
underfunded three-phase support opening ending in Defeat. They remain causal
failures requiring coherent migration, not waived acceptance. No integrated
checkpoint, full-fast, native/ENet/protocol15 delivery, final paid matrix, package
or final-head CI qualification is claimed by these selected passes.

### Earned-state fixture batch 018: partial, unresolved support witness

The failed early Hall purchase and arbitrary reserve-W5 slot6 expectation are
consolidated into the same **paid** Hall witness: actual second END before
construction, pre-affordability atomic refusal, wounded service members,
production-only healing, send/re-store with identical healed HP/equipment/XP,
base Hall6 overflow refusal, separately paid capacity2 upgrade (healing2
unchanged) accepting the **same** seventh member into individual slot6. Final
END funds seven stored individuals and immediately enters W3 Combat; all remain
off-grid/nonparticipating/without claims through strict receiving. The earlier
wave's field6/stored2 funding and legal send/retire/recruit negatives remain.
No W3 battle victory claimed. This replaces the invalid first batch assumption
that six field members would survive W2; actual survivors were three. It does
not generate replacement HP or impose an arbitrary campaign deadline.

Exclusive-land test retains both declared seeds, unchanged no-heal/no-member
land-purchase checks, then **explicitly pays** recruitment for an observed second
owned squad before the successful first move/rejected conflicting second move.
Capacity3 supplies the bounded source requirement, not guessed squad IDs or
removal of exclusivity because only one squad survived.

First batch `20261013-143628-fb239f1c`: **5 pass/2 fail of7**, 38.56s. Land seeds,
paid-home negative, joint schedule and independent single-mine specialist pass;
Hall setup and stored-spectator elimination fail. Actual-count Hall correction
`20261013-145333-9a90c79e`: **1 targeted pass**, 6.31s; locked build0warnings/errors,
4.28s. Counts overlap and are not a full batch/full-fast result.

Stored receiving input now distinguishes an **earned veteran** from the former
no-Farm, unfunded XP0 recruit. Whole source-defined schedule before replay:
owner3 actually funds four W1 field members, earns real first clear/service,
sells Farm at W2, earns the missing Stone through actual END, buys Hall and
stores all actual surviving veterans. Supporting owners use six paid L1 at W1,
then seven mixed L1/L2 members at W2 through paid producer/refund/trainer/home
commands. Existing paid receipt owner checks quotes/production/equipment against
real AuthoritySession transitions; no alternative simulator or invented money.

**Precise remaining setup contradiction:** this complete finite two-wave input
**rescues** the third Keep instead of eliminating it. Actual W3 Building,
tick1274, six productions, Keeps10000/10000/8600, all cities living. Owner3's
actual stored IDs13/16 have HP1400/2400, L1/XP1; owner1 retains IDs1/6/29 with
HP1000/2000/2000, owner2 has no remaining troops. All recorded 66 commands/outcomes
are paid/legal; the required continuing **eliminated-owner** state never occurs.
The previous weaker support terminated Defeat; neither is the desired witness.
This proves a limitation of these fixed inputs/horizon, **not** a general
impossibility of default paid cooperation. No terminal Defeat or rescued Keep
is renamed elimination. The full before/after ledger and exact outcome summary
are retained under `logs/shared-arena-task/battle-integration/earned-018/`.
No further guessed building/seed/stat/price changes, repeated suffix probes,
transport launch or final receiving acceptance is reported. Existing active
fallen-field proof remains separate. Stored-spectator receiving/production/
future allocation/resume guards remain unsatisfied at these merged inputs;
C must consume a genuinely qualified earned A input, not launder this failure.

Integration is therefore still uncommitted. Native/ENet/protocol15 delivery,
final paid matrix/full-fast/deep/stress/package and owning pipeline remain
unexecuted at the final integrated head.

### Fixed-prefix spectator observation horizon 019: no qualified boundary

One pre-written continuation extends the **same** 66-entry seed1/default-rule
prefix through exactly W3's three ENDs, **nine Ready commands only**, no new
purchases/recruitment/stat/profile/phase changes. All 66 prefix entries match the
preserved ledger in every field except four fresh per-authority `MatchId` values
in the two existing full before/after combat-outcome snapshots; only those IDs
were normalized for comparison. Cleanup ends at tick1322. W3 entry has production9,
Keeps10000/10000/8600 and the actual served stored IDs13/16 at HP2000/3000, XP1.
That +600HP each comes from the three genuinely funded preparation settlements.

Per-authority-tick observation records **705 states**, tick1322 through2026:
- Owner1 falls at tick1995 while Combat continues, other Keeps100/1100. Owner1
  has **no stored member**; this does not satisfy the spectator guard.
- Owners2 and3 both fall at tick2026, immediately entering **Defeat /
  AllCitiesFallen**. Owner3's stored IDs13/16 persist at exact HP2000/3000, XP1
  in every recorded tick, but their owner never falls with another living Keep
  and a still-Combat match.

Thus extending this fixed input's horizon **does not qualify the conjunction**.
It is not evidence of general campaign impossibility or a receiving defect.
No further horizon/recipe/seed trial, manufactured damage or end-state relabeling
was used. Own-command/retry/codec/pause/resume/future-production/allocation guards
remain strict but **unreached** in this input; no ENet proof launched or checkpoint
committed. Existing active-fighter evidence remains separate, not a substitute.

First observation `20261013-151119-5144b0f9`: one failure, 19.92s runner. Diagnosis
found boxed `ValueTuple` observations serialized to `{}` under production JSON
options. Explicit named member projections replaced that vacuous observation;
no numerical/paid input changed. Corrected actual member receipt
`20261013-151727-591aad18`: same one failure, 20.16s; locked build0warnings/errors,
4.29s. This repeat corrected relevant evidence input, not a blind retry. Raw
ordered events, full entry/final states, all explicit per-tick member observations,
old source and normalized-prefix/source/binary hashes are in
`logs/shared-arena-task/battle-integration/horizon-019/`. Exact unavailable
conjunction requires a mapped coverage disposition; it cannot be suppressed
into a green integration result.

### Historical mapped spectator coverage 020: native boundary then blocked

The authorized mapping is written before consumer retirement in
`logs/shared-arena-task/battle-integration/mapped-020/coverage-plan.md`.
The original strict paid consumer, the exact66-prefix/nineReady/705-state
failure, explicit member/event observations and their corrections remain
preserved. **At the 020 checkpoint**, the strict consumer was still present
and failed its unavailable conjunction. It was not renamed into spectator
acceptance. The 021 supersession below records its later conditional retirement.
Default-paid combined full-session, campaign balance and twenty-wave acceptance
remain unqualified.

The existing fixed authority/owned headless loopback receiving seam now admits
`fallen-stored` and `protected-stored` at W3 Combat only, after actual paid
service/storage. No resources, members, equipment or experience are granted.
Keeping one original configured enemy and positioning genuine squads, plus
Keep health5 versus100, are declared **controlled enemy-roster/pose/Keep inputs**,
not earned injury/economy/balance evidence. Ordinary combat must perform the
Keep destruction. The single complete fallen state must contain the same served
stored veterans, another living Keep and ongoing Combat. Active fallen-field
actual-impact coverage and simultaneous last-Keep Defeat remain distinct.

Selected numerical qualification covers real destruction, strict codec/member
identity, stored/nonparticipating/no claims, stopped owner weapons, pause,
command denial/retry/nonmutation, foreign ownership refusal, original purchase
receipt, disconnect/rebind/resume, team clear and next-wave living-only
production/upkeep/allocation. Protected input passed its separate no-fall
control. These are selective executions, not one complete current-source green
batch. Fallen input `20261013-155344-9fd81c5f` passed1/1 in12.04s; additional
mid-prefix config/codec observations around tick991..994 and the same full
fallen guards passed1/1 in12.57s (`20261013-161256-f86b492d`). Builds had
zero warnings/errors.

Causal failures are retained: an initial ranged enemy correctly struck an
in-range Watchpost instead of the intended Keep. Geometry was corrected using
actual configured range and distance, excluding Watchpost range; no target/hit
flags, stat change or extended observation window. A raw future foreign command
sequence poisoned the test-owned sequence counter; the negative now uses the
ordinary owned sequence with foreign City. The numerical conjunction and later
guards subsequently passed.

**At 020, actual ENet was not qualified.** The new `eliminated-resume` driver uses the real
paid prefix and native authority, plus fallen/protected stored cases, exact
paused receiving, malformed/protocol14 refusal, authentic retries, peer/process
credential restoration, owner exclusions and host shutdown. It currently fails
**before controlled W3 input**, waiting for the ally to receive a paid clear:

- `20261013-155520-3d6d5aff`:300.54s phase/305.68s runner, cancelled at the
  unchanged300s suite budget. Host/owner received W3Building tick1322, while
  ally's last received state was W2Combat tick991/revision1065.
- An owned acceleration1 versus4 admission/CPU discriminant first exposed an
  actual startup fixture race: Lobby and first Building share TurnSerial1.
  The barrier now requires current MatchId/Phase/Wave/TurnSerial. A launch using
  the already-loaded old driver remained an old-input failure; preparation
  inside that process cannot replace its loaded driver assembly. Both receipts
  remain, and the next launch used an explicitly completed build and hash.
- `20261013-162448-bcc3ec9f`, completed current driver/owned acceleration1:
 300.85s phase/305.90s runner, still cancelled awaiting a paid clear. Host/owner
  received W2Building tick361; ally stopped at W1Combat tick218/revision265.
  Therefore a simple acceleration reduction did not establish receiving. This
  does not diagnose a product defect or prove an identical deterministic tick
  failure; producer/receiver/native delivery cause remains unresolved.

All three native children in each attempt were awaited and exited0; recorded
PIDs and owned runtime directories are absent, and engine logs contain no
reported error. Exact source/binary, receipt, process and boundary evidence is
in `mapped-020/{driver-receipt,current-source-binary,custody-and-receiving-boundary}.txt`
and the immutable run directories. No deadline increase, blind replay, new paid
recipe, old-consumer retirement, integration commit, ENet acceptance or broader
qualification follows this unresolved native receiving boundary.

### 021 supersession: strict native stored conjunction qualified causally

The old native blocker is resolved, not reclassified as a passing observer.
A single bounded45s probe through existing timing/event owners correlated host
publication/send, ENet receive, strict decode/admission/apply and observer emit.
The existing independent native `checkpoint` falsified a stdout-only-loss theory:
actual native state and its observer both lagged. Both receivers kept servicing
and progressing; no admission refusal or trace overflow occurred.

Strict validation repeatedly rebuilt immutable four-owner all-pairs topology
while recomputing full combat configuration identity, twice/snapshot. Baseline
`20261013-170039-6432f11f` measured median decode83–84ms and apply74–75ms,
publish-to-RPC6.55–6.64s, maximum15.90s. Four bounded lazy immutable `HexBoard`
instances now reuse constant topology by roster width. Owner IDs, footprints,
DTOs and claims remain private; rules/full fingerprints/forecasts/payload bounds
and strict validation are unchanged. Four structural regression rows failed red
before correction; the affected89-row batch passed11.12s. The regression asserts
same-width immutable graph identity and independent mutable projections/owners,
not a fragile wall-time threshold.

Matched same45s/input diagnostic `20261013-171410-2eb79ed1` reached both clears
in19.72s: decode6.65–6.79ms/apply4.20–4.37ms and publish-to-RPC9.81–10.79ms
median. Its intentional stop is **diagnostic only**, never acceptance. Temporary
tracing/checkpoints/stops were removed from the driver before strict qualification.

- Actual `eliminated-resume`, `20261013-171834-39998b68`: **PASS**,
  76.95s scenario/82.09s runner, fallen/protected stored inputs and the complete
  conjunction plus identity/claims/denial/retry/foreign-owner/legacy14/malformed/
  credentials/peer restart/future-allocation/host-loss guards. Original300s budget.
- Separate actual-impact `active-fallen`, `20261013-173432-5c67f17b`: **PASS**,
  31.82s scenario/37.59s runner. Stored nonparticipants cannot substitute for this.
- Minimal native `arena`, `20261013-173510-4068a9ec`: **PASS**,2.70s/8.12s.
- Minimal native `receiving`, `20261013-173518-d93759af`: **PASS**,23.63s/29.13s,
  current three-stage settlements/Stonecutter/forecast/preview inspection.

All owned commands returned, children were awaited and diagnostic PID/runtime
absence receipts retained. Source/binary and correlated evidence live in
`logs/shared-arena-task/battle-integration/diagnosis-021/`. Native results are
selected controlled/component qualification, not full default-paid networking,
twenty-wave balance or rendered acceptance.

Only after complete mapped numerical A and actual C qualification was the old
strict consumer retired. Its original source and exact66-prefix/nineReady/
705-state failure remain. `PaidStoredVeteranHistoryTests` retains that same earned
history, payment/service/storage/healing/member identity/codec/nonparticipation/
claim proof and reports the absent conjunction accurately. Complete spectator
guards live in the actual controlled authority/ENet tests. Default-paid combined
eliminated-stored acceptance is still **not** established.

### 022 runner migration and remaining integrated qualification

The old city-local board caused missing shared cell73; obsolete city Destination
filters hid foreign rescue and old member anchors simulated stacking. The runner
now inspects shared `Arena.Board`, squad membership and all-owner rendered
clearance/cue positions. Shared population counts exclude retained deaths; dead
poses still support local clearance/near-far inspection. Empty living target
population previously caused `Min()` failure; retained cosmetic poses do not
become live occupancy. These red controls remain in receipts.

`SharedCombatProjection` is the pure presentation geometry owner used by native
`CombatLayout` and runner observers: centered connected hexes, formation slots,
boss zero offset, committed interpolation and frozen corpse movement. It is not
another simulator. Analytical two-sector geometry separately falsifies wrong
centering/city offsets/formation arithmetic. Existing constants are unchanged.
No Godot or full application is needed for these deterministic detector controls.

`MeleeVisualProofTests` consolidates obsolete301/638/973-tick windows into finite
4000-tick paid W1 controls (seeds0/1/123; one configured ordinary wave), retaining
real six-member paid openings, linked near/far/shared/simultaneous windup/impact,
cue/action/weapon/malformed negatives, current-state pause admission, capture
bounds/order, stale turn/session/terminal refusals and frozen playback. Metadata
positions/bones/captures are explicitly **not imported rig or PNG proof**. Actual
miss/death/commitment mechanics remain in core/native coverage; these detector
fixtures do not fabricate impacts to claim native acceptance.

Targeted runner `20261013-200406-b4cc77dc`: **19/19 PASS**,0.984s binary/2.12s
runner. First attempt retained4 failures (empty corpse-group geometry and cross-
owner clearance), corrected through the existing owners. Broader fast
`20261013-200535-121f2855` remains **FAILED/PARTIAL**: runner645pass/2fail/
2Windows skips; core cancelled with an obsolete EconomyCatalog Used8-versus-
four-actual-members assertion. That assertion now verifies two real squads,
home usage3+1 and individual member accounting; EconomyCatalog9/9 passed.

BurnAdmission's bounded evidence first found W2 Preparation tick2563 with one
fallen unready owner and one living ready owner. The test driver—not production
readiness—failed to advance real death cleanup because it counted the fallen
owner. Correcting its living-ready predicate exposed actual W2 AllCitiesFallen
at tick3824, before Mage/foundation/Fire, which remains preserved. A whole paid
opening reconstruction was planned before replay: canonical two-MetalMine
six-member W1 funding; ordinary second-mine sale in W2; then the existing quote-
driven research/producer/upgrades/Mage-first policy. No new seed/horizon/rule/
price/grant/gate or waived burn assertion. Both modes now earn actual W8 Combat
burn at tick2165/production24 with both Keeps10000; source119 burns members137/
138 through real impacts.156 accepted before/after command/resource/service
records per mode are retained. Strict pause/stale-turn/receipt/frozen-status
negatives passed in the **36/36** BurnAdmission batch,6.48s runner,
`20261013-202344-5168891c`. This is earned finite research admission, not20wave
research transport/rendered acceptance.

Next complete fast attempt `20261013-202404-ba4a567c` ran both binaries: runner
**647PASS/2Windows skips**, core557PASS/1obsolete fourth-spending-window failure.
That obsolete witness was replaced coherently with real four-member recruitment
before END3, refusal to prespend future metal, exact production+food accounting,
unchanged identities and atomic stale/fourth-gate refusals. Subsequent complete
core `20261013-202840-e512e72a`: **558/558PASS**,46s binary/47.56s runner.
SHA256 checks confirm Core/DevRunner/DevRunner.Tests binaries unchanged across
that last test-only build, so the completed647-row runner receipt remains valid.
Thus current-input fast coverage is complete across two receipts; the earlier
combined commands remain failures, and the two Windows rows remain unexecuted.

The shared projection extraction justified a focused native arena recheck—not
an unchanged repeat: `20261013-201925-d17d944f` **PASS**,2.58s/7.65s. No repeat
of already-passed stored/active ENet or receiving/animation follows automatically.

The current unchanged-policy twenty-wave matrix is **13 verification passes /
1 failure**, but **10 victories / 4 W1 defeats**. Cooperative seed123 lacks any
foreign-origin impact; both cooperative policies and both ranged policies lose.
See [paid baselines](paid-squad-baselines.md#historical-initial-three-preparation-matrix-before-recovery).
Further earned policy/balance investigation, complete fast/deep/stress/package
qualification, integration checkpoint and publication remain. No paid policy,
seed, price, stat, spending gate or acceptance assertion was changed to hide it.

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


## Shared arena recovery: current-source checkpoint (2026-10-13)

Recovery starts at `85ebbcc` with no unresolved merges. Installed SDK10.0.401
and Godot4.7.2 .NET match the locks. `mise run build` completed its locked
solution restore/build with zero warnings/errors. These receipts supersede no
historical failures and do not establish complete-head acceptance.

The selected current-source fast filter covers Shared*, SquadReservation,
CombatConfiguration, CombatReceiving, BattleFood, PaidPreparation, ArmyRoster,
ArmyRecovery, ArmyCommand and ArmyProgression: 129/129 passed, no skips
(`logs/20261013-214226-492074d9`). The trace-enabled retained cooperative method
ran seeds1/123: one accounting pass and one failed foreign-impact guard
(`logs/20261013-214201-57c20f07`). Both outcomes are genuine W1 AllCitiesFallen
defeats, not campaign wins. Raw numerical transcripts are retained under
`logs/shared-arena-recovery/before/`; all twelve entry enemy IDs are unique,
event sequences strictly increase with nondecreasing ticks, and independent
entry-origin attribution agrees with six/zero foreign impacts. There are
532/519 ordered events. Each investing owner paid/fed only four members; the
first deaths occur at133/164ticks. No extra history/deadline was introduced.

The first unresolved causal boundary is the paid opening: infrastructure
preceded adequate combat strength. No current routing defect has yet been
established. Native styling, full paid ranged progression, earned four-owner
rescue, broad fast/network/package/graphical acceptance remain unexecuted at
this checkpoint. Historical 558/647 receipts predate the tracing edit and are
not substitutes for this qualification.


### Recovery coverage admission and outcomes

Strengthened existing full-force `SharedCombatTests` with two Crossbow seeds:
arithmetic paid equipment/food, default W1/profiles and exact six-member/two-group
setup. Underfunded paid diagnostics cannot isolate full-force strength; TierA,
<6000steps, no engine/children, measured in the46-row4.5s selection. Expanded
existing configuration identity theory with independent ranged HP/X changes;
full fingerprints and reconstructed received profiles must change together.
Pure paid opening acceptance was explicitly deferred by the owner after two
small candidates failed; raw defeats and payment ledger remain in baselines.

Added bounded TierDeep four-width paid support and four-owner rescue assertions
through existing PaidCampaignFixture, not a simulator or reader. Synthetic
actor/transport fixtures cannot prove paid progression or physical live-Keep
rescue. Retained seeds/100000step bounds, exact ordinary quotes/upkeep, adjacent
travel and event attribution catch empty witness/teleport/post-elimination claims.
The seven-row selection hit its180s budget and awaited owned hosts; it is failed
partial coverage (`logs/20261013-215033-0f149680`). Smaller selections, without
deadline changes, completed the four campaign rows and both retained rescue
guards; the short-horizon helper defect and corrected one-row rescue pass are
recorded in the paid baselines. No production routing repair was needed: funding
six members before infrastructure established the actual cross-sector witness.

Native arena extension (TierB, existing minimal scene, measured4.48s engine)
loads three pinned terrain meshes/materials, exact flat contacts/picking, bounded
44-prop maximum, all four widths and supported landscape/portrait/extreme pan,
static IDs across revisions/toggles, old-node disposal on four-to-solo replacement,
new/reinforce/regroup/store/return/retire callbacks and delayed acknowledgments.
`logs/20261013-215524-42288d50` passes. This replaces no arithmetic/network
coverage and establishes no rendered pixels/physical sound.


### Recovery integration defects retained

The first final fast run (`logs/20261013-220931-981c5a73`) passed647runner
checks/two native-Windows skips, but core passed555/562 with seven failures:
old Crossbow30HP constant; old four-member policy expectation; fragile single-
mine specialist clear; three consumers of an empty earned stored prefix; and
a Hall upkeep total assuming fixed casualties. These are failed evidence.
Funded solo controls independently passed12accounting rows with tenW20wins and
two deferred ranged defeats. The restored six-member policy inputs changed the
old capacity expectation, not physical rules. Hall allocation now asserts its
actual authoritative forecast/count while preserving exact seven stored members,
individual slot6, capacity refusal and independent upgrade/recovery guards.

Authorized default graphical combat first failed at native Berserker recruitment
(`logs/20261013-220019-9559b9b9`). Diagnosis found a retired size-budget helper
and, after repair, actual production field callers passing obsolete size1 as
the new Hall-slot argument, disabling valid recruit/new-squad/return controls.
The native component now checks enabled availability before emitting Pressed,
as well as accepted/denied mutation and delayed acknowledgment. All remaining
field callers use field defaults; actual Hall calls preserve named slot/generation.
The focused melee diagnostic (`logs/20261013-220529-f52ecdfd`) also exposed
the production layout retaining solo geometry when the same lobby expanded.
`CombatLayout.Synchronize` now refreshes accepted width and structure footprints;
the native arena consumer reuses one layout across1..4joins. A build failure
from initially placing that engine-dependent adapter test in the pure core
project was corrected by moving it to existing TierB; no Godot test dependency
was introduced. Mixed failed-build receipts are not current-head acceptance.

Rendered health verification now consumes actual native squad labels plus each
participant's ID/HP/max and retains per-member comparison, current lifecycle and
projection guards. No synthetic per-soldier bars are reported. Cosmetic death
overlap is intentionally allowed; the replaced cheap anchor check retains the
negative against overlapping settled living actors and distinct transit handling.
Individual native inspector/bar and imported rig/death/miss checks remain separate.


The earned stored-spectator prefix now uses the same paid six-member opening
for its third owner. W2 buys a Stonecutter before its two genuine production
gates; only when actual stone/wood plus Farm refund afford the Hall does it
sell/rebuild/store. This retains actual served IDs and exact food/recovery,
with no synthetic XP/injuries. Existing current/controlled stored consumers
passed in the15-row repair selection except the obsolete66-command count.
That count is replaced by the semantic six-settlement prefix and exactly nine
subsequent Ready commands; all before/after quote/identity assertions remain.
The independent single-mine specialist supply chain remains single-mine and
uses an ordinary paid second-owner defender, matching the cooperative game;
its true material/production/refund windows and specialist survivors are kept.
The updated native arena check passed (`logs/20261013-221453-8a0c7e91`),
including real enabled recruitment/new-group/Hall-return controls and same-lobby
width refresh. Disabled signal emission alone no longer certifies availability.


### Coherent numerical and transport receipts

`logs/20261013-221845-27d6d9f4`: broad fast562/562core and647/649runner,
two native-Windows argument-reader skips, no failures (57.39s). This supersedes
the intermediate fast failures above. Subsequent presentation-only fixes still
require their affected component/graphical/runner verification.
`logs/20261013-221748-66860a3f`: headless animation component1.79s, imported
rig/sockets/member summary/health/status/death/miss/stock behavior.
`logs/20261013-221746-2181c51b`: repaired earned stored-history test passes.

`logs/20261013-221937-7fdc3db8`: all three default network cases passed,
18.15s scenario time, serial admission. Intentional occupied-bind/expired-peer
negatives are expected refusals, not ignored engine faults.
`logs/20261013-222050-b2025919`: selected active-fallen passed29.41s.
`logs/20261013-222333-3ba6ffee`: selected eliminated-resume passed62.03s
with the changed paid service/storage prefix. Both retain real original-host
ENet identity/retry/paused/credential restoration and protocol14 refusal; the
active and stored controlled conjunctions remain separate from earned rescue.
All owned children/runtime cleanup remains in their runner receipts.

`logs/20261013-222050-test-scale-670651eb`: explicit Debug seed1 stress,
2048members,600normal ticks,668landed contributions/100deaths,458deployed and
1490queued at completion. Synthetic conservation/claim/event/resource-window
proof, not earned balance, rendered performance or a Release benchmark.

Authorized default combat progressed to actual `combat-catapult.png`
(`logs/20261013-222159-2bfa7f31`) after fixing paid supply: W2cloth supports
the Mage and a real W3mine rebuilt before END1 earns specialist metal. Its
checkpoint still failed because a hidden settlement countryside was evaluated
as the arena. The checker now observes the visible native arena's real ground
instances, bounded clear dressing and actual opaque camera footprint. The
retained frame also exposed a wrapped speed label inflating top chrome; it is
now a compact unwrapped native strip using existing woodland tokens. Native
receiving protects the compact height/width, and arena summary glyph size is
increased without changing cells/camera/timing. This intermediate PNG and the
headless receipts are not a passing rendered acceptance claim.


`logs/20261013-222740-6ce919ef`: receiving component passed23.50s, including
the compact native speed strip, Settings priority and existing economy/research/
accepted-only speed controls. `logs/20261013-222948-80156e98`: Linux package
qualification passed31.94s with already installed matching checksum-marked4.7.2
.NET templates; source/client/server managed/native/resource identities and
headless offline roles plus exported Worldtree component were checked. No
template/tool acquisition, tagged release, installer or graphical package claim.
Native Windows remains pending the owning PR's CI.

The authorized rendered run (`logs/20261013-222851-3499ac31`) retained actual
catapult/locomotion/inspection frames after native strip/ground coverage repair;
it still failed the old hidden Plot0 dismissal step. Arena inspection now uses
an ordinary outside world click, retaining cosmetic/no-duplicate command and
drag suppression, while settlement plot behavior stays separate. Active-view
camera reference observations use arena origin, not a hidden city. Short weapon
headings retain individual HP/max and reduce crowded overview text. These
intermediate frames and failures remain distinguishable from final visual proof.


The later rendered camera run (`logs/20261013-224244-a67a2b1b`) retained
resize, camera, pause, casualty and inspection frames, but selected wounded
enemy47 at its projected center and actually inspected the nearer48. Production
probe clickability now requires the same ray picker as normal input; the
consumer selects a truly clickable wounded target, retaining actual identity/HP
and live-inspection mutation checks rather than claiming an occluded point hits.
The current runner-only suite passes647 with the two native-Windows skips
(`logs/20261013-224244-4fad3d4e`).

A discovered inherited-preview regression hid every forecast under the arena-
only unit mask. Both ordinary update and reframing now use one pure view policy:
negative forecast IDs are shown only in their observed settlement, positive
field/death IDs only in the shared arena, and undeployed reserves nowhere. A
focused TierA test uses actual two-owner forecasts and an ordinarily produced/
paid member; no engine dependency or grants. Initial synthetic Create was
undeployed and correctly refused visibility; the test now uses normal commands.
This is preview identity/view routing, not pixels or paid campaign viability.


### Final authorized rendered acceptance and strict campaign controls

`logs/20261013-225736-51c0ad8c`: the selected graphical `combat` scenario
passed150.25s (owned private display/software OpenGL, normal paid preparation
and real peers). Actual retained catapult/locomotion/inspection/paused/camera/
resize/casualty PNGs and observation JSON demonstrate the styled arena, native
controls, independent member health, live inspector damage, sword/axe/cast/hit
cues, frozen deaths and cleanup. The scenario passed every pose predicate.
The owner authorized graphical checks explicitly; no desktop fallback was used.
These are two-owner all-sector rendered frames. Four-owner geometry/controls
remain headless native evidence, not four-owner pixel acceptance. Overview
labels still overlap in dense combat; close inspection remains the precise
member detail route. This is a bounded software-rendered sample, not universal
readability, GPU performance, every visual selector or graphical package proof.

`logs/20261013-230019-28c8f59d`: twelve selected deep paid solo rows passed
31.23s. Ten funded frontline/mixed/tower/research/reserve seed1/123 controls now
assert actual campaign victory (reserve also asserts served healing); two pure
Crossbow rows retain their genuine diagnostic losses. Accounting success alone
can no longer satisfy those victory controls. The final pure preview policy
selection passed24 rows (`logs/20261013-225510-715f05e6`).


Final coherent source checkpoint: `dotnet format Odot.slnx --no-restore`
completed cleanly; locked fast preparation built with zero warnings/errors.
`logs/20261013-230414-14aa7300`: broad fast563 core and647 runner passes,
51.67s; the two native Windows argument-reader tests are skipped on Linux.
`logs/20261013-230507-356b115f`: refreshed native arena component passed4.17s,
covering the final label/resource/preview-era source. Strict main/change OpenSpec
validation and whitespace checks pass. Linux package/stress receipts above are
boundary evidence; required exact-head Linux/Windows/source PR CI remains the
delivery gate. No release, installer, Steam pair or graphical package is claimed.


The owning no-mistakes run `01M4VR8BMAW9YK60SQY3RSSCGT` ended before
publication after three final review-report schema failures (invalid risk/test
metadata). Its two fix commits were recovered through guarded custody recovery,
not discarded: selector rejection, corpse-safe actor lifetime, production routing
coverage, valid partial-funding admission, nested-rule/claim/pose validation and
shared-wave deadline ownership. This is a failed pipeline, not delivery evidence.
Remaining compressed home/city-shape negatives are added at the same receiving
boundary; the current stored-spectator description now distinguishes the earned
veteran prefix from controlled elimination and actual ENet resume. A subsequent
owning run must still complete review/Test/docs/lint/publication and exact-head
required source/Linux/Windows CI. No test exception or release is authorized.


Guarded recovery follow-up: the first selected local run passed115/116
(`logs/20261014-010044-083a44ed`) and caught an overly strict assumption that
purchased homes form a prefix. The check now compares purchased totals, retaining
legal arbitrary purchases; an explicit paid nonsequential purchase codec regression
joins the existing deployment test. Final selected receiving/lifecycle/shared-state/
Crossbow/deadline suite passed117/117 in11.25s
(`logs/20261014-010135-babaa351`). The twelve new malformed city/home compressed
inputs refuse recoverably. Locked solution build has zero warnings/errors; changed
C# formatting, strict change validation and whitespace checks pass. These remain
local codec/domain receipts, not final PR CI or native credential/pixel proof.

Final recovered-source broad fast:641 core/647 runner passes with the same
two native-Windows skips on Linux;57.00s,
`logs/20261014-010227-5dbde6a3`. This supersedes the pre-pipeline fast count
for domain/codec coverage; exact published-head CI remains pending.

## Review follow-up: receiving projections and physical melee proof

R11 coverage extends the existing Tier A compressed receiving fixture: paid Hall
construction and both track upgrades, ordinary Barracks recruitment and food,
arbitrary nonsequential home purchase, and ordinary combat frames are positive
controls. Null/missing/duplicate catalogs and quotes, unbounded or mismatched Hall
tracks/identities/generations/quotes, malformed food/receipt arrays, and injected
preview target/pose/action data must refuse before receiving assignment. Earlier
city-shape tests did not exercise these native-consumed values. Tests use bounded
configured resources and real commands, without sockets, credentials or native
controls; setup/maintenance cost is estimated below one second per case.

R12 preserves numerical linkage/group/near-far/simultaneous and malformed-cue
controls, but removes fabricated dashed intent from their projected observations.
The supported proof now requires the actual attached/aligned weapon, imported
attack clip/blend/socket-bone observation and authoritative timing during physical
windup; only landed impacts may show target-side contact. The existing Tier B
animation selection emits genuine production UnitView/MeleeStrike observations
from two fixed adjacent two-member groups; DevRunner feeds them through both
production detectors for windup and impact. This minimal headless component uses
owned processes/preferences and existing cleanup, not Main, ENet or a display.
Estimated incremental native cost is below five seconds. It cannot prove pixels,
visibility under occlusion, camera framing or the graphical melee checkpoint.

R13 corrects both the emitted selector rationale and command guidance: earned
paid two-wave veteran/storage prefix, controlled W3 fallen/protected-stored
encounter, actual ENet resume, then allocation W4. Scenario behavior and the
not-earned-eliminated-stored balance limitation are unchanged.

Focused follow-up evidence: `logs/review-r11-r13/core-final.trx` passed147
codec/lifecycle rows (12s), including genuine partial-funding deaths and latched
lost-target windups; `runner-final.trx` passed7 melee detector rows (0.98s).
The first attempt caught insufficient paid fixture stone and a milestone at the
zero-blend action-start tick; the fixture now buys additional producers and the
shared milestone waits for elapsed windup. Those failures remain in the initial
logs, not waived. Locked restore, changed-file formatting and solution build
passed with zero warnings/errors. Tools remained .NET10.0.401/Godot4.7.2.NET;
process-local mise trust was used without changing user configuration.

`logs/20261014-013813-f2c350da`: selected native animation passed1.77s after
10.96s import. Both production detectors accepted all four actual native windup
and impact observations, with real changing imported socket bones; the retained
native committed miss, casualty, projectile and stockpile controls also passed.
No broad suite, graphical melee checkpoint, transport, package or exact-head CI
outcome is claimed by this review round. Native state/credential installation
and rendered pixels remain outer Test/CI/explicit graphical qualification scope;
the codec regressions prove refusal before assignment, not credential persistence
or transport delivery. Locks, imported assets/provenance and balance are unchanged.


### Validated delivery and late reviewer follow-up

No-mistakes run `01M4VZ0HPTR2S73F9N9CKX3BWG` returned `checks-passed`
for PR25 at `9b34f38b401e272e312d399da41ec006ebb09faf`. All four checks
passed: source, linux-package, windows-package and CodeRabbit. Required native
Linux/Windows qualification is in [Actions run38504242071](https://github.com/seraph1nia/odot-game/actions/runs/38504242071).
The PR is non-draft and unmerged; this is delivery readiness, not a release or
merge. Pipeline review/Test repaired malformed-selector refusal, empty logical
actors retained by cosmetic bodies, receiving projection/config/city/forecast
admission, production route coverage, shared-wave deadlines, truthful contracts,
real attached-weapon melee proof and ENet departure relay during hosted drain.
Final ordinary eliminated-resume and same-process host restart passed with engine
error checking intact; the selected graphical melee checkpoint passed68.87s
under its unchanged70s bound. Fourteen malformed kinds through actual native
Welcome and subsequent Snapshot refused before credentials/live state changed.
Details remain in the owning run's `retry-test-receipt.md`; these are scoped
receipts, not every journey or four-owner pixels. No Test exception was used.

Late review identified duplicate full validation on locally produced/already
decoded states and a W2 preparation barrier observing only the first peer.
`Main.SetState` now consumes trusted local or previously validated remote state;
`DecodeForReceiving` remains the sole remote admission boundary before Welcome
credentials or state assignment. Every current remote delivery caller uses it.
The redistribution helper now awaits each peer's advanced revision/phase/turn
serial with the original cancellation ownership and deadlines.

The first explicit seed8 redistribution run on the follow-up source failed
42.68s (`logs/20261014-032835-21c7c8a8`): its two ordinarily funded six-Sword
armies clearedW1 but the zero-investment third Keep fell before the changed W2
barrier was exercised. This is not a passing rescue or a synchronization fault.
This transport fixture now funds one Arrow Tower in that third sector through
its real published quote, retaining zero field soldiers, seed8, three production
settlements, stock/payment/identity assertions, all three living Keeps, pause/
exact wire equality, credential restart and accepted-Ready W2 allocation4/4/4.
The ordinarily paid, uninvested-sector four-owner rescue and width1..4 campaign
requirements remain unchanged and separately qualified. No numerical balance,
grants, extra edit window or seed substitution was introduced. Historical claims
about this old zero-investment three-owner fixture are superseded for the final
shared profile, not relabeled as current passes.

`logs/20261014-033705-8aa65f15`: corrected redistribution passed42.95s
(49.55s with preparation). Changed C# formatting and locked build passed; owned
children/readers were awaited with engine-error checking retained. New final-tip
CI must be refreshed after this committed follow-up; the parent delivery evidence
above does not certify an unpublished descendant.

Final local follow-up fast coverage: `logs/20261014-034029-b57d3339`,694
core/648 runner passes,57.89s; the same two Windows-native rows skip on Linux.
Strict OpenSpec change validation and whitespace checks pass. This supplements
the changed seed8 transport receipt; fresh exact-tip platform CI remains required
before final delivery.


### Frozen melee displacement recovery (2026-10-14)

Later review found that `UnitView.Sample` reset its root to the projected cell
position and applied the cosmetic melee lunge only while alive. A casualty
therefore jumped backward at death even though the authority retained its action,
FrozenTick and aim. The earlier native casualty proof compared two already-dead
samples and missed the transition. The owner authorized cancelling parked run
`01M4W85RTV5C0P3BG0AE0X7295`, preserving every unpublished commit with
guarded recovery, repairing this source defect and restarting validation.
Review removal of unused faction-distance precomputation and both documentation
fix commits are retained; the cancelled run is not publication readiness.
Its scoped Test receipt still records real admission/transport evidence at
`3e7f44f563bf805f6e276d9e8bb01f14fa95a71b`, including43 native admission
runs, the paid tower-only redistribution, ordinary eliminated-resume and
same-process hosted teardown; unchanged boundaries need no blanket repetition.

The renderer now samples cosmetic lunge independently from the live attack
blend, using the retained FrozenTick for casualties. The imported death clip
continues on the shared playback clock. Rules, logical claims, profiles, wire
fields and imported assets are unchanged. Tier A tests cover windup/impact/
recovery freeze, zero dead attack blend and moving/ranged/nonattack exclusions.
The existing Tier B animation selection adds one adjacent one-member Sword
encounter through actual CombatSimulation damage and compressed receiving.
A component-only1HP victim creates a deterministic simultaneous lethal impact;
it is not an earned-balance witness. The same real UnitView is sampled live at
impact and then dead, with exact native root preservation, advancing imported
bone pose and no retained logical squad claims. Owned rig cleanup uses the
existing component scene; no full application, peers or graphical journey.
Expected incremental native cost is below one second.

The native test failed before the repair at the live-to-dead position assertion
(`logs/20261014-061429-fcf23046`), after confirming the actual live lunge.
After repair,16 selected CombatVisualTiming tests passed
(`logs/20261014-061529-78711d7a`,1.19s), and the complete selected
headless animation component passed1.57s (6.66s including preparation,
`logs/20261014-061538-dbc60c68`). Retained projectile, committed miss,
health, stockpile and both native melee detector controls pass as well.
Locked solution restore/build and C# formatting passed with zero warnings/errors.
These receipts prove resource/node motion, not rendered pixels, four-owner
readability or final-tip CI; the earlier authorized graphical frames remain
scoped historical evidence. Fresh owning delivery at the repaired head remains
required before handoff. No acceptance failure was waived.

Coherent recovery fast coverage passed694 core and651 runner tests in36.43s
(`logs/20261014-061603-fc212b27`), with the same two Windows-native
argument-reader tests skipped on Linux. All21 durable specs and the finishing
change pass strict OpenSpec validation; whitespace checks pass. This closes
task6.3 on the repaired source; exact published-head platform CI is still pending.


### Cleanup, buffered summaries and configuration recovery

Owning run `01M4WGZYZ9MPFKHE1GTX0416TR` preserved three source fix
commits before its final re-review stopped with the backend error
“Our servers are currently overloaded. Please try again later.” This is a
failed infrastructure run, not Review/Test/publication readiness. Guarded
`sync --recover` retained every commit on the same feature branch.

- `fcefc923`: post-clear deployment changes persistent home claims without
  replacing a frozen source/transit/action, and summaries sample the exact
  buffered units used by native models.20 focused C# checks and the native arena
  component passed (`logs/20261014-064500-66a557ee`,
  `logs/20261014-064449-9e9a1311`,4.08s native). The initial native fixture
  incorrectly treated the One speed clock factor as1 instead of.5; its failure
  is retained at `logs/20261014-064251-4e3827f3`.
- `09d737f7`: restoration/configuration owns the cleanup invariant for every
  accepted recruit/regroup/store/send and army-tower build/sale path. Persistent
  purchases and assignments update immediately; new/reconfigured members and
  footprints wait without taking occupied physical claims. Cleanup refreshes
  footprints/restores homes before the waiting third settlement enters combat.
  Two pre-fix regressions reproduced premature survivor movement/action
  cancellation;38 focused core checks and native arena passed
  (`logs/20261014-071559-bcf16a17`,
  `logs/20261014-071658-0d1f7e35`,3.89s native).
- `b8b7f8ba`: removal/reassignment share participation reconciliation, releasing
  logical claims when only reserves remain without moving other survivors or
  replacing numerical death bodies. Rally activation/expiry/reset uses the
  existing buffered event timeline, including multiple snapshots before playback
  catches up. Tower secondary damage joins the existing accumulator bound;
  defaults and balance are unchanged. Eight expected pre-fix failures reproduced
  retirement/configuration defects, followed by72 focused core passes and native
  arena4.30s including actual summary Rally activation/expiry/pause
  (`logs/20261014-075918-3abbf94b`,
  `logs/20261014-075939-9223f57f`).

The additions strengthen existing Tier A compressed receiving/configuration
fixtures and Tier B arena component coverage. Ordinary paid opening/income/Hall
inputs underpin the post-clear edit fixtures; controlled enemy/pose setup remains
numerical ownership evidence, not earned rescue, campaign balance, native
transport or pixels. Runtime labels/models and actual playback are sampled; no
implementation-text tests or independent supervisor/clock/history were added.
Per-case incremental cost is estimated below one second; native receipts report
the entire selected component. Owned scene/process cleanup remains unchanged.

After recovery at `b8b7f8baaf092b5f9a3c9b53f35143c7f904ef22`, locked
restore/build and C# formatting passed with zero warnings/errors. Coherent broad
fast passed704 core and651 runner tests in40.09s
(`logs/20261014-081806-72aeae5e`), with the same two Windows-native
argument-reader rows skipped on Linux. The earlier unchanged art/campaign/
transport/package receipts remain scoped; neither these local results nor the
failed run certify required exact-tip source/Linux/Windows CI. Fresh owning
validation and final delivery remain required. No failed acceptance was waived.


### Final shared-arena implementation delivery (2026-10-14)

Owning run `01M4WR52S4MBC4C9SNTTX9HQ1C` completed Review, Test,
Document/Lint and publication, returning `checks-passed` at
`36dea87d862eae54c2e693ec7e06bece12c74cfd`. No phase was skipped and
no failed Test was approved. [PR25](https://github.com/seraph1nia/odot-game/pull/25)
is non-draft and unmerged. [Actions38553567582](https://github.com/seraph1nia/odot-game/actions/runs/38553567582)
has successful source, native Linux package and native Windows package jobs at
that exact head; CodeRabbit passed too. The source job qualifies coherent
headless formatting, broad fast C#, components and bounded transport after
all fixes. Guarded sync preserved the complete published history in the clean
caller workspace. Later tracking-only commits need refreshed publication/CI;
this is an exact implementation-head receipt, not a future-tip promise.

The final Review rounds repaired three missed boundaries:

- Persistent sold-home eligibility now follows current purchases while frozen
  physical footprints remain until cleanup. Unsold, foreign and duplicate
  placements still refuse without mutation.
- Existing playback retains consecutive pending admission, arrival, action,
  death and Rally boundaries until the shared clock reaches them, including
  several accepted snapshots before playback catches up. Summary and model
  projections use the same sampled population.
- Terminal freezing captures the selected primary's actual pose before
  freezing the squad and retains the original stop tick/aim across repeated
  stops. Native root/facing stays exact while the imported death clip advances.

Initial Test passed270 targeted C# cases (including full-six Crossbow seeds1/123),
native arena/animation, paid three-owner tower-only redistribution and explicitly
authorized two-owner private-display combat/melee graphics. The paid live
post-clear edit journey then reproduced a stale decision objective when the last
participant was retired with reserves; the real receiving guest disconnected.
Commit `68b26b7e6b075e3dce815433e73ba7822eff4a91` resets that decision
when releasing participation/claims. Fifteen selected lifecycle cases passed
on the repaired source, followed by the real Godot playing host/ENet guest
journey with byte-equivalent compressed accepted snapshots. Natural objective9
became0, the remaining member was Reserve without claims, and the guest stayed
connected. Sold-home placement, regroup/reinforce/store/send/retire, tower
purchase/sale, other-owner frozen poses and eventual cleanup restoration passed.
Exactly three ENDs per peer were accepted: final readiness waited in Preparation
for genuine corpse expiry, then automatically settled production6→9 and entered
wave3 with the reserve restored at its persistent home. Driver synchronization
mistakes were corrected without changing production guards or timing.

The detailed scoped receipt is
`/home/bart/.no-mistakes/evidence/01M4WR52S4MBC4C9SNTTX9HQ1C/test-final-receipt.md`;
raw logs/screenshots are ignored local evidence. Owned children exited0 and
were awaited before temporary runtime cleanup. Prior campaign/rescue/stress/art
receipts retain their original scopes; they were not blanket-repeated after
this reconciliation-only fix. Native components prove resources/poses, not
pixels. Actual graphical frames show the woodland arena and melee cues, with
acknowledged dense overview label overlap; four-owner pixels, universal
readability, GPU performance and terminal-freeze pixels are not claimed.
Pure-Crossbow paid opening/progression stays explicitly deferred. No merge,
archive or release was performed.

## Wave power rebalance (2026-10-14)

Baseline: shared-arena `4430c3ed1d2de402ae40f404d82404036e076959`, not
reduced defaults. [Wave power](wave-power.md) preserves the coefficients,
all twenty before/after compositions/powers and seven finite paid opening pairs.
That reference owns the enemy-strength rule, additive score and its distinction
from experiential difficulty; this section owns coverage and execution receipts.

Coverage admission and fixture changes:

- `WavePowerTests` is Tier A: real forecast/live/serialized rules, all20waves,
  original rosters1..4 and all co-op widths with one controlled fallen owner.
  It catches double application, faction cache collisions, changed players,
  lost inherited pressure, profile/identity mismatch and missing strength fields.
  Deterministic seed123,140 short transition boundaries, no child processes.
  Existing catalog tests alone did not protect the new profile resolution seam.
- `WavePowerEncounterTests` is explicit Tier E: seven baseline/current paid
  opening pairs plus three selected baseline/current twenty-wave role horizons,
  fixed seeds1/123, existing100000step/100decision bound and ordered event reader.
  Strictly lower first-wave soldier HP loss, no extra deaths/Keep loss, unchanged
  opening payment/army and retained progress are asserted. Melee/mixed retain
  victory assertions; ranged losses are measured honestly, not relabeled wins.
- Default faction-symmetry/HP goldens now assert independent75% expected profiles.
  Synthetic exact-damage/timing/screen tests explicitly retain100% enemy inputs
  where their purpose is symmetric arithmetic, not default-wave acceptance.
  The historical paid three-city casualty/authority negative likewise retains its
  original100% input; new75% paid co-op evidence is separate. No assertion, skip
  policy, deadline or acceptance requirement is relaxed to make a test green.
- Controlled receiving/active-fallen fixtures use one **actual enemy structural
  impact** as the wounded Keep threshold (field3.75HP, stored2.25HP), not the old
  five-HP constant. The100HP protected control, actual destruction/active-impact,
  stored service, owner denial, claims, codec and same-host resume assertions stay.
  The isolated Hall elimination test fixes its enemy pose beside the wounded
  Keep, rather than relying on an approach-time winner. These remain controlled
  numerical negatives, not earned injury or difficulty compensation.

Selected later paired paid outcomes (seed1, solo, normal prices):

| Policy | Baseline clear/outcome | Current clear/outcome | Combat ticks before→after | Casualties before→after |
| --- | --- | --- | ---: | ---: |
| Frontline | 20/Victory | 20/Victory | 4678→4139 | 41→29 |
| Mixed | 20/Victory | 20/Victory | 3965→3626 | 39→24 |
| Ranged | 2/Defeat | 20/Victory | 1755→3897 | 8→17 |

The ranged casualty total rises because the current army fights eighteen more
waves; it is not an equal-horizon attrition comparison. Baseline W3 loss remains
recorded. Existing funded-support acceptance also wins all20waves at1/2/3/4
owners, seed1:4139/4563/4773/4682combat ticks and29/55/85/109casualties.
This finite sample is not every seed/strategy or a graphical playability proof.

Execution receipts, all headless, local ignored evidence:

| Selected coverage | Result | Evidence |
| --- | --- | --- |
| Locked restore, serial/no-server solution build, required analyzers, `dotnet format Odot.slnx --no-restore` | Passed | `logs/wave-power-{restore,build,format}.log` |
| Targeted all-wave/faction/receipt coverage | 9 passed,20.22s phase | `logs/20261014-193328-31627648` |
| Affected existing fast core assertions | 165 passed,20.04s phase | `logs/20261014-192516-4bbf7630` |
| Paired paid opening/late encounters | 10 passed,26.70s phase | `logs/20261014-192908-58b850c9`; JSON pairs `logs/wave-power-paid/*-enemy{100,75}.json` |
| Existing all-width funded support campaign acceptance | 4 passed,39.28s phase | `logs/20261014-193409-77030a24` |
| Final broad fast core + runner | 737 core +651 runner passed,2 platform skips;59.50s/15.89s phases | `logs/20261014-193449-1a1a8cc6` |
| Real ENet controlled active-fallen receiving/resume | Passed,23.11s scenario | `logs/20261014-192957-3108ed69` |
| Real ENet stored eliminated-resume | Passed,52.65s scenario | `logs/20261014-193038-ac81d221` |

Initial preparation failed with MSB4166 (restore child node exited), then
CSharpFormattingAnalyzer AD0001/NullReferenceException in Game.Core; formatter
also reported a Roslyn body-binding NullReferenceException, without a source
file/line. Bounded diagnosis preserved the patch, built clean landed core under
the same locked SDK, then changed core without/with the new score file; all
isolated builds succeeded. Full formatter/solution build subsequently passed
with required checks intact. No reproducible syntax trigger or SDK defect was
established, and no analyzer was disabled. The original broad fast run exposed41
expected old-strength assumptions; the repairs above preserve their meaningful
arithmetic/authority outcomes. Final broad fast coverage is green.

Unexecuted here: unfiltered deep/calibration/reference matrix, other transport
selectors, Tier B native component suite, packages, native Windows checks,
release/installer/Steam pairs, stress and every graphical test. The two runner
platform skips are not claimed as executed. No install/global configuration,
visual UI/asset changes, check waiver, merge, archive or release was performed.

## Health-only actor overlays (2026-10-14)

The [health-overlay contract](../openspec/specs/game-feedback/spec.md#requirement-authoritative-overhead-unit-health-bars)
owns presentation requirements; [native presentation](shared-arena.md#native-presentation)
identifies the production control owners. The former squad-label observations
remain nonvisual diagnostics with empty text; graphical health detectors now
consume real member controls instead of label strings. Other building titles
and empty/locked plot labels remain. No rules/wire/asset locks or mechanics change.

Coverage admission/replacement:
- Tier A extends `CombatPlaybackTests`: nonlethal member/objective damage,
  overlapping accepted frames, event draining, buffered Keep loss/inactivation,
  baseline and match replacement. Pure policy catches anticipatory health without
  an engine; the deterministic fixture adds one sub-second fast case.
- Existing Tier B `animation`/`arena` replace label assertions with actual native
  text absence, independent HP/max/fill, input transparency, stable nonoverlapping
  compact member controls, no duplicates across updates, hidden/dead actors and
  real native member/objective control cleanup. Actual compressed lethal member
  and Watchpost hits verify bars and rigs/ruins share admission/death timing,
  including pause/reset. Pure tests cannot prove actual Control/resource creation
  or freeing; no new full-game journey or supervisor was added.
- Existing `receiving` retains paid tower upgrade/artwork/roof picking and useful
  inspector details while asserting no purchased-tower home label; forecast rigs
  likewise remain read-only, inspectable and text-free. Dormant graphical status
  selectors use the inspector rather than requiring removed floating badges.

Local locked tools: .NET 10.0.401 and Godot .NET 4.7.2. Formatting and filtered
47 core cases passed (`logs/20261014-191128-38798cb7`). Coherent broad fast passed
729 core + 651 runner with two Windows-only skips in 43.12s
(`logs/20261014-192028-37f18d50`). Selected headless animation passed 1.88s
(`logs/20261014-192650-6c14d732`, including native compact style minimums), arena passed 5.20s
(`logs/20261014-192408-a1ab0ac7`, including hidden-arena and purchased-tower
sentinel regressions), and receiving passed 23.14s
(`logs/20261014-192420-d29cb9d0`). These are scoped local receipts, not future-tip
PR/CI acceptance. Rendered readability/overlap, actual-game graphical journeys,
GPU/native display, deep campaigns, transport and package qualification are
**unexecuted for this overlay change**; older rendered squad-label receipts do not
qualify the new bars. No graphical permission, check waiver, release or merge.

## Vivid overgrown assets and native ambient loops (2026-10-15)

[Import/runtime receipts](vivid-ambient-import.md) own the exact immutable source
revision, selected inventory, retained failures and successful local evidence.
The changed boundary is imported object animation in real production building and
water owners, not numerical state or a standalone asset preview.

Coverage admission/replacement:
- Tier A extends existing authored-byte tests with selected ambient clip names,
  source timing and absence of unit rigs. Existing exact GLB/map provenance checks
  now admit source image indices so five valid duplicate image names cannot
  overwrite or misattribute their distinct bytes. Full canonical building/unit
  mappings and six unit clips remain covered. Cost: 15 targeted cases in 3.13s
  including test-process overhead after final engine import.
- Existing Tier B `animation` executes production building placement and native
  stream/river resource loading, actual idle-frame playback advancement,
  imported-name/loop lookup, changing localized targets, unchanged structural
  local transforms, actual zero/end seams and repeat playback. Water assertions
  use its real direct-mesh tracks, packed material bindings and eight-triangle
  interior detail; the production landscape verifies native consumers, fixed
  home/defender playback, no duplicate cover owners and shadowless water geometry.
  The fixture owns its nodes in the existing minimal child. Component cost: 6.18s.
- Static consolidation remains forbidden on animated/skinned hierarchies. The
  former static stream/surface-water consolidation assessment is replaced by
  native animated rendering, plus detached cached rest meshes for contact/port
  queries. Existing `assets` checks compare native/rest vertices across connector
  orientations, full neighboring profiles and bridge-deck transforms, and retain
  corrupt-normal/UV/winding/material/geometry rejection and forest PBR/mipmap
  checks. It does not flatten active animation just to satisfy static coverage.
- Existing `arena`, `receiving` and `worldtree` passed their lifebar/invulnerable
  tower/shared-squad, ledger/battle-flow and living-tree preservation boundaries.
  Broad fast passed 738 core + 652 runner, with two Windows-only skips; deep
  campaign tests were not selected. Source preparation mutations were sequential.

No full-game journey or supervisor was added. Rendered **in-game** vividness,
aesthetic/readability acceptance and GPU cost remain unexecuted under headless-only
policy; source standalone evidence is not a substitute. These implementation
receipts do not replace subsequent final-head pipeline/CI qualification.

### Optimized successor review fixes

The approved successor pin is `133c7cdeb368cada408d1c95c9414e08488371a9`.
The initial vivid-pin receipts above are historical, not evidence for these bytes.
The [current import report](vivid-ambient-import.md) records the accepted GLB-size
tradeoff and preserved map/animation contracts.

Coverage admission:
- Tier A adds deserialization failures for omitted/null image indices and calls
  the production indexed-map validator with both real duplicate-name payloads,
  invalid indices, wrong name/index/suffix and incorrect hash/size. The previous
  positive distribution test could not catch the unnecessary name-only fallback.
  Inputs are immutable imported GLBs/maps; only disposable manifest mutations use
  an owned worktree `.test-runtime` directory with awaited synchronous cleanup.
  Expected cost is subsecond per case, with no engine or new dependency.
- Existing Tier B `animation` adds actual imported building/bridge mesh, surface
  and native-node work bounds, reports production load/measurement/add/play cost,
  and includes the original bridge's animated targets alongside all selected
  buildings, stream and river overlays. Existing motion, seam, material and
  production-landscape checks remain. Metadata alone cannot prove Godot preserved
  the optimized hierarchy or that playback survives mesh-to-owner conversion.
- Existing Tier B `assets` retains its native bridge-deck preservation and
  depth checks, now resolving actual material bindings rather than obsolete
  static submesh names. The live consumer still removes only its incompatible
  recessed components and grass; complete source bridge playback is tested
  independently. No broader journey or runtime batching subsystem is added.

This is a bounded review-fix verification selection, not full test/lint, transport,
package, native Windows, memory or rendered-performance qualification.
