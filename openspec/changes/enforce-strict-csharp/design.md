# Design

## Context

See `proposal.md` for motivation. Every C# project already inherits nullable checking, implicit usings, warnings as errors and locked restores from `Directory.Build.props`. The existing CI runner verifies formatting, builds/imports once, overlaps cheap tests with bounded network scenarios, then checks source UI before sequential exports and package smoke.

## Goals / Non-Goals

**Goals:** Make ordinary builds enforce useful code-quality and style diagnostics consistently across engine, core, tooling and tests; resolve every newly reported finding.

**Non-Goals:** Change gameplay rules, add expensive scenarios, introduce another test framework or update tool/dependency locks.

## Decisions

- Use `EnableNETAnalyzers=true`, `AnalysisLevel=latest-recommended` and `EnforceCodeStyleInBuild=true` in shared properties. The exact SDK lock fixes the analyzer version. An additional analyzer package would duplicate the available SDK capability and require unnecessary dependency changes; enabling every optional rule would introduce unrelated policies.
- Keep `.editorconfig` as the style source: file-scoped namespaces and eligible readonly fields are warnings promoted to errors; formatting drift fails verification. Fix findings instead of suppressing them.
- Parse and format machine-facing numbers and evidence timestamps with invariant culture; reuse cached JSON serializer options. Move Godot presentation types into `Game`, preserving their script resource paths, and match Godot's override parameter names.
- Check native process-group signal results. Linux ESRCH means the owned group already exited and is successful cleanup; any other native failure raises an attributable exception. Existing bounded descendant cleanup assertions remain in force.
- Reuse the existing full CI gate. Temporary diagnostic probes establish that nullable, analyzer and style warnings fail ordinary compilation and formatting drift fails verification. A cheap xUnit regression checks runner arguments under a culture with a different positive sign.

## Risks / Trade-offs

- Future intentional SDK changes can introduce new recommended diagnostics; resolve them as part of that toolchain update.
- Namespaces affect Godot-generated bindings; source network/UI and both exported-package smoke checks verify the existing script paths and protocol still work.
- Cleanup can now surface native errors previously ignored; existing runner cleanup fixtures and the private-display lifecycles exercise successful and already-exited group handling.

## Migration Plan

Apply shared enforcement, resolve findings, format after locked restore and run the full before/after CI gate. No deployment or dependency migration is required. The implementation and verification are complete; evidence is recorded in `docs/verification.md`.
