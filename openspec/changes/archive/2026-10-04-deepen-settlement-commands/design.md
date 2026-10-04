# Design

## Context

See proposal.md for motivation and the private `/home/bart/projects/personal/firstmate/data/odot-architecture-hardening/report.md` for prioritized evidence/counterevidence/exclusions. The existing public deep module is Match/AuthoritySession; its adapter Main and ordinary verification EconomyAction both construct the same contextual request fields. ArmyConfiguration already serves both authority and disabled-action previews. CombatSimulation is the only identity/health owner. There is no I/O dependency to mock or new public port to invent.

## Goals / Non-Goals

**Goals:** improve locality of stale-quote construction and city editing; remove branch-local revision bookkeeping and the placement value-membership join; keep tests at real executable interfaces and exact wire/feedback behavior.

**Non-Goals:** a generic command bus, new rollback protocol, public mutable city interface, exhaustive migration of hand-written adversarial tests, a new UI policy framework, any numerical or compatibility change, runner lifecycle redesign. No new requirement: schema explicitly skips deltas for this pure refactor.

## Decisions

### Snapshot-to-request seam

Keep Command's positional constructor intact. Add a static `FromSnapshot(snapshot, sequence, action, city, slot, building, soldierType, technology, resource, bundles, payment, unitId)` factory. It derives match/phase/turn, building generation, plot expansion count, purchased home count and selected track level. Missing city uses the existing Main sentinel expectations (generation zero, counts/track minus one). Invalid slots remain raw invalid values with generation zero/track minus one for authority rejection. The caller reserves identity before construction and saves the returned Command; no quote refresh on retry.

Use from Main.SendAction and EconomyAction.Command only; the latter keeps its existing missing-player precondition. Preserve wire property order, enum identities, default Swordsman/Standard payment and JSON. A separate builder object or per-action request hierarchy would increase the interface and force protocol changes, so reject both.

### Internal settlement-edit seam

Construct one internal SettlementCommands with the existing frozen EconomyConfiguration, ArmyConfiguration, CombatConfiguration and match-owned CombatSimulation. Its only operation is Apply(City, Command), returning the existing CommandResult. It owns research, home purchase, retire/store/send and building/plot/market/recruit/hall-track policy, including the same rejection messages/order and direct validated commit operations. Shared Match guards execute first in their existing order: authenticated connected player, stale match/phase/turn, foreign city, terminal, payment selector, pause/resume, paused, start, editable phase/alive, Ready/unready, ready-lock. Match alone increments revision once for an accepted edit after the module returns. No scene/transport dependency crosses this in-process seam.

Keep restoration inside recruitment/transfer exactly where it currently executes. Production preflight, BeginWave, outcome/reward/food/recovery authorization, death cleanup and disposal remain in Match/CombatSimulation unchanged. No double store of resources/health/assignments, transaction journal, generalized effects object or catch-all exception rollback is introduced. This preserves current supported failure atomicity rather than claiming transactional protection against OOM/engine failure.

Alternatives: partial Match file-only split improves scrolling but leaves the revision policy repeated; per-action handlers spread a small bounded dispatch across many interfaces. The selected module earns depth by hiding city edit implementation behind one internal call while deleting accepted-path duplication.

### Scope persistent claims directly

ArmyConfiguration.Find builds one array of living assigned soldiers scoped to field or exact hall slot/generation. For each existing ordered tile, compute capacity and occupied anchors from those soldiers; choose the same first fitting tile and forwardmost/free-identity anchor. This removes the assignment record-equality join and repeated roster enumeration. No cache/index survives a mutation; no persistent spatial index or generic packing abstraction. Sizes, six-point per-tile capacity, purchased tile order and generation isolation remain unchanged.

### Demonstrated paid network preparation correction

Focused authority-resume-victory failed at seed 11669866211869037831: ordinary refill occupies both purchased homes, then the fixture unconditionally requests a Crossbowman. The correct full-capacity refusal is not a refactor regression. A cheap same-seed replay runs both landed source and refactored source, retaining exact six veteran IDs/health/assignments, G7/W2/F24/S3/M4 stocks, refusal without mutation, quoted 5-gold purchase, successful 2-wood/1-metal equipment payment, one revision per accepted edit and original identities/retries across rebind. Firstmate authorized reusing existing EnsureFieldRoom with actualInput:false before the explicit network recruit and its before snapshot. This uses normal earned gold, not retirement/grants or an assertion waiver. It does not reopen the deferred runner architecture work in [proposal.md](proposal.md#impact); no runner architecture redesign or expensive scenario is added.

## Acceptance criteria

The private HTML comparison remains evidence; Firstmate explicitly deferred its optional browser-render verification after missing owned browser prerequisites. It is **unverified**, not passed. No required game/network/export/package validation is deferred.

- Pre-refactor characterization through AuthoritySession records every supported city-edit action: research-tech, buy-home, retire/store/send, buy-plot, sell/trade/build, upgrade-capacity/upgrade-healing, upgrade/recruit, plus unknown/legacy research refusals. Accepted edits advance exactly one revision, rejected edits preserve full serialized state, and duplicate accepted identities never mutate again.
- Shared multi-invalid guards retain exact feedback precedence, local/remote equivalence, Ready/paused/eliminated/connection and stale instance/count/track checks. The new authority characterizations use ordinary paid construction/production/research receipts rather than injected funds.
- Factory output independently matches every contextual field and explicit argument, round-trips JSON, preserves omitted defaults and invalid-target sentinels, and executes through both delivery modes. `CommandConstructionTests` checks saved factory requests after building replacement and home-count changes, while an accepted retry retains the original result. Existing plot-count and hall-track stale-quote regressions remain required in `BuildingTransactionTests` and `ArmyRosterTests`.
- Placement fixtures distinguish field/hall generation despite colliding tile/anchor integers, exclude dead/unassigned soldiers and keep exact insertion order/no cross-tile pooling. Existing survivor/restoration, recovery, food and frozen reference assertions stay unchanged.
- Cheap suite, focused authority-resume network and economy/army UI, then complete after-CI pass; record timings/owned cleanup and inspect retained graphical evidence. No added expensive scenario is needed because current slices already exercise both real adapters and exact roster controls.

## Risks / Trade-offs

- Changed feedback precedence → shared guards stay in Match; characterize multi-invalid requests before extraction.
- Factory tests become tautological → assert expected fields independently and retain manually constructed stale/adversarial requests.
- Extraction becomes relocation-only → collapse revision/result duplication; no per-action classes, interfaces or generic infrastructure.
- Placement accidentally includes another hall → explicit collision/generation fixtures and unchanged full references.
- Unexpected failure after payment → retain current failure semantics; broader rollback would need a separate proven defect/design.
- Broad review exceeds safe coherent scope → retain the explicit deferrals in [proposal.md](proposal.md#impact), reject file-size-only splitting; do not claim all opportunities implemented.

## Migration Plan

No save/session/protocol/dependency migration or main-spec sync is required. Restore locked, characterize existing source, refactor incrementally with cheap checks, format the solution, run focused/complete acceptance and record evidence. Strictly validate planning before apply and at completion; archive only this completed skip-specs change. Rollback is source revision rollback, not conversion of running matches. Local verification has no publishing/upload; no-mistakes owns the later publication handoff.
