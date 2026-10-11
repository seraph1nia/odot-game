# themed-ui Specification

## Purpose

Give all game-owned menus and match controls the pinned authored woodland native Godot UI presentation, preserving real authoritative data/actions, services and readable native interactions without off-the-shelf pack dependencies.

## Requirements

### Requirement: Bundled authored woodland UI resources
Graphical clients SHALL use the accepted woodland native kit from odot-game-assets, pinned by exact source revision and production member/provenance hashes. Selected resources SHALL be available from a clean checkout and in graphical exports without runtime downloads or paid packs. The original menu seal, editable authored SVG resources and licensed unmodified DejaVu Serif font SHALL retain their provenance and complete applicable notices. Preview/showcase fixtures and the supplied hud.png reference SHALL NOT become runtime artwork or game state. Superseded Trio pack resources SHALL be removed only after replacing their live consumers and fallbacks, retaining exact removed-member evidence and historical notices. Headless roles and stripped dedicated-server exports SHALL remain operable without loading these graphical resources.

#### Scenario: Run an offline graphical export
- **WHEN** a player launches an exported client without internet access and opens its menus and a solo match
- **THEN** kit-styled controls, mapped icons and unit health bars load from bundled resources
- **AND** no asset download or paid content is required

#### Scenario: Run a stripped server
- **WHEN** a dedicated-server export runs with graphical resources stripped
- **THEN** its normal authority and network behavior works without loading UI kit assets

### Requirement: Consistent presentation across owned menus
The start screen, multiplayer entry, cooperative lobby, in-match HUD and contextual actions, pause controls, outcome and disconnected/reconnect states, Settings Graphics/Audio/About pages and update controls, friend-invitation list, and join confirmation SHALL share one coordinated woodland native panel, button and control style with dark insets, bark/leaves, parchment/amber primaries, cream serif copy and violet/blue accents. Normal, hovered, pressed, disabled, selected and keyboard-focused states SHALL remain distinguishable where applicable. The refresh SHALL preserve each screen's existing actions, feedback, modality and session behavior. Platform-owned Steam UI and operating-system window decorations are outside this contract.

#### Scenario: Move from menus into a session
- **WHEN** a player visits the start screen, multiplayer entry, a hosted lobby and match controls
- **THEN** their panels and controls share the chosen kit styling while the existing hosting, readiness and session actions remain available under their existing eligibility rules

#### Scenario: Open owned dialogs
- **WHEN** a player opens Settings including About, the friend-invitation list, or join confirmation
- **THEN** the dialog surface, tabs, buttons, dropdowns, sliders and scroll controls have coordinated styling and visible keyboard focus
- **AND** existing modal input protection and focus restoration continue to work

#### Scenario: Display a paused or disconnected match
- **WHEN** a match pauses, ends, or loses its connection
- **THEN** the corresponding controls and messages retain the shared styling
- **AND** pause/resume, return and reconnect remain governed by current authoritative state and session mode

### Requirement: Text-first native controls with original scan aids
Owned command buttons SHALL retain explicit text, exact quotes and native affordances rather than depend on semantic icons. The original fixed-aspect menu seal and pointer-inert woodland ornament MAY accompany menus. The top-right resource table SHALL pair its six explicit names and exact amounts with the original resource atlas cells; unknown icon meanings SHALL fall back to text. Native affordances such as dropdown arrows, check marks and scrollbar controls SHALL remain usable. A bundled gold/buy icon on locked world plots SHALL be the explicit exception; unit health bars with Roman level markers SHALL remain available, while full unit name, role, faction and boss identification SHALL be available in unit inspection instead of overhead name/code labels. Actions SHALL retain readable text labels and applicable explanatory tooltips; resource totals and costs SHALL retain exact numeric values and identifiable resource names. A concept without a suitable original icon SHALL retain a readable text representation instead of using a misleading icon or requiring additional paid assets. Icons SHALL NOT change hit targets, command meaning, action eligibility or authoritative values.

#### Scenario: Identify a resource and its cost
- **WHEN** the player views resource totals and an eligible construction, recruitment or research action
- **THEN** resources and costs use explicit text names and exact authoritative amounts, with optional original scan aids
- **AND** unsupported resource icons fall back to explicit text

#### Scenario: Navigate an icon-enhanced action
- **WHEN** a player navigates controls using the keyboard or inspects a disabled action
- **THEN** labels, focus and disabled feedback remain understandable without inferring the action solely from an icon

### Requirement: Readable responsive menus and HUD
Kit panels and controls SHALL scale without visibly stretched borders or clipped essential labels. At 1100x820 and 1280x720 and after supported display changes, essential actions, status, costs and modal controls SHALL remain reachable. The bottom HUD SHALL target approximately 180px at both reference sizes, 60% of its previous 300px height, with compact controls and intentional inspection/scroll surfaces for longer details rather than expanding back to the previous height. The narrow top-right resource panel SHALL remain bounded around its native stock/income/upkeep contents and SHALL NOT cover essential world targets at the fitted overview. The temporary unit inspector SHALL fit at centre-right below the resource table and above the bottom HUD without clipping its essential information or covering resource totals and match controls. The bottom HUD SHALL continue to leave the city's nine plots, home and battle approach visible and selectable. Decorative images and information-only icons SHALL NOT intercept world or control input; modal surfaces SHALL continue to block underlying actions.

#### Scenario: Use the minimum supported layouts
- **WHEN** the player views menus, settings and contextual build/recruit/research controls at 1100x820 or 1280x720
- **THEN** essential controls and values remain readable and inside their usable viewport or intentional scroll region, the construction area has three columns and three row positions plus category selection, and the lower HUD stays approximately 180px high
- **AND** all nine world plots and the battle approach fit above the bottom HUD

#### Scenario: Click through decorative imagery
- **WHEN** the player clicks a selectable world plot near an information-only icon or health bar while no modal is open
- **THEN** the decoration does not consume the plot input
- **AND** opening a modal continues to prevent underlying world and match actions
