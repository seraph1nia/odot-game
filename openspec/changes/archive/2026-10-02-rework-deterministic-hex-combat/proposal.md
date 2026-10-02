# Proposal

## Why

The current autobattle uses continuous body-contact geometry, persistent targets and immediate removal of dead bodies, leaving hex capacity, movement commitment and tie ordering undefined. Reinforcement entry, automatic formation and readable melee also need explicit contracts before the new spatial rules are integrated. Replace it with a seeded, discrete combat model whose positioning, action timing and space release can be reproduced and tuned without depending on rendering.

## What Changes

- **BREAKING:** Replace longitudinal/lateral collision-based combat with bounded hex graphs, fixed positions within each hex, and capacity consumed by a unit-type attribute. Allies may share a hex; opposing factions may not share its occupied or reserved positions.
- Protect a deployment row for each faction permanently against opposing occupation/reservation, while keeping deployed occupants attackable. Reinforcements entering a previously cleared board must deploy after at most bounded existing death cleanup, even with defenders at the forward entrance.
- Specify formation: reserve rear placement for support, spread melee across the forward columns before filling them, queue overflow, and use legal protected-entry fallback during combat. Units can converge on the nearest reachable opponent without permanent lane assignments.
- Depict adjacent-hex melee as abstract tabletop combat with an explicit attacker-to-target strike cue and target-side impact, including far-side occupants. Validate actual models in two occupied adjacent hexes before accepting tile scale/anchors.
- Add character-type initiative, closest-target selection at action boundaries, and seeded tie resolution for targets, movement scheduling and equivalent routes. The same canonical setup, seed, rules version and ordered gameplay inputs reproduce the same combat trace.
- Make one-hex movement a timed committed action reserving source and destination positions. A unit remains attackable at its source until arrival; movement-route conflicts also use discrete reservations.
- Configure integer move, windup, recovery and death durations at the existing 60 Hz simulation rate. Hold through windup/recovery, accumulate simultaneous impacts, and retain dead-unit reservations until the authoritative death deadline.
- Use graph queries and occupancy revisions for coordinated approach and bounded waits. A deterministic no-progress or duration limit ends the entire match as defeat with an explicit battle-stalled reason, without altering city health or inventing casualties.
- **BREAKING:** Version the combat snapshot/event contract to carry seeds, board/configuration identity, action intervals, reservations, current dying bodies and terminal reasons. Reconnect reconstructs current action/death progress without replaying historical effects.
- Add comparative frontline/support, mixed access and single-target-versus-clustered Mage/Crossbowman acceptance. Mage tuning must show a real two-victim splash benefit, not merely appear in a winning mixed army; preserve equal faction profiles.
- Put a gameplay gate after minimal occupancy/actions and before broad protocol/reconnect/presentation work: ordinary three-wave strategies, blocked reinforcement admission, crowded mixed armies, role comparisons and one representative rendered engagement. Reuse existing checks and keep final tuning/full CI at completion.
- Adapt combat presentation, existing verification and documented strategies to the new rules, preserving cooperative allocation, redistribution, pause and session ownership.

## Capabilities

### New Capabilities

None; extend the existing combat and match capabilities.

### Modified Capabilities

- `ecs-unit-combat`: Hex occupancy, committed movement/death reservations, initiative and seeded decisions, action scheduling, reproducibility, protected deployment, specified formation and bounded progression.
- `combat-archetypes`: Symmetric capacity/initiative/timing profiles, hex-distance splash and consistent defense targeting and measurable role contributions.
- `coop-city-match`: Separate combat hexes from building capacity, retain automatic defense and transfers, and define battle-stalled defeat and post-wave cleanup.
- `city-tabletop`: Render fixed combat positions, explicit abstract melee and timed movement/death state; expose the correct defeat reason.
- `game-feedback`: Restore current dying bodies without replaying old death effects or creating expired corpses.
- `coop-verification`: Verify capacity and seeded traces cheaply, and extend existing network/graphical slices for reservation, timing and reconstruction defects, with an early gameplay/visual gate.

## Impact

Numerical changes belong in `src/Game.Core/Combat`, `World.cs`, `Catalogs.cs`, `AuthoritySession.cs` and combat DTOs/protocol. Presentation changes affect `CombatPlayback`, `UnitView`, `Tabletop`, layout mapping and feedback; verification affects core/authority/strategy tests and the existing DevRunner combat, reconnect, redistribution and package observations.

No engine/SDK upgrade, dependency addition, asset acquisition, save system, manual unit commands, terrain bonuses or seed-selection UI is proposed. Existing camera and countryside changes remain separate: this change supplies authoritative combat bounds/anchors and reconciles consumers with their final layout interfaces. The wire change intentionally refuses older peers; no in-flight match migration is required.
