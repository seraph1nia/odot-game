# Worldtree delivery verification plan — headless only

This is separate verification evidence, not an expansion of captain intent.
Validate the current submitted/pipeline/published head and report exact selected
coverage and unexecuted checks. Keep no-mistakes as the sole owner of fixes,
review, tests, documentation, lint, push, PR and CI once its run is active.
Use the requested Pi profile `openai/gpt-6.1-sol`, effort `high`.

## Prerequisites and ownership

- Use declared SDK 10.0.401, Godot .NET 4.7.2 and matching already installed
  checksum-verified templates. `MISE_AUTO_INSTALL=0`; no installation, download,
  upgrade, global changes, alternate mise root or sibling worktree mutation.
- Locked restore of `Odot.slnx`; formatting with `dotnet format Odot.slnx
  --no-restore`. Missing prerequisites are unexecuted/nonzero, not passing.
- Restore/build/import/export mutations remain sequential. Isolated completed
  C# binaries may run concurrently using existing admission. Existing Child,
  ScenarioScope and EngineTests own logs, marker/private runtime state and cleanup.
- Never set `ODOT_ALLOW_GRAPHICAL_TESTS=1`. Do not run `test-ui`, screenshots,
  desktop capture, X11, full-game graphical journeys, Steam pairing, release or
  installer checks. No standalone asset-showcase graphical permission transfers.

## Cheapest meaningful checks

1. Tier A during fixes: `mise run test-core --filter
   'FullyQualifiedName~Research'`; this is partial filtered coverage. Verify new
   effects, catalog/state validation, acquired-chain/exclusive guards,
   unaffordability, malformed IDs, duplicate packet/purchase, stale phase/turn,
   overflow/atomic income, real projection, JSON/Brotli restoration and all
   authority policies. No Godot process for arithmetic.
2. Once coherent: `mise run test-fast` (all non-Deep core/runner coverage), and
   `mise run check`. Preserve any platform-specific skips as explicitly skipped.
   AuthoredAssetTests verifies original imported member bytes against the existing
   authored manifest's source pin; engine loading is independently required.
3. Tier B: `mise run test-engine --scenario worldtree`. Minimal SceneTree only,
   not Main/GameApplication/menu/renderer/peers. This loads actual canonical JSON,
   all 30 imported icons and the actual game adapter. Check complete snapshot
   rejection/atomicity, intent-only before receipt, actual authority callback,
   unchanged tick/feedback preserving a child tween, independent sap-before-leaves
   awakening and second-instance state, selection, real native Tab/Enter and
   host-boundary escape/re-entry, modal confinement, socket/purchase/Close clicks
   and scrolling through the shared application modal-selection/input boundary,
   embedded-window outside-event consumption, world-input blocking/resumption,
   Escape, resize, disconnect and free-during-awakening. The component case does not qualify
   rendered window stacking or pixels.
4. Existing headless source gate: `mise run ci-source --startup-timeout-ms 60000`
   selects the [current headless CI coverage](verification.md#ci-coverage),
   with no deep/UI journey. Targeted
   `test-network --scenario playing-host-lifecycle` is useful for entry-point
   changes; do not blindly repeat passed transport for documentation-only inputs.
5. Package boundary: after verifying installed template marker/files, sequential
   `mise run export-client`, then `mise run test-engine --scenario worldtree
   --exported`. Do not prepare/re-export from the exported-only probe. The locked
   release template forbids scene overrides; its early deferred component-probe
   dispatcher selects EngineTests before application/session/renderer/peer setup.
   Prove both JSON files, dynamic images/Theme/font, C# adapter and child lifetime
   load from the actual newly generated PCK. Record executable/PCK/managed hashes.
   Existing `ci-linux-package` and native `ci-windows` CI owners also run this
   component probe. Linux does not claim native Windows execution.

Ordinary required hosted CI remains headless. Do not replace required checks or
waive a failure. No dedicated wave-eight, twenty-wave calibration, graphical
research, GPU/performance or installed-release acceptance is required here.

## Evidence and delivery

`docs/worldtree.md` documents user entry/action, all stable mappings, three new
functional production nodes and costs/prerequisites, preserved combat effects,
three established-system sockets, three inactive slots, authority/persistence
scope, source pin/import contract and qualification limits.
`docs/worldtree-validation.json` retains measured implementation input hashes,
commands/results, genuine failed attempts and package identities; do not relabel
those dirty-source executions as a later published head. Pipeline validation must
name its actual current head, commands, exact results/coverage/skips and any
unexecuted boundary. Keep asset original provenance separate from game execution.

After no-mistakes returns CI-ready, use gh-axi to verify the full PR URL, exact
published head, non-draft status and green required CI. The same worker reports
these to firstmate; it never merges, bypasses protection, publishes a release or
edits/commits source while the pipeline owns it. Escalate ask-user findings as a
single verbatim snapshot and gate event; do not self-answer or use --yes, skips,
waivers, competing runs, abort/restart, force or discarded history.

Visual/game-GPU/performance, physical input, listening, tiny-window/RTL/HiDPI,
Steam invitation pairing, new durable save/server restart and deep balance
acceptance remain explicitly unexecuted. Reconnect means the existing retained
running authority, not a new disk-save subsystem.
