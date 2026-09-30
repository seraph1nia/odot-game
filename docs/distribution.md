# Versioned desktop distribution

Odot releases provide ready-to-run Windows x86_64 and Linux x86_64 clients.
Friends do not need Godot, the .NET SDK, mise, or a dedicated server. Packaged
multiplayer uses Steam; when Steam is unavailable, the start screen and solo
game remain usable and explain that multiplayer is unavailable.

## Installing on Windows

Download `odot-V-windows-x64-setup.exe` from the selected GitHub release and
run it. The unsigned installer may trigger Microsoft Defender SmartScreen;
inspect the release checksum and choose the operating-system option to continue
only if the checksum matches. It installs for the current user under
`%LOCALAPPDATA%\Programs\Odot`, creates an **Odot** Start Menu entry, and does
not request administrator access. Godot and a system-wide .NET runtime are not
required.

Run a newer installer to upgrade. It asks Odot to close before replacing the
complete managed payload and removes files left by an older payload. Player
settings and private session data live outside the installation and remain
available. Remove the application through Windows **Installed apps** or its
registered Odot uninstaller; player data is preserved by default.

Windows needs a supported 64-bit Windows desktop and graphics hardware/drivers
meeting [Godot's compatibility renderer requirements](https://docs.godotengine.org/en/stable/about/system_requirements.html).
Steam multiplayer needs the native 64-bit Steam client and an eligible signed-in
account. The installer is currently unsigned because no code-signing certificate
is configured.

## Installing on Linux

Download the version-specific install script from a release, inspect it, and
run it. For release `v0.1.0-beta.1`:

```sh
curl -fLO https://github.com/seraph1nia/odot-game/releases/download/v0.1.0-beta.1/odot-0.1.0-beta.1-linux-x64-install.sh
sh odot-0.1.0-beta.1-linux-x64-install.sh
```

The script checks Linux x86_64 plus `curl`, `tar`, `sha256sum`, and `mktemp`.
It installs no OS packages and uses no `sudo`. It downloads the matching
`odot-V-linux-x64.tar.gz` from the fixed release URL, verifies the checksum
baked into the script, rejects unsafe archive paths, stages the complete
version, and then atomically switches the launcher. Re-running the same script
is safe. Running a newer version performs a full upgrade while leaving the
previous version active if download, checksum, or staging fails.

The default locations are:

- application versions: `${XDG_DATA_HOME:-$HOME/.local/share}/odot`
- commands: `$HOME/.local/bin/odot` and `$HOME/.local/bin/odot-uninstall`
- application menu entry: `${XDG_DATA_HOME:-$HOME/.local/share}/applications/odot.desktop`

`ODOT_INSTALL_ROOT`, `ODOT_BIN_DIR`, and `ODOT_APPLICATIONS_DIR` override these
locations. Paths containing spaces are supported. Run `odot-uninstall` to
remove owned versions, launchers, and the application entry. Player preferences
and private session data remain outside those paths and are preserved.

The launcher forwards all arguments and retains an inherited `LD_PRELOAD`. If
available, it adds a valid native Steam overlay from the standard Steam
locations before graphics starts. `ODOT_STEAM_DISABLED=1 odot` disables this
integration. Missing Steam or overlay files never prevent solo play.

The Linux build targets recent x86_64 glibc distributions and currently needs
glibc 2.28 or newer plus ordinary system graphics/windowing libraries and
OpenGL 3.3-capable drivers. It runs on current Arch-family distributions and
Ubuntu 24.04. It is not a universal package for musl-only systems, old glibc
releases, ARM machines, or every Linux graphics stack. Native Steam multiplayer
expects the distribution's normal 64-bit Steam client.

## Manual update checks

Settings contains an **About** tab with the installed version and **Check for
updates**. No background polling occurs. The check reads public GitHub release
metadata with a bounded request and does not need credentials. A stable build
only considers newer stable releases. A preview build considers newer previews
and stable successors. Drafts, malformed versions, incomplete platform assets,
foreign download URLs, equal versions, and downgrades are never offered.

When an update is available, **Download update** opens the exact Windows
installer or the version-specific Linux release page in the default browser.
Odot does not download executable code itself, exit the game, or install during
a match. Close Odot after downloading, then run the new installer or script.
Offline, timed-out, rate-limited, malformed, and browser-open failures appear as
recoverable status in the same dialog.

## Version and Steam policy

Release tags use `vMAJOR.MINOR.PATCH[-prerelease][+metadata]`, for example
`v0.1.0-beta.1` or `v1.0.0`. Numeric components are limited to 0..65534 so
assembly and Windows metadata can represent them. Leading zeroes in numeric
identifiers are invalid. Build metadata remains visible but does not affect
update ordering. Version, commit, channel, target, engine, SDK/runtime and Steam
mode are stamped into the package; the multiplayer protocol remains independent.
Ordinary source and untagged exports identify themselves as development builds.

Prerelease tags default to preview/test packages using Steam AppID 480. Both
friends must start Odot themselves before accepting invitations. AppID 480 does
not establish genuine cold launch, remote relay, invitations, or production
qualification. Stable tags require repository variable `ODOT_STEAM_APP_ID` to
contain Odot's own positive non-480 AppID. Real two-account Steam acceptance
remains a separate check.

## Preparing and publishing a release

The repository owner first makes `seraph1nia/odot-game` public and, for stable
releases, configures the `ODOT_STEAM_APP_ID` GitHub Actions repository variable.
Create a SemVer tag on the exact commit, then create and manually publish a
GitHub release for that tag with no attached assets. Mark a prerelease when the
tag has a prerelease component; leave stable releases unmarked. Draft creation
does not start distribution.

Publishing triggers **Build published release**. It checks repository visibility,
the tag/source identity, release channel and absence of existing assets, then
builds Linux and Windows packages in parallel on native GitHub-hosted runners.
It does not rerun gameplay, UI, network, or installation tests; those belong to
ordinary CI and the checked-in selected verification tasks. After both builds
and their identity/checksum checks pass, it attaches:

```text
odot-V-windows-x64-setup.exe
odot-V-linux-x64.tar.gz
odot-V-linux-x64-install.sh
SHA256SUMS
build-info.json
```

The release is visible briefly without binaries while its checks run. A retry
never replaces existing release assets. Verification logs, screenshots, player
data, and intermediate manifests remain in the ephemeral jobs and are not
published. Ordinary push/pull-request CI remains read-only and uploads nothing.

Local package preparation and optional installation verification require a clean
checkout of an existing exact tag:

```sh
mise run release-preflight --tag v0.1.0-beta.1 --target linux-x64
mise run package-linux --tag v0.1.0-beta.1
mise run verify-installed-linux --tag v0.1.0-beta.1
```

Windows uses the equivalent `package-windows` and `verify-installed-windows`
tasks on native Windows. Set `ISCC_PATH` to the compiler from Inno Setup 6.7.3.
The required installer URL and SHA-256 are pinned in
`tools/Distribution/inno-setup.json`; local tasks report a missing or different
compiler and never install or upgrade it. The release workflow alone downloads
that exact compiler into its ephemeral runner and verifies the checksum.
