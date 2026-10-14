# Design

## Context

See [proposal.md](proposal.md) for scope and motivation. This is a recovery and finishing change on `fm/odot-shared-arena-squads`, starting from the clean merge checkpoint `85ebbcc`. It does not restart the redesign.

### Observed starting point

| Area | Existing owner and state | Remaining obligation |
| --- | --- | --- |
| Numerical combat | `CombatSimulation` retains individual Arch members; `CombatSquad`, `SquadDamage`, `SquadMovement`, `SharedArena` and `CombatReservations` own shared actors/policies | Protect the implemented invariants and repair only demonstrated defects |
| Persistence/economy | `World`, `ArmyRoster`, `SettlementCommands`, `BattleFood` and research owners | Keep paid commands, member identities and exact three-END settlement; complete viable paid openings |
| Compatibility | `SnapshotCompatibility`, `SnapshotPayload` and `AuthoritySession`; protocol 15/rules 7/flow 2 | Preserve strict admission/full identity, current forecast and refusal of both predecessor-14 schemas |
| Native presentation | `SharedArenaView`, `SquadPanel`, `CombatLayout`, `SharedCombatProjection`, `UnitView` | Finish modest arena dressing and verify all-owner controls/camera/feedback |
| Verification | Existing C# fixtures, minimal engine components and owned DevRunner scenarios | Qualify the unfinished trace, retain meaningful negatives and distinguish numerical/component/transport/visual evidence |

Historical current-input receipts cover 558 core passes and 647 runner passes with two Windows skips across separate runs. The final campaign trace additions in `PaidCampaignFixture` were made after those receipts and are committed but unqualified. The integrated default matrix recorded ten twenty-wave wins and four W1 defeats; cooperative seed123 also failed its foreign-impact guard. Existing binaries now differ from the recorded receipt hashes, so rebuild current source before execution rather than infer current-head coverage.

The tracked evidence is in `docs/testing-audit.md` (integrated sections), `docs/paid-squad-baselines.md` and `docs/shared-arena.md`. Raw historical evidence under ignored `logs/shared-arena-task/` is useful when available, but this change and updated tracked coverage must remain understandable without private task/inbox files. Some shared-arena prose still calls the already merged preparation prerequisite unlanded.

## Goals / Non-Goals

**Goals:**

- Finish the existing owners with bounded, attributable edits and clear exit criteria.
- Establish full-six Crossbow survival with a small shared-profile buff, retaining paid progression as an honest diagnostic.
- Keep one viable ordinary cooperative campaign and genuine cross-sector assistance.
- Make the arena visually belong to the settlement's woodland diorama while preserving interaction and geometry.
- Reconcile obsolete durable contracts and provide a reviewable, verified PR.

**Non-Goals:**

- Exhaustive balance, universal strategy parity, seed searches or a new economic progression.
- Rewriting working actors, introducing another simulator/supervisor or restoring city-local combat.
- New art exports, biome/river redesign, cinematics, destructible purchased towers, new morale/upgrades or manual combat control.
- Treating a successful headless component as rendered acceptance or adding graphical work to ordinary CI.
- Shipping packages as releases, changing tool/dependency locks, or merging without the normal review path.

## Decisions

### 1. Preserve the branch and qualify its last edit first

Use existing production seams rather than introduce a recovery layer. Start with current locked restore/build and targeted trace/compatibility/shared mechanics checks. Retain seeds 1/123, existing histories, genuine defeats and the strict positive cooperative witness.

The unfinished trace already records complete entry enemies and ordered events through the fixture's single numerical reader, with an independent foreign-impact count. Exercise that edit before using it to select a fix. Do not extend protocol history or scenario deadlines to obtain observations. Preserve raw failure events in ignored evidence; summarize the causal result in tracked docs.

Alternative rejected: treating the previous 558/647 receipts or the merge's clean Git state as proof of the later tracing source.

### 2. Separate affordable preparation from full-force combat strength

