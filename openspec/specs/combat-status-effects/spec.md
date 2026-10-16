# combat-status-effects Specification

## Purpose

Make researched burn, poison and chill produce readable, bounded and reproducible combat consequences while preserving authoritative timing and cooperative lifecycle rules.

## Requirements

### Requirement: Authoritative status application and bounded state

Only a landed contribution causing positive direct damage to an eligible living participating member SHALL create that contributor's enabled on-hit application once per member/action identity. [Personal status cards](../../../docs/roguelite.md#all-36-reward-cards) MAY enable applications without the corresponding research branch. Fire and Frost SHALL follow actual positive X/Y victims; Venom SHALL follow its actual damaged members. A zero-damage primary SHALL receive no effect from that contribution. Structure damage, misses, cosmetic collisions, repeated observations and snapshots SHALL grant no member statuses. Rules SHALL validate potency/period/duration/caps and safe arithmetic; each member SHALL retain at most one burn, three poison stacks and one chill. Canonical multi-source resolution SHALL be independent of traversal and render cadence.

#### Scenario: Zero-X group attack
- **WHEN** a researched strike with zero X and positive Y lands on a living group
- **THEN** only positively damaged secondary members receive its effect and the zero-damage primary does not

#### Scenario: Miss
- **WHEN** the locked group has no eligible participant at impact
- **THEN** no direct damage or status is applied to another group

#### Scenario: Status follows actual splash victims
- **WHEN** a Fire Mage lands uncapped X/Y contributions on an enemy group
- **THEN** only positively damaged members receive burn once per contributing identity
- **AND** zero-damage primaries and neighboring groups receive none

#### Scenario: Bounded multi-source applications
- **WHEN** many researched units apply effects to one target in the same tick
- **THEN** its state remains inside the declared burn/poison/chill caps
- **AND** reversed traversal yields identical strengths, deadlines, stacks and events

### Requirement: Refreshable burn
Burn SHALL deal positive periodic damage for a short configured lifetime. Reapplication SHALL retain one effect, use the greater existing/incoming potency, extend expiry to the later deadline and preserve the existing next damage deadline. It SHALL NOT add independent damage streams or delay a scheduled tick. Initial application SHALL schedule its first damage after one full period. A final periodic tick due exactly at expiry SHALL resolve before removal. Periodic damage SHALL retain its captured potency after its source dies or transfers and SHALL NOT recursively apply statuses.

#### Scenario: Repeated fire attacks
- **WHEN** several fire attacks refresh an already burning enemy before its next damage tick
- **THEN** it retains one burn and the same next tick, with the greatest applied potency
- **AND** no refresh produces immediate extra damage

#### Scenario: Terminal burn tick
- **WHEN** a burn's next damage tick and expiry are the same authority tick
- **THEN** that final scheduled damage is accumulated once before the effect is removed

### Requirement: Capped independently expiring poison
Poison SHALL provide a longer periodic damage lifetime than burn and at most three independently timed stacks. A hit below the cap SHALL create one stack with captured potency, source/action identity, expiry and first damage deadline. At the cap a hit SHALL refresh the earliest-expiring stack, breaking equal expiry by stable stack identity, preserve that stack's next damage deadline and retain the greater existing/incoming potency. Refresh SHALL not postpone other stacks, cause immediate extra ticks or increase the cap. Each stack SHALL expire independently after its final eligible scheduled damage; a due tick at exact expiry SHALL precede removal. Poison damage SHALL continue after its source dies and SHALL not recursively apply another effect.

#### Scenario: Fourth poison hit
- **WHEN** a fourth poison application reaches a target with three active stacks
- **THEN** it refreshes the canonically earliest-expiring stack without creating a fourth
- **AND** every existing next damage deadline remains unchanged

#### Scenario: Source death and independent expiry
- **WHEN** a poisoning Crossbowman dies while its target has stacks with different expiries
- **THEN** those stacks retain captured potency and scheduled damage until their individual expiry or target death

### Requirement: Chill affects new actions without hard control

