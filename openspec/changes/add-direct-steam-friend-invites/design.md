# Design

## Context

See proposal.md for the observed invitation failure. `SteamPlatform.InviteFriends` currently calls `activateGameOverlayInviteDialog`; `SteamSessions.Invite` checks overlay availability and starts a deadline that any overlay activation satisfies. This proves only that the overlay opened, not that its lobby invitation picker appeared or an invitation was sent. The precise cause of the user's failure remains unknown.

`SteamSessions` owns lobby/session state, callback polling, accepted `join_requested` routing, and session cleanup. `GameApplication` owns modal input, focus, settings, join consent, and UI observation. `SteamOperationState` and `SteamLobbyRules` already have cheap policy coverage. The main steam-sessions spec explicitly requires an overlay interface, so this change replaces that requirement rather than adding a competing one.

## Goals / Non-Goals

**Goals:** Keep Steam operations on the application thread, bind selection to an authenticated account and current host generation, and exercise UI behavior without live Steam side effects.

**Non-Goals:** Overlay repair, rich-presence Join Game, lobby-code entry, public lobby discovery, new transport/dependencies, and production cold-launch qualification. No new fresh-join permission after the match starts; invitations remain useful for valid roster reconnects.

## Decisions

### Use native lobby invitations from an Odot picker

Extend the small `SteamPlatform` helper to enumerate immediate friends with `getFriendCount`/`getFriendByIndex`, resolve names and persona presence, revalidate friendship, and call `inviteUserToLobby(lobby, steamId)`. Verify exact methods, argument order, enum values, and return marshaling against the pinned GodotSteam release before implementing. Preserve AppID selection and native peer handling. Valve documents this direct send API independently of the overlay picker: https://partner.steamgames.com/doc/api/ISteamMatchmaking#InviteUserToLobby.

Prefer this over opening another overlay page because recipient selection then belongs to Odot. Prefer lobby invitations over `InviteUserToGame` because the existing lobby-join callback, validation, and consent flow already handle them.

### Keep UI and Steam state ownership separate

Introduce a small friends-dialog component under `src/Game`, composed by `GameApplication`, with scrollable rows, presence, Refresh, Close, per-row Invite, and result text. Use existing theme/modal/focus conventions and register it in modal checks, native exit handling, UI observation, and session teardown. Close incompatible open dialogs before opening it; incoming join consent closes the picker before displaying its own decision.

Represent each row by its Steam ID, never by display name or list index. Sort online friends first, then names with Steam ID as a stable tie-breaker. Include offline friends with their presence; do not claim they are running a compatible Odot build or require that presence to send. Refresh on opening and explicit Refresh. Use a short account-ID suffix to distinguish duplicate names; do not expose resume credentials. Keep an Invite action available after a completed send for an intentional retry.

`SteamSessions` provides the list and handles the send. Capture lobby ID and session generation when the picker opens. Recheck those, hosting state, `CanInvite`, Steam login, and current friendship at selection time. Disable the selected action during processing and use the native boolean result for sent/failure feedback. A successful send does not allocate a city or prove delivery. Clear picker state on Leave, session replacement, and exit; Steam login loss disables sends and shows feedback. Remove the obsolete overlay availability gate/deadline from this action.

### Use a narrow test seam with no live invitation side effects

Keep immutable friend data and a minimal friend-query/send boundary injectable for cheap tests and owned UI fixtures. Extract only meaningful selection/session policy into an engine-independent helper if needed; do not build a general Steam abstraction or fake transport. Fixture friends and send outcomes must remain confined to explicit verification launch paths with `ODOT_STEAM_DISABLED=1`, without permitting real SDK initialization, real lobby creation, or real invitations.

Cheap coverage should catch wrong recipient IDs for duplicate names, stale generations, lost friendship/login, rejected sends, and repeated activation. Extend the existing `launcher` UI slice for one small fixture dialog interaction, since cheap tests cannot establish modal input, keyboard focus, clipping, or rendered feedback. Budget approximately one additional owned launch or reuse its current launch, a few seconds of interaction, and maintenance of a handful of selectors; no headless battle or new option matrix. Record measured cost during implementation.

Real Steam acceptance stays separate: adapt the checked-in paired harness only enough to select a real friend through this picker and observe the normal join callback and authenticated native admission on two machines/accounts. A fixture pass cannot close this external gate. Do not automatically invite anyone during ordinary CI.

## Risks / Trade-offs

- [The overlay failure may coexist with a deeper Steam problem] → Verify direct send plus recipient acceptance and native admission separately; retain failures as evidence.
- [Steam accepts a request but the friend does not receive or accept it] → Report sent only as the native request result and observe admission separately.
- [Refresh changes row ordering or duplicate names confuse selection] → Bind callbacks to account IDs, preserve or safely reset focus on refresh, and disambiguate duplicate names.
- [Lifecycle changes leave stale UI callbacks] → Capture and revalidate lobby/generation and invalidate on cleanup, with cheap stale-action coverage.
- [Fixture behavior diverges from pinned native bindings] → Read the pinned API and run non-sending compatibility probes where possible; retain real two-account acceptance as unverified until observed.
- [A large friends list overflows the window] → Use a scroll container and verify representative content at existing supported UI sizes.

## Migration Plan

Replace the existing Invite friends action with the picker without changing lobby metadata, multiplayer protocol, AppID configuration, or stored credentials. Update README and verification notes to describe direct invitations and development warm-start requirements. Run required before/after implementation CI with locked tools and dependencies; reuse a baseline only when inputs match. Rollback restores the former picker action and matching spec requirement without a data migration.
