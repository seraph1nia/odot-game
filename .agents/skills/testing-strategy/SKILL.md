---
name: testing-strategy
description: Choose verification for an odot-game change or redesign its test coverage. Load before selecting a testing tier, changing CI, or adding expensive tests.
---

# Choose the cheapest meaningful proof

Tests exist to catch realistic regressions quickly, not to maximize counts.
Identify the changed production behavior and the defect that a test would catch.
Use [testing-commands](../testing-commands/SKILL.md) for execution and
[testing-hygiene](../testing-hygiene/SKILL.md) before changing coverage.

| Tier | Owner / purpose | Ordinary development |
| --- | --- | --- |
| A | xUnit `tests/Game.Core.Tests` and `tests/DevRunner.Tests`: deterministic rules, authority, snapshot/codec boundaries, presentation calculations, ownership/cancellation/security | Default `test-fast`; no Godot process |
| B | `src/Game/Verification/EngineTests.tscn`: minimal headless SceneTree against production settings, imported rigs/animation, rendering-resource construction | Select `test-engine` when an engine boundary changes; no Main, menu, session or peers |
| C | Existing DevRunner headless real transport and package probes: delivery, retry/resume, original host, failure/cleanup, native managed payload | Select relevant `test-network`; bounded three-case default. Package checks for distribution changes |
| D | `UiSmoke.cs`: one explicitly authorized offline graphical menu/solo/build/recruit/return/Exit witness | Not automatic; headless-only authorization excludes this tier |
| E | `Tier=Deep` C# campaign/calibration/reference replay; headless multiplayer journeys; explicitly gated graphical acceptance, stress, native/release qualification | Scheduled headless or explicitly requested/high-risk qualification |

Do not launch a full game to check arithmetic, quotes, statuses, research,
snapshot freshness or animation-time mapping. These already have direct C#
coverage. Do not move such tests into Godot merely to call them engine tests.
Use real component signals/resources when Godot is needed, not static config
strings or fixtures that reproduce themselves. A headless component cannot prove
rendered pixels, native focus/compositor behavior or physical sound/input; retain
that limitation rather than claiming equivalent graphical coverage.

During implementation use targeted A; once coherent use broader A and formatting.
Add B/C only for changed engine/transport/package boundaries. Deep verification
is not required before/after every task. Documentation-only edits need link and
planning consistency. Repeat a green check only for changed inputs, failures or
unresolved concerns. Report exact selected coverage and everything unexecuted.

PR/main CI uses A/B/bounded C plus independent native Linux/Windows headless
package jobs. Scheduled/manual `deep.yml` runs headless deep qualification only.
Graphical tests have no automatic workflow. Release build/identity/checksum/
allowlisted assembly/publishing protections remain independent; ordinary CI is
not installer, Steam-pair, rendering or release qualification.

For the measured audit, removals/replacements and limitations read
[verification](../../../docs/verification.md).
