# Session ownership

`GameApplication` owns screens, preferences and one music player. It survives
session transitions. `Main` owns the active session, its generation, transport
and Godot RPC adapters. `Tabletop` consumes the narrow `IGameSession` interface;
it reads snapshots/status and submits ordinary actions.

| Mode | Authority | Local player | Delivery |
| --- | --- | --- | --- |
| Single player | Same process | One bound city | Direct shared validation; no socket |
| Playing host | Original host process | Host's bound city | Direct local validation plus ENet/native Steam guests |
| Guest | Original host elsewhere | Retained admitted city | Godot reliable RPCs and full snapshots |
| Dedicated | Explicit headless server | None | ENet RPCs; connected members may start |

The engine-independent `AuthoritySession` owns `Match`, authenticated bindings,
private resume credentials, per-player request ledgers and remote rate limits.
Local actions use the same ownership, costs, phase/turn and retry validation as
remote actions. Hosted start belongs to the original bound host; dedicated
start remains available to connected members. Capacity is four, with nine plots
per city. No gameplay or balance rules are duplicated in presentation.

Only authority calls `Step()` on the fixed physics tick. Local rendering and
remote reliable channel-1 broadcasts consume complete authoritative snapshots.
Reliable channel 0 carries admission, requests, acknowledgments and session end.
An acknowledgment is still delivered if a newer channel-1 snapshot has already
arrived; presentation retains the newer state. Protocol v4 retains v3 handshake attempt
and expected-match isolation. Exported reconnect retains the required Godot
RPC-node reset while the persistent application stays alive.

Guest files contain transport, application, endpoint, immutable original host,
match, secret credential and next command sequence. Legacy ENet files still load
for their original endpoint. Steam storage also separates AppID/host/lobby;
credentials require the same SDK-authenticated Steam identity. Lobby metadata
contains only game/protocol/application/original-host/match/availability.
The native peer's identity mapping supplies admission identity, never a client
claim. Files and logs must not expose private credentials.

Leaving increments the session generation and immediately removes logical
state, bindings and callbacks. A playing host keeps its old transport alive
until guests receive the reliable session-end message and disconnect, bounded
by the existing connection deadline. Fresh sessions wait for that transport
to close; a later menu/exit action cancels a queued start. Exit also waits for
this drain before disposing Steam and audio. Queued old gameplay inputs and old-match packets cannot affect a
fresh match or consume its new command ledger. Read-only UI observations inspect
the persistent application after rendering, including across a transition.
Guest credentials persist for the same running host; gameplay is unsaved.

Steam's callback owner uses a small operation-state helper because create results
have no request identifier and join results identify only the lobby. Abandoned
operations remain reserved until their late callback drains; retries cannot adopt
an old result. Invitations during a session ask before leaving. The original
host remains immutable despite Steam lobby-owner reassignment. Orderly host leave
ends gameplay; loss disables guest actions and exposes reconnect/return feedback.
There is no authority migration or server-restart recovery.

Pending invitation consent has its own session generation, separate from a
create/join request. An epoch change expires the consent and dismisses the native
dialog; old accept/decline delegates cannot act on a replacement invitation for
the same lobby. Consent in a still-current solo or hosted session stays open.

Steam login and persona status come directly from the extension's `isSteamRunning`,
`loggedOn` and `getPersonaName` APIs. Menu status refreshes once a second after SDK
initialization. A normal launch detects Steam starting later and retries initialization;
an SDK/app-access initialization failure can be retried through Host game. Explicit
local roles and offline verification do not make automatic initialization attempts.
Host/join attempts recheck login even when initialization already
succeeded, and host creation checks again before attaching gameplay. Offline/local
launches keep Steam disabled. A temporary online-status loss updates the menu
status without forcibly destroying an existing match; Valve documents the live
connection semantics of [BLoggedOn](https://partner.steamgames.com/doc/api/ISteamUser#BLoggedOn).


Protocol v4 adds typed recruitment, scalar forward/lateral bodies, movement,
profile and target/action timing, and bounded ordered combat-event history.
`recruit <slot>` still means Swordsman; automation also accepts
`recruit <slot> crossbowman`. Admission refuses earlier protocol versions; there
is no mixed-version play. Stable unit IDs and event sequences are match-scoped,
never Arch entity handles. Guest revision/match guards apply to all complete
states. Presentation baselines history on admission/resume, seeks pending actions
at the current tick, and resets to living state on a detectable history gap.
