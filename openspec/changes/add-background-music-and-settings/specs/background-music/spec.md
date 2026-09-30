# Spec Delta

## Purpose

Provide continuous medieval background music for graphical clients, preserving the chosen track's authored loop and supporting offline desktop exports.

## ADDED Requirements

### Requirement: Authored music playback
Graphical clients SHALL play the supplied Echoes of Valhalla track non-positionally, starting with its intro once and then repeatedly playing its authored loop from approximately 35.123 to 140.488 seconds. Loop transitions SHALL avoid the track's later outro and any audible gap or click introduced by playback handling. Camera position and observed city SHALL NOT change music volume or restart playback.

#### Scenario: Reach the first loop boundary
- **WHEN** playback reaches the authored loop end for the first time
- **THEN** it continues from the authored loop start instead of playing the remaining outro or restarting the intro
- **AND** subsequent loop passes use the same boundaries without an audible playback gap or click

#### Scenario: Inspect another city
- **WHEN** a player switches the observed city
- **THEN** the existing music continues at its current playback position and volume

### Requirement: Graphical-client music lifetime
Music SHALL begin when the graphical client starts, including while connecting or waiting in the lobby. One music instance SHALL continue across gameplay, settings opening/closing, shared match pause/resume, outcomes, connection failure, disconnect, and reconnect. A reconnect SHALL NOT create a second player or restart the track. Music SHALL stop when that graphical client exits.

#### Scenario: Reconnect without duplicate playback
- **WHEN** a graphical client disconnects and successfully reconnects to its match
- **THEN** its existing music continues without restarting or playing a duplicate stream

#### Scenario: Open settings during a paused match
- **WHEN** a player opens and closes settings while the match is paused
- **THEN** music continues and the shared pause state remains unchanged

### Requirement: Offline source and exported music
The selected music source, import configuration, and recorded artist/source provenance SHALL be included in the repository so a fresh preparation and exported desktop client can play the track without the original Downloads folder or runtime downloads. A client export SHALL preserve the authored loop behavior.

#### Scenario: Run an exported client away from the source tree
- **WHEN** the exported desktop client runs without access to the project or original downloaded files
- **THEN** it plays the bundled music and repeats the authored loop

### Requirement: Audio-free server and headless operation
Dedicated servers and headless clients SHALL NOT instantiate music playback or load the music for runtime playback. They SHALL continue operating with stripped audio resources and without an audio device.

#### Scenario: Start headless multiplayer roles
- **WHEN** a dedicated server and automated headless clients start and run a match
- **THEN** their startup and gameplay succeed without music players or audio-device requirements
