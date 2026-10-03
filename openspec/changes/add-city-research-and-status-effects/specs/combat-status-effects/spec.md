# Spec Delta

## Purpose

Make researched burn, poison and chill produce readable, bounded and reproducible combat consequences while preserving authoritative timing and cooperative lifecycle rules.

## ADDED Requirements

### Requirement: Authoritative status application and bounded state
Only a valid landed attack against an eligible living deployed opponent SHALL create researched status applications, once per chosen victim and attack identity. Fire Mage splash SHALL use the existing victim cap/radius and apply burn only to actual victims; Venom Crossbowmen SHALL apply poison to their primary; Frost Mages SHALL apply chill to actual victims. Misses, cosmetic collision, city-health hits, snapshots and repeated observation SHALL not apply statuses. Status damage/strength, tick periods, durations and caps SHALL come from validated immutable rules; unsafe arithmetic, invalid duration/period and unbounded caps SHALL reject before a match starts. A target SHALL have at most one burn, three poison stacks and one chill. Equivalent inputs SHALL produce canonically ordered status state and events independent of entity storage or rendering cadence.

#### Scenario: Status follows actual splash victims
- **WHEN** a Fire Mage lands a valid capped splash attack inside a larger enemy cluster
- **THEN** only the chosen damage victims gain burn once
- **AND** an invalid primary produces no damage or burn on its former neighbors

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
Chill SHALL reduce movement speed and attack cadence through a bounded positive percentage increase to new action durations. Reapplication SHALL use the greatest existing/incoming slow and later expiry without stacking percentages. At each new move/attack commitment the authority SHALL sample current chill and calculate positive integer durations with declared upward rounding; movement, windup and recovery SHALL use the same duration multiplier. The committed deadlines SHALL then remain unchanged if chill is applied, refreshed or expires. Chill SHALL expire before new action selection on its expiry tick. It SHALL neither interrupt an attack nor immobilize/freeze/stun a unit, alter initiative/range/size, or create new reservations outside the ordinary movement admission rules.

#### Scenario: Chill arrives during movement or windup
- **WHEN** chill is applied to a unit with an already committed move or attack
- **THEN** that action retains its original arrival, impact and recovery deadlines
- **AND** a later action started while chilled uses the configured longer durations

#### Scenario: Expiry during a slowed action
- **WHEN** chill expires after a slowed attack has started
- **THEN** the committed attack keeps its declared interval and the following eligible attack uses the unchilled timing

### Requirement: Periodic damage and status lifecycle
Due periodic damage SHALL join ordinary unit/defense damage against a common pre-damage state before casualties. A unit killed by periodic damage that tick SHALL still deliver an otherwise valid already-due impact, and SHALL start no later action. New statuses SHALL commit only to survivors after accumulated damage, so no application revives a casualty. Only actual health reduction SHALL reset existing engagement progress; refreshes and chill alone SHALL not reset it. Death SHALL remove active status scheduling while retaining sufficient final state for presentation. Enemy redistribution SHALL preserve statuses, captured potency and absolute deadlines without adopting recipient technologies or granting free ticks. Already afflicted transferred queued enemies SHALL continue periodic damage/expiry without becoming attack targets or acquiring spatial claims. Fresh undeployed recruits and unfed reserves SHALL start without effects. Wave resolution SHALL clear surviving temporary effects without healing and cleanup SHALL not deliver post-result status damage.

#### Scenario: Periodic damage and mutual lethal impacts
- **WHEN** poison and ordinary impacts are due on a tick with mutually lethal contributions
- **THEN** all valid due contributions resolve before casualties regardless of storage order
- **AND** the normal defeat/victory precedence is retained

#### Scenario: Transfer while poisoned and queued
- **WHEN** an afflicted enemy transfers and waits for destination deployment space
- **THEN** its identities, stacks and deadlines persist and due poison can reduce its health in the destination allocation
- **AND** it remains untargetable and occupies no space while queued, with no duplicate tick on admission

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
