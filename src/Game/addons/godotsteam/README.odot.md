# Pinned GodotSteam GDExtension

Official [v4.22.1-gde release](https://codeberg.org/godotsteam/godotsteam/releases/tag/v4.22.1-gde),
Steamworks SDK 1.65, inspected 2026-09-30. The canonical
[repository](https://codeberg.org/godotsteam/godotsteam) is active and not archived.
The release API reports non-prerelease, while the
[publisher](https://store.godotengine.org/asset/godotsteam/godotsteam-gdextension/)
marks this version **unstable**. The owner accepted this pin for development;
release stability and remote multiplayer acceptance remain open gates.

`manifest.json` records the archive URL/hash, metadata and hashes of unmodified
files. Only the Linux x86_64 debug/release libraries, Valve runtime and upstream
license/readme are retained. The descriptor removes unsupported platform entries;
its entry point, Linux paths and dependencies match the upstream descriptor.
No editor plugin/dock is enabled or needed. Ordinary stock Godot .NET templates
bundle the native dependency; no custom engine, generated C# bindings, beta
adapter, runtime download or fallback transport is used.

To refresh intentionally, download the exact archive URL in the manifest,
verify its SHA-256 before extraction, and compare each retained file hash. Never
replace the pin from a moving branch. Assess compatibility, stability, licenses
and source/export checks again for a version update. Source is available at
the matching release tag, including `register_types.cpp`,
`godotsteam_multiplayer_peer.cpp` and `steam_packet_peer.cpp`.

`license.md` is the upstream MIT license for GodotSteam. `libsteam_api.so` is
Valve's Steamworks SDK redistributable and is **not** relicensed as MIT. Its use
and distribution are subject to the
[Steamworks SDK agreement](https://partner.steamgames.com/doc/sdk) and the
developer's Steamworks terms. Exports retain these notices alongside the native
libraries. Steam API initialization is opt-in; local roles require no login.
