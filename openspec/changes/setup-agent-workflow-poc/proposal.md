# Proposal

## Why

Odot has OpenSpec and a substantial verification harness, but no durable idea/roadmap workflow or single conversational agent that supervises isolated implementation and independent review. Validate that existing FirstMate, Pi, Herdr, Codex and OpenSpec facilities can provide that workflow for this repository before adding another orchestration layer.

## What Changes

- Establish four logical roles: Pi/FirstMate Orchestrator and fresh Codex Scout, Shipper and Reviewer workers. FirstMate owns execution and Treehouse-backed worktrees; Git-tracked project artifacts own product and engineering decisions.
- Add idea intake, mandatory proposal preflight, deterministic roadmap analysis, single-change implementation and independent review skills alongside the existing OpenSpec skills.
- Add `planning/README.md`, `planning/roadmap.yaml`, idea and idea-archive conventions, durable workflow evidence, and an ADR describing the project-wide authority boundary. Preserve existing specifications, active changes and uncommitted work.
- Add a small C# planning validator and next-work selector in DevRunner, with cheap xUnit coverage and mise commands. Validate references, ordering, cycles, state, promoted ideas and completion evidence; selection follows queue order and hard dependencies.
- Provide tracked configuration templates and a thin launch entry point for an external, version-recorded FirstMate checkout/home, Herdr backend and Codex dispatch. Report missing tools for user-managed installation; do not patch upstream or install tools automatically.
- Require explicit implementation approval, a fresh read-only Reviewer, at most one implementation repair and a second fresh review. Return specification failures and product decisions to the user. Gate sync/archive/completion on current independent PASS evidence; keep merging explicitly approved.
- Run five independently documented live POC scenarios against Odot: fuzzy intake, proposal, straightforward implementation, injected defect/repair, and genuine specification ambiguity. Record outcomes, restart behavior and limitations; unexecuted scenarios prevent a viability claim.
- After the baseline passes, prepare an Atomic comparison experiment without integrating Atomic.

## Capabilities

### New Capabilities

- `agent-project-workflow`: Durable planning state, deterministic selection, role/approval boundaries, existing-runtime integration, independent verification and live POC acceptance for this project.

### Modified Capabilities

None. Existing game behavior and verification requirements remain applicable; this change adds workflow tooling rather than redefining game/network/UI acceptance.

## Impact

Planned additions affect root guidance, `.agents/skills/`, `planning/`, `docs/adr/`, workflow setup/runbook documentation and templates under `tools/AgentWorkflow/`. C# validator/selector code belongs in `tools/DevRunner`, tests in `tests/DevRunner.Tests`, and commands in `mise.toml`. A maintained YAML parser may require an intentional, narrowly scoped locked NuGet dependency addition. No changes to game numerical rules, presentation, tool versions, asset provenance or existing active proposals are implicit.

Pi, a FirstMate checkout and Treehouse are missing from the inspected environment; Herdr 0.9.1 and Codex CLI 0.160.0 are available. Upstream FirstMate bootstrap also declares auxiliary tool prerequisites that must be reported honestly. Planning creates only this OpenSpec change; applying it and executing workers require a separate request and available user-managed prerequisites. Atomic, databases, external product trackers, custom messaging, parallel change implementation and autonomous merging are excluded.
