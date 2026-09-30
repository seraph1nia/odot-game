# Spec Delta

## MODIFIED Requirements

### Requirement: Fixed roster and personal cities
The game SHALL offer a lobby for up to four players and an explicit start action that locks the current roster. Each roster member SHALL begin with an independent city of exactly nine empty indexed building slots, positive city health, starting gold, and a built-in ranged defender outside those slots. Slot identities SHALL remain stable across graphical placement, upgrades, observation, and reconnects; the slots SHALL NOT require square geometry. Hexagonal terrain presentation SHALL NOT add adjacency bonuses, terrain movement rules, or additional building capacity. One player SHALL be sufficient for local testing; the match SHALL support at least two simultaneous players. Fresh players SHALL NOT join an already started match, but roster members SHALL be able to reconnect.

#### Scenario: Start a cooperative match
- **WHEN** a connected lobby member starts a match with two players present
- **THEN** both players receive distinct cities with nine available slots and the same configured starting resources and city health
- **AND** the first building turn starts with the roster locked

#### Scenario: New arrival after start
- **WHEN** a client without valid credentials for a roster member joins after the match starts
- **THEN** it receives a clear refusal and no new city or wave allocation is created

#### Scenario: Resume a city with hexagonal plots
- **WHEN** a roster member resumes a city containing buildings and upgrades
- **THEN** all nine authoritative slot identities retain their buildings and levels at the corresponding graphical plots
- **AND** surrounding decorative terrain provides no extra building slots or gameplay effects
