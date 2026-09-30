# Proposal

## Why

During a real co-op attempt with both players already running Odot, the host saw only Watch and Remote Play options in Steam. Odot should let the host select a Steam friend and send a multiplayer lobby invitation directly, without depending on the overlay invitation picker.

## What Changes

- Make Invite friends open an Odot dialog with Steam friends, presence, refresh, and a per-friend Invite action.
- Send invitations through GodotSteam's native `inviteUserToLobby` call for the current hosted lobby; show whether Steam accepted or rejected the send request.
- Preserve accepted-invitation routing, authenticated admission, four-player capacity, and roster reconnect behavior.
- Close and invalidate the picker when its host session ends or changes; report Steam loss and empty friends lists clearly.
- Verify selection and lifecycle behavior locally without sending real invitations; retain a separate two-account development acceptance check.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `steam-sessions`: Replace the required overlay invitation interface with an in-game Steam friends picker and direct lobby invitation semantics.

## Impact

Changes center on `src/Game/SteamPlatform.cs`, `SteamSessions.cs`, and `GameApplication.cs`, plus the existing launcher UI checks and Steam paired verification documentation/harness. Any extracted invitation policy belongs in `src/Game.Core` and its cheap tests. Use the pinned GodotSteam extension and existing development AppID 480; no dependency update or production AppID is required. This proposal does not establish the cause of the overlay failure or claim successful real Steam invitations.
