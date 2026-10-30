import re

def analyze_buttons(path):
    print(f'=== BUTTONS IN {path} ===')
    with open(path, 'r', encoding='utf-8', errors='ignore') as f:
        c = f.read()
    
    blocks = c.split('--- !u!')
    gameobjects = {}
    recttransforms = {}
    buttons = {}
    for b in blocks:
        if not b.strip(): continue
        lines = b.split('\n')
        header = lines[0]
        parts = header.split('&')
        if len(parts) < 2: continue
        type_id = parts[0].strip()
        fid = parts[1].strip()
        if type_id == '1': # GameObject
            m = re.search(r'm_Name:\s*([^\r\n]+)', b)
            name = m.group(1) if m else 'Unknown'
            gameobjects[fid] = name
        elif type_id == '224': # RectTransform
            go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
            pos_m = re.search(r'm_AnchoredPosition:\s*\{x:\s*([-\d\.]+),\s*y:\s*([-\d\.]+)\}', b)
            size_m = re.search(r'm_SizeDelta:\s*\{x:\s*([-\d\.]+),\s*y:\s*([-\d\.]+)\}', b)
            if go_m and pos_m and size_m:
                recttransforms[go_m.group(1)] = (pos_m.group(1), pos_m.group(2), size_m.group(1), size_m.group(2))
        elif type_id == '114': # Button / Image
            if 'm_Script: {fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff' in b: # Button
                go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
                if go_m:
                    buttons[go_m.group(1)] = True

    for go_id in buttons:
        name = gameobjects.get(go_id, 'Unknown')
        rt = recttransforms.get(go_id, ('?', '?', '?', '?'))
        print(f"  Button [{name}] (fid={go_id}): pos=({rt[0]}, {rt[1]}), size=({rt[2]}, {rt[3]})")

analyze_buttons('Assets/Scenes/Physics/Game2/19. Result_TN.unity')
analyze_buttons('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity')
