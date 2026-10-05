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
Visible combat SHALL use archetype-appropriate sword/axe impacts, ranged projectiles, magic sparks, skeleton rattles and tower effects with short sounds. Melee feedback SHALL supply restrained explicit attacker-to-target windup intent, a short local sword/axe strike accent and landed target-side impact, including far-side adjacent-hex occupants; it SHALL NOT depict an opaque continuous hand-to-target beam or imply physical weapon reach. Misses SHALL remain visually distinct from landed hits and SHALL NOT show landed target-side impact. Effects SHALL use match-scoped action/event identity and the same presentation clock as unit poses. Overlapping snapshots SHALL NOT duplicate effects. Initial admission, reconnect or event-history gaps SHALL baseline the cursor without replaying old hits, deaths or sounds. Current moving units and unexpired dying bodies SHALL be restored from complete state at their authoritative elapsed progress; an expired corpse SHALL NOT be recreated from event history. Restoring a current death pose SHALL NOT restart its interval or play its historical rattle. Pause or transport loss SHALL freeze battle effects and stop active combat sound voices without resuming historical sounds on recovery. Switching observed cities SHALL NOT play the other city's backlog. Death SHALL override incompatible living actions; visual projectiles SHALL remain cosmetic. Headless roles SHALL create no audio players or visual effects.

#### Scenario: Reconnect after a skeleton casualty
- **WHEN** a client reconnects after a skeleton death occurred
- **THEN** it reconstructs living current state and, only while the authority still retains that dying body, its current death pose without replaying the death rattle or creating an expired historical corpse

#### Scenario: Crowded battle
- **WHEN** many impacts occur together in the observed city
- **THEN** voice and visual-effect counts remain bounded while all authoritative damage is preserved
- **AND** melee intention remains linked to each current locked target without large opaque beam geometry

#### Scenario: History gap during death
- **WHEN** event history no longer contains the start of a currently unexpired death
- **THEN** the client still reconstructs its elapsed death pose from current state
- **AND** no old impact, death sound or secondary effect is replayed

#### Scenario: Missed sword strike
- **WHEN** a locked sword primary becomes invalid before impact
- **THEN** its current windup intent and rigged swing remain attributable to that action but its miss cue is distinguishable from a landed strike
- **AND** no landed target-side flash or extra damage is manufactured

### Requirement: Restrained ambient motion and sound
Graphical scenery SHALL include waving procedural flags and restrained environmental motion or sound appropriate to the visible authored palette; it SHALL NOT fabricate missing source-asset animation. Ambient and battle sound SHALL respect the existing master volume, including complete mute at zero, while continuous background music SHALL preserve its current lifetime and loop. Gameplay-linked ambient motion and effects SHALL freeze during shared pause or transport loss; UI feedback SHALL remain available. Sound sources SHALL be free with recorded provenance or generated locally with documented synthesis parameters, with no runtime downloads or paid asset requirement. Silent automated audio-state checks SHALL NOT be represented as listening-quality verification.

#### Scenario: Mute while effects are active
- **WHEN** the player sets master volume to zero during a visible battle
- **THEN** music, ambience and effect playback are muted without changing match state
- **AND** restoring volume continues music without replaying suppressed battle events

### Requirement: Authoritative overhead unit health bars
Every visible, deployed, living friendly and enemy unit in the observed city SHALL show a compact Trio-styled health bar above its model, including units at full health. The fill SHALL represent current authoritative health divided by that unit's authoritative maximum health, clamped to the zero-to-one range, using the same health scale for both values. Maximum health SHALL include the unit's effective archetype and research rank rather than the health first observed by this client. Bars SHALL remain readable against the battlefield at supported window sizes and follow unit movement and camera/layout changes. Overhead unit names, role codes such as S L1, enemy prefixes and BOSS labels SHALL NOT be shown. Each bar SHALL show the authoritative unit level as a conventional Roman numeral immediately above its left part, not the research rank or recruitment-building level. Numerals SHALL remain readable and follow the same projection and visibility rules as the bar. Full name, role, faction and boss identification SHALL remain available through click inspection and army details independently of interpreting health-fill color. Bars SHALL be cosmetic and SHALL NOT intercept input, alter combat or introduce manual unit control.

#### Scenario: Observe a wounded ranked unit
- **WHEN** a client first receives or reconnects to a living unit with current health 525 and authoritative maximum health 1050 in the wire's integer health scale
- **THEN** its overhead bar shows half health immediately
- **AND** the client does not treat 525 as that unit's full health

#### Scenario: Follow visible allies and enemies
- **WHEN** living friendly and enemy units move in the focused city and the window changes between supported sizes
- **THEN** each bar stays above its corresponding visible model and retains a readable size
- **AND** full-health units also show a full bar with a Roman level marker, overhead names/codes are absent, and faction/role identification remains available in click inspection and army details

#### Scenario: Read a veteran's level
- **WHEN** a level-four veteran remains in the army after its recruitment building changes level
- **THEN** its health bar has IV immediately above the left part, with no overhead unit name or role code
- **AND** inspection uses that veteran's own level and authoritative current/max health and damage

### Requirement: Health bar visibility and lifetime
Health bars SHALL reflect the same observed unit state and presentation lifetime as their models. Shared pause or transport loss SHALL freeze their last observed health and world-space anchors with battle presentation until synchronization resumes, while screen placement and visibility SHALL continue to follow local camera and layout changes. Units outside the observed city, undeployed arrivals, units behind the camera or outside its visible world area, and death visuals SHALL NOT show living health bars. Switching cities, reconnecting and enemy redistribution SHALL reconstruct the correct visible bars from current state without duplicates or historical damage playback. Session end and fresh-match transitions SHALL release previous bars, with the number of live bars bounded by visible living units. Headless roles SHALL create no health-bar presentation.

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
