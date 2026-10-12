# Dreamlike woodland art direction

This pass extends the [existing atmosphere owner](atmosphere.md), using the
[already pinned forest import](forest-import.md). It is presentation only; native
appearance, scenic/gameplay readability and GPU cost are **not yet visually
qualified**. The current authorization is headless-only: no GUI game, screenshots,
private display, native Forward+ scenic run or graphical package journey was run.
Historical renderer and asset screenshots do not certify these new inputs.

## Settings and palette

Graphics has an **Art Style** group in the existing native scroll/focus/modal
transaction, not another settings store:

- **Forest theme**: Natural Woodland / Enchanted Forest / **Dreaming Forest**.
- **Magical effects**: Off / **Subtle** / Full.

Bold entries are first-run defaults. `visuals/forest_theme` (0–2) and
`visuals/magic` (0–2) persist in the existing `user://settings.cfg`. Invalid types,
missing or out-of-range values fall back independently. Quality presets leave
these choices intact, as do weather changes, weather-particle toggles and fog
choices. Theme/magic selection previews immediately; only the two art fields are
previewed over applied performance/weather preferences. Apply establishes the
new cancellation baseline; Cancel, Escape/close and discarded Reset restore it.
Reset remains a cancellable draft. No restart, city reconstruction, network/save
mutation or authority reset is needed. Renderer next-launch/CLI override ownership
is unchanged.

`ForestTheme` generates Godot's native **16×16×16 RGB8 ImageTexture3D** correction
LUT, not a 2D strip masquerading as a LUT or a fullscreen overlay. Red is X, green
Y, blue the slice index. The recipe is original game presentation code in
`src/Game/ForestTheme.cs`; there is no external LUT source/license, downloaded
asset, baked Blender correction or machine-specific path. Each owner caches at
most three LUTs (12 KiB each raw RGB, no mips) and releases them on exit. These are
CPU input byte counts, not GPU memory/performance measurements.

Natural uses an identity LUT and the existing warm sunlight/cool-neutral fill.
Enchanted adds low-strength teal shadow separation, slight green saturation and
warm highlights. Dreaming uses restrained blue-violet shadows, turquoise haze
and golden focal sunlight. Palette offsets compose with all five weather profiles;
their light energies, sun angles, precipitation, fog density and wind remain
meaningful and unchanged. Black and white remain endpoints; channel shifts are
bounded below 0.06 for checked gameplay category colors. Existing Filmic exposure,
slight desaturation and high-threshold restrained glow remain owned by atmosphere.
Native Canvas/UI text and resource/health UI are not graded; 3D category markers
receive only the restrained world correction, with no replacement of their
semantic colors. Real normal-zoom visual distinguishability remains unexecuted.

## One bounded environmental effect rig

