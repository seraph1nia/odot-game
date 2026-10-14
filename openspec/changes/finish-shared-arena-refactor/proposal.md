# Proposal

## Why

The shared-arena branch already contains most of the squad combat redesign, but its specifications still describe the replaced city-lane model, its latest tracing edit is unqualified, and ranged/cooperative openings fail. Finish and land the existing refactor with modest tuning and a woodland arena presentation rather than expand it into a general balance project.

## What Changes

- Recover from branch checkpoint `85ebbcc`, preserve working mechanics and past failure evidence, and make this change the remaining-work plan.
- **BREAKING**: reconcile the durable contracts with one connected arena, one squad per hex, individual soldiers, within-squad X/Y damage, physical objectives, immediate logical death release, and protocol 15/rules 7/flow 2. The breaking implementation already exists on the branch; legacy state is refused rather than migrated.
- Qualify the unfinished campaign event trace and repair paid opening policies before interpreting their defeats as combat defects.
- Apply a small configuration-based Crossbowman buff, selected at 40 HP and 12 primary damage after two bounded candidates against the original 30/10. Require a normal-profile six-member/two-squad force to survive the default first-wave encounter without a melee screen. Per the owner decision during apply, defer pure-ranged paid-opening/progression viability and retain its genuine defeats and three-stage payment ledger as balance follow-up.
- Restore bounded ordinary cooperative progression and demonstrate actual help against another sector's threatening enemies. Retain genuine losses and controlled-fixture limitations.
- Style the arena with existing pinned woodland ground, perimeter vegetation/rocks, restrained objective dressing and readable deployment/ownership cues. Keep the active field clear and the camera/picking geometry stable.
- Complete relevant headless/component/transport/package verification and publish one reviewable PR through the repository's delivery workflow. Record visual checks separately and run them only with explicit graphical authorization.

## Capabilities

### New Capabilities

None; extend the existing capabilities rather than create a competing combat specification.

### Modified Capabilities

- `ecs-unit-combat`: shared squad actors, coordinated movement, independent casualties, physical targeting, logical versus cosmetic death and reproducibility.
- `combat-archetypes`: homogeneous groups, uncapped X/Y contributions, physical weapons, shared profiles and bounded pure-ranged viability.
- `army-roster`: squad home claims, paid reinforcement/regrouping/deployment, individual Hall capacity and retained fallen rosters.
- `combat-status-effects`: actual X/Y victims, individual effects and deterministic squad chill aggregation.
- `coop-city-match`: shared physical defense, nonteleporting redirection, fallen-owner continuation, upkeep and same-architecture bosses.
- `resumable-multiplayer`: complete squad/arena/objective restoration and explicit incompatible-state refusal.
- `city-tabletop`: connected arena, native squad management, coherent formations, camera bounds and a restrained woodland styling pass.
- `game-feedback`: compact individual-health squad summaries and authoritative structure/member feedback across the whole arena.
- `coop-verification`: current squad acceptance, finite funded ranged/cooperative witnesses, evidence scope and cost-aware completion.

## Impact

Existing owners in `src/Game.Core/Combat`, `World`, `ArmyRoster`, `SettlementCommands`, `src/Game/SharedArenaView`, `SquadPanel`, `CombatLayout`, `SharedCombatProjection`, and existing DevRunner campaign/network/component consumers. Existing tests and `docs/shared-arena.md`, `docs/paid-squad-baselines.md`, `docs/testing-audit.md`, gameplay/session/verification documentation need reconciliation. No new dependency, tool installation, asset import, simulator, supervisor or upstream asset work is planned.
