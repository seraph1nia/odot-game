using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using Game.Distribution;

namespace DevRunner;

internal sealed partial class Runner
{
    private const string ReleaseRepository = "seraph1nia/odot-game";
    internal const string InnoSetupVersion = "6.7.3";
    internal const string InnoSetupChecksum = "9c73c3bae7ed48d44112a0f48e66742c00090bdb5bef71d9d3c056c66e97b732";

    private async Task PackageRelease()
    {
        ReleaseVersion version = ReleaseVersion.FromTag(options.ReleaseTag);
        await TaggedExport();
        ReleaseIdentity identity = await ReadReleaseIdentity(version);
        if (OperatingSystem.IsLinux()) await PackageLinux(version, identity);
        else if (OperatingSystem.IsWindows()) await PackageWindows(version, identity);
        else throw new VerificationPrerequisiteException("Release packaging requires Linux or Windows x86_64.");
    }

    private async Task<ReleaseIdentity> ReadReleaseIdentity(ReleaseVersion version)
    {
        string path = Path.Combine(_root, "dist", "releases", version.Value, options.ExportTarget, "build-info.json");
        if (!File.Exists(path)) throw new InvalidOperationException("Tagged export build-info.json is missing.");
        return ReleaseIdentity.FromJson(await File.ReadAllTextAsync(path, cancellation));
    }

