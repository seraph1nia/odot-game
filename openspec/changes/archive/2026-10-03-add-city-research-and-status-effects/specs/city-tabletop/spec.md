# Spec Delta

## MODIFIED Requirements

### Requirement: City interaction and match feedback
Clients SHALL expose construction on empty purchased plots, locked-plot purchases, building upgrades and sales, explicit building-specific archetype recruitment, independent technology research, Market sales, ready/unready, and pause/resume through a compact control panel anchored to the bottom of the game window. The interface SHALL show complete construction/upgrade/recruitment/research costs, building refund and Market quotes, gold, food, wood, stone, metal and cloth in a separate top-right resource table; next expansion cost only with a selected locked plot; current army upkeep demand, food forecast, purchased technologies and soldier details through compact contextual inspection; city health at the home through a percentage health bar; building level in occupied-plot details; phase including final preparation through a small vertical stage indicator with compact turn and wave counters and boss indication; readiness through Ready/Unready; and exceptional connection status and outcome, distinguishing all-cities-fallen defeat from battle-stalled defeat. A stalled result SHALL explain that the battle could not finish, rather than claim every city fell. City inspection controls SHALL identify the player's own city and the observed city using a very small `< [YOU] >` selector in the existing city-navigation location, with text left/right buttons and a concise player identifier when observing another city. Cycling SHALL visit roster cities in stable player order, wrap at either end, clear plot selection and fit the new city overview; arrows SHALL be disabled when only one city is available. Lobby roster/host/readiness information SHALL remain available in the lobby without restoring the verbose player bar during gameplay. Construction, plot purchase, building sale, upgrade, Market trade and recruitment actions SHALL be contextual to the directly selected world plot or building; without a selection, the panel SHALL NOT offer an actionable default plot. A separate Research control SHALL open the observed city's technology tree without a plot selection or owned Research Tower; only eligible own-city purchases SHALL be enabled. Routine select-a-plot and Open plot X / choose a building prompts SHALL be omitted; meaningful disabled-action reasons and occupied-building details SHALL remain accessible. The interface SHALL NOT include a duplicate numbered grid for selecting building slots. A cooperative lobby SHALL expose the current roster, identify the host where applicable, and expose the start action only to eligible players. Hosted lobbies SHALL offer Invite friends; solo play SHALL enter the first building turn directly. Lobby, match, outcome, and disconnected views SHALL offer Return to menu through Settings and a cancelable confirmation, with no Return to menu control in the bottom HUD. The bottom HUD SHALL omit ODOT, P1/Your City headings, City/Army summary rows, purchased land counts, resource summary rows, the verbose P1/You/Here bar and routine Match started text. Server errors, actionable feedback, pause/disconnection and outcomes SHALL remain visible when relevant. Connection and host-loss feedback SHALL reflect the selected session mode. Unavailable actions SHALL be visibly disabled or explained, and server rejection SHALL be surfaced. A disconnected guest SHALL offer reconnect without discarding its private resume information while the original authority may still be available, and input SHALL remain disabled until resynchronization completes. An ended hosted session SHALL offer return to the start screen without suggesting authority migration.

#### Scenario: Recruit through the graphical interface
- **WHEN** a player clicks their recruitment building during an eligible building or preparation phase
- **THEN** the bottom panel displays its level and explicit recruitment actions for each archetype that building unlocks and its authoritative recruit level, resulting health/damage, rounded material/gold costs and separate per-battle food upkeep
- **AND** activating recruitment shows the authoritative resource and army changes after acceptance

#### Scenario: Pause and reconnect feedback
- **WHEN** the match is paused and a client reconnects
- **THEN** the compact match view identifies pause and the observed city, keeps roster connectivity accessible through inspection, and shows a resume control
- **AND** it does not display frozen gameplay as active progression

#### Scenario: No plot has been selected
- **WHEN** a synchronized player first views a city or switches to another city
- **THEN** the panel omits routine selection instructions and no economic action can target a previously selected plot
- **AND** resources, city inspection, and available match controls remain accessible

#### Scenario: Inspect another player's city
- **WHEN** a player switches to another city's view and selects a building there
- **THEN** contextual controls show the observed city's building information, the top-right table shows its resources, and spending or recruitment on that city is visibly prevented

#### Scenario: Recruit a ranged soldier through controls
- **WHEN** the player selects their Archery Range and activates its enabled Crossbowman control
- **THEN** the accepted request adds a Crossbowman and updates the resource table with the authoritative material deduction and army inspection with the new soldier without a food recruitment charge
- **AND** observing a foreign recruitment building, becoming ready, pausing or entering combat disables its recruitment controls

#### Scenario: Inspect research and preparation
- **WHEN** a player opens Research in final preparation without selecting a plot
- **THEN** the research panel shows points, progress, technologies, prerequisites, exclusive locks and authoritative point costs
- **AND** Ready is labeled to start battle rather than produce resources

