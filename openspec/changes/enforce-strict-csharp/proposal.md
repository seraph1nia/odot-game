# Proposal

## Why

Compiler warnings already fail builds, but ordinary compilation did not enforce the repository's C# style rules and used only the default analyzer set. Strict checks should catch nullable, code-quality and style regressions before running game processes or producing exports.

## What Changes

- Explicitly enable the locked SDK's recommended .NET analyzers and build-time style enforcement across every C# project.
- Require file-scoped namespaces, readonly fields where possible and verified formatting.
- Fix reported findings: invariant numeric parsing/formatting, namespaced Godot types, cached JSON serializer options, specific exceptions, matching override parameter names and checked process-group signal results.
- Document the existing formatting/build/unit/Godot verification loop and verify enforcement with rejected diagnostic probes and full CI.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `linux-test-execution`: Require strict C# compilation and style/formatting gates before source verification and exports.

## Impact

Shared MSBuild properties, `.editorconfig`, Godot presentation classes, DevRunner, cheap xUnit fixtures and verification documentation. The existing CI harness and scenario set provide integration coverage; no new analyzer packages or tool/dependency lock changes are required.
