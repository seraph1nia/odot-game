# Woodland native UI payload

Production resources from `seraph1nia/odot-game-assets` at exact revision
`12bc5ef02765c1d4b725013791f7420918aacd64`, copied from `ui/preview/UI/`.
[manifest.json](manifest.json) binds each of the 46 runtime/source/import/UID
members and the original design, catalog/package, API and provenance owners by
SHA-256. The current woodland presentation supersedes the earlier flat Ledger
skin; `theme/ledger.tres` retains its canonical portable resource name.

Only production native Godot resources are included: no preview project,
showcase, fixtures, reference raster, build tools, Blender or generated caches.
The portable package's current 554,750-byte archive is recorded by its upstream
SHA-256 and exact member inventory. The archive itself was absent in the
read-only source checkout: these game resources were verified/copied directly
from the pinned source members, **not claimed loaded from that ZIP**. Receiving
Godot regenerates native SVG/font imports locally.

Original authored art uses the repository owner's existing permission: **the
repo is my own work, so yes, use it**. Origin is not a new open-source license
grant. [woodland-art.json](woodland-art.json) and
[seal-provenance.json](seal-provenance.json) retain original editable source/output
bindings. The seal remains the original 192px PNG at fixed 48px display; its
editable source remains upstream, without a Blender rebuild. The four editable
SVGs are byte-identical to their pinned source originals. The unmodified DejaVu
Serif font retains its complete [redistribution license](fonts/LICENSE-DejaVu.txt).
The supplied `hud.png` remains reference context upstream only: its original hash
is recorded, and it is **not shipped as runtime artwork**.

See [the live game migration](../../../docs/ui-migration.md) for actual consumers,
authority boundaries, removed-resource inventory and current evidence. Engine
widgets/system fallbacks remain engine-owned, not off-the-shelf pack assets.
