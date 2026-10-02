# Design

## Context

See proposal.md for motivation and the five capability deltas for behavior. `Tabletop` currently builds a minimum-300px bottom panel with city/resources, contextual actions, phase controls and a footer. Its construction grid has four columns. Resource totals use two horizontal rows; verbose city tabs select the observed player. `ProgressionPresentation` combines large phase/wave/turn text with land, upkeep, rewards and army details. `TabletopCamera` owns fitted camera state, wheel zoom, bounded keyboard panning and reset; `Tabletop` routes world selection and input.

`ClientSettings` is application-owned, reused outside and inside sessions. `GameApplication.ReturnToMenu` currently refuses calls while any modal is open. `UiAssets.Decorate` inserts icons, `UiAssets.Cost` embeds gold/food images, and Settings tabs have explicit icons. World stockpiles and their tests are separate from resource labels. Existing runner checks depend on ResetView, CityN and ReturnToMenu targets and icon-bearing controls, so observation and driver changes belong in this feature. `UnitHealthBar` currently renders role/level codes below the bar using `ProgressionPresentation.UnitLabel`; runner and presentation tests assert those codes. `UnitView.State` already contains identity, faction, level, current health and a resolved profile with maximum health and per-attack damage. Unit selection and descriptions are new presentation behavior; existing picking only targets plots/buildings.

The working tree already contains unrelated changes to CI, exports, README and verification documentation. Preserve them. Planning edits are confined to this change directory; full game CI belongs to implementation, not proposal authoring.

## Goals / Non-Goals

**Goals:** Separate current-state presentation from layout, reduce HUD reservation, preserve authoritative action eligibility and keep ordinary selection distinct from camera gestures. Reuse the existing application/session lifecycle and bundled assets.

**Non-Goals:** New economy resources, gameplay cadence changes, new networking commands, persistent camera preferences, dependency updates, new bitmap assets or platform-owned Steam UI changes.

## Decisions

### 1. Three compact presentation areas

Use Godot containers for a roughly 180px bottom panel, narrow top-right resource table and existing top-left Settings entry. Keep small city navigation at its current lower-HUD position. Reserve the center for category selection and a three-column grid with three row positions; categories contain fewer than nine choices today, so empty space is non-actionable. Retain Production/Army/Defense/Trade, which are construction categories, while removing the separate City/Army summary row.

Aim for an approximately 220px-wide resource panel whose header and six rows make it taller than wide. Resource names and right-aligned exact amounts use plain text in Gold/Food/Wood/Stone/Metal/Cloth order. Update from the observed city, not always the local player. Its occupied rectangle blocks world selection, wheel zoom and drag initiation. Camera fitting and supported-size checks must account for this obstruction as well as bottom HUD height.

Keep complete prices/refunds on compact text controls. Move long missing-resource/producer reasons, upkeep forecast, actual wave reward and army details into an accessible text-only Details inspection surface in the city section; contextual building quotes remain next to their actions. Detailed inspection is opened explicitly rather than growing the bottom panel. This preserves economy and reserve contracts without restoring removed summary rows. Display only actionable status/rejection/outcome feedback; do not blanket-hide errors along with routine Match started text.

Alternative: shrink the existing three dense columns uniformly. Rejected because it leaves duplicated labels and can clip exact costs while freeing little world space.

### 2. City selector and phase projection

Replace per-player buttons with text `<`, a concise `[YOU]` or `[P2]` indicator, and `>`, cycling the snapshot roster in stable player order with wraparound. Disable arrows in solo play. Keep existing city-switch behavior: clear selected/hovered slot, reset overview and reconstruct the observed city. Lobby roster/host/readiness remains lobby-specific; detailed current roster state belongs in inspection during a match.

Project the sequence Building 1, Building 2, Building 3, Preparation, Combat from snapshot Phase/Turn. Highlight the active row using both emphasis and a textual cue; keep small Wave n/total and Turn n/3 and boss status. Production occurs when a building stage resolves and is not a new snapshot phase. Place Ready/Unready and Pause/Resume side by side below this sequence, preserving command meanings and disabled rules. Lobby and outcomes use their own compact state instead of an incorrect active-stage highlight.

