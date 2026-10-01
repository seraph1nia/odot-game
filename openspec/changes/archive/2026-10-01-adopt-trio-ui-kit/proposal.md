# Proposal

## Why

Odot's menus and match controls currently use plain Godot controls with a small custom theme, and combat units display health as text above their heads. Using the requested free Trio UI kit throughout the interface will give menus a consistent visual identity and make actions and unit health easier to read.

## What Changes

- Bundle the selected free Trio UI kit assets with license evidence, source/version information and hashes; require no paid edition or runtime downloads.
- Apply one shared kit-based style to every application-owned menu and overlay: start screen, multiplayer entry, hosted lobby, match HUD and contextual actions, pause/outcome/disconnection controls, Settings including About/update controls, friend invitations and join confirmation.
- Add kit icons to actions and information where their meaning matches, keeping readable labels, exact values, costs, disabled states and keyboard focus. Use text for concepts absent from the free sample.
- Show kit-styled health bars above every visible living friendly and enemy unit, driven by authoritative current and maximum health and removed on death.
- Preserve session behavior, modal input protection, responsive world/HUD layout, existing cooperative assertions and headless/export compatibility.

## Capabilities

### New Capabilities

- `themed-ui`: A consistent bundled Trio presentation across all application-owned menus, semantic icons and usable control states/layouts.

### Modified Capabilities

- `game-feedback`: Add authoritative overhead unit health bars with correct visibility and lifecycle behavior.

## Impact

Presentation changes center on `src/Game/ApplicationTheme.cs`, `GameApplication.cs`, `ClientSettings.cs`, `SteamFriendsDialog.cs`, `Tabletop.cs` and `UnitView.cs`, plus a new asset directory under `src/Game/Assets`. Existing unit snapshots already carry `Health` and `Profile.Health`; gameplay rules and wire schemas need no change. Extend relevant DevRunner UI observations and existing launcher/settings/economy/reconnect/combat scenarios, and confirm client exports bundle the assets while stripped server exports remain independent of them. No tool or dependency upgrade is planned.