The current generic frontline policy spends on Stonecutter/Market land before field strength, leaving four paid supporters per investing owner against twelve enemies in the three-player rescue row. The ranged policy uses two Lumbermills but its Archery Range consumes early wood, leaving one paid Crossbowman before the last investment boundary. The third END grants income and immediately starts battle; that income cannot fund another purchase.

Recover the complete payment/production sequence at the existing `CampaignStrategy`/`PaidCampaignFixture` seam. Use the already demonstrated ordinary two-MetalMine/six-melee opening and later mine sale for cooperative support where appropriate. For ranged, choose the best small legal producer/trainer/refund schedule based on actual quotes. Keep the existing equipment, upkeep, production and three-END rules. Do not assume six Crossbowmen are affordable before W1 merely because six starting army slots exist.

Pure-ranged acceptance now requires one short explicit numerical six-Crossbow-versus-default-W1 encounter with normal profiles and legal homes, at retained seeds1/123. This arithmetic setup is disclosed and cannot be called earned economy. The full force must clear with a living Keep and ranged survivors without a melee screen or BattleStalled.

During apply, an actual three-Crossbow paid producer/refund schedule was tested. The initial35HP/11X candidate lost W1 at both seeds; the second40HP/12X candidate cleared W1 at seed1 but lost all five W2 members and W3, while seed123 still lost W1. Both preserve costs/range/cadence/Y0/structural3 and faction symmetry. The owner explicitly chose to prioritize landing the refactor, require full-six survival and defer pure-Crossbow paid opening/progression acceptance. Keep40HP/12X as the final small candidate, qualify the full-force encounter and protect other passing controls; do not tune further or remove diagnostic losses.

The conditional no-casualty ledger first permits six paid members at W3, not W2: W1 recruits3 after two settlements and actual temporary-producer refunds; W2 rebuilds Farm and can afford only two more before final END; a W3 sixth would use retained earned resources. Actual casualties invalidate that full-force progression; record them rather than certify paid viability. The existing campaign-policy owner retains this diagnostic schedule.

Alternative rejected: another stat-tuning loop, silently claiming three ranged recruits are a full army, adding a fourth spending window, or converting this refactor into an economic balance project.

### 3. Bound cooperative acceptance and stop balance work there

Keep the existing failed three-owner seed1/123 rows as the diagnostic starting point. First establish actual early foreign impacts and where supporters die using the full trace. Repair the paid support schedule before changing routing or stats. If complete paid supporters still fail to approach a reachable foreign threat, isolate the first wrong production decision and fix it within `CombatSimulation`/shared routing with a small regression.

Required rescue evidence is physical movement across a sector boundary, a landed allied contribution against an enemy threatening another owner's Keep while that Keep still lives, and a subsequent team clear. A final survivor roster, zero foreign impacts, synthetic Keep injury, or post-result historical events do not satisfy this earned witness. Use a bounded four-owner setup to cover the original rescue intent; default weak/uninvested owners may fall, but the threat owner receiving the claimed assistance must still be alive at impact. No requirement that every Keep survives the whole campaign is added.

One default-price policy must win twenty waves for each roster size 1/2/3/4, with a declared finite seed sample (start with seed1 for roster-width integration; retain seeds1/123 on the failed rescue controls). Preserve previously passing solo family controls after changed profiles. Pure-ranged opening and twenty-wave outcomes remain diagnostics deferred by the owner; their losses do not trigger a general balance project once full-six survival passes. Do not mark a failed test passed or weaken its rescue predicate; correct its input or report failure.

Alternative rejected: make every named policy win every seed or replace earned cooperation with the existing controlled fallen-owner fixture.

### 4. Style the existing arena owner with the shipped woodland palette

Keep `SharedArenaView` as the native owner. It currently builds flat green cylinder hexes and a plain dark box backstop; physical imported structures already exist. Reuse `LandscapeAssets` placement/geometry helpers and the shipped `DetailedGround.Grass`, Moss, Dirt, Litter/Duff families plus registered forest boulder/tree/fern/moss/leaf props. The pinned manifest/source bytes remain unchanged. Reuse static geometry/material resources and bounded batching already demonstrated in `VillageLandscape`; do not instantiate a new full village or session inside the arena.

