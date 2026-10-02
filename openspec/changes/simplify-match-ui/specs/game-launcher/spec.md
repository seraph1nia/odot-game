# Spec Delta

## MODIFIED Requirements

### Requirement: Return to menu and fresh session isolation
Graphical lobby, match, outcome, and disconnected views SHALL provide Return to menu inside Settings, behind the session-aware cancelable confirmation, rather than a bottom-HUD button. Opening or canceling the confirmation SHALL NOT leave the session; explicit confirmation SHALL perform the return exactly once. Returning SHALL stop local gameplay presentation, release the current session's connections and lobby membership, and clear transient input, selection, and pending requests while preserving local preferences and valid private guest resume credentials. A host leaving SHALL end its hosted session; a guest leaving SHALL disconnect without deleting its retained city from a still-running host. Repeated leave operations and late callbacks SHALL NOT create duplicate sessions or apply state to a subsequent session. Leaving solo play SHALL discard the unsaved local match.

#### Scenario: Leave and host again
- **WHEN** the host returns to the start screen and hosts a new game in the same process
- **THEN** exactly one new authority and lobby are active with a new match identity and fresh roster
- **AND** old callbacks, snapshots, or commands cannot change the new session

#### Scenario: Guest returns to the menu
- **WHEN** a guest confirms Return to menu from Settings while its host remains running
- **THEN** the guest sees the start screen and the host retains the disconnected guest's city
- **AND** valid reconnect information remains private and available for that running match

#### Scenario: Confirm leaving solo play
- **WHEN** a player confirms the unsaved-progress warning and then starts solo play again
- **THEN** the prior local match is discarded and the new match has fresh identity, starting balances and empty selection
- **AND** local preferences and the application music instance survive the transition

