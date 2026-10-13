# Spec Delta

## MODIFIED Requirements

### Requirement: Event-driven combat feedback

Shared-arena feedback SHALL identify individual contributions and actual member/structure targets using archetype-appropriate rigs, projectiles, restrained melee intent/local accents/landed effects and short sounds. Effects SHALL use match-scoped action/event identities and the common presentation clock. Misses SHALL be distinct and grant no landed flashes. Overlapping updates SHALL not duplicate cues; join/resume/history gaps SHALL baseline and reconstruct current moving/dying state without old effects or expired corpses. A casualty's physical pose SHALL freeze while its current death interval advances without logical claims. Keep/Watchpost damage/destruction and owning-squad Rally disruption SHALL use authoritative events/state; Keep fall SHALL not fabricate purchased tower destruction. Shared pause/loss SHALL freeze battle effects and stop voices without historical replay. Camera/owner inspection SHALL not replay foreign backlogs. Headless roles SHALL create no effects/audio.

#### Scenario: Watchpost and Keep feedback
- **WHEN** an accepted update reports a destroyed Watchpost and later a fallen Keep
- **THEN** their damage/fallen states and affected-owner disruption are distinguishable, while retained purchased towers show inactivity

#### Scenario: Repeated impact
- **WHEN** two deliveries contain the same landed contribution
- **THEN** its victim feedback is presented once and no health mutation comes from presentation

#### Scenario: Reconnect after a skeleton casualty
- **WHEN** a client reconnects after a skeleton death occurred
- **THEN** it reconstructs living current state and, only while the authority still retains that dying body, its current death pose without replaying the death rattle or creating an expired historical corpse

#### Scenario: Crowded battle
- **WHEN** many impacts occur together in the observed settlement or shared arena
- **THEN** voice and visual-effect counts remain bounded while all authoritative damage is preserved
- **AND** melee intention remains linked to each current locked target without large opaque beam geometry

#### Scenario: History gap during death
- **WHEN** event history no longer contains the start of a currently unexpired death
- **THEN** the client still reconstructs its elapsed death pose from current state
- **AND** no old impact, death sound or secondary effect is replayed

#### Scenario: Missed sword strike
- **WHEN** a locked sword target group becomes invalid before impact
- **THEN** its current windup intent and rigged swing remain attributable to that action with a distinct miss
- **AND** no landed target-side flash, replacement hit or extra damage is manufactured

### Requirement: Authoritative overhead unit health bars

Shared battle overview SHALL use compact squad summaries exposing living participating count and each member's authoritative current/max HP with owner/type/status context; it SHALL not imply pooled squad health. Detailed individual health/level/service SHALL remain available through member inspection and the roster. Native individual bars used for focused inspection SHALL compute their fraction from actual effective maximum HP, preserve human-readable integer-health scaling and remain input-transparent. Full and wounded members, enemies and one-member bosses SHALL use the same numerical source. Overview labels SHALL be restrained and readable against styled ground at supported sizes without adding independent soldier control.

#### Scenario: Two differently wounded members
- **WHEN** one squad contains members at 10/40 and 30/40 HP
- **THEN** its summary/details expose both injuries rather than only an undifferentiated 40/80 pool

#### Scenario: Effective maximum
- **WHEN** inspection receives 525 current HP and 1050 effective maximum in hundredths
- **THEN** it shows half health immediately without treating current HP as maximum

#### Scenario: Observe a wounded ranked unit
- **WHEN** a living member is first received with current525/max1050HP in hundredths
- **THEN** its summary/focused health display shows half health
- **AND** 525 is not treated as its maximum

#### Scenario: Follow visible allies and enemies
- **WHEN** all-owner living groups move and the viewport changes size
- **THEN** compact health/count/status projections follow their current group/member anchors
- **AND** full-health members and individual levels remain accessible without a pooled-HP interpretation

#### Scenario: Read a veteran's level
- **WHEN** a level-four veteran survives after its trainer level changes
- **THEN** inspection/roster reports that member's level-four profile and actual HP/service
- **AND** no trainer level or obsolete required overhead Roman marker substitutes for its identity

### Requirement: Health bar visibility and lifetime

Only current visible living deployed participants SHALL appear in active arena health summaries. Stored/unfunded/queued members SHALL remain inspectable in rosters without combat health markers or walking bodies. Cosmetic deaths SHALL show no living health indicator. Pause/loss SHALL freeze received health and physical battle anchors while camera/layout changes reproject labels. Accepted reconnect/state replacement SHALL reconstruct current indicators without duplicate/historical damage effects. Session end SHALL release all prior summaries/bars; headless roles SHALL instantiate none.

#### Scenario: Partial funding
- **WHEN** a three-member roster squad enters with two funded field participants
- **THEN** the battle summary counts two participants and roster inspection identifies the retained unfunded third

#### Scenario: Death and camera movement
- **WHEN** a participant dies and the camera pans during its retained cosmetic interval
- **THEN** no living bar remains on the body and the other current summaries reproject without advancing paused gameplay

#### Scenario: Pause after damage
- **WHEN** a visible unit has lost health and the shared match pauses or transport is lost
- **THEN** its health bar and unit presentation retain their last observed health and world-space state without simulated damage or cosmetic progression
- **AND** recovery uses newly synchronized state without replaying old damage

#### Scenario: Move the camera over frozen units
- **WHEN** the player zooms or pans while battle presentation is paused or disconnected
- **THEN** bars reproject above their frozen unit anchors and visibility follows the current world area
- **AND** unit poses, world positions and displayed health remain unchanged

#### Scenario: Observe a casualty
- **WHEN** a running graphical client observes a unit's authoritative death
- **THEN** the unit's living health bar disappears immediately while its bounded death visual can finish
- **AND** no bar remains after the unit visual is removed

#### Scenario: Change the observed city and restart
- **WHEN** the player switches to another city, receives a redistributed enemy, or starts a fresh session
- **THEN** bars correspond only to the current visible living units and their current health
- **AND** no bar is duplicated or retained from the previous city or match
