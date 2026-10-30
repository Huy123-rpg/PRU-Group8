import re

def parse_meta(meta_path):
    table = {}
    with open(meta_path, 'r', encoding='utf-8') as f:
        text = f.read()
    matches = re.findall(r'213:\s*(-?\d+)\s*\}\s*\n\s*second:\s*"?(.*?)"?\n', text)
    for fid, name in matches:
        table[fid] = name
    return table

atlas_table = parse_meta('Assets/Art/Environment/Atlas chữ kết quả Aurelia Academy.png.meta')
ui_table = parse_meta('Assets/Art/Environment/Bộ sprite giao diện Unity xanh vàng.png.meta')

with open('Assets/Scenes/Physics/Game2/19.1. Result_TL.unity', 'r', encoding='utf-8') as f:
    scene_text = f.read()

docs = scene_text.split('--- !u!')

for doc in docs:
    if 'UnityEngine.UI.Image' in doc:
        go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', doc)
        sp_m = re.search(r'm_Sprite:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([^,]+)', doc)
        if not go_m or not sp_m: continue
        go_id, fid, guid = go_m.group(1), sp_m.group(1), sp_m.group(2)
        
        # name
        name = "Unknown"
        for d in docs:
            if f"&{go_id}\n" in d or f"&{go_id}\r\n" in d:
                nm = re.search(r'm_Name:\s*(.+)', d)
                if nm: name = nm.group(1).strip()
                break
                
        # rect
        x, y, w, h = 0, 0, 0, 0
        for d in docs:
            if 'RectTransform:' in d and f"m_GameObject: {{fileID: {go_id}}}" in d:
                pos_m = re.search(r'm_AnchoredPosition:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', d)
                size_m = re.search(r'm_SizeDelta:\s*\{x:\s*([^,]+),\s*y:\s*([^}]+)\}', d)
                if pos_m: x, y = float(pos_m.group(1)), float(pos_m.group(2))
                if size_m: w, h = float(size_m.group(1)), float(size_m.group(2))
                break
                
        sp_name = atlas_table.get(fid, ui_table.get(fid, f"ID_{fid}"))
        print(f"[{go_id}] '{name}' pos=({x:.1f}, {y:.1f}) size=({w:.1f}x{h:.1f}) -> Sprite: '{sp_name}'")