Alternative: list only enum phases. Rejected because one Building row does not explain the three-turn cadence the user wants to see.

### 3. Text-only UI with an explicit world-marker exception

Change button decoration and cost rendering to plain labels; remove explicit Settings tab and owned-menu content icons, including friend/invitation surfaces. Retain kit panel/control textures, focus borders and native widget affordances. Reuse the bundled gold-pile SVG for a small input-transparent marker anchored over locked plots, with exact costs in contextual Buy plot controls. Remove stockpile resource text labels only; keep physical stockpiles, their bounded authoritative tiers and asset provenance intact.

Alternative: remove icon assets from exports. Rejected because the requested plot marker still uses one and unrelated asset deletion creates provenance/export risk.

### 4. Home health as a projected percentage bar

Use a dedicated home-health presentation in the existing screen-space health layer, sharing styling/projection conventions with unit bars without pretending the home is a unit. Derive fill from city health and maximum city health in the same numeric scale, clamp to [0,1], and round the percentage to the nearest whole number. Show 100% at full health, 0% and a fallen cue at elimination. Remove the old Pn/You/heart/raw-health label, including redundant Home/Defender caption. Bars track the world anchor through pan/zoom/resizing and remain input-transparent; cleanup follows tabletop lifecycle. Freeze health on lost transport and use fresh snapshot health on recovery.

Alternative: reuse the old Label3D with a percentage string. Rejected because the user explicitly asked for a health bar, which must remain readable at camera limits.

### 5. Camera shortcuts and mouse gestures

Route a non-echo Space press through normal unhandled-world input to existing reset behavior; consuming focused controls and modals take priority. Add a small shortcut tooltip/help entry rather than another reset button.

Assume left mouse button for click-and-drag. Track a pending gesture on an eligible world press, delay normal selection until release, and classify as a drag only after approximately six screen pixels of movement. Capture the ground point beneath the pointer and translate the camera to keep that point beneath subsequent cursor positions, clamped by existing travel bounds. Crossing the threshold suppresses both initial and release selection; a small click resolves selection once. Handle release even after the pointer leaves the world rectangle, and cancel pending/active gestures on modal/focus loss, city/session replacement or reset. Preserve zoom and orientation. Existing keyboard pan remains available; reset/interrupt clears held state. All gestures remain cosmetic and submit no gameplay commands.

Alternative: use middle/right mouse drag. Left drag matches the request using an ordinary mouse without a new binding; threshold classification preserves plot clicks. Input routing changes require actual-input regression coverage because cheap state projections cannot establish Godot click/drag priority.

### 6. Roman unit levels and centre-right inspection

Replace the overhead role/name label with a Roman level numeral positioned immediately above the health bar's left edge. Use conventional subtractive notation (I, IV, IX, etc.) for the authoritative unit level, not research rank or building level. Size and visibility bounds include the numeral, including longer values in the supported level range. Keep the existing authoritative fill, effective maximum, faction-colored ground markers and bar projection/lifetime rules. Full unit name, role, faction and boss status move into inspection and existing army details; do not add overhead replacement abbreviations.

Add a tabletop-owned nonmodal unit inspector anchored at centre-right below the top-right resource table and above the bottom HUD at both supported sizes. It shows a static preview of the selected unit's existing bundled character and weapon, a readable archetype/faction-specific name, short role description, level, current/max health text and health bar, damage per attack and boss status when applicable. A bounded dedicated preview viewport reuses asset loading without a second gameplay UnitView, combat effects, sounds or authoritative actor. Descriptions live in a small presentation mapping for the current archetypes/factions; numbers come from the selected unit's resolved profile and HealthPoints formatting, including level, research and boss modifiers. No new wire fields or gameplay rules are required.

Retain a match-scoped unit id, resolve it from current observed state on updates, and refresh displayed health/stats without replaying events. Pause or transport loss freezes values at the last authoritative observation. Close the popup on death, removal/transfer away from the observed city, observed-city change, fresh match/session end, or when opening a blocking modal. Do not inspect reserves, undeployed arrivals or retained death bodies that have no visible living model.

