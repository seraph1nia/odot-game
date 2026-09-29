# Spec Delta

## Purpose

Provide a small desktop multiplayer game that demonstrates C# gameplay and a real headless server through synchronized movement, coin collection, and scores.

## ADDED Requirements

### Requirement: Graphical clients and dedicated server
The game SHALL support graphical desktop clients and a separately launched headless Godot server. Both roles SHALL use the same gameplay rules. Server/client role selection SHALL be explicit, allowing automated clients to run headlessly without becoming servers. The initial game SHALL support at least two simultaneous clients.

#### Scenario: Two desktop players connect
- **WHEN** two clients connect to a running server
- **THEN** each receives a distinct player identity and sees both players, the room, the current coin, and the shared scoreboard
- **AND** the server runs without a display or audio device

#### Scenario: Automated headless client
- **WHEN** the game is launched headlessly in client mode
- **THEN** it connects and receives game state as a client without opening a listening server

### Requirement: Server-owned movement
Clients SHALL submit movement intent for their own player. The server SHALL validate input, apply a bounded movement speed, constrain players to the room, and distribute authoritative positions. A client SHALL NOT directly set authoritative position or submit movement on behalf of another player.

#### Scenario: Visible synchronized movement
- **WHEN** a connected player presses the documented movement controls
- **THEN** that player's server-calculated position changes within the room
- **AND** both clients observe the updated position

#### Scenario: Invalid movement input
- **WHEN** a client submits non-finite, oversized, or invalid movement input
- **THEN** the server rejects or bounds it without permitting movement beyond the defined speed or room boundaries
- **AND** other players remain responsive

### Requirement: Coin collection and scores
The game SHALL display one collectible coin. Only the server SHALL determine collection from authoritative player positions, award one point to exactly one player per coin generation, and spawn the next coin within the room. Every client SHALL receive the resulting score and coin state.

#### Scenario: Collect a coin
- **WHEN** a player's authoritative position reaches the coin's collection area
- **THEN** that player receives one point
- **AND** both clients display the updated scores and replacement coin

#### Scenario: Competing pickups
- **WHEN** two players reach the same coin during one simulation step
- **THEN** a stable server-side tie-break selects one winner and awards exactly one point in total for that coin

### Requirement: Session lifecycle and feedback
Clients SHALL display connecting, connected, connection-failed, and server-disconnected states as applicable. Joining clients SHALL receive the current world and scores. The server SHALL remove a disconnected player's avatar and session score and notify remaining clients. Reconnecting SHALL create a new player session.

#### Scenario: Join an existing session
- **WHEN** a second client joins after the first has moved and scored
- **THEN** it receives the current players, positions, scores, and coin rather than a new independent world

#### Scenario: Player disconnects
- **WHEN** a client disconnects
- **THEN** the server removes its player and score and the remaining client observes that removal

#### Scenario: Server is unavailable or stops
- **WHEN** a client cannot connect within the documented timeout or loses its server connection
- **THEN** it shows connection-failed or server-disconnected feedback and stops submitting gameplay input to that session
