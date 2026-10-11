# Repository guidance

The Common Watch is a C# Godot cooperative game with local, playing-host and dedicated authorities.
Keep numerical rules in `src/Game.Core`, engine/presentation code in `src/Game`,
and verification/process supervision in `tools/DevRunner`. Use `Odot.slnx` for
the repository; Godot also needs its smaller `src/Game/Game.sln`.

Use the declared, locked Godot **.NET** and SDK versions. Local installation of
tools and OS packages is user-managed: report missing prerequisites rather than
installing/upgrading them. Keep NuGet restores locked except during an intentional
dependency update. Preserve tool/dependency locks and asset provenance. Generated
`.godot`, `bin`, `obj`, `dist`, sessions and logs stay untracked. Format changed
C# with `dotnet format Odot.slnx --no-restore` after restoring the solution.

## Choose checks by cost and affected behavior

Testing is headless by default. Identify the changed behavior, run targeted fast
checks during implementation, then broader fast checks once coherent. Add focused
engine/transport verification only when the changed boundary needs it. Deep
qualification is explicit, scheduled or risk-driven, not a before/after tax on
every feature. Repeat passed checks only for changed relevant inputs, failure or
an unresolved concern; documentation-only changes need consistency checks.
Record exact coverage, evidence and unexecuted checks; a filtered pass is partial.

Load the matching repository skill before testing work:
- [testing-strategy](.agents/skills/testing-strategy/SKILL.md): choose the cheapest meaningful tier.
- [testing-commands](.agents/skills/testing-commands/SKILL.md): local/targeted/CI commands and preparation.
- [testing-hygiene](.agents/skills/testing-hygiene/SKILL.md): add, consolidate or remove coverage.
- [diagnose-flaky-tests](.agents/skills/diagnose-flaky-tests/SKILL.md): investigate failures and synchronization.

## Execution and ownership

Never parallelize mutations of shared restore/build/import/export outputs.
Prepare once, then run isolated completed binaries with bounded admission.
Verification owns per-client preferences, credentials, endpoints and engine logs;
intentional restarts reuse only that client's state. Await every owned child on
failure/cancellation before removing runtime data. Never change developer
preferences, kill unrelated processes or log credentials. Keep one ordered driver
per child's event reader. Reuse `Child.cs` and `Scenarios.cs`, not a new supervisor.

Ordinary CI and the default test commands launch no graphical processes.
Graphical acceptance needs explicit authorization and `ODOT_ALLOW_GRAPHICAL_TESTS=1`;
select one scenario. Do not use a desktop fallback. Real Steam invitations require
separate accounts/machines and are not ordinary CI. Missing prerequisites are
unexecuted/nonzero, not success. `dev` still deliberately opens one playable ENet
host plus 1..3 guests; `play` opens the normal menu. Do not change that development
behavior to make verification headless.

See [README.md](README.md) for project entry points and
[docs/verification.md](docs/verification.md) for coverage, measurements and limitations.
