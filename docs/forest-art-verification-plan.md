# Forest art: headless-only verification plan

This file is the separate no-mistakes verification-plan input. The latest captain
instruction (headless tests only) supersedes the original scenic/graphical requests.
Do not start any GUI game, private X11/Xvfb display, full-game UI/scenic journey,
Forward+ native capture, graphical package smoke or installed graphical test.
Do not set ODOT_ALLOW_GRAPHICAL_TESTS=1, including as a pipeline or CI workaround.
Do not certify appearance from older renderer receipts or the asset showcase.
No release, merge, skip, waiver or prerequisite download/install is authorized.

Use declared Godot .NET 4.7.2 and locked SDK/NuGet/template inputs. Matching local
export templates already exist at ~/.local/share/godot/export_templates/4.7.2.stable.mono;
check availability rather than acquiring missing ones. Run shared preparation and
exports serially; only completed isolated binaries/owned headless transport peers
may overlap. Existing Child/ScenarioScope cleanup/error/readiness/admission owners
and deadlines remain unchanged. Never touch developer preferences or unrelated
processes/endpoints. Missing prerequisites are nonzero/unexecuted.

1. Check current changed inputs after pipeline rebase. Format changed C# with
   `dotnet format Odot.slnx --no-restore` after locked restore. Run
   `git diff --check`. No tool/runtime/driver upgrade.
2. Tier A: `mise run test-fast` (Deep excluded). During fixes use the existing
   `test-runner --filter 'FullyQualifiedName~GraphicsTests'` and relevant asset/
   provenance tests, then broader fast once coherent. Zero-test filters fail;
   native-Windows skips on Linux are explicitly unexecuted, not Windows proof.
3. Tier B: `mise run test-engine --scenario settings` and `--scenario assets`
   for this changed settings/resource/lifecycle boundary. These instantiate only
   production components in the minimal headless scene, not Main/menu/solo/peers.
   Preserve checks for native Settings signal draft/preview/Apply/Cancel/Reset/
   persistence/invalid types and independent weather/audio; 16-cube RGB LUT upload
   images/metadata/binding; shadowless finite lights; actual device-based decal
   fallback; imported receiver/support/provenance/material/mip contracts; bounded
   GPU configurations, Off/comfort/quality, transitions and disposal. Dummy
   cannot verify pixels or 3D GPU readback. The exported asset proof uses the same
   production resource assertions, not a source-text proxy.
4. Required full **headless** current workflow coverage: `mise run ci-source
   --startup-timeout-ms 60000`, `mise run ci-linux-package
   --startup-timeout-ms 60000` (serial after source). Existing bounded essential
   headless transport cases, animation/resources and sequential package exports/
   payload probes are permitted. `mise run ci` is also the current sequential
   headless source + Linux package wrapper. Never invoke old graphical CI or
   `test-ui`, `test-vulkan`, `test-atmosphere`, exported scenic play/X11, including
   a hidden fallback. Deeper paid campaigns are not this pass's default tax.
5. Because runtime/import resources changed, run the existing fresh Linux client
   package `mise run test-assets --exported` after the package phase, only if that
   phase did not already exercise this same asset proof. This requires current
   package bindings, uses one bounded owned headless component child and never
   starts a menu/match. No redundant export/preparation in parallel. Existing
   asset-fidelity proof must retain source/manifest/GLB/map/texture provenance,
   numerical geometry/material channel/ZeroEmission/mipmap negative checks.
6. Forge CI: all current required source/Linux package/Windows headless workflow
   checks must pass on the exact published head before reporting ready. Pipeline
   alone owns rebase/fixes/lint/push/PR/CI; no manual push/merge or gate edits.
   Windows job is real Windows headless execution, not inferred Linux coverage.

Retain exact command/evidence/timing/counts, genuine failures, cancelled/unexecuted
checks and source/export/published-head identity in ordinary docs/PR evidence.
Explicitly leave native Forward+/aesthetic/performance/compositor/scenic acceptance
unexecuted. Resource/property/lifecycle proofs establish bounded behavior, not
cohesive rendered appearance or hardware throughput. All protected numerical,
authority/network/save/terrain/navigation/camera/art-source contracts stay intact.
