#!/usr/bin/env python3
"""One scoped immutable AI-texture import. Reads Git objects, never Blender sources.

Dry-run by default. --apply copies only fourteen declared exports and their exact
embedded PNGs, removes only superseded derived maps, updates the manifest pin.
Then update AssetCatalog.Revision and use the existing locked prepare/test tools.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import struct
import subprocess

BASELINE = '133c7cdeb368cada408d1c95c9414e08488371a9'
PREVIOUS = '2d88d355393f145aedfc19fa198960dac741c6fb'
REVISION = '60ce86f04c02419d9c09d30c2e25931345bf2bf3'
IDENTITY = '62ba705d9dd5e26d4ed6fd0b0c7e64337fc064629148420737f55a5e9057c8a7'
ROOT = Path(__file__).resolve().parents[2]


def sha(data):
    return hashlib.sha256(data).hexdigest()


def import_assets(repo, apply=False):
    def git(*args):
        return subprocess.check_output(['git', '-C', str(repo), *args])

    # A stale/private feature working tree is irrelevant: read only immutable
    # objects proven reachable from the local origin/main tracking ref. Verify
    # that ref against the forge before invoking this intentionally scoped tool.
    git('merge-base', '--is-ancestor', REVISION, 'origin/main')
    handoff = json.loads(git('show', REVISION + ':docs/ai-textures/handoff.json'))
    assert handoff['baselineRevision'] == BASELINE
    assert handoff['contentIdentity'] == IDENTITY == sha(json.dumps(handoff['files'], sort_keys=True).encode())
    selected = {row['sourcePath']: row for row in handoff['files']}
    assert len(selected) == 14
    folder = ROOT / 'src/Game/Assets/Authored'
    path = folder / 'manifest.json'
    manifest = json.loads(path.read_text())
    assert manifest['revision'] in (BASELINE, PREVIOUS, REVISION), 'Unrecognized pin; preserve and review newer work'
    assert len(selected.keys() - {f['sourcePath'] for f in manifest['files']}) == 0
    payloads, derived, files, changed = {}, [], [], []
    # Validate the COMPLETE selected source inventory before any mutation.
    for entry in manifest['files']:
        relative = entry['path']
        assert entry['sourcePath'] == 'exports/' + relative and '..' not in Path(relative).parts
        current = (folder / relative).read_bytes()
        assert sha(current) == entry['sha256'] and len(current) == entry['bytes'], relative
        data = git('show', REVISION + ':' + entry['sourcePath'])
        if entry['sourcePath'] not in selected:
            assert sha(data) == entry['sha256'], 'Unrelated export differs: ' + relative
            files.append(entry)
            derived.extend(d for d in manifest['derived'] if d['parent'] == relative)
            continue
        row = selected[entry['sourcePath']]
        assert sha(data) == row['sha256'] and len(data) == row['bytes'], relative
        if current != data:
            changed.append(relative)
        length = struct.unpack_from('<I', data, 12)[0]
        doc = json.loads(data[20:20 + length])
        binary = data[28 + length:]
        assert len(doc['materials']) == row['materials']
        assert [a['name'] for a in doc.get('animations', [])] == row['animationNames']
        assert len(doc['images']) == len(row['embeddedImages'])
        seen = set()
        for index, (image, expected) in enumerate(zip(doc['images'], row['embeddedImages'])):
            name = image['name']
            assert re.fullmatch(r'[A-Za-z0-9_.-]+', name), 'Unsafe image name'
            view = doc['bufferViews'][image['bufferView']]
            start = view.get('byteOffset', 0)
            png = binary[start:start + view['byteLength']]
            assert sha(png) == expected['sha256'] and len(png) == expected['bytes']
            assert png[:8] == b'\x89PNG\r\n\x1a\n' and list(struct.unpack_from('>II', png, 16)) == expected['dimensions']
            destination = relative[:-4] + '_' + name + ('_' + str(index) if name in seen else '') + '.png'
            seen.add(name)
            payloads[destination] = png
            derived.append(dict(path=destination, parent=relative, imageName=name,
                                sha256=sha(png), bytes=len(png), imageIndex=index))
        payloads[relative] = data
        files.append(dict(path=relative, sourcePath=entry['sourcePath'], sha256=sha(data), bytes=len(data),
                          embeddedImages=len(doc['images']), materials=len(doc['materials']), animationNames=row['animationNames']))
    for entry in manifest['derived'] + manifest['textures']:
        data = (folder / entry['path']).read_bytes()
        assert sha(data) == entry['sha256'] and len(data) == entry['bytes'], entry['path']
    for entry in manifest['textures']:
        assert sha(git('show', REVISION + ':' + entry['sourcePath'])) == entry['sha256'], 'Unrelated standalone texture differs'
    obsolete = {d['path'] for d in manifest['derived']} - {d['path'] for d in derived}
    print(json.dumps(dict(previous=manifest['revision'], revision=REVISION, handoff=IDENTITY,
                          changed=changed, removedMaps=sorted(obsolete), derivedCount=len(derived), apply=apply), indent=2))
    if apply:
        for relative, data in payloads.items():
            if not (folder / relative).exists() or (folder / relative).read_bytes() != data:
                (folder / relative).write_bytes(data)
        for relative in obsolete:
            (folder / relative).unlink()
            (folder / (relative + '.import')).unlink(missing_ok=True)
        manifest.update(revision=REVISION, files=files, derived=derived)
        path.write_text(json.dumps(manifest, indent=2) + '\n')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('asset_repo', type=Path)
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    import_assets(args.asset_repo, args.apply)