Integrate unit hits with the pending click/drag gesture from decision 5. A short eligible world click picks the nearest visible living unit model before a plot behind it; overlapping unit targets resolve by hit distance with a stable id tie-break. Picking bounds follow the rendered pose/anchor and camera rather than raw future positions. A unit click changes only local inspection, retaining the selected building slot. Crossing the drag threshold suppresses both unit and plot selection and dismisses an existing inspector at drag start. Health bars/numerals remain input-transparent so clicks reach world picking.

Clicking inside the inspector keeps it open and blocks world input behind its rectangle. A click elsewhere, including another HUD control or plot, dismisses the inspector and performs the eligible normal action once; clicking another unit directly replaces the contents in one gesture. Observe outside presses without consuming the normal target event; newly opened inspector state must not dismiss itself from its opening click. Opening the popup does not pause combat, submit commands or alter readiness. Camera fitting continues to prioritize the ordinary overview; the temporary inspector can cover part of the world while open but must not overlap resource totals or match controls.

Alternative: turn bars into buttons or make the inspector modal. Rejected because bars must remain input-transparent and inspection should coexist with combat, camera navigation and ordinary controls. Reusing the character model avoids inventing portrait assets and keeps unit appearance consistent.

### 7. Application-owned return confirmation

Expose a Settings return-request event with session-only visibility, wired by `GameApplication`. Use an application-owned confirmation dialog included in modal/focus observation. Suggested text: solo, “Return to menu? Unsaved game progress will be lost.”; host, “Return to menu? This ends the session for everyone and game progress will be lost.”; guest, “Leave this game and return to menu? Your city remains with the host while this session is running.” Cancel has default focus; Escape/window close cancel and return to Settings.

On explicit confirm, close confirmation and Settings without deferred focus restoration to destroyed controls, then invoke the existing leave path once. Do not broadly bypass the current modal guard. Gate stale callbacks by current session/screen; if the session ends while confirmation is open, close or rebase to valid menu focus. Keep solo discard, host teardown and guest retained-city/resume semantics, preferences and continuous music.

Alternative: put leave logic into ClientSettings. Rejected because Settings owns local preferences while GameApplication/session owns session lifetime.

## Risks / Trade-offs

- [Nine grid cells plus categories and prices may exceed 180px] → Use compact spacing and appropriate text wrapping, with long explanations in inspection. Verify actual control bounds at both reference sizes rather than scaling all text blindly.
- [Top-right table obscures plots or battle] → Fit/position the overview against occupied UI rectangles; confirm all nine plot targets and the battle approach in source and packed captures.
- [Delaying selection changes mouse ordering] → Test short clicks, building roofs, drags across plots, release outside world, modal/focus interruption and a subsequent normal click with the existing actual-input harness.
- [Space activates focused Ready or a modal control] → Use normal Godot event consumption and explicitly test that consumed Space cannot reset or leak to gameplay.
- [Confirmation overlaps existing Settings modal guard] → Make confirm an explicit ordered modal-close/leave transition and verify cancellation, repeated inputs and late session callbacks.
- [Unit click conflicts with drag or plot selection] → Resolve selection on short-click release with one ordered nearest-hit decision, and verify drag suppression, popup switching and outside-click action delivery through actual input.
- [Inspector data or preview survives a casualty/session] → Scope selection to match and unit identity; close invalid selection and release the preview with the tabletop. Verify live damage, pause/loss freeze and lifecycle cleanup using existing combat captures.
- [Roman levels or popup clip at supported sizes] → Include numeral extents in bar visibility and capture inspector/resource/HUD bounds at both sizes.
- [HUD cleanup loses useful economy feedback] → Retain exact costs and explicit Details inspection, and preserve current cooperative, reserve, reward and affordability assertions.

## Migration Plan

No saved-state, protocol or dependency migration is needed. Implement presentation and update its runner observations/drivers in one coherent change. Run a full CI baseline before implementation (reuse only if source/environment inputs match), cheap applicable checks while editing, selected affected UI slices, then full CI including exports. Retain timing JSON, observations and PNGs under owned ignored log directories. Rollback restores presentation and its verification expectations together; no publishing step is involved.
