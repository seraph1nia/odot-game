# Selected free KayKit assets

These files are vendored for offline import and export. All selected packs are CC0; license texts accompany each subset. `manifest.json` records official source paths, pinned Git commits or original archive members, retrieval date, and SHA-256 checksums. No paid tiers, third-party mirrors or runtime downloads are used.

- [Medieval Hexagon Pack 1.0](https://github.com/KayKit-Game-Assets/KayKit-Medieval-Hexagon-Pack-1.0): blue mine, barracks, windmill (labeled Farm), home, tower, grass, low slopes, one river bend variant, wooded hills, tree clusters, rocks, barrel and sack; shared atlas. Nine indexed plots form three staggered hex rows. Terrain and adjacency have no gameplay effects. Home/tower and decorations occupy no building slot.
- [Prototype Bits 1.0](https://github.com/KayKit-Game-Assets/KayKit-Prototype-Bits-1.0): previously vendored Dummy Base, Can A and Cube Prototype Small remain available for prototypes and non-character markers.
- [Adventurers Character Pack 1.0](https://github.com/KayKit-Game-Assets/KayKit-Character-Pack-Adventures-1.0/tree/672074b73ba276876a19e8816ecdc5241817ab47): Knight, Rogue, Barbarian and Mage rigged GLBs with embedded textures and animation clips; one-handed sword, two-handed axe, two-handed crossbow, staff and arrow with their buffers/shared atlas. The immutable commit, per-file official sources and hashes are recorded in the manifest. The character subset has its own CC0 license. Imported clips/hand bindings are validated in C#; graphical verification establishes the actual rendered poses.
- [Resource Bits 1.0 Free](https://kaylousberg.itch.io/resource-bits): Gold Bars and free Wood logs/planks/pallets, their buffers and texture, acquired from the official Free download (upload 13266824). This is a gold prop, not the paid coin/food extras. Archive checksum is recorded in the manifest.

Grass, slopes and river preserve authored origins at one common scale (1.5); river bends alternate 60°/240° to join along the side and exit the patch at both ends. Buildings and village props are normalized to fit their slot and use their original imported materials. Only graphical clients instantiate them. Headless clients and stripped dedicated-server exports operate entirely from numerical state.


Character binding uses `handslot.r`: Knight/Swordsman with sword, Barbarian/Berserker with axe, Rogue/Crossbowman with crossbow and Mage with staff. Skeleton Warrior/Minion/Rogue/Mage mirror those roles with their own weapons. The free Skeleton pack is pinned at official commit `15b62b9bad122f72926c10fb14d622c73819fa54`; its separate license and sources are in the manifest. Optional props
embedded in the GLBs are hidden. Required clips: Idle, Walking_A, Running_A,
1H_Melee_Attack_Slice_Horizontal, 2H_Ranged_Shoot, Hit_A, Death_A. `UnitAssets`
checks rigs/clips/materials; `UnitView` samples them with a programmatic
AnimationTree, suppresses horizontal root motion and attaches the selected weapon.
Cosmetic arrows use the vendored arrow model. See gameplay documentation for
strike/release markers and authoritative timing.

Godot extracts `Knight_knight_texture.png` and `Rogue_rogue_texture.png` from the embedded GLB atlases. These are byte-identical to the pinned weapon atlases; the manifest records their source/hash and extraction relationship. Scene/texture `.import` metadata is retained alongside the vendored sources, matching the existing repository convention.

Village additions use the same pinned Medieval free pack: lumbermill, archery
range, church (Arcanum), Blacksmith, second tower, tower base/catapult, grain,
scaffolding/stages, lumber/weapon racks/targets, mountains and bridge. Level two
adds these imported props without replacing their meshes/materials. Flags are
original procedural colored cloth markers. The windmill's authored
`building_windmill_top_fan_blue` mesh has a separate rotation pivot; only that
node rotates. Raised terraces and lowered riverbank remain presentation geometry;
the battle corridor is flat. Resource pile instance counts are capped at six per
resource. No paid coin, food, character or skeleton extras are included.

Additional extracted GLB atlases are recorded with their source and hash in the
manifest. Clips required for each role are common Idle/Walking_A/Running_A/Hit_A/
Death_A plus sword slice, two-handed axe chop, crossbow shot or Spellcast_Shoot.
Original synthesized PCM is separate from these assets; its parameters are
documented in gameplay.

Measured 30 fps hand-path markers for new role clips: two-handed axe chop
(1.6333s duration) descends fastest at frame 23 / 0.7667s; Spellcast_Shoot
(0.9333s duration) reaches forward extension at frame 8 / 0.2667s. Adventurer
and corresponding skeleton clips share these timelines. Checked-in binding
validation requires the marker to fit each actual imported clip; playback tests
map the marker to authoritative impacts and graphical combat samples the poses.


Progression uses the already bundled free models: Stonecutter reuses the blue
Blacksmith, Weaver the blue Lumbermill, and Market the neutral stage C. Explicit
world labels identify each role. Metal Mine reuses the blue Mine with a bundled
rock prop and its own label, keeping Gold Mine distinct. Their ground placement
uses the same imported foot/bounds mapping. No new files or downloads are needed.

Research Tower uses the already bundled blue tower A, with a structural tower base at level two. Stonecutter continues to reuse the original blacksmith asset. No asset or provenance record was added or altered.
