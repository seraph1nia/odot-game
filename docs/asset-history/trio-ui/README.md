# Trio UI assets

Official free Trio UI Kit v2.2 by Moonpunch Studio, downloaded from https://moonpunchstudio.itch.io/trio-ui-kit-free on 2026-10-01. The archive contains the complete Cozy theme (30 SVG widgets) and 40 icons. Odot bundles only the selected SVG subset; Godot imports these at their original raster dimensions. No runtime downloads or paid assets are used.

## Permission and provenance

`UPSTREAM-README.txt` preserves the supplied terms, including unrestricted personal/commercial use and the restriction on redistribution of the files themselves. The project owner explicitly confirmed permission to include the assets in this repository on 2026-10-01 and stated that they will keep them in a private repository. This confirmation resolves the source-bundling question for this change; it does not replace the upstream terms for other recipients.

`manifest.json` records the archive SHA-256 and original paths/file hashes. Derived buttons remove the baked PLAY text so each action can supply its own label. Derived health textures separate the kit track and fill, extending the fill to full width; authoritative progress controls its visible extent. Dropdown/close variants preserve vector geometry at native control icon sizes; the slider knob is extracted from the supplied slider. Originals remain intact. The panel border is 20px; button borders preserve corners and the bottom shadow. Control content padding is independent of these texture margins.

## Semantics and fallback

Cozy uses parchment panels, amber buttons, brown text/borders, red health fill and teal focus. Settings uses gear; play/start/resume use play; return/back use left arrow; home/host/cities use home; inspection uses map; health uses heart; audio uses sound; gold uses gold pile; food uses bread; army uses helmet; melee/recruitment use sword, Berserker axe, ranged bow, magic staff; research/upgrade uses buff and defence uses shield. Labels and exact values remain visible. Wood, pause, reconnect, friends/invite, exit and updates retain text where no unambiguous free icon exists. No new resource or action is implied by unused kit art. Resource costs use the same gold/food icons as totals and explicit wood text.

Focus borders, slider/scroll primitives, tab surfaces and tooltips use the same palette. Health controls are one projected bar per visible living unit, use existing authoritative health/max values, ignore input and share textures. OS decorations and external Steam screens retain their platform presentation.
