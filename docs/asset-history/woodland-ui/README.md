# Original woodland kit inventory

`upstream-manifest.json` preserves the receiving import's complete original
46-member inventory at asset MAIN revision
`12bc5ef02765c1d4b725013791f7420918aacd64`, including every source member hash,
source-document binding, original package identity and superseded Trio inventory.
It is historical provenance, not the current runtime selection or an archived ZIP.
The original import bytes remain in git history; the canonical portable components
remain upstream and were not edited by the receiving-game selection.

The [runtime manifest](../../../src/Game/UI/manifest.json) now selects 22 members:
original artwork/import recipes, licensed font, theme, helpers, modal and resource
ledger. The eight unused portable components (skill_tree, army_roster,
city_navigation, match_controls, quoted_action, session_feedback, unit_inspection,
upkeep_summary), each with script, UID and scene, are not shipped or loaded merely
for verification. Their real C# gameplay consumers remain, and live research uses
the separately pinned landed Worldtree. This is a selected subset, not a claim to
ship the complete original portable package.
