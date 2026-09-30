# Selected free KayKit assets

These files are vendored for offline import and export. All four selected packs are CC0; license texts accompany each subset. `manifest.json` records official source paths, pinned Git commits or original archive members, retrieval date, and SHA-256 checksums. No paid tiers, third-party mirrors or runtime downloads are used.

- [Medieval Hexagon Pack 1.0](https://github.com/KayKit-Game-Assets/KayKit-Medieval-Hexagon-Pack-1.0): blue mine, barracks, windmill (labeled Farm), home, tower, grass, low slopes, one river bend variant, wooded hills, tree clusters, rocks, barrel and sack; shared atlas. Nine indexed plots form three staggered hex rows. Terrain and adjacency have no gameplay effects. Home/tower and decorations occupy no building slot.
- [Prototype Bits 1.0](https://github.com/KayKit-Game-Assets/KayKit-Prototype-Bits-1.0): previously vendored Dummy Base, Can A and Cube Prototype Small remain available for prototypes and non-character markers.
- [Adventurers Character Pack 1.0](https://github.com/KayKit-Game-Assets/KayKit-Character-Pack-Adventures-1.0/tree/672074b73ba276876a19e8816ecdc5241817ab47): Knight and Rogue rigged GLBs with embedded textures and animation clips; one-handed sword, two-handed crossbow and arrow with their buffers/shared atlas. The immutable commit, per-file official sources and hashes are recorded in the manifest. The character subset has its own CC0 license. Imported clips/hand bindings are validated in C#; graphical verification establishes the actual rendered poses.
- [Resource Bits 1.0 Free](https://kaylousberg.itch.io/resource-bits): Gold Bars, its buffer and texture, acquired from the official Free download (upload 13266824). This is a gold prop, not the paid coin/food extras. Archive checksum is recorded in the manifest.

Grass, slopes and river preserve authored origins at one common scale (1.5); river bends alternate 60°/240° to join along the side and exit the patch at both ends. Buildings and village props are normalized to fit their slot and use their original imported materials. Only graphical clients instantiate them. Headless clients and stripped dedicated-server exports operate entirely from numerical state.


Character binding uses `handslot.r`, with Knight/Swordsman and Knight/enemy using
`sword_1handed`, and Rogue/Crossbowman using `crossbow_2handed`. Optional props
embedded in the GLBs are hidden. Required clips: Idle, Walking_A, Running_A,
1H_Melee_Attack_Slice_Horizontal, 2H_Ranged_Shoot, Hit_A, Death_A. `UnitAssets`
checks rigs/clips/materials; `UnitView` samples them with a programmatic
AnimationTree, suppresses horizontal root motion and attaches the selected weapon.
Cosmetic arrows use the vendored arrow model. See gameplay documentation for
strike/release markers and authoritative timing.

Godot extracts `Knight_knight_texture.png` and `Rogue_rogue_texture.png` from the embedded GLB atlases. These are byte-identical to the pinned weapon atlases; the manifest records their source/hash and extraction relationship. Scene/texture `.import` metadata is retained alongside the vendored sources, matching the existing repository convention.
