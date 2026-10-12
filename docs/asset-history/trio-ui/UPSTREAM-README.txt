Trio UI Kit — FREE Sample
=========================

WHAT'S IN HERE
  cozy/   30 files — the complete Cozy theme: panels, buttons (4 states),
                     HP/MP/XP/stamina bars, slider, checkbox, radio, toggle,
                     arrows, close button.
  icons/  40 files — a cross-section of the 110-icon set: weapons, consumables,
                     loot, status effects and system icons.

  70 SVG files. Enough to build and ship a complete prototype UI.

LICENCE
  Free for unlimited personal and commercial use. No royalties, no attribution
  required. Do not resell or redistribute the files themselves.

USING THESE
  Godot   Drop the .svg files in. Godot rasterises SVG at import; each file has
          a scale setting in the Import dock, so you can raise resolution
          without new exports.
  Unity   SVG needs the Vector Graphics package. Without it, export PNGs at the
          size you need — every file is plain SVG, so any editor or CLI works.

9-SLICING THE PANELS
  panel_plain / panel_small / panel_notch ....... border 20 px
  panel_header .................................. border 49 / 20 / 20 / 20
     (the title bar occupies the top 49 px; a smaller top border stretches it)

  Panels are 300x200 (panel_small is 180x110). The fill is a vertical gradient,
  so on very tall panels the gradient flattens in the middle — fine at dialog
  sizes, worth knowing at full-screen sizes.

THE FULL KIT
  3 themes (Cozy / Dark / Pastel), 200 vector assets, 110 icons,
  SVG + transparent PNG at 1x and 2x.

  The three themes are the same geometry in different colours — every widget
  matches across all three in size, stroke weight and corner radius. Build
  against Cozy here, and switching to Dark later is a folder swap.

  https://moonpunchstudio.itch.io/trio-ui-kit   ($9.95)

PRO EDITION
  Everything in the full kit, plus tooling for pipelines:
    - retheme.py: recolour the whole kit with any of 8 colour presets
      in one command — no editor work.
    - 4 sprite atlases with JSON metadata, kit.json manifest, 3x PNG exports.
  The art itself is the same 200 assets; Pro adds the tools, not more art.

  https://steamloc.gumroad.com/l/triokit-pro   ($39)

--
Moonpunch Studio
