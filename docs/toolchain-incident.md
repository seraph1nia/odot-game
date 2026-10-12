# Retained Test toolchain incident

## Scope and evidence

This is a lint-phase audit of the existing Test receipt, **not a new test run or
publication clearance**. Test names its head as
`01c05c8dcf6cee1d8ec8ba803c7820f86e2fd9c2`; the lint input is
`703f3815d5c0494fcdd9e10eb47266f5ae9b727c`.

The retained evidence root is
`/home/bart/.no-mistakes/evidence/01M4N6QHV3Y2Y254Q4GY8P7VC8/`:

- `testing-receipt.json` records the incident and claimed restoration/cleanup.
- `package-resource-setup-attempt.log` contains actual acquisition output:
  **35 tools installed**, an `op` plugin clone, Godot downloaded, and the .NET
  installer downloading/extracting SDK 10.0.401. Its explicit .NET destination is
  the worktree's `.test-runtime/package-data/mise/dotnet-root`. The log does not
  capture the invoking command or all environment/destination variables; it is
  not evidence that every tool/plugin/config write stayed there.
- `package-pointer-initial-attempt.log` records two failed pointer assertions.
- `package-resource-consumer.log` records the later passing headless consumer.
- `headless-settings-transcript.log`, `targeted-contracts-transcript.log` and
  `linux-package-transcript.log` retain the other selected passes.
- Read-only `no-mistakes axi logs --run 01M4N6QHV3Y2Y254Q4GY8P7VC8 --step test --full`
  exposes Test's narrative and result, but not the exact acquisition/restoration
  shell commands. The lint inspection copy is `logs/lint-test-phase-retained.log`.

No retained incident evidence was deleted or rewritten by this audit.

## What lint could establish

`mise.lock` currently matches the Test head and lint input byte-for-byte, with
SHA-256 `f7356d6f531c64c7042ed12d88a2f5ca9a6a34ded920e086e4ec45ed855c41f9`.
The dependency locks are also unchanged from Test head. The worktree's
`.test-runtime` and `dist` directories are absent. These establish current lock
restoration and owned-directory removal, not the exact restoration command or
absence of residual writes elsewhere.

Read-only `mise which dotnet`, `mise which node` and `mise which godot` returned
these existing absolute executables. Direct version queries match the locked
versions; these are **current identities**, not pre-incident identity snapshots:

| Executable | Direct version | Current executable SHA-256 |
| --- | --- | --- |
| `/home/bart/.local/share/mise/dotnet-root/dotnet` | 10.0.401 | `01d89e0a0191052bfea616cd4ce624c8faf13b05bbddf7f64499c23e2a9d9269` |
| `/home/bart/.local/share/mise/installs/godot-dotnet/4.7.2-stable/godot` | 4.7.2.stable.mono.official.ed1daf0bf | `fc1bd592b5e771c65d52968f99835b4f2b7fe7b7cbc141e202cb63e01c95ec4d` |
| `/home/bart/.local/share/mise/installs/node/24.21.0/bin/node` | v24.21.0 | `7fde7b8afa198da66257f42ee2001d874c7355631e6d1579a5fb5ef1f246df4c` |

These executable hashes are not comparisons with the lock's archive checksums.
`mise which godot-dotnet` does not resolve an executable; the executable name is
`godot`. No reinstall or filesystem hunt was attempted.

## Separate results from the acquisition-touched attempts

The retained Test narrative places the 89-assertion Settings pass, filtered
34-core/42-runner pass (two Windows-only skips), and Linux package/protocol smoke
before the extra package-consumer setup incident. Their transcripts report SDK
10.0.401 and Godot 4.7.2 .NET where applicable, but do not capture resolved
executable paths/hashes. They remain selected coverage, not a complete suite.

The download-touched setup attempt is not qualifying evidence: it logs missing
debug Steam-extension errors despite its resource probe reporting zero failures.
The initial pointer attempt also failed. The later consumer log reports a pass;
Test says it used the existing engine after restoring the lock and copying the
repository's debug Steam extension and `steam_api` into disposable client
staging. That pass is separate from the earlier release-package smoke and does
not certify the unmodified release staging. No retained command/environment
capture or final package/staged-library hashes independently binds that final
consumer to the claimed executable and inputs.

## Prevention and remaining publication blocker

The project already disabled task auto-install, but read-only
`mise settings get exec_auto_install` returned `true` before this lint fix.
`mise.toml` now also sets `exec_auto_install = false`; both settings resolve to
`false`. Process-only `MISE_EXEC_AUTO_INSTALL=false` and
`MISE_TASK_RUN_AUTO_INSTALL=false` were also checked with `mise settings get`.
No global/user setting was changed. These controls do not prohibit an explicit
install command; agents must still never run one.

For further authorized work, resolve installed tools **before** isolating XDG
state, then use those absolute executables directly. Put their directories before
mise shims on the process-local `PATH` so existing supervisor child commands
`dotnet`/`godot` also resolve installed tools. Keep the two no-auto-install
controls in that process environment; if an executable is absent, stop rather
than falling back to `mise exec`, installation, or acquisition. No new wrapper or
supervisor is needed. This lint turn used the installed absolute .NET/OpenSpec
executables for locked restore, changed-file formatting and static compilation;
it ran no behavioral checks.