The small art pass has three layers:

| Layer | Planned treatment | Acceptance |
| --- | --- | --- |
| Fighting ground | Authored grass with restrained moss/earth variation aligned to existing hex centers and contact plane | Units/structures remain grounded; picking and pure projection stay exact |
| Outer frame | Compact forest/rock clusters, low ground dressing and a finished opaque edge/backstop | The arena reads as a woodland diorama without tall silhouettes covering active cells or exposing bare edges at camera limits |
| Tactical information | Strong owned deployment highlights, quieter combat grid, compact owner seals and readable structure/member states | Selection/availability/occupation and damaged/fallen/inactive objectives remain distinguishable without color alone |

Keep tall props outside board cells, deployment/entry cells, static objectives and their sightlines. Decoration follows deterministic geometry IDs and never consumes combat RNG. Ground and outer framing are cosmetic; they grant no terrain bonuses, collision or route changes. Preserve existing `SharedCombatProjection` centers/formation slots and `BattlefieldFrame`/`TabletopCamera` fitting based on playable content, not scenery extent. Keep the existing supported pan/zoom/portrait boundary coverage.

Build static dressing on arena identity/geometry change and reuse it across HP/action snapshots; preserve bounded node/resource counts and release everything on session/layout replacement. Avoid new dynamic lights/shader systems and avoid decorative clutter inside melee/projectile lanes. Required style is modest environment cohesion, not an upstream asset production task.

Alternative rejected: reskin the arena as an unrelated biome, scale the entire board to hide awkward cues, or edit imported GLB/material bytes.

### 5. Repair native interaction at the existing command/receiving boundary

Use `SquadPanel` and existing HUD/input/inspector owners. Verify new/selected squad recruitment, reinforcement, regrouping, individual storage/return/retirement and placement through ordinary command -> acknowledgment -> compatible snapshot. Preserve focus and delayed acknowledgment behavior. Owner, ready, pause, combat and eliminated-state refusals stay authoritative; foreign inspection does not grant edits. Expose individual wounds/levels/experience and participant counts without suggesting pooled HP. Preserve real primary aiming, sockets, misses, frozen physical death and advancing clip behavior; repair only demonstrated defects.

Post-clear cleanup keeps the runtime battlefield separate from accepted preparation
edits: persistent roster/home/tower purchases update immediately, while the combat
owner defers physical restoration and new claims until existing corpses expire.
Removal and reassignment reconcile participation centrally, so reserve-only squads
cannot remain Alive or retain source/transit claims. Summaries consume buffered
member poses and the existing Rally event timeline, rather than newest-snapshot
status deadlines; no additional clock/history or wire field is needed.

Alternative rejected: optimistic local member placement, per-soldier targeting or a replacement UI/session framework.

### 6. Consolidate specifications before the delivery gate

The nine deltas replace explicit old contracts about size six, permanent protected lanes, corpse blocking, capped/cross-hex splash, virtual city shields, teleporting transfer, deleted fallen storage and per-city views. They retain independent economic/research/ownership/retry guarantees and the landed preparation/speed/forecast behavior. During apply, sync these deltas with the repository sync workflow and update gameplay/session/shared-arena/paid-baseline/coverage docs before publication. Main spec Purpose wording that still says lane contact should also be corrected then; delta Purpose does not update an existing capability.

Keep protocol15/rules7/flow2 unless a later actual schema/semantic change requires another explicit version. The Crossbow base change must be represented by the existing full configuration fingerprint, catalog, forecast and received profile. Never claim numeric protocol equality alone makes predecessor schemas compatible.

Alternative rejected: another private progress file as the only plan or a blanket golden/hash refresh.

### 7. Verify each changed boundary once, then hand delivery to one owner

