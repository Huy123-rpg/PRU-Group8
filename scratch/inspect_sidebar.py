import re

with open('Assets/Scenes/Physics/Game2/12_WrittenAnswer.unity', 'r', encoding='utf-8', errors='ignore') as f:
    c = f.read()

blocks = c.split('--- !u!')
gameobjects = {}
recttransforms = {}
images = {}
tmps = {}

for b in blocks:
    if not b.strip(): continue
    lines = b.split('\n')
    parts = lines[0].split('&')
    if len(parts) < 2: continue
    type_id = parts[0].strip()
    fid = parts[1].strip()
    
    if type_id == '1':
        m = re.search(r'm_Name:\s*([^\r\n]+)', b)
        gameobjects[fid] = m.group(1) if m else 'Unknown'
    elif type_id in ['4', '224']:
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
        pos_m = re.search(r'm_AnchoredPosition:\s*\{x:\s*([-\d\.]+),\s*y:\s*([-\d\.]+)\}', b)
        size_m = re.search(r'm_SizeDelta:\s*\{x:\s*([-\d\.]+),\s*y:\s*([-\d\.]+)\}', b)
        if go_m and pos_m:
            recttransforms[go_m.group(1)] = (pos_m.group(1), pos_m.group(2), size_m.group(1) if size_m else '?', size_m.group(2) if size_m else '?')
    elif type_id == '114':
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
        if 'm_Sprite:' in b and go_m:
            sprite_m = re.search(r'guid:\s*([a-f0-9]+)', b)
            images[go_m.group(1)] = sprite_m.group(1) if sprite_m else 'none'
        elif 'm_text:' in b and go_m:
            t_m = re.search(r'm_text:\s*([^\r\n]+)', b)
            tmps[go_m.group(1)] = t_m.group(1) if t_m else ''

# Look for objects around x < -600
for go_id, (px, py, sx, sy) in recttransforms.items():
    try:
        x = float(px)
        y = float(py)
        if x < -600 and y > -300 and y < 350:
            name = gameobjects.get(go_id, 'Unknown')
            spr = images.get(go_id, 'none')
            txt = tmps.get(go_id, '')
            print(f"Sidebar obj [{name}] fid={go_id}: pos=({x}, {y}), size=({sx}, {sy}), sprite={spr}, text='{txt}'")
    except:
        pass
