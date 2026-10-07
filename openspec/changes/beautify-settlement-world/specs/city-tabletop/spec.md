# Spec Delta

## ADDED Requirements

### Requirement: Fixed-layout woodland village connections
The non-battle countryside SHALL present deliberate connected settlement-edge paths, a readable clearing at the existing river bridge, bank-side landmarks and clustered woodland and mushroom pockets separated by negative space. Connections SHALL meet visible bridge banks and settlement edges rather than end arbitrarily in water or building footprints. The composition SHALL be deterministic and shared by the passive menu and fresh village. All nine physical plot/building centers and scales, their ownership/selection identities, home/defender, actual defense projectile origins and authoritative hex topology SHALL remain unchanged. The redesign SHALL NOT change battle-area terrain/scenery, global lighting/camera framing or unit presentation/mechanics. New geometry, shadows and occlusion SHALL remain outside the unchanged battle presentation. Existing authored assets and provenance SHALL remain immutable; no literal increase in building-center distances SHALL be claimed.

#### Scenario: Start in a connected woodland village
- **WHEN** the menu or a fresh city is rendered
- **THEN** clustered non-battle woodland and mushroom pockets frame a connected settlement-edge path and the real authored bridge with clear bank contacts and deliberate open space
- **AND** the nine existing plots, home, defender and battle approach retain their physical positions and readable sightlines

#### Scenario: Grow without displacing battle contributors
- **WHEN** buildings are constructed, upgraded or sold on any slot, including defense towers, at either supported viewport size
- **THEN** every building remains on its unchanged supporting plot, genuine defense shot origins remain at their original measured structure bounds and existing picking/selection actions keep their identities
- **AND** paths and decorative pockets do not overlap building footprints, hide roof targets or obstruct controls

#### Scenario: Verify the unchanged battle boundary
- **WHEN** the redesigned view is compared with its retained before view at the same controlled battle state and camera
- **THEN** combat geometry, scenery, current unit poses/equipment/effects and behavior remain unchanged within the battle boundary
- **AND** intentional non-battle visual changes do not waive asset fidelity, numerical outcome, current-frame or persistence assertions