#### Scenario: Preview a recruitment-building upgrade
- **WHEN** the player selects a recruitment building below level five
- **THEN** the panel shows its current recruit level, next level, upgrade cost and the next level's material recruitment price, health/damage and per-battle upkeep
- **AND** it explains that existing soldiers retain their level, shows no food-discount claim and disables an unavailable upgrade

#### Scenario: Inspect maximum recruitment level
- **WHEN** the player selects a level-five recruitment building
- **THEN** recruitment remains available under normal eligibility while upgrading is shown as complete
- **AND** a level-two production building or tower and a level-one Market remain at their own maxima


#### Scenario: Stalled defeat is truthful
- **WHEN** the authority reports defeat due to a combat limit while city health remains positive
- **THEN** the panel displays the battle-stalled reason and Settings offers confirmed return to menu
- **AND** it does not describe surviving cities as fallen

### Requirement: Readable unit progression and wave rewards
The graphical match view SHALL expose an observed army's unit levels as Roman numerals above the left part of their health bars, with full names, descriptions, faction, boss identification and level in click inspection and army details rather than overhead name/role codes. Displayed health and damage SHALL use authoritative resolved profiles. Army inspection SHALL show authoritative unit size, with normal units at size two and bosses at size six. Mixed-level soldiers SHALL remain distinguishable through Roman level markers or grouped army details without adding individual upgrade controls. The view SHALL show the latest completed wave's actual gold, food, wood and research reward for the observed city, including on the final victory screen. A summary SHALL identify its wave and SHALL NOT imply a second payment on repeated snapshots, city switches or reconnect. A fresh match SHALL clear the prior match's reward summary. Unit levels SHALL NOT require new character assets or modify unit size through visual scaling.

#### Scenario: Observe a boss
- **WHEN** the observed city receives a boss allocation or a transferred boss
- **THEN** the overhead bar shows its Roman level and click inspection identifies it as a boss with its level, authoritative current/maximum health, damage per attack and size six

#### Scenario: Restore the reward summary
- **WHEN** a player reconnects after wave ten clears
- **THEN** the current balances and latest summary show that city's actual wave-ten reward without playing another payment effect

#### Scenario: View mixed-level survivors
- **WHEN** level-one veterans and level-three recruits belong to the observed army
- **THEN** army details distinguish their levels and use their own profiles rather than displaying all soldiers at the current building level

## ADDED Requirements

### Requirement: Readable personal research tree
The research panel SHALL show the observed city's whole points, partial production progress, tower contribution, purchased nodes, prerequisites, eligible unit roles, effects and exact costs. Exclusive siblings and descendants SHALL remain visible with a readable permanent-lock explanation; the purchase action SHALL explicitly name the sibling branch it locks without requiring a second confirmation dialog. Unaffordable, prerequisite-locked, foreign-city, ready, paused, disconnected, combat and eliminated purchases SHALL be disabled with non-color-only reasons. Viewing research SHALL not pause or advance the shared match. The panel SHALL block underlying world input, fit supported window sizes using scrolling where needed, dismiss cleanly, refresh from acknowledged authoritative state and show no successful purchase effect before acceptance. Research Towers SHALL be identified by name and distinguishable two-level structures using the bundled palette; contextual output SHALL explain one/two points per full three-production cycle rather than per turn.

#### Scenario: First purchase without a research building
- **WHEN** a towerless eligible owner opens Research with three points and no selected plot
- **THEN** its affordable foundation can be purchased through actual controls and updates after acknowledgment
- **AND** the same tree is read-only when observing another city

#### Scenario: Permanent branch lock is visible
- **WHEN** an owner inspects Fire and then successfully purchases it
- **THEN** its purchase description identifies the Frost lock and Frost/descendants remain visible with the permanent reason afterward
- **AND** rejected or retried requests produce no duplicate success cue

### Requirement: Technology and status inspection
Unit inspection and army details SHALL describe actual acquired capabilities and current burn, poison and chill with authoritative strengths, poison stack count, remaining simulation duration and effective committed action timings where relevant. Visible living afflicted units SHALL have restrained distinguishable indicators using text/shape as well as color; indicators SHALL follow the shared presentation clock without owning damage scheduling or altering rig materials permanently. Poisoned queued units SHALL remain inspectable through army/enemy state without adding a world body. Death, effect expiry, city switches and fresh sessions SHALL clear or reconstruct indicator state; reconnect/history gaps SHALL reconstruct currently active effects without old sounds or application flashes. Existing Roman level markers, health bars, rigged actions and single-location movement SHALL remain readable. Chill descriptions SHALL promise slower future actions and SHALL NOT imply freeze, stun or interruption.

#### Scenario: Inspect poison and chilled timing
- **WHEN** a visible unit has two poison stacks and a committed chilled attack
- **THEN** its inspection shows the actual stacks and fixed action timing and its indicators agree with authoritative state
- **AND** pausing freezes remaining effect time and reconnect restores it without replaying applications
