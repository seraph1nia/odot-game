# Spec Delta

## MODIFIED Requirements

### Requirement: Authoritative status application and bounded state

Only a landed contribution causing positive direct damage to an eligible living participating member SHALL create that contributor's researched on-hit application once per member/action identity. Fire and Frost SHALL follow actual positive X/Y victims; Venom SHALL follow its actual damaged members. A zero-damage primary SHALL receive no effect from that contribution. Structure damage, misses, cosmetic collisions, repeated observations and snapshots SHALL grant no member statuses. Rules SHALL validate potency/period/duration/caps and safe arithmetic; each member SHALL retain at most one burn, three poison stacks and one chill. Canonical multi-source resolution SHALL be independent of traversal and render cadence.

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
