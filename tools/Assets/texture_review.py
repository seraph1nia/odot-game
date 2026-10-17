#!/usr/bin/env python3
"""Package retained owned-game PNGs; no game launches, pixel generation or server.

Requires the matching successful --checkpoint textures evidence directories.
Camera/graphics/viewport mismatches fail before writing the portable gallery.
"""
import argparse
import hashlib
import html
import json
from pathlib import Path
import shutil
import subprocess

VIEWS = {
    'settlement': 'Actual fitted solo overview',
    'archery': 'Paid Archery Range · permitted 3× close view',
    'menu-texture-mixed': 'Mixed neighbors · live menu landscape',
    'menu-texture-forest': 'Forest · five-family floor selection',
    'menu-texture-path': 'Connected path curves and quiet endcaps',
    'menu-texture-river': 'Connected river bends · authored banks',
    'settlement-river': 'Actual district divider, bridge and river',
    'settlement-river-motion': 'Later frame · same live river camera',
}


def sha(data):
    return hashlib.sha256(data).hexdigest()


def package(before, after, before_head, after_head, output):
    before_head, after_head = (subprocess.check_output(['git', 'rev-parse', head + '^{commit}']).decode().strip()
                               for head in (before_head, after_head))
    records = []
    inputs = {s: json.loads((p / 'texture-inputs.json').read_text()) for s, p in [('before', before), ('after', after)]}
    for stage, head in [('before', before_head), ('after', after_head)]:
        data = subprocess.check_output(['git', 'show', head + ':src/Game/Assets/Authored/manifest.json'])
        assert sha(data) == inputs[stage]['ManifestSha256'], 'Capture manifest differs from recorded source head'
    for name in VIEWS:
        pair = {s: json.loads((p / (name + '.json')).read_text()) for s, p in [('before', before), ('after', after)]}
        a, b = pair['before'], pair['after']
        for key in ('Camera', 'Width', 'Height', 'Screen'):
            assert a[key] == b[key], (name, key, 'unmatched actual camera/frame')
        assert a['Landscape']['View'] == b['Landscape']['View'], (name, 'actual ground-camera footprint differs')
        assert a['Graphics'] == b['Graphics'], (name, 'unmatched lighting/color/renderer settings')
        for stage, p in [('before', before), ('after', after)]:
            frame = pair[stage]
            assert frame['Graphics']['Gpu'] == 'AMD Radeon RX 6800 (RADV NAVI21)'
            assert frame['Graphics']['Driver'] == 'vulkan' and frame['Graphics']['RenderingDevice']
            assert frame['Graphics']['DeviceType'] == 'DiscreteGpu' and frame['Graphics']['Display'] == 'X11'
            assert frame['ProcessId'] > 0 and frame['NativeWindow'] != 0 and frame['WindowMode'] == 'Windowed'
            data = (p / (name + '.png')).read_bytes()
            assert data[:8] == b'\x89PNG\r\n\x1a\n'
            records.append(dict(view=name, stage=stage, pngSha256=sha(data), input=inputs[stage],
                                sourceHead=before_head if stage == 'before' else after_head,
                                observation={k: frame[k] for k in ('Graphics', 'Camera', 'Width', 'Height', 'Screen',
                                             'ProcessId', 'NativeWindow', 'WindowMode', 'RenderCosts', 'ProbeCosts')},
                                groundCameraFootprint=frame['Landscape']['View']))
    output.mkdir(parents=True, exist_ok=True)
    for stage, source in [('before', before), ('after', after)]:
        (output / stage).mkdir(exist_ok=True)
        for name in VIEWS:
            shutil.copyfile(source / (name + '.png'), output / stage / (name + '.png'))
        subprocess.run(['magick', str(source / 'archery.png'), '-crop', '270x240+420+125', '+repage',
                        str(output / stage / 'archery-crop.png')], check=True)
    subprocess.run(['magick', str(output / 'before/settlement.png'), str(output / 'after/settlement.png'),
                    '+append', str(output / 'settlement-pair.png')], check=True)
    receipt = dict(inputs=inputs, sourceHeads=dict(before=before_head, after=after_head), frames=records,
                   captureScope='Owned game viewports only; no desktop captures. Ambient poses/particles use real elapsed time, not pixel-equality targets.',
                   crop=dict(source='archery', xywh=[420, 125, 270, 240]),
                   encodedArtifacts={p.relative_to(output).as_posix(): sha(p.read_bytes()) for p in output.rglob('*.png')})
    (output / 'evidence.json').write_text(json.dumps(receipt, indent=2) + '\n')
    cards = []
    for name, title in VIEWS.items():
        figures = ''.join('<figure><figcaption>' + stage.upper() + ' · assets ' + inputs[stage]['AssetRevision'][:12]
                          + '</figcaption><a href="' + stage + '/' + name + '.png"><img loading="lazy" src="'
                          + stage + '/' + name + '.png" alt="' + html.escape(stage + ' ' + title)
                          + '"></a></figure>' for stage in ('before', 'after'))
        cards.append('<article id="' + name + '"><h2>' + html.escape(title) + '</h2><div class="pair">' + figures + '</div></article>')
    (output / 'review.html').write_text('''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Odot · actual-game AI textures</title><style>
:root{color-scheme:dark;--bg:#262724;--panel:#333530;--ink:#eee7d7;--muted:#c0b7a6;--line:#606353;--accent:#bdcb83}*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--ink);font:16px/1.6 system-ui,sans-serif}main{max-width:1500px;margin:auto;padding:32px 20px}h1,h2{font-family:Georgia,serif;font-weight:normal;line-height:1.2}h1{font-size:clamp(30px,5vw,56px)}h2{font-size:24px}p{max-width:90ch;color:var(--muted)}a{color:var(--accent)}article{min-width:0;border:1px solid var(--line);background:var(--panel);padding:16px;margin:24px 0;border-radius:8px}.pair{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:12px}figure{min-width:0;margin:0}img{display:block;width:100%;height:auto}figcaption{font-size:12px;color:var(--muted);overflow-wrap:anywhere}code{overflow-wrap:anywhere}.status{border-left:4px solid var(--accent);padding:12px 18px;background:var(--panel)}a:focus-visible{outline:3px solid var(--accent);outline-offset:3px}@media(max-width:700px){.pair{grid-template-columns:minmax(0,1fr)}}</style><main><p>ODOT / ACTUAL GAME CAMERA / RX 6800</p><h1>More painted detail.<br>A busier, still visibly tiled world.</h1>
<div class="status"><strong>Implemented and locally GPU-tested POC, not production art certification.</strong> Near/medium terrain is visibly richer; the fitted overview exposes more repeated hex borders. Archery's subtle grain/cloth detail largely disappears at overview. Read the tradeoffs before artistic acceptance.</div>
<p>These are real Godot game viewports, not the upstream Blender studio. Both revisions use the same executable witness, 1100×820 X11 window, Forward+ / Vulkan, fixed fair-weather light and cosmetic time 12, High preset, Woodland grading, exposure 1, 4× MSAA and identical camera/framing. Menu photo views hide only its central controls; no extra assets or terrain fixtures are installed. Solo uses the unchanged horizontal board and normal paid Archery construction. Native ambient animation/particles remain live, so these are not pose- or pixel-identical frames.</p>
<p><a href="README.md">Results, coverage, preview and rollback</a> · <a href="evidence.json">Exact source/asset revisions, PNG/manifest/DLL hashes, camera/graphics and observed costs</a> · <a href="import-receipt.json">All imported GLB hashes</a> · <a href="settlement-pair.png">Full-resolution side-by-side overview PNG (BEFORE left / AFTER right)</a></p>
<p>Click a frame for its native 1100×820 pixels. The overview is the actual normal game-view scale; close views use permitted 3× zoom. Before images are retained, never re-rendered with the new pin. The two river frames demonstrate successive visible playback, not a claim that all pixel differences are water motion; the engine test separately checks real animated targets and fixed seams.</p>
<article><h2>Archery · native-pixel crop, not invented detail</h2><div class="pair"><figure><figcaption>BEFORE · same 270×240 crop</figcaption><img src="before/archery-crop.png" alt="Original Archery native crop"></figure><figure><figcaption>AFTER · same 270×240 crop</figcaption><img src="after/archery-crop.png" alt="Textured Archery native crop"></figure></div></article>
''' + ''.join(cards) + '<footer>Static offline gallery; project asset-review olive/parchment palette and serif hierarchy. No review server or browser inspection claimed.</footer></main></html>')
    print(output / 'review.html')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    for arg in ('before', 'after', 'output'):
        parser.add_argument('--' + arg, type=Path, required=True)
    parser.add_argument('--before-head', required=True)
    parser.add_argument('--after-head', required=True)
    args = parser.parse_args()
    package(args.before, args.after, args.before_head, args.after_head, args.output)
