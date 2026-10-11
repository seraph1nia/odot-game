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

**Publication remains blocked by unresolved incident provenance.** Current
versions, restored locks and absent disposable directories do not prove that the
35-tool installation left global/user tool/plugin/config state unchanged, or
prove the final consumer's input identities. The retained evidence lacks the
exact incident/final commands, full destination environment and pre/post state
comparison. The outer executor needs retained acquisition/restoration/cleanup
command evidence and attributable final executable/package/staged-library
identities, or an explicit resolution of those unknowns; this lint phase cannot
invent that evidence, mutate global state, rerun Test, or clear publication.
