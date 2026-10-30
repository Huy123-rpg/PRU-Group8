import re

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# Find guid to asset path mapping if possible, or read guid
docs = text.split('--- !u!')

images_info = []

for doc in docs:
    if 'UnityEngine.UI.Image' in doc:
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc)
        sprite_m = re.search(r'm_Sprite:\s*\{fileID:\s*([^,]+),\s*guid:\s*([^,]+)', doc)
        color_m = re.search(r'm_Color:\s*\{r:\s*([^,]+),\s*g:\s*([^,]+),\s*b:\s*([^,]+),\s*a:\s*([^}]+)\}', doc)
        raycast_m = re.search(r'm_RaycastTarget:\s*(\d+)', doc)
        go_id = go_m.group(1) if go_m else ""
        
        # find go name
        name_doc = ""
        for d in docs:
            if f"&{go_id}\n" in d or f"&{go_id}\r\n" in d:
                nm = re.search(r'm_Name:\s*(.+)', d)
                if nm:
                    name_doc = nm.group(1).strip()
                break
                
        # find rect
        x, y, w, h = 0, 0, 0, 0
        for d in docs:
            if 'RectTransform:' in d and f"m_GameObject: {{fileID: {go_id}}}" in d:
                pos_m = re.search(r'm_AnchoredPosition:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', d)
                size_m = re.search(r'm_SizeDelta:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', d)
                if pos_m: x, y = float(pos_m.group(1)), float(pos_m.group(2))
                if size_m: w, h = float(size_m.group(1)), float(size_m.group(2))
                break
                
        images_info.append({
            'go_id': go_id,
            'name': name_doc,
            'guid': sprite_m.group(2) if sprite_m else "",
            'fileID': sprite_m.group(1) if sprite_m else "",
            'raycast': raycast_m.group(1) if raycast_m else "",
            'x': x, 'y': y, 'w': w, 'h': h
        })

for info in images_info:
    print(f"[{info['go_id']}] '{info['name']}' pos=({info['x']:.1f}, {info['y']:.1f}) size=({info['w']:.1f}x{info['h']:.1f}) raycast={info['raycast']} guid={info['guid'][:8]}")
