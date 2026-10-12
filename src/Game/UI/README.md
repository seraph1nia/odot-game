# Woodland native UI payload

Production resources from `seraph1nia/odot-game-assets`.
[manifest.json](manifest.json) owns the exact source revision and root, production
member inventory and SHA-256 bindings to the original design, catalog/package,
API and provenance owners. The current woodland presentation supersedes the
earlier flat Ledger skin; `theme/ledger.tres` retains its canonical portable
resource name.

Only production native Godot resources are included: no preview project,
showcase, fixtures, reference raster, build tools, Blender or generated caches.
The portable package identity is recorded in the manifest's `package` record;
the selected 22 receiving-game members are recorded in `files`, not all 46
original package members. The [original inventory](../../../docs/asset-history/woodland-ui/README.md)
preserves all source hashes and package provenance separately. Eight unused
portable component triples are omitted; live gameplay keeps its C# receivers
and separately pinned Worldtree. The archive itself was absent
in the read-only source checkout: these game resources were verified/copied
directly from the pinned source members, **not claimed loaded from that ZIP**. Receiving
Godot regenerates native SVG/font imports locally. The additional landed
Worldtree members retain their own exact inventory/revision in
[the authored manifest](../Assets/Authored/manifest.json) and
[immutable import contract](../../../docs/worldtree.md#immutable-import-contract),
not a relabeled expansion of this original kit pin.

Original authored art uses the repository owner's existing permission: **the
repo is my own work, so yes, use it**. Origin is not a new open-source license
grant. [woodland-art.json](woodland-art.json) and
[seal-provenance.json](seal-provenance.json) retain original editable source/output
bindings. The seal's editable source remains upstream, without a Blender rebuild.
The editable SVGs are byte-identical to their pinned source originals. The
unmodified DejaVu Serif font retains its complete
[redistribution license](fonts/LICENSE-DejaVu.txt).
The supplied `hud.png` remains reference context upstream only: its original hash
is recorded, and it is **not shipped as runtime artwork**.

See [the live game migration](../../../docs/ui-migration.md) for actual consumers,
authority boundaries, removed-resource inventory and current evidence. Engine
widgets/system fallbacks remain engine-owned, not off-the-shelf pack assets.
