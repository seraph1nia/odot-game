# Nine Tiles POC

One to four players share a match, each owning nine indexed building slots displayed as three staggered rows of hex plots and a separate grassy battle approach. Single player starts one city immediately; hosted multiplayer waits for the original host to click Start. Join the roster before start. The roster is fixed after start; returning players can resume, fresh players cannot join. Return to menu and start again for a fresh local/hosted match; restart a dedicated server for another match.

Each city starts with 100 health, 60 gold and no food. A production turn gives every living city 10 gold, including disconnected cities. Mine, farm and barracks each cost 20 gold. Mines produce 3 gold per level per turn; farms produce 5 food per level. Spend 20 gold to upgrade a building once, to level two. Select a barracks and click **Swordsman** or **Crossbowman** to buy exactly one soldier for 5 food (4 at level two). Barracks never recruit automatically. Swordsmen have 10 health and deal 4 damage; Crossbowmen have 8 health and deal 3 damage. Both attack once per second.

Click an outlined grass plot or the visible body/roof of a building, then choose a construction or upgrade action in the bottom panel. Clicking only selects; it never spends. With no selection, the panel asks you to select in the world. Select a barracks, then choose the soldier type to recruit. Changing city tabs clears selection. The surrounding grass, river, wooded hills, rocks and village props are decorative: they use no building slots and add no adjacency or movement rules. The roster tabs inspect other cities; their buildings cannot be edited. Ready prevents further editing; Unready restores it until the turn resolves. All connected living players must be ready. Production happens once, clears readiness, and after the third turn immediately starts combat. There is no opportunity to spend the third turn's food before that battle. There are three turns before each of three waves, nine production turns total.

Enemies allocate 4, 6 and 8 per original player over the three waves. Each has 10 health and deals 3 damage each second. The city's built-in tower deals 1 damage each second even without soldiers, outside the nine slots. Units move and fight automatically within each city’s numerical corridor; no orders are required. The authority accumulates damage before applying it, so simultaneous attacks count. Surviving soldiers keep their identity and remaining health; cities keep damage too. No healing occurs between waves.

A city falls at zero health. Its remaining attackers immediately move to the entrances of surviving cities' lanes, retaining identity, health and attack cooldown. They are split evenly, with remainders assigned in player-ID order; a previously cleared city can receive them. If two cities fall in the same step, neither receives transfers. Disconnected living cities still count as survivors. Future waves include one baseline allocation per original player, with fallen players' allocations divided among survivors. For example, after one of three cities falls, wave two gives each survivor 9 enemies (18 total). Inheriting enemies does not multiply future allocations. Eliminated players can observe, pause and resume. All players share victory after wave three if a city survives; all cities falling ends the match in defeat, including simultaneous final enemy deaths.

A verified starting strategy: build a farm in slot 1, upgrade it, and build a barracks in slot 2 using the starting 60 gold. Recruit all affordable soldiers before clicking Ready on later turns. Repeat for all three waves. Production feeds later recruitment; food from the third production waits until the next building phase. This strategy wins with the standard rules, without privileged test commands or extra grants. For a losing strategy, ready without building or recruiting.

Any connected roster member can pause or resume the whole match. Pause freezes movement, attack cooldowns, health, production and turn progression. It also rejects economy/readiness commands. Networking stays active, and absent players can reconnect. Disconnecting itself never pauses or deletes a city: production/combat continue, readiness clears, and disconnected players do not block the ready check. With no connected living players, building waits; combat keeps running unless paused.

Gold and food are the only resources. Walls, selling, healing, trades, reinforcements, save recovery, host migration, server restart recovery and matchmaking are outside this POC. Rules are centralized in `src/Game.Core/World.cs`; the server sends costs/output values in snapshots so the UI uses the actual rules.

The bottom panel keeps city resources/health/army and inspection tabs on the left, selected building details/costs in the middle, and match controls on the right. Start is available to the original playing host in a hosted lobby (or connected players on a dedicated server); Ready/Unready and Pause/Resume follow the authoritative phase. Connection and rejection messages appear along the bottom. A disconnected window offers Reconnect to my city; a synchronized resume retains the city and starts with no selected plot. Expired credentials expose the fresh-session lobby action. Fallen players can inspect all cities and pause/resume; victory/defeat remains visible to the entire roster.

![The nine hex plots and bottom controls](images/source-building.png)

