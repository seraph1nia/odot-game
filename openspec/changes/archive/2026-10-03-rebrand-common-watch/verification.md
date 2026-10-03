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
- At this stage return-from-solo, hosted routes, packed branding and full CI remained unverified pending the independent correction. No manual visual approval is required by the follow-up direction; rely on owned evidence and automated checks.

## After landed correction and guarded branch synchronization

- The independent fixture correction landed at `227edbf` via https://github.com/seraph1nia/odot-game/pull/1. A clean `git fetch origin main` and `git rebase origin/main` preserved the bounded rebrand without conflicts; no unlanded fixes were copied. Tested implementation head: `fa7f55cac10bd3f7f65867c724cb4c35eb70cc1a`.
- `mise run test-ui --scenario launcher`: **passed**, 95.55s total; console `logs/rebrand/launcher-rebased.log`, evidence `logs/20261003-154959-ce576174/`. This covers both menu sizes, multiplayer, solo return, hosted lifecycle and legacy preferences/private credentials. Branding assertions passed throughout; I also inspected the 1280x720 menu PNG and found it legible/unclipped.
- `mise run ci`: **passed**, exit 0, 314.94s runner / 316.39s total; console `logs/rebrand/after-ci.log`, evidence `logs/20261003-155213-c16c2369/`. Locked restore, format verification, strict build/import, 439 gameplay tests, 152 runner tests, all six network scenarios, all five source UI slices, sequential Linux client/server exports, headless package smoke and graphical exported-package smoke passed. Nothing was uploaded or published.
- Exported branding evidence: `logs/20261003-155213-c16c2369/exported-package-worker/exported-package/packed-menu.png` and `packed-layout-1280x720.png`; live window title, menu copy, fit, unchanged legacy data leaf and actual package exit passed.
- The `game-branding` delta was synced to `openspec/specs/game-branding/spec.md` with all four requirements and six scenarios intact. Strict main-spec validation passed (19 specs), as did strict change validation and whitespace review. Completed artifacts are archived after the implementation/validation checklist is complete.
- Final changes after this CI run are planning/evidence/spec synchronization/archive only, not runtime or package source changes; the successful full run remains applicable.

## Limitations

Linux private-display verification uses owned X11, Mesa software rendering and Dummy audio. It does not establish native GPU/compositor performance, listening quality, physical input or real Steam invitation delivery. Windows display/identity changes have cheap source-policy coverage here, not a native Windows installer execution in this lane; the normal forge CI remains required. Name/trademark/domain/store availability was not checked. Legacy Windows shortcut/group aliases intentionally remain Odot. No save/credential migration, protocol/version bump, release publication or gameplay change is part of this rebrand.
