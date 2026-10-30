import re

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's inspect the exact IDs of the bottom buttons and images
docs = text.split('--- !u!')

info = {}
for doc in docs:
    lines = doc.strip().split('\n')
    if not lines or not lines[0]: continue
    m = re.match(r'(\d+)\s+&(\d+)', lines[0])
    if not m: continue
    tid, fid = m.groups()
    if tid == '1': # GO
        nm = re.search(r'm_Name:\s*(.+)', doc)
        name = nm.group(1).strip() if nm else ""
        info[fid] = {'type': tid, 'name': name, 'doc': doc}
    elif tid == '224': # RT
        gom = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc)
        gid = gom.group(1) if gom else ""
        info[fid] = {'type': tid, 'gid': gid, 'doc': doc}

for fid, d in info.items():
    if d['type'] == '1' and ('Button' in d['name'] or 'Image' in d['name']):
        print(f"GO &{fid}: {d['name']}")
