# Design

## Context

See proposal.md for motivation and scope. `GameApplication.cs` currently constructs an ODOT label at 42px with “Nine plots. One countryside.”; the same panel serves start and multiplayer. `project.godot` has `config/name="Odot - Nine Tiles"`, which determines the default user-data directory. Existing release-stamping tests deliberately retain that name. Friend and update instructions contain Odot. Linux packaging writes `odot.desktop` with `Name=Odot`; Windows has separate visible product strings and stable GUID, location, group and shortcut paths. The current UI observation includes user-data path and control bounds but no explicit brand text/window-title fields.

The launcher, desktop-installation and themed-ui specifications have been read; their existing behavioral contracts are unchanged. The active economy change is independent and must not be edited.

## Goals / Non-Goals

**Goals:** Separate presentation identity from persistence/installation identity, add minimal shared-defense language to the existing menu, and validate the longer title without another E2E fixture.

**Non-Goals:** No identifier migration, new assets or module redesign. Do not rename historical documents, copyright holders, solution commands or third-party provenance as if they were current marketing copy.

## Decisions

### Keep Godot's legacy project identity; override the visible window title

Set the root graphical window title during application initialization, using the same public title as the menu. Leave `config/name`, custom-user-directory settings and release stamping unchanged. This keeps user:// resolution identical across operating systems and needs no filesystem migration. Renaming config/name would silently relocate preferences and credentials; a custom directory replacement needs platform-specific compatibility work disproportionate to this rebrand. Headless roles remain unaffected.

Use a small shared presentation identity constant for the title and menu positioning, available to client copy. Keep it outside numerical gameplay rules. Do not introduce a service or broad localization framework for a handful of strings.

### Theme through language and hierarchy, not new visuals

Use title case **The Common Watch** and concise positioning such as “Build and provision your village.\nHold together against automatic waves.” Supporting current documentation explains provisioning, automatic combat and the shared outcome; menu wording alone need not restate every rule. Adjust title font/panel width only as necessary to fit the existing Trio layout at both reference sizes. Retain scenery, assets, controls and colors rather than invent bells, watchtowers or a new narrative.

### Rebrand display strings, retain technical aliases

Update friend/update text, Linux desktop `Name`, Windows `AppName`, `AppVerName` and launch description, and packaging user messages. Keep filenames, archives, repository/update requests, environment variables, Steam AppID, executable targets and Inno GUID intact. Preserve Windows shortcut/group names as legacy aliases to avoid adding duplicate shortcuts or changing existing installer/uninstaller ownership. Explicitly document this narrow visible legacy exception alongside Linux `odot` commands and persisted data paths. This is preferable to an untested Windows shortcut migration.

Update current README and gameplay/distribution descriptions, but preserve technical command examples, actual filesystem names, authorship/legal attribution and asset notices. Add an identity note explaining why technical paths still say Odot. No version bump or publication follows from this change.

### Extend existing verification at the affected boundaries

Use cheap packaging/update/source-identity assertions for display strings and stable identities. Extend the existing `launcher` UI observation with live title, positioning, window title and title control bounds, asserting fit and consistency at its already-tested sizes and after returning from solo. The defect caught is stale/clipped branding or a window-title reset: cheap source assertions cannot establish native layout, and existing launcher checks do not inspect branded labels. Added execution cost is a few assertions/probes within the owned fixture; no additional display, battle or standalone scenario is justified. Existing source/packed CI then verifies export inclusion.

## Risks / Trade-offs

- [Longer title may widen the panel or clip] → Verify current control bounds plus text measurement and capture the existing launcher frames at both sizes.
- [Legacy project/shortcut names remain visible in technical tooling] → Deliberately preserve and document them; public in-game/installer identity is new, technical paths are not.
- [Accidental broad string replacement breaks compatibility or provenance] → Inventory changes by surface, assert legacy identities, review the diff, and avoid replacing all Odot occurrences.
- [Warm wording could imply a purely cozy builder] → Keep automatic waves, persistent damage and collective survival explicit in the overview.
- [Availability is unknown] → No trademark, store or domain clearance claims; no external clearance work included.

## Migration Plan

No data or identifier migration. Ship through the normal future full-package release after approval, with unchanged version-selection and release tooling. Rollback is a presentation/copy revert; it must not relocate player data or affect running authorities. Do not publish a release or merge from this task.
