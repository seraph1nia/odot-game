# Tasks

## 1. Shared hex and support placement

- [x] 1.1 Establish or reuse an unchanged full `mise run ci` baseline; record source/environment inputs and evidence, and confirm the final framing/travel interfaces of `add-tabletop-camera-controls` before integration. Report missing prerequisites without installing tools.
- [x] 1.2 Extract deterministic starting-landscape data and reusable asset placement/rendering under `src/Game`; inspect bundled terrain/structure bounds, distinguish embedded terrain from overlay props and define footprint/support metadata. Verify the nine slot identities and authored tile joins remain unchanged through locked build and geometry observations.
- [x] 1.3 Anchor home, defender, buildings, upgrade bases, resource groups and decorations to supporting hexes/surfaces; replace fixed tower lift with support attachment and update shot origins, labels, picking bounds and markers. Extend existing economy assertions for raised contacts, centered footprints and tower/base selection; run `mise run test` for affected runner changes and `mise run test-ui --scenario economy`, retaining cooperative assertions and PNG contact evidence.
- [x] 1.4 Document placement conventions and the cosmetic-only terrain boundary in README.md and update economy risk/coverage notes in docs/verification.md; verify documentation agrees with observed contacts and unchanged gameplay behavior.

## 2. Continuous countryside coverage

- [x] 2.1 Separate playable overview bounds from scenery bounds and derive conservative hex coverage from final viewport/camera geometry, elevation range, pan envelope and overhang margin. Cache assets and update deterministic cell ranges only when needed; verify independent coverage assertions catch a deliberately undersized range and `mise run test` passes for any new cheap runner geometry checks.
- [x] 2.2 Extend the landscape across required ranges with continuous river joins, both banks, elevation transitions and peripheral decoration, protecting plot and combat sightlines. Extend economy's existing camera checkpoints for overview/resizing and permitted travel extremes at supported sizes; run `mise run test-ui --scenario economy` and inspect captures for exterior edges, seams, full-city readability and unchanged overview scale.
- [x] 2.3 Record countryside coverage behavior, model counts, measured additional scenario cost and software-rendering limits in README.md/docs/verification.md; verify recorded ranges/timings correspond to retained logs and PNG evidence.

## 3. Shared menu backdrop and packed parity

- [x] 3.1 Replace GameApplication's separate patch with a passive instance of the shared empty starting landscape, consistent asset scales/lighting and full-menu viewport coverage. Extend launcher observations/assertions for actual menu/solo static placements, resizing and return after construction; run `mise run test-ui --scenario launcher`, retaining no-match/no-connection, keyboard, modal and cleanup assertions and menu/solo captures.
- [x] 3.2 Add compact shared-layout/contact and coverage assertions to the existing exported-package route; verify with `mise run test-ui --scenario exported-package` against current explicitly prepared exports or within final CI. Preserve that slice's no-implicit-rebuild behavior and inspect packed menu/solo PNGs for parity.
- [x] 3.3 Update launcher/package risk descriptions and README.md/docs/verification.md for the shared starting backdrop, return-to-menu behavior and measured verification cost; check descriptions against source and exported evidence without adding a new full-match scenario.

## 4. Integration gate

- [x] 4.1 Restore `Odot.slnx` in locked mode, format changed C# using `dotnet format Odot.slnx --no-restore` and run full `mise run ci` after the coherent implementation, including ordinary headless checks, sequential exports and package verification. Record complete results and evidence, and explicitly identify any unexecuted coverage.
- [x] 4.2 Run `openspec validate fill-countryside-and-align-assets --strict` and review final code/docs/spec consistency with the integrated camera change; verify presentation remains under src/Game, rules remain unchanged, headless roles instantiate no scenery and unrelated workspace changes are preserved before marking tasks complete.