    [System.Runtime.Versioning.SupportedOSPlatform("linux")]
    private async Task PackageLinux(ReleaseVersion version, ReleaseIdentity identity)
    {
        if (identity.Target != "linux-x64") throw new InvalidOperationException("Linux packaging requires a linux-x64 identity.");
        string release = Path.Combine(_root, "dist", "releases", version.Value);
        string payload = Path.Combine(release, "linux-x64");
        string asset = $"odot-{version.Value}-linux-x64.tar.gz";
        string archive = Path.Combine(release, asset);
        string rootName = $"odot-{version.Value}";
        await using (var output = File.Create(archive))
        await using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize))
        using (var writer = new TarWriter(gzip, leaveOpen: false))
        {
            foreach (string file in Directory.EnumerateFiles(payload, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
            {
                string relative = Path.GetRelativePath(payload, file).Replace(Path.DirectorySeparatorChar, '/');
                using var input = File.OpenRead(file);
                var entry = new PaxTarEntry(TarEntryType.RegularFile, rootName + "/" + relative) { DataStream = input };
                UnixFileMode mode = relative == "odot.x86_64" ? UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute
                    : UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead | UnixFileMode.OtherRead;
                entry.Mode = mode; writer.WriteEntry(entry);
            }
        }
        string hash = await Hash(archive);
        string scriptName = $"odot-{version.Value}-linux-x64-install.sh";
        string script = LinuxInstaller(version.Value, asset, hash);
        await File.WriteAllTextAsync(Path.Combine(release, scriptName), script, cancellation);
        File.SetUnixFileMode(Path.Combine(release, scriptName), UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
        await WritePlatformManifest(release, identity, [asset, scriptName]);
        Console.WriteLine($"Linux release assets ready: {asset}, {scriptName}. Nothing uploaded or published.");
    }

    private async Task PackageWindows(ReleaseVersion version, ReleaseIdentity identity)
    {
        if (identity.Target != "windows-x64" || !OperatingSystem.IsWindows())
            throw new VerificationPrerequisiteException("Windows installer packaging requires native Windows x86_64.");
        string? compiler = Environment.GetEnvironmentVariable("ISCC_PATH");
        if (string.IsNullOrWhiteSpace(compiler) || !File.Exists(compiler))
            throw new VerificationPrerequisiteException($"Inno Setup {InnoSetupVersion} compiler missing. Set ISCC_PATH to its ISCC.exe; ordinary tasks never install it.");
        string release = Path.Combine(_root, "dist", "releases", version.Value);
        string payload = Path.Combine(release, "windows-x64");
        string name = $"odot-{version.Value}-windows-x64-setup";
        // ISCC /? exits nonzero and its executable version resource is 0.0.0.
        // A nonquiet successful compile reports the actual loaded compiler engine.
        await _evidence.Measure("inno-installer", "phase", async () =>
        {
            await using var child = new Child("inno-installer", compiler,
                ["/DSourceDir=" + payload, "/DOutputDir=" + release, "/DOutputName=" + name,
                    "/DAppVersion=" + version.Value, Path.Combine(_root, "tools", "Distribution", "Odot.iss")],
                _root, evidenceDirectory: _evidence.Directory);
            int code = await child.WaitExit(cancellation);
            if (code != 0) throw new InvalidOperationException($"Inno Setup compilation failed with exit code {code}.");
            // Close the writer before reopening its transcript (Windows sharing rules).
            await child.DisposeAsync();
            ValidateInnoCompilerVersion(await File.ReadAllTextAsync(child.LogPath, cancellation));
        });
        string installer = name + ".exe";
        if (!File.Exists(Path.Combine(release, installer))) throw new InvalidOperationException("Inno Setup did not create the expected installer.");
        await WritePlatformManifest(release, identity, [installer]);
        Console.WriteLine($"Windows release installer ready: {installer}. Nothing uploaded or published.");
    }

    internal static void ValidateInnoCompilerVersion(string output)
    {
        if (!output.Split('\n').Any(line => line.TrimEnd('\r') == "Compiler engine version: Inno Setup " + InnoSetupVersion))
            throw new VerificationPrerequisiteException($"ISCC_PATH must identify Inno Setup {InnoSetupVersion}; expected compiler engine banner is missing.");
    }

    private static async Task<string> Hash(string path)
    {
        await using var stream = File.OpenRead(path);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream)).ToLowerInvariant();
    }

    private async Task WritePlatformManifest(string release, ReleaseIdentity identity, string[] assets)
    {
        var lines = new List<string>();
        foreach (string asset in assets) lines.Add(await Hash(Path.Combine(release, asset)) + "  " + asset);
        await File.WriteAllLinesAsync(Path.Combine(release, $"SHA256SUMS.{identity.Target}"), lines, cancellation);
        await File.WriteAllTextAsync(Path.Combine(release, $"build-info.{identity.Target}.json"), identity.ToJson() + "\n", cancellation);
    }

    internal static string LinuxInstaller(string version, string archive, string hash) => $$"""
        #!/bin/sh
        set -eu
        VERSION='{{version}}'
        ARCHIVE='{{archive}}'
        SHA256='{{hash}}'
        URL='https://github.com/{{ReleaseRepository}}/releases/download/v{{version}}/{{archive}}'
        if [ "${ODOT_INSTALL_TESTING:-}" = 1 ] && [ -n "${ODOT_ARCHIVE_URL:-}" ]; then URL=$ODOT_ARCHIVE_URL; fi
        [ "$(uname -m)" = x86_64 ] || { echo 'The Common Watch requires Linux x86_64.' >&2; exit 1; }
        [ -n "${HOME:-}" ] || { echo 'HOME is required.' >&2; exit 1; }
        for tool in curl tar sha256sum mktemp; do command -v "$tool" >/dev/null 2>&1 || { echo "Missing prerequisite: $tool" >&2; exit 1; }; done
        DATA_BASE=${XDG_DATA_HOME:-"$HOME/.local/share"}
        INSTALL_ROOT=${ODOT_INSTALL_ROOT:-"$DATA_BASE/odot"}
        BIN_DIR=${ODOT_BIN_DIR:-"$HOME/.local/bin"}
        APP_DIR=${ODOT_APPLICATIONS_DIR:-"$DATA_BASE/applications"}
        LOCK="$INSTALL_ROOT/.install-lock"
        mkdir -p "$INSTALL_ROOT" "$BIN_DIR" "$APP_DIR"
        if ! mkdir "$LOCK" 2>/dev/null; then echo 'An installation of The Common Watch is already running.' >&2; exit 1; fi
        WORK=$(mktemp -d "${TMPDIR:-/tmp}/odot-install.XXXXXX")
        cleanup() { rm -rf "$WORK"; rmdir "$LOCK" 2>/dev/null || true; }
        trap cleanup EXIT HUP INT TERM
        if [ "${ODOT_INSTALL_TESTING:-}" = 1 ]; then
          curl --fail --location --proto '=https,file' --connect-timeout 15 --max-time 300 --output "$WORK/$ARCHIVE" "$URL"
        else
          curl --fail --location --proto '=https' --tlsv1.2 --connect-timeout 15 --max-time 300 --output "$WORK/$ARCHIVE" "$URL"
        fi
        printf '%s  %s\n' "$SHA256" "$WORK/$ARCHIVE" | sha256sum -c -
        tar -tzf "$WORK/$ARCHIVE" | awk 'BEGIN{ok=1} /(^\/|(^|\/)\.\.($|\/))/{ok=0} END{exit !ok}' || { echo 'Unsafe archive paths.' >&2; exit 1; }
        tar -xzf "$WORK/$ARCHIVE" -C "$WORK"
        SOURCE="$WORK/odot-$VERSION"
        [ -x "$SOURCE/odot.x86_64" ] && [ -f "$SOURCE/odot.pck" ] && [ -f "$SOURCE/build-info.json" ] || { echo 'Incomplete The Common Watch archive.' >&2; exit 1; }
        TARGET="$INSTALL_ROOT/versions/$VERSION"
        STAGE="$INSTALL_ROOT/versions/.stage-$VERSION-$$"
        mkdir -p "$INSTALL_ROOT/versions"
        rm -rf "$STAGE"
        cp -R "$SOURCE" "$STAGE"
        chmod 755 "$STAGE/odot.x86_64"
        if [ -e "$TARGET" ]; then cmp "$TARGET/build-info.json" "$STAGE/build-info.json" >/dev/null || { echo 'Existing version has different identity.' >&2; exit 1; }; rm -rf "$STAGE"; else mv "$STAGE" "$TARGET"; fi
        cat > "$INSTALL_ROOT/launcher" <<'LAUNCHER'
        #!/bin/sh
        set -eu
        ROOT=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
        GAME="$ROOT/current/odot.x86_64"
        preload_overlay() {
          overlay=$1
          if [ -r "$overlay" ] && [ "$(dd if="$overlay" bs=1 count=4 2>/dev/null | od -An -tx1 | tr -d ' \n')" = 7f454c46 ]; then
            case ":${LD_PRELOAD:-}:" in *":$overlay:"*) ;; *) LD_PRELOAD=${LD_PRELOAD:+"$LD_PRELOAD:"}$overlay; export LD_PRELOAD;; esac
            return 0
          fi
          return 1
        }
        if [ "${ODOT_STEAM_DISABLED:-}" != 1 ]; then
          loaded=0
          if [ "${ODOT_INSTALL_TESTING:-}" = 1 ] && [ -n "${ODOT_STEAM_OVERLAY:-}" ]; then preload_overlay "$ODOT_STEAM_OVERLAY" && loaded=1 || true; fi
          if [ "$loaded" = 0 ]; then
            for base in "$HOME/.steam/root" "$HOME/.steam/steam" "$HOME/.steam/debian-installation" "$HOME/.local/share/Steam"; do
              preload_overlay "$base/ubuntu12_64/gameoverlayrenderer.so" && break || true
            done
          fi
        fi
        exec "$GAME" "$@"
        LAUNCHER
        chmod 755 "$INSTALL_ROOT/launcher"
        ln -sfn "$TARGET" "$INSTALL_ROOT/current.new"
        mv -Tf "$INSTALL_ROOT/current.new" "$INSTALL_ROOT/current"
        ln -sfn "$INSTALL_ROOT/launcher" "$BIN_DIR/odot"
        cat > "$INSTALL_ROOT/uninstall" <<UNINSTALL
        #!/bin/sh
        set -eu
        rm -f "$BIN_DIR/odot" "$BIN_DIR/odot-uninstall" "$APP_DIR/odot.desktop"
        rm -rf "$INSTALL_ROOT/versions"
        rm -f "$INSTALL_ROOT/current" "$INSTALL_ROOT/launcher" "$INSTALL_ROOT/uninstall"
        rmdir "$INSTALL_ROOT" 2>/dev/null || true
        UNINSTALL
        chmod 755 "$INSTALL_ROOT/uninstall"
        ln -sfn "$INSTALL_ROOT/uninstall" "$BIN_DIR/odot-uninstall"
        cat > "$APP_DIR/odot.desktop" <<DESKTOP
        [Desktop Entry]
        Type=Application
        Name=The Common Watch
        Exec="$INSTALL_ROOT/launcher"
        Terminal=false
        Categories=Game;
        DESKTOP
        chmod 644 "$APP_DIR/odot.desktop"
        echo "Installed The Common Watch $VERSION. Run: $BIN_DIR/odot"
        """;
}