![Paused combat after reconnecting in the exported client](images/export-paused-resume.png)

## Music and local settings

Press **Esc** or click **Settings** in the top-left to open Graphics and Audio.
Press Esc again or **Close** to return; Esc dismisses an open dropdown first.
Opening settings blocks gameplay input in your window while the shared match and
network updates continue. Use **Pause whole match** separately to pause everyone.
One application music player continues across menu, solo/hosted sessions, connecting, pauses/outcomes and reconnects.

Graphics offers **Windowed** and **Fullscreen**. Windowed resolution changes the
window's size, with 1100×820, 1280×720, 1600×900, and 1920×1080 offered when they
fit your monitor's usable area. Fullscreen uses the current monitor's native size;
the windowed selector is disabled until you return to Windowed, restoring your
previous size. Manual resizing is shown as a custom size. Saved sizes that no
longer fit fall back to a usable size. Explicit engine launch arguments such as
`--resolution`, `--fullscreen`, and `--windowed` take precedence for that launch;
those overrides do not become saved preferences until you edit display settings.

Audio provides a live **Master volume** slider from **0–100**. Zero mutes all
audio; raising it restores music at its ongoing position. First-launch defaults
are Windowed 1100×820 and Master 50, with the music itself mixed at -12 dB.
The bundled Echoes of Valhalla track plays its intro once and repeats its authored
loop through Godot's WAV playback.

Preferences are stored separately from multiplayer credentials in
`user://settings.cfg` (on a standard Linux desktop,
`~/.local/share/godot/app_userdata/Odot - Nine Tiles/settings.cfg`). Display
selections, completed slider edits, menu closing, and pending changes on orderly
exit save them. Restart restores valid values; reconnect retains the current
ones. Invalid/missing fields use defaults. A failed save displays a message in
the menu while your current settings keep working.

Two running local clients keep independent live preferences. The last successful
save becomes the shared OS user's defaults for later launches; it never updates
the other running window. Servers and headless clients ignore graphical settings
and do not create music players. Music provenance and import details are recorded
in [the asset README](../src/Game/Assets/Music/README.md).


## Start screen and session lifetime

A normal graphical launch opens **Single player**, **Multiplayer**, **Settings**
and **Exit Game**, over static bundled medieval scenery. Keyboard arrows/Tab and
Enter work alongside pointer input. The menu creates no match or gameplay socket.
The bottom of the start screen and Multiplayer view shows your Steam display
name, or **Open Steam to log in** when Steam is unavailable or offline. Host game
checks the current login each time and warns you to open Steam and log in before
creating a lobby.

Single player binds one local city and uses the same validated actions and rules
as multiplayer. Multiplayer offers **Host game** and **Back**. With Steam running
and signed in, Host game creates a private invitation-only lobby; use **Invite
friends** in the bottom panel. Steam’s overlay must be enabled; Shift+Tab
checks whether it can open. If it is unavailable or an invitation dialog does
not activate, the bottom panel explains the problem. Only the original host
starts the match. Missing
Steam/login/access reports recoverable feedback; solo, settings, Back and Exit
remain available. The local `mise run dev` host/guest flow uses ENet independently.

**Return to menu** ends unsaved solo/host gameplay and clears selection and
pending actions; preferences and music persist. Guests keep private credentials
for the same running host. An invitation during a match asks before leaving;
declining preserves it. Returning players can resume after the roster locks,
including pause or elimination, using their retained credential and Steam account.
Fresh players cannot join after start. A new host session has a new match and
refuses old credentials.

The original host owns the whole hosted session. Its orderly leave ends the
session for guests; unexpected loss disables guest actions and offers bounded
reconnect/return feedback. A Steam lobby owner change never migrates gameplay.
**Exit Game** and the window's native close control stop the session, music,
network/Steam resources and the game process. Closing settings restores valid
focus and changes neither readiness nor shared pause.

## Arch combat and animated soldiers

Each authoritative `Match` owns one private Arch 2.1.0 world. Identity, health,
body position/movement, target, attack timing and weapon profile are components;
city soldier/enemy collections are read-only DTO projections, not writable unit
stores. Public IDs increase within the match and never contain Arch handles.
`AuthoritySession.End` caches its final immutable snapshot and disposes the world,
including its Arch registry entry, on orderly teardown. Local, playing-host and
dedicated authorities execute the same systems synchronously on the fixed 60 Hz
Godot physics thread. Guests only render 20 Hz complete snapshots. There are no
worker-thread rule updates, Godot physics bodies or animation damage callbacks.
Core tests use an internal fixture builder to seed components, never a public
cheat command or secondary production store, and dispose their owned matches.

