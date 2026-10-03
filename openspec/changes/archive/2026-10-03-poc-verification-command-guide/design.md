# Design

## Context

See [proposal.md](proposal.md#why). The [fresh preflight](../../../planning/evidence/setup-agent-workflow-poc/scenario-2-proposal.md#standalone-proposal-preflight)
distinguishes documentation navigation from existing command behavior and pending
canonical synchronization. README already owns command descriptions and setup;
AGENTS owns agent execution policy; `docs/verification.md` records coverage,
measurements and limitations. No new source behavior needs specification.

## Goals / Non-Goals

**Goals:** Let a reader match a routine task to an existing check in one compact
section, understand its preparation and coverage, and follow links for detail.
Keep selection examples reviewable without launching game processes.

**Non-Goals:** A comprehensive reference, automatic recommendation logic,
additional runtime checks, altered cost/CI policy or specification reconciliation.

## Decisions

1. **Recommend a README entry point for routine human and agent readers.** Place
   a short "Choose a check" section before existing Verification and exports
   detail, linked to AGENTS and existing documentation. This puts selection beside
   its maintained reference. A separate guide would add another surface; runner
   help/recommendation changes would exceed the selected documentation scope.
   These are proposed defaults for later approval, not human answers from intake.
2. **Use six task rows and a short coverage note.** The future guide should use
   the examples below, with brief preparation/relative-cost/coverage information.
   Link the full selector lists rather than reproduce an option matrix. Relative
   cost means engine-free, real peers, private graphics or full exports; do not
   turn historical measurements into execution-time promises.

   | Routine task | Existing check / guidance | Preparation and coverage to explain |
   | --- | --- | --- |
   | Change numerical rules or runner logic | `mise run test` | Locked restore/build of cheap C# suites; no Godot. Frequent when applicable. |
   | Diagnose affected networking or transfer | `mise run test-network --scenario redistribution` | Standalone source preparation and headless real peers; selected partial coverage. |
   | Change rendered controls or recovery | `mise run test-ui --scenario reconnect` | Standalone source preparation and owned Linux display prerequisites; selected partial coverage. |
   | Check existing exported presentation | `mise run test-ui --scenario exported-package` | Existing client/server exports required; no source preparation or implicit package rebuild; selected partial coverage. |
   | Start/finish a substantial implementation | `mise run ci` | Full local Linux source gates, ordered exports and headless/graphical package checks; reuse an unchanged successful before baseline. |
   | Edit documentation or planning only | Relevant link/content consistency; `mise run planning-validate` for planning; `openspec validate CHANGE-ID --strict` for changed proposals | No automatic game/export run. Ordinary mise planning tasks still restore/build DevRunner, while starting no Godot. |

   The examples are witnesses to documentation consistency, not newly admitted
   tests or a claim that these selected slices suit every networking/UI edit.
   The guide should direct readers to choose the slice affected by their task.
3. **Keep coverage and prerequisites explicit.** A filtered pass covers its slice;
   it cannot replace a full required gate. Ordinary CI triggers retain all gates.
   Successful checks repeat only after relevant changed inputs, failure or an
   unresolved concern. Link README setup for user-managed locked tools and private
   display prerequisites, and verification records for ignored log/PNG/timing
   evidence. Missing prerequisites are unexecuted; failed assertions are failed.
   Software graphics and silent audio do not establish native performance or
   listening quality. Link runtime profiles, native package checks and two-account
   Steam acceptance separately; do not expand the six routine rows.
4. **Validate documentation, not gameplay.** Compare the future section with
   `mise.toml`, `Program.cs`, `Runner.cs`, `Options.cs`, `Scenarios.cs`,
   `PrivateDisplay.cs`, `Exports.cs`, AGENTS and existing README guidance. Check
   links and planning/OpenSpec consistency. No new tests, builds, game runs,
   exports or benchmarks are needed just to demonstrate a docs edit.

## Risks / Trade-offs

- [Duplicated facts drift] → Keep rows short and link current selector/reference
  owners; compare examples with checked-in commands during review.
- [Canonical and completed-delta guidance differ] → Preserve the recorded
  pre-existing drift and separate sync scope. Document current supported commands
  consistently with source/root guidance; do not edit or restate obsolete specs.
- [Guide usefulness remains unmeasured] → Review the six concrete task examples;
  make no quantified usability claim or invented confusing-user incident.
- [Later README work overlaps] → Reconcile ordinary text integration with the
  separately approved research change; no hard dependency or conflict is inferred.

## Migration Plan

After explicit implementation approval, add the compact section, check its
examples/links and record applicable consistency results. Revert that section if
its organization is rejected. No dependency, data, deployment or runtime migration
is involved. Implementation approval, independent review and later landing remain
separate workflow steps.