Use the repository testing strategy, commands and hygiene skills. No preparation/build/import/export mutations run concurrently. Suggested commands below are implementation-time selections, not executed evidence for this proposal.

| Boundary | Selection | Purpose / limit |
| --- | --- | --- |
| Trace and rules | Focused `test-core` shared/receiving/configuration classes and existing failed cooperative `test-campaign` method with opt-in trace | Causal diagnosis and current-source coherence; original failed rescue guard remains failure until fixed |
| Ranged/support policies | New focused paid witnesses plus `test-campaign --filter 'FullyQualifiedName~SquadCampaignBaselineTests'` and one 1..4-player paid policy sample | Bounded survival/progression; no universal balance claim |
| Native styling/control | `test-engine --scenario arena`, `receiving`, `animation` for affected inputs | Real resource/node/control/picking/camera/death proofs, not pixels |
| Ordinary/controlled transport | Default bounded `test-network`; selected `active-fallen`, `eliminated-resume` only if relevant profile/state changes invalidate receipts | Real ENet delivery/resume; distinguish controlled fixtures from paid progression |
| Integrated regressions | `test-fast`, locked solution build and changed-C# formatting | Broad fast invariants after coherent edits, including explicit platform skips |
| Large actor risk | One explicit `test-scale --scenario large-battle --seed 1` if combat/routing changed | Existing synthetic stress window, not a full performance campaign |
| Bundled resources/runtime | `ci-linux-package` with preinstalled matching templates; native Windows CI | Required package loading/identity/resources; no installer/Steam/rendering claim |
| Visual quality | One selected combat graphical checkpoint only after explicit permission; otherwise owner preview and an unverified visual limitation | Actual arena appearance and cue readability, not silent component equivalence |

After implementation/docs/spec sync and coherent checkpoints, invoke one no-mistakes delivery run for review/tests/lint/docs/push/PR/CI. That run owns its fixes while active; do not perform competing caller edits/reviews. Reuse qualified evidence where inputs remain unchanged and inspect every required exact-head CI result. End with a concrete PR for human review/merge, not a release or inferred graphical acceptance. Request graphical permission only when the implemented result is ready to inspect, not during this planning turn.

## Risks / Trade-offs

- Shared Crossbow profiles also strengthen enemy ranged waves -> retain mirrored-profile checks and existing passing paid campaign controls.
- Preparation may fund fewer than six ranged members at W1 -> retain the failed paid-opening ledger separately from required full-six arithmetic strength; keep actual prices and no fourth edit.
- Requiring every strategy to win invites an endless tuning loop -> use the bounded obligations above, retain late pure-ranged losses as known balance follow-up, and stop after meaningful acceptance.
- Scenery can shrink framing or hide figures -> derive fitting from board/objectives, keep active cells clear and check supported sizes/extreme travel through actual native resources.
- Native components cannot judge visual appeal -> record that boundary and obtain one focused visual review when authorized.
- Spec replacement can accidentally remove unrelated guarantees -> compare effective synced specs against unaffected economy/research/session requirements before the delivery run.
- Historical receipts mix old protocols/inputs and ignore later trace changes -> attach every new claim to actual current inputs and keep superseded failures labeled.

## Migration Plan

1. Work on top of `85ebbcc`; preserve merged prerequisites and all existing tracked work.
2. Commit trace/policy/profile repairs as one coherent behavior checkpoint with causal evidence.
3. Commit native arena dressing/control fixes as a focused presentation checkpoint.
4. Sync deltas, reconcile docs and complete current-input qualification before the single delivery gate.
5. Publish one integrated PR with exact head/checks and honest deferred visual/platform/balance items. Archive only after implemented tasks are complete; keep any delivery-only handoff clearly distinguished.

Authorities and peers must be compatible with protocol15/rules7/flow2; old saved combat state is refused with rejoin/fresh-match guidance. A rollback uses normal revert/review of coherent commits and a fresh compatible authority rather than rewriting history or loading old combat state into the new model.
