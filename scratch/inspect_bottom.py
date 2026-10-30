import re

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'r', encoding='utf-8') as f:
    text = f.read()

def parse_meta(meta_path):
    table = {}
    with open(meta_path, 'r', encoding='utf-8') as mf:
        t = mf.read()
    for fid, name in re.findall(r'213:\s*(-?\d+)\s*\}\s*\n\s*second:\s*"?(.*?)"?\n', t):
        table[fid] = name
    return table

atlas = parse_meta('Assets/Art/Environment/Atlas chữ kết quả Aurelia Academy.png.meta')
ui = parse_meta('Assets/Art/Environment/Bộ sprite giao diện Unity xanh vàng.png.meta')

docs = text.split('--- !u!')
for doc in docs:
    if 'RectTransform:' in doc:
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc)
        pos_m = re.search(r'm_AnchoredPosition:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', doc)
        size_m = re.search(r'm_SizeDelta:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', doc)
        if not go_m or not pos_m: continue
        go_id = go_m.group(1)
        y = float(pos_m.group(2))
        x = float(pos_m.group(1))
        w = float(size_m.group(1)) if size_m else 0
        h = float(size_m.group(2)) if size_m else 0
        
        # Filter for bottom elements: y < -350
        if y < -350:
            # find go name & sprite
            go_name = "Unknown"
            sp_name = ""
            for d in docs:
                if f"&{go_id}\n" in d or f"&{go_id}\r\n" in d:
                    nm = re.search(r'm_Name:\s*(.+)', d)
                    if nm: go_name = nm.group(1).strip()
                if f"m_GameObject: {{fileID: {go_id}}}" in d and 'UnityEngine.UI.Image' in d:
                    sp_m = re.search(r'm_Sprite:\s*\{fileID:\s*(-?\d+)', d)
                    if sp_m:
                        fid = sp_m.group(1)
                        sp_name = atlas.get(fid, ui.get(fid, fid))
            print(f"[{go_id}] '{go_name}' pos=({x:.1f}, {y:.1f}) size=({w:.1f}x{h:.1f}) sprite='{sp_name}'")