`GameApplication` already owns one `AtmosphereController` for menu, village,
combat and city transitions. That controller now owns one `ForestMagic` child.
`VillageLandscape` supplies four grounded focal sites using its existing
actual-contact resolver; the table lists their source hex coordinates. Current
battlefield placement is bounded by [compact framing](view-renderer-worldtree.md#battlefield-framing),
and the same rig refreshes when those sites move on the viewed landscape.
`Tabletop` exposes only the viewed landscape to that owner. No second WorldEnvironment, weather clock, per-city effect controller,
procedural terrain system or simulation RNG was introduced.

| Source site | Native accent | Ground texture | Full footprint |
| --- | --- | --- | --- |
| Mushroom hollow (-3,4) | soft cyan | spore growth | 0.6 m |
| Bridge lantern (2,6) | warm amber | moss wash | 0.8 m |
| West clearing (-4,9) | muted mint | fairy ring | 1.2 m |
| Wayside runes (2,10) | lavender | rune circle | 1.4 m |

These use the four source-identical RGBA resources already in the pinned manifest;
no import revision, GLB, map pixels, source/material identity, factor, UV, sampler
or mipmap was changed. The mushroom/bridge ZeroEmission post-import correction
remains intact, including unused black-factor emission textures and positive
gills/runes/lanterns. Alpha remains coverage, never guessed emission.

- Four shadowless `OmniLight3D`s, energy 0.11 Subtle / 0.22 Full, range 2.4–3 m,
  no volumetric-light energy, fade beginning 45 m and completely out at 57 m.
  Natural warms/desaturates the owned accent colors; Dreaming retains the cool
  groves and warm lantern. No dynamic light is attached to particles.
- At most four native `Decal`s, **only when actual Forward+ has a RenderingDevice**.
  Compatibility and headless Dummy allocate no live decal nodes. Subtle shows
  only spores/runes. Source coverage is further attenuated by 0.3/0.55 modulation
  and 0.65 albedo mix, with no emission/ORM/normal substitution. Native downward
  projection uses PNG top toward −Z, a 0.12 m receiving depth, normal-angle fade
  and 40–52 m distance fade. Only existing opaque base-floor batches additionally
  opt into visual layer 20; original visible layer 1 is retained. Props, units,
  selectable markers, bridge and river/path overlays do not receive these marks.
  No terrain mesh or contact geometry was replaced. Component checks resolve
  real surface support at every projection corner and reject route overlap.
- Three shared `GpuParticles3D` behaviors: 24 warm fireflies at the hollow,
  16 pastel pollen points at the west clearing, two rare lavender wisps at the
  wayside. Full maximum is **42 particles**; Subtle is 12 + 8 and no wisps.
  Fixed seeds, local coordinates, 20 Hz simulation, 0.025–0.12 m/s drifting/orbit
  velocities, tiny 0.025–0.085 m soft native billboard quads and lifetime alpha
  curves avoid large transparent veils. No turbulence, collisions, per-particle
  nodes, gameplay timing or RNG. Visibility AABBs and a 52 m camera cutoff bound
  active work. This budget is a configured work bound, not measured GPU throughput.

Off hides lights, marks and **existing live particles immediately**, not merely
stopping new emission. Atmosphere Off also suppresses optional magic. Reduced
motion hides ambient magic particles and stops new emission; it does not freeze
the particle simulation. Reduced visual effects caps magic at Subtle. Low quality
also caps magic at Subtle, with no ambient magic particles and at most two accents.
Weather particle density stays independent. Full on Medium does not require
volumetric fog. Changing the viewed city resets/moves the same local particle rig, frees
old decals and binds the new grounded sites; repeated selection does not add
nodes. A null landscape suppresses the rig. Owner disposal releases its subtree.

## Composition and deliberate exclusions

Fixed canopy pockets, seed-varied batched vegetation and woodland/river/bridge/trail
plans remain the composition inputs. [Compact battlefield framing](view-renderer-worldtree.md#battlefield-framing)
owns their current visibility/coverage boundary; menu coverage remains separate.
Nine low plants/mushrooms are grouped around three selected clearings;
fixed-coordinate decorative yaw variation breaks repetition without consuming
random simulation state. The central village, all building plots and the combat
approach stay open. No unrelated asset-library refresh occurred.

Built-in toon/rim and light projectors were evaluated but not applied in this
initial pass. Existing foliage has a channel-preserving lit wind shader, while
mushroom caps/gills intentionally differ in emission. Replacing these with toon
or rim risks flattening authored PBR detail or suggesting emissive cap bodies;
without authorized native views there is no demonstrated aesthetic gain to
justify it. Additional patterned/projector lights likewise add a capability/cost
boundary and directional-shadow interaction without verified visual benefit.
The selected faint fairy/rune floor patterns already provide localized rhythm.
This is an intentional fidelity/cohesion choice, not a claim that projector or
selective shader rendering was tested. Neither improvement requires new exports;
further asset exports are currently unnecessary.

Protected: authoritative terrain/hexes, floor/river/bridge/trail topology and
heights, collision/picking/navigation/combat paths, fixed plots, camera controls,
rigs/animations/sockets, Arch numerical state, gameplay input, multiplayer,
economy and saves. The additions above, decorative transforms and the receiver
visibility bit affect presentation only.

## Verification and limitations

The [headless verification plan](forest-art-verification-plan.md) names allowed
pipeline checks and prohibits obsolete GUI verification. Tier A extends existing
preference policy tests; Tier B extends the existing Settings signals/persistence
and source/export asset resource proof. No new process, probe journey or test
supervisor was added. The asset proof checks CPU LUT upload image pixels, texture
metadata/native Environment binding, actual native light/decal/particle properties,
imported terrain supports/receiver opt-in, comfort/Off/Low and transition/disposal.
Dummy does not expose 3D GPU readback: the failed readback attempt is retained,
not replaced by a claim of rendered texture/color accuracy.

Implementation-phase evidence (before pipeline rebase/review):

| Check | Coverage / evidence |
| --- | --- |
| Targeted graphics C# | 42 passed, 2 Windows-only skipped; `logs/20261011-111315-ba1a8d35`, 1.20 s |
| Headless Settings | theme/magic draft preview, Apply/Cancel/Reset/close, persistence and invalid saved values; `logs/20261011-111318-9d64997d`, 0.62 s owned |
| Broader fast C# | 457 core + 647 runner passed, 2 Windows-only skipped, excludes Deep; `logs/20261011-111850-4ea8f439`, 19.61 s |
| Source headless assets | original imported material/mip/geometry negative checks plus forest native resource/lifecycle proof; `logs/20261011-112247-297a39a2`, 7.24 s owned |

A first component run exposed a real capability defect: headless Dummy reports
requested method Forward+ without a RenderingDevice. Live decal admission now
requires the actual device as well; that failure is retained under
`logs/20261011-111329-73aaf9ed`. The later impossible Dummy texture readback failure
is retained under `logs/20261011-111608-3fbfdf04`. No deadline/assertion suppression,
prerequisite installation, GUI retry or driver change was used. Subsequent changed
inputs and packages belong to pipeline validation, not these earlier receipts.

**Unexecuted:** all requested menu/overview/forest/grove/river/combat/morning/evening/
weather screenshots, actual Forward+ appearance, native selectable/UI readability,
rendering/compositor behavior and FPS/VRAM qualification. No screenshot hashes,
property getters, Dummy method label, old native campaign or separate asset-showcase
picture substitute for those. Headless package/native platform checks establish
resource/payload behavior, not visual excellence. No release is part of this pass.
