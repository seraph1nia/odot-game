# Spec Delta

## MODIFIED Requirements

### Requirement: Meaningful icons with readable information
Owned non-game menus SHALL contain no decorative or semantic content icons, including start/multiplayer menus, cooperative lobby, Settings tabs, About/update controls, friend lists and confirmation dialogs. All owned buttons, including in-game actions and embedded cost displays, SHALL use text without icons. The top-right resource table SHALL use names and amounts without icons. Native affordances such as dropdown arrows, check marks and scrollbar controls SHALL remain usable. A bundled gold/buy icon on locked world plots SHALL be the explicit exception; unit health bars and role identification SHALL remain available. Actions SHALL retain readable text labels and applicable explanatory tooltips; resource totals and costs SHALL retain exact numeric values and identifiable resource names. A concept without a suitable free icon SHALL retain a readable text representation instead of using a misleading icon or requiring additional paid assets. Icons SHALL NOT change hit targets, command meaning, action eligibility or authoritative values.

#### Scenario: Identify a resource and its cost
- **WHEN** the player views resource totals and an eligible construction, recruitment or research action
- **THEN** resources and costs use explicit text names and exact authoritative amounts without resource icons
- **AND** unsupported resource icons fall back to explicit text

#### Scenario: Navigate an icon-enhanced action
- **WHEN** a player navigates controls using the keyboard or inspects a disabled action
- **THEN** labels, focus and disabled feedback remain understandable without inferring the action solely from an icon

### Requirement: Readable responsive menus and HUD
Kit panels and controls SHALL scale without visibly stretched borders or clipped essential labels. At 1100x820 and 1280x720 and after supported display changes, essential actions, status, costs and modal controls SHALL remain reachable. The bottom HUD SHALL target approximately 180px at both reference sizes, 60% of its previous 300px height, with compact controls and intentional inspection/scroll surfaces for longer details rather than expanding back to the previous height. The narrow top-right resource panel SHALL be taller than it is wide and SHALL NOT cover essential world targets at the fitted overview. The bottom HUD SHALL continue to leave the city's nine plots, home and battle approach visible and selectable. Decorative images and information-only icons SHALL NOT intercept world or control input; modal surfaces SHALL continue to block underlying actions.

#### Scenario: Use the minimum supported layouts
- **WHEN** the player views menus, settings and contextual build/recruit/research controls at 1100x820 or 1280x720
- **THEN** essential controls and values remain readable and inside their usable viewport or intentional scroll region, the construction area has three columns and three row positions plus category selection, and the lower HUD stays approximately 180px high
- **AND** all nine world plots and the battle approach fit above the bottom HUD

#### Scenario: Click through decorative imagery
- **WHEN** the player clicks a selectable world plot near an information-only icon or health bar while no modal is open
- **THEN** the decoration does not consume the plot input
- **AND** opening a modal continues to prevent underlying world and match actions
