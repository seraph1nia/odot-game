# Tasks

Planning only: all tasks below await separate explicit implementation approval.
Specs are skipped because this guide changes no verification behavior.

## 1. Compact task-based guide

- [ ] 1.1 Add the recommended "Choose a check" section at the start of README's Verification and exports section, covering the six routine tasks in design.md with existing commands, preparation, relative cost and coverage; verify every example against mise.toml and the current runner dispatch/options/selectors/package gate without executing game commands.
- [ ] 1.2 Link current policy, prerequisites, selector details, planning conventions and verification evidence, keeping profiles, native packages and paired Steam acceptance separate; verify local links/anchors resolve and the wording preserves partial coverage, unchanged-baseline reuse, full substantial-task/normal-CI gates, user-managed prerequisites and software-rendering/audio limitations.

## 2. Documentation and planning integration

- [ ] 2.1 Run documentation/link consistency, `openspec validate poc-verification-command-guide --strict` and the planning validator through the implementation brief's permitted entry point (ordinary task: `mise run planning-validate`); record actual commands/results and any unexecuted gap, verifying no game test/export or benchmark is added for this documentation-only change.
- [ ] 2.2 Record the exact implementation base/head and final diff for independent review; verify content changes are limited to the README guide and authorized change/lifecycle bookkeeping, all acceptance examples agree with existing guidance, and game/runner/tests/AGENTS/canonical specs/locks and other proposals remain unchanged, without syncing, archiving or landing in the implementation worker.
