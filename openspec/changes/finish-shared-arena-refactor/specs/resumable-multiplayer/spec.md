# Spec Delta

## MODIFIED Requirements

### Requirement: Complete authoritative resynchronization

Initial join/resume SHALL deliver complete compatible current state before gameplay input or reconnect credentials are enabled/replaced. State SHALL include original roster/connectivity, each city's slots/economy/investment/research/upkeep/health, connected arena/sectors/static footprints, squads with separate member/participant IDs, persistent homes, formation slots, individual HP/equipment/service/status/eligibility, physical structures/weapons, current actions/targets/claims/deadlines, seed/full rules identity, battle-flow identity/speed, authoritative next-wave forecast, phase/readiness/pause/outcome/rewards and current cosmetic deaths. Revisions SHALL increase monotonically within a match, including paused revisions. Welcome, acknowledgment and normal delivery SHALL use the same compatibility validation. Stale/inconsistent projections SHALL not partially replace live state.

Bounded match-scoped sequenced events SHALL be deduplicated. Join/resume/history gaps SHALL reconstruct current state without replaying effects, inventing damage, resurrecting actors or expanding history. Protocol 15/combat rules 7/battle flow 2 SHALL refuse both incompatible predecessor protocol-14 schemas and unversioned/legacy size-budget/splash snapshots. Missing arena/forecast, duplicate claims, invalid squad/member/formation identity or mismatched configuration SHALL fail before binding gameplay or replacing credentials. Failure SHALL explain compatible rejoin/fresh-match recovery. Resume files SHALL be credentials for the same running authority, not migrated saved matches.

#### Scenario: Fallen spectator reconnect
- **WHEN** an original fallen owner resumes the same running host during pause
- **THEN** the same stored/inactive/current-fighting member identities, injuries and shared battle state restore without revival, payment or extra roster allocation

#### Scenario: Old schema
- **WHEN** a predecessor protocol-14 peer or legacy compressed combat state attempts admission
- **THEN** it is refused clearly without changing the legitimate live state or reconnect credentials

#### Scenario: History gap
- **WHEN** a client restores a current moving/dying member after missing the event window
- **THEN** its current group cycle and elapsed pose restore without historical shots/hits/death sounds

#### Scenario: Duplicate delivery
- **WHEN** acknowledgment and ordinary delivery contain overlapping events or an older revision
- **THEN** the newest accepted state remains and each visible effect is consumed at most once

#### Scenario: Reconnect after match progression
- **WHEN** turns or combat progressed while a player was absent
- **THEN** their resumed client displays the current authoritative state, including any losses or transfers, rather than the state from disconnection

#### Scenario: Reconnect during pause
- **WHEN** a player resumes while the match is paused
- **THEN** they receive the paused state and current connection/readiness state even though combat time has not advanced

#### Scenario: Snapshot interval contains a complete action
- **WHEN** an attack impact or casualty occurs between two snapshots received by an already synchronized client
- **THEN** retained event identity and timing let it present that action or death once even if the unit's current action or living presence has already changed

#### Scenario: Duplicate or out-of-order combat state
- **WHEN** a client receives overlapping recent events or an older state revision
- **THEN** it keeps its newest authoritative state and does not replay already consumed strikes, shots, hits or deaths

#### Scenario: Resume after the event window
- **WHEN** a client resumes after missing more combat than the retained event history covers
- **THEN** it receives current living units, profiles, positions and in-progress actions without replaying a historical effect backlog
- **AND** missing historical events do not affect health or outcomes

#### Scenario: Old combat protocol attempts admission
- **WHEN** a build without the compatible typed-recruitment and combat-state protocol attempts to join
- **THEN** the authority refuses clearly before binding that connection to gameplay

#### Scenario: Reconnect restores research and periodic effects
- **WHEN** a city reconnects after earning points and choosing Fire while a current enemy remains poisoned
- **THEN** the complete state restores balances, partial progress, purchased nodes, Frost lock and the enemy's current stack deadlines before enabling input
- **AND** the client neither grants points nor reapplies historical poison damage