**Publication remains blocked by unresolved incident provenance and missing fresh
qualification.** Current versions, restored locks and absent disposable directories
do not establish the destinations of every acquisition or the final consumer's
input identities. Historical before-state is unknown: current inspection cannot
prove complete global restoration, and no such claim is made. The latest recorded
approval requires bounded reconstruction and fresh pipeline-owned headless Test
qualification, not a waiver of those unknowns. This lint phase cannot invent
missing records, mutate global state, rerun Test, or clear publication.

## Bounded follow-up at `0177c9a`

The follow-up lint input is
`0177c9ab841e562c6d9e6355a09bc753990dafd1`. Inspection stayed within the named
run's evidence root, the worktree and previously resolved executable paths.
The supplied latest recorded decision defines the approval boundary; no separate
updated canonical-brief file or Test-native-session path was supplied or found
in the worktree/named evidence root. No host/session-directory search was made.

A fresh read-only `axi logs --run 01M4N6QHV3Y2Y254Q4GY8P7VC8 --step test --full`
returned byte-identical output to `logs/lint-test-phase-retained.log`; the new
copy is `logs/lint-test-phase-current.log`. The retained launch record identifies
Test's `pi` PID as `1453573`, but exposes only narrative/result, not native tool
calls. `axi logs --help` offers step-log retrieval, not a native-session export or
qualification dispatch. The available evidence root contains the receipts and
attempt logs listed above, not the missing launch environment or cleanup commands.

The setup log explicitly reports these 35 installed tool/version identities:

- zoxide 0.10.0, fzf 0.74.4, tmux 3.7c, starship 1.26.0, direnv 2.37.1,
  eza 0.23.5, ripgrep 15.2.0, fd 10.5.0, jq 1.8.2, bat 0.26.1;
- node 24.21.0, yq 4.53.6, herdr 0.9.1, docker-cli 29.8.2, gh 2.101.0,
  kubectl 1.37.1, kubectx 0.11.0, uv 0.12.19, helm 4.3.0, go 1.27.1;
- kubens 0.11.0, kubelogin 1.36.4, python 3.14.7, pnpm 11.28.0, op 2.40.0,
  just 1.58.0, npm:@fission-ai/openspec 1.13.2, godot-dotnet 4.7.2-stable;
- awscli 2.37.9, opencode 1.18.32, npm:cdk 2.1143.0, copilot 1.0.88,
  claude 2.1.283, npm:vite-plus 0.3.3, dotnet 10.0.401.

The plugin clone is separately identified as `vfox-1password.git#912327d`.
Only the disposable .NET destination is explicit in the log. The receipt attributes
the workaround to `mise exec` after changing `XDG_DATA_HOME`, but its exact argv,
complete environment and other destinations remain unobserved. There is no basis
to label any particular global directory as incident-mutated, or unchanged.
The concrete owned `.test-runtime` and `dist` paths remain absent; lockfiles remain
unchanged from Test head. Current executable versions and hashes still match the
table above. Those observations do not recover historical before-state.

The failed/setup and passing receipts were preserved. SHA-256 identities measured
in this follow-up, relative to the retained evidence root, are:

| Retained file | SHA-256 |
| --- | --- |
| `testing-receipt.json` | `417028e4d08888d0359bdaad8792d95c9847cd53ae28727c63ba3c87db673330` |
| `package-resource-setup-attempt.log` | `8da7b40977d57da4e8e9e3da738f3b52d43982b71a8e7de324588ac975d4a82c` |
| `package-pointer-initial-attempt.log` | `dab8313bd7519675cabe00ea81e60a4dc699c2bb57731108167a78f693fa633a` |
| `package-resource-consumer.log` | `78c947fe11ddfe8c1a2e023cb2cc9d535ad6c60d14f6a0544d80e9c11ecfa3c2` |

Current-head **static checks only** passed: installed absolute .NET 10.0.401
locked restore, formatting verification scoped to all 21 changed C# files and
solution compilation with zero warnings/errors; installed OpenSpec 1.13.2 with
installed Node 24.21.0 passed `validate --all --strict` (26 passed, zero failed).
Logs are `logs/lint-round3-{restore,format,build,openspec}.log`. Process-local
installed-tool directories preceded mise shims on `PATH`, and both no-auto-install
settings resolved to `false`. No behavioral test or graphical process ran.

Fresh source/package/staged-library qualification is **unexecuted**. The phase
boundary explicitly forbids this worker from running Test or invoking pipeline
control/continuation commands, so it cannot dispatch the supported pipeline-owned
Test route. The outer executor must arrange that continuation and supply the
owned native Test launch/session records if available. Qualification must capture
exact commands, resolved locked executable paths/hashes, source head, package and
staged-library paths/hashes, actual results and awaited owned cleanup, retaining
old attempts separately. If the outer executor has no supported continuation or
cannot recover incident facts, it must report that limitation without approving,
skipping, restarting or claiming complete restoration. This static pass is not
current-head behavioral qualification or publication clearance.
