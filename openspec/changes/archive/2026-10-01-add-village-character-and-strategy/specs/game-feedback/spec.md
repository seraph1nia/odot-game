# Spec Delta

## Purpose

Make building, resource changes and automatic battles perceptible through restrained, synchronized visual and audio feedback on graphical clients.

## ADDED Requirements

### Requirement: Authoritative bounded stockpiles
Graphical clients SHALL show separately identifiable gold bars, food sacks/grain and wood piles near each city's home, using free assets. Each pile SHALL reflect the observed city's authoritative amount through deterministic empty, small, medium and large arrangements with bounded object counts. Zero SHALL show no stored resource objects. Earning enough to cross a visual threshold SHALL grow the pile and spending below one SHALL shrink it; exact amounts SHALL remain visible in the HUD even beyond the largest pile tier. Switching cities and reconnecting SHALL immediately restore the correct piles without replaying historical earning animations. Stockpiles SHALL occupy no building slots and SHALL NOT block plot selection or combat.

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
Visible combat SHALL use archetype-appropriate sword/axe impacts, crossbow projectiles, magic sparks, skeleton rattles and tower effects with short sounds. Effects SHALL use match-scoped action/event identity and the same presentation clock as unit poses. Overlapping snapshots SHALL NOT duplicate effects. Initial admission, reconnect or event-history gaps SHALL baseline the cursor without replaying old hits, deaths or sounds. Pause or transport loss SHALL freeze battle effects and stop active combat sound voices without resuming historical sounds on recovery. Switching observed cities SHALL NOT play the other city's backlog. Death SHALL override incompatible living actions; visual projectiles SHALL remain cosmetic. Headless roles SHALL create no audio players or visual effects.

#### Scenario: Reconnect after a skeleton casualty
- **WHEN** a client reconnects after a skeleton death occurred
- **THEN** it reconstructs living current state without replaying the death rattle or creating a historical corpse

#### Scenario: Crowded battle
- **WHEN** many impacts occur together in the observed city
- **THEN** voice and visual-effect counts remain bounded while all authoritative damage is preserved

### Requirement: Restrained ambient motion and sound
Graphical scenery SHALL include waving flags, turning windmills and restrained environmental motion or sound appropriate to the visible village. Ambient and battle sound SHALL respect the existing master volume, including complete mute at zero, while continuous background music SHALL preserve its current lifetime and loop. Gameplay-linked ambient motion and effects SHALL freeze during shared pause or transport loss; UI feedback SHALL remain available. Sound sources SHALL be free with recorded provenance or generated locally with documented synthesis parameters, with no runtime downloads or paid asset requirement. Silent automated audio-state checks SHALL NOT be represented as listening-quality verification.

#### Scenario: Mute while effects are active
- **WHEN** the player sets master volume to zero during a visible battle
- **THEN** music, ambience and effect playback are muted without changing match state
- **AND** restoring volume continues music without replaying suppressed battle events
