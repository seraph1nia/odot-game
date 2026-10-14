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
remote actions. Hosted start and combat-speed selection belong to the original bound host;
dedicated start and speed selection remain available to authenticated connected
members. Solo uses its bound owner. [Gameplay controls](gameplay.md#preparation-and-battle-controls)
own the available rates and editing/settlement behavior. Roster capacity is four; city and
army land follow the [gameplay plot contract](gameplay.md#purchased-army-homes-and-town-hall-rotation).
No gameplay or balance rules are duplicated in presentation.

`Command.FromSnapshot` captures match/phase/stage and building-generation,
plot-count, home-count and selected hall-track or tower-level expectations for
both `Main` and ordinary campaign verification. Callers reserve the sequence first and
retain the resulting request for retry; they never refresh its quotes when
resending it. The raw wire constructor/defaults remain available for delivery
and deliberately stale/invalid request checks. Authority validation remains
mandatory; constructing a request does not authorize or mutate it.

After its shared owner/match/phase/payment/pause/Ready guards, `Match.Apply`
delegates city editing to the internal `SettlementCommands` module and advances
revision once for an accepted edit. That module owns quote/instance/capacity
checks and validated city mutations, not admission, retries, production or
battle progression. `ArmyConfiguration.Find` uses one living assigned-soldier
view scoped to field or exact Hall generation for homogeneous member capacity and stable assignment checks;
authority and previews share it, passing actual army-plot ownership and type so
arbitrary purchases retain fixed first-fit order and tower plots are excluded.
Combat remains the only unit health/identity store; [army-home restoration](gameplay.md#purchased-army-homes-and-town-hall-rotation)
owns the separation between accepted persistent edits and cleanup-time physical
claims.

Only authority admits physics delta through `AuthoritySession.Advance`; its
bounded wall-time driver owns deterministic steps, as described in
[battle-flow integration](battle-flow.md#behavior-and-integration-boundaries).
Local rendering and remote reliable channel-1 broadcasts consume complete
authoritative snapshots.
Reliable channel 0 carries admission, requests, acknowledgments and session end.
An acknowledgment is still delivered if a newer channel-1 snapshot has already
arrived; presentation retains the newer state. Admission retains handshake attempt
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


The current protocol, declared by
[`WireJson.ProtocolVersion`](../src/Game.Core/Diagnostics.cs), includes typed recruitment, hex movement/profile/action
state and bounded ordered combat-event history, plus each army plot's stable
identity, ownership, building generation/investment and projected occupation,
field/Town hall assignments, independent hall levels/quotes and completed-paid
recovery eligibility. City command IDs remain 0–8; army IDs are 9–14, displayed
as Army 1–6. It also carries the [Worldtree production technologies](worldtree.md#bounded-node-design)
and their optional catalog benefits, and the distinct rapid-arrow/bombard tower
profiles, synchronized combat speed and the authoritative next-wave manifest.
Integrated protocol **15** rejects older peers before sending a snapshot,
including both incompatible predecessor protocol14 schemas. Steam lobby matching
uses the same version boundary. Combat rules7 and battle-flow version2 are also
validated explicitly; matching a protocol integer alone cannot validate actors,
profiles, geometry or the full authoritative configuration fingerprint.

`MatchSnapshot.CurrentBattleFlowVersion` in [the snapshot schema](../src/Game.Core/World.cs)
distinguishes numbered preparation semantics from legacy four-check snapshots.
[The payload codec](../src/Game/SnapshotPayload.cs) rejects a missing or mismatched
version, Building turns other than 1/2, Preparation turns other than 3, and invalid
combat-speed values. Historical snapshots are not silently migrated or treated
as game saves. `SessionFile`'s credential/sequence schema is unchanged; reconnect
to the same matching running authority retains the selected pace and forecast.
`Command.UnitId` targets retire/store/send and `regroup-member`.
`SquadId` selects reinforcement/regrouping/return: zero uses first fit, -1 requests
a new squad at a free home, and a positive ID selects an existing compatible
squad. Selectors below -1 reject before placement lookup or mutation.
`deploy-squad` uses `SquadId` and the physical arena `Cell` to move the complete
home claim. `ExpectedHomeCount` binds the army purchase-count quote,
`ExpectedTrackLevel` binds the selected hall track or base tower level, and
`ExpectedGeneration` binds the building instance.
`buy-plot` targets a chosen locked army ID as well as city land; legacy
`buy-home` remains an alias for actual army land acquisition (the first locked
plot when no ID is supplied), not a separate capacity purchase.
`upgrade-tower` selects a final rapid-arrow or bombard identity on a base Arrow
tower; the legacy `upgrade` action retains the ordinary Arrow/Catapult L2 paths.
The existing authenticated owner, match/phase/stage, ready/pause and sequence
ledger checks still apply. Full snapshots detach plot states, assignments,
all-owned food forecast/funded/field-participant receipts and current wounds;
reconnect never
replays production, food payment or healing.

Full compatibility identity includes all six resolved tower profiles, the current
combat rules declared in [`CombatSettings`](../src/Game.Core/Combat/CombatConfiguration.cs)
and the actor decision fingerprint. Both combat fingerprints contain actual
rules-v7 inputs; retired size, victim-cap and radius authoring fields are refused,
not retained as hidden decision inputs. The base-actor namespace excludes the
optional Rapid Arrow/Bombard profiles, so changing only their timing changes full
compatibility identity without rerolling unchanged squads, defenders or base
weapons. `CombatConfigurationTests` covers complete identity round-trips and
observation-invariant reconstructed squad decisions;
`ArmyPlotTowerTests.BaseTowerSelectionIgnoresOptionalUpgradeTimingWithOneSaltCounterfactual`
checks that isolation against a full-identity counterfactual. Snapshot codec/load
and credential resume against the same live authority remain the persistence
boundary, not an authority disk-save or restart-recovery API.

`recruit <slot>` still means Swordsman; automation also accepts
`recruit <slot> crossbowman`. Admission refuses earlier protocol versions; there
is no mixed-version play. Stable unit IDs and event sequences are match-scoped,
never Arch entity handles. Guest revision/match guards apply to all complete
states. [Shared playback](gameplay.md#deterministic-hex-combat) owns admission/resume
and history-gap baselines for current living poses and unexpired deaths.