Individual chill SHALL remain inspectable with bounded strength and expiry. Each new squad move/attack SHALL sample the rounded-up arithmetic mean of current chill percentages across living participating members, including healthy members' zero values. The resulting bounded multiplier SHALL use declared upward rounding for positive movement, windup and recovery durations. Committed deadlines SHALL not change when a member dies or chill arrives/expires. Expired chill SHALL be absent before new decisions. Chill SHALL not interrupt, freeze, stun, change range/initiative/capacity or acquire extra claims. Reapplication SHALL retain the greater member strength and later expiry without stacking percentages.

#### Scenario: One chilled member
- **WHEN** a three-participant squad has one member at 30 percent chill and two at zero
- **THEN** a new squad action samples 10 percent rather than 30 percent

#### Scenario: Existing action
- **WHEN** chill changes after a move or attack was committed
- **THEN** that action retains its arrival/impact/recovery deadlines and only later commitments resample

#### Scenario: Chill arrives during movement or windup
- **WHEN** chill is applied to a unit with an already committed move or attack
- **THEN** that action retains its original arrival, impact and recovery deadlines
- **AND** a later action started while chilled uses the configured longer durations

#### Scenario: Expiry during a slowed action
- **WHEN** chill expires after a slowed attack has started
- **THEN** the committed attack keeps its declared interval and the following eligible attack uses the unchilled timing

### Requirement: Periodic damage and status lifecycle

Due individual periodic damage SHALL accumulate with valid due group/weapon impacts before casualties. An otherwise valid due strike SHALL survive same-tick lethal periodic damage; casualties SHALL start no later action. New effects SHALL apply only to surviving victims. Only actual member/structure HP loss SHALL count as health progress. Member death SHALL stop its scheduling; fresh recruits/unfunded reserves SHALL have no invented effects. Changing an enemy squad's preferred sector SHALL retain member statuses, captured potency and absolute deadlines without teleportation, recipient technology or extra ticks. Wave resolution SHALL clear surviving temporary effects without healing; cleanup SHALL deliver no post-result status damage.

#### Scenario: Poisoned enemy redirects
- **WHEN** a poisoned enemy squad's preferred Keep falls
- **THEN** its member's physical pose, poison stacks and next damage deadlines remain unchanged while future objective preference redirects

#### Scenario: Result and cleanup
- **WHEN** a shared wave ends with afflicted survivors and unexpired bodies
- **THEN** temporary effects clear without healing and later cosmetic cleanup creates no status damage

#### Scenario: Periodic damage and mutual lethal impacts
- **WHEN** poison and ordinary impacts are due on a tick with mutually lethal contributions
- **THEN** all valid due contributions resolve before casualties regardless of storage order
- **AND** the normal defeat/victory precedence is retained

#### Scenario: Transfer while poisoned and queued
- **WHEN** an afflicted enemy squad redirects preferred sector or an explicitly queued actor awaits legal entry
- **THEN** absolute individual stacks/deadlines persist without another tick or recipient technology
- **AND** redirection never moves the actor; an actually queued actor remains untargetable and claim-free

#### Scenario: Wave end and no-progress clock
- **WHEN** a shared wave completes with surviving afflicted allies, or chill repeatedly refreshes without health reduction
- **THEN** wave resolution clears temporary effects without healing
- **AND** chill-only activity does not extend the existing no-health-progress deadline

### Requirement: Complete status restoration and paused clocks
Current snapshots SHALL expose active effect kinds, strengths, stack identities, source/captured potency where relevant, application/expiry/next-damage deadlines and effective action timings sufficient to reconstruct the current state without replaying historical applications. Pause SHALL freeze all simulation-driven effect deadlines, damage and progress; resume SHALL perform no wall-clock catch-up. Reconnect, duplicate/out-of-order snapshots and history gaps SHALL reconstruct active indicators and timing without replaying old damage or sounds. Clients SHALL never own status scheduling or combat mutation.

#### Scenario: Reconnect while paused and poisoned
- **WHEN** a client reconnects during a paused poisoned enemy's stack lifetime
- **THEN** it reconstructs current stacks and their remaining simulation deadlines with unchanged health
- **AND** resume processes subsequent due damage only once
