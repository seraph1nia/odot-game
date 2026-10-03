# Verification evidence

## Before baseline

- Base commit: `0e1d4b7` (before implementation; only planning artifacts were untracked).
- Command: `mise run ci`.
- Result: **failed**, exit 1, 176.85s; this is not a passing baseline.
- Console: `logs/rebrand/before-ci.log`.
- Runner evidence: `logs/20261003-131519-fde060fa/`.
- Observed failure: source UI `launcher` reached ArcheryRange construction, then required `RecruitRanged` was missing, hidden or disabled. Detailed owned evidence is under `launcher-worker/launcher/` and `display-launcher/`.
- Restore, format, strict build, rules partitions (438 tests), runner tests (145 tests), network and earlier UI slices passed before the failing launcher gate. Exports/package checks were not reached.
- Firstmate identified independent economy/combat fixture corrections as another worker's responsibility. This rebrand will not duplicate those changes; final validation must use landed corrections through ordinary branch synchronization if necessary.

## After implementation

- `dotnet format Odot.slnx --no-restore`: passed; log `logs/rebrand/format.log`.
- `mise run test`: passed; 438 gameplay tests and 148 runner tests, including new update-copy/legacy-request identity cases, Linux install/upgrade desktop display assertions and Windows installer/source data-identity assertions. Evidence: `logs/20261003-132407-2cb4fc3a/`; console `logs/rebrand/test.log`.
- Documentation review: current title/overview/Steam/distribution copy now uses The Common Watch, while command examples, release URLs, filesystem paths, Windows shortcut aliases and authorship/provenance remain unchanged.
- `git fetch origin main` after cheap checks: remote still at baseline `0e1d4b7`; independent fixture corrections are not landed yet.
- `mise run test-ui --scenario launcher`: **failed**, exit 1, 34.62s; console `logs/rebrand/launcher.log`, evidence `logs/20261003-132559-e0848ce5/launcher-worker/launcher/`. The unchanged baseline `RecruitRanged` disabled/missing fixture failure repeated after ArcheryRange construction. The rebrand did not change this economy setup.
- Before that failure, live public title/window identity, unchanged legacy data leaf, cooperative positioning, text fit and complete label bounds passed on start and multiplayer menus at 1100x820 and 1280x720. Captures: `menu-1100x820.png`, `menu-1280x720.png`, `multiplayer-1100x820.png`, `multiplayer-1280x720.png`; these are partial coverage, not a passed launcher scenario. I inspected the 1100x820 menu PNG and found the title/copy legible and unclipped.
- Return-from-solo, hosted routes, packed branding and final full CI remain unverified until the independent baseline correction lands. No manual visual approval is required by the latest direction; rely on owned evidence and automated checks.
- Completed spec deltas are to be synced before final handoff under the follow-up instruction; synchronization is pending implementation validation.
