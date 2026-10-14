# game-feedback Specification

## Purpose

Make building, resource changes and automatic battles perceptible through restrained, synchronized visual and audio feedback on graphical clients.

## Requirements

### Requirement: Authoritative bounded stockpiles
Graphical clients SHALL show separately identifiable gold, food and wood stockpiles near each city's home, using the authored substitutes specified by [city-tabletop](../city-tabletop/spec.md#requirement-requested-free-asset-palette). Each pile SHALL reflect the observed city's authoritative amount through deterministic empty, small, medium and large arrangements with bounded object counts. Zero SHALL show no stored resource objects. Earning enough to cross a visual threshold SHALL grow the pile and spending below one SHALL shrink it; exact amounts SHALL remain visible in the HUD even beyond the largest pile tier. Switching cities and reconnecting SHALL immediately restore the correct piles without replaying historical earning animations. Stockpiles SHALL occupy no building slots and SHALL NOT block plot selection or combat.

#### Scenario: Spend the last wood
- **WHEN** an accepted purchase reduces wood to zero
- **THEN** the wood pile becomes empty and the exact HUD value reads zero
- **AND** a rejected purchase leaves the pile and amount unchanged

#### Scenario: Inspect a wealthy city
- **WHEN** the player views a city whose gold exceeds the largest pile threshold
- **THEN** the arrangement remains bounded and the HUD still shows the full amount

### Requirement: Accepted action feedback
Graphical clients SHALL provide a short construction puff/clunk, upgrade flourish, recruitment cue and research cue only after the corresponding owned action is accepted. Merely selecting a plot, an unaffordable request or a rejected action SHALL NOT play success feedback. A repeated accepted command result SHALL NOT replay its feedback. Feedback SHALL be bounded and SHALL NOT change numerical rules. A fresh session SHALL clear pending action effects.

#### Scenario: Retry a purchase
- **WHEN** the same accepted construction acknowledgement is observed twice
- **THEN** its puff and sound occur once and resources remain deducted once

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

### Requirement: Restrained ambient motion and sound
Graphical scenery SHALL include waving procedural flags and restrained environmental motion or sound appropriate to the visible authored palette; it SHALL NOT fabricate missing source-asset animation. Ambient and battle sound SHALL respect the existing master volume, including complete mute at zero, while continuous background music SHALL preserve its current lifetime and loop. Gameplay-linked ambient motion and effects SHALL freeze during shared pause or transport loss; UI feedback SHALL remain available. Sound sources SHALL be free with recorded provenance or generated locally with documented synthesis parameters, with no runtime downloads or paid asset requirement. Silent automated audio-state checks SHALL NOT be represented as listening-quality verification.

#### Scenario: Mute while effects are active
- **WHEN** the player sets master volume to zero during a visible battle
- **THEN** music, ambience and effect playback are muted without changing match state
- **AND** restoring volume continues music without replaying suppressed battle events

### Requirement: Authoritative overhead unit health bars

Characters, including next-wave forecast figures, SHALL use small text-free lifebars rather than floating names, owners, levels, counts or status text. Each sampled member SHALL own one input-transparent bar with a fraction computed from its authoritative current/effective maximum HP; grouped members SHALL occupy stable compact formation slots without a duplicate squad overlay or pooled HP. Full and wounded allies, enemies and one-member bosses SHALL use the same numerical source. Keep/Watchpost objectives, including the settlement home, SHALL use text-free bars for meaningful authoritative health/max. Purchased invulnerable towers SHALL have no floating text or lifebar; their sentinel health fields SHALL NOT imply damageability or change their mechanics. Panels, member inspection and rosters SHALL retain identity, level, ownership, count, service and status details with human-readable health scaling. Bars SHALL remain readable at supported sizes without adding independent soldier control.

#### Scenario: Two differently wounded members
- **WHEN** one squad contains members at 10/40 and 30/40 HP
- **THEN** its two member bars show one-quarter and three-quarter fill, with exact health available in inspection rather than an undifferentiated 40/80 pool

#### Scenario: Effective maximum
- **WHEN** inspection receives 525 current HP and 1050 effective maximum in hundredths
- **THEN** it shows half health immediately without treating current HP as maximum

#### Scenario: Observe a wounded ranked unit
- **WHEN** a living member is first received with current525/max1050HP in hundredths
- **THEN** its member bar shows half health
- **AND** 525 is not treated as its maximum

#### Scenario: Follow visible allies and enemies
- **WHEN** all-owner living groups move and the viewport changes size
- **THEN** compact health bars follow their current group/member anchors
- **AND** full-health members and individual levels remain accessible without a pooled-HP interpretation

#### Scenario: Read a veteran's level
- **WHEN** a level-four veteran survives after its trainer level changes
- **THEN** inspection/roster reports that member's level-four profile and actual HP/service
- **AND** no trainer level or obsolete required overhead Roman marker substitutes for its identity

### Requirement: Health bar visibility and lifetime

Only current visible living deployed participants SHALL have visible arena member bars. Stored/unfunded/queued members SHALL remain inspectable in rosters without combat health markers or walking bodies. Member/objective health and objective ruins SHALL follow the shared buffered presentation clock, not anticipate pending effective damage or destruction. Cosmetic deaths SHALL immediately hide their member bar at the sampled death boundary; hidden controls MAY remain until their sampled rig is removed. Destroyed or inactive objectives SHALL have no visible bar. Pause/loss SHALL freeze sampled health and physical battle anchors while camera/layout changes reproject bars within the world area. Accepted reconnect/state replacement and history-gap baselines SHALL reconstruct current indicators without duplicate/historical damage effects. Rig removal and session end SHALL release owned bars; headless application roles SHALL instantiate none.

#### Scenario: Partial funding
- **WHEN** a three-member roster squad enters with two funded field participants
- **THEN** only the two funded participants have battle bars and roster inspection identifies the retained unfunded third

#### Scenario: Death and camera movement
- **WHEN** a participant dies and the camera pans during its retained cosmetic interval
- **THEN** no visible bar remains on the body and the other current bars reproject without advancing paused gameplay

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
