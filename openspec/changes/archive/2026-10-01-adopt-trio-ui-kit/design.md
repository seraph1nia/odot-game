# Design

## Context

See `proposal.md` for motivation. `GameApplication` creates one `ApplicationTheme` shared by the menu root, `ClientSettings`, `SteamFriendsDialog`, the join `ConfirmationDialog`, and `Tabletop`. Today it supplies flat panels and button states but leaves tabs, popups, sliders, scrollbars and dialog surfaces largely at engine defaults. `Tabletop` uses a bottom panel with resource text, dynamically created city/build/recruit/research buttons and state-dependent session controls. Settings also contains an About page and update buttons.

`UnitView` currently attaches a billboard `Label3D` containing faction/role and numeric health, hides it at death, and samples authoritative `UnitState` through the existing playback clock. Snapshots already include current `Health` and the effective maximum in `Profile.Health`; both use integer hundredths. `Tabletop` owns unit views and camera framing. Existing DevRunner launcher, settings, economy, reconnect and combat scenarios already drive the affected paths and capture frames.

The official [free-kit page](https://moonpunchstudio.itch.io/trio-ui-kit-free) names `TrioUIKit_FreeSample_v2.2.zip` and permits personal/commercial use without required attribution. Its title and body give inconsistent asset counts, so archive contents and included terms must be checked before selecting files. The official [v2.2 devlog](https://moonpunchstudio.itch.io/trio-ui-kit-free/devlog/1656136/free-sample-updated-v22-pro-edition-is-out) confirms the free release and distinguishes the paid full/PRO editions. This proposal has inspected the pages, not downloaded or licensed a paid edition.

## Goals / Non-Goals

**Goals:** Reuse the application theme and existing control trees, centralize asset/icon mappings, and render overhead bars without per-unit render viewports or new gameplay state. Preserve existing city-tabletop, client-settings, game-launcher and game-feedback contracts.

**Non-Goals:** New menu navigation flows, user-selectable themes, new currencies, gameplay balance changes, character/scenery replacement, manual combat, restyling native Steam/OS interfaces, or dependency/tool upgrades.

## Decisions

### 1. Bundle a verified free subset

Place selected originals under `src/Game/Assets/TrioUI/`, accompanied by a README, the supplied license or captured author permission evidence, and a manifest with source page, archive filename/version/hash and selected-file hashes. Retain original names and document any derived variants separately. Prefer SVG originals imported by Godot, with explicit import scale sufficient for supported UI sizes; use supplied PNGs only if an SVG does not import faithfully. Nothing downloads at runtime.

Use one warm/cozy variant if the free archive provides it, otherwise its supplied default palette; this is a recorded visual assumption, not a runtime setting. Exact available filenames and icon inventory are deferrable asset-selection details. Do not infer availability from paid-kit previews. If public permission and bundled terms conflict, resolve permission before redistributing files.

Alternative: load assets remotely or use the paid kit to fill gaps. Rejected because offline exports, free-only scope and provenance require a bundled verified subset.

### 2. Extend the existing shared theme

Use `StyleBoxTexture` with verified nine-slice margins for free panels/buttons, preserving content padding separately from texture borders. The page gives 20px borders for plain/small/notched panels and a 49px top border for the header panel; apply these only to matching files actually present. Add coordinated styles for dialog/window surfaces, tab states, `OptionButton`/`PopupMenu`, `HSlider`, scrollbars and tooltips. Reuse the free button texture with documented tinting where a state is missing; keyboard focus gets a distinct border. Never distort icons with panel stretching.

Keep Godot's existing dialogs, embedded subwindows, tab/dropdown routing and callbacks. Update hardcoded color overrides such as the Steam identity footer when they conflict with the new palette. Ensure dynamic city and friend rows receive the same styles. Validate layout from actual content; if extra padding/icons grow the bottom panel, adjust container spacing and camera framing together so plots and the battle approach remain visible.

Alternative: rebuild each screen as independent scenes. Rejected because the current shared theme already connects these screens and rebuilding would risk focus, modal and session behavior without advancing the visual requirement.

### 3. Centralize semantic icons and preserve text

Introduce a small presentation-only asset/icon catalog with semantic keys, cached textures and explicit mappings to verified free filenames. Candidate meanings include settings, sound, health, back, play, pause, upgrade, friends, army and resources; only activate mappings supported by clear artwork in the archive. Gold, food and wood remain the only currencies. Do not use a gem as a new currency, or a generic coin as a different labeled resource without a clear mapping.

Use `Button.Icon` with bounded sizes for actions. Replace monolithic resource/cost text where needed with compact icon/name/value rows, while retaining exact values and existing observation text or equivalent structured fields. Icons are supplementary; absent mappings render text. Keep stable node names/selectors, existing tooltip detail and eligibility logic. Purely decorative controls ignore mouse input.

Alternative: remove labels to fit more icons. Rejected because unfamiliar sample icons would obscure recruitment, research, costs and keyboard interaction.

### 4. Use one projected overhead bar per living view

Add an information-only Control layer in `Tabletop`, below modal/HUD layers, and a reusable small health-bar component using the verified free bar frame/fill or matching theme primitives if the sample lacks a usable bar. Associate each bar with its match-scoped unit view/id. Project an anchor above the model through the existing camera using the viewport transforms already used for world targets. Maintain readable screen dimensions across the orthographic camera and supported resolutions; reuse textures across units and allocate no per-frame textures or per-unit `SubViewport`.

Compute the fraction from sampled `UnitView.State.Health / (double)State.Profile.Health`, clamped to [0,1]. Never infer maximum from initial observed health, catalog defaults or a hardcoded common maximum; a missing/nonpositive authoritative maximum is a presentation failure to report. Show full-health bars too. Retain short role/faction identification and ground markers; replace the overhead numeric-health string with the bar to reduce clutter. Observe exact current/max values for verification.

Hide bars for dead, undeployed, unfocused or offscreen views and clip to the world area above the bottom HUD. Pause/transport loss use the existing sampled presentation state; viewport resize may still reproject that frozen world state. Removing a view releases its bar immediately, and all bars are disposed with the tabletop on return/session replacement. Reconnect and redistribution use current snapshots and existing view reconciliation without damage animation backlogs.

Alternatives: textured billboard meshes or individual 3D viewports. A projected Control keeps kit textures sharp and screen-readable, reuses UI styling, and avoids the render-target cost of per-unit viewports. It requires explicit projection/clipping and lifecycle ownership, covered by existing combat observations.

### 5. Extend existing verification slices

No new network/UI scenario is planned. Cheap tests cover any extracted health-fraction calculation with wounded/ranked values and clamping; existing numerical rules stay in Game.Core. Extend actual UI observations to expose bar visibility/value/max/projected bounds and loaded theme/icon resources rather than returning intent flags.

Extend launcher/settings/economy checkpoints for kit resources, focus/disabled states, semantic values and layout; use owned friend fixtures and join confirmation without opening real Steam UI. Extend combat for both factions, full/damaged bars, pause and casualty cleanup, and reconnect for immediate restored fractions and absence of duplicates. These defects require rendered controls/camera/lifetime evidence that cheap gameplay tests miss. Reuse current scenario setup and battles, adding bounded assertions/checkpoints without extra full matches or option matrices. Expected cost is a few observation/frame waits within the existing slice, no additional child/display setup; maintain the selectors and observation fields alongside UI changes. Update scenario risk descriptions if coverage expands.

Run `mise run ci` before and after implementation, reusing a valid unchanged before baseline. Restore with locks and format changed C# with `dotnet format Odot.slnx --no-restore`. Use applicable `mise run test` checks during implementation and affected UI slices while iterating. Full CI owns sequential exports and source/package verification. Retain timing/log/PNG evidence under owned ignored `logs/<run-id>/`; manually inspect representative screenshots for border distortion, icon meaning, contrast and bar overlap. Missing user-managed prerequisites are reported, not installed. Planning-only edits require OpenSpec validation and consistency checks, not game CI.

## Risks / Trade-offs

- [Free sample is smaller than promotional previews] → Verify the archive and keep explicit text fallbacks; derive matching primitive styles for missing controls.
- [Panel padding and icon widths crowd the 1280x720 HUD] → Check full control bounds and essential text, then adjust spacing and camera framing together without hiding plots or costs.
- [Projected bars overlap in dense combat] → Use compact consistent dimensions above models, retain role/faction markers and inspect crowded combat frames; do not shift authoritative positions.
- [Kit imports or paths work in source but fail in exports] → Include manifest-listed resources and exercise existing graphical package smoke plus stripped-server smoke through CI.
- [Pause/reconnect leaves stale bars] → Tie bars to sampled view state and owned teardown; assert restored fractions and immediate casualty hiding in current scenarios.

## Migration Plan

Bundle assets, implement the shared theme/catalog, convert menus/HUD, then integrate projected bars and extend existing observations. There is no preference, save or protocol migration. Ship through the normal repository/export pipeline; this change adds no publishing step. Rollback consists of reverting presentation/asset changes and related observation assertions without touching retained credentials or settings.

## Implemented details

The official free v2.2 archive contains 30 Cozy widget SVGs and 40 icon SVGs with matching PNGs. The selected subset has 40 original/derived provenance entries. The user confirmed repository permission after inspection of the bundled redistribution restriction; upstream terms and that context are preserved beside the assets.

Derived button variants remove baked text so native labels remain readable. Separate frame/fill variants support clipped health bars; dropdown, close and slider-knob derivatives use bounded import dimensions. Panels use 20px nine-slice borders, and all four supplied button states remain distinct. Both TabBar and TabContainer receive styles because Godot's internal tab variation resolves the latter. The shared Cozy palette uses parchment surfaces, amber actions, brown text and a teal focus border.

The fixed HUD reservation is 300px, with non-wrapping resource rows and information-only cost labels. Bars are 46×9px, use a 43px fill span and project an anchor 1.35 world units above each sampled unit. Wood, pause, reconnect, friends, exit and update actions keep explicit text where the free inventory lacks a clear icon. Full source/export verification and visual findings are recorded in `verification.md`.
