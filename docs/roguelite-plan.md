# Roguelite milestone implementation and evidence plan

Baseline: ed9c53f (optimized vivid assets), protocol 15 / rules 7 / flow 2.
Read gameplay, shared-arena, session ownership and verification guidance before editing.

## Canonical ownership map

- D01–D06 (Warden, Merchant, Warmaster, Arcanist, Grovekeeper, Huntsman): frozen Core definitions, per-city run state; lobby command, start-once allocation, economy/profile projections; native doctrine dialog and roster/inspection.
- E01–E06: canonical production plus projected income, additive with Worldtree.
- A01–A05, M01–M05, R04: shared integer profile/capability resolver, existing/future soldiers; A06: canonical upkeep and quotes.
- T01: physical Watchpost current/reset HP; T02–T06: actual weapon profiles/cadence; no purchased tower HP.
- M06, R01: pre-impact squad state and first committed squad action; R02: existing status application; R03: simultaneous lethal-tick attribution; R06: actual Watchpost at structural resolution.
- F01–F06: atomic authoritative selection; immediate bounded stock/research and living-member healing, no resurrection.
- R05: personal successful-clear counter, acquisition clear excluded.
- All 36 IDs: offers, stack caps, rarity, meaningful eligibility, native projected description/selection, authoritative validation and snapshot/reconnect.

## Ordered implementation

1. Define catalog/run state and bounded fixed-point resolver; add exact-value/catalog tests.
2. Wire economy, recruitment, upkeep and combat effects at current canonical seams.
3. Add authoritative RewardDraft, seed-isolated offers/encounters, presence fallback and snapshot compatibility; test flow, commands and stream invariants.
4. Build native woodland doctrine/draft components using existing theme and session bridge; headless actual component tests and selected real transport evidence.
5. Update gameplay/help/verification evidence; restore locked, format, focused then broad fast/headless checks. Commit coherent implementation before same-worker no-mistakes handoff.

## Implementation complete

The ordered implementation is present in Core, production Godot Controls and
existing process drivers. [Session ownership](sessions.md) owns current wire
compatibility. See [the complete player-facing rules](roguelite.md) and [actual verification
receipts](verification.md#roguelite-milestone-1-evidence-2026-10-15).

- Six personal doctrines and all 36 fixed-value rewards are wired into live
  allocations, production, recruitment/upkeep, research, profiles/statuses,
  physical defenses, healing, kill attribution and clear receipts.
- All nineteen drafts use the authenticated command/ledger boundary, safe seeded
  eligibility, one-choice state, presence-driven fallback and detached snapshots.
- Seeded encounter variation intentionally permutes authored entry order at the
  exact existing numerical budget. `NextWave` remains the reusable canonical
  forecast/combat-start projection; logical board and allocation contracts remain.
- Native woodland doctrine/reward dialog, actual selection/ack binding, live
  descriptions/stacks/rarity/forecast, modal ownership and normal solo lobby
  choice are implemented. Headless solo/dev host/guest behavior remains intact.

## Verification scope

Tier A owns exact 6/36 values, caps/rounding/research interactions, start-once/city locality, all nineteen drafts/no final draft, exactly sixty settlements, offer/stream/variation invariants and negative authority/codec checks. Existing core fixtures and command/ledger seams are reused. Tier B instantiates actual native dialog Controls and observes signals/commands without rendering claims. Tier C uses existing Child/Scenarios ownership for bounded real ENet disconnect/reconnect convergence. Preserve failures in owned evidence; [verification](verification.md#roguelite-milestone-1-evidence-2026-10-15) owns executed receipts and remaining qualification, including the separately authorized local GPU journeys. Headless checks alone do not establish rendered, physical-input or platform acceptance.