The battle approach is 12 units long and 3.4 wide. Circular bodies have radius
0.20; active centers remain between 0.20–11.80 forward and ±1.50 laterally.
Seven columns spaced 0.48 apart fill up to six entry rows per side. Swordsmen
enter ahead of the rear Crossbowmen; enemies enter at the far end. Recruitment,
new waves and transfers account for every arrival, including units waiting in
an explicit entry queue when all separated entry locations are occupied. Queued
units cannot target, attack or act as contact bodies. Surviving soldiers retain
stable identity, type and health between waves; each wave reforms the approach.
Terrain remains decorative and the nine city slots are unchanged.

Targeting reads one shared start-of-step buffer. Stable IDs break distance ties;
valid targets remain engaged unless a closer melee opponent intercepts the path.
Motion stops at the weapon range, uses swept circular contact against the shared
buffer and approved moves, and permits bounded local tangents around friendly
blockers. A blocked stationary unit has zero locomotion velocity. No opposing
bodies cross or overlap; this is a deterministic local contact solver, not a
navigation mesh or general physics simulation.

| Profile | Health | Damage | Food | Range | Speed | Windup | Start cadence |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Swordsman | 10 | 4 | 5 / 4 upgraded | 0.55 | 1.0 | 12 ticks | 60 ticks |
| Crossbowman | 8 | 3 | 5 / 4 upgraded | 3.0 | 1.0 | 18 ticks | 60 ticks |
| Enemy | 10 | 3 | — | 0.55 | 0.8 | 12 ticks | 60 ticks |

An attack starting at tick T impacts at T + windup, and can start again at
T + 60. The attacker holds its position through windup. Impact revalidates the
locked target’s identity, destination and range. Death or transfer of that target
produces a miss, not a hit on another unit. Crossbow impacts are single-target
numerical damage, also usable at close range. All same-tick damage is accumulated
before applying casualties, preserving mutual kills and defeat precedence. The
city defender remains an immediate weak city-wide 1-damage shot every second.
Transfer cancels an old windup while retaining the remaining recovery deadline.
The original winning economy and an alternating mixed-army economy both win all
three waves; no starting profile adjustment was needed.

Knight and Rogue use imported skeletons with sword/crossbow hand attachments and
programmatic AnimationTrees with a synchronized idle/walk/run blend space, timed attack one-shots, a filtered upper-body hit layer and terminal death override. Idle, actual-motion walk/run, melee, shooting, hit
and death sample the imported clips. Horizontal root movement is suppressed.
The melee strike marker at 0.40 clip seconds and crossbow release at 0.43 are
mapped to the authoritative impact tick, then recovery samples the rest of each
1.0667-second clip until the ready tick. Cosmetic arrows last 0.12 presentation
seconds and never apply damage. Death overrides hit; a short hit pose can overlay
an attack without altering its timing. Corpses leave combat immediately and their
non-interactive visuals are freed after 1.4 unpaused presentation seconds, even
when the last casualty ends the wave. Pause and transport loss freeze this clock;
resume does not catch up elapsed wall time.

A shared, bounded snapshot clock interpolates all bodies with the same fraction,
without extrapolating beyond authority. A 120-tick, at-most-4096-record event
history carries final casualty states. Initial admission, reconnect and detected
history gaps baseline the event cursor at the current high-water mark: only living
current poses are reconstructed. Overlapping snapshots deduplicate effects;
revision and match guards reject stale state. A fresh session clears all live/dead
views, pose buffers and cursors.

New entry visuals wait for their deployment snapshot’s common clock before appearing, preserving separation from interpolated neighbors. Surviving units return to idle after combat while death visuals finish independently.

The 32-vs-32 fixtures cleared wave one at 1220 ticks (20.33 simulated seconds)
for all Swordsmen and 869 ticks (14.48 seconds) for a mixed army, with 7 and 19
survivors respectively. These fixtures disable the defender to isolate units;
they assert separation every tick and reproduce under reversed storage. The
ordinary three-wave economy tests retain the real defender and starting rules.
