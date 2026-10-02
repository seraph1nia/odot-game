# Proposal

## Why

The match HUD repeats city identity, resource information and navigation across a tall bottom panel and world labels, obscuring the village and making building actions harder to scan. Simplify these surfaces while keeping cooperative inspection, economic decisions and session controls accessible.

## What Changes

- Remove decorative/semantic icons from owned non-game menus and all buttons, including embedded cost icons; retain Trio panel styling, text, focus and disabled states. Keep the explicitly requested world purchase marker.
- Move Return to menu from the HUD into session Settings behind a cancelable progress-loss confirmation.
- Replace the Reset view button with Space during eligible world interaction.
- Add left-click-and-drag world camera panning, preserving normal plot selection for clicks and preventing a completed drag from selecting or spending.
- Reduce the lower HUD to about 60% of its current height (300px to approximately 180px at supported reference sizes). Remove ODOT, P1/Your City headings, City/Army summaries, the verbose player roster bar, empty-plot instructions, land/resource summaries and routine Match started text.
- Preserve city switching in the current location as a very small `< [YOU] >` selector with left/right buttons and a concise identifier for other cities.
- Arrange construction in a three-column, three-row area under the current type/category selection; retain complete text costs and unavailable-action explanations.
- Replace the large phase block with a small vertical sequence highlighting the current stage, compact wave/turn counters, and side-by-side Ready/Pause buttons beneath it.
- Add a narrow top-right two-column resource table: Resource name / Amount, with Gold, Food, Wood, Stone, Metal and Cloth from the observed city's authoritative balances.
- Replace Locked land text with a gold/buy marker; remove board resource labels while retaining physical stockpiles. Replace the home's P1/You/heart/raw-health label with a percentage health bar.
- Replace overhead unit names/codes such as S L1 and BOSS L5 with a Roman level numeral above the left edge of each health bar.
- Make visible living friendly/enemy units and bosses clickable for a centre-right inspection popup showing the unit model, name, description, level, current/max health with a bar, and damage per attack. Outside clicks dismiss it; another unit click replaces its contents; camera dragging does not select units.
- Keep detailed upkeep, rewards and army inspection accessible outside the removed summary rows.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `themed-ui`: Text-only menus/buttons and resource table, with a world purchase-marker exception and compact responsive layout.
- `city-tabletop`: Reduced HUD, compact city navigation and phases, relocated resources, Space reset, cleaner board labels, percentage city health and click-to-inspect units.
- `game-feedback`: Roman unit level markers replacing overhead names/codes while preserving authoritative health-bar accuracy and lifecycle.
- `client-settings`: Session-only return action and cancelable confirmation with modal input/focus protection.
- `game-launcher`: Confirmed return through Settings while preserving existing solo, host and guest leave semantics.

## Impact

Presentation changes affect `src/Game/Tabletop.cs`, `TabletopCamera.cs`, `ProgressionPresentation.cs`, `ClientSettings.cs`, `GameApplication.cs`, `UiAssets.cs`, `UnitHealthBar.cs`, `UnitView.cs` and owned invitation/dialog surfaces. Extend existing `tools/DevRunner` UI observations and scenarios to reflect changed controls, resource placement, health, unit inspection and keyboard/mouse input. Gameplay rules, authority/network protocols, tool/dependency locks and asset provenance remain unchanged; no new resource types or downloaded assets are proposed.
